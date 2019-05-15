#region License
// =================================================================================================
// Copyright 2018 DataArt, Inc.
// -------------------------------------------------------------------------------------------------
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this work except in compliance with the License.
// You may obtain a copy of the License in the LICENSE file, or at:
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// =================================================================================================
#endregion

#if NET452
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using AutoMapper;
using DataArt.Atlas.CallContext.Correlation;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Core.Application;
using DataArt.Atlas.Core.Application.Http;
using DataArt.Atlas.Core.Application.Logging;
using DataArt.Atlas.Core.Application.Messaging;
using DataArt.Atlas.Core.Application.Owin;
using DataArt.Atlas.Core.Application.Swagger;
using DataArt.Atlas.Core.Application.WebApi;
using DataArt.Atlas.Core.HealthCheck;
using DataArt.Atlas.Core.VersionInfo;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Hosting.HealthCheck;
using DataArt.Atlas.Infrastructure;
using DataArt.Atlas.Infrastructure.AutoMapper;
using DataArt.Atlas.Infrastructure.Startup;
using DataArt.Atlas.Logging;
using DataArt.Atlas.Messaging;
using DataArt.Atlas.Messaging.Autofac;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.Messaging.Intercept;
using DataArt.Atlas.WebCommunication.Autofac;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using Serilog;

[assembly: ExplicitTypeUsage(typeof(Microsoft.Owin.Host.HttpListener.OwinHttpListener))]

namespace DataArt.Atlas.Core.Shell
{
    public abstract class ServiceShell : IApplication
    {
        private readonly TimeSpan busStopInterval = TimeSpan.FromSeconds(50);

        // disposables
        private ApplicationSettings settings;
        private IContainer applicationContainer;
        private IDisposable webApp;

        private ShellInitializationParameters initializationParameters;

        private Guid mainThreadCorrelationId;

        public HostingSettings HostingSettings => Settings.Hosting;

#region HealthState
        public Action<string, HealthState, string> ReportHealthStateAction { get; set; }

        public Action<string, HealthState, TimeSpan, string> ReportRecurrentHealthStateAction { get; set; }
#endregion HealthState

        public Func<string> GetCodePackageVersionFunction { get; set; }

        public Func<string, string> GetDataPackageVersionFunction { get; set; }

        public Func<string, string> GetServiceResourcePathFunction { get; set; }

        public virtual ApplicationSettings Settings => settings;

        protected internal string FabricApplicationName { get; set; }

        protected abstract string ServiceKey { get; }

        protected HttpConfiguration HttpConfiguration { get; private set; }

        internal static ServiceShell CreateInstance<T>(ShellInitializationParameters initializationParameters)
            where T : ServiceShell, new()
        {
            var instance = new T
            {
                initializationParameters = initializationParameters
            };

            return instance;
        }

        private IContainer ApplicationContainer
        {
            get
            {
                if (applicationContainer == null)
                {
                    var builder = initializationParameters.GetContainerBuilder();
                    ConfigureAutoMapper(builder);

                    ConfigureContainer(builder);
                    ConfigureContainerInternal(builder);

                    builder.RegisterInstance(Settings).As<ApplicationSettings>().SingleInstance();
                    builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                    builder.RegisterApiControllers(TypeLocator.GetEntryPointAssembly());
                    builder.RegisterInstance(initializationParameters.GetLoggerConfiguration(Settings.Logging)
                            .Configure(Settings.Logging, new ServiceNameEnricher())).As<ILogger>().SingleInstance();
                    builder.RegisterModule<BaseWebCommunicationAutofacModule>();

                    RegisterServiceBuses(builder);

                    applicationContainer = builder.Build();
                }

                return applicationContainer;
            }
        }

        public async Task StartAsync(string applicationUrlOverride, CancellationToken cancellationToken)
        {
            try
            {
                // CorrelationId should be set in a very begining
                CorrelationContext.SetCorrelationId();
                mainThreadCorrelationId = CorrelationContext.CorrelationId;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var settingsReader = new ApplicationSettingsReader(initializationParameters.GetContainerBuilder().Build()))
                {
                    settings = settingsReader.Read();
                    settings.Hosting.Url = applicationUrlOverride ?? settings.Hosting.Url;
                }

                webApp = StartWebApplication(Settings.Hosting.Url, ApplicationContainer);

                await StartServiceBusesAsync(cancellationToken);

                ConfigureService(ApplicationContainer);

                await StartStartablesAsync(cancellationToken);

                Log.Information("{ServiceName} {version} is started on {url} under {user}", GetType().Name, GetVersion(), Settings.Hosting.Url, WindowsIdentity.GetCurrent().Name);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
                {
                    Log.Error("Host requested cancelation while starting {ServiceName}", GetType().Name);
                    return;
                }

                Log.Fatal(ex, "Unhandled exception while starting {ServiceName}", GetType().Name);
                throw;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                CorrelationContext.SetCorrelationId(mainThreadCorrelationId.ToString());

                Log.Information("Stopping {ServiceName} {version}", GetType().Name, GetVersion());

                await StopServiceBusesAsync();
                await StopStartablesAsync();

                StopApplication();

                applicationContainer?.GracefulDispose();
                applicationContainer = null;
                webApp?.Dispose();
                webApp = null;
                settings = null;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception while stopping {ServiceName}", GetType().Name);
                throw;
            }
        }

        protected virtual IDisposable StartWebApplication(string url, IContainer container)
        {
            return WebApp.Start(url, builder => SelfHostConfiguration(builder, container));
        }

        protected virtual void StopApplication()
        {
        }

        protected virtual void ConfigureContainer(ContainerBuilder builder)
        {
        }

        protected virtual void ConfigureAutoMapper(IMapperConfigurationExpression configuration)
        {
        }

        protected virtual void ConfigureService(IContainer container)
        {
        }

        protected virtual void ConfigureSwagger(IContainer container)
        {
            SwaggerConfig.Configure(HttpConfiguration, GetType(), Settings.Hosting.AuthenticationRequired);
        }

        protected virtual void SelfHostConfiguration(IAppBuilder appBuilder, IContainer container)
        {
            appBuilder.Use<UnhandledExceptionLogMiddleware>();

            // todo: clear up CORS policy
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.Use<CorrelationIdHandlerMiddleware>();

            if (Settings.Hosting.AuthenticationRequired)
            {
                JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

                appBuilder.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
                {
                    BackchannelHttpHandler = new CorrelatedHttpClientHandler(),
                    Authority = Settings.Hosting.AuthorityUrl,
                    ValidationMode = ValidationMode.Local,
                    RequiredScopes = new[] { Settings.Hosting.ScopeName },
                    DelayLoadMetadata = true
                });
            }

            HttpConfiguration = WebApiConfig.Configure(appBuilder, container, Settings.Hosting);

            if (Settings.Hosting.AuthenticationRequired)
            {
                Claim claim = null;

                if (!string.IsNullOrEmpty(Settings.Hosting.ClientId) && initializationParameters.ClaimParameter != null)
                {
                    claim = initializationParameters.ClaimParameter(Settings.Hosting.ClientId);
                }

                HttpConfiguration.Filters.Add(new AuthorizeCustomAttribute(claim));
            }

            if (Settings.Hosting.SwaggerEnabled)
            {
                ConfigureSwagger(container);
                appBuilder.Run(owinContext =>
                {
                    return Task.Factory.StartNew(() => owinContext.Response.Redirect("swagger/ui/index"));
                });
            }
        }

        protected string GetServiceResourcePath(string resourceName)
        {
            return GetServiceResourcePathFunction?.Invoke(resourceName);
        }

        private static string GetVersion()
        {
            return TypeLocator.GetEntryPointAssembly().GetName().Version.ToString();
        }

        private async Task StartStartablesAsync(CancellationToken cancellationToken)
        {
            var awaitList = ApplicationContainer
                .Resolve<IEnumerable<Startable>>()
                .Select(startable => startable.StartAsync(cancellationToken)).ToList();

            if (awaitList.Any())
            {
                await Task.WhenAll(awaitList);
            }
        }

        private async Task StopStartablesAsync()
        {
            var awaitList = ApplicationContainer
                .Resolve<IEnumerable<Startable>>()
                .Select(startable => startable.StopAsync()).ToList();

            if (awaitList.Any())
            {
                await Task.WhenAll(awaitList);
            }
        }

        private Task StartServiceBusesAsync(CancellationToken cancellationToken)
        {
            return ExecuteServiceBusesActionAsync(sb => sb.StartAsync(cancellationToken));
        }

        private Task StopServiceBusesAsync()
        {
            return ExecuteServiceBusesActionAsync(sb => sb.StopAsync(busStopInterval));
        }

        private async Task ExecuteServiceBusesActionAsync(Func<IBaseServiceBus, Task> action)
        {
            var awaitList = new List<Task>();

            var sb = ApplicationContainer?.ResolveOptional<IServiceBus>();

            if (sb != null)
            {
                awaitList.Add(action(sb));
            }

            var interopSb = ApplicationContainer?.ResolveOptional<IInteropServiceBus>();

            if (interopSb != null)
            {
                awaitList.Add(action(interopSb));
            }

            if (awaitList.Any())
            {
                await Task.WhenAll(awaitList);
            }
        }

        private void RegisterServiceBuses(ContainerBuilder builder)
        {
            if (initializationParameters.ServiceBusParameters.Any())
            {
                var module = new ServiceBusAutofacModule(initializationParameters.ServiceBusParameters);
                builder.RegisterModule(module);
                ServiceBusInterceptorsProvider.Add(new ServiceBusInterceptor());
            }

            if (initializationParameters.ConsumerRegistrator != null)
            {
                var entryAssembly = TypeLocator.GetEntryPointAssembly();
                var consumers = entryAssembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IConsumer)));
                var module = new ConsumerAutofacModule(consumers, initializationParameters.ConsumerRegistrator);
                builder.RegisterModule(module);
            }
        }

        private void ConfigureContainerInternal(ContainerBuilder builder)
        {
            builder.Register(context => new HealthReporter(ReportHealthStateAction, ReportRecurrentHealthStateAction)).As<IHealthReporter>().InstancePerDependency();
            builder.Register(context => new VersionInfoResolver(GetCodePackageVersionFunction, GetDataPackageVersionFunction)).As<IVersionInfoResolver>().InstancePerDependency();
        }

        private void ConfigureAutoMapper(ContainerBuilder builder)
        {
            // static
            Mapper.Reset();
            Mapper.Initialize(ConfigureAutoMapper);
            Mapper.Configuration.CompileMappings();

            // instance
            builder.Register(c => (MapperConfiguration)Mapper.Configuration).AsSelf().SingleInstance();
            builder.Register(c => c.Resolve<MapperConfiguration>().CreateMapper(c.Resolve)).As<IMapper>().InstancePerLifetimeScope();

            // linq
            var projectionConfiguration = new ProjectionMapperConfiguration(cfg =>
            {
                cfg.AllowNullDestinationValues = false;
                ConfigureAutoMapper(cfg);
            });
            projectionConfiguration.CompileMappings();
            builder.Register(c => projectionConfiguration).As<IProjectionConfigurationProvider>().SingleInstance();
        }
    }
}
#endif
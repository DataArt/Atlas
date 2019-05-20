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
#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DataArt.Atlas.CallContext.Correlation;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Core.Application;
using DataArt.Atlas.Core.Application.Http;
using DataArt.Atlas.Core.Application.Http.OData;
using DataArt.Atlas.Core.Application.Messaging;
using DataArt.Atlas.Core.Application.Owin;
using DataArt.Atlas.Core.Application.Swagger;
using DataArt.Atlas.Core.Application.WebApi;
using DataArt.Atlas.Core.HealthCheck;
using DataArt.Atlas.Core.VersionInfo;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Infrastructure.Container;
using DataArt.Atlas.Infrastructure.Logging;
using DataArt.Atlas.Infrastructure.Startup;
using DataArt.Atlas.Messaging;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.Messaging.Container;
using DataArt.Atlas.Messaging.Intercept;
using DataArt.Atlas.WebCommunication.Container;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DataArt.Atlas.Core.Shell
{
    public abstract class Startup
    {
        private readonly TimeSpan busStopInterval = TimeSpan.FromSeconds(50);
        private readonly CancellationTokenSource source = new CancellationTokenSource();
        private ILogger logger;
        private Guid mainThreadCorrelationId;

        protected IServiceProvider Container { get; private set; }

        protected IConfiguration Configuration { get; }

        protected IApplication Application { get; }

        protected IHostingEnvironment HostingEnvironment { get; }

        protected ApplicationSettings ApplicationSettings { get; private set; }

        protected abstract string ServiceKey { get; }

        protected virtual void ConfigureAutoMapper(IMapperConfigurationExpression configurationExpression)
        {
        }

        protected virtual void ConfigureContainer(IServiceCollection services)
        {
        }

        protected virtual IServiceProvider ConfigureServiceProvider(IServiceCollection services)
        {
            // please do not forget to populate custom container with existing services
            return services.BuildServiceProvider(validateScopes: true);
        }

        protected virtual void StopApplication()
        {
        }

        protected virtual void StartApplication()
        {
        }

        protected Startup(IHostingEnvironment env, IConfiguration configuration, IApplication application)
        {
            HostingEnvironment = env;
            Configuration = configuration;
            Application = application;
        }

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // CorrelationId should be set in a very begining
            CorrelationContext.SetCorrelationId();
            mainThreadCorrelationId = CorrelationContext.CorrelationId;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ApplicationSettings = new ApplicationSettings();
            Configuration.GetSection(typeof(ApplicationSettings).Name).Bind(ApplicationSettings);

            // add custom configuration section
            services.Configure<ApplicationSettings>(opt => Configuration.GetSection(typeof(ApplicationSettings).Name));
            services.AddSingleton(ApplicationSettings);

            // add AutoMapper
            services.ConfigureAutoMapper(ConfigureAutoMapper);

            // add healthState reporter
            services.AddTransient<IHealthReporter>(provider =>
                new HealthReporter(Application.ReportHealthStateAction, Application.ReportRecurrentHealthStateAction));

            // add version resolver
            services.AddTransient<IVersionInfoResolver>(provider =>
                new VersionInfoResolver(Application.GetCodePackageVersionFunction, Application.GetDataPackageVersionFunction));

            // add services to container
            ConfigureContainer(services);

            services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true; // false by default

                if (ApplicationSettings.Hosting.AuthenticationRequired)
                {
                    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                    services
                        .AddAuthorization(opt =>
                        {
                            opt.AddPolicy("policy", builder =>
                                {
                                    builder.RequireScope(ApplicationSettings.Hosting.ScopeName);
                                });
                        })
                        .AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                        .AddIdentityServerAuthentication(authOptions =>
                            {
                                authOptions.JwtBackChannelHandler = new CorrelatedHttpClientHandler();
                                authOptions.Authority = ApplicationSettings.Hosting.AuthorityUrl;
                                authOptions.SupportedTokens = SupportedTokens.Jwt;
                            });

                    Claim claim = null;

                    if (!string.IsNullOrEmpty(ApplicationSettings.Hosting.ClientId) && StartupParameters.SecurityConfiguration != null)
                    {
                        claim = StartupParameters.SecurityConfiguration(ApplicationSettings.Hosting.ClientId);
                    }

                    options.Filters.Add(new AuthorizeCustomFilter(claim));
                }

                if (ApplicationSettings.Hosting.HttpsRequired)
                {
                    options.Filters.Add(new RequireHttpsFilter());
                    options.Filters.Add(new HSTSAttribute());
                }

                options.Filters.Add(new ApiExceptionHandlingFilter(
                    ApplicationSettings.Hosting.ShowErrorDetails,
                    ApplicationSettings.Hosting.IsGateway));
            }).AddControllersAsServices();

            services.AddOData();

            // Workaround: https://github.com/OData/WebApi/issues/1177
            services.AddMvcCore(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
                }
            });

            services.AddTransient<IConfigureOptions<MvcJsonOptions>, JsonOptionsSetup>();
            services.AddSingleton(JsonOptionsSetup.Settings);
            services.AddMemoryCache();

            if (ApplicationSettings.Hosting.SwaggerEnabled)
            {
                services.ConfigureSwagger(GetType(), ApplicationSettings.Hosting.AuthenticationRequired);

                // todo: add swagger redirect
            }

            RegisterServiceBuses(services);
            services.RegisterModule<WebCommunicationModule>();

            Container = ConfigureServiceProvider(services);

            return Container;
        }

        public virtual void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            AtlasLogging.LoggerFactory = loggerFactory;
            logger = AtlasLogging.CreateLogger<Startup>();

            app.UseMiddleware<CorrelationIdHandlerMiddleware>();
            app.UseMiddleware<UnhandledExceptionLogMiddleware>();
            app.UseMiddleware<ApiAccessLoggingMiddleware>();
            app.UseMiddleware<HttpNotFoundMiddleware>();

            if (ApplicationSettings.Hosting.SwaggerEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", GetType().Name);
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "api/{controller}/{id?}");

                if (ApplicationSettings.Hosting.AuthenticationRequired)
                {
                    object constraints = ApplicationSettings.Hosting.SwaggerEnabled
                        ? new { url = "^((?!swagger).)*$" }
                        : null;
                    routes.MapRoute(
                        name: "NotFound",
                        template: "{*url}",
                        defaults: new { controller = "Errors", action = "Handle404" },
                        constraints: constraints);
                }

                routes.Filter().OrderBy(QueryOptionSetting.Allowed).MaxTop(EnableQueryWithInlineCountAttribute.TopParamMaxValue).Count();
                routes.EnableDependencyInjection();
            });

            appLifetime.ApplicationStarted.Register(Start);
            appLifetime.ApplicationStopped.Register(Stop);
        }

        private void Start()
        {
            try
            {
                logger.LogInformation("Starting up {applicationName}", ServiceKey);
                StartServiceBuses();
                StartStartables();
                StartApplication();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Unhandled exception while starting {ServiceName}", GetType().Name);
                throw;
            }
        }

        private void Stop()
        {
            try
            {
                CorrelationContext.SetCorrelationId(mainThreadCorrelationId.ToString());
                logger.LogInformation("Stopping {ServiceName}", GetType().Name);
                StopServiceBuses();
                StopStartables();
                StopApplication();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Unhandled exception while stopping {ServiceName}", GetType().Name);
                throw;
            }
        }

        private void StartServiceBuses()
        {
            ExecuteServiceBusesAction(sb => sb.StartAsync(source.Token));
        }

        private void StopServiceBuses()
        {
            ExecuteServiceBusesAction(sb => sb.StopAsync(busStopInterval));
        }

        private void ExecuteServiceBusesAction(Func<IBaseServiceBus, Task> action)
        {
            var awaitList = new List<Task>();

            var sb = Container?.GetService<IServiceBus>();

            if (sb != null)
            {
                awaitList.Add(action(sb));
            }

            var interopSb = Container?.GetService<IInteropServiceBus>();

            if (interopSb != null)
            {
                awaitList.Add(action(interopSb));
            }

            if (awaitList.Any())
            {
                Task.WaitAll(awaitList.ToArray());
            }
        }

        private void RegisterServiceBuses(IServiceCollection services)
        {
            if (StartupParameters.BusInitiators.Any())
            {
                var module = new ServiceBusModule(StartupParameters.BusInitiators.Select(x =>
                    new Tuple<Type, IServiceBusInitiator>(x.ConsumerBusType, x.Initiator)));
                services.RegisterModule(module);
                ServiceBusInterceptorsProvider.Add(new ServiceBusInterceptor());
            }

            var registrator = StartupParameters.ConsumerRegistrator;
            if (registrator != null)
            {
                var entryAssembly = TypeLocator.GetEntryPointAssembly();
                var consumers = entryAssembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IConsumer)));
                var module = new ConsumerModule(consumers, registrator);
                services.RegisterModule(module);
            }
        }

        private void StopStartables()
        {
            var awaitList = Container.GetServices<Startable>().Select(startable => startable.StopAsync()).ToArray();

            if (awaitList.Any())
            {
                Task.WaitAll(awaitList);
            }
        }

        private void StartStartables()
        {
            var awaitList = Container.GetServices<Startable>().Select(startable => startable.StartAsync(source.Token)).ToArray();

            if (awaitList.Any())
            {
                Task.WaitAll(awaitList);
            }
        }
    }
}
#endif
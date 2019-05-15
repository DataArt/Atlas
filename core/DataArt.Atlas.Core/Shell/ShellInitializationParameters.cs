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
using System.Security.Claims;
using Autofac;
using DataArt.Atlas.Configuration;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Messaging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Module = Autofac.Module;

namespace DataArt.Atlas.Core.Shell
{
    internal class ShellInitializationParameters
    {
        public List<Tuple<Type, Type>> ConfigurationClientParameters { get; }

        public List<Type> DiscoveryClientParameters { get; }

        public List<Tuple<Type, IServiceBusInitiator>> ServiceBusParameters { get; }

        public IConsumerRegistrator ConsumerRegistrator { get; set; }

        public List<Type> AutofacModules { get; }

        public List<Action<LoggerSinkConfiguration>> LoggerSinkParameters { get; }

        public List<Action<LoggingSettings, LoggerSinkConfiguration>> LoggerSinkProviders { get; }

        public List<ILogEventEnricher> LoggerEnricherParameters { get; }

        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public Func<string, Claim> ClaimParameter { get; set; }

        public ShellInitializationParameters()
        {
            ConfigurationClientParameters = new List<Tuple<Type, Type>>();
            DiscoveryClientParameters = new List<Type>();
            ServiceBusParameters = new List<Tuple<Type, IServiceBusInitiator>>();
            AutofacModules = new List<Type>();
            LoggerSinkParameters = new List<Action<LoggerSinkConfiguration>>();
            JsonSerializerSettings = new JsonSerializerSettings();
            LoggerSinkProviders = new List<Action<LoggingSettings, LoggerSinkConfiguration>>();
            LoggerEnricherParameters = new List<ILogEventEnricher>();
        }

        public ContainerBuilder GetContainerBuilder()
        {
            var builder = new ContainerBuilder();
            foreach (var configurationClientParameter in ConfigurationClientParameters)
            {
                builder.RegisterType(configurationClientParameter.Item1).AsImplementedInterfaces().InstancePerDependency();
                builder.Register(ctx =>
                    {
                        var client = ctx.Resolve<IConfigurationClient>();
                        var clientType = client.GetType();

                        var getMethod = clientType.GetMethod("GetSettings");
                        var genericGet = getMethod.MakeGenericMethod(configurationClientParameter.Item2);

                        return Convert.ChangeType(genericGet.Invoke(client, null), configurationClientParameter.Item2);
                    }).As(configurationClientParameter.Item2).As<ApplicationSettings>().SingleInstance();
            }

            foreach (var discoveryClientParameter in DiscoveryClientParameters)
            {
                builder.RegisterType(discoveryClientParameter).AsImplementedInterfaces().InstancePerDependency();
            }

            foreach (var autofacModule in AutofacModules)
            {
                var module = (Module)Activator.CreateInstance(autofacModule);
                builder.RegisterModule(module);
            }

            builder.RegisterInstance(JsonSerializerSettings).AsSelf().SingleInstance();

            return builder;
        }

        public LoggerConfiguration GetLoggerConfiguration(LoggingSettings settings)
        {
            var configuration = new LoggerConfiguration();
            foreach (var sinkParameter in LoggerSinkParameters)
            {
                sinkParameter(configuration.WriteTo);
            }

            if (settings != null)
            {
                foreach (var loggerSinkProvider in LoggerSinkProviders)
                {
                    loggerSinkProvider(settings, configuration.WriteTo);
                }
            }

            foreach (var enricher in LoggerEnricherParameters)
            {
                configuration.Enrich.With(enricher);
            }

            return configuration;
        }
    }
}
#endif
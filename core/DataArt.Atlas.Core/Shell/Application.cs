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
using System.Security.Claims;
using Autofac;
using DataArt.Atlas.Configuration;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Messaging;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.ServiceDiscovery;
using Newtonsoft.Json;
using Serilog.Configuration;
using Serilog.Core;

namespace DataArt.Atlas.Core.Shell
{
    public class Application<TService>
        where TService : ServiceShell, new()
    {
        private Type Runner { get; set; }

        private ShellInitializationParameters initializationParameters;

        private ShellInitializationParameters ShellInitializationParameters => initializationParameters ??
                                                                               (initializationParameters =
                                                                                   new ShellInitializationParameters());

        public Application<TService> WithRunner<TRunner>()
            where TRunner : IApplicationRunner, new()
        {
            if (Runner != null)
            {
                throw new InvalidProgramException($"Application runner already specified {Runner.Name}");
            }

            Runner = typeof(TRunner);
            return this;
        }

        public Application<TService> WithConfigurationClient<TConfigurator, TSettings>()
            where TConfigurator : IConfigurationClient
            where TSettings : ApplicationSettings, new()
        {
            ShellInitializationParameters.ConfigurationClientParameters.Add(new Tuple<Type, Type>(typeof(TConfigurator), typeof(TSettings)));
            return this;
        }

        public Application<TService> WithConfigurationClient<TConfigurator>()
            where TConfigurator : IConfigurationClient
        {
            return WithConfigurationClient<TConfigurator, ApplicationSettings>();
        }

        public Application<TService> WithServiceDiscovery<TServiceDiscovery>()
            where TServiceDiscovery : IServiceDiscovery
        {
            ShellInitializationParameters.DiscoveryClientParameters.Add(typeof(TServiceDiscovery));
            return this;
        }

        public Application<TService> WithServiceBus<TI, TT>()
            where TI : IServiceBusInitiator, new()
            where TT : IConsumerBusType
        {
            ShellInitializationParameters.ServiceBusParameters.Add(
                new Tuple<Type, IServiceBusInitiator>(
                    typeof(TT),
                    (IServiceBusInitiator)Activator.CreateInstance(typeof(TI))));
            return this;
        }

        public Application<TService> WithConsumerRegistration<T>()
            where T : IConsumerRegistrator, new()
        {
            if (ShellInitializationParameters.ConsumerRegistrator != null)
            {
                throw new InvalidProgramException("Consumer registrator already specified");
            }

            ShellInitializationParameters.ConsumerRegistrator =
                (IConsumerRegistrator)Activator.CreateInstance(typeof(T));
            return this;
        }

        public Application<TService> WithAutofacModule<TAutofacModule>()
            where TAutofacModule : Module, new()
        {
            ShellInitializationParameters.AutofacModules.Add(typeof(TAutofacModule));
            return this;
        }

        public Application<TService> UseJsonMediaTypeFormatter(JsonSerializerSettings serializerSettings)
        {
            ShellInitializationParameters.JsonSerializerSettings = serializerSettings;
            return this;
        }

        public Application<TService> UseSerilogSinkConfiguration(Action<LoggerSinkConfiguration> sinkConfiguration)
        {
            ShellInitializationParameters.LoggerSinkParameters.Add(sinkConfiguration);
            return this;
        }

        public Application<TService> UseSerilogSinkConfiguration(Action<LoggingSettings, LoggerSinkConfiguration> sinkConfiguration)
        {
            ShellInitializationParameters.LoggerSinkProviders.Add(sinkConfiguration);
            return this;
        }

        public Application<TService> WithSerilogEnrichers(params ILogEventEnricher[] enrichers)
        {
            ShellInitializationParameters.LoggerEnricherParameters.AddRange(enrichers);
            return this;
        }

        public Application<TService> UseSecurityConfiguration(Func<string, Claim> securityConfiguration)
        {
            if (ShellInitializationParameters.ClaimParameter != null)
            {
                throw new InvalidProgramException("Security configuration already specified");
            }

            ShellInitializationParameters.ClaimParameter = securityConfiguration;
            return this;
        }

        public void Run()
        {
            var runner = (IApplicationRunner)Activator.CreateInstance(Runner);
            var instance =
                ServiceShell.CreateInstance<TService>(ShellInitializationParameters);
            runner.Run(instance);
        }
    }
}
#endif
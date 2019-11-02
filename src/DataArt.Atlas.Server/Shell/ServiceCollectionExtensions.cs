//--------------------------------------------------------------------------------------------------
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
//--------------------------------------------------------------------------------------------------
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using DataArt.Atlas.Messaging;
using DataArt.Atlas.Messaging.Consume;

namespace DataArt.Atlas.Core.Shell
{
    // native net core DI container cannot resolve Func<TService>
    // when TService is registered
    // todo
    public static class ServiceCollectionExtensions
    {
        public static void RegisterConfiguration<TConfiguration>(this IServiceCollection services, IConfiguration configuration)
            where TConfiguration : class, new()
        {
            var settings = Activator.CreateInstance<TConfiguration>();
            configuration.GetSection(typeof(TConfiguration).Name).Bind(settings);

            services.Configure<TConfiguration>(opt => configuration.GetSection(typeof(TConfiguration).Name));
            services.AddSingleton(settings);
        }

        public static void RegisterBusInitiator<TServiceBusInitiator, TConsumerBusType>(this IServiceCollection services)
            where TServiceBusInitiator : IServiceBusInitiator, new()
            where TConsumerBusType : IConsumerBusType
        {
            var initiator = new BusInitiator
            {
                ConsumerBusType = typeof(TConsumerBusType),
                Initiator = (IServiceBusInitiator)Activator.CreateInstance(typeof(TServiceBusInitiator))
            };

            StartupParameters.BusInitiators.Add(initiator);
        }

        public static void UseConsumerRegistration<T>(this IServiceCollection services)
            where T : IConsumerRegistrator, new()
        {
            if (StartupParameters.ConsumerRegistrator != null)
            {
                throw new InvalidProgramException("Consumer registrator already specified");
            }

            StartupParameters.ConsumerRegistrator = (IConsumerRegistrator)Activator.CreateInstance(typeof(T));
        }

        public static void UseSecurityConfiguration(Func<string, Claim> securityConfiguration)
        {
            if (StartupParameters.SecurityConfiguration != null)
            {
                throw new InvalidProgramException("Security configuration already specified");
            }

            StartupParameters.SecurityConfiguration = securityConfiguration;
        }
    }
}

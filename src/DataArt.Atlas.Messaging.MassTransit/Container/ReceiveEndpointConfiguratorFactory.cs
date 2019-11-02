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
#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using DataArt.Atlas.Infrastructure.Logging;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.Messaging.MassTransit.Consume;
using DataArt.Atlas.Messaging.MassTransit.Intercept;
using MassTransit;
using MassTransit.Internals.Extensions;
using MassTransit.Registration;
using MassTransit.Scoping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataArt.Atlas.Messaging.MassTransit.Container
{
    public static class ReceiveEndpointConfiguratorFactory
    {
        private static readonly ILogger Logger = AtlasLogging.CreateLogger("ReceiveEndpointConfiguratorFactory");

        public static Action<IReceiveEndpointConfigurator> Create<TBusType, TRoutingType>(
            IServiceProvider provider)
            where TBusType : class, IConsumerBusType
            where TRoutingType : class, IConsumerRountingType
        {
            return IsAnyConsumer<TBusType, TRoutingType>(provider)
                ? c =>
                {
                    c.UseInterceptor();
                    c.LoadConsumersFrom<TBusType, TRoutingType>(provider);
                }
                : (Action<IReceiveEndpointConfigurator>)null;
        }

        private static bool IsAnyConsumer<TBusType, TRoutingType>(IServiceProvider provider)
            where TBusType : class, IConsumerBusType
            where TRoutingType : class, IConsumerRountingType
        {
            return provider.FindConsumerTypes<TBusType, TRoutingType>().Any();
        }

        private static void LoadConsumersFrom<TBusType, TRoutingType>(
            this IReceiveEndpointConfigurator configurator,
            IServiceProvider provider)
            where TBusType : class, IConsumerBusType
            where TRoutingType : class, IConsumerRountingType
        {
            try
            {
                foreach (var consumerType in provider.FindConsumerTypes<TBusType, TRoutingType>())
                {
                    var scopeProvider = provider.GetRequiredService<IConsumerScopeProvider>();
                    ConsumerConfiguratorCache.Configure(consumerType, configurator, scopeProvider);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to register receive endpoint: {configurator.InputAddress}");
            }
        }

        private static IEnumerable<Type> FindConsumerTypes<TBusType, TRoutingType>(this IServiceProvider provider)
            where TBusType : class, IConsumerBusType
            where TRoutingType : class, IConsumerRountingType
        {
            var services = provider.GetServices<IConsumerMarker>()
                .Where(service => service.GetType().HasInterface<global::MassTransit.IConsumer>());

            foreach (var service in services)
            {
                var serviceType = service.GetType().GetInterfaces().FirstOrDefault(intr =>
                    typeof(IMassTransitConsumer<,,>) == intr.GetGenericTypeDefinition());

                if (serviceType == null)
                {
                    continue;
                }

                var genericArgs = serviceType.GetGenericArguments();
                if (genericArgs.Contains(typeof(TBusType)) && genericArgs.Contains(typeof(TRoutingType)))
                {
                    yield return serviceType;
                }
            }
        }
    }
}
#endif
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
#if NET452
using System;
using Autofac;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.Messaging.MassTransit;
using DataArt.Atlas.Messaging.MassTransit.Autofac;
using MassTransit;

namespace DataArt.Atlas.Messaging.RabbitMq
{
    public class RabbitMqServiceBusInitiator : IServiceBusInitiator
    {
        public IServiceBus CreateDefaultInstance(ServiceBusSettings settings, ILifetimeScope rootScope)
        {
            return CreateInstance<DefaultBusType>(settings, rootScope);
        }

        public IInteropServiceBus CreateInteropInstance(ServiceBusSettings settings, ILifetimeScope rootScope)
        {
            return CreateInstance<InteropBusType>(settings, rootScope);
        }

        internal static IBusControl Create(
            IComponentContext context,
            ServiceBusSettings settings,
            Action<IReceiveEndpointConfigurator> receiveEndpointConfigurator,
            Action<IReceiveEndpointConfigurator> fanoutReceiveEndpointConfigurator)
        {
            var isRabbitTransport = !string.IsNullOrWhiteSpace(settings.RabbitUrl);

            if (string.IsNullOrWhiteSpace(settings.QueueName) && receiveEndpointConfigurator != null)
            {
                throw new InvalidOperationException("Invalid service bus configuration settings: queue name should be specified");
            }

            if (isRabbitTransport)
            {
                return RabbitMqTransportBusFactory.Create(settings, receiveEndpointConfigurator, fanoutReceiveEndpointConfigurator);
            }

            throw new InvalidOperationException("Invalid service bus configuration settings");
        }

        private static ServiceBus CreateInstance<T>(ServiceBusSettings settings, ILifetimeScope rootScope)
            where T : class, IConsumerBusType
        {
            var bus = Create(
                rootScope,
                settings,
                ReceiveEndpointConfiguratorFactory.Create<T, DefaultRoutingType>(rootScope),
                ReceiveEndpointConfiguratorFactory.Create<T, FanoutRoutingType>(rootScope));

            var name = typeof(T) == typeof(DefaultBusType) ? "Service bus" : "Interop service bus";
            return new ServiceBus(bus, name);
        }
    }
}
#endif
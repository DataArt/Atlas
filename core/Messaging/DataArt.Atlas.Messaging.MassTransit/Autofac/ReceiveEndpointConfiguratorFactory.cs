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
using System.Linq;
using Autofac;
using Autofac.Core;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.Messaging.MassTransit.Intercept;
using MassTransit;
using MassTransit.AutofacIntegration;
using MassTransit.AutofacIntegration.ScopeProviders;
using MassTransit.Internals.Extensions;
using MassTransit.Registration;
using Serilog;

namespace DataArt.Atlas.Messaging.MassTransit.Autofac
{
    public static class ReceiveEndpointConfiguratorFactory
    {
        public static Action<IReceiveEndpointConfigurator> Create<TBusType, TRoutingType>(ILifetimeScope rootScope)
            where TBusType : class, IConsumerBusType
            where TRoutingType : class, IConsumerRountingType
        {
            return IsAnyConsumer<TBusType, TRoutingType>(rootScope) ? c =>
            {
                c.UseInterceptor();
                c.LoadConsumersFrom<TBusType, TRoutingType>(rootScope);
            }
            : (Action<IReceiveEndpointConfigurator>)null;
        }

        private static bool IsAnyConsumer<TBusType, TRoutingType>(ILifetimeScope rootScope)
            where TBusType : class, IConsumerBusType
            where TRoutingType : class, IConsumerRountingType
        {
            return rootScope.FindConsumerTypes<TBusType, TRoutingType>().Any();
        }

        private static void LoadConsumersFrom<TBusType, TRoutingType>(this IReceiveEndpointConfigurator configurator, ILifetimeScope rootScope)
            where TBusType : class, IConsumerBusType
            where TRoutingType : class, IConsumerRountingType
        {
            try
            {
                foreach (var consumerType in rootScope.FindConsumerTypes<TBusType, TRoutingType>())
                {
                    var scopeProvider = new AutofacConsumerScopeProvider(
                        new SingleLifetimeScopeProvider(rootScope),
                        AutofacExtensions.ServiceBusMessageScopeTag,
                        (builder, context) => builder.ConfigureScope(context));

                    ConsumerConfiguratorCache.Configure(consumerType, configurator, scopeProvider);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to register receive endpoint: {configurator.InputAddress}");
            }
        }

        private static IEnumerable<Type> FindConsumerTypes<TBusType, TRoutingType>(this IComponentContext scope)
            where TBusType : class, IConsumerBusType
            where TRoutingType : class, IConsumerRountingType
        {
            return scope.ComponentRegistry.Registrations
                .SelectMany(r => r.Services.OfType<IServiceWithType>(), (r, s) => new { r, s })
                .Where(rs => rs.s.ServiceType.HasInterface<global::MassTransit.IConsumer>())
                .Where(rs => (Type)rs.r.Metadata[nameof(IConsumerBusType)] == typeof(TBusType))
                .Where(rs => (Type)rs.r.Metadata[nameof(IConsumerRountingType)] == typeof(TRoutingType))
                .Select(rs => rs.s.ServiceType);
        }
    }
}
#endif
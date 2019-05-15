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
using System;
using System.Threading.Tasks;
using DataArt.Atlas.Messaging.Consume;

#if NET452
using Serilog;
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
#endif

namespace DataArt.Atlas.Messaging.MassTransit.Consume
{
    internal class MassTransitConsumer<TBusType, TRoutingType, TMessage> : IMassTransitConsumer<TBusType, TRoutingType, TMessage>
        where TMessage : class, new()
        where TBusType : class, IConsumerBusType
        where TRoutingType : class, IConsumerRountingType
    {
#if NETSTANDARD2_0
        private readonly ILogger logger =
            AtlasLogging.CreateLogger<MassTransitConsumer<TBusType, TRoutingType, TMessage>>();
#endif

        private readonly IConsumer<TBusType, TRoutingType, TMessage> consumer;

        public MassTransitConsumer(IConsumer<TBusType, TRoutingType, TMessage> consumer)
        {
            this.consumer = consumer;
        }

        public async Task Consume(global::MassTransit.ConsumeContext<TMessage> context)
        {
            try
            {
                await consumer.Consume(context.Message);
#if NET452
                Log.Debug("Service bus message was consumed: {@Message}", context.Message);
#endif

#if NETSTANDARD2_0
                logger.LogDebug("Service bus message was consumed: {@Message}", context.Message);
#endif
            }
            catch (Exception exception)
            {
#if NET452
                Log.Error(exception, "Service bus message consuming failed {@Message}", context.Message);
#endif

#if NETSTANDARD2_0
                logger.LogError(exception, "Service bus message consuming failed {@Message}", context.Message);
#endif
                throw;
            }
        }
    }
}

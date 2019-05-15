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
using DataArt.Atlas.Infrastructure.Container;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Messaging.Container
{
    public sealed class ConsumerModule : Module
    {
        private readonly IEnumerable<Type> consumers;
        private readonly IConsumerRegistrator consumerRegistrator;

        public ConsumerModule(IEnumerable<Type> consumers, IConsumerRegistrator consumerRegistrator)
        {
            this.consumers = consumers;
            this.consumerRegistrator = consumerRegistrator;
        }

        protected override void Load(IServiceCollection services)
        {
            consumerRegistrator.LoadDependencies(services);

            foreach (var consumer in consumers)
            {
                var method = consumerRegistrator.GetType().GetMethod("RegisterConsumer");
                var generic = method.MakeGenericMethod(consumer);
                generic.Invoke(consumerRegistrator, new object[] { services });
            }
        }
    }
}
#endif
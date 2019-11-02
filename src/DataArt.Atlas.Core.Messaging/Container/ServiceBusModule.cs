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
using DataArt.Atlas.Core.Container;
#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Messaging.Consume;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Messaging.Container
{
    public sealed class ServiceBusModule : Module
    {
        private readonly IEnumerable<Tuple<Type, IServiceBusInitiator>> initiators;

        public ServiceBusModule(IEnumerable<Tuple<Type, IServiceBusInitiator>> initiators)
        {
            this.initiators = initiators;
        }

        protected override void Configure(IServiceCollection services)
        {
            foreach (var initiator in initiators)
            {
                if (initiator.Item1 == typeof(DefaultBusType))
                {
                    services.AddSingleton(provider =>
                        initiator.Item2.CreateDefaultInstance(
                        provider.GetService<ApplicationSettings>().ServiceBus, provider));
                }
                else
                {
                    services.AddSingleton(provider =>
                        initiator.Item2.CreateInteropInstance(
                            provider.GetService<ApplicationSettings>().InteropServiceBus, provider));
                }
            }
        }
    }
}
#endif
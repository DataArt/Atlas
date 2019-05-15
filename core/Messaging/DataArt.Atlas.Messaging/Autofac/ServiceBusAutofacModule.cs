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
using Autofac;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Messaging.Consume;

namespace DataArt.Atlas.Messaging.Autofac
{
    public sealed class ServiceBusAutofacModule : Module
    {
        private readonly IEnumerable<Tuple<Type, IServiceBusInitiator>> initiators;

        public ServiceBusAutofacModule(IEnumerable<Tuple<Type, IServiceBusInitiator>> initiators)
        {
            this.initiators = initiators;
        }

        protected override void Load(ContainerBuilder builder)
        {
            foreach (var initiator in initiators)
            {
                if (initiator.Item1 == typeof(DefaultBusType))
                {
                    builder.Register(context =>
                        {
                            var rootScope = context.Resolve<ILifetimeScope>();
                            return initiator.Item2.CreateDefaultInstance(rootScope.Resolve<ApplicationSettings>().ServiceBus, rootScope);
                        })
                        .As<IServiceBus>()
                        .SingleInstance();
                }
                else
                {
                    builder.Register(context =>
                        {
                            var rootScope = context.Resolve<ILifetimeScope>();
                            return initiator.Item2.CreateInteropInstance(rootScope.Resolve<ApplicationSettings>().InteropServiceBus, rootScope);
                        })
                        .As<IInteropServiceBus>()
                        .SingleInstance();
                }
            }
        }
    }
}
#endif
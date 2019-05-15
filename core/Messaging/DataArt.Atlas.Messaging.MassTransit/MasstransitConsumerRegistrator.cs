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
using Autofac;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.Messaging.MassTransit.Autofac;

namespace DataArt.Atlas.Messaging.MassTransit
{
    public sealed class MasstransitConsumerRegistrator : IConsumerRegistrator
    {
        public void RegisterConsumer<TConsumer>(ContainerBuilder builder)
            where TConsumer : class, IConsumer
        {
            builder.RegisterConsumer<TConsumer>();
        }
    }
}
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Messaging.MassTransit.Container;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using IConsumer = DataArt.Atlas.Messaging.Consume.IConsumer;

namespace DataArt.Atlas.Messaging.MassTransit
{
    public sealed class MasstransitConsumerRegistrator : IConsumerRegistrator
    {
        public void RegisterConsumer<TConsumer>(IServiceCollection services)
            where TConsumer : class, IConsumer
        {
            services.RegisterConsumer<TConsumer>();
        }

        public void LoadDependencies(IServiceCollection services)
        {
            services.AddMassTransit();
        }
    }
}
#endif
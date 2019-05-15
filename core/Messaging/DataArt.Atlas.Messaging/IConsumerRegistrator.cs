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
using DataArt.Atlas.Messaging.Consume;

#if NET452
using Autofac;
#endif

#if NETSTANDARD2_0
using Microsoft.Extensions.DependencyInjection;
#endif

namespace DataArt.Atlas.Messaging
{
    public interface IConsumerRegistrator
    {
#if NET452
        void RegisterConsumer<TConsumer>(ContainerBuilder builder)
#endif

#if NETSTANDARD2_0
        void RegisterConsumer<TConsumer>(IServiceCollection services)
#endif
            where TConsumer : class, IConsumer;

#if NETSTANDARD2_0
        void LoadDependencies(IServiceCollection services);
#endif
    }
}

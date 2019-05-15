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
using System.Linq;
using System.Reflection;
using DataArt.Atlas.Infrastructure.Extensions;
using DataArt.Atlas.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Infrastructure.Container
{
    public abstract class SdkModule : Module
    {
        protected override void Load(IServiceCollection services)
        {
            var assembly = GetType().Assembly;
            var assemblyTypes = assembly.GetTypes();

            // gets all sdk configs form the assembly where Autofac module is defined
            var sdkConfigTypes = assemblyTypes.Where(x => typeof(ISdkConfig).IsAssignableFrom(x));
            Register(sdkConfigTypes, services);

            // now retrive all interfaces inherited from ISdkClient
            var interfaces = assemblyTypes
                .Where(x => x.IsInterface)
                .Where(x => typeof(ISdkClient).IsAssignableFrom(x))
                .ToList();

            // and register all the classes implementing client's interfaces
            var classTypes = assemblyTypes
                .Where(x => x.IsClass)
                .Where(x => interfaces.Any(y => y.IsAssignableFrom(x)));
            Register(classTypes, services);
        }

        private static IEnumerable<Type> GetImplementedInterfaces(Type type)
        {
            var types = type.GetTypeInfo().ImplementedInterfaces.Where(i => i != typeof(IDisposable));
            if (!type.GetTypeInfo().IsInterface)
            {
                return types.ToArray();
            }

            return types.AppendItem(type).ToArray();
        }

        private static void Register(IEnumerable<Type> types, IServiceCollection services)
        {
            foreach (var sdkConfigType in types)
            {
                foreach (var implementedInterface in GetImplementedInterfaces(sdkConfigType))
                {
                    services.Add(new ServiceDescriptor(implementedInterface, sdkConfigType, ServiceLifetime.Singleton));
                }
            }
        }
    }
}
#endif
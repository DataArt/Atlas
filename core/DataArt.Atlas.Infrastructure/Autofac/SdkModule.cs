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
using System.Linq;
using Autofac;
using DataArt.Atlas.Infrastructure.Interfaces;

namespace DataArt.Atlas.Infrastructure.Autofac
{
    public abstract class SdkModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // gets all sdk configs form the assmebly where Autofac module is defined
            var assembly = GetType().Assembly;
            builder.RegisterAssemblyTypes(assembly)
                .Where(x => typeof(ISdkConfig).IsAssignableFrom(x))
                .AsImplementedInterfaces()
                .SingleInstance();

            // now retrive all interfaces inherited from ISdkClient
            var interfaces = assembly.GetTypes()
                .Where(x => x.IsInterface)
                .Where(x => typeof(ISdkClient).IsAssignableFrom(x))
                .ToList();

            // and register all the classes implementing client's interfaces
            builder.RegisterAssemblyTypes(assembly)
                .Where(x => x.IsClass)
                .Where(x => interfaces.Any(y => y.IsAssignableFrom(x)))
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
#endif
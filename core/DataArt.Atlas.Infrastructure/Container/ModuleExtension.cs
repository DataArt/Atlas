﻿#region License
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
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Infrastructure.Container
{
    public static class ModuleExtension
    {
        public static void RegisterModule<T>(this IServiceCollection services)
            where T : Module, new()
        {
            var instance = (Module)Activator.CreateInstance(typeof(T));
            services.RegisterModule(instance);
        }

        public static void RegisterModule(this IServiceCollection services, Module module)
        {
            module.Configure(services);
        }
    }
}
#endif
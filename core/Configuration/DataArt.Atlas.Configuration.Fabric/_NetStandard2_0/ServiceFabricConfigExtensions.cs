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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Configuration.Fabric
{
    public static class ServiceFabricConfigExtensions
    {
        private const string DefaultConfigurationPackageName = "Config";

        public static IConfigurationBuilder UseServiceFabricConfiguration(this IConfigurationBuilder builder, string packageName = DefaultConfigurationPackageName)
        {
            return builder.Add(new ServiceFabricConfigSource(packageName));
        }

        public static void UseServiceFabricConfiguration(this IServiceCollection services, string packageName = DefaultConfigurationPackageName)
        {
            var builder = new ConfigurationBuilder().UseServiceFabricConfiguration(packageName);

            services.AddSingleton<IConfiguration>(builder.Build());
        }
    }
}
#endif
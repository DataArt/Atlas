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
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Runtime;

namespace DataArt.Atlas.Hosting.Fabric
{
    public static class ServiceFabricExtensions
    {
        private static readonly ILogger Logger = AtlasLogging.CreateLogger("ServiceFabricExtensions");

        // In case of multiple http/https endpoints per service in ServiceFabric
        // Please use method without 'builder' parameter
        public static void StartApplicationInFabric<TStartup>(
            this IWebHostBuilder builder,
            string serviceTypeName,
            string serviceManifestName,
            Action<IServiceCollection> configureServices,
            Func<X509Certificate2> configureEndpointCertificate = null)
            where TStartup : class
        {
            ConfigureEventSource(serviceTypeName);

            try
            {
                ServiceRuntime.RegisterServiceAsync(serviceTypeName, context =>
                    {
                        var baseStatelessService = new BaseStatelessService<TStartup>(
                            builder,
                            serviceTypeName,
                            serviceManifestName,
                            context,
                            configureServices,
                            configureEndpointCertificate);

                        baseStatelessService.Initialize();
                        return baseStatelessService;
                    })
                    .GetAwaiter()
                    .GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, serviceTypeName);

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "Unhandled exception while registering the fabric service");
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        public static void StartApplicationInFabric<TStartup>(
            string serviceTypeName,
            string serviceManifestName,
            Action<IServiceCollection> configureServices,
            Func<X509Certificate2> configureEndpointCertificate = null)
            where TStartup : class
        {
            StartApplicationInFabric<TStartup>(null, serviceTypeName, serviceManifestName, configureServices, configureEndpointCertificate);
        }

        public static void StartInFabric<TStartup>(this IWebHostBuilder builder, string serviceTypeName, string serviceManifestName, string serviceKey, Action<IServiceCollection> configureServices = null, Func<X509Certificate2> configureEndpointCertificate = null)
            where TStartup : class
        {
            StartApplicationInFabric<TStartup>(builder, serviceTypeName, serviceManifestName, configureServices, configureEndpointCertificate);
        }

        public static void StartInFabric<TStartup>(this IWebHostBuilder builder, string serviceKey, Action<IServiceCollection> configureServices = null, Func<X509Certificate2> configureEndpointCertificate = null)
            where TStartup : class
        {
            StartApplicationInFabric<TStartup>(builder, $"{serviceKey}Type", $"{serviceKey}Pkg", configureServices, configureEndpointCertificate);
        }

        private static void ConfigureEventSource(string appTypeName)
        {
            var eventSource = (EventSourceAttribute)typeof(ServiceEventSource).GetCustomAttributes(typeof(EventSourceAttribute), false).First();
            eventSource.Name = appTypeName;
        }
    }
}
#endif
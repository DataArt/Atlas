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
#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Fabric.Description;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace DataArt.Atlas.Azure.Hosting.Fabric
{
    internal sealed class BaseStatelessService<TStartup> : StatelessService
        where TStartup : class
    {
        private readonly IWebHostBuilder builder;
        private readonly string serviceTypeName;
        private readonly string serviceManifestName;
        private readonly StatelessServiceContext context;
        private readonly Action<IServiceCollection> configureServices;
        private readonly Func<X509Certificate2> configureEndpointCertificate;
        private ServiceFabricApplication fabricApplication;

        public BaseStatelessService(
            IWebHostBuilder builder,
            string serviceTypeName,
            string serviceManifestName,
            StatelessServiceContext context,
            Action<IServiceCollection> configureServices,
            Func<X509Certificate2> configureEndpointCertificate)
            : base(context)
        {
            this.builder = builder;
            this.serviceTypeName = serviceTypeName;
            this.serviceManifestName = serviceManifestName;
            this.context = context;
            this.configureServices = configureServices;
            this.configureEndpointCertificate = configureEndpointCertificate;
        }

        public void Initialize()
        {
            FabricApplication.Name = context.CodePackageActivationContext.ApplicationName;
            fabricApplication = new ServiceFabricApplication(serviceTypeName, Context);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var endpoints = context.CodePackageActivationContext.GetEndpoints()
                .Where(endpoint => endpoint.Protocol == EndpointProtocol.Http || endpoint.Protocol == EndpointProtocol.Https);

                return endpoints.Select(endpoint => new ServiceInstanceListener(serviceContext =>

                // New Service Fabric listener for each endpoint configuration in the manifest.
                new KestrelCommunicationListener(serviceContext, endpoint.Name, (url, listener) =>
                {
                    var webHostBuilder = builder ?? new WebHostBuilder();
                    return webHostBuilder
                        .UseKestrel(options =>
                        {
                            var port = endpoint.Port;
                            if (endpoint.Protocol == EndpointProtocol.Http)
                            {
                                options.Listen(
                                    IPAddress.IPv6Any,
                                    port,
                                    listenOptions =>
                                    {
                                        // todo:
                                        // listenOptions.UseConnectionLogging();
                                    });
                            }

                            if (endpoint.Protocol == EndpointProtocol.Https)
                            {
                                options.Listen(
                                    IPAddress.IPv6Any,
                                    port,
                                    listenOptions =>
                                    {
                                        // todo:
                                        // listenOptions.UseConnectionLogging();
                                        listenOptions.UseHttps(configureEndpointCertificate != null ?
                                            configureEndpointCertificate() :
                                            Certificate.FindBindedCertificate(fabricApplication, context.CodePackageActivationContext.ApplicationTypeName, serviceManifestName));
                                    });
                            }
                        })
                        .ConfigureServices(services =>
                        {
                            configureServices?.Invoke(services);
                            services.AddSingleton(serviceContext);
                            services.AddSingleton<IApplication>(fabricApplication);
                        })
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<TStartup>()
                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                        .Build();
                })));
        }
    }
}
#endif
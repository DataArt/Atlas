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
using System;
using DataArt.Atlas.Client.Request;
using DataArt.Atlas.Core.Container;
using DataArt.Atlas.Core.ServiceDiscovery;
using Flurl.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Client.Container
{
    public class ClientModule : Module
    {
        protected override void Configure(IServiceCollection services)
        {
            services.AddSingleton<IFlurlClientFactory, PerHostFlurlClientFactory>();
            services.AddSingleton<DefaultFlurlHttpSettings>();
            services.AddTransient<IRequestFactory, RequestFactory>();
            services.AddTransient<IInternalRequestFactory, InternalRequestFactory>();
            services.AddTransient<Func<string, IInternalRequestFactory>>(provider =>
            {
                return str => new InternalRequestFactory(
                    str,
                    provider.GetService<IServiceDiscovery>(),
                    provider.GetService<IFlurlClientFactory>(),
                    provider.GetService<DefaultFlurlHttpSettings>());
            });

            services.AddTransient<Func<IClient, IRequestFactory>>(provider =>
            {
                return client => new RequestFactory(
                    client,
                    provider.GetService<IServiceDiscovery>(),
                    provider.GetService<IFlurlClientFactory>(),
                    provider.GetServices<IClientConfig>(),
                    provider.GetService<DefaultFlurlHttpSettings>());
            });
        }
    }
}
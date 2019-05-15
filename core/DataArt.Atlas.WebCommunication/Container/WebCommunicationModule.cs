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
using DataArt.Atlas.Infrastructure.Container;
using DataArt.Atlas.Infrastructure.Interfaces;
using DataArt.Atlas.ServiceDiscovery;
using DataArt.Atlas.WebCommunication.Request;
using Flurl.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.WebCommunication.Container
{
    public class WebCommunicationModule : Module
    {
        protected override void Load(IServiceCollection services)
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

            services.AddTransient<Func<ISdkClient, IRequestFactory>>(provider =>
            {
                return client => new RequestFactory(
                    client,
                    provider.GetService<IServiceDiscovery>(),
                    provider.GetService<IFlurlClientFactory>(),
                    provider.GetServices<ISdkConfig>(),
                    provider.GetService<DefaultFlurlHttpSettings>());
            });
        }
    }
}
#endif
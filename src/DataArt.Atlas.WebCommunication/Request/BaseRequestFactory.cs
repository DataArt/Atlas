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
using System.Collections.Generic;
using DataArt.Atlas.Client.Extensions;
using DataArt.Atlas.Core.ServiceDiscovery;
using Flurl.Http.Configuration;

namespace DataArt.Atlas.Client.Request
{
    internal abstract class BaseRequestFactory
    {
        private readonly string serviceKey;

        private readonly IServiceDiscovery serviceDiscovery;
        private readonly IFlurlClientFactory flurlClientFactory;
        private readonly DefaultFlurlHttpSettings defaultFlurlHttpSettings;

        protected BaseRequestFactory(
            string serviceKey,
            IServiceDiscovery serviceDiscovery,
            IFlurlClientFactory flurlClientFactory,
            DefaultFlurlHttpSettings defaultFlurlHttpSettings)
        {
            this.serviceKey = serviceKey;

            this.serviceDiscovery = serviceDiscovery;
            this.flurlClientFactory = flurlClientFactory;
            this.defaultFlurlHttpSettings = defaultFlurlHttpSettings;
        }

        protected BaseRequestFactory(
            IClient client,
            IServiceDiscovery serviceDiscovery,
            IFlurlClientFactory flurlClientFactory,
            IEnumerable<IClientConfig> configProvider,
            DefaultFlurlHttpSettings defaultFlurlHttpSettings)
        {
            serviceKey = configProvider.GetServiceKey(client.GetType());

            this.serviceDiscovery = serviceDiscovery;
            this.flurlClientFactory = flurlClientFactory;
            this.defaultFlurlHttpSettings = defaultFlurlHttpSettings;
        }

        public IGetRequest GetRequest(string resource)
        {
            return Create(resource);
        }

        public IPostRequest PostRequest(string resource)
        {
            return Create(resource);
        }

        public IPutRequest PutRequest(string resource)
        {
            return Create(resource);
        }

        public IDeleteRequest DeleteRequest(string resource)
        {
            return Create(resource);
        }

        private WebRequest Create(string resource)
        {
            return new WebRequest(resource, serviceKey, serviceDiscovery, flurlClientFactory, defaultFlurlHttpSettings);
        }
    }
}
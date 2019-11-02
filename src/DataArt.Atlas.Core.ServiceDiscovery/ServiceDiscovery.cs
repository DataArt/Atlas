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
using System.Collections.Generic;

namespace DataArt.Atlas.Core.ServiceDiscovery
{
    public sealed class ServiceDiscovery : IServiceDiscovery
    {
        private readonly IDictionary<string, string> serviceMap;

        public ServiceDiscovery(IDictionary<string, string> serviceMap)
        {
            this.serviceMap = serviceMap;
        }

        public Uri ResolveServiceUrl(string serviceKey)
        {
            if (!serviceMap.TryGetValue(serviceKey, out var result))
            {
                throw new InvalidOperationException($"Invalid {serviceKey}");
            }

            return new Uri(result);
        }
    }
}
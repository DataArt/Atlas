#region License

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

#endregion

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataArt.Atlas.Core.ServiceDiscovery.File
{
    public sealed class ServiceDiscovery : IServiceDiscovery
    {
        private readonly object lockObject = new object();
        private readonly string configurationPath;
        private volatile IDictionary<string, string> serviceUrls;

        private IDictionary<string, string> ServiceUrls
        {
            get
            {
                if (serviceUrls == null)
                {
                    lock (lockObject)
                    {
                        if (serviceUrls == null)
                        {
                            serviceUrls = GetServiceUrls();
                        }
                    }
                }

                return serviceUrls;
            }
        }

        public ServiceDiscovery() : this(null)
        {
        }

        public ServiceDiscovery(string configurationPath)
        {
            if (string.IsNullOrEmpty(configurationPath))
            {
                configurationPath = "../../console/configuration.json";
            }

            this.configurationPath = configurationPath;
        }

        public Uri ResolveServiceUrl(string serviceKey)
        {
            if (!ServiceUrls.TryGetValue(serviceKey, out var result))
            {
                throw new InvalidOperationException("Invalid service key");
            }

            return new Uri(result);
        }

        private IDictionary<string, string> GetServiceUrls()
        {
            var result = new Dictionary<string, string>();

            if (!System.IO.File.Exists(configurationPath))
            {
                throw new InvalidOperationException("Invalid configuration settings");
            }

            var json = System.IO.File.ReadAllText(configurationPath);
            var configuration = JsonConvert.DeserializeObject<JObject>(json);

            foreach (var item in configuration)
            {
                result.Add(item.Key, item.Value["ApplicationUrl"].Value<string>().Replace("+", "localhost"));
            }

            return result;
        }
    }
}
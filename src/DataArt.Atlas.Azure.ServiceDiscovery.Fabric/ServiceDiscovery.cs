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
using System.Collections.Concurrent;
using System.Fabric;
using System.Linq;
using System.Threading;
using DataArt.Atlas.Core.ServiceDiscovery;
using DataArt.Atlas.Hosting.Fabric;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;

namespace DataArt.Atlas.ServiceDiscovery.Fabric
{
    public sealed class ServiceDiscovery : IServiceDiscovery
    {
        private readonly ConcurrentDictionary<string, Service> services = new ConcurrentDictionary<string, Service>();
        private readonly ServicePartitionResolver resolver;

        public ServiceDiscovery()
        {
            resolver = ServicePartitionResolver.GetDefault();
        }

        public Uri ResolveServiceUrl(string serviceKey)
        {
            var service = services.AddOrUpdate(serviceKey, FirstCall, RetryCall);
            return new Uri(service.Endpoint);
        }

        private static string GetRandomEndpoint(ResolvedServicePartition partition, string previous = null)
        {
            var serviceEndpoints = partition.Endpoints
                .Select(e => e.Address)
                .Select(JsonConvert.DeserializeObject<ServiceDiscoveryEndpoints>)
                .SelectMany(e => e.Endpoints)
                .Select(e => e.Value)
                .Where(e => e != previous)
                .ToList();

            if (serviceEndpoints.Count == 0)
            {
                return previous;
            }

            return serviceEndpoints.RandomElement();
        }

        private Service FirstCall(string serviceKey)
        {
            var cancelationToken = new CancellationTokenSource().Token;
            var partition = resolver.ResolveAsync(new Uri($"{FabricApplication.Name}/{serviceKey}"), null, cancelationToken).Result;
            var endpoint = GetRandomEndpoint(partition);

            return new Service(partition, endpoint);
        }

        private Service RetryCall(string serviceKey, Service service)
        {
            var cancelationToken = new CancellationTokenSource().Token;
            var partition = resolver.ResolveAsync(service.Partition, cancelationToken).Result;
            var endpoint = GetRandomEndpoint(partition, service.Endpoint);

            return new Service(partition, endpoint);
        }

        private sealed class Service
        {
#pragma warning disable SA1401 // Fields must be private
            public readonly ResolvedServicePartition Partition;
            public readonly string Endpoint;
#pragma warning restore SA1401 // Fields must be private

            public Service(ResolvedServicePartition partition, string endpoint)
            {
                Partition = partition;
                Endpoint = endpoint;
            }
        }
    }
}

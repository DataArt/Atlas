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
#if NET452
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace DataArt.Atlas.Hosting.Fabric
{
    internal class FabricOwinCommunicationListener : ICommunicationListener
    {
        private readonly IApplication appInstance;
        private readonly EndpointResourceDescription endpoint;

        public FabricOwinCommunicationListener(IApplication appInstance, EndpointResourceDescription endpoint = null)
        {
            this.appInstance = appInstance ?? throw new ArgumentNullException(nameof(appInstance));
            this.endpoint = endpoint;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            await appInstance.StartAsync(CreateUrlFromEndpoint(), cancellationToken);
            return appInstance.HostingSettings.Url.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await appInstance.StopAsync();
        }

        public void Abort()
        {
            appInstance.StopAsync().GetAwaiter().GetResult();
        }

        private string CreateUrlFromEndpoint()
        {
            if (endpoint == null)
            {
                return null;
            }

            switch (endpoint.Protocol)
            {
                case EndpointProtocol.Https:
                    return $"https://+:{endpoint.Port}/";
                case EndpointProtocol.Http:
                    return $"http://+:{endpoint.Port}/";
                default:
                    throw new ArgumentOutOfRangeException($"{endpoint.Protocol} protocol is not supported");
            }
        }
    }
}
#endif
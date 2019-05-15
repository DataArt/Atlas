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
using System;
using DataArt.Atlas.Common.Esb;
using DataArt.Atlas.Messaging;
using DataArt.Atlas.WebCommunication;
using Microsoft.Extensions.Logging;

namespace DataArt.Atlas.AlphaService.Communication
{
    internal sealed class CommunicationService : ICommunicationService
    {
        private readonly IServiceBus bus;
        private readonly Func<string, IInternalRequestFactory> factoryFunc;
        private readonly ILogger<CommunicationService> logger;

        public CommunicationService(IServiceBus bus, Func<string, IInternalRequestFactory> factoryFunc, ILogger<CommunicationService> logger)
        {
            this.bus = bus;
            this.factoryFunc = factoryFunc;
            this.logger = logger;
        }

        public void Communicate(CommunicationType type, string serviceKey = null)
        {
            logger.LogDebug("Choosen communication type {type}", type);

            switch (type)
            {
                case CommunicationType.Esb:
                    SendEsbMessage();
                    break;
                case CommunicationType.WebAPi:
                    SendWebApiVersionRequest(serviceKey);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void SendEsbMessage()
        {
            bus.Publish(new CoreCommunicationMessage());
        }

        private void SendWebApiVersionRequest(string serviceKey)
        {
            var factory = factoryFunc(serviceKey);
            factory.GetRequest("api/version/assembly").GetAsync<Version>().Wait();
        }
    }
}

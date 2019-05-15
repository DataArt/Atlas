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
using Serilog;
using Serilog.Context;

namespace DataArt.Atlas.AlphaService.Communication
{
    internal sealed class CommunicationService : ICommunicationService
    {
        private readonly IServiceBus bus;
        private readonly Func<string, IInternalRequestFactory> factoryFunc;
        
        public CommunicationService(IServiceBus bus, Func<string, IInternalRequestFactory> factoryFunc)
        {
            this.bus = bus;
            this.factoryFunc = factoryFunc;
        }

        public void Communicate(CommunicationType type, string serviceKey = null)
        {
            // Example of PushProperty usage
            //using (LogContext.PushProperty("CommunicationType", type))
            //{
            //    Log.Information("Communication flow");
            //}

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
            bus.Publish(new CommunicationMessage());
        }

        private void SendWebApiVersionRequest(string serviceKey)
        {
            var factory = factoryFunc(serviceKey);
            factory.GetRequest("api/version").GetAsync<Version>().Wait();
        }
    }
}

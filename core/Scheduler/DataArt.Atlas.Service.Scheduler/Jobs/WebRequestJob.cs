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
using System.Linq;
using System.Threading.Tasks;
using DataArt.Atlas.Service.Scheduler.Sdk.Models;
using DataArt.Atlas.WebCommunication;
using Quartz;

namespace DataArt.Atlas.Service.Scheduler.Jobs
{
    [DisallowConcurrentExecution]
    internal class WebRequestJob : BaseJob<WebRequestJobSettingsModel>
    {
        private readonly Func<string, IInternalRequestFactory> serviceRequestFactory;

        public WebRequestJob(Func<string, IInternalRequestFactory> serviceRequestFactory)
        {
            this.serviceRequestFactory = serviceRequestFactory;
        }

        public override Task ExecuteAsync(WebRequestJobSettingsModel settings)
        {
            var requestFactory = serviceRequestFactory(settings.ServiceKey);
            var request = requestFactory.PostRequest(settings.Uri);
            request.SetTimeout(settings.Timeout);

            if (settings.Parameters != null)
            {
                foreach (var parameter in settings.Parameters.Where(p => !string.IsNullOrWhiteSpace(p.Value)))
                {
                    request.AddUriParameter(parameter.Key, parameter.Value);
                }
            }

            return request.PostAsync();
        }
    }
}

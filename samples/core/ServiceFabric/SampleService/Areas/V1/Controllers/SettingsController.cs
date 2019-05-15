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
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.ServiceDiscovery;
using SampleService.Settings;
using Microsoft.AspNetCore.Mvc;

namespace SampleService.Areas.V1.Controllers
{
    [Route("api/v1/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly IServiceDiscovery serviceDiscovery;
        private readonly SampleSettings sampleSettings;

        public SettingsController(IServiceDiscovery serviceDiscovery, SampleSettings sampleSettings)
        {
            this.serviceDiscovery = serviceDiscovery;
            this.sampleSettings = sampleSettings;
        }

        [Route("url")]
        [HttpGet]
        public string GetUrl()
        {
            var url = serviceDiscovery.ResolveServiceUrl(Application.SampleService.Key).AbsoluteUri;
            return $"Current service URL: {url}";
        }

        [Route("encrypted")]
        [HttpGet]
        public string GetEncryptedAndUnEncryptedSettings()
        {
            // Getting unencrypted and encrypted settings from ServiceFabric

            // For a complete tutorial on how to issue certificate, register it,
            // encrypt text and promote it to SF service settings file please refer to
            // https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-application-secret-management
            // tutorial

            // The last steps to get parameters from configuration and decrypt it if necessary
            // Atlas does for you automatically, please notice here the usage:
            //     serviceFabricSampleSettings.EncryptedProperty
            // which just returns decrypted text from settings

            var unencrypted = sampleSettings.UnEncryptedProperty;
            var encrypted = sampleSettings.EncryptedProperty;
            return $"Unencrypted prop: {unencrypted}, encrypted prop: {encrypted}";
        }
    }
}

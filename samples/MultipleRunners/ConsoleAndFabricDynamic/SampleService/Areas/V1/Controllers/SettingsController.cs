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
using System.Web.Http;

namespace SampleService.Areas.V1.Controllers
{
    [RoutePrefix("api/v1/settings")]
    public class SettingsController : ApiController
    {
        private readonly ApplicationSettings applicationSettings;

        public SettingsController(ApplicationSettings applicationSettings)
        {
            this.applicationSettings = applicationSettings;
        }

        [Route("url")]
        [HttpGet]
        public string GetUrl()
        {
            var url = applicationSettings.Hosting.Url;
            var name = applicationSettings.Logging.ApplicationName;
            return $"Current service URL: {url}, current service name: {name}";
        }
    }
}

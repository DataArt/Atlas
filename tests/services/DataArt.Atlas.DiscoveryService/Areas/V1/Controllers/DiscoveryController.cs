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
using System.Web.Http;
using DataArt.Atlas.DiscoveryService.Settings;
using DataArt.Atlas.Infrastructure.Exceptions;

namespace DataArt.Atlas.DiscoveryService.Areas.V1.Controllers
{
    [RoutePrefix("api/v1/discovery")]
    public sealed class DiscoveryController : ApiController
    {
        private readonly DiscoverySettings[] settings;

        public DiscoveryController(KeyValueSettings settings)
        {
            this.settings = new[] {settings.AlphaDiscovery, settings.BetaDiscovery};
        }

        [HttpGet]
        [Route("")]
        public Uri Get([FromUri] string serviceKey)
        {
            var setting = settings.FirstOrDefault(s => s.ServiceKey.Equals(serviceKey, StringComparison.InvariantCultureIgnoreCase));
            if (setting == null)
            {
                throw new NotFoundException();    
            }

            return new Uri(setting.ServiceEndpoint);
        }
    }
}

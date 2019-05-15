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
using System.Collections.Generic;
using DataArt.Atlas.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DataArt.Atlas.DiscoveryService.Areas.V1.Controllers
{
    [Route("api/v1/discovery")]
    public sealed class DiscoveryController : ControllerBase
    {
        private readonly Dictionary<string,string> settings;

        public DiscoveryController(Discovery settings)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            this.settings = new Dictionary<string, string>(settings.Endpoints, comparer);
        }

        [HttpGet]
        [Route("")]
        public Uri Get([FromQuery] string serviceKey)
        {
            
            if (!settings.TryGetValue(serviceKey, out string value))
            {
                throw new NotFoundException();    
            }

            return new Uri(value);
        }
    }
}

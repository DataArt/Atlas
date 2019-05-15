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
using System.Web.Http;
using NSwag.Annotations;

namespace DataArt.Atlas.Core.Application.Http.Controllers
{
    [RoutePrefix("api/version")]
    public class VersionController : ApiController
    {
        private const string SwaggerSectionInfo = @"{
                                                        ""title"":""Version"",
                                                        ""linkHash"":""version""
                                                    }";

        /// <summary>
        /// Gets the version of API
        /// </summary>
        /// <returns>API Version</returns>
        [HttpGet]
        [Route("")]
        [AuthorizeDisabled]
#pragma warning disable SA1118 // Parameter must not span multiple lines
        [SwaggerTags(SwaggerSectionInfo, @"{
                                    ""title"":""Get API Version"",
                                    ""description"":""Returns the API Version."",
                                    ""linkHash"":""getversion""
                                    }")]
#pragma warning restore SA1118 // Parameter must not span multiple lines
        public Version GetVersion()
        {
            var assembly = TypeLocator.GetEntryPointAssembly();
            return assembly.GetName().Version;
        }
    }
 }
#endif
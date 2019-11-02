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
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace DataArt.Atlas.Core.Application.Http.Controllers
{
    [Route("api/version")]
    [AuthorizeDisabled]
    public class VersionController : ControllerBase
    {
        #region Consts
        private const string SwaggerSectionInfo = @"{
                                                        ""title"":""Version"",
                                                        ""linkHash"":""version""
                                                    }";

        private const string SwaggerSectionAssembly = @"{
                                    ""title"":""Get Assembly Version"",
                                    ""description"":""Returns the Assembly Version."",
                                    ""linkHash"":""getassemblyversion""
                                    }";

        private const string SwaggerSectionService = @"{
                                    ""title"":""Get Service Version"",
                                    ""description"":""Returns the Service Version."",
                                    ""linkHash"":""getserviceversion""
                                    }";

        private const string SwaggerSectionApplication = @"{
                                    ""title"":""Get Application Version"",
                                    ""description"":""Returns the Application Version."",
                                    ""linkHash"":""getapplicationversion""
                                    }";
        #endregion

        private readonly IApplication application;

        public VersionController(IApplication application)
        {
            this.application = application;
        }

        /// <summary>
        /// Gets the version of API
        /// </summary>
        /// <returns>API Version</returns>
        [HttpGet]
        [Route("assembly")]
        [SwaggerTags(SwaggerSectionInfo, SwaggerSectionAssembly)]
        public Version GetAssemblyVersion()
        {
            var assembly = TypeLocator.GetEntryPointAssembly();
            return assembly.GetName().Version;
        }

        /// <summary>
        /// Gets the version of API
        /// </summary>
        /// <returns>API Version</returns>
        [HttpGet]
        [Route("service")]
        [SwaggerTags(SwaggerSectionInfo, SwaggerSectionService)]
        public string GetServiceVersion()
        {
            return application.GetServiceVersionFunction();
        }

        /// <summary>
        /// Gets the version of API
        /// </summary>
        /// <returns>API Version</returns>
        [HttpGet]
        [Route("application")]
        [SwaggerTags(SwaggerSectionInfo, SwaggerSectionApplication)]
        public string GetApplicationVersion()
        {
            return application.GetApplicationVersionFunction();
        }
    }
}

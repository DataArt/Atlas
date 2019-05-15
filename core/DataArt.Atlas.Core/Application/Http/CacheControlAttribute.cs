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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace DataArt.Atlas.Core.Application.Http
{
    public class CacheControlAttribute : ActionFilterAttribute
    {
        private readonly int maxAge;

        public CacheControlAttribute(int maxAge = 0)
        {
            this.maxAge = maxAge;
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context.Response != null && context.Request != null && context.Request.Method == HttpMethod.Get)
            {
                context.Response.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = maxAge == 0,
                    NoStore = maxAge == 0,
                    MaxAge = TimeSpan.FromSeconds(maxAge)
                };

                if (maxAge == 0)
                {
                    context.Response.Headers.TryAddWithoutValidation("Pragma", new[] { "no-cache" });
                }
            }

            base.OnActionExecuted(context);
        }
    }
}
#endif
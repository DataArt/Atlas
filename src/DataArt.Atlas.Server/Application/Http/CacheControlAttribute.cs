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

#if NETSTANDARD2_0
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DataArt.Atlas.Core.Application.Http
{
    public class CacheControlAttribute : ActionFilterAttribute
    {
        private readonly int maxAge;

        public CacheControlAttribute(int maxAge = 0)
        {
            this.maxAge = maxAge;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Response != null &&
                context.HttpContext.Request != null &&
                context.HttpContext.Request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
            {
                var headers = context.HttpContext.Response.GetTypedHeaders();
                headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                {
                    NoCache = maxAge == 0,
                    NoStore = maxAge == 0,
                    MaxAge = TimeSpan.FromSeconds(maxAge)
                };

                if (maxAge == 0)
                {
                    context.HttpContext.Response.Headers.Add("Pragma", new[] { "no-cache" });
                }
            }

            base.OnActionExecuted(context);
        }
    }
}
#endif
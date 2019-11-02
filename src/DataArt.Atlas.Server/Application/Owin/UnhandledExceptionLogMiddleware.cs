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
using System.Threading.Tasks;
using DataArt.Atlas.Core.Application.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DataArt.Atlas.Core.Application.Owin
{
    public class UnhandledExceptionLogMiddleware
    {
        private readonly RequestDelegate next;

        public UnhandledExceptionLogMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext, ILogger<UnhandledExceptionLogMiddleware> logger)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Unhandled exception: {@OwinUnhandledExceptionEvent}", new OwinUnhandledExceptionEvent(httpContext));
            }
        }
    }
}
#endif
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
using System.Diagnostics;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using DataArt.Atlas.Core.Application.Logging;
using Serilog;

namespace DataArt.Atlas.Core.Application.Http
{
    public class ApiAccessLoggingFilter : ActionFilterAttribute
    {
        private const string StopwatchKey = "StopwatchFilter";

        public override void OnActionExecuting(HttpActionContext context)
        {
            base.OnActionExecuting(context);

            context.Request.Properties[StopwatchKey] = Stopwatch.StartNew();
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            var stopwatch = (Stopwatch)context.Request.Properties[StopwatchKey];
            var apiAccessEvent = new ApiAccessEvent(context.Request, context.Response, stopwatch.ElapsedMilliseconds);

            var statusCode = context.Response?.StatusCode;

            var isSuccess = context.Response != null && context.Response.IsSuccessStatusCode;
            var isWarning = !isSuccess && statusCode.HasValue && statusCode != HttpStatusCode.InternalServerError;

            if (isSuccess)
            {
                Log.Debug("Api successful access: {@ApiAccess}", apiAccessEvent);
            }
            else
            {
                var exception = GetHandledException(context);

                if (isWarning)
                {
                    Log.Warning(exception, "Api unsuccessful access: {@ApiAccess}", apiAccessEvent);
                }
                else
                {
                    Log.Error(exception, "Api error: {@ApiAccess}", apiAccessEvent);
                }
            }
        }

        private static Exception GetHandledException(HttpActionExecutedContext context)
        {
            context.Request.Properties.TryGetValue(ApiExceptionHandlingFilter.HandledExceptionKey, out var handledException);
            return handledException as Exception;
        }
    }
}
#endif
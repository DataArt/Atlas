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

#if NETSTANDARD2_0
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DataArt.Atlas.Core.Application.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DataArt.Atlas.Core.Application.Http
{
    public class ApiAccessLoggingMiddleware
    {
        private readonly RequestDelegate requestDelegate;

        public ApiAccessLoggingMiddleware(RequestDelegate next)
        {
            requestDelegate = next;
        }

        public async Task Invoke(HttpContext httpContext, ILogger<ApiAccessLoggingMiddleware> logger)
        {
            try
            {
                var start = Stopwatch.GetTimestamp();

                await requestDelegate(httpContext);

                var elapsed = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());

                var apiAccessEvent = new ApiAccessEvent(httpContext.Request, httpContext.Response, elapsed);

                var statusCode = httpContext.Response?.StatusCode;

                var level = LogLevel.Warning;
                if (statusCode == null || statusCode > 499)
                {
                    level = LogLevel.Error;
                }
                else if (statusCode >= 200 && statusCode <= 299)
                {
                    level = LogLevel.Debug;
                }

                Exception exception;
                switch (level)
                {
                    case LogLevel.Error:
                        exception = GetHandledException(httpContext);
                        logger.LogError(exception, "Api error: {@ApiAccess}", apiAccessEvent);
                        break;
                    case LogLevel.Warning:
                        exception = GetHandledException(httpContext);
                        logger.LogWarning(exception, "Api unsuccessful access: {@ApiAccess}", apiAccessEvent);
                        break;
                    case LogLevel.Debug:
                        logger.LogDebug("Api successful access: {@ApiAccess}", apiAccessEvent);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static Exception GetHandledException(HttpContext context)
        {
            context.Items.TryGetValue(ApiExceptionHandlingFilter.HandledExceptionKey, out var handledException);
            return handledException as Exception;
        }

        private static long GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / Stopwatch.Frequency;
        }
    }
}
#endif
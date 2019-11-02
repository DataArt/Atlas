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
using System.Net;
using DataArt.Atlas.Core.Application.Http;

#if NET452
using System.Net.Http;
#endif

#if NETSTANDARD2_0
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
#endif

namespace DataArt.Atlas.Core.Application.Logging
{
    public class ApiAccessEvent
    {
        public DateTime AccessTime { get; set; }

        public string ActionName { get; set; }

        public string ControllerName { get; set; }

        public string HttpMethod { get; set; }

        public string RemoteIpAddress { get; set; }

        public long RequestExecutionTime { get; set; }

        public string RequestUri { get; set; }

        public HttpStatusCode? ResponseStatusCode { get; set; }

        public string RouteTemplate { get; set; }

        public string UserAgent { get; set; }

#if NET452
        public ApiAccessEvent(
            HttpRequestMessage request,
            HttpResponseMessage response = null,
            long? requestExecutionTime = null)
        {
            AccessTime = DateTime.Now;

            var routeData = request.GetRouteData();

            if (requestExecutionTime.HasValue)
            {
                RequestExecutionTime = requestExecutionTime.Value;
            }

            ResponseStatusCode = response?.StatusCode;

            RequestUri = request.RequestUri.AbsoluteUri;
            RemoteIpAddress = request.GetClientIpAddress();
            HttpMethod = request.Method.Method;

            RouteTemplate = routeData?.Route.RouteTemplate ?? string.Empty;
            UserAgent = request.Headers.UserAgent?.ToString() ?? string.Empty;

            var actionDescriptor = request.GetActionDescriptor();
            if (actionDescriptor != null)
            {
                ControllerName = actionDescriptor.ControllerDescriptor.ControllerName;
                ActionName = actionDescriptor.ActionName;
            }
        }
#endif
#if NETSTANDARD2_0
        public ApiAccessEvent(
            HttpRequest request,
            HttpResponse response = null,
            long? requestExecutionTime = null)
        {
            AccessTime = DateTime.Now;
            RequestExecutionTime = requestExecutionTime ?? 0;
            ResponseStatusCode = response != null ? (HttpStatusCode)response.StatusCode : default(HttpStatusCode?);
            RequestUri = request.GetDisplayUrl();
            RemoteIpAddress = request.GetClientIpAddress();
            HttpMethod = request.Method;

            RouteTemplate = request.Path.HasValue ? request.Path.Value : string.Empty;
            UserAgent = request.Headers["User-Agent"].FirstOrDefault();

            ControllerName = request.HttpContext.GetRouteValue("controller")?.ToString();
            ActionName = request.HttpContext.GetRouteValue("action")?.ToString();
        }
#endif
    }
}

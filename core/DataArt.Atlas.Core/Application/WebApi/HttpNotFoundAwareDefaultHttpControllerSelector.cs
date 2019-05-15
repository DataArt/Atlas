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
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace DataArt.Atlas.Core.Application.WebApi
{
    public class HttpNotFoundAwareDefaultHttpControllerSelector : DefaultHttpControllerSelector
    {
        public HttpNotFoundAwareDefaultHttpControllerSelector(HttpConfiguration configuration)
            : base(configuration)
        {
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            HttpControllerDescriptor decriptor;
            try
            {
                decriptor = base.SelectController(request);
            }
            catch (HttpResponseException e)
            {
                if (e.Response.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }

                var routeValues = request.GetRouteData().Values;
                routeValues["controller"] = "Errors";
                routeValues["action"] = "Handle404";
                decriptor = base.SelectController(request);
            }

            return decriptor;
        }
    }
}
#endif
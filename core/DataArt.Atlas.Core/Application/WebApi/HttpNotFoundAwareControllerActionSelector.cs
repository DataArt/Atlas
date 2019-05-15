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
using System.Web.Http;
using System.Web.Http.Controllers;
using DataArt.Atlas.Core.Application.Http.Controllers;

namespace DataArt.Atlas.Core.Application.WebApi
{
    public class HttpNotFoundAwareControllerActionSelector : ApiControllerActionSelector
    {
        public HttpNotFoundAwareControllerActionSelector()
        {
        }

        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            HttpActionDescriptor decriptor;
            try
            {
                decriptor = base.SelectAction(controllerContext);
            }
            catch (HttpResponseException e)
            {
                if (e.Response.StatusCode != HttpStatusCode.NotFound &&
                    e.Response.StatusCode != HttpStatusCode.MethodNotAllowed)
                {
                    throw;
                }

                controllerContext.RouteData.Values.Clear();
                IHttpController errorsController = new ErrorsController();
                controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "Errors", errorsController.GetType());
                controllerContext.RouteData.Values["action"] = "Handle404";
                controllerContext.Controller = errorsController;
                decriptor = base.SelectAction(controllerContext);
            }

            return decriptor;
        }
    }
}
#endif
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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using DataArt.Atlas.Core.Application.Logging;
using Serilog;

namespace DataArt.Atlas.Core.Application.Http
{
    internal sealed class AuthorizeCustomAttribute : AuthorizeAttribute
    {
        private readonly Claim requiredClaim;

        public AuthorizeCustomAttribute(Claim requiredClaim = null)
        {
            this.requiredClaim = requiredClaim;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var isAuthorized = IsAuthorizeDisabled(actionContext) || (base.IsAuthorized(actionContext) && IsClaimValid(actionContext));

            if (!isAuthorized)
            {
                Log.Warning("Api unauthorized access {@ApiAccess}", new ApiAccessEvent(actionContext.Request));
            }

            return isAuthorized;
        }

        private bool IsAuthorizeDisabled(HttpActionContext actionContext)
        {
            var attrs = new List<object>();
            attrs.AddRange(actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AuthorizeDisabledAttribute>());
            attrs.AddRange(actionContext.ActionDescriptor.GetCustomAttributes<AuthorizeDisabledAttribute>());

            return attrs.Any();
        }

        private bool IsClaimValid(HttpActionContext actionContext)
        {
            var isValid = true;
            if (requiredClaim != null)
            {
                var principal = (ClaimsPrincipal)actionContext?.RequestContext?.Principal;
                isValid = principal?.HasClaim(x => x.Type.Equals(requiredClaim.Type, StringComparison.OrdinalIgnoreCase)
                                                   && x.Value.Equals(requiredClaim.Value, StringComparison.OrdinalIgnoreCase)) ?? false;
            }

            return isValid;
        }
    }
}
#endif
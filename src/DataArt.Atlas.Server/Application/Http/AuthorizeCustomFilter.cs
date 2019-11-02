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
using DataArt.Atlas.Core.Logging;
#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using DataArt.Atlas.Core.Application.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace DataArt.Atlas.Core.Application.Http
{
    internal sealed class AuthorizeCustomFilter : IAuthorizationFilter
    {
        private readonly ILogger logger = AtlasLogging.CreateLogger<AuthorizeCustomFilter>();
        private readonly Claim requiredClaim;

        public AuthorizeCustomFilter(Claim requiredClaim = null)
        {
            this.requiredClaim = requiredClaim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isAuthorized = IsAuthorizeDisabled(context) || IsClaimValid(context);

            if (!isAuthorized)
            {
                logger.LogWarning("Api unauthorized access {@ApiAccess}", new ApiAccessEvent(context.HttpContext.Request));
                context.Result = new UnauthorizedResult();
            }
        }

        private bool IsAuthorizeDisabled(AuthorizationFilterContext context)
        {
            var attrs = new List<object>();
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var controllerAttrs = controllerActionDescriptor?.ControllerTypeInfo
                .GetCustomAttributes<AuthorizeDisabledAttribute>();
            if (controllerAttrs != null)
            {
                attrs.AddRange(controllerAttrs);
            }

            var actionAttrs = controllerActionDescriptor?.MethodInfo.GetCustomAttributes<AuthorizeDisabledAttribute>();
            if (actionAttrs != null)
            {
                attrs.AddRange(actionAttrs);
            }

            return attrs.Any();
        }

        private bool IsClaimValid(AuthorizationFilterContext actionContext)
        {
            var isValid = true;
            if (requiredClaim != null)
            {
                var user = actionContext.HttpContext.User;
                isValid = user?.HasClaim(x => x.Type.Equals(requiredClaim.Type, StringComparison.OrdinalIgnoreCase)
                                                   && x.Value.Equals(requiredClaim.Value, StringComparison.OrdinalIgnoreCase)) ?? false;
            }

            return isValid;
        }
    }
}
#endif
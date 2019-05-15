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
using System.Collections.Generic;
using DataArt.Atlas.Infrastructure.Exceptions;
using DataArt.Atlas.Infrastructure.OData;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DataArt.Atlas.Core.Application.Http.OData
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SupportODataAttribute : ActionFilterAttribute
    {
        private const int TopParameterMaxValueRestriction = 100;

        public bool RestrictTopParameterValue { get; set; } = true;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (RestrictTopParameterValue)
            {
                ValidateTopParameter(context.HttpContext.Request.GetODataParams());
            }

            base.OnActionExecuting(context);
        }

        private static void ValidateTopParameter(IDictionary<string, string> queryParams)
        {
            const string TopParamName = "$top";

            if (!queryParams.ContainsKey(TopParamName))
            {
                throw new ApiValidationException($"Invalid query: {TopParamName} must be specified");
            }

            if (!int.TryParse(queryParams[TopParamName], out var topValue) || topValue > TopParameterMaxValueRestriction)
            {
                throw new ApiValidationException($"Invalid query: {TopParamName} can not be more than {TopParameterMaxValueRestriction}");
            }
        }
    }
}
#endif
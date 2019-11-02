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
using System.Linq;
using DataArt.Atlas.Core.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace DataArt.Atlas.Core.Application.Http
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                throw new ApiValidationException(ErrorMessage(actionContext.ModelState));
            }
        }

        private static string ErrorMessage(ModelStateDictionary state)
        {
            const string separator = ", ";

            return string.Join(separator, state.Values.SelectMany(x => x.Errors).Select(FilteredMessages));
        }

        // we don't want to show parsing errors due to security reasons
        private static string FilteredMessages(ModelError x)
        {
            if (string.IsNullOrEmpty(x.ErrorMessage))
            {
                return x.Exception is JsonException ? "The request is invalid" : x.Exception?.Message;
            }
            else
            {
                return x.ErrorMessage;
            }
        }
    }
}
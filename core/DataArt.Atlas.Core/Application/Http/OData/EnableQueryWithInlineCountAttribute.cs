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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using DataArt.Atlas.Infrastructure.Exceptions;
using DataArt.Atlas.Infrastructure.OData;

namespace DataArt.Atlas.Core.Application.Http.OData
{
    public class EnableQueryWithInlineCountAttribute : EnableQueryAttribute
    {
        private static readonly MethodInfo EnumerableToArrayMethod = typeof(System.Linq.Enumerable).GetMethod(nameof(System.Linq.Enumerable.ToArray));

        private static readonly string ODataResponseItemsPropertyName = nameof(ODataResponse<object>.Items);
        private static readonly string ODataResponseCountPropertyName = nameof(ODataResponse<object>.Count);

        public override void OnActionExecuting(HttpActionContext context)
        {
            var queryParams = context.Request.GetODataParams();

            ValidateTopParameter(queryParams);
            ValidateSkipParameter(queryParams);

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            if (!ResponseIsValid(context.Response))
            {
                return;
            }

            if (TryGetInlineCount(context.Request, out var count) && context.Response.TryGetContentValue(out object responseObject))
            {
                var dataType = responseObject.GetType().GenericTypeArguments[0];
                var dataArray = EnumerableToArrayMethod.MakeGenericMethod(dataType).Invoke(null, new[] { responseObject });
                var odataResponseObject = CreateOdataResponse(dataType, dataArray, (int)count);
                context.Response = context.Request.CreateResponse(HttpStatusCode.OK, odataResponseObject);
            }
        }

        private static bool ResponseIsValid(HttpResponseMessage response)
        {
            return response != null && response.IsSuccessStatusCode && response.Content is ObjectContent;
        }

        private static bool TryGetInlineCount(HttpRequestMessage request, out long inlineCount)
        {
            inlineCount = 0;

            const string inlineCountParamName = "$inlinecount";
            const string inlineCountParamValue = "allpages";

            var queryParams = request.GetODataParams();

            if (!queryParams.ContainsKey(inlineCountParamName) || queryParams[inlineCountParamName] != inlineCountParamValue)
            {
                return false;
            }

            const string inlineCountPropertyKey = "MS_InlineCount";

            if (!request.Properties.ContainsKey(inlineCountPropertyKey))
            {
                return false;
            }

            inlineCount = (long)request.Properties[inlineCountPropertyKey];

            return true;
        }

        private static void ValidateTopParameter(IDictionary<string, string> queryParams)
        {
            const string paramName = "$top";

            if (!queryParams.ContainsKey(paramName))
            {
                return;
            }

            if (!int.TryParse(queryParams[paramName], out var value) || value <= 0)
            {
                throw new ApiValidationException($"Invalid query: {paramName} must be positive");
            }
        }

        private static void ValidateSkipParameter(IDictionary<string, string> queryParams)
        {
            const string paramName = "$skip";

            if (!queryParams.ContainsKey(paramName))
            {
                return;
            }

            if (!int.TryParse(queryParams[paramName], out var value) || value < 0)
            {
                throw new ApiValidationException($"Invalid query: {paramName} must not be negative");
            }
        }

        private object CreateOdataResponse(Type dataType, object dataArray, int count)
        {
            var responseType = typeof(ODataResponse<>).MakeGenericType(dataType);
            var response = Activator.CreateInstance(responseType);

            // ReSharper disable once PossibleNullReferenceException
            responseType.GetProperty(ODataResponseItemsPropertyName).SetValue(response, dataArray);

            // ReSharper disable once PossibleNullReferenceException
            responseType.GetProperty(ODataResponseCountPropertyName).SetValue(response, count);

            return response;
        }
    }
}
#endif
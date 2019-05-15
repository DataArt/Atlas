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
using System.Linq;
using System.Reflection;
using DataArt.Atlas.Infrastructure.Exceptions;
using DataArt.Atlas.Infrastructure.OData;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DataArt.Atlas.Core.Application.Http.OData
{
    public class EnableQueryWithInlineCountAttribute : EnableQueryAttribute
    {
        internal const int TopParamMaxValue = 100;
        private const string InlineCountParamName = "$count";
        private const string InlineCountParamValue = "true";
        private const string TopParamName = "$top";
        private const int TopParamMinValue = 1;
        private const string SkipParamName = "$skip";

        private static readonly MethodInfo EnumerableToArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));
        private static readonly string ODataResponseItemsPropertyName = nameof(ODataResponse<object>.Items);
        private static readonly string ODataResponseCountPropertyName = nameof(ODataResponse<object>.Count);

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var queryParams = actionContext.HttpContext.Request.GetODataParams().ToDictionary(k => k.Key.ToLower(), v => v.Value.ToLower());

            if (!queryParams.ContainsKey(TopParamName))
            {
                throw new ApiValidationException($"Invalid query: {TopParamName} must be specified");
            }

            if (!int.TryParse(queryParams[TopParamName], out var topValue) || topValue > TopParamMaxValue || topValue < TopParamMinValue)
            {
                throw new ApiValidationException($"Invalid query: {TopParamName} must be in range {TopParamMinValue}-{TopParamMaxValue}");
            }

            if (queryParams.ContainsKey(SkipParamName) && (!int.TryParse(queryParams[SkipParamName], out var skipValue) || skipValue < 0))
            {
                throw new ApiValidationException($"Invalid query: {SkipParamName} must be positive");
            }

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            var queryParams = actionExecutedContext.HttpContext.Request.GetODataParams()
                .ToDictionary(k => k.Key.ToLower(), v => v.Value.ToLower());
            var feature = actionExecutedContext.HttpContext.Request.ODataFeature();

            if (ResponseIsValid(actionExecutedContext.HttpContext.Response)
                && queryParams.ContainsKey(InlineCountParamName)
                && queryParams[InlineCountParamName]
                    .Equals(InlineCountParamValue, StringComparison.CurrentCultureIgnoreCase)
                && feature != null)
            {
#pragma warning disable SA1119 // Statement must not use unnecessary parenthesis
                if (!(actionExecutedContext.Result is ObjectResult obj))
#pragma warning restore SA1119 // Statement must not use unnecessary parenthesis
                {
                    return;
                }

                var dataType = obj.Value.GetType().GenericTypeArguments[0];
                var dataArray = EnumerableToArrayMethod.MakeGenericMethod(dataType).Invoke(null, new[] { obj.Value });

                var odataResponseObject = CreateOdataResponse(
                    dataType,
                    dataArray,
                    (int)(feature.TotalCount ?? 0));

                actionExecutedContext.Result = new ObjectResult(odataResponseObject);
            }
        }

        private static bool ResponseIsValid(HttpResponse response)
        {
            // todo: check response content not null (Body.Length != 0?)
            return response != null && response.IsSuccessStatusCode();
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
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using DataArt.Atlas.Infrastructure.Exceptions;
#if NET452
using System.Net.Http;
using HttpRequestMessage = System.Net.Http.HttpRequestMessage;
#endif

#if NETSTANDARD2_0
using HttpRequestMessage = Microsoft.AspNetCore.Http.HttpRequest;
#endif

namespace DataArt.Atlas.Infrastructure.OData
{
    public static class ODataExtensions
    {
        private const string FilterParamName = "$filter";

#if NET452
        private const string InlineCountParamName = "$inlinecount";
        private const string InlineCountParamValue = "allpages";
#endif

#if NETSTANDARD2_0
        private const string InlineCountParamName = "$count";
        private const string InlineCountParamValue = "true";
#endif

        public static IEnumerable<KeyValuePair<string, string>> MapODataFilterEnum<TSourceEnum, TDestinationEnum>(this IEnumerable<KeyValuePair<string, string>> oDataQuery, string clientFieldName)
            where TSourceEnum : struct
            where TDestinationEnum : struct
        {
            var query = oDataQuery.ToList();

            var filter = query.SingleOrDefault(kvp => kvp.Key.Equals(FilterParamName, StringComparison.InvariantCultureIgnoreCase));

            if (filter.Equals(default(KeyValuePair<string, string>)))
            {
                return query;
            }

            var regex = new Regex(clientFieldName + " eq ([\\w']*)");

            var filterValue = regex.Replace(filter.Value, match =>
            {
                var sourceEnum = (TSourceEnum)Enum.Parse(typeof(TSourceEnum), match.Groups[1].Value.Trim('\''), true);
                var destinationEnums = Mapper.Map<TDestinationEnum[]>(sourceEnum);
                return string.Join(" or ", destinationEnums.Select(d => clientFieldName + " eq '" + d + "'"));
            });

            query.Remove(filter);
            query.Add(new KeyValuePair<string, string>(filter.Key, filterValue));

            return query;
        }

        public static Dictionary<string, string> ToODataQuery(this IEnumerable<KeyValuePair<string, string>> oDataQuery, Dictionary<string, string> map = null)
        {
            if (map == null)
            {
                return oDataQuery.ToDictionary(e => e.Key, e => e.Value);
            }

            var result = new Dictionary<string, string>();

            foreach (var pair in oDataQuery)
            {
                if (pair.Key == "$orderby" || pair.Key == FilterParamName)
                {
                    var currentValue = new StringBuilder(pair.Value);

                    foreach (var mapRule in map)
                    {
                        currentValue.Replace(mapRule.Key, mapRule.Value);
                    }

                    result[pair.Key] = currentValue.ToString();
                }
                else
                {
                    result[pair.Key] = pair.Value;
                }
            }

            return result;
        }

#if NET452
        public static Dictionary<string, string> GetODataParams(this HttpRequestMessage request)
        {
            return request.GetQueryNameValuePairs().Where(kvp => kvp.Key.StartsWith("$")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
#endif

#if NETSTANDARD2_0
        public static Dictionary<string, string> GetODataParams(this HttpRequestMessage request)
        {
            return request.Query.Where(kvp => kvp.Key.StartsWith("$")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
        }
#endif

        public static IDictionary<string, string> AppendInlineCount(this IDictionary<string, string> oDataQuery)
        {
            if (oDataQuery.ContainsKey(InlineCountParamName))
            {
                if (!oDataQuery[InlineCountParamName].Equals(InlineCountParamValue, StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ApiValidationException($"Invalid query parameter {InlineCountParamName}");
                }
            }
            else
            {
                oDataQuery.Add(InlineCountParamName, InlineCountParamValue);
            }

            return oDataQuery;
        }

        public static IDictionary<string, string> SetSorting(this IDictionary<string, string> oDataQuery, string fieldName, bool isAscending = true)
        {
            const string orderByParamName = "$orderby";
            var order = isAscending ? "asc" : "desc";

            oDataQuery[orderByParamName] = $"{fieldName} {order}";

            return oDataQuery;
        }
    }
}

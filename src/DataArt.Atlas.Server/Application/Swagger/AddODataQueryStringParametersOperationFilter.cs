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

#if NETSTANDARD2_0
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataArt.Atlas.Core.Application.Http.OData;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataArt.Atlas.Core.Application.Swagger
{
    internal class AddODataQueryStringParametersOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            context.ApiDescription.TryGetMethodInfo(out MethodInfo info);
            var customAttr = info.GetCustomAttributes().ToList();

            var isOdataAction = customAttr.OfType<EnableQueryWithInlineCountAttribute>().Any() ||
                                customAttr.OfType<SupportODataAttribute>().Any();

            // todo
            operation.Produces.Clear();
            operation.Produces.Add("application/json");

            if (isOdataAction)
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<IParameter>();
                }

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$top",
                    In = "query",
                    Description = "Top",
                    Type = "integer",
                    Minimum = 1,
                    Maximum = 100,
                    Default = 20,
                    Required = true
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$skip",
                    In = "query",
                    Description = "Skip",
                    Type = "integer"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$filter",
                    In = "query",
                    Description = "Filter",
                    Type = "string"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "$orderby",
                    In = "query",
                    Description = "Order By",
                    Type = "string"
                });
            }
        }
    }
}
#endif
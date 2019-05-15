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
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using DataArt.Atlas.Core.Application.Http.OData;
using Swashbuckle.Swagger;

namespace DataArt.Atlas.Core.Application.Swagger
{
    internal class AddODataQueryStringParametersOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var isOdataAction = apiDescription.GetControllerAndActionAttributes<EnableQueryWithInlineCountAttribute>().Any()
                                || apiDescription.GetControllerAndActionAttributes<SupportODataAttribute>().Any();

            if (isOdataAction)
            {
                if (operation.parameters == null)
                {
                    operation.parameters = new List<Parameter>();
                }

                operation.parameters.Add(new Parameter
                {
                    name = "$top",
                    @in = "query",
                    description = "Top",
                    type = "integer",
                    minimum = 1,
                    maximum = 100,
                    @default = 20,
                    required = true
                });

                operation.parameters.Add(new Parameter
                {
                    name = "$skip",
                    @in = "query",
                    description = "Skip",
                    type = "integer"
                });

                operation.parameters.Add(new Parameter
                {
                    name = "$filter",
                    @in = "query",
                    description = "Filter",
                    type = "string"
                });

                operation.parameters.Add(new Parameter
                {
                    name = "$orderby",
                    @in = "query",
                    description = "Order By",
                    type = "string"
                });
            }
        }
    }
}
#endif
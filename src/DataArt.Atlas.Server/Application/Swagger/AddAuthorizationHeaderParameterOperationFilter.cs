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
using DataArt.Atlas.Core.Application.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataArt.Atlas.Core.Application.Swagger
{
    internal class AddAuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            context.ApiDescription.TryGetMethodInfo(out MethodInfo info);
            var shouldBeAuthorized = !info.GetCustomAttributes().OfType<AuthorizeDisabledAttribute>().Any();

            if (shouldBeAuthorized)
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<IParameter>();
                }

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "Authorization",
                    In = "header",
                    Description = "Access Token",
                    Required = true,
                    Type = "string"
                });
            }
        }
    }
}
#endif
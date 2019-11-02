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
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataArt.Atlas.Core.Application.Swagger
{
    public static class SwaggerExtensions
    {
        // todo: figure out how to deal with X-Original-Host
        /*
        public static string OriginalHostName(HttpRequestMessage req)
        {
            var defaultUrl = req.RequestUri.Scheme + "://" + req.RequestUri.Authority;

            if (req.Headers.Contains("X-Forwarded-For"))
            {
                // we are behind a reverse proxy, use the host that was used by the client
                var xForwardedHost = req.Headers.GetValues("X-Original-Host").FirstOrDefault();
                if (xForwardedHost == null)
                {
                    return defaultUrl;
                }

                var firstForwardedHost = xForwardedHost.Split(',')[0];

                Log.Debug("Swagger request was forwarded from: {firstForwardedHost}, {xForwardedHost}",
                    firstForwardedHost, xForwardedHost);

                // if we under proxy => https scheme
                // ReSharper disable once InconsistentNaming
                const string XForwardedProto = "https";
                return XForwardedProto + "://" + firstForwardedHost;
            }

            return defaultUrl;
        }
        */

        public static void ConfigureSwagger(this IServiceCollection collection, Type serviceType, bool authorizationRequired)
        {
            collection.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = serviceType.Name, Version = "v1" });
                c.SwaggerAddAuth(authorizationRequired);
                c.OperationFilter<AddODataQueryStringParametersOperationFilter>();

                c.DescribeAllEnumsAsStrings();
                c.CustomSchemaIds(x => x.FullName);

                c.MapType<Guid>(() => new Schema { Type = "string" });

                var commentsFile = GetXmlCommentsPath();

                if (commentsFile != null)
                {
                    c.IncludeXmlComments(GetXmlCommentsPath());
                }
            });
        }

        public static void SwaggerAddAuth(this SwaggerGenOptions options, bool authorizationRequired)
        {
            if (authorizationRequired)
            {
                options.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();

                // todo
                // options.AddSecurityDefinition(
                //    "Bearer",
                //    new ApiKeyScheme { In = "header", Description = "Access Token", Name = "Authorization", Type = "string" });
            }
        }

        private static string GetXmlCommentsPath()
        {
            var assembly = TypeLocator.GetEntryPointAssembly();
            var xmlFile = $@"{AppDomain.CurrentDomain.BaseDirectory}\{assembly.GetName().Name}.xml";
            return File.Exists(xmlFile) ? xmlFile : null;
        }
    }
}
#endif
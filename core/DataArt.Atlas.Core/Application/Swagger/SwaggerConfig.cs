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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Serilog;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace DataArt.Atlas.Core.Application.Swagger
{
    internal sealed class SwaggerConfig
    {
        public static void Configure(HttpConfiguration configuration, Type serviceType, bool authorizationRequired)
        {
            configuration.EnableSwagger(
                "docs/{apiVersion}/swagger",
                c =>
                {
                    c.RootUrl(OriginalHostName);
                    if (authorizationRequired)
                    {
                        c.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();
                    }

                    c.OperationFilter<AddODataQueryStringParametersOperationFilter>();

                    c.SingleApiVersion("v1", serviceType.Name);
                    c.DescribeAllEnumsAsStrings();
                    c.UseFullTypeNameInSchemaIds();

                    c.MapType<Guid>(() => new Schema { type = "string" });

                    var commentsFile = GetXmlCommentsPath();

                    if (commentsFile != null)
                    {
                        c.IncludeXmlComments(GetXmlCommentsPath());
                    }
                }).EnableSwaggerUi(c => c.DisableValidator());
        }

        private static string GetXmlCommentsPath()
        {
            var assembly = TypeLocator.GetEntryPointAssembly();
            var xmlFile = $@"{AppDomain.CurrentDomain.BaseDirectory}\{assembly.GetName().Name}.xml";
            return File.Exists(xmlFile) ? xmlFile : null;
        }

        private static string OriginalHostName(HttpRequestMessage req)
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

                Log.Debug("Swagger request was forwarded from: {firstForwardedHost}, {xForwardedHost}", firstForwardedHost, xForwardedHost);

                // if we under proxy => https scheme
                // ReSharper disable once InconsistentNaming
                const string XForwardedProto = "https";
                return XForwardedProto + "://" + firstForwardedHost;
            }

            return defaultUrl;
        }
    }
}
#endif
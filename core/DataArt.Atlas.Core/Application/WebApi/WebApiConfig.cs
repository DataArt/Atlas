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
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Validation;
using Autofac;
using Autofac.Integration.WebApi;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Core.Application.Http;
using DataArt.Atlas.Core.Application.Logging;
using Newtonsoft.Json;
using Owin;

namespace DataArt.Atlas.Core.Application.WebApi
{
    public sealed class WebApiConfig
    {
        public static HttpConfiguration BaseConfiguration(IContainer container, HostingSettings hostingSettings)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            var configuration = new HttpConfiguration
            {
                DependencyResolver = new AutofacWebApiDependencyResolver(container)
            };

            configuration.Filters.Add(new ApiAccessLoggingFilter());
            configuration.Filters.Add(new ApiExceptionHandlingFilter(hostingSettings.ShowErrorDetails, hostingSettings.IsGateway));

            configuration.MapHttpAttributeRoutes();

            configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            if (hostingSettings.AuthenticationRequired)
            {
                configuration.Routes.IgnoreRoute(
                    routeName: "Favicon",
                    routeTemplate: "favicon.ico");
                configuration.Routes.IgnoreRoute(
                    routeName: "Png",
                    routeTemplate: "*.png"); // logo
                object constraints = hostingSettings.SwaggerEnabled ? new { url = "^((?!swagger).)*$" } : null;
                configuration.Routes.MapHttpRoute(
                        name: "NotFound",
                        routeTemplate: "{*url}",
                        defaults: new { controller = "Errors", action = "Handle404" },
                        constraints: constraints);
            }

            configuration.Services.Add(typeof(IExceptionLogger), new ApiUnhandledExceptionLogger());
            configuration.Services.Replace(typeof(IHttpControllerSelector), new HttpNotFoundAwareDefaultHttpControllerSelector(configuration));
            configuration.Services.Replace(typeof(IHttpActionSelector), new HttpNotFoundAwareControllerActionSelector());
            configuration.Services.Clear(typeof(ModelValidatorProvider));

            configuration.Formatters.Clear();
            var settings = container.Resolve<JsonSerializerSettings>();
            var formatter = new JsonMediaTypeFormatter { SerializerSettings = settings };
            configuration.Formatters.Add(formatter);

            configuration.EnsureInitialized();
            return configuration;
        }

        public static HttpConfiguration Configure(IAppBuilder builder, IContainer container, HostingSettings hostingSettings)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            var configuration = BaseConfiguration(container, hostingSettings);

            if (hostingSettings.HttpsRequired)
            {
                configuration.Filters.Add(new RequireHttpsAttribute());
                configuration.Filters.Add(new HSTSAttribute());
            }

            configuration.EnsureInitialized();
            builder.UseWebApi(configuration);

            return configuration;
        }
    }
}
#endif
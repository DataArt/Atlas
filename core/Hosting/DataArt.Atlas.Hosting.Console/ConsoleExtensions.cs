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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Hosting.Console
{
    public static class ConsoleExtensions
    {
        public static void StartInConsole<TStartup>(this IWebHostBuilder builder, string serviceName = null, string[] urls = null)
            where TStartup : class
        {
            builder
                .UseKestrel()
                .UseStartup<TStartup>();

            if (urls != null)
            {
                builder.UseUrls(urls);
            }

            builder.ConfigureServices(collection => collection.UseConsole(serviceName, urls));

            var host = builder.Build();
            host.Run();
        }

        public static void StartInConsole<TStartup>(this IWebHostBuilder builder, string serviceName = null, string url = null)
            where TStartup : class
        {
            builder.StartInConsole<TStartup>(serviceName, !string.IsNullOrEmpty(url) ? new[] { url } : null);
        }

        public static void UseConsole(this IServiceCollection services, string serviceName = null, string[] urls = null)
        {
            var urlTitle = string.Empty;
            if (urls != null)
            {
                urlTitle = urls.Aggregate(urlTitle, (current, url) => current + GetConsoleTitle(url));
            }

            System.Console.Title = $"{serviceName}{urlTitle}";

            var consoleApplication = new ConsoleApplication();
            services.AddSingleton<IApplication>(consoleApplication);
        }

        public static void UseConsole(this IServiceCollection services, string serviceName = null, string url = null)
        {
            services.UseConsole(serviceName, !string.IsNullOrEmpty(url) ? new[] { url } : null);
        }

        private static string GetConsoleTitle(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            // most probably we receive not full url but http://+:<port>
            url = url.Replace("+", "localhost");
            var uriBuilder = new UriBuilder(url);
            return $"@{uriBuilder.Port} ({uriBuilder.Scheme})";
        }
    }
}
#endif
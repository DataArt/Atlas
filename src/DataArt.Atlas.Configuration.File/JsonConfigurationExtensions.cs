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
// </copyright>
//--------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataArt.Atlas.Configuration.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataArt.Atlas.Configuration.File
{
    /// <summary>
    ///     Json base configuration extensions.
    /// </summary>
    public static class JsonConfigurationExtensions
    {
        public static void UseJsonConfiguration(this IServiceCollection services, string basePath, string[] args = null, params Tuple<string, bool>[] fileOpts)
        {
            var builder = new ConfigurationBuilder().SetBasePath(basePath);
            foreach (var fileOpt in fileOpts)
            {
                builder.AddJsonFile(fileOpt.Item1, optional: fileOpt.Item2, reloadOnChange: true);
            }

            builder.AddEnvironmentVariables();
            builder.AddCommandLine(args ?? new string[0]);

            var configuration = builder.Build();
            services.AddSingleton<IConfiguration>(configuration);
        }

        public static void UseJsonConfiguration(this IServiceCollection services, string basePath, string[] args = null, params string[] fileNames)
        {
            UseJsonConfiguration(services, basePath, args, fileNames.Select(x => new Tuple<string, bool>(x, true)).ToArray());
        }

        public static void UseJsonConfiguration(this IServiceCollection services, IHostingEnvironment env, string[] args = null, params string[] fileNames)
        {
            var options = new List<Tuple<string, bool>>(fileNames.Select(x => new Tuple<string, bool>(x, false)));
            options.AddRange(fileNames.Select(fileName =>
                new Tuple<string, bool>(
                    $"{Path.GetFileNameWithoutExtension(fileName)}.{env.EnvironmentName}{Path.GetExtension(fileName)}",
                    true)));

            UseJsonConfiguration(services, basePath: env.ContentRootPath, args: args, fileOpts: options.ToArray());
        }

        public static EndpointSetting GetJsonEndpointSetting(this IConfigurationBuilder builder, string basePath, string fileName)
        {
            var config = builder
                .SetBasePath(basePath)
                .AddJsonFile(fileName, optional: false, reloadOnChange: false)
                .Build();

            var endpoint = new EndpointSetting();
            config.GetSection(typeof(EndpointSetting).Name).Bind(endpoint);
            return endpoint;
        }

        /// <summary>
        ///     Add common and environment-specific files to builder.
        /// </summary>
        /// <param name="builder">Configuration builder.</param>
        /// <returns>Appended configuration builder.</returns>
        public static IConfigurationBuilder AddJsonConfigFiles(this IConfigurationBuilder builder)
        {
            const string defaultEnvName = "Development";
            var envName = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? defaultEnvName;

            builder.AddJsonFile("appsettings.json", false)
                   .AddJsonFile($"appsettings.{envName}.json", true)
                   .AddJsonFile($"appsettings.{envName}.local.json", true);

            return builder;
        }
    }
}

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataArt.Atlas.Configuration.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Configuration.File
{
    public static class XmlConfigurationExtensions
    {
        public static void UseXmlConfiguration(this IServiceCollection services, string basePath, string[] args = null, params Tuple<string, bool>[] fileOpts)
        {
            var builder = new ConfigurationBuilder().SetBasePath(basePath);
            foreach (var fileOpt in fileOpts)
            {
                builder.AddXmlFile(fileOpt.Item1, optional: fileOpt.Item2, reloadOnChange: true);
            }

            builder.AddEnvironmentVariables();
            builder.AddCommandLine(args ?? new string[0]);

            var configuration = builder.Build();
            services.AddSingleton<IConfiguration>(configuration);
        }

        public static void UseXmlConfiguration(this IServiceCollection services, string basePath, string[] args = null, params string[] fileNames)
        {
            UseXmlConfiguration(services, basePath, args, fileNames.Select(x => new Tuple<string, bool>(x, true)).ToArray());
        }

        public static void UseXmlConfiguration(this IServiceCollection services, IHostingEnvironment env, string[] args = null, params string[] fileNames)
        {
            var options = new List<Tuple<string, bool>>(fileNames.Select(x => new Tuple<string, bool>(x, false)));
            options.AddRange(fileNames.Select(fileName =>
                new Tuple<string, bool>(
                    $"{Path.GetFileNameWithoutExtension(fileName)}.{env.EnvironmentName}{Path.GetExtension(fileName)}",
                    true)));

            UseXmlConfiguration(services, basePath: env.ContentRootPath, args: args, fileOpts: options.ToArray());
        }

        public static EndpointSetting GetXmlEndpointSetting(this IConfigurationBuilder builder, string basePath, string fileName)
        {
            var config = builder
                .SetBasePath(basePath)
                .AddXmlFile(fileName, optional: false, reloadOnChange: false)
                .Build();

            var endpoint = new EndpointSetting();
            config.GetSection(typeof(EndpointSetting).Name).Bind(endpoint);
            return endpoint;
        }
    }
}
#endif
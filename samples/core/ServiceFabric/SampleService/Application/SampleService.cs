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
using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Infrastructure.Logging;
using DataArt.Atlas.Logging;
using DataArt.Atlas.ServiceDiscovery.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SampleService.Settings;
using Serilog;

namespace SampleService.Application
{
    internal class SampleService : Startup
    {
        // example field to demonstrate "startup time" injection
        private readonly CustomStartupInfo info;
        public static readonly string Key = "SampleService";

        protected override string ServiceKey => Key;

        public SampleService(IHostingEnvironment env, IConfiguration configuration, IApplication application, CustomStartupInfo info) : base(env, configuration, application)
        {
            this.info = info;
        }

        protected override void ConfigureContainer(IServiceCollection services)
        {
            var cfg = new LoggerConfiguration();
            cfg.Enrich.With(new ServiceNameEnricher(Key));
            services.UseSeriLog(cfg);

            services.RegisterConfiguration<SampleSettings>(Configuration);
        }

        protected override void StartApplication()
        {
            base.StartApplication();

            var logger = AtlasLogging.CreateLogger<SampleService>();
            logger.LogDebug("{info}", info.Info);
        }
    }
}

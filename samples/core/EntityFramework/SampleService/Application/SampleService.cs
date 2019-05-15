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
using DataArt.Atlas.EntityFramework.MsSql.EntityFramework;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleService.DataAccess;
using Serilog;

namespace SampleService.Application
{
    internal class SampleService : Startup
    {
        public static readonly string Key = "SampleService";

        protected override string ServiceKey => Key;

        public SampleService(IHostingEnvironment env, IConfiguration configuration, IApplication application) : base(env, configuration, application)
        {
        }

        protected override void ConfigureContainer(IServiceCollection services)
        {
            var cfg = new LoggerConfiguration();
            cfg.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({CorrelationId}) {Message}{NewLine}{Exception}");
            services.UseSeriLog(cfg);

            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<SampleEntityContext>(options =>
            {
                options.UseSqlServer(ApplicationSettings.Persistence.ConnectionString);
            });

            services.AddTransient<SampleEntityRepository>();
        }

        protected override void StartApplication()
        {
            Container.InitializeContext<SampleEntityContext>();
        }
    }
}

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
using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Service.Scheduler.Sdk;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DataArt.Atlas.Service.Scheduler.Application
{
    public class SchedulerService : Startup
    {
        protected override string ServiceKey => SchedulerClientConfig.ServiceKey;

        public SchedulerService(IHostingEnvironment env, IConfiguration configuration, IApplication application)
            : base(env, configuration, application)
        {
        }

        protected override void ConfigureContainer(IServiceCollection services)
        {
            services.UseQuartz();
        }

        protected override void StartApplication()
        {
            var scheduler = Container.GetService<IScheduler>();
            scheduler.Start().GetAwaiter().GetResult();
        }

        protected override void StopApplication()
        {
            var scheduler = Container.GetService<IScheduler>();
            scheduler.Shutdown().GetAwaiter().GetResult();
        }
    }
}
#endif
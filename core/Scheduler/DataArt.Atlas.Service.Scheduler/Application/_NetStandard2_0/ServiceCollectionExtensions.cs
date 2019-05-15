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
using System.Collections.Specialized;
using DataArt.Atlas.Infrastructure;
using DataArt.Atlas.Infrastructure.Startup;
using DataArt.Atlas.Service.Scheduler.HealthCheck;
using DataArt.Atlas.Service.Scheduler.Jobs;
using DataArt.Atlas.Service.Scheduler.Scheduler;
using DataArt.Atlas.Service.Scheduler.Settings;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Impl;
using Quartz.Spi;

[assembly: ExplicitTypeUsage(typeof(Quartz.Simpl.JsonObjectSerializer))]
namespace DataArt.Atlas.Service.Scheduler.Application
{
    internal static class ServiceCollectionExtensions
    {
        public static void UseQuartz(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, QuartzFactory>();
            services.AddSingleton(provider =>
            {
                var quartzSettings = provider.GetService<QuartzSettings>();
                var properties = new NameValueCollection
                {
                    { "quartz.threadPool.threadCount", quartzSettings.ThreadPoolThreadCount },
                    { "quartz.scheduler.instanceName", quartzSettings.SchedulerInstanceName },
                    { "quartz.jobStore.type", quartzSettings.JobStoreType },
                    { "quartz.jobStore.useProperties", quartzSettings.JobStoreUseProperties },
                    { "quartz.jobStore.dataSource", quartzSettings.JobStoreDataSource },
                    { "quartz.jobStore.tablePrefix", quartzSettings.JobStoreTablePrefix },
                    { "quartz.jobStore.driverDelegateType", quartzSettings.JobStoreDriverDelegateType },
                    { "quartz.dataSource.default.connectionString", quartzSettings.DataSourceConnectionString },
                    { "quartz.dataSource.default.provider", quartzSettings.DataSourceProvider },
                    { "quartz.serializer.type", quartzSettings.SerializerType }
                };

                var schedulerFactory = new StdSchedulerFactory(properties);
                var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
                scheduler.JobFactory = provider.GetService<IJobFactory>();
                return scheduler;
            });

            services.AddTransient<IAsyncScheduler, AsyncScheduler>();
            services.AddSingleton<Startable, TriggersStateCheck>();

            services.AddSingleton<Scheduler.SchedulerService, Scheduler.SchedulerService>();
            services.AddSingleton<ISchedulerService>(x => x.GetService<Scheduler.SchedulerService>());
            services.AddSingleton<Startable>(x => x.GetService<Scheduler.SchedulerService>());

            services.AddSingleton<SchedulerSettings>();

            services.AddTransient<WebRequestJob>();
        }
    }
}
#endif
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
using System.Collections.Specialized;
using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using Autofac.Integration.WebApi;
using DataArt.Atlas.Infrastructure.Startup;
using DataArt.Atlas.Service.Scheduler.HealthCheck;
using DataArt.Atlas.Service.Scheduler.Scheduler;
using DataArt.Atlas.Service.Scheduler.Settings;

namespace DataArt.Atlas.Service.Scheduler.Application
{
    internal sealed class AutofacConfig
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterModule(new QuartzAutofacFactoryModule
            {
                ConfigurationProvider = c =>
                {
                    var quartzSettings = c.Resolve<QuartzSettings>();
                    return new NameValueCollection
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
                }
            });

            builder.RegisterModule(new QuartzAutofacJobsModule(Assembly.GetExecutingAssembly()));

            builder.RegisterType<AsyncScheduler>().As<IAsyncScheduler>();
            builder.RegisterType<TriggersStateCheck>().As<Startable>().SingleInstance();
            builder.RegisterType<Scheduler.SchedulerService>().As<ISchedulerService>().As<Startable>().SingleInstance();
            builder.RegisterType<SchedulerSettings>().AsSelf().IfNotRegistered(typeof(SchedulerSettings)).SingleInstance();
        }
    }
}
#endif
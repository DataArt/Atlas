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
using DataArt.Atlas.Infrastructure.Container;
using DataArt.Atlas.Infrastructure.Startup;
using DataArt.Atlas.Service.Scheduler.Sdk.JobRegistration;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Service.Scheduler.Sdk
{
    public class ContainerModule : SdkModule
    {
        protected override void Load(IServiceCollection services)
        {
            base.Load(services);

            services.AddSingleton<Startable, DeprecatedJobsInvalidator>();

            // todo: startup may be a bit slow after searhing for registrars in domain assemblies
            var jobTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(t => typeof(IJob).IsAssignableFrom(t) && !t.IsAbstract)).ToList();

            foreach (var jobType in jobTypes)
            {
                services.Add(new ServiceDescriptor(jobType, jobType, ServiceLifetime.Singleton));
                services.AddSingleton(x => (IJob)x.GetService(jobType));
                services.AddSingleton<Startable>(x => new JobRegistrar(x.GetService<ISchedulerClient>(), (IJob)x.GetService(jobType)));
            }
        }
    }
}
#endif
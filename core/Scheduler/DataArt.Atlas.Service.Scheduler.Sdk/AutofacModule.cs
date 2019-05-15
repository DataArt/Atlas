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
using System.Linq;
using Autofac;
using DataArt.Atlas.Infrastructure.Autofac;
using DataArt.Atlas.Infrastructure.Startup;
using DataArt.Atlas.Service.Scheduler.Sdk.JobRegistration;

namespace DataArt.Atlas.Service.Scheduler.Sdk
{
    public class AutofacModule : SdkModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<DeprecatedJobsInvalidator>().As<Startable>().SingleInstance();

            // todo: startup may be a bit slow after searhing for registrars in domain assemblies
            var jobTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(t => typeof(IJob).IsAssignableFrom(t) && !t.IsAbstract)).ToList();

            builder.RegisterTypes(jobTypes.ToArray())
                .Named<IJob>(t => t.Name)
                .As<IJob>()
                .SingleInstance();

            jobTypes.ForEach(t =>
            {
                builder.Register(c => new JobRegistrar(c.Resolve<ISchedulerClient>(), c.ResolveNamed<IJob>(t.Name)))
                    .As<Startable>().SingleInstance();
            });
        }
    }
}
#endif
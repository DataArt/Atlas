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
//--------------------------------------------------------------------------------------------------
#if NETSTANDARD2_0

using System;
using System.Fabric;
using System.Fabric.Health;
using System.Fabric.Query;
using System.Linq;
using FabricHealthState = System.Fabric.Health.HealthState;
using HealthState = DataArt.Atlas.Hosting.HealthCheck.HealthState;

namespace DataArt.Atlas.Azure.Hosting.Fabric
{
    internal class ServiceFabricApplication : IApplication
    {
        private readonly string serviceName;
        private readonly StatelessServiceContext context;
        private readonly FabricClient fabricClient;
        private Application application;

        public Application Application
        {
            get
            {
                if (application == null)
                {
                    var applications = fabricClient.QueryManager.GetApplicationListAsync().ConfigureAwait(false)
                        .GetAwaiter().GetResult();
                    application = applications.FirstOrDefault(x =>
                        x.ApplicationName.AbsoluteUri.Contains(FabricApplication.Name));
                }

                return application;
            }
        }

        // todo: IStatelessServicePartition partition
        public ServiceFabricApplication(string serviceName, StatelessServiceContext context)
        {
            this.serviceName = serviceName;
            this.context = context;
            fabricClient = new FabricClient(FabricClientRole.Admin);
        }

        public Action<string, HealthState, string> ReportHealthStateAction => (property, state, description) =>
        {
            ReportInstanceHealth(new HealthInformation(serviceName, property, (FabricHealthState)state)
                {
                    Description = description
                });
        };

        public Action<string, HealthState, TimeSpan, string> ReportRecurrentHealthStateAction => (property, state, timeToLive, description) =>
        {
            ReportInstanceHealth(new HealthInformation($"{serviceName}_Recurrent", property, (FabricHealthState)state)
                {
                    Description = description,
                    TimeToLive = timeToLive,
                    RemoveWhenExpired = false
                });
        };

        public Func<string> GetApplicationVersionFunction =>
            () =>
            {
                return GetApplicationVersion();
            };

        public Func<string> GetServiceVersionFunction =>
            () => context.CodePackageActivationContext.GetServiceManifestVersion();

        public Func<string> GetCodePackageVersionFunction =>
            () => context.CodePackageActivationContext.CodePackageVersion;

        public Func<string, string> GetDataPackageVersionFunction =>
            packageName => context.CodePackageActivationContext.GetDataPackageObject(packageName)?.Description?.Version;

        public Func<string, string> GetServiceResourcePathFunction =>
            resourceName => context.CodePackageActivationContext.GetDataPackageObject(resourceName).Path;

        private string GetApplicationVersion()
        {
            return Application == null ? string.Empty : Application.ApplicationTypeVersion;
        }

        private void ReportInstanceHealth(HealthInformation healthInformation)
        {
            fabricClient.HealthManager.ReportHealth(
                new StatelessServiceInstanceHealthReport(context.PartitionId, context.InstanceId, healthInformation),
                new HealthReportSendOptions { Immediate = true });
        }
    }
}
#endif
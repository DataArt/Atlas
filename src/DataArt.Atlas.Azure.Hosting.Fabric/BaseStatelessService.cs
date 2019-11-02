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

#if NET452
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Health;
using System.Linq;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FabricHealthState = System.Fabric.Health.HealthState;

namespace DataArt.Atlas.Azure.Hosting.Fabric
{
    internal sealed class BaseStatelessService : StatelessService
    {
        private readonly StatelessServiceContext context;
        private IApplication service;

        public BaseStatelessService(StatelessServiceContext context)
            : base(context)
        {
            this.context = context;
        }

        public void Initialize(IApplication appInstance)
        {
            FabricApplication.Name = context.CodePackageActivationContext.ApplicationName;
            appInstance.ReportHealthStateAction = (property, state, description) =>
            {
                Partition.ReportInstanceHealth(
                    new HealthInformation(appInstance.GetType().Name, property, (FabricHealthState)state)
                    {
                        Description = description
                    });
            };
            appInstance.ReportRecurrentHealthStateAction = (property, state, timeToLive, description) =>
            {
                Partition.ReportInstanceHealth(
                    new HealthInformation($"{appInstance.GetType().Name}_Recurrent", property, (FabricHealthState)state)
                    {
                        Description = description,
                        TimeToLive = timeToLive,
                        RemoveWhenExpired = false
                    });
            };
            appInstance.GetServiceResourcePathFunction = resourceName => context.CodePackageActivationContext
                .GetDataPackageObject(resourceName)
                .Path;
            appInstance.GetCodePackageVersionFunction = () => context.CodePackageActivationContext.CodePackageVersion;
            appInstance.GetDataPackageVersionFunction = packageName => context.CodePackageActivationContext
                .GetDataPackageObject(packageName)?.Description?.Version;

            service = appInstance;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var endpoints = Context.CodePackageActivationContext.GetEndpoints()
                .Where(e => e.Protocol == EndpointProtocol.Http || e.Protocol == EndpointProtocol.Https).ToList();

            if (endpoints.Count == 0)
            {
                return new[] { CreateListener() };
            }

            return endpoints.Select(CreateListener).ToList();
        }

        private ServiceInstanceListener CreateListener(EndpointResourceDescription endpoint = null)
        {
            return new ServiceInstanceListener(c => new FabricOwinCommunicationListener(service, endpoint));
        }
    }
}
#endif
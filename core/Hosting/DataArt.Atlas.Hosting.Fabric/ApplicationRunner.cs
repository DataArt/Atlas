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
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;

namespace DataArt.Atlas.Hosting.Fabric
{
    public sealed class ApplicationRunner : IApplicationRunner
    {
        private const string ServiceTypePostfix = "Type";

        public void Run(IApplication appInstance)
        {
            var serviceTypeName = GetApplicationTypeName();

            ConfigureEventSource(serviceTypeName);

            try
            {
                ServiceRuntime.RegisterServiceAsync(serviceTypeName, context =>
                    {
                        var baseStatelessService = new BaseStatelessService(context);
                        baseStatelessService.Initialize(appInstance);
                        return baseStatelessService;
                    })
                    .GetAwaiter()
                    .GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, appInstance.GetType().Name);

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Unhandled exception while registering the fabric service");
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static void ConfigureEventSource(string appTypeName)
        {
            var eventSource = (EventSourceAttribute)typeof(ServiceEventSource).GetCustomAttributes(typeof(EventSourceAttribute), false).First();
            eventSource.Name = appTypeName;
        }

        // must be same as ServiceTypeName in ServiceManifest
        private static string GetApplicationTypeName()
        {
            return $"{GetFabricServiceType()}{ServiceTypePostfix}";
        }

        private static string GetFabricServiceType()
        {
            return Assembly.GetEntryAssembly().FullName.Split(',').First().Split('.').Last();
        }
    }
}
#endif
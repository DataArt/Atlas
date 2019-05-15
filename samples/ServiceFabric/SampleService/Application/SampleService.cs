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
using Autofac;
using DataArt.Atlas.Core.Shell;

namespace SampleService.Application
{
    internal class SampleService : ServiceShell
    {
        // This key should be the name of the service, specified in ApplicationManifest.xml
        // file for Service Fabric deployment.
        // This name is available in the /ApplicationManifest/DefaultServices/Service node
        public static readonly string Key = "SampleService";
        protected override string ServiceKey => Key;

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            AutofacConfig.Configure(builder);
        }
    }
}

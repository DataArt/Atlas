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
using Autofac;
using DataArt.Atlas.WebCommunication.Request;
using Flurl.Http.Configuration;

namespace DataArt.Atlas.WebCommunication.Autofac
{
    public class BaseWebCommunicationAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PerHostFlurlClientFactory>().As<IFlurlClientFactory>().SingleInstance();
            builder.RegisterType<RequestFactory>().As<IRequestFactory>().InstancePerDependency();
            builder.RegisterType<InternalRequestFactory>().As<IInternalRequestFactory>().InstancePerDependency();
            builder.RegisterType<DefaultFlurlHttpSettings>().AsSelf().SingleInstance();
        }
    }
}
#endif
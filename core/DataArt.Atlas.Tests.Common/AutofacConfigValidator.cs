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
using Autofac.Core;
using AutoMapper;
using DataArt.Atlas.Configuration;
using DataArt.Atlas.Configuration.Settings;
using DataArt.Atlas.Core.HealthCheck;
using DataArt.Atlas.Infrastructure.AutoMapper;
using DataArt.Atlas.Messaging;
using DataArt.Atlas.ServiceDiscovery;
using DataArt.Atlas.WebCommunication;
using Moq;

namespace DataArt.Atlas.Tests.Common
{
    public static class AutofacConfigValidator<T>
        where T : ApplicationSettings, new()
    {
        private static readonly T DummyApplicationSettings = ServiceSettingsStub.Create<T>();

        public static void Validate(Action<ContainerBuilder> configure)
        {
            Validate<object>(configure);
        }

        public static void Validate<TServiceSettings>(Action<ContainerBuilder> configure)
            where TServiceSettings : class, new()
        {
            Effort.Provider.EffortProviderConfiguration.RegisterProvider();

            var configurationClientMock = new Mock<IConfigurationClient>();
            configurationClientMock.Setup(x => x.GetApplicationSettings()).Returns(DummyApplicationSettings);
            configurationClientMock.Setup(x => x.GetSettings<T>()).Returns(DummyApplicationSettings);

            if (typeof(TServiceSettings) != typeof(object))
            {
                configurationClientMock.Setup(c => c.GetSettings<TServiceSettings>()).Returns(ServiceSettingsStub.Create<TServiceSettings>());
            }

            var builder = CreateDefaultContainerBuilder(DummyApplicationSettings, configurationClientMock.Object);

            configure(builder);

            var container = builder.Build();

            ValidateContainer(container);

            container.Dispose();
        }

        private static ContainerBuilder CreateDefaultContainerBuilder(ApplicationSettings applicationSettings, IConfigurationClient configurationClient)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(applicationSettings).AsSelf();
            builder.RegisterInstance(configurationClient).As<IConfigurationClient>();

            builder.RegisterInstance(new Mock<IServiceDiscovery>().Object).As<IServiceDiscovery>();
            builder.RegisterInstance(new Mock<IRequestFactory>().Object).As<IRequestFactory>();
            builder.RegisterInstance(new Mock<IInternalRequestFactory>().Object).As<IInternalRequestFactory>();
            builder.RegisterInstance(new Mock<IServiceBus>().Object).As<IServiceBus>();
            builder.RegisterInstance(new Mock<IInteropServiceBus>().Object).As<IInteropServiceBus>();
            builder.RegisterInstance(new Mock<IHealthReporter>().Object).As<IHealthReporter>();
            builder.RegisterInstance(new Mock<IMapper>().Object).As<IMapper>();
            builder.RegisterInstance(new Mock<IProjectionConfigurationProvider>().Object).As<IProjectionConfigurationProvider>();

            return builder;
        }

        private static void ValidateContainer(IContainer container)
        {
            var rootAutofacNamespace = typeof(IContainer).Namespace;

            if (rootAutofacNamespace == null || rootAutofacNamespace.Contains("."))
            {
                throw new InvalidOperationException("Autofac root namespace not found.");
            }

            var services = container
                .ComponentRegistry
                .Registrations
                .SelectMany(x => x.Services)
                .OfType<TypedService>()
                .Where(x => x.ServiceType.Namespace != null && !x.ServiceType.Namespace.StartsWith(rootAutofacNamespace));

            foreach (var typedService in services)
            {
                container.Resolve(typedService.ServiceType);
            }
        }
    }
}
#endif
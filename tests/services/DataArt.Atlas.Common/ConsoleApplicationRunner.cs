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
using DataArt.Atlas.Common.Settings;
using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.Messaging.MassTransit;
using DataArt.Atlas.Messaging.RabbitMq;

namespace DataArt.Atlas.Common
{
    public sealed class ConsoleApplicationRunner<TService>
        where TService : ServiceShell, new()
    {
        public static void Run(bool useEsb = true)
        {
            var app = new Application<TService>()
                .WithRunner<Hosting.Console.ApplicationRunner>()
                .WithConfigurationClient<Configuration.File.ConfigurationClient, AtlasSettings>()
                .UseSerilogSinkConfiguration(CustomColoredConsole.SinkConfiguration)
                .WithSerilogEnrichers(new CustomLogEnricher())
                .WithServiceDiscovery<ServiceDiscovery>();

            if (useEsb)
            {
                app = app.WithServiceBus<RabbitMqServiceBusInitiator, DefaultBusType>()
                    .WithConsumerRegistration<MasstransitConsumerRegistrator>();
            }

            app.Run();
        }
    }
}

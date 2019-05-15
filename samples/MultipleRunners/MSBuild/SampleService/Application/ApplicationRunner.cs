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

using DataArt.Atlas.Core.Shell;

namespace SampleService.Application
{
    public sealed class ApplicationRunner<TService>
        where TService : ServiceShell, new()
    {
        public static void Run()
        {
            var app =
#if FABRIC
                new Application<TService>()
                    .WithRunner<DataArt.Atlas.Hosting.Fabric.ApplicationRunner>()
                    .WithConfigurationClient<DataArt.Atlas.Configuration.Fabric.ConfigurationClient>();
#else
                new Application<TService>()
                    .WithRunner<DataArt.Atlas.Hosting.Console.ApplicationRunner>()
                    .WithConfigurationClient<DataArt.Atlas.Configuration.File.ConfigurationClient>()
                    .UseSerilogSinkConfiguration(DataArt.Atlas.Hosting.Console.ColoredConsole.Sink);
#endif
            app.Run();
        }
    }

}

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
namespace DataArt.Atlas.Configuration.Settings
{
    public class ApplicationSettings
    {
        public ApplicationSettings()
        {
            Hosting = new HostingSettings();
            ServiceBus = new ServiceBusSettings();
            InteropServiceBus = new ServiceBusSettings();
            Logging = new LoggingSettings();
            Persistence = new PersistenceSettings();
        }

        public HostingSettings Hosting { get; set; }

        public ServiceBusSettings ServiceBus { get; set; }

        public ServiceBusSettings InteropServiceBus { get; set; }

        public LoggingSettings Logging { get; set; }

        public PersistenceSettings Persistence { get; set; }
    }
}

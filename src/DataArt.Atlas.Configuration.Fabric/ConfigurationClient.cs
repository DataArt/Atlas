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
using DataArt.Atlas.Configuration.Impl;

namespace DataArt.Atlas.Configuration.Fabric
{
    public sealed class ConfigurationClient : BaseConfigurationClient
    {
        private const string DefaultConfigurationPackageName = "Config";

        public override string GetConfigurationFolderPath()
        {
            var context = FabricRuntime.GetActivationContext();
            var configuration = context.GetConfigurationPackageObject(DefaultConfigurationPackageName);

            return configuration.Path;
        }

        protected override List<SettingsSection> ReadConfig()
        {
            var context = FabricRuntime.GetActivationContext();
            var configuration = context.GetConfigurationPackageObject(DefaultConfigurationPackageName);
            var fabricSettings = configuration.Settings;
            var settings = new List<SettingsSection>();
            foreach (var section in fabricSettings.Sections)
            {
                var ss = new SettingsSection(section.Name);

                foreach (var param in section.Parameters)
                {
                    var value = param.IsEncrypted && !string.IsNullOrEmpty(param.Value) ? param.DecryptValue().ConvertToUnsecureString() : param.Value;
                    ss.Settings.Add(new Setting(param.Name, value));
                }

                settings.Add(ss);
            }

            return settings;
        }
    }
}
#endif
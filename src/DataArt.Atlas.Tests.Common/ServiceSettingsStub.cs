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
using System;
using DataArt.Atlas.Azure.FileStorage;
using DataArt.Atlas.Azure.FileStorage.Models;

namespace DataArt.Atlas.Tests.Common
{
    public static class ServiceSettingsStub
    {
        public static TServiceSettings Create<TServiceSettings>()
            where TServiceSettings : class, new()
        {
            var settings = Activator.CreateInstance<TServiceSettings>();

            foreach (var settingsProperty in typeof(TServiceSettings).GetProperties())
            {
                var settingsSectionType = settingsProperty.PropertyType;

                var settingsSection = Activator.CreateInstance(settingsSectionType);

                foreach (var sectionProperty in settingsSectionType.GetProperties())
                {
                    var value = CreateDummySectionValue(settingsSectionType, sectionProperty.PropertyType, sectionProperty.Name);

                    if (value != null)
                    {
                        sectionProperty.SetValue(settingsSection, value);
                    }
                }

                settingsProperty.SetValue(settings, settingsSection);
            }

            return settings;
        }

        private static object CreateDummySectionValue(Type sectionType, Type sectionPropertyType, string sectionPropertyName)
        {
            if (sectionType == typeof(FileStorageSettings))
            {
                if (sectionPropertyName == nameof(FileStorageSettings.ConnectionString))
                {
                    return "UseDevelopmentStorage=true";
                }
            }

            if (sectionPropertyType == typeof(string))
            {
                if (sectionPropertyName.Contains("Url"))
                {
                    return "http://localhost:80";
                }

                return sectionPropertyName;
            }

            return null;
        }
    }
}

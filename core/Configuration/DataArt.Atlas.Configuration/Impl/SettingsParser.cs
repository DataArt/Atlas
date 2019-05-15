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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DataArt.Atlas.Configuration.Impl
{
    public static class SettingsParser
    {
        public static T GetSettings<T>(IList<SettingsSection> parsedSections)
            where T : new()
        {
            var resultSettings = new T();

            foreach (var resultSettingsProperty in typeof(T).GetProperties())
            {
                var matchedSection = parsedSections.FirstOrDefault(s => s.Name.Equals(resultSettingsProperty.Name, StringComparison.OrdinalIgnoreCase));
                if (matchedSection == null)
                {
                    continue;
                }

                var innerSetting = Activator.CreateInstance(resultSettingsProperty.PropertyType);

                foreach (var innerProperty in resultSettingsProperty.PropertyType.GetProperties())
                {
                    var parsedSetting =
                        matchedSection.Settings.FirstOrDefault(s => s.Name.Equals(innerProperty.Name, StringComparison.OrdinalIgnoreCase));
                    if (parsedSetting == null)
                    {
                        continue;
                    }

                    var converter = TypeDescriptor.GetConverter(innerProperty.PropertyType);
                    if (!converter.CanConvertFrom(typeof(string)))
                    {
                        continue;
                    }

                    object value;
                    try
                    {
                        value = converter.ConvertFrom(parsedSetting.Value);
                    }
                    catch (Exception)
                    {
                        value = innerProperty.PropertyType.IsValueType ? Activator.CreateInstance(innerProperty.PropertyType) : null;
                    }

                    innerProperty.SetValue(innerSetting, value);
                }

                resultSettingsProperty.SetValue(resultSettings, innerSetting);
            }

            return resultSettings;
        }
    }
}
#endif
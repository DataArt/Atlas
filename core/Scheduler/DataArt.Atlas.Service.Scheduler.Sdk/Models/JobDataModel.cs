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
using DataArt.Atlas.Infrastructure.Helpers;
using Newtonsoft.Json;

namespace DataArt.Atlas.Service.Scheduler.Sdk.Models
{
    public sealed class JobDataModel
    {
        public static string SerializeSettings(object settings)
        {
            return JsonConvert.SerializeObject(settings);
        }

        public static T DeserializeSettings<T>(string serializedSettings)
            where T : class
        {
            return JsonConvert.DeserializeObject<T>(serializedSettings);
        }

        public string SdkVersion { get; set; }

        public string Settings { get; set; }

        public ScheduleType ScheduleType { get; set; }

        public string ScheduleSettings { get; set; }

        public string GetDataHashCode()
        {
            return StringHelpers.Sha256(JsonConvert.SerializeObject(this));
        }
    }
}

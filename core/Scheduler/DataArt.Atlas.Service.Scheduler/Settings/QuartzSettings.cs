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
using System;

namespace DataArt.Atlas.Service.Scheduler.Settings
{
    public sealed class QuartzSettings
    {
        public string SchedulerInstanceName { get; set; }

        public string ThreadPoolThreadCount { get; set; }

        public string JobStoreType { get; set; }

        public string JobStoreUseProperties { get; set; }

        public string JobStoreDataSource { get; set; }

        public string JobStoreTablePrefix { get; set; }

        public string JobStoreDriverDelegateType { get; set; }

        public string DataSourceConnectionString { get; set; }

        public string DataSourceProvider { get; set; }

        public string SerializerType { get; set; }

        public TimeSpan TriggerStateCheckInterval { get; set; }
    }
}

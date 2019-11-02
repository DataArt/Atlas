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
using DataArt.Atlas.Service.Scheduler.Sdk.Models;
using Quartz;

namespace DataArt.Atlas.Service.Scheduler.Scheduler
{
    internal static class JobDetailExtensions
    {
        public static void SetJobData(this IJobDetail jobDetail, JobDataModel jobData)
        {
            jobDetail.JobDataMap.Put(DataMapKeys.DataHashCode, jobData.GetDataHashCode());
            jobDetail.JobDataMap.Put(DataMapKeys.SdkVersion, jobData.SdkVersion);
            jobDetail.JobDataMap.Put(DataMapKeys.Settings, jobData.Settings);
        }

        public static string GetDataHashCode(this IJobDetail job)
        {
            return job.TryToReadDataMapString(DataMapKeys.DataHashCode);
        }

        public static string GetSdkVersion(this IJobDetail job)
        {
            return job.TryToReadDataMapString(DataMapKeys.SdkVersion);
        }

        public static TJobSettings GetSettings<TJobSettings>(this IJobDetail job)
            where TJobSettings : class
        {
            return JobDataModel.DeserializeSettings<TJobSettings>(job.TryToReadDataMapString(DataMapKeys.Settings));
        }

        private static string TryToReadDataMapString(this IJobDetail job, string key)
        {
            return job.JobDataMap.Keys.Contains(key) ? job.JobDataMap.GetString(key) : string.Empty;
        }

        private static class DataMapKeys
        {
            public const string DataHashCode = "dataHashCode";
            public const string SdkVersion = "sdkVersion";
            public const string Settings = "settings";
        }
    }
}

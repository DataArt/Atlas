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
using System.Collections.Generic;
using DataArt.Atlas.Service.Scheduler.Sdk.Models;

namespace DataArt.Atlas.Service.Scheduler.Sdk.JobRegistration
{
    public abstract class Job<TScheduleSettings> : IJob
    {
        private readonly ScheduleType scheduleType;

        protected Job(ScheduleType scheduleType)
        {
            this.scheduleType = scheduleType;
        }

        JobDataModel IJob.GetJobDataModel()
        {
            return new JobDataModel
            {
                SdkVersion = SchedulerClientConfig.Version,
                Settings = JobDataModel.SerializeSettings(GetJobSettings()),
                ScheduleType = scheduleType,
                ScheduleSettings = JobDataModel.SerializeSettings(GetScheduleSettings())
            };
        }

        protected abstract string JobId { get; }

        protected virtual bool IsJobEnabled => true;

        #region Web request job settings

        protected abstract string ServiceKey { get; }

        protected abstract string WebRequestUri { get; }

        protected virtual IDictionary<string, string> WebRequestParameters { get; } = null;

        protected virtual TimeSpan WebRequestTimeout { get; } = TimeSpan.FromMinutes(2);

        #endregion

        protected abstract TScheduleSettings GetScheduleSettings();

        private WebRequestJobSettingsModel GetJobSettings()
        {
            return new WebRequestJobSettingsModel
            {
                ServiceKey = ServiceKey,
                Uri = WebRequestUri,
                Parameters = WebRequestParameters,
                Timeout = WebRequestTimeout
            };
        }

        string IJob.JobId => JobId;

        string IJob.ServiceKey => ServiceKey;

        bool IJob.IsJobEnabled => IsJobEnabled;
    }
}

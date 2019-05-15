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
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl.Matchers;

namespace DataArt.Atlas.Service.Scheduler.Scheduler
{
    internal class AsyncScheduler : IAsyncScheduler
    {
        private readonly IScheduler scheduler;

        public AsyncScheduler(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public Task StartAsync(CancellationToken cancelationToken)
        {
            return scheduler.Start(cancelationToken);
        }

        public Task ShutdownAsync()
        {
            return scheduler.Shutdown(true);
        }

        public Task<IReadOnlyCollection<JobKey>> GetAllJobKeysAsync()
        {
            return scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        }

        public Task<IReadOnlyCollection<JobKey>> GetJobKeysAsync(string groupName)
        {
            return scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName));
        }

        public Task<bool> CheckJobExistsAsync(JobKey jobKey)
        {
            return scheduler.CheckExists(jobKey);
        }

        public Task<IJobDetail> GetJobDetailAsync(JobKey jobKey)
        {
            return scheduler.GetJobDetail(jobKey);
        }

        public Task CreateJobAsync(IJobDetail jobDetail)
        {
            return scheduler.AddJob(jobDetail, false);
        }

        public async Task DeleteJobAsync(JobKey jobKey)
        {
            if (!await scheduler.DeleteJob(jobKey))
            {
                throw new InvalidOperationException($"Unable to delete {jobKey.Name} job");
            }
        }

        public Task<IReadOnlyCollection<TriggerKey>> GetTriggerKeysAsync()
        {
            return scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
        }

        public Task<TriggerState> GetTriggerStateAsync(TriggerKey triggerKey)
        {
            return scheduler.GetTriggerState(triggerKey);
        }

        public Task<DateTimeOffset> CreateTriggerAsync(ITrigger trigger)
        {
            return scheduler.ScheduleJob(trigger);
        }
    }
}

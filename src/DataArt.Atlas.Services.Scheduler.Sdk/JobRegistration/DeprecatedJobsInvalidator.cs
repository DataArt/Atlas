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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataArt.Atlas.Infrastructure.Startup;

namespace DataArt.Atlas.Service.Scheduler.Sdk.JobRegistration
{
    internal sealed class DeprecatedJobsInvalidator : Startable
    {
        private readonly IJob[] registeredJobs;
        private readonly ISchedulerClient schedulerClient;

        public DeprecatedJobsInvalidator(IEnumerable<IJob> registeredJobs, ISchedulerClient schedulerClient)
        {
            this.registeredJobs = registeredJobs.ToArray();
            this.schedulerClient = schedulerClient;
        }

        protected override void StartInternal(CancellationToken cancellationToken)
        {
            var jobIds = registeredJobs.Where(j => j.IsJobEnabled).Select(j => j.JobId).ToArray();
            schedulerClient.InvalidateDeprecatedJobs(registeredJobs[0].ServiceKey, jobIds);
        }
    }
}

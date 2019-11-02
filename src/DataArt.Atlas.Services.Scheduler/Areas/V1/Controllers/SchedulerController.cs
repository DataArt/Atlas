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
using System;
using System.Threading.Tasks;
using System.Web.Http;
using DataArt.Atlas.Services.Scheduler.Scheduler;
using DataArt.Atlas.Services.Scheduler.Sdk.Models;

namespace DataArt.Atlas.Services.Scheduler.Areas.V1.Controllers
{
    [RoutePrefix("api/v1/scheduler/jobs")]
    public class SchedulerController : ApiController
    {
        private readonly ISchedulerService schedulerService;

        public SchedulerController(ISchedulerService schedulerService)
        {
            this.schedulerService = schedulerService;
        }

        [HttpPost]
        [Route("{jobGroup}/{id}")]
        public async Task ScheduleJob([FromUri] string jobGroup, [FromUri] string id, JobDataModel jobData)
        {
            if (jobData == null)
            {
                throw new ArgumentNullException(nameof(jobData));
            }

            await schedulerService.ScheduleJobAsync(jobGroup, id, jobData);
        }

        [HttpDelete]
        [Route("{jobGroup}/{id}")]
        public async Task DeleteJob([FromUri] string jobGroup, [FromUri] string id)
        {
            await schedulerService.DeleteJobAsync(jobGroup, id);
        }

        [HttpPut]
        [Route("{jobGroup}")]
        public async Task InvalidateDeprecatedJobs([FromUri] string jobGroup, string[] jobIds)
        {
            if (jobIds == null)
            {
                throw new ArgumentNullException(nameof(jobIds));
            }

            await schedulerService.InvalidateDeprecatedJobs(jobGroup, jobIds);
        }
    }
}
#endif
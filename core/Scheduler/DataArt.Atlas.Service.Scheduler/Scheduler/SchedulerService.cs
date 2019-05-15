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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataArt.Atlas.Infrastructure.Exceptions;
using DataArt.Atlas.Infrastructure.Startup;
using DataArt.Atlas.Service.Scheduler.Sdk;
using DataArt.Atlas.Service.Scheduler.Sdk.Models;
using DataArt.Atlas.Service.Scheduler.Settings;
using Quartz;

#if NET452
using Serilog;
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
#endif

namespace DataArt.Atlas.Service.Scheduler.Scheduler
{
    internal sealed class SchedulerService : Startable, IDisposable, ISchedulerService
    {
#if NETSTANDARD2_0
        private readonly ILogger logger = AtlasLogging.CreateLogger<SchedulerService>();
#endif

        private readonly ManualResetEvent invalidatedEvent = new ManualResetEvent(false);
        private readonly JobLockProvider jobLockProvider = new JobLockProvider();
        private readonly IAsyncScheduler scheduler;
        private readonly SchedulerSettings settings;

        public SchedulerService(IAsyncScheduler scheduler, SchedulerSettings settings)
        {
            this.scheduler = scheduler;
            this.settings = settings;
        }

        public void Dispose()
        {
            scheduler?.ShutdownAsync().GetAwaiter().GetResult();
            jobLockProvider.Dispose();
            invalidatedEvent.Dispose();
        }

        public async Task ScheduleJobAsync(string jobGroup, string jobId, JobDataModel jobData)
        {
            if (!SchedulerClientConfig.Version.Equals(jobData.SdkVersion))
            {
                throw new ApiValidationException("Invalid SDK version");
            }

            invalidatedEvent.WaitOne();

            var jobKey = JobKey.Create(jobId, jobGroup);

            using (await jobLockProvider.AcquireAsync(jobKey))
            {
                if (await scheduler.CheckJobExistsAsync(jobKey))
                {
                    var existingJobDetail = await scheduler.GetJobDetailAsync(jobKey);

                    if (jobData.GetDataHashCode().Equals(existingJobDetail.GetDataHashCode()))
                    {
#if NET452
                        Log.Debug("{jobId} job is up to date", jobId);
#endif

#if NETSTANDARD2_0
                        logger.LogDebug("{jobId} job is up to date", jobId);
#endif
                        return;
                    }

#if NET452
                    Log.Debug("{jobId} job will be updated", jobId);
#endif

#if NETSTANDARD2_0
                    logger.LogDebug("{jobId} job will be updated", jobId);
#endif

                    await scheduler.DeleteJobAsync(jobKey);

#if NET452
                    Log.Information("{jobId} job was deleted", jobKey.Name);
#endif

#if NETSTANDARD2_0
                    logger.LogInformation("{jobId} job was deleted", jobKey.Name);
#endif
                }

                var newJobDetail = JobDetailFactory.Create(jobKey, jobData);
                await scheduler.CreateJobAsync(newJobDetail);

#if NET452
                Log.Information("{jobId} job was created", jobId);
#endif

#if NETSTANDARD2_0
                logger.LogInformation("{jobId} job was created", jobId);
#endif

                var newTrigger = TriggerFactory.Create(jobKey, jobData);
                var firstFireMoment = await scheduler.CreateTriggerAsync(newTrigger);

#if NET452
                Log.Information("{jobId} job was scheduled (next execution: {firstFireMoment})", jobId, firstFireMoment);
#endif

#if NETSTANDARD2_0
                logger.LogInformation("{jobId} job was scheduled (next execution: {firstFireMoment})", jobId, firstFireMoment);
#endif
            }
        }

        public async Task DeleteJobAsync(string jobGroup, string jobId)
        {
            invalidatedEvent.WaitOne();

            var jobKey = JobKey.Create(jobId, jobGroup);

            using (await jobLockProvider.AcquireAsync(jobKey))
            {
                if (!await scheduler.CheckJobExistsAsync(jobKey))
                {
                    throw new NotFoundException(jobId);
                }

                await scheduler.DeleteJobAsync(jobKey);
            }
        }

        public async Task InvalidateDeprecatedJobs(string groupName, string[] jobIds)
        {
            var existingJobKeys = await scheduler.GetJobKeysAsync(groupName);
            var outdatedJobKeys = existingJobKeys.Where(jobKey => !jobIds.Contains(jobKey.Name));
            await InvalidateJobsAsync(outdatedJobKeys);
        }

        protected override async Task StartInternalAsync(CancellationToken cancellationToken)
        {
            await InvalidateJobsAsync(cancellationToken);
            await scheduler.StartAsync(cancellationToken);
        }

        protected override async Task StopInternalAsync()
        {
            await scheduler.ShutdownAsync();
        }

        private async Task InvalidateJobsAsync(CancellationToken cancellationToken)
        {
            {
                if (settings.EraseAllJobs)
                {
                    await InvalidateJobsAsync(await scheduler.GetAllJobKeysAsync(), cancellationToken);
                }

                invalidatedEvent.Set();
            }
        }

        private async Task InvalidateJobsAsync(IEnumerable<JobKey> jobIds, CancellationToken cancellationToken = default(CancellationToken))
        {
#if NET452
            Log.Debug("Jobs invalidation started");
#endif

#if NETSTANDARD2_0
            logger.LogDebug("Jobs invalidation started");
#endif

            foreach (var jobKey in jobIds)
            {
                try
                {
                    await scheduler.DeleteJobAsync(jobKey);
#if NET452
                    Log.Information("{jobId} job was invalidted", jobKey.Name);
#endif

#if NETSTANDARD2_0
                    logger.LogInformation("{jobId} job was invalidted", jobKey.Name);
#endif
                }
                catch (Exception e)
                {
#if NET452
                    Log.Error(e, "Unable to invalidate {jobId} job", jobKey.Name);
#endif

#if NETSTANDARD2_0
                    logger.LogError(e, "Unable to invalidate {jobId} job", jobKey.Name);
#endif
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            invalidatedEvent.Set();

#if NET452
            Log.Information("Jobs invalidation completed");
#endif

#if NETSTANDARD2_0
            logger.LogInformation("Jobs invalidation completed");
#endif
        }
    }
}

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
using System.Threading.Tasks;
using DataArt.Atlas.CallContext.Correlation;
using DataArt.Atlas.Service.Scheduler.Scheduler;
using DataArt.Atlas.Service.Scheduler.Sdk;
using Quartz;

#if NET452
using Serilog;
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
#endif

namespace DataArt.Atlas.Service.Scheduler.Jobs
{
    public abstract class BaseJob<TJobSettings> : IJob
        where TJobSettings : class
    {
#if NETSTANDARD2_0
        private readonly ILogger logger = AtlasLogging.CreateLogger<BaseJob<TJobSettings>>();
#endif

        public abstract Task ExecuteAsync(TJobSettings settings);

        public async Task Execute(IJobExecutionContext context)
        {
            var jobDetail = context.JobDetail;

            using (new CorrelatedSequence())
            {
#if NET452
                Log.Debug(
                    "Starting {jobId} job (sdk version: {version}, scheduled with sdk version {sdkVersion})",
                    jobDetail.Key.Name,
                    SchedulerClientConfig.Version,
                    jobDetail.GetSdkVersion());
#endif

#if NETSTANDARD2_0
                logger.LogDebug(
                    "Starting {jobId} job (sdk version: {clientVersion}, scheduled with sdk version {sdkVersion})",
                    jobDetail.Key.Name,
                    SchedulerClientConfig.Version,
                    jobDetail.GetSdkVersion());
#endif

                try
                {
                    var jobSettings = context.JobDetail.GetSettings<TJobSettings>();

                    await ExecuteAsync(jobSettings);

#if NET452
                    Log.Information(
                        "{jobId} job executed (sdk version: {clientVersion}, scheduled with sdk version {sdkVersion})",
                        jobDetail.Key.Name,
                        SchedulerClientConfig.Version,
                        jobDetail.GetSdkVersion());
#endif

#if NETSTANDARD2_0
                    logger.LogInformation(
                        "{jobId} job executed (sdk version: {clientVersion}, scheduled with sdk version {sdkVersion})",
                        jobDetail.Key.Name,
                        SchedulerClientConfig.Version,
                        jobDetail.GetSdkVersion());
#endif
                }
                catch (Exception e)
                {
#if NET452
                    Log.Error(
                        e,
                        "{jobId} job execution failed (sdk version: {clientVersion}, scheduled with sdk version {sdkVersion})",
                        jobDetail.Key.Name,
                        SchedulerClientConfig.Version,
                        jobDetail.GetSdkVersion());
#endif

#if NETSTANDARD2_0
                    logger.LogError(
                        e,
                        "{jobId} job execution failed (sdk version: {clientVersion}, scheduled with sdk version {sdkVersion})",
                        jobDetail.Key.Name,
                        SchedulerClientConfig.Version,
                        jobDetail.GetSdkVersion());
#endif
                }
            }
        }
    }
}

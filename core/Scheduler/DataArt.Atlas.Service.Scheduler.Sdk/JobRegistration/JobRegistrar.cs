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
using System.Threading;
using System.Threading.Tasks;
using DataArt.Atlas.Infrastructure.Exceptions;
using DataArt.Atlas.Infrastructure.Startup;
using DataArt.Atlas.Service.Scheduler.Sdk.Models;
using DataArt.Atlas.WebCommunication.Exceptions;
using Polly;

#if NET452
using Serilog;
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
#endif

namespace DataArt.Atlas.Service.Scheduler.Sdk.JobRegistration
{
    internal sealed class JobRegistrar : Startable
    {
#if NETSTANDARD2_0
        private readonly ILogger logger = AtlasLogging.CreateLogger<JobRegistrar>();
#endif

        private readonly ISchedulerClient schedulerClient;

        private readonly IJob job;
        private readonly JobDataModel jobData;

        private CancellationTokenSource registrationTaskCancellationTokenSource;

        protected override string Name => $"{job.GetType().Name}Registrar";

        public JobRegistrar(ISchedulerClient schedulerClient, IJob job)
        {
            this.schedulerClient = schedulerClient;

            this.job = job;
            jobData = job.GetJobDataModel();
        }

        protected override void StartInternal(CancellationToken cancellationToken)
        {
            if (!job.IsJobEnabled)
            {
#if NET452
                Log.Debug("{jobId} job registration is disabled", job.JobId);
#endif

#if NETSTANDARD2_0
                logger.LogDebug("{jobId} job registration is disabled", job.JobId);
#endif
                return;
            }

            registrationTaskCancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(
                () =>
            {
                return Policy.Handle<Exception>()
                    .WaitAndRetryForeverAsync(
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, timespan) => HandleRegisterJobException(exception, timespan))
                    .ExecuteAsync(RegisterJobAsync);
            }, registrationTaskCancellationTokenSource.Token);
        }

        protected override void StopInternal()
        {
            registrationTaskCancellationTokenSource.Cancel();
        }

        private Task RegisterJobAsync()
        {
            return schedulerClient.ScheduleJobAsync(job.ServiceKey, job.JobId, jobData);
        }

        private void HandleRegisterJobException(Exception exception, TimeSpan retryTimeSpan)
        {
            var messageTemplate = "Failed to register {JobId} job (sdk version: {JobSdkVersion}), next try in {RetryTimeSpan}";

            if (exception is CommunicationException || exception is OperationCanceledException || exception is ApiValidationException)
            {
#if NET452
                Log.Warning(exception, messageTemplate, job.JobId, jobData.SdkVersion, retryTimeSpan);
#endif

#if NETSTANDARD2_0
                logger.LogWarning(exception, messageTemplate, job.JobId, jobData.SdkVersion, retryTimeSpan);
#endif
            }
            else
            {
#if NET452
                Log.Error(exception, messageTemplate, job.JobId, jobData.SdkVersion, retryTimeSpan);
#endif

#if NETSTANDARD2_0
                logger.LogError(exception, messageTemplate, job.JobId, jobData.SdkVersion, retryTimeSpan);
#endif
            }
        }
    }
}

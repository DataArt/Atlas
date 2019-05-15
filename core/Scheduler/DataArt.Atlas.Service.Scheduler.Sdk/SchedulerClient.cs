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
using DataArt.Atlas.Infrastructure.Interfaces;
using DataArt.Atlas.Service.Scheduler.Sdk.Models;
using DataArt.Atlas.WebCommunication;

namespace DataArt.Atlas.Service.Scheduler.Sdk
{
    internal sealed class SchedulerClient : ISchedulerClient
    {
        private const string RoutePrefix = "api/v1/scheduler/jobs";
        private readonly IRequestFactory requestFactory;

        public SchedulerClient(Func<ISdkClient, IRequestFactory> requestFactoryFunc)
        {
            requestFactory = requestFactoryFunc(this);
        }

        public Task ScheduleJobAsync(string jobGroup, string jobId, JobDataModel jobData)
        {
            return requestFactory.PostRequest(RoutePrefix)
                .AddUrlSegment(jobGroup)
                .AddUrlSegment(jobId)
                .AddBody(jobData)
                .PostAsync();
        }

        public Task InvalidateDeprecatedJobs(string serviceKey, string[] jobIds)
        {
            return requestFactory.PutRequest(RoutePrefix)
                .AddUrlSegment(serviceKey)
                .AddBody(jobIds)
                .PutAsync();
        }
    }
}

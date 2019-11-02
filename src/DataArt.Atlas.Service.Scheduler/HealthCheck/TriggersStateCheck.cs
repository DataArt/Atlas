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
using System.Threading.Tasks;
using DataArt.Atlas.Core.HealthCheck;
using DataArt.Atlas.Hosting.HealthCheck;
using DataArt.Atlas.Service.Scheduler.Scheduler;
using DataArt.Atlas.Service.Scheduler.Settings;
using Quartz;

#if NET452
using Serilog;
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
#endif

namespace DataArt.Atlas.Service.Scheduler.HealthCheck
{
    internal sealed class TriggersStateCheck : RecurrentHealthCheck
    {
#if NETSTANDARD2_0
        private readonly ILogger logger = AtlasLogging.CreateLogger<TriggersStateCheck>();
#endif

        private readonly IAsyncScheduler scheduler;

        public TriggersStateCheck(IAsyncScheduler scheduler, IHealthReporter healthReporter, QuartzSettings settings) : base(settings.TriggerStateCheckInterval, healthReporter)
        {
            this.scheduler = scheduler;
        }

        protected override string Property => "SchedulerTriggersState";

        protected override HealthState HealthCheck()
        {
            return CheckAsync().GetAwaiter().GetResult();
        }

        private async Task<HealthState> CheckAsync()
        {
            var anyErrors = false;

            foreach (var triggerKey in await scheduler.GetTriggerKeysAsync())
            {
                if (await scheduler.GetTriggerStateAsync(triggerKey) == TriggerState.Error)
                {
#if NET452
                    Log.Error("{jobId} job trigger is in an error state", triggerKey.Name);
#endif

#if NETSTANDARD2_0
                    logger.LogError("{jobId} job trigger is in an error state", triggerKey.Name);
#endif
                    anyErrors = true;
                    break;
                }
            }

            return anyErrors ? HealthState.Error : HealthState.Ok;
        }
    }
}

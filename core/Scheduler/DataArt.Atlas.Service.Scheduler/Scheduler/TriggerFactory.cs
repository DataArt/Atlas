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
using DataArt.Atlas.Infrastructure.Exceptions;
using DataArt.Atlas.Service.Scheduler.Sdk.Models;
using Quartz;

namespace DataArt.Atlas.Service.Scheduler.Scheduler
{
    internal class TriggerFactory
    {
        public static ITrigger Create(JobKey key, JobDataModel data)
        {
            switch (data.ScheduleType)
            {
                case ScheduleType.Recurrent: return CreateRecurrentTrigger(key, JobDataModel.DeserializeSettings<RecurrentScheduleSettingsModel>(data.ScheduleSettings));
                case ScheduleType.Cron: return CreateCronTrigger(key, JobDataModel.DeserializeSettings<CronScheduleSettingsModel>(data.ScheduleSettings));
                default: throw new NotSupportedException();
            }
        }

        private static ITrigger CreateRecurrentTrigger(JobKey jobKey, RecurrentScheduleSettingsModel settings)
        {
            var trigger = TriggerBuilder.Create()
                .ForJob(jobKey)
                .WithIdentity(jobKey.GetTriggerKey())
                .StartNow()
                .WithSimpleSchedule(s => s
                    .WithInterval(settings.RunningInterval)
                    .RepeatForever()
                    .WithMisfireHandlingInstructionNextWithRemainingCount())
                .Build();

            return trigger;
        }

        private static ITrigger CreateCronTrigger(JobKey jobKey, CronScheduleSettingsModel settings)
        {
            if (string.IsNullOrWhiteSpace(settings.CronExpression))
            {
                throw new ApiValidationException("Invalid cron expression");
            }

            var trigger = TriggerBuilder.Create()
                .ForJob(jobKey)
                .WithIdentity(jobKey.GetTriggerKey())
                .StartNow()
                .WithCronSchedule(settings.CronExpression, cs => cs
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId))
                    .WithMisfireHandlingInstructionFireAndProceed())
                .Build();

            return trigger;
        }
    }
}
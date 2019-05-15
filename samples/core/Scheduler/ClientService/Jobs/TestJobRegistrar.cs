using System;
using ClientService.Application;
using DataArt.Atlas.Service.Scheduler.Sdk.JobRegistration;

namespace ClientService.Jobs
{
    internal class TestJobRegistrar : RecurrentJob
    {
        private readonly JobSettings settings;

        public TestJobRegistrar(JobSettings settings)
        {
            this.settings = settings;
        }

        protected override string JobId => "TestJob";

        protected override string ServiceKey => SchedulerClientStartup.Key;

        protected override string WebRequestUri => "api/v1/jobs";

        protected override TimeSpan RunningInterval => settings.Interval;

        // for job auto delete please set:
        // protected override bool IsJobEnabled => false;
    }
}

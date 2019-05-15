using System.Collections.Generic;
using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Logging;
using DataArt.Atlas.Service.Scheduler.Settings;
using DataArt.Atlas.ServiceDiscovery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace SchedulerService.Application
{
    internal sealed class SchedulerStartup : DataArt.Atlas.Service.Scheduler.Application.SchedulerService
    {
        public SchedulerStartup(IHostingEnvironment env, IConfiguration configuration, IApplication application) 
            : base(env, configuration, application)
        {
        }

        protected override void ConfigureContainer(IServiceCollection services)
        {
            var cfg = new LoggerConfiguration();
            cfg.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({CorrelationId}) {Message}{NewLine}{Exception}");
            services.UseSeriLog(cfg);

            services.RegisterConfiguration<QuartzSettings>(Configuration);

            // todo: add appropriate service discovery
            services.AddTransient<IServiceDiscovery>(x => new ServiceDiscovery(new Dictionary<string, string>
            {
                {"SchedulerClient", "http://localhost:10002"}
            }));

            base.ConfigureContainer(services);
        }
    }
}

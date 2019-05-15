using System.Collections.Generic;
using ClientService.Jobs;
using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Infrastructure.Container;
using DataArt.Atlas.Logging;
using DataArt.Atlas.Service.Scheduler.Sdk;
using DataArt.Atlas.ServiceDiscovery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ClientService.Application
{
    internal sealed class SchedulerClientStartup : Startup
    {
        public static readonly string Key = "SchedulerClient";

        protected override string ServiceKey => Key;

        public SchedulerClientStartup(IHostingEnvironment env, IConfiguration configuration, IApplication application) : base(env, configuration, application)
        {
        }

        protected override void ConfigureContainer(IServiceCollection services)
        {
            var cfg = new LoggerConfiguration();
            cfg.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({CorrelationId}) {Message}{NewLine}{Exception}");
            services.UseSeriLog(cfg);

            // todo: add appropriate service discovery
            services.AddTransient<IServiceDiscovery>(x => new ServiceDiscovery(new Dictionary<string, string>
            {
                {"Scheduler", "http://localhost:10001"}
            }));

            services.RegisterModule<ContainerModule>();
            services.RegisterConfiguration<JobSettings>(Configuration);
        }
    }
}

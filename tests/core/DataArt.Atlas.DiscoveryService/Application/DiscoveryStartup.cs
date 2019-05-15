using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DataArt.Atlas.DiscoveryService.Application
{
    internal class DiscoveryStartup : Startup
    {
        private readonly IConfiguration configuration;
        public static string Key = "Discovery";

        protected override string ServiceKey => Key;

        public DiscoveryStartup(IHostingEnvironment env, IConfiguration configuration, IApplication application) : base(env, configuration, application)
        {
            this.configuration = configuration;
        }

        protected override void ConfigureContainer(IServiceCollection services)
        {
            var cfg = new LoggerConfiguration();
            cfg.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({CorrelationId}) {Message}{NewLine}{Exception}");
            services.UseSeriLog(cfg);

            services.RegisterConfiguration<Discovery>(configuration);
        }
    }
}

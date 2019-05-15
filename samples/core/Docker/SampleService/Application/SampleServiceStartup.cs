using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Infrastructure.Container;
using DataArt.Atlas.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace SampleService.Application
{
    internal class SampleServiceStartup : Startup
    {
        public SampleServiceStartup(IHostingEnvironment env, IConfiguration configuration, IApplication application) : base(env, configuration, application)
        {
        }

        public static string Key = "Sample";

        protected override string ServiceKey => Key;

        protected override void ConfigureContainer(IServiceCollection services)
        {
            var cfg = new LoggerConfiguration();
            cfg.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({CorrelationId}) {Message}{NewLine}{Exception}");
            services.UseSeriLog(cfg);
        }
    }
}

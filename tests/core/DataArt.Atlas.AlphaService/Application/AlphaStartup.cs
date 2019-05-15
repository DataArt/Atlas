using DataArt.Atlas.AlphaService.Communication;
using DataArt.Atlas.BetaService.Sdk;
using DataArt.Atlas.Common;
using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Infrastructure.Container;
using DataArt.Atlas.Logging;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.Messaging.RabbitMq;
using DataArt.Atlas.ServiceDiscovery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DataArt.Atlas.AlphaService.Application
{
    internal class AlphaStartup : Startup
    {

        public AlphaStartup(IHostingEnvironment env, IConfiguration configuration, IApplication application) : base(env, configuration, application)
        {
        }

        public static string Key = "Alpha";

        protected override string ServiceKey => Key;

        protected override void ConfigureContainer(IServiceCollection services)
        {
            var cfg = new LoggerConfiguration();
            cfg.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({CorrelationId}) {Message}{NewLine}{Exception}");
            services.UseSeriLog(cfg);

            services.RegisterConfiguration<DiscoverySettings>(Configuration);
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddSingleton<IServiceDiscovery, Common.ServiceDiscovery>();
            services.RegisterBusInitiator<RabbitMqServiceBusInitiator, DefaultBusType>();
            services.RegisterModule<ContainerModule>();
        }
    }
}

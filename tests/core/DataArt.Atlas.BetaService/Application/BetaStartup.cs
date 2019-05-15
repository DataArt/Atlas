using DataArt.Atlas.BetaService.Data;
using DataArt.Atlas.BetaService.Sdk;
using DataArt.Atlas.BetaService.State;
using DataArt.Atlas.Common;
using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Hosting;
using DataArt.Atlas.Logging;
using DataArt.Atlas.Messaging.Consume;
using DataArt.Atlas.Messaging.MassTransit;
using DataArt.Atlas.Messaging.RabbitMq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DataArt.Atlas.BetaService.Application
{
    internal class BetaStartup : Startup
    {
        private readonly IConfiguration configuration;

        protected override string ServiceKey => ClientConfig.ServiceKey;

        public BetaStartup(IHostingEnvironment env, IConfiguration configuration, IApplication application) : base(env, configuration, application)
        {
            this.configuration = configuration;
        }

        protected override void ConfigureContainer(IServiceCollection services)
        {
            var cfg = new LoggerConfiguration();
            cfg.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({CorrelationId}) {Message}{NewLine}{Exception}");
            services.UseSeriLog(cfg);

            services.RegisterConfiguration<DiscoverySettings>(configuration);
            services.AddSingleton<IStateService, StateService>();
            services.RegisterBusInitiator<RabbitMqServiceBusInitiator, DefaultBusType>();
            services.UseConsumerRegistration<MasstransitConsumerRegistrator>();
            services.AddSingleton<IDataService, DataService>();
        }
    }
}

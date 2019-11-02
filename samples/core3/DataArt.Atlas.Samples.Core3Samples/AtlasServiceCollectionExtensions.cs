using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Samples.Core3Samples
{
    public static class AtlasServiceCollectionExtensions
    {
        public static IServiceCollection AddAtlasServices(this IServiceCollection services)
        {
            return services.AddAtlasServices(configure => { });
        }

        public static IServiceCollection AddAtlasServices(this IServiceCollection services, Action<IAtlasBuilder> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Adding atlas core services

            // CorrelationId should be set in a very begining
            //CorrelationContext.SetCorrelationId();
            //mainThreadCorrelationId = CorrelationContext.CorrelationId;

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //ApplicationSettings = new ApplicationSettings();
            //Configuration.GetSection(typeof(ApplicationSettings).Name).Bind(ApplicationSettings);

            //// add custom configuration section
            //services.Configure<ApplicationSettings>(opt => Configuration.GetSection(typeof(ApplicationSettings).Name));
            //services.AddSingleton(ApplicationSettings);

            //// add AutoMapper
            //services.ConfigureAutoMapper(ConfigureAutoMapper);

            //// add version resolver
            //services.AddTransient<IVersionInfoResolver>(
            //    provider =>
            //        new VersionInfoResolver(Application.GetCodePackageVersionFunction, Application.GetDataPackageVersionFunction));

            //delegate the rest to a user 
            var builder = new AtlasBuilder(services);
            configure(builder);

            var app = builder.Build();

            return services;
        }
    }
}
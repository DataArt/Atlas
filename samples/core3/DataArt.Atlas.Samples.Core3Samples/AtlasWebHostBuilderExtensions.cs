using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataArt.Atlas.Samples.Core3Samples
{
    public static class AtlasGenericHostBuilderExtensions
    {
        public static IHostBuilder UseAtlas(this IHostBuilder builder, Action<IAtlasBuilder> configure)
        {
            return builder
                   .ConfigureWebHostDefaults(whb => whb.Configure(app => app.ConfigureAtlasMiddleware()))
                   .ConfigureServices(services => services.ConfigureAtlasServices(configure));
        }
    }

    public class AtlasOptions
    {
        public bool UseSwagger { get; set; }

        public bool RedirectToSwaggerUI { get; set; }

        public bool UseMemoryCache { get; set; }

        public bool UseServicesAutoRegistration { get; internal set; }
    }

    public static class AtlasWebHostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureAtlas(this IWebHostBuilder builder, Action<AtlasOptions> configure)
        {
            builder
                .Configure(app => app.ConfigureAtlasMiddleware())
                .ConfigureServices(services => services.ConfigureAtlasServices(configure));
            var options = new AtlasOptions();
            configure(options);
            return builder;
        }
    }

    public static class AtlasApplicationBuilderExtensions
    { 
        public static void ConfigureAtlasMiddleware(this IApplicationBuilder app)
        {
            app.UseAtlasMiddleware()
               .UseRouting()
               .UseEndpoints(routes => { routes.MapControllers(); })
               .UseWelcomePage();

            //var logger2 = app.ApplicationServices.GetService<ILoggerFactory>();
            //var logger = logger2.CreateLogger("Main");

            app.Run(context => { return context.Response.WriteAsync("Hello world. Make sure you run this app using 'dotnet watch run'."); });
            app.UseResponseCompression();
        }

        private static IApplicationBuilder UseAtlasMiddleware(this IApplicationBuilder app)
        {
            //app.UseMiddleware<CorrelationIdHandlerMiddleware>();
            //app.UseMiddleware<UnhandledExceptionLogMiddleware>();
            //app.UseMiddleware<ApiAccessLoggingMiddleware>();
            //app.UseMiddleware<HttpNotFoundMiddleware>();
            return app;
        }

        public static void ConfigureAtlasServices(this IServiceCollection services, Action<AtlasOptions> configure)
        {
            var builder = new AtlasOptions();
            configure(builder);

            //var app = builder.Build();

            //services.AddAtlasServices(configure);
        }
    }
}
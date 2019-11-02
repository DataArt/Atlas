using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Samples.Core3Samples
{
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
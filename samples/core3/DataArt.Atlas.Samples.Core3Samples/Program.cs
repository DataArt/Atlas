using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DataArt.Atlas.Samples.Core3Samples
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(cfg =>
                {
                    Console.WriteLine("ConfigureHostConfiguration");
                    cfg.AddJsonFile("appsettings.json");
                })
                .UseAtlas(app =>
                {
                    Console.WriteLine("Use atlas");
                    app.UseHealthCheck();
                    app.UseMessaging(options =>
                    {
                        Console.WriteLine("Use messaging options");
                        options.UseMassTransitRuntime();
                        options.RegisterConsumers();
                    });
                    app.UseServiceDiscovery(options => options.UseKubernetesDiscovery());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    Console.WriteLine("ConfigureWebHostDefaults");

                    webBuilder
                        .ConfigureAtlas(options =>
                        {
                            Console.WriteLine("--ConfigureAtlas");
                            options.UseSwagger = true;
                            options.RedirectToSwaggerUI = true;
                            options.UseServicesAutoRegistration = true;
                        })
                        .ConfigureKestrel(options =>
                        {
                            Console.WriteLine("--ConfigureKestrel");
                            options.ListenLocalhost(5000, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1;

                                //listenOptions.UseHttps();
                            });
                        })
                        .ConfigureLogging(logging =>
                        {
                            Console.WriteLine("--ConfigureLogging");
                        })
                        .CaptureStartupErrors(true)
                        .UseEnvironment("Development");
                })
                .UseConsoleLifetime();
    }
}
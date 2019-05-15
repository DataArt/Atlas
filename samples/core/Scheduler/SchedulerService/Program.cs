using System.IO;
using System.Reflection;
using DataArt.Atlas.Configuration.File;
using DataArt.Atlas.Hosting.Console;
using DataArt.Atlas.Service.Scheduler.Sdk;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SchedulerService.Application;

namespace SchedulerService
{
    class Program
    {
        static void Main()
        {
            var endpoint = new ConfigurationBuilder()
                .GetJsonEndpointSetting(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "hosting.json");

            var builder = new WebHostBuilder();
            builder
                .ConfigureServices((c, s) =>
                {
                    s.UseJsonConfiguration(c.HostingEnvironment, fileNames: "appsettings.json");
                })
                .StartInConsole<SchedulerStartup>(
                    serviceName: SchedulerClientConfig.ServiceKey,
                    url: endpoint.Endpoint);
        }
    }
}

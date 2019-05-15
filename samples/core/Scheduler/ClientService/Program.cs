using System.IO;
using System.Reflection;
using ClientService.Application;
using DataArt.Atlas.Configuration.File;
using DataArt.Atlas.Hosting.Console;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ClientService
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
                .StartInConsole<SchedulerClientStartup>(
                    serviceName: SchedulerClientStartup.Key,
                    url: endpoint.Endpoint);
        }
    }
}

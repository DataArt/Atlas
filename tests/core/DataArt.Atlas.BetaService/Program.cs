using System.IO;
using System.Reflection;
using DataArt.Atlas.BetaService.Application;
using DataArt.Atlas.BetaService.Sdk;
using DataArt.Atlas.Configuration.File;
using DataArt.Atlas.Hosting.Console;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DataArt.Atlas.BetaService
{
    class Program
    {
        static void Main(string[] args)
        {
            // todo
            var endpoint = new ConfigurationBuilder()
                .GetJsonEndpointSetting(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "hosting.json");

            var builder = new WebHostBuilder();
            builder
                .ConfigureServices((c, s) =>
                {
                    s.UseJsonConfiguration(c.HostingEnvironment, fileNames: "appsettings.json");
                })
                .StartInConsole<BetaStartup>(serviceName: ClientConfig.ServiceKey, url: endpoint.Endpoint);
        }
    }
}

using System.IO;
using System.Reflection;
using DataArt.Atlas.Configuration.File;
using DataArt.Atlas.Hosting.Console;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SampleService.Application;

namespace SampleService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var endpoint = new ConfigurationBuilder()
                    .GetJsonEndpointSetting(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "hosting.json");

            var builder = new WebHostBuilder();
            builder
                .ConfigureServices((c, s) =>
                {
                    s.UseJsonConfiguration(c.HostingEnvironment, fileNames: "appsettings.json");
                })
                .StartInConsole<SampleServiceStartup>(serviceName: SampleServiceStartup.Key, url: endpoint.Endpoint);
        }
    }
}

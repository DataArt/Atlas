using Microsoft.Extensions.DependencyInjection;

namespace DataArt.Atlas.Samples.Core3Samples
{
    public interface IAtlasExtensionModule
    {
        void ConfigureApplication(IApplicationBuilder builder);
        void ConfigureServices(IServiceCollection services);
    }
}
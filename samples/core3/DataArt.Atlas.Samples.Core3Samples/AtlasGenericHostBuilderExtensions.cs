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
}
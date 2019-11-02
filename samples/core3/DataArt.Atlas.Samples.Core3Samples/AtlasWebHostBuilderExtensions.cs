using System;

namespace DataArt.Atlas.Samples.Core3Samples
{
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
}
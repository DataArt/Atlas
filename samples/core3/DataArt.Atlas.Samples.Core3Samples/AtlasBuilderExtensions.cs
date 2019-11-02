using System;

namespace DataArt.Atlas.Samples.Core3Samples
{
    public static class AtlasBuilderExtensions
    {
        public static IAtlasBuilder UseHealthCheck(this IAtlasBuilder builder, Action<HealthCheckOptions> options = null)
        {
            // add health check services to builder
            return builder;
        }

        public static IAtlasBuilder UseMessaging(this IAtlasBuilder builder, Action<MessagingOptions> options = null)
        {
            return builder;
        }

        public static IAtlasBuilder UseServiceDiscovery(this IAtlasBuilder builder, Action<ServiceDiscoveryOptions> options = null)
        {
            return builder;
        }
    }
}
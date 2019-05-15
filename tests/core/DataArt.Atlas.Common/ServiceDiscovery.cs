using System;
using DataArt.Atlas.CallContext.Correlation;
using DataArt.Atlas.ServiceDiscovery;
using Flurl;
using Flurl.Http;

namespace DataArt.Atlas.Common
{
    public sealed class ServiceDiscovery : IServiceDiscovery
    {
        private const string Path = "api/v1/discovery";
        private readonly string endpoint;

        public ServiceDiscovery(DiscoverySettings settings)
        {
            endpoint = settings.DiscoveryEndpoint;
        }

        public Uri ResolveServiceUrl(string serviceKey)
        {
            var url = endpoint
                .AppendPathSegment(Path)
                .SetQueryParam("serviceKey", serviceKey)
                .WithHeader(CorrelationContext.CorrelationIdName, CorrelationContext.CorrelationId)
                .GetJsonAsync<Uri>().Result;

            return url;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataArt.Atlas.Infrastructure.Interfaces;
using DataArt.Atlas.Infrastructure.OData;
using DataArt.Atlas.WebCommunication;

namespace DataArt.Atlas.BetaService.Sdk
{
    internal class Client : IClient
    {
        private readonly IRequestFactory requestFactory;

        private const string RoutePrefix = "api/v1/data";

        public Client(Func<ISdkClient, IRequestFactory> requestFactoryFunc)
        {
            requestFactory = requestFactoryFunc(this);
        }

        public Task<ODataResponse<Data>> GetListAsync(IDictionary<string, string> oDataQuery)
        {
            return requestFactory.GetRequest(RoutePrefix)
                .AddUriParameters(oDataQuery.AppendInlineCount())
                .GetAsync<ODataResponse<Data>>();

        }
    }
}

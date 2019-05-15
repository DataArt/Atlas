using System;
using System.Threading.Tasks;
using DataArt.Atlas.BetaService.Sdk;
using DataArt.Atlas.Infrastructure.OData;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Polly;

namespace DataArt.Atlas.Tests
{
    [TestFixture]
    public class WebApiCommunicationTests
    {
        [Test]
        public async Task ChainWebApiCallTest()
        {
            const int retryCount = 5;
            const int delayInSeconds = 1;

            var correlationId = Guid.NewGuid();

            var response = await $"{TestConstants.EndpointUrl}:{TestConstants.AlphaServicePort}"
                .AppendPathSegments("api/v1/communication/webapi")
                .SetQueryParam("serviceKey", "Beta")
                .WithHeader("CorrelationId", correlationId)
                .GetAsync();

            Assert.IsTrue(response.IsSuccessStatusCode);

            var events = await Policy.HandleResult<JArray>(res => res.Count < 3)
                .WaitAndRetryAsync(retryCount,
                    retryStepNumber => TimeSpan.FromSeconds(retryStepNumber * delayInSeconds))
                .ExecuteAsync(() => $"{TestConstants.EndpointUrl}:{TestConstants.SeqPort}"
                    .AppendPathSegments("api/events")
                    .SetQueryParam("filter", $"CorrelationId='{correlationId}' && SourceContext='DataArt.Atlas.Core.Application.Http.ApiAccessLoggingMiddleware'")
                    .GetJsonAsync<JArray>());

            Assert.AreEqual(3, events.Count);
        }

        [Test]
        public async Task ODataWebApiTest()
        {
            var response = await $"{TestConstants.EndpointUrl}:{TestConstants.AlphaServicePort}"
                .AppendPathSegments("api/v1/data")
                .SetQueryParam("$top", "20")
                .SetQueryParam("$orderby", "Name")
                .GetJsonAsync<ODataResponse<Data>>();

            Assert.AreEqual(3, response.Count);
            for (var i = 0; i<response.Count-1; ++i)
            {
                Assert.Less(response.Items[i].Name, response.Items[i+1].Name);
            }
        }
    }
}

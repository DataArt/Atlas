using System;
using System.Threading.Tasks;
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
                    .SetQueryParam("filter", $"CorrelationId='{correlationId}'")
                    .GetJsonAsync<JArray>());

            Assert.AreEqual(3, events.Count);
        }
    }
}

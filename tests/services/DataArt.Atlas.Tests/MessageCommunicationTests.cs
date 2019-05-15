using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using NUnit.Framework;
using Polly;

namespace DataArt.Atlas.Tests
{
    [TestFixture]
    public class MessageCommunicationTests
    {
        [Test]
        public async Task EsbPublishTest()
        {
            const int retryCount = 5;
            const int delayInSeconds = 1;

            var initState = await GetCurrentEsbState();

            var response = await $"{TestConstants.EndpointUrl}:{TestConstants.AlphaServicePort}"
                .AppendPathSegments("api/v1/communication/esb")
                .GetAsync();

            Assert.IsTrue(response.IsSuccessStatusCode);

            var state = await Policy.HandleResult<int>(res => res == initState)
                .WaitAndRetryAsync(retryCount,
                    retryStepNumber => TimeSpan.FromSeconds(retryStepNumber * delayInSeconds))
                .ExecuteAsync(GetCurrentEsbState);

            Assert.AreEqual(1, state - initState);
        }

        private async Task<int> GetCurrentEsbState()
        {
            return await $"{TestConstants.EndpointUrl}:{TestConstants.BetaServicePort}"
                .AppendPathSegments("api/v1/state/communicationMessage")
                .GetJsonAsync<int>();
        }
    }
}

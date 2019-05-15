using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using NUnit.Framework;

namespace DataArt.Atlas.Tests
{
    [TestFixture]
    public class ServiceInitializationTests
    {
        private const string VersionPath = "api/version";

        [TestCase(TestConstants.AlphaServicePort)]
        [TestCase(TestConstants.BetaServicePort)]
        [TestCase(TestConstants.DiscoveryServicePort)]
        public async Task AssertServiceStartSuccessfully(int servicePort)
        {
            var response = await $"{TestConstants.EndpointUrl}:{servicePort}".AppendPathSegments(VersionPath)
                .GetAsync();

            Assert.IsTrue(response.IsSuccessStatusCode);
        }
    }
}

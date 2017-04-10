using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Jolokia.Tests
{
    public class ServerTests : TestBase
    {
        public ServerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            
        }

        [Fact]
        public async Task Test_ServerInfo()
        {
            var version = await Client.LoadVersionAsync();
            Assert.NotEmpty(version);
        }
    }
}

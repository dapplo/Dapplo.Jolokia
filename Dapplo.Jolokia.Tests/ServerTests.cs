using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dapplo.Jolokia.Tests
{
    public class ServerTests
    {
        [Fact]
        public async Task Test_ServerInfo()
        {
            var client = await JolokiaClient.CreateAsync(new Uri("http://jeeadmin:ingdiba1!@gf0vsxja095t:8180/jolokia"));
            await client.LoadVersionAsync();

        }
    }
}

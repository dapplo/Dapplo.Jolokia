using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Jolokia.Tests
{
    public class MBeanTests : TestBase
    {
        public MBeanTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [Fact]
        public async Task Test_MemoryMBean()
        {
            await Client.LoadListAsync("java.lang", "type=Memory");

            var javaLangDomain = Client.Domains["java.lang"];
            Assert.NotNull(javaLangDomain);
            var memoryMBean = (from mbean in javaLangDomain.Values
                               where mbean.Name == "type=Memory"
                               select mbean).First();
            Assert.NotNull(memoryMBean);
            var gcOperation = (from operation in memoryMBean.Operations
                                        where operation.Key == "gc"
                                        select operation.Value).First();

            Assert.NotNull(gcOperation);
            var heapMemoryAttribute = (from attribute in memoryMBean.Attributes
                                         where attribute.Key == "HeapMemoryUsage"
                                         select attribute.Value).First();
            Assert.NotNull(heapMemoryAttribute);
            var nonHeapMemoryAttribute = (from attribute in memoryMBean.Attributes
                                            where attribute.Key == "NonHeapMemoryUsage"
                                            select attribute.Value).First();
            Assert.NotNull(nonHeapMemoryAttribute);

            await Client.ExecuteAsync<string>(gcOperation, Enumerable.Empty<string>());
        }
    }
}

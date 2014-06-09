using NUnit.Core;
using NUnit.Framework;

namespace NUnitAllureAdapter
{
    [TestFixture]
    public class Test
    {
        static Test()
        {
            new AllureAdapter().Install(CoreExtensions.Host);
        }

        [Test]
        public void Test1()
        {
            
        }

        [TestCase("s")]
        [TestCase("f")]
        [TestCase("g")]
        public void Test2(string s)
        {

        }
    }
}

using System;
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
        [Ignore]
        public void Test1()
        {
            throw new AssertionException("1");
        }

        [TestCase("s")]
        [TestCase("f")]
        [TestCase("g")]
        public void Test2(string s)
        {
            throw new Exception();
        }
    }
}

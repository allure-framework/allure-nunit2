using System;
using System.Threading;
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
            throw new AssertionException("42");
        }

        [Test]
        public void Test3()
        {
            Thread.Sleep(10000);
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

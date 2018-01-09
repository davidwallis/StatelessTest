using System;
using Moq;
using NUnit.Framework;
using StatelessTest;

namespace StatelessTest_Tests
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            // TODO: Add your test code here
            //Assert.Pass("Your first passing test");

            Mock<IPerson> person = new Mock<IPerson>();
        }
    }
}

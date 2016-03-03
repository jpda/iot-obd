using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Parkwood.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var device = new TestObdDevice(new TestObdPort());
            var subscriber = new TestObdPublisher();
            var unsubscriber = device.Subscribe(subscriber);
            Assert.IsNotNull(subscriber.State);
        }
    }
}

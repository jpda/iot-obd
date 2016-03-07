using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Parkwood.Test
{
    [TestClass]
    public class UnitTest1
    {
        //[TestMethod]
        //public void TestMethod1()
        //{
        //    var device = new TestObdDevice(new TestObdPort());
        //    var subscriber = new TestSubscriber();
        //    var unsubscriber = device.Subscribe(subscriber);
        //    Assert.IsNotNull(subscriber.State);
        //}

        [TestMethod]
        public void LineTerminatorByte()
        {
            var eol = Encoding.ASCII.GetBytes(">");
            Assert.IsTrue(Encoding.ASCII.GetByteCount(">") == 1);
            var parsed = byte.Parse("3E", NumberStyles.HexNumber);
            //Assert.AreEqual(eol.Single(), parsed);

        }
    }
}

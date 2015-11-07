using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pk.OrleansUtils.Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Tests
{
    [TestClass]
    public class ConsulConnectionInfoTests
    {
        [TestMethod]
        public void ConnectionInfo_ShouldBeCreatedFromConnectionString()
        {
            var ci = ConsulConnectionInfo.FromConnectionString("host=Xyz;datacenter=datacenter1;port=8512");
            Assert.IsTrue(ci.Host == "Xyz");
            Assert.IsTrue(ci.Datacenter == "datacenter1");
            Assert.IsTrue(ci.Port == 8512);
        }
        [TestMethod]
        public void ConnectionInfo_ParamlessShouldPointAtLocalhost8500()
        {
            var ci = new ConsulConnectionInfo();
            Assert.IsTrue(ci.Host == "localhost");
            Assert.IsTrue(ci.Datacenter == "");
            Assert.IsTrue(ci.Port == 8500);
        }
    }
}

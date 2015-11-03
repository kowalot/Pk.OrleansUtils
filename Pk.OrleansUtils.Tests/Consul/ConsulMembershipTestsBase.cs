using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.TestingHost;
using Pk.OrleansUtils.Consul;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Pk.OrleansUtils.Tests.Consul;

namespace Pk.OrleansUtils.Tests.Consul
{
    public class ConsulMembershipTestsBase
    {


        public ConsulSystemStoreProvider ConsulMembershipTable { get; set; }

        public MyTestingHost SiloHost { get; set; }
        public TestingSiloOptions SiloHostConfig { get; private set; }

        public ClusterConfiguration ClusterConfig { get; set; }

        protected void Init(int mode = 1)
        {
            SiloHostConfig = new TestingSiloOptions
            {
                StartPrimary = true,
                ParallelStart = false,
                PickNewDeploymentId = true,
                StartFreshOrleans = true,
                StartSecondary = false,
                SiloConfigFile = new FileInfo("OrleansConfigurationForTesting.xml"),
                LivenessType = Orleans.Runtime.Configuration.GlobalConfiguration.LivenessProviderType.MembershipTableGrain,
                StartClient = true
            };

            var clientOptions = new TestingClientOptions
            {
                ProxiedGateway = true,
                Gateways = new List<IPEndPoint>(new[]
                    {
                        new IPEndPoint(IPAddress.Loopback, TestingSiloHost.ProxyBasePort),
                    }),
                PreferedGatewayIndex = 0
            };
            ClusterConfig = new ClusterConfiguration();
            ClusterConfig.LoadFromFile(new FileInfo("OrleansConfigurationForConsulTesting.xml").FullName);
            ClusterConfig.Globals.DataConnectionString = $"host=localhost;datacenter=dc1;mode={mode}";
            ClusterConfig.Globals.DataConnectionStringForReminders = $"host=localhost;datacenter=dc1;mode={mode}";
            SiloHost = new MyTestingHost(SiloHostConfig, clientOptions);
            ConsulMembershipTable = new ConsulSystemStoreProvider();
        }
        [TestInitialize]
        public virtual void BeforeEachTest()
        {

            Init(1);
        }
        [TestCleanup]
        public virtual void AfterEachTest()
        {
            MyTestingHost.StopAllSilos();
            SiloHost = null;
            ConsulMembershipTable.DeleteMembershipTableEntries(ClusterConfig.Globals.DeploymentId);
        }

    }
}

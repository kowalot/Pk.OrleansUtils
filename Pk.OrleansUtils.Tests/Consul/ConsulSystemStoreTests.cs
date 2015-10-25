using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestingHost;
using Pk.OrleansUtils.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Tests
{
    [DeploymentItem("OrleansProviders.dll")]
    [DeploymentItem("Pk.OrleansUtils.dll")]
    [DeploymentItem("Pk.OrleansUtils.Consul.dll")]
    [DeploymentItem("Pk.OrleansUtils.ElasticSearch.dll")]
    [DeploymentItem("OrleansConfigurationForConsulTesting.xml")]
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [TestClass]
    public class ConsulSystemStoreTests 
    {
        public TestingSiloOptions SiloConfig { get; private set; }
        public MyTestingHost SiloHost { get; private set; }

        public ConsulSystemStoreTests()
            : base(
                    
                    )
        {
        }

        private void Init()
        {
            SiloConfig = new TestingSiloOptions
            {
                StartPrimary = true,
                ParallelStart = true,
                PickNewDeploymentId = true,
                StartFreshOrleans = true,
                StartSecondary = true,
                SiloConfigFile = new FileInfo("OrleansConfigurationForConsulTesting.xml"),
                LivenessType = Orleans.Runtime.Configuration.GlobalConfiguration.LivenessProviderType.Custom
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
            SiloHost = new MyTestingHost(SiloConfig, clientOptions);
        }

        [TestInitialize]
        public void BeforeEachTest()
        {
            Init();
        }
        [TestCleanup]
        public void AfterEachTest()
        {
            MyTestingHost.StopAllSilos();
        }

        [TestMethod]
        public  void ConsulSystemStore_FirstTest()
        {
            for (int i = 0; i < 10; i++)
            {
                var grain = SiloHost.GrainFactory.GetGrain<IMyGrain>(Guid.NewGuid());
                Thread.Sleep(3000);
                SiloHost.StartAdditionalSilos(1);
                grain.TaskDoSomething().Wait();
            }


            var silos = SiloHost.GetActiveSilos();

            Thread.Sleep(6000);
            MyTestingHost.StopAdditionalSilos();
            SiloHost.Logger.Info("Closing...");
        }


    }
}

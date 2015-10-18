using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestingHost;
using Pk.OrleansUtils.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Tests
{
    [TestClass]
    public class ConsulSystemStoreTests : TestingSiloHost
    {

        public ConsulSystemStoreTests()
            : base(
                    new TestingSiloOptions {
                            StartPrimary = true,
                            ParallelStart = true,
                            PickNewDeploymentId=true,
                            StartFreshOrleans=true,
                            StartSecondary = true,
                            SiloConfigFile = new FileInfo("OrleansConfigurationForConsulTesting.xml"),
                            LivenessType= Orleans.Runtime.Configuration.GlobalConfiguration.LivenessProviderType.Custom
                    }
                    )
        {
        }



        [TestMethod]
        public  void ConsulSystemStore_FirstTest()
        {
            for (int i = 0; i < 10; i++)
            {
                var grain = GrainFactory.GetGrain<IMyGrain>(Guid.NewGuid());
                Thread.Sleep(10000);
                StartAdditionalSilos(1);
                grain.TaskDoSomething().Wait();
            }

            var silos = this.GetActiveSilos();

            Thread.Sleep(60000);
            logger.Info("Closing...");
        }


        [ClassCleanup]
        public static void MyClassCleanup()
        {
            StopAllSilos();
        }
    }
}

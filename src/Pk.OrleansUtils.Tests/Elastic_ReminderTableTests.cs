using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Pk.OrleansUtils.ElasticSearch;
using Orleans.Runtime.Configuration;
using Orleans.Runtime;
using System.Threading.Tasks;
using Orleans.TestingHost;
using System.Net;
using System.IO;
using Pk.OrleansUtils.Interfaces;
using System.Threading;
using Nest;

namespace Pk.OrleansUtils.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [DeploymentItem("ElasticSearchSiloConfig.xml")]
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [DeploymentItem("OrleansProviders.dll")]
    [DeploymentItem("Pk.OrleansUtils.dll")]
    [DeploymentItem("Pk.OrleansUtils.Consul.dll")]
    [DeploymentItem("Pk.OrleansUtils.ElasticSearch.dll")]
    [TestClass]
    public class Elastic_ReminderTableTests 
    {


        public TestingSiloHost Host { get; set; }

        public IReminderTable TestReminderTable { get; set; }

        private static readonly string remindersIndex = "orleans_reminders";

        private static readonly TestingSiloOptions siloOptions = new TestingSiloOptions
        {
            StartFreshOrleans = true,
            StartPrimary = true,
            StartSecondary = false,
            SiloConfigFile = new FileInfo("ElasticSearchSiloConfig.xml"),
            LivenessType = GlobalConfiguration.LivenessProviderType.Custom,
            ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.Custom,
            DataConnectionString = $"index={remindersIndex};Host=localhost",
            StartClient=true
        };

        private static readonly TestingClientOptions clientOptions = new TestingClientOptions
        {
            ProxiedGateway = true,
            Gateways = new List<IPEndPoint>(new[]
                    {
                        new IPEndPoint(IPAddress.Loopback, TestingSiloHost.ProxyBasePort),
                    }),
            PreferedGatewayIndex = 0
            
        };
 

        private static void deleteTestIndices()
        {
            var elastic = new ElasticClient(new ConnectionSettings(new UriBuilder("http", "localhost", 9200, "", "").Uri, remindersIndex));

            var indexExists = elastic.IndexExists(remindersIndex);
            if (indexExists.Exists)
            {
                var deleteResponse = elastic.DeleteIndex(remindersIndex, d => d.Index(remindersIndex));
                if (!deleteResponse.IsValid)
                    throw new Exception("Initialization failed");
            }
        }


        public class MyTestSilo:TestingSiloHost
        {
            public MyTestSilo():base(siloOptions,clientOptions)
            {

            }
        }

     
        public void Initialize()
        {
            deleteTestIndices();
            Host = new MyTestSilo();
        }

        [TestMethod]
        public void ElasticsearchReminderTable_BasicTest()
        {
            Initialize();

            var longReminderGrain = GrainClient.GrainFactory.GetGrain<IReminderTest>("looong");
            longReminderGrain.RegisterReminder(6000).Wait();
            var reminderTest = GrainClient.GrainFactory.GetGrain<IReminderTest>("abc");
            reminderTest.RegisterReminder(60).Wait();
            Thread.Sleep(130 * 1000);
            var countTask = reminderTest.GetCount();
            countTask.Wait();
            Assert.IsTrue(countTask.Result == 2);
            var remindersCountTask = reminderTest.GetRemindersCount();
            remindersCountTask.Wait();
            Assert.IsTrue(remindersCountTask.Result == 0);
            var longTask = longReminderGrain.GetRemindersCount();
            longTask.Wait();
            Assert.IsTrue(longTask.Result == 1);
        }
    }
}

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
    public class ElasticsearchReminderStorageTests : TestingSiloHost
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) {
            //StopAllSilos();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        //Use TestInitialize to run code before running each test

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
            DataConnectionString = $"index={remindersIndex};Host=localhost"
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

        public ElasticsearchReminderStorageTests()
            : base(siloOptions,clientOptions)
        { }

        [TestInitialize()]
         public void MyTestInitialize() {
            var elastic = new ElasticClient(new ConnectionSettings(new UriBuilder("http","localhost",9200,"","").Uri,remindersIndex));

            var indexExists = elastic.IndexExists(remindersIndex);
            if (indexExists.Exists)
            {
                var deleteResponse =  elastic.DeleteIndex(remindersIndex, d=>d.Index(remindersIndex));
                if (!deleteResponse.IsValid)
                    throw new Exception("Initialization failed");
            }
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
         public void MyTestCleanup() {
        }

        #endregion

  
        [TestMethod]
        public void ElasticsearchReminderTable_BasicTest()
        {
            var reminderTest = GrainClient.GrainFactory.GetGrain<IReminderTest>("xxx");
            reminderTest.RegisterReminder(60).Wait();
            Thread.Sleep(130 * 1000);
            var countTask = reminderTest.GetCount();
            countTask.Wait();
            Assert.IsTrue(countTask.Result == 2);
            var remindersCountTask = reminderTest.GetRemindersCount();
            remindersCountTask.Wait();
            Assert.IsTrue(remindersCountTask.Result == 0);
        }
    }
}

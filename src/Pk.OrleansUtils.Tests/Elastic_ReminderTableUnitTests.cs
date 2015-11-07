using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.TestingHost;
using Pk.OrleansUtils.ElasticSearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Tests
{

    public class MyForGodSakePublicLogger:Logger
    {
        public MyForGodSakePublicLogger():base()
        {

        }


        public override Severity SeverityLevel
        {
            get
            {
                return Severity.Off;
            }
        }

        //
        // Summary:
        //     Output the specified message and Exception at Error log level with the specified
        //     log id value.
        public override  void Error(int logCode, string message, Exception exception = null) { }
        //
        // Summary:
        //     Output the specified message at Info log level.
        public override void Info(string format, params object[] args) { }
        //
        // Summary:
        //     Output the specified message at Info log level with the specified log id value.
        public override void Info(int logCode, string format, params object[] args) { }
        //
        // Summary:
        //     Output the specified message at Verbose log level.
        public override void Verbose(string format, params object[] args) { }
        //
        // Summary:
        //     Output the specified message at Verbose log level with the specified log id value.
        public override void Verbose(int logCode, string format, params object[] args) { }
        //
        // Summary:
        //     Output the specified message at Verbose2 log level.
        public override void Verbose2(string format, params object[] args) { }
        //
        // Summary:
        //     Output the specified message at Verbose2 log level with the specified log id
        //     value.
        public override void Verbose2(int logCode, string format, params object[] args) { }
        //
        // Summary:
        //     Output the specified message at Verbose3 log level.
        public override void Verbose3(string format, params object[] args) { }
        //
        // Summary:
        //     Output the specified message at Verbose3 log level with the specified log id
        //     value.
        public override void Verbose3(int logCode, string format, params object[] args) { }
        //
        // Summary:
        //     Output the specified message and Exception at Warning log level with the specified
        //     log id value.
        public override void Warn(int logCode, string message, Exception exception) { }
        //
        // Summary:
        //     Output the specified message at Warning log level with the specified log id value.
        public override void Warn(int logCode, string format, params object[] args) { }

    }


    [DeploymentItem("OrleansProviders.dll")]
    [DeploymentItem("Pk.OrleansUtils.dll")]
    [DeploymentItem("Pk.OrleansUtils.Consul.dll")]
    [DeploymentItem("Pk.OrleansUtils.ElasticSearch.dll")]
    [DeploymentItem("OrleansConfigurationForConsulTesting.xml")]
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [DeploymentItem("ElasticSearchSiloConfig.xml")]
    [TestClass]
    public class Elastic_ReminderTableUnitTests  {


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
   

    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    public static void MyClassCleanup()
    {
        TraceLogger.Initialize(new NodeConfiguration());
    }
  
  

    public IReminderTable TestReminderTable { get; set; }

    private static readonly string remindersIndex = "orleans_reminders";

  
    private readonly string ExampleGrainRefKeyString = "GrainReference=00000000000000000000000000000000060000006aa96326+abc";

   

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


        #endregion

        public ClusterConfiguration ClusterConfig { get; set; }

        public MyTestingHost SiloHost { get; set; }


      

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void BeforeEachTest()
        {

            var siloOptions = new TestingSiloOptions
            {
                StartFreshOrleans = true,
                StartPrimary = true,
                StartSecondary = false,
                SiloConfigFile = new FileInfo("OrleansConfigurationForConsulTesting.xml"),
                LivenessType = GlobalConfiguration.LivenessProviderType.MembershipTableGrain,
                ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain,
                DataConnectionString = $"index={remindersIndex};Host=localhost"
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
            deleteTestIndices();
            ClusterConfig = new ClusterConfiguration();
            ClusterConfig.LoadFromFile(siloOptions.SiloConfigFile.FullName);
            TestReminderTable = new ElasticReminderTable();
            ClusterConfig.Globals.DataConnectionStringForReminders = $"index={ remindersIndex};Host=localhost";
            SiloHost = new MyTestingHost(siloOptions,clientOptions);
        }

        [TestCleanup]
        public void AfterEachTest()
        {
            MyTestingHost.StopAllSilos();
        }


        [TestMethod]
        public void ElasticsearchReminderTable_Initialization()
        {
            var tableTask = TestReminderTable.Init(ClusterConfig.Globals,SiloHost.Logger);
            tableTask.Wait();
        }

        [TestMethod]
        public void ElasticsearchReminderTable_AddReminder()
        {
            var tableTask = TestReminderTable.Init(ClusterConfig.Globals, SiloHost.Logger);
            tableTask.Wait();
            var new1 = new ReminderEntry() { ETag = null, GrainRef = GrainReference.FromKeyString(ExampleGrainRefKeyString), Period = TimeSpan.FromMinutes(1), ReminderName = "test", StartAt = DateTime.UtcNow };
            var upsertTask = TestReminderTable.UpsertRow(new1);
            upsertTask.Wait();
            Assert.IsFalse(String.IsNullOrEmpty(upsertTask.Result));
        }

        [TestMethod]
        public void ElasticsearchReminderTable_ReadRow()
        {
            var tableTask = TestReminderTable.Init(ClusterConfig.Globals, SiloHost.Logger);
            tableTask.Wait();
            var new1 = new ReminderEntry() { ETag = null, GrainRef = GrainReference.FromKeyString(ExampleGrainRefKeyString), Period = TimeSpan.FromMinutes(1), ReminderName = "ElasticsearchReminderTable_ReadRow", StartAt = DateTime.UtcNow };
            var upsertTask = TestReminderTable.UpsertRow(new1);
            upsertTask.Wait();
            Assert.IsFalse(String.IsNullOrEmpty(upsertTask.Result));
            var readTask = TestReminderTable.ReadRow(GrainReference.FromKeyString(ExampleGrainRefKeyString), "ElasticsearchReminderTable_ReadRow");
            readTask.Wait();
            Assert.IsTrue(readTask.Result!=null);
        }


    }
}

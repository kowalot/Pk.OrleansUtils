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

namespace Pk.OrleansUtils.Tests
{
    /// <summary>
    /// Test for Consul membership table provider
    /// </summary>
    [TestClass]
    [DeploymentItem("Pk.OrleansUtils.Consul.dll")]
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [DeploymentItem("OrleansConfigurationForTesting.xml")]
    [DeploymentItem("OrleansConfigurationForConsulTesting.xml")]
    [DeploymentItem("OrleansProviders.dll")]
    [DeploymentItem("Orleans.dll")]
    [DeploymentItem("OrleansRuntime.dll")]
    [DeploymentItem("Pk.OrleansUtils.dll")]
    [DeploymentItem("Pk.OrleansUtils.Consul.dll")]
    public class Consul_MembershipTableTests
    {

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            TraceLogger.Initialize(new NodeConfiguration());
        }


        public ConsulSystemStoreProvider ConsulMembershipTable { get; set; }

        public MyTestingHost SiloHost { get; set; }
        public TestingSiloOptions SiloHostConfig { get; private set; }

        public ClusterConfiguration ClusterConfig { get; set; }

        private void Init()
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
            ClusterConfig.Globals.DataConnectionStringForReminders = "host=localhost;datacenter=dc1";
            SiloHost = new MyTestingHost(SiloHostConfig, clientOptions);
            ConsulMembershipTable = new ConsulSystemStoreProvider();
        }
        [TestInitialize]
        public  void BeforeEachTest()
        {

            Init();
        }
        [TestCleanup]
        public void AfterEachTest()
        {
            MyTestingHost.StopAllSilos();
            SiloHost = null;
            ConsulMembershipTable.DeleteMembershipTableEntries(ClusterConfig.Globals.DeploymentId);
        }

        [TestMethod, TestCategory("Membership"), TestCategory("Consul")]
        public async Task MembershipTable_Consul_Init()
        {
            await ConsulMembershipTable.InitializeMembershipTable(ClusterConfig.Globals,true, SiloHost.Logger);
        }

        [TestMethod, TestCategory("Membership")]
        public async Task MembershipTable_Consul_ReadAll_EmptyTable()
        {
            await MembershipTable_Consul_Init();
            await ConsulMembershipTable.ReadAll();
        }

        [TestMethod, TestCategory("Membership"), TestCategory("Consul")]
        public async Task MembershipTable_Consul_InsertRow_ReadAll_ReadRow()
        {
            await MembershipTable_Consul_Init();
            var table = await ConsulMembershipTable.ReadAll();
            Assert.IsTrue(table.Members.Count == 0);
            var me = new MembershipEntry() {
                    FaultZone =0, HostName="xxx",
                    IAmAliveTime =DateTime.Today,
                    InstanceName ="instance1",
                    RoleName ="role",
                    ProxyPort =123,
                    SiloAddress = SiloAddress.FromParsableString("127.0.0.1:22223@183457693"), StartTime=DateTime.Now, Status= SiloStatus.Joining, SuspectTimes= new List<Tuple<SiloAddress, DateTime>>(), UpdateZone=0 };
            var ret = await ConsulMembershipTable.InsertRow(me,table.Version);
            Assert.IsTrue(ret);
            var refreshedTable = await ConsulMembershipTable.ReadAll();
            Assert.IsTrue(refreshedTable.Members.Count == 1);
            var entry = refreshedTable.Members.Select(t => t.Item1).FirstOrDefault();
            var readRowTable = await ConsulMembershipTable.ReadRow(entry.SiloAddress);
            var storedEntry = readRowTable.Members.Select(t => t.Item1).FirstOrDefault();
            Assert.AreEqual(entry.SiloAddress.ToParsableString(), storedEntry.SiloAddress.ToParsableString());
        }

        [TestMethod, TestCategory("Membership"), TestCategory("Consul")]
        public async Task MembershipTable_Consul_ReadAll_InsertRow_UpdateRow()
        {
            await MembershipTable_Consul_Init();
            var table = await ConsulMembershipTable.ReadAll();
            Assert.IsTrue(table.Members.Count == 0);
            var me = new MembershipEntry()
            {
                FaultZone = 0,
                HostName = "xxx",
                IAmAliveTime = DateTime.Today,
                InstanceName = "instance2",
                RoleName = "role",
                ProxyPort = 123,
                SiloAddress = SiloAddress.FromParsableString("127.0.0.1:22223@12345"),
                StartTime = DateTime.Now,
                Status = SiloStatus.Joining,
                SuspectTimes = new List<Tuple<SiloAddress, DateTime>>(),
                UpdateZone = 0
            };
            var ret = await ConsulMembershipTable.InsertRow(me, table.Version);
            Assert.IsTrue(ret);
            var refreshedTable = await ConsulMembershipTable.ReadAll();
            Assert.IsTrue(refreshedTable.Members.Count == 1);
            var entry = refreshedTable.Members.Select(t => t.Item1).FirstOrDefault();
            var iamAliveDate = DateTime.Parse(DateTime.UtcNow.ToString());
            entry.IAmAliveTime = iamAliveDate;
            var updateStatus = await ConsulMembershipTable.UpdateRow(entry, refreshedTable.Version.VersionEtag, refreshedTable.Version);
            Assert.IsTrue(updateStatus);
            var updatedEntryTable = await ConsulMembershipTable.ReadRow(entry.SiloAddress);
            var updatedEntry = refreshedTable.Members.Select(t => t.Item1).FirstOrDefault();
            Assert.AreEqual(iamAliveDate, updatedEntry.IAmAliveTime);
        }


        [TestMethod, TestCategory("Membership"), TestCategory("Consul")]
        public async Task MembershipTable_Consul_ReadAll_InsertRow_UpdateIAmAlive_DeleteRow()
        {
            await MembershipTable_Consul_Init();
            var table = await ConsulMembershipTable.ReadAll();
            Assert.IsTrue(table.Members.Count == 0);
            var me = new MembershipEntry()
            {
                FaultZone = 0,
                HostName = "xxx",
                IAmAliveTime = DateTime.Parse("1900-01-01"),
                InstanceName = "instance2",
                RoleName = "role",
                ProxyPort = 123,
                SiloAddress = SiloAddress.FromParsableString("127.0.0.1:22223@12345"),
                StartTime = DateTime.Now,
                Status = SiloStatus.Joining,
                SuspectTimes = new List<Tuple<SiloAddress, DateTime>>(),
                UpdateZone = 0
            };
            var ret = await ConsulMembershipTable.InsertRow(me, table.Version);
            Assert.IsTrue(ret);
            var refreshedTable = await ConsulMembershipTable.ReadAll();
            Assert.IsTrue(refreshedTable.Members.Count == 1);
            var entry = refreshedTable.Members.Select(t => t.Item1).FirstOrDefault();
            var iamAliveDate = DateTime.UtcNow;
            iamAliveDate = DateTime.Parse(iamAliveDate.ToString());//TRICKY: because miliseconds are NOT stored so Assert wouldnt work
            await ConsulMembershipTable.UpdateIAmAlive(entry);
            var updatedEntryTable = await ConsulMembershipTable.ReadRow(entry.SiloAddress);
            var updatedEntry = updatedEntryTable.Members.Select(t => t.Item1).FirstOrDefault();
            Assert.IsTrue(updatedEntry.IAmAliveTime>=iamAliveDate);
        }
    }
}

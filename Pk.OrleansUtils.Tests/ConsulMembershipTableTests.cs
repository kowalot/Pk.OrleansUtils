﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.TestingHost;


namespace Pk.OrleansUtils.Tests
{
    /// <summary>
    /// Tests for operation of Orleans SiloInstanceManager using ZookeeperStore - Requires access to external Zookeeper storage
    /// </summary>
    [TestClass]
    [DeploymentItem("Pk.OrleansUtils.Consul.dll")]
    public class ConsulMembershipTableTests
    {
        public TestContext TestContext { get; set; }

        private string deploymentId;
        private SiloAddress siloAddress;
        private IMembershipTable membership;
        private static readonly TimeSpan timeout = TimeSpan.FromMinutes(1);


        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            TraceLogger.Initialize(new NodeConfiguration());
        }

        private async Task Initialize()
        {
            //deploymentId = "test-" + Guid.NewGuid();
            //int generation = SiloAddress.AllocateNewGeneration();
            //siloAddress = SiloAddress.NewLocalAddress(generation);

            //GlobalConfiguration config = new GlobalConfiguration
            //{
            //    DeploymentId = deploymentId,
            //    DataConnectionString = StorageTestConstants.GetZooKeeperConnectionString()
            //};

            //var mbr = AssemblyLoader.LoadAndCreateInstance<IMembershipTable>(Constants.ORLEANS_ZOOKEEPER_UTILS_DLL, logger);
            //await mbr.InitializeMembershipTable(config, true, logger).WithTimeout(timeout);
            //membership = mbr;
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void TestCleanup()
        {
            //if (membership != null && SiloInstanceTableTestConstants.DeleteEntriesAfterTest)
            //{
            //    membership.DeleteMembershipTableEntries(deploymentId).Wait();
            //    membership = null;
            //}
        }

        [TestMethod, TestCategory("Membership"), TestCategory("ZooKeeper")]
        public async Task MembershipTable_ZooKeeper_Init()
        {
          //  await Initialize();
          //  Assert.IsNotNull(membership, "Membership Table handler created");
        }

        [TestMethod, TestCategory("Membership"), TestCategory("ZooKeeper")]
        public async Task MembershipTable_ZooKeeper_ReadAll_EmptyTable()
        {
           // await Initialize();
           // await MembershipTablePluginTests.MembershipTable_ReadAll_EmptyTable(membership);
        }

        [TestMethod, TestCategory("Membership"), TestCategory("ZooKeeper")]
        public async Task MembershipTable_ZooKeeper_InsertRow()
        {
           // await Initialize();
            //await MembershipTablePluginTests.MembershipTable_InsertRow(membership);
        }

        [TestMethod, TestCategory("Membership"), TestCategory("ZooKeeper")]
        public async Task MembershipTable_ZooKeeper_ReadRow_Insert_Read()
        {
           // await Initialize();
//await MembershipTablePluginTests.MembershipTable_ReadRow_Insert_Read(membership);
        }

        [TestMethod, TestCategory("Membership"), TestCategory("ZooKeeper")]
        public async Task MembershipTable_ZooKeeper_ReadAll_Insert_ReadAll()
        {
            //await Initialize();
            //await MembershipTablePluginTests.MembershipTable_ReadAll_Insert_ReadAll(membership);
        }

        [TestMethod, TestCategory("Membership"), TestCategory("ZooKeeper")]
        public async Task MembershipTable_ZooKeeper_UpdateRow()
        {
            //await Initialize();
            //await MembershipTablePluginTests.MembershipTable_UpdateRow(membership);
        }
    }
}

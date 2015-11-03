using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Tests.Consul
{
    [DeploymentItem("OrleansProviders.dll")]
    [DeploymentItem("Pk.OrleansUtils.dll")]
    [DeploymentItem("Pk.OrleansUtils.Consul.dll")]
    [DeploymentItem("Pk.OrleansUtils.ElasticSearch.dll")]
    [DeploymentItem("OrleansConfigurationForConsulTesting.xml")]
    [DeploymentItem("OrleansConfigurationForTesting.xml")]
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [TestClass]
    public class OrleansAsConsulServiceTests : Consul_MembershipTableTests
    {

        [TestInitialize]
        public override void BeforeEachTest()
        {
            Init(2);// Orleans as consul service
        }

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            TraceLogger.Initialize(new NodeConfiguration());
        }


        [TestMethod, TestCategory("Membership"), TestCategory("Consul")]
        public async Task OrleansAsConsulService_Init()
        {
            await ConsulMembershipTable.InitializeMembershipTable(ClusterConfig.Globals, true, SiloHost.Logger);
        }

        [TestMethod, TestCategory("Membership"), TestCategory("Consul")]
        public async Task OrleansAsConsulService_ReadAll_InsertRow_UpdateIAmAlive_DeleteRow()
        {
            await base.MembershipTable_Consul_ReadAll_InsertRow_UpdateIAmAlive_DeleteRow();
        }
    }
}

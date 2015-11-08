using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pk.OrleansUtils.ElasticSearch;
using NSubstitute;
using Orleans.Providers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans.Runtime;
using Orleans.Storage;
using Pk.OrleansUtils.Tests.Elastic.TestDomain;
using Nest;
using FluentAssertions;

namespace Pk.OrleansUtils.Tests
{
    [TestClass]
    public class Elastic_StorageProviderTests
    {
        private readonly string TEST_CONNECTION_STRING = "index=orleans;Host=localhost";
        private readonly string ExampleGrainRefKeyString = "GrainReference=00000000000000000000000000000000060000006aa96326+abc";

        public IProviderConfiguration Configuration { get; private set; }
        public Logger LoggerObj { get; private set; }
        public IProviderRuntime Runtime { get; private set; }

        public IStorageProvider Provider { get; set; }
        public ElasticStorageProvider ProviderExt { get; set; }

        private  void deleteTestIndices()
        {
            var ci = ElasticStorageProvider.FromConnectionString<ConnectionInfo>(TEST_CONNECTION_STRING);
            var cs = new ConnectionSettings(new UriBuilder("http", ci.Host, ci.Port, "", "").Uri, ci.Index);
            var elastic = new ElasticClient(cs);

            var indexExists = elastic.IndexExists(ci.Index);
            if (indexExists.Exists)
            {
                var deleteResponse = elastic.DeleteIndex(ci.Index, d => d.Index(ci.Index));
                if (!deleteResponse.IsValid)
                    throw new Exception("Initialization failed");
            }
        }
        [TestInitialize]
        public void BeforeEachTest()
        {
            deleteTestIndices();
        }


        private void InitilizeProviderHost()
        {
            // Arrange
            Runtime = Substitute.For<IProviderRuntime>();
            LoggerObj = Substitute.For<Logger>();
            Configuration = Substitute.For<IProviderConfiguration>();
            IDictionary<string, string> dic1 = new Dictionary<string, string>();
            dic1["DataConnectionString"] = TEST_CONNECTION_STRING;
            Configuration.Properties.Returns(new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(dic1));
            Runtime.GetLogger(Arg.Any<string>()).Returns(LoggerObj);
        }

       

        [TestMethod]
        public async Task Storage_Init()
        {
            //Arrange
            InitilizeProviderHost();
            Provider = new ElasticStorageProvider();
            //Act
            await Provider.Init("test", Runtime,Configuration);
            //Assert

        }

        [TestMethod]
        public async Task Storage_WriteAsync_ShouldSave_GrainState_FirstTime_UnderOptimisticLockingControl()
        {
            //Arrange
            InitilizeProviderHost();
            ProviderExt = new ElasticStorageProvider();
            Provider = ProviderExt;
            var grainState = new MyTestGrainState();
            var currentVersion = grainState.Version;

            //Act
            await ProviderExt.Init("test", Runtime, Configuration);
            // due to difficulties in creating grainReference as object we test internal WriteStateToElasticAsync
            await ProviderExt.WriteStateToElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, grainState);

            //Assert
            grainState.Version.Should().BeGreaterThan(currentVersion);
        }

        [TestMethod]
        public async Task Storage_WriteAsync_Should_Allow_Store_updated_GrainState_UnderOptimisticLockingControl()
        {
            //Arrange
            InitilizeProviderHost();
            ProviderExt = new ElasticStorageProvider();
            Provider = ProviderExt;
            var grainState = new MyTestGrainState();
            await ProviderExt.Init("test", Runtime, Configuration);
            // due to difficulties in creating grainReference as object we test internal WriteStateToElasticAsync
            await ProviderExt.WriteStateToElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, grainState);
            var currentVersion = grainState.Version;

            //Act
            grainState.MyInt = 4;
            await ProviderExt.WriteStateToElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, grainState);

            //Assert
            grainState.Version.Should().BeGreaterThan(currentVersion);
        }

        [TestMethod]
        [ExpectedException(typeof(ElasticsearchStorageException))]
        public async Task Storage_WriteAsync_Should_NOT_Allow_Store_updated_GrainState_WithIncorrectVersion()
        {
            //Arrange
            InitilizeProviderHost();
            ProviderExt = new ElasticStorageProvider();
            Provider = ProviderExt;
            var grainState = new MyTestGrainState();
            await ProviderExt.Init("test", Runtime, Configuration);
            // due to difficulties in creating grainReference as object we test internal WriteStateToElasticAsync
            await ProviderExt.WriteStateToElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, grainState);
            grainState.Version = 222;//incorrect version

            //Act
            grainState.MyInt = 4;
            await ProviderExt.WriteStateToElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, grainState);

            //Assert
            // throw ElasticsearchStorageException
        }


        [TestMethod]
        public async Task Storage_ReadStateAsync_Should_Read_GrainState()
        {
            //Arrange
            InitilizeProviderHost();
            ProviderExt = new ElasticStorageProvider();
            Provider = ProviderExt;
            var grainState = new MyTestGrainState() {  Etag="xxx", MyString="mystring", MyInt=1};
            await ProviderExt.Init("test", Runtime, Configuration);
            // due to difficulties in creating grainReference as object we test internal WriteStateToElasticAsync
            await ProviderExt.WriteStateToElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, grainState);

            //Act
            var anotherCopy = new MyTestGrainState();
            await ProviderExt.ReadStateFromElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, anotherCopy);

            //Assert
            anotherCopy.ShouldBeEquivalentTo(grainState, x=>x.Excluding(t=>t.Etag));
        }


        [TestMethod]
        public async Task Storage_ClearStateAsync_Should_DeleteStored_GrainState()
        {
            //Arrange
            InitilizeProviderHost();
            ProviderExt = new ElasticStorageProvider();
            Provider = ProviderExt;
            var grainState = new MyTestGrainState() { Etag = "xxx", MyString = "mystring", MyInt = 1 };
            await ProviderExt.Init("test", Runtime, Configuration);
            // due to difficulties in creating grainReference as object we test internal WriteStateToElasticAsync
            await ProviderExt.WriteStateToElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, grainState);

            //Act
            await ProviderExt.ClearStateInElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, grainState);

            //Assert
            var anotherCopy = new MyTestGrainState();
            var newObject = new MyTestGrainState();
            await ProviderExt.ReadStateFromElasticAsync(nameof(MyGrainState), ExampleGrainRefKeyString, anotherCopy);
            anotherCopy.ShouldBeEquivalentTo(newObject, x => x.Excluding(t => t.Etag));
        }

    }
}

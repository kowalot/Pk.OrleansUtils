using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pk.OrleansUtils.Consul;
using Newtonsoft.Json;
using FluentAssertions;

namespace Pk.OrleansUtils.Tests
{
    /// <summary>
    /// Summary description for KVEntryTests
    /// </summary>
    [TestClass]
    public class KVEntryTests
    {
        public KVEntryTests()
        {
       
        }



        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        class JsonClass
        {
            public int A { get; set; }
        }

        [TestMethod]
        public void KVEntry_ValueCanBeDeserializedFromJson()
        {
            var new1 = KVEntry.CreateForKey("id");
            new1.SetValue("{ A:1 }");
            var val = new1.GetValueAsObject<JsonClass>();
            Assert.IsTrue(val != null);
            Assert.IsTrue(val.A==1);
        }

        [TestMethod]
        public void KVEntry_CanBeCreatedForKeyNameWithDefaultValues()
        {
            var new1 = KVEntry.CreateForKey("id");
            Assert.IsTrue(new1.Key =="id");
            Assert.IsTrue(new1.CreateIndex == 0);
            Assert.IsTrue(new1.LockIndex == 0);
            Assert.IsTrue(new1.ModifyIndex == 0);
            Assert.IsTrue(new1.Flags == 0);
        }


        [TestMethod]
        public void KVEntry_CanCreateValidMultilevelKey()
        {
            var key = KVEntry.GetKey("level1", "level2", "level3");
            //Assert
            key.Should().Be("level1/level2/level3");
        }


        [TestMethod]
        public void KVEntry_CanExtractSubKeyAndCheckIfItsFromCertainFolder()
        {
            //Arrange
            var sut = KVEntry.CreateForKey("level1", "level2", "level3");
            var subKey = "";
            //Act
            var ret = sut.IsSubKeyOf(out subKey, "level1", "level2");
            //Assert
            ret.Should().BeTrue();
            subKey.Should().Be("level3");

         
        }

        [TestMethod]
        public void KVEntry_CanExtractSubKeyAndCheckIfItsNotFromCertainFolder()
        {
            //Arrange
            var sut = KVEntry.CreateForKey("level1", "level2", "level3");
            var subKey2 = "";
            //Act
            var ret2 = sut.IsSubKeyOf(out subKey2, "level1");
            // Assert
            ret2.Should().BeFalse();
            subKey2.Should().BeEmpty();
        }


        [TestMethod]
        public void KVEntry_CanBeDeserializedFromConsulKVEntry()
        {
            //Arrange
            var entryJson = "{\"CreateIndex\":2569,\"ModifyIndex\":11,\"LockIndex\":1,\"Key\":\"MembershipTable/testdepid-2015-10-18-12-08-35-102-281/IAmAlive/127.0.0.1:22222@182866116\",\"Flags\":0,\"Value\":\"MjAxNS0xMC0xOCAxMjoxMDowMQ == \"}";
            //Act
            var kv = JsonConvert.DeserializeObject<KVEntry>(entryJson);
            //Assert
            kv.Should().NotBeNull();
            kv.CreateIndex.Should().Be(2569);
            kv.ModifyIndex.Should().Be(11);
            kv.LockIndex.Should().Be(1);
            kv.Key.Should().Be("MembershipTable/testdepid-2015-10-18-12-08-35-102-281/IAmAlive/127.0.0.1:22222@182866116");
            kv.Value.Should().Be("2015-10-18 12:10:01");
        }
    }
}

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pk.OrleansUtils.Consul;
using Newtonsoft.Json;

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
            Assert.IsTrue(key == "level1/level2/level3");
        }


        [TestMethod]
        public void KVEntry_CanExtractSubKeyAndCheckIfItsFromCertainFolder()
        {
            var new1 = KVEntry.CreateForKey("level1", "level2", "level3");
            var subKey = "";
            Assert.IsTrue(new1.IsSubKeyOf(out subKey, "level1","level2"));
            Assert.IsTrue(subKey == "level3");
            subKey = "";
            Assert.IsFalse(new1.IsSubKeyOf(out subKey, "level1"));
            Assert.IsTrue(subKey == "");
        }
        [TestMethod]
        public void KVEntry_CanBeDeserializedFromConsulKVEntry()
        {
            var entryJson = "{\"CreateIndex\":2569,\"ModifyIndex\":11,\"LockIndex\":1,\"Key\":\"MembershipTable/testdepid-2015-10-18-12-08-35-102-281/IAmAlive/127.0.0.1:22222@182866116\",\"Flags\":0,\"Value\":\"MjAxNS0xMC0xOCAxMjoxMDowMQ == \"}";
            var kv = JsonConvert.DeserializeObject<KVEntry>(entryJson);
            Assert.IsTrue(kv.CreateIndex==2569);
            Assert.IsTrue(kv.ModifyIndex == 11);
            Assert.IsTrue(kv.LockIndex == 1);
            Assert.IsTrue(kv.Key == "MembershipTable/testdepid-2015-10-18-12-08-35-102-281/IAmAlive/127.0.0.1:22222@182866116");
            Assert.IsTrue(kv.Value== "2015-10-18 12:10:01");
        }
    }
}

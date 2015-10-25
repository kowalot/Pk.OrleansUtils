using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pk.OrleansUtils.Consul;
using System.Linq;

namespace Pk.OrleansUtils.Tests
{
    /// <summary>
    /// Summary description for ConsulClientTests
    /// </summary>
    [TestClass]
    public class ConsulClientTests
    {
        public ConsulClientTests()
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
        [TestInitialize()]
        public void MyTestInitialize() {
            var cl = new ConsulClient(new ConsulConnectionInfo());
            cl.DeleteKV("Pk_OrleansUtils_Tests/",new { recurse=1 }).Wait();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup() {
            var cl = new ConsulClient(new ConsulConnectionInfo());
            cl.DeleteKV("Pk_OrleansUtils_Tests/", new { recurse=1 }).Wait();
        }
        
        #endregion

        [TestMethod]
        public void ConsulClient_CanCreateKVEntriesAndReadAllOfThem()
        {
            var cl = new ConsulClient(new ConsulConnectionInfo());
            var statusTask =  cl.PutKV(KVEntry.CreateForKey("Pk_OrleansUtils_Tests", "ConsulClient_CanCreateKVEntriesAndReadAllOfThem", "KeyA"));
            statusTask.Wait();
            Assert.IsTrue(statusTask.Result);
            var statusTask2 = cl.PutKV(KVEntry.CreateForKey("Pk_OrleansUtils_Tests", "ConsulClient_CanCreateKVEntriesAndReadAllOfThem", "KeyB"));
            statusTask2.Wait();
            Assert.IsTrue(statusTask2.Result);
            var readAllTask = cl.ReadKVEntries(new  { recurse=1 },"Pk_OrleansUtils_Tests", "ConsulClient_CanCreateKVEntriesAndReadAllOfThem");
            readAllTask.Wait();
            Assert.IsTrue(readAllTask.Result.Count() == 2);
            var subKey = "";
            foreach (var kv in readAllTask.Result)
            {
                Assert.IsTrue(kv.IsSubKeyOf(out subKey, "Pk_OrleansUtils_Tests", "ConsulClient_CanCreateKVEntriesAndReadAllOfThem"));
                Assert.IsTrue(new string[] { "KeyA", "KeyB" }.Contains(subKey));
            }
        }

        [TestMethod]
        public void ConsulClient_CanUpdateUsingCheckAndSetFeature()
        {
            var arr = new string[] { "Pk_OrleansUtils_Tests", "ConsulClient_CanUpdateUsingCASFeature", "CAS" };
            var cl = new ConsulClient(new ConsulConnectionInfo());
            var statusTask = cl.PutKV(KVEntry.CreateForKey(arr));
            statusTask.Wait();
            var storedEntries = cl.ReadKVEntries(null, arr);
            storedEntries.Wait();
            var e = storedEntries.Result.FirstOrDefault();
            Assert.IsTrue(e != null);
            e.SetValue("bingo");
            var updateTask = cl.PutKV(e, new { cas = e.ModifyIndex });
            updateTask.Wait();
            Assert.IsTrue(updateTask.Result);
            var e2Task = cl.ReadKVEntries(null, arr);
            e2Task.Wait();
            var e2 = e2Task.Result.FirstOrDefault();
            Assert.IsTrue(e2.Value == "bingo");
            var notGonnaHappenValue = "It MUST not be saved";
            e.SetValue(notGonnaHappenValue);
            var updateTask2 = cl.PutKV(e, new { cas = e.ModifyIndex });//Wrong version
            updateTask2.Wait();
            Assert.IsFalse(updateTask2.Result);
            var notGonnaKVTask = cl.ReadKVEntries(null, arr);
            notGonnaKVTask.Wait();
            var notGonnaKV = notGonnaKVTask.Result.FirstOrDefault();
            Assert.IsTrue(notGonnaKV.Value!=notGonnaHappenValue);
            Assert.IsTrue(notGonnaKV.Value == "bingo");
            Assert.IsTrue(notGonnaKV.ModifyIndex == e2.ModifyIndex);
        }

    }
}

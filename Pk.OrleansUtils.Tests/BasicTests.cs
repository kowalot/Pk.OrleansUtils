using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestingHost;

namespace Pk.OrleansUtils.Tests
{
    [TestClass]
    public class BasicTests : TestingSiloHost
    {

        public BasicTests()
            : base(new TestingSiloOptions { StartPrimary = true, StartSecondary = false })
        {
        }

       

        [TestMethod]
        public void TestMethod1()
        {
        }


        [ClassCleanup]
        public static void MyClassCleanup()
        {
            StopAllSilos();
        }
    }
}

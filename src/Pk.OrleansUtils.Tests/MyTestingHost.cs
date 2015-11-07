using Orleans.Runtime;
using Orleans.TestingHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Tests
{
    public class MyTestingHost : TestingSiloHost
    {
        private TestingClientOptions clientOptions;
        private TestingSiloOptions siloOptions;

        public MyTestingHost(TestingSiloOptions siloOptions, TestingClientOptions clientOptions) :
            base(siloOptions, clientOptions)
        {
            this.siloOptions = siloOptions;
            this.clientOptions = clientOptions;


        }

        public TraceLogger Logger { get { return this.logger as TraceLogger; } }
    }
}

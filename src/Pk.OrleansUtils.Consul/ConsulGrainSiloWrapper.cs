using Orleans.Runtime.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulGrainSiloWrapper : IDisposable
    {
        private SiloHost siloHost;

        public ConsulGrainSiloWrapper()
        {

        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool dispose)
        {
            siloHost.Dispose();
            siloHost = null;
        }

        public bool Run()
        {
            throw new NotImplementedException();
        }
    }
}

using Orleans;
using Pk.OrleansUtils.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Tests.Elastic.TestDomain
{
    public class MyTestGrainState : GrainState, IOptimisticConcurrencyControl
    {
        public string MyString { get; set; }
        public int MyInt { get; set; }
        public List<Guid> MyIds { get; set; }

        private long _version = 0;
        public long Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = value;
            }
        }
    }
}

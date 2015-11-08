using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.ElasticSearch
{
    public interface IOptimisticConcurrencyControl
    {
        long Version { get; set; }
    }
}

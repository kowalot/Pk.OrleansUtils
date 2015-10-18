using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.ElasticSearch
{
    public class ConnectionInfo
    {
        public string IndexName { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public ConnectionInfo()
        {
            Host = "localhost";
            Port = 9200;
        }

        public bool IsValid()
        {
            return (!String.IsNullOrEmpty(IndexName) && !String.IsNullOrEmpty(Host) && Port > 0);
        }

    }
}
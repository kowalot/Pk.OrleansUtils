using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulServiceDescriptor
    {
//        {
//  "ID": "redis1",
//  "Name": "redis",
//  "Tags": [
//    "master",
//    "v1"
//  ],
//  "Address": "127.0.0.1",
//  "Port": 8000,
//  "Check": {
//    "Script": "/usr/local/bin/check_redis.py",
//    "HTTP": "http://localhost:5000/health",
//    "Interval": "10s",
//    "TTL": "15s"
//  }
//}

        public string ID { get; set; }

        public string Name { get; set; }

        public List<string> Tags { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public ConsulCheckDescriptor Check { get; set; }
    }
}

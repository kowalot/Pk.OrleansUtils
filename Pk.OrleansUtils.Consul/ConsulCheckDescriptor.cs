using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulCheckDescriptor
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Script { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HTTP { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Interval { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TTL { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceID { get; internal set; }
        public string Name { get; internal set; }
        public string Notes { get; internal set; }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulClient
    {
        private ConsulConnectionInfo consulInfo;
        public ConsulConnectionInfo ConsulInfo { get;}

        public const string ConsulProtocolDataMediaType = "application/json";
        public const string KV_ENTRIES = "kv";


        public ConsulClient(ConsulConnectionInfo consulInfo)
        {
            this.consulInfo = consulInfo;
        }

        private HttpClient UsingHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        public async Task<bool> PutKV(KVEntry entry)
        {
            using (var client = UsingHttpClient())
            {
                var keys = new List<string>();
                keys.Add(entry.Key);
                var uri = consulInfo.GetKVEntryUri(keys.ToArray());
                var httpContent = new StringContent(entry.Value ?? "", Encoding.UTF8, ConsulProtocolDataMediaType);
                var res = await client.PutAsync(uri, httpContent);
                return (res.StatusCode == HttpStatusCode.OK);
            }
        }

        public async Task<IEnumerable<KVEntry>> ReadKVEntries(object extraValues, params string[] keyPath)
        {

            using (var client = UsingHttpClient())
            {
                var uri = consulInfo.GetUri(KV_ENTRIES , extraValues, keyPath);
                var res = await client.GetAsync(uri);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    var entries = JsonConvert.DeserializeObject<IEnumerable<KVEntry>>(content);
                    return entries;
                }
                return new List<KVEntry>();
            }
        }
    }
}

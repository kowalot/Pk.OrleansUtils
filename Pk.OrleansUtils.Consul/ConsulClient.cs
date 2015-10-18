using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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

        public async Task<bool> PutKV(KVEntry entry, object extraParams=null )
        {
            using (var client = UsingHttpClient())
            {
                var uri = new UriBuilder("http", consulInfo.Host, consulInfo.Port, String.Format("/v{0}/{1}/{2}",
                                consulInfo.Version, KV_ENTRIES, entry.Key),
                                (extraParams==null) ? "" : GetExtraValuesFragment(extraParams)).Uri;
                var httpContent = new StringContent(entry.Value ?? "", Encoding.UTF8, ConsulProtocolDataMediaType);
                var res = await client.PutAsync(uri, httpContent);
                var responseContent = await res.Content.ReadAsStringAsync();
                return (res.StatusCode == HttpStatusCode.OK && Convert.ToBoolean(responseContent));
            }
        }

        public async Task<bool> DeleteKV(string path,object extraParams=null)
        {
            using (var client = UsingHttpClient())
            {
                //var uri =  GetUri(KV_ENTRIES, extraParams ?? new { }, keyPath); // GetKVEntryUri(keys.ToArray());
                var uri = new UriBuilder("http", consulInfo.Host, consulInfo.Port, String.Format("/v{0}/{1}/{2}", consulInfo.Version, KV_ENTRIES, path),
                    (extraParams==null) ? "" : GetExtraValuesFragment(extraParams)
                    ).Uri;
                var res = await client.DeleteAsync(uri);
                var responseContent = await res.Content.ReadAsStringAsync();
                return (res.StatusCode == HttpStatusCode.OK && Convert.ToBoolean(responseContent));
            }
        }

        internal string GetExtraValuesFragment(object extraValues)
        {
            if (extraValues == null) return "";
            var stringBuilder = new StringBuilder();
            var props = extraValues.GetType().GetProperties();
            stringBuilder.Append("?");
            foreach (var item in props)
            {
                var value = (item.GetValue(extraValues) != null) ? item.GetValue(extraValues).ToString() : "";
                stringBuilder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(item.Name), HttpUtility.UrlEncode(value));
            }
            return stringBuilder.ToString();
        }

        internal Uri GetUri(string commandType, object extraValues, params string[] path)
        {
          
            return new UriBuilder("http", consulInfo.Host, consulInfo.Port, String.Format("/v{0}/{1}/{2}", consulInfo.Version, commandType, String.Join(@"/", path)), GetExtraValuesFragment(extraValues)).Uri;
        }

        public async Task<IEnumerable<KVEntry>> ReadKVEntries(object extraValues, params string[] keyPath)
        {

            using (var client = UsingHttpClient())
            {
                var uri = GetUri(KV_ENTRIES , extraValues, keyPath);
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

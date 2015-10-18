using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulConnectionInfo
    {
        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 8500;

        public int Version { get; set; } = 1;

        public string Datacenter { get; set; } = "dc1";

        internal Uri GetDeleteKeyUri(object key)
        {
            return new UriBuilder("http", Host, Port, String.Format("/v{0}/kv/{1}/", Version, key)).Uri;
        }

        internal Uri GetCatalogUri(string key)
        {
            return new UriBuilder("http", Host, Port, String.Format("/v{0}/kv/{1}/", Version, key), "?recurse&dc="+Datacenter).Uri;
        }

        internal Uri GetKVEntryUri(params string[] path)
        {
            return new UriBuilder("http", Host, Port, String.Format("/v{0}/kv/{1}", Version, String.Join(@"/",path))).Uri;
        }

        internal Uri GetUri(string commandType, object extraValues, params string[] path)
        {
            var stringBuilder = new StringBuilder();
            var props = extraValues.GetType().GetProperties();
            stringBuilder.Append("?");
            foreach (var item in props)
            {
                var value = (item.GetValue(extraValues) != null) ? item.GetValue(extraValues).ToString() : "";
                stringBuilder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(item.Name),HttpUtility.UrlEncode(value));
            }

            return new UriBuilder("http", Host, Port, String.Format("/v{0}/{1}/{2}/", Version,commandType, String.Join(@"/", path)),stringBuilder.ToString()).Uri;
        }

    }
}

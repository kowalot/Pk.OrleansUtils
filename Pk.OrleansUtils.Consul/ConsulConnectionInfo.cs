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

        public string Datacenter { get; set; } = "";

        public static ConsulConnectionInfo FromConnectionString(string dataConnectionString)
        {
            var new1 =  ConsulConnectionInfo.FromConnectionString<ConsulConnectionInfo>(dataConnectionString);
            return new1;
        }

        private static T FromConnectionString<T>(string cs)
        where T : new()
        {
            var parts = cs.Split(';');
            var connectionInfo = new T();
            foreach (var part in parts)
            {
                var nv = part.Split('=');
                if (nv.Length == 2)
                {
                    var pi = connectionInfo.GetType().GetProperties().FirstOrDefault(t => t.Name.ToLowerInvariant() == nv[0]);
                    if (pi != null)
                    {
                        switch (Type.GetTypeCode(pi.PropertyType))
                        {
                            case TypeCode.Boolean:
                                pi.SetValue(connectionInfo, Boolean.Parse(nv[1]));
                                break;
                            case TypeCode.Int32:
                                pi.SetValue(connectionInfo, Int32.Parse(nv[1]));
                                break;
                            default:
                                pi.SetValue(connectionInfo, nv[1]);
                                break;
                        }
                    }
                }
            }
            return connectionInfo;
        }

        }
    }

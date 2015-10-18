using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Consul
{
    public class KVEntry
    {
        public const char CatalogSeparatorChar = '/';

        public int CreateIndex { get; set; }
        public int ModifyIndex { get; set; }
        public int LockIndex { get; set; }
        public string Key { get; set; }
        public int Flags { get; set; }


        private string _value = "";
        public string Value { get { return _value; }
            set
            {
                if (value != null)
                {
                    byte[] data = Convert.FromBase64String(value);
                    _value = Encoding.UTF8.GetString(data);
                }
                else
                    _value = "";
            }
        }
        public string Session { get; set; }

        public T GetValueAsObject<T>()
        {
            return JsonConvert.DeserializeObject<T>(Value ?? String.Empty);
        }
        public static string GetKey(params string[] keyPath)
        {
            var key = String.Join(CatalogSeparatorChar.ToString(), keyPath);
            return key;
        }


        public static KVEntry CreateForKey(params string[] keyPath)
        {
            var new1 = new KVEntry();
            new1.Key = GetKey(keyPath);
            return new1;
        }

        internal void SetValue(string v)
        {
            _value = v;
        }

        public bool IsSubKeyOf(out string subkeyName,params string[] keyPath)
        {
            subkeyName = "";
            var parts = Key.Split(CatalogSeparatorChar).ToArray();
            if (parts.Length == keyPath.Length+1)
            {
                for (int i = 0; i < keyPath.Length; i++)
                    if (parts[i] != keyPath[i]) return false;
                subkeyName = parts[keyPath.Length];
                return true;
            }
            return false;
        }

     
    }
}

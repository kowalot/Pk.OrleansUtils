using Newtonsoft.Json;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulMembershipTable
    {
        public Dictionary<string,ConsulMembershipEntry> Members { get; set; }

        public int Version { get; set; }

        public ConsulMembershipTable()
        {
            Members = new Dictionary<string, ConsulMembershipEntry>();
        }

        private KVEntry _kvEntry = null;
        public ConsulMembershipTable(KVEntry kvEntry)
        {
            _kvEntry = kvEntry;
            Members = new Dictionary<string, ConsulMembershipEntry>();
        }
        public void SetOriginKVEntry(KVEntry kvEntry)
        {
            _kvEntry = kvEntry;
        }

        internal MembershipTableData GetMembershipTableData()
        {
            var mb = new MembershipTableData(Members.Select(t => new Tuple<MembershipEntry,string>(t.Value.GetMembershipEntry(),t.Key)).ToList()
                                            , new TableVersion(Version,this._kvEntry.ModifyIndex.ToString()));
            return mb;
        }

        internal  Task<bool> Save(ConsulClient consul,TableVersion newVersion=null)
        {
            if (newVersion!=null)
            {
                Version = newVersion.Version;
            }
            _kvEntry.SetValue(JsonConvert.SerializeObject(this,Formatting.Indented));
            return  consul.PutKV(_kvEntry, (newVersion!=null) ? (object)new  { cas=newVersion.VersionEtag } : new object{ });
        }

        internal bool CanBeUpdate(TableVersion tableVersion)
        {
            return (_kvEntry.ModifyIndex.ToString() == tableVersion.VersionEtag);
        }
    }
}

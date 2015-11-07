using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulMembershipEntry
    {

        public ConsulMembershipEntry()
        {

        }

        public ConsulMembershipEntry(MembershipEntry entry)
        {
            this.FaultZone = entry.FaultZone;
            this.HostName = entry.HostName;
            this.IAmAliveTime = entry.IAmAliveTime;
            this.InstanceName = entry.InstanceName;
            this.ProxyPort = entry.ProxyPort;
            this.RoleName = entry.RoleName;
            this.SiloAddress = entry.SiloAddress.ToParsableString();
            this.StartTime = entry.StartTime;
            this.Status = entry.Status;
            this.SuspectTimes = entry.SuspectTimes.Select(t => new Tuple<string, DateTime>(t.Item1.ToParsableString(), t.Item2)).ToList();
            this.UpdateZone = entry.UpdateZone;
        }

        public int FaultZone { get; set; }
        public string HostName { get; set; }
        public DateTime IAmAliveTime { get; set; }
        public string InstanceName { get; set; }
        public int ProxyPort { get; set; }
        public string RoleName { get; set; }
        public string SiloAddress { get; set; }
        public DateTime StartTime { get; set; }
        public SiloStatus Status { get; set; }
        public List<Tuple<string, DateTime>> SuspectTimes { get; set; }
        public int UpdateZone { get; set; }

        internal MembershipEntry GetMembershipEntry()
        {
            var mse = new MembershipEntry();
            mse.FaultZone = this.FaultZone;
            mse.HostName = this.HostName;
            mse.IAmAliveTime = this.IAmAliveTime;
            mse.InstanceName = this.InstanceName;
            mse.ProxyPort = this.ProxyPort;
            mse.RoleName = this.RoleName;
            mse.SiloAddress = Orleans.Runtime.SiloAddress.FromParsableString(this.SiloAddress);
            mse.StartTime = this.StartTime;
            mse.Status = this.Status;
            mse.SuspectTimes = this.SuspectTimes.Select(t => new Tuple<SiloAddress, DateTime>(Orleans.Runtime.SiloAddress.FromParsableString(t.Item1), t.Item2)).ToList();
            mse.UpdateZone = this.UpdateZone;
            return mse;
        }
    }
}

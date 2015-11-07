using Orleans;
using Orleans.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;

namespace Pk.OrleansUtils.ElasticSearch
{
    public class ElasticMembershipTableProvider : IMembershipTable, IGatewayListProvider
    {
        public bool IsUpdatable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public TimeSpan MaxStaleness
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task DeleteMembershipTableEntries(string deploymentId)
        {
            throw new NotImplementedException();
        }

        public IList<Uri> GetGateways()
        {
            throw new NotImplementedException();
        }

        public Task InitializeGatewayListProvider(ClientConfiguration clientConfiguration, TraceLogger traceLogger)
        {
            throw new NotImplementedException();
        }

        public Task InitializeMembershipTable(GlobalConfiguration globalConfiguration, bool tryInitTableVersion, TraceLogger traceLogger)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InsertRow(MembershipEntry entry, TableVersion tableVersion)
        {
            throw new NotImplementedException();
        }

        public Task<MembershipTableData> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Task<MembershipTableData> ReadRow(SiloAddress key)
        {
            throw new NotImplementedException();
        }

        public Task UpdateIAmAlive(MembershipEntry entry)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateRow(MembershipEntry entry, string etag, TableVersion tableVersion)
        {
            throw new NotImplementedException();
        }
    }
}

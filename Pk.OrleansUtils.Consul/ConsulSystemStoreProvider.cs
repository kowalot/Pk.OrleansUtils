using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Messaging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulSystemStoreProvider : IMembershipTable, IGatewayListProvider
    {
        private TraceLogger logger;
        public ConsulClient Consul { get; set; }
       
        private const string ORLEANS_CATALOG_KEY = "MembershipTable";
        private const string ORLEANS_MEMBERS_SUBKEY = "Members";
        private const string ORLEANS_I_AM_ALIVE_FOLDER_KEY = "IAmAlive";
        private TimeSpan maxStaleness;

        public bool IsUpdatable
        {
            get
            {
                return true;
            }
        }

        public TimeSpan MaxStaleness
        {
            get
            {
                return maxStaleness;
            }
        }

        public string DeploymentId { get; private set; }
        public GlobalConfiguration GlobalConfiguration { get; private set; }

        public async Task DeleteMembershipTableEntries(string deploymentId)
        {
            var key = KVEntry.GetFolderKey(ORLEANS_CATALOG_KEY, DeploymentId);
            var deleteRet = await Consul.DeleteKV(key, new { recurse = 1 });
            if (logger.IsVerbose3)
                logger.Verbose3("DeleteMembershipTableEntries has been finished.");
        }


        public IList<Uri> GetGateways()
        {
            return ReadAll().Result.Members.Select(e => e.Item1).
                                          Where(m => m.Status == SiloStatus.Active && m.ProxyPort != 0).
                                          Select(m =>
                                          {
                                              m.SiloAddress.Endpoint.Port = m.ProxyPort;
                                              return m.SiloAddress.ToGatewayUri();
                                          }).ToList();
        }


        private async Task InitializeConfig(string dataConnectionString, bool tryInitTableVersion, TraceLogger traceLogger)
        {
            logger = traceLogger;
            if (logger.IsVerbose3)
                logger.Verbose3("ConsulSystemStoreProvider.InitializeMembershipTable called.");

            this.Consul = new ConsulClient(ConsulConnectionInfo.FromConnectionString(dataConnectionString));
            if (tryInitTableVersion)
            {
                // create new root catalog
                await CreateRootCatalog(tryInitTableVersion);
            }
          
        }

        public async Task InitializeGatewayListProvider(ClientConfiguration clientConfiguration, TraceLogger traceLogger)
        {
            DeploymentId = clientConfiguration.DeploymentId;
            maxStaleness = clientConfiguration.GatewayListRefreshPeriod;
            await InitializeConfig(clientConfiguration.DataConnectionString,false,traceLogger);

        }

        public async Task InitializeMembershipTable(GlobalConfiguration globalConfiguration, bool tryInitTableVersion, TraceLogger traceLogger)
        {
            DeploymentId = globalConfiguration.DeploymentId;
            this.GlobalConfiguration = globalConfiguration;
            await InitializeConfig(globalConfiguration.DataConnectionString, tryInitTableVersion, traceLogger);
            switch (Consul.ConsulInfo.Mode)
            {
                case ConsulIntegrationMode.AsConsulService:
                    if (!globalConfiguration.LivenessEnabled || globalConfiguration.IAmAliveTablePublishTimeout == TimeSpan.Zero)
                        throw new ConsulSystemStoreException.InvalidConfiguration("Liveness and IAmAliveTablePublishTimeout must be configured in selected system store mode.");
                    break;
                default:
                    break;
            }

        }

        private async Task<bool> RegisterSilo(MembershipEntry entry)
        {
            var service = new ConsulServiceDescriptor();
            service.ID = entry.InstanceName;
            service.Name = "orleans";
            service.Tags = new List<string>();
            service.Tags.Add(DeploymentId);
            service.Tags.Add("orleans");
            service.Tags.Add(entry.RoleName);
            service.Port = entry.SiloAddress.Endpoint.Port;


            var ret = await Consul.RegisterService(service);
            var check = new ConsulCheckDescriptor();
            check.ServiceID = service.ID;
            check.TTL = $"{GlobalConfiguration.IAmAliveTablePublishTimeout.TotalSeconds+1}s";// +1 just to avoid hazards
            check.Name = "Orleans silo availability";
            check.Notes = $"Generation:{entry.SiloAddress.Generation} Updated at:{DateTime.UtcNow.ToString("O")}";
            check.ID = GetCheckId(entry);
            var ret1 = await Consul.RegisterCheck(check);
            return ret && ret1;
        }

        private async Task CreateRootCatalog(bool recreate)
        {
            var newCatalogEntry = KVEntry.CreateForKey(ORLEANS_CATALOG_KEY , DeploymentId, ORLEANS_MEMBERS_SUBKEY);
            var consulTable = new ConsulMembershipTableEntry(newCatalogEntry);
            await consulTable.Save(Consul, new TableVersion(0,0.ToString()));
        }

   

        private async Task<IEnumerable<KVEntry>> ReadCatalogKVEntries()
        {
            var entries = await Consul.ReadKVEntries(new { recurse = 1 }, ORLEANS_CATALOG_KEY,DeploymentId);
            return entries;
        }
        public async Task<MembershipTableData> ReadAll()
        {
            var res= await BuildTableWhere(t=> (t!= null));
            return res.GetMembershipTableData();
        }

        protected async Task<ConsulMembershipTableEntry> BuildTableWhere(Func<ConsulMembershipEntry,bool> entryPredicate)
        {
            var res = await ReadCatalogKVEntries();
            var catalogKey = KVEntry.GetKey(ORLEANS_CATALOG_KEY, DeploymentId,ORLEANS_MEMBERS_SUBKEY);
            var catalogKVEntry = res.FirstOrDefault(t => t.Key == catalogKey);
            if (catalogKVEntry != null)
            {
                var consulTable = catalogKVEntry.GetValueAsObject<ConsulMembershipTableEntry>();
                if (entryPredicate != null)
                {
                    consulTable.Members = consulTable.Members.Values.Where(entryPredicate).ToDictionary(t => t.SiloAddress);
                }
                // Apply separate "dirty writes" IamAlive kvs
                var subKeyName = "";
                foreach (var kv in res)
                    if (kv.IsSubKeyOf(out subKeyName, ORLEANS_CATALOG_KEY, DeploymentId, ORLEANS_I_AM_ALIVE_FOLDER_KEY))
                    {
                        var silo = consulTable.Members.Values.FirstOrDefault(t => t.SiloAddress == subKeyName);
                        if (silo != null)
                            silo.IAmAliveTime = DateTime.Parse(kv.Value);
                    }
                consulTable.SetOriginKVEntry(catalogKVEntry);
                return consulTable;
            }
            else
                return new ConsulMembershipTableEntry();
        }


        /// <summary>
        /// It Should:
        /// Atomically tries to insert (add) a new MembershipEntry for one silo and also update the TableVersion.
        /// If operation succeeds, the following changes would be made to the table:
        /// 1) New MembershipEntry will be added to the table.
        /// 2) The newly added MembershipEntry will also be added with the new unique automatically generated eTag.
        /// 3) TableVersion.Version in the table will be updated to the new TableVersion.Version.
        /// 4) TableVersion etag in the table will be updated to the new unique automatically generated eTag.
        /// All those changes to the table, insert of a new row and update of the table version and the associated etags, should happen atomically, or fail atomically with no side effects.
        /// The operation should fail in each of the following conditions:
        /// 1) A MembershipEntry for a given silo already exist in the table
        /// 2) Update of the TableVersion failed since the given TableVersion etag (as specified by the TableVersion.VersionEtag property) did not match the TableVersion etag in the table.
        /// </summary>
        /// <param name="entry">MembershipEntry to be inserted.</param>
        /// <param name="tableVersion">The new TableVersion for this table, along with its etag.</param>
        /// <returns>True if the insert operation succeeded and false otherwise.</returns>
        public async Task<bool> InsertRow(MembershipEntry entry, TableVersion tableVersion)
        {
            var consulTable = await BuildTableWhere(t => true);
            if (consulTable.Members.ContainsKey(entry.SiloAddress.ToParsableString()))
                throw new ConsulSystemStoreException.DuplicatedEntry();
            consulTable.Members.Add(entry.SiloAddress.ToParsableString(),new ConsulMembershipEntry(entry));

            switch (Consul.ConsulInfo.Mode)
            {
                case ConsulIntegrationMode.AsConsulService:
                    // Register silo in local agent
                    if (!await RegisterSilo(entry))
                    {
                        throw new ConsulSystemStoreException.ConsulServiceRegistrationFailed();
                    }
                    break;
                default:
                    break;
            }

            return await consulTable.Save(Consul,tableVersion);
        }

        public async Task<MembershipTableData> ReadRow(SiloAddress key)
        {
            var res = await BuildTableWhere(y => y.SiloAddress == key.ToParsableString());
            return res.GetMembershipTableData();
        }

      
        public async Task UpdateIAmAlive(MembershipEntry entry)
        {
            if (logger.IsVerbose3)
                logger.Verbose3("ConsulSystemStoreProvider.UpdateIAmAlive called for "+entry.SiloAddress.ToParsableString());
            var keyPath = new string[] { ORLEANS_CATALOG_KEY, DeploymentId, ORLEANS_I_AM_ALIVE_FOLDER_KEY, entry.SiloAddress.ToParsableString() };
            var storedKV = (await Consul.ReadKVEntries(new { }, keyPath)).FirstOrDefault();
            KVEntry kv = storedKV ?? KVEntry.CreateForKey(keyPath);
            kv.SetValue(DateTime.UtcNow.ToString());

            switch (this.Consul.ConsulInfo.Mode)
            {
                case ConsulIntegrationMode.KV:
                    break;
                case ConsulIntegrationMode.AsConsulService:
                    if (!await Consul.CheckPass(GetCheckId(entry)))
                    {
                        this.logger.Warn(1,"Consul pass check failed.Silo may be unavailable for clients.");
                    }
                    break;
                default:
                    break;
            }
            await Consul.PutKV(kv);

        }

        private string GetCheckId(MembershipEntry entry)
        {
            return entry.SiloAddress.Endpoint.ToString().Replace(".", "_").Replace(":", "__");
        }

        /// <summary>
        /// Atomically tries to update the MembershipEntry for one silo and also update the TableVersion.
        /// If operation succeeds, the following changes would be made to the table:
        /// 1) The MembershipEntry for this silo will be updated to the new MembershipEntry (the old entry will be fully substitued by the new entry) 
        /// 2) The eTag for the updated MembershipEntry will also be eTag with the new unique automatically generated eTag.
        /// 3) TableVersion.Version in the table will be updated to the new TableVersion.Version.
        /// 4) TableVersion etag in the table will be updated to the new unique automatically generated eTag.
        /// All those changes to the table, update of a new row and update of the table version and the associated etags, should happen atomically, or fail atomically with no side effects.
        /// The operation should fail in each of the following conditions:
        /// 1) A MembershipEntry for a given silo does not exist in the table
        /// 2) A MembershipEntry for a given silo exist in the table but its etag in the table does not match the provided etag.
        /// 3) Update of the TableVersion failed since the given TableVersion etag (as specified by the TableVersion.VersionEtag property) did not match the TableVersion etag in the table.
        /// </summary>
        /// <param name="entry">MembershipEntry to be updated.</param>
        /// <param name="etag">The etag  for the given MembershipEntry.</param>
        /// <param name="tableVersion">The new TableVersion for this table, along with its etag.</param>
        /// <returns>True if the update operation succeeded and false otherwise.</returns>
        public async Task<bool> UpdateRow(MembershipEntry entry, string etag, TableVersion tableVersion)
        {
            var consulTable = await BuildTableWhere(t => true);
            var storedEntry = consulTable.Members.Values.FirstOrDefault(t=>t.SiloAddress == entry.SiloAddress.ToParsableString());
            if (storedEntry==null)
                /// 1) A MembershipEntry for a given silo does not exist in the table
                throw new ConsulSystemStoreException.UpdateRowFailedNoEntry();
            /// 2) A MembershipEntry for a given silo exist in the table but its etag in the table does not match the provided etag.
            /// 3) Update of the TableVersion failed since the given TableVersion etag (as specified by the TableVersion.VersionEtag property) did not match the TableVersion etag in the table.
            if (!consulTable.CanBeUpdated(tableVersion))// 2&3 because they are updated in Consul together
                throw new ConsulSystemStoreException.WrongVersionException();
            consulTable.Members.Remove(storedEntry.SiloAddress);
            consulTable.Members.Add(entry.SiloAddress.ToParsableString(), new ConsulMembershipEntry(entry));
            return await consulTable.Save(Consul,tableVersion);
        }
    }
}

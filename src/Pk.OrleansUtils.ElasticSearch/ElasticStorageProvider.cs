using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Nest;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Elasticsearch.Net;

namespace Pk.OrleansUtils.ElasticSearch
{
    /// <summary>
    /// Grain implementation class Grain1.
    /// </summary>
    public class ElasticStorageProvider : IStorageProvider
    {


        /// <summary>
        /// Logger object
        /// </summary>
        public Logger Log
        {
            get; protected set;
        }

        public ConnectionSettings ConnectionSettings { get; set; }

        private string _name = "";
        public string Name
        {
            get
            {
                return _name;
            }
        }

        public ConnectionInfo ConnectionStringInfo { get; private set; }


        public ElasticClient Client { get; protected set; }

        public ElasticStorageProvider()
        {

        }
        public ElasticStorageProvider(ElasticClient client)
        {
            Client = client;
        }

        internal async Task ClearStateInElasticAsync(string grainType, string key, GrainState grainState)
        {
            var res = await Client.DeleteAsync(ConnectionStringInfo.Index, GetElasticSearchType(grainType), key);

        }


        public async Task ClearStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var key = GetElasticSearchKey(grainReference);
            await ClearStateInElasticAsync(grainType, key, grainState);
        }

        public Task Close()
        {
            return TaskDone.Done;
        }

        public static T FromConnectionString<T>(string cs)
            where T :new()
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


        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            ConnectionStringInfo =  FromConnectionString<ConnectionInfo>(config.Properties["DataConnectionString"]);
            if (!ConnectionStringInfo.IsValid())
                throw new Exception("Invalid connection string for provider:"+name);
            ConnectionSettings = new ConnectionSettings(new UriBuilder("http",ConnectionStringInfo.Host,ConnectionStringInfo.Port,"","").Uri,ConnectionStringInfo.Index);
            _name = name;
            Log = providerRuntime.GetLogger(this.GetType().FullName);
            Client = Client ?? new ElasticClient(ConnectionSettings);
            var res = await Client.IndexExistsAsync(ConnectionStringInfo.Index);
            if (!res.Exists)
            {
                Log.Info("Index {0} does not exist.Creating...", ConnectionStringInfo.Index);
                var createResponse = await Client.CreateIndexAsync(ConnectionStringInfo.Index);
                if (createResponse.Acknowledged)
                {
                    Log.Info("Index {0} has been created.", ConnectionStringInfo.Index);
                }
            }
        }

        internal async Task ReadStateFromElasticAsync(string grainType, string key, GrainState grainState)
        {
            var response = await Client.Raw.GetAsync(ConnectionStringInfo.Index, GetElasticSearchType(grainType), key);
            if (response.Success && (bool)response.Response["found"])
            {
                JsonConvert.PopulateObject(response.Response["_source"], grainState, new JsonSerializerSettings() { });
                grainState.Etag = response.Response["_version"];
                var occ = grainState as IOptimisticConcurrencyControl;
                if (occ != null)
                    occ.Version = response.Response["_version"];
            }
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var key = GetElasticSearchKey(grainReference);
            await ReadStateFromElasticAsync(grainType, key, grainState);
        }

        internal async Task WriteStateToElasticAsync(string grainType, string key, GrainState grainState)
        {
            var occ = grainState as IOptimisticConcurrencyControl;
            Func<IndexRequestParameters, IndexRequestParameters> rp = null;
            if (occ != null)
            {
                if (occ.Version==0) 
                    rp = x => x.OpType(OpType.Create).Version(occ.Version);
                 else
                    rp = x => x.Version(occ.Version);

            }
            var response = await Client.Raw.
                                    IndexAsync(ConnectionStringInfo.Index, GetElasticSearchType(grainType), key, grainState,
                                    rp
                                    );
            if (!response.Success)
            {
                throw new ElasticsearchStorageException("WriteStateAsync operation failed.",response.OriginalException);
            }
            if (occ!=null)//update version
            {
                occ.Version = response.Response["_version"];
            }
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var key = GetElasticSearchKey(grainReference);
            await WriteStateToElasticAsync(grainType, key, grainState);
        }
        protected string GetElasticSearchType(string grainType)
        {
            return grainType.Replace('.', '_');
        }
        protected string GetElasticSearchKey(GrainReference reference)
        {
            return reference.ToString();
        }
    }
}

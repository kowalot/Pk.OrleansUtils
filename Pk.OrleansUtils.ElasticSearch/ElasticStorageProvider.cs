using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Nest;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var elasticType = grainType.Replace('.', '_');
            var key = grainReference.ToString();
            var client = new ElasticClient();
            var res = await client.DeleteAsync(ConnectionStringInfo.Index,elasticType,key);
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
                    var pi = connectionInfo.GetType().GetProperty(nv[0]);
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
            var client = new ElasticClient();
            var res = await client.IndexExistsAsync(ConnectionStringInfo.Index);
            if (!res.Exists)
            {
                Log.Info("Index {0} does not exist.Creating...", ConnectionStringInfo.Index);
                var createResponse = await client.CreateIndexAsync(ConnectionStringInfo.Index);
                if (createResponse.Acknowledged)
                {
                    Log.Info("Index {0} has been created.", ConnectionStringInfo.Index);
                }
            }
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var key = GetElasticSearchKey(grainReference);
            var client = CreateClient();
            var response = await client.Raw.GetAsync(ConnectionStringInfo.Index, GetElasticSearchType(grainType), key);
            if (response.Success && (bool)response.Response["found"])
            {
                JsonConvert.PopulateObject(response.Response["_source"], grainState, new JsonSerializerSettings() { });
            }
            grainState.Etag = DateTime.UtcNow.ToString();
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var key = GetElasticSearchKey(grainReference);
            var client = CreateClient();
            var response = await client.Raw.IndexAsync(ConnectionStringInfo.Index, GetElasticSearchType(grainType), key, grainState);
            if (!response.Success)
            {
                throw new Exception("WriteStateAsync operation failed");
            }

        }
        protected ElasticClient CreateClient()
        {
            return new ElasticClient(ConnectionSettings);
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

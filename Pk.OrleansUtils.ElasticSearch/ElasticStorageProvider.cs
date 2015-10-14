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


        public string IndexName { get; set; } 

        private string _name = "";
        public string Name
        {
            get
            {
                return _name;
            }
        }


        public async Task ClearStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var elasticType = grainType.Replace('.', '_');
            var key = grainReference.ToString();
            var client = new ElasticClient();
            var res = await client.DeleteAsync(IndexName,elasticType,key);
        }

        public Task Close()
        {
            return TaskDone.Done;
        }

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            IndexName = "orleans";
            _name = name;
            Log = providerRuntime.GetLogger(this.GetType().FullName);
            var client = new ElasticClient();
            var res = await client.IndexExistsAsync(IndexName);
            if (!res.Exists)
            {
                Log.Info("Index {0} does not exist.Creating...",IndexName);
                var createResponse = await client.CreateIndexAsync(IndexName);
                if (createResponse.Acknowledged)
                {
                    Log.Info("Index {0} has been created.", IndexName);
                }
            }
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var elasticType = grainType.Replace('.', '_');
            var key = grainReference.ToString();
            var client = new ElasticClient();
            var response = await client.Raw.GetAsync(IndexName, elasticType, key);
            if (response.Success && (bool)response.Response["found"])
            {
                JsonConvert.PopulateObject(response.Response["_source"], grainState, new JsonSerializerSettings() { });
            }
            grainState.Etag = DateTime.UtcNow.ToString();
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var elasticType = grainType.Replace('.', '_');
            var key = grainReference.ToString();
            var client = new ElasticClient();
            var response = await client.Raw.IndexAsync(IndexName, elasticType, key, grainState);
            if (!response.Success)
            {
                throw new Exception("WriteStateAsync operation failed");
            }

        }
    }
}

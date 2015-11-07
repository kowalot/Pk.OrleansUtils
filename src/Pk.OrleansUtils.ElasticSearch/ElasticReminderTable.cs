using Nest;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.ElasticSearch
{
    public class ElasticReminderTable : IReminderTable
    {
        private TraceLogger _logger;

        public const string ORLEANS_REMINDER_TABLE_KEY = "RemindersTable";

        public ElasticClient Elastic { get; set; }

        public ElasticReminderTable()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<ReminderTableData> ReadRows(GrainReference key)
        {
            
            var skey = key.ToKeyString();
            var op = await Elastic.SearchAsync<ElasticReminderEntry>(s => s.Query(qu => qu.Filtered(x => x.Filter(fs => fs.Term(tx => tx.GrainRefKey, skey)))));
            if (op.IsValid)
            {
                return new ReminderTableData(op.Hits.Select(x => x.Source.GetReminderEntry(x.Version)));
            }
            else
                throw new ElasticsearchStorageException("Delete row problem", op.ConnectionStatus.OriginalException);

        }
        public async Task Init(GlobalConfiguration config, TraceLogger logger)
        {
            this._logger = logger;
            if (logger.IsVerbose3)
                logger.Verbose3("ElasticReminderTable.InitializeMembershipTable called.");
            var ci = FromConnectionString<ConnectionInfo>(config.DataConnectionStringForReminders);
            Elastic = new ElasticClient(ci.GetConnectionSettings());
            var indexExists = await Elastic.IndexExistsAsync(ci.Index);
            if (!indexExists.IsValid)
                throw new ElasticsearchStorageException("Error occured during initialization", indexExists.ConnectionStatus.OriginalException);
            if (!indexExists.Exists)
            {
                var createIndexResponse = await Elastic.CreateIndexAsync(ci.Index,cd=>cd.AddMapping<ElasticReminderEntry>(md=>md.MapFromAttributes()));
                if (createIndexResponse.IsValid && createIndexResponse.Acknowledged)
                {
                    _logger.Info("Elasticsearch index named:{0} has been created for Orleans reminders",ci.Index);
                }
            }
            //var mapResponse = await Elastic.MapAsync<ElasticReminderEntry>(s=>s.MapFromAttributes());
            //if (!mapResponse.IsValid)
            //{
            //    throw new ElasticsearchStorageException("Error occured during initialization", mapResponse.ConnectionStatus.OriginalException);
            //}

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

        /// <summary>
        /// Return all rows that have their GrainReference's.GetUniformHashCode() in the range (start, end]
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<ReminderTableData> ReadRows(uint begin, uint end)
        {
            try
            {
                var op = await Elastic.SearchAsync<ElasticReminderEntry>(d => d.Query(qd => qd.Range(s => s.OnField(x => x.UniformHashCode).Greater(begin.ToString()).LowerOrEquals(end.ToString()))));
                if (!op.IsValid)
                    throw new ElasticsearchStorageException("Exception occured during Elastic operation in ReadRows method", op.ConnectionStatus.OriginalException);
                return new ReminderTableData(op.Hits.Select(t => t.Source.GetReminderEntry(t.Version)));
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<ReminderEntry> ReadRow(GrainReference grainRef, string reminderName)
        {
            var ret = await ElasticReminderEntry.Get(Elastic,grainRef, reminderName);
            return ret.GetReminderEntry();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Consul"></param>
        /// <returns></returns>
        public async Task<string> UpsertRow(ReminderEntry entry)
       {
            var ee = new ElasticReminderEntry(entry);
            return  await ee.Upsert(Elastic);
        }

        public async Task<bool> RemoveRow(GrainReference grainRef, string reminderName, string eTag)
        {
            var key = ElasticReminderEntry.CreateIdFrom(grainRef, reminderName);
            var op = await Elastic.DeleteAsync<ElasticReminderEntry>(d => d
                                            .Id(key)
                                            .Version(long.Parse(eTag)));
            if (op.IsValid)
                return op.IsValid;
            else
                throw new ElasticsearchStorageException("Delete row problem",op.ConnectionStatus.OriginalException);
        }

        public Task TestOnlyClearTable()
        {
            return TaskDone.Done;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Orleans;
using Orleans.Runtime;

namespace Pk.OrleansUtils.ElasticSearch
{
    [ElasticType(IdProperty = "Id")]
    public class ElasticReminderEntry
    {
        private ReminderEntry entry;

        public ElasticReminderEntry()
        {

        }

        public string ToElasticValue(GrainReference gref)
        {
            ///"GrainReference=00000000000000000000000000000000060000006aa96326+abc"
            /// to
            /// "GrainReference^00000000000000000000000000000000060000006aa96326_abc"
            return gref.ToKeyString().Replace("=", "__").Replace("_", "+");
        }

        public ElasticReminderEntry(ReminderEntry entry)
        {
            this.entry = entry;
            UniformHashCode = entry.GrainRef.GetUniformHashCode();
            ReminderName = entry.ReminderName;
            GrainRefKey = entry.GrainRef.ToKeyString();
            Period = entry.Period;
            ETag = entry.ETag ?? "0";
            StartAt = entry.StartAt;
            Id = CreateIdFrom(entry.GrainRef, entry.ReminderName);
        }

        public static string CreateIdFrom(GrainReference grainRef,string reminderName)
        {
            if (String.IsNullOrEmpty(reminderName))
                throw new ArgumentNullException("reminderName");
            if (grainRef==null)
                throw new ArgumentNullException("grainRef");
            return grainRef.ToKeyString() + "," + reminderName;
        }

        public string Id { get; set; }

        [ElasticProperty(Type=FieldType.Long)]
        public uint UniformHashCode { get; set; }

        public string ReminderName { get; set; }

        [ElasticProperty(OmitNorms = true, Index = FieldIndexOption.NotAnalyzed)]
        public string GrainRefKey { get; set; }
        public TimeSpan Period { get; set; }
        public string ETag { get; set; }
        public DateTime StartAt { get; set; }

        internal async Task<string> Upsert(ElasticClient elastic)
        {
            var op = await elastic.UpdateAsync<ElasticReminderEntry>(
                    u => u
                         .Doc(this)
                         .DocAsUpsert(true)
                        );
            if (!op.IsValid )
                throw new ElasticsearchStorageException("Error occured during update for ElasticReminderEntry",op.ConnectionStatus.OriginalException);
            return op.Version;
        }


        internal static async Task<ElasticReminderEntry> Get(ElasticClient elastic, GrainReference grainRef, string reminderName)
        {
            var op = await elastic.GetAsync<ElasticReminderEntry>(CreateIdFrom(grainRef, reminderName));
            if (op.IsValid)
            {
                op.Source.ETag = op.Version;
                return op.Source;
            }
            else
                throw new ElasticsearchStorageException();
        }

        internal ReminderEntry GetReminderEntry(string eTag=null)
        {
            entry = new ReminderEntry();
            entry.ETag = eTag ?? ETag;
            entry.GrainRef = GrainReference.FromKeyString(GrainRefKey);
            entry.Period = Period;
            entry.ReminderName = ReminderName;
            entry.StartAt = StartAt;
            return entry;
        }
    }
}

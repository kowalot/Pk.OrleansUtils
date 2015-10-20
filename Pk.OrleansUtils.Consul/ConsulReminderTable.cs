using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Consul
{
    public class ConsulReminderProvider : IReminderTable
    {
        private TraceLogger _logger;
        public ConsulClient Consul { get; set; }

        private const string ORLEANS_CATALOG_KEY = "RemindersTable";
        private const string ORLEANS_MEMBERS_SUBKEY = "Members";
        private const string ORLEANS_I_AM_ALIVE_FOLDER_KEY = "IAmAlive";
        private TimeSpan maxStaleness;

        private const string MOCK_E_TAG = "MockETag";
        private readonly TimeSpan delay;

        public string dataConnectionString { get; private set; }

        public ConsulReminderProvider()
        {
            this.delay = TimeSpan.FromMilliseconds(100);
        }

        public ConsulReminderProvider(TimeSpan delay)
        {
            this.delay = delay;
        }

        public Task Init(GlobalConfiguration config, TraceLogger logger)
        {
            this._logger = logger;
            if (logger.IsVerbose3)
                logger.Verbose3("ConsulReminderTable.InitializeMembershipTable called.");
            this.Consul = new ConsulClient(ConsulConnectionInfo.FromConnectionString(config.DataConnectionStringForReminders));
            return TaskDone.Done;
        }

        public async Task<ReminderTableData> ReadRows(GrainReference key)
        {
            await Task.Delay(delay);
            return Empty();
        }

        protected ReminderTableData Empty()
        {
            return new ReminderTableData();
        }

        public async Task<ReminderTableData> ReadRows(uint begin, uint end)
        {
            await Task.Delay(delay);
            return Empty();
        }

        public async Task<ReminderEntry> ReadRow(GrainReference grainRef, string reminderName)
        {
            await Task.Delay(delay);
            return null;
        }

        public async Task<string> UpsertRow(ReminderEntry entry)
        {
            await Task.Delay(delay);
            return MOCK_E_TAG;
        }

        public async Task<bool> RemoveRow(GrainReference grainRef, string reminderName, string eTag)
        {
            await Task.Delay(delay);
            return true;
        }

        public Task TestOnlyClearTable()
        {
            return Task.Delay(delay);
        }
    }
}

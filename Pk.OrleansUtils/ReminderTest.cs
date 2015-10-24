using Orleans;
using Pk.OrleansUtils.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Pk.OrleansUtils
{
    public class ReminderTest : Grain<ReminderState>,IReminderTest, IRemindable
    {
        public Task<int> GetCount()
        {
            return Task.FromResult(State.Count);
        }

        public async Task<int> GetRemindersCount()
        {
            return (await GetReminders()).Count;
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            //var me = await this.GetReminder(reminderName);
            //await this.UnregisterReminder(me);
            State.Count++;
            var reminder = (await this.GetReminders()).FirstOrDefault(t => t.ReminderName == "xxx");
            if (reminder!=null && State.Count == 2)
            {
                await this.UnregisterReminder(reminder);
            }
            await WriteStateAsync();
        }

        public Task RegisterReminder(int secondsAhead)
        {
            this.RegisterOrUpdateReminder("xxx", TimeSpan.FromSeconds(secondsAhead), TimeSpan.FromSeconds(secondsAhead));
            return TaskDone.Done;
        }
    }
}

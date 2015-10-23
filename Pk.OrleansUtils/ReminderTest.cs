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
    public class ReminderTest : Grain,IReminderTest, IRemindable
    {
        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            var me = await this.GetReminder(reminderName);
            await this.UnregisterReminder(me);
        }

        public Task RegisterReminder(int secondsAhead)
        {
          //  this.GetReminders();
          //  this.RegisterOrUpdateReminder("xxx", TimeSpan.FromSeconds(secondsAhead), TimeSpan.FromSeconds(secondsAhead));
            return TaskDone.Done;
        }
    }
}

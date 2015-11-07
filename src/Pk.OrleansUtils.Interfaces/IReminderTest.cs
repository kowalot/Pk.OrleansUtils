using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.Interfaces
{
    public interface IReminderTest : IGrainWithStringKey
    {
        Task RegisterReminder(int secondsAhead);
        Task<int> GetCount();
        Task<int> GetRemindersCount();
    }
}

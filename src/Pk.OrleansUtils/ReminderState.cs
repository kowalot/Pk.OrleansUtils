using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils
{
    public class ReminderState : GrainState
    {
        public int Count { get; set; }
    }
}

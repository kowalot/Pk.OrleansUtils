using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pk.OrleansUtils
{
    public class InternalClass
    {
        public DateTime WasHere { get; set; }
        public int Token { get; set; }
    }


    public class MyGrainState : GrainState
    {
        public string Something { get; set; }

        public List<string> List { get; set; }
        public SortedDictionary<string, InternalClass> ObjDic { get; set; }

        public MyGrainState()
        {
            List = new List<string>();
            ObjDic = new SortedDictionary<string, InternalClass>();
        }

    }
}

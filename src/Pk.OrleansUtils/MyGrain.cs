using System;
using System.Threading.Tasks;
using Orleans;
using Pk.OrleansUtils.Interfaces;

namespace Pk.OrleansUtils
{
    /// <summary>
    /// Grain implementation class Grain1.
    /// </summary>
    public class MyGrain : Grain<MyGrainState>, IMyGrain
    {
        public async Task<int> IncrementAndGet(int incValue)
        {
            State.Counter += incValue;
            await WriteStateAsync();
            return State.Counter;
        }

        public Task TaskDoSomething()
        {
            State.Something = "Touched: "+DateTime.Now.ToString();
            State.List.Add(State.Something);
            State.ObjDic.Add(DateTime.Now.Ticks.ToString(), new InternalClass() { Token = DateTime.Now.Second, WasHere = DateTime.MinValue });
            
            return WriteStateAsync();
        }
    }
}

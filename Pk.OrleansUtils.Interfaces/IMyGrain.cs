using System.Threading.Tasks;
using Orleans;

namespace Pk.OrleansUtils.Interfaces
{
    /// <summary>
    /// Grain interface IGrain1
    /// </summary>
	public interface IMyGrain : IGrainWithGuidKey
    {
        Task TaskDoSomething();
    }
}

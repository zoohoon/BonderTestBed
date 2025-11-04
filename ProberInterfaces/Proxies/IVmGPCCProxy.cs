using ProberInterfaces.Proxies;
using System.ServiceModel;

namespace ProberInterfaces
{
    public interface IVmGPCCProxy : IProberProxy
    {
        bool IsOpened();
        CommunicationState GetCommunicationState();
    }
}

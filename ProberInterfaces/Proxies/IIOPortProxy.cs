using ProberInterfaces.Proxies;
using System.Collections.Generic;
using System.ServiceModel;

namespace ProberInterfaces
{
    public interface IIOPortProxy : IProberProxy
    {
        bool IsOpened();
        CommunicationState GetCommunicationState();

        List<IOPortDescripter<bool>> GetInputPorts();

        void SetForcedIO(IOPortDescripter<bool> ioport, bool IsForced, bool ForecedValue);
    }
}

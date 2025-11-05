using ProberErrorCode;
using ProberInterfaces.Communication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Temperature.EnvMonitoring
{
    public interface IEnvMonitoringHub : IFactoryModule, IModule
    {               
        IByteCommModule CommModule { get; set; }
        CommunicationParameterBase CommunicationParam { get; set; }
        EventCodeEnum Commmunication();
        void Send(byte[] buffer, int offset, int count);
    }
}

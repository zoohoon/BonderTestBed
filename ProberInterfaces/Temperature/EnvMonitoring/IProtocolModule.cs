using ProberErrorCode;
using ProberInterfaces.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Temperature.EnvMonitoring
{
    public interface IProtocolModule : IFactoryModule, IModule
    {
        EventCodeEnum VerifyData(int idx, byte[] sendBuff);
        void ParseData(byte[] receiveData, ISensorInfo sensorInfo);
        int GetDataIdFunc(byte[] buffer);
        void BuffInit();
        byte[] Sendbuff { get; set; }
        byte[] AlarmResetbuff { get; set; }
        byte[] SetTempbuff { get; set; }        
        byte[] SetTempDeviationbuff { get; set; }                
    }
}

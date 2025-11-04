using ProberErrorCode;
using System;

namespace ProberInterfaces
{
    public interface IPMASManager : IFactoryModule, IHasSysParameterizable, IModule
    {
        int ConnHndl { get; set; }
        bool InitFlag { get; set; }
        EventCodeEnum InitializeController();
        //EventCodeEnum LoadECATIODefinitions(string filePath);
        int SendMessage(int nodeNum, String obj,String param);
        int ReceiveMessage(int nodeNum, String obj, out int value);
    }
}

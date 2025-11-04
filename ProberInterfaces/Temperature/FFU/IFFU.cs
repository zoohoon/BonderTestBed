using ProberErrorCode;
using System;
using System.ComponentModel;

namespace ProberInterfaces.Temperature.FFU
{
    public enum FFUProcessType
    {
        IDLE,
        RUNNING,
        DONE,
        ERROR
    }

    public interface IFFUModule : IFactoryModule, INotifyPropertyChanged, IModule
    {
        FFUInfo PreFFUInfo { get; set; }
        FFUInfo CurFFUInfo { get; set; }
        EventCodeEnum DisConnect();
        string GetFFUInfo(FFUInfo info);
        string GetFFUInfo(FFUInfo info, ushort startaddress, ushort numregisters);
    }

    public interface IFFUComm : IDisposable, IFactoryModule
    {
        EventCodeEnum InitModule();
        EventCodeEnum Connect();
        void DisConnect();

        ushort[] GetData(int nodeNum, ushort startAddress, ushort numRegisters);
        bool GetCommState();
    }
}

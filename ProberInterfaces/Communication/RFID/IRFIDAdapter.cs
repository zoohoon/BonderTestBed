using System;

namespace ProberInterfaces.Communication.RFID
{
    public interface IRFIDAdapter : IModule, IFactoryModule
    {
        ICommModule CommModule { get; }
        Boolean RFID_RD_DATA(int foupIndex);
    }
}

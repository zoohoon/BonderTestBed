using System;

namespace ProberInterfaces.Cooler.DryAir
{
    using ProberErrorCode;

    public interface IDryAirProcessor : IDisposable, IFactoryModule
    {
        DryAirNetIOMappings HBNetIOMap { get; }
        EventCodeEnum InitModule();
        int WriteBit(IOPortDescripter<bool> portDesc, bool value);
        int ReadBit(IOPortDescripter<bool> portDesc, out bool value);
        int WaitForIO(IOPortDescripter<bool> io, bool level, long timeout = 0);
        void SetHBNetIOMap(DryAirNetIOMappings hBNetIOMap);
        bool GetIOServInputValue(int channel, int port);
    }
}

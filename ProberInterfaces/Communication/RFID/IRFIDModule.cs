namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.RFID;
    using ProberInterfaces.Communication.RFID;
    using System.IO.Ports;
    using ProberInterfaces.Communication;
    using ProberInterfaces.CassetteIDReader;

    public interface IRFIDModule : IModule, IFactoryModule, IHasSysParameterizable, ICSTIDReader
    {
        RFIDSysParameters RFIDSysParam { get; set; }
        IRFIDAdapter GetRFIDAdapter();
        void ClearRFID();
        StopBits StopBitsEnum { get; set; }
        string SerialPort { get; set; }
    }

}
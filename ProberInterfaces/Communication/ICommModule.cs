using ProberErrorCode;
using ProberInterfaces.Enum;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Communication
{
    public delegate void setDataHandlerforByte(byte[] receiveData);    
    public delegate void setDataHandler(string receiveData);
    public interface ICommModule : IDisposable, IFactoryModule
    {
        event setDataHandler SetDataChanged;        
        EventCodeEnum InitModule();
        EventCodeEnum ReInitalize(bool attattch = true);
        EventCodeEnum Connect();

        void DisConnect();   

        string GetReceivedData();
        void SetReceivedData(string receiveData);
        EnumCommunicationState GetCommState();        
        
        void Send(string sendData);
        EnumCommunicationState CurState { get; }
        void Send(byte[] buffer, int offset, int count);                
        SerialPort Port { get; set; }       
    }

    public interface IByteCommModule : ICommModule
    {
        event setDataHandlerforByte SetDataChangedByte;
    }

    public enum EnumCommmunicationType
    {
        EMUL,
        SERIAL,
        TCPIP,
        RS232,
        USB,
        MODBUS
    }
}

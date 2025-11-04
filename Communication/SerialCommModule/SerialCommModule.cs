using LogModule;
using ProberErrorCode;
using ProberInterfaces.Communication;
using ProberInterfaces.Enum;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;

namespace Communication.SerialCommModule
{
    public partial class SerialCommModule : ICommModule, INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;


        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        public event setDataHandler SetDataChanged;        

        public SerialCommState SerialCommState = null;
        private bool IsDisposed = false;
        public bool Initialized { get; set; }

        private EnumCommunicationState _CurState;
        public EnumCommunicationState CurState
        {
            get { return _CurState; }
            set
            {
                if (value != _CurState)
                {
                    _CurState = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public SerialCommModule(string port, int baudRate, int dataRate, StopBits stopBit)
        {
            SerialPort = port;
            BaudRate = baudRate;
            DataBits = dataRate;
            StopBit = stopBit;
        }
        private SerialPort _Port;
        public SerialPort Port
        {
            get { return _Port; }
            set { _Port = value; }
        }
        //로그
        private StringBuilder Log_Strings;
        //로그 객체
        private String LogStrings
        {
            set
            {
                if (Log_Strings == null)
                    Log_Strings = new StringBuilder(1024);
                //로그 1024 제한
                if (Log_Strings.Length >= (1024 - value.Length))
                    Log_Strings.Clear();
                //로그 추가 및 화면 표시
                Log_Strings.AppendLine(value);

            }
        }
        private string _SerialPort;
        public string SerialPort
        {
            get { return _SerialPort; }
            set { _SerialPort = value; }
        }
        private int _BaudRate;
        public int BaudRate
        {
            get { return _BaudRate; }
            set { _BaudRate = value; }
        }
        private int _DataBits;
        public int DataBits
        {
            get { return _DataBits; }
            set { _DataBits = value; }
        }
        private StopBits _StopBit;
        public StopBits StopBit
        {
            get { return _StopBit; }
            set { _StopBit = value; }
        }
        public EventCodeEnum Connect()
            => this?.SerialCommState.Connect()
            ?? EventCodeEnum.UNDEFINED;

        public void Dispose()
        {
            try
            {
                if (!IsDisposed)
                {
                    SerialCommState.Disconnect();
                    IsDisposed = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CommModule.Dispose() Error occurred. Err = {err.Message}");
            }
        }

        public string GetReceivedData()
        {
            throw new NotImplementedException();
        }

        public void CommStateTransition(SerialCommState state)
        {
            if (SerialCommState?.GetType() != state?.GetType())
            {
                LoggerManager.Debug($"[CommModule_Serial] CommStateTransition() Old = {SerialCommState.GetType()}, New = {state.GetType()}");
                this.SerialCommState = state;
                CurState = this.SerialCommState.GetCommunicationState();
            }
        }

        public void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string rcstrbuf = Port.ReadExisting();
                LoggerManager.Debug($"[CommModule_Serial] Receive Data = {rcstrbuf}");
                SetDataChanged(rcstrbuf);
            }
            catch (Exception err)
            {
                DisConnect();
                LoggerManager.Exception(err);
            }
        }

        /////mod main////
        public Boolean bRcvDone;
        public Boolean bCommErr;
        public Boolean bRcvErrCode;
        public string RCV_STR_Buf;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!Initialized)
                {
                    SerialCommState?.Disconnect();
                    SerialCommState = new SerialDisconnectState(this);
                    Initialized = true;
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EnumCommunicationState GetCommState()
        => this.SerialCommState?.GetCommunicationState() ?? EnumCommunicationState.DISCONNECT;

        public void SetReceivedData(string receiveData)
        {
        }

        public void Send(string sendData)
        {
            SerialCommState?.Send(sendData);
        }

        public EventCodeEnum ReInitalize(bool attatch)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                DisConnect();
                if (SerialCommState.GetCommunicationState() == EnumCommunicationState.DISCONNECT)
                {
                    retVal = Connect();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void DisConnect()
        {
            try
            {
                SerialCommState?.Disconnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Send(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
    public abstract class SerialCommState
    {
        public SerialCommModule CommModule;
        public SerialCommState(SerialCommModule commModule)
        {
            this.CommModule = commModule;

        }
        public abstract EventCodeEnum Connect();
        public abstract void Disconnect();
        public abstract EnumCommunicationState GetCommunicationState();
        public abstract EventCodeEnum Send(string sednData);
    }
    public sealed class SerialDisconnectState : SerialCommState
    {
        public SerialDisconnectState(SerialCommModule commModule) : base(commModule)
        {
        }

        public override EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CommModule.Port.PortName = CommModule.SerialPort;
                CommModule.Port.BaudRate = CommModule.BaudRate;
                CommModule.Port.DataBits = CommModule.DataBits;
                CommModule.Port.Parity = Parity.None;
                CommModule.Port.Handshake = Handshake.None;
                CommModule.Port.StopBits = CommModule.StopBit;
                CommModule.Port.Encoding = Encoding.UTF8;
                CommModule.Port.DataReceived += CommModule.Port_DataReceived;
                CommModule.Port.Open();

                if (CommModule.Port.IsOpen)
                {
                    LoggerManager.Debug($"[CommModule_Serial] Connect Success.");
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
                // TODO 실패시 리턴 0 으로 내보내야함
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void Disconnect()
        {
            if (CommModule.Port != null)
            {
                CommModule.Port.Close();
                CommModule.Port = null;
                this.CommModule.CommStateTransition(new SerialDisconnectState(this.CommModule));
                LoggerManager.Debug($"[CommModule_Serial] Disconnect()");
            }
        }

        public override EnumCommunicationState GetCommunicationState()
            => EnumCommunicationState.DISCONNECT;

        public override EventCodeEnum Send(string sednData)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class SerialConnectState : SerialCommState
    {
        public SerialConnectState(SerialCommModule commModule) : base(commModule)
        {
        }

        public override EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
            }
            catch (Exception)
            {

            }

            return retVal;
        }

        public override void Disconnect()
        {
            try
            {
                if (CommModule.Port != null)
                {
                    CommModule.Port.Close();
                    CommModule.Port = null;
                    this.CommModule.CommStateTransition(new SerialDisconnectState(this.CommModule));
                    LoggerManager.Debug($"[CommModule_Serial] Disconnect()");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EnumCommunicationState GetCommunicationState()
            => EnumCommunicationState.CONNECTED;

        public override EventCodeEnum Send(string sednData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"[CommModule_Serial] Send Data = {sednData}");
                CommModule.Port.Write(sednData);
            }
            catch (Exception err)
            {
                Disconnect();
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}

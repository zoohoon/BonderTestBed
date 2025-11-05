using EnvMonitoring.ProtocolModules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ProberInterfaces.Communication;
using ProberInterfaces.Enum;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;

namespace EnvMonitoring
{
    public class EnvSerialCommModule : IByteCommModule, INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion        
        public event setDataHandlerforByte SetDataChangedByte;
        public event setDataHandler SetDataChanged;

        public bool Initialized { get; set; } = false;        
        private SerialCommState _SerialCommState = null;
        protected object lockObj;
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

        private SerialPort _Port = new SerialPort();
        public SerialPort Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        public void CommStateTransition(SerialCommState state)
        {
            if (_SerialCommState?.GetType() != state?.GetType())
            {
                LoggerManager.Debug($"[SerialCommModule_EnvMonitoring] CommStateTransition() Old = {_SerialCommState.GetType()}, New = {state.GetType()}");
                this._SerialCommState = state;
                CurState = this._SerialCommState.GetCommunicationState();
            }
        }        

        private byte[] ReadSerialByteData()
        {
            Port.ReadTimeout = 100;            
            byte[] bytesBuffer = new byte[Port.BytesToRead];
            int bufferOffset = 0;
            int bytesToRead = Port.BytesToRead;
            int byteCheck = Port.ReceivedBytesThreshold;

            while (bytesToRead > 0)
            {
                try
                {
                    int readBytes = Port.Read(bytesBuffer, bufferOffset, bytesToRead - bufferOffset);
                    bytesToRead -= readBytes;
                    bufferOffset += readBytes;
                }
                catch (TimeoutException err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }

            return bytesBuffer;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (Port.IsOpen)
                {
                    lock(lockObj)
                    {                        
                        // 1바이트 데이터를 수신: 1.041 ms
                        // 최대 24바이트: 24.984                        
                        byte[] bytesBuffer = ReadSerialByteData();
                        if (bytesBuffer != null)
                            SetDataChangedByte(bytesBuffer);
                        
                    }
                }                           
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);                
            }
        }
        
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    lockObj = new object();
                    _SerialCommState?.Disconnect();
                    _SerialCommState = new SerialDisconnectState(this);                    
                    Initialized = true;
                    retval = EventCodeEnum.NONE;
                }
                else
                {                 
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        public EventCodeEnum ReInitalize(bool attach = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                
                if (_SerialCommState.GetCommunicationState() == EnumCommunicationState.CONNECTED)
                {
                    DisConnect();
                }
                if (_SerialCommState.GetCommunicationState() == EnumCommunicationState.DISCONNECT)
                {
                    if (attach)
                    {
                        retVal = Connect();                        
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum Connect()
            => this?._SerialCommState.Connect()
            ?? EventCodeEnum.UNDEFINED;   
        
        public void DisConnect()
        {
            try
            {
                _SerialCommState?.Disconnect();
                this.CurState = EnumCommunicationState.DISCONNECT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }     
        
        public void Send(byte[] buffer, int offset, int count)
            => this._SerialCommState?.Send(buffer, offset, count);

        public EnumCommunicationState GetCommState()
        => this._SerialCommState?.GetCommunicationState() ?? EnumCommunicationState.DISCONNECT;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string GetReceivedData()
        {
            throw new NotImplementedException();
        }

        public void SetReceivedData(string receiveData)
        {
            try
            {
                SetDataChanged(receiveData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Send(string sendData)
        {
            throw new NotImplementedException();
        }

        public EnvSerialCommModule()
        {
            try
            {
                                            
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public abstract class SerialCommState
        {
            public EnvSerialCommModule CommModule;
            public SerialCommState(EnvSerialCommModule commModule)
            {
                this.CommModule = commModule;

            }
            public abstract EventCodeEnum Connect();
            public abstract void Disconnect();
            public abstract EnumCommunicationState GetCommunicationState();
            public abstract void Send(byte[] buffer, int offset, int count);
        }
        public sealed class SerialConnectState : SerialCommState
        {
            public SerialConnectState(EnvSerialCommModule commModule) : base(commModule)
            {
            }

            public override EventCodeEnum Connect()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                try
                {
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                return retVal;
            }

            public override void Disconnect()
            {
                try
                {
                    if (CommModule.Port != null)
                    {
                        lock (CommModule.lockObj)
                        {                            
                            CommModule.Port.DataReceived -= new SerialDataReceivedEventHandler(CommModule.Port_DataReceived);
                            CommModule.Port.Close();                            
                            this.CommModule.CommStateTransition(new SerialDisconnectState(this.CommModule));
                            LoggerManager.Debug($"[SerialCommModule_EnvMonitoring] Disconnect()");
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            public override EnumCommunicationState GetCommunicationState()
                => EnumCommunicationState.CONNECTED;


            public override void Send(byte[] buffer, int offset, int count)
            {
                try
                {
                    if (CommModule.Port != null && !CommModule.Port.IsOpen)
                    {
                        this.CommModule.CommStateTransition(new SerialDisconnectState(this.CommModule));
                        return;
                    }
                        

                    lock (CommModule.lockObj)
                    {
                        CommModule.Port.Write(buffer, offset, count);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public sealed class SerialDisconnectState : SerialCommState
        {
            public SerialDisconnectState(EnvSerialCommModule commModule) : base(commModule)
            {
            }           
            public override EventCodeEnum Connect()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                try
                {
                    foreach (var item in CommModule.EnvMonitoringManager().SensorParams.SensorCommParams)//  이거 빼기  
                    {
                        lock (CommModule.lockObj) 
                        {
                            if (!CommModule.Port.IsOpen)
                            {
                                CommModule.Port.PortName = item.PortName.Value;
                                CommModule.Port.BaudRate = item.BaudRate.Value;
                                CommModule.Port.DataBits = item.DataBits.Value;
                                CommModule.Port.Parity = Parity.None;
                                CommModule.Port.StopBits = item.StopBits.Value;
                                CommModule.Port.DataReceived += new SerialDataReceivedEventHandler(CommModule.Port_DataReceived);

                                CommModule.Port.Open();                                
                                System.Threading.Thread.Sleep(3000);    // connect timeout
                                if (CommModule.Port.IsOpen)
                                {
                                    LoggerManager.Debug($"[SerialCommModule_EnvMonitoring] Connect Success.");
                                    this.CommModule.CommStateTransition(new SerialConnectState(this.CommModule));
                                    retVal = EventCodeEnum.NONE;
                                }
                                else
                                {
                                    CommModule.Port.DataReceived -= new SerialDataReceivedEventHandler(CommModule.Port_DataReceived);
                                    retVal = EventCodeEnum.UNDEFINED;
                                }
                            }
                        }
                               
                    }
                    
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }

            public override void Disconnect()
            {
                try
                {                    
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            public override EnumCommunicationState GetCommunicationState()
                => EnumCommunicationState.DISCONNECT;

            public override void Send(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
    }
}

using LogModule;
using Modbus.Device;
using ProberErrorCode;
using ProberInterfaces.Temperature.FFU;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace FFUCommModule
{
    public class FFUSerialCommModule : IFFUComm, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
           => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        public bool Initialized { get; set; }
        private bool IsDisposed = false;
        private string Address { get; set; }
        private object lockObject = new object();
        private IModbusSerialMaster Master { get; set; }
        private SerialPort Port{ get; set; }
        public FFUSerialCommModule(string commaddress)
        {
            Address = commaddress;
        }
        public EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                using (Port = new SerialPort(Address))
                {
                    // configure serial port
                    Port.BaudRate = 9600;
                    Port.DataBits = 8;
                    Port.Parity = Parity.None;
                    Port.StopBits = StopBits.One;
                    Port.DtrEnable = true;
                    Port.ReadBufferSize = 1024;
                    Port.WriteBufferSize = 512;
                    Port.Handshake = Handshake.None;
                    Port.ReadTimeout = 1000;
                    Port.WriteTimeout = 1000;
                    Port.Open();
                    LoggerManager.Debug($"FFUSerial connect success - Port=[{Address}]");

                    Master = ModbusSerialMaster.CreateRtu(Port);
                    LoggerManager.Debug($"Create ModbusSerialMaster[{Master}]");

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"FFUSerial connect failed - Port=[{Address}]");
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        public void DisConnect()
        {
            try
            {
                Master?.Dispose();
                Master = null;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        public void Dispose()
        {
            try
            {
                if (!IsDisposed)
                {
                    this.DisConnect();
                    IsDisposed = true;
                }
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.Dispose() Error occurred. Err = {err.Message}");
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!Initialized)
                {
                    Initialized = true;
                    Connect();

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


        public ushort[] GetData(int nodenum, ushort startaddress, ushort numregisters)
        {
            ushort[] retVal = null;
            lock (lockObject)
            {
                try
                {
                    byte slaveId = (byte)nodenum;

                    if (!Port.IsOpen)
                        Port.Open();
                    retVal = Master.ReadHoldingRegisters(slaveId, startaddress, numregisters);
                }
                catch (Exception err)
                {
                    //작업시간초과 - 연결 없음
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public bool GetCommState()
        {
            bool retVal = true;
            try
            {
                if (!Port.IsOpen)
                    retVal = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}

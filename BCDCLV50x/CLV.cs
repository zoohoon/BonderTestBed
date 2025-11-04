using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BCDCLV50x
{
    public class CLVBCDSensor : IHasSysParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public delegate void DataReadyEventHandler(string barcode);
        public event DataReadyEventHandler DataReadyEvent;

        Stopwatch stw = new Stopwatch();
        StringBuilder rcvSTB = new StringBuilder();
        private SerialPort serPort;
        private string _ReceivedBCD;
        public string ReceivedBCD
        {
            get { return _ReceivedBCD; }
            set
            {
                if (value != _ReceivedBCD)
                {
                    _ReceivedBCD = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _DataReady;
        public bool DataReady
        {
            get { return _DataReady; }
            set
            {
                if (value != _DataReady)
                {
                    _DataReady = value;
                    RaisePropertyChanged();
                }
            }
        }


        public CLVBCDSensor()
        {
            serPort = new SerialPort();
            serPort.PortName = "COM3";
            serPort.BaudRate = 9600;
            serPort.DataBits = 8;
            serPort.Parity = Parity.None;
            serPort.Handshake = Handshake.None;
            serPort.StopBits = StopBits.One;
            serPort.Encoding = Encoding.UTF8;
            serPort.DataReceived += Port_DataReceived;
            rcvSTB = new StringBuilder();
        }
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string rcstrbuf = serPort.ReadExisting();
            rcvSTB.Append(rcstrbuf);
            string rcvBuff = rcvSTB.ToString();
            if (rcvBuff.Contains("\r") || rcvBuff.Contains("\n"))
            {
                ReceivedBCD = rcvSTB.ToString();
                ReceivedBCD = ReceivedBCD.Replace("\r", "");
                DataReady = true;
                if (DataReadyEvent != null)
                {
                    DataReadyEvent(ReceivedBCD);
                }
                rcvSTB.Clear();
            }
        }
        public void Clear()
        {
            rcvSTB.Clear();
            DataReady = false;
        }
        public EventCodeEnum InitComm(int portnum)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                serPort.PortName = $"COM{portnum}";

                if (serPort.IsOpen)
                {
                    serPort.Close();
                }

                var PortNames = SerialPort.GetPortNames();

                var port = PortNames.FirstOrDefault(x => x == serPort.PortName);

                if (port != null)
                {
                    serPort.Open();

                    errorCode = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"[BCDCLV50x] InitComm(): serPort.PortName:{serPort.PortName} not exist.");
                }

            }
            catch (Exception err)
            {
                errorCode = EventCodeEnum.IO_PORT_ERROR;
                LoggerManager.Error($"InitComm(): Error occurred. Err = {err.Message}");
            }
            return errorCode;
        }
        public void DeInit()
        {
            try
            {
                if (serPort != null)
                {
                    if (serPort.IsOpen == true)
                    {
                        serPort.Close();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"DeInit(): BCD Deinit. Error occurred. Err = {err.Message}");
            }
            serPort = null;
        }

        public EventCodeEnum LoadSysParameter()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveSysParameter()
        {
            throw new NotImplementedException();
        }
    }
}

using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using ProberInterfaces.Network;
using ProberInterfaces.Enum;

namespace TCPIPParamObject
{
    [Serializable]
    public class TCPIPSysParam : ISystemParameterizable, INotifyPropertyChanged, ITCPIPSysParam, IParam, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.IP.Value.IP == string.Empty || this.IP.Value.IP == null)
                {
                    this.IP.Value = IPAddressVer4.GetData(this.IP.Value.IP);
                }

                //if (this.Terminator.Value == string.Empty || this.Terminator.Value == null)
                //{
                //    this.Terminator.Value = "\r\n";

                //    LoggerManager.Debug($"[TCPIPSysParam], Init() : Terminator value initialized. value = {this.Terminator.Value}");
                //}

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {
            this.EnumTCPIPOnOff.ElementName = "TCP/IP Enable";
            this.EnumTCPIPOnOff.ElementAdmin = "Alvin";
            this.EnumTCPIPOnOff.CategoryID = "30003";
            this.EnumTCPIPOnOff.Description = "Determines whether to use communication or not";

            this.EnumTCPIPComType.ElementName = "TCP/IP Type";
            this.EnumTCPIPComType.ElementAdmin = "Alvin";
            this.EnumTCPIPComType.CategoryID = "30003";
            this.EnumTCPIPComType.Description = "Type of TCP/IP communication driver";

            this.SendPort.ElementName = "SendPort";
            this.SendPort.ElementAdmin = "Alvin";
            this.SendPort.CategoryID = "30003";
            this.SendPort.Description = "Send Port number used for TCP/IP communication";

            this.ReceivePort.ElementName = "ReceivePort";
            this.ReceivePort.ElementAdmin = "Alvin";
            this.ReceivePort.CategoryID = "30003";
            this.ReceivePort.Description = "Receive Port number used for TCP/IP communication";

            this.IP.ElementName = "IP address";
            this.IP.ElementAdmin = "Alvin";
            this.IP.CategoryID = "30003";
            this.IP.Description = "IPv4 type IP address used for TCP/IP communication";

            //this.Terminator.ElementName = "Terminator";
            //this.Terminator.ElementAdmin = "Alvin";
            //this.Terminator.CategoryID = "30003";
            //this.Terminator.Description = "Set Terminator for Read and Write Communication";

            this.SendDelayTime.ElementName = "Send delay time";
            this.SendDelayTime.ElementAdmin = "Alvin";
            this.SendDelayTime.CategoryID = "30003";
            this.SendDelayTime.Description = "Set delay time when sending a response.";

            this.EnumBinType.ElementName = "Bin Type";
            this.EnumBinType.ElementAdmin = "Alvin";
            this.EnumBinType.CategoryID = "30003";
            this.EnumBinType.Description = "Set bin type.";
        }

        [ParamIgnore]
        public string FilePath { get; } = "TCPIP";

        [ParamIgnore]
        public string FileName { get; } = "TCPIPSystemParam.Json";
        public string Genealogy { get; set; } = "TCPIPSysParam";
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        private Element<EnumTCPIPEnable> _EnumTCPIPOnOff = new Element<EnumTCPIPEnable>();
        public Element<EnumTCPIPEnable> EnumTCPIPOnOff
        {
            get { return _EnumTCPIPOnOff; }
            set
            {
                if (value != _EnumTCPIPOnOff)
                {
                    _EnumTCPIPOnOff = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumTCPIPCommType> _EnumTCPIPComType = new Element<EnumTCPIPCommType>();
        public Element<EnumTCPIPCommType> EnumTCPIPComType
        {
            get { return _EnumTCPIPComType; }
            set
            {
                if (value != _EnumTCPIPComType)
                {
                    _EnumTCPIPComType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SendPort = new Element<int>();
        public Element<int> SendPort
        {
            get { return _SendPort; }
            set
            {
                if (value != _SendPort)
                {
                    _SendPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ReceivePort = new Element<int>();
        public Element<int> ReceivePort
        {
            get { return _ReceivePort; }
            set
            {
                if (value != _ReceivePort)
                {
                    _ReceivePort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<IPAddressVer4> _IP = new Element<IPAddressVer4>();
        public Element<IPAddressVer4> IP
        {
            get { return _IP; }
            set
            {
                if (value != _IP)
                {
                    _IP = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<string> _Terminator = new Element<string>();
        //public Element<string> Terminator
        //{
        //    get { return _Terminator; }
        //    set
        //    {
        //        if (value != _Terminator)
        //        {
        //            _Terminator = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<int> _SendDelayTime = new Element<int>();
        public Element<int> SendDelayTime
        {
            get { return _SendDelayTime; }
            set
            {
                if (value != _SendDelayTime)
                {
                    _SendDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<BinType> _EnumBinType = new Element<BinType>();
        public Element<BinType> EnumBinType
        {
            get { return _EnumBinType; }
            set
            {
                if (value != _EnumBinType)
                {
                    _EnumBinType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _InitializeConnect = new Element<bool>(false);
        public Element<bool> InitializeConnect
        {
            get { return _InitializeConnect; }
            set
            {
                if (value != _InitializeConnect)
                {
                    _InitializeConnect = value;
                    RaisePropertyChanged();
                }
            }
        }

        public TCPIPSysParam()
        {
        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                {
                    EnumTCPIPOnOff.Value = EnumTCPIPEnable.ENABLE;
                    EnumTCPIPComType.Value = EnumTCPIPCommType.REAL;

                    IP.Value = IPAddressVer4.GetData("127.0.0.1");
                    SendPort.Value = 7109;
                    ReceivePort.Value = 7009;
                }
                else
                {
                    EnumTCPIPOnOff.Value = EnumTCPIPEnable.DISABLE;
                    EnumTCPIPComType.Value = EnumTCPIPCommType.EMUL;
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw new Exception("Error during Setting Default Param From TCPIPSysParam.");
            }

            return RetVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
    }
}

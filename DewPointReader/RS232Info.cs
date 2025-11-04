using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProberInterfaces.Error;
using System.Xml.Serialization;
using ProberErrorCode;
using LogModule;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Temperature.Temp.DewPoint
{
    public enum DPState
    {
        DISCONNECT,
        CONNECT
    }

    [Serializable]
    public class DPComm_RS232Info : INotifyPropertyChanged, ISystemParameterizable, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }


        public ErrorCodeEnum Init()
        {
            ErrorCodeEnum retval = ErrorCodeEnum.UNDEFINED;

            try
            {
                retval = ErrorCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = ErrorCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void PropertyChange(String propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public void SetElementMetaData()
        {

        }
        public ErrorCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public ErrorCodeEnum SetDefaultParam()
        {
            IsAttached.Value = false;
            PortName.Value = "COM1";
            BaudRate.Value = 9600;
            DataBits.Value = 8;
            StopBits.Value = System.IO.Ports.StopBits.One;
            Parity.Value = System.IO.Ports.Parity.None;

            ModAddress.Value = 1;
            ModADCH.Value = 0;

            return ErrorCodeEnum.NONE;
        }

        private Element<bool> _IsAttached
             = new Element<bool>();
        public Element<bool> IsAttached
        {
            get
            {
                return _IsAttached;
            }
            set
            {
                _IsAttached = value;
                PropertyChange(nameof(IsAttached));
            }
        }

        private Element<String> _PortName
             = new Element<string>();
        public Element<String> PortName
        {
            get { return _PortName; }
            set
            {
                if (_PortName != value)
                {
                    _PortName = value;
                    PropertyChange(nameof(PortName));
                }
            }
        }
        private Element<int> _BaudRate
             = new Element<int>();
        public Element<int> BaudRate
        {
            get { return _BaudRate; }
            set
            {
                _BaudRate = value;
                PropertyChange(nameof(BaudRate));
            }
        }
        private Element<int> _DataBits
             = new Element<int>();
        public Element<int> DataBits
        {
            get { return _DataBits; }
            set
            {
                _DataBits = value;
                PropertyChange(nameof(DataBits));
            }
        }
        private Element<StopBits> _StopBits
             = new Element<StopBits>();
        public Element<StopBits> StopBits
        {
            get { return _StopBits; }
            set
            {
                _StopBits = value;
                PropertyChange(nameof(StopBits));
            }
        }
        private Element<Parity> _Parity
             = new Element<Parity>();
        public Element<Parity> Parity
        {
            get { return _Parity; }
            set
            {
                _Parity = value;
                PropertyChange(nameof(Parity));
            }
        }

        private Element<int> _ModAddress
             = new Element<int>();
        public Element<int> ModAddress
        {
            get { return _ModAddress; }
            set
            {
                _ModAddress = value;
                PropertyChange(nameof(ModAddress));
            }
        }

        private Element<int> _ModADCH
             = new Element<int>();
        public Element<int> ModADCH
        {
            get { return _ModADCH; }
            set
            {
                _ModADCH = value;
                PropertyChange(nameof(ModADCH));
            }
        }
        [ParamIgnore]
        public string FilePath { get; } = "Temperature";
        [ParamIgnore]
        public string FileName { get; } = "DewPointCommParam.Json";
        [ParamIgnore]
        public string Genealogy { get; set; }
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

    }
}

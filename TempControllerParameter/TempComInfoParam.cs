using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Temperature.TempManager;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TempControllerParameter
{
    [Serializable]
    public class TempCommInfoParam : IParamNode, ITempCommInfoParam
    {
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


        private Element<bool> _IsAttached
             = new Element<bool>();
        public Element<bool> IsAttached
        {
            get { return _IsAttached; }
            set
            {
                _IsAttached = value;
            }
        }

        private Element<string> _SerialPort
             = new Element<string>();
        public Element<string> SerialPort
        {
            get { return _SerialPort; }
            set { _SerialPort = value; }
        }

        private Element<byte> _Unitidentifier
             = new Element<byte>();
        public Element<byte> Unitidentifier
        {
            get { return _Unitidentifier; }
            set
            {
                _Unitidentifier = value;
            }
        }

        private Element<bool> _Init_WriteEnable
             = new Element<bool>();
        public Element<bool> Init_WriteEnable
        {
            get { return _Init_WriteEnable; }
            set
            {
                _Init_WriteEnable = value;
            }
        }

        private Element<int> _GetCurTempLoopTime
             = new Element<int>();
        public Element<int> GetCurTempLoopTime
        {
            get { return _GetCurTempLoopTime; }
            set
            {
                _GetCurTempLoopTime = value;
            }
        }

        private Element<int> _HCType
             = new Element<int>();
        public Element<int> HCType
        {
            get { return _HCType; }
            set
            {
                _HCType = value;
            }
        }

        private Element<int> _SetTemp
             = new Element<int>();
        public Element<int> SetTemp
        {
            get { return _SetTemp; }
            set
            {
                _SetTemp = value;
            }
        }

        public List<object> Nodes { get ; set ; }

        public EventCodeEnum SetDefaultData()
        {
            IsAttached.Value = true;
            SerialPort.Value = "COM4";
            Unitidentifier.Value = 1;
            Init_WriteEnable.Value = true;
            GetCurTempLoopTime.Value = 1000;
            HCType.Value = 2;
            SetTemp.Value = 300;

            return EventCodeEnum.NONE;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO.Ports;

using ProberErrorCode;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using ProberInterfaces.Communication;

namespace ProberInterfaces.RFID
{
    public enum EnumRFIDProtocolType
    {
        SINGLE,
        MULTIPLE
    }
    public enum EnumRFIDModuleType
    {
        FOUP,
        PROBECARD
    }
    public enum EnumRFIDModbusReadDataType
    {
        TAGID,
        ASCII,
        CUSTOM
    }
    interface IRFIDParameters
    {
        Element<int> RFIDIndex { get; set; }
        Element<string> SerialPort { get; set; }
        Element<int> BaudRate { get; set; }
        Element<int> DataBits { get; set; }
        Element<StopBits> StopBitsEnum { get; set; }
        Element<int> Port { get; set; }
        Element<string> IP { get; set; }
        Element<EnumCommmunicationType> ModuleCommType { get; set; }
        Element<bool> ModuleAttached { get; set; }
    }
    [Serializable]
    public class RFIDSysParameters : ISystemParameterizable, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        public string FilePath { get; } = "";
        public string FileName { get; } = "RFIDSysParam.json";
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void DefaultSetting()
        {
            try
            {
                RFIDProtocolType = EnumRFIDProtocolType.SINGLE;

                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    RFIDParams.Add(new RFIDParameter());
                    RFIDParams[i].RFIDIndex.Value = i;
                    RFIDParams[i].SerialPort.Value = "COM3";
                    RFIDParams[i].BaudRate.Value = 9600;
                    RFIDParams[i].DataBits.Value = 8;
                    RFIDParams[i].StopBitsEnum.Value = StopBits.One;
                    RFIDParams[i].ModuleAttached.Value = false;
                    RFIDParams[i].ModuleCommType.Value = EnumCommmunicationType.EMUL;
                    RFIDParams[i].Port.Value = 7090;
                    RFIDParams[i].IP.Value = "127.0.0.1";
                    RFIDParams[i].ModbusReadDataType.Value = EnumRFIDModbusReadDataType.TAGID;
                    RFIDParams[i].ModbusRegisterNumber.Value = "A000";
                    RFIDParams[i].ModbusWordCount.Value = "0004";
                }

                ProbeCardRFIDParams.Add(new RFIDParameter());
                ProbeCardRFIDParams[0].RFIDIndex.Value = 0;
                ProbeCardRFIDParams[0].SerialPort.Value = "COM5";
                ProbeCardRFIDParams[0].BaudRate.Value = 9600;
                ProbeCardRFIDParams[0].DataBits.Value = 8;
                ProbeCardRFIDParams[0].StopBitsEnum.Value = StopBits.One;
                ProbeCardRFIDParams[0].ModuleAttached.Value = false;
                ProbeCardRFIDParams[0].ModuleCommType.Value = EnumCommmunicationType.EMUL;
                ProbeCardRFIDParams[0].Port.Value = 7090;
                ProbeCardRFIDParams[0].IP.Value = "192.168.1.200";
                ProbeCardRFIDParams[0].ModbusReadDataType.Value = EnumRFIDModbusReadDataType.TAGID;
                ProbeCardRFIDParams[0].ModbusRegisterNumber.Value = "A000";
                ProbeCardRFIDParams[0].ModbusWordCount.Value = "0004";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                //TODO ReturnValue 다시 할 수 있게 
                DefaultSetting();
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        public void SetElementMetaData()
        {

        }
        
        private ObservableCollection<RFIDParameter> _RFIDParams
     = new ObservableCollection<RFIDParameter>();
        public ObservableCollection<RFIDParameter> RFIDParams
        {
            get { return _RFIDParams; }
            set
            {
                if (value != _RFIDParams)
                {
                    _RFIDParams = value;
                }
            }
        }

        private ObservableCollection<RFIDParameter> _ProbeCardRFIDParams = new ObservableCollection<RFIDParameter>();
        public ObservableCollection<RFIDParameter> ProbeCardRFIDParams
        {
            get { return _ProbeCardRFIDParams; }
            set
            {
                if (value != _ProbeCardRFIDParams)
                {
                    _ProbeCardRFIDParams = value;
                }
            }
        }

        private EnumRFIDProtocolType _RFIDProtocolType;
        public EnumRFIDProtocolType RFIDProtocolType
        {
            get { return _RFIDProtocolType; }
            set
            {
                if (value != _RFIDProtocolType)
                {
                    _RFIDProtocolType = value;
                }
            }
        }

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

    public class RFIDParameter : IRFIDParameters
    {
        private Element<int> _RFIDIndex = new Element<int>();
        public Element<int> RFIDIndex
        {
            get { return _RFIDIndex; }
            set { _RFIDIndex = value; }
        }
        private Element<string> _SerialPort = new Element<string>();
        public Element<string> SerialPort
        {
            get { return _SerialPort; }
            set { _SerialPort = value; }
        }
        private Element<int> _BaudRate = new Element<int>();
        public Element<int> BaudRate
        {
            get { return _BaudRate; }
            set { _BaudRate = value; }
        }
        private Element<int> _DataBits = new Element<int>();
        public Element<int> DataBits
        {
            get { return _DataBits; }
            set { _DataBits = value; }
        }
        private Element<StopBits> _StopBits = new Element<StopBits>();
        public Element<StopBits> StopBitsEnum
        {
            get { return _StopBits; }
            set { _StopBits = value; }
        }
        private Element<bool> _ModuleAttached = new Element<bool>();
        public Element<bool> ModuleAttached
        {
            get { return _ModuleAttached; }
            set { _ModuleAttached = value; }
        }
        private Element<EnumCommmunicationType> _ModuleCommType = new Element<EnumCommmunicationType>();
        public Element<EnumCommmunicationType> ModuleCommType
        {
            get { return _ModuleCommType; }
            set { _ModuleCommType = value; }
        }

        private Element<string> _IP = new Element<string>();
        public Element<string> IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

        private Element<int> _Port = new Element<int>();
        public Element<int> Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        private Element<EnumRFIDModbusReadDataType> _ModbusReadDataType = new Element<EnumRFIDModbusReadDataType>();
        public Element<EnumRFIDModbusReadDataType> ModbusReadDataType
        {
            get { return _ModbusReadDataType; }
            set { _ModbusReadDataType = value; }
        }

        private Element<string> _ModbusRegisterNumber = new Element<string>();
        public Element<string> ModbusRegisterNumber
        {
            get { return _ModbusRegisterNumber; }
            set { _ModbusRegisterNumber = value; }
        }

        private Element<string> _ModbusWordCount = new Element<string>();
        public Element<string> ModbusWordCount
        {
            get { return _ModbusWordCount; }
            set { _ModbusWordCount = value; }
        }
    }

}
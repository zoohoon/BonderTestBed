using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces.Communication;
using ProberInterfaces.EnvControl.Enum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ProberInterfaces.Temperature.EnvMonitoring
{
    public class SensorParameters : INotifyPropertyChanged, ISystemParameterizable, IParamNode
    {        
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChange([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));    

        private List<CommunicationParameterBase> _SensorCommParams
            = new List<CommunicationParameterBase>();
        public List<CommunicationParameterBase> SensorCommParams
        {
            get { return _SensorCommParams; }
            set
            {
                if (value != _SensorCommParams)
                {
                    _SensorCommParams = value;
                    RaisePropertyChange();
                }
            }
        }

        private List<SensorSysParameter> _SensorSysParams
            = new List<SensorSysParameter>();
        public List<SensorSysParameter> SensorSysParams
        {
            get { return _SensorSysParams; }
            set
            {
                if (value != _SensorSysParams)
                {
                    _SensorSysParams = value;
                    RaisePropertyChange();
                }
            }
        }

        private Element<int> _SensorMaxCount = new Element<int>();
        public Element<int> SensorMaxCount
        {
            get { return _SensorMaxCount; }
            set
            {
                if (value != _SensorMaxCount)
                {
                    _SensorMaxCount = value;                    
                }
            }
        }

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
        public void SetElementMetaData()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                DefaultSetting();
                ret = EventCodeEnum.NONE;                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public void DefaultSetting()
        {
            try
            {
                SensorMaxCount.Value = 32;
                // 실제 통신 담당하는 포트 파라미터
                SensorCommParams.Add(new CommunicationParameterBase());                
                SensorCommParams[0].PortName.Value = "COM5";
                SensorCommParams[0].StopBits.Value = StopBits.One;
                SensorCommParams[0].DataBits.Value = 8;
                SensorCommParams[0].BaudRate.Value = 115200;                
                SensorCommParams[0].Hub.Value = "HUB1";
                SensorCommParams[0].ModuleCommType.Value = EnumCommmunicationType.SERIAL;
                SensorCommParams[0].IntervalTime.Value = 1000;       /// Milliseconds 

                // 위의 포트에 연결되어있는 센서 파라미터
                SensorSysParams.Add(new SensorSysParameter());
                SensorSysParams[0].Sensor = new SmokeSensorSettingParam();
                SensorSysParams[0].Sensor.SensorAlias.Value = "Front";
                SensorSysParams[0].Hub.Value = "HUB1";                
                SensorSysParams[0].Index.Value = 1;
                SensorSysParams[0].ModuleAttached.Value = true;                
                SensorSysParams[0].RunMode.Value = "NONE";
                SensorSysParams[0].ModuleEnable.Value = false;
                SensorSysParams[0].ProtocolType.Value = "ONOFF";

                SensorSysParams.Add(new SensorSysParameter());
                SensorSysParams[1].Sensor = new SmokeSensorSettingParam();
                SensorSysParams[1].Sensor.SensorAlias.Value = "Back";                
                SensorSysParams[1].Hub.Value = "HUB1";                
                SensorSysParams[1].Index.Value = 2;
                SensorSysParams[1].ModuleAttached.Value = true;                
                SensorSysParams[1].RunMode.Value = "NONE";
                SensorSysParams[1].ModuleEnable.Value = false;
                SensorSysParams[1].ProtocolType.Value = "ONOFF";

                SensorSysParams.Add(new SensorSysParameter());
                SensorSysParams[2].Sensor = new SmokeSensorSettingParam();
                SensorSysParams[2].Sensor.SensorAlias.Value = "Right";
                SensorSysParams[2].Hub.Value = "HUB1";
                SensorSysParams[2].Index.Value = 3;
                SensorSysParams[2].ModuleAttached.Value = true;
                SensorSysParams[2].RunMode.Value = "NONE";
                SensorSysParams[2].ModuleEnable.Value = false;
                SensorSysParams[2].ProtocolType.Value = "ONOFF";

                SensorSysParams.Add(new SensorSysParameter());
                SensorSysParams[3].Sensor = new SmokeSensorSettingParam();
                SensorSysParams[3].Sensor.SensorAlias.Value = "Left";
                SensorSysParams[3].Hub.Value = "HUB1";
                SensorSysParams[3].Index.Value = 4;
                SensorSysParams[3].ModuleAttached.Value = true;
                SensorSysParams[3].RunMode.Value = "NONE";
                SensorSysParams[3].ModuleEnable.Value = false;
                SensorSysParams[3].ProtocolType.Value = "ONOFF";
            }
            catch (Exception err) 
            {
                LoggerManager.Exception(err);
                throw;
            }
        }        

        [ParamIgnore]
        public string FilePath { get; } = "Temperature";
        [ParamIgnore]
        public string FileName { get; } = nameof(SensorParameters) + ".Json";
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

    public abstract class SensorInfo : ISensorInfo
    {
        public abstract void SetData(ISensorInfo sensorInfo);
    }

    // User setting value & UI Binding
    public class SmokeSensorInfo : SensorInfo, INotifyPropertyChanged
    {
        public override void SetData(ISensorInfo sensorInfo)
        {

        }
        #region // ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion            

        private Element<double> _CurTemp
            = new Element<double>();
        [JsonIgnore, ParamIgnore]
        public Element<double> CurTemp
        {
            get { return _CurTemp; }
            set
            {
                if (value != _CurTemp)
                {
                    _CurTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _CurHumi
            = new Element<double>();
        [JsonIgnore, ParamIgnore]
        public Element<double> CurHumi
        {
            get { return _CurHumi; }
            set
            {
                if (value != _CurHumi)
                {
                    _CurHumi = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _WarningTemp
             = new Element<double>();
        [JsonIgnore, ParamIgnore]
        public Element<double> WarningTemp
        {
            get { return _WarningTemp; }
            set
            {
                if (value != _WarningTemp)
                {
                    _WarningTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _AlarmTemp
            = new Element<double>();
        [JsonIgnore, ParamIgnore]
        public Element<double> AlarmTemp
        {
            get { return _AlarmTemp; }
            set
            {
                if (value != _AlarmTemp)
                {
                    _AlarmTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _TempDeviation
            = new Element<double>();
        [JsonIgnore, ParamIgnore]
        public Element<double> TempDeviation
        {
            get { return _TempDeviation; }
            set
            {
                if (value != _TempDeviation)
                {
                    _TempDeviation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _bSmoke_Detect
            = new Element<bool>();
        [JsonIgnore, ParamIgnore]
        public Element<bool> bSmoke_Detect
        {
            get { return _bSmoke_Detect; }
            set
            {
                if (value != _bSmoke_Detect)
                {
                    _bSmoke_Detect = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _bTemp_Warning
            = new Element<bool>();
        [JsonIgnore, ParamIgnore]
        public Element<bool> bTemp_Warning
        {
            get { return _bTemp_Warning; }
            set
            {
                if (value != _bTemp_Warning)
                {
                    _bTemp_Warning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _bTemp_Alarm
            = new Element<bool>();
        [JsonIgnore, ParamIgnore]
        public Element<bool> bTemp_Alarm
        {
            get { return _bTemp_Alarm; }
            set
            {
                if (value != _bTemp_Alarm)
                {
                    _bTemp_Alarm = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _bDisconnect_Sensor
            = new Element<bool>();
        [JsonIgnore, ParamIgnore]
        public Element<bool> bDisconnect_Sensor
        {
            get { return _bDisconnect_Sensor; }
            set
            {
                if (value != _bDisconnect_Sensor)
                {
                    _bDisconnect_Sensor = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SmokeSensorInfo()
        {
            
        }
    }

    public class AnonymousSensorSettingParam
    {
        private Element<string> _SensorAlias
            = new Element<string>();        
        public Element<string> SensorAlias
        {
            get { return _SensorAlias; }
            set { _SensorAlias = value; }            
        }
    }

    // User setting value & save parameter
    public class SmokeSensorSettingParam : AnonymousSensorSettingParam
    {
        private Element<double> _WarningTemp
            = new Element<double>();        
        public Element<double> WarningTemp
        {
            get { return _WarningTemp; }
            set { _WarningTemp = value; }            
        }

        private Element<double> _AlarmTemp
            = new Element<double>();        
        public Element<double> AlarmTemp
        {
            get { return _AlarmTemp; }
            set { _AlarmTemp = value; }            
        }

        private Element<double> _TempDeviation
             = new Element<double>();        
        public Element<double> TempDeviation
        {
            get { return _TempDeviation; }
            set { _TempDeviation = value; }            
        }


    }

    
    public class SensorSysParameter
    {
        // 센서 자체가 가지고있는 고유값
        private AnonymousSensorSettingParam _Sensor = new AnonymousSensorSettingParam();
        public AnonymousSensorSettingParam Sensor
        {
            get { return _Sensor; }
            set { _Sensor = value; }
        }

        // 범용적으로 쓸 센서 파라미터들   
        private Element<string> _Hub = new Element<string>();
        public Element<string> Hub
        {
            get { return _Hub; }
            set { _Hub = value; }
        }

        private Element<int> _Index = new Element<int>();
        public Element<int> Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        private Element<bool> _ModuleAttached = new Element<bool>();
        public Element<bool> ModuleAttached
        {
            get { return _ModuleAttached; }
            set { _ModuleAttached = value; }
        }

        private Element<bool> _ModuleEnable = new Element<bool>();
        public Element<bool> ModuleEnable
        {
            get { return _ModuleEnable; }
            set { _ModuleEnable = value; }
        }

        private Element<string> _RunMode = new Element<string>();
        public Element<string> RunMode
        {
            get { return _RunMode; }
            set { _RunMode = value; }
        }

        // ex) MODBUS, ONOFF
        private Element<string> _ProtocolType = new Element<string>();
        public Element<string> ProtocolType
        {
            get { return _ProtocolType; }
            set { _ProtocolType = value; }
        }
    }

}

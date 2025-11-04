using LogModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ProberInterfaces.Temperature
{
    [DataContract]
    public class TemperatureInfo : IHasComParameterizable, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public TemperatureInfo()
        {
            if (SetTemp == null)
                SetTemp = new Element<double>();

            SetTemp.PropertyChanged -= SetTemp_PropertyChanged;
            SetTemp.PropertyChanged += SetTemp_PropertyChanged;

            TargetTemp.Value = TargetTempDefaultValue;
        }
      

        ~TemperatureInfo()
        {
            SetTemp.PropertyChanged -= SetTemp_PropertyChanged;
        }

        private void SetTemp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SETTEMP, SetTemp.Value.ToString());
        }


        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public object Owner { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public List<Object> Nodes { get; set; }

        private Element<double> _CurTemp = new Element<double>();
        [DataMember]
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

        // not 1/10!!
        private Element<double> _PreSetTemp = new Element<double>();
        [DataMember]
        public Element<double> PreSetTemp
        {
            get { return _PreSetTemp; }
            set
            {
                if (value != _PreSetTemp)
                {
                    _PreSetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

     


        public double TargetTempDefaultValue { get; } = -999;

        private Element<double> _TargetTemp = new Element<double>();
        public Element<double> TargetTemp
        {
            get { return _TargetTemp; }
            set
            {
                if (value != _TargetTemp)
                {
                    _TargetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DeviceSetTemp;
        public double DeviceSetTemp
        {
            get { return _DeviceSetTemp; }
            set
            {
                if (value != _DeviceSetTemp)
                {
                    _DeviceSetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SetTemp = new Element<double>();
        [DataMember]
        public Element<double> SetTemp 
        {
            get
            {
                return _SetTemp;
            }
            private set
            {
                if (_SetTemp != value)
                {
                    _SetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MV = new Element<double>();
        public Element<double> MV
        {
            get { return _MV; }
            set
            {
                if (value != _MV)
                {
                    _MV = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _DewPoint = new Element<double>();
        public Element<double> DewPoint
        {
            get { return _DewPoint; }
            set
            {
                if (value != _DewPoint)
                {
                    _DewPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TempMonitoringInfo _MonitoringInfo = new TempMonitoringInfo();
        public TempMonitoringInfo MonitoringInfo
        {
            get { return _MonitoringInfo; }
            set
            {
                if (value != _MonitoringInfo)
                {
                    _MonitoringInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OverHeatingOffset;
        public double OverHeatingOffset
        {
            get { return _OverHeatingOffset; }
            set
            {
                if (value != _OverHeatingOffset)
                {
                    _OverHeatingOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OverHeatingHysteresis;
        public double OverHeatingHysteresis
        {
            get { return _OverHeatingHysteresis; }
            set
            {
                if (value != _OverHeatingHysteresis)
                {
                    _OverHeatingHysteresis = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _TemperatureWIthInDeviationFlag;
        /// <summary>
        /// SV 설정 이후 PV 가 deviation 내에 들어온적이 있으면 true, 없으면 false. 
        /// 기본값은 false 이고, 프로그램 실행 시에도 false 임.
        /// (Module State 로 판단하기 어려운 이유는 , Monitoring 상태에서 Running 상태로 넘어갈 수 있기 때문)
        /// </summary>
        public bool TemperatureWIthInDeviationFlag
        {
            get { return _TemperatureWIthInDeviationFlag; }
            private set
            {
                if (value != _TemperatureWIthInDeviationFlag)
                {
                    _TemperatureWIthInDeviationFlag = value;
                    RaisePropertyChanged();
                }
            }
        }


        public void SetElementMetaData()
        {
            
        }

        public void SetTemperatureWithInDeviation(bool flag)
        {
            try
            {
                TemperatureWIthInDeviationFlag = false;
                LoggerManager.Debug($"[TemperatrueInfo] TemperatureInOfDeviationFlag set to {TemperatureWIthInDeviationFlag}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    /// <summary>
    /// SetTemp를 어떤 값으로 했는 지에 대한 데이터 
    /// </summary>
    [Serializable, DataContract]
    public class TempEventInfo 
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public TempEventInfo(TemperatureChangeSource source, double setTemp)
        {
            this.TempChangeSource = source;
            this.SetTemp = setTemp;
        }

        private TemperatureChangeSource _TempChangeSource;
        [DataMember]
        public TemperatureChangeSource TempChangeSource
        {
            get { return _TempChangeSource; }
            set
            {
                if (value != _TempChangeSource)
                {
                    _TempChangeSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SetTemp;
        [DataMember]
        public double SetTemp
        {
            get
            {
                return _SetTemp;
            }
            set
            {
                if (_SetTemp != value)
                {
                    _SetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    [Serializable, DataContract]
    public enum TemperatureChangeSource
    {
        [EnumMember]
        UNDEFINED,     // 이전 Source를 유지하고 TargetTemp를 변경한다.

        [EnumMember]
        TEMP_DEVICE,   // Device 온도 -> Host에서 설정하더라도 DeviceParam값이 반영되면 TEMP_DEVICE Source이다. 

        [EnumMember]
        TEMP_EXTERNAL, // Host, UI 등 외부에서 설정 -> SET_PARAMETER 로 설정하는 값은 DeviceParam에 적용하지 않는다.

        [EnumMember]
        CARD_CHANGE,    // Card Change 동작 온도        
    }
}

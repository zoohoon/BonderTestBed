using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TempControllerParameter;
using LogModule;
using Newtonsoft.Json;

namespace Temperature
{
    using ProberInterfaces;
    using ProberInterfaces.Temperature;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    [Serializable]
    public class TempControllerParam : ISystemParameterizable, IParamNode
    {       

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggingInterval.Value = 15;
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
            LoggingInterval.Description = "Logging interval";
            LoggingInterval.UpperLimit = 10000;
            LoggingInterval.LowerLimit = 1;
            LoggingInterval.CategoryID = "";
        }

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


        [ParamIgnore]
        public string FilePath { get; } = "Temperature";
        [ParamIgnore]
        public string FileName { get; } = "TempControllerParam.Json";

        private Element<TempModuleMode> _TempModuleMode
             = new Element<TempModuleMode>();
        [XmlElement(nameof(TempModuleMode))]
        public Element<TempModuleMode> TempModuleMode
        {
            get { return _TempModuleMode; }
            set
            {
                _TempModuleMode = value;
            }
        }


        private Element<long> _LimitRunTimeSeconds
             = new Element<long>();
        public Element<long> LimitRunTimeSeconds
        {
            get { return _LimitRunTimeSeconds; }
            set
            {
                _LimitRunTimeSeconds = value;
            }
        }

        // 1/10
        private Element<long> _FrontDoorOpenTemp
             = new Element<long>();
        public Element<long> FrontDoorOpenTemp
        {
            get { return _FrontDoorOpenTemp; }
            set
            {
                _FrontDoorOpenTemp = value;
            }
        }

        private Element<long> _LoggingInterval = new Element<long>();
        public Element<long> LoggingInterval
        {
            get { return _LoggingInterval; }
            set
            {
                _LoggingInterval = value;
            }
        }

        private Element<double> _MonitoringMVTimeInSec = new Element<double>() { Value = 300 };
        public Element<double> MonitoringMVTimeInSec
        {
            get { return _MonitoringMVTimeInSec; }
            set
            {
                if (value != _MonitoringMVTimeInSec)
                {
                    _MonitoringMVTimeInSec = value;

                }
            }
        }
        /// <summary>
        /// Reference temperature of Control Purge Air
        /// </summary>
        private Element<double> _RefTempOfControlPurgeAir = new Element<double>() { Value = 10 };
        public Element<double> RefTempOfControlPurgeAir
        {
            get { return _RefTempOfControlPurgeAir; }
            set
            {
                if (value != _RefTempOfControlPurgeAir)
                {
                    _RefTempOfControlPurgeAir = value;
                }
            }
        }
        /// <summary>
        /// SV와 PV를 보고 Purge Air On/Off 결정에 대한 hysteresis 값
        /// </summary>
        private Element<double> _HysteresisValue_PurgeAir = new Element<double>() { Value = 1 };
        public Element<double> HysteresisValue_PurgeAir
        {
            get { return _HysteresisValue_PurgeAir; }
            set
            {
                if (value != _HysteresisValue_PurgeAir)
                {
                    _HysteresisValue_PurgeAir = value;
                }
            }
        }

        private Element<bool> _ApplySVChangesBasedOnDevice = new Element<bool>() {Value = true};
        public Element<bool> ApplySVChangesBasedOnDevice
        {
            get { return _ApplySVChangesBasedOnDevice; }
            set
            {
                if (value != _ApplySVChangesBasedOnDevice)
                {
                    _ApplySVChangesBasedOnDevice = value;
                }
            }
        }
        //private Element<TemperatureChangeSource> _LastTempChangeSource = new Element<TemperatureChangeSource>();
        ///// <summary>
        ///// 마지막 설정 온도(현재 진행 온도)의 Source
        ///// </summary>
        //public Element<TemperatureChangeSource> LastTempChangeSource
        //{
        //    get { return _LastTempChangeSource; }
        //    set
        //    {
        //        if (value != _LastTempChangeSource)
        //        {
        //            _LastTempChangeSource = value;
        //        }
        //    }
        //}


        private Element<double> _LastSetTargetTemp = new Element<double>();
        /// <summary>
        /// 마지막 설정 온도(현재 진행 온도)
        /// </summary>
        public Element<double> LastSetTargetTemp
        {
            get { return _LastSetTargetTemp; }
            set
            {
                if (value != _LastSetTargetTemp)
                {
                    _LastSetTargetTemp = value;
                }
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            TempModuleMode.Value = TempControllerParameter.TempModuleMode.EMUL;
            LimitRunTimeSeconds.Value = 60 * 60;
            FrontDoorOpenTemp.Value = 30 * 10;

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                TempModuleMode.Value = TempControllerParameter.TempModuleMode.EMUL;
                LimitRunTimeSeconds.Value = 60 * 60;
                FrontDoorOpenTemp.Value = 30 * 10;
                LoggingInterval.Value = 15;
                //TempModuleMode.Value = TempControllerParameter.TempModuleMode.E5EN;
                //ChillerModuleMode.Value = Temperature.ChillerModuleMode.EMUL;
                //DryAirModuleMode.Value = Temperature.DryAirModuleMode.EMUL;
                //LimitRunTimeSeconds.Value = 5 * 60;
                //FrontDoorOpenTemp.Value = 40 * 10;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                
            }
            return retVal;
        }
    }

    [Serializable]
    public class TempBackupInfo : ISystemParameterizable
    {

        public string FilePath => "";

        public string FileName => "TempBackupInfo.json";

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

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }



        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
            return;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        private List<TempEventInfo> _TempInfoHistory = new List<TempEventInfo>();

        /// <summary>
        /// Source에 대해서 사용된 TempInfomation 값을 가지고 있음. 
        /// 새로운 Source로 인해서 설정되는 경우 
        /// </summary>
        public List<TempEventInfo> TempInfoHistory
        {
            get { return _TempInfoHistory; }
            set { _TempInfoHistory = value; }
        }

    }


    [Serializable]
    public class TempControllerDevParam : IDeviceParameterizable, IParamNode, ITempControllerDevParam
    {
        #region ==> PropertyChanged
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
                if (this.TempToleranceDeviation.LowerLimit <= 0)
                {
                    this.TempToleranceDeviation.LowerLimit = 1;
                }

                if (this.TempToleranceDeviation.UpperLimit == 0)
                {
                    this.TempToleranceDeviation.UpperLimit = 10;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[TempControllerDevParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {
            if(this.SetTemp.LowerLimit == 0)
            {
                this.SetTemp.LowerLimit = -55;
            }

            if (this.SetTemp.UpperLimit == 0)
            {
                this.SetTemp.UpperLimit = 200;
            }
        }

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


        [ParamIgnore]
        public string FilePath { get; } = "Temperature";
        [ParamIgnore]
        public string FileName { get; } = "TempControllerDevParam.json";

        //Set해야하는 온도.
        private Element<double> _SetTemp
             = new Element<double>();
        [DataMember]
        public Element<double> SetTemp
        {
            get { return _SetTemp; }
            set
            {
                _SetTemp = value;
                RaisePropertyChanged();
            }
        }

        //Set해야하는 온도로부터 Cur온도의 허용 오차.
        private Element<double> _TempToleranceDeviation
             = new Element<double>() { UpperLimit = 10.0, LowerLimit = 1.0 };
        [DataMember]
        public Element<double> TempToleranceDeviation
        {
            get { return _TempToleranceDeviation; }
            set
            {
                _TempToleranceDeviation = value;
            }
        }

        /// <summary>
        /// Probing(Contact) 중에 TempControllerState에서 Deviation 확인하고 Abort 하기 위한 값
        /// 
        /// </summary>
        private Element<double> _EmergencyAbortTempTolerance
             = new Element<double>() { UpperLimit = 20.0, LowerLimit = 2.0 };
        [DataMember]
        public Element<double> EmergencyAbortTempTolerance
        {
            get { return _EmergencyAbortTempTolerance; }
            set
            {
                _EmergencyAbortTempTolerance = value;
            }
        }

        //온도가 Deviation 범위 안에 못들었을 경우 작동해야하는 타입.
        private Element<TempPauseTypeEnum> _TempPauseType
             = new Element<TempPauseTypeEnum>();
        [DataMember]
        public Element<TempPauseTypeEnum> TempPauseType
        {
            get { return _TempPauseType; }
            set
            {
                _TempPauseType = value;
            }
        }

        //온도가 Deviation 범위 안에 못들었을 경우 작동해야하는 타입.
        private Element<bool> _IsBreakAlignState
             = new Element<bool>();
        [DataMember]
        public Element<bool> IsBreakAlignState
        {
            get { return _IsBreakAlignState; }
            set
            {
                _IsBreakAlignState = value;
            }
        }

        //Over Heating Mode Enable
        private Element<bool> _IsEnableOverHeating
             = new Element<bool>();
        [DataMember]
        public Element<bool> IsEnableOverHeating
        {
            get { return _IsEnableOverHeating; }
            set
            {
                _IsEnableOverHeating = value;
            }
        }

        //Over Heating Mode Enable 1/10 ex) 1도는 10. 10도는 100.
        private Element<long> _OverHeatingOffset
             = new Element<long>();
        [DataMember]
        public Element<long> OverHeatingOffset
        {
            get { return _OverHeatingOffset; }
            set
            {
                _OverHeatingOffset = value;
            }
        }

        //OverHeating을 위한 Deviation.
        //Deviation의 기준 온도는 OverHeating을 적용한 값이 아닌, Setting Temp 값이다.
        private Element<long> _DeviationForOverHeating
             = new Element<long>();
        [DataMember]
        public Element<long> DeviationForOverHeating
        {
            get { return _DeviationForOverHeating; }
            set
            {
                _DeviationForOverHeating = value;
            }
        }

        //Auto Cooling Enable
        private Element<bool> _IsEnableAutoCoolingControl
             = new Element<bool>();
        [DataMember]
        public Element<bool> IsEnableAutoCoolingControl
        {
            get { return _IsEnableAutoCoolingControl; }
            set
            {
                _IsEnableAutoCoolingControl = value;
            }
        }

        //Auto Cooling Activating Offset //1/10 (Cooling이 시작되어야하는온도 차이)
        private Element<long> _AutoCoolingActivatingOffset
             = new Element<long>();
        [DataMember]
        public Element<long> AutoCoolingActivatingOffset
        {
            get { return _AutoCoolingActivatingOffset; }
            set
            {
                _AutoCoolingActivatingOffset = value;
            }
        }

        //Auto Cooling Deactivating Offset //1/10 (Cooling이 꺼져야하는온도 차이)
        private Element<long> _AutoCoolingDectivatingOffset
             = new Element<long>();
        [DataMember]
        public Element<long> AutoCoolingDectivatingOffset
        {
            get { return _AutoCoolingDectivatingOffset; }
            set
            {
                _AutoCoolingDectivatingOffset = value;
            }
        }

        //Auto Cooling Activating Duration //sec (활성화된 Auto Cooling이 유지되는 시간)
        private Element<double> _AutoCoolingActivatingDurationSec
             = new Element<double>();
        [DataMember]
        public Element<double> AutoCoolingActivatingDurationSec
        {
            get { return _AutoCoolingActivatingDurationSec; }
            set
            {
                _AutoCoolingActivatingDurationSec = value;
            }
        }

        //Auto Cooling Deactivating Duration //sec (비활성화된 Auto Cooling이 유지되는 시간)
        private Element<double> _AutoCoolingDeactivatingDurationSec
             = new Element<double>();
        [DataMember]
        public Element<double> AutoCoolingDeactivatingDurationSec
        {
            get { return _AutoCoolingDeactivatingDurationSec; }
            set
            {
                _AutoCoolingDeactivatingDurationSec = value;
            }
        }

        //Auto Cooling Deactivating Temp //1/10 (Auto Cooling 작동이 안되는 온도)
        private Element<long> _AutoCoolingDeactivatingTemp
             = new Element<long>();
        [DataMember]
        public Element<long> AutoCoolingDeactivatingTemp
        {
            get { return _AutoCoolingDeactivatingTemp; }
            set
            {
                _AutoCoolingDeactivatingTemp = value;
            }
        }

        private Element<bool> _TemperatureMonitorEnable
             = new Element<bool>();
        /// <summary>
        /// Temperature Monitor Enable
        /// </summary>
        [DataMember]
        public Element<bool> TemperatureMonitorEnable
        {
            get { return _TemperatureMonitorEnable; }
            set
            {
                _TemperatureMonitorEnable = value;
            }
        }

        private Element<double> _WaitMonitorTimeSec
            = new Element<double>() { Value = 0.0};
        /// <summary>
        /// 컨텍 후, 온도 모니터링을 하기 전에 대기 시간(초)(별다른 이벤트등을 보내거나 온도확인은 안하는 것이지 멈춰있거나 하는것은 아님.)
        /// </summary>
        [DataMember]
        public Element<double> WaitMonitorTimeSec
        {
            get { return _WaitMonitorTimeSec; }
            set { _WaitMonitorTimeSec = value; }
        }

        private Element<double> _TempMonitorRange
             = new Element<double>();
        /// <summary>
        /// Temperature Monitor Range (+/-) [0.0 to 5.0(℃)]
        /// </summary>
        [DataMember]
        public Element<double> TempMonitorRange
        {
            get { return _TempMonitorRange; }
            set { _TempMonitorRange = value; }
        }

        private Element<double> _TempGEMToleranceDeviation
            = new Element<double>();
        /// <summary>
        /// Temperature Change value (0.1 to 5.0(℃) in 0.1 increments)  [0.1 to 5.0(℃)]
        /// GEM 에 Event 등을 보낼때 전용 Deviation
        /// v22.4 에서 Contact 중 알람 deviation 으로 변경 됨. device parameter 여서 이름을 변경하지 않음.
        /// “Temperature Change Value” 
        /// </summary>
        [DataMember]
        public Element<double> TempGEMToleranceDeviation 
        {
            get { return _TempGEMToleranceDeviation; }
            set { _TempGEMToleranceDeviation = value; }
        }

        private Element<bool> _AssistStopOnZDown
             = new Element<bool>();
        /// <summary>
        /// Assist Stop on Z-dn : probing 중에 OutRange 발생시 바로 z_down 하지않고 (웨이퍼나 카드 손상우려)
        /// Contact 이 끝난뒤 Z_down 후 Lot 를 중지시킨다.
        /// </summary>
        [DataMember]
        public Element<bool> AssistStopOnZDown
        {
            get { return _AssistStopOnZDown; }
            set { _AssistStopOnZDown = value; }
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                SetTemp.Value = 30;
                TempToleranceDeviation.Value = 1;
                TempPauseType.Value = TempPauseTypeEnum.NONE;
                IsBreakAlignState.Value = false;
                IsEnableOverHeating.Value = false;
                OverHeatingOffset.Value = 2;
                DeviationForOverHeating.Value = 1;
                IsEnableAutoCoolingControl.Value = true;
                AutoCoolingActivatingOffset.Value = 3;
                AutoCoolingDectivatingOffset.Value = 1;
                AutoCoolingActivatingDurationSec.Value = 2.0;
                AutoCoolingDeactivatingDurationSec.Value = 1.0;
                AutoCoolingDeactivatingTemp.Value = 30;
                EmergencyAbortTempTolerance.Value = 10;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                
            }
            return retVal;
        }
    }

    [Serializable]
    public class TempSafetyDevParam : IDeviceParameterizable, IParamNode, ITempSafetyDevParam
    {
        #region ==> PropertyChanged
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
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[TempSafetyDevParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {
            
        }

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


        [ParamIgnore]
        public string FilePath { get; } = "Temperature";
        [ParamIgnore]
        public string FileName { get; } = "TempSafetyDevParam.json";

        //Wafer Load중 온도안정화를 위해 삼발이 위에서 대기시키는 시간
        private Element<int> _WaferLoadDelay
             = new Element<int>() { UpperLimit = 60000, LowerLimit = 0};
        [DataMember]
        public Element<int> WaferLoadDelay
        {
            get { return _WaferLoadDelay; }
            set
            {
                _WaferLoadDelay = value;
                RaisePropertyChanged();
            }
        }

        //Wafer UnLoad중 온도안정화를 위해 삼발이 위에서 대기시키는 시간
        private Element<int> _WaferUnLoadDelay
             = new Element<int>() { UpperLimit = 60000, LowerLimit = 0 };
        [DataMember]
        public Element<int> WaferUnLoadDelay
        {
            get { return _WaferUnLoadDelay; }
            set
            {
                _WaferUnLoadDelay = value;
            }
        }



        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                WaferLoadDelay.Value = 0;
                WaferUnLoadDelay.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    [Serializable]
    public class TempSafetySysParam : ISystemParameterizable, IParamNode, ITempSafetySysParam
    {
        #region ==> PropertyChanged
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
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[TempSafetySysParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

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


        [ParamIgnore]
        public string FilePath { get; } = "Temperature";
        [ParamIgnore]
        public string FileName { get; } = "TempSafetySysParam.json";

        // Wafe Load 중 웨이퍼 대기를 하기 위한 상방 온도 조건
        // SV가 설정 온도보다 높은 경우 WaferLoad_HighTempSoakTime 만큼 대기 한다.
        private Element<int> _WaferLoad_SoakTempUpper = new Element<int>() { UpperLimit = 200, LowerLimit = 50, Value = 85};
        [DataMember]
        public Element<int> WaferLoad_SoakTempUpper
        {
            get { return _WaferLoad_SoakTempUpper; }
            set
            {
                _WaferLoad_SoakTempUpper = value;
                RaisePropertyChanged();
            }
        }

        // Wafe Load 중 웨이퍼 대기를 하기 위한 하방 온도 조건
        // SV가 설정 온도보다 낮은 경우 WaferLoad_LowTempSoakTime 만큼 대기 한다.
        private Element<int> _WaferLoad_SoakTempLower = new Element<int>() { UpperLimit = 30, LowerLimit = -70, Value = 0 };
        [DataMember]
        public Element<int> WaferLoad_SoakTempLower
        {
            get { return _WaferLoad_SoakTempLower; }
            set
            {
                _WaferLoad_SoakTempLower = value;
                RaisePropertyChanged();
            }
        }

        //Wafer Load중 온도안정화를 위해 삼발이 위에서 대기시키는 시간 (SV > WaferLoad_SoakTempUpper 인 경우)
        private Element<int> _WaferLoad_HighTempSoakTime = new Element<int>() { UpperLimit = 60000, LowerLimit = 0 };
        [DataMember]
        public Element<int> WaferLoad_HighTempSoakTime
        {
            get { return _WaferLoad_HighTempSoakTime; }
            set
            {
                _WaferLoad_HighTempSoakTime = value;
                RaisePropertyChanged();
            }
        }

        //Wafer Load중 온도안정화를 위해 삼발이 위에서 대기시키는 시간 (SV < WaferLoad_SoakTempLower 인 경우)
        private Element<int> _WaferLoad_LowTempSoakTime = new Element<int>() { UpperLimit = 60000, LowerLimit = 0 };
        [DataMember]
        public Element<int> WaferLoad_LowTempSoakTime
        {
            get { return _WaferLoad_LowTempSoakTime; }
            set
            {
                _WaferLoad_LowTempSoakTime = value;
                RaisePropertyChanged();
            }
        }

        // Wafe UnLoad 중 웨이퍼 대기를 하기 위한 상방 온도 조건
        // SV가 설정 온도보다 높은 경우 WaferUnLoad_HighTempSoakTime 만큼 대기 한다.
        private Element<int> _WaferUnLoad_SoakTempUpper = new Element<int>() { UpperLimit = 200, LowerLimit = 50, Value = 85};
        [DataMember]
        public Element<int> WaferUnLoad_SoakTempUpper
        {
            get { return _WaferUnLoad_SoakTempUpper; }
            set
            {
                _WaferUnLoad_SoakTempUpper = value;
                RaisePropertyChanged();
            }
        }

        // Wafe UnLoad 중 웨이퍼 대기를 하기 위한 하방 온도 조건
        // SV가 설정 온도보다 낮은 경우 WaferUnLoad_LowTempSoakTime 만큼 대기 한다.
        private Element<int> _WaferUnLoad_SoakTempLower = new Element<int>() { UpperLimit = 30, LowerLimit = -70, Value = 0};
        [DataMember]
        public Element<int> WaferUnLoad_SoakTempLower
        {
            get { return _WaferUnLoad_SoakTempLower; }
            set
            {
                _WaferUnLoad_SoakTempLower = value;
                RaisePropertyChanged();
            }
        }

        //Wafer UnLoad중 온도안정화를 위해 삼발이 위에서 대기시키는 시간 (SV > WaferUnLoad_SoakTempUpper 인 경우)
        private Element<int> _WaferUnLoad_HighTempSoakTime = new Element<int>() { UpperLimit = 60000, LowerLimit = 0 };
        [DataMember]
        public Element<int> WaferUnLoad_HighTempSoakTime
        {
            get { return _WaferUnLoad_HighTempSoakTime; }
            set
            {
                _WaferUnLoad_HighTempSoakTime = value;
                RaisePropertyChanged();
            }
        }

        //Wafer UnLoad중 온도안정화를 위해 삼발이 위에서 대기시키는 시간 (SV < WaferUnLoad_SoakTempLower 인 경우)
        private Element<int> _WaferUnLoad_LowTempSoakTime = new Element<int>() { UpperLimit = 60000, LowerLimit = 0 };
        [DataMember]
        public Element<int> WaferUnLoad_LowTempSoakTime
        {
            get { return _WaferUnLoad_LowTempSoakTime; }
            set
            {
                _WaferUnLoad_LowTempSoakTime = value;
                RaisePropertyChanged();
            }
        }


        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProberInterfaces.Param;
using System.Xml.Serialization;
using ProberErrorCode;
using LogModule;
using Newtonsoft.Json;

namespace ProberInterfaces.AirBlow
{
    public enum EnumAirBlowCleaningType
    {
        UNDEFINED = -1,
        TIME,
        CLEANINGCOUNT
    }
    public interface IAirBlowChuckCleaningModule : IStateModule
    {
        AirBlowDeviceFile ABDeviceFile { get; set; }
        AirBlowSystemFile ABSysFile { get; set; }
    }
    public interface IAirBlowWaferCleaningModule : IStateModule
    {
        AirBlowDeviceFile ABDeviceFile { get; set; }
        AirBlowSystemFile ABSysFile { get; set; }

    }
    public interface IAirBlowTempControlModule : IStateModule
    {
        AirBlowDeviceFile ABDeviceFile { get; set; }
        AirBlowSystemFile ABSysFile { get; set; }
    }


    [Serializable]
    public class AirBlowDeviceFile : INotifyPropertyChanged, IDeviceParameterizable, IParamNode
    {
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
                LoggerManager.Exception(err);
                
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

        public string FilePath { get; } = "";
        [JsonIgnore]
        public string FileName { get; } = "AirBlowDeviceFile.json";

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        /// <summary>
        /// 에어블로우할때 척의 목표 온도. (30도라면 300으로 Set)
        /// </summary>
        private Element<double> _TargetTemp = new Element<double>();
        public Element<double> TargetTemp
        {
            get { return _TargetTemp; }
            set
            {
                if (value != _TargetTemp)
                {
                    _TargetTemp = value;
                    NotifyPropertyChanged("TargetTemp");
                }
            }
        }
        /// <summary>
        /// 클리닝
        /// </summary>
        private Element<int> _CleaningCount = new Element<int>();
        public Element<int> CleaningCount
        {
            get { return _CleaningCount; }
            set
            {
                if (value != _CleaningCount)
                {
                    _CleaningCount = value;
                    NotifyPropertyChanged("CleaningCount");
                }
            }
        }
        /// <summary>
        /// 클리닝할때 시간을 얼마나 할지 
        /// </summary>
        private Element<double> _CleaningTime = new Element<double>();
        public Element<double> CleaningTime
        {
            get { return _CleaningTime; }
            set
            {
                if (value != _CleaningTime)
                {
                    _CleaningTime = value;
                    NotifyPropertyChanged("CleaningTime");
                }
            }
        }
        /// <summary>
        /// 로딩하기전에 척 클리닝을 한다 
        /// </summary>
        private Element<bool> _ChuckCleaningBeforeLoading = new Element<bool>();
        public Element<bool> ChuckCleaningBeforeLoading
        {
            get { return _ChuckCleaningBeforeLoading; }
            set
            {
                if (value != _ChuckCleaningBeforeLoading)
                {
                    _ChuckCleaningBeforeLoading = value;
                    NotifyPropertyChanged("ChuckCleaningBeforeLoading");
                }
            }
        }
        /// <summary>
        /// 로딩한후 척 클리닝 
        /// </summary>
        private Element<bool> _ChuckCleaningAfterLoading = new Element<bool>();
        public Element<bool> ChuckCleaningAfterLoading
        {
            get { return _ChuckCleaningAfterLoading; }
            set
            {
                if (value != _ChuckCleaningAfterLoading)
                {
                    _ChuckCleaningAfterLoading = value;
                    NotifyPropertyChanged("ChuckCleaningAfterLoading");
                }
            }
        }
        /// <summary>
        /// 언로딩하기전 척 클리닝
        /// </summary>
        private Element<bool> _ChuckCleaningBeforeUnLoading = new Element<bool>();
        public Element<bool> ChuckCleaningBeforeUnLoading
        {
            get { return _ChuckCleaningBeforeUnLoading; }
            set
            {
                if (value != _ChuckCleaningBeforeUnLoading)
                {
                    _ChuckCleaningBeforeUnLoading = value;
                    NotifyPropertyChanged("ChuckCleaningBeforeUnLoading");
                }
            }
        }

        /// <summary>
        /// 언로딩한후 척 클리닝
        /// </summary>
        private Element<bool> _ChuckCleaningAfterUnLoading = new Element<bool>();
        public Element<bool> ChuckCleaningAfterUnLoading
        {
            get { return _ChuckCleaningAfterUnLoading; }
            set
            {
                if (value != _ChuckCleaningAfterUnLoading)
                {
                    _ChuckCleaningAfterUnLoading = value;
                    NotifyPropertyChanged("ChuckCleaningAfterUnLoading");
                }
            }
        }

        private Element<int> _CheckTempInjuryTimeOfSeconds = new Element<int>();
        public Element<int> CheckTempInjuryTimeOfSeconds
        {
            get { return _CheckTempInjuryTimeOfSeconds; }
            set
            {
                if (value != _CheckTempInjuryTimeOfSeconds)
                {
                    _CheckTempInjuryTimeOfSeconds = value;
                    NotifyPropertyChanged("CheckTempInjuryTimeOfSeconds");
                }
            }
        }
        private Element<int> _CheckTempTimeofSeconds = new Element<int>();
        public Element<int> CheckTempTimeofSeconds
        {
            get { return _CheckTempTimeofSeconds; }
            set
            {
                if (value != _CheckTempTimeofSeconds)
                {
                    _CheckTempTimeofSeconds = value;
                    NotifyPropertyChanged("CheckTempTimeofSeconds");
                }
            }
        }

        private Element<double> _TempLimitOffset = new Element<double>();
        public Element<double> TempOffsetLimit
        {
            get { return _TempLimitOffset; }
            set
            {
                if (value != _TempLimitOffset)
                {
                    _TempLimitOffset = value;
                    NotifyPropertyChanged("CheckTempOffset");
                }
            }
        }

        private Element<bool> _AirBlowCleaningEnable = new Element<bool>();
        public Element<bool> AirBlowCleaningEnable
        {
            get { return _AirBlowCleaningEnable; }
            set
            {
                if (value != _AirBlowCleaningEnable)
                {
                    _AirBlowCleaningEnable = value;
                    NotifyPropertyChanged("AirBlowCleaningEnable");
                }
            }
        }

        private Element<bool> _AirBlowTempControlEnable = new Element<bool>();
        public Element<bool> AirBlowTempControlEnable
        {
            get { return _AirBlowTempControlEnable; }
            set
            {
                if (value != _AirBlowTempControlEnable)
                {
                    _AirBlowTempControlEnable = value;
                    NotifyPropertyChanged("AirBlowTempControlEnable");
                }
            }
        }

        public string Genealogy { get; set; }

        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore]
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

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.AirBlowCleaningEnable.Value = false;

                this.TargetTemp.Value = 360;
                this.CleaningCount.Value = 10;
                this.CleaningTime.Value = 60;
                this.ChuckCleaningBeforeLoading.Value = false;
                this.ChuckCleaningBeforeUnLoading.Value = false;
                this.ChuckCleaningAfterLoading.Value = false;
                this.ChuckCleaningAfterUnLoading.Value = false;

                this.CheckTempTimeofSeconds.Value = 60;
                this.CheckTempInjuryTimeOfSeconds.Value = 60;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }


    }
    [Serializable]
    public class AirBlowSystemFile : INotifyPropertyChanged, ISystemParameterizable, IParamNode
    {
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
                LoggerManager.Debug($"[AirBlowSystemFile] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public string FilePath { get; } = "";
        public string FileName { get; } = "AirBlowSystemFile.json";
        public string Genealogy { get; set; }

        public List<object> Nodes { get; set; }
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
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public AirBlowSystemFile()
        {

        }
        /// <summary>
        /// H/W 부착 여부 
        /// </summary>
        private Element<bool> _AirBlowOnProber = new Element<bool>();
        public Element<bool> AirBlowOnProber
        {
            get { return _AirBlowOnProber; }
            set
            {
                if (value != _AirBlowOnProber)
                {
                    _AirBlowOnProber = value;
                    NotifyPropertyChanged("AirBlowOnProber");
                }
            }
        }
        /// <summary>
        /// 에어브로우를 할때 척 위치
        /// </summary>
        /// 
        private MachineCoordinate _StartPos = new MachineCoordinate();
        public MachineCoordinate StartPos
        {
            get { return _StartPos; }
            set
            {
                if (value != _StartPos)
                {
                    _StartPos = value;
                    NotifyPropertyChanged("StartPos");
                }
            }
        }
        /// <summary>
        /// 클리닝할때 위치 거리 
        /// </summary>
        private Element<double> _CleaningDistance = new Element<double>();
        public Element<double> CleaningDistance
        {
            get { return _CleaningDistance; }
            set
            {
                if (value != _CleaningDistance)
                {
                    _CleaningDistance = value;
                    NotifyPropertyChanged("CleaningDistance");
                }
            }
        }

        private Element<double> _AirBlowSpeed = new Element<double>();
        public Element<double> AirBlowSpeed
        {
            get { return _AirBlowSpeed; }
            set
            {
                if (value != _AirBlowSpeed)
                {
                    _AirBlowSpeed = value;
                    NotifyPropertyChanged("AirBlowSpeed");
                }
            }
        }

        private Element<double> _AirBlowAcc = new Element<double>();
        public Element<double> AirBlowAcc
        {
            get { return _AirBlowAcc; }
            set
            {
                if (value != _AirBlowAcc)
                {
                    _AirBlowAcc = value;
                    NotifyPropertyChanged("AirBlowAcc");
                }
            }
        }

        private Element<EnumAirBlowCleaningType> _AirBlowCleaningType = new Element<EnumAirBlowCleaningType>();
        public Element<EnumAirBlowCleaningType> AirBlowCleaningType
        {
            get { return _AirBlowCleaningType; }
            set
            {
                if (value != _AirBlowCleaningType)
                {
                    _AirBlowCleaningType = value;
                    NotifyPropertyChanged("AirBlowCleaningType");
                }
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return retVal;
        }
    }
}

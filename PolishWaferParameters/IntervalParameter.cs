using System;
using System.Collections.Generic;

namespace PolishWaferParameters
{
    using Focusing;
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PolishWafer;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class PolishWaferIntervalParameter : IParam, INotifyPropertyChanged, IDeviceParameterizable, IPolishWaferIntervalParameter
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..IParamProperty
        [JsonIgnore]
        public string FilePath { get; } = "Cleaning";
        [JsonIgnore]
        public string FileName { get; } = "CleaningParam.Json";
        [JsonIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore]
        public List<object> Nodes { get; set; }
        [JsonIgnore]
        public string Genealogy { get; set; }
        [JsonIgnore]
        public object Owner { get; set; }
        #endregion

        public PolishWaferIntervalParameter()
        {
            try
            {
                this.HashCode = SecuritySystem.SecurityUtil.GetHashCode_SHA256(DateTime.Now.Ticks + this.GetHashCode().ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region //..IParam Function
        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetDefaultParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PolishWaferCleaningParameter cleaningparam1 = new PolishWaferCleaningParameter();

                cleaningparam1.CleaningDirection.Value = CleaningDirection.Down;
                cleaningparam1.CleaningScrubMode.Value = PWScrubMode.One_Direction;

                cleaningparam1.ContactCount.Value = 10;
                cleaningparam1.ContactLength.Value = 10;

                cleaningparam1.EdgeDetectionBeforeCleaning.Value = true;
                cleaningparam1.PinAlignAfterCleaning.Value = true;
                cleaningparam1.PinAlignBeforeCleaning.Value = true;

                //cleaningparam1.PinAlignAfterCleaningProcessed = false;
                //cleaningparam1.PinAlignBeforeCleaningProcessed = false;
                //cleaningparam1.PolishWaferCleaningProcessed = false;
                //cleaningparam1.PolishWaferCleaningRetry = false;

                cleaningparam1.ScrubingLength.Value = 10;
                cleaningparam1.WaferDefineType.Value = "TonyTest1";
                cleaningparam1.OverdriveValue.Value = -1000;
                cleaningparam1.Clearance.Value = -10000;

                if (cleaningparam1.CenteringLightParams == null)
                {
                    cleaningparam1.CenteringLightParams = new ObservableCollection<LightValueParam>();

                    LightValueParam tmp = new LightValueParam();

                    tmp.Type.Value = EnumLightType.COAXIAL;
                    tmp.Value.Value = 255;

                    cleaningparam1.CenteringLightParams.Add(tmp);
                }

                if (cleaningparam1.FocusParam == null)
                {
                    cleaningparam1.FocusParam = new NormalFocusParameter();
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, cleaningparam1.FocusParam, 300);
                }

                if (cleaningparam1.FocusingModuleDllInfo == null)
                {
                    cleaningparam1.FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if (cleaningparam1.FocusingPointMode.Value == PWFocusingPointMode.UNDEFINED)
                {
                    cleaningparam1.FocusingPointMode.Value = PWFocusingPointMode.POINT1;
                }

                if (cleaningparam1.FocusParam.LightParams == null)
                {
                    cleaningparam1.FocusParam.LightParams = new ObservableCollection<LightValueParam>();

                    LightValueParam tmp = new LightValueParam();

                    tmp.Type.Value = EnumLightType.COAXIAL;
                    tmp.Value.Value = 70;

                    cleaningparam1.FocusParam.LightParams.Add(tmp);
                }

                CleaningParameters.Add(cleaningparam1);

                CleaningTriggerMode.Value = EnumCleaningTriggerMode.LOT_START;
                IntervalCount.Value = 2;
                CleaningType.Value = EnumCleaningType.ABRAISIVE;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public void SetElementMetaData()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        #endregion

        #region //..Property

        private string _HashCode = string.Empty;
        public string HashCode
        {
            get { return _HashCode; }
            set
            {
                if (value != _HashCode)
                {
                    _HashCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IPolishWaferCleaningParameter> _CleaningParameters = new ObservableCollection<IPolishWaferCleaningParameter>();
        public ObservableCollection<IPolishWaferCleaningParameter> CleaningParameters
        {
            get { return _CleaningParameters; }
            set
            {
                if (value != _CleaningParameters)
                {
                    _CleaningParameters = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumCleaningTriggerMode> _CleaningTriggerMode = new Element<EnumCleaningTriggerMode>();
        public Element<EnumCleaningTriggerMode> CleaningTriggerMode
        {
            get { return _CleaningTriggerMode; }
            set
            {
                if (value != _CleaningTriggerMode)
                {
                    _CleaningTriggerMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _IntervalCount = new Element<int>();
        public Element<int> IntervalCount
        {
            get { return _IntervalCount; }
            set
            {
                if (value != _IntervalCount)
                {
                    _IntervalCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _TouchdownCount = new Element<int>();
        public Element<int> TouchdownCount
        {
            get { return _TouchdownCount; }
            set
            {
                if (value != _TouchdownCount)
                {
                    _TouchdownCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumCleaningType> _CleaningType = new Element<EnumCleaningType>();
        /// <summary>
        /// ABRAISIVE,GEL,COMBO => Cleaning 종류.
        /// </summary>
        public Element<EnumCleaningType> CleaningType
        {
            get { return _CleaningType; }
            set
            {
                if (value != _CleaningType)
                {
                    _CleaningType = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
    }
}

namespace ProberInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;

    public class ParamDevParameter : INotifyPropertyChanged, IDeviceParameterizable
    {
        #region <remarks> PropertyChanged </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> IDeviceParameterizable Property </remarks>
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "";

        public string FileName { get; } = "ParamDevParameter.json";

        [JsonIgnore]
        public string Genealogy { get; set; }

        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
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

        [JsonIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        #region <remarks> Property </remarks>

        private bool _VerifyParameterBeforeStartLotEnable
             = false;
        public bool VerifyParameterBeforeStartLotEnable
        {
            get { return _VerifyParameterBeforeStartLotEnable; }
            set
            {
                if (value != _VerifyParameterBeforeStartLotEnable)
                {
                    _VerifyParameterBeforeStartLotEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<int> _VerifyLotDevVIDs
             = new ObservableCollection<int>();
        public ObservableCollection<int> VerifyLotDevVIDs
        {
            get { return _VerifyLotDevVIDs; }
            set
            {
                if (value != _VerifyLotDevVIDs)
                {
                    _VerifyLotDevVIDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<VerifyParamInfo> _VerifyParamInfos
             = new List<VerifyParamInfo>();
        public List<VerifyParamInfo> VerifyParamInfos
        {
            get { return _VerifyParamInfos; }
            set
            {
                if (value != _VerifyParamInfos)
                {
                    _VerifyParamInfos = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region <remarks> IDeviceParameterizable Method </remarks>
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
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                VerifyParamInfos.Add(new VerifyParamInfo());
                VerifyParamInfos[0].PropertyName = "Test OverDrive";
                VerifyParamInfos[0].Description = "";
                VerifyParamInfos[0].EnumProperty = VerifyPropertyEnum.OVER_DRIVE;
                VerifyParamInfos.Add(new VerifyParamInfo());
                VerifyParamInfos[1].PropertyName = "Temperature";
                VerifyParamInfos[1].Description = "";
                VerifyParamInfos[1].EnumProperty = VerifyPropertyEnum.TEMPERATURE;
                VerifyParamInfos.Add(new VerifyParamInfo());
                VerifyParamInfos[2].PropertyName = "Clean InterVal";
                VerifyParamInfos[2].Description = "";
                VerifyParamInfos[2].EnumProperty = VerifyPropertyEnum.CLEAN_INTERNAL;
                VerifyParamInfos.Add(new VerifyParamInfo());
                VerifyParamInfos[3].PropertyName = "Clean OverDrive";
                VerifyParamInfos[3].Description = "";
                VerifyParamInfos[3].EnumProperty = VerifyPropertyEnum.CLEAN_OVER_DRIVE;
                VerifyParamInfos.Add(new VerifyParamInfo());
                VerifyParamInfos[4].PropertyName = "Clean Thickness";
                VerifyParamInfos[4].Description = "";
                VerifyParamInfos[4].EnumProperty = VerifyPropertyEnum.CLEAN_THICKNESS;
                VerifyParamInfos.Add(new VerifyParamInfo());
                VerifyParamInfos[5].PropertyName = "Clean Time";
                VerifyParamInfos[5].Description = "";
                VerifyParamInfos[5].EnumProperty = VerifyPropertyEnum.CLEAN_TOUCH_COUNT;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetElementMetaData()
        {

        }
        #endregion
    }


    public class VerifyParamInfo : INotifyPropertyChanged
    {
        #region // ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private string _PropertyName;
        public string PropertyName
        {
            get { return _PropertyName; }
            set
            {
                if (value != _PropertyName)
                {
                    _PropertyName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MinValue;
        public double MinValue
        {
            get { return _MinValue; }
            set
            {
                if (value != _MinValue)
                {
                    _MinValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MaxValve;
        public double MaxValve
        {
            get { return _MaxValve; }
            set
            {
                if (value != _MaxValve)
                {
                    _MaxValve = value;
                    RaisePropertyChanged();
                }
            }
        }

        private VerifyPropertyEnum _EnumProperty;
        public VerifyPropertyEnum EnumProperty
        {
            get { return _EnumProperty; }
            set
            {
                if (value != _EnumProperty)
                {
                    _EnumProperty = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public enum VerifyPropertyEnum
    {
        OVER_DRIVE,
        TEMPERATURE,
        CLEAN_OVER_DRIVE,
        CLEAN_THICKNESS,
        CLEAN_INTERNAL,
        CLEAN_TOUCH_COUNT
    }
}

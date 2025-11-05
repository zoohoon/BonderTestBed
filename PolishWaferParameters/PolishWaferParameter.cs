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

    public class PolishWaferParameter : IParam, INotifyPropertyChanged, IParamNode, IDeviceParameterizable, IPolishWaferParameter
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

        #region //..ParamProperty
        [JsonIgnore]
        public string FilePath { get; } = "PolishWaferParam";
        [JsonIgnore]
        public string FileName { get; } = "PolishWaferParam.Json";
        [JsonIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore]
        public List<object> Nodes { get; set; }
        [JsonIgnore]
        public string Genealogy { get; set; }
        [JsonIgnore]
        public object Owner { get; set; }
        #endregion

        #region //..IParam Function
        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (PolishWaferIntervalParameters == null)
                {
                    PolishWaferIntervalParameters = new ObservableCollection<PolishWaferIntervalParameter>();
                }

                //if (SourceParameters == null)
                //{
                //    SourceParameters = new AsyncObservableCollection<PolishWaferInformation>();
                //}

                foreach (var interval in PolishWaferIntervalParameters)
                {
                    if(string.IsNullOrEmpty(interval.HashCode) == true)
                    {
                        interval.HashCode = SecuritySystem.SecurityUtil.GetHashCode_SHA256(DateTime.Now.Ticks + interval.GetHashCode().ToString());
                    }

                    foreach (var cleaning in interval.CleaningParameters)
                    {
                        if (string.IsNullOrEmpty(cleaning.HashCode) == true)
                        {
                            cleaning.HashCode = SecuritySystem.SecurityUtil.GetHashCode_SHA256(DateTime.Now.Ticks + cleaning.GetHashCode().ToString());
                        }

                        if (cleaning.CenteringLightParams == null)
                        {
                            cleaning.CenteringLightParams = new ObservableCollection<LightValueParam>();

                            LightValueParam tmp = new LightValueParam();

                            tmp.Type.Value = EnumLightType.COAXIAL;
                            tmp.Value.Value = 255;

                            cleaning.CenteringLightParams.Add(tmp);
                        }

                        if (cleaning.FocusParam == null)
                        {
                            cleaning.FocusParam = new NormalFocusParameter();
                            this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, cleaning.FocusParam, 300);
                        }

                        if (cleaning.FocusingModuleDllInfo == null)
                        {
                            cleaning.FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                        }

                        if (cleaning.FocusingPointMode.Value == PWFocusingPointMode.UNDEFINED)
                        {
                            cleaning.FocusingPointMode.Value = PWFocusingPointMode.POINT1;
                        }

                        if (cleaning.FocusParam.LightParams == null)
                        {
                            cleaning.FocusParam.LightParams = new ObservableCollection<LightValueParam>();

                            LightValueParam tmp = new LightValueParam();

                            tmp.Type.Value = EnumLightType.COAXIAL;
                            tmp.Value.Value = 70;

                            cleaning.FocusParam.LightParams.Add(tmp);
                        }

                        if(cleaning.Clearance.Value > 0)
                        {
                            cleaning.Clearance.Value = Math.Abs(cleaning.Clearance.Value) * -1;
                        }
                    }

                    SetCleaningParamRange();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        //private EventCodeEnum ValidationSourceParam()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        if( (SourceParameters != null) && (SourceParameters.Count == 0) )
        //        {
        //            retval = EventCodeEnum.NONE;
        //        }

        //        foreach (var source in SourceParameters)
        //        {
        //            if (source.FocusParam == null)
        //            {
        //                source.FocusParam = new NormalFocusParameter();
        //            }

        //            retval = this.FocusManager().ValidationFocusParam(source.FocusParam);

        //            if (retval != EventCodeEnum.NONE)
        //            {
        //                this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, source.FocusParam);

        //                retval = EventCodeEnum.NONE;
        //            }

        //            if(source.FocusingModuleDllInfo == null)
        //            {
        //                source.FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}


        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                #region //.. Emul Data
                PolishWaferIntervalParameters = new ObservableCollection<PolishWaferIntervalParameter>();
                PolishWaferIntervalParameters.Add(new PolishWaferIntervalParameter());

                PolishWaferIntervalParameters[0].CleaningParameters = new ObservableCollection<IPolishWaferCleaningParameter>();
                PolishWaferIntervalParameters[0].CleaningParameters.Add(new PolishWaferCleaningParameter());
                PolishWaferIntervalParameters[0].CleaningParameters[0].WaferDefineType.Value = "TestSource1";
                PolishWaferIntervalParameters[0].CleaningParameters[0].ContactCount.Value = 5;
                PolishWaferIntervalParameters[0].CleaningTriggerMode.Value = EnumCleaningTriggerMode.WAFER_INTERVAL;
                PolishWaferIntervalParameters[0].IntervalCount.Value = 3;
                PolishWaferIntervalParameters[0].CleaningType.Value = EnumCleaningType.ABRAISIVE;

                //PolishWaferIntervalParameters[1].CleaningParameters = new ObservableCollection<IPolishWaferCleaningParameter>();
                //PolishWaferIntervalParameters[1].CleaningParameters.Add(new PolishWaferCleaningParameter());
                //PolishWaferIntervalParameters[1].CleaningParameters[0].WaferDefineType.Value = "GEL_CLEANING_WAFER";
                //PolishWaferIntervalParameters[1].CleaningParameters[0].CleaningScrubMode.Value = PWScrubMode.One_Direction;
                //PolishWaferIntervalParameters[1].CleaningIntervalMode.Value = EnumCleaningIntervalMode.WAFER_INTERVAL;
                //PolishWaferIntervalParameters[1].IntervalCount.Value = 5;
                //PolishWaferIntervalParameters[1].CleaningType.Value = EnumCleaningType.GEL;


                //PolishWaferIntervalParameters.Add(new PolishWaferPolishWaferIntervalParameter());
                //PolishWaferIntervalParameters[2].CleaningParameters = new ObservableCollection<IPolishWaferCleaningParameter>();
                //PolishWaferIntervalParameters[2].CleaningParameters.Add(new PolishWaferCleaningParameter());
                //PolishWaferIntervalParameters[2].CleaningParameters[0].WaferDefineType.Value = "ABRAISIVE_CLEANING_WAFER";
                //PolishWaferIntervalParameters[2].CleaningParameters[0].CleaningScrubMode.Value = PWScrubMode.One_Direction;
                //PolishWaferIntervalParameters[2].CleaningParameters[0].ContactCount.Value = 3;

                //PolishWaferIntervalParameters[2].CleaningParameters.Add(new PolishWaferCleaningParameter());
                //PolishWaferIntervalParameters[2].CleaningParameters[1].WaferDefineType.Value = "GEL_CLEANING_WAFER";
                //PolishWaferIntervalParameters[2].CleaningParameters[1].CleaningScrubMode.Value = PWScrubMode.Square;
                //PolishWaferIntervalParameters[2].CleaningParameters[1].ContactCount.Value = 3;

                //PolishWaferIntervalParameters[2].CleaningIntervalMode.Value = EnumCleaningIntervalMode.WAFER_INTERVAL;
                //PolishWaferIntervalParameters[2].IntervalCount.Value = 18;
                //PolishWaferIntervalParameters[2].CleaningType.Value = EnumCleaningType.COMBO;

                //PolishWaferIntervalParameters.Add(new PolishWaferPolishWaferIntervalParameter());
                //PolishWaferIntervalParameters[3].CleaningParameters = new ObservableCollection<IPolishWaferCleaningParameter>();
                //PolishWaferIntervalParameters[3].CleaningParameters.Add(new PolishWaferCleaningParameter());
                //PolishWaferIntervalParameters[3].CleaningParameters[0].WaferDefineType.Value = "ABRAISIVE_CLEANING_WAFER";
                //PolishWaferIntervalParameters[3].CleaningParameters[0].CleaningScrubMode.Value = PWScrubMode.One_Direction;
                //PolishWaferIntervalParameters[3].CleaningParameters[0].ContactCount.Value = 3;
                //PolishWaferIntervalParameters[3].CleaningIntervalMode.Value = EnumCleaningIntervalMode.WAFER_INTERVAL;
                //PolishWaferIntervalParameters[3].IntervalCount.Value = 3;
                //PolishWaferIntervalParameters[3].CleaningType.Value = EnumCleaningType.ABRAISIVE;

                //SourceParameters = new AsyncObservableCollection<PolishWaferInformation>();

                //SourceParameters.Add(new PolishWaferInformation());
                //SourceParameters[0].DefineName.Value = "TestSource1";
                //SourceParameters[0].Thickness.Value = 300;
                //SourceParameters[0].FocusingPointMode.Value = PWFocusingPointMode.POINT5;
                //SourceParameters[0].FocusingType.Value = PWFocusingType.CAMERA;
                //SourceParameters[0].OverdriveValue.Value = 30;
                //SourceParameters[0].ZClearance.Value = 10;

                //SourceParameters.Add(new PolishWaferInformation());
                //SourceParameters[1].DefineName.Value = "TestSource2";
                //SourceParameters[1].Thickness.Value = 300;
                //SourceParameters[1].FocusingPointMode.Value = PWFocusingPointMode.POINT5;
                //SourceParameters[1].FocusingType.Value = PWFocusingType.CAMERA;
                //SourceParameters[1].OverdriveValue.Value = 50;
                //SourceParameters[1].ZClearance.Value = 20;

                //SourceParameters.Add(new PolishWaferInformation());
                //SourceParameters[2].DefineName.Value = "TestSource3";
                //SourceParameters[2].Thickness.Value = 700;
                //SourceParameters[2].FocusingPointMode.Value = PWFocusingPointMode.POINT5;
                //SourceParameters[2].FocusingType.Value = PWFocusingType.CAMERA;
                //SourceParameters[2].OverdriveValue.Value = 70;
                //SourceParameters[2].ZClearance.Value = 30;
                #endregion

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
                //SourceParameters = new AsyncObservableCollection<PolishWaferInformation>();

                //PolishWaferInformation polishinfo1 = new PolishWaferInformation();
                //polishinfo1.DefineName.Value = "TonyTest1";
                //polishinfo1.MaxLimitCount.Value = 2;
                //polishinfo1.TouchCount.Value = 3;
                //polishinfo1.Margin.Value = 270;
                //polishinfo1.Thickness.Value = 750;
                //polishinfo1.DeadZone = 500;

                //SourceParameters.Add(polishinfo1);

                PolishWaferIntervalParameters = new ObservableCollection<PolishWaferIntervalParameter>();
                PolishWaferIntervalParameter polishinterval1 = new PolishWaferIntervalParameter();

                polishinterval1.SetDefaultParam();

                PolishWaferIntervalParameters.Add(polishinterval1);

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
                //this.PinAlignBeforeCleaning.ElementName = "Pin align before cleaning";
                //this.PinAlignBeforeCleaning.ElementAdmin = "Alvin";
                //this.PinAlignBeforeCleaning.CategoryID = "41000";
                //this.PinAlignBeforeCleaning.Description = "Unknown";

                //this.PinAlignAfterCleaning.ElementName = "Pin align after cleaning";
                //this.PinAlignAfterCleaning.ElementAdmin = "Alvin";
                //this.PinAlignAfterCleaning.CategoryID = "41000";
                //this.PinAlignAfterCleaning.Description = "Unknown";

                //this.EdgeDetectionBeforeCleaning.ElementName = "Edge detection before cleaning";
                //this.EdgeDetectionBeforeCleaning.ElementAdmin = "Alvin";
                //this.EdgeDetectionBeforeCleaning.CategoryID = "41000";
                //this.EdgeDetectionBeforeCleaning.Description = "Unknown";

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        #endregion

        public void SetCleaningParamRange()
        {
            try
            {
                foreach (var interval in PolishWaferIntervalParameters)
                {
                    foreach (var cleaning in interval.CleaningParameters)
                    {

                        cleaning.ContactCount.UpperLimit = 999;
                        cleaning.ContactCount.LowerLimit = 0;

                        cleaning.ContactLength.UpperLimit = 99999;
                        cleaning.ContactLength.LowerLimit = 0;

                        cleaning.ScrubingLength.UpperLimit = 999;
                        cleaning.ScrubingLength.LowerLimit = 0;

                        cleaning.OverdriveValue.UpperLimit = 999;
                        cleaning.OverdriveValue.LowerLimit = -999;

                        cleaning.Clearance.UpperLimit = 0;
                        cleaning.Clearance.LowerLimit = -9999;

                        cleaning.FocusingHeightTolerance.UpperLimit = 999;
                        cleaning.FocusingHeightTolerance.LowerLimit = 0;

                        cleaning.FocusParam.FocusRange.UpperLimit = 999;
                        cleaning.FocusParam.FocusRange.LowerLimit = 1;

                        cleaning.FocusParam.FocusMaxStep.UpperLimit = 999;
                        cleaning.FocusParam.FocusMaxStep.LowerLimit = 1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region //..Property

        //[NonSerialized]
        //private bool _NeedeLoadWaferFlag;
        //[JsonIgnore]
        //public bool NeedLoadWaferFlag
        //{
        //    get { return _NeedeLoadWaferFlag; }
        //    set
        //    {
        //        if (value != _NeedeLoadWaferFlag)
        //        {
        //            _NeedeLoadWaferFlag = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //[NonSerialized]
        //private string _LoadWaferType;
        //[JsonIgnore]
        //public string LoadWaferType
        //{
        //    get { return _LoadWaferType; }
        //    set
        //    {
        //        if (value != _LoadWaferType)
        //        {
        //            _LoadWaferType = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private ObservableCollection<PolishWaferInformation> _SourceParameters;
        //public ObservableCollection<PolishWaferInformation> SourceParameters
        //{
        //    get { return _SourceParameters; }
        //    set
        //    {
        //        if (value != _SourceParameters)
        //        {
        //            _SourceParameters = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //private AsyncObservableCollection<PolishWaferInformation> _SourceParameters;
        //public AsyncObservableCollection<PolishWaferInformation> SourceParameters
        //{
        //    get { return _SourceParameters; }
        //    set
        //    {
        //        if (value != _SourceParameters)
        //        {
        //            _SourceParameters = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<PolishWaferIntervalParameter> _PolishWaferIntervalParameters
           = new ObservableCollection<PolishWaferIntervalParameter>();
        public ObservableCollection<PolishWaferIntervalParameter> PolishWaferIntervalParameters
        {
            get { return _PolishWaferIntervalParameters; }
            set
            {
                if (value != _PolishWaferIntervalParameters)
                {
                    _PolishWaferIntervalParameters = value;
                    RaisePropertyChanged();
                }
            }
        }

        //[NonSerialized]
        //private IPolishWaferIntervalParameter _CurIntervalParameter;
        //[JsonIgnore]
        //public IPolishWaferIntervalParameter CurIntervalParameter
        //{
        //    get { return _CurIntervalParameter; }
        //    set
        //    {
        //        if (value != _CurIntervalParameter)
        //        {
        //            _CurIntervalParameter = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        #endregion
    }
}

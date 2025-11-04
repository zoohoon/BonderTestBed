namespace ProberInterfaces.Param
{
    using LogModule;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces.Enum;

    public enum PeakSelectionStrategy
    {
        NONE,
        HIGHEST,
        LOWEST,
    }

    public interface IFocusParameter : INotifyPropertyChanged
    {
        Element<int> FocusMaxStep { get; set; }
        Element<double> FocusRange { get; set; }
        Element<double> DepthOfField { get; set; }
        Element<double> FocusThreshold { get; set; }
        Element<double> FlatnessThreshold { get; set; }
        Element<int> PotentialThreshold { get; set; }
        Element<double> PeakRangeThreshold { get; set; }
        Element<bool> CheckThresholdFocusValue { get; set; }
        Element<bool> CheckPotential { get; set; }
        Element<bool> CheckFlatness { get; set; }
        Element<bool> CheckDualPeak { get; set; }
        Element<EnumProberCam> FocusingCam { get; set; }
        Element<Rect> FocusingROI { get; set; }
        Element<EnumAxisConstants> FocusingAxis { get; set; }
        Element<EnumFocusingType> FocusingType { get; set; }
        Element<double> OutFocusLimit { get; set; }
        //Element<int> ChuckRefHight { get; set; }

        long FocusTime { get; set; }
        double FocusValue { get; set; }
        double FocusResultPos { get; set; }
        EventCodeEnum SetDefaultParam();
        EventCodeEnum SetEmulParam();
    }

    [Serializable]
    public abstract class FocusParameter : IFocusParameter, IParamNode
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

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual Object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        

        private Element<int> _FocusMaxStep = new Element<int>() { UpperLimit = 999, LowerLimit = 1};
        public Element<int> FocusMaxStep
        {
            get { return _FocusMaxStep; }
            set
            {
                if (value != _FocusMaxStep)
                {
                    _FocusMaxStep = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private Element<int> _ChuckRefHight = new Element<int>();
        //public Element<int> ChuckRefHight
        //{
        //    get { return _ChuckRefHight; }
        //    set
        //    {
        //        if (value != _ChuckRefHight)
        //        {
        //            _ChuckRefHight = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<double> _FocusRange = new Element<double>() { UpperLimit = 999, LowerLimit = 1};
        public Element<double> FocusRange
        {
            get { return _FocusRange; }
            set
            {
                if (value != _FocusRange)
                {
                    _FocusRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _DepthOfField = new Element<double>();
        public Element<double> DepthOfField
        {
            get { return _DepthOfField; }
            set
            {
                if (value != _DepthOfField)
                {
                    _DepthOfField = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _OutFocusLimit = new Element<double>();
        public Element<double> OutFocusLimit
        {
            get { return _OutFocusLimit; }
            set
            {
                if (value != _OutFocusLimit)
                {
                    _OutFocusLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _FocusThreshold = new Element<double>();
        public Element<double> FocusThreshold
        {
            get { return _FocusThreshold; }
            set
            {
                if (value != _FocusThreshold)
                {
                    _FocusThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _FlatnessThreshold = new Element<double>();
        public Element<double> FlatnessThreshold
        {
            get { return _FlatnessThreshold; }
            set
            {
                if (value != _FlatnessThreshold)
                {
                    _FlatnessThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PeakRangeThreshold = new Element<double>();
        public Element<double> PeakRangeThreshold
        {
            get { return _PeakRangeThreshold; }
            set
            {
                if (value != _PeakRangeThreshold)
                {
                    _PeakRangeThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PotentialThreshold = new Element<int>();
        public Element<int> PotentialThreshold
        {
            get { return _PotentialThreshold; }
            set
            {
                if (value != _PotentialThreshold)
                {
                    _PotentialThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _CheckPotential = new Element<bool>();
        public Element<bool> CheckPotential
        {
            get { return _CheckPotential; }
            set
            {
                if (value != _CheckPotential)
                {
                    _CheckPotential = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _CheckThresholdFocusValue = new Element<bool>();
        public Element<bool> CheckThresholdFocusValue
        {
            get { return _CheckThresholdFocusValue; }
            set
            {
                if (value != _CheckThresholdFocusValue)
                {
                    _CheckThresholdFocusValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _CheckFlatness = new Element<bool>();
        public Element<bool> CheckFlatness
        {
            get { return _CheckFlatness; }
            set
            {
                if (value != _CheckFlatness)
                {
                    _CheckFlatness = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _CheckDualPeak = new Element<bool>();
        public Element<bool> CheckDualPeak
        {
            get { return _CheckDualPeak; }
            set
            {
                if (value != _CheckDualPeak)
                {
                    _CheckDualPeak = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumProberCam> _FocusingCam = new Element<EnumProberCam>();
        public Element<EnumProberCam> FocusingCam
        {
            get { return _FocusingCam; }
            set
            {
                if (value != _FocusingCam)
                {
                    _FocusingCam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Rect> _FocusingROI = new Element<Rect>();
        public Element<Rect> FocusingROI
        {
            get { return _FocusingROI; }
            set
            {
                if (value != _FocusingROI)
                {
                    _FocusingROI = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<EnumAxisConstants> _FocusingAxis = new Element<EnumAxisConstants>();
        public Element<EnumAxisConstants> FocusingAxis
        {
            get { return _FocusingAxis; }
            set
            {
                if (value != _FocusingAxis)
                {
                    _FocusingAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<EnumFocusingType> _FocusingType = new Element<EnumFocusingType>();
        public Element<EnumFocusingType> FocusingType
        {
            get { return _FocusingType; }
            set
            {
                if (value != _FocusingType)
                {
                    _FocusingType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LightValueParam> _LightParams;
        [SharePropPath]
        public ObservableCollection<LightValueParam> LightParams
        {
            get { return _LightParams; }
            set
            {
                if (value != _LightParams)
                {
                    _LightParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _FocusValue;
        [ParamIgnore, JsonIgnore]
        public double FocusValue
        {
            get { return _FocusValue; }
            set
            {
                if (value != _FocusValue)
                {
                    _FocusValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _FocusResultPos;
        [ParamIgnore, JsonIgnore]
        public double FocusResultPos
        {
            get { return _FocusResultPos; }
            set
            {
                if (value != _FocusResultPos)
                {
                    _FocusResultPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private long _FocusTime;
        [ParamIgnore, JsonIgnore]
        public long FocusTime
        {
            get { return _FocusTime; }
            set
            {
                if (value != _FocusTime)
                {
                    _FocusTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
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
                _FocusMaxStep.Value = 20;
                _FocusRange.Value = 200;
                _DepthOfField.Value = 1;
                _FocusThreshold.Value = 10000;
                _FlatnessThreshold.Value = 20;
                _PeakRangeThreshold.Value = 40;
                _PotentialThreshold.Value = 20;
                _CheckPotential.Value = true;
                _CheckThresholdFocusValue.Value = true;
                _CheckFlatness.Value = true;
                _CheckDualPeak.Value = true;
                _FocusingROI.Value = new Rect(0, 0, 960, 960);
                _FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;
                _FocusingAxis.Value = EnumAxisConstants.Z;
                _OutFocusLimit.Value = 40;
                //_ChuckRefHight.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public FocusParameter()
        {
            try
            {
                //_FocusMaxStep = 15;
                //_FocusRange = 200;
                //_DepthOfField = 1;
                //_FocusThreshold = 10000;
                //_FlatnessThreshold = 90;
                //_PeakRangeThreshold = 30;
                //_PotentialThreshold = 20;
                //_CheckPotential = true;
                //_CheckThresholdFocusValue = true;
                //_CheckFlatness = true;
                //_CheckDualPeak = true;
                //_FocusingROI = new Rect(0, 0, 480, 480);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public FocusParameter(EnumProberCam camtype)
        {
            try
            {
                //_FocusingCam = camtype;
                //_FocusMaxStep = 15;
                //_FocusRange = 200;
                //_DepthOfField = 1;
                //_FocusThreshold = 10000;
                //_FlatnessThreshold = 90;
                //_PeakRangeThreshold = 30;
                //_PotentialThreshold = 20;
                //_CheckPotential = true;
                //_CheckThresholdFocusValue = true;
                //_CheckFlatness = true;
                //_CheckDualPeak = true;
                //_FocusingROI = new Rect(0, 0, 480, 480);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public FocusParameter(FocusParameter param)
        {
            try
            {
                //_FocusMaxStep = param.FocusMaxStep;
                //_FocusRange = param.FocusRange;
                //_DepthOfField = param.DepthOfField;
                //_FocusThreshold = param.FocusThreshold;
                //_FlatnessThreshold = param.FlatnessThreshold;
                //_PeakRangeThreshold = param.PeakRangeThreshold;
                //_PotentialThreshold = param.PotentialThreshold;
                //_CheckPotential = param.CheckPotential;
                //_CheckThresholdFocusValue = param.CheckThresholdFocusValue;
                //_CheckFlatness = param.CheckFlatness;
                //_CheckDualPeak = param.CheckDualPeak;
                //_FocusingCam = param.FocusingCam;
                //_FocusingROI = param.FocusingROI;
                //_FocusingAxis = param._FocusingAxis;
                //_FocusingModel = param.FocusingModel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void CopyTo(FocusParameter param)
        {
            param.FocusRange.Value = this.FocusRange.Value;
            param.FocusMaxStep.Value = this.FocusMaxStep.Value;
            param.DepthOfField.Value = this.DepthOfField.Value;
            param.FocusThreshold.Value = this.FocusThreshold.Value;
            param.FlatnessThreshold.Value = this.FlatnessThreshold.Value;
            param.PeakRangeThreshold.Value = this.PeakRangeThreshold.Value;
            param.PotentialThreshold.Value = this.PotentialThreshold.Value;
            param.OutFocusLimit.Value = this.OutFocusLimit.Value;
            param.CheckPotential.Value = this.CheckPotential.Value;
            param.CheckThresholdFocusValue.Value = this.CheckThresholdFocusValue.Value;
            param.CheckFlatness.Value = this.CheckFlatness.Value;
            param.CheckDualPeak.Value = this.CheckDualPeak.Value;
            param.FocusingCam.Value = this.FocusingCam.Value;
            param.FocusingAxis.Value = this.FocusingAxis.Value;
            param.FocusingROI.Value = this.FocusingROI.Value;
            param.FocusingType.Value = this.FocusingType.Value;

            param.LightParams = new ObservableCollection<LightValueParam>();

            if(this.LightParams != null)
            {
                if (param.LightParams == null)
                {
                    param.LightParams = new ObservableCollection<LightValueParam>();
                }

                foreach (var lightparam in this.LightParams)
                {
                    param.LightParams.Add(lightparam);
                }
            }
        }
    }

    //[Serializable]
    //public class MarkFocusParameter : FocusParameter
    //{
    //    public MarkFocusParameter() : base()
    //    {
    //        FocusingCam = EnumProberCam.WAFER_HIGH_CAM;
    //    }

    //    public MarkFocusParameter(FocusParameter param) : base(param)
    //    {

    //    }
    //}
    //[Serializable]
    //public class PinLowFocusParameter : FocusParameter
    //{
    //    public PinLowFocusParameter() : base()
    //    {
    //        FocusingCam = EnumProberCam.PIN_LOW_CAM;
    //    }

    //    public PinLowFocusParameter(FocusParameter param) : base(param)
    //    {
    //    }
    //}

    //[Serializable]
    //public class PinHighFocusParameter : FocusParameter
    //{
    //    public PinHighFocusParameter() : base()
    //    {
    //        FocusingCam = EnumProberCam.PIN_HIGH_CAM;
    //    }

    //    public PinHighFocusParameter(FocusParameter param) : base(param)
    //    {
    //    }
    //}

    //[Serializable]
    //public class WaferFocusParameter : FocusParameter
    //{
    //    public WaferFocusParameter() : base()
    //    {
    //        FocusingCam = EnumProberCam.WAFER_HIGH_CAM;
    //    }
    //    public WaferFocusParameter(EnumProberCam camtype) : base(camtype)
    //    {
    //        FocusingCam = camtype;

    //    }
    //    public WaferFocusParameter(EnumProberCam camtype, EnumAxisConstants axis) : base(camtype)
    //    {
    //        FocusingCam = camtype;
    //        FocusingAxis = axis;
    //    }
    //    public WaferFocusParameter(FocusParameter param) : base(param)
    //    {
    //    }
    //}
    //[Serializable]
    //public class NeedleFocusParameter : FocusParameter
    //{
    //    public NeedleFocusParameter() : base()
    //    {
    //        FocusingCam = EnumProberCam.WAFER_HIGH_CAM;
    //    }
    //    public NeedleFocusParameter(FocusParameter param) : base(param)
    //    {
    //    }
    //}
    //public enum EnumIOType
    //{
    //    UNDEFINED,
    //    INPUT,
    //    OUTPUT,
    //    BIDIRECTION,
    //    MEMORY,
    //    AI,
    //    AO,
    //    INT,
    //    CNT,
    //    LAST_TYPE = CNT
    //}

    public enum FocusingRet
    {
        Error = -1,
        Failed = 0,
        Success = 1

    }
}

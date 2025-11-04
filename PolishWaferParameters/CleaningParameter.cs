using System;
using System.Collections.Generic;

namespace PolishWaferParameters
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.PolishWafer;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using ProberInterfaces.Command;
    using ProberInterfaces.WaferAlignEX;

    public enum EnumEnable
    {
        Disable = 0,
        Enable = 1
    }

    public class FocusingPosition : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FocusingPosition(WaferCoordinate pos, HeightProfilignPosEnum posenum)
        {
            Position = pos;
            PosEnum = posenum;
        }

        private WaferCoordinate _Position;
        public WaferCoordinate Position
        {
            get { return _Position; }
            set
            {
                if (value != _Position)
                {
                    _Position = value;
                    RaisePropertyChanged();
                }
            }
        }

        private HeightProfilignPosEnum _PosEnum;
        public HeightProfilignPosEnum PosEnum
        {
            get { return _PosEnum; }
            set
            {
                if (value != _PosEnum)
                {
                    _PosEnum = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable, DataContract]
    public class PolishWaferCleaningParameter : IParam, INotifyPropertyChanged, IDeviceParameterizable, IPolishWaferCleaningParameter, IProbeCommandParameter
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
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
                #region //.. Emul Data

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

        public PolishWaferCleaningParameter()
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
        public PolishWaferCleaningParameter(string waferdefinetype)
        {
            this.WaferDefineType.Value = waferdefinetype;
        }

        #region //..Property


        private string _HashCode = string.Empty;
        [DataMember]
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

        private Element<string> _WaferDefineType = new Element<string>();
        [DataMember]
        public Element<string> WaferDefineType
        {
            get { return _WaferDefineType; }
            set
            {
                if (value != _WaferDefineType)
                {
                    _WaferDefineType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PWScrubMode> _CleaningScrubMode = new Element<PWScrubMode>();
        [DataMember]
        public Element<PWScrubMode> CleaningScrubMode
        {
            get { return _CleaningScrubMode; }
            set
            {
                if (value != _CleaningScrubMode)
                {
                    _CleaningScrubMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<CleaningDirection> _CleaningDirection = new Element<CleaningDirection>();
        /// <summary>
        /// 긁는방향
        /// </summary>
        [DataMember]
        public Element<CleaningDirection> CleaningDirection
        {
            get { return _CleaningDirection; }
            set
            {
                if (value != _CleaningDirection)
                {
                    _CleaningDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<CleaningDirection> _MoveDirection = new Element<CleaningDirection>();
        ///// <summary>
        ///// 이동 방향
        ///// </summary>
        //[DataMember]
        //public Element<CleaningDirection> MoveDirection
        //{
        //    get { return _MoveDirection; }
        //    set
        //    {
        //        if (value != _MoveDirection)
        //        {
        //            _MoveDirection = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<double> _ContactLength = new Element<double>() { Value = 1 , UpperLimit = 99999, LowerLimit = 0};
        /// <summary>
        /// 한 번 Contact 이후 움직일 길이.
        /// </summary>
        [DataMember]
        public Element<double> ContactLength
        {
            get { return _ContactLength; }
            set
            {
                if (value != _ContactLength)
                {
                    _ContactLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ContactCount = new Element<double>() { Value = 1 , UpperLimit = 999, LowerLimit = 0 };
        //private Element<double> _ContactCount = new Element<double>();
        /// <summary>
        /// 몇 번 찍을 것인지 (Pin과 접촉 횟수)
        /// </summary>
        [DataMember]
        public Element<double> ContactCount
        {
            get { return _ContactCount; }
            set
            {
                if (value != _ContactCount)
                {
                    _ContactCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ScrubingLength = new Element<double>() { Value = 1, UpperLimit = 999, LowerLimit = 0};
        /// <summary>
        /// 핀에 웨이퍼 댄체로 긁는길이
        /// </summary>
        [DataMember]
        public Element<double> ScrubingLength
        {
            get { return _ScrubingLength; }
            set
            {
                if (value != _ScrubingLength)
                {
                    _ScrubingLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _OverdriveValue = new Element<double>() { Value = 1, UpperLimit = 999, LowerLimit = -999};
        [DataMember]
        public Element<double> OverdriveValue
        {
            get { return _OverdriveValue; }
            set
            {
                if (value != _OverdriveValue)
                {
                    _OverdriveValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Clearance = new Element<double>() { Value = -1, UpperLimit = 0, LowerLimit = -9999};
        /// <summary>
        /// ZDown 시 얼마나 내릴건지 (Pin과 접촉후)
        /// </summary>
        [DataMember]
        public Element<double> Clearance
        {
            get { return _Clearance; }
            set
            {
                if (value != _Clearance)
                {
                    _Clearance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Thickness = new Element<double>();
        [DataMember]
        public Element<double> Thickness
        {
            get { return _Thickness; }
            set
            {
                if (value != _Thickness)
                {
                    _Thickness = value;
                    RaisePropertyChanged();
                }
            }
        }

        //Cleaning Sequence
        //private Element<PWContactSeqMode> _ContactSeqMode = new Element<PWContactSeqMode>();
        //public Element<PWContactSeqMode> ContactSeqMode
        //{
        //    get { return _ContactSeqMode; }
        //    set
        //    {
        //        if (value != _ContactSeqMode)
        //        {
        //            _ContactSeqMode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<PWPadShiftMode> _ShiftMode = new Element<PWPadShiftMode>();
        //public Element<PWPadShiftMode> ShiftMode
        //{
        //    get { return _ShiftMode; }
        //    set
        //    {
        //        if (value != _ShiftMode)
        //        {
        //            _ShiftMode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<double> _PadShiftOffset = new Element<double>();
        [DataMember]
        public Element<double> PadShiftOffset
        {
            get { return _PadShiftOffset; }
            set
            {
                if (value != _PadShiftOffset)
                {
                    _PadShiftOffset = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private Element<EnumEnable> _IsUserSeq = new Element<EnumEnable>();
        //public Element<EnumEnable> IsUserSeq
        //{
        //    get { return _IsUserSeq; }
        //    set
        //    {
        //        if (value != _IsUserSeq)
        //        {
        //            _IsUserSeq = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<double> _CurrentAngle = new Element<double>();
        /// <summary>
        /// CurrentAngle 을 가져오거나 설정합니다. 
        /// </summary>
        [DataMember]
        public Element<double> CurrentAngle
        {
            get { return _CurrentAngle; }
            set { _CurrentAngle = value; RaisePropertyChanged(); }
        }
        private Element<double> _RotateAngle = new Element<double>();
        /// <summary>
        /// RotateAngle 을 가져오거나 설정합니다. 
        /// </summary>
        [DataMember]
        public Element<double> RotateAngle
        {
            get { return _RotateAngle; }
            set { _RotateAngle = value; RaisePropertyChanged(); }
        }

        private FocusParameter _FocusParam;
        /// <summary>
        /// Focusing 파라미터를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set { _FocusParam = value; RaisePropertyChanged(); }
        }

        private ModuleDllInfo _FocusingModuleDllInfo;
        /// <summary>
        /// Focusing을 할 때, 사용되는 Module 정보를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }

        private Element<PWFocusingPointMode> _FocusingPointMode = new Element<PWFocusingPointMode>();
        /// <summary>
        /// Focusing을 할 때, 몇 포인트를 Focusing 할지 결정하는 파라미터입니다.
        /// </summary>
        [DataMember]
        public Element<PWFocusingPointMode> FocusingPointMode
        {
            get { return _FocusingPointMode; }
            set { _FocusingPointMode = value; }
        }

        private Element<PWFocusingType> _FocusingType = new Element<PWFocusingType>();
        /// <summary>
        /// Focusing을 할 때, 카메라로 할지, 터치 센서로 할지 결정하는 파라미터입니다.
        /// </summary>
        [DataMember]
        public Element<PWFocusingType> FocusingType
        {
            get { return _FocusingType; }
            set { _FocusingType = value; }
        }

        [NonSerialized]
        private ObservableCollection<FocusingPosition> _FocusingPos = new ObservableCollection<FocusingPosition>();
        /// <summary>
        ///     
        /// </summary>
        [ParamIgnore, JsonIgnore]
        public ObservableCollection<FocusingPosition> FocusingPos
        {
            get { return _FocusingPos; }
            set
            {
                if (value != _FocusingPos)
                {
                    _FocusingPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _CenterOffsetX;
        [ParamIgnore]
        [JsonIgnore]
        public double CenterOffsetX
        {
            get { return _CenterOffsetX; }
            set
            {
                if (value != _CenterOffsetX)
                {
                    _CenterOffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }
        [NonSerialized]
        private double _CenterOffsetY;
        [ParamIgnore]
        [JsonIgnore]
        public double CenterOffsetY
        {
            get { return _CenterOffsetY; }
            set
            {
                if (value != _CenterOffsetY)
                {
                    _CenterOffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }
        [NonSerialized]
        private double _MaxCleaningArea;
        [ParamIgnore]
        [JsonIgnore]
        public double MaxCleaningArea
        {
            get { return _MaxCleaningArea; }
            set
            {
                if (value != _MaxCleaningArea)
                {
                    _MaxCleaningArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private List<WaferCoordinate> _UserSeqPositions = new List<WaferCoordinate>();
        [ParamIgnore, JsonIgnore]
        public List<WaferCoordinate> UserSeqPositions
        {
            get { return _UserSeqPositions; }
            set
            {
                if (value != _UserSeqPositions)
                {
                    _UserSeqPositions = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _FocusingRangeOffset = new Element<double>();
        [DataMember]
        public Element<double> FocusingRangeOffset
        {
            get { return _FocusingRangeOffset; }
            set
            {
                if (value != _FocusingRangeOffset)
                {
                    _FocusingRangeOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Probe Card 가 버틸수 있는 OD 를 150 으로 본다. (카드마다 차이가 있긴 함) 
        /// </summary>
        private Element<double> _focusingHeightTolerance = new Element<double>() { Value = 150, UpperLimit = 999, LowerLimit = 0};
        [DataMember]
        public Element<double> FocusingHeightTolerance
        {
            get { return _focusingHeightTolerance; }
            set
            {
                if (value != _focusingHeightTolerance)
                {
                    _focusingHeightTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PinAlignBeforeCleaning = new Element<bool>();
        /// <summary>
        /// Cleaning_PolishWafer 하기 전에 PinAlign을 할 것인지.
        /// </summary>
        [DataMember]
        public Element<bool> PinAlignBeforeCleaning
        {
            get { return _PinAlignBeforeCleaning; }
            set
            {
                if (value != _PinAlignBeforeCleaning)
                {
                    _PinAlignBeforeCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PinAlignAfterCleaning = new Element<bool>();
        /// <summary>
        /// Cleaning_PolishWafer 후에 PinAlign을 할 것인지.
        /// </summary>
        [DataMember]
        public Element<bool> PinAlignAfterCleaning
        {
            get { return _PinAlignAfterCleaning; }
            set
            {
                if (value != _PinAlignAfterCleaning)
                {
                    _PinAlignAfterCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _EdgeDetectionBeforeCleaning = new Element<bool>();
        [DataMember]
        public Element<bool> EdgeDetectionBeforeCleaning
        {
            get { return _EdgeDetectionBeforeCleaning; }
            set
            {
                if (value != _EdgeDetectionBeforeCleaning)
                {
                    _EdgeDetectionBeforeCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LightValueParam> _CenteringLightParams;
        [DataMember]
        public ObservableCollection<LightValueParam> CenteringLightParams
        {
            get { return _CenteringLightParams; }
            set
            {
                if (value != _CenteringLightParams)
                {
                    _CenteringLightParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        //[NonSerialized]
        //private bool _RequestedPolishWafer = new bool();
        //[ParamIgnore, JsonIgnore]
        //public bool RequestedPolishWafer
        //{
        //    get { return _RequestedPolishWafer; }
        //    set
        //    {
        //        if (value != _RequestedPolishWafer)
        //        {
        //            _RequestedPolishWafer = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        [NonSerialized]
        private string _Command = string.Empty;
        [ParamIgnore, JsonIgnore]
        public string Command
        {
            get { return _Command; }
            set
            {
                if (value != _Command)
                {
                    _Command = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
    }
}

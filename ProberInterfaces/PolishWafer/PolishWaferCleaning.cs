using System;

namespace ProberInterfaces.PolishWafer
{
    using ProberErrorCode;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [Serializable, DataContract]

    public enum PWFocusingPointMode
    {
        [EnumMember]
        UNDEFINED = 0,
        [EnumMember]
        POINT1 = 1,
        [EnumMember]
        POINT5 = 5,
        [EnumMember]
        POINT9 = 9
    }
    [Serializable, DataContract]

    public enum PWFocusingType
    {
        [EnumMember]
        UNDEFIEND = 0,
        [EnumMember]
        CAMERA,
        [EnumMember]
        TOUCHSENSOR
    }
    [Serializable, DataContract]

    public enum PWScrubMode
    {
        [EnumMember]
        UP_DOWN,
        [EnumMember]
        One_Direction,
        [EnumMember]
        Octagonal,
        [EnumMember]
        Square
    }
    [Serializable, DataContract]

    public enum PWContactSeqMode
    {
        [EnumMember]
        ContactLength,
        [EnumMember]
        PositionShift
    }
    [Serializable, DataContract]

    public enum PWPadShiftMode
    {
        [EnumMember]
        DutSize,
        [EnumMember]
        DutSize_Total,
        [EnumMember]
        UserOffset
    }

    public interface IPolishWaferCentering : IFactoryModule
    {
        EventCodeEnum DoCentering(IPolishWaferCleaningParameter param);
    }

    public interface IPolishWaferFocusing : IFactoryModule
    {
        EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param, bool manualmode = false);
    }
    public interface IPolishWaferFocusingBySensor : IFactoryModule
    {
        EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param, bool manualmode = false);
    }

    public interface IPolishWaferCleaning : IFactoryModule
    {
        EventCodeEnum DoCleaning(IPolishWaferCleaningParameter param);
    }

    public interface IPolishWaferProcessor : IFactoryModule
    {
        EventCodeEnum DoCentering(IPolishWaferCleaningParameter param);
        EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param);
        EventCodeEnum DoCleaning(IPolishWaferCleaningParameter param);
    }

    public interface IPolishWaferIntervalParameter
    {
        ObservableCollection<IPolishWaferCleaningParameter> CleaningParameters { get; set; }
        //IPolishWaferCleaningParameter CurCleaningParameter { get; set; }
        Element<int> IntervalCount { get; set; }
        Element<int> TouchdownCount { get; set; }
        Element<EnumCleaningTriggerMode> CleaningTriggerMode { get; set; }
        string HashCode { get; set; }
    }

    public interface IPolishWaferCleaningParameter: IParamNode
    {
        Element<string> WaferDefineType { get; set; }
        Element<PWScrubMode> CleaningScrubMode { get; set; }
        Element<double> ContactCount { get; set; }
        Element<double> ContactLength { get; set; }
        Element<CleaningDirection> CleaningDirection { get; set; }
        Element<double> ScrubingLength { get; set; }
        Element<bool> PinAlignBeforeCleaning { get; set; }
        Element<bool> PinAlignAfterCleaning { get; set; }
        Element<bool> EdgeDetectionBeforeCleaning { get; set; }

        //bool PinAlignBeforeCleaningProcessed { get; set; }
        //bool PinAlignAfterCleaningProcessed { get; set; }
        //bool PolishWaferCleaningProcessed { get; set; }
        //bool RequestedPolishWafer { get; set; }
        //bool PolishWaferCleaningRetry { get; set; }
        Element<PWFocusingType> FocusingType { get; set; }
        FocusParameter FocusParam { get; set; }
        ModuleDllInfo FocusingModuleDllInfo { get; set; }
        Element<PWFocusingPointMode> FocusingPointMode { get; set; }
        Element<double> FocusingHeightTolerance { get; set; }
        ObservableCollection<LightValueParam> CenteringLightParams { get; set; }

        Element<double> Clearance { get; set; }
        Element<double> OverdriveValue { get; set; }
        Element<double> Thickness { get; set; }
        string HashCode { get; set; }
    }


    public interface IPolishWaferParameter
    {
        //Element<bool> PinAlignBeforeCleaning { get; set; }
        //Element<bool> PinAlignAfterCleaning { get; set; }
        //Element<bool> EdgeDetectionBeforeCleaning { get; set; }
        //bool NeedLoadWaferFlag { get; set; }
        //string LoadWaferType { get; set; }
        //bool PinAlignBeforeCleaningProcessed { get; set; }
        //bool PinAlignAfterCleaningProcessed { get; set; }
        //bool PolishWaferCleaningProcessed { get; set; }
    }
}

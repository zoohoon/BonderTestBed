using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    //using MapObject;
    using ProberInterfaces.Param;
    using ProberInterfaces.WaferAlign;
    using ProberInterfaces.PnpSetup;
    using ProberErrorCode;
    using ProberInterfaces.PMI;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using ProberInterfaces.Enum;
    using System.Windows;

    public class WaferObjectDelegate : INotifyPropertyChanged
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

        public delegate Task ChangeMapIndexDelegate(object newVal);
    }
    public interface IWaferObject : IZoomObject, IFactoryModule, IAlignModule,
                                    IHasDevParameterizable, INotifyPropertyChanged,
                                    IModuleParameter, IHasComParameterizable
    {
        WaferObjectDelegate.ChangeMapIndexDelegate ChangeMapIndexDelegate { get; set; }
        List<IDeviceObject> GetDevices();
        UserIndex MachineToUserIndex(MachineIndex machine);
        MachineIndex UserToMachineIndex(UserIndex user);
        List<MachineIndex> makeMultiProbeSeq(IProbeCard card, bool bLeft, bool bTop, int SampleInterval = 1, bool bStartRight = false, bool bUseOneDir = false);
        List<MachineIndex> makeMultiProbeSeq_hor(IProbeCard card, bool bLeft, bool bTop, int SampleInterval = 1, bool bStartRight = false, bool bUseOneDir = false);
        int CheckSampleDieExist(int mx, int my, IProbeCard card, Byte[,] tmp_map);
        byte[,] DevicesConvertByteArray();
        EnumWaferState GetState();
        EnumSubsStatus GetStatus();
        IPhysicalInfo GetPhysInfo();
        ISubstrateInfo GetSubsInfo();
        IPolishWaferSourceInformation GetPolishInfo();
        void SetPhysInfo(IPhysicalInfo physicalInfo);
        void SetWaferStatus(EnumSubsStatus status, EnumWaferType waferType = EnumWaferType.UNDEFINED, string waferID = "", int slotNum = 0);
        void SetWaferState(EnumWaferState state);
        EventCodeEnum ResetWaferData();
        EventCodeEnum ResetWaferData(MachineIndex MI);
        EnumSubsStatus WaferStatus { get; }
        EnumWaferType GetWaferType();
        EventCodeEnum UpdateWaferObject();
        EventCodeEnum UpdateData();
        void SetCurrentMIndex(MachineIndex index);
        void SetCurrentMIndex(long xindex, long yindex);
        void SetCurrentUIndex(UserIndex index);
        MachineIndex GetCurrentMIndex();
        UserIndex GetCurrentUIndex();
        bool MapViewCurIndexVisiablity { get; set; }
        bool MapViewStageSyncEnable { get; set; }
        string MapViewStepLabel { get; set; }
        bool IsMapViewShowPMITable { get; set; }
        bool IsMapViewShowPMIEnable { get; set; }
        MapViewMode MapViewControlMode { get; set; }
        EventCodeEnum DrawDieOverlay(ICamera cam);
        EventCodeEnum StopDrawDieOberlay(ICamera cam);
        Element<EnumProberCam> MapViewAssignCamType { get; set; }
        Element<int> SequenceProcessedCount { get; }
        Element<long> ValidDieCount { get; }
        WaferHeightMapping WaferHeightMapping { get; }
        List<DutWaferIndex> DutDieMatchIndexs { get; }
        Element<long> TestDieCount { get; }
        IWaferDevObject WaferDevObjectRef { get; }
        IPMIInfo PMIInfo { get; }
        void SetDutDieMatchIndexs(List<DutWaferIndex> dutDieMatchIndexs);
        Element<bool> WaferAlignSetupChangedToggle { get; set; }
        Element<bool> PadSetupChangedToggle { get; set; }
        event EventHandler ChangedWaferObjectEvent;
        void CallWaferobjectChangedEvent();
        int? GetSlotIndex();
        bool OnceStopBeforeProbing { get; set; }
        bool OnceStopAfterProbing { get; set; }
        bool IsSendfWaferStartEvent { get; set; }
        int GetOriginFoupNumber();
        DispFlipEnum DispHorFlip { get; set; }
        DispFlipEnum DispVerFlip { get; set; }
        EventCodeEnum ChangeAlignSetupControlFlag(DrawDieOverlayEnum mode, bool flag, int offset = 0);

        Visibility TopLeftToBottomRightLineVisible { get; set; }
        Visibility BottomLeftToTopRightLineVisible { get; set; }
    }

    public interface IWaferDevObject : IDeviceParameterizable
    {
    }

    [DataContract]
    public class WrapperDIEs
    {
        private DeviceObject[,] _DIEs;
        [DataMember]
        public DeviceObject[,] DIEs
        {
            get { return _DIEs; }
            set
            {
                if (value != _DIEs)
                {
                    _DIEs = value;
                }
            }
        }
    }

    [DataContract]
    public class WaferObjectInfoNonSerialized
    {

        private MapViewMode _MapViewRenderMode = MapViewMode.MapMode;
        [DataMember]
        public MapViewMode MapViewRenderMode
        {
            get { return _MapViewRenderMode; }
            set { _MapViewRenderMode = value; }
        }

        private bool _MapViewStageSyncEnable;
        [DataMember]
        public bool MapViewStageSyncEnable
        {
            get { return _MapViewStageSyncEnable; }
            set { _MapViewStageSyncEnable = value; }
        }

        private bool _MapViewCurIndexVisiablity;
        [DataMember]
        public bool MapViewCurIndexVisiablity
        {
            get { return _MapViewCurIndexVisiablity; }
            set { _MapViewCurIndexVisiablity = value; }
        }

        private Visibility _TopLeftToBottomRightLineVisible;
        [DataMember]
        public Visibility TopLeftToBottomRightLineVisible
        {
            get { return _TopLeftToBottomRightLineVisible; }
            set { _TopLeftToBottomRightLineVisible = value; }
        }

        private Visibility _BottomLeftToTopRightLineVisible;
        [DataMember]
        public Visibility BottomLeftToTopRightLineVisible
        {
            get { return _BottomLeftToTopRightLineVisible; }
            set { _BottomLeftToTopRightLineVisible = value; }
        }
    }

    [DataContract]
    public class SubstrateInfoNonSerialized : ISubstrateInfoNonSerialized
    {
        private Element<bool> _WaferObjectChangedToggle;
        [DataMember]
        public Element<bool> WaferObjectChangedToggle
        {
            get { return _WaferObjectChangedToggle; }
            set
            {
                if (value != _WaferObjectChangedToggle)
                {
                    _WaferObjectChangedToggle = value;
                }
            }
        }

        private WaferCoordinate _WaferCenter;
        [DataMember]
        public WaferCoordinate WaferCenter
        {
            get { return _WaferCenter; }
            set
            {
                if (value != _WaferCenter)
                {
                    _WaferCenter = value;
                }
            }
        }

        //public WaferCoordinate WaferCenter { get; set; }
        //[DataMember]
        //public double LoadingAngle { get; set; }
        //[DataMember]
        //public double ContactCount { get; set; }
        //[DataMember]
        //public EnumWaferType WaferType { get; set; }

        //private WrapperDIEs _WrapperDIEs;
        //[DataMember]
        //public WrapperDIEs WrapperDIEs
        //{
        //    get { return _WrapperDIEs; }
        //    set
        //    {
        //        if (value != _WrapperDIEs)
        //        {
        //            _WrapperDIEs = value;
        //        }
        //    }
        //}

        //private IDeviceObject[,] _DIEs;
        //[DataMember]
        //public IDeviceObject[,] DIEs
        //{
        //    get { return _DIEs; }
        //    set
        //    {
        //        if (value != _DIEs)
        //        {
        //            _DIEs = value;
        //        }
        //    }
        //}

        //[DataMember]
        //public SynchronizedObservableCollection<DeviceObject> PMIDIEs { get; set; }
        //[DataMember]
        //public DeviceObject CurrentDie { get; set; }
        //[DataMember]
        //public Element<int> SequenceProcessedCount { get; set; }

        private RectSize _ActualDieSize;
        [DataMember]
        public RectSize ActualDieSize
        {
            get { return _ActualDieSize; }
            set
            {
                if (value != _ActualDieSize)
                {
                    _ActualDieSize = value;
                }
            }
        }

        private RectSize _ActualDeviceSize;
        [DataMember]
        public RectSize ActualDeviceSize
        {
            get { return _ActualDeviceSize; }
            set
            {
                if (value != _ActualDeviceSize)
                {
                    _ActualDeviceSize = value;
                }
            }
        }



        //[DataMember]
        //public WaferCoordinate WaferCenterOffset { get; set; }
        //[DataMember]
        //public double MachineCoordZeroPosX { get; set; }
        //[DataMember]
        //public double MachineCoordZeroPosY { get; set; }
        //[DataMember]
        //public Element<double> WaferSequareness { get; set; }
        //[DataMember]
        //public Element<double> MachineSequareness { get; set; }
        //[DataMember]
        //public double AveWaferThick { get; set; }
        //[DataMember]
        //public double ActualThickness { get; set; }
        //[DataMember]
        //public WaferCoordinate RefDieLeftCorner { get; set; }
        //[DataMember]
        //public bool isProbingDone { get; set; }
        //[DataMember]
        //public string OperatorID { get; set; }
        //[DataMember]
        //public long OperatorLevel { get; set; }
        //[DataMember]
        //public int CPCount { get; set; }
        //[DataMember]
        //public int RetestCount { get; set; }
        //[DataMember]
        //public Element<long> TestedDieCount { get; set; }
        //[DataMember]
        //public Element<long> PassedDieCount { get; set; }
        //[DataMember]
        //public Element<long> FailedDieCount { get; set; }
        //[DataMember]
        //public Element<long> CurTestedDieCount { get; set; }
        //[DataMember]
        //public Element<long> CurPassedDieCount { get; set; }
        //[DataMember]
        //public Element<long> CurFailedDieCount { get; set; }
        //[DataMember]
        //public double Yield { get; set; }
        //[DataMember]
        //public double RetestYield { get; set; }
        //[DataMember]
        //public float ChuckTemperature { get; set; }
        //[DataMember]
        //public DateTime ProbingStartTime { get; set; }
        //[DataMember]
        //public DateTime ProbingEndTime { get; set; }
        //[DataMember]
        //public DateTime LoadingTime { get; set; }
        //[DataMember]
        //public DateTime UnloadingTime { get; set; }
        [DataMember]
        public double DutCenX { get; set; }
        [DataMember]
        public double DutCenY { get; set; }
    }

    [DataContract]
    public class WrapperSubstrateInfoNonSerialized
    {
        [DataMember]
        private SubstrateInfoNonSerialized _MyInterface;
        public ISubstrateInfoNonSerialized MyInterface
        {
            get { return (_MyInterface as ISubstrateInfoNonSerialized); }
            set { _MyInterface = (value as SubstrateInfoNonSerialized); }
        }
    }

    public interface ISubstrateInfoNonSerialized
    {
        Element<bool> WaferObjectChangedToggle { get; set; }
        WaferCoordinate WaferCenter { get; set; }
        //double LoadingAngle { get; set; }
        //double ContactCount { get; set; }
        //EnumWaferType WaferType { get; set; }

        //WrapperDIEs WrapperDIEs { get;set; }
        //IDeviceObject[,] DIEs { get; set; }
        //SynchronizedObservableCollection<DeviceObject> PMIDIEs { get; set; }
        //DeviceObject CurrentDie { get; set; }
        //Element<int> SequenceProcessedCount { get; set; }
        RectSize ActualDieSize { get; set; }
        RectSize ActualDeviceSize { get; set; }
        //WaferCoordinate WaferCenterOffset { get; set; }
        //double MachineCoordZeroPosX { get; set; }
        //double MachineCoordZeroPosY { get; set; }
        //Element<double> WaferSequareness { get; set; }
        //Element<double> MachineSequareness { get; set; }
        //double AveWaferThick { get; set; }
        //double ActualThickness { get; set; }
        //WaferCoordinate RefDieLeftCorner { get; set; }
        //bool isProbingDone { get; set; }
        //string OperatorID { get; set; }
        //long OperatorLevel { get; set; }
        //int CPCount { get; set; }
        //int RetestCount { get; set; }
        //Element<long> TestedDieCount { get; set; }
        //Element<long> PassedDieCount { get; set; }
        //Element<long> FailedDieCount { get; set; }
        //Element<long> CurTestedDieCount { get; set; }
        //Element<long> CurPassedDieCount { get; set; }
        //Element<long> CurFailedDieCount { get; set; }
        //double Yield { get; set; }
        //double RetestYield { get; set; }
        //float ChuckTemperature { get; set; }
        //DateTime ProbingStartTime { get; set; }
        //DateTime ProbingEndTime { get; set; }
        //DateTime LoadingTime { get; set; }
        //DateTime UnloadingTime { get; set; }
        //double DutCenX { get; set; }
        //double DutCenY { get; set; }
    }

    public interface ISubstrateInfo
    {
        Element<bool> WaferObjectChangedToggle { get; set; }
        Element<long> TestedDieCount { get; set; }
        Element<long> PassedDieCount { get; set; }
        Element<long> FailedDieCount { get; set; }
        Element<long> CurTestedDieCount { get; set; }
        Element<long> CurPassedDieCount { get; set; }
        Element<long> CurFailedDieCount { get; set; }
        DateTime ProbingStartTime { get; set; }
        DateTime ProbingEndTime { get; set; }
        DateTime LoadingTime { get; set; }
        DateTime UnloadingTime { get; set; }
        Element<string> OperatorName { get; set; }
        Element<string> DeviceName { get; set; }
        double Yield { get; set; }
        double RetestYield { get; set; }
        string CassetteID { get; set; }
        byte CassetteNo { get; set; }
        Element<string> WaferID { get; set; }
        EnumWaferType WaferType { get; set; }
        IDeviceObject[,] DIEs { get; set; }
        RectSize ActualDeviceSize { get; set; }
        RectSize ActualDieSize { get; set; }
        double AveWaferThick { get; set; }
        double ActualThickness { get; set; }
        Element<double> DieXClearance { get; set; }
        Element<double> DieYClearance { get; set; }
        double LoadingAngle { get; set; }
        double MachineCoordZeroPosX { get; set; }
        double MachineCoordZeroPosY { get; set; }
        Element<double> MachineSequareness { get; set; }
        WaferCoordinate RefDieLeftCorner { get; set; }
        WaferCoordinate WaferCenter { get; set; }
        WaferCoordinate WaferCenterOffset { get; set; }
        Element<double> WaferSequareness { get; set; }
        WaferCoordinate WaferCenterOriginatEdge { get; set; }
        PadGroup Pads { get; set; }
        double DutCenX { get; set; }
        double DutCenY { get; set; }
        double ContactCount { get; set; }
        Element<int> SlotIndex { get; set; }
        double MoveZOffset { get; set; }
        IPMIInfo GetPMIInfo();
        void SetPMIInfo(IPMIInfo pmiinfo);
        IList<DeviceObject> Devices { get; set; }
        SynchronizedObservableCollection<DeviceObject> PMIDIEs { get; set; }
        DeviceObject CurrentDie { get; set; }
        EventCodeEnum UpdatePadsToDevice(DeviceObject target);
        EventCodeEnum UpdatePadsToDevices();
        EventCodeEnum UpdateYield();
        EventCodeEnum UpdateCurrentDieCount();
    }

    public interface IPolishWaferSourceInformation
    {
        Element<string> DefineName { get; set; }
        Element<double> MaxLimitCount { get; set; }
        Element<double> TouchCount { get; set; }
        Element<double> Margin { get; set; }
        Element<SubstrateSizeEnum> Size { get; set; }
        Element<WaferNotchTypeEnum> NotchType { get; set; }
        double DeadZone { get; set; }
        WaferHeightMapping WaferHeightMapping { get; set; }
        Element<double> Thickness { get; set; }
        WaferCoordinate PolishWaferCenter { get; set; }
        void Copy(IPolishWaferSourceInformation Source);
        Element<double> CurrentAngle { get; set; }
        Element<double> NotchAngle { get; set; }
        Element<double> RotateAngle { get; set; }
        Element<string> IdentificationColorBrush { get; set; }
        Element<int> Priorty { get; set; }
        OCRDevParameter OCRConfigParam { get; set; }


    }

    public enum MapHorDirectionEnum
    {
        UNDEFINED,
        RIGHT = 1,
        LEFT = -1,
    }
    public enum MapVertDirectionEnum
    {
        UNDEFINED,
        DOWN = -1,
        UP = 1,
    }
    public enum WaferNotchTypeEnum
    {
        UNKNOWN = -1,
        FLAT = 0,
        NOTCH,
    }

    public enum NotchDriectionEnum
    {
        COUNTERCLOCKWISE,
        CLOCKWISE,
    }

    public enum SubstrateType
    {
        WAFER = 0,
        FRAMED = 1,
    }

    [Serializable, DataContract]
    public enum EnumSubsStatus
    {
        [EnumMember]
        UNKNOWN = -1,
        [EnumMember]
        UNDEFINED = 0,
        [EnumMember]
        NOT_EXIST = 1,
        [EnumMember]
        EXIST = 2,
        [EnumMember]
        HIDDEN = 3,
        [EnumMember]
        CARRIER = 4
    }
    public enum EnumReservationState
    {
        NOT_RESERVE,
        RESERVE
    }

    [Serializable, DataContract]
    public enum EnumWaferState
    {
        [EnumMember]
        UNDEFINED = 0,
        //NOT_EXIST = 1,
        [EnumMember]
        UNPROCESSED = 1,
        [EnumMember]
        PROBING = 2,
        [EnumMember]
        PROCESSED = 3,
        [EnumMember]
        TESTED = 4,
        [EnumMember]
        SKIPPED = 5,
        [EnumMember]
        MISSED = 6,
        [EnumMember]
        CLEANING = 7,
        [EnumMember]
        READY = 8,
        [EnumMember]
        SOAKINGSUSPEND = 9,
        [EnumMember]
        SOAKINGDONE = 10 
    }
    public enum EnumDieState
    {
        PASS = 0,
        FAIL = 1,
        NOTPROCESSED = 2,
        PROCESSED = 3,
        EMPTY = 4,
        MARK = 5,
        SKIP = 6,
    }

    public enum MapViewMode
    {
        UNDIFIND = 0,
        MapMode,
        BinMode,
        SeqenceMode
    }
    //public interface IProbingSequence : IEnumerable<MachineIndex>
    //{
    //    List<MachineIndex> Sequences { get; set; }
    //}

    //[System.Xml.Serialization.XmlRoot("ProbingSequenceBase")]
    //public abstract class ProbingSequenceBase : IProbingSequence
    //{

    //    public abstract List<MachineIndex> Sequences { get; set; }

    //    public abstract IEnumerator<MachineIndex> GetEnumerator();

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return this.GetEnumerator();
    //    }
    //    public void Add(object value)
    //    {

    //    }
    //}
    public interface IWaferState
    {
        EnumWaferState GetState();
        void SetUnprocessed();
        void SetProcessed();
        void SetProcessing();
        void SetSkipped();
        void SetSoakingSuspend();
        void SetSoakingDone();
    }
    public interface IWaferStatus
    {
        EnumSubsStatus GetState();
        void SetStatusMissing();
        void SetStatusUnloaded();
        void SetStatusLoaded();
    }
}

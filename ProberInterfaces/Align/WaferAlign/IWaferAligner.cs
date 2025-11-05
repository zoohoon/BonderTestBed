namespace ProberInterfaces
{
    using ProberInterfaces.Param;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Windows;
    using ProberInterfaces.Align;
    //using NavigationMap;
    using System;
    using System.Collections.ObjectModel;
    using ProberInterfaces.WaferAlignEX;
    using ProberInterfaces.PnpSetup;
    using System.Collections.Generic;
    using ProberErrorCode;
    using ProberInterfaces.Wizard;
    using ProberInterfaces.State;
    using System.ComponentModel;
    using ProberInterfaces.WaferAlignEX.Enum;
    using System.ServiceModel;

    [ServiceContract]
    public interface IWaferAligner : IPnpSetupScreen, ITemplateStateModule, IWizardMainStep, IStateModule, IManualOPReadyAble
    {
        IInnerState PreInnerState { get; }
        [OperationContract(IsOneWay =true)]
        void SetIsNewSetup(bool flag);
        [OperationContract(IsOneWay = true)]
        void SetIsModifySetup(bool flag);
        [OperationContract]
        bool GetIsModifySetup();
        [OperationContract]
        bool GetIsModify();
        [OperationContract]
        void SetIsModify(bool flag);
        bool IsNewSetup { get; set; }
        int TotalHeightPoint { get; set; }
        bool waferaligncontinus { get; set; }
        bool IsOnDubugMode { get; set; }
        string IsOnDebugImagePathBase { get; set; }
        string IsOnDebugPadPathBase { get; set; }
       [OperationContract]
        bool IsServiceAvailable();
        WaferAlignInfomation WaferAlignInfo { get; }

        //AlignState WaferAlignState { get; set; }
       // IWaferAlignDeviceFile WADeviceFile { get; }
        WARecoveryModule RecoveryInfo { get; set; }

        WaferCoordinate PlanePointCenter { get; set; }
        HeightPointEnum HeightProfilingPointType { get; set; }
        double GetReviseSquarness(double xpos, double ypos);
        //int HeightSearchIndex(double x, double y);
        void AddHeighPlanePoint(WAHeightPositionParam param = null, bool center = false);
        void ResetHeightPlanePoint();
        WaferAlignInnerStateEnum GetWAInnerStateEnum();
        List<WaferCoordinate> GetHieghtPlanePoint();
        EventCodeEnum GetHeightValueAddZOffsetFromDutIndex(EnumProberCam camtype, long mach_x, long mach_y, double zoffset, out double zpos);
        double GetHeightValue(double posX, double posY, bool logwrite = false);
        double GetHeightValueAddZOffset(double posX, double posY, double offsetZ, bool logwrite = false);
        //EventCodeEnum GetHeightValueFromDutIndex(EnumProberCam camtype, long mach_x, long mach_y, out double zpos);
        //Task<FocusingRet> FocusingAsync(EnumProberCam cam, Rect roi, CancellationToken token);
        double CalcThreePodTiltedPlane(double posx, double posy, bool logwrite = false);
        int SaveWaferAveThick();
        void PlanePointChangetoFocusing5pt(double xpos, double ypos, double zpos);
        void PlanePointChangetoFocusing9pt(ObservableCollection<WAHeightPositionParam> heightparam);
        [OperationContract]
        WaferCoordinate MachineIndexConvertToDieLeftCorner(long xindex, long yindex);
        [OperationContract]
        WaferCoordinate MachineIndexConvertToDieLeftCorner_NonCalcZ(long xindex, long yindex);
        [OperationContract]
        WaferCoordinate MachineIndexConvertToDieCenter(long xindex, long yindex);
        [OperationContract]
        WaferCoordinate MachineIndexConvertToProbingCoord(long xindex, long yindex, bool logWrite = true);
        Point GetLeftCornerPosition(double positionx, double positiony);
        Point GetLeftCornerPosition(WaferCoordinate position);
        [OperationContract]
        Point GetLeftCornerPositionForWAFCoord(WaferCoordinate position);
        Point UserIndexConvertLeftBottomCorner(UserIndex uindex);
        Point UserIndexConvertLeftBottomCorner(long xindex, long yindex);
        MachineIndex WPosToMIndex(WaferCoordinate coordinate);
        [OperationContract(IsOneWay = true)]
        void SetSetupState();
        void SetModuleDoneState();
        [OperationContract]
        EventCodeEnum EdgeCheck(ref WaferCoordinate centeroffset, ref double maximum_Value_X, ref double maximum_Value_Y);
        void InitIdelState();
        void UpdatePadCen();
        void CreateBaseHeightProfiling(WaferCoordinate coordinate);
        //PMResult PatternMatchingRetry(PatternInfomation ptinfo, FocusParameter focusingparam = null, IFocusing focusmodel = null, object callerAssembly = null,
        //    ushort miniumscore = 0, ushort maxinumscore = 255, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0);
        EventCodeEnum SetTeachDevice(bool isMoving = true, long xindex = -1, long yindex = -1, EnumProberCam enumProberCam = EnumProberCam.WAFER_HIGH_CAM);
        List<DutWaferIndex> GetDutDieIndexs();
        void SetDefaultDutDieIndexs();
        long TeachDieXIndex { get; set; }
        long TeachDieYIndex { get; set; }
        IWaferAlignControItems WaferAlignControItems { get; }
        [OperationContract]
        Task<bool> CheckPossibleSetup(bool isrecovery = false);
        [OperationContract]
        ModuleStateEnum GetModuleState();

        [OperationContract]
        ModuleStateEnum GetPreModuleState();
        EventCodeEnum ClearRecoveryData();
        string ManuallAlignmentErrTxt { get; set; }

        void AddHeighPlanePoint(WaferCoordinate wcoord);
        EventCodeEnum GetHighStandardParam();
        EventCodeEnum SetHighStandardParam(HeightPointEnum heightpoint);
        WaferCoordinate GetPatternWaferCenter();
        EventCodeEnum SaveRecoveryLowPattern();
        EventCodeEnum SaveRecoveryHighPattern();
        EventCodeEnum ClearRecoverySetupPattern();
        void SetRefPad(IList<DUTPadObject> padinfo);

        [OperationContract]
        (double, double) GetVerifyCenterLimitXYValue();
        [OperationContract]
        void SetVerifyCenterLimitXYValue(double xLimit, double yLimit);
        List<Guid> GetRecoverySteps();
        //EventCodeEnum DoWaferAlignProcess();
        double SafeHeightOnException { get; }
        EventCodeEnum CalculateOffsetToAutoFocusedPosition(ICamera curcam, double padsize_x, double padsize_y);
    }

    public class WARecoveryModule
    {
        private ObservableCollection<ISubModule> _InvaliedModules
            = new ObservableCollection<ISubModule>();

        public ObservableCollection<ISubModule> InvaliedModules
        {
            get { return _InvaliedModules; }
            set { _InvaliedModules = value; }
        }


        private int _FailedModuleIndex;

        public int FailedModuleIndex
        {
            get { return _FailedModuleIndex; }
            set { _FailedModuleIndex = value; }
        }

        public WARecoveryModule()
        {

        }
        public WARecoveryModule(int index)
        {
            FailedModuleIndex = index;
        }
    }

    public class WaferAlignInfomation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private List<WaferProcResult> _AlignProcResult = new List<WaferProcResult>();
        public List<WaferProcResult> AlignProcResult
        {
            get { return _AlignProcResult; }
            set
            {
                if (value != _AlignProcResult)
                {
                    _AlignProcResult = value;
                    NotifyPropertyChanged("AlignProcResult");
                }
            }
        }
        private double _AlignAngle;
        public double AlignAngle
        {
            get { return _AlignAngle; }
            set
            {
                if (value != _AlignAngle)
                {
                    _AlignAngle = value;
                    NotifyPropertyChanged("AlignAngle");
                }
            }
        }


        private double _VerifyAngle;
        public double VerifyAngle
        {
            get { return _VerifyAngle; }
            set
            {
                if (value != _VerifyAngle)
                {
                    _VerifyAngle = value;
                    NotifyPropertyChanged("VerifyAngle");
                }
            }
        }

        private WARecoveryParam _RecoveryParam = new WARecoveryParam();
        public WARecoveryParam RecoveryParam
        {
            get { return _RecoveryParam; }
            set
            {
                if (value != _RecoveryParam)
                {
                    _RecoveryParam = value;
                    NotifyPropertyChanged("RecoveryParam");
                }
            }
        }

        private ThetaAlignMeasurementTable _LowMeasurementTable
            = new ThetaAlignMeasurementTable();
        public ThetaAlignMeasurementTable LowMeasurementTable
        {
            get { return _LowMeasurementTable; }
            set
            {
                if (value != _LowMeasurementTable)
                {
                    _LowMeasurementTable = value;
                    NotifyPropertyChanged("LowMeasurementTable");
                }
            }
        }

        private ThetaAlignMeasurementTable _HighMeasurementTable
            = new ThetaAlignMeasurementTable();
        public ThetaAlignMeasurementTable HighMeasurementTable
        {
            get { return _HighMeasurementTable; }
            set
            {
                if (value != _HighMeasurementTable)
                {
                    _HighMeasurementTable = value;
                    NotifyPropertyChanged("HighMeasurementTable");
                }
            }
        }

        private List<WAHeightPositionParam> _HeightPositions;

        public List<WAHeightPositionParam> HeightPositions
        {
            get { return _HeightPositions; }
            set { _HeightPositions = value; }
        }


        /// <summary>
        /// Pattern 등록시 모든 패턴의 기준이 될 WaferCenter
        /// </summary>
        private WaferCoordinate _PTWaferCenter;
        public WaferCoordinate PTWaferCenter
        {
            get { return _PTWaferCenter; }
            set { _PTWaferCenter = value; }
        }

        private CatCoordinates _LowFirstPatternPosition;
        /// <summary>
        /// Setup 시 High 단계에서 Low 패턴위치로 이동하기위해.
        /// </summary>
        public CatCoordinates LowFirstPatternPosition
        {
            get { return _LowFirstPatternPosition; }
            set { _LowFirstPatternPosition = value; }
        }

        private bool _LotLoadingPosCheckMode = false;
        public bool LotLoadingPosCheckMode
        {
            get { return _LotLoadingPosCheckMode; }    
            set
            {
                if (value != _LotLoadingPosCheckMode)
                {
                    _LotLoadingPosCheckMode = value;
                }
            }
        }


    }

    public interface IWaferAlignControItems
    {
        bool IsManualRecoveryModifyMode { get; set; }
        bool EdgeFail { get; set; }
        EnumLowStandardPosition LowFailPos { get; set; }
        EnumHighStandardPosition HighFailPos { get; set; }
        bool IsDebugEdgeProcessing { get; set; }
    }       
}

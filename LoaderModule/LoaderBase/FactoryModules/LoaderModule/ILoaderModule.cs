using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using ProberInterfaces;
using LoaderParameters;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LoaderServiceBase;
using LoaderParameters.Data;
using ProberInterfaces.PreAligner;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using SecsGemServiceInterface;
using LoaderBase.Communication;
using LogModule;
using ProberInterfaces.Monitoring;
using MetroDialogInterfaces;
using ProberInterfaces.Device;
using ProberInterfaces.Enum;
using LoaderBase.LoaderLog;

namespace LoaderBase
{
    /// <summary>
    /// LoaderModule을 정의합니다.
    /// </summary>
    public interface ILoaderModule : ILoaderSubModule
    {
        /// <summary>
        /// ModuleState 를 가져옵니다.
        /// </summary>
        /// 
        SubstrateSizeEnum DeviceSize { get; set; }
        ObservableCollection<LoaderJobViewData> LoaderJobViewList { get; }
        ObservableCollection<LoaderJobViewList> LoaderJobCollection { get; }
        ModuleStateEnum ModuleState { get; }
        string ResonOfError { get; set; }
        string ErrorDetails { get; set; }
        string RecoveryBehavior { get; set; }
        int RecoveryCellIdx { get; set; }
        /// <summary>
        /// RootParamPath 를 가져옵니다.
        /// </summary>
        string RootParamPath { get; }

        /// <summary>
        /// 모듈이 초기화 되었는 지 여부를 가져옵니다.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 모듈의 시스템 파라미터를 가져옵니다.
        /// </summary>
        EventCodeEnum LoadDevParameter();

        /// <summary>
        /// 모듈의 시스템 파라미터를 가져옵니다.
        /// </summary>
        LoaderSystemParameter SystemParameter { get; }


        LoaderTestOption LoaderOption { get; set; }

        LoaderProcModuleInfo ProcModuleInfo { get; set; }
        /// <summary>
        /// 모듈의 디바이스 파라미터를 가져옵니다.
        /// </summary>
        LoaderDeviceParameter DeviceParameter { get; set; }
        List<ISlotModule> PrevSlotInfo { get; set; }
        /// <summary>
        /// 로더의 컨테이너를 가져옵니다.
        /// </summary>
        Autofac.IContainer Container { get; }

        /// <summary>
        /// 스테이지의 컨테이너를 가져옵니다.
        /// </summary>
        Autofac.IContainer StageContainer { get; }

        /// <summary>
        /// 로더 서비스 타입을 가져옵니다.
        /// </summary>
        LoaderServiceTypeEnum ServiceType { get; }

        /// <summary>
        /// IMotionManagerProxy 를 가져옵니다.
        /// </summary>
        IMotionManagerProxy MotionManager { get; }

        /// <summary>
        /// IIOManagerProxy 를 가져옵니다.
        /// </summary>
        IIOManagerProxy IOManager { get; }

        /// <summary>
        /// IVisionManagerProxy 를 가져옵니다.
        /// </summary>
        IVisionManagerProxy VisionManager { get; }

        /// <summary>
        /// Get Pre-aligner Manager
        /// </summary>
        IPAManager PAManager { get; }
        /// <summary>
        /// ILightProxy 를 가져옵니다.
        /// </summary>
        ILightProxy Light { get; }

        /// <summary>
        /// ILoaderMove 를 가져옵니다.
        /// </summary>
        ILoaderMove Move { get; }

        /// <summary>
        /// IModuleManager 를 가져옵니다.
        /// </summary>
        IModuleManager ModuleManager { get; }

        /// <summary>
        /// ILoaderSequencer 를 가져옵니다.
        /// </summary>
        ILoaderSequencer Sequencer { get; }

        /// <summary>
        /// IWaferTransferRemoteService 를 가져옵니다.
        /// </summary>
        IWaferTransferRemoteService WaferTransferRemoteService { get; }


        ICardTransferRemoteService CardTransferRemoteService { get; }
        /// <summary>
        /// IOCRRemoteService 를 가져옵니다.
        /// </summary>
        IOCRRemoteService OCRRemoteService { get; }

        /// <summary>
        /// ILoaderServiceCallback 를 가져옵니다.
        /// </summary>
        ILoaderServiceCallback ServiceCallback { get; }

        ILoaderService LoaderService { get; set; }

        ILoaderSupervisor LoaderMaster { get; set; }
        INotifyManager NotifyManager { get; }
        //IDeviceManager DeviceManager { get;}

        int ChuckNumber { get; set; }
        int ScanCount { get; set; }
        int EmulScanCount { get; set; }
        bool[] ScanFlag { get; set; }

        ObservableCollection<FoupFlag> FoupScanFlag { get; set; }

        bool GetUseLotProcessingVerify();
        long GetBlockingJobtimeforOpenedDoor();
        #region => Init Methods
        /// <summary>
        /// 로더 컨테이너를 설정합니다.
        /// </summary>
        /// <param name="loaderContainer">인스턴스</param>
        void SetLoaderContainer(Autofac.IContainer loaderContainer);

        /// <summary>
        /// 스테이즈 컨테이너를 설정합니다.
        /// </summary>
        /// <param name="stageContainer">인스턴스</param>
        void SetStageContainer(Autofac.IContainer stageContainer);

        /// <summary>
        /// 로더에 연결합니다.
        /// </summary>
        /// <param name="callback">콜백</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Connect(ILoaderServiceCallback callback);

        /// <summary>
        /// 로더에 연결을 해제합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Disconnect();

        /// <summary>
        /// 로더를 초기화 합니다.
        /// </summary>
        /// <param name="serviceType">서비스 타입</param>
        /// <param name="rootParamPath">파라미터 기본 경로</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Initialize(LoaderServiceTypeEnum serviceType, string rootParamPath);

        /// <summary>
        /// 로더를 파괴합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Deinitialize();
        #endregion

        #region => Loader Work Methods
        /// <summary>
        /// 로더의 정보를 가져옵니다.
        /// </summary>
        /// <returns>LoaderInfo</returns>
        LoaderInfo GetLoaderInfo();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns></returns>
        bool SetNoReadScanState(int cassetteNumber);

        /// <summary>
        /// Loader가 현재 Foup에 접근한 상태인지 여부를 가져옵니다.
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns>접근상태면 true, 그렇지 않으면 false</returns>
        bool IsFoupAccessed(int cassetteNumber);

        /// <summary>
        /// Loader System을 초기화합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SystemInit();

        /// <summary>
        /// Loader 명령을 요청합니다.
        /// </summary>
        /// <param name="dstMap">명령 맵</param>
        /// <returns>응답결과</returns>
        ResponseResult SetRequest(LoaderMap dstMap);

        /// <summary>
        /// Loader 모듈을 Awake합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum AwakeProcessModule();

        EventCodeEnum AbortProcessModule();
        /// <summary>
        /// Loader에 요청된 명령맵을 취소합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum AbortRequest();


        /// <summary>
        /// Loader에 요청된 명령을 초기화합니다. 
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ClearRequestData();

        /// <summary>
        /// Loader가 Error상태일때 SelfRecovery명령을 요청합니다.
        /// </summary>
        void SelfRecovery();
        #endregion

        #region => Motion Methods
        /// <summary>
        /// JogRelMove
        /// </summary>
        /// <param name="axis">axis</param>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MOTION_JogRelMove(EnumAxisConstants axis, double value);

        /// <summary>
        /// JogAbsMove
        /// </summary>
        /// <param name="axis">axis</param>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MOTION_JogAbsMove(EnumAxisConstants axis, double value);
        #endregion

        #region => Setting Param Methods
        /// <summary>
        /// Loader의 시스템 파라미터를 갱신합니다.
        /// </summary>
        /// <param name="systemParam">변경된 파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam);
        EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam);
        /// <summary>
        /// Loader의 디바이스 파라미터를 갱신합니다.
        /// </summary>
        /// <param name="deviceParam">변경된 파라미터</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam);
        //EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam);

        EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index);
        EventCodeEnum RetractAll();


        #endregion

        #region => Notify Foup State
        /// <summary>
        /// Loader에 Foup의 상태가 변경되었음을 알립니다.
        /// </summary>
        /// <param name="foupInfo">FOUP Info</param>
        void FOUP_RaiseFoupStateChanged(FoupModuleInfo foupInfo);

        /// <summary>
        /// Loader에 Wafer Out Sensor가 감지되었음을 알립니다.
        /// </summary>
        /// <param name="cassetteNumber">감지된 카세트 번호</param>
        void FOUP_RaiseWaferOutDetected(int cassetteNumber);
        #endregion

        #region => Recovery Methods
        /// <summary>
        /// [Recovery] Loader Motion을 초기화합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum RECOVERY_MotionInit();

        /// <summary>
        /// [Recovery] Loader의 Holder들의 WaferStatus를 다시 설정합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum RECOVERY_ResetWaferLocation();
        #endregion

        #region => SetTestFlag
        void SetTestCenteringFlag(bool centeringtestflag);
        #endregion

        /// <summary>
        /// Loader 정보가 변경되었음을 알립니다.
        /// </summary>
        /// 
        //void BroadcastLoaderInfo();
        void BroadcastLoaderInfo(bool updatebackupdata = false);
        void SetPause();
        void SetResume();

        void SetLoaderMapConvert(ILoaderMapConvert mapConvert);

        void SetTopBar(ILoaderMapConvert mapConvert);

        void SetLoaderMapConvertHandling(ILoaderMapConvert mapConvert);
        void SetHandlingConvert();
        Task<EventCodeEnum> DoScanJob(int cassetteIdx);

        void ResetUnknownModule();

        void AddUnknownModule(IWaferOwnable module);
        EventCodeEnum SetEMGSTOP();
        ObservableCollection<FoupObject> Foups { get; set; }

        string SlotToFoupConvert(ModuleID ID);

        EventCodeEnum ModuleInit(bool IsRefresh = false);
        EventCodeEnum LoaderJobSorting();
        EventCodeEnum SaveLoaderOCRConfig();
        EventCodeEnum LoadLoaderOCRConfig();
        LoaderOCRConfigParam OCRConfig { get; set; }
        EventCodeEnum SetDeviceSize(SubstrateSizeEnum size, int Foupindex);
        CassetteTypeEnum GetCassetteType(int foupindex);
        EventCodeEnum ValidateCassetteTypesConsistency(out string combinedLog);
        EventCodeEnum SetCassetteDeviceSize(SubstrateSizeEnum size, int Foupindex);
        SubstrateSizeEnum GetDefaultWaferSize();
        bool IsFoupUnload(int foupindex);
        EventCodeEnum SetModuleEnable();
        EventCodeEnum SetTransferWaferSize(TransferObject curr, EnumSubsStatus status);
        EventCodeEnum GetModulesSupportingCassetteType(CassetteTypeEnum cassetteType);
        bool ExceedRunningStateDuration();
        Task CloseFoupCoverFunc(ICassetteModule cassetteModule, bool IsManual = false);
        void ClearAlreadyAssignedStages();

        void SetTransferAbort();

        bool LoaderRobotRunning { get; set; }
        bool LoaderRobotAbortFlag { get; set; }
    }

    public enum LoaderMasterMode
    {
        Internal = 0,
        External = 1
    }

    public enum FoupShiftModeEnum
    {
        NORMAL = 0,
        SHIFT = 1
    }

    public enum FoupReservationEnum
    {
        NONE = 0,
        RESERVE = 1, //풉 언로드 예약이 걸린상태
        NOT_PROCESS = 2, //풉 언로드를 바로 못하는상태
        DONE = 3 // 완료된 상태
    }
    public enum GemMode
    {
        Single,
        Cascading,
        Parrallel
    }

    public enum CardChangeStateEnum
    {
        UNDEFINED,
        IDLE,
        TRANSFER_READY,
        WAIT_CMD,
        TRANSFERED,
        TRANSFER,
        CLEAR,
        DONE,
        ABORT,
        ERROR,
        RECOVERY,
    }

    public enum WaferChangeStateEnum
    {
        UNDEFINED,
        IDLE,
        RUNNING,
        CLEAR,
        DONE,
        ABORT,
        ERROR,
        RECOVERY,
    }
    //public class ErrorEndInfo
    //{
    //    #region ==> PropertyChanged
    //    public PropertyChangedEventHandler propertyChanged;

    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (propertyChanged != null)
    //        {
    //            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //        }
    //    }
    //    public event PropertyChangedEventHandler PropertyChanged
    //    {
    //        add { this.propertyChanged += value; }
    //        remove { this.propertyChanged -= value; }
    //    }
    //    #endregion

    //    public ErrorEndInfo()
    //    {

    //    }
    //    public ErrorEndInfo(ErrorEndStateEnum state, int stageNum)
    //    {
    //        _State = state;
    //        _StageNumber = stageNum;
    //    }
    //    private int _StageNumber;
    //    public int StageNumber
    //    {
    //        get { return _StageNumber; }
    //        set
    //        {
    //            if (value != _StageNumber)
    //            {
    //                _StageNumber = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    private ErrorEndStateEnum _State;
    //    public ErrorEndStateEnum State
    //    {
    //        get { return _State; }
    //        set
    //        {
    //            if (value != _State)
    //            {
    //                _State = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //}
    public class LoaderJobViewData : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion

        public LoaderJobViewData()
        {
        }

        public LoaderJobViewData(ModuleID curr, ModuleID dst, ModuleID orign)
        {
            CurrentHolder = curr;
            DstHolder = dst;
            OriginHolder = orign;
            JobDone = false;
        }
        private ModuleID _CurrentHolder;
        public ModuleID CurrentHolder
        {
            get { return _CurrentHolder; }
            set
            {
                if (value != _CurrentHolder)
                {
                    _CurrentHolder = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleID _DstHolder;
        public ModuleID DstHolder
        {
            get { return _DstHolder; }
            set
            {
                if (value != _DstHolder)
                {
                    _DstHolder = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleID _OriginHolder;
        public ModuleID OriginHolder
        {
            get { return _OriginHolder; }
            set
            {
                if (value != _OriginHolder)
                {
                    _OriginHolder = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _JobDone;
        public bool JobDone
        {
            get { return _JobDone; }
            set
            {
                if (value != _JobDone)
                {
                    _JobDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isRunning;
        public bool isRunning
        {
            get { return _isRunning; }
            set
            {
                if (value != _isRunning)
                {
                    _isRunning = value;
                    RaisePropertyChanged();
                }
            }
        }


    }

    public class LoaderJobViewList : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion

        public LoaderJobViewList()
        {
        }

        private ObservableCollection<LoaderJobViewData> _LoaderJobList = new ObservableCollection<LoaderJobViewData>();
        public ObservableCollection<LoaderJobViewData> LoaderJobList
        {
            get { return _LoaderJobList; }
            set
            {
                if (value != _LoaderJobList)
                {
                    _LoaderJobList = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ModuleID _Origin;
        public ModuleID Origin
        {
            get { return _Origin; }
            set
            {
                if (value != _Origin)
                {
                    _Origin = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    public class LoaderProcModuleInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion

        public LoaderProcModuleInfo()
        {
            _ProcModule = LoaderProcModuleEnum.UNDIFINED;

        }
        private LoaderProcModuleEnum _ProcModule;
        public LoaderProcModuleEnum ProcModule
        {
            get { return _ProcModule; }
            set
            {
                if (value != _ProcModule)
                {
                    _ProcModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LoaderProcStateEnum _ProcModuleState;
        public LoaderProcStateEnum ProcModuleState
        {
            get { return _ProcModuleState; }
            set
            {
                if (value != _ProcModuleState)
                {
                    _ProcModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleID _Source;
        public ModuleID Source
        {
            get { return _Source; }
            set
            {
                if (value != _Source)
                {
                    _Source = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ModuleID _Destnation;
        public ModuleID Destnation
        {
            get { return _Destnation; }
            set
            {
                if (value != _Destnation)
                {
                    _Destnation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleID _Origin;
        public ModuleID Origin
        {
            get { return _Origin; }
            set
            {
                if (value != _Origin)
                {
                    _Origin = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public interface IActiveLotInfo
    {
        int FoupNumber { get; set; }
        bool isEnableOCRTable { get; set; }
        //int DoPMICount { get; set; }
        //int PMIEveryInterval { get; set; }
        //List<int> AssignedUsingStageIdxList { get; set; }
        //List<int> UsingStageIdxList { get; set; }
        //List<int> UsingSlotList { get; set; }
        //Dictionary<int, List<int>> UsingStagesBySlot { get; set; }
        //Dictionary<int, int> UnloadFoupBySlot { get; set; }
        //Dictionary<int, string> UsingDeviceNameBySlot { get; set; }
        //List<int> UsingPMIList { get; set; }
        //List<WaferIDInfo> WaferIDInfo { get; set; }
        //Queue<int> RingBuffer { get; set; }
        //List<int> NotDoneSlotList { get; set; }
        string LotID { get; set; }
        int LotPriority { get; set; }
        string DeviceName { get; set; }
        string CST_HashCode { get; set; }
        LotStateEnum State { get; set; }
        DynamicFoupStateEnum DynamicFoupState { get; set; }
        FoupReservationEnum ResevationState { get; set; }
        bool IsFoupEnd { get; set; }
        bool IsActiveFromHost { get; set; }
        int RemainCount { get; set; }
        Dictionary<int, string> CellDeviceInfoDic { get; set; }
        Dictionary<string, string> OriginalDeviceZipName { get; set; }
    }

    public class ActiveLotInfo : INotifyPropertyChanged, IActiveLotInfo
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion
        public ActiveLotInfo(int foupNum, ILoaderSupervisor master)
        {
            FoupNumber = foupNum + 1;
            _LoaderMaster = master;
        }
        private ILoaderSupervisor _LoaderMaster = null;

        private int _FoupNumber;
        public int FoupNumber
        {
            get { return _FoupNumber; }
            set
            {
                if (value != _FoupNumber)
                {
                    _FoupNumber = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _isEnableOCRTable = true;
        public bool isEnableOCRTable
        {
            get { return _isEnableOCRTable; }
            set
            {
                if (value != _isEnableOCRTable)
                {
                    _isEnableOCRTable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int DoPMICount = 0; // testVersion
        public int PMIEveryInterval = 0;
        public List<int> AssignedUsingStageIdxList = new List<int>();
        public List<int> UsingStageIdxList = new List<int>();
        public List<int> UsingSlotList = new List<int>();
        public Dictionary<int, List<int>> UsingStagesBySlot = new Dictionary<int, List<int>>(); //1번쨰 슬롯 넘버, 2번째 Stage넘버들.(다이나믹일경우만)
        public Dictionary<int, int> UnloadFoupBySlot = new Dictionary<int, int>(); //1번쨰 슬롯 넘버, 2번째 Foup Number.(다이나믹일경우만)
        public Dictionary<int, string> UsingDeviceNameBySlot = new Dictionary<int, string>(); //1번쨰 슬롯 넘버, 2번째 DeviceName.(다이나믹일경우만)
        public List<int> UsingPMIList = new List<int>();
        public List<WaferIDInfo> WaferIDInfo = new List<WaferIDInfo>();
        public Queue<int> RingBuffer = new Queue<int>();

        public List<int> NotDoneSlotList = new List<int>();
        private string _LotID = "";
        public string LotID
        {
            get { return _LotID; }
            set
            {
                if (value != _LotID)
                {
                    _LotID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _LotPriority;
        public int LotPriority
        {
            get { return _LotPriority; }
            set
            {
                if (value != _LotPriority)
                {
                    _LotPriority = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _DeviceName;
        public string DeviceName
        {
            get { return _DeviceName; }
            set
            {
                if (value != _DeviceName)
                {
                    _DeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CST_HashCode = "";
        public string CST_HashCode
        {
            get { return _CST_HashCode; }
            set
            {
                if (value != _CST_HashCode)
                {
                    _CST_HashCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LotStateEnum _State;
        public LotStateEnum State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    LoggerManager.Debug($"[ActiveLotInfo] State Changed to {_State} => {value}, Foup{FoupNumber}, LotId:{LotID}, CST_HashCode:{CST_HashCode}");
                    _State = value;
                    RaisePropertyChanged();
                }
            }
        }
        private DynamicFoupStateEnum _DynamicFoupState;
        public DynamicFoupStateEnum DynamicFoupState
        {
            get { return _DynamicFoupState; }
            set
            {
                if (value != _DynamicFoupState)
                {
                    _DynamicFoupState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private FoupReservationEnum _ResevationState;
        public FoupReservationEnum ResevationState
        {
            get { return _ResevationState; }
            set
            {
                if (value != _ResevationState)
                {
                    _ResevationState = value;
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        //랏드 엔드시 Foup 언로드 중복을 막기 위해서 필요한 변수
        /// </summary>
        private bool _IsFoupEnd;
        public bool IsFoupEnd
        {
            get { return _IsFoupEnd; }
            set
            {
                if (value != _IsFoupEnd)
                {
                    _IsFoupEnd = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Foup unload 를 시도했다는 변수
        /// </summary>
        private bool _AttemptedFoupUnload = false;
        public bool AttemptedFoupUnload
        {
            get { return _AttemptedFoupUnload; }
            set
            {
                if (value != _AttemptedFoupUnload)
                {
                    _AttemptedFoupUnload = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsActiveFormHost = false;
        /// <summary>
        /// Host 로부터 Active 되어 Foup 이 동작하는 것인지, 아닌지를 알기 위해.
        /// true : Host 로 부터 PROCEED WITH CARREIR 를 받음
        /// fale : Manual 동작중 , Lot 가 종료되고, Carrier 가 Unload 되었을때.
        /// </summary>
        public bool IsActiveFromHost
        {
            get { return _IsActiveFormHost; }
            set
            {
                if (value != _IsActiveFormHost)
                {
                    _IsActiveFormHost = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LotAssignStateEnum _AssignState = LotAssignStateEnum.UNASSIGNED;
        /// <summary> 
        /// Host 를 통한 LOT 할당 상태
        /// UNDEFIEND : 초기값 (LOT 이후에 Cassette 가 없어진 상태)
        /// NONE : LOT 가 완료 된 상태 (LOT 는 완료되었지만 Cassette 가 아직 있는 상태)
        /// ASSIGNED : LOT가 할당 된 상태 
        /// CANCEL : LOT 가 취소 된 상태 
        /// POSTPONED : LOT 가 연기된 상태 (Loader 가 수동조작 되고 있는 경우 대기 상태)
        /// </summary>
        public LotAssignStateEnum AssignState
        {
            get { return _AssignState; }
            private set
            {
                if (value != _AssignState)
                {
                    _AssignState = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private List<int> _LotOutStageIndexList = new List<int>();
        ///// <summary>
        ///// Pabort 받은 셀넘버
        ///// </summary>
        //public List<int> LotOutStageIndexList
        //{
        //    get { return _LotOutStageIndexList; }
        //    set
        //    {
        //        if (value != _LotOutStageIndexList)
        //        {
        //            LoggerManager.Debug($"[ActiveLotInfo] LotOutStageIndexList Changed to {string.Join(",", _LotOutStageIndexList)}=>{string.Join(",", value)} , Foup{FoupNumber}, LotId:{LotID}, CST_HashCode:{CST_HashCode}");
        //            _LotOutStageIndexList = value;                                       
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private int _RemainCount = 0;

        public int RemainCount
        {
            get { return _RemainCount; }
            set { _RemainCount = value; }
        }

        private Dictionary<int, string> _CellDeviceInfoDic = new Dictionary<int, string>();

        public Dictionary<int, string> CellDeviceInfoDic
        {
            get { return _CellDeviceInfoDic; }
            set { _CellDeviceInfoDic = value; }
        }

        private Dictionary<string, string> _OriginalDeviceZipNameDic = new Dictionary<string, string>();

        public Dictionary<string, string> OriginalDeviceZipName
        {
            get { return _OriginalDeviceZipNameDic; }
            set { _OriginalDeviceZipNameDic = value; }
        }

        private Dictionary<int, NeedChangeParameterInDevice> _CellSetParamOfDeviceDic = new Dictionary<int, NeedChangeParameterInDevice>();

        public Dictionary<int, NeedChangeParameterInDevice> CellSetParamOfDeviceDic
        {
            get { return _CellSetParamOfDeviceDic; }
            set { _CellSetParamOfDeviceDic = value; }
        }

        private Dictionary<string, string> _RCMDErrorCheckDic = new Dictionary<string, string>();
        #region [STM_CATANIA] Error Check Dictionary
        /// <summary>
        /// Host로 부터 RCMD를 통해 전달받은 정보들에 대해서 매번 Error Check를 하기 위해 필요함.
        /// </summary>
        #endregion
        public Dictionary<string, string> RCMDErrorCheckDic
        {
            get { return _RCMDErrorCheckDic; }
            set { _RCMDErrorCheckDic = value; }
        }

        private bool _IsManaulEnd;

        public bool IsManaulEnd
        {
            get 
            {                
                return _IsManaulEnd; 
            }
            set 
            {
                if(_IsManaulEnd != value)
                {
                    LoggerManager.Debug($"[ActiveLotInfo] IsManaulEnd Changed to {_IsManaulEnd} => {value}, Foup{FoupNumber}, LotId:{LotID}, CST_HashCode:{CST_HashCode}");
                    _IsManaulEnd = value;
                }                
            }
        }


        public void DI_LOAD_SWITCH_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                var val = _LoaderMaster.FoupIOManager.IOMap.Inputs.DI_FOUP_LOAD_BUTTONs[FoupNumber - 1].Value;
                if (val == true)
                {
                    if (this.State == LotStateEnum.Idle || this.State == LotStateEnum.Done)
                    {
                        if (_LoaderMaster.Loader.Foups[FoupNumber - 1].State != FoupStateEnum.LOAD)
                        {
                            var GPLoader = _LoaderMaster.GetGPLoader();

                            if (GPLoader != null && GPLoader.IsLoaderBusy)
                            {
                                _LoaderMaster.MetroDialogManager().ShowMessageDialog("Loader Busy", "It cannot be load.", EnumMessageStyle.Affirmative);
                            }
                            else
                            {
                                // E84 가 연결되어있고 Auto Mode 인 경우에는 Manual Load/Unload 를 동작하지 못하게 한다.
                                var e84controller = _LoaderMaster.E84Module().GetE84Controller(FoupNumber, E84OPModuleTypeEnum.FOUP);

                                if (e84controller != null)
                                {
                                    if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                                    {
                                        if (e84controller.CommModule.RunMode == E84Mode.AUTO)
                                        {
                                            _LoaderMaster.MetroDialogManager().ShowMessageDialog("Error Message", "Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.", EnumMessageStyle.Affirmative);
                                            LoggerManager.Debug("Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.");
                                            return;
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"Load() Foup#{FoupNumber}. e84controller runmode is {e84controller.CommModule.RunMode}.");
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"Load() Foup#{FoupNumber}. e84controller connection state is {e84controller.CommModule.Connection}");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"Load() Foup#{FoupNumber}. e84controller is null.");
                                }

                                FoupLoad();

                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"Load() Switch Foup#{FoupNumber}. Error. Foup State:{_LoaderMaster.Loader.Foups[FoupNumber - 1].State }");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"Load() Switch Foup#{FoupNumber}. Error. LOT State:{State}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum FoupLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this._LoaderMaster.FoupOpModule().FoupControllers[FoupNumber - 1].ValidationCassetteAvailable(out string msg) == EventCodeEnum.NONE)
                {
                    retVal = this._LoaderMaster.FoupOpModule().FoupControllers[FoupNumber - 1].Execute(new FoupLoadCommand());
                    this._LoaderMaster.Loader.BroadcastLoaderInfo();
                }
                else
                {
                    _LoaderMaster.MetroDialogManager().ShowMessageDialog("Foup Load Error", $"Foup#{FoupNumber} {msg}." +
                        $"\nNo CST was detected for the specified CST type." +
                        $"\nPlease reload this foup.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void DI_UNLOAD_SWITCH_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                var val = _LoaderMaster.FoupIOManager.IOMap.Inputs.DI_FOUP_UNLOAD_BUTTONs[FoupNumber - 1].Value;
                if (val == true)
                {
                    if (this.State == LotStateEnum.Idle || this.State == LotStateEnum.Done)
                    {
                        if (_LoaderMaster.Loader.Foups[FoupNumber - 1].State != FoupStateEnum.UNLOAD)
                        {
                            var GPLoader = _LoaderMaster.GetGPLoader();

                            if (GPLoader != null && GPLoader.IsLoaderBusy)
                            {
                                _LoaderMaster.MetroDialogManager().ShowMessageDialog("Loader Busy", "It cannot be Unload.", EnumMessageStyle.Affirmative);
                            }
                            else
                            {
                                // E84 가 연결되어있고 Auto Mode 인 경우에는 Manual Load/Unload 를 동작하지 못하게 한다.
                                var e84controller = _LoaderMaster.E84Module().GetE84Controller(FoupNumber, E84OPModuleTypeEnum.FOUP);

                                if (e84controller != null)
                                {
                                    if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                                    {
                                        if (e84controller.CommModule.RunMode == E84Mode.AUTO)
                                        {
                                            _LoaderMaster.MetroDialogManager().ShowMessageDialog("Error Message", "Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.", EnumMessageStyle.Affirmative);

                                            return;
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"Unload() Foup#{FoupNumber}. e84controller runmode is {e84controller.CommModule.RunMode}.");
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"Unload() Foup#{FoupNumber}. e84controller connection state is {e84controller.CommModule.Connection}");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"Unload() Foup#{FoupNumber}. e84controller is null.");
                                }

                                FoupUnLoad();
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"Unload() Switch Foup#{FoupNumber}. Error. Foup State:{_LoaderMaster.Loader.Foups[FoupNumber - 1].State }");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"Unload() Switch Foup#{FoupNumber}. Error. LOT State:{State}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum FoupUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"ActiveLotInfo.FoupUnload({FoupNumber}): Start, LoaderMaster.ModuleState.State:{_LoaderMaster.ModuleState.State}");
                if (_LoaderMaster.ModuleState.State != ModuleStateEnum.RUNNING)
                {
                    if (_LoaderMaster.DynamicMode == DynamicModeEnum.DYNAMIC)
                    {
                        if (DynamicFoupState == DynamicFoupStateEnum.LOAD_AND_UNLOAD)
                        {
                            if (_LoaderMaster.ModuleState.State == ModuleStateEnum.IDLE ||
                                _LoaderMaster.ModuleState.State == ModuleStateEnum.ABORT ||
                                _LoaderMaster.ModuleState.State == ModuleStateEnum.DONE)
                            {
                                retVal = this._LoaderMaster.FoupOpModule().FoupControllers[FoupNumber - 1].Execute(new FoupUnloadCommand());
                                this._LoaderMaster.Loader.BroadcastLoaderInfo();
                                if (retVal != EventCodeEnum.NONE)
                                    return retVal;                                
                            }
                            else
                            {
                                LoggerManager.Debug($"ActiveLotInfo.FoupUnload({FoupNumber}): Rejected cause of LoaderMaster.ModuleStaet:{_LoaderMaster.ModuleState.State}");
                                retVal = EventCodeEnum.CASSETTE_UNDOCK_COMMAND_REJECTED;
                                return retVal;//경고 메세지 
                            }

                        }
                        else
                        {
                            retVal = this._LoaderMaster.FoupOpModule().FoupControllers[FoupNumber - 1].Execute(new FoupUnloadCommand());
                            this._LoaderMaster.Loader.BroadcastLoaderInfo();
                            if (retVal != EventCodeEnum.NONE)
                                return retVal;
                        }
                    }
                    else
                    {
                        retVal = this._LoaderMaster.FoupOpModule().FoupControllers[FoupNumber - 1].Execute(new FoupUnloadCommand());
                        this._LoaderMaster.Loader.BroadcastLoaderInfo();
                        if (retVal != EventCodeEnum.NONE)
                            return retVal;
                    }
                }
                else
                {

                    if (_LoaderMaster.GetFoupShiftMode() == FoupShiftModeEnum.SHIFT)
                    {
                        //FoupShiftMode에서 FoupUnload 가능한 
                        //현재 cst가 비었을 경우 
                        //현재 cst가 비지 않았음. 
                        // unprocessed가 아닌 wafer가 있는데 
                        // 그 wafer의 pair wafer가 모두 cst에 있을때 

                        var wafersInCst = _LoaderMaster.Loader.GetLoaderInfo().StateMap.GetTransferObjectAll().Where(w => ((w.CurrHolder.Index - 1) / 25) + 1 == FoupNumber &&
                                                                                                               w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT);
                        var cst_hashcodesInCst = wafersInCst.Select(w => w.CST_HashCode).Distinct();



                        bool canUnload = false;
                        ActiveLotInfo prev_lotinfo = null;
                        // 현재 카세트에 있는 pre_lotinfo 정보를 가지고 오자
                        foreach (var prev in _LoaderMaster.Prev_ActiveLotInfos)
                        {
                            if (cst_hashcodesInCst.Contains(prev.CST_HashCode))
                            {
                                prev_lotinfo = prev;
                            }
                        }


                        // 현재 lot와 prev lot에 할당된 wafer만 검사할것. 현재 폽에 있지 않은 웨이퍼도 가지고옴. 
                        IEnumerable<TransferObject> allocatedallWafers = new List<TransferObject>();
                        if (prev_lotinfo != null)
                        {
                            allocatedallWafers = _LoaderMaster.Loader.ModuleManager.GetTransferObjectAll().Where(w => UsingSlotList.Contains((w.OriginHolder.Index % 25 == 0) ? 25 : w.OriginHolder.Index % 25) ||
                                                                                                                      w.CST_HashCode == prev_lotinfo.CST_HashCode);
                        }
                        else
                        {
                            allocatedallWafers = _LoaderMaster.Loader.ModuleManager.GetTransferObjectAll().Where(w => UsingSlotList.Contains((w.OriginHolder.Index % 25 == 0) ? 25 : w.OriginHolder.Index % 25));
                        }

                        var allocWafersInCst = allocatedallWafers.Where(w => w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                                      (w.CurrHolder.Index - 1) / 25 + 1 == FoupNumber
                                                                      );


                        //현재 cst가 비었을 경우 (현재 lot와 prev lot 에 할당된 wafer가 모두 0개)                       
                        if (allocWafersInCst.Count() == 0)
                        {
                            LoggerManager.Debug($"FoupUnLoad({FoupNumber}): Success, wafersInCst.Count = 0");
                            canUnload = true;
                        }
                        else
                        {
                            //현재 cst가 비지 않았음. 
                            //현재 Lot 또는 Prev_Lot에 할당된 wafer가 있는데 
                            //Unprocessed인 wafer가 0개여야한다. 
                            //카세트에 있는 Unprocessed가 아닌 wafer의 pair wafer는 모두 카세트에 있어야함. 
                            var notUnprocessedWafers = allocatedallWafers.Where(w => w.WaferState != EnumWaferState.UNPROCESSED);
                            var notUnprocessedInCst = notUnprocessedWafers.Where(w => w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT);
                            var UnprocessedInCst = allocWafersInCst.Where(w => w.WaferState == EnumWaferState.UNPROCESSED);
                            //var UnprocessedWafer_Count = allocWafersInCst.Count() - notUnprocessedWafers.Count();

                            if (UnprocessedInCst.Count() == 0)
                            {
                                var some_wafer = notUnprocessedInCst.FirstOrDefault();
                                var pair_waferInCst = allocatedallWafers.Where(w => w.CST_HashCode == some_wafer.CST_HashCode &&
                                                                                    w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                                                    (w.OriginHolder.Index - 1) / 25 + 1 == FoupNumber
                                                                                    );

                                if (pair_waferInCst.Count() == notUnprocessedInCst.Count())
                                {
                                    LoggerManager.Debug($"FoupUnLoad({FoupNumber}): Success, CST_HashCode:{some_wafer}, pair_waferInCst == notUnprocessedWafers");
                                    canUnload = true;
                                }
                                else
                                {
                                    canUnload = false;
                                    LoggerManager.Debug($"FoupUnLoad({FoupNumber}):CASSETTE_UNDOCK_COMMAND_REJECTED, pair_waferInCst:{pair_waferInCst}, notUnprocessedWafers:{notUnprocessedWafers}");
                                }
                            }
                            else
                            {
                                canUnload = false;
                                LoggerManager.Debug($"FoupUnLoad({FoupNumber}):CASSETTE_UNDOCK_COMMAND_REJECTED, UnprocessedWafer_Count:{UnprocessedInCst.Count() }");
                            }

                        }

                        if (State == LotStateEnum.Cancel)
                        {
                            canUnload = true;
                            LoggerManager.Debug($"FoupUnLoad({FoupNumber}): Success, ActiveLot State is {State}");
                        }

                        // 최종 판단
                        if (canUnload)
                        {
                            retVal = Forced_FoupUnLoad();//Execute(new FoupUnloadCommand()) 하면 안됨.  
                            this._LoaderMaster.Loader.BroadcastLoaderInfo();
                        }
                        else
                        {
                            retVal = EventCodeEnum.CASSETTE_UNDOCK_COMMAND_REJECTED;//EAP 에 Fail로 올려줘야함.
                        }
                    }
                    else
                    {
                        if (_LoaderMaster.DynamicMode == DynamicModeEnum.DYNAMIC)
                        {
                            this.ResevationState = FoupReservationEnum.RESERVE;
                        }
                        else
                        {
                            var Cassette = _LoaderMaster.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, _LoaderMaster.ActiveLotInfos[FoupNumber - 1].FoupNumber);
                            if (Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD && (_LoaderMaster.ActiveLotInfos[FoupNumber - 1].State == LotStateEnum.End || _LoaderMaster.ActiveLotInfos[FoupNumber - 1].State == LotStateEnum.Done))
                            {
                                retVal = this._LoaderMaster.FoupOpModule().FoupControllers[FoupNumber - 1].Execute(new FoupUnloadCommand());
                                if (retVal != EventCodeEnum.NONE)
                                    return retVal;
                            }
                            else
                            {
                                // Normal Lot에서 Lot 안끝나서 Foup Unload 안돼야 하는 부분
                                LoggerManager.Debug($"FoupUnLoad({FoupNumber}):CASSETTE_UNDOCK_COMMAND_REJECTED, Cassette.FoupState:{Cassette.FoupState}, ActiveLotInfos.State:{_LoaderMaster.ActiveLotInfos[FoupNumber - 1].State}");
                                retVal = EventCodeEnum.CASSETTE_UNDOCK_COMMAND_REJECTED;
                                return retVal;
                            }
                        }
                    }
                }

                retVal = EventCodeEnum.NONE;        // 위에서 error 가 발생해도 none 으로 뱉어버린다. 이게 맞는가 ?
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                LoggerManager.Debug($"ActiveLotInfo.FoupUnload({FoupNumber}): End, LoaderMaster.ModuleState.State:{_LoaderMaster.ModuleState.State}");
            }
            return retVal;
        }

        public EventCodeEnum Forced_FoupUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                var canRemovePrev = CanRemovePrevLotInfo();

                this._LoaderMaster.FoupOpModule().FoupControllers[FoupNumber - 1].SetLock(false);
                retVal = this._LoaderMaster.FoupOpModule().FoupControllers[FoupNumber - 1].Execute(new FoupUnloadCommand());
                this._LoaderMaster.Loader.BroadcastLoaderInfo();
                this.ResevationState = FoupReservationEnum.DONE;
                if (retVal == EventCodeEnum.NONE)
                {
                    if (State == LotStateEnum.Running)
                    {
                        var ActiveLot = new ActiveLotInfo(FoupNumber, _LoaderMaster);
                        ActiveLot.CST_HashCode = CST_HashCode;
                        ActiveLot.State = State;
                        ActiveLot.UsingSlotList = UsingSlotList.ToList();
                        ActiveLot.UsingStageIdxList = UsingStageIdxList.ToList();
                        ActiveLot.LotPriority = LotPriority;
                        ActiveLot.DeviceName = DeviceName;
                        ActiveLot.DynamicFoupState = DynamicFoupState;
                        ActiveLot.FoupNumber = FoupNumber;
                        ActiveLot.isEnableOCRTable = isEnableOCRTable;
                        ActiveLot.LotID = LotID;
                        ActiveLot.UsingPMIList = UsingPMIList.ToList();
                        ActiveLot.UsingStagesBySlot = new Dictionary<int, List<int>>(UsingStagesBySlot);
                        ActiveLot.UnloadFoupBySlot = new Dictionary<int, int>(UnloadFoupBySlot);
                        ActiveLot.UsingDeviceNameBySlot = new Dictionary<int, string>(UsingDeviceNameBySlot);

                        ActiveLot.NotDoneSlotList = NotDoneSlotList.ToList();
                        ActiveLot.CellDeviceInfoDic = new Dictionary<int, string>(CellDeviceInfoDic);
                        ActiveLot.OriginalDeviceZipName = new Dictionary<string, string>(OriginalDeviceZipName);


                        lock (_LoaderMaster.Prev_ActiveLotInfos)
                        {
                            _LoaderMaster.Prev_ActiveLotInfos.Add(ActiveLot);

                            // 완료된 Prev_LotInfo가 있다면 제거
                            // 현재 이 cst에 있는 웨이퍼의 HashCode가 이 장비내에 없다면 Remove
                            if (canRemovePrev.CanRemove)
                            {
                                _LoaderMaster.Prev_ActiveLotInfos.Remove(canRemovePrev.TargetInfo); //Unload완료된후에 해줘야함.
                                LoggerManager.Debug($"Forced_FoupUnLoad(): Removed Prev_ActiveLotInfos, Hash:{canRemovePrev.TargetInfo.CST_HashCode} ");
                            }
                        }

                    }
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    //#Hynix_Merge: ActiveLotInfo Clear 중복기능
                    State = LotStateEnum.Idle;
                    AssignState = LotAssignStateEnum.JOB_FINISHED;
                    //IsReserveCancel = false;
                    UsingSlotList = new List<int>();
                    UsingStageIdxList = new List<int>();
                    LotID = "";
                    UsingStagesBySlot = new Dictionary<int, List<int>>();

                    _LoaderMaster.Loader.Foups[FoupNumber - 1].LotState = LotStateEnum.Idle;
                    _LoaderMaster.Loader.Foups[FoupNumber - 1].AllocatedCellInfo = "";
                    LotPriority = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private (bool CanRemove, ActiveLotInfo TargetInfo) CanRemovePrevLotInfo()
        {
            bool canRemove = false;
            ActiveLotInfo targetInfo = null;

            if (_LoaderMaster.GetFoupShiftMode() != FoupShiftModeEnum.SHIFT)
            {
                return (canRemove, targetInfo);
            }

            var wafersInCst = _LoaderMaster.Loader.GetLoaderInfo().StateMap.GetTransferObjectAll().Where(w => ((w.CurrHolder.Index - 1) / 25) + 1 == FoupNumber &&
                                                                                                               w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT);
            var cst_hashcodesInCst = wafersInCst.Select(w => w.CST_HashCode).Distinct();

            var wafersNotInCst = _LoaderMaster.Loader.GetLoaderInfo().StateMap.GetTransferObjectAll().Where(w => w.CurrHolder.ModuleType != ModuleTypeEnum.SLOT);
            var cst_hashcodesNotInCst = wafersNotInCst.Select(w => w.CST_HashCode).Distinct();

            foreach (var cst_hash in cst_hashcodesInCst)
            {
                //만약 장비내에 없다면 해당 cst_hash가 Prev에 있으면 제거
                if (cst_hashcodesNotInCst.Contains(cst_hash) == false)
                {
                    var isWaferExistInLoader = _LoaderMaster.Prev_ActiveLotInfos.Where(l => l.CST_HashCode == cst_hash);
                    if (isWaferExistInLoader.Count() > 0)
                    {
                        //_LoaderMaster.Prev_ActiveLotInfos.Remove(isWaferExistInLoader.FirstOrDefault()); //Unload완료된후에 해줘야함.
                        canRemove = true;
                        targetInfo = isWaferExistInLoader.FirstOrDefault();

                        continue;
                    }

                }

            }

            return (canRemove, targetInfo);
        }

        public void SetAssignLotState(LotAssignStateEnum assignStateEnum)
        {
            try
            {
                LotAssignStateEnum preState = AssignState;
                AssignState = assignStateEnum;
                LoggerManager.Debug($"[LOT INFO] Foup Index : {FoupNumber}, LOT ID : {LotID}, Pre Assign State : {preState}, Cur Assign State : {AssignState}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ClearLotInfo()
        {
            try
            {
                IsActiveFromHost = false;
                isEnableOCRTable = false;
                UsingPMIList.Clear();
                UsingStagesBySlot.Clear();
                UnloadFoupBySlot.Clear();
                UsingDeviceNameBySlot.Clear();
                NotDoneSlotList.Clear();
                CellDeviceInfoDic.Clear();
                OriginalDeviceZipName.Clear();
                LotPriority = 0;
                LotID = "";
                UsingStageIdxList.Clear();
                AssignedUsingStageIdxList.Clear();
                CST_HashCode = "";
                RingBuffer.Clear();
                IsFoupEnd = false;
                RemainCount = 0;
                State = LotStateEnum.Idle;
                SetAssignLotState(LotAssignStateEnum.UNASSIGNED);

                LoggerManager.Debug($"ActiveLotInfo FoupNumber : {FoupNumber} ClearLotInfo().");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class AbortStageInformation : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion

        private int _StageNumber;
        public int StageNumber
        {
            get { return _StageNumber; }
            set
            {
                if (value != _StageNumber)
                {
                    _StageNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCanReAssignLot;
        /// <summary>
        /// LOT 에 다시 합류할 수 있는지에 대한 여부
        /// ( 시나리오에 따라서 Abort 후 LOT 에 다시 합류할 수 도 없어야 할 수 도 있어서)
        /// </summary>
        public bool IsCanReAssignLot
        {
            get { return _IsCanReAssignLot; }
            set
            {
                if (value != _IsCanReAssignLot)
                {
                    _IsCanReAssignLot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsReAssignTrigger = false;
        /// <summary>
        /// 다시 LOT 에 합류하라는 트리거가 발생했는지 확인하기 위한 속성.
        /// True인 경우에는 LOT 에 합류 ( Using Stage Idx List 에 추가해야함 )
        /// </summary>
        public bool IsReAssignTrigger
        {
            get { return _IsReAssignTrigger; }
            set
            {
                if (value != _IsReAssignTrigger)
                {
                    _IsReAssignTrigger = value;
                    RaisePropertyChanged();
                }
            }
        }


        private List<LotInfoPack> _LotBannedList = new List<LotInfoPack>();
        /// <summary>
        /// Stage 가 할당된 LOT 정보를 관리하려는 속성 
        /// </summary>
        public List<LotInfoPack> LotBannedList
        {
            get { return _LotBannedList; }
            set
            {
                if (value != _LotBannedList)
                {
                    _LotBannedList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsRemoved = false;
        /// <summary>
        /// RemoveUsingStage 함수에서 동작완료 여부 ( LOT ACtive Info 의 Stage List 에서 제거 하는 등)
        /// </summary>
        public bool IsRemoved
        {
            get { return _IsRemoved; }
            set
            {
                if (value != _IsRemoved)
                {
                    _IsRemoved = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

    public class WaferIDInfo
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion
        public WaferIDInfo(int foupIdx, int slotIdx, string id)
        {
            FoupIndex = foupIdx;
            SlotIndex = slotIdx;
            WaferID = id;
        }
        private int _SlotIndex;
        public int SlotIndex
        {
            get { return _SlotIndex; }
            set
            {
                if (value != _SlotIndex)
                {
                    _SlotIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _WaferID;
        public string WaferID
        {
            get { return _WaferID; }
            set
            {
                if (value != _WaferID)
                {
                    _WaferID = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    public interface ILoaderSupervisor : IFactoryModule, IStateModule, IHasSysParameterizable
    {
        ILoaderModule Loader { get; }
        LoaderMasterMode Mode { get; }
        bool IsSelectedLoader { get; set; }
        List<ModuleStateEnum> StageStates { get; }
        List<bool> StagesIsTempReady { get; }
        List<double> StageSetTemp { get; }
        ModuleStateEnum StageSetSoakingState { get; }
        //List<ErrorEndInfo> ErrorEndList { get; set; }
        List<ActiveLotInfo> ActiveLotInfos { get; }
        bool HostInitiatedWaferChangeInProgress { get; set; }
        List<ActiveLotInfo> Prev_ActiveLotInfos { get; set; }
        List<ActiveLotInfo> BackupActiveLotInfos { get; }
        List<AbortStageInformation> LotAbortStageInfos { get; set; }
        ActiveLotInfo SelectedLotInfo { get; }
        IFoupIOStates FoupIOManager { get; }
        Dictionary<string, ILoaderServiceCallback> ClientList { get; }
        Dictionary<int, object> ClientListLock { get; }
        ILoaderServiceCallback GetClient(int chuckIndex, bool bCalledDisconnect = false);
        void RemoveClientAtList(int chuckIndex);
        LoaderMap RequestJob();
        bool IsAliveClient(ILoaderServiceCallback client, int waittime = 15);
        EventCodeEnum Initialize(Autofac.IContainer container);
        void LotOPStart();
        void SetMapSlicerLotPause(bool val);
        EventCodeEnum Connect(string sessionId, ILoaderServiceCallback loaderController);
        new void DeInitModule();
        void ArrangeCallbackIndex();
        void SetStageState(int idx, ModuleStateEnum stageState, bool isBuzzerOn);
        ModuleStateEnum CurrentModuleState { get; set; }
        //void RemoveUsingStage(ActiveLotInfo lotinfo);
        bool IsSameDeviceEndToSlot(string deviceName, List<int> usingStageList, bool dontCareUnprocessedWafer = false);
        bool IsAllStageOut(ActiveLotInfo lotinfo);
        void SetSkipUnprocessedWafer(LoaderMap map, ActiveLotInfo lotinfo);
        EventCodeEnum ConfirmWaferArrivalInFoup(LoaderMap Map, ActiveLotInfo activelotinfo);
        EventCodeEnum ResponseSystemInit(EventCodeEnum errorCode);
        DynamicModeEnum DynamicMode { get; set; }
        //Element<FoupShiftModeEnum> FoupShiftMode { get; }
        FoupShiftModeEnum GetFoupShiftMode();
        void SetFoupShiftMode(int mode);
        EventCodeEnum ResponseCardRecovery(EventCodeEnum errorCode);

        bool OCRDebugginFlag { get; set; }
        bool ContinueLotFlag { get; set; }
        bool IsSuperUser { get; set; }
        string CardIDLastTwoWord { get; set; }
        string CardIDFullWord { get; set; }
        void ChangeExternalMode();
        bool IsAbortError { get; set; }

        bool IsLoaderJobDoneWait { get; set; }

        bool StatusSoakingUpdateInfoStop { get; set; }
        bool StageWatchDogStop { get; set; }

        ObservableCollection<IStageObject> CellsInfo { get; }
        void ChangeInternalMode();

        EventCodeEnum ActiveProcess(ActiveProcessActReqData actReqData);
        EventCodeEnum SelectSlot(int foupNumber, string lotID, List<int> slots);
        EventCodeEnum VerifyParam(int foupNumber, string lotID);
        EventCodeEnum Select_Slot_Stages(int foupNumber, string lotID, Dictionary<int, List<int>> usingStageBySlot, List<SlotCellInfo> slotCellInfos = null);

        EventCodeEnum ExternalLotOPStart(int foupNum, string lotID);

        EventCodeEnum ExternalTCW_Start(int foupNum, string lotID);
        bool ExtenalIsLotEndReady();

        EventCodeEnum SetRecipeToDevice(DownloadStageRecipeActReqData data);

        EventCodeEnum OnLoaderParameterChangedClient(ILoaderServiceCallback client, LoaderSystemParameter systemParam, LoaderDeviceParameter deviceParam);
        EventCodeEnum OnLoaderInfoChangedClient(ILoaderServiceCallback client, LoaderInfo info);
        EventCodeEnum CSTInfoChangedClient(ILoaderServiceCallback client, LoaderInfo info);
        EventCodeEnum WaferIDChangedClient(ILoaderServiceCallback client, int slotNum, string ID);

        EventCodeEnum WaferHolderChangedClient(ILoaderServiceCallback client, int slotNum, string holder);

        EventCodeEnum WaferStateChangedClient(ILoaderServiceCallback client, int slotNum, EnumSubsStatus waferStatus, EnumWaferState waferState);

        EventCodeEnum WaferSwapChangedClient(ILoaderServiceCallback client, int originSlotNum, int changeSlotNum, bool isInit);
        EnumSubsStatus GetChuckWaferStatusClient(ILoaderServiceCallback client);
        EnumSubsStatus GetCardStatusClient(ILoaderServiceCallback client, out EnumWaferState cardState);
        ModuleID GetChuckIDClient(ILoaderServiceCallback client);
        LoaderMap RequestJobClient(ILoaderServiceCallback client, LoaderInfo loaderInfo, out bool isExchange, out bool isNeedWafer, out bool isTempReady, out string cstHashCodeOfRequestLot, bool canloadwafer = true);
        bool LotOPStartClient(ILoaderServiceCallback client, bool iscellstart = false, string lotID = "",int foupnumber = -1, string cstHashCodeOfRequestLot = "");
        bool LotOPEndReadyClient(ILoaderServiceCallback client);
        bool LotOPPauseClient(ILoaderServiceCallback client, bool isabort = false, IStageObject stageobj = null);
        bool LotOPResumeClient(ILoaderServiceCallback client);
        bool LotOPEndClient(ILoaderServiceCallback client, int foupnumber = -1, bool isabortlot = false);
        ModuleStateEnum GetLotStateClient(ILoaderServiceCallback client);

        TransferObject GetDeviceInfoClient(ILoaderServiceCallback client);
        void DisConnectClient(ILoaderServiceCallback client);

        EventCodeEnum UsingPMICalc(ISlotModule slot);
        StageLotData GetStageLotData(int stageIndex);
        EventCodeEnum SetStageMoveLockState(int stageIndex, ReasonOfStageMoveLock reason);
        EventCodeEnum SetStageMoveUnLockState(int stageIndex, ReasonOfStageMoveLock reason);
        List<ReasonOfStageMoveLock> GetReasonofLockFromClient(int stageIdx);

        bool GetTesterAvailableData(int stageIndex);

        EventCodeEnum SetStageMode(int stageIndex, GPCellModeEnum mode);
        void SetCellModeChanging(int Cell_Idx);
        void ResetCellModeChanging(int Cell_Idx);
        void StageSoakingMode();
        void SetStreamingMode(int stageIndex, StreamingModeEnum mode);
        EventCodeEnum NotifyStageSystemError(int cellindex);
        EventCodeEnum NotifyClearStageSystemError(int cellindex);
        EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo);
        void SetTitleMessage(int cellno, string message, string foreground = "", string background = "");
        bool IsLoaderEMOActive();
        bool IsLoaderMainAirDown();
        bool IsLoaderMainVacDown();
        bool GetIsWaitForWaferIdConfirm();
        int GetWaitForWaferIdConfirmTimeout();
        EventCodeEnum CarrierCancel(int foupIndex);
        EventCodeEnum SetWaferIDs(AssignWaferIDMap waferIdData);
        Task<(EventCodeEnum, string)> CollectAllWafer();
        bool IsCardUpmoduleUp();
        EventCodeEnum ErrorEndCell(int cellIdx, bool canUnloadWhilePaused = false);
        EventCodeEnum GetLoaderDoorStatus(out bool leftdoor, out bool rightdoor);
        EventCodeEnum CellWaferRefresh(int cellIdx);
        EventCodeEnum CellCardRefresh(int cellIdx);

        void SetProbingStart(int cellIdx, bool isStart);
        void SetTransferError(int cellIdx, bool isError);
        void SetActionLogMessage(string message, int idx, ModuleLogType ModuleType, StateLogType State);
        void SetParamLogMessage(string message, int idx);
        EventCodeEnum NotifyLotEndToCell(int foupNumber, string lotID);
        EventCodeEnum SetAngleInfo(int chuckIndex, TransferObject wafer);
        EventCodeEnum SetNotchType(int chuckIndex, TransferObject wafer);
        string GetPreDefindWaferId(int foupNum, int slotIndex);

        bool WaferIdConfirm(TransferObject transferObj, string ocr);

        EventCodeEnum OcrReadStateRisingEvent(TransferObject transferObj, ModuleID pa, int pwIDReadResult = -1);
        EventCodeEnum LotSuspend(int foupNum);

        void SetDynamicMode(DynamicModeEnum modeEnum);

        void SetStageLock(int stageIndex, StageLockMode mode);
        void SetForcedDoneMode(int stageIndex, EnumModuleForcedState forcedDoneMode);

        void SetStopBeforeProbingFlag(int stageIdx, bool flag);
        void SetStopAfterProbingFlag(int stageIdx, bool flag);

        bool GetStopBeforeProbingFlag(int stageIdx);
        bool GetStopAfterProbingFlag(int stageIdx);

        #region <remarks> Get & Set Lot SysParam </remarks>
        bool GetIsCassetteAutoLock();
        void SetIsCassetteAutoLock(bool flag);
        bool GetIsCassetteAutoUnloadAfterLot();
        void SetIsCassetteAutoUnloadAfterLot(bool flag);
        bool GetIsCassetteAutoLockLeftOHT();
        void SetIsCassetteAutoLockLeftOHT(bool flag);
        bool GetIsCancelCarrierEventNotRuning();
        void SetIsCancelCarrierEventNotRuning(bool flag);
        bool GetIsCassetteDetectEventAfterRFID();
        void SetIsCassetteDetectEventAfterRFID(bool flag);
        bool GetIsLoaderLotEndBuzzerON();
        void SetIsLoaderLotEndBuzzerON(bool flag);
        bool GetIsAlwaysCloseFoupCover();
        void SetIsAlwaysCloseFoupCover(bool flag);
        int GetLotPauseTimeoutAlarm();
        void SetLotPauseTimeoutAlarm(int time);
        int GetExecutionTimeoutError();
        #endregion
        EventCodeEnum WriteWaitHandle(short value);
        EventCodeEnum WaitForHandle(short handle, long timeout = 60000);
        int ReadWaitHandle();

        EventCodeEnum IsShutterClose(int cellIdx);
        bool ValidateTransferCardObject(ICardOwnable source, ICardOwnable target);
        bool TransferCardObjectFunc(ICardOwnable source, ICardOwnable target);

        AutoFeedResult TransferWaferObjectFunc(OCRModeEnum ocrmode, IWaferOwnable source, IWaferOwnable target, string waferid);
        List<(IWaferOwnable sourceModule, IWaferOwnable targetModule)> GetLoadModules(IWaferOwnable tloc1, IWaferOwnable tloc2);
        EventCodeEnum SetPolishWaferInfoByLoadModule(IWaferOwnable tloc1, IWaferOwnable tloc2);

        void IsAlignDoing(ref bool pinAlignDoing, ref bool waferAlignDoing);
        void SetTCW_Mode(int stageIndex, TCW_Mode mode);
        EventCodeEnum SetCaseetteHashCodeToStage(int foupnumber, string cassetteHashCode);

        void InitStageWaferObject(int nCellIndex);
        void GEMDisconnectCallback(long lStageID);
        GPCellModeEnum GetStageMode(int stageIndex);
        Task<(EventCodeEnum, string)> RecoveryUnknownStatus(ModuleTypeEnum moduleType, int moduleNum, EnumSubsStatus status);
        EventCodeEnum SkipUnknownWaferLocation(ModuleTypeEnum moduleType, int moduleNum);
        TransferObject GetTransferObjectToSlotInfo(int cellNum);
        bool IsReAssignUnavailableStage(int stageNumber);
        void AddLotAbortStageInfos(int stageIdx, bool isCanReassignLot, bool abortCurrentLot);
        void SetMonitoringBehavior(List<IMonitoringBehavior> monitoringBehaviors, int stageIdx);

        List<IMonitoringBehavior> GetMonitoringBehaviorFromClient(int stageIdx);

        void ManualRecoveryToStage(int stageIdx, int behaviorIndex);

        EventCodeEnum GetLoaderEmergency();
        EventCodeEnum ValidationFoupLoadedState(int foupNuber, ref string stateStr);
        EventCodeEnum ValidationFoupUnloadedState(int foupNumber, ref string stateStr);
        void SetActiveLotInfotoStage(ActiveLotInfo activeLotInfo);
        EventCodeEnum GetCardPodStatusClient(int stageIndex);
        ILoaderLogManagerModule LoaderLogManager { get; }
        bool LoaderSystemInitFailure { get; set; }
    }


    public class FoupFlag
    {
        public bool ScanFlag { get; set; }

        public string FoupName { get; set; }
    }

    public interface ICardChangeSupervisor : ILoaderFactoryModule, IStateModule
    {
        bool CanStartCardChange(IAttachedModule cardloadport, IAttachedModule cardreqmodule);
        EventCodeEnum AllocateActiveCCInfo(ActiveCCInfo activeinfo);
        EventCodeEnum DeallocateActiveCCInfo(string allocHash);
        EventCodeEnum ClearAllActiveCCInfo();
        bool IsAllocatedStage(int number);
        (bool isExist, ActiveCCInfo activeCCInfo) FindActiveCCInfo(IAttachedModule cardloadport, IAttachedModule cardreqmodule);
        (bool isExist, ActiveCCInfo activeCCInfo) FindActiveCCInfo(string allocSeqId);

        (string allocSeqId, ModuleTypeEnum cardlpType, ModuleTypeEnum cardreqmoduleType, int cardlpIndex, int cardreqmoduleIndex) GetRunningCCInfo();
        EventCodeEnum CardinfoValidation(string Cardid, out string Msg);
        List<ActiveCCInfo> GetActiveCCInfos();
    }

    public interface IWaferChangeSupervisor : IFactoryModule, ILoaderFactoryModule, IStateModule
    {
        WaferChangeAutofeed WaferChangeAutofeed { get; set; }
        bool CanStartWaferChange();
        EventCodeEnum AllocateAutoFeedActions(WaferChangeData data);
        bool CSTAutoUnloadAfterWaferChange();

        //List<ActiveModuleInfo> ActiveModuleList { get; }
    }

    public class ActiveCCInfo
    {
        public ActiveCCInfo(string allochseqid, IAttachedModule cardlp, IAttachedModule cardreqmodule)
        {
            AllcatedSeqId = allochseqid;
            CardLoadPort = cardlp;
            CardReqModule = cardreqmodule;
        }

        private string _AllcatedSeqId = "";

        /// <summary>
        /// Pgv에서 할당된 CC Info의 ID
        /// TODO: LotID처럼 할당한 시퀀스에 대해서 구별가능한 별다른 id가 없어서 임의로 부여함. 나중에 시퀀스가 더 복잡해져서 생긴다면 이것을 사용하도록...
        /// TODO: 한시퀀스만 start-end 할때는 의미 없지만 여러 시퀀스가 큐로 할당될때 의미 있음.</summary>
        /// <summary>
        public string AllcatedSeqId
        {
            get { return _AllcatedSeqId; }
            set { _AllcatedSeqId = value; }
        }

        private IAttachedModule _CardLoadPort = null;

        public IAttachedModule CardLoadPort
        {
            get { return _CardLoadPort; }
            set { _CardLoadPort = value; }
        }


        private IAttachedModule _CardReqModule = null;

        public IAttachedModule CardReqModule
        {
            get { return _CardReqModule; }
            set { _CardReqModule = value; }
        }
    }
    public class WaferChangeAutofeed
    {
        public WaferChangeAutofeed(OCRModeEnum ocrmode, List<AutoFeedAction> activemoduleinfo)
        {
            PolishWaferOCRMode = ocrmode;
            AutoFeedActions = activemoduleinfo;
        }

        private OCRModeEnum _PolishWaferOCRMode = OCRModeEnum.NONE;
        public OCRModeEnum PolishWaferOCRMode
        {
            get { return _PolishWaferOCRMode; }
            set { _PolishWaferOCRMode = value; }
        }

        private List<AutoFeedAction> _AutoFeedActions = new List<AutoFeedAction>();

        public List<AutoFeedAction> AutoFeedActions
        {
            get { return _AutoFeedActions; }
            set { _AutoFeedActions = value; }
        }

        public void Clear()
        {
            try
            {
                if (AutoFeedActions != null)
                {
                    AutoFeedActions.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public enum AutoFeedResult
    {
        UNDEFINED,
        SUCCESS,
        FAILURE,
        SKIPPED 
    }

    public class AutoFeedAction
    {
        public AutoFeedResult Result { get; set; }
        public int LPNum1 { get; set; }
        public int LPNum2 { get; set; }

        private IAttachedModule _Allocate_Loc1 = null;

        public IAttachedModule Allocate_Loc1
        {
            get { return _Allocate_Loc1; }
            set { _Allocate_Loc1 = value; }
        }

        //wafer change rcmd로 부터 받은 loc2
        private IAttachedModule _Allocate_Loc2 = null;

        public IAttachedModule Allocate_Loc2
        {
            get { return _Allocate_Loc2; }
            set { _Allocate_Loc2 = value; }
        }

        //wafer change rcmd 로 부터 받은 wafer id
        private string _Allocate_WaferID = "";
        public string Allocate_WaferID
        {
            get { return _Allocate_WaferID; }
            set { _Allocate_WaferID = value; }
        }

        public AutoFeedAction(IAttachedModule source, IAttachedModule target, int lpnum1, int lpnum2, string wafer_id)
        {
            try
            {
                Allocate_Loc1 = source;
                Allocate_Loc2 = target;

                LPNum1 = lpnum1;
                LPNum2 = lpnum2;

                Allocate_WaferID = wafer_id;

                Result = AutoFeedResult.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetLoc1LoadPortId()
        {
            string lpId = "";
            if (LPNum1 > 0)
            {
                lpId = $"LP{LPNum1}";
            }
            return lpId;
        }
        public string GetLoc2LoadPortId()
        {
            string lpId = "";
            if (LPNum2 > 0)
            {
                lpId = $"LP{LPNum2}";
            }
            return lpId;
        }

        public string GetLoc1AtomId()
        {
            string atomId = "";
            if (Allocate_Loc1.ModuleType == ModuleTypeEnum.SLOT)
            {
                int slotNum = Allocate_Loc1.ID.Index % 25;
                if (slotNum == 0)
                {
                    slotNum = 25;
                }

                atomId = "S" + slotNum;
            }
            else if (Allocate_Loc1.ModuleType == ModuleTypeEnum.FIXEDTRAY)
            {
                atomId = "F" + Allocate_Loc1.ID.Index;
            }

            return atomId;
        }

        public string GetLoc2AtomId()
        {
            string atomId = "";
            if (Allocate_Loc2.ModuleType == ModuleTypeEnum.SLOT)
            {
                int slotNum = Allocate_Loc2.ID.Index % 25;
                if (slotNum == 0)
                {
                    slotNum = 25;
                }

                atomId = "S" + slotNum;
            }
            else if (Allocate_Loc2.ModuleType == ModuleTypeEnum.FIXEDTRAY)
            {
                atomId = "F" + Allocate_Loc2.ID.Index;
            }

            return atomId;
        }

        public string GetLoc1WaferId()
        {
            return (Allocate_Loc1 as IWaferOwnable)?.Holder?.TransferObject?.OCR.Value ?? ""; // Polish wafer id를 읽지 않았을 때 "EMPTY"로 들어가므로 ""는 웨이퍼가 없다는 뜻!
        }

        public string GetLoc2WaferId()
        {
            return (Allocate_Loc2 as IWaferOwnable)?.Holder?.TransferObject?.OCR.Value ?? ""; // Polish wafer id를 읽지 않았을 때 "EMPTY"로 들어가므로 ""는 웨이퍼가 없다는 뜻!
        }
    }
}

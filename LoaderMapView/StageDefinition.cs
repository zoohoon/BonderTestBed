using LoaderParameters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LoaderMapView
{
    using IOStateServiceClient;
    using ProberInterfaces;
    using System.Runtime.CompilerServices;
    using System.ComponentModel;
    using LoaderBase.Communication;
    using ServiceInterfaces;
    using ProberInterfaces.Proxies;
    using StageStateEnum = LoaderBase.Communication.StageStateEnum;
    using System.Windows.Threading;
    using System.Windows;
    using LogModule;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.PreAligner;
    using ProberInterfaces.Monitoring;
    using ProberInterfaces.Enum;

    public class StageObject_forLot : INotifyPropertyChanged, IStagelotSetting
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private bool _IsAssigned;
        public bool IsAssigned
        {
            get { return _IsAssigned; }
            set
            {
                if (value != _IsAssigned)
                {
                    _IsAssigned = value;
                    RaisePropertyChanged();
                }
            }
        }
        //TODO: V22에서 다시 살려야함. 
        //private bool _IsVerified;
        //public bool IsVerified
        //{
        //    get { return _IsVerified; }
        //    set
        //    {
        //        if (value != _IsVerified)
        //        {
        //            _IsVerified = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool _IsVerified;
        public bool IsVerified
        {
            get { return _IsVerified; }
            set
            {
                if (value != _IsVerified)
                {
                    _IsVerified = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LotModeEnum _LotMode;
        public LotModeEnum LotMode
        {
            get { return _LotMode; }
            set
            {
                if (value != _LotMode)
                {
                    _LotMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ForcedLotModeEnum _ForcedLotMode;
        public ForcedLotModeEnum ForcedLotMode
        {
            get { return _ForcedLotMode; }
            set
            {
                if (value != _ForcedLotMode)
                {
                    _ForcedLotMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _bIsSelected;
        public bool IsSelected
        {
            get { return _bIsSelected; }
            set
            {
                if (value != _bIsSelected)
                {
                    _bIsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LotName;
        public string LotName
        {
            get { return _LotName; }
            set
            {
                if (value != _LotName)
                {
                    _LotName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _OperatorName;
        public string OperatorName
        {
            get { return _OperatorName; }
            set
            {
                if (value != _OperatorName)
                {
                    _OperatorName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _RecipeName;
        public string RecipeName
        {
            get { return _RecipeName; }
            set
            {
                if (value != _RecipeName)
                {
                    _RecipeName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void Clear(bool UseLotProcessingVerify = false)
        {
            try
            {
                this.RecipeName = string.Empty;
                this.LotName = string.Empty;
                this.OperatorName = string.Empty;
                this.IsAssigned = false;
                this.IsSelected = false;

                // 사용하지 않으면, 항상 True
                if (UseLotProcessingVerify == true)
                {
                    this.IsVerified = false;
                }
                else
                {
                    this.IsVerified = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public StageObject_forLot(bool UseLotProcessingVerify)
        {
            Clear(UseLotProcessingVerify);
        }
    }

    public class StageObject : INotifyPropertyChanged, IStageObject
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private StageStateEnum _State;
        public StageStateEnum State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GPCellModeEnum _StageMode = GPCellModeEnum.DISCONNECT;
        public GPCellModeEnum StageMode
        {
            get { return _StageMode; }
            set
            {
                if (value != _StageMode)
                {
                    _StageMode = value;

                    if (_StageMode == GPCellModeEnum.ONLINE)
                    {
                        StreamingEnable = true;
                    }
                    else
                    {
                        StreamingEnable = false;
                    }

                    RaisePropertyChanged();
                }
            }
        }

        private StageLockMode _LockMode = StageLockMode.UNLOCK;
        public StageLockMode LockMode
        {
            get { return _LockMode; }
            set
            {
                if (value != _LockMode)
                {
                    _LockMode = value;
                    if (_LockMode == StageLockMode.LOCK)
                        StreamingEnable = true;
                    else
                        StreamingEnable = false;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Reconnecting = false; //연결된 cell이 비정상적으로 종료된 경우 true가 됨, watchdog thread에서 재연결에 성공한 경우 false가 됨
        public bool Reconnecting
        {
            get { return _Reconnecting; }
            set
            {
                if (value != _Reconnecting)
                {
                    _Reconnecting = value;
                    RaisePropertyChanged();
                }
            }
        }

        private StreamingModeEnum _StreamingMode = StreamingModeEnum.STREAMING_OFF;
        public StreamingModeEnum StreamingMode
        {
            get { return _StreamingMode; }
            set
            {
                if (value != _StreamingMode)
                {
                    _StreamingMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _StreamingEnable = false;
        public bool StreamingEnable
        {
            get { return _StreamingEnable; }
            set
            {
                if (value != _StreamingEnable)
                {
                    _StreamingEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isTransferError = false;
        public bool isTransferError
        {
            get { return _isTransferError; }
            set
            {
                if (value != _isTransferError)
                {
                    _isTransferError = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsStageModeChanged = false;
        public bool IsStageModeChanged
        {
            get { return _IsStageModeChanged; }
            set
            {
                if (value != _IsStageModeChanged)
                {
                    _IsStageModeChanged = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsWaferOnHandler;
        public bool IsWaferOnHandler
        {
            get { return _IsWaferOnHandler; }
            set
            {
                if (value != _IsWaferOnHandler)
                {
                    _IsWaferOnHandler = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TargetName;
        public string TargetName
        {
            get { return _TargetName; }
            set
            {
                if (value != _TargetName)
                {
                    _TargetName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    if (_WaferStatus == EnumSubsStatus.EXIST)
                    {
                        CellWaferAlignBtnEnable = true;
                    }
                    else
                    {
                        CellWaferAlignBtnEnable = false;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private EnumSubsStatus _CardStatus = EnumSubsStatus.NOT_EXIST;
        public EnumSubsStatus CardStatus
        {
            get { return _CardStatus; }
            set
            {
                if (value != _CardStatus)
                {
                    _CardStatus = value;
                    if (_CardStatus == EnumSubsStatus.EXIST)
                    {
                        CellPinAlignBtnEnable = true;
                    }
                    else
                    {
                        CellPinAlignBtnEnable = false;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleStateEnum _StageState = ModuleStateEnum.UNDEFINED;
        public ModuleStateEnum StageState
        {
            get { return _StageState; }
            set
            {
                if (value != _StageState)
                {
                    _StageState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Progress;
        public int Progress
        {
            get { return _Progress; }
            set
            {
                if (value != _Progress)
                {
                    _Progress = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleInfo _Info = new ModuleInfo();
        public ModuleInfo Info
        {
            get { return _Info; }
            set
            {
                if (value != _Info)
                {
                    _Info = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CellInfo _StageInfo = new CellInfo();
        public ICellInfo StageInfo
        {
            get { return _StageInfo; }
            set
            {
                if (value != _StageInfo)
                {
                    _StageInfo = (CellInfo)value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _LauncherIndex;

        public int LauncherIndex
        {
            get { return _LauncherIndex; }
            set { _LauncherIndex = value; }
        }

        private TransferObject _WaferObj;
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TransferObject _CardObj;
        public TransferObject CardObj
        {
            get { return _CardObj; }
            set
            {
                if (value != _CardObj)
                {
                    _CardObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _ChillingTimeMax = 100;
        public long ChillingTimeMax
        {
            get { return _ChillingTimeMax; }
            set
            {
                if (value != _ChillingTimeMax)
                {
                    _ChillingTimeMax = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _ChillingTime = 0;
        public long ChillingTime
        {
            get { return _ChillingTime; }
            set
            {
                if (value != _ChillingTime)
                {
                    _ChillingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _ChillingTimeProgressBar_Visibility = Visibility.Hidden;
        public Visibility ChillingTimeProgressBar_Visibility
        {
            get { return _ChillingTimeProgressBar_Visibility; }
            set
            {
                if (value != _ChillingTimeProgressBar_Visibility)
                {
                    _ChillingTimeProgressBar_Visibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsProbing;
        public bool IsProbing
        {
            get { return _IsProbing; }
            set
            {
                if (value != _IsProbing)
                {
                    _IsProbing = value;
                    RaisePropertyChanged();
                }
            }
        }
        private DateTime _ProbingTime;
        public DateTime ProbingTime
        {
            get { return _ProbingTime; }
            set
            {
                if (value != _ProbingTime)
                {
                    _ProbingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private String _ProbingTimeStr;
        public String ProbingTimeStr
        {
            get { return _ProbingTimeStr; }
            set
            {
                if (value != _ProbingTimeStr)
                {
                    _ProbingTimeStr = value;
                    RaisePropertyChanged();
                }
            }

        }
        private DateTime _ManualProbingTime;
        public DateTime ManualProbingTime
        {
            get { return _ManualProbingTime; }
            set
            {
                if (value != _ManualProbingTime)
                {
                    _ManualProbingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private String _ManualProbingTimeStr;
        public String ManualProbingTimeStr
        {
            get { return _ManualProbingTimeStr; }
            set
            {
                if (value != _ManualProbingTimeStr)
                {
                    _ManualProbingTimeStr = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _StopBeforeProbing;
        public bool StopBeforeProbing
        {
            get { return _StopBeforeProbing; }
            set
            {
                if (value != _StopBeforeProbing)
                {
                    _StopBeforeProbing = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _StopAfterProbing;
        public bool StopAfterProbing
        {
            get { return _StopAfterProbing; }
            set
            {
                if (value != _StopAfterProbing)
                {
                    _StopAfterProbing = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OnceStopBeforeProbing;
        public bool OnceStopBeforeProbing
        {
            get { return _OnceStopBeforeProbing; }
            set
            {
                if (value != _OnceStopBeforeProbing)
                {
                    _OnceStopBeforeProbing = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OnceStopAfterProbing;
        public bool OnceStopAfterProbing
        {
            get { return _OnceStopAfterProbing; }
            set
            {
                if (value != _OnceStopAfterProbing)
                {
                    _OnceStopAfterProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ManualZUPStateEnum _ManualZUPState;
        public ManualZUPStateEnum ManualZUPState
        {
            get { return _ManualZUPState; }
            set
            {
                if (value != _ManualZUPState)
                {
                    _ManualZUPState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _PauseReason;
        public string PauseReason
        {
            get { return _PauseReason; }
            set
            {
                if (value != _PauseReason)
                {
                    _PauseReason = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ManualZUPEnableEnum _ManualZUPEnable;
        public ManualZUPEnableEnum ManualZUPEnable
        {
            get { return _ManualZUPEnable; }
            set
            {
                if (value != _ManualZUPEnable)
                {
                    _ManualZUPEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellZUPBtnEnable;
        public bool CellZUPBtnEnable
        {
            get { return _CellZUPBtnEnable; }
            set

            {
                if (value != _CellZUPBtnEnable)
                {
                    _CellZUPBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellZDownBtnEnable;
        public bool CellZDownBtnEnable
        {
            get { return _CellZDownBtnEnable; }
            set
            {
                if (value != _CellZDownBtnEnable)
                {
                    _CellZDownBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellWaferAlignBtnEnable;
        public bool CellWaferAlignBtnEnable
        {
            get { return _CellWaferAlignBtnEnable; }
            set
            {
                if (value != _CellWaferAlignBtnEnable)
                {
                    _CellWaferAlignBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellPinAlignBtnEnable;
        public bool CellPinAlignBtnEnable
        {
            get { return _CellPinAlignBtnEnable; }
            set
            {
                if (value != _CellPinAlignBtnEnable)
                {
                    _CellPinAlignBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellTransferBtnEnable;
        public bool CellTransferBtnEnable
        {
            get { return _CellTransferBtnEnable; }
            set
            {
                if (value != _CellTransferBtnEnable)
                {
                    _CellTransferBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellInspectionBtnEnable;
        public bool CellInspectionBtnEnable
        {
            get { return _CellInspectionBtnEnable; }
            set
            {
                if (value != _CellInspectionBtnEnable)
                {
                    _CellInspectionBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellManualSoakingEnable;
        public bool CellManualSoakingEnable
        {
            get { return _CellManualSoakingEnable; }
            set
            {
                if (value != _CellManualSoakingEnable)
                {
                    _CellManualSoakingEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellParamSettingBtnEnable;
        public bool CellParamSettingBtnEnable
        {
            get { return _CellTransferBtnEnable; }
            set
            {
                if (value != _CellParamSettingBtnEnable)
                {
                    _CellParamSettingBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsRecoveryMode;
        public bool IsRecoveryMode
        {
            get { return _IsRecoveryMode; }
            set
            {
                if (value != _IsRecoveryMode)
                {
                    _IsRecoveryMode = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal; //Wafer,Pin ,PMI, Polish,Soaking중에 하나라도 켜져있을 경우 ForceDone 모드로 체크한다.
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set
            {
                if (value != _ForcedDone)
                {
                    _ForcedDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<IMonitoringBehavior> _MonitoringBehaviorList = new List<IMonitoringBehavior>();
        public List<IMonitoringBehavior> MonitoringBehaviorList
        {
            get { return _MonitoringBehaviorList; }
            set
            {
                if (value != _MonitoringBehaviorList)
                {
                    _MonitoringBehaviorList = value;
                    RaisePropertyChanged();
                }
            }
        }


        private DispFlipEnum _DispHorFlip;
        public DispFlipEnum DispHorFlip
        {
            get { return _DispHorFlip; }
            set
            {
                if (value != _DispHorFlip)
                {
                    _DispHorFlip = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DispFlipEnum _DispVerFlip;
        public DispFlipEnum DispVerFlip
        {
            get { return _DispVerFlip; }
            set
            {
                if (value != _DispVerFlip)
                {
                    _DispVerFlip = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ReverseManualMoveX;
        public bool ReverseManualMoveX
        {
            get { return _ReverseManualMoveX; }
            set
            {
                if (value != _ReverseManualMoveX)
                {
                    _ReverseManualMoveX = value;
                }
            }
        }

        private bool _ReverseManualMoveY;
        public bool ReverseManualMoveY
        {
            get { return _ReverseManualMoveY; }
            set
            {
                if (value != _ReverseManualMoveY)
                {
                    _ReverseManualMoveY = value;
                }
            }
        }

        private TCW_Mode _TCWMode;
        public TCW_Mode TCWMode
        {
            get { return _TCWMode; }
            set
            {
                if (value != _TCWMode)
                {
                    _TCWMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _InfoVisibility;
        public Visibility InfoVisibility
        {
            get { return _InfoVisibility; }
            set
            {
                if (value != _InfoVisibility)
                {
                    _InfoVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Visibility _ParamVisibility = Visibility.Hidden;
        public Visibility ParamVisibility
        {
            get { return _ParamVisibility; }
            set
            {
                if (value != _ParamVisibility)
                {
                    _ParamVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelectedParamSettingBtn;
        public bool IsSelectedParamSettingBtn
        {
            get { return _IsSelectedParamSettingBtn; }
            set
            {
                if (value != _IsSelectedParamSettingBtn)
                {
                    _IsSelectedParamSettingBtn = value;
                    RaisePropertyChanged();
                }
            }
        }

        public StageObject()
        {

        }

        public StageObject(int index)
        {
            Name = $"Cell #{index}";
            this.WaferStatus = EnumSubsStatus.NOT_EXIST;
            State = StageStateEnum.Not_Request;
            _StageInfo.Index = index;
            Index = index;
            (StageInfo as CellInfo)._StageObject = this;
        }

        //public StageObject(int index, StageSupervisorProxy stageproxy, IWaferObject wafer, RemoteMediumProxy dataproxy)
        //{
        //    Index = index;
        //    Name = $"Cell #{index}";
        //    this.WaferStatus = EnumSubsStatus.NOT_EXIST;
        //    State = StageStateEnum.Not_Request;
        //    _StageInfo.Index = index;
        //    _StageInfo.StageProxy = stageproxy;
        //    _StageInfo.RemoteMediumProxy = dataproxy;
        //    _StageInfo.WaferObject = (WaferObject)wafer;
        //}

        public void Clear()
        {
            StageState = ModuleStateEnum.UNDEFINED;
            //StageInfo.Clear();
            StageInfo = new CellInfo(this);
        }
    }

    public class CellInfo : INotifyPropertyChanged, ICellInfo
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

        public StageObject _StageObject { get; set; }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private StageLotData _LotData = new StageLotData();
        public StageLotData LotData
        {
            get { return _LotData; }
            set
            {
                if (value != _LotData)
                {
                    _LotData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<string> _SetTitles;
        public ObservableCollection<string> SetTitles
        {
            get { return _SetTitles; }
            set
            {
                if (value != _SetTitles)
                {
                    _SetTitles = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LastTitle;
        public string LastTitle
        {
            get { return _LastTitle; }
            set
            {
                if (value != _LastTitle)
                {
                    _LastTitle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _AlarmEnabled = true;
        public bool AlarmEnabled
        {
            get { return _AlarmEnabled; }
            set
            {
                Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            RaisePropertyChanged();
                        }),
                        DispatcherPriority.ContextIdle,
                        null
                    );

                //if (value != _AlarmEnabled)
                //{
                //    _AlarmEnabled = value;
                //    RaisePropertyChanged();
                //}
            }
        }


        private bool _IsReservePause = false;
        public bool IsReservePause
        {
            get { return _IsReservePause; }
            set
            {
                if (value != _IsReservePause)
                {
                    _IsReservePause = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<AlarmLogData> _ErrorCodeAlarams = new ObservableCollection<AlarmLogData>();
        public ObservableCollection<AlarmLogData> ErrorCodeAlarams
        {
            get { return _ErrorCodeAlarams; }
            set
            {
                if (value != _ErrorCodeAlarams)
                {
                    _ErrorCodeAlarams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _AlarmMessageNotNotifiedCount;
        public int AlarmMessageNotNotifiedCount
        {
            get { return _AlarmMessageNotNotifiedCount; }
            set
            {
                if (value != _AlarmMessageNotNotifiedCount)
                {
                    _AlarmMessageNotNotifiedCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private bool _IsOccurTempError;
        ////TimeOut Error 를 표시한 목적으로 사용할 예정.
        //public bool IsOccurTempError
        //{
        //    get { return _IsOccurTempError; }
        //    set
        //    {
        //        if (value != _IsOccurTempError)
        //        {
        //            _IsOccurTempError = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        private double _PV;
        public double PV
        {
            get { return _PV; }
            set
            {
                if (value != _PV)
                {
                    _PV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DewPoint;
        public double DewPoint
        {
            get { return _DewPoint; }
            set
            {
                if (value != _DewPoint)
                {
                    _DewPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsReceiveErrorEnd;
        public bool IsReceiveErrorEnd
        {
            get { return _IsReceiveErrorEnd; }
            set
            {
                if (value != _IsReceiveErrorEnd)
                {
                    _IsReceiveErrorEnd = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region //..Proxy

        //private Dictionary<Type, IProberProxy> _Proxies = new Dictionary<Type, IProberProxy>();
        //public Dictionary<Type, IProberProxy> Proxies
        //{
        //    get { return _Proxies; }
        //    set
        //    {
        //        if (value != _Proxies)
        //        {
        //            _Proxies = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        #endregion

        #region //..ServiceHost
        private ImageDispHost _DispHostService;
        public IImageDispHost DispHostService
        {
            get { return _DispHostService; }
            set
            {
                if (value != _DispHostService)
                {
                    _DispHostService = (ImageDispHost)value;
                    RaisePropertyChanged();
                }
            }
        }

        private DelegateEventHost _DelegateEventHost;
        public IDelegateEventHost DelegateEventHost
        {
            get { return _DelegateEventHost; }
            set
            {
                if (value != _DelegateEventHost)
                {
                    _DelegateEventHost = (DelegateEventHost)value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        private long _MapIndexX;
        public long MapIndexX
        {
            get { return _MapIndexX; }
            set
            {
                if (value != _MapIndexX)
                {
                    _MapIndexX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _MapIndexY;
        public long MapIndexY
        {
            get { return _MapIndexY; }
            set
            {
                if (value != _MapIndexY)
                {
                    _MapIndexY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsExcuteProgram = false;
        public bool IsExcuteProgram
        {
            get { return _IsExcuteProgram; }
            set
            {
                if (value != _IsExcuteProgram)
                {
                    PreIsExcuteProgram = _IsExcuteProgram;
                    _IsExcuteProgram = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PreIsExcuteProgram;
        public bool PreIsExcuteProgram
        {
            get { return _PreIsExcuteProgram; }
            set
            {
                if (value != _PreIsExcuteProgram)
                {
                    _PreIsExcuteProgram = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsConnectChecking = false; //cell을 connect or disconnet 하는동안 true로 설정되는 flag
        public bool IsConnectChecking
        {
            get { return _IsConnectChecking; }
            set
            {
                if (value != _IsConnectChecking)
                {
                    _IsConnectChecking = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsConnected = false;
        public bool IsConnected
        {

            get { return GetIsConnected(); }
            set
            {
                if (value != _IsConnected)
                {
                    _IsConnected = value;
                    RaisePropertyChanged();
                    if (_IsConnected)
                        LoggerManager.Debug($"STAGE #{Index} is connected");
                    else
                        LoggerManager.Debug($"STAGE #{Index} is disconnected");
                    if (_IsConnected == false)
                    {
                        if (_StageObject != null)
                        {
                            _StageObject.StageMode = GPCellModeEnum.DISCONNECT;
                        }
                    }
                }
            }
        }
        //private bool _IsRecoveryWaferAlign = false; 
        //public bool IsRecoveryWaferAlign
        //{
        //    get { return _IsRecoveryWaferAlign; }
        //    set
        //    {
        //        if (value != _IsRecoveryWaferAlign)
        //        {
        //            _IsRecoveryWaferAlign = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private bool _IsRecoveryPinAlign = false;
        public bool IsRecoveryPinAlign
        {
            get { return _IsRecoveryPinAlign; }
            set
            {
                if (value != _IsRecoveryPinAlign)
                {
                    _IsRecoveryPinAlign = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool GetIsConnected()
        {
            //{
            //    if (IsConnected
            //                & (_Stage as ICommunicationObject)?.State == CommunicationState.Opened
            //                & (_IOProxy as ICommunicationObject)?.State == CommunicationState.Opened
            //                & (_MotionProxy as ICommunicationObject).State == CommunicationState.Opened
            //                & (_VmGPCardChangeProxy as ICommunicationObject).State == CommunicationState.Opened
            //                & (_RemoteMediumProxy as ICommunicationObject).State == CommunicationState.Opened
            //                )
            //        return true;
            if (_IsConnected)
                return true;
            else
                return false;
        }

        private bool _IsExcuteLot = false;
        public bool IsExcuteLot
        {
            get { return _IsExcuteLot; }
            set
            {
                if (value != _IsExcuteLot)
                {
                    _IsExcuteLot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsChecked = false;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (value != _IsChecked)
                {
                    _IsChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderCommunicationManagerCallback _ServiceCallback;
        public ILoaderCommunicationManagerCallback ServiceCallback
        {
            get { return _ServiceCallback; }
            set
            {
                if (value != _ServiceCallback)
                {
                    _ServiceCallback = value;
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

        private bool _DeviceLoadResult = true;
        // true : Load 성공, false : Load 실패
        public bool DeviceLoadResult
        {
            get { return _DeviceLoadResult; }
            set
            {
                if (value != _DeviceLoadResult)
                {
                    _DeviceLoadResult = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _GemConnState;
        public bool GemConnState
        {
            get { return _GemConnState; }
            set
            {
                if (value != _GemConnState)
                {
                    _GemConnState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ChillerConnState;
        public bool ChillerConnState
        {
            get { return _ChillerConnState; }
            set
            {
                if (value != _ChillerConnState)
                {
                    _ChillerConnState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnalbeAutoConnect;
        public bool EnableAutoConnect
        {
            get { return _EnalbeAutoConnect; }
            set
            {
                if (value != _EnalbeAutoConnect)
                {
                    _EnalbeAutoConnect = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStagelotSetting _LotSetting;
        public IStagelotSetting LotSetting
        {
            get { return _LotSetting; }
            set
            {
                if (value != _LotSetting)
                {
                    _LotSetting = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private bool _IsVerified;
        //public bool IsVerified
        //{
        //    get { return _IsVerified; }
        //    set
        //    {
        //        if (value != _IsVerified)
        //        {
        //            _IsVerified = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private List<EnumUploadLogType> _UploadLogList = new List<EnumUploadLogType>();
        public List<EnumUploadLogType> UploadLogList
        {
            get { return _UploadLogList; }
            set
            {
                if (value != _UploadLogList)
                {
                    _UploadLogList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsMapDownloaded;
        public bool IsMapDownloaded
        {
            get { return _IsMapDownloaded; }
            set
            {
                if (value != _IsMapDownloaded)
                {
                    _IsMapDownloaded = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsAvailableTesterConnect;
        public bool IsAvailableTesterConnect
        {
            get { return _IsAvailableTesterConnect; }
            set
            {
                if (value != _IsAvailableTesterConnect)
                {
                    _IsAvailableTesterConnect = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsTesterConnected;
        public bool IsTesterConnected
        {
            get { return _IsTesterConnected; }
            set
            {
                if (value != _IsTesterConnected)
                {
                    _IsTesterConnected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProxyManager _proxyManager;
        public ProxyManager ProxyManager
        {
            get { return _proxyManager; }
            set
            {
                if (value != _proxyManager)
                {
                    _proxyManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _WaferObjHashCode;
        public int WaferObjHashCode
        {
            get { return _WaferObjHashCode; }
            set
            {
                if (value != _WaferObjHashCode)
                {
                    _WaferObjHashCode = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IWaferObject _WaferObject;
        public IWaferObject WaferObject
        {
            get { return _WaferObject; }
            set
            {
                if (value != _WaferObject)
                {
                    _WaferObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        //public void CollectProxies()
        //{
        //    try
        //    {
        //        Proxies.Clear();
        //    }
        //    catch (Exception err)is
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public void DisConnectProxies()
        {
            try
            {
                ProxyManager?.Disconnect();

                //foreach (var proxy in Proxies)
                //{
                //    System.ServiceModel.ICommunicationObject comobj = (proxy.Value as System.ServiceModel.ICommunicationObject);

                //    // State가 CommunicationState.Opened 또는 CommunicationState.Created인 경우, Close()를 호출해줍시다.
                //    if ((comobj.State == CommunicationState.Opened) || (comobj.State == CommunicationState.Created))
                //    {
                //        try
                //        {
                //            (proxy.Value as System.ServiceModel.ICommunicationObject).Close();
                //        }
                //        catch (CommunicationException err)
                //        {
                //            LoggerManager.Error(err.ToString());
                //            (proxy.Value as System.ServiceModel.ICommunicationObject).Abort();
                //        }
                //        catch (TimeoutException err)
                //        {
                //            LoggerManager.Error(err.ToString());
                //            (proxy.Value as System.ServiceModel.ICommunicationObject).Abort();
                //        }
                //        catch (Exception err)
                //        {
                //            LoggerManager.Error(err.ToString());
                //            (proxy.Value as System.ServiceModel.ICommunicationObject).Abort();
                //        }

                //    }
                //}

                //Proxies.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ProxyClear()
        {
            try
            {
                ProxyManager?.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void AbortProxies()
        {
            try
            {
                ProxyManager?.AbortProxies();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public CellInfo()
        {
        }

        public CellInfo(StageObject stageObject)
        {
            _StageObject = stageObject;
        }

        public void Clear()
        {
            //Proxies.Clear();
            ProxyClear();
            IsConnected = false;
        }
    }

    public class LauncherObject : INotifyPropertyChanged, ILauncherObject
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsConnected;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (value != _IsConnected)
                {
                    _IsConnected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PrevIsConnected;
        public bool PrevIsConnected
        {
            get { return _PrevIsConnected; }
            set
            {
                if (value != _PrevIsConnected)
                {
                    _PrevIsConnected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsChecked = false;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (value != _IsChecked)
                {
                    _IsChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderCommunicationManagerCallback _ServiceCallback;
        public ILoaderCommunicationManagerCallback ServiceCallback
        {
            get { return _ServiceCallback; }
            set
            {
                if (value != _ServiceCallback)
                {
                    _ServiceCallback = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMultiExecuterProxy _MultiExecuterProxy;
        public IMultiExecuterProxy MultiExecuterProxy
        {
            get { return _MultiExecuterProxy; }
            set
            {
                if (value != _MultiExecuterProxy)
                {
                    _MultiExecuterProxy = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<int> _CellIndexs = new List<int>();
        public List<int> CellIndexs
        {
            get { return _CellIndexs; }
            set
            {
                if (value != _CellIndexs)
                {
                    _CellIndexs = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LauncherObject(int index, IMultiExecuterProxy proxy)
        {
            MultiExecuterProxy = proxy;
            Name = $"LAUNCHER #{index + 1}";
            Index = index;
            LauncherDiskObjectCollection = new ObservableCollection<ILauncherDiskObject>();
        }

        //DiskInfoCollection
        private ObservableCollection<ILauncherDiskObject> _LauncherDiskObjectCollection = new ObservableCollection<ILauncherDiskObject>();
        public ObservableCollection<ILauncherDiskObject> LauncherDiskObjectCollection
        {
            get { return _LauncherDiskObjectCollection; }
            set
            {
                if (value != _LauncherDiskObjectCollection)
                {
                    _LauncherDiskObjectCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class LauncherDiskObject : INotifyPropertyChanged, ILauncherDiskObject
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public LauncherDiskObject(string drivename, string usagespace, string availablespace, string totalspace, string percent)
        {
            this.DriveName = drivename;
            this.UsageSpace = usagespace;
            this.AvailableSpace = availablespace;
            this.TotalSpace = totalspace;
            this.Percent = percent;

        }



        private string _DriveName;
        public string DriveName
        {
            get { return _DriveName; }
            set
            {
                if (value != _DriveName)
                {
                    _DriveName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _AvailableSpace;
        public string AvailableSpace
        {
            get { return _AvailableSpace; }
            set
            {
                if (value != _AvailableSpace)
                {
                    _AvailableSpace = value;
                    RaisePropertyChanged();
                }


            }
        }


        private string _TotalSpace;
        public string TotalSpace
        {
            get { return _TotalSpace; }
            set
            {
                if (value != _TotalSpace)
                {
                    _TotalSpace = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _UsageSpace;
        public string UsageSpace
        {
            get { return _UsageSpace; }
            set
            {
                if (value != _UsageSpace)
                {
                    _UsageSpace = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Percent;
        public string Percent
        {
            get { return _Percent; }
            set
            {
                if (value != _Percent)
                {
                    _Percent = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    public class PAObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsWaferExist;
        public bool IsWaferExist
        {
            get { return _IsWaferExist; }
            set
            {
                if (value != _IsWaferExist)
                {
                    _IsWaferExist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private StageObject _ReservedCell;
        public StageObject ReservedCell
        {
            get { return _ReservedCell; }
            set
            {
                if (value != _ReservedCell)
                {
                    _ReservedCell = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _Progress;
        public double Progress
        {
            get { return _Progress; }
            set
            {
                if (value != _Progress)
                {
                    _Progress = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _WaferObj;
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _Enable;
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumPAStatus _PAStatus;
        public EnumPAStatus PAStatus
        {
            get { return _PAStatus; }
            set
            {
                if (value != _PAStatus)
                {
                    _PAStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PAObject(int index)
        {
            Name = $"PA #{index + 1}";
            Index = index + 1;
            WaferStatus = EnumSubsStatus.NOT_EXIST;
        }

        public bool Lock(StageObject requestor)
        {
            ReservedCell = requestor;
            return true;
        }
    }

    public class BufferObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _WaferObj;
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Enable;
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }
        public BufferObject(int index)
        {
            Name = $"Buffer #{index + 1}";
            Index = index + 1;
            WaferStatus = EnumSubsStatus.NOT_EXIST;
        }

    }
    public class InspectionTrayInfoObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _WaferObj;
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }
        public InspectionTrayInfoObject(int index)
        {
            Name = $"INSP Tray #{index + 1}";
            Index = index + 1;
            WaferStatus = EnumSubsStatus.NOT_EXIST;
        }

    }
    public class FixedTrayInfoObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _WaferObj;
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CanUseBuffer;
        public bool CanUseBuffer
        {
            get { return _CanUseBuffer; }
            set
            {
                if (value != _CanUseBuffer)
                {
                    _CanUseBuffer = value;
                    RaisePropertyChanged();
                }
            }
        }
        public FixedTrayInfoObject(int index)
        {
            Name = $"FixedTray #{index + 1}";
            Index = index + 1;
            WaferStatus = EnumSubsStatus.NOT_EXIST;
        }

    }
    public class CardBufferObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumWaferType _WaferType;
        public EnumWaferType WaferType
        {
            get { return _WaferType; }
            set
            {
                if (value != _WaferType)
                {
                    _WaferType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CardPRESENCEStateEnum _CardPRESENCEState;
        public CardPRESENCEStateEnum CardPRESENCEState
        {
            get { return _CardPRESENCEState; }
            set
            {
                if (value != _CardPRESENCEState)
                {
                    _CardPRESENCEState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCardAttachHolder = true;
        public bool IsCardAttachHolder
        {
            get { return _IsCardAttachHolder; }
            set
            {
                if (value != _IsCardAttachHolder)
                {
                    _IsCardAttachHolder = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _CardObj;
        public TransferObject CardObj
        {
            get { return _CardObj; }
            set
            {
                if (value != _CardObj)
                {
                    _CardObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Enable = true;
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }
        public CardBufferObject(int index)
        {
            //Name = $"CardBuffer #{index + 1}";
            Name = $"CB #{index + 1}";
            Index = index + 1;
            WaferStatus = EnumSubsStatus.NOT_EXIST;

        }
    }
    public class CardTrayObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumWaferType _WaferType;
        public EnumWaferType WaferType
        {
            get { return _WaferType; }
            set
            {
                if (value != _WaferType)
                {
                    _WaferType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCardAttachHolder = true;
        public bool IsCardAttachHolder
        {
            get { return _IsCardAttachHolder; }
            set
            {
                if (value != _IsCardAttachHolder)
                {
                    _IsCardAttachHolder = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _CardObj;
        public TransferObject CardObj
        {
            get { return _CardObj; }
            set
            {
                if (value != _CardObj)
                {
                    _CardObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }
        public CardTrayObject(int index)
        {
            //Name = $"CardTray #{index + 1}";
            Name = $"CT #{index + 1}";
            Index = index + 1;
            WaferStatus = EnumSubsStatus.NOT_EXIST;

        }


    }
    public class CardArmObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumWaferType _WaferType;
        public EnumWaferType WaferType
        {
            get { return _WaferType; }
            set
            {
                if (value != _WaferType)
                {
                    _WaferType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _CardObj;
        public TransferObject CardObj
        {
            get { return _CardObj; }
            set
            {
                if (value != _CardObj)
                {
                    _CardObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }
        public CardArmObject(int index)
        {
            Name = $"CardArm #{index + 1}";
            Index = index + 1;
            WaferStatus = EnumSubsStatus.NOT_EXIST;

        }


    }

    public class ArmObject : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableTransfer = true;
        public bool IsEnableTransfer
        {
            get { return _IsEnableTransfer; }
            set
            {
                if (value != _IsEnableTransfer)
                {
                    _IsEnableTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _WaferObj;
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ArmObject(int index)
        {
            Name = $"Arm #{index + 1}";
            Index = index + 1;
            WaferStatus = EnumSubsStatus.NOT_EXIST;
        }
    }

    //public class FoupObject : INotifyPropertyChanged
    //{
    //    #region ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion

    //    private string _Name;
    //    public string Name
    //    {
    //        get { return _Name; }
    //        set
    //        {
    //            if (value != _Name)
    //            {
    //                _Name = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    private int _WaferCount;
    //    public int WaferCount
    //    {
    //        get { return _WaferCount; }
    //        set
    //        {
    //            if (value != _WaferCount)
    //            {
    //                _WaferCount = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    private FoupStateEnum _State;
    //    public FoupStateEnum State
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

    //    private ObservableCollection<SlotObject> _Slots;
    //    public ObservableCollection<SlotObject> Slots
    //    {
    //        get { return _Slots; }
    //        set
    //        {
    //            if (value != _Slots)
    //            {
    //                _Slots = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private int _Index;

    //    public int Index
    //    {
    //        get { return _Index; }
    //        set { _Index = value; }
    //    }

    //    public bool _PreIsSelected { get; set; } = false;

    //    private bool _IsSelected = false;
    //    public bool IsSelected
    //    {
    //        get { return _IsSelected; }
    //        set
    //        {
    //            if (value != _IsSelected)
    //            {
    //                _PreIsSelected = _IsSelected;
    //                _IsSelected = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }


    //    public FoupObject(int index)
    //    {
    //        Name = $"Foup #{index + 1}";
    //        State = FoupStateEnum.IDLE;
    //        _Slots = new ObservableCollection<SlotObject>();
    //        for (int i = 0; i < 25; i++)
    //        {
    //            _Slots.Add(new SlotObject(25 - i, index + 1));
    //        }
    //        Index = index + 1;
    //    }

    //    public void Load()
    //    {
    //        WaferCount = 25;
    //    }
    //    public void SetProcess()
    //    {
    //        if (WaferCount > 0) WaferCount--;
    //    }
    //}


    //public class SlotObject : TransferObjectDeviceInfo, INotifyPropertyChanged
    //{
    //    #region ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion


    //    private int _FoupNumber;
    //    public int FoupNumber
    //    {
    //        get { return _FoupNumber; }
    //        set
    //        {
    //            if (value != _FoupNumber)
    //            {
    //                _FoupNumber = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private string _Name;
    //    public string Name
    //    {
    //        get { return _Name; }
    //        set
    //        {
    //            if (value != _Name)
    //            {
    //                _Name = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private EnumSubsStatus _WaferStatus;
    //    public EnumSubsStatus WaferStatus
    //    {
    //        get { return _WaferStatus; }
    //        set
    //        {
    //            if (value != _WaferStatus)
    //            {
    //                _WaferStatus = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private EnumWaferState _WaferState;
    //    public EnumWaferState WaferState
    //    {
    //        get { return _WaferState; }
    //        set
    //        {
    //            if (value != _WaferState)
    //            {
    //                _WaferState = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private Element<EnumWaferType> _WaferType = new Element<EnumWaferType>();
    //    public Element<EnumWaferType> WaferType
    //    {
    //        get { return _WaferType; }
    //        set
    //        {
    //            if (value != _WaferType)
    //            {
    //                _WaferType = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private bool _IsEnableTransfer = true;
    //    public bool IsEnableTransfer
    //    {
    //        get { return _IsEnableTransfer; }
    //        set
    //        {
    //            if (value != _IsEnableTransfer)
    //            {
    //                _IsEnableTransfer = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private bool _IsSelected = false;
    //    public bool IsSelected
    //    {
    //        get { return _IsSelected; }
    //        set
    //        {
    //            if (value != _IsSelected)
    //            {
    //                _IsSelected = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //private int _Index;
    //public int Index
    //{
    //    get { return _Index; }
    //    set
    //    {
    //        if (value != _Index)
    //        {
    //            _Index = value;
    //            RaisePropertyChanged();
    //        }
    //    }
    //}


    //    public SlotObject(int index)
    //    {
    //        Name = $"SLOT #{index}";
    //        WaferStatus = EnumSubsStatus.NOT_EXIST;
    //    }
    //    public SlotObject(int index, int foupindex)
    //    {
    //        Name = $"SLOT #{index}";
    //        WaferStatus = EnumSubsStatus.NOT_EXIST;
    //        FoupNumber = foupindex;
    //    }
    //    public void Load()
    //    {
    //    }

    //}

}

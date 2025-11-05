using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using ProberInterfaces.LoaderController;
using ProberInterfaces.Foup;
using System.IO;
using ProberInterfaces.Command;
using ProberErrorCode;

namespace LoaderController.GPController
{
    using LoaderControllerBase;
    using LoaderParameters;
    using LoaderServiceBase;

    using LoaderControllerStates;
    using ProberInterfaces.State;
    using ProberInterfaces.Wizard;
    using LogModule;
    using global::LoaderController.GPWaferTransferScheduler;
    using System.ServiceModel;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Loader;
    using SequenceRunner;
    using ProberInterfaces.CardChange;
    using NotifyEventModule;
    using StageModule;
    using ProberInterfaces.Event;
    using ProberInterfaces.Param;
    using ProberInterfaces.Soaking;
    using System.Linq;
    using RetestObject;
    using ProberInterfaces.Monitoring;
    using SerializerUtil;
    using ProberInterfaces.Enum;
    using ProberInterfaces.WaferTransfer;

    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GP_LoaderController : ILoaderController, ILoaderServiceCallback, INotifyPropertyChanged, IHasDevParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public ILoaderControllerParam LoaderConnectParam { get => ControllerParam; }

        public bool Initialized { get; set; } = false;

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.LoaderController);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isExchange;
        public bool isExchange
        {
            get { return _isExchange; }
            set
            {
                if (value != _isExchange)
                {
                    _isExchange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _LoaderDoorOpenTicks = 0;
        public long LoaderDoorOpenTicks
        {
            get { return _LoaderDoorOpenTicks; }
            set
            {
                if (value != _LoaderDoorOpenTicks)
                {
                    _LoaderDoorOpenTicks = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsAbort;
        public bool IsAbort
        {
            get { return _IsAbort; }
            private set
            {
                if (value != _IsAbort)
                {
                    _IsAbort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLotOut = false;
        public bool IsLotOut
        {
            get { return _IsLotOut; }
            set
            {
                if (value != _IsLotOut)
                {
                    _IsLotOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCancel = false;

        public bool IsCancel
        {
            get { return _IsCancel; }
            set 
            { 
                if(_IsCancel != value)
                {
                    LoggerManager.Debug($"[GP_LoaderController] IsCancel Changed to {IsCancel} => {value}");
                    _IsCancel = value;
                }
                
            }
        }

        private IParam _DevParam;
        [ParamIgnore]
        public IParam DevParam
        {
            get { return _DevParam; }
            set
            {
                if (value != _DevParam)
                {
                    _DevParam = value;
                    // NotifyPropertyChanged("Parameter");
                }
            }
        }
        private IParam _SysParam;
        [ParamIgnore]
        public IParam SysParam
        {
            get { return _SysParam; }
            set
            {
                if (value != _SysParam)
                {
                    _SysParam = value;
                    // NotifyPropertyChanged("SysParam");
                }
            }
        }

        protected GP_LoaderControllerStateBase ControllerState;
        public IInnerState InnerState
        {
            get { return ControllerState; }
            set
            {
                if (value != ControllerState)
                {
                    ControllerState = value as GP_LoaderControllerStateBase;
                }
            }
        }
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }
        public IInnerState PreInnerState { get; set; }

        private LoaderControllerParam _ControllerParam;
        public LoaderControllerParam ControllerParam
        {
            get { return _ControllerParam; }
            set { _ControllerParam = value; RaisePropertyChanged(); }
        }

        private LoaderSystemParameter _LoaderSystemParam;
        public LoaderSystemParameter LoaderSystemParam
        {
            get { return _LoaderSystemParam; }
            set { _LoaderSystemParam = value; RaisePropertyChanged(); }
        }

        private LoaderDeviceParameter _LoaderDeviceParam;
        public LoaderDeviceParameter LoaderDeviceParam
        {
            get { return _LoaderDeviceParam; }
            set { _LoaderDeviceParam = value; RaisePropertyChanged(); }
        }

        public ModuleID ChuckID => ModuleID.Create(ModuleTypeEnum.CHUCK, ControllerParam.ChuckIndex, "");

        #region Properties
        private LoaderInfo _LoaderInfo;
        public LoaderInfo LoaderInfo
        {
            get { return _LoaderInfo; }
            set
            {
                _LoaderInfo = value;
                RaisePropertyChanged();
                LoaderInfoObj = _LoaderInfo;
            }
        }

        private ILoaderInfo _LoaderInfoObj;
        public ILoaderInfo LoaderInfoObj
        {
            get { return _LoaderInfoObj; }
            set
            {
                if (value != _LoaderInfoObj)
                {
                    _LoaderInfoObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public IGPLoaderService GPLoaderService
        {
            get
            {
                return LoaderServiceProvider.GetGPLoaderService(this);
            }
        }


        //public IGPLoaderService GPLoaderService { get; set; }
        public IWaferTransferScheduler WaferTransferScheduler { get; set; }


        public StageLotData stageInfo { get; set; }

        public LoaderMap ReqMap { get; set; }

        private bool _StopBeforeProbingFlag = false;
        public bool StopBeforeProbingFlag
        {
            get { return _StopBeforeProbingFlag; }
            set
            {
                if (value != _StopBeforeProbingFlag)
                {
                    _StopBeforeProbingFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _StopAfterProbingFlag = false;
        public bool StopAfterProbingFlag
        {
            get { return _StopAfterProbingFlag; }
            set
            {
                if (value != _StopAfterProbingFlag)
                {
                    _StopAfterProbingFlag = value;
                    RaisePropertyChanged();
                }
            }
        }


        public string GetFoupNumberStr()
        {
            string retval = string.Empty;

            try
            {
                if (this.GetParam_Wafer().GetSubsInfo() != null)
                {
                    int slotNum = this.GetParam_Wafer().GetSubsInfo().SlotIndex.Value % 25;
                    int offset = 0;

                    if (slotNum == 0)
                    {
                        slotNum = 25;
                        offset = -1;
                    }

                    retval = (((this.GetParam_Wafer().GetSubsInfo().SlotIndex.Value + offset) / 25) + 1).ToString();
                }
                else
                {
                    LoggerManager.Debug($"GetFoupNumberStr(): GetParam_Wafer().GetSubsInfo() is null");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        //=> TODO : Commands

        public StageLotData GetStageInfo()
        {
            try
            {
                stageInfo = new StageLotData();
                stageInfo.ConnectState = "On";
                stageInfo.RenewTime = DateTime.Now;
                stageInfo.DeviceName = this.FileManager().GetDeviceName();
                stageInfo.ProbeCardID = this.CardChangeModule().GetProbeCardID();

                stageInfo.WaferID = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;

                stageInfo.WaferCount = this.LotOPModule().LotInfo.ProcessedWaferCnt.ToString();
                stageInfo.LoadingTimeEnable = false;
                stageInfo.FoupNumber = this.LotOPModule().LotInfo.FoupNumber.Value.ToString();

                if (this.GetParam_Wafer().WaferStatus == EnumSubsStatus.EXIST)
                {
                    stageInfo.WaferLoadingTime = this.GetParam_Wafer().GetSubsInfo().LoadingTime.ToString();

                    int? snum = this.StageSupervisor().WaferObject.GetSlotIndex();

                    if (snum != null)
                    {
                        stageInfo.SlotNumber = snum.ToString();
                    }
                    else
                    {
                        stageInfo.SlotNumber = string.Empty;
                    }

                    stageInfo.LoadingTimeEnable = true;
                }
                else
                {
                    stageInfo.SlotNumber = string.Empty;
                }

                stageInfo.CurTemp = (this.TempController().TempInfo.CurTemp.Value).ToString("F1");
                stageInfo.SetTemp = (this.TempController().TempInfo.SetTemp.Value).ToString("F1");
                stageInfo.TargetTemp = (this.TempController().TempInfo.TargetTemp.Value);
                stageInfo.Deviation = (this.TempController().GetDeviaitionValue()).ToString();
                stageInfo.LotState = this.LotOPModule().ModuleState.State.ToString();
                stageInfo.WaferAlignState = this.GetParam_Wafer().AlignState.Value.ToString();
                stageInfo.PadCount = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Count.ToString();
                stageInfo.PinAlignState = this.GetParam_ProbeCard().AlignState.Value.ToString();
                stageInfo.SoakingState = this.SoakingModule().ModuleState.State.ToString();
                stageInfo.MarkAlignState = this.StageSupervisor().MarkObject.AlignState.Value.ToString();
                stageInfo.ProbingState = this.ProbingModule().ProbingStateEnum.ToString();
                stageInfo.ProbingOD = this.ProbingModule().OverDrive.ToString();
                //stageInfo.SoakingTime = this.SoakingModule().GetSoakingTime().ToString();
                stageInfo.TempState = this.TempController().GetTempControllerState().ToString();
                stageInfo.LotStartTime = this.LotOPModule().LotInfo.LotStartTime.ToString();
                stageInfo.LotStartTimeEnable = this.LotOPModule().LotInfo.LotStartTimeEnable;
                stageInfo.LotEndTime = this.LotOPModule().LotInfo.LotEndTime.ToString();
                stageInfo.LotEndTimeEnable = this.LotOPModule().LotInfo.LotEndTimeEnable;
                stageInfo.CellMode = this.StageSupervisor().StageMode;
                stageInfo.StageMoveState = this.StageSupervisor().StageModuleState.GetState().ToString();
                stageInfo.Clearance = this.ProbingModule().ZClearence.ToString();
                stageInfo.ProcessedWaferCountUntilBeforeCardChange = this.LotOPModule().SystemInfo.ProcessedWaferCountUntilBeforeCardChange.ToString();
                stageInfo.MarkedWaferCountLastPolishWaferCleaning = this.LotOPModule().SystemInfo.MarkedWaferCountLastPolishWaferCleaning;
                stageInfo.TouchDownCountUntilBeforeCardChange = this.LotOPModule().SystemInfo.TouchDownCountUntilBeforeCardChange.ToString();
                stageInfo.MarkedTouchDownCountLastPolishWaferCleaning = this.LotOPModule().SystemInfo.MarkedTouchDownCountLastPolishWaferCleaning;
                stageInfo.LotID = this.LotOPModule().LotInfo.LotName.Value;
                stageInfo.LotAbortedByUser = this.LotOPModule().IsLotAbortedByUser();

                string SoakingTypeStr = "";
                string RemainingTimeStr = "";
                string OD_Val = "";
                bool EnableStopSoakBtn = true;
                if (this.SoakingModule().GetStatusSoakingInfoUpdateToLoader(ref SoakingTypeStr, ref RemainingTimeStr, ref OD_Val, ref EnableStopSoakBtn))
                {
                    stageInfo.SoakingRemainTime = RemainingTimeStr;
                    stageInfo.SoakingType = SoakingTypeStr;
                    stageInfo.SoakingZClearance = OD_Val;
                }

                stageInfo.StopSoakBtnEnable = EnableStopSoakBtn;
                stageInfo.LotMode = this.LotOPModule().LotInfo.LotMode.Value;
                this.NotifyManager().SendLastMSGToLoader();

                //LoggerManager.Debug("LoaderController - GetStageInfo End");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
            return stageInfo;
        }

        public void ReadStatusSoakingChillingTime(ref long _ChillingTimeMax, ref long _ChillingTime, ref SoakingStateEnum CurStatusSoaking_State)
        {
            _ChillingTimeMax = 100;
            _ChillingTime = 0;

            try
            {
                bool enableStatusSoaking = false;
                this.SoakingModule().StatusSoakingParamIF.IsEnableStausSoaking(ref enableStatusSoaking);

                if (enableStatusSoaking)
                {
                    List<ChillingTimeTableInfo> chillingTimeTableList = new List<ChillingTimeTableInfo>();
                    this.SoakingModule().StatusSoakingParamIF.Get_ChillingTimeTableInfo(ref chillingTimeTableList);
                    if (chillingTimeTableList.Count > 0)
                    {

                        _ChillingTimeMax = chillingTimeTableList[0].ChillingTimeSec * 1000; // Millisecond 단위로 변경하기 위해 1000을 곱한다.
                        _ChillingTime = _ChillingTimeMax - this.SoakingModule().ChillingTimeMngObj.GetChillingTime(); // Guage 표시를 위해 Max에서 Chilling Time을 뺀다.
                        CurStatusSoaking_State = this.SoakingModule().GetStatusSoakingState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetShowStatusSoakingSettingPageToggleValue()
        {
            return this.SoakingModule().GetShowStatusSoakingSettingPageToggleValue();
        }

        public void Check_N_ClearStatusSoaking()
        {
            this.SoakingModule().Check_N_ClearStatusSoaking();
        }

        public bool IsEnablePolishWaferSoaking()
        {
            bool usePolishWafer = false;
            try
            {
                if (this.SoakingModule().GetShowStatusSoakingSettingPageToggleValue()
                    && this.SoakingModule().GetCurrentStatusSoakingUsingFlag()
                    && this.SoakingModule().IsUsePolishWafer())
                    usePolishWafer = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return usePolishWafer;
        }
        public bool GetTesterAvailableData()
        {
            bool retval = false;

            try
            {
                retval = this.TesterCommunicationManager().GetTesterAvailable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void LoadLoaderControllerParameter()
        {
            try
            {

                string OpParameterPath = Path.Combine(this.FileManager().GetRootParamPath(), @"Parameters\Loader\LoaderControllerParam.json");

                if (File.Exists(OpParameterPath) == false)
                {
                    ControllerParam = CreateDefaultLoaderControllerOpParamm();

                    Extensions_IParam.SaveParameter(null, ControllerParam, null, OpParameterPath);
                    //Serialize(OpParameterPath, ControllerParam);
                }
                else
                {
                    EventCodeEnum retVal = EventCodeEnum.NONE;
                    IParam tmpParam = null;
                    retVal = this.LoadParameter(ref tmpParam, typeof(LoaderControllerParam), null, OpParameterPath);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        throw new Exception("[LoaderController] Faile LoadLoaderControllerParameter.");
                    }
                    else
                    {
                        ControllerParam = tmpParam as LoaderControllerParam;
                    }
                }

#if DEBUG
                //Replace New parameter
                if (ControllerParam == null)
                {
                    ControllerParam = CreateDefaultLoaderControllerOpParamm();
                }
#endif

                //**
                if (string.IsNullOrEmpty(ControllerParam.ControllerID))
                {
                    ControllerParam.ControllerID = AppDomain.CurrentDomain.Id.ToString();
                }

                LoaderControllerParam CreateDefaultLoaderControllerOpParamm()
                {
                    LoaderControllerParam defaultParam = new LoaderControllerParam();

                    defaultParam.ControllerID = "STAGE1";
                    defaultParam.LoaderServiceType = LoaderServiceTypeEnum.DynamicLinking;
                    defaultParam.EndpointConfigurationName = "";
                    defaultParam.ChuckIndex = 1;
                    defaultParam.LoaderIP = "192.168.8.1";

                    return defaultParam;
                }

                VirtualConnector.ChuckIndex = ControllerParam.ChuckIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region # IModule 
        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set { _ModuleState = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set { _TransitionInfo = value; RaisePropertyChanged(); }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //TODO : Dll or Parameterize
                    LoadLoaderControllerParameter();

                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    //
                    WaferTransferScheduler = new GP_WaferTransferScheduler(this);

                    //
                    //    InitLoaderService();

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    ControllerState = new IDLE(this);
                    ModuleState = new ModuleUndefinedState(this);

                    UpdateDeviceParam(this.LoaderDeviceParam);
                    Initialized = true;
                    IsAbort = false;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");
                    UpdateDeviceParam(this.LoaderDeviceParam);
                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"InitModule() : error occurred. msg = {0}", ex.Message);
                LoggerManager.Exception(err);

                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }

        //=> inner methods
        private void InitLoaderService()
        {
            try
            {
                string rootParamPath = this.FileManager().GetRootParamPath();

                LoaderServiceProvider.CreateGPLoader(this);
                if (this.GPLoaderService != null)
                {
                    BroadcastLotState(false);

                    SetStopBeforeProbingFlag(this.LotOPModule().LotDeviceParam.StopOption.StopBeforeProbing.Value);
                    SetStopAfterProbingFlag(this.LotOPModule().LotDeviceParam.StopOption.StopAfterProbing.Value);

                    SetOnceStopBeforeProbingFlag(this.LotOPModule().LotDeviceParam.StopOption.OnceStopBeforeProbing.Value);
                    SetOnceStopAfterProbingFlag(this.LotOPModule().LotDeviceParam.StopOption.OnceStopAfterProbing.Value);

                    UpdateLogUploadList(EnumUploadLogType.Debug);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
                LoaderServiceProvider.GPDeinit(this.GPLoaderService);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($ex.Message);
                LoggerManager.Exception(err);

            }
        }

        public void RecoveryWithMotionInit()
        {
            try
            {
                GPLoaderService.RECOVERY_MotionInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Error($"{err}, Error occurred while loader recovery.");
            }

        }

        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retVal = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retVal = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retVal = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public ModuleStateEnum Abort()
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retVal = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;

            try
            {
                EventCodeEnum retVal = InnerState.Execute();
                ModuleState.StateTransition(InnerState.GetModuleState());
                RunTokenSet.Update();
                stat = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "InitModule() Error occurred.");
                LoggerManager.Exception(err);

                throw err;
            }

            return stat;
        }

        public EventCodeEnum ResponseSystemInit(EventCodeEnum errorCode)
        {
            return GPLoaderService.ResponseSystemInit(errorCode);
        }
        public EventCodeEnum ResponseCardRecovery(EventCodeEnum errorCode)
        {
            return GPLoaderService.ResponseCardRecovery(errorCode);
        }

        public void StateTransition(ModuleStateBase state)
        {
            try
            {
                ModuleState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (state != null)
                {
                    PreInnerState = InnerState;
                    InnerState = state;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void BroadcastLotState(bool isBuzzerOn)
        {
            if (GPLoaderService != null)
            {
                try
                {
                    GPLoaderService?.IsServiceAvailable(); //통신에 문제가 발생하면 exception 발생할 것임
                    {
                        ModuleStateEnum stateenum = ModuleStateEnum.UNDEFINED;

                        if (this.MonitoringManager().IsSystemError)
                        {
                            stateenum = ModuleStateEnum.ERROR;
                            // [STM_CATANIA] State 값을 받고 싶어하기 때문에 Enum을 추가해서 처리함.
                            this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.LOTOP_ERROR);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(StageErrorEvent).FullName, new ProbeEventArgs(this, semaphore));
                            semaphore.Wait();

                            //this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(StageErrorEvent).FullName));
                        }
                        else
                        {
                            stateenum = this.LotOPModule().InnerState.GetModuleState();
                        }

                        GPLoaderService?.SetStageState(ChuckID.Index, stateenum, isBuzzerOn);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }



        #endregion

        #region => CommandToken


        private CommandSlot _CommandRecvSlot = new CommandSlot();
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set { _CommandRecvSlot = value; }
        }

        private CommandSlot _CommandProcSlot = new CommandSlot();
        public CommandSlot CommandRecvProcSlot
        {
            get { return _CommandProcSlot; }
            set { _CommandProcSlot = value; }
        }

        private CommandSlot _CommandRecvDoneSlot = new CommandSlot();
        public CommandSlot CommandRecvDoneSlot
        {
            get { return _CommandRecvDoneSlot; }
            set { _CommandRecvDoneSlot = value; }
        }


        public CommandTokenSet RunTokenSet { get; set; }
        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        public bool CanExecute(IProbeCommandToken token)
        {


            bool isInjected;
            isInjected = ControllerState.CanExecute(token);
            return isInjected;
        }
        #endregion

        #region => ILoaderService Callback
        [MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum OnLoaderParameterChanged(LoaderSystemParameter systemParam, LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.LoaderSystemParam = systemParam;
                this.LoaderDeviceParam = deviceParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum OnLoaderInfoChanged(LoaderInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.LoaderInfo = info;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"OnLoaderEventRaised() : " + ex.Message);
                LoggerManager.Exception(err);

                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum WaferIDChanged(int slotNum, string ID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //this.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value = ID;
                this.LotOPModule().LotInfo.SetWaferID(slotNum, ID);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Error($"WaferIDChanged() : {err.Message}");
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum WaferHolderChanged(int slotNum, string holder)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.LotOPModule().LotInfo.SetHolder(slotNum, holder);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Error($"WaferHolderChanged() : {err.Message}");
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum WaferStateChanged(int slotNum, EnumSubsStatus waferStatus, EnumWaferState waferState)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.LotOPModule().LotInfo.SetWaferState(slotNum, waferStatus, waferState);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Error($"WaferStateChanged() : {err.Message}");
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum WaferSwapChanged(int originSlotNum, int changeSlotNum, bool isInit)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.LotOPModule().LotInfo.WaferSwapChanged(originSlotNum, changeSlotNum, isInit);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Error($"WaferStateChanged() : {err.Message}");
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }



        [MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum CSTInfoChanged(LoaderInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //TODO Change
                WaferSummary waferObject = new WaferSummary();
                foreach (var holder in info.StateMap.CassetteModules[0].SlotModules)
                {
                    waferObject.WaferStatus = holder.WaferStatus;
                    if (holder.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        waferObject.WaferState = EnumWaferState.UNPROCESSED;
                    }
                    else
                    {
                        waferObject.WaferState = EnumWaferState.UNDEFINED;
                    }
                    waferObject.SlotNumber = holder.ID.Index;

                    this.LotOPModule().LotInfo.UpdateWafer(waferObject);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Error($"CSTInfoChanged() : {err.Message}");
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        public EnumSubsStatus GetChuckWaferStatus()
        {

            return this.StageSupervisor().WaferObject.GetStatus();
        }

        public bool IsHandlerholdWafer()
        {
            return this.StageSupervisor().StageModuleState.IsHandlerholdWafer();
        }
        public EnumWaferType GetWaferType()
        {
            return this.StageSupervisor().WaferObject.GetWaferType();
        }

        public EventCodeEnum ClearHandlerStatus()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.StageSupervisor().CheckUsingHandler())
                {
                    //this.StageSupervisor().WaferObject.SetStatusUnloaded();
                    SetRecoveryMode(false);
                    retVal = this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Error($"CSTInfoChanged() : {err.Message}");
            }
            return retVal;
        }

        public EnumSubsStatus UpdateCardStatus(out EnumWaferState cardState)
        {
            cardState = EnumWaferState.UNPROCESSED;
            SequenceBehavior command;
            ProberInterfaces.SequenceRunner.IBehaviorResult retVal;
            var cardstatus = EnumSubsStatus.UNDEFINED;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    command = new GP_CheckCardIsInStage();//=> Prober Card Pod에 Prober Card가 있는지 확인
                    retVal = command.Run().Result;

                    if (retVal.ErrorCode == EventCodeEnum.NONE)
                    {
                        cardState = EnumWaferState.READY;
                        return EnumSubsStatus.EXIST;
                    }
                    else if (retVal.ErrorCode == EventCodeEnum.GP_CardChange_CHECK_TO_CARD_UP_MOUDLE)
                    {
                        cardState = EnumWaferState.UNPROCESSED;
                        return EnumSubsStatus.EXIST;
                    }
                    break;
                case EnumCardChangeType.CARRIER:
                    bool zifLock = true;
                    // tester - mb connection check 
                    var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITH_MBLOCK, true, 500, 1000);
                    if (intret != 0)
                    {
                        zifLock = false;
                    }
                    else
                    {
                        // mb - pcard connection check
                        intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITH_PBUNLOCK, false, 500, 1000);
                        if (intret != 0)
                        {
                            zifLock = false;
                        }
                    }
                    if ((this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsCardExist
                        && (this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsDocked
                        && (GetProbeCardID() != null || GetProbeCardID() != ""))
                    {
                        cardstatus = EnumSubsStatus.EXIST;

                        if (zifLock)
                        {
                            cardState = EnumWaferState.READY;
                        }
                        else
                        {
                            cardState = EnumWaferState.PROCESSED;// 카드일수도 카드홀더 일수도 있음. 카드ID를 봐야함.
                        }
                        return cardstatus;
                    }
                    else if ((this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsCardExist
                        && (this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsDocked == false)
                    {
                        cardState = EnumWaferState.UNPROCESSED;
                        cardstatus = EnumSubsStatus.EXIST;
                        return cardstatus;
                    }
                    else if ((this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsCardExist
                              && (this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsDocked
                              && (GetProbeCardID() == null || GetProbeCardID() == ""))
                    {
                        cardState = EnumWaferState.PROCESSED; // 카드일수도 카드홀더 일수도 있음. 카드ID를 봐야함.
                        cardstatus = EnumSubsStatus.EXIST;
                        return cardstatus;
                    }
                    else
                    {
                        LoggerManager.Debug($"GetCardStatus(): The card status of the current equipment is invalid." +
                            $"\nIsCardExist = {(this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsCardExist}" +
                            $"\nIsDocked = {(this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsDocked}" +
                            $"\nGetProbeCardID() = {GetProbeCardID()}" +
                            $"\ncardState = {cardState}" +
                            $"\nGetProbeCardID() = {GetProbeCardID()}"
                            );
                    }
                    LoggerManager.Debug($"GetCardStatus(): The card status of the current equipment is invalid." +
                        $"\nIsCardExist = {(this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsCardExist}" +
                        $"\nIsDocked = {(this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsDocked}" +
                        $"\nGetProbeCardID() = {GetProbeCardID()}" +
                        $"\ncardState = {cardState}" +
                        $"\nGetProbeCardID() = {GetProbeCardID()}"
                        );
                    break;
                default:
                    break;
            }


            return EnumSubsStatus.NOT_EXIST;
        }

        public bool GetMachineInitDoneState()
        {
            bool isdone = false;
            try
            {
                isdone = this.MonitoringManager()?.IsMachineInitDone ?? false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return isdone;
        }

        public EnumCardChangeType GetCardChangeType()
        {
            EnumCardChangeType retVal = EnumCardChangeType.UNDEFINED;
            try
            {
                retVal = this.CardChangeModule().GetCCType();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum GetCardIDValidateResult(string CardID, out string Msg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            Msg = "";
            try
            {
                retVal = this.CardChangeModule().CardIDValidate(CardID, out Msg);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void UI_ShowLoaderCam()
        {
            try
            {
                IProberStation prober = this.ProberStation();
                //prober.ProberViewModel.LoaderScreenToLotScreen();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($$"{nameof(UI_ShowLoaderCam)}() : err={ex.Message}");
                LoggerManager.Exception(err);

            }
        }

        public void UI_HideLoaderCam()
        {
            try
            {
                IProberStation prober = this.ProberStation();
                //prober.ProberViewModel.HiddenLoaderScreenToLotScreen();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($$"{nameof(UI_ShowLoaderCam)}() : err={ex.Message}");
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum FOUP_FoupCoverUp(int cassetteNumber)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.FoupOpModule().GetFoupController(cassetteNumber).Execute(new FoupCoverUpCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum FOUP_FoupCoverDown(int cassetteNumber)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.FoupOpModule().GetFoupController(cassetteNumber).Execute(new FoupCoverDownCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum FOUP_FoupTiltDown(int cassetteNumber)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.FoupOpModule().GetFoupController(cassetteNumber).Execute(new FoupTiltDownCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum FOUP_FoupTiltUp(int cassetteNumber)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.FoupOpModule().GetFoupController(cassetteNumber).Execute(new FoupTiltUpCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public FoupModuleInfo FOUP_GetFoupModuleInfo(int cassetteNumber)
        {
            FoupModuleInfo retVal = null;
            try
            {
                retVal = this.FoupOpModule().GetFoupController(cassetteNumber).GetFoupModuleInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum FOUP_MonitorForWaferOutSensor(int cassetteNumber, bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.FoupOpModule().GetFoupController(cassetteNumber).MonitorForWaferOutSensor(value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo)
        {
            EventCodeEnum retval = EventCodeEnum.NOTIFY_ERROR;
            try
            {
                retval = this.StageCommunicationManager().NotifyStageAlarm(noticeCodeInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.NOTIFY_ERROR;

            }
            return retval;
        }

        public void NotifyReasonOfError(string errmsg)
        {
            try
            {
                if (GPLoaderService != null)
                {
                    GPLoaderService.NotifyReasonOfError(errmsg);
                }
                else
                {
                    LoggerManager.Error($"[GP_LoaderController], NotifyReasonOfError() : GPLoaderService is null. errmsg = {errmsg}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetTitleMessage(int cellno, string message, string foreground = "", string background = "")
        {
            try
            {
                GPLoaderService?.SetTitleMessage(cellno, message, foreground, background);
                //SetDeviceName(cellno, this.FileManager().GetDeviceName());
                this.NotifyManager().SetLastStageMSG(message);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void SetStageLock(StageLockMode mode)
        {
            try
            {
                GPLoaderService?.SetStageLock(this.GetChuckIndex(), mode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetForcedDoneState()
        {
            try
            {
                if (CheckForcedDoneState())
                {
                    if (this.ForcedDone != EnumModuleForcedState.ForcedDone)
                    {
                        this.ForcedDone = EnumModuleForcedState.ForcedDone;
                        GPLoaderService?.SetForcedDoneMode(this.GetChuckIndex(), EnumModuleForcedState.ForcedDone);
                    }
                }
                else
                {
                    if (this.ForcedDone != EnumModuleForcedState.Normal)
                    {
                        this.ForcedDone = EnumModuleForcedState.Normal;

                        GPLoaderService?.SetForcedDoneMode(this.GetChuckIndex(), EnumModuleForcedState.Normal);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool CheckForcedDoneState()
        {
            bool retVal = false;
            try
            {
                var waferForcedDone = this.WaferAligner().ForcedDone;
                var pinForcedDone = this.PinAligner().ForcedDone;
                var soakingForcedDone = this.SoakingModule().ForcedDone;
                var polishForcedDone = this.PolishWaferModule().ForcedDone;
                var pmiForcedDone = this.PMIModule().ForcedDone;

                if (waferForcedDone == EnumModuleForcedState.ForcedDone ||
                    pinForcedDone == EnumModuleForcedState.ForcedDone ||
                    soakingForcedDone == EnumModuleForcedState.ForcedDone ||
                    polishForcedDone == EnumModuleForcedState.ForcedDone ||
                    pmiForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = false;
            }
            return retVal;
        }
        public void GetTempModuleState()
        {
            this.TempController().GetModuleMessage();
        }

        object lockobj = new object();
        public void SetLotLogMessage(string message, int idx, ModuleLogType ModuleType, StateLogType State)
        {
            try
            {
                lock (lockobj)
                {
                    GPLoaderService?.SetActionLogMessage(message, idx, ModuleType, State);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetParamLogMessage(string message, int idx)
        {
            try
            {
                lock (lockobj)
                {
                    GPLoaderService?.SetParamLogMessage(message, idx);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetDeviceName(int cellno, string deviceName)
        {
            try
            {
                GPLoaderService?.SetDeviceName(cellno, deviceName);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetDeviceLoadResult(bool result)
        {
            try
            {
                GPLoaderService?.SetDeviceLoadResult(GetChuckIndex(), result);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public byte[] GetBytesFoupObjects()
        {
            byte[] retval = null;

            try
            {
                retval = GPLoaderService?.GetBytesFoupObjects();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion

        #region OpusV 함수
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            try
            {
                msg = "";
                return true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum OFR_SetOcrID(string inputOCR)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum OFR_OCRRemoteEnd()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum OFR_OCRFail()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetWaferInfo(IWaferInfo waferInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = ModifyWaferSize(this.LoaderDeviceParam, waferInfo);

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = UpdateDeviceParam(LoaderDeviceParam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        private EventCodeEnum ModifyWaferSize(LoaderDeviceParameter LoaderDeviceParam, IWaferInfo waferInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                SubstrateSizeEnum size = SubstrateSizeEnum.UNDEFINED;
                size = WaferSizeToSubstracteSizeConverter(waferInfo?.WaferSize ?? EnumWaferSize.UNDEFINED);
                foreach (var loaderModule in LoaderDeviceParam.CassetteModules)
                    loaderModule.AllocateDeviceInfo.Size.Value = size;
                foreach (var loaderModule in LoaderDeviceParam.FixedTrayModules)
                    loaderModule.AllocateDeviceInfo.Size.Value = size;
                foreach (var loaderModule in LoaderDeviceParam.InspectionTrayModules)
                    loaderModule.AllocateDeviceInfo.Size.Value = size;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private static SubstrateSizeEnum WaferSizeToSubstracteSizeConverter(EnumWaferSize waferSize)
        {
            SubstrateSizeEnum size = SubstrateSizeEnum.UNDEFINED;
            try
            {
                if (waferSize == EnumWaferSize.INCH6)
                    size = SubstrateSizeEnum.INCH6;
                else if (waferSize == EnumWaferSize.INCH8)
                    size = SubstrateSizeEnum.INCH8;
                else if (waferSize == EnumWaferSize.INCH12)
                    size = SubstrateSizeEnum.INCH12;
                else
                    size = SubstrateSizeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return size;
        }
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new LoaderDeviceParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderDeviceParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    this.LoaderDeviceParam = tmpParam as LoaderDeviceParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveDevParameter() // Don`t Touch
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(LoaderDeviceParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public LoaderDeviceParameter GetLoaderDeviceParameter()
        {
            return this.LoaderDeviceParam;
        }

        public EventCodeEnum LoaderSystemInit()
        {
            EventCodeEnum rel = EventCodeEnum.NONE;



            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
        public Autofac.IContainer GetLoaderContainer()
        {
            //Direct에서만 가능
            return null;
        }
        public EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;


            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }
        public EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }
        public EventCodeEnum SaveSystemParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }
        public EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum SaveDeviceParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }
        public EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }
        public EventCodeEnum OCRUaxisRelMove(double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum OCRWaxisRelMove(double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum WaitForCommandDone()
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
        public EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum RetractAll()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public LoaderMapEditor GetLoaderMapEditor()
        {
            LoaderMapEditor retVal = null;

            try
            {
                retVal = ControllerState.GetLoaderMapEditor();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void OnFoupModuleStateChanged(FoupModuleInfo info)
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void RaiseWaferOutDetected(int foupNumber)
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool IsFoupUsingByLoader(int foupNumber)
        {
            bool retVal = false;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        internal LoaderControllerStateEnum StateWhenPaused { get; set; }
        private SubStepModules _SubModules;
        public ISubStepModules SubModules
        {
            get { return _SubModules; }
            set
            {
                if (value != _SubModules)
                {
                    _SubModules = (SubStepModules)value;

                }
            }
        }
        private bool _LotEndCassetteFlag;
        public bool LotEndCassetteFlag
        {
            get { return _LotEndCassetteFlag; }
            set
            {
                if (value != _LotEndCassetteFlag)
                {
                    _LotEndCassetteFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        public VirtualStageConnector.IVirtualStageConnector VirtualConnector
        {
            get => VirtualStageConnector.VirtualStageConnector.Instance;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                LoaderControllerStateEnum state = (InnerState as GP_LoaderControllerState).GetState();

                retval = state.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            bool retVal = false;
            try
            {
                foreach (var subModule in SubModules.SubModules)
                {
                    if (subModule.GetMovingState() == MovingStateEnum.MOVING)
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetLotEndFlag(bool flag)
        {

        }
        public bool GetLotEndFlag()
        {
            return false;
        }
        private bool _FoupTiltIgoreFlag = false;
        public bool FoupTiltIgoreFlag
        {
            get { return _FoupTiltIgoreFlag; }
            set
            {
                if (value != _FoupTiltIgoreFlag)
                {
                    _FoupTiltIgoreFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public int GetLoadableAllocatedWaferCount(LoaderMap map)
        {
            int ret = -1;

            try
            {
                List<StageLotInfo> lotinfos = this.LotOPModule().LotInfo.GetLotInfos();

                if (map != null)
                {
                    ret = 0;

                    foreach (var lotinfo in lotinfos)
                    {
                        if (lotinfo.IsLotStarted)// Lot Start를 받은 Slot에 대해서만 count
                        {
                            // 할당된 Slot 중 현재 위치가 Slot에 있고 Unprocessed 인 웨이퍼 개수 + 할당된 Slot 중 Unprocessed 이고 현재 위치가 Chuck에 있지 않은 웨이퍼 개수 
                            var wafers = map.GetTransferObjectAll().Where(w =>
                                                                  (w.WaferType.Value == EnumWaferType.STANDARD || w.WaferType.Value == EnumWaferType.POLISH || w.WaferType.Value == EnumWaferType.TCW) &&
                                                                  (w.CST_HashCode ?? "").Equals(lotinfo.CassetteHashCode ?? "") &&
                                                                  w.WaferState == EnumWaferState.UNPROCESSED && //TODO: 할당되지 않은 Slot은 Skip 상태임을 확인하기.
                                                                  w.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK);
                            if (wafers != null)
                            {
                                ret += wafers.Count();
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[GPLoaderController] GetLoadableAllocatedWaferCount(): map is null");
                }
            }
            catch (Exception err)
            {
                ret = -1;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public void UpdateIsNeedLotEnd(LoaderInfo loaderInfo)
        {
            try
            {
                List<StageLotInfo> lotinfos = this.LotOPModule().LotInfo.GetLotInfos();

                if (lotinfos.Where(w => w.IsLotStarted == true).Count() == 0)
                {
                    this.LotOPModule().IsNeedLotEnd = false;
                }
                else
                {
                    int loadableWaferCount = GetLoadableAllocatedWaferCount(loaderInfo.StateMap);
                    bool isLotEndReady = IsLotEndReady();
                    EnumSubsStatus wafersubsStatus = this.GetParam_Wafer().GetStatus();
                    ModuleStateEnum wafertransferstate = this.WaferTransferModule().ModuleState.GetState();

                    if (loadableWaferCount == 0 && isLotEndReady && wafersubsStatus == EnumSubsStatus.NOT_EXIST && wafertransferstate == ModuleStateEnum.IDLE)
                    {
                        this.LotOPModule().IsNeedLotEnd = true;// Polish wafer 까지 완료된 후 LotEnd가 되어야함.

                    }
                    else
                    {
                        this.LotOPModule().IsNeedLotEnd = false;// 이 시점에 Clear 필요.
                    }
                }
            }
            catch (Exception err)
            {
                this.LotOPModule().IsNeedLotEnd = false;
                LoggerManager.Exception(err);
            }

        }

        public LoaderMap RequestJob(LoaderInfo loaderInfo, out bool isExchange, out bool isNeedWafer, out bool isTempReady, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            //[FOUP_SHIFT]*
            //Try-catch catch를 null로 반환
            LoaderMap retVal = null;
            isExchange = false;
            isNeedWafer = false;
            isTempReady = false;
            cstHashCodeOfRequestLot = "";
            ReqMap = loaderInfo.StateMap;
            //bool isNotExistFoup = false;

            try
            {
                // Machine InitDone 이 안된 상태에서는 LoaderMap을 만들지 않도록 한다.
                if (!this.MonitoringManager().IsMachineInitDone)
                {
                    return null;
                }

                var ChuckModule = loaderInfo.StateMap.ChuckModules.Where(item => item.ID == ChuckID).FirstOrDefault();

                //TODO: 더 적절한 위치가 있는 지 확인 필요.
                //UpdateIsNeedLotEnd();

                if (loaderInfo.FoupShiftMode == 1)
                {
                    // [FOUP_SHIFT]
                    // FoupShift UnloadHolder를 결정해줘야함. DynamicFoup이랑 함수분리.
                    // 점유하지 않은 카세트 && 빈 슬롯으로 보내기                 
                    if (ChuckModule.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        if (ChuckModule.Substrate.WaferType.Value == EnumWaferType.STANDARD)// && this.LotOPModule().UnloadFoupNumber != 0)// UnloadFoupNumber Triggered by DY Window TESTEND button
                        {
                            bool isCanCstUnlaod = true;
                            bool canUseCst = false;

                            var pairWafers = loaderInfo.StateMap.GetTransferObjectAll().Where(x => x.CST_HashCode == ChuckModule.Substrate.CST_HashCode &&
                                                                                                    x.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                                                                                                    ChuckModule.Substrate.ID.Value != x.ID.Value);// 자기 자신을 제외하고 pairWafer를 검색.
                            if (pairWafers.Count() != pairWafers.Where(p => p.WaferState != EnumWaferState.UNPROCESSED).Count())
                            {
                                isCanCstUnlaod = false;
                            }

                            //foreach (var wafer in pairWafers)
                            //{
                            //    if (wafer.WaferState != EnumWaferState.PROCESSED || wafer.WaferState != EnumWaferState.SKIPPED)
                            //    {
                            //        isCanCstUnlaod = false;
                            //    }
                            //}

                            if (isCanCstUnlaod == true)
                            {
                                foreach (var acFoupList in loaderInfo.StateMap.ActiveFoupList)
                                {
                                    var cstModule = loaderInfo.StateMap.CassetteModules[acFoupList - 1];

                                    //페어 웨이퍼 카세트 안에 없고                                    
                                    ////언로드 할 수 있는 상태의 카세트 있고                                  
                                    ////슬랏 있고                                 
                                    ////다른 웨이퍼가 점유하지 않고                               
                                    //// 슬랏 검사 후 언로드홀더 지정


                                    var unloadCST = ReqMap.IsUnloadCassetteExist(ChuckModule.Substrate);
                                    canUseCst = unloadCST.isExist;
                                    if (canUseCst)
                                    {
                                        ChuckModule.Substrate.UnloadHolder = unloadCST.cst.SlotModules.FirstOrDefault(item => item.ID.Index == unloadCST.slotIdx).ID;
                                    }
                                }
                            }

                            FixedTrayModuleInfo unloadFixedTray = null;
                            // 카세트에 못넣는다면 FixedTray에 넣어야함
                            if (canUseCst != true)
                            {
                                //사용가능한 Fixed Tray 찾고
                                unloadFixedTray = loaderInfo.StateMap.FixedTrayModules.Where(x => x.CanUseBuffer == true && x.WaferStatus != EnumSubsStatus.EXIST).FirstOrDefault();

                                //넣어!
                                if (unloadFixedTray != null)
                                {
                                    ChuckModule.Substrate.UnloadHolder = unloadFixedTray.ID;
                                }
                            }

                            // Fixed Tray에도 못넣는 상황이라면? 아무것도 못하는거지 뭐
                            if (canUseCst != true && unloadFixedTray == null)
                            {
                                //ChuckModule.Substrate.UnloadHolder = ChuckModule.ID;

                                isExchange = false;
                                isNeedWafer = false;
                                isTempReady = false;
                                return null;
                            }
                        }
                    }
                }
                else if (loaderInfo.DynamicMode == 1) // Dynamic
                {
                    if (ChuckModule.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        if (ChuckModule.Substrate.WaferType.Value == EnumWaferType.STANDARD && this.LotOPModule().UnloadFoupNumber != 0)
                        {
                            var cstModule = loaderInfo.StateMap.CassetteModules.Where(item => item.ID.Index == this.LotOPModule().UnloadFoupNumber).FirstOrDefault();
                            if (cstModule.FoupState == FoupStateEnum.LOAD && cstModule.ScanState == CassetteScanStateEnum.READ)
                            {
                                bool isSameHashCode = false;
                                if (cstModule.CST_HashCode != "" && cstModule.CST_HashCode == ChuckModule.Substrate.CST_HashCode)
                                {
                                    var unloadOriginWafer = cstModule.SlotModules.Where(
                                          item =>
                                          item.WaferStatus == EnumSubsStatus.NOT_EXIST && item.ID.Index == ChuckModule.Substrate.OriginHolder.Index
                                          ).FirstOrDefault();
                                    if (unloadOriginWafer != null)
                                    {
                                        ChuckModule.Substrate.UnloadHolder = unloadOriginWafer.ID;
                                        isSameHashCode = true;
                                        LoggerManager.Debug("unload wafer is same origin holder");
                                    }
                                    else
                                    {
                                        isSameHashCode = false;
                                        LoggerManager.Debug("unload wafer is not matched origin holder");
                                    }
                                }
                                if (!isSameHashCode)
                                {
                                    var unloadWafers = cstModule.SlotModules.Where(
                                         item =>
                                         item.WaferStatus == EnumSubsStatus.NOT_EXIST
                                         ).ToList();
                                    var unloadWafer = unloadWafers.OrderBy(item => item.ID.Index).FirstOrDefault();
                                    ChuckModule.Substrate.UnloadHolder = unloadWafer.ID;
                                }
                            }
                            else
                            {
                                //isNotExistFoup = true;
                            }
                        }
                        else
                        {
                            if (IsAbort)
                            {
                                isExchange = false;
                                isNeedWafer = false;
                                isTempReady = false;

                                if (ChuckModule.Substrate.WaferType.Value == EnumWaferType.STANDARD)
                                {
                                    this.LotOPModule().UnloadFoupNumber = this.GetParam_Wafer().GetOriginFoupNumber();
                                }
                            }
                            else
                            {
                                isExchange = false;
                                isNeedWafer = false;
                                isTempReady = false;

                                if (ChuckModule.Substrate != null)
                                {
                                    ChuckModule.Substrate.UnloadHolder = ChuckModule.Substrate.OriginHolder;
                                }
                            }
                            return null;
                        }
                    }
                }
                else
                {
                    if (ChuckModule.Substrate != null)
                    {
                        ChuckModule.Substrate.UnloadHolder = ChuckModule.Substrate.OriginHolder;
                    }
                }


                if (this.GetParam_Wafer().GetStatus() == EnumSubsStatus.NOT_EXIST)
                {
                    var TCW_Wafers = loaderInfo.StateMap.GetTransferObjectAll().Where(
                           item =>
                           item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                           item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                           item.WaferType.Value == EnumWaferType.TCW &&
                           item.ReservationState == ReservationStateEnum.NONE &&
                           item.WaferState == EnumWaferState.UNPROCESSED &&
                           item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                           item.UsingStageList.Contains(ChuckID.Index)
                           ).ToList();
                    if (TCW_Wafers != null && TCW_Wafers.Count > 0)
                    {
                        if (this.StageSupervisor().Get_TCW_Mode() == TCW_Mode.OFF)
                        {
                            this.StageSupervisor().Set_TCW_Mode(true);
                        }
                    }
                }
                retVal = ControllerState.RequestJob(out isExchange, out isNeedWafer, out cstHashCodeOfRequestLot, canloadwafer);

                if (this.EnvModule().IsConditionSatisfied() != EventCodeEnum.NONE)
                {
                    retVal = null;
                    isExchange = false;
                    isNeedWafer = false;
                    isTempReady = false;
                }
                else
                {
                    isTempReady = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public TransferObject GetDeviceInfo()
        {
            TransferObject deviceInfo = new TransferObject();

            try
            {
                var wafer = this.GetParam_Wafer();
                deviceInfo.WaferType.Value = wafer.GetSubsInfo().WaferType;
                deviceInfo.Size.Value = (SubstrateSizeEnum)wafer.GetPhysInfo().WaferSizeEnum;
                deviceInfo.NotchAngle.Value = wafer.GetPhysInfo().NotchAngle.Value;
                deviceInfo.SlotNotchAngle.Value = wafer.GetPhysInfo().UnLoadingAngle.Value;
                deviceInfo.DeviceName.Value = this.FileManager().GetDeviceName();
                deviceInfo.NotchType = (WaferNotchTypeEnum)Enum.Parse(typeof(WaferNotchTypeEnum), wafer.GetPhysInfo().NotchType.Value.ToString(), true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return deviceInfo;
        }

        public ModuleID GetChuckID()
        {
            return ChuckID;
        }

        public ModuleID GetOriginHolder()
        {
            ModuleID retVal = new ModuleID();
            try
            {
                retVal = this.StageSupervisor().GetSlotInfo().OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool LotOPStart(int foupnumber, bool iscellstart = false, string lotID = "", string cstHashCode = "")
        {
            bool retVal = false;
            try
            {
                if ((retVal = IsCanPerformLotStart()))
                {
                    retVal = this.CommandManager().SetCommand<ILotOpStart>(this);

                    if (retVal)
                    {
                        //if (iscellstart)
                        //{
                        //    this.GEMModule().GetPIVContainer().SetBackupStageLotInfo(0, "", isstart: true);
                        //}

                        if (string.IsNullOrEmpty(lotID) == false)
                        {
                            this.LotOPModule().UpdateLotName(lotID);
                            if (foupnumber > 0)
                            {
                                this.LotOPModule().LotInfo.SetFoupInfo(foupnumber, cstHashCode);
                            }
                            else
                            {
                                this.LotOPModule().LotInfo.SetFoupInfo(this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotID), cstHashCode);
                            }
                            this.GEMModule().GetPIVContainer().FoupNumber.Value = this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotID);

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsNeedLotEnd()
        {
            try
            {
                return this.LotOPModule().IsNeedLotEnd;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return false;
        }

        public bool IsLotAbort()
        {
            try
            {
                return this.LoaderController().IsAbort;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return false;
        }

        public bool GetLotOutState()
        {
            try
            {
                return this.LoaderController().IsLotOut;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return false;
        }
        public bool IsLotEndReady()
        {
            bool noRemainJob = false;
            try
            {
                noRemainJob = !this.SequenceEngineManager().IsDoPolish();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"IsLotEndReady(): Error occurred. Err = {err.Message}");
                noRemainJob = true;
            }

            return noRemainJob;
        }

        //CELL END
        public bool LotOPEnd(int foupnumber = -1)
        {
            bool retVal = false;
            try
            {
                retVal = this.CommandManager().SetCommand<ILotOpEnd>(this);
                if (retVal)
                {
                    this.LotOPModule().LotInfo.NeedLotDeallocated = true;
                    this.LotOPModule().LotInfo.RemoveLotInfo(foupnumber);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetAbort(bool isAbort, bool isForced = false)
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IsAbort = isAbort;
                LoggerManager.Debug($"GP_LoaderController SetAbort execute {isAbort}");
                try
                {
                    //if (iscellend)
                    //{
                    //    IsCancel = true;
                    //    //if (IsCancel == false)
                    //    //{
                    //    //var lockobj = this.StageSupervisor().GetStagePIV().GetPIVDataLockObject();
                    //    //    lock (lockobj)
                    //    //    {
                    //    //        this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(NotifyEventModule.WaferTestingAborted).FullName));
                    //    //    }
                    //    //}
                    //}
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                if (IsAbort)
                {
                    if (this.StageSupervisor().GetParam_Wafer().WaferStatus == EnumSubsStatus.EXIST)
                    {
                        if (this.StageSupervisor().GetParam_Wafer().GetState() != EnumWaferState.PROCESSED)
                        {
                            if (this.StageSupervisor().GetParam_Wafer().GetWaferType() == EnumWaferType.STANDARD)
                            {
                                this.StageSupervisor().GetParam_Wafer().SetWaferState(EnumWaferState.SKIPPED);
                            }
                        }
                    }
                    //this.StageSupervisor().GetStagePIV().SetStageState(GEMStageStateEnum.IDLE);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetLotOut(bool islotout)
        {
            try
            {
                if (_IsLotOut != islotout)
                {
                    LoggerManager.Debug($"GP_LoaderController.SetLotOut(): {islotout}");
                }

                _IsLotOut = islotout;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public bool LotOPPause(bool isabort = false)
        {
            bool retVal = false;
            try
            {
                if (isabort == false)
                {
                    //LOT PAUSE 가능 조건
                    // 1. LOT 가 running 이여야 됨,
                    // 2. Probing 이 PINPADMATCHPERFORM, PINPADMATCHED, ZUP, ZUPPERFORM, ZUPDWELL 상태가 아니여야됨
                    if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RESUMMING)
                    {
                        this.ProbingModule().IsReservePause = true;
                        retVal = this.CommandManager().SetCommand<ILotOpPause>(this);
                    }
                    else if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT)
                    {
                        this.ProbingModule().IsReservePause = true;
                        retVal = this.CommandManager().SetCommand<ILotOpPause>(this);
                    }
                    else if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        if (this.ProbingModule().ProbingStateEnum != EnumProbingState.PINPADMATCHPERFORM
                           & this.ProbingModule().ProbingStateEnum != EnumProbingState.PINPADMATCHED
                           & this.ProbingModule().ProbingStateEnum != EnumProbingState.ZUP
                           & this.ProbingModule().ProbingStateEnum != EnumProbingState.ZUPPERFORM
                           & this.ProbingModule().ProbingStateEnum != EnumProbingState.ZUPDWELL)
                        {
                            this.ProbingModule().IsReservePause = true;
                            retVal = this.CommandManager().SetCommand<ILotOpPause>(this);
                        }
                        else
                        {
                            this.ProbingModule().IsReservePause = true;
                            retVal = true;
                        }
                    }
                    else
                    {
                        this.ProbingModule().IsReservePause = true;
                        retVal = true;
                    }
                }
                else
                {
                    retVal = this.CommandManager().SetCommand<ILotOpPause>(this);

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(CellLotAbortedByUser).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();

                    //semaphore = new SemaphoreSlim(0);
                    //this.EventManager().RaisingEvent(typeof(WaferTestingAbortedByUser).FullName, new ProbeEventArgs(this, semaphore));
                    //semaphore.Wait();
                }

                this.LotOPModule().ReasonOfError.AddEventCodeInfo(EventCodeEnum.PAUSED_BY_OTHERS,
                        "Pause by user.", this.GetType().Name);
                this.LotOPModule().PauseSourceEvent = this.LotOPModule().ReasonOfError.GetLastEventCode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool LotOPResume()
        {
            bool retVal = false;
            try
            {

                retVal = this.CommandManager().SetCommand<ILotOpResume>(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool CardAbort()
        {
            bool retVal = false;
            try
            {
                retVal = this.CommandManager().SetCommand<IAbortcardChange>(this.WaferTransferModule());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public ModuleStateEnum GetLotState()
        {
            return this.LotOPModule().ModuleState.GetState();
        }
        public StageLockMode GetStageLock()
        {
            StageLockMode lockmode = StageLockMode.UNLOCK;
            try
            {
                lockmode = this.StageSupervisor().GetStageLockMode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return lockmode;
        }
        public async Task<EventCodeEnum> ConnectLoaderService()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            Task task = new Task(() =>
            {
                InitLoaderService();
            });

            task.Start();
            await task;
            return retVal;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public void DisConnect()
        {
            try
            {
                lock (aliveLockObj)
                {
                    LoaderServiceProvider.DisConnect();
                }
                //cell에서 loader env, chiller host에 연결된 client proxy를 끊는다.
                this.EnvControlManager().DisConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetLoaderIP()
        {
            return ControllerParam.LoaderIP;
        }
        public int GetChuckIndex()
        {
            if (ControllerParam.LoaderServiceType == LoaderServiceTypeEnum.DynamicLinking)
            {
                return 0;
            }
            else
            {
                return ControllerParam.ChuckIndex;
            }
        }
        public string GetChuckIndexString()
        {
            if (ControllerParam.ChuckIndex >= 10)
            {
                return ControllerParam.ChuckIndex.ToString();
            }
            else
            {
                return "0" + ControllerParam.ChuckIndex.ToString();
            }
        }

        private object aliveLockObj = new object();
        public bool GetconnectFlag()
        {
            bool retval = false;

            try
            {
                lock (aliveLockObj) //cell의 여러 thread에서의 동시 접근을 막기 위함, IsServiceAvailable faulted 등에 의해 hang걸리는 상황에서 GPLoaderService null로 리턴되는 경우가 있으므로 lock 위치를 여기로 한다.
                {
                    IGPLoaderService channel = GPLoaderService;
                    if (channel != null) //GPLoaderService getter에서 channel 검증 수행 후 정상이면 service를 return 한다.
                    {
                        var originOperationTimeout = (channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 15);
                            channel?.IsServiceAvailable(); //oneway로 exception이 발생하지 않는 다면true 이다.
                            retval = true;
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"GPLoaderService IsServiceAvailable timeout error.");
                        }
                        finally
                        {
                            (channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GPServiceProxy IsServiceAvailable failed");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetActiveLotInfo(int foupNumber, string lotId, string cstHashCode, string carrierid)
        {
            try
            {
                this.LotOPModule().LotInfo.CreateLotInfo(foupNumber, lotid: lotId,
                    recipeid: this.FileManager().GetDeviceName(), isAssignLot: true, cstHashCode: cstHashCode, carrierid: carrierid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetCassetteHashCode(int foupNumber, string lotId, string cstHashCode)
        {
            try
            {
                this.LotOPModule().LotInfo.SetCassetteHashCode(foupNumber, lotId, cstHashCode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// LotModule에 이 Lot 정보가 LotStart를 받은 Lot인지에 대한 정보를 전달한다.
        /// </summary>
        /// <param name="foupNumber"></param>
        /// <param name="lotId"></param>
        /// <param name="cstHashCode"></param>
        public void SetLotStarted(int foupNumber, string lotId, string cstHashCode)
        {
            try
            {
                this.LotOPModule().LotInfo.SetLotStarted(foupNumber, lotId, cstHashCode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public string GetProbeCardID()
        {
            string retVal = "";
            try
            {
                retVal = this.CardChangeModule().GetProbeCardID();
                //   retVal = this.GetParam_ProbeCard().GetProbeCardID();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public string GetWaferID()
        {
            string retVal = "";
            try
            {
                retVal = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public int GetSlotIndex()
        {
            int retVal = 0;
            try
            {
                //retVal = this.GetParam_Wafer().GetPhysInfo().SlotIndex.Value;
                retVal = this.GetParam_Wafer().GetSubsInfo().SlotIndex.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetStageMode(GPCellModeEnum mode)
        {
            var ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = this.StageSupervisor().SetStageMode(mode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public void SetCellModeChanging()
        {
            try
            {
                this.StageSupervisor().IsModeChanging = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ResetCellModeChanging()
        {
            try
            {
                this.StageSupervisor().IsModeChanging = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetStreamingMode(StreamingModeEnum mode)
        {
            this.StageSupervisor().StreamingMode = mode;
        }
        public void SetForcedDone(EnumModuleForcedState flag)
        {
            this.AirBlowChuckCleaningModule().ForcedDone = flag;
            this.PinAligner().ForcedDone = flag;
            this.AirBlowWaferCleaningModule().ForcedDone = flag;
            this.WaferAligner().ForcedDone = flag;
            this.PMIModule().ForcedDone = flag;
            this.PolishWaferModule().ForcedDone = flag;
            this.SoakingModule().ForcedDone = flag;
            this.NeedleCleaner().ForcedDone = flag;
            this.NeedleBrush().ForcedDone = flag;
        }

        public void CellProbingResume()
        {
            this.CommandManager().SetCommand<IResumeProbing>(this.ProbingModule());
        }


        public void SetForcedDoneSpecificModule(EnumModuleForcedState flag, ModuleEnum moduleEnum)
        {
            try
            {
                if (moduleEnum == ModuleEnum.AirBlowChuckCleaning)
                {
                    this.AirBlowChuckCleaningModule().ForcedDone = flag;
                }

                if (moduleEnum == ModuleEnum.PinAlign)
                {
                    this.PinAligner().ForcedDone = flag;
                }

                if (moduleEnum == ModuleEnum.AirBlowWaferCleaning)
                {
                    this.AirBlowWaferCleaningModule().ForcedDone = flag;
                }

                if (moduleEnum == ModuleEnum.WaferAlign)
                {
                    this.WaferAligner().ForcedDone = flag;
                }

                if (moduleEnum == ModuleEnum.PMI)
                {
                    this.PMIModule().ForcedDone = flag;
                }

                if (moduleEnum == ModuleEnum.PolishWafer)
                {
                    this.PolishWaferModule().ForcedDone = flag;
                }

                if (moduleEnum == ModuleEnum.Soaking)
                {
                    this.SoakingModule().ForcedDone = flag;
                }

                if (moduleEnum == ModuleEnum.NeedleClean)
                {
                    this.NeedleCleaner().ForcedDone = flag;
                }

                if (moduleEnum == ModuleEnum.NeedleBrush)
                {
                    this.NeedleBrush().ForcedDone = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetStilProbingZUp(bool flag = true)
        {
            this.ProbingModule().StilProbingZUpFlag = flag;
        }


        public void SetLotLoadingPosCheckMode(bool flag)
        {
            this.WaferAligner().WaferAlignInfo.LotLoadingPosCheckMode = flag;
        }

        /// <summary>
        /// Cairrer_Cancel 시 들어옴.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum WaferCancel()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                this.isExchange = false;
                this.IsCancel = true;
                this.LotOPModule().End();

                if (this.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST &&
                    this.GetParam_Wafer().GetWaferType() == EnumWaferType.STANDARD)
                {
                    this.GetParam_Wafer().SetWaferState(EnumWaferState.SKIPPED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EnumWaferState GetWaferState()
        {
            EnumWaferState waferState = EnumWaferState.UNPROCESSED;
            try
            {
                if (this.GetParam_Wafer().WaferStatus == EnumSubsStatus.EXIST)
                {
                    waferState = this.GetParam_Wafer().GetState();
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return waferState;
        }
        public void SetWaferStateOnChuck(EnumWaferState waferstate)
        {
            try
            {
                if (this.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST &&
                    this.GetParam_Wafer().GetWaferType() == EnumWaferType.STANDARD)
                {
                    this.GetParam_Wafer().SetWaferState(waferstate);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EnumProbingState GetProbingState()
        {
            EnumProbingState probingState = EnumProbingState.IDLE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return probingState;
        }

        public bool GetRunState(bool isTransfer = false)
        {
            if (this.CardChangeModule().ModuleState.State == ModuleStateEnum.RUNNING)
            {
                return false;
            }
            return this.SequenceEngineManager().GetRunState(isWaferTransfer: isTransfer); //카드 체인저도 포함되어야한다.
        }
        public bool GetMovingState()
        {
            return this.SequenceEngineManager().GetMovingState();
        }

        public EventCodeEnum UpdateSoakingInfo(SoakingInfo soakinfo)
        {
            try
            {
                if (GetconnectFlag())
                {
                    GPLoaderService?.UpdateSoakingInfo(soakinfo);
                    return EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.UNKNOWN_EXCEPTION;
        }

        public void UpdateLotVerifyInfo(int portnum, bool flag)
        {
            if (GPLoaderService != null)
            {
                try
                {
                    GPLoaderService?.IsServiceAvailable();
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], UpdateLotVerifyInfo() : flag: {flag}");

                        GPLoaderService?.UpdateLotVerifyInfo(portnum, GetChuckIndex(), flag);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public void UpdateLogUploadList(EnumUploadLogType type)
        {
            if (GPLoaderService != null)
            {
                try
                {
                    GPLoaderService?.IsServiceAvailable();
                    {
                        LoggerManager.Debug($"GP_LoaderController.UpdateLogUploadList() type: {type}");
                        GPLoaderService?.UpdateLogUploadList(GetChuckIndex(), type);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public void UpdateDownloadMapResult(bool flag)
        {
            if (GPLoaderService != null)
            {
                try
                {
                    GPLoaderService?.IsServiceAvailable();
                    {
                        LoggerManager.Debug($"GP_LoaderController.UpdateDownloadMapResult() flag: {flag}");
                        GPLoaderService?.UpdateDownloadMapResult(GetChuckIndex(), flag);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public void UpdateTesterConnectedStatus(bool flag)
        {
            if (GPLoaderService != null)
            {
                try
                {
                    GPLoaderService?.IsServiceAvailable();
                    {
                        GPLoaderService?.UpdateTesterConnectedStatus(GetChuckIndex(), flag);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public void UpdateLotDataInfo(StageLotDataEnum type, string val)
        {
            try
            {
                if (GPLoaderService != null)
                {
                    try
                    {
                        GPLoaderService?.IsServiceAvailable();
                        {
                            LoggerManager.Debug($"GP_LoaderController.UpdateLotDataInfo() Stage[{GetChuckIndex()}] StageLotData({type}) : {val}");
                            GPLoaderService?.UpdateLotDataInfo(GetChuckIndex(), type, val);
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void UpdateStageMove(StageMoveInfo info)
        {
            try
            {
                if (GPLoaderService != null)
                {
                    try
                    {
                        GPLoaderService?.IsServiceAvailable();
                        {
                            GPLoaderService?.UpdateStageMove(info);
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public EnumWaferType GetActiveLotWaferType(string lotid)//lot id 말고 다른 값은 없을까..
        {
            EnumWaferType retVal = EnumWaferType.UNDEFINED;
            try
            {
                if (GPLoaderService != null)
                {
                    retVal = GPLoaderService.GetActiveLotWaferType(lotid);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        public EventCodeEnum UploadCardPatternImages(byte[] data, string filename, string devicename, string cardid)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                GPLoaderService.UploadCardPatternImages(data, filename, devicename, cardid);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum UploadProbeCardInfo(ProberCardListParameter probeCard)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                GPLoaderService.UploadProbeCardInfo(probeCard);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public ProberCardListParameter DownloadProbeCardInfo(string cardID)
        {
            ProberCardListParameter retVal = null;
            try
            {
                retVal = GPLoaderService.DownloadProbeCardInfo(cardID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ODTPUpload(int stageindex, string filename)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                GPLoaderService.ODTPUpload(stageindex, filename);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"Error occurred while upload Result Map to loader");
            }
            return retVal;
        }
        public EventCodeEnum ResultMapUpload(int stageindex, string filename)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                GPLoaderService.ResultMapUpload(stageindex, filename);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"Error occurred while upload Result Map to loader");
            }
            return retVal;
        }

        public EventCodeEnum ResultMapDownload(int stageindex, string filename)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                GPLoaderService.ResultMapDownload(stageindex, filename);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"Error occurred while Download Result Map to loader");
            }
            return retVal;
        }


        public List<CardImageBuffer> DownloadCardPatternImages(string devicename, int downimgcnt, string cardid)
        {
            List<CardImageBuffer> ret = new List<CardImageBuffer>();
            try
            {
                ret = GPLoaderService.DownloadCardPatternImages(devicename, downimgcnt, cardid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public bool IsCardUpModuleUp()
        {
            bool iscardupmodule = false;
            try
            {
                bool upmoduleLeft = false;
                bool upmoduleRight = false;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out upmoduleLeft);
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out upmoduleRight);
                if (upmoduleLeft | upmoduleRight)
                {
                    iscardupmodule = true;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"{this.LoaderController().GetChuckIndex()}Error occurred while IsCardUpModuleUp Function ");
            }
            return iscardupmodule;
        }

        public CatCoordinates GetPMShifhtValue()
        {
            CatCoordinates info = null;

            try
            {
                info = this.ProbingModule().GetPMShifhtValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return info;
        }

        public double GetSetTemp()
        {
            double retVal = 0.0;
            try
            {
                retVal = this.TempController().TempInfo.SetTemp.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public ModuleStateEnum GetSetSoakingState()
        {
            ModuleStateEnum retVal = ModuleStateEnum.IDLE;
            try
            {
                retVal = this.SequenceEngineManager().LotOPModule().SoakingModule().ModuleState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void LotCancelSoakingAbort(int stageindex)
        {
            try
            {
                this.SoakingModule().SetStatusSoakingForceTransitionState();
                if (stageInfo != null)
                {
                    stageInfo.SoakingZClearance = "N/A";
                    stageInfo.SoakingRemainTime = 0.ToString();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void IsAlignDoing(ref bool pinAlignDoing, ref bool waferAlignDoing)
        {
            pinAlignDoing = true;
            waferAlignDoing = true;

            try
            {
                var PinModuleState = this.SequenceEngineManager().LotOPModule().PinAligner().ModuleState.GetState();
                if (PinModuleState == ModuleStateEnum.IDLE ||
                    PinModuleState == ModuleStateEnum.PAUSED ||
                    PinModuleState == ModuleStateEnum.PAUSING ||
                    PinModuleState == ModuleStateEnum.ERROR ||
                    PinModuleState == ModuleStateEnum.RECOVERY
                    )
                {
                    pinAlignDoing = false;
                }

                var WaferModuleState = this.SequenceEngineManager().LotOPModule().WaferAligner().ModuleState.GetState();
                if (WaferModuleState == ModuleStateEnum.IDLE ||
                    WaferModuleState == ModuleStateEnum.PAUSED ||
                    WaferModuleState == ModuleStateEnum.PAUSING ||
                    WaferModuleState == ModuleStateEnum.ERROR ||
                    WaferModuleState == ModuleStateEnum.RECOVERY
                    )
                {
                    waferAlignDoing = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum ReserveErrorEnd(string ErrorMessage = "Paused by host(CELL ABORT TEST).")
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                IsCancel = true;
                this.LotOPModule().SetErrorEndFalg(true);   //셀쪽 플래그 true
                this.LotOPModule().ModuleStopFlag = true;
                this.LotOPModule().ErrorEndState = ErrorEndStateEnum.Reserve;

                this.LotOPModule().ReasonOfError.AddEventCodeInfo(EventCodeEnum.ERROR_END, ErrorMessage, this.GetType().Name);
                this.LotOPModule().PauseSourceEvent = this.LotOPModule().ReasonOfError.GetLastEventCode();

                this.ProbingModule().SetProbingEndState(ProbingEndReason.ERROR_NG);

                if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST)
                {
                    this.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SKIPPED);
                }

                LoggerManager.Debug($"GP_LoaderController ReserveErrorEnd() execute.  ModuleStopFlag:true");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public ErrorEndStateEnum GetErrorEndState()
        {
            ErrorEndStateEnum retVal = ErrorEndStateEnum.NONE;
            try
            {
                retVal = this.LotOPModule().ErrorEndState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetRecipeDownloadEnable(bool flag)
        {
            this.StageSupervisor().IsRecipeDownloadEnable = flag;
        }

        public void CancelCellReservePause()
        {
            try
            {
                if (this.ProbingModule().IsReservePause)
                {
                    this.ProbingModule().IsReservePause = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CancelLot(int foupnumber, bool iscellend, string lotID = "", string cstHashCode = "")
        {
            try
            {                
                // IsCancel = true;                
                this.LotOPModule().ValidateCancelLot(iscellend, foupnumber, lotID, cstHashCode);                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetReservePause()
        {
            bool retVal = false;
            try
            {
                retVal = this.ProbingModule().IsReservePause;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum CanWaferUnloadRecovery(ref bool canrecovery, ref ModuleStateEnum wafertransferstate)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                canrecovery = this.WaferTransferModule().NeedToRecovery;
                wafertransferstate = this.WaferTransferModule().ModuleState.GetState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetProbingStart(bool isStart)
        {

            if (GPLoaderService != null)
            {
                try
                {
                    GPLoaderService?.IsServiceAvailable();
                    {
                        GPLoaderService?.SetProbingStart(GetChuckIndex(), isStart);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }
        public void SetTransferError(bool isError)
        {

            if (GPLoaderService != null)
            {
                try
                {
                    GPLoaderService?.IsServiceAvailable();
                    {
                        if (isError)
                        {
                            this.StageSupervisor().SetStageMode(GPCellModeEnum.MAINTENANCE);
                        }
                        GPLoaderService?.SetTransferError(GetChuckIndex(), isError);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }
        public EventCodeEnum GetAngleInfo(out double notchAngle, out double slotAngle, out double ocrAngle)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            notchAngle = 0;
            slotAngle = 0;
            ocrAngle = 0;

            try
            {
                notchAngle = this.GetParam_Wafer().GetPhysInfo().NotchAngle.Value;
                slotAngle = this.GetParam_Wafer().GetPhysInfo().UnLoadingAngle.Value;
                //ocrAngle = this.GetParam_Wafer().GetPhysInfo().OCRAngle.Value;

                if (this.StageSupervisor() is StageSupervisor && (this.StageSupervisor() as StageSupervisor).OCRDevParam != null)
                {
                    ocrAngle = (this.StageSupervisor() as StageSupervisor).OCRDevParam.OCRAngle;
                }

                //this.DeviceManager().DetachDeviceFolderName;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum GetNotchTypeInfo(out WaferNotchTypeEnum notchType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            notchType = WaferNotchTypeEnum.NOTCH;
            try
            {
                notchType = (WaferNotchTypeEnum)Enum.Parse(typeof(WaferNotchTypeEnum), this.GetParam_Wafer().GetPhysInfo().NotchType.Value.ToString(), true);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        /// <summary>
        /// 
        /// </summary>
        public EventCodeEnum NotifyLotEnd(int foupNumber, string lotID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                string allocatedLotID = this.LotOPModule().LotInfo.GetLotIDAtStageLotInfos(foupNumber);
                int allocatedFoupNumber = this.GEMModule().GetPIVContainer().FoupNumber.Value;
               
                if (allocatedLotID != null)
                {
                    if (allocatedLotID != "" && allocatedFoupNumber != 0)
                    {
                        LotAssignStateEnum lotAssignStateEnum = this.LotOPModule().LotInfo.GetStageLotAssignState(allocatedLotID);

                        if (allocatedLotID.Equals(lotID) && allocatedFoupNumber == foupNumber && lotAssignStateEnum == LotAssignStateEnum.PROCESSING)
                        {
                            this.LotOPModule().LotInfo.SetStageLotAssignState(LotAssignStateEnum.JOB_FINISHED, allocatedLotID);

                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(StageDeallocatedEvent).FullName, new ProbeEventArgs(this, semaphore));// #Hynix_Merge: 검토 필요, Hynix 브랜치에서는 LoadportActivatedEvent로 되어있었는데 이전에 잘못되어있던 건가?
                            semaphore.Wait();

                            LoggerManager.Debug($"NotifyLotEnd() - allocatedLotId : {allocatedLotID}, allocatedFoupNumber : {allocatedFoupNumber} ");
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum DoPinAlign()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.PinAligner().DoManualOperation();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool GetErrorEndFlag()
        {
            bool retval = false;

            try
            {
                retval = this.LotOPModule().GetErrorEndFlag();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void SetErrorEndFalg(bool flag)
        {
            try
            {
                if (this.LotOPModule().GetErrorEndFlag() == true && flag == false)
                {
                    var pivinfo = new PIVInfo() { FoupNumber = this.LotOPModule().GetParam_Wafer().GetOriginFoupNumber() };
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(StageReadyToEnterToProcessEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    //this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(StageReadyToEnterToProcessEvent).FullName));
                }
                this.LotOPModule().SetErrorEndFalg(flag);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetPauseReason()
        {
            string str = "";
            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    //ReasonOfError
                    var reasonOfError = this.LotOPModule().PauseSourceEvent;
                    if (reasonOfError != null)
                    {
                        str = reasonOfError.Message;
                    }
                    //var reasonOfError = this.LotOPModule().ReasonOfError.GetLastEventCode();
                    //if(reasonOfError != null)
                    //{
                    //    str = reasonOfError.EventCode.ToString();
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return str;
        }



        public EventCodeEnum OFR_OCRAbort()
        {
            throw new NotImplementedException();
        }

        public bool SetNoReadScanState(int cassetteNumber)
        {
            throw new NotImplementedException();
        }
        public void SetCardStatus(bool isExist, string id, bool isDocked = false)
        {
            var cardParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
            this.CardChangeModule().SetIsCardExist(isExist);
            if (isExist)
            {
                this.CardChangeModule().SetProbeCardID(id);
                this.CardChangeModule().SetIsDocked(isDocked);
            }
            else
            {
                this.CardChangeModule().SetProbeCardID("");
                this.CardChangeModule().SetIsDocked(false);
            }
            this.CardChangeModule().SaveSysParameter();
        }

        public bool GetStopBeforeProbingFlag()
        {
            bool retVal = false;

            retVal = GPLoaderService.GetStopBeforeProbingFlag(GetChuckIndex());

            return retVal;
        }

        public bool GetStopAfterProbingFlag()
        {
            bool retVal = false;

            retVal = GPLoaderService.GetStopAfterProbingFlag(GetChuckIndex());

            return retVal;
        }
        public EventCodeEnum SetRecoveryMode(bool isRecovery)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.GPLoaderService != null)
                {
                    retVal = GPLoaderService.SetRecoveryMode(this.ChuckID.Index, isRecovery);
                    this.StageSupervisor().IsRecoveryMode = isRecovery;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetStopBeforeProbingFlag(bool flag, int stageidx = 0)
        {
            try
            {
                if (this.GPLoaderService != null)
                {
                    GPLoaderService.SetStopBeforeProbingFlag(this.ChuckID.Index, flag);
                    LoggerManager.Debug($"Set Stop Before Probing to Loader : {flag}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetStopAfterProbingFlag(bool flag, int stageidx = 0)
        {
            try
            {
                if (this.GPLoaderService != null)
                {
                    GPLoaderService.SetStopAfterProbingFlag(this.ChuckID.Index, flag);
                    LoggerManager.Debug($"Set Stop After Probing to Loader : {flag}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetOnceStopBeforeProbingFlag(bool flag, int stageidx = 0)
        {
            try
            {
                if (this.GPLoaderService != null)
                {
                    GPLoaderService.SetOnceStopBeforeProbingFlag(this.ChuckID.Index, flag);
                    LoggerManager.Debug($"Set Once Stop Before Probing to Loader : {flag}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetOnceStopAfterProbingFlag(bool flag, int stageidx = 0)
        {
            try
            {
                if (this.GPLoaderService != null)
                {
                    GPLoaderService.SetOnceStopAfterProbingFlag(this.ChuckID.Index, flag);
                    LoggerManager.Debug($"Set Once Stop After Probing to Loader : {flag}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum IsShutterClose()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (GPLoaderService != null)
            {
                try
                {
                    retVal = GPLoaderService.IsShutterClose(GetChuckIndex());
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            else
            {
                retVal = EventCodeEnum.NONE;
            }
            return retVal;
        }
        public EventCodeEnum WriteWaitHandle(short value)
        {
            return this.GetGPLoader().WriteWaitHandle(value);
        }

        public EventCodeEnum WaitForHandle(short handle, long timeout = 60000)
        {
            return this.GetGPLoader().WaitForHandle(handle, timeout);
        }

        public List<ReasonOfStageMoveLock> GetReasonofLockFromClient()
        {
            List<ReasonOfStageMoveLock> retval = new List<ReasonOfStageMoveLock>();
            try
            {
                retval = this.StageSupervisor().IStageMoveLockStatus.LastStageMoveLockReasonList;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum StageMoveLockState(ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = this.StageSupervisor().StageModuleState.StageMoveLockState();
                retVal = this.StageSupervisor().SetStageLock(reason);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum StageMoveUnLockState(ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = this.StageSupervisor().StageModuleState.StageMoveUnLockState();
                retVal = this.StageSupervisor().SetStageUnlock(reason);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void ResetAssignLotInfo(int foupnumber, string lotid, string cstHashCode)
        {
            try
            {
                this.DeviceModule().ClearActiveDeviceDic(foupnumber, lotid, cstHashCode);
                this.LotOPModule().LotInfo.RemoveLotInfo(foupnumber, lotid, cstHashCode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ModuleEnum[] GetForcedDoneModules()
        {
            var ret = new List<ModuleEnum>();

            if (this.AirBlowChuckCleaningModule().ForcedDone == EnumModuleForcedState.ForcedDone)
                ret.Add(ModuleEnum.AirBlowChuckCleaning);
            if (this.PinAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                ret.Add(ModuleEnum.PinAlign);
            if (this.AirBlowWaferCleaningModule().ForcedDone == EnumModuleForcedState.ForcedDone)
                ret.Add(ModuleEnum.AirBlowWaferCleaning);
            if (this.WaferAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                ret.Add(ModuleEnum.WaferAlign);
            if (this.PMIModule().ForcedDone == EnumModuleForcedState.ForcedDone)
                ret.Add(ModuleEnum.PMI);
            if (this.PolishWaferModule().ForcedDone == EnumModuleForcedState.ForcedDone)
                ret.Add(ModuleEnum.PolishWafer);
            if (this.SoakingModule().ForcedDone == EnumModuleForcedState.ForcedDone)
                ret.Add(ModuleEnum.Soaking);
            if (this.NeedleCleaner().ForcedDone == EnumModuleForcedState.ForcedDone)
                ret.Add(ModuleEnum.NeedleClean);
            if (this.NeedleBrush().ForcedDone == EnumModuleForcedState.ForcedDone)
                ret.Add(ModuleEnum.NeedleBrush);

            return ret.ToArray();
        }

        public EnumModuleForcedState GetModuleForcedState(ModuleEnum m)
        {
            if (m == ModuleEnum.AirBlowChuckCleaning) return this.AirBlowChuckCleaningModule().ForcedDone;
            if (m == ModuleEnum.PinAlign) return this.AirBlowChuckCleaningModule().ForcedDone;
            if (m == ModuleEnum.AirBlowWaferCleaning) return this.AirBlowChuckCleaningModule().ForcedDone;
            if (m == ModuleEnum.WaferAlign) return this.AirBlowChuckCleaningModule().ForcedDone;
            if (m == ModuleEnum.PMI) return this.AirBlowChuckCleaningModule().ForcedDone;
            if (m == ModuleEnum.PolishWafer) return this.AirBlowChuckCleaningModule().ForcedDone;
            if (m == ModuleEnum.Soaking) return this.AirBlowChuckCleaningModule().ForcedDone;
            if (m == ModuleEnum.NeedleClean) return this.AirBlowChuckCleaningModule().ForcedDone;
            if (m == ModuleEnum.NeedleBrush) return this.AirBlowChuckCleaningModule().ForcedDone;

            return EnumModuleForcedState.Normal;
        }

        public ModuleStateEnum GetSoakingModuleState()
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            try
            {
                //retVal = this.StageSupervisor().StageModuleState.StageMoveUnLockState();
                retVal = this.SoakingModule().ModuleState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public void SetTCW_Mode(TCW_Mode tcw_Mode)
        {
            try
            {
                GPLoaderService?.SetTCW_Mode(this.GetChuckIndex(), tcw_Mode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ChangeWaferStatus(EnumSubsStatus status, out bool iswaferhold, out string errormsg)
        {
            // 로더 통해서만 불림. Module Info창에서 리커버리 할 때, Chuck에 Wafer 상태를 EXIST로 눌렀을 때 체크하는 함수.

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            iswaferhold = false;
            errormsg = "";
            try
            {
                iswaferhold = this.StageSupervisor().StageModuleState.IsHandlerholdWafer();

                retVal = this.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true, extraVacOn: false);// 삼발이 내리기 전이니까 extraVacOn: false   

                if (retVal == EventCodeEnum.NONE)
                {
                    if (this.IOManager().IO.Inputs.DIEXTRA_CHUCK_VAC_READY.IOOveride.Value == EnumIOOverride.NONE)
                    {
                        bool isVacTankValid = false;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIEXTRA_CHUCK_VAC_READY, out isVacTankValid);

                        if (isVacTankValid == false)
                        {
                            LoggerManager.Debug($"Start Wait Until Extra vacuum ready.");
                            int ioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIEXTRA_CHUCK_VAC_READY, true);
                            if (ioret == 0)
                            {
                                LoggerManager.Debug($"End Wait Until Extra vacuum ready.");
                            }
                            else
                            {
                                retVal = EventCodeEnum.EXTRA_CHUCK_VACUUM_STATUS_INVALID;
                                errormsg = $"Extra Vacuum is not ready. target:true, current:{this.IOManager().IO.Inputs.DIEXTRA_CHUCK_VAC_READY.Value}, Timeout:{this.IOManager().IO.Inputs.DIEXTRA_CHUCK_VAC_READY.TimeOut.Value}";
                            }
                            LoggerManager.Debug($"Error Wait Until Vacuum tack empty. timeout: {this.IOManager().IO.Inputs.DIEXTRA_CHUCK_VAC_READY.TimeOut.Value}msec");
                        }

                    }

                    if (retVal == EventCodeEnum.NONE)
                    {
                        // 웨이퍼 언로드 하다 Fail난 경우 삼발이가 Up된 상태로 Unknown으로 되어 있을 수 있음.
                        // 이 때 웨이퍼는 실제 셀에 있는것이기 때문에 EIXST버튼을 눌렀다면 삼발이를 내린 후 척 베큠으로 웨이퍼를 인식하고
                        // 실제 EXIST상태로 바꿔줄 수 있다.
                        if (this.StageSupervisor().CheckUsingHandler() == false)
                        {
                            bool isthreelegup = false;
                            bool isthreelegdown = false;

                            this.MotionManager().IsThreeLegUp(EnumAxisConstants.TRI, ref isthreelegup);
                            this.MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isthreelegdown);

                            // TriLeg Up인 경우 내림.
                            if (isthreelegup && !isthreelegdown)
                            {
                                // 일단 OPERA만.
                                retVal = this.StageSupervisor().StageModuleState.Handlerrelease(20000);
                            }
                        }
                    }

                     
                }
                else
                {
                    LoggerManager.Debug($"[GP_LoaderController]ChangeWaferStatus() VacuumOn is fail. {retVal}");
                }

                retVal = this.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true, extraVacOn: true);// 삼발이 내린 후니까 extraVacOn: true
                if (retVal == EventCodeEnum.NONE)
                {
                    if (this.IOManager().IO.Outputs.DOCHUCK_EXTRA_AIRON_0.IOOveride.Value == EnumIOOverride.NONE ||
                        this.IOManager().IO.Outputs.DOCHUCK_EXTRA_AIRON_2.IOOveride.Value == EnumIOOverride.NONE)
                    {
                        retVal = this.StageSupervisor().StageModuleState.ChuckMainVacOff();// 일정 시간 뒤에 미니멀 베큠 끄기 
                        if (retVal != EventCodeEnum.NONE)
                        {
                            errormsg = $"Chuck vacuum off failed.";
                        }
                    }

                    if (status == EnumSubsStatus.EXIST)
                    {
                        retVal = this.StageSupervisor().CheckWaferStatus(true);

                        if (retVal == EventCodeEnum.NONE)
                        {
                            retVal = this.StageSupervisor().SetWaferObjectStatus();
                            iswaferhold = false;
                        }
                        else
                        {
                            errormsg = $"Wafer is not detected. expected result: detected";
                            this.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: true); // Unknown 인 경우 extraVacReady: true로 유지한다.
                            //Exist 로 알았지만 실제 Wafer 센서가 안잡히는 경우.
                            LoggerManager.Debug($"[Change Wafer Status Error] Current Wafer Status:NOT_EXIST");
                        }
                    }
                    else
                    {
                        retVal = this.StageSupervisor().CheckWaferStatus(false);
                        if (retVal == EventCodeEnum.NONE)
                        {
                            if (iswaferhold != true)
                            {
                                retVal = this.StageSupervisor().ClearWaferStatus();
                            }
                        }
                        else
                        {
                            errormsg = $"Wafer is detected. expected result: not detected";
                            LoggerManager.Debug($"[Change Wafer Status Error] Current Wafer Status:EXIST");
                        }
                        this.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: false);
                    }
                }
                else
                {
                    LoggerManager.Debug($"[Change Wafer Status Error] Vacuum On Error. returnValue:{retVal}");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public TransferObject GetTransferObjectToSlotInfo()
        {
            TransferObject transferObj = new TransferObject();
            try
            {
                if (this.StageSupervisor().GetSlotInfo().WaferStatus == EnumSubsStatus.EXIST)
                {
                    transferObj.WaferState = this.StageSupervisor().GetSlotInfo().WaferState;
                    transferObj.OCR.Value = this.StageSupervisor().GetSlotInfo().WaferID;
                    transferObj.Size.Value = this.StageSupervisor().GetSlotInfo().WaferSize;
                    transferObj.WaferType.Value = this.StageSupervisor().GetSlotInfo().WaferType;
                    transferObj.NotchAngle.Value = this.StageSupervisor().GetSlotInfo().LoadingAngle;
                    transferObj.NotchType = this.StageSupervisor().GetSlotInfo().NotchType;
                    transferObj.ChuckNotchAngle.Value = this.StageSupervisor().GetSlotInfo().LoadingAngle;
                    transferObj.SlotNotchAngle.Value = this.StageSupervisor().GetSlotInfo().UnloadingAngle;
                    transferObj.OCRAngle.Value = this.StageSupervisor().GetSlotInfo().OCRAngle;
                    transferObj.OriginHolder = this.StageSupervisor().GetSlotInfo().OriginHolder;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return transferObj;
        }

        public void SetMonitoringBehavior(List<IMonitoringBehavior> monitoringBehaviors, int stageIdx)
        {
            try
            {
                byte[] bytes = this.ObjectToByteArray(monitoringBehaviors);

                GPLoaderService?.SetMonitoringBehavior(bytes, stageIdx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public byte[] GetMonitoringBehaviorFromClient()
        {
            List<IMonitoringBehavior> MonitoringBehaviorList = new List<IMonitoringBehavior>();
            byte[] ret = null;
            try
            {
                MonitoringBehaviorList = this.MonitoringManager().MonitoringBehaviorList;
                byte[] bytes = this.ObjectToByteArray(MonitoringBehaviorList); ;

                ret = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public void ChangeTabIndex(TabControlEnum tabEnum)
        {
            try
            {
                GPLoaderService?.ChangeTabIndex(tabEnum);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ManualRecoveryToStage(int behaviorIndex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                this.MonitoringManager().MonitoringBehaviorList[behaviorIndex].ManualRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public EventCodeEnum GetLoaderEmergency()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (GPLoaderService != null)
                {
                    ret = GPLoaderService.GetLoaderEmergency();
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                    LoggerManager.Debug($"GP_LoaderController.GetLoaderEmergency() GPLoaderService is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }
        public bool CanRunningLot()
        {
            bool retVal = false;
            try
            {
                bool chillerReady = this.EnvControlManager()?.ChillerManager?.CanRunningLot() ?? false;

                retVal = chillerReady;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// 현재 할당되어 있는 LOT 정보와 Loader 에서 확인하고자 하는 LOT 정보와 일치하는지 비교하려는 함수
        /// true : 일치한다 , false : 일치하지 않는다.
        /// </summary>
        public bool CheckCurrentAssignLotInfo(string lotID, string cstHashCode)
        {
            bool retVal = false;
            try
            {
                var stageLotInfos = this.LotOPModule().LotInfo.GetLotInfos();
                if (stageLotInfos != null && stageLotInfos.Count != 0)
                {
                    var lotInfo = stageLotInfos.Find(info => info.CassetteHashCode.Equals(cstHashCode));
                    if (lotInfo != null)
                    {
                        // 일치하는 cst HashCode 에 할당되었던 LOT 정보와 현재 LOT Namae, 비교하고자 하는 lotID 가 동일한지 확인.
                        if (this.LotOPModule().LotInfo.LotName.Value.Equals(lotInfo.LotID) && this.LotOPModule().LotInfo.LotName.Value.Equals(lotID))
                        {
                            retVal = true;
                        }
                    }
                }
                else
                {
                    //할당 된 정보가 없다면 종료해도 되는것으로 봄.
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }        


        public bool IsCanPerformLotStart()
        {
            bool retVal = false;
            try
            {
                retVal = this.LotOPModule().IsCanPerformLotStart();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public List<StageLotInfo> GetStageLotInfos()
        {
            try
            {
                if (this.LotOPModule().LotInfo != null)
                {
                    return this.LotOPModule().LotInfo.GetLotInfos();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }
        public bool IsHasProcessingLotAssignState()
        {
            bool retVal = false;
            try
            {
                if (this.LotOPModule().LotInfo != null && this.LotOPModule().LotInfo.GetLotInfos() != null)
                {
                    int processingLOTAssingStateCount = this.LotOPModule().LotInfo.GetLotInfos().FindAll(info => info.StageLotAssignState == LotAssignStateEnum.PROCESSING).Count;
                    if (processingLOTAssingStateCount != 0)
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum CardChangeIsConditionSatisfied(bool needToSetTempToken)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.CardChangeModule().IsCCAvailableSatisfied(needToSetTempToken);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool NeedToSetCCActivatableTemp()
        {
            bool retVal = false;
            try
            {
                retVal = this.CardChangeModule().NeedToSetCCActivatableTemp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum IsAvailableToSetOtherThanCCActiveTemp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.CardChangeModule().IsAvailableToSetOtherThanCCActiveTemp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum RecoveryCCBeforeTemp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.CardChangeModule().RecoveryCCBeforeTemp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetCCActiveTemp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.CardChangeModule().SetCCActiveTemp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void AbortCardChange()
        {
            try
            {
                this.CardChangeModule().AbortCardChange();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private EnumWaferSize TransferWaferSize = EnumWaferSize.UNDEFINED;
        public EnumWaferSize GetTransferWaferSize()
        {
            EnumWaferSize ret = EnumWaferSize.UNDEFINED;
            try
            {
                ret = TransferWaferSize;
                switch (ret)
                {
                    case EnumWaferSize.INVALID:
                    case EnumWaferSize.UNDEFINED:
                        ret = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum;
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
           
            return ret;
        }

        public EventCodeEnum SetTransferWaferSize(EnumWaferSize waferSize)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                TransferWaferSize = waferSize;
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public EventCodeEnum GetCardPodState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            SequenceBehavior command;

            try
            {
                command = new GP_CheckPCardPodIsDown();//=> Card Pod 이 내려가 있는 지 확인
                retVal = command.Run().Result.ErrorCode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}

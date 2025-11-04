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

namespace LoaderController
{
    using LoaderControllerBase;
    using LoaderParameters;
    using LoaderServiceBase;

    using LoaderControllerStates;
    using ProberInterfaces.State;
    using ProberInterfaces.Wizard;
    using LogModule;
    using ProberInterfaces.Loader;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Param;
    using ProberInterfaces.Monitoring;

    //using ProberInterfaces.ThreadSync;

    public class LoaderController : ILoaderController, ILoaderControllerExtension, ILoaderServiceCallback, INotifyPropertyChanged, IHasDevParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        //Test
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
        private bool _IsAbort;
        public bool IsAbort
        {
            get { return _IsAbort; }
            set
            {
                if (value != _IsAbort)
                {
                    _IsAbort = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ILoaderControllerParam LoaderConnectParam { get => ControllerParam; }

        //

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
        private bool LotEndFlag = false;

        public void SetLotEndFlag(bool flag)
        {
            LotEndFlag = flag;
        }
        public bool GetLotEndFlag()
        {
            return LotEndFlag;
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

        private LoaderControllerStateBase _ControllerState;
        public LoaderControllerStateBase ControllerState
        {
            get { return _ControllerState; }
        }

        public IInnerState InnerState
        {
            get { return _ControllerState; }
            set
            {
                if (value != _ControllerState)
                {
                    _ControllerState = value as LoaderControllerStateBase;
                    RaisePropertyChanged();
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

        public ILoaderService LoaderService { get; set; }
        public IWaferTransferScheduler WaferTransferScheduler { get; set; }
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
        public bool IsCancel { get; set; }
        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
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

        public LoaderMap ReqMap { get; set; }

        //=> TODO : Commands
        public EventCodeEnum LoaderSystemInit()
        {
            EventCodeEnum rel = EventCodeEnum.UNDEFINED;
            //using (Locker locker = new Locker(syncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return rel;
            //    }
            lock (syncObj)
            {

                try
                {
                    rel = this.ControllerState.LoaderSystemInit();

                    if (rel == EventCodeEnum.NONE)
                    {
                        rel = ClearState();
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return rel;
        }
        public Autofac.IContainer GetLoaderContainer()
        {
            Autofac.IContainer retval = null;

            //Direct에서만 가능: GP에서는 ProberSystem/SystemInfo의 SystemMode.json에서 Single을 Multiple로 변경

            try
            {
                retval = (LoaderService as IDirectLoaderService)?.GetLoaderContainer();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
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

        public EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (syncObj)
            {

                try
                {
                    retVal = this.ControllerState.UpdateSystemParam(systemParam);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }




        public EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //using (Locker locker = new Locker(syncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return retVal;
            //    }
            lock (syncObj)
            {

                try
                {
                    retVal = this.ControllerState.SaveSystemParam(systemParam);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retVal;
        }
        public EventCodeEnum SaveSystemParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //using (Locker locker = new Locker(syncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return retVal;
            //    }
            lock (syncObj)
            {

                try
                {
                    retVal = SaveSystemParam(null);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retVal;
        }


        public EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //using (Locker locker = new Locker(syncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return retVal;
            //    }
            lock (syncObj)
            {
                try
                {
                    retVal = this.ControllerState.UpdateDeviceParam(deviceParam);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }
        public EventCodeEnum SaveDeviceParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //using (Locker locker = new Locker(syncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return retVal;
            //    }
            lock (syncObj)
            {

                try
                {
                    retVal = SaveDeviceParam(null);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retVal;
        }


        public EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //using (Locker locker = new Locker(syncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return retVal;
            //    }

            lock (syncObj)
            {
                try
                {
                    retVal = this.ControllerState.SaveDeviceParam(deviceParam);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retVal;
        }

        public EventCodeEnum OCRUaxisRelMove(double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ControllerState.JogRelMove(EnumAxisConstants.U1, value);
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
                retVal = ControllerState.JogRelMove(EnumAxisConstants.W, value);
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

            LoggerManager.Debug($"WaitForCommandDone() START");

            try
            {
                while (true)
                {
                    var runningCmdState = CommandRecvSlot.GetState();

                    if (CommandRecvProcSlot != null)
                    {
                        if (CommandRecvProcSlot.GetState() == CommandStateEnum.REQUESTED)
                        {
                            continue;
                        }
                    }

                    bool isCommandProcessed =
                        runningCmdState == CommandStateEnum.NOCOMMAND ||
                        runningCmdState == CommandStateEnum.ABORTED;

                    if (isCommandProcessed && (ModuleState.GetState() == ModuleStateEnum.IDLE) && (LoaderInfo.ModuleInfo.ModuleState == ModuleStateEnum.IDLE))
                    {
                        break;
                    }
                    else if (ModuleState.GetState() == ModuleStateEnum.ERROR)
                    {
                        break;
                    }
                    else if (ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                    {
                        // TODO : 리커버리 상태에서 복구 로직 동작 중, 상태가 리커버리를 유지한다면,
                        // 해당 로직이 문제가 있는가 없는가 따져봐야 됨.

                        break;
                    }
                    else
                    {

                    }

                    //delays.DelayFor(1);
                    Thread.Sleep(10);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            LoggerManager.Debug($"WaitForCommandDone() END");

            return retVal;
        }

        public EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ControllerState.MoveToModuleForSetup(module, skipuaxis, slot, index);
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
                retVal = ControllerState.RetractAll();
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
                LoaderService.FOUP_RaiseFoupStateChanged(info);
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
                LoaderService.FOUP_RaiseWaferOutDetected(foupNumber);
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
                retVal = LoaderService.IsFoupAccessed(foupNumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
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
                    defaultParam.LoaderIP = "";

                    return defaultParam;
                }

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
                    WaferTransferScheduler = new WaferTransferScheduler(this);

                    //
                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    _ControllerState = new IDLE(this);
                    ModuleState = new ModuleUndefinedState(this);

                    InitLoaderService();

                    UpdateDeviceParam(this.LoaderDeviceParam);

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
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
        public void InitLoaderService()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                string rootParamPath = this.FileManager().GetRootParamPath();

                this.LoaderService = LoaderServiceProvider.Create(this);

                if (ControllerParam.LoaderServiceType != LoaderServiceTypeEnum.WCF)
                {
                    //=> Connect
                    //EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                    retVal = LoaderService.Connect();
                    if (retVal != EventCodeEnum.NONE)
                        throw new Exception("Connect error. code = " + retVal);

                    retVal = LoaderService.Initialize(rootParamPath);
                    if (retVal != EventCodeEnum.NONE)
                        throw new Exception("Initialize error. code = " + retVal);

                    //=> Update loader info
                    this.LoaderSystemParam = LoaderService.GetSystemParam();
                    //this.LoaderDeviceParam = LoaderService.GetDeviceParam();
                    this.LoaderInfo = LoaderService.GetLoaderInfo();
                }
                else if (ControllerParam.LoaderServiceType == LoaderServiceTypeEnum.WCF)
                {
                    LoaderServiceProvider.ServiceProxy.Faulted += ServiceProxy_Faulted;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ServiceProxy_Faulted(object sender, EventArgs e)
        {
            LoggerManager.Debug($"Service proxy fault.");
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

                LoaderServiceProvider.Deinit(this.LoaderService);
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
                LoaderService.RECOVERY_MotionInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Error($"{err}, Error occurred while loader recovery.");
            }

        }

        ////private LockKey syncObj = new LockKey("Loader Controller");
        private object syncObj = new object();
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

        //public ModuleStateEnum FreeRun_Remove()
        //{
        //    lock (syncObj)
        //    {
        //        this.ControllerState.FreeRun();
        //    }

        //    ModuleState.Execute(this.ControllerState.ModuleState);
        //    return ModuleState.State;
        //}

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

        public bool CanExecute(IProbeCommandToken token)
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return false;
            //    }
            lock (syncObj)
            {
                bool isInjected;
                isInjected = ControllerState.CanExecute(token);
                return isInjected;
            }
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
        public EventCodeEnum ClearHandlerStatus()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.StageSupervisor().CheckUsingHandler())
                {
                    this.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.NOT_EXIST);

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
        public EnumWaferType GetWaferType()
        {
            return this.StageSupervisor().WaferObject.GetWaferType();
        }

        public EnumSubsStatus UpdateCardStatus(out EnumWaferState cardState)
        {
            cardState = EnumWaferState.UNPROCESSED;
            return EnumSubsStatus.NOT_EXIST;
        }
        public EnumCardChangeType GetCardChangeType()
        {
            EnumCardChangeType cardState = EnumCardChangeType.UNDEFINED;
            return cardState;
        }
        public EventCodeEnum GetCardIDValidateResult(string CardID, out string Msg)
        {
            Msg = "";
            return EventCodeEnum.NONE;
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


        public void UI_ShowLoaderCam()
        {
            try
            {
                //IProberStation prober = this.ProberStation();
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
                //IProberStation prober = this.ProberStation();
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
                retVal = EventCodeEnum.NONE;
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
        #endregion


        public EventCodeEnum LoadParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                DevParam = new IParamEmpty();
                SysParam = new IParamEmpty();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
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
            try
            {
                return LoaderService.OFR_SetOcrID(inputOCR);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum OFR_OCRRemoteEnd()
        {
            try
            {
                return LoaderService.OFR_OCRRemoteEnd();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum OFR_OCRFail()
        {
            try
            {
                return LoaderService.OFR_OCRFail();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum OFR_OCRAbort()
        {
            try
            {
                return LoaderService.OFR_OCRAbort();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void UpdateIsNeedLotEnd(LoaderInfo loaderInfo)
        {
            return;
        }


        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public void SetTestCenteringFlag(bool centeringflag)
        {
            try
            {
                LoaderService.SetTestCenteringFlag(centeringflag);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum SetWaferInfo(IWaferInfo waferInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = UpdateWaferData(this.LoaderDeviceParam, waferInfo);

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

        private EventCodeEnum UpdateWaferData(LoaderDeviceParameter LoaderDeviceParam, IWaferInfo waferInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                SubstrateSizeEnum size = SubstrateSizeEnum.UNDEFINED;
                size = WaferSizeToSubstracteSizeConverter(waferInfo?.WaferSize ?? EnumWaferSize.UNDEFINED);

                foreach (var loaderModule in LoaderDeviceParam.CassetteModules)
                {
                    loaderModule.AllocateDeviceInfo.Size.Value = size;
                    loaderModule.WaferThickness.Value = waferInfo.WaferThickness;

                    loaderModule.AllocateDeviceInfo.OCRMode.Value = waferInfo.OCRMode;
                    loaderModule.AllocateDeviceInfo.OCRType.Value = waferInfo.OCRType;
                    loaderModule.AllocateDeviceInfo.OCRDirection.Value = waferInfo.OCRDirection;

                    // Validation Slot size

                    double slotsize = 0;
                    double slotsizetol = 0;

                    switch (size)
                    {
                        case SubstrateSizeEnum.UNDEFINED:
                            break;
                        case SubstrateSizeEnum.INCH6:

                            if (this.LoaderSystemParam != null)
                            {
                                slotsize = this.LoaderSystemParam.DefaultSlotSizes.SlotSize6inch.Value;
                                slotsizetol = this.LoaderSystemParam.DefaultSlotSizes.SlotSize6inchTolerance.Value;
                            }

                            break;
                        case SubstrateSizeEnum.INCH8:

                            if (this.LoaderSystemParam != null)
                            {
                                slotsize = this.LoaderSystemParam.DefaultSlotSizes.SlotSize8inch.Value;
                                slotsizetol = this.LoaderSystemParam.DefaultSlotSizes.SlotSize8inchTolerance.Value;
                            }

                            break;
                        case SubstrateSizeEnum.INCH12:

                            if (this.LoaderSystemParam != null)
                            {
                                slotsize = this.LoaderSystemParam.DefaultSlotSizes.SlotSize12inch.Value;
                                slotsizetol = this.LoaderSystemParam.DefaultSlotSizes.SlotSize12inchTolerance.Value;
                            }

                            break;
                        case SubstrateSizeEnum.CUSTOM:
                            break;
                        default:
                            break;
                    }

                    if ((loaderModule.SlotSize.Value < slotsize - slotsizetol) || (loaderModule.SlotSize.Value > slotsize - slotsizetol))
                    {
                        loaderModule.SlotSize.Value = slotsize;
                    }

                    //this.GetParam_Wafer().GetPhysInfo()
                }

                foreach (var loaderModule in LoaderDeviceParam.FixedTrayModules)
                {
                    loaderModule.AllocateDeviceInfo.Size.Value = size;
                }

                foreach (var loaderModule in LoaderDeviceParam.InspectionTrayModules)
                {
                    loaderModule.AllocateDeviceInfo.Size.Value = size;
                }

                foreach (var loaderModule in LoaderDeviceParam.ChuckModules)
                {
                    loaderModule.LoadingNotchAngle.Value = waferInfo.Notchangle;
                }

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

        public LoaderMap RequestJob(LoaderInfo loaderInfo, out bool isExchange, out bool isNeedWafer, out bool isTempReady, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            isExchange = false;
            isNeedWafer = false;
            isTempReady = false;
            cstHashCodeOfRequestLot = "";
            return null;
        }

      
        public LoaderMap UnloadRequestJob(LoaderInfo loaderInfo)
        {
            return null;
        }
        public ModuleID GetChuckID()
        {
            return ChuckID;
        }
        public ModuleID GetOriginHolder()
        {
            return new ModuleID();
        }
        public bool LotOPStart(int foupnumber, bool iscellstart = false, string lotID = "", string cstHashCode = "")
        {
            return false;
        }
        public ModuleStateEnum GetLotState()
        {
            return this.LotOPModule().ModuleState.GetState();
        }
        public bool IsLotEndReady()
        {
            return false;
        }
        public bool LotOPEnd(int foupnumber = -1)
        {
            return false;
        }
        public bool LotOPPause(bool isabort = false)
        {
            return false;
        }
        public bool LotOPResume()
        {
            return false;
        }

        public bool CardAbort()
        {
            return false;
        }

        public Task<EventCodeEnum> ConnectLoaderService()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            // InitLoaderService();
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void BroadcastLotState(bool isBuzzorOn)
        {
        }

        public EventCodeEnum ResponseSystemInit(EventCodeEnum errorCode)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum ResponseCardRecovery(EventCodeEnum errorCode)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public TransferObject GetDeviceInfo()
        {
            return null;
        }
        public bool IsServiceAvailable()
        {
            return true;
        }
        public void DisConnect()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //return retVal;
        }
        public string GetLoaderIP()
        {
            return ControllerParam.LoaderIP;
        }
        public int GetChuckIndex()
        {
            return ControllerParam.ChuckIndex;
        }
        public void SetActiveLotInfo(int foupnumber, string lotid, string cstHashCode, string carrierid)
        {
            return;
        }

        public void SetCassetteHashCode(int foupNumber, string lotId, string cstHashCode)
        {
            return;
        }
        public void SetLotStarted(int foupNumber, string lotId, string cstHashCode)
        {
            return;
        }

        public bool GetconnectFlag()
        {
            return false;
        }

        public string GetProbeCardID()
        {
            string retVal = "";
            try
            {
                retVal = this.CardChangeModule().GetProbeCardID();
                // retVal = this.GetParam_ProbeCard().GetProbeCardID();
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
                retVal = this.GetParam_Wafer().GetSubsInfo().SlotIndex.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public StageLotData GetStageInfo()
        {
            return null;
        }

        public void ReadStatusSoakingChillingTime(ref long _ChillingTimeMax, ref long _ChillingTime, ref SoakingStateEnum CurStatusSoaking_State)
        {
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

        public EventCodeEnum SetStageMode(GPCellModeEnum mode)
        {
            return EventCodeEnum.NONE;
        }

        public void SetStreamingMode(StreamingModeEnum mode)
        {

        }
        public void SetForcedDone(EnumModuleForcedState flag)
        {

        }
        public void CellProbingResume()
        {

        }
        public void SetStilProbingZUp(bool flag = true)
        {

        }
      
        public void SetLotLoadingPosCheckMode(bool flag)
        {

        }
        public EventCodeEnum WaferCancel()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            }
            catch (Exception)
            {
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
        public EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo)
        {
            return EventCodeEnum.NOTIFY_ERROR;
        }

        public void SetLotLogMessage(string message, int idx, ModuleLogType ModuleType, StateLogType State)
        {
            return;
        }
        public void SetParamLogMessage(string message, int idx)
        {
            return;
        }

        public bool SetNoReadScanState(int cassetteNumber)
        {
            bool retVal = false;

            try
            {
                retVal = LoaderService.SetNoReadScanState(cassetteNumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool GetRunState(bool isTransfer = false)
        {
            return this.SequenceEngineManager().GetRunState(); //카드 체인저도 포함되어야한다.
        }
        public bool GetMovingState()
        {
            return this.SequenceEngineManager().GetMovingState();
        }
        public EventCodeEnum UpdateSoakingInfo(SoakingInfo soakinfo)
        {
            return EventCodeEnum.NONE;
        }

        public bool IsCardUpModuleUp()
        {
            return false;
        }
        public double GetSetTemp()
        {
            return 0.0;
        }

        public EventCodeEnum ReserveErrorEnd(string ErrorMessage = "Paused by host(CELL ABORT TEST).")
        {
            return EventCodeEnum.NONE;
        }
        public void SetRecipeDownloadEnable(bool flag)
        {
            return;
        }
        public ErrorEndStateEnum GetErrorEndState()
        {
            ErrorEndStateEnum retVal = ErrorEndStateEnum.NONE;

            return retVal;
        }

        public EventCodeEnum SetAbort(bool isAbort, bool isForced = false)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            return retVal;
        }

        public void SetLotOut(bool islotout)
        {
            try
            {
                _IsLotOut = islotout;
                LoggerManager.Debug($"GP_LoaderController SetLotOut execute {islotout}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void SetDeviceName(int cellno, string deviceName)
        {
        }
        public void SetDeviceLoadResult(bool result)
        {
        }
        public bool GetReservePause()
        {
            return false;
        }
        public void CancelCellReservePause()
        {
            return;
        }
        public void CancelLot(int foupnumber, bool iscellend, string lotID, string cstHashCode)
        {
            return;
        }

        public EventCodeEnum CanWaferUnloadRecovery(ref bool canrecovery, ref ModuleStateEnum wafertransferstate)
        {
            throw new NotImplementedException();
        }

        public EnumValveModuleType GetValveModuleType()
        {
            EnumValveModuleType retVal = EnumValveModuleType.INVALID;
            try
            {
                retVal = this.EnvControlManager().GetValveParam().ValveModuleType.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum UploadCardPatternImages(byte[] data, string filename, string devicename, string cardid)
        {
            return EventCodeEnum.NONE;
        }

        public EnumWaferType GetActiveLotWaferType(string lotid)
        {
            return EnumWaferType.STANDARD;
        }

        public List<CardImageBuffer> DownloadCardPatternImages(string devicename, int downimgcnt, string cardid)
        {
            return null;
        }

        public EventCodeEnum UploadProbeCardInfo(ProberCardListParameter probeCard)
        {
            return EventCodeEnum.NONE;
        }

        public ProberCardListParameter DownloadProbeCardInfo(string cardID)
        {
            return null;
        }

        public void SetProbingStart(bool isStart)
        {

        }
        public void SetTransferError(bool isError)
        {

        }

        public void SetTitleMessage(int cellno, string message, string foreground = "", string background = "")
        {
            return;
        }

        public byte[] GetBytesFoupObjects()
        {
            byte[] retval = null;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum NotifyLotEnd(int foupNumber, string lotID)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum GetAngleInfo(out double notchAngle, out double slotAngle, out double ocrAngle)
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            notchAngle = 0;
            slotAngle = 0;
            ocrAngle = 0;
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
        public string GetPauseReason()
        {
            return "";
        }
        public bool GetErrorEndFlag()
        {
            return false;
        }
        public void SetErrorEndFalg(bool flag)
        {
        }
        
        public void UpdateLotVerifyInfo(int portnum, bool flag)
        {

        }
        public void UpdateDownloadMapResult( bool flag)
        {

        }

       
        public void UpdateTesterConnectedStatus(bool flag)
        {

        }

        public bool GetTesterAvailableData()
        {
            return false;
        }

        public ModuleStateEnum GetSetSoakingState()
        {
            throw new NotImplementedException();
        }

        public void UpdateLotDataInfo(StageLotDataEnum type, string val)
        {
            if(SystemManager.SysteMode != SystemModeEnum.Single)
            {
                throw new NotImplementedException();
            }

        }
        public void UpdateStageMove(StageMoveInfo info)
        {
            if (SystemManager.SysteMode != SystemModeEnum.Single)
            {
                throw new NotImplementedException();
            }
        }

        public string GetFoupNumberStr()
        {
            if (SystemManager.SysteMode != SystemModeEnum.Single)
            {
                throw new NotImplementedException();
            }
            else
            {
                return "1";
            }
        }
        public void SetCardStatus(bool isExist, string id, bool isDocked)
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

        public void SetStageLock(StageLockMode mode)
        {
            throw new NotImplementedException();
        }
       

        public EventCodeEnum SetRecoveryMode(bool isRecovery)
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
        public void SetStopBeforeProbingFlag(bool flag, int stageidx = 0)
        {

        }

        public void SetStopAfterProbingFlag(bool flag, int stageidx = 0)
        {

        }
        public void SetOnceStopBeforeProbingFlag(bool flag, int stageidx = 0)
        {

        }

        public void SetOnceStopAfterProbingFlag(bool flag, int stageidx = 0)
        {

        }

        public StageLockMode GetStageLock()
        {
            return StageLockMode.UNLOCK;
        }

        public void NotifyReasonOfError(string errmsg)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ODTPUpload(int stageindex, string filename)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ResultMapUpload(int stageindex ,string filename)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ResultMapDownload(int stageindex, string filename)
        {
            return EventCodeEnum.NONE;
        }

        public bool GetStopBeforeProbingFlag()
        {
            return false;
        }

        public bool GetStopAfterProbingFlag()
        {
            return false;
        }

        public EventCodeEnum WriteWaitHandle(short value)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WaitForHandle(short handle, long timeout = 60000)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoPinAlign()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMoveLockState(ReasonOfStageMoveLock reason)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMoveUnLockState(ReasonOfStageMoveLock reason)
        {
            throw new NotImplementedException();
        }
        public void ResetAssignLotInfo(int foupnumber, string lotid, string cstHashCode)
        {

        }
        public CatCoordinates GetPMShifhtValue()
        {
            throw new NotImplementedException();
        }

        public void SetForcedDoneState()
        {
           
        }
        public void SetForcedDoneSpecificModule(EnumModuleForcedState flag, ModuleEnum moduleEnum)
        {
            
        }

        public List<ReasonOfStageMoveLock> GetReasonofLockFromClient()
        {
            throw new NotImplementedException();
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

        public void IsAlignDoing(ref bool pinAlignDoing, ref bool waferAlignDoing)
        {            
        }

        public bool IsNeedLotEnd()
        {
            return false;
        }
        public bool IsLotAbort()
        {
            return false;
        }


        public bool GetLotOutState()
        {
            return false;
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

        public EventCodeEnum CheckSetValueAvailable(string propertypath, object val, out string errorlog)//, string source_classname)
        {
            errorlog = "";
            return EventCodeEnum.NONE;
        }


        //public EventCodeEnum SetValue(string propertypath, Object val, bool isNeedValidation = false)//, string source_classname = null)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        //(bool needToValidation, IModule convertModule) = this.ParamManager().ClassNameConverter(source_classname);
        //        IElement element = this.ParamManager().GetElement(propertypath);
        //        if (element.PropertyPath != element.OriginPropertyPath)
        //        {
        //            element = this.ParamManager().GetElement(element.OriginPropertyPath);

        //        }

        //        if (element != null)
        //        {
        //            retVal = element.SetValue(val, isNeedValidation);//, convertModule);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}

        public void SetTCW_Mode(TCW_Mode tcw_Mode)
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum IsShutterClose()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public LotModeEnum GetLotModefromAssiginedFoup(int foupNumber)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum ChangeWaferStatus(EnumSubsStatus status, out bool iswaferhold, out string errormsg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            iswaferhold = false;
            errormsg = "";
            try
            {
                //retVal = this.StageSupervisor().StageModuleState.StageMoveUnLockState();
                retVal = this.StageSupervisor().ClearWaferStatus();
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
              
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return transferObj;
        }

        public void SetMonitoringBehavior(List<IMonitoringBehavior> monitoringBehaviors, int stageIdx)
        {
            throw new NotImplementedException();
        }

        public byte[] GetMonitoringBehaviorFromClient()
        {
            throw new NotImplementedException();
        }

        public void ChangeTabIndex(TabControlEnum tabEnum)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum ManualRecoveryToStage(int behaviorIndex)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum GetLoaderEmergency()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public void SetWaferObjectState(AlignStateEnum state)
        {
            throw new NotImplementedException();
        }

        public void LotCancelSoakingAbort(int stageindex)
        {
            return;
        }
        public bool CanRunningLot()
        {
            return true;
        }

        public bool CheckCurrentAssignLotInfo(string lotID, string cstHashCode)
        {
            return true;
        }

        public bool IsCanPerformLotStart()
        {
            return false;
        }

        public void SetCellModeChanging()
        {
            throw new NotImplementedException();
        }

        public void ResetCellModeChanging()
        {
            throw new NotImplementedException();
        }
        public List<StageLotInfo> GetStageLotInfos()
        {
            return null;
        }
        public bool IsHasProcessingLotAssignState()
        {
            return false;
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

        public void UpdateLogUploadList(EnumUploadLogType type)
        {
            return;
        }

        public EventCodeEnum GetNotchTypeInfo(out WaferNotchTypeEnum notchType)
        {
            notchType = WaferNotchTypeEnum.NOTCH;
            return EventCodeEnum.NONE;
        }
    }
}

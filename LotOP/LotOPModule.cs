using AppSelector;
using EventProcessModule.Internal;
using LogModule;
using LotParamObject;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Event;
using ProberInterfaces.Event.EventProcess;
using ProberInterfaces.NeedleClean;
using ProberInterfaces.State;
using ProberInterfaces.Wizard;
using SequenceService;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using UcDisplayPort;
////using ProberInterfaces.ThreadSync;

namespace LotOP
{
    enum StageCam
    {
        WAFER_HIGH_CAM,
        WAFER_LOW_CAM,
        PIN_HIGH_CAM,
        PIN_LOW_CAM,
    }

    enum LoaderCam
    {
        PACL6_CAM,
        PACL8_CAM,
        PACL12_CAM,
        ARM_6_CAM,
        ARM_8_12_CAM,
        OCR1_CAM,
        OCR2_CAM,
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LotOPModule : SequenceServiceBase, ILotOPModule, INotifyPropertyChanged, IHasDevParameterizable, IHasSysParameterizable, IProbeEventSubscriber, IHasDll
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public bool Initialized { get; set; } = false;

        private WaferObject Wafer { get { return (WaferObject)this.StageSupervisor().WaferObject; } }
        //private LotOPModule View3D;

        private INeedleCleanObject NCObject { get { return this.StageSupervisor().NCObject; } }
        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private IParam _LotDeviceParam_IParam;
        [ParamIgnore]
        public IParam LotDeviceParam_IParam
        {
            get { return _LotDeviceParam_IParam; }
            set
            {
                if (value != _LotDeviceParam_IParam)
                {
                    _LotDeviceParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.Lot);
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

        private EventCodeInfo _PauseSourceEvent = null;
        public EventCodeInfo PauseSourceEvent
        {
            get { return _PauseSourceEvent; }
            set
            {
                if (value != _PauseSourceEvent)
                {
                    _PauseSourceEvent = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ReasonOfStopOption _ReasonOfStopOption = new ReasonOfStopOption();
        public ReasonOfStopOption ReasonOfStopOption
        {
            get { return _ReasonOfStopOption; }
            set
            {
                if (value != _ReasonOfStopOption)
                {
                    _ReasonOfStopOption = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LotDeviceParam _LotDevParam;
        public LotDeviceParam LotDevParam
        {
            get { return _LotDevParam; }
            set
            {
                if (value != _LotDevParam)
                {
                    _LotDevParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LotSystemParam _LotSysParam;
        public LotSystemParam LotSysParam
        {
            get { return _LotSysParam; }
            set
            {
                if (value != _LotSysParam)
                {
                    _LotSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ILotDeviceParam LotDeviceParam
        {
            get { return LotDevParam; }

        }

        //private IParam _DevParam;
        //[ParamIgnore]
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        public string LotStartFailReason = null;

        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private IParam _AppItems_IParam;
        public IParam AppItems_IParam
        {
            get { return _AppItems_IParam; }
            set
            {
                if (value != _AppItems_IParam)
                {
                    _AppItems_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private AppModuleItems _AppItems;
        public AppModuleItems AppItems
        {
            get { return _AppItems; }
            set
            {
                if (value != _AppItems)
                {
                    _AppItems = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LotStartFlag;
        public bool LotStartFlag
        {
            get { return _LotStartFlag; }
            set
            {
                if (value != _LotStartFlag)
                {
                    _LotStartFlag = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _LotEndFlag;
        public bool LotEndFlag
        {
            get { return _LotEndFlag; }
            set
            {
                if (value != _LotEndFlag)
                {
                    _LotEndFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLastWafer;
        public bool IsLastWafer
        {
            get { return _IsLastWafer; }
            set
            {
                if (value != _IsLastWafer)
                {
                    LoggerManager.Debug($"IsLastWafer value changed {_IsLastWafer} to {value}");
                    _IsLastWafer = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsNeedLotEnd;
        public bool IsNeedLotEnd
        {
            get { return _IsNeedLotEnd; }
            set
            {
                if (value != _IsNeedLotEnd)
                {
                    LoggerManager.Debug($"IsNeedLotEnd value changed {_IsNeedLotEnd} to {value}");
                    _IsNeedLotEnd = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ModuleStopFlag;
        public bool ModuleStopFlag
        {
            get { return _ModuleStopFlag; }
            set
            {
                if (value != _ModuleStopFlag)
                {
                    _ModuleStopFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ErrorEndStateEnum _ErrorEndState;
        public ErrorEndStateEnum ErrorEndState
        {
            get { return _ErrorEndState; }
            set
            {
                if (value != _ErrorEndState)
                {
                    _ErrorEndState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ErrorEndFlag;
        /// <summary>
        /// ERROR_END 를 받으면 true, UI를 통해 사용자가 Clear 해줬으면 false 
        /// </summary>
        public bool ErrorEndFlag
        {
            get { return _ErrorEndFlag; }
            set
            {
                if (value != _ErrorEndFlag)
                {
                    _ErrorEndFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _UnloadFoupNumber; //다이나믹 모드에서 사용
        /// <summary>
        /// ERROR_END 를 받으면 true, UI를 통해 사용자가 Clear 해줬으면 false 
        /// </summary>
        public int UnloadFoupNumber
        {
            get { return _UnloadFoupNumber; }
            set
            {
                if (value != _UnloadFoupNumber)
                {
                    _UnloadFoupNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<IStateModule> _RunList = new List<IStateModule>();
        [ParamIgnore]

        public List<IStateModule> RunList
        {
            get { return _RunList; }
            set
            {
                if (value != _RunList)
                {
                    _RunList = value;
                }
            }
        }

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

        //public LotInfo LotInfo { get; set; }

        private ILotInfo _LotInfo;
        public ILotInfo LotInfo
        {
            get { return _LotInfo; }
            set
            {
                if (value != _LotInfo)
                {
                    _LotInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ISystemInfo _SystemInfo;
        public ISystemInfo SystemInfo
        {
            get { return _SystemInfo; }
            set
            {
                if (value != _SystemInfo)
                {
                    _SystemInfo = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IDeviceInfo _DeviceInfo;
        public IDeviceInfo DeviceInfo
        {
            get { return _DeviceInfo; }
            set
            {
                if (value != _DeviceInfo)
                {
                    _DeviceInfo = value;
                    RaisePropertyChanged();
                }
            }
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
                throw;
            }
            return retVal;
        }


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
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }

        private CommandTokenSet _RunTokenSet = new CommandTokenSet();

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleStateEnum _CurrentModuleState;
        public ModuleStateEnum CurrentModuleState
        {
            get { return _CurrentModuleState; }
            set
            {
                if (value != _CurrentModuleState)
                {
                    _CurrentModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get
            {
                return _ModuleState;
            }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    CurrentModuleState = _ModuleState.GetState();
                    RaisePropertyChanged();
                }
            }
        }

        private LotOPState _LotModuleState;

        public IInnerState InnerState
        {
            get { return _LotModuleState; }
            set { _LotModuleState = value as LotOPState; }
        }



        //.. Lot Screen Binding Properties
        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDisplayPort _LoaderDisplayPort;
        public IDisplayPort LoaderDisplayPort
        {
            get { return _LoaderDisplayPort; }
            set
            {
                if (value != _LoaderDisplayPort)
                {
                    _LoaderDisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }
        private void _model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //  If Model.Foo changed, announce that Model changed. Any binding using 
            //  the Model property as its source will update, and that will cause 
            //  the template selector to be re-invoked. 

            if (e.PropertyName == nameof(_ViewTarget))
            {
                RaisePropertyChanged(nameof(_ViewTarget));
            }
        }
        private object _ViewTarget;
        public object ViewTarget
        {
            get { return _ViewTarget; }
            set
            {
                if (value != _ViewTarget)
                {

                    // _ViewTarget = null;
                    PreViewTarget = _ViewTarget;
                    _ViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MiniVisible;
        public Visibility MiniVisible
        {
            get { return _MiniVisible; }
            set
            {
                if (value != _MiniVisible)
                {
                    _MiniVisible = value;
                    RaisePropertyChanged();
                }
            }
        }




        public object PreViewTarget { get; set; }

        private object _MiniViewTarget;
        public object MiniViewTarget
        {
            get { return _MiniViewTarget; }
            set
            {
                if (value != _MiniViewTarget)
                {
                    _MiniViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _SwitchVisiability = Visibility.Hidden;
        public Visibility SwitchVisiability
        {
            get { return _SwitchVisiability; }
            set
            {
                if (value != _SwitchVisiability)
                {
                    _SwitchVisiability = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _ZoomVisiability = Visibility.Visible;
        public Visibility ZoomVisiability
        {
            get { return _ZoomVisiability; }
            set
            {
                if (value != _ZoomVisiability)
                {
                    _ZoomVisiability = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _TransferReservationAboutPolishWafer = false;
        public bool TransferReservationAboutPolishWafer
        {
            get { return _TransferReservationAboutPolishWafer; }
            set { _TransferReservationAboutPolishWafer = value; }
        }

        public IInnerState PreInnerState
        {
            get;
            set;
        } = null;

        //private LockKey lockObject = new LockKey("LotOP Module DLL");
        private object lockObject = new object();

        public ModuleStateEnum End()
        {
            try
            {
                if ((!this.MonitoringManager().IsSystemError) && (!this.MonitoringManager().IsStageSystemError))
                {
                    LoggerManager.Debug("LOTOPModule RunList End");
                    foreach (IStateModule module in this.RunList)
                    {
                        if (!(module.ModuleState.State == ModuleStateEnum.PAUSED) && !(module.ModuleState.State == ModuleStateEnum.IDLE))
                        {
                            module.End();
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"LOTOPModule End Failed.  System Error:{this.MonitoringManager().IsSystemError} ,StageSystemError:{this.MonitoringManager().IsStageSystemError} ");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Pause()
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(_LotModuleState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ModuleState.GetState();
        }

        public ModuleStateEnum Resume()
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(_LotModuleState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ModuleState.GetState();
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                PreInnerState = _LotModuleState;
                InnerState = state;
                this.LoaderController()?.BroadcastLotState(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        /// <summary>
        /// Lot 시작시에 Data 초기화 필요한 부분있으면 여기서 하기.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum InitData()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                this.GEMModule().GetPIVContainer().WaferID.Value = string.Empty;

                this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                this.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.IDLE);
                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);

                this.SoakingModule().SoackingDone = false;

                foreach (IStateModule module in RunList)
                {
                    ret = module.ClearState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum RetVal = ModuleStateEnum.UNDEFINED;
            try
            {
                Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public ModuleStateEnum Execute()
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;

            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                stat = (ModuleStateEnum)_LotModuleState.Execute();
                retval = ModuleState.StateTransition(_LotModuleState.GetModuleState());

                if (retval == EventCodeEnum.STATE_TRANSITION_OK)
                {
                    // Lot State 변경시, 상태 변경 이벤트 발생.
                    // 현재 IDLE, RUN, PAUSE, ERROR 로 변경 시..

                    if (LotStateEnum == LotOPStateEnum.IDLE ||
                        LotStateEnum == LotOPStateEnum.RUNNING ||
                        LotStateEnum == LotOPStateEnum.PAUSED ||
                        LotStateEnum == LotOPStateEnum.ERROR)
                    {
                        retval = this.EventManager().RaisingEvent(typeof(ProberStatusEvent).FullName);
                    }

                    string moduleState = ModuleState.State.ToString();
                    this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.LOTSTATE, moduleState);

                    LoggerManager.SetLotState(moduleState);
                }

                if (IsDllLoad())
                {
                    LoadDll();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                ModuleState.StateTransition(ModuleStateEnum.ERROR);

                this.MonitoringManager()?.StageEmergencyStop();
            }

            return stat;
        }
        /*
        //3D view
        private object _View3DTarget;
        public object View3DTarget
        {
            get { return _View3DTarget; }
            set
            {
                if (value != _View3DTarget)
                {

                    _View3DTarget = value;
                    RaisePropertyChanged();
                }
            }
        }
        */
        //
        public void StateTransition(ModuleStateBase state)
        {
            try
            {
                ModuleState = state;

                if (state.GetState() == ModuleStateEnum.IDLE)
                {
                    var previousLoadedWaferCount = LotInfo.LoadedWaferCountUntilBeforeLotStart;
                    LotInfo.LoadedWaferCountUntilBeforeLotStart = 0;

                    LoggerManager.Debug($"[{this.GetType().Name}], SetStatusLoaded() : previous LoadedWaferCountUntilBeforeLotStart = {previousLoadedWaferCount}, Current LoadedWaferCountUntilBeforeLotStart = {LotInfo.LoadedWaferCountUntilBeforeLotStart}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private EventCodeEnum SetModuleListInLotRunList()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                // 순서 함부로 임의로 바꾸지 말것!!!!!!!!!!!!!!!!!!
                RunList.Add(this.DeviceModule());
                //RunList.Add(this.EnvModule());
                RunList.Add(this.AirBlowChuckCleaningModule());
                RunList.Add(this.PinAligner());
                RunList.Add(this.AirBlowWaferCleaningModule());
                RunList.Add(this.WaferAligner());
                // RunList.Add(this.MarkAligner());
                RunList.Add(this.FDWaferAligner()); // 251013 sebas
                RunList.Add(this.PMIModule());
                RunList.Add(this.PolishWaferModule());
                RunList.Add(this.SoakingModule());
                RunList.Add(this.NeedleCleaner());
                //RunList.Add(this.NeedleBrush());
                RunList.Add(this.WaferTransferModule());
                RunList.Add(this.BonderModule());   // 251013 sebas
                RunList.Add(this.ProbingModule()); //제일 아래 있어야해. 무조건.

                foreach (var module in RunList)
                {
                    LoggerManager.InitialModuleState(module.GetType().Name, module.ModuleState?.GetState().ToString());
                }

                LoggerManager.UpdateModuleState(true);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }   
            return retval;
        }
        public void ClearToken()
        {
            try
            {
                foreach (IStateModule module in RunList)
                {
                    module.CommandSendSlot.ClearToken();
                    module.CommandRecvSlot.ClearToken();
                    module.CommandInfo = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    if (LotInfo == null)
                    {
                        LotInfo = new LotInfo();
                        SetDeviceName(this.FileManager().FileManagerParam.DeviceName);
                    }

                    if (SystemInfo == null)
                    {
                        if (SystemManager.SysteMode == SystemModeEnum.Single)
                        {
                            SystemInfo = new SystemInfo();
                        }
                        else
                        {
                            string filepath = string.Empty;
                            string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";

                            filepath = $@"C:\ProberSystem\SystemInfo\{cellNo}\SystemInfo.txt";

                            SystemInfo = new SystemInfo(filepath);
                        }

                        retval = SystemInfo.LoadInfo();

                    }

                    if (DeviceInfo == null)
                    {
                        DeviceInfo = new DeviceInfo();
                        //retval = DeviceInfo.LoadInfo();
                    }

                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        _LotModuleState = new LotOPIdleState(this);
                    }
                    else
                    {
                        _LotModuleState = new GP_LotOPIdleState(this);
                    }

                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(_LotModuleState.GetModuleState());

                    string moduleState = ModuleState.State.ToString();
                    LoggerManager.SetLotState(moduleState);

                    //View3DTarget = View3D;

                    retval = SetModuleListInLotRunList();

                    DisplayPort = new DisplayPort() { GUID = new Guid("C4E51FA2-3384-40CF-9790-3B6F2BA4A817") };
                    LoaderDisplayPort = new DisplayPort()
                    {
                        GUID = new Guid("369E0EC1-1A75-404B-825D-15F03B634B6D"),
                        EnalbeClickToMove = false
                    };


                    Array stagecamvalues = Enum.GetValues(typeof(StageCam));
                    Array loadercamvalues = Enum.GetValues(typeof(LoaderCam));

                    if (this.VisionManager().CameraDescriptor != null)
                    {
                        foreach (var cam in this.VisionManager().CameraDescriptor.Cams)
                        {
                            for (int index = 0; index < stagecamvalues.Length; index++)
                            {
                                if (((StageCam)stagecamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                                {
                                    this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                                    break;
                                }
                            }
                        }

                        foreach (var cam in this.VisionManager().CameraDescriptor.Cams)
                        {
                            for (int index = 0; index < loadercamvalues.Length; index++)
                            {
                                if (((LoaderCam)loadercamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                                {
                                    this.VisionManager().SetDisplayChannel(cam, LoaderDisplayPort);
                                    break;
                                }
                            }
                        }
                    }

                    retval = this.EventManager().RegisterEvent(typeof(CardChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(CellLotAbortedByUser).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LotStartEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LotResumeEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LotEndEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(StageMachineInitCompletedEvent).FullName, "ProbeEventSubscibers", EventFired);

                    //LotInfo.LotProcessingVerifed.ValueChangedEvent += LotProcessingVerifed_ValueChangedEvent;
                    LotInfo.LotMode.ValueChangedEvent += LotModeEnum_ValueChangedEvent;

                    ViewTarget = Wafer;
                    //MapViewTarget = Wafer;
                    //   DisPortViewTarget = DisplayPort;
                    //   NCViewTarget = NCObject;
                    //DisPortVisible = Visibility.Hidden;
                    //NCVisible = Visibility.Hidden;
                    //MapViewVisible = Visibility.Visible;
                    MiniViewTarget = LoaderDisplayPort;
                    MiniVisible = Visibility.Hidden;
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //private void LotProcessingVerifed_ValueChangedEvent(object oldValue, object newValue)
        //{
        //    try
        //    {
        //        this.LoaderController()?.UpdateLotVerifyInfo(LotInfo.LotProcessingVerifed.Value);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private void LotModeEnum_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.LOTMODE, LotInfo.LotMode.Value.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is CardChangedEvent)
                {
                    this.SystemInfo.ResetProcessedWaferCountUntilBeforeCardChange();
                    this.SystemInfo.ResetTouchDownCountUntilBeforeCardChange();
                    LoggerManager.Debug($"[LotOpModule] EventFired() : sender = CardChangedEvent");
                }
                else if (sender is CellLotAbortedByUser)
                {
                    LoggerManager.Debug($"[LotOpModule] EventFired() : sender = CellLotAbortedByUser");
                    LotAbortedByUser = true;
                }
                else if (sender is StageMachineInitCompletedEvent)
                {
                    LoggerManager.Debug($"[LotOpModule] EventFired() : sender = StageMachineInitCompletedEvent");
                    LotAbortedByUser = false;
                }
                else if (sender is LotEndEvent)
                {
                    LoggerManager.Debug($"[LotOpModule] EventFired() : sender = LotEndEvent");
                    LotAbortedByUser = false;
                }
                else if (sender is LotStartEvent)
                {
                    // Lot 관련 flag 오동작으로 인해 초기화 안되었을 수 있으니 초기 화 하기 위함
                    LoggerManager.Debug($"[LotOpModule] EventFired() : sender = LotStartEvent");
                    LotAbortedByUser = false;
                }
                else if (sender is LotResumeEvent)
                {
                    // Lot 관련 flag 오동작으로 인해 초기화 안되었을 수 있으니 초기 화 하기 위함
                    LoggerManager.Debug($"[LotOpModule] EventFired() : sender = LotResumeEvent");
                    LotAbortedByUser = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        bool LotAbortedByUser = false;
        public bool IsLotAbortedByUser()
        {
            bool ret = false;
            try
            {
                ret = LotAbortedByUser;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public int PauseRequest(object caller)
        {
            //if (Injector != null)
            //{
            //    if (Requestors == null) Requestors = new List<object>();
            //    Requestors.Add(Injector);
            //    Injector = null;
            //}

            //LotOPStateTransition(new LotOPPausedState(this));

            return 1;
        }

        public int ResumeRequest(object caller)
        {
            //LotOPStateTransition(new LotOPPausedState(this));
            return 1;
        }

        public int StartRequest(object caller)
        {
            //SetInjector(caller);
            //LotOPStateTransition(new LotOPRunningState(this));

            return 1;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool backupStopBeforeProbing = false;
                bool backupStopAfterProbing = false;
                bool backupOnceStopBeforeProbing = false;
                bool backupOnceStopAfterProbing = false;

                if (LotDevParam != null)
                {
                    backupStopBeforeProbing = LotDevParam.StopOption.StopBeforeProbing.Value;
                    backupStopAfterProbing = LotDevParam.StopOption.StopAfterProbing.Value;
                    LoggerManager.Debug($"LotDevParam.StopOption.StopBeforeProbing : {backupStopBeforeProbing}");
                    LoggerManager.Debug($"LotDevParam.StopOption.StopAfterProbing : {backupStopAfterProbing}");

                    backupOnceStopBeforeProbing = LotDevParam.StopOption.OnceStopBeforeProbing.Value;
                    backupOnceStopAfterProbing = LotDevParam.StopOption.OnceStopAfterProbing.Value;
                    LoggerManager.Debug($"LotDevParam.StopOption.OnceStopBeforeProbing : {backupOnceStopBeforeProbing}");
                    LoggerManager.Debug($"LotDevParam.StopOption.OnceStopAfterProbing : {backupOnceStopAfterProbing}");
                }

                IParam tmpParam = null;
                tmpParam = new LotDeviceParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(LotDeviceParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    LotDeviceParam_IParam = tmpParam;
                    LotDevParam = LotDeviceParam_IParam as LotDeviceParam;
                    LotDevParam.OperatorStopOption = new LotStopOption();
                    LotDevParam.StopOption.StopBeforeProbing.Value = backupStopBeforeProbing;
                    LotDevParam.StopOption.StopAfterProbing.Value = backupStopAfterProbing;
                    LotDevParam.StopOption.OnceStopBeforeProbing.Value = backupOnceStopBeforeProbing;
                    LotDevParam.StopOption.OnceStopAfterProbing.Value = backupOnceStopAfterProbing;

                    StopBeforeProbingChangedEvent();
                    StopAfterProbingChangedEvent();

                    OnceStopBeforeProbingChangedEvent();
                    OnceStopAfterProbingChangedEvent();

                    LotDevParam.StopOption.StopBeforeProbing.PropertyChanged += ProbingStopOption_PropertyChanged;
                    LotDevParam.StopOption.StopAfterProbing.PropertyChanged += ProbingStopOption_PropertyChanged;

                    LotDevParam.StopOption.OnceStopBeforeProbing.PropertyChanged += ProbingStopOption_PropertyChanged;
                    LotDevParam.StopOption.OnceStopAfterProbing.PropertyChanged += ProbingStopOption_PropertyChanged;

                    //LotDevParam.StopOption.StopBeforeProbing.PropertyChanged += StopBeforeProbingChangedEvent;
                    //LotDevParam.StopOption.StopAfterProbing.PropertyChanged += StopAfterProbingChangedEvent;
                    //LotDevParam.OperatorStopOption.CopyTo(LotDevParam.StopOption);
                }
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateStopOptionToStage(this.LoaderController().GetChuckIndex());
                }
                //DevParam = LotDevParam;

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    retval = this.SaveParameter(LotDeviceParam_IParam);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new AppModuleItems();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(AppModuleItems));

                if (retval == EventCodeEnum.NONE)
                {
                    AppItems_IParam = tmpParam;
                    AppItems = AppItems_IParam as AppModuleItems;
                }
                tmpParam = null;
                tmpParam = new LotSystemParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(LotSystemParam));

                if (retval == EventCodeEnum.NONE)
                {
                    LotSysParam = tmpParam as LotSystemParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = SaveAppItems();
                RetVal = this.SaveParameter(LotSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveAppItems()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (AppItems != null)
                {
                    RetVal = this.SaveParameter(AppItems);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum ClearState()
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

        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {

                RetVal = _LotModuleState.CanExecute(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventProcessList SubscribeRecipeParam { get; set; }
        private Queue<ModuleDllInfo> _DllInfos = new Queue<ModuleDllInfo>();
        public Queue<ModuleDllInfo> DllInfos
        {
            get { return _DllInfos; }
            set
            {
                if (value != _DllInfos)
                {
                    _DllInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        public LotOPStateEnum LotStateEnum
        {
            get
            {
                return _LotModuleState.GetState();
            }
        }

        public EventCodeEnum LoadLotOPSubscribeRecipe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                SubscribeRecipeParam = new EventProcessList();

                string FullPath;

                FullPath = this.FileManager().GetSystemParamFullPath("Event", "Recipe_Subscribe_LotOP.json");

                try
                {
                    if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                    }


                    if (File.Exists(FullPath) == false)
                    {
                        SubscribeRecipeParam.Add(new LotEventProc_CassetteLoadDone() { EventFullName = typeof(CassetteLoadDoneEvent).FullName });

                        RetVal = Extensions_IParam.SaveParameter(null, SubscribeRecipeParam, null, FullPath);

                        if (RetVal == EventCodeEnum.PARAM_ERROR)
                        {
                            LoggerManager.Error($"[LotOpModule] LoadDevParam(): Serialize Error");
                            return RetVal;
                        }
                    }

                    IParam tmpPram = null;
                    RetVal = this.LoadParameter(ref tmpPram, typeof(EventProcessList), null, FullPath);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        SubscribeRecipeParam = tmpPram as EventProcessList;
                    }
                    else
                    {
                        RetVal = EventCodeEnum.PARAM_ERROR;

                        LoggerManager.Error($"[LotOpModule] LoadSysParam(): DeSerialize Error");
                        return RetVal;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($String.Format("[LotOPModule] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum RegistEventSubscribe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = LoadLotOPSubscribeRecipe();

                foreach (var evtname in SubscribeRecipeParam)
                {
                    evtname.OwnerModuleName = "LOT";
                    RetVal = this.EventManager().RegisterEvent(evtname.EventFullName, "ProbeEventSubscibers", evtname.EventNotify);

                    if (RetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LotOP] Regist EventSubscribe Error...");

                        break;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public void InsertDllInfo(ModuleDllInfo DllInfo)
        {
            try
            {
                //using (Locker locker = new Locker(lockObject))
                //{
                lock (lockObject)
                {
                    _DllInfos.Enqueue(DllInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ModuleStateEnum Abort()
        {
            InnerState.Abort();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }
        public EventCodeEnum LoadDll()
        {
            EventCodeEnum retVal = EventCodeEnum.NODATA;
            try
            {
                //using (Locker locker = new Locker(lockObject))
                //{
                lock (lockObject)
                {

                    ModuleDllInfo dllInfo = DllInfos.Dequeue();
                    //Prober.ProberViewModel.LoadProberMainScreen(dllInfo.DLLPath,dllInfo.ParamName);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($err + "LoadDllInfo() : Error occured.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsDllLoad()
        {
            //using (Locker locker = new Locker(lockObject))
            //{
            lock (lockObject)
            {
                return (_DllInfos.Count > 0) ? true : false;
            }
        }

        public void VisionScreenToLotScreen()
        {
            try
            {
                ViewTarget = DisplayPort;
                LoggerManager.Debug("DisplayPort HashCode: " + DisplayPort.GetHashCode());
                SwitchVisiability = Visibility.Hidden;
                ZoomVisiability = Visibility.Hidden;
                //if (this.StageSupervisor().StageMode == GPCellModeEnum.ONLINE & this.StageSupervisor().StreamingMode == StreamingModeEnum.STREAMING_ON)
                //    this.LoaderRemoteMediator().GetServiceCallBack()?.VisionScreenToLotScreen(this.LoaderController().GetChuckIndex());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void MapScreenToLotScreen()
        {
            try
            {
                ViewTarget = Wafer;
                LoggerManager.Debug("Wafer HashCode: " + Wafer.GetHashCode());
                SwitchVisiability = Visibility.Visible;
                ZoomVisiability = Visibility.Visible;
                if (this.ModuleState.GetState() != ModuleStateEnum.RUNNING)
                    Wafer.MapViewCurIndexVisiablity = false;
                else
                    Wafer.MapViewCurIndexVisiablity = true;

                //if(this.StageSupervisor().StageMode == GPCellModeEnum.ONLINE & this.StageSupervisor().StreamingMode == StreamingModeEnum.STREAMING_ON)
                //    this.LoaderRemoteMediator().GetServiceCallBack()?.MapScreenToLotScreen(this.LoaderController().GetChuckIndex());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void NCToLotScreen()
        {
            try
            {
                ViewTarget = NCObject;
                LoggerManager.Debug("NCObject HashCode: " + NCObject.GetHashCode());
                SwitchVisiability = Visibility.Visible;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ChangeMainViewUserTarget(object target)
        {
            try
            {
                ViewTarget = target;
                LoggerManager.Debug("target HashCode: " + target.GetHashCode());
                SwitchVisiability = Visibility.Hidden;
                ZoomVisiability = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ChangePreMainViewTarget()
        {
            try
            {
                ViewTarget = PreViewTarget;
                LoggerManager.Debug("PreViewTarget HashCode: " + PreViewTarget.GetHashCode());
                SwitchVisiability = Visibility.Visible;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void LoaderScreenToLotScreen()
        {
            try
            {
                MiniVisible = Visibility.Visible;
                SwitchVisiability = Visibility.Visible;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void HiddenLoaderScreenToLotScreen()
        {
            try
            {
                MiniVisible = Visibility.Hidden;
                SwitchVisiability = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetLotViewDisplayChannel()
        {
            try
            {
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                this.VisionManager().SetDisplyChannelLoaderCameras(LoaderDisplayPort);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ViewSwip()
        {
            try
            {
                object swap = ViewTarget;
                ViewTarget = MiniViewTarget;
                MiniViewTarget = swap;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool CanLotPauseState(ModuleStateEnum state)
        {
            bool retVal = false;
            try
            {
                if (state == ModuleStateEnum.ERROR ||
                    state == ModuleStateEnum.PAUSED ||
                    state == ModuleStateEnum.RECOVERY)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// LOT START command 를 받을 수 있는 상태인지 확인하는 함수
        /// true : 명령을 받을 수 있는 상태
        /// false : 이미 받은 명령이 있거나, 받을 수 없는 상태
        /// </summary>
        public bool IsCanPerformLotStart()
        {
            bool retVal = false;
            try
            {
                bool isHaveLotOpStartCommnad = false;
                bool isLotIdleState = false;
                if(CommandRecvSlot.IsRequested<ILotOpStart>())
                {
                    isHaveLotOpStartCommnad = true;
                }
                if(ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    isLotIdleState = true;
                }


                retVal = !isHaveLotOpStartCommnad && isLotIdleState;
                LoggerManager.Debug($"[LotOPModule] IsCanPerformLotStart(). IsHabeLotOPStartCommand : {isHaveLotOpStartCommnad}, isLotIdleState: {isLotIdleState}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        /// <summary>
        /// End 하려는 Lot와 현재 돌고 있는 Lot 의 정보가 일치하는지 확인하는 함수
        /// </summary>
        /// <param name="foupidx"></param>
        /// <param name="lotID"></param>
        /// <param name="cstHashCode"></param>
        /// <returns></returns>
        public bool IsCanPerformLotEnd(int foupidx, string lotID, string cstHashCode,bool isCheckHashCode)
        {
            bool retVal = false;
            try
            {
                // 이미 진행중인 lot 를 cancel 할 경우
                if(isCheckHashCode)
                {
                    if (LotInfo.FoupNumber.Value == foupidx &&
                    LotInfo.LotName.Value == lotID &&
                    LotInfo.CSTHashCode == cstHashCode)
                    {
                        // possible lot end
                        retVal = true;
                    }
                }
                else
                {
                    if (LotInfo.FoupNumber.Value == foupidx &&
                        LotInfo.LotName.Value == lotID)
                    {
                        // possible lot end
                        retVal = true;
                    }
                }
                                                            
                if(retVal)
                {
                    LoggerManager.Debug($"[LotOPModule] IsCanPerformLotEnd(). Possible Lot End. Foup Index: {foupidx.ToString()}, Lot ID: {lotID}, CST HashCode: {cstHashCode}");
                }
                else
                {
                    LoggerManager.Debug($"[LotOPModule] IsCanPerformLotEnd(). Impossible Lot End. Request Lot End Info: Foup Index: {foupidx.ToString()}, Lot ID: {lotID}, CST HashCode: {cstHashCode} " +
                                        $"Current Lot Running Info: Foup Index: {LotInfo.FoupNumber.Value.ToString()}, Lot ID: {LotInfo.LotName.Value}, CST HashCode: {LotInfo.CSTHashCode}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void ValidateCancelLot(bool iscellend, int foupNumber, string lotID, string cstHashCode)
        {
            bool isCanPerformLotEndFlagWithHashCode = true;
            bool isCanPerformLotEndFlagWithoutHashCode = true;

            try
            {
                string effectivelotID = string.Empty;
                string effectiveCstHashCode = string.Empty;

                if (string.IsNullOrEmpty(lotID))
                {
                    effectivelotID = LotInfo.LotName.Value;
                }
                else
                {
                    effectivelotID = lotID;
                }

                if (string.IsNullOrEmpty(cstHashCode))
                {
                    effectiveCstHashCode = LotInfo.CSTHashCode;
                }
                else
                {
                    effectiveCstHashCode = cstHashCode;
                }

                LotAssignStateEnum lotAssignStateEnum = LotInfo.GetStageLotAssignState(effectivelotID, effectiveCstHashCode);

                if (lotAssignStateEnum != LotAssignStateEnum.CANCEL && lotAssignStateEnum != LotAssignStateEnum.JOB_FINISHED)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], ValidateCancelLot() : lotID = {lotID}, effectivelotID = {effectivelotID}, cstHashCode = {cstHashCode}, effectiveCstHashCode = {effectiveCstHashCode}, lotAssignStateEnum = {lotAssignStateEnum}, foupNumber = {foupNumber}, iscellend = {iscellend}.");
                }

                if (lotAssignStateEnum == LotAssignStateEnum.ASSIGNED ||
                    lotAssignStateEnum == LotAssignStateEnum.PROCESSING)
                {
                    // cancel 이 아닌경우에만 호출하도록
                    if (foupNumber != -1) // foupnumber == -1는 강제 종료의미이므로 IsCanPerformLotEnd 안본다.
                    {
                        isCanPerformLotEndFlagWithHashCode = IsCanPerformLotEnd(foupNumber, effectivelotID, effectiveCstHashCode, true);
                        isCanPerformLotEndFlagWithoutHashCode = IsCanPerformLotEnd(foupNumber, effectivelotID, effectiveCstHashCode, false);
                    }

                    if (isCanPerformLotEndFlagWithHashCode)
                    {
                        if (iscellend)
                        {
                            /// <!-- iscellend = true 는 Manual Loader/Cell End 한 경우 -->
                            this.ProbingModule().SetProbingEndState(ProbingEndReason.MANUAL_LOT_END, EnumWaferState.UNDEFINED);
                        }
                        else
                        {
                            this.ProbingModule().SetProbingEndState(ProbingEndReason.OTHER_REJECT);
                        }
                    }

                    if (isCanPerformLotEndFlagWithoutHashCode)
                    {
                        this.LoaderController().LotCancelSoakingAbort(this.LoaderController().GetChuckIndex());
                    }

                    this.LoaderController().IsCancel = isCanPerformLotEndFlagWithoutHashCode | iscellend;

                    // TODO selly lot end 시, 돌고있던 것이든, 할당되어있던  다 cancel 로 만들어줘야하므로, 추가로 볼 조건 필요.
                    LotInfo.SetStageLotAssignState(LotAssignStateEnum.CANCEL, effectivelotID, effectiveCstHashCode);

                    // 1. not_exist 인경우,
                    // 2. exist 이지만, 취소하려는 cstHashCode 비교해서 달라도 이벤트 발생
                    EnumSubsStatus enumSubsStatus = this.GetParam_Wafer().GetStatus();

                    if ((enumSubsStatus == EnumSubsStatus.NOT_EXIST ||
                       (enumSubsStatus == EnumSubsStatus.EXIST && effectiveCstHashCode != LotInfo.CSTHashCode))
                       && lotAssignStateEnum == LotAssignStateEnum.PROCESSING)
                    {
                        LotInfo.SetStageLotAssignState(LotAssignStateEnum.JOB_FINISHED, effectivelotID, effectiveCstHashCode);

                        PIVInfo pivinfo = new PIVInfo(foupnumber: foupNumber);

                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(StageDeallocatedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
            return true;
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public void UpdateWafer(IWaferObject waferObject)
        {
            this.LotInfo.UpdateWafer(waferObject);
        }
        public void InitLotScreen()
        {
            ViewTarget = null;
            MiniViewTarget = null;
        }
        public void ViewTargetUpdate()
        {
            try
            {
                if (this.ViewTarget is IWaferObject)
                {
                    this.ViewTarget = Wafer;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public void SetDeviceName(string devicename)
        {
            try
            {
                LotInfo.DeviceName.Value = devicename;
                LoggerManager.SetDeviceName(LotInfo.DeviceName.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                LotOPStateEnum state = (InnerState as LotOPState).GetState();

                retval = state.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool GetErrorEndFlag()
        {
            return ErrorEndFlag;
        }

        public void SetErrorEndFalg(bool flag)
        {
            ErrorEndFlag = flag;
        }

        public void SetErrorState()
        {
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    InnerStateTransition(new LotOPErrorState(this));
                }
                else
                {
                    InnerStateTransition(new GP_LotOPErrorState(this));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StopBeforeProbingChangedEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                this.LoaderController().SetStopBeforeProbingFlag(LotDevParam.StopOption.StopBeforeProbing.Value);
                LoggerManager.Debug($"Stop Before Probing Option changed to {LotDevParam.StopOption.StopBeforeProbing.Value}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StopAfterProbingChangedEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                this.LoaderController().SetStopAfterProbingFlag(LotDevParam.StopOption.StopAfterProbing.Value);
                LoggerManager.Debug($"Stop After Probing Option changed to {LotDevParam.StopOption.StopAfterProbing.Value}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void OnceStopBeforeProbingChangedEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                this.LoaderController().SetOnceStopBeforeProbingFlag(LotDevParam.StopOption.OnceStopBeforeProbing.Value);
                LoggerManager.Debug($"Stop Once Before Probing Option changed to {LotDevParam.StopOption.OnceStopBeforeProbing.Value}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void OnceStopAfterProbingChangedEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                this.LoaderController().SetOnceStopAfterProbingFlag(LotDevParam.StopOption.OnceStopAfterProbing.Value);
                LoggerManager.Debug($"Stop Once After Probing Option changed to {LotDevParam.StopOption.OnceStopAfterProbing.Value}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int GetLotPauseTimeoutAlarm()
        {
            return LotSysParam?.LotPauseTimeoutAlarm?.Value ?? 0;
        }
        public void SetLotPauseTimeoutAlarm(int time)
        {
            try
            {
                if (LotSysParam != null)
                {
                    double preTimeOut = LotSysParam.LotPauseTimeoutAlarm.Value;
                    LotSysParam.LotPauseTimeoutAlarm.Value = time;
                    LoggerManager.Debug($"LotPauseTimeoutAlarm {preTimeOut}(sec) change to {time}(sec).");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ProbingStopOption_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                this.LoaderController().SetStopBeforeProbingFlag(LotDevParam.StopOption.StopBeforeProbing.Value);
                this.LoaderController().SetStopAfterProbingFlag(LotDevParam.StopOption.StopAfterProbing.Value);
                this.LoaderController().SetOnceStopBeforeProbingFlag(LotDevParam.StopOption.OnceStopBeforeProbing.Value);
                this.LoaderController().SetOnceStopAfterProbingFlag(LotDevParam.StopOption.OnceStopAfterProbing.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateLotName(string lotname)
        {
            try
            {
                LotInfo.LotName.Value = lotname;
                this.LoaderController().UpdateLotDataInfo(StageLotDataEnum.LOTNAME, LotInfo.LotName.Value);

                LoggerManager.SetLotName(LotInfo.LotName.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateWaferID(string id)
        {
            try
            {
                this.GetParam_Wafer().GetSubsInfo().WaferID.Value = id;

                LoggerManager.SetWaferID(this.GetParam_Wafer().GetSubsInfo().WaferID.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        } 
        
      
    }
}

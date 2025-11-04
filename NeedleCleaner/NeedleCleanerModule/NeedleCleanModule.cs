//using ModuleParameter;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.AlignEX;
using ProberInterfaces.Command;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NeedleCleanerModuleParameter;
using ProberInterfaces.NeedleClean;
using PnPControl;
using LogModule;
using DllImporter;
using ProberInterfaces.Param;
using SubstrateObjects;
using NotifyEventModule;
using ProberInterfaces.Event;
using ProberInterfaces.Template;
using NeedleCleanerModuleParameter.Template;
using System.Runtime.CompilerServices;
using NeedleCleanHeightProfilingModule;
using NeedleCleanHeightProfilingParamObject;
using SequenceRunner;
using ProberInterfaces.SequenceRunner;
using NeedleCleanViewer;

namespace NeedleCleanerModule
{
    public class NeedleCleanModule : INeedleCleanModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
        public bool Initialized { get; set; } = false;

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }
        
        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.NeedleClean);
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


        //private IParam _SysCommonParam;
        //[ParamIgnore]
        //public IParam SysCommonParam
        //{
        //    get { return _SysCommonParam; }
        //    set
        //    {
        //        if (value != _SysCommonParam)
        //        {
        //            _SysCommonParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        private ProbeAxisObject _NCAxis;
        [ParamIgnore]
        public ProbeAxisObject NCAxis
        {
            get
            {
                //if(_NCAxis == null)
                //{
                //    _NCAxis = this.MotionManager().GetAxis(
                //    ((NeedleCleanSystemParameter)this.StageSupervisor().NeedleCleanObject.SysParam).AxisType.Value
                //    );
                //}
                return _NCAxis;
            }
            set
            {
                if (value != _NCAxis)
                {
                    _NCAxis = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _NeedleCleanDeviceParameter_IParam;
        [ParamIgnore]
        public IParam NeedleCleanDeviceParameter_IParam
        {
            get { return _NeedleCleanDeviceParameter_IParam; }
            set
            {
                if (value != _NeedleCleanDeviceParameter_IParam)
                {
                    _NeedleCleanDeviceParameter_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NeedleCleanDeviceParameter _NeedleCleanDeviceParameter_Clone;
        public NeedleCleanDeviceParameter NeedleCleanDeviceParameter_Clone
        {
            get { return _NeedleCleanDeviceParameter_Clone; }
            set
            {
                if (value != _NeedleCleanDeviceParameter_Clone)
                {
                    _NeedleCleanDeviceParameter_Clone = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NeedleCleanObject NC { get { return this.StageSupervisor().NCObject as NeedleCleanObject; } }

        private IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }

        public WaferObject WaferObject { get { return (WaferObject)this.StageSupervisor().WaferObject; } }

        private double _ZoomLevel;
        public double ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ISubRoutine _SubRoutine;
        //public ISubRoutine SubRoutine
        //{
        //    get { return _SubRoutine; }
        //    set { _SubRoutine = value; }
        //}

        private NeedleCleanViewModel _NeedleCleanVM;
        public NeedleCleanViewModel NeedleCleanVM
        {
            get { return _NeedleCleanVM; }
            set
            {
                if (value != _NeedleCleanVM)
                {
                    _NeedleCleanVM = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private NeedleCleanSystemCommonParameter _NeedleCleanSysCommonParam;
        //public NeedleCleanSystemCommonParameter NeedleCleanSysCommonParam
        //{
        //    get { return _NeedleCleanSysCommonParam; }
        //    set
        //    {
        //        if (value != _NeedleCleanSysCommonParam)
        //        {
        //            _NeedleCleanSysCommonParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private NeedleCleanDeviceParameter _NeedleCleanDevParam_Clone;
        //public NeedleCleanDeviceParameter NeedleCleanDevParam_Clone
        //{
        //    get { return _NeedleCleanDevParam_Clone; }
        //    set
        //    {
        //        if (value != _NeedleCleanDevParam_Clone)
        //        {
        //            _NeedleCleanDevParam_Clone = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private SubStepModules _SubModules;
        //public ISubStepModules SubModules
        //{
        //    get { return _SubModules; }
        //    set
        //    {
        //        if (value != _SubModules)
        //        {
        //            _SubModules = (SubStepModules)value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        private SequenceBehaviors _NCPadChangeBehaviors;
        public SequenceBehaviors NCPadChangeBehaviors
        {
            get { return _NCPadChangeBehaviors; }
            set
            {
                if (_NCPadChangeBehaviors != value)
                {
                    _NCPadChangeBehaviors = value;
                    RaisePropertyChanged();
                }
            }
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

        private CommandTokenSet _RunTokenSet;
        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }

        private ModuleStateBase _ModuleState; //모듈의 현재 대표 State
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
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

        private NeedleCleanModuleStateBase _NeedleCleanState;
        public NeedleCleanModuleStateBase NeedleCleanState
        {
            get { return _NeedleCleanState; }
            set
            {
                if (value != _NeedleCleanState)
                {
                    _NeedleCleanState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private INeedleCleanHeightProfiling _NCHeightProfilingModule;
        public INeedleCleanHeightProfiling NCHeightProfilingModule
        {
            get { return _NCHeightProfilingModule; }
            set
            {
                if (value != _NCHeightProfilingModule)
                {
                    _NCHeightProfilingModule = value;
                    RaisePropertyChanged();
                }
            }
        }


        public IInnerState InnerState // Inner모듈의 현재 State
        {
            get { return _NeedleCleanState; }
            set
            {
                if (value != _NeedleCleanState)
                {
                    _NeedleCleanState = value as NeedleCleanModuleStateBase;
                }
            }
        }

        public IInnerState PreInnerState { get; set; } //Inner모듈의 이전 State


        #region //..Template
        private TemplateStateCollection _Template;
        public TemplateStateCollection Template
        {
            get { return _Template; }
            set
            {
                if (value != _Template)
                {
                    _Template = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ITemplateFileParam _TemplateParameter;
        public ITemplateFileParam TemplateParameter
        {
            get { return _TemplateParameter; }
            set
            {
                if (value != _TemplateParameter)
                {
                    _TemplateParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ITemplateParam LoadTemplateParam { get; set; }
        public ISubRoutine SubRoutine { get; set; }
        #endregion


        public CleanUnitFocusing5pt DelFocusing { get; set; }
        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        public bool CanExecute(IProbeCommandToken token) // 커맨드가 날라왔을때 할수 있는지 보는 행동
        {
            throw new NotImplementedException();
        }

        public void DeInitModule() //DeInit해야할때 해야 하는 행동
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

        public EventCodeEnum InitModule() // Init해야할때 해야 하는 행동
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            //Template을 아직 사용안하므로 InitModule 에서 직접 초기화.
            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();
                    
                    InnerState = new NeedleCleanModuleIdleState(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    var stage = this.StageSupervisor();
                    NCAxis = this.MotionManager().GetAxis(NC.NCSysParam.AxisType.Value);

                    NeedleCleanDeviceParameter deviceParam = (NeedleCleanDeviceParameter_IParam != null) ? NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter : null;

                    NeedleCleanVM = new NeedleCleanViewModel();

                    ZoomLevel = 4;

                    retval = this.EventManager().RegisterEvent(typeof(WaferUnloadedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LotStartEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LotEndEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(MachineInitEvent).FullName, "ProbeEventSubscibers", EventFired);

                    this.TemplateManager().InitTemplate(this);

                    NCHeightProfilingModule = new NeedleCleanHeightProfiling();
                    retval = (NCHeightProfilingModule as IHasSysParameterizable).LoadSysParameter();
                    retval = (NCHeightProfilingModule as IModule).InitModule();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"HeightProfiling Parameter Load is failid in NeedleClean Module.");
                    }

                    retval = LoadNeedleCleanPadChangeBehaviors();

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
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is LotEndEvent || sender is LotStartEvent)
                {
                    foreach (var list in NC.NCSheetVMDefs)
                    {
                        list.FlagCleaningForCurrentLot = false;
                        list.FlagFocusedForCurrentLot = false;
                    }
                }
                else if (sender is WaferUnloadedEvent)
                {
                    foreach (var list in NC.NCSheetVMDefs)
                    {
                        list.FlagFocusedForCurrentWafer = false;
                    }
                }
                else if (sender is MachineInitEvent)
                {
                    NC.NCSheetVMDef.FlagTouchSensorBaseConfirmed = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public async Task<EventCodeEnum> Focusing5pt(int ncNum)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                // Test Code
                //while (true)
                //    {
                //        this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                //        await Task.Run(() => RetVal = FocusAsyncCommnad(ncNum));
                //        this.ViewModelManager().UnLock(this.GetHashCode());
                //    }

                //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                

                await Task.Run(() => RetVal = FocusAsyncCommnad(ncNum));
                

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanModule - FocusAsyncCommnad() : Error occured.");
                RetVal = EventCodeEnum.UNDEFINED;
            }

            return RetVal;

        }

        public EventCodeEnum FocusAsyncCommnad(int ncNum)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.StageSupervisor().StageModuleState.ZCLEARED();

                retVal = this.DelFocusing.Invoke(ncNum);

            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanModule - FocusAsyncCommnad({ncNum}) : Error occured.");
                throw err;
            }

            return retVal;
        }

        public EventCodeEnum DoNeedleCleaningProcess()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                bool doneFlag = false;

                List<ISubModule> modules = Template.GetProcessingModule();

                foreach (var subModule in modules)
                {
                    retVal = subModule.ClearData();
                    retVal = subModule.Execute();

                    if (subModule.GetState() == SubModuleStateEnum.ERROR)
                    {
                        //InnerStateTransition(new NeedleCleanModuleErrorState(this));
                        InnerStateTransition(new NeedleCleanModuleErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, "ERROR", this.NeedleCleanState.GetType().Name)));

                        break;
                    }
                    else if (subModule.GetState() == SubModuleStateEnum.RECOVERY)
                    {
                        InnerStateTransition(new NeedleCleanModuleRecoveryState(this));
                        break;
                    }
                    else if (subModule.GetState() == SubModuleStateEnum.SUSPEND)
                    {
                        InnerStateTransition(new NeedleCleanModuleSuspendState(this));
                        break;
                    }

                    if (subModule.Equals(modules.LastOrDefault()) && subModule.GetState() == SubModuleStateEnum.DONE)
                    {
                        doneFlag = true;
                        break;
                    }
                }
                if (doneFlag)
                {
                    InnerStateTransition(new NeedleCleanModuleDoneState(this));
                    this.LotOPModule().MapScreenToLotScreen();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanModule - DoNeedleCleaningProcess() : Error occured.");
                System.Diagnostics.Debug.Assert(true);
            }



            return retVal;
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
                throw;
            }
            return retVal;
        }

        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum Abort()
        {
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
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
                LoggerManager.Exception(err);
                throw;
            }
            return stat;
        }

        public EventCodeEnum InnerStateTransition(IInnerState state) // Don`t Touch
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
                throw;
            }
            return retVal;
        }

        public void StateTransition(ModuleStateBase state) // Don`t Touch
        {
            ModuleState = state;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                NeedleCleanStateEnum state = (InnerState as NeedleCleanState).GetState();

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
                List<ISubModule> modules = Template.GetProcessingModule();
                foreach (var subModule in modules)
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

        private EventCodeEnum CalcRangeLimit(int ncNum, out double Range_L, out double Range_R, out double Range_T, out double Range_B)
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                MachineCoordinate mccoord = new MachineCoordinate();
                NCCoordinate nccoord = new NCCoordinate();
                double probecard_cen_x = 0;
                double probecard_cen_y = 0;
                double sizeX = 0; double sizeY = 0;

                try
                {
                    sizeX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX * this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.Value;
                    sizeY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY * this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.Value;

                    // SW LIMIT 을 고려하여 Range를 바탕으로 총 사용할 수 있는 영역의 크기를 구한다.
                    // TODO: 프로브 카드의 중심이 치우친 만큼 고려해 줄것!!
                    probecard_cen_x = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                    probecard_cen_y = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;


                    // 클리닝 가능한 제일 왼쪽 모서리 위치
                    nccoord.X.Value = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + (sizeX / 2);
                    mccoord = this.StageSupervisor().CoordinateManager().WaferHighNCPadConvert.ConvertBack(nccoord);

                    if (mccoord.X.Value + probecard_cen_x > this.MotionManager().GetAxis(EnumAxisConstants.X).Param.PosSWLimit.Value)
                    {
                        // Range를 벗어남
                        Range_L = (mccoord.X.Value + probecard_cen_x) - this.MotionManager().GetAxis(EnumAxisConstants.X).Param.PosSWLimit.Value;
                    }
                    else
                    {
                        Range_L = 0;
                    }
                    // 클리닝 가능한 제일 오른쪽 모서리 위치
                    nccoord.X.Value = NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value - (sizeX / 2);
                    mccoord = this.StageSupervisor().CoordinateManager().WaferHighNCPadConvert.ConvertBack(nccoord);

                    if (mccoord.X.Value + probecard_cen_x < this.MotionManager().GetAxis(EnumAxisConstants.X).Param.NegSWLimit.Value)
                    {
                        // Range를 벗어남
                        Range_R = (mccoord.X.Value + probecard_cen_x) - this.MotionManager().GetAxis(EnumAxisConstants.X).Param.NegSWLimit.Value;
                    }
                    else
                    {
                        Range_R = 0;
                    }
                    // 클리닝 가능한 제일 위쪽 모서리 위치
                    nccoord.Y.Value = NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value - (sizeY / 2);
                    mccoord = this.StageSupervisor().CoordinateManager().WaferHighNCPadConvert.ConvertBack(nccoord);

                    if (mccoord.Y.Value + probecard_cen_y < this.MotionManager().GetAxis(EnumAxisConstants.Y).Param.NegSWLimit.Value)
                    {
                        // Range를 벗어남
                        Range_T = (mccoord.Y.Value + probecard_cen_x) - this.MotionManager().GetAxis(EnumAxisConstants.Y).Param.NegSWLimit.Value;
                    }
                    else
                    {
                        Range_T = 0;
                    }
                    // 클리닝 가능한 제일 아래쪽 모서리 위치
                    nccoord.Y.Value = -NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value + NC.NCSysParam.SheetDefs[ncNum].Margin.Value + (sizeY / 2);
                    mccoord = this.StageSupervisor().CoordinateManager().WaferHighNCPadConvert.ConvertBack(nccoord);

                    if (mccoord.Y.Value + probecard_cen_y > this.MotionManager().GetAxis(EnumAxisConstants.Y).Param.PosSWLimit.Value)
                    {
                        // Range를 벗어남
                        Range_B = (mccoord.Y.Value + probecard_cen_x) - this.MotionManager().GetAxis(EnumAxisConstants.Y).Param.PosSWLimit.Value;
                    }
                    else
                    {
                        Range_B = 0;
                    }
                }

                catch (Exception err)
                {
                    System.Diagnostics.Debug.Assert(true);
                    LoggerManager.Debug($"{err.ToString()}. NeedleCleanModule - CalcRangeLimit() : Error occured.");
                    RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

                    Range_L = 0;
                    Range_R = 0;
                    Range_T = 0;
                    Range_B = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            //throw new NotImplementedException();
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            bool bEnabled = false;
            try
            {
                int NumX = 0;
                int NumY = 0;
                double Range_L = 0;
                double Range_R = 0;
                double Range_T = 0;
                double Range_B = 0;

                double sizeX = 0; double sizeY = 0;
                double marginX = 0.0;
                double marginY = 0.0;
                double minX = 0; double maxX = 0;
                double minY = 0; double maxY = 0;
                double curX = 0; double curY = 0;

                var NCHeightProfileParam = this.NeedleCleaner().NCHeightProfilingModule.NeedleCleanHeightProfilingParameter as NeedleCleanHeightProfilingParameter;

                if (NCHeightProfileParam != null)
                {
                    var limit = NCHeightProfileParam.MaxLimit.Value;

                    foreach (var table in NCHeightProfileParam.ErrorTable)
                    {
                        for (int m = 0; m < table.Positions.Count - 1; m++)
                        {
                            var item1 = table.Positions[m];

                            for (int n = (m + 1); n < table.Positions.Count; n++)
                            {
                                var item2 = table.Positions[n];

                                if (Math.Abs((item1.Z.Value - item2.Z.Value)) > Math.Abs(limit))
                                {
                                    msg = $"Invalid Parameter for NC. Please check the height profile's parameters.\nIt could not be over {limit} micron, but current planarity offset is {Math.Abs((item1.Z.Value - item2.Z.Value))}.";
                                    return false;
                                }
                            }
                        }
                    }
                }

                if (NC.NCSysParam.CleanUnitAttached.Value == true)
                {
                    for (int ncNum = 0; ncNum <= NC.NCSysParam.MaxCleanPadNum.Value - 1; ncNum++)
                    {
                        if (NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].Enabled.Value == true)
                        {
                            bEnabled = true;

                            if (NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].ContactLimit.Value != 0 && (NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].ContactCount.Value + NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].CleaningCount.Value >
                                NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].ContactLimit.Value))
                            {
                                msg = "Contact Count is over limit";
                                return false;
                            }

                            if (NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].CycleLimit.Value != 0 && (NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].CycleCount.Value > NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].CycleLimit.Value))
                            {
                                msg = "Cycle Count is over limit";
                                return false;
                            }

                            RetVal = CalcRangeLimit(ncNum, out Range_L, out Range_R, out Range_T, out Range_B);
                            sizeX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX * this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.Value;
                            sizeY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY * this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.Value;

                            if (NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].CleaningType.Value == NeedleCleanerModuleParameter.NC_CleaningType.SINGLEDIR)
                            {
                                if (NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].CleaningCount.Value <= 0)
                                {
                                    msg = "Contact Count is zero";
                                    return false;
                                }

                                marginX = ((((NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value) * 2.0) - Math.Abs(Range_L) - Math.Abs(Range_R)) - sizeX);
                                marginY = ((((NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value) * 2.0) - Math.Abs(Range_T) - Math.Abs(Range_B)) - sizeY);

                                if (marginX < 0)
                                {
                                    msg = "Probe Card Size is bigger than cleaning pad area (Width)";
                                    return false;
                                }

                                if (marginY < 0)
                                {
                                    msg = "Probe Card Size is bigger than cleaning pad area (Height)";
                                    return false;
                                }
                            }
                            else
                            {
                                if (NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].UserDefinedSeq.Count <= 0)
                                {
                                    msg = "Contact Sequence is not registered yet";
                                    return false;
                                }

                                curX = 0;
                                curY = 0;
                                for (int i = 0; i <= NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].UserDefinedSeq.Count - 1; i++)
                                {
                                    curX = curX + NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].UserDefinedSeq[i].Value.X.Value;
                                    curY = curY + NeedleCleanDeviceParameter_Clone.SheetDevs[ncNum].UserDefinedSeq[i].Value.Y.Value;

                                    if (curX > maxX) { maxX = curX; }
                                    if (curX < minX) { minX = curX; }
                                    if (curY > maxY) { maxY = curY; }
                                    if (curY < minY) { minY = curY; }
                                }

                                sizeX = sizeX + Math.Abs(minX) + Math.Abs(maxX);
                                sizeY = sizeY + Math.Abs(minY) + Math.Abs(maxY);

                                NumX = (int)Math.Truncate((((NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value) * 2.0) - Math.Abs(Range_L) - Math.Abs(Range_R)) / sizeX);
                                NumY = (int)Math.Truncate((((NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value - NC.NCSysParam.SheetDefs[ncNum].Margin.Value) * 2.0) - Math.Abs(Range_T) - Math.Abs(Range_B)) / sizeY);

                                if (NumX <= 0 || NumY <= 0)
                                {
                                    msg = "Cleaning sheet has not enough area for user defined sequence";
                                    return false;
                                }
                            }
                        }

                    }

                    if (bEnabled)
                    {
                        if (NC.NCSysParam.TouchSensorRegistered.Value != true || NC.NCSysParam.TouchSensorBaseRegistered.Value != true ||
                            NC.NCSysParam.TouchSensorPadBaseRegistered.Value != true || NC.NCSysParam.TouchSensorOffsetRegistered.Value != true)
                        {
                            msg = "Touch Sensor is not registered yet";
                            return false;
                        }

                        msg = "Ready To Cleaning";
                        return true;
                    }
                    else
                    {
                        msg = "Needle Cleaning fucntion is disabled";
                        return true;
                    }
                }
                else
                {
                    msg = "Cleaning Unit is not attached";
                    return true;
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanModule - IsLotReady() : Error occured.");
                msg = "Unexpected System Error";
                return false;
            }
        }
        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }


        #region //..SubRoutine
        public bool IsNCSensorON()
        {
            bool retVal = false;
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.IsNCSensorON();
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public bool IsCleanPadUP()
        {
            bool retVal = false;
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.IsCleanPadUP();
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public bool IsCleanPadDown()
        {
            bool retVal = false;
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.IsCleanPadDown();
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum WaitForCleanPadUp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.WaitForCleanPadUp();
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum WaitForCleanPadDown()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.WaitForCleanPadDown();
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum CleanPadUP(bool bWait)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.CleanPadUP(bWait);
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum CleanPadDown(bool bWait)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.CleanPadDown(bWait);
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public double GetMeasuredNcPadHeight(int ncNum, double posX, double posY)
        {
            double retVal = 20000;
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.GetMeasuredNcPadHeight(ncNum, posX, posY);
                }
                else
                {
                    retVal = 20000;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public NCCoordinate ReadNcCurPosForWaferCam(int ncNum)
        {
            NCCoordinate retVal = new NCCoordinate(0, 0, 0);
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.ReadNcCurPosForWaferCam(ncNum);
                }
                else
                {
                    //retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public NCCoordinate ReadNcCurPosForPin(int ncNum)
        {
            NCCoordinate retVal = new NCCoordinate(0, 0, 0);
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.ReadNcCurPosForPin(ncNum);
                }
                else
                {
                    //retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public NCCoordinate ReadNcCurPosForSensor(int ncNum)
        {
            NCCoordinate retVal = new NCCoordinate(0, 0, 0);
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.ReadNcCurPosForSensor(ncNum);
                }
                else
                {
                    //retVal = false;                
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;

        }

        public bool IsTimeToCleaning(int ncNum)
        {
            bool retVal = false;
            try
            {
                INeeldleCleanerSubRoutineStandard ncSubRoutineStandard = (SubRoutine as INeeldleCleanerSubRoutineStandard);
                if (ncSubRoutineStandard != null)
                {
                    retVal = ncSubRoutineStandard.IsTimeToCleaning(ncNum);
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


        #endregion

        #region INcIOFucntions

        /*
        public bool IsNCSensorON()
        {
            return this.IOManager().IO.Inputs.DINC_SENSOR.Value;
        }

        public bool IsCleanPadUP()
        {
            if (NeedleCleanSysParam.NC_TYPE.Value == NeedleCleanSystemParameter.NC_MachineType.MOTOR_NC)
            {
                // Motorized NC                    
                double tmpVal = 0;
                this.MotionManager().GetActualPos(EnumAxisConstants.PZ, out tmpVal);

                if (tmpVal <= NeedleCleanSysParam.CleanUnitDownPos.Value.Z.Value)
                {   // Down position
                    return false;
                }
                else
                {   // Up position
                    return true;
                }
            }
            else
            {
                // Cylinder Type NC
                return this.IOManager().IO.Inputs.DICLEANUNITUP_1.Value;
            }
        }

        public bool IsCleanPadDown()
        {
            if (NeedleCleanSysParam.NC_TYPE.Value == NeedleCleanSystemParameter.NC_MachineType.MOTOR_NC)
            {
                // Motorized NC                    
                double tmpVal = 0;
                this.MotionManager().GetActualPos(EnumAxisConstants.PZ, out tmpVal);

                if (tmpVal <= NeedleCleanSysParam.CleanUnitDownPos.Value.Z.Value)
                {   // Down position
                    return true;
                }
                else
                {   // Up position
                    return false;
                }
            }
            else
            {
                // Cylinder Type NC
                return this.IOManager().IO.Inputs.DICLEANUNITUP_0.Value;
            }
        }

        public EventCodeEnum WaitForCleanPadUp()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            try
            {
                this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DICLEANUNITUP_1, true, 30000);
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - WaitForCleanPadUp() : Error occured.");
                RetVal = EventCodeEnum.UNDEFINED;

            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum WaitForCleanPadDown()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            try
            {
                this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DICLEANUNITUP_0, true, 30000);
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - WaitForCleanPadDown() : Error occured.");
                RetVal = EventCodeEnum.UNDEFINED;

            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum CleanPadUP(bool bWait)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCLEANUNITUP, true);
                if (bWait == true)
                {
                    RetVal = WaitForCleanPadUp();
                }
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - CleanPadUP() : Error occured.");
                RetVal = EventCodeEnum.UNDEFINED;

            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum CleanPadDown(bool bWait)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCLEANUNITUP, false);
                if (bWait == true)
                {
                    RetVal = WaitForCleanPadDown();
                }
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - CleanPadDown() : Error occured.");
                RetVal = EventCodeEnum.UNDEFINED;

            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }
        */
        #endregion

        #region Parameter Load and Save Method
        public EventCodeEnum LoadDevParameter() // Parameter Type만 변경
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new NeedleCleanDeviceParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                //RetVal = Extensions_IParam.LoadParameter(ref ncdp, typeof(NeedleCleanDeviceParameter),null,null,Extensions_IParam.FileType.XML);
                RetVal = this.LoadParameter(ref tmpParam, typeof(NeedleCleanDeviceParameter));
                if (RetVal == EventCodeEnum.NONE)
                {
                    NeedleCleanDeviceParameter_IParam = tmpParam;
                    NeedleCleanDeviceParameter_Clone = NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                }

                LoadTemplate();
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

                try
                {
                    retVal = this.SaveParameter(NeedleCleanDeviceParameter_IParam);
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"{err.ToString()}. NeedleCleanModule - SaveDevParameter() : Error occured.");
                    throw err;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = EventCodeEnum.NONE;

                for (int i = 0; i <= NC.NCSysParam.MaxCleanPadNum.Value - 1; i++)
                {
                    NC.NCSysParam.SheetDefs[i].MarkedWaferCountVal = (long)this.LotOPModule().SystemInfo.WaferCount;
                    NC.NCSysParam.SheetDefs[i].MarkedDieCountVal = (long)this.LotOPModule().SystemInfo.DieCount;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> GetPnpSteps()
        {
            return TemplateToPnpConverter.Converter(Template.TemplateModules);
        }

        //public EventCodeEnum LoadCommonSysParameter()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        IParam tmpParam = null;
        //        tmpParam = new NeedleCleanSystemCommonParameter();
        //        tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
        //        RetVal = Extensions_IParam.LoadParameter(ref tmpParam, typeof(NeedleCleanSystemCommonParameter));

        //        if (RetVal == EventCodeEnum.NONE)
        //        {
        //            _NeedleCleanSysCommonParam = tmpParam as NeedleCleanSystemCommonParameter;
        //        }

        //        SysCommonParam = _NeedleCleanSysCommonParam;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"{err.ToString()}. NeedleCleanModule - LoadCommonSysParameter() : Error occured.");
        //        throw;
        //    }


        //    return RetVal;
        //}

        //public EventCodeEnum SaveCommonSysParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        if(SysCommonParam != null)
        //        {
        //            retVal = Extensions_IParam.SaveParameter(SysCommonParam);
        //        }
        //    }
        //    catch (Exception err)
        //    {

        //        throw;
        //    }            

        //    return retVal;
        //}

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IStageSupervisor stageSupervisor = this.StageSupervisor();
                retVal = stageSupervisor.LoadNCSysObject();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IStageSupervisor stageSupervisor = this.StageSupervisor();
                retVal = stageSupervisor.SaveNCSysObject();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private EventCodeEnum LoadNeedleCleanPadChangeBehaviors()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            string FullPath = this.FileManager().GetSystemParamFullPath("NeedleCleanModule", "NeedleCleanPadChangeSequence.json");

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(SequenceBehaviors), null, FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    NCPadChangeBehaviors = tmpParam as SequenceBehaviors;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        #endregion

        public ObservableCollection<ISequenceBehaviorGroupItem> GetNCPadChangeGroupCollection()
        {
            ObservableCollection<ISequenceBehaviorGroupItem> retVal = null;
            try
            {
                retVal = NCPadChangeBehaviors.ISequenceBehaviorCollection;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        #region //..Template
        public EventCodeEnum LoadTemplate()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                IParam tmpParam = null;
                tmpParam = new NeedleCleanTemplateFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(NeedleCleanTemplateFile));

                TemplateParameter = (NeedleCleanTemplateFile)tmpParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion
    }
}

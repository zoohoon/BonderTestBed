using ProberInterfaces.FDAlign;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FDWaferAlign
{
    using ProberInterfaces;
    using System.ComponentModel;
    using System;
    using System.Collections.ObjectModel;
    using ProberInterfaces.Command;
    using ProberErrorCode;
    using ProberInterfaces.State;
    using LogModule;
    using ProberInterfaces.Template;

    public class FDWaferAligner : IFDWaferAligner, INotifyPropertyChanged, IHasDevParameterizable, IHasSysParameterizable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region => Properties
        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.FDWaferAlign);
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
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
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
        private CommandTokenSet _RunTokenSet;
        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }
        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }
        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set
            {
                _ForcedDone = value;
                if (this.LoaderController() != null)
                {
                    this.LoaderController()?.SetForcedDoneState();
                }
            }
        }
        private FDWaferAlignState _FDWaferAlignState;
        public FDWaferAlignState FDWaferAlignState
        {
            get { return _FDWaferAlignState; }
        }
        public IInnerState InnerState
        {
            get { return _FDWaferAlignState; }
            set
            {
                if (value != _FDWaferAlignState)
                {
                    _FDWaferAlignState = value as FDWaferAlignState;
                    RaisePropertyChanged();
                }
            }
        }
        public IInnerState PreInnerState
        {
            get;
            set;
        }
        private ObservableCollection<TransitionInfo> _TransitionInfo;
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
        public bool Initialized { get; set; } = false;  // override 빠짐
        public bool IsParameterChanged(bool issave = false) // override 빠짐
        {
            return false;
        }
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
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum Abort()
        {
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }
        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {

                RetVal = _FDWaferAlignState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }
        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());

                //List<ISubModule> modules = Template.GetProcessingModule();

                //for (int index = 0; index < modules.Count; index++)
                //{
                //    retVal = modules[index].ClearData();
                //}

                //UpdateHeightProfiling(); //Align시작 전, HeightProfilingPointType에 따라서 AcceptFocusing 값 변경.

                LoggerManager.Debug("FDWaferAligner ClearState()");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public bool IsBusy()
        {
            bool retVal = false;
            try
            {
                //List<ISubModule> modules = Template.GetProcessingModule();
                //foreach (var subModule in modules)
                //{
                //    if (subModule.GetMovingState() == MovingStateEnum.MOVING)
                //    {
                //        retVal = true;
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public new void DeInitModule()
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
        public void StateTransition(ModuleStateBase state)
        {
            try
            {
                ModuleState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                FDWaferAlignInnerStateEnum state = (InnerState as FDWaferAlignState).GetState();

                retval = state.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public new EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    //EctFunction = new WaferAlignEctFunction(this);
                    //retval = EctFunction.InitModule();

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    InnerState = new FDWaferAlignIdleState(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    //EnableState = new EnableIdleState(EnableState);
                    //WaferAlignControItems = new WaferAlignControItems();
                    Initialized = true;

                    //this.EventManager().RegisterEvent(typeof(WaferUnloadedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    //this.EventManager().RegisterEvent(typeof(DeviceChangedEvent).FullName, "ProbeEventSubscibers", EventFired); //ISSD-4260 Event 확인 필요.

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
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            bool retVal = false;
            msg = null;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum ParamValidation()  // override 빠짐
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
        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                PreInnerState = _FDWaferAlignState;
                InnerState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public ModuleStateEnum GetModuleState()
        {
            return ModuleState.GetState();
        }
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //    IParam tmpParam = null;
                //    tmpParam = new FDWaferAlignTemplateFile();
                //    tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                //    RetVal = this.LoadParameter(ref tmpParam, typeof(FDWaferAlignTemplateFile));

                //    if (RetVal == EventCodeEnum.NONE)
                //    {
                //        TemplateParameter = tmpParam as FDWaferAlignTemplateFile;
                //    }

                //    RetVal = LoadTemplate();

                LoggerManager.Debug("[Bonder] BonderSupervisor. LoadDevParameter()");

                if (this.TemplateManager() != null)
                {
                    // 아래 에러로 주석처리, 나중에 FD Align 추가할 때 수정필요
                    // RetVal = this.TemplateManager().InitTemplate(this);
                }
                else
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                //tmpParam = new WaferAlignSysParameter();
                //tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                //RetVal = this.LoadParameter(ref tmpParam, typeof(WaferAlignSysParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    //WaferAlignSysParam = tmpParam as WaferAlignSysParameter;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.SaveParameter(TemplateParameter);
                foreach (var submodule in Template.GetProcessingModule())
                {
                    if (submodule is IHasDevParameterizable)
                        (submodule as IHasDevParameterizable).SaveDevParameter();
                }

                LoggerManager.Debug("[Bonder] BonderSupervisor. SaveDevParameter()");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                //RetVal = this.SaveParameter(FDWaferAlignSysParam);
                LoggerManager.Debug("[Bonder] BonderSupervisor. SaveSysParameter()");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            LoggerManager.Debug("[Bonder] BonderSupervisor. InitDevParameter()");

            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        #region => FDAlign 신규 함수
        public EventCodeEnum DoFDWaferAlignProcess()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug("[FD Align] FD Align Start.");

                if(true)
                {
                    LoggerManager.Debug("[FD Align] FDWafer Move to FD Wafer Loading Position");    //이 부분은 나중에 ArmToChuck() 쪽에서 실행

                    //retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(0, 0, Wafer.GetSubsInfo().ActualThickness);
                    //retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0);
                    LoggerManager.Debug("[FD Align] FDWafer Move to FD Low Camera");

                    if (false)
                    {
                        // 얼라인 Move 에러 체크 방법
                        InnerStateTransition(new FDWaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, "Move Error", FDWaferAlignState.GetType().Name)));

                        return retVal;
                    }

                    if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                    {
                        //retVal = this.MarkAligner().DoMarkAlign();
                        LoggerManager.Debug("[FD Align] Mark Align Start.");
                    }

                    bool doneFlag = false;

                    //List<ISubModule> modules = Template.GetProcessingModule();
                    LoggerManager.Debug("[FD Align] Get Processing Module.");

                    //for (int index = 0; index < modules.Count; index++)
                    //{
                    //    ISubModule module = modules[index];

                    //    retVal = module.Execute();
                    //}
                    // WaferAlign의 경우 실행하는 하위 모듈로는
                    // WAEdgeStadnardModule , WALowStandardModule , WAHighStandardModule , WABoundaryStandardModule , WAIndexAlignEdgeModule , WAPadStandardModule 이며
                    // FDAlign의 경우 얼라인 방법이 다른만큼 하위 모듈이 다를 것으로 생각됨.

                    LoggerManager.Debug("[FD Align] Processing Module Execute().");

                    doneFlag = true;

                    if(doneFlag)
                    {
                        retVal = EventCodeEnum.NONE;
                    }

                    if (retVal != EventCodeEnum.NONE)
                    {
                        InnerStateTransition(new FDWaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, "Error", FDWaferAlignState.GetType().Name)));
                    }
                    else
                    {
                        InnerStateTransition(new FDWaferAlignDoneState(this));
                    }
                }
                else
                {

                }
            }
            catch
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retVal;
        }
        
        #endregion
    }
}

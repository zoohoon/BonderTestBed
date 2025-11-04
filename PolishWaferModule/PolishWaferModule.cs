using ProberInterfaces.PolishWafer;
using System;
using System.Collections.Generic;
using System.Linq;
using ProberInterfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using ProberInterfaces.Command;
using ProberErrorCode;
using PolishWaferParameters;
using ProberInterfaces.State;
using ProberInterfaces.Wizard;
using System.Runtime.CompilerServices;
using LogModule;
using ProberInterfaces.Template;
using SerializerUtil;
using System.ServiceModel;
using ProberInterfaces.Command.Internal;
using System.Threading.Tasks;
using ProberInterfaces.Event;
using NotifyEventModule;
using TouchSensorSystemParameter;
using TouchSensorObject;
using ProberInterfaces.Param;
using PolishWaferFocusingBySensorModule;
using PolishWaferCleaningModule;
using PolishWaferFocusingModule;
using System.Threading;
using MetroDialogInterfaces;

namespace PolishWaferModule
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PolishWaferModule : IPolishWaferModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public bool Initialized { get; set; } = false;

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
            private set { _ModuleState = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.PolishWafer);
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
        private SubStepModules _SubModules;
        public ISubStepModules SubModules
        {
            get { return _SubModules; }
            set
            {
                if (value != _SubModules)
                {
                    _SubModules = (SubStepModules)value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsManualTriggered;
        public bool IsManualTriggered
        {
            get { return _IsManualTriggered; }
            set
            {
                if (value != _IsManualTriggered)
                {
                    _IsManualTriggered = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsManualOPFinished = false;
        public bool IsManualOPFinished
        {
            get { return _IsManualOPFinished; }
            set
            {
                if (value != _IsManualOPFinished)
                {
                    _IsManualOPFinished = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EventCodeEnum _ManualOPResult = EventCodeEnum.UNDEFINED;
        public EventCodeEnum ManualOPResult
        {
            get { return _ManualOPResult; }
            set
            {
                if (value != _ManualOPResult)
                {
                    _ManualOPResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LotStartFlag = false;
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

        private bool _LotEndFlag = false;
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

        private int _RestoredMarkedWaferCountLastPolishWaferCleaning;
        public int RestoredMarkedWaferCountLastPolishWaferCleaning
        {
            get { return _RestoredMarkedWaferCountLastPolishWaferCleaning; }
            set
            {
                if (value != _RestoredMarkedWaferCountLastPolishWaferCleaning)
                {
                    _RestoredMarkedWaferCountLastPolishWaferCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _RestoredMarkedTouchDownCountLastPolishWaferCleaning;
        public int RestoredMarkedTouchDownCountLastPolishWaferCleaning
        {
            get { return _RestoredMarkedTouchDownCountLastPolishWaferCleaning; }
            set
            {
                if (value != _RestoredMarkedTouchDownCountLastPolishWaferCleaning)
                {
                    _RestoredMarkedTouchDownCountLastPolishWaferCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }


        private PolishWafertCleaningInfo _ManualCleaningInfo;
        public PolishWafertCleaningInfo ManualCleaningInfo
        {
            get { return _ManualCleaningInfo; }
            set
            {
                if (value != _ManualCleaningInfo)
                {
                    _ManualCleaningInfo = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IPolishWaferCleaningParameter _ManualCleaningParam;
        public IPolishWaferCleaningParameter ManualCleaningParam
        {
            get { return _ManualCleaningParam; }
            set
            {
                if (value != _ManualCleaningParam)
                {
                    _ManualCleaningParam = value;
                    RaisePropertyChanged();
                }
            }
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

        private PolishWaferModuleState _PolishWaferModuleState;
        public PolishWaferModuleState PolishWaferModuleState
        {
            get { return _PolishWaferModuleState; }
        }

        public IInnerState InnerState
        {
            get { return _PolishWaferModuleState; }
            set
            {
                if (value != _PolishWaferModuleState)
                {
                    _PolishWaferModuleState = value as PolishWaferModuleState;
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


        private IPolishWaferControlItems _PolishWaferControlItems;
        public IPolishWaferControlItems PolishWaferControlItems
        {
            get { return _PolishWaferControlItems; }
            set { _PolishWaferControlItems = value; }
        }

        private IPolishWaferFocusing _FocusingModule;

        public IPolishWaferFocusing FocusingModule
        {
            get { return _FocusingModule; }
            set { _FocusingModule = value; }
        }

        private IPolishWaferFocusingBySensor _FocusingBySensorModule;

        public IPolishWaferFocusingBySensor FocusingBySensorModule
        {
            get { return _FocusingBySensorModule; }
            set { _FocusingBySensorModule = value; }
        }
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

        public IPolishWaferProcessor processor { get; set; }
        #endregion

        #region //..New Property
        //public IParam PolishWaferParameter { get; set; } = new PolishWaferParameter();
        //private PolishWaferParameter PolishWaferParam
        //{
        //    get { return PolishWaferParameter as PolishWaferParameter; }
        //}
        private IParam _PolishWaferParameter;

        public IParam PolishWaferParameter
        {
            get { return _PolishWaferParameter; }
            set { _PolishWaferParameter = value; }
        }
        #endregion

        private PolishWaferProcessingInfo _ProcessingInfo;
        public PolishWaferProcessingInfo ProcessingInfo
        {
            get { return _ProcessingInfo; }
            set { _ProcessingInfo = value; }
        }

        //private IPolishWaferIntervalParameter _CurIntervalParameter;
        //public IPolishWaferIntervalParameter CurIntervalParameter
        //{
        //    get { return _CurIntervalParameter; }
        //    set { _CurIntervalParameter = value; }
        //}

        #region IModule

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

        private bool _AngleUpdated = false;
        public bool NeedAngleUpdate
        {
            get { return _AngleUpdated; }
            set
            {
                if (value != _AngleUpdated)
                {
                    _AngleUpdated = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _RotateAngle = 0;
        public double RotateAngle
        {
            get { return _RotateAngle; }
            set
            {
                if (value != _RotateAngle)
                {
                    _RotateAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CurrentAngle = 0;
        public double CurrentAngle
        {
            get { return _CurrentAngle; }
            set
            {
                if (value != _CurrentAngle)
                {
                    _CurrentAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public void StateTransition(ModuleStateBase state)
        {
            ModuleState = state;
        }

        public IParam GetPolishWaferIParam()
        {
            return this.PolishWaferParameter;
        }

        public void SetPolishWaferIParam(byte[] param)
        {
            try
            {
                object target = null;

                var result = SerializeManager.DeserializeFromByte(param, out target, typeof(PolishWaferParameter));

                if (target != null)
                {
                    this.PolishWaferParameter = target as PolishWaferParameter;
                    this.SaveDevParameter();
                }
                else
                {
                    LoggerManager.Error($"SetPolishWaferParam function is faild.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool PWIntervalhasLotstart(int index = -1)
        {
            bool ret = false;
            try
            {
                var param = (PolishWaferParameter as PolishWaferParameter);
                if (param != null)
                {
                    if (param.PolishWaferIntervalParameters != null)
                    {
                        foreach (var iparm in param.PolishWaferIntervalParameters)
                        {
                            if (iparm.CleaningTriggerMode.Value == EnumCleaningTriggerMode.LOT_START)
                            {
                                ret = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //if ((PolishWaferParameter as PolishWaferParameter).NeedLoadWaferFlag == true)
                //{
                //    (PolishWaferParameter as PolishWaferParameter).NeedLoadWaferFlag = false;

                //    LoggerManager.Debug($"Polish wafer's load flag is reset.");
                //}

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
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return ModuleStateEnum.UNDEFINED;
            }
        }

        public void UpdateCurIntervalParameter()
        {
            try
            {
                // TODO : 갖고 있는 최신 파라미터로 값을 사용해야 된다.
                // 기존에 갖고 있던 CurIntervalParameter의 데이터 중 유지해야 되는 데이터를 제외,
                // 파라미터 셋에서 기억해놓은 index 정보를 활용하여 데이터를 가져오자.
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                return ModuleStateEnum.UNDEFINED;
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
                return ModuleStateEnum.UNDEFINED;
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
                return ModuleStateEnum.UNDEFINED;
            }
        }

        public ModuleStateEnum Execute() // Don`t Touch
        {
            try
            {
                ModuleStateEnum stat = ModuleStateEnum.ERROR;
                EventCodeEnum retVal = InnerState.Execute();
                ModuleState.StateTransition(InnerState.GetModuleState());
                RunTokenSet.Update();
                stat = InnerState.GetModuleState();

                return stat;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return ModuleStateEnum.UNDEFINED;
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    PolishWaferControlItems = new PolishWaferControlItems();

                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    _PolishWaferModuleState = new PolishWaferModuleIdleState(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    retval = this.EventManager().RegisterEvent(typeof(LotStartEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LotEndEvent).FullName, "ProbeEventSubscibers", EventFired);

                    RestoredMarkedWaferCountLastPolishWaferCleaning = Convert.ToInt32(this.LotOPModule().SystemInfo.MarkedWaferCountLastPolishWaferCleaning);
                    RestoredMarkedTouchDownCountLastPolishWaferCleaning = Convert.ToInt32(this.LotOPModule().SystemInfo.MarkedTouchDownCountLastPolishWaferCleaning);

                    ProcessingInfo = new PolishWaferProcessingInfo();

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

        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is LotStartEvent)
                {
                    // 초기화
                    ProcessingInfo.Init();

                    this.LotStartFlag = true;
                    this.LotEndFlag = false;

                    RestoredMarkedWaferCountLastPolishWaferCleaning = Convert.ToInt32(this.LotOPModule().SystemInfo.MarkedWaferCountLastPolishWaferCleaning);
                    RestoredMarkedTouchDownCountLastPolishWaferCleaning = Convert.ToInt32(this.LotOPModule().SystemInfo.MarkedTouchDownCountLastPolishWaferCleaning);

                    LoggerManager.Debug($"[PolishWaferModule] EventFired() : sender = LotStartEvent");
                }
                else if (sender is LotEndEvent)
                {
                    this.LotEndFlag = true;
                    this.LotStartFlag = false;
                    LoggerManager.Debug($"[PolishWaferModule] EventFired() : sender = LotEndEvent");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public EventCodeEnum PolishWaferLoad()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.NODATA;
        //    return retVal;
        //}

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NODATA;

            try
            {
                retVal = this.SaveParameter(PolishWaferParameter);

                if (retVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"[PolishWaferModule] SaveParamFunc(): Serialize Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
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

                ///TemplateModule
                tmpParam = new CleaningTemplateFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(CleaningTemplateFile));

                if (RetVal == EventCodeEnum.NONE)
                {
                    TemplateParameter = tmpParam as CleaningTemplateFile;
                }
                this.TemplateManager().InitTemplate(this);

                //Template.EntryModules.Where(x => x.)
                foreach (var module in this.Template.TemplateModules)
                {
                    bool check;
                    check = (module is IPolishWaferProcessor);

                    if (check == true)
                    {
                        this.processor = module as IPolishWaferProcessor;
                        break;
                    }
                }

                tmpParam = new PolishWaferParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(PolishWaferParameter));
                if (RetVal == EventCodeEnum.NONE)
                {
                    PolishWaferParameter = tmpParam as PolishWaferParameter;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
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

        public bool CanExecute(IProbeCommandToken token)
        {
            bool retval = false;

            try
            {
                retval = PolishWaferModuleState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                PolishWaferModuleStateEnum state = (InnerState as PolishWaferModuleState).GetState();

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

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            //touchsensor offset계산?
            msg = "";
            bool retval = false;

            try
            {
                if (CheckTouchSensorSetupState() == false)
                {
                    msg = "Touch Sensor Setup is not completed yet for Polish Wafer";
                    return retval;
                }
                else
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. PolishWaferModule - IsLotReady() : Error occured.");
                msg = "Unexpected System Error";
                return false;
            }

            return retval;
        }

        //메뉴얼 동작시 조건 체크
        public bool IsManualOPReady(out string msg)
        {
            bool retval = false;
            msg = string.Empty;

            try
            {
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void CompleteManualOperation(EventCodeEnum retval)
        {
            try
            {
                if (IsManualTriggered)
                {
                    this.ManualOPResult = retval;
                }

                this.ManualCleaningParam = null;
                this.ManualCleaningInfo = null;

                this.IsManualOPFinished = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

        public EventCodeEnum SetRemainingCleaningData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                bool IsRemain = false;

                PolishWaferParameter pwparam = PolishWaferParameter as PolishWaferParameter;

                foreach (var intervalparam in pwparam.PolishWaferIntervalParameters.Select((value, index) => new { Value = value, Index = index }))
                {
                    bool IsTriggered = ProcessingInfo.GetIntervalTrigger(intervalparam.Value.HashCode);

                    if (IsTriggered)
                    {
                        string cleaningHashCode = string.Empty;

                        IsRemain = ProcessingInfo.IsRemainingCleaningParam(intervalparam.Value.HashCode, ref cleaningHashCode);

                        if (IsRemain)
                        {
                            retval = EventCodeEnum.NONE;

                            ProcessingInfo.SetCurrentData(intervalparam.Value.HashCode, cleaningHashCode);

                            break;
                        }
                    }
                }

                if (IsRemain == false)
                {
                    retval = EventCodeEnum.POLISHWAFER_NOT_EXIST_CLEANING;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum CheckPolishWaferOnChuck()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                // Chuck 위에 폴리쉬 웨이퍼가 있고, 요청한 웨이퍼와 일치하는 웨이퍼 일때 NONE 반환.
                //PILISHWAFER_SUSPEND_ON_CHUCK => 아직 웨이퍼가 올라오지 않은 상태
                //INCORRECT_POLISHWAFER_ON_CHUCK => 요청한 웨이퍼와 일치하지 않는다.

                if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST &&
                    this.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.POLISH &&
                    this.StageSupervisor().WaferObject.GetState() == EnumWaferState.UNPROCESSED)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.POLISHWAFER_SUSPEND_ON_CHUCK;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DoManualOperation(IProbeCommandParameter param = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.IsManualTriggered = true;
                ManualOPResult = EventCodeEnum.UNDEFINED;
                IsManualOPFinished = false;

                string msg = null;
                
                bool isReady = IsManualOPReady(out msg);
                if(isReady == true)
                {
                    retval = ClearState();
                }

                if (isReady == false || retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], DoManualOperation(): ClearState = {retval}, msg = {msg}");
                }

                if (retval == EventCodeEnum.NONE && isReady)
                {
                    bool isInjected = false;

                    isInjected = this.CommandManager().SetCommand<IDoManualPolishWaferCleaning>(this, param);

                    if (isInjected == true)
                    {
                        while (true)
                        {
                            if (IsManualOPFinished == true)
                            {
                                retval = this.ManualOPResult;
                                break;
                            }

                            Thread.Sleep(30);
                        }
                    }
                    else
                    {
                        retval = EventCodeEnum.POLISHWAFER_MANUALOPERATION_COMMAND_REJECTED;

                        LoggerManager.Debug($"[{this.GetType().Name}], DoManualOperation(): retval = {retval}");
                    }
                }
                else
                {
                    if (isReady == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Manual Polish Wafer", $"Polish Wafer cannot be performed.\r\n The reason is {msg}", EnumMessageStyle.Affirmative);
                    }

                    retval = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.IsManualTriggered = false;
            }

            return retval;
        }

        public async Task<EventCodeEnum> DoManualPolishWaferCleaning(byte[] param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                PolishWaferCleaningParameter cleaningparam = null;
                var obj = await this.ByteArrayToObject(param);

                cleaningparam = (PolishWaferCleaningParameter)obj;

                if (cleaningparam != null)
                {
                    retval = DoManualOperation(cleaningparam);
                }
                else
                {
                    retval = EventCodeEnum.PARAM_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task<EventCodeEnum> ManualPolishWaferFocusing(byte[] param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                TouchSensorSysParameter TouchSensorParam = this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;

                FocusingModule = new PolishWaferFocusing_Standard();
                FocusingBySensorModule = new PolishWaferFocusingBySensor_Standard();

                PolishWaferCleaningParameter cleaningparam = null;
                var obj = await this.ByteArrayToObject(param);

                cleaningparam = (PolishWaferCleaningParameter)obj;

                if (cleaningparam != null)
                {
                    if (TouchSensorParam.TouchSensorAttached.Value == true)
                    {
                        retval = FocusingBySensorModule.DoFocusing(cleaningparam, true);
                    }
                    else
                    {
                        retval = FocusingModule.DoFocusing(cleaningparam, true);
                    }
                }
                else
                {
                    retval = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #region //..Modify Cleaning

        //private void ResetCurCleaningParam()
        //{
        //    try
        //    {
        //        PolishWaferCleaningParameter param = GetCurCleaningParam();

        //        param = null;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        public IPolishWaferCleaningParameter GetCurCleaningParam()
        {
            IPolishWaferCleaningParameter retval = null;

            try
            {
                PolishWaferParameter pwparam = PolishWaferParameter as PolishWaferParameter;

                var CurInterval = pwparam.PolishWaferIntervalParameters.FirstOrDefault(x => x.HashCode == ProcessingInfo.CurrentIntervalHashCode);

                if (CurInterval != null)
                {
                    retval = CurInterval.CleaningParameters.FirstOrDefault(x => x.HashCode == ProcessingInfo.CurrentCleaningHashCode);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void UpdateMarkedInfo(EnumCleaningTriggerMode triggermode)
        {
            try
            {
                // 메뉴얼 동작이 아닌 경우
                if (triggermode != EnumCleaningTriggerMode.UNDEFIEND)
                {
                    if (triggermode != EnumCleaningTriggerMode.LOT_START)
                    {
                        if (triggermode == EnumCleaningTriggerMode.WAFER_INTERVAL)
                        {
                            this.LotOPModule().SystemInfo.SetMarkedWaferCountLastPolishWaferCleaning();
                        }
                        else if (triggermode == EnumCleaningTriggerMode.TOUCHDOWN_COUNT)
                        {
                            this.LotOPModule().SystemInfo.SetMarkedTouchDownCountLastPolishWaferCleaning();
                        }

                        this.LotOPModule().SystemInfo.SaveLotInfo();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum CheckCurIntervalEnd()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                bool IsRemain = false;

                IsRemain = ProcessingInfo.IsRemainingCleaningParam();

                // 현재 인터벌의 모든 클리닝이 정상적으로 수행되었음.
                if (IsRemain == false)
                {
                    PolishWaferParameter pwparam = PolishWaferParameter as PolishWaferParameter;
                    var intervalparam = pwparam.PolishWaferIntervalParameters.FirstOrDefault(x => x.HashCode == ProcessingInfo.CurrentIntervalHashCode);

                    EnumCleaningTriggerMode triggerMode = intervalparam.CleaningTriggerMode.Value;

                    if (intervalparam != null)
                    {
                        UpdateMarkedInfo(triggerMode);
                    }
                    else
                    {
                        // UNKNOWN
                    }

                    var currentintervalinfo = ProcessingInfo.GetCurrentIntervalInfo();

                    ProcessingInfo.SetIntervalTrigger(ProcessingInfo.CurrentIntervalHashCode, false);

                    int ProcessedWaferCount = Convert.ToInt32(this.LotOPModule().SystemInfo.ProcessedWaferCountUntilBeforeCardChange);
                    int TouchDownCount = Convert.ToInt32(this.LotOPModule().SystemInfo.TouchDownCountUntilBeforeCardChange);

                    if (triggerMode == EnumCleaningTriggerMode.WAFER_INTERVAL)
                    {
                        int NumberOfWafersProcessedSinceLastCleaning = ProcessedWaferCount - this.PolishWaferModule().RestoredMarkedWaferCountLastPolishWaferCleaning;

                        currentintervalinfo.TriggeredInterval = NumberOfWafersProcessedSinceLastCleaning;
                    }
                    else if (triggerMode == EnumCleaningTriggerMode.TOUCHDOWN_COUNT)
                    {
                        int NumberOfTouchDownSinceLastCleaning = TouchDownCount - this.PolishWaferModule().RestoredMarkedTouchDownCountLastPolishWaferCleaning;

                        currentintervalinfo.TriggeredTouchDown = NumberOfTouchDownSinceLastCleaning;
                    }

                    retval = EventCodeEnum.POLISHWAFER_NO_CLEANING_DATA_REMAIN;

                    // 클리닝 플래그 초기화
                    foreach (var info in currentintervalinfo.CleaningInfos)
                    {
                        info.PolishWaferCleaningProcessed = false;
                        info.PolishWaferCleaningRetry = false;
                    }
                }
                else
                {
                    retval = EventCodeEnum.POLISHWAFER_CLEANING_DATA_REMAIN;
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool CheckAllIntervalEnd()
        {
            bool retval = false;

            try
            {
                //PolishWaferParameter pwparam = PolishWaferParameter as PolishWaferParameter;

                if (ProcessingInfo != null)
                {
                    if (ProcessingInfo.IntervalInfos != null)
                    {
                        bool AllTriggeredOff = ProcessingInfo.IntervalInfos.All(x => x.Triggered == false);

                        if (AllTriggeredOff == true)
                        {
                            retval = true;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[{this.GetType().Name}], CheckAllIntervalEnd(), CurrentProcessingParam.IntervalInfos is null.");
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], CheckAllIntervalEnd(), CurrentProcessingParam is null.");

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DoCleaningProcessing()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            List<EventCodeEnum> conditions = new List<EventCodeEnum>();
            conditions.Clear();

            try
            {
                List<ISubModule> modules = Template.GetProcessingModule();

                // 해당 루프의 동작은 현재 트리거 된 Cleaing Parameter를 기준으로 동작 됨.
                // 따라서, 트리거 된 인터벌 파라미터에 Cleaning Parameter의 개수가 여러개일 때, 반복해서 동작 된다. 

                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    if (IsManualTriggered == false)
                    {
                        var curcleaningparam = GetCurCleaningParam();

                        if (curcleaningparam != null)
                        {
                            var curintervalparam = GetCurrentIntervalParam();
                            var curcleaninginfo = ProcessingInfo.GetCurrentCleaningInfo();

                            // sub module 에서 Pin align 돌리고 다시 DoCleaningProcessing() 가 불리게 되는데 해당 부분에서 CLEANING START 가 중복으로 찍히게 됨.
                            if (curcleaninginfo?.PolishWaferCleaningProcessed == false)
                            {
                                LoggerManager.ActionLog(ModuleLogType.CLEANING, StateLogType.START,
                                                            $"Source Name : {curcleaningparam.WaferDefineType.Value}, " +
                                                            $"Trigger Type: {curintervalparam.CleaningTriggerMode.Value}, " +
                                                            $"Interval: {curintervalparam.IntervalCount.Value}",
                                                            this.LoaderController().GetChuckIndex());
                            }
                            else
                            {
                                LoggerManager.Error($"[{this.GetType().Name}], DoCleaningProcessing(), curintervalparam is null.");
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[PolishWaferModule], DoCleaningProcessing(), curintervalparam is null.");

                            LoggerManager.ActionLog(ModuleLogType.CLEANING, StateLogType.START, "Cleaning parameter is wrong.", this.LoaderController().GetChuckIndex());
                        }
                    }
                    else
                    {
                        // Unknown
                        LoggerManager.Error($"[{this.GetType().Name}], DoCleaningProcessing(), Unknown case 1");
                    }
                }
                else
                {
                    if (IsManualTriggered)
                    {
                        LoggerManager.ActionLog(ModuleLogType.CLEANING, StateLogType.START, $"Source Name : {this.ManualCleaningParam.WaferDefineType.Value}", this.LoaderController().GetChuckIndex());
                    }
                    else
                    {
                        // Unknown
                        LoggerManager.Error($"[{this.GetType().Name}], DoCleaningProcessing(), Unknown case 2");
                    }
                }

                foreach (var module in modules)
                {
                    retVal = module.Execute();

                    SubModuleStateEnum submodulestate = module.GetState();

                    if (submodulestate == SubModuleStateEnum.ERROR)
                    {
                        //InnerStateTransition(new PolishWaferModuleErrorState(this));

                        if (ForcedDone != EnumModuleForcedState.ForcedRunningAndDone)
                        {
                            InnerStateTransition(new PolishWaferModuleErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, "ERROR", this.PolishWaferModuleState.GetType().Name)));
                        }
                        return EventCodeEnum.NONE;
                    }
                    else if (submodulestate == SubModuleStateEnum.SUSPEND)
                    {
                        InnerStateTransition(new PolishWaferModuleSuspendState(this));

                        return EventCodeEnum.NONE;
                    }
                }

                if (IsManualTriggered == false)
                {
                    // 하나의 Cleaning이 완료될 때마다, 현재 인터벌의 트리거를 끌지 확인.
                    // 트리거가 꺼진 경우, 해당 폴리쉬 웨이퍼의 언로드 요청 가능 상태.
                    retVal = CheckCurIntervalEnd();

                    if (retVal == EventCodeEnum.POLISHWAFER_NO_CLEANING_DATA_REMAIN)
                    {
                        if (this.StageSupervisor().WaferObject.GetState() != EnumWaferState.READY)
                        {
                            // Unload가 가능하도록 State를 READY로 변경.
                            this.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.READY);
                        }

                        if (this.CheckAllIntervalEnd() == true)
                        {
                            // 동작에 사용 된 정보 초기화
                            ProcessingInfo.Init();

                            // 트리거 된 데이터가 남아 있지 않음.
                            InnerStateTransition(new PolishWaferModuleDoneState(this));

                            LoggerManager.Debug($"Cleaning END");
                        }
                        else
                        {
                            // 트리거 된 데이터가 남아 있음. (= 연속으로 Polish Wafer를 요청해야 되는 경우)
                            InnerStateTransition(new PolishWaferModuleRequestingState(this));
                        }
                    }
                    else
                    {
                        // 트리거 된 데이터가 남아 있음.
                        InnerStateTransition(new PolishWaferModuleRequestingState(this));
                    }
                }
                else
                {
                    UpdateMarkedInfo(EnumCleaningTriggerMode.UNDEFIEND);

                    InnerStateTransition(new PolishWaferModuleDoneState(this));
                    LoggerManager.Debug($"Cleaning END");
                }
            }
            catch (Exception err)
            {
                InnerStateTransition(new PolishWaferModuleErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, EventCodeEnum.EXCEPTION, "Can not Execute cleaning", this.PolishWaferModuleState.GetType().Name)));

                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool CheckWaferSource(ref string errorReasonStr)
        {
            bool retval = false;

            try
            {
                if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST &&
                    this.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.POLISH)
                {
                    if (this.StageSupervisor().WaferObject.GetPolishInfo().DefineName.Value == GetCurCleaningParam().WaferDefineType.Value)
                    {
                        retval = true;
                    }
                    else
                    {
                        errorReasonStr += $"Pw type Invailed.\n" +
                            $"Type of wafer currently loaded : {this.StageSupervisor().WaferObject.GetPolishInfo().DefineName.Value}\n" +
                            $"Cur Cleaning Wafer Define Type : {GetCurCleaningParam().WaferDefineType.Value}";
                        retval = false;
                    }
                }
                else
                {
                    retval = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion

        #region //..Processor

        public EventCodeEnum DoCentering(IPolishWaferCleaningParameter param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (processor != null)
                {
                    retval = processor.DoCentering(param);
                }
                else
                {
                    LoggerManager.Error("ProcessorModule is not loaded.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (processor != null)
                {
                    retval = processor.DoFocusing(param);
                }
                else
                {
                    LoggerManager.Error("ProcessorModule is not loaded.");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DoCleaning(IPolishWaferCleaningParameter param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (processor != null)
                {
                    processor.DoCleaning(param);
                }
                else
                {
                    LoggerManager.Error("ProcessorModule is not loaded.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        #endregion

        #region //..RealyPolishWafer
        public bool IsExecute()
        {
            bool retval = false;

            try
            {
                retval = Template.SchedulingModule.IsExecute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion
        public bool IsReadyPolishWafer()
        {
            return IsExecute();
        }

        public EventCodeEnum PolishWaferValidate(bool isExist)
        {
            return _PolishWaferModuleState.PolishWaferValidate(isExist);
        }

        public void SetDevParam(byte[] param)
        {
            try
            {
                string fullPath = this.FileManager().GetDeviceParamFullPath(PolishWaferParameter.FilePath, PolishWaferParameter.FileName);
                using (Stream stream = new MemoryStream(param))
                {
                    this.DecompressFilesFromByteArray(stream, fullPath);
                }
                LoadDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public byte[] GetPolishWaferParam()
        {

            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(PolishWaferParameter, typeof(PolishWaferParameter));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetPolishWaferParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;

        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public void InitTriggeredData()
        {
            try
            {
                foreach (var item in ProcessingInfo.IntervalInfos)
                {
                    item.TriggeredInterval = 0;
                    item.TriggeredTouchDown = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 모듈의 상태가 요청중인 경우, True를 반환
        /// </summary>
        /// <returns></returns>
        public bool IsRequested(ref string RequestedWaferName)
        {
            bool retval = false;

            try
            {
                if (this.PolishWaferModuleState.GetState() == PolishWaferModuleStateEnum.WAITLOADWAFER)
                {
                    retval = true;
                    RequestedWaferName = GetCurCleaningParam().WaferDefineType.Value;
                }
                else
                {
                    retval = false;
                    RequestedWaferName = string.Empty;
                }
                //LoggerManager.Debug($"[PolishWaferModule] IsRequested(): {retval}, RequestedWaferName:{RequestedWaferName}, PolishWaferModuleState: {this.PolishWaferModuleState.GetState()}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 모듈의 스케쥴러의 IsExecute값을 반환하는 함수.
        /// </summary>
        /// <returns></returns>
        public bool IsExecuteOfScheduler()
        {
            bool retval = false;

            try
            {
                if (ForcedDone == EnumModuleForcedState.Normal || ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                {
                    retval = (this.Template.SchedulingModule as ICleaningScheduleModule).RequiredRunningStateTransition();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public List<IPolishWaferIntervalParameter> GetPolishWaferIntervalParameters()
        {
            List<IPolishWaferIntervalParameter> parameters = new List<IPolishWaferIntervalParameter>();
            try
            {
                var param = (PolishWaferParameter as PolishWaferParameter);
                if (param != null)
                {
                    if (param.PolishWaferIntervalParameters != null)
                    {
                        foreach (var iparm in param.PolishWaferIntervalParameters)
                        {
                            parameters.Add(iparm);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        public IPolishWaferIntervalParameter GetCurrentIntervalParam()
        {
            IPolishWaferIntervalParameter retval = null;

            try
            {
                if (ProcessingInfo != null)
                {
                    var currentintervalinfo = ProcessingInfo.GetCurrentIntervalInfo();

                    if (currentintervalinfo != null)
                    {
                        string targethashcode = currentintervalinfo.HashCode;

                        PolishWaferParameter pwparam = PolishWaferParameter as PolishWaferParameter;

                        retval = pwparam.PolishWaferIntervalParameters.FirstOrDefault(x => x.HashCode == targethashcode);

                        if (retval == null)
                        {
                            LoggerManager.Error($"[{this.GetType().Name}], GetCurrentIntervalParam(), PolishWaferIntervalParameter not found.");
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], GetCurrentIntervalParam(), CurrentProcessingParam is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public IPolishWaferCleaningParameter GetCurrentCleaningParam()
        {
            IPolishWaferCleaningParameter retval = null;

            try
            {
                if (ProcessingInfo != null)
                {
                    string IntervalHC = ProcessingInfo.CurrentIntervalHashCode;
                    string cleaningHC = ProcessingInfo.CurrentCleaningHashCode;

                    PolishWaferParameter pwparam = PolishWaferParameter as PolishWaferParameter;

                    var intervalparam = pwparam.PolishWaferIntervalParameters.FirstOrDefault(x => x.HashCode == IntervalHC);

                    if (intervalparam != null)
                    {
                        retval = intervalparam.CleaningParameters.FirstOrDefault(x => x.HashCode == cleaningHC);
                    }

                    if (retval == null)
                    {
                        LoggerManager.Error($"[{this.GetType().Name}], GetCurrentCleaningParam(), PolishWaferCleaningParameter not found.");
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], GetCurrentCleaningParam(), CurrentProcessingParam is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool CheckTouchSensorSetupState()
        {
            bool retval = true;

            try
            {
                //TouchSensor parameter check.
                TouchSensorSysParameter TouchSensorSysParameter = this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;
                if (TouchSensorSysParameter.TouchSensorAttached.Value == true)
                {
                    if (TouchSensorSysParameter.TouchSensorRegistered.Value != true || TouchSensorSysParameter.TouchSensorBaseRegistered.Value != true ||
                        TouchSensorSysParameter.TouchSensorPadBaseRegistered.Value != true || TouchSensorSysParameter.TouchSensorOffsetRegistered.Value != true)
                    {
                        retval = false;
                    }
                    else
                    {
                        retval = true;
                    }
                }
                else
                {
                    //polish wafer module using cam
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"CheckTouchSensorSetupState() : {err.Message}");
                LoggerManager.Exception(err);
                retval = false;
            }

            return retval;
        }
    }
}

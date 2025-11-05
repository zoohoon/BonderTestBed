namespace EnvModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Enum;
    using ProberInterfaces.EnvControl;
    using ProberInterfaces.State;
    using ProberInterfaces.Temperature.Chiller;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class EnvModule : IEnvModule, INotifyPropertyChanged
    {
        #region <remarks> PropertyChanged </remarks> 
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> Property </remarks> 

        public bool Initialized { get; set; } = false;

        /// <remarks> IStateModule Property
        /// 
        //private CommandInformation _CommandInfo;
        //public CommandInformation CommandInfo
        //{
        //    get { return _CommandInfo; }
        //    set { _CommandInfo = value; }
        //}

        //private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.ENV);
        //public ReasonOfError ReasonOfError
        //{
        //    get { return _ReasonOfError; }
        //    set
        //    {
        //        if (value != _ReasonOfError)
        //        {
        //            _ReasonOfError = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        //public EnumModuleForcedState ForcedDone
        //{
        //    get { return _ForcedDone; }
        //    set { _ForcedDone = value; }
        //}

        #region <remarks> Command Slot Properties </remarks> 
        //private CommandSlot _CommandSendSlot = new CommandSlot();
        //public CommandSlot CommandSendSlot
        //{
        //    get { return _CommandSendSlot; }
        //    set { _CommandSendSlot = value; }
        //}

        //private CommandSlot _CommandRecvSlot = new CommandSlot();
        //public CommandSlot CommandRecvSlot
        //{
        //    get { return _CommandRecvSlot; }
        //    set { _CommandRecvSlot = value; }
        //}

        //private CommandSlot _CommandProcSlot = new CommandSlot();
        //public CommandSlot CommandRecvProcSlot
        //{
        //    get { return _CommandProcSlot; }
        //    set { _CommandProcSlot = value; }
        //}

        //private CommandSlot _CommandRecvDoneSlot = new CommandSlot();
        //public CommandSlot CommandRecvDoneSlot
        //{
        //    get { return _CommandRecvDoneSlot; }
        //    set { _CommandRecvDoneSlot = value; }
        //}

        //private CommandTokenSet _RunTokenSet;

        //public CommandTokenSet RunTokenSet
        //{
        //    get { return _RunTokenSet; }
        //    set { _RunTokenSet = value; }
        //}
        #endregion

        #region ==> State
        //private ModuleStateBase _ModuleState;
        //public ModuleStateBase ModuleState
        //{
        //    get { return _ModuleState; }
        //    set
        //    {
        //        if (value != _ModuleState)
        //        {
        //            _ModuleState = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //public IInnerState PreInnerState { get; set; }

        //public IInnerState InnerState
        //{
        //    get { return _EnvModuleState; }
        //    set
        //    {
        //        if (value != _EnvModuleState)
        //        {
        //            _EnvModuleState = value as EnvModuleState;
        //        }
        //    }
        //}

        //private EnvModuleState _EnvModuleState;
        //public EnvModuleState EnvModuleState
        //{
        //    get { return _EnvModuleState; }
        //}

        #endregion

        //private ObservableCollection<TransitionInfo> _TransitionInfo;
        //public ObservableCollection<TransitionInfo> TransitionInfo
        //{
        //    get { return _TransitionInfo; }
        //    set
        //    {
        //        if (value != _TransitionInfo)
        //        {
        //            _TransitionInfo = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private EnvModuleSystemParameter _EnvModuleSysParam;

        public EnvModuleSystemParameter EnvModuleSysParam
        {
            get { return _EnvModuleSysParam; }
            set { _EnvModuleSysParam = value; }
        }

        private List<IEnvConditionChecker> _EnvConditionCheckerList;
        public List<IEnvConditionChecker> EnvConditionCheckerList
        {
            get { return _EnvConditionCheckerList; }
            set
            {
                if (value != _EnvConditionCheckerList)
                {
                    _EnvConditionCheckerList = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// </remarks>

        #endregion

        #region <remarks> IModule Method </remarks> 
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    //CommandRecvSlot = new CommandSlot();
                    //RunTokenSet = new CommandTokenSet();
                    //_TransitionInfo = new ObservableCollection<TransitionInfo>();
                    //InnerState = new EnvIdleState(this);
                    //ModuleState = new ModuleUndefinedState(this);
                    //ModuleState.StateTransition(InnerState.GetModuleState());

                    Initialized = true;

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                    retVal = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
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

        #endregion

        #region <remarks> ILotReadyAble Method </remarks> 

        public bool IsLotReady(out string msg)
        {
            bool Sequenceexist = true;
            msg = "";
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Sequenceexist;
        }
        #endregion

        #region <remarks> IParamValidation Method </remarks> 
        public EventCodeEnum ParamValidation()
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

        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;

            return retVal;
        }
        #endregion

        #region <remarks> IStateModule Method </remarks> 
        //public bool CanExecute(IProbeCommandToken token)
        //{
        //    bool RetVal = false;
        //    try
        //    {
        //        //RetVal = ProbingModuleState.CanExecute(token);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return RetVal;
        //}
        //public void StateTransition(ModuleStateBase state)
        //{
        //    ModuleState = state;
        //}

        //public EventCodeEnum InnerStateTransition(IInnerState state)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.NONE;

        //    try
        //    {
        //        if (state != null)
        //        {
        //            EnvModuleState envModuleState = state as EnvModuleState;

        //            if (EnvModuleState.GetState() != envModuleState.GetState())
        //            {
        //                PreInnerState = InnerState;
        //                InnerState = state;

        //                //if (state.GetModuleState() == ModuleStateEnum.SUSPENDED || state.GetModuleState() == ModuleStateEnum.ERROR)
        //                //{
        //                //    LoggerManager.Debug($"[{GetType().Name}].EnvStateTransition() : Pre state = {(PreInnerState as EnvModuleState).GetState()}({PreInnerState.GetModuleState()}), Now State = {(InnerState as EnvModuleState).GetState()}({InnerState.GetModuleState()})");
        //                //}
        //                retVal = EventCodeEnum.NONE;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw err;
        //    }

        //    return retVal;
        //}

        //public ModuleStateEnum Execute() // Don`t Touch
        //{
        //    ModuleStateEnum stat = ModuleStateEnum.ERROR;

        //    try
        //    {
        //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //        retVal = InnerState.Execute();

        //        ModuleState.StateTransition(InnerState.GetModuleState());
        //        RunTokenSet.Update();
        //        stat = InnerState.GetModuleState();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return stat;
        //}

        //public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        //{
        //    InnerState.Pause();
        //    ModuleState.StateTransition(InnerState.GetModuleState());
        //    return InnerState.GetModuleState();
        //}

        //public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        //{
        //    InnerState.Resume();
        //    ModuleState.StateTransition(InnerState.GetModuleState());
        //    return InnerState.GetModuleState();
        //}
        //public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        //{
        //    InnerState.End();
        //    ModuleState.StateTransition(InnerState.GetModuleState());
        //    return InnerState.GetModuleState();
        //}
        //public ModuleStateEnum Abort()
        //{
        //    InnerState.Abort();
        //    ModuleState.StateTransition(InnerState.GetModuleState());
        //    return InnerState.GetModuleState();
        //}

        //public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        retVal = InnerState.ClearState();
        //        ModuleState.StateTransition(InnerState.GetModuleState());
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return retVal;
        //}

        //public bool IsBusy()
        //{
        //    bool retVal = true;
        //    try
        //    {

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return retVal;
        //}

        //public string GetModuleMessage()
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        EnumEnvModuleState state = (InnerState as EnvModuleState).GetState();
        //        retval = state.ToString();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}
        #endregion

        #region <remarks> IHasSysParameterizable Method </remarks> 
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //IParam tmpParam = null;
                //tmpParam = new EnvModuleSystemParameter();
                //tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                //retVal = this.LoadParameter(ref tmpParam, typeof(EnvModuleSystemParameter));

                //if(retVal == EventCodeEnum.NONE)
                //{
                //    EnvModuleSysParam = (EnvModuleSystemParameter)tmpParam;
                //    EnvConditionCheckerList = EnvModuleSysParam?.EnvConditionCheckerList;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //retVal = this.SaveParameter(EnvModuleSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region <remarks> EnvModule Method </remarks> 

        private bool Temp_Notify { get; set; }
        private bool Chiller_Notify { get; set; }
        public EventCodeEnum IsConditionSatisfied()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            EventCodeEnum retVal_TemperatureChecker = EventCodeEnum.UNDEFINED;
            EventCodeEnum retVal_ChillerChecker = EventCodeEnum.UNDEFINED;
            bool isValid = true;
            try
            {
                // TemperatureChecker

                if (this.TempController().ModuleState.GetState() != ModuleStateEnum.ERROR)
                {
                    if (this.TempController().TempInfo.TargetTemp.Value == this.TempController().TempInfo.SetTemp.Value)
                    {
                        if (this.TempController().ModuleState.GetState() != ModuleStateEnum.SUSPENDED)
                        {
                            if (this.TempController().IsCurTempWithinSetTempRange() == false)
                            {
                                retVal_TemperatureChecker = EventCodeEnum.ENV_TEMPARATURE_OUT_OF_RANGE;
                            }
                            else
                            {
                                retVal_TemperatureChecker = EventCodeEnum.NONE;
                            }
                        }
                        else
                        {
                            retVal_TemperatureChecker = EventCodeEnum.ENV_TEMPERATURE_STATE_SUSPEND;
                        }
                    }
                    else
                    {
                        retVal_TemperatureChecker = EventCodeEnum.ENV_TEMPERAUTRE_NOT_MATCHED;
                    }
                }
                else
                {
                    retVal_TemperatureChecker = EventCodeEnum.ENV_TEMPERATURE_STATE_ERROR;
                }

                if (retVal_TemperatureChecker == EventCodeEnum.NONE)
                {
                    Temp_Notify = false;
                }
                else
                {
                    var lotinfos = this.LotOPModule().LotInfo.GetLotInfos();
                    var assignedlotinfos = lotinfos.FindAll(info => info.StageLotAssignState == LotAssignStateEnum.ASSIGNED);
                    
                    // lot assign 상태와 stagemode 를 확인하는 이유는 lot가 동작 하지 못하는 이유를 알려주기 위함인데, 할당 된 랏드가 없다면 notify 를 주지 않기 위함.
                    if (Temp_Notify == false && (assignedlotinfos.Count > 0 && this.StageSupervisor().StageMode == GPCellModeEnum.ONLINE))
                    {
                        this.NotifyManager().Notify(retVal_TemperatureChecker);
                        Temp_Notify = true;
                    }
                }

                // ChillerChecker
                if (this.TempController().IsUsingChillerState() && this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {
                    IChillerModule chillerModule = this.EnvControlManager().GetChillerModule();

                    // disconnected loader - cell OR EnvControlServiceProxy 보는 코드 추가
                    if (!this.LoaderController().GetconnectFlag() || this.EnvControlManager().GetIsExcute() == false)
                    {
                        isValid = false;
                        retVal_ChillerChecker = EventCodeEnum.ENV_CHILLER_NOT_CONNECTED;
                    }

                    if (isValid)
                    {
                        if (chillerModule.IsConnected)
                        {
                            // Chiller가 동작 중이 아닌 경우, 동작 제한
                            if (chillerModule.ChillerInfo?.CoolantActivate == false)
                            {
                                retVal_ChillerChecker = EventCodeEnum.CHILLER_ACTIVATE_ERROR;
                            }
                            else
                            {
                                
                                //  Coolant Valve가 닫힌 경우, 동작 제한
                                if (this.EnvControlManager().GetValveState(EnumValveType.IN) == false)
                                {
                                    retVal_ChillerChecker = EventCodeEnum.ENV_VALVE_STATE_ERROR;
                                }
                                else
                                {
                                    retVal_ChillerChecker = EventCodeEnum.NONE;
                                }
                            }
                        }
                        else
                        {
                            retVal_ChillerChecker = EventCodeEnum.ENV_CHILLER_NOT_CONNECTED;
                        }
                    }
                }
                else
                {
                    retVal_ChillerChecker = EventCodeEnum.NONE;
                }

                if (retVal_ChillerChecker == EventCodeEnum.NONE)
                {
                    Chiller_Notify = false;
                }
                else
                {
                    // TODO: event 1번 발생을 위한 변수로 추측되는데, 정상 -> 비정상일땐 1번 발생하지만, 이미 비정상 notify 발생한 후에 다른 비정상 상황오면 notify 하지 않는다.
                    if (Chiller_Notify == false)
                    {
                        this.NotifyManager().Notify(retVal_ChillerChecker);
                        Chiller_Notify = true;
                    }
                }

                if (retVal_TemperatureChecker == EventCodeEnum.NONE && retVal_ChillerChecker == EventCodeEnum.NONE)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.ENV_CONDITIONS_NOT_SATISFIED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        //public EventCodeEnum IsConditionSatisfied()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.ENV_CONDITIONS_NOT_SATISFIED;

        //    try
        //    {
        //        if (this.EnvModule().ModuleState.GetState() == ModuleStateEnum.IDLE || this.EnvModule().ModuleState.GetState() == ModuleStateEnum.DONE)
        //        {
        //            if(EnvConditionCheckerList != null)
        //            {
        //                int isExistErrorCheckerCount = EnvConditionCheckerList.FindAll(checker => checker.ErrorOccurredTime != null).Count;
        //                if(isExistErrorCheckerCount == 0)
        //                {
        //                    retVal = EventCodeEnum.NONE;
        //                }
        //            }
        //            else
        //            {
        //                retVal = EventCodeEnum.NONE;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}
        #endregion
    }
}

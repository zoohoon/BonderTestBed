using LogModule;
using PolishWaferParameters;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.PolishWafer;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;

namespace PolishWaferModule
{
    public abstract class PolishWaferModuleState : IInnerState
    {
        public abstract bool CanExecute(IProbeCommandToken token);
        public abstract EventCodeEnum Execute();
        public abstract PolishWaferModuleStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();

        public abstract EventCodeEnum Pause();

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        //public virtual EventCodeEnum ClearState()
        //{
        //    return EventCodeEnum.NONE;
        //}
        public abstract EventCodeEnum ClearState();

        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }

        public virtual EventCodeEnum PolishWaferValidate(bool isExist)
        {
            return EventCodeEnum.NONE;
        }
    }

    public abstract class PolishWaferModuleStateBase : PolishWaferModuleState
    {
        private PolishWaferModule _Module;

        public PolishWaferModule Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public PolishWaferModuleStateBase(PolishWaferModule module)
        {
            Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void CheckManualOP()
        {
            Func<bool> conditionFunc = () =>
            {
                if (Module.SequenceEngineManager().GetRunState() &&
                  (Module.StageSupervisor().WaferObject.WaferStatus == EnumSubsStatus.EXIST) &&
                  (Module.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.POLISH)
                  )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };

            Action doAction = () =>
            {
                ChangeIdleToRunningStateForManual();
            };

            Action abortAction = () => { };

            if (this.GetModuleState() == ModuleStateEnum.IDLE ||
                this.GetModuleState() == ModuleStateEnum.PAUSED)
            {
                bool isExecuted = Module.CommandManager().ProcessIfRequested<IDoManualPolishWaferCleaning>(
                                Module,
                                conditionFunc,
                                doAction,
                                abortAction);

                if (isExecuted == true)
                {
                    LoggerManager.Debug($"[PolishWaferModuleStateBase], CheckManualOP() : ProcessIfRequested is {isExecuted}");
                }
            }
        }
        private void ChangeIdleToRunningStateForManual()
        {
            Module.ManualCleaningParam = Module.CommandRecvProcSlot.Token.Parameter as PolishWaferCleaningParameter;

            if (Module.ManualCleaningParam != null)
            {
                Module.ManualCleaningInfo = new PolishWafertCleaningInfo(Module.ManualCleaningParam.HashCode);
                Module.InnerStateTransition(new PolishWaferModuleRunningState(Module));
            }
            else
            {
                LoggerManager.Error($"Manual Cleaning command not exist.");
            }
        }
    }

    public class PolishWaferModuleIdleState : PolishWaferModuleStateBase
    {
        public PolishWaferModuleIdleState(PolishWaferModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.Template.SchedulingModule.IsExecute())
                {
                    List<ISubModule> modules = Module.Template.GetProcessingModule();

                    foreach (var subModule in modules)
                    {
                        if (subModule.IsExecute())
                        {
                            retVal = subModule.ClearData();

                            if (retVal != EventCodeEnum.NONE)
                            {
                                break;
                            }
                        }
                    }
                    
                    LoggerManager.Debug($"PolishWaferModuleIdleState(): SchedulingModule.IsExecute(): true, modules.Count:{modules.Count}");
                    
                    if (retVal == EventCodeEnum.NONE)
                    {
                        Module.InnerStateTransition(new PolishWaferModuleRunningState(Module));
                    }
                    else
                    {
                        Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Clear data is faild.", Module.PolishWaferModuleState.GetType().Name)));
                        Module.NotifyManager().Notify(EventCodeEnum.POLISHWAFER_CAN_NOT_PERFORMED);
                    }
                }

                CheckManualOP();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }


        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new PolishWaferModulePauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IDoManualPolishWaferCleaning;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }
    }
    public class PolishWaferModuleRunningState : PolishWaferModuleStateBase
    {
        public PolishWaferModuleRunningState(PolishWaferModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NODATA;

            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new PolishWaferModuleDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");

                    return EventCodeEnum.NONE;
                }
                else
                {
                    bool PolishWaferCleaningRetry = Module.ProcessingInfo.GetCurrentPolishWaferCleaningRetry();

                    if (Module.IsManualTriggered == true)
                    {
                        retVal = Module.DoCleaningProcessing();
                    }
                    else if (PolishWaferCleaningRetry == true &&
                        Module.StageSupervisor().WaferObject.WaferStatus == EnumSubsStatus.EXIST &&
                        Module.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.POLISH)
                    {
                        string errorReasonStr = "";

                        if (Module.CheckWaferSource(ref errorReasonStr))
                        {
                            Module.InnerStateTransition(new PolishWaferModuleLoadedState(Module));
                        }
                        else
                        {
                            Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, EventCodeEnum.INVALID_PARAMETER_FIND, $"{errorReasonStr}", Module.PolishWaferModuleState.GetType().Name)));
                            Module.NotifyManager().Notify(EventCodeEnum.POLISHWAFER_CAN_NOT_PERFORMED);
                        }
                    }
                    else
                    {
                        Module.InnerStateTransition(new PolishWaferModuleRequestingState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

    }

    public class PolishWaferModuleRequestingState : PolishWaferModuleStateBase
    {
        public PolishWaferModuleRequestingState(PolishWaferModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NODATA;

            try
            {
                retVal = Module.SetRemainingCleaningData();

                if (retVal == EventCodeEnum.NONE)
                {
                    IPolishWaferCleaningParameter curcleaningparam = Module.GetCurCleaningParam();

                    // 이미 올라와 있는 경우
                    if (Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST &&
                        Module.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.POLISH)
                    {
                        IPolishWaferSourceInformation pwinfo = Module.StageSupervisor().WaferObject.GetPolishInfo();

                        if (pwinfo != null && curcleaningparam != null)
                        {
                            // => PW를 다시 로드 해야 됨. 즉, SetReady()를 통해, 현재 PW를 언로드 시키고, 다음 PW를 Load해야 됨.
                            if (pwinfo.DefineName.Value != curcleaningparam.WaferDefineType.Value)
                            {
                                if (Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.READY)
                                {
                                    // Unload가 가능하도록 State를 READY로 변경.
                                    Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.READY);
                                }

                                // Polish Wafer를 Unload 시키기 위해, SUSPENDED 상태로 전환되어야 함.
                                Module.InnerStateTransition(new PolishWaferModuleWaitUntilLoadWaferState(Module));
                            }
                            else
                            {
                                // => PW를 다시 로드 안해도 됨. 즉, Loaded 상태로 가면 된다.
                                Module.InnerStateTransition(new PolishWaferModuleLoadedState(Module));
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"{this.GetType().Name}, Execute() : pwinfo = {pwinfo}, curcleaningparam = {curcleaningparam}");
                        }
                    }
                    else
                    {
                        if ((curcleaningparam != null) && (curcleaningparam.WaferDefineType.Value != null) && (curcleaningparam.WaferDefineType.Value != string.Empty))
                        {
                            Module.InnerStateTransition(new PolishWaferModuleWaitUntilLoadWaferState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                        }
                    }
                }
                else if (retVal == EventCodeEnum.POLISHWAFER_NOT_EXIST_CLEANING)
                {
                    Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Logic Error - 1", Module.PolishWaferModuleState.GetType().Name)));
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;

                    Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Logic Error - 2", Module.PolishWaferModuleState.GetType().Name)));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.REQUESTING;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

    }

    public class PolishWaferModuleWaitUntilLoadWaferState : PolishWaferModuleStateBase
    {
        public PolishWaferModuleWaitUntilLoadWaferState(PolishWaferModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                // Wafer가 아직 올라오지 않은 경우 || 이미 올라가 있는 웨이퍼가 클리닝이 끝난 경우
                retVal = Module.CheckPolishWaferOnChuck();

                // Wafer가 Load 됨.
                if (retVal == EventCodeEnum.NONE)
                {
                    if (Module.LotStartFlag == true)
                    {
                        Module.LotStartFlag = false;

                        LoggerManager.Debug($"[PolishWaferModuleRunningState] : LotStartFlag is reset.");
                    }

                    if (Module.LotEndFlag == true)
                    {
                        Module.LotEndFlag = false;

                        LoggerManager.Debug($"[PolishWaferModuleRunningState] : LotEndFlag is reset.");
                    }

                    // 이전 State가 Running이라면, GetRunState()를 통해, Running으로 갈 수 있는지 확인하고 스테이트의 전환이 이루어져야 한다.
                    if (Module.PreInnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                    {
                        if (Module.SequenceEngineManager().GetRunState() == true)
                        {
                            Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.CLEANING);

                            //Module.InnerStateTransition(Module.PreInnerState);
                            Module.InnerStateTransition(new PolishWaferModuleLoadedState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}, PreInnerState: {Module.PreInnerState.GetModuleState()}");
                        }
                    }
                    else
                    {
                        Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.CLEANING);

                        //Module.InnerStateTransition(Module.PreInnerState);
                        Module.InnerStateTransition(new PolishWaferModuleLoadedState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    }

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    //No WORKS.
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override EventCodeEnum PolishWaferValidate(bool isExist)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errormessage = "";
            try
            {
                if (!isExist)
                {
                    string RequestedWaferName = string.Empty;
                    bool IsPWRequested = Module.IsRequested(ref RequestedWaferName);
                    if (IsPWRequested && !string.IsNullOrEmpty(RequestedWaferName)) 
                    {
                        errormessage = $"{RequestedWaferName} type polsh wafer does not exist.";
                    }
                    else 
                    {
                        errormessage = "No wafer avaliable";
                    }

                    retVal = EventCodeEnum.POLISHWAFER_NO_WAFER_AVAILABLE;
                    retVal = Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, $"{errormessage}", Module.PolishWaferModuleState.GetType().Name)));
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.WAITLOADWAFER;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                PolishWaferParameter _PWParameter = Module.PolishWaferParameter as PolishWaferParameter;

                LoggerManager.Debug($"PolishWaferModuleWaitUntilLoadWaferState.End(): IntervalParameters.Count: {_PWParameter.PolishWaferIntervalParameters.Count} ");
                foreach (var intervalparam in _PWParameter.PolishWaferIntervalParameters)
                {
                    int IntervalIndex = _PWParameter.PolishWaferIntervalParameters.IndexOf(intervalparam);

                    bool intervalTriggered = Module.ProcessingInfo.GetIntervalTrigger(intervalparam.HashCode);

                    if (intervalTriggered)
                    {
                        Module.ProcessingInfo.SetIntervalTrigger(intervalparam, false);

                        LoggerManager.Debug($"[PolishWaferModuleWaitUntilLoadWaferState] End() : Canceled, Interval Triggered, Index = {IntervalIndex}, TriggerMode = {intervalparam.CleaningTriggerMode.Value}, Cleaing Count = {intervalparam.CleaningParameters.Count}");

                        Module.ProcessingInfo.ResetAllCleaningProcessedParam(intervalparam.HashCode);
                    }
                }
                Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            // TODO : Logic check
            Module.InnerStateTransition(new PolishWaferModulePauseState(Module));
            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }
    }

    public class PolishWaferModuleLoadedState : PolishWaferModuleStateBase
    {
        public PolishWaferModuleLoadedState(PolishWaferModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NODATA;

            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new PolishWaferModuleDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }
                else
                {
                    if (Module.IsManualTriggered == true)
                    {
                        // 올 수 없는 경우, 에러 내서 체크하자.
                        Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Unknown path", Module.PolishWaferModuleState.GetType().Name)));
                    }
                    else
                    {
                        retVal = Module.DoCleaningProcessing();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.CLEANING_WAFER_LOADED;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

    }
    public class PolishWaferModuleSuspendState : PolishWaferModuleStateBase
    {
        IInnerState RememberInnerState { get; set; }
        public PolishWaferModuleSuspendState(PolishWaferModule module) : base(module)
        {
            RememberInnerState = Module.InnerState;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.SequenceEngineManager().GetRunState() == true)
                {
                    // 내가 갖고 있는 (SendSlot의 HashCode)와 Target의 Token이 갖고 있는 HashCode가 같은지 비교.
                    if (Module.CommandSendSlot.Token.SubjectInfo == Module.PinAligner().CommandRecvDoneSlot.Token.SubjectInfo)
                    {
                        if (Module.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.DONE &&
                            Module.PinAligner().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                        {
                            if (Module.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                            {
                                Module.PinAligner().CommandRecvDoneSlot.ClearToken();

                                if (Module.PreInnerState.GetModuleState() == ModuleStateEnum.ERROR)
                                {
                                    retVal = Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
                                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                                }
                                else
                                {
                                    retVal = Module.InnerStateTransition(RememberInnerState);
                                }

                                //Module.InnerStateTransition(new PolishWaferModuleRunningState(Module));
                            }
                            else
                            {
                                retVal = EventCodeEnum.POLISHWAFER_CLEANING_PIN_ALIGNMENT_NOT_DONE;

                                // 핀 얼라인이 동작됐지만, 에러 발생된 경우.
                                Module.CommandSendSlot.ClearToken();
                                Module.PinAligner().CommandRecvDoneSlot.ClearToken();
                                //Module.PinAligner().CommandRecvDoneSlot.Token.SetError();

                                Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Pin Alignment is failed", Module.PolishWaferModuleState.GetType().Name)));
                            }
                        }
                        else if (Module.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ABORTED ||
                                 Module.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ERROR)
                        {
                            //Module.ReasonOfError.Reason = "Pin Alignment is failed";
                            LoggerManager.Debug("Request pin alignment for needle cleaning... failed (pin alignment is failed)");

                            retVal = EventCodeEnum.POLISHWAFER_CLEANING_PIN_ALIGNMENT_NOT_DONE;

                            Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Pin Alignment is failed", Module.PolishWaferModuleState.GetType().Name)));
                        }
                        else if (Module.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.NOCOMMAND)
                        {
                            LoggerManager.Debug("Request pin alignment before needle cleaning... failed. (no response)");

                            //Module.ReasonOfError.Reason = "Pin Alignment Command no response";

                            retVal = EventCodeEnum.POLISHWAFER_CLEANING_PIN_ALIGNMENT_COMMAND_DISAPPERED;

                            Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Pin Alignment command no response", Module.PolishWaferModuleState.GetType().Name)));
                        }
                        else if (Module.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.REJECTED)
                        {
                            LoggerManager.Debug("Request pin alignment for needle cleaning... failed. (busy)");

                            retVal = EventCodeEnum.POLISHWAFER_CLEANING_PIN_ALIGNMENT_COMMAND_REJECTED;

                            Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Pin Alignment command is rejected", Module.PolishWaferModuleState.GetType().Name)));
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

        public override EventCodeEnum PolishWaferValidate(bool isExist)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (!isExist)
                {
                    //Module.InnerStateTransition(new PolishWaferModuleErrorState(Module));

                    retVal = EventCodeEnum.POLISHWAFER_NO_WAFER_AVAILABLE;
                    retVal = Module.InnerStateTransition(new PolishWaferModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "No wafer avaliable", Module.PolishWaferModuleState.GetType().Name)));
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.SUSPENDED;
        }

        public override EventCodeEnum Pause()
        {
            // TODO : Logic check
            Module.InnerStateTransition(new PolishWaferModulePauseState(Module));
            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }
    }

    public class PolishWaferModuleErrorState : PolishWaferModuleStateBase
    {
        public PolishWaferModuleErrorState(PolishWaferModule module, EventCodeInfo eventcode) : base(module)
        {
            Module.CompleteManualOperation(eventcode.EventCode);

            LoggerManager.ActionLog(ModuleLogType.CLEANING, StateLogType.ERROR, $"{eventcode.EventCode}", this.Module.LoaderController().GetChuckIndex());
            //LoggerManager.LOTLog($"[CELL{this.Module.LoaderController().GetChuckIndexString()}] [CLEAING] [ERROR]", this.Module.LoaderController().GetChuckIndex());

            if (this.GetModuleState() == ModuleStateEnum.ERROR)
            {
                Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
            }
            else
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Current State = {this.GetModuleState()}, Can not add ReasonOfError.");
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (Module.LotOPModule().InnerState.GetModuleState() != ModuleStateEnum.RUNNING)
                {
                    Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                Module.InnerStateTransition(new PolishWaferModulePauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }
    }

    public class PolishWaferModuleAbortState : PolishWaferModuleStateBase
    {
        public PolishWaferModuleAbortState(PolishWaferModule module) : base(module)
        {
            if (Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST &&
                Module.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.POLISH)
            {
                if (Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.CLEANING)
                {
                    Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SKIPPED);
                }
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.ABORT;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = Module.InnerStateTransition(new PolishWaferModulePauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }

    }

    public class PolishWaferModuleDoneState : PolishWaferModuleStateBase
    {
        public PolishWaferModuleDoneState(PolishWaferModule module) : base(module)
        {
            Module.CompleteManualOperation(EventCodeEnum.NONE);

            LoggerManager.ActionLog(ModuleLogType.CLEANING, StateLogType.DONE, $"", Module.LoaderController().GetChuckIndex());
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }

                if (Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.NOT_EXIST)
                {
                    Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
                }

                if (Module.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.DONE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new PolishWaferModulePauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }
    }

    public class PolishWaferModulePauseState : PolishWaferModuleStateBase
    {
        public PolishWaferModulePauseState(PolishWaferModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                CheckManualOP();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override PolishWaferModuleStateEnum GetState()
        {
            return PolishWaferModuleStateEnum.PAUSED;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Abort()
        {
            Module.InnerStateTransition(new PolishWaferModuleAbortState(Module));

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (Module.PreInnerState.GetModuleState() == ModuleStateEnum.ERROR)
                {
                    retVal = Module.InnerStateTransition(new PolishWaferModuleIdleState(Module));
                }
                else
                {
                    retVal = Module.InnerStateTransition(Module.PreInnerState);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new PolishWaferModuleAbortState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = token is IDoManualPolishWaferCleaning;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValidCommand;
        }
    }
}

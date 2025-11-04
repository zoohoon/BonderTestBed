//namespace EnvModule
//{
//    using LogModule;
//    using ProberErrorCode;
//    using ProberInterfaces;
//    using ProberInterfaces.Command;
//    using ProberInterfaces.EnvControl;
//    using ProberInterfaces.State;
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Text;
//    using System.Threading.Tasks;

//    public abstract class EnvModuleState : IInnerState
//    {
//        public abstract bool CanExecute(IProbeCommandToken token);
//        public abstract EventCodeEnum Execute();
//        public abstract EventCodeEnum Pause();
//        public abstract EnumEnvModuleState GetState();
//        public abstract ModuleStateEnum GetModuleState();
//        public virtual EventCodeEnum End()
//        {
//            return EventCodeEnum.NONE;
//        }

//        public virtual EventCodeEnum Abort()
//        {
//            return EventCodeEnum.NONE;
//        }
//        public abstract EventCodeEnum ClearState();
//        public virtual EventCodeEnum Resume()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }

//    public abstract class EnvModuleStateBase : EnvModuleState
//    {
//        private EnvModule _Module;

//        public EnvModule Module
//        {
//            get { return _Module; }
//            set { _Module = value; }
//        }

//        public EnvModuleStateBase(EnvModule module)
//        {
//            Module = module;
//        }

//        public override EventCodeEnum ClearState()
//        {
//            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

//            try
//            {
//                retval = Module.InnerStateTransition(new EnvIdleState(Module));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }

//            return retval;
//        }
//        public override bool CanExecute(IProbeCommandToken token)
//        {
//            var isInjected = false;
//            return isInjected;
//        }
//    }

//    public class EnvIdleState : EnvModuleStateBase
//    {
//        public EnvIdleState(EnvModule module) : base(module)
//        {

//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                if(SchedulingExecute() == EventCodeEnum.NONE)
//                {
//                    bool initErrorOccurred = false;
//                    bool checkingErrorOccurred = false;

//                    // Init()
//                    if (Module.EnvConditionCheckerList != null)
//                    {
//                        foreach (var checker in Module.EnvConditionCheckerList)
//                        {
//                            if (checker != null)
//                            {
//                                checker.ErrorOccurredTime = null;
//                                var retInit = checker.Init();
//                                if (retInit != EventCodeEnum.NONE)
//                                {
//                                    initErrorOccurred = true;
//                                    LoggerManager.Debug($"[EnvModule] Checker Init Fail. Chekcer : {checker.CheckerClassName.Value}, ErrorCode : {retInit}.");
//                                }
//                            }
//                        }
//                    }

//                    // Checking()
//                    if (initErrorOccurred == false)
//                    {
//                        if (Module.EnvConditionCheckerList != null)
//                        {
//                            foreach (var checker in Module.EnvConditionCheckerList)
//                            {
//                                string errorMsg = "";
//                                var retChecking = checker?.Checking(out errorMsg) ?? EventCodeEnum.NONE;

//                                if (retChecking != EventCodeEnum.NONE)
//                                {
//                                    if (checker.ErrorOccurredTime == null)
//                                    {
//                                        checker.ErrorOccurredTime = DateTime.Now;

//                                        if(String.IsNullOrEmpty(errorMsg))
//                                        {
//                                            var param = Module.NotifyManager().GetNotifyParam(retChecking);

//                                            if(param != null)
//                                            {
//                                                errorMsg = param.Message;
//                                            }
//                                        }

//                                        LoggerManager.Debug($"[EnvModule] Checking Error Occurred. Chekcer : {checker.CheckerClassName.Value}, ErrorOccurredTime : {checker.ErrorOccurredTime}, ErrorCode : {retChecking}, ErrorMsg : {errorMsg}.");
//                                        Module.NotifyManager().Notify(retChecking);
//                                    }

//                                    checkingErrorOccurred = true;
//                                }
//                            }
//                        }
                        
//                        if(checkingErrorOccurred)
//                        {
//                            retVal = Module.InnerStateTransition(new EnvSuspendState(Module));
//                        }
//                    }
//                    else
//                    {
//                        retVal = Module.InnerStateTransition(new EnvErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "EnvCheker init fail.", this.GetType().Name)));
//                    }
//                }
//                else
//                {
//                    retVal = EventCodeEnum.NONE;
//                }
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }

//            return retVal;
//        }

//        private EventCodeEnum SchedulingExecute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                if(Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
//                {
//                    retVal = EventCodeEnum.NONE;
//                }
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }

//        public override EnumEnvModuleState GetState()
//        {
//            return EnumEnvModuleState.IDLE;
//        }
//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.IDLE;
//        }

//        public override EventCodeEnum Pause()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

//            try
//            {
//                retVal = Module.InnerStateTransition(new EnvPauseState(Module));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }

//            return retVal;
//        }
//    }

//    public class EnvRunningState : EnvModuleStateBase
//    {
//        public EnvRunningState(EnvModule module) : base(module)
//        {

//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                bool errorOccurred = false;

//                if (Module.EnvConditionCheckerList != null)
//                {
//                    foreach (var checker in Module.EnvConditionCheckerList)
//                    {
//                        string errorMsg = "";
//                        var retCheck = checker?.Checking(out errorMsg) ?? EventCodeEnum.NONE;
//                        if (retCheck != EventCodeEnum.NONE)
//                        {
//                            if(checker.ErrorOccurredTime == null)
//                            {
//                                checker.ErrorOccurredTime = DateTime.Now;
//                                LoggerManager.Debug($"[EnvModule] Checking Error Occurred. Chekcer : {checker.CheckerClassName.Value}, ErrorOccurredTime : {checker.ErrorOccurredTime}, ErrorCode : {retCheck}, ErrorMsg : {errorMsg}.");
//                            }
//                            errorOccurred = true;
//                        }
//                    }

//                    if(errorOccurred)
//                    {
//                        retVal = Module.InnerStateTransition(new EnvSuspendState(Module));
//                    }
//                }

//                if(errorOccurred == false)
//                {
//                    retVal = Module.InnerStateTransition(new EnvDoneState(Module));
//                }
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }

//            return retVal;
//        }

//        public override EnumEnvModuleState GetState()
//        {
//            return EnumEnvModuleState.RUNNING;
//        }
//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.RUNNING;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//    }
//    public class EnvSuspendState : EnvModuleStateBase
//    {
//        public EnvSuspendState(EnvModule module) : base(module)
//        {

//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                bool errorOccurred = false;
//                bool timeoutErrorOccurred = false;

//                if (Module.EnvConditionCheckerList != null)
//                {
//                    string errorMsgStr = "";
//                    foreach (var checker in Module.EnvConditionCheckerList)
//                    {
//                        if(checker != null)
//                        {
//                            string errorMsg;
//                            var retChecking = checker.Checking(out errorMsg);
//                            if (retChecking != EventCodeEnum.NONE)
//                            {
//                                if (checker.ErrorOccurredTime == null)
//                                {
//                                    checker.ErrorOccurredTime = DateTime.Now;
//                                    LoggerManager.Debug($"[EnvModule] Checking Error Occurred. Chekcer : {checker.CheckerClassName.Value}, ErrorCode : {retChecking}, ErrorOccurredTime : {checker.ErrorOccurredTime}");
//                                }
//                                if ((DateTime.Now - checker.ErrorOccurredTime).Value.TotalSeconds >= checker.ErrorOccurredTimeoutSec)
//                                {
//                                    retVal = retChecking;
//                                    if (String.IsNullOrEmpty(errorMsg))
//                                    {
//                                        var param = Module.NotifyManager().GetNotifyParam(retChecking);
//                                        if (param != null)
//                                        {
//                                            errorMsg = param.Message;
//                                        }
//                                    }
//                                    errorMsgStr += $"\n{errorMsg}";
//                                    timeoutErrorOccurred = true;
//                                }
//                                errorOccurred = true;
//                            }
//                        }
//                    }

//                    if(timeoutErrorOccurred)
//                    {
//                        retVal = Module.InnerStateTransition(new EnvErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, errorMsgStr, this.GetType().Name)));
//                    }
//                }

//                if(errorOccurred == false)
//                {
//                    retVal = Module.InnerStateTransition(new EnvDoneState(Module));
//                }
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }

//        public override EnumEnvModuleState GetState()
//        {
//            return EnumEnvModuleState.SUSPEND;
//        }
//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.SUSPENDED;
//        }

//        public override EventCodeEnum Pause()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                retVal = Module.InnerStateTransition(new EnvPauseState(Module));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }
//    }

//    public class EnvPauseState : EnvModuleStateBase
//    {
//        public EnvPauseState(EnvModule module) : base(module)
//        {

//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                retVal = EventCodeEnum.NONE;
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }

//        public override EnumEnvModuleState GetState()
//        {
//            return EnumEnvModuleState.PAUSE;
//        }
//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.PAUSED;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }
//        public override EventCodeEnum Resume()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                retVal = Module.InnerStateTransition(Module.PreInnerState);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }

//        public override EventCodeEnum End()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

//            try
//            {
//                retVal = Module.InnerStateTransition(new EnvAbortState(Module));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }

//            return retVal;
//        }
//    }

//    public class EnvDoneState : EnvModuleStateBase
//    {
//        public EnvDoneState(EnvModule module) : base(module)
//        {

//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                retVal = Module.InnerStateTransition(new EnvIdleState(Module));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }

//        public override EnumEnvModuleState GetState()
//        {
//            return EnumEnvModuleState.DONE;
//        }
//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.DONE;
//        }

//        public override EventCodeEnum Pause()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

//            try
//            {
//                retVal = Module.InnerStateTransition(new EnvPauseState(Module));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }

//            return retVal;
//        }
//    }

//    public class EnvAbortState : EnvModuleStateBase
//    {
//        public EnvAbortState(EnvModule module) : base(module)
//        {

//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                retVal = Module.InnerStateTransition(new EnvIdleState(Module));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }

//        public override EnumEnvModuleState GetState()
//        {
//            return EnumEnvModuleState.ABORT;
//        }
//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.ABORT;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.UNDEFINED;
//        }

//    }

//    public class EnvErrorState : EnvModuleStateBase
//    {
//        public EnvErrorState(EnvModule module, EventCodeInfo eventcode) : base(module)
//        {
//            try
//            {
//                Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                if (Module.LotOPModule().InnerState.GetModuleState() != ModuleStateEnum.RUNNING)
//                {
//                    Module.InnerStateTransition(new EnvIdleState(Module));
//                }
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }

//        public override EnumEnvModuleState GetState()
//        {
//            return EnumEnvModuleState.ERROR;
//        }
//        public override ModuleStateEnum GetModuleState()
//        {
//            return ModuleStateEnum.ERROR;
//        }

//        public override EventCodeEnum Pause()
//        {
//            return EventCodeEnum.NONE;
//        }

//        public override EventCodeEnum Resume()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                retVal = Module.InnerStateTransition(new EnvIdleState(Module));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }

//        public override EventCodeEnum End()
//        {
//            EventCodeEnum retVal = EventCodeEnum.NONE;

//            try
//            {
//                Module.InnerStateTransition(new EnvIdleState(Module));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }

//            return retVal;
//        }
//    }
//}

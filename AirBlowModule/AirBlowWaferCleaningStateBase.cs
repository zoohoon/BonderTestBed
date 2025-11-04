using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.State;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AirBlowModule
{
    public enum EnumWaferAirBlowCleaningState
    {
        UNDEFINED = -1,
        IDLE,
        AFTER_LOADING_RUN,
        BEFORE_UNLOADING_RUN,
        AFTER_LOADING_DONE,
        BEFORE_UNLOADING_DONE,
        DONE,
        PAUSE,
        ERROR,
        ABORT,
    }

    public abstract class AirBlowWaferCleaningState : IInnerState
    {
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract EnumWaferAirBlowCleaningState GetState();
        public abstract ModuleStateEnum GetModuleState();
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
    }
    public abstract class AirBlowWaferCleaningStateBase : AirBlowWaferCleaningState
    {
        protected AirBlowWaferCleaningModule Module;

        public virtual bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        protected static Random rnd = new Random();
        protected static DateTime EndTime;
        public AirBlowWaferCleaningStateBase(AirBlowWaferCleaningModule module)
        {
            this.Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new AirBlowWaferCleaningIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class AirBlowWaferCleaningIdleState : AirBlowWaferCleaningStateBase
    {
        public AirBlowWaferCleaningIdleState(AirBlowWaferCleaningModule abmodule) : base(abmodule) { }

        public DateTime GetLastPinAlignDoneTime()
        {
            var pinAlignerLastStateInfo = Module.PinAligner().TransitionInfo
                .Where(item => item.state == ModuleStateEnum.DONE)
                .Last();

            return pinAlignerLastStateInfo.TransitionTime;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IAirBlowWaferCleaningCommand;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var waferstatus = Module.StageSupervisor().WaferObject.GetStatus();

                //New Code
                if (Module.CommandRecvSlot.IsRequested<IAirBlowWaferCleaningCommand>())
                {
                    Func<bool> conditionFunc = () =>
                    {
                        bool isCanExecute;
                        if (Module.ABDeviceFile.ChuckCleaningBeforeUnLoading.Value)
                        {
                            isCanExecute =
                                Module.StageSupervisor().StageModuleState.GetState() == StageStateEnum.AIRBLOW ||
                                Module.SequenceEngineManager().GetRunState();
                        }
                        else
                        {
                            isCanExecute = true;
                        }
                        return isCanExecute;
                    };

                    Action doAction = () =>
                    {
                        if (Module.ABDeviceFile.ChuckCleaningBeforeUnLoading.Value == false ||
                            Module.StageSupervisor().StageModuleState.GetState() == StageStateEnum.AIRBLOW)
                        {
                            Module.InnerStateTransition(new AirBlowWaferCleaningDoneState(Module));
                        }
                        else
                        {
                            Module.InnerStateTransition(new AirBlowCleaningBeforeUnLoadingRunState(Module));
                        }
                    };
                    Action abortAction = () => { };

                    Module.CommandManager().ProcessIfRequested<IAirBlowWaferCleaningCommand>(
                        Module,
                        conditionFunc,
                        doAction,
                        abortAction);
                }
                // OLD Code
                //if (AirBlowModule.CmdSlot.GetState() == ProberInterfaces.Command.CommandStateEnum.REQUESTED &&
                //AirBlowModule.CmdSlot.Token is IAirBlowWaferCleaningCommand)
                //{
                //    if (AirBlowModule.ABDeviceFile.ChuckCleaningBeforeUnLoading == true)
                //    {
                //        if (AirBlowModule.SequenceEngineManager.GetRunState())
                //        {
                //            AirBlowModule.InnerStateTransition(new AirBlowCleaningBeforeUnLoadingRunState(AirBlowModule));
                //            LoggerManager.Debug($"{0}.StateTransition() : STATE={1} ", GetType().Name, AirBlowModule.WaferAirBlowState.GetState());
                //            ret = EventCodeEnum.NONE;
                //        }
                //    }
                //    else
                //    {
                //        AirBlowModule.InnerStateTransition(new AirBlowWaferCleaningDoneState(AirBlowModule));
                //        LoggerManager.Debug($"{0}.StateTransition() : STATE={1} ", GetType().Name, AirBlowModule.WaferAirBlowState.GetState());
                //        ret = EventCodeEnum.NONE;
                //    }
                //}
                else
                {
                    if ((Module.PrevWaferStatus == EnumSubsStatus.NOT_EXIST && waferstatus == EnumSubsStatus.EXIST))
                    {
                        if (Module.ABDeviceFile.ChuckCleaningAfterLoading.Value == true)
                        {
                            if (Module.SequenceEngineManager().GetRunState())
                            {
                                Module.InnerStateTransition(new AirBlowCleaningAfterLoadingRunState(Module));
                                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
                                ret = EventCodeEnum.NONE;

                                Module.PrevWaferStatus = waferstatus;
                            }
                            else
                            {
                                ret = EventCodeEnum.NONE;
                            }
                        }
                        else
                        {
                            //AirBlowModule.InnerStateTransition(new AirBlowWaferCleaningDoneState(AirBlowModule));
                            //LoggerManager.Debug($"{0}.StateTransition() : STATE={1} ", GetType().Name, AirBlowModule.WaferAirBlowState.GetState());
                            //ret = EventCodeEnum.NONE;

                            //AirBlowModule.PrevWaferStatus = waferstatus;
                        }
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }

                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred AirBlowWaferCleaningIdleState.");
                //LoggerManager.Error($err.InnerException);

                LoggerManager.Exception(err);

                Module.InnerStateTransition(new AirBlowWaferCleaningErrorState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
            }


            //AirBlowModule.PrevWaferStatus = waferstatus;
            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override EnumWaferAirBlowCleaningState GetState()
        {
            return EnumWaferAirBlowCleaningState.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            retVal = Module.InnerStateTransition(new AirBlowWaferCleaningPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
    }

    public class AirBlowCleaningAfterLoadingRunState : AirBlowWaferCleaningStateBase
    {
        public AirBlowCleaningAfterLoadingRunState(AirBlowWaferCleaningModule abmodule) : base(abmodule) { }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new AirBlowWaferCleaningDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
                    return EventCodeEnum.NONE;
                }

                var task = Task.Run(() =>
                {
                    ret = Module.ChuckCleaning();
                });

                //Module.WaitCancelDialogService().ShowDialog("Please Wait");
                Module.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please Wait");

                task.Wait();

                if (ret == EventCodeEnum.NONE)
                {
                    System.Threading.Thread.Sleep(2000);
                    //Module.WaitCancelDialogService().CloseDialog();
                    Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    Module.InnerStateTransition(new AirBlowWaferCleaningDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
                }
                else
                {
                    if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                    {
                        Module.InnerStateTransition(new AirBlowWaferCleaningDoneState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
                        return EventCodeEnum.NONE;
                    }

                    System.Threading.Thread.Sleep(2000);
                    //Module.WaitCancelDialogService().CloseDialog();
                    Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    Module.InnerStateTransition(new AirBlowWaferCleaningErrorState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
                    ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred AirBlowCleaningAfterLoadingRunState.");
                //LoggerManager.Error($err.InnerException);

                ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                Module.InnerStateTransition(new AirBlowWaferCleaningErrorState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");

                LoggerManager.Exception(err);
            }

            //AirBlowModule.GetRequestToken().ProbeResponse.Execute(AirBlowModule.Container);
            //AirBlowModule.SetRequestToken(null);
            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumWaferAirBlowCleaningState GetState()
        {
            return EnumWaferAirBlowCleaningState.AFTER_LOADING_RUN;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AirBlowCleaningBeforeUnLoadingRunState : AirBlowWaferCleaningStateBase
    {
        public AirBlowCleaningBeforeUnLoadingRunState(AirBlowWaferCleaningModule abmodule) : base(abmodule) { }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new AirBlowWaferCleaningDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
                    return EventCodeEnum.NONE;
                }

                var task = Task.Run(() =>
                {
                    ret = Module.ChuckCleaning();
                });

                //Module.WaitCancelDialogService().ShowDialog("Please Wait");
                Module.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please Wait");

                task.Wait();

                if (ret == EventCodeEnum.NONE)
                {
                    System.Threading.Thread.Sleep(2000);

                    //Module.WaitCancelDialogService().CloseDialog();
                    Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    Module.InnerStateTransition(new AirBlowWaferCleaningDoneState(Module));

                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
                }
                else
                {

                    if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                    {
                        Module.InnerStateTransition(new AirBlowWaferCleaningDoneState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
                        return EventCodeEnum.NONE;
                    }
                    System.Threading.Thread.Sleep(2000);
                    //Module.WaitCancelDialogService().CloseDialog();
                    Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    Module.InnerStateTransition(new AirBlowWaferCleaningErrorState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");

                    ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                }
            }
            catch (Exception err)
            {
                //ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                //LoggerManager.Error($err.Message, "Error occurred AirBlowCleaningBeforeUnLoadingRunState.");
                //LoggerManager.Error($err.InnerException);

                ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                Module.InnerStateTransition(new AirBlowWaferCleaningErrorState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");

                LoggerManager.Exception(err);
            }

            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumWaferAirBlowCleaningState GetState()
        {
            return EnumWaferAirBlowCleaningState.BEFORE_UNLOADING_RUN;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class AirBlowWaferCleaningDoneState : AirBlowWaferCleaningStateBase
    {
        public AirBlowWaferCleaningDoneState(AirBlowWaferCleaningModule abmodule) : base(abmodule) { }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IAirBlowWaferCleaningCommand;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var waferstatus = Module.StageSupervisor().WaferObject.GetStatus();

                ////New Code
                //Func<bool> conditionFunc = () => true;
                //Action doAction = () => { };
                //Action abortAction = () => { };

                //AirBlowModule.CommandManager().ProcessIfRequested<IAirBlowWaferCleaningCommand>(
                //    AirBlowModule,
                //    conditionFunc,
                //    doAction,
                //    abortAction);

                //Old Code
                //if (AirBlowModule.CmdSlot.GetState() != ProberInterfaces.Command.CommandStateEnum.NOCOMMAND)
                //{
                //    AirBlowModule.CmdSlot.SetDone();
                //}

                // if (AirBlowModule.ABDeviceFile.AirBlowCleaningEnable == false)
                //{
                //  ret = EventCodeEnum.NONE;
                // }
                // else
                //{
                //if (waferstatus == EnumWaferStatus.NOT_EXIST ||
                //    AirBlowModule.StageSuperVisor.StageModuleState.GetState() != StageStateEnum.AIRBLOW)
                Module.InnerStateTransition(new AirBlowWaferCleaningIdleState(Module));
                //if (waferstatus == EnumWaferStatus.NOT_EXIST)
                //{
                //    AirBlowModule.PrevWaferStatus = waferstatus;
                //    AirBlowModule.InnerStateTransition(new AirBlowWaferCleaningIdleState(AirBlowModule));
                //    LoggerManager.Debug($"{0}.StateTransition() : STATE={1} ", GetType().Name, AirBlowModule.WaferAirBlowState.GetState());
                //}
                //else
                //{
                //    //Do Nothing 
                //    //LoggerManager.Debug($"{0} Nothing ", GetType().Name);
                //}
                //ret = EventCodeEnum.NONE;
                //}
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred AirBlowWaferCleaningDoneState.");
                //LoggerManager.Error($err.InnerException);
                LoggerManager.Exception(err);

                ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                Module.InnerStateTransition(new AirBlowWaferCleaningErrorState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.WaferAirBlowState.GetState()}");
            }


            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override EnumWaferAirBlowCleaningState GetState()
        {
            return EnumWaferAirBlowCleaningState.DONE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            retVal = Module.InnerStateTransition(new AirBlowWaferCleaningPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
    }

    public class AirBlowWaferCleaningAbortState : AirBlowWaferCleaningStateBase
    {
        public AirBlowWaferCleaningAbortState(AirBlowWaferCleaningModule abmodule) : base(abmodule) { }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
            ret = Module.InnerStateTransition(new AirBlowWaferCleaningIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override EnumWaferAirBlowCleaningState GetState()
        {
            return EnumWaferAirBlowCleaningState.ABORT;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }


    public class AirBlowWaferCleaningPauseState : AirBlowWaferCleaningStateBase
    {
        public AirBlowWaferCleaningPauseState(AirBlowWaferCleaningModule abmodule) : base(abmodule) { }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
           
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override EnumWaferAirBlowCleaningState GetState()
        {
            return EnumWaferAirBlowCleaningState.PAUSE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            retVal = Module.InnerStateTransition(Module.PreInnerState);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            retVal = Module.InnerStateTransition(new AirBlowWaferCleaningAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
    }
    //public class AirBlowCleaningSuspendedState : ChuckAirBlowCleaningStateBase
    //{
    //    public AirBlowCleaningSuspendedState(AirBlowChuckCleaningModule abmodule) : base(abmodule) { }

    //    public override EventCodeEnum Execute()
    //    {
    //        EventCodeEnum ret = EventCodeEnum.UNDEFINED;
    //        var waferstatus = AirBlowModule.StageSuperVisor.WaferObject.GetStatus();
    //        var waferstate = AirBlowModule.StageSuperVisor.WaferObject.GetState();

    //        if (AirBlowModule.AirBlowPrevState == EnumChuckAirBlowCleaningState.BEFORE_LOADING_RUN)
    //        {
    //            if (AirBlowModule.ABDeviceFile.ChuckCleaningAfterLoading == true &&
    //                waferstatus == EnumWaferStatus.EXIST &&
    //                waferstate == EnumWaferState.UNPROCESSED)
    //            {
    //                AirBlowModule.AirBlowPrevState = EnumChuckAirBlowCleaningState.SUSPENDED;
    //                AirBlowModule.InnerStateTransition(new AirBlowCleaningAfterLoadingRunState(AirBlowModule));
    //                LoggerManager.Debug($"{0}.StateTransition() : STATE={1} ", GetType().Name, AirBlowModule.AirBlowState.GetState());
    //            }
    //        }
    //        else if (AirBlowModule.AirBlowPrevState == EnumChuckAirBlowCleaningState.BEFORE_UNLOADING_RUN)
    //        {
    //            if (AirBlowModule.ABDeviceFile.ChuckCleaningAfterUnLoading == true &&
    //                waferstatus == EnumWaferStatus.NOT_EXIST)
    //            {
    //                AirBlowModule.AirBlowPrevState = EnumChuckAirBlowCleaningState.SUSPENDED;
    //                AirBlowModule.InnerStateTransition(new AirBlowCleaningAfterUnLoadingRunState(AirBlowModule));
    //                LoggerManager.Debug($"{0}.StateTransition() : STATE={1} ", GetType().Name, AirBlowModule.AirBlowState.GetState());
    //            }
    //        }
    //        else
    //        {
    //            //State keep
    //            ret = EventCodeEnum.NONE;
    //        }

    //        return ret;

    //    }

    //    public override ModuleStateEnum GetModuleState()
    //    {
    //        return ModuleStateEnum.SUSPENDED;
    //    }

    //    public override EnumChuckAirBlowCleaningState GetState()
    //    {
    //        return EnumChuckAirBlowCleaningState.SUSPENDED;
    //    }
    //}
    public class AirBlowWaferCleaningErrorState : AirBlowWaferCleaningStateBase
    {
        public AirBlowWaferCleaningErrorState(AirBlowWaferCleaningModule abmodule) : base(abmodule) { }

        public override EventCodeEnum Execute()
        {
            //LoggerManager.Error($"{GetType().Name} Error ");

            return EventCodeEnum.AIRBLOW_CLEANING_ERROR;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override EnumWaferAirBlowCleaningState GetState()
        {
            return EnumWaferAirBlowCleaningState.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AirBlowWaferCleaningUndefinedState : AirBlowWaferCleaningStateBase
    {
        public AirBlowWaferCleaningUndefinedState(AirBlowWaferCleaningModule abmodule) : base(abmodule) { }

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.UNDEFINED;
        }

        public override EnumWaferAirBlowCleaningState GetState()
        {
            return EnumWaferAirBlowCleaningState.UNDEFINED;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
}

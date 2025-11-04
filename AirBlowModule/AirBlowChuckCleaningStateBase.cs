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
    public enum EnumChuckAirBlowCleaningState
    {
        UNDEFINED = -1,
        IDLE,
        BEFORE_LOADING_RUN,
        AFTER_UNLOADING_RUN,
        DEVICE_CHANGE_RUN,
        BEFORE_LOADING_DONE,
        AFTER_UNLOADING_DONE,
        DONE,
        PAUSE,
        //SUSPENDED,
        ERROR,
        ABORT,
    }

    public abstract class ChuckAirBlowCleaningState : IInnerState
    {
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract EnumChuckAirBlowCleaningState GetState();
        public abstract ModuleStateEnum GetModuleState();
        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public abstract EventCodeEnum ClearState();
        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public abstract class ChuckAirBlowCleaningStateBase : ChuckAirBlowCleaningState
    {
        protected AirBlowChuckCleaningModule Module;

        public virtual bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        protected static Random rnd = new Random();
        protected static DateTime EndTime;

        public ChuckAirBlowCleaningStateBase(AirBlowChuckCleaningModule module)
        {
            this.Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new AirBlowChuckCleaningIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class AirBlowChuckCleaningIdleState : ChuckAirBlowCleaningStateBase
    {
        public AirBlowChuckCleaningIdleState(AirBlowChuckCleaningModule abmodule) : base(abmodule) { }

        public DateTime GetLastPinAlignDoneTime()
        {
            var pinAlignerLastStateInfo = Module.PinAligner().TransitionInfo
                .Where(item => item.state == ModuleStateEnum.DONE)
                .Last();

            return pinAlignerLastStateInfo.TransitionTime;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand;
            try
            {

                isValidCommand = token is IAirBlowChuckCleaningCommand;

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
                var lotrunlist = Module.LotOPModule().RunList;


                if (Module.CommandRecvSlot.IsRequested<IAirBlowChuckCleaningCommand>())
                {
                    //=> NewCode
                    Func<bool> conditionFunc = () =>
                    {
                        bool isCanExecute;
                        if (Module.ABDeviceFile.ChuckCleaningBeforeLoading.Value)
                        {
                            isCanExecute =
                                Module.StageSupervisor().StageModuleState.GetState() == StageStateEnum.AIRBLOW ||
                                Module.SequenceEngineManager.GetRunState();
                        }
                        else
                        {
                            isCanExecute = true;
                        }
                        return isCanExecute;
                    };

                    Action doAction = () =>
                    {
                        if (Module.ABDeviceFile.ChuckCleaningBeforeLoading.Value == false ||
                            Module.StageSupervisor().StageModuleState.GetState() == StageStateEnum.AIRBLOW)
                        {
                            Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                        }
                        else
                        {
                            Module.InnerStateTransition(new AirBlowCleaningBeforeLoadingRunState(Module));
                        }
                    };

                    Action abortAction = () => { };

                    Module.CommandManager().ProcessIfRequested<IAirBlowChuckCleaningCommand>(
                        Module,
                        conditionFunc,
                        doAction,
                        abortAction);

                }
                else
                {
                    if (Module.PrevWaferStatus == EnumSubsStatus.EXIST && waferstatus == EnumSubsStatus.NOT_EXIST)
                    {
                        if (Module.ABDeviceFile.ChuckCleaningAfterUnLoading.Value == true)
                        {
                            if (Module.SequenceEngineManager.GetRunState())
                            {
                                Module.InnerStateTransition(new AirBlowCleaningAfterUnLoadingRunState(Module));
                                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
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
                            Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                            ret = EventCodeEnum.NONE;
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
                //LoggerManager.Error($err.Message, "Error occurred AirBlowChuckCleaningIdleState.");
                //LoggerManager.Error($err.InnerException);
                LoggerManager.Exception(err);

                Module.InnerStateTransition(new AirBlowChuckCleaningErrorState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
            }

            //AirBlowModule.PrevWaferStatus = waferstatus;
            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override EnumChuckAirBlowCleaningState GetState()
        {
            return EnumChuckAirBlowCleaningState.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new AirBlowChuckCleaningPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class AirBlowChuckCleaningPauseState : ChuckAirBlowCleaningStateBase
    {
        public AirBlowChuckCleaningPauseState(AirBlowChuckCleaningModule abmodule) : base(abmodule) { }

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

                //AirBlowModule.InnerStateTransition(new AirBlowChuckCleaningDoneState(AirBlowModule));

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

        public override EnumChuckAirBlowCleaningState GetState()
        {
            return EnumChuckAirBlowCleaningState.PAUSE;
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
                retVal = Module.InnerStateTransition(new AirBlowChuckCleaningAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class AirBlowCleaningBeforeLoadingRunState : ChuckAirBlowCleaningStateBase
    {
        public AirBlowCleaningBeforeLoadingRunState(AirBlowChuckCleaningModule abmodule) : base(abmodule) { }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                    return EventCodeEnum.NONE;
                }

                var task = Task.Run(() =>
                {
                    retVal = Module.ChuckCleaning();
                });

                //Module.WaitCancelDialogService().ShowDialog("Wait");
                Module.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                task.Wait();

                if (retVal == EventCodeEnum.NONE)
                {
                    System.Threading.Thread.Sleep(2000);
                    //Module.WaitCancelDialogService().CloseDialog();
                    Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));

                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                }
                else
                {
                    if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                    {
                        Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                        return EventCodeEnum.NONE;
                    }

                    System.Threading.Thread.Sleep(2000);
                    //Module.WaitCancelDialogService().CloseDialog();
                    Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    Module.InnerStateTransition(new AirBlowChuckCleaningErrorState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                    retVal = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred AirBlowCleaningBeforeLoadingRunState.");
                //LoggerManager.Error($err.InnerException);

                retVal = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                Module.InnerStateTransition(new AirBlowChuckCleaningErrorState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");

                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumChuckAirBlowCleaningState GetState()
        {
            return EnumChuckAirBlowCleaningState.BEFORE_LOADING_RUN;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class AirBlowCleaningAfterUnLoadingRunState : ChuckAirBlowCleaningStateBase
    {
        public AirBlowCleaningAfterUnLoadingRunState(AirBlowChuckCleaningModule abmodule) : base(abmodule) { }

        public override EventCodeEnum Execute()
        {

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
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

                    Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                }
                else
                {
                    if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                    {
                        Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                        return EventCodeEnum.NONE;
                    }

                    System.Threading.Thread.Sleep(2000);
                    //Module.WaitCancelDialogService().CloseDialog();
                    Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    Module.InnerStateTransition(new AirBlowChuckCleaningErrorState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                    ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred AirBlowCleaningAfterUnLoadingRunState.");
                //LoggerManager.Error($err.InnerException);

                ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                Module.InnerStateTransition(new AirBlowChuckCleaningErrorState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");

                LoggerManager.Exception(err);
            }

            //AirBlowModule.GetCommandToken().ProbeResponse.Execute(AirBlowModule.Container);
            //AirBlowModule.SetCommandToken(null);

            return ret;

        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EnumChuckAirBlowCleaningState GetState()
        {
            return EnumChuckAirBlowCleaningState.AFTER_UNLOADING_RUN;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AirBlowCleaningDevicehangegRunState : ChuckAirBlowCleaningStateBase
    {
        public AirBlowCleaningDevicehangegRunState(AirBlowChuckCleaningModule abmodule) : base(abmodule) { }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                    return EventCodeEnum.NONE;
                }

                ret = Module.ChuckCleaning();
                if (ret == EventCodeEnum.NONE)
                {
                    Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                }
                else
                {
                    if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                    {
                        Module.InnerStateTransition(new AirBlowChuckCleaningDoneState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                        return EventCodeEnum.NONE;
                    }

                    Module.InnerStateTransition(new AirBlowChuckCleaningErrorState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
                    ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                }
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
            return ModuleStateEnum.RUNNING;
        }

        public override EnumChuckAirBlowCleaningState GetState()
        {
            return EnumChuckAirBlowCleaningState.DEVICE_CHANGE_RUN;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AirBlowChuckCleaningDoneState : ChuckAirBlowCleaningStateBase
    {
        public AirBlowChuckCleaningDoneState(AirBlowChuckCleaningModule abmodule) : base(abmodule) { }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand;
            try
            {
                isValidCommand = token is IAirBlowChuckCleaningCommand;

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
                //Func<bool> conditionFunc = () => true;
                //Action doAction = () => { };
                //Action abortAction = () => { };

                //AirBlowModule.CommandManager().ProcessIfRequested<IAirBlowChuckCleaningCommand>(
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
                // {
                //   ret = EventCodeEnum.NONE;
                // }
                //else
                // {
                //if (waferstatus == EnumWaferStatus.EXIST ||
                //    AirBlowModule.StageSuperVisor.StageModuleState.GetState() != StageStateEnum.AIRBLOW)
                Module.InnerStateTransition(new AirBlowChuckCleaningIdleState(Module));
                //if (waferstatus == EnumWaferStatus.EXIST)
                //{
                //    AirBlowModule.PrevWaferStatus = waferstatus;
                //    AirBlowModule.InnerStateTransition(new AirBlowChuckCleaningIdleState(AirBlowModule));
                //    LoggerManager.Debug($"{0}.StateTransition() : STATE={1} ", GetType().Name, AirBlowModule.AirBlowState.GetState());
                //}
                //else
                //{
                //}
                //ret = EventCodeEnum.NONE;
                //}
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred AirBlowChuckCleaningDoneState.");
                //LoggerManager.Error($err.InnerException);
                LoggerManager.Exception(err);

                ret = EventCodeEnum.AIRBLOW_CLEANING_ERROR;
                Module.InnerStateTransition(new AirBlowChuckCleaningErrorState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowState.GetState()}");
            }

            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override EnumChuckAirBlowCleaningState GetState()
        {
            return EnumChuckAirBlowCleaningState.DONE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new AirBlowChuckCleaningPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class AirBlowChuckCleaningAbortState : ChuckAirBlowCleaningStateBase
    {
        public AirBlowChuckCleaningAbortState(AirBlowChuckCleaningModule abmodule) : base(abmodule) { }

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
                ret = Module.InnerStateTransition(new AirBlowChuckCleaningIdleState(Module));
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

        public override EnumChuckAirBlowCleaningState GetState()
        {
            return EnumChuckAirBlowCleaningState.ABORT;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
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
    public class AirBlowChuckCleaningErrorState : ChuckAirBlowCleaningStateBase
    {
        public AirBlowChuckCleaningErrorState(AirBlowChuckCleaningModule abmodule) : base(abmodule) { }

        public override EventCodeEnum Execute()
        {
            //LoggerManager.Error($"{0} Error ", GetType().Name);
            return EventCodeEnum.AIRBLOW_CLEANING_ERROR;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override EnumChuckAirBlowCleaningState GetState()
        {
            return EnumChuckAirBlowCleaningState.ERROR;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

}

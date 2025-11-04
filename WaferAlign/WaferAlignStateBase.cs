using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WaferAlign
{
    using ProberInterfaces.Align;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberErrorCode;
    using ProberInterfaces.State;
    using LogModule;
    using ProberInterfaces.Param;
    using NotifyEventModule;
    using ProberInterfaces.Event;
    using System.Threading;

    public abstract class WaferAlignState : IFactoryModule, IInnerState
    {
        public abstract EventCodeEnum Execute();
        public abstract WaferAlignInnerStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();
        public abstract bool CanExecute(IProbeCommandToken token);
        public abstract EventCodeEnum Pause();
        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.UNDEFINED;
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

    public abstract class WaferAlignStateBase : WaferAlignState
    {
        private WaferAligner _Module;

        public WaferAligner Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public WaferAlignStateBase(WaferAligner module)
        {
            Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new WaferAlignIdleState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            var isInjected = false;
            return isInjected;
        }

        public void ChangeRunningState()
        {
            try
            {
                Module.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                Module.InnerStateTransition(new WaferAlignRunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CheckCommands()
        {
            try
            {
                bool consumed = false;

                Func<bool> conditionFunc = () =>
                {
                    if (this.StageSupervisor().Get_TCW_Mode() == TCW_Mode.OFF)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                };
                Action doAction = () => { ChangeRunningState(); };
                Action abortAction = () => { };

                if (this.GetModuleState() == ModuleStateEnum.IDLE || this.GetModuleState() == ModuleStateEnum.PAUSED)
                {
                    //consumed = this.CommandManager().ProcessIfRequested<IDoManualWaferAlign>(Module, conditionFunc, doAction, abortAction);
                }

                consumed = this.CommandManager().ProcessIfRequested<IDOWAFERALIGN>(Module, conditionFunc, doAction, abortAction);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class WaferAlignIdleState : WaferAlignStateBase
    {
        public WaferAlignIdleState(WaferAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.Template != null)
                {
                    List<ISubModule> modules = Module.Template.GetProcessingModule();

                    if (modules != null)
                    {
                        if (Module.Template.SchedulingModule.IsExecute())
                        {
                            bool isExecute = false;

                            foreach (var subModule in modules)
                            {
                                isExecute = subModule.IsExecute();
                            }

                            if (isExecute)
                            {
                                ChangeRunningState();
                            }
                        }
                    }
                }

                CheckCommands();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override WaferAlignInnerStateEnum GetState()
        {
            return WaferAlignInnerStateEnum.IDLE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;

            isInjected = Module.CommandRecvSlot.IsNoCommand() &&
                            token is IDOWAFERALIGN ||
                            token is IDoManualWaferAlign;

            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new WaferAlignPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }

    public class WaferAlignRunningState : WaferAlignStateBase
    {
        public WaferAlignRunningState(WaferAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSING ||
                    this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT)
                {
                    Module.InnerStateTransition(new WaferAlignIdleState(Module));
                }
                else
                {
                    Module.VisionManager().ClearGrabberUserImage(EnumProberCam.WAFER_LOW_CAM);
                    Module.LotOPModule().VisionScreenToLotScreen();

                    int foupnum = Module.GetParam_Wafer().GetOriginFoupNumber();
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    Module.EventManager().RaisingEvent(typeof(WaferAlignStartEvent).FullName, new ProbeEventArgs(this, semaphore, new PIVInfo() { FoupNumber = foupnum }));
                    semaphore.Wait();

                    retVal = Module.DoWaferAlignProcess();

                    if (EventCodeEnum.NONE != retVal)
                    {
                        Module.WaferAlignFailProcForStatusSoaking();
                    }

                    Module.LotOPModule().MapScreenToLotScreen();
                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString());
            }

            return retVal;
        }

        public override WaferAlignInnerStateEnum GetState()
        {
            return WaferAlignInnerStateEnum.ALIGN;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.UNDEFINED;
        }
    }

    public class WaferAlignSetupState : WaferAlignStateBase
    {
        public WaferAlignSetupState(WaferAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING ||
                    Module.CommandRecvSlot.IsRequested<IDOWAFERALIGN>())
                {
                    Module.InnerStateTransition(new WaferAlignIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override WaferAlignInnerStateEnum GetState()
        {
            return WaferAlignInnerStateEnum.SETUP;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;

            isInjected = Module.CommandRecvSlot.IsNoCommand() && token is IDOWAFERALIGN;

            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class WaferAlignRecoveryState : WaferAlignStateBase
    {
        public WaferAlignRecoveryState(WaferAligner module, EventCodeInfo eventcode) : base(module)
        {
            Module.CompleteManualOperation(eventcode.EventCode);

            LoggerManager.ActionLog(ModuleLogType.WAFER_ALIGN, StateLogType.ERROR, $"{eventcode.EventCode}", this.Module.LoaderController().GetChuckIndex());
            this.NotifyManager().Notify(EventCodeEnum.WAFER_ALING_FAIL);
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED ||
                    Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSING)
                {
                    List<ISubModule> modules = Module.Template.GetProcessingModule();

                    for (int index = 0; index < modules.Count; index++)
                    {
                        ISubModule module = modules[index];

                        if (module.GetState() == SubModuleStateEnum.RECOVERY)
                        {
                            retVal = module.Recovery();
                            Module.SetIsModify(true);

                            break;
                        }
                    }

                    Module.InnerStateTransition(new WaferAlignIdleState(Module));
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RECOVERY;
        }

        public override WaferAlignInnerStateEnum GetState()
        {
            return WaferAlignInnerStateEnum.RECOVERY;
        }
        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                Module.InnerStateTransition(new WaferAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class WaferAlignPausedState : WaferAlignStateBase
    {
        public WaferAlignPausedState(WaferAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                CheckCommands();
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

        public override WaferAlignInnerStateEnum GetState()
        {
            return WaferAlignInnerStateEnum.PAUSED;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected;

            isInjected = Module.CommandRecvSlot.IsNoCommand() && token is IDoManualWaferAlign;

            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new WaferAlignAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }

    public class WaferAlignSuspendedState : WaferAlignStateBase
    {
        public WaferAlignSuspendedState(WaferAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (Module.TempController().IsCurTempWithinSetTempRange())
                {
                    retVal = Module.InnerStateTransition(Module.PreInnerState);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override WaferAlignInnerStateEnum GetState()
        {
            return WaferAlignInnerStateEnum.SUSPENDED;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new WaferAlignPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }

    public class WaferAlignDoneState : WaferAlignStateBase
    {
        public WaferAlignDoneState(WaferAligner module) : base(module)
        {
            Module.CompleteManualOperation(EventCodeEnum.NONE);

            int foupnum = Module.GetParam_Wafer().GetOriginFoupNumber();

            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            Module.EventManager().RaisingEvent(typeof(WaferAlignEndEvent).FullName, new ProbeEventArgs(this, semaphore, new PIVInfo() { FoupNumber = foupnum }));
            semaphore.Wait();

            LoggerManager.ActionLog(ModuleLogType.WAFER_ALIGN, StateLogType.DONE,
                $"Wafer Center X: {Module.GetParam_Wafer().GetSubsInfo().WaferCenter.GetX()}, " +
                $"Wafer Center Y: {Module.GetParam_Wafer().GetSubsInfo().WaferCenter.GetY()}, " +
                $"Wafer Angle: {Module.WaferAlignInfo.AlignAngle}",
                this.Module.LoaderController().GetChuckIndex());
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.GetModuleState() != ModuleStateEnum.DONE)
                {
                    Module.ModuleState.StateTransition(this.GetModuleState());
                }

                Module.ClearState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override WaferAlignInnerStateEnum GetState()
        {
            return WaferAlignInnerStateEnum.DONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new WaferAlignPausedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }

    public class WaferAlignAbortState : WaferAlignStateBase
    {
        public WaferAlignAbortState(WaferAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.InnerStateTransition(new WaferAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override WaferAlignInnerStateEnum GetState()
        {
            return WaferAlignInnerStateEnum.ABORT;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.UNDEFINED;
        }
    }

    public class WaferAlignErrorState : WaferAlignStateBase
    {
        public WaferAlignErrorState(WaferAligner module, EventCodeInfo eventcode) : base(module)
        {
            Module.CompleteManualOperation(eventcode.EventCode);

            if (this.GetModuleState() == ModuleStateEnum.ERROR)
            {
                Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
            }
            else
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Current State = {this.GetModuleState()}, Can not add ReasonOfError.");
            }

            this.NotifyManager().Notify(EventCodeEnum.WAFER_ALING_FAIL);
            LoggerManager.ActionLog(ModuleLogType.WAFER_ALIGN, StateLogType.ERROR, $"{eventcode.EventCode}", this.Module.LoaderController().GetChuckIndex());
        }

        public override EventCodeEnum Execute()
        {
            try
            {
                if (Module.LotOPModule().InnerState.GetModuleState() != ModuleStateEnum.RUNNING)
                {
                    Module.InnerStateTransition(new WaferAlignIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override WaferAlignInnerStateEnum GetState()
        {
            return WaferAlignInnerStateEnum.ERROR;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
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
                Module.InnerStateTransition(new WaferAlignIdleState(Module));
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
                Module.InnerStateTransition(new WaferAlignIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }
}

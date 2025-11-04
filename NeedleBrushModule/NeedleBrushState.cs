using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.NeedleBrush;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeedleBrushModule
{
    public abstract class NeedleBrushState : IInnerState
    {

        public abstract bool CanExecute(IProbeCommandToken token);
        public abstract EventCodeEnum Execute();

        public abstract EventCodeEnum Pause();

        public abstract NeedleBrushStateEnum GetState();

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
    public abstract class NeedleBrushStateBase : NeedleBrushState
    {
        private NeedleBrush _Module;

        public NeedleBrush Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public NeedleBrushStateBase(NeedleBrush module)
        {
            Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new NeedleBrushIdleState(Module));
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
    }

    public class NeedleBrushIdleState : NeedleBrushStateBase
    {
        public NeedleBrushIdleState(NeedleBrush module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                List<ISubModule> modules = Module.Template.GetProcessingModule();

                if (Module.Template.SchedulingModule.IsExecute())
                {
                    foreach (var subModule in modules)
                    {
                        if (subModule.IsExecute())
                        {
                            Module.InnerStateTransition(new NeedleBrushRunningState(Module));
                            break;
                        }
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

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override NeedleBrushStateEnum GetState()
        {
            return NeedleBrushStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                retVal = Module.InnerStateTransition(new NeedleBrushPauseState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }

    public class NeedleBrushRunningState : NeedleBrushStateBase
    {
        public NeedleBrushRunningState(NeedleBrush module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                bool doneFlag = false;

                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new NeedleBrushDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }

                List<ISubModule> modules = Module.Template.GetProcessingModule();

                foreach (var subModule in modules)
                {
                    retVal = subModule.ClearData();
                    retVal = subModule.Execute();

                    if (subModule.GetState() == SubModuleStateEnum.ERROR)
                    {
                        if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                        {
                            Module.InnerStateTransition(new NeedleBrushDoneState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                            return EventCodeEnum.NONE;
                        }
                        else
                        {
                            Module.InnerStateTransition(new NeedleBrushErrorState(Module));
                        }
                        break;
                    }
                    else if (subModule.GetState() == SubModuleStateEnum.RECOVERY)
                    {
                        if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                        {
                            Module.InnerStateTransition(new NeedleBrushDoneState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                            return EventCodeEnum.NONE;
                        }

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
                    Module.InnerStateTransition(new NeedleBrushDoneState(Module));
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
            return ModuleStateEnum.RUNNING;
        }

        public override NeedleBrushStateEnum GetState()
        {
            return NeedleBrushStateEnum.RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class NeedleBrushSuspendState : NeedleBrushStateBase
    {
        public NeedleBrushSuspendState(NeedleBrush module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.CommandSendSlot.GetState() == CommandStateEnum.DONE)
                {
                    Module.InnerStateTransition(new NeedleBrushRunningState(Module));
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

        public override NeedleBrushStateEnum GetState()
        {
            return NeedleBrushStateEnum.SUSPENDED;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class NeedleBrushAbortState : NeedleBrushStateBase
    {
        public NeedleBrushAbortState(NeedleBrush module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new NeedleBrushIdleState(Module));
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

        public override NeedleBrushStateEnum GetState()
        {
            return NeedleBrushStateEnum.ABORT;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class NeedleBrushPauseState : NeedleBrushStateBase
    {
        public NeedleBrushPauseState(NeedleBrush module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override NeedleBrushStateEnum GetState()
        {
            return NeedleBrushStateEnum.PAUSED;
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
                retVal = Module.InnerStateTransition(new NeedleBrushAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class NeedleBrushDoneState : NeedleBrushStateBase
    {
        public NeedleBrushDoneState(NeedleBrush module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
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

        public override NeedleBrushStateEnum GetState()
        {
            return NeedleBrushStateEnum.DONE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class NeedleBrushErrorState : NeedleBrushStateBase
    {
        public NeedleBrushErrorState(NeedleBrush module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.NODATA;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override NeedleBrushStateEnum GetState()
        {
            return NeedleBrushStateEnum.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.InnerStateTransition(new NeedleBrushPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new NeedleBrushIdleState(Module));
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
                Module.InnerStateTransition(new NeedleBrushIdleState(Module));
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

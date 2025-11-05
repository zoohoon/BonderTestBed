using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.State;
using System;
using System.Threading.Tasks;

namespace AirBlowModule
{
    public enum EnumAirBlowTempControlState
    {
        UNDEFINED = -1,
        IDLE,
        RUN,
        DONE,
        SUSPENDED,
        PAUSE,
        ERROR,
        ABORT,
    }
    public abstract class AirBlowTempControlStateBase : IInnerState
    {
        protected AirBlowTempControlModule Module;
        public abstract EventCodeEnum Execute();
        public abstract EnumAirBlowTempControlState GetState();
        public abstract ModuleStateEnum GetModuleState();
        public abstract EventCodeEnum Pause();

        protected static Random rnd = new Random();
        protected static DateTime EndTime;
        public AirBlowTempControlStateBase(AirBlowTempControlModule abtempcontrolmodule)
        {
            this.Module = abtempcontrolmodule;
        }

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class AirBlowTempControlIdleState : AirBlowTempControlStateBase
    {
        public AirBlowTempControlIdleState(AirBlowTempControlModule tcmodule) : base(tcmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                //이벤트가 들어오기전까지 그냥 아이들 
                ret = EventCodeEnum.NONE;

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
            return ModuleStateEnum.IDLE;
        }

        public override EnumAirBlowTempControlState GetState()
        {
            return EnumAirBlowTempControlState.IDLE;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

    }
    public class AirBlowTempControlRunningState : AirBlowTempControlStateBase
    {
        public AirBlowTempControlRunningState(AirBlowTempControlModule tcmodule) : base(tcmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new AirBlowTempControlDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowTempControlState.GetState()}");
                    return EventCodeEnum.NONE;
                }

                var task = Task.Run(() =>
               {
                   ret = Module.ChuckTempControl();
               });

                //Module.WaitCancelDialogService().ShowDialog("Wait");
                Module.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                task.Wait();

                if (ret == EventCodeEnum.NONE)
                {
                    System.Threading.Thread.Sleep(2000);
                    //Module.WaitCancelDialogService().CloseDialog();
                    Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    Module.InnerStateTransition(new AirBlowTempControlDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowTempControlState.GetState()}");
                }
                else
                {
                    if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                    {
                        Module.InnerStateTransition(new AirBlowTempControlDoneState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowTempControlState.GetState()}");
                        return EventCodeEnum.NONE;
                    }

                    System.Threading.Thread.Sleep(2000);
                    Module.InnerStateTransition(new AirBlowTempControlErrorState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowTempControlState.GetState()}");
                    ret = EventCodeEnum.AIRBLOW_TEMPERATURE_CONTROL_ERROR;
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

        public override EnumAirBlowTempControlState GetState()
        {
            return EnumAirBlowTempControlState.RUN;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AirBlowTempControlDoneState : AirBlowTempControlStateBase
    {
        public AirBlowTempControlDoneState(AirBlowTempControlModule tcmodule) : base(tcmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                Module.InnerStateTransition(new AirBlowTempControlIdleState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.AirBlowTempControlState.GetState()}");
                ret = EventCodeEnum.NONE;

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
            return ModuleStateEnum.DONE;
        }

        public override EnumAirBlowTempControlState GetState()
        {
            return EnumAirBlowTempControlState.DONE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class AirBlowTempControlAbortState : AirBlowTempControlStateBase
    {
        public AirBlowTempControlAbortState(AirBlowTempControlModule abmodule) : base(abmodule) { }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new AirBlowTempControlIdleState(Module));
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

        public override EnumAirBlowTempControlState GetState()
        {
            return EnumAirBlowTempControlState.ABORT;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }


    public class AirBlowTempControlSuspendedState : AirBlowTempControlStateBase
    {
        public AirBlowTempControlSuspendedState(AirBlowTempControlModule tcmodule) : base(tcmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                // Old 
                //IModule tmp = (IModule)this.TempControlModule.InjectionTarget.obj;

                //if (tmp != null)
                //{
                //    bool alreadyExists = tmp.Requestors.Any(x => x.HashCode == this.TempControlModule.InjectionTarget.HashCode);
                //    if (alreadyExists == true)
                //    {
                //        TempControlModule.AirBlowTempControlStateTransition(new AirBlowTempControlRunningState(TempControlModule));
                //        ret = EventCodeEnum.NONE;
                //    }
                //}

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
            return ModuleStateEnum.SUSPENDED;
        }

        public override EnumAirBlowTempControlState GetState()
        {
            return EnumAirBlowTempControlState.SUSPENDED;
        }

        public override EventCodeEnum Pause()
        {
            return Module.InnerStateTransition(new AirBlowTempControlPauseState(Module));
        }
    }

    public class AirBlowTempControlPauseState : AirBlowTempControlStateBase
    {
        public AirBlowTempControlPauseState(AirBlowTempControlModule tcmodule) : base(tcmodule) { }
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

        public override EnumAirBlowTempControlState GetState()
        {
            return EnumAirBlowTempControlState.PAUSE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new AirBlowTempControlAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class AirBlowTempControlErrorState : AirBlowTempControlStateBase
    {
        public AirBlowTempControlErrorState(AirBlowTempControlModule tcmodule) : base(tcmodule) { }
        public override EventCodeEnum Execute()
        {
            LoggerManager.Error($"{GetType().Name} Error ");
            return EventCodeEnum.AIRBLOW_TEMPERATURE_CONTROL_ERROR;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override EnumAirBlowTempControlState GetState()
        {
            return EnumAirBlowTempControlState.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
}

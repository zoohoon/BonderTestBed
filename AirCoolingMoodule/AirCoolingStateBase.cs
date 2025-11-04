using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.State;
using System;

namespace AirCoolingMoodule
{
    public enum EnumAirCoolingState
    {
        UNDEFINED = -1,
        IDLE,
        RUN,
        DONE,
        PAUSE,
        SUSPENDED,
        ERROR,
    }
    public abstract class AirCoolingStateBase : IInnerState
    {
        protected AirCoolingModule AirCoolingModule;
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract EnumAirCoolingState GetState();
        public abstract ModuleStateEnum GetModuleState();

        public AirCoolingStateBase(AirCoolingModule aircoolingmodule)
        {
            this.AirCoolingModule = aircoolingmodule;
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
    public class AirCoolingIdleState : AirCoolingStateBase
    {
        public AirCoolingIdleState(AirCoolingModule acmodule) : base(acmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (AirCoolingModule.AirCoolingSysFile.AirCoolingOnProber.Value == true)
                {
                    if (AirCoolingModule.TempController().TempInfo.CurTemp.Value > AirCoolingModule.AirCoolingDevFile.TargetTemp.Value)
                    {
                        ret = EventCodeEnum.NONE;
                        AirCoolingModule.InnerStateTransition(new AirCoolingRunningState(AirCoolingModule));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={AirCoolingModule.AirCoolingState.GetState()}");
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    ret = EventCodeEnum.NONE;
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
            return ModuleStateEnum.IDLE;
        }

        public override EnumAirCoolingState GetState()
        {
            return EnumAirCoolingState.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

    }
    public class AirCoolingRunningState : AirCoolingStateBase
    {
        public AirCoolingRunningState(AirCoolingModule acmodule) : base(acmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                if (AirCoolingModule.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    AirCoolingModule.InnerStateTransition(new AirCoolingDoneState(AirCoolingModule));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={AirCoolingModule.AirCoolingState.GetState()}");
                    return EventCodeEnum.NONE;
                }

                ret = AirCoolingModule.AirCoolingFunc();
                if (ret == EventCodeEnum.NONE)
                {
                    ret = EventCodeEnum.NONE;
                    AirCoolingModule.InnerStateTransition(new AirCoolingDoneState(AirCoolingModule));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={AirCoolingModule.AirCoolingState.GetState()}");
                }
                else
                {
                    if (AirCoolingModule.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                    {
                        AirCoolingModule.InnerStateTransition(new AirCoolingDoneState(AirCoolingModule));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={AirCoolingModule.AirCoolingState.GetState()}");
                        return EventCodeEnum.NONE;
                    }

                    ret = EventCodeEnum.AIRCOOLING_ERROR;
                    AirCoolingModule.InnerStateTransition(new AirCoolingErrorState(AirCoolingModule));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={AirCoolingModule.AirCoolingState.GetState()}");
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

        public override EnumAirCoolingState GetState()
        {
            return EnumAirCoolingState.RUN;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AirCoolingDoneState : AirCoolingStateBase
    {
        public AirCoolingDoneState(AirCoolingModule acmodule) : base(acmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                if (AirCoolingModule.AirCoolingSysFile.AirCoolingOnProber.Value == true)
                {
                    if (AirCoolingModule.TempController().TempInfo.CurTemp.Value > AirCoolingModule.AirCoolingDevFile.TargetTemp.Value)
                    {
                        ret = EventCodeEnum.NONE;
                        AirCoolingModule.InnerStateTransition(new AirCoolingIdleState(AirCoolingModule));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={AirCoolingModule.AirCoolingState.GetState()}");
                    }
                    else
                    {
                        //상태 유지
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    //상태 유지
                    ret = EventCodeEnum.NONE;
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
            return ModuleStateEnum.DONE;
        }

        public override EnumAirCoolingState GetState()
        {
            return EnumAirCoolingState.DONE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class AirCoolingErrorState : AirCoolingStateBase
    {
        public AirCoolingErrorState(AirCoolingModule acmodule) : base(acmodule) { }
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
            return ModuleStateEnum.ERROR;
        }

        public override EnumAirCoolingState GetState()
        {
            return EnumAirCoolingState.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class AirCoolingPauseState : AirCoolingStateBase
    {
        public AirCoolingPauseState(AirCoolingModule acmodule) : base(acmodule) { }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

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
            return ModuleStateEnum.PAUSED;
        }

        public override EnumAirCoolingState GetState()
        {
            return EnumAirCoolingState.PAUSE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class AirCoolingSuspendedState : AirCoolingStateBase
    {
        public AirCoolingSuspendedState(AirCoolingModule acmodule) : base(acmodule) { }
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
            return ModuleStateEnum.SUSPENDED;
        }

        public override EnumAirCoolingState GetState()
        {
            return EnumAirCoolingState.SUSPENDED;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
}

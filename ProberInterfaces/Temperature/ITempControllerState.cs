using ProberErrorCode;
using ProberInterfaces.Command;
using ProberInterfaces.State;

namespace ProberInterfaces.Temperature
{
    public abstract class AutoCoolingProcState : IInnerState
    {
        public abstract ModuleStateEnum GetModuleState();
        public abstract EventCodeEnum Execute();
        public abstract bool CanExecute(IProbeCommandToken token);

        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum ClearState()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum Pause()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.UNDEFINED;
        }

    }
}

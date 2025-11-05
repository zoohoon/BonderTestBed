using ProberErrorCode;
using ProberInterfaces.CardChange;
using ProberInterfaces.State;
using System.Threading.Tasks;

namespace ProberInterfaces.SequenceRunner
{
    public abstract class SequenceRunnerState : IInnerState
    {
        public bool frontDoorTempEventFlag;
        public bool frontDoorTempStopEventFlag;
        public abstract EventCodeEnum Execute();
        public abstract Task<EventCodeEnum> TaskExecute();
        public abstract ModuleStateEnum GetModuleState();
        public abstract SequenceRunnerStateEnum GetSequenceRunnerStateEnum();

        public virtual EventCodeEnum RunCardChange(bool IsTempSetBeforeOperation)
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum RunTestHeadDockUndock(THDockType type, bool IsTempSetBeforeOperation)
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum RunNCPadChange(bool IsTempSetBeforeOperation)
        {
            return EventCodeEnum.UNDEFINED;
        }

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

        public virtual EventCodeEnum RunRetry()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum RunReverse()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum RunManualRetry()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum RunManualReverse()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum AlternateManualMode()
        {
            return EventCodeEnum.UNDEFINED;
        }
    }
}

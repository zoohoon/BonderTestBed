using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.State;
using System;

namespace CardChangeSupervisor.CardChangeSupervisorState.InnerState
{
    //public enum CardChangeSupervisorStateEnum
    //{
    //    IDLE,
    //    TRANSFER_READY,
    //    TRANSFER_START,
    //    HAND_OFF_AVAILABLE,
    //    TRANSFER_TO_STAGE,
    //    CLEAR,
    //    DONE,
    //    ABORT,
    //    ERROR,
    //    RECOVERY,

    //}

    public abstract class CardChangeInnerState : IInnerState
    {
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract ModuleStateEnum GetModuleState();
        public abstract EventCodeEnum End();
        public abstract EventCodeEnum Abort();
        public abstract EventCodeEnum ClearState();
        public abstract EventCodeEnum Resume();

    }

    public abstract class CardChangeInnerStateBase : IInnerState
    {
        public CardChangeSupervisor Module { get; set; }

        public CardChangeInnerStateBase(CardChangeSupervisor module)
        {
            this.Module = module;
        }

        public virtual bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }
        public abstract EventCodeEnum Execute();

        public abstract ModuleStateEnum GetModuleState();
        public abstract CardChangeStateEnum GetState();

        /// <summary>
        /// 현재 Transfer 중인 ActiveCCInfo를 반환함. 유효하지 않은 State에서는 null을 반환함.
        /// </summary>
        /// <returns></returns>
        public abstract ActiveCCInfo GetRunningCCInfo();

        public virtual EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }

       
        public EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }

        public virtual EventCodeEnum Abort()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                
                Module.InnerStateTransition(new ABORT(Module));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
               
            }
            return retVal;
        }

        public EventCodeEnum ClearState()
        {
            // 구현 해야함.
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = Module.InnerStateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

    }
}

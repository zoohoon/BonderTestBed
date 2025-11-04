using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;

namespace CardChange
{
    public abstract class GPCardChangeStateBase: IGPCardChangeState
    {
        private CardChangeModule _Module;

        public CardChangeModule Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }
        public GPCardChangeStateBase(CardChangeModule module)
        {
            Module = module;
        }
        
        public abstract CardChangeModuleStateEnum GetState();

        public abstract ModuleStateEnum GetModuleState();

        public EventCodeEnum Execute()
        {
            return EventCodeEnum.UNDEFINED;
        }
        public abstract EventCodeEnum Pause();

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

    public class GPCardChangeIdleState : GPCardChangeStateBase
    {
        public GPCardChangeIdleState(CardChangeModule module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.IDLE;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class GPCardChangeRunningState : GPCardChangeStateBase
    {
        public GPCardChangeRunningState(CardChangeModule module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override CardChangeModuleStateEnum GetState()
        {
            return CardChangeModuleStateEnum.RUNNING;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

}

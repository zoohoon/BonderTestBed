using ProberErrorCode;

namespace ProberInterfaces.Align
{
    public enum WaferAlignInnerStateEnum
    {
        IDLE = 0,
        SUSPENDED,
        ALIGN,
        READY,
        ERROR,
        SETUP,
        RECOVERY,
        RECOVERING,
        FAILED,
        PAUSED,
        DONE,
        ABORT
    }

    
    public enum PinAlignInnerStateEnum
    {
        IDLE = 0,
        SUSPENDED,
        ALIGN,
        READY,
        ERROR,
        SETUP,
        RECOVERY,
        FAILED,
        PAUSED,
        DONE,
        ABORT
    }

    public enum CalcStateEnum
    {
        IDLE = 0,
        SUSPENDED,
        CALC,
        DONE,
        ERROR
    }
    public enum PosStateEnum
    {
        IDLE = 0,
        SUSPENDED,
        POSITIONING,
        DONE,
        ERROR
    }
    //public abstract class AlignState : IFactoryModule, IInnerState
    //{
    //    public abstract EventCodeEnum Execute();
    //    //public abstract AlignStateEnum GetState();
    //    public abstract ModuleStateEnum GetModuleState();
    //    public abstract bool CanExecute(IProbeCommandToken token);
    //    //public abstract EventCodeEnum Perform();

    //    public abstract EventCodeEnum Pause();

    //    public virtual EventCodeEnum End()
    //    {
    //        return EventCodeEnum.UNDEFINED;
    //    }
    //    public virtual EventCodeEnum Abort()
    //    {
    //        return EventCodeEnum.NONE;
    //    }
    //    public virtual EventCodeEnum ClearState()
    //    {
    //        return EventCodeEnum.NONE;
    //    }
    //    public virtual EventCodeEnum Resume()
    //    {
    //        return EventCodeEnum.NONE;
    //    }
    //}

    public abstract class CalculateState
    {
        public abstract EventCodeEnum Execute();
        public abstract CalcStateEnum GetState();
    }
    public abstract class PositioningState
    {
        public abstract EventCodeEnum Execute();
        public abstract PosStateEnum GetState();
    }
}

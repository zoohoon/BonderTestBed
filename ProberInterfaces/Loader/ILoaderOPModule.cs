using System.Collections.Generic;

namespace ProberInterfaces
{

    using ProberErrorCode;
    using ProberInterfaces.Command;
    using ProberInterfaces.State;


    public enum GPCellModeEnum
    {
        OFFLINE,
        ONLINE,
        MAINTENANCE,
        DISCONNECT
    }

    public enum StageLockMode
    {
        UNLOCK,
        LOCK,
        RESERVE_LOCK
    }
    public enum TCW_Mode
    {
        OFF,
        ON
    }
    public enum ReasonOfStageMoveLock
    {
        STAGE_BACKSIDEDOOR_OPEN,
        MANUAL
    }


    public enum GPSummaryLayoutModeEnum
    {
        SETTING,
        CELLS,
        BOTH,
    }

    public interface ILoaderOPModule : IStateModule
    {
        List<IStateModule> RunList { get; set; }
    }

    public abstract class LoaderOPState : IInnerState
    {
        public abstract bool CanExecute(IProbeCommandToken token);
        public abstract EventCodeEnum Resume();
        public abstract EventCodeEnum Pause();
        public abstract EventCodeEnum Execute();
        public abstract LoaderOPStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.UNDEFINED;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public abstract EventCodeEnum ClearState();


    }

    public enum LoaderOPStateEnum
    {
        IDLE = 0,
        RUNNING,
        PAUSED,
        ERROR,
        ABORTED,
        DONE,
    }

    public enum ManualZUPStateEnum
    {
        UNDEFINED = 0,
        NONE,
        Z_DOWN,
        Z_UP
    }
    public enum ManualZUPEnableEnum
    {
        Disable = 0,
        Enable,
    }
}

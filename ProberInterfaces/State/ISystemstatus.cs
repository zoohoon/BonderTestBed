using ProberErrorCode;

namespace ProberInterfaces.State
{
    public interface ISystemstatus
    {
        SystemStatus State { get; }
        EnumSysState GetSysState();
        EventCodeEnum SetSetUpState();
        //EventCodeEnum SetSetUpDoneState();
        EventCodeEnum SetIdleState();
        EventCodeEnum SetErrorState();
        EventCodeEnum SetLotState();
        
        EventCodeEnum StateTransition(EnumSysState state);
    }
    public enum EnumSysState
    {
        IDLE = 0,
        SETUP,
        LOT,
        ERROR,
        SETUPDONE
    }
    public abstract class SystemStatus
    {
        public abstract EnumSysState GetSysState();

        public abstract EventCodeEnum SetErrorState();

        public abstract EventCodeEnum SetIdleState();

        public abstract EventCodeEnum SetLotState();

        public abstract EventCodeEnum SetSetUpState();
        public abstract EventCodeEnum SetSetUpDoneState();
    }
}

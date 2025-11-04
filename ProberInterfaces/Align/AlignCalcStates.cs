using ProberErrorCode;

namespace ProberInterfaces.Align
{
    public enum AlignCalcStateEnum
    {
        IDLE = 0,
        CALCUATING,
        DONE,
        ERROR,
        REJECTED,
    }
    public abstract class AlignCalcStateBase
    {
        public abstract AlignCalcStateEnum GetState();
        public abstract EventCodeEnum Run();
    }

}

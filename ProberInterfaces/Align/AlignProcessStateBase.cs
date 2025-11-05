using ProberErrorCode;

namespace ProberInterfaces.Align
{
    using Enum;

    public abstract class AlignProcessStateBase 
    {

        public AlignProcessBase Module
        {
            get;
            private set;
        }

        public abstract AlignProcStateEnum GetState();
        public abstract EventCodeEnum Run();
       // public abstract EventCodeEnum Retry();
        public AlignProcessStateBase(AlignProcessBase module)
        {
            this.Module = module;
        }
    }
}

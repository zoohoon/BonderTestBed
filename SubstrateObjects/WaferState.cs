using System;
using System.ComponentModel;

namespace SubstrateObjects
{
    using Newtonsoft.Json;
    using ProberInterfaces;

    [Serializable]
    public abstract class WaferStateBase : INotifyPropertyChanged, IWaferState
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private WaferObject _Wafer;

        public WaferObject Wafer
        {
            get { return _Wafer; }
            protected set
            {
                if (value != _Wafer)
                {
                    _Wafer = value;
                    NotifyPropertyChanged("Wafer");
                }
            }
        }
        public abstract EnumWaferState GetState();

        public virtual void SetProcessing()
        {
        }
        public virtual void SetTested()
        {
        }
        public virtual void SetProcessed()
        {
        }
        public virtual void SetSkipped()
        {
        }
        public virtual void SetUnprocessed()
        {
        }
        public virtual void SetCleaning()
        {
        }
        public virtual void SetReady()
        {
        }

        public virtual void SetSoakingSuspend()
        {
        }

        public virtual void SetSoakingDone()
        {
        }

        public WaferStateBase(WaferObject wafer)
        {
            Wafer = wafer;
        }
    }

    [Serializable]
    public class NullWaferState : WaferStateBase
    {
        public NullWaferState(WaferObject wafer) : base(wafer)
        {
            Wafer.SetAlignState(AlignStateEnum.IDLE);
            wafer.WaferState = EnumWaferState.UNDEFINED;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.UNDEFINED;
        }

        public override void SetUnprocessed()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
    }

    [Serializable]
    public class WaferUnprocessedState : WaferStateBase
    {
        public WaferUnprocessedState(WaferObject wafer) : base(wafer)
        {
            Wafer.SetAlignState(AlignStateEnum.IDLE);
            wafer.WaferState = EnumWaferState.UNPROCESSED;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.UNPROCESSED;
        }
        public override void SetUnprocessed()
        {
            Wafer.SetAlignState(AlignStateEnum.IDLE);
        }
        public override void SetProcessing()
        {
            Wafer.ChangeState(new WaferProbingState(Wafer));
        }
        public override void SetProcessed()
        {
            Wafer.ChangeState(new WaferProcessedState(Wafer));
        }
        public override void SetSkipped()
        {
            Wafer.ChangeState(new WaferSkippedState(Wafer));
        }

        public override void SetCleaning()
        {
            Wafer.ChangeState(new WaferCleaningState(Wafer));
        }
        public override void SetReady()
        {
            Wafer.ChangeState(new WaferReadyState(Wafer));
        }

        public override void SetSoakingSuspend()
        {
            Wafer.ChangeState(new WaferSoakingSuspendState(Wafer));
        }

        public override void SetSoakingDone()
        {
            Wafer.ChangeState(new WaferSoakingDoneState(Wafer));
        }
    }

    [Serializable]
    public class WaferProbingState : WaferStateBase
    {
        public WaferProbingState(WaferObject wafer) : base(wafer)
        {
            wafer.WaferState = EnumWaferState.PROBING;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.PROBING;
        }
        public override void SetProcessed()
        {
            Wafer.ChangeState(new WaferProcessedState(Wafer));
        }
        public override void SetSkipped()
        {
            Wafer.ChangeState(new WaferSkippedState(Wafer));
        }
        public override void SetUnprocessed()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
        public override void SetProcessing()
        {
        }
        public override void SetTested()
        {
            Wafer.ChangeState(new WaferTestedState(Wafer));
        }

    }
    [Serializable]
    public class WaferTestedState : WaferStateBase
    {
        public WaferTestedState(WaferObject wafer) : base(wafer)
        {
            wafer.WaferState = EnumWaferState.TESTED;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.TESTED;
        }
        public override void SetProcessed()
        {
            Wafer.ChangeState(new WaferProcessedState(Wafer));
        }
        public override void SetSkipped()
        {
            Wafer.ChangeState(new WaferSkippedState(Wafer));
        }
        public override void SetUnprocessed()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
        public override void SetProcessing()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
    }
    [Serializable]
    public class WaferProcessedState : WaferStateBase
    {
        public WaferProcessedState(WaferObject wafer) : base(wafer)
        {
            wafer.WaferState = EnumWaferState.PROCESSED;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.PROCESSED;
        }
        public override void SetSkipped()
        {
            Wafer.ChangeState(new WaferSkippedState(Wafer));
        }
        public override void SetProcessed()
        {
            Wafer.ChangeState(new WaferProcessedState(Wafer));
        }
        public override void SetUnprocessed()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
    }

    [Serializable]
    public class WaferSkippedState : WaferStateBase
    {
        public WaferSkippedState(WaferObject wafer) : base(wafer)
        {
            wafer.WaferState = EnumWaferState.SKIPPED;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.SKIPPED;
        }
        public override void SetProcessing()
        {
            Wafer.ChangeState(new WaferProbingState(Wafer));
        }
        public override void SetProcessed()
        {
            Wafer.ChangeState(new WaferProcessedState(Wafer));
        }
        public override void SetUnprocessed()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
    }

    [Serializable]
    public class WaferCleaningState : WaferStateBase
    {
        public WaferCleaningState(WaferObject wafer) : base(wafer)
        {
            wafer.WaferState = EnumWaferState.CLEANING;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.CLEANING;
        }
        public override void SetUnprocessed()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
        public override void SetReady()
        {
            Wafer.ChangeState(new WaferReadyState(Wafer));
        }
    }

    [Serializable]
    public class WaferReadyState : WaferStateBase
    {
        public WaferReadyState(WaferObject wafer) : base(wafer)
        {
            wafer.WaferState = EnumWaferState.READY;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.READY;
        }
        public override void SetProcessing()
        {
            Wafer.ChangeState(new WaferProbingState(Wafer));
        }
        public override void SetProcessed()
        {
            Wafer.ChangeState(new WaferProcessedState(Wafer));
        }
        public override void SetUnprocessed()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
        public override void SetCleaning()
        {
            Wafer.ChangeState(new WaferCleaningState(Wafer));
        }
        public override void SetSoakingSuspend()
        {
            Wafer.ChangeState(new WaferSoakingSuspendState(Wafer));
        }
        public override void SetSoakingDone()
        {
            Wafer.ChangeState(new WaferSoakingDoneState(Wafer));
        }
    }

    [Serializable]
    public class WaferSoakingSuspendState : WaferStateBase
    {
        public WaferSoakingSuspendState(WaferObject wafer) : base(wafer)
        {
            wafer.WaferState = EnumWaferState.SOAKINGSUSPEND;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.SOAKINGSUSPEND;
        }
        public override void SetProcessing()
        {
            Wafer.ChangeState(new WaferProbingState(Wafer));
        }
        public override void SetProcessed()
        {
            Wafer.ChangeState(new WaferProcessedState(Wafer));
        }
        public override void SetUnprocessed()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
        public override void SetCleaning()
        {
            Wafer.ChangeState(new WaferCleaningState(Wafer));
        }
        public override void SetReady()
        {
            Wafer.ChangeState(new WaferReadyState(Wafer));
        }
    }

    [Serializable]
    public class WaferSoakingDoneState : WaferStateBase
    {
        public WaferSoakingDoneState(WaferObject wafer) : base(wafer)
        {
            wafer.WaferState = EnumWaferState.SOAKINGDONE;
        }

        public override EnumWaferState GetState()
        {
            return EnumWaferState.SOAKINGDONE;
        }
        public override void SetProcessing()
        {
            Wafer.ChangeState(new WaferProbingState(Wafer));
        }
        public override void SetProcessed()
        {
            Wafer.ChangeState(new WaferProcessedState(Wafer));
        }
        public override void SetUnprocessed()
        {
            Wafer.ChangeState(new WaferUnprocessedState(Wafer));
        }
        public override void SetCleaning()
        {
            Wafer.ChangeState(new WaferCleaningState(Wafer));
        }
        public override void SetReady()
        {
            Wafer.ChangeState(new WaferReadyState(Wafer));
        }
    }
}

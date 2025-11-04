using ProberInterfaces.SequenceRunner;
using System;

namespace SequenceRunner
{
    [Serializable]
    public abstract class SequenceBehaviorState : ISequenceBehaviorState
    {
        public SequenceBehaviorState() { }
        public abstract SequenceBehaviorStateEnum GetState();
    }

    [Serializable]
    public class SequenceBehaviorIdleState : SequenceBehaviorState
    {
        public SequenceBehaviorIdleState() { }

        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.IDLE;
        }
    }

    [Serializable]
    public class SequenceBehaviorRunningState : SequenceBehaviorState
    {
        public SequenceBehaviorRunningState() { }

        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.RUNNING;
        }
    }

    [Serializable]
    public class SepenceBehaviorDoneState : SequenceBehaviorState
    {
        public SepenceBehaviorDoneState() { }

        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.DONE;
        }
    }

    [Serializable]
    public class SequenceBehaviorClearState : SequenceBehaviorState
    {
        public SequenceBehaviorClearState() { }

        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.CLEAR;
        }
    }

    [Serializable]
    public class SequenceBehaviorErrorState : SequenceBehaviorState
    {
        public SequenceBehaviorErrorState() { }
        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.ERROR;
        }
    }

    [Serializable]
    public class SequenceSafetyPreValidState : SequenceBehaviorState
    {
        public SequenceSafetyPreValidState() { }
        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.PRE_SAFETY_VALID;
        }
    }

    [Serializable]
    public class SequenceSafetyPreInvalidState : SequenceBehaviorState
    {
        public SequenceSafetyPreInvalidState() { }
        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.PRE_SAFETY_INVALID;
        }
    }

    [Serializable]
    public class SequenceSafetyPostValidState : SequenceBehaviorState
    {
        public SequenceSafetyPostValidState() { }
        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.POST_SAFETY_VALID;
        }
    }

    [Serializable]
    public class SequenceSafetyPostInvalidState : SequenceBehaviorState
    {
        public SequenceSafetyPostInvalidState() { }
        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.POST_SAFETY_INVALID;
        }
    }

    [Serializable]
    public class SequenceBehaviorPausedState : SequenceBehaviorState
    {
        public SequenceBehaviorPausedState() { }
        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.PAUSED;
        }
    }

    [Serializable]
    public class SequenceBehaviorAbortState : SequenceBehaviorState
    {
        public SequenceBehaviorAbortState() { }
        public override SequenceBehaviorStateEnum GetState()
        {
            return SequenceBehaviorStateEnum.ABORT;
        }
    }
}

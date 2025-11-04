namespace ProberInterfaces.SequenceRunner
{
    public enum SequenceRunnerWhatRunThing
    {
        CARD_CHANGE = 0x0000,
        TESTHEAD_DOCK_UNDOCK = 0x0010,
        NCPAD_CHANGE = 0x0020,
        NONE = 0x1111
    }

    public enum SequenceRunnerStateEnum
    {
        IDLE                    = 0x0001,

        CARD_CHANGE             = SequenceRunnerWhatRunThing.CARD_CHANGE            | 0x0002,
        TESTHEAD_DOCK_UNDOCK    = SequenceRunnerWhatRunThing.TESTHEAD_DOCK_UNDOCK   | 0x0002,
        NCPAD_CHANGE            = SequenceRunnerWhatRunThing.NCPAD_CHANGE           | 0x0002,

        PAUSING                 = 0x0003,
        PAUSED                  = 0x0013,
        ERROR                   = 0x0004,

        RECOVERY                = 0x0005,
        RETRY                   = 0x0015,
        REVERSE                 = 0x0025,
        MANUAL_WAITING          = 0x0105,
        MANUAL_RETRY            = 0x0115,
        MANUAL_REVERSE          = 0x0125,

        SUSPENDED               = 0x0006,
        REACH_TO_SET_TEMP       = 0x0016,

        ABORT                   = 0x0007,
        DONE                    = 0x0008,
    }

    public enum SequenceBehaviorStateEnum
    {
        IDLE = 0,
        RUNNING,
        DONE,
        ERROR,
        PRE_SAFETY_VALID,
        PRE_SAFETY_INVALID,
        POST_SAFETY_VALID,
        POST_SAFETY_INVALID,
        CLEAR,
        PAUSED,
        ABORT
    }

    public enum BehaviorFlag
    {
        NONE,
        TH_BEFORE,
        TH_INTERFACE,
        TH_AFTER,
        DOCK,
        UNDOCK
    }

}

namespace ProberInterfaces.CardChange
{
    public enum CardChangeModuleStateEnum
    {
        IDLE = 0,
        INIT = 1,
        RUNNING,
        SUSPENDED,
        CHECK,
        DONE,
        ERROR,
        END,
        RECOVERY_RETRY,
        RECOVERY_REVERSE,
        RECOVERY_MENUAL,
        RECOVERY_MENUAL_REVERSE,
        RECOVERY_MENUAL_RETRY,
        STEP_UP,
        REVERSE,
        PAUSE,
        ABORT,

    }

    public enum THDockType
    {
        TH_DOCK,
        TH_UNDOCK
    }
    public enum EnumCardChangeType // CCSeq change by CardChangeType 
    {
        INVALID = -1,
        UNDEFINED = 0,
        NONE = 1,
        DIRECT_CARD = 2,
        CARRIER = 3
    }

    public enum CardLoadConditionTypeEnum
    {
        CardIDStringCondition = 0,
        Gem,
        Tcpip,
        Gpib
    }
    public enum CardTestModeEnum
    {
        InternalValidation = 0,
        ExternalValidation
    }
    public enum CardPRESENCEStateEnum
    {
        UNDEFINED,
        CARD_ATTACH,// CARD + HOLDER + CARRIER in CardBuffer
        CARD_DETACH,// HOLDER + CARRIER in CardBuffer
        EMPTY       // CARRIER or Nothing in CardBuffer
    }

    public enum EnumCardDockType
    {
        INVALID = -1,
        NORMAL = 0,     // Equipped with POGO tower. Card will be attated to the POGO tower.
        DIRECTDOCK = 1  // DRAX system. Direct dock with tester.
    }
    public enum EnumPogoAlignPoint
    {
        INVALID = -1,
        POINT_4 = 0,
        POINT_3 = 1
    }

    public enum EnumCCAlignModule
    {
        CARD = 0,
        POGO = 1,
        POD = 2,
        CARRIER = 3
    }
}

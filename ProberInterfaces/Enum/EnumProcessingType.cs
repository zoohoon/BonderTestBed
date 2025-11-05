namespace ProberInterfaces.Enum
{
    public enum EnumProcessingResult
    {
        NONE = 0,
        SUCESS = NONE + 1,
        FAILED = NONE - 1
    }

    public enum EnumSetupProgressState
    {
        INVALID = -1,
        UNDEFINED = 0,
        SKIP,
        IDLE,
        CANCEL,
        DONE,
        PROGRESSING,
        PROCESSING,
    }
}

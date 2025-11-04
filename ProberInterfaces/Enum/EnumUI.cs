namespace ProberInterfaces
{
    public enum StreamingModeEnum
    {
        INVALID = -1,
        UNDEFINED = 0,
        STREAMING_ON = UNDEFINED + 1,
        STREAMING_OFF,
    }

    public enum StageAssignStateEnum
    {
        NOT_ASSIGN,
        ASSIGN,
        UNASSIGN
    }
}

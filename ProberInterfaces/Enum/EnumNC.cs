namespace ProberInterfaces.Enum
{
    public enum CleaningStateEnum
    {
        IDLE = 0,
        SUSPENDED,
        CLEANING,
        READY,
        ERROR
    }
    public enum CleaningPositiongStateEnum
    {
        IDLE = 0,
        POSITIONING,
        DONE,
        ERROR,
        REJECTED,
    }

    public enum CleaningProcStateEnum
    {
        IDLE = 0,
        RUNNING,
        DONE,
        FAILED,
        REJECTED,
    }
    public enum NCAlignProcAcqEnum
    {
        UNDEFINED = 0,
        WHCAMFOCUSING,
        TOUCHFOCUSING
    }
    public enum NCCleaningProcAcqEnum
    {
        UNDEFINED = 0,
        NCPAD1,
        NCPAD2,
        NCPAD3
    }

    public enum NeedleCleaingStateEnum
    {
        IDLE = 0,
        ALIGNED,
        DONE,
        CLEANING,
        CLEANED,
        ERROR
    }

    public enum NCTouchHeightMappingType
    {
        INVALID = -1,
        UNDEFINED = 0,
        PlanePoint_1 = 1,
        PlanePoint_3 = 3,
        PlanePoint_6 = 6,
        PlanePoint_7 = 7
    }

    public enum NCIntervalType
    {
        UNDEFINED = -1,
        DIEINTERVAL,
        WAFERINTERVAL,
        LOTINTERVAL
    }
    public enum FocusingState
    {
        UNDEFINED = -1,
        IDLE,
        RUNNING,
        DONE,
    }
    public enum CleaningState
    {
        UNDEFINED = -1,
        IDLE,
        RUNNING,
        DONE,
    }
    public enum EnumContactDirection
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        LEFTUP,
        RIGHTUP,
        LEFTDOWN,
        RIGHTDOWN,
    }
    public enum EnumCleaningDirectionType
    {
        VERTICAL,
        HORIZONTAL
    }
    public enum EnumNCPad
    {
        FIRST_NCPAD,
        SECOND_NCPAD,
        THIRD_NCPAD
    }
}

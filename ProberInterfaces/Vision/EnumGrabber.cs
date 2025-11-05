namespace ProberInterfaces.Vision
{
    public enum EnumGrabberRaft
    {
        UNDIFIND = -1,
        INVALID = UNDIFIND + 1,
        MILMORPHIS,
        MILGIGE,
        USB,
        TIS,
        EMULGRABBER,
        GIGE_EMULGRABBER,
        SIMUL_GRABBER
    }

    public enum EnumGrabberMode
    {
        UNDIFIND = -1,
        INVALID = UNDIFIND + 1,
        DEFAULT,
        AUTO,
        MANUAL
    }


    public enum EnumVisionProcRaft
    {
        UNDIFIND = -1,
        INVALID = UNDIFIND + 1,
        MIL,
        EMUL,
        OPENCV
    }
}

namespace ProberInterfaces.Enum
{
    public enum EnumUCScopeType
    {
        Processing = 0,
        Setup,
        Recovery
    }
    public enum AlignProcStateEnum
    {
        IDLE = 0,
        RUNNING,
        DONE,
        FAILED,
        RETRY,
        UPDATED,
        VALIDED,
        INVALIDED,
        REJECTED,
    }
    public enum SetupProcStateEnum
    {
        IDLE = 0,
        MODIFIY,
        RUNNING,
        DONE,
        ERROR
    }
    public enum WaferAlignProcTypeEnum
    {
        NOT_DEFINED = 0,
        PM_HM = 1,
        PM_LM,
        EDGE_PROC,
        PLANE_MEASURE,
        LL_REG,
        SINGULATION,

    }

    public enum NCPadAlignProcTypeEnum
    {
        NOT_DEFINED = 0,
        WH_FOCUSING = 1,
        TOUCH_FOCUSING
    }

    public enum PinLowAlignPatternOrderEnum
    {
        FIRST = 0,
        SECOND
    }
    public enum PatternStateEnum
    {
        READY = 0,
        PROCESSING,
        NOTREG,
        FAILED,
        MODIFY
    }
    public enum PinSetupMode
    {
        POSITION = 0,
        SIZE,
        BASEFOCUSINGAREA,
        TIPSEARCHAREA,
        TIPANDKEYSEARCHAREA,
        THRESHOLD,
        TIPBLOBMIN,
        TIPBLOBMAX,
        KEYBLOBMIN,
        KEYBLOBMAX,
        KEYFOCUSINGAREA
    }

    // DrawDieOverlay 함수 호출 시 추가로 화면 표시할 정보들을 나열한 Enum
    public enum DrawDieOverlayEnum
    {
        ALLDie = 0,
        CenterDie,
        Edge_Center,
        Align_Center,
        Centerofthecenterdie
    }

}

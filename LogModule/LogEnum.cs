namespace LogModule
{
    public class CallSiteInformation
    {
        public string callerfile { get; set; }
        public string callermembername { get; set; }
        public int callerlinenumber { get; set; }

        public CallSiteInformation(string file, string name, int number)
        {
            callerfile = file;
            callermembername = name;
            callerlinenumber = number;
        }
    }

    public enum EnumLoggerType 
    { 
        PROLOG, 
        DEBUG, 
        EXCEPTION,
        EVENT, 
        PIN, 
        PMI, 
        SOAKING,
        GPIB, 
        TCPIP,
        TEMP, 
        LOT,
        PARAMETER,
        COMPVERIFY,
        MONITORING,
        INFO,
        ENVMONITORING,
        LOADERMAP
    }

    public enum ProberLogLevel
    {
        UNDEFINED = 0,
        PROLOG,
        EVENT,
        DEBUG,
        FILTEREDDEBUG,
    }

    public enum EnumProberModule
    {
        WAFERALIGNER,
        PINALIGNER,
        PMI,
        POLISHWAFER,
        SOAKING,
        CARDCHANGE,
        VISIONPROCESSING,
        MARKALIGNER
    }

    public enum PrologType
    {
        UNDEFINED = 0,
        INFORMATION,
        OPERRATION_ALARM,
        SYSTEM_FAULT,
    }

    public enum DebuglogType
    {
        UNDEFINED = 0,
        DEBUG,
        ERROR,
        EXCEPTION,
    }

    public enum EventlogType
    {
        UNDEFINED = 0,
        EVENT,
    }

    public enum GPIBlogType
    {
        UNDEFINED = 0,
        GPIB,
    }

    public enum ModuleLogType
    {
        UNDEFIEND = -1,
        LOT,
        PMI,
        PROBING,
        PROBING_ZUP,
        PROBING_ZDOWN,
        SOAKING,
        CLEANING,
        PIN_ALIGN,
        WAFER_LOAD,
        WAFER_ALIGN,
        WAFER_UNLOAD,
        WAFER_EXCHANGE,
        CLEANING_WAFER_LOAD,
        CLEANING_WAFER_UNLOAD,

        LOT_SETTING,
        ARM_TO_BUFFER,
        ARM_TO_STAGE,
        ARM_TO_FIXED,
        ARM_TO_INSP,
        ARM_TO_PREALIGN,
        ARM_TO_SLOT,
        BUFFER_TO_ARM,
        CARM_TO_CBUFFER,
        CARM_TO_STAGE,
        CARM_TO_CARDTRAY,
        CBUFFER_TO_CARM,
        STAGE_TO_CARM,
        TRAY_TO_CARM,
        STAGE_TO_ARM,
        FIXED_TO_ARM,
        INSP_TO_ARM,
        OCR,
        OCR_TO_PREALIGN,
        PREALIGN,
        PREALIGN_TO_ARM,
        PREALIGN_TO_OCR,
        SCAN_CASSETTE,
        SLOT_TO_ARM,
        CARD_LOAD,
        CARD_UNLOAD,
        CARD_DOCK,
        CARD_UNDOCK,
        MODE_CHANGE,
        CLOSE_FOUP_COVER,
        MARK_ALIGN
    }
    public enum StateLogType
    {
        UNDEFINED = -1,
        START,
        DONE,
        ERROR,
        ABORT,
        PAUSE,
        RESUME,
        SUSPEND,
        RETRY,
        SET,
        LOADERALLDONE,   //Loader ExternalState의 상태가 Done State가 될 경우 (모든 Lot가 끝날 경우)
    }
    public enum LoaderMapLogType
    {
        DN = -1, //lot중이 아닌상황에서의 로그
        DF = 0, //lot중이나 mapdraw 비대상 로그
        DT = 1, // lot중이며 mapdraw 대상 로그
    }
    public enum SubSequenceType
    {
        UNDEFINED = -1,
        PA_PUT,
        PRE_ALIGN,
        PA_PICK,
        WAFER_ALIGN,
        PIN_ALIGN,
        MARK_ALIGN,
        PROBING,
        CLEANING,
    }
    /// <summary>
    /// 하드웨어 모니터링 할 때 Log Type
    /// </summary>
    public enum MonitoringLogType
    {
        MOTION,
        TEMP
    }
}

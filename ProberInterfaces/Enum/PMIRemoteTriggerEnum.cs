using System.ComponentModel;

namespace ProberInterfaces.Enum
{
    public enum PMIRemoteOperationEnum
    {
        UNDEFIEND = 0,
        ITSELF,
        SKIP,
        FORCEDEXECUTE
    }
    public enum PMIRemoteTriggerEnum
    {
        [Description("NONE")]
        UNDIFINED,

        // 아래의 파라미터에 의한
        // 1. EveryWaferInterval 
        // 2. TotalNumberOfWafersToPerform
        [Description("EVERY")]
        EVERY_WAFER_TRIGGER,
        [Description("TOTAL")]
        TOTALNUMBER_WAFER_TRIGGER,
    }
}

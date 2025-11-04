using System.Runtime.Serialization;

namespace ProberInterfaces
{
    public enum PINPROCESSINGSTATE
    {
        BE_CONTINUE = 0,
        USER_ABORT = 1,
        ALIGN_FAILED = 2,
        NO_MORE_AVAIABLE_PIN = 3,
        ALIGN_PASSED = 4,
        UNKNOWN_STATE = 5
    }

    public enum PINALIGNRESULT
    {
        PIN_UNKNOWN_FAILED = -1,
        PIN_NOT_PERFORMED = 0,
        PIN_PASSED = 1,
        PIN_SKIP = 2,
        PIN_FORCED_PASS = 3,
        PIN_FOCUS_FAILED = 4,
        PIN_TIP_FOCUS_FAILED = 5,
        PIN_BLOB_FAILED = 6,
        PIN_OVER_TOLERANCE = 7,
        PIN_BLOB_TOLERANCE = 8,
        INVALID_PIN_TIP_SIZE= 9
    }

    public enum PINGROUPALIGNRESULT
    {
        PASS = 0,
        CONTINUE = 1,
        FAIL = 2
    }

    public enum PINMODE
    {
        UPDATE_ONLY = 0,
        RESIZE_BOLB = 1,
        SKIP_PIN = 2,
        FORCED_PASS = 3   
    }    
    public enum PINALIGNSOURCE
    {
        UNDEFINED = 0,
        WAFER_INTERVAL,
        DIE_INTERVAL,
        TIME_INTERVAL,
        NEEDLE_CLEANING,
        POLISH_WAFER,
        SOAKING,
        PIN_REGISTRATION,
        DEVICE_CHANGE,
        CARD_CHANGE,
        AUTO_RETRY
    }
    public enum SAMPLEPINALGINRESULT
    {
        CONTINUE = 0,//Align 진행 시 Sample Pin Align으로 진행.
        PASS = 1,//EachPinResult의 PinResult가 Pass인 것 중 Sample Pin 관련 조건에 통과 했을 때.
        FAIL = 2,//EachPinResult의 PinResult가 Pass인 것 중 Sample Pin 관련 조건에 통과하지 못 했을 때.
        DISABLED = 3//Pin Align시 Sample Pin으로 Align하지 않음.
    }

    public enum EnumThresholdType
    {
        AUTO,
        MANUAL
    }

    public enum IMAGE_PROC_TYPE
    {
        PROC_BLOB = 0,
        PROC_PATTERN_MATCHING = 1
    }

    [DataContract]
    public enum PROBECARD_TYPE
    {
        [EnumMember]
        Cantilever_Standard = 0,        // 가장 평범한 캔틸레버 핀
        [EnumMember]
        MEMS_Dual_AlignKey = 1,          // 마이크론 전용
        [EnumMember]
        VerticalType = 2
    }

    public enum PIN_OBJECT_COLOR
    {
        WHITE = 0,
        BLACK = 1
    }
    public enum TIP_SIZE_VALIDATION_RESULT
    {
        UNDEFINED = 0,
        OUT_OF_RANGE = 1,
        IN_RANGE = 2
    }
}

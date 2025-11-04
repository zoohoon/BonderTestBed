namespace ProberInterfaces
{
    public enum EnumProbingState
    {
        IDLE = 0x0000,
        RUNNING = 0x0100,        
        PINPADMATCHED = 0x0140,
        PINPADMATCHPERFORM = 0x0141,
        ZUP = 0x0110,
        ZUPPERFORM = 0x0111,
        ZDN = 0x0120,
        ZDNPERFORM = 0x0121,
        ZUPDWELL = 0x0122,
        ZDOWNDELL = 0x0123,
        SUSPENDED = 0x0200,
        DONE = 0x0400,
        ERROR = 0x0800,
        ABORTED = 0x2000,
        PAUSED = 0x2100,
        ABORT = 0x2200,
        EVENT = 0x2400,
        READY = 0x2401,
        MOVE_HEATING_POS = 0x2402
    }

    public enum EnumProbingJobResult
    {
        UNKNOWN = -1,
        PROB_READY = 0x200000,
        PROB_PROGRESSING,
        PROB_INVALIDSTATE,
        PROB_PROGRESS_DONE,
        PROB_RESTEST_DONE,
        PROB_NOT_PERFORMED,
        PROB_EOW,
        PROB_PAUSED,
        PROB_ABORTED,
    }

    public enum OverDriveStartPositionType
    {
        FIRST_CONTACT,
        ALL_CONTACT
    }

    // 'Testing End Information' in TSK
    public enum ProbingEndReason
    {
        UNDEFINED              = 0x1111, // Probing 종료 명령을 수신하지 않았을 때의 기본 Enum 입니다.
        NORMAL                  = 0x0000, // Probing이 정상적으로 종료되었을 때의 Enum입니다.
        YIELD_NG               = 0x0001,
        CONTINUOUS_FAIL_NG     = 0x0002,
        MANUAL_UNLOAD          = 0x0003,
        OTHER_REJECT           = 0x0004,
        ERROR_NG               = 0x0005, // Tester 나 Host 로 부터 비정상 종료 명령을 받았을 때의 Enum 입니다.
        MANUAL_LOT_END         = 0x0006, // UI에서 버튼을 통한 Cell End, Lot End 로 종료 명령을 받았을 때의 Enum 입니다.
        SEQUENCE_INVALID_ERROR = 0x0007, // SEQUENCE_INVALID_ERROR 로 인해 wafer 가 skip 되는 상황인 경우 호출됩니다.
    }
    /// <summary>
    /// Probing이 Suspended 로 전환 되는 이유에 대한 Enum입니다.   
    /// </summary>
    public enum ReasonforSuspend
    {
        NOMAL = 0x0000,
        SOAKING,
        PMI,
        TEMPERATURE,
        ALIGNMENT
    }

    public enum ProbingCoordCalcType
    {
        USERCOORD, //Wafer Coord User Undex [0, 0]
        STANDARDEDGE,
        USERCOORDINDUT // 첫번째 Dut User Index [0, 0]
    }

    public enum TestingTimeoutEnum
    {
        UNKNOWN = -1,
        ON,
        OFF
    }

    public enum RepeatedAlarmEnum
    {
        UNKNOWN = -1,
        ON,
        OFF
    }

    public enum MultipleContactBackODOptionEnum
    {
        BackODFromProbingOD = 0,
        BackODFromAllContact = 1,
        BackODFromFirstContact = 2
    }
}

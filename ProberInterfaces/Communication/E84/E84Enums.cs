namespace ProberInterfaces
{
    public enum E84CommModuleTypeEnum
    {
        INVALID = -1,
        UNDEFINED = 0,
        EMOTIONTEK =1,
        EMUL
    }

    public enum E84ConnTypeEnum
    {
        INVALID = -1,
        UNDEFINED = 0,
        ETHERNET,
        SERIAL,
        SIMUL
    }

    public enum E84SignalActiveEnum
    {
        INVALID = -1,
        UNDEFINED = 0,
        INPUT,
        OUTPUT,
        AUX
    }
    public enum E84SignalTypeEnum
    {
        INVALID = -1,
        UNDEFINED = 0,
        L_REQ,
        U_REQ,
        VA,
        READY,
        VS_0,
        VS_1,
        HO_AVBL,
        ES,
        NC,
        SELECT,
        MODE,
        GO,
        VALID,
        CS_0,
        CS_1,
        AM_AVBL,
        TR_REQ,
        BUSY,
        COMPT,
        CONT,
        V24,
        V24GNC,
        SIGCOM,
        FG
    }

    public enum E84OPTypeEnum
    {
        SINGLE, // 싱글 핸드오프
        SIMULTANEOUS, // 동시 핸드오프
        CONTINUOUS, // 연속 핸드오프
    }

    public enum E84OPModuleTypeEnum
    {
        INVALID = -1,
        UNDEFINED = 0,
        FOUP,
        CARD
    }

    public enum E84SignalInputIndex
    {
        VALID = 0,
        CS_0 = 1,
        CS_1 = 2,
        AM_AVBL = 3,
        TR_REQ = 4,
        BUSY = 5,
        COMPT = 6,
        CONT = 7,
        GO = 8,
    }

    public enum E84SignalOutputIndex
    {
        L_REQ = 0,
        U_REQ = 1,
        VA = 2,
        READY = 3,
        VS_0 = 4,
        VS_1 = 5,
        HO_AVBL = 6,
        ES = 7,
        SELECT = 8,
        MODE = 9,
    }

    public enum E84ComStatus
    {
        DISCONNECTED = 0,
        CONNECTED = 1,
        CONNECTING = 2,
    }

    public enum E84ComType
    {
        SERIAL = 0,
        UDP = 2,
    }

    public enum E84Mode
    {
        UNDEFIND = -1,
        MANUAL = 0,
        AUTO = 1,
    }

    public enum E84Steps
    {
        UNDEFIND = -1,
        WAIT_HO_AVBL_ON = 0,
        WAIT_CS_0_OR_CS_1_ON = 1,
        WAIT_VALID_ON = 2,
        WAIT_TR_REQ_ON = 3,
        WAIT_CLAMP_OFF = 4,
        WAIT_BUSY_ON = 5,
        WAIT_CHANGING_CARRIER_STATUS = 6,
        WAIT_BUSY_OFF = 7,
        WAIT_TR_REQ_OFF = 8,
        WAIT_COMPT_ON = 9,
        WAIT_VALID_OFF = 10,
        WAIT_COMPT_OFF = 11,
        WAIT_CS_0_OR_CS_1_OFF = 12,
    }

    public enum E84SubStateEnum
    {
        UNDEFINE = -1,
        IDLE = 0,
        READY,
        CS_SPECIFIED,
        TRANSFET_START,
        CARRIER_DETECTED,
        TRANSFER_DONE,
        CS_RELEASE,
        AUTO_RECOVERY,
        DONE
    }
    public enum E84SubSteps
    {
        //INITITAL_STATUS = 0,
        //AFTER_L_REQ_ON_TO_OHT = 1,
        //AFTER_CS_0_OR_CS_1_OFF_FROM_OHT = 2,
        //AFTER_UL_REQ_ON_TO_OHT = 3,
        INVALID = 0,
        START_LOADING = 1,
        DONE_LOADING = 2,
        START_UNLOADING = 3,
        DONE_UNLOADING = 4
    }
    public enum E84EventCode
    {
        UNDEFINE,
        TP1_TIMEOUT,
        TP2_TIMEOUT,
        TP3_TIMEOUT,
        TP4_TIMEOUT,
        TP5_TIMEOUT,
        TP6_TIMEOUT,
        TD0_DELAY,
        TD1_DELAY,
        HEARTBEAT_TIMEOUT,
        LIGHT_CURTAIN_ERROR,
        DETERMINE_TRANSFET_TYPE_ERROR,
        UNCLAMP_TIMEOUT,
        HO_AVBL_SEQUENCE_ERROR,
        CS_ON_SEQUENCE_ERROR,
        VALED_ON_SEQUENCE_ERROR,
        TR_REQ_ON_SEQUENCE_ERROR,
        CLAMP_OFF_SEQUENCE_ERROR,
        BUSY_ON_SEQUENCE_ERROR,
        CARRIER_SUATUS_CHANGE_STEP_SEQUENC_ERROR,
        BUSY_OFF_SEQUENCE_ERROR,
        TR_REQ_OFF_SEQUENCE_ERROR,
        COMPT_ON_SEQUENCE_ERROR,
        VALID_OFF_SEQUENCE_ERROR,
        COMPT_OFF_SEQUENCE_ERROR,
        CS_OFF_SEQUENCE_ERROR,
        HO_AVAL_OFF_SEQUENCE_ERROR,
        SENSOR_ERROR_LOAD_ONLY_PRESENCS,
        SENSOR_ERROR_LOAD_ONLY_PLANCEMENT,
        SENSOR_ERROR_UNLOAD_STILL_PRESENCS,
        SENSOR_ERROR_UNLOAD_STILL_PLANCEMENT,
        HAND_SHAKE_ERROR_LOAD_PRESENCE,  // 이재작업이 시작된 상태가 아닌데 캐리어가 감지된 경우
        HAND_SHAKE_ERROR_LOAD_MODE,   // OHT 가 도착하여 CS, VALID 신호가 켜졌는데 모드를 변경하려고 한 경우
        CLAMP_ON_BEFORE_OFF_BUSY        
    }

    /// <summary>
    ///  E84Module로부터 Event가 들어왔을때 무슨 동작을 해야하는지 정의함.
    ///  ERROR_Warning: log, clearEvent
    ///  ERROR_Req_Off: log, showMsg, errorstate, req off, ready off
    ///  ERROR_Ho_Off: log, showMsg, errorstate, req off, ready off, ho off
    ///  ERROR_Emergency: log, showMsg, errorstate, req off, ready off, ho off, es off
    /// </summary>
    public enum E84ErrorActEnum
    {
        ERROR_Ho_Off,
        ERROR_Warning,
        ERROR_Req_Off,        
        ERROR_Emergency,
    }

    public enum E84PresenceTypeEnum
    {
        UNDEFINE,
        EXIST, // Exist 센서만 들어왔을때 Presence ON
        PRESENCE // Exist 센서 & Presence 둘다 들어왔을때 Presence ON
    }

    public enum CardBufferOPEnum
    {
        UNDEFINE,
        LOAD, // Buffer 에 Card가 없는 상태
        UNLOAD // Buffer 에 Card 가 있는 상태.
    }

    public enum E84MaxCount
    {
        E84_MAX_AUX_INPUT = 6,
        E84_MAX_AUX_OUTPUT = 1,
        E84_MAX_AUX_IO = E84_MAX_AUX_INPUT + E84_MAX_AUX_OUTPUT,
        E84_MAX_E84_INPUT = 9,
        E84_MAX_E84_OUTPUT = 10,
        E84_MAX_TIMER_TP = 6,
        E84_MAX_TIMER_TD = 2,
        E84_MAX_LOAD_PORT = 2,
    }

    public enum e84ErrorCode
    {
        E84_ERROR_SUCCESS = 0,
        E84_ERROR_NOT_INITIALIZED = 1,
        E84_ERROR_NOT_FOUND = 2,
        E84_ERROR_WRONG_PARAMETER = 8,
        E84_ERROR_TIME_OUT = 14,
        E84_ERROR_WRONG_RETURN = 15,
        E84_ERROR_DISCONNECT = 50,
        E84_ERROR_DISCONNECTING = 51,
        E84_ERROR_CONNECTED = 1003,
    }
}

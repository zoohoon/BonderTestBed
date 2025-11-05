using System.Runtime.Serialization;

namespace SecsGemServiceInterface
{
    [DataContract(Name = nameof(EnumCarrierAction))]
    public enum EnumCarrierAction
    {
        [EnumMember]
        PROCEEDWITHCARRIER,
        [EnumMember]
        PROCEEDWITHSLOT,
        [EnumMember]
        CANCELCARRIER,
        [EnumMember]
        RELEASECARRIER,
        [EnumMember]
        PROCESSEDWITHCELLSLOT,
        [EnumMember]
        CHANGEACCESSMODE
    }


    [DataContract(Name = nameof(SecsGemStateEnum))]
    public enum SecsGemStateEnum
    {
        [EnumMember]
        UNKNOWN = -1,
        [EnumMember]
        INIT = 0,
        [EnumMember]
        IDLE = 1,
        [EnumMember]
        SETUP = 2,
        [EnumMember]
        READY = 3,
        [EnumMember]
        EXECUTE = 4
    }


    [DataContract(Name = nameof(SecsCommStateEnum))]
    public enum SecsCommStateEnum
    {
        [EnumMember]
        UNKNOWN = -1,
        [EnumMember]
        COMM_DISABLED = 1,
        [EnumMember]
        WAIT_CR_FROM_HOST = 2,
        [EnumMember]
        WAIT_DELAY = 3,
        [EnumMember]
        WAIT_CRA = 4,
        [EnumMember]
        COMMUNICATING = 5
    }

    [DataContract(Name = nameof(SecsControlStateEnum))]
    public enum SecsControlStateEnum
    {
        [EnumMember]
        UNKNOWN = -1,
        [EnumMember]
        EQ_OFFLINE = 1,
        [EnumMember]
        ATTEMPT_ONLINE = 2,
        [EnumMember]
        HOST_OFFLINE = 3,
        [EnumMember]
        ONLINE_LOCAL = 4,
        [EnumMember]
        ONLINE_REMOTE = 5
    }

    [DataContract(Name = nameof(EnumRemoteCommand))]
    public enum EnumRemoteCommand
    {
        [EnumMember]
        UNDEFINE = -1,
        [EnumMember]
        ABORT = 0,
        [EnumMember]
        CC_START = 1,
        [EnumMember]
        DLRECIPE = 2,
        [EnumMember]
        JOB_CANCEL = 3,
        [EnumMember]
        JOB_CREATE = 4,
        [EnumMember]
        ONLINE_LOCAL = 5,
        [EnumMember]
        ONLINE_REMOTE = 6,
        [EnumMember]
        ONLINEPP_SELECT = 7,
        [EnumMember]
        PAUSE = 8,
        [EnumMember]
        PP_SELECT = 9,
        [EnumMember]
        PSTART = 10,
        [EnumMember]
        PW_REQUEST = 11,
        [EnumMember]
        RESTART = 12,
        [EnumMember]
        RESUME = 13,
        [EnumMember]
        SCAN_CASSETTE = 14,
        [EnumMember]
        SIGNAL_TOWER = 15,
        [EnumMember]
        START = 16,
        [EnumMember]
        STOP = 17,
        [EnumMember]
        UNDOCK = 18,
        [EnumMember]
        WFCLN = 19,
        [EnumMember]
        WFIDCONFPROC = 20,
        [EnumMember]
        ZIF_REQUEST = 21,
        [EnumMember]
        ACTIVATE_PROCESS = 22,
        [EnumMember]
        DOWNLOAD_STAGE_RECIPE = 23,
        [EnumMember]
        SET_PARAMETERS = 24,
        [EnumMember]
        DOCK_FOUP = 25,
        [EnumMember]
        SELECT_SLOTS = 26,
        [EnumMember]
        START_LOT = 27,
        [EnumMember]
        Z_UP = 28,
        [EnumMember]
        END_TEST = 29,
        [EnumMember]
        CANCEL_CARRIER = 30,
        [EnumMember]
        CARRIER_SUSPEND = 31,
        [EnumMember]
        ERROR_END = 32,
        [EnumMember]
        START_STAGE = 33,
        [EnumMember]
        CANCELCARRIER,
        [EnumMember]
        CHECKPARMETER,
        [EnumMember]
        ZUP,
        [EnumMember]
        TESTEND,
        [EnumMember]
        WAFERUNLOAD,
        [EnumMember]
        WAFERID_LIST,
        [EnumMember]
        UNDOCK_FOUP,        
        [EnumMember]
        STAGE_SLOT,
        [EnumMember]
        SELECT_SLOTS_STAGES,
        [EnumMember]
        END_TEST_LP,
        [EnumMember]
        ERROR_END_LP,
        [EnumMember]
        CHANGE_LP_MODE_STATE,
        [EnumMember]
        PABORT,    
        [EnumMember]
        START_CARD_CHANGE,
        [EnumMember]
        MOVEIN_CARD_CLOSE_COVER,
        [EnumMember]
        PROCEED_CARD_CHANGE,
        [EnumMember]
        SKIP_CARD_ATTACH,
        [EnumMember]
        CARD_SEQ_ABORT,        
        [EnumMember]
        DEVICE_CHANGE,
        [EnumMember]
        LOTMODE_CHANGE,
        [EnumMember]
        SELECT_SLOTS_STAGE,
        [EnumMember]
        CHANGE_LOADPORT_MODE,
        [EnumMember]
        TC_START,
        [EnumMember]
        TC_END,
        [EnumMember]
        WAFER_CHANGE

    }

}
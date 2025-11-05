namespace ProberInterfaces
{
    public enum GEMStageStateEnum
    {
        UNDIFIND = 0,
        OFFLINE = 1,
        ONLINE =2,
        IDLE = 3,
        ALLOCATED = 4,
        PROCESSING = 5,
        READY_TO_TEST = 6, //READY_TO_TEST
        UNLOADING = 7,
        STAGE_ERROR = 8,
        READY_Z_UP = 9,
        NEXT_WAFER_PREPRCOESSING = 10,
        Z_DOWN = 11,
        MAINTENANCE,
        DISCONNECTED,
        LOTOP_PAUSED,
        LOTOP_ABORTED,
        LOTOP_IDLE,
        LOTOP_RUNNING,
        LOTOP_ERROR,
        AVAILABLE,
        UNAVAILABLE
    }

    public enum GEMPreHeatStateEnum
    {
        UNDIFIND = 0,
        NOT_PRE_HEATING = 1,
        PRE_HEATING = 2
    }

    public enum GEMFoupStateEnum
    {
        UNDIFIND = 0,
        OFFLINE = 1,
        ONLINE =2,
        READY_TO_LOAD = 3,
        PLACED = 4,
        ACTIVED = 5,
        READ_CARRIER_MAP = 6,
        SLOT_SELECTED = 7,
        PROCESSING = 8,
        READY_TO_UNLOAD = 9,
        OUT_OF_SERVICE,
        TRANSFER_BLOCKED,
        IN_SERVICE,
        TRANSFER_READY
    }

    public enum GEMSensorStatusEnum
    {
        DISCONNECTED = 0,                 
        NORMAL = 1,        
        ALARM = 2
    }
}

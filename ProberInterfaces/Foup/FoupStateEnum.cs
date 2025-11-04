using System;
using System.Runtime.Serialization;

namespace ProberInterfaces.Foup
{
    public enum FoupBehaviorStateEnum
    {
        UNDEFINED,
        IDLE,
        TIMEOUT,
        ERROR,
        Done
    }

    public enum FoupSafetyStateEnum
    {
        UNDEFINED,
        IDLE,
        TIMEOUT,
        ERROR,
        DONE
    }

    public enum FoupProcedureStateEnum
    {
        IDLE,
        PreSafetyError,
        PostSafetyError,
        BehaviorError,
        DONE
    }

    [Serializable, DataContract]
    public enum FoupStateEnum
    {
        [EnumMember]
        UNDEFIND,
        /// <summary>
        /// FOUP ERROR 상태
        /// </summary>
        [EnumMember]
        ERROR,
        /// <summary>
        /// FOUP ILLEGAL 상태
        /// </summary>
        //ILLEGAL,
        /// <summary>
        /// FOUP LOAD 상태
        /// </summary>
        [EnumMember]
        LOAD,
        /// <summary>
        /// FOUP LOAD SEQUENCE 동작 중 상태
        /// </summary>
        [EnumMember]
        LOADING,
        /// <summary>
        /// FOUP UNLOAD 상태
        /// </summary>
        [EnumMember]
        UNLOAD,
        /// <summary>
        /// FOUP UNLOAD SEQUENCE 동작 중 상태
        /// </summary>
        [EnumMember]
        UNLOADING,
        /// <summary>
        /// FOUP CASSETTE 없는 상태
        /// </summary>
        [EnumMember]
        EMPTY_CASSETTE,
    }

    public enum FoupPermissionStateEnum
    {
        /// <summary>
        /// 모두 사용 가능
        /// </summary>
        EVERY_ONE,
        /// <summary>
        /// LOAD, UNLOAD 불가능, FOUP COVER UP & DOWN 가능
        /// </summary>
        BUSY,
        /// <summary>
        /// 외부동작 불가,
        /// </summary>
        AUTO,
    }


    public enum DockingPlateStateEnum
    {
        LOCK,
        UNLOCK,
        //IDLE,
        ERROR
    }
    public enum DockingPortStateEnum
    {
        IN,
        OUT,
        ERROR,
        IDLE
    }
    public enum DockingPort40StateEnum
    {
        IN,
        OUT,
        ERROR,
        IDLE
    }
    public enum DockingPortDoorStateEnum
    {
        OPEN,
        CLOSE,
        IDLE,
        ERROR
    }
    public enum TiltStateEnum
    {
        UP,
        DOWN,
        IDLE,
        ERROR
    }
    public enum FoupCoverStateEnum
    {
        CLOSE,
        OPEN,
        IDLE,
        IGNORE,   // 추가 selly 
        ERROR
    }

    public enum FoupCassetteOpenerStateEnum
    {
        UNLOCK,
        LOCK,
        IDLE,
        ERROR
    }
    //public enum FoupScanCassetteStateEnum
    //{
    //    ATTACH,
    //    DETACH,
    //    IDLE,
    //    ERROR
    //}
    public enum FoupPRESENCEStateEnum
    {
        CST_ATTACH,
        CST_DETACH,
        CST_NOT_MATCHED,
        ERROR
    }
    public enum Foup6IN_PRESENCEStateEnum
    {
        CST_ATTACH,
        CST_DETACH,
        CST_NOT_MATCHED,
        ERROR
    }
    public enum Foup8IN_PRESENCEStateEnum
    {
        CST_ATTACH,
        CST_DETACH,
        CST_NOT_MATCHED,
        ERROR
    }
    public enum Foup12IN_PRESENCEStateEnum
    {
        CST_ATTACH,
        CST_DETACH,
        CST_NOT_MATCHED,
        ERROR
    }
    public enum FoupWaferOutSensorStateEnum
    {
        NotDetected,
        Detected,        
        ERROR
    }

    public enum FoupModeStatusEnum
    {
        ONLINE,
        OFFLINE
    }
}

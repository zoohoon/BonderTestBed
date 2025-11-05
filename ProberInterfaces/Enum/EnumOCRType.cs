using System;
using System.Runtime.Serialization;

namespace ProberInterfaces.Enum
{
    /// <summary>
    /// OCR Type을 정의합니다.
    /// </summary>
    [DataContract]
    public enum OCRTypeEnum
    {
        /// <summary>
        /// UNDEFINED
        /// </summary>
        [EnumMember]
        UNDEFINED,
        /// <summary>
        /// SEMICS
        /// </summary>
        [EnumMember]
        SEMICS,
        /// <summary>
        /// COGNEX
        /// </summary>
        [EnumMember]
        COGNEX,
    }

    /// <summary>
    /// OCR 방향을 정의합니다.
    /// </summary>
    [DataContract]
    public enum OCRDirectionEnum
    {
        /// <summary>
        /// UNDEFINED
        /// </summary>
        [EnumMember]
        UNDEFINED,
        /// <summary>
        /// FRONT
        /// </summary>
        [EnumMember]
        FRONT,
        /// <summary>
        /// BACK
        /// </summary>
        [EnumMember]
        BACK,
    }

    /// <summary>
    /// OCR Mode를 정의합니다.
    /// </summary>
    [DataContract]
    public enum OCRModeEnum
    {
        /// <summary>
        /// NONE: OCR이 달려있지 않은 장비에서 사용하기 위함.  OCR시도를 하지않고 임의로 자동 생성된 값으로 WaferID를 사용함. 현재 잘 사용되고 있지 않은것으로 보임.
        /// </summary>
        [EnumMember]
        NONE,
        /// <summary>
        /// READ: OCRDevParam대로 OCR시도함. 
        /// </summary>
        [EnumMember]
        READ,
        /// <summary>
        /// MANUAL: 무조건 Manual Input으로 WaferID값 사용함. 
        /// </summary>
        [EnumMember]
        MANUAL,
        /// <summary>
        ///DEBUGGING: OCR이 달려있지만 OCR시도를 안하기 위해 사용함. 무조건 Done으로 처리, 임의로 생성된 값으로 WaferID를 사용함. 
        /// </summary>
        [EnumMember]
        DEBUGGING,
    }

    /// <summary>
    /// OCR 상태를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum OCRReadStateEnum
    {
        /// <summary>
        /// NONE: OCR을 한번도 시도하지 않은 상태 
        /// </summary>
        [EnumMember]
        NONE,
        
        /// <summary>
        /// READING: 잘사용되고 있지 않은 상태
        /// </summary>
        [EnumMember]
        READING, 

        /// <summary>
        /// DONE: OCR Configs중 OCR 성공해서 더이상 OCR을 시도하지 않는 상태
        /// </summary>        
        [EnumMember]
        DONE,

        /// <summary>
        /// ABORT: OCR Configs중 모두 실패하고 더이상 OCR을 시도하지 않는 상태 (단, Retry아직 시도하기 전의 상태 )
        /// </summary>
        [EnumMember]
        FAILED,

        /// <summary>
        /// INVALID: 잘사용되고 있지 않은 상태
        /// </summary>
        [EnumMember]
        INVALID,


        /// <summary>
        /// ABORT: OCR Configs중 모두 실패하고 더이상 OCR을 시도하지 않는 상태 (단, Retry도 모두 실패한 상태 )
        /// </summary>
        [EnumMember]
        ABORT
    }
}

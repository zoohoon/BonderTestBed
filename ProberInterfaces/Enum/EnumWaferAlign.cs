using System.Runtime.Serialization;

namespace ProberInterfaces
{
    public enum EnumWaferSize //Loader에서는 SubstrateSizeEnum타입을 사용한다.
    {
        INVALID = -1,
        UNDEFINED = 0,
        INCH6 = UNDEFINED + 1,
        INCH8,
        INCH12,
        CUSTOM,
    }

    [DataContract]
    public enum EnumWaferType
    {
        [EnumMember]
        INVALID = -1,
        [EnumMember]
        UNDEFINED = 0,
        [EnumMember]
        STANDARD = UNDEFINED + 1,
        [EnumMember]
        POLISH,
        [EnumMember]
        CARD,
        [EnumMember]
        TCW
    }

    public enum EnumPadType
    {
        INVALID = -1,
        UNDEFINED = 0,
        NORMAL = UNDEFINED + 1,
        POSITIVE_BUMP,
        NEGATIVE_BUMP
    }

    public enum EnumWaferAlignType
    {
        INVALID = -1,
        UNDEFINED = 0,
        STANDARD = UNDEFINED + 1
    }

    public enum EnumWaferAlignResult
    {
        INVALID = -1,
        UNDEFINED = 0,
        SUCESS = UNDEFINED + 1,
        FAILURE
    }
    public enum LowMagPosition
    {
        Original = 0,
        Left,
        Right
    }

    public enum HeightMappingType
    {
        INVALID = -1,
        UNDEFINED = 0,
        PlanePoint_1 = 1,
        PlanePoint_5 = 5,
        PlanePoint_9 = 9
    }

    public enum EnumExecution
    {
        INVALID = -1,
        UNDEFINED = 0,
        EXECUTION = UNDEFINED + 1,
        DO_NOT_EXECUTION
    }

    public enum EnumWAProcDirection
    {
        INVALID = -1,
        UNDEFINED = 0,
        LEFT = UNDEFINED + 1,
        VERTICAL,
        HORIZONTAL,
        BIDIRECTIONAL
    }


}

using System.Runtime.Serialization;

namespace ProberInterfaces
{
    //0xABCD에서 B는 Chiller Type, D는 사용유무, 이 법칙은 필요 없어짐.
    public enum EnumChillerModuleMode
    {
        NONE = 0x0000,
        EMUL = 0x0101,
        HUBER = 0x0201,
        REMOTE = 0x0202,
        SOLARDIN = 0x0203,
        CHAGO = 0x0204
    }

    //0xABCD에서 B는 DryAir Type, D는 사용유무
    public enum EnumDryAirModuleMode
    {
        NONE = 0x0000,
        EMUL = 0x0101,
        HUBER = 0x0201,
        REMOTE = 0x0202
    }
    public enum EnumValveType
    {
        [EnumMember]
        INVALID = -1,
        [EnumMember]
        UNDEFINED = 0,
        [EnumMember]
        IN = UNDEFINED + 1,
        [EnumMember]
        OUT,
        [EnumMember]
        PURGE,
        [EnumMember]
        DRAIN,
        [EnumMember]
        DRYAIR,
        [EnumMember]
        Leak,
        [EnumMember]
        MANUAL_PURGE,
    }
}

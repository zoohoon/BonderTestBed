using System.Runtime.Serialization;

namespace ProberInterfaces.Enum
{
    public enum EnumArrowDirection
    {
        LEFTUP,
        UP,
        RIGHTUP,
        LEFT,
        RIGHT,
        LEFTDOWN,
        DOWN,
        RIGHTDOWN
    }

    [DataContract]
    public enum EnumChuckPosition
    {
        [EnumMember]
        UNDEFINED,
        [EnumMember]
        LEFTUP,
        [EnumMember]
        UP,
        [EnumMember]
        RIGHTUP,
        [EnumMember]
        CENTER,
        [EnumMember]
        LEFT,
        [EnumMember]
        RIGHT,
        [EnumMember]
        LEFTDOWN,
        [EnumMember]
        DOWN,
        [EnumMember]
        RIGHTDOWN
    }
}

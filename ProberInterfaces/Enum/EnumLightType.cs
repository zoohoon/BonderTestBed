using System.Runtime.Serialization;

namespace ProberInterfaces
{

    public enum EnumLightType
    {
        [EnumMember]
        INVALID = -1,
        [EnumMember]
        UNDEFINED = 0,
        [EnumMember]
        COAXIAL = UNDEFINED + 1,
        [EnumMember]
        OBLIQUE = 2,
        [EnumMember]
        AUX = 3,
        [EnumMember]
        EXTERNAL1,
        [EnumMember]
        EXTERNAL2,
        [EnumMember]
        EXTERNAL3,
        [EnumMember]
        EXTERNAL4,
        [EnumMember]
        EXTERNAL5,
        [EnumMember]
        LastLightType = EXTERNAL5
    }
}

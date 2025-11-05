using System.Runtime.Serialization;

namespace ProberInterfaces
{
    public enum EnumProberCam 
    {
        [EnumMember]
        INVALID = -1,
        [EnumMember]
        UNDEFINED = 0,
        //==>Stage
        [EnumMember]
        WAFER_HIGH_CAM = UNDEFINED + 1,
        [EnumMember]
        WAFER_LOW_CAM,
        [EnumMember]
        PIN_HIGH_CAM,
        [EnumMember]
        PIN_LOW_CAM,

        //==>Loader
        PACL6_CAM,
        [EnumMember]
        PACL8_CAM,
        [EnumMember]
        PACL12_CAM,
        [EnumMember]
        ARM_6_CAM,
        [EnumMember]
        ARM_8_12_CAM,
        [EnumMember]
        OCR1_CAM,
        [EnumMember]
        OCR2_CAM,

        //==>ErrorMappingCam
        [EnumMember]
        MAP_1_CAM,
        [EnumMember]
        MAP_2_CAM,
        [EnumMember]
        MAP_3_CAM,
        [EnumMember]
        MAP_4_CAM,
        [EnumMember]
        MAP_5_CAM,
        [EnumMember]
        MAP_6_CAM,
        [EnumMember]
        MAP_7_CAM,
        [EnumMember]
        MAP_8_CAM,
        [EnumMember]
        MAP_REF_CAM,
        [EnumMember]
        CAM_LAST = MAP_REF_CAM + 1,
        [EnumMember]
        GIGE_VM0,


    }

    public enum EnumProberCamType
    {
        INVALID = -1,
        UNDEFINED = 0,
        STAGE_CAM = UNDEFINED +1,
        LOADER_CAM,
    }

    //public enum EnumProberCam
    //{
    //    [EnumMember]
    //    INVALID = -1,
    //    [EnumMember]
    //    UNDEFINED = 0,
    //    //==>Stage
    //    [EnumMember]
    //    WAFER_HIGH_CAM = UNDEFINED + 1,
    //    [EnumMember]
    //    WAFER_LOW_CAM,
    //    [EnumMember]
    //    PIN_HIGH_CAM,
    //    [EnumMember]
    //    PIN_LOW_CAM,


    //    //==>Loader
    //    [EnumMember]
    //    PACL6_CAM,
    //    [EnumMember]
    //    PACL8_CAM,
    //    [EnumMember]
    //    PACL12_CAM,
    //    [EnumMember]
    //    ARM_6_CAM,
    //    [EnumMember]
    //    ARM_8_12_CAM,
    //    [EnumMember]
    //    OCR1_CAM,
    //    [EnumMember]
    //    OCR2_CAM,

    //    //==>ErrorMappingCam
    //    [EnumMember]
    //    MAP_1_CAM,
    //    [EnumMember]
    //    MAP_2_CAM,
    //    [EnumMember]
    //    MAP_3_CAM,
    //    [EnumMember]
    //    MAP_4_CAM,
    //    [EnumMember]
    //    MAP_5_CAM,
    //    [EnumMember]
    //    MAP_6_CAM,
    //    [EnumMember]
    //    MAP_7_CAM,
    //    [EnumMember]
    //    MAP_8_CAM,
    //    [EnumMember]
    //    MAP_REF_CAM,
    //    [EnumMember]
    //    CAM_LAST = MAP_REF_CAM + 1
    //}

    public enum EnumErrorMapptionCam
    {
        INVALID = -1,
        UNDEFINED = 0,
        MAP_1_CAM,
        MAP_2_CAM,
        MAP_3_CAM,
        MAP_4_CAM,
        MAP_5_CAM,
        MAP_6_CAM,
        MAP_7_CAM,
        MAP_8_CAM,
        MAP_REF_CAM
    }

    public enum ColorDept
    {
        UNDIFIND =0,
        BlackAndWhite = 8,
        Color24 = 24,
        Color32 = 32
    }

    public enum FlipEnum
    {
        NONE =-1,
        FLIP =1
    }

    // xaml rendertransfrom flip = -1

    [DataContract]
    public enum DispFlipEnum
    {
        [EnumMember]
        NONE = 1,
        [EnumMember]
        FLIP = -1
    }

    public enum EnumGPCam
    {
        UNDEFINED = 0,
        //==>Stage
        WAFER_HIGH_CAM = UNDEFINED + 1,
        WAFER_LOW_CAM,
        PIN_HIGH_CAM,
        PIN_LOW_CAM,


        //==>Loader
        PACL6_CAM,
        PACL8_CAM,
        PACL12_CAM,
        ARM_6_CAM,
        ARM_8_12_CAM,
        OCR1_CAM,
        OCR2_CAM,
    }
}

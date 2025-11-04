using System.ComponentModel;

namespace ProberInterfaces.WaferAlignEX
{
    public enum WABoundarySetupFunction
    {
        UNDEFINED = -1,
        INVALID = UNDEFINED+1,
        TEACHBOTTOMLEFT,
        TEACHUPPERRIGHT,
        TEACHBOUNDARY,
        SAVE,
        PREVSTEP
    }

    public enum HeightPointEnum
    {
        UNDEFINED = -1,
        INVALID = UNDEFINED + 1,
        POINT1,
        POINT5,
        POINT9
        //SETTINGUSER,
    }

    public enum Low_ProcessingPointEnum
    {
        [Description("3 Point (ALL)")]
        LOW_3PT = 0,
        [Description("2 Point(Skip Left or Right)")]
        LOW_2PT
    }

    public enum High_ProcessingPointEnum
    {
        [Description("7 Point (ALL)")]
        HIGH_7PT = 0,
        [Description("5 Point (Skip Short)")]
        HIGH_5PT
    }

    public enum WAHeightSetupFunction
    {
        UNDIFINE = -1,
        INVALID = UNDIFINE + 1,
        ADDPOSITION,
        DELETEPOSITION,
        BACKSTEP,
        CHANGEDHIGHTTYPE,
        SHOWCONTROL,
        APPLY
    }


    public enum WACoordSetupFunction
    {
        UNDIFINE = -1,
        INVALID = UNDIFINE + 1,
        SETCOORD,
        SAVE,
        PREVSETP
    }

    public enum WADrawMapSetupFunction
    {
        UNDIFINE = -1,
        INVALID = UNDIFINE + 1,
        CHANGEEMPTY,
        CHANGETEST,
        CHANGEMARK,
        CHANGESKIP,
        DRAWAUTOMAP,
        SAVE,
        PREVSETP,
        CHANGEPATTERNWIDTH,
        CHANGEDPATTRNHEIGHT,
    }
}



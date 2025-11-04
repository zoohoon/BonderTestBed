namespace ProberInterfaces.WaferAlignEX.Enum
{
    public enum EnumWASubModuleEnable
    {
        UNDEFIND,
        ENABLE,
        DISABLE,
        THETA_RETRY
    }

    //public enum EnumWASubModuleEnable
    //{
    //    UNDEFIND,
    //    ENABLE,
    //    DISABLE
    //}

    public enum EnumIndexAlignDirection
    {
        UNDIFIND,
        LEFTUPPER,
        RIGHTUPPER,
        RIGHTLOWER,
        LEFTLOWER,
        LEFT,
        RIGHT,
        UPPER,
        LOWER
    }

    public enum EnumLowStandardPosition
    {
        UNDIFIND =-1,
        CENTER =0,
        LEFT,
        RIGHT
    }

    public enum EnumHighStandardPosition
    {
        UNDIFIND = -1,
        CENTER = 0,
        RIGHT_SHORT,
        LEFT_SHORT,
        LEFT_LONG,
        RIGHT_LOGN,
        UPPER,
        LOWER
    }

}

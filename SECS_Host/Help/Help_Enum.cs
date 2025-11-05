using System;

namespace SECS_Host.Help
{
    #region ==> Define Enum[DataType, ID]
    public enum SECS_DataTypeCategory
    {
        NONE = -1, LIST = 0, BINARY = 8,
        BOOL = 9, ASCII = 16, JIS8 = 17,
        INT8 = 24, INT1 = 25, INT2 = 26,
        INT4 = 28, FLOAT8 = 32, FLOAT4 = 36,
        UINT8 = 40, UINT1 = 41, UINT2 = 42, UINT4 = 44
    }
    public enum SECS_IDCategory { NONE, VID, RPTID, DATAID, CEID, CEED, SVID, SVNAME, UNITS }
    #endregion

    #region ==> Delegate[Log]
    public delegate void AddLogDelegate(string formatString, params object[] args);
    #endregion

    public static class EnumConverter
    {
        public static SECS_DataTypeCategory ParseDataTypeCategory(string strCategory)
        {
            SECS_DataTypeCategory retVal = SECS_DataTypeCategory.NONE;

            if (Enum.TryParse(strCategory, out retVal)){}

            return retVal;
        }

        public static SECS_IDCategory ParseIdCategory(string strCategory)
        {
            SECS_IDCategory retVal = SECS_IDCategory.NONE;

            if (Enum.TryParse(strCategory, out retVal))
            {
                retVal = (SECS_IDCategory)Enum.Parse(typeof(SECS_IDCategory), strCategory);
            }

            return retVal;
        }
    }
}


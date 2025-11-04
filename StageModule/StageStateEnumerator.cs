namespace StageModule
{
    public enum EnumStageJobResult
    {
        UNKNOWN = -1,
        STG_UNKNOWN = 0x100000,
        STG_DONE,
        STG_ERROR,
        STG_INVALIDSTATE,
        STG_AXIS_ERROR,
        STG_AXIS_ERRORLIMIT,
        STG_AXIS_SWLIMITERROR,
        STG_PIN_RANGE_ERROR,
        STG_PIN_HEIGHT_RANGE_ERROR,
        STG_PIN_NOT_ALIGNED,
        STG_WAF_RANGE_ERROR,
        STG_WAF_HEIGHT_RANGE_ERROR,
        STG_WAF_NOT_ALIGNED,
        STG_OD_LIMIT,
    }

    
}

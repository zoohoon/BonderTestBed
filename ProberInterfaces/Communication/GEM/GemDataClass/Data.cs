namespace ProberInterfaces.GEM
{
    public static class GemGlobalData
    {
        public static readonly string IP = "IP";
        public static readonly string Port = "Port";
        public static readonly string Active = "Active";
        public static readonly string DeviceID = "Device ID";
        public static readonly string LinkTestInterval = "Link Test Interval";
        public static readonly string RetryLimit = "Retry Limit";
        public static readonly string T3 = "T3";
        public static readonly string T5 = "T5";
        public static readonly string T6 = "T6";
        public static readonly string T7 = "T7";
        public static readonly string T8 = "T8";
    }

    public enum GEM_HCACK
    {
        COMMAND_HAS_BEEN_PERFORMED          = 0,
        COMMAND_DOES_NOT_EXIST              = 1,
        CANNOT_PERFORM_NOW                  = 2,
        AT_LEAST_ONE_PARAMETER_IS_INVALID   = 3,
        COMMAND_WILL_BE_PERFORMED           = 4,    //WITH COMPLETION SIGNALED LATER BY AN EVENT
        REJECTED                            = 5,    //ALREADY IN DESIRED CONDITION
        NO_SUCH_OBJECT_EXISTS               = 6
    }

    public enum GEMDataIDIndex
    {
        ALARM_DATAID_INDEX = 0,
        ECV_DATAID_INDEX = 1,
        RCMD_DATAID_INDEX = 2,
        SVDV_DATAID_INDEX = 3
    }


    //public enum GEM_ERRCODE
    //{
    //    No_ERROR                            = 0,
    //    UNKOWN_OBJECT_IN_OBJECT_SPECIFIER   = 1,
    //    UNKNOWN_TARGET_OBJECT_TYPE          = 2,
    //    UNKNOWN_OBJECT_INSTANCE             = 3,
    //    UNKNOWN_ATTRIBUTE_NAME              = 4,
    //    READ_ONLY_ATTRIBUTE_ACCESS_DENIED   = 5,
    //    UNKNOWN_OBJECT_TYPE                 = 6,
    //    INVALID_ATTRIBUTE_VALUE             = 7,
    //    SYNTAX_ERROR                        = 8,
    //    VERIFICATION_ERROR                  = 9,
    //    VALIDATION_ERROR                    = 10,
    //    OBJECT_IDENTIFIER_IN_USE            = 11,
    //    PARAMETERS_IMPROPERLY_SPECIFIED     = 12,
    //    INSUFFICIENT_PARAMETERS_SPECIFIED   = 13,
    //    UNSUPPORTED_OPTION_REQUESTED        = 14,
    //    BUSY                                = 15,
    //    NOT_AVAILABLE_FOR_PROCESSING        = 16,
    //    COMMAND_NOT_VALID_FOR_CURRENT_STATE = 17,
    //    NO_MATERIAL_ALTERED                 = 18,
    //    MATERIAL_PARTIALLY_PROCESSED        = 19,
    //    ALL_MATERIAL_PROCESSED              = 20,
    //    RECIPE_SPECIFICATION_RELATED_ERROR  = 21,
    //    FAILED_DURING_PROCESSING            = 22,
    //    FAILED_WHILE_NOT_PROCESSING         = 23,
    //    FAILED_DUE_TO_LACK_OF_MATERIAL      = 24,
    //    JOB_ABORTED                         = 25,
    //    JOB_STOPPED                         = 26,
    //    JOB_CANCELLED                       = 27,
    //    CANNOT_CHANGE_SELECTED_RECIPE       = 28,
    //    UNKNOWN_EVENT                       = 29,
    //    DUPLICATE_REPORT_ID                 = 30,
    //    UNKNOWN_DATA_REPORT                 = 31,
    //    DATA_REPORT_NOT_LINKED              = 32,
    //    UNKNOWN_TRACE_REPORT                = 33,
    //    DUPLICATE_TRACE_ID                  = 34,
    //    TOO_MANY_DATA_REPORTS               = 35,
    //    SAMPLE_PERIOD_OUT_OF_RANGE          = 36,
    //    GROUP_SIZE_TO_LARGE                 = 37,
    //    RECOVERY_ACTION_CURRENTLY_INVALID   = 38,
    //    BUSY_WITH_ANOTHER_RECOVERY_CURRENTLY_UNABLE_TO_PERFORM_THE_RECOVERY = 39,
    //    NO_ACTIVE_RECOVERY_ACTION           = 40,
    //    EXCEPTION_RECOVERY_FAILED           = 41,
    //    EXCEPTION_RECOVERY_ABORTED          = 42,
    //    INVALID_TABLE_ELEMENT               = 43,
    //    UNKNOWN_TABLE_ELEMENT               = 44,
    //    CANNOT_DELETE_PREDEFINED            = 45,
    //    INVALID_TOKEN                       = 46,
    //    INVALID_PARAMETER                   = 47,
    //    LOAD_PORT_DOES_NOT_EXIST            = 48,
    //    LOAD_PORT_ALREADY_IN_USE            = 49,
    //    MISSING_CARRIER                     = 50,
    //    ACTION_WILL_BE_PERFORMED_AT_EARLIEST_OPPORTUNITY = 32768
    //}
}

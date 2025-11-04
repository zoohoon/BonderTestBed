using System.Runtime.Serialization;

namespace ProberInterfaces.GEM
{
    public enum EnumGemCommand
    {
        IDLE,
        RUN,
        WAITDONE,
        DONE,
        END
    }

    public enum EnumGemRecipeState
    {
        IDLE,
        RUN,
        DONE
    }

    public enum EnumGEMAlarmCodeALCD
    {
        /// <summary>
        ///  0 Not Used
        ///  1 Personal safety
        ///  2 Equipment safety
        ///  3 parameter control warning
        ///  4 parameter control error
        ///  5 irrecoverable error
        ///  6 equipment status warning
        ///  7 attention flags
        ///  8 other cateories
        ///  >8 = Other categories
        ///  9-63 Reserved
        /// </summary>
        [EnumMember]
        INVALID = -1,
        [EnumMember]
        NOT_USED = INVALID + 1,
        [EnumMember]
        PERSONAL_SAFETY,
        [EnumMember]
        EQUIPMENT_SAFETY,
        [EnumMember]
        PARAMETER_CONTROL_WARNING,
        [EnumMember]
        PARAMETER_CONTROL_ERROR,
        [EnumMember]
        IRRECOVERABLE_ERROR,
        [EnumMember]
        EQUIPMENT_STATUS_WARNING,
        [EnumMember]
        ATTENTION_FLAGS,
        [EnumMember]
        DATA_INTEFRITY,
        [EnumMember]
        OTHER_CATEGORIES
    }

    public enum GemAlarmState
    {
        [EnumMember]
        INVALID = -1,
        [EnumMember]
        CLEAR = INVALID + 1,
        [EnumMember]
        SET
    }

    #region Ack

    //// < note > S6F2, F4, F10, F12, F14 
    //public enum EnumGEMAck6
    //{
    //    ERROR = -1, //>0 = Error, not accepted 
    //    ACCEPTED = 0x00, // Accepted 
    //    RESERVED = 0x04 //1-63 Reserved 
    //}

    //// < note > S3F18, F20, F22, F24, F26, F30, F32 
    //public enum EnumGEMCaAck
    //{
    //    PERFORMED = 0x00, // Acknowledge, command has been performed.
    //    INVALID_COMMAND = 0x01, //Invalid command
    //    CANNOT_PERFORMED = 0x02, // Can not perform now 
    //    INVALID_DATA = 0x03, // Invalid data or argument 
    //    WILL_BE_PERFORMED = 0x04, //  Acknowledge, request will be performed with completion signaled later by an event. 
    //    REJECTED = 0x05, // Rejected.  Invalid state. 
    //    PERFORMED_WITH_ERROR = 0x06, // Command performed with errors. 
    //    RESERVED,
    //}



    public enum EnumGEM_COMMACK	{ }
    public enum EnumGEM_OFLACK{ }
    public enum EnumGEM_ONLACK{ }
    public enum EnumGEM_SPAACK{ }
    public enum EnumGEM_CSAACK{ }
    public enum EnumGEM_TIAACK{ }
    public enum EnumGEM_TIACK{ }
    public enum EnumGEM_DRACK{ }
    public enum EnumGEM_LRACK{ }
    public enum EnumGEM_ERACK{ }

    /// <summary>
    ///EAC     Format: B:1 (invariant)     Used by: S2F16
    ///equipment acknowledge code, 0 ok Example: "B:1 0x00"
    ///0 - ok
    ///1 - one or more constants does not exist
    ///2 - busy
    ///3 - one or more values out of range
    /// </summary>
    public enum EnumGEM_EAC 
    {   
        UNDEFINE = -1,
        OK,
        ONE_OR_MORE_CONSTANT_IS_NOT_EXIST,
        BUSY,
        ONE_OR_MORE_OUT_OF_RANGE

    }
    public enum EnumGEM_HCACK{ }
    public enum EnumGEM_CPACK{ }
    public enum EnumGEM_RSPACK{ }
    public enum EnumGEM_STRACK{ }
    public enum EnumGEM_VLAACK{ }
    public enum EnumGEM_LVACK{ }
    public enum EnumGEM_LIMITACK{ }
    public enum EnumGEM_CEPACK{ }
    public enum EnumGEM_ACKC3{ }
    public enum EnumGEM_CAACK{ }
    public enum EnumGEM_RPMACK{ }
    public enum EnumGEM_RSACK{ }
    public enum EnumGEM_RRACK{ }
    public enum EnumGEM_TRACK{ }
    public enum EnumGEM_HOACK{ }
    public enum EnumGEM_HOCANCELACK{ }
    public enum EnumGEM_HOHALTACK{ }
    public enum EnumGEM_ACKC5{ }
    public enum EnumGEM_ACKA{ }
    public enum EnumGEM_ACKC6{ }
    public enum EnumGEM_RMACK{ }
    public enum EnumGEM_ACKC7{ }
    public enum EnumGEM_ACKC7A{ }
    public enum EnumGEM_ACKC10{ }
    public enum EnumGEM_SDACK{ }
    public enum EnumGEM_MDACK{ }
    public enum EnumGEM_ACKC13{ }
    public enum EnumGEM_TBLACK{ }
    public enum EnumGEM_OBJACK{ }
    public enum EnumGEM_SVCACK{ }
    public enum EnumGEM_DATAACK{ }
    public enum EnumGEM_ACKC15{ }
    public enum EnumGEM_SSACK{ }
    public enum EnumGEM_SSAACK{ }
    public enum EnumGEM_GOILACK{ }
    public enum EnumGEM_OCEACK{ }
    public enum EnumGEM_CCEACK{ }
    public enum EnumGEM_COACK{ }
    public enum EnumGEM_GRXLACK{ }
    public enum EnumGEM_DRRACK{ }
    public enum EnumGEM_WRACK{ }
    public enum EnumGEM_RRACK_S20{ }
    public enum EnumGEM_QRXLEACK{ }
    public enum EnumGEM_QREACK{ }
    public enum EnumGEM_PREACK{ }
    public enum EnumGEM_PSRACK{ }
    public enum EnumGEM_QPRKEACK{ }
    public enum EnumGEM_PECEACK{ }
    public enum EnumGEM_PSREACK{ }
    public enum EnumGEM_ITEMACK{ }


    #endregion

    public enum CPACK
    {
        PARAM_NAME_DOES_NOT_EXIST = 1,
        ILLEGAL_VALUE_SPECIFIED_FOR_CPVAL = 2,
        ILLEGAL_FORMAT_SPECIFIED_FOR_CPVal = 3,
        OTHER_EQ_SPECIFIC_ERROR = 4,
    }

    // <note>  remote command acknowledge  / Example: "B:1 0x00" / Used by: S2F42 S2F50
    public enum EnumGemHCACK
    {
        HAS_BEEN_PERFORMED = 0x00, // ok, completed
        NOT_EXIST = 0x01, // invalid command
        CANNOT_PERFORM_NOW = 0x02, //cannot do now
        INVALID_PARAMM = 0x03, //parameter error
        WILL_BE_PERFORMED = 0x04, //initiated for asynchronous completion
        REJECTED = 0x05, // rejected, already in desired condition
        NOT_EXISTS_OBJECT = 0x05 //  invalid object
    }
}

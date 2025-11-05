using LogModule;
using System;
using System.Collections.Generic;
using ProberErrorCode;

namespace ProberInterfaces
{
    public enum EnumMotionProviderType
    {
        UNDEFINED = 0,
        MPI = 1,
        SCL = 2,
        LAST = SCL + 1
    }
    public enum EnumMoterType
    {
        SERVO = 0,
        STEPPER = 1

    }
    public enum EnumAmpActionType
    {
        NONE = 0,
        CMD_ACT = 1
    }
    public enum EnumMCInputs
    {
        Index = 0,
        X1 = 1,
        X2 = 2,
        NOT = 8,
        POT = 7,
        X3 = 3,
        X4 = 4,
        X5 = 5,
        X6 = 6
    }
    //public enum EnumAxisState
    //{
    //    NO_EVENT = 0,
    //    NEW_FRAME = 2,
    //    CHECK_FRAMES = 6,
    //    STOP_EVENT = 8,
    //    ESTOP_EVENT = 10,
    //    ABORT_EVENT = 14,
    //}
    public enum EnumTrjType
    {
        Normal = 0,
        Probing = 1,
        Homming = 2,
        Emergency = 3
    }
    public enum EnumMotionResult
    {
        NO_ERR = 0,
        MessageOK = 1,
        ERR_PORT_INVALID = 11,
        ERR_PORT_FATAL = 12,
        ERR_DEVICE_NOTHING = 13,
        ERR_DEVICE_IDINVALID = 14,
        ERR_FAIL_EXECUTE = 15,
        ERR_PARAM_INVALID = 16,
        ERR_PARAM_OUTOFRANGE = 100,
        ERR_COMM_NORESPONSE = 101,
        ERR_COMM_TXEMPTY = 102,
        ERR_COMM_TXFULL = 103,
        ERR_TX_BADCHECKSUM = 110,
        ERR_RX_BADCHECKSUM = 111,
        ERR_COMM_NACK = 112,
        ERR_COMM_NOCHECKSUM = 113,
        ERR_COMM_DONTNEEDCHECKSUM = 114,
        ERR_MOTION_STOP = -1,

        ERR_UNKNOWN = -999
    }

    public enum FilterAlgorithm
    {
        FilterAlgorithmINVALID = -1,

        FilterAlgorithmNONE,
        FilterAlgorithmPID,
        FilterAlgorithmPIV,
        FilterAlgorithmPIV1,
        FilterAlgorithmUSER,

        FilterAlgorithmEND,
        FilterAlgorithmFIRST = FilterAlgorithmINVALID + 1
    };
    public enum EnumFeedbackType
    {
        QUAD_AB = 0,
        DRIVE = 1,
        SSI = 2
    }
    public enum CaptureState
    {
        CaptureStateINVALID = -1,

        CaptureStateIDLE,        /**< initial state newly created capture */
        CaptureStateARMED,       /**< Waiting for trigger ( single-shot or 
									 auto-arm mode). */
        CaptureStateCAPTURED,    /**< Event was captured, waiting to be cleared. */
        CaptureStateCLEAR,       /**< Event has been cleared. */

        CaptureStateEND,
        CaptureStateFIRST = CaptureStateINVALID + 1
    }

    //public const short NO_EVENT = 0;
    //public const short NEW_FRAME = 2;
    //public const short CHECK_FRAMES = 6;
    //public const short STOP_EVENT = 8;
    //public const short E_STOP_EVENT = 10;
    //public const short ABORT_EVENT = 14;
    public enum EnumEventActionType
    {
        ActionINVALID = -1,

        ActionNONE,
        ActionTRIGGERED_MODIFY,
        ActionSTOP,
        ActionABORT,
        ActionE_STOP,
        ActionE_STOP_ABORT,
        ActionE_STOP_CMD_EQ_ACT,
        ActionE_STOP_MODIFY,

        /* Differing priority support for E_STOP_MODIFY,
           PRIORITY_0 is the highest and the default
         */
        ActionE_STOP_MODIFY_PRIORITY_0 = ActionE_STOP_MODIFY,
        ActionE_STOP_MODIFY_PRIORITY_1,
        ActionE_STOP_MODIFY_PRIORITY_2,
        ActionE_STOP_MODIFY_PRIORITY_3,
        ActionE_STOP_MODIFY_PRIORITY_4,
        ActionE_STOP_MODIFY_PRIORITY_5,
        ActionE_STOP_MODIFY_PRIORITY_6,
        ActionE_STOP_MODIFY_PRIORITY_7,

        ActionDONE,
        ActionSTART,
        ActionRESUME,
        ActionRESET,
        ActionCANCEL_REPEAT,

        ActionEND,
        ActionFIRST = ActionINVALID + 1
    }
    public enum EnumBrakeMode
    {
        NONE = 0,
        DELAY = 1
    }
    public enum EnumPulseType
    {
        MotorStepperPulseTypeINVALID = -1,

        MotorStepperPulseTypeSTEP,
        MotorStepperPulseTypeDIR,

        MotorStepperPulseTypeCW,
        MotorStepperPulseTypeCCW,

        MotorStepperPulseTypeQUADA,
        MotorStepperPulseTypeQUADB,

        MotorStepperPulseTypeLAST,
        MotorStepperPulseTypeFIRST = MotorStepperPulseTypeINVALID + 1
    }
    public enum EnumOutputType
    {
        MotorIoTypeINVALID = -1,

        MotorIoTypeOUTPUT,

        MotorIoTypePULSE_A,
        MotorIoTypePULSE_B,
        MotorIoTypeCOMPARE_0,
        MotorIoTypeCOMPARE_1,
        MotorIoTypeSOURCE5,
        MotorIoTypeBRAKE,
        MotorIoTypeSSI_CLOCK0,
        MotorIoTypeSSI_CLOCK1,
        MotorIoTypeOUTPUT_DISABLED,

        MotorIoTypeSOURCE10,
        MotorIoTypeSOURCE11,
        MotorIoTypeSOURCE12,
        MotorIoTypeSOURCE13,
        MotorIoTypeSOURCE14,
        MotorIoTypeSOURCE15,

        MotorIoTypeINPUT,

        MotorIoTypeLAST,
        MotorIoTypeFIRST = MotorIoTypeINVALID + 1,

        MotorIoTypeSOURCE1 = MotorIoTypePULSE_A,
        MotorIoTypeSOURCE2 = MotorIoTypePULSE_B,
        MotorIoTypeSOURCE3 = MotorIoTypeCOMPARE_0,
        MotorIoTypeSOURCE4 = MotorIoTypeCOMPARE_1,
        MotorIoTypeSOURCE6 = MotorIoTypeBRAKE,
        MotorIoTypeSOURCE7 = MotorIoTypeSSI_CLOCK0,
        MotorIoTypeSOURCE8 = MotorIoTypeSSI_CLOCK1,
        MotorIoTypeSOURCE9 = MotorIoTypeOUTPUT_DISABLED
    }
    //public enum EnumReturnCodes
    //{
    //    ReturnCodeINVALID = -1,

    //    ReturnCodeOK,

    //    ReturnCodeARG_INVALID,
    //    ReturnCodePARAM_INVALID,
    //    ReturnCodeHANDLE_INVALID,

    //    ReturnCodeNO_MEMORY,

    //    ReturnCodeOBJECT_FREED,
    //    ReturnCodeOBJECT_NOT_ENABLED,
    //    ReturnCodeOBJECT_NOT_FOUND,
    //    ReturnCodeOBJECT_ON_LIST,
    //    ReturnCodeOBJECT_IN_USE,

    //    ReturnCodeTIMEOUT,

    //    ReturnCodeUNSUPPORTED,
    //    ReturnCodeFATAL_ERROR,

    //    ReturnCodeFILE_CLOSE_ERROR,
    //    ReturnCodeFILE_OPEN_ERROR,
    //    ReturnCodeFILE_READ_ERROR,
    //    ReturnCodeFILE_WRITE_ERROR,
    //    ReturnCodeFILE_MISMATCH,


    //    ReturnCodeLAST,
    //    ReturnCodeFIRST = ReturnCodeINVALID + 1
    //}

    public enum EnumDedicateInputs
    {
        DedicateInputINVALID = -1,

        DedicateInputMOTOR_IO_0,
        DedicateInputMOTOR_IO_1,
        DedicateInputMOTOR_IO_2,
        DedicateInputMOTOR_IO_3,
        DedicateInputMOTOR_IO_4,
        DedicateInputMOTOR_IO_5,
        DedicateInputMOTOR_IO_6,
        DedicateInputMOTOR_IO_7,
        DedicateInputHOME,
        DedicateInputINDEX,
        DedicateInputLIMIT_HW_NEG,
        DedicateInputLIMIT_HW_POS,
        DedicateInputGLOBAL,
        DedicateInputINDEX_SECONDARY,

        DedicateInputLAST,
        DedicateInputFIRST = DedicateInputINVALID + 1,

        DedicateInputCOUNT = DedicateInputLAST
    }

    public enum EnumEdgeType
    {
        EdgeTypeINVALID = -1,

        EdgeTypeNONE,
        EdgeTypeRISING,
        EdgeTypeFALLING,
        EdgeTypeEITHER,

        EdgeTypeLAST,
        EdgeTypeFIRST = EdgeTypeINVALID + 1
    }

    public enum EnumInputLevel
    {
        Normal = 0,
        Inverted = 1
    }

    public enum EnumIndexConfig
    {
        HOME_ONLY = 0,
        LOWHOME_AND_INDEX = 1,
        INDEX0_ONLY = 2,
        HIHOME_AND_INDEX = 3,
        INDEX1_ONLY = 4
    }
    /// <summary>
    /// DS402 Object 0x20FD 
    /// </summary>
    public enum EnumDigitalInputs
    {
        RLS = 0,
        FLS = 1,
        HomeSwitch = 2,
        DI1 = 16,
        DI_LAST = 31,
    }
    public enum EnumMotorDedicatedIn
    {
        MotorDedicatedInINVALID = -1,

        MotorDedicatedInAMP_FAULT = 0,
        MotorDedicatedInBRAKE_APPLIED = 1,
        MotorDedicatedInHOME = 2,
        MotorDedicatedInLIMIT_HW_POS = 3,
        MotorDedicatedInLIMIT_HW_NEG = 4,
        MotorDedicatedInINDEX_PRIMARY = 5,
        MotorDedicatedInFEEDBACK_FAULT = 6,
        MotorDedicatedInDRIVE_CAPTURED = 7,
        MotorDedicatedInHALL_A = 8,

        MotorDedicatedInHALL_B = 9,
        MotorDedicatedInHALL_C = 10,
        MotorDedicatedInAMP_ACTIVE = 11,
        MotorDedicatedInINDEX_SECONDARY = 12,
        MotorDedicatedInDRIVE_WARNING = 13,
        MotorDedicatedInDRIVE_STATUS_9 = 14,


        #region Elmo 
        //엘모용 
        MotorDedicatedIn_HOMERISING = 1,
        MotorDedicatedIn_HOMEFALLING = 2,
        MotorDedicatedIn_FLS_RISING = 5,
        MotorDedicatedIn_FLS_FALLING = 6,
        MotorDedicatedIn_RLS_RISING = 7,
        MotorDedicatedIn_RLS_FALLING = 8,
        MotorDedicatedIn_1R = 9,
        MotorDedicatedIn_1F = 10,
        MotorDedicatedIn_2R = 11,
        MotorDedicatedIn_2F = 12,
        MotorDedicatedIn_3R = 13,
        MotorDedicatedIn_3F = 14,
        #endregion

        MotorDedicatedInDRIVE_STATUS_10 = 15,
        MotorDedicatedInFEEDBACK_FAULT_PRIMARY = 16,
        MotorDedicatedInFEEDBACK_FAULT_SECONDARY = 17,

        MotorDedicatedInEND,
        MotorDedicatedInFIRST = MotorDedicatedInINVALID + 1,
        MotorDedicatedInCOUNT = MotorDedicatedInEND - MotorDedicatedInFIRST
    }
    public enum EnumActionType
    {
        ActionINVALID = -1,
        ActionNONE = 0,
        ActionFIRST = 0,
        ActionTRIGGERED_MODIFY = 1,
        ActionSTOP = 2,
        ActionABORT = 3,
        ActionE_STOP = 4,
        ActionE_STOP_ABORT = 5,
        ActionE_STOP_CMD_EQ_ACT = 6,
        ActionE_STOP_MODIFY = 7,
        ActionE_STOP_MODIFY_PRIORITY_0 = 7,
        ActionE_STOP_MODIFY_PRIORITY_1 = 8,
        ActionE_STOP_MODIFY_PRIORITY_2 = 9,
        ActionE_STOP_MODIFY_PRIORITY_3 = 10,
        ActionE_STOP_MODIFY_PRIORITY_4 = 11,
        ActionE_STOP_MODIFY_PRIORITY_5 = 12,
        ActionE_STOP_MODIFY_PRIORITY_6 = 13,
        ActionE_STOP_MODIFY_PRIORITY_7 = 14,
        ActionDONE = 15,
        ActionSTART = 16,
        ActionRESUME = 17,
        ActionRESET = 18,
        ActionCANCEL_REPEAT = 19,
        ActionEND = 20
    }
    public enum EnumAxisState
    {
        INVALID = -1,


        IDLE,
        MOVING,
        STOPPING,
        STOPPED,
        STOPPING_ERROR,
        ERROR,
        DISABLED,
        END,
        //StateFIRST = INVALID + 1,
        //StateLAST = END,
    }

    public enum EnumAxisActionSource
    {
        ActionSourceINVALID = -1,

        ActionSourceUSER,
        ActionSourceCONTROLLER,

        ActionSourceEND,
        ActionSourceFIRST = ActionSourceINVALID + 1
    }
    public enum EnumMotionBaseReturnCode
    {
        InputRetreiveError = -30,
        ThreeLegDnError = -29,
        ThreeLegUpError = -28,
        IsInpositionError = -27,
        GetErrorPositionError = -26,
        ErrorPositionLimitError = -25,
        StopError = -24,
        TimeOutError = -23,
        DeInitError = -22,
        WaitforFunctionError = -21,
        GetVelocityError = -20,
        GetCommandPulseError = -19,
        GetCommandPositionError = -18,
        GetAxisStatusError = -17,
        GetAxisStateError = -16,
        GetActualPulseError = -15,
        GetActualPositionError = -14,
        DisbleAxisrError = -13,
        EnableAxisrError = -12,
        AmpfaultClearError = -11,
        SetPositionError = -10,
        WaitforMotionDoneError = -9,
        SWPOSLimitError = -8,
        SWNEGLimitError = -7,
        InitError = -6,
        HomingError = -5,
        VelocityMoveError = -4,
        RelMoveError = -3,
        AbsMoveError = -2,
        FatalError = -1,
        ReturnCodeOK = 0,

    }
    //public enum EnumMotionReturn
    //{
    //    NO_ERR = 1,
    //    ERR_PORT_INVALID = 1,
    //    ERR_PORT_FATAL = 2,
    //    ERR_DEVICE_NOTHING = 3,
    //    ERR_DEVICE_IDINVALID = 4,
    //    ERR_FAIL_EXECUTE = 5,
    //    ERR_PARAM_INVALID = 6,
    //    ERR_PARAM_OUTOFRANGE = 100,
    //    ERR_COMM_NORESPONSE = 101,
    //    ERR_COMM_TXEMPTY = 102,
    //    ERR_COMM_TXFULL = 103,
    //    ERR_COMM_BADCHECKSUM = 110,
    //    ERR_COMM_NACK = 111,
    //    ERR_COMM_NOCHECKSUM = 112,
    //    ERR_COMM_DONTNEEDCHECKSUM = 113,

    //    ERR_UNKNOWN = 999
    //}

    //  [StructLayout(LayoutKind::Sequential)]//,Pack=8)]
    //  public value struct Trajectory
    //  {
    //      public:
    //double velocity;
    //      double acceleration;
    //      double deceleration;
    //      double jerkPercent;
    //      double accelerationJerk;
    //      double decelerationJerk;
    //  };

    //  [StructLayout(LayoutKind::Sequential)]
    //  public value struct MotionParams
    //  {
    //      public:
    //long motionId;
    //      double position;
    //      double finalVelocity;
    //      double delay;
    //  };
    public enum EnumPolarity
    {
        ACTIVE_LOW = 0,
        ACTIVE_HIGH = 1
    }
    public interface IMotionBase
    {
        List<AxisStatus> AxisStatusList { get; }
        int PortNo { get; }
        bool DevConnected { get; set; }
        bool IsMotionReady { get; }

        int InitMotionProvider(string ctrlNo);
        int SetMotorAmpEnable(AxisObject axis, bool turnAmp);
        int GetMotorAmpEnable(AxisObject axis, ref bool turnAmp);
        int DeInitMotionService();
        int ClearUserLimit(AxisObject axis);

        int GetCmdPosition(AxisObject axis, ref double cmdpos);
        //int GetActPosition(AxisObject axis, ref double actpos);
        int AbsMove(AxisObject axis, double abspos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1.0);
        int AbsMove(AxisObject axis, double abspos, double finalVel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1.0);
        int AbsMove(AxisObject axis, double abspos, double vel, double acc);
        int AbsMove(AxisObject axis, double abspos, double vel, double acc, double dcc);
        int RelMove(AxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1.0);
        int RelMove(AxisObject axis, double pos, double vel, double acc);
        int RelMove(AxisObject axis, double pos, double vel, double acc, double dcc);
        int ConfigCapture(AxisObject axis, EnumMotorDedicatedIn input);
        int DisableCapture(AxisObject axis);
        int GetCaptureStatus(int axisindex, AxisStatus status);
        int VMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int JogMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1.0);
        int JogMove(AxisObject axis, double velocity, double accel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1.0);
        int Homming(AxisObject axis);
        int Homming(AxisObject axis, bool reverse, EnumIndexConfig input, double homeoffset);
        int Homming(AxisObject axis, bool reverse, int input, double homeoffset);
        int Homming(AxisObject axis, bool reverse, EnumMCInputs input, double homeoffset);
        // int Homming(AxisObject axis, bool reverse, EnumIndexConfig input, double homeoffset);
        int VMoveStop(AxisObject axis);
        int Stop(AxisObject axis);
        int Halt(AxisObject axis, double value);
        int Pause(AxisObject axis);
        int Resume(AxisObject axis);
        int SetOverride(AxisObject axis, double ovrd);
        int LinInterpolation(AxisObject[] axes, double[] poss);
        int ResultValidate(int retcode);
        double GetAccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1.0);
        double GetDeccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1.0);
        int GetVelocity(AxisObject axis, EnumTrjType trjtype, ref double vel, double ovrd = 1.0);
        int EnableAxis(AxisObject axis);
        int DisableAxis(AxisObject axis);
        int WaitForVMoveAxisMotionDone(AxisObject axis, long timeout = 0);
        int WaitForAxisMotionDone(AxisObject axis, long timeout = 0);
        int WaitForAxisMotionDone(AxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0);
        int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0);
        int WaitForAxisEvent(AxisObject axis, EnumAxisState waitfor, double distlimit, long timeout = -1);
        int GetAxisStatus(AxisObject axis);
        int GetAxisState(AxisObject axis, ref int state);
        int GetAxisInputs(AxisObject axis, ref uint instatus);
        int IsMotorEnabled(AxisObject axis, ref bool val);
        int IsMoving(AxisObject axis, ref bool val);
        int IsInposition(AxisObject axis, ref bool val);
        int ApplyAxisConfig(AxisObject axis);
        //bool IsHoming(AxisObject axis);
        bool HasAlarm(AxisObject axis);
        int GetAlarmCode(AxisObject axis, ref ushort alarmcode);
        int GetActualPosition(AxisObject axis, ref double pos);
        int GetCommandPosition(AxisObject axis, ref double pos);
        int GetActualPulse(AxisObject axis, ref int pos);
        int GetCommandPulse(AxisObject axis, ref int pos);
        int GetAuxPulse(AxisObject axis, ref int pos);
        int SetPosition(AxisObject axis, double pos);
        int SetPulse(AxisObject axis, double pos);
        int AlarmReset(AxisObject axis);

        int GetPosError(AxisObject axis, ref double poserr);
        bool GetIOAmpFault(AxisObject axis);
        bool GetIOHome(AxisObject axis);
        bool GetIONegLim(AxisObject axis);
        bool GetIOPosLim(AxisObject axis);
        int SetSWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity);
        int SetHWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity);
        int SetSWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity);
        int SetHWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity);
        int SetSwitchAction(AxisObject axis,
            EnumDedicateInputs input, EnumEventActionType action, EnumInputLevel reverse);
        int ClearUserLimit(int axisNumber);
        int SetZeroPosition(AxisObject axis);
        int AmpFaultClear(AxisObject axis);
        int Abort(AxisObject axis);
        int SetLimitSWNegAct(AxisObject axis, EnumEventActionType action);
        int SetLimitSWPosAct(AxisObject axis, EnumEventActionType action);
        bool GetHomeSensorState(AxisObject axis);
        int WaitForHomeSensor(AxisObject axis, long timeout = 0);
        int SetSettlingTime(AxisObject axis, double settlingTime);
        int SetFeedrate(AxisObject axis, double normfeedrate, double pausefeedrate);
        int NotchFinding(AxisObject axis, EnumMotorDedicatedIn input);
        int StartScanPosCapt(AxisObject axis, ProberInterfaces.EnumMotorDedicatedIn MotorDedicatedIn);
        int StopScanPosCapt(AxisObject axis);
        int OriginSet(AxisObject axis, double pos);
        int ForcedZDown(AxisObject axis);
        int SetDualLoop(bool dualloop);
        int SetLoadCellZero();

        int SetMotorStopCommand(AxisObject axis, string setevent, EnumMotorDedicatedIn input);
        int IsThreeLegUp(AxisObject axis, ref bool isthreelegup);
        int IsThreeLegDown(AxisObject axis, ref bool isthreelegdn);

        int IsFls(AxisObject axis, ref bool isfls);
        int IsRls(AxisObject axis, ref bool isrls);

        int ChuckTiltMove(AxisObject axis, double offsetz0, double offsetz1, double offsetz2, double abspos, double vel, double acc);
        long UpdateProcTime { get; }
    }

    public static class EnumReturnCodesConverter
    {
        // TODO : 모든 EventCode가 정의되면 없애는 것?
        public static EventCodeEnum EnumReturnCodeToEventCodeConvert(int retcode, EventCodeEnum ErrorCodeEnum = EventCodeEnum.UNDEFINED)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            EnumMotionBaseReturnCode returncode = (EnumMotionBaseReturnCode)retcode;

            if (returncode == EnumMotionBaseReturnCode.ReturnCodeOK)
            {
                retval = EventCodeEnum.NONE;
            }
            else
            {
                retval = ConvertToEventCode(retcode);
            }

            return retval;
        }

        public static EventCodeEnum ConvertToEventCode(int retval)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                switch ((EnumMotionBaseReturnCode)retval)
                {
                    case EnumMotionBaseReturnCode.ErrorPositionLimitError:
                        retVal = EventCodeEnum.MOTION_ERROR_LIMIT_POSITION_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.StopError:
                        retVal = EventCodeEnum.MOTION_STOP_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.TimeOutError:
                        retVal = EventCodeEnum.MOTION_TIMEOUT_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.DeInitError:
                        retVal = EventCodeEnum.MOTION_DEINIT_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.WaitforFunctionError:
                        retVal = EventCodeEnum.MOTION_EVENTDONE_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.GetVelocityError:
                        retVal = EventCodeEnum.MOTION_GET_VELOCITY_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.GetCommandPulseError:
                        retVal = EventCodeEnum.MOTION_GET_COMMAND_PULSE_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.GetCommandPositionError:
                        retVal = EventCodeEnum.MOTION_GET_COMMAND_POSITION_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.GetAxisStatusError:
                        retVal = EventCodeEnum.MOTION_GET_AXIS_STATUS_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.GetAxisStateError:
                        retVal = EventCodeEnum.MOTION_GET_AXIS_STATE_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.GetActualPulseError:
                        retVal = EventCodeEnum.MOTION_GET_ACTUAL_PULSE_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.GetActualPositionError:
                        retVal = EventCodeEnum.MOTION_GET_ACTUAL_POSITION_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.DisbleAxisrError:
                        retVal = EventCodeEnum.MOTION_DISABLE_AXIS_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.EnableAxisrError:
                        retVal = EventCodeEnum.MOTION_ENABLE_AXIS_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.AmpfaultClearError:
                        retVal = EventCodeEnum.MOTION_AMPFAULT_CLEAR_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.SetPositionError:
                        retVal = EventCodeEnum.MOTION_SETPOSITION_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.WaitforMotionDoneError:
                        retVal = EventCodeEnum.MOTION_MOTIONDONE_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.SWPOSLimitError:
                        retVal = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.SWNEGLimitError:
                        retVal = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.InitError:
                        retVal = EventCodeEnum.MOTION_INIT_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.HomingError:
                        retVal = EventCodeEnum.MOTION_HOMING_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.VelocityMoveError:
                        retVal = EventCodeEnum.MOTION_VMOVING_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.RelMoveError:
                        retVal = EventCodeEnum.MOTION_REL_MOVING_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.AbsMoveError:
                        retVal = EventCodeEnum.MOTION_ABS_MOVING_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.FatalError:
                        retVal = EventCodeEnum.MOTION_FATAL_ERROR;
                        break;
                    case EnumMotionBaseReturnCode.ReturnCodeOK:
                        retVal = EventCodeEnum.NONE;
                        break;
                    default:
                        retVal = EventCodeEnum.UNDEFINED;
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                LoggerManager.Debug($"ConvertToEventCode() : int value : {retval} To EventCode : {retVal}");
            }

            return retVal;
        }
    }

    public class MoveParam
    {
        private AxisObject _Axis;

        public AxisObject Axis
        {
            get { return _Axis; }
            set { _Axis = value; }
        }

        private double _TargetPos;

        public double TargetPos
        {
            get { return _TargetPos; }
            set { _TargetPos = value; }
        }

        private EnumTrjType _TrjType;

        public EnumTrjType TrjType
        {
            get { return _TrjType; }
            set { _TrjType = value; }
        }

        public MoveParam(AxisObject axis, double pos, EnumTrjType trj)
        {
            try
            {
            _Axis = axis;
            _TargetPos = pos;
            _TrjType = trj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public MoveParam()
        {

        }
    }
}

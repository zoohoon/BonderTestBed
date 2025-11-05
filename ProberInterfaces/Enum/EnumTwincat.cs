using System;
using System.Runtime.InteropServices;

namespace ProberInterfaces.Enum
{
    public enum EnumThreePodAxis
    {
        Z1 = 0,
        Z2 = 1,
        Z3 = 2,

        THREEPOD_AXIS_LAST = Z3 + 1,
    }

    public enum EnumAxisStatusDWord
    {
        //0 Operational Axis is ready for operation
        AxisReady = 0,

        //1 Homed Axis has been referenced/ homed ("Axis calibrated")
        Homed = 1,
        //2 NotMoving Axis is logically stationary ("Axis not moving") 
        NotMoving = 2,
        //3 InPositionArea Axis is in position window (physical feedback) 
        InpositionArea = 3,
        //4 InTargetPosition Axis is at target position (PEH) (physical feedback) 
        InTargetPosition = 4,
        //5 Protected Axis is in a protected operating mode (e.g. as a slave axis) 
        ProtectedOpMode = 5,
        //6 ErrorPropagationDelayed Axis signals an error pre warning (from TC 2.11) 
        ErrorPropagationDelayed = 6,
        //7 HasBeenStopped Axis has been stopped or is presently executing a stop 
        HasBeenStopped = 7,
        //8 HasJob Axis has instructions, is carrying instructions out 
        HasJob = 8,
        //9 PositiveDirection Axis moving to logically larger values 
        PositiveDirection = 9,
        //10 NegativeDirection Axis moving to logically smaller values 
        NegativeDirection = 10,
        //11 HomingBusy Axis referenced ("Axis being calibrated") 
        HomingBusy = 11,
        //12 ConstantVelocity Axis has reached its constant velocity or rotary speed
        ConstantVelocity = 12,

        //13 Compensating Section compensation passive[0]/active[1] (s. "MC_MoveSuperImposed") 
        MC_MoveSuperImposed = 13,
        //14 ExtSetPointGenEnabled External setpoint generator enabled
        ExtSetPointGenEnabled = 14,

        //15  Operating mode not yet executed (Busy). Not implemented yet! 
        //16 ExternalLatchValid External latch value or sensing switch has become valid
        ExternalLatchValid = 16,
        //17 NewTargetPos Axis has a new target position or a new velocity 
        NewTargetPos = 17,
        //18  Axis is not at target position or cannot reach the target position (e.g. stop). Not implemented yet! 

        //19 ContinuousMotion Axis has target position (±) endless
        ContinuousMotion = 19,

        //20 ControlLoopClosed Axis is ready for operation and axis control loop is closed (e.g. position control) 
        ControlLoopClosed = 20,
        //21 CamTableQueued CAM table is queued for  "Online Change" and waiting for activation 
        CamTableQueued = 21,
        //22 CamDataQueued CAM data (only MF) are queued for  "Online Change" and waiting for activation 
        CamDataQueued = 22,
        //23 CamScalingPending CAM scaling are queued for  "Online Change" and waiting for activation
        CamScalingPending = 23,

        //24 CmdBuffered Following command is queued in then command buffer (s. Buffer Mode)
        //(from TwinCAT V2.10 Build 1311) 
        CmdBuffered = 24,

        //25 PTPmode Axis in PTP mode (no slave, no NCI axis, no FIFO axis) (from TC 2.10 Build 1326) 
        PTPmode = 25,
        //26 SoftLimitMinExceeded Position software limit switch minimum is exceeded (from TC 2.10 Build 1327) 
        SoftLimitMinExceeded = 26,
        //27 SoftLimitMaxExceeded Position software limit switch maximum is exceeded (from TC 2.10 Build 1327) 
        SoftLimitMaxExceeded = 27,
        //28 DriveDeviceError Hardware drive device error (no warning), interpretation only possible when drive is data exchanging, 
        //e.g. EtherCAT "OP"-state (from TC 2.10 Build 1326) 
        DriveDeviceError = 28,
        //29 MotionCommandsLocked Axis is locked for motion commands (TcMc2) 
        MotionCommandsLocked = 29,
        //30 IoDataInvalid IO data invalid (e.g. 'WcState' or 'CdlState') 
        IoDataInvalid = 30,
        //31 Error Axis is in a fault state 
        Error = 31,

    }

    //public enum EnumTwinCatAxisState
    //{
    //    MC_AXISSTATE_UNDEFINED,
    //    MC_AXISSTATE_DISABLED,
    //    MC_AXISSTATE_STANDSTILL,
    //    MC_AXISSTATE_ERRORSTOP,
    //    MC_AXISSTATE_STOPPING,
    //    MC_AXISSTATE_HOMING,
    //    MC_AXISSTATE_DISCRETEMOTION,
    //    MC_AXISSTATE_CONTINOUSMOTION,
    //    MC_AXISSTATE_SYNCHRONIZEDMOTION
    //}
    public enum EnumTwinCatAxisState
    {
        AXISSTATE_INACTIVE,
        AXISSTATE_RUNNING,
        AXISSTATE_OVERRIDE_ZERO,
        AXISSTATE_PHASE_VELOCONST,
        AXISSTATE_PHASE_ACCPOS,
        AXISSTATE_PHASE_PHASE_ACCNEG,

    }
    [StructLayout(LayoutKind.Sequential)]
    public class TCTrajectoryParams
    {
        public double Velocity;
        public double Acc;
        public double Dcc;
        public double Jerk;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class VirtualAxisRef
    {
        public int AxisIndex;

        public double HomeOffset;
        public double SWPosLimit;
        public double SWNegLimit;

    }

    [StructLayout(LayoutKind.Sequential)]
    public class CDXMachineStatus
    {

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)EnumThreePodAxis.THREEPOD_AXIS_LAST)]
        public UInt32[] AxisState = new UInt32[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)EnumThreePodAxis.THREEPOD_AXIS_LAST)]
        public UInt32[] bAxisEnable = new UInt32[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)EnumThreePodAxis.THREEPOD_AXIS_LAST)]
        public double[] dblActPos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)EnumThreePodAxis.THREEPOD_AXIS_LAST)]
        public double[] dblTargetPos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)EnumThreePodAxis.THREEPOD_AXIS_LAST)]
        public UInt32[] Inposition = new UInt32[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)EnumThreePodAxis.THREEPOD_AXIS_LAST)]
        public UInt32[] bAxisMotionDone = new UInt32[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)EnumThreePodAxis.THREEPOD_AXIS_LAST)]
        public double[] dblSensVal = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

        public UInt32 bFSensValZeroSet;

        public UInt32 bZDualLoopEnable;

        public UInt32 bAxisConfigDone;

        public UInt32 bAxisHomingDone;

        public UInt32 bAxisHomingFinalDone;
    }

    //[StructLayout(LayoutKind.Sequential)]

    //public struct CDXMachineStatus
    //{
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    //    public UInt32[] u32AxisState;

    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    //    public UInt32[] u32AxisStatus;

    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    //    public UInt32[] u32AxisErrCode;

    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    //    public double[] dblActPos;

    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    //    public double[] dblTargetPos;
    //}
}

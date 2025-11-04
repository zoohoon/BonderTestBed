using System;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Param;
    using System.ComponentModel;
    using System.ServiceModel;
    [ServiceContract(CallbackContract = typeof(IMotionManagerCallback))]
    public interface IMotionManager : IFactoryModule, INotifyPropertyChanged, IModule, IHasSysParameterizable
    {
        //ProbeAxes LoaderAxes { get; set; }

        //ProbeAxes StageAxes { get; set; }
        int ForcedZDown();
        IMotionProvider GetMotionProvider();
        LoaderAxes LoaderAxes { get; set; }
        int Abort(ProbeAxisObject axis);
        StageAxes StageAxes { get; set; }
        IErrorCompensationManager ErrorManager { get; }


        //EventCodeEnum InitParamSet(string StageAxisParamFilePath, string LoaderAxisParamFilePath);

        //=> Common 
        [OperationContract]
        ProbeAxisObject GetAxis(EnumAxisConstants axis);
        int GetActualPos(EnumAxisConstants axisType, ref double pos);
        int GetRefPos(EnumAxisConstants axisType, ref double pos);
        int GetRefPos(ref double xpos, ref double ypos, ref double zpos, ref double tpos);
        int GetActualPoss(ref double xpos, ref double ypos, ref double zpos, ref double tpos);
        int GetCommandPos(ProbeAxisObject axis, ref double pos);
        int GetCommandPos(EnumAxisConstants axisType, ref double pos);
        int GetAuxPulse(ProbeAxisObject axis, ref int pos);
        int SetPosition(ProbeAxisObject probeAxisObject, double pos);
        bool GetIOHome(ProbeAxisObject axis);
        int SetSettlingTime(ProbeAxisObject axis, double settlingTime);
        int AmpFaultClear(EnumAxisConstants axis);
        int Stop(ProbeAxisObject axis);
        int DisableAxes();
        [OperationContract]
        EventCodeEnum VMove(EnumAxisConstants axis, double vel, EnumTrjType trjtype);
        EventCodeEnum VMove(ProbeAxisObject axis, double vel, EnumTrjType trjtype);
        //Task<ErrorCodeEnum> VMoveAsync(ProbeAxisObject axis, double vel, EnumTrjType trjtype);
        EventCodeEnum Homing(EnumAxisConstants axis);
        EventCodeEnum Homing(ProbeAxisObject axis);
        //Task<ErrorCodeEnum> HomingAsync(EnumAxisConstants axis);
        Task<EventCodeEnum> HomingAsync(ProbeAxisObject axis);

        //=> Loader Move
        EventCodeEnum LoaderSystemInit();

        //=> 삭제요청===========
        //EventCodeEnum HommingVaxis(ProbeAxisObject axis, double speed, EnumInputLevel inputLevel);
        //======================

        EventCodeEnum NotchFinding(AxisObject axis, EnumMotorDedicatedIn input);
        //=> Stage Move
        EventCodeEnum StageSystemInit();
        Task<EventCodeEnum> StageSystemInitAsync();



        EventCodeEnum AbsMove(EnumAxisConstants axis, double pos);

        EventCodeEnum AbsMove(ProbeAxisObject axis, double pos, double vel, double acc);
        EventCodeEnum AbsMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        EventCodeEnum AbsMove(ProbeAxisObject axis, double pos, double vel, double acc, double dcc);
        EventCodeEnum AbsMove(ProbeAxisObject axis, double pos);
        [OperationContract]
        EventCodeEnum RelMove(EnumAxisConstants axis, double pos);
        EventCodeEnum RelMove(ProbeAxisObject axis, double pos);
        EventCodeEnum RelMove(ProbeAxisObject axis, double pos, double vel, double acc);
        EventCodeEnum RelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        EventCodeEnum RelMove(ProbeAxisObject axis, double pos, double vel, double acc, double dcc);

        EventCodeEnum RelMoveZAxis(double pos, double vel, double acc);

        int AbsMoveAsync(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int AbsMoveAsync(ProbeAxisObject axis, double abspos, double vel, double acc);
        int RelMoveAsync(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int RelMoveAsync(ProbeAxisObject axis, double pos, double vel, double acc);
        int RelMoveAsync(double xpos, double ypos, double xvel, double xacc, double yvel, double yacc);

        EventCodeEnum AbsMoveWithSpeedRate(ProbeAxisObject axis, double pos, double vel, double acc, ProbingSpeedRateList FeedRateList);

        Task<int> WaitForMotionDoneAsync();

        EventCodeEnum StageMove(double xpos, double ypos);

        EventCodeEnum StageMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        EventCodeEnum StageMove(double xpos, double ypos, double zpos);
        EventCodeEnum StageMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        EventCodeEnum StageMove(double xpos, double ypos, double zpos, double cpos);
        EventCodeEnum StageMove(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        EventCodeEnum StageMove(double xpos, double xvel, double xacc, double ypos, double yvel, double yacc);
        int StageRelMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int StageRelMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int StageRelMove(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);

        int StageMoveAync(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int StageMoveAync(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int StageMoveAync(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int StageRelMoveAsync(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int StageRelMoveAsync(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int StageRelMoveAsync(double xpos, double ypos, double zpos, double cpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        [OperationContract]
        EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis, long timeout = 0);
        int WaitForAxisVMotionDone(ProbeAxisObject axis, long timeout = 0);
        int WaitForAxisMotionDone(ProbeAxisObject axis, long timeout = 0);
        int WaitForAxisMotionDone(ProbeAxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0);
        int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0);
        int SetOverride(ProbeAxisObject axis, double ovrd);
        int Resume(ProbeAxisObject axis);
        int SetFeedrate(ProbeAxisObject axis, double normfeedrate, double pausefeedrate);
        int EnableAxis(ProbeAxisObject axis);
        int DisableAxis(ProbeAxisObject axis);

        EventCodeEnum CheckSWLimit(EnumAxisConstants axistype, double position);
        double GetVel(EnumAxisConstants axistype);
        double GetAccel(EnumAxisConstants axistype);
        EventCodeEnum TiltingMove(double tz1pos, double tz2pos, double tz3pos);

        int StartScanPosCapt(AxisObject axis, ProberInterfaces.EnumMotorDedicatedIn MotorDedicatedIn);
        int StopScanPosCapt(AxisObject axis);
        EventCodeEnum ChuckTiltMove(double rpos, double ttpos);

        EventCodeEnum SetDualLoop(bool dualloop);
        EventCodeEnum SetLoadCellZero();
        EventCodeEnum HomingTaskRun(params EnumAxisConstants[] axes);
        EventCodeEnum SetMotorStopCommand(EnumAxisConstants axis, string setevent, EnumMotorDedicatedIn input);
        bool IsEmulMode(ProbeAxisObject axis);
        bool StageAxesBusy();
        [OperationContract]
        EventCodeEnum IsThreeLegUp(EnumAxisConstants axis, ref bool isthreelegup);
        [OperationContract]
        EventCodeEnum IsThreeLegDown(EnumAxisConstants axis, ref bool isthreelegdn);
        [OperationContract]
        EventCodeEnum IsFls(EnumAxisConstants axis, ref bool isfls);
        [OperationContract]
        EventCodeEnum IsRls(EnumAxisConstants axis, ref bool isrls);

        EventCodeEnum StageEMGStop();
        EventCodeEnum LoaderEMGStop();
        EventCodeEnum ScanJogMove(double xVel, double yVel, EnumTrjType trjtype);
        long UpdateProcTime { get; }
        long MaxUpdateProcTime { get; }
        [OperationContract]
        int InitHostService();
        void DeInitService();
        [OperationContract]
        bool IsServiceAvailable();

        EventCodeEnum StageEMGZDown();
        EventCodeEnum StageAxisLock();
        EventCodeEnum StageEMGAmpDisable();
        EventCodeEnum CheckCurrentofThreePod();
        double GetAxisTorque(EnumAxisConstants axisType);
        double GetAxisPos(EnumAxisConstants axisType);
        EventCodeEnum CalcZTorque(bool writelog);
    }
    public interface IMotionManagerCallback
    {
        [OperationContract]
        void OnAxisStateUpdated(ProbeAxisObject axis);

        [OperationContract]
        bool IsServiceAvailable();
    }
}

using System;

using System.ComponentModel;
using ProberErrorCode;

namespace ProberInterfaces
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    public interface IMotionProvider : INotifyPropertyChanged, IFactoryModule, IHasSysParameterizable
    {
        int Address { get; }
        Axes AxisProviders { get; set; }
        bool IsMotionReady { get; }

        ObservableCollection<IMotionBase> MotionProviders { get; set; }

        int AbsMove(AxisObject axis, double abspos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int AbsMove(AxisObject axis, double abspos, EnumTrjType trjtype = EnumTrjType.Normal, double velovrd = 1, double accovrd = 1, double dccovrd = 1);
        int AbsMove(AxisObject axis, double abspos, double vel, double acc);
        int AbsMove(AxisObject axis, double abspos, double vel, double acc, double dcc);
        int AlarmReset(AxisObject axis);
        int ApplyAxisConfig(AxisObject axis);
        int DeInitMotionService();
        int DisableAxis(AxisObject axis);
        int EnableAxis(AxisObject axis);
        double GetAccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1);
        // int GetActPosition(AxisObject axis, ref double actpos);
        int GetActualPosition(AxisObject axis, ref double pos);
        int GetActualPulse(AxisObject axis, ref int pos);
        int GetAlarmCode(AxisObject axis, ref ushort alarmcode);
        int GetAxisInputs(AxisObject axis, ref uint instatus);
        int GetAxisState(AxisObject axis, ref int state);
        int GetAxisStatus(AxisObject axis);
        int GetCmdPosition(AxisObject axis, ref double cmdpos);
        int GetCommandPosition(AxisObject axis, ref double pos);
        int GetCommandPulse(AxisObject axis, ref int pos);
        double GetDeccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1);
        bool GetIOAmpFault(AxisObject axis);
        bool GetIOHome(AxisObject axis);
        bool GetIONegLim(AxisObject axis);
        bool GetIOPosLim(AxisObject axis);
        int GetPosError(AxisObject axis, ref double poserr);
        int GetAuxPulse(AxisObject axis, ref int auxpulse);

        int GetVelocity(AxisObject axis, EnumTrjType trjtype, ref double vel, double ovrd = 1);
        int Halt(AxisObject axis, double value);
        bool HasAlarm(AxisObject axis);
        int Homming(AxisObject axis, bool reverse, EnumIndexConfig input, double homeoffset);
        int Homming(AxisObject axis, bool reverse, EnumMCInputs input, double homeoffset);
        //int Homming(AxisObject axis, bool reverse, int input, double homeoffset);
        int Homming(AxisObject axis);
        int InitMotionProvider(List<AxisObject> axes);
        EventCodeEnum InitMotionProvider(ObservableCollection<AxisObject> axes);
        //bool IsHoming(AxisObject axis);
        int IsInposition(AxisObject axis,ref bool val);
        int IsMotorEnabled(AxisObject axis,ref bool val);
        int IsMoving(AxisObject axis,ref bool val);
        int JogMove(AxisObject axis, double velocity, double accel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int LinInterpolation(AxisObject[] axes, double[] poss);
        int LoadMotionBaseDescriptor(string MotionBaseParamFilePath);
        int Pause(AxisObject axis);
        int RelMove(AxisObject axis, double pos, double vel, double acc);
        int RelMove(AxisObject axis, double pos, double vel, double acc, double dcc);

        int RelMove(AxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int RelMove(AxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double velovrd = 1, double accovrd = 1, double dccovrd = 1);
        int ResultValidate(int retcode);
        int Resume(AxisObject axis);
        int SetNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity);
        int SetOverride(AxisObject axis, double ovrd);
        int SetPosition(AxisObject axis, double pos);
        int SetPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity);
        int SetPulse(AxisObject axis, int pos);
        int SetSwitchAction(AxisObject axis, EnumDedicateInputs input, EnumEventActionType action, EnumInputLevel reverse);
        int VMoveStop(AxisObject axis);
        int Stop(AxisObject axis);
        int WaitForAxisEvent(AxisObject axis, EnumAxisState waitfor, double distlimit, long timeout = 0);
        int WaitForVMoveAxisMotionDone(AxisObject axis, long timeout = 0);
        int WaitForAxisMotionDone(AxisObject axis, long timeout = 0);
        int WaitForAxisMotionDone(AxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0);
        int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0);
        int ClearUserLimit(AxisObject axis);
        int AmpFaultClear(AxisObject axisObject);
        int SetZeroPosition(AxisObject axisObject);
        int Abort(AxisObject axisObject);
        int ConfigCapture(AxisObject axis, EnumMotorDedicatedIn input);
        int DisableCapture(AxisObject axis);
        int GetCaptureStatus(AxisObject axis);
        int VMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1);
        int SetLimitSWPosAct(AxisObject axis, EnumEventActionType action);
        int SetLimitSWNegAct(AxisObject axis, EnumEventActionType action);
        int SetSettlingTime(AxisObject axis, double settlingTime);
        int SetFeedrate(AxisObject axis, double normfeedrate, double pausefeedrate);
        EventCodeEnum NotchFinding(AxisObject axis, EnumMotorDedicatedIn input);

        int StartScanPosCapt(AxisObject axis, ProberInterfaces.EnumMotorDedicatedIn MotorDedicatedIn);
        int StopScanPosCapt(AxisObject axis);
        int ForcedZDown(AxisObject axis);
        int SetDualLoop(AxisObject axis,bool dualloop);
        int SetLoadCellZero(AxisObject axis);
        int SetMotorStopCommand(AxisObject axis, string setevent, EnumMotorDedicatedIn input);
        bool IsEmulMode(ProbeAxisObject axis);
        int IsThreeLegUp(AxisObject axis, ref bool isthreelegup);
        int IsThreeLegDown(AxisObject axis, ref bool isthreelegdn);
        int IsFls(AxisObject axis, ref bool isfls);
        int IsRls(AxisObject axis, ref bool isrls);
        int ChuckTiltMove(AxisObject axis, double offsetz0, double offsetz1, double offsetz2, double abspos, double vel, double acc);
        long UpdateProcTime { get; }
    }
}

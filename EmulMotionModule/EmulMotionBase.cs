using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using LogModule;

namespace EmulMotionModule
{
    public class EmulMotionBase : IMotionBase
    {
        string dirPath = @"C:\ProberSystem\EMUL\Parameters\SystemParam\ScanPositions.txt";
        private int ctrlNum;
        private int axisNum;

        private AxisObject CurrAxis;
        private bool isRunningCapture = false;
        bool bStopcaptureThread;

        public EmulMotionBase(string ctrlNum)
        {
            try
            {
                this.ctrlNum = Convert.ToInt32(ctrlNum);

                InitMotionProvider(ctrlNum);

                IsMotionReady = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EmulMotionBase(string ctrlNum, int axisnum)
        {
            try
            {
                this.ctrlNum = Convert.ToInt32(axisnum);
                axisNum = axisnum;

                InitMotionProvider(ctrlNum);

                IsMotionReady = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public List<AxisStatus> AxisStatusList { get; set; }
        private List<double> CapturePosList { get; set; }
        public int PortNo { get; set; }
        public bool DevConnected { get; set; }
        public bool IsMotionReady { get; set; }
        private long _UpdateProcTime;
        public long UpdateProcTime
        {
            get { return _UpdateProcTime; }
            set { _UpdateProcTime = value; }
        }
        //Interval 값을 이용하여 계산하는 StepPos에 x50 해주는 이유는 원래 Interval 값이 100이었어서 거리를 맞춰주기 위해서 해주는 것이다.
        private static double MONITORING_INTERVAL_INMS = 10;
        public int Abort(AxisObject axis)
        {
            LoggerManager.Debug($"Pause({axis.AxisIndex.Value}): Motion halted.");
            AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
            return 0;
        }
        private void LoadScanPositions()
        {
            try
            {
                //  74315.796
                //  75498.657
                //  84135.742
                //  85228.882
                //  94221.191
                //  95373.535
                //  104225.464
                //  105368.652
                //  134213.867
                //  135344.849
                //  144172.363
                //  145353.394
                //  154181.519
                //  155362.549
                //  164462.891
                //  165582.275
                //  174459.839
                //  175603.027
                //  184393.921
                //  185512.695
                //  194489.746
                //  195571.289
                //  244396.362
                //  245490.112
                //  254343.872
                //  255461.426
                //  264276.733
                //  265444.946
                //  274360.962
                //  275566.406
                //  304271.851
                //  305462.036
                //  314515.381
                //  315684.814

                if (Directory.Exists(Path.GetDirectoryName(dirPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dirPath));
                }

                FileInfo fileInfo = new FileInfo(dirPath);

                if (!fileInfo.Exists)
                {
                    WriteScanPositions();
                }

                StreamReader file = new System.IO.StreamReader(dirPath);
                CapturePosList = new List<double>();

                while (file.EndOfStream == false)
                {
                    var line = file.ReadLine();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    double pos = 0;

                    if (double.TryParse(line, out pos) == false)
                    {
                        break;
                    }

                    CapturePosList.Add(pos);
                }

                file.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void WriteScanPositions()
        {
            try
            {
                FileInfo fileInfo = new FileInfo(dirPath);
                if (!fileInfo.Exists)
                {
                    FileStream fs = fileInfo.Create();
                    fs.Close();
                }
                string line;
                StreamWriter file = new System.IO.StreamWriter(dirPath, false);

                for (int i = 0; i < CapturePosList.Count; i++)
                {
                    line = string.Format("{0,-15:0.000} ", CapturePosList[i]);
                    file.WriteLine(line);
                }
                file.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public int AbsMove(AxisObject axis, double abspos, double finalVel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            return AbsMove(axis, abspos, trjtype, ovrd);
        }
        public int AbsMove(AxisObject axis, double abspos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal;
            try
            {
                if (axis is AxisObject == false) //Check Null
                {
                    LoggerManager.Error($"AbsMove() : Axis is null.");
                    retVal = -1;
                }
                else if (AxisStatusList[axis.AxisIndex.Value].AxisEnabled == false)
                {
                    LoggerManager.Error($"AbsMove() : Axis not enabled. Axis={axis.Label.Value}");
                    retVal = -1;
                }
                else if (AxisStatusList[axis.AxisIndex.Value].AxisBusy == true)
                {
                    LoggerManager.Error($"AbsMove() : Axis busy. Axis={axis.Label.Value}");
                    //retVal = -1;
                    retVal = 0;
                }
                else if (abspos > axis.Param.PosSWLimit.Value)
                {
                    LoggerManager.Error($"AbsMove() : Positive SW Limit occurred. Axis={axis.Label.Value}, Target={abspos}, Limit={axis.Param.PosSWLimit.Value}");
                    retVal = -1;
                }
                else if (abspos < axis.Param.NegSWLimit.Value)
                {
                    LoggerManager.Error($"AbsMove() : Negative SW Limit occurred. Axis={axis.Label.Value}, Target={abspos}, Limit={axis.Param.NegSWLimit.Value}");
                    retVal = -1;
                }
                else
                {
                    double targetPulse = (double)axis.DtoP(abspos);

                    AxisStatusList[axis.AxisIndex.Value].AxisBusy = true;
                    AxisStatusList[axis.AxisIndex.Value].State = EnumAxisState.MOVING;

                    Task task = new Task(() =>
                    {
                        //LoggerManager.Debug($"AbsMove(): Start. Axis={axis.Label.Value}, trjtype={trjtype}");

                        AxisStatusList[axis.AxisIndex.Value].Pulse.Ref = targetPulse;
                        AxisStatusList[axis.AxisIndex.Value].Pulse.Command = targetPulse;

                        //=> NEW
                        double vel = 0;
                        GetVelocity(axis, trjtype, ref vel);
                        double dist = targetPulse - AxisStatusList[axis.AxisIndex.Value].Pulse.Actual;
                        double moveTime = (long)Math.Abs(dist / (vel * ovrd) * 1000.0);

                        double stepPos = 1;
                        if (moveTime < 10)
                        {
                            stepPos = dist;
                        }
                        else
                        {
                            // TODO : SCAN 0.3
                            stepPos = dist == 0 ? 0 : dist / ((double)moveTime / (double)(MONITORING_INTERVAL_INMS * 50));
                        }
                        while (true)
                        {
                            if (Math.Abs(AxisStatusList[axis.AxisIndex.Value].Pulse.Actual - AxisStatusList[axis.AxisIndex.Value].Pulse.Ref) <= axis.Config.Inposition.Value + Math.Abs(stepPos))
                            {
                                AxisStatusList[axis.AxisIndex.Value].Pulse.Actual = AxisStatusList[axis.AxisIndex.Value].Pulse.Ref;

                                AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
                                AxisStatusList[axis.AxisIndex.Value].State = EnumAxisState.IDLE;

                                //LoggerManager.Debug($"AbsMove(): Finished. Axis={axis.Label.Value}");

                                break;
                            }

                            if (AxisStatusList[axis.AxisIndex.Value].AxisEnabled == false)
                            {
                                AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
                                AxisStatusList[axis.AxisIndex.Value].State = EnumAxisState.STOPPED;
                                LoggerManager.Debug($"AbsMove(): Stopped. Axis={axis.Label.Value}");
                                break;
                            }

                            AxisStatusList[axis.AxisIndex.Value].Pulse.Actual += stepPos;
                            Thread.Sleep(1);
                        }
                    });
                    task.RunSynchronously();

                    if (axis is ProbeAxisObject)
                    {
                        ProbeAxisObject probeAxis = axis as ProbeAxisObject;

                        // 이동을 이미 한 뒤, 현재 위치를 이용하여 각 좌표계에 맞는 위치로 이동.
                        if (probeAxis.AxisType.Value == EnumAxisConstants.X ||
                            probeAxis.AxisType.Value == EnumAxisConstants.Y ||
                            probeAxis.AxisType.Value == EnumAxisConstants.C ||
                            probeAxis.AxisType.Value == EnumAxisConstants.Z ||
                            probeAxis.AxisType.Value == EnumAxisConstants.PZ)
                        {
                            VirtualStageConnector.VirtualStageConnector.Instance.MoveAbsoluteWrapper(probeAxis.AxisType.Value);
                        }
                    }

                    retVal = 0;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($ex);
                LoggerManager.Exception(err);

                retVal = -1;
            }

            return retVal;
        }
        public int AbsMove(AxisObject axis, double abspos, double vel, double acc)
        {
            try
            {
                return AbsMove(axis, abspos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            //return ret;
        }
        public int AbsMove(AxisObject axis, double abspos, double vel, double acc, double dcc)
        {
            return AbsMove(axis, abspos);
        }
        public int JogMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            LoggerManager.Debug($"JogMove({axis.AxisIndex.Value}): Jog move velocity {velocity} with {trjtype} trajectory.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int JogMove(AxisObject axis, double velocity, double accel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            return JogMove(axis, velocity);
        }
        public int RelMove(AxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;
            AxisStatus axisStatus = AxisStatusList[axis.AxisIndex.Value];

            try
            {
                //double abspos = axis.Status.Position.Actual;
                double abspos;

                if (axis.Label.Value == "Z" || axis.Label.Value == "PZ" || axis.Label.Value == "A")
                {
                    abspos = axis.Status.Position.Ref;
                }
                else
                {
                    abspos = axis.Status.Position.Actual;
                }

                if (axis is AxisObject == false) //Check Null
                {
                    LoggerManager.Error($"RelMove() : Axis is null.");
                    retVal = (int)-4;
                }
                else if (axisStatus.AxisEnabled == false)
                {
                    LoggerManager.Error($"RelMove() : Axis not enabled. Axis={axis.Label.Value}");
                    retVal = -1;
                }
                else if (axisStatus.AxisBusy == true)
                {
                    LoggerManager.Error($"RelMove() : Axis busy. Axis={axis.Label.Value}");
                    retVal = -1;
                }
                else if (abspos > axis.Param.PosSWLimit.Value)
                {
                    LoggerManager.Error($"RelMove() : Positive SW Limit occurred. Axis={axis.Label.Value}, Target={abspos}, Limit={axis.Param.PosSWLimit.Value}");
                    retVal = -1;
                }
                else if (abspos < axis.Param.NegSWLimit.Value)
                {
                    LoggerManager.Error($"RelMove() : Negative SW Limit occurred. Axis={axis.Label.Value}, Target={abspos}, Limit={axis.Param.NegSWLimit.Value}");
                    retVal = -1;
                }
                else
                {
                    double targetPulse;
                    targetPulse = axis.DtoP(pos);

                    axisStatus.AxisBusy = true;
                    axisStatus.State = EnumAxisState.MOVING;

                    Task task = new Task(() =>
                    {
                        //LoggerManager.Debug($"RelMove(): Start. axis={axis.Label.Value}, trj={trjtype}");

                        axisStatus.Pulse.Ref = axisStatus.Pulse.Actual + targetPulse;
                        axisStatus.Pulse.Command = axisStatus.Pulse.Actual + targetPulse;

                        double vel = 0;
                        GetVelocity(axis, trjtype, ref vel);
                        double startPos = axisStatus.Pulse.Actual;
                        double moveTime = (long)Math.Abs(targetPulse / (vel * ovrd) * 1000.0);
                        double dist = targetPulse;
                        double stepPos = dist == 0 ? 0 : dist / ((double)moveTime / (double)(MONITORING_INTERVAL_INMS * 50));

                        while (true)
                        {
                            if (Math.Abs(axisStatus.Pulse.Actual - axisStatus.Pulse.Ref) <= axis.Config.Inposition.Value + Math.Abs(stepPos))
                            {
                                axisStatus.Pulse.Actual = axisStatus.Pulse.Ref;
                                axisStatus.AxisBusy = false;
                                axisStatus.State = EnumAxisState.IDLE;

                                //LoggerManager.Debug($"RelMove(): Finished. Axis={axis.Label.Value}");
                                break;
                            }

                            if (!axisStatus.AxisBusy || !axisStatus.AxisEnabled)
                            {
                                axisStatus.AxisBusy = false;
                                axisStatus.State = EnumAxisState.ERROR;

                                LoggerManager.Debug($"RelMove(): Stopped. Axis={axis.Label.Value}");
                                break;
                            }

                            axisStatus.Pulse.Actual += stepPos;
                            Thread.Sleep(1);
                        }
                    });

                    task.RunSynchronously();

                    if (axis is ProbeAxisObject)
                    {
                        ProbeAxisObject probeAxis = axis as ProbeAxisObject;

                        // 이동을 이미 한 뒤, 현재 위치를 이용하여 각 좌표계에 맞는 위치로 이동.
                        if (probeAxis.AxisType.Value == EnumAxisConstants.X ||
                            probeAxis.AxisType.Value == EnumAxisConstants.Y ||
                            probeAxis.AxisType.Value == EnumAxisConstants.C ||
                            probeAxis.AxisType.Value == EnumAxisConstants.Z ||
                            probeAxis.AxisType.Value == EnumAxisConstants.PZ)
                        {
                            VirtualStageConnector.VirtualStageConnector.Instance.MoveAbsoluteWrapper(probeAxis.AxisType.Value);
                        }
                    }

                    retVal = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                axisStatus.State = EnumAxisState.ERROR;
                retVal = -1;

            }
            return retVal;
        }
        public int RelMove(AxisObject axis, double pos, double vel, double acc)
        {
            return RelMove(axis, pos);
        }
        public int RelMove(AxisObject axis, double pos, double vel, double acc, double dcc)
        {
            return RelMove(axis, pos);
        }
        public int VMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = (int)EnumMotionResult.NO_ERR;
            try
            {
                if (AxisStatusList[axis.AxisIndex.Value].AxisEnabled && AxisStatusList[axis.AxisIndex.Value].AxisBusy == false)
                {
                    AxisStatusList[axis.AxisIndex.Value].AxisBusy = true;

                    var task = new Task(() =>
                    {
                        LoggerManager.Debug($"VMove({axis.AxisIndex.Value}): Absolute move to  {trjtype} trajectory.");

                        long moveTime;
                        double stepPos;
                        //double dist;
                        //double startPos;
                        //int direction;


                        //moveTime = (long)Math.Abs(velocity);
                        moveTime = (long)(velocity);

                        stepPos = ((double)moveTime / (double)(MONITORING_INTERVAL_INMS * 50)) / 10.2;

                        while (AxisStatusList[axis.AxisIndex.Value].AxisEnabled && AxisStatusList[axis.AxisIndex.Value].AxisBusy)
                        {
                            AxisStatusList[axis.AxisIndex.Value].Pulse.Actual += stepPos;
                            AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
                            Thread.Sleep(10);
                        }
                        AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
                    });
                    task.RunSynchronously();

                    // TODO : 
                }
                else
                {
                    LoggerManager.Error($"Motion Exception occurred while Velocity moving for Axis {axis.Label.Value}, AxisEnabled = { AxisStatusList[axis.AxisIndex.Value].AxisEnabled}, IsMoving = {AxisStatusList[axis.AxisIndex.Value].AxisBusy}");
                    retVal = -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retVal = -1;

            }
            return retVal;

        }
        public int AlarmReset(AxisObject axis)
        {
            LoggerManager.Debug($"AlarmReset ({axis.AxisIndex.Value}): Alarm Reset.");

            AxisStatusList[axis.AxisIndex.Value].AxisEnabled = false;
            return (int)EnumMotionResult.NO_ERR; ;
        }
        public int AmpFaultClear(AxisObject axis)
        {
            LoggerManager.Debug($"AmpFaultClear ({axis.AxisIndex.Value}): AmpFault Clear.");
            return 0;
        }
        public int ApplyAxisConfig(AxisObject axis)
        {
            LoggerManager.Debug($"ApplyAxisConfig ({axis.AxisIndex.Value}): Apply AxisConfig.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int ClearUserLimit(AxisObject axis)
        {
            ClearUserLimit(axis.AxisIndex.Value);
            System.Threading.Thread.Sleep(10);
            try
            {
                AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
                AxisStatusList[axis.AxisIndex.Value].AxisEnabled = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0;
        }
        public int ClearUserLimit(int axisNumber)
        {
            LoggerManager.Debug($"ClearUserLimit ({axisNumber}): Clear UserLimit.");

            return 0;
        }
        public int ConfigCapture(AxisObject axis, EnumMotorDedicatedIn input)
        {
            AxisStatusList[axis.AxisIndex.Value].MotionCaptureStatus.CaptureState = CaptureState.CaptureStateARMED;
            LoggerManager.Debug($"ConfigCapture ({axis.AxisIndex.Value}): ConfigCapture.");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int DeInitMotionService()
        {
            //bStopUpdateThread = true;
            //UpdateThread?.Join();

            LoggerManager.Debug($"DeInitMotionService().");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int DisableAxis(AxisObject axis)
        {
            LoggerManager.Debug($"DisableAxis({axis.AxisIndex.Value}): Axis disabled.");

            AxisStatusList[axis.AxisIndex.Value].AxisEnabled = false;
            AxisStatusList[axis.AxisIndex.Value].State = EnumAxisState.DISABLED;
            axis.Status.IsHomeSeted = false;
            return (int)EnumMotionResult.NO_ERR;
        }
        public int DisableCapture(AxisObject axis)
        {
            AxisStatusList[axis.AxisIndex.Value].MotionCaptureStatus.CaptureState = CaptureState.CaptureStateIDLE;
            LoggerManager.Debug($"DisableCapture({axis.AxisIndex.Value}): Disable Capture.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int EnableAxis(AxisObject axis)
        {
            //LoggerManager.Debug($"EnableAxis({0}): Axis Enabled.",axis.AxisIndex.Value));
            AxisStatusList[axis.AxisIndex.Value].AxisEnabled = true;
            AxisStatusList[axis.AxisIndex.Value].State = EnumAxisState.IDLE;
            return (int)EnumMotionResult.NO_ERR;
        }
        public double GetAccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            //LoggerManager.Debug($"GetAccel({0}): Axis Accel.",axis.AxisIndex.Value));
            return axis.Param.Acceleration.Value;
        }
        public int GetActualPosition(AxisObject axis, ref double pos)
        {
            //LoggerManager.Debug($"GetActualPosition({0}): Axis ActualPosition.",
            // axis.AxisIndex.Value));
            pos = axis.PtoD(AxisStatusList[axis.AxisIndex.Value].Pulse.Actual);
            return (int)EnumMotionResult.NO_ERR;
        }
        public int GetActualPulse(AxisObject axis, ref int pos)
        {
            //LoggerManager.Debug($"GetActualPosition({0}): Axis ActualPosition.",axis.AxisIndex.Value));
            pos = (int)AxisStatusList[axis.AxisIndex.Value].Pulse.Actual;
            return (int)EnumMotionResult.NO_ERR;
        }
        public int GetAlarmCode(AxisObject axis, ref ushort alarmcode)
        {
            //LoggerManager.Debug($"GetAlarmCode({0}): GetAlarmCode.",axis.AxisIndex.Value));
            alarmcode = 0;
            return (int)EnumMotionResult.NO_ERR;
        }
        public int GetAxisInputs(AxisObject axis, ref uint instatus)
        {
            int retVal = -1;
            try
            {
                uint inputs = 0x00;
                try
                {
                    retVal = (int)EnumMotionResult.NO_ERR;

                    ResultValidate(retVal);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"GetAxisInputs() Function error: " + err.Message);
                }
                finally
                {
                    instatus = inputs;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public int GetAxisState(AxisObject axis, ref int state)
        {
            //LoggerManager.Debug($"GetAxisState({0}): Get AxisState.",axis.AxisIndex.Value));
            state = (AxisStatusList[axis.AxisIndex.Value].AxisEnabled) ? 1 : 0;
            return (int)EnumMotionResult.NO_ERR;

        }
        public int GetAxisStatus(AxisObject axis)
        {
            //LoggerManager.Debug($"GetActualPosition({0}): Axis ActualPosition.",axis.AxisIndex.Value));
            return (AxisStatusList[axis.AxisIndex.Value].AxisBusy) ? 1 : 0;
        }
        public int GetCaptureStatus(int axisindex, AxisStatus status)
        {
            //LoggerManager.Debug($"GetCaptureStatus({0}): GetCaptureStatus.",axisindex));
            return (int)EnumMotionResult.NO_ERR;
        }
        public int GetCmdPosition(AxisObject axis, ref double cmdpos)
        {
            //LoggerManager.Debug($"GetCmdPosition({0}): Get CmdPosition.",axis.AxisIndex.Value));
            cmdpos = axis.PtoD(AxisStatusList[axis.AxisIndex.Value].Pulse.Command);
            return (int)EnumMotionResult.NO_ERR;
        }
        public int GetCommandPosition(AxisObject axis, ref double pos)
        {
            //LoggerManager.Debug($"GetCommandPosition({0}): Get CommandPosition.",axis.AxisIndex.Value));
            pos = axis.PtoD(AxisStatusList[axis.AxisIndex.Value].Pulse.Command);
            return (int)EnumMotionResult.NO_ERR;
        }
        public int GetCommandPulse(AxisObject axis, ref int pos)
        {
            //LoggerManager.Debug($"GetCommandPulse({0}): GetCommandPulse.",axis.AxisIndex.Value));
            pos = (int)AxisStatusList[axis.AxisIndex.Value].Pulse.Command;
            return (int)EnumMotionResult.NO_ERR;
        }
        public double GetDeccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            //LoggerManager.Debug($"GetDeccel({0}): GetDeccel.",axis.AxisIndex.Value));
            return axis.Param.Decceleration.Value;
        }
        public bool GetHomeSensorState(AxisObject axis)
        {
            //LoggerManager.Debug($"GetHomeSensorState({0}): GetHomeSensorState.",axis.AxisIndex.Value));
            return false;
        }
        public bool GetIOAmpFault(AxisObject axis)
        {
            //LoggerManager.Debug($"GetIOAmpFault({0}): GetIOAmpFault.",axis.AxisIndex.Value));
            return false;
        }
        public bool GetIOHome(AxisObject axis)
        {
            //LoggerManager.Debug($"GetIOHome({0}): GetIOHome.",axis.AxisIndex.Value));
            return false;
        }
        public bool GetIONegLim(AxisObject axis)
        {
            //LoggerManager.Debug($"GetIONegLim({0}): GetIONegLim.",axis.AxisIndex.Value));
            return false;
        }
        public bool GetIOPosLim(AxisObject axis)
        {
            //LoggerManager.Debug($"GetIOPosLim({0}): GetIOPosLim.",axis.AxisIndex.Value));
            return false;
        }
        public int GetMotorAmpEnable(AxisObject axis, ref bool turnAmp)
        {
            //LoggerManager.Debug($"GetMotorAmpEnable({0}):  GetMotorAmpEnable.",axis.AxisIndex.Value));
            turnAmp = AxisStatusList[axis.AxisIndex.Value].AxisEnabled;
            return (int)EnumMotionResult.NO_ERR;
        }
        public int GetPosError(AxisObject axis, ref double poserr)
        {
            //LoggerManager.Debug($"GetPosError({0}):  GetPosError.",axis.AxisIndex.Value));
            poserr = AxisStatusList[axis.AxisIndex.Value].Pulse.Error;
            return (int)EnumMotionResult.NO_ERR;
        }
        public int GetVelocity(AxisObject axis, EnumTrjType trjtype, ref double vel, double ovrd = 1)
        {
            //LoggerManager.Debug($"GetVelocity({0}):  GetVelocity.",axis.AxisIndex.Value));
            double pulse = 0;
            int retVal = -1;
            try
            {
                switch (trjtype)
                {
                    case EnumTrjType.Normal:
                        vel = (double)axis.DtoP(axis.Param.Speed.Value);
                        break;
                    case EnumTrjType.Probing:
                        vel = (double)axis.DtoP(axis.Param.SeqSpeed.Value);
                        break;
                    case EnumTrjType.Homming:
                        pulse = axis.DtoP(axis.Param.HommingSpeed.Value);
                        vel = (double)pulse;
                        break;
                    default:
                        break;
                }
                vel = vel * ovrd;
                retVal = (int)EnumMotionBaseReturnCode.ReturnCodeOK;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetAccel() Function error: " + err.Message);
            }
            return retVal;
        }
        public int Halt(AxisObject axis, double value)
        {
            LoggerManager.Debug($"Halt({axis.AxisIndex.Value}):  Halt.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public bool HasAlarm(AxisObject axis)
        {
            LoggerManager.Debug($"HasAlarm({axis.AxisIndex.Value}):  HasAlarm.");
            return false;
        }
        public int Homming(AxisObject axis)
        {
            LoggerManager.Debug($"Homming({axis.AxisIndex.Value}):  Homming.");
            try
            {
                if (axis.AxisGroupType.Value == EnumAxisGroupType.SINGEAXIS)
                {
                    AxisStatusList[axis.AxisIndex.Value].Pulse.Actual = axis.DtoP(axis.Param.HomeOffset.Value);
                    AxisStatusList[axis.AxisIndex.Value].Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                    AxisStatusList[axis.AxisIndex.Value].Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                    AxisStatusList[axis.AxisIndex.Value].AxisEnabled = true;
                    AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
                    AxisStatusList[axis.AxisIndex.Value].IsHomeSeted = true;
                    AxisStatusList[axis.AxisIndex.Value].State = EnumAxisState.IDLE;
                    axis.Status.IsHomeSeted = true;
                    axis.Status.State = EnumAxisState.IDLE;
                    axis.Status.AxisBusy = false;
                    axis.Status.AxisEnabled = true;
                    axis.Status.Pulse.Actual = axis.DtoP(axis.Param.HomeOffset.Value);
                    axis.Status.Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                    axis.Status.Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                }
                else
                {
                    foreach (var item in axis.GroupMembers)
                    {
                        AxisStatusList[axis.AxisIndex.Value].Pulse.Actual = axis.DtoP(axis.Param.HomeOffset.Value);
                        AxisStatusList[axis.AxisIndex.Value].Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                        AxisStatusList[axis.AxisIndex.Value].Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                        AxisStatusList[axis.AxisIndex.Value].AxisEnabled = true;
                        AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
                        AxisStatusList[axis.AxisIndex.Value].IsHomeSeted = true;
                        AxisStatusList[axis.AxisIndex.Value].State = EnumAxisState.IDLE;
                        item.Status.IsHomeSeted = true;
                        item.Status.State = EnumAxisState.IDLE;
                        item.Status.AxisBusy = false;
                        item.Status.AxisEnabled = true;
                        item.Status.Pulse.Actual = axis.DtoP(item.Param.HomeOffset.Value);
                        item.Status.Pulse.Ref = axis.DtoP(item.Param.HomeOffset.Value);
                        item.Status.Pulse.Command = axis.DtoP(item.Param.HomeOffset.Value);
                    }
                    AxisStatusList[axis.AxisIndex.Value].Pulse.Actual = axis.DtoP(axis.Param.HomeOffset.Value);
                    AxisStatusList[axis.AxisIndex.Value].Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                    AxisStatusList[axis.AxisIndex.Value].Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                    AxisStatusList[axis.AxisIndex.Value].AxisEnabled = true;
                    AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
                    AxisStatusList[axis.AxisIndex.Value].IsHomeSeted = true;
                    AxisStatusList[axis.AxisIndex.Value].State = EnumAxisState.IDLE;
                    axis.Status.IsHomeSeted = true;
                    axis.Status.State = EnumAxisState.IDLE;
                    axis.Status.AxisBusy = false;
                    axis.Status.AxisEnabled = true;
                    axis.Status.Pulse.Actual = axis.DtoP(axis.Param.HomeOffset.Value);
                    axis.Status.Pulse.Ref = axis.DtoP(axis.Param.HomeOffset.Value);
                    axis.Status.Pulse.Command = axis.DtoP(axis.Param.HomeOffset.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return (int)EnumMotionResult.NO_ERR;
        }
        public int Homming(AxisObject axis, bool reverse, EnumIndexConfig input, double homeoffset)
        {
            try
            {
                return Homming(axis);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public int Homming(AxisObject axis, bool reverse, int input, double homeoffset)
        {
            try
            {
                return Homming(axis);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public int Homming(AxisObject axis, bool reverse, EnumMCInputs input, double homeoffset)
        {
            try
            {
                return Homming(axis);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public int InitMotionProvider(string ctrlNo)
        {
            try
            {
                AxisStatusList = new List<AxisStatus>();
                for (int axisindex = 0; axisindex < this.axisNum; axisindex++)
                {
                    var axis = new AxisStatus();
                    axis.AxisEnabled = true;
                    AxisStatusList.Add(axis);
                }
                CapturePosList = new List<double>();
                #region
                //CapturePosList.Add(74315.795898);
                //CapturePosList.Add(75498.657227);
                //CapturePosList.Add(84135.742188);
                //CapturePosList.Add(85228.881836);
                //CapturePosList.Add(94221.191406);
                //CapturePosList.Add(95373.535156);
                //CapturePosList.Add(104225.463867);
                //CapturePosList.Add(105368.652344);
                //CapturePosList.Add(134213.867188);
                //CapturePosList.Add(135344.848633);
                //CapturePosList.Add(144172.363281);
                //CapturePosList.Add(145353.393555);
                //CapturePosList.Add(154181.518555);
                //CapturePosList.Add(155362.548828);
                //CapturePosList.Add(164462.890625);
                //CapturePosList.Add(165582.275391);
                //CapturePosList.Add(174459.838867);
                //CapturePosList.Add(175603.027344);
                //CapturePosList.Add(184393.920898);
                //CapturePosList.Add(185512.695313);
                //CapturePosList.Add(194489.746094);
                //CapturePosList.Add(195571.289063);
                //CapturePosList.Add(244396.362305);
                //CapturePosList.Add(245490.112305);
                //CapturePosList.Add(254343.87207);
                //CapturePosList.Add(255461.425781);
                //CapturePosList.Add(264276.733398);
                //CapturePosList.Add(265444.946289);
                //CapturePosList.Add(274360.961914);
                //CapturePosList.Add(275566.40625);
                //CapturePosList.Add(304271.850586);
                //CapturePosList.Add(305462.036133);
                //CapturePosList.Add(314515.380859);
                //CapturePosList.Add(315684.814453);
                #endregion
                LoadScanPositions();

                //bStopUpdateThread = false;
                //UpdateThread = new Thread(new ThreadStart(UpdateProc));
                //UpdateThread.Start();

                return (int)EnumMotionResult.NO_ERR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public bool IsHoming(AxisObject axis)
        {
            bool val = false;
            try
            {
                IsMoving(axis, ref val);
                //LoggerManager.Debug($"IsHoming({0}):  IsHoming.",axis.AxisIndex.Value));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return val;
        }
        public int IsInposition(AxisObject axis, ref bool val)
        {
            //   LoggerManager.Debug($"IsInposition({0}):  IsInposition.",
            try
            {
                //axis.AxisIndex.Value));
                val = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0;
        }
        public int IsMotorEnabled(AxisObject axis, ref bool val)
        {
            //LoggerManager.Debug($"IsMotorEnabled({0}):  IsMotorEnabled.",axis.AxisIndex.Value));
            try
            {
                val = axis.Status.AxisEnabled;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0;
        }
        public int IsMoving(AxisObject axis, ref bool val)
        {
            int state = -1;
            int retVal = -1;
            try
            {
                GetAxisState(axis, ref state);
                switch ((EnumAxisState)state)
                {
                    case EnumAxisState.INVALID:
                        val = false;
                        break;
                    case EnumAxisState.IDLE:
                        val = false;
                        break;
                    case EnumAxisState.MOVING:
                        val = true;
                        break;
                    case EnumAxisState.STOPPING:
                        val = true;
                        break;
                    case EnumAxisState.STOPPED:
                        val = false;
                        break;
                    case EnumAxisState.STOPPING_ERROR:
                        val = false;
                        break;
                    case EnumAxisState.ERROR:
                        val = false;
                        break;
                    case EnumAxisState.END:
                        val = false;
                        break;
                    default:
                        val = false;
                        break;
                }

                retVal = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public int LinInterpolation(AxisObject[] axes, double[] poss)
        {
            LoggerManager.Debug($"LinInterpolation({0}): Motion halted.");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int Pause(AxisObject axis)
        {
            LoggerManager.Debug($"Pause({axis.AxisIndex.Value}): Motion halted.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int ResultValidate(int retcode)
        {
            if ((EnumMotionResult)retcode != EnumMotionResult.NO_ERR)
                try
                {
                    {
                        LoggerManager.Error($"ResultValidate(): Err code = {retcode}, Description = {((EnumMotionResult)retcode).ToString()}");
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            return retcode;
        }
        public int Resume(AxisObject axis)
        {
            LoggerManager.Debug($"Resume({axis.AxisIndex.Value}): Motion Resumed.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetFeedrate(AxisObject axis, double normfeedrate, double pausefeedrate)
        {
            LoggerManager.Debug($"SetFeedrate({axis.AxisIndex.Value}): SetFeedrate.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetHWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            LoggerManager.Debug($"SetHWNegLimAction({axis.AxisIndex.Value}): SetHWNegLimAction.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetHWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            LoggerManager.Debug($"SetHWPosLimAction({axis.AxisIndex.Value}): SetHWPosLimAction.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetLimitSWNegAct(AxisObject axis, EnumEventActionType action)
        {
            LoggerManager.Debug($"SetLimitSWNegAct({axis.AxisIndex.Value}): SetLimitSWNegAct.");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetLimitSWPosAct(AxisObject axis, EnumEventActionType action)
        {
            LoggerManager.Debug($"SetLimitSWPosAct({axis.AxisIndex.Value}): SetLimitSWPosAct.");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetMotorAmpEnable(AxisObject axis, bool turnAmp)
        {
            LoggerManager.Debug($"SetMotorAmpEnable({axis.AxisIndex.Value}): SetMotorAmpEnable.");
            AxisStatusList[axis.AxisIndex.Value].AxisEnabled = turnAmp;
            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetOverride(AxisObject axis, double ovrd)
        {
            LoggerManager.Debug($"SetOverride({axis.AxisIndex.Value}): SetOverride.");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetPosition(AxisObject axis, double pos)
        {
            int retVal = -1;
            try
            {
                double _pos = 0;
                try
                {
                    _pos = axis.DtoP(pos);
                    retVal = SetPulse(axis, _pos);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"SetPosition() Function error: " + err.Message);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public int SetPulse(AxisObject axis, double pos)
        {
            int retVal = -1;
            try
            {
                AxisStatusList[axis.AxisIndex.Value].Pulse.Actual = pos;
                AxisStatusList[axis.AxisIndex.Value].Pulse.Command = pos;
                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetPulse() Function error: " + err.Message);
            }
            return retVal;
        }
        public int SetSettlingTime(AxisObject axis, double settlingTime)
        {

            LoggerManager.Debug($"SetOverride({axis.AxisIndex.Value}): SetOverride.");

            axis.SettlingTime = (long)settlingTime;
            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetSwitchAction(AxisObject axis, EnumDedicateInputs input, EnumEventActionType action, EnumInputLevel reverse)
        {

            LoggerManager.Debug($"SetSwitchAction({axis.AxisIndex.Value}): SetSwitchAction.");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetSWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            LoggerManager.Debug($"SetSWNegLimAction({axis.AxisIndex.Value}): SetSWNegLimAction.");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetSWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            LoggerManager.Debug($"SetSWPosLimAction({axis.AxisIndex.Value}): SetSWPosLimAction.");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int SetZeroPosition(AxisObject axis)
        {
            SetPulse(axis, 0);
            try
            {
                AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0;
        }
        public int VMoveStop(AxisObject axis)
        {
            int retVal = -1;
            return retVal;
        }
        public int Stop(AxisObject axis)
        {
            LoggerManager.Debug($"Stop({axis.AxisIndex.Value}): Stop.");

            AxisStatusList[axis.AxisIndex.Value].AxisBusy = false;
            return (int)EnumMotionResult.NO_ERR;
        }

        public int WaitForAxisEvent(AxisObject axis, EnumAxisState waitfor, double distlimit, long timeout = -1)
        {
            LoggerManager.Debug($"WaitForAxisEvent({axis.AxisIndex.Value}): WaitForAxisEvent.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int WaitForVMoveAxisMotionDone(AxisObject axis, long timeout = 0)
        {
            LoggerManager.Debug($"WaitForVMoveAxisMotionDone({axis.AxisIndex.Value}): WaitForVMoveAxisMotionDone.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int WaitForAxisMotionDone(AxisObject axis, long timeout = 0)
        {
            int retVal = -1;
            try
            {
                bool isinpos = false;
                //int cnt = 0;
                Stopwatch elapsedStopWatch = new Stopwatch();

                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();

                Stopwatch stw = new Stopwatch();
                List<KeyValuePair<string, long>> timeStamp;
                timeStamp = new List<KeyValuePair<string, long>>();

                stw.Restart();
                stw.Start();
                timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                try
                {
                    bool runFlag = true;

                    //GetAxisStatus(axis);
                    //axis.Status.AxisBusy = DirectIsMoving(axis.AxisIndex.Value);
                    //axis.Status.AxisEnabled = IsMotorEnabled(axis);
                    //axis.Status.Inposition = IsInposition(axis);
                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("First End GetAxisStatus {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));

                    do
                    {
                        //axis.Status.AxisEnabled = IsMotorEnabled(axis);
                        IsInposition(axis, ref isinpos);
                        axis.Status.Inposition = isinpos;
                        timeStamp.Add(new KeyValuePair<string, long>(string.Format("End ISMoving {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                        if (AxisStatusList[axis.AxisIndex.Value].AxisBusy == true)
                        {
                            if (timeout != 0)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                                {

                                    LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                                    runFlag = false;
                                    retVal = -2;
                                    Stop(axis);
                                }
                            }
                            else
                            {
                                runFlag = true;
                            }
                        }
                        else
                        {
                            runFlag = false;
                            if (axis.Status.ErrCode == 0x0000)
                            {
                                retVal = 0;
                                //LoggerManager.Debug(string.Format("WaitForAxisMotionDone({0}) : Axis motion done.", axis.AxisIndex.Value));
                            }
                            else
                            {
                                Stop(axis);
                                retVal = -1;
                                LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex.Value}) : Axis motion error occurred. Err code = {axis.Status.ErrCode}");
                            }
                        }
                        timeStamp.Add(new KeyValuePair<string, long>(string.Format("End GetAxisStatus {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));

                        Thread.Sleep(10);
                    } while (runFlag == true);

                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("Settling.... {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                    timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($"WaitForAxisMotionDone() Function error: {0}" + err.Message));
                    LoggerManager.Exception(err);

                    retVal = -1;
                }
                finally
                {
                    elapsedStopWatch?.Stop();
                    //double pos=0;
                    // GetCmdPosition(axis, ref pos);
                    // axis.Status.Position.Ref = pos;
                }
                timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label.Value), stw.ElapsedMilliseconds));
                stw.Stop();
                //int step = 0;
                //foreach (var item in timeStamp)
                //{
                //    step++;
                //    LoggerManager.Debug($"Time Stamp [{0}] - Desc: {1}, Time: {2}", step, item.Key, item.Value));
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public int WaitForAxisMotionDone(AxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0)
        {
            return WaitForAxisMotionDone(axis, timeout);
        }
        public int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0)
        {
            LoggerManager.Debug($"MonitorForAxisMotion({axis.AxisIndex.Value}): MonitorForAxisMotion.");
            return (int)EnumMotionResult.NO_ERR;
        }
        public int WaitForHomeSensor(AxisObject axis, long timeout = 0)
        {
            LoggerManager.Debug($"WaitForHomeSensor({axis.AxisIndex.Value}): WaitForHomeSensor.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int NotchFinding(AxisObject axis, EnumMotorDedicatedIn input)
        {
            LoggerManager.Debug($"NotchFinding({axis.AxisIndex.Value}): NotchFinding.");

            return (int)EnumMotionResult.NO_ERR;
        }
        public int OriginSet(AxisObject axis, double pos)
        {
            throw new NotImplementedException();
        }
        public int StartScanPosCapt(AxisObject axis, EnumMotorDedicatedIn MotorDedicatedIn)
        {

            try
            {
                //Test Demo
                //AxisStatusList[axis.AxisIndex.Value].CapturePositions = new List<double>();
                //foreach (double pos in CapturePosList)
                //{
                //    AxisStatusList[axis.AxisIndex.Value].CapturePositions.Add(pos);
                //}
                LoadScanPositions();

                try
                {
                    if (isRunningCapture == true)
                    {
                        return -1;
                    }

                    isRunningCapture = false;
                    CurrAxis = axis;
                    System.Threading.Thread t = new Thread(new ThreadStart(StartScanCapt));
                    t.Priority = ThreadPriority.Normal;
                    t.Start();
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($err, $"StartScanPosCapt({axis}, {MotorDedicatedIn}): Error occurred.");
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0;
        }
        private void StartScanCapt()
        {
            try
            {

                //thread 돌면서 캡쳐 받아오기
                isRunningCapture = true;

                bStopcaptureThread = false;
                short axisIndex = (short)CurrAxis.AxisIndex.Value;
                AxisStatusList[axisIndex].CapturePositions = new List<double>();
                Queue<double> poss = new Queue<double>();

                foreach (double pos in CapturePosList)
                {
                    poss.Enqueue(pos);
                }

                double cmdPos = 0;

                try
                {

                    int startIdx = 0;

                    bool IgnorecmdPos = true;

                    AxisStatusList[axisIndex].MotionCaptureStatus.CaptureState = CaptureState.CaptureStateARMED;

                    while (bStopcaptureThread == false)
                    {
                        GetCmdPosition(CurrAxis, ref cmdPos);

                        if (poss.Count > 0)
                        {
                            var pos = poss.Peek();

                            if (IgnorecmdPos == true)
                            {
                                poss.Dequeue();
                                AxisStatusList[axisIndex].CapturePositions.Add(pos);
                                AxisStatusList[axisIndex].MotionCaptureStatus.CaptureState = CaptureState.CaptureStateCAPTURED;
                            }
                            else
                            {
                                if (cmdPos > pos)
                                {
                                    poss.Dequeue();
                                    AxisStatusList[axisIndex].CapturePositions.Add(pos);
                                    AxisStatusList[axisIndex].MotionCaptureStatus.CaptureState = CaptureState.CaptureStateCAPTURED;
                                }
                            }
                        }

                        Thread.Sleep(10);

                        startIdx++;
                    }

                    foreach (double pos in AxisStatusList[axisIndex].CapturePositions)
                    {
                        LoggerManager.Debug($"Scan capture: Captured position = {pos}(㎛)");
                    }
                    GetCmdPosition(CurrAxis, ref cmdPos);
                    AxisStatusList[axisIndex].MotionCaptureStatus.CaptureState = CaptureState.CaptureStateIDLE;

                }
                catch (Exception err)
                {
                    LoggerManager.Error($"GetCaptureStatus() Function error: " + err.Message);
                }
                finally
                {
                    isRunningCapture = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            //return retVal;
        }
        public int StopScanPosCapt(AxisObject axis)
        {
            int retVal = 0;
            try
            {
                bStopcaptureThread = true;

                while (true)
                {
                    if (isRunningCapture == false)
                        break;

                    Thread.Sleep(100);
                }
                AxisStatusList[4].MotionCaptureStatus.CaptureState = CaptureState.CaptureStateIDLE;
                bStopcaptureThread = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public int ForcedZDown(AxisObject axis)
        {
            return 0;
        }
        public int SetDualLoop(bool dualloop)
        {
            return 0;
        }
        public int SetLoadCellZero()
        {
            return 0;
        }
        public int SetMotorStopCommand(AxisObject axis, string setevent, EnumMotorDedicatedIn input)
        {
            return 0;
        }
        public int IsThreeLegUp(AxisObject axis, ref bool isthreelegup)
        {
            isthreelegup = true;
            return 0;
        }
        public int IsThreeLegDown(AxisObject axis, ref bool isthreelegdn)
        {
            isthreelegdn = true;
            return 0;
        }
        public int IsFls(AxisObject axis, ref bool isfls)
        {
            isfls = true;
            return 0;
        }
        public int IsRls(AxisObject axis, ref bool isrls)
        {
            isrls = true;
            return 0;
        }
        public int ChuckTiltMove(AxisObject axis, double offsetz0, double offsetz1, double offsetz2, double abspos, double vel, double acc)
        {
            return 0;
        }
        public int GetAuxPulse(AxisObject axis, ref int pos)
        {
            pos = 0;
            return 0;
        }
    }
}

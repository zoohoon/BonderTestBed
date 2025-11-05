using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Motion
{

    public class ADSRemoteMotion : IMotionBase, IFactoryModule
    {
        private enum EnumGPLAxisConst
        {
            LX = 0,
            LZM,
            LZS,
            LW,
            LB,
            LUD,
            LUU,
            LCC,
            FC1,
            FC2,
            FC3,
            FC4,
            LAST_GPLOADER_AXIS = FC4,
        }

        bool bStopUpdateThread;
        Thread UpdateThread;

        private static int DefaultAxisCount = (int)EnumGPLAxisConst.LAST_GPLOADER_AXIS + 1;
        #region // Properties
        private List<AxisStatus> _AxisStatusList;
        public List<AxisStatus> AxisStatusList
        {
            get
            {
                return _AxisStatusList;
            }
            private set { _AxisStatusList = value; }
        }
        public int PortNo => throw new NotImplementedException();

        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            set { _DevConnected = value; }
        }

        private object _TcLockObj;

        public object TcLockObj
        {
            get { return _TcLockObj; }
            set { _TcLockObj = value; }
        }

        public IGPLoader GPLoader
        {
            get { return this.GetGPLoader(); }
        }

        public bool IsMotionReady => throw new NotImplementedException();

        public long UpdateProcTime => throw new NotImplementedException();
        #endregion
        public ADSRemoteMotion()
        {
            TcLockObj = new object();
            InitMotionProvider(string.Empty);
        }
        public ADSRemoteMotion(string CtrlNum)
        {
            TcLockObj = new object();
            InitMotionProvider(CtrlNum);
        }
        public ADSRemoteMotion(string CtrlNum, int axisnum)
        {
            TcLockObj = new object();
            InitMotionProvider(CtrlNum, axisnum);
        }
        ~ADSRemoteMotion()
        {
            bStopUpdateThread = true;
            if(UpdateThread != null)
            {
                UpdateThread.Join();
            }
        }
        private void ConvertStateCodeToStates(AxisStatus status)
        {
            int NotMovingMask = 0x01;
            int MovingMask = 0x02;
            //int NotHasJobMask = 0x04;
            //int PosMovingMask = 0x08;
            //int NegMovingMask = 0x10;
            int EnabledMask = 0x80;
            //gst_Int_PLCToPC.nLX_State.0         := Axis_LX.Status.NotMoving;
            //gst_Int_PLCToPC.nLX_State.1         := Axis_LX.Status.Moving;
            //gst_Int_PLCToPC.nLX_State.2         := NOT Axis_LX.Status.HasJob;
            //gst_Int_PLCToPC.nLX_State.3         := Axis_LX.Status.PositiveDirection;
            //gst_Int_PLCToPC.nLX_State.4         := Axis_LX.Status.NegativeDirection;
            //gst_Int_PLCToPC.nLX_State.7         := NOT Axis_LX.Status.Disabled;
            status.AxisEnabled = (status.StatusCode & EnabledMask) == EnabledMask ? true : false;
            status.Inposition = (status.StatusCode & NotMovingMask) == NotMovingMask ? true : false;
            status.AxisBusy = (status.StatusCode & MovingMask) == MovingMask ? true : false;

            if(status.AxisEnabled == false)
            {
                status.State = EnumAxisState.DISABLED;
            }
            else if (status.AxisBusy == true)
            {
                status.State = EnumAxisState.MOVING;
            }
            else if (status.Inposition == true)
            {
                status.State = EnumAxisState.IDLE;
            }
        }

        private void UpdateProc()
        {
            try
            {
                while (bStopUpdateThread == false)
                {
                    try
                    {
                        if(GPLoader != null)
                        {
                            if (GPLoader.DevConnected == true)
                            {
                                GPLoader.SyncEvent.WaitOne(1000);
                                AxisStatusList[(int)EnumGPLAxisConst.LX].Pulse.Actual = GPLoader.CDXIn.nLX_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LX].Pulse.Command = GPLoader.CDXIn.nLX_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LX].StatusCode = GPLoader.CDXIn.nLX_State;

                                AxisStatusList[(int)EnumGPLAxisConst.LZM].Pulse.Actual = GPLoader.CDXIn.nLZM_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LZM].Pulse.Command = GPLoader.CDXIn.nLZM_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LZM].StatusCode = GPLoader.CDXIn.nLZM_State;

                                AxisStatusList[(int)EnumGPLAxisConst.LZS].Pulse.Actual = GPLoader.CDXIn.nLZS_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LZS].Pulse.Command = GPLoader.CDXIn.nLZS_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LZS].StatusCode = GPLoader.CDXIn.nLZS_State;

                                AxisStatusList[(int)EnumGPLAxisConst.LW].Pulse.Actual = GPLoader.CDXIn.nLW_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LW].Pulse.Command = GPLoader.CDXIn.nLW_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LW].StatusCode = GPLoader.CDXIn.nLW_State;

                                AxisStatusList[(int)EnumGPLAxisConst.LB].Pulse.Actual = GPLoader.CDXIn.nLT_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LB].Pulse.Command = GPLoader.CDXIn.nLT_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LB].StatusCode = GPLoader.CDXIn.nLT_State;

                                AxisStatusList[(int)EnumGPLAxisConst.LUD].Pulse.Actual = GPLoader.CDXIn.nLUD_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LUD].Pulse.Command = GPLoader.CDXIn.nLUD_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LUD].StatusCode = GPLoader.CDXIn.nLUD_State;

                                AxisStatusList[(int)EnumGPLAxisConst.LUU].Pulse.Actual = GPLoader.CDXIn.nLUU_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LUU].Pulse.Command = GPLoader.CDXIn.nLUU_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LUU].StatusCode = GPLoader.CDXIn.nLUU_State;

                                AxisStatusList[(int)EnumGPLAxisConst.LCC].Pulse.Actual = GPLoader.CDXIn.nLCC_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LCC].Pulse.Command = GPLoader.CDXIn.nLCC_Pos;
                                AxisStatusList[(int)EnumGPLAxisConst.LCC].StatusCode = GPLoader.CDXIn.nLCC_State;
                                //if(GPLoader.CDXIn.nFC_Pos != null)
                                //{
                                //    AxisStatusList[(int)EnumGPLAxisConst.FC1].Pulse.Actual = GPLoader.CDXIn.nFC_Pos[0];
                                //    AxisStatusList[(int)EnumGPLAxisConst.FC1].Pulse.Command = GPLoader.CDXIn.nFC_Pos[0];
                                //    AxisStatusList[(int)EnumGPLAxisConst.FC1].StatusCode = GPLoader.CDXIn.nFC_State[0];

                                //    AxisStatusList[(int)EnumGPLAxisConst.FC2].Pulse.Actual = GPLoader.CDXIn.nFC_Pos[1];
                                //    AxisStatusList[(int)EnumGPLAxisConst.FC2].Pulse.Command = GPLoader.CDXIn.nFC_Pos[1];
                                //    AxisStatusList[(int)EnumGPLAxisConst.FC2].StatusCode = GPLoader.CDXIn.nFC_State[1];

                                //    AxisStatusList[(int)EnumGPLAxisConst.FC3].Pulse.Actual = GPLoader.CDXIn.nFC_Pos[2];
                                //    AxisStatusList[(int)EnumGPLAxisConst.FC3].Pulse.Command = GPLoader.CDXIn.nFC_Pos[2];
                                //    AxisStatusList[(int)EnumGPLAxisConst.FC3].StatusCode = GPLoader.CDXIn.nFC_State[2];
                                //}
                                //else
                                //{

                                //}
                                if (GPLoader.CDXIn.nFoup_DriveErrCode != null)
                                {
                                    AxisStatusList[(int)EnumGPLAxisConst.FC1].Pulse.Actual = GPLoader.CDXIn.nFoup1_Pos;
                                    AxisStatusList[(int)EnumGPLAxisConst.FC1].Pulse.Command = GPLoader.CDXIn.nFoup1_Pos;
                                    AxisStatusList[(int)EnumGPLAxisConst.FC1].StatusCode = (int)GPLoader.CDXIn.nFoup_DriveErrCode[0];

                                    AxisStatusList[(int)EnumGPLAxisConst.FC2].Pulse.Actual = GPLoader.CDXIn.nFoup2_Pos;
                                    AxisStatusList[(int)EnumGPLAxisConst.FC2].Pulse.Command = GPLoader.CDXIn.nFoup2_Pos;
                                    AxisStatusList[(int)EnumGPLAxisConst.FC2].StatusCode = (int)GPLoader.CDXIn.nFoup_DriveErrCode[1];

                                    AxisStatusList[(int)EnumGPLAxisConst.FC3].Pulse.Actual = GPLoader.CDXIn.nFoup3_Pos;
                                    AxisStatusList[(int)EnumGPLAxisConst.FC3].Pulse.Command = GPLoader.CDXIn.nFoup3_Pos;
                                    AxisStatusList[(int)EnumGPLAxisConst.FC3].StatusCode = (int)GPLoader.CDXIn.nFoup_DriveErrCode[2];

                                    if(SystemModuleCount.ModuleCnt.FoupCount > 3)
                                    {
                                        AxisStatusList[(int)EnumGPLAxisConst.FC1].Pulse.Actual = GPLoader.FOUPPoss[0];
                                        AxisStatusList[(int)EnumGPLAxisConst.FC1].Pulse.Command = GPLoader.FOUPPoss[0];
                                        AxisStatusList[(int)EnumGPLAxisConst.FC1].StatusCode = (int)GPLoader.FOUPDriveErrs[0];

                                        AxisStatusList[(int)EnumGPLAxisConst.FC2].Pulse.Actual = GPLoader.FOUPPoss[1];
                                        AxisStatusList[(int)EnumGPLAxisConst.FC2].Pulse.Command = GPLoader.FOUPPoss[1];
                                        AxisStatusList[(int)EnumGPLAxisConst.FC2].StatusCode = (int)GPLoader.FOUPDriveErrs[1];

                                        AxisStatusList[(int)EnumGPLAxisConst.FC3].Pulse.Actual = GPLoader.FOUPPoss[2];
                                        AxisStatusList[(int)EnumGPLAxisConst.FC3].Pulse.Command = GPLoader.FOUPPoss[2];
                                        AxisStatusList[(int)EnumGPLAxisConst.FC3].StatusCode = (int)GPLoader.FOUPDriveErrs[2];

                                        AxisStatusList[(int)EnumGPLAxisConst.FC4].Pulse.Actual = GPLoader.FOUPPoss[3];
                                        AxisStatusList[(int)EnumGPLAxisConst.FC4].Pulse.Command = GPLoader.FOUPPoss[3];
                                        AxisStatusList[(int)EnumGPLAxisConst.FC4].StatusCode = (int)GPLoader.FOUPDriveErrs[3];
                                    }

                                }
                                for (int axisindex = (int)EnumGPLAxisConst.LX; axisindex < (int)EnumGPLAxisConst.LAST_GPLOADER_AXIS; axisindex++)
                                {
                                    ConvertStateCodeToStates(AxisStatusList[axisindex]);
                                }
                            }
                        }

                        Thread.Sleep(1);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Debug($"Exception occurred. Err. = {err.Message}");
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"UpdateIOProc(): Error occurred while update io proc. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
        }
        #region // IMotionBase implementaion
        public int InitMotionProvider(string ctrlNo)
        {
            int retVal = -1;

            try
            {
                AxisStatusList = new List<AxisStatus>();
                AxisStatusList.Clear();
                for (int axisindex = 0; axisindex < DefaultAxisCount; axisindex++)
                {
                    AxisStatusList.Add(new AxisStatus());
                }


                bStopUpdateThread = false;
                UpdateThread = new Thread(new ThreadStart(UpdateProc));
                UpdateThread.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitMotionProvider(): Error occurred while init. Err = {err.Message}");
            }

            retVal = 0;
            return retVal;
        }
        public int InitMotionProvider(string ctrlNo, int axisnum)
        {
            int retVal = -1;
            try
            {
                bStopUpdateThread = false;
                UpdateThread = new Thread(new ThreadStart(UpdateProc));
                UpdateThread.Start();
                AxisStatusList = new List<AxisStatus>();
                AxisStatusList.Clear();
                for (int axisindex = 0; axisindex < axisnum; axisindex++)
                {
                    AxisStatusList.Add(new AxisStatus());
                }

                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitMotionProvider(): Error occurred while init. Err = {err.Message}");
            }

            return retVal;
        }
        
        public int SetMotorAmpEnable(AxisObject axis, bool turnAmp)
        {
            throw new NotImplementedException();
        }

        public int GetMotorAmpEnable(AxisObject axis, ref bool turnAmp)
        {
            throw new NotImplementedException();
        }

        public int DeInitMotionService()
        {
            bStopUpdateThread = true;
            if (UpdateThread != null)
            {
                UpdateThread.Join();
            }
            return 0;
        }

        public int ClearUserLimit(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int GetCmdPosition(AxisObject axis, ref double cmdpos)
        {
            throw new NotImplementedException();
        }

        public int AbsMove(AxisObject axis, double abspos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int AbsMove(AxisObject axis, double abspos, double finalVel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int AbsMove(AxisObject axis, double abspos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public int AbsMove(AxisObject axis, double abspos, double vel, double acc, double dcc)
        {
            throw new NotImplementedException();
        }

        public int RelMove(AxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int RelMove(AxisObject axis, double pos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public int RelMove(AxisObject axis, double pos, double vel, double acc, double dcc)
        {
            throw new NotImplementedException();
        }

        public int ConfigCapture(AxisObject axis, EnumMotorDedicatedIn input)
        {
            throw new NotImplementedException();
        }

        public int DisableCapture(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int GetCaptureStatus(int axisindex, AxisStatus status)
        {
            throw new NotImplementedException();
        }

        public int VMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int JogMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int JogMove(AxisObject axis, double velocity, double accel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int Homming(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int Homming(AxisObject axis, bool reverse, EnumIndexConfig input, double homeoffset)
        {
            throw new NotImplementedException();
        }

        public int Homming(AxisObject axis, bool reverse, int input, double homeoffset)
        {
            throw new NotImplementedException();
        }

        public int Homming(AxisObject axis, bool reverse, EnumMCInputs input, double homeoffset)
        {
            throw new NotImplementedException();
        }

        public int Stop(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int Halt(AxisObject axis, double value)
        {
            throw new NotImplementedException();
        }

        public int Pause(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int Resume(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int SetOverride(AxisObject axis, double ovrd)
        {
            throw new NotImplementedException();
        }

        public int LinInterpolation(AxisObject[] axes, double[] poss)
        {
            throw new NotImplementedException();
        }

        public int ResultValidate(int retcode)
        {
            throw new NotImplementedException();
        }

        public double GetAccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public double GetDeccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int GetVelocity(AxisObject axis, EnumTrjType trjtype, ref double vel, double ovrd = 1)
        {
            throw new NotImplementedException();
        }

        public int EnableAxis(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int DisableAxis(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int WaitForAxisMotionDone(AxisObject axis, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public int WaitForAxisMotionDone(AxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public int WaitForAxisEvent(AxisObject axis, EnumAxisState waitfor, double distlimit, long timeout = -1)
        {
            throw new NotImplementedException();
        }

        public int GetAxisStatus(AxisObject axis)
        {
            return AxisStatusList[axis.AxisIndex.Value].StatusCode;
        }

        public int GetAxisState(AxisObject axis, ref int state)
        {
            state = (int)AxisStatusList[axis.AxisIndex.Value].State;
            return 0;
        }

        public int GetAxisInputs(AxisObject axis, ref uint instatus)
        {
            throw new NotImplementedException();
        }

        public int IsMotorEnabled(AxisObject axis, ref bool val)
        {
            throw new NotImplementedException();
        }

        public int IsMoving(AxisObject axis, ref bool val)
        {
            throw new NotImplementedException();
        }

        public int IsInposition(AxisObject axis, ref bool val)
        {
            throw new NotImplementedException();
        }

        public int ApplyAxisConfig(AxisObject axis)
        {
            return 0;
        }

        public bool HasAlarm(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int GetAlarmCode(AxisObject axis, ref ushort alarmcode)
        {
            throw new NotImplementedException();
        }

        public int GetActualPosition(AxisObject axis, ref double pos)
        {
            throw new NotImplementedException();
        }

        public int GetCommandPosition(AxisObject axis, ref double pos)
        {
            pos = AxisStatusList[axis.AxisIndex.Value].Pulse.Command;
            return 0;
        }

        public int GetActualPulse(AxisObject axis, ref int pos)
        {
            throw new NotImplementedException();
        }

        public int GetCommandPulse(AxisObject axis, ref int pos)
        {
            throw new NotImplementedException();
        }

        public int SetPosition(AxisObject axis, double pos)
        {
            throw new NotImplementedException();
        }

        public int SetPulse(AxisObject axis, double pos)
        {
            throw new NotImplementedException();
        }

        public int AlarmReset(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int GetPosError(AxisObject axis, ref double poserr)
        {
            poserr = AxisStatusList[axis.AxisIndex.Value].Pulse.Error;
            return 0;
        }

        public bool GetIOAmpFault(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public bool GetIOHome(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public bool GetIONegLim(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public bool GetIOPosLim(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int SetSWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            throw new NotImplementedException();
        }

        public int SetHWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            throw new NotImplementedException();
        }

        public int SetSWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            throw new NotImplementedException();
        }

        public int SetHWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            throw new NotImplementedException();
        }

        public int SetSwitchAction(AxisObject axis, EnumDedicateInputs input, EnumEventActionType action, EnumInputLevel reverse)
        {
            throw new NotImplementedException();
        }

        public int ClearUserLimit(int axisNumber)
        {
            // TODO: clear limit
            return 0;
        }

        public int SetZeroPosition(AxisObject axis)
        {
            // TODO: set zero position
            return 0;
        }

        public int AmpFaultClear(AxisObject axis)
        {
            // TODO: clear amp error
            return 0;
        }

        public int Abort(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int SetLimitSWNegAct(AxisObject axis, EnumEventActionType action)
        {
            throw new NotImplementedException();
        }

        public int SetLimitSWPosAct(AxisObject axis, EnumEventActionType action)
        {
            throw new NotImplementedException();
        }

        public bool GetHomeSensorState(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int WaitForHomeSensor(AxisObject axis, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public int SetSettlingTime(AxisObject axis, double settlingTime)
        {
            throw new NotImplementedException();
        }

        public int SetFeedrate(AxisObject axis, double normfeedrate, double pausefeedrate)
        {
            throw new NotImplementedException();
        }

        public int NotchFinding(AxisObject axis, EnumMotorDedicatedIn input)
        {
            throw new NotImplementedException();
        }

        public int StartScanPosCapt(AxisObject axis, EnumMotorDedicatedIn MotorDedicatedIn)
        {
            throw new NotImplementedException();
        }

        public int StopScanPosCapt(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int OriginSet(AxisObject axis, double pos)
        {
            throw new NotImplementedException();
        }

        public int ForcedZDown(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int SetDualLoop(bool dualloop)
        {
            throw new NotImplementedException();
        }

        public int SetLoadCellZero()
        {
            throw new NotImplementedException();
        }

        public int SetMotorStopCommand(AxisObject axis, string setevent, EnumMotorDedicatedIn input)
        {
            throw new NotImplementedException();
        }

        public int IsThreeLegUp(AxisObject axis, ref bool isthreelegup)
        {
            throw new NotImplementedException();
        }

        public int IsThreeLegDown(AxisObject axis, ref bool isthreelegdn)
        {
            throw new NotImplementedException();
        }

        public int ChuckTiltMove(AxisObject axis, double offsetz0, double offsetz1, double offsetz2, double abspos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public int VMoveStop(AxisObject axis)
        {
            throw new NotImplementedException();
        }

        public int WaitForVMoveAxisMotionDone(AxisObject axis, long timeout = 0)
        {
            throw new NotImplementedException();
        }

        public int GetAuxPulse(AxisObject axis, ref int pos)
        {
            pos = 0;
            return 1;
        }

        public int IsFls(AxisObject axis, ref bool isfls)
        {
            throw new NotImplementedException();
        }

        public int IsRls(AxisObject axis, ref bool isrls)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}

using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Timers;
using SystemExceptions.MotionException;
////using ProberInterfaces.ThreadSync;
using TwinCatHelper;

namespace Motion
{
    public class TcAdsMotionBase : IMotionBase, IFactoryModule
    {
        

        private static int MonitoringInterValInms = 12;
        ManualResetEvent mreUpdateEvent = new ManualResetEvent(false);
        ManualResetEvent mreWaitForEvent = new ManualResetEvent(false);

        System.Timers.Timer _monitoringTimer;
        bool bStopUpdateThread;
        Thread UpdateThread;

        private ADSRouter _PLCModule;

        public ADSRouter PLCModule
        {
            get { return _PLCModule; }
            set { _PLCModule = value; }
        }

        private SymbolMap _SymbolMap;
        public SymbolMap SymbolMap
        {
            get { return _SymbolMap; }
            set { _SymbolMap = value; }
        }

        private SymbolHelper _SymbolService;
        public SymbolHelper SymbolService
        {
            get { return _SymbolService; }
            set { _SymbolService = value; }
        }
        private List<AxisStatus> _AxisStatusList;
        public List<AxisStatus> AxisStatusList
        {
            get
            {
                return _AxisStatusList;
            }
        }

        private CDXMachineStatus _PlcStatus;

        public CDXMachineStatus PlcStatus
        {
            get { return _PlcStatus; }
            set { _PlcStatus = value; }
        }
        private long _UpdateProcTime;

        public long UpdateProcTime
        {
            get { return _UpdateProcTime; }
            set { _UpdateProcTime = value; }
        }
        public readonly string TcFIlePath = "Twincat";
        public readonly string TcFIleName = "SymbolMap.json";
        public int PortNo => throw new NotImplementedException();

        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            set { _DevConnected = value; }
        }

        public bool IsMotionReady => throw new NotImplementedException();

        //private LockKey TcLockObj = new LockKey("Twin Cat");
        private static object TcLockObj = new object();

        public TcAdsMotionBase(string CtrlNum)
        {
            InitMotionProvider(CtrlNum);
            PlcStatus = new CDXMachineStatus();
        }
        public int InitMotionProvider(string ctrlNo)
        {
            try
            {
                _AxisStatusList = new List<AxisStatus>();
                _monitoringTimer = new System.Timers.Timer(MonitoringInterValInms);
                _monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                _monitoringTimer.Start();

                PLCModule = new ADSRouter(ctrlNo);
                SymbolService = new SymbolHelper(PLCModule, SymbolMap);
                PLCModule.InitComm();

                SymbolMapLoad();

                SymbolService.InitSymbolDictionary(SymbolMap);

                for (int axisindex = 0; axisindex < 3; axisindex++)
                {
                    _AxisStatusList.Add(new AxisStatus());
                }

                if (PLCModule.tcClient.IsConnected == true)
                {
                    bStopUpdateThread = false;
                    UpdateThread = new Thread(new ThreadStart(UpdateTwincatBaseProc));
                    UpdateThread.Start();
                }
            }
            catch (MotionException err)
            {
                LoggerManager.Error($"InitMotionProvider() MotionException occurred: " + err.Message);

                throw new MotionException("InitMotionProvider : MotionException occurred", EventCodeEnum.UNDEFINED);

                //throw new MotionException("InitMotionProvider : MotionException occurred");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitMotionProvider() Function error: " + err.Message);

                throw new MotionException("Init. system exception occurred", EventCodeEnum.UNDEFINED);

                //throw new MotionException("Init. system exception occurred");
            }

            return 0;
        }

        private int SymbolMapLoad()
        {
            int ret = -1;
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            object DeSerializedObj;
            string RootPath = this.FileManager().FileManagerParam.SystemParamRootDirectory;
            string fullPath;

            if (TcFIleName != "")
            {
                fullPath = RootPath + "\\" + TcFIlePath + "\\" + TcFIleName;
            }
            else
            {
                fullPath = RootPath + "\\" + TcFIleName;
            }
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(fullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                }

                if (File.Exists(fullPath) == false)
                {
                    SymbolMap = new SymbolMap();
                    SymbolMap.initSymbols();
                    RetVal = Extensions_IParam.SaveDataToJson(SymbolMap, fullPath);
                    if (RetVal == EventCodeEnum.PARAM_ERROR)
                    {
                        LoggerManager.Error($"[ElmoMotionBase] LoadSysParam(): Serialize Error");
                        return -1;
                    }
                }
                SymbolMap = new SymbolMap();
                object tmpParam = null;
                DeSerializedObj = Extensions_IParam.LoadDataFromJson(ref tmpParam, typeof(SymbolMap), fullPath);
                if (DeSerializedObj == null)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error($"[ElmoMotionBase] LoadSysParam(): DeSerialize Error");
                    return -1;
                }
                SymbolMap = (SymbolMap)DeSerializedObj;
                SymbolMap.InitVariableValue();
                ret = 0;
            }
            catch (Exception)
            {

                throw;
            }


            return ret;
        }
        public int ResultValidate(object funcname, int retcode)
        {
            if ((EnumMotionBaseReturnCode)retcode != EnumMotionBaseReturnCode.ReturnCodeOK)
            {
                LoggerManager.Error($"Function: {funcname} ReturnValue = {retcode} Error occurred in TwinCatMotionBase");
            }
            return retcode;
        }
        public int Abort(AxisObject axis)
        {
            return 0;
        }

        public int AbsMove(AxisObject axis, double abspos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;
            double targetPos;
            bool[] bAbsMove = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            double[] lrpos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            TCTrajectoryParams[] trjparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            targetPos = abspos;

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bAbsMove = (bool[])SymbolService.ReadSymbol(
                    SymbolMap.MoveCommandSymbols.AbsMoveRun, typeof(bool[]),
                    SymbolMap.MoveCommandSymbols.AbsMoveRun.DataNumber);

                    if (PlcStatus.bAxisMotionDone[axis.AxisIndex.Value] == 1)
                    {
                        if (!(axis is AxisObject))
                        {
                            LoggerManager.Error($"Axis is Not AxisObject Axis = {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }

                        if (abspos > axis.Param.PosSWLimit.Value)
                        {
                            targetPos = abspos;
                            LoggerManager.Error($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }
                        else if (abspos < axis.Param.NegSWLimit.Value)
                        {
                            targetPos = abspos;
                            LoggerManager.Error($"Negative SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                            return -1;
                        }
                        else
                        {
                            trjparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]),
                            SymbolMap.MotionParamSymbols.TrjParam.DataNumber);
                            trjparam[axis.AxisIndex.Value].Acc = axis.DtoP(axis.Param.Acceleration.Value);
                            trjparam[axis.AxisIndex.Value].Dcc = axis.DtoP(axis.Param.Decceleration.Value);
                            trjparam[axis.AxisIndex.Value].Velocity = axis.DtoP(axis.Param.Speed.Value);
                            trjparam[axis.AxisIndex.Value].Jerk = axis.Param.AccelerationJerk.Value;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MotionParamSymbols.TrjParam, trjparam);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);


                            lrpos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.AbsPos, typeof(double[]),
                                SymbolMap.CommandPosSymbols.AbsPos.DataNumber);
                            lrpos[axis.AxisIndex.Value] = axis.DtoP(abspos);
                            retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.AbsPos, lrpos);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            bAbsMove[axis.AxisIndex.Value] = true;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MoveCommandSymbols.AbsMoveRun, bAbsMove);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                        }

                    }
                    else
                    {
                        //Something Error 
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"AbsMove() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int AbsMove(AxisObject axis, double abspos, double finalVel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;
            double targetPos;
            bool[] bAbsMove = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            double[] lrpos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            TCTrajectoryParams[] trjparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            targetPos = abspos;

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bAbsMove = (bool[])SymbolService.ReadSymbol(
                    SymbolMap.MoveCommandSymbols.AbsMoveRun, typeof(bool[]),
                    SymbolMap.MoveCommandSymbols.AbsMoveRun.DataNumber);

                    if (PlcStatus.bAxisMotionDone[axis.AxisIndex.Value] == 1)
                    {
                        if (!(axis is AxisObject))
                        {
                            LoggerManager.Error($"Axis is Not AxisObject Axis = {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }

                        if (abspos > axis.Param.PosSWLimit.Value)
                        {
                            targetPos = abspos;
                            LoggerManager.Error($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }
                        else if (abspos < axis.Param.NegSWLimit.Value)
                        {
                            targetPos = abspos;
                            LoggerManager.Error($"Negative SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                            return -1;
                        }
                        else
                        {
                            trjparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]),
                            SymbolMap.MotionParamSymbols.TrjParam.DataNumber);
                            trjparam[axis.AxisIndex.Value].Acc = axis.DtoP(axis.Param.Acceleration.Value);
                            trjparam[axis.AxisIndex.Value].Dcc = axis.DtoP(axis.Param.Decceleration.Value);
                            trjparam[axis.AxisIndex.Value].Velocity = axis.DtoP(finalVel);
                            trjparam[axis.AxisIndex.Value].Jerk = axis.Param.AccelerationJerk.Value;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MotionParamSymbols.TrjParam, trjparam);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);


                            lrpos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.AbsPos, typeof(double[]),
                                SymbolMap.CommandPosSymbols.AbsPos.DataNumber);
                            lrpos[axis.AxisIndex.Value] = axis.DtoP(abspos);
                            retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.AbsPos, lrpos);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            bAbsMove[axis.AxisIndex.Value] = true;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MoveCommandSymbols.AbsMoveRun, bAbsMove);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                        }

                    }
                    else
                    {
                        //Something Error 
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"AbsMove() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int AbsMove(AxisObject axis, double abspos, double vel, double acc)
        {
            int retVal = -1;
            double targetPos;
            bool[] bAbsMove = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            double[] lrpos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            TCTrajectoryParams[] trjparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            targetPos = abspos;

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bAbsMove = (bool[])SymbolService.ReadSymbol(
                    SymbolMap.MoveCommandSymbols.AbsMoveRun, typeof(bool[]),
                    SymbolMap.MoveCommandSymbols.AbsMoveRun.DataNumber);

                    if (PlcStatus.bAxisMotionDone[axis.AxisIndex.Value] == 1)
                    {
                        if (!(axis is AxisObject))
                        {
                            LoggerManager.Error($"Axis is Not AxisObject Axis = {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }

                        if (abspos > axis.Param.PosSWLimit.Value)
                        {
                            targetPos = abspos;
                            LoggerManager.Error($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }
                        else if (abspos < axis.Param.NegSWLimit.Value)
                        {
                            targetPos = abspos;
                            LoggerManager.Error($"Negative SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                            return -1;
                        }
                        else
                        {
                            trjparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]),
                            SymbolMap.MotionParamSymbols.TrjParam.DataNumber);
                            trjparam[axis.AxisIndex.Value].Acc = axis.DtoP(acc);
                            trjparam[axis.AxisIndex.Value].Dcc = axis.DtoP(axis.Param.Decceleration.Value);
                            trjparam[axis.AxisIndex.Value].Velocity = axis.DtoP(vel);
                            trjparam[axis.AxisIndex.Value].Jerk = axis.Param.AccelerationJerk.Value;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MotionParamSymbols.TrjParam, trjparam);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);


                            lrpos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.AbsPos, typeof(double[]),
                                SymbolMap.CommandPosSymbols.AbsPos.DataNumber);
                            lrpos[axis.AxisIndex.Value] = axis.DtoP(abspos);
                            retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.AbsPos, lrpos);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            bAbsMove[axis.AxisIndex.Value] = true;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MoveCommandSymbols.AbsMoveRun, bAbsMove);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                        }

                    }
                    else
                    {
                        //Something Error 
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"AbsMove() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int AbsMove(AxisObject axis, double abspos, double vel, double acc, double dcc)
        {
            int retVal = -1;
            double targetPos;
            bool[] bAbsMove = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            double[] lrpos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            TCTrajectoryParams[] trjparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            targetPos = abspos;

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bAbsMove = (bool[])SymbolService.ReadSymbol(
                    SymbolMap.MoveCommandSymbols.AbsMoveRun, typeof(bool[]),
                    SymbolMap.MoveCommandSymbols.AbsMoveRun.DataNumber);

                    if (PlcStatus.bAxisMotionDone[axis.AxisIndex.Value] == 1)
                    {
                        if (!(axis is AxisObject))
                        {
                            LoggerManager.Error($"Axis is Not AxisObject Axis = {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }

                        if (abspos > axis.Param.PosSWLimit.Value)
                        {
                            targetPos = abspos;
                            LoggerManager.Error($"Positive SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }
                        else if (abspos < axis.Param.NegSWLimit.Value)
                        {
                            targetPos = abspos;
                            LoggerManager.Error($"Negative SW Limit occurred while AbsMove moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                            return -1;
                        }
                        else
                        {
                            trjparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]),
                            SymbolMap.MotionParamSymbols.TrjParam.DataNumber);
                            trjparam[axis.AxisIndex.Value].Acc = axis.DtoP(acc);
                            trjparam[axis.AxisIndex.Value].Dcc = axis.DtoP(dcc);
                            trjparam[axis.AxisIndex.Value].Velocity = axis.DtoP(vel);
                            trjparam[axis.AxisIndex.Value].Jerk = axis.Param.AccelerationJerk.Value;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MotionParamSymbols.TrjParam, trjparam);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);


                            lrpos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.AbsPos, typeof(double[]),
                                SymbolMap.CommandPosSymbols.AbsPos.DataNumber);
                            lrpos[axis.AxisIndex.Value] = axis.DtoP(abspos);
                            retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.AbsPos, lrpos);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            bAbsMove[axis.AxisIndex.Value] = true;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MoveCommandSymbols.AbsMoveRun, bAbsMove);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                        }

                    }
                    else
                    {
                        //Something Error 
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"AbsMove() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int AlarmReset(AxisObject axis)
        {
            int retVal = -1;
            bool[] bresetFlag = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bresetFlag = (bool[])SymbolService.ReadSymbol(SymbolMap.ControlSymbols.AmpFaultClearCommand, typeof(bool[]),
                    SymbolMap.ControlSymbols.AmpFaultClearCommand.DataNumber);

                    bresetFlag[axis.AxisIndex.Value] = true;

                    retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.AmpFaultClearCommand, bresetFlag);

                    retVal = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"AlarmReset() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int AmpFaultClear(AxisObject axis)
        {
            int retVal = -1;
            bool[] bclearFlag = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bclearFlag = (bool[])SymbolService.ReadSymbol(SymbolMap.ControlSymbols.AmpFaultClearCommand, typeof(bool[]), (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);

                    bclearFlag[axis.AxisIndex.Value] = true;

                    retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.AmpFaultClearCommand, bclearFlag);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                    retVal = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"AmpFaultClear() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int ApplyAxisConfig(AxisObject axis)
        {
            int retVal = -1;
            try
            {
                VirtualAxisRef[] vrAxisobj = new VirtualAxisRef[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
                TCTrajectoryParams[] trjparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
                trjparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]), (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
                vrAxisobj = (VirtualAxisRef[])SymbolService.ReadSymbol(SymbolMap.AxisSymbols.VirtualAxisObj, typeof(VirtualAxisRef[]), (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);

                vrAxisobj[axis.AxisIndex.Value].AxisIndex = axis.AxisIndex.Value;
                vrAxisobj[axis.AxisIndex.Value].HomeOffset = axis.DtoP(axis.Param.HomeOffset.Value);
                vrAxisobj[axis.AxisIndex.Value].SWNegLimit = axis.DtoP(axis.Param.NegSWLimit.Value);
                vrAxisobj[axis.AxisIndex.Value].SWPosLimit = axis.DtoP(axis.Param.PosSWLimit.Value);

                trjparam[axis.AxisIndex.Value].Acc = axis.DtoP(axis.Param.Acceleration.Value);
                trjparam[axis.AxisIndex.Value].Dcc = axis.DtoP(axis.Param.Decceleration.Value);
                trjparam[axis.AxisIndex.Value].Jerk = axis.Param.AccelerationJerk.Value;
                trjparam[axis.AxisIndex.Value].Velocity = axis.DtoP(axis.Param.Speed.Value);


                retVal = SymbolService.WriteSymbol(SymbolMap.MotionParamSymbols.TrjParam, trjparam);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                retVal = SymbolService.WriteSymbol(SymbolMap.AxisSymbols.VirtualAxisObj, vrAxisobj);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                bool[] axisenable = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
                axisenable = (bool[])SymbolService.ReadSymbol(SymbolMap.ControlSymbols.EnableAmps, typeof(bool[]),
                    SymbolMap.ControlSymbols.EnableAmps.DataNumber);

                axisenable[axis.AxisIndex.Value] = true;

                retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.EnableAmps, axisenable);
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);


            }

            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred. in TwinCatMotionBase");
                //LoggerManager.Error($"ApplyAxisConfig() Function error: " + err.Message);
                LoggerManager.Exception(err);

                return -1;
            }
            return retVal;
        }

        public int ClearUserLimit(AxisObject axis)
        {
            return 0;
        }

        public int ClearUserLimit(int axisNumber)
        {
            return 0;
        }

        public int ConfigCapture(AxisObject axis, EnumMotorDedicatedIn input)
        {
            return 0;
        }

        public int DeInitMotionService()
        {
            SymbolService.DeleteHandles();
            PLCModule.DeInitComm();

            _monitoringTimer?.Stop();

            return 0;
        }

        public int DisableAxis(AxisObject axis)
        {
            int retVal = -1;

            bool[] bEnableFlags = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bEnableFlags = (bool[])SymbolService.ReadSymbol(SymbolMap.ControlSymbols.EnableAmps, typeof(bool[]), (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
                    bEnableFlags[axis.AxisIndex.Value] = false;

                    retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.EnableAmps.Handle, bEnableFlags);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"DisableAxis() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int DisableCapture(AxisObject axis)
        {
            return -1;
        }

        public int EnableAxis(AxisObject axis)
        {
            int retVal = -1;

            bool[] bEnableFlags = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bEnableFlags = (bool[])SymbolService.ReadSymbol(SymbolMap.ControlSymbols.EnableAmps, typeof(bool[]), (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
                    bEnableFlags[axis.AxisIndex.Value] = true;

                    retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.EnableAmps.Handle, bEnableFlags);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"EnableAxis() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public double GetAccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            TCTrajectoryParams[] motparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            double acc = 0;

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    motparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]), (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
                    acc = axis.PtoD(motparam[axis.AxisIndex.Value].Acc);
                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetAccel() Function error: " + err.Message);
                return -1;
            }

            return acc;
        }

        public int GetActualPosition(AxisObject axis, ref double pos)
        {
            int retVal = -1;
            pos = 0;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    pos = axis.PtoD(PlcStatus.dblActPos[axis.AxisIndex.Value]);
                    retVal = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetActualPosition() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int GetActualPulse(AxisObject axis, ref int pos)
        {
            int retVal = -1;
            pos = 0;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    pos = (int)PlcStatus.dblActPos[axis.AxisIndex.Value];
                    retVal = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetActualPulse() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int GetAlarmCode(AxisObject axis, ref ushort alarmcode)
        {
            alarmcode = 0;
            return 0;
        }

        public int GetAxisInputs(AxisObject axis, ref uint instatus)
        {
            instatus = 0;
            return 0;
        }

        public int GetAxisState(AxisObject axis, ref int state)
        {
            return GetAxisState(axis.AxisIndex.Value, ref state);
        }

        private int GetAxisState(int axisindex, ref int state)
        {
            int retVal = -1;
            state = -1;
            try
            {
                if (PLCModule.tcClient.IsConnected == true)
                {
                    //using(Locker locker = new Locker(TcLockObj))
                    //{
                    lock (TcLockObj)
                    {
                        state = (int)PlcStatus.AxisState[axisindex];
                        retVal = 0;
                    }
                }
                else
                {
                    retVal = -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetAxisState() Function error: " + err.Message);
                return -1;
            }


            return retVal;
        }
        private int GetAxisStatus(int axisindex, AxisStatus status)
        {
            int retVal = -1;
            int state = -1;

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    retVal = GetAxisState(axisindex, ref state);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                    status.State = (EnumAxisState)state;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetAxisStatus() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }


        public int GetAxisStatus(AxisObject axis)
        {
            int retVal = -1;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    retVal = GetAxisStatus(axis.AxisIndex.Value, axis.Status);
                    retVal = ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }


            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetAxisStatus() Function error: " + err.Message);
                return -1;
            }
            return retVal;

        }

        public int GetCaptureStatus(int axisindex, AxisStatus status)
        {
            return 0;
        }

        public int GetCmdPosition(AxisObject axis, ref double cmdpos)
        {
            int retVal = -1;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    cmdpos = axis.PtoD(PlcStatus.dblActPos[axis.AxisIndex.Value]);
                    retVal = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetAxisStatus() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int GetCommandPosition(AxisObject axis, ref double pos)
        {
            double cmdpos = 0;
            int retVal = -1;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    retVal = GetCmdPosition(axis, ref cmdpos);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetCommandPosition() Function error: " + err.Message);
                retVal = -1;
            }
            finally
            {
                pos = cmdpos;
            }
            return retVal;
        }

        public int GetCommandPulse(AxisObject axis, ref int pos)
        {
            int retVal = -1;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    pos = Convert.ToInt32(PlcStatus.dblTargetPos[axis.AxisIndex.Value]);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetAxisStatus() Function error: " + err.Message);
                return -1;
            }
            finally
            {
                pos = Convert.ToInt32(PlcStatus.dblTargetPos[axis.AxisIndex.Value]);
            }

            return retVal;
        }

        public double GetDeccel(AxisObject axis, EnumTrjType trjtype, double ovrd = 1)
        {
            TCTrajectoryParams[] motparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            double dec = 0;

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    motparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]), (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
                    dec = axis.PtoD(motparam[axis.AxisIndex.Value].Dcc);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetDeccel() Function error: " + err.Message);
                return -1;
            }

            return dec;
        }

        public bool GetHomeSensorState(AxisObject axis)
        {
            throw new NotImplementedException();
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

        public int GetMotorAmpEnable(AxisObject axis, ref bool turnAmp)
        {
            int retVal = -1;
            UInt32[] bEnableFlags = new UInt32[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            bool ret = false;

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bEnableFlags = PlcStatus.bAxisEnable;
                    if (bEnableFlags[axis.AxisIndex.Value] == 1)
                    {
                        ret = true;
                    }
                    else
                    {
                        ret = false;
                    }
                }
                turnAmp = ret;
                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetMotorAmpEnable() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int GetPosError(AxisObject axis, ref double poserr)
        {
            poserr = 0;
            return 0;
        }

        public int GetVelocity(AxisObject axis, EnumTrjType trjtype,ref double vel ,double ovrd = 1)
        {
            int retVal = -1;
            TCTrajectoryParams[] motparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    motparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]), (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
                    vel = motparam[axis.AxisIndex.Value].Velocity;
                    retVal = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetVelocity() Function error: " + err.Message);
                return -1;
            }
            return retVal;

        }

        public int Halt(AxisObject axis, double value)
        {
            return -1;
        }

        public bool HasAlarm(AxisObject axis)
        {
            throw new NotImplementedException();
        }
        private int WaitforHomingDone(AxisObject axis,int timeout)
        {
            int retVal = -1;
            int hSymbol;
            bool runFlag = true;
            Stopwatch elapsedStopWatch = new Stopwatch();

            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            UInt32 doneFlag;
            try
            {
                mreWaitForEvent.WaitOne(1);

                do
                {
                    mreWaitForEvent.WaitOne(1);
                    SymbolService.MachineStatusSymbolDict.TryGetValue(SymbolMap.MachineStatusSymbols.MachineStatus, out hSymbol);
                    PlcStatus = (CDXMachineStatus)PLCModule.tcClient.ReadAny(hSymbol, typeof(CDXMachineStatus));
                    doneFlag = PlcStatus.bAxisHomingDone;

                    if (doneFlag == 1)
                    {
                        runFlag = false;
                        retVal = 0;
                    }
                    else
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > axis.Param.TimeOut.Value)
                            {

                                LoggerManager.Error($"WaitForAxisMotionDone({axis.AxisIndex}) : Axis motion timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = -2;
                                Stop(axis);
                            }
                        }
                    }

                } while (runFlag == true);


                
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"WaitForAxisMotionDone() Function error: {0}" + err.Message));
                //LoggerManager.Error($"WaitForAxisMotionDone({0}) : Axis motion error occurred. Err code = {1}", axis.AxisIndex, axis.Status.ErrCode));
                LoggerManager.Exception(err);

                return -1;
            }
            finally
            {
                elapsedStopWatch.Stop();
            }
            return retVal;
        }
        public int Homming(AxisObject axis)
        {
            int retVal = -1;
            bool[] bhomingFlag = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            Stopwatch stw = new Stopwatch();

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bhomingFlag = (bool[])SymbolService.ReadSymbol(SymbolMap.ControlSymbols.HomingCommand, typeof(bool[]),
                    SymbolMap.ControlSymbols.HomingCommand.DataNumber);

                    bhomingFlag[axis.AxisIndex.Value] = true;
                    retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.HomingCommand, bhomingFlag);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                    retVal = WaitforHomingDone(axis, axis.Param.TimeOut.Value);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Homming() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int Homming(AxisObject axis, bool reverse, EnumIndexConfig input, double homeoffset)
        {
            return -1;
        }

        public int Homming(AxisObject axis, bool reverse, int input, double homeoffset)
        {
            return -1;
        }

        public int Homming(AxisObject axis, bool reverse, EnumMCInputs input, double homeoffset)
        {
            return -1;
        }
        private void _monitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _monitoringTimer.Stop();
            //if (setState == false)
            //{
            //    mreUpdateEvent.Set();
            //    setState = true;
            //}
            //else
            //{
            //    mreUpdateEvent.Reset();
            //    setState = false;
            //}
            _monitoringTimer.Start();
        }

        public int IsInposition(AxisObject axis,ref bool val)
        {
            return IsInposition(axis.AxisIndex.Value,ref val);
        }
        private int IsInposition(int axisindex,ref bool val)
        {
            int retVal = -1;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    if (PlcStatus.Inposition[axisindex] == 1)
                    {
                        val = true;
                    }
                    else
                    {
                        val = false;
                    }
                }
                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"IsInposition() Function error: " + err.Message);
            }
            finally
            {
                val = false;
            }
            return retVal;

        }

        public int IsMotorEnabled(AxisObject axis,ref bool val)
        {
            return IsMotorEnabled(axis.AxisIndex.Value,ref val);
        }
        private int IsMotorEnabled(int axisindex,ref bool val)
        {
            int retVal = -1;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    if (PlcStatus.bAxisEnable[axisindex] == 1)
                    {
                        val = true;
                    }
                    else
                    {
                        val = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"IsMotorEnabled() Function error: " + err.Message);
            }
            finally
            {
                val = false;
            }
            return retVal;
        }

        public int IsMoving(AxisObject axis,ref bool val)
        {
            return IsMoving(axis.AxisIndex.Value,ref val);
        }
        public int IsMoving(int axisindex,ref bool val)
        {
            int retVal = -1;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    if (PlcStatus.AxisState[axisindex] == (int)EnumAxisState.MOVING)
                    {
                        val = true;
                    }
                    else
                    {
                        val = false;
                    }
                    retVal = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"IsMotorEnabled() Function error: " + err.Message);
            }
            finally
            {
                val = false;
            }
            return retVal;
        }

        public int JogMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            return -1;
        }

        public int JogMove(AxisObject axis, double velocity, double accel, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            return -1;
        }

        public int LinInterpolation(AxisObject[] axes, double[] poss)
        {
            return -1;
        }

        public int MonitorForAxisMotion(ProbeAxisObject axis, double pos, double allowanceRange, long maintainTime = 0, long timeout = 0)
        {
            return -1;
        }

        public int NotchFinding(AxisObject axis, EnumMotorDedicatedIn input)
        {
            return -1;
        }

        public int OriginSet(AxisObject axis, double pos)
        {
            int retVal = -1;
            double[] originpos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            bool[] origincmd = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    originpos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.OriingPos, typeof(double[]),
                    SymbolMap.CommandPosSymbols.OriingPos.DataNumber);
                    originpos[axis.AxisIndex.Value] = pos;
                    retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.OriingPos, originpos);
                    retVal = ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                    origincmd = (bool[])SymbolService.ReadSymbol(SymbolMap.ControlSymbols.CMDOriginPosSet, typeof(bool[]),
                        SymbolMap.CommandPosSymbols.OriingPos.DataNumber);
                    origincmd[axis.AxisIndex.Value] = true;
                    retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.CMDOriginPosSet, origincmd);
                    retVal = ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"OriginSet() Function error: " + err.Message);
                return -1;
            }
            return retVal;
        }

        public int Pause(AxisObject axis)
        {
            return -1;
        }

        public int RelMove(AxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;
            double targetPos;
            bool[] bRelMove = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            double[] lrpos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            TCTrajectoryParams[] trjparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            int apos = 0;
            GetActualPulse(axis, ref apos);
            targetPos = axis.PtoD(apos) + pos;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bRelMove = (bool[])SymbolService.ReadSymbol(
                    SymbolMap.MoveCommandSymbols.RelMoveRun, typeof(bool[]),
                    SymbolMap.MoveCommandSymbols.RelMoveRun.DataNumber);

                    if (PlcStatus.bAxisMotionDone[axis.AxisIndex.Value] == 1)
                    {
                        if (targetPos > axis.Param.PosSWLimit.Value)
                        {

                            targetPos = axis.Status.Pulse.Actual;
                            LoggerManager.Error($"Positive SW Limit occurred while Relative moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }
                        else if (targetPos < axis.Param.NegSWLimit.Value)
                        {
                            targetPos = axis.Status.Pulse.Actual;
                            LoggerManager.Error($"Negative SW Limit occurred while Relative moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                            return -1;
                        }
                        else
                        {
                            trjparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]),
                                                        SymbolMap.MotionParamSymbols.TrjParam.DataNumber);
                            trjparam[axis.AxisIndex.Value].Acc = axis.DtoP(axis.Param.Acceleration.Value);
                            trjparam[axis.AxisIndex.Value].Dcc = axis.DtoP(axis.Param.Decceleration.Value);
                            trjparam[axis.AxisIndex.Value].Velocity = axis.DtoP(axis.Param.Speed.Value);
                            trjparam[axis.AxisIndex.Value].Jerk = axis.Param.AccelerationJerk.Value;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MotionParamSymbols.TrjParam, trjparam);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            lrpos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.RelPos, typeof(double[]),
                                SymbolMap.CommandPosSymbols.RelPos.DataNumber);
                            lrpos[axis.AxisIndex.Value] = axis.DtoP(pos);

                            retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.RelPos, lrpos);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            bRelMove[axis.AxisIndex.Value] = true;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MoveCommandSymbols.RelMoveRun, bRelMove);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                        }

                    }

                    else
                    {
                        //Something Error 
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"RelMove() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int RelMove(AxisObject axis, double pos, double vel, double acc)
        {
            int retVal = -1;
            int hSymbol;
            double targetPos;
            bool[] bRelMove = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            double[] lrpos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            TCTrajectoryParams[] trjparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            int apos = 0;
            GetActualPulse(axis, ref apos);
            targetPos = axis.PtoD(apos) + pos;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    SymbolService.MachineStatusSymbolDict.TryGetValue(SymbolMap.MachineStatusSymbols.MachineStatus, out hSymbol);
                    PlcStatus = (CDXMachineStatus)PLCModule.tcClient.ReadAny(hSymbol, typeof(CDXMachineStatus));

                    bRelMove = (bool[])SymbolService.ReadSymbol(
                    SymbolMap.MoveCommandSymbols.RelMoveRun, typeof(bool[]),
                    SymbolMap.MoveCommandSymbols.RelMoveRun.DataNumber);

                    if (PlcStatus.bAxisMotionDone[axis.AxisIndex.Value] == 1)
                    {
                        if (targetPos > axis.Param.PosSWLimit.Value)
                        {

                            targetPos = axis.Status.Pulse.Actual;
                            LoggerManager.Error($"Positive SW Limit occurred while Relative moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }
                        else if (targetPos < axis.Param.NegSWLimit.Value)
                        {
                            targetPos = axis.Status.Pulse.Actual;
                            LoggerManager.Error($"Negative SW Limit occurred while Relative moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                            return -1;
                        }
                        else
                        {
                            trjparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]),
                                                        SymbolMap.MotionParamSymbols.TrjParam.DataNumber);
                            trjparam[axis.AxisIndex.Value].Acc = axis.DtoP(acc);
                            trjparam[axis.AxisIndex.Value].Dcc = axis.DtoP(acc);
                            trjparam[axis.AxisIndex.Value].Velocity = axis.DtoP(vel);
                            trjparam[axis.AxisIndex.Value].Jerk = axis.Param.AccelerationJerk.Value;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MotionParamSymbols.TrjParam, trjparam);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            lrpos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.RelPos, typeof(double[]),
                                SymbolMap.CommandPosSymbols.RelPos.DataNumber);
                            lrpos[axis.AxisIndex.Value] = axis.DtoP(pos);
                            retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.RelPos, lrpos);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            bRelMove[axis.AxisIndex.Value] = true;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MoveCommandSymbols.RelMoveRun, bRelMove);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                        }

                    }

                    else
                    {
                        //Something Error 
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"RelMove() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int RelMove(AxisObject axis, double pos, double vel, double acc, double dcc)
        {
            int retVal = -1;
            double targetPos;
            bool[] bRelMove = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            double[] lrpos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            TCTrajectoryParams[] trjparam = new TCTrajectoryParams[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            int apos = 0;
            GetActualPulse(axis, ref apos);
            targetPos = axis.PtoD(apos) + pos;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bRelMove = (bool[])SymbolService.ReadSymbol(
                    SymbolMap.MoveCommandSymbols.RelMoveRun, typeof(bool[]),
                    SymbolMap.MoveCommandSymbols.RelMoveRun.DataNumber);

                    if (PlcStatus.bAxisMotionDone[axis.AxisIndex.Value] == 1)
                    {
                        if (targetPos > axis.Param.PosSWLimit.Value)
                        {

                            targetPos = axis.Status.Pulse.Actual;
                            LoggerManager.Error($"Positive SW Limit occurred while Relative moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.PosSWLimit.Value}");
                            return -1;
                        }
                        else if (targetPos < axis.Param.NegSWLimit.Value)
                        {
                            targetPos = axis.Status.Pulse.Actual;
                            LoggerManager.Error($"Negative SW Limit occurred while Relative moving for Axis {axis.Label}, Target = {targetPos}, Limit = {axis.Param.NegSWLimit.Value}");
                            return -1;
                        }
                        else
                        {
                            trjparam = (TCTrajectoryParams[])SymbolService.ReadSymbol(SymbolMap.MotionParamSymbols.TrjParam, typeof(TCTrajectoryParams[]),
                                                        SymbolMap.MotionParamSymbols.TrjParam.DataNumber);
                            trjparam[axis.AxisIndex.Value].Acc = axis.DtoP(acc);
                            trjparam[axis.AxisIndex.Value].Dcc = axis.DtoP(dcc);
                            trjparam[axis.AxisIndex.Value].Velocity = axis.DtoP(vel);
                            trjparam[axis.AxisIndex.Value].Jerk = axis.Param.AccelerationJerk.Value;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MotionParamSymbols.TrjParam, trjparam);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            lrpos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.RelPos, typeof(double[]),
                                SymbolMap.CommandPosSymbols.RelPos.DataNumber);
                            lrpos[axis.AxisIndex.Value] = axis.DtoP(pos);

                            retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.RelPos, lrpos);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                            bRelMove[axis.AxisIndex.Value] = true;
                            retVal = SymbolService.WriteSymbol(SymbolMap.MoveCommandSymbols.RelMoveRun, bRelMove);
                            ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                        }

                    }

                    else
                    {
                        //Something Error 
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"RelMove() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int ResultValidate(int retcode)
        {
            throw new NotImplementedException();
        }

        public int Resume(AxisObject axis)
        {
            return -1;
        }

        public int SetFeedrate(AxisObject axis, double normfeedrate, double pausefeedrate)
        {
            return -1;
        }

        public int SetHWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            return -1;
        }

        public int SetHWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            return -1;
        }

        public int SetLimitSWNegAct(AxisObject axis, EnumEventActionType action)
        {
            return -1;
        }

        public int SetLimitSWPosAct(AxisObject axis, EnumEventActionType action)
        {
            return -1;
        }

        public int SetMotorAmpEnable(AxisObject axis, bool turnAmp)
        {
            int retVal = -1;

            bool[] bEnableFlags = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bEnableFlags = (bool[])SymbolService.ReadSymbol(SymbolMap.ControlSymbols.EnableAmps, typeof(bool[]),
                        SymbolMap.ControlSymbols.EnableAmps.DataNumber);
                    bEnableFlags[axis.AxisIndex.Value] = turnAmp;
                    retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.EnableAmps.Handle, bEnableFlags);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetMotorAmpEnable() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int SetOverride(AxisObject axis, double ovrd)
        {
            return -1;
        }

        public int SetPosition(AxisObject axis, double pos)
        {
            int retVal = -1;

            double[] lrSetPos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    lrSetPos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.SetPosition, typeof(double[]),
                        SymbolMap.CommandPosSymbols.SetPosition.DataNumber);
                    lrSetPos[axis.AxisIndex.Value] = axis.DtoP(pos);
                    retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.SetPosition.Handle, lrSetPos);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetPosition() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int SetPulse(AxisObject axis, double pos)
        {
            int retVal = -1;

            double[] lrSetPos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                ////{
                ///                    
                lock(TcLockObj)
                {
                    lrSetPos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.SetPosition, typeof(double[]),
                        SymbolMap.CommandPosSymbols.SetPosition.DataNumber);
                    lrSetPos[axis.AxisIndex.Value] = pos;
                    retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.SetPosition.Handle, lrSetPos);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetPulse() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int SetSettlingTime(AxisObject axis, double settlingTime)
        {
            int retVal = -1;

            try
            {
                axis.SettlingTime = (long)(settlingTime);
                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetPulse() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int SetSwitchAction(AxisObject axis, EnumDedicateInputs input, EnumEventActionType action, EnumInputLevel reverse)
        {
            return -1;
        }

        public int SetSWNegLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            return -1;
        }

        public int SetSWPosLimAction(AxisObject axis, EnumEventActionType action, EnumPolarity polarity)
        {
            return -1;
        }

        public int SetZeroPosition(AxisObject axis)
        {
            int retVal = -1;

            double[] lrSetPos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    lrSetPos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.SetPosition, typeof(double[]),
                        SymbolMap.CommandPosSymbols.SetPosition.DataNumber);
                    lrSetPos[axis.AxisIndex.Value] = 0;
                    retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.SetPosition.Handle, lrSetPos);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetZeroPosition() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int StartScanPosCapt(AxisObject axis, EnumMotorDedicatedIn MotorDedicatedIn)
        {
            return -1;
        }
        public int VMoveStop(AxisObject axis)
        {
            int retVal = -1;
            return retVal;
        }
        public int Stop(AxisObject axis)
        {
            int retVal = -1;

            bool[] bstopFlag = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    bstopFlag = (bool[])SymbolService.ReadSymbol(SymbolMap.MoveCommandSymbols.MoveStop, typeof(bool[]),
                        SymbolMap.CommandPosSymbols.SetPosition.DataNumber);
                    bstopFlag[axis.AxisIndex.Value] = true;
                    retVal = SymbolService.WriteSymbol(SymbolMap.MoveCommandSymbols.MoveStop.Handle, bstopFlag);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Stop() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }

        public int StopScanPosCapt(AxisObject axis)
        {
            return -1;
        }

        public int VMove(AxisObject axis, double velocity, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            int retVal = -1;

            double[] VPos = new double[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            bool[] vmoveFlag = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    VPos = (double[])SymbolService.ReadSymbol(SymbolMap.CommandPosSymbols.SetPosition, typeof(double[]),
                        SymbolMap.CommandPosSymbols.SetPosition.DataNumber);
                    VPos[axis.AxisIndex.Value] = axis.DtoP(velocity);

                    retVal = SymbolService.WriteSymbol(SymbolMap.CommandPosSymbols.SetPosition.Handle, VPos);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);


                    vmoveFlag = (bool[])SymbolService.ReadSymbol(SymbolMap.MoveCommandSymbols.VMoveRun, typeof(bool[]),
                        SymbolMap.MoveCommandSymbols.VMoveRun.DataNumber);
                    vmoveFlag[axis.AxisIndex.Value] = true;

                    retVal = SymbolService.WriteSymbol(SymbolMap.MoveCommandSymbols.VMoveRun.Handle, vmoveFlag);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"VMove() Function error: " + err.Message);
                return -1;
            }

            return retVal;
        }
        public int SetLoadCellZero()
        {
            int retVal = -1;

            bool setzeroflag = true;
            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.CmdSetZeroForce.Handle, setzeroflag);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"VMove() Function error: " + err.Message);
                return -1;
            }
            return retVal;
        }

        public int SetDualLoop(bool dualloop)
        {
            int retVal = -1;
            bool bdualLoop = dualloop;

            try
            {
                //using(Locker locker = new Locker(TcLockObj))
                //{
                lock (TcLockObj)
                {
                    retVal = SymbolService.WriteSymbol(SymbolMap.ControlSymbols.CmdDualLoopEnable.Handle, bdualLoop);
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            return retVal;
        }

        public int WaitForAxisEvent(AxisObject axis, EnumAxisState waitfor, double distlimit, long timeout = -1)
        {
            return -1;
        }
        public int WaitForVMoveAxisMotionDone(AxisObject axis, long timeout = 0)
        {
            return -1;
        }
        public int WaitForAxisMotionDone(AxisObject axis, long timeout = 0)
        {

            #region OldCode
            //int retVal = -1;

            //Stopwatch elapsedStopWatch = new Stopwatch();

            //elapsedStopWatch.Reset();
            //elapsedStopWatch.Start();

            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            //stw.Restart();
            //stw.Start();

            ////LoggerManager.Debug($"WaitForAxisMotionDone({axis.AxisIndex.Value}): Start WaitForAxisMotionDone.");

            ////timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //try
            //{
            //    bool runFlag = true;

            //    mreWaitForEvent.WaitOne(1);
            //    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("First Enter GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));

            //    GetAxisStatus(axis);
            //    axis.Status.AxisBusy = IsMoving(axis);
            //    axis.Status.AxisEnabled = IsMotorEnabled(axis);
            //    axis.Status.Inposition = IsInposition(axis);
            //    //timeStamp.Add(new KeyValuePair<string, long>(string.Format("First End GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));

            //    do
            //    {
            //        //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //        retVal = GetAxisStatus(axis);
            //        retVal = ResultValidate(MethodBase.GetCurrentMethod(), retVal);

            //        axis.Status.AxisBusy = IsMoving(axis);
            //        axis.Status.AxisEnabled = IsMotorEnabled(axis);
            //        axis.Status.Inposition = IsInposition(axis);
            //        //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter WaitOne Before GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //        mreWaitForEvent.WaitOne(1);
            //        if (axis.Status.Inposition == false & axis.Status.AxisBusy == true & axis.Status.ErrCode == 0x0000)
            //        {
            //            if (timeout != 0)
            //            {
            //                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
            //                {

            //                    LoggerManager.Error($"WaitForAxisMotionDone({0}) : Axis motion timeout error occurred. Timeout = {1}ms", axis.AxisIndex.Value, timeout));

            //                    runFlag = false;
            //                    retVal = -2;
            //                    Stop(axis);
            //                }
            //            }
            //            else
            //            {
            //                runFlag = true;
            //            }
            //        }
            //        else
            //        {
            //            runFlag = false;
            //            if (axis.Status.ErrCode == 0x0000)
            //            {
            //                retVal = 0;
            //                //LoggerManager.Debug(string.Format("WaitForAxisMotionDone({0}) : Axis motion done.", axis.AxisIndex.Value));
            //            }
            //            else
            //            {
            //                Stop(axis);
            //                retVal = -1;
            //                LoggerManager.Error($"WaitForAxisMotionDone({0}) : Axis motion error occurred. Err code = {1}", axis.AxisIndex.Value, axis.Status.ErrCode));
            //            }
            //        }
            //        //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //    } while (runFlag == true);


            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Error($"WaitForAxisMotionDone() Function error: {0}" + err.Message));
            //    LoggerManager.Error($"WaitForAxisMotionDone({0}) : Axis motion error occurred. Err code = {1}", axis.AxisIndex.Value, axis.Status.ErrCode));
            //    return -1;
            //}
            //finally
            //{
            //    elapsedStopWatch.Stop();

            //    //double pos=0;
            //    // GetCmdPosition(axis, ref pos);
            //    // axis.Status.Position.Ref = pos;
            //}
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //stw.Stop();
            ////int step = 0;
            ////foreach (var item in timeStamp)
            ////{
            ////    step++;
            ////    LoggerManager.Debug($string.Format("Time Stamp [{0}] - Desc: {1}, Time: {2}", step, item.Key, item.Value));
            ////}
            ////LoggerManager.Debug($"WaitForAxisMotionDone({axis.AxisIndex.Value}): End WaitForAxisMotionDone.");
            //return retVal;
            #endregion
            int retVal = -1;
            int hSymbol;
            bool runFlag = true;
            Stopwatch elapsedStopWatch = new Stopwatch();

            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            UInt32[] doneFlag = new UInt32[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            try
            {
                mreWaitForEvent.WaitOne(1);

                do
                {
                    mreWaitForEvent.WaitOne(1);
                    SymbolService.MachineStatusSymbolDict.TryGetValue(SymbolMap.MachineStatusSymbols.MachineStatus, out hSymbol);
                    PlcStatus = (CDXMachineStatus)PLCModule.tcClient.ReadAny(hSymbol, typeof(CDXMachineStatus));
                    doneFlag = PlcStatus.bAxisMotionDone;

                    if (doneFlag[axis.AxisIndex.Value] == 1)
                    {
                        runFlag = false;
                        retVal = 0;
                    }
                    else
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
                    }

                } while (runFlag == true);


                if (axis.SettlingTime > 0)
                {
                    //delay.DelayFor(axis.SettlingTime / 2);
                    Thread.Sleep((int)axis.SettlingTime / 2);

                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"WaitForAxisMotionDone() Function error: {0}" + err.Message));
                //LoggerManager.Error($"WaitForAxisMotionDone({0}) : Axis motion error occurred. Err code = {1}", axis.AxisIndex.Value, axis.Status.ErrCode));
                LoggerManager.Exception(err);

                return -1;
            }
            finally
            {
                elapsedStopWatch?.Stop();
            }
            return retVal;
        }

        public int WaitForAxisMotionDone(AxisObject axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0)
        {
            #region OldCode

            //int retVal = -1;
            //Stopwatch elapsedStopWatch = new Stopwatch();

            //elapsedStopWatch.Reset();
            //elapsedStopWatch.Start();

            //Stopwatch stw = new Stopwatch();
            //List<KeyValuePair<string, long>> timeStamp;
            //timeStamp = new List<KeyValuePair<string, long>>();

            //stw.Restart();
            //stw.Start();
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Start {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //try
            //{
            //    bool runFlag = true;
            //    bool state = false;
            //    bool isEnter = false;

            //    mreUpdateEvent.WaitOne(1);
            //    timeStamp.Add(new KeyValuePair<string, long>(string.Format("First Enter GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //    GetAxisStatus(axis);

            //    axis.Status.AxisBusy = IsMoving(axis);
            //    axis.Status.AxisEnabled = IsMotorEnabled(axis);
            //    axis.Status.Inposition = IsInposition(axis);
            //    timeStamp.Add(new KeyValuePair<string, long>(string.Format("First End GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));

            //    do
            //    {
            //        state = GetSourceLevel();
            //        if (isEnter && (state == resumeLevel))
            //        {
            //            Resume(axis);
            //            isEnter = false;
            //        }
            //        else if (!isEnter && (!state == resumeLevel))
            //        {
            //            Stop(axis);
            //            isEnter = true;
            //        }


            //        timeStamp.Add(new KeyValuePair<string, long>(string.Format("Enter GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //        GetAxisStatus(axis);

            //        axis.Status.AxisBusy = IsMoving(axis);
            //        axis.Status.AxisEnabled = IsMotorEnabled(axis);
            //        axis.Status.Inposition = IsInposition(axis);

            //        mreUpdateEvent.WaitOne(1);
            //        if (axis.Status.Inposition == false & axis.Status.ErrCode == 0x0000)
            //        {
            //            if (timeout != 0)
            //            {
            //                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
            //                {

            //                    LoggerManager.Error($"WaitForAxisMotionDone({0}) : Axis motion timeout error occurred. Timeout = {1}ms", axis.AxisIndex.Value, timeout));

            //                    runFlag = false;
            //                    retVal = -2;
            //                    Stop(axis);
            //                    //  throw new MotionException(string.Format("WaitForAxisMotionDone({0}) : Axis motion timeout error occurred. Timeout = {1}ms. ", axis.AxisIndex.Value, timeout));
            //                }
            //            }
            //            else
            //            {
            //                runFlag = true;
            //            }
            //        }
            //        else
            //        {
            //            runFlag = false;
            //            if (axis.Status.ErrCode == 0x0000)
            //            {
            //                retVal = 0;
            //                //LoggerManager.Debug(string.Format("WaitForAxisMotionDone({0}) : Axis motion done.", axis.AxisIndex.Value));
            //            }
            //            else
            //            {
            //                Stop(axis);
            //                retVal = -1;
            //                LoggerManager.Error($"WaitForAxisMotionDone({0}) : Axis motion error occurred. Err code = {1}", axis.AxisIndex.Value, axis.Status.ErrCode));
            //                //    throw new MotionException(string.Format("WaitForAxisMotionDone({0}) : Axis motion error occurred. Err code = {1}", axis.AxisIndex.Value, axis.Status.ErrCode));
            //            }
            //        }
            //        timeStamp.Add(new KeyValuePair<string, long>(string.Format("End GetAxisStatus {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //    } while (runFlag == true);


            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Error($"WaitForAxisMotionDone() Function error: {0}" + err.Message));
            //    retVal = -1;
            //    //  throw new MotionException(string.Format("WaitForAxisMotionDone({0}) : Axis motion error occurred. Err code = {1}", axis.AxisIndex.Value, axis.Status.ErrCode));
            //}
            //finally
            //{
            //    elapsedStopWatch.Stop();
            //    //double pos=0;
            //    // GetCmdPosition(axis, ref pos);
            //    // axis.Status.Position.Ref = pos;
            //}
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("Settling.... {0}axis", axis.Label), stw.ElapsedMilliseconds));

            //if (axis.SettlingTime > 0)
            //{
            //    delay.DelayFor((long)axis.SettlingTime);
            //}
            //timeStamp.Add(new KeyValuePair<string, long>(string.Format("End {0}axis", axis.Label), stw.ElapsedMilliseconds));
            //stw.Stop();
            ////int step = 0;
            ////foreach (var item in timeStamp)
            ////{
            ////    step++;
            ////    LoggerManager.Debug($string.Format("Time Stamp [{0}] - Desc: {1}, Time: {2}", step, item.Key, item.Value));
            ////}
            //return retVal;
            #endregion

            int retVal = -1;
            int hSymbol = -1;
            bool runFlag = true;
            Stopwatch elapsedStopWatch = new Stopwatch();

            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            bool[] doneFlag = new bool[(int)EnumThreePodAxis.THREEPOD_AXIS_LAST];
            try
            {
                mreWaitForEvent.WaitOne(1);

                do
                {
                    mreWaitForEvent.WaitOne(1);
                    PlcStatus = (CDXMachineStatus)PLCModule.tcClient.ReadAny(hSymbol, typeof(CDXMachineStatus));
                    // doneFlag = PlcStatus.bAxisMotionDone;
                    if (doneFlag[axis.AxisIndex.Value] == true)
                    {
                        runFlag = false;
                        retVal = 0;
                    }
                    else
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
                    }


                } while (runFlag == true);

                if (axis.SettlingTime > 0)
                {
                    //delay.DelayFor(axis.SettlingTime / 2);
                    Thread.Sleep((int)axis.SettlingTime / 2);
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"WaitForAxisMotionDone() Function error: {0}" + err.Message));
                //LoggerManager.Error($"WaitForAxisMotionDone({0}) : Axis motion error occurred. Err code = {1}", axis.AxisIndex.Value, axis.Status.ErrCode));
                LoggerManager.Exception(err);

                return -1;
            }
            finally
            {
                elapsedStopWatch?.Stop();
            }
            return retVal;

        }

        public int WaitForHomeSensor(AxisObject axis, long timeout = 0)
        {
            return -1;
        }

        private void UpdateTwincatBaseProc()
        {
            int retVal = -1;
            bool enable;
            bool inpos;
            bool ismoving = false;
            int hSymbol = 0;
            try
            {
                while (bStopUpdateThread == false)
                {
                    if (PLCModule.tcClient.IsConnected == true)
                    {
                        SymbolService.MachineStatusSymbolDict.TryGetValue(SymbolMap.MachineStatusSymbols.MachineStatus, out hSymbol);
                        PlcStatus = (CDXMachineStatus)PLCModule.tcClient.ReadAny(hSymbol, typeof(CDXMachineStatus));
                        
                        for (int axisindex = 0; axisindex < (int)EnumThreePodAxis.THREEPOD_AXIS_LAST; axisindex++)
                        {
                            AxisStatusList[axisindex].Pulse.Actual = PlcStatus.dblActPos[axisindex];
                            AxisStatusList[axisindex].Pulse.Command = PlcStatus.dblTargetPos[axisindex];
                            AxisStatusList[axisindex].Pulse.Command = PlcStatus.dblActPos[axisindex];
                            if (PlcStatus.bAxisEnable[axisindex] == 1)
                            {
                                enable = true;
                            }
                            else
                            {
                                enable = false;
                            }
                            AxisStatusList[axisindex].AxisEnabled = enable;
                            retVal = IsMoving(axisindex, ref ismoving);
                            ResultValidate(retVal);
                            AxisStatusList[axisindex].AxisBusy = ismoving;

                            if (PlcStatus.Inposition[axisindex] == 1)
                            {
                                inpos = true;
                            }
                            else
                            {
                                inpos = false;
                            }
                            AxisStatusList[axisindex].Inposition = inpos;
                            GetAxisStatus(axisindex, AxisStatusList[axisindex]);
                        }
                    }
                    mreUpdateEvent.WaitOne(1);
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($"UpdateIOProc(): Error occurred while update io proc. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }

        }

        public int ForcedZDown(AxisObject axis)
        {
            return 0;
        }

        public int SetMotorStopCommand(AxisObject axis, string setevent,EnumMotorDedicatedIn input)
        {
            throw new NotImplementedException();
        }

        public int IsThreeLegUp(AxisObject axis, ref bool isthreelegup)
        {
            return (int)EnumMotionBaseReturnCode.ReturnCodeOK;
        }

        public int IsThreeLegDown(AxisObject axis, ref bool isthreelegdn)
        {
            return (int)EnumMotionBaseReturnCode.ReturnCodeOK;
        }
        public int IsFls(AxisObject axis, ref bool isfls)
        {
            return (int)EnumMotionBaseReturnCode.ReturnCodeOK;
        }

        public int IsRls(AxisObject axis, ref bool isrls)
        {
            return (int)EnumMotionBaseReturnCode.ReturnCodeOK;
        }

        public int ChuckTiltMove(AxisObject axis, double offsetz0, double offsetz1, double offsetz2, double abspos, double vel, double acc)
        {
            return -1;
        }

        public int GetAuxPulse(AxisObject axis, ref int pos)
        {
            pos = 0;
            return 1;
        }
    }

}

using LogModule;
using MetroDialogInterfaces;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.DialogControl;
using ProberInterfaces.State;
using ProberInterfaces.WaferTransfer;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Temperature;
using ProberInterfaces.Temperature.Chiller;
using ProberInterfaces.Temperature.DewPoint;
using System.Threading;
using ProberInterfaces.Event;
using ProberInterfaces.Temperature;
using ProberInterfaces.Enum;

namespace TempControl
{
    public enum TempCheckType
    {
        NOMAL,
        OVERHEATING,
    }

    public class MonitoringInfomation
    {
        public bool IsErrorState = false;
        public double PreTemp = -999;
        /// <summary>
        /// 첫 PreTemp 를 넣기위한 Falg. 실제 Monitoring 이 시작되면 pretemp 에 그때의 값을 넣고 true 로 만든다.
        /// </summary>
        public bool StartMonitorFlag = false;

        public bool InMonitoringState = false;

        public void Set(double preTemp, bool startMonitoringFlag)
        {
            try
            {
                PreTemp = preTemp;
                StartMonitorFlag = startMonitoringFlag;
                LoggerManager.Debug($"[TempController] Set MonitoringInfomation. PreTemp:{PreTemp}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void Clear()
        {
            try
            {
                IsErrorState = false;
                PreTemp = -999;
                StartMonitorFlag = false;

                LoggerManager.Debug("[TempController] Clear MonitoringInfomation.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetMonitoringState(bool state)
        {
            try
            {
                if(InMonitoringState != state)
                {
                    InMonitoringState = state;
                    LoggerManager.Debug($"[TempController] InMonitoringState set to {InMonitoringState}");
                    if(InMonitoringState == false)
                    {
                        Clear();
                    }
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public abstract class TempControllerState : IInnerState
    {
        public abstract ModuleStateEnum GetModuleState();
        public abstract EnumTemperatureState GetState();
        public abstract EventCodeEnum Execute();
        public abstract bool CanExecute(IProbeCommandToken token);

        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.UNDEFINED;
        }

        //public virtual EventCodeEnum ClearState()
        //{
        //    return EventCodeEnum.NONE;
        //}

        public abstract EventCodeEnum ClearState();

        public abstract EventCodeEnum End();

        public abstract EventCodeEnum Pause();

        public abstract EventCodeEnum Resume();

    }

    public abstract class TCStateBase : TempControllerState
    {
        private Type ReserveCommandType = null;
        protected TempController Module { get; }
        public TCStateBase(TempController module) => Module = module;

        private MonitoringInfomation _MonitoringInfo = new MonitoringInfomation();

        public MonitoringInfomation MonitoringInfo
        {
            get { return _MonitoringInfo; }
            set { _MonitoringInfo = value; }
        }


        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.InitState(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        protected virtual bool ReservedCommandSet()
        {
            bool result = false;
            try
            {

                IProbingModule ProbingModule = Module.ProbingModule();

                if (ProbingModule.ProbingStateEnum == EnumProbingState.ZDN && ReserveCommandType == typeof(ILotOpPause))
                {
                    result = CommandSetCommonFunc(ReserveCommandType);
                    ReserveCommandType = null;
                    Module.InnerStateTransition(new TCErrorPerformState(Module));
                }
                else if (ReserveCommandType == typeof(IGpibAbort))
                {
                    result = CommandSetCommonFunc(ReserveCommandType);
                    ReserveCommandType = null;
                    ForceZDown();
                    Module.InnerStateTransition(new TCErrorPerformState(Module));
                }
                else
                {
                    result = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }
        protected virtual bool StopProbingThatDependOnParameter()
        {
            bool functionResult = false;
            try
            {

                IProbingModule ProbingModule = Module.ProbingModule();
                if (Module.TempControllerDevParameter.TempPauseType.Value
                    == TempPauseTypeEnum.ZDOWN_ABORT)
                {
                    //구현 1-1 로직.
                    functionResult = GPIBAbort();
                }
                else if (Module.TempControllerDevParameter.TempPauseType.Value
                    == TempPauseTypeEnum.EMERGENCY_ABORT)
                {
                    //구현 1-2 로직.
                    functionResult = LotPause();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return functionResult;
        }
        private bool GPIBAbort()
        {
            bool result = true;
            try
            {

                if (ReserveCommandType == null)
                    ReserveCommandType = typeof(IGpibAbort);

            }
            catch (Exception err)
            {
                result = false;
                LoggerManager.Exception(err);
            }
            return result;
        }
        private bool LotPause()
        {
            bool result = true;
            try
            {

                if (ReserveCommandType == null)
                    ReserveCommandType = typeof(ILotOpPause);

            }
            catch (Exception err)
            {
                result = false;
                LoggerManager.Exception(err);
            }
            return result;
        }
        protected bool IsEmergencyTempWithinRange(double targetTemperature, TempCheckType checkType = TempCheckType.NOMAL)
        {
            bool reachTargetTemp = false;
            try
            {
                double setTemp = 0;
                double minTemp = 0;
                double maxTemp = 0;
                double deviation = 0;

                setTemp = targetTemperature;
                if (checkType == TempCheckType.NOMAL)
                {
                    var tempcontroller = Module.TempControllerDevParameter;

                    if (tempcontroller.EmergencyAbortTempTolerance.Value < tempcontroller.EmergencyAbortTempTolerance.LowerLimit
                        || tempcontroller.EmergencyAbortTempTolerance.Value > tempcontroller.EmergencyAbortTempTolerance.UpperLimit)
                    {
                        tempcontroller.EmergencyAbortTempTolerance.Value = 10;//default value
                        deviation = tempcontroller.EmergencyAbortTempTolerance.Value;
                    }
                    else
                    {
                        //emergency temp 허용 편차를 검사하여 설정
                        if (tempcontroller.TempToleranceDeviation.Value > tempcontroller.EmergencyAbortTempTolerance.Value)
                        {
                            double newToleranceValue = tempcontroller.TempToleranceDeviation.Value * 2;

                            // 새 허용값이 범위를 벗어나는지 확인하고 조정
                            if (newToleranceValue >= tempcontroller.EmergencyAbortTempTolerance.UpperLimit)
                            {
                                tempcontroller.EmergencyAbortTempTolerance.Value = tempcontroller.EmergencyAbortTempTolerance.UpperLimit;
                            }
                            else if (newToleranceValue <= tempcontroller.EmergencyAbortTempTolerance.LowerLimit)
                            {
                                tempcontroller.EmergencyAbortTempTolerance.Value = tempcontroller.EmergencyAbortTempTolerance.LowerLimit;
                            }
                            else
                            {
                                tempcontroller.EmergencyAbortTempTolerance.Value = newToleranceValue;
                            }
                        }
                        else
                        {
                            // 허용 편차가 올바른 범위 내에 있음을 나타냄
                        }
                    }

                    deviation = tempcontroller.EmergencyAbortTempTolerance.Value;
                }
                else
                {
                    deviation = Module.TempControllerDevParameter.DeviationForOverHeating.Value;
                }

                minTemp = setTemp - deviation;
                maxTemp = setTemp + deviation;

                MinMaxCheckAndSwap(ref minTemp, ref maxTemp);

                if (minTemp <= Module.TempInfo.CurTemp.Value && Module.TempInfo.CurTemp.Value <= maxTemp)
                {
                    reachTargetTemp = true;
                }
                else
                {
                    reachTargetTemp = false;

                    if (checkType == TempCheckType.OVERHEATING)
                    {
                        if (0 < setTemp)
                        {
                            if (maxTemp < Module.TempInfo.CurTemp.Value)
                            {
                                reachTargetTemp = true;
                            }
                        }
                        else
                        {
                            if (Module.TempInfo.CurTemp.Value < minTemp)
                            {
                                reachTargetTemp = true;
                            }
                        }
                    }
                }

                if (!reachTargetTemp)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] IsEmergencyTempWithinRange(), CurTemp : {Module.TempInfo.CurTemp.Value} SetTemp : {targetTemperature} Deviation : {deviation} Min Temp : {minTemp} Max Temp : {maxTemp}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return reachTargetTemp;
        }
        protected bool IsTempWithinRange(double targetTemperature, TempCheckType checkType = TempCheckType.NOMAL)
        {
            bool reachTargetTemp = false;
            try
            {
                double setTemp = 0;
                double minTemp = 0;
                double maxTemp = 0;
                double deviation = 0;

                setTemp = targetTemperature;
                if (checkType == TempCheckType.NOMAL)
                {
                    deviation = Module.TempControllerDevParameter.TempToleranceDeviation.Value;
                }
                else
                {
                    deviation = Module.TempControllerDevParameter.DeviationForOverHeating.Value;
                }

                minTemp = setTemp - deviation;
                maxTemp = setTemp + deviation;

                MinMaxCheckAndSwap(ref minTemp, ref maxTemp);

                if (minTemp <= Module.TempInfo.CurTemp.Value && Module.TempInfo.CurTemp.Value <= maxTemp)
                {
                    reachTargetTemp = true;
                }
                else
                {
                    reachTargetTemp = false;

                    if (checkType == TempCheckType.OVERHEATING)
                    {
                        if (0 < setTemp)
                        {
                            if (maxTemp < Module.TempInfo.CurTemp.Value)
                            {
                                reachTargetTemp = true;
                            }
                        }
                        else
                        {
                            if (Module.TempInfo.CurTemp.Value < minTemp)
                            {
                                reachTargetTemp = true;
                            }
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return reachTargetTemp;
        }

        protected bool IsTempWithinProbingRange(double targetTemperature)
        {
            bool reachTargetTemp = false;
            try
            {
                double setTemp = 0;
                double minTemp = 0;
                double maxTemp = 0;
                double deviation = 0;

                setTemp = targetTemperature;

                deviation = Module.TempControllerDevParameter.TempGEMToleranceDeviation.Value;

                minTemp = setTemp - deviation;
                maxTemp = setTemp + deviation;

                MinMaxCheckAndSwap(ref minTemp, ref maxTemp);

                if (minTemp <= Module.TempInfo.CurTemp.Value && Module.TempInfo.CurTemp.Value <= maxTemp)
                {
                    reachTargetTemp = true;
                }
                else
                {
                    reachTargetTemp = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return reachTargetTemp;
        }

        protected void MinMaxCheckAndSwap(ref double minTemp, ref double maxTemp)
        {
            try
            {
                if (maxTemp < minTemp)
                {
                    double tmp = maxTemp;
                    maxTemp = minTemp;
                    maxTemp = tmp;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// 
        /// (구현 1)의 경우 Chiller 내부적으로 TempManager의 SV를 보고 조절을 하기 때문에 SetTemp에 값만 넣어준다.
        /// setTemp와 overheatingTemp는 일반 온도 값이 들어온다.
        /// 
        /// </summary>
        /// <param name="SetTemp"></param>
        /// <returns>EventCodeEnum</returns>
        protected EventCodeEnum SetTemperature(double setTemp, double overheatingTemp = 0, bool willYouSaveSetValue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock(Module.TempChangeLockObj)
                {
                    // 구현 1.
                    Module.SetSV(TemperatureChangeSource.UNDEFINED, setTemp, willYouSaveSetValue);
                    Module.OverHeatTemp = overheatingTemp;
                    Module.TempManager.SetTempWithOption(setTemp + overheatingTemp);

                    LoggerManager.Debug($"Call SetTemperature () Temp : {setTemp}");

                    Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.Temp);
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected bool CommandSetCommonFunc(Type interfaceType, [CallerMemberName] string callerFuncName = "")
        {
            ICommandManager CommandManager = Module.CommandManager();
            bool commandSetResult = false;
            try
            {
                commandSetResult = CommandManager.SetCommand(this, CommandNameGen.Generate(interfaceType));
                LoggerManager.Debug($"[TempControllerState({this.GetType().Name}) - {callerFuncName}()] " +
                    $"Command Set {interfaceType.Name} => CommandSetResult = {commandSetResult}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return commandSetResult;
        }

        protected bool CheckCanExecuteUsingInterfaceType(IProbeCommandToken token, params Type[] args)
        {
            bool isValidCommand = false;
            try
            {

                if (args != null && 0 < args.Length)
                {
                    foreach (Type type in args)
                    {
                        if (type.IsInstanceOfType(token))
                        {
                            isValidCommand = true;
                        }
                    }
                }
                else
                {
                    isValidCommand = false;
                }

                if (isValidCommand == false)
                {
                    LoggerManager.Debug($"[TempControllerState({this.GetType().Name}) - CanExecute()] " +
                        $"{token.GetType().Name} Command Can't Execute");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        protected virtual EventCodeEnum BehaviorFollowingProbingModuleState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IProbingModule ProbingModule = Module.ProbingModule();

                if (ProbingModule.ModuleState.State == ModuleStateEnum.RUNNING
                    || ProbingModule.ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    retVal = Module.InnerStateTransition(new TCMonitoringState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected bool ForceZDown()
        {
            bool result = false;
            try
            {
                result = CommandSetCommonFunc(typeof(IZDownRequest));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }

        protected EventCodeEnum PreSetTempCompareExcutor()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (Module.TempInfo.SetTemp.Value != Module.TempInfo.TargetTemp.Value)
                {
                    retVal = Module.InnerStateTransition(new TCNomalTriggerState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        bool waitmonitor = true;
        public EventCodeEnum MonitoringTemperature()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                bool IsTempWithinRange = false;
                bool reservedCommandSetResult = false;
                bool IsTempWithinProbingRange = true;
               
                if (Module.CanCheckDeviationState())
                {
                    bool inProbingState = Module.ProbingModule().ProbingStateEnum == EnumProbingState.ZUP
                    || Module.ProbingModule().ProbingStateEnum == EnumProbingState.ZUPDWELL;
                    double setTemp = Module.TempInfo.SetTemp.Value;

                    if (inProbingState)
                    {
                        if (MonitoringInfo.InMonitoringState == false)
                        {
                            // MonitoringState : contact 중 emergency error 발생 여부, 온도 변화(TempMonitorChangeValueEvent)이벤트 발생 로직에 사용되는 데이터 설정
                            MonitoringInfo.SetMonitoringState(true);
                        }

                        // monitoring 기능은 probing 상태에서만 지원됨.
                        if (MonitoringInfo.InMonitoringState && waitmonitor && Module.TempControllerDevParameter.WaitMonitorTimeSec.Value != 0)
                        {
                            var curTime = DateTime.Now;
                            var offsetTime = curTime - Module.ProbingModule().GetZupStartTime();
                            if (offsetTime.TotalSeconds > Module.TempControllerDevParameter.WaitMonitorTimeSec.Value)
                            {
                                waitmonitor = false;
                                LoggerManager.Debug("TempController Wait Monitor Time End");
                            }
                        }
                        else if (Module.TempControllerDevParameter.WaitMonitorTimeSec.Value == 0)
                        {
                            if (waitmonitor)
                            {
                                waitmonitor = false;
                            }
                        }

                        if (!waitmonitor)
                        {
                            IsTempWithinProbingRange = this.IsTempWithinProbingRange(setTemp);

                            #region <summary> Temp In,Out event, alarm 발생 로직 </summary>
                            if (IsTempWithinProbingRange == false && MonitoringInfo.IsErrorState == false && Module.IsOccurOutOfRange == false)
                            {
                                double deviation = Module.TempControllerDevParameter.TempGEMToleranceDeviation.Value;
                                WriteOutOfRangeLog(setTemp, deviation);

                                PIVInfo pivinfo = new PIVInfo(curtemperature: Module.TempInfo.CurTemp.Value, foupnumber: Module.GetParam_Wafer().GetOriginFoupNumber());
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(TempMoveOutofDeviationRangeDuringProbingEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();
                                Module.IsOccurOutOfRangeProbingDeviation = true;

                                if (Module.TempInfo.SetTemp.Value > 0)
                                {
                                    //Temp Alarm (Out of Range)
                                    Module.NotifyManager().Notify(EventCodeEnum.TEMPERATURE_OUT_RANGE_HOT_PROBING);
                                }
                                else
                                {
                                    //Temp Cold Alarm (Out of Range)
                                    Module.NotifyManager().Notify(EventCodeEnum.TEMPERATURE_OUT_RANGE_COLD_PROBING);
                                }

                                Module.IsOccurOutOfRange = true;
                            }
                            else if (Module.IsOccurOutOfRange && Module.IsOccurOutOfRangeProbingDeviation && IsTempWithinProbingRange)
                            {
                                LoggerManager.Debug($"[GEM TempMoveIntoRangeEvent occur] SetTemp:{setTemp}, CurTemp:{Module.TempInfo.CurTemp.Value},");

                                PIVInfo pivinfo = new PIVInfo(curtemperature: Module.TempInfo.CurTemp.Value, foupnumber: Module.GetParam_Wafer().GetOriginFoupNumber());
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(TempMoveInofDeviationRangeDuringProbingEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();
                                if (Module.IsOccurOutOfRangeProbingDeviation && IsTempWithinProbingRange == false)
                                {
                                    Module.IsOccurOutOfRangeProbingDeviation = false;
                                }

                                if (Module.TempInfo.SetTemp.Value > 0)
                                {
                                    //Temp Alarm (Out of Range)
                                    Module.NotifyManager().Notify(EventCodeEnum.TEMPERATURE_IN_RANGE_HOT_PROBING);
                                }
                                else
                                {
                                    //Temp Cold Alarm (Out of Range)
                                    Module.NotifyManager().Notify(EventCodeEnum.TEMPERATURE_IN_RANGE_COLD_PROBING);
                                }
                                Module.IsOccurOutOfRange = false;
                            }
                            #endregion

                            #region <summary> Monitoring 이벤트 발생 로직 </summary>
                            if (Module.TempControllerDevParameter.TemperatureMonitorEnable.Value)
                            {
                                if (!MonitoringInfo.StartMonitorFlag)
                                {
                                    MonitoringInfo.Set(Module.TempInfo.CurTemp.Value, true);
                                }
                                if (Math.Abs(MonitoringInfo.PreTemp - Module.TempInfo.CurTemp.Value) >= Module.TempControllerDevParameter.TempMonitorRange.Value)
                                {
                                    LoggerManager.Debug($"[GEM TempMonitorChangeValueEvent occur] CurTemp:{Module.TempInfo.CurTemp.Value}, PreTemp:{MonitoringInfo.PreTemp}");

                                    PIVInfo pivinfo = new PIVInfo(curtemperature: Module.TempInfo.CurTemp.Value, foupnumber: Module.GetParam_Wafer().GetOriginFoupNumber());
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    Module.EventManager().RaisingEvent(typeof(TempMonitorChangeValueEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();

                                    //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(TempMonitorChangeValueEvent).FullName));
                                    MonitoringInfo.PreTemp = Module.TempInfo.CurTemp.Value;
                                }
                            }
                            #endregion
                        }

                        #region <summary> Abort 동작 로직 </summary>
                        if (this.IsEmergencyTempWithinRange(setTemp) == false)//GPIB Abort 동작에서 보는 Range는 Emergency Temp를 확인한다.
                        {
                            //구현 1 로직.
                            bool functionResult = StopProbingThatDependOnParameter();
                            MonitoringInfo.IsErrorState = true; //StopProbingThatDependOnParameter 밖으로 꺼냄
                        }
                        //바로 command를 날리지 않은 경우가 있으므로 밑 로직을 밖으로 뺐다.
                        reservedCommandSetResult = ReservedCommandSet();
                        #endregion
                    }
                    else
                    {
                        if (MonitoringInfo.InMonitoringState)
                        {
                            // contact 중 emergency error 발생 여부, 온도 변화(TempMonitorChangeValueEvent)이벤트 발생 로직에 사용되는 데이터 해제
                            MonitoringInfo.SetMonitoringState(false);

                            // Monitoring 상태가 아니면 초기화
                            if (waitmonitor == false)
                            {
                                waitmonitor = true;
                            }
                        }

                        IsTempWithinRange = this.IsTempWithinRange(setTemp);
                        #region <summary> Temp In/Out event, alarm 발생 로직 </summary>
                        if (IsTempWithinRange == false && Module.IsOccurOutOfRange == false)
                        {
                            
                            double deviation = Module.TempControllerDevParameter.TempToleranceDeviation.Value;
                            WriteOutOfRangeLog(setTemp, deviation);

                            PIVInfo pivinfo = new PIVInfo(curtemperature: Module.TempInfo.CurTemp.Value, foupnumber: Module.GetParam_Wafer().GetOriginFoupNumber());
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            Module.EventManager().RaisingEvent(typeof(TempMoveOutofRangeEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                            Module.IsOccurOutOfRangeDeviation = true;

                            if (Module.TempInfo.SetTemp.Value > 0)
                            {
                                //Temp Alarm (Out of Range)
                                Module.NotifyManager().Notify(EventCodeEnum.TEMPERATURE_OUT_RANGE_HOT);
                            }
                            else
                            {
                                //Temp Cold Alarm (Out of Range)
                                Module.NotifyManager().Notify(EventCodeEnum.TEMPERATURE_OUT_RANGE_COLD);
                            }

                            Module.IsOccurOutOfRange = true;
                        }
                        else if (Module.IsOccurOutOfRange && Module.IsOccurOutOfRangeDeviation && IsTempWithinRange)
                        {
                            LoggerManager.Debug($"[GEM TempMoveIntoRangeEvent occur] SetTemp:{setTemp}, CurTemp:{Module.TempInfo.CurTemp.Value},");

                            {
                                PIVInfo pivinfo = new PIVInfo(curtemperature: Module.TempInfo.CurTemp.Value, foupnumber: Module.GetParam_Wafer().GetOriginFoupNumber());
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(TempMoveIntoRangeEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();

                                if (Module.IsOccurOutOfRangeDeviation && IsTempWithinRange == true)
                                {
                                    Module.IsOccurOutOfRangeDeviation = false;  
                                }

                                if (Module.TempInfo.SetTemp.Value > 0)
                                {
                                    //Temp Alarm (Out of Range)
                                    Module.NotifyManager().Notify(EventCodeEnum.TEMPERATURE_IN_RANGE_HOT);
                                }
                                else
                                {
                                    //Temp Cold Alarm (Out of Range)
                                    Module.NotifyManager().Notify(EventCodeEnum.TEMPERATURE_IN_RANGE_COLD);
                                }
                            }
                            Module.IsOccurOutOfRange = false;
                        }
                        #endregion
                    }
                }
                else
                {
                    // Monitoring 이 끝나면 초기화 
                    if (waitmonitor == false)
                    {
                        waitmonitor = true;
                    }
                }
                void WriteOutOfRangeLog(double setTemp, double deviation)
                {
                    LoggerManager.Debug($"[GEM TempMoveOutofRangeEvent occur] SetTemp:{setTemp}, CurTemp:{Module.TempInfo.CurTemp.Value}, Deviation:{deviation}, DevDeviation:{Module.IsOccurOutOfRangeDeviation}, AlarmDeviaiton:{Module.IsOccurOutOfRangeProbingDeviation}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public abstract class TCColdStateBase : TCStateBase
    {
        private Type ReserveCommandType = null;
        protected IDewPointModule DewPointModule { get; }
        protected IChillerModule ChillerModule { get; }
        public TCColdStateBase(TempController module) : base(module)
        {
            ChillerModule = Module.EnvControlManager().GetChillerModule();
            DewPointModule = Module.EnvControlManager().GetDewPointModule();
        }


        bool prev_checkDP = false;
        protected bool CheckDewPointMonitoring()
        {
            bool retVal = false;
            try
            {
                double curDewPoint = DewPointModule.CurDewPoint;
                double chillerCurTemp = ChillerModule.ChillerInfo.ChillerInternalTemp;
                if (chillerCurTemp != -999)
                {
                    if (Module.TempInfo.CurTemp.Value < 0)
                    {
                        if (chillerCurTemp < ChillerModule.ChillerParam.AmbientTemp.Value)
                        {
                            if (curDewPoint - DewPointModule.Tolerence > chillerCurTemp)
                            {
                                LoggerManager.Debug($"[{this.GetType().Name}] DewPoint Error. DP : {curDewPoint}, Chiller Internal Temp : {chillerCurTemp}, Chiller AmbientTemp : {ChillerModule.ChillerParam.AmbientTemp.Value}, DewPointModule.Tolerence :{DewPointModule.Tolerence} ");
                                retVal = false;
                                return retVal;
                            }
                            else
                            {
                                retVal = true;
                                return retVal;
                            }
                                
                        }
                        else
                        {
                            retVal = true;
                            return retVal;
                        }


                    }
                    else
                    {
                        retVal = true;
                    }
                }
                else
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
               
                if(prev_checkDP != retVal)
                {
                    LoggerManager.Debug($"[{Module.ModuleState.GetType()}] CheckDewPointMonitoring(): result = {retVal}, CurTemp({Module.TempInfo.CurTemp.Value}) < 0, " +
                                $"chillerCurTemp({ChillerModule.ChillerInfo.ChillerInternalTemp}) < ChillerModule.ChillerParam.AmbientTemp.Value({ChillerModule.ChillerParam.AmbientTemp.Value})" +
                                $",curDewPoint({DewPointModule.CurDewPoint}) - DewPointModule.Tolerence{DewPointModule.Tolerence} > chillerCurTemp({ChillerModule.ChillerInfo.ChillerInternalTemp})");
                }
                
                prev_checkDP = retVal;
            }
            return retVal;
        }

        /// <summary>
        /// ChillerActiveableDewPointTolerance
        /// Valve를 여는 조건 : 현재 DewPoint 가 칠러 internal temp 보다 10도 이상 낮을 경우 .
        /// TargetDewPoint 와 DewPoint 의 차이가 10도 일경우에는 internal temp 와 targetdewpoint가 같은걸로 확인한다.
        /// </summary>
        /// <returns></returns>
        protected bool CheckActiveValve()
        {
            bool retVal = false;
            try
            {
                double targetDewPoint = DewPointModule.CurDewPoint - DewPointModule.Tolerence;
                double chillerCurTemp = ChillerModule.ChillerInfo.ChillerInternalTemp;

                if (ChillerModule.ChillerInfo.ActivdCoolantValve)
                {
                    if (targetDewPoint > ChillerModule.ChillerInfo.ChillerInternalTemp)
                        return false;
                    else
                        return true;
                }
                else
                {
                    //Chiller 를 가동시켰다가 멈춘상태라면
                    if (ChillerModule.ChillerInfo._PreActivdCoolantValve)
                    {
                        //현재 DewPoint 와 Target Temp까지 ChillerActiveableDewPointTolerance(default 10) 도  이상으로 남았다면,
                        if (Math.Abs(targetDewPoint - DewPointModule.CurDewPoint) >= ChillerModule.ChillerInfo.ChillerActiveableDewPointTolerance)
                        {
                            if (Math.Abs(DewPointModule.CurDewPoint - chillerCurTemp) >= ChillerModule.ChillerInfo.ChillerActiveableDewPointTolerance)
                            {
                                return true;
                            }

                        }
                        else
                        {
                            if (Math.Abs(DewPointModule.CurDewPoint - chillerCurTemp) <= 0)
                            {
                                return true;
                            }
                        }
                    }
                    else
                        return true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        protected bool CheckChillerRangeMonitoring()
        {
            bool retVal = false;
            try
            {
                double inRange = 0.0;
                inRange = ChillerModule.ChillerParam.InRangeWindowTemp.Value;

                double minTemp = ChillerModule.ChillerInfo.SetTemp + (inRange * -1);
                double maxTemp = ChillerModule.ChillerInfo.SetTemp + inRange;
                double CurTemp = Module.TempInfo.CurTemp.Value;
                if (CurTemp >= minTemp & CurTemp <= maxTemp)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        protected new EventCodeEnum PreSetTempCompareExcutor()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.TempInfo.TargetTemp.Value != Module.TempInfo.SetTemp.Value)
                {
                    LoggerManager.Debug($"[TC] Check need change temperature condition : Target Temp is {Module.TempInfo.TargetTemp.Value}, Set Temp is {Module.TempInfo.SetTemp.Value}");
                    retVal = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        protected EventCodeEnum CheckChillerAbortOp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ChillerModule.ChillerInfo.AbortChiller)
                {
                    retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);
                    retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                    retVal = Module.InnerStateTransition(new ChillerAbortStop(Module));
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        protected EventCodeEnum CheckChillerDifferentTemp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //TargetTemp( Activated State 에서 계산되는 데이터 )
                //SetTemp ( 칠러로 부터 얻어오는 데이터 )
                var chillerTargetTemp = ChillerModule.ChillerInfo.TargetTemp;
                var chillersetTemp = ChillerModule.ChillerInfo.SetTemp;

                //if ((chillerTargetTemp != chillersetTemp) & !ChillerModule.ChillerInfo.AbortChiller)

                if(Module.TempInfo.SetTemp.Value == Module.TempInfo.TargetTemp.Value)
                {
                    if (!ChillerModule.IsMatchedTargetTemp(Module.TempInfo.SetTemp.Value) & !ChillerModule.ChillerInfo.AbortChiller)
                    {
                        LoggerManager.Debug($"[Check Chiller Temp] TargetTemp : {chillerTargetTemp}, SetTemp : {chillersetTemp}");
                        LoggerManager.Debug($"[Check Chiller Temp] SV : {Module.TempInfo.SetTemp.Value}, " +
                            $"Offset : {ChillerModule.GetChillerTempoffset(Module.TempInfo.SetTemp.Value)}");

                        var coolantInTemp = Module.EnvControlManager().GetChillerModule().ChillerParam.CoolantInTemp.Value;
                        if (!(Module.TempInfo.SetTemp.Value <= coolantInTemp))
                        {
                            return retVal;
                        }

                        if (!((Module.InnerState as TempControllerState).GetState() == EnumTemperatureState.PauseDiffTemp))
                        {
                            retVal = Module.InnerStateTransition(new PauseDifferenceTemp(Module));
                            return retVal;
                        }

                        retVal = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        protected override EventCodeEnum BehaviorFollowingProbingModuleState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IProbingModule ProbingModule = Module.ProbingModule();

                if (ProbingModule.ProbingStateEnum == EnumProbingState.ZUP
                    || ProbingModule.ProbingStateEnum == EnumProbingState.ZUPDWELL)
                {
                    retVal = Module.InnerStateTransition(new TC_ColdMonitoringState(Module));//TC_ColdMonitoringState
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        protected override bool StopProbingThatDependOnParameter()
        {
            bool functionResult = false;
            try
            {

                IProbingModule ProbingModule = Module.ProbingModule();
                if (Module.TempControllerDevParameter.TempPauseType.Value
                    == TempPauseTypeEnum.ZDOWN_ABORT)
                {
                    //구현 1-1 로직.
                    functionResult = GPIBAbort();
                }
                else if (Module.TempControllerDevParameter.TempPauseType.Value
                    == TempPauseTypeEnum.EMERGENCY_ABORT)
                {
                    //구현 1-2 로직.
                    functionResult = LotPause();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return functionResult;
        }
        private bool GPIBAbort()
        {
            bool result = true;
            try
            {

                if (ReserveCommandType == null)
                    ReserveCommandType = typeof(IGpibAbort);

            }
            catch (Exception err)
            {
                result = false;
                LoggerManager.Exception(err);
            }
            return result;
        }
        private bool LotPause()
        {
            bool result = true;
            try
            {

                if (ReserveCommandType == null)
                    ReserveCommandType = typeof(ILotOpPause);

            }
            catch (Exception err)
            {
                result = false;
                LoggerManager.Exception(err);
            }
            return result;
        }
        protected override bool ReservedCommandSet()
        {
            bool result = false;
            try
            {

                IProbingModule ProbingModule = Module.ProbingModule();

                //ZDown을 기다리고 Pause를 할 것인가? ZUp 상황에서는 Pause가 안될 수 있음.
                //알람 목적이기 때문에 Probing Zup 상태에서도 바로 LotPause 가 되어야 함.
                if (ReserveCommandType == typeof(ILotOpPause))
                {
                    result = CommandSetCommonFunc(ReserveCommandType);
                    ReserveCommandType = null;
                    Module.InnerStateTransition(new TC_ColdErrorPerformState(Module));
                }
                else if (ReserveCommandType == typeof(IGpibAbort))
                {
                    result = CommandSetCommonFunc(ReserveCommandType);
                    ReserveCommandType = null;
                    ForceZDown();
                    Module.InnerStateTransition(new TC_ColdErrorPerformState(Module));

                }
                else
                {
                    result = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }
        public virtual ChillerProcessType GetChillerState() { return ChillerProcessType.IDLE; }
    }


    public class TCIdleState : TCStateBase
    {
        public TCIdleState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.IDLE;
        public override EnumTemperatureState GetState() => EnumTemperatureState.IDLE;

        /// <summary>
        /// Execute 구현 내용.
        ///            
        /// 구현 1.       IChangeTempTemperatureToSetTemp, IChangeTemperatureToSetTempWhenWaferTransfer가 들어오면
        ///              State 변경.-------------ok.
        /// 구현 1-1-1.   IsEnableOverHeating이 true일때, IChangeTemperatureToSetTempWhenWaferTransfer Command가 들어오면,
        ///              TCOverheatingTriggerState로 상태 변경.------------ok.
        /// 구현 1-1-2.   IsEnableOverHeating이 false일때, IChangeTemperatureToSetTempWhenWaferTransfer Command가 들어오면,
        ///              TCNomalTriggerState로 상태 변경.------------ok.
        /// 구현 1-2.     IChangeTemperatureToSetTempWhenWaferTransfer Command가 들어오면
        ///              TCNomalTriggerState로 상태 변경.-----------------ok.
        /// 구현 2.       Probing중이면 TCMonitoringState로 변경.(suspend, running 일때가 프로빙 상태임.)-----ok.
        /// 
        /// </summary>
        /// <returns>EventCodeEnum</returns>
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //구현 1 로직.
                retVal = ProbeCmdExecutor();

                //구현 2 로직.
                retVal = BehaviorFollowingProbingModuleState();

                //구현 3 로직.
                retVal = PreSetTempCompareExcutor();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private EventCodeEnum ProbeCmdExecutor()
        {
            try
            {
                EventCodeEnum executeResult = EventCodeEnum.NONE;
                bool consumed;

                if (Module.CommandRecvSlot.IsRequested<IChangeTemperatureToSetTempAfterConnectTempController>())
                {
                    ChangeTempToSetTempDoAction();
                }

                    Func<bool> conditionFunc = () => true;
                #region => IChangeTemperatureToSetTemp Command
                Action changeTempToSetTempDoAction = ChangeTempToSetTempDoAction;
                Action changeTempToSetTempAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - IChangeTempToSetTempCommandExecutor()] => {executeResult.ToString()}");
                    };
                #endregion

                consumed = Module.CommandManager()
                    .ProcessIfRequested<IChangeTemperatureToSetTemp>(
                        Module, conditionFunc, changeTempToSetTempDoAction, changeTempToSetTempAbortAction);

                #region => IChangeTempToSetTempFullReach Command
                Action IChangeTempToSetTempFullReachDoAction = ChangeTempToSetTempFullReachDoAction;
                Action IChangeTempToSetTempFullReachAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - IChangeTempToSetTempFullReach()] => {executeResult.ToString()}");
                    };
                #endregion

                consumed = Module.CommandManager()
                    .ProcessIfRequested<IChangeTempToSetTempFullReach>(
                        Module, conditionFunc, IChangeTempToSetTempFullReachDoAction, IChangeTempToSetTempFullReachAbortAction);

                #region => IChangeTemperatureToSetTempWhenWaferTransfer Command
                Action startWaferTransferDoAction = StartWaferTransferDoAction;
                Action startWaferTransferAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - IChangeTemperatureToSetTempWhenWaferTransfer()] => {executeResult.ToString()}");
                    };
                #endregion

                consumed = Module.CommandManager()
                    .ProcessIfRequested<IChangeTemperatureToSetTempWhenWaferTransfer>(
                        Module, conditionFunc, startWaferTransferDoAction, startWaferTransferAbortAction);

                #region => ISetTempForFrontDoorOpen Command
                Action setTempForFrontDoorOpenDoAction = SetTempForFrontDoorOpen;
                Action setTempForFrontDoorOpenAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ISetTempForFrontDoorOpen()] => {executeResult.ToString()}");
                    };
                #endregion

                consumed = Module.CommandManager()
                    .ProcessIfRequested<ISetTempForFrontDoorOpen>(
                        Module, conditionFunc, setTempForFrontDoorOpenDoAction, setTempForFrontDoorOpenAbortAction);

                #region => ITemperatureSettingTriggerOccurrence Command
                Action temperatureSettingTriggerOccurrenceDoAction = TemperatureSettingTriggerOccurrenceDoAction;
                Action temperatureSettingTriggerOccurrenceAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ITemperatureSettingTriggerOccurrence()] => {executeResult.ToString()}");
                    };

                void TemperatureSettingTriggerOccurrenceDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] " +
                        $"StateTransition to {nameof(TC_ColdNomalTriggerState)} => {executeResult.ToString()}");
                }
                #endregion

                consumed = Module.CommandManager()
                   .ProcessIfRequested<ITemperatureSettingTriggerOccurrence>(
                       Module, conditionFunc, temperatureSettingTriggerOccurrenceDoAction, temperatureSettingTriggerOccurrenceAbortAction);

                return executeResult;

                void ChangeTempToSetTempDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TCNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] " +
                        $"StateTransition to {nameof(TCNomalTriggerState)} => {executeResult.ToString()}");
                }

                void ChangeTempToSetTempFullReachDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TCWaitingFullReachSetTempTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempFullReachDoAction()] " +
                        $"StateTransition to {nameof(TCWaitingFullReachSetTempTriggerState)} => {executeResult.ToString()}");
                }

                void StartWaferTransferDoAction()
                {
                    if (Module.TempControllerDevParameter.IsEnableOverHeating.Value == true)
                    {
                        executeResult = Module.InnerStateTransition(new TCOverHeatingTriggerState(Module));
                    }
                    else
                    {
                        executeResult = Module.InnerStateTransition(new TCNomalTriggerState(Module));
                    }
                    LoggerManager.Debug($"[{this.GetType().Name} - StartWaferTransferDoAction()] " +
                        $"StateTransition to {Module.InnerState.GetType().Name} => {executeResult.ToString()}");
                }

                void SetTempForFrontDoorOpen()
                {
                    executeResult = Module.InnerStateTransition(new TCSetTempForFrontDoorOpenReachedTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - SetTempForFrontDoorOpen()] " +
                        $"StateTransition to {nameof(TCSetTempForFrontDoorOpenReachedTriggerState)} => {executeResult.ToString()}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private new EventCodeEnum PreSetTempCompareExcutor()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (Module.TempInfo.SetTemp.Value != Module.TempInfo.TargetTemp.Value)
                {
                    retVal = Module.InnerStateTransition(new TCNomalTriggerState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token,
                                typeof(IChangeTemperatureToSetTemp),
                                typeof(IChangeTempToSetTempFullReach),
                                typeof(IChangeTemperatureToSetTempWhenWaferTransfer),
                                typeof(ISetTempForFrontDoorOpen),
                                typeof(IChangeTemperatureToSetTempAfterConnectTempController),
                                typeof(ITemperatureSettingTriggerOccurrence));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCMonitoringState : TCStateBase
    {
        //private Type ReserveCommandType = null;

        /// <summary>
        /// MonitoringState 가 되었을때의 시간. Monitor Time 을 기다리기 위해 기록.
        /// </summary>
        private DateTime InitDataTime;
        public TCMonitoringState(TempController module) : base(module)
        {
            InitDataTime = DateTime.Now;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Monitoring;
        /// <summary>
        /// Execute 구현 내용.
        /// 
        /// 구현 0.     pause 안함.------------------------------------------------ok. 세부구현 : ok.
        /// 구현 1.     한 번이라도 값이 튀면 pause.--------------------------------ok. 세부구현 : ok.
        /// 구현 1-1.   한 번이라도 값이 튀면 강제 Z-Down, GPIB 연결 끊기.-----------ok. 세부구현 : ok.
        /// 구현 1-2.   한 번이라도 값이 뒤면 Z-Down 기다렸다가 Lot Pause.-----------ok. 세부구현 : ok.
        /// 
        /// </summary>
        /// <returns>EventCodeEnum</returns>
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IProbingModule ProbingModule = Module.ProbingModule();


                if (ProbingModule.ModuleState.State == ModuleStateEnum.IDLE
                    || ProbingModule.ModuleState.State == ModuleStateEnum.DONE
                    || Module.GetParam_Wafer().GetState() == EnumWaferState.PROCESSED)
                {
                    retVal = Module.InnerStateTransition(new TCDoneState(Module));
                    return retVal;
                }

                bool IsEmergencyTempWithinRange = this.IsEmergencyTempWithinRange(Module.TempInfo.SetTemp.Value);
                if (IsEmergencyTempWithinRange == false)
                {
                    if (Module.TempControllerDevParameter.TempPauseType.Value == TempPauseTypeEnum.NONE)
                    {
                        // 웨이퍼 로딩 후, 순간적으로 온도가 바뀔 수 있으며, 이 때 이곳으로 들어올 수 있다.
                        Module.InnerStateTransition(new TCWaitingFullReachSetTempTriggerState(Module));
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        //private bool StopProbingThatDependOnParameter()
        //{
        //    bool functionResult = false;
        //    try
        //    {

        //        IProbingModule ProbingModule = Module.ProbingModule();
        //        if (Module.TempControllerDevParameter.TempPauseType.Value
        //            == TempPauseTypeEnum.ZDOWN_ABORT)
        //        {
        //            //구현 1-1 로직.
        //            functionResult = GPIBAbort();
        //        }
        //        else if (Module.TempControllerDevParameter.TempPauseType.Value
        //            == TempPauseTypeEnum.EMERGENCY_ABORT)
        //        {
        //            //구현 1-2 로직.
        //            functionResult = LotPause();
        //        }
        //        IsErrorState = true;

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return functionResult;
        //}

        //private bool LotPause()
        //{
        //    bool result = true;
        //    try
        //    {

        //        if (ReserveCommandType == null)
        //            ReserveCommandType = typeof(ILotOpPause);

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return result;
        //}

        //private bool GPIBAbort()
        //{
        //    bool result = true;
        //    try
        //    {

        //        if (ReserveCommandType == null)
        //            ReserveCommandType = typeof(IGpibAbort);

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return result;
        //}

        //private bool ReservedCommandSet()
        //{
        //    bool result = false;
        //    try
        //    {

        //        IProbingModule ProbingModule = Module.ProbingModule();

        //        if ((ProbingModule.ProbingStateEnum == EnumProbingState.ZDN && ReserveCommandType == typeof(ILotOpPause)))
        //        {
        //            result = CommandSetCommonFunc(ReserveCommandType);
        //            ReserveCommandType = null;
        //            Module.InnerStateTransition(new TCErrorPerformState(Module));
        //        }
        //        else if (ReserveCommandType == typeof(IGpibAbort))
        //        {
        //            result = CommandSetCommonFunc(ReserveCommandType);
        //            ReserveCommandType = null;
        //            ForceZDown();
        //            Module.InnerStateTransition(new TCErrorPerformState(Module));
        //        }
        //        else
        //        {
        //            result = true;
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return result;
        ////}

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCNomalTriggerState : TCStateBase
    {
        public TCNomalTriggerState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.SetToTemp;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double targetTemp = Module.TempInfo.TargetTemp.Value;
                if (Module.TempInfo.SetTemp.Value == targetTemp)
                {
                    retVal = SetTemperature(targetTemp, 0, false);
                }
                else
                {
                    retVal = SetTemperature(targetTemp);
                }
                retVal = Module.InnerStateTransition(new TCWaitUntilNomalSetTempReached(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCWaitUntilNomalSetTempReached : TCStateBase
    {
        private DateTime RunningTime = DateTime.Now;

        public TCWaitUntilNomalSetTempReached(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;
        public override EventCodeEnum Execute()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                bool reachTargetTemp = false;
                DateTime curDateTime = DateTime.Now;
                TimeSpan ts = curDateTime - RunningTime;
                Module.RunTimeSpan = ts;

              if (Module.TempControllerParam.LimitRunTimeSeconds.Value < ts.TotalSeconds)
                {
                    ICommandManager CommandManager = Module.CommandManager();
                    ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                    if (TempDisplayDialogService.IsShowing)
                    {
                        TempDisplayDialogService.CloseDialog();
                    }
                    //CommandManager.SetCommand<IGpibAbort>(Module);
                    if (Module.TempControllerDevParameter.TempPauseType.Value != TempPauseTypeEnum.NONE)
                    {
                        StopProbingThatDependOnParameter();
                    }

                    ReservedCommandSet();
                    //Module.InnerStateTransition(new TCErrorPerformState(Module));
                }
                else
                {
                    try
                    {
                        reachTargetTemp = IsTempWithinRange(Module.TempInfo.SetTemp.Value);
                        if (reachTargetTemp == true)
                        {
                            ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                            if (TempDisplayDialogService.IsShowing)
                            {
                                TempDisplayDialogService.CloseDialog();
                            }
                            retVal = Module.InnerStateTransition(new TCDoneState(Module));
                            return retVal;
                        }
                        retVal = EventCodeEnum.NONE;
                    }
                    catch
                    {
                        Module.InnerStateTransition(new TCErrorState(Module));
                        retVal = EventCodeEnum.UNDEFINED;
                        return retVal;
                    }
                }

                #region => ISetTempForFrontDoorOpen Command
                Func<bool> conditionFunc = () => true;
                Action setTempForFrontDoorOpenDoAction = SetTempForFrontDoorOpen;
                Action setTempForFrontDoorOpenAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCWaitUntilNomalSetTempReached]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ISetTempForFrontDoorOpen()] => {retVal.ToString()}");
                    };
                bool consumed = Module.CommandManager()
                .ProcessIfRequested<ISetTempForFrontDoorOpen>(
                 Module, conditionFunc, setTempForFrontDoorOpenDoAction, setTempForFrontDoorOpenAbortAction);
                #endregion

                if ((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE)
                {
                    return retVal;
                }

                EventCodeEnum executeResult = EventCodeEnum.NONE;
                #region => ITemperatureSettingTriggerOccurrence Command
                Action temperatureSettingTriggerOccurrenceDoAction = TemperatureSettingTriggerOccurrenceDoAction;
                Action temperatureSettingTriggerOccurrenceAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ITemperatureSettingTriggerOccurrence()] => {executeResult.ToString()}");
                    };

                void TemperatureSettingTriggerOccurrenceDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] " +
                        $"StateTransition to {nameof(TC_ColdNomalTriggerState)} => {executeResult.ToString()}");
                }
                #endregion

                consumed = Module.CommandManager()
                   .ProcessIfRequested<ITemperatureSettingTriggerOccurrence>(
                       Module, conditionFunc, temperatureSettingTriggerOccurrenceDoAction, temperatureSettingTriggerOccurrenceAbortAction);

                return retVal;

                void SetTempForFrontDoorOpen()
                {
                    retVal = Module.InnerStateTransition(new TCSetTempForFrontDoorOpenReachedTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - SetTempForFrontDoorOpen()] " +
                        $"StateTransition to {nameof(TCSetTempForFrontDoorOpenReachedTriggerState)} => {retVal.ToString()}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {
                isValidCommand = CheckCanExecuteUsingInterfaceType(token, 
                    typeof(ISetTempForFrontDoorOpen),
                    typeof(ITemperatureSettingTriggerOccurrence));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }


        private new EventCodeEnum PreSetTempCompareExcutor()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (Module.TempInfo.SetTemp.Value != Module.TempInfo.TargetTemp.Value)
                {
                    retVal = Module.InnerStateTransition(new TCNomalTriggerState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCWaitingFullReachSetTempTriggerState : TCStateBase
    {
        public TCWaitingFullReachSetTempTriggerState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //double SetTemp = Module.TempControllerDevParameter.SetTemp.Value;
                double targetTemp = Module.TempInfo.TargetTemp.Value;

                retVal = SetTemperature(targetTemp);

                ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                if (TempDisplayDialogService.IsShowing == false)
                {
                    TempDisplayDialogService.TurnOnPossibleFlag();
                    Task dialogServiceTask = Task.Run(async () =>
                    {
                        bool result = false;
                        result = await TempDisplayDialogService.ShowDialog();

                        if (result == false)
                        {
                            Module.InnerStateTransition(new TCErrorState(Module));
                        }
                    });
                }

                retVal = Module.InnerStateTransition(new TCWaitUntilSetTempFullReached(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCWaitUntilSetTempFullReached : TCStateBase
    {
        private DateTime RunningTime = DateTime.Now;

        public TCWaitUntilSetTempFullReached(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool reachTargetTemp = false;
                DateTime curDateTime = DateTime.Now;
                TimeSpan ts = curDateTime - RunningTime;
                Module.RunTimeSpan = ts;

                if (Module.TempControllerParam.LimitRunTimeSeconds.Value < ts.TotalSeconds)
                {
                    ICommandManager CommandManager = Module.CommandManager();
                    ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                    if (TempDisplayDialogService.IsShowing)
                    {
                        TempDisplayDialogService.CloseDialog();
                    }

                    //Pause Type 부른다. 동작한다.
                    if (Module.TempControllerDevParameter.TempPauseType.Value != TempPauseTypeEnum.NONE)
                    {
                        StopProbingThatDependOnParameter();
                    }

                    ReservedCommandSet();
                    //Module.InnerStateTransition(new TCErrorPerformState(Module));
                }
                else
                {
                    try
                    {
                        reachTargetTemp = IsTempWithinRange(Module.TempInfo.SetTemp.Value);
                        if (reachTargetTemp == true)
                        {
                            ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                            if (TempDisplayDialogService.IsShowing)
                            {
                                TempDisplayDialogService.CloseDialog();
                            }
                            retVal = Module.InnerStateTransition(new TCDoneState(Module));
                            return retVal;
                        }
                        retVal = EventCodeEnum.NONE;
                    }
                    catch
                    {
                        ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                        if (TempDisplayDialogService.IsShowing)
                        {
                            TempDisplayDialogService.CloseDialog();
                        }
                        retVal = Module.InnerStateTransition(new TCErrorState(Module));
                        return retVal;
                    }
                }

                bool consumed;
                EventCodeEnum executeResult = EventCodeEnum.NONE;
                Func<bool> conditionFunc = () => true;
                #region => ITemperatureSettingTriggerOccurrence Command
                Action temperatureSettingTriggerOccurrenceDoAction = TemperatureSettingTriggerOccurrenceDoAction;
                Action temperatureSettingTriggerOccurrenceAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ITemperatureSettingTriggerOccurrence()] => {executeResult.ToString()}");
                    };

                void TemperatureSettingTriggerOccurrenceDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TCNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] " +
                        $"StateTransition to {nameof(TCNomalTriggerState)} => {executeResult.ToString()}");
                }
                #endregion

                consumed = Module.CommandManager()
                   .ProcessIfRequested<ITemperatureSettingTriggerOccurrence>(
                       Module, conditionFunc, temperatureSettingTriggerOccurrenceDoAction, temperatureSettingTriggerOccurrenceAbortAction);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token,
                    typeof(ITemperatureSettingTriggerOccurrence));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }


    public class TCOverHeatingTriggerState : TCStateBase
    {
        public TCOverHeatingTriggerState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.SetToTemp;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double targetTemp = Module.TempInfo.TargetTemp.Value;
                double overheatingTemp = Module.TempControllerDevParameter.OverHeatingOffset.Value;
                retVal = SetTemperature(targetTemp, overheatingTemp);
                retVal = Module.InnerStateTransition(new TCWaitUntilOverHeatingSetTempReached(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCWaitUntilOverHeatingSetTempReached : TCStateBase
    {
        private DateTime RunningTime = DateTime.Now;

        public TCWaitUntilOverHeatingSetTempReached(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool reachTargetTemp = false;
                DateTime curDateTime = DateTime.Now;
                TimeSpan ts = curDateTime - RunningTime;
                Module.RunTimeSpan = ts;

                if (Module.TempControllerParam.LimitRunTimeSeconds.Value < ts.TotalSeconds)
                {
                    retVal = Module.InnerStateTransition(new TCErrorState(Module));
                    return retVal;
                }
                else
                {
                    try
                    {
                        IWaferAligner WaferAligner = Module.WaferAligner();
                        IWaferTransferModule WaferTransferModule = Module.WaferTransferModule();

                        reachTargetTemp = IsTempWithinRange(Module.TempInfo.SetTemp.Value, TempCheckType.OVERHEATING);
                        if ((reachTargetTemp == true && WaferTransferModule.ModuleState.State == ModuleStateEnum.DONE)
                            || WaferAligner.ModuleState.State == ModuleStateEnum.DONE)
                        {
                            ReturnToOriginSetTemp();
                            retVal =  Module.InnerStateTransition(new TCDoneState(Module));
                            return retVal;
                        }
                        retVal = EventCodeEnum.NONE;
                    }
                    catch
                    {
                        retVal = Module.InnerStateTransition(new TCErrorState(Module));
                        return retVal;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private EventCodeEnum ReturnToOriginSetTemp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetTemperature(Module.TempInfo.SetTemp.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCSetTempForFrontDoorOpenReachedTriggerState : TCStateBase
    {
        public TCSetTempForFrontDoorOpenReachedTriggerState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.SetToTemp;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double SetTemp = Module.TempControllerParam.FrontDoorOpenTemp.Value;

                retVal = SetTemperature(SetTemp, willYouSaveSetValue: false);
                retVal = Module.InnerStateTransition(new TCWaitUntilSetTempForFrontDoorOpenReached(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCWaitUntilSetTempForFrontDoorOpenReached : TCStateBase
    {
        private DateTime RunningTime = DateTime.Now;

        public TCWaitUntilSetTempForFrontDoorOpenReached(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;
        public override EventCodeEnum Execute()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                bool reachTargetTemp = false;
                bool consumed = false;
                DateTime curDateTime = DateTime.Now;
                TimeSpan ts = curDateTime - RunningTime;
                Module.RunTimeSpan = ts;

                try
                {
                    #region => IStopFrontDoorOpenSetTemp Command
                    Func<bool> conditionFunc = () => true;
                    Action stopFrontDoorOpenSetTempDoAction = StopFrontDoorOpenSetTemp;
                    Action stopFrontDoorOpenSetTempAbortAction =
                        () =>
                        {
                            Module.MetroDialogManager().ShowMessageDialog(
                                "[TCWaitUntilSetTempForFrontDoorOpenReached]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                            LoggerManager.Debug($"[{this.GetType().Name} - IStopFrontDoorOpenSetTemp()] => Fail");
                        };
                    #endregion

                    consumed = Module.CommandManager()
                        .ProcessIfRequested<IReturnToDefaltSetTemp>(
                            Module, conditionFunc, stopFrontDoorOpenSetTempDoAction, stopFrontDoorOpenSetTempAbortAction);

                    reachTargetTemp = IsTempWithinRange(Module.TempControllerParam.FrontDoorOpenTemp.Value);
                    if (reachTargetTemp == true)
                    {
                        //retVal = SetTemperature(TempController.PreSetTemp);
                        Module.InnerStateTransition(new TCDoneState(Module));
                    }

                    if (Module.SequenceRunner().ModuleState.GetState() == ModuleStateEnum.DONE
                        || Module.SequenceRunner().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        ICommandManager CommandManager = Module.CommandManager();
                        Module.InnerStateTransition(new TCIdleState(Module));
                        bool t = CommandManager.SetCommand<IChangeTemperatureToSetTemp>(Module);
                    }
                    retVal = EventCodeEnum.NONE;
                }
                catch
                {
                    retVal = SetTemperature(Module.TempInfo.PreSetTemp.Value);
                    Module.InnerStateTransition(new TCErrorState(Module));
                    retVal = EventCodeEnum.UNDEFINED;
                }

                return retVal;


                void StopFrontDoorOpenSetTemp()
                {
                    retVal = Module.InnerStateTransition(new TCIdleState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - SetTempForFrontDoorOpen()] " +
                        $"StateTransition to {nameof(TCDoneState)} => {retVal.ToString()}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token, typeof(IReturnToDefaltSetTemp));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCErrorPerformState : TCStateBase
    {
        public TCErrorPerformState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Error;
        /// <summary>
        /// Execute() 구현.
        /// 
        ///         구현 1.     Error에 빠지기전에 처리.------------------------------------ok. 세부구현 : ok.
        /// to do:  구현 1-1.   알람.------------------------------------------------------ok. 세부구현 : -no.
        ///         구현 1-2.   State Breaker.---------------------------------------------ok. 세부구현 : ok.
        /// 
        /// </summary>
        /// <returns></returns>
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetErrorState();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void SetErrorState()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                bool functionResult = false;

                //구현 1 로직.
                functionResult = ProceduresPriorToErrorStateChange();
                retVal = Module.InnerStateTransition(new TCErrorState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private bool ProceduresPriorToErrorStateChange()
        {
            bool functionResult = false;
            try
            {
                //구현 1-1 로직.
                functionResult = Alam();

                //구현 1-2 로직.
                if (Module.TempControllerDevParameter.IsBreakAlignState.Value == true 
                    && Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROBING) //zup, probing일 때는 위험하니 Align 데이터 깨지않도록 한다.
                {
                    functionResult = AlignModuleStateBreaker();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return functionResult;
        }

        private bool Alam()
        {
            bool result = false;
            try
            {
                //todo : Alam().. how???
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }

        private bool AlignModuleStateBreaker()
        {
            bool result = false;
            try
            {

                try
                {
                    Module.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                    Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    Module.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.IDLE);
                    result = true;
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    result = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCErrorState : TCStateBase
    {
        public TCErrorState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ERROR;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Error;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                #region => IEndTempEmergencyError Command

                bool consumed;
                EventCodeEnum executeResult = EventCodeEnum.NONE;
                Func<bool> conditionFunc = () => true;
                Action IEndTempEmergencyErrorDoAction = ChangeEndTempEmergencyErrorDoAction;
                consumed = Module.CommandManager()
                    .ProcessIfRequested<IEndTempEmergencyError>(
                        Module, conditionFunc, IEndTempEmergencyErrorDoAction);

                void ChangeEndTempEmergencyErrorDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TCNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeEndTempEmergencyErrorDoAction()] " +
                        $"StateTransition to {nameof(TCNomalTriggerState)} => {executeResult.ToString()}");
                }
                #endregion

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {
                isValidCommand = CheckCanExecuteUsingInterfaceType(token, typeof(IEndTempEmergencyError));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new TCIdleState(Module));

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new TCIdleState(Module));

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TCDoneState : TCStateBase
    {
        public TCDoneState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Done;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE)
                {
                    return retVal;
                }


                if (!Module.IsCurTempWithinSetTempRange(false))
                {
                    retVal = Module.InnerStateTransition(new TCWaitUntilNomalSetTempReached(Module));
                    return retVal;
                }

                if ((retVal = BehaviorFollowingProbingModuleState()) == EventCodeEnum.NONE)
                    return retVal;
                //retVal = Module.InnerStateTransition(new TCIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token,
                    typeof(ITemperatureSettingTriggerOccurrence));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }
}

// 현재 사용하지 않아 주석 처리 해놓음 - 2023/05/16

//using System;

//namespace ChillerModule.Chago
//{
//    using EasyModbus;
//    using LogModule;
//    using ProberErrorCode;
//    using ProberInterfaces;
//    using ProberInterfaces.Enum;
//    using ProberInterfaces.Temperature;
//    using System.Net.NetworkInformation;
//    using Temperature.Temp.Chiller;

//    public class ChagoChillerAdapter : IChillerAdapter
//    {
//        public ChagoChillerCommState ChillerCommState { get;  set; }
//        public bool Initialized { get; set; }
//        private bool IsDisposed = false;
//        private Ping ping = new Ping();
//        private int _SubModuleIndex;

//        public int SubModuleIndex
//        {
//            get { return _SubModuleIndex; }
//            set { _SubModuleIndex = value; }
//        }

//        public void Dispose()
//        {
//            try
//            {
//                if (!IsDisposed)
//                {
//                    ChillerCommState.DisConnect();
//                    IsDisposed = true;
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.Dispose() Error occurred. Err = {err.Message}");
//            }
//        }

//        public EventCodeEnum InitModule()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

//            try
//            {
//                if (!Initialized)
//                {
//                    ChillerCommState?.DisConnect();
//                    ChillerCommState = new DisconnectState(this);
//                    Initialized = true;
//                    retVal = EventCodeEnum.NONE;
//                }
//            }
//            catch (Exception err)
//            {
//                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
//                retVal = EventCodeEnum.UNDEFINED;

//            }

//            return retVal;
//        }

//        public void DeInitModule()
//        {
//            Dispose();
//        }

//        public EnumCommunicationState GetCommState(byte subModuleIndex = 0x00)
//            => this.ChillerCommState?.GetCommunicationState() ?? EnumCommunicationState.DISCONNECT;

//        public void CommStateTransition(ChagoChillerCommState state)
//        {
//            if (ChillerCommState?.GetType() != state?.GetType())
//            {
//                this.ChillerCommState = state;
//            }
//        }

//        public EventCodeEnum Connect(string address, int port)
//            => this?.ChillerCommState.Connect(address, port)
//            ?? EventCodeEnum.UNDEFINED;

//        public IPStatus PingTest(string address)
//            => this.ping.Send(address)?.Status ?? IPStatus.Unknown;

//        public void DisConnect()
//            => this?.ChillerCommState.DisConnect();

//        public void SetCommUnitID(byte unitID)
//        {
//            if(ChillerCommState.GetCommunicationState() == EnumCommunicationState.CONNECTED)
//            {
//                ChillerCommState.SetCommUnitID(unitID);
//            }
//        }

//        public ICommunicationMeans GetCommunicationObj()
//        {
//            ICommunicationMeans obj = null;
//            try
//            {
//                if (GetCommState() == EnumCommunicationState.CONNECTED)
//                {
//                    obj = (ChillerCommState as ConnectState)?.Client;
//                }
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw err;
//            }
//            return obj;
//        }

//        public object GetCommLockObj()
//        {
//            return null;
//        }

//        public EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false)
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                ChillerCommState.CheckCanUseChiller(sendVal, stageindex, offvalve);
//            }
//            catch (Exception  err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retVal;
//        }

//        public bool GetErorrState(int stageindex = -1)
//        {
//            return false;
//        }

//        #region ==> Get Data from Chiller
//        /// <summary>
//        /// Value in 0.01℃ unit. 1000 -> 10.0℃   (-21474836.48)
//        /// </summary>
//        /// <returns></returns>
//        public double GetSetTempValue(byte subModuleIndex = 0x00)
//        {
//            double retVal = ChillerCommState.GetSetTempValue(subModuleIndex);
//            return (double)retVal;
//        }
//        /// <summary>
//        /// Internal temperature
//        /// Value in 0.01℃ unit. 1000 -> 10.0℃
//        /// </summary>
//        /// <returns></returns>

//        public double GetInternalTempValue(byte subModuleIndex = 0x00)
//        {
//            double retVal = ChillerCommState.GetInternalTempValue(subModuleIndex);
//            return (double)retVal;
//        }

//        //- return temperature
//        public double GetReturnTempVal(byte subModuleIndex = 0x00)
//        {
//            double retVal = ChillerCommState.GetReturnTempVal(subModuleIndex);
//            return (double)retVal;
//        }
//        /// <summary>
//        /// Value in mbar
//        /// pump pressure(absolute)
//        /// </summary>
//        /// <returns></returns>
//        public int GetPumpPressureVal(byte subModuleIndex = 0x00)
//        {
//            int retVal;
//            retVal = ChillerCommState.GetPumpPressureVal(subModuleIndex);
//            return retVal;
//        }

//        /// <summary>
//        /// Power consumption of chiller in W unit.
//        /// Negative value for cold operation to -32767, 
//        /// Positive value for heating operation to 32767.
//        /// </summary>
//        /// <returns></returns>
//        public int GetCurrentPower(byte subModuleIndex = 0x00)
//        {
//            int retVal;
//            retVal = ChillerCommState.GetCurrentPower(subModuleIndex);
//            return retVal;
//        }

//        //- error report
//        public int GetErrorReport(byte subModuleIndex = 0x00)
//        {
//            int retVal;
//            retVal = ChillerCommState.GetErrorReport(subModuleIndex);
//            return retVal;
//        }

//        //- warning message
//        public int GetWarningMessage(byte subModuleIndex = 0x00)
//        {
//            int retVal;
//            retVal = ChillerCommState.GetWarningMessage(subModuleIndex);
//            return retVal;
//        }

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public double GetProcessTempVal(byte subModuleIndex = 0x00)
//        {
//            double retVal = ChillerCommState.GetProcessTempVal(subModuleIndex);
//            return (double)retVal * 0.01;
//        }

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public double GetExtMoveVal(byte subModuleIndex = 0x00)
//        {
//            double retVal = ChillerCommState.GetExtMoveVal(subModuleIndex);
//            return (double)retVal;
//        }

//        //- status of the thermostat
//        // Bit 0    : Temperature control operating mode: 1: active / 0: inactive
//        // Bit 1    : Circulation operating mode: 1: active / 0: inactive
//        // Bit 2    : Refrigerator compressor: 1: switched on / 0: switched off
//        // Bit 3    : Temperature control mode "Process control": 1: active / 0: inactive
//        // Bit 4    : Circulationg pump: 1: switched on / 0: switched off
//        // Bit 5    : Cooling power available: 1: available / 0: not available
//        // Bit 6    : Tkeylock: 1: active / 0: inactive
//        // Bit 7    : PID parameter set, temperature controller : 1: Automatic mode / 0: Expert mode
//        // Bit 8    : Error: 1: Error occurred / 0: no error
//        // Bit 9    : Warning: 1: Error occurred / 0: no error
//        // Bit 10   : Mode for presetting the internal temperature (see Address 8): 1: active / 0: inactive
//        // Bit 11   : Mode for presetting the external temperature (see Address 9): 1: active / 0: inactive
//        // Bit 12   : DV E-grade: 1: activated / 0: not activated
//        // Bit 13   : Not Using.
//        // Bit 14   : Restart electronics / Power failure (*): 1: No new start / 0: New start
//        // Bit 15   : Freeze protection (not available on all devices): 1: active / ): inactive
//        public int GetStatusOfThermostat(byte subModuleIndex = 0x00)
//        {
//            int retVal;
//            retVal = ChillerCommState.GetStatusOfThermostat(subModuleIndex);
//            return retVal;
//        }

//        //- AutoPID
//        // The value 1 means that the controller is working in automatic mode.
//        // The value 0 means that the controller is working in expert mode.
//        //  (If you would like to work in expert mode, 
//        //      the individual control parameters must be entered first.)
//        public bool IsAutoPID(byte subModuleIndex = 0x00)
//        {
//            bool retVal;
//            retVal = ChillerCommState.IsAutoPID(subModuleIndex);
//            return retVal;
//        }

//        //- temperature control mode
//        //  0: Temperature control mode internal
//        //  1: Temperature control mode process (cascade control)
//        public bool IsTempControlProcessMode(byte subModuleIndex = 0x00)
//        {
//            bool retVal;
//            retVal = ChillerCommState.IsTempControlProcessMode(subModuleIndex);
//            return retVal;
//        }

//        //- Temperature control current status.
//        //  0: Temperature control not active.
//        //  1: Temperature control active.
//        public bool IsTempControlActive(byte subModuleIndex = 0x00)
//        {
//            bool retVal;
//            retVal = ChillerCommState.IsTempControlActive(subModuleIndex);
//            return retVal;
//        }

//        //- Process temp actual value presetting mode
//        //  프로세스 온도 실제 값 설정 모드.
//        //  Bit 0:  Operating mode active or inactive.
//        //          0: Mode not active.
//        //          1: Mode active.
//        //  Bit 1: Watchdog behavior.
//        //          0: The thermostat issues the warning -2130, deactivates the actual value presetting,
//        //          changes from process to inernal control (see vTmpMode, Variable 19) and continues to
//        //          operate temperture control.
//        //          1: The thermostat issues the error message -329 and changes to standby mode
//        //          (temperature control is Stopped).
//        public (bool, bool) GetProcTempActValSetMode(byte subModuleIndex = 0x00)
//        {
//            (bool, bool) retVal;
//            retVal = ChillerCommState.GetProcTempActValSetMode(subModuleIndex);
//            return retVal;
//        }

//        public int GetSerialNumLow(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = ChillerCommState.GetSerialNumLow(subModuleIndex);
//            return retVal;
//        }

//        public int GetSerialNumHigh(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = ChillerCommState.GetSerialNumHigh(subModuleIndex);
//            return retVal;
//        }

//        public int GetSerialNumber(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = ChillerCommState.GetSerialNumber(subModuleIndex);
//            return retVal;
//        }

//        //start or stop circulation
//        //0: Circulation operationg mode not active.
//        //1: Circulation operationg mode active.
//        //Note: If the temperatue control is active, 
//        //  circulation is also carried out, but circulation operationg mode is not active.
//        public bool IsCirculationActive(byte subModuleIndex = 0x00)
//        {
//            bool retVal;
//            retVal = ChillerCommState.IsCirculationActive(subModuleIndex);
//            return retVal;
//        }

//        //Activate or deactivate touch panel lock
//        //Activate, deactivate or query operating lock at Pilot.
//        //Bit 0:    Operating lock active or inactive.
//        //          0: Operating lock inactive.
//        //          1: Operating lock active. Manual operation of the thermostat via Pilot is not possible.
//        //Bit 1:    Watchdog behavior
//        //          0: Watchdog inactive.
//        //          1: Activate Watchdog for 30s. If Bit 1 is not reset within 30s, the operating lock is
//        //          automatically cancelled. This can be used to permit manual operation again if
//        //          communication with the thermostat is interrupted for any reason.
//        public (bool, bool) IsOperatingLock(byte subModuleIndex = 0x00)
//        {
//            (bool, bool) retVal;
//            retVal = ChillerCommState.IsOperatingLock(subModuleIndex);
//            return retVal;
//        }

//        //- pump speed
//        public int GetPumpSpeed(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = ChillerCommState.GetPumpSpeed(subModuleIndex);
//            return retVal;
//        }

//        //- minimum setpoint
//        public double GetMinSetTemp(byte subModuleIndex = 0x00)
//        {
//            double retVal = ChillerCommState.GetMinSetTemp(subModuleIndex);
//            return (double)retVal;
//        }

//        //- maximum setpoint
//        public double GetMaxSetTemp(byte subModuleIndex = 0x00)
//        {
//            double retVal = ChillerCommState.GetMaxSetTemp(subModuleIndex);
//            return (double)retVal;
//        }

//        //read setpoint pump speed
//        public int GetSetTempPumpSpeed(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = ChillerCommState.GetTargetPumpSpeed(subModuleIndex);
//            return retVal;
//        }

//        //RW    /Upper alram internal limit.
//        public double GetUpperAlramInternalLimit(byte subModuleIndex = 0x00)
//        {
//            double retVal = 0;
//            retVal = ChillerCommState.GetUpperAlramInternalLimit(subModuleIndex);
//            return retVal;
//        }

//        //RW    /Lower alram external limit.
//        public double GetLowerAlramInternalLimit(byte subModuleIndex = 0x00)
//        {
//            double retVal = 0;
//            retVal = ChillerCommState.GetLowerAlramInternalLimit(subModuleIndex);
//            return retVal;
//        }

//        //RW    /Upper alram process limit.
//        public double GetUpperAlramProcessLimit(byte subModuleIndex = 0x00)
//        {
//            double retVal = 0;
//            retVal = ChillerCommState.GetUpperAlramProcessLimit(subModuleIndex);
//            return retVal;
//        }

//        //RW    /Lower alram process limit.
//        public double GetLowerAlramProcessLimit(byte subModuleIndex = 0x00)
//        {
//            double retVal = 0;
//            retVal = ChillerCommState.GetLowerAlramProcessLimit(subModuleIndex);
//            return retVal;
//        }

//        #endregion

//        #region ==> Set data to Chiller


//        public EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, byte subModuleIndex = 0x00)
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                switch (sendTempValueType)
//                {
//                    case TempValueType.HUBER:
//                        break;
//                    case TempValueType.TEMPCONTROLLER:
//                        sendVal *= 10;
//                        break;
//                    case TempValueType.CELSIUS:
//                        sendVal *= 100;
//                        break;
//                    default:
//                        break;
//                }
//                LoggerManager.Debug($"[ChillerComm SetTargetTemp] Temp : {sendVal} , TempValueTarget : {sendTempValueType}");
//                retVal = ChillerCommState.SetTargetTemp((int)sendVal, subModuleIndex);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_ERROR;
//            }
//            return retVal;
//        }

//        public void SetPresetProcTempVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetPresetProcTempVal(sendVal, subModuleIndex);
//        }

//        public void SetAutoPIDMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetAutoPIDMode(bValue, subModuleIndex);
//        }

//        public void SetTempMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetTempMode(bValue, subModuleIndex);
//        }

//        public void SetTempActiveMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetTempActiveMode(bValue, subModuleIndex);
//        }

//        public void SetProcTempActValSetMode(bool isOperatingMode, bool isChangeStandbyModeWhenErr329, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetProcTempActValSetMode(isOperatingMode, isChangeStandbyModeWhenErr329, subModuleIndex);
//        }


//        public void SetMinSetTemp(int sendVal, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetMinSetTemp(sendVal, subModuleIndex);
//        }


//        public void SetMaxSetTemp(int sendVal, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetMaxSetTemp(sendVal, subModuleIndex);
//        }


//        public void SetAlarmInternalUpperVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetAlarmInternalUpperVal(sendVal, subModuleIndex);
//        }


//        public void SetAlarmInternalLowerVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetAlarmInternalLowerVal(sendVal, subModuleIndex);
//        }


//        public void SetAlarmProcessUpperVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetAlarmProcessUpperVal(sendVal, subModuleIndex);
//        }

//        public void SetAlarmProcessLowerVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetAlarmProcessLowerVal(sendVal, subModuleIndex);
//        }

//        //- operating lock
//        public void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetOperatingLock(bOperatinglock, bWatchdogBehavior, subModuleIndex);
//        }

//        //- circuation Active
//        public void SetCircuationActive(bool bValue, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetCircuationActive(bValue, subModuleIndex);
//        }

//        //- read setpoint pump speed
//        public void SetSetTempPumpSpeed(int iValue, byte subModuleIndex = 0x00)
//        {
//            ChillerCommState.SetPumpSpeed(iValue, subModuleIndex);
//        }
//        #endregion

//    }

//    public abstract class ChagoChillerCommState
//    {
//        protected ChagoChillerAdapter CommModule;
//        public ChagoChillerCommState(ChagoChillerAdapter commModule)
//        {
//            this.CommModule = commModule;
//        }
//        public abstract EnumCommunicationState GetCommunicationState(byte subModuleIndex = 0x00);

//        public abstract EventCodeEnum Connect(string address, int port);

//        public abstract void DisConnect();

//        public abstract void SetCommUnitID(byte unitID);
//        public EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false)
//        {
//            return EventCodeEnum.UNDEFINED;
//        }

//        #region ==> Get Data from Chiller
//        //- Setpoint,temperatur controller
//        public abstract double GetSetTempValue(byte subModuleIndex = 0x00);

//        //- Internal temperature
//        public abstract double GetInternalTempValue(byte subModuleIndex = 0x00);

//        //- return temperature
//        public abstract double GetReturnTempVal(byte subModuleIndex = 0x00);

//        //- pump pressure(absolute)
//        public abstract int GetPumpPressureVal(byte subModuleIndex = 0x00);

//        //- current power
//        public abstract int GetCurrentPower(byte subModuleIndex = 0x00);

//        //- error report
//        public abstract int GetErrorReport(byte subModuleIndex = 0x00);

//        //- warning message
//        public abstract int GetWarningMessage(byte subModuleIndex = 0x00);

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public abstract double GetProcessTempVal(byte subModuleIndex = 0x00);

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public abstract double GetExtMoveVal(byte subModuleIndex = 0x00);

//        //- status of the thermostat
//        // Bit 0    : Temperature control operating mode: 1: active / 0: inactive
//        // Bit 1    : Circulation operating mode: 1: active / 0: inactive
//        // Bit 2    : Refrigerator compressor: 1: switched on / 0: switched off
//        // Bit 3    : Temperature control mode "Process control": 1: active / 0: inactive
//        // Bit 4    : Circulationg pump: 1: switched on / 0: switched off
//        // Bit 5    : Cooling power available: 1: available / 0: not available
//        // Bit 6    : Tkeylock: 1: active / 0: inactive
//        // Bit 7    : PID parameter set, temperature controller : 1: Automatic mode / 0: Expert mode
//        // Bit 8    : Error: 1: Error occurred / 0: no error
//        // Bit 9    : Warning: 1: Error occurred / 0: no error
//        // Bit 10   : Mode for presetting the internal temperature (see Address 8): 1: active / 0: inactive
//        // Bit 11   : Mode for presetting the external temperature (see Address 9): 1: active / 0: inactive
//        // Bit 12   : DV E-grade: 1: activated / 0: not activated
//        // Bit 13   : Not Using.
//        // Bit 14   : Restart electronics / Power failure (*): 1: No new start / 0: New start
//        // Bit 15   : Freeze protection (not available on all devices): 1: active / ): inactive
//        public abstract int GetStatusOfThermostat(byte subModuleIndex = 0x00);

//        //- AutoPID
//        // The value 1 means that the controller is working in automatic mode.
//        // The value 0 means that the controller is working in expert mode.
//        //  (If you would like to work in expert mode, 
//        //      the individual control parameters must be entered first.)
//        public abstract bool IsAutoPID(byte subModuleIndex = 0x00);

//        //- temperature control mode
//        //  0: Temperature control mode internal
//        //  1: Temperature control mode process (cascade control)
//        public abstract bool IsTempControlProcessMode(byte subModuleIndex = 0x00);

//        //- Temperature control current status.
//        //  0: Temperature control not active.
//        //  1: Temperature control active.
//        public abstract bool IsTempControlActive(byte subModuleIndex = 0x00);

//        //- Process temp actual value presetting mode
//        //  프로세스 온도 실제 값 설정 모드.
//        //  Bit 0:  Operating mode active or inactive.
//        //          0: Mode not active.
//        //          1: Mode active.
//        //  Bit 1: Watchdog behavior.
//        //          0: The thermostat issues the warning -2130, deactivates the actual value presetting,
//        //          changes from process to inernal control (see vTmpMode, Variable 19) and continues to
//        //          operate temperture control.
//        //          1: The thermostat issues the error message -329 and changes to standby mode
//        //          (temperature control is Stopped).
//        public abstract (bool, bool) GetProcTempActValSetMode(byte subModuleIndex = 0x00);

//        public abstract int GetSerialNumLow(byte subModuleIndex = 0x00);

//        public abstract int GetSerialNumHigh(byte subModuleIndex = 0x00);

//        public abstract int GetSerialNumber(byte subModuleIndex = 0x00);

//        //start or stop circulation
//        //0: Circulation operationg mode not active.
//        //1: Circulation operationg mode active.
//        //Note: If the temperatue control is active, 
//        //  circulation is also carried out, but circulation operationg mode is not active.
//        public abstract bool IsCirculationActive(byte subModuleIndex = 0x00);

//        //Activate or deactivate touch panel lock
//        //Activate, deactivate or query operating lock at Pilot.
//        //Bit 0:    Operating lock active or inactive.
//        //          0: Operating lock inactive.
//        //          1: Operating lock active. Manual operation of the thermostat via Pilot is not possible.
//        //Bit 1:    Watchdog behavior
//        //          0: Watchdog inactive.
//        //          1: Activate Watchdog for 30s. If Bit 1 is not reset within 30s, the operating lock is
//        //          automatically cancelled. This can be used to permit manual operation again if
//        //          communication with the thermostat is interrupted for any reason.
//        public abstract (bool, bool) IsOperatingLock(byte subModuleIndex = 0x00);

//        //- pump speed
//        public abstract int GetPumpSpeed(byte subModuleIndex = 0x00);

//        //- minimum setpoint
//        public abstract double GetMinSetTemp(byte subModuleIndex = 0x00);

//        //- maximum setpoint
//        public abstract double GetMaxSetTemp(byte subModuleIndex = 0x00);

//        //read setpoint pump speed
//        public abstract int GetTargetPumpSpeed(byte subModuleIndex = 0x00);

//        //RW    /Upper alram internal limit.
//        public abstract double GetUpperAlramInternalLimit(byte subModuleIndex = 0x00);

//        //RW    /Lower alram external limit.
//        public abstract double GetLowerAlramInternalLimit(byte subModuleIndex = 0x00);

//        //RW    /Upper alram process limit.
//        public abstract double GetUpperAlramProcessLimit(byte subModuleIndex = 0x00);

//        //RW    /Lower alram process limit.
//        public abstract double GetLowerAlramProcessLimit(byte subModuleIndex = 0x00);

//        #endregion

//        #region ==> Set data to Chiller
//        public abstract EventCodeEnum SetTargetTemp(double sendVal, byte subModuleIndex = 0x00);

//        public abstract void SetPresetProcTempVal(double sendVal, byte subModuleIndex = 0x00);

//        public abstract void SetAutoPIDMode(bool bValue, byte subModuleIndex = 0x00);

//        public abstract void SetTempMode(bool bValue, byte subModuleIndex = 0x00);

//        public abstract void SetTempActiveMode(bool bValue, byte subModuleIndex = 0x00);

//        public abstract void SetProcTempActValSetMode(bool isOperatingMode, bool isChangeStandbyModeWhenErr329, byte subModuleIndex = 0x00);

//        public abstract void SetMinSetTemp(double sendVal, byte subModuleIndex = 0x00);

//        public abstract void SetMaxSetTemp(double sendVal, byte subModuleIndex = 0x00);

//        public abstract void SetAlarmInternalUpperVal(double sendVal, byte subModuleIndex = 0x00);

//        public abstract void SetAlarmInternalLowerVal(double sendVal, byte subModuleIndex = 0x00);

//        public abstract void SetAlarmProcessUpperVal(int sendVal, byte subModuleIndex = 0x00);

//        public abstract void SetAlarmProcessLowerVal(int sendVal, byte subModuleIndex = 0x00);

//        //- operating lock
//        public abstract void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior, byte subModuleIndex = 0x00);

//        //- circuation Active
//        public abstract void SetCircuationActive(bool bValue, byte subModuleIndex = 0x00);

//        //- read setpoint pump speed
//        public abstract void SetPumpSpeed(int iValue, byte subModuleIndex = 0x00);

//        #endregion

//    }

//    public sealed class DisconnectState : ChagoChillerCommState
//    {
//        public DisconnectState(ChagoChillerAdapter commModule) : base(commModule)
//        {
//        }
//        public override EnumCommunicationState GetCommunicationState(byte subModuleIndex = 0x00)
//             => EnumCommunicationState.DISCONNECT;

//        public override EventCodeEnum Connect(string address, int port)
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

//            try
//            {
//                try
//                {
//                    var pingStatus = this.CommModule.PingTest(address);

//                    if (pingStatus == IPStatus.Success)
//                    {
//                        ModbusConnect(address, port);
//                    }
//                    else
//                    {
//                        //Ping Retry
//                        pingStatus = this.CommModule.PingTest(address);
//                        if (pingStatus == IPStatus.Success)
//                        {
//                            ModbusConnect(address, port);
//                        }
//                        else
//                        {
//                            retVal = EventCodeEnum.UNDEFINED;
//                        }
//                    }
//                }
//                catch (Exception err)
//                {
//                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
//                    //Ping Retry
//                    var pingStatus = this.CommModule.PingTest(address);
//                    if (pingStatus == IPStatus.Success)
//                    {
//                        ModbusConnect(address, port);
//                    }
//                    else
//                    {
//                        retVal = EventCodeEnum.UNDEFINED;
//                    }
//                }


//                void ModbusConnect(string modbusaddress, int modubsport)
//                {
//                    ModbusClient HBClient = new ModbusClient(address, port) { UnitIdentifier = 0xFF };
//                    //HBClient.Connect();
//                    HBClient.Connect(address, port);


//                    if (HBClient?.Connected == true)
//                    {
//                        this.CommModule.CommStateTransition(new ConnectState(this.CommModule) { Client = HBClient });
//                        retVal = EventCodeEnum.NONE;
//                    }
//                    else
//                    {
//                        HBClient.Connect(address, port);
//                        if (HBClient?.Connected == true)
//                        {
//                            this.CommModule.CommStateTransition(new ConnectState(this.CommModule) { Client = HBClient });
//                            retVal = EventCodeEnum.NONE;
//                        }
//                        else
//                        {
//                            retVal = EventCodeEnum.UNDEFINED;
//                        }
//                    }
//                }

//            }
//            catch (Exception err)
//            {
//                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
//                DisConnect();
//                retVal = EventCodeEnum.UNDEFINED;
//            }

//            return retVal;
//        }

//        public override void DisConnect()
//        {
//            try
//            {
//            }
//            catch (Exception)
//            {

//            }
//        }

//        public override void SetCommUnitID(byte unitID)
//        {
//            CommonMsg();
//        }

//        #region ==> Get Data from Chiller
//        //- Setpoint,temperatur controller
//        public override double GetSetTempValue(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- Internal temperature
//        public override double GetInternalTempValue(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- return temperature
//        public override double GetReturnTempVal(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- pump pressure(absolute)
//        public override int GetPumpPressureVal(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- current power
//        public override int GetCurrentPower(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- error report
//        public override int GetErrorReport(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- warning message
//        public override int GetWarningMessage(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public override double GetProcessTempVal(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public override double GetExtMoveVal(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- status of the thermostat
//        // Bit 0    : Temperature control operating mode: 1: active / 0: inactive
//        // Bit 1    : Circulation operating mode: 1: active / 0: inactive
//        // Bit 2    : Refrigerator compressor: 1: switched on / 0: switched off
//        // Bit 3    : Temperature control mode "Process control": 1: active / 0: inactive
//        // Bit 4    : Circulationg pump: 1: switched on / 0: switched off
//        // Bit 5    : Cooling power available: 1: available / 0: not available
//        // Bit 6    : Tkeylock: 1: active / 0: inactive
//        // Bit 7    : PID parameter set, temperature controller : 1: Automatic mode / 0: Expert mode
//        // Bit 8    : Error: 1: Error occurred / 0: no error
//        // Bit 9    : Warning: 1: Error occurred / 0: no error
//        // Bit 10   : Mode for presetting the internal temperature (see Address 8): 1: active / 0: inactive
//        // Bit 11   : Mode for presetting the external temperature (see Address 9): 1: active / 0: inactive
//        // Bit 12   : DV E-grade: 1: activated / 0: not activated
//        // Bit 13   : Not Using.
//        // Bit 14   : Restart electronics / Power failure (*): 1: No new start / 0: New start
//        // Bit 15   : Freeze protection (not available on all devices): 1: active / ): inactive
//        public override int GetStatusOfThermostat(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- AutoPID
//        // The value 1 means that the controller is working in automatic mode.
//        // The value 0 means that the controller is working in expert mode.
//        //  (If you would like to work in expert mode, 
//        //      the individual control parameters must be entered first.)
//        public override bool IsAutoPID(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return false;
//        }

//        //- temperature control mode
//        //  0: Temperature control mode internal
//        //  1: Temperature control mode process (cascade control)
//        public override bool IsTempControlProcessMode(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return false;
//        }

//        //- Temperature control current status.
//        //  0: Temperature control not active.
//        //  1: Temperature control active.
//        public override bool IsTempControlActive(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return false;
//        }

//        //- Process temp actual value presetting mode
//        //  프로세스 온도 실제 값 설정 모드.
//        //  Bit 0:  Operating mode active or inactive.
//        //          0: Mode not active.
//        //          1: Mode active.
//        //  Bit 1: Watchdog behavior.
//        //          0: The thermostat issues the warning -2130, deactivates the actual value presetting,
//        //          changes from process to inernal control (see vTmpMode, Variable 19) and continues to
//        //          operate temperture control.
//        //          1: The thermostat issues the error message -329 and changes to standby mode
//        //          (temperature control is Stopped).
//        public override (bool, bool) GetProcTempActValSetMode(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return (false, false);
//        }

//        public override int GetSerialNumLow(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return 0;
//        }

//        public override int GetSerialNumHigh(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return 0;
//        }

//        public override int GetSerialNumber(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return 0;
//        }

//        //start or stop circulation
//        //0: Circulation operationg mode not active.
//        //1: Circulation operationg mode active.
//        //Note: If the temperatue control is active, 
//        //  circulation is also carried out, but circulation operationg mode is not active.
//        public override bool IsCirculationActive(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return false;
//        }

//        //Activate or deactivate touch panel lock
//        //Activate, deactivate or query operating lock at Pilot.
//        //Bit 0:    Operating lock active or inactive.
//        //          0: Operating lock inactive.
//        //          1: Operating lock active. Manual operation of the thermostat via Pilot is not possible.
//        //Bit 1:    Watchdog behavior
//        //          0: Watchdog inactive.
//        //          1: Activate Watchdog for 30s. If Bit 1 is not reset within 30s, the operating lock is
//        //          automatically cancelled. This can be used to permit manual operation again if
//        //          communication with the thermostat is interrupted for any reason.
//        public override (bool, bool) IsOperatingLock(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return (false, false);
//        }

//        //- pump speed
//        public override int GetPumpSpeed(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- minimum setpoint
//        public override double GetMinSetTemp(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //- maximum setpoint
//        public override double GetMaxSetTemp(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //read setpoint pump speed
//        public override int GetTargetPumpSpeed(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return int.MinValue;
//        }

//        //RW    /Upper alram internal limit.
//        public override double GetUpperAlramInternalLimit(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return 500;
//        }

//        //RW    /Lower alram external limit.
//        public override double GetLowerAlramInternalLimit(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return -151;
//        }

//        //RW    /Upper alram process limit.
//        public override double GetUpperAlramProcessLimit(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return 500;
//        }

//        //RW    /Lower alram process limit.
//        public override double GetLowerAlramProcessLimit(byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return -151;
//        }

//        #endregion

//        #region ==> Set data to Chiller
//        public override EventCodeEnum SetTargetTemp(double sendVal, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//            return EventCodeEnum.CHILLER_SET_TARGET_TEMP_ERROR;
//        }

//        public override void SetPresetProcTempVal(double sendVal, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }

//        public override void SetAutoPIDMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }

//        public override void SetTempMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }

//        public override void SetTempActiveMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }

//        public override void SetProcTempActValSetMode(bool isOperatingMode, bool isChangeStandbyModeWhenErr329, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }


//        public override void SetMinSetTemp(double sendVal, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }


//        public override void SetMaxSetTemp(double sendVal, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }


//        public override void SetAlarmInternalUpperVal(double sendVal, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }


//        public override void SetAlarmInternalLowerVal(double sendVal, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }


//        public override void SetAlarmProcessUpperVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }

//        public override void SetAlarmProcessLowerVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }

//        //- operating lock
//        public override void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }

//        //- circuation Active
//        public override void SetCircuationActive(bool bValue, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }

//        //- read setpoint pump speed
//        public override void SetPumpSpeed(int iValue, byte subModuleIndex = 0x00)
//        {
//            CommonMsg();
//        }

//        #endregion

//        private void CommonMsg()
//        {
//            try
//            {
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.CommonMsg(): Error occurred. Err = {err.Message}");
//            }
//        }
//    }

//    public sealed class ConnectState : ChagoChillerCommState
//    {
//        private object lockObject = new object();
//        internal ModbusClient Client = null;

//        public ConnectState(ChagoChillerAdapter commModule) : base(commModule)
//        {
//        }

//        public override EnumCommunicationState GetCommunicationState(byte subModuleIndex = 0x00)
//            => EnumCommunicationState.CONNECTED;

//        public void SetChillerClient()
//        {
//        }

//        public override EventCodeEnum Connect(string address, int port)
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

//            try
//            {
//            }
//            catch (Exception)
//            {

//            }

//            return retVal;
//        }

//        public override void DisConnect()
//        {
//            try
//            {
//                Client?.Disconnect();
//                Client = null;
//                this.CommModule.CommStateTransition(new DisconnectState(this.CommModule));
//            }
//            catch (Exception err)
//            {
//                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
//            }
//        }

//        public override void SetCommUnitID(byte unitID)
//        {
//            Client.SubModuleIndex = unitID;
//            //ModuleAbortState..subModuleIndex = unitID;
//            CommModule.SubModuleIndex = unitID;
//        }

//        #region ==> Modbus Read/Write
//        //Read
//        private int[] ReadHoldingRegister(ModBusChillerEnum enumdata, byte subModuleIndex)
//        {
//            lock (lockObject)
//            {
//                try
//                {
//                    var pingTestStatus = this.CommModule.PingTest(Client?.IPAddress ?? "0");
//                    if (pingTestStatus == IPStatus.Success)
//                    {
//                        int add = (int)enumdata | (subModuleIndex << 8);
//                        var bytes = BitConverter.GetBytes(add);
//                        //LoggerManager.Debug($"");
//                        int[] result = Client?.ReadHoldingRegisters((int)add, 1) ?? null;
//                        if (result == null)
//                        {
//                            //Retry
//                            result = Client?.ReadHoldingRegisters((int)add, 1) ?? null;
//                            if (result == null)
//                                throw new Exception($"[Chiller_Modbus] ReadHoldingRegister() : Fail Get {enumdata} Data.");
//                        }
//                        return result;
//                    }
//                    else
//                    {
//                        throw new Exception($"[Chiller_Modbus] ReadHoldingRegister() : Fail Ping Test. Check connect the chiller.");
//                    }
//                }
//                catch (Exception err)
//                {
//                    throw err;
//                }
//            }
//        }

//        //Write
//        private void WriteSingleRegister(ModBusChillerEnum enumdata, int value, byte subModuleIndex)
//        {
//            lock (lockObject)
//            {
//                try
//                {
//                    var pingTestStatus = this.CommModule.PingTest(Client.IPAddress);
//                    if (pingTestStatus == IPStatus.Success)
//                    {
//                        int add = (int)enumdata | (subModuleIndex << 8);
//                        Client?.WriteSingleRegister((int)add, value);
//                    }
//                    else
//                    {
//                        throw new Exception($"[Chiller_Modbus] WriteSingleRegister() : Fail Ping Test. Check connect the chiller.");
//                    }
//                }
//                catch (Exception err)
//                {

//                    throw err;
//                }
//            }
//        }

//        private void WriteSingleRegister(int data, int value, byte subModuleIndex)
//        {
//            lock (lockObject)
//            {
//                try
//                {
//                    var pingTestStatus = this.CommModule.PingTest(Client.IPAddress);
//                    if (pingTestStatus == IPStatus.Success)
//                    {
//                        int add = (int)data | (subModuleIndex << 8);
//                        Client?.WriteSingleRegister(add, value);
//                    }
//                    else
//                    {
//                        throw new Exception($"[Chiller_Modbus] WriteSingleRegister() : Fail Ping Test. Check connect the chiller.");
//                    }
//                }
//                catch (Exception err)
//                {

//                    throw err;
//                }
//            }
//        }

//        private void WriteMuliteRegister(ModBusChillerEnum enumdata, int[] value, byte subModuleIndex)
//        {
//            lock (lockObject)
//            {
//                try
//                {
//                    var pingTestStatus = this.CommModule.PingTest(Client.IPAddress);
//                    if (pingTestStatus == IPStatus.Success)
//                    {
//                        int add = (int)enumdata | (subModuleIndex << 8);
//                        Client?.WriteMultipleRegisters((int)add, value);
//                    }
//                    else
//                    {
//                        throw new Exception($"[Chiller_Modbus] WriteMuliteRegister() : Fail Ping Test. Check connect the chiller.");
//                    }

//                }
//                catch (Exception err)
//                {
//                    throw err;
//                }
//            }
//        }
//        #endregion

//        #region ==> Get Data from Chiller
//        //- Setpoint,temperatur controller
//        public override double GetSetTempValue(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;
//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.SET_TEMP, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {
//                //Ping fail or Ping throw exception
//                LoggerManager.Error($"Chiller.GetSetTempValue(): Error occurred. Err = {err.Message}");
//            }

//            return (double)retValue * 0.01;
//        }

//        //- Internal temperature
//        public override double GetInternalTempValue(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.PROCESS_TEMP, subModuleIndex);
//                //tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.INTERNAL_TEMP);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetInternalTempValue(): Error occurred. Err = {err.Message}");
//            }

//            return (double)retValue * 0.01;
//        }

//        //- return temperature
//        public override double GetReturnTempVal(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.RETURN_TEMP, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetReturnTempVal(): Error occurred. Err = {err.Message}");
//            }

//            return (double)retValue * 0.01;
//        }

//        //- pump pressure(absolute)
//        public override int GetPumpPressureVal(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.PUMP_PRESSURE, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetPumpPressureVal(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //- current power
//        public override int GetCurrentPower(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.CUR_POWER, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetCurrentPower(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //- error report
//        public override int GetErrorReport(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ERROR, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetErrorReport(): Error occurred. Err = {err.Message}");

//            }

//            return retValue;
//        }

//        //- warning message
//        public override int GetWarningMessage(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.WARNING, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetWarningMessage(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public override double GetProcessTempVal(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.PROCESS_TEMP, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetProcessTempVal(): Error occurred. Err = {err.Message}");
//            }

//            return (double)retValue * 0.01;
//        }

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public override double GetExtMoveVal(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.EXTMOVE, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetExtMoveVal(): Error occurred. Err = {err.Message}");
//            }

//            return (double)retValue;
//        }

//        //- status of the thermostat
//        // Bit 0    : Temperature control operating mode: 1: active / 0: inactive
//        // Bit 1    : Circulation operating mode: 1: active / 0: inactive
//        // Bit 2    : Refrigerator compressor: 1: switched on / 0: switched off
//        // Bit 3    : Temperature control mode "Process control": 1: active / 0: inactive

//        // Bit 4    : Circulationg pump: 1: switched on / 0: switched off
//        // Bit 5    : Cooling power available: 1: available / 0: not available
//        // Bit 6    : Tkeylock: 1: active / 0: inactive
//        // Bit 7    : PID parameter set, temperature controller : 1: Automatic mode / 0: Expert mode

//        // Bit 8    : Error: 1: Error occurred / 0: no error
//        // Bit 9    : Warning: 1: Error occurred / 0: no error
//        // Bit 10   : Mode for presetting the internal temperature (see Address 8): 1: active / 0: inactive
//        // Bit 11   : Mode for presetting the external temperature (see Address 9): 1: active / 0: inactive

//        // Bit 12   : DV E-grade: 1: activated / 0: not activated
//        // Bit 13   : Not Using.
//        // Bit 14   : Restart electronics / Power failure (*): 1: No new start / 0: New start
//        // Bit 15   : Freeze protection (not available on all devices): 1: active / ): inactive
//        public override int GetStatusOfThermostat(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.STATUS1, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetStatusOfThermostat(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //- AutoPID
//        // The value 1 means that the controller is working in automatic mode.
//        // The value 0 means that the controller is working in expert mode.
//        //  (If you would like to work in expert mode, 
//        //      the individual control parameters must be entered first.)
//        public override bool IsAutoPID(byte subModuleIndex = 0x00)
//        {
//            bool retValue = false;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.AUTO_PID, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    if (tmpSetTempValue[0] == 0)
//                        retValue = false;
//                    else if (tmpSetTempValue[0] == 1)
//                        retValue = true;
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.IsAutoPID(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //- temperature control mode
//        //  0: Temperature control mode internal
//        //  1: Temperature control mode process (cascade control)
//        public override bool IsTempControlProcessMode(byte subModuleIndex = 0x00)
//        {
//            bool retValue = false;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.TMP_MODE, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    if (tmpSetTempValue[0] == 0)
//                        retValue = false;
//                    else if (tmpSetTempValue[0] == 1)
//                        retValue = true;
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.IsTempControlProcessMode(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //- Temperature control current status.
//        //  0: Temperature control not active.
//        //  1: Temperature control active.
//        public override bool IsTempControlActive(byte subModuleIndex = 0x00)
//        {
//            bool retValue = false;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.TMP_ACTIVE, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    if (tmpSetTempValue[0] == 0)
//                        retValue = false;
//                    else if (tmpSetTempValue[0] == 1)
//                        retValue = true;
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.IsTempControlActive(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //- Process temp actual value presetting mode
//        //  프로세스 온도 실제 값 설정 모드.
//        //  Bit 0:  Operating mode active or inactive.
//        //          0: Mode not active.
//        //          1: Mode active.
//        //  Bit 1: Watchdog behavior.
//        //          0: The thermostat issues the warning -2130, deactivates the actual value presetting,
//        //          changes from process to inernal control (see vTmpMode, Variable 19) and continues to
//        //          operate temperture control.
//        //          1: The thermostat issues the error message -329 and changes to standby mode
//        //          (temperature control is Stopped).
//        public override (bool, bool) GetProcTempActValSetMode(byte subModuleIndex = 0x00)
//        {
//            bool isOperatingMode = false;
//            bool isChangeStandbyModeWhenErr329 = false;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.PROC_TEMP_PRESET, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    if ((tmpSetTempValue[0] & 0x01) == 0x00)
//                    {
//                        isOperatingMode = false;
//                    }
//                    else if ((tmpSetTempValue[0] & 0x01) == 0x01)
//                    {
//                        isOperatingMode = true;
//                    }

//                    if ((tmpSetTempValue[0] & 0x10) == 0x00)
//                    {
//                        isChangeStandbyModeWhenErr329 = true;
//                    }
//                    else if ((tmpSetTempValue[0] & 0x10) == 0x10)
//                    {
//                        isChangeStandbyModeWhenErr329 = false;
//                    }
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetProcTempActValSetMode(): Error occurred. Err = {err.Message}");
//            }

//            return (isOperatingMode, isChangeStandbyModeWhenErr329);
//        }

//        public override int GetSerialNumLow(byte subModuleIndex = 0x00)
//        {
//            int retValue = 0;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.SERIAL_NUMBER_L, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetSerialNumLow(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        public override int GetSerialNumHigh(byte subModuleIndex = 0x00)
//        {
//            int retValue = 0;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.SERIAL_NUMBER_H, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetSerialNumHigh(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        public override int GetSerialNumber(byte subModuleIndex = 0x00)
//        {
//            int retValue = 0;
//            int lowNum = 0;
//            int highNum = 0;
//            try
//            {
//                lowNum = GetSerialNumLow();
//                lowNum = (lowNum & 0xFFFF);
//                highNum = GetSerialNumHigh();
//                highNum = (highNum & 0xFFFF) << 16;

//                retValue = lowNum + highNum;
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//            }
//            return retValue;
//        }

//        //start or stop circulation
//        //0: Circulation operationg mode not active.
//        //1: Circulation operationg mode active.
//        //Note: If the temperatue control is active, 
//        //  circulation is also carried out, but circulation operationg mode is not active.
//        public override bool IsCirculationActive(byte subModuleIndex = 0x00)
//        {
//            bool retValue = false;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.CIRC_ACTIVE, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    if (tmpSetTempValue[0] == 0)
//                        retValue = false;
//                    else if (tmpSetTempValue[0] == 1)
//                        retValue = true;
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.IsCirculationActive(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //Activate or deactivate touch panel lock
//        //Activate, deactivate or query operating lock at Pilot.
//        //Bit 0:    Operating lock active or inactive.
//        //          0: Operating lock inactive.
//        //          1: Operating lock active. Manual operation of the thermostat via Pilot is not possible.
//        //Bit 1:    Watchdog behavior
//        //          0: Watchdog inactive.
//        //          1: Activate Watchdog for 30s. If Bit 1 is not reset within 30s, the operating lock is
//        //          automatically cancelled. This can be used to permit manual operation again if
//        //          communication with the thermostat is interrupted for any reason.
//        public override (bool, bool) IsOperatingLock(byte subModuleIndex = 0x00)
//        {
//            bool bOperatinglock = false;
//            bool bWatchdogBehavior = false;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.OPERATING_LOCK, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    if ((tmpSetTempValue[0] & 0x01) == 0x00)
//                    {
//                        bOperatinglock = false;
//                    }
//                    else if ((tmpSetTempValue[0] & 0x01) == 0x01)
//                    {
//                        bOperatinglock = true;
//                    }

//                    if ((tmpSetTempValue[0] & 0x10) == 0x00)
//                    {
//                        bWatchdogBehavior = true;
//                    }
//                    else if ((tmpSetTempValue[0] & 0x10) == 0x10)
//                    {
//                        bWatchdogBehavior = false;
//                    }
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.IsOperatingLock(): Error occurred. Err = {err.Message}");
//            }

//            return (bOperatinglock, bWatchdogBehavior);
//        }

//        //- pump speed
//        public override int GetPumpSpeed(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.PUMP_SPEED, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetPumpSpeed(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //- minimum setpoint
//        public override double GetMinSetTemp(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.MIN_SET_TEMP, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetMinSetTemp(): Error occurred. Err = {err.Message}");
//            }

//            return (double)retValue * 0.01;
//        }

//        //- maximum setpoint
//        public override double GetMaxSetTemp(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.MAX_SET_POINT, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetMaxSetTemp(): Error occurred. Err = {err.Message}");
//            }

//            return (double)retValue * 0.01;
//        }

//        //read setpoint pump speed
//        public override int GetTargetPumpSpeed(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.SET_TEMP_PUMP_SPEED, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetTargetPumpSpeed(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //RW    /Upper alram internal limit.
//        public override double GetUpperAlramInternalLimit(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ALARAM_LIMIT_INTERNAL_UPPER, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetUpperAlramInternalLimit(): Error occurred. Err = {err.Message}");
//            }

//            return retValue * 0.01;
//        }

//        //RW    /Lower alram external limit.
//        public override double GetLowerAlramInternalLimit(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ALARAM_LIMIT_INTERNAL_LOWER, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetLowerAlramInternalLimit(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //RW    /Upper alram process limit.
//        public override double GetUpperAlramProcessLimit(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ALARAM_LIMIT_PROCESS_UPPER, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetUpperAlramProcessLimit(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        //RW    /Lower alram process limit.
//        public override double GetLowerAlramProcessLimit(byte subModuleIndex = 0x00)
//        {
//            int retValue = int.MinValue;
//            int[] tmpSetTempValue = null;

//            try
//            {
//                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ALARAM_LIMIT_PROCESS_LOWER, subModuleIndex);

//                if (tmpSetTempValue != null)
//                {
//                    retValue = (short)tmpSetTempValue[0];
//                }
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.GetLowerAlramProcessLimit(): Error occurred. Err = {err.Message}");
//            }

//            return retValue;
//        }

//        #endregion

//        #region ==> Set data to Chiller
//        /// <summary>
//        /// Temp in 0.01 scale.
//        /// Ex. 10.00℃ -> 1000
//        /// </summary>
//        /// <param name="sendVal"></param>
//        public override EventCodeEnum SetTargetTemp(double sendVal, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int tempData = (int)(sendVal * 100.0);
//                WriteSingleRegister(ModBusChillerEnum.SET_TEMP, tempData, subModuleIndex);
//                return EventCodeEnum.NONE;
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Error($"Chiller.SetTargetTemp({sendVal}): Error occurred. Err = {err.Message}");
//                return EventCodeEnum.CHILLER_SET_TARGET_TEMP_ERROR;
//            }
//        }
//        /// <summary>
//        /// Temp in 0.01 scale.
//        /// Ex. 10.00℃ -> 1000
//        /// </summary>
//        /// <param name="sendVal"></param>
//        public override void SetPresetProcTempVal(double sendVal, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int tempData = (int)(sendVal * 100.0);
//                WriteSingleRegister(ModBusChillerEnum.EXTMOVE, tempData, subModuleIndex);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Error($"Chiller.SetPresetProcTempVal({sendVal}): Error occurred. Err = {err.Message}");
//            }
//        }

//        public override void SetAutoPIDMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                WriteSingleRegister(ModBusChillerEnum.AUTO_PID, (bValue ? 1 : 0), subModuleIndex);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Error($"Chiller.SetAutoPIDMode({bValue}): Error occurred. Err = {err.Message}");
//            }
//        }

//        public override void SetTempMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                WriteSingleRegister(ModBusChillerEnum.TMP_MODE, (bValue ? 1 : 0), subModuleIndex);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Error($"Chiller.SetTempMode({bValue}): Error occurred. Err = {err.Message}");
//            }
//        }

//        public override void SetTempActiveMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                WriteSingleRegister(ModBusChillerEnum.TMP_ACTIVE, (bValue ? 1 : 0), subModuleIndex);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Error($"Chiller.SetTempActiveMode({bValue}): Error occurred. Err = {err.Message}");
//            }
//        }

//        public override void SetProcTempActValSetMode(bool isOperatingMode, bool isChangeStandbyModeWhenErr329, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int sendData = 0;
//                sendData = Convert.ToInt32(isOperatingMode) + (Convert.ToInt32(isChangeStandbyModeWhenErr329) * 2);

//                WriteSingleRegister(ModBusChillerEnum.TMP_ACTIVE, sendData, subModuleIndex);
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.SetProcTempActValSetMode({isOperatingMode}, {isChangeStandbyModeWhenErr329}): Error occurred. Err = {err.Message}");
//            }
//        }

//        public override void SetMinSetTemp(double sendVal, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int tempData = (int)(sendVal * 100.0);
//                WriteSingleRegister(ModBusChillerEnum.MIN_SET_TEMP, tempData, subModuleIndex);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Error($"Chiller.SetMinSetTemp({sendVal}): Error occurred. Err = {err.Message}");
//            }
//        }


//        public override void SetMaxSetTemp(double sendVal, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int tempData = (int)(sendVal * 100.0);
//                WriteSingleRegister(ModBusChillerEnum.MAX_SET_POINT, tempData, subModuleIndex);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Error($"Chiller.SetMaxSetTemp({sendVal}): Error occurred. Err = {err.Message}");
//            }
//        }


//        public override void SetAlarmInternalUpperVal(double sendVal, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int tempData = (int)(sendVal * 100.0);
//                WriteSingleRegister(ModBusChillerEnum.ALARAM_LIMIT_INTERNAL_UPPER, tempData, subModuleIndex);
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.SetAlarmInternalUpperVal({sendVal}): Error occurred. Err = {err.Message}");
//            }
//        }


//        public override void SetAlarmInternalLowerVal(double sendVal, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int tempData = (int)(sendVal * 100.0);
//                WriteSingleRegister(ModBusChillerEnum.ALARAM_LIMIT_INTERNAL_LOWER, tempData, subModuleIndex);
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.SetAlarmInternalLowerVal({sendVal}): Error occurred. Err = {err.Message}");
//            }
//        }

//        public override void SetAlarmProcessUpperVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int tempData = (int)(sendVal * 100.0);
//                WriteSingleRegister(ModBusChillerEnum.ALARAM_LIMIT_PROCESS_UPPER, tempData, subModuleIndex);
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.SetAlarmProcessUpperVal({sendVal}): Error occurred. Err = {err.Message}");
//            }
//        }

//        public override void SetAlarmProcessLowerVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int tempData = (int)(sendVal * 100.0);
//                WriteSingleRegister(ModBusChillerEnum.ALARAM_LIMIT_PROCESS_LOWER, tempData, subModuleIndex);
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.SetAlarmProcessLowerVal({sendVal}): Error occurred. Err = {err.Message}");
//            }
//        }

//        //- operating lock
//        public override void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                int sendData = 0;
//                sendData = Convert.ToInt32(bOperatinglock) + (Convert.ToInt32(bWatchdogBehavior) * 2);

//                WriteSingleRegister(ModBusChillerEnum.OPERATING_LOCK, sendData, subModuleIndex);
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.SetOperatingLock({bOperatinglock}, {bWatchdogBehavior}): Error occurred. Err = {err.Message}");
//            }
//        }

//        //- circuation Active
//        public override void SetCircuationActive(bool bValue, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                WriteSingleRegister(ModBusChillerEnum.CIRC_ACTIVE, (bValue ? 1 : 0), subModuleIndex);
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.SetCircuationActive({bValue}): Error occurred. Err = {err.Message}");
//            }
//        }

//        /// <summary>
//        /// read setpoint pump speed
//        /// Value range in 0 ~ 32000 mbar
//        /// 
//        /// </summary>
//        /// <param name="iValue"></param>
//        public override void SetPumpSpeed(int iValue, byte subModuleIndex = 0x00)
//        {
//            try
//            {
//                WriteSingleRegister(ModBusChillerEnum.SET_TEMP_PUMP_SPEED, iValue, subModuleIndex);
//            }
//            catch (Exception err)
//            {

//                LoggerManager.Error($"Chiller.SetPumpSpeed({iValue}): Error occurred. Err = {err.Message}");
//            }
//        }

//        #endregion
//    }
//}

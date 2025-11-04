// 현재 사용하지 않아 주석 처리 해놓음 - 2023/05/16
//using System;

//namespace ChillerModule.Chago
//{
//    using LogModule;
//    using ProberErrorCode;
//    using ProberInterfaces;
//    using ProberInterfaces.Enum;
//    using ProberInterfaces.Temperature;
//    using ProberInterfaces.Temperature.Chiller;
//    using System.ComponentModel;
//    using System.Runtime.CompilerServices;

//    public partial class ChagoChillerComm: IChillerComm, INotifyPropertyChanged
//    {
//        #region <!-- NotifyPropertyChanged -->
//        public event PropertyChangedEventHandler PropertyChanged;
//        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
//            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
//        #endregion

//        public bool Initialized { get; set; }
//        private bool IsDisposed = false;

//        private ChagoChillerAdapter _ChillerAdapter { get; set; }
//        private byte _ChillerSubIndex; 
//        public ChagoChillerComm(IChillerAdapter adapter, byte subIndex)
//        {
//            _ChillerAdapter = adapter as ChagoChillerAdapter;
//            _ChillerSubIndex = subIndex;
//        }

//        ~ChagoChillerComm()
//        {

//        }

//        private ChagoChillerAdapter GetChillerAdapter()
//        {
//            try
//            {
//                //_ChillerAdapter?.SetCommUnitID(_ChillerSubIndex);
//                return _ChillerAdapter;
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw err;
//            }
//        }

//        public void Dispose()
//        {
//            try
//            {
//                if (!IsDisposed)
//                {
//                    _ChillerAdapter.DisConnect();
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
//                    //ChillerAdapter?.DisConnect();
//                    //ChillerAdapter = new DisconnectState(this);
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
//            => this._ChillerAdapter?.GetCommState(_ChillerSubIndex) ?? EnumCommunicationState.DISCONNECT;

//        public EventCodeEnum Connect(string address, int port)
//            => this?._ChillerAdapter.Connect(address, port)
//            ?? EventCodeEnum.UNDEFINED;

//        public void DisConnect()
//            => this?._ChillerAdapter.DisConnect();

//        public ICommunicationMeans GetCommunicationObj()
//        {
//            ICommunicationMeans obj = null;
//            try
//            {
//                if (GetCommState() == EnumCommunicationState.CONNECTED)
//                {
//                    obj = (this._ChillerAdapter.ChillerCommState as ConnectState)?.Client;
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
//            double retVal = GetChillerAdapter().GetSetTempValue(_ChillerSubIndex);
//            return (double)retVal;
//        }
//        /// <summary>
//        /// Internal temperature
//        /// Value in 0.01℃ unit. 1000 -> 10.0℃
//        /// </summary>
//        /// <returns></returns>

//        public double GetInternalTempValue(byte subModuleIndex = 0x00)
//        {
//            double retVal = GetChillerAdapter().GetInternalTempValue(_ChillerSubIndex);
//            return (double)retVal;
//        }

//        //- return temperature
//        public double GetReturnTempVal(byte subModuleIndex = 0x00)
//        {
//            double retVal = GetChillerAdapter().GetReturnTempVal(_ChillerSubIndex);
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
//            retVal = GetChillerAdapter().GetPumpPressureVal(_ChillerSubIndex);
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
//            retVal = GetChillerAdapter().GetCurrentPower(_ChillerSubIndex);
//            return retVal;
//        }

//        //- error report
//        public int GetErrorReport(byte subModuleIndex = 0x00)
//        {
//            int retVal;
//            retVal = GetChillerAdapter().GetErrorReport(_ChillerSubIndex);
//            return retVal;
//        }

//        //- warning message
//        public int GetWarningMessage(byte subModuleIndex = 0x00)
//        {
//            int retVal;
//            retVal = GetChillerAdapter().GetWarningMessage(_ChillerSubIndex);
//            return retVal;
//        }

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public double GetProcessTempVal(byte subModuleIndex = 0x00)
//        {
//            double retVal = GetChillerAdapter().GetProcessTempVal(_ChillerSubIndex);
//            return (double)retVal * 0.01;
//        }

//        //- Process temperature (lemosa sensor )
//        //- If no sensor is connected, the value -151°C is returned.
//        public double GetExtMoveVal(byte subModuleIndex = 0x00)
//        {
//            double retVal = GetChillerAdapter().GetExtMoveVal(_ChillerSubIndex);
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
//            retVal = GetChillerAdapter().GetStatusOfThermostat(_ChillerSubIndex);
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
//            retVal = GetChillerAdapter().IsAutoPID(_ChillerSubIndex);
//            return retVal;
//        }

//        //- temperature control mode
//        //  0: Temperature control mode internal
//        //  1: Temperature control mode process (cascade control)
//        public bool IsTempControlProcessMode(byte subModuleIndex = 0x00)
//        {
//            bool retVal;
//            retVal = GetChillerAdapter().IsTempControlProcessMode(_ChillerSubIndex);
//            return retVal;
//        }

//        //- Temperature control current status.
//        //  0: Temperature control not active.
//        //  1: Temperature control active.
//        public bool IsTempControlActive(byte subModuleIndex = 0x00)
//        {
//            bool retVal;
//            retVal = GetChillerAdapter().IsTempControlActive(_ChillerSubIndex);
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
//            retVal = GetChillerAdapter().GetProcTempActValSetMode(_ChillerSubIndex);
//            return retVal;
//        }

//        public int GetSerialNumLow(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = GetChillerAdapter().GetSerialNumLow(_ChillerSubIndex);
//            return retVal;
//        }

//        public int GetSerialNumHigh(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = GetChillerAdapter().GetSerialNumHigh(_ChillerSubIndex);
//            return retVal;
//        }

//        public int GetSerialNumber(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = GetChillerAdapter().GetSerialNumber(_ChillerSubIndex);
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
//            retVal = GetChillerAdapter().IsCirculationActive(_ChillerSubIndex);
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
//            retVal = GetChillerAdapter().IsOperatingLock(_ChillerSubIndex);
//            return retVal;
//        }

//        //- pump speed
//        public int GetPumpSpeed(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = GetChillerAdapter().GetPumpSpeed(_ChillerSubIndex);
//            return retVal;
//        }

//        //- minimum setpoint
//        public double GetMinSetTemp(byte subModuleIndex = 0x00)
//        {
//            double retVal = GetChillerAdapter().GetMinSetTemp(_ChillerSubIndex);
//            return (double)retVal;
//        }

//        //- maximum setpoint
//        public double GetMaxSetTemp(byte subModuleIndex = 0x00)
//        {
//            double retVal = GetChillerAdapter().GetMaxSetTemp(_ChillerSubIndex);
//            return (double)retVal;
//        }

//        //read setpoint pump speed
//        public int GetSetTempPumpSpeed(byte subModuleIndex = 0x00)
//        {
//            int retVal = 0;
//            retVal = GetChillerAdapter().GetSetTempPumpSpeed(_ChillerSubIndex);
//            return retVal;
//        }

//        //RW    /Upper alram internal limit.
//        public double GetUpperAlramInternalLimit(byte subModuleIndex = 0x00)
//        {
//            double retVal = 0;
//            retVal = GetChillerAdapter().GetUpperAlramInternalLimit(_ChillerSubIndex);
//            return retVal;
//        }

//        //RW    /Lower alram external limit.
//        public double GetLowerAlramInternalLimit(byte subModuleIndex = 0x00)
//        {
//            double retVal = 0;
//            retVal = GetChillerAdapter().GetLowerAlramInternalLimit(_ChillerSubIndex);
//            return retVal;
//        }

//        //RW    /Upper alram process limit.
//        public double GetUpperAlramProcessLimit(byte subModuleIndex = 0x00)
//        {
//            double retVal = 0;
//            retVal = GetChillerAdapter().GetUpperAlramProcessLimit(_ChillerSubIndex);
//            return retVal;
//        }

//        //RW    /Lower alram process limit.
//        public double GetLowerAlramProcessLimit(byte subModuleIndex = 0x00)
//        {
//            double retVal = 0;
//            retVal = GetChillerAdapter().GetLowerAlramProcessLimit(_ChillerSubIndex);
//            return retVal;
//        }

//        #endregion

//        #region ==> Set data to Chiller


//        public EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, byte subModuleIndex = 0x00)
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                retVal = GetChillerAdapter().SetTargetTemp((int)sendVal, sendTempValueType, _ChillerSubIndex);
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
//            GetChillerAdapter().SetPresetProcTempVal(sendVal, _ChillerSubIndex);
//        }

//        public void SetAutoPIDMode(bool bValue)
//        {
//            GetChillerAdapter().SetAutoPIDMode(bValue, _ChillerSubIndex);
//        }

//        public void SetTempMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetTempMode(bValue, _ChillerSubIndex);
//        }

//        public void SetTempActiveMode(bool bValue, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetTempActiveMode(bValue, _ChillerSubIndex);
//        }

//        public void SetProcTempActValSetMode(bool isOperatingMode, bool isChangeStandbyModeWhenErr329, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetProcTempActValSetMode(isOperatingMode, isChangeStandbyModeWhenErr329, _ChillerSubIndex);
//        }


//        public void SetMinSetTemp(int sendVal, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetMinSetTemp(sendVal, _ChillerSubIndex);
//        }


//        public void SetMaxSetTemp(int sendVal, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetMaxSetTemp(sendVal, _ChillerSubIndex);
//        }


//        public void SetAlarmInternalUpperVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetAlarmInternalUpperVal(sendVal, _ChillerSubIndex);
//        }


//        public void SetAlarmInternalLowerVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetAlarmInternalLowerVal(sendVal, _ChillerSubIndex);
//        }


//        public void SetAlarmProcessUpperVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetAlarmProcessUpperVal(sendVal, _ChillerSubIndex);
//        }

//        public void SetAlarmProcessLowerVal(int sendVal, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetAlarmProcessLowerVal(sendVal, _ChillerSubIndex);
//        }

//        //- operating lock
//        public void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetOperatingLock(bOperatinglock, bWatchdogBehavior, _ChillerSubIndex);
//        }

//        //- circuation Active
//        public void SetCircuationActive(bool bValue, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetCircuationActive(bValue, _ChillerSubIndex);
//        }

//        //- read setpoint pump speed
//        public void SetSetTempPumpSpeed(int iValue, byte subModuleIndex = 0x00)
//        {
//            GetChillerAdapter().SetSetTempPumpSpeed(iValue, _ChillerSubIndex);
//        }
//        #endregion
//    }

//}

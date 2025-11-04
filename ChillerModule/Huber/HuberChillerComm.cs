using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Temperature;
using ProberInterfaces.Temperature.Chiller;
using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace Temperature.Temp.Chiller
{
    public partial class HuberChillerComm : IChillerComm, INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        public HcCommState ChillerCommState = null;
        public bool Initialized { get; set; }
        private bool IsDisposed = false;
        private Ping ping = new Ping();
        public ChillerModule Module { get; set; }
        private int ChillerIndeex { get; set; }
        public HuberChillerComm(ChillerModule module)
        {
            Module = module;
            ChillerIndeex = Module?.ChillerInfo?.Index ?? -1;
        }

        ~HuberChillerComm()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                if (!IsDisposed)
                {
                    ChillerCommState.DisConnect();
                    IsDisposed = true;
                }
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.Dispose() Error occurred. Err = {err.Message}");
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!Initialized)
                {
                    ChillerCommState?.DisConnect();
                    ChillerCommState = new HcDisconnectState(this);
                    Initialized = true;
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;

            }

            return retVal;
        }

        public void DeInitModule()
        {
            Dispose();
        }

        public EnumCommunicationState GetCommState(byte subModuleIndex = 0x00)
            => this.ChillerCommState?.GetCommunicationState() ?? EnumCommunicationState.DISCONNECT;
        public ICommunicationMeans GetCommunicationObj()
        {
            ICommunicationMeans obj = null;
            try
            {
                if (GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    obj = (this.ChillerCommState as HcConnectState)?.Client;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return obj;
        }

        public object GetCommLockObj()
        {
            object lockObj = null;
            try
            {
                if (GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    lockObj = (this.ChillerCommState as HcConnectState)?.lockObject;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return lockObj;
        }
        public void CommStateTransition(HcCommState state)
        {
            if (ChillerCommState?.GetType() != state?.GetType())
            {
                LoggerManager.Debug($"[Chiller #{ChillerIndeex}] CommStateTransition(): change {ChillerCommState?.GetType().Name} to {state?.GetType().Name}");
                this.ChillerCommState = state;
            }
        }

        public EventCodeEnum Connect(string address, int port)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PingTimeOut = this.Module.EnvControlManager().ChillerManager.GetPingTimeOut();
                retVal = this.ChillerCommState.Connect(address, port);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private int PingTimeOut { get; set; }
        public IPStatus PingTest(string address)
        {
            IPStatus status = IPStatus.Unknown;
            try
            {
                if (PingTimeOut == 0)
                {
                    status = this.ping.Send(address)?.Status ?? IPStatus.Unknown;
                    if (status != IPStatus.Success)
                    {
                        int retryCount = 3;
                        for (int count = 0; count < retryCount; count++)
                        {
                            status = this.ping.Send(address)?.Status ?? IPStatus.Unknown;
                            LoggerManager.Debug($"Chiller PingTest Retry . retry count : {count + 1}, address : {address}");
                            if (status == IPStatus.Success)
                            {
                                LoggerManager.Debug($"Chiller PingTest suceess. address : {address}");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    status = this.ping.Send(address, PingTimeOut)?.Status ?? IPStatus.Unknown;
                    if (status != IPStatus.Success)
                    {
                        int retryCount = 3;
                        for (int count = 0; count < retryCount; count++)
                        {
                            status = this.ping.Send(address, PingTimeOut)?.Status ?? IPStatus.Unknown;
                            LoggerManager.Debug($"Chiller PingTest Retry . retry count : {count + 1}, address : {address}");
                            if (status == IPStatus.Success)
                            {
                                LoggerManager.Debug($"Chiller PingTest suceess. address : {address}");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return status;
        }

        public void DisConnect()
            => this?.ChillerCommState.DisConnect();

        public EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        #region ==> Get Data from Chiller
        /// <summary>
        /// Value in 0.01�� unit. 1000 -> 10.0��   (-21474836.48)
        /// </summary>
        /// <returns></returns>
        public double GetSetTempValue(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetSetTempValue();
            }
            catch (Exception err)
            {
                throw err;
            }
            return (double)retVal;
        }
        /// <summary>
        /// Internal temperature
        /// Value in 0.01�� unit. 1000 -> 10.0��
        /// </summary>
        /// <returns></returns>

        public double GetInternalTempValue(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetInternalTempValue();
            }
            catch (Exception err)
            {
                throw err;
            }
            return (double)retVal;
        }

        //- return temperature
        public double GetReturnTempVal(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetReturnTempVal();
            }
            catch (Exception err)
            {
                throw err;
            }
            return (double)retVal;
        }
        /// <summary>
        /// Value in mbar
        /// pump pressure(absolute)
        /// </summary>
        /// <returns></returns>
        public int GetPumpPressureVal(byte subModuleIndex = 0x00)
        {
            int retVal;
            try
            {
                retVal = ChillerCommState.GetPumpPressureVal();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        /// <summary>
        /// Power consumption of chiller in W unit.
        /// Negative value for cold operation to -32767, 
        /// Positive value for heating operation to 32767.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentPower(byte subModuleIndex = 0x00)
        {
            int retVal;
            try
            {
                retVal = ChillerCommState.GetCurrentPower();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //- error report
        public int GetErrorReport(byte subModuleIndex = 0x00)
        {
            int retVal;
            try
            {
                retVal = ChillerCommState.GetErrorReport();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //- warning message
        public int GetWarningMessage(byte subModuleIndex = 0x00)
        {
            int retVal;
            try
            {
                retVal = ChillerCommState.GetWarningMessage();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //- Process temperature (lemosa sensor )
        //- If no sensor is connected, the value -151��C is returned.
        public double GetProcessTempVal(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetProcessTempVal();
            }
            catch (Exception err)
            {
                throw err;
            }
            return (double)retVal * 0.01;
        }

        //- Process temperature (lemosa sensor )
        //- If no sensor is connected, the value -151��C is returned.
        public double GetExtMoveVal(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetExtMoveVal();
            }
            catch (Exception err)
            {
                throw err;
            }
            return (double)retVal;
        }

        //- status of the thermostat
        // Bit 0    : Temperature control operating mode: 1: active / 0: inactive
        // Bit 1    : Circulation operating mode: 1: active / 0: inactive
        // Bit 2    : Refrigerator compressor: 1: switched on / 0: switched off
        // Bit 3    : Temperature control mode "Process control": 1: active / 0: inactive
        // Bit 4    : Circulationg pump: 1: switched on / 0: switched off
        // Bit 5    : Cooling power available: 1: available / 0: not available
        // Bit 6    : Tkeylock: 1: active / 0: inactive
        // Bit 7    : PID parameter set, temperature controller : 1: Automatic mode / 0: Expert mode
        // Bit 8    : Error: 1: Error occurred / 0: no error
        // Bit 9    : Warning: 1: Error occurred / 0: no error
        // Bit 10   : Mode for presetting the internal temperature (see Address 8): 1: active / 0: inactive
        // Bit 11   : Mode for presetting the external temperature (see Address 9): 1: active / 0: inactive
        // Bit 12   : DV E-grade: 1: activated / 0: not activated
        // Bit 13   : Not Using.
        // Bit 14   : Restart electronics / Power failure (*): 1: No new start / 0: New start
        // Bit 15   : Freeze protection (not available on all devices): 1: active / ): inactive
        public int GetStatusOfThermostat(byte subModuleIndex = 0x00)
        {
            int retVal;
            try
            {
                retVal = ChillerCommState.GetStatusOfThermostat();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //- AutoPID
        // The value 1 means that the controller is working in automatic mode.
        // The value 0 means that the controller is working in expert mode.
        //  (If you would like to work in expert mode, 
        //      the individual control parameters must be entered first.)
        public bool IsAutoPID(byte subModuleIndex = 0x00)
        {
            bool retVal;
            try
            {
                retVal = ChillerCommState.IsAutoPID();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //- temperature control mode
        //  0: Temperature control mode internal
        //  1: Temperature control mode process (cascade control)
        public bool IsTempControlProcessMode(byte subModuleIndex = 0x00)
        {
            bool retVal;
            try
            {
                retVal = ChillerCommState.IsTempControlProcessMode();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //- Temperature control current status.
        //  0: Temperature control not active.
        //  1: Temperature control active.
        public bool IsTempControlActive(byte subModuleIndex = 0x00)
        {
            bool retVal;
            try
            {
                retVal = ChillerCommState.IsTempControlActive();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //- Process temp actual value presetting mode
        //  ���μ��� �µ� ���� �� ���� ���.
        //  Bit 0:  Operating mode active or inactive.
        //          0: Mode not active.
        //          1: Mode active.
        //  Bit 1: Watchdog behavior.
        //          0: The thermostat issues the warning -2130, deactivates the actual value presetting,
        //          changes from process to inernal control (see vTmpMode, Variable 19) and continues to
        //          operate temperture control.
        //          1: The thermostat issues the error message -329 and changes to standby mode
        //          (temperature control is Stopped).
        public (bool, bool) GetProcTempActValSetMode(byte subModuleIndex = 0x00)
        {
            (bool, bool) retVal;
            try
            {
                retVal = ChillerCommState.GetProcTempActValSetMode();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public int GetSerialNumLow(byte subModuleIndex = 0x00)
        {
            int retVal = 0;
            try
            {
                retVal = ChillerCommState.GetSerialNumLow();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public int GetSerialNumHigh(byte subModuleIndex = 0x00)
        {
            int retVal = 0;
            try
            {
                retVal = ChillerCommState.GetSerialNumHigh();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public int GetSerialNumber(byte subModuleIndex = 0x00)
        {
            int retVal = 0;
            try
            {
                retVal = ChillerCommState.GetSerialNumber();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public bool Prev_IsCirculationActive { get; set; }

        //start or stop circulation
        //0: Circulation operationg mode not active.
        //1: Circulation operationg mode active.
        //Note: If the temperatue control is active, 
        //  circulation is also carried out, but circulation operationg mode is not active.
        public bool IsCirculationActive(byte subModuleIndex = 0x00)
        {
            bool retVal;
            try
            {
                retVal = ChillerCommState.IsCirculationActive();
                if (Prev_IsCirculationActive != retVal)
                {
                    LoggerManager.Debug($"[HuberChillerComm] IsCirculationActive(): change {Prev_IsCirculationActive} to {retVal}");
                    Prev_IsCirculationActive = retVal;
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //Activate or deactivate touch panel lock
        //Activate, deactivate or query operating lock at Pilot.
        //Bit 0:    Operating lock active or inactive.
        //          0: Operating lock inactive.
        //          1: Operating lock active. Manual operation of the thermostat via Pilot is not possible.
        //Bit 1:    Watchdog behavior
        //          0: Watchdog inactive.
        //          1: Activate Watchdog for 30s. If Bit 1 is not reset within 30s, the operating lock is
        //          automatically cancelled. This can be used to permit manual operation again if
        //          communication with the thermostat is interrupted for any reason.
        public (bool, bool) IsOperatingLock(byte subModuleIndex = 0x00)
        {
            (bool, bool) retVal;
            try
            {
                retVal = ChillerCommState.IsOperatingLock();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //- pump speed
        public int GetPumpSpeed(byte subModuleIndex = 0x00)
        {
            int retVal = 0;
            try
            {
                retVal = ChillerCommState.GetPumpSpeed();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //- minimum setpoint
        public double GetMinSetTemp(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetMinSetTemp();
            }
            catch (Exception err)
            {
                throw err;
            }
            return (double)retVal;
        }

        //- maximum setpoint
        public double GetMaxSetTemp(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetMaxSetTemp();
            }
            catch (Exception err)
            {
                throw err;
            }
            return (double)retVal;
        }

        //read setpoint pump speed
        public int GetSetTempPumpSpeed(byte subModuleIndex = 0x00)
        {
            int retVal;
            try
            {
                retVal = ChillerCommState.GetTargetPumpSpeed();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //RW    /Upper alram internal limit.
        public double GetUpperAlramInternalLimit(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetUpperAlramInternalLimit();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //RW    /Lower alram external limit.
        public double GetLowerAlramInternalLimit(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetLowerAlramInternalLimit();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //RW    /Upper alram process limit.
        public double GetUpperAlramProcessLimit(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetUpperAlramProcessLimit();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        //RW    /Lower alram process limit.
        public double GetLowerAlramProcessLimit(byte subModuleIndex = 0x00)
        {
            double retVal;
            try
            {
                retVal = ChillerCommState.GetLowerAlramProcessLimit();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        #endregion

        #region ==> Set data to Chiller


        public EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, byte subModuleIndex = 0x00, bool forcedSetValue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                switch (sendTempValueType)
                {
                    case TempValueType.HUBER:
                        break;
                    case TempValueType.TEMPCONTROLLER:
                        sendVal *= 10;
                        break;
                    case TempValueType.CELSIUS:
                        sendVal *= 100;
                        break;
                    default:
                        break;
                }
                LoggerManager.Debug($"[CHI][Chiller# {ChillerIndeex} SetTemp] Temp : {sendVal} , TempValueTarget : {sendTempValueType}");

                retVal = ChillerCommState.SetTargetTemp(sendVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_ERROR;
            }
            return retVal;
        }

        public void SetPresetProcTempVal(int sendVal, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetPresetProcTempVal(sendVal);
        }

        public void SetAutoPIDMode(bool bValue, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetAutoPIDMode(bValue);
        }

        public void SetTempMode(bool bValue, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetTempMode(bValue);
        }

        public void SetTempActiveMode(bool bValue, byte subModuleIndex = 0x00)
        {
            try
            {
                ChillerCommState.SetTempActiveMode(bValue);
                LoggerManager.Debug($"[CHI][Chiller #{ChillerIndeex}] SetTempActiveMode set to {bValue}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetProcTempActValSetMode(bool isOperatingMode, bool isChangeStandbyModeWhenErr329, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetProcTempActValSetMode(isOperatingMode, isChangeStandbyModeWhenErr329);
        }


        public void SetMinSetTemp(int sendVal, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetMinSetTemp(sendVal);
        }


        public void SetMaxSetTemp(int sendVal, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetMaxSetTemp(sendVal);
        }


        public void SetAlarmInternalUpperVal(int sendVal, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetAlarmInternalUpperVal(sendVal);
        }


        public void SetAlarmInternalLowerVal(int sendVal, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetAlarmInternalLowerVal(sendVal);
        }


        public void SetAlarmProcessUpperVal(int sendVal, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetAlarmProcessUpperVal(sendVal);
        }

        public void SetAlarmProcessLowerVal(int sendVal, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetAlarmProcessLowerVal(sendVal);
        }

        //- operating lock
        public void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetOperatingLock(bOperatinglock, bWatchdogBehavior);
        }

        //- circuation Active
        public void SetCircuationActive(bool bValue, byte subModuleIndex = 0x00)
        {
            try
            {
                string mode;
                if (bValue)
                {
                    mode = "external";
                }
                else
                {
                    mode = "internal";
                }

                ChillerCommState.SetCircuationActive(bValue);
                LoggerManager.Debug($"[CHI][Chiller #{ChillerIndeex}] SetCircuationActive set to {mode}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //- read setpoint pump speed
        public void SetSetTempPumpSpeed(int iValue, byte subModuleIndex = 0x00)
        {
            ChillerCommState.SetPumpSpeed(iValue);
        }
        #endregion
    }
}

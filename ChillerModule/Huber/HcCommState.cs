using EasyModbus;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using System;
using System.Net.NetworkInformation;
using System.Threading;

namespace Temperature.Temp.Chiller
{
    public abstract class HcCommState
    {
        public HuberChillerComm CommunicationModule;

        public HcCommState(HuberChillerComm commModule)
        {
            this.CommunicationModule = commModule;
        }

        public abstract EnumCommunicationState GetCommunicationState();

        public abstract EventCodeEnum Connect(string address, int port);

        public abstract void DisConnect();

        #region ==> Get Data from Chiller
        //- Setpoint,temperatur controller
        public abstract double GetSetTempValue();

        //- Internal temperature
        public abstract double GetInternalTempValue();

        //- return temperature
        public abstract double GetReturnTempVal();

        //- pump pressure(absolute)
        public abstract int GetPumpPressureVal();

        //- current power
        public abstract int GetCurrentPower();

        //- error report
        public abstract int GetErrorReport();

        //- warning message
        public abstract int GetWarningMessage();

        //- Process temperature (lemosa sensor )
        //- If no sensor is connected, the value -151��C is returned.
        public abstract double GetProcessTempVal();

        //- Process temperature (lemosa sensor )
        //- If no sensor is connected, the value -151��C is returned.
        public abstract double GetExtMoveVal();

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
        public abstract int GetStatusOfThermostat();

        //- AutoPID
        // The value 1 means that the controller is working in automatic mode.
        // The value 0 means that the controller is working in expert mode.
        //  (If you would like to work in expert mode, 
        //      the individual control parameters must be entered first.)
        public abstract bool IsAutoPID();

        //- temperature control mode
        //  0: Temperature control mode internal
        //  1: Temperature control mode process (cascade control)
        public abstract bool IsTempControlProcessMode();

        //- Temperature control current status.
        //  0: Temperature control not active.
        //  1: Temperature control active.
        public abstract bool IsTempControlActive();

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
        public abstract (bool, bool) GetProcTempActValSetMode();

        public abstract int GetSerialNumLow();

        public abstract int GetSerialNumHigh();

        public abstract int GetSerialNumber();

        //start or stop circulation
        //0: Circulation operationg mode not active.
        //1: Circulation operationg mode active.
        //Note: If the temperatue control is active, 
        //  circulation is also carried out, but circulation operationg mode is not active.
        public abstract bool IsCirculationActive();

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
        public abstract (bool, bool) IsOperatingLock();

        //- pump speed
        public abstract int GetPumpSpeed();

        //- minimum setpoint
        public abstract double GetMinSetTemp();

        //- maximum setpoint
        public abstract double GetMaxSetTemp();

        //read setpoint pump speed
        public abstract int GetTargetPumpSpeed();

        //RW    /Upper alram internal limit.
        public abstract double GetUpperAlramInternalLimit();

        //RW    /Lower alram external limit.
        public abstract double GetLowerAlramInternalLimit();

        //RW    /Upper alram process limit.
        public abstract double GetUpperAlramProcessLimit();

        //RW    /Lower alram process limit.
        public abstract double GetLowerAlramProcessLimit();

        #endregion

        #region ==> Set data to Chiller
        public abstract EventCodeEnum SetTargetTemp(double sendVal);

        public abstract void SetPresetProcTempVal(double sendVal);

        public abstract void SetAutoPIDMode(bool bValue);

        public abstract void SetTempMode(bool bValue);

        public abstract void SetTempActiveMode(bool bValue);

        public abstract void SetProcTempActValSetMode(bool isOperatingMode, bool isChangeStandbyModeWhenErr329);

        public abstract void SetMinSetTemp(double sendVal);

        public abstract void SetMaxSetTemp(double sendVal);

        public abstract void SetAlarmInternalUpperVal(double sendVal);

        public abstract void SetAlarmInternalLowerVal(double sendVal);

        public abstract void SetAlarmProcessUpperVal(int sendVal);

        public abstract void SetAlarmProcessLowerVal(int sendVal);

        //- operating lock
        public abstract void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior);

        //- circuation Active
        public abstract void SetCircuationActive(bool bValue);

        //- read setpoint pump speed
        public abstract void SetPumpSpeed(int iValue);

        #endregion
    }

    public sealed class HcDisconnectState : HcCommState
    {
        public HcDisconnectState(HuberChillerComm commModule) : base(commModule)
        {
        }

        public override EnumCommunicationState GetCommunicationState() => EnumCommunicationState.DISCONNECT;

        public override EventCodeEnum Connect(string address, int port)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                try
                {
                    var pingStatus = this.CommunicationModule.PingTest(address);

                    if (pingStatus == IPStatus.Success)
                    {
                        ModbusConnect(address, port);
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    //Ping Retry
                    var pingStatus = this.CommunicationModule.PingTest(address);
                    if (pingStatus == IPStatus.Success)
                    {
                        ModbusConnect(address, port);
                    }
                    else
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }

                void ModbusConnect(string modbusaddress, int modubsport)
                {
                    ModbusClient HBClient = new ModbusClient(address, port) { UnitIdentifier = 0xFF };
                    //HBClient.Connect();
                    HBClient.Connect(address, port);


                    if (HBClient?.Connected == true)
                    {
                        this.CommunicationModule.CommStateTransition(new HcConnectState(this.CommunicationModule) { Client = HBClient });
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        HBClient.Connect(address, port);
                        if (HBClient?.Connected == true)
                        {
                            this.CommunicationModule.CommStateTransition(new HcConnectState(this.CommunicationModule) { Client = HBClient });
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                DisConnect();
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public override void DisConnect()
        {
        }

        #region ==> Get Data from Chiller
        //- Setpoint,temperatur controller
        public override double GetSetTempValue()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- Internal temperature
        public override double GetInternalTempValue()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- return temperature
        public override double GetReturnTempVal()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- pump pressure(absolute)
        public override int GetPumpPressureVal()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- current power
        public override int GetCurrentPower()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- error report
        public override int GetErrorReport()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- warning message
        public override int GetWarningMessage()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- Process temperature (lemosa sensor )
        //- If no sensor is connected, the value -151��C is returned.
        public override double GetProcessTempVal()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- Process temperature (lemosa sensor )
        //- If no sensor is connected, the value -151��C is returned.
        public override double GetExtMoveVal()
        {
            CommonMsg();
            return int.MinValue;
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
        public override int GetStatusOfThermostat()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- AutoPID
        // The value 1 means that the controller is working in automatic mode.
        // The value 0 means that the controller is working in expert mode.
        //  (If you would like to work in expert mode, 
        //      the individual control parameters must be entered first.)
        public override bool IsAutoPID()
        {
            CommonMsg();
            return false;
        }

        //- temperature control mode
        //  0: Temperature control mode internal
        //  1: Temperature control mode process (cascade control)
        public override bool IsTempControlProcessMode()
        {
            CommonMsg();
            return false;
        }

        //- Temperature control current status.
        //  0: Temperature control not active.
        //  1: Temperature control active.
        public override bool IsTempControlActive()
        {
            CommonMsg();
            return false;
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
        public override (bool, bool) GetProcTempActValSetMode()
        {
            CommonMsg();
            return (false, false);
        }

        public override int GetSerialNumLow()
        {
            CommonMsg();
            return 0;
        }

        public override int GetSerialNumHigh()
        {
            CommonMsg();
            return 0;
        }

        public override int GetSerialNumber()
        {
            CommonMsg();
            return 0;
        }

        //start or stop circulation
        //0: Circulation operationg mode not active.
        //1: Circulation operationg mode active.
        //Note: If the temperatue control is active, 
        //  circulation is also carried out, but circulation operationg mode is not active.
        public override bool IsCirculationActive()
        {
            CommonMsg();
            return false;
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
        public override (bool, bool) IsOperatingLock()
        {
            CommonMsg();
            return (false, false);
        }

        //- pump speed
        public override int GetPumpSpeed()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- minimum setpoint
        public override double GetMinSetTemp()
        {
            CommonMsg();
            return int.MinValue;
        }

        //- maximum setpoint
        public override double GetMaxSetTemp()
        {
            CommonMsg();
            return int.MinValue;
        }

        //read setpoint pump speed
        public override int GetTargetPumpSpeed()
        {
            CommonMsg();
            return int.MinValue;
        }

        //RW    /Upper alram internal limit.
        public override double GetUpperAlramInternalLimit()
        {
            CommonMsg();
            return 500;
        }

        //RW    /Lower alram external limit.
        public override double GetLowerAlramInternalLimit()
        {
            CommonMsg();
            return -151;
        }

        //RW    /Upper alram process limit.
        public override double GetUpperAlramProcessLimit()
        {
            CommonMsg();
            return 500;
        }

        //RW    /Lower alram process limit.
        public override double GetLowerAlramProcessLimit()
        {
            CommonMsg();
            return -151;
        }

        #endregion

        #region ==> Set data to Chiller
        public override EventCodeEnum SetTargetTemp(double sendVal)
        {
            CommonMsg();
            return EventCodeEnum.CHILLER_SET_TARGET_TEMP_ERROR;
        }

        public override void SetPresetProcTempVal(double sendVal)
        {
            CommonMsg();
        }

        public override void SetAutoPIDMode(bool bValue)
        {
            CommonMsg();
        }

        public override void SetTempMode(bool bValue)
        {
            CommonMsg();
        }

        public override void SetTempActiveMode(bool bValue)
        {
            CommonMsg();
        }

        public override void SetProcTempActValSetMode(bool isOperatingMode, bool isChangeStandbyModeWhenErr329)
        {
            CommonMsg();
        }


        public override void SetMinSetTemp(double sendVal)
        {
            CommonMsg();
        }


        public override void SetMaxSetTemp(double sendVal)
        {
            CommonMsg();
        }


        public override void SetAlarmInternalUpperVal(double sendVal)
        {
            CommonMsg();
        }


        public override void SetAlarmInternalLowerVal(double sendVal)
        {
            CommonMsg();
        }


        public override void SetAlarmProcessUpperVal(int sendVal)
        {
            CommonMsg();
        }

        public override void SetAlarmProcessLowerVal(int sendVal)
        {
            CommonMsg();
        }

        //- operating lock
        public override void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior)
        {
            CommonMsg();
        }

        //- circuation Active
        public override void SetCircuationActive(bool bValue)
        {
            CommonMsg();
        }

        //- read setpoint pump speed
        public override void SetPumpSpeed(int iValue)
        {
            CommonMsg();
        }

        #endregion

        private void CommonMsg()
        {
            try
            {
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.CommonMsg(): Error occurred. Err = {err.Message}");
            }
        }
    }

    public sealed class HcConnectState : HcCommState
    {
        internal object lockObject = new object();
        internal ModbusClient Client = null;
        private long CommReadDelayTime { get; set; }
        private long CommWriteDelayTime { get; set; }
        public HcConnectState(HuberChillerComm commModule) : base(commModule)
        {
            try
            {
                CommReadDelayTime = commModule?.Module?.EnvControlManager()?.ChillerManager?.GetCommReadDelayTime() ?? 1;
                CommWriteDelayTime = commModule?.Module?.EnvControlManager()?.ChillerManager?.GetCommWriteDelayTime() ?? 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EnumCommunicationState GetCommunicationState() => EnumCommunicationState.CONNECTED;

        public override EventCodeEnum Connect(string address, int port)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            return retVal;
        }

        public override void DisConnect()
        {
            try
            {
                Client?.Disconnect();
                Client = null;
                this.CommunicationModule.CommStateTransition(new HcDisconnectState(this.CommunicationModule));
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        #region ==> Modbus Read/Write
        //Read
        public int[] ReadHoldingRegister(ModBusChillerEnum enumdata)
        {
            lock (lockObject)
            {
                try
                {
                    int[] result = Client?.ReadHoldingRegisters((int)enumdata, 1) ?? null;

                    if (result == null)
                    {
                        //Retry
                        LoggerManager.Debug($"[Chiller_Modbus] ReadHoldingRegister() fail. Retry {Client?.IPAddress} ");
                        result = Client?.ReadHoldingRegisters((int)enumdata, 1) ?? null;
                        if (result == null)
                            throw new Exception($"[Chiller_Modbus] ReadHoldingRegister() : Fail Get {enumdata} Data.");
                    }

                    return result;
                }
                catch (Exception err)
                {
                    throw err;
                }
            }
        }

        //Write
        public void WriteSingleRegister(ModBusChillerEnum enumdata, int value)
        {
            lock (lockObject)
            {
                try
                {
                    bool retVal = Client?.WriteSingleRegister((int)enumdata, value) ?? false;

                    if (retVal == false)
                    {
                        LoggerManager.Debug($"[Chiller_Modbus] WriteSingleRegister() fail. Retry {Client?.IPAddress} ");
                    }
                    else
                    {
                        Thread.Sleep((int)CommWriteDelayTime);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
        }

        #endregion

        #region ==> Get Data from Chiller
        //- Setpoint,temperatur controller
        public override double GetSetTempValue()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;
            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.SET_TEMP);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                //Ping fail or Ping throw exception
                LoggerManager.Error($"Chiller.GetSetTempValue(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return (double)retValue * 0.01;
        }

        //- Internal temperature
        public override double GetInternalTempValue()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.INTERNAL_TEMP);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetInternalTempValue(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return (double)retValue * 0.01;
        }

        //- return temperature
        public override double GetReturnTempVal()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.RETURN_TEMP);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetReturnTempVal(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return (double)retValue * 0.01;
        }

        //- pump pressure(absolute)
        public override int GetPumpPressureVal()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.PUMP_PRESSURE);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetPumpPressureVal(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //- current power
        public override int GetCurrentPower()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.CUR_POWER);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetCurrentPower(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //- error report
        public override int GetErrorReport()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ERROR);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetErrorReport(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //- warning message
        public override int GetWarningMessage()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.WARNING);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetWarningMessage(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //- Process temperature (lemosa sensor )
        //- If no sensor is connected, the value -151��C is returned.
        public override double GetProcessTempVal()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.PROCESS_TEMP);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetProcessTempVal(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return (double)retValue * 0.01;
        }

        //- Process temperature (lemosa sensor )
        //- If no sensor is connected, the value -151��C is returned.
        public override double GetExtMoveVal()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.EXTMOVE);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetExtMoveVal(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return (double)retValue;
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
        public override int GetStatusOfThermostat()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.STATUS1);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetStatusOfThermostat(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //- AutoPID
        // The value 1 means that the controller is working in automatic mode.
        // The value 0 means that the controller is working in expert mode.
        //  (If you would like to work in expert mode, 
        //      the individual control parameters must be entered first.)
        public override bool IsAutoPID()
        {
            bool retValue = false;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.AUTO_PID);

                if (tmpSetTempValue != null)
                {
                    if (tmpSetTempValue[0] == 0)
                        retValue = false;
                    else if (tmpSetTempValue[0] == 1)
                        retValue = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.IsAutoPID(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //- temperature control mode
        //  0: Temperature control mode internal
        //  1: Temperature control mode process (cascade control)
        public override bool IsTempControlProcessMode()
        {
            bool retValue = false;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.TMP_MODE);

                if (tmpSetTempValue != null)
                {
                    if (tmpSetTempValue[0] == 0)
                        retValue = false;
                    else if (tmpSetTempValue[0] == 1)
                        retValue = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.IsTempControlProcessMode(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //- Temperature control current status.
        //  0: Temperature control not active.
        //  1: Temperature control active.
        public override bool IsTempControlActive()
        {
            bool retValue = false;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.TMP_ACTIVE);

                if (tmpSetTempValue != null)
                {
                    if (tmpSetTempValue[0] == 0)
                        retValue = false;
                    else if (tmpSetTempValue[0] == 1)
                        retValue = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.IsTempControlActive(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
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
        public override (bool, bool) GetProcTempActValSetMode()
        {
            bool isOperatingMode = false;
            bool isChangeStandbyModeWhenErr329 = false;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.PROC_TEMP_PRESET);

                if (tmpSetTempValue != null)
                {
                    if ((tmpSetTempValue[0] & 0x01) == 0x00)
                    {
                        isOperatingMode = false;
                    }
                    else if ((tmpSetTempValue[0] & 0x01) == 0x01)
                    {
                        isOperatingMode = true;
                    }

                    if ((tmpSetTempValue[0] & 0x10) == 0x00)
                    {
                        isChangeStandbyModeWhenErr329 = true;
                    }
                    else if ((tmpSetTempValue[0] & 0x10) == 0x10)
                    {
                        isChangeStandbyModeWhenErr329 = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetProcTempActValSetMode(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return (isOperatingMode, isChangeStandbyModeWhenErr329);
        }

        public override int GetSerialNumLow()
        {
            int retValue = 0;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.SERIAL_NUMBER_L);

                if (tmpSetTempValue != null)
                {
                    retValue = tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetSerialNumLow(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        public override int GetSerialNumHigh()
        {
            int retValue = 0;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.SERIAL_NUMBER_H);

                if (tmpSetTempValue != null)
                {
                    retValue = tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetSerialNumHigh(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        public override int GetSerialNumber()
        {
            int retValue = 0;
            int lowNum = 0;
            int highNum = 0;
            try
            {
                lowNum = GetSerialNumLow();
                lowNum = (lowNum & 0xFFFF);
                highNum = GetSerialNumHigh();
                highNum = (highNum & 0xFFFF) << 16;

                retValue = lowNum + highNum;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retValue;
        }

        //start or stop circulation
        //0: Circulation operationg mode not active.
        //1: Circulation operationg mode active.
        //Note: If the temperatue control is active, 
        //  circulation is also carried out, but circulation operationg mode is not active.
        public override bool IsCirculationActive()
        {
            bool retValue = false;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.CIRC_ACTIVE);

                if (tmpSetTempValue != null)
                {
                    if (tmpSetTempValue[0] == 0)
                        retValue = false;
                    else if (tmpSetTempValue[0] == 1)
                        retValue = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.IsCirculationActive(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
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
        public override (bool, bool) IsOperatingLock()
        {
            bool bOperatinglock = false;
            bool bWatchdogBehavior = false;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.OPERATING_LOCK);

                if (tmpSetTempValue != null)
                {
                    if ((tmpSetTempValue[0] & 0x01) == 0x00)
                    {
                        bOperatinglock = false;
                    }
                    else if ((tmpSetTempValue[0] & 0x01) == 0x01)
                    {
                        bOperatinglock = true;
                    }

                    if ((tmpSetTempValue[0] & 0x10) == 0x00)
                    {
                        bWatchdogBehavior = true;
                    }
                    else if ((tmpSetTempValue[0] & 0x10) == 0x10)
                    {
                        bWatchdogBehavior = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.IsOperatingLock(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return (bOperatinglock, bWatchdogBehavior);
        }

        //- pump speed
        public override int GetPumpSpeed()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.PUMP_SPEED);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetPumpSpeed(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //- minimum setpoint
        public override double GetMinSetTemp()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.MIN_SET_TEMP);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetMinSetTemp(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return (double)retValue * 0.01;
        }

        //- maximum setpoint
        public override double GetMaxSetTemp()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.MAX_SET_POINT);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetMaxSetTemp(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return (double)retValue * 0.01;
        }

        //read setpoint pump speed
        public override int GetTargetPumpSpeed()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.SET_TEMP_PUMP_SPEED);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetTargetPumpSpeed(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //RW    /Upper alram internal limit.
        public override double GetUpperAlramInternalLimit()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ALARAM_LIMIT_INTERNAL_UPPER);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetUpperAlramInternalLimit(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue * 0.01;
        }

        //RW    /Lower alram external limit.
        public override double GetLowerAlramInternalLimit()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ALARAM_LIMIT_INTERNAL_LOWER);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetLowerAlramInternalLimit(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //RW    /Upper alram process limit.
        public override double GetUpperAlramProcessLimit()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ALARAM_LIMIT_PROCESS_UPPER);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetUpperAlramProcessLimit(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        //RW    /Lower alram process limit.
        public override double GetLowerAlramProcessLimit()
        {
            int retValue = int.MinValue;
            int[] tmpSetTempValue = null;

            try
            {
                tmpSetTempValue = ReadHoldingRegister(ModBusChillerEnum.ALARAM_LIMIT_PROCESS_LOWER);

                if (tmpSetTempValue != null)
                {
                    retValue = (short)tmpSetTempValue[0];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.GetLowerAlramProcessLimit(): Error occurred. Err = {err.Message}");
                throw err;
            }

            return retValue;
        }

        #endregion

        #region ==> Set data to Chiller
        /// <summary>
        /// Temp in 0.01 scale.
        /// Ex. 10.00�� -> 1000
        /// </summary>
        /// <param name="sendVal"></param>
        public override EventCodeEnum SetTargetTemp(double sendVal)
        {
            try
            {
                int tempData = (int)(sendVal * 100.0);
                WriteSingleRegister(ModBusChillerEnum.SET_TEMP, tempData);
                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.SetTargetTemp({sendVal}): Error occurred. Err = {err.Message}");
                return EventCodeEnum.CHILLER_SET_TARGET_TEMP_ERROR;
            }
        }
        /// <summary>
        /// Temp in 0.01 scale.
        /// Ex. 10.00�� -> 1000
        /// </summary>
        /// <param name="sendVal"></param>
        public override void SetPresetProcTempVal(double sendVal)
        {
            try
            {
                int tempData = (int)(sendVal * 100.0);
                WriteSingleRegister(ModBusChillerEnum.EXTMOVE, tempData);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.SetPresetProcTempVal({sendVal}): Error occurred. Err = {err.Message}");
            }
        }

        public override void SetAutoPIDMode(bool bValue)
        {
            try
            {
                WriteSingleRegister(ModBusChillerEnum.AUTO_PID, (bValue ? 1 : 0));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.SetAutoPIDMode({bValue}): Error occurred. Err = {err.Message}");
            }
        }

        public override void SetTempMode(bool bValue)
        {
            try
            {
                WriteSingleRegister(ModBusChillerEnum.TMP_MODE, (bValue ? 1 : 0));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.SetTempMode({bValue}): Error occurred. Err = {err.Message}");
            }
        }

        public override void SetTempActiveMode(bool bValue)
        {
            try
            {
                WriteSingleRegister(ModBusChillerEnum.TMP_ACTIVE, (bValue ? 1 : 0));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.SetTempActiveMode({bValue}): Error occurred. Err = {err.Message}");
            }
        }

        public override void SetProcTempActValSetMode(bool isOperatingMode, bool isChangeStandbyModeWhenErr329)
        {
            try
            {
                int sendData = 0;
                sendData = Convert.ToInt32(isOperatingMode) + (Convert.ToInt32(isChangeStandbyModeWhenErr329) * 2);

                WriteSingleRegister(ModBusChillerEnum.TMP_ACTIVE, sendData);
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.SetProcTempActValSetMode({isOperatingMode}, {isChangeStandbyModeWhenErr329}): Error occurred. Err = {err.Message}");
            }
        }

        public override void SetMinSetTemp(double sendVal)
        {
            try
            {
                int tempData = (int)(sendVal * 100.0);
                WriteSingleRegister(ModBusChillerEnum.MIN_SET_TEMP, tempData);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.SetMinSetTemp({sendVal}): Error occurred. Err = {err.Message}");
            }
        }


        public override void SetMaxSetTemp(double sendVal)
        {
            try
            {
                int tempData = (int)(sendVal * 100.0);
                WriteSingleRegister(ModBusChillerEnum.MAX_SET_POINT, tempData);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Chiller.SetMaxSetTemp({sendVal}): Error occurred. Err = {err.Message}");
            }
        }


        public override void SetAlarmInternalUpperVal(double sendVal)
        {
            try
            {
                int tempData = (int)(sendVal * 100.0);
                WriteSingleRegister(ModBusChillerEnum.ALARAM_LIMIT_INTERNAL_UPPER, tempData);
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.SetAlarmInternalUpperVal({sendVal}): Error occurred. Err = {err.Message}");
            }
        }


        public override void SetAlarmInternalLowerVal(double sendVal)
        {
            try
            {
                int tempData = (int)(sendVal * 100.0);
                WriteSingleRegister(ModBusChillerEnum.ALARAM_LIMIT_INTERNAL_LOWER, tempData);
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.SetAlarmInternalLowerVal({sendVal}): Error occurred. Err = {err.Message}");
            }
        }

        public override void SetAlarmProcessUpperVal(int sendVal)
        {
            try
            {
                int tempData = (int)(sendVal * 100.0);
                WriteSingleRegister(ModBusChillerEnum.ALARAM_LIMIT_PROCESS_UPPER, tempData);
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.SetAlarmProcessUpperVal({sendVal}): Error occurred. Err = {err.Message}");
            }
        }

        public override void SetAlarmProcessLowerVal(int sendVal)
        {
            try
            {
                int tempData = (int)(sendVal * 100.0);
                WriteSingleRegister(ModBusChillerEnum.ALARAM_LIMIT_PROCESS_LOWER, tempData);
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.SetAlarmProcessLowerVal({sendVal}): Error occurred. Err = {err.Message}");
            }
        }

        //- operating lock
        public override void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior)
        {
            try
            {
                int sendData = 0;
                sendData = Convert.ToInt32(bOperatinglock) + (Convert.ToInt32(bWatchdogBehavior) * 2);

                WriteSingleRegister(ModBusChillerEnum.OPERATING_LOCK, sendData);
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.SetOperatingLock({bOperatinglock}, {bWatchdogBehavior}): Error occurred. Err = {err.Message}");
            }
        }

        //- circuation Active
        public override void SetCircuationActive(bool bValue)
        {
            try
            {
                WriteSingleRegister(ModBusChillerEnum.CIRC_ACTIVE, (bValue ? 1 : 0));
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.SetCircuationActive({bValue}): Error occurred. Err = {err.Message}");
            }
        }

        /// <summary>
        /// read setpoint pump speed
        /// Value range in 0 ~ 32000 mbar
        /// 
        /// </summary>
        /// <param name="iValue"></param>
        public override void SetPumpSpeed(int iValue)
        {
            try
            {
                WriteSingleRegister(ModBusChillerEnum.SET_TEMP_PUMP_SPEED, iValue);
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Chiller.SetPumpSpeed({iValue}): Error occurred. Err = {err.Message}");
            }
        }

        #endregion
    }
}

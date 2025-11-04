using System;

namespace ChillerModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using SerializerUtil;
    using System.ServiceModel;
    using ProberInterfaces.Temperature.Chiller;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Temperature;
    using CommunicationModule;

    public class ChillerRemote : IChillerComm, IFactoryModule
    {
        public ChillerServiceDirectProxy ChillerServiceProxy { get; set; }
        public ChillerServiceDirectProxy GetChillerServiceProxy()
        {
            ChillerServiceDirectProxy proxy = null;

            try
            {
                if (ChillerServiceProxy?.IsServiceAvailable() == true)
                {
                    proxy = ChillerServiceProxy;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return proxy;
        }

        public void DisConnect()
        {
            try
            {
                ChillerServiceProxy?.DisConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                ChillerServiceProxy = null;
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LoaderController().GetconnectFlag())
                {
                    retVal = ConnectService();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug("Chiller channel ConnectService failed");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LoaderController().GetconnectFlag() == false)
                {
                    return retVal;
                }
                if (CommunicationManager.CheckAvailabilityCommunication(this.LoaderController().GetLoaderIP(), ServicePort.ChillerServicePort))
                {
                    var statecheck = (ChillerServiceProxy as ICommunicationObject)?.State == CommunicationState.Closed
                          || (ChillerServiceProxy as ICommunicationObject)?.State == CommunicationState.Faulted;
                    if (ChillerServiceProxy == null || statecheck)
                    {
                        // Connect
                        ChillerServiceProxy = new ChillerServiceDirectProxy(this.LoaderController().GetLoaderIP(), ServicePort.ChillerServicePort);
                        if(ChillerServiceProxy != null)
                        {
                            ChillerServiceProxy.InitService();
                            ChillerServiceProxy.ChannelFactory.Faulted += Channel_Faulted;
                            ChillerServiceProxy.ChannelFactory.Closed += Channel_Closed;


                            this.EnvControlManager().GetChillerModule().ChillerParam.IsAbortActivate = ChillerServiceProxy?.GetChillerAbortActiveState() ?? true;


                            //Chiller Parameter
                            var param = ChillerServiceProxy.GetChillerParam();
                            object target;
                            SerializeManager.DeserializeFromByte(param, out target, typeof(ChillerParameter));
                            this.EnvControlManager().GetChillerModule().InitParam((IChillerParameter)target);
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        //연결된 상태
                        retVal = EventCodeEnum.NONE;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                ChillerServiceProxy = null;
            }
            return retVal;
        }

        public EventCodeEnum Connect(string address, int port)
        {
            try
            {
                if (ChillerServiceProxy != null)
                {
                    //해당 함수는 새로 연결 요청시 호출된다. 그러므로 기존 채널이 있다는 것은 abnormal channel 이다.
                    ChillerServiceProxy?.Abort();
                    ChillerServiceProxy = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            EventCodeEnum ret = ConnectService();

            if (ret != EventCodeEnum.NONE)
                return EventCodeEnum.CHILLER_NOT_CONNECTED;
            return ret;
        }
        public void Channel_Faulted(object sender, EventArgs e)
        {
            LoggerManager.Debug($"Chiller channel Faulted. Sender = {sender}");
        }

        public void Channel_Closed(object sender, EventArgs e)
        {
            LoggerManager.Debug($"Chiller channel Closed. Sender = {sender}");
        }

        public EnumCommunicationState GetCommState(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetCommState() ?? EnumCommunicationState.DISCONNECT;
        }
        public ICommunicationMeans GetCommunicationObj()
        {
            return null;
        }
        public object GetCommLockObj()
        {
            return null;
        }
        public EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false)
        {
            return GetChillerServiceProxy()?.CheckCanUseChiller(sendVal, stageindex, false, forcedSetValue) ?? EventCodeEnum.UNDEFINED;
        }
        public EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, byte subModuleIndex = 0x00, bool forcedSetValue = false)
        {
            try
            {
                LoggerManager.Debug($"[ChillerComm SetTargetTemp] Temp : {sendVal}, ValueType : {sendTempValueType}");
                return GetChillerServiceProxy()?.SetTargetTemp(sendVal, sendTempValueType, forcedSetValue:forcedSetValue) ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.CHILLER_SET_TARGET_TEMP_ERROR;
            }
        }

        public void SetTempActiveMode(bool bValue, byte subModuleIndex = 0x00)
        {
            try
            {
                GetChillerServiceProxy()?.SetTempActiveMode(bValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetSetTempPumpSpeed(int iValue, byte subModuleIndex = 0x00)
        {
            try
            {
                GetChillerServiceProxy()?.SetSetTempPumpSpeed(iValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior, byte subModuleIndex = 0x00)
        {
            return;
        }

        public void SetCircuationActive(bool bValue, byte subModuleIndex = 0x00)
        {
            return;
        }

        public double GetSetTempValue(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetSetTempValue() ?? -999;
        }

        public double GetInternalTempValue(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetInternalTempValue() ?? -999;
        }

        public double GetReturnTempVal(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetReturnTempVal() ?? 0;
        }

        public int GetPumpPressureVal(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetPumpPressureVal() ?? 0;
        }

        public int GetCurrentPower(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetCurrentPower() ?? 0;
        }

        public int GetErrorReport(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetErrorReport() ?? 0;
        }

        public int GetWarningMessage(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetWarningMessage() ?? 0;
        }

        public double GetProcessTempVal(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetProcessTempVal() ?? 0;
        }

        public double GetExtMoveVal(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetExtMoveVal() ?? 0;
        }

        public int GetStatusOfThermostat(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetStatusOfThermostat() ?? 0;
        }

        public bool IsAutoPID(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.IsAutoPID() ?? false;
        }

        public bool IsTempControlProcessMode(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.IsTempControlProcessMode() ?? false;
        }

        public bool IsTempControlActive(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.IsTempControlActive() ?? false;
        }

        public (bool, bool) GetProcTempActValSetMode(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetProcTempActValSetMode() ?? (false, false);
        }

        public int GetSerialNumLow(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetSerialNumLow() ?? -1;
        }

        public int GetSerialNumHigh(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetSerialNumHigh() ?? -1;
        }

        public int GetSerialNumber(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetSerialNumber() ?? -1;
        }

        public bool IsCirculationActive(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.IsCirculationActive() ?? false;
        }

        public (bool, bool) IsOperatingLock(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.IsOperatingLock() ?? (false, false);
        }

        public int GetPumpSpeed(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetPumpSpeed() ?? -9999;
        }

        public double GetMinSetTemp(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetMinSetTemp() ?? -9999;
        }

        public double GetMaxSetTemp(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetMaxSetTemp() ?? -9999;
        }

        public int GetSetTempPumpSpeed(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetSetTempPumpSpeed() ?? -9999;
        }

        public double GetUpperAlramInternalLimit(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetUpperAlramInternalLimit() ?? -9999;
        }

        public double GetLowerAlramInternalLimit(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetLowerAlramInternalLimit() ?? -9999;
        }

        public double GetUpperAlramProcessLimit(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetUpperAlramProcessLimit() ?? -9999;
        }

        public double GetLowerAlramProcessLimit(byte subModuleIndex = 0x00)
        {
            return GetChillerServiceProxy()?.GetLowerAlramProcessLimit() ?? -9999;
        }

        public void Dispose()
        {
            try
            {
                DisConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


    }
}

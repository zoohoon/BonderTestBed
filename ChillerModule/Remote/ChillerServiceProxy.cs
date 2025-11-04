using System;

namespace ChillerModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Temperature;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChillerServiceDirectProxy : ClientBase<IChillerService>, IChillerServiceProxy
    {
        int stageIdx = -1;
        private object chnLockObj = new object();
        public ChillerServiceDirectProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(IChillerService)),
            new NetTcpBinding()
            {
                SendTimeout = new TimeSpan(0, 5, 0),
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 50000000,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/chillerpipe")))
        {
            stageIdx = this.LoaderController().GetChuckIndex();
        }

        public ChillerServiceDirectProxy(InstanceContext callback, ServiceEndpoint endpoint) :
            base(callback, endpoint)
        {
            if (endpoint.Binding is NetTcpBinding)
            {

            }
            stageIdx = this.LoaderController().GetChuckIndex();
        }

        public IChillerService GetService()
        {
            return Channel;
        }

        public ICommunicationMeans GetCommunicationObj()
        {
            return null;
        }
        public object GetCommLockObj()
        {
            return null;
        }
        public bool IsServiceAvailable()
        {
            bool retVal = false;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 15);
                            retVal = Channel.IsServiceAvailable();
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"Chiller Service IsServiceAvailable timeout error.");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                retVal = false;
            }

            return retVal;
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public void InitService()
        {
            lock (chnLockObj)
            {
                Channel.InitService();
            }
        }
        public EventCodeEnum Connect(string address, int port)
        {
            lock(chnLockObj)
            {
                return Channel.Connect(address, port, stageIdx);
            }
        }

        public void DisConnect()
        {
            try
            {
                try
                {
                    lock (chnLockObj)
                    {
                        if (IsOpened())
                        {
                            this.Close();
                        }
                        else
                        {
                            this.Abort();
                        }
                    }
                }
                catch (CommunicationException err)
                {
                    this.Abort();
                    LoggerManager.Exception(err);
                }
                catch (Exception err)
                {
                    this.Abort();
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsOpened()
        {
            if (State == CommunicationState.Created || State == CommunicationState.Opened)
            {
                return true;
            }
            return false;
        }

        public EnumCommunicationState GetCommState(byte SubModuleIndex = 0x00)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                    return Channel.GetCommState(stageIdx);
            }
            return EnumCommunicationState.DISCONNECT;
        }
        public byte[] GetChillerParam()
        {
            byte[] retVal = null;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        retVal = Channel.GetChillerParam(stageIdx);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            lock (chnLockObj)
            {
                if (IsOpened())
                    retVal = Channel.CheckCanUseChiller(sendVal, stageIdx, offvalve, forcedSetValue);
            }
            return retVal;
        }

        public EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, byte SubModuleIndex = 0x00, bool forcedSetValue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        retVal = Channel.SetTargetTemp(sendVal, sendTempValueType, stageIdx, forcedSetValue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetTempActiveMode(bool bValue, byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        Channel.SetTempActiveMode(bValue, stageIdx);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
            }
        }

        public void SetSetTempPumpSpeed(int iValue, byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        Channel.SetSetTempPumpSpeed(iValue, stageIdx);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
            }
        }

        public void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior, byte SubModuleIndex = 0x00)
        {
            return;
        }
        public void SetCircuationActive(bool bValue, byte SubModuleIndex = 0x00)
        {
            return;
        }
        public double GetSetTempValue(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetSetTempValue(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetInternalTempValue(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetInternalTempValue(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetReturnTempVal(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetReturnTempVal(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetPumpPressureVal(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetPumpPressureVal(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetCurrentPower(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetCurrentPower(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetErrorReport(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetErrorReport(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetWarningMessage(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetWarningMessage(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetProcessTempVal(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetProcessTempVal(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetExtMoveVal(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetExtMoveVal(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetStatusOfThermostat(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetStatusOfThermostat(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public bool IsAutoPID(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.IsAutoPID(stageIdx);
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool IsTempControlProcessMode(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.IsTempControlProcessMode(stageIdx);
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool IsTempControlActive(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.IsTempControlActive(stageIdx);
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return false;
            }
        }

        public (bool, bool) GetProcTempActValSetMode(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetProcTempActValSetMode(stageIdx);
                    return (false, false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return (false, false);
            }
        }

        public int GetSerialNumLow(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetSerialNumLow(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetSerialNumHigh(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetSerialNumHigh(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetSerialNumber(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetSerialNumber(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public bool IsCirculationActive(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.IsCirculationActive(stageIdx);
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return false;
            }
        }

        public (bool, bool) IsOperatingLock(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.IsOperatingLock(stageIdx);
                    return (false, false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return (false, false);
            }
        }

        public int GetPumpSpeed(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetPumpSpeed(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetMinSetTemp(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetMinSetTemp(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetMaxSetTemp(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetMaxSetTemp(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetSetTempPumpSpeed(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetSetTempPumpSpeed(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetUpperAlramInternalLimit(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetUpperAlramInternalLimit(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetLowerAlramInternalLimit(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetLowerAlramInternalLimit(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetUpperAlramProcessLimit(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetUpperAlramProcessLimit(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetLowerAlramProcessLimit(byte SubModuleIndex = 0x00)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetLowerAlramProcessLimit(stageIdx);
                    return -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public bool GetChillerAbortActiveState()
        {
            try
            {
                lock(chnLockObj)
                {
                    if (IsOpened())
                        return Channel.GetChillerAbortActiveState(stageIdx);
                    return true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return true;
            }
        }

    }
}

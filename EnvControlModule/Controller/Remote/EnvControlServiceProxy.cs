namespace EnvControlModule
{
    using LogModule;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using ProberErrorCode;
    using ProberInterfaces;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class EnvControlServiceDirectProxy : DuplexClientBase<IEnvControlService>, IFactoryModule
    {

        private int stageIdx = -1;

        public EnvControlServiceDirectProxy(InstanceContext callback, ServiceEndpoint endpoint) :
            base(callback, endpoint)
        {

            if (endpoint.Binding is NetTcpBinding)
            {

            }
            stageIdx = this.LoaderController().GetChuckIndex();
        }

        public static object proxyLock { get; } = new object();

        public void InitService(int stageIndex = 0)
        {
            Channel.InitService(stageIndex);
        }

        public bool IsServiceAvailable()
        {
            bool retVal = false;
            try
            {
                lock (proxyLock)
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
                            LoggerManager.Error($"EnvControl Service IsServiceAvailable timeout error.");
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
        public bool IsOpened()
        {
            return (State == CommunicationState.Opened || State == CommunicationState.Created);
        }

        public void DisConnect()
        {
            try
            {
                try
                {
                    lock (proxyLock)
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


        public bool IsUsingDryAir()
        {
            try
            {
                lock (proxyLock)
                {
                    if (IsOpened())
                    {
                        return Channel.IsUsingDryAir(stageIdx);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
            }
            return false;
        }

        public bool IsUsingChiller()
        {
            try
            {
                lock (proxyLock)
                {
                    if (IsOpened())
                    {
                        return Channel.IsUsingChiller(stageIdx);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return false;
            }
        }

        #region ... Valve
        public EventCodeEnum SetValveState(bool enableFlag, EnumValveType valveType, int stageIndex = -1)
        {
            try
            {
                lock (proxyLock)
                {
                    if (IsOpened())
                    {
                        return Channel.SetValveState(enableFlag, valveType, stageIdx);
                    }
                    else
                    {
                        return EventCodeEnum.ENVCONTROL_COMM_ERROR;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return EventCodeEnum.ENVCONTROL_COMM_ERROR;
            }
        }

        public bool GetValveState(EnumValveType valveType)
        {
            try
            {
                lock (proxyLock)
                {
                    if (IsOpened())
                        return Channel.GetValveState(valveType, stageIdx);
                    else
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
        #endregion

        #region ... Dry Air
        public byte[] GetDryAirParam(int stageindex = -1)
        {
            try
            {
                lock (proxyLock)
                {
                    if (IsOpened())
                        return Channel.GetDryAirParam(stageindex);
                    else
                        return null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return null;
            }
        }
        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            try
            {
                lock (proxyLock)
                {
                    if (IsOpened())
                        return Channel.DryAirForProber(value, dryairType, stageIndex);
                    else
                        return EventCodeEnum.ENVCONTROL_CONNECT_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                return EventCodeEnum.ENVCONTROL_CONNECT_ERROR;
            }
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            try
            {
                lock (proxyLock)
                {
                    if (IsOpened())
                        return Channel.GetLeakSensor(out value, leakSensorIndex, stageindex);
                    else
                    {
                        value = false;
                        return -1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
                value = false;
                return -1;
            }
        }
        #endregion

        #region ... FFU

        public void RaiseFFUAlarm(string alarmmessage)
        {
            try
            {
                lock (proxyLock)
                {
                    if (IsOpened())
                        Channel.RaiseFFUAlarm(alarmmessage);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}

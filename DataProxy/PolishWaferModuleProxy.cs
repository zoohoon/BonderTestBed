using System;

namespace RemoteServiceProxy
{
    using LogModule;
    using ProberInterfaces;

    using System.ServiceModel;
    using System.ServiceModel.Description;
    using ProberInterfaces.PolishWafer;
    using ProberErrorCode;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PolishWaferModuleProxy : ClientBase<IPolishWaferModule>, IPolishWaferModuleProxy
    {
        public PolishWaferModuleProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(IPolishWaferModule)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 50000000,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.PolishWaferModuleService}")))
        {
            lock (chnLockObj)
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
            }
        }
        private object chnLockObj = new object();
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
                            LoggerManager.Error($"PolishWaferModuleProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"PolishWafer Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"PolishWafer Service service error.");
                retVal = false;
            }

            return retVal;
        }
        public void InitService()
        {
            Channel.IsServiceAvailable();
        }
        public void DeInitService()
        {
            //Dispose
        }
        public void Dispose()
        {
            lock (chnLockObj)
            {

            }
        }

        public bool IsOpened()
        {
            bool retVal = false;

            if (State == CommunicationState.Opened | State == CommunicationState.Created)
                retVal = true;
            return retVal;
        }

        public byte[] GetPolishWaferParam()
        {
            byte[] retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetPolishWaferParam();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public void SetPolishWaferIParam(byte[] param)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetPolishWaferIParam(param);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public bool PWIntervalhasLotstart(int index = -1)
        {
            bool ret = false;
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        ret = Channel.PWIntervalhasLotstart(index);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            return ret;
        }
        public EventCodeEnum DoCentering(IPolishWaferCleaningParameter param)
        {
            if (!IsOpened())
                return EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            lock (chnLockObj)
            {
                return Channel.DoCentering(param);
            }
        }

        public EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param)
        {
            if (!IsOpened())
                return EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            lock (chnLockObj)
            {
                return Channel.DoFocusing(param);
            }
        }

        public EventCodeEnum DoCleaning(IPolishWaferCleaningParameter param)
        {
            if (!IsOpened())
                return EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
            lock (chnLockObj)
            {
                return Channel.DoCleaning(param);
            }
        }
    }
}

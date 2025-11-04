using LogModule;
using ProberInterfaces.Proxies;
using ProberInterfaces.Retest;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace RemoteServiceProxy
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RetestModuleProxy : ClientBase<IRetestModule>, IRetestModuleProxy
    {
        public RetestModuleProxy(string ip, int port)
           : base(
           new ServiceEndpoint(ContractDescription.GetContract(typeof(IRetestModule)),
           new NetTcpBinding()
           {
               ReceiveTimeout = TimeSpan.MaxValue,
               MaxBufferPoolSize = 524288,
               MaxReceivedMessageSize = 50000000,
               Security = new NetTcpSecurity() { Mode = SecurityMode.None },
               ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
           },
           new EndpointAddress($"net.tcp://{ip}:{port}/POS/RetestModuleService")))
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
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 10, 0);
                            retVal = Channel.IsServiceAvailable();
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"RetestModuleProxy IsServiceAvailable timeout error");
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

        public bool IsOpened()
        {
            bool retVal = false;

            try
            {
                if (State == CommunicationState.Opened | State == CommunicationState.Created)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }

        public void InitService()
        {
            try
            {
                IsServiceAvailable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DeInitService()
        {
            //Dispose
        }

        public byte[] GetRetestParam()
        {
            byte[] retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetRetestParam();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public void SetRetestParam(byte[] param)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetRetestIParam(param);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

    }
}

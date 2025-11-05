using LogModule;
using ProberInterfaces;
using ProberInterfaces.Proxies;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace RemoteServiceProxy
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LotOPModuleProxy : ClientBase<ILotOPModule>, ILotOPModuleProxy
    {
        public LotOPModuleProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(ILotOPModule)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 50000000,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.LotOPModuleService}")))
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
                            LoggerManager.Error($"LotOPModuleProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"LotOPModule Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"LotOPModule Service service error.");
                retVal = false;
            }

            return retVal;
        }
        public void InitService()
        {

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

        public void SetDeviceName(string devicename)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetDeviceName(devicename);
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

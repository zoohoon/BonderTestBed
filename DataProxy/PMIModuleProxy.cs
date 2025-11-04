using System;

namespace RemoteServiceProxy
{
    using LogModule;
    using ProberInterfaces;

    using System.ServiceModel;
    using System.ServiceModel.Description;
    using ProberErrorCode;
    using ProberInterfaces.PMI;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PMIModuleProxy : ClientBase<IPMIModule>, IPMIModuleProxy
    {
        public PMIModuleProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(IPMIModule)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 50000000,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.PMIModuleSerice}")))
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
                            LoggerManager.Error($"PMIModuleProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"PMI Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"PMI Service service error.");
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

        public EventCodeEnum LoadDevParameter()
        {
            lock (chnLockObj)
            {
                return Channel.LoadDevParameter();
            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            lock (chnLockObj)
            {
                return Channel.LoadSysParameter();
            }
        }

        public EventCodeEnum InitModule()
        {
            lock (chnLockObj)
            {
                return Channel.InitModule();
            }
        }

        public EventCodeEnum SaveDevParameter()
        {
            lock (chnLockObj)
            {
                return Channel.SaveDevParameter();
            }
        }

        public EventCodeEnum SaveSysParameter()
        {
            lock (chnLockObj)
            {
                return Channel.SaveSysParameter();
            }
        }

        public EventCodeEnum InitDevParameter()
        {
            lock (chnLockObj)
            {
                return Channel.InitDevParameter();
            }
        }

        public void AddPadTemplate(PadTemplate template)
        {
            lock (chnLockObj)
            {
                Channel.AddPadTemplate(template);
            }
        }

        public PMITriggerComponent GetTriggerComponent()
        {
            lock (chnLockObj)
            {
                return Channel.GetTriggerComponent();
            }
        }

        public bool GetPMIEnableParam()
        {
            lock (chnLockObj)
            {
                return Channel.GetPMIEnableParam();
            }
        }
    }
}

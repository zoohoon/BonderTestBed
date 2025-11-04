using System;

namespace RemoteServiceProxy
{
    using LogModule;
    using ProberInterfaces;

    using System.ServiceModel;
    using System.ServiceModel.Description;
    using ProberInterfaces.PinAlign;
    using ProberInterfaces.Proxies;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PinAlignerProxy : ClientBase<IPinAligner>, IPinAlignerProxy
    {
        public PinAlignerProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(IPinAligner)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 50000000,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None},
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.PinAlignerService}")))
        {
            lock (chnLockObj)
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
            }
        }
        private object chnLockObj = new object();

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
                            LoggerManager.Error($"PinAlignerProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"PinAlign Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"PinAlign Service service error.");
                retVal = false;
            }

            return retVal;
        }

        public bool IsOpened()
        {
            bool retVal = false;

            if (State == CommunicationState.Opened | State == CommunicationState.Created)
                retVal = true;
            return retVal;
        }

        public byte[] GetPinAlignerParam()
        {
            byte[] retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetPinAlignerParam();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public void SetPinAlignerIParam(byte[] param)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetPinAlignerIParam(param);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        //public byte[] GetTemplateParam()
        //{
        //    byte[] retval = null;

        //    lock (chnLockObj)
        //    {
        //        if (IsOpened())
        //        {
        //            try
        //            {
        //                retval = Channel.GetTemplateParam();
        //            }
        //            catch (Exception err)
        //            {
        //                LoggerManager.Exception(err);
        //            }
        //        }

        //        return retval;
        //    }
        //}
    }
}

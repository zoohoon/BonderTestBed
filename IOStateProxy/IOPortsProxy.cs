using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.ServiceModel;

namespace IOStateProxy
{
    using LogModule;
    using ProberInterfaces;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class IOPortProxy : ClientBase<IIOMappingsParameter>, IIOPortProxy
    {
        public IOPortProxy() :
           base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IIOMappingsParameter)),
       new NetNamedPipeBinding(),
       new EndpointAddress("net.pipe://localhost/POS/IOPortsService")))
        {

        }

        public IOPortProxy(InstanceContext callback) :
                base(callback, new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IIOMappingsParameter)),
                    new NetTcpBinding() { MaxBufferPoolSize = 524288, MaxReceivedMessageSize = 1000000 },
                    new EndpointAddress($"net.pipe://localhost/POS/{ServiceAddress.IOPortsService}")))
        {
            try
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");

            }
            catch (Exception err)
            {
                LoggerManager.Error($"IOPortProxy(): Error occurred. Err = {err.Message}");
            }
        }

        public IOPortProxy(string ip, int port, InstanceContext callback) : base(callback, new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IIOMappingsParameter)),
                    new NetTcpBinding()
                    {
                        MaxBufferSize = 10000000,
                        MaxBufferPoolSize = 524288,
                        MaxReceivedMessageSize = 50000000,
                        Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                        ReceiveTimeout = TimeSpan.MaxValue,
                        ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                    },
                    new EndpointAddress($"net.tcp://{ip}:{port}/POS/IOPortsService")))
        {
            try
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"IOPortProxy({port}): Error occurred. Err = {err.Message}");

            }
        }
        private object chnLockObj = new object();
        public bool IsServiceAvailable()
        {
            bool retVal = false;
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
                        LoggerManager.Error($"IOPortsProxy IsServiceAvailable timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
                else
                {
                    LoggerManager.Error($"FileManager Service service error.");
                    retVal = false;
                }
            }
            return retVal;
        }

        public void SetForcedIO(IOPortDescripter<bool> ioport, bool IsForced, bool ForecedValue)
        {
            try
            {
                if (IsOpened())
                {
                    Channel.SetForcedIO(ioport, IsForced, ForecedValue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<IOPortDescripter<bool>> GetInputPorts()
        {
            List<IOPortDescripter<bool>> retval = null;

            try
            {
                if (IsOpened())
                {
                    retval = Channel.GetInputPorts();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public List<IOPortDescripter<bool>> GetOutputPorts()
        {
            List<IOPortDescripter<bool>> retval = null;

            try
            {
                if (IsOpened())
                {
                    retval = Channel.GetOutputPorts();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public IOPortDescripter<bool> GetPortStatus(string key)
        {
            if (IsOpened())
                return Channel.GetPortStatus(key);
            else
                return null;
        }

        public void SetPortStateAs(IOPortDescripter<bool> port, bool value)
        {
            try
            {
                if (IsOpened())
                    Channel.SetPortStateAs(port, value);
            }
            catch (CommunicationException err)
            {
                LoggerManager.Error($"CommunicationException occurred. Err = {err.Message}");
            }
            catch (TimeoutException err)
            {
                LoggerManager.Error($"TimeoutException occurred. Err = {err.Message}");
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Error occurred. Err = {err.Message}");
            }
        }

        public void InitService()
        {
            if (IsOpened())
                Channel.InitService();
            IsServiceAvailable();
        }
        public bool IsOpened()
        {
            if (State == CommunicationState.Opened | State == CommunicationState.Created)
                return true;
            else
                return false;
        }
        public CommunicationState GetCommunicationState()
        {
            return this.State;
        }

        public void DeInitService()
        {
            //Dispose
        }
    }
}

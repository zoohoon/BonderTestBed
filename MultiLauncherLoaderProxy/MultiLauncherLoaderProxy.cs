using LogModule;
using ProberInterfaces;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using LoaderBase.Communication;

namespace MultiLauncherLoaderProxy
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class MultiLauncherLoaderProxy : DuplexClientBase<ILoaderCommunicationManager>, IMultiExecuterLoaderProxy
    {
        public int Port { get; set; }
        public string IP { get; set;}
        public MultiLauncherLoaderProxy(int port, InstanceContext callback, string ip) :
               base(callback, new ServiceEndpoint(
                   ContractDescription.GetContract(typeof(ILoaderCommunicationManager)),
                   new NetTcpBinding()
                   {
                       OpenTimeout = new TimeSpan(0, 0, 5, 10),
                       SendTimeout = new TimeSpan(0, 0, 5, 10),
                       ReceiveTimeout = TimeSpan.MaxValue,
                       MaxBufferPoolSize = 524288,
                       MaxReceivedMessageSize = 2147483646,
                       Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                       ReliableSession = new OptionalReliableSession() { InactivityTimeout = new TimeSpan(0, 1, 0), Enabled = false }
                   }, new EndpointAddress($"net.tcp://{ip}:{port}/POS/LoaderSystemService")))

        {
            Port = port;
            IP = ip;

            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }

        private bool _Init = false;
        public bool Init
        {
            get { return _Init; }
            set { _Init = value; }
        }

        public bool LoaderInitService(int cellnum)
        {
            bool retVal = false;
            try
            {
                Channel.LoaderInitService(cellnum);
                Init = true;
                retVal = true;
            }
            catch (Exception err)
            {
                if (Init)
                {
                    LoggerManager.Error(err.ToString());
                }
            }

            return retVal;
        }
        public void DeInitService()
        {
            try
            {
                Close();

                if (IsOpened())
                {
                    Abort();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Abort();
            }
        }
        public bool IsOpened()
        {
            bool retVal = false;
            if (State == CommunicationState.Opened || State == CommunicationState.Created)
                retVal = true;

            return retVal;
        }

    }
}

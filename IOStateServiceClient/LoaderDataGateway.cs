using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LoaderRemoteMediatorModule
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class LoaderDataGatewayHost : ILoaderDataGateway
    {
        public bool Initialized { get; set; }
        public Dictionary<int, ILoaderDataGatewayHostCallback> Clients { get; set; } = new Dictionary<int, ILoaderDataGatewayHostCallback>();
        private ServiceHost idServiceHost;
        private ILoaderSupervisor LoaderMaster { get; set; }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (!Initialized)
                {
                    Initialized = true;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void DeInitModule()
        {
            return;
        }

        #region <remarks> Communication Method </remarks>

        public string InitService(int port, string ip = null)
        {
            //string baseURI = $"net.tcp://localhost";
            string baseURI = $"net.tcp://{ip}";
            string listenAbsURI = "";
            try
            {
                if (idServiceHost != null)
                {
                    idServiceHost.Abort();
                }

                Task task = new Task(() =>
                {
                    idServiceHost = new ServiceHost(this, new Uri[] { new Uri($"{baseURI}:{port}/POS") });
                    var netTcpBinding = new NetTcpBinding()
                    {
                        ReceiveTimeout = TimeSpan.MaxValue,
                        SendTimeout = new TimeSpan(0, 10, 0),
                        OpenTimeout = new TimeSpan(0, 10, 0),
                        CloseTimeout = new TimeSpan(0, 10, 0),
                        MaxBufferPoolSize = 1024768,
                        MaxReceivedMessageSize = 2147483646,
                        ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }

                    };
                    netTcpBinding.Security.Mode = SecurityMode.None;
                    idServiceHost.AddServiceEndpoint(typeof(ILoaderDataGateway), netTcpBinding, ServiceAddress.DataGatewayService);
                    idServiceHost.Open();
                    listenAbsURI = idServiceHost.Description.Endpoints[0].ListenUri.AbsoluteUri;
                    idServiceHost.Faulted += ServiceHost_Faulted;
                });
                task.Start();
                task.Wait();

                Autofac.IContainer cont = this.GetLoaderContainer();
                LoaderMaster = cont?.Resolve<ILoaderSupervisor>();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitService error. Err = {err.Message}");
            }

            return listenAbsURI;
        }

        public void InitService(int chuckId)
        {
            try
            {
                OperationContext.Current.Channel.Faulted += Channel_Faulted;
                if (Clients.ContainsKey(chuckId))
                    Clients.Remove(chuckId);
                ILoaderDataGatewayHostCallback callback = OperationContext.Current.GetCallbackChannel<ILoaderDataGatewayHostCallback>();
                Clients.Add(chuckId, callback);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            try
            {
                if (Clients.ContainsValue((ILoaderDataGatewayHostCallback)sender))
                {
                    foreach (var keyvaluepair in Clients)
                    {
                        if (ReferenceEquals(keyvaluepair.Value, (ILoaderDataGatewayHostCallback)sender))
                        {
                            //minskim// faulted event 발생전에 이미 재연결된 경우가 있을 수 있다.
                            if ((keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Faulted || (keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Closed)
                            {
                                LoggerManager.Debug($"LoaderDataGateway Host Callback Channel faulted. Sender = {sender}, cell index = {keyvaluepair.Key}");
                                Clients.Remove(keyvaluepair.Key);
                            }
                            else
                            {
                                LoggerManager.Debug($"Ignore LoaderDataGateway Host Callback Channel faulted. Sender = {sender}, Already Reconnected");
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ServiceHost_Faulted(object sender, EventArgs e)
        {
            try
            {
                LoggerManager.Debug("LoaderDataGatewayHost channel Faulted. Try Reopen");
                (idServiceHost as ICommunicationObject).Open();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum Disconnect(int index = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int clientIndex = -1;
                foreach (var client in Clients)
                {
                    if (client.Key == index)
                    {
                        if ((client.Value as ICommunicationObject).State != CommunicationState.Faulted
                            && (client.Value as ICommunicationObject).State != CommunicationState.Closed)
                        {
                            client.Value.DisConnect();
                            clientIndex = client.Key;
                            retVal = EventCodeEnum.NONE;
                            break;
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderDataGateway Host Callback Channel client #[{client.Key}] Already Close.");
                        }
                    }
                }

                if (clientIndex != -1)
                {
                    Clients.Remove(clientIndex);
                    LoggerManager.Debug($"LoaderDataGateway Host Callback Channel close. cell index = {clientIndex}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #endregion

        public ILoaderDataGatewayHostCallback GetDataGatewayHostClient(int index = -1)
        {
            ILoaderDataGatewayHostCallback client = null;
            try
            {
                if (index != -1)
                {
                    if (Clients.ContainsKey(index))
                    {
                        Clients.TryGetValue(index, out client);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return client;
        }

        public EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (LoaderMaster != null)
                {
                    retVal = LoaderMaster.NotifyStageAlarm(noticeCodeInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}

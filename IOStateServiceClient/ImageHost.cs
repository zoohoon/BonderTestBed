using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Reflection;

namespace IOStateServiceClient
{
    using System.ComponentModel;
    using LogModule;
    using ServiceInterfaces;
    using ProberInterfaces.ServiceHost;
    using ProberErrorCode;
    using System.Threading.Tasks;

    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ImageDispHost: INotifyPropertyChanged, IImageDispHost
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public event ImageUpdatedEventHandler ImageUpdate;
        private ServiceHost idServiceHost;

        public Dictionary<int, IImageDispHostCallback> Clients { get; set; } = new Dictionary<int, IImageDispHostCallback>();

        public string InitService(int port, string ip = null)
        {
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
                        SendTimeout = new TimeSpan(0, 3, 0),
                        OpenTimeout = new TimeSpan(0, 3, 0),
                        CloseTimeout = new TimeSpan(0, 3, 0),
                        MaxBufferPoolSize = 2147483646,
                        MaxReceivedMessageSize = 2147483646,
                        ReliableSession = new OptionalReliableSession()
                        {
                            InactivityTimeout = TimeSpan.FromMinutes(1),
                            Enabled = true
                        }
                    };
                    netTcpBinding.Security.Mode = SecurityMode.None;
                    idServiceHost.AddServiceEndpoint(typeof(IImageDispHost), netTcpBinding, "ImageDispService");
                    idServiceHost.Open();
                    listenAbsURI = idServiceHost.Description.Endpoints[0].ListenUri.AbsoluteUri;
                    idServiceHost.Faulted += ServiceHost_Faulted;
                });
                task.Start();
                task.Wait();

            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitService error. Err = {err.Message}");
            }

            return listenAbsURI;
        }

        private void ServiceHost_Faulted(object sender, EventArgs e)
        {
            try
            {
                LoggerManager.Debug("Image Host channel Faulted. Try Reopen");
                (idServiceHost as ICommunicationObject).Open();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateImage(ImageBuffer img)
        {
            try
            {
                if (ImageUpdate != null) ImageUpdate(img);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        //public bool UpdateImage(ImageBuffer img)
        //{
        //    bool retVal = false;
        //    try
        //    {
        //        if (ImageUpdate != null)
        //        {
        //            retVal = ImageUpdate(img);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}
        public CommunicationState GetState()
        {
            var state = idServiceHost.State;
            return state;
        }

        public bool DeInitService()
        {
            bool retVal = false;
            try
            {
                if(idServiceHost != null)
                {
                    if (idServiceHost.State == CommunicationState.Opened | idServiceHost.State == CommunicationState.Opening)
                        idServiceHost.Close();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void InitService(int chuckId)
        {
            try
            {
                OperationContext.Current.Channel.Faulted += Channel_Faulted;
                if (Clients.ContainsKey(chuckId))
                {
                    Clients.Remove(chuckId);
                }
                Clients.Add(chuckId, OperationContext.Current.GetCallbackChannel<IImageDispHostCallback>());
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
                if (Clients.ContainsValue((IImageDispHostCallback)sender))
                {
                    foreach (var keyvaluepair in Clients)
                    {
                        if (ReferenceEquals(keyvaluepair.Value, (IDelegateEventHostCallback)sender))
                        {
                            //minskim// faulted event 발생전에 이미 재연결된 경우가 있을 수 있다.
                            if ((keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Faulted || (keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Closed)
                            {
                                LoggerManager.Debug($"Image Host Callback Channel faulted. Sender = {sender}, cell index = {keyvaluepair.Key}");
                                Clients.Remove(keyvaluepair.Key);
                            }
                            else
                            {
                                LoggerManager.Debug($"Ignore Dialog Callback Channel faulted. Sender = {sender}, Already Reconnected");
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
                        ICommunicationObject comobj = (client.Value as ICommunicationObject);

                        if (comobj.State != CommunicationState.Faulted && comobj.State != CommunicationState.Closed)
                        {
                            client.Value.DisConnect();
                            clientIndex = client.Key;
                            retVal = EventCodeEnum.NONE;
                            break;
                        }
                        else
                        {
                            LoggerManager.Debug($"Image Host Callback Channel client #[{client.Key}] Already Close.");
                        }
                    }
                }
                if (clientIndex != -1)
                {
                    Clients.Remove(clientIndex);
                    LoggerManager.Debug($"Image Host Callback Channel close. cell index = {clientIndex}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public IImageDispHostCallback GetDispHostClient(int index = -1)
        {
            IImageDispHostCallback client = null;
            try
            {
                if(index != -1)
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
    }

}

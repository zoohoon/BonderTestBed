using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IOStateServiceClient
{
    using LogModule;
    using ProberInterfaces;
    using System.ServiceModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ProberErrorCode;
    using System.Threading;
    using System.Reflection;
    using MetroDialogInterfaces;
    using System.Windows.Input;
    using Autofac;
    using LoaderBase;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DelegateEventHost : INotifyPropertyChanged, IDelegateEventHost, IFactoryModule, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public event MessageDialogShowEventHandler MessageDialogShow;

        public event MetroDialogShowEventHandler MetroDialogShowEvent;
        public event MetroDialogCloseEventHandler MetroDialogCloseEvent;
        //public event MetroDialogShowChuckIndexEventHandler MetroDialogShowChuckIndexEvent;
        //public event MetroDialogCloseChuckIndexEventHandler MetroDialogCloseChuckIndexEvent;

        //public event ProgressDialogShowEventHandler ProgressDialogShow;
        //public event ProgressDialogCloseEventHandler ProgressDialogClose;
        //public event ProgressDialogCloseAllEventHandler ProgressDialogCloseAll;
        //public event ProgressDialogCancelEventHandler ProgressDialogCancel;

        public event SingleInputDialogShowEventHandler SingleInputDialogShow;
        public event SingleInputGetInputDataEventHandler SingleInputDialogGetInputData;


        //public event SoakDialogCloseEventHandler SoakDialogCloseEvent;
        //public event SoakgDialogShowEventHandler SoakDialogShowEvent;

        private ServiceHost idServiceHost;

        public Dictionary<int, IDelegateEventHostCallback> Clients { get; set; } = new Dictionary<int, IDelegateEventHostCallback>();

        public InitPriorityEnum InitPriority => throw new NotImplementedException();
        private ILoaderSupervisor LoaderMaster { get; set; }

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
                    idServiceHost.AddServiceEndpoint(typeof(IDelegateEventHost), netTcpBinding, "DelegateEventHost");
                    idServiceHost.Open();
                    listenAbsURI = idServiceHost.Description.Endpoints[0].ListenUri.AbsoluteUri;
                    idServiceHost.Faulted += ServiceHost_Faulted;
                });
                task.Start();
                task.Wait();

                Autofac.IContainer cont  = this.GetLoaderContainer();
                LoaderMaster = cont?.Resolve<ILoaderSupervisor>();
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
                LoggerManager.Debug("Dialog Host channel Faulted. Try Reopen");
                (idServiceHost as ICommunicationObject).Open();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EnumMessageDialogResult> ShowMessageDialog(string Title, string Message, EnumMessageStyle enummessagesytel, string AffirmativeButtonText = "OK", string NegativeButtonText = "Cancel", string firstAuxiliaryButtonText = null, string secondAuxiliaryButtonText = null)
        {
            try
            {
                if (MessageDialogShow != null)
                {
                    return await MessageDialogShow(Title, Message, enummessagesytel, AffirmativeButtonText, NegativeButtonText, firstAuxiliaryButtonText, secondAuxiliaryButtonText);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EnumMessageDialogResult.UNDEFIND;
        }

        public async Task ShowMetroDialog(string viewAssemblyName, string viewClassName,
            string viewModelAssemblyName, string viewModelClassName, List<byte[]> data = null, bool waitunload = true)
        {
            try
            {
                if (MetroDialogShowEvent != null) await MetroDialogShowEvent(viewAssemblyName, viewClassName, viewModelAssemblyName, viewModelClassName, data, waitunload);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task CloseMetroDialog(string classname = null)
        {
            try
            {
                if (MetroDialogCloseEvent != null) await MetroDialogCloseEvent(classname);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ShowWaitCancelDialog(string hashcode, string message, CancellationTokenSource canceltokensource = null, string caller = "", string cancelButtonText = "Cancel")
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(hashcode, message, canceltokensource: canceltokensource, caller:caller, localonly:false, cancelButtonText: cancelButtonText);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        public async Task CloseWaitCancelDialog(string hashcode)
        {
            try
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(hashcode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public async Task SetDataWaitCancelDialog(string message, string hashcoe, CancellationTokenSource canceltokensource = null, string cancelButtonText = "Cancel", bool restarttimer = false)
        {
            try
            {
                await this.MetroDialogManager().SetDataWaitCancelDialog(message,  hashcoe, canceltokensource, cancelButtonText, restarttimer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ClearDataWaitCancelDialog(bool restarttimer = false)
        {
            try
            {
                await this.MetroDialogManager().ClearDataWaitCancelDialog(restarttimer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EnumMessageDialogResult> ShowSingleInputDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel", string subLabel = "")
        {
            EnumMessageDialogResult retval = EnumMessageDialogResult.UNDEFIND;

            try
            {
                if (SingleInputDialogShow != null)
                {
                    retval = await SingleInputDialogShow(Label, posBtnLabel, negBtnLabel, subLabel);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public string GetInputDataSingleInput()
        {
            try
            {
                if (SingleInputDialogGetInputData != null)
                {
                    return SingleInputDialogGetInputData();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return string.Empty;
        }


        public void InitService(int chuckId)
        {
            try
            {
                OperationContext.Current.Channel.Faulted += Channel_Faulted;
                if (Clients.ContainsKey(chuckId))
                    Clients.Remove(chuckId);
                IDelegateEventHostCallback callback = OperationContext.Current.GetCallbackChannel<IDelegateEventHostCallback>();
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
                if (Clients.ContainsValue((IDelegateEventHostCallback)sender))
                {
                    foreach (var keyvaluepair in Clients)
                    {
                        if (ReferenceEquals(keyvaluepair.Value, (IDelegateEventHostCallback)sender))
                        {
                            //minskim// faulted event 발생전에 이미 재연결된 경우가 있을 수 있다.
                            if ((keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Faulted || (keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Closed)
                            {
                                LoggerManager.Debug($"Dialog Host Callback Channel faulted. Sender = {sender}, cell index = {keyvaluepair.Key}");
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
                            LoggerManager.Debug($"Dialog Host Callback Channel client #[{client.Key}] Already Close.");
                        }
                    }
                }

                if (clientIndex != -1)
                {
                    Clients.Remove(clientIndex);
                    LoggerManager.Debug($"Dialog Host Callback Channel close. cell index = {clientIndex}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public IDelegateEventHostCallback GetDialogHostClient(int index = -1)
        {
            IDelegateEventHostCallback client = null;
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
                if(LoaderMaster != null)
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
        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            try
            {
                if (idServiceHost != null)
                {
                    if (idServiceHost.State == CommunicationState.Opened || idServiceHost.State == CommunicationState.Opening || idServiceHost.State == CommunicationState.Created)
                    {
                        idServiceHost.Close();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


    }
}

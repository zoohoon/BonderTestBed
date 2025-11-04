using System.Collections.Generic;
using System.Linq;

namespace EnvControlModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading.Tasks;

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class EnvControlServiceHost : IEnvControlService
    {
        public IEnvController Manager { get; set; }
        private ServiceHost ServiceHost = null;
        // Cell Callback 객체입니다.
        public Dictionary<long, IEnvControlServiceCallback> DicServiceCallback
            = new Dictionary<long, IEnvControlServiceCallback>();
        public bool Initialized { get; set; } = false;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(!Initialized)
                {
                    retVal = InitServiceHost("localhost", ServicePort.EnvControlServicePort);

                    Initialized = true;
                }
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

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (!Initialized)
                {
                    retVal = InitServiceHost("localhost", ServicePort.EnvControlServicePort);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DisConnect(int index = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                long clientIndex = -1;
                foreach (var client in DicServiceCallback)
                {
                    if (client.Key == index)
                    {
                        ICommunicationObject comobj = (client.Value as ICommunicationObject);

                        if (comobj.State != CommunicationState.Faulted && comobj.State != CommunicationState.Closed)
                        {
                            //loader client disconnect시 cell에서 disconnect가 호출되게 된다. 로더에서 할 필요 없음 dic객체만 삭제 해줌
                            //client.Value.DisConnect(index);
                            clientIndex = client.Key;
                            retVal = EventCodeEnum.NONE;
                            break;
                        }
                        else
                        {
                            LoggerManager.Debug($"ENVControlServiceHost Callback Channel client #[{client.Key}] Already Close.");
                        }
                    }
                }
                if (clientIndex != -1)
                {
                    DicServiceCallback.Remove(clientIndex);
                    LoggerManager.Debug($"ENVControlServiceHost Callback Channel close. cell index = {clientIndex}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// port : 8423
        /// </summary>
        public EventCodeEnum InitServiceHost(string ip, int port)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ServiceMetadataBehavior serviceMetadataBehavior = null;
                ServiceDebugBehavior debugBehavior = null;
                string localURI = $"net.tcp://{ip}:{port}/envcontrolpipe";

                Task task = new Task(() =>
                {
                    var netTcpBinding = new NetTcpBinding()
                    {
                        MaxBufferPoolSize = 2147483647,
                        MaxBufferSize = 2147483647,
                        MaxReceivedMessageSize = 2147483647,
                        SendTimeout = new TimeSpan(0, 5, 0),
                        ReceiveTimeout = TimeSpan.MaxValue,
                        OpenTimeout = new TimeSpan(0, 10, 0),
                        CloseTimeout = new TimeSpan(0, 10, 0),
                        ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                    };

                    netTcpBinding.Security.Mode = SecurityMode.None;
                    ServiceHost = new ServiceHost(this);
                    ServiceHost.AddServiceEndpoint(typeof(IEnvControlService), netTcpBinding, localURI);

                    debugBehavior = ServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
                    if (debugBehavior != null)
                    {
                        debugBehavior.IncludeExceptionDetailInFaults = true;
                    }

                    serviceMetadataBehavior = ServiceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                    if (serviceMetadataBehavior == null)
                        serviceMetadataBehavior = new ServiceMetadataBehavior();

                    serviceMetadataBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                    ServiceHost.Description.Behaviors.Add(serviceMetadataBehavior);

                    ServiceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                        MetadataExchangeBindings.CreateMexTcpBinding(),
                        $"{localURI}/mex"
                        );

                    ServiceHost.Open();
                    ServiceHost.Faulted += ServiceHost_Faulted;
                });
                task.Start();
                task.Wait();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void ServiceHost_Faulted(object sender, EventArgs e)
        {
            try
            {
                LoggerManager.Debug("ENV Host channel Faulted. Try Reopen");
                (ServiceHost as ICommunicationObject).Open();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void InitService(int stageIndex = 0)
        {
            try
            {
                var serviceCallbackObj = OperationContext.Current.GetCallbackChannel<IEnvControlServiceCallback>();

                if (serviceCallbackObj != null)
                {
                    if (DicServiceCallback.ContainsKey(stageIndex))
                    {
                        DicServiceCallback.Remove(stageIndex);
                    }
                    DicServiceCallback.Add(stageIndex, serviceCallbackObj);
                }
                LoggerManager.Debug($"InitEnvControlServiceProxy #[{stageIndex}]");
                (serviceCallbackObj as ICommunicationObject).Faulted += Channel_Faulted;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType().Name}] InitService() - Failed ENV Host Callback Channel Init (Exception : {err}");
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
                if(DicServiceCallback.ContainsValue((IEnvControlServiceCallback)sender))
                {
                    foreach (var keyvaluepair in DicServiceCallback)
                    {
                        if (ReferenceEquals(keyvaluepair.Value, (IEnvControlServiceCallback)sender))
                        {
                            //minskim// faulted event 발생전에 이미 재연결된 경우가 있을 수 있다.
                            if ((keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Faulted || (keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Closed)
                            {
                                LoggerManager.Debug($"ENV Host Callback Channel faulted. Sender = {sender}, cell index = {keyvaluepair.Key}");
                                DicServiceCallback.Remove(keyvaluepair.Key);
                            }
                            else
                            {
                                LoggerManager.Debug($"Ignore ENV Callback Channel faulted. Sender = {sender}, Already Reconnected");
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

        public bool IsUsingDryAir(int stageindex = -1)
        {
            return Manager?.IsUsingDryAir(stageindex) ?? false;
        }

        public bool IsUsingChiller(int stageindex = -1)
        {
            return Manager?.IsUsingChiller(stageindex) ?? false;
        }

        public IEnvControlServiceCallback GetEnvControlClient(int stageindex = -1)
        {
            IEnvControlServiceCallback client = null;
            try
            {
                DicServiceCallback.TryGetValue(stageindex, out client);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return client;
        }

        public bool GetIsExcute()
        {
            return true;
        }

        #region ... Valve
        public EventCodeEnum SetValveState(bool state, EnumValveType valveType, int stageIndex = -1)
        {
            try
            {
                return Manager.SetValveState(state, valveType, stageIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.VALVE_SET_ERROR;
            }
        }
        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            try
            {
                return Manager.GetValveState(valveType, stageIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }
        #endregion

        #region ... Dry Air
        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            return Manager.DryAirForProber(value, dryairType, stageIndex);
        }

        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            return Manager.GetLeakSensor(out value, leakSensorIndex, stageindex);
        }
        public byte[] GetDryAirParam(int stageindex = -1)
        {
            return Manager.GetDryAirParam(stageindex);
        }
        #endregion

        #region ... FFU
        public void RaiseFFUAlarm(string alarmmessage)
        {
            try
            {
                Manager?.RaiseFFUAlarm(alarmmessage);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}

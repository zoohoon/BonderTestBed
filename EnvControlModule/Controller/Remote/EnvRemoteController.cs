using System;
using System.Collections.Generic;

namespace EnvControlModule
{
    using CommunicationModule;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using SerializerUtil;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class EnvRemoteController : IEnvController, IEnvControlServiceCallback
    {
        #region //..Property
        public bool Initialized { get; set; }

        public EnvControlServiceDirectProxy EnvControlServiceProxy { get; set; }

        public EnvControlServiceDirectProxy GetEnvControlServiceProxy()
        {
            EnvControlServiceDirectProxy proxy = null;

            try
            {
                if (EnvControlServiceProxy?.IsServiceAvailable() == true)
                {
                    proxy = EnvControlServiceProxy;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return proxy;
        }

        private Dictionary<EnumValveType, bool> _ValveStates
            = new Dictionary<EnumValveType, bool>();
        public Dictionary<EnumValveType, bool> ValveStates
        {
            get { return _ValveStates; }
            set
            {
                if (value != _ValveStates)
                {
                    _ValveStates = value;
                }
            }
        }
        private AutoResetEvent areUpdateEvent = new AutoResetEvent(false);
        private Thread UpdateThread = null;
        private bool bIsUpdating = true;
        #endregion

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InitConnect();

                ValveStates.Add(EnumValveType.IN, false);
                ValveStates.Add(EnumValveType.OUT, false);
                ValveStates.Add(EnumValveType.DRYAIR, false);
                ValveStates.Add(EnumValveType.DRAIN, false);
                ValveStates.Add(EnumValveType.PURGE, false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (EnvControlServiceProxy != null)
                {
                    //해당 함수는 새로 연결 요청시 호출된다. 그러므로 기존 채널이 있다는 것은 abnormal channel 이다.
                    EnvControlServiceProxy?.Abort();
                    EnvControlServiceProxy = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            retVal = ConnectService();// 셀쪽에서 EnvControl의 Callback을 요청함.
            if (retVal == EventCodeEnum.NONE)
            {
                if (UpdateThread?.IsAlive == true)
                {
                    //log 추가 필요
                    LoggerManager.Debug($"[{this.GetType().Name}] UpdateThread abnormal state when start ");
                    PrepareThreadStop();
                    UpdateThread?.Abort();
                    UpdateThread = null;
                }

                bIsUpdating = true;
                UpdateThread = new Thread(new ThreadStart(UpdateEvnData));
                UpdateThread.Name = this.GetType().Name;
                UpdateThread.Start();
            }
            return retVal;
        }
        public EventCodeEnum ConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LoaderController().GetconnectFlag() == false)
                {
                    return retVal;
                }
                if (CommunicationManager.CheckAvailabilityCommunication(this.LoaderController().GetLoaderIP(), ServicePort.EnvControlServicePort))
                {
                    var statecheck = (EnvControlServiceProxy as ICommunicationObject)?.State == CommunicationState.Closed
                          || (EnvControlServiceProxy as ICommunicationObject)?.State == CommunicationState.Faulted;
                    if (EnvControlServiceProxy == null || statecheck)
                    {
                        var context = new InstanceContext(this);
                        Binding binding = new NetTcpBinding()
                        {
                            Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                            MaxBufferPoolSize = 2147483647,
                            MaxBufferSize = 2147483647,
                            MaxReceivedMessageSize = 2147483647,
                            SendTimeout = new TimeSpan(0, 5, 0),
                            ReceiveTimeout = TimeSpan.MaxValue,
                            ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                        };

                        try
                        {
                            EndpointAddress endpointAddress = new EndpointAddress($"net.tcp://{this.LoaderController().GetLoaderIP()}:{ServicePort.EnvControlServicePort}/envcontrolpipe");
                            var contract = ContractDescription.GetContract(typeof(IEnvControlService));
                            var serviceEndpoint = new ServiceEndpoint(contract, binding, endpointAddress);
                            EnvControlServiceProxy = new EnvControlServiceDirectProxy(context, serviceEndpoint);
                            var duplex = new DuplexChannelFactory<IEnvControlService>(context, binding, endpointAddress);
                            var clientCallback = duplex.CreateChannel(context);
                            clientCallback.InitService(this.LoaderController().GetChuckIndex());
                            LoggerManager.Debug($"[{this.GetType().Name}] ConnectSuccess() - EnvControlProxy Init");

                            EnvControlServiceProxy.ChannelFactory.Closed += Channel_Closed;
                            EnvControlServiceProxy.ChannelFactory.Faulted += Channel_Faulted;

                            retVal = EventCodeEnum.NONE;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}] ConnectFailed() - Failed EnvControlProxy Init (Exception : {err}");
                            EnvControlServiceProxy = null;
                        }
                    }
                    else
                    {
                        //연결된 상태
                        LoggerManager.Debug($"[{this.GetType().Name}] ConnectSuccess() - EnvControlProxy Alerady connected");
                        retVal = EventCodeEnum.ENVCONTROL_ALREADY_CONNECT;
                    }

                }
            }
            catch (Exception err)
            {
                EnvControlServiceProxy = null;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void Channel_Closed(object sender, EventArgs e)
        {
            LoggerManager.Debug($"ENV channel Closed. Sender = {sender}");
        }

        public void Channel_Faulted(object sender, EventArgs e)
        {
            LoggerManager.Debug($"ENV channel Faulted. Sender = {sender}");
        }

        public void DeInitModule()
        {
            try
            {
                PrepareThreadStop();
                if (UpdateThread?.IsAlive == true)
                {
                    UpdateThread?.Join();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return;
        }
        public EventCodeEnum DisConnect(int index = -1)
        {
            //cell이므로 index는 무시해도 된다.
            PrepareThreadStop();

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                EnvControlServiceProxy?.DisConnect();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                EnvControlServiceProxy = null;
            }
            return retVal;
        }

        public bool IsAlive()
        {
            lock(EnvControlServiceDirectProxy.proxyLock)
            {
                return true;
            }
        }
        public bool GetIsExcute()
        {
            try
            {
                if (GetEnvControlServiceProxy() != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public IEnvControlServiceCallback GetEnvControlClient(int stageindex = -1)
        {
            lock (EnvControlServiceDirectProxy.proxyLock)
            {
                return null;
            }
        }
        public bool IsUsingChiller(int stageindex = -1)
        {
            bool retVal = false;
            try
            {
                retVal = GetEnvControlServiceProxy()?.IsUsingChiller() ?? false;
            }
            catch (Exception)
            {
            }
            return retVal;
        }

        public bool IsUsingDryAir(int stageindex = -1)
        {
            bool retVal = false;
            try
            {
                retVal = GetEnvControlServiceProxy()?.IsUsingDryAir() ?? false;
            }
            catch (Exception)
            {
            }
            return retVal;
            
        }

        public double GetDewPointVal()
        {
           return this.EnvControlManager().GetDewPointModule()?.CurDewPoint ?? 9999;
        }

        #region ... Chiller & Temp

        public double GetChillerTargetTemp()
        {
            lock (EnvControlServiceDirectProxy.proxyLock)
            {
                return this.EnvControlManager().GetChillerModule()?.ChillerInfo.TargetTemp ?? 9999;
            }
        }

        public double GetTempTargetTemp()
        {
            lock (EnvControlServiceDirectProxy.proxyLock)
            {
                return this.TempController().TempInfo.TargetTemp.Value;
            }
        }
        public bool GetChillerActiveState()
        {
            //return this.EnvControlManager().GetChillerModule()?.ChillerInfo.ChillerActiveStage ?? false;
            bool retVal = false;
            try
            {
                lock (EnvControlServiceDirectProxy.proxyLock)
                {
                    var lotstate = this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE;

                    retVal = !(lotstate);
                }

            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetChillerAbortMode(bool flag)
        {
            try
            {
                lock (EnvControlServiceDirectProxy.proxyLock)
                {
                    if (this.EnvControlManager().GetChillerModule() != null)
                    {
                        this.EnvControlManager().GetChillerModule().ChillerInfo.AbortChiller = flag;
                        LoggerManager.Debug($"[Chiller AbortFlag] : Change chiller abortfalg is {flag}");
                    }
                } 
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public RemoteStageColdSetupData GetRemoteColdData()
        {
            RemoteStageColdSetupData data = new RemoteStageColdSetupData();
            try
            {
                //data.DewPointTolerance = this.EnvControlManager().GetDewPointModule().DewPointOffset;
                data.DewPointTolerance = this.EnvControlManager().GetDewPointModule().Tolerence;
                data.DryAirActivatableHighTemp = this.EnvControlManager().GetDryAirModule().DryAirActivableHighTemp;
                data.DewPointTimeOut = this.EnvControlManager().GetDewPointModule().WaitTimeout;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return data;
        }

        public void SetRemoteColdData(RemoteStageColdSetupData remotedata)
        {
            try
            {
                lock (EnvControlServiceDirectProxy.proxyLock)
                {
                    if (this.EnvControlManager().GetDewPointModule().WaitTimeout != (long)remotedata.DewPointTimeOut
                        || this.EnvControlManager().GetDewPointModule().Tolerence != remotedata.DewPointTolerance)
                    {
                        this.EnvControlManager().GetDewPointModule().Tolerence = remotedata.DewPointTolerance;
                        this.EnvControlManager().GetDewPointModule().WaitTimeout = (long)remotedata.DewPointTimeOut;
                        this.EnvControlManager().GetDewPointModule().SaveSysParameter();
                    }
                    if (this.EnvControlManager().GetDryAirModule().DryAirActivableHighTemp != remotedata.DryAirActivatableHighTemp)
                    {
                        this.EnvControlManager().GetDryAirModule().DryAirActivableHighTemp = remotedata.DryAirActivatableHighTemp;
                        this.EnvControlManager().SaveSysParameter();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetChillerData(byte[] chillerparam, bool setremotechange = false)
        {
            try
            {
                lock (EnvControlServiceDirectProxy.proxyLock)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(chillerparam, out target, typeof(ChillerParameter));
                    this.EnvControlManager().GetChillerModule()?.InitParam((IChillerParameter)target, setremotechange);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ... Valve
        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            bool retVal = false;
            try
            {
                retVal = ValveStates[valveType];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetValveState(bool enableFlag, EnumValveType valveType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = GetEnvControlServiceProxy()?.SetValveState(enableFlag, valveType, stageIndex) ?? EventCodeEnum.ENVCONTROL_COMM_ERROR;
            }
            catch (Exception)
            {
                retVal = EventCodeEnum.ENVCONTROL_COMM_ERROR;
            }
            return retVal;
        }
        #endregion

        #region ... Dry Air
        public byte[] GetDryAirParam(int stageindex = -1)
        {
            byte[] param = null;
            try
            {
                stageindex = this.LoaderController().GetChuckIndex();
                param = GetEnvControlServiceProxy()?.GetDryAirParam(stageindex) ?? null;
            }
            catch (Exception)
            {
            }
            return param;
        }
        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                stageIndex = this.LoaderController().GetChuckIndex();
                retVal = GetEnvControlServiceProxy()?.DryAirForProber(value, dryairType, stageIndex) ?? EventCodeEnum.ENVCONTROL_NOT_CONNECTED;
            }
            catch (Exception)
            {
                retVal = EventCodeEnum.ENVCONTROL_NOT_CONNECTED;
            }
            return retVal;
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            int retVal = -1;
            try
            {
                stageindex = this.LoaderController().GetChuckIndex();
                retVal = GetEnvControlServiceProxy().GetLeakSensor(out value, leakSensorIndex, stageindex);
            }
            catch (Exception err)
            {
                value = false;
                retVal = -1;
                throw err;
            }
            return retVal;
        }
        #endregion

        #region ... FFU
        public void RaiseFFUAlarm(string alarmmessage)
        {
            try
            {
                GetEnvControlServiceProxy()?.RaiseFFUAlarm(alarmmessage);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        private void UpdateEvnData()
        {
            try
            {
                while(bIsUpdating)
                {
                    try
                    {
                        if (!bIsUpdating)
                        {
                            break;
                        }

                        try
                        {
                            ValveStates[EnumValveType.IN] = GetEnvControlServiceProxy()?.GetValveState(EnumValveType.IN) ?? false;
                        }
                        catch (Exception)
                        {
                        }
                        
                        areUpdateEvent.WaitOne(500);
                        if (!bIsUpdating)
                        {
                            break;
                        }
                        try
                        {
                            ValveStates[EnumValveType.OUT] = GetEnvControlServiceProxy()?.GetValveState(EnumValveType.OUT) ?? false;
                        }
                        catch (Exception)
                        {
                        }
                        
                        areUpdateEvent.WaitOne(500);
                        if (!bIsUpdating)
                        {
                            break;
                        }

                        try
                        {
                            ValveStates[EnumValveType.DRYAIR] = GetEnvControlServiceProxy()?.GetValveState(EnumValveType.DRYAIR) ?? false;
                        }
                        catch (Exception)
                        {
                        }
                        
                        areUpdateEvent.WaitOne(500);

                        if (!bIsUpdating)
                        {
                            break;
                        }
                        areUpdateEvent.WaitOne(300);
                    }
                    catch (Exception err)
                    {
                        if (err is ThreadAbortException)
                        {
                            throw err;
                        }
                        // 정상적인 disconnect 및 강제 종료상황이 아닌 경우에는 thread를 중지 하지 않고 계속 동작 시키도록 한다.
                        LoggerManager.Exception(err);
                    }
                }
                UpdateThread = null;
            }
            catch (Exception err)
            {
                if (err is ThreadAbortException)
                {
                    //loader(cell에서)에 새로 연결 하는 시점에서 기존 동작 중인 thread를 abort한 경우 임, UpdateThread null 할당은 abort시 처리한다.
                    LoggerManager.Debug($"EnvRemoteController.UpdateProc(): Thread Abort ENVControl");
                }
                else
                {
                    UpdateThread = null;
                    LoggerManager.Exception(err);
                }
            }
        }

        private void PrepareThreadStop()
        {
            bIsUpdating = false;
            areUpdateEvent.Set();
        }
    }
}

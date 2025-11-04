using System;
using System.Collections.Generic;

namespace StageCommunicationModule
{
    using Autofac;
    using LogModule;
    using ProberInterfaces;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using System.ServiceModel;
    using ProberInterfaces.ViewModel;
    using RemoteServiceProxy;
    using System.Transactions;
    using StageCommunicationModule.CallBack;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.PMI;
    using System.ServiceModel.Description;
    using System.Xml;
    using ProberInterfaces.PinAlign;
    using ProberInterfaces.Retest;
    using System.Threading.Tasks;

    public class StageCommunicationManager : IStageCommunicationManager, IFactoryModule, IHasDevParameterizable, INotifyPropertyChanged, IDisposable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region //..Property
        private ImageDispHostCallback ImageDispHostCallback = new ImageDispHostCallback();
        private DelegateEventHostCallback DelegateEventHostCallback = new DelegateEventHostCallback();
        private DataGatewayHostCallback DataGatewayHostCallback = new DataGatewayHostCallback();

        public const string baseURI = "net.tcp://localhost";

        public StageCommunicationParameter CommParam { get; set; }
        private IIOManager IOState => this.IOManager();
        private IMotionManager MotionManager => this.MotionManager();
        private IStageSupervisor StageSupervisor => this.StageSupervisor();
        private ILoaderRemoteMediator LoaderRemoteMediator { get; set; }
        private ServiceHost StageServiceHost { get; set; }
        private ServiceHost StageMoveHost { get; set; }
        private ServiceHost IOServiceHost { get; set; }
        private ServiceHost MotionServiceHost { get; set; }
        private ServiceHost LoaderRemoteMediatorHost { get; set; }
        private ServiceHost GPCCViewModelHost { get; set; }
        private ServiceHost TempConHost { get; set; }

        private ServiceHost RetestModuleHost { get; set; }
        private ServiceHost PolishWaferModuleHost { get; set; }
        private ServiceHost FileManagerHost { get; set; }
        private ServiceHost LotOPModuleHost { get; set; }
        private ServiceHost SoakingModuleHost { get; set; }

        private ServiceHost PMIModuleHost { get; set; }

        private ServiceHost paramManagerConHost { get; set; }
        private ServiceHost CoordinateManagerHost { get; set; }
        private ServiceHost WAModuleHost { get; set; }

        private ServiceHost PAModuleHost { get; set; }


        private readonly Dictionary<Type, object> factories;

        private DelegateEventProxy DelegateEventProxy { get; set; }
        private DataGatewayProxy DataGatewayProxy { get; set; }
        public bool Initialized { get; set; }
        #endregion

        public StageCommunicationManager()
        {
            factories = new Dictionary<Type, object>();
        }

        #region //..IModule Method
        public EventCodeEnum InitModule()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 200;

            if (!Initialized)
                Initialized = true;
            return EventCodeEnum.NONE;
        }


        public void DeInitModule()
        {
            try
            {
                DeInitService();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Dispose()
        {
            //DeInitModule();
        }

        #endregion

        #region //..IHasDevParameterizable Method
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = new StageCommunicationParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                this.LoadParameter(ref tmpParam, typeof(StageCommunicationParameter));
                this.CommParam = tmpParam as StageCommunicationParameter;

                if (this.CommParam != null)
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion


        #region //..Method

        private EventCodeEnum CreateServiceHost<T>(object singletoninstance, int port, int portoffset, string address, ServiceHost servicehost, XmlDictionaryReaderQuotas readerquotas = null, EventHandler ClosedEventHandler = null, EventHandler FaultedEventHandler = null,
        TimeSpan? sendtimeout = null,
        TimeSpan? receivetimeout = null,
        TimeSpan? opentimeout = null,
        TimeSpan? closetimeout = null,
        TimeSpan? inactivitytimeout = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Type type = typeof(T);

                if (singletoninstance == null)
                    return retval;

                var serviceHostOverTCP = new ServiceHost(singletoninstance, new Uri[] { new Uri($"{baseURI}:{port + portoffset}/POS") });

                NetTcpBinding tcpBinding = new NetTcpBinding();

                if (sendtimeout == null)
                {
                    tcpBinding.SendTimeout = new TimeSpan(0, 5, 0);
                }
                else
                {
                    tcpBinding.SendTimeout = (TimeSpan)sendtimeout;
                }

                if (receivetimeout == null)
                {
                    tcpBinding.ReceiveTimeout = TimeSpan.MaxValue;
                }
                else
                {
                    tcpBinding.ReceiveTimeout = (TimeSpan)receivetimeout;
                }

                if (opentimeout == null)
                {
                    tcpBinding.OpenTimeout = new TimeSpan(0, 10, 0);
                }
                else
                {
                    tcpBinding.OpenTimeout = (TimeSpan)opentimeout;
                }

                if (closetimeout == null)
                {
                    tcpBinding.CloseTimeout = new TimeSpan(0, 10, 0);
                }
                else
                {
                    tcpBinding.OpenTimeout = (TimeSpan)closetimeout;
                }

                tcpBinding.MaxBufferPoolSize = 524288;
                tcpBinding.MaxReceivedMessageSize = 2147483646;

                if (readerquotas != null)
                {
                    tcpBinding.ReaderQuotas = readerquotas;
                }

                if (inactivitytimeout == null)
                {
                    tcpBinding.ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true };
                }
                else
                {
                    tcpBinding.ReliableSession = new OptionalReliableSession() { InactivityTimeout = (TimeSpan)inactivitytimeout, Enabled = true };
                }


                //NetTcpBinding tcpBinding = new NetTcpBinding()
                //{
                //    SendTimeout = new TimeSpan(0, 5, 0),
                //    ReceiveTimeout = TimeSpan.MaxValue,
                //    OpenTimeout = new TimeSpan(0, 10, 0),
                //    CloseTimeout = new TimeSpan(0, 10, 0),
                //    MaxBufferPoolSize = 524288,
                //    MaxReceivedMessageSize = 2147483646,

                //    ReaderQuotas = readerquotas,

                //    ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1) }
                //};

                tcpBinding.Security.Mode = SecurityMode.None;

                if (ClosedEventHandler != null)
                {
                    serviceHostOverTCP.Closed += ClosedEventHandler;
                }

                if (FaultedEventHandler != null)
                {
                    serviceHostOverTCP.Faulted += FaultedEventHandler;
                }
                else
                {
                    serviceHostOverTCP.Faulted += Channel_Faulted<T>;
                }

                serviceHostOverTCP.AddServiceEndpoint(type, tcpBinding, address);

                ServiceDebugBehavior debug = serviceHostOverTCP.Description.Behaviors.Find<ServiceDebugBehavior>();

                // if not found - add behavior with setting turned on 
                if (debug == null)
                {
                    serviceHostOverTCP.Description.Behaviors.Add(
                         new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                }
                else
                {
                    // make sure setting is turned ON
                    if (!debug.IncludeExceptionDetailInFaults)
                    {
                        debug.IncludeExceptionDetailInFaults = true;
                    }
                }

                serviceHostOverTCP.Open();
                servicehost = serviceHostOverTCP;

                LoggerManager.Debug("Service started. Available in following endpoints");

                foreach (var serviceEndpoint in serviceHostOverTCP.Description.Endpoints)
                {
                    LoggerManager.Debug($"Service End point :{serviceEndpoint.ListenUri.AbsoluteUri}");
                }

                if (!factories.ContainsKey(type))
                {
                    factories.Add(type, servicehost);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// Event handler for ClientBase.Faulted event.
        /// </summary>
        /// <typeparam name="T">Interface type of service</typeparam>
        /// <param name="sender">ClientBase instance</param>
        /// <param name="e">Event Args</param>
        private void Channel_Faulted<T>(object sender, EventArgs e)
        {
            try
            {
                LoggerManager.Debug($"Service Channel Faulted. Service = {typeof(T).FullName}, . Try Reopen");
                ((ICommunicationObject)sender).Abort();
                var factory = (ChannelFactory<T>)factories[typeof(T)];
                factory.CreateChannel();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"Service Channel Faulted. Service = {typeof(T).FullName}, Err = {err.Message}");
            }
        }

        public bool InitServiceHosts(int port = -1)
        {
            if (port == -1)
                port = GetServicePort();
            bool retVal = false;

            try
            {
                LoaderRemoteMediator = this.GetContainer().Resolve<ILoaderRemoteMediator>();

                DeInitService();

                Task task = new Task(() =>
                {                

                    #region //.. IO

                    CreateServiceHost<IIOMappingsParameter>(this.IOState.IO, port, PortOffsets.IOServicePortOffset, ServiceAddress.IOPortsService, IOServiceHost);

                    #endregion

                    #region //.. Motion

                    CreateServiceHost<IMotionManager>(MotionManager, port, PortOffsets.MotionServicePortOffset, ServiceAddress.MotionManagerService, MotionServiceHost,
                        new System.Xml.XmlDictionaryReaderQuotas()
                        {
                            MaxDepth = 64,
                            MaxStringContentLength = 2147483647,
                            MaxArrayLength = 2147483647,
                            MaxBytesPerRead = 4096,
                            MaxNameTableCharCount = 16384
                        });

                    #endregion

                    #region //.. StageSuperVisor

                    CreateServiceHost<IStageSupervisor>(StageSupervisor, port, PortOffsets.StageSupervisorServicePortOffset, ServiceAddress.StageSupervisorService, StageServiceHost,
                        new System.Xml.XmlDictionaryReaderQuotas()
                        {
                            MaxDepth = 64,
                            MaxStringContentLength = 2147483647,
                            MaxArrayLength = 2147483647,
                            MaxBytesPerRead = 4096,
                            MaxNameTableCharCount = 16384
                        });

                    #endregion

                    #region //..StageMove
                    CreateServiceHost<IStageMove>(StageSupervisor.StageModuleState, port, PortOffsets.StageMovePortOffset, ServiceAddress.StageMoveService, StageMoveHost,
                        new System.Xml.XmlDictionaryReaderQuotas()
                        {
                            MaxDepth = 64,
                            MaxStringContentLength = 2147483647,
                            MaxArrayLength = 2147483647,
                            MaxBytesPerRead = 256,
                            MaxNameTableCharCount = 16384
                        });

                    #endregion

                    #region //.. LoaderRemoteMediator

                    CreateServiceHost<ILoaderRemoteMediator>(LoaderRemoteMediator, port, PortOffsets.LoaderRemoteMediatorServicePortOffset, ServiceAddress.LoaderRemoteMediatorService, LoaderRemoteMediatorHost,
                        new System.Xml.XmlDictionaryReaderQuotas()
                        {
                            MaxDepth = 64,
                            MaxStringContentLength = 2147483647,
                            MaxArrayLength = 2147483647,
                            MaxBytesPerRead = 4096,
                            MaxNameTableCharCount = 16384
                        });

                    #endregion

                    #region //.. GPCC ViewModel

                    if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                    {
                        CreateServiceHost<ICCObservationVM>(this.ViewModelManager().FindViewModelObject(new Guid("b094fbf9-35a0-43ab-9311-def5a717a9f7")), port, PortOffsets.GPCCServicePortOffset, ServiceAddress.VmGPCardChangeMainPage, GPCCViewModelHost);
                    }
                    else
                    {
                        CreateServiceHost<ICCObservationVM>(this.ViewModelManager().FindViewModelObject(new Guid("b7104207-1f96-4669-b027-03061794d5a5")), port, PortOffsets.GPCCServicePortOffset, ServiceAddress.VmGPCardChangeMainPage, GPCCViewModelHost);

                    }

                    #endregion

                    #region //.. Temp. Controller service

                    CreateServiceHost<ITempController>(this.TempController(), port, PortOffsets.TempContServicePortOffset, ServiceAddress.TempControlService, TempConHost,
                        new System.Xml.XmlDictionaryReaderQuotas()
                        {
                            MaxDepth = 64,
                            MaxStringContentLength = 2147483647,
                            MaxArrayLength = 2147483647,
                            MaxBytesPerRead = 4096,
                            MaxNameTableCharCount = 16384
                        });

                    #endregion

                    #region //..

                    CreateServiceHost<IRetestModule>(this.RetestModule(), port, PortOffsets.RetestServicePortOffset, "RetestModuleService", RetestModuleHost);



                    #endregion
                    #region //.. Polish Wafer Module service

                    CreateServiceHost<IPolishWaferModule>(this.PolishWaferModule(), port, PortOffsets.PWServicePortOffset, ServiceAddress.PolishWaferModuleService, PolishWaferModuleHost);

                    #endregion

                    #region //.. FileManager service

                    CreateServiceHost<IFileManager>(this.FileManager(), port, PortOffsets.FileManagerServicePortOffset, ServiceAddress.FileManagerService, FileManagerHost);

                    #endregion

                    #region //.. LotOPModule service

                    CreateServiceHost<ILotOPModule>(this.LotOPModule(), port, PortOffsets.LotOPModuleServicePortOffset, ServiceAddress.LotOPModuleService, LotOPModuleHost);

                    #endregion

                    #region //.. SoakingModule service

                    CreateServiceHost<ISoakingModule>(this.SoakingModule(), port, PortOffsets.SoakingModuleServicePortOffset, ServiceAddress.SoakingModuleService, SoakingModuleHost);

                    #endregion

                    #region //.. PMI Module service

                    CreateServiceHost<IPMIModule>(this.PMIModule(), port, PortOffsets.PMIServicePortOffset, "PMIModuleSerice", PMIModuleHost);

                    #endregion

                    #region //.. CoordinateManager. Controller service

                    CreateServiceHost<ICoordinateManager>(this.CoordinateManager(), port, PortOffsets.CoordManagerServicePortOffset, "CoordinateManagerService", CoordinateManagerHost,
                        new System.Xml.XmlDictionaryReaderQuotas()
                        {
                            MaxDepth = 64,
                            MaxStringContentLength = 2147483647,
                            MaxArrayLength = 2147483647,
                            MaxBytesPerRead = 4096,
                            MaxNameTableCharCount = 16384
                        });

                    #endregion

                    #region //.. Param. Manager service

                    CreateServiceHost<IParamManager>(this.ParamManager(),
                        port,
                        PortOffsets.ParamManagerServicePortOffset,
                        "ParamManagerService",
                        paramManagerConHost,
                        new System.Xml.XmlDictionaryReaderQuotas()
                        {
                            //MaxDepth = 128,
                            MaxStringContentLength = 2147483647,
                            MaxArrayLength = 2147483647,
                            //MaxBytesPerRead = 4096,
                            MaxNameTableCharCount = 16384
                        }, inactivitytimeout: TimeSpan.FromMinutes(2));

                    #endregion

                    #region //.. WaferAligner service

                    //CreateServiceHost<IWaferAligner>(this.WaferAligner(), port, PortOffsets.WALServicePortOffset, "WAService", WAModuleHost,
                    //    null, null, null,
                    //    new TimeSpan(0, 1, 0),
                    //    new TimeSpan(0, 1, 0),
                    //    new TimeSpan(0, 1, 0),
                    //    new TimeSpan(0, 1, 0));
                    CreateServiceHost<IWaferAligner>(this.WaferAligner(), port, PortOffsets.WALServicePortOffset, "WAService", WAModuleHost);

                    #endregion

                    #region //.. PinAligner service

                    //CreateServiceHost<IPinAligner>(this.PinAligner(), port, PortOffsets.PinAlignerServicePortOffset, "PinAlignerService", PAModuleHost,
                    //    null, null, null,
                    //    new TimeSpan(0, 1, 0),
                    //    new TimeSpan(0, 1, 0),
                    //    new TimeSpan(0, 1, 0),
                    //    new TimeSpan(0, 1, 0));
                    CreateServiceHost<IPinAligner>(this.PinAligner(), port, PortOffsets.PinAlignerServicePortOffset, "PinAlignerService", PAModuleHost);

                        #endregion

                });
                task.Start();
                task.Wait();

                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitServiceHost() Error occurred. Err = {err.Message}");
            }

            return retVal;
        }

        public bool DeInitService()
        {
            bool retVal = false;
            try
            {
                if (StageServiceHost == null)
                {
                    retVal = true;
                    return retVal;
                }
                this.StageSupervisor().DeInitService();
                this.LoaderRemoteMediator().DeInitService();
                this.MotionManager().DeInitService();
                this.IOManager().DeInitService();

                DisConnectdispService();
                DisConnectDelegateEventService();
                DisconnectDataGatewayService();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetServicePort()
        {
            return CommParam.Port;
        }
        string LoaderURI = null;
        public bool IsEnableDialogProxy()
        {
            bool retVal = false;
            try
            {
                if (DelegateEventProxy != null && this.LoaderController().GetconnectFlag())
                {
                    if (DelegateEventProxy.State == CommunicationState.Opened || DelegateEventProxy.State == CommunicationState.Created)
                    {
                        retVal = true;
                    }
                    else
                    {
                        LoggerManager.Debug($"Invalid DialogProxy Channel. Try Abort and Reconnect DialogProxy.");
                        DisConnectDelegateEventService(); //비정상 channel abort
                        BindDelegateEventService(LoaderURI.ToString());
                        if ((DelegateEventProxy.State == CommunicationState.Opened) || (DelegateEventProxy.State == CommunicationState.Created))
                        {
                            retVal = true;
                        }
                        else
                        {
                            retVal = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsEnableDataGatewayProxy()
        {
            bool retVal = false;
            try
            {
                if (DataGatewayProxy != null && this.LoaderController().GetconnectFlag())
                {
                    if (DataGatewayProxy.State == CommunicationState.Opened || DataGatewayProxy.State == CommunicationState.Created)
                    {
                        retVal = true;
                    }
                    else
                    {
                        LoggerManager.Debug($"Invalid DataGatewayProxy Channel. Try Abort and Reconnect DataGatewayProxy.");
                        DisconnectDataGatewayService(); //비정상 channel abort
                        BindDataGatewayService(LoaderURI.ToString());
                        if ((DataGatewayProxy.State == CommunicationState.Opened) || (DataGatewayProxy.State == CommunicationState.Created))
                        {
                            retVal = true;
                        }
                        else
                        {
                            retVal = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        string DispUri = null;
        DispHostProxy.DispProxy dispProxy = null;
        public DispHostProxy.DispProxy GetDispProxy()
        {
            DispHostProxy.DispProxy proxy = null;
            try
            {
                if (dispProxy != null && this.LoaderController().GetconnectFlag())
                {
                    if (dispProxy.State == CommunicationState.Opened || dispProxy.State == CommunicationState.Created)
                    {
                        proxy = dispProxy;
                    }
                    else
                    {
                        LoggerManager.Debug($"Invalid Display Channel. Try Abort and Reconnect DisplayProxy.");
                        DisConnectdispService();
                        BindDispService(DispUri);
                        proxy = dispProxy;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return proxy;
        }
        public bool AcceptUpdateDisp { get; set; } = false;

        public void SetAcceptUpdateDisp(bool flag)
        {
            AcceptUpdateDisp = flag;
        }
        public bool GetAcceptUpdateDisp()
        {
            return AcceptUpdateDisp;
        }
        public void BindDispService(string uri)
        {
            try
            {
                Uri absUri = new Uri(uri);
                DispUri = uri;

                //GetDispProxy에서 호출하는 경우는 이미 기존 채널을 abort하고 dispProxy null인채로 호출되게 된다.
                if (dispProxy != null)
                {
                    //Connect시에만 호출되므로, 기존 연결된 채널이 있다면 종료 처리함 (Loader 강제 종료 및 재실행후 연결한 경우임)
                    DisConnectdispService(true); //등록된 callback등을 제거해야하므로 abort 하더라도 호출해 주어야함
                }
                dispProxy = new DispHostProxy.DispProxy(absUri, ImageDispHostCallback.GetInstanceContext());
                dispProxy.InitService(this.LoaderController().GetChuckIndex());

                (dispProxy as ICommunicationObject).Closed += DispProxy_Closed;
                (dispProxy as ICommunicationObject).Faulted += DispProxy_Faulted;

                foreach (var cam in this.VisionManager().CameraDescriptor.Cams)
                {
                    cam.DisplayService.ImageUpdated += DisplayService_ImageUpdated;
                }

                if ((this.StageSupervisor().StageMode == GPCellModeEnum.ONLINE && this.StageSupervisor().StreamingMode == StreamingModeEnum.STREAMING_ON)
                    || this.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE)
                {
                    AcceptUpdateDisp = true;
                }
                else
                {
                    AcceptUpdateDisp = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"BindDispService(): Error occurred. Err = {err.Message}");
            }
        }

        public void BindDelegateEventService(string uri)
        {
            try
            {
                Uri absUri = new Uri(uri);

                //IsEnableDialogProxy에서 호출되는 경우는 이미 기존 채널을 abort하고 DelegateEventProxy값이 null인채로 호출되게 된다.
                if (DelegateEventProxy != null)
                {
                    //Connect시에만 호출되므로, 기존 연결된 채널이 있다면 종료 처리함 (Loader 강제 종료 및 재실행후 연결한 경우임)
                    DisConnectDelegateEventService(true); //등록된 callback등을 제거해야하므로 abort 하더라도 호출해 주어야함
                }

                DelegateEventProxy = new DelegateEventProxy(absUri, DelegateEventHostCallback.GetInstanceContext());
                LoaderURI = uri;
                DelegateEventProxy.InitService(this.LoaderController().GetChuckIndex());
                (DelegateEventProxy as ICommunicationObject).Faulted += DelegateEventProxy_Faulted;
                (DelegateEventProxy as ICommunicationObject).Closed += DelegateEventProxy_Closed;

                this.MetroDialogManager().MessageDialogShow += DelegateEventProxy.ShowMessageDialog;
                this.MetroDialogManager().SingleInputDialogShow += DelegateEventProxy.ShowSingleInputDialog;
                this.MetroDialogManager().SingleInputGetInputData += DelegateEventProxy.GetInputDataSingleInput;
                this.MetroDialogManager().MetroDialoShow += DelegateEventProxy.ShowMetroDialog;
                this.MetroDialogManager().MetroDialogClose += DelegateEventProxy.CloseMetroDialog;


            }
            catch (Exception err)
            {
                LoggerManager.Error($"BindDialogService(): Error occurred. Err = {err.Message}");
            }
        }

        private void DispProxy_Closed(object sender, EventArgs e)
        {
            LoggerManager.Debug($"DispProxy Channel closed. Sender = {sender}");
        }

        private void DispProxy_Faulted(object sender, EventArgs e)
        {
            LoggerManager.Debug($"DispProxy Channel faulted. Sender = {sender}");
        }

        private void DelegateEventProxy_Faulted(object sender, EventArgs e)
        {
            LoggerManager.Debug($"DelegateEvent Channel faulted. Sender = {sender}");
        }

        private void DelegateEventProxy_Closed(object sender, EventArgs e)
        {
            LoggerManager.Debug($"DelegateEvent Channel Closed. Sender = {sender}");
        }

        private void DataGatewayProxy_Faulted(object sender, EventArgs e)
        {
            LoggerManager.Debug($"DataGateway Channel faulted. Sender = {sender}");
        }

        private void DataGatewayProxy_Closed(object sender, EventArgs e)
        {
            LoggerManager.Debug($"DataGateway Channel Closed. Sender = {sender}");
        }

        public void DisConnectdispService(bool bForceAbort = false)
        {
            try
            {
                AcceptUpdateDisp = false;

                foreach (var cam in this.VisionManager().CameraDescriptor.Cams)
                {
                    if (!cam.DisplayService.IsImageUpdatedNull())
                    {
                        cam.DisplayService.ImageUpdated -= DisplayService_ImageUpdated;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (dispProxy != null)
                {
                    dispProxy.DeInitService(bForceAbort);
                }
                dispProxy = null;
            }
        }
        public void DisConnectDelegateEventService(bool bForceAbort = false)
        {
            try
            {
                if (DelegateEventProxy != null)
                {
                    this.MetroDialogManager().MessageDialogShow -= DelegateEventProxy.ShowMessageDialog;

                    this.MetroDialogManager().SingleInputDialogShow -= DelegateEventProxy.ShowSingleInputDialog;
                    this.MetroDialogManager().SingleInputGetInputData -= DelegateEventProxy.GetInputDataSingleInput;
                    this.MetroDialogManager().MetroDialoShow -= DelegateEventProxy.ShowMetroDialog;
                    this.MetroDialogManager().MetroDialogClose -= DelegateEventProxy.CloseMetroDialog;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (DelegateEventProxy != null)
                {
                    DelegateEventProxy.DeInitService(bForceAbort);
                }
                DelegateEventProxy = null;
            }
        }

        public void DisconnectDataGatewayService(bool bForceAbort = false)
        {
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (DataGatewayProxy != null)
                {
                    DataGatewayProxy.DeInitService(bForceAbort);
                }
                DataGatewayProxy = null;
            }
        }

        private void DisplayService_ImageUpdated(ImageBuffer image)
        {
            lock (image)
            {
                using (var tranScope = new TransactionScope())
                {
                    try
                    {
                        if (!AcceptUpdateDisp)
                            return;
                            
                        if ((this.StageSupervisor().StageMode == GPCellModeEnum.ONLINE && this.StageSupervisor().StreamingMode == StreamingModeEnum.STREAMING_ON)
                            || this.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE)
                        {
                            GetDispProxy()?.UpdateImage(image);
                        }
                    }
                    catch (ProtocolException protocolEx)
                    {
                        dispProxy.Abort(); //abort후 다음번 image update시점에 faulted 상태로 체크되어 disconnect후 reconnect를 시도하게 됨
                        LoggerManager.Error($"DisplayService_ImageUpdated(): ProtocolException occurred. Err = {protocolEx.Message}");
                    }
                    catch (CommunicationException err)
                    {
                        dispProxy.Abort(); //abort후 다음번 image update시점에 faulted 상태로 체크되어 disconnect후 reconnect를 시도하게 됨
                        LoggerManager.Error($"DisplayService_ImageUpdated(): CommunicationException occurred. Err = {err.Message}");
                    }
                    catch (TimeoutException err)
                    {
                        LoggerManager.Error($"DisplayService_ImageUpdated(): TimeoutException occurred. Err = {err.Message}");
                    }

                    catch (Exception err)
                    {
                        LoggerManager.Error($"DisplayService_ImageUpdated(): Error occurred. Err = {err.Message}");
                        throw;
                    }
                }
            }
        }

        public IDialogServiceProxy GetMessageEventHost()
        {
            return (IDialogServiceProxy)DelegateEventProxy;
        }

        public EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsEnableDataGatewayProxy())
                {
                    retVal = DataGatewayProxy.NotifyStageAlarm(noticeCodeInfo) ;
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
             return retVal;
        }

        public void BindDataGatewayService(string uri)
        {
            try
            {
                Uri absUri = new Uri(uri);

                //IsEnableDialogProxy에서 호출되는 경우는 이미 기존 채널을 abort하고 DelegateEventProxy값이 null인채로 호출되게 된다.
                if (DataGatewayProxy != null)
                {
                    //Connect시에만 호출되므로, 기존 연결된 채널이 있다면 종료 처리함 (Loader 강제 종료 및 재실행후 연결한 경우임)
                    DisconnectDataGatewayService(true); //등록된 callback등을 제거해야하므로 abort 하더라도 호출해 주어야함
                }

                DataGatewayProxy = new DataGatewayProxy(absUri, DataGatewayHostCallback.GetInstanceContext());
                LoaderURI = uri;
                DataGatewayProxy.InitService(this.LoaderController().GetChuckIndex());
                (DataGatewayProxy as ICommunicationObject).Faulted += DataGatewayProxy_Faulted;
                (DataGatewayProxy as ICommunicationObject).Closed += DataGatewayProxy_Closed;

                this.NotifyManager().NotifyStackParams();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"BindDataGatewayService(): Error occurred. Err = {err.Message}");
            }
        }

        #endregion

    }

    public class StageCommunicationParameter : ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public StageCommunicationParameter()
        {

        }

        public string FilePath { get; set; } = "";

        public string FileName { get; set; } = "StageServiceParameter.Json";

        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }


        private int _Port;
        public int Port
        {
            get { return _Port; }
            set
            {
                if (value != _Port)
                {
                    _Port = value;
                    RaisePropertyChanged();
                }
            }
        }


        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            Port = 9000;
            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
            return;
        }

        public EventCodeEnum SetEmulParam()
        {
            SetDefaultParam();
            return EventCodeEnum.NONE;
        }
    }
}

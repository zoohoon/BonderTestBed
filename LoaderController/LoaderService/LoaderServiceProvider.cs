using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using ProberInterfaces.LoaderController;
using ProberInterfaces.Utility;
using ProberErrorCode;
using LoaderParameters;
using LoaderControllerBase;
using ProberInterfaces;
using LoaderServiceBase;
using LogModule;
using System.ServiceModel.Description;
using System.ComponentModel;

namespace LoaderController
{
    public class LoaderServiceProvider : IFactoryModule
    {
        public static ILoaderServiceProxy ServiceProxy { get; set; }
        public static IGPLoaderServiceProxy GPServiceProxy { get; set; }
        public static ILoaderService Create(ILoaderController module)
        {
            ILoaderService Service = null;

            try
            {
                var loaderController = module as LoaderController;
                var param = loaderController.LoaderConnectParam as LoaderControllerParam;
                if (param.LoaderServiceType == LoaderServiceTypeEnum.DynamicLinking)
                {
                    string loaderFactoryDllName = "LoaderFactory.dll";
                    var loaderFactoryAssembly = Assembly.LoadFrom(loaderFactoryDllName);

                    var loaderResolver = ReflectionEx.GetAssignableInstances<ILoaderResolver>(loaderFactoryAssembly).FirstOrDefault();
                    var loaderContainer = loaderResolver.ConfigureDependencies();

                    string loaderCoreDllName = "LoaderCore.dll";
                    var loaderCoreAssembly = Assembly.LoadFrom(loaderCoreDllName);

                    var directLoaderService = ReflectionEx.GetAssignableInstances<IDirectLoaderService>(loaderCoreAssembly).FirstOrDefault();
                    directLoaderService.SetLoaderContainer(loaderContainer);
                    directLoaderService.Set(module.GetContainer(), module as LoaderController);

                    Service = directLoaderService;
                }
                else if (param.LoaderServiceType == LoaderServiceTypeEnum.WCF)
                {
                    //Hardcoding-------------
                    //var uri = new Uri(@"net.pipe://localhost/services/loader");
                    //var binding = new NetNamedPipeBinding();
                    //var uri = new Uri(@"net.tcp://localhost:7070/services/loader");
                    //var binding = new NetTcpBinding();
                    //var factory = new DuplexChannelFactory<ILoaderService>(callback, binding, new EndpointAddress(uri));
                    //-----------------------
                    //var factory = new DuplexChannelFactory<ILoaderService>(module, param.EndpointConfigurationName);
                    //Service = factory.CreateChannel();

                    //var service = new LoaderServiceProxy(param.EndpointConfigurationName);
                    //var ret = service.Connect();
                    //var context = new InstanceContext(module);
                    //ServiceProxy = new LoaderServiceProxy(param.EndpointConfigurationName, context);


                    var context = new InstanceContext(module);
                    ServiceProxy = new LoaderServiceProxy(param.EndpointConfigurationName, context);
                    //((DuplexClientBase<ILoaderService>)ServiceProxy).ChannelFactory.CreateChannel();

                    Service = ServiceProxy.GetService();
                    ServiceProxy.Connect();
                    Service.IsServiceAvailable();
                }
                else if (param.LoaderServiceType == LoaderServiceTypeEnum.REMOTE)
                {
                    string loaderFactoryDllName = "LoaderFactory.dll";
                    var loaderFactoryAssembly = Assembly.LoadFrom(loaderFactoryDllName);

                    var loaderResolver = ReflectionEx.GetAssignableInstances<ILoaderResolver>(loaderFactoryAssembly).FirstOrDefault();
                    var loaderContainer = loaderResolver.ConfigureDependencies();

                    string loaderCoreDllName = "LoaderCore.dll";
                    var loaderCoreAssembly = Assembly.LoadFrom(loaderCoreDllName);
                    var services = ReflectionEx.GetAssignableInstances<ILoaderService>(loaderCoreAssembly);
                    var loaderService = services.FirstOrDefault(s => s.GetServiceType() == LoaderServiceTypeEnum.REMOTE);
                    Service = loaderService;
                    Service.SetContainer(module.GetContainer());
                    //Service = loaderService;
                    //var factory = new DuplexChannelFactory<ILoaderService>(module, param.EndpointConfigurationName);
                    //Service = factory.CreateChannel();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Service;
        }

        public static void CreateGPLoader(ILoaderServiceCallback module)
        {
            try
            {
                var param = module.LoaderConnectParam as LoaderControllerParam;

                if (param.LoaderServiceType == LoaderServiceTypeEnum.WCF)
                {
                    //brett// 해당 함수는 Loader에서 cell connect 시도시 호출되게 된다. 일반적으로는 GPServiceProxy가 null일 것이다.
                    // loader와 cell이 연결중인 상태에서 loader가 죽고 아직 channell faulted나 closed가 호출되지 않은 경우에는 null이 아닐수 있다.
                    if (GPServiceProxy != null)
                    {
                        GPServiceProxy.Abort();
                        GPServiceProxy = null;
                    }
                    var context = new InstanceContext(module);
                    GPServiceProxy = new GPLoaderServiceProxy(param.EndpointConfigurationName, context);
                    GPServiceProxy.Connect(module.GetChuckID().ToString());

                    GPServiceProxy.Faulted += new EventHandler(Channel_Faulted);
                    GPServiceProxy.Closed += new EventHandler(Channel_Closed);

                    void Channel_Faulted(object sender, EventArgs e)
                    {
                        //Loader 가 꺼졌을때           
                        LoggerManager.Debug($"GPLoaderClient Channel faulted. Sender = {sender}");
                    }
                    void Channel_Closed(object sender, EventArgs e)
                    {
                        //Loader 가 꺼졌을때
                        LoggerManager.Debug($"GPLoaderClient Channel closed. Sender = {sender}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public static IGPLoaderService GetGPLoaderService(ILoaderServiceCallback module)
        {
            IGPLoaderService Service = null;

            try
            {
                var param = module.LoaderConnectParam as LoaderControllerParam;

                if (param != null && param.LoaderServiceType == LoaderServiceTypeEnum.WCF)
                {
                    if (GPServiceProxy != null)
                    {
                        if (IsOpened())
                        {                            
                            Service = GPServiceProxy?.GetService();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return Service;
        }


        private static void ServiceProxy_Opened(object sender, EventArgs e)
        {
            LoggerManager.Debug($"Service proxy opened.");
        }

        public static void Deinit(ILoaderService Service)
        {
            try
            {
                if (Service is IDirectLoaderService)
                {
                    (Service as IDirectLoaderService).Deinitialize();
                }

                if (Service is ILoaderService)
                {
                    (Service as ILoaderService).Disconnect();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void GPDeinit(IGPLoaderService Service)
        {
            try
            {

                if (Service is IGPLoaderService)
                {
                    (Service as IGPLoaderService).Disconnect();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static bool DisConnect()
        {
            bool retVal = false;
            try
            {
                if (IsOpened())
                {
                    try
                    {
                        GPServiceProxy?.Close();
                        retVal = true;
                    }
                    catch (CommunicationException err)
                    {
                        GPServiceProxy?.Abort();
                        retVal = true;
                        LoggerManager.Exception(err);
                    }
                    catch (Exception err)
                    {
                        GPServiceProxy?.Abort();
                        retVal = true;
                        LoggerManager.Exception(err);
                    }
                }
                else
                {
                    if (GPServiceProxy?.State == CommunicationState.Faulted)
                        GPServiceProxy?.Abort();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                GPServiceProxy = null;
            }

            return retVal;
        }

        private static bool IsOpened()
        {
            if (GPServiceProxy == null)
                return false;
            if (GPServiceProxy?.State == CommunicationState.Opened || GPServiceProxy?.State == CommunicationState.Created)
                return true;
            else
                return false;
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
        public class LoaderServiceProxy : DuplexClientBase<ILoaderService>, ILoaderServiceProxy
        {
            public LoaderServiceProxy(string endpoint, InstanceContext callback) :
            base(callback, new ServiceEndpoint(
                ContractDescription.GetContract(typeof(ILoaderService)),
                new NetTcpBinding()
                {
                    Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                    ReceiveTimeout = TimeSpan.MaxValue,
                    SendTimeout = new TimeSpan(0, 10, 0),
                    OpenTimeout = new TimeSpan(0, 10, 0),
                    CloseTimeout = new TimeSpan(0, 10, 0),
                    MaxBufferPoolSize = 524288,
                    MaxReceivedMessageSize = 2147483647,
                },
                new EndpointAddress(endpoint)))
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
            }
            public LoaderServiceProxy(string endpoint) :
            base(new ServiceEndpoint(
                ContractDescription.GetContract(typeof(ILoaderService)),
                new NetTcpBinding()
                {
                    Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                    ReceiveTimeout = TimeSpan.MaxValue,
                    SendTimeout = new TimeSpan(0, 10, 0),
                    OpenTimeout = new TimeSpan(0, 10, 0),
                    CloseTimeout = new TimeSpan(0, 10, 0),
                    MaxBufferPoolSize = 524288,
                    MaxReceivedMessageSize = 2147483647,
                },
                new EndpointAddress(endpoint)))
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
            }

            public ILoaderService GetService()
            {
                return Channel;
            }

            public EventCodeEnum Connect()
            {
                return Channel.Connect();
            }
        }

        [ServiceBehavior(
            InstanceContextMode = InstanceContextMode.Single, 
            ConcurrencyMode = ConcurrencyMode.Multiple,
            UseSynchronizationContext = false)]
        public class GPLoaderServiceProxy : DuplexClientBase<IGPLoaderService>, IGPLoaderServiceProxy
        {            
            public GPLoaderServiceProxy(string endpoint, InstanceContext callback) :
            base(callback, new ServiceEndpoint(
                ContractDescription.GetContract(typeof(IGPLoaderService)),
                new NetTcpBinding()
                {
                    Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                    ReceiveTimeout = TimeSpan.MaxValue,
                    SendTimeout = new TimeSpan(0, 5, 0),
                    OpenTimeout = new TimeSpan(0, 1, 0),
                    CloseTimeout = new TimeSpan(0, 1, 0),
                    MaxBufferSize = 2147483647,
                    MaxBufferPoolSize = 524288,
                    MaxReceivedMessageSize = 2147483647,
                    ReliableSession = new OptionalReliableSession() { Enabled=true, InactivityTimeout = TimeSpan.FromMinutes(1) }
                },
                new EndpointAddress(endpoint)))
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
            }

            public IGPLoaderService GetService()
            {
                return Channel;
            }


            public EventCodeEnum Connect(string chuckID)
            {
                var result = Channel.Connect(chuckID);

                return result;
            }
        }
    }
}

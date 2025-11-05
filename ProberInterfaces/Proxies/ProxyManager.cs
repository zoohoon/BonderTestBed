using LogModule;
using ProberInterfaces.Proxies.Behaviors;
using ProberInterfaces.Proxies.Helpers;
using ProberInterfaces.Utility;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ProberInterfaces.Proxies
{
    public class MyBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new MyMessageInspector());
        }

        public void ApplyDispatchBehavior(
            ServiceEndpoint endpoint,
            EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(
            ServiceEndpoint endpoint)
        {
        }
    }

    public class MyMessageInspector : IClientMessageInspector
    {
        public void AfterReceiveReply(
            ref Message reply,
            object correlationState)
        {
            Console.WriteLine(
            "Received the following reply: '{0}'", reply.ToString());
        }

        public object BeforeSendRequest(
            ref Message request,
            IClientChannel channel)
        {
            Console.WriteLine(
            "Sending the following request: '{0}'", request.ToString());
            return null;
        }
    }

    public class ProxySet
    {
        public object Proxy { get; set; }
        public string HostName { get; set; }
        public ProxySet(object proxy, string hostname)
        {
            this.Proxy = proxy;
            this.HostName = hostname;
        }
    }


    public class ProxyManager : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        //Our Ubber Brand New Cool cache Objects
        //private static readonly Hashtable _channelFactoryCache = new Hashtable();
        //private static readonly Hashtable _proxyCache = new Hashtable();

        //private Hashtable _proxyCache = new Hashtable();
        //public Hashtable ProxyCache
        //{
        //    get { return _proxyCache; }
        //    set
        //    {
        //        if (value != _proxyCache)
        //        {
        //            _proxyCache = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private MethodInfo InitProxyMethod { get; set; }

        //minskim// Proxy Fault 발생시 Cell Disconnect Logice을 수행하기 위해 추가 함
        private MethodInfo DisconnectStageMethod { get; set; }

        private int stageIndex { get; set; }

        private object loadercommanagerobj { get; set; }

        private ObservableDictionary<Type, ProxySet> _proxyCache = new ObservableDictionary<Type, ProxySet>();
        public ObservableDictionary<Type, ProxySet> ProxyCache
        {
            get { return _proxyCache; }
            set
            {
                if (value != _proxyCache)
                {
                    _proxyCache = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private readonly object _proxyLogsCollectionLock;
        private ObservableCollection<string> _proxyLogs = new ObservableCollection<string>();
        public ObservableCollection<string> ProxyLogs
        {
            get { return _proxyLogs; }
            set
            {
                if (value != _proxyLogs)
                {
                    _proxyLogs = value;
                    RaisePropertyChanged();

                    //BindingOperations.EnableCollectionSynchronization(_proxyLogs, _proxyLogsCollectionLock);
                }
            }
        }

        private bool _isDebug = false;
        public bool IsDebug
        {
            get { return _isDebug; }
            set
            {
                if (value != _isDebug)
                {
                    _isDebug = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private static readonly Hashtable _dnsCache = new Hashtable();

        //private ObservableDictionary<Type, IProberProxy> _Proxies = new ObservableDictionary<Type, IProberProxy>();
        //public ObservableDictionary<Type, IProberProxy> Proxies
        //{
        //    get { return _Proxies; }
        //    set
        //    {
        //        if (value != _Proxies)
        //        {
        //            _Proxies = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //public ProxyManager()
        //{
        //    this.Proxies = new Proxies(this);
        //}

        //private Proxies _proxies;
        //public Proxies Proxies
        //{
        //    get { return _proxies; }
        //    set
        //    {
        //        if (value != _proxies)
        //        {
        //            _proxies = value;
        //        }
        //    }
        //}


        public bool IsRegist<T>()
        {
            bool retval = false;

            try
            {
                var t = typeof(T);

                if (_proxyCache.ContainsKey(t) == true)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ProxyManager(MethodInfo initproxy, MethodInfo disconnectstage, int stageindex, object obj)
        {
            this.InitProxyMethod = initproxy;
            this.DisconnectStageMethod = disconnectstage; //proxy fault 발생시 cell 정상 종료(UI갱신등)을 처리하기 위한 callback 등록
            this.stageIndex = stageindex;

            this.loadercommanagerobj = obj;
        }

        // Return an Encapsulated IClientChannel
        //public T GetProxy<T>()
        //{
        //    var t = typeof(T);

        //    if (!_proxyCache.ContainsKey(t))
        //    {
        //        try
        //        {
        //            _proxyCache.Add(t, createProxy<T>());
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception("Failed to create provider: " + ex.Message, ex);
        //        }
        //    }

        //    var s = (ProxyBase<T>)_proxyCache[t];
        //    var ic = (IClientChannel)s.InnerChannel;

        //    //here is the key Abort anything and force dispose
        //    ic.Abort();

        //    // Recreate the channel there is a small amount of overhead here about 4-5 milliseconds 
        //    // Well worth the ease of deployment / durability
        //    s.SetChannel();

        //    return s.InnerChannel;
        //}

        public void RegistEvent(ICommunicationObject communicationObject)
        {
            try
            {
                if (communicationObject != null)
                {
                    communicationObject.Faulted += Proxy_Faulted;
                    communicationObject.Closed += Proxy_Closed;
                    communicationObject.Opening += Proxy_Opening;
                    communicationObject.Closing += Proxy_Closing;
                    communicationObject.Opened += Proxy_Opened;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ReleaseEvent(ICommunicationObject communicationObject)
        {
            try
            {
                if (communicationObject != null)
                {
                    communicationObject.Faulted -= Proxy_Faulted;
                    communicationObject.Closed -= Proxy_Closed;
                    communicationObject.Opening -= Proxy_Opening;
                    communicationObject.Closing -= Proxy_Closing;
                    communicationObject.Opened -= Proxy_Opened;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void RegistProxy<T>(ProxySet proxyset)
        {
            var t = typeof(T);

            try
            {
                if (IsRegist<T>() == true)
                {
                    _proxyCache.TryGetValue(typeof(T), out var preProxy);

                    if (preProxy != null)
                    {
                        ReleaseEvent(preProxy.Proxy as ICommunicationObject);
                    }

                    _proxyCache.Remove(typeof(T));
                }

                _proxyCache.Add(t, proxyset);

                string logStr = $"RegistProxy : Type = {t}, Proxy = {proxyset.Proxy}";
                Addlog(logStr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        // Return an Encapsulated IClientChannel
        public T GetProxy<T>()
        {
            var t = typeof(T);

            T retval = default(T);

            //if (!_proxyCache.ContainsKey(t))
            //{
            //    try
            //    {
            //        _proxyCache.Add(t, createProxy<T>(ip, port, callback));
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception("Failed to create provider: " + ex.Message, ex);
            //    }
            //}

            if (_proxyCache.ContainsKey(t) == true)
            {
                try
                {
                    retval = (T)_proxyCache[t].Proxy;

                    //string logStr = $"GetProxy : Type = {t}, Proxy = {retval}";
                    //Addlog(logStr);
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
            }

            //var s = (ProxyBase<T>)_proxyCache[t];
            //var ic = (IClientChannel)s.InnerChannel;

            //here is the key Abort anything and force dispose
            //ic.Abort();

            //s.Abort();
            // Recreate the channel there is a small amount of overhead here about 4-5 milliseconds 
            // Well worth the ease of deployment / durability
            //s.SetChannel();

            //return s.InnerChannel;

            return retval;
        }

        /// <summary>
        ///   This is where we new up an encapsulated IClientChannel
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <returns> </returns>
        //internal static ProxyBase<T> createProxy<T>()
        //{
        //    var t = typeof(T);
        //    // Get The Channel Factory or create it if needed
        //    ChannelFactory channelFactory;

        //    lock (_channelFactoryCache)
        //    {
        //        if (_channelFactoryCache.ContainsKey(t))
        //        {
        //            channelFactory = (ChannelFactory)_channelFactoryCache[t];
        //        }
        //        else
        //        {
        //            channelFactory = createChannelFactory<T>();
        //            _channelFactoryCache.Add(t, channelFactory);
        //        }
        //    }
        //    EndpointAddress endpoint = null;

        //    //get Configuration
        //    var s = ConfigurationHelper.GetKey("HOST", Environment.MachineName);
        //    var port = ConfigurationHelper.GetKey("PORT", "8080");
        //    var binding = ConfigurationHelper.GetKey("BINDING", "HTTP");

        //    var serviceName = typeof(T).ToString();
        //    //Ser the correct service name Defaults to the interface name minus the I
        //    if (serviceName[0] == char.Parse("I"))
        //    {
        //        serviceName = serviceName.Remove(0, 1);
        //    }
        //    //Create the URI
        //    string server;
        //    switch (binding)
        //    {
        //        case "TCP":
        //            server = string.Format("net.tcp://" + getIPAddress(s) + ":{0}/{1}", port, serviceName);
        //            endpoint = new EndpointAddress(server);
        //            break;
        //        case "HTTP":
        //            server = string.Format("http://" + getIPAddress(s) + ":{0}/{1}", port, serviceName);
        //            endpoint = new EndpointAddress(server);
        //            break;
        //    }

        //    //Create the Enapsulated IClientChanenel
        //    var pb = new ProxyBase<T>((ChannelFactory<T>)channelFactory, endpoint);

        //    return pb;
        //}

        //public ProxyBase<T> createProxy<T>(string ip, int port, InstanceContext callback)
        //public T createProxy<T>(string ip, int port, InstanceContext callback)
        //{
        //    var t = typeof(T);
        //    // Get The Channel Factory or create it if needed
        //    //ChannelFactory channelFactory;

        //    //lock (_channelFactoryCache)
        //    //{
        //    //    if (_channelFactoryCache.ContainsKey(t))
        //    //    {
        //    //        channelFactory = (ChannelFactory)_channelFactoryCache[t];
        //    //    }
        //    //    else
        //    //    {
        //    //        channelFactory = createChannelFactory<T>();
        //    //        _channelFactoryCache.Add(t, channelFactory);
        //    //    }
        //    //}

        //    ServiceEndpoint endPoint = null;
        //    EndpointAddress endpointaddress = null;

        //    //get Configuration
        //    //var s = ConfigurationHelper.GetKey("HOST", Environment.MachineName);
        //    //var port = ConfigurationHelper.GetKey("PORT", "8080");
        //    //var binding = ConfigurationHelper.GetKey("BINDING", "HTTP");

        //    var serviceName = typeof(T).ToString();

        //    //Ser the correct service name Defaults to the interface name minus the I
        //    if (serviceName[0] == char.Parse("I"))
        //    {
        //        serviceName = serviceName.Remove(0, 1);
        //    }

        //    //Create the URI
        //    string server;

        //    var binding = new NetTcpBinding()
        //    {
        //        ReceiveTimeout = TimeSpan.MaxValue,
        //        MaxBufferPoolSize = 524288,
        //        MaxReceivedMessageSize = 50000000,
        //        Security = new NetTcpSecurity() { Mode = SecurityMode.None },
        //        ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
        //    };

        //    endpointaddress = new EndpointAddress($"net.tcp://{ip}:{port}/POS/PolishWaferModuleService");

        //    endPoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(T)), binding, endpointaddress);

        //    //switch (binding)
        //    //{
        //    //    case "TCP":
        //    //        server = string.Format("net.tcp://" + getIPAddress(s) + ":{0}/{1}", port, serviceName);
        //    //        endpoint = new EndpointAddress(server);
        //    //        break;
        //    //    case "HTTP":
        //    //        server = string.Format("http://" + getIPAddress(s) + ":{0}/{1}", port, serviceName);
        //    //        endpoint = new EndpointAddress(server);
        //    //        break;
        //    //}

        //    //Create the Enapsulated IClientChanenel
        //    //var pb = new ProxyBase<T>((ChannelFactory<T>)channelFactory, endPoint);

        //    pb = new PinAlignerProxy();
        //    return pb;
        //}

        // For speed I have us resolving to the Ip of the end 
        // Address.  This is probally totally not needed 
        //private static string getIPAddress(string server)
        //{
        //    lock (_dnsCache)
        //    {
        //        if (!_dnsCache.ContainsKey(server))
        //        {
        //            var host = Dns.GetHostEntry(server);
        //            _dnsCache.Add(server, host.AddressList[0].ToString());
        //        }

        //        return _dnsCache[server].ToString();
        //    }
        //}

        /// <summary>
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <returns> </returns>
        /// <exception cref="ArgumentException"></exception>
        private static ChannelFactory createChannelFactory<T>()
        {
            System.ServiceModel.Channels.Binding b = null;
            switch (ConfigurationHelper.GetKey("BINDING", "HTTP"))
            {
                case "HTTP":
                    b = new BasicHttpBinding();

                    break;
                case "TCP":
                    b = new NetTcpBinding();
                    break;
            }

            if (b != null)
            {
                var factory = new ChannelFactory<T>(b);
                // This is super important
                // Why ? This is where you can add
                // Custom behaviors to your outgoing calls
                // like Message Inspectors ... or behaviors.
                ApplyContextToChannelFactory(factory);

                return factory;
            }
            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="factory"> </param>
        public static void ApplyContextToChannelFactory(ChannelFactory factory)
        {
            //This area in where custom nehaviors are added 
            if (factory == null)
            {
                throw new ArgumentException("The argument 'factory' cannot be null.");
            }

            // here is an example of a behavior swap were some default
            // values where set on the replacement
            foreach (var desc in factory.Endpoint.Contract.Operations)
            {
                desc.Behaviors.Remove<DataContractSerializerOperationBehavior>();
                desc.Behaviors.Add(new ReferencePreservingBehavior(desc));
            }
        }

        private void Addlog(string log)
        {
            try
            {
                if (IsDebug == true)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ProxyLogs.Add($"{DateTime.Now} | {log}");
                    }));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Proxy_Faulted(object sender, EventArgs e)
        {
            string logStr = $"Proxy faulted. Sender = {sender}, cell index = {stageIndex}";

            ICommunicationObject communicationObject = null;

            try
            {
                if (sender != null)
                {
                    communicationObject = sender as ICommunicationObject;

                    if (communicationObject != null)
                    {
                        if (communicationObject.State != CommunicationState.Faulted)
                        {
                            communicationObject.Close();
                        }
                        else
                        {
                            communicationObject.Abort();

                            //minskim// 하위 logic은 retry를 수행하는 logic이지만 무한 retry 및 연결중 상태에서의 끊어짐 상태에서 문제(UI Lock)가 있어 주석 처리함
                            //if (DisconnectStageMethod != null)
                            //{
                            //    if (_proxyCache != null && _proxyCache.Count > 0)
                            //    {
                            //        var cache = ProxyCache.FirstOrDefault(x => x.Value.HostName == sender.GetType().Name);

                            //        if (cache.Equals(default(KeyValuePair<Type, ProxySet>)) == false)
                            //        {
                            //            MethodInfo generic = DisconnectStageMethod.MakeGenericMethod(cache.Key);

                            //            object[] param = new object[] { stageIndex };
                            //            generic.Invoke(loadercommanagerobj, param);

                            //            // TODO : 실패하면 어떻게, LoaderCommunicationManager의 DisConnectStage()를 호출?
                            //        }
                            //    }
                            //}

                            // 하위 logic은 falut 발생시 retry를 위한 로직임, 추후 retry 회수를 추가하여 재개발 필요함
                            // retry 후에도 계속 falut 이벤트가 발생한다면 상위의 disconnect 로직을 호출하면 될 것임
                            /*
                            if (InitProxyMethod != null)
                            {
                                if (_proxyCache != null && _proxyCache.Count > 0)
                                {
                                    var cache = ProxyCache.FirstOrDefault(x => x.Value.HostName == sender.GetType().Name);

                                    if (cache.Equals(default(KeyValuePair<Type, ProxySet>)) == false)
                                    {
                                        MethodInfo generic = InitProxyMethod.MakeGenericMethod(cache.Key);

                                        object[] param = new object[] { stageIndex };
                                        generic.Invoke(loadercommanagerobj, param);

                                        // TODO : 실패하면 어떻게, LoaderCommunicationManager의 DisConnectStage()를 호출?
                                    }
                                }
                            }
                            */
                        }
                    }
                }
            }
            catch (CommunicationException err)
            {
                LoggerManager.Exception(err);

                // Communication exceptions are normal when
                // closing the connection.
                communicationObject?.Abort();
            }
            catch (TimeoutException err)
            {
                LoggerManager.Exception(err);

                // Timeout exceptions are normal when closing
                // the connection.
                communicationObject?.Abort();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                // Any other exception and you should 
                // abort the connection and rethrow to 
                // allow the exception to bubble upwards.
                communicationObject?.Abort();
                throw;
            }
            finally
            {
                // This is just to stop you from trying to 
                // close it again (with the null check at the start).
                // This may not be necessary depending on
                // your architecture.
                sender = null;

                LoggerManager.Debug(logStr);
                Addlog(logStr);
            }
        }

        public void Proxy_Opening(object sender, EventArgs e)
        {
            try
            {
                string logStr = $"Proxy Opening. Sender = {sender}, cell index = {stageIndex}";

                LoggerManager.Debug(logStr);
                Addlog(logStr);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Proxy_Closing(object sender, EventArgs e)
        {
            try
            {
                string logStr = $"Proxy Closing. Sender = {sender}, cell index = {stageIndex}";

                LoggerManager.Debug(logStr);
                Addlog(logStr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Proxy_Opened(object sender, EventArgs e)
        {
            try
            {
                string logStr = $"Proxy Opened. Sender = {sender}, cell index = {stageIndex}";

                LoggerManager.Debug(logStr);
                Addlog(logStr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void Proxy_Closed(object sender, EventArgs e)
        {
            try
            {
                string logStr = $"Proxy closed. Sender = {sender}, cell index = {stageIndex}";

                LoggerManager.Debug(logStr);
                Addlog(logStr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Clear()
        {
            try
            {
                // TODO : 그냥 날려버려도 되는가?

                ProxyCache.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void AbortProxies()
        {
            try
            {
                for (int i = ProxyCache.Count - 1; i >= 0; i--)
                {
                    var item = ProxyCache.ElementAt(i);
                    var itemKey = item.Key;
                    var itemValue = item.Value;

                    ICommunicationObject comobj = (itemValue.Proxy as ICommunicationObject);

                    if (comobj != null)
                    {
                        comobj.Abort();
                        ReleaseEvent(comobj);
                    }
                }

                ProxyCache.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Close(Type type, object obj)
        {
            ICommunicationObject comobj = null;

            try
            {
                if (obj != null)
                {
                    comobj = obj as ICommunicationObject;

                    // State가 CommunicationState.Opened 또는 CommunicationState.Created인 경우, Close()를 호출해줍시다.
                    if ((comobj?.State == CommunicationState.Opened) || (comobj?.State == CommunicationState.Created))
                    {
                        try
                        {
                            comobj.Close();
                        }
                        catch (CommunicationException err)
                        {
                            LoggerManager.Error(err.ToString());
                            comobj.Abort();
                        }
                        catch (TimeoutException err)
                        {
                            LoggerManager.Error(err.ToString());
                            comobj.Abort();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error(err.ToString());
                            comobj.Abort();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                ProxyCache.Remove(type);
            }
        }

        public void Disconnect()
        {
            try
            {
                for (int i = ProxyCache.Count - 1; i >= 0; i--)
                {
                    var item = ProxyCache.ElementAt(i);
                    var itemKey = item.Key;
                    var itemValue = item.Value;

                    Close(itemKey, itemValue.Proxy);

                    ICommunicationObject comobj = itemValue.Proxy as ICommunicationObject;
                    ReleaseEvent(comobj);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

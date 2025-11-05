using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Loader;
using ProberInterfaces.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace RemoteServiceProxy
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DataGatewayProxy : DuplexClientBase<ILoaderDataGateway>, IFactoryModule, IDataGatewayProxy
    {
        public DataGatewayProxy(Uri uri, InstanceContext callback) :
                 base(callback, new ServiceEndpoint(
                     ContractDescription.GetContract(typeof(ILoaderDataGateway)),
                     new NetTcpBinding()
                     {
                         Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                         ReceiveTimeout = TimeSpan.MaxValue,
                         SendTimeout = new TimeSpan(0, 3, 0),
                         OpenTimeout = new TimeSpan(0, 3, 0),
                         CloseTimeout = new TimeSpan(0, 3, 0),
                         MaxBufferPoolSize = 2147483646,
                         MaxReceivedMessageSize = 2147483646,
                         ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                         {
                            MaxStringContentLength = 2147483647,
                             MaxArrayLength = 2147483647,
                            MaxNameTableCharCount = 16384
                         },
                         ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                     },
                     new EndpointAddress(uri)))
        {
            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }
        private object chnLockObj = new object();

        public void InitService(int chuckId)
        {
            try
            {
                LoggerManager.Debug($"DataGatewayProxy State is [{this.State}]");

                Channel.InitService(chuckId);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DeInitService(bool bForceAbort = false)
        {
            try
            {
                try
                {
                    if (!bForceAbort && IsOpened())
                        Close();
                    else
                        Abort();
                }
                catch (CommunicationException err)
                {
                    LoggerManager.Error(err.ToString());
                    Abort();
                }
                catch (TimeoutException err)
                {
                    LoggerManager.Error(err.ToString());
                    Abort();
                }
                catch (Exception err)
                {
                    LoggerManager.Error(err.ToString());
                    Abort();
                }
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
            }
            
        }

        public bool IsServiceAvailable()
        {
            bool retVal = false;
            try
            {
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
                            LoggerManager.Error($"DataGatewayProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"DataGateway Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"DataGateway Service service error.");
                LoggerManager.Exception(err);

                retVal = false;
            }

            return retVal;
        }

        public bool IsOpened()
        {
            bool retVal = false;

            try
            {
                if (State == CommunicationState.Opened || State == CommunicationState.Created)
                {
                    retVal = true;
                }
                else
                {
                    LoggerManager.Debug($"DataGatewayProxy state = {State}. Please check Connect state.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (IsOpened())
                {

                    Channel.NotifyStageAlarm(noticeCodeInfo);
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
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

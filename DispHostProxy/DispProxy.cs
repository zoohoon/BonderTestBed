using System;


namespace DispHostProxy
{
    using LogModule;
    using ProberInterfaces;
    using ServiceInterfaces;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    //using VisionParams.Camera;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DispProxy : DuplexClientBase<IImageDispHost>
    {
        public DispProxy(Uri uri, InstanceContext callback) :
                base(callback, new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IImageDispHost)),
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
                            //MaxDepth = 64,
                            MaxStringContentLength = 2147483647,
                            MaxArrayLength = 2147483647,
                            //MaxBytesPerRead = 4096,
                            MaxNameTableCharCount = 16384
                        },
                        ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                    },
                    new EndpointAddress(uri)))
        {
            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }
        object dispLockObject = new object();
        private int updateImageFrame = 0;
        public bool IsServiceAvailable()
        {
            bool retVal = false;
            try
            {
                lock (dispLockObject)
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
                            LoggerManager.Error($"DispProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"DispProxy Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"DispProxy Service service error.");
                retVal = false;
            }
            return retVal;
        }
        public void InitService(int chuckId)
        {
            try
            {
                lock (dispLockObject)
                {
                    if (IsOpened())
                    {
                        Channel.InitService(chuckId);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void DeInitService(bool bForceAbort = false)
        {
            Stopwatch stw = new Stopwatch();
            try
            {
                lock (dispLockObject)
                {
                    if (!bForceAbort && IsOpened())
                    {
                        LoggerManager.Debug($"[DispProxy] DeInitService() - UpdateFramCount is {updateImageFrame}");
                        Close();
                    }
                    else
                    {
                        Abort();
                    }
                }
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

        public CommunicationState GetState()
        {
            if (IsOpened())
            {
                CommunicationState state = CommunicationState.Faulted;
                lock (dispLockObject)
                {
                    state = Channel.GetState();
                }
                return state;
            }
            else
                return CommunicationState.Closed;
        }
        public void UpdateImage(ImageBuffer image)
        {
            try
            {
                lock (dispLockObject)
                {
                    if (IsOpened())
                    {
                        updateImageFrame++;
                        Channel.UpdateImage(image);
                        updateImageFrame--;
                    }
                }
            }
            catch (Exception err)
            {
                updateImageFrame--;
                LoggerManager.Error($"UpdateImage(): Error occurred. updateImageFrame is {updateImageFrame}," +
                    $" Err = {err.Message}");
            }
        }

        private bool IsOpened()
        {
            if (State == CommunicationState.Opened || State == CommunicationState.Created)
            {
                return true;
            }
            else
            {
                if (updateImageFrame > 0)
                {
                    updateImageFrame = 0;
                }

                return false;
            }
        }
    }
}

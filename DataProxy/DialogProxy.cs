using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteServiceProxy
{
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Windows.Input;

    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ReleaseServiceInstanceOnTransactionComplete = false,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DelegateEventProxy : DuplexClientBase<IDelegateEventHost>, IDisposable, IDialogServiceProxy
    {
        public DelegateEventProxy(Uri uri, InstanceContext callback) :
               base(callback, new ServiceEndpoint(
                   ContractDescription.GetContract(typeof(IDelegateEventHost)),
                   new NetTcpBinding()
                   {
                       Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                       ReceiveTimeout = TimeSpan.MaxValue,
                       SendTimeout = new TimeSpan(0, 3, 0),
                       OpenTimeout = new TimeSpan(0, 3, 0),
                       CloseTimeout = new TimeSpan(0, 3, 0),
                       MaxBufferPoolSize = 1024768,
                       
                       //MaxBufferSize = 2147483646,
                       MaxReceivedMessageSize = 2147483646,
                       
                       //ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                       //{
                       //    MaxDepth = 64,
                       //    MaxStringContentLength = 2147483647, 
                       //    MaxArrayLength = 2147483647,
                       //    MaxBytesPerRead = 4096,
                       //    MaxNameTableCharCount = 16384
                       //},
                       ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                   },
                   new EndpointAddress(uri)))
        {
            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }

        public void InitService(int chuckId)
        {
            try
            {
                if (IsOpened())
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
        public async Task<EnumMessageDialogResult> ShowMessageDialog(string Title, string Message, EnumMessageStyle enummessagesytel, string AffirmativeButtonText = "OK", string NegativeButtonText = "Cancel", string firstAuxiliaryButtonText = null, string secondAuxiliaryButtonText = null)
        {
            try
            {
                if (IsOpened())
                    return await Channel.ShowMessageDialog(Title, Message, enummessagesytel, AffirmativeButtonText, NegativeButtonText, firstAuxiliaryButtonText, secondAuxiliaryButtonText);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ShowMessageDialog(): Error occurred. Err = {err.Message}");
            }
            return EnumMessageDialogResult.UNDEFIND;
        }
        static SemaphoreSlim showWaitDialogSemaphore = new SemaphoreSlim(1, 1);
        static SemaphoreSlim hideWaitDialogSemaphore = new SemaphoreSlim(1, 1);
        public async Task ShowWaitCancelDialog(string hashcode, string message, CancellationTokenSource canceltokensource = null, string caller = "", string cancelButtonText = "Cancel")
        {
            try
            {
                int maxReconnectCount = 5;
                await showWaitDialogSemaphore.WaitAsync();
                var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;

                if (IsOpened())
                {
                    for (int i = 0; i < maxReconnectCount; i++)
                    {
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 25);
                            await Channel.ShowWaitCancelDialog(hashcode, message, canceltokensource, caller, cancelButtonText);
                            break;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Debug($"ShowWaitCancelDialog(): Hash = {hashcode}, Failed to connect to service. Retry connect...{i + 1} / {maxReconnectCount}");
                            LoggerManager.Exception(err);
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                }
                else
                {
                    if (IsServiceAvailable())
                    {
                        for (int i = 0; i < maxReconnectCount; i++)
                        {
                            try
                            {
                                (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 25);
                                await Channel.ShowWaitCancelDialog(hashcode, message, canceltokensource, caller, cancelButtonText);
                                break;
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Debug($"ShowWaitCancelDialog(): Hash = {hashcode}, Failed to connect to service. Retry connect...{i + 1} / {maxReconnectCount}");
                                LoggerManager.Exception(err);
                            }
                            finally
                            {
                                (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                showWaitDialogSemaphore.Release();
            }
        }

        public async Task CloseWaitCancelDialog(string hashcode)
        {
            try
            {
                await hideWaitDialogSemaphore.WaitAsync();
                int maxReconnectCount = 5;
                var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;

                if (IsOpened())
                {
                    for (int i = 0; i < maxReconnectCount; i++)
                    {
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 25);
                            await Channel.CloseWaitCancelDialog(hashcode);
                            break;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Debug($"CloseWaitCancelDialog(): Hash = {hashcode}, Failed to connect to service. Retry connect...{i + 1} / {maxReconnectCount}");
                            LoggerManager.Exception(err);
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                }
                else
                {
                    if (IsServiceAvailable())
                    {
                        for (int i = 0; i < maxReconnectCount; i++)
                        {
                            try
                            {
                                (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 25);
                                await Channel.CloseWaitCancelDialog(hashcode);
                                break;
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Debug($"CloseWaitCancelDialog(): Hash = {hashcode}, Failed to connect to service. Retry connect...{i + 1} / {maxReconnectCount}");
                                LoggerManager.Exception(err);
                            }
                            finally
                            {
                                (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"CloseWaitCancelDialog(): Connection lost. Retreive connection...");

                        for (int i = 0; i < maxReconnectCount; i++)
                        {
                            try
                            {
                                (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 10);
                                await Channel.CloseWaitCancelDialog(hashcode);
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Debug($"CloseWaitCancelDialog(): Hash = {hashcode}, Failed to connect to service. Retry connect...{i + 1} / {maxReconnectCount}");
                                LoggerManager.Exception(err);
                            }
                            finally
                            {
                                (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                            }
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
                hideWaitDialogSemaphore.Release();
            }
        }

        public async Task SetDataWaitCancelDialog(string message, string hashcoe, CancellationTokenSource canceltokensource = null, string cancelButtonText = "Cancel", bool restarttimer = false)
        {
            try
            {
                await Channel.SetDataWaitCancelDialog(message, hashcoe, canceltokensource, cancelButtonText, restarttimer);
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
                await Channel.ClearDataWaitCancelDialog(restarttimer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task<EventCodeEnum> ShowProgressDialog(string title, string message, object callerassembly = null,CancellationTokenSource cancelts = null,bool issetprogress = false,bool visibilitycancel = true)
        //{
        //    try
        //    {
        //        if (IsOpened())
        //            return await Channel.ShowProgressDialog(title, message, callerassembly, cancelts, issetprogress, visibilitycancel);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Error($"ShowProgressDialog(): Error occurred. Err = {err.Message}");
        //    }
        //    return EventCodeEnum.UNDEFINED;
        //}

        //public async Task<EventCodeEnum> CloseProgressDialog(object callerassembly = null)
        //{
        //    try
        //    {
        //        if (IsOpened())
        //            return await Channel.CloseProgressDialog(callerassembly);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Error($"CloseProgressDialog(): Error occurred. Err = {err.Message}");
        //    }
        //    return EventCodeEnum.UNDEFINED;
        //}

        //public async Task CloseAllProgressDialog()
        //{
        //    try
        //    {
        //        if (IsOpened())
        //        {
        //            await Channel.CloseAllProgressDialog();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Error($"CloseAllProgressDialog(): Error occurred. Err = {err.Message}");
        //    }

        //    return;
        //}

        public async Task<EnumMessageDialogResult> ShowSingleInputDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel", string subLabel = "")
        {
            EnumMessageDialogResult retval = EnumMessageDialogResult.NEGATIVE;

            try
            {
                if (IsOpened())
                {
                    retval = await Channel.ShowSingleInputDialog(Label, posBtnLabel, negBtnLabel, subLabel);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ShowSingleInputDialog(): Error occurred. Err = {err.Message}");
            }

            return retval;
        }

        public string GetInputDataSingleInput()
        {
            try
            {
                if (IsOpened())
                {
                    return Channel.GetInputDataSingleInput();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetInputDataSingleInput(): Error occurred. Err = {err.Message}");
            }

            return string.Empty;
        }
        //public async Task ShowMetroDialogChuckIndex(string viewAssemblyName, string viewClassName, string viewModelAssemblyName, string viewModelClassName, int chuckindex=-1,List<byte[]> data = null, bool waitunload = false)
        //{
        //    if (IsOpened())
        //        await Channel.ShowMetroDialogChuckIndex(viewAssemblyName, viewClassName, viewModelAssemblyName, viewModelClassName,
        //            chuckindex, data, waitunload);
        //}
        //public async Task CloseMetroDialogChuckIndex(string classname,int chuckIndex=-1)
        //{
        //    if (IsOpened())
        //        await Channel.CloseMetroDialogChuckIndex(classname, chuckIndex);
        //}
        public async Task ShowMetroDialog(string viewAssemblyName, string viewClassName, string viewModelAssemblyName, string viewModelClassName, List<byte[]> data = null, bool waitunload = true)
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.ShowMetroDialog(viewAssemblyName, viewClassName, viewModelAssemblyName, viewModelClassName, data, waitunload);
                }
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
                if (IsOpened())
                {
                    await Channel.CloseMetroDialog(classname);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool IsOpened()
        {
            bool retVal = false;
            try
            {
                if (State == CommunicationState.Opened || State == CommunicationState.Created)
                    retVal = true;
                else
                    retVal = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool IsServiceAvailable()
        {
            bool retVal = false;

            try
            {
                var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                try
                {
                    (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 10);
                    retVal = Channel.IsServiceAvailable();
                }
                catch (Exception)
                {
                    LoggerManager.Error($"DialogHost Service IsServiceAvailable timeout error.");
                }
                finally
                {
                    (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"DialogHost Service service error.");
                LoggerManager.Exception(err);

                retVal = false;
            }

            return retVal;
        }
    }
}

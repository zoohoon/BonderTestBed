using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteServiceProxy
{
    using LogModule;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.Enum;
    using ProberErrorCode;
    using System.Collections.ObjectModel;
    using ProberInterfaces.ViewModel;
    using ProberInterfaces.State;
    using SharpDXRender.RenderObjectPack;
    using System.Windows;
    using ProberInterfaces.Temperature;
    using System.Threading;
    using ProberInterfaces.ControlClass.ViewModel.Wafer.Sequence;
    using ProberInterfaces.Loader.RemoteDataDescription;
    using ProberInterfaces.PMI;
    using SerializerUtil;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.ControlClass.ViewModel;
    using System.IO;
    using ProberInterfaces.ControlClass.ViewModel.PMI;
    using ProberInterfaces.Param;
    using ProberInterfaces.Utility;
    using ProberInterfaces.SequenceRunner;
    using ProberInterfaces.BinData;
    using LogModule.LoggerParam;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.WaferAlignEX;
    using ProberVision;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RemoteMediumProxy : DuplexClientBase<ILoaderRemoteMediator>, IRemoteMediumProxy
    {
        public int Index = -1;
        public RemoteMediumProxy(int port, InstanceContext callback, string ip = null) :
               base(callback, new ServiceEndpoint(
                   ContractDescription.GetContract(typeof(ILoaderRemoteMediator)),
                   new NetTcpBinding()
                   {
                       ReceiveTimeout = TimeSpan.MaxValue,
                       MaxBufferPoolSize = 524288,
                       MaxReceivedMessageSize = 2147483647,
                       MaxBufferSize = 2147483647,
                       //SendTimeout = new TimeSpan(0, 10, 0),
                       //OpenTimeout = new TimeSpan(0, 10, 0),
                       //CloseTimeout = new TimeSpan(0, 10, 0),
                       //ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                       //{
                       //    MaxDepth = 32,
                       //    MaxStringContentLength = 2147483647,
                       //    MaxArrayLength = 2147483647,
                       //    MaxBytesPerRead = 4096,
                       //    MaxNameTableCharCount = 16384
                       //},
                       Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                       ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                   }, new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.LoaderRemoteMediatorService}")))

        {
            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }
        private object chnLockObj = new object();
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
                            LoggerManager.Error($"RemoteMediumProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"RemoteMedium Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RemoteMedium Service service error.");
                LoggerManager.Exception(err);

                retVal = false;
            }

            return retVal;
        }


        public void InitService()
        {
            try
            {
                LoggerManager.Debug($"RemoteMediumProxy State is [{this.State}]");

                Channel.InitService();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void DeInitService()
        {
            //Dispose
        }

        object channelLockObj = new object();

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);


        public bool IsOpened()
        {
            bool retVal = false;

            try
            {
                if (State == CommunicationState.Opened | State == CommunicationState.Created)
                {
                    retVal = true;
                }
                else
                {
                    LoggerManager.Debug($"RemoteMediumProxy state = {State}. Please check Connect state.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public CommunicationState GetCommunicationState()
        {
            return this.State;
        }

        #region PMI

        public byte[] GetPMIDevParam()
        {
            byte[] retval = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetPMIDevParam();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
            //}
        }


        #endregion

        #region ..ResultMap

        public byte[] GetResultMapConvParam()
        {
            byte[] retval = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetResultMapConvParam();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
            //}
        }

        public EventCodeEnum SaveResultMapConvParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    retval = Channel.SaveResultMapConvParam();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetResultMapConvIParam(byte[] param)
        {
            try
            {
                if (IsOpened())
                {
                    Channel.SetResultMapConvIParam(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }
        public bool SetResultMapByFileName(byte[] device, string resultmapname)
        {
            bool retval = false;

            try
            {
                if (IsOpened())
                {
                    retval = Channel.SetResultMapByFileName(device, resultmapname);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public string[] GetNamerAliaslist()
        {
            string[] retval = null;

            try
            {
                if (IsOpened())
                {
                    retval = Channel.GetNamerAliaslist();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion
        #region Probing

        public EventCodeEnum ProbingModuleSaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //lock (channelLockObj)
                //{
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.ProbingModuleSaveDevParameter();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetProbingDevParam(IParam param)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    byte[] p = this.ObjectToByteArray(param);

                    Channel.SetProbingDevParam(p);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }   
        public byte[] GetProbingDevParam()
        {
            byte[] retval = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetProbingDevParam();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
            //}
        }       
        public byte[] GetBinDevParam()
        {
            byte[] retval = null;

            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetBinDevParam();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
        }

        public EventCodeEnum SetBinInfos(List<IBINInfo> binInfos)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (IsOpened())
            {
                try
                {
                    byte[] bytes = this.ObjectToByteArray(binInfos);

                    retval = Channel.SetBinInfos(bytes);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
        }

        public EventCodeEnum SaveBinDevParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (IsOpened())
            {
                try
                {
                    retval = Channel.SaveBinDevParam();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
        }


        public async Task<MarkShiftValues> GetUserSystemMarkShiftValue()
        {
            MarkShiftValues retval = null;
            try
            {
                if (IsOpened())
                {
                    retval = await Channel.GetUserSystemMarkShiftValue();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        #endregion

        #region //..Common (WaferObject)
        public void SetWaferPhysicalInfo(IPhysicalInfo physinfo)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    Channel.SetWaferPhysicalInfo(this.ObjectToByteArray(physinfo));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }

        public byte[] GetWaferDevObjectbyFileToStream()
        {
            byte[] ret = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    ret = Channel.GetWaferDevObjectbyFileToStream();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return ret;
            //}
        }
        public byte[] GetWaferDevObject()
        {
            byte[] retval = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetWaferDevObject();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
            //}
        }

        #endregion

        #region //..Common(ViewModelManagerModule)
        public Guid GetViewGuidFromViewModelGuid(Guid guid)
        {
            Guid retval = new Guid();

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetViewGuidFromViewModelGuid(guid);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
            //}
        }
        public async Task<EventCodeEnum> PageSwitched(Guid viewGuid, object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                //retVal = Channel.PageSwitched(viewGuid, parameter).GetAwaiter().GetResult();
                retVal = await Channel.PageSwitched(viewGuid, parameter);
            }

            return retVal;
            //}
        }

        public async Task<EventCodeEnum> Cleanup(Guid viewGuid, object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                //retVal = Channel.CleanUp(viewGuid, parameter).GetAwaiter().GetResult();
                retVal = await Channel.CleanUp(viewGuid, parameter);
            }
            return retVal;
            //}
        }

        #endregion

        #region //..PNP

        public EventCodeEnum GetCuiBtnParam(object module, Guid cuiguid, out Guid viewguid, out List<Guid> stepguids, bool extrastep = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                retval = Channel.GetCuiBtnParam(module, cuiguid, out viewguid, out stepguids, extrastep);
            }
            else
            {
                viewguid = new Guid();
                stepguids = new List<Guid>();
            }

            return retval;
            //}
        }

        public ObservableCollection<ObservableCollection<CategoryNameItems>> GetCategoryNameList(string modulename, string interfacename, Guid cuiguid, bool extrastep = false)
        {
            ObservableCollection<ObservableCollection<CategoryNameItems>> retval = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetCategoryNameList(modulename, interfacename, cuiguid, extrastep);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
            //}
        }

        public PNPDataDescription GetPNPDataDescriptor()
        {
            PNPDataDescription retval = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetPNPDataDescriptor();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
            //}
        }

        public PnpUIData GetRemoteData()
        {
            PnpUIData retVal = null;
            lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retVal = Channel.GetRemoteData();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retVal;
            }
        }

        public void PNPSetPackagableParams()
        {
            try
            {
                //lock (channelLockObj)
                //{
                if (IsOpened())
                {
                    Channel.PNPSetPackagableParams();
                }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public List<byte[]> PNPGetPackagableParams()
        {
            List<byte[]> param = null;
            try
            {
                //lock (channelLockObj)
                //{
                if (IsOpened())
                {
                    param = Channel.PNPGetPackagableParams();
                }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return param;
        }
        public PMITemplateMiniViewModel GetPMITemplateMiniViewModel()
        {
            PMITemplateMiniViewModel retval = null;
            byte[] bytes;
            object target;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    bytes = Channel.GetPMITemplateMiniViewModel();

                    bool vaild = SerializeManager.DeserializeFromByte(bytes, out target, typeof(PMITemplateMiniViewModel));

                    if (vaild == true && target != null)
                    {
                        retval = target as PMITemplateMiniViewModel;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
            return retval;
        }

        public IPnpSetup GetPnpSetup()
        {
            IPnpSetup retval = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetPnpSetup();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
            //}
        }

        public List<RenderContainer> GetRenderContainers()
        {
            List<RenderContainer> retval = null;

            if (IsOpened())
            {
                var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                try
                {
                    (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 15);
                    retval = Channel.GetRenderContainers();
                }
                catch (Exception)
                {
                    LoggerManager.Error($"RemoteMediumProxy GetRenderContainers timeout error");
                }
                finally
                {
                    (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                }
            }

            return retval;
        }


        public void ButtonExecuteSync(object param, PNPCommandButtonType type)
        {
            if (IsOpened())
            {
                try
                {
                    Channel.PNPButtonExecuteSync(param, type);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }
        public async Task ButtonExecuteAsync(object param, PNPCommandButtonType type)
        {
            if (IsOpened())
            {
                try
                {
                    if (type == PNPCommandButtonType.ONEBUTTON)
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                            await Channel.PNPButtonExecuteAsync(param, type);
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"RemoteMediumProxy ButtonExecuteAsync timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        await Channel.PNPButtonExecuteAsync(param, type);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public PNPCommandButtonDescriptor GetPNPButtonDescriptor(PNPCommandButtonType param)
        {
            PNPCommandButtonDescriptor retval = null;

            if (IsOpened())
            {
                try
                {
                    retval = Channel.GetPNPCommandButtonDescriptorCurStep(param);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retval;
        }

        public EnumProberCam GetCamType()
        {
            EnumProberCam retval = EnumProberCam.UNDEFINED;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetCamType();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }
        public void SetSetupState(string header = null)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetSetupState(header);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void SetMiniViewTarget(object miniView)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetMiniViewTarget(miniView);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public EnumMoudleSetupState GetSetupState(string header = null)
        {
            EnumMoudleSetupState retval = EnumMoudleSetupState.UNDEFINED;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetSetupState(header);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }
        public async Task<EventCodeEnum> StepPageSwitching(string moduleheader, object parameter)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    retval = await Channel.StepPageSwitching(moduleheader, parameter);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }

            return retval;
        }

        public async Task<EventCodeEnum> StepCleanup(string moduleheader, object parameter)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    retval = await Channel.StepCleanup(moduleheader, parameter);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }

            return retval;
        }

        public bool StepIsParameterChanged(string moduleheader, bool issave)
        {
            bool retval = false;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.StepIsParameterChanged(moduleheader, issave);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public EventCodeEnum StepParamValidation(string moduleheader)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.StepParamValidation(moduleheader);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }
        public async Task SetCurrentStep(string moduleheader)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.SetCurrentStep(moduleheader);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void ApplyParams(List<byte[]> parameters)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.ApplyParams(parameters);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void CloseAdvanceSetupView()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.CloseAdvanceSetupView();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void SetDislayPortTargetRectInfo(double left, double top)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetDislayPortTargetRectInfo(left, top);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        //public void SetPackagableParams()
        //{
        //    lock(Channel)
        //    {
        //        Channel.SetPackagableParams();
        //    }
        //}

        #endregion

        #region //..Jog

        /// Light Jog
        public void ChangeCamPosition(EnumProberCam cam)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.ChangeCamPosition(cam);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void UpdateCamera(EnumProberCam cam, string interfaceType)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.UpdateCamera(cam, interfaceType);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void SetLightValue(int intensity)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetLightValue(intensity);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void SetLightChannel(EnumLightType lightchnnel)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetLightChannel(lightchnnel);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public int GetLightValue(EnumLightType lightchnnel)
        {
            int retval = 0;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetLightValue(lightchnnel);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public List<EnumLightType> GetLightTypes()
        {
            List<EnumLightType> retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetLightTypes();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }
        public bool CheckSWLimit(EnumProberCam cam)
        {
            bool retval = true;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {

                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        /// Motion Jog
        public void StickIndexMove(JogParam parameter, bool setzoffsetenable)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.StickIndexMove(parameter, setzoffsetenable);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void StickStepMove(JogParam parameter)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.StickStepMove(parameter);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }



        #endregion

        #region //..DisplayPort
        public void StageMove()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        #endregion

        #region //..Soaking
        public void SetSoakingParam(byte[] param)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetSoakingParam(param);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public SoakingRecipeDataDescription GetSoakingRecipeInfo()
        {
            SoakingRecipeDataDescription retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetSaokingRecipeInfo();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        #endregion

        #region //..RetestViewModel

        public async Task RetestViewModel_PageSwitched()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.RetestViewModel_PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        public async Task RetestViewModel_Cleanup()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.RetestViewModel_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        public void RetestViewModel_SetRetestIParam(byte[] param)
        {
            try
            {
                if (IsOpened())
                {
                    Channel.RetestViewModel_SetRetestIParam(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        #endregion

        #region //..PolishWaferMakeSourceVM

        public async Task PolishWaferMakeSourceVM_PageSwitched()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.PolishWaferMakeSourceVM_PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task PolishWaferMakeSourceVM_Cleanup()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.PolishWaferMakeSourceVM_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        //public void PolishWaferMakeSourceVM_SetPolishWaferIParam(byte[] param)
        //{
        //    try
        //    {
        //        semaphoreSlim.WaitAsync();

        //        if (IsOpened())
        //        {
        //            Channel.PolishWaferMakeSourceVM_SetPolishWaferIParam(param);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //semaphoreSlim.Release();
        //    }
        //}

        //public void PolishWaferMakeSourceVM_SetSelectedObjectCommand(byte[] info)
        //{
        //    try
        //    {
        //        semaphoreSlim.WaitAsync();

        //        if (IsOpened())
        //        {
        //            Channel.PolishWaferMakeSourceVM_SetSelectedObjectCommand(info);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //semaphoreSlim.Release();
        //    }
        //}

        public async Task PolishWaferMakeSourceVM_AddSourceCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.PolishWaferMakeSourceVM_AddSourceCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task PolishWaferMakeSourceVM_RemoveSourceCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.PolishWaferMakeSourceVM_RemoveSourceCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void PolishWaferMakeSourceVM_AssignCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.PolishWaferMakeSourceVM_AssignCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void PolishWaferMakeSourceVM_RemoveCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.PolishWaferMakeSourceVM_RemoveCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void UpdateCleaningParameters(string sourcename)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.PolishWaferMakeSourceVM_UpdateCleaningParameters(sourcename);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        #endregion

        #region //..PolishWaferRecipeSettingVM

        public async Task PolishWaferRecipeSettingVM_PageSwitched()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.PolishWaferRecipeSettingVM_PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task PolishWaferRecipeSettingVM_Cleanup()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.PolishWaferRecipeSettingVM_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task PolishWaferRecipeSettingVM_IntervalAddCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.PolishWaferRecipeSettingVM_IntervalAddCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        //public async Task PolishWaferRecipeSettingVM_CleaningDeleteCommand(PolishWaferIndexModel param)
        //{
        //    try
        //    {
        //        //await semaphoreSlim.WaitAsync();

        //        if (IsOpened())
        //        {
        //            byte[] byteparam = this.ObjectToByteArray(param);

        //            await Channel.PolishWaferRecipeSettingVM_CleaningDeleteCommandExcute(byteparam);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //semaphoreSlim.Release();
        //    }
        //}

        public void PolishWaferRecipeSettingVM_CleaningDelete(PolishWaferIndexModel param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    //byte[] byteparam = this.ObjectToByteArray(param);
                    Channel.PolishWaferRecipeSettingVM_CleaningDelete(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public void PolishWaferRecipeSettingVM_SetPolishWaferIParam(byte[] param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.PolishWaferRecipeSettingVM_SetPolishWaferIParam(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task PolishWaferRecipeSettingVM_SetSelectedInfos(SelectionUIType selectiontype, byte[] cleaningparam, byte[] pwinfo, byte[] intervalparam, int intervalindex, int cleaningindex)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.PolishWaferRecipeSettingVM_SetSelectedInfos(selectiontype, cleaningparam, pwinfo, intervalparam, intervalindex, cleaningindex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        //public async Task PolishWaferRecipeSettingVM_CleaningAddCommand(object param)
        //{
        //    try
        //    {
        //        //await semaphoreSlim.WaitAsync();

        //        if (IsOpened())
        //        {
        //            await Channel.PolishWaferRecipeSettingVM_CleaningAddCommandExcute(param);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //semaphoreSlim.Release();
        //    }
        //}

        public void PolishWaferRecipeSettingVM_CleaningAdd(int index)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.PolishWaferRecipeSettingVM_CleaningAdd(index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        //public async Task PolishWaferRecipeSettingVM_IntervalDeleteCommand(object param)
        //{
        //    //await semaphoreSlim.WaitAsync();
        //    try
        //    {
        //        if (IsOpened())
        //        {
        //            await Channel.PolishWaferRecipeSettingVM_IntervalDeleteCommandExcute(param);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //semaphoreSlim.Release();
        //    }
        //}
        public void PolishWaferRecipeSettingVM_IntervalDelete(int index)
        {
            //await semaphoreSlim.WaitAsync();
            try
            {
                if (IsOpened())
                {
                    Channel.PolishWaferRecipeSettingVM_IntervalDelete(index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        #endregion

        #region //..PolishWafer
        public void SetPolishWaferParam(byte[] param)
        {
            //await semaphoreSlim.WaitAsync();

            try
            {
                if (IsOpened())
                {
                    Channel.SetPolishWaferParam(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task<EventCodeEnum> DoManualPolishWaferCleaningCommand(byte[] param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 10, 0);
                        retval = await Channel.DoManualPolishWaferCleaningCommand(param);
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy DoManualPolishWaferCleaningCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public async Task ManualPolishWaferFocusingCommand(byte[] param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 10, 0);
                        await Channel.ManualPolishWaferFocusingCommand(param);
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy ManualPolishWaferFocusingCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        #endregion

        #region ..Sequence Maker

        public SequenceMakerDataDescription GetSequenceMakerInfo()
        {
            SequenceMakerDataDescription retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetSequenceMakerInfo();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public List<DeviceObject> GetUnderDutDIEs()
        {
            List<DeviceObject> retval = null;

            try
            {
                if (IsOpened())
                {
                    retval = Channel.GetUnderDutDevices();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task SequenceMakerVM_Cleanup()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.SequenceMakerVM_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task SequenceMakerVM_PageSwitched()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 3, 0);
                        await Channel.SequenceMakerVM_PageSwitched();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy SequenceMakerVM_PageSwitched timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task SequenceMakerVM_SetMXYIndex(Point mxyindex)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.SequenceMakerVM_SetMXYIndex(mxyindex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task<EventCodeEnum> SequenceMakerVM_GetUnderDutDices(MachineIndex mxy)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            //await semaphoreSlim.WaitAsync();
            try
            {


                if (IsOpened())
                {
                    retval = await Channel.SequenceMakerVM_GetUnderDutDices(mxy);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }

            return retval;
        }

        public async Task SequenceMakerVM_MoveToPrevSeqCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.SequenceMakerVM_MoveToPrevSeqCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task SequenceMakerVM_MoveToNextSeqCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.SequenceMakerVM_MoveToNextSeqCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task SequenceMakerVM_InsertSeqCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.SequenceMakerVM_InsertSeqCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void SequenceMakerVM_ChangeAutoAddSeqEnable(bool flag)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.SequenceMakerVM_ChangeAutoAddSeqEnable(flag);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task SequenceMakerVM_DeleteSeqCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.SequenceMakerVM_DeleteSeqCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task SequenceMakerVM_SeqNumberSeletedCommand(object param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 3, 0);
                        await Channel.SequenceMakerVM_SeqNumberSeletedCommandExcute(param);
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy SequenceMakerVM_SeqNumberSeletedCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task SequenceMakerVM_AutoMakeSeqCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.SequenceMakerVM_AutoMakeSeqCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task SequenceMakerVM_MapMoveCommand(object param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.SequenceMakerVM_MapMoveCommandExcute(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task SequenceMakerVM_DeleteAllSeqCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 3, 0);
                        await Channel.SequenceMakerVM_DeleteAllSeqCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy SequenceMakerVM_DeleteAllSeqCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        #endregion

        #region //..InspectionVM

        public async Task<InspcetionDataDescription> GetInspectionInfo()
        {
            InspcetionDataDescription retval = null;

            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    retval = await Channel.GetInspectionInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }

            return retval;
        }
        public EventCodeEnum UpdateSysparam()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;
                if (IsOpened())
                {
                    retval = Channel.UpdateSysparam();
                }
                if (retval != EventCodeEnum.NONE)
                {
                    retval = EventCodeEnum.UNDEFINED;
                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
        public CatCoordinates GetSetTemperaturePMShifhtValue()
        {
            CatCoordinates retval = null;

            try
            {
                //lock (channelLockObj)
                //{
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetSetTemperaturePMShifhtValue();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Dictionary<double, CatCoordinates> GetTemperaturePMShifhtTable()
        {
            Dictionary<double, CatCoordinates> retval = null;

            try
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetTemperaturePMShifhtTable();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task Inspection_SetFromCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_SetFromCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task<EventCodeEnum> Inspection_CheckPMShiftLimit(double checkvalue)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    ret = await Channel.InspectionVM_CheckPMShiftLimit(checkvalue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public async Task Inspection_SaveCommand(InspcetionDataDescription info)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_SaveCommandExcute(info);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task Inspection_ApplyCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_ApplyCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task Inspection_SystemApplyCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_SystemApplyCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task Inspection_ClearCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_ClearCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task Inspection_SystemClearCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_SystemClearCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task Inspection_PrevDutCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_PrevDutCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task Inspection_NextDutCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_NextDutCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task Inspection_PadPrevCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_PadPrevCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task Inspection_PadNextCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_PadNextCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        //public async Task Inspection_ManualSetIndexCommand()
        //{
        //    try
        //    {
        //        //await semaphoreSlim.WaitAsync();

        //        if (IsOpened())
        //        {
        //            await Channel.InspectionVM_ManualSetIndexCommandExcute();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //semaphoreSlim.Release();
        //    }
        //}
        public async Task Inspection_PinAlignCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.InspectionVM_PinAlignCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy Inspection_PinAlignCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task Inspection_WaferAlignCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.InspectionVM_WaferAlignCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy Inspection_WaferAlignCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task Inspection_SavePads()
        {
            //int retVal = 0;
            try
            {
                //semaphoreSlim.Wait();

                if (IsOpened())
                {
                    await Channel.InspectionVM_SavePads();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Inspection_SavePads(): Error occurred. Err = {err.Message}");
            }
            finally
            {
                //semaphoreSlim.Release();
            }

            //return retVal;
        }

        public async Task Inspection_SaveTempOffset(ObservableDictionary<double, CatCoordinates> table)
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.InspectionVM_SaveTempOffset(table);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Inspection_SavePads(): Error occurred. Err = {err.Message}");
            }
        }
        public void Inspection_ChangeXManualCommand()
        {

            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.InspectionVM_ChangeXManualCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void Inspection_ChangeYManualCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.InspectionVM_ChangeYManualCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void Inspection_ChangeXManualIndex(long index)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.InspectionVM_ChangeXManualIndex(index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void Inspection_ChangeYManualIndex(long index)
        {

            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.InspectionVM_ChangeYManualIndex(index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task InspectionVM_PageSwitched()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.InspectionVM_PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task InspectionVM_Cleanup()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.InspectionVM_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region PMI Viewer

        public async Task PMIViewer_PageSwitched()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.PMIViewer_PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }
        public int PMIViewer_GetTotalImageCount()
        {
            int retval = 0;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.PMIViewer_GetTotalImageCount();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}

            return retval;
        }

        public void PMIViewer_UpdateFilterDatas(DateTime Startdate, DateTime Enddate, PadStatusResultEnum Status)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    Channel.PMIViewer_UpdateFilterDatas(Startdate, Enddate, Status);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }

        public void PMIViewer_LoadImage()
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    Channel.PMIViewer_LoadImage();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }

        public PMIImageInformationPack PMIViewer_GetImageFileData(int index)
        {
            PMIImageInformationPack retval = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.PMIViewer_GetImageFileData(index);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}

            return retval;
        }

        public ObservableCollection<PMIWaferInfo> PMIViewer_GetWaferlist()
        {
            ObservableCollection<PMIWaferInfo> retval = null;

            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retval = Channel.PMIViewer_GetWaferlist();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}

            return retval;
        }

        public void PMIViewer_ChangedWaferListItem(PMIWaferInfo pmiwaferinfo)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    Channel.PMIViewer_ChangedWaferListItem(pmiwaferinfo);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }

        public void PMIViewer_WaferListClear()
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    Channel.PMIViewer_WaferListClear();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }

        #endregion

        #region DutEditor

        public void DutEditor_ChangedSelectedCoordM(MachineIndex param)
        {

            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.DutEditor_ChangedSelectedCoordM(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void DutEditor_ChangedFirstDutM(MachineIndex param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.DutEditor_ChangedChangedFirstDutM(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void DutEditor_ChangedAddCheckBoxIsChecked(bool? param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.DutEditor_ChangedAddCheckBoxIsChecked(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public DutEditorDataDescription GetDutEditorInfo()
        {
            DutEditorDataDescription retval = null;

            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    retval = Channel.GetDutEditorInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }

            return retval;
        }

        public byte[] DutEditor_GetDutlist()
        {
            byte[] dutlist = null;

            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    dutlist = Channel.DutEditor_GetDutlist();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }

            return dutlist;
        }
        public void DutEditor_ImportFilePath(string filePath)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.DutEditor_ImportFilePath(filePath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }

        }

        public async Task<EventCodeEnum> DutEditor_CmdImportCardDataCommand(Stream fileStream)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    retVal = await Channel.DutEditor_CmdImportCardDataCommandExcute(fileStream);
                }
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task DutEditor_InitializePalletCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DutEditor_InitializePalletCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task DutEditor_DutAddCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DutEditor_DutAddCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();

            }
        }

        public async Task DutEditor_DutDeleteCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DutEditor_DutDeleteCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void DutEditor_ChangedAddCheckBoxIsChecked(bool param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.DutEditor_ChangedAddCheckBoxIsChecked(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void DutEditor_ZoomInCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.DutEditor_ZoomInCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void DutEditor_ZoomOutCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.DutEditor_ZoomOutCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task DutEditor_DutEditerMoveCommand(EnumArrowDirection param)
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.DutEditor_DutEditerMoveCommandExcute(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task DutEditor_DutAddMouseDownCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DutEditor_DutAddMouseDownCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task DutEditor_Cleanup()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DutEditor_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task DutEditor_PageSwitched()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DutEditor_PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        #endregion

       #region //..Manual Contact.

        public async Task ManualContactVM_PageSwitched()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.ManualContactVM_PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task ManualContactVM_Cleanup()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.ManualContactVM_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public bool GetManaulContactMovingStage()
        {
            bool retval = true;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetManaulContactMovingStage();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public ManaulContactDataDescription GetManualContactInfo()
        {
            ManaulContactDataDescription retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetManualContactInfo();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }
        public async Task SetMCM_XYInex(Point index)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.SetMCM_XYInex(index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void MCMIncreaseX()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.MCMIncreaseX();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void MCMIncreaseY()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.MCMIncreaseY();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void MCMDecreaseX()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.MCMDecreaseX();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void MCMDecreaseY()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.MCMDecreaseY();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void MCMSetIndex(EnumMovingDirection dirx, EnumMovingDirection diry)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.MCMSetIndex(dirx, diry);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task ManualContact_FirstContactSetCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_FirstContactSetCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task ManualContact_AllContactSetCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_AllContactSetCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task ManualContact_ResetContactStartPositionCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_ResetContactStartPositionCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task ManualContact_OverDriveTBClickCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_OverDriveTBClickCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }


        public async Task ManualContact_CPC_Z1_ClickCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_CPC_Z1_ClickCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }


        public async Task ManualContact_CPC_Z2_ClickCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_CPC_Z2_ClickCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void MCMChangeOverDrive(string overdrive)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.MCMChangeOverDrive(overdrive);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void MCMChangeCPC_Z1(string z1)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.MCMChangeCPC_Z1(z1);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void MCMChangeCPC_Z2(string z2)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.MCMChangeCPC_Z2(z2);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task ManualContact_ChangeZUpStateCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_ChangeZUpStateCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task ManualContact_MoveToWannaZIntervalPlusCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_MoveToWannaZIntervalPlusCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task ManualContact_MoveToWannaZIntervalMinusCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_MoveToWannaZIntervalMinusCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task ManualContact_WantToMoveZIntervalTBClickCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_WantToMoveZIntervalTBClickCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task ManualContact_SetOverDriveCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.ManualContactVM_SetOverDriveCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void GetOverDriveFromProbingModule()
        {
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GetOverDriveFromProbingModule();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        //public void MCMSetAlawaysMoveToTeachDie(bool flag)
        //{
        //    //lock (channelLockObj)
        //    {
        //        if (IsOpened())
        //        {
        //            try
        //            {
        //                Channel.MCMSetAlawaysMoveToFirstDut(flag);
        //            }
        //            catch (Exception err)
        //            {
        //                LoggerManager.Exception(err);
        //            }
        //        }
        //    }
        //}

        #endregion       

        #region //..Soaking Recipe

        public void SoakingRecipe_DropDownClosedCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SoakingRecipeVM_DropDownClosedCommandExecute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        #endregion


        #region //..Wafer Map Maker VM
        public void WaferMapMaker_UpdateWaferSize(EnumWaferSize waferSize)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMaker_UpdateWaferSize(waferSize);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void WaferMapMaker_UpdateWaferSizeOffset(double WaferSizeOffset)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMaker_UpdateWaferSizeOffset(WaferSizeOffset);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void WaferMapMaker_UpdateDieSizeX(double diesizex)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMakerVM_UpdateDieSizeX(diesizex);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void WaferMapMakerVM_WaferSubstrateType(WaferSubstrateTypeEnum wafersubstratetype)
        {
            lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMakerVM_WaferSubstrateType(wafersubstratetype);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void WaferMapMaker_UpdateDieSizeY(double diesizey)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMakerVM_UpdateDieSizeY(diesizey);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void WaferMapMaker_UpdateThickness(double thickness)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMakerVM_UpdateThickness(thickness);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void WaferMapMake_UpdateEdgeMargin(double margin)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMakeVM_UpdateEdgeMargin(margin);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void WaferMapMaker_NotchAngle(double notchangle)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMakerVM_NotchAngle(notchangle);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void WaferMapMaker_NotchType(WaferNotchTypeEnum notchtype)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMakerVM_NotchType(notchtype);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void WaferMapMaker_NotchAngleOffset(double notchangleoffset)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMakerVM_NotchAngleOffset(notchangleoffset);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task WaferMapMaker_Cleanup()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.WaferMapMaker_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }


        public async Task WaferMapMaker_ApplyCreateWaferMapCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 10, 0);
                        await Channel.WaferMapMakerVM_ApplyCreateWaferMapCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy WaferMapMaker_ApplyCreateWaferMapCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                    watch.Stop();

                    LoggerManager.Debug($"[{this.GetType().Name}] WaferMapMaker_ApplyCreateWaferMapCommand() : {watch.ElapsedMilliseconds} ms");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task WaferMapMaker_MoveToWaferThicknessCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.WaferMapMakerVM_MoveToWaferThicknessCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();

            }
        }
        public async Task WaferMapMaker_AdjustWaferHeightCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.WaferMapMakerVM_AdjustWaferHeightCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task<EventCodeEnum> WaferMapMaker_CmdImportWaferData(Stream fileStream)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    retVal = await Channel.WaferMapMakerVM_CmdImportWaferDataCommandExcute(fileStream);
                }
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public void WaferMapMaker_ImportFilePath(string filePath)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.WaferMapMakerVM_ImportFilePath(filePath);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void WaferMapMakerVM_SetHighStandardParam(HeightPointEnum heightpoint)
        {
            if (IsOpened())
            {
                try
                {
                    Channel.WaferMapMakerVM_SetHighStandardParam(heightpoint);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public HeightPointEnum WaferMapMakerVM_GetHeightProfiling()
        {
            HeightPointEnum retVal = HeightPointEnum.UNDEFINED;
            if (IsOpened())
            {
                try
                {
                    retVal = Channel.WaferMapMakerVM_GetHeightProfiling();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }
        #endregion

        #region //..Device Change (FilManager)

        public async Task GetParamFromDevice(DeviceInfo device)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DeviceChange_GetParamFromDeviceCommandExcute(device);

                    //await Channel.DeviceChange_GetParamFromDeviceCommandExcute(device);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public DeviceChangeDataDescription GetDeviceChangeInfo()
        {
            DeviceChangeDataDescription retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetDeviceChangeInfo();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public async Task DeviceChange_GetDevList(bool isPageSwiching = false)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.DeviceChange_GetDevList(isPageSwiching);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void DeviceChange_RefreshDevListCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.DeviceChange_RefreshDevListCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void DeviceChange_ClearSearchDataCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.DeviceChange_ClearSearchDataCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void DeviceChange_SearchTBClickCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.DeviceChange_SearchTBClickCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void DeviceChange_PageSwitchingCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.DeviceChange_PageSwitchingCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task DeviceChange_ChangeDeviceCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DeviceChange_ChangeDeviceCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task DeviceChangeWithName_ChangeDeviceCommand(string deviceName)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DeviceChangeWithName_ChangeDeviceCommandExcute(deviceName);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task DeviceChange_CreateNewDeviceCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.DeviceChange_CreateNewDeviceCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy DeviceChange_CreateNewDeviceCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task DeviceChange_SaveAsDeviceCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.DeviceChange_SaveAsDeviceCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy DeviceChange_SaveAsDeviceCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task DeviceChange_DeleteDeviceCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.DeviceChange_DeleteDeviceCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        #region ChuckPlanarity

        public async Task ChuckPlanarity_ChuckMoveCommand(EnumChuckPosition param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.ChuckPlanarity_ChuckMoveCommandExcute(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task ChuckPlanarity_MeasureOnePositionCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.ChuckPlanarity_MeasureOnePositionCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy ChuckPlanarity_MeasureOnePositionCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task ChuckPlanarity_SetAdjustPlanartyFunc()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.ChuckPlanarity_SetAdjustPlanartyFuncExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy ChuckPlanarity_SetAdjustPlanartyFunc timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task ChuckPlanarity_MeasureAllPositionCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.ChuckPlanarity_MeasureAllPositionCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy ChuckPlanarity_MeasureAllPositionCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void ChuckPlanarity_ChangeMarginValue(double value)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.ChuckPlanarity_ChangeMarginValue(value);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void ChuckPlanarity_FocusingRangeValue(double value)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.ChuckPlanarity_FocusingRangeValue(value);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public ChuckPlanarityDataDescription GetChuckPlanarityInfo()
        {
            ChuckPlanarityDataDescription retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.ChuckPlanarity_GetChuckPlanarityInfo();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public async Task ChuckPlanarity_PageSwitched()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.ChuckPlanarity_PageSwitched();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task ChuckPlanarity_Cleanup()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.ChuckPlanarity_Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task ChuckMoveCommand(ChuckPos param)
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.ChuckMoveCommand(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }


        #endregion

        #endregion

        #region // Temp. Module
        public ITempController GetTempModule()
        {
            ITempController retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetTempModule();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        #endregion

        #region GPCC_Observation
        public async Task GPCC_Observation_CardSettingCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_CardSettingCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_Observation_PogoSettingCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_PogoSettingCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_Observation_PodSettingCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_PodSettingCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void GPCC_Observation_PatternWidthPlusCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GPCC_Observation_PatternWidthPlusCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void GPCC_Observation_PatternWidthMinusCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GPCC_Observation_PatternWidthMinusCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public string GPCC_OP_GetPosition()
        {
            string retVal = null;
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    retVal = Channel.GPCC_GetPosition();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
            return retVal;
        }

        public bool IsCheckCardPodState()
        {
            bool retVal = false;
            try
            {
                if (IsOpened())
                {
                    try
                    {
                        retVal = Channel.IsCheckCardPodState();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void GPCC_Observation_PatternHeightPlusCommand()
        {
            //lock(channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GPCC_Observation_PatternHeightPlusCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void GPCC_Observation_PatternHeightMinusCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GPCC_Observation_PatternHeightMinusCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_Observation_WaferCamExtendCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_WaferCamExtendCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_Observation_WaferCamFoldCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_WaferCamFoldCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_MoveToCenterCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_MoveToCenterCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_ReadyToGetCardCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_ReadyToGetCardCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_Observation_DropZCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_DropZCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_Observation_RaiseZCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_RaiseZCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_Observation_ManualZMoveCommand(EnumProberCam camType, double value)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_ManualZMoveCommand(camType, value);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_Observation_SetMFModelLightsCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_SetMFModelLightsCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_SetMFChildLightsCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_SetMFChildLightsCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void GPCC_Observation_IncreaseLightIntensityCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GPCC_Observation_IncreaseLightIntensityCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public void GPCC_Observation_DecreaseLightIntensityCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GPCC_Observation_DecreaseLightIntensityCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_RegisterPatternCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_RegisterPatternCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_Observation_PogoAlignPointCommand(EnumPogoAlignPoint point)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_PogoAlignPointCommandExcute(point);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_RegisterPosCommand()
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_Observation_RegisterPosCommandExcute();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public GPCardChangeVMData GPCC_Observation_GetGPCCDataCommand()
        {
            GPCardChangeVMData retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetGPCCData();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public async Task GPCC_Observation_MoveToMarkCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_RegisterPatternCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_SetSelectedMarkPosCommand(int selectedmarkposIdx)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_SetSelectedMarkPosCommandExcute(selectedmarkposIdx);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public ObservableCollection<ISequenceBehaviorGroupItem> GPCC_OP_GetDockSequence()
        {
            //lock (channelLockObj)
            {
                ObservableCollection<ISequenceBehaviorGroupItem> retval = null;
                if (IsOpened())
                {
                    try
                    {
                        byte[] serialize = Channel.GPCC_OP_GetDockSequence();
                        ObservableCollection<ISequenceBehaviorGroupItem> deserializeObjcet =
                            ObjectSerialize.DeSerialize(serialize) as ObservableCollection<ISequenceBehaviorGroupItem>;
                        retval = deserializeObjcet;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
                return retval;
            }
        }
        public ObservableCollection<ISequenceBehaviorGroupItem> GPCC_OP_GetUnDockSequence()
        {
            //lock (channelLockObj)
            {
                ObservableCollection<ISequenceBehaviorGroupItem> retval = null;
                if (IsOpened())
                {
                    try
                    {
                        byte[] serialize = Channel.GPCC_OP_GetUnDockSequence();
                        ObservableCollection<ISequenceBehaviorGroupItem> deserializeObjcet =
                            ObjectSerialize.DeSerialize(serialize) as ObservableCollection<ISequenceBehaviorGroupItem>;
                        retval = deserializeObjcet;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
                return retval;
            }
        }

        public int GPCC_OP_GetCurBehaviorIdx()
        {
            //lock (channelLockObj)
            {
                int retval = 0;
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GPCC_OP_GetCurBehaviorIdx();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
                return retval;
            }
        }


        public void GPCC_Docking_PauseState()
        {
            try
            {
                Channel.GPCC_Docking_PauseState();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }
        public void GPCC_Docking_StepUpState()
        {
            try
            {
                Channel.GPCC_Docking_StepUpState();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }
        public void GPCC_Docking_ContinueState()
        {
            try
            {
                Channel.GPCC_Docking_ContinueState();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }
        public void GPCC_Docking_AbortState()
        {
            try
            {
                Channel.GPCC_Docking_AbortState();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }
        public void GPCC_UnDocking_PauseState()
        {
            try
            {
                Channel.GPCC_UnDocking_PauseState();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }
        public void GPCC_UnDocking_StepUpState()
        {
            try
            {
                Channel.GPCC_UnDocking_StepUpState();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }
        public void GPCC_UnDocking_ContinueState()
        {
            try
            {
                Channel.GPCC_UnDocking_ContinueState();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }
        public void GPCC_UnDocking_AbortState()
        {
            try
            {
                Channel.GPCC_UnDocking_AbortState();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_SetSelectLightTypeCommand(EnumLightType selectlight)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_SetSelectLightTypeCommandExcute(selectlight);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_SetLightValueCommand(ushort lightvalue)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        //Channel.GPCC_Observation_SetLightValueCommandExcute(SerializerUtil.ObjectSerialize.Serialize(lightvalue));
                        await Channel.GPCC_Observation_SetLightValueCommandExcute(lightvalue);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_SetZTickValueCommand(int ztickvalue)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_SetZTickValueCommandExcute(ztickvalue);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_SetZDistanceValueCommand(double zdistancevalue)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_SetZDistanceValueCommandExcute(zdistancevalue);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_SetLightTickValueCommand(int lighttickvalue)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_Observation_SetLightTickValueCommandExcute(lighttickvalue);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_Observation_PageSwitchCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.GPCC_Observation_PageSwitchCommandExcute(true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task GPCC_Observation_CleanUpCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.GPCC_Observation_CleanUpCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task GPCC_Observation_FocusingCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.GPCC_Observation_FocusingCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task GPCC_Observation_AlignCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.GPCC_Observation_AlignCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy GPCC_Observation_AlignCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        #endregion

        #region GPCC_OP
        public async Task GPCC_OP_SetContactOffsetZValueCommand(double offsetz)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        //Channel.GPCC_OP_SetContactOffsetzCommandExcute(SerializerUtil.ObjectSerialize.Serialize(offsetz));
                        await Channel.GPCC_OP_SetContactOffsetzCommandExcute(offsetz);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_OP_SetContactOffsetXValueCommand(double offsetx)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        //Channel.GPCC_OP_SetContactOffsetzCommandExcute(SerializerUtil.ObjectSerialize.Serialize(offsetz));
                        await Channel.GPCC_OP_SetContactOffsetxCommandExcute(offsetx);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_OP_SetContactOffsetYValueCommand(double offsety)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        //Channel.GPCC_OP_SetContactOffsetzCommandExcute(SerializerUtil.ObjectSerialize.Serialize(offsetz));
                        await Channel.GPCC_OP_SetContactOffsetyCommandExcute(offsety);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_OP_SetUndockContactOffsetZValueCommand(double offsetz)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_OP_SetUndockContactOffsetzCommandExcute(offsetz);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_OP_SetFocusRangeValueCommand(double rangevalue)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardFuncsRangeCommandExcute(rangevalue);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public async Task<CardChangeSysparamData> GPCC_OP_GetGPCardChangeSysParamData()
        {
            CardChangeSysparamData retval = null;
            byte[] bytes = null;
            object target = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Task task = new Task(() =>
                        {
                            bytes = Channel.GetGPCardChangeSysParamData();
                            bool vaild = SerializeManager.DeserializeFromByte(bytes, out target, typeof(CardChangeSysparamData));
                            if (vaild == true && target != null)
                            {
                                retval = target as CardChangeSysparamData;
                            }
                        });
                        task.Start();
                        await task;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public async Task<CardChangeDevparamData> GPCC_OP_GetGPCardChangeDevParamData()
        {
            CardChangeDevparamData retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = await Channel.GetGPCardChangeDevParamData();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public CardChangeVacuumAndIOStatus GPCC_OP_GetCCVacuumStatus()
        {
            CardChangeVacuumAndIOStatus retval = null;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetCCVacuumStatus();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }
        public void GPCC_OP_CardContactSettingZCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GPCC_OP_CardContactSettingZCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void GPCC_OP_CardUndockContactSettingZCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GPCC_OP_CardUndockContactSettingZCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_OP_CardFocusRangeSettingZCommand(double rangevalue)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_OP_CardFocusRangeSettingZCommandExcute(rangevalue);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
                //}
            }
        }
        public async Task GPCC_OP_SetAlignRetryCountCommand(int retrycount)
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_OP_SetAlignRetryCountCommandExcute(retrycount);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }


        public async Task GPCC_OP_SetDistanceOffsetCommand(double distanceOffset)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetDistanceOffsetCommandExcute(distanceOffset);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }

        public async Task GPCC_OP_SetCardTopFromChuckPlaneSettingCommand(double distance)
        {
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardTopFromChuckPlaneSettingCommandExcute(distance);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public async Task GPCC_OP_SetCardCenterOffsetX1Command(double value)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardCenterOffsetX1CommandExcute(value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public async Task GPCC_OP_SetCardCenterOffsetX2Command(double value)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardCenterOffsetX2CommandExcute(value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public async Task GPCC_OP_SetCardCenterOffsetY1Command(double value)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardCenterOffsetY1CommandExcute(value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public async Task GPCC_OP_SetCardCenterOffsetY2Command(double value)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardCenterOffsetY2CommandExcute(value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public async Task GPCC_OP_SetCardPodCenterXCommand(double value)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardPodCenterXCommandExcute(value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public async Task GPCC_OP_SetCardPodCenterYCommand(double value)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardPodCenterYCommandExcute(value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public async Task GPCC_OP_SetCardLoadZLimitCommand(double value)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardLoadZLimitCommandExcute(value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public async Task GPCC_OP_SetCardLoadZIntervalCommand(double value)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardLoadZIntervalCommandExcute(value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }
        public async Task GPCC_OP_SetCardUnloadZOffsetCommand(double value)
        {
            //lock (channelLockObj)
            //{
            if (IsOpened())
            {
                try
                {
                    await Channel.GPCC_OP_SetCardUnloadZOffsetCommandExcute(value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            //}
        }




        public async Task GPCC_OP_ZifToggleCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_OP_ZifToggleCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_OP_SmallRaiseChuckCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_OP_SmallRaiseChuckCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public async Task GPCC_OP_SmallDropChuckCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        await Channel.GPCC_OP_SmallDropChuckCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        public void GPCC_OP_MoveToZClearedCommand()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.GPCC_OP_MoveToZClearedCommandExcute();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public async Task GPCC_OP_MoveToLoaderCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_MoveToLoaderCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_MoveToCenterCommand()
        {
            try
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);

                if (IsOpened())
                {
                    await Channel.GPCC_OP_MoveToCenterCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public async Task GPCC_OP_MoveToFrontCommand()
        {
            try
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);

                if (IsOpened())
                {
                    await Channel.GPCC_OP_MoveToFrontCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public async Task GPCC_OP_MoveToBackCommand()
        {
            try
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);

                if (IsOpened())
                {
                    await Channel.GPCC_OP_MoveToBackCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task GPCC_OP_RaisePodAfterMoveCardAlignPosCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_RaisePodAfterMoveCardAlignPosCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_RaisePodCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_RaisePodCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_DropPodCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_DropPodCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> GPCC_OP_ForcedDropPodCommand()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    retVal = await Channel.GPCC_OP_ForcedDropPodCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task GPCC_OP_TopPlateSolLockCommand()
        {
            try
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);

                if (IsOpened())
                {
                    await Channel.GPCC_OP_TopPlateSolLockCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public async Task GPCC_OP_TopPlateSolUnLockCommand()
        {
            try
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);

                if (IsOpened())
                {
                    await Channel.GPCC_OP_TopPlateSolUnLockCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public async Task GPCC_OP_PCardPodVacuumOffCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_PCardPodVacuumOffCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_PCardPodVacuumOnCommand()
        {

            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_PCardPodVacuumOnCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_UpPlateTesterCOfftactVacuumOffCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_UpPlateTesterCOfftactVacuumOffCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_UpPlateTesterContactVacuumOnCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_UpPlateTesterContactVacuumOnCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_UpPlateTesterPurgeAirOffCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_UpPlateTesterPurgeAirOffCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_UpPlateTesterPurgeAirOnCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_UpPlateTesterPurgeAirOnCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_PogoVacuumOffCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_PogoVacuumOffCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_PogoVacuumOnCommand()
        {
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_OP_PogoVacuumOnCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_DockCardCommand()
        {
            try
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        await Channel.GPCC_OP_DockCardCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy GPCC_OP_DockCardCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public async Task GPCC_OP_UnDockCardCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 3, 0);
                        await Channel.GPCC_OP_UnDockCardCommandExcute();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy GPCC_OP_UnDockCardCommand timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }
        public async Task GPCC_OP_CardAlignCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.GPCC_OP_CardAlignCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task GPCC_OP_PageSwitchCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.GPCC_Observation_PageSwitchCommandExcute(false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public async Task GPCC_OP_CleanUpCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    await Channel.GPCC_Observation_CleanUpCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        #endregion

        #region TemplateManager

        public EventCodeEnum CheckTemplate(ITemplate module, bool applyload = true, int index = -1)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        string modulename = "PinAligner";

                        retval = Channel.CheckTemplateUsedType(modulename, applyload, index);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public void GPCC_OP_LoaderDoorOpenCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.GPCC_OP_LoaderDoorOpenCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public void GPCC_OP_LoaderDoorCloseCommand()
        {
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    Channel.GPCC_OP_LoaderDoorCloseCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
        }

        public bool GPCC_OP_IsLoaderDoorOpenCommand(bool writelog = true)
        {
            bool isOpen = false;
            try
            {

                if (IsOpened())
                {
                    isOpen = Channel.GPCC_OP_IsLoaderDoorOpenCommandExcute(writelog);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
            return isOpen;
        }
        public bool GPCC_OP_IsLoaderDoorCloseCommand(bool writelog = true)
        {
            bool isClose = false;
            try
            {
                if (IsOpened())
                {
                    isClose = Channel.GPCC_OP_IsLoaderDoorCloseCommandExcute(writelog);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
            return isClose;
        }


        public void GPCC_OP_CardDoorOpenCommand()
        {
            try
            {
                if (IsOpened())
                {
                    Channel.GPCC_OP_CardDoorOpenCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GPCC_OP_CardDoorCloseCommand()
        {
            try
            {
                if (IsOpened())
                {
                    Channel.GPCC_OP_CardDoorCloseCommandExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GPCC_OP_IsLCardExistCommand()
        {
            bool isExist = false;
            try
            {
                if (IsOpened())
                {
                    isExist = Channel.GPCC_OP_IsCardExistExcute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
            return isExist;
        }

        public async Task GPCC_SetWaitForCardPermitEnableCommand(bool value)
        {
            //bool isExist = false;
            try
            {
                if (IsOpened())
                {
                    await Channel.GPCC_SetWaitForCardPermitEnable(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        #region LogTransfer
        public async Task<List<List<string>>> LogTransfer_UpdateLogFile()
        {
            List<List<string>> LogFileList = null;
            Task<List<List<string>>> task;
            try
            {
                //await semaphoreSlim.WaitAsync();

                if (IsOpened())
                {
                    task = new Task<List<List<string>>>(() =>
                    {
                        return Channel.LogTransfer_UpdateLogFile();
                    });
                    task.Start();
                    LogFileList = await task;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
            return LogFileList;
        }

        public async Task<byte[]> LogTransfer_OpenLogFile(string selectedFilePath)
        {
            //await semaphoreSlim.WaitAsync();
            byte[] ret = null;
            try
            {


                if (IsOpened())
                {
                    Task<byte[]> task = new Task<byte[]>(() =>
                    {
                        return Channel.LogTransfer_OpenLogFile(selectedFilePath);
                    });
                    task.Start();
                    ret = await task;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //semaphoreSlim.Release();
            }
            return ret;
        }


        public ObservableDictionary<string, string> GetLogPathInfos()
        {
            ObservableDictionary<string, string> logPathInfos = null;
            try
            {
                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 10);
                        logPathInfos = Channel.GetLogPathInfos();
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy GetLogPathInfos timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return logPathInfos;
        }
        public EventCodeEnum SetLogPathInfos(ObservableDictionary<string, string> infos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    retVal = Channel.SetLogPathInfos(infos);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetmageLoggerParam(ImageLoggerParam imageLoggerParam)
        {
            try
            {
                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 10);
                        Channel.SetmageLoggerParam(imageLoggerParam);
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy SetmageLoggerParam timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ImageDataSet GetImageDataSet(EnumProberModule moduletype, string moduleStartTime, string hashcode)
        {
            ImageDataSet retval = null;

            try
            {
                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;

                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                        retval = Channel.GetImageDataSet(moduletype, moduleStartTime, hashcode);
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"RemoteMediumProxy GetLogPathInfos timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion

        #region LightAdmin
        public void LightAdmin_LoadLUT()
        {
            //lock (channelLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.LightAdmin_LoadLUT();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }
        #endregion

        #region TCPIP
        public EventCodeEnum ReInitializeAndConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //lock (channelLockObj)
                {
                    if (IsOpened())
                    {
                        try
                        {
                            Channel.ReInitializeAndConnect();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
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

        public EventCodeEnum CheckAndConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //lock (channelLockObj)
                {
                    if (IsOpened())
                    {
                        try
                        {
                            Channel.CheckAndConnect();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
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

        public EventCodeEnum FoupAllocated(FoupAllocatedInfo allocatedInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    try
                    {
                        retVal = Channel.FoupAllocated(allocatedInfo);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetOperatorName(string name)
        {
            try
            {
                if (IsOpened())
                {
                    try
                    {
                        Channel.SetOperatorName(name);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
    }
}

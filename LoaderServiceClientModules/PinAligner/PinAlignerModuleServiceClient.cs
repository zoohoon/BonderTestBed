using System;

namespace LoaderServiceClientModules.PinAligner
{
    using Autofac;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase.Communication;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    using LogModule;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.Template;
    using SerializerUtil;
    using ProberInterfaces.PinAlign;
    using ProberInterfaces.Proxies;
    using ProberInterfaces.Align;
    using ProbeCardObject;
    using System.Threading;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class PinAlignerModuleServiceClient : IPinAligner, INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        private IParam _PinAlignDevParam;
        public IParam PinAlignDevParam
        {
            get
            {
                _PinAlignDevParam = PinAlignerParam();
                return _PinAlignDevParam;
            }
            set { _PinAlignDevParam = value; }
        }


        public bool Initialized
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set { }
        }
        public EnumCommunicationState CommunicationState
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    return EnumCommunicationState.CONNECTED;
                }
                else
                {
                    return EnumCommunicationState.UNAVAILABLE;
                }
            }
            set { }
        }

        #region // IStateModule Implementation. Does not need on remote system.
        public ReasonOfError ReasonOfError
        {
            get
            {
                return new ReasonOfError(ModuleEnum.Temperature);
            }
            set => throw new NotImplementedException();
        }
        public CommandSlot CommandSendSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvProcSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvDoneSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandTokenSet RunTokenSet { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandInformation CommandInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ModuleStateBase ModuleState => throw new NotImplementedException();

        public ObservableCollection<TransitionInfo> TransitionInfo => throw new NotImplementedException();

        public EnumModuleForcedState ForcedDone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public void StateTransition(ModuleStateBase state)
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Execute()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Pause()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Resume()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum End()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Abort()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearState()
        {
            throw new NotImplementedException();
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            throw new NotImplementedException();
        }


        public bool IsLotReady(out string msg)
        {
            bool retval = true;
            try
            {
                msg = "";
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"PolishWaferModuleServiceClient() Function error: " + err.Message);
            }

            Initialized = false;
        }

        public EventCodeEnum InitModule()
        {
            Initialized = true;
            return EventCodeEnum.NONE;
        }

        #endregion  

        public TemplateStateCollection Template { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public ITemplateParam LoadTemplateParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISubRoutine SubRoutine { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsManualTriggered { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IPolishWaferCleaningParameter ManualCleaningParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PARecoveryModule RecoveryModules { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PINALIGNSOURCE PinAlignSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime LastAlignDoneTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IPinAlignInfo PinAlignInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PinCoordinate SoakingPinTolerance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISinglePinAlign SinglePinAligner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsRecoveryStarted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ITemplateFileParam SinglePinTemplateParameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TemplateStateCollection SinglePinTemplate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ITemplateFileParam TemplateParameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsChangedSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool PinAlignRunning
        {
            get
            {
                return false;
            }
        }

        public bool UseSoakingSamplePinAlign { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool WaferTransferRunning { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool PIN_ALIGN_Failure { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsManualOPFinished { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public EventCodeEnum ManualOPResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<PinSizeValidateResult> ValidPinTipSize_Original_List { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<PinSizeValidateResult> ValidPinTipSize_OutOfSize_List { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<PinSizeValidateResult> ValidPinTipSize_SizeInRange_List { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Each_Pin_Failure { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //private ITemplateFileParam _TemplateParameter;
        //public ITemplateFileParam TemplateParameter
        //{
        //    get
        //    {
        //        _TemplateParameter = GetTemplateParameter();
        //        return _TemplateParameter;
        //    }
        //    set { _TemplateParameter = value; }
        //}

        //public PinAlignTemplateFile GetTemplateParameter()
        //{
        //    byte[] obj = GetTemplateParam();
        //    object target = null;

        //    PinAlignTemplateFile retval = null;

        //    if (obj != null)
        //    {
        //        var result = SerializeManager.DeserializeFromByte(obj, out target, typeof(PinAlignTemplateFile));
        //        retval = target as PinAlignTemplateFile;
        //    }

        //    return retval;
        //}

        //public byte[] GetTemplateParam()
        //{
        //    byte[] retval = null;

        //    IPinAlignerProxy proxy = LoaderCommunicationManager.GetClientProxy<IPinAlignerProxy>();

        //    if (proxy != null)
        //    {
        //        retval = proxy.GetTemplateParam();
        //    }

        //    return retval;
        //}

        public bool IsServiceAvailable()
        {
            return true;
        }
        public byte[] GetPinAlignerParam()
        {
            byte[] retval = null;

            IPinAlignerProxy proxy = LoaderCommunicationManager.GetProxy<IPinAlignerProxy>();

            if (proxy != null)
            {
                retval = proxy.GetPinAlignerParam();
            }

            return retval;
        }

        public IParam GetPinAlignerIParam()
        {
            return PinAlignerParam();
        }

        public PinAlignDevParameters PinAlignerParam()
        {
            byte[] obj = GetPinAlignerParam();
            object target = null;

            PinAlignDevParameters retval = null;

            if (obj != null)
            {
                var result = SerializeManager.DeserializeFromByte(obj, out target, typeof(PinAlignDevParameters));
                retval = target as PinAlignDevParameters;
            }

            return retval;
        }

        public void SetDevParam(byte[] param)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitDevParameter()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoPolishWaferCleaning(byte[] param)
        {
            throw new NotImplementedException();
        }

        public void SetPinAligerIParam(byte[] param)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<IPinAlignerProxy>()?.SetPinAlignerIParam(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SavePinAlignDevParam()
        {
            throw new NotImplementedException();
        }

        public void GetTransformationPin(PinCoordinate crossPin, PinCoordinate orgPin, double degree, out PinCoordinate updatePin)
        {
            throw new NotImplementedException();
        }

        public double GetDegree(PinCoordinate pivot, PinCoordinate pointOld, PinCoordinate pointNew)
        {
            throw new NotImplementedException();
        }

        public bool IsExistParamFile(string paramPath)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DrawDutOverlay(ICamera cam)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StopDrawDutOverlay(ICamera cam)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StateTranstionSetup()
        {
            throw new NotImplementedException();
        }

        public PinAlignInnerStateEnum GetPAInnerStateEnum()
        {
            throw new NotImplementedException();
        }

        public bool GetPlaneAdjustEnabled()
        {
            throw new NotImplementedException();
        }

        public void SetPinAlignerIParam(byte[] param)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> GetPnpSteps()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ChangeAlignKeySetupControlFlag(PinSetupMode mode, bool flag)
        {
            throw new NotImplementedException();
        }
        public bool GetAlignKeySetupControlFlag(PinSetupMode mode)
        {
            return false;
        }
        public void ConnectValueChangedEventHandler()
        {
            throw new NotImplementedException();
        }

        public string MakeFailDescription()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum CollectElement()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum DoPinAlignProcess(CancellationTokenSource token = null)
        {
            throw new NotImplementedException();
        }

        public bool IsManualOPReady(out string msg)
        {
            throw new NotImplementedException();
        }

        public void CompleteManualOperation(EventCodeEnum retval)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoManualOperation()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoManualOperation(IProbeCommandParameter param = null)
        {
            throw new NotImplementedException();
        }

        public TIP_SIZE_VALIDATION_RESULT Validation_Pin_Tip_Size(BlobResult singlealignpin, ISinglePinAlign singlepinalign, double ratio_x, double ratio_y)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum PinAlignResultServerUpload(List<PinSizeValidateResult> originList, List<PinSizeValidateResult> outList, List<PinSizeValidateResult> inList)
        {
            throw new NotImplementedException();
        }
        public bool CheckSubModulesInTheSkipstate(IProcessingModule subModule)
        {
            throw new NotImplementedException();
        }
    }
}

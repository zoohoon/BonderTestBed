using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoaderServiceClientModules.PolishWaferModule
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
    using LogModule;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.Template;
    using PolishWaferParameters;
    using SerializerUtil;

    public class PolishWaferModuleServiceClient : IPolishWaferModule, INotifyPropertyChanged, IFactoryModule
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

        private bool _LotStartFlag = false;
        public bool LotStartFlag
        {
            get { return _LotStartFlag; }
            set
            {
                if (value != _LotStartFlag)
                {
                    _LotStartFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LotEndFlag = false;
        public bool LotEndFlag
        {
            get { return _LotEndFlag; }
            set
            {
                if (value != _LotEndFlag)
                {
                    _LotEndFlag = value;
                    RaisePropertyChanged();
                }
            }
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


        private IParam _PolishWaferParameter;
        public IParam PolishWaferParameter
        {
            get
            {
                _PolishWaferParameter = PolishWaferParam();
                return _PolishWaferParameter;
            }
            set { _PolishWaferParameter = value; }
        }
        public TemplateStateCollection Template { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ITemplateFileParam TemplateParameter => throw new NotImplementedException();

        public ITemplateParam LoadTemplateParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISubRoutine SubRoutine { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsManualTriggered { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IPolishWaferCleaningParameter ManualCleaningParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool NeedAngleUpdate { get; set; }
        public double RotateAngle { get; set; }
        public double CurrentAngle { get; set; }

        public int RestoredMarkedWaferCountLastPolishWaferCleaning => throw new NotImplementedException();

        public int RestoredMarkedTouchDownCountLastPolishWaferCleaning => throw new NotImplementedException();

        public bool CanIntervalTriggered { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IPolishWaferControlItems PolishWaferControlItems { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PolishWaferProcessingInfo ProcessingInfo { get; set; }
        public PolishWafertCleaningInfo ManualCleaningInfo { get; set; }
        public bool IsManualOPFinished { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public EventCodeEnum ManualOPResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte[] GetPolishWaferParam()
        {
            byte[] retval = null;

            IPolishWaferModuleProxy proxy = LoaderCommunicationManager.GetProxy<IPolishWaferModuleProxy>();

            if (proxy != null)
            {
                retval = proxy.GetPolishWaferParam();
            }

            return retval;
        }

        public IParam GetPolishWaferIParam()
        {
            return PolishWaferParam();
        }

        public PolishWaferParameter PolishWaferParam()
        {
            byte[] obj = GetPolishWaferParam();
            object target = null;

            PolishWaferParameter retval = null;

            if (obj != null)
            {
                var result = SerializeManager.DeserializeFromByte(obj, out target, typeof(PolishWaferParameter));
                retval = target as PolishWaferParameter;
                retval.SetCleaningParamRange();
            }

            return retval;
        }

        public EventCodeEnum DoCentering(IPolishWaferCleaningParameter param)
        {
            return LoaderCommunicationManager
                .GetProxy<IPolishWaferModuleProxy>().DoCentering(param);
        }

        public EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param)
        {
            return LoaderCommunicationManager
                .GetProxy<IPolishWaferModuleProxy>().DoFocusing(param);
        }

        public EventCodeEnum DoCleaning(IPolishWaferCleaningParameter param)
        {
            return LoaderCommunicationManager
                .GetProxy<IPolishWaferModuleProxy>().DoCleaning(param);
        }

        public EventCodeEnum SelectIntervalWafer()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum UnLoadPolishWafer()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadPolishWafer(string definetype)
        {
            throw new NotImplementedException();
        }

        public bool IsReadyPolishWafer()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum PolishWaferValidate(bool isExist)
        {
            throw new NotImplementedException();
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

        public Task<EventCodeEnum> DoManualPolishWaferCleaning(byte[] param)
        {
            throw new NotImplementedException();
        }

        public void SetPolishWaferIParam(byte[] param)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<IPolishWaferModuleProxy>().SetPolishWaferIParam(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool PWIntervalhasLotstart(int index =-1)
        {
            bool ret = false;
            try
            {
                ret = LoaderCommunicationManager.GetProxy<IPolishWaferModuleProxy>(index).PWIntervalhasLotstart();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public IPolishWaferSourceInformation GetPolishWaferInfo(string name)
        {
            throw new NotImplementedException();
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public void InitTriggeredData()
        {
            return;
        }

        public bool IsRequested(ref string RequestedWaferName)
        {
            throw new NotImplementedException();
        }

        public bool IsExecuteOfScheduler()
        {
            throw new NotImplementedException();
        }
        public List<IPolishWaferIntervalParameter> GetPolishWaferIntervalParameters()
        {
            return null;
        }

        public IPolishWaferIntervalParameter GetCurrentIntervalParam()
        {
            return null;
        }

        public IPolishWaferCleaningParameter GetCurrentCleaningParam()
        {
            return null;
        }

        public Task<EventCodeEnum> ManualPolishWaferFocusing(byte[] param)
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
    }
}

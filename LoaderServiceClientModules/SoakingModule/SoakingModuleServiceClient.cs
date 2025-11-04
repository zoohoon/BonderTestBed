using Autofac;
using LoaderBase.Communication;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Param;
using ProberInterfaces.Proxies;
using ProberInterfaces.Soaking;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LoaderServiceClientModules.SoakingModule
{
    public class SoakingModuleServiceClient : ISoakingModule, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool _SoakingCancelFlag;

        public bool SoakingCancelFlag
        {
            get { return _SoakingCancelFlag; }
            set
            {
                if (value != _SoakingCancelFlag)
                {
                    _SoakingCancelFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SoakingTitle;
        public string SoakingTitle
        {
            get { return _SoakingTitle; }
            set
            {
                if (value != _SoakingTitle)
                {
                    _SoakingTitle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SoakingMessage;
        public string SoakingMessage
        {
            get { return _SoakingMessage; }
            set
            {
                if (value != _SoakingMessage)
                {
                    _SoakingMessage = value;
                    RaisePropertyChanged();
                }
            }
        }
        public void SetCancleFlag(bool value, int chuckindex)
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>(chuckindex);
            if (proxy != null)
            {
                proxy.SetCancleFlag(value, chuckindex);
            }
        }

        public string GetSoakingTitle()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                return proxy.GetSoakingTitle();
            }
            return null;
        }

        public string GetSoakingMessage()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                return proxy.GetSoakingMessage();
            }
            return null;
        }
        public bool IsServiceAvailable()
        {
            return true;
        }


        public EventCodeEnum SaveSoakingDeviceFile()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadSoakingDeviceFile()
        {
            throw new NotImplementedException();
        }

        public void SetDevParam(byte[] param)
        {
            throw new NotImplementedException();
        }

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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();

                if (proxy != null)
                {
                    retVal = proxy.ClearState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
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

        public void DeInitModule()
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
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

        public EventCodeEnum LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();

                if (proxy != null)
                {
                    retval = proxy.SaveDevParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum InitDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public double GetSoakingTime()
        {
            throw new NotImplementedException();
        }

        public double GetSoakingTime(EnumSoakingType type)
        {
            throw new NotImplementedException();
        }

        public int GetSoakQueueCount()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();

                if (proxy != null)
                {
                    retval = proxy.SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> SoakingMeasurementTest()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetChangedDeviceName(string curdevname, string changedevname)
        {
            throw new NotImplementedException();
        }




        #endregion


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
        public bool GetLotResumeTriggeredFlag()
        {
            return false;
        }
        public void SetSoakingTime(EnumSoakingType soakingType, int timesec)
        {
            return;
        }
        public IParam SoakingDeviceFile_IParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsPreHeatEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public EventHandler PreHeatEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool SoackingDone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ReasonOfError ReasonOfError { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandSendSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvProcSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvDoneSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandTokenSet RunTokenSet { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandInformation CommandInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ModuleStateBase ModuleState => throw new NotImplementedException();

        public ObservableCollection<TransitionInfo> TransitionInfo => throw new NotImplementedException();

        public EnumModuleForcedState ForcedDone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool MeasurementSoakingCancelFlag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IMetroDialogManager DialogManager => throw new NotImplementedException();

        public ISoakingChillingTimeMng ChillingTimeMngObj { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IStatusSoakingParam StatusSoakingParamIF { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Idle_SoakingFailed_PinAlign { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Idle_SoakingFailed_WaferAlign { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool ManualSoakingStart { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool UsePreviousStatusSoakingDataForRunning { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsUsePolishWafer()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return false;
            }

            return proxy.IsUsePolishWafer();
        }

        public int ChuckAwayToleranceLimitDef => 200;

        public IParam SoakingSysParam_IParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsSoakingDoneAfterWaferLoad { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int GetChuckAwayToleranceLimitX()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return ChuckAwayToleranceLimitDef;
            }

            return proxy.GetChuckAwayToleranceLimitX();
        }

        public int GetChuckAwayToleranceLimitY()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return ChuckAwayToleranceLimitDef;
            }

            return proxy.GetChuckAwayToleranceLimitY();
        }

        public int GetChuckAwayToleranceLimitZ()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return ChuckAwayToleranceLimitDef;
            }

            return proxy.GetChuckAwayToleranceLimitZ();
        }

        public void SetChuckInRangeValue(DateTime time) { }

        public EventCodeEnum CheckCardModuleAndThreeLeg()
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<string> GetPolishWaferNameListForSoaking() { throw new NotImplementedException(); }

        public bool IsStatusSoakingOk()
        {
            throw new NotImplementedException();
        }

        public byte[] GetStatusSoakingConfigParam()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return null;
            }

            return proxy.GetStatusSoakingConfigParam();
        }

        public bool SetStatusSoakingConfigParam(byte[] param, bool save_to_file = true)
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return false;
            }

            return proxy.SetStatusSoakingConfigParam(param, save_to_file);
        }

        public SoakingStateEnum GetStatusSoakingState()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return SoakingStateEnum.UNDEFINED;
            }

            return proxy.GetStatusSoakingState();
        }
        public bool GetShowStatusSoakingSettingPageToggleValue()
        {
            bool retVal = false;
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                retVal = proxy.GetShowStatusSoakingSettingPageToggleValue();
            }

            return retVal;
        }

        public void Check_N_ClearStatusSoaking()
        {            
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                proxy.Check_N_ClearStatusSoaking();
            }
        }

        public void SetShowStatusSoakingSettingPageToggleValue(bool ToggleValue)
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                proxy.SetShowStatusSoakingSettingPageToggleValue(ToggleValue);
            }
        }
        public int GetStatusSoakingTime()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return -1;
            }

            return proxy.GetStatusSoakingTime();
        }

        public EventCodeEnum StartManualSoakingProc()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return proxy.StartManualSoakingProc();
        }

        public EventCodeEnum StopManualSoakingProc()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return proxy.StopManualSoakingProc();
        }

        public (EventCodeEnum, DateTime/*Soaking start*/, SoakingStateEnum/*Soaking Status*/, SoakingStateEnum/*soakingSubState*/, ModuleStateEnum) GetCurrentSoakingInfo()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return (EventCodeEnum.NONE, default, SoakingStateEnum.UNDEFINED, SoakingStateEnum.UNDEFINED, ModuleStateEnum.UNDEFINED);
            }

            return proxy.GetCurrentSoakingInfo();
        }

        public void TraceLastSoakingStateInfo(bool bStart)
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy == null)
            {
                return;
            }

            proxy.TraceLastSoakingStateInfo(bStart);
        }

        public bool GetStatusSoakingInfoUpdateToLoader(ref string SoakingTypeStr, ref string remainingTime, ref string ODVal, ref bool EnableStopSoakBtn)
        {
            throw new NotImplementedException();
        }
        
        public void GetBeforeZupSoak_SettingInfo(out bool UseStatusSoakingFlag, out bool IsBeforeZupSoakingEnableFlag, out int BeforeZupSoakingTime, out double BeforeZupSoakingClearanceZ)
        {
            throw new NotImplementedException();
        }

        public bool GetCurrentStatusSoakingUsingFlag()
        {
            bool retVal = false;
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                retVal = proxy.GetCurrentStatusSoakingUsingFlag();
            }

            return retVal;
        }

        public bool GetBeforeStatusSoakingUsingFlag()
        {
            bool retVal = false;
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                retVal = proxy.GetBeforeStatusSoakingUsingFlag();
            }

            return retVal;
        }

        public EventCodeEnum Get_StatusSoakingPosition(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord, bool use_chuck_focusing = true, bool logWrite = true)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum Get_MaintainSoaking_OD(out double retval) 
        {
            throw new NotImplementedException();
        }

        public void SetBeforeStatusSoakingUsingFlag(bool UseStatusSoakingFlag)
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                proxy.SetBeforeStatusSoakingUsingFlag(UseStatusSoakingFlag);
            }            
        }

        public void ForceChange_PrepareStatus()
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                proxy.ForceChange_PrepareStatus();
            }
        }

        public bool GetPolishWaferForStatusSoaking()
        {
            return false;
        }

        public bool IsStatusSoakingRunning()
        {
            throw new NotImplementedException();
        }

        public void Check_N_KeepPreviousSoakingData()
        {
            throw new NotImplementedException();
        }

        public bool IsEnablePolishWaferSoakingOnCurState()
        {
            bool retVal = false;
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                retVal = proxy.IsEnablePolishWaferSoakingOnCurState();
            }

            return retVal;
        }

        public void Clear_SoakingInfoTxt(bool ForceChageSoakingSubIdle = false)
        {
            throw new NotImplementedException();
        }

        public bool IsCurTempWithinSetSoakingTempRange()
        {
            bool retVal = false;
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                retVal = proxy.IsCurTempWithinSetSoakingTempRange();
            }

            return retVal;
        }

        public bool Get_PrepareStatusSoak_after_DeviceChange()
        {
            bool retVal = false;
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                retVal = proxy.Get_PrepareStatusSoak_after_DeviceChange();
            }

            return retVal;
        }

        public void Set_PrepareStatusSoak_after_DeviceChange(bool PreheatSoak_after_DeviceChange)
        {
            ISoakingModuleProxy proxy = LoaderCommunicationManager.GetProxy<ISoakingModuleProxy>();
            if (proxy != null)
            {
                proxy.Set_PrepareStatusSoak_after_DeviceChange(PreheatSoak_after_DeviceChange);
            }
        }

        public bool IsDeviceLoadpossible(out SoakingStateEnum stateEnum)
        {
            throw new NotImplementedException();
        }

        public void SetStatusSoakingForceTransitionState()
        {
            throw new NotImplementedException();
        }
    }
}
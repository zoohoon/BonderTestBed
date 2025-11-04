using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces.Param;
using ProberInterfaces.Soaking;
using ProberInterfaces.State;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    public enum EnumAutoSoakingType
    {
        DISABLE = 0,
        TO_SAFE_HEIGHT,
        USE_SOAK_CLEARANCE
    }
    public enum EnumSoakPositionType
    {
        CENTER,
        USER_INDEX,
    }
    public enum EnumSoakingType 
    {
        UNDEFINED = 0,
        //PREHAT_SOAK1,
        //PREHAT_SOAK2,
        //FIRSTWAFER_SOAK,
       
        LOTRESUME_SOAK,
        //PINPOSCHANGED_SOAK,
        //POLISHWAFER_SOAK,
        CHUCKAWAY_SOAK,
        TEMPDIFF_SOAK,
        PROBECARDCHANGE_SOAK,
        LOTSTART_SOAK,
        DEVICECHANGE_SOAK,
        AUTO_SOAK,
        EVERYWAFER_SOAK,
        NEEDLE_CLEANING
    }

    [ServiceContract]
    public interface ISoakingModule : IStateModule, IHasDevParameterizable, IHasSysParameterizable
    {
        IParam SoakingDeviceFile_IParam { get; set; }
        IParam SoakingSysParam_IParam { get; set; }
        //SoakingDeviceFile SoakingDevFile { get; }
        EventCodeEnum SaveSoakingDeviceFile();
        EventCodeEnum LoadSoakingDeviceFile();
        bool IsPreHeatEvent { get; set; }
        EventHandler PreHeatEvent { get; set; }
        bool SoakingCancelFlag { get; set; }
        bool Idle_SoakingFailed_PinAlign { get; set; }
        bool Idle_SoakingFailed_WaferAlign { get; set; }
        bool MeasurementSoakingCancelFlag { get; set; }
        bool SoackingDone { get; set; }
        bool ManualSoakingStart { get; set; }
        bool UsePreviousStatusSoakingDataForRunning { get; set; }
        [OperationContract]
        bool IsUsePolishWafer();
        [OperationContract]
        bool Get_PrepareStatusSoak_after_DeviceChange();
        [OperationContract]
        void Set_PrepareStatusSoak_after_DeviceChange(bool PreheatSoak_after_DeviceChange);
        [OperationContract]
        int GetChuckAwayToleranceLimitX();
        [OperationContract]
        int GetChuckAwayToleranceLimitY();
        [OperationContract]
        int GetChuckAwayToleranceLimitZ();
        void SetDevParam(byte[] param);
        [OperationContract]
        void SetCancleFlag(bool value,int chuckindex);
        string SoakingTitle { get; set; }
        string SoakingMessage { get; set; }
        [OperationContract]
        string GetSoakingTitle();
        [OperationContract]
        string GetSoakingMessage();
        [OperationContract]
        bool IsServiceAvailable();
        int GetSoakQueueCount();
        double GetSoakingTime();
        double GetSoakingTime(EnumSoakingType type);
        //Task<EventCodeEnum> SoakingMeasurementTest();
        EventCodeEnum SetChangedDeviceName(string curdevname, string changedevname);
        void SetChuckInRangeValue(DateTime time);
        IMetroDialogManager DialogManager { get; }
        bool GetLotResumeTriggeredFlag();
        void SetSoakingTime(EnumSoakingType soakingType, int timesec);
        ObservableCollection<string> GetPolishWaferNameListForSoaking();
        ISoakingChillingTimeMng ChillingTimeMngObj { get; set; }
        IStatusSoakingParam StatusSoakingParamIF { get; set; }
        EventCodeEnum CheckCardModuleAndThreeLeg();
        bool IsStatusSoakingOk();
        void Clear_SoakingInfoTxt(bool ForceChageSoakingSubIdle = false);
        void SetStatusSoakingForceTransitionState();

        [OperationContract]
        byte[] GetStatusSoakingConfigParam();
        [OperationContract]
        bool SetStatusSoakingConfigParam(byte[] param, bool save_to_file = true);
        [OperationContract]
        SoakingStateEnum GetStatusSoakingState();
        [OperationContract]
        int GetStatusSoakingTime();

        [OperationContract]
        EventCodeEnum StartManualSoakingProc();
        [OperationContract]
        EventCodeEnum StopManualSoakingProc();
        [OperationContract]
        (EventCodeEnum, DateTime/*Soaking start*/, SoakingStateEnum/*Soaking Status*/, SoakingStateEnum/*soakingSubState*/, ModuleStateEnum) GetCurrentSoakingInfo();
        
        [OperationContract]
        void TraceLastSoakingStateInfo(bool bStart);

        bool GetStatusSoakingInfoUpdateToLoader(ref string SoakingTypeStr, ref string remainingTime, ref string ODVal, ref bool EnableStopSoakBtn);
        [OperationContract]
        bool GetShowStatusSoakingSettingPageToggleValue();
        [OperationContract]
        void SetShowStatusSoakingSettingPageToggleValue(bool ToggleValue);

        [OperationContract]
        void Check_N_ClearStatusSoaking();

        void GetBeforeZupSoak_SettingInfo(out bool UseStatusSoakingFlag, out bool IsBeforeZupSoakingEnableFlag, out int BeforeZupSoakingTime, out double BeforeZupSoakingClearanceZ);

        [OperationContract]
        bool GetCurrentStatusSoakingUsingFlag();
        [OperationContract]
        bool GetBeforeStatusSoakingUsingFlag();


        [OperationContract]
        EventCodeEnum Get_StatusSoakingPosition(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord, bool use_chuck_focusing = true, bool logWrite = true);
        
        [OperationContract]
        EventCodeEnum Get_MaintainSoaking_OD(out double retval);

        [OperationContract]
        void SetBeforeStatusSoakingUsingFlag(bool UseStatusSoakingFlag);
        [OperationContract]
        void ForceChange_PrepareStatus();

        bool GetPolishWaferForStatusSoaking();
        bool IsStatusSoakingRunning();
        void Check_N_KeepPreviousSoakingData();

        [OperationContract]
        bool IsEnablePolishWaferSoakingOnCurState();
        [OperationContract]
        bool IsCurTempWithinSetSoakingTempRange();


        bool IsDeviceLoadpossible(out SoakingStateEnum stateEnum);
        bool IsSoakingDoneAfterWaferLoad { get; set; }
    }
    public interface IAutoSoakingModule : IStateModule
    {

    }

    public interface ISoakingSubState : IInnerState
    {
        SoakingStateEnum GetState();
        void StatusSoakingInfoUpdateToLoader(long CurrentElapseSoakingTime, bool no_soak = false, bool force_update = false, bool show_before_remainniingTime = false, bool waitingPWLoading = false, bool Dispaly_NoWafer_SoakingTime = false);
    }
    public interface ISoakingState : IInnerState
    {
        EventCodeEnum SubStateTransition(ISoakingSubState innerState);
        ISoakingModule GetModule();
        SoakingStateEnum GetState();        
    }

    public enum SoakingStateEnum
    {
        IDLE = 0,
        RUNNING,
        AUTOSOAKING_RUNNING,
        EVENTSOAKING_RUNNING,
        EVENTSOAKING_SUSPENDED,
        PAUSE,
        DONE,
        ERROR,
        ABORT,
        SUSPENDED,
        PREPARE,
        RECOVERY,
        MAINTAIN,
        MANUAL,
        SOAKING_RUNNING,
        STATUS_EVENT_SOAK,
        SUSPENDED_FOR_ALIGN,
        SUSPENDED_FOR_WAITING_WAFER_OBJ,
        SUSPENDED_FOR_TEMPERATURE,
        SUSPENDED_FOR_CARDDOCKING,
        SUSPENDED_FOR_OTHER_MODULE,
        SUSPENDED_FOR_MAINTAIN_ABORT,
        UNDEFINED,
    }

    public enum ForceTransitionEnum
    { 
        NEED_TO_STATUS_SUBIDLE = 0,
        NEED_TO_STATUS_SUBIDLE_AND_ZCLEARANCE,
        NEED_TO_STATUS_SUBPAUSE,
        NEED_TO_STATUS_SUBABORT,
        NEED_TO_STATUS_SUBMAINTAINABORT,
        NEED_TO_STATUS_SUBCARDCHANGEABORT,
        NEED_TO_STATUS_RUNNING,
        NOT_NECESSARY
    }

        [DataContract]
    public class SoakingInfo:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private string _SoakingType;
        [DataMember]
        public string SoakingType
        {
            get { return _SoakingType; }
            set
            {
                if (value != _SoakingType)
                {
                    _SoakingType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _RemainTime;
        [DataMember]
        public int RemainTime
        {
            get { return _RemainTime; }
            set
            {
                if (value != _RemainTime)
                {
                    _RemainTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ZClearance;
        [DataMember]
        public double ZClearance
        {
            get { return _ZClearance; }
            set
            {
                if (value != _ZClearance)
                {
                    _ZClearance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ChuckIndex;
        [DataMember]
        public int ChuckIndex
        {
            get { return _ChuckIndex; }
            set
            {
                if (value != _ChuckIndex)
                {
                    _ChuckIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _StopSoakBtnEnable = true;
        [DataMember]
        public bool StopSoakBtnEnable
        {
            get { return _StopSoakBtnEnable; }
            set
            {
                if (value != _StopSoakBtnEnable)
                {
                    _StopSoakBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}

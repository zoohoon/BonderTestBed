using System;
using ProberErrorCode;

namespace ProberInterfaces.Proxies
{
    public interface ISoakingModuleProxy: IProberProxy
    {
        new void InitService();
        bool SoakingCancelFlag { get; set; }
        
        void SetCancleFlag(bool value,int chuckindex);
        string GetSoakingTitle();
        string GetSoakingMessage();
        EventCodeEnum SaveDevParameter();
        EventCodeEnum SaveSysParameter();
        EventCodeEnum ClearState();

        byte[] GetStatusSoakingConfigParam();
        bool SetStatusSoakingConfigParam(byte[] param, bool save_to_file = true);
        SoakingStateEnum GetStatusSoakingState();
        bool GetShowStatusSoakingSettingPageToggleValue();
        void SetShowStatusSoakingSettingPageToggleValue(bool ToggleValue);
        int GetStatusSoakingTime();
        bool IsUsePolishWafer();
        int GetChuckAwayToleranceLimitX();
        int GetChuckAwayToleranceLimitY();
        int GetChuckAwayToleranceLimitZ();

        void Check_N_ClearStatusSoaking();
        EventCodeEnum StartManualSoakingProc(); //manual soaking start
        EventCodeEnum StopManualSoakingProc(); //manual soaking stop
        (EventCodeEnum, DateTime/*Soaking start*/, SoakingStateEnum/*Soaking Status*/, SoakingStateEnum/*soakingSubState*/, ModuleStateEnum) GetCurrentSoakingInfo();

        void TraceLastSoakingStateInfo(bool bStart);
        bool GetBeforeStatusSoakingUsingFlag();
        void SetBeforeStatusSoakingUsingFlag(bool UseStatusSoakingFlag);
        bool GetCurrentStatusSoakingUsingFlag();
        void ForceChange_PrepareStatus();
        bool IsEnablePolishWaferSoakingOnCurState();
        bool IsCurTempWithinSetSoakingTempRange();
        bool Get_PrepareStatusSoak_after_DeviceChange();
        void Set_PrepareStatusSoak_after_DeviceChange(bool PreheatSoak_after_DeviceChange);
    }
}

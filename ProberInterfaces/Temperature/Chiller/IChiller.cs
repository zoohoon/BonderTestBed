using ProberErrorCode;
using ProberInterfaces.Enum;
using System;
using System.ComponentModel;

namespace ProberInterfaces.Temperature.Chiller
{
    public enum ChillerProcessType
    {
        IDLE,
        RUNNING,
        DONE,
        ERROR
    }
    public enum ChillerPumpingSpeedType
    {
        NORMAL,
        SLOW,
        FAST
    }

    public interface IChillerModule : IFactoryModule, INotifyPropertyChanged, IModule
    {
        IChillerParameter ChillerParam { get; set; }
        //IChillerComm ChillerComm { get; }
        IChillerInfo ChillerInfo { get; set; }
        void InitParam(IChillerParameter chillerparam, bool setremotechange = false);
        byte[] GetChillerParam(int stageindex = -1);
        EventCodeEnum InitConnect();
        EventCodeEnum Start(bool bInit = false);
        EventCodeEnum DisConnect(bool bNormalEnd = false);
        EventCodeEnum Activate();
        //EventCodeEnum Inactivate();
        EventCodeEnum Inactivate(bool isEMO = false);

        void SetCircuationActive(bool bValue, byte SubModuleIndex, int timeoutMsec = 600);
        bool CanRunningLot();
        EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType);
        void SetPumpSpeed(ChillerPumpingSpeedType pumpSpeedType);
        void SetPumpSpeed(double pumpSpeed);
        EventCodeEnum SetSlowChilling(double targetTemp, TempValueType targetTempValueType);
        EventCodeEnum SetNormalChilling(double targetTemp, TempValueType targetTempValueType, bool forcedSetValue = false);
        EventCodeEnum SetFastChilling(double targetTemp, TempValueType targetTempValueType);
        void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior);
        double GetChillerTempoffset(double targetTemp);

        ICommunicationMeans GetCommunicationObj();
        object GetCommLockObj();
        EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false);
        EnumChillerModuleMode GetChillerMode();
        bool IsMatchedTargetTemp(double targetTemp);
        double ConvertTargetTempApplyOffset(double targetTemp);

        #region ==> Get Data from Chiller
        EnumCommunicationState GetCommState();
        EnumCommunicationState CommunicationState { get; }
        double GetSetTempValue();
        double GetReturnTempVal();


        double GetProcessTempVal();
        double GetInternalTempValue();
        double GetExtMoveVal();

        double GetMinSetTemp();
        double GetMaxSetTemp();
        int GetCurrentPower();
        int GetPumpPressureVal();
        int GetSetTempPumpSpeed();
        int GetPumpSpeed();
        int GetErrorReport();
        int GetWarningMessage();
        int GetStatusOfThermostat();
        bool IsAutoPID();
        bool IsTempControlProcessMode();
        bool IsTempControlActive();
        (bool, bool) GetProcTempActValSetMode();
        int GetSerialNumLow();
        int GetSerialNumHigh();
        int GetSerialNumber();
        bool IsCirculationActive();
        (bool, bool) IsOperatingLock();
        double GetUpperAlramInternalLimit();
        double GetLowerAlramInternalLimit();
        double GetUpperAlramProcessLimit();
        double GetLowerAlramProcessLimit();
        #endregion

        double StageSetSV { get; set; }
        double ChillerSVSetValue { get; set; }

        bool UnLockStageSvBtn { get; set; }

        bool IsConnected { get; }
        bool ChillerMaintenanceFlag { get; set; }
    }

    public interface IPID
    {
        double SVPb { get; set; }
        double SViT { get; set; }
        double SVdE { get; set; }
    }

    public interface IChillerComm : IDisposable, IFactoryModule
    {
        EventCodeEnum InitModule();
        EventCodeEnum Connect(string address, int port);
        void DisConnect();
        EnumCommunicationState GetCommState(byte SubModuleIndex);
        EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, byte SubModuleIndex, bool forcedSetValue = false);
        void SetTempActiveMode(bool bValue, byte SubModuleIndex);
        void SetSetTempPumpSpeed(int iValue, byte SubModuleIndex);
        void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavio, byte SubModuleIndex);
        void SetCircuationActive(bool bValue, byte SubModuleIndex);

        ICommunicationMeans GetCommunicationObj();

        object GetCommLockObj();
        EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false);

        #region ==> Get Data from Chiller
        double GetSetTempValue(byte SubModuleIndex);
        double GetReturnTempVal(byte SubModuleIndex);
        
        
        double GetProcessTempVal(byte SubModuleIndex);

        double GetInternalTempValue(byte SubModuleIndex);
        double GetExtMoveVal(byte SubModuleIndex);
        
        double GetMinSetTemp(byte SubModuleIndex);
        double GetMaxSetTemp(byte SubModuleIndex);
        int GetCurrentPower(byte SubModuleIndex);
        int GetPumpPressureVal(byte SubModuleIndex);
        int GetSetTempPumpSpeed(byte SubModuleIndex);
        int GetPumpSpeed(byte SubModuleIndex);
        int GetErrorReport(byte SubModuleIndex);
        int GetWarningMessage(byte SubModuleIndex);
        int GetStatusOfThermostat(byte SubModuleIndex);
        bool IsAutoPID(byte SubModuleIndex);
        bool IsTempControlProcessMode(byte SubModuleIndex);
        bool IsTempControlActive(byte SubModuleIndex);
        bool IsCirculationActive(byte SubModuleIndex);
        (bool, bool) GetProcTempActValSetMode(byte SubModuleIndex);
        int GetSerialNumLow(byte SubModuleIndex);
        int GetSerialNumHigh(byte SubModuleIndex);
        int GetSerialNumber(byte SubModuleIndex);
        (bool, bool) IsOperatingLock(byte SubModuleIndex);
        double GetUpperAlramInternalLimit(byte SubModuleIndex);
        double GetLowerAlramInternalLimit(byte SubModuleIndex);
        double GetUpperAlramProcessLimit(byte SubModuleIndex);
        double GetLowerAlramProcessLimit(byte SubModuleIndex);
        #endregion
    }

    public interface IChillerEmulable
    {
        IChillerEmulParam ChillerEmulParam { get; set; }
    }

    public interface IChillerEmulParam
    {
        int vSP { get; set; }
        short vTI { get; set; }
        short vTR { get; set; }
        short vpP { get; set; }
        short vPow { get; set; }
        short vError { get; set; }
        short vWarn { get; set; }
        short vTE { get; set; }
        short vStatus1 { get; set; }
        bool vAutoPID { get; set; }
        bool vTmpMode { get; set; }
        bool vTmpActive { get; set; }
        bool vCETM { get; set; }
        string vSNRL { get; set; }
        string vSNRH { get; set; }
        bool vCircActive { get; set; }
        bool vKeyLock { get; set; }
        int vnP { get; set; }
        int vMinSP { get; set; }
        int vMaxSP { get; set; }
        int vExtmode { get; set; }
        int vTIAlarmHi { get; set; }
        int vTIAlarmLo { get; set; }
        int vTEAlarmHi { get; set; }
        int vTEAlarmLo { get; set; }
        int vnPSet { get; set; }
    }
}

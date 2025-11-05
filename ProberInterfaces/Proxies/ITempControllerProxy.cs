namespace ProberInterfaces
{
    using ProberErrorCode;
    using System.Collections.Generic;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Proxies;
    using System.ServiceModel;

    [ServiceKnownType(typeof(TemperatureChangeSource))]
    public interface ITempControllerProxy : IFactoryModule, IProberProxy
    {
        new void InitService();
        int GetHeaterOffsetCount();
        double GetTemperatureOffset(double value);
        Dictionary<double, double> GetHeaterOffset();
        void SetSV(TemperatureChangeSource source, double changeSetTemp, bool willYouSaveSetValue = true, bool forcedSetValue = false);
        void SetSVOnlyTC(double setTemp);
        void SetAmbientTemp();
        void ClearHeaterOffset();
        void AddHeaterOffset(double reftemp, double measuredtemp);
        void SaveOffsetParameter();
        void Dispose();
        new bool IsServiceAvailable();
        void SetLoggingInterval(long seconds);
        EnumTemperatureState GetTempControllerState();
        double GetDeviaitionValue();
        double GetEmergencyAbortTempTolereance();
        TempPauseTypeEnum GetTempPauseType();
        void SetDeviaitionValue(double deviation, bool emergencyparam);
        void SetTempPauseType(TempPauseTypeEnum pausetype);
        bool CheckSetDeviationParamLimit(double deviation, bool emergencyparam);
        void SetTemperature();
        double GetTemperature();
        double GetCurDewPointValue();
        double GetMV();
        double GetDewPointTolerance();
        double GetSetTemp();
        bool GetHeaterOutPutState();
        void SetTempMonitorInfo(TempMonitoringInfo param);
        TempMonitoringInfo GetTempMonitorInfo();
        byte[] GetParamByte();
        EventCodeEnum SetParamByte(byte[] devparam);
        void SetEndTempEmergencyErrorCommand();
        bool GetIsOccurTimeOutError();
        void ClearTimeOutError();
        double GetMonitoringMVTimeInSec();
        void SetMonitoringMVTimeInSec(double value);
        double GetCCActivatableTemp();
        bool IsCurTempWithinSetTempRange();
        bool IsCurTempUpperThanSetTemp(double setTemp, double margin);
        double GetDevSetTemp();
        EventCodeEnum SetDevSetTemp(double setTemp);
        bool GetApplySVChangesBasedOnDeviceValue();
        void SetActivatedState(bool forced = false);
        TempEventInfo GetPreviousTempInfoInHistory();
        TempEventInfo GetCurrentTempInfoInHistory();

        TempEventInfo GetPreviousSourceTempInfoInHistory();
    }
}

using ProberErrorCode;
using ProberInterfaces.Temperature.TempManager;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ProberInterfaces.Temperature
{
    public enum TempPauseTypeEnum
    {
        NONE,
        ZDOWN_ABORT,
        EMERGENCY_ABORT
    }

    [ServiceContract]
    public interface ITempController : ICommunicationable, IStateModule, IHasSysParameterizable, IHasDevParameterizable
    {
        IParam TempSafetySysParam { get; }
        IParam TempSafetyDevParam { get; }
        IParam TempControllerDevParam { get; }
        XyDataSeries<DateTime, double> dataSeries_CurTemp { get; set; }
        XyDataSeries<DateTime, double> dataSeries_SetTemp { get; set; }
        TemperatureInfo TempInfo { get; set; }
        //bool IsUsingDryAir();
        //bool IsUsingChiller();
        ITempManager TempManager { get; set; }
        //IChiller ChillerModule { get; set; }
        //IDewPoint DewPointModule { get; set; }
        //IDryAir DryAirModule { get; set; }
        [OperationContract]
        double GetDevSetTemp();
        [OperationContract]
        EventCodeEnum SetDevSetTemp(double setTemp);
        [OperationContract]
        EnumTemperatureState GetTempControllerState();
        [OperationContract]
        double GetSetTempWithOverHeatTemp();  //*
        [OperationContract]
        void SetTemperatureFromDevParamSetTemp();//*
        [OperationContract]
        bool IsCurTempWithinSetTempRange(bool checkDevTemp = true);//*
        [OperationContract]
        bool IsCurTempWithinSetTempRangeDeviation(double deviationval);
        [OperationContract]
        bool IsCurTempUpperThanSetTemp(double setTemp, double mergin);
        [OperationContract]
        /// <summary>
        /// Argument로 가져온 온도 값의 Offset값을 가져옵니다.
        /// </summary>
        /// <param name="temperature"></param>
        /// <returns></returns>
        double GetTemperatureOffset(double temperature);//*
        [OperationContract]
        int GetHeaterOffsetCount();
        [OperationContract]
        Dictionary<double, double> GetHeaterOffsets();
        [OperationContract]
        bool GetCheckingTCTempTable();
        [OperationContract]
        void ClearHeaterOffset();
        [OperationContract]
        void AddHeaterOffset(double reftemp, double measuredtemp);
        [OperationContract]
        void SetTemperature();
        [OperationContract]
        double GetTemperature();
        [OperationContract]
        void SetVacuum(bool ison);
        [OperationContract]
        void EnableRemoteUpdate();
        [OperationContract]
        void DisableRemoteUpdate();
        [OperationContract]
        bool IsServiceAvailable();
        [OperationContract]
        void SetLoggingInterval(long seconds);

        [OperationContract]
        double GetDeviaitionValue();
        [OperationContract]
        double GetEmergencyAbortTempTolereance();
        [OperationContract]
        TempPauseTypeEnum GetTempPauseType();
        [OperationContract]
        void SetDeviaitionValue(double deviation, bool emergencyparam = false);
        [OperationContract]
        void SetTempPauseType(TempPauseTypeEnum pausetype);
        [OperationContract]
        bool CheckSetDeviationParamLimit(double deviation, bool emergencyparam = false);
        [OperationContract]
        double GetCurDewPointValue();
        [OperationContract]
        double GetMV();
        /// <summary>
        /// Target 온도를 설정하고 OverHeating 데이터를 설정 합니다.       
        /// </summary>
        /// <param name="changeSetTemp"> Set할 온도 값 입니다. </param>
        /// <param name="willYouSaveSetValue"> Set할 온도를 Devece파일에 저장 유무에 관한 파라미터입니다. </param>
        [OperationContract(IsOneWay = true)]
        void SetSV(TemperatureChangeSource source, double changeSetTemp, bool willYouSaveSetValue = true, bool forcedSetValue = false, double overHeating = 0.0, double Hysteresis = 0.0);
        [OperationContract]
        void SetAmbientTemp([CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0);
        [OperationContract]
        double GetDewPointTolerance();
        [OperationContract]
        double GetSetTemp();
        [OperationContract]
        bool GetHeaterOutPutState();
        [OperationContract(IsOneWay = true)]
        void SetTempMonitorInfo(TempMonitoringInfo param);
        [OperationContract]
        TempMonitoringInfo GetTempMonitorInfo();
        [OperationContract]
        byte[] GetParamByte();
        [OperationContract]
        EventCodeEnum SetParamByte(byte[] devparam);
        [OperationContract]
        void SaveOffsetParameter();
        [OperationContract]
        void SetEndTempEmergencyErrorCommand();

        [OperationContract]
        double GetMonitoringMVTimeInSec();
        [OperationContract]
        void SetMonitoringMVTimeInSec(double value);
        [OperationContract]
        bool GetIsOccurTimeOutError();
        [OperationContract]
        void ClearTimeOutError();
        [OperationContract]
        double GetCCActivatableTemp();
        [OperationContract]
        EventCodeEnum CheckSVWithDevice();
        [OperationContract]
        bool CheckIfTempIsIncluded();
        [OperationContract]
        bool ControlTopPurgeAir();
        bool IsPurgeAirBackUpValue { get; set; }
        [OperationContract]
        bool GetApplySVChangesBasedOnDeviceValue();
        bool IsUsingChillerState();
        long GetLimitRunTimeSeconds();
        [OperationContract(IsOneWay = true)]
        void SetActivatedState(bool forced = false);
        void RestorePrevSetTemp();
        TempEventInfo GetPreviousTempInfoInHistory();
        TempEventInfo GetCurrentTempInfoInHistory();
        TempEventInfo GetPreviousSourceTempInfoInHistory();
    }

    public interface ITempControllerDevParam
    {
        Element<double> SetTemp { get; set; }
    }

    public interface ITempSafetyDevParam
    {
        Element<int> WaferLoadDelay { get; set; }
        Element<int> WaferUnLoadDelay { get; set; }
    }

    public interface ITempSafetySysParam
    {
        Element<int> WaferLoad_SoakTempUpper { get; set; }
        Element<int> WaferLoad_SoakTempLower { get; set; }
        Element<int> WaferLoad_HighTempSoakTime { get; set; }
        Element<int> WaferLoad_LowTempSoakTime { get; set; }
        Element<int> WaferUnLoad_SoakTempUpper { get; set; }
        Element<int> WaferUnLoad_SoakTempLower { get; set; }
        Element<int> WaferUnLoad_HighTempSoakTime { get; set; }
        Element<int> WaferUnLoad_LowTempSoakTime { get; set; }
    }

    public enum TempValueType
    {
        CELSIUS,
        TEMPCONTROLLER,
        HUBER
    }

    [DataContract]
    public class TempMonitoringInfo
    {
        private double _TempMonitorRange;
        [DataMember]
        public double TempMonitorRange
        {
            get { return _TempMonitorRange; }
            set { _TempMonitorRange = value; }
        }

        private double _WaitMonitorTimeSec;
        [DataMember]
        public double WaitMonitorTimeSec
        {
            get { return _WaitMonitorTimeSec; }
            set { _WaitMonitorTimeSec = value; }
        }

        private bool _MonitoringEnable;
        [DataMember]
        public bool MonitoringEnable
        {
            get { return _MonitoringEnable; }
            set { _MonitoringEnable = value; }
        }


    }

}

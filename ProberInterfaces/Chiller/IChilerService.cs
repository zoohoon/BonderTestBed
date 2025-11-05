using ProberErrorCode;
using ProberInterfaces.Enum;
using ProberInterfaces.Temperature;
using System.ServiceModel;

namespace ProberInterfaces
{
    [ServiceContract]
    public interface IChillerService
    {

        [OperationContract]
        /// <summary>
        /// Loader에 연결을 해제합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum Disconnect(int stageindex = -1);


        [OperationContract]
        bool IsServiceAvailable();
        [OperationContract]
        void InitService();
        [OperationContract]
        EventCodeEnum Connect(string address, int port, int stageindex = -1);
        [OperationContract]
        EnumCommunicationState GetCommState(int stageindex = -1);
        [OperationContract]
        byte[] GetChillerParam(int stageindex = -1);
        [OperationContract]
        EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false);
        [OperationContract]
        EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, int stageindex = -1, bool forcedSetValue = false);
        [OperationContract(IsOneWay = true)]
        void SetTempActiveMode(bool bValue, int stageindex = -1);
        [OperationContract(IsOneWay = true)]
        void SetSetTempPumpSpeed(int iValue, int stageindex = -1);
        void SetCircuationActive(bool bValue, int stageindex = -1, int chillerindex = -1);
        void Set_OperaionLockValue(int chillerindex = -1, bool lockValue = false);
        #region ==> Get Data from Chiller
        [OperationContract]
        double GetSetTempValue(int stageindex = -1);
        [OperationContract]
        double GetInternalTempValue(int stageindex = -1);
        [OperationContract]
        double GetReturnTempVal(int stageindex = -1);
        [OperationContract]
        int GetPumpPressureVal(int stageindex = -1);
        [OperationContract]
        int GetCurrentPower(int stageindex = -1);
        [OperationContract]
        int GetErrorReport(int stageindex = -1);
        [OperationContract]
        int GetWarningMessage(int stageindex = -1);
        [OperationContract]
        double GetProcessTempVal(int stageindex = -1);
        [OperationContract]
        double GetExtMoveVal(int stageindex = -1);
        [OperationContract]
        int GetStatusOfThermostat(int stageindex = -1);
        [OperationContract]
        bool IsAutoPID(int stageindex = -1);
        [OperationContract]
        bool IsTempControlProcessMode(int stageindex = -1);
        [OperationContract]
        bool IsTempControlActive(int stageindex = -1);
        [OperationContract]
        (bool, bool) GetProcTempActValSetMode(int stageindex = -1);
        [OperationContract]
        int GetSerialNumLow(int stageindex = -1);
        [OperationContract]
        int GetSerialNumHigh(int stageindex = -1);
        [OperationContract]
        int GetSerialNumber(int stageindex = -1);
        [OperationContract]
        bool IsCirculationActive(int stageindex = -1, int chillerindex = -1);
        [OperationContract]
        (bool, bool) IsOperatingLock(int stageindex = -1);
        [OperationContract]
        int GetPumpSpeed(int stageindex = -1);
        [OperationContract]
        double GetMinSetTemp(int stageindex = -1);
        [OperationContract]
        double GetMaxSetTemp(int stageindex = -1);
        [OperationContract]
        int GetSetTempPumpSpeed(int stageindex = -1);
        [OperationContract]
        double GetUpperAlramInternalLimit(int stageindex = -1);
        [OperationContract]
        double GetLowerAlramInternalLimit(int stageindex = -1);
        [OperationContract]
        double GetUpperAlramProcessLimit(int stageindex = -1);
        [OperationContract]
        double GetLowerAlramProcessLimit(int stageindex = -1);
        [OperationContract]
        bool GetChillerAbortActiveState(int stageindex = -1);
        #endregion
    }

    public interface IChilerServiceCallback
    { }

}

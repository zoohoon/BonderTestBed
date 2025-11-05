using System;
using System.ServiceModel;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.EnvControl.Parameter;
    using ProberInterfaces.Temperature.Chiller;
    using ProberInterfaces.Temperature.DewPoint;
    using ProberInterfaces.Temperature.DryAir;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    public enum ModuleEnableType
    {
        [EnumMember]
        DISABLE = 0x0000,
        [EnumMember]
        ENABLE = 0x0001
    }
    public enum EnumEnvControlModuleMode
    {
        [EnumMember]
        UNDIFIND = -1,
        [EnumMember]
        LOCAL = UNDIFIND+1,
        [EnumMember]
        REMOTE,
        [EnumMember]
        EMUL,
        [EnumMember]
        NONE
    }

    public enum EnumValveModuleType
    {
        [EnumMember]
        INVALID = -1,
        [EnumMember]
        UNDEFINED = 0,
        [EnumMember]
        LOADER,    // PLC
        [EnumMember]
        MODBUS,  //Chiller (솔라딘)
        [EnumMember]
        REMOTE,     // Cell
        [EnumMember]
        NONE,// 사용안함
        [EnumMember]
        NA // 사용안함
    }

    public interface IEnvControlManager : IHasSysParameterizable, IEnvControlCore, IDisposable, ILoaderFactoryModule, IModule
    {
        IParam EnvSysParam { get; set; }
        IChillerManager ChillerManager { get; }
        IDryAirManager DryAirManager { get; }
        IDewPointManager DewPointManager { get; }
        IValveManager ValveManager { get; }

        EventCodeEnum InitConnect();
        EventCodeEnum DisConnect(int chuckID = -1);
        EnumEnvControlModuleMode GetEnvcontrolMode();
        IChillerModule GetChillerModule();
        IDryAirModule GetDryAirModule();
        IDewPointModule GetDewPointModule();
        bool CanInactivateChiller(ObservableCollection<int> stageIdxs);
        EventCodeEnum SetEMGSTOP();
        bool GetIsExcute();

        //==== Valve =======
        EventCodeEnum SetValveState(bool enableFlag, EnumValveType valveType, int stageIndex = -1);
        bool GetValveState(EnumValveType valveType, int stageIndex = -1);
        IValveSysParameter GetValveParam();
        //==================

        //==== Dry Air =====
        byte[] GetDryAirParam(int stageindex = -1);
        EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1);
        int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1);
        //==================

        //======= FFU ======
        void RaiseFFUAlarm(string alarmMessage);
        //==================
    }

    [ServiceContract]
    public interface IEnvControlCore : IFactoryModule, IModule
    {
        [OperationContract]
        bool IsUsingDryAir(int stageindex = -1);
        [OperationContract]
        bool IsUsingChiller(int stageindex = -1);
        [OperationContract]
        IEnvControlServiceCallback GetEnvControlClient(int stageindex = -1);
    }

    [ServiceContract]
    public interface IEnvController : IEnvControlCore
    {
        [OperationContract]
        EventCodeEnum InitConnect();
        [OperationContract]
        EventCodeEnum DisConnect(int index = -1);

        bool GetIsExcute();

        #region Valve
        [OperationContract]
        EventCodeEnum SetValveState(bool state, EnumValveType valveType, int stageIndex = -1);
        [OperationContract]
        bool GetValveState(EnumValveType valveType, int stageIndex = -1);
        #endregion

        #region FFU
        [OperationContract]
        void RaiseFFUAlarm(string alarmmessage);
        #endregion

        #region DryAir
        [OperationContract]
        byte[] GetDryAirParam(int stageindex = -1);
        [OperationContract]
        EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1);
        [OperationContract]
        int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1);
        #endregion
    }

    [ServiceContract(CallbackContract = typeof(IEnvControlServiceCallback))]
    public interface IEnvControlService : IEnvController
    {
        [OperationContract]
        void InitService(int stageIndex = 0);
        [OperationContract]
        bool IsServiceAvailable();
    }

    public interface IEnvControlServiceCallback
    {
        [OperationContract]
        EventCodeEnum DisConnect(int index = -1);
        [OperationContract]
        bool IsAlive();
        [OperationContract]
        double GetDewPointVal();
        [OperationContract]
        double GetChillerTargetTemp();
        [OperationContract]
        double GetTempTargetTemp();
        [OperationContract]
        /// Chiller를 사용하는지 판단하는 Flag => Loader 에서 이 Flag 를 보고 flase 라면 valve 조작및 칠러 온도 변경할 수 있다.
        bool GetChillerActiveState();
        [OperationContract(IsOneWay = false)]
        RemoteStageColdSetupData GetRemoteColdData();
        [OperationContract(IsOneWay = true)]
        void SetRemoteColdData(RemoteStageColdSetupData remotedata);
        [OperationContract(IsOneWay = true)]
        void SetChillerData(byte[] chillerparam, bool setremotechange = false);
        [OperationContract(IsOneWay = true)]
        void SetChillerAbortMode(bool flag);
    }

    [DataContract]
    public class RemoteStageColdSetupData
    {
        private double _DewPointTolerance;
        [DataMember]
        public double DewPointTolerance
        {
            get { return _DewPointTolerance; }
            set { _DewPointTolerance = value; }
        }

        private double _DryAirActivatableHighTemp;
        [DataMember]
        public double DryAirActivatableHighTemp
        {
            get { return _DryAirActivatableHighTemp; }
            set { _DryAirActivatableHighTemp = value; }
        }

        private double _DewPointTimeOut;
        [DataMember]
        public double DewPointTimeOut
        {
            get { return _DewPointTimeOut; }
            set { _DewPointTimeOut = value; }
        }

        public RemoteStageColdSetupData()
        {

        }
        public RemoteStageColdSetupData(double dewpointTolerance, double dryairActivateHighTemp, double dewpointTimeOut)
        {
            DewPointTolerance = dewpointTolerance;
            DryAirActivatableHighTemp = dryairActivateHighTemp;
            DewPointTimeOut = dewpointTimeOut;
        }
    }
}

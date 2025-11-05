using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using Autofac.Core;
using CameraChannelManager;
using ElmoManager;
using FileSystem;
using IOManagerModule;
using LightManager;
using LogModule;
using MetroDialogInterfaces;
using MetroDialogModule;
using ParameterManager;
using ProbeMotion;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Data;
using ProberInterfaces.Device;
using ProberInterfaces.Focus;
using ProberInterfaces.LightJog;
using ProberInterfaces.Monitoring;
using ProberInterfaces.NeedleClean;
using ProberInterfaces.Temperature;
using ProberVision;
using SorterSystem.TempCtrl;
using SystemExceptions;
using ViewModelModule;
using FocusManager = FocusingManager.FocusManager;
using IModule = ProberInterfaces.IModule;

namespace SorterSystem
{
    public class SorterStageSupervisor : IStageSupervisor, IModule
    {
        int IStageSupervisor.AbsoluteIndex => throw new NotImplementedException();

        bool IStageSupervisor.StageMoveFlag_Display { get { return false; } }

        double IStageSupervisor.PinZClearance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        double IStageSupervisor.PinMaxRegRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        double IStageSupervisor.PinMinRegRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        double IStageSupervisor.WaferRegRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        double IStageSupervisor.WaferMaxThickness { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IWaferObject IStageSupervisor.WaferObject { get => null; set => throw new NotImplementedException(); }
        IMarkObject IStageSupervisor.MarkObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IStageMove IStageSupervisor.StageModuleState => throw new NotImplementedException();

        IProbeCard IStageSupervisor.ProbeCardInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ProbingInfo IStageSupervisor.ProbingProcessStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        ICommand IStageSupervisor.ClickToMoveLButtonDownCommand => throw new NotImplementedException();

        private double _MoveTargetPosX = 0D;
        double IStageSupervisor.MoveTargetPosX { get => _MoveTargetPosX; set => _MoveTargetPosX = value; }

        private double _MoveTargetPosY = 0D;
        double IStageSupervisor.MoveTargetPosY { get => _MoveTargetPosY; set => _MoveTargetPosY=value; }
        double IStageSupervisor.UserCoordXPos { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        double IStageSupervisor.UserCoordYPos { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        double IStageSupervisor.UserCoordZPos { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        double IStageSupervisor.UserWaferIndexX { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        double IStageSupervisor.UserWaferIndexY { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        LightJogViewModel IStageSupervisor.PnpLightJog { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IHexagonJogViewModel IStageSupervisor.PnpMotionJog { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        double IStageSupervisor.WaferINCH6Size => throw new NotImplementedException();

        double IStageSupervisor.WaferINCH8Size => throw new NotImplementedException();

        double IStageSupervisor.WaferINCH12Size => throw new NotImplementedException();

        StageStateEnum IStageSupervisor.StageMoveState => throw new NotImplementedException();

        bool IStageSupervisor.IsModeChanging { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICylinderManager IStageSupervisor.IStageCylinderManager { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IStageMoveLockStatus IStageSupervisor.IStageMoveLockStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        INeedleCleanObject IStageSupervisor.NCObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ITouchSensorObject IStageSupervisor.TouchSensorObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        GPCellModeEnum IStageSupervisor.StageMode => throw new NotImplementedException();

        IStageMoveLockParameter IStageSupervisor.IStageMoveLockParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IStageSupervisor.IsRecoveryMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        StreamingModeEnum IStageSupervisor.StreamingMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IStageSupervisor.IsRecipeDownloadEnable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        EventHandler IStageSupervisor.MachineInitEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        EventHandler IStageSupervisor.MachineInitEndEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        bool IModule.Initialized => true;

        event EventHandler IStageSupervisor.ChangedWaferObjectEvent
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler IStageSupervisor.ChangedProbeCardObjectEvent
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event System.ComponentModel.PropertyChangedEventHandler System.ComponentModel.INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        void IStageSupervisor.BindDataGatewayService(string uri)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.BindDelegateEventService(string uri)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.BindDispService(string uri)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.CallWaferobjectChangedEvent()
        {
            throw new NotImplementedException();
        }

        Task IStageSupervisor.ChangeDeviceFuncUsingName(string devName)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.ChangeLotMode(LotModeEnum mode)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.CheckAvailableStageAbsMove(double xPos, double yPos, double zPos, double tPos, double PZPos, ref bool stagebusy)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.CheckAvailableStageRelMove(double xPos, double yPos, double zPos, double tPos, double PZPos, ref bool stagebusy)
        {
            throw new NotImplementedException();
        }

        bool IStageSupervisor.CheckAxisBusy()
        {
            throw new NotImplementedException();
        }

        bool IStageSupervisor.CheckAxisIdle()
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.CheckManualZUpState()
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.CheckPinPadParameterValidity()
        {
            throw new NotImplementedException();
        }

        bool IStageSupervisor.CheckUsingHandler(int stageindex)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.CheckWaferStatus(bool isExist)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.ClearWaferStatus()
        {
            throw new NotImplementedException();
        }

        Task IStageSupervisor.ClickToMoveLButtonDown(object enableClickToMove)
        {
            throw new NotImplementedException();
        }

        ExceptionReturnData IStageSupervisor.ConvertToExceptionErrorCode(Exception err)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.DeInitGemConnectService()
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.DeInitService()
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.DoLot()
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.DoManualPinAlign(bool CheckStageMode)
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.DoManualSoaking()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.DoManualWaferAlign(bool CheckStageMode)
        {
            return EventCodeEnum.NONE;
        }

        Task<EventCodeEnum> IStageSupervisor.DoPinAlign()
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.DoPinPadMatch_FirstSequence()
        {
            return EventCodeEnum.NONE;
        }

        Task IStageSupervisor.DoSystemInit(bool showMessageDialogFlag)
        {
            throw new NotImplementedException();
        }

        Task<EventCodeEnum> IStageSupervisor.DoWaferAlign()
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.DO_ManualZDown()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.DO_ManualZUP()
        {
            return EventCodeEnum.NONE;
        }

        int IStageSupervisor.DutPadInfosCount()
        {
            throw new NotImplementedException();
        }

        Element<AlignStateEnum> IStageSupervisor.GetAlignState(AlignTypeEnum AlignType)
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetDevice()
        {
            throw new NotImplementedException();
        }

        string IStageSupervisor.GetDeviceName()
        {
            throw new NotImplementedException();
        }

        List<DeviceObject> IStageSupervisor.GetDevices()
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetDIEs()
        {
            throw new NotImplementedException();
        }

        (DispFlipEnum disphorflip, DispFlipEnum dispverflip) IStageSupervisor.GetDisplayFlipInfo()
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetLog(string date)
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetLogFromFilename(List<string> debug, List<string> temp, List<string> pin, List<string> pmi, List<string> lot)
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetLogFromFileName(EnumUploadLogType logtype, List<string> data)
        {
            throw new NotImplementedException();
        }

        string IStageSupervisor.GetLotErrorMessage()
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetMarkObject()
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetNCObject()
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetODTPdataFromFileName(string filename)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.GetPinDataFromPads()
        {
            return EventCodeEnum.NONE;
        }

        byte[] IStageSupervisor.GetPinImageFromStage(List<string> pinImage)
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetProbeCardObject()
        {
            throw new NotImplementedException();
        }

        PROBECARD_TYPE IStageSupervisor.GetProbeCardType()
        {
            throw new NotImplementedException();
        }

        (bool reverseX, bool reverseY) IStageSupervisor.GetReverseMoveInfo()
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetRMdataFromFileName(string filename)
        {
            throw new NotImplementedException();
        }

        IStageSlotInformation IStageSupervisor.GetSlotInfo()
        {
            throw new NotImplementedException();
        }

        List<string> IStageSupervisor.GetStageDebugDates()
        {
            throw new NotImplementedException();
        }

        CellInitModeEnum IStageSupervisor.GetStageInitState()
        {
            throw new NotImplementedException();
        }

        StageLockMode IStageSupervisor.GetStageLockMode()
        {
            throw new NotImplementedException();
        }

        List<string> IStageSupervisor.GetStageLotDates()
        {
            throw new NotImplementedException();
        }

        (GPCellModeEnum, StreamingModeEnum) IStageSupervisor.GetStageMode()
        {
            throw new NotImplementedException();
        }

        List<string> IStageSupervisor.GetStagePinDates()
        {
            throw new NotImplementedException();
        }

        List<string> IStageSupervisor.GetStagePMIDates()
        {
            throw new NotImplementedException();
        }

        List<string> IStageSupervisor.GetStageTempDates()
        {
            throw new NotImplementedException();
        }

        SubstrateInfoNonSerialized IStageSupervisor.GetSubstrateInfoNonSerialized()
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.GetWaferObject()
        {
            throw new NotImplementedException();
        }

        WaferObjectInfoNonSerialized IStageSupervisor.GetWaferObjectInfoNonSerialize()
        {
            throw new NotImplementedException();
        }

        int IStageSupervisor.GetWaferObjHashCode()
        {
            throw new NotImplementedException();
        }

        EnumSubsStatus IStageSupervisor.GetWaferStatus()
        {
            throw new NotImplementedException();
        }

        EnumWaferType IStageSupervisor.GetWaferType()
        {
            throw new NotImplementedException();
        }

        string IStageSupervisor.GetWaitCancelDialogHashCode()
        {
            throw new NotImplementedException();
        }

        TCW_Mode IStageSupervisor.Get_TCW_Mode()
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.HandlerVacOnOff(bool val, int stageindex)
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IHasDevParameterizable.InitDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.InitGemConnectService()
        {
            return EventCodeEnum.NONE;
        }

        Task IStageSupervisor.InitLoaderClient()
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.InitStageService(int stageAbsIndex)
        {
            throw new NotImplementedException();
        }

        bool IStageSupervisor.IsExistParamFile(string paramPath)
        {
            throw new NotImplementedException();
        }

        bool IStageSupervisor.IsForcedDoneMode()
        {
            throw new NotImplementedException();
        }

        bool IStageSupervisor.IsMovingState()
        {
            throw new NotImplementedException();
        }

        bool IStageSupervisor.IsServiceAvailable()
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IHasDevParameterizable.LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        void IStageSupervisor.LoaderConnected()
        {
            throw new NotImplementedException();
        }

        Task<EventCodeEnum> IStageSupervisor.LoaderInit()
        {
            return new Task<EventCodeEnum>(loaderInitFunc);
        }
        EventCodeEnum loaderInitFunc()
        {
            return EventCodeEnum.NONE;
        }


        string[] IStageSupervisor.LoadEventLog(string lFileName)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.LoadLUT()
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.LoadNCSysObject()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.LoadProberCard()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IHasSysParameterizable.LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.LoadTouchSensorObject()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.LoadWaferObject()
        {
            return EventCodeEnum.NONE;
        }

        void IStageSupervisor.LotPause()
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.MoveStageToTargetPos(object enableClickToMove)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.MOVETONEXTDIE()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.NotifySystemErrorToConnectedCells(EnumLoaderEmergency emgtype)
        {
            return EventCodeEnum.NONE;
        }

        void IStageSupervisor.OnceStopAfterProbingCmd(bool onceStopAfterProbing)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.OnceStopBeforeProbingCmd(bool onceStopBeforeProbing)
        {
            throw new NotImplementedException();
        }

        byte[] IStageSupervisor.OpenLogFile(string selectedFilePath)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IHasDevParameterizable.SaveDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.SaveNCSysObject()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.SaveProberCard()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.SaveSlotInfo()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IHasSysParameterizable.SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.SaveTouchSensorObject()
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.SaveWaferObject()
        {
            return EventCodeEnum.NONE;
        }

        void IStageSupervisor.SetAcceptUpdateDisp(bool flag)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetDevice(byte[] device, string devicename, string lotid, string lotCstHashCode, bool loaddev, int foupnumber, bool showprogress, bool manualDownload)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetDynamicMode(DynamicModeEnum dynamicModeEnum)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetEMG(EventCodeEnum errorCode)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetErrorCodeAlarm(EventCodeEnum errorCode)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetLotModeByForcedLotMode()
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetMoveTargetPos(double xpos, double ypos)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetNeedChangeParaemterInDeviceInfo(NeedChangeParameterInDevice needChangeParameter)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetProbeCardObject(IProbeCard param)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetStageInitState(CellInitModeEnum e)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.SetStageLock(ReasonOfStageMoveLock reason)
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.SetStageMode(GPCellModeEnum cellmodeenum)
        {
            return EventCodeEnum.NONE;
        }

        EventCodeEnum IStageSupervisor.SetStageUnlock(ReasonOfStageMoveLock reason)
        {
            return EventCodeEnum.NONE;
        }

        void IStageSupervisor.SetVacuum(bool ison)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.SetWaferMapCam(EnumProberCam cam)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.SetWaferObjectStatus()
        {
            return EventCodeEnum.NONE;
        }

        void IStageSupervisor.SetWaitCancelDialogHashCode(string hashCode)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.Set_TCW_Mode(bool isOn)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.StageSupervisorStateTransition(StageState state)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.StopAfterProbingCmd(bool stopAfterProbing)
        {
            throw new NotImplementedException();
        }

        void IStageSupervisor.StopBeforeProbingCmd(bool stopBeforeProbing)
        {
            throw new NotImplementedException();
        }

        Task<ErrorCodeResult> IStageSupervisor.SystemInit()
        {
            return null;
        }

        List<List<string>> IStageSupervisor.UpdateLogFile()
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.WaferHighViewIndexCoordMove(long mix, long miy)
        {
            return EventCodeEnum.NONE;
        }

        void IStageSupervisor.WaferIndexUpdated(long xindex, long yindex)
        {
            throw new NotImplementedException();
        }

        EventCodeEnum IStageSupervisor.WaferLowViewIndexCoordMove(long mix, long miy)
        {
            return EventCodeEnum.NONE;
        }

        void IModule.DeInitModule()
        {
        }

        EventCodeEnum IModule.InitModule()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class SorterMonitoringManager : IMonitoringManager
    {
        public IParam MonitoringSystemParam_IParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private IIOService _IOService;
        public IIOService IOService {
            get
            {
                if (_IOService == null) _IOService = this.IOManager().IOServ;
                return _IOService;
            }
            set
            {
                if (_IOService != value)
                {
                    _IOService = value;
                }
            }
        }

        public bool IsSystemError => false;

        public bool IsStageSystemError => false;

        public bool IsLoaderSystemError => false;

        public bool IsMachineInitDone => true;

        public bool IsMachinInitOn
        {
            get { return true; }
            set { }
        }

        public bool SkipCheckChuckVacuumFlag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private List<IMonitoringBehavior> _MonitoringBehaviorList;
        public List<IMonitoringBehavior> MonitoringBehaviorList { get => _MonitoringBehaviorList;
            set
            { 
                if (_MonitoringBehaviorList != value )
                {
                    _MonitoringBehaviorList = value;
                }
            } }

        private bool _Initialized;
        public bool Initialized
        {
            get { return _Initialized; }
        }

        public void DEBUG_Check()
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
        }

        public object GetHWPartCheckList()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {
            _Initialized = true;
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoaderEmergencyStop()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> MachineMonitoring()
        {
            return new Task<EventCodeEnum>(()=> { return EventCodeEnum.NONE; });
        }

        public EventCodeEnum RecievedFromLoaderEMG(EnumLoaderEmergency emgtype)
        {
            throw new NotImplementedException();
        }

        public void RunCheck()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveSysParameter()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageEmergencyStop()
        {
            throw new NotImplementedException();
        }

        public void StopCheck()
        {
            throw new NotImplementedException();
        }
    }

    public class SorterModuleResolver
    {
        public static IContainer Container;
        private static bool isConfiureDependecies = false;

        public static IContainer ConfigureDependencies()
        {
            try
            {
                if (isConfiureDependecies == false)
                {
                    var builder = new ContainerBuilder();

                    builder.RegisterType<FileManager>().As<IFileManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<ViewModelManager>().As<IViewModelManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance();
                    builder.RegisterType<VisionManager>().As<IVisionManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<LightAdmin>().As<ILightAdmin>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<CameraChannelAdmin>().As<ICameraChannelAdmin>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<IOManager>().As<IIOManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<ParamManager>().As<IParamManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<PMASManager>().As<IPMASManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<MotionManager>().As<IMotionManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<SorterStageSupervisor>().As<IStageSupervisor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<SorterMonitoringManager>().As<IMonitoringManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<FocusManager>().As<IFocusManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<MetroDialogManager>().As<IMetroDialogManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<TempCotrlSorter>().As<ITempController>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    Container = builder.Build();
                    isConfiureDependecies = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ConfigureDependencies(): Error occurred. Err = {err.Message}");
            }

            return Container;
        }
        private static void ModuleConstructorEvent(IActivatedEventArgs<IFactoryModule> obj)
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                string var = obj.Instance.GetType().FullName;

                // Load System Parameter
                if (obj.Instance is IHasSysParameterizable)
                {

                    try
                    {
                        LoggerManager.Debug($"Start {obj.Instance} LoadSysParameter.");
                        retval = (obj.Instance as IHasSysParameterizable).LoadSysParameter();
                        LoggerManager.Debug($"End {obj.Instance} LoadSysParameter.");

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"(obj.Instance as IHasSysParameterizable).LoadSysParameter() Failed");
                        }

                        LoggerManager.Debug($"[ModuleResolver] [ModuleConstructorEvent] Load System Parameter : {obj.Instance} - {retval}");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);

                        throw new Exception($"ModuleConstructorEvent : Occurt during LoadSysParameter in {obj.Instance} ");
                    }
                }

                // Load Device Parameter
                if (obj.Instance is IHasDevParameterizable)
                {
                    try
                    {
                        LoggerManager.Debug($"Start {obj.Instance} LoadDevParameter.");
                        retval = (obj.Instance as IHasDevParameterizable).LoadDevParameter();
                        retval = (obj.Instance as IHasDevParameterizable).InitDevParameter();
                        LoggerManager.Debug($"End {obj.Instance} LoadDevParameter.");

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"(obj.Instance as IHasDevParameterizable).LoadDevParameter() Failed");
                        }

                        LoggerManager.Debug($"[ModuleResolver] [ModuleConstructorEvent] Load Device Parameter : {obj.Instance} - {retval}");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);

                        throw new Exception($"ModuleConstructorEvent : Occurt during LoadDevParameter in {obj.Instance} ");
                    }
                }

                // Initialize Module
                if (obj.Instance is ProberInterfaces.IModule)
                {
                    try
                    {
                        DateTime data = DateTime.Now;
                        LoggerManager.Debug($"Start {obj.Instance} InitModule.");
                        retval = (obj.Instance as ProberInterfaces.IModule).InitModule();
                        LoggerManager.Debug($"End {obj.Instance} InitModule.");

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"(obj.Instance as ProberInterfaces.IModule).InitModule() Failed");
                        }

                        TimeSpan t = DateTime.Now - data;
                        LoggerManager.Debug($"[ModuleResolver] [ModuleConstructorEvent] Initialize : {obj.Instance} - {retval} // {t.TotalSeconds}");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);

                        throw new Exception($"ModuleConstructorEvent : occurred during InitModule in {obj.Instance} ");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static void UserModuleConstructorEvent(IFactoryModule obj)
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                string var = obj.GetType().FullName;

                // Load System Parameter
                if (obj is IHasSysParameterizable)
                {

                    try
                    {
                        LoggerManager.Debug($"Start {obj} LoadSysParameter.");
                        retval = (obj as IHasSysParameterizable).LoadSysParameter();
                        LoggerManager.Debug($"End {obj} LoadSysParameter.");

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"(obj as IHasSysParameterizable).LoadSysParameter() Failed");
                        }

                        LoggerManager.Debug($"[ModuleResolver] [ModuleConstructorEvent] Load System Parameter : {obj} - {retval}");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);

                        throw new Exception($"ModuleConstructorEvent : Occurt during LoadSysParameter in {obj} ");
                    }
                }

                // Load Device Parameter
                if (obj is IHasDevParameterizable)
                {
                    try
                    {
                        LoggerManager.Debug($"Start {obj} LoadDevParameter.");
                        retval = (obj as IHasDevParameterizable).LoadDevParameter();
                        retval = (obj as IHasDevParameterizable).InitDevParameter();
                        LoggerManager.Debug($"End {obj} LoadDevParameter.");

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"(obj as IHasDevParameterizable).LoadDevParameter() Failed");
                        }

                        LoggerManager.Debug($"[ModuleResolver] [ModuleConstructorEvent] Load Device Parameter : {obj} - {retval}");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);

                        throw new Exception($"ModuleConstructorEvent : Occurt during LoadDevParameter in {obj} ");
                    }
                }

                // Initialize Module
                if (obj is ProberInterfaces.IModule)
                {
                    try
                    {
                        DateTime data = DateTime.Now;
                        LoggerManager.Debug($"Start {obj} InitModule.");
                        retval = (obj as ProberInterfaces.IModule).InitModule();
                        LoggerManager.Debug($"End {obj} InitModule.");

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"(obj as ProberInterfaces.IModule).InitModule() Failed");
                        }

                        TimeSpan t = DateTime.Now - data;
                        LoggerManager.Debug($"[ModuleResolver] [ModuleConstructorEvent] Initialize : {obj} - {retval} // {t.TotalSeconds}");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);

                        throw new Exception($"ModuleConstructorEvent : occurred during InitModule in {obj} ");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}

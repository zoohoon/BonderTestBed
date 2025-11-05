using LogModule;
using System;
using System.Linq;
using Autofac;
using ProberInterfaces.PinAlign;
using ProberInterfaces.MarkAlign;
using System.Collections.ObjectModel;
using ProberInterfaces.PnpSetup;

using ProberInterfaces.NeedleClean;
using ProberInterfaces.LoaderController;
using ProberInterfaces.Foup;
using ProberInterfaces.Event;
using ProberInterfaces.AirBlow;
using ProberInterfaces.AirCooling;
using ProberInterfaces.Device;
using ProberInterfaces.DialogControl;
using ProberInterfaces.Command;
using ProberInterfaces.CardChange;
using ProberErrorCode;
using ProberInterfaces.Pad;
using ProberInterfaces.Temperature;
using ProberInterfaces.Service;
using ProberInterfaces.Wizard;
using ProberInterfaces.E84.ProberInterfaces;
using ProberInterfaces.PolishWafer;
using ProberInterfaces.AutoTilt;
using ProberInterfaces.PMI;
using ProberInterfaces.WaferTransfer;
using ProberInterfaces.State;
using System.ComponentModel;
using ProberInterfaces.SequenceRunner;
using ProberInterfaces.Template;
using ProberInterfaces.Focus;
using ProberInterfaces.ResultMap;
using ProberInterfaces.NeedleBrush;
using ProberInterfaces.Loader;
using System.Runtime.Serialization;
using System.ServiceModel;
using MetroDialogInterfaces;
using System.Runtime.CompilerServices;
using ProberInterfaces.Utility;
using ProberInterfaces.Communication.Tester;
using ProberInterfaces.Retest;
using ProberInterfaces.ODTP;
using ProberInterfaces.TouchSensor;
using ProberInterfaces.SignalTower;
using ProberInterfaces.EnvControl;
using ProberInterfaces.Temperature.EnvMonitoring;
using ProberInterfaces.Bonder;
using ProberInterfaces.FDAlign;

namespace ProberInterfaces
{
    //using Autofac;
    public enum ProberSystemMode
    {
        EMUL,
        NOMAL
    }

    [Serializable]
    public class ProberSystemModeParam
    {
        public ProberSystemMode Mode;
    }
    public interface IProberStation : IFactoryModule, IModule, INotifyPropertyChanged
    {
        int MaskingLevel { get; set; }
        void ChangeScreenLotOP();
        void ChangeScreenToPnp(object setupstep);

    }

    public static class Extensions_IModule
    {
        private static Autofac.IContainer Container;
        private static Autofac.IContainer LoaderContainer;

        public static void SetContainer(this IFactoryModule module, Autofac.IContainer container)
        {
            Container = container;
        }
        public static void SetLoaderContainer(this IFactoryModule module, Autofac.IContainer container)
        {
            LoaderContainer = container;
        }

        public static Autofac.IContainer GetContainer(this IFactoryModule module)
        {
            return Container;
        }

        public static Autofac.IContainer GetLoaderContainer(this IFactoryModule module)
        {
            return LoaderContainer;
        }

        private static IMotionManager _Motion;
        public static IMotionManager Motion(this IFactoryModule module)
        {
            if (_Motion == null)
                GetModule<IMotionManager>(out _Motion);

            return _Motion;
        }

        private static IDutObject _DutObject;
        public static IDutObject DutObject(this IFactoryModule module)
        {
            if (_DutObject == null)
                GetModule<IDutObject>(out _DutObject);

            return _DutObject;
        }

        private static IProberStation _ProberStation;
        public static IProberStation ProberStation(this IFactoryModule module)
        {
            if (_ProberStation == null)
                GetModule<IProberStation>(out _ProberStation);

            return _ProberStation;
        }

        private static ILoaderController _LoaderController;
        public static ILoaderController LoaderController(this IFactoryModule module)
        {
            if (_LoaderController == null)
                GetModule<ILoaderController>(out _LoaderController);

            return _LoaderController;
        }

        private static IPMASManager _PMASManager;
        public static IPMASManager PMASManager(this IFactoryModule module)
        {
            if (_PMASManager == null)
                GetModule<IPMASManager>(out _PMASManager);

            return _PMASManager;
        }

        private static IMotionManager _MotionManager;
        public static IMotionManager MotionManager(this IFactoryModule module)
        {
            if (_MotionManager == null)
                GetModule<IMotionManager>(out _MotionManager);

            return _MotionManager;
        }

        private static ICoordinateManager _CoordinateManager;
        public static ICoordinateManager CoordinateManager(this IFactoryModule module)
        {
            if (_CoordinateManager == null)
                GetModule<ICoordinateManager>(out _CoordinateManager);

            return _CoordinateManager;
        }

        private static IStageSupervisor _StageSupervisor;
        public static IStageSupervisor StageSupervisor(this IFactoryModule module)
        {

            if (_StageSupervisor == null)
                GetModule<IStageSupervisor>(out _StageSupervisor);

            return _StageSupervisor;
        }

        private static IFileManager _FileManager;
        public static IFileManager FileManager(this IFactoryModule module)
        {
            if (_FileManager == null)
                GetModule<IFileManager>(out _FileManager);

            return _FileManager;
        }

        private static ILoaderFileManager _LoaderFileManager;
        public static IFileManager LoaderFileManager(this IFactoryModule module)
        {
            if (_LoaderFileManager == null)
                GetModule<ILoaderFileManager>(out _LoaderFileManager);

            return _LoaderFileManager as IFileManager;
        }

        private static IVisionManager _VisionManager;
        public static IVisionManager VisionManager(this IFactoryModule module)
        {
            if (_VisionManager == null)
                GetModule<IVisionManager>(out _VisionManager);

            return _VisionManager;
        }

        private static ILightAdmin _LightAdmin;
        public static ILightAdmin LightAdmin(this IFactoryModule module)
        {
            if (_LightAdmin == null)
                GetModule<ILightAdmin>(out _LightAdmin);

            return _LightAdmin;
        }

        private static ICameraChannelAdmin _CameraChannel;
        public static ICameraChannelAdmin CameraChannel(this IFactoryModule module)
        {
            if (_CameraChannel == null)
                GetModule<ICameraChannelAdmin>(out _CameraChannel);

            return _CameraChannel;
        }

        private static IIOManager _IOManager;
        public static IIOManager IOManager(this IFactoryModule module)
        {
            if (_IOManager == null)
                GetModule<IIOManager>(out _IOManager);

            return _IOManager;
        }

        private static IMarkAligner _MarkAligner;
        public static IMarkAligner MarkAligner(this IFactoryModule module)
        {
            if (_MarkAligner == null)
                GetModule<IMarkAligner>(out _MarkAligner);

            return _MarkAligner;
        }

        private static IWaferAligner _WaferAligner;
        public static IWaferAligner WaferAligner(this IFactoryModule module)
        {
            if (_WaferAligner == null)
                GetModule<IWaferAligner>(out _WaferAligner);

            return _WaferAligner;
        }

        private static IPinAligner _PinAligner;
        public static IPinAligner PinAligner(this IFactoryModule module)
        {
            if (_PinAligner == null)
                GetModule<IPinAligner>(out _PinAligner);

            return _PinAligner;
        }

        private static IFDWaferAligner _FDWaferAligner; // 251013 sebas
        public static IFDWaferAligner FDWaferAligner(this IFactoryModule module)    // 251013 sebas
        {
            if (_FDWaferAligner == null)
                GetModule<IFDWaferAligner>(out _FDWaferAligner);

            return _FDWaferAligner;
        }

        private static IPadRegist _PadRegist;
        public static IPadRegist PadRegist(this IFactoryModule module)
        {
            if (_PadRegist == null)
                GetModule<IPadRegist>(out _PadRegist);

            return _PadRegist;
        }

        private static IAutoLightAdvisor _AutoLightAdvisor;
        public static IAutoLightAdvisor AutoLightAdvisor(this IFactoryModule module)
        {
            if (_AutoLightAdvisor == null)
                GetModule<IAutoLightAdvisor>(out _AutoLightAdvisor);

            return _AutoLightAdvisor;
        }

        private static ITempController _TempController;
        public static ITempController TempController(this IFactoryModule module)
        {
            if (_TempController == null)
                GetModule<ITempController>(out _TempController);


            return _TempController;
        }

        private static IGEMModule _GEMModule;
        public static IGEMModule GEMModule(this IFactoryModule module)
        {
            if (_GEMModule == null)
                GetModule<IGEMModule>(out _GEMModule);

            return _GEMModule;
        }
        private static ISystemstatus _SysStateModule;
        public static ISystemstatus SysState(this IFactoryModule module)
        {
            if (_SysStateModule == null)
                _SysStateModule = Container?.Resolve<ISystemstatus>();
            return _SysStateModule;
        }
        private static IProbingSequenceModule _ProbingSequenceModule;
        public static IProbingSequenceModule ProbingSequenceModule(this IFactoryModule module)
        {
            if (_ProbingSequenceModule == null)
                GetModule<IProbingSequenceModule>(out _ProbingSequenceModule);

            return _ProbingSequenceModule;
        }

        private static ITesterCommunicationManager _TesterCommunicationManager;
        public static ITesterCommunicationManager TesterCommunicationManager(this IFactoryModule module)
        {
            if (_TesterCommunicationManager == null)
                GetModule<ITesterCommunicationManager>(out _TesterCommunicationManager);

            return _TesterCommunicationManager;
        }

        private static IGPIB _GPIB;
        public static IGPIB GPIB(this IFactoryModule module)
        {
            if (_GPIB == null)
                GetModule<IGPIB>(out _GPIB);

            return _GPIB;
        }

        private static ITCPIP _TCPIPModule;
        public static ITCPIP TCPIPModule(this IFactoryModule module)
        {
            if (_TCPIPModule == null)
                GetModule<ITCPIP>(out _TCPIPModule);

            return _TCPIPModule;
        }
        private static ILotOPModule _LotOPModule;
        public static ILotOPModule LotOPModule(this IFactoryModule module)
        {
            if (_LotOPModule == null)
                GetModule<ILotOPModule>(out _LotOPModule);

            return _LotOPModule;
        }

        private static ILoaderOPModule _LoaderOPModule;
        public static ILoaderOPModule LoaderOPModule(this IFactoryModule module)
        {
            if (_LoaderOPModule == null)
                GetModule<ILoaderOPModule>(out _LoaderOPModule);

            return _LoaderOPModule;
        }
        
        private static IRetestModule _RetestModule;
        public static IRetestModule RetestModule(this IFactoryModule module)
        {
            if (_RetestModule == null)
                GetModule<IRetestModule>(out _RetestModule);

            return _RetestModule;
        }

        private static IProbingModule _ProbingModule;
        public static IProbingModule ProbingModule(this IFactoryModule module)
        {
            if (_ProbingModule == null)
                GetModule<IProbingModule>(out _ProbingModule);

            return _ProbingModule;
        }
        private static IForceMeasure _ForceMeasure;
        public static IForceMeasure GetForceMeasure(this IFactoryModule module)
        {
            if (_ForceMeasure == null)
                GetModule<IForceMeasure>(out _ForceMeasure);

            return _ForceMeasure;
        }

        private static ICardChangeModule _CardChangeModule;
        public static ICardChangeModule CardChangeModule(this IFactoryModule module)
        {
            if (_CardChangeModule == null)
                GetModule<ICardChangeModule>(out _CardChangeModule);

            return _CardChangeModule;
        }

        private static ISoakingModule _SoakingModule;
        public static ISoakingModule SoakingModule(this IFactoryModule module)
        {
            if (_SoakingModule == null)
                GetModule<ISoakingModule>(out _SoakingModule);

            return _SoakingModule;
        }

        private static IWaferTransferModule _WaferTransferModule;
        public static IWaferTransferModule WaferTransferModule(this IFactoryModule module)
        {
            if (_WaferTransferModule == null)
                GetModule<IWaferTransferModule>(out _WaferTransferModule);

            return _WaferTransferModule;
        }

        private static IBonderModule _BonderModule; // 251013 sebas
        public static IBonderModule BonderModule(this IFactoryModule module)    // 251013 sebas
        {
            if (_BonderModule == null)
                GetModule<IBonderModule>(out _BonderModule);

            return _BonderModule;
        }

        private static IBonderSupervisor _BonderSupervisor; // 251013 sebas
        public static IBonderSupervisor BonderSupervisor(this IFactoryModule module)    // 251013 sebas
        {
            if (_BonderSupervisor == null)
                GetModule<IBonderSupervisor>(out _BonderSupervisor);

            return _BonderSupervisor;
        }

        private static INeedleCleanModule _NeedleCleaner;
        public static INeedleCleanModule NeedleCleaner(this IFactoryModule module)
        {
            if (_NeedleCleaner == null)
                GetModule<INeedleCleanModule>(out _NeedleCleaner);

            return _NeedleCleaner;
        }

        private static INeedleBrushModule _NeedleBrush;
        public static INeedleBrushModule NeedleBrush(this IFactoryModule module)
        {
            if (_NeedleBrush == null)
                GetModule<INeedleBrushModule>(out _NeedleBrush);

            return _NeedleBrush;
        }

        private static ISequenceEngineManager _SequenceEngineManager;
        public static ISequenceEngineManager SequenceEngineManager(this IFactoryModule module)
        {
            if (_SequenceEngineManager == null)
                GetModule<ISequenceEngineManager>(out _SequenceEngineManager);

            return _SequenceEngineManager;
        }

        private static IPMIModule _PMIModule;
        public static IPMIModule PMIModule(this IFactoryModule module)
        {
            if (_PMIModule == null)
                GetModule<IPMIModule>(out _PMIModule);

            return _PMIModule;
        }
        private static IFocusManager _FocusManager;
        public static IFocusManager FocusManager(this IFactoryModule module)
        {
            if (_FocusManager == null)
                GetModule<IFocusManager>(out _FocusManager);

            return _FocusManager;
        }

        /// <summary>
        /// == 임시 설명 ==
        /// <para>Event를 추가하고 싶으신가요? 발생하고 싶으신가요? EventManager를 사용해보세요!</para>
        /// <para>사용법을 모르신다고요? 그럼 GEM, GPIB 모듈을 참고하세요!</para>
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private static IEventManager _EventManager;
        public static IEventManager EventManager(this IFactoryModule module)
        {
            if (_EventManager == null)
                GetModule<IEventManager>(out _EventManager);

            return _EventManager;
        }


        private static IEventExecutor _EventExecutor;
        public static IEventExecutor EventExecutor(this IFactoryModule module)
        {
            if (_EventExecutor == null)
                GetModule<IEventExecutor>(out _EventExecutor);

            return _EventExecutor;
        }

        private static IInternal _Internal;
        public static IInternal Internal(this IFactoryModule module)
        {
            if (_Internal == null)
                GetModule<IInternal>(out _Internal);

            return _Internal;
        }
        private static INotify _Notify;
        public static INotify Notify(this IFactoryModule module)
        {
            if (_Notify == null)
                GetModule<INotify>(out _Notify);

            return _Notify;
        }

        private static IDisplayPortDialog _DisplayPortDialog;
        public static IDisplayPortDialog DisplayPortDialog(this IFactoryModule module)
        {
            if (_DisplayPortDialog == null)
                GetModule<IDisplayPortDialog>(out _DisplayPortDialog);

            return _DisplayPortDialog;
        }

        private static IInspection _InspectionModule;
        public static IInspection InspectionModule(this IFactoryModule module)
        {
            if (_InspectionModule == null)
                GetModule<IInspection>(out _InspectionModule);

            return _InspectionModule;
        }

        private static ITempDisplayDialogService _TempDisplayDialogService;
        public static ITempDisplayDialogService TempDisplayDialogService(this IFactoryModule module)
        {
            if (_TempDisplayDialogService == null)
                GetModule<ITempDisplayDialogService>(out _TempDisplayDialogService);

            return _TempDisplayDialogService;
        }

        private static IAutoTiltModule _AutoTiltModule;
        public static IAutoTiltModule AutoTiltModule(this IFactoryModule module)
        {
            if (_AutoTiltModule == null)
                GetModule<IAutoTiltModule>(out _AutoTiltModule);

            return _AutoTiltModule;
        }

        static IGPLoader _GPLoader;
        public static IGPLoader GetGPLoader(this IFactoryModule module)
        {
            if (_GPLoader == null)
                GetModule<IGPLoader>(out _GPLoader);

            return _GPLoader;
        }
        
        private static IParamManager _ParamManager;
        public static IParamManager ParamManager(this IFactoryModule module)
        {
            if (_ParamManager == null)
                GetModule<IParamManager>(out _ParamManager);

            return _ParamManager;
        }
        private static IPolishWaferModule _PolishWaferModule;
        public static IPolishWaferModule PolishWaferModule(this IFactoryModule module)
        {
            if (_PolishWaferModule == null)
                GetModule<IPolishWaferModule>(out _PolishWaferModule);

            return _PolishWaferModule;
        }

        private static IAirBlowChuckCleaningModule _AirBlowChuckCleaningModule;
        public static IAirBlowChuckCleaningModule AirBlowChuckCleaningModule(this IFactoryModule module)
        {
            if (_AirBlowChuckCleaningModule == null)
                GetModule<IAirBlowChuckCleaningModule>(out _AirBlowChuckCleaningModule);

            return _AirBlowChuckCleaningModule;
        }

        private static IAirBlowWaferCleaningModule _AirBlowWaferCleaningModule;
        public static IAirBlowWaferCleaningModule AirBlowWaferCleaningModule(this IFactoryModule module)
        {
            if (_AirBlowWaferCleaningModule == null)
                GetModule<IAirBlowWaferCleaningModule>(out _AirBlowWaferCleaningModule);

            return _AirBlowWaferCleaningModule;
        }

        private static IManualContact _ManualContactModule;
        public static IManualContact ManualContactModule(this IFactoryModule module)
        {
            if (_ManualContactModule == null)
                GetModule<IManualContact>(out _ManualContactModule);

            return _ManualContactModule;
        }

        private static IAirBlowTempControlModule _AirBlowTempControlModule;
        public static IAirBlowTempControlModule AirBlowTempControlModule(this IFactoryModule module)
        {
            if (_AirBlowTempControlModule == null)
                GetModule<IAirBlowTempControlModule>(out _AirBlowTempControlModule);

            return _AirBlowTempControlModule;
        }

        private static IAirCoolingModule _AirCoolingModule;
        public static IAirCoolingModule AirCoolingModule(this IFactoryModule module)
        {
            if (_AirCoolingModule == null)
                GetModule<IAirCoolingModule>(out _AirCoolingModule);

            return _AirCoolingModule;
        }

        private static IE84Module _E84Module;
        public static IE84Module E84Module(this IFactoryModule module)
        {
            if (_E84Module == null)
                GetModule<IE84Module>(out _E84Module);

            return _E84Module;
        }

        //private static IRFIDModule _RFIDModule;
        //public static IRFIDModule RFIDModule(this IFactoryModule module)
        //{
        //    if (_RFIDModule == null)
        //        _RFIDModule = Container?.Resolve<IRFIDModule>();
        //    return _RFIDModule;
        //}

        private static IFoupOpModule _FoupOpModule;
        public static IFoupOpModule FoupOpModule(this IFactoryModule module)
        {
            if (_FoupOpModule == null)
                GetModule<IFoupOpModule>(out _FoupOpModule);

            return _FoupOpModule;
        }
        private static IFoupIOStates _FoupIO;
        public static IFoupIOStates GetFoupIO(this IFactoryModule module)
        {
            if (_FoupIO == null)
                GetModule<IFoupIOStates>(out _FoupIO);

            return _FoupIO;
        }
        private static ICommandManager _CommandManager;
        public static ICommandManager CommandManager(this IFactoryModule module)
        {
            if (_CommandManager == null)
                GetModule<ICommandManager>(out _CommandManager);

            return _CommandManager;
        }

        private static ILampManager _LampManager;
        public static ILampManager LampManager(this IFactoryModule module)
        {
            if (_LampManager == null)
                GetModule<ILampManager>(out _LampManager);

            return _LampManager;
        }

        private static ISequenceRunner _SequenceRunner;
        public static ISequenceRunner SequenceRunner(this IFactoryModule module)
        {
            if (_SequenceRunner == null)
                GetModule<ISequenceRunner>(out _SequenceRunner);

            return _SequenceRunner;
        }

        private static IModuleUpdater _ModuleUpdater;
        public static IModuleUpdater ModuleUpdater(this IFactoryModule module)
        {
            if (_ModuleUpdater == null)
                GetModule<IModuleUpdater>(out _ModuleUpdater);

            return _ModuleUpdater;
        }

        private static IRemoteServiceModule _RemoteServiceModule;
        public static IRemoteServiceModule RemoteServiceModule(this IFactoryModule module)
        {
            if (_RemoteServiceModule == null)
                GetModule<IRemoteServiceModule>(out _RemoteServiceModule);

            return _RemoteServiceModule;
        }

        private static IWizardManager _WizardManager;
        public static IWizardManager WizardManager(this IFactoryModule module)
        {
            if (_WizardManager == null)
                GetModule<IWizardManager>(out _WizardManager);

            return _WizardManager;
        }

        private static IViewModelManager _ViewModelManager;
        public static IViewModelManager ViewModelManager(this IFactoryModule module)
        {
            if (_ViewModelManager == null)
                GetModule<IViewModelManager>(out _ViewModelManager);

            return _ViewModelManager;
        }

        private static IMonitoringManager _MonitoringManager;
        public static IMonitoringManager MonitoringManager(this IFactoryModule module)
        {
            if (_MonitoringManager == null)
                GetModule<IMonitoringManager>(out _MonitoringManager);

            return _MonitoringManager;
        }

        private static ICylinderManager _CylinderManager;
        public static ICylinderManager CylinderManager(this IFactoryModule module)
        {
            if (_CylinderManager == null)
                GetModule<ICylinderManager>(out _CylinderManager);

            return _CylinderManager;
        }

        private static IStatisticsManager _StatisticsManager;
        public static IStatisticsManager StatisticsManager(this IFactoryModule module)
        {
            if (_StatisticsManager == null)
                GetModule<IStatisticsManager>(out _StatisticsManager);

            return _StatisticsManager;
        }

        private static IPnpManager _PnPManager;
        public static IPnpManager PnPManager(this IFactoryModule module)
        {
            if (_PnPManager == null)
                GetModule<IPnpManager>(out _PnPManager);

            return _PnPManager;
        }

        private static ITemplateManager _TemplateManager;
        public static ITemplateManager TemplateManager(this IFactoryModule module)
        {
            if (_TemplateManager == null)
                GetModule<ITemplateManager>(out _TemplateManager);

            return _TemplateManager;
        }

        private static IMetroDialogManager _MetroDialogManager;
        public static IMetroDialogManager MetroDialogManager(this IFactoryModule module)
        {
            if (_MetroDialogManager == null)
                GetModule<IMetroDialogManager>(out _MetroDialogManager);

            return _MetroDialogManager;
        }

        private static IDeviceUpDownManager _DeviceUpDownManager;
        public static IDeviceUpDownManager DeviceUpDownManager(this IFactoryModule module)
        {
            if (_DeviceUpDownManager == null)
                GetModule<IDeviceUpDownManager>(out _DeviceUpDownManager);

            return _DeviceUpDownManager;
        }

        private static IStageCommunicationManager _StageCommunicationManager;
        public static IStageCommunicationManager StageCommunicationManager(this IFactoryModule module)
        {
            if (_StageCommunicationManager == null)
                GetModule<IStageCommunicationManager>(out _StageCommunicationManager);

            return _StageCommunicationManager;
        }

        private static IDeviceManager _DeviceManager;
        public static IDeviceManager DeviceManager(this IFactoryModule module)
        {
            if (_DeviceManager == null)
                GetModule<IDeviceManager>(out _DeviceManager);

            return _DeviceManager;
        }

        private static IResultMapManager _ResultMapManager;
        public static IResultMapManager ResultMapManager(this IFactoryModule module)
        {
            if (_ResultMapManager == null)
                GetModule<IResultMapManager>(out _ResultMapManager);

            return _ResultMapManager;
        }

        private static IODTPManager _ODTPManager;
        public static IODTPManager ODTPManager(this IFactoryModule module)
        {
            if (_ODTPManager == null)
                GetModule<IODTPManager>(out _ODTPManager);

            return _ODTPManager;
        }

        private static IGPCardAligner _GPCardAligner;
        public static IGPCardAligner GPCardAligner(this IFactoryModule module)
        {
            if (_GPCardAligner == null)
                GetModule<IGPCardAligner>(out _GPCardAligner);

            return _GPCardAligner;
        }

        private static ILoaderRemoteMediator _LoaderRemoteMediator;
        public static ILoaderRemoteMediator LoaderRemoteMediator(this IFactoryModule module)
        {
            if (_LoaderRemoteMediator == null)
            {
                GetModule<ILoaderRemoteMediator>(out _LoaderRemoteMediator);
            }

            return _LoaderRemoteMediator;
        }

        private static IEnvControlManager _EnvControlManager;
        public static IEnvControlManager EnvControlManager(this IFactoryModule module)
        {
            if (_EnvControlManager == null)
                GetModule<IEnvControlManager>(out _EnvControlManager);

            return _EnvControlManager;
        }

        private static INotifyManager _NotifyManager;
        public static INotifyManager NotifyManager(this IFactoryModule module)
        {
            if (_NotifyManager == null)
                GetModule<INotifyManager>(out _NotifyManager);

            return _NotifyManager;
        }

        private static IDeviceModule _DeviceModule;
        public static IDeviceModule DeviceModule(this IFactoryModule module)
        {
            if (_DeviceModule == null)
                GetModule<IDeviceModule>(out _DeviceModule);

            return _DeviceModule;
        }

        private static ITouchSensorBaseSetupModule _TouchSensorBaseSetupModule;
        public static ITouchSensorBaseSetupModule TouchSensorBaseSetupModule(this IFactoryModule module)
        {
            if (_TouchSensorBaseSetupModule == null)
                GetModule<ITouchSensorBaseSetupModule>(out _TouchSensorBaseSetupModule);

            return _TouchSensorBaseSetupModule;
        }

        private static ITouchSensorTipSetupModule _TouchSensorTipSetupModule;
        public static ITouchSensorTipSetupModule TouchSensorTipSetupModule(this IFactoryModule module)
        {
            if (_TouchSensorTipSetupModule == null)
                GetModule<ITouchSensorTipSetupModule>(out _TouchSensorTipSetupModule);

            return _TouchSensorTipSetupModule;
        }

        private static ITouchSensorPadRefSetupModule _TouchSensorPadRefSetupModule;
        public static ITouchSensorPadRefSetupModule TouchSensorPadRefSetupModule(this IFactoryModule module)
        {
            if (_TouchSensorPadRefSetupModule == null)
                GetModule<ITouchSensorPadRefSetupModule>(out _TouchSensorPadRefSetupModule);

            return _TouchSensorPadRefSetupModule;
        }

        private static ITouchSensorCalcOffsetModule _TouchSensorCalcOffsetModule;
        public static ITouchSensorCalcOffsetModule TouchSensorCalcOffsetModule(this IFactoryModule module)
        {
            if (_TouchSensorCalcOffsetModule == null)
                GetModule<ITouchSensorCalcOffsetModule>(out _TouchSensorCalcOffsetModule);

            return _TouchSensorCalcOffsetModule;
        }

        private static ISignalTowerManager _SignalTowerManager;
        public static ISignalTowerManager SignalTowerManager(this IFactoryModule module)
        {
            if (_SignalTowerManager == null)
                GetModule<ISignalTowerManager>(out _SignalTowerManager);

            return _SignalTowerManager;
        }        

        private static ITransferManager _TranferManager;
        public static ITransferManager TranferManager(this IFactoryModule module)
        {
            if (_TranferManager == null)
                GetModule<ITransferManager>(out _TranferManager);

            return _TranferManager;
        }

        private static IEnvModule _EnvModule;
        public static IEnvModule EnvModule(this IFactoryModule module)
        {
            if (_EnvModule == null)
                GetModule<IEnvModule>(out _EnvModule);

            return _EnvModule;
        }



        private static IEnvMonitoringManager _EnvMonitoringManager;
        public static IEnvMonitoringManager EnvMonitoringManager(this IFactoryModule module)
        {
            if (_EnvMonitoringManager == null)
                GetModule<IEnvMonitoringManager>(out _EnvMonitoringManager);

            return _EnvMonitoringManager;
        }


        private static void GetModule<T>(out T module)
        {
            // TOOD : 호출 측, 간소화 가능?

            if (Container != null)
            {
                if (Container.IsRegistered<T>() == true)
                {
                    module = Container.Resolve<T>();
                }
                else
                {
                    module = default(T);
                }
            }
            else
            {
                module = default(T);
            }
        }
    }

    public interface IAlignModule
    {
        Element<AlignStateEnum> AlignState { get; set; }
        AlignStateEnum GetAlignState();
        void SetAlignState(AlignStateEnum state);
    }

    [ServiceContract]
    public interface IStateModule : IFactoryModule, IModule, ILotReadyAble, IParamValidation
    {
        ReasonOfError ReasonOfError { get; set; }

        CommandSlot CommandSendSlot { get; set; }
        CommandSlot CommandRecvSlot { get; set; }
        CommandSlot CommandRecvProcSlot { get; set; }
        CommandSlot CommandRecvDoneSlot { get; set; }
        CommandTokenSet RunTokenSet { get; set; }
        CommandInformation CommandInfo { get; set; }

        ModuleStateBase ModuleState { get; }
        bool CanExecute(IProbeCommandToken token);
        void StateTransition(ModuleStateBase state);

        ObservableCollection<TransitionInfo> TransitionInfo { get; }

        ModuleStateEnum Execute();
        ModuleStateEnum Pause();
        ModuleStateEnum Resume();
        ModuleStateEnum End();
        ModuleStateEnum Abort();

        [OperationContract]
        EventCodeEnum ClearState();

        bool IsBusy();
        EnumModuleForcedState ForcedDone { get; set; }

        string GetModuleMessage();
    }

    public interface ILotReadyAble
    {
        bool IsLotReady(out string msg);
    }

    public interface IManualOPReadyAble
    {
        bool IsManualOPReady(out string msg);
        bool IsManualTriggered { get; set; }
        bool IsManualOPFinished { get; set; }
        EventCodeEnum ManualOPResult { get; set; }
        void CompleteManualOperation(EventCodeEnum retval);
        EventCodeEnum DoManualOperation(IProbeCommandParameter param = null);
    }

    public class ReasonOfError : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ModuleEnum _ModuleType;
        public ModuleEnum ModuleType
        {
            get { return _ModuleType; }
            set
            {
                if (value != _ModuleType)
                {
                    _ModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LimitedSizeObservableCollection<EventCodeInfo> _EventCodeHistories = new LimitedSizeObservableCollection<EventCodeInfo>(100);
        private LimitedSizeObservableCollection<EventCodeInfo> EventCodeHistories
        {
            get { return _EventCodeHistories; }
            set
            {
                if (value != _EventCodeHistories)
                {
                    _EventCodeHistories = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ReasonOfError(ModuleEnum moduleType)
        {
            this.ModuleType = moduleType;
        }

        public void AddEventCodeInfo(EventCodeEnum code, string message, string caller, [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EventCodeHistories.Add(new EventCodeInfo(this.ModuleType, code, message, caller));
                });
                
                LoggerManager.Debug($"ReasonOfError has occurred. Module Type = {ModuleType.ToString()}, Code = {code.ToString()}, Message = {message}, Caller = {caller}, LineNumber = {lineNumber}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetLastEventMessage()
        {
            string retval = string.Empty;

            try
            {
                var last = EventCodeHistories.LastOrDefault();

                if (last != null)
                {
                    retval = last.Message;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeInfo GetLastEventCode()
        {
            EventCodeInfo retval = null;

            try
            {
                var last = EventCodeHistories.LastOrDefault();

                if (last != null)
                {
                    retval = last;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void Confirmed()
        {
            try
            {
                if (EventCodeHistories.Count > 0)
                {
                    foreach (var item in EventCodeHistories)
                    {
                        item.Checked = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private string _Reason;
        //public string Reason
        //{
        //    get { return _Reason; }
        //    set { _Reason = value; }
        //}

        //private ModuleEnum _ModuleType;
        //public ModuleEnum ModuleType
        //{
        //    get { return _ModuleType; }
        //    set { _ModuleType = value; }
        //}

        //public ReasonOfError(ModuleEnum moduleType)
        //{
        //    _ModuleType = moduleType;
        //}

        //public ReasonOfError(ModuleEnum moduleType, string reason)
        //{
        //    _ModuleType = moduleType;
        //    _Reason = reason;
        //}
    }

    public class EventCodeInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ModuleEnum _ModuleType;
        public ModuleEnum ModuleType
        {
            get { return _ModuleType; }
            set
            {
                if (value != _ModuleType)
                {
                    _ModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EventCodeEnum _EventCode;
        public EventCodeEnum EventCode
        {
            get { return _EventCode; }
            set
            {
                if (value != _EventCode)
                {
                    _EventCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set
            {
                if (value != _Message)
                {
                    _Message = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Caller;
        public string Caller
        {
            get { return _Caller; }
            set
            {
                if (value != _Caller)
                {
                    _Caller = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _OccurredTime;
        public string OccurredTime
        {
            get { return _OccurredTime; }
            private set
            {
                if (value != _OccurredTime)
                {
                    _OccurredTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Checked;
        public bool Checked
        {
            get { return _Checked; }
            set
            {
                if (value != _Checked)
                {
                    _Checked = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeInfo(ModuleEnum moduletype, EventCodeEnum code, string message, string caller)
        {
            try
            {
                this.ModuleType = moduletype;
                this.EventCode = code;
                this.Message = message;

                this.Caller = caller;

                this.OccurredTime = DateTime.Now.ToString();

                //this.OccurredTime = DateTime.Now.ToString("yyyy-MM-dd, HH:mm");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public enum StopOptionEnum
    {
        UNDEFINED,
        STOP_AFTER_CASSETTE,
        STOP_AFTER_WAFERLOAD,
        EVERY_STOP_BEFORE_PROBING,
        EVERY_STOP_AFTER_PROBING,
        STOP_BEFORE_PROBING,
        STOP_AFTER_PROBING,
        STOP_BEFORE_RETEST,
    }

    public class ReasonOfStopOption
    {
        private StopOptionEnum _Reason;
        public StopOptionEnum Reason
        {
            get { return _Reason; }
            set { _Reason = value; }
        }

        private bool _isStop;
        public bool IsStop
        {
            get { return _isStop; }
            set { _isStop = value; }
        }

        public ReasonOfStopOption()
        {
            _isStop = false;
        }
    }
    public enum ModuleEnum
    {
        Undefined,
        AirBlowChuckCleaning,
        AirBlowTempControl,
        AirBlowWaferCleaning,
        AirCooling,
        AutoTilt,
        CardChange,
        E84,
        //ENV,
        EVENT,
        GEM,
        GPIB,
        Loader,
        LoaderController,
        Lot,
        MarkAlign,
        NeedelClean,
        PinAlign,
        PMI,
        PolishWafer,
        Probing,
        RFID,
        Soaking,
        SequenceRunner,
        Temperature,
        WaferAlign,
        WaferTransfer,
        NeedleBrush,
        NeedleClean,
        Foup,
        TouchSensor,
        TCPIP,
        Bonder, // 251013 sebas
        FDWaferAlign // 251013 sebas
    }

   
    [DataContract]
    public enum EnumLoaderEmergency
    {
        [EnumMember]
        EMG = 0,
        [EnumMember]
        AIR = 1,
        [EnumMember]
        VACUUM = 2,
    }
    [DataContract]
    public enum ModuleStateEnum
    {
        [EnumMember]
        UNDEFINED = 0x0000,
        [EnumMember]
        INIT = 0x0001,
        [EnumMember]
        IDLE = 0x0002,
        [EnumMember]
        RUNNING = 0x0003,
        [EnumMember]
        PENDING = 0x0004,
        [EnumMember]
        SUSPENDED = 0x0005,
        [EnumMember]
        ABORT = 0x0006,
        [EnumMember]
        DONE = 0x0007,
        [EnumMember]
        ERROR = 0x0008,
        [EnumMember]
        PAUSED = 0x0009,
        [EnumMember]
        RECOVERY = 0x0010,
        [EnumMember]
        RESUMMING = 0x0103,
        [EnumMember]
        PAUSING = 0x0109
    }


    [DataContract]
    public enum AlignTypeEnum
    {
        [EnumMember]
        Wafer = 0,
        [EnumMember]
        Pin,
        [EnumMember]
        Mark
    }
    [DataContract]
    public enum AlignStateEnum
    {
        [EnumMember]
        IDLE,
        [EnumMember]
        FAIL,
        [EnumMember]
        DONE
    }

    public enum EnumModuleForcedState
    {
        ForcedDone = 0,
        ForcedRunningAndDone = 1,
        Normal = 2,
    }

    public enum EnumTemperatureState
    {
        [EnumMember]
        IDLE = 0,
        [EnumMember]
        WaitForRechead,
        [EnumMember]
        Monitoring,
        [EnumMember]
        SetToTemp,
        [EnumMember]
        Done,
        [EnumMember]
        Error,
        [EnumMember]
        DewPoint,
        [EnumMember]
        WaitForChiller,
        [EnumMember]
        WaitForCondition,
        [EnumMember]
        Chilling,
        [EnumMember]
        Heatting,
        [EnumMember]
        AirPurge,
        [EnumMember]
        AirPurgeIdle,
        [EnumMember]
        ConnectError,
        [EnumMember]
        AbortChiller,
        [EnumMember]
        PauseDiffTemp,
        [EnumMember]
        TempInRange,
        [EnumMember]
        Inactivated,
        [EnumMember]
        Activated
    }
}

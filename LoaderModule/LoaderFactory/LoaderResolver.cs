using System;
using System.Collections.Generic;

using Autofac;
using Autofac.Core;
using Cognex.Controls;
using Command;
using FileSystem;
using GEMModule;
using GPLoaderRouter;
using InternalModule;
using IOMappingsObject;
using LoaderBase;
using LoaderBase.AttachModules.ModuleInterfaces;
using LoaderBase.Communication;
using LoaderBase.FactoryModules.ViewModelModule;
using LoaderCommunicationModule;
using LoaderCore;
using LoaderCore.DeviceManager;
using LoaderCore.ProxyModules;
using LoaderMaster;
using LoaderServiceBase;
using LoaderViewModelModule;
using LogModule;
using PAModule;
using ParameterManager;
using ProbeEvent;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Event;
using ProberInterfaces.PreAligner;
using ProberInterfaces.Template;
using ProberInterfaces.Foup;
using ViewModelModule;
using ProberInterfaces.DialogControl;
using LoaderPnpManagerModule;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.Loader;
using MetroDialogModule;
using ProberInterfaces.Focus;
using FocusingManager;
using FoupModules;
using FoupOP;
using ProberInterfaces.LoaderController;
using LoaderController.GPController;
using LoaderServiceClientModules;
using LoaderServiceClientModules.Inspection;
using LoaderServiceClientModules.TempController;
using ProberInterfaces.Temperature;
using LoaderServiceClientModules.ParamManager;
using LoaderServiceClientModules.CoordManager;
using ProberInterfaces.PolishWafer;
using LoaderServiceClientModules.PolishWaferModule;
using LoaderServiceClientModules.PMIModule;
using ProberInterfaces.PMI;
using LoaderLogModule;
using LoaderBase.LoaderLog;
using LoaderServiceClientModules.VisionManager;
using LoaderServiceClientModules.SoakingModule;
using ProberInterfaces.PinAlign;
using LoaderServiceClientModules.PinAligner;
using LoaderServiceClientModules.TemplateManager;
using MetroDialogInterfaces;
using EnvControlModule;
using NotifyModule;
using LoaderLogSpliterModule;
using LoaderDoorDialogServiceProvider;

using LoaderParkingDialogServiceProvider;
using PIVManagerModule;
using LoaderServiceClientModules.TCPIPModule;
using ProberInterfaces.ResultMap;
using LoaderServiceClientModules.ResultMap;
using E84;
using ProberInterfaces.E84.ProberInterfaces;
using ProberInterfaces.Retest;
using LoaderServiceClientModules.Retest;
using SequenceEngine;
using LoaderResultMapUpDown;
using LoaderBase.LoaderResultMapUpDown;
using ProberInterfaces.ODTP;
using LoaderODTPUpDown;
using EnvMonitoring;
using ProberInterfaces.Temperature.EnvMonitoring;
using SignalTowerModule;
using ProberInterfaces.SignalTower;
using LoaderCore.TransferManager;

namespace LoaderFactory
{
    public class LoaderResolver : ILoaderResolver
    {
        private static bool isConfiureDependecies = false;
        public static IContainer Container { get; private set; }

        public List<object> RegisteInstances = new List<object>();

        public IContainer ConfigureDependencies()
        {
            try
            {
                if (isConfiureDependecies == false)
                {
                    ContainerBuilder builder = new ContainerBuilder();

                    //=> main module
                    builder.RegisterType<LoaderModule>().As<ILoaderModule>().SingleInstance();

                    //=> External
                    builder.RegisterType<IOManagerProxy>().As<IIOManagerProxy>().SingleInstance();
                    builder.RegisterType<LightProxy>().As<ILightProxy>().SingleInstance();
                    builder.RegisterType<VisionManagerProxy>().As<IVisionManagerProxy>().SingleInstance();
                    builder.RegisterType<MotionManagerProxy>().As<IMotionManagerProxy>().SingleInstance();

                    //=> Internal
                    builder.RegisterType<LoaderMove>().As<ILoaderMove>().SingleInstance();
                    builder.RegisterType<ModuleManager>().As<IModuleManager>().SingleInstance();
                    builder.RegisterType<LoaderSequencer>().As<ILoaderSequencer>().SingleInstance();
                    builder.RegisterType<WaferTransferRemoteService>().As<IWaferTransferRemoteService>().SingleInstance();
                    builder.RegisterType<CardTransferRemoteService>().As<ICardTransferRemoteService>().SingleInstance();
                    builder.RegisterType<OCRRemoteService>().As<IOCRRemoteService>().SingleInstance();

                    builder.RegisterType<GPLoaderCommandEmulator>().Named<IGPLoaderCommands>("CmdEmul");

                    //builder.RegisterType<GPLoaderCommandEmulator>().Named<IGPLoaderCommands>("CmdEmul");
                    builder.RegisterType<CognexProcessManager>().As<ICognexProcessManager>().SingleInstance();

                    #region // Utility module
                    builder.RegisterType<FileManager>().As<IFileManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<ParamManager>().As<IParamManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<ViewModelManager>().As<IViewModelManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<ProcessModulePakage>().As<ILoaderProcessModulePakage>().SingleInstance();

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        builder.RegisterType<GPDeviceManager>().As<IDeviceManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    }

                    #endregion

                    builder.RegisterType<LoaderCommunicationManager>().As<ILoaderCommunicationManager>().SingleInstance();
                    builder.RegisterType<LoaderLogManagerModule>().As<ILoaderLogManagerModule>().SingleInstance();
                    builder.RegisterType<LoaderLogSplitManager>().As<ILoaderLogSplitManager>().SingleInstance();
                    builder.RegisterType<FocusManager>().As<IFocusManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<EnvControlManager>().As<IEnvControlManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<NotifyManager>().As<INotifyManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<LoaderResultMapUpDownMng>().As<ILoaderResultMapUpDownMng>().SingleInstance();
                    builder.RegisterType<LoaderODTPUpDownMng>().As<ILoaderODTPManager>().SingleInstance();
                    
                    Container = builder.Build();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Container;
        }
        public IContainer RemoteModeConfigDependencies()
        {
            try
            {
                if (isConfiureDependecies == false)
                {
                    ContainerBuilder builder = new ContainerBuilder();


                    //=> main module
                    builder.RegisterType<LoaderModule>().As<ILoaderModule>().SingleInstance(); /*RegisteInstances.Add(ILoaderModule);*/

                    //=> External
                    builder.RegisterType<RemoteIOProxy>()
                        .As<IIOManagerProxy>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<RemoteLightProxy>().As<ILightProxy>().As<IFactoryModule>().SingleInstance();
                    builder.RegisterType<RemoteVisionProxy>().As<IVisionManagerProxy>().As<IFactoryModule>().SingleInstance();
                    // builder.RegisterType<VisionManagerProxy>().As<IVisionManagerProxy>().SingleInstance();
                    builder.RegisterType<RemoteMotionProxy>().As<IMotionManagerProxy>().As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<StageSupervisorServiceClient>().As<IStageSupervisor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);


                    //=> Internal
                    builder.RegisterType<LoaderMove>().As<ILoaderMove>().SingleInstance();
                    builder.RegisterType<ModuleManager>().As<IModuleManager>().SingleInstance();
                    builder.RegisterType<LoaderSequencer>().As<ILoaderSequencer>().SingleInstance();
                    builder.RegisterType<WaferTransferRemoteService>().As<IWaferTransferRemoteService>().SingleInstance();
                    builder.RegisterType<CardTransferRemoteService>().As<ICardTransferRemoteService>().SingleInstance();
                    builder.RegisterType<OCRRemoteService>().As<IOCRRemoteService>().SingleInstance();
                    builder.RegisterType<CognexProcessManager>().As<ICognexProcessManager>().SingleInstance();
                    builder.RegisterType<LoaderSupervisor>().As<ILoaderSupervisor>().SingleInstance();
                    builder.RegisterType<LoaderPnpManager>().As<IPnpManager>().SingleInstance().OnActivated(ModuleConstructorEvent); ;

                    #region // Utility module
                    //builder.RegisterType<ViewModelManager>().As<IViewModelManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance();
                    builder.RegisterType<LoaderViewModelManager>().As<IViewModelManager>().As<IFactoryModule>().As<ILoaderViewModelManager>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<IOMappings>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<FileManager>().As<IFileManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<FileManagerServiceClient>().As<ILoaderFileManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ParamManager>().As<IParamManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<GPLoader>().As<IGPLoaderCommands>();
                    builder.RegisterType<GPLoader>().As<IGPLoader>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<GPLoaderCommandEmulator>().Named<IGPLoaderCommands>("CmdEmul");
                    //builder.RegisterType<IOMappings>().As<IIOMappingsParameter>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent); 
                    builder.RegisterType<LoaderMapSlicer>().As<ILoaderMapSlicer>().SingleInstance();
                    builder.RegisterType<GP_ProcessModulePakage>().As<ILoaderProcessModulePakage>().SingleInstance();
                    builder.RegisterType<CommandManager>().As<ICommandManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    #endregion

                    builder.RegisterType<PAModuleManager>().As<IPAManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<GPDeviceManager>().As<IDeviceManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<LoaderCommunicationManager>().As<ILoaderCommunicationManager>().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<LoaderLogManagerModule>().As<ILoaderLogManagerModule>().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<LoaderLogSplitManager>().As<ILoaderLogSplitManager>().SingleInstance().OnActivated(ModuleConstructorEvent); ;
                    builder.RegisterType<LoaderResultMapUpDownMng>().As<ILoaderResultMapUpDownMng>().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<LoaderODTPUpDownMng>().As<ILoaderODTPManager>().SingleInstance().OnActivated(ModuleConstructorEvent);
                    
                    //builder.RegisterType<LoaderViewModelManager>().As<ILoaderViewModelManager>().SingleInstance();

                    //Dialog
                    //builder.RegisterType<WaitCancelDialogService>().As<IWaitCancelDialogService>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<MetroDialogManager>().As<IMetroDialogManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<LoaderDoorDisplayService>().As<ILoaderDoorDisplayDialogService>().As<ILoaderDoorDisplayDialogService>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<LoaderParkingDisplayService>().As<ILoaderParkingDisplayDialogService>().As<ILoaderParkingDisplayDialogService>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<LoaderParkingDisplayService>().As<ILoaderParkingDisplayDialogService>().As<ILoaderParkingDisplayDialogService>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    //builder.RegisterType<GPIB>().As<IGPIB>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<GEM>().As<IGEMModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<Internal>().As<IInternal>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<EventManager>().As<IEventManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                    .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<EventExecutor>().As<IEventExecutor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TemplateManager.TemplateManager>().As<ITemplateManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<GP_LoaderController>().As<ILoaderController>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<RemoteIOManagerProxy>().As<IIOManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);


                    //ServiceClient
                    builder.RegisterType<ManualContactModuleServiceClient>().As<IManualContact>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<ProbingServiceClient>().As<IProbingModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                         .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<InspectionModuleServiceClient>().As<IInspection>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<MotionManagerServiceClient>().As<IMotionManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                         .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<TempControllerServiceClient>().As<ITempController>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<WaferAlignerServiceClient>().As<IWaferAligner>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<PolishWaferModuleServiceClient>().As<IPolishWaferModule>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<RetestModuleServiceClient>().As<IRetestModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<LotOPModuleServiceClient>().As<ILotOPModule>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<SoakingModuleServiceClient>().As<ISoakingModule>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);


                    builder.RegisterType<PMIModuleServiceClient>().As<IPMIModule>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ResultMapManagerServiceClient>().As<IResultMapManager>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ParamManagerServiceClient>().As<ILoaderParamManager>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance();

                    builder.RegisterType<PinAlignerModuleServiceClient>().As<IPinAligner>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance();

                    //builder.RegisterType<FoupModule>().As<IFoupModule>().As<IFactoryModule>()
                    //    .InstancePerLifetimeScope().SingleInstance()
                    //    .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<CoordManagerServiceClient>().As<ICoordinateManager>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<FoupOpModule>().As<IFoupOpModule>().As<IFactoryModule>()
                        .InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<FocusManager>().As<IFocusManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<FoupIOStates>().As<IFoupIOStates>().As<IFactoryModule>()
                        .InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<VisionManagerServiceClient>().As<IVisionManager>()
                        .As<IFactoryModule>()
                        .InstancePerLifetimeScope()
                        .SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TemplateManagerServiceClient>().As<ITemplateManager>()
                       .As<IFactoryModule>()
                       .InstancePerLifetimeScope()
                       .SingleInstance()
                       .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<EnvControlManager>().As<IEnvControlManager>().
                        As<IFactoryModule>().
                        InstancePerLifetimeScope().
                        SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<NotifyManager>().As<INotifyManager>().
                        As<IFactoryModule>().
                        InstancePerLifetimeScope().
                        SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<CardChangeSupervisor.CardChangeSupervisor>().As<ICardChangeSupervisor>().
                       As<IFactoryModule>().                       
                       SingleInstance()
                       .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TCPIPModuleServiceClient>().As<ITCPIP>().
                        As<IFactoryModule>().
                        InstancePerLifetimeScope().
                        SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<PIVManager>().As<IPIVManager>().As<IFactoryModule>().
                       InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<E84Module>().As<IE84Module>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                                .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<SignalTowerManager>().As<ISignalTowerManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().
                        OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<SequenceEngineManager>().As<ISequenceEngineManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<EnvMonitoringManager>().As<IEnvMonitoringManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                                .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<GPTransferManager>().As<ITransferManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<WaferChangeSupervisor.WaferChangeSupervisor>().As<IWaferChangeSupervisor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    Container = builder.Build();

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Container;
        }



        private static List<ProberInterfaces.IModule> factoryModules = new List<ProberInterfaces.IModule>();
        public static List<ProberInterfaces.IModule> GetFactoryModules()
        {
            return factoryModules;
        }


        private static void ModuleConstructorEvent(IActivatedEventArgs<IFactoryModule> obj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            string var = obj.Instance.GetType().FullName;
            //string root;

            //// For Debugging
            //Stack.Add(obj);

            //if (Stack.Count > 1)
            //{
            //    root = Stack[Stack.Count - 2].Instance.GetType().FullName;
            //}
            //else
            //{
            //    root = "this";
            //}

            //LoggerManager.Debug($"[Autofac] Module Name = {var} Start, Root Module = {root} Count({Stack.Count})");
            if (obj.Instance is ProberInterfaces.IModule)
            {
                if (factoryModules.Find(module => module.GetType() == obj.Instance.GetType()) == null)
                    factoryModules.Add((ProberInterfaces.IModule)obj.Instance);
            }

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

            // Regist Event 
            if (obj.Instance is IProbeEventSubscriber)
            {
                try
                {
                    retval = (obj.Instance as IProbeEventSubscriber).RegistEventSubscribe();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"(obj.Instance as IProbeEventSubscriber).RegistEventSubscribe() Failed");
                    }

                    LoggerManager.Debug($"[ModuleResolver] [ModuleConstructorEvent] RegistEventSubscribe : {obj.Instance} - {retval}");
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);

                    throw err;
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
    }
}

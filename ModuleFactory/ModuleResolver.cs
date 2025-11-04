namespace ModuleFactory
{
    using AirBlowModule;
    using AirCoolingMoodule;
    using Autofac;
    using Autofac.Core;
    using AutoLightModule;
    using AutoTiltModule;
    using CameraChannelManager;
    using CardChange;
    using ClientModule;
    using Command;
    using CoordinateSystem;
    using CylinderManagerModule;
    using DeviceModule;
    using DeviceUpDownModule;
    using DisplayPortDialogVM;
    using E84;
    using ElmoManager;
    using EnvModule;
    using EnvControlModule;
    using EnvMonitoring;
    using FileSystem;
    using FocusingManager;
    using ForceMeasureModule;
    using FoupModules;
    using FoupOP;
    using GEMModule;
    using GPCardAlign;
    using GPIBModule;
    using GPLoaderRouter;
    using InspectionModules;
    using InternalModule;
    using IOManagerModule;
    using LampModule;
    using LightManager;
    using LoaderController;
    using LoaderController.GPController;
    using LoaderCore.DeviceManager;
    using LoaderOP;
    using LoaderRemoteMediatorModule;
    using LogModule;
    using LotOP;
    using ManualContact;
    using MarkAlign;
    using MetroDialogInterfaces;
    using MetroDialogModule;
    using ModuleManager;
    using ModuleStatistics;
    using MonitoringModule;
    using NeedleBrushModule;
    using NeedleCleanerModule;
    using NotifyModule;
    using ODTPModule;
    using PadModule;
    using ParameterManager;
    using PinAlign;
    using PIVManagerModule;
    using PMIModule;
    using PnpServiceManager;
    using PolishWaferModule;
    using ProbeEvent;
    using ProbeMotion;
    using ProberCore;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.AirBlow;
    using ProberInterfaces.AirCooling;
    using ProberInterfaces.AutoTilt;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Command;
    using ProberInterfaces.Communication.Tester;
    using ProberInterfaces.Device;
    using ProberInterfaces.DialogControl;
    using ProberInterfaces.E84.ProberInterfaces;
    using ProberInterfaces.EnvControl;
    using ProberInterfaces.Event;
    using ProberInterfaces.Focus;
    using ProberInterfaces.Foup;
    using ProberInterfaces.Loader;
    using ProberInterfaces.LoaderController;
    using ProberInterfaces.MarkAlign;
    using ProberInterfaces.NeedleBrush;
    using ProberInterfaces.NeedleClean;
    using ProberInterfaces.ODTP;
    using ProberInterfaces.Pad;
    using ProberInterfaces.PinAlign;
    using ProberInterfaces.PMI;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.ResultMap;
    using ProberInterfaces.Retest;
    using ProberInterfaces.SequenceRunner;
    using ProberInterfaces.Service;
    using ProberInterfaces.State;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Temperature.EnvMonitoring;    
    using ProberInterfaces.Template;
    using ProberInterfaces.TouchSensor;
    using ProberInterfaces.WaferTransfer;
    using ProberVision;
    using ProbingModule;
    //using ActionModule;
    using ProbingSequenceManager;
    using RetestModule;
    using SequenceEngine;
    using SequenceRunnerModule;
    using Soaking;
    using StageCommunicationModule;
    using StageModule;
    using System;
    using System.Collections.Generic;
    using SystemState;
    using TCPIP;
    using TempDisplayDialogServiceProvider;
    using Temperature;
    using TemplateManager;
    using TesterCommunicationModule;
    using TouchSensorBaseRegisterModule;
    using TouchSensorOffsetRegisterModule;
    using TouchSensorPadRefRegisterModule;
    using TouchSensorRegisterModule;
    using ViewModelModule;
    using WaferAlign;
    using WaferTransfer;
    using ProberInterfaces.Bonder;
    using Bonder;
    using FDWaferAlign;
    using ProberInterfaces.FDAlign;
    using BonderSupervisor;

    public static class ModuleResolver
    {
        public static IContainer Container;

        public static bool isGpSystem = false;
        private static bool isConfiureDependecies = false;

        private static bool IsInfo = false;

        public static IContainer ConfigureDependencies()
        {
            try
            {
                if (isConfiureDependecies == false)
                {
                    var builder = new ContainerBuilder();

                    //builder.RegisterType<MotionProvider>().As<IMotionProvider>().InstancePerLifetimeScope().SingleInstance();
                    //builder.RegisterType<DutObject>().As<IDutObject>().InstancePerLifetimeScope().SingleInstance();
                    //builder.RegisterType<ProbeCard>().As<IProbeCard>().InstancePerLifetimeScope().SingleInstance();
                    //builder.RegisterType<FoupController>().As<IFoupController>().InstancePerLifetimeScope().SingleInstance();

                    // MainWindow에서 InitModule을 호출해주는 대상은 OnActivated를 사용하면 않된다.
                    builder.RegisterType<ProberStation>().As<IProberStation>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance();
                    builder.RegisterType<ViewModelManager>().As<IViewModelManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance();
                    //builder.RegisterType<INIFileManager>().As<IINIFileManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance();


                    builder.RegisterType<SequenceEngineManager>().As<ISequenceEngineManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        builder.RegisterType<LoaderController>().As<ILoaderController>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                           .OnActivated(ModuleConstructorEvent);
                    }
                    else
                    {
                        builder.RegisterType<GP_LoaderController>().As<ILoaderController>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                           .OnActivated(ModuleConstructorEvent);
                    }

                    builder.RegisterType<PMASManager>().As<IPMASManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<MotionManager>().As<IMotionManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<CoordinateManager>().As<ICoordinateManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<StageSupervisor>().As<IStageSupervisor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<FileManager>().As<IFileManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<VisionManager>().As<IVisionManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<LightAdmin>().As<ILightAdmin>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<CameraChannelAdmin>().As<ICameraChannelAdmin>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<IOManager>().As<IIOManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    //builder.RegisterType<IOService>().As<IIOService>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                    //    .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<MarkAligner>().As<IMarkAligner>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<WaferAligner>().As<IWaferAligner>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<PinAligner>().As<IPinAligner>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<PadRegistrationAssistanceModule>().As<IPadRegist>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<AutoLightAdvisor>().As<IAutoLightAdvisor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TempController>().As<ITempController>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<GEM>().As<IGEMModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ProbingSequenceModule>().As<IProbingSequenceModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TesterCommunicationManager>().As<ITesterCommunicationManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<GPIB>().As<IGPIB>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TCPIPModule>().As<ITCPIP>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<LotOPModule>().As<ILotOPModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<LoaderOPModule>().As<ILoaderOPModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<Probing>().As<IProbingModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ManualContactModule>().As<IManualContact>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<CardChangeModule>().As<ICardChangeModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<SoakingModule>().As<ISoakingModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<WaferTransferModule>().As<IWaferTransferModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<EventManager>().As<IEventManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<EventExecutor>().As<IEventExecutor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<Internal>().As<IInternal>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<Notify>().As<INotify>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);


                    builder.RegisterType<DisplayPortDialogViewModel>().As<IDisplayPortDialog>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);


                    builder.RegisterType<TempDisplayDialogService>().As<ITempDisplayDialogService>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);


                    builder.RegisterType<AutoTiltModule>().As<IAutoTiltModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ParamManager>().As<IParamManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<PolishWaferModule>().As<IPolishWaferModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<AirBlowChuckCleaningModule>().As<IAirBlowChuckCleaningModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<AirBlowWaferCleaningModule>().As<IAirBlowWaferCleaningModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<AirBlowTempControlModule>().As<IAirBlowTempControlModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<AirCoolingModule>().As<IAirCoolingModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        builder.RegisterType<E84Module>().As<IE84Module>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    }

                    //builder.RegisterType<RFIDModule>().As<IRFIDModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                    //    .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<FoupOpModule>().As<IFoupOpModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<CommandManager>().As<ICommandManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<LampManager>().As<ILampManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ModuleUpdater>().As<IModuleUpdater>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<RemoteServiceModule>().As<IRemoteServiceModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<MonitoringManager>().As<IMonitoringManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<CylinderManager>().As<ICylinderManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<StatisticsManager>().As<IStatisticsManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<PnpManager>().As<IPnpManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<NeedleCleanModule>().As<INeedleCleanModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<PMIModule>().As<IPMIModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<SequenceRunner>().As<ISequenceRunner>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TemplateManager>().As<ITemplateManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<FocusManager>().As<IFocusManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<InspectionModule>().As<IInspection>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<MetroDialogManager>().As<IMetroDialogManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        builder.RegisterType<GPLoader>().As<IGPLoader>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    }

                    builder.RegisterType<DeviceUpDownManager>().As<IDeviceUpDownManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        builder.RegisterType<GPDeviceManager>().As<IDeviceManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    }

                    builder.RegisterType<StageCommunicationManager>().As<IStageCommunicationManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<NeedleBrush>().As<INeedleBrushModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ResultMapModule.ResultMapManager>().As<IResultMapManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ODTPManager>().As<IODTPManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<LoaderRemoteMediator>().As<ILoaderRemoteMediator>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        builder.RegisterType<GPCardAligner>().As<IGPCardAligner>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    }

                    builder.RegisterType<FoupIOStates>().As<IFoupIOStates>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ForceMeasure>().As<IForceMeasure>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<EnvControlManager>().As<IEnvControlManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<NotifyManager>().As<INotifyManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<DeviceModule>().As<IDeviceModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<PIVManager>().As<IPIVManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<RetestModule>().As<IRetestModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<SystemStatusModule>().As<ISystemstatus>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                       .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TouchSensorSetup>().As<ITouchSensorTipSetupModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<TouchSensorBaseSetup>().As<ITouchSensorBaseSetupModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<TouchSensorPadRefSetup>().As<ITouchSensorPadRefSetupModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<TouchSensorOffsetSetup>().As<ITouchSensorCalcOffsetModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<EnvModule>().As<IEnvModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated(ModuleConstructorEvent);


                    builder.RegisterType<EnvMonitoringManager>().As<IEnvMonitoringManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                       .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<BonderModule>().As<IBonderModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);   // 251013 sebas

                    builder.RegisterType<FDWaferAligner>().As<IFDWaferAligner>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);   // 251013 sebas

                    builder.RegisterType<BonderSupervisor>().As<IBonderSupervisor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);   // 251013 sebas

                    Container = builder.Build();
                    isConfiureDependecies = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return Container;
        }


        private static object obj = new object();
        private static List<IActivatedEventArgs<IFactoryModule>> Stack = new List<IActivatedEventArgs<IFactoryModule>>();

        public static List<string> moduleFulllogs = new List<string>();
        private static Stack<string> moduleStack = new Stack<string>();
        private static List<string> moduleInvocationlogs = new List<string>();

        private static string GetIndentation()
        {
            return new string(' ', moduleStack.Count * 4);
        }

        public static void PrintModuleTree()
        {

            try
            {
                LoggerManager.Debug("Module Invocation Tree:");

                foreach (string log in moduleInvocationlogs)
                {
                    LoggerManager.Debug(log);
                }

                LoggerManager.Debug("Module Full Log :");

                foreach (string log in moduleFulllogs)
                {
                    LoggerManager.Debug(log);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ModuleConstructorEvent(IActivatedEventArgs<IFactoryModule> obj)
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                string indentation = GetIndentation();
                moduleInvocationlogs.Add($"{indentation}{obj.Instance.ToString()} (START) ->");
                moduleStack.Push(obj.Instance.ToString());

                retval = LoadSysParameter(obj);
                retval = LoadDevParameter(obj);
                retval = RegistEvent(obj);
                retval = InitializeModule(obj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                string poppedModule = moduleStack.Pop();
                string indentation = GetIndentation();
                moduleInvocationlogs.Add($"{indentation}{poppedModule} (END) ->");
            }
        }
        private static void AddModuleOrder(string msg)
        {

            try
            {
                string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH\\:mm\\:ss.fff");

                moduleFulllogs.Add($"{formattedDateTime} | {msg}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private static EventCodeEnum LoadSysParameter(IActivatedEventArgs<IFactoryModule> obj)
        {
            // Load System Parameter
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            if (obj.Instance is IHasSysParameterizable)
            {

                try
                {
                    DateTime data = DateTime.Now;

                    string msg = $"[S] {obj.Instance}, LoadSysParameter()";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    retval = (obj.Instance as IHasSysParameterizable).LoadSysParameter();
                    TimeSpan t = (DateTime.Now - data);

                    msg = $"[E] {obj.Instance}, LoadSysParameter(), Time = {t.TotalSeconds} sec";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[ERROR] {obj.Instance}, LoadSysParameter(), Error Code = {retval}");
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);

                    throw new Exception($"ModuleConstructorEvent : Occurred during LoadSysParameter in {obj.Instance} ");
                }
            }

            return retval;
        }
        private static EventCodeEnum LoadDevParameter(IActivatedEventArgs<IFactoryModule> obj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            // Load Device Parameter
            if (obj.Instance is IHasDevParameterizable)
            {
                try
                {
                    DateTime data = DateTime.Now;

                    string msg = $"[S] {obj.Instance}, LoadDevParameter()";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    TimeSpan t = (DateTime.Now - data);
                    retval = (obj.Instance as IHasDevParameterizable).LoadDevParameter();

                    msg = $"[E] {obj.Instance}, LoadDevParameter(), Time = {t.TotalSeconds} sec";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    data = DateTime.Now;

                    msg = $"[S] {obj.Instance}, InitDevParameter()";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    retval = (obj.Instance as IHasDevParameterizable).InitDevParameter();

                    msg = $"[E] {obj.Instance}, InitDevParameter(), Time = {t.TotalSeconds} sec";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[ERROR] {obj.Instance}, LoadDevParameter(), Error Code = {retval}");
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);

                    throw new Exception($"ModuleConstructorEvent : Occurred during LoadDevParameter in {obj.Instance} ");
                }
            }

            return retval;
        }
        private static EventCodeEnum RegistEvent(IActivatedEventArgs<IFactoryModule> obj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            // Regist Event 
            if (obj.Instance is IProbeEventSubscriber)
            {
                try
                {
                    string msg = $"[S] {obj.Instance}, RegistEventSubscribe()";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    retval = (obj.Instance as IProbeEventSubscriber).RegistEventSubscribe();

                    msg = $"[E] {obj.Instance}, RegistEventSubscribe()";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"(obj.Instance as IProbeEventSubscriber).RegistEventSubscribe() Failed");
                    }

                    LoggerManager.Debug($"[{nameof(ModuleResolver)}] [ModuleConstructorEvent] RegistEventSubscribe : {obj.Instance} - {retval}", isInfo: IsInfo);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);

                    throw err;
                }
            }

            return retval;
        }
        private static EventCodeEnum InitializeModule(IActivatedEventArgs<IFactoryModule> obj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            // Initialize Module
            if (obj.Instance is ProberInterfaces.IModule)
            {
                try
                {
                    DateTime data = DateTime.Now;

                    string msg = $"[S] {obj.Instance}, InitModule()";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    retval = (obj.Instance as ProberInterfaces.IModule).InitModule();
                    TimeSpan t = (DateTime.Now - data);

                    msg = $"[E] {obj.Instance}, InitModule(), Time = {t.TotalSeconds} sec";
                    LoggerManager.Debug(msg, isInfo: IsInfo);
                    AddModuleOrder(msg);

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[ERROR] {obj.Instance}, InitModule(), Error Code = {retval}");
                    }

                    //LoggerManager.Debug($"[ModuleResolver] [ModuleConstructorEvent] Initialize : {obj.Instance} - {retval} // {t.TotalSeconds}");
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);

                    throw new Exception($"ModuleConstructorEvent : Occurred during InitModule in {obj.Instance} ");
                }
            }

            return retval;
        }
    }
}


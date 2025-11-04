using Autofac;
using Autofac.Core;
using Command;
using FileSystem;
using LogModule;
using TCPIP;
using ProbeEvent;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Communication.Tester;
using ProberInterfaces.Event;
using SequenceEngine;
using System;
using System.Collections.Generic;
using TesterCommunicationModule;
using WaferAlign;
using StageModule;
using CoordinateSystem;
using MetroDialogInterfaces;
using MetroDialogModule;
using ProbingSequenceManager;
using LotOP;
using ProbingModule;
using ProberVision;
using ProberInterfaces.PnpSetup;
using PnpServiceManager;
using ProberInterfaces.Template;
using ViewModelModule;
using ResultMapModule;
using ProberInterfaces.ResultMap;
using ProberInterfaces.Temperature;
using Temperature;
using ParameterManager;

namespace ProberEmulator
{
    public static class ModuleResolver
    {
        public static IContainer Container;

        public static bool isGpSystem = false;
        private static bool isConfiureDependecies = false;
        public static IContainer ConfigureDependencies()
        {
            try
            {
                if (isConfiureDependecies == false)
                {
                    var builder = new ContainerBuilder();

                    builder.RegisterType<SequenceEngineManager>().As<ISequenceEngineManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<FileManager>().As<IFileManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TCPIPModule>().As<ITCPIP>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<EventManager>().As<IEventManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<EventExecutor>().As<IEventExecutor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<CommandManager>().As<ICommandManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TesterCommunicationManager>().As<ITesterCommunicationManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<WaferAligner>().As<IWaferAligner>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<StageSupervisor>().As<IStageSupervisor>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<CoordinateManager>().As<ICoordinateManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<MetroDialogManager>().As<IMetroDialogManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ProbingSequenceModule>().As<IProbingSequenceModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<Probing>().As<IProbingModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<LotOPModule>().As<ILotOPModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<VisionManager>().As<IVisionManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<PnpManager>().As<IPnpManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TemplateManager.TemplateManager>().As<ITemplateManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ViewModelManager>().As<IViewModelManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ResultMapModule.ResultMapManager>().As<IResultMapManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<TempController>().As<ITempController>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ParamManager>().As<IParamManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    builder.RegisterType<ResultMapManager>().As<IResultMapManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);

                    

                    // TODO : TEST
                    //SystemManager.SysteMode = SystemModeEnum.Multiple;

                    //if (SystemManager.SysteMode == SystemModeEnum.Single)
                    //{
                    //    builder.RegisterType<LoaderController.LoaderController>().As<ILoaderController>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                    //       .OnActivated(ModuleConstructorEvent);
                    //}
                    //else
                    //{
                    //    builder.RegisterType<GP_LoaderController>().As<ILoaderController>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                    //       .OnActivated(ModuleConstructorEvent);
                    //}

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

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ModuleConstructorEvent(IActivatedEventArgs<IFactoryModule> obj)
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                retval = LoadSysParameter(obj);
                retval = LoadDevParameter(obj);
                retval = RegistEvent(obj);
                retval = InitializeModule(obj);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
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
                    LoggerManager.Debug($"[S] {obj.Instance}, LoadSysParameter()");
                    retval = (obj.Instance as IHasSysParameterizable).LoadSysParameter();
                    TimeSpan t = (DateTime.Now - data);
                    LoggerManager.Debug($"[E] {obj.Instance}, LoadSysParameter(), Time = {t.TotalSeconds} sec");

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
                    LoggerManager.Debug($"[S] {obj.Instance}, LoadDevParameter()");
                    TimeSpan t = (DateTime.Now - data);
                    retval = (obj.Instance as IHasDevParameterizable).LoadDevParameter();
                    LoggerManager.Debug($"[E] {obj.Instance}, LoadDevParameter(), Time = {t.TotalSeconds} sec");

                    data = DateTime.Now;
                    LoggerManager.Debug($"[S] {obj.Instance}, InitDevParameter()");
                    retval = (obj.Instance as IHasDevParameterizable).InitDevParameter();
                    LoggerManager.Debug($"[E] {obj.Instance}, InitDevParameter(), Time = {t.TotalSeconds} sec");

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

        private static EventCodeEnum InitializeModule(IActivatedEventArgs<IFactoryModule> obj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            // Initialize Module
            if (obj.Instance is ProberInterfaces.IModule)
            {
                try
                {
                    DateTime data = DateTime.Now;
                    LoggerManager.Debug($"[S] {obj.Instance}, InitModule()");
                    retval = (obj.Instance as ProberInterfaces.IModule).InitModule();
                    TimeSpan t = (DateTime.Now - data);
                    LoggerManager.Debug($"[E] {obj.Instance}, InitModule(), Time = {t.TotalSeconds} sec");

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

        private static EventCodeEnum RegistEvent(IActivatedEventArgs<IFactoryModule> obj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
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

                    LoggerManager.Debug($"[{nameof(ModuleResolver)}] [ModuleConstructorEvent] RegistEventSubscribe : {obj.Instance} - {retval}");
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);

                    throw err;
                }
            }

            return retval;
        }
    }
}

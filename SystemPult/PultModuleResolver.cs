using System;
using Autofac;
using Autofac.Core;

using CameraChannelManager;
using ElmoManager;
using FileSystem;
using FoupOP;
using IOManagerModule;
using LightManager;
using LogModule;
using ParameterManager;
using ProbeMotion;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberVision;
using ViewModelModule;
using MetroDialogInterfaces;
using MetroDialogModule;
namespace SystemPult
{
    public static class PultModuleResolver
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
                    builder.RegisterType<ViewModelManager>().As<IViewModelManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance();

                    //builder.RegisterType<FileManager>().As<IFileManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                    //.OnActivated(ModuleConstructorEvent);

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
                    builder.RegisterType<ParamManager>().As<IParamManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<PMASManager>().As<IPMASManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<MotionManager>().As<IMotionManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<FoupOpModule>().As<IFoupOpModule>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
                        .OnActivated(ModuleConstructorEvent);
                    builder.RegisterType<MetroDialogManager>().As<IMetroDialogManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance()
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

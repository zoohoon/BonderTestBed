using System;
using System.Collections.Generic;
using Autofac;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System.Xml.Serialization;
using Newtonsoft.Json;
using LogModule;
using NotifyEventModule;
using LoaderBase;
using System.Threading;
using ProberInterfaces.Event;
using LoaderRecoveryControl;

namespace FoupModules.Service
{
    public class DirectFoupService : IDirectFoupService
    {
        public List<object> Nodes { get; set; }

        public IFoupModule FoupModule { get; set; }

        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public Autofac.IContainer Container => this.GetLoaderContainer();

        ILoaderModule loaderModule => this.GetLoaderContainer().Resolve<ILoaderModule>();

        public void SetCallback(IFoupServiceCallback callback, FoupServiceTypeEnum servtype)
        {
            try
            {
                //if(Extensions_IParam.ProberRunMode == RunMode.EMUL)
                //{
                //    FoupModule = new EmulFoupModule();
                //}
                //else
                //{
                //    FoupModule = new FoupModule();
                //}
                switch (servtype)
                {
                    case FoupServiceTypeEnum.Direct:
                        FoupModule = new FoupModule();
                        break;
                    case FoupServiceTypeEnum.WCF:
                        LoggerManager.Debug($"Invalid service type. Service type = {servtype}. Use default service.");
                        FoupModule = new FoupModule();
                        break;
                    case FoupServiceTypeEnum.EMUL:
                        FoupModule = new EmulFoupModule();
                        break;
                    default:
                        FoupModule = new FoupModule();
                        break;
                }
                FoupModule.SetCallback(callback);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum InitProcedures()
        {
            EventCodeEnum retVal;
            try
            {
                retVal = FoupModule.InitProcedures();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum FoupStateInit()
        {
            EventCodeEnum retVal;
            try
            {
                retVal = FoupModule.InitState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum Connect()
        {
            EventCodeEnum retVal;
            try
            {
                retVal = FoupModule.Connect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum Disconnect()
        {
            EventCodeEnum retVal;
            try
            {
                retVal = FoupModule.Disconnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum InitModule(int foupNumber, FoupSystemParam systemParam, FoupDeviceParam deviceParam, CassetteConfigurationParameter CassetteConfigurationParam)
        {
            EventCodeEnum retVal;
            try
            {
                //FoupModule = new FoupModule();
                retVal = FoupModule.InitModule(foupNumber, systemParam, deviceParam, CassetteConfigurationParam);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum Deinit()
        {
            EventCodeEnum retVal;
            try
            {

                retVal = FoupModule.Deinit();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public FoupModuleInfo GetFoupModuleInfo()
        {
            FoupModuleInfo info;
            try
            {

                info = FoupModule.GetFoupModuleInfo();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return info;
        }
        ILoaderSupervisor LoaderMaster = null;
        public EventCodeEnum SetCommand(FoupCommandBase command)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (command is FoupInitStateCommand)
                {
                    retVal = FoupModule.InitState();
                }
                else if (command is FoupDeviceSetupCommand)
                {
                    var cmd = command as FoupDeviceSetupCommand;
                    retVal = FoupModule.ChangeDevice(cmd.DeviceParam);
                }
                else if (command is FoupLoadCommand)
                {
                    retVal = FoupModule.Load();
                }
                else if (command is FoupUnloadCommand)
                {
                    LotStateEnum temp = LotStateEnum.Idle;
                    if (SystemManager.SysteMode == SystemModeEnum.Multiple && LoaderMaster == null)
                    {
                        LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                        temp = LoaderMaster.ActiveLotInfos[FoupModule.FoupIndex].State;
                    }
                                        
                    if (!FoupModule.IsLock)
                    {
                        retVal = FoupModule.Unload();
                    }
                    else
                    {
                        LoggerManager.Debug("Cassette cannot be Unloaded. Lock is True");
                    }

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        if (LoaderMaster.GetIsCassetteAutoUnloadAfterLot() == true)
                        {
                            if (temp != LotStateEnum.Abort &&
                                temp != LotStateEnum.Idle)
                            {
                                PIVInfo pivinfo = new PIVInfo(foupnumber: FoupModule.FoupIndex + 1);
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();
                            }
                        }
                    }
                }
                else if (command is FosB_LoadCommand)
                {
                    retVal = FoupModule.FosB_Load();


                }
                else if (command is FosB_UnloadCommand)
                {
                    if (!FoupModule.IsLock)
                    {
                        retVal = FoupModule.FosB_Unload();
                    }
                    else
                    {
                        LoggerManager.Debug("Cassette cannot be Unloaded. Lock is True");
                    }

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        if (LoaderMaster == null)
                        {
                            LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                        }

                        if (LoaderMaster.GetIsCassetteAutoUnloadAfterLot() == true)
                        {
                            PIVInfo pivinfo = new PIVInfo(foupnumber: FoupModule.FoupIndex + 1);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                    }
                }
                else if (command is FoupNormalUnloadCommand)
                {
                    retVal = FoupModule.Unload();
                }
                else if (command is FoupCoverUpCommand)
                {
                    retVal = FoupModule.CoverUp();
                }
                else if (command is FoupCoverDownCommand)
                {
                    retVal = FoupModule.CoverDown();
                }
                else if (command is FoupDockingPlateLockCommand)
                {
                    retVal = FoupModule.DockingPlateLock();
                }
                else if (command is FoupDockingPlateUnlockCommand)
                {
                    retVal = FoupModule.DockingPlateUnlock();
                }
                else if (command is FoupDockingPortInCommand)
                {
                    retVal = FoupModule.DockingPortIn();
                }
                else if (command is FoupDockingPortOutCommand)
                {
                    retVal = FoupModule.DockingPortOut();
                }
                else if (command is FoupDockingPort40InCommand)
                {
                    retVal = FoupModule.DockingPort40In();
                }
                else if (command is FoupDockingPort40OutCommand)
                {
                    retVal = FoupModule.DockingPort40Out();
                }
                else if (command is FoupCassetteOpenerLockCommand)
                {
                    retVal = FoupModule.CassetteOpenerLock();
                }
                else if (command is FoupCassetteOpenerUnlockCommand)
                {
                    retVal = FoupModule.CassetteOpenerUnlock();
                }
                else if (command is FoupTiltUpCommand)
                {
                    retVal = FoupModule.FoupTiltUp();
                }
                else if (command is FoupTiltDownCommand)
                {
                    retVal = FoupModule.FoupTiltDown();
                }
                else if (command is FoupRecoveryFastBackwardCommand)
                {
                    retVal = FoupModule.ProcManager.FastBackwardRun();
                }
                else if (command is FoupRecoveryFastForwardCommand)
                {
                    retVal = FoupModule.ProcManager.FastForwardRun();
                }
                else if (command is FoupRecoveryNextCommand)
                {
                    retVal = FoupModule.ProcManager.NextRun();
                }
                else if (command is FoupRecoveryReverseCommand)
                {
                    retVal = FoupModule.ProcManager.ReverseRun();
                }
                else if (command is FoupRecoveryPreviousCommand)
                {
                    retVal = FoupModule.ProcManager.PreviousRun();
                }

                if (command is FosB_LoadCommand || command is FosB_UnloadCommand)
                {
                    FoupModule.UpdateFosBFoupState();
                }
                else
                {
                    FoupModule.UpdateFoupState();
                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        FoupModule.BroadcastFoupStateAsync();
                    }
                }
                
                //foup module?? ??? ??.
                //if (FoupModule.ModuleState.State == FoupStateEnum.ERROR)
                //{
                //    FoupModule.FoupModuleReset();
                //    LoaderRecoveryControlVM.Show(Container, loaderModule.ResonOfError, loaderModule.ErrorDetails);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetLoaderState(ModuleStateEnum state)
        {
            FoupModule.SetLoaderState(state);
        }

        public void SetLotState(ModuleStateEnum state)
        {
            FoupModule.SetLotState(state);
        }

        public EventCodeEnum MonitorForWaferOutSensor(bool value)
        {
            return FoupModule.MonitorForWaferOutSensor(value);
        }

        public ICylinderManager GetFoupCylinderManager()
        {
            return FoupModule.CylinderManager;
        }

        public IFoupProcedureManager GetFoupProcedureManager()
        {
            return FoupModule.ProcManager;
        }

        public void ChangeState(FoupStateEnum state)
        {
            FoupModule.ChangeState(state);
        }

        public FoupIOMappings GetFoupIOMap()
        {
            return FoupModule.IOManager.IOMap;
        }

        public EventCodeEnum CassetteTypeAvailable(CassetteTypeEnum cassetteType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = FoupModule.CassetteTypeAvailable(cassetteType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ValidationCassetteAvailable(out string msg, CassetteTypeEnum cassetteType = CassetteTypeEnum.UNDEFINED)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            msg = "";
            CassetteTypeEnum type = CassetteTypeEnum.UNDEFINED;
            try
            {
                if (cassetteType == CassetteTypeEnum.UNDEFINED)
                {
                    type = GetCassetteType();
                }
                else 
                {
                    type = cassetteType;
                }

                retVal = FoupModule.CassetteTypeAvailable(type);
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = FoupModule.ValidationCassetteAvailable(type, out msg);
                }
                else 
                {
                    retVal = EventCodeEnum.PARAM_ERROR;
                    msg = "Feature usage is not supported. This means the parameter settings are not configured.";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public CassetteTypeEnum GetCassetteType()
        {
            CassetteTypeEnum cassetteTypeEnum = CassetteTypeEnum.FOUP_25;
            try
            {
                cassetteTypeEnum = FoupModule.GetCassetteType();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return cassetteTypeEnum;
        }

        public EventCodeEnum SetDevice(FoupDeviceParam devParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = FoupModule.ChangeDevice(devParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetLock(bool isLock)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            FoupModule.IsLock = isLock;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

}

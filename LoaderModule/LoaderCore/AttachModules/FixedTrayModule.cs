using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;
using LoaderParameters.Data;
using ProberInterfaces.Loader;
using LoaderCore.DeviceManager;
using System.ComponentModel;
using System.Threading;
using ProberInterfaces.Event;

namespace LoaderCore
{
    internal class FixedTrayModule : AttachedModuleBase, IFixedTrayModule
    {
        

        private IOPortDescripter<bool> DIWAFERONMODULE;
        private IOPortDescripter<bool> DI6INCHWAFERONMODULE;
        private IOPortDescripter<bool> DI8INCHWAFERONMODULE;

        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.FIXEDTRAY;

        public FixedTrayDefinition Definition { get; set; }

        public FixedTrayDevice Device { get; set; }
        public WaferHolder Holder { get; set; }
        public ReservationInfo ReservationInfo { get; set; }
        public bool Enable { get; set; }

        public bool CanUseBuffer { get; set; }

        public TransferObject GetSourceDeviceInfo()
        {
            TransferObject retval = null;

            try
            {
                IDeviceManager DeivceManager = Container.Resolve<IDeviceManager>();

                var dev = DeivceManager as GPDeviceManager;

                if (dev != null)
                {
                    retval = dev.GetDeviceInfo(this);
                }

                retval = Device?.AllocateDeviceInfo ?? null;

                if (retval != null && DI6INCHWAFERONMODULE != null && DI8INCHWAFERONMODULE != null)
                {
                    ValidateWaferSize();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public UpdateHolderDelegate HolderStatusChanged { get; set; }
        
        public EventCodeEnum SetDefinition(FixedTrayDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = true;
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                DIWAFERONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DIWAFERONMODULE.Value);

                DI6INCHWAFERONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DI6INCHWAFERONMODULE.Value);

                DI8INCHWAFERONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DI8INCHWAFERONMODULE.Value);

                CanUseBuffer = definition.CanUseBuffer.Value;

                Holder = new WaferHolder();
                Holder.SetOwner(this);

                //RecoveryWaferStatus();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetDevice(FixedTrayDevice device)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Device = device;

                if (string.IsNullOrEmpty(device.Label.Value) == false)
                    this.ID = ModuleID.Create(ID.ModuleType, ID.Index, device.Label.Value);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    RecoveryWaferStatus();

                    Initialized = false;
                    ReservationInfo = new ReservationInfo();
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override void DeInitModule()
        {

        }

        public EventCodeEnum MonitorForSubstrate(bool onTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.IOManager.MonitorForIO(DIWAFERONMODULE, onTray, 100, Definition.IOCheckDelayTimeout.Value);
                if (retVal == EventCodeEnum.NONE && onTray && DI6INCHWAFERONMODULE != null && DI8INCHWAFERONMODULE != null)
                {
                    ValidateWaferSize();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ValidateWaferSize()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            EventCodeEnum retval_6 = EventCodeEnum.UNDEFINED;
            EventCodeEnum retval_8 = EventCodeEnum.UNDEFINED;
            try
            {
                retval_6 = Loader.IOManager.MonitorForIO(DI6INCHWAFERONMODULE, true, 100, Definition.IOCheckDelayTimeout.Value);
                retval_8 = Loader.IOManager.MonitorForIO(DI8INCHWAFERONMODULE, true, 100, Definition.IOCheckDelayTimeout.Value);

                if (retval_6 == EventCodeEnum.NONE && retval_8 != EventCodeEnum.NONE)
                {
                    if (Device?.AllocateDeviceInfo != null)
                    {
                        Device.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                        Device.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH6;
                    }
                }
                else if (retval_6 == EventCodeEnum.NONE && retval_8 == EventCodeEnum.NONE)
                {
                    if (Device?.AllocateDeviceInfo != null)
                    {
                        Device.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                        Device.AllocateDeviceInfo.Size.Value = SubstrateSizeEnum.INCH8;
                    }
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.IO_NOT_MATCHED;
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public EventCodeEnum IsWaferonmodule(out bool Result)
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            Result = false;
            try
            {
                retval = MonitorForSubstrate(true);
                if (retval == EventCodeEnum.NONE) 
                {
                    Result = true;
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.IO_NOT_MATCHED;
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum RecoveryWaferStatus(bool forcedAllocate = false)
        {
            EventCodeEnum subOnTrayRetVal = EventCodeEnum.NONE;

            try
            {
                bool isAcess = true;
                if (forcedAllocate == true)
                {
                    Holder.SetAllocate();

                    return EventCodeEnum.NONE;
                }

                if (Loader.ModuleState == ModuleStateEnum.RUNNING)
                {
                    if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.ARM_TO_FIXEDTRAY)
                    {
                        if (Loader.ProcModuleInfo.Destnation.Index == ID.Index)
                        {
                            isAcess = false;
                        }
                    }
                    else if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.FIXEDTRAY_TO_ARM)
                    {
                        if (Loader.ProcModuleInfo.Source.Index == ID.Index)
                        {
                            isAcess = false;
                        }
                    }
                    subOnTrayRetVal = EventCodeEnum.NONE;
                }

                if (isAcess) 
                {
                    if (Holder.Status == EnumSubsStatus.UNDEFINED)
                    {
                        subOnTrayRetVal = MonitorForSubstrate(false);

                        //Check no wafer on module.
                        if (subOnTrayRetVal == EventCodeEnum.NONE)
                        {
                            Holder.SetUnload();
                        }
                        else
                        {
                            //Check wafer on module.
                            subOnTrayRetVal = MonitorForSubstrate(true);
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetAllocate();
                            }
                            else
                            {

                                this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_FIXED_TRAY_WAF_MISSED, ID.Index);

                                Holder.SetUnknown();
                            }
                        }
                    }
                    else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                    {
                        //Check no wafer on module.
                        subOnTrayRetVal = MonitorForSubstrate(false);
                        if (subOnTrayRetVal == EventCodeEnum.NONE)
                        {
                            Holder.SetUnload();
                        }
                        else
                        {
                            subOnTrayRetVal = MonitorForSubstrate(true);
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetAllocate();
                            }
                            else
                            {

                                this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_FIXED_TRAY_WAF_MISSED, ID.Index);

                                Holder.SetUnknown();

                            }
                        }
                    }
                    else if (Holder.Status == EnumSubsStatus.EXIST)
                    {
                        // EXIST가 되어 있고, EMUL인 경우, 기존 상태를 유지한다.
                        // TODO : Single일 경우, 어떻게할지?
                        if (this.GetLoaderCommands() is GPLoaderRouter.GPLoaderCommandEmulator)
                        {
                            Holder.SetAllocate();

                            return EventCodeEnum.NONE;
                        }

                        //Check wafer on module.
                        subOnTrayRetVal = MonitorForSubstrate(true);

                        if (subOnTrayRetVal == EventCodeEnum.NONE)
                        {
                            Holder.SetAllocate();
                        }
                        else
                        {
                            subOnTrayRetVal = MonitorForSubstrate(false);
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetUnload();
                            }
                            else
                            {

                                this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_FIXED_TRAY_WAF_MISSED, ID.Index);

                                Holder.SetUnknown();
                            }
                        }
                    }
                    else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                    {
                        //Check no wafer on module.
                        //** Unknwon상태에서는 사용자가 직접 제거해야 한다.
                    }
                    else
                    {
                        throw new NotImplementedException("InitWaferStatus()");
                    }
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return subOnTrayRetVal;
        }

        public void ValidateWaferStatus()
        {
            try
            {
                bool isExistObj = Holder.Status == EnumSubsStatus.EXIST;

                //=> get iostate
                EventCodeEnum ioRetVal;
                ioRetVal = MonitorForSubstrate(isExistObj);

                if (isExistObj)
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        Holder.SetAllocate();
                        //obj : exist, io : exist
                        //No changed.
                    }
                    else
                    {
                        ioRetVal = MonitorForSubstrate(false);
                        if (ioRetVal == EventCodeEnum.NONE)
                        {
                            Holder.SetUnload();
                        }
                        else
                        {

                            this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_FIXED_TRAY_WAF_MISSED, ID.Index);

                            Holder.SetUnknown();
                        }
                    }
                }
                else
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        Holder.SetUnload();
                    }
                    else
                    {
                        ioRetVal = MonitorForSubstrate(true);
                        if (ioRetVal == EventCodeEnum.NONE)
                        {
                            Holder.SetAllocate();
                        }
                        else
                        {

                            this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_FIXED_TRAY_WAF_MISSED, ID.Index);

                            Holder.SetUnknown();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public FixedTrayAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            FixedTrayAccessParam system = null;

            try
            {
                system = Definition.AccessParams
                    .Where(item =>
                    item.SubstrateType.Value == type &&
                    item.SubstrateSize.Value == size
                    ).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return system;
        }

    }

}

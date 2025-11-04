using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;
using LoaderParameters.Data;

namespace LoaderCore
{
    internal class BufferModule : AttachedModuleBase, IBufferModule
    {
        private IOPortDescripter<bool> DIWAFERONMODULE;
        private IOPortDescripter<bool> DOAIRON;

        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.BUFFER;

        public BufferDefinition Definition { get; set; }

        public BufferDevice Device { get; set; }

        public WaferHolder Holder { get; set; }

        public ReservationInfo ReservationInfo { get; set; }
        public EnumWaferType WaferType { get; set; }
        public bool Enable { get; set; }
        public UpdateHolderDelegate HolderStatusChanged { get; set; }
        public TransferObject GetSourceDeviceInfo()
        {
            TransferObject retval = null;

            try
            {
                retval = Device?.AllocateDeviceInfo ?? null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetDefinition(BufferDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = definition.Enable.Value;
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");
                DIWAFERONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DIWAFERONMODULE.Value);

                if (Definition.DOAIRON.Value != null && Definition.DOAIRON.Value != string.Empty)
                {
                    DOAIRON = Loader.IOManager.GetIOPortDescripter(Definition.DOAIRON.Value);
                }

                Holder = new WaferHolder();
                Holder.SetOwner(this);


                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetDevice(BufferDevice device)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //  WaferType = definition.WaferType.Value;
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
                    //RecoveryWaferStatus();

                    Initialized = false;
                    ReservationInfo = new ReservationInfo();
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");

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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Definition.DOAIRON.Value != null && Definition.DOAIRON.Value != string.Empty)
                {
                    retval = Loader.IOManager.WriteIO(DOAIRON, value);
                    System.Threading.Thread.Sleep(1000);
                }
                else
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum RecoveryWaferStatus(bool forcedAllocate = false)
        {
            EventCodeEnum subOnTrayRetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Holder.Status == EnumSubsStatus.UNDEFINED)
                {
                    subOnTrayRetVal = WriteVacuum(true);
                    subOnTrayRetVal = MonitorForSubstrate(false);

                    //Check no wafer on module.
                    if (subOnTrayRetVal == EventCodeEnum.NONE)
                    {
                        Holder.SetUnload();
                        subOnTrayRetVal = WriteVacuum(false);
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
                            this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_BUFFER_WAF_MISSED, ID.Index);
                            Holder.SetUnknown();
                        }
                    }
                }
                else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    subOnTrayRetVal = WriteVacuum(true);
                    //Check no wafer on module.
                    subOnTrayRetVal = MonitorForSubstrate(false);
                    if (subOnTrayRetVal == EventCodeEnum.NONE)
                    {
                        //Holder.SetUnload();
                        subOnTrayRetVal = WriteVacuum(false);
                    }
                    else
                    {
                        subOnTrayRetVal = MonitorForSubstrate(true);
                        if (subOnTrayRetVal == EventCodeEnum.NONE)
                        {
                            bool isChecked = WaferDisappearControl.WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.NOT_EXIST.ToString(), EnumSubsStatus.EXIST.ToString());
                            if (isChecked)
                            {
                                Holder.SetAllocate();
                            }
                        }
                        else
                        {
                            this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_BUFFER_WAF_MISSED, ID.Index);
                            Holder.SetUnknown();
                        }
                    }
                }
                else if (Holder.Status == EnumSubsStatus.EXIST)
                {
                    subOnTrayRetVal = WriteVacuum(true);
                    //Check wafer on module.
                    subOnTrayRetVal = MonitorForSubstrate(true);
                    if (subOnTrayRetVal == EventCodeEnum.NONE)
                    {
                      //  Holder.SetAllocate();
                    }
                    else
                    {
                        subOnTrayRetVal = MonitorForSubstrate(false);
                        if (subOnTrayRetVal == EventCodeEnum.NONE)
                        {
                            bool isChecked = WaferDisappearControl.WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.EXIST.ToString(), EnumSubsStatus.NOT_EXIST.ToString());
                            if (isChecked)
                            {
                                Holder.SetUnload();
                                subOnTrayRetVal = WriteVacuum(false);
                            }
                        }
                        else
                        {
                            this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_BUFFER_WAF_MISSED, ID.Index);
                            Holder.SetUnknown();
                        }
                        // Holder.SetUnknown();
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
                        //obj : exist, io : exist
                        //No changed.
                    }
                    else
                    {
                        //obj : exist, io : not exist
                        Holder.SetUnknown();
                    }
                }
                else
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        //obj : not exist, io : not exist
                        //No changed.
                    }
                    else
                    {
                        //obj : not exist, io : exist
                        //status error - unknown detected.
                        Holder.SetUnknown();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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

        public BufferAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            BufferAccessParam system = null;

            try
            {
                system = Definition.AccessParams
               .Where(
               item =>
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

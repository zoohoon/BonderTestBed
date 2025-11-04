using System;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;
using WaferDisappearControl;
using LoaderParameters.Data;

namespace LoaderCore
{
    [Serializable]
    internal class ARMModule : AttachedModuleBase, IARMModule
    {
        private IOPortDescripter<bool> DOAIRON;
        private IOPortDescripter<bool> DIWAFERONMODULE;

        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.ARM;

        public ARMDefinition Definition { get; set; }

        public ARMDevice Device { get; set; }

        public WaferHolder Holder { get; set; }

        public ReservationInfo ReservationInfo { get; set; }
        public bool Enable { get; set; }
        public UpdateHolderDelegate HolderStatusChanged { get; set; }
        public EventCodeEnum SetDefinition(ARMDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = true;
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                DOAIRON = Loader.IOManager.GetIOPortDescripter(Definition.DOAIRON.Value);
                DIWAFERONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DIWAFERONMODULE.Value);
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

        public EventCodeEnum SetDevice(ARMDevice device)
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

                    retval=RecoveryWaferStatus();

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

        public EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.IOManager.WriteIO(DOAIRON, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum MonitorForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.IOManager.MonitorForIO(DIWAFERONMODULE, value, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.IOManager.WaitForIO(DIWAFERONMODULE, value, Definition.IOWaitTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum RecoveryWaferStatus(bool forcedAllocate = false)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //=> get io state

                retVal = WriteVacuum(true);

                if (Holder.Status == EnumSubsStatus.UNDEFINED)
                {
                    //Check no wafer on module. 
                    retVal = MonitorForVacuum(false);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        //obj : undefined, io : no exist
                        Holder.SetUnload();
                        retVal = WriteVacuum(false);
                        retVal = WaitForVacuum(false);
                    }
                    else
                    {
                        //Check wafer on module. 
                        retVal = MonitorForVacuum(true);
                        if (retVal == EventCodeEnum.NONE)
                        {
                            //obj : undefined, io : exist
                            Holder.SetAllocate();
                        }
                        else
                        {
                            //obj : undefined, io : unknown
                            this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_WAFER_MISSED_ON_ARM, ID.Index);

                            Holder.SetUnknown();
                            retVal = WriteVacuum(false);
                            retVal = WaitForVacuum(false);
                        }
                    }
                }
                else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    //Check no wafer on module.
                    retVal = MonitorForVacuum(false);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        //obj : not exist, io : not exist
                        //No changed.
                        retVal = WriteVacuum(false);
                        retVal = WaitForVacuum(false);
                    }
                    else
                    {

                        //obj : exist, io : unknown
                        retVal = MonitorForVacuum(true);
                        if (retVal != EventCodeEnum.NONE)
                        {

                            this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_WAFER_MISSED_ON_ARM, ID.Index);

                            retVal = EventCodeEnum.LOADER_STATE_INVALID;
                            Holder.SetUnknown();
                            retVal = WriteVacuum(false);
                            retVal = WaitForVacuum(false);
                        }
                        else
                        {
                            bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.NOT_EXIST.ToString(), EnumSubsStatus.EXIST.ToString());
                            if (isChecked)
                            {
                                Holder.SetAllocate();
                                retVal = WriteVacuum(true);
                                retVal = WaitForVacuum(true);
                            }
                            else
                            {

                                this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_WAFER_MISSED_ON_ARM, ID.Index);

                                retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                Holder.SetUnknown();
                            }
                           
                        }
                    }
                }
                else if (Holder.Status == EnumSubsStatus.EXIST)
                {
                    //Check wafer on module.
                    retVal = MonitorForVacuum(true);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        //obj : exist, io : exist
                        //No changed.
                      //  Holder.SetAllocate();
                    }
                    else
                    {
                        //obj : exist, io : unknown
                        retVal = MonitorForVacuum(false);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_WAFER_MISSED_ON_ARM, ID.Index);

                            retVal = EventCodeEnum.LOADER_STATE_INVALID;
                            Holder.SetUnknown();
                            retVal = WriteVacuum(false);
                            retVal = WaitForVacuum(false);
                        }
                        else
                        {
                            bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.EXIST.ToString(), EnumSubsStatus.NOT_EXIST.ToString());
                            if (isChecked)
                            {
                                Holder.SetUnload();
                                retVal = WriteVacuum(false);
                                retVal = WaitForVacuum(false);
                            }
                            else
                            {
                                this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_WAFER_MISSED_ON_ARM, ID.Index);
                                retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                Holder.SetUnknown();
                            }
                        }
                    }
                }
                else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                {
                }
                else
                {
                    retVal = WriteVacuum(false);
                    retVal = WaitForVacuum(false);

                    throw new NotImplementedException("InitWaferStatus()");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            return retVal;
        }

        public void ValidateWaferStatus()
        {
            try
            {
                bool isExistObj = Holder.Status == EnumSubsStatus.EXIST;

                //=> get iostate
                EventCodeEnum ioRetVal;
                ioRetVal = WriteVacuum(true);
                ioRetVal = MonitorForVacuum(isExistObj);

                if (isExistObj)
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        //obj : exist, io : exist
                        //No changed.
                    }
                    else
                    {
                        WriteVacuum(false);
                        WaitForVacuum(false);

                        //obj : exist, io : not exist
                        Holder.SetUnknown();
                    }
                }
                else
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        WriteVacuum(false);
                        WaitForVacuum(false);

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
            EventCodeEnum retVal = EventCodeEnum.NONE;
            Result = false;
            try
            {
                retVal = WriteVacuum(true);
                //Check wafer on module. 
                retVal = MonitorForVacuum(true);
                if (retVal == EventCodeEnum.NONE)
                {
                    Result = true;
                }
                else
                {
                    retVal = WriteVacuum(false);
                    retVal = WaitForVacuum(false);
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.IO_NOT_MATCHED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }
}

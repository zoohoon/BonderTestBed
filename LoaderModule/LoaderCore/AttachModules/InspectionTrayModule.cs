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
    internal class InspectionTrayModule : AttachedModuleBase, IInspectionTrayModule
    {
        private IOPortDescripter<bool> DIWAFERONMODULE;
        private IOPortDescripter<bool> DIOPENDED;
        private IOPortDescripter<bool> DI6INCHWAFERONMODULE;
        private IOPortDescripter<bool> DI8INCHWAFERONMODULE;
        public override bool Initialized { get; set; } = false;

        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.INSPECTIONTRAY;

        public InspectionTrayDefinition Definition { get; set; }

        public InspectionTrayDevice Device { get; set; }

        public WaferHolder Holder { get; set; }

        public ReservationInfo ReservationInfo { get; set; }
        public bool Enable { get; set; }
        public UpdateHolderDelegate HolderStatusChanged { get; set; }
        public EventCodeEnum SetDefinition(InspectionTrayDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = true;
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");
                #region => DI6INCHWAFERONMODULE
                DI6INCHWAFERONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DI6INCHWAFERONMODULE.Value);
                if (DI6INCHWAFERONMODULE != null)
                {
                    DI6INCHWAFERONMODULE.PropertyChanged += DIWAFERONMODULE_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error($"[InspectionTrayModule], SetDefinition(), DI6INCHWAFERONMODULE is null.");
                }
                #endregion

                #region => DI8INCHWAFERONMODULE
                DI8INCHWAFERONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DI8INCHWAFERONMODULE.Value);
                if (DI8INCHWAFERONMODULE != null)
                {
                    DI8INCHWAFERONMODULE.PropertyChanged += DIWAFERONMODULE_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error($"[InspectionTrayModule], SetDefinition(), DI8INCHWAFERONMODULE is null.");
                }
                #endregion

                #region => DIWAFERONMODULE
                DIWAFERONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DIWAFERONMODULE.Value);
                if (DIWAFERONMODULE != null)
                {
                    DIWAFERONMODULE.PropertyChanged += DIWAFERONMODULE_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error($"[InspectionTrayModule], SetDefinition(), DIWAFERONMODULE is null.");
                }
                #endregion
                #region => DIOPENDED
                DIOPENDED = Loader.IOManager.GetIOPortDescripter(Definition.DIOPENDED.Value);
                if (DIOPENDED != null)
                {
                    DIOPENDED.PropertyChanged += DIOPENDED_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error($"[InspectionTrayModule], SetDefinition(), DIOPENDED is null.");
                }
                #endregion

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

        public EventCodeEnum SetDevice(InspectionTrayDevice device)
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
                    retval = RecoveryWaferStatus();
                    ReservationInfo = new ReservationInfo();
                    Initialized = false;

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
        private void DIWAFERONMODULE_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                RecoveryWaferStatus();
                this.Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DIOPENDED_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                RecoveryWaferStatus();
                this.Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void DeInitModule()
        {

        }
        public EventCodeEnum RecoveryWaferStatus(bool forcedAllocate = false)
        {
            EventCodeEnum openerRetVal = EventCodeEnum.NONE;
            try
            {
                bool isAcess = true;
                openerRetVal = MonitorForOpened(true);
                if (openerRetVal == EventCodeEnum.NONE)
                {
                    isAcess = false;
                    Holder.SetUnload();
                }
                else if (Loader.ModuleState == ModuleStateEnum.RUNNING)
                {
                    if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.ARM_TO_INSPTRAY)
                    {
                        if (Loader.ProcModuleInfo.Destnation.Index == ID.Index)
                        {
                            isAcess = false;
                        }
                    }
                    else if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.INSPTRAY_TO_ARM)
                    {
                        if (Loader.ProcModuleInfo.Source.Index == ID.Index)
                        {
                            isAcess = false;
                        }
                    }
                    openerRetVal = EventCodeEnum.NONE;
                }

                if (isAcess)
                {
                    if (Holder.Status == EnumSubsStatus.UNDEFINED)
                    {
                        openerRetVal = MonitorForSubstrate(false);

                        //Check no wafer on module.
                        if (openerRetVal == EventCodeEnum.NONE)
                        {
                            Holder.SetUnload();
                        }
                        else
                        {
                            //Check wafer on module.
                            openerRetVal = MonitorForSubstrate(true);
                            if (openerRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetAllocate();
                            }
                            else
                            {
                                Holder.SetUnknown();
                            }
                        }

                    }
                    else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                    {
                        openerRetVal = MonitorForSubstrate(false);
                        if (openerRetVal == EventCodeEnum.NONE)
                        {
                            Holder.SetUnload();
                        }
                        else
                        {
                            openerRetVal = MonitorForSubstrate(true);
                            if (openerRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetAllocate();
                            }
                            else
                            {
                                Holder.SetUnknown();
                            }
                        }
                    }
                    else if (Holder.Status == EnumSubsStatus.EXIST)
                    {
                        openerRetVal = MonitorForSubstrate(true);
                        if (openerRetVal == EventCodeEnum.NONE)
                        {
                            //Holder.SetAllocate();
                        }
                        else
                        {
                            openerRetVal = MonitorForSubstrate(false);
                            if (openerRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetUnload();
                            }
                            else
                            {
                                Holder.SetUnknown();
                            }
                        }
                    }
                    else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                    {
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
            return openerRetVal;
        }
        public void ValidateWaferStatus()
        {
            try
            {
                bool isExistObj = Holder.Status == EnumSubsStatus.EXIST;
                bool expectedOpenerVal = false;
                bool expectedSubOnTrayVal = false;

                //=> get io state
                //if (isExistObj == true)
                //{
                //    expectedOpenerVal = !isExistObj;
                //}
                //else
                //{
                //    expectedOpenerVal = isExistObj;
                //}

                //EventCodeEnum openerRetVal = MonitorForOpened(expectedOpenerVal);
                //EventCodeEnum subOnTrayRetVal = MonitorForSubstrate(expectedSubOnTrayVal);
                //if (isExistObj)
                //{
                //    if (openerRetVal == EventCodeEnum.NONE &&
                //        subOnTrayRetVal == EventCodeEnum.NONE)
                //    {
                //        //obj : exist, io : exist
                //        //No Changed.
                //    }
                //    else
                //    {
                //        //obj : exist, io : not exist
                //        Holder.SetUnknown();
                //    }
                //}
                //else
                //{
                //    if (openerRetVal == EventCodeEnum.NONE &&
                //        subOnTrayRetVal == EventCodeEnum.NONE)
                //    {
                //        //obj : not exist, io : not exist
                //        //No changed.
                //        Holder.SetUnload();
                //    }
                //    else
                //    {
                //        //obj : not exist, io : exist
                //        //status error - unknown detected.
                //        Holder.SetUnknown();
                //    }
                //}
                EventCodeEnum openerRetVal = ReadOpened(out expectedOpenerVal);
                EventCodeEnum subOnTrayRetVal = ReadWaferOnInspectionTray(out expectedSubOnTrayVal);
                bool isEmulMode = Extensions_IParam.ProberRunMode == RunMode.EMUL;
                if (expectedOpenerVal == true)
                {
                    //드로워 열려있어서 신경 안쓴다 웨이퍼온 센서같은거 
                    Holder.SetUnload();
                }
                else
                {
                    if (isExistObj)
                    {
                        if (isEmulMode || (expectedOpenerVal == false &&
                            expectedSubOnTrayVal == true))
                        {
                            //obj : exist, io : exist
                            //No Changed.
                        }
                        else
                        {
                            //obj : exist, io : not exist
                            Holder.SetUnknown();
                        }
                    }
                    else
                    {
                        if (isEmulMode || expectedSubOnTrayVal == false)
                        {
                            //obj : not exist, io : not exist
                            //No changed.
                            Holder.SetUnload();
                        }
                        else
                        {
                            //obj : not exist, io : exist
                            //status error - unknown detected.
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

        public TransferObject GetSourceDeviceInfo()
        {
            TransferObject retval = null;

            try
            {
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

        public InspectionTrayAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            InspectionTrayAccessParam system = null;

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

        public EventCodeEnum ReadOpened(out bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            value = false;

            try
            {
                retVal = Loader.IOManager.ReadIO(DIOPENDED, out value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ReadWaferOnInspectionTray(out bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            value = false;

            try
            {
                retVal = Loader.IOManager.ReadIO(DIWAFERONMODULE, out value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MonitorForOpened(bool isOpen)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.IOManager.MonitorForIO(DIOPENDED, isOpen, 100, Definition.IOCheckDelayTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MonitorForSubstrate(bool onTray)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.IOManager.MonitorForIO(DIWAFERONMODULE, onTray, 100, Definition.IOCheckDelayTimeout.Value);
                if (retval == EventCodeEnum.NONE && onTray && DI6INCHWAFERONMODULE != null && DI8INCHWAFERONMODULE != null)
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
                        Device.AllocateDeviceInfo.NotchType = WaferNotchTypeEnum.FLAT;
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
                else 
                {
                    retval = EventCodeEnum.IO_NOT_MATCHED;
                    LoggerManager.Error($"{this.GetType().Name}| DI6INCHWAFERONMODULE: {retval_6}, DI8INCHWAFERONMODULE {retval_8}");
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
    }

}
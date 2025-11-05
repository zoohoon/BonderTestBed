using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;
using WaferDisappearControl;
using LoaderParameters.Data;
using LoaderBase.AttachModules.ModuleInterfaces;
using GPLoaderRouter;
using ProberInterfaces.PreAligner;

namespace LoaderCore
{
    internal class PreAlignModule : AttachedModuleBase, IPreAlignModule
    {
        private IOPortDescripter<bool> DOAIRON;
        private IOPortDescripter<bool> DIWAFERONMODULE;

        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.PA;

        public PreAlignDefinition Definition { get; set; }

        public PreAlignDevice Device { get; set; }

        public WaferHolder Holder { get; set; }
        public ReservationInfo ReservationInfo { get; set; }
        public bool Enable { get; set; }
        public UpdateHolderDelegate HolderStatusChanged { get; set; }
        public EnumPAStatus PAStatus 
        {
            get { return Loader.PAManager.PAModules[ID.Index - 1].PAStatus; }
        }

        public EventCodeEnum SetDefinition(PreAlignDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = definition.Enable.Value;
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
      

        public EventCodeEnum SetDevice(PreAlignDevice device)
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

        public EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DOAIRON == null)
                {
                    return EventCodeEnum.NONE;
                }
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (DIWAFERONMODULE == null)
                {
                    return EventCodeEnum.NONE;
                }
                retVal = Loader.IOManager.MonitorForIO(DIWAFERONMODULE, value, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DIWAFERONMODULE == null)
                {
                    return EventCodeEnum.NONE;
                }
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Loader != null && Loader.GetLoaderCommands() is GPLoader)
                {
                    bool isExist = false;
                    retVal = Loader.GetLoaderCommands().CheckWaferIsOnPA(ID.Index, out isExist);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        if (isExist == true)
                        {
                            if (Holder.Status != EnumSubsStatus.EXIST)
                            {
                                Holder.SetAllocate();
                            }
                        }
                        else 
                        {
                            Holder.SetUnload();
                        }
                    }
                    else
                    {
                        Holder.SetUnknown();
                    }
                }
                else
                {
                    //=> get io state
                    retVal = WriteVacuum(true);
                    //var sustainTime = Definition.IOCheckMaintainTime;
                    //Definition.IOCheckMaintainTime.Value = sustainTime.Value * 3;
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

                                this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_PA_WAF_MISSED, ID.Index);

                                retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                //obj : undefined, io : unknown
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
                            retVal = MonitorForVacuum(true);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.NOT_EXIST.ToString(), EnumSubsStatus.EXIST.ToString());
                                if (isChecked)
                                {
                                    Holder.SetAllocate();
                                }
                                else
                                {

                                    this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_PA_WAF_MISSED, ID.Index);

                                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                    Holder.SetUnknown();
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                            }
                            else
                            {

                                this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_PA_WAF_MISSED, ID.Index);

                                retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                //obj : not exist, io : unknown
                                Holder.SetUnknown();
                                retVal = WriteVacuum(false);
                                retVal = WaitForVacuum(false);
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
                            Holder.SetAllocate();
                        }
                        else
                        {
                            retVal = MonitorForVacuum(false);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.EXIST.ToString(), EnumSubsStatus.NOT_EXIST.ToString());
                                if (isChecked)
                                {
                                    Holder.SetUnload();
                                }
                                else
                                {

                                    this.Loader.NotifyManager.Notify(EventCodeEnum.LOADER_PA_WAF_MISSED, ID.Index);

                                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                    Holder.SetUnknown();
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                Holder.SetUnknown();
                                retVal = WriteVacuum(false);
                                retVal = WaitForVacuum(false);
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
                        retVal = WriteVacuum(false);
                        retVal = WaitForVacuum(false);
                        //Definition.IOCheckMaintainTime = sustainTime;

                        throw new NotImplementedException("InitWaferStatus()");
                    }
                }
                //Definition.IOCheckMaintainTime = sustainTime;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
            EventCodeEnum retval = EventCodeEnum.NONE;
            Result = false;
            try
            {
                retval = Loader.GetLoaderCommands().CheckWaferIsOnPA(ID.Index, out bool isExist);
                if (retval == EventCodeEnum.NONE)
                {
                    Result = isExist;
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.IO_NOT_MATCHED;
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public bool CanPreAlignable(TransferObject transferObject)
        {
            bool retval = false;

            try
            {
                retval = transferObject.Type.Value == SubstrateTypeEnum.Wafer;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public PreAlignAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            PreAlignAccessParam param = null;

            try
            {
                param = Definition.AccessParams
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

            return param;
        }

        public PreAlignProcessingParam GetProcessingParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            PreAlignProcessingParam param = null;

            try
            {
                param = Definition.ProcessingParams
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

            return param;
        }

        public bool IsNeedRatateOffsetNotchAngle(TransferObject transferObject, IHasLoadNotchAngle destination)
        {
            bool retval = false;

            try
            {
                var rotateOffsetAngle = CalcRatateOffsetNotchAngle(transferObject, destination);

                retval = Math.Abs(rotateOffsetAngle.Value) > 0.001;
                retval = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Degree CalcRatateOffsetNotchAngle(TransferObject transferObject, IHasLoadNotchAngle destination)
        {
            Degree rotateOffsetAngle = Degree.ZERO;

            try
            {
                var accessParam = GetAccessParam(transferObject.Type.Value, transferObject.Size.Value);
                var procParam = GetProcessingParam(transferObject.Type.Value, transferObject.Size.Value);

                if(accessParam != null && procParam != null)
                {
                    Degree WP = new Degree(accessParam.Position.W.Value * ConstantValues.W_DIST_TO_DEGREE);

                    Degree WD = destination.GetWaxisAngle(transferObject.Type.Value, transferObject.Size.Value);

                    Degree T = procParam.NotchSensorAngle.Value;
                    Degree CurrNotchAngle = transferObject.NotchAngle.Value;

                    Degree L = destination.GetLoadingAngle();

                    if (transferObject.OverrideLoadNotchAngleOption.IsEnable.Value)
                    {
                        L = transferObject.OverrideLoadNotchAngleOption.Angle.Value;
                    }

                    rotateOffsetAngle = WP - WD - T + L;

                    rotateOffsetAngle = rotateOffsetAngle.Normalized(-180, 180);
                }
                else
                {
                    if (accessParam == null)
                    {
                        LoggerManager.Error($"[PreAlignModule], CalcRatateOffsetNotchAngle() : accessParam is null. SubstrateTypeEnum = {transferObject.Type.Value}, SubstrateSizeEnum = {transferObject.Size.Value}");
                    }

                    if (procParam == null)
                    {
                        LoggerManager.Error($"[PreAlignModule], CalcRatateOffsetNotchAngle() : procParam is null. SubstrateTypeEnum = {transferObject.Type.Value}, SubstrateSizeEnum = {transferObject.Size.Value}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rotateOffsetAngle;
        }




        public bool IsNeedRatateOffsetNotchAngle(TransferObject transferObject, IOCRReadable destination)
        {
            bool retval = false;

            try
            {
                var rotateOffsetAngle = CalcRatateOffsetNotchAngle(transferObject, destination);
                retval = Math.Abs(rotateOffsetAngle.Value) > 0.001;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Degree CalcRatateOffsetNotchAngle(TransferObject transferObject, IOCRReadable destination)
        {
            Degree rotateOffsetAngle = Degree.ZERO;

            try
            {
                var originParam = destination.GetAccessParam(transferObject.Type.Value, transferObject.Size.Value);

                double offsetV = 0;
                if (transferObject.OverrideOCRDeviceOption.IsEnable.Value)
                {
                    offsetV = transferObject.OverrideOCRDeviceOption.OCRDeviceBase.OffsetV.Value;
                }
                else
                {
                    var offsetPos = destination.GetOCROffset();
                    offsetV = offsetPos.OffsetV;
                }

                //double fullVpos = originParam.VPos.Value + offsetV;
                //double fullVpos = originParam.VPos.Value + originParam.OCROffsetV;
                
                double fullVpos = originParam.VPos.Value 
                    + this.Loader.Container.Resolve<ICognexProcessManager>().CognexProcDevParam.ConfigList[ID.Index - 1].AngleOffset;

                var dstNotchAngle = new Degree(fullVpos * ConstantValues.V_DIST_TO_DEGREE);

                if(transferObject.NotchAngle.Value < 0)
                {
                    var angle = transferObject.NotchAngle.Value + 360.0;
                    transferObject.NotchAngle.Value = angle;
                }
                rotateOffsetAngle = dstNotchAngle - new Degree(transferObject.NotchAngle.Value);
                rotateOffsetAngle = rotateOffsetAngle.Normalized(-180, 180);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rotateOffsetAngle;
        }

    }

}

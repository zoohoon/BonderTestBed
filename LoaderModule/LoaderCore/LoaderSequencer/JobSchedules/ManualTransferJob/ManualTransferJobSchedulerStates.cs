using System;

using LoaderBase;
using LoaderParameters;
using LogModule;
using ProberInterfaces;

namespace LoaderCore.ManualTransferJobStates
{
    public abstract class ManualTransferJobStateBase
    {
        public ManualTransferJob Scheduler { get; set; }

        public ManualTransferJobStateBase(ManualTransferJob scheduler)
        {
            try
            {
                this.Scheduler = scheduler;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public abstract JobValidateResult Validate();

        public abstract JobScheduleResult Execute();

    }

    public class OnSLOT : ManualTransferJobStateBase
    {
        public OnSLOT(ManualTransferJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                if (Scheduler.Destination is IARMModule)
                {
                    var ARM = Scheduler.Destination as IARMModule;
                    if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"ARM status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IPreAlignable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    var PA = Scheduler.Destination as IPreAlignable;
                    if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"PA status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IOCRReadable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                    if (usablePA == null)
                    {
                        rel.SetError($"Can not found loadable PreAligner.");
                        return rel;
                    }
                }

                rel.SetValid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

        public override JobScheduleResult Execute()
        {
            JobScheduleResult rel = new JobScheduleResult();
            try
            {
                IARMModule useARM;
                if (Scheduler.Destination is IARMModule)
                {
                    useARM = Scheduler.Destination as IARMModule;
                }
                else
                {
                    useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                }

                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useARM, useARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }

    }//end of state class

    public class OnFixedTray : ManualTransferJobStateBase
    {
        public OnFixedTray(ManualTransferJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                if (Scheduler.Destination is IARMModule)
                {
                    var ARM = Scheduler.Destination as IARMModule;
                    if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"ARM status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IPreAlignable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    var PA = Scheduler.Destination as IPreAlignable;
                    if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"PA status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IOCRReadable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                    if (usablePA == null)
                    {
                        rel.SetError($"Can not found loadable PreAligner.");
                        return rel;
                    }
                }

                rel.SetValid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

        public override JobScheduleResult Execute()
        {
            JobScheduleResult rel = new JobScheduleResult();
            try
            {
                IARMModule useARM;
                if (Scheduler.Destination is IARMModule)
                {
                    useARM = Scheduler.Destination as IARMModule;
                }
                else
                {
                    useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                }

                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useARM, useARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }

    }//end of state class

    public class OnInspectionTray : ManualTransferJobStateBase
    {
        public OnInspectionTray(ManualTransferJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                if (Scheduler.Destination is IARMModule)
                {
                    var ARM = Scheduler.Destination as IARMModule;
                    if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"ARM status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IPreAlignable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    var PA = Scheduler.Destination as IPreAlignable;
                    if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"PA status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IOCRReadable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                    if (usablePA == null)
                    {
                        rel.SetError($"Can not found loadable PreAligner.");
                        return rel;
                    }
                }

                rel.SetValid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

        public override JobScheduleResult Execute()
        {
            JobScheduleResult rel = new JobScheduleResult();
            try
            {
                IARMModule useARM;
                if (Scheduler.Destination is IARMModule)
                {
                    useARM = Scheduler.Destination as IARMModule;
                }
                else
                {
                    useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                }

                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useARM, useARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }

    }//end of state class

    public class OnChuck : ManualTransferJobStateBase
    {
        public OnChuck(ManualTransferJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                if (Scheduler.Destination is IARMModule)
                {
                    var ARM = Scheduler.Destination as IARMModule;
                    if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"ARM status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IPreAlignable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }

                    var PA = Scheduler.Destination as IPreAlignable;
                    if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"PA status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IOCRReadable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }

                    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                    if (usablePA == null)
                    {
                        rel.SetError($"Can not found loadable PreAligner.");
                        return rel;
                    }
                }

                rel.SetValid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

        public override JobScheduleResult Execute()
        {
            JobScheduleResult rel = new JobScheduleResult();
            try
            {
                IARMModule useARM;
                if (Scheduler.Destination is IARMModule)
                {
                    useARM = Scheduler.Destination as IARMModule;
                }
                else
                {
                    useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                }

                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useARM, useARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }

    }//end of state class

    public class OnARM : ManualTransferJobStateBase
    {
        public OnARM(ManualTransferJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                if (Scheduler.Destination is IARMModule)
                {
                    IARMModule dstARM = Scheduler.Destination as IARMModule;
                    if (dstARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"Destination ARM status invalid.");
                        return rel;
                    }

                    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                    if (usablePA == null)
                    {
                        rel.SetError($"Can not found loadable PreAligner.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IPreAlignable)
                {
                    //IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    //if (loadARM == null)
                    //{
                    //    rel.SetError($"Can not found loadable ARM.");
                    //    return rel;
                    //}

                    var PA = Scheduler.Destination as IPreAlignable;
                    if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"PA status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IOCRReadable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                    if (usablePA == null)
                    {
                        rel.SetError($"Can not found loadable PreAligner.");
                        return rel;
                    }
                }

                rel.SetValid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

        public override JobScheduleResult Execute()
        {
            JobScheduleResult rel = new JobScheduleResult();
            try
            {
                var ARM = Scheduler.CurrHolder as IARMModule;
                var DestPos = Scheduler.Destination;

                if (DestPos == ARM)
                {
                    //=> clear options
                    Scheduler.TransferObject.DisableOptionAll();

                    rel.SetJobDone();
                    return rel;
                }

                if (DestPos is IARMModule ||
                    DestPos is IPreAlignable)
                {
                    IPreAlignable usablePA = DestPos as IPreAlignable;

                    rel.SetTransfer(Scheduler.TransferObject, ARM, usablePA, ARM, Scheduler.Destination);
                    return rel;
                }

                if (DestPos is IOCRReadable)
                {
                    var readable = DestPos as IOCRReadable;

                    IPreAlignModule usablePA = readable.GetDependecyPA();

                    rel.SetTransfer(Scheduler.TransferObject, ARM, usablePA, ARM, Scheduler.Destination);
                    return rel;
                }

                rel.SetError($"Request invalid.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class

    public class OnPreAlign : ManualTransferJobStateBase
    {
        public OnPreAlign(ManualTransferJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                if (Scheduler.Destination is IARMModule)
                {
                    var ARM = Scheduler.Destination as IARMModule;
                    if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"ARM status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IOCRReadable)
                {
                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }
                }

                rel.SetValid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

        public override JobScheduleResult Execute()
        {
            JobScheduleResult rel = new JobScheduleResult();
            try
            {
                IPreAlignModule PA = Scheduler.CurrHolder as IPreAlignModule;
                var DestPos = Scheduler.Destination;

                if (Scheduler.TransferObject.NeedPreAlign())
                {
                    var useARM = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.PrevHolder) as IARMModule;
                    if (useARM == null || useARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        useARM = Scheduler.Loader.ModuleManager.FindUsableModule<IARMModule>();
                    }

                    rel.SetPreAlign(Scheduler.TransferObject, PA, useARM, Scheduler.Destination);
                    return rel;
                }

                if (DestPos == Scheduler.CurrHolder)
                {
                    //=> clear options
                    Scheduler.TransferObject.DisableOptionAll();

                    rel.SetJobDone();
                    return rel;
                }

                if (DestPos is IARMModule)
                {
                    var useARM = Scheduler.Destination as IARMModule;

                    rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useARM, useARM, Scheduler.Destination);
                    return rel;
                }

                if (DestPos is IOCRReadable)
                {
                    var OcrReadable = Scheduler.Destination as IOCRReadable;

                    var useARM = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.PrevHolder) as IARMModule;
                    if (useARM == null || useARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    }

                    if (PA != OcrReadable.GetDependecyPA())
                    {
                        rel.SetError();
                        return rel;
                    }
                    else
                    {
                        rel.SetTransfer(Scheduler.TransferObject, PA, OcrReadable, useARM, Scheduler.Destination);
                        return rel;
                    }
                }

                rel.SetError("Not supported.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class

    public class OnOCR : ManualTransferJobStateBase
    {
        public OnOCR(ManualTransferJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                var OcrReadable = Scheduler.CurrPos as IOCRReadable;
                var DestPos = Scheduler.Destination;

                if (DestPos is IOCRReadable &&
                    DestPos != Scheduler.CurrPos)
                {
                    rel.SetError($"Not supported.");
                    return rel;
                }

                if (DestPos is IPreAlignModule)
                {
                    var PA = DestPos as IPreAlignModule;
                    var dependecyPA = OcrReadable.GetDependecyPA();
                    if (dependecyPA != PA)
                    {
                        rel.SetError($"Destination invalid.");
                        return rel;
                    }
                }

                rel.SetValid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

        public override JobScheduleResult Execute()
        {
            JobScheduleResult rel = new JobScheduleResult();
            try
            {
                var useARM = Scheduler.CurrHolder as IARMModule;
                var OcrReadable = Scheduler.CurrPos as IOCRReadable;
                var DestPos = Scheduler.Destination;

                var usedPA = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.UsedPA) as IPreAlignModule;
                if (Scheduler.TransferObject.NeedPreAlign() ||
                    usedPA.IsNeedRatateOffsetNotchAngle(Scheduler.TransferObject, OcrReadable))
                {
                    IPreAlignModule PA = OcrReadable.GetDependecyPA();

                    rel.SetTransfer(Scheduler.TransferObject, OcrReadable, PA, useARM, Scheduler.Destination);
                    return rel;
                }

                if (Scheduler.TransferObject.NeedOCR())
                {
                    rel.SetReadOCR(Scheduler.TransferObject, OcrReadable, useARM, Scheduler.Destination);
                    return rel;
                }

                if (DestPos == Scheduler.CurrPos)
                {
                    //=> clear options
                    Scheduler.TransferObject.DisableOptionAll();

                    rel.SetJobDone();
                    return rel;
                }

                if (DestPos is IPreAlignModule)
                {
                    IPreAlignModule PA = OcrReadable.GetDependecyPA();

                    rel.SetTransfer(Scheduler.TransferObject, OcrReadable, PA, useARM, Scheduler.Destination);
                    return rel;
                }

                rel.SetError("Not supported.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class
}

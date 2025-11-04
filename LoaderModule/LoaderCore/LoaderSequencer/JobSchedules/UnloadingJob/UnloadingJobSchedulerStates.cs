using System;

using LoaderBase;
using LoaderParameters;
using LogModule;
using ProberInterfaces;

namespace LoaderCore.UnloadingJobStates
{
    public abstract class UnloadingJobSchedulerStateBase
    {
        public UnloadingJob Scheduler { get; set; }

        public UnloadingJobSchedulerStateBase(UnloadingJob scheduler)
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

    public class OnCHUCK : UnloadingJobSchedulerStateBase
    {
        public OnCHUCK(UnloadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                //=> Check ARM
                if (Scheduler.Destination.Holder.Status == EnumSubsStatus.EXIST)
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }

                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (unloadARM == loadARM)
                    {
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                    }

                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }
                else
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                IPreAlignable usablePA = Scheduler.Loader.ModuleManager.FindUsablePreAlignable(Scheduler.TransferObject);
                if (usablePA == null)
                {
                    rel.SetError($"Can not found loadable PreAligner.");
                    return rel;
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
                var CHUCK = Scheduler.CurrHolder as IChuckModule;
                var ARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);

                rel.SetTransfer(Scheduler.TransferObject, CHUCK, ARM, ARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
    }//end of state class

    public class OnARM : UnloadingJobSchedulerStateBase
    {
        public OnARM(UnloadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                IARMModule CurrARM = Scheduler.CurrHolder as IARMModule;

                //=> Check ARM
                if (Scheduler.Destination.Holder.Status == EnumSubsStatus.EXIST)
                {
                    IARMModule unloadARM;
                    if (CurrARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.UNLOADING))
                    {
                        unloadARM = CurrARM;
                    }
                    else
                    {
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    }
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }

                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING, unloadARM);
                    if (loadARM == null)
                    {
                        if (unloadARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.LOADING))
                        {
                            loadARM = unloadARM;
                            unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                        }
                    }

                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }
                else
                {
                    IARMModule unloadARM;
                    if (CurrARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.UNLOADING))
                    {
                        unloadARM = CurrARM;
                    }
                    else
                    {
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    }
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                //=> Check PA
                var usedPA = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.UsedPA) as IPreAlignable;

                if (usedPA == null || 
                    Scheduler.TransferObject.NeedPreAlign() || 
                    (Scheduler.Destination is ISlotModule && usedPA.IsNeedRatateOffsetNotchAngle(Scheduler.TransferObject, Scheduler.Destination as ISlotModule)))
                {
                    IPreAlignable usablePA = Scheduler.Loader.ModuleManager.FindUsablePreAlignable(Scheduler.TransferObject);
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

                var usedPA = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.UsedPA) as IPreAlignable;
                
                if(Scheduler.CurrHolder.Holder.TransferObject.PrevHolder.ModuleType==ModuleTypeEnum.SLOT&& Scheduler.Destination.ModuleType == ModuleTypeEnum.SLOT)
                {

                }
                else if (usedPA == null ||
                    Scheduler.TransferObject.NeedPreAlign() ||
                    (Scheduler.Destination is ISlotModule && usedPA.IsNeedRatateOffsetNotchAngle(Scheduler.TransferObject, Scheduler.Destination as ISlotModule)))
                {
                    var usablePA = Scheduler.Loader.ModuleManager.FindUsablePreAlignable(Scheduler.TransferObject);

                    rel.SetTransfer(Scheduler.TransferObject, ARM, usablePA, ARM, Scheduler.Destination);
                    return rel;
                }

                rel.SetTransfer(Scheduler.TransferObject, ARM, Scheduler.Destination, ARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }//end of state class

    public class OnPreAlign : UnloadingJobSchedulerStateBase
    {
        public OnPreAlign(UnloadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {

                //=> Check ARM
                if (Scheduler.Destination.Holder.Status == EnumSubsStatus.EXIST)
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }

                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (unloadARM == loadARM)
                    {
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                    }

                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }
                else
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
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
                var PA = Scheduler.CurrPos as IPreAlignModule;

                var unloadARM = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.PrevHolder) as IARMModule;
                if (unloadARM == null ||
                    unloadARM.Holder.Status != EnumSubsStatus.NOT_EXIST ||
                    unloadARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.UNLOADING) == false)
                {
                    //이전 ARM의 정보가 유효하지 않으므로
                    unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                }

                if (Scheduler.Destination.Holder.Status == EnumSubsStatus.EXIST)
                {
                    var loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING, unloadARM);
                    if (loadARM == null && unloadARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.LOADING))
                    {
                        //switch to unloadARM
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, unloadARM);
                    }
                }

                if (Scheduler.TransferObject.NeedPreAlign())
                {
                    rel.SetPreAlign(Scheduler.TransferObject, PA, unloadARM, Scheduler.Destination);
                    return rel;
                }

                rel.SetTransfer(Scheduler.TransferObject, PA, unloadARM, unloadARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }//end of state class

    public class OnOCR : UnloadingJobSchedulerStateBase
    {
        public OnOCR(UnloadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                IARMModule CurrARM = Scheduler.CurrHolder as IARMModule;
                IOCRReadable OCR = Scheduler.CurrPos as IOCRReadable;

                //=> Check ARM
                if (Scheduler.Destination.Holder.Status == EnumSubsStatus.EXIST)
                {
                    IARMModule unloadARM;
                    if (CurrARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.UNLOADING))
                    {
                        unloadARM = CurrARM;
                    }
                    else
                    {
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    }
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }

                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING, unloadARM);
                    if (loadARM == null)
                    {
                        if (unloadARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.LOADING))
                        {
                            loadARM = unloadARM;
                            unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                        }
                    }
                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }
                else
                {
                    IARMModule unloadARM;
                    if (CurrARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.UNLOADING))
                    {
                        unloadARM = CurrARM;
                    }
                    else
                    {
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    }
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                //=> Check PA
                var usablePA = OCR.GetDependecyPA();
                if (usablePA == null || usablePA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    rel.SetError($"Can not found loadable PreAligner.");
                    return rel;
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
                var readable = Scheduler.CurrPos as IOCRReadable;
                var useARM = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.CurrHolder) as IARMModule;
                var usedPA = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.UsedPA) as IPreAlignModule;

                if (usedPA.IsNeedRatateOffsetNotchAngle(Scheduler.TransferObject, readable))
                {
                    rel.SetTransfer(Scheduler.TransferObject, readable, usedPA, useARM, Scheduler.Destination);
                    return rel;
                }

                if (Scheduler.TransferObject.NeedOCR())
                {
                    rel.SetReadOCR(Scheduler.TransferObject, readable, useARM, Scheduler.Destination);
                    return rel;
                }

                rel.SetTransfer(Scheduler.TransferObject, readable, usedPA, useARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class

    public class OnSLOT : UnloadingJobSchedulerStateBase
    {
        public OnSLOT(UnloadingJob job) : base(job) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                IWaferOwnable CurrHolder = Scheduler.CurrHolder as IWaferOwnable;

                if (CurrHolder == Scheduler.Destination)
                {
                    rel.SetValid();
                    return rel;
                }

                //=> Check ARM
                if (Scheduler.Destination.Holder.Status == EnumSubsStatus.EXIST)
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }

                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (unloadARM == loadARM)
                    {
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                    }

                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }
                else
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                IPreAlignable usablePA = Scheduler.Loader.ModuleManager.FindUsablePreAlignable(Scheduler.TransferObject);
                if (usablePA == null)
                {
                    rel.SetError($"Can not found loadable PreAligner.");
                    return rel;
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
                if (Scheduler.CurrHolder == Scheduler.Destination)
                {
                    //=> clear options
                    Scheduler.TransferObject.DisableOptionAll();

                    rel.SetJobDone();
                    return rel;
                }

                var useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);

                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useARM, useARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }//end of state class

    public class OnFixedTray : UnloadingJobSchedulerStateBase
    {
        public OnFixedTray(UnloadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                IWaferOwnable CurrHolder = Scheduler.CurrHolder as IWaferOwnable;

                if (CurrHolder == Scheduler.Destination)
                {
                    rel.SetValid();
                    return rel;
                }

                //=> Check ARM
                if (Scheduler.Destination.Holder.Status == EnumSubsStatus.EXIST)
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }

                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (unloadARM == loadARM)
                    {
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                    }

                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }
                else
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                IPreAlignable usablePA = Scheduler.Loader.ModuleManager.FindUsablePreAlignable(Scheduler.TransferObject);
                if (usablePA == null)
                {
                    rel.SetError($"Can not found loadable PreAligner.");
                    return rel;
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
                if (Scheduler.CurrHolder == Scheduler.Destination)
                {
                    //=> clear options
                    Scheduler.TransferObject.DisableOptionAll();

                    rel.SetJobDone();
                    return rel;
                }

                var useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);

                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useARM, useARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }//end of state class

    public class OnInspectionTray : UnloadingJobSchedulerStateBase
    {
        public OnInspectionTray(UnloadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                IWaferOwnable CurrHolder = Scheduler.CurrHolder as IWaferOwnable;

                if (CurrHolder == Scheduler.Destination)
                {
                    rel.SetValid();
                    return rel;
                }

                //=> Check ARM
                if (Scheduler.Destination.Holder.Status == EnumSubsStatus.EXIST)
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }

                    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                    if (unloadARM == loadARM)
                    {
                        unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                    }

                    if (loadARM == null)
                    {
                        rel.SetError($"Can not found loadable ARM.");
                        return rel;
                    }

                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }
                else
                {
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                IPreAlignable usablePA = Scheduler.Loader.ModuleManager.FindUsablePreAlignable(Scheduler.TransferObject);
                if (usablePA == null)
                {
                    rel.SetError($"Can not found loadable PreAligner.");
                    return rel;
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
                if (Scheduler.CurrHolder == Scheduler.Destination)
                {
                    //=> clear options
                    Scheduler.TransferObject.DisableOptionAll();

                    rel.SetJobDone();
                    return rel;
                }

                var useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);

                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useARM, useARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }//end of state class
}

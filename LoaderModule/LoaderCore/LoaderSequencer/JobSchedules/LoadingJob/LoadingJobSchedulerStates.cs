using System;
using System.Linq;
using Autofac;
using LoaderBase;
using LoaderBase.AttachModules.ModuleInterfaces;
using LoaderParameters;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Enum;

namespace LoaderCore.LoadingJobStates
{
    public abstract class LoadingJobStateBase
    {
        public LoadingJob Scheduler { get; set; }
        public ICognexProcessManager CognexProcessManager => Scheduler.Loader.Container.Resolve<ICognexProcessManager>();

        public LoadingJobStateBase(LoadingJob scheduler)
        {
            this.Scheduler = scheduler;
        }

        public abstract JobValidateResult Validate();

        public abstract JobScheduleResult Execute();
    }

    public class OnSLOT : LoadingJobStateBase
    {
        public OnSLOT(LoadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                var SLOT = Scheduler.CurrHolder as ISlotModule;
                var CHUCK = Scheduler.Destination as IChuckModule;

                //=> Check Cassette
                if (SLOT.Cassette.ScanState != CassetteScanStateEnum.READ)
                {
                    rel.SetError($"Cassette scan state invalid. cassette={SLOT.Cassette.ID}");
                    return rel;
                }

                //=> Check Load ARM
                IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                if (loadARM == null)
                {
                    rel.SetError($"Can not found loadable ARM.");
                    return rel;
                }

                //=> Check Unload ARM
                if (CHUCK.Holder.Status == EnumSubsStatus.EXIST)
                {
                    //LoadARM을 제외한 UnloadARM 찾기
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                //=> Check PreAlign
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
                //Select transfer process module. ARM or Gripper or ...
                var SLOT = Scheduler.CurrHolder as ISlotModule;
                var Cassette = SLOT.Cassette;

                var ARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                rel.SetTransfer(Scheduler.TransferObject, SLOT, ARM, ARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
    }

    public class OnFixedTray : LoadingJobStateBase
    {
        public OnFixedTray(LoadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                var CHUCK = Scheduler.Destination as IChuckModule;

                //=> Check Load ARM
                IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                if (loadARM == null)
                {
                    rel.SetError($"Can not found loadable ARM.");
                    return rel;
                }

                //=> Check Unload ARM
                if (CHUCK.Holder.Status == EnumSubsStatus.EXIST)
                {
                    //LoadARM을 제외한 UnloadARM 찾기
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                //=> Check PreAlign
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
                var loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);

                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, loadARM, loadARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
    }

    public class OnInspectionTray : LoadingJobStateBase
    {
        public OnInspectionTray(LoadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                var CHUCK = Scheduler.Destination as IChuckModule;

                //=> Check Load ARM
                IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                if (loadARM == null)
                {
                    rel.SetError($"Can not found loadable ARM.");
                    return rel;
                }

                //=> Check Unload ARM
                if (CHUCK.Holder.Status == EnumSubsStatus.EXIST)
                {
                    //LoadARM을 제외한 UnloadARM 찾기
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                //=> Check PreAlign
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

                var loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);

                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, loadARM, loadARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
    }

    public class OnARM : LoadingJobStateBase
    {
        public OnARM(LoadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();

            try
            {
                var ARM = Scheduler.CurrHolder as IARMModule;
                var CHUCK = Scheduler.Destination as IChuckModule;

                //=> Check Load ARM
                IARMModule loadARM = ARM;
                if (loadARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.LOADING) == false)
                {
                    loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                }
                if (loadARM == null)
                {
                    rel.SetError($"Can not found loadable ARM.");
                    return rel;
                }

                //=> Check Unload ARM
                if (CHUCK.Holder.Status == EnumSubsStatus.EXIST)
                {
                    //LoadARM을 제외한 UnloadARM 찾기
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                //=> Check PreAlign
                var usedPA = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.UsedPA) as IPreAlignable;
                if (Scheduler.TransferObject.NeedPreAlign() ||
                    Scheduler.TransferObject.NeedOCR() ||
                    usedPA.IsNeedRatateOffsetNotchAngle(Scheduler.TransferObject, CHUCK))
                {
                    IPreAlignable usablePA = Scheduler.Loader.ModuleManager.FindUsablePreAlignable(Scheduler.TransferObject);
                    if (usablePA == null)
                    {
                        rel.SetError($"Can not found loadable PreAlginer.");
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
                var loadARM = Scheduler.CurrHolder as IARMModule;
                var CHUCK = Scheduler.Destination as IChuckModule;

                var usedPA = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.UsedPA) as IPreAlignable;

                // NeedOCR 함수에서 확인 되므로, 따로 확인한 필요 없음. 코그넥스만을 확인하는 것도 수정되어야 함. 따라서 지워짐.
                //EnumCognexMode cognexMode = CognexProcessManager.CognexProcDevParam.Mode.Value;

                //if (Scheduler.TransferObject.NeedPreAlign() ||
                //    (Scheduler.TransferObject.NeedOCR() && cognexMode != EnumCognexMode.DISABLE) ||
                //    usedPA.IsNeedRatateOffsetNotchAngle(Scheduler.TransferObject, CHUCK))
                //{
                if (Scheduler.TransferObject.NeedPreAlign() ||
                    (Scheduler.TransferObject.NeedOCR()) ||
                    usedPA.IsNeedRatateOffsetNotchAngle(Scheduler.TransferObject, CHUCK))
                {
                    IPreAlignable usablePA = Scheduler.Loader.ModuleManager.FindUsablePreAlignable(Scheduler.TransferObject);

                    rel.SetTransfer(Scheduler.TransferObject, loadARM, usablePA, loadARM, Scheduler.Destination);
                    return rel;
                }

                if (CHUCK.Holder.Status == EnumSubsStatus.EXIST)
                {
                    //Chuck에 웨이퍼가 존재하므로 먼저 내린다.
                    var unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING);

                    rel.SetTransfer(CHUCK.Holder.TransferObject, CHUCK, unloadARM, unloadARM, Scheduler.Destination);
                    return rel;
                }

                rel.SetTransfer(Scheduler.TransferObject, loadARM, CHUCK, loadARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class

    public class OnPreAlign : LoadingJobStateBase
    {
        public OnPreAlign(LoadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                var CHUCK = Scheduler.Destination as IChuckModule;

                //=> Check Load ARM
                IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                if (loadARM == null)
                {
                    rel.SetError($"Can not found loadable ARM.");
                    return rel;
                }

                //=> Check Unload ARM
                if (CHUCK.Holder.Status == EnumSubsStatus.EXIST)
                {
                    //LoadARM을 제외한 UnloadARM 찾기
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.UNLOADING, loadARM);
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
                var PA = Scheduler.CurrHolder as IPreAlignModule;
                var CHUCK = Scheduler.Destination as IChuckModule;

                var loadARM = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.PrevHolder) as IARMModule;
                if (loadARM == null ||
                    loadARM.Holder.Status != EnumSubsStatus.NOT_EXIST ||
                    loadARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.LOADING) == false)//switch ARM
                {
                    //이전 ARM의 정보가 유효하지 않으므로
                    loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                }

                if (Scheduler.TransferObject.NeedPreAlign())
                {
                    rel.SetPreAlign(Scheduler.TransferObject, PA, loadARM, Scheduler.Destination);
                    return rel;
                }

                //EnumCognexMode cognexMode = CognexProcessManager.CognexProcDevParam.Mode.Value;

                if (Scheduler.TransferObject.NeedOCR())
                {
                    IOCRReadable usableOCR = Scheduler.Loader.ModuleManager.FindModules<IOCRReadable>().Where(
                        item =>
                        item.CanOCR(Scheduler.TransferObject) &&
                        item.GetDependecyPA() == PA
                        ).FirstOrDefault();

                    rel.SetTransfer(Scheduler.TransferObject, PA, usableOCR, loadARM, Scheduler.Destination);
                    return rel;
                }

                rel.SetTransfer(Scheduler.TransferObject, PA, loadARM, loadARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class

    public class OnOCR : LoadingJobStateBase
    {
        public OnOCR(LoadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                var ARM = Scheduler.CurrHolder as IARMModule;
                var OCR = Scheduler.CurrPos as IOCRReadable;
                var CHUCK = Scheduler.Destination as IChuckModule;

                //=> Check Load ARM
                IARMModule loadARM = ARM;
                if (loadARM.Definition.UseType.Value.HasFlag(ARMUseTypeEnum.LOADING) == false)
                {
                    loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                }
                if (loadARM == null)
                {
                    rel.SetError($"Can not found loadable ARM.");
                    return rel;
                }

                //=> Check Unload ARM
                if (CHUCK.Holder.Status == EnumSubsStatus.EXIST)
                {
                    //LoadARM을 제외한 UnloadARM 찾기
                    IARMModule unloadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING, loadARM);
                    if (unloadARM == null)
                    {
                        rel.SetError($"Can not found unloadable ARM.");
                        return rel;
                    }
                }

                //=> Check PreAlign
                IPreAlignModule usablePA = OCR.GetDependecyPA();
                if (usablePA == null || usablePA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    rel.SetError($"Can not found loadable PreAlginer.");
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
                var loadARM = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.CurrHolder) as IARMModule;

                var usedPA = readable.GetDependecyPA();

                if (usedPA.IsNeedRatateOffsetNotchAngle(Scheduler.TransferObject, readable))
                {
                    rel.SetTransfer(Scheduler.TransferObject, readable, usedPA, loadARM, Scheduler.Destination);
                    return rel;
                }

                if (Scheduler.TransferObject.NeedOCR())
                {
                    rel.SetReadOCR(Scheduler.TransferObject, readable, loadARM, Scheduler.Destination);
                    return rel;
                }

                rel.SetTransfer(Scheduler.TransferObject, readable, usedPA, loadARM, Scheduler.Destination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class

    public class OnChuck : LoadingJobStateBase
    {
        public OnChuck(LoadingJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
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
                //=> clear options
                Scheduler.TransferObject.DisableOptionAll();

                if (Scheduler.Loader.LoaderOption.OptionFlag)
                {
                    Scheduler.TransferObject.OCRReadState = OCRReadStateEnum.NONE;
                }

                rel.SetJobDone();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class
}

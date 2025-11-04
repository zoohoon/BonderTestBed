using LoaderBase;
using LoaderParameters;
using LogModule;
using ProberInterfaces;
using System;

namespace LoaderCore.DescriptorJobStates
{
    public abstract class DescriptorJobStateBase
    {
        public DescriptorJob Scheduler { get; set; }

        public DescriptorJobStateBase(DescriptorJob scheduler)
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

    public class OnSLOT : DescriptorJobStateBase
    {
        public OnSLOT(DescriptorJob scheduler) : base(scheduler) { }

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

                if(Scheduler.CurrPos== Scheduler.Destination)
                {

                    rel.SetJobDone();
                    return rel;
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

    public class OnFixedTray : DescriptorJobStateBase
    {
        public OnFixedTray(DescriptorJob scheduler) : base(scheduler) { }

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
                if(Scheduler.CurrPos== Scheduler.Destination)
                {
                    rel.SetJobDone();
                    return rel;
                }
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


    public class OnBuffer : DescriptorJobStateBase
    {
        public OnBuffer(DescriptorJob scheduler) : base(scheduler) { }

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



                if (Scheduler.CurrPos == Scheduler.Destination)
                {
                    //=> clear options
                    //Scheduler.TransferObject.DisableOptionAll();

                    rel.SetJobDone();
                    return rel;
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

    public class OnInspectionTray : DescriptorJobStateBase
    {
        public OnInspectionTray(DescriptorJob scheduler) : base(scheduler) { }

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
                if (Scheduler.CurrPos == Scheduler.Destination)
                {
                    rel.SetJobDone();
                    return rel;
                }
                else if (Scheduler.Destination is IARMModule)
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

    public class OnChuck : DescriptorJobStateBase
    {
        public OnChuck(DescriptorJob scheduler) : base(scheduler) { }

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

                if(Scheduler.CurrPos== Scheduler.Destination)
                {
                    rel.SetJobDone();
                    return rel;
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

    public class OnARM : DescriptorJobStateBase
    {
        public OnARM(DescriptorJob scheduler) : base(scheduler) { }

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

                if (Scheduler.Destination is IBufferModule)
                {
                    IBufferModule dstBuffer = Scheduler.Destination as IBufferModule;
                    if (dstBuffer.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"Destination ARM status invalid.");
                        return rel;
                    }
                }

                if (Scheduler.Destination is IChuckModule)
                {
                    IChuckModule dstChuck = Scheduler.Destination as IChuckModule;
                    if (dstChuck.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"Destination ARM status invalid.");
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


                //rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useARM, useARM, Scheduler.Destination);
                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, Scheduler.Destination, ARM, Scheduler.Destination);
                return rel;

              //  rel.SetError($"Request invalid.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class

    public class OnPreAlign : DescriptorJobStateBase
    {
        public OnPreAlign(DescriptorJob scheduler) : base(scheduler) { }

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

                //if(PA.Holder.TransferObject.OCRReadState == OCRReadStateEnum.NONE && PA.Holder.TransferObject.OCRMode.Value == OCRModeEnum.READ)
                //{
                //    IOCRReadable OcrReadable = null;
                //    if (PA.Holder.TransferObject.OCRType.Value==OCRTypeEnum.COGNEX)
                //    {
                //       OcrReadable=Scheduler.Loader.ModuleManager.FindModule(ModuleTypeEnum.COGNEXOCR, PA.ID.Index) as IOCRReadable;
                //    }else if(PA.Holder.TransferObject.OCRType.Value == OCRTypeEnum.SEMICS)
                //    {
                //        OcrReadable = Scheduler.Loader.ModuleManager.FindModule(ModuleTypeEnum.SEMICSOCR, PA.ID.Index) as IOCRReadable;
                //    }

                //    var useARM = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.PrevHolder) as IARMModule;
                //    if (useARM == null || useARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                //    {
                //        useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    }

                //    if (PA != OcrReadable.GetDependecyPA())
                //    {
                //        rel.SetError();
                //        return rel;
                //    }
                //    else
                //    {
                //        rel.SetTransfer(Scheduler.TransferObject, PA, OcrReadable, useARM, Scheduler.Destination, PA.Holder.TransferObject.OCRReadState);
                //        return rel;
                //    }
                //}

                if (DestPos == Scheduler.CurrHolder|| (DestPos is ICognexOCRModule && Scheduler.TransferObject.IsOCRDone()))
                    //||(PA.Holder.TransferObject.OCRReadState==OCRReadStateEnum.DONE||PA.Holder.TransferObject.OCRMode.Value==OCRModeEnum.NONE))
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
                if (DestPos is ICognexOCRModule)
                {
                    var useOCR = Scheduler.Destination as ICognexOCRModule;
                    var useARM = Scheduler.Destination as IARMModule;
                    rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrPos, useOCR, useARM, Scheduler.Destination);
                    return rel;
                }
                //if (DestPos is IOCRReadable)
                //{
                //    var OcrReadable = Scheduler.Destination as IOCRReadable;

                //    var useARM = Scheduler.Loader.ModuleManager.FindModule(Scheduler.TransferObject.PrevHolder) as IARMModule;
                //    if (useARM == null || useARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                //    {
                //        useARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    }

                //    if (PA != OcrReadable.GetDependecyPA())
                //    {
                //        rel.SetError();
                //        return rel;
                //    }
                //    else
                //    {
                //        rel.SetTransfer(Scheduler.TransferObject, PA, OcrReadable, useARM, Scheduler.Destination, PA.Holder.TransferObject.OCRReadState);
                //        return rel;
                //    }
                //}

                rel.SetError("Not supported.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class

    public class OnOCR : DescriptorJobStateBase
    {
        public OnOCR(DescriptorJob scheduler) : base(scheduler) { }

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
                if (Scheduler.TransferObject.NeedPreAlign() )
                    //||usedPA.IsNeedRatateOffsetNotchAngle(Scheduler.TransferObject, OcrReadable))
                {
                    IPreAlignModule PA = OcrReadable.GetDependecyPA();

                    rel.SetTransfer(Scheduler.TransferObject, OcrReadable, PA, useARM, Scheduler.Destination);
                    return rel;
                }

                if (Scheduler.TransferObject.NeedOCR())
                {
                    IPreAlignModule PA = OcrReadable.GetDependecyPA();
                    rel.SetReadOCR(Scheduler.TransferObject, OcrReadable, PA, Scheduler.Destination, Scheduler.TransferObject.OCRReadState);
                    return rel;
                }

                if (DestPos == Scheduler.CurrPos)
                {
                    //=> clear options
                    Scheduler.TransferObject.DisableOptionAll();

                    rel.SetJobDone();
                    return rel;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }

    }//end of state class



    public class OnCardTray : DescriptorJobStateBase
    {
        public OnCardTray(DescriptorJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                if (Scheduler.CardDestination is ICardARMModule)
                {
                    var Arm = Scheduler.CardDestination as ICardARMModule;
                    if (Arm.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"Card Arm Status Invalid.");
                        return rel;
                    }
                }

                //if (Scheduler.Destination is IPreAlignable)
                //{
                //    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    if (loadARM == null)
                //    {
                //        rel.SetError($"Can not found loadable ARM.");
                //        return rel;
                //    }

                //    var PA = Scheduler.Destination as IPreAlignable;
                //    if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                //    {
                //        rel.SetError($"PA status invalid.");
                //        return rel;
                //    }
                //}

                //if (Scheduler.Destination is IOCRReadable)
                //{
                //    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    if (loadARM == null)
                //    {
                //        rel.SetError($"Can not found loadable ARM.");
                //        return rel;
                //    }

                //    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                //    if (usablePA == null)
                //    {
                //        rel.SetError($"Can not found loadable PreAligner.");
                //        return rel;
                //    }
                //}

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
                if (Scheduler.CurrCardPos == Scheduler.CardDestination)
                {
                    rel.SetJobDone();
                    return rel;
                }

                ICardARMModule useARM=null;
                if (Scheduler.CardDestination is ICardARMModule)
                {
                    useARM = Scheduler.CardDestination as ICardARMModule;
                }
                else
                {
                }
                // rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrCardPos, useARM, useARM, Scheduler.Destination);
                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrCardPos, useARM, useARM, Scheduler.CardDestination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }

    }//end of state class
    public class OnCardChange : DescriptorJobStateBase
    {
        public OnCardChange(DescriptorJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                if (Scheduler.CardDestination is ICardARMModule)
                {
                    var Arm = Scheduler.CardDestination as ICardARMModule;
                    if (Arm.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"Card Arm Status Invalid.");
                        return rel;
                    }
                }

                //if (Scheduler.Destination is IPreAlignable)
                //{
                //    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    if (loadARM == null)
                //    {
                //        rel.SetError($"Can not found loadable ARM.");
                //        return rel;
                //    }

                //    var PA = Scheduler.Destination as IPreAlignable;
                //    if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                //    {
                //        rel.SetError($"PA status invalid.");
                //        return rel;
                //    }
                //}

                //if (Scheduler.Destination is IOCRReadable)
                //{
                //    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    if (loadARM == null)
                //    {
                //        rel.SetError($"Can not found loadable ARM.");
                //        return rel;
                //    }

                //    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                //    if (usablePA == null)
                //    {
                //        rel.SetError($"Can not found loadable PreAligner.");
                //        return rel;
                //    }
                //}

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
                ICardARMModule useARM = null;
                if (Scheduler.CardDestination is ICardARMModule)
                {
                    useARM = Scheduler.CardDestination as ICardARMModule;

                }
                if (Scheduler.CurrCardPos == Scheduler.CardDestination)
                {
                    rel.SetJobDone();
                    return rel;
                }else if (useARM != null&&useARM.Holder.Status == EnumSubsStatus.CARRIER)
                {
                    this.Scheduler.TransferObject = useARM.Holder.TransferObject;

                    rel.SetJobDone();
                    return rel;
                }
               
                // rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrCardPos, useARM, useARM, Scheduler.Destination);
                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrCardPos, useARM, useARM, Scheduler.CardDestination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }

    }//end of state class


    public class OnCardBuffer : DescriptorJobStateBase
    {
        public OnCardBuffer(DescriptorJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                if (Scheduler.CardDestination is ICardARMModule)
                {
                    var Arm = Scheduler.CardDestination as ICardARMModule;
                    if (Arm.Holder.Status != EnumSubsStatus.NOT_EXIST)
                    {
                        rel.SetError($"Card Arm Status Invalid.");
                        return rel;
                    }
                }

                //if (Scheduler.Destination is IPreAlignable)
                //{
                //    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    if (loadARM == null)
                //    {
                //        rel.SetError($"Can not found loadable ARM.");
                //        return rel;
                //    }

                //    var PA = Scheduler.Destination as IPreAlignable;
                //    if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                //    {
                //        rel.SetError($"PA status invalid.");
                //        return rel;
                //    }
                //}

                //if (Scheduler.Destination is IOCRReadable)
                //{
                //    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    if (loadARM == null)
                //    {
                //        rel.SetError($"Can not found loadable ARM.");
                //        return rel;
                //    }

                //    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                //    if (usablePA == null)
                //    {
                //        rel.SetError($"Can not found loadable PreAligner.");
                //        return rel;
                //    }
                //}

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
                if (Scheduler.CurrCardPos == Scheduler.CardDestination)
                {

                    rel.SetJobDone();
                    return rel;
                }
                ICardARMModule useARM = null;
                if (Scheduler.CardDestination is ICardARMModule)
                {
                    useARM = Scheduler.CardDestination as ICardARMModule;
                }
                else
                {
                }
                // rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrCardPos, useARM, useARM, Scheduler.Destination);
                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrCardPos, useARM, useARM, Scheduler.CardDestination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }

    }//end of state class


    public class OnCardARM : DescriptorJobStateBase
    {
        public OnCardARM(DescriptorJob scheduler) : base(scheduler) { }

        public override JobValidateResult Validate()
        {
            JobValidateResult rel = new JobValidateResult();
            try
            {
                //if (Scheduler.Destination is IARMModule)
                //{
                //    var ARM = Scheduler.Destination as IARMModule;
                //    if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                //    {
                //        rel.SetError($"ARM status invalid.");
                //        return rel;
                //    }
                //}

                //if (Scheduler.Destination is IPreAlignable)
                //{
                //    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    if (loadARM == null)
                //    {
                //        rel.SetError($"Can not found loadable ARM.");
                //        return rel;
                //    }

                //    var PA = Scheduler.Destination as IPreAlignable;
                //    if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                //    {
                //        rel.SetError($"PA status invalid.");
                //        return rel;
                //    }
                //}

                //if (Scheduler.Destination is IOCRReadable)
                //{
                //    IARMModule loadARM = Scheduler.Loader.ModuleManager.FindUsableARM(ARMUseTypeEnum.LOADING);
                //    if (loadARM == null)
                //    {
                //        rel.SetError($"Can not found loadable ARM.");
                //        return rel;
                //    }

                //    IPreAlignModule usablePA = Scheduler.Loader.ModuleManager.FindUsableModule<IPreAlignModule>();
                //    if (usablePA == null)
                //    {
                //        rel.SetError($"Can not found loadable PreAligner.");
                //        return rel;
                //    }
                //}

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
                if (Scheduler.CurrCardPos == Scheduler.CardDestination)
                {

                    rel.SetJobDone();
                    return rel;
                }
                ICardARMModule useARM = null;
              
                    useARM = Scheduler.CurrCardPos as ICardARMModule;
               
            
                // rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrCardPos, useARM, useARM, Scheduler.Destination);
                rel.SetTransfer(Scheduler.TransferObject, Scheduler.CurrCardPos, Scheduler.CardDestination, useARM, Scheduler.CardDestination);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }

    }//end of state class
}

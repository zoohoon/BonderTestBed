using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NotifyEventModule;
using System.Threading;
using ProberInterfaces.Event;

namespace LoaderCore.GP_SlotToARMStates
{
    public abstract class GP_SlotToARMState : LoaderProcStateBase
    {
        public GP_SlotToARM Module { get; set; }

        public GP_SlotToARMState(GP_SlotToARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_SlotToARMState stateObj)
        {
            try
            {

                Module.StateObj = stateObj;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state transition : {State}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        ILoaderModule loader;
        protected ILoaderModule Loader
        {
            get
            {
                if (loader == null)
                {
                    loader = Module.Container.Resolve<ILoaderModule>();
                }
                return loader;
            }
        }
        protected ISlotModule SLOT => Module.Param.Curr as ISlotModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;

        protected ICassetteModule Cassette => SLOT.Cassette;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_SlotToARMState
    {
        public IdleState(GP_SlotToARM module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.SLOT_TO_ARM;

                Loader.ProcModuleInfo.Source = SLOT.ID;
                Loader.ProcModuleInfo.Destnation = ARM.ID;
                Loader.ProcModuleInfo.Origin = SLOT.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_SlotToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new RunningState(Module));
        }
    }
    public class RunningState : GP_SlotToARMState
    {
        public RunningState(GP_SlotToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.SLOT_TO_ARM, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(SLOT.Holder.TransferObject.OriginHolder)}, Source: {Loader.SlotToFoupConvert(SLOT.ID)}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.SLOT_TO_ARM, StateLogType.START, SLOT.ID.Label, ARM.ID.Label, SLOT.Holder.TransferObject.OriginHolder.Label);

                ISlotModule slot = (ISlotModule)Module.Param.Curr;

                string OCRIDfromHost = "";

                if (slot.Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD&& slot.Cassette.ScanState==LoaderParameters.CassetteScanStateEnum.READ)
                {
                    int foupnum = ((SLOT.Holder.TransferObject.OriginHolder.Index - 1) / 25) + 1; 
                    int slotnum = (SLOT.Holder.TransferObject.OriginHolder.Index % 25 == 0) ? 25 : SLOT.Holder.TransferObject.OriginHolder.Index % 25;
                    PIVInfo pivinfo = new PIVInfo(foupnumber: foupnum, preloadingSlotIndex: slotnum, foupshiftmode: (int)Loader.LoaderMaster.GetFoupShiftMode());

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(CassetteToArmEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                    var result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, SLOT.Holder.Status);
                    if (result != EventCodeEnum.NONE)
                    {
                        // Wafer obj 에 정보 이상일 수 있음.
                        SLOT.Holder.SetUnknown();
                        Loader.ResonOfError = $"SLOT{SLOT.ID.Index} To ARM{ARM.ID.Index} Transfer failed. Job result = {result}";
                        LoggerManager.Error($"GP_SlotToARMState(): Transfer failed. Job result = {result}");
                        Loader.BroadcastLoaderInfo();
                        StateTransition(new SystemErrorState(Module));
                    }
                    else
                    {
                        result = this.GetLoaderCommands().CassettePick((ISlotModule)Module.Param.Curr, Module.Param.UseARM);
                        if (result == EventCodeEnum.NONE)
                        {
                            //string tempHash = SLOT.Holder.TransferObject.CST_HashCode.ToString();
                            Loader.LoaderMaster.UsingPMICalc(SLOT);

                            if (SLOT.Holder.TransferObject.OCR.Value != "" && SLOT.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                            {
                                OCRIDfromHost = SLOT.Holder.TransferObject.OCR.Value;
                            }
                            SLOT.Holder.CurrentWaferInfo = SLOT.Holder.TransferObject;
                            SLOT.Holder.TransferObject.SetOCRState(OCRIDfromHost, 0, ProberInterfaces.Enum.OCRReadStateEnum.NONE);
                            SLOT.Holder.SetTransfered(ARM);


                            ////TODO: Eap쪽에서는 CompleateEvent(9022: 웨이퍼가 모두 테스트가 끝나서 슬롯으로 들어옴)를 보고 Undock가능상태를 처리한다고함.
                            ////      foupshift 에서는 웨이퍼가 다 빠지기만 하면 언로드 가능해야함. 타이밍은 slotToArm에 있어야할거같긴한데 
                            ///        OPUS3에서는 OCR실패했을때 LotEnd 되고 풉이 있을경우 웨이퍼 모두 복귀시키고 끝나고 풉이 없을경우에는 그냥 그자리에서 랏드 엔드됨.
                            ///        ** 맨첫 2장만 가지고 있는 CST할 경우
                            if (Loader.LoaderMaster.GetFoupShiftMode() == FoupShiftModeEnum.SHIFT)
                            {
                                if (foupnum != 0)
                                {
                                    if (CanFoupUnload(foupnum))
                                    {
                                        this.FoupOpModule().FoupControllers[foupnum - 1].SetLock(false);
                                        pivinfo = new PIVInfo(foupnumber: foupnum);
                                        semaphore = new SemaphoreSlim(0);
                                        this.EventManager().RaisingEvent(typeof(NotifyEventModule.CanFoupUnloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));// Unload할 웨이퍼가 0개 일때는 안불림
                                        semaphore.Wait();
                                    }
                                }
                            }
                            Loader.BroadcastLoaderInfo();

                            StateTransition(new DoneState(Module));
                        }
                        else
                        {
                            Loader.ResonOfError = "CassettePick Error. result:" + result.ToString();

                            LoggerManager.Error($"GP_SlotToARMState(): Transfer failed. Job result = {result}");
                            Loader.ResonOfError = $"{Loader.SlotToFoupConvert(SLOT.ID)} To ARM{ARM.ID.Index} Transfer failed. {Environment.NewLine} Job result = {result}";
                            result = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                            if (result == EventCodeEnum.NONE) // arm에 있을경우 없을 경우
                            {
                                SLOT.Holder.SetTransfered(ARM);
                            }
                            Loader.BroadcastLoaderInfo();
                            StateTransition(new SystemErrorState(Module));
                        }
                    }
                }
                else
                {
                    Loader.ResonOfError = $"GP_SlotToARMState Error : FoupState={slot.Cassette.FoupState} ScanState={slot.Cassette.ScanState}";

                    LoggerManager.Error($"GP_SlotToARMState(): Transfer failed. ResonOfError = {Loader.ResonOfError}");
                 
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                TransferObject clonedTransferObject = null;
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                }
                else if (SLOT.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = SLOT.Holder.TransferObject.Clone() as TransferObject;
                }

                LoggerManager.Error($"GP_SlotToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                Loader.ResonOfError = $"{Loader.SlotToFoupConvert(SLOT.ID)} To ARM{ARM.ID.Index} Transfer failed. {Environment.NewLine} Job result = {err.Message}";
                var result = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                if (result == EventCodeEnum.NONE) // arm에 있을경우 없을 경우
                {
                    if (SLOT.Holder.Status == EnumSubsStatus.EXIST)
                        SLOT.Holder.SetTransfered(ARM);
                }
                else 
                {
                    ARM.Holder.SetUnknown(clonedTransferObject);
                    SLOT.Holder.SetUnknown(clonedTransferObject);
                }
                Loader.BroadcastLoaderInfo();
                StateTransition(new SystemErrorState(Module));
            }
        }

        private bool CanFoupUnload(int foupnum)
        {
            bool canExecute = false;
            try
            {
                // [FOUP_SHIFT]* Cassette가 빈 상태이면 EAP로 Event를 보낸다.
                var cassette = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupnum);
                if (cassette.ScanState == LoaderParameters.CassetteScanStateEnum.READ)
                {
                    //var active_assignedslotwafers = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll()
                    //                              .Where(item => item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                    //                                            Loader.LoaderMaster.ActiveLotInfos[foupnum - 1].UsingSlotList.Contains(((item.OriginHolder.Index % 25 == 0) ? 25 : item.OriginHolder.Index % 25)) 
                    //                                            //((item.OriginHolder.Index - 1) / 25) + 1 == foupnum
                    //                                             ).ToList();
                    ActiveLotInfo originLotInfo = null;
                    originLotInfo = Loader.LoaderMaster.ActiveLotInfos.Find(l => l.CST_HashCode == ARM.Holder.TransferObject.CST_HashCode);
                    if (originLotInfo == null)
                    {
                        originLotInfo = Loader.LoaderMaster.Prev_ActiveLotInfos.Find(l => l.CST_HashCode == ARM.Holder.TransferObject.CST_HashCode);
                    }

                    var active_assignedslotwafers = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll()
                                                  .Where(item => item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                                item.CST_HashCode == ARM.Holder.TransferObject.CST_HashCode &&
                                                                originLotInfo.UsingSlotList.Contains(((item.OriginHolder.Index % 25 == 0) ? 25 : item.OriginHolder.Index % 25))).ToList();
                    



                    var processedWafersInCst = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll().Where(item => item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                                                        ((item.CurrHolder.Index - 1) / 25) + 1 == foupnum &&
                                                                                        item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                                                        item.WaferState != EnumWaferState.UNPROCESSED).ToList();

                    var active_InCstWafers = active_assignedslotwafers.Where(x => (x.CurrHolder.ModuleType == ModuleTypeEnum.SLOT) && (x.WaferState == EnumWaferState.UNPROCESSED)).ToList();
                    if (active_InCstWafers.Count == 0 && processedWafersInCst.Count == 0)
                    {
                        canExecute = true;
                        return canExecute;
                    }


                    //var active_unprocessewafers = active_assignedslotwafers
                    //                              .Where(item => item.WaferState == EnumWaferState.UNPROCESSED &&
                    //                                             item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT
                    //                                            ).ToList();


                    //var active_processewafers = active_assignedslotwafers
                    //                              .Where(item => item.WaferState != EnumWaferState.UNPROCESSED &&
                    //                                             item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT
                    //                                            ).ToList();



                    //List<ActiveLotInfo> prev_Lots = Loader.LoaderMaster.Prev_ActiveLotInfos.Where(lot => lot.FoupNumber == foupnum).ToList();
                    //if (prev_Lots.Count == 0)
                    //{
                    //    if (active_unprocessewafers.Count == 0 && active_processewafers.Count == 0)
                    //    {
                    //        canExecute = true;
                    //        return canExecute;
                    //    }
                    //}



                    //else
                    //{
                    //    foreach (var prev_Lot in prev_Lots)
                    //    {
                    //        var prev_existwafers = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll()
                    //                            .Where(item => item.CST_HashCode == prev_Lot.CST_HashCode).ToList();

                        //        if (prev_existwafers.Count > 0)
                        //        {
                        //            var prev_home_wafers = prev_existwafers
                        //                                     .Where(item => item.WaferState != EnumWaferState.UNPROCESSED &&
                        //                                                    item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT
                        //                                           ).ToList();
                        //            if (prev_home_wafers.Count > 0)
                        //            {
                        //                if (prev_home_wafers.Count == prev_existwafers.Count)
                        //                {
                        //                    canExecute = true;
                        //                    return canExecute;
                        //                }
                        //            }
                        //        }

                        //    }
                        //}

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return canExecute;

        }
    }
    public class DoneState : GP_SlotToARMState
    {
        public DoneState(GP_SlotToARM module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.SLOT_TO_ARM, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {Loader.SlotToFoupConvert(SLOT.ID)}, DestinationHolder: {ARM}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.SLOT_TO_ARM, StateLogType.DONE, SLOT.ID.Label, ARM.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_SlotToARMState
    {
        public SystemErrorState(GP_SlotToARM module) : base(module)
        {
            try
            {
                TransferObject transObj = null;
                if (ARM.Holder.TransferObject != null)
                {
                    transObj = ARM.Holder.TransferObject;
                }
                else
                {
                    transObj = SLOT.Holder.TransferObject;
                }
                EventCodeEnum errorcode = EventCodeEnum.LOADER_SLOT_TO_ARM_TRANSFER_ERROR;
                LoggerManager.ActionLog(ModuleLogType.SLOT_TO_ARM, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {Loader.SlotToFoupConvert(SLOT.ID)}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.SLOT_TO_ARM, StateLogType.ERROR, SLOT.ID.Label, ARM.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
                this.NotifyManager().Notify(errorcode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}

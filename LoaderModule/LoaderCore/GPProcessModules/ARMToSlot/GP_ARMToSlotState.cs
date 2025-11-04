using Autofac;
using LoaderBase;
using LoaderParameters;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LoaderCore.GP_ARMToSlotStates
{
    public abstract class GP_ARMToSlotState : LoaderProcStateBase
    {
        public GP_ARMToSlot Module { get; set; }

        public GP_ARMToSlotState(GP_ARMToSlot module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_ARMToSlotState stateObj)
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
        protected IARMModule ARM => Module.Param.Curr as IARMModule;

        protected ISlotModule SLOT => Module.Param.Next as ISlotModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName] string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_ARMToSlotState
    {
        public IdleState(GP_ARMToSlot module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.ARM_TO_SLOT;
                Loader.ProcModuleInfo.Source = ARM.ID;
                Loader.ProcModuleInfo.Destnation = SLOT.ID;
                Loader.ProcModuleInfo.Origin = ARM.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToSlotState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{ARM.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {Loader.SlotToFoupConvert(SLOT.ID)}");

                StateTransition(new RunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToSlotState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RunningState : GP_ARMToSlotState
    {
        public RunningState(GP_ARMToSlot module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            //v22_merge// 코드 검토 필요
            try
            {
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_SLOT, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {Loader.SlotToFoupConvert(SLOT.ID)}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_SLOT, StateLogType.START, ARM.ID.Label, SLOT.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);


                
                ISlotModule slot = (ISlotModule)Module.Param.DestPos;
                if (slot.Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD && slot.Cassette.ScanState == CassetteScanStateEnum.READ)
                {
                    var result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, ARM.Holder.Status);
                    if (result != EventCodeEnum.NONE)
                    {
                        // Wafer obj 에 정보 이상일 수 있음.
                        ARM.Holder.SetUnknown();
                        Loader.ResonOfError = $"ARM{ARM.ID.Index} To SLOT{SLOT.ID.Index} Transfer failed. Job result = {result}";
                        LoggerManager.Error($"GP_ARMToSlotState(): Transfer failed. Job result = {result}");
                        Loader.BroadcastLoaderInfo();
                        StateTransition(new SystemErrorState(Module));
                    }
                    else
                    {
                        result = this.GetLoaderCommands().CassettePut(Module.Param.UseARM, slot);
                        if (result == EventCodeEnum.NONE)
                        {
                            if (ARM.Holder.TransferObject.OCRReadState == ProberInterfaces.Enum.OCRReadStateEnum.FAILED)
                            {
                                ARM.Holder.TransferObject.OCRReadState = ProberInterfaces.Enum.OCRReadStateEnum.NONE;
                            }
                            ModuleID preOrigin = new ModuleID(ARM.Holder.TransferObject.OriginHolder.ModuleType,
                                ARM.Holder.TransferObject.OriginHolder.Index,
                                ARM.Holder.TransferObject.OriginHolder.Label);

                            ARM.Holder.TransferObject.OriginHolder = SLOT.ID;

                            ARM.Holder.TransferObject.PreAlignState = PreAlignStateEnum.NONE;

                            ARM.Holder.CurrentWaferInfo = ARM.Holder.TransferObject;
                            ARM.Holder.SetTransfered(SLOT);


                            int foupNum = ((SLOT.Holder.TransferObject.OriginHolder.Index - 1) / 25) + 1;
                            int slotNum = (SLOT.Holder.TransferObject.OriginHolder.Index % 25 == 0) ? 25 : SLOT.Holder.TransferObject.OriginHolder.Index % 25;
                            PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum, lotid: SLOT.Holder.TransferObject.LOTID, waferid: SLOT.Holder.TransferObject.OCR.Value, slotnumber: slotNum,
                                                          unloadingslotnum: (SLOT.ID.Index % 25 == 0) ? 25 : SLOT.ID.Index % 25, unloadingwid: SLOT.Holder.TransferObject.OCR.Value);

                            if (SLOT.Holder.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(WaferUnloadedToSlotEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();

                                if (Loader.LoaderMaster.GetFoupShiftMode() == FoupShiftModeEnum.SHIFT)
                                {
                                    if (Loader.LoaderMaster.IsSameDeviceEndToSlot(SLOT.Holder.TransferObject.DeviceName.Value, SLOT.Holder.TransferObject.UsingStageList) == true)
                                    {
                                        semaphore = new SemaphoreSlim(0);
                                        this.EventManager().RaisingEvent(typeof(SameDeviceEndToSlot).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                        semaphore.Wait();
                                    }
                                }
                            }


                            int oldfoupNum = 0;
                            int oldslotNum = 0;
                            if ((SLOT.Holder.TransferObject.OriginHolder != preOrigin))
                            {
                                ModuleID old_ID = preOrigin;
                                var new_ID = SLOT.ID;
                                ICassetteModule cstModule = Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, 1) as ICassetteModule;
                                var slotCount = Loader.ModuleManager.FindSlots(cstModule).Count();
                                oldslotNum = old_ID.Index % slotCount;
                                int oldoffset = 0;
                                if (oldslotNum == 0)
                                {
                                    oldslotNum = slotCount;
                                    oldoffset = -1;
                                }
                                oldfoupNum = ((old_ID.Index + oldoffset) / slotCount) + 1;

                                int newslotNum = new_ID.Index % slotCount;
                                int newoffset = 0;
                                if (newslotNum == 0)
                                {
                                    newslotNum = slotCount;
                                    newoffset = -1;
                                }
                                int newfoupNum = ((new_ID.Index + newoffset) / slotCount) + 1;

                                // Slot으로 들어오는 Wafer의 Origin이 SLOT이 아닌 경우(InspectionTray, FixedTray 등) Skip처리 해주기 위함.
                                if (preOrigin.ModuleType != ModuleTypeEnum.SLOT)
                                {
                                    if (Loader.Foups[newfoupNum - 1].LotState == LotStateEnum.Running)
                                    {
                                        if (SLOT.Holder.TransferObject.WaferState != EnumWaferState.PROCESSED)
                                        {
                                            Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = EnumWaferState.SKIPPED;
                                            Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferObj = SLOT.Holder.TransferObject;
                                            Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = EnumWaferState.SKIPPED;
                                            SLOT.Holder.TransferObject.WaferState = EnumWaferState.SKIPPED;
                                        }
                                    }
                                    else
                                    {
                                        if (String.IsNullOrEmpty(SLOT.Holder.TransferObject.LOTID))
                                        {
                                            Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = EnumWaferState.UNPROCESSED;
                                            Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferObj = SLOT.Holder.TransferObject;
                                            Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = EnumWaferState.UNPROCESSED;
                                            SLOT.Holder.TransferObject.WaferState = EnumWaferState.UNPROCESSED;
                                        }
                                    }
                                    ICassetteModule newcst = Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, newfoupNum) as ICassetteModule;
                                    SLOT.Holder.TransferObject.CST_HashCode = newcst.HashCode;
                                }
                                else
                                {
                                    if (oldfoupNum == newfoupNum && oldslotNum == newslotNum) // Origin 위치랑 unload할 위치랑 같을때에는 데이터 넣어줄 필요 없음
                                    {

                                    }
                                    else
                                    {
                                        ICassetteModule cst = Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, oldfoupNum) as ICassetteModule;
                                        ICassetteModule newcst = Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, newfoupNum) as ICassetteModule;
                                        var slots = Loader.ModuleManager.FindSlots(cst);
                                        if (slots[slotCount - oldslotNum].Holder.Status == EnumSubsStatus.NOT_EXIST)
                                        {
                                            Loader.Foups[oldfoupNum - 1].Slots[slotCount - oldslotNum].WaferObj = null;
                                            Loader.Foups[oldfoupNum - 1].Slots[slotCount - oldslotNum].WaferStatus = EnumSubsStatus.NOT_EXIST;
                                        }

                                        Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferObj = SLOT.Holder.TransferObject;
                                        Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferStatus = EnumSubsStatus.EXIST;

                                        if (SLOT.Holder.TransferObject != null)
                                        {
                                            Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = SLOT.Holder.TransferObject.WaferState;
                                        }
                                        else
                                        {
                                            Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = EnumWaferState.UNDEFINED;
                                            LoggerManager.Debug($"Foup #{newfoupNum}, Slot #{newslotNum} Wafer State set to UNDEFINED. because TransferObject is null");
                                        }

                                        if (Loader.Foups[newfoupNum - 1].LotState == LotStateEnum.Running)
                                        {
                                            if (SLOT.Holder.TransferObject.WaferState != EnumWaferState.PROCESSED)
                                            {
                                                Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = EnumWaferState.SKIPPED;
                                                SLOT.Holder.TransferObject.WaferState = EnumWaferState.SKIPPED;
                                            }
                                        }
                                        else
                                        {
                                            if (String.IsNullOrEmpty(SLOT.Holder.TransferObject.LOTID))
                                            {
                                                Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = EnumWaferState.UNPROCESSED;
                                                SLOT.Holder.TransferObject.WaferState = EnumWaferState.UNPROCESSED;
                                            }
                                        }
                                        SLOT.Holder.TransferObject.CST_HashCode = newcst.HashCode;
                                    }
                                }

                                ////TODO: Eap쪽에서는 CompleateEvent(9022: 웨이퍼가 모두 테스트가 끝나서 슬롯으로 들어옴)를 보고 Undock가능상태를 처리한다고함.
                                ////      foupshift 에서는 웨이퍼가 다 빠지기만 하면 언로드 가능해야함. 타이밍은 slotToArm에 있어야할거같긴한데 
                                ///        OPUS3에서는 OCR실패했을때 LotEnd 되고 풉이 있을경우 웨이퍼 모두 복귀시키고 끝나고 풉이 없을경우에는 그냥 그자리에서 랏드 엔드됨.
                                ///        ** 두번째 이상의 카세트의 경우, 2장을 가지고 와서 스왑해서 다한 웨이퍼를 가져가는 CST일 경우
                                if (Loader.LoaderMaster.GetFoupShiftMode() == FoupShiftModeEnum.SHIFT)
                                {
                                    Loader.Foups[foupNum - 1].Slots[25 - slotNum].WaferObj = null;
                                    Loader.Foups[foupNum - 1].Slots[25 - slotNum].WaferObj = SLOT.Holder.TransferObject;
                                    if (foupNum != 0)
                                    {
                                        if (CanFoupUnload(foupNum))
                                        {
                                            this.FoupOpModule().FoupControllers[foupNum - 1].SetLock(false);
                                            //PIVInfo pivinfo = new PIVInfo(foupnumber: foupnum);// #FoupShift 검토 필요.
                                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                            this.EventManager().RaisingEvent(typeof(NotifyEventModule.CanFoupUnloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));// Unload할 웨이퍼가 0개 일때는 안불림
                                            semaphore.Wait();
                                        }

                                    }
                                }
                            }
                            else
                            {
                                ModuleID old_ID = preOrigin;
                                var new_ID = SLOT.ID;
                                ICassetteModule cstModule = Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, 1) as ICassetteModule;
                                var slotCount = Loader.ModuleManager.FindSlots(cstModule).Count();
                                oldslotNum = old_ID.Index % slotCount;
                                int oldoffset = 0;
                                if (oldslotNum == 0)
                                {
                                    oldslotNum = slotCount;
                                    oldoffset = -1;
                                }
                                oldfoupNum = ((old_ID.Index + oldoffset) / slotCount) + 1;

                                int newslotNum = new_ID.Index % slotCount;
                                int newoffset = 0;
                                if (newslotNum == 0)
                                {
                                    newslotNum = slotCount;
                                    newoffset = -1;
                                }
                                int newfoupNum = ((new_ID.Index + newoffset) / slotCount) + 1;

                                if (Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferStatus != EnumSubsStatus.EXIST)
                                {
                                    Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferObj = SLOT.Holder.TransferObject;
                                    Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferStatus = EnumSubsStatus.EXIST;
                                    if (SLOT.Holder.TransferObject != null)
                                    {
                                        Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = SLOT.Holder.TransferObject.WaferState;
                                    }
                                    else
                                    {
                                        Loader.Foups[newfoupNum - 1].Slots[slotCount - newslotNum].WaferState = EnumWaferState.UNDEFINED;
                                        LoggerManager.Debug($"Foup #{newfoupNum}, Slot #{newslotNum} Wafer State set to UNDEFINED. because TransferObject is null");
                                    }
                                }
                            }

                            if (SLOT.Holder.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                ActiveLotInfo lotinfo = this.Loader.LoaderMaster.ActiveLotInfos.Find(info => info.FoupNumber == oldfoupNum && info.LotID != null && info.LotID.Equals(SLOT.Holder.TransferObject.LOTID));
                                if (lotinfo == null)
                                {
                                    lotinfo = this.Loader.LoaderMaster.Prev_ActiveLotInfos.Find(info => info.FoupNumber == oldfoupNum && info.LotID.Equals(SLOT.Holder.TransferObject.LOTID));
                                }

                                if (lotinfo != null)
                                {
                                    lotinfo.NotDoneSlotList.Remove(oldslotNum);
                                    if (lotinfo.NotDoneSlotList.Count() == 0)
                                    {
                                        PIVInfo lotEndpivinfo = new PIVInfo(lotid: SLOT.Holder.TransferObject.LOTID);
                                        SemaphoreSlim lotEndsemaphore = new SemaphoreSlim(0);
                                        this.EventManager().RaisingEvent(typeof(LotEndDueToUnloadAllWaferEvent).FullName, new ProbeEventArgs(this, lotEndsemaphore, lotEndpivinfo));
                                        lotEndsemaphore.Wait();
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"GP_ARMTOSlotState.Running State LOT INFO is null. Wafer ID : {SLOT.Holder.TransferObject.OCR.Value}, LOT ID : {SLOT.Holder.TransferObject.LOTID}, FOUP NUM : {foupNum}");
                                }
                            }

                            Loader.BroadcastLoaderInfo();

                            StateTransition(new DoneState(Module, preOrigin.Label));
                        }
                        else
                        {
                            Loader.ResonOfError = "CassettePut Error. result:" + result.ToString();
                            result = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                            if (result != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                            {
                                ARM.Holder.SetTransfered(SLOT);
                            }
                            Loader.BroadcastLoaderInfo();
                            LoggerManager.Error($"GP_ARMToSlotState(): Transfer failed. Job result = {result}");

                            Loader.ResonOfError = $"ARM{ARM.ID.Index} To {Loader.SlotToFoupConvert(SLOT.ID)} Transfer failed. {Environment.NewLine} Job result = {result}";
                            StateTransition(new SystemErrorState(Module));
                        }
                    }
                }
                else
                {
                    Loader.ResonOfError = $"GP_ARMToSlotState Error : FoupState={slot.Cassette.FoupState} ScanState={slot.Cassette.ScanState}";
                    LoggerManager.Error($"GP_ARMToSlotState(): Transfer failed. ResonOfError = {Loader.ResonOfError}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToSlotState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                Loader.ResonOfError = $"ARM{ARM.ID.Index} To {Loader.SlotToFoupConvert(SLOT.ID)} Transfer failed. {Environment.NewLine} Job result = {err.Message}";

                TransferObject clonedTransferObject = null;
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                }
                else if (SLOT.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = SLOT.Holder.TransferObject.Clone() as TransferObject;
                }

                var result = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                if (result == EventCodeEnum.NONE)
                {
                    if (SLOT.Holder.Status == EnumSubsStatus.EXIST)
                        SLOT.Holder.SetTransfered(ARM);
                }
                else
                {
                    SLOT.Holder.SetUnknown(clonedTransferObject);
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
                // [FOUP_SHIFT]* 
                // CST_Hashcode가 같고 UnProcessed 가 아닌 웨이퍼 한세트가 CST에 모두 들어왔으면 Unload가능한 상태가 된다.
                

                var cassette = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupnum);
                if (cassette.ScanState == LoaderParameters.CassetteScanStateEnum.READ)
                {
                    var pairwafers = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll().Where(w => w.CST_HashCode == SLOT.Holder.TransferObject.CST_HashCode);
                    var pairwafersInCst = pairwafers.Where(w => w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT);
                    var waferNotInCst = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll().Where(w => w.CurrHolder.ModuleType != ModuleTypeEnum.SLOT).Count();
                    if (pairwafers.Count() == pairwafersInCst.Count() && waferNotInCst == 0)//Loader.LoaderMaster.ActiveLotInfos[foupnum - 1].UsingSlotList.Count() == 0 && 
                    {
                        //if(Loader.LoaderMaster.ActiveLotInfos[foupnum - 1].State != LoaderParameters.LotStateEnum.Cancel)
                        //{
                        //    Loader.LoaderMaster.ActiveLotInfos[foupnum - 1].State = LoaderParameters.LotStateEnum.End;
                        //}                        
                        canExecute = true;
                        return canExecute;
                    }
                    //var active_assignedslotwafers = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll()
                    //                              .Where(item => item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                    //                                            Loader.LoaderMaster.ActiveLotInfos[foupnum - 1].UsingSlotList.Contains(((item.OriginHolder.Index % 25 == 0) ? 25 : item.OriginHolder.Index % 25)) &&
                    //                                            item.CST_HashCode == Loader.LoaderMaster.ActiveLotInfos[foupnum - 1].CST_HashCode
                    //                                             ).ToList();

                    ActiveLotInfo originLotInfo = null;
                    originLotInfo = Loader.LoaderMaster.ActiveLotInfos.Find(l => l.CST_HashCode == SLOT.Holder.TransferObject.CST_HashCode);
                    if (originLotInfo == null)
                    {
                        originLotInfo = Loader.LoaderMaster.Prev_ActiveLotInfos.Find(l => l.CST_HashCode == SLOT.Holder.TransferObject.CST_HashCode);
                    }

                    var active_assignedslotwafers = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll()
                                                  .Where(item => item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                                originLotInfo.UsingSlotList.Contains(((item.OriginHolder.Index % 25 == 0) ? 25 : item.OriginHolder.Index % 25)) &&
                                                                item.CST_HashCode == originLotInfo.CST_HashCode
                                                                 ).ToList();

                    var active_unprocessewafers = active_assignedslotwafers
                                                  .Where(item => item.WaferState == EnumWaferState.UNPROCESSED &&
                                                                item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT
                                                        ).ToList();

                    var active_home_wafers = active_assignedslotwafers
                                                 .Where(item => item.WaferState != EnumWaferState.UNPROCESSED &&
                                                                item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT
                                                         ).ToList();

                    if(active_unprocessewafers.Count == 0 && active_assignedslotwafers.Count == active_home_wafers.Count)// 현재 랏드에서 종료할때
                    {
                        canExecute = true;
                        return canExecute;
                    }


                    //List<ActiveLotInfo> prev_Lots = Loader.LoaderMaster.Prev_ActiveLotInfos.Where(lot => lot.CST_HashCode == originLotInfo.CST_HashCode).ToList();
                    //foreach (var prev_Lot in prev_Lots)
                    //{
                    //    var prev_existwafers = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll()
                    //                        .Where(item => item.CST_HashCode == prev_Lot.CST_HashCode &&
                    //                                        prev_Lot.UsingSlotList.Contains(((item.OriginHolder.Index % 25 == 0) ? 25 : item.OriginHolder.Index % 25)) )
                    //                        .ToList();

                    //    if (prev_existwafers.Count > 0)
                    //    {
                    //        var prev_home_wafers = prev_existwafers
                    //                                    .Where(item => item.WaferState != EnumWaferState.UNPROCESSED &&
                    //                                                item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT
                    //                                        ).ToList();
                    //        if (prev_home_wafers.Count > 0)
                    //        {
                    //            if (active_unprocessewafers.Count == 0 && prev_home_wafers.Count == prev_existwafers.Count)
                    //            {
                    //                canExecute = true;
                    //                return canExecute;
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
    public class DoneState : GP_ARMToSlotState
    {
        public DoneState(GP_ARMToSlot module, string oldOrigin = "") : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.ARM_TO_SLOT, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(SLOT.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {Loader.SlotToFoupConvert(SLOT.ID)}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_SLOT, StateLogType.DONE, ARM.ID.Label, SLOT.ID.Label, SLOT.Holder.TransferObject.OriginHolder.Label, old: oldOrigin);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_ARMToSlotState
    {
        public SystemErrorState(GP_ARMToSlot module) : base(module)
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

                Loader.LoaderMaster.NotifyManager().Notify(EventCodeEnum.ARM_TO_SLOT);

                EventCodeEnum errorcode = EventCodeEnum.LOADER_ARM_TO_SLOT_TRANSFER_ERROR;
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_SLOT, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {ARM}, DestinationHolder: {Loader.SlotToFoupConvert(SLOT.ID)}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_SLOT, StateLogType.ERROR, ARM.ID.Label, SLOT.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());

                if (Loader.LoaderMaster.DynamicMode == DynamicModeEnum.DYNAMIC)
                {
                    int foupNum = 0;
                    if (transObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        var slotCount = 25;
                        int slotNum = transObj.OriginHolder.Index % slotCount;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = 25;
                            offset = -1;
                        }
                        foupNum = ((transObj.OriginHolder.Index + offset) / slotCount) + 1;
                    }
                    PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum, lotid: transObj.LOTID, waferid: transObj.OCR.Value);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(WaferUnloadedFailToSlotEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }
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

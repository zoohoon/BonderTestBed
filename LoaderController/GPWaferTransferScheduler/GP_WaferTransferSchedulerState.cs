using LoaderController.GPController;
using LoaderControllerBase;
using LoaderParameters;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Command.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoaderController.GPWaferTransferScheduler
{
    public abstract class GP_WaferTransferSchedulerStateBase
    {
        public GP_WaferTransferScheduler Module { get; set; }

        public GP_WaferTransferSchedulerStateBase(GP_WaferTransferScheduler module)
        {
            try
            {
                Module = module;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected GP_LoaderController LoaderController => Module.LoaderController;

        protected LoaderMap StateMap => LoaderController.ReqMap;

        public void StateTransition(GP_WaferTransferSchedulerStateBase stateObj)
        {
            try
            {
                Module.StateObj = stateObj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool UpdateState(out bool isNeedWafer, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            bool exchange = false;
            isNeedWafer = false;
            cstHashCodeOfRequestLot = "";
            bool isReservation = false;
            bool paOccupied = false;
            List<TransferObject> toToRemove;

            try
            {
                bool isWatingPolishWafer = false;

                var allocateWafersInCarrier = StateMap.GetTransferObjectAll().Where(
                    item =>
                    item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                    item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                    (item.WaferType.Value == EnumWaferType.STANDARD|| item.WaferType.Value == EnumWaferType.TCW) &&
                    item.ReservationState == ReservationStateEnum.NONE &&
                    item.WaferState == EnumWaferState.UNPROCESSED &&                    
                    item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                    (item.LOTID != null ? item.LOTID.Equals(Module.LotOPModule().LotInfo.LotName.Value) : false) &&
                    item.CST_HashCode.Equals(Module.LotOPModule().LotInfo.CSTHashCode) &&
                    item.UsingStageList.Contains(Module.LoaderController.ChuckID.Index)
                    ).ToList();              
                if (allocateWafersInCarrier.Count == 0)// 이당시에 chuck 위에 웨이퍼가 있을 수 있음.
                {
                    if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST & Module.LotOPModule().LotInfo.NeedLotDeallocated == false
                        & Module.GetParam_Wafer().GetWaferType() == EnumWaferType.STANDARD)
                    {
                        Module.LotOPModule().LotInfo.NeedLotDeallocated = true;
                        LoggerManager.Debug("NeedLotDeallocated is on");
                    }
                }

                var loadWafer = FindLoadWafer(out isNeedWafer, out isWatingPolishWafer, out isReservation, out paOccupied, out toToRemove);

                var ChuckModule = StateMap.ChuckModules.Where(item => item.ID == LoaderController.ChuckID).FirstOrDefault();
                var currentMap = StateMap;

                Module.LotOPModule().IsLastWafer = false;

                // TODO : V19
                if (ChuckModule.WaferStatus == EnumSubsStatus.EXIST && Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                {
                    if (loadWafer == null && (!isNeedWafer) && (!isReservation) || canloadwafer == false) // canloadwafer == false 일 때 LAST_WAFER로 만든 이유는 해당 랏드에서 더이상 웨이퍼를 받으면 안되기 때문.
                    {
                        if (isWatingPolishWafer == false)
                        {
                            Module.LotOPModule().IsLastWafer = true;
                            StateTransition(new LAST_WAFER_ON_CHUCK_ONLY(Module));
                            //  LoggerManager.Debug("WaferTransferScheduler State=LAST_WAFER_ON_CHUCK_ONLY, LoadWafer:NotExist, UnloadWafer:Exist");
                        }
                    }
                    else if (loadWafer == null && isReservation)
                    {
                        StateTransition(new LAST_WAFER_ON_CHUCK_ONLY(Module));
                    }
                    else
                    {
                        if (loadWafer != null)
                        {
                            exchange = true;
                        }
                        StateTransition(new WAFER_ON_CHUCK(Module));
                        //   LoggerManager.Debug("WaferTransferScheduler State=WAFER_ON_CHUCK, LoadWafer:Exist, UnloadWafer:Exist, Exchange Operate");
                    }
                }
                else if (ChuckModule.WaferStatus == EnumSubsStatus.EXIST && Module.GetParam_Wafer().GetStatus() != EnumSubsStatus.EXIST)
                {
                    if (ChuckModule.Substrate != null)
                    {
                        LoggerManager.Debug($"WaferTransferScheduler INVALED Error LoaderMap ChuckModule: Exist[{ChuckModule.Substrate.OriginHolder.ToString()}], WaferObject: {Module.GetParam_Wafer().GetStatus()}");
                    }
                    else
                    {
                        LoggerManager.Debug($"WaferTransferScheduler INVALED Error LoaderMap ChuckModule: Exist, WaferObject: {Module.GetParam_Wafer().GetStatus()}");
                    }
                    StateTransition(new ERROR(Module));
                }
                else if (ChuckModule.WaferStatus == EnumSubsStatus.NOT_EXIST && Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"WaferTransferScheduler INVALED Error LoaderMap ChuckModule: NOT Exist, WaferObject: EXIST");
                    StateTransition(new ERROR(Module));
                }
                else if (ChuckModule.WaferStatus == EnumSubsStatus.UNDEFINED || ChuckModule.WaferStatus == EnumSubsStatus.UNKNOWN || Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.UNDEFINED || Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.UNKNOWN)
                {
                    LoggerManager.Debug($"WaferTransferScheduler INVALED Error LoaderMap ChuckModule:{ChuckModule.WaferStatus}, WaferObject: {Module.GetParam_Wafer().GetStatus()}.");
                    StateTransition(new ERROR(Module));
                }
                else
                {
                    if (loadWafer == null)
                    {
                        if (isWatingPolishWafer != true)
                        {
                            StateTransition(new CASSETTE_DONE(Module));
                        }
                        //  LoggerManager.Debug("WaferTransferScheduler State=CASSETTE_DONE, LoadWafer:NotExist, UnloadWafer:NotExist");
                    }
                    else
                    {
                        StateTransition(new NO_WAFER_ON_CHUCK(Module));
                        cstHashCodeOfRequestLot = loadWafer.CST_HashCode;
                        // LoggerManager.Debug("WaferTransferScheduler State=NO_WAFER_ON_CHUCK, LoadWafer:Exist, UnloadWafer:NotExist");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return exchange;
        }

        public abstract WaferTransferSchedulerStateEnum State { get; }

        public abstract WaferTransferScheduleResult Execute();

        protected bool CanReqToScanCassette()
        {
            return true;
        }

        protected bool CanReqToNextWafer()
        {
            bool CanReqToNext = false;
            try
            {
                var chuckWaferStatus = Module.StageSupervisor().WaferObject.GetStatus();
                var chuckWaferState = Module.StageSupervisor().WaferObject.GetState();

                if (chuckWaferStatus == EnumSubsStatus.NOT_EXIST || chuckWaferStatus == EnumSubsStatus.UNDEFINED)
                {
                    CanReqToNext = true;
                }
                else if (chuckWaferStatus == EnumSubsStatus.EXIST)
                {
                    if (Module.LotOPModule().ModuleStopFlag == false)
                    {
                        CanReqToNext =
                        chuckWaferState == EnumWaferState.PROCESSED ||
                        chuckWaferState == EnumWaferState.SKIPPED ||
                        chuckWaferState == EnumWaferState.READY ||
                        chuckWaferState == EnumWaferState.SOAKINGSUSPEND ||
                        chuckWaferState == EnumWaferState.SOAKINGDONE ||
                        Module.LotOPModule().ErrorEndState != ErrorEndStateEnum.NONE;
                    }
                    else
                    {
                        CanReqToNext = false;
                    }

                    //Wafer 취소시 조건 추가
                }
                else
                {
                    CanReqToNext = false;
                }
                // LoggerManager.Debug($"WaferTransferScheduler:CanReqToNextWafer() ChuckWaferStatus: {chuckWaferStatus.ToString()}, ChuckWaferState: {chuckWaferState}, CanReqToNext: {CanReqToNext}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return CanReqToNext;
        }

        protected TransferObject FindUnloadWafer()
        {
            TransferObject retVal = null;

            try
            {
                retVal = StateMap.GetTransferObjectAll().Where(
                    item =>
                    item.CurrHolder == LoaderController.ChuckID
                    ).FirstOrDefault();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        protected TransferObject FindLoadWafer(out bool isNeedWafer, out bool isWaitingPolishWafer, out bool isReservation,
                                               out bool isPAOccupied, out List<TransferObject> toremove)
        {
            Func<ObservableCollection<string>, string> LoadablePolishWaferInSelectedList = (ObservableCollection<string> selectedPolishWaferNameList) =>
            {
                string loadablePolishWafer = string.Empty;
                foreach (var pw in selectedPolishWaferNameList)
                {
                    var loadablePolishWaferObj = StateMap.GetTransferObjectAll().FirstOrDefault(
                    item => item.WaferType.Value == EnumWaferType.POLISH &&
                            item.PolishWaferInfo != null &&
                            item.PolishWaferInfo.DefineName.Value == pw &&
                            (item.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY || item.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY) &&
                            (item.CurrHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY || item.CurrHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                         );

                    if (loadablePolishWaferObj != null)
                    {
                        loadablePolishWafer = pw;
                        break;
                    }
                }

                return loadablePolishWafer;
            };

            TransferObject loadWafer = null;

            isNeedWafer = false;
            isWaitingPolishWafer = false;
            isReservation = false;
            isPAOccupied = false;
            toremove = new List<TransferObject>();
            try
            {
                if (Module.LotOPModule().ErrorEndState != ErrorEndStateEnum.NONE)
                {
                    loadWafer = null;
                    return loadWafer;
                }



                string RequestedWaferName = string.Empty;
                bool IsPWRequested = false;

                // PolishWaferModule이 Running State가 되어야 하는 경우
                // Polish Wafer를 올려야 되는 상황.
                var paModules = StateMap.PreAlignModules.ToList().Where(p => p.Enable == true);
                var wafersOnPA = StateMap.GetTransferObjectAll().Where(item =>
                        item.CurrHolder.ModuleType == ModuleTypeEnum.PA &&
                        item.ReservationState == ReservationStateEnum.NONE).ToList();

                //test code//
                //if (wafersOnPA.Count >= paModules.Count())
                //{
                //    isPAOccupied = true;
                //    toremove.Add(wafersOnPA.FirstOrDefault());
                //}

                bool isNeedPolishWaferForStatusSoaking = Module.SoakingModule().GetPolishWaferForStatusSoaking();
                if (isNeedPolishWaferForStatusSoaking ||
                    (Module.GetParam_Wafer().GetState() == EnumWaferState.SOAKINGSUSPEND) ||
                    Module.PolishWaferModule().IsExecuteOfScheduler() == true ||
                    Module.PolishWaferModule().ModuleState.State == ModuleStateEnum.RUNNING ||
                    Module.PolishWaferModule().ModuleState.State == ModuleStateEnum.SUSPENDED ||
                    Module.PolishWaferModule().ModuleState.State == ModuleStateEnum.ERROR)
                {
                    //LoggerManager.Debug($"FindLoadWafer(): PolishWaferModule().IsExecuteOfScheduler(): {this.Module.PolishWaferModule().IsExecuteOfScheduler()}");
                    //LoggerManager.Debug($"FindLoadWafer(): PolishWaferModuleState: {this.Module.PolishWaferModule().ModuleState.State}");
                    RequestedWaferName = string.Empty;

                    IsPWRequested = this.Module.PolishWaferModule().IsRequested(ref RequestedWaferName);

                    // [BEGIN] Polish Wafer load for Status Soaking
                    if (!IsPWRequested)
                    {
                        if (isNeedPolishWaferForStatusSoaking ||
                            (Module.GetParam_Wafer().GetState() == EnumWaferState.SOAKINGSUSPEND))
                        {
                            var selectedPolishWaferNameList = Module.SoakingModule().GetPolishWaferNameListForSoaking();
                            RequestedWaferName = LoadablePolishWaferInSelectedList(selectedPolishWaferNameList);

                            IsPWRequested = !string.IsNullOrEmpty(RequestedWaferName);
                        }
                    }
                    // [END] 

                    if (IsPWRequested && !string.IsNullOrEmpty(RequestedWaferName))
                    {
                        var loadablePolishWafers = StateMap.GetTransferObjectAll().Where(
                        item => item.WaferType.Value == EnumWaferType.POLISH &&
                         (item.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY ||
                             item.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY ||
                             item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT) &&
                             item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                             item.ReservationState == ReservationStateEnum.NONE &&
                             item.PolishWaferInfo != null &&
                             item.PolishWaferInfo.DefineName.Value == RequestedWaferName
                        ).ToList();
                        

                        loadWafer = loadablePolishWafers.Where(w => w.CurrHolder.ModuleType == ModuleTypeEnum.BUFFER || w.CurrHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY || w.CurrHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY).OrderBy(item => item.PolishWaferInfo.Priorty.Value).FirstOrDefault();
                        if (loadWafer == null)
                        {
                            loadWafer = loadablePolishWafers.OrderBy(item => item.PolishWaferInfo.Priorty.Value).FirstOrDefault();
                            //loadWafer = loadablePolishWafers.OrderBy(item => item.OriginHolder.Index).FirstOrDefault();
                        }

                        if (loadWafer == null)
                        {
                            loadablePolishWafers = StateMap.GetTransferObjectAll().Where(
                                item => item.WaferType.Value == EnumWaferType.POLISH &&
                                item.PolishWaferInfo.DefineName.Value == RequestedWaferName &&
                                item.CurrHolder != this.LoaderController.ChuckID
                                ).ToList();

                            // 현재 로더에 폴리쉬 웨이퍼는 없지만, 다른 스테이지가 갖고 있다. 따라서, 대기
                            if (loadablePolishWafers.Count > 0)
                            {
                                isWaitingPolishWafer = true;
                            }
                            else
                            {
                                this.Module.PolishWaferModule().PolishWaferValidate(false);
                                isWaitingPolishWafer = true;
                            }
                        }
                        else
                        {
                            // soaking을 위한 pw가 필요하고 Lot idle일때(polish wafer 전달을 위한 map이 있다면 PW를 받고 나서 Lot Start가 되도록 한다.)
                            if (isNeedPolishWaferForStatusSoaking && Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                            {
                                Module.LotOPModule().TransferReservationAboutPolishWafer = true; //해당 flag가 true라면 Lot start가 되면 안된다.
                                LoggerManager.SoakingLog("Set 'TransferReservationAboutPolishWafer(true)' flag.(Lot can not start until the flag is off.");
                            }
                        }
                    }
                    else
                    {
                        if (IsPWRequested == false)
                        {
                            isWaitingPolishWafer = true;
                        }
                        else
                        {
                            LoggerManager.Debug($"[WaferTransferScheduler] PolishWafer NotMatched. IsPWRequested:{IsPWRequested} , RequestedWaferName :{ RequestedWaferName }");
                        }
                    }
                }
                else
                {
                    var loadableWafers = StateMap.GetTransferObjectAll().Where(
                            item =>
                            item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                            (item.WaferType.Value == EnumWaferType.STANDARD|| item.WaferType.Value == EnumWaferType.TCW) &&
                            item.ReservationState == ReservationStateEnum.NONE &&
                            item.WaferState == EnumWaferState.UNPROCESSED &&
                            item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                            item.OCRReadState == ProberInterfaces.Enum.OCRReadStateEnum.DONE &&
                            item.UsingStageList.Contains(Module.LoaderController.ChuckID.Index)
                            ).ToList();

                    //LoggerManager.Debug($"FindLoadWafer(): No PolishWafer");
                    //LoggerManager.Debug($"FindLoadWafer(): loadableWafers count:{loadableWafers.Count}");
                    loadWafer = loadableWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();

                    TransferObject realLoadWafer = null;

                    if (loadWafer != null)
                    {
                        var loadableWafers_exclude_ocr_done = StateMap.GetTransferObjectAll().Where(
                            item =>
                            item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                            (item.WaferType.Value == EnumWaferType.STANDARD || item.WaferType.Value == EnumWaferType.TCW) &&
                            item.ReservationState == ReservationStateEnum.NONE &&
                            item.WaferState == EnumWaferState.UNPROCESSED &&
                            item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                            item.OCRReadState != ProberInterfaces.Enum.OCRReadStateEnum.ABORT &&
                            item.UsingStageList.Contains(Module.LoaderController.ChuckID.Index)
                            ).ToList();

                        if (loadableWafers_exclude_ocr_done != null)
                        {
                            var loadWafer_exclude_ocr_done = loadableWafers_exclude_ocr_done.OrderBy(item => item.LotPriority).FirstOrDefault();

                            // LotPriority의 값이 동일한 것을 확인함으로써 우선순위가 낮은 웨이퍼의 요청을 막을 수 있다.
                            // 우선순위가 낮은 (다른 Foup) 웨이퍼(OCR DONE)가 요청될 수 없게 함.
                            if (loadWafer.LotPriority == loadWafer_exclude_ocr_done.LotPriority)
                            {
                                realLoadWafer = FindSuitableWafer(loadWafer);
                            }
                            else
                            {
                                isNeedWafer = true;
                            }

                        }
                    }

                    if (isNeedWafer == false)
                    {
                        if (realLoadWafer == null)
                        {
                            loadableWafers = StateMap.GetTransferObjectAll().Where(
                                item =>
                                item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                                (item.WaferType.Value == EnumWaferType.STANDARD || item.WaferType.Value == EnumWaferType.TCW) &&
                                item.ReservationState == ReservationStateEnum.NONE &&
                                item.WaferState == EnumWaferState.UNPROCESSED &&
                                item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                                  item.OCRReadState != ProberInterfaces.Enum.OCRReadStateEnum.ABORT &&
                                item.UsingStageList.Contains(Module.LoaderController.ChuckID.Index)
                                ).ToList();

                            if (loadWafer != null)
                            {
                                isReservation = true;
                                isNeedWafer = false;
                            }
                            else if (loadableWafers != null && loadableWafers.Count > 0)
                            {
                                isNeedWafer = true; //true
                            }
                            else
                            {
                                isNeedWafer = false;
                            }
                        }
                        else
                        {
                            realLoadWafer.NotchAngle.Value = this.Module.GetParam_Wafer().GetPhysInfo().NotchAngle.Value;
                            //this.Module.LotOPModule().LotInfo.LotName.Value = realLoadWafer.LOTID;
                        }
                    }

                    loadWafer = realLoadWafer;

                    if ((Module.LotOPModule().CommandRecvSlot.IsRequested<ILotOpStart>()
                         || Module.LotOPModule().LotStateEnum == LotOPStateEnum.READYTORUNNING))
                    {
                        if (Module.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                        {
                            loadWafer = null;
                        }
                    }

                    if (Module.StageSupervisor().StageMode != GPCellModeEnum.ONLINE)
                    {
                        loadWafer = null;
                        LoggerManager.Debug($"GP_WaferTansferSchedulerState - FindLoadWafer(). StageMode is :{Module.StageSupervisor().StageMode}");
                    }
                }

                //pin align module 또는 Soaking module 이 동작중이거나 사용불가한 상태에서는 Wafer transfer를 하지 않도록 한다.

                if (Module.PinAligner().ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                    Module.PinAligner().ModuleState.GetState() == ModuleStateEnum.PENDING ||
                    Module.PinAligner().ModuleState.GetState() == ModuleStateEnum.ERROR ||
                    Module.SoakingModule().ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                    Module.SoakingModule().ModuleState.GetState() == ModuleStateEnum.PENDING ||
                    Module.SoakingModule().ModuleState.GetState() == ModuleStateEnum.ERROR
                    )
                {
                    loadWafer = null;
                    return loadWafer;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }



            return loadWafer;
        }

        private TransferObject FindSuitableWafer(TransferObject loadableWafer)
        {
            try
            {
                if (loadableWafer == null)
                {
                    return loadableWafer;
                }
                
                int slotNum = loadableWafer.OriginHolder.Index % 25;
                int offset = 0;

                if (slotNum == 0)
                {
                    slotNum = 25;
                    offset = -1;
                }

                int foupNum = ((loadableWafer.OriginHolder.Index + offset) / 25) + 1;

                if (this.Module.DeviceModule().IsHaveReservationRecipe(foupNum, loadableWafer.LOTID, loadableWafer.CST_HashCode)) //레시피 예약 개념.
                {
                    this.Module.DeviceModule().SetLoadReserveDevice(foupNum, loadableWafer.LOTID, loadableWafer.CST_HashCode);

                    var cassette = StateMap.CassetteModules.FirstOrDefault(item => item.ID.Index == foupNum);

                    if(cassette != null)
                    {
                        if(cassette.LotMode != LotModeEnum.UNDEFINED)
                        {
                            Module.StageSupervisor().ChangeLotMode(cassette.LotMode);
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[{this.GetType().Name}], FindSuitableWafer() : cassette is null. foupNum = {foupNum}");
                    }

                    loadableWafer = null;
                }
            }
            catch (Exception err)
            {
                //targetWafers = null;
                LoggerManager.Debug($"FindSuitableWafer(): Error occurred. Err = {err.Message}");
            }

            return loadableWafer;
        }
    }

    public class NO_CASSETTE : GP_WaferTransferSchedulerStateBase
    {
        public NO_CASSETTE(GP_WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.NO_CASSETTE;

        public override WaferTransferScheduleResult Execute()
        {
            return WaferTransferScheduleResult.NOT_NEED;
        }
    }

    public class NO_READ : GP_WaferTransferSchedulerStateBase
    {
        public NO_READ(GP_WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.NO_READ;

        public override WaferTransferScheduleResult Execute()
        {
            WaferTransferScheduleResult rel = null;

            try
            {
                if (CanReqToScanCassette())
                {
                    var cassette = StateMap.CassetteModules.FirstOrDefault();

                    LoaderMapEditor editor = new LoaderMapEditor(StateMap);
                    editor.EditorState.SetScanCassette(cassette.ID);

                    rel = WaferTransferScheduleResult.CreateNeed(editor);
                }
                else
                {
                    rel = WaferTransferScheduleResult.NOT_NEED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
    }

    public class NO_WAFER_ON_CHUCK : GP_WaferTransferSchedulerStateBase
    {
        public NO_WAFER_ON_CHUCK(GP_WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.NO_WAFER_ON_CHUCK;

        public override WaferTransferScheduleResult Execute()
        {
            WaferTransferScheduleResult rel = null;

            try
            {

                if (CanReqToNextWafer())
                {
                    bool isNeedWafer;
                    bool isWatingPolishWafer = false;
                    bool isReservation = false;
                    bool paOccupied = false;
                    List<TransferObject> toToRemove;
                    TransferObject loadSub = FindLoadWafer(out isNeedWafer, out isWatingPolishWafer, out isReservation,
                            out paOccupied, out toToRemove);
                    //TransferObject loadSub = FindLoadWafer(out isNeedWafer);
                    TransferObject removeTarget = null;

                    if (loadSub != null)
                    {
                        if (loadSub.WaferType.Value == EnumWaferType.STANDARD && (Module.LoaderController.IsAbort))// || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE))
                        {
                            rel = WaferTransferScheduleResult.NOT_NEED;
                            return rel;
                        }
                        LoaderMapEditor editor = new LoaderMapEditor(StateMap);

                        loadSub.SetReservationState(ReservationStateEnum.RESERVATION);
                        var LoadNotchAngleOptionVar = new LoadNotchAngleOption();
                        LoadNotchAngleOptionVar.IsEnable.Value = false;
                        if (paOccupied & loadSub.WaferType.Value != EnumWaferType.STANDARD)
                        {
                            if (toToRemove.Count > 0)
                            {
                                var notchAngleOptionVar = new LoadNotchAngleOption();
                                notchAngleOptionVar.IsEnable.Value = false;
                                removeTarget = toToRemove.FirstOrDefault();
                                removeTarget.SetReservationState(ReservationStateEnum.RESERVATION);
                                editor.EditorState.SetTransfer(removeTarget.ID.Value, removeTarget.OriginHolder, notchAngleOptionVar);
                            }
                            else
                            {
                                LoggerManager.Debug($"GPWaferTransferScheduler.NO_WAFER_ON_CHUCK.Excute(): Exception. No target object to remove.");
                            }
                        }


                        editor.EditorState.SetTransfer(loadSub.ID.Value, LoaderController.ChuckID, LoadNotchAngleOptionVar);
                        //editor.EditorState.SetTransfer(loadSub.ID, LoaderController.ChuckID, new LoadNotchAngleOption() { IsEnable = false });
                        //LoggerManager.Debug($"[WaferTransferScheduler] NO_WAFER_ON_CHUCK .  LoadWafer: {loadSub.CurrHolder.Label}{loadSub.CurrHolder.Index}");
                        rel = WaferTransferScheduleResult.CreateNeed(editor);
                        Module.PinAligner().WaferTransferRunning = true;
                    }
                    else
                    {
                        if (isWatingPolishWafer == true)
                        {
                            LoggerManager.Debug($"[NO_WAFER_ON_CHUCK] isWatingPolishWafer = true. abnormal status.");
                        }

                        rel = WaferTransferScheduleResult.NOT_NEED;
                    }
                }
                else
                {
                    rel = WaferTransferScheduleResult.NOT_NEED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }

    public class WAFER_ON_CHUCK : GP_WaferTransferSchedulerStateBase
    {
        public WAFER_ON_CHUCK(GP_WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.WAFER_ON_CHUCK;

        public override WaferTransferScheduleResult Execute()
        {
            WaferTransferScheduleResult rel = null;

            try
            {
                if (CanReqToNextWafer())
                {
                    bool isNeedWafer;
                    bool isWatingPolishWafer;
                    bool isReservation = false;
                    bool paOccupied = false;
                    List<TransferObject> toToRemove;
                    TransferObject unloadSub = FindUnloadWafer();
                    if (unloadSub == null || unloadSub.ID == null)
                    {
                        rel = WaferTransferScheduleResult.NOT_NEED;
                        LoggerManager.Debug($"[WAFER_ON_CHUCK], Execute() : WaferTransferScheduleResult.NOT_NEED");
                        return rel;
                    }
                    //TransferObject loadSub = FindLoadWafer(out isNeedWafer);
                    TransferObject loadSub = FindLoadWafer(out isNeedWafer, out isWatingPolishWafer, out isReservation,
                                            out paOccupied, out toToRemove);
                    TransferObject removeTarget = null;

                    if (loadSub != null)
                    {
                        unloadSub.WaferState = Module.StageSupervisor().WaferObject.GetState();
                        unloadSub.SetReservationState(ReservationStateEnum.RESERVATION);
                        loadSub.SetReservationState(ReservationStateEnum.RESERVATION);
                        LoaderMapEditor editor = new LoaderMapEditor(StateMap);

                        this.Module.GEMModule().GetPIVContainer().StageNumber.Value = this.Module.LoaderController().GetChuckIndex();
                        //this.Module.GEMModule().GetPIVContainer().StageState.Value = 0;
                        //this.Module.StageSupervisor().GetStagePIV().LotID.Value = this.Module.LotOPModule().LotInfo.LotName.Value;

                        //this.Module.StageSupervisor().GetStagePIV().WaferID.Value = Module.StageSupervisor().WaferObject.GetPhysInfo().WaferID.Value;

                        this.Module.GEMModule().GetPIVContainer().WaferID.Value = Module.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value;

                        //int slotNum = Module.StageSupervisor().WaferObject.GetPhysInfo().SlotIndex.Value;
                        //this.Module.StageSupervisor().GetStagePIV().SlotNumber.Value = slotNum;
                        this.Module.GEMModule().GetPIVContainer().CurTemperature.Value = this.Module.TempController().TempInfo.CurTemp.Value;
                        //this.Module.StageSupervisor().GetStagePIV().ProbeCardID.Value = this.Module.PinAligner().GetParam_ProbeCard().GetProbeCardID();

                        //this.Module.GEMModule().SetEvent(105);
                        //this.Module.GEMModule().SetEvent(116);
                        //var lockobj = Module.StageSupervisor().GetStagePIV().GetPIVDataLockObject();
                        //lock (lockobj)
                        //{
                        // Module.StageSupervisor().GetStagePIV().SetStageState(GEMStageStateEnum.NEXT_WAFER_PREPRCOESSING);
                        // Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(NextWaferPreprocessingEvent).FullName));
                        //}
                        if (paOccupied)
                        {
                            if (toToRemove.Count > 0)
                            {
                                var notchAngleOptionVar = new LoadNotchAngleOption();
                                notchAngleOptionVar.IsEnable.Value = false;
                                removeTarget = toToRemove.FirstOrDefault();
                                removeTarget.SetReservationState(ReservationStateEnum.RESERVATION);
                                editor.EditorState.SetTransfer(removeTarget.ID.Value, removeTarget.OriginHolder, notchAngleOptionVar);
                            }
                            else
                            {
                                LoggerManager.Debug($"GPWaferTransferScheduler.NO_WAFER_ON_CHUCK.Excute(): Exception. No target object to remove.");
                            }
                        }

                        editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.UnloadHolder);
                        editor.EditorState.SetTransfer(loadSub.ID.Value, LoaderController.ChuckID);
                        Module.PinAligner().WaferTransferRunning = true;

                        rel = WaferTransferScheduleResult.CreateNeed(editor);
                    }
                    else
                    {
                        LoaderMapEditor editor = new LoaderMapEditor(StateMap);

                        //#Hynix_Merge: 아래 코드 Hynix 코드 Normal Lot 일때 UnloadHolder 언제 Set해주는지 확인 할것.
                        //if (unloadSub.UnloadHolder.ModuleType != ModuleTypeEnum.UNDEFINED)// DynamicFoup사용할 때...
                        //{
                        //    editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.UnloadHolder);
                        //}
                        //else
                        //{
                        //    editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.OriginHolder);
                        //}
                        editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.UnloadHolder);
                        rel = WaferTransferScheduleResult.CreateNeed(editor);
                    }
                }
                else
                {
                    rel = WaferTransferScheduleResult.NOT_NEED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
    }

    public class LAST_WAFER_ON_CHUCK_ONLY : GP_WaferTransferSchedulerStateBase
    {
        public LAST_WAFER_ON_CHUCK_ONLY(GP_WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.LAST_WAFER_ON_CHUCK_ONLY;

        public override WaferTransferScheduleResult Execute()
        {
            WaferTransferScheduleResult rel = null;

            try
            {
                if (CanReqToNextWafer())
                {
                    TransferObject unloadSub = FindUnloadWafer();
                    unloadSub.WaferState = Module.StageSupervisor().WaferObject.GetState();
                    unloadSub.SetReservationState(ReservationStateEnum.RESERVATION);
                    LoaderMapEditor editor = new LoaderMapEditor(StateMap);


                    this.Module.GEMModule().GetPIVContainer().StageNumber.Value = this.Module.LoaderController().GetChuckIndex();
                    //this.Module.GEMModule().GetPIVContainer().StageState.Value = 0;
                    //this.Module.StageSupervisor().GetStagePIV().LotID.Value = this.Module.LotOPModule().LotInfo.LotName.Value;
                    //this.Module.StageSupervisor().GetStagePIV().WaferID.Value = Module.StageSupervisor().WaferObject.GetPhysInfo().WaferID.Value;
                    //int slotNum = Module.StageSupervisor().WaferObject.GetPhysInfo().SlotIndex.Value;
                    //this.Module.StageSupervisor().GetStagePIV().SlotNumber.Value = slotNum;
                    this.Module.GEMModule().GetPIVContainer().CurTemperature.Value = this.Module.TempController().TempInfo.CurTemp.Value;
                    //this.Module.StageSupervisor().GetStagePIV().ProbeCardID.Value = this.Module.PinAligner().GetParam_ProbeCard().GetProbeCardID();
                    if (unloadSub.UnloadHolder.ModuleType != ModuleTypeEnum.UNDEFINED)
                    {
                        editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.UnloadHolder);
                        // LoggerManager.Debug($"[WaferTransferScheduler] LAST_WAFER_ON_CHUCK_ONLY . UnLoadWafer:{unloadSub.CurrHolder.Label}{unloadSub.CurrHolder.Index}");
                        rel = WaferTransferScheduleResult.CreateNeed(editor);
                    }
                    else
                    {
                        rel = WaferTransferScheduleResult.NOT_NEED;
                    }
                }
                else
                {
                    rel = WaferTransferScheduleResult.NOT_NEED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
    }

    public class CASSETTE_DONE : GP_WaferTransferSchedulerStateBase
    {
        public CASSETTE_DONE(GP_WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.CASSETTE_DONE;

        public override WaferTransferScheduleResult Execute()
        {
            return WaferTransferScheduleResult.NOT_NEED;
        }
    }

    public class ERROR : GP_WaferTransferSchedulerStateBase
    {
        public ERROR(GP_WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.UNDEFINED;

        public override WaferTransferScheduleResult Execute()
        {
            return WaferTransferScheduleResult.NOT_NEED;
        }
    }
}

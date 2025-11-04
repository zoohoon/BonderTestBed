using Autofac;
using LoaderBase;
using LoaderParameters;
using LoaderServiceBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Foup;
using ProberInterfaces.LoaderController;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace LoaderMaster.StageEmul
{
    public class StageEmulation : ILoaderServiceCallback
    {

        public ModuleID ChuckID => ModuleID.Create(ModuleTypeEnum.CHUCK, ChuckIndex, "");
        int ChuckIndex;
        public bool JobDone = false;
        ILoaderModule Loader;
        public bool IsCancel { get; set; }
        public StageEmulation(int chuckIdx, ILoaderModule loader)
        {
            ChuckIndex = chuckIdx;
            Loader = loader;
            int time = chuckIdx * 1000;
            Loader.LoaderMaster.SetStageState(ChuckIndex, ModuleStateEnum.IDLE, false);
            ModuleState = ModuleStateEnum.IDLE;
            timer = new System.Timers.Timer(6000);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            isWaferTransfer = true;
        }
        private int waferInterValCnt = 0;
        private int waferInterVal = 9999;
        private LoaderMap StateMap;
        bool isWaferTransfer = true;
        public LoaderMap RequestJob(LoaderInfo loaderInfo, out bool isExchange, out bool isNeedWafer, out bool isTempReady, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            StateMap = null;
            isExchange = false;
            isNeedWafer = false;
            isTempReady = true;
            cstHashCodeOfRequestLot = "";
            if (isWaferTransfer)
            {
                if (JobDone || ModuleState == ModuleStateEnum.PAUSED)
                {
                    return null;
                }
                StateMap = (LoaderMap)loaderInfo.StateMap.Clone();
                var loadWafer = FindLoadWafer(out isNeedWafer);
                var ChuckModule = StateMap.ChuckModules.Where(item => item.ID == ChuckID).FirstOrDefault();

                if (ChuckModule.WaferStatus == EnumSubsStatus.EXIST)
                {

                    if (isNeedWafer)
                    {
                    }
                    else if(loadWafer == null)
                    {
                        TransferObject unloadSub = FindUnloadWafer();
                        //  SetTransfer(unloadSub.ID.Value, unloadSub.OriginHolder);
                        unloadSub.WaferState = EnumWaferState.PROCESSED;
                        LoaderMapEditor editor = new LoaderMapEditor(StateMap);
                        editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.OriginHolder);
                        unloadSub.ReservationTime = DateTime.Now;
                        unloadSub.WaferState = EnumWaferState.PROCESSED;
                        StateMap = editor.EditMap;
                        isWaferTransfer = false;
                        //loaderInfo.StateMap = StateMap;
                        // StateTransition(new LAST_WAFER_ON_CHUCK_ONLY(Module));

                    }
                    else
                    {
                        //if (loaderInfo.StateMap.ChuckModules[11].WaferStatus == EnumSubsStatus.EXIST)
                        //// if(number==5)
                        //{
                        //return null;
                        TransferObject unloadSub = FindUnloadWafer();
                        unloadSub.WaferState = EnumWaferState.PROCESSED;
                        LoaderMapEditor editor = new LoaderMapEditor(StateMap);
                        editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.OriginHolder);
                        editor.EditorState.SetTransfer(loadWafer.ID.Value, ChuckID);
                        //ChuckModule.WaferStatus = EnumSubsStatus.EXIST;
                        StateMap = editor.EditMap;
                        loadWafer.ReservationTime = DateTime.Now;
                        unloadSub.ReservationTime = DateTime.Now;
                        // loaderInfo.StateMap = StateMap;
                        isExchange = true;

                        isWaferTransfer = false;
                        //JobDone = true;
                        //}
                        //else
                        //{
                        //    return null;
                        //}
                    }
                }
                else
                {
                    if (loadWafer == null)
                    {
                        StateMap = null;
                        isWaferTransfer = true;
                    }
                    else
                    {
                        SetTransfer(loadWafer.ID.Value, ChuckID);
                        loadWafer.SetReservationState(ReservationStateEnum.RESERVATION);
                        isWaferTransfer = false;
                        //ChuckModule.WaferStatus = EnumSubsStatus.EXIST;
                    }
                }

            }

            return StateMap;
        }


        public void UpdateIsNeedLotEnd(LoaderInfo loaderInfo)
        {
            return;
        }

        public LoaderMap UnloadRequestJob(LoaderInfo loaderInfo)
        {
            return null;
        }


        public void SetTransfer(string id, ModuleID destinationID)
        {
            TransferObject subObj = StateMap.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();
            ModuleInfoBase dstLoc = StateMap.GetLocationModules().Where(item => item.ID == destinationID).FirstOrDefault();
            if (subObj == null)
            {
                StateMap = null;
            }

            if (dstLoc == null)
            {
                StateMap = null;
            }

            if (subObj.CurrPos == destinationID)
            {
                StateMap = null;
            }

            if (dstLoc is HolderModuleInfo)
            {
                var currHolder = StateMap.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                var dstHolder = dstLoc as HolderModuleInfo;

                subObj.PrevHolder = subObj.CurrHolder;
                subObj.PrevPos = subObj.CurrPos;

                subObj.CurrHolder = destinationID;
                subObj.CurrPos = destinationID;


                currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                currHolder.Substrate = null;

                dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                dstHolder.Substrate = subObj;
                subObj.ReservationTime = DateTime.UtcNow;
            }
            else
            {
                subObj.PrevPos = subObj.CurrPos;
                subObj.CurrPos = destinationID;
                subObj.ReservationTime = DateTime.UtcNow;
            }
        }


        protected TransferObject FindLoadWafer(out bool isNeedWafer)
        {
            TransferObject loadWafer = null;
            isNeedWafer = false;

            try
            {
                if (waferInterValCnt == waferInterVal)
                {
                    var loadablePolishWafers = StateMap.GetTransferObjectAll().Where(
                   item => item.WaferType.Value == EnumWaferType.POLISH &&
                    (item.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY ||
                    item.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY ||
                    item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT) &&
                    item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                    item.ReservationState == ReservationStateEnum.NONE
               //item.PolishWaferInfo!=null&&
               //item.PolishWaferInfo.DefineName.Value== needPolish.LoadWaferType
               ).ToList();
                    loadWafer = loadablePolishWafers.OrderBy(item => item.OriginHolder.Index).FirstOrDefault();
                    if (loadWafer == null)
                    {
                        loadablePolishWafers = StateMap.GetTransferObjectAll().Where(
                        item => item.WaferType.Value == EnumWaferType.POLISH).ToList();
                        if (loadablePolishWafers.Count > 0)
                        {
                        }
                    }
                    else
                    {

                        waferInterValCnt = 0;

                    }
                }
                else
                {
                    var allWafers = StateMap.GetTransferObjectAll();
                    var loadableWafers = StateMap.GetTransferObjectAll().Where(
                            item =>
                            item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                            item.WaferType.Value == EnumWaferType.STANDARD &&
                            item.WaferState == EnumWaferState.UNPROCESSED &&
                            item.ReservationState == ReservationStateEnum.NONE &&
                            item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                            item.OCRReadState == ProberInterfaces.Enum.OCRReadStateEnum.DONE &&
                            item.UsingStageList.Contains(ChuckID.Index)
                        ).ToList();
                    loadWafer = loadableWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();
                    if (loadWafer == null)
                    {
                        loadableWafers = StateMap.GetTransferObjectAll().Where(
                               item =>
                               item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                               item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                               item.WaferType.Value == EnumWaferType.STANDARD &&
                               item.WaferState == EnumWaferState.UNPROCESSED &&
                               item.ReservationState == ReservationStateEnum.NONE &&
                               item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                               item.UsingStageList.Contains(ChuckID.Index)
                           ).ToList();
                        if (loadableWafers.Count > 0)
                        {
                            isNeedWafer = true;
                        }
                    }
                    if (loadWafer != null)
                    {
                        waferInterValCnt++;
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return loadWafer;
        }

        protected TransferObject FindUnloadWafer()
        {
            TransferObject retVal = null;

            try
            {
                retVal = StateMap.GetTransferObjectAll().Where(
                    item =>
                    item.CurrHolder == ChuckID
                    ).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public ModuleID GetChuckID()
        {
            return ChuckID;
        }

        public ModuleID GetOriginHolder()
        {
            return new ModuleID();
        }

        public TransferObject GetDeviceInfo()
        {
            return null;
        }

        #region 

        public ILoaderControllerParam LoaderConnectParam => throw new NotImplementedException();

        public bool FoupTiltIgoreFlag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public EventCodeEnum CSTInfoChanged(LoaderInfo info)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum FOUP_FoupCoverDown(int cassetteNumber)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum FOUP_FoupCoverUp(int cassetteNumber)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum FOUP_FoupTiltDown(int cassetteNumber)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum FOUP_FoupTiltUp(int cassetteNumber)
        {
            throw new NotImplementedException();
        }

        public FoupModuleInfo FOUP_GetFoupModuleInfo(int cassetteNumber)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum FOUP_MonitorForWaferOutSensor(int cassetteNumber, bool value)
        {
            throw new NotImplementedException();
        }


        public EnumSubsStatus GetChuckWaferStatus()
        {
            throw new NotImplementedException();
        }

        public bool IsHandlerholdWafer()
        {
            throw new NotImplementedException();
        }

        public EnumWaferType GetWaferType()
        {
            return EnumWaferType.UNDEFINED;
        }

        public EnumSubsStatus UpdateCardStatus(out EnumWaferState cardState)
        {
            cardState = EnumWaferState.UNPROCESSED;
            return EnumSubsStatus.NOT_EXIST;
        }

        public bool GetMachineInitDoneState()
        {          
            return false;
        }

        public LoaderDeviceParameter GetLoaderDeviceParameter()
        {
            throw new NotImplementedException();
        }

        Timer timer = new Timer();
        bool isLoadFirst = true;
        bool isUnloadFirst = true;
        public EventCodeEnum OnLoaderInfoChanged(LoaderInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var currInfo = info;
                if (currInfo != null)
                {
                    switch (currInfo.ModuleInfo.ModuleState)
                    {

                        case ModuleStateEnum.SUSPENDED:
                            if (currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.LOAD)

                            {
                                if (currInfo.ModuleInfo.ChuckNumber == ChuckIndex)
                                {
                                    if (isLoadFirst)
                                    {
                                        isLoadFirst = false;
                                        Loader.AwakeProcessModule();
                                        TransferObject obj = null;

                                        Loader.WaferTransferRemoteService.NotifyLoadedToThreeLeg(out obj);
                                        Loader.WaferTransferRemoteService.NotifyWaferTransferResult(true);
                                        isLoadFirst = true;
                                        isUnloadFirst = true;
                                    }
                                }
                                else
                                {
                                    isLoadFirst = true;
                                }
                            }
                            else if (currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.UNLOAD)
                            {
                                if (currInfo.ModuleInfo.ChuckNumber == ChuckIndex)
                                {
                                    if (isUnloadFirst)
                                    {
                                        isUnloadFirst = false;
                                        Loader.AwakeProcessModule();

                                        Loader.WaferTransferRemoteService.NotifyUnloadedFromThreeLeg(EnumWaferState.PROCESSED, ChuckIndex, true);
                                        Loader.WaferTransferRemoteService.NotifyWaferTransferResult(true);
                                        isLoadFirst = true;
                                        isUnloadFirst = true;
                                    }
                                }
                                else
                                {
                                    isUnloadFirst = true;
                                }
                            }
                            else if (currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.CARD_LOAD || currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.CARD_UNLOAD)
                            {
                                if (currInfo.ModuleInfo.ChuckNumber == ChuckIndex)
                                {
                                    Loader.AwakeProcessModule();

                                    Loader.CardTransferRemoteService.NotifyCardTransferResult(true);
                                }
                            }


                            break;

                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"OnLoaderEventRaised() : " + ex.Message);
                LoggerManager.Exception(err);

                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        public EventCodeEnum OnLoaderParameterChanged(LoaderSystemParameter systemParam, LoaderDeviceParameter deviceParam)
        {
            throw new NotImplementedException();
        }

        public void UI_HideLoaderCam()
        {
            throw new NotImplementedException();
        }

        public void UI_ShowLoaderCam()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WaferHolderChanged(int slotNum, string holder)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WaferIDChanged(int slotNum, string ID)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WaferStateChanged(int slotNum, EnumSubsStatus waferStatus, EnumWaferState waferState)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WaferSwapChanged(int originSlotNum, int changeSlotNum, bool isInit)
        {
            throw new NotImplementedException();
        }

        public bool LotOPStart(int foupnumber, bool iscellstart = false, string lotID = "", string cstHashCode = "")
        {
            Loader.LoaderMaster.SetStageState(ChuckIndex, ModuleStateEnum.RUNNING, false);
            ModuleState = ModuleStateEnum.RUNNING;
            return true;
        }
        public ModuleStateEnum GetLotState()
        {
            return ModuleState;
        }
        public bool IsLotEndReady()
        {
            return true;
        }

        public bool LotOPEnd(int foupnumber = -1)
        {
            Loader.LoaderMaster.SetStageState(ChuckIndex, ModuleStateEnum.IDLE, false);
            ModuleState = ModuleStateEnum.IDLE;
            return false;
        }
        ModuleStateEnum ModuleState;

        public bool LotOPPause(bool isabort = false)
        {
            Loader.LoaderMaster.SetStageState(ChuckIndex, ModuleStateEnum.PAUSED, false);
            ModuleState = ModuleStateEnum.PAUSED;
            //JobDone = true;
            return true;
        }
        public bool LotOPResume()
        {
            Loader.LoaderMaster.SetStageState(ChuckIndex, ModuleStateEnum.RUNNING, false);
            ModuleState = ModuleStateEnum.RUNNING;
            return true;
        }
        public bool CardAbort()
        {           
            return true;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public void DisConnect()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetActiveLotInfo(int foupnumber, string lotid, string cstHashCode, string carrierid)
        {
            return;
        }
        public void SetCassetteHashCode(int foupNumber, string lotId, string cstHashCode)
        {
            return;
        }
        public void SetLotStarted(int foupNumber, string lotId, string cstHashCode)
        {
            return;
        }


        public string GetProbeCardID()
        {
            string retVal = "";
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public string GetWaferID()
        {
            string retVal = "";
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public StageLotData GetStageInfo()
        {
            StageLotData tmp = new StageLotData();
            return tmp;
        }

        public void ReadStatusSoakingChillingTime(ref long _ChillingTimeMax, ref long _ChillingTime, ref SoakingStateEnum CurStatusSoaking_State)
        {
        }

        public bool GetShowStatusSoakingSettingPageToggleValue()
        {
            return false;
        }

        public void Check_N_ClearStatusSoaking()
        {

        }

        public bool IsEnablePolishWaferSoaking()
        {
            return false;
        }

        public int GetSlotIndex()
        {
            int retVal = 0;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetStageMode(GPCellModeEnum mode)
        {
            return EventCodeEnum.NONE;
        }

        public void SetStreamingMode(StreamingModeEnum mode)
        {

        }
        public void SetForcedDone(EnumModuleForcedState flag)
        {

        }
        public void CellProbingResume()
        {

        }
        public void SetStilProbingZUp(bool flag = true)
        {

        }
      
        public void SetLotLoadingPosCheckMode(bool flag)
        {

        }
        #endregion

        public EventCodeEnum WaferCancel()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception)
            {

            }
            return retVal;
        }
        public EnumWaferState GetWaferState()
        {
            EnumWaferState waferState = EnumWaferState.PROCESSED;
            return waferState;
        }
        public void SetWaferStateOnChuck(EnumWaferState waferstate)
        {
            return;
        }

        public bool GetRunState(bool isTransfer = false)
        {
            return true; //카드 체인저도 포함되어야한다.
        }
        public bool GetMovingState()
        {
            return true;
        }

        public bool IsCardUpModuleUp()
        {
            return false;
        }
        public double GetSetTemp()
        {
            return 0.0;
        }
        public EventCodeEnum ReserveErrorEnd(string ErrorMessage = "Paused by host(CELL ABORT TEST).")
        {
            return EventCodeEnum.NONE;
        }
        public ErrorEndStateEnum GetErrorEndState()
        {
            ErrorEndStateEnum retVal = ErrorEndStateEnum.NONE;

            return retVal;
        }
        public void SetRecipeDownloadEnable(bool flag)
        {

            return;
        }
        public EventCodeEnum SetAbort(bool isAbort, bool isForced = false)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            return retVal;
        }
        public bool GetReservePause()
        {
            return false;
        }

        public void CancelCellReservePause()
        {
            return;
        }
        public void CancelLot(int foupnumber, bool iscellend, string lotID, string cstHashCode)
        {
            return;
        }
        public EventCodeEnum GetAngleInfo(out double notchAngle, out double slotAngle, out double ocrAngle)
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            notchAngle = 0;
            slotAngle = 0;
            ocrAngle = 0;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum CanWaferUnloadRecovery(ref bool canrecovery, ref ModuleStateEnum wafertransferstate)
        {
            canrecovery = true;
            wafertransferstate = ModuleStateEnum.ERROR;
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum NotifyLotEnd(int foupNumber, string lotID)
        {
            return EventCodeEnum.NONE;
        }
        public string GetPauseReason()
        {
            return "";
        }
        public bool GetErrorEndFlag()
        {
            return false;
        }
        public void SetErrorEndFalg(bool flag)
        {
        }

        public bool GetTesterAvailableData()
        {
            return false;
        }

        public EventCodeEnum DoPinAlign()
        {
            return EventCodeEnum.NONE;
        }
        public void SetCardStatus(bool isExist, string id, bool isDocked = false)
        {
        }

        public ModuleStateEnum GetSetSoakingState()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMoveLockState(ReasonOfStageMoveLock reason)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMoveUnLockState(ReasonOfStageMoveLock reason)
        {
            throw new NotImplementedException();
        }
        public void SetStopBeforeProbingFlag(bool flag, int stageidx = 0)
        {

        }

        public void SetStopAfterProbingFlag(bool flag, int stageidx = 0)
        {

        }
        public void SetOnceStopBeforeProbingFlag(bool flag, int stageidx = 0)
        {

        }

        public void SetOnceStopAfterProbingFlag(bool flag, int stageidx = 0)
        {

        }
        public StageLockMode GetStageLock()
        {
            return StageLockMode.UNLOCK;
        }
        public void ResetAssignLotInfo(int foupnumber, string lotid, string cstHashCode)
        {

        }
        public CatCoordinates GetPMShifhtValue()
        {
            throw new NotImplementedException();
        }

        public void SetForcedDoneSpecificModule(EnumModuleForcedState flag, ModuleEnum moduleEnum)
        {

        }

        public List<ReasonOfStageMoveLock> GetReasonofLockFromClient()
        {
            throw new NotImplementedException();
        }

        public ModuleEnum[] GetForcedDoneModules()
        {
            ModuleEnum[] tmp = new ModuleEnum[12];
            for (int i = 0; i < 12; i++)
            {
                tmp[i] = ModuleEnum.WaferAlign;
            }
            return tmp;
        }

        public void IsAlignDoing(ref bool pinAlignDoing, ref bool waferAlignDoing)
        {
            throw new NotImplementedException();
        }

        public bool IsNeedLotEnd()
        {
            return false;
        }
        public bool IsLotAbort()
        {
            throw new NotImplementedException();
        }

        public bool GetLotOutState()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum GetSoakingModuleState()
        {
            ModuleStateEnum retVal = ModuleStateEnum.IDLE;
            try
            {
                //retVal = this.StageSupervisor().StageModuleState.StageMoveUnLockState();
             //   retVal = this.SoakingModule().ModuleState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetLotOut(bool islotout)
        {
            //throw new NotImplementedException();
        }
        public ProberInterfaces.CardChange.EnumCardChangeType GetCardChangeType()
        {
            ProberInterfaces.CardChange.EnumCardChangeType retVal = ProberInterfaces.CardChange.EnumCardChangeType.UNDEFINED;
            try
            {
                retVal = ProberInterfaces.CardChange.EnumCardChangeType.DIRECT_CARD;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum GetCardIDValidateResult(string CardID, out string Msg)
        {
            Msg = "";
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ChangeWaferStatus(EnumSubsStatus status, out bool iswaferhold)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            iswaferhold = false;
            return retVal;
        }
        public TransferObject GetTransferObjectToSlotInfo()
        {
            TransferObject transferObj = new TransferObject();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return transferObj;
        }

        public EventCodeEnum CheckSetValueAvailable(string propertypath, object val, out string errorlog)//, string source_classname = null)
        {
            errorlog = "";
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetValue(string propertypath, Object val, bool isNeedValidation = false)//, string source_classname = null)
        {            
            return EventCodeEnum.NONE;
        }
        public bool CanRunningLot()
        {
            return true;
        }


        public byte[] GetMonitoringBehaviorFromClient()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ManualRecoveryToStage(int behaviorIndex)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearHandlerStatus()
        {
            throw new NotImplementedException();
        }
        public void LotCancelSoakingAbort(int stageidx)
        {
            return;
        }

        public bool CheckCurrentAssignLotInfo(string lotID, string cstHashCode)
        {
            return true;
        }

        public bool IsCanPerformLotStart()
        {
            return false;
        }

        public void SetCellModeChanging()
        {
            throw new NotImplementedException();
        }

        public void ResetCellModeChanging()
        {
            throw new NotImplementedException();
        }

        public List<StageLotInfo> GetStageLotInfos()
        {
            return null;
        }
        public bool IsHasProcessingLotAssignState()
        {
            return false;
        }
        public EventCodeEnum CardChangeIsConditionSatisfied(bool needToSetTempToken)
        {
            return EventCodeEnum.NONE;
        }

        public bool NeedToSetCCActivatableTemp()
        {
            return false;
        }


        public EventCodeEnum IsAvailableToSetOtherThanCCActiveTemp()
        {          
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum RecoveryCCBeforeTemp()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetCCActiveTemp()
        {
            return EventCodeEnum.NONE;
        }
        public void AbortCardChange()
        {
            return;
        }
        public EventCodeEnum GetCardPodState()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum GetNotchTypeInfo(out WaferNotchTypeEnum notchType)
        {
            notchType = WaferNotchTypeEnum.NOTCH;
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ChangeWaferStatus(EnumSubsStatus status, out bool iswaferhold, out string errormsg)
        {
            throw new NotImplementedException();
        }
    }
}

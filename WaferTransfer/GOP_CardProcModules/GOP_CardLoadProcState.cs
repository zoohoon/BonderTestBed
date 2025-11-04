using System;
using System.Threading.Tasks;
using ProberInterfaces.WaferTransfer;
using ProberInterfaces;
using ProberErrorCode;
using NotifyEventModule;
using LogModule;
using LoaderController.GPController;
using SequenceRunner;
using ProberInterfaces.Enum;
using System.Threading;
using ProberInterfaces.Event;
using MetroDialogInterfaces;

namespace WaferTransfer.GOP_CardLoadProcStates
{
    public abstract class GOP_CardLoadProcState
    {
        public GOP_CardLoadProcModule Module { get; set; }

        public GOP_CardLoadProcState(GOP_CardLoadProcModule module)
        {
            this.Module = module;
        }

        public void StateTransition(GOP_CardLoadProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }

        public abstract WaferTransferProcStateEnum State { get; }

        public abstract void Execute();

        public abstract void SelfRecovery();

    }

    public class IdleState : GOP_CardLoadProcState
    {
        public IdleState(GOP_CardLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new RunningState(Module));
        }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class RunningState : GOP_CardLoadProcState
    {
        public RunningState(GOP_CardLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {

                bool usinghandler = Module.StageSupervisor().CheckUsingHandler(Module.LoaderController().GetChuckIndex());
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (Module.TCPIPModule().GetTCPIPOnOff() == EnumTCPIPEnable.ENABLE)
                {
                    string CardID = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardID();
                    var iscard = string.IsNullOrEmpty(CardID) == false && CardID.ToLower() != "holder";
                    var Seqtimeout = (Module.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).WaitTesterResponse.Value;
                    if (Seqtimeout == true && iscard)
                    {
                        // -> todo : tcpip 통신 중인가 체크하는 로직으로 변경 되어야함

                        Module.TCPIPModule().CheckAndConnect();

                        SemaphoreSlim Semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CardChangeVaildationStartEvent).FullName, new ProbeEventArgs(this, Semaphore, CardID));
                        Semaphore.Wait();

                        var cardchangevaildation = new GOP_CardChangeVaildation(CardID);
                        retVal = Module.CardChangeModule().BehaviorRun(cardchangevaildation).Result;
                        if (retVal != EventCodeEnum.NONE)
                        {
                            try
                            {
                                Module.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                                Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                this.Module.MetroDialogManager().ShowMessageDialog("[CARD_CHANGE_FAIL]", $"{Module.CardChangeModule().ReasonOfError.GetLastEventMessage()} ", EnumMessageStyle.Affirmative);
                                LoggerManager.Debug($"[GOP_CardChangeVaildation Error] (GOP_CardChangeVaildation) RetVal:{retVal}");

                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_OriginCardPut();
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    this.Module.MetroDialogManager().ShowMessageDialog("[CARD_CHANGE_FAIL]", $"Card Put", EnumMessageStyle.Affirmative);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                            finally 
                            {
                                Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                            }
                            // 논의 필요
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"GOP_CardChangeVaildation(): Tester Interlock Seq Skip [{Seqtimeout} , {CardID}]");
                    }
                }

                var ccsuperviosrState = (Module.LoaderController() as GP_LoaderController).GPLoaderService.GetPGVCardChangeState();

                if (Module.CardChangeModule().GetCardIDValidationEnable() && ccsuperviosrState == ModuleStateEnum.IDLE)
                {
                    string CardID = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardID();
                    var isCard = !string.IsNullOrEmpty(CardID) && CardID.ToLower() != "holder";

                    if (isCard)
                    {
                        string Msg = "";
                        bool retry = true;

                        while (retry)
                        {
                            try
                            {
                                Msg = "";
                                retVal = Module.CardChangeModule().CardIDValidate(CardID, out Msg);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    var answer = this.Module.MetroDialogManager().ShowMessageDialog("[CARD_CHANGE_FAIL]", $"Fail Card ID: [{CardID}]\n{Msg}\nReturn the card to the origin, or input a new ID?", EnumMessageStyle.AffirmativeAndNegative, "Card Return", "Card Input").Result;

                                    // Return
                                    if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                                    {
                                        Msg += " User chose to return the card to the original position.";
                                        LoggerManager.Debug($"[GOP_CardLoadProcState Error] User chose to return the card to the original position. Msg: {Msg}");
                                        retry = false;
                                        break;
                                    }
                                    // Input
                                    else
                                    {
                                        string userCardIdInput = string.Empty;

                                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_UserCardIDInput(out userCardIdInput);

                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            CardID = userCardIdInput;
                                            LoggerManager.Debug($"[GOP_CardLoadProcState Error] User entered new Card ID: {CardID}");
                                            retry = true;
                                            continue;
                                        }
                                        else if (retVal == EventCodeEnum.CARDCHANGE_CARD_ID_NULL)
                                        {
                                            Msg += " The user canceled the card ID input and returned to the original position.";
                                            LoggerManager.Debug($"[GOP_CardLoadProcState Error] User canceled the card ID input. Msg: {Msg}");
                                            retry = false;
                                            break;
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"[GOP_CardLoadProcState Error] An error occurred in the user input section.");
                                            retry = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"[GOP_CardLoadProcState Success] Card ID validation succeeded for CardID: {CardID}");
                                    break;
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                                break;
                            }
                        }

                        if (retVal != EventCodeEnum.NONE)
                        {
                            this.Module.MetroDialogManager().ShowMessageDialog("[CARD_CHANGE_FAIL]", $"Fail Card ID: [{CardID}]\n{Msg}\n", EnumMessageStyle.Affirmative);
                            var ret = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_OriginCardPut();
                            if (ret != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"[GOP_CardLoadProcState Error] Failed to return card to origin. RetVal: {ret}");
                            }
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GOP_CardLoadProcState Error] (CardIDValidate failed!) RetVal: {retVal}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"Tester Interlock Seq Skip [CardID: {CardID}]");
                    }
                }

                //WMB 식별 코드가 안맞을 경우 에러 
                var interfaceValidation = new GOP_WMBIdentifierVaildation();
                retVal = Module.CardChangeModule().BehaviorRun(interfaceValidation).Result;
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_WMBIdentifierVaildation) RetVal:{retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                // retract wafer cam
                var retractWaferCam = new GOP_RetractWaferCam();
                retVal = Module.CardChangeModule().BehaviorRun(retractWaferCam).Result;
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_RetractWaferCam) RetVal:{retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                
                retVal = Module.StageSupervisor().StageModuleState.ZCLEARED();//-->IDLE
                retVal = Module.StageSupervisor().StageModuleState.CCZCLEARED();//-->CC
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GOP_CardLoadProcState Error] (MoveToCardHolderPositionAndCheck error) RetVal:{retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                var checkPCardIsNotOnTopPlate = new GOP_CheckPCardIsNotOnTopPlate();
                retVal = Module.CardChangeModule().BehaviorRun(checkPCardIsNotOnTopPlate).Result;
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_CheckPCardIsNotOnTopPlate) RetVal:{retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                //card not on pod check + raise chuck + set IsCardExist(holder docked)

                bool MoveToCardHolderPosEnable = (Module.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).MoveToCardHolderPosEnable;
                if(MoveToCardHolderPosEnable == true)
                {
                    var prev = (Module.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsCardExist;
                    retVal = Module.StageSupervisor().StageModuleState.MoveToCardHolderPositionAndCheck();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[GOP_CardLoadProcState Error] (MoveToCardHolderPositionAndCheck error) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    if ((Module.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsCardExist != prev)
                    {
                        this.Module.MetroDialogManager().ShowMessageDialog("[Information]", $"CELL{Module.LoaderController().GetChuckIndex()} Car State Changed. Card Exist {prev} to {(Module.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsCardExist} ", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.GP_CardChange_CARD_STATE_CHANGED, description: $"CELL{Module.LoaderController().GetChuckIndex()} Car State Changed. Card Exist {prev} to {(Module.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsCardExist} ");
                        Module.CardChangeModule().SaveSysParameter();
                    }

                }

                bool waitForLoadingPos = true;
                Task.Run(() =>
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_Card_MoveLoadingPosition();
                    waitForLoadingPos = false;
                });

                var moveSafeFromDoor = new GOP_MoveSafeFromDoor();
                retVal = Module.CardChangeModule().BehaviorRun(moveSafeFromDoor).Result;
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_MoveSafeFromDoor) RetVal:{retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                retVal = Module.StageSupervisor().StageModuleState.CardDoorOpen();
                Task.Delay(1000).Wait();
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                var readyToGetCard = new GOP_ReadyToGetCard();
                retVal = Module.CardChangeModule().BehaviorRun(readyToGetCard).Result;
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_ReadyToGetCard error) RetVal:{retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                while (waitForLoadingPos)
                {
                    Task.Delay(10).Wait();
                }

                string probeCardID = "";
                TransferObject loadedObject;
                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangePut(out loadedObject);
                if (retVal != EventCodeEnum.NONE)
                {                    
                    //실패일 때 Pod에 놓고 실패했는지 Arm이 잡고있는 상태로 실패했는지 체크
                    if (retVal == EventCodeEnum.LOADER_PUT_ERROR_BUT_NOTEXIST_IN_ARM)// Arm은 들고있는 카드가 없다고 뜸
                    {
                        var dockPCardTopPlate = new GOP_CheckPCardIsOnPCardPod();//Cell에 Object가 올라와 있는지 체크
                        retVal = Module.CardChangeModule().BehaviorRun(dockPCardTopPlate).Result;
                        if (retVal == EventCodeEnum.NONE)//Pod에 Card 올려져 있으면 None
                        {
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_SetTransferAfterCardChangePutError(out TransferObject transObj, EnumWaferState.UNPROCESSED);//SetTransfer하는 함수
                        }
                        else
                        {
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_SetTransferAfterCardChangePutError(out TransferObject transObj, EnumWaferState.UNDEFINED);//SetTransfer하는 함수
                        }
                    }
                    probeCardID = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardID();

                    Module.CardChangeModule().SetProbeCardID(probeCardID);

                    this.Module.LoaderController().SetCardStatus(true, probeCardID);

                    Module.GetParam_ProbeCard().ProbeCardDevObjectRef.TouchdownCount.Value = 0;
                    Module.GEMModule().GetPIVContainer().PCardContactCount.Value = 0;

                    Module.GEMModule().GetPIVContainer().SetProberCardID(probeCardID);

                    Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                    Module.CardChangeModule().StartCardDockingFlag = false;

                    LoggerManager.Debug($"[GOP_CardLoadProcState Error] (CTR_CardChangePut error) RetVal:{retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                
                probeCardID = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardID();

                Module.CardChangeModule().SetProbeCardID(probeCardID);
                
                this.Module.LoaderController().SetCardStatus(true, probeCardID);//IsCardExist, IsDocked업데이트

                Module.CardChangeModule().LoaderNotifyCardStatus();
              
                Module.GetParam_ProbeCard().ProbeCardDevObjectRef.TouchdownCount.Value = 0;
                Module.GEMModule().GetPIVContainer().PCardContactCount.Value = 0;

                Module.GEMModule().GetPIVContainer().SetProberCardID(probeCardID);
                //this.Module.GEMModule().GetPIVContainer().SetStageRecipeInfo(Module.LoaderController().GetChuckIndex(), cardid: probeCardID );

                var iscardholder = string.IsNullOrEmpty(probeCardID) || probeCardID.ToLower() == "holder";
                if (iscardholder)
                {
                    LoggerManager.Debug($"[GOP_CardLoadProcState] probeCardID is {probeCardID}, skip check for zif connet condition.");
                }
                else
                {
                    // TH DOCK
                    var checkThDockReady = new GOP_THDockReady();
                    retVal = Module.CardChangeModule().BehaviorRun(checkThDockReady).Result;
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_THDockReady error) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }
                //    Module.StageSupervisor().ProbeCardInfo.SetProbeCardID(probeCardID);
                Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                Module.CardChangeModule().StartCardDockingFlag = false;

                if (loadedObject == null)
                {
                    LoggerManager.Debug($"[GOP_CardLoadProcState Error] (Load Card Object Null) ");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                else
                {
                    //this.Module.GetParam_ProbeCard().SetProbeCardID(loadedObject.ProbeCardID.Value);
                   
                    if (loadedObject.CardSkip == CardSkipEnum.NONE)
                    {
                        Module.GEMModule().GetPIVContainer().ProbeCardID.Value = probeCardID;// CardDockEvent 에서 불러주기 위함.                                               

                        LoggerManager.ActionLog(ModuleLogType.CARD_DOCK, StateLogType.START, $"ProbeCard ID:{probeCardID}", this.Module.LoaderController().GetChuckIndex());
                        var dockPCardTopPlate = new GOP_DockPCardTopPlate();

                        retVal = Module.CardChangeModule().BehaviorRun(dockPCardTopPlate).Result;

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.ActionLog(ModuleLogType.CARD_DOCK, StateLogType.ERROR, $"Reason : {retVal}", this.Module.LoaderController().GetChuckIndex());
                            LoggerManager.Debug($"[GOP_CardLoadProcState Error or Pause] (GOP_DockPCardTopPlate) RetVal:{retVal}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                        else
                        {
                            LoggerManager.ActionLog(ModuleLogType.CARD_DOCK, StateLogType.DONE, $"ProbeCard ID:{probeCardID}", this.Module.LoaderController().GetChuckIndex());

                            moveSafeFromDoor = new GOP_MoveSafeFromDoor();
                            retVal = Module.CardChangeModule().BehaviorRun(moveSafeFromDoor).Result;
                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_MoveSafeFromDoor) RetVal:{retVal}");
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }

                            retVal = Module.StageSupervisor().StageModuleState.CardDoorOpen();
                            Task.Delay(1000).Wait();
                            if (retVal != EventCodeEnum.NONE)
                            {
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }

                            var returnCardCarrier = new GOP_ReturnCardCarrier();
                            retVal = Module.CardChangeModule().BehaviorRun(returnCardCarrier).Result;
                            if (retVal != EventCodeEnum.NONE)
                            {
                                Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                LoggerManager.Debug($"[GOP_ReturnCardCarrier Error] (GOP_ReturnCardCarrier error) RetVal:{retVal}");
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }

                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangeCarrierPick();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                moveSafeFromDoor = new GOP_MoveSafeFromDoor();
                                retVal = Module.CardChangeModule().BehaviorRun(moveSafeFromDoor).Result;
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_MoveSafeFromDoor) RetVal:{retVal}");
                                    StateTransition(new SystemErrorState(Module));
                                    return;
                                }

                                retVal = Module.StageSupervisor().StageModuleState.CardDoorClose();
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    StateTransition(new SystemErrorState(Module));
                                    return;
                                }

                                iscardholder = string.IsNullOrEmpty(probeCardID) || probeCardID.ToLower() == "holder";
                                if (iscardholder)
                                {
                                    LoggerManager.Debug($"[GOP_CardLoadProcState] probeCardID is {probeCardID}, skip zip lock.");
                                }
                                else
                                {
                                    //TODO: 테스터 인터페이스와 관련하여 타입을 만들어서 서로 영향을 안주도록 한다. 

                                    // TH LOCK READY - TWICE
                                    var checkThDockReady = new GOP_THDockReady();
                                    retVal = Module.CardChangeModule().BehaviorRun(checkThDockReady).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_THDockReady) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                    // PC SPC LOCK REQ 
                                    var zifcommandreq = new Request_ZIFCommandLowActive(); // 다른 사이트에 영향을 줄 수 있음 delay time 
                                    retVal = Module.CardChangeModule().BehaviorRun(zifcommandreq).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[GOP_CardLoadProcState Error] (Request_ZIFCommandLowActive) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                    // Tester - Prober Lock
                                    var testerHeadRotLock = new TesterHeadRotLock();
                                    retVal = Module.CardChangeModule().BehaviorRun(testerHeadRotLock).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[GOP_CardLoadProcState Error] (TesterHeadRotLock) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                    // TH DOCK CHECK 
                                    var thdockcleared = new GOP_THDockClearedCheck();
                                    retVal = Module.CardChangeModule().BehaviorRun(thdockcleared).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_THDockClearedCheck) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                }

                                retVal = Module.CardChangeModule().LoaderNotifyCardStatus();

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"[GOP_CardLoadProcState Error] LoaderNotifyCardStatus() RetVal:{retVal}");
                                }

                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_OriginCarrierPut();

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(true);
                                }
                                else
                                {
                                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                                }

                                var clearCardDocking = new GOP_ClearCardDocking();
                                
                                retVal = Module.CardChangeModule().BehaviorRun(clearCardDocking).Result;

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    

                                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                    LoggerManager.Debug($"[GOP_ClearCardDocking Error] (GOP_ClearCardDocking error) RetVal:{retVal}");
                                    StateTransition(new SystemErrorState(Module));
                                    return;
                                }
                            }
                            else
                            {
                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                            }

                        }

                    }
                }
                
                StateTransition(new DoneState(Module));

                PIVInfo pivinfo = new PIVInfo(probecardid: probeCardID);
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(CardLoadingEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();

                //Module.EventManager().RaisingEvent(typeof(CardLoadingEvent).FullName);
                //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(CardLoadingEvent).FullName));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($ex);
                LoggerManager.Exception(err);

                StateTransition(new SystemErrorState(Module));
            }
        }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class DoneState : GOP_CardLoadProcState
    {
        public DoneState(GOP_CardLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SystemErrorState : GOP_CardLoadProcState
    {
        public SystemErrorState(GOP_CardLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            //No WOKRS.
        }
    }

}

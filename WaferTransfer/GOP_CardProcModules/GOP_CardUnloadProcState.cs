using System;
using System.Threading.Tasks;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.WaferTransfer;
using LoaderControllerBase;
using NotifyEventModule;
using LogModule;
using LoaderController.GPController;
using SequenceRunner;
using System.Threading;
using ProberInterfaces.Event;
using MetroDialogInterfaces;

namespace WaferTransfer.GOP_CardUnLoadProcStates
{
    public abstract class GOP_CardUnLoadProcState
    {
        public GOP_CardUnLoadProcModule Module { get; set; }

        public GOP_CardUnLoadProcState(GOP_CardUnLoadProcModule module)
        {
            this.Module = module;
        }

        public void StateTransition(GOP_CardUnLoadProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }

        public abstract WaferTransferProcStateEnum State { get; }

        public abstract void Execute();

        public abstract void SelfRecovery();
    }

    public class IdleState : GOP_CardUnLoadProcState
    {
        public IdleState(GOP_CardUnLoadProcModule module) : base(module) { }

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

    public class RunningState : GOP_CardUnLoadProcState
    {
        public RunningState(GOP_CardUnLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.RUNNING;
        /*
        private long CHUCK_VAC_CHECK_TIME = 500;
        private long CHUCK_VAC_WAIT_TIME = 10000;
        private long CHUCK_THREELEG_WAIT_TIME = 10000;
        */
        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;


                EventCodeEnum ret = EventCodeEnum.UNDEFINED;
                if (Extensions_IParam.ProberRunMode != RunMode.EMUL)
                {
                    var CheckCarrierIsOnPCardPod = new GOP_ReturnCardPodState();
                    ret = Module.CardChangeModule().BehaviorRun(CheckCarrierIsOnPCardPod).Result;
                }
                else
                {
                    ret = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                }
                
                
                if (ret == EventCodeEnum.GP_CardChange_CARRIER_ON_POD) // Card는 Docking되어있는 상태에서 캐리어만 가지고 가라 하기.
                {
                    //====> MoveSafeFromDoor 
                    var movesafefromdoor = new GOP_MoveSafeFromDoor();
                    retVal = Module.CardChangeModule().BehaviorRun(movesafefromdoor).Result;
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveSafeFromDoor) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                    //====> CardDoorOpen 
                    retVal = Module.StageSupervisor().StageModuleState.CardDoorOpen();
                    Task.Delay(1000).Wait();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (CardDoorOpen) RetVal:{retVal}");
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
                        movesafefromdoor = new GOP_MoveSafeFromDoor();
                        retVal = Module.CardChangeModule().BehaviorRun(movesafefromdoor).Result;
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

                        //retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_OriginCarrierPut();
                        //if (retVal == EventCodeEnum.NONE)
                        //{
                        //    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(true);
                        //    StateTransition(new DoneState(Module));
                        //}
                        //else
                        //{
                        //    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                        //    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        //    LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveChuckToLoader Error) RetVal:{retVal}");
                        //    StateTransition(new SystemErrorState(Module));
                        //    return;
                        //}

                        var clearCardDocking = new GOP_ClearCardDocking();
                        retVal = Module.CardChangeModule().BehaviorRun(clearCardDocking).Result;
                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GOP_ClearCardDocking Error] (GOP_ClearCardDocking error) RetVal:{retVal}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(true);
                        StateTransition(new DoneState(Module));

                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        Module.EventManager().RaisingEvent(typeof(CardUnloadedEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();

                    }
                    else
                    {
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_ReturnCardCarrier Error] (GOP_ReturnCardCarrier error) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                    

                }
               
                else if(ret == EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD)//Carrier, Holder, Card 모두 Pod위에 있을 때 Unload
                {
                  var  movesafefromdoor = new GOP_MoveSafeFromDoor();
                    retVal = Module.CardChangeModule().BehaviorRun(movesafefromdoor).Result;
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveSafeFromDoor) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    //====> CardDoorOpen 
                    retVal = Module.StageSupervisor().StageModuleState.CardDoorOpen();
                    Task.Delay(1000).Wait();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (CardDoorOpen) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    //====> MoveChuckToLoader 
                   var movetoloader = new GOP_MoveChuckToLoader();
                    retVal = Module.CardChangeModule().BehaviorRun(movetoloader).Result;
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveChuckToLoader) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }


                    //====> Loader: CTR_CardChangePick
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangePick();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (CTR_CardChangePick) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                    this.Module.LoaderController().SetCardStatus(false, "");

                    //====> GEM: CardUnloadedEvent
                    Module.CardChangeModule().SetProbeCardID("");
                    Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                    //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    //Module.EventManager().RaisingEvent(typeof(CardUnloadedEvent).FullName, new ProbeEventArgs(this, semaphore));
                    //semaphore.Wait();

                    //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(CardUnloadedEvent).FullName));

                    // ====> ClearCardChange : check card exist, down card pod, cc zcleard 
                    var clearCardChange = new GOP_ClearCardChange();
                    retVal = Module.CardChangeModule().BehaviorRun(clearCardChange).Result;
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_ClearCardChange) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }


                    var moveSafeFromDoor = new GOP_MoveSafeFromDoor();
                    retVal = Module.CardChangeModule().BehaviorRun(moveSafeFromDoor).Result;
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveSafeFromDoor) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    // ====> CardDoorClose
                    retVal = Module.StageSupervisor().StageModuleState.CardDoorClose();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    // ====> Done
                    retVal = Module.StageSupervisor().StageModuleState.ZCLEARED();//-->IDLE
                    StateTransition(new DoneState(Module));

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    Module.EventManager().RaisingEvent(typeof(CardUnloadedEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();



                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(true);
                }
                else if (ret == EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD)//Pod 위에 아무것도 없을 때 Unload
                {
                    // retract wafer cam
                    var retractWaferCam = new GOP_RetractWaferCam();
                    ret = Module.CardChangeModule().BehaviorRun(retractWaferCam).Result;
                    if (ret != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_RetractWaferCam) RetVal:{ret}");
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

                    var checkPCardIsOnTopPlate = new GOP_CheckPCardIsOnTopPlate();
                    retVal = Module.CardChangeModule().BehaviorRun(checkPCardIsOnTopPlate).Result;
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardLoadProcState Error] (GOP_CheckPCardIsOnTopPlate) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }


                    bool MoveToCardHolderPosEnable = (Module.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).MoveToCardHolderPosEnable;
                    if (MoveToCardHolderPosEnable == true)
                    {
                        var prev = (Module.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsCardExist;
                        retVal = Module.StageSupervisor().StageModuleState.MoveToCardHolderPositionAndCheck();
                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[GOP_CardUnLoadProcModule Error] (MoveToCardHolderPositionAndCheck error) RetVal:{retVal}");
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

                    var probeCardID = Module.CardChangeModule().GetProbeCardID();
                    var iscardholder = string.IsNullOrEmpty(probeCardID) || probeCardID.ToLower() == "holder";
                    if (iscardholder)
                    {
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState] probeCardID is {probeCardID}, skip zip unlock.");
                    }
                    else 
                    {

                        // TH UNDOCK READY
                        var checkTHUndockReady = new GOP_THUndockReady();
                        retVal = Module.CardChangeModule().BehaviorRun(checkTHUndockReady).Result;
                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_THUndockReady) RetVal:{retVal}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }                       

                        // Tester - Prober Unlock
                        var testerHeadRotunlock = new TesterHeadRotUnlock();
                        retVal = Module.CardChangeModule().BehaviorRun(testerHeadRotunlock).Result;
                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[GOP_CardLoadProcState Error] (TesterHeadRotUnlock) RetVal:{retVal}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }

                        // PC SPC UNLOCK REQ 
                        var zifcommand = new Request_ZIFCommandLowActive();
                        retVal = Module.CardChangeModule().BehaviorRun(zifcommand).Result;
                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (zifcommand) RetVal:{retVal}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }                     

                        // TH UNDOCK CHECK
                        var checkTHUndockCleared = new GOP_THUndockClearedCheck();
                        retVal = Module.CardChangeModule().BehaviorRun(checkTHUndockCleared).Result;
                        if (retVal == EventCodeEnum.NONE)
                        {
                            retVal = Module.CardChangeModule().LoaderNotifyCardStatus();
                        }
                        else
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (checkTHUndockCleared) RetVal:{retVal}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }

                    }
                   

                    //====>Loader: CTR_Card_MoveLoadingPosition
                    bool waitForLoadingPos = true;
                    bool isSucessReadyJob = false;
                    bool isErrorOut = false;
                    Task.Run(() =>
                    {
                        try
                        {
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_OriginCarrierPick();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_Card_MoveLoadingPosition();
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    isSucessReadyJob = true;
                                }
                                else
                                {
                                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                                    StateTransition(new SystemErrorState(Module));
                                    isErrorOut = true;
                                }
                            }
                            else
                            {
                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                                StateTransition(new SystemErrorState(Module));
                                isErrorOut = true;
                            }
                            waitForLoadingPos = false;
                        }
                        catch (Exception err)
                        {
                            waitForLoadingPos = false;
                            StateTransition(new SystemErrorState(Module));
                            isErrorOut = true;
                            LoggerManager.Exception(err);
                        }

                    });


                    while (waitForLoadingPos)
                    {
                        Task.Delay(10).Wait();

                    }

                    if (isErrorOut)
                    {
                        return;
                    }




                    //====> ReadyToOpenDoor                
                    var readyToOpenDoor = new GOP_MoveSafeFromDoor(); 
                    retVal = Module.CardChangeModule().BehaviorRun(readyToOpenDoor).Result;
                    if (retVal != EventCodeEnum.NONE)
                    {
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveSafeFromDoor error) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }




                    if (isSucessReadyJob)
                    {
                        //====> CardDoorOpen : CTR_CardChangeCarrierPut 떄문에 지금 열어야함. 
                        retVal = Module.StageSupervisor().StageModuleState.CardDoorOpen();
                        Task.Delay(1000).Wait();
                        if (retVal != EventCodeEnum.NONE)
                        {
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (CardDoorOpen Error) RetVal:{retVal}");
                            StateTransition(new SystemErrorState(Module));
                        }


                        //====> MoveChuckToLoader
                        var movetoloader = new GOP_MoveChuckToLoader();
                        retVal = Module.CardChangeModule().BehaviorRun(movetoloader).Result;
                        if (retVal == EventCodeEnum.NONE)
                        {

                            if (retVal == EventCodeEnum.NONE)
                            {
                                // ===> RaisePCardPod : Z up
                                var raisePCardPod = new GOP_RaisePCardPod();
                                retVal = Module.CardChangeModule().BehaviorRun(raisePCardPod).Result;
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                    LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_RaisePCardPod) RetVal:{retVal}");
                                    StateTransition(new SystemErrorState(Module));
                                    return;
                                }

                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangeCarrierPut();
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    //====> MoveSafeFromDoor 
                                    var movesafefromdoor = new GOP_MoveSafeFromDoor();
                                    retVal = Module.CardChangeModule().BehaviorRun(movesafefromdoor).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveSafeFromDoor) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                    // ====> CardDoorClose
                                    retVal = Module.StageSupervisor().StageModuleState.CardDoorClose();
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }


                                    //====> UndockPCardTopPlate
                                    var undockPCardTopPlate = new GOP_UndockPCardTopPlate();
                                    retVal = Module.CardChangeModule().BehaviorRun(undockPCardTopPlate).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_UndockPCardTopPlate) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                    //====> MoveSafeFromDoor 
                                    movesafefromdoor = new GOP_MoveSafeFromDoor();
                                    retVal = Module.CardChangeModule().BehaviorRun(movesafefromdoor).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveSafeFromDoor) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                    //====> CardDoorOpen 
                                    retVal = Module.StageSupervisor().StageModuleState.CardDoorOpen();
                                    Task.Delay(1000).Wait();

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (CardDoorOpen) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                    //====> MoveChuckToLoader 
                                    movetoloader = new GOP_MoveChuckToLoader();
                                    retVal = Module.CardChangeModule().BehaviorRun(movetoloader).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveChuckToLoader) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }


                                    //====> Loader: CTR_CardChangePick
                                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangePick();
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (CTR_CardChangePick) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }
                                    this.Module.LoaderController().SetCardStatus(false, "");
                                    Module.CardChangeModule().LoaderNotifyCardStatus();

                                    Module.GEMModule().GetPIVContainer().SetProberCardID("");
                                //this.Module.GEMModule().GetPIVContainer().SetStageRecipeInfo(Module.LoaderController().GetChuckIndex(), cardid: probeCardID );


                                //====> GEM: CardUnloadedEvent
                                Module.CardChangeModule().SetProbeCardID("");
                                Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                                Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                                    //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    //Module.EventManager().RaisingEvent(typeof(CardUnloadedEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    //semaphore.Wait();

                                    //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(CardUnloadedEvent).FullName));

                                    // ====> ClearCardChange : check card exist, down card pod, cc zcleard 
                                    var clearCardChange = new GOP_ClearCardChange();
                                    retVal = Module.CardChangeModule().BehaviorRun(clearCardChange).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_ClearCardChange) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }


                                    var moveSafeFromDoor = new GOP_MoveSafeFromDoor();
                                    retVal = Module.CardChangeModule().BehaviorRun(moveSafeFromDoor).Result;
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveSafeFromDoor) RetVal:{retVal}");
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                    // ====> CardDoorClose
                                    retVal = Module.StageSupervisor().StageModuleState.CardDoorClose();
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                        StateTransition(new SystemErrorState(Module));
                                        return;
                                    }

                                    // ====> Done
                                    retVal = Module.StageSupervisor().StageModuleState.ZCLEARED();//-->IDLE
                                    StateTransition(new DoneState(Module));

                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    Module.EventManager().RaisingEvent(typeof(CardUnloadedEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();



                                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(true);
                                }
                                else
                                {
                                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                    LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (CTR_CardChangeCarrierPut Error) RetVal:{retVal}");
                                    StateTransition(new SystemErrorState(Module));
                                }
                            }
                            else
                            {
                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                                Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (CardDoorOpen Error) RetVal:{retVal}");
                                StateTransition(new SystemErrorState(Module));
                            }
                        }
                        else
                        {
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GOP_CardUnLoadProcState Error] (GOP_MoveChuckToLoader Error) RetVal:{retVal}");
                            StateTransition(new SystemErrorState(Module));
                        }
                    }
                    else
                    {
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                    }
                }
                else 
                {
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    this.Module.MetroDialogManager().ShowMessageDialog("[Information]", $"Card UnLoading or Carrier UnLoding Fail ", EnumMessageStyle.AffirmativeAndNegative);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

            }
            catch (Exception err)
            {
                (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardTransferDone(false);
                Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
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

    public class DoneState : GOP_CardUnLoadProcState
    {
        public DoneState(GOP_CardUnLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SystemErrorState : GOP_CardUnLoadProcState
    {
        public SystemErrorState(GOP_CardUnLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { 
            /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class WaferMisssingErrorStateAfterThreeLegDown : GOP_CardUnLoadProcState
    {
        public WaferMisssingErrorStateAfterThreeLegDown(GOP_CardUnLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            EventCodeEnum retVal;

            try
            {
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SelfRecoveryTransferToPreAlign();

                //if (retVal == EventCodeEnum.NONE)
                //{
                //    Module.StageSupervisor().WaferObject.SetStatusUnloaded();

                //    retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SelfRecoveryRetractARM();

                //    StateTransition(new SelfRecoveredState(Module));
                //}
                //else
                //{
                //    StateTransition(new SelfRecoveryFailedState(Module));
                //}
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.NOT_EXIST);
            }
        }
    }

    //public class SelfRecoveredState : CardUnLoadProcState
    //{
    //    public SelfRecoveredState(CardUnLoadProcModule module) : base(module) { }

    //    public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.SELF_RECOVERED;

    //    public override void Execute() { /*NoWORKS*/ }

    //    public override void SelfRecovery()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

}

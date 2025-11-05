using System;
using System.Threading.Tasks;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.WaferTransfer;
using NotifyEventModule;
using LogModule;
using LoaderController.GPController;
using SequenceRunner;
using System.Threading;
using ProberInterfaces.Event;
using ProberInterfaces.CardChange;

namespace WaferTransfer.GP_CardUnLoadProcStates
{


    public class IdleDDState : GP_CardUnLoadProcState
    {
        public IdleDDState(GP_CardUnLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new RunningDDState(Module));
        }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class RunningDDState : GP_CardUnLoadProcState
    {
        public RunningDDState(GP_CardUnLoadProcModule module) : base(module) { }

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
                EventCodeEnum retVal;
                bool waitForLoadingPos = true;
                Task.Run(() =>
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_Card_MoveLoadingPosition();
                    waitForLoadingPos = false;
                });

                //Stage가 Card를 Undock해서 LoadingPos에 갖다 놓기.
                LoggerManager.ActionLog(ModuleLogType.CARD_UNDOCK, StateLogType.START, $"ProbeCard ID:{Module.CardChangeModule().GetProbeCardID()}", this.Module.LoaderController().GetChuckIndex());
                var undockPCardTopPlate = new GP_UndockPCardTopPlate();
                var ret = Module.CardChangeModule().BehaviorRun(undockPCardTopPlate).Result;

                //IBehaviorResult commandResult = undockPCardTopPlate.Run().Result;
                if (ret != EventCodeEnum.NONE)
                {
                    LoggerManager.ActionLog(ModuleLogType.CARD_UNDOCK, StateLogType.ERROR, $"Reason : {ret}", this.Module.LoaderController().GetChuckIndex());
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Card Undocking Error in Cell{Module.LoaderController().GetChuckIndex()}, Result Value:{ret}");
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_UndockPCardTopPlate) RetVal:{ret}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                LoggerManager.ActionLog(ModuleLogType.CARD_UNDOCK, StateLogType.DONE, $"", this.Module.LoaderController().GetChuckIndex());
                while (waitForLoadingPos)
                {
                    System.Threading.Thread.Sleep(10);
                }


                LoggerManager.ActionLog(ModuleLogType.CARD_UNLOAD, StateLogType.START, $"ProbeCard ID:{Module.CardChangeModule().GetProbeCardID()}", this.Module.LoaderController().GetChuckIndex());
                retVal = Module.StageSupervisor().StageModuleState.LoaderDoorOpen();
                System.Threading.Thread.Sleep(1000);
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Shutter Door Open Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                var ccSysParam = (Module.CardChangeModule().CcSysParams_IParam) as ICardChangeSysParam;
                //=> Check wafer on chuck.
                if (ccSysParam.CardDockType.Value == EnumCardDockType.DIRECTDOCK)
                {
                    waitForLoadingPos = true;
                    Task<EventCodeEnum> pickTask = new Task<EventCodeEnum>(() =>
                    {
                        var result = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangePick();
                        waitForLoadingPos = false;
                        return result;
                    });
                    pickTask.Start();

                    if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.CC_STANDBY) == EventCodeEnum.NONE)
                    {
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.PICK_APPROACH);
                    }
                    else
                    {
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Transfer Timeout occurred on {Module.LoaderController().GetChuckIndex()}");
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }

                    var returnCard = new GP_HandoffForCardReturn();
                    //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                    ret = Module.CardChangeModule().BehaviorRun(returnCard).Result;
                    if (ret != EventCodeEnum.NONE)
                    {
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ABORT_REQ);
                        if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.ABORTED) != EventCodeEnum.NONE)
                        {
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Retracting failed on {Module.LoaderController().GetChuckIndex()}");
                            StateTransition(new SystemErrorDDState(Module));
                            return;
                        }
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_HandoffForCardReturn error) RetVal:{ret}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"GP_HandoffForCardReturn Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{ret}");
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.PICK_REQ);



                    // (Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.CARD_PICKED);
                    if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.CARD_PICKED, 15000) == EventCodeEnum.NONE)
                    {
                        var vacuumOff = new GP_PCardPodVacuumOff();//=> 베큠을 Z를 내리기전 꺼야한다.

                         var retVacOff = Module.CardChangeModule().BehaviorRun(vacuumOff).Result;
                        //Z축을 내리고 (PrepareCardHandoff);
                        var PrepareCardHandoff = new GP_NotExistCheckCardHandoff(); //check

                        //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                        var behret = Module.CardChangeModule().BehaviorRun(PrepareCardHandoff).Result;
                        if (behret != EventCodeEnum.NONE)
                        {
                            //if (behret == EventCodeEnum.GP_CardChange_MOVE_ERROR)
                            //{
                            //    //(Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ABORT_REQ);
                            //    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            //    LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_NotExistCheckCardHandoff error) RetVal:{behret}");
                            //    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetResonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"NotExistCheckCardHandoff Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}");
                            //    StateTransition(new SystemErrorState(Module));
                            //    return;
                            //}
                            //else
                            //{
                            //    (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.TRANSFER_CANCELED);
                            //    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            //    LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_NotExistCheckCardHandoff error) RetVal:{behret}");
                            //    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetResonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"NotExistCheckCardHandoff Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}");
                            //    StateTransition(new SystemErrorState(Module));
                            //    return;
                            //}
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ABORT_REQ);
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_NotExistCheckCardHandoff error) RetVal:{behret}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"NotExistCheckCardHandoff Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                        else
                        {
                            var dropPCardPod = new GP_DropPCardPod(); //check

                            //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                            behret = Module.CardChangeModule().BehaviorRun(dropPCardPod).Result;

                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.HANDOFF_CARD_TO_ARM);
                            if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.ARM_RETRACT_DONE) == EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"GP_CardUnLoadProcState(): ARM Retracted.");
                            }
                            else
                            {
                                Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (ARM RETRACT FAILED).");
                                (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"CTR_CardChangePick Error in Cell{Module.LoaderController().GetChuckIndex()}, ARM RETRACT FAILED");
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }
                            if (behret != EventCodeEnum.NONE)
                            {
                                Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_DropPCardPod error) RetVal:{behret}");
                                (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"GP_DropPCardPod Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}");
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }
                        }
                    }
                    else
                    {
                        var PrepareCardHandoff = new GP_NotExistCheckCardHandoff(); //check

                        //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                        var behret = Module.CardChangeModule().BehaviorRun(PrepareCardHandoff).Result;
                        if (behret != EventCodeEnum.NONE)
                        {
                            //if (behret == EventCodeEnum.GP_CardChange_MOVE_ERROR)
                            //{
                            //    (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ABORT_REQ);
                            //    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            //    LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_NotExistCheckCardHandoff error) RetVal:{behret}");
                            //    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetResonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"NotExistCheckCardHandoff Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}");
                            //    StateTransition(new SystemErrorState(Module));
                            //    return;
                            //}
                            //else
                            //{
                            //    (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.TRANSFER_CANCELED);
                            //    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            //    LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_NotExistCheckCardHandoff error) RetVal:{behret}");
                            //    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetResonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"NotExistCheckCardHandoff Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}");
                            //    StateTransition(new SystemErrorState(Module));
                            //    return;
                            //}
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ABORT_REQ);
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_NotExistCheckCardHandoff error) RetVal:{behret}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"NotExistCheckCardHandoff Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Card Pick Error in Loader");
                        LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (CTR_CardChangePick) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    pickTask.Wait();
                    retVal = pickTask.Result;
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Card Pick Error in Loader");
                        LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (CTR_CardChangePick) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }
                else
                {
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangePick();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Card Pick Error in Loader");
                        LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (CTR_CardChangePick) RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }

                Module.CardChangeModule().SetProbeCardID("");
                Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(CardUnloadedEvent).FullName));

                LoggerManager.ActionLog(ModuleLogType.CARD_UNLOAD, StateLogType.DONE, $"", this.Module.LoaderController().GetChuckIndex());
                var clearCardChange = new GP_ClearCardChange();
                ret = Module.CardChangeModule().BehaviorRun(clearCardChange).Result;

                //commandResult = clearCardChange.Run().Result;
                if (ret != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Card Clear Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_ClearCardChange) RetVal:{ret}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Module.StageSupervisor().StageModuleState.CardDoorClose();
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Shutter Door Close Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                StateTransition(new DoneState(Module));

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(CardUnloadedEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();

                //Module.EventManager().RaisingEvent(typeof(CardUnloadedEvent).FullName);
            }
            catch (Exception err)
            {
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

    public class DoneDDState : GP_CardUnLoadProcState
    {
        public DoneDDState(GP_CardUnLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SystemErrorDDState : GP_CardUnLoadProcState
    {
        public SystemErrorDDState(GP_CardUnLoadProcModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.CARD_UNLOAD, StateLogType.ERROR, $"", this.Module.LoaderController().GetChuckIndex());
            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetTransferAbort();
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }



}

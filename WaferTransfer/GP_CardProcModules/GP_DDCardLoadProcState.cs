using System;
using System.Collections.Generic;
using System.Linq;
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
using ProberInterfaces.CardChange;
using ProberInterfaces.Param;

namespace WaferTransfer.GP_CardLoadProcStates
{
    public class IdleDDState : GP_CardLoadProcState
    {
        public IdleDDState(GP_CardLoadProcModule module) : base(module) { }

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

    public class RunningDDState : GP_CardLoadProcState
    {
        public RunningDDState(GP_CardLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.CARD_LOAD, StateLogType.START, $"", this.Module.LoaderController().GetChuckIndex());

                string probeCardID = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
                ICardChangeSysParam cardChangeSysParam = Module.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                if (cardChangeSysParam.ProberCardList == null)
                {
                    cardChangeSysParam.ProberCardList = new List<ProberCardListParameter>();
                }

                ProberCardListParameter proberCard = cardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);
                if (proberCard == null)
                {
                    proberCard = (Module.LoaderController() as GP_LoaderController).DownloadProbeCardInfo(probeCardID);
                    if (proberCard == null)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GP_DDCardLoadProcState Error] Probe card info({probeCardID}) is not exist in cell and loader");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"No matching card ID. in Cell{Module.LoaderController().GetChuckIndex()}");
                        StateTransition(new SystemErrorDDState(Module));
                        Module.WaferTransferModule.TransferBrake = true;
                        return;
                    }
                }

                var readyToLoading = new GP_DD_ReadyToLoading();
                //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                var ret = Module.CardChangeModule().BehaviorRun(readyToLoading).Result;
                if (ret != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_DD_ReadyToLoading error) RetVal:{ret}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"GP_DD_ReadyToLoading Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{ret}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }

                bool waitForLoadingPos = true;
                Task.Run(() =>
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_Card_MoveLoadingPosition();
                    waitForLoadingPos = false;
                });
                

                while (waitForLoadingPos)
                {
                    System.Threading.Thread.Sleep(10);
                }
                var retVal = Module.StageSupervisor().StageModuleState.LoaderDoorOpen();
                System.Threading.Thread.Sleep(1000);
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Shutter Door Open Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }

                TransferObject loadedObject = null;

                waitForLoadingPos = true;
                Task.Run(() =>
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangePut(out loadedObject);
                    waitForLoadingPos = false;
                });

                #region // Handoff seq.
                //Module.LoaderController().WaitForHandle
                //(Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle
                if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.CC_STANDBY) == EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.LOCK_REQUEST);
                }
                else
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Transfer Timeout occurred on {Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }

                if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.LUC_LOCKED) == EventCodeEnum.NONE)
                {
                   (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ALIGNMENT);
                }
                else
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"LUC Lock Failed on {Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }
                // Do Alignment
                double cenDiffX, cenDiffY, cenDiffT, cenDiffZ = 0;
                var result = Module.GPCardAligner().AlignCard(out cenDiffX, out cenDiffY, out cenDiffT, out cenDiffZ, proberCard);  // Do bloody great jobs
                if (result == false)
                {
                   (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ABORT_REQ);
                    if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.ABORTED) != EventCodeEnum.NONE)
                    {
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"AlignCard failed on {Module.LoaderController().GetChuckIndex()}");
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }
                    // Drop the POD
                    var dropPCardPod = new GP_DropPCardPod();
                    //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                    var behret = Module.CardChangeModule().BehaviorRun(dropPCardPod).Result;
                    if (behret != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GP_CardLoadProcState Error] (AlignCard() Failed) RetVal:{behret}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"GP_ReadyToGetCard Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}");
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }

                    //(Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.POD_CLEARED);
                    if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.ABORTED) != EventCodeEnum.NONE)
                    {
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"POD Clearing Failed on {Module.LoaderController().GetChuckIndex()}");
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }

                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Card Alignment Failed on {Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }
                // Move to aligned position with CardTransferPos
                var readyToGetCard = new GP_PrepareCardHandoff();
                //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                ret = Module.CardChangeModule().BehaviorRun(readyToGetCard).Result;
                if (ret != EventCodeEnum.NONE)
                {
                    // PrePareCardHandoff 에서도 에러가 발생했을 경우 예외처리 추가
                    // 똑같이 CardPod 내리고 Abort를 날려줘야 PLC쪽에서 시퀀스가 정리됨.
                    // CCZCleared를 해주지 않아도 됨. HandOff전이라 Z를 올린적이 없음. (Fiducial Align 바로 직후 임)
                    var dropPCardPod = new GP_DropPCardPod();
                    var behret = Module.CardChangeModule().BehaviorRun(dropPCardPod).Result;
                    if (behret != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_PrepareCardHandoff error) RetVal:{behret}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",
                            $"Prepare HandOff(DropPod seq) Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}",
                            "CardHandOffRecovery",
                            Module.LoaderController().GetChuckIndex());
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }

                    var intRetL = Module.IOManager().IOServ.MonitorForIO(Module.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, false, 1000, 15000);
                    var intRetR = Module.IOManager().IOServ.MonitorForIO(Module.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, false, 1000, 15000);

                    if (intRetL == 0 & intRetR == 0)
                    {
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ABORT_REQ);
                        if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.ABORTED) != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_PrepareCardHandoff error(CardPod Down State), ABORT_REQ Fail) RetVal:{ret}");
                        }
                        else
                        {
                            LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_PrepareCardHandoff error(CardPod Down State)) RetVal:{ret}");
                        }
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Prepare HandOff Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{ret}");
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }
                    else
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_PrepareCardHandoff error(CardPod Up State)) RetVal:{ret}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", 
                            $"Prepare HandOff Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{ret}",
                            "CardHandOffRecovery",
                            Module.LoaderController().GetChuckIndex());
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }
                }
                var handOffMove = new GP_Handoff();
                //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                ret = Module.CardChangeModule().BehaviorRun(handOffMove).Result;
                if (ret != EventCodeEnum.NONE)
                {
                    // Handoff 중에 CardPod Vac이 잡히지 않은 경우 예외처리 추가
                    // Pod Vac Off, CCZCleared, Pod Down 모두 HandOff 내부에서 해주긴 하지만 혹시나 예외적으로 안되었을 경우 다시 해주기 위해 추가.
                    IORet ioret = IORet.UNKNOWN;
                    ioret = Module.IOManager().IOServ.WriteBit(Module.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                    Module.StageSupervisor().StageModuleState.CCZCLEARED();
                    
                    var dropPCardPod = new GP_DropPCardPod();
                    var behret = Module.CardChangeModule().BehaviorRun(dropPCardPod).Result;
                    if (behret != EventCodeEnum.NONE)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_PrepareCardHandoff error) RetVal:{behret}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError(
                            $"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", 
                            $"HandOff(DropPod seq) Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}",
                            "CardHandOffRecovery",
                            Module.LoaderController().GetChuckIndex());
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }

                    var intRetL = Module.IOManager().IOServ.MonitorForIO(Module.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, false, 1000, 15000);
                    var intRetR = Module.IOManager().IOServ.MonitorForIO(Module.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, false, 1000, 15000);

                    if(intRetL == 0 & intRetR == 0)
                    {
                        // Card Pod Down 상태를 보고 PLC 쪽에 Abort Req를 날린다. PLC쪽에서는 Abort를 받은 경우 Card Arm을 뒤로 뺀다.
                        // (Card Pod 이 내려가있는 경우에만 Card Arm을 빼야 함.)
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ABORT_REQ);
                        if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.ABORTED) != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_Handoff error(CardPod Down State), ABORT_REQ Fail) RetVal:{ret}");
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError(
                                $"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",
                                $"HandOff failed on {Module.LoaderController().GetChuckIndex()}",
                                "CardHandOffRecovery",
                                Module.LoaderController().GetChuckIndex());
                        }
                        else
                        {
                            LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_Handoff error(CardPod Down State)) RetVal:{ret}");
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError(
                                $"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",
                                $"HandOff failed on {Module.LoaderController().GetChuckIndex()}");
                        }
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }
                    else
                    {
                        // 이쪽으로 빠지게 되는 경우 ISSD-4940 이슈가 동일하게 발생할 가능성이 있으나, 위에서 CCZCleared, CardPod 다운 동작을 했으므로 문제 없을 것으로 판단됨.
                        Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_Handoff error(CardPod Up State)) RetVal:{ret}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError(
                            $"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",
                            $"HandOff failed in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{ret}",
                            "CardHandOffRecovery",
                            Module.LoaderController().GetChuckIndex());
                        StateTransition(new SystemErrorDDState(Module));
                        return;
                    }
                }
                // Release request
               (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.RELEASE_REQ);
                if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.RELEASED) != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"LUC Lock Failed on {Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }
                // Z Escape from Handoff position
                var dropChuckSafety = new GP_DropChuckSafety();
                //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                ret = Module.CardChangeModule().BehaviorRun(dropChuckSafety).Result;
                if (ret != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_PrepareCardHandoff error) RetVal:{ret}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"GP_ReadyToGetCard Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{ret}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }

               (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.TRANSFFERED);
                if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.LUC_ESCAPED) != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"LUC Lock Failed on {Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }
                //transfTask.Wait();
                while (waitForLoadingPos)
                {
                    if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.LUC_ESCAPED) == EventCodeEnum.NONE)
                    {
                        waitForLoadingPos = false;
                    }
                    System.Threading.Thread.Sleep(100);
                }
                #endregion



                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Card Put Error in Loader");
                    LoggerManager.Debug($"[GP_CardLoadProcState Error] (CTR_CardChangePut error) RetVal:{retVal}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }

                retVal = Module.StageSupervisor().StageModuleState.CardDoorClose();
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Shutter Door Close Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }

                System.Threading.Thread.Sleep(1000);

                probeCardID = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardID();

                Module.CardChangeModule().SetProbeCardID(probeCardID);
                Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                LoggerManager.ActionLog(ModuleLogType.CARD_LOAD, StateLogType.DONE, $"ProbeCard ID:{probeCardID}", this.Module.LoaderController().GetChuckIndex());
                if (loadedObject == null)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Card Object is Null Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    LoggerManager.Debug($"[GP_CardLoadProcState Error] (Load Card Object Null) ");
                    StateTransition(new SystemErrorDDState(Module));
                    return;
                }
                else
                {
                    //this.Module.GetParam_ProbeCard().SetProbeCardID(loadedObject.ProbeCardID.Value);

                    if (loadedObject.CardSkip == CardSkipEnum.NONE)
                    {
                        LoggerManager.ActionLog(ModuleLogType.CARD_DOCK, StateLogType.START, $"ProbeCard ID:{probeCardID}", this.Module.LoaderController().GetChuckIndex());
                        var dockPCardTopPlate = new GP_DockPCardTopPlate();

                        var commandResult = Module.CardChangeModule().BehaviorRun(dockPCardTopPlate);
                        if (commandResult.Result != EventCodeEnum.NONE)
                        {
                            LoggerManager.ActionLog(ModuleLogType.CARD_DOCK, StateLogType.ERROR, $"Reason : {commandResult.Result}", this.Module.LoaderController().GetChuckIndex());
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}",$"Card Docking Error in Cell{Module.LoaderController().GetChuckIndex()}, Result Value:{commandResult.Result}");
                            LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_DockPCardTopPlate) RetVal:{commandResult.Result}");
                            StateTransition(new SystemErrorDDState(Module));
                            return;
                        }
                        else
                        {
                            LoggerManager.ActionLog(ModuleLogType.CARD_DOCK, StateLogType.DONE, $"ProbeCard ID:{probeCardID}", this.Module.LoaderController().GetChuckIndex());
                        }
                    }
                }
        
                StateTransition(new DoneDDState(Module));

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

                StateTransition(new SystemErrorDDState(Module));
            }
        }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class DoneDDState : GP_CardLoadProcState
    {
        public DoneDDState(GP_CardLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SystemErrorDDState : GP_CardLoadProcState
    {
        public SystemErrorDDState(GP_CardLoadProcModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.CARD_LOAD, StateLogType.ERROR, $"", this.Module.LoaderController().GetChuckIndex());
            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetTransferAbort();
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            //No WOKRS.
        }
    }

}

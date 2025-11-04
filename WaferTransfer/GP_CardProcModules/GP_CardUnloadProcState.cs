using System;
using System.Threading.Tasks;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.WaferTransfer;
using LoaderControllerBase;
using LogModule;
using LoaderController.GPController;
using SequenceRunner;
using ProberInterfaces.CardChange;

namespace WaferTransfer.GP_CardUnLoadProcStates
{
    public abstract class GP_CardUnLoadProcState
    {
        public GP_CardUnLoadProcModule Module { get; set; }

        public GP_CardUnLoadProcState(GP_CardUnLoadProcModule module)
        {
            this.Module = module;
        }

        public void StateTransition(GP_CardUnLoadProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }

        public abstract WaferTransferProcStateEnum State { get; }

        public abstract void Execute();

        public abstract void SelfRecovery();
    }

    public class IdleState : GP_CardUnLoadProcState
    {
        public IdleState(GP_CardUnLoadProcModule module) : base(module) { }

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

    public class RunningState : GP_CardUnLoadProcState
    {
        public RunningState(GP_CardUnLoadProcModule module) : base(module) { }

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
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Card Undocking Error in Cell{Module.LoaderController().GetChuckIndex()}, Result Value:{ret}");
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

                var vacuumOff = new GP_PCardPodVacuumOff(); //=> 언도킹 Sequence에서 베큠 끄는 부분을 빼고 언로딩 시작할 때(로더가 가져가기 전) 베큠 끄게 변경.
                var retVacOff = Module.CardChangeModule().BehaviorRun(vacuumOff).Result;
                if (retVacOff != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"CardPod Vacuum Off Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Module.StageSupervisor().StageModuleState.CardDoorOpen();
                System.Threading.Thread.Sleep(1000);
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Shutter Door Open Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                var ccSysParam = (Module.CardChangeModule().CcSysParams_IParam) as ICardChangeSysParam;
                //=> Check wafer on chuck.
                if(ccSysParam.CardDockType.Value == EnumCardDockType.DIRECTDOCK)
                {
                    waitForLoadingPos = true;
                    Task<EventCodeEnum> pickTask = new Task<EventCodeEnum>(() =>
                    {
                        var result = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangePick();
                        waitForLoadingPos = false;
                        return result;
                    });
                    pickTask.Start();
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.CARD_PICKED);
                    if ((Module.LoaderController() as GP_LoaderController).GPLoaderService.WaitForHandle((short)EnumCCWaitHandle.CARD_PICKED) == EventCodeEnum.NONE)
                    {
                        var dropPCardPod = new GP_DropPCardPod();
                        //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                        var behret = Module.CardChangeModule().BehaviorRun(dropPCardPod).Result;
                        if (behret != EventCodeEnum.NONE)
                        {
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.WriteWaitHandle((short)EnumCCWaitHandle.ABORT_REQ);
                            Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (CTR_CardChangePick error) RetVal:{behret}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}",$"CTR_CardChangePick Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{behret}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                        else
                        {
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
                        }
                    }
                    else
                    {
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
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Card Clear Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    LoggerManager.Debug($"[GP_CardUnLoadProcState Error] (GP_ClearCardChange) RetVal:{ret}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Module.StageSupervisor().StageModuleState.CardDoorClose();
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Shutter Door Close Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                StateTransition(new DoneState(Module));

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

    public class DoneState : GP_CardUnLoadProcState
    {
        public DoneState(GP_CardUnLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SystemErrorState : GP_CardUnLoadProcState
    {
        public SystemErrorState(GP_CardUnLoadProcModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.CARD_UNLOAD, StateLogType.ERROR, $"", this.Module.LoaderController().GetChuckIndex());
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class WaferMisssingErrorStateAfterThreeLegDown : GP_CardUnLoadProcState
    {
        public WaferMisssingErrorStateAfterThreeLegDown(GP_CardUnLoadProcModule module) : base(module) { }

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

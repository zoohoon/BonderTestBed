using System;
using System.Threading.Tasks;
using ProberInterfaces.WaferTransfer;
using ProberInterfaces;
using ProberErrorCode;
using LogModule;
using LoaderController.GPController;
using SequenceRunner;
using ProberInterfaces.Enum;

namespace WaferTransfer.GP_CardLoadProcStates
{
    public abstract class GP_CardLoadProcState
    {
        public GP_CardLoadProcModule Module { get; set; }

        public GP_CardLoadProcState(GP_CardLoadProcModule module)
        {
            this.Module = module;
        }

        public void StateTransition(GP_CardLoadProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }

        public abstract WaferTransferProcStateEnum State { get; }

        public abstract void Execute();

        public abstract void SelfRecovery();

    }

    public class IdleState : GP_CardLoadProcState
    {
        public IdleState(GP_CardLoadProcModule module) : base(module) { }

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

    public class RunningState : GP_CardLoadProcState
    {
        public RunningState(GP_CardLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.CARD_LOAD, StateLogType.START, $"", this.Module.LoaderController().GetChuckIndex());
                bool waitForLoadingPos = true;
                Task.Run(() =>
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_Card_MoveLoadingPosition();
                    waitForLoadingPos = false;
                });
                var readyToGetCard = new GP_ReadyToGetCard();
                //IBehaviorResult commandResult = readyToGetCard.Run().Result;
                var ret = Module.CardChangeModule().BehaviorRun(readyToGetCard).Result;
                if (ret != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_ReadyToGetCard error) RetVal:{ret}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"GP_ReadyToGetCard Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{ret}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                while (waitForLoadingPos)
                {
                    System.Threading.Thread.Sleep(10);
                }
                var retVal = Module.StageSupervisor().StageModuleState.CardDoorOpen();  
                System.Threading.Thread.Sleep(1000);
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Shutter Door Open Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                TransferObject loadedObject;
                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_CardChangePut(out loadedObject);
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Card Put Error in Loader");
                    LoggerManager.Debug($"[GP_CardLoadProcState Error] (CTR_CardChangePut error) RetVal:{retVal}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                string probeCardID = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardID();

                Module.CardChangeModule().SetProbeCardID(probeCardID);

                var cardPodVacOn = new GP_PCardPodVacuumOn();
                var retCardPodVacOn = Module.CardChangeModule().BehaviorRun(cardPodVacOn).Result;
                if (retCardPodVacOn != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_PCardPodVacuumOn error) RetVal:{ret}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"GP_PCardPodVacuumOn Error in Cell{Module.LoaderController().GetChuckIndex()}, Return Value:{ret}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Module.StageSupervisor().StageModuleState.CardDoorClose();
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Shutter Door Close Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                
                Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                Module.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                LoggerManager.ActionLog(ModuleLogType.CARD_LOAD, StateLogType.DONE, $"ProbeCard ID:{probeCardID}", this.Module.LoaderController().GetChuckIndex());
                if (loadedObject == null)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Card Object is Null Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    LoggerManager.Debug($"[GP_CardLoadProcState Error] (Load Card Object Null) ");
                    StateTransition(new SystemErrorState(Module));
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
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Card Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Card Docking Error in Cell{Module.LoaderController().GetChuckIndex()}, Result Value:{commandResult.Result}");
                            LoggerManager.Debug($"[GP_CardLoadProcState Error] (GP_DockPCardTopPlate) RetVal:{commandResult.Result}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                        else
                        {
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_NotifyCardDocking();
                            LoggerManager.ActionLog(ModuleLogType.CARD_DOCK, StateLogType.DONE, $"ProbeCard ID:{probeCardID}", this.Module.LoaderController().GetChuckIndex());
                        }
                    }
                }

                StateTransition(new DoneState(Module));

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

    public class DoneState : GP_CardLoadProcState
    {
        public DoneState(GP_CardLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SystemErrorState : GP_CardLoadProcState
    {
        public SystemErrorState(GP_CardLoadProcModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.CARD_LOAD, StateLogType.ERROR, $"", this.Module.LoaderController().GetChuckIndex());
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            //No WOKRS.
        }
    }

}

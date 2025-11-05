using System;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.WaferTransfer;
using ProberInterfaces.State;
using LogModule;
using LoaderController.GPController;
using System.Threading;
using NotifyEventModule;
using ProberInterfaces.Event;

namespace WaferTransfer.WaferTransferStates
{
    public abstract class WaferTransferState : IInnerState
    {
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract ModuleStateEnum GetModuleState();
        public abstract ModuleStateEnum GetState { get; }

        public abstract EventCodeEnum End();

        public abstract EventCodeEnum Abort();

        public abstract EventCodeEnum ClearState();

        public abstract EventCodeEnum Resume();

    }

    public abstract class WaferTransferStateBase : WaferTransferState
    {
        public WaferTransferModule Module { get; set; }

        public WaferTransferStateBase(WaferTransferModule module)
        {
            this.Module = module;
        }

        public virtual bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        public virtual void SelfRecovery()
        {
            //Invalid call
        }

        public virtual void ClearErrorState()
        {
            //Invalid call
        }

        protected bool CanTransitionToLoading(bool isFreeRun, bool isCheckBlowOption)
        {
            try
            {
                var chuckWaferStatus = Module.StageSupervisor().WaferObject.GetStatus();
                //var chuckWaferState = Module.StageSupervisor.WaferObject.GetState();

                bool isPassWaferStatus = chuckWaferStatus == EnumSubsStatus.NOT_EXIST;
                var moduleStopFlag = Module.LotOPModule().ModuleStopFlag;
                bool isPassWaferState = true;

                bool isPassBlowState = isCheckBlowOption == false || Module.AirBlowChuckCleaningModule().ModuleState.GetState() == ModuleStateEnum.DONE;
                bool isRunningState = false;
                if (isFreeRun)
                {
                    isRunningState = Module.SequenceEngineManager().isMovingState();
                }
                else
                {
                    isRunningState = Module.SequenceEngineManager().GetRunState(isWaferTransfer:true);
                }

                return
                    //(!moduleStopFlag)&&
                    isPassWaferStatus &&
                    isPassWaferState &&
                    isPassBlowState &&
                    isRunningState;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected bool CanTransitionToUnloading(bool isFreeRun, bool isCheckBlowOption)
        {
            try
            {
                var chuckWaferStatus = Module.StageSupervisor().WaferObject.GetStatus();
                var chuckWaferState = Module.StageSupervisor().WaferObject.GetState();
                var moduleStopFlag = Module.LotOPModule().ModuleStopFlag;
                bool isPassWaferStatus = chuckWaferStatus == EnumSubsStatus.EXIST;

                bool isPassWaferState = isFreeRun == true ||
                    (chuckWaferState == EnumWaferState.PROCESSED 
                     || chuckWaferState == EnumWaferState.SKIPPED
                     || chuckWaferState == EnumWaferState.READY 
                     || chuckWaferState == EnumWaferState.SOAKINGSUSPEND
                     || chuckWaferState == EnumWaferState.SOAKINGDONE);

                bool isPassBlowState = isCheckBlowOption == false || Module.AirBlowWaferCleaningModule().ModuleState.GetState() == ModuleStateEnum.DONE;
                isPassBlowState = true;

                bool isRunningState = false;
                if (isFreeRun)
                {
                    isRunningState = Module.SequenceEngineManager().isMovingState();
                }
                else
                {
                    isRunningState = Module.SequenceEngineManager().GetRunState(isWaferTransfer: true);
                }
                return
                    //(!moduleStopFlag)&&
                    isPassWaferStatus &&
                    isPassWaferState &&
                    isPassBlowState &&
                    isRunningState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected bool CanTransitionToRecoveryUnloading()
        {
            bool ret = false;
            try
            {
                if (Module.NeedToRecovery == true)
                    ret = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;

        }
        public override EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        //public virtual EventCodeEnum ClearState()
        //{
        //    return EventCodeEnum.NONE;
        //}

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    retval = Module.InnerStateTransition(new IDLE(Module));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public override EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }

        protected EventCodeEnum FreeRunForIdle()
        {
            EventCodeEnum retVal;
            try
            {
                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;

                #region => Process IChuckLoadCommand
                conditionFuc = () =>
                {
                    return CanTransitionToLoading(true, false);
                };

                doAction = () =>
                {
                    Module.ActivateProcModule(WaferTransferTypeEnum.Loading);

                    Module.InnerStateTransition(new PROCESSING(Module));
                };
                abortAction = () => { LoggerManager.Debug($"[IChuckLoadCommand] : Aborted."); };

                Module.CommandManager().ProcessIfRequested<IChuckLoadCommand>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);
                #endregion

                #region => Process ICardLoadCommand
                conditionFuc = () =>
                {
                    return Module.SequenceEngineManager().GetRunState(false, isWaferTransfer: true);
                };

                doAction = () =>
                {
                    Module.ActivateProcModule(WaferTransferTypeEnum.CardLoading);

                    Module.InnerStateTransition(new PROCESSING(Module));
                };
                abortAction = () => { LoggerManager.Debug($"[ICardLoadCommand] : Aborted."); };

                Module.CommandManager().ProcessIfRequested<ICardLoadCommand>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);
                #endregion

                #region => Process IChuckUnloadCommand
                conditionFuc = () =>
                {
                    return CanTransitionToUnloading(true, false);
                };

                doAction = () =>
                {
                    Module.ActivateProcModule(WaferTransferTypeEnum.Unloading);

                    Module.InnerStateTransition(new PROCESSING(Module));
                };
                abortAction = () => { LoggerManager.Debug($"[IChuckUnloadCommand] : Aborted."); };

                Module.CommandManager().ProcessIfRequested<IChuckUnloadCommand>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);
                #endregion



                #region => Process IChuckUnloadCommand
                conditionFuc = () =>
                {
                    return Module.SequenceEngineManager().GetRunState(false, isWaferTransfer: true);
                };

                doAction = () =>
                {
                    Module.ActivateProcModule(WaferTransferTypeEnum.CardUnLoding);

                    Module.InnerStateTransition(new PROCESSING(Module));
                };
                abortAction = () => { LoggerManager.Debug($"[ICardUnloadCommand] : Aborted."); };

                Module.CommandManager().ProcessIfRequested<ICardUnloadCommand>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);
                #endregion

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected EventCodeEnum FreeRunForWaitForChuckBlow()
        {
            EventCodeEnum retVal;
            try
            {

                if (CanTransitionToLoading(true, false))
                {
                    Module.InnerStateTransition(new PROCESSING(Module));
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected EventCodeEnum FreeRunForWaitForWaferBlow()
        {
            EventCodeEnum retVal;
            try
            {

                if (CanTransitionToUnloading(true, false))
                {
                    Module.InnerStateTransition(new PROCESSING(Module));
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected EventCodeEnum FreeRunForDone()
        {
            EventCodeEnum retVal;
            try
            {

                //clear process module
                if (Module.ProcModule != null)
                {
                    if (Module.LoaderControllerExt != null)
                    {
                        Module.LoaderControllerExt.LoaderService.WTR_NotifyWaferTransferResult(true);
                    }
                    Module.ProcModule = null;
                }

                if (CanTransitionToIdle(true))
                {
                    Module.InnerStateTransition(new IDLE(Module));
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected bool CanTransitionToIdle(bool isFreeRun)
        {
            bool isPassStageMoveState =
                Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.MOVETOLOADPOS;

            bool isInjectedCommand =
                Module.CommandRecvSlot.IsRequested<IChuckLoadCommand>() ||
                Module.CommandRecvSlot.IsRequested<IChuckUnloadCommand>();

            return isPassStageMoveState || isInjectedCommand;
        }


    }

    public class IDLE : WaferTransferStateBase
    {
        public IDLE(WaferTransferModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.IDLE;


        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {
                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;
                bool ret;

                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    #region => Process IChuckLoadCommand
                    if (Module.EnvModule().IsConditionSatisfied() == EventCodeEnum.NONE)
                    {
                        conditionFuc = () =>
                        {
                            return CanTransitionToLoading(false, false);
                        };

                        doAction = () =>
                        {
                            bool isInjected = Module.CommandManager().SetCommand<IAirBlowChuckCleaningCommand>(Module);

                            if (isInjected)
                            {
                                Module.ActivateProcModule(WaferTransferTypeEnum.Loading);

                                Module.InnerStateTransition(new WAIT_FOR_CHUCK_BLOW(Module));
                            }
                            else
                            {
                                LoggerManager.Debug($"[IAirBlowChuckCleaningCommand] : Inject failed.");
                            }
                        };
                        abortAction = () => { LoggerManager.Debug($"[IChuckLoadCommand] : Aborted."); };

                        if (CanTransitionToLoading(false, false))
                        {
                            ret = Module.CommandManager().ProcessIfRequested<IChuckLoadCommand>(
                                Module,
                                conditionFuc,
                                doAction,
                                abortAction);
                        }
                    }
                    #endregion

                    #region => Process IChuckUnloadCommand
                    conditionFuc = () =>
                    {
                        return CanTransitionToUnloading(false, false);
                    };

                    doAction = () =>
                    {
                        bool isInjected = Module.CommandManager().SetCommand<IAirBlowWaferCleaningCommand>(Module);
                        if (isInjected)
                        {
                            Module.ActivateProcModule(WaferTransferTypeEnum.Unloading);

                            Module.InnerStateTransition(new WAIT_FOR_WAFER_BLOW(Module));
                        }
                        else
                        {
                            LoggerManager.Debug($"[IAirBlowWaferCleaningCommand] : Inject failed.");
                        }
                    };
                    abortAction = () => { LoggerManager.Debug($"[IChuckUnloadCommand] : Aborted."); };

                    if (CanTransitionToUnloading(false, false))
                    {
                        ret = Module.CommandManager().ProcessIfRequested<IChuckUnloadCommand>(
                            Module,
                            conditionFuc,
                            doAction,
                            abortAction);
                        //if (ret == true)
                        //{

                        //}
                    }

                    #endregion

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = FreeRunForIdle();
                }

                if(Module.StopAfterTransferDone == true)
                {
                    // ZDown 후 축 Disable
                    Module.MotionManager().StageEMGZDown();
                    Module.StopAfterTransferDone = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected =
                   token is IChuckLoadCommand ||
                   token is IChuckUnloadCommand ||
                   token is ICardLoadCommand ||
                   token is ICardUnloadCommand ||
                   token is IAbortcardChange;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            if (Module.CommandRecvSlot.IsRequested<IChuckLoadCommand>())
            {
            }
            else
            {
                Module.PreInnerState = this;
                Module.InnerStateTransition(new PAUSE(Module));
            }
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
    }

    public class WAIT_FOR_CHUCK_BLOW : WaferTransferStateBase
    {
        bool AirBlowChuckCleaning_Done = false;
        public WAIT_FOR_CHUCK_BLOW(WaferTransferModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.SUSPENDED;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {

                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    // AirBlowChuckCleaningModule 이 Done이 되었지만 CanTransitionToLoading 내부에서 Module.SequenceEngineManager().GetRunState()가 false면
                    // 다음 tick에서 Module.AirBlowChuckCleaningModule() Idle로 State가 변경되어 있어 조건을 만족하지 못해 계속 WAIT_FOR_CHUCK_BLOW State유지로 Wafer load하지 못함.
                    // 따라서 AirBlowChuckCleaningModule이 Done되었다면 다음 Tick에서는 AirBlowChuckCleaningModule의 Done여부를 체크하지 않도록 한다.
                    bool CheckBlowOption = true; 
                    if (AirBlowChuckCleaning_Done)
                        CheckBlowOption = false;

                    bool CanTransitionToLoadingFlag = CanTransitionToLoading(false, CheckBlowOption);
                    if(false == CanTransitionToLoadingFlag)
                    {
                        if (Module.AirBlowChuckCleaningModule().ModuleState.GetState() == ModuleStateEnum.DONE)
                            AirBlowChuckCleaning_Done = true;
                    }

                    if (CanTransitionToLoadingFlag)
                    {
                        Module.InnerStateTransition(new PROCESSING(Module));
                    }
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = FreeRunForWaitForChuckBlow();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            Module.PreInnerState = this;
            Module.InnerStateTransition(new PAUSE(Module));
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }
    }

    public class WAIT_FOR_WAFER_BLOW : WaferTransferStateBase
    {
        public WAIT_FOR_WAFER_BLOW(WaferTransferModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.SUSPENDED;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {

                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    if (CanTransitionToUnloading(false, true))
                    {
                        Module.InnerStateTransition(new PROCESSING(Module));
                    }
                }
                else
                {
                    retVal = FreeRunForWaitForWaferBlow();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            Module.PreInnerState = this;
            Module.InnerStateTransition(new PAUSE(Module));
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }
    }

    public class PROCESSING : WaferTransferStateBase
    {
        public PROCESSING(WaferTransferModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.RUNNING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new DONE(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }
                while (true)
                {
                    Module.ProcModule.Execute();

                    var procState = Module.ProcModule.State;

                    if (Module.GetParam_Wafer().WaferStatus == EnumSubsStatus.EXIST &&
                       Module.GetParam_Wafer().GetWaferType() == EnumWaferType.TCW &&
                       Module.GetParam_Wafer().GetState()!=EnumWaferState.PROCESSED&&
                       Module.ProcModule.TransferType==WaferTransferTypeEnum.Loading)
                    {
                        Module.InnerStateTransition(new TCW_RUNNING(Module));
                        break;
                    }

                    if (procState == WaferTransferProcStateEnum.DONE)
                    {
                        if (Module.ProcModule != null)
                        {
                            if (Module.ProcModule is WaferLoadProcModule || Module.ProcModule is GP_WaferLoadProcModule)
                            {
                                Module.LotOPModule().ModuleStopFlag = false;
                            }

                            if (Module.LoaderControllerExt != null)
                            {
                                Module.LoaderControllerExt.LoaderService.WTR_NotifyWaferTransferResult(true);
                            }
                            //else if (Module.ProcModule.TransferType == WaferTransferTypeEnum.Loading || Module.ProcModule.TransferType == WaferTransferTypeEnum.Unloading)
                            //{
                            //   // (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(true);
                            //}
                            else if (Module.ProcModule.TransferType == WaferTransferTypeEnum.CardLoading || Module.ProcModule.TransferType == WaferTransferTypeEnum.CardUnLoding)
                            {
                                if (Module.ProcModule is GP_CardLoadProcModule || Module.ProcModule is GP_CardUnLoadProcModule)
                                {
                                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_NotifyCardTransferResult(true);
                                }
                            }

                            Module.ProcModule = null;
                        }
                        Module.InnerStateTransition(new DONE(Module));
                        break;
                    }

                    if (procState == WaferTransferProcStateEnum.ERROR)
                    {
                        if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                        {
                            Module.InnerStateTransition(new DONE(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                            return EventCodeEnum.NONE;
                        }
                        if (Module.LoaderControllerExt != null)
                        {
                            Module.LoaderControllerExt.LoaderService.WTR_NotifyWaferTransferResult(false);
                        }
                        else if (Module.ProcModule.TransferType == WaferTransferTypeEnum.Loading || Module.ProcModule.TransferType == WaferTransferTypeEnum.Unloading)
                        {
                            // (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);

                        }
                        else if (Module.ProcModule.TransferType == WaferTransferTypeEnum.CardLoading || Module.ProcModule.TransferType == WaferTransferTypeEnum.CardUnLoding)
                        {
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.CTR_NotifyCardTransferResult(false);
                            Module.ProcModule = null;
                            Module.InnerStateTransition(new DONE(Module));
                            return EventCodeEnum.NONE;
                        }

                        Module.InnerStateTransition(new ERROR(Module));
                        break;
                    }

                    if (procState == WaferTransferProcStateEnum.PENDING)
                    {
                        if (Module.ProcModule.TransferType == WaferTransferTypeEnum.Loading || Module.ProcModule.TransferType == WaferTransferTypeEnum.Unloading)
                        {
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(true);
                            Module.ProcModule = null;
                        }
                        Module.InnerStateTransition(new PENDING(Module));
                        break;
                    }

                   
                    //Module._delays.DelayFor(1);
                    System.Threading.Thread.Sleep(1);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = token is IAbortcardChange;
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            //Processing 상태에서는 로더와의 동기를 위해서 바로 Paused 상태로 만들면 안된다고함. 
            //단, Pause Action이 다시 안불리기 때문에 다음 State에서 Pause Action을 해줘야함.
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

    }

    public class ABORT : WaferTransferStateBase
    {
        public ABORT(WaferTransferModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.ABORT;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {
                retVal = Module.InnerStateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.PreInnerState = this;
                retVal = Module.InnerStateTransition(new PAUSE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }
    }

    public class DONE : WaferTransferStateBase
    {
        public DONE(WaferTransferModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.DONE;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                //clear process module



                ModuleStateEnum LotOpModuleState = Module.LotOPModule().InnerState.GetModuleState();
                if (LotOpModuleState == ModuleStateEnum.RUNNING)
                {

                    //if (CanTransitionToIdle(false))
                    //{
                    Module.InnerStateTransition(new IDLE(Module));
                    //}

                    retVal = EventCodeEnum.NONE;
                }
                else if (LotOpModuleState == ModuleStateEnum.PAUSING)
                {
                    retVal = FreeRunForDone();
                    retVal = Module.InnerStateTransition(new PAUSE(Module));
                }
                else if (LotOpModuleState == ModuleStateEnum.ABORT || LotOpModuleState == ModuleStateEnum.IDLE || LotOpModuleState == ModuleStateEnum.PAUSED)
                {
                    //if (CanTransitionToIdle(false))
                    //{
                    retVal = Module.InnerStateTransition(new IDLE(Module));
                    //}
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected =
                    token is IChuckLoadCommand ||
                    token is IChuckUnloadCommand ||
                    token is ICardLoadCommand ||
                    token is ICardUnloadCommand;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.PreInnerState = this;
                retVal = Module.InnerStateTransition(new PAUSE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }
    }

    public class TCW_RUNNING : WaferTransferStateBase
    {
        public TCW_RUNNING(WaferTransferModule module) : base(module) {
            try
            {
                if (Module.StageSupervisor().Get_TCW_Mode() == TCW_Mode.OFF)
                {
                    Module.StageSupervisor().Set_TCW_Mode(true);
                }

                EventCodeEnum ret = Module.StageSupervisor().StageModuleState.MoveTCW_Position();

                if(ret == EventCodeEnum.NONE)
                {
                    var pivinfo = new PIVInfo() { FoupNumber = Module.GetParam_Wafer().GetOriginFoupNumber(), 
                                                  WaferID = Module.GetParam_Wafer().GetSubsInfo().WaferID.Value, 
                                                  SetTemperature = Module.TempController().TempInfo.SetTemp.Value, 
                                                  CurTemperature = module.TempController().TempInfo.CurTemp.Value,
                                                  RecipeID = module.FileManager().GetDeviceName()};

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    Module.EventManager().RaisingEvent(typeof(TcwMoveCompleteEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "TCW_RUNNING");
                }
                else
                {
                    Module.LotOPModule().ReasonOfError.AddEventCodeInfo(ret, "TCW Lot Pause by Move Error.\n Move TCW Position Failed.", this.GetType().Name);
                    Module.LotOPModule().PauseSourceEvent = Module.LotOPModule().ReasonOfError.GetLastEventCode();
                    Module.CommandManager().SetCommand<ILotOpPause>(Module);
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public override ModuleStateEnum GetState => ModuleStateEnum.RUNNING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
             
                if (!Module.NotifyManager().GetLastStageMSG().Equals("TCW_RUNNING"))
                {
                    Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "TCW_RUNNING");
                }

                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;
                bool ret;
             
                   
                    #region => Process IChuckUnloadCommand
                    conditionFuc = () =>
                    {
                        return true;
                    };

                    doAction = () =>
                    {
                      
                        Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.PROCESSED);

                        Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "TCW_END");
                        Module.InnerStateTransition(new IDLE(Module));
                    };
                    abortAction = () => { LoggerManager.Debug($"[IChuckUnloadCommand] : Aborted."); };

                   
                        ret = Module.CommandManager().ProcessIfRequested<IChuckUnloadCommand>(
                            Module,
                            conditionFuc,
                            doAction,
                            abortAction);


                #endregion

                if (Module.LotOPModule().ModuleState.State == ModuleStateEnum.PAUSED)
                {
                    //Transfer 중에 Pause 들어왔을 경우를 대비하기 위함.
                    this.Pause();
                }
                else if (Module.LotOPModule().ModuleState.State == ModuleStateEnum.PAUSING)
                {
                    //do nothing
                }
                else if(Module.LotOPModule().ModuleState.State != ModuleStateEnum.RUNNING) 
                {
                    LoggerManager.Error($"[WaferTransferModule] TCW_RUNNING.Execute(): Unexpected Lot State. Current Lot State:{Module.LotOPModule().ModuleState.State}");
                    // 무조건 Pause() Action 을 하고  Paused 상태에서 End() 를 호출하도록 만들어줘야함. 여기는 들어오면 안되는 상태

                }

                retVal = EventCodeEnum.NONE;
              
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected =
                    token is IChuckUnloadCommand;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.PreInnerState = this;
                Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "TCW_PAUSE");
                retVal = Module.InnerStateTransition(new PAUSE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
    }
    public class PENDING : WaferTransferStateBase
    {
        public PENDING(WaferTransferModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.PENDING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;

                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING || Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.ABORT || Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.PAUSING)
                {
                    #region => Process IChuckLoadCommand
                    conditionFuc = () =>
                    {
                        var chuckWaferStatus = Module.StageSupervisor().WaferObject.GetStatus();
                        //var chuckWaferState = Module.StageSupervisor.WaferObject.GetState();

                        bool isPassWaferStatus = chuckWaferStatus == EnumSubsStatus.NOT_EXIST;
                        return isPassWaferStatus;
                    };

                    doAction = () =>
                    {

                        Module.ActivateProcModule(WaferTransferTypeEnum.Loading);

                        Module.InnerStateTransition(new PROCESSING(Module));

                    };
                    abortAction = () => { LoggerManager.Debug($"[IChuckLoadCommand] : Aborted."); };

                    bool ret;

                    ret = Module.CommandManager().ProcessIfRequested<IChuckLoadCommand>(
                        Module,
                        conditionFuc,
                        doAction,
                        abortAction);
                    #endregion

                    retVal = EventCodeEnum.NONE;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected =
                    token is IChuckLoadCommand;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.PreInnerState = this;
                retVal = Module.InnerStateTransition(new PAUSE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PENDING;
        }
    }
    public class ERROR : WaferTransferStateBase
    {
        public ERROR(WaferTransferModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.ERROR;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {
                //삼발이 올려져 있는거 확인하고 삼발이 진공 확인 되었을때
                //UnloadProcModule에서? LoadProcModule에서? 하고 ProcModule들 Idle로 바꿔주고

                //No WORKS.

                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;

                if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && Module.NeedToRecovery == true)
                {
                    #region => Process IChuckUnloadCommand
                    conditionFuc = () =>
                    {
                        return CanTransitionToRecoveryUnloading();
                    };

                    doAction = () =>
                    {
                        Module.ActivateProcModule(WaferTransferTypeEnum.Unloading);

                        Module.InnerStateTransition(new PROCESSING(Module));
                    };
                    abortAction = () => { LoggerManager.Debug($"[IChuckUnloadCommand] : Aborted."); };

                    Module.CommandManager().ProcessIfRequested<IChuckUnloadCommand>(
                        Module,
                        conditionFuc,
                        doAction,
                        abortAction);
                    #endregion

                    retVal = EventCodeEnum.NONE;
                }



                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected =
                   token is IChuckUnloadCommand;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }
        public override void ClearErrorState()
        {
            Module.InnerStateTransition(new IDLE(Module));
        }

        public override void SelfRecovery()
        {
            try
            {
                if (Module.ProcModule != null)
                {
                    Module.ProcModule.SelfRecovery();
                    Module.ProcModule = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class PAUSE : WaferTransferStateBase
    {
        public PAUSE(WaferTransferModule module) : base(module) { }

        public override ModuleStateEnum GetState => ModuleStateEnum.PAUSED;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;
            try
            {
                Func<bool> conditionFuc;
                Action doAction;
                Action abortAction;

                if (Module.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    #region => Process IChuckLoadCommand
                    conditionFuc = () =>
                    {
                        return CanTransitionToLoading(false, false);
                    };

                    doAction = () =>
                    {
                        bool isInjected = Module.CommandManager().SetCommand<IAirBlowChuckCleaningCommand>(Module);
                        if (isInjected)
                        {
                            Module.ActivateProcModule(WaferTransferTypeEnum.Loading);

                            Module.InnerStateTransition(new WAIT_FOR_CHUCK_BLOW(Module));
                        }
                        else
                        {
                            LoggerManager.Debug($"[IAirBlowChuckCleaningCommand] : Inject failed.");
                        }
                    };
                    abortAction = () => { LoggerManager.Debug($"[IChuckLoadCommand] : Aborted."); };

                    bool ret;

                    ret = Module.CommandManager().ProcessIfRequested<IChuckLoadCommand>(
                        Module,
                        conditionFuc,
                        doAction,
                        abortAction);
                    #endregion

                    #region => Process IChuckUnloadCommand
                    conditionFuc = () =>
                    {
                        return CanTransitionToUnloading(false, false);
                    };

                    doAction = () =>
                    {
                        bool isInjected = Module.CommandManager().SetCommand<IAirBlowWaferCleaningCommand>(Module);
                        if (isInjected)
                        {
                            Module.ActivateProcModule(WaferTransferTypeEnum.Unloading);

                            Module.InnerStateTransition(new WAIT_FOR_WAFER_BLOW(Module));
                        }
                        else
                        {
                            LoggerManager.Debug($"[IAirBlowWaferCleaningCommand] : Inject failed.");
                        }
                    };
                    abortAction = () => { LoggerManager.Debug($"[IChuckUnloadCommand] : Aborted."); };

                    Module.CommandManager().ProcessIfRequested<IChuckUnloadCommand>(
                        Module,
                        conditionFuc,
                        doAction,
                        abortAction);
                    #endregion
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = FreeRunForIdle();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected =
                      token is IChuckLoadCommand ||
                      token is IChuckUnloadCommand ||
                      token is ICardLoadCommand ||
                      token is ICardUnloadCommand;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.PreInnerState.GetModuleState()==ModuleStateEnum.RUNNING && Module.GetParam_Wafer().GetSubsInfo().WaferType == EnumWaferType.TCW)
                {
                    retVal = Module.InnerStateTransition(new TCW_RUNNING(Module));
                    
                } else
                {
                    retVal = Module.InnerStateTransition(Module.PreInnerState);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            
            try
            {  
                if(Module.GetParam_Wafer().WaferStatus == EnumSubsStatus.EXIST &&
                    Module.GetParam_Wafer().GetWaferType() == EnumWaferType.TCW &&
                    Module.PreInnerState.GetModuleState()==ModuleStateEnum.RUNNING)
                {
                    Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.PROCESSED);
                    //TODO: Skip 처리랑 Processed 처리 시나리오 확인해서 분리하기
                }

                FreeRunForDone();
                retVal = Module.InnerStateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }
}

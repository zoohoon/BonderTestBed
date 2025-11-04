using System;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System.Runtime.CompilerServices;
using ProberErrorCode;
using LoaderParameters;
using LoaderControllerBase;
using ProberInterfaces.State;
using LogModule;
using MetroDialogInterfaces;
using CognexOCRManualDialog;

namespace LoaderController.LoaderControllerStates
{
    public enum LoaderControllerStateEnum
    {
        IDLE,
        MONITORING,
        SUSPENDED,
        WAFER_TRANSFERING,
        PAUSED,
        ABORT,
        DONE,
        SELF_RECOVERY,
        ERROR,
        RECOVERY,
        RECOVERY_DONE,
        UNLOADALL
    }

    public abstract class LoaderControllerState : IInnerState
    {
        public abstract EventCodeEnum Execute();

        public abstract ModuleStateEnum GetModuleState();

        public abstract EventCodeEnum Pause();

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        //public virtual EventCodeEnum ClearState()
        //{
        //    return EventCodeEnum.NONE;
        //}

        public abstract EventCodeEnum ClearState();

        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
        public virtual LoaderMapEditor GetLoaderMapEditor()
        {
            //RaiseInvalidState();
            return LoaderMapEditor.ERROR_EDITOR;
        }
    }

    public abstract class LoaderControllerStateBase : LoaderControllerState
    {
        public LoaderController Module { get; set; }

        public LoaderControllerStateBase(LoaderController module)
        {
            this.Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new IDLE(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }

        public void StateTransition(LoaderControllerStateEnum state)
        {
            try
            {
                switch (state)
                {
                    case LoaderControllerStateEnum.IDLE:
                        Module.InnerStateTransition(new IDLE(Module));
                        break;
                    case LoaderControllerStateEnum.MONITORING:
                        Module.InnerStateTransition(new MONITORING(Module));
                        break;
                    case LoaderControllerStateEnum.SUSPENDED:
                        Module.InnerStateTransition(new SUSPENDED(Module));
                        break;
                    case LoaderControllerStateEnum.WAFER_TRANSFERING:
                        Module.InnerStateTransition(new WAFER_TRANSFERING(Module));
                        break;
                    case LoaderControllerStateEnum.PAUSED:
                        Module.InnerStateTransition(new PAUSED(Module));
                        break;
                    case LoaderControllerStateEnum.RECOVERY:
                        Module.InnerStateTransition(new RECOVERY(Module));
                        break;
                    case LoaderControllerStateEnum.ABORT:
                        Module.InnerStateTransition(new ABORT(Module));
                        break;
                    case LoaderControllerStateEnum.DONE:
                        Module.InnerStateTransition(new DONE(Module));
                        break;
                    case LoaderControllerStateEnum.SELF_RECOVERY:
                        Module.InnerStateTransition(new SELF_RECOVERY(Module));
                        break;
                    case LoaderControllerStateEnum.UNLOADALL:
                        Module.InnerStateTransition(new UNLOADALL(Module));
                        break;
                    case LoaderControllerStateEnum.ERROR:
                        Module.InnerStateTransition(new ERROR(Module));
                        break;
                        // LoggerManager.Debug($"[LoaderController].ControllerStateTransition() : {this.GetType()}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public abstract ModuleStateEnum ModuleState { get; }

        public abstract LoaderControllerStateEnum State { get; }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    Module.PreInnerState = this;
                    StateTransition(LoaderControllerStateEnum.PAUSED);
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
            return retVal;
        }

        protected EventCodeEnum RaiseInvalidState([CallerMemberName] string memberName = "")
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"LoaderController.{memberName}() : Invalid state called.");
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum LoaderSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.LoaderService.LoaderSystemInit();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual bool CanExecute(IProbeCommandToken token)
        {
            //RaiseInvalidState();
            return false;
        }
        public virtual EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum RetractAll()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum JogAbsMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum JogRelMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        protected void WaitForLoaderState(ModuleStateEnum state)
        {
            try
            {
                while (true)
                {
                    var loaderState = Module.LoaderInfo.ModuleInfo.ModuleState;
                    if (loaderState == state)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


    }

    public class IDLE : LoaderControllerStateBase
    {
        public IDLE(LoaderController module) : base(module)
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.IDLE;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.IDLE;

        public override EventCodeEnum JogAbsMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.LoaderService.MOTION_JogAbsMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        public override EventCodeEnum JogRelMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.LoaderService.MOTION_JogRelMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.LoaderService.UpdateSystemParam(systemParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.LoaderService.SaveSystemParam(systemParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.LoaderService.UpdateDeviceParam(deviceParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = (Module as IHasDevParameterizable).SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override EventCodeEnum LoaderSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.LoaderService.LoaderSystemInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.LoaderService.MoveToModuleForSetup(module, skipuaxis, slot, index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;

        }

        public override EventCodeEnum RetractAll()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.LoaderService.RetractAll();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is ILoaderMapCommand;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        private EventCodeEnum FreeRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.CommandRecvSlot.IsRequested<ILoaderMapCommand>())
                {
                    Func<bool> conditionFuc = () => Module.CommandRecvSlot.Token.Parameter is LoaderMapCommandParameter;
                    Action doAction = () =>
                    {
                        LoaderMapCommandParameter cmdParam = Module.CommandRecvProcSlot.Token.Parameter as LoaderMapCommandParameter;

                        ResponseResult rr = Module.LoaderService.SetRequest(cmdParam.Editor.EditMap);

                        if (rr.IsSucceed)
                        {
                            StateTransition(LoaderControllerStateEnum.MONITORING);
                        }
                        else
                        {
                            Module.MetroDialogManager().ShowMessageDialog("Warnning", rr.ErrorMessage, EnumMessageStyle.Affirmative);
                        }
                    };
                    Action abortAction = () =>
                    {
                        LoggerManager.Debug("[ILoaderMapCommand] : Aborted.");
                    };

                    Module.CommandManager().ProcessIfRequested<ILoaderMapCommand>(
                        Module,
                        conditionFuc,
                        doAction,
                        abortAction);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if ((Module.LoaderOPModule() as IStateModule).ModuleState.GetState() == ModuleStateEnum.IDLE ||
                    (Module.LoaderOPModule() as IStateModule).ModuleState.GetState() == ModuleStateEnum.PAUSED ||
                    (Module.LoaderOPModule() as IStateModule).ModuleState.GetState() == ModuleStateEnum.PENDING ||
                    (Module.LoaderOPModule() as IStateModule).ModuleState.GetState() == ModuleStateEnum.UNDEFINED)
                {
                    retVal = FreeRun();
                }
                else
                {
                    if (Module.CommandRecvSlot.IsRequested<ILoaderMapCommand>())
                    {
                        Func<bool> conditionFuc = () => Module.CommandRecvSlot.Token.Parameter is LoaderMapCommandParameter;
                        Action doAction = () =>
                        {
                            var cmdParam = Module.CommandRecvProcSlot.Token.Parameter as LoaderMapCommandParameter;

                            ResponseResult rr = Module.LoaderService.SetRequest(cmdParam.Editor.EditMap);

                            if (rr.IsSucceed)
                            {
                                StateTransition(LoaderControllerStateEnum.MONITORING);
                            }
                            else
                            {
                                LoggerManager.Error(rr.ErrorMessage);
                            }
                        };
                        Action abortAction = () => { LoggerManager.Debug("[ILoaderMapCommand] : Aborted."); };

                        Module.CommandManager().ProcessIfRequested<ILoaderMapCommand>(
                            Module,
                            conditionFuc,
                            doAction,
                            abortAction);

                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        //TODO : Check Polish Need ??
                        if (!Module.PolishWaferModule().IsReadyPolishWafer())
                        {
                            bool isNeedWafer = false;
                            string cstHashCodeOfRequestLot = "";
                            Module.WaferTransferScheduler.UpdateState(out isNeedWafer, out cstHashCodeOfRequestLot);

                            var schRel = Module.WaferTransferScheduler.Execute();

                            if (schRel.ResultCode == WaferTransferScheduleRelEnum.NEED)
                            {
                                ResponseResult rr = Module.LoaderService.SetRequest(schRel.Editor.EditMap);

                                if (rr.IsSucceed)
                                {
                                    StateTransition(LoaderControllerStateEnum.MONITORING);
                                }
                                else
                                {
                                    LoggerManager.Error(rr.ErrorMessage);
                                }
                            }
                            else if (schRel.ResultCode == WaferTransferScheduleRelEnum.NOT_NEED)
                            {
                                //No Works.
                            }
                            else//Error
                            {
                                LoggerManager.Error($"MapScheduleState.DoSchedule() : Logic error.");
                            }
                        }
                        else
                        {
                            //command 가 제대로 동작하는지 ..? 확인
                            //다 정상적이라면 MONITORING 으로 상태 변경.
                            retVal = FreeRun();
                            Module.CommandRecvSlot.ClearToken();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                StateTransition(LoaderControllerStateEnum.ERROR);
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override LoaderMapEditor GetLoaderMapEditor()
        {
            LoaderMapEditor retVal = null;
            try
            {
                var schedulingMap = Module.LoaderInfo.StateMap as LoaderMap;
                retVal = new LoaderMapEditor(schedulingMap);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //Module.LoaderService.SetPause();
            //while (Module.LoaderInfo.ModuleInfo.ModuleState == ModuleStateEnum.PAUSED)
            //{

            //}
            try
            {
                Module.PreInnerState = this;

                StateTransition(LoaderControllerStateEnum.PAUSED);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class MONITORING : LoaderControllerStateBase
    {
        public MONITORING(LoaderController module) : base(module)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.MONITORING;

        LoaderInfo prevInfo = null;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                try
                {
                    var currInfo = Module.LoaderInfo;

                    var WaferTransferModuleState = Module.WaferTransferModule().ModuleState.GetState();
                    //if (WaferTransferModuleState == ModuleStateEnum.PAUSED)
                    //{
                    //    Pause();
                    //}
                    //else
                    //{
                    //if (prevInfo == null || prevInfo.TimeStamp < currInfo.TimeStamp)
                    {
                        switch (currInfo.ModuleInfo.ModuleState)
                        {
                            case ModuleStateEnum.SUSPENDED:
                                StateTransition(LoaderControllerStateEnum.SUSPENDED);
                                break;
                            case ModuleStateEnum.ERROR:
                                //Module.ReasonOfError.Reason = Module.LoaderService.GetResonOfError();

                                Module.ReasonOfError.AddEventCodeInfo(EventCodeEnum.UNDEFINED, Module.LoaderService.GetResonOfError(), Module.ControllerState.State.ToString());

                                StateTransition(LoaderControllerStateEnum.SELF_RECOVERY);
                                break;
                            case ModuleStateEnum.DONE:
                                StateTransition(LoaderControllerStateEnum.DONE);
                                break;
                        }

                        prevInfo = currInfo;
                    }
                    // }
                    retVal = EventCodeEnum.NONE;

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
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                // Module.LoaderService.SetPause();
                Module.PreInnerState = this;
                //while (Module.LoaderInfo.ModuleInfo.ModuleState == ModuleStateEnum.PAUSED)
                //{

                //}
                Module.InnerStateTransition(new PAUSED(Module));
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class SUSPENDED : LoaderControllerStateBase
    {
        public SUSPENDED(LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.SUSPENDED;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.SUSPENDED;

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is ILoaderMapCommand;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        private bool CanRunningForLoad(bool isFreeRun)
        {
            try
            {
                bool isPassWaferStatus = Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.NOT_EXIST;
                bool isPassWaferState = true;
                //bool isPinAligndone = Module.GetParam_ProbeCard().AlignState.Value == AlignStateEnum.DONE;
                // bool isNcdone = Module.GetParam_NcObject().NeedleCleaningProcessed == AlignStateEnum.DONE;
                bool isRunningState = false;

                if (isFreeRun)
                {
                    isRunningState = Module.SequenceEngineManager().isMovingState();
                }
                else
                {
                    isRunningState = Module.SequenceEngineManager().GetRunState();
                }

                return
                    //isPinAligndone &&
                    //isGetresultMap &&
                    isPassWaferStatus &&
                    isPassWaferState &&
                    isRunningState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private bool CanRunningForUnload(bool isFreeRun)
        {
            bool retVal = false;

            try
            {
                bool isPassWaferStatus = Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST;
                bool isPassWaferState;
                bool isRunningState = true;
                if (isFreeRun)
                {
                    isPassWaferState = true;
                    isRunningState = Module.SequenceEngineManager().isMovingState();
                }
                else
                {
                    isPassWaferState =
                        Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.PROCESSED ||
                        Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.SKIPPED ||
                        Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.READY ||
                        Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.SOAKINGSUSPEND ||
                        Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.SOAKINGDONE
                        ;
                }

                retVal =
                    isPassWaferStatus &&
                    isPassWaferState &&
                    isRunningState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum FreeRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var reasonOfSuspended = Module.LoaderInfo.ModuleInfo.ReasonOfSuspended;

                if (reasonOfSuspended == ReasonOfSuspendedEnum.LOAD)
                {
                    if (CanRunningForLoad(true))
                    {
                        if (Module.ResultMapManager().NeedDownload() == true)
                        {
                            retVal = Module.ResultMapManager().ConvertResultMapToProberDataAfterReadyToLoadWafer();

                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error("[LoaderController.SUSPENDED], FreeRun() : ConvertResultMapToProberDataAfterReadyToLoadWafer failed.");
                            }
                        }

                        retVal = Module.LoaderService.AwakeProcessModule();

                        if (retVal == EventCodeEnum.NONE)
                        {
                            bool isInjected = Module.CommandManager().SetCommand<IChuckLoadCommand>(Module);

                            while (true)
                            {
                                var loaderState = Module.WaferTransferModule().ModuleState.GetState();
                                if (loaderState == ModuleStateEnum.RUNNING)
                                {
                                    break;
                                }

                                System.Threading.Thread.Sleep(1);
                            }

                            StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);
                        }
                        else
                        {
                            LoggerManager.Error($"LoaderControllerSuspendedState.FreeRun() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                        }
                    }
                }
                else if (reasonOfSuspended == ReasonOfSuspendedEnum.UNLOAD)
                {
                    if (CanRunningForUnload(true))
                    {
                        retVal = Module.LoaderService.AwakeProcessModule();
                        if (retVal == EventCodeEnum.NONE)
                        {
                            WaitForLoaderState(ModuleStateEnum.RUNNING);

                            bool isInjected = Module.CommandManager().SetCommand<IChuckUnloadCommand>(Module);

                            while (true)
                            {
                                var loaderState = Module.WaferTransferModule().ModuleState.GetState();
                                if (loaderState == ModuleStateEnum.RUNNING)
                                {
                                    break;
                                }

                                System.Threading.Thread.Sleep(1);
                            }
                            StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);

                        }
                        else
                        {
                            LoggerManager.Error($"LoaderControllerSuspendedState.FreeRun() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                        }
                    }
                }
                else if (reasonOfSuspended == ReasonOfSuspendedEnum.OCR_REMOTING)
                {
                    //==> none
                }
                else if (reasonOfSuspended == ReasonOfSuspendedEnum.OCR_ABORT)
                {
                    //Task.Delay(1000).Wait();

                    //string caption = "Scan Fail";
                    //string msg = "Scan Data Invalid";

                    retVal = Module.LoaderService.AbortRequest();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = Module.LoaderService.ClearRequestData();
                    }

                    if (retVal == EventCodeEnum.NONE)
                    {
                        StateTransition(LoaderControllerStateEnum.IDLE);
                    }

                    //StateTransition(LoaderControllerStateEnum.IDLE);
                }
                else if (reasonOfSuspended == ReasonOfSuspendedEnum.OCR_FAILED)
                {
                    //TODO : 
                    //1. Wait for state running

                    //2. Change OCR Control UI (func : Light Control, Input by user, Resume)

                    //3. Transition to OCR Remote State

                    //==> ReadCognexOCR Process가 WaitForOCRRemoteState 로 대기중이며 이 때문에 
                    //==> Loader는 Suspend 상태에 빠진다.
                    //==> WaitForOCRRemoteState => RemotingState 로 변한다
                    retVal = Module.LoaderService.AwakeProcessModule();
                    WaitForLoaderState(ModuleStateEnum.RUNNING);

                    //CognexManualInput.AsyncShow();
                    CognexManualInput.AsyncShow(Module.GetContainer());

                    StateTransition(LoaderControllerStateEnum.MONITORING);
                }
                else if (reasonOfSuspended == ReasonOfSuspendedEnum.SCAN_FAILED)
                {
                    System.Threading.Thread.Sleep(1000);

                    retVal = Module.LoaderService.AbortRequest();
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = Module.LoaderService.ClearRequestData();
                    }
                    if (retVal == EventCodeEnum.NONE)
                    {
                        StateTransition(LoaderControllerStateEnum.IDLE);
                    }
                    //  LoggerManager.DebugError($"LoaderControllerSuspendedState.Run() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                    //throw new AccessViolationException();
                }
                else
                {
                    retVal = Module.LoaderService.AwakeProcessModule();
                    if (retVal == EventCodeEnum.NONE)
                    {
                        WaitForLoaderState(ModuleStateEnum.RUNNING);
                        StateTransition(LoaderControllerStateEnum.MONITORING);
                    }
                    else
                    {
                        LoggerManager.Error($"LoaderControllerSuspendedState.FreeRun() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ModuleStateEnum loaderOPState = (Module.LoaderOPModule() as IStateModule).ModuleState.GetState();

                if (loaderOPState == ModuleStateEnum.IDLE ||
                    loaderOPState == ModuleStateEnum.PAUSED ||
                    loaderOPState == ModuleStateEnum.PENDING)
                {
                    retVal = FreeRun();
                }
                else
                {
                    var reasonOfSuspended = Module.LoaderInfo.ModuleInfo.ReasonOfSuspended;

                    if (Module.CommandRecvSlot.GetState() == CommandStateEnum.REQUESTED)
                    {
                        if (reasonOfSuspended == ReasonOfSuspendedEnum.OCR_ABORT)
                        {
                            retVal = Module.LoaderService.AbortRequest();

                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = Module.LoaderService.ClearRequestData();
                            }

                            if (retVal == EventCodeEnum.NONE)
                            {
                                StateTransition(LoaderControllerStateEnum.IDLE);
                            }
                        }
                        else
                        {
                            retVal = Module.LoaderService.AbortRequest();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                StateTransition(LoaderControllerStateEnum.ABORT);
                            }
                        }
                    }
                    else
                    {
                        //var reasonOfSuspended = Module.LoaderInfo.ModuleInfo.ReasonOfSuspended;

                        if (reasonOfSuspended == ReasonOfSuspendedEnum.LOAD)
                        {
                            if (CanRunningForLoad(false))
                            {
                                if (Module.ResultMapManager().NeedDownload() == true)
                                {
                                    retVal = Module.ResultMapManager().ConvertResultMapToProberDataAfterReadyToLoadWafer();

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Error("[LoaderController.SUSPENDED], Execute() : ConvertResultMapToProberDataAfterReadyToLoadWafer failed.");
                                    }
                                }

                                var wtfModule = Module.WaferTransferModule();
                                var lotModuleInnerState = Module.LotOPModule().InnerState as LotOPState;

                                if ((wtfModule.ModuleState.GetState() == ModuleStateEnum.IDLE
                                    || wtfModule.ModuleState.GetState() == ModuleStateEnum.DONE
                                    || wtfModule.ModuleState.GetState() == ModuleStateEnum.PENDING)
                                    && lotModuleInnerState.GetState() != LotOPStateEnum.PAUSING)
                                {
                                    Module.LotOPModule().ModuleStopFlag = true;
                                    retVal = Module.LoaderService.AwakeProcessModule();
                                    if (retVal == EventCodeEnum.NONE)
                                    {

                                        WaitForLoaderState(ModuleStateEnum.RUNNING);
                                        bool isInjected = Module.CommandManager().SetCommand<IChuckLoadCommand>(Module);
                                        if (isInjected)
                                        {
                                            while (true)
                                            {
                                                var wtfstate = wtfModule.ModuleState.GetState();
                                                if (wtfstate != ModuleStateEnum.IDLE)
                                                {
                                                    StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);
                                                    break;
                                                }

                                                System.Threading.Thread.Sleep(1);
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug("LoaderControllerSuspendedState Load: IChuckLoadCommand REJECTED");
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Error($"LoaderControllerSuspendedState.Run() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug("LoaderController Load:  WaferTransferModuleState" + wtfModule.ModuleState.GetState() + ", LotModuleState: " + lotModuleInnerState.GetState());
                                }
                                //else if (lotModuleInnerState.GetState() == LotOPStateEnum.PAUSING || lotModuleInnerState.GetState() == LotOPStateEnum.PAUSED)
                                //{
                                //    this.Pause();
                                //}
                            }
                            //else if (this.Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSING)
                            //{
                            //    this.Pause();
                            //}
                            //else
                            //{
                            //    // TODO : Pause 해도 되는지 ...?
                            //    this.Pause();
                            //}
                        }
                        else if (reasonOfSuspended == ReasonOfSuspendedEnum.UNLOAD)
                        {
                            if (CanRunningForUnload(false))
                            {
                                var wtfModule = Module.WaferTransferModule();

                                if (wtfModule.ModuleState.GetState() == ModuleStateEnum.IDLE
                                || wtfModule.ModuleState.GetState() == ModuleStateEnum.DONE)
                                {
                                    retVal = Module.LoaderService.AwakeProcessModule();
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        WaitForLoaderState(ModuleStateEnum.RUNNING);

                                        bool isInjected = Module.CommandManager().SetCommand<IChuckUnloadCommand>(Module);
                                        if (isInjected)
                                        {
                                            while (true)
                                            {
                                                var wtfstate = wtfModule.ModuleState.GetState();
                                                if (wtfstate != ModuleStateEnum.IDLE)
                                                {
                                                    StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);
                                                    break;
                                                }

                                                System.Threading.Thread.Sleep(1);

                                            }
                                        }

                                        //while (true)
                                        //{
                                        //    var loaderState = Module.WaferTransferModule().ModuleState.GetState();
                                        //    if (loaderState == ModuleStateEnum.RUNNING)
                                        //    {
                                        //        StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);
                                        //        break;
                                        //    }
                                        //    else if (loaderState == ModuleStateEnum.PAUSED)
                                        //    {
                                        //        break;
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        LoggerManager.Error($"LoaderControllerSuspendedState.Run() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                                    }
                                }
                            }
                        }
                        else if (reasonOfSuspended == ReasonOfSuspendedEnum.OCR_REMOTING)
                        {
                            //==> none
                        }
                        else if (reasonOfSuspended == ReasonOfSuspendedEnum.OCR_ABORT)
                        {
                            retVal = Module.LoaderService.AbortRequest();

                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = Module.LoaderService.ClearRequestData();
                            }

                            if (retVal == EventCodeEnum.NONE)
                            {
                                StateTransition(LoaderControllerStateEnum.IDLE);
                            }
                        }
                        else if (reasonOfSuspended == ReasonOfSuspendedEnum.OCR_FAILED)
                        {
                            //TODO : 
                            //1. Wait for state r-unning
                            //2. Change OCR Control UI (func : Light Control, Input by user, Resume)
                            //3. Transition to OCR Remote State


                            //==> ReadCognexOCR Process가 WaitForOCRRemoteState 로 대기중이며 이 때문에 
                            //==> Loader는 Suspend 상태에 빠진다.
                            //==> WaitForOCRRemoteState => RemotingState 로 변한다
                            retVal = Module.LoaderService.AwakeProcessModule();
                            WaitForLoaderState(ModuleStateEnum.RUNNING);

                            //CognexManualInput.AsyncShow();
                            CognexManualInput.AsyncShow(Module.GetContainer());

                            StateTransition(LoaderControllerStateEnum.MONITORING);
                        }
                        else if (reasonOfSuspended == ReasonOfSuspendedEnum.SCAN_FAILED)
                        {
                            retVal = Module.LoaderService.AbortRequest();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = Module.LoaderService.ClearRequestData();
                            }
                            if (retVal == EventCodeEnum.NONE)
                            {
                                StateTransition(LoaderControllerStateEnum.IDLE);
                            }

                            this.Module.CommandManager().SetCommand<ILotOpPause>(this);
                            string caption = "LOT PAUSE";
                            string msg = "Scan Data Invalid";

                            var dlgRel = this.Module.MetroDialogManager().ShowMessageDialog(caption, msg, EnumMessageStyle.Affirmative);

                            //throw new AccessViolationException();
                        }
                        else
                        {
                            retVal = Module.LoaderService.AwakeProcessModule();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                WaitForLoaderState(ModuleStateEnum.RUNNING);
                                StateTransition(LoaderControllerStateEnum.MONITORING);
                            }
                            else
                            {
                                LoggerManager.Error($"LoaderControllerSuspendedState.Run() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                            }

                            ////TODO : 에러..
                            //LoggerManager.DebugError($"LoaderControllerSuspendedState.Run() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                            ////throw new AccessViolationException();
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //Module.LoaderService.SetPause();
            //while (Module.LoaderInfo.ModuleInfo.ModuleState == ModuleStateEnum.PAUSED)Mo
            //{

            //}
            try
            {


                Module.PreInnerState = this;

                StateTransition(LoaderControllerStateEnum.PAUSED);
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override LoaderMapEditor GetLoaderMapEditor()
        {
            LoaderMapEditor retVal = null;

            try
            {
                var schedulingMap = Module.ReqMap as LoaderMap;

                retVal = new LoaderMapEditor(schedulingMap);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }
    }

    public class WAFER_TRANSFERING : LoaderControllerStateBase
    {
        public WAFER_TRANSFERING(LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.PENDING;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.WAFER_TRANSFERING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                var WaferTransferModuleState = Module.WaferTransferModule().ModuleState.GetState();

                if (WaferTransferModuleState == ModuleStateEnum.DONE || WaferTransferModuleState == ModuleStateEnum.IDLE)
                {
                    StateTransition(LoaderControllerStateEnum.MONITORING);
                    retVal = EventCodeEnum.NONE;
                }
                else if (WaferTransferModuleState == ModuleStateEnum.ERROR)
                {
                    StateTransition(LoaderControllerStateEnum.SELF_RECOVERY);
                    retVal = EventCodeEnum.NONE;
                }
                else if (WaferTransferModuleState == ModuleStateEnum.PAUSED)
                {
                    Pause();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PENDING;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //Module.LoaderService.SetPause();
            //while (Module.LoaderInfo.ModuleInfo.ModuleState == ModuleStateEnum.PAUSED)
            //{

            //}
            try
            {

                Module.PreInnerState = this;

                StateTransition(LoaderControllerStateEnum.PAUSED);
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class PAUSED : LoaderControllerStateBase
    {
        public PAUSED(LoaderController module) : base(module)
        {
        }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.PAUSED;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.PAUSED;

        public override EventCodeEnum Execute()
        {

            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {


                Module.LoaderService.SetResume();
                Module.InnerStateTransition(Module.PreInnerState);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                var currInfo = Module.LoaderInfo;

                switch (currInfo.ModuleInfo.ModuleState)
                {
                    case ModuleStateEnum.SUSPENDED:
                        retVal = Module.LoaderService.AbortRequest();
                        break;
                    case ModuleStateEnum.DONE:
                        retVal = Module.LoaderService.ClearRequestData();
                        break;
                        //case ModuleStateEnum.PAUSED:
                        //    retVal=Module.
                }

                if (retVal == EventCodeEnum.NONE && Module.LoaderService.GetLoaderInfo().ModuleInfo.ModuleState == ModuleStateEnum.ABORT)
                {
                    retVal = Module.LoaderService.ClearRequestData();
                }

                if (retVal == EventCodeEnum.NONE
                    || Module.LoaderService.GetLoaderInfo().ModuleInfo.ModuleState == ModuleStateEnum.IDLE)
                {
                    StateTransition(LoaderControllerStateEnum.ABORT);
                }
                else
                {
                    RaiseInvalidState();//Error
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }
    }

    public class ABORT : LoaderControllerStateBase
    {
        public ABORT(LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.ABORT;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.ABORT;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.LoaderService.ClearRequestData();

                if (retVal == EventCodeEnum.NONE
                    || Module.LoaderService.GetLoaderInfo().ModuleInfo.ModuleState == ModuleStateEnum.IDLE)
                {
                    //Module.LotEndCassetteFlag = true;
                    Module.ReqMap = null;

                    StateTransition(LoaderControllerStateEnum.UNLOADALL);
                }
                else
                {
                    RaiseInvalidState();//Error
                }

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }
        public override EventCodeEnum LoaderSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.LoaderService.LoaderSystemInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class UNLOADALL : LoaderControllerStateBase
    {
        public UNLOADALL(LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.ABORT;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.ABORT;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                var loaderCntrllr = Module.LoaderController() as ILoaderControllerExtension;
                var currentMap = loaderCntrllr.LoaderInfo.StateMap;
                var editor = loaderCntrllr.GetLoaderMapEditor();
                bool isUnloadWafer = false;

                foreach (var armModule in currentMap.ARMModules)
                {
                    if (armModule.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        isUnloadWafer = true;
                        editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                    }
                }
                foreach (var preModule in currentMap.PreAlignModules)
                {
                    if (preModule.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        isUnloadWafer = true;
                        editor.EditorState.SetTransfer(preModule.Substrate.ID.Value, preModule.Substrate.OriginHolder);
                    }
                }
                foreach (var chuckModule in currentMap.ChuckModules)
                {
                    if (chuckModule.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        isUnloadWafer = true;
                        editor.EditorState.SetTransfer(chuckModule.Substrate.ID.Value, chuckModule.Substrate.OriginHolder);
                    }
                }
                bool isInjected = true;
                if (isUnloadWafer)
                {
                    LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                    cmdParam.Editor = editor;
                    isInjected = Module.CommandManager().SetCommand<ILoaderMapCommand>(this, cmdParam);
                }
                if (isInjected)
                {
                    Module.SetLotEndFlag(true);
                    StateTransition(LoaderControllerStateEnum.DONE);
                    retVal = EventCodeEnum.NONE;

                }
                else
                {
                    retVal = EventCodeEnum.NODATA;
                    throw new Exception();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is ILoaderMapCommand;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }
        public override LoaderMapEditor GetLoaderMapEditor()
        {
            LoaderMapEditor retVal = null;

            try
            {
                var schedulingMap = Module.LoaderInfo.StateMap as LoaderMap;
                retVal = new LoaderMapEditor(schedulingMap);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }
    }
    public class DONE : LoaderControllerStateBase
    {
        public DONE(LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.DONE;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.DONE;

        private EventCodeEnum FreeRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.LoaderService.ClearRequestData();
                if (retVal == EventCodeEnum.NONE)
                {
                    Module.ReqMap = null;

                    StateTransition(LoaderControllerStateEnum.IDLE);
                }
                else
                {
                    RaiseInvalidState();//Error
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //var reasonOfSuspended = Module.LoaderInfo.ModuleInfo.ReasonOfSuspended;
                if ((Module.LoaderOPModule() as IStateModule).ModuleState.GetState() == ModuleStateEnum.IDLE
                    || (Module.LoaderOPModule() as IStateModule).ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    retVal = FreeRun();
                }
                else
                {
                    //
                    bool isNeedWafer = false;
                    string cstHashCodeOfRequestLot = "";
                    Module.WaferTransferScheduler.UpdateState(out isNeedWafer, out cstHashCodeOfRequestLot);

                    //TODO : Foup Shift Mode 옵션정보 가져오기
                    bool isFoupShiftMode = false;
                    bool isCassetteDone;
                    if (isFoupShiftMode == true)
                    {
                        isCassetteDone =
                            Module.WaferTransferScheduler.State == WaferTransferSchedulerStateEnum.LAST_WAFER_ON_CHUCK_ONLY ||
                            Module.WaferTransferScheduler.State == WaferTransferSchedulerStateEnum.CASSETTE_DONE;
                    }
                    else
                    {
                        isCassetteDone =
                            Module.WaferTransferScheduler.State == WaferTransferSchedulerStateEnum.CASSETTE_DONE;
                    }

                    if (isCassetteDone)
                    {
                        Module.SetLotEndFlag(false);
                        bool isInjected = Module.CommandManager().SetCommand<ICassetteUnLoadCommand>(Module, new FoupCommandParam() { CassetteNumber = 1 });

                        if (isInjected)
                        {
                            retVal = Module.LoaderService.ClearRequestData();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                Module.ReqMap = null;
                                this.Module.LotOPModule().LotEndFlag = true;
                                StateTransition(LoaderControllerStateEnum.IDLE);
                            }
                            else
                            {
                                RaiseInvalidState();//Error
                            }
                        }
                    }
                    else
                    {
                        retVal = Module.LoaderService.ClearRequestData();
                        if (retVal == EventCodeEnum.NONE)
                        {
                            Module.ReqMap = null;
                            StateTransition(LoaderControllerStateEnum.IDLE);
                        }
                        else
                        {
                            RaiseInvalidState();//Error
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //Module.LoaderService.SetPause();
            //while (Module.LoaderInfo.ModuleInfo.ModuleState == ModuleStateEnum.PAUSED)
            //{

            //}
            try
            {

                Module.PreInnerState = this;

                Module.InnerStateTransition(new PAUSED(Module));
                //StateTransition(LoaderControllerStateEnum.PAUSED);
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class SELF_RECOVERY : LoaderControllerStateBase
    {
        public SELF_RECOVERY(LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.SELF_RECOVERY;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                bool isEanbleSelfRecoveryOption = true;

                if (isEanbleSelfRecoveryOption)
                {
                    if (Module.WaferTransferModule().ModuleState.GetState() == ModuleStateEnum.ERROR)
                    {
                        Module.WaferTransferModule().SelfRecovery();
                    }
                    else
                    {
                        Module.LoaderService.SelfRecovery();
                    }
                }

                StateTransition(LoaderControllerStateEnum.ERROR);

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
    }

    public class ERROR : LoaderControllerStateBase
    {
        public ERROR(LoaderController module) : base(module)
        {
            try
            {
                this.Module.MotionManager().LoaderEMGStop();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.ERROR;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.ERROR;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                //TODO
                //. Request Lot Pause

                //. Wait for Stage Idle(.. LotOp Pause
                bool isStageIdle = false; //get running state();



                //. Switching Recovery UI
                bool isActivateUI = true;
                if (isStageIdle == true)
                {
                    //TODO : UI
                    bool isAllSafeAttachedModules = false;

                    if (isAllSafeAttachedModules)
                    {
                        // Return Wafer 가능한 UI
                    }
                    else
                    {
                        // 사용자가 Unknwon Wafer 직접 제거 UI
                        // WaferDisappearVM.Show(this.Module.GetContainer());
                    }
                }

                //. State Transition to Recovery
                if (isActivateUI)
                {
                    StateTransition(LoaderControllerStateEnum.RECOVERY);
                }
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum LoaderSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.LoaderService.LoaderSystemInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }
    }

    public class RECOVERY : LoaderControllerStateBase
    {
        public RECOVERY(LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.RECOVERY;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.RECOVERY;

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.NONE;
        }

        //TODO : UI Methods
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is ILoaderMapCommand;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override LoaderMapEditor GetLoaderMapEditor()
        {
            LoaderMapEditor retVal = null;
            try
            {
                var schedulingMap = Module.LoaderInfo.StateMap as LoaderMap;
                retVal = new LoaderMapEditor(schedulingMap);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum LoaderSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                retVal = Module.LoaderService.LoaderSystemInit();
                if (retVal == EventCodeEnum.NONE)
                {
                    //CassetteModuleInfo cassette = Module.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();
                    //var foupController = Module.FoupOpModule().GetFoupController(cassette.ID.Index);
                    //if (foupController.FoupModuleInfo.State == FoupStateEnum.UNLOAD)
                    //{
                    //    retVal = foupController.Execute(new FoupLoadCommand() { });
                    //}

                    //if (foupController.FoupModuleInfo.State != FoupStateEnum.LOAD)
                    //{
                    //    string caption = "ERROR";
                    //    string message = $"Foup state invalid. state={foupController.FoupModuleInfo.State}";
                    //}

                    //if (cassette != null)
                    //{
                    //    var editor = Module.GetLoaderMapEditor();
                    //    editor.EditorState.SetScanCassette(cassette.ID);
                    //    LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                    //    cmdParam.Editor = editor;
                    //    ResponseResult rr = Module.LoaderService.SetRequest(cmdParam.Editor.EditMap);

                    //    while (true)
                    //    {
                    //        var currInfo = Module.LoaderInfo;
                    //        if (currInfo.ModuleInfo.ModuleState == ModuleStateEnum.SUSPENDED || currInfo.ModuleInfo.ModuleState == ModuleStateEnum.ERROR || currInfo.ModuleInfo.ModuleState == ModuleStateEnum.DONE)
                    //        {
                    //            break;
                    //        }
                    //        System.Threading.Thread.Sleep(1);
                    //    }
                    //}
                }

                //Module.WaitCancelDialogService().CloseDialog();
                Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                if (retVal == EventCodeEnum.NONE)
                {
                    if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                    {
                        StateTransition(LoaderControllerStateEnum.PAUSED);
                    }
                    else
                    {
                        StateTransition(LoaderControllerStateEnum.IDLE);
                    }
                }
                //else
                //{
                //    string message = retVal.ToString();
                //    string caption = "Loader Init Fail.";
                //    MessageBoxButton buttons = MessageBoxButton.OK;
                //    MessageBoxImage icon = MessageBoxImage.Question;
                //    // MessageBox.Show(message, caption, buttons, icon)==MessageBBO
                //    if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                //    {
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void ResetWaferLocation()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void EndRecoveryAndReturnAllWafers()
        {
            try
            {

                // TODO : 
                //1. Clear Loader State to IDLE

                //2. Clear Wafer Transfer Module State to IDLE

                bool hasUnloadableWafer = false; //현재위치와 Origin Position으로 비교하여 다른경우 UNLOAD

                //3. 꺼내진 웨이퍼가 존재하면 Origin위치로 이송하는 명령을 요청
                if (hasUnloadableWafer)
                {
                    var mapEditor = new LoaderMapEditor(Module.LoaderInfo.StateMap);

                    //TODO : 꺼내진 웨이퍼가 존재하면 Origin위치로 이송하는 명령을 요청

                    //

                    ResponseResult rr = Module.LoaderService.SetRequest(mapEditor.EditMap);

                    if (rr.IsSucceed)
                    {
                        StateTransition(LoaderControllerStateEnum.MONITORING);
                    }
                    else
                    {
                        LoggerManager.Error(rr.ErrorMessage);
                    }
                }
                else
                {
                    StateTransition(LoaderControllerStateEnum.IDLE);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RECOVERY;
        }

    }

}

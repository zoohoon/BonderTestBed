using System;
using System.Linq;

using Autofac;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System.Runtime.CompilerServices;
using ProberErrorCode;
using LoaderParameters;
using LoaderControllerBase;
using ProberInterfaces.State;
using LogModule;
using LoaderController.LoaderControllerStates;
using ProberInterfaces.WaferTransfer;

namespace LoaderController.GPController
{
    public abstract class GP_LoaderControllerState : IInnerState
    {
        internal int retryInterval = 150;
        public abstract EventCodeEnum Execute();
        public abstract LoaderControllerStateEnum GetState();

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
        public virtual LoaderMap RequestJob(out bool isExchange, out bool isNeedWafer, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            isExchange = false;
            isNeedWafer = false;
            cstHashCodeOfRequestLot = "";
            return null;
        }
        public virtual LoaderMap UnloadRequestJob()
        {
            return null;
        }
        public virtual bool LotOPStart()
        {
            return false;
        }
    }

    public abstract class GP_LoaderControllerStateBase : GP_LoaderControllerState
    {

        public GP_LoaderController Module { get; set; }

        public GP_LoaderControllerStateBase(GP_LoaderController module)
        {
            this.Module = module;
        }

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
                    case LoaderControllerStateEnum.UNLOADALL:
                        Module.InnerStateTransition(new UNLOADALL(Module));
                        break;
                    case LoaderControllerStateEnum.ERROR:
                        Module.InnerStateTransition(new ERROR(Module));
                        break;
                }

                //LoggerManager.Debug($"[LoaderController].ControllerStateTransition() : {this.GetType()}");
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

        public virtual bool CanExecute(IProbeCommandToken token)
        {
            //RaiseInvalidState();
            return false;
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

                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public  LoaderMap UnloadJob()
        {
            LoggerManager.Debug($"[GPLoaderController] UnloadJob() Execute");
            TransferObject unloadSub = this.Module.ReqMap.GetTransferObjectAll().Where(
                   item =>
                   item.CurrHolder == Module.ChuckID
                   ).FirstOrDefault();
            unloadSub.WaferState = Module.StageSupervisor().WaferObject.GetState();
            unloadSub.SetReservationState(ReservationStateEnum.RESERVATION);
            LoaderMapEditor editor = new LoaderMapEditor(this.Module.ReqMap);

            this.Module.GEMModule().GetPIVContainer().StageNumber.Value = this.Module.LoaderController().GetChuckIndex();
            this.Module.GEMModule().GetPIVContainer().SlotNumber.Value = Module.StageSupervisor().WaferObject.GetSubsInfo().SlotIndex.Value;
            this.Module.GEMModule().GetPIVContainer().CurTemperature.Value = this.Module.TempController().TempInfo.CurTemp.Value;

            // #Hynix_Merge: 검토 필요, Hynix코드는 아래와 같음. UnloadHolder 어디서 설정해주는지 확인 필요.
            bool isExistUnloadHolder = true;
            var unloadHolder = unloadSub.UnloadHolder;//#Hynix_Merge: RC_Integrted 에서는 Origin이 아닌 UnloadHolder로 Set함. 

            if (Module.LoaderInfo.FoupShiftMode == 1)
            {
                (isExistUnloadHolder, unloadHolder) = Module.ReqMap.IsUnloaderHolderExist(unloadSub);
            }

            editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadHolder);
            var needMap = WaferTransferScheduleResult.CreateNeed(editor);

            if (isExistUnloadHolder)
            {
                return needMap.Editor.EditMap;
            }
            else
            {
                return null;
            }
        }

        public override LoaderMap RequestJob(out bool isExchange, out bool isNeedWafer, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            isExchange = false;
            isNeedWafer = false;
            cstHashCodeOfRequestLot = "";

            try
            {
                Module.isExchange = isExchange;
                if (Module.IsAbort || canloadwafer == false)
                {
                    if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                    {

                        return UnloadRequestJob();
                    }
                    else
                    {
                        return null;// #Hynix_Merge 이부분 Dev_Integrated랑 다름. 
                    }
                }
                else if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSING || // #Hynix_Merge 이부분 Dev_Integrated랑 다름. else if 
                    Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED ||
                    Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT||
                    Module.LotOPModule().ModuleStopFlag ||
                    this.Module.SoakingModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    if (Module.LotOPModule().ErrorEndState == ErrorEndStateEnum.Unload || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT)// 이부분 Dev_Integrated랑 다름. 
                    {
                        if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                        {
                            return UnloadJob();
                        }
                    }
                    return null;
                }
                else if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.NOT_EXIST
                    || Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.UNDEFINED
                    || Module.GetParam_Wafer().GetState() == EnumWaferState.PROCESSED
                    || Module.GetParam_Wafer().GetState() == EnumWaferState.READY) // #Hynix_Merge  이부분 Dev_Integrated랑 다름. 
                {
                    isExchange = Module.WaferTransferScheduler.UpdateState(out isNeedWafer, out cstHashCodeOfRequestLot, canloadwafer);
                    Module.isExchange = isExchange;

                    var schRel = Module.WaferTransferScheduler.Execute();
                    if (schRel != null)
                    {
                        if (schRel.ResultCode == WaferTransferScheduleRelEnum.NEED)
                        {
                            return schRel.Editor.EditMap;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }

        public override LoaderMap UnloadRequestJob()
        {
            LoggerManager.Debug($"[GPLoaderController] UnloadRequestJob() Execute");
            TransferObject unloadSub = this.Module.ReqMap.GetTransferObjectAll().Where(
                   item =>
                   item.CurrHolder == Module.ChuckID
                   ).FirstOrDefault();
            unloadSub.WaferState = Module.StageSupervisor().WaferObject.GetState();
            unloadSub.SetReservationState(ReservationStateEnum.RESERVATION);
            LoaderMapEditor editor = new LoaderMapEditor(this.Module.ReqMap);

            this.Module.GEMModule().GetPIVContainer().StageNumber.Value = this.Module.LoaderController().GetChuckIndex();
            this.Module.GEMModule().GetPIVContainer().SlotNumber.Value = Module.StageSupervisor().WaferObject.GetSubsInfo().SlotIndex.Value;
            this.Module.GEMModule().GetPIVContainer().CurTemperature.Value = this.Module.TempController().TempInfo.CurTemp.Value;

            //#Hynix_Merge: 검토 필요, Hynix 코드는 아래와 같음. UnloadHolder어디서 설정해주는지 확인필요.
            //if (unloadSub.UnloadHolder.ModuleType != ModuleTypeEnum.UNDEFINED)
            //{
            //    editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.UnloadHolder);
            //}
            //else
            //{
            //    editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.OriginHolder);
            //}

            editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.UnloadHolder);
            var needMap = WaferTransferScheduleResult.CreateNeed(editor);

            return needMap.Editor.EditMap;
        }

        public override bool LotOPStart()
        {
            return (this).Module.CommandManager().SetCommand<ILotOpStart>(this);
        }
    }

    public class IDLE : GP_LoaderControllerStateBase
    {
        public IDLE(GP_LoaderController module) : base(module)
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

        public override EventCodeEnum RetractAll()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
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
      
        public override LoaderMap RequestJob(out bool isExchange, out bool isNeedWafer, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            isExchange = false;
            isNeedWafer = false;
            cstHashCodeOfRequestLot = "";

            try
            {
                if (!Module.MonitoringManager().IsMachineInitDone)
                {
                    return null;
                }

                Module.isExchange = isExchange;
                if (Module.IsAbort)
                {
                    LoaderMap ret = null;
                    if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST && Module.GetParam_Wafer().GetWaferType() != EnumWaferType.POLISH)
                    {
                        ret = UnloadRequestJob();
                    }

                    /// Lot idle일때
                    /// Soaking system param에서 pw사용
                    /// 현재 soaking state에서 pw사용 할때
                    /// chuck에 올라간 wafer type이 pw일때
                    if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE &&
                        Module.SoakingModule().IsUsePolishWafer() &&
                        Module.SoakingModule().IsEnablePolishWaferSoakingOnCurState() &&
                        Module.GetParam_Wafer().GetWaferType() == EnumWaferType.POLISH
                        )
                    {
                        //soaking이 running중이거나 pause일때는 pw를 반납하지 않도록
                        if (Module.SoakingModule().ModuleState.GetState() == ModuleStateEnum.PAUSED ||
                            Module.SoakingModule().ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                            Module.SoakingModule().ModuleState.GetState() == ModuleStateEnum.SUSPENDED)
                            ret = null;
                        else
                        {
                            //Maintain state에서는 pw사용이라면 주구장창 pw를 사용해야 하므로..
                            if (Module.SoakingModule().GetStatusSoakingState() == SoakingStateEnum.MAINTAIN)
                                ret = null;
                        }
                    }

                    // Module.IsAbort = false;  /// Lot end 후 Idle soaking(polish wafer를 이용한)의 구동이 필요하므로 해당 flag를 초기화 한다(기존에는 Lot start에 초기화하였기 때문에 Idle soaking 동작이 안됨)
                    if (ret != null)
                    {
                        return ret;
                    }
                }

                if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSING ||
                    Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED ||
                    Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT ||
                    Module.LotOPModule().ModuleStopFlag || this.Module.SoakingModule().ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                    Module.SoakingModule().ModuleState.GetState() == ModuleStateEnum.ERROR)
                {
                    if (Module.LotOPModule().ErrorEndState == ErrorEndStateEnum.Unload || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT)
                    {
                        if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                        {                         
                            return UnloadJob();
                        }
                    }
                    return null;
                }
                else if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.NOT_EXIST
                   || Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.UNDEFINED
                   || Module.GetParam_Wafer().GetState() == EnumWaferState.PROCESSED
                   || Module.GetParam_Wafer().GetState() == EnumWaferState.SKIPPED
                   || Module.GetParam_Wafer().GetState() == EnumWaferState.READY
                   || Module.GetParam_Wafer().GetState() == EnumWaferState.SOAKINGSUSPEND
                   || Module.GetParam_Wafer().GetState() == EnumWaferState.SOAKINGDONE
                   || Module.LotOPModule().ErrorEndState != ErrorEndStateEnum.NONE)// 정상 언로드 시에는 여기를 탐.
                {
                    isExchange = Module.WaferTransferScheduler.UpdateState(out isNeedWafer, out cstHashCodeOfRequestLot, canloadwafer);
                    Module.isExchange = isExchange;

                    var schRel = Module.WaferTransferScheduler.Execute();
                    if (schRel != null)
                    {
                        if (schRel.ResultCode == WaferTransferScheduleRelEnum.NEED)
                        {
                            return schRel.Editor.EditMap;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var currInfo = Module.LoaderInfo;
                if (currInfo != null)
                {
                    switch (currInfo.ModuleInfo.ModuleState)
                    {

                        case ModuleStateEnum.SUSPENDED:

                            if (currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.LOAD || currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.UNLOAD ||
                                currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.CARD_LOAD || currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.CARD_UNLOAD)
                            {
                                if (currInfo.ModuleInfo.ChuckNumber == Module.ChuckID.Index)
                                {
                                    StateTransition(LoaderControllerStateEnum.SUSPENDED);
                                }
                            }
                            break;
                        default:
                            break;
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


        private EventCodeEnum FreeRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.IDLE;
        }
    }

    public class MONITORING : GP_LoaderControllerStateBase
    {
        public MONITORING(GP_LoaderController module) : base(module)
        {
            try
            {
                //if (module.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                //{
                //    module.ViewModelManager().BackPreScreenTransition(false);
                //}
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
                                if (currInfo.ModuleInfo.ChuckNumber == Module.ChuckID.Index)
                                {
                                    StateTransition(LoaderControllerStateEnum.SUSPENDED);
                                }
                                break;
                            case ModuleStateEnum.ERROR:
                                StateTransition(LoaderControllerStateEnum.SELF_RECOVERY);
                                break;
                            case ModuleStateEnum.DONE:
                                StateTransition(LoaderControllerStateEnum.DONE);
                                break;
                            case ModuleStateEnum.IDLE:
                                StateTransition(LoaderControllerStateEnum.IDLE);
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

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.MONITORING;
        }
    }

    public class SUSPENDED : GP_LoaderControllerStateBase
    {

        public SUSPENDED(GP_LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

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

        public override LoaderMap RequestJob(out bool isExchange, out bool isNeedWafer, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            isExchange = false;
            isNeedWafer = false;
            cstHashCodeOfRequestLot = "";
            return null;
        }

        private bool CanRunningForLoad(bool isFreeRun)
        {
            try
            {
                //bool isGetresultMap = false;

                bool isPassWaferStatus = Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.NOT_EXIST;
                bool isPassWaferState = true;

                bool isMovingState = Module.SequenceEngineManager().isMovingState();
                bool isInit = Module.MonitoringManager().IsMachineInitDone;
                bool isGetRunstate = Module.SequenceEngineManager().GetRunState(false,isWaferTransfer:true);
                bool iswtrState = Module.WaferTransferModule().ModuleState.GetState() == ModuleStateEnum.PENDING;
                // LoggerManager.Debug("CanRunningForLoad : WaferStatus" + Module.StageSupervisor().WaferObject.GetStatus() + ", Module.SequenceEngineManager().GetRunState(): " + isGetRunstate);

                return
                    isPassWaferStatus &&
                    isPassWaferState &&
                    isInit&&
                    (isGetRunstate || iswtrState);// &&
                                                  //isGetresultMap;
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
                if (Module.StageSupervisor().StageModuleState.IsHandlerholdWafer())
                {
                    isPassWaferStatus = true;
                }
                bool isPassWaferState;
                bool isMovingState = Module.SequenceEngineManager().isMovingState();
                bool isInit = Module.MonitoringManager().IsMachineInitDone;
                bool isGetRunstate = Module.SequenceEngineManager().GetRunState(false, isWaferTransfer: true);
                if (isFreeRun)
                {
                    isPassWaferState = true;
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
                //   LoggerManager.Debug("CanRunningForUnload : WaferStatus" + Module.StageSupervisor().WaferObject.GetStatus() + ",WaferState:"+ isPassWaferState + " , Module.SequenceEngineManager().GetRunState(): " + isGetRunstate);
                retVal =
                    isPassWaferStatus &&
                    isPassWaferState &&
                   isGetRunstate &&
                   isInit;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private bool CanRunningForRecoveryUnload()
        {
            bool retVal = false;

            try
            {
                if (Module.WaferTransferModule().NeedToRecovery && Module.WaferTransferModule().ModuleState.GetState() == ModuleStateEnum.ERROR)
                {
                    retVal = true;
                }
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
                    if (CanRunningForLoad(false)) //원래는 True TODO  : false=> true;
                    {
                        // 메뉴얼 동작 시, 동작하지 않음.
                        // 셀의 Mode가 Maintenance인 경우 

                        if (Module.StageSupervisor().StageMode != GPCellModeEnum.MAINTENANCE)
                        {
                            if (Module.ResultMapManager().NeedDownload() == true)
                            {
                                retVal = Module.ResultMapManager().ConvertResultMapToProberDataAfterReadyToLoadWafer();

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    // Module.SetAbort(true, false);

                                    LoggerManager.Error("[GPLoaderController.SUSPENDED], Execute() : ConvertResultMapToProberDataAfterReadyToLoadWafer failed.");
                                }
                            }
                        }

                        retVal = Module.GPLoaderService.AwakeProcessModule();

                        if (retVal == EventCodeEnum.NONE)
                        {
                            bool isInjected = Module.CommandManager().SetCommand<IChuckLoadCommand>(Module);
                            int retryCnt = 0;
                            bool transferRunningState = false;

                            while (!transferRunningState)
                            {
                                var loaderState = Module.WaferTransferModule().ModuleState.GetState();
                                if (loaderState == ModuleStateEnum.RUNNING)
                                {
                                    transferRunningState = true;
                                }

                                if(transferRunningState == false)
                                {
                                    if (retryCnt > 10 && !isInjected)
                                    {
                                        // interval 10번 돌았고(1500ms), 마지막 보낸 커맨드도 Inject이 안되었다면 다시한번 보냄.
                                        LoggerManager.Debug("IChuckLoadCommand Retry Send Command");
                                        isInjected = Module.CommandManager().SetCommand<IChuckLoadCommand>(Module);
                                        retryCnt = 0;
                                    }
                                    else
                                    {
                                        retryCnt++;
                                    }
                                }
                                System.Threading.Thread.Sleep(retryInterval);
                            }
                            StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);

                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderControllerSuspendedState.FreeRun() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                        }
                    }
                }
                else if (reasonOfSuspended == ReasonOfSuspendedEnum.UNLOAD)
                {
                    if (CanRunningForUnload(true))
                    {
                        retVal = Module.GPLoaderService.AwakeProcessModule();

                        if (retVal == EventCodeEnum.NONE)
                        {
                            WaitForLoaderState(ModuleStateEnum.RUNNING);

                            bool isInjected = Module.CommandManager().SetCommand<IChuckUnloadCommand>(Module);
                            int retryCnt = 0;
                            bool transferRunningState = false;

                            while (!transferRunningState)
                            {
                                var loaderState = Module.WaferTransferModule().ModuleState.GetState();
                                if (loaderState == ModuleStateEnum.RUNNING)
                                {
                                    transferRunningState = true;
                                }

                                if(transferRunningState == false)
                                {
                                    if (retryCnt > 10 && !isInjected)
                                    {
                                        LoggerManager.Debug("IChuckUnloadCommand Retry Send Command");
                                        isInjected = Module.CommandManager().SetCommand<IChuckUnloadCommand>(Module);
                                        retryCnt = 0;
                                    }
                                    else
                                    {
                                        retryCnt++;
                                    }
                                }
                                System.Threading.Thread.Sleep(retryInterval);
                            }

                            StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);

                            // Wafer가 Unload 될때 Soaking에 의한 Unload(SOAKINGSUSPEND 상태)이면 UnProcessed 상태로 변경한다.
                            if (Module.GetParam_Wafer().GetState() == EnumWaferState.SOAKINGSUSPEND)
                            {
                                Module.GetParam_Wafer().SetWaferState(EnumWaferState.UNPROCESSED);
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderControllerSuspendedState.FreeRun() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                        }
                    }
                    else if (CanRunningForRecoveryUnload())
                    {
                        retVal = Module.GPLoaderService.AwakeProcessModule();
                        if (retVal == EventCodeEnum.NONE)
                        {
                            WaitForLoaderState(ModuleStateEnum.RUNNING);

                            bool isInjected = Module.CommandManager().SetCommand<IChuckUnloadCommand>(Module);
                            int retryCnt = 0;
                            bool transferRunningState = false;

                            while (!transferRunningState)
                            {
                                var loaderState = Module.WaferTransferModule().ModuleState.GetState();
                                if (loaderState == ModuleStateEnum.RUNNING)
                                {
                                    transferRunningState = true;
                                }

                                if(transferRunningState == false)
                                {
                                    if (retryCnt > 10 && !isInjected)
                                    {
                                        LoggerManager.Debug("IChuckUnloadCommand Retry Send Command");
                                        isInjected = Module.CommandManager().SetCommand<IChuckUnloadCommand>(Module);
                                        retryCnt = 0;
                                    }
                                    else
                                    {
                                        retryCnt++;
                                    }
                                }
                                
                                System.Threading.Thread.Sleep(retryInterval);
                            }
                            StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);

                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderControllerSuspendedState.FreeRun() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                        }
                    }
                }
                else if (reasonOfSuspended == ReasonOfSuspendedEnum.CARD_LOAD)
                {
                    if (Module.SequenceEngineManager().isMovingState())
                    {
                        retVal = Module.GPLoaderService.AwakeProcessModule();
                        if (retVal == EventCodeEnum.NONE)
                        {
                            bool isInjected = Module.CommandManager().SetCommand<ICardLoadCommand>(Module);
                            bool transferRunningState = false;
                            while (!transferRunningState)
                            {
                                var loaderState = Module.WaferTransferModule().ModuleState.GetState();

                                // ISSD-4480 이슈 - Randy, Brett
                                // CardLoadCommand를 날리고 WaferTransferModule이 러닝이 되길 기다리는데,
                                // 이 때 WaferTransferModule은 CardID가 없는 경우 바로 Error로 빠지게 되고
                                // Error -> Done -> Idle로 State가 전환되는데 
                                // Thread.Sleep 주기로 인하여 RUNNING State를 포착하지 못하고 Loop에 계속 빠져있게 되는 문제가 있다.
                                // 이 경우를 탈출 시켜주기 위해 CardID를 잘못 입력하여 Error가 난 경우 TransferBrake 변수를 true로 해주어 빠져나갈 수 있게 하였다.

                                bool transferBrake = Module.WaferTransferModule().TransferBrake;

                                if (loaderState == ModuleStateEnum.RUNNING || transferBrake)
                                {
                                    transferRunningState = true;
                                }

                                if(transferRunningState == false)
                                {
                                    if (!isInjected)
                                    {
                                        // SetCommand Retry
                                    }
                                }
                                System.Threading.Thread.Sleep(retryInterval);
                            }
                            StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);
                            // 플래그 초기화.
                            Module.WaferTransferModule().TransferBrake = false;
                        }
                        else
                        {
                            LoggerManager.Error($"LoaderControllerSuspendedState.FreeRun() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                        }
                    }
                }
                else if (reasonOfSuspended == ReasonOfSuspendedEnum.CARD_UNLOAD)
                {
                    if (Module.SequenceEngineManager().isMovingState())
                    {
                        retVal = Module.GPLoaderService.AwakeProcessModule();
                        if (retVal == EventCodeEnum.NONE)
                        {
                            WaitForLoaderState(ModuleStateEnum.RUNNING);

                            bool isInjected = Module.CommandManager().SetCommand<ICardUnloadCommand>(Module);
                            bool transferRunningState = false;
                            while (!transferRunningState)
                            {
                                var loaderState = Module.WaferTransferModule().ModuleState.GetState();
                                if (loaderState == ModuleStateEnum.RUNNING)
                                {
                                    transferRunningState = true;
                                }

                                if (transferRunningState == false)
                                {
                                    if (!isInjected)
                                    {
                                        // SetCommand Retry
                                    }
                                }
                                System.Threading.Thread.Sleep(retryInterval);
                            }
                            StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);
                        }
                        else
                        {
                            LoggerManager.Error($"LoaderControllerSuspendedState.FreeRun() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                        }
                    }
                }
                else if (reasonOfSuspended == ReasonOfSuspendedEnum.NONE)
                {
                    LoggerManager.Debug($"LoaderControllerSuspendedState.FreeRun() : reasonOfSuspended Logic Error:{ReasonOfSuspendedEnum.NONE}");
                    StateTransition(LoaderControllerStateEnum.IDLE);
                }
                else
                {
                    retVal = Module.GPLoaderService.AwakeProcessModule();
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
                    if (Module.CommandRecvSlot.GetState() == CommandStateEnum.REQUESTED)
                    {
                        retVal = Module.GPLoaderService.AbortRequest();
                        if (retVal == EventCodeEnum.NONE)
                        {
                            StateTransition(LoaderControllerStateEnum.ABORT);
                        }
                    }
                    else
                    {
                        var reasonOfSuspended = Module.LoaderInfo.ModuleInfo.ReasonOfSuspended;
                        if (reasonOfSuspended == ReasonOfSuspendedEnum.LOAD)
                        {
                            if (CanRunningForLoad(false))
                            {
                                if (Module.StageSupervisor().StageMode != GPCellModeEnum.MAINTENANCE)
                                {
                                    if (Module.ResultMapManager().NeedDownload() == true)
                                    {
                                        retVal = Module.ResultMapManager().ConvertResultMapToProberDataAfterReadyToLoadWafer();

                                        if (retVal != EventCodeEnum.NONE)
                                        {
                                            Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SKIPPED);

                                            LoggerManager.Error("[GPLoaderController.SUSPENDED], Execute() : ConvertResultMapToProberDataAfterReadyToLoadWafer failed.");
                                        }
                                    }
                                }

                                var wtfModule = Module.WaferTransferModule();
                                var lotModuleInnerState = Module.LotOPModule().InnerState as LotOPState;

                                if ((wtfModule.ModuleState.GetState() == ModuleStateEnum.IDLE
                                    || wtfModule.ModuleState.GetState() == ModuleStateEnum.DONE
                                    || wtfModule.ModuleState.GetState() == ModuleStateEnum.PENDING)
                                    && lotModuleInnerState.GetState() != LotOPStateEnum.PAUSING)
                                {

                                    retVal = Module.GPLoaderService.AwakeProcessModule();
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        WaitForLoaderState(ModuleStateEnum.RUNNING);
                                        bool isInjected = Module.CommandManager().SetCommand<IChuckLoadCommand>(Module);
                                        int retryCnt = 0;
                                        if (isInjected)
                                        {
                                            while (isInjected)
                                            {
                                                var wtfstate = wtfModule.ModuleState.GetState();
                                                if (wtfstate != ModuleStateEnum.IDLE)
                                                {
                                                    StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);
                                                    break;
                                                }
                                                else if (retryCnt > 10)
                                                {
                                                    LoggerManager.Debug("IChuckLoadCommand Retry Send Command");
                                                    isInjected = Module.CommandManager().SetCommand<IChuckLoadCommand>(Module);
                                                    retryCnt = 0;
                                                }
                                                else
                                                {
                                                    retryCnt++;
                                                }

                                                System.Threading.Thread.Sleep(retryInterval);
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug("GPLoaderController Load: IChuckLoadCommand REJECTED");
                                        }


                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"LoaderControllerSuspendedState.Run() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug("GPLoaderController Load:  WaferTransferModuleState" + wtfModule.ModuleState.GetState() + ", LotModuleState: " + lotModuleInnerState.GetState());
                                }

                            }
                        }
                        else if (reasonOfSuspended == ReasonOfSuspendedEnum.UNLOAD)
                        {
                            if (CanRunningForUnload(false))
                            {
                                var wtfModule = Module.WaferTransferModule();

                                if (wtfModule.ModuleState.GetState() == ModuleStateEnum.IDLE
                                || wtfModule.ModuleState.GetState() == ModuleStateEnum.DONE)
                                {
                                    retVal = Module.GPLoaderService.AwakeProcessModule();
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        WaitForLoaderState(ModuleStateEnum.RUNNING);

                                        bool isInjected = Module.CommandManager().SetCommand<IChuckUnloadCommand>(Module);

                                        if (isInjected)
                                        {
                                            int retryCnt = 0;
                                            while (isInjected)
                                            {
                                                //Module._delays.DelayFor(500);
                                                System.Threading.Thread.Sleep(500);

                                                var wtfstate = wtfModule.ModuleState.GetState();
                                                if (wtfstate != ModuleStateEnum.IDLE)
                                                {
                                                    StateTransition(LoaderControllerStateEnum.WAFER_TRANSFERING);
                                                    break;
                                                }
                                                else if (retryCnt > 10)
                                                {
                                                    LoggerManager.Debug("IChuckUnloadCommand Retry Send Command");
                                                    isInjected = Module.CommandManager().SetCommand<IChuckUnloadCommand>(Module);
                                                    retryCnt = 0;
                                                }
                                                else
                                                {
                                                    retryCnt++;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug("GPLoaderController UnLoad: IChuckUnloadCommand REJECTED");
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
                                        LoggerManager.Debug($"LoaderControllerSuspendedState.Run() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug("GPLoaderController UNLoad : WaferTransferModuleState" + wtfModule.ModuleState.GetState());
                                }
                            }

                        }
                        else
                        {
                            retVal = Module.GPLoaderService.AwakeProcessModule();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                WaitForLoaderState(ModuleStateEnum.RUNNING);
                                StateTransition(LoaderControllerStateEnum.MONITORING);
                            }
                            else
                            {
                                LoggerManager.Debug($"LoaderControllerSuspendedState.Run() : LoaderAwakeCommand logic error. ({reasonOfSuspended})");
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

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.SUSPENDED;
        }
    }

    public class WAFER_TRANSFERING : GP_LoaderControllerStateBase
    {
        public WAFER_TRANSFERING(GP_LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.WAFER_TRANSFERING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                var WaferTransferModuleState = Module.WaferTransferModule().ModuleState.GetState();

                if (WaferTransferModuleState == ModuleStateEnum.DONE || WaferTransferModuleState == ModuleStateEnum.IDLE || WaferTransferModuleState == ModuleStateEnum.PENDING)
                {
                    StateTransition(LoaderControllerStateEnum.MONITORING);
                    retVal = EventCodeEnum.NONE;
                }
                else if (WaferTransferModuleState == ModuleStateEnum.ERROR)
                {
                    StateTransition(LoaderControllerStateEnum.ERROR);
                    retVal = EventCodeEnum.NONE;
                }
                else if (WaferTransferModuleState == ModuleStateEnum.PAUSED)
                {
                    StateTransition(LoaderControllerStateEnum.IDLE);
                    retVal = EventCodeEnum.NONE;
                    //   Pause();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override LoaderMap RequestJob(out bool isExchange, out bool isNeedWafer, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            isExchange = false;
            isNeedWafer = false;
            cstHashCodeOfRequestLot = "";
            return null;
        }


        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PENDING;
        }

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.WAFER_TRANSFERING;
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

    public class PAUSED : GP_LoaderControllerStateBase
    {
        public PAUSED(GP_LoaderController module) : base(module)
        {
            //if (module.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
            //{
            //    module.ViewModelManager().BackPreScreenTransition(true);
            //}
        }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.PAUSED;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.PAUSED;

        public override EventCodeEnum Execute()
        {
            var currInfo = Module.LoaderInfo;
            var WaferTransferModuleState = Module.WaferTransferModule().ModuleState.GetState();
            //StateTransition(LoaderControllerStateEnum.IDLE);
            if (currInfo != null)
            {
                switch (currInfo.ModuleInfo.ModuleState)
                {

                    case ModuleStateEnum.SUSPENDED:
                        if (currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.LOAD || currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.UNLOAD ||
                            currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.CARD_LOAD || currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.CARD_UNLOAD)

                        {
                            if (currInfo.ModuleInfo.ChuckNumber == Module.ChuckID.Index)
                            {
                                StateTransition(LoaderControllerStateEnum.SUSPENDED);
                            }
                        }
                        break;

                }
                if (WaferTransferModuleState == ModuleStateEnum.IDLE)
                {
                    StateTransition(LoaderControllerStateEnum.IDLE);
                }
            }

            return EventCodeEnum.NONE;
        }

        public override LoaderMap RequestJob(out bool isExchange, out bool isNeedWafer, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            isExchange = false;
            isNeedWafer = false;
            cstHashCodeOfRequestLot = "";

            try
            {
                if (Module.IsAbort || canloadwafer == false)
                {
                    if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                    {
                        return UnloadRequestJob();
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (Module.LotOPModule().ErrorEndState == ErrorEndStateEnum.Unload)
                {
                    if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                    {
                        return UnloadJob();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {



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

                if (currInfo != null)
                {
                    switch (currInfo.ModuleInfo.ModuleState)
                    {
                        case ModuleStateEnum.SUSPENDED:
                            retVal = Module.GPLoaderService.AbortRequest();
                            break;
                        case ModuleStateEnum.DONE:
                            retVal = Module.GPLoaderService.ClearRequestData();
                            break;
                            //case ModuleStateEnum.PAUSED:
                            //    retVal=Module.
                    }
                }

                if (Module.GPLoaderService != null)
                {
                    if (retVal == EventCodeEnum.NONE && Module.GPLoaderService.GetLoaderInfo().ModuleInfo.ModuleState == ModuleStateEnum.ABORT)
                    {
                        retVal = Module.GPLoaderService.ClearRequestData();
                    }

                    if (retVal == EventCodeEnum.NONE
                        || Module.GPLoaderService.GetLoaderInfo().ModuleInfo.ModuleState == ModuleStateEnum.IDLE)
                    {
                        StateTransition(LoaderControllerStateEnum.ABORT);
                    }
                    else
                    {
                        RaiseInvalidState();//Error
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
            return ModuleStateEnum.PAUSED;
        }

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.PAUSED;
        }
    }

    public class ABORT : GP_LoaderControllerStateBase
    {
        public ABORT(GP_LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.ABORT;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.ABORT;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.GPLoaderService.ClearRequestData();
                if (retVal == EventCodeEnum.NONE
                    || Module.GPLoaderService.GetLoaderInfo().ModuleInfo.ModuleState == ModuleStateEnum.IDLE)
                {
                    Module.LotEndCassetteFlag = true;
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

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.ABORT;
        }
    }

    public class UNLOADALL : GP_LoaderControllerStateBase
    {
        public UNLOADALL(GP_LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.ABORT;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.ABORT;

        public override EventCodeEnum Execute()
        {
            TransferObject unloadSub = Module.ReqMap.GetTransferObjectAll().Where(
                   item =>
                   item.CurrHolder == Module.ChuckID
                   ).FirstOrDefault();
            unloadSub.WaferState = Module.StageSupervisor().WaferObject.GetState();
            unloadSub.SetReservationState(ReservationStateEnum.RESERVATION);
            LoaderMapEditor editor = new LoaderMapEditor(Module.ReqMap);

            Module.GEMModule().GetPIVContainer().StageNumber.Value = Module.LoaderController().GetChuckIndex();
            Module.GEMModule().GetPIVContainer().StageState.Value = 0;
            Module.GEMModule().GetPIVContainer().SlotNumber.Value = Module.StageSupervisor().WaferObject.GetSubsInfo().SlotIndex.Value;
            Module.GEMModule().GetPIVContainer().CurTemperature.Value = Module.TempController().TempInfo.CurTemp.Value;
            
            bool isExistUnloadHolder = true;
            var unloadHolder = unloadSub.OriginHolder;

            if (Module.LoaderInfo.FoupShiftMode == 1)
            {
                (isExistUnloadHolder, unloadHolder) = Module.ReqMap.IsUnloaderHolderExist(unloadSub);
            }

            bool isInjected = false;

            if (isExistUnloadHolder)
            {
                editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadHolder);

                LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                cmdParam.Editor = editor;
                isInjected = Module.CommandManager().SetCommand<ILoaderMapCommand>(this, cmdParam);
            }

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
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

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.UNLOADALL;
        }
    }
    public class DONE : GP_LoaderControllerStateBase
    {
        public DONE(GP_LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.DONE;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.DONE;

        private EventCodeEnum FreeRun()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.GPLoaderService.ClearRequestData();
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
                    StateTransition(LoaderControllerStateEnum.IDLE);
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

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.DONE;
        }
    }

    public class ERROR : GP_LoaderControllerStateBase
    {
        public ERROR(GP_LoaderController module) : base(module)
        {
            try
            {
                this.Module.MotionManager().LoaderEMGStop();

                //if (module.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                //{
                //    module.ViewModelManager().BackPreScreenTransition(true);
                //}
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

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.ERROR;
        }
    }

    public class RECOVERY : GP_LoaderControllerStateBase
    {
        public RECOVERY(GP_LoaderController module) : base(module) { }

        public override ModuleStateEnum ModuleState => ModuleStateEnum.RECOVERY;

        public override LoaderControllerStateEnum State => LoaderControllerStateEnum.RECOVERY;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                var currInfo = Module.LoaderInfo;
                if (currInfo != null)
                {
                    switch (currInfo.ModuleInfo.ModuleState)
                    {

                        case ModuleStateEnum.SUSPENDED:
                            if (currInfo.ModuleInfo.ReasonOfSuspended == ReasonOfSuspendedEnum.UNLOAD)

                            {
                                if (currInfo.ModuleInfo.ChuckNumber == Module.ChuckID.Index)
                                {
                                    StateTransition(LoaderControllerStateEnum.SUSPENDED);
                                }
                            }
                            break;
                        default:
                            break;

                    }
                }
                if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.NOT_EXIST ||
                    (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST && Module.GetParam_Wafer().GetState() == EnumWaferState.SKIPPED))
                {
                    StateTransition(LoaderControllerStateEnum.IDLE);
                }
            }
            catch (Exception err)
            {
                StateTransition(LoaderControllerStateEnum.ERROR);
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        private bool CanRunningForUnload(bool isFreeRun)
        {
            bool retVal = false;

            try
            {
                bool isPassWaferStatus = Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST;
                bool isGetRunstate = Module.LotOPModule().RunList.All(
                        item =>
                        !(item is IWaferTransferModule) &&
                        item.ModuleState.GetState() != ModuleStateEnum.RUNNING &&
                        item.ModuleState.GetState() != ModuleStateEnum.PENDING &&
                        item.ModuleState.GetState() != ModuleStateEnum.ERROR
                        //&&item.ModuleState.GetState() != ModuleStateEnum.RECOVERY
                        );

                //   LoggerManager.Debug("CanRunningForUnload : WaferStatus" + Module.StageSupervisor().WaferObject.GetStatus() + ",WaferState:"+ isPassWaferState + " , Module.SequenceEngineManager().GetRunState(): " + isGetRunstate);
                retVal = isPassWaferStatus && isGetRunstate;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
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

                    ResponseResult rr = Module.GPLoaderService.SetRequest(mapEditor.EditMap);

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

        public override LoaderControllerStateEnum GetState()
        {
            return LoaderControllerStateEnum.RECOVERY;
        }
    }
}

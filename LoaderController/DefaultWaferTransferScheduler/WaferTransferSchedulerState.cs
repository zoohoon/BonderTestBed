using System;
using System.Linq;

using ProberInterfaces;
using ProberInterfaces.Foup;
using LoaderControllerBase;
using LoaderParameters;
using LogModule;
using ProberInterfaces.Command.Internal;

namespace LoaderController.WaferTransferSchedulerStates
{
    public abstract class WaferTransferSchedulerStateBase
    {
        public WaferTransferScheduler Module { get; set; }

        public WaferTransferSchedulerStateBase(WaferTransferScheduler module)
        {
            try
            {
                Module = module;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected LoaderController LoaderController => Module.LoaderController;

        protected LoaderMap StateMap => LoaderController.LoaderInfo.StateMap;

        public void StateTransition(WaferTransferSchedulerStateBase stateObj)
        {
            try
            {
                Module.StateObj = stateObj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool UpdateState(out bool isNeedWafer, bool canloadwafer = true)
        {
            isNeedWafer = false;
            try
            {
                var cassette = StateMap.CassetteModules.FirstOrDefault();

                if (cassette != null)
                {
                    //var foup = Module.FoupController.GetFoupModuleInfo(cassette.ID.Index);

                    var foupController = Module.FoupOpModule().GetFoupController(cassette.ID.Index);
                    var foupInfo = foupController.FoupModuleInfo;

                    if (foupInfo.State == FoupStateEnum.LOAD)
                    {
                        if (cassette.ScanState != CassetteScanStateEnum.READ)
                        {
                            if (Module.LoaderController.GetLotEndFlag())
                            {
                                StateTransition(new CASSETTE_DONE(Module));
                            }
                            else
                            {

                                StateTransition(new NO_READ(Module));
                            }
                            Module.IsCstStopOption = false;
                        }
                        else
                        {

                            if (LoaderController.ModuleState.GetState() != ModuleStateEnum.DONE && (!(this.Module.LotOPModule().LotInfo.StopAfterScanCSTFlag)) &&
                                (this.Module.LotOPModule().LotDeviceParam.StopOption.StopAfterScanCassette.Value ||
                                this.Module.LotOPModule().LotDeviceParam.OperatorStopOption.StopAfterScanCassette.Value))
                            {

                                if (!Module.IsCstStopOption)
                                {
                                    //this.Module.LotOPModule().LotInfo.StopAfterScanCSTFlag = true;
                                    this.Module.LotOPModule().ReasonOfStopOption.IsStop = true;
                                    this.Module.LotOPModule().ReasonOfStopOption.Reason = StopOptionEnum.STOP_AFTER_CASSETTE;
                                    StateTransition(new NO_CASSETTE(Module));
                                    this.Module.CommandManager().SetCommand<ILotOpPause>(this);
                                }
                                Module.IsCstStopOption = true;

                            }

                            else
                            {
                                Module.IsCstStopOption = false;
                                var loadWafer = FindLoadWafer();

                                var ChuckModule = StateMap.ChuckModules.Where(item => item.ID == LoaderController.ChuckID).FirstOrDefault();
                                var currentMap = StateMap;
                                bool isUnloadWafer = false;
                                foreach (var armModule in currentMap.ARMModules)
                                {
                                    if (armModule.WaferStatus == EnumSubsStatus.EXIST)
                                    {
                                        isUnloadWafer = true;
                                    }
                                }
                                foreach (var preModule in currentMap.PreAlignModules)
                                {
                                    if (preModule.WaferStatus == EnumSubsStatus.EXIST)
                                    {
                                        isUnloadWafer = true;
                                    }
                                }
                                foreach (var chuckModule in currentMap.ChuckModules)
                                {
                                    if (chuckModule.WaferStatus == EnumSubsStatus.EXIST)
                                    {
                                        isUnloadWafer = true;
                                    }
                                }

                                if (ChuckModule.WaferStatus == EnumSubsStatus.EXIST)
                                {
                                    if (loadWafer == null)
                                    {
                                        this.Module.LotOPModule().IsLastWafer = true;
                                        StateTransition(new LAST_WAFER_ON_CHUCK_ONLY(Module));
                                    }
                                    else
                                    {
                                        StateTransition(new WAFER_ON_CHUCK(Module));
                                    }
                                }
                                else
                                {
                                    if (loadWafer == null && !isUnloadWafer)
                                    {
                                        StateTransition(new CASSETTE_DONE(Module));
                                    }
                                    else
                                    {
                                        StateTransition(new NO_WAFER_ON_CHUCK(Module));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        StateTransition(new NO_CASSETTE(Module));
                    }
                }
                else
                {
                    StateTransition(new NO_CASSETTE(Module));
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return false;
        }

        public abstract WaferTransferSchedulerStateEnum State { get; }

        public abstract WaferTransferScheduleResult Execute();

        protected bool CanReqToScanCassette()
        {
            return true;
        }

        public abstract bool CanReqToNextWafer();


        protected TransferObject FindUnloadWafer()
        {
            TransferObject retVal = null;

            try
            {
                retVal = StateMap.GetTransferObjectAll().Where(
                    item =>
                    item.CurrHolder == LoaderController.ChuckID
                    ).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        protected TransferObject FindLoadWafer()
        {
            TransferObject loadWafer = null;

            try
            {
                if (!this.LoaderController.GetLotEndFlag() && this.LoaderController.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    bool isNeedPolish = false;
                    if (isNeedPolish)
                    {
                        var loadablePolishWafers = StateMap.GetTransferObjectAll().Where(
                      item => item.WaferType.Value == EnumWaferType.POLISH).ToList();
                        loadWafer = loadablePolishWafers.OrderBy(item => item.OriginHolder.Index).FirstOrDefault();
                    }
                    else
                    {

                        var allWafers = StateMap.GetTransferObjectAll();
                        var loadableWafers = StateMap.GetTransferObjectAll().Where(
                                item =>
                                item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                                item.WaferType.Value == EnumWaferType.STANDARD &&
                                item.WaferState == EnumWaferState.UNPROCESSED).ToList();


                        for (int i = 0; i < loadableWafers.Count; i++)
                        {
                            var wafer = loadableWafers[i];
                            if (!this.LoaderController.LotOPModule().LotInfo.IsLoadingWafer(wafer.OriginHolder.Index))
                            {
                                loadableWafers.Remove(wafer);
                                i--;
                            }
                        }
                        //if (Module.LoaderController.Stage.Service.GetSlotOrderBy() == SlotOrderByEnum.ASCENDING)
                        //{
                        loadWafer = loadableWafers.OrderBy(item => item.OriginHolder.Index).FirstOrDefault();
                    }
                }
                else
                {

                }
                //}
                //else
                //{
                // loadWafer = loadableWafers.OrderByDescending(item => item.OriginHolder.Index).FirstOrDefault();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return loadWafer;
        }
    }

    public class NO_CASSETTE : WaferTransferSchedulerStateBase
    {
        public NO_CASSETTE(WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.NO_CASSETTE;

        public override bool CanReqToNextWafer()
        {
            return false;
        }

        public override WaferTransferScheduleResult Execute()
        {
            return WaferTransferScheduleResult.NOT_NEED;
        }

    }

    public class NO_READ : WaferTransferSchedulerStateBase
    {
        public NO_READ(WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.NO_READ;

        public override bool CanReqToNextWafer()
        {

            return false;
        }

        public override WaferTransferScheduleResult Execute()
        {
            WaferTransferScheduleResult rel = null;

            try
            {
                if (CanReqToScanCassette())
                {
                    var cassette = StateMap.CassetteModules.FirstOrDefault();

                    LoaderMapEditor editor = new LoaderMapEditor(StateMap);
                    editor.EditorState.SetScanCassette(cassette.ID);

                    rel = WaferTransferScheduleResult.CreateNeed(editor);
                }
                else
                {
                    rel = WaferTransferScheduleResult.NOT_NEED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
    }

    public class NO_WAFER_ON_CHUCK : WaferTransferSchedulerStateBase
    {
        public NO_WAFER_ON_CHUCK(WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.NO_WAFER_ON_CHUCK;

        public override bool CanReqToNextWafer()
        {
            bool CanReqToNext = false;
            try
            {
                var chuckWaferStatus = Module.StageSupervisor().WaferObject.GetStatus();
                var chuckWaferState = Module.StageSupervisor().WaferObject.GetState();

                if (chuckWaferStatus == EnumSubsStatus.NOT_EXIST)
                {
                    CanReqToNext = true;
                }
                else if (chuckWaferStatus == EnumSubsStatus.EXIST)
                {
                    CanReqToNext =
                        chuckWaferState == EnumWaferState.PROBING ||
                        chuckWaferState == EnumWaferState.PROCESSED ||
                        chuckWaferState == EnumWaferState.TESTED ||
                        chuckWaferState == EnumWaferState.SKIPPED ||
                        chuckWaferState == EnumWaferState.SOAKINGSUSPEND ||
                        chuckWaferState == EnumWaferState.SOAKINGDONE
                        ;
                }
                else
                {
                    CanReqToNext = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return CanReqToNext;
        }

        public override WaferTransferScheduleResult Execute()
        {
            WaferTransferScheduleResult rel = null;

            try
            {
                if (CanReqToNextWafer())
                {
                    TransferObject loadSub = FindLoadWafer();

                    if (loadSub != null)
                    {
                        LoaderMapEditor editor = new LoaderMapEditor(StateMap);

                        var LoadNotchAngleOptionVar = new LoadNotchAngleOption();
                        LoadNotchAngleOptionVar.IsEnable.Value = false;


                        editor.EditorState.SetTransfer(loadSub.ID.Value, LoaderController.ChuckID, LoadNotchAngleOptionVar);
                        //editor.EditorState.SetTransfer(loadSub.ID, LoaderController.ChuckID, new LoadNotchAngleOption() { IsEnable = false });

                        rel = WaferTransferScheduleResult.CreateNeed(editor);
                    }
                    else
                    {
                        rel = WaferTransferScheduleResult.NOT_NEED;
                    }
                }
                else
                {
                    rel = WaferTransferScheduleResult.NOT_NEED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return rel;
        }
    }

    public class WAFER_ON_CHUCK : WaferTransferSchedulerStateBase
    {
        public WAFER_ON_CHUCK(WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.WAFER_ON_CHUCK;

        public override bool CanReqToNextWafer()
        {
            bool CanReqToNext = false;
            try
            {
                var chuckWaferStatus = Module.StageSupervisor().WaferObject.GetStatus();
                var chuckWaferState = Module.StageSupervisor().WaferObject.GetState();

                if (chuckWaferStatus == EnumSubsStatus.NOT_EXIST)
                {
                    CanReqToNext = true;
                }
                else if (chuckWaferStatus == EnumSubsStatus.EXIST)
                {
                    CanReqToNext =
                        chuckWaferState == EnumWaferState.PROBING ||
                        chuckWaferState == EnumWaferState.PROCESSED ||
                        chuckWaferState == EnumWaferState.TESTED ||
                        chuckWaferState == EnumWaferState.SKIPPED ||
                        chuckWaferState == EnumWaferState.UNPROCESSED ||
                        chuckWaferState == EnumWaferState.SOAKINGSUSPEND ||
                        chuckWaferState == EnumWaferState.SOAKINGDONE
                        ;
                }
                else
                {
                    CanReqToNext = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return CanReqToNext;
        }
        public override WaferTransferScheduleResult Execute()
        {
            WaferTransferScheduleResult rel = null;

            try
            {
                if (CanReqToNextWafer())
                {
                    TransferObject unloadSub = FindUnloadWafer();
                    TransferObject loadSub = FindLoadWafer();

                    LoaderMapEditor editor = new LoaderMapEditor(StateMap);
                    editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.OriginHolder);
                    if (loadSub != null)
                    {
                        editor.EditorState.SetTransfer(loadSub.ID.Value, LoaderController.ChuckID);
                    }
                    rel = WaferTransferScheduleResult.CreateNeed(editor);
                }
                else
                {
                    rel = WaferTransferScheduleResult.NOT_NEED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
    }

    public class LAST_WAFER_ON_CHUCK_ONLY : WaferTransferSchedulerStateBase
    {
        public LAST_WAFER_ON_CHUCK_ONLY(WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.LAST_WAFER_ON_CHUCK_ONLY;

        public override WaferTransferScheduleResult Execute()
        {
            WaferTransferScheduleResult rel = null;

            try
            {
                if (CanReqToNextWafer())
                {
                    TransferObject unloadSub = FindUnloadWafer();

                    LoaderMapEditor editor = new LoaderMapEditor(StateMap);
                    editor.EditorState.SetTransfer(unloadSub.ID.Value, unloadSub.OriginHolder);

                    rel = WaferTransferScheduleResult.CreateNeed(editor);
                }
                else
                {
                    rel = WaferTransferScheduleResult.NOT_NEED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return rel;
        }
        public override bool CanReqToNextWafer()
        {
            bool CanReqToNext = false;
            try
            {
                var chuckWaferStatus = Module.StageSupervisor().WaferObject.GetStatus();
                var chuckWaferState = Module.StageSupervisor().WaferObject.GetState();

                if (chuckWaferStatus == EnumSubsStatus.NOT_EXIST)
                {
                    CanReqToNext = true;
                }
                else if (chuckWaferStatus == EnumSubsStatus.EXIST)
                {
                    CanReqToNext =
                        chuckWaferState == EnumWaferState.PROBING ||
                        chuckWaferState == EnumWaferState.PROCESSED ||
                        chuckWaferState == EnumWaferState.TESTED ||
                        chuckWaferState == EnumWaferState.SKIPPED ||
                        chuckWaferState == EnumWaferState.SOAKINGSUSPEND ||
                        chuckWaferState == EnumWaferState.SOAKINGDONE
                        ;
                }
                else
                {
                    CanReqToNext = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return CanReqToNext;
        }
    }

    public class CASSETTE_DONE : WaferTransferSchedulerStateBase
    {
        public CASSETTE_DONE(WaferTransferScheduler module) : base(module) { }

        public override WaferTransferSchedulerStateEnum State => WaferTransferSchedulerStateEnum.CASSETTE_DONE;

        public override WaferTransferScheduleResult Execute()
        {
            return WaferTransferScheduleResult.NOT_NEED;
        }
        public override bool CanReqToNextWafer()
        {

            return false;
        }
    }
}

using System;
using System.Threading.Tasks;
using ProberInterfaces.WaferTransfer;
using ProberInterfaces;
using ProberErrorCode;
using LoaderControllerBase;
using NotifyEventModule;
using LogModule;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.ResultMap;
using System.Threading;
using ProberInterfaces.Event;

namespace WaferTransfer.WaferLoadProcStates
{
    public abstract class WaferLoadProcState
    {
        public WaferLoadProcModule Module { get; set; }

        public WaferLoadProcState(WaferLoadProcModule module)
        {
            this.Module = module;
        }

        public void StateTransition(WaferLoadProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }

        internal void SetDoneState()
        {
            StateTransition(new DoneState(Module));
            //ISubstrateInfo waferObject = Module.StageSupervisor().WaferObject.GetSubsInfo();
            //int slotnum = (waferObject.SlotIndex.Value % 25 == 0) ? 25 : waferObject.SlotIndex.Value % 25;

            //PIVInfo pivinfo = new PIVInfo(waferid: waferObject.WaferID.Value,
            //                                    receipeid: Module.StageSupervisor().GetDeviceName(),
            //                                    od: Module.StageSupervisor().ProbingModule().OverDrive,
            //                                    slotnumber: slotnum,
            //                                    curtemperature: Module.StageSupervisor().TempController().TempInfo.CurTemp.Value,
            //                                    settemperature: Module.StageSupervisor().TempController().TempInfo.SetTemp.Value
            //                                    );

            

            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            Module.EventManager().RaisingEvent(typeof(WaferLoadedEvent).FullName, new ProbeEventArgs(this, semaphore));
            semaphore.Wait();
        }

        public abstract WaferTransferProcStateEnum State { get; }

        public abstract void Execute();

        public abstract void SelfRecovery();

        protected long CHUCK_VAC_MAINTAIN_TIME = 50;
        protected long CHUCK_VAC_CHECK_TIME = 5000;
        protected long CHUCK_VAC_WAIT_TIME = 10000;
        protected long CHUCK_THREELEG_WAIT_TIME = 10000;
    }

    public class IdleState : WaferLoadProcState
    {
        public IdleState(WaferLoadProcModule module) : base(module) { }

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

    public class RunningState : WaferLoadProcState
    {
        public RunningState(WaferLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                Module.MonitoringManager().SkipCheckChuckVacuumFlag = false;

                //=> Check no wafer on chuck
                retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                // true = wafer not exist
                retVal = Module.StageSupervisor().StageModuleState.MonitorForVacuum(true, CHUCK_VAC_MAINTAIN_TIME, CHUCK_VAC_CHECK_TIME);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: true);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(true, CHUCK_VAC_WAIT_TIME);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Check wafer on ARM
                //* Loader Process에서 체크했음.
                //retVal = LoaderController.LoaderService.WriteARMVacuum(true);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    StateTransition(new SystemErrorState(Module));
                //    return;
                //}

                //retVal = LoaderController.LoaderService.WaitForARMVacuum(true);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    StateTransition(new SystemErrorState(Module));
                //    return;
                //}
                double armOffset = (Module.LoaderController() as LoaderController.LoaderController).LoaderService.GetArmUpOffset();
                //=> Stage move to loading position
                //Module.StageSupervisor().StageModuleState.ZCLEARED();
                //retVal = Module.StageSupervisor().StageModuleState.MoveLoadingPosition(armOffset);
                //TODO V하고 3하고 달라서 변경방식을 논해야함
                armOffset = 0;
                retVal = Module.StageSupervisor().StageModuleState.MoveLoadingPosition(armOffset);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Module.StageSupervisor().StageModuleState.LoaderDoorOpen();
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Loader move to chuck up position.
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_ChuckUpMove();
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Check wafer on ARM
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_WriteARMVacuum(true);
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_WaitForARMVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> ARM vacuum off and wait.
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_WriteARMVacuum(false);
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_WaitForARMVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Three leg up
                retVal = Module.StageSupervisor().StageModuleState.Handlerhold(CHUCK_THREELEG_WAIT_TIME);
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SetWaferUnknownStatus(true, true);
                    Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Check no wafer on ARM.
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_WriteARMVacuum(true);
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_MonitorForARMVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_WriteARMVacuum(false);
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_WaitForARMVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Set transferred to Chuck(three leg)
                TransferObject loadedObject;
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_NotifyLoadedToThreeLeg(out loadedObject);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                if (loadedObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                {
                    Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.EXIST, loadedObject.WaferType.Value, loadedObject.OCR.Value, loadedObject.OriginHolder.Index);

                    if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        if (this.Module.LotOPModule().LotDeviceParam.StopOption.StopAfterWaferLoad.Value || this.Module.LotOPModule().LotDeviceParam.OperatorStopOption.StopAfterWaferLoad.Value)
                        {
                            Module.LotOPModule().ModuleStopFlag = true;
                            this.Module.LotOPModule().ReasonOfStopOption.IsStop = true;
                            this.Module.LotOPModule().ReasonOfStopOption.Reason = StopOptionEnum.STOP_AFTER_WAFERLOAD;
                            this.Module.CommandManager().SetCommand<ILotOpPause>(this);
                        }
                    }
                }
                else
                {
                    Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.EXIST, loadedObject.WaferType.Value, loadedObject.OCR.Value, -1);
                }

                Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.UNPROCESSED);

                //=> Retract ARM
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_RetractARM();
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Chuck vacuum on
                retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true, extraVacOn: false);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Three leg down
                retVal = Module.StageSupervisor().StageModuleState.Handlerrelease(CHUCK_THREELEG_WAIT_TIME);
                if (retVal != EventCodeEnum.NONE)
                {
                    //Chuck io error => system error
                    (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SetWaferUnknownStatus(false, true);

                    Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Chuck wait for vacuum on
                retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(false, CHUCK_VAC_WAIT_TIME);
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SetWaferUnknownStatus(false, true);
                    
                    Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SafePosW();
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                retVal = Module.StageSupervisor().StageModuleState.LoaderDoorClose();
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                this.Module.GetParam_Wafer().GetSubsInfo().LoadingTime = DateTime.Now.ToLocalTime();
                Module.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                Module.LotOPModule().LotInfo.UpdateWafer(Module.StageSupervisor().WaferObject);

                Module.PMIModule().ResetPMIData();
                this.Module.LotOPModule().ModuleStopFlag = false;

                if (Module.ResultMapManager().NeedDownload() && Module.ResultMapManager().CanDownload())
                {
                    Task<EventCodeEnum> loadResultMapTask = new Task<EventCodeEnum>(() =>
                {
                    IResultMapManager resultMapModule = this.Module.ResultMapManager();
                    var header = resultMapModule.ResultMapHeader;
                    var resultMap = resultMapModule.ResultMapData;
                    resultMapModule.ResultMapHeader = null;
                    resultMapModule.ResultMapData = null;

                    return resultMapModule.ApplyBaseMap(header, resultMap);
                });

                    loadResultMapTask.Start();

                    StateTransition(new SuspendedState(Module) { LoadResultMapTask = loadResultMapTask });
                }
                else
                {
                    SetDoneState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                StateTransition(new SystemErrorState(Module));
            }
        }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SuspendedState : WaferLoadProcState
    {
        public Task<EventCodeEnum> LoadResultMapTask;
        public SuspendedState(WaferLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.SUSPENDED;

        public override void Execute()
        {
            if (LoadResultMapTask != null && LoadResultMapTask.IsCompleted)
            {
                if (LoadResultMapTask.Result == EventCodeEnum.NONE
                    && !LoadResultMapTask.IsCanceled
                    && !LoadResultMapTask.IsFaulted)
                {
                    SetDoneState();
                }
                else
                {
                    StateTransition(new SystemErrorState(Module));
                }
            }
            else if (LoadResultMapTask == null)
            {
                SetDoneState();
            }
        }

        public override void SelfRecovery()
        {
            LoggerManager.Debug($"[WaferLoadProcState - SuspendedState] Not Implemented.");
        }
    }

    public class DoneState : WaferLoadProcState
    {
        public DoneState(WaferLoadProcModule module) : base(module)
        {
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SystemErrorState : WaferLoadProcState
    {
        public SystemErrorState(WaferLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            //No WOKRS.
        }
    }

}

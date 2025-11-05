using LoaderControllerBase;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.WaferTransfer;
using System;

namespace WaferTransfer.WaferUnloadProcStates
{
    public abstract class WaferUnloadProcState
    {
        public WaferUnloadProcModule Module { get; set; }

        public WaferUnloadProcState(WaferUnloadProcModule module)
        {
            this.Module = module;
        }

        public void StateTransition(WaferUnloadProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }

        public abstract WaferTransferProcStateEnum State { get; }

        public abstract void Execute();

        public abstract void SelfRecovery();
    }

    public class IdleState : WaferUnloadProcState
    {
        public IdleState(WaferUnloadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.IDLE;

        public override void Execute()
        {
            // TODO:
            //if( Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.PROCESSED ||
            //    Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.PROBING)
            //{
            //    if (this.Module.ResultMapManager().NeedUpload())
            //    {
            //        Task<EventCodeEnum> uploadResultMapTask = new Task<EventCodeEnum>(() =>
            //        {
            //            return this.Module.ResultMapManager().Upload();
            //        });
            //        uploadResultMapTask.Start();

            //        StateTransition(new SuspendedState(Module) { UploadResultMapTask = uploadResultMapTask });
            //    }
            //    else
            //    {
            //        StateTransition(new RunningState(Module));
            //    }
            //}
            //else
            //{
                StateTransition(new RunningState(Module));
            //}

            //if (this.Module.ResultMapModule().IsUsingResultMap())
            //{
            //    Task<EventCodeEnum> uploadResultMapTask = new Task<EventCodeEnum>(() =>
            //    {
            //        return this.Module.ResultMapModule().UploadResultMap();
            //    });
            //    uploadResultMapTask.Start();

            //    StateTransition(new SuspendedState(Module) { UploadResultMapTask = uploadResultMapTask });
            //}
            //else
            //{
            //    StateTransition(new RunningState(Module));
            //}
        }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class RunningState : WaferUnloadProcState
    {
        public RunningState(WaferUnloadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.RUNNING;

        private long CHUCK_VAC_CHECK_TIME = 5000;
        private long CHUCK_VAC_WAIT_TIME = 10000;
        private long CHUCK_THREELEG_WAIT_TIME = 10000;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                Module.MonitoringManager().SkipCheckChuckVacuumFlag = false;

                //=> Check wafer on chuck.
                retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                // false = wafer exist
                retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(false, CHUCK_VAC_CHECK_TIME);

                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SetWaferUnknownStatus(false, true);
                    Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Check no wafer on ARM. 
                //* Loader Process에서 체크했음.
                //retVal = LoaderController.LoaderService.WriteARMVacuum(true);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    StateTransition(new SystemErrorState(Module));
                //    return;
                //}

                //retVal = LoaderController.LoaderService.MonitorForARMVacuum(false);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    StateTransition(new SystemErrorState(Module));
                //    return;
                //}

                //retVal = LoaderController.LoaderService.WriteARMVacuum(false);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    StateTransition(new SystemErrorState(Module));
                //    return;
                //}

                //=> Stage move to loading pos(== unloading pos)
                double armOffset = 0;
                
                // 신규 장비 추가(EnumLoaderMovingMethodType 추가)시 구분해서 처리해 주어야 함
                if ((Module.LoaderController() as ILoaderControllerExtension).LoaderSystemParam.LoaderMovingMethodType == LoaderParameters.EnumLoaderMovingMethodType.OPUSV_MINI)
                {
                    armOffset = (Module.LoaderController() as LoaderController.LoaderController).LoaderService.GetArmUpOffset();
                }
                else
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
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


                bool hasLoaderWafer = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_IsLoadWafer(); //Load할 웨이퍼가 있는지 먼저 물어본다.


                //=> Check wafer on chuck.
                retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(false, CHUCK_VAC_CHECK_TIME);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Chuck Vacuum Off & Wait
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

                //=> Three leg up
                retVal = Module.StageSupervisor().StageModuleState.Handlerhold(CHUCK_THREELEG_WAIT_TIME);
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SetWaferUnknownStatus(false, true);
                    Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Loader move to chuck down pos 
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_ChuckDownMove();
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> ARM vacuum on.
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_WriteARMVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Three leg down
                retVal = Module.StageSupervisor().StageModuleState.Handlerrelease(CHUCK_THREELEG_WAIT_TIME);
                if (retVal != EventCodeEnum.NONE)
                {
                    (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SetWaferUnknownStatus(true, true);

                    Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> ARM wait for vacuum on
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_WaitForARMVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    //Wafer missed
                    (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SetWaferUnknownStatus(true, true);

                    Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                    StateTransition(new WaferMisssingErrorStateAfterThreeLegDown(Module));
                    return;
                }

                //=> Set transferred to ARM
                var waferState = Module.StageSupervisor().WaferObject.GetState();

                Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.NOT_EXIST);

                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_NotifyUnloadedFromThreeLeg(waferState,1,false);
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Retract ARM
                retVal = (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_RetractARM();
                if (retVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                if (Module.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {
                    if (Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.PROCESSED)
                    {
                        Module.LotOPModule().SystemInfo.IncreaseWaferCount();

                    }
                    Module.LotOPModule().LotInfo.UpdateWafer(Module.StageSupervisor().WaferObject);

                    Module.PMIModule().ResetPMIData();

                    Module.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                    Module.StageSupervisor().WaferObject.GetSubsInfo().FailedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().TestedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().PassedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().CurFailedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().CurTestedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().CurPassedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().Yield = 0;

                    if (this.Module.LotOPModule().IsLastWafer == true)
                    {
                        this.Module.LotOPModule().ModuleStopFlag = true;
                    }

                    Module.EventManager().RaisingEvent(typeof(WaferUnloadedEvent).FullName);
                }
                //Todo : Load 할 wafer확인 하고 있으면 안돌리고 안닫고 없

                if (hasLoaderWafer)
                {
                    this.Module.LotOPModule().ModuleStopFlag = true;
                }
                else
                {
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
                    //1. rotate w axis

                    //2. close door
                }

                this.Module.GetParam_Wafer().GetSubsInfo().UnloadingTime = DateTime.Now;
                StateTransition(new DoneState(Module));
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

    public class SuspendedState : WaferUnloadProcState
    {
        //public Task<EventCodeEnum> UploadResultMapTask = null;
        public SuspendedState(WaferUnloadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.SUSPENDED;

        public override void Execute()
        {
            //if (UploadResultMapTask != null &&
            //    UploadResultMapTask.IsCompleted)
            //{
            //    if (UploadResultMapTask.Result == EventCodeEnum.NONE &&
            //        UploadResultMapTask.IsCanceled == false &&
            //        UploadResultMapTask.IsFaulted == false)
            //    {
            //        StateTransition(new RunningState(Module));
            //    }
            //    else
            //    {
            //        StateTransition(new SystemErrorState(Module));
            //    }
            //}
            //else if (UploadResultMapTask == null)
            //{
            //    StateTransition(new RunningState(Module));
            //}
            StateTransition(new RunningState(Module));
        }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class DoneState : WaferUnloadProcState
    {
        public DoneState(WaferUnloadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SystemErrorState : WaferUnloadProcState
    {
        public SystemErrorState(WaferUnloadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class WaferMisssingErrorStateAfterThreeLegDown : WaferUnloadProcState
    {
        public WaferMisssingErrorStateAfterThreeLegDown(WaferUnloadProcModule module) : base(module) { }

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

    //public class SelfRecoveredStateSelfRecoveredState : WaferUnloadProcState
    //{
    //    public SelfRecoveredState(WaferUnloadProcModule module) : base(module) { }

    //    public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.SELF_RECOVERED;

    //    public override void Execute() { /*NoWORKS*/ }

    //    public override void SelfRecovery()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

}

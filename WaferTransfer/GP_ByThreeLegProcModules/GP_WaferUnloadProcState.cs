using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.WaferTransfer;
using LoaderControllerBase;
using NotifyEventModule;
using LogModule;
using LoaderController.GPController;
using ProberInterfaces.Command.Internal;
using System.Threading;
using ProberInterfaces.Event;
using StageModule;
using System.IO;
using SequenceRunner;
using ProberInterfaces.Temperature;

namespace WaferTransfer.GP_WaferUnloadProcStates
{
    public abstract class GP_WaferUnloadProcState
    {
        public GP_WaferUnloadProcModule Module { get; set; }

        public GP_WaferUnloadProcState(GP_WaferUnloadProcModule module)
        {
            this.Module = module;
        }

        public void StateTransition(GP_WaferUnloadProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }

        public abstract WaferTransferProcStateEnum State { get; }

        public abstract void Execute();

        public abstract void SelfRecovery();
    }

    public class IdleState : GP_WaferUnloadProcState
    {
        public IdleState(GP_WaferUnloadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.IDLE;

        public override void Execute()
        {
            //if (Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.PROCESSED ||
            //    Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.PROBING)
            //{
            //    if (this.Module.StageSupervisor().StageMode != GPCellModeEnum.MAINTENANCE)
            //    {
            //        if (this.Module.ResultMapManager().NeedUpload() && 
            //            Module.StageSupervisor().WaferObject.GetSubsInfo().WaferType != EnumWaferType.POLISH)
            //        {
            //            Task<EventCodeEnum> uploadResultMapTask = new Task<EventCodeEnum>(() =>
            //            {
            //                return this.Module.ResultMapManager().Upload();
            //            });
            //            uploadResultMapTask.Start();

            //            StateTransition(new SuspendedState(Module) { UploadResultMapTask = uploadResultMapTask });
            //        }
            //        else
            //        {
            //            StateTransition(new RunningState(Module));
            //        }
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

            //StateTransition(new RunningState(Module));
        }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class RunningState : GP_WaferUnloadProcState
    {
        //private bool sendDelloated = false;
        public RunningState(GP_WaferUnloadProcModule module) : base(module)
        {
            Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "WAFER UNLOAD START");

            if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
            {
                LoggerManager.ActionLog(ModuleLogType.WAFER_UNLOAD, StateLogType.START, $"Lot ID: {Module.LotOPModule().LotInfo.LotName.Value}, Wafer ID: {Module.GetParam_Wafer().GetSubsInfo().WaferID.Value}", this.Module.LoaderController().GetChuckIndex());
            }
            else
            {
                LoggerManager.ActionLog(ModuleLogType.WAFER_UNLOAD, StateLogType.START, $"Wafer ID: {Module.GetParam_Wafer().GetSubsInfo().WaferID.Value}", this.Module.LoaderController().GetChuckIndex());
            }
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.RUNNING;

        private long CHUCK_VAC_CHECK_TIME = 500;
        private long CHUCK_VAC_WAIT_TIME = 10000;
        private long CHUCK_THREELEG_WAIT_TIME = 20000;

        private void WaitWaferUnLoadingOnTriPod()
        {
            try
            {
                bool UsingBernoulliTopHandler = Module.StageSupervisor().CheckUsingHandler(Module.LoaderController().GetChuckIndex());
                int delayTimeOfDev = (this.Module.TempController().TempSafetyDevParam as ITempSafetyDevParam).WaferUnLoadDelay.Value;
                
                int delayTimeHighTempOfSys = (this.Module.TempController().TempSafetySysParam as ITempSafetySysParam).WaferUnLoad_HighTempSoakTime.Value;
                int delayTimeLowTempOfSys = (this.Module.TempController().TempSafetySysParam as ITempSafetySysParam).WaferUnLoad_LowTempSoakTime.Value;
                int waferUnLoad_SoakTempUpper = (this.Module.TempController().TempSafetySysParam as ITempSafetySysParam).WaferUnLoad_SoakTempUpper.Value;
                int waferUnLoad_SoakTempLower = (this.Module.TempController().TempSafetySysParam as ITempSafetySysParam).WaferUnLoad_SoakTempLower.Value;
                
                double setTemp = this.Module.TempController().TempInfo.SetTemp.Value;
                double curTemp = this.Module.TempController().TempInfo.CurTemp.Value;

                if (delayTimeOfDev > 0)
                {
                    LoggerManager.Debug($"[S] Wafer unloading is waiting on the TriPod. DelayTimeOfDev : {delayTimeOfDev}, [DEV]");
                    // 삼발이 위에서 웨이퍼 대기시키는 동안 삼발이 베큠으로 잡고 있기 위해 베큠 On
                    if (UsingBernoulliTopHandler == false) Module.IOManager().IO.Outputs.DO_TRILEG_SUCTION.SetValue();
                    Thread.Sleep(delayTimeOfDev);
                    // 로더가 가져갈 수 있도록 삼발이 베큠 Off
                    if (UsingBernoulliTopHandler == false) Module.IOManager().IO.Outputs.DO_TRILEG_SUCTION.ResetValue();
                    LoggerManager.Debug($"[E] Wafer unloading is waiting on the TriPod. DelayTimeOfDev : {delayTimeOfDev}, [DEV]");
                }
                else
                {
                    if (waferUnLoad_SoakTempUpper < waferUnLoad_SoakTempLower)
                    {
                        LoggerManager.Debug($"WaitWaferUnLoadingOnTriPod() not execute. Because WaferUnLoad_SoakTempUpper({waferUnLoad_SoakTempUpper}) is lower than WaferUnLoad_SoakTempLower({waferUnLoad_SoakTempLower})");
                        return;
                    }

                    if (setTemp > waferUnLoad_SoakTempUpper || curTemp > waferUnLoad_SoakTempUpper)
                    {
                        if (delayTimeHighTempOfSys > 0)
                        {
                            LoggerManager.Debug($"[S] Wafer unloading is waiting on the TriPod. WaferUnLoad_HighTempSoakTime : {delayTimeHighTempOfSys}, [SYS]");
                            if (UsingBernoulliTopHandler == false) Module.IOManager().IO.Outputs.DO_TRILEG_SUCTION.SetValue();
                            Thread.Sleep(delayTimeHighTempOfSys);
                            if (UsingBernoulliTopHandler == false) Module.IOManager().IO.Outputs.DO_TRILEG_SUCTION.ResetValue();
                            LoggerManager.Debug($"[E] Wafer unloading is waiting on the TriPod. WaferUnLoad_HighTempSoakTime : {delayTimeHighTempOfSys}, [SYS]");
                        }
                        else
                        {
                            LoggerManager.Debug($"delayTimeHighTempOfSys is 0");
                        }
                    }
                    else if (setTemp < waferUnLoad_SoakTempLower || curTemp < waferUnLoad_SoakTempLower)
                    {
                        if (delayTimeLowTempOfSys > 0)
                        {
                            LoggerManager.Debug($"[S] Wafer unloading is waiting on the TriPod. WaferUnLoad_LowTempSoakTime : {delayTimeLowTempOfSys}, [SYS]");
                            if (UsingBernoulliTopHandler == false) Module.IOManager().IO.Outputs.DO_TRILEG_SUCTION.SetValue();
                            Thread.Sleep(delayTimeLowTempOfSys);
                            if (UsingBernoulliTopHandler == false) Module.IOManager().IO.Outputs.DO_TRILEG_SUCTION.ResetValue();
                            LoggerManager.Debug($"[E] Wafer unloading is waiting on the TriPod. WaferUnLoad_LowTempSoakTime : {delayTimeLowTempOfSys}, [SYS]");
                        }
                        else
                        {
                            LoggerManager.Debug($"delayTimeLowTempOfSys is 0");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"WaitWaferUnLoadingOnTriPod() not execute. Because setTemp is {setTemp}, WaferUnLoad_SoakTempUpper is {waferUnLoad_SoakTempUpper}, WaferUnLoad_SoakTempLower is {waferUnLoad_SoakTempLower}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                var cardPodUpCheckcommand = new GP_CheckPCardPodIsDown();
                var retVal_CardPod = Module.CardChangeModule().BehaviorRun(cardPodUpCheckcommand).Result;
                if (retVal_CardPod != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] GP_CheckPCardPodIsDown RetVal:{retVal_CardPod}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                EnumWaferType LoadedWaferType = Module.StageSupervisor().WaferObject.GetSubsInfo().WaferType;

                Module.MonitoringManager().SkipCheckChuckVacuumFlag = false;

                if (this.Module.PolishWaferModule().NeedAngleUpdate == true)
                {
                    IPolishWaferSourceInformation pwinfo = Module.StageSupervisor().WaferObject.GetPolishInfo();

                    double CurrentAngle = pwinfo.CurrentAngle.Value + pwinfo.RotateAngle.Value;
                    int Curpriorty = pwinfo.Priorty.Value + 999;

                    TransferObject ChangedObject = new TransferObject();
                    ChangedObject.PolishWaferInfo = new PolishWaferInformation();
                    ChangedObject.PolishWaferInfo.CurrentAngle.Value = CurrentAngle;
                    ChangedObject.PolishWaferInfo.Priorty.Value = Curpriorty;
                    ChangedObject.PolishWaferInfo.DefineName.Value = pwinfo.DefineName.Value;
                    ChangedObject.PolishWaferInfo.TouchCount.Value = pwinfo.TouchCount.Value;

                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyTransferObject(ChangedObject);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"Polish wafer's angle value was not updated normally. Current Angle : {pwinfo.CurrentAngle.Value}, Notch Angle : {pwinfo.NotchAngle.Value}, Rotate Angle : {pwinfo.RotateAngle.Value}");
                    }
                    else
                    {
                        LoggerManager.Debug($"Polish wafer's angle value was updated normally. Current Angle : {pwinfo.CurrentAngle.Value}, Updated Angle : {CurrentAngle}, Notch Angle : {pwinfo.NotchAngle.Value}, Rotate Angle : {pwinfo.RotateAngle.Value}");
                    }

                    this.Module.PolishWaferModule().NeedAngleUpdate = false;
                }

                if (Module.GetParam_Wafer().GetSubsInfo().WaferType == EnumWaferType.STANDARD)
                {
                    var pivinfo = new PIVInfo() { FoupNumber = Module.GetParam_Wafer().GetOriginFoupNumber() };
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.Module.EventManager().RaisingEvent(typeof(WaferUnloading).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }

                bool UsingBernoulliTopHandler = Module.StageSupervisor().CheckUsingHandler(Module.LoaderController().GetChuckIndex());
                
                if (UsingBernoulliTopHandler == true && Module.StageSupervisor().StageModuleState.IsHandlerholdWafer() == true)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    var CheckTheelegUpAndDown = new GOP_ThreeLegCheckAndDown();
                    retVal = Module.CardChangeModule().BehaviorRun(CheckTheelegUpAndDown).Result;

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "ThreeLeg down failed before receiving the wafer", this.ToString());
                        LoggerManager.Debug($"[GP_WaferLoadProcState Error] GOP_ThreeLegCheckAndDown RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"ThreeLeg Down Error before receiving the wafer. in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Chuck VacuumOn Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (VacuumOnOff) Value :true RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum On Check Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                    // false = wafer exist
                    retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(false, CHUCK_VAC_CHECK_TIME);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "WaitForVacuum(false) Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (WaitForVacuum_CHUCK_VAC_CHECK_TIME) Value :false RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_SetWaferUnknownStatus(false, true);

                        Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum Wait TimeOut Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }
               
                //=> Stage move to loading pos(== unloading pos)
                double armOffset = 0;
                bool waitForLoadingPos = true;
                
                Task.Run(() =>
                {
                    retVal = Module.StageSupervisor().StageModuleState.MoveLoadingPosition(armOffset);
                    waitForLoadingPos = false;
                });

                EventCodeEnum loaderRetVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_Wafer_MoveLoadingPosition();
                while (waitForLoadingPos)
                {
                    System.Threading.Thread.Sleep(10);
                }

                if (retVal != EventCodeEnum.NONE)
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "MoveLoadingPosition Error", this.ToString());
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (MoveLoadingPosition) RetVal:{retVal}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Move Loading Position Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                }

                if (loaderRetVal != EventCodeEnum.NONE)
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Loader MoveLoadingPosition Error", this.ToString());
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (Loader MoveLoadingPosition) RetVal:{retVal}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Move Loading Position Error in Loader to Cell{Module.LoaderController().GetChuckIndex()}");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                }

                if (retVal != EventCodeEnum.NONE || loaderRetVal != EventCodeEnum.NONE)
                {
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (UsingBernoulliTopHandler == true && Module.StageSupervisor().StageModuleState.IsHandlerholdWafer() == true)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    //=> Check wafer on chuck.
                    retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Chuck Vacuum On Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (VacuumOnOff) Value:true RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum On Check Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(false, CHUCK_VAC_CHECK_TIME);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "WaitForVacuum(false) Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (WaitForVacuum_CHUCK_VAC_CHECK_TIME) Value:false RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum Wait TimeOut Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }
               
                if (UsingBernoulliTopHandler == false)
                {
                    //=> Chuck Vacuum Off & Wait
                    retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: true);//(Module.LoaderController() as GP_LoaderController).isExchange); 
                                                                                                // Exchange 와 상관없이 Three Leg Down 동작에서 Chuck Vac 을 사용하는 동작이 있기 때문에 extraVacReady:true 여야함.
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Chuck Vaccum off Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (VacuumOnOff) Value:false RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum Off Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(true, CHUCK_VAC_WAIT_TIME);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "WaitForVacuum(true) Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (WaitForVacuum_CHUCK_VAC_WAIT_TIME) Value:true RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum Wait TimeOut Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }

                var waferState = Module.StageSupervisor().WaferObject.GetState();

                //=> Three leg up
                retVal = Module.StageSupervisor().StageModuleState.Handlerhold(CHUCK_THREELEG_WAIT_TIME);// 이 시점에 척에 웨이퍼가 EXIST 상태여서 베큠을 꺼놨는데 다시 킴. 

                if (retVal != EventCodeEnum.NONE)
                {
                    if (UsingBernoulliTopHandler == false)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "3Pin Up Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (ThreeLegUp error)  RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"3Pin Up Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);

                        retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true);

                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Chuck VaccumOn Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (VacuumOnOff) Value :true RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum On Check Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }

                        retVal = Module.StageSupervisor().StageModuleState.Handlerrelease(CHUCK_THREELEG_WAIT_TIME);

                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "3Pin Down Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (ThreeLegDown error)  RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_SetWaferUnknownStatus(true, true);
                            
                            Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                            Module.WaferTransferModule.NeedToRecovery = true;
                            Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"3Pin Down Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }

                        retVal = Module.StageSupervisor().CheckWaferStatus(true);

                        LoggerManager.Debug($"CheckWaferStatus: isExist = true, ret:{retVal.ToString()}");

                        if (retVal != EventCodeEnum.NONE)
                        {
                            retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: true);//(Module.LoaderController() as GP_LoaderController).isExchange); 
                                                                                                                       // Exchange 와 상관없이 Three Leg Down 동작에서 Chuck Vac 을 사용하는 동작이 있기 때문에 extraVacReady:true 여야함.
                            if (retVal != EventCodeEnum.NONE)
                            {
                                Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Chuck VaccumOn Error", this.ToString());
                                LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (VacuumOnOff) Value :false RetVal:{retVal}");
                                (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum On Check Error in Cell{Module.LoaderController().GetChuckIndex()}");
                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }

                            retVal = Module.StageSupervisor().StageModuleState.Handlerhold(CHUCK_THREELEG_WAIT_TIME);

                            Module.WaferTransferModule.NeedToRecovery = true;
                            Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);
                        }
                        else
                        {
                            LoggerManager.Debug($"[GP WaferUnLoadProcState Error] Wafer exist on Chuck");
                        }
                    }
                    else
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Bernoulli Transfer Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (Bernoulli Transfer error) Chuck -> Bernoulli  RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_SetWaferUnknownStatus(true, true);
                        
                        Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Bernoulli Transfer Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Handlerhold Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                        StateTransition(new SystemErrorState(Module));

                        return;

                    }

                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                
                // 언로드 전 대기
                WaitWaferUnLoadingOnTriPod();

                if (UsingBernoulliTopHandler)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_Notifyhandlerholdwafer(true);
                }
                retVal = Module.StageSupervisor().StageModuleState.LoaderDoorOpen();
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Loader Door Open Error", this.ToString());
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (LoaderDoorOpen error)  RetVal:{retVal}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Shutter Door Open Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);

                    retVal = Module.StageSupervisor().StageModuleState.Handlerrelease(CHUCK_THREELEG_WAIT_TIME);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "3Pin Down Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (ThreeLegDown error)  RetVal:{retVal}");
                        retVal = Module.StageSupervisor().StageModuleState.Handlerhold(CHUCK_THREELEG_WAIT_TIME);
                        Module.WaferTransferModule.NeedToRecovery = true;
                        Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"3Pin Down Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                    retVal = Module.StageSupervisor().CheckWaferStatus(true);
                    LoggerManager.Debug($"CheckWaferStatus: isExist = true, ret:{retVal.ToString()}");
                    if (retVal != EventCodeEnum.NONE)
                    {
                        retVal = Module.StageSupervisor().StageModuleState.Handlerhold(CHUCK_THREELEG_WAIT_TIME);
                        Module.WaferTransferModule.NeedToRecovery = true;
                        Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);
                    }
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                else
                {
                    (Module.LoaderController() as GP_LoaderController).LoaderDoorOpenTicks = DateTime.UtcNow.Ticks;
                }                

                //=> Loader move to chuck down pos 
                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_ChuckDownMove();
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Wafer Unload Error.\n Wafer Pick Error in Loader(Location of wafer: cell)", this.ToString());
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (WTR_ChuckDownMove error)  RetVal:{retVal} Wafer Pick Error in Loader(Location of wafer: cell)");
                    retVal = Module.StageSupervisor().StageModuleState.LoaderDoorClose();
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Wafer Pick Error in Loader.(Location of wafer: cell)");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (UsingBernoulliTopHandler)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_Notifyhandlerholdwafer(false);
                }

                LoggerManager.Debug($"[WaferUnload] (Save_SlotInfo) Start");
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferStatus = EnumSubsStatus.NOT_EXIST;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferType = EnumWaferType.UNDEFINED;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.OCRAngle = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferID = "";
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferSize = SubstrateSizeEnum.UNDEFINED;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.NotchType = WaferNotchTypeEnum.UNKNOWN;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.OriginSlotIndex = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.LoadingAngle = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.UnloadingAngle = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.SlotIndex = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.FoupIndex = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferState = EnumWaferState.UNDEFINED;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.OriginHolder = new ModuleID();
                var ret = (Module.StageSupervisor() as StageSupervisor).SaveSlotInfo();
                LoggerManager.Debug($"[WaferUnload] (Save_SlotInfo) Done RetVal:{ret}");

                string waferIdBeforeUnload = Module.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value;

                if (Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST &&
                    Module.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.POLISH)
                {
                    if (Module.StageSupervisor().WaferObject.GetState() == EnumWaferState.SKIPPED)
                    {
                        Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.UNPROCESSED);
                    }
                }

                Module.WaferTransferModule.NeedToRecovery = false;
                Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);

                if ((Module.ProbingModule().ProbingEndReason == ProbingEndReason.UNDEFINED) &&
                    (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT))
                {
                    Module.ProbingModule().SetProbingEndState(ProbingEndReason.MANUAL_UNLOAD, waferState);
                }

                // SetWaferStatus에서 SlotIndex를 0으로 초기화 해놓기 때문에 그 밑으로는 전부 OriginFoupIndex가 0임. 미리 받아 놓음.
                int originFoupNum = Module.GetParam_Wafer().GetOriginFoupNumber();
                // 위치 중요함!! 함부로 건드리지 말것.
                Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.NOT_EXIST);

                Module.GEMModule().GetPIVContainer().SetUnloadedFormChuckWaferID(Module.GEMModule().GetPIVContainer().WaferID.Value);

                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyUnloadedFromThreeLeg(waferState, this.Module.LoaderController().GetChuckIndex(), !Module.WaferTransferModule.NeedToRecovery);
                
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "WTR_NotifyUnloadedFromThreeLeg Error", this.ToString());
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (WTR_NotifyUnloadedFromThreeLeg error)  RetVal:{retVal}");
                    retVal = Module.StageSupervisor().StageModuleState.LoaderDoorClose();
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"WaferState Setting Error in Loader");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    StateTransition(new SystemErrorState(Module));

                    return;
                }

                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(true);

                this.Module.LoaderController().SetProbingStart(false);

                //=> Three leg down

                if ((Module.LoaderController() as GP_LoaderController).isExchange && Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                }
                else
                {
                    retVal = Module.StageSupervisor().StageModuleState.LoaderDoorClose();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Loader Door Close Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (LoaderDoorClose error)  RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Shutter Door Close Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        StateTransition(new SystemErrorState(Module));

                        return;
                    }
                    else
                    {
                        long startTicks = (Module.LoaderController() as GP_LoaderController).LoaderDoorOpenTicks;
                        if (startTicks > 0)
                        {
                            long endTicks = DateTime.UtcNow.Ticks;
                            long elapsedTicks = endTicks - startTicks;
                            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                            LoggerManager.Debug($"LoaderDoor Opened Duration={elapsedSpan.TotalSeconds}");
                            (Module.LoaderController() as GP_LoaderController).LoaderDoorOpenTicks = 0;
                        }
                    }

                    retVal = Module.StageSupervisor().StageModuleState.Handlerrelease(CHUCK_THREELEG_WAIT_TIME);// 여기에서 Vacuuum 또 사용함. 따라서 이 시퀀스 이전에는 Extra Chuck Air On 안끈다.

                    if (retVal != EventCodeEnum.NONE)
                    {
                        if (UsingBernoulliTopHandler == false)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "3Pin Down Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (ThreeLegDown error)  RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_SetWaferUnknownStatus(true, true);
                            
                            Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"3Pin Down Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            StateTransition(new SystemErrorState(Module));

                            return;
                        }
                        else 
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Bernoulli Transfer Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (Bernoulli Transfer error) Bernoulli -> Chuck RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_SetWaferUnknownStatus(true, true);

                            Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Bernoulli Transfer Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Handlerrelease Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            StateTransition(new SystemErrorState(Module));

                            return;
                        }
                    }
                    else
                    {
                        //=> Chuck Vacuum Off & Wait
                        retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: false);//(Module.LoaderController() as GP_LoaderController).isExchange); 
                                                                                                                   // Exchange 와 상관없이 Three Leg Down 동작에서 Chuck Vac 을 사용하는 동작이 있기 때문에 extraVacReady:true 여야함.
                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "After Three Leg Down After Three Leg Down  ", this.ToString());
                            LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (VacuumOnOff) After Three Leg Down  Value:false RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"After Three Leg Down After Three Leg Down in Cell{Module.LoaderController().GetChuckIndex()}");
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }
                }
              
                var pivlockobj = Module.GEMModule().GetPIVContainer().GetPIVDataLockObject();
                lock (pivlockobj)
                {
                    //Wafer Unload 시 Gem의 WaferId, Slot number 초기화.
                    Module.GEMModule().GetPIVContainer().ResetWaferID("");
                    Module.GEMModule().GetPIVContainer().SlotNumber.Value = -9999;
                }

                if (Module.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {
                    Module.LotOPModule().LotInfo.UpdateWafer(Module.StageSupervisor().WaferObject);

                    Module.PMIModule().ResetPMIData();

                    Module.StageSupervisor().WaferObject.GetSubsInfo().FailedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().TestedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().PassedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().CurFailedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().CurTestedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().CurPassedDieCount.Value = 0;
                    Module.StageSupervisor().WaferObject.GetSubsInfo().Yield = 0;

                    string cstHash = Module.StageSupervisor().GetSlotInfo().CSTHashCode;

                    //var anotherlotinfos = this.Module.LotOPModule().LotInfo.GetLotInfos().FindAll(x => x.FoupIndex != originFoupNum && x.CassetteHashCode != cstHash);
                    //int anotherstartlotinfocount = 0;

                    //if (anotherlotinfos != null)
                    //{
                    //    anotherstartlotinfocount = anotherlotinfos.Count(linfo => linfo.IsLotStarted == true);
                    //}

                    //int lotinfosCnt = this.Module.LotOPModule().LotInfo.GetLotInfos().Count(x => x.FoupIndex != originFoupNum && x.CassetteHashCode != cstHash);
                    //if (this.Module.LotOPModule().IsLastWafer == true && (lotinfosCnt == 0 ))

                    //LoggerManager.Debug($"IsLastWafer is {this.Module.LotOPModule().IsLastWafer}, anotherstartlotinfocount = {anotherstartlotinfocount}");

                    //if (this.Module.LotOPModule().IsLastWafer == true && anotherstartlotinfocount == 0)
                    if (this.Module.LotOPModule().IsLastWafer == true)
                        {
                        if (!(Module.LoaderController() as GP_LoaderController).isExchange)
                        {
                            LoggerManager.Debug($"{this.GetType().Name}] Execute() : Before SetCommand<ILotOpEnd>");

                            Module.CommandManager().SetCommand<ILotOpEnd>(this);
                            this.Module.LotOPModule().ModuleStopFlag = true; // TODO: 이거 놓치면 안될 듯함.
                        }
                    }

                    Module.EventManager().RaisingEvent(typeof(WaferUnloadedEvent).FullName);

                    if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                    {
                        LotAssignStateEnum lotAssignStateEnum = Module.LotOPModule().LotInfo.GetStageLotAssignState(cstHashCode: cstHash);
                        if (Module.LotOPModule().LotInfo.NeedLotDeallocated || lotAssignStateEnum == LotAssignStateEnum.CANCEL)
                        {
                            Module.LotOPModule().LotInfo.SetStageLotAssignState(LotAssignStateEnum.JOB_FINISHED, cstHashCode: cstHash);

                            var pivinfo = new PIVInfo() { FoupNumber = originFoupNum };
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            Module.EventManager().RaisingEvent(typeof(StageDeallocatedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                            //sendDelloated = true;
                            Module.LotOPModule().LotInfo.NeedLotDeallocated = false;

                            LoggerManager.Debug("NeedLotDeallocated is off (Prev NeedLotDeallocated is true)");
                        }
                    }

                    if (this.Module.ProbingModule().IsReservePause == true)
                    {
                        this.Module.ProbingModule().IsReservePause = false;
                    }
                    if (Module.LoaderController().IsCancel == true)
                    {
                        Module.LoaderController().IsCancel = false;
                    }
                }

                Module.LotOPModule().UnloadFoupNumber = 0;
                Module.LotOPModule().LotInfo.SetCassetteHashCode("");
                bool ispmiOP = this.Module.PMIModule().GetPMIEnableParam();

                // Polish Wafer인 경우, WaferType이 UNDEFINED 값을 갖고 있다.
                if (ispmiOP && LoadedWaferType == EnumWaferType.STANDARD)
                {
                    EventCodeEnum retValImageUpload = EventCodeEnum.UNDEFINED;
                    // log upload 비동기 처리
                    Task task = new Task(() =>
                    {
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.LogUpload(Module.LoaderController().GetChuckIndex(), EnumUploadLogType.PMI);
                    });
                    task.Start();

                    // image upload
                    bool originImgSaveEnable = (Module.PMIModule().PMIModuleDevParam_IParam as IPMIModuleDevParam).LogInfo.OriginalImageSaveHDDEnable.Value;

                    if (originImgSaveEnable)
                    {
                        string SaveBasePath = Module.FileManager().GetImageSavePath(EnumProberModule.PMI);

                        string[] files = Directory.GetFiles(SaveBasePath);

                        if (files.Length != 0)
                        {
                            if (waferIdBeforeUnload != null && waferIdBeforeUnload != "" && waferIdBeforeUnload != "DEBUGGING")
                            {
                                foreach (var file in files)
                                {

                                    // 이번에 언로드 된 웨이퍼에 대한 PMI 이미지만 로더로 업로드.
                                    if (file.Contains(waferIdBeforeUnload) && !(file.Contains("PASS") || file.Contains("FAIL")))
                                    {
                                        byte[] pmiimges = new byte[0];
                                        FileInfo fi = null;
                                        try
                                        {
                                            fi = new FileInfo(file);
                                            pmiimges = File.ReadAllBytes(fi.FullName);
                                            string filename = Path.GetFileNameWithoutExtension(fi.FullName);

                                            retValImageUpload = (Module.LoaderController() as GP_LoaderController).GPLoaderService.PMIImageUploadStageToLoader(Module.LoaderController().GetChuckIndex(), pmiimges, filename);
                                            if (retValImageUpload != EventCodeEnum.NONE)
                                            {
                                                retVal = retValImageUpload;
                                                LoggerManager.Debug($"PMIImageUploadStageToLoader Failed");
                                                break;
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            LoggerManager.Exception(err);
                                        }
                                    }
                                }
                                if (retValImageUpload == EventCodeEnum.NONE)
                                {
                                    //이미지를 서버로 올리는 시간 소요가 크기 때문에 Task로 처리하여 함수 콜 시키고 지나가게 함. (return type이 task 이므로 return 값을 확인하지 않을 경우 비동기 호출이 된다.)
                                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.PMIImageUploadLoaderToServer(Module.LoaderController().GetChuckIndex());
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"WaferID is NULL");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"The file does not exist in {SaveBasePath}");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"originImgSaveEnable is false");
                    }
                }

                if ((Module.LoaderController() as GP_LoaderController).isExchange && Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    StateTransition(new PendingState(Module));
                  
                }
                else
                {
                    StateTransition(new DoneState(Module));
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($ex);
                LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] Exception:{err.ToString()}");
                LoggerManager.Exception(err);
                Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(EventCodeEnum.UNDEFINED, $"{err.Message}", this.ToString());
                StateTransition(new SystemErrorState(Module));
            }
        }

        protected TransferObject FindLoadWafer(ref List<TransferObject> loadablewafers)
        {
            TransferObject loadWafer = null;

            try
            {
                var allWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll();
                var loadableWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
                        item =>
                        item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                        item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                        item.WaferType.Value == EnumWaferType.STANDARD &&
                        item.ReservationState == ReservationStateEnum.NONE &&
                        item.WaferState == EnumWaferState.UNPROCESSED &&
                        item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                        item.OCRReadState == ProberInterfaces.Enum.OCRReadStateEnum.DONE &&
                        item.UsingStageList.Contains((this.Module.LoaderController() as GP_LoaderController).ChuckID.Index)
                        ).ToList();


                loadablewafers = loadableWafers;
                var loadWafers = loadableWafers.FindAll(wafers => wafers.DeviceName.Value.Equals(this.Module.FileManager().FileManagerParam.DeviceName)).OrderBy(item => item.OriginHolder.Index);
                loadWafer = loadWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();

                if (loadWafer != null)
                {
                    loadWafer.NotchAngle.Value = this.Module.GetParam_Wafer().GetPhysInfo().NotchAngle.Value;
                }
                if (loadWafer == null)
                {
                    loadableWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
                        item =>
                        item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                        item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                        item.WaferType.Value == EnumWaferType.STANDARD &&
                        item.ReservationState == ReservationStateEnum.NONE &&
                        item.WaferState == EnumWaferState.UNPROCESSED &&
                        item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                        item.UsingStageList.Contains((this.Module.LoaderController() as GP_LoaderController).ChuckID.Index)
                        ).ToList();
                }

                //var polishparam = (this.Module.PolishWaferModule().PolishWaferParameter as IPolishWaferParameter);

                //if (polishparam.NeedLoadWaferFlag == true)
                //{
                //    var loadablePolishWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
                //        item => item.WaferType.Value == EnumWaferType.POLISH &&
                //         (item.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY ||
                //         item.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY ||
                //         item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT) &&
                //         item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                //         item.ReservationState == ReservationStateEnum.NONE &&
                //         item.PolishWaferInfo.DefineName.Value== polishparam.LoadWaferType
                //    ).ToList();

                //    loadWafer = loadablePolishWafers.OrderBy(item => item.OriginHolder.Index).FirstOrDefault();

                //    if (loadWafer == null)
                //    {
                //        loadablePolishWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
                //        item => item.WaferType.Value == EnumWaferType.POLISH).ToList();

                //        // 현재 로더에 폴리쉬 웨이퍼는 없지만, 다른 스테이지가 갖고 있다. 따라서, 대기
                //        if (loadablePolishWafers.Count > 0)
                //        {
                //        }
                //        else
                //        {
                //            //this.Module.PolishWaferModule().PolishWaferValidate(false);
                //        }
                //    }
                //}
                //else
                //{
                //    var allWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll();
                //    var loadableWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
                //            item =>
                //            item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                //            item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                //            item.WaferType.Value == EnumWaferType.STANDARD &&
                //            item.ReservationState == ReservationStateEnum.NONE &&
                //            item.WaferState == EnumWaferState.UNPROCESSED &&
                //            item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                //            item.OCRReadState == ProberInterfaces.Enum.OCRReadStateEnum.DONE &&
                //            item.UsingStageList.Contains((this.Module.LoaderController() as GP_LoaderController).ChuckID.Index)
                //            ).ToList();



                //    loadWafer = loadableWafers.FindAll(wafers => wafers.DeviceName.Value.Equals(this.Module.FileManager().FileManagerParam.DeviceName)).OrderBy(item => item.OriginHolder.Index).FirstOrDefault();
                //    loadWafer = loadableWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();

                //    if (loadWafer != null)
                //    {
                //        loadWafer.NotchAngle.Value = this.Module.GetParam_Wafer().GetPhysInfo().NotchAngle.Value;
                //    }
                //    if (loadWafer == null)
                //    {
                //        loadableWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
                //            item =>
                //            item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                //            item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                //            item.WaferType.Value == EnumWaferType.STANDARD &&
                //            item.ReservationState == ReservationStateEnum.NONE &&
                //            item.WaferState == EnumWaferState.UNPROCESSED &&
                //            item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                //            item.UsingStageList.Contains((this.Module.LoaderController() as GP_LoaderController).ChuckID.Index)
                //            ).ToList();
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return loadWafer;
        }


        //protected TransferObject FindLoadWafer()
        //{
        //    TransferObject loadWafer = null;
        //    try
        //    {
        //        var needPolish = (this.Module.PolishWaferModule().PolishWaferParameter as IPolishWaferParameter);
        //        if (needPolish.NeedLoadWaferFlag == true)
        //        {
        //            var loadablePolishWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
        //                item => item.WaferType.Value == EnumWaferType.POLISH &&
        //                 (item.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY ||
        //                 item.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY ||
        //                 item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT) &&
        //                 item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
        //                 item.ReservationState == ReservationStateEnum.NONE
        //            //item.PolishWaferInfo!=null&&
        //            //item.PolishWaferInfo.DefineName.Value== needPolish.LoadWaferType
        //            ).ToList();

        //            loadWafer = loadablePolishWafers.OrderBy(item => item.OriginHolder.Index).FirstOrDefault();

        //            if (loadWafer == null)
        //            {
        //                loadablePolishWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
        //                item => item.WaferType.Value == EnumWaferType.POLISH).ToList();

        //                // 현재 로더에 폴리쉬 웨이퍼는 없지만, 다른 스테이지가 갖고 있다. 따라서, 대기
        //                if (loadablePolishWafers.Count > 0)
        //                {
        //                }
        //                else
        //                {
        //                    //this.Module.PolishWaferModule().PolishWaferValidate(false);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var allWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll();
        //            var loadableWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
        //                    item =>
        //                    item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
        //                    item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
        //                    item.WaferType.Value == EnumWaferType.STANDARD &&
        //                    item.ReservationState == ReservationStateEnum.NONE &&
        //                    item.WaferState == EnumWaferState.UNPROCESSED &&
        //                    item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
        //                    item.OCRReadState == ProberInterfaces.Enum.OCRReadStateEnum.DONE &&
        //                    item.UsingStageList.Contains((this.Module.LoaderController() as GP_LoaderController).ChuckID.Index)
        //                    ).ToList();



        //            loadWafer = loadableWafers.FindAll(wafers => wafers.DeviceName.Value.Equals(this.Module.FileManager().FileManagerParam.DeviceName)).OrderBy(item => item.OriginHolder.Index).FirstOrDefault();
        //            loadWafer = loadableWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();

        //            if (loadWafer != null)
        //            {
        //                loadWafer.NotchAngle.Value = this.Module.GetParam_Wafer().GetPhysInfo().NotchAngle.Value;
        //            }
        //            if (loadWafer == null)
        //            {
        //                loadableWafers = (this.Module.LoaderController() as GP_LoaderController).ReqMap.GetTransferObjectAll().Where(
        //                    item =>
        //                    item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
        //                    item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
        //                    item.WaferType.Value == EnumWaferType.STANDARD &&
        //                    item.ReservationState == ReservationStateEnum.NONE &&
        //                    item.WaferState == EnumWaferState.UNPROCESSED &&
        //                    item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
        //                    item.UsingStageList.Contains((this.Module.LoaderController() as GP_LoaderController).ChuckID.Index)
        //                    ).ToList();
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return loadWafer;
        //}
        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SuspendedState : GP_WaferUnloadProcState
    {
        //public Task<EventCodeEnum> UploadResultMapTask = null;
        public SuspendedState(GP_WaferUnloadProcModule module) : base(module) { }

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

    public class DoneState : GP_WaferUnloadProcState
    {
        public DoneState(GP_WaferUnloadProcModule module) : base(module)
        {
            Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "WAFER UNLOAD DONE");

            LoggerManager.ActionLog(ModuleLogType.WAFER_UNLOAD, StateLogType.DONE,
                $"Lot ID: {Module.LotOPModule().LotInfo.LotName.Value}, Wafer ID: {Module.GetParam_Wafer().GetSubsInfo().WaferID.Value}",
                this.Module.LoaderController().GetChuckIndex());

            if (Module.GetParam_Wafer().GetSubsInfo().WaferType == EnumWaferType.STANDARD)
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.Module.EventManager().RaisingEvent(typeof(WaferUnloadedEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();

                //this.Module.GEMModule().SetEvent(this.Module.GEMModule().GetEventNumberFormEventName(typeof(WaferUnloadedEvent).FullName));
            }
            else if (Module.GetParam_Wafer().GetSubsInfo().WaferType == EnumWaferType.POLISH)
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.Module.EventManager().RaisingEvent(typeof(PolishWaferUnloadedEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
                //this.Module.GEMModule().SetEvent(this.Module.GEMModule().GetEventNumberFormEventName(typeof(PolishWaferUnloadedEvent).FullName));
            }
            this.Module.GEMModule().GetPIVContainer().SetUnloadedFormChuckWaferID("");
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class PendingState : GP_WaferUnloadProcState
    {
        public PendingState(GP_WaferUnloadProcModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.WAFER_UNLOAD, StateLogType.DONE, $"Lot ID: {Module.LotOPModule().LotInfo.LotName.Value}, Wafer ID: {Module.GetParam_Wafer().GetSubsInfo().WaferID.Value}", this.Module.LoaderController().GetChuckIndex());
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.PENDING;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }
    }

    public class SystemErrorState : GP_WaferUnloadProcState
    {
        public SystemErrorState(GP_WaferUnloadProcModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.WAFER_UNLOAD, StateLogType.ERROR, $"device:{Module.FileManager().GetDeviceName()}, ReasonOfError: {Module.WaferTransferModule.ReasonOfError.GetLastEventMessage()}", this.Module.LoaderController().GetChuckIndex());
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;
        //private long CHUCK_VAC_CHECK_TIME = 500;
        private long CHUCK_VAC_WAIT_TIME = 10000;
        private long CHUCK_THREELEG_WAIT_TIME = 20000;
        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                Module.MonitoringManager().SkipCheckChuckVacuumFlag = false;

                (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_Wafer_MoveLoadingPosition();

                //=> Chuck Vacuum Off & Wait
                retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: false);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (VacuumOnOff) Value:false RetVal:{retVal}");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(true, CHUCK_VAC_WAIT_TIME);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (WaitForVacuum_CHUCK_VAC_WAIT_TIME) Value:true RetVal:{retVal}");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                
                retVal = Module.StageSupervisor().StageModuleState.Handlerhold(CHUCK_THREELEG_WAIT_TIME);
                if (retVal != EventCodeEnum.NONE && !(Module.LoaderController() as GP_LoaderController).isExchange) //exchange상황에서는 삼발이 업했는데도 또 하라고해서 에러날수도 있으니 조건을 추가함
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "3Pin Up Error", this.ToString());
                    LoggerManager.Debug($"[GP_WaferLoadProcState Error] (3Pin Up) RetVal:{retVal}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Unload Error in Cell{Module.LoaderController().GetChuckIndex()}", $"3Pin Up Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                retVal = Module.StageSupervisor().StageModuleState.LoaderDoorOpen();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (LoaderDoorOpen error)  RetVal:{retVal}");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                System.Threading.Thread.Sleep(1000);

                //=> Loader move to chuck down pos 
                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_ChuckDownMove();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (WTR_ChuckDownMove error)  RetVal:{retVal}");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                LoggerManager.Debug($"[WaferUnload] (Save_SlotInfo) Start");
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferStatus = EnumSubsStatus.NOT_EXIST;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferType = EnumWaferType.UNDEFINED;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.OCRAngle = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferID = "";
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferSize = SubstrateSizeEnum.UNDEFINED;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.OriginSlotIndex = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.LoadingAngle = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.UnloadingAngle = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.SlotIndex = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.FoupIndex = -1;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferState = EnumWaferState.UNDEFINED;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.OriginHolder = new ModuleID();
                var ret = (Module.StageSupervisor() as StageSupervisor).SaveSlotInfo();
                LoggerManager.Debug($"[WaferUnload] (Save_SlotInfo) Done RetVal:{ret}");

                Module.WaferTransferModule.NeedToRecovery = false;
                Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);

                Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.NOT_EXIST);

                var waferState = Module.StageSupervisor().WaferObject.GetState();

                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyUnloadedFromThreeLeg(waferState, this.Module.LoaderController().GetChuckIndex(), !Module.WaferTransferModule.NeedToRecovery);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (WTR_NotifyUnloadedFromThreeLeg error)  RetVal:{retVal}");
                    retVal = Module.StageSupervisor().StageModuleState.LoaderDoorClose();
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(true);
                //=> Three leg down

                if ((Module.LoaderController() as GP_LoaderController).isExchange && Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                }
                else
                {
                    retVal = Module.StageSupervisor().StageModuleState.LoaderDoorClose();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (LoaderDoorClose error)  RetVal:{retVal}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    retVal = Module.StageSupervisor().StageModuleState.Handlerrelease(CHUCK_THREELEG_WAIT_TIME);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] (ThreeLegDown error)  RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_SetWaferUnknownStatus(true, true);

                        Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }
            
                var pivlockobj = Module.GEMModule().GetPIVContainer().GetPIVDataLockObject();
                lock (pivlockobj)
                {
                    Module.GEMModule().GetPIVContainer().ResetWaferID(" ");
                }

                if ((Module.LoaderController() as GP_LoaderController).isExchange && Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    StateTransition(new PendingState(Module));
                }
                else
                {
                    this.Module.LoaderController().SetTransferError(false);
                    StateTransition(new DoneState(Module));
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($ex);
                LoggerManager.Debug($"[GP_WaferUnLoadProcState Error] Exception:{err.ToString()}");
                LoggerManager.Exception(err);

                StateTransition(new SystemErrorState(Module));
            }
            finally
            {
                //Module.LoaderController().UpdateIsNeedLotEnd();
            }
        }
        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class WaferMisssingErrorStateAfterThreeLegDown : GP_WaferUnloadProcState
    {
        public WaferMisssingErrorStateAfterThreeLegDown(GP_WaferUnloadProcModule module) : base(module) { }

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

    //public class SelfRecoveredState : WaferUnloadProcState
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

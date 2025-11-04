using System;
using System.Threading.Tasks;
using ProberInterfaces.WaferTransfer;
using ProberInterfaces;
using ProberErrorCode;
using LoaderControllerBase;
using NotifyEventModule;
using LogModule;
using ProberInterfaces.Command.Internal;
using LoaderController.GPController;
using System.Threading;
using ProberInterfaces.Event;
using StageModule;
using ProberInterfaces.ResultMap;
using SequenceRunner;
using ProberInterfaces.Temperature;

namespace WaferTransfer.GP_WaferLoadProcStates
{
    public abstract class GP_WaferLoadProcState
    {
        public GP_WaferLoadProcModule Module { get; set; }

        public GP_WaferLoadProcState(GP_WaferLoadProcModule module)
        {
            this.Module = module;
        }

        public void StateTransition(GP_WaferLoadProcState stateObj)
        {
            this.Module.StateObj = stateObj;
        }

        public abstract WaferTransferProcStateEnum State { get; }

        public abstract void Execute();

        public abstract void SelfRecovery();

        internal void SetDoneState()
        {
            StateTransition(new DoneState(Module));

            //Module.EventManager().RaisingEvent(typeof(WaferLoadedEvent).FullName); // #Hynix_Merge: WaferLoadedEvent 중복 호출 가능성 있음. 이타이밍에 필요하다면 ResultMap을 다운받았다는 의미의 이벤트를 따로 만들것.
        }

        protected long CHUCK_VAC_MAINTAIN_TIME = 100;
        protected long CHUCK_VAC_CHECK_TIME = 3000;
        protected long CHUCK_VAC_WAIT_TIME = 10000;
        protected long CHUCK_THREELEG_WAIT_TIME = 20000;
    }

    public class IdleState : GP_WaferLoadProcState
    {
        public IdleState(GP_WaferLoadProcModule module) : base(module) { }

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

    public class RunningState : GP_WaferLoadProcState
    {
        public RunningState(GP_WaferLoadProcModule module) : base(module)
        {
            Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "WAFER LOAD START");

            //Wafer가 load start시 Soaking Module이 Waiting wafer 상태였다면 No Soak으로 출력해준다.
            //Chuck의 위치가 Soaking position을 벗어날 것이므로 Soaking State를 Idle로 정리해줘야 한다.
            Module.SoakingModule().Clear_SoakingInfoTxt(true);
            LoggerManager.ActionLog(ModuleLogType.WAFER_LOAD, StateLogType.START, $"Lot ID: {Module.LotOPModule().LotInfo.LotName.Value}, Wafer ID: {Module.GetParam_Wafer().GetSubsInfo().WaferID.Value}", this.Module.LoaderController().GetChuckIndex());
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.RUNNING;

        private void WaitWaferLoadingOnTriPod()
        {
            try
            {
                int delayTimeOfDev = (this.Module.TempController().TempSafetyDevParam as ITempSafetyDevParam).WaferLoadDelay.Value;

                int delayTimeHighTempOfSys = (this.Module.TempController().TempSafetySysParam as ITempSafetySysParam).WaferLoad_HighTempSoakTime.Value;
                int delayTimeLowTempOfSys = (this.Module.TempController().TempSafetySysParam as ITempSafetySysParam).WaferLoad_LowTempSoakTime.Value;
                int waferLoad_SoakTempUpper = (this.Module.TempController().TempSafetySysParam as ITempSafetySysParam).WaferLoad_SoakTempUpper.Value;
                int waferLoad_SoakTempLower = (this.Module.TempController().TempSafetySysParam as ITempSafetySysParam).WaferLoad_SoakTempLower.Value;

                double setTemp = this.Module.TempController().TempInfo.SetTemp.Value;
                double curTemp = this.Module.TempController().TempInfo.CurTemp.Value;

                if (delayTimeOfDev > 0)
                {
                    LoggerManager.Debug($"[S] Wafer loading is waiting on the TriPod. DelayTimeOfDev : {delayTimeOfDev}, [DEV]");
                    Thread.Sleep(delayTimeOfDev);
                    LoggerManager.Debug($"[E] Wafer loading is waiting on the TriPod. DelayTimeOfDev : {delayTimeOfDev}, [DEV]");
                }
                else
                {
                    if (waferLoad_SoakTempUpper < waferLoad_SoakTempLower)
                    {
                        LoggerManager.Debug($"WaitWaferLoadingOnTriPod() not execute. Because WaferLoad_SoakTempUpper({waferLoad_SoakTempUpper}) is lower than WaferLoad_SoakTempLower({waferLoad_SoakTempLower})");
                        return;
                    }

                    if (setTemp > waferLoad_SoakTempUpper || curTemp > waferLoad_SoakTempUpper)
                    {
                        if (delayTimeHighTempOfSys > 0)
                        {
                            LoggerManager.Debug($"[S] Wafer loading is waiting on the TriPod. WaferLoad_HighTempSoakTime : {delayTimeHighTempOfSys}, [SYS]");
                            Thread.Sleep(delayTimeHighTempOfSys);
                            LoggerManager.Debug($"[E] Wafer loading is waiting on the TriPod. WaferLoad_HighTempSoakTime : {delayTimeHighTempOfSys}, [SYS]");
                        }
                        else
                        {
                            LoggerManager.Debug($"delayTimeHighTempOfSys is 0");
                        }
                    }
                    else if (setTemp < waferLoad_SoakTempLower || curTemp < waferLoad_SoakTempLower)
                    {
                        if (delayTimeLowTempOfSys > 0)
                        {
                            LoggerManager.Debug($"[S] Wafer loading is waiting on the TriPod. WaferLoad_LowTempSoakTime : {delayTimeLowTempOfSys}, [SYS]");
                            Thread.Sleep(delayTimeLowTempOfSys);
                            LoggerManager.Debug($"[E] Wafer loading is waiting on the TriPod. WaferLoad_LowTempSoakTime : {delayTimeLowTempOfSys}, [SYS]");
                        }
                        else
                        {
                            LoggerManager.Debug($"delayTimeLowTempOfSys is 0");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"WaitWaferLoadingOnTriPod() not execute. Because setTemp is {setTemp}, WaferLoad_SoakTempUpper is {waferLoad_SoakTempUpper}, WaferLoad_SoakTempLower is {waferLoad_SoakTempLower}");
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
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                Module.MonitoringManager().SkipCheckChuckVacuumFlag = false;

                var cardPodUpCheckcommand = new GP_CheckPCardPodIsDown();
                var retVal_CardPod = Module.CardChangeModule().BehaviorRun(cardPodUpCheckcommand).Result;
                
                if (retVal_CardPod != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GP_WaferLoadProcState Error] GP_CheckPCardPodIsDown RetVal:{retVal_CardPod}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                SubstrateSizeEnum TransferWafersize = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_GetTransferWaferSize();
                Module.LoaderController().SetTransferWaferSize((EnumWaferSize)TransferWafersize);

                bool UsingBernoulliTopHandler = Module.StageSupervisor().CheckUsingHandler(this.Module.LoaderController().GetChuckIndex());

                double armOffset = 0;

                //=> Stage move to loading position
                if ((Module.LoaderController() as GP_LoaderController).isExchange && Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    LoggerManager.ActionLog(ModuleLogType.WAFER_EXCHANGE, StateLogType.START, "", this.Module.LoaderController().GetChuckIndex());
                }
                else
                {
                    if (UsingBernoulliTopHandler == false)
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
                        // true = wafer not exist
                        retVal = Module.StageSupervisor().StageModuleState.MonitorForVacuum(true, CHUCK_VAC_MAINTAIN_TIME, CHUCK_VAC_CHECK_TIME);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "MonitorForVacuum(true): CHUCK_VAC_MAINTAIN_TIME", this.ToString());
                            LoggerManager.Debug($"[GP_WaferLoadProcState Error] (MonitorForVacuum: CHUCK_VAC_MAINTAIN_TIME) RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum On Check Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }

                        retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: true);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Vacuum Off: extraVacReady: true", this.ToString());
                            LoggerManager.Debug($"[GP_WaferLoadProcState Error] (VacuumOnOff(): val:false, extraVacReady: true) RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum Off Check Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }

                        retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(true, CHUCK_VAC_WAIT_TIME);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "WaitForVacuum(true): CHUCK_VAC_WAIT_TIME", this.ToString());
                            LoggerManager.Debug($"[GP_WaferLoadProcState Error] (MonitorForVacuum: CHUCK_VAC_WAIT_TIME) RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum Off Check Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }

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
                        LoggerManager.Debug($"[GP_WaferLoadProcState Error] (MoveLoadingPosition) RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Move Loading Position Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    }

                    if (loaderRetVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Loader MoveLoadingPosition Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferLoadProcState Error] (Loader MoveLoadingPosition) RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Move Loading Position Error in Loader to Cell{Module.LoaderController().GetChuckIndex()}");
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    }

                    if (retVal != EventCodeEnum.NONE || loaderRetVal != EventCodeEnum.NONE)
                    {
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    retVal = Module.StageSupervisor().StageModuleState.Handlerhold(CHUCK_THREELEG_WAIT_TIME);

                    if (retVal != EventCodeEnum.NONE && !(Module.LoaderController() as GP_LoaderController).isExchange) //exchange상황에서는 삼발이 업했는데도 또 하라고해서 에러날수도 있으니 조건을 추가함
                    {
                        if (UsingBernoulliTopHandler == false)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "3Pin Up Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferLoadProcState Error] (3Pin Up) RetVal:{retVal}");
                            // (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_SetWaferUnknownStatus(true, true);
                            // Module.StageSupervisor().WaferObject.SetStatusMissing();
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"3Pin Up Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                        else
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Bernoulli Transfer Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferLoadProcState Error] (Bernoulli Transfer error) Chuck -> Bernoulli  RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Bernoulli Transfer Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Handlerhold Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }
                }

                //=> Loader move to chuck up position.

                retVal = Module.StageSupervisor().StageModuleState.LoaderDoorOpen();

                if (retVal != EventCodeEnum.NONE)
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Shutter Door Error", this.ToString());
                    LoggerManager.Debug($"[GP_WaferLoadProcState Error] (LoaderDoorOpen error)  RetVal:{retVal}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Shutter Door Open Error in Cell{Module.LoaderController().GetChuckIndex()}");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                else
                {
                    if ((Module.LoaderController() as GP_LoaderController).isExchange == false)
                    {
                        (Module.LoaderController() as GP_LoaderController).LoaderDoorOpenTicks = DateTime.UtcNow.Ticks;
                    }
                }                

                // 웨이퍼 받기전에 삼발이 베큠 켜기.
                if (UsingBernoulliTopHandler == false) Module.IOManager().IO.Outputs.DO_TRILEG_SUCTION.SetValue();

                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_ChuckUpMove();
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Loader ChuckUPMove Error", this.ToString());
                    LoggerManager.Debug($"[GP_WaferLoadProcState Error] (WTR_ChuckUpMove) RetVal:{retVal}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Wafer Put Error in Loader");
                    retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(false);
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                TransferObject loadedObject;
                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyLoadedToThreeLeg(out loadedObject);
                if (retVal != EventCodeEnum.NONE)
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Notify Loaded To Wafer Error", this.ToString());
                    LoggerManager.Debug($"[GP_WaferLoadProcState Error] (WTR_NotifyLoadedToThreeLeg) RetVal:{retVal}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Put Error in Loader", "");

                    retVal = Module.StageSupervisor().StageModuleState.LoaderDoorClose();

                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (UsingBernoulliTopHandler)
                {
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_Notifyhandlerholdwafer(true);
                }

                int slotNum = loadedObject.OriginHolder.Index % 25;//로더가 Wafer를 Chuck에 Load하고 바로 저장
                int offset = 0;
                if (slotNum == 0)
                {
                    slotNum = 25;
                    offset = -1;
                }
                int foupNum = ((loadedObject.OriginHolder.Index + offset) / 25) + 1;

                if (loadedObject.WaferType.Value == EnumWaferType.POLISH)
                {
                    foupNum = this.Module.LotOPModule().LotInfo.FoupNumber.Value;
                    LoggerManager.Debug($"[WaferLoad Polish Wafer Load] Previous Load FoupNumber:{this.Module.LotOPModule().LotInfo.FoupNumber.Value}");
                }

                LoggerManager.Debug($"[WaferLoad] (Save_SlotInfo) Start");

                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferStatus = EnumSubsStatus.EXIST;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferType = loadedObject.WaferType.Value;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.OCRAngle = loadedObject.OCRAngle.Value;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferID = loadedObject.OCR.Value;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferSize = loadedObject.Size.Value;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.NotchType = loadedObject.NotchType;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.LoadingAngle = loadedObject.NotchAngle.Value;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.UnloadingAngle = loadedObject.SlotNotchAngle.Value;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.OriginSlotIndex = loadedObject.OriginHolder.Index;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.SlotIndex = slotNum;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.FoupIndex = foupNum;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferState = EnumWaferState.UNPROCESSED;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.OriginHolder = loadedObject.OriginHolder;
                (Module.StageSupervisor() as StageSupervisor).SlotInformation.CSTHashCode = loadedObject.CST_HashCode;

                var ret = (Module.StageSupervisor() as StageSupervisor).SaveSlotInfo();

                string cstHashCode = loadedObject.CST_HashCode;

                LoggerManager.Debug($"[WaferLoad] (Save_SlotInfo) Done RetVal:{ret}");

                if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING && UsingBernoulliTopHandler == false)
                {
                    try
                    {
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(true);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferStatus = EnumSubsStatus.UNKNOWN;
                        Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN, loadedObject.WaferType.Value, loadedObject.OCR.Value, loadedObject.OriginHolder.Index);
                        throw;
                    }
                }

                bool isLoaderDoorClose = true;
                retVal = Module.StageSupervisor().StageModuleState.LoaderDoorClose();                

                if (retVal != EventCodeEnum.NONE)
                {
                    Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Shutter Door Close Error", this.ToString());
                    LoggerManager.Debug($"[GP_WaferLoadProcState Error] (LoaderDoorClose error)  RetVal:{retVal}");
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Shutter Door Close Error in Cell{Module.LoaderController().GetChuckIndex()}");

                    isLoaderDoorClose = false;
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

                // 로드 후 대기
                WaitWaferLoadingOnTriPod();

                if (loadedObject != null)
                {
                    Module.PMIModule().SetPMITrigger(loadedObject.PMITirgger);
                }

                // Wafer TouchDwonCount 초기화
                Module.LotOPModule().LotInfo.TouchDownCount = 0;

                Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.EXIST, loadedObject.WaferType.Value, loadedObject.OCR.Value, loadedObject.OriginHolder.Index);

                if (Module.LotOPModule().TransferReservationAboutPolishWafer)
                {
                    //wafer가 load되면 Lot start를 할 수 있도록 flag를 off 한다.(polish wafer인지 standard인지 구분없이 wafer가 chuck에 배치되면 해제)
                    Module.LotOPModule().TransferReservationAboutPolishWafer = false;
                    LoggerManager.SoakingLog($"Wafer object({loadedObject.WaferType.Value.ToString()}) is put on chuck. so 'TransferReservationAboutPolishWafer' flag is off.");
                }

                Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.UNPROCESSED);

                if (Module.LoaderController().IsCancel == true)
                {
                    Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SKIPPED);
                }

                if (loadedObject.WaferType.Value == EnumWaferType.STANDARD)
                {
                    if (loadedObject.WaferState == EnumWaferState.PROCESSED)
                    {
                        Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.PROCESSED);
                    }
                    else if (loadedObject.WaferState == EnumWaferState.SKIPPED)
                    {
                        Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SKIPPED);
                    }
                    else if (loadedObject.WaferState == EnumWaferState.SOAKINGSUSPEND)
                    {
                        Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SOAKINGSUSPEND);
                    }
                    else if (loadedObject.WaferState == EnumWaferState.SOAKINGDONE)
                    {
                        Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SOAKINGDONE);
                    }
                }
                // Polish Wafer의 경우, Unload 때, Angle을 업데이트 해놓기 위해 기억해야 한다.
                else if (loadedObject.WaferType.Value == EnumWaferType.POLISH)
                {
                    if (loadedObject.PolishWaferInfo != null)
                    {
                        IPolishWaferSourceInformation TargetInfo = Module.StageSupervisor().WaferObject.GetPolishInfo();
                        //IPolishWaferInformation SourceInfo = Module.PolishWaferModule().GetPolishWaferInfo(loadedObject.PolishWaferInfo.DefineName.Value);

                        // 로더에서 설정되서 넘어온 데이터를 그대로 사용.
                        IPolishWaferSourceInformation SourceInfo = loadedObject.PolishWaferInfo;

                        if (SourceInfo != null)
                        {
                            TargetInfo.Copy(SourceInfo);
                        }
                        else
                        {
                            LoggerManager.Debug($"Not found polish wafer information. Please check parameter and definename value.");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"Polish Wafer information is null.");
                    }
                }

                try
                {
                    if (UsingBernoulliTopHandler == false)
                    {
                        //=> Chuck vacuum on
                        retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true, extraVacOn: false);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Chuck VaccumOn Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferLoadProcState Error] (VacuumOnOff) Value:true RetVal:{retVal}");
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum On Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }
                    else
                    {
                        if (Module.StageSupervisor().StageModuleState.IsHandlerholdWafer() == false)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Bernoulli Transfer Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferLoadProcState Error] (Bernoulli Transfer Error) RetVal:{EventCodeEnum.Wafer_Missing_Error}");

                            if (Module.LoaderController() is ILoaderControllerExtension)
                            {
                                (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SetWaferUnknownStatus(false, true);
                            }

                            Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                            Module.WaferTransferModule.NeedToRecovery = true;
                            Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);
                            if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {
                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(true);
                            }
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Bernoulli Transfer Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }

                    //=> Three leg down
                    retVal = Module.StageSupervisor().StageModuleState.Handlerrelease(CHUCK_THREELEG_WAIT_TIME);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        //Chuck io error => system error
                        if (UsingBernoulliTopHandler == false)
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "3Pin Down Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferLoadProcState Error] (3Pin Down) RetVal:{retVal}");

                            if (Module.LoaderController() is ILoaderControllerExtension)
                            {
                                (Module.LoaderController() as ILoaderControllerExtension).LoaderService.WTR_SetWaferUnknownStatus(false, true);
                            }

                            Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                            Module.WaferTransferModule.NeedToRecovery = true;
                            Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"3Pin Down Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                        else
                        {
                            Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Bernoulli Transfer Error", this.ToString());
                            LoggerManager.Debug($"[GP_WaferLoadProcState Error] (Bernoulli Transfer error) Bernoulli -> Chuck RetVal:{retVal}");
                            Module.WaferTransferModule.NeedToRecovery = true;
                            Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);
                            if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {
                                retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(true);
                            }
                            (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Bernoulli Transfer Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Handlerrelease Error in Cell{Module.LoaderController().GetChuckIndex()}");
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }
                    if (UsingBernoulliTopHandler)
                    {
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_Notifyhandlerholdwafer(false);
                        retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(true);
                    }

                    retVal = Module.StageSupervisor().CheckWaferStatus(true);

                    LoggerManager.Debug($"CheckWaferStatus: isExist = true, ret:{retVal.ToString()}");

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "CheckWaferStatus(true) Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferLoadProcState Error] SetWaferObjectStatus RetVal:{retVal}");

                        Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                        if (UsingBernoulliTopHandler == false)
                        {
                            retVal = Module.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: false);// 아래 쪽에서 웨이퍼 없는게 맞는지 확인하고 있으므로 extraVacReady: false

                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"Vacuumm off Error in WaferLodProc");
                                (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum On Error in Cell{Module.LoaderController().GetChuckIndex()}");
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }

                            System.Threading.Thread.Sleep(10000);

                            retVal = Module.StageSupervisor().CheckWaferStatus(false);
                            LoggerManager.Debug($"CheckWaferStatus: isExist = false, ret:{retVal.ToString()}");
                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"Error occured while prepare for recovery to three leg up");
                                (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum On Error in Cell{Module.LoaderController().GetChuckIndex()}");
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }

                            retVal = Module.StageSupervisor().StageModuleState.Handlerhold(CHUCK_THREELEG_WAIT_TIME);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                Module.WaferTransferModule.NeedToRecovery = true;
                                Module.LoaderController().SetRecoveryMode(Module.WaferTransferModule.NeedToRecovery);
                                LoggerManager.Error($"Need to recovery flag on");
                            }
                            else
                            {
                                LoggerManager.Debug($"Error occured while prepare for recovery to three leg up");
                            }
                        }
                      (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Chuck Vaccum On Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }

                    //=> Retract ARM

                    //=> Chuck wait for vacuum on
                    retVal = Module.StageSupervisor().StageModuleState.WaitForVacuum(false, CHUCK_VAC_WAIT_TIME);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[GP_WaferLoadProcState Error] (WaitForVacuum CHUCK_VAC_WAIT_TIME ) Value:false RetVal:{retVal}");
                    }

                    ProbeAxisObject zaxis = Module.MotionManager().GetAxis(EnumAxisConstants.Z);
                    retVal = Module.MotionManager().StageMove(0, 0, zaxis.Param.HomeOffset.Value);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Move To Center Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferLoadProcState Error] (MoveToCenter error)  RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Move To Center Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LoggerManager.Debug($"[GP_WaferLoadProcState Exception] Msg 0:{ex.Message}");
                }
                finally
                {
                    if (Module.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                    {
                        try
                        {
                            retVal = (Module.LoaderController() as GP_LoaderController).GPLoaderService.WTR_NotifyWaferTransferResult(true);
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            (Module.StageSupervisor() as StageSupervisor).SlotInformation.WaferStatus = EnumSubsStatus.UNKNOWN;
                            Module.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN, loadedObject.WaferType.Value, loadedObject.OCR.Value, loadedObject.OriginHolder.Index);
                            throw;
                        }
                    }
                }

                if (isLoaderDoorClose == false)
                {
                    retVal = Module.StageSupervisor().StageModuleState.LoaderDoorCloseRecovery();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(retVal, "Shutter Door Close Recovery Error", this.ToString());
                        LoggerManager.Debug($"[GP_WaferLoadProcState Error] (LoaderDoorClose error)  RetVal:{retVal}");
                        (Module.LoaderController() as GP_LoaderController).GPLoaderService.SetReasonOfError($"Wafer Load Error in Cell{Module.LoaderController().GetChuckIndex()}", $"Shutter Door Close Error in Cell{Module.LoaderController().GetChuckIndex()}");
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }

                Module.LotOPModule().LotInfo.UpdateWafer(Module.StageSupervisor().WaferObject);

                this.Module.LotOPModule().ModuleStopFlag = false;

                this.Module.GEMModule().GetPIVContainer().StageNumber.Value = this.Module.LoaderController().GetChuckIndex();
                this.Module.GEMModule().GetPIVContainer().SetWaferID(loadedObject.OCR.Value);

                if (!String.IsNullOrEmpty(loadedObject.LOTID) && this.Module.LotOPModule().LotInfo.LotName.Value.Equals(loadedObject.LOTID) == false)
                {
                    this.Module.LotOPModule().UpdateLotName(loadedObject.LOTID);
                }

                this.Module.LotOPModule().LotInfo.SetFoupInfo(foupNum, loadedObject.CST_HashCode);

                this.Module.GEMModule().GetPIVContainer().FoupNumber.Value = foupNum;
                this.Module.GEMModule().GetPIVContainer().SlotNumber.Value = slotNum;
                this.Module.GEMModule().GetPIVContainer().CurTemperature.Value = this.Module.TempController().TempInfo.CurTemp.Value;

                this.Module.GetParam_Wafer().GetSubsInfo().LoadingTime = DateTime.Now.ToLocalTime();

                Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.WAFERLOADINGTIME, this.Module.GetParam_Wafer().GetSubsInfo().LoadingTime.ToString());

                this.Module.ProbingModule().SetProbingEndState(ProbingEndReason.UNDEFINED);

                this.Module.GetParam_Wafer().GetSubsInfo().SlotIndex.Value = loadedObject.OriginHolder.Index;

                this.Module.LotOPModule().LotInfo.SetStageLotAssignState(LotAssignStateEnum.PROCESSING, loadedObject.LOTID, loadedObject.CST_HashCode);

                retVal = Module.ProbingModule().ResetOnWaferInformation();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GP_WaferLoadProcStates.RunningState], ResetOnWaferInformation() failed.");
                }

                if (Module.GetParam_Wafer().GetSubsInfo().WaferType == EnumWaferType.TCW)
                {
                    Module.StageSupervisor().StageModuleState.MoveTCW_Position();
                }
                else if (Module.StageSupervisor().StageMode != GPCellModeEnum.MAINTENANCE)
                {
                    if (Module.ResultMapManager().NeedDownload() && Module.ResultMapManager().CanDownload() && (loadedObject.WaferType.Value != EnumWaferType.POLISH))
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
                else
                {
                    SetDoneState();
                }

                if (Module.GetParam_Wafer().GetSubsInfo().WaferType != EnumWaferType.TCW && Module.StageSupervisor().Get_TCW_Mode() == TCW_Mode.ON)
                {
                    Module.StageSupervisor().Set_TCW_Mode(false);
                }

                var pivinfo = new PIVInfo() { FoupNumber = Module.GetParam_Wafer().GetOriginFoupNumber() };
                SemaphoreSlim semaphore = new SemaphoreSlim(0);

                if (Module.GetParam_Wafer().GetSubsInfo().WaferType == EnumWaferType.STANDARD)
                {
                    int foupnum = ((loadedObject.OriginHolder.Index - 1) / 25) + 1;
                    int slotnum = (loadedObject.OriginHolder.Index % 25 == 0) ? 25 : loadedObject.OriginHolder.Index % 25;
                    pivinfo = new PIVInfo(
                                                 foupnumber: foupnum,
                                                 slotnumber: slotnum,
                                                 stagenumber: this.Module.LoaderController().GetChuckIndex(),
                                                 curtemperature: Module.StageSupervisor().TempController().TempInfo.CurTemp.Value,
                                                 settemperature: Module.StageSupervisor().TempController().TempInfo.SetTemp.Value
                                                 ); ;

                    if (Module.LotOPModule().LotInfo.isNewLot)
                    {
                        Module.EventManager().RaisingEvent(typeof(StageAllocatedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                    else
                    {
                        Module.EventManager().RaisingEvent(typeof(WaferLoadedInCurrentLotEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }

                    semaphore = new SemaphoreSlim(0);
                    Module.EventManager().RaisingEvent(typeof(WaferLoadedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }
                else if (Module.GetParam_Wafer().GetSubsInfo().WaferType == EnumWaferType.POLISH)
                {
                    semaphore = new SemaphoreSlim(0);
                    Module.EventManager().RaisingEvent(typeof(PolishWaferLoadedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.WaferTransferModule.ReasonOfError.AddEventCodeInfo(EventCodeEnum.UNDEFINED, $"{err.Message}", this.ToString());
                StateTransition(new SystemErrorState(Module));
            }
            finally
            {
                //Module.LoaderController().UpdateIsNeedLotEnd();
            }
        }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }

    }


    public class SuspendedState : GP_WaferLoadProcState
    {
        public Task<EventCodeEnum> LoadResultMapTask;
        public SuspendedState(GP_WaferLoadProcModule module) : base(module) { }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.SUSPENDED;

        public override void Execute()
        {
            bool NoSequence = Module.ProbingSequenceModule().GetSequenceExistCurWafer() == EventCodeEnum.NONE ? true : false;
            if (LoadResultMapTask != null && LoadResultMapTask.IsCompleted)
            {
                if (LoadResultMapTask.Result == EventCodeEnum.NONE
                    && !LoadResultMapTask.IsCanceled
                    && !LoadResultMapTask.IsFaulted
                    && NoSequence)
                {
                    SetDoneState();
                }
                else
                {
                    Module.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SKIPPED);

                    string ocr = Module.GetParam_Wafer()?.GetSubsInfo()?.WaferID?.Value;
                    Module.LoaderController().BroadcastLotState(true);

                    if (NoSequence == true)
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.PROBING_SEQUENCE_INVALID_ERROR);
                        Module.MetroDialogManager().ShowMessageDialog("There is currently no die to test on the wafer", $"Wafer[{ocr}] is changed to the skipped state and returns to the origin position.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        Module.NotifyManager().Notify(EventCodeEnum.MAP_CONVERT_FAIL);
                        Module.MetroDialogManager().ShowMessageDialog("Failed to apply Result Map", $"Wafer[{ocr}] is changed to the skipped state and returns to the origin position.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
                    SetDoneState();
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

    public class DoneState : GP_WaferLoadProcState
    {
        public DoneState(GP_WaferLoadProcModule module) : base(module)
        {
            try
            {
                Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "WAFER LOAD DONE");

                LoggerManager.ActionLog(ModuleLogType.WAFER_LOAD, StateLogType.DONE, $"Lot ID: {Module.LotOPModule().LotInfo.LotName.Value}, Wafer ID: {Module.GetParam_Wafer().GetSubsInfo().WaferID.Value}", this.Module.LoaderController().GetChuckIndex());

                if ((module.LoaderController() as GP_LoaderController).isExchange && module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    LoggerManager.ActionLog(ModuleLogType.WAFER_EXCHANGE, StateLogType.DONE, "", module.LoaderController().GetChuckIndex());
                }

                Module.PinAligner().WaferTransferRunning = false; //wafer 가 chuck에 load 되었다면 wafer이송 flag를 off한다.

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            throw new NotImplementedException();
        }

    }

    public class SystemErrorState : GP_WaferLoadProcState
    {
        public SystemErrorState(GP_WaferLoadProcModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.WAFER_LOAD, StateLogType.ERROR, $"device:{Module.FileManager().GetDeviceName()}, ReasonOfError: {Module.WaferTransferModule.ReasonOfError.GetLastEventMessage()}", this.Module.LoaderController().GetChuckIndex());
            if ((module.LoaderController() as GP_LoaderController).isExchange && module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
            {
                LoggerManager.ActionLog(ModuleLogType.WAFER_EXCHANGE, StateLogType.ERROR, "", module.LoaderController().GetChuckIndex());
            }
        }

        public override WaferTransferProcStateEnum State => WaferTransferProcStateEnum.ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            //No WOKRS.
        }
    }

}

using System;
using System.Linq;
using System.Threading.Tasks;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberErrorCode;
using System.Threading;
using NotifyEventModule;
using LogModule;
using ProbingSequenceManager;
using SequenceRunner;
using MetroDialogInterfaces;
using ProberInterfaces.Event;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using ProberInterfaces.Param;
using ProberInterfaces.State;
using ProberInterfaces.PinAlign;
using ProberInterfaces.PMI;
using ProberInterfaces.PolishWafer;
using ProberInterfaces.Device;
using ProberInterfaces.NeedleClean;
using ProberInterfaces.WaferTransfer;
using LoaderController.GPController;

namespace LotOP
{
    #region // Lot OP states

    #endregion
    public abstract class GP_LotOPStateBase : LotOPState
    {
        private LotOPModule _Module;

        public LotOPModule Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public GP_LotOPStateBase(LotOPModule module) => Module = module;

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new GP_LotOPIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private LotStartValidations _LotStartValidations = new LotStartValidations();
        public LotStartValidations LotStartValidations
        {
            get { return _LotStartValidations; }
            set
            {
                if (value != _LotStartValidations)
                {
                    _LotStartValidations = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected bool IsCanStartLot()
        {
            bool retVal = true;
            string errorMsg = string.Empty;

            try
            {
                retVal = LotStartValidations.Valid(out errorMsg);

                if (!retVal)
                {
                    Module.MetroDialogManager().ShowMessageDialog("Lot Start Fail", errorMsg, EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if ((!Module.MonitoringManager().IsSystemError) && (!Module.MonitoringManager().IsStageSystemError))
                {
                    LoggerManager.Debug("LOTOPModule RunList End");
                    foreach (IStateModule module in Module.RunList)
                    {
                        if (!(module.ModuleState.State == ModuleStateEnum.PAUSED) && !(module.ModuleState.State == ModuleStateEnum.IDLE))
                        {
                            module.End();
                        }
                    }

                }
                else
                {
                    LoggerManager.Debug($"LOTOPModule End Failed.  System Error:{Module.MonitoringManager().IsSystemError} ,StageSystemError:{Module.MonitoringManager().IsStageSystemError} ");
                }
                retVal = EventCodeEnum.NONE;
                Module.PinAligner().WaferTransferRunning = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        protected void ClearDataBeforLot()
        {
            try
            {
                if (Module.ErrorEndState != ErrorEndStateEnum.NONE)
                {
                    Module.ErrorEndState = ErrorEndStateEnum.NONE;
                    LoggerManager.Debug("[LotOPState] ErrorEndState change to NONE");
                }

                if (Module.ModuleStopFlag != false)
                {
                    Module.ModuleStopFlag = false;
                    LoggerManager.Debug("[LotOPState] ModuleStopFlag change to false");
                }

                if (Module.GetErrorEndFlag() != false)
                {
                    Module.SetErrorEndFalg(false);
                    LoggerManager.Debug("[LotOPState] ErrorEndFalg change to false");
                }

                if (Module.LoaderController().IsCancel != false)
                {
                    Module.LoaderController().IsCancel = false;
                    LoggerManager.Debug("[LotOPState] IsCancel change to false");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class GP_LotOPIdleState : GP_LotOPStateBase
    {
        public GP_LotOPIdleState(LotOPModule module) : base(module)
        {
            try
            {
                Module.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);

                this.Module.CommandSendSlot.ClearToken();
                this.Module.CommandRecvSlot.ClearToken();
                Module.LotOPModule().LotInfo.isNewLot = true;
                Module.ErrorEndState = ErrorEndStateEnum.NONE;
                //여기에서 ErrorEnd 풀어줌.

                if (this.Module.LoaderController().GetconnectFlag()) //#Hynix_Merge 
                {
                    //#PAbort
                    Module.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.AVAILABLE);
                }

                if (Module.StageSupervisor().WaferObject.WaferStatus == EnumSubsStatus.EXIST && Module.StageSupervisor().WaferObject.GetWaferType() == EnumWaferType.TCW)
                {
                    Module.StageSupervisor().Set_TCW_Mode(true);
                }
                else
                {
                    Module.StageSupervisor().Set_TCW_Mode(false);
                }
                this.Module.ModuleStopFlag = false;
                (this.Module.ProbingSequenceModule() as ProbingSequenceModule).ProbingSequenceRemainCount = 0;

                Module.UpdateLotName(string.Empty);

                if (Module.LotOPModule().LotInfo.NeedLotDeallocated)
                {
                    Module.LotOPModule().LotInfo.NeedLotDeallocated = false;
                    LoggerManager.Debug("LotInfo.NeedLotDeallocated is set to false ");
                }

                if (Module.StageSupervisor().StageMode == GPCellModeEnum.ONLINE)
                {
                    // [STM_CATANIA] State 값을 받고 싶어하기 때문에 Enum을 추가해서 처리함.
                    Module.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.LOTOP_IDLE);
                }

                if (Module.LoaderController().IsCancel != false)
                {
                    Module.LoaderController().IsCancel = false;
                    LoggerManager.Debug($"[{this.GetType().Name}], InitData() : IsCancel change to false");
                }

                //<!-- Lot, Device 정보 초기화 -->
                Module.LotInfo.ClearLotInfos();
                Module.DeviceModule().RemoveSpecificReservationRecipe();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.CommandRecvSlot.Token is ISystemInit)
                {
                    Module.InnerStateTransition(new GP_LotOPReadyIdleToRunningState(Module));
                    return EventCodeEnum.NONE;
                }

                if (Module.StageSupervisor().GetStageLockMode() == StageLockMode.RESERVE_LOCK)
                {
                    ReasonOfStageMoveLock lastReason = Module.StageSupervisor().IStageMoveLockStatus.LastStageMoveLockReasonList.Last();
                    Module.StageSupervisor().SetStageLock(lastReason);
                }

                Func<bool> conditionFunc = () =>
                {
                    bool canRunning = false;

                    if (Module.SequenceEngineManager().GetRunState(true, false, true))
                    {
                        if (Module.StageSupervisor().IsModeChanging == false)
                        {
                            if (Module.StageSupervisor().StageMode != GPCellModeEnum.MAINTENANCE)
                            {
                                canRunning = true;
                            }
                        }
                    }
                        Module.LotStartFailReason = null;

                    Module.ProbingSequenceModule().ResetProbingSequence();

                    return canRunning;
                };

                Action doAction = () =>
                {
                    Module.InnerStateTransition(new GP_LotOPReadyIdleToRunningState(Module));
                };

                Action abortAction = () =>
                {
                };

                bool consumed;
                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();

                    if(Module.CommandRecvSlot.IsRequested<ILotOpStart>())
                    {
                        bool conditionCheck = conditionFunc();
                        if(conditionCheck)
                        {
                            consumed = Module.CommandManager().ProcessIfRequested<ILotOpStart>(
                                Module,
                                conditionFunc,
                                doAction,
                                abortAction);
                            if (consumed)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            LoggerManager.Debug($"Entry Point : Pause, Lot State : Idle.");

            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpStart | token is ISystemInit;



            return isValidCommand;
        }
    }

    public class GP_LotOPReadyIdleToRunningState : GP_LotOPStateBase
    {
        public GP_LotOPReadyIdleToRunningState(LotOPModule module) : base(module)
        {
            Module.NotifyManager().Notify(EventCodeEnum.LOT_START);
            Module.GEMModule().ClearAlarmOnly();
            Module.SoakingModule().Idle_SoakingFailed_PinAlign = false;
            Module.SoakingModule().Idle_SoakingFailed_WaferAlign = false;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.CommandRecvSlot.Token is ISystemInit)
                {
                    Task.Run(() =>
                    {
                        ErrorCodeResult systemInitResult = Module.StageSupervisor().SystemInit().Result;
                        
                        EventCodeEnum responseFromLoader = Module.LoaderController().ResponseSystemInit(systemInitResult.ErrorCode);

                        if (responseFromLoader == EventCodeEnum.NONE)
                        {
                            SequenceBehavior cardStuckRecovery = null;

                            //==> Recovery 동작을 수행 해여라
                            if (this.Module.CardChangeModule().GetCCType() == ProberInterfaces.CardChange.EnumCardChangeType.DIRECT_CARD)
                            {
                                cardStuckRecovery = new GP_PCardSutckRecovery();
                            }
                            else if (this.Module.CardChangeModule().GetCCType() == ProberInterfaces.CardChange.EnumCardChangeType.CARRIER)
                            {
                                cardStuckRecovery = new GOP_PCardSutckRecovery();
                            }

                            if (cardStuckRecovery != null)
                            {
                                var cardStuckRecoveryResult = cardStuckRecovery.Run().Result;

                                if (cardStuckRecoveryResult.ErrorCode != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Error($"GP_LotOPReadyIdleToRunningState(): retaval:{cardStuckRecoveryResult.ErrorCode},  CardChangeType: {this.Module.CardChangeModule().GetCCType()} ");
                                    retVal = cardStuckRecoveryResult.ErrorCode;
                                }
                            }
                        }
                        else
                        {
                            //==> Machine Init 수행 취소
                            retVal = responseFromLoader;
                            LoggerManager.Error($"GP_LotOPReadyIdleToRunningState(): retaval:{retVal},  ResponseSystemInit Fail from Loader  ");
                        }
                    });

                    Module.CommandRecvSlot.ClearToken();
                    Module.InnerStateTransition(new GP_LotOPIdleState(Module));
                    return retVal;
                }

                bool canTransitionToRunning;

                // status soakin이 Polish wafer를 사용한 Prepare Idle soaking running 중이라면 Lot Run으로 Soaking을 다시 진행 시 이어서 진행될 수 있도록 이전 data 유지 flag를 설정한다.
                //Module.SoakingModule().Check_N_KeepPreviousSoakingData();  //todo: 좀 더 보완 필요(추후 필요없으면 제거 예정)

                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();
                }

                if (Module.RunList.Count(item => item.ModuleState.GetState() == ModuleStateEnum.ERROR) > 0)
                {
                    Module.InnerStateTransition(new GP_LotOPPausingState(Module));
                }
                else
                {
                    canTransitionToRunning = Module.SequenceEngineManager().GetRunState(true, false, isWaferTransfer: true);

                    //TODO: 지금은 LotOpState가 Running으로 갈 때 Device Module만 Suspend인걸 확인하지만 추후에는 다른 모듈도 Suspend인 걸 확인하는 부분이 필요하다.
                    ModuleStateEnum devicemodulestate = Module.DeviceModule().ModuleState.GetState();

                    if (canTransitionToRunning && devicemodulestate != ModuleStateEnum.SUSPENDED)//Device Module이 Suspended이면 Lot Running가면 안됨.
                    {
                        Module.InitData();

                        if (IsCanStartLot())
                        {
                            if(Module.TempController().GetApplySVChangesBasedOnDeviceValue() == true)
                            {
                                if (Module.TempController().GetCurrentTempInfoInHistory()?.TempChangeSource != ProberInterfaces.Temperature.TemperatureChangeSource.TEMP_DEVICE)
                                {
                                    Module.TempController().SetTemperatureFromDevParamSetTemp();
                                }
                            }
                            else
                            {
                                //TODO: false인 경우 Host에서 보낸 SetTemp를 사용하도록 해야함. 현재는 false가 Host에서 변경한 값으로만 쓰고 있지만 추후 Enum으로 변경될 수도..
                            }


                            ClearDataBeforLot();

                            if (Module.GetParam_Wafer().WaferStatus == EnumSubsStatus.EXIST)
                            {
                                Module.GEMModule().GetPIVContainer().SetWaferID(Module.GetParam_Wafer().GetSubsInfo().WaferID.Value);
                                Module.LotOPModule().LotInfo.isNewLot = false;
                            }

                            Module.GEMModule().GetPIVContainer().UpdateStageLotInfo(Module.GEMModule().GetPIVContainer().FoupNumber.Value);

                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            Module.EventManager().RaisingEvent(typeof(LotStartEvent).FullName, new ProbeEventArgs(this, semaphore));
                            semaphore.Wait();

                            if (Module.LotOPModule().LotInfo.NeedLotDeallocated)
                            {
                                Module.LotOPModule().LotInfo.NeedLotDeallocated = false;
                                LoggerManager.Debug("LotInfo.NeedLotDeallocated is set to false. [LOT_START]");
                            }

                            Module.InnerStateTransition(new GP_LotOPRunningState(Module));

                            LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.START,
                                       $"Lot ID: {Module.LotOPModule().LotInfo.LotName.Value}, Device:{Module.FileManager().GetDeviceName()}," +
                                       $"Card ID:{Module.CardChangeModule().GetProbeCardID()}, OD:{Module.ProbingModule().OverDrive}, " +
                                       $"TouchDown Count: {Module.LotOPModule().SystemInfo.TouchDownCountUntilBeforeCardChange} "
                                       , this.Module.LoaderController().GetChuckIndex());

                            Module.LotInfo.ClearWaferSummary();
                            Module.LotInfo.LotEndTimeEnable = false;
                            Module.LotInfo.LotStartTimeEnable = true;
                            Module.LotInfo.LotStartTime = DateTime.Now;

                            Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.LOTSTARTTIME, Module.LotInfo.LotStartTime.ToString());

                            Module.LotInfo.ProcessedWaferCnt = 0;
                            Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.WAFERCOUNT, Module.LotOPModule().LotInfo.ProcessedWaferCnt.ToString());

                            Module.LotInfo.ProcessedDieCnt = 0;

                            Module.PolishWaferModule().InitTriggeredData();
                        }
                        else
                        {
                            Module.InnerStateTransition(new GP_LotOPPausingState(Module));
                        }

                        LoggerManager.Debug($"LOT START");
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.READYTORUNNING;
        }

        public override EventCodeEnum Pause()
        {
            LoggerManager.Debug($"Entry Point : Pause, Lot State : Idle.");

            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
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
    }

    public class GP_LotOPReadyPausedToRunningState : GP_LotOPStateBase
    {
        public GP_LotOPReadyPausedToRunningState(LotOPModule module) : base(module)
        {

        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal;

            try
            {
                //bool canTransitionToRunning;

                //canTransitionToRunning =
                //    Module.SequenceEngineManager().GetRunState();

                //if (canTransitionToRunning)
                //{

                //}

                if (Module.WaferTransferModule().ModuleState.State == ModuleStateEnum.RUNNING &&
                    Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST &&
                    Module.GetParam_Wafer().GetWaferType() == EnumWaferType.TCW)
                {
                    Module.InnerStateTransition(new GP_LotOPRunningState(Module));

                    Module.EventManager().RaisingEvent(typeof(LotResumeEvent).FullName);
                    Module.LotInfo.LotStartTime = DateTime.Now;
                }
                else if (IsCanStartLot())
                {
                    Module.InnerStateTransition(new GP_LotOPRunningState(Module));

                    Module.EventManager().RaisingEvent(typeof(LotResumeEvent).FullName);
                    Module.LotInfo.LotStartTime = DateTime.Now;

                    Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.LOTSTARTTIME, Module.LotInfo.LotStartTime.ToString());

                    LoggerManager.Debug($"[{Module.LotInfo.LotStartTime}] : LOT START");
                }
                else
                {
                    Module.InnerStateTransition(new GP_LotOPPausingState(Module));
                }

                Func<bool> conditionPauseFunc = () =>
                {
                    Module.Pause();

                    while (true)
                    {
                        bool canPause = Module.RunList.Count(item =>
                          item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                          item.ModuleState.GetState() == ModuleStateEnum.PENDING ||
                          item.ModuleState.GetState() == ModuleStateEnum.SUSPENDED) == 0;

                        if (canPause)
                        {
                            break;
                        }
                        else
                        {
                            foreach (IStateModule module in Module.RunList)
                            {
                                module.Execute();

                                module.Pause();
                            }
                        }

                        Thread.Sleep(1);
                    };

                    return true;
                };

                Action doPauseAction = () =>
                {
                    foreach (IStateModule module in Module.RunList)
                    {
                        module.Pause();
                    }
                };
                Action abortPauseAction = () => Module.MetroDialogManager().ShowMessageDialog("[LOTPAUSE]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);

                bool consumedPuase;


                foreach (IStateModule module in Module.RunList)
                {
                    //if (module.ModuleState.GetState() == ModuleStateEnum.ERROR)
                    //{
                    //    Module.ReasonOfError = module.ReasonOfError;
                    //    Module.CommandManager().SetCommand<ILotOpPause>(Module);
                    //}

                    consumedPuase = Module.CommandManager().ProcessIfRequested<ILotOpPause>(
                     Module,
                     conditionPauseFunc,
                     doPauseAction,
                     abortPauseAction);

                    if (consumedPuase)
                    {
                        break;
                    }

                    module.Execute();

                    if (module.ModuleState.GetState() == ModuleStateEnum.ERROR ||
                      module.ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                    {
                        // 이곳에서 포즈의 소스를 기록해두자. 
                        // 포즈가 완료된 후, 사용자에게 인폼을 해주기 위한 정보

                        Module.PauseSourceEvent = module.ReasonOfError.GetLastEventCode();

                        LoggerManager.Debug("Pause source event is occurred in lot state. ");
                        this.Module.LoaderController().BroadcastLotState(true);
                        Module.CommandManager().SetCommand<ILotOpPause>(Module);
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RESUMMING;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.READY_PAUSED_TO_RUNNING;
        }

        public override EventCodeEnum Pause()
        {

            Module.InnerStateTransition(new GP_LotOPPausingState(Module));

            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            isValidCommand = token is ILotOpPause ||
                                token is ILotOpEnd;

            return isValidCommand;
        }
    }

    public class GP_LotOPRunningState : GP_LotOPStateBase
    {
        public GP_LotOPRunningState(LotOPModule module) : base(module)
        {
            this.Module.ModuleStopFlag = false;
            // [STM_CATANIA] State 값을 받고 싶어하기 때문에 Enum을 추가해서 처리함.
            Module.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.LOTOP_RUNNING);
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            bool bCheckToken = false;
            bool consumedPuase;

            try
            {

                //Stopwatch stw = new Stopwatch();

                //stw.Start();

                Func<bool> conditionPauseFunc = () =>
                {
                    Module.Pause();

                    while (true)
                    {
                        bool canPause = Module.RunList.Count(item =>
                          item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                          item.ModuleState.GetState() == ModuleStateEnum.PENDING ||
                          item.ModuleState.GetState() == ModuleStateEnum.SUSPENDED) == 0;

                        if (canPause)
                        {
                            break;
                        }
                        else
                        {
                            foreach (IStateModule module in Module.RunList)
                            {
                                module.Execute();

                                module.Pause();
                            }
                        }

                        Thread.Sleep(1);
                    };

                    return true;
                };

                Action doPauseAction = () =>
                {
                    foreach (IStateModule module in Module.RunList)
                    {
                        module.Pause();
                    }
                };

                Action abortPauseAction = () => Module.MetroDialogManager().ShowMessageDialog("[LOTPAUSE]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);

                Func<bool> conditionEndFunc = () =>
                {
                    while (true)
                    {
                        bool canEnd = Module.RunList.Count(item =>
                          item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                          item.ModuleState.GetState() == ModuleStateEnum.PENDING ||
                          item.ModuleState.GetState() == ModuleStateEnum.SUSPENDED ||
                          item.ModuleState.GetState() == ModuleStateEnum.RECOVERY) == 0;

                        if (canEnd)
                        {
                            break;
                        }
                        else
                        {
                            foreach (IStateModule module in Module.RunList)
                            {
                                module.Execute();
                                if (module.ModuleState.State == ModuleStateEnum.RECOVERY || module.ModuleState.State == ModuleStateEnum.SUSPENDED)
                                {
                                    LoggerManager.Debug($"[Module End] Module:{module.ToString()} , ModuleState:{ module.ModuleState.State}");
                                    module.End();
                                }
                            }
                        }

                        Thread.Sleep(1);
                    };
                    return true;
                };

                Action doEndAction = () =>
                {
                    //Module.EventManager().RaisingEvent(typeof(LotEndEvent).FullName);

                    //while (true)
                    //{
                    //    bool LotEndFlag = true;

                    //    LotEndFlag = true;

                    //    foreach (var runJob in Module.RunList)
                    //    {
                    //        runJob.Execute();
                    //        if (runJob.ModuleState.GetState() != ModuleStateEnum.IDLE)
                    //        {
                    //            if (runJob.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                    //            {
                    //                runJob.Resume();
                    //            }
                    //            LotEndFlag = false;
                    //        }
                    //    }
                    //    if (Module.LoaderController().ModuleState.GetState() != ModuleStateEnum.IDLE)
                    //    {
                    //        LotEndFlag = false;
                    //    }

                    //    if(LotEndFlag == true)
                    //    {
                    //        break;
                    //    }

                    //    Module._delays.DelayFor(1);
                    //};
                    Module.InnerStateTransition(new GP_LotOPAborted(Module));
                };
                Action abortEndAction = () => Module.MetroDialogManager().ShowMessageDialog("[LOTEND]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                bool consumedEnd = false;


                if (Module.LoaderController()?.ModuleState.GetState() == ModuleStateEnum.ERROR)
                {
                    if (Module.StageSupervisor()?.WaferObject.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        if (Module.StageSupervisor()?.WaferObject.GetState() == EnumWaferState.PROCESSED)
                        {
                            bool isSucesses = Module.CommandManager().SetCommand<ILotOpPause>(Module);
                            if (isSucesses)
                            {
                            }
                        }
                    }
                    else
                    {
                        bool isSucesses = Module.CommandManager().SetCommand<ILotOpPause>(Module);
                        if (isSucesses)
                        {
                        }
                    }
                }

                if (Module.ErrorEndState == ErrorEndStateEnum.Reserve)
                {
                    Module.CommandManager().SetCommand<ILotOpPause>(Module);
                    Module.ErrorEndState = ErrorEndStateEnum.Processing;
                }

                if (Module.StageSupervisor().GetStageLockMode() == StageLockMode.RESERVE_LOCK)//#Hynix_Merge: 검토 필요, 위의 기능과 중복기능인지 확인 필요.
                {
                    if (Module.WaferTransferModule().ModuleState.State != ModuleStateEnum.PENDING && Module.WaferTransferModule().ModuleState.State != ModuleStateEnum.RUNNING)
                    {
                        Module.CommandManager().SetCommand<ILotOpPause>(Module);
                        LoggerManager.Debug("[SetCommand LOT PAUSE] StageLock State is Reserve Lock.");
                    }
                }

                foreach (IStateModule module in Module.RunList)
                {
                    /*
                       [ 커맨드 토큰 동작 로직 정리 ver 1.0 by Ralph ]

                       한 모듈이 다른 모듈로 커맨드 토큰을 발행하고 수행되는 과정에 대하여....
                       여기서는 NC와 Pin 그리고 프로빙 모듈간 동작 순서를 가지고 설명.

                       1) 프로빙 도중에 NC가 다이 인터벌로 발동, 이 때 프로빙은 GPIB에 의해서 커맨드 토큰을 받은 상태로 SUSPEND가 됨
                       2) NC동작이 시작되고 Pin align before NC 옵션에 의해 NC가 핀 모듈로 커맨드 토큰 발행, 그 후 SUSPEND 상태로 전환
                       3) Run List에서 프로빙 모듈이 NC보다 뒤에 있으므로 특별하게 처리해 주지 않으면 다음 틱에서 pin이 수행되기 전에 프로빙 모듈이 끼어 들게 됨
                       4) 이를 방지하고자 프로빙 모듈인 경우에 한해서 동작 전에 매번 다른 모듈들에 발행된 토큰이 있는지 확인 후 동작
                       5) 만약 다른 모듈에 토큰이 발행되어 있다면 이번 틱에서 프로빙 모듈의 동작은 스킵
                       6) 즉, 프로빙 모듈은 항상 토큰들 간 우선순위에서 가장 후위에 놓이게 되며 한 번에 다수개의 토큰이 발행되는 상황이 벌어진다면
                          그것들을 처리하는 순서는 Run List를 따름
                       7) 추가적으로 NC가 발행한 커맨드 토큰에 대해 핀이 동작을 끝낸 순간 NC 모듈의 턴에서 자신의 상태를 SUSPEND에서 RUNNING으로 전환하고 턴을
                          끝내게 되는데, 이 때 다음 턴인 프로빙 모듈이 이미 SUSPEND에서 RUNNING으로 전환되어 있는 상황이 되어 프로빙 모듈이 끼어드는 것을 방지하기
                          위하여 다른 모듈이 SUSPEND나 혹은 RUNNING이 있는지 확인하는 로직을 추가함


                       3줄요약
                       
                       1. 프로빙 모듈의 토큰은 우선순위에서 가장 뒤쪽에 놓인다. 
                       2. 프로빙 모듈을 제외한 다른 모듈 중 어느 하나가 SUSPEND 혹은 RUNNING인 상태일 경우 프로빙 모듈은 대기 상태가 된다.
                       3. 커맨드 토큰의 동작 순서는 Run List에 있는 모듈의 동작 순서를 따른다. 따라서 먼저 수행되어야 할 우선순위 대로 Run List를 구성해야 한다.

                    */

                    //Lot Start 를 알기위해 ==
                    Module.LotStartFlag = true;
                    //=======================

                    if (module != null)
                    {
                        if (module is IProbingModule)
                        {
                            bCheckToken = false;

                            foreach (IStateModule runModule in Module.RunList)
                            {
                                if (runModule != null)
                                {
                                    if (!(runModule is IProbingModule))
                                    {
                                        if (!runModule.CommandRecvSlot.IsNoCommand()
                                            || !(runModule.CommandRecvProcSlot?.IsNoCommand() ?? true)
                                            || runModule.ModuleState.GetState() == ModuleStateEnum.SUSPENDED
                                            || runModule.ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                        {
                                            bCheckToken = true;
                                            if (Module.SoakingModule().ChillingTimeMngObj.IsShowDebugString())
                                            {
                                                Trace.WriteLine($"[ShowDebugStr] GP_LotOP >> RecvSlot:{runModule.CommandRecvSlot.IsNoCommand().ToString()},{runModule.CommandRecvSlot.GetState().ToString()}, " +
                                                    $"runModule:{runModule.ToString()}, RecvProcSlot:{(runModule.CommandRecvProcSlot?.IsNoCommand() ?? true).ToString()}, " +
                                                    $"runModule.ModuleState.GetState:{runModule.ModuleState.GetState().ToString()}");

                                                if (null != runModule.CommandRecvProcSlot && false == runModule.CommandRecvProcSlot?.IsNoCommand())
                                                {
                                                    Trace.WriteLine($"[ShowDebugStr] GP_LotOP >>  CommandRecvProcSlot:{runModule.CommandRecvProcSlot.GetState().ToString()}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (bCheckToken == true)
                            continue;

                        module.Execute();

                        Module.VisionManager().AllStageCameraStopGrab();

                        // LOT가 돌리는 모듈 중, 해당 모듈의 STATE가 ERROR 또는 PAUSED 또는 RECOVERY가 됐을 때,
                        // LOT PAUSE 커맨드 발행.
                        if (Module.CanLotPauseState(module.ModuleState.GetState())) // lot Pause 상태를 만들수 있는 상태인지 확인하는 함수로 대체 -> RunList에서 해당 상태에 포함되는 모듈 있으면 List에 담는다. 
                        {
                            // 이곳에서 포즈의 소스를 기록해두자. 
                            // 포즈가 완료된 후, 사용자에게 인폼을 해주기 위한 정보

                            Module.PauseSourceEvent = module.ReasonOfError.GetLastEventCode();

                            LoggerManager.Debug("Pause source event is occurred in lot state. ");
                            this.Module.LoaderController().BroadcastLotState(true);
                            Module.CommandManager().SetCommand<ILotOpPause>(Module);
                        }
                    }

                    consumedPuase = Module.CommandManager().ProcessIfRequested<ILotOpPause>(
                        Module,
                        conditionPauseFunc,
                        doPauseAction,
                        abortPauseAction);
                    if (consumedPuase)
                    {
                        break;
                    }
                    consumedEnd = Module.CommandManager().ProcessIfRequested<ILotOpEnd>(
                        Module,
                        conditionEndFunc,
                        doEndAction,
                        abortEndAction);
                    if (consumedEnd)
                    {
                        break;
                    }
                }

                if (consumedEnd == false)
                {
                    if (Module.IsNeedLotEnd)
                    {
                        Module.LotOPModule().ModuleStopFlag = true;
                        //Module.LoaderController().SetAbort(true);// Loader에서 셀을 다시 Start 시키지 않기 위해서 Abort 처리.
                        Module.CommandManager().SetCommand<ILotOpEnd>(this);
                        LoggerManager.Debug($"[GP_LotOPRunningState] SetCommand<ILotOpEnd>(): Reason: IsNeedLotEnd:{Module.IsNeedLotEnd}");
                    }

                    consumedEnd = Module.CommandManager().ProcessIfRequested<ILotOpEnd>(
                        Module,
                        conditionEndFunc,
                        doEndAction,
                        abortEndAction);
                }


                if (consumedEnd == true)
                {
                    Module.LotInfo.LotEndTime = DateTime.Now.ToLocalTime();

                    Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.LOTENDTIME, Module.LotInfo.LotEndTime.ToString());

                    LoggerManager.Debug($"[{DateTime.Now}] : LOT END");
                }

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            Module.InnerStateTransition(new GP_LotOPPausingState(Module));
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = token is ILotOpPause ||
                token is ILotOpEnd ||
                token is IUnloadAllWafer;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }
    }
    public class GP_LotOPPausingState : GP_LotOPStateBase
    {
        public LotOPStateEnum PausingSource { get; set; }

        public GP_LotOPPausingState(LotOPModule module) : base(module)
        {
            this.PausingSource = module.LotStateEnum;            

        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool canTransitionToPaused = false;
                int pauseCountModule = 0;

                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();

                    // 스테이트의 꼬임(?) 또는 정상적인 시퀀스 흐름에서 모듈의 스테이트가 포즈가 안됐을 수 있다.
                    // 다시 호출해주자.
                    module.Pause();
                }

                pauseCountModule = Module.RunList.Count
                (item => item.ModuleState.GetState() != ModuleStateEnum.PAUSED &&
                         item.ModuleState.GetState() != ModuleStateEnum.ERROR &&
                         item.ModuleState.GetState() != ModuleStateEnum.SUSPENDED &&
                         item.ModuleState.GetState() != ModuleStateEnum.RECOVERY);
                canTransitionToPaused = (pauseCountModule == 0);

                if (canTransitionToPaused)
                {
                    string fail_reason;
                    
                    if (CheckPossibleToMoveToHeatingPosition(Module.PauseSourceEvent.ModuleType, out fail_reason))
                    {
                        retVal = MoveToHeatingPosition(); // 한번만 동작한다. 

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"GP_LotOPPausingState(): Move heating position failed.");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"GP_LotOPPausingState(): It is impossible to move to the heating position. Reason:{fail_reason}");
                    }

                    // [STM_CATANIA] State 값을 받고 싶어하기 때문에 Enum을 추가해서 처리함.
                    Module.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.LOTOP_PAUSED);

                    Module.InnerStateTransition(new GP_LotOPPausedState(Module, this.PausingSource));

                    Module.EventManager().RaisingEvent(typeof(LotPausedEvent).FullName);
                    //  LoggerManager.Debug($"[{DateTime.Now}] : LOT PAUSED");
                    LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.PAUSE, $"device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}, OD:{Module.ProbingModule().OverDrive}", this.Module.LoaderController().GetChuckIndex());
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSING;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.PAUSING;
        }

        public override EventCodeEnum Pause()
        {
            LoggerManager.Debug($"Already paused.");

            return EventCodeEnum.NONE;
        }

        private bool CheckPossibleToMoveToHeatingPosition(ModuleEnum errorModule, out string reason)
        {            
            bool retModule = true;
            bool retMonitor = false;
            reason = "";
            try
            {

                if((errorModule is ModuleEnum.PinAlign ||
                    errorModule is ModuleEnum.WaferAlign ||
                    errorModule is ModuleEnum.PMI ||
                    errorModule is ModuleEnum.PolishWafer ||
                    errorModule is ModuleEnum.Lot) == false)//유저가 Pause 했을때 LotModule이 Pause됨.
                {
                    retModule = false;
                    reason += $"There is not allowed Module to go to the Soaking Position, ErrorModule: {errorModule}, ";
                }



                bool IsMachineInitDone = Module.MonitoringManager().IsMachineInitDone;
                AlignStateEnum markAlignState = Module.StageSupervisor().MarkObject.AlignState.Value;
                EnumSysState sysState = Module.SysState().GetSysState();
                StageStateEnum moveState = Module.StageSupervisor().StageModuleState.GetState();
                StageLockMode lockState = Module.StageSupervisor().GetStageLockMode();
                EnumProbingState probingStateEnum = Module.ProbingModule().ProbingStateEnum;
                bool isSystemError = Module.MonitoringManager().IsSystemError;
                EnumSubsStatus waferStatus = Module.StageSupervisor().WaferObject.GetStatus();
                bool isCardExist = Module.CardChangeModule().IsExistCard();
                EventCodeEnum checkCardModuleThreeLeg = Module.SoakingModule().CheckCardModuleAndThreeLeg();

                if (IsMachineInitDone == true
                   && markAlignState == AlignStateEnum.DONE
                   && sysState == EnumSysState.IDLE
                   && moveState != StageStateEnum.Z_UP
                   && moveState != StageStateEnum.CARDCHANGE               
                   && moveState != StageStateEnum.PROBING
                   && lockState == StageLockMode.UNLOCK
                   && probingStateEnum != EnumProbingState.ZUP
                   && probingStateEnum != EnumProbingState.ZUPDWELL
                   && isSystemError == false
                   && waferStatus != EnumSubsStatus.UNKNOWN
                   && isCardExist
                   && checkCardModuleThreeLeg == EventCodeEnum.NONE)
                {
                    retMonitor = true;
                }
                else
                {
                    if (!IsMachineInitDone)
                    {
                        reason += $"IsMachineInitDone:{IsMachineInitDone}, ";
                    }
                    if (markAlignState != AlignStateEnum.DONE)
                    {
                        reason += $"Mark AlignState:{markAlignState}, ";
                    }
                    if (sysState != EnumSysState.IDLE)
                    {
                        reason += $"SysState:{sysState}, ";
                    }
                    if (moveState == StageStateEnum.Z_UP ||
                        moveState == StageStateEnum.CARDCHANGE ||
                        moveState == StageStateEnum.PROBING)
                    {
                        reason += $"StageModuleState:{moveState}, ";
                    }
                    if (lockState != StageLockMode.UNLOCK)
                    {
                        reason += $"LockMode:{lockState}, ";
                    }
                    if (probingStateEnum == EnumProbingState.ZUP ||
                        probingStateEnum == EnumProbingState.ZUPDWELL)
                    {
                        reason += $"ProbingStateEnum:{probingStateEnum}, ";
                    }
                    if (isSystemError)
                    {
                        reason += $"IsSystemError:{isSystemError}, ";
                    }
                    if (waferStatus == EnumSubsStatus.UNKNOWN)
                    {
                        reason += $"WaferStatus:{waferStatus}, ";
                    }
                    if (!isCardExist)
                    {
                        reason += $"CardStatus:{isCardExist}, ";
                    }
                    if (checkCardModuleThreeLeg != EventCodeEnum.NONE)
                    {
                        reason += $"CheckCardModuleAndThreeLeg:{checkCardModuleThreeLeg}";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retMonitor && retModule;
        }

        private EventCodeEnum MoveToHeatingPosition()//TODO: 한번만 동작하도록 처리 필요.
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;        

            try
            {
                RetVal = Module.StageSupervisor().StageModuleState.ZCLEARED();

                if (RetVal != EventCodeEnum.NONE)
                {
                    return RetVal;
                }

                RetVal = Module.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                
                if (RetVal != EventCodeEnum.NONE)
                {
                    return RetVal;
                }

                bool isEnableStatusSoaking = false;
                bool ShowToggleFlag = Module.SoakingModule().StatusSoakingParamIF.Get_ShowStatusSoakingSettingPageToggleValue();
                bool IsGettingOptionSuccessul = Module.SoakingModule().StatusSoakingParamIF.IsEnableStausSoaking(ref isEnableStatusSoaking);

                if (ShowToggleFlag && IsGettingOptionSuccessul && isEnableStatusSoaking)
                {
                    WaferCoordinate wafercoord = new WaferCoordinate();
                    PinCoordinate pincoord = new PinCoordinate();
                    MachineCoordinate targetpos = new MachineCoordinate();

                    RetVal = Module.SoakingModule().Get_StatusSoakingPosition(ref wafercoord, ref pincoord, use_chuck_focusing: true, logWrite: true);
                    
                    if (RetVal != EventCodeEnum.NONE) 
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] Get_StatusSoakingPosition(), failed. retVal:{RetVal}");// Chuck Focusing 하는 경우 Status Soak OD 못찾아서 PARAM_ERROR로 리턴할 수도 있음.  
                        return RetVal;
                    }
               
                    double get_overdrive;
                    RetVal = Module.SoakingModule().Get_MaintainSoaking_OD(out get_overdrive);
                    
                    if (RetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] Get_MaintainSoaking_OD(), failed. retVal:{RetVal}");
                        return RetVal;
                    }

                    LoggerManager.Debug($"[{this.GetType().Name}] GetSoakingPosition(), WaferObject.AlignState = {Module.StageSupervisor().WaferObject.AlignState.Value}, ProbeCardInfo.AlignState = {Module.StageSupervisor().ProbeCardInfo.AlignState.Value}");
                    LoggerManager.Debug($"[{this.GetType().Name}] GetSoakingPosition(), wafercoord(X, Y ,Z) = ({wafercoord.X.Value:0.00}, {wafercoord.Y.Value:0.00}, {wafercoord.Z.Value:0.00})");
                    LoggerManager.Debug($"[{this.GetType().Name}] GetSoakingPosition(), pincoord(X, Y ,Z) = ({pincoord.X.Value:0.00}, {pincoord.Y.Value:0.00}, {pincoord.Z.Value:0.00})");
                    LoggerManager.Debug($"[{this.GetType().Name}] GetSoakingPosition(), od = ({get_overdrive:0.00})");
                    LoggerManager.Debug($"[{this.GetType().Name}] GetSoakingPosition(), Ref Info: wafer maxThickness = ({Module.StageSupervisor().WaferMaxThickness:0.00}), actualThickness = ({Module.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness:0.00}), " +
                                                                                                $"wafer cen(X, Y)= ({Module.StageSupervisor().WaferObject.GetSubsInfo().WaferCenter.X.Value:0.00}, {Module.StageSupervisor().WaferObject.GetSubsInfo().WaferCenter.Y.Value:0.00})," +
                                                                                                $"pin reg min = ({Module.CoordinateManager().StageCoord.PinReg.PinRegMin.Value:0.00})," +
                                                                                                $"pin cen(X, Y) = ({Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX:0.00}, {Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY:0.00})," +
                                                                                                $"dut cen(X, Y) = ({Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX:0.00}, {Module.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY:0.00})");

                    RetVal = Module.StageSupervisor().StageModuleState.MoveToSoaking(wafercoord, pincoord, get_overdrive);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;

        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {
                isValidCommand =
                    token is IUnloadAllWafer;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class GP_LotOPPausedState : GP_LotOPStateBase
    {
        private DateTime RunningTime = DateTime.Now;
        private bool occuralarm = false;

        private LotOPStateEnum PausingSource { get; set; }

        public GP_LotOPPausedState(LotOPModule module, LotOPStateEnum pausingSource) : base(module)
        {
            try
            {
                this.PausingSource = pausingSource;

                Module.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                // [YMTC] 모든 ALID를 보고 해야함. 일단 모든 Error에 대해서 Stage Error로 무조건 보고한다.
                Module.NotifyManager().Notify(EventCodeEnum.STAGE_ERROR_OCCUR);

                if (Module.ProbingModule().IsReservePause == true)
                {
                    Module.ProbingModule().IsReservePause = false;
                }
                //Task.Run((Action)(() =>
                //{
                //    Module.ViewModelManager().UnLockViewControl((int)this.Module.GetHashCode());
                //}));
                if (Module.ErrorEndState == ErrorEndStateEnum.Processing)
                {
                    /// Leina 2024.01.17 V22.1
                    if (Module.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                    {
                        if (this.Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST
                            && this.Module.StageSupervisor().StageMoveState != StageStateEnum.Z_UP)// #Hynix_Merge  이부분 Dev_Integrated랑 다름.  PAbort 하면서 수정한건데 ErrorEnd 보완해야할 부분.
                        {
                            Module.ErrorEndState = ErrorEndStateEnum.Unload;
                        }
                    }
                    else
                    {    /// Z_UP 상태 보고 있으면 host 로 부터 강제 종료 명령시 wafer unload 안되는 상황 있음
                        if (this.Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                        {
                            Module.ErrorEndState = ErrorEndStateEnum.Unload;
                        }
                    }

                       
                    this.Module.LotOPModule().ModuleStopFlag = false;
                    this.Module.MetroDialogManager().ShowMessageDialog("[LOT Pause]", $"Error End. Wafer:{this.Module.GetParam_Wafer().GetStatus()}., Time:" + DateTime.Now, EnumMessageStyle.Affirmative);

                }
                this.Module.LoaderController().SetProbingStart(false);

                if (Module.PauseSourceEvent != null && Module.PauseSourceEvent.Checked == false && module.LoaderController().IsLotOut == false)
                {
                    Module.MetroDialogManager().ShowMessageDialog($"[LOT PAUSE]",
                        $"Occurred time : {Module.PauseSourceEvent.OccurredTime}\n" +
                        $"Occurred location : {Module.PauseSourceEvent.ModuleType}\n" +
                        $"Reason : {Module.PauseSourceEvent.Message}", EnumMessageStyle.Affirmative);

                    //Module.MetroDialogManager().ShowMessageDialog("[LOTPAUSE]" + DateTime.Now, "[" + Module.ReasonOfError.ModuleType + "] Reason=" + Module.ReasonOfError.Reason, EnumMessageStyle.Affirmative);
                }

                if ((Module.LoaderController() as GP_LoaderController).GPLoaderService != null)
                {
                    this.Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.LOT);
                    (Module.LoaderController() as GP_LoaderController).GPLoaderService?.UploadRecentLogs(Module.LoaderController().GetChuckIndex());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.StageSupervisor().GetStageLockMode() == StageLockMode.RESERVE_LOCK)
                {
                    ReasonOfStageMoveLock lastReason = Module.StageSupervisor().IStageMoveLockStatus.LastStageMoveLockReasonList.Last();
                    Module.StageSupervisor().SetStageLock(lastReason);
                }

                bool isExecuted;

                foreach (IStateModule module in Module.RunList)
                {
                    module.Execute();
                }

                //=> Process LotResume
                Func<bool> conditionFunc = () =>
                {
                    bool isInjected = true;

                    if (Module.TempController().ModuleState.GetState() == ModuleStateEnum.ERROR)
                    {
                        Module.TempController().Resume();
                    }

                    //isInjected = isInjected && Module.CommandManager().SetCommand<IFoupOpStart>(Module);
                    return isInjected;
                };
                Action doAction = () =>
                {
                    if (IsCanStartLot())
                    {
                        if (Module.TempController().GetApplySVChangesBasedOnDeviceValue() == true)
                        {
                            if (Module.TempController().GetCurrentTempInfoInHistory()?.TempChangeSource != ProberInterfaces.Temperature.TemperatureChangeSource.TEMP_DEVICE)
                            {
                                Module.TempController().SetTemperatureFromDevParamSetTemp(); 
                            }
                        }
                        else
                        {
                            //TODO: false인 경우 Host에서 보낸 SetTemp를 사용하도록 해야함. 현재는 false가 Host에서 변경한 값으로만 쓰고 있지만 추후 Enum으로 변경될 수도..
                        }
                        

                        if (Module.ErrorEndState == ErrorEndStateEnum.Reserve)
                        {
                            if (this.Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                            {
                                Module.ErrorEndState = ErrorEndStateEnum.Unload;
                                this.Module.LotOPModule().ModuleStopFlag = false;
                                Module.CommandManager().SetCommand<ILotOpPause>(Module);
                            }
                            else
                            {
                                foreach (IStateModule module in Module.RunList)
                                {
                                    module.ReasonOfError.Confirmed();
                                }
                                Module.ErrorEndState = ErrorEndStateEnum.NONE;
                                Module.Resume();
                                Module.GEMModule().ClearAlarmOnly();
                            }
                        }
                        else
                        {
                            foreach (IStateModule module in Module.RunList)
                            {
                                module.ReasonOfError.Confirmed();
                            }
                            Module.ErrorEndState = ErrorEndStateEnum.NONE;
                            Module.Resume();
                            Module.GEMModule().ClearAlarmOnly();
                        }
                    }
                };

                Action abortAction = () => Module.MetroDialogManager().ShowMessageDialog("[LOTRESUME]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);

                isExecuted = Module.CommandManager().ProcessIfRequested<ILotOpResume>(
                    Module,
                    conditionFunc,
                    doAction,
                    abortAction);

                //=> Process LotEnd
                conditionFunc = () =>
                {
                    bool isInjected = true;
                    //    isInjected = (Module.ErrorEndState == ErrorEndStateEnum.NONE || Module.ErrorEndState == ErrorEndStateEnum.DONE);
                    return isInjected;
                };
                doAction = () =>
                {
                    this.End();

                    foreach (IStateModule module in Module.RunList)
                    {
                        module.End();
                    }
                };
                abortAction = () => Module.MetroDialogManager().ShowMessageDialog("[LOTEND]", "FAIL", EnumMessageStyle.Affirmative);

                var consumedEnd = Module.CommandManager().ProcessIfRequested<ILotOpEnd>(
                    Module,
                    conditionFunc,
                    doAction,
                    abortAction);

                isExecuted = Module.CommandManager().ProcessIfRequested<IUnloadAllWafer>(
                    Module,
                    conditionFunc,
                    doAction,
                    abortAction);

                //v22_merge// hynix 버전에만 존재하는 코드, alllot flag 필요 여부 확인 필요, hynix 기준으로 우선 merge함
                if (Module.LoaderController().IsLotOut == true &&
                  Module.LoaderController().IsAbort == false &&
                  Module.StageSupervisor().StageMoveState != StageStateEnum.Z_UP)
                {
                    Module.LoaderController().SetAbort(true);
                    Module.LotInfo.NeedLotDeallocated = true;
                    Module.CommandManager().SetCommand<ILotOpEnd>(this);
                }

                //Module.LoaderController().UpdateIsNeedLotEnd();
                if (Module.CommandRecvSlot.Token is ILotOpEnd == false && Module.IsNeedLotEnd)
                {
                    Module.LotOPModule().ModuleStopFlag = true;
                    //Module.LoaderController().SetAbort(true);// Loader에서 셀을 다시 Start 시키지 않기 위해서 Abort 처리.
                    Module.CommandManager().SetCommand<ILotOpEnd>(this);
                    LoggerManager.Debug($"[GP_LotOPPausedState] SetCommand<ILotOpEnd>(): Reason: IsNeedLotEnd:{Module.IsNeedLotEnd}");
                }

                if (consumedEnd == true)
                {
                    Module.LotInfo.LotEndTime = DateTime.Now.ToLocalTime();
                    Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.LOTENDTIME, Module.LotInfo.LotEndTime.ToString());

                    LoggerManager.Debug($"[{DateTime.Now}] : LOT END_(ABORT)");
                }
                //=> Process UnloadWafer (Reasult Map DN fail 후 sk() => true;

                #region <remarks> Check Paused Time </remarks>
                if (Module.GetLotPauseTimeoutAlarm() != 0)
                {
                    DateTime curDateTime = DateTime.Now;
                    TimeSpan ts = curDateTime - RunningTime;
                    if (Module.GetLotPauseTimeoutAlarm() <= ts.TotalSeconds)
                    {
                        if (occuralarm == false)
                        {
                            // Occur timeout alarm.
                            occuralarm = true;
                            LoggerManager.Debug($"Occur LOT_PAUSE_TIMEOUT. Set Time : {Module.GetLotPauseTimeoutAlarm()}(sec), Cur Time : {ts.TotalSeconds}(sec)");
                            Module.NotifyManager().Notify(EventCodeEnum.LOT_PAUSE_TIMEOUT);
                        }
                    }
                }
                #endregion

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.PAUSED;
        }

        public override EventCodeEnum Pause()
        {
            LoggerManager.Debug($"Already paused.");

            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            isValidCommand =
                token is ILotOpResume ||
                token is ILotOpEnd ||
                token is IUnloadAllWafer;

            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new GP_LotOPAborted(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.RESUME, $"device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}, OD:{Module.ProbingModule().OverDrive}", this.Module.LoaderController().GetChuckIndex());
                
                if(PausingSource == LotOPStateEnum.READYTORUNNING)
                {
                    Module.InnerStateTransition(new GP_LotOPReadyIdleToRunningState(Module));
                }
                else
                {
                    Module.InnerStateTransition(new GP_LotOPReadyPausedToRunningState(Module));
                }

                foreach (var runJob in Module.RunList)
                {
                    runJob.Resume();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class GP_LotOPErrorState : GP_LotOPStateBase
    {
        public GP_LotOPErrorState(LotOPModule module) : base(module)
        {
            if ((Module.LoaderController() as GP_LoaderController).GPLoaderService != null)
            {
                this.Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.LOT);
                (Module.LoaderController() as GP_LoaderController).GPLoaderService?.UploadRecentLogs(Module.LoaderController().GetChuckIndex());
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            throw new NotImplementedException();
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new GP_LotOPIdleState(Module));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LOT ERROR] Current State: {this.GetModuleState()}, Exception:{err.ToString()} ");
                Module.InnerStateTransition(new GP_LotOPErrorState(Module));
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpEnd || token is ILotOpStart;
            return isValidCommand;
        }
    }
    public class GP_LotOPAborted : GP_LotOPStateBase
    {
        public GP_LotOPAborted(LotOPModule module) : base(module)
        {
            this.Module.ModuleStopFlag = false;
            Module.GEMModule().ClearAlarmOnly();
            Module.PinAligner().WaferTransferRunning = false;
            // [STM_CATANIA] State 값을 받고 싶어하기 때문에 Enum을 추가해서 처리함.
            Module.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.LOTOP_ABORTED);
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                bool LotEndFlag = true;

                foreach (var runJob in Module.RunList)
                {
                    runJob.Execute();

                    if (runJob.ModuleState.GetState() != ModuleStateEnum.IDLE)
                    {
                        if (runJob.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                        {
                            runJob.Resume();
                        }
                        else if (runJob.ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                        {
                            runJob.End();
                        }
                        else if (runJob.ModuleState.GetState() == ModuleStateEnum.ERROR)
                        {
                            RetVal = Module.InnerStateTransition(new GP_LotOPPausingState(Module));
                        }

                        LotEndFlag = false;
                    }
                }

                if (Module.LoaderController().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {
                    LotEndFlag = false;
                }

                if (LotEndFlag && this.Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[GP_LotOPAborted] LOTEND");
                    LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.DONE,
                                   $"Lot ID: {Module.LotOPModule().LotInfo.LotName.Value}, Device:{Module.FileManager().GetDeviceName()}," +
                                   $"Card ID:{Module.CardChangeModule().GetProbeCardID()}"
                                   , this.Module.LoaderController().GetChuckIndex());


                    var pivinfo = new PIVInfo() { FoupNumber = Module.GEMModule().GetPIVContainer().FoupNumber.Value };
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    Module.EventManager().RaisingEvent(typeof(LotEndEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    Module.DeviceModule().RemoveSpecificReservationRecipe();
                    Module.LotOPModule().LotInfo.ClearLotInfos();

                    RetVal = Module.InnerStateTransition(new GP_LotOPIdleState(Module));

                    RetVal = EventCodeEnum.NONE;
                }


                Func<bool> conditionPauseFunc = () =>
                {
                    Module.Pause();
                    while (true)
                    {
                        bool canPause = Module.RunList.Count(item =>
                          item.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                          item.ModuleState.GetState() == ModuleStateEnum.PENDING) == 0;

                        if (canPause)
                        {
                            break;
                        }
                        else
                        {
                            foreach (IStateModule module in Module.RunList)
                            {
                                module.Execute();
                            }
                        }

                        Thread.Sleep(1);
                    };

                    return true;
                };

                Action doPauseAction = () =>
                {
                    foreach (IStateModule module in Module.RunList)
                    {
                        module.Pause();
                    }
                };
                Action abortPauseAction = () => Module.MetroDialogManager().ShowMessageDialog("[LOTPAUSE]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);

                var consumedPuase = Module.CommandManager().ProcessIfRequested<ILotOpPause>(
                       Module,
                       conditionPauseFunc,
                       doPauseAction,
                       abortPauseAction);
                Thread.Sleep(1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.ABORTED;
        }

        public override EventCodeEnum Pause()
        {
            Module.InnerStateTransition(new GP_LotOPPausingState(Module));

            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand =
                token is ILotOpEnd || token is ILotOpStart || token is ILotOpPause;
            return isValidCommand;
        }
    }
    public class GP_LotOPDone : GP_LotOPStateBase
    {
        public GP_LotOPDone(LotOPModule module) : base(module)
        {
            try
            {
                this.Module.ModuleStopFlag = false;
                Module.NotifyManager().Notify(EventCodeEnum.LOT_END);
                this.Module.LoaderController().UpdateLogUploadList(EnumUploadLogType.LOT);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }
        public override LotOPStateEnum GetState()
        {
            return LotOPStateEnum.DONE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }
    }
}

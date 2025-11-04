

namespace Soaking
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using LogModule;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Event;
    using ProberInterfaces.Param;
    using ProberInterfaces.State;
    using SoakingParameters;
    using ProberInterfaces.Soaking;
    using MetroDialogInterfaces;
    using LoaderController.GPController;

    public abstract class SoakingSubStateBase : ISoakingSubState
    {
        public abstract SoakingStateEnum GetState();
        public virtual ModuleStateEnum GetModuleState()
        {
            var module = SoakingState.GetModule();
            ModuleStateEnum moduleState = ModuleStateEnum.UNDEFINED;

            if (module != null)
            {
                moduleState = module.ModuleState.State;
            }
            return moduleState;
        }
        public abstract bool CanExecute(IProbeCommandToken token);

        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();

        public virtual EventCodeEnum End()
        {
            throw new NotImplementedException();
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }

        public abstract EventCodeEnum ClearState();
        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }

        private SoakingModule _Module;

        public SoakingModule Module
        {
            get
            {
                if (null == _Module)
                {
                    _Module = SoakingState.GetModule() as SoakingModule;
                }

                return _Module;
            }
            set
            {
                if (value != _Module)
                {
                    _Module = value;
                }
            }
        }

        public ISoakingState SoakingState { get; set; }
        public DateTime TransitionIdleStartTime;

        public void ClearTransitionIdleStartTime()
        {
            LoggerManager.SoakingLog($"ClearTransitionIdleStartTime() TransitionIdleStartTime : {TransitionIdleStartTime}");
            TransitionIdleStartTime = default;
        }
        public SoakingSubStateBase(ISoakingState state)
        {
            SoakingState = state;
        }
        public enum LoadMapCmdForPolishWafer
        {
            enumSetSoakingNothing = 0,
            enumSetSoakingDone,
            enumSetSoakingSuspend,
            LoadMapCmdForPolishWafer_End
        }

        protected EventCodeEnum Raising_StatusPreHeatStartEvent(double timeSec)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IStagePIV stagePIV = Module.GEMModule().GetPIVContainer();
                if (stagePIV != null)
                {
                    if (stagePIV.SoakingTimeSec != null)
                    {
                        stagePIV.SoakingTimeSec.Value = timeSec;
                    }
                    else
                    {
                        LoggerManager.SoakingErrLog($"[{this.GetType().Name}], Raising_StatusPreHeatStartEvent() : stagePIV.SoakingTimeSec is null.");
                    }

                    if (SoakingState.GetState() == SoakingStateEnum.PREPARE)
                    {
                        stagePIV.SetPreHeatState(GEMPreHeatStateEnum.PRE_HEATING);
                        LoggerManager.SoakingLog($"PreHeating Start Event({GEMPreHeatStateEnum.PRE_HEATING.ToString()}), TimeSec:{timeSec}");
                    }
                }
                else
                {
                    LoggerManager.SoakingErrLog($"[{this.GetType().Name}], RaisingPreHeatStartEvent() : stagePIV is null.");
                }

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(PreHeatStartEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        protected EventCodeEnum Raising_StatusPreHeatEndEvent()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IStagePIV stagePIV = Module.GEMModule().GetPIVContainer();

                Module.GEMModule().GetPIVContainer().SoakingTimeSec.Value = 0;

                SemaphoreSlim semaphore = new SemaphoreSlim(0);

                if (stagePIV != null && SoakingState.GetState() == SoakingStateEnum.PREPARE)
                {
                    stagePIV.SetPreHeatState(GEMPreHeatStateEnum.NOT_PRE_HEATING);
                    LoggerManager.SoakingLog($"PreHeating End Event,({GEMPreHeatStateEnum.NOT_PRE_HEATING.ToString()})");
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], RaisingPreHeatEndEvent() : stagePIV is null.");
                }

                Module.EventManager().RaisingEvent(typeof(PreHeatEndEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        protected EventCodeEnum Raising_StatusPreHeatFailEvent()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(PreHeatFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        protected void SoakingAbortLotLog(string AbortReason)
        {
            if (SoakingStateEnum.MAINTAIN != SoakingState.GetState())
            {
                string SoakingType = GetCurSoakingTypeForLotLog();
                LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.ABORT,
                                       $"Type: {SoakingType}({AbortReason}), device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}", this.Module.LoaderController().GetChuckIndex());
            }
        }

        /// <summary>
        /// soaking 정보 갱신 처리(Loader)
        /// </summary>
        /// <param name="CurrentElapseSoakingTime> Soaking이 진행된 시간</param>
        public void StatusSoakingInfoUpdateToLoader(long CurrentElapseSoakingTime, bool no_soak = false, bool force_update = false, bool show_before_remainniingTime = false, bool waitingPWLoading = false, bool Dispaly_NoWafer_SoakingTime = false)
        {
            if (Module.StageSupervisor() != null)
            {
                if (Module.LoaderController() != null)
                {
                    string currentSoakInfoStr = ConvertStringCurSoakSatate(waitingPWLoading);
                    if (string.IsNullOrEmpty(currentSoakInfoStr))
                        no_soak = true;

                    if (no_soak)
                        currentSoakInfoStr = "No Soak";

                    long AssignedSoakingTime = Module.StatusSoakingTempInfo.StatusSoakingTime;

                    //Wafer 없이 recovery soaking으로 인해 chilling time이 감소 시 출력되어야 하는 시간 변수를 셋 해줌
                    if (Dispaly_NoWafer_SoakingTime)
                    {
                        AssignedSoakingTime = Module.StatusSoakingTempInfo.RecoverySoakingTime_WithoutWafer;
                    }

                    long CurRemainingSoakingTimeMil = AssignedSoakingTime - CurrentElapseSoakingTime;
                    if (CurRemainingSoakingTimeMil < 0)
                        CurRemainingSoakingTimeMil = 0;

                    if ((Module.StatusSoakingTempInfo.beforeSendSoakingInfo.RemainTime) != (CurRemainingSoakingTimeMil) / 1000 ||
                        Module.StatusSoakingTempInfo.beforeSendSoakingInfo.ZClearance != Module.StatusSoakingTempInfo.CurrentODValueForSoaking ||
                        Module.StatusSoakingTempInfo.beforeSendSoakingInfo.SoakingType != currentSoakInfoStr ||
                        force_update
                        )
                    {
                        SoakingInfo soakinfo = new SoakingInfo();
                        soakinfo.StopSoakBtnEnable = true;
                        soakinfo.SoakingType = currentSoakInfoStr;
                        SoakingStateEnum SubState = SoakingState.GetState();
                        if (SoakingStateEnum.MAINTAIN == SubState || Module.SoakingCancelFlag)
                            soakinfo.StopSoakBtnEnable = false;

                        if (SoakingState.GetModuleState() == ModuleStateEnum.IDLE || SoakingState.GetModuleState() == ModuleStateEnum.DONE)
                        {
                            soakinfo.RemainTime = 0;
                            soakinfo.ZClearance = 0;
                            soakinfo.StopSoakBtnEnable = false;
                            if (SoakingStateEnum.MAINTAIN == SubState && false == no_soak)
                                soakinfo.ZClearance = Module.StatusSoakingTempInfo.CurrentODValueForSoaking;
                        }
                        else
                        {
                            if (false == show_before_remainniingTime)
                                soakinfo.RemainTime = (int)(CurRemainingSoakingTimeMil / 1000);
                            else
                                soakinfo.RemainTime = Module.StatusSoakingTempInfo.beforeSendSoakingInfo.RemainTime;

                            soakinfo.ZClearance = Module.StatusSoakingTempInfo.CurrentODValueForSoaking;
                        }

                        SoakingStateEnum CurSubStat = GetState();
                        if (SoakingStateEnum.SUSPENDED_FOR_TEMPERATURE == CurSubStat)
                        {
                            soakinfo.RemainTime = 0;
                        }

                        if ("No Soak" == currentSoakInfoStr)
                        {
                            soakinfo.RemainTime = 0;
                            soakinfo.ZClearance = 0;
                            soakinfo.StopSoakBtnEnable = false;
                        }

                        if (SoakingStateEnum.SUSPENDED_FOR_ALIGN == CurSubStat)
                        {
                            soakinfo.RemainTime = 0;
                            soakinfo.ZClearance = 0;
                        }

                        soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                        if (EventCodeEnum.NONE == Module.LoaderController().UpdateSoakingInfo(soakinfo))
                            Module.StatusSoakingTempInfo.beforeSendSoakingInfo = soakinfo;
                    }
                }
            }
        }

        public bool StatusIdleSoakingStart_InfoUpdate(int remainingTiimeForSoakingStart, string force_displayStr = "")
        {
            try
            {
                if (Module.StageSupervisor() != null)
                {
                    if (Module.LoaderController() != null)
                    {
                        SoakingInfo soakinfo = new SoakingInfo();
                        SoakingStateEnum InnerState = SoakingState.GetState();
                        string StatusSoakState = "";
                        if (SoakingStateEnum.PREPARE == InnerState)
                            StatusSoakState = "PRE HEATING";
                        else
                            StatusSoakState = InnerState.ToString();

                        string readyToSoakTitle = $"{StatusSoakState}\r\n(Ready to soak)";
                        if (force_displayStr != "")
                            readyToSoakTitle = force_displayStr;
                        soakinfo.SoakingType = readyToSoakTitle;
                        soakinfo.RemainTime = remainingTiimeForSoakingStart;
                        soakinfo.ZClearance = 0;
                        soakinfo.StopSoakBtnEnable = false;
                        soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                        if (EventCodeEnum.NONE == Module.LoaderController().UpdateSoakingInfo(soakinfo))
                            return true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
            }

            return false;
        }

        /// <summary>
        /// Pause시 Soaking 진행중이었다면 현 호출시점까지의 Soaking을 처리하고 Done처리하여 다음 Tick에서 Soaking을 처리할 수 있도록
        /// </summary>
        /// <param name="SoakingStartTime">해당 State가 호출되어 시작된 시간 </param>
        /// <param name="MovetoZclearnace">zclearance 로 움직일 것인지</param>
        /// <param name="AddSoakingElapsedTime">해당 State에서 경과된 시간을 soaking진행 시간에 포함 시킬것인지</param>
        public void ChillingTimeProcForSoakingPause(DateTime SoakingStartTime, bool MovetoZclearnace = true, bool AddSoakingElapsedTime = false, bool needToDecrease = false)
        {
            try
            {
                if (SoakingState.GetState() != SoakingStateEnum.MAINTAIN) //maintain status에서는 ChillingTime Manager에서 누적된 ChillingTime에 대한 증가 및 감소를 해주기 때문에 여기서 Pause에 대한 처리를 해주면 안됨
                {
                    //해당 state에서 머문시간만큼 chilling time 감소가 필요한 경우
                    if (needToDecrease)
                    {
                        TimeSpan ElapseSoakingTime = DateTime.Now - SoakingStartTime;
                        long CurrentElapseSoakingTime = Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime + (long)ElapseSoakingTime.TotalMilliseconds;
                        if (AddSoakingElapsedTime)
                            Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime += (int)ElapseSoakingTime.TotalMilliseconds;
                        LoggerManager.SoakingLog($"Pause - Current State({SoakingState.GetState().ToString()}), elapsed time({ElapseSoakingTime.TotalSeconds} sec), Chilling time decrease(SoakingTime:{Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime})");
                        //soaking한 시간에 대한 chillingtime을 가져와 감소처리
                        Module.ChillingTimeMngObj.ActualProcessedSoakingTime((int)Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime, 0, false);
                    }
                }

                if (MovetoZclearnace)
                    Module.StageSupervisor().StageModuleState.ZCLEARED(); //hhh_todo:확인 필요, Pause로 인해 정리 후 ZCLEARED를 하는게 맞는지, PW는 하는게 맞을거 같은데
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
            }
        }

        /// <summary>
        /// 현재 soaking status와 sub 동작에 대한 string 반환
        /// </summary>
        /// <returns></returns>
        private string ConvertStringCurSoakSatate(bool waitingPWLoading = false)
        {
            string StatusSoaking_Mode = "";
            string subStateVal = "";
            string retVal = "";

            SoakingStateEnum InnerState = SoakingState.GetState();

            if (SoakingState.GetModuleState() == ModuleStateEnum.IDLE || SoakingState.GetModuleState() == ModuleStateEnum.DONE)
            {
                if (InnerState == SoakingStateEnum.MAINTAIN)
                {
                    retVal = "MAINTAIN";
                    var waferExist = Module.GetParam_Wafer().GetStatus();
                    if (waferExist != EnumSubsStatus.EXIST)
                    {
                        if (waitingPWLoading)
                            retVal += "\r\n(Waiting Wafer)";
                        else
                        {
                            if (false == Module.StatusSoakingTempInfo.use_polishwafer)
                                retVal += "\r\n(No Wafer)";
                            else
                                retVal += "\r\n(Waiting PW)";
                        }
                    }
                }
                else
                    retVal = "No Soak";

                return retVal;
            }

            if (InnerState == SoakingStateEnum.PREPARE)
                StatusSoaking_Mode = "PRE HEATING";
            else if (InnerState == SoakingStateEnum.STATUS_EVENT_SOAK)
            {
                StatusSoaking_Mode = Module.StatusSoakingTempInfo.SoakingEvtType.ToString();
            }
            else
            {
                StatusSoaking_Mode = InnerState.ToString();
            }

            SoakingStateEnum CurSubStat = GetState();
            switch (CurSubStat)
            {
                case SoakingStateEnum.SOAKING_RUNNING:
                    if (Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime_Org != 0)
                        subStateVal = "\r\n(Extra Soaking)"; // 기본 soaking완료 후 soaking중 추가적으로 발생한 soaking을 진행중이라는 표기 구분
                    else
                        subStateVal = "\r\n(Soaking)";
                    break;
                case SoakingStateEnum.SUSPENDED_FOR_ALIGN:
                    subStateVal = "\r\n(Waiting Align)";
                    break;
                case SoakingStateEnum.SUSPENDED_FOR_WAITING_WAFER_OBJ:
                    if (false == Module.StatusSoakingTempInfo.use_polishwafer)
                        subStateVal = "\r\n(No Wafer)";
                    else
                    {
                        if (waitingPWLoading)
                        {
                            subStateVal = "\r\n(Waiting Wafer)";
                        }
                        else
                            subStateVal = "\r\n(Waiting PW)";
                    }

                    break;
                case SoakingStateEnum.SUSPENDED_FOR_TEMPERATURE:
                    subStateVal = "\r\n(Waiting Temp)";
                    break;
                case SoakingStateEnum.SUSPENDED_FOR_CARDDOCKING:
                    subStateVal = "\r\n(No Card)";
                    break;
                default:
                    subStateVal = "";
                    break;
            }

            if (subStateVal == "")
                retVal = "";
            else
                retVal = $"{StatusSoaking_Mode}{subStateVal}";

            return retVal;
        }

        /// <summary>
        /// 외부 조건에 의해 soaking sub transition을 변경해야 하는 부분을 확인하고 transition 한다.
        /// </summary>
        /// <returns></returns>
        protected bool Check_N_DO_ForceTransition()
        {
            bool ret = false;
            try
            {
                if (ForceTransitionEnum.NEED_TO_STATUS_SUBIDLE == Module.StatusSoakingForceTransitionState || ForceTransitionEnum.NEED_TO_STATUS_SUBIDLE_AND_ZCLEARANCE == Module.StatusSoakingForceTransitionState)
                {
                    if (this.GetType().Name != "SoakingSubIdle")
                    {
                        LoggerManager.SoakingLog($"Force change state(SoakingSubIdle)");
                        SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                        ret = true;
                    }
                    else
                    {
                        ClearTransitionIdleStartTime();
                        LoggerManager.SoakingLog($"it doesn't need to change state(already SoakingSubIdle)");
                    }
                }
                else if (ForceTransitionEnum.NEED_TO_STATUS_SUBPAUSE == Module.StatusSoakingForceTransitionState)
                {
                    if (this.GetType().Name != "SoakingSubPause")
                    {
                        LoggerManager.SoakingLog($"Force change state(SoakingSubPause)");
                        SoakingState.SubStateTransition(new SoakingSubPause(SoakingState));
                        ret = true;
                    }
                    else
                        LoggerManager.SoakingLog($"it doesn't need to change state(already SoakingSubPause)");
                }
                else if (ForceTransitionEnum.NEED_TO_STATUS_SUBABORT == Module.StatusSoakingForceTransitionState || ForceTransitionEnum.NEED_TO_STATUS_SUBCARDCHANGEABORT == Module.StatusSoakingForceTransitionState)
                {
                    if (this.GetType().Name != "SoakingSubAbort")
                    {
                        LoggerManager.SoakingLog($"Force change state(SoakingSubAbort)");
                        SoakingState.SubStateTransition(new SoakingSubAbort(SoakingState, DateTime.Now, Module.StatusSoakingForceTransitionState));
                        ret = true;
                    }
                    else
                        LoggerManager.SoakingLog($"it doesn't need to change state(already SoakingSubAbort)");
                }
                else if (ForceTransitionEnum.NEED_TO_STATUS_SUBMAINTAINABORT == Module.StatusSoakingForceTransitionState)
                {
                    if (this.GetType().Name != "SoakingSubMaintainAbort")
                    {
                        LoggerManager.SoakingLog($"Force change state(SoakingSubMaintainAbort)");
                        SoakingState.SubStateTransition(new SoakingSubMaintainAbortState(SoakingState,this.GetState())) ;
                        ret = true;
                    }
                    else
                        LoggerManager.SoakingLog($"it doesn't need to change state(already SoakingSubAbort)");
                }
                else if (ForceTransitionEnum.NEED_TO_STATUS_RUNNING == Module.StatusSoakingForceTransitionState)
                {
                    if (false == (this is SoakingSubRunning) && Module.SequenceEngineManager().GetRunState())
                    {
                        LoggerManager.SoakingLog($"Force change state(SoakingSubRunning)");
                        SoakingState.SubStateTransition(new SoakingSubRunning(SoakingState));
                        ret = true;
                    }
                    else
                        LoggerManager.SoakingLog($"it doesn't need to change state(already SoakingSubPause)");
                }

                if (Module.StatusSoakingForceTransitionState != ForceTransitionEnum.NOT_NECESSARY)
                {
                    Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NOT_NECESSARY;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");

            }

            return ret;
        }

        /// <summary>
        /// 현재 Chuck위에 standard wafer가 있거나 비어 있는 상태에서 Polish wafer soaking이 필요한 경우
        /// </summary>
        /// <returns>EventCodeEnum</returns>
        protected void Check_N_Request_PolishWafer(ref bool requested)
        {
            ///Chuck위에 Standard wafer가 존재하고, Polish wafer를 이용한 soaking인 경우 Polish wafer를 받을 수 있도록 처리
            if (Module.StatusSoakingTempInfo.use_polishwafer)
            {
                if (Module.StatusSoakingTempInfo.request_PolishWafer) //이미 요청한 상태라면 재 요청하지 않도록 한다.
                    return;

                var waferExist = Module.GetParam_Wafer().GetStatus();
                if (waferExist == EnumSubsStatus.EXIST && Module.GetParam_Wafer().GetWaferType() != EnumWaferType.STANDARD)
                {
                    return;
                }

                bool RequestPolishWafer = false;
                if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    RequestPolishWafer = true;
                }
                else if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING) // lot running 중에는 prepare, recovery status에서만 PW Soaking가능
                {
                    if (SoakingState.GetState() == SoakingStateEnum.PREPARE || SoakingState.GetState() == SoakingStateEnum.RECOVERY)
                    {
                        RequestPolishWafer = true;
                    }
                }
                else
                {
                    LoggerManager.SoakingErrLog($"Failed to request cmd - Lot module state({Module.LotOPModule().ModuleState.GetState().ToString()})");
                    return;
                }

                if (RequestPolishWafer)
                {
                    LoadMapCmdForPolishWafer soakingFlagMode = LoadMapCmdForPolishWafer.enumSetSoakingNothing;
                    if (waferExist == EnumSubsStatus.EXIST && Module.GetParam_Wafer().GetWaferType() == EnumWaferType.STANDARD)
                    {
                        soakingFlagMode = LoadMapCmdForPolishWafer.enumSetSoakingSuspend;
                    }

                    SendLoadMapCmdForPolishWafer(soakingFlagMode);
                    requested = true;
                }
            }
        }

        /// <summary>
        /// Chuck에 Polish wafer가 있는 상태에서 Lot start가 되면 Soaking param에서 따라 Polish wafer를 반납할 수 있도록 한다.
        /// LotStartTriggered_ToReturnPolishWafer flag는 정상적으로 cmd를 전송했거나 필요없는 경우 초기화 처리한다.
        /// </summary>
        protected void Check_N_Return_PolishWafer()
        {
            if (Module.LotStartTriggered_ToReturnPolishWafer)
            {
                //현재 status에 설정된 Polish wafer 사용 유무를 가져온다.
                bool UsePolishWafer = false;
                Module.StatusSoakingParamIF.Get_UsePolishWaferFlag(SoakingState.GetState(), ref UsePolishWafer);
                if (SoakingState.GetState() != SoakingStateEnum.PREPARE) //lot start시에는 prepare에서만 polish wafer 사용가능하다.
                    UsePolishWafer = false;

                Module.LotStartTriggered_ToReturnPolishWafer = false;
                // polish wafer 미사용
                if (false == UsePolishWafer)
                {
                    //chuck에 polish wafer가 존재하면 반납 필요
                    var waferType = Module.StageSupervisor().WaferObject.GetWaferType();
                    var waferExist = Module.GetParam_Wafer().GetStatus();
                    if (EnumSubsStatus.EXIST == waferExist && EnumWaferType.POLISH == waferType)
                    {
                        SendLoadMapCmdForPolishWafer(LoadMapCmdForPolishWafer.enumSetSoakingDone);
                    }

                    LoggerManager.SoakingLog("LotStartTriggered_ToReturnPolishWafer is off(Polish wafer soaking does not use)");
                }
            }
        }

        protected void SendLoadMapCmdForPolishWafer(LoadMapCmdForPolishWafer enumSoakingFlag)
        {
            if (LoadMapCmdForPolishWafer.enumSetSoakingDone == enumSoakingFlag)
            {
                LoggerManager.SoakingLog($"Set - SetSoakingDone");
                Module.GetParam_Wafer().SetWaferState(EnumWaferState.SOAKINGDONE);
            }
            else if (LoadMapCmdForPolishWafer.enumSetSoakingSuspend == enumSoakingFlag)
            {
                LoggerManager.SoakingLog($"Set - SetSoakingSuspend");
                Module.GetParam_Wafer().SetWaferState(EnumWaferState.SOAKINGSUSPEND);
            }

            if (Module.CommandManager().SetCommand<ILoaderMapCommand>(Module))
            {
                LoggerManager.SoakingLog($"Set CMD - ILoaderMapCommand");
            }
            else
                LoggerManager.SoakingErrLog($"Failed to set CMD - ILoaderMapCommand");
        }

        /// <summary>
        /// chuck을 soaking postion에 배치하고 대기하고 있는데 어떤이유로 chuck이 good position을 벗어나면 idle로 변경하여 다시 soaking을 진행할 수 있도록 하기 위함
        /// </summary>
        /// <param name="ChuckAwayStartTimeForSoaking">Chuck이 벗어났지는지를 감시하기 위한 기준 시간</param>
        /// <param name="chillingTimeReCalc"> WaitSoakingSubRunning과 같이 실제 soaking 시간 차감이 필요한 경우 soaking을 진행한 chilling time을 재 계산할 수 있도록</param>
        /// <param name="MonitoringTime">해당 시간만큼 연속적으로 chuck이 벗어나 있으면 SoakingSubIdle로 간다.</param>
        /// <returns>true: SoakingSubIdle로 변경됨, false:그렇지 않음</returns>
        protected bool IsChangedSoakingPosition(ref DateTime ChuckAwayStartTimeForSoaking, bool chillingTimeReCalc = false, int MonitoringTime = 3)
        {
            string PositionLog = "";

            //현재 Chuck의 위치가 Not Good Position(Chilling Time증가 위치)에 있는지 체크, 일반적으로는 발생하지 않겠지만 soaking중 chuck의 위치가 벗어나는 case가 발생하면 다시 soaking이 진행되어야 함
            if (Module.ChillingTimeMngObj.IsChuckPositionIncreaseChillingTime(true, ref PositionLog))

            {
                //최소 chuck이 good position을 벗어나는게 3초이상일때 바꿔준다.
                if (default == ChuckAwayStartTimeForSoaking)
                    ChuckAwayStartTimeForSoaking = DateTime.Now;
                else
                {
                    TimeSpan ElapsedTime = DateTime.Now - ChuckAwayStartTimeForSoaking;
                    if (ElapsedTime.TotalSeconds >= MonitoringTime)
                    {
                        LoggerManager.SoakingErrLog($"Soaking position is not good. so it will be 'SoakingSubIdle' for Soaking(ElapsedTime:{ElapsedTime.TotalSeconds.ToString()})");
                        LoggerManager.SoakingErrLog($"Chuck Position -> {PositionLog}");
                        if (chillingTimeReCalc)
                        {
                            ChillingTimeProcForSoakingPause(ChuckAwayStartTimeForSoaking, false);
                        }

                        Raising_StatusPreHeatEndEvent();
                        return true;
                    }
                }
            }
            else
            {
                ChuckAwayStartTimeForSoaking = default;
            }

            return false;
        }

        public string GetCurSoakingTypeForLotLog()
        {
            string CurSoakingType = SoakingState.GetState().ToString();
            if (SoakingState.GetState() == SoakingStateEnum.STATUS_EVENT_SOAK)
            {
                if (Module.StatusSoakingTempInfo.SoakingEvtType == EventSoakType.EveryWaferSoak)
                    CurSoakingType = "EVERYWAFER";
            }

            return CurSoakingType;
        }

        /// <summary>
        /// Soaking이 동작할 수 있는 상태인지 체크한다.
        /// </summary>
        /// <param name="WriteLog"> true:log 기록(false 반환 시 이유를 추적할 수 있는 로그를 남긴다.)</param>        
        /// <returns>true: 동작 가능, false: 불가</returns>
        public bool IsGoodOtherModuleStateToRun(bool WriteLog = false)
        {
            if (Module.MonitoringManager().IsMachineInitDone
                && Module.SysState().GetSysState() == EnumSysState.IDLE
                && GetModuleState() != ModuleStateEnum.RUNNING
                && GetModuleState() != ModuleStateEnum.SUSPENDED
                && Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.Z_UP
                && Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.CARDCHANGE
                && Module.StageSupervisor().GetStageLockMode() == StageLockMode.UNLOCK
                && Module.SequenceEngineManager().GetRunState()
                && Module.LoaderController().ModuleState.GetState() != ModuleStateEnum.SUSPENDED
                && Module.LoaderController().IsCancel == false
                && Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED
                && Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.TESTED
                && Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.SKIPPED
                && Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.PROBING
                && (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                && Module.StageSupervisor().PinAligner().ModuleState.GetState() != ModuleStateEnum.RUNNING
                && Module.StageSupervisor().WaferAligner().ModuleState.GetState() != ModuleStateEnum.RUNNING
                && Module.PolishWaferModule().ModuleState.GetState() == ModuleStateEnum.IDLE //polish wafer 동작중일때 running되지 않도록
                && Module.ProbingModule().ModuleState.GetState() == ModuleStateEnum.IDLE
                && Module.PolishWaferModule().ModuleState.GetState() == ModuleStateEnum.IDLE
                && false == Module.PinAligner().WaferTransferRunning  //wafer가 transfer 중이라면 Soaking이 동작하지 않도록 한다.Wafer를 받고 동작한다.(Wafer 이송중 Soaking Module 또는 Soaking으로 타 모듈이 동작되어 Wafer가 들어오지 못할 수 있음)
                && Module.ProbingModule().ProbingStateEnum != EnumProbingState.ZUP
                && Module.ProbingModule().ProbingStateEnum != EnumProbingState.ZUPDWELL
                && Module.DeviceModule().IsHaveDontCareLotReservationRecipe() == false
                && Module.CardChangeModule().ModuleState.GetState() != ModuleStateEnum.SUSPENDED
                && (Module.LotOPModule().CommandRecvSlot.IsRequested<ILotOpEnd>() || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT) == false //Lot End Command가 들어왔거나 LotOpModuleState가 Abort일 때 Soaking이 동작하지 않도록. <ISSR-3983> Soaking Module 뿐만아니라 모든 모듈에서 처리 되도록 되어야 함.
                )
            {
                return true;
            }
            else if (Module.MonitoringManager().IsMachineInitDone
                && Module.SysState().GetSysState() == EnumSysState.IDLE
                && GetModuleState() != ModuleStateEnum.RUNNING
                && GetModuleState() != ModuleStateEnum.SUSPENDED
                && Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.Z_UP
                && Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.CARDCHANGE
                && Module.StageSupervisor().GetStageLockMode() == StageLockMode.UNLOCK
                && Module.SequenceEngineManager().GetRunState()
                && Module.LoaderController().ModuleState.GetState() != ModuleStateEnum.SUSPENDED
                && Module.LoaderController().IsCancel == false
                && Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED
                && Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.TESTED
                && Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.SKIPPED
                && (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                && Module.PolishWaferModule().ModuleState.GetState() == ModuleStateEnum.IDLE
                && Module.ProbingModule().ModuleState.GetState() == ModuleStateEnum.SUSPENDED
                && Module.ProbingModule().ProbingStateEnum == EnumProbingState.SUSPENDED
                && false == Module.PinAligner().WaferTransferRunning  //wafer가 transfer 중이라면 Soaking이 동작하지 않도록 한다.Wafer를 받고 동작한다.(Wafer 이송중 Soaking Module 또는 Soaking으로 타 모듈이 동작되어 Wafer가 들어오지 못할 수 있음)
                && (Module.LotOPModule().CommandRecvSlot.IsRequested<ILotOpEnd>() || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT) == false //Lot End Command가 들어왔거나 LotOpModuleState가 Abort일 때 Soaking이 동작하지 않도록. <ISSR-3983> Soaking Module 뿐만아니라 모든 모듈에서 처리 되도록 되어야 함.
                )
            {
                return true;
            }
            else
            {
                if (WriteLog)
                {
                    LoggerManager.SoakingErrLog($"Can't start manual soaking(" +
                       $"IsMachineInitDone:{Module.MonitoringManager().IsMachineInitDone.ToString()}," +
                       $"SysState:{Module.SysState().GetSysState().ToString()}," +
                       $"Soaking State:{GetModuleState().ToString()}," +
                       $"StageModuleState GetState:{Module.StageSupervisor().StageModuleState.GetState().ToString()}," +
                       $"GetStageLockMode:{Module.StageSupervisor().GetStageLockMode().ToString()}," +
                       $"GetRunState:{Module.SequenceEngineManager().GetRunState().ToString()}," +
                       $"LoaderController State:{Module.LoaderController().ModuleState.GetState().ToString()}," +
                       $"WaferObject GetState:{Module.StageSupervisor().WaferObject.GetState().ToString()}," +
                       $"LotOPModule State:{Module.LotOPModule().ModuleState.GetState().ToString()}," +
                       $"StageMode:{Module.StageSupervisor().StageMode.ToString()}," +
                       $"ManualSoakingStart flag:{Module.ManualSoakingStart.ToString()}," +
                       $"ProbingModule State:{Module.ProbingModule().ModuleState.GetState().ToString()}," +
                       $"PolishwaferModule State:{Module.PolishWaferModule().ModuleState.GetState().ToString()}");

                }

                return false;
            }
        }
    }

    public class SoakingSubIdle : SoakingSubStateBase
    {
        int remainingTimeForIdleSoaking = 0;
        DateTime LastSendedInfoTime = default;
        bool send_soaking_failinfo = false;
        bool request_polish_wafer_cmd = false;
        bool LotStateRun = false;
            
        public SoakingSubIdle(ISoakingState state) : base(state)
        {
            ClearTransitionIdleStartTime();
            Module.ResetSoakingAbort();
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        /// <summary>
        /// Soaking이 Runngind으로 갈 수 있는 상태인지 체크한다.
        /// </summary>
        private bool CanIRun()
        {            
            if (IsGoodOtherModuleStateToRun())
            {
                //위에 기본 조건을 만족한 상태에서 maintenance mode에서 대한 동작을 체크한다.
                if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && Module.ManualSoakingStart) //maintenace mode에서의 manual soaking이 동작되는 경우
                {
                    return true;
                }
                else if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE) //maintenace mode에서는 soaking동작 안함
                {
                    TransitionIdleStartTime = default; //soaking동작을 위한 elapsed time 초기화
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (Module.SysState().GetSysState() == EnumSysState.SETUP)
                {
                    TransitionIdleStartTime = default; //soaking동작을 위한 elapsed time 초기화
                }
                
                return false;
            }                
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {
                if (Module.StatusSoakingDeviceFileObj != null)
                {
                    if (Module.StatusSoakingDeviceFileObj.Get_ShowStatusSoakingSettingPageToggleValue())
                    {
                        if (Check_N_DO_ForceTransition())
                            return EventCodeEnum.NONE;

                        bool EnableStatusSoaking = false;
                        bool IsGettingOptionSuccessul = Module.StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoaking);
                        if (false == IsGettingOptionSuccessul)
                        {
                            LoggerManager.SoakingErrLog($"Failed to get 'IsEnableStausSoaking'");
                            ret = EventCodeEnum.SOAKING_FAILED_GET_SOAKING_DATA;
                        }
                        else
                        {
                            bool ManualSoakingWorking = false;
                            if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && Module.ManualSoakingStart)
                                ManualSoakingWorking = true;

                            bool SendToLoader_SoakingInfo = false;
                            if (EnableStatusSoaking || ManualSoakingWorking)
                            {
                               
                                if ( CanIRun() )
                                {
                                    if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                    {
                                        LotStateRun = true;
                                    }
                                    else
                                    {
                                        if (LotStateRun)
                                        {
                                            TransitionIdleStartTime = DateTime.Now;
                                            LotStateRun = false;
                                        }
                                    }

                                    // Idle에서 더이상 Soaking 돌지 않는 조건.
                                    if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                                    {
                                        var waferExist = Module.GetParam_Wafer().GetStatus();
                                        var waferType = Module.GetParam_Wafer().GetWaferType();
                                        var waferState = Module.GetParam_Wafer().GetState();
                                        var ccAllocated = ((Module.LoaderController() as GP_LoaderController).GPLoaderService?.IsActiveCCAllocatedState(Module.LoaderController().GetChuckIndex()) ?? false);
                                        var pgvRunning = ((Module.LoaderController() as GP_LoaderController).GPLoaderService?.GetPGVCardChangeState() ?? ModuleStateEnum.UNDEFINED) == ModuleStateEnum.RUNNING;

                                        if (waferExist == EnumSubsStatus.EXIST &&
                                            waferType == EnumWaferType.POLISH && 
                                            waferState != EnumWaferState.SOAKINGDONE &&
                                            ccAllocated &&
                                            pgvRunning)
                                        {
                                            Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NEED_TO_STATUS_SUBCARDCHANGEABORT;
                                            return EventCodeEnum.NONE;
                                        }
                                        else if (Module.Idle_SoakingFailed_PinAlign || Module.Idle_SoakingFailed_WaferAlign)
                                        {
                                            //Lot IDLE 상태에서 pin align or wafer align이 실패하는 경우 soaking을 진행하지 않도록 함(soaking 진행 시 pin align을 하는데 해당 부분이 계속 핑퐁되는 문제 발생)
                                            if (false == send_soaking_failinfo && StatusIdleSoakingStart_InfoUpdate(0, "No Soak"))
                                            {
                                                send_soaking_failinfo = true;
                                            }

                                            return EventCodeEnum.NONE;
                                        }
                                        else if (SoakingStateEnum.STATUS_EVENT_SOAK == SoakingState.GetState())
                                        {
                                            //brett// IDLE 상태에서 event soaking이 도는 경우 abort(clear를 위함)로 transition함 (lot중 에러로 인해 pause후 cell end 없이 machine init(idle로 전환)한 경우 등)
                                            Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NEED_TO_STATUS_SUBABORT;//
                                            return EventCodeEnum.NONE;
                                        }
                                        else
                                        {
                                            send_soaking_failinfo = false;
                                        }
                                    }
                                    else
                                    {
                                        send_soaking_failinfo = false;
                                    }

                                    // Maintenance mode가 아니면서 manual soakingStart가 활성화에 대한 비정상 동작 대비(다른 로직에서 이러한 일이 발생되지 않도로 처리되어 있어야 함)
                                    if (Module.StageSupervisor().StageMode != GPCellModeEnum.MAINTENANCE && Module.ManualSoakingStart)
                                    {
                                        Module.ManualSoakingStart = false;
                                        LoggerManager.SoakingErrLog($"ManualSoaking data is wrong()");
                                        return EventCodeEnum.NONE;
                                    }
                                                                        
                                    ret = Module.CheckCardModuleAndThreeLeg(); // Card pod 또는 Three Leg가 모두 내려가 있는지 체크
                                    if (EventCodeEnum.NONE == ret)
                                    {
                                        ///Soaking Running전 현재 Soaking Status에 따라(Prepare,Recovery) 동작되어야할 Soaking 정보를 set한다.Maintain은 ElapseTime이 있기 때문에 해당 조건이 맞을 때 정보를 set하고 시작
                                        bool DoSetStatusSoakingParam = true;

                                        // manual soaking으로 진행되는 경우
                                        if (SoakingStateEnum.MANUAL == SoakingState.GetState())
                                        {
                                            ret = Set_StatusSoakingDataForRunning();
                                            if (ret != EventCodeEnum.NONE)
                                            {
                                                LoggerManager.SoakingErrLog($"Failed to set soaking data(State:{SoakingState.GetState().ToString()},ret:{ret.ToString()})");
                                                return ret;
                                            }
                                        }
                                        else
                                        {
                                            bool CheckToTransition = true;
                                            //IDLE에서 바로 Running으로 Transition하지 않고 설정된 Elapse Time이 지나면 동작한다.(단 Lot Running에서는 Maintain이외에 바로 진행한다.)                            
                                            if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING && SoakingStateEnum.MAINTAIN != SoakingState.GetState())
                                            {
                                                CheckToTransition = false;
                                            }

                                            int Cur_remainingTimeForIdleSoaking = 0;
                                            if (CheckToTransition)
                                            {
                                                ret = CheckToTransitonOnMaintainStatus(out DoSetStatusSoakingParam, ref TransitionIdleStartTime, ref Cur_remainingTimeForIdleSoaking);
                                            }

                                            if (DoSetStatusSoakingParam)
                                            {
                                                ret = Set_StatusSoakingDataForRunning();
                                                if (ret != EventCodeEnum.NONE)
                                                {
                                                    LoggerManager.SoakingErrLog($"Failed to set soaking data(State:{SoakingState.GetState().ToString()},ret:{ret.ToString()})");
                                                    return ret;
                                                }
                                            }
                                            else
                                            {
                                                //IDLE 이면서 Chuck 위치가 Chilling Time 증가 Position에 배치시에 Loader에 정보 표기 정보 전달
                                                if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE && default != TransitionIdleStartTime)
                                                {
                                                    SendToLoader_SoakingInfo = true;
                                                    if (remainingTimeForIdleSoaking != Cur_remainingTimeForIdleSoaking)
                                                    {
                                                        StatusIdleSoakingStart_InfoUpdate(Cur_remainingTimeForIdleSoaking);
                                                        remainingTimeForIdleSoaking = Cur_remainingTimeForIdleSoaking;
                                                    }
                                                }
                                            }
                                        }

                                        if (DoSetStatusSoakingParam && EventCodeEnum.NONE == ret)
                                        {
                                            if (Module.SoakingCancelFlag)
                                            {
                                                //soaking이 시작하기도 전에 cancel을 이미 눌러놓은 경우를 처리
                                                Module.SoakingCancelFlag = false;
                                            }
                                            LoggerManager.SoakingLog($"******* Status Soaking Start({SoakingState.GetState().ToString()}, EVT:{Module.StatusSoakingTempInfo.SoakingEvtType.ToString()}) *******");
                                            double SoakingTimeSec = Module.StatusSoakingTempInfo.StatusSoakingTime / 1000;
                                            Raising_StatusPreHeatStartEvent(SoakingTimeSec);

                                            //z cleared 후 시작(cell에서 ManualJogView화면에 들어가면 StageManualState로 이동되는데 StageManualState로 나오기 위해 추가. StageManualState에는 overriding되어있지 않은 함수들이 많아 에러 발생                                            
                                            Module.StageSupervisor().StageModuleState.ZCLEARED();
                                            ret = SoakingState.SubStateTransition(new SoakingSubRunning(SoakingState));
                                        }
                                        else
                                        {                                            
                                            //Lot이 Running인데 Polish wafer가 존재하는 경우
                                            if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                            {
                                                var waferExist = Module.GetParam_Wafer().GetStatus();
                                                var waferType = Module.StageSupervisor().WaferObject.GetWaferType();
                                                if(Module.GetParam_Wafer().GetWaferType() == EnumWaferType.POLISH)
                                                {
                                                    if (SoakingStateEnum.MAINTAIN == SoakingState.GetState())
                                                    {
                                                        //Maintain state에서 Lot run 일때 Soaking을 위해 들어온 Polish wafer가 있다면 LotRun을 할 수 없다. 따라서 해당 Polish wafer는 반환처리한다.(이미 전송했다면 재요청하지 않음)
                                                        if (EnumWaferType.POLISH == waferType && 
                                                            Module.PolishWaferModule().ModuleState.GetState() == ModuleStateEnum.IDLE && 
                                                            false == request_polish_wafer_cmd)
                                                        {
                                                            LoggerManager.SoakingLog("Return polish wafer because of Lot is running(polish wafer for soaking)");
                                                            SendLoadMapCmdForPolishWafer(LoadMapCmdForPolishWafer.enumSetSoakingDone);
                                                            request_polish_wafer_cmd = true;
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (Module.ChillingTimeMngObj.IsShowDebugString())
                                            Trace.WriteLine($"[SoakingSubState][Error] CheckCardModuleAndThreeLeg() is not good");
                                    }
                                }
                                else
                                {
                                    ret = EventCodeEnum.NONE;
                                }

                                if (false == SendToLoader_SoakingInfo)
                                {
                                    bool send_info = false;
                                    if (default == LastSendedInfoTime)
                                    {
                                        LastSendedInfoTime = DateTime.Now;
                                        send_info = true;
                                    }
                                    else
                                    {
                                        TimeSpan interval_time = DateTime.Now - LastSendedInfoTime;
                                        if (interval_time.TotalSeconds >= 3)
                                        {
                                            send_info = true;
                                            LastSendedInfoTime = DateTime.Now;
                                        }
                                    }

                                    //특정 시간을 주기로 전달한 이유는 Loader와 connection을 하는 시점에 전달한 데이터가 Loader UI에 반영하지 못하는 case있음
                                    if(send_info)
                                    {
                                        bool No_soak = true;                                        
                                        if (default == TransitionIdleStartTime) //chuck이 good positon에 있는 상태
                                           No_soak = false;
                                        
                                        if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && Module.LoaderController().GetconnectFlag() )                                            
                                        {
                                            No_soak = true;
                                        }

                                        if (Module.ProbingModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                                            No_soak = true;

                                        StatusSoakingInfoUpdateToLoader(0, No_soak, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }

        /// <summary>
        /// 바로 Running state로 동작하지 않고 일정시간 chuck이 Chilling Time 증가 position에 있는 경우 동작된다.
        /// Lot Idle 일때 Chuck away elapsed time을 chek한다. Lot Run중일때는 Maintain에서 Chuck에 Wafer가 없을때만 Elapsed time을 체크하여 running으로 갈 수 있도록 한다.
        /// </summary>
        /// <param name="NeedToTransition_ToRunning"> Running으로 Transition 해도 되는지 여부</param>
        /// <param name="NotGoodPositonStartTm"> Ready to Soak 시간이 시작되는 시간</param>
        /// <param name="remainingTimeForWorking"> soaking 동작이 trigger 되기 위해 남은 시간(IDLE)</param>
        /// <returns></returns>
        private EventCodeEnum CheckToTransitonOnMaintainStatus(out bool NeedToTransition_ToRunning, ref DateTime NotGoodPositonStartTm, ref int remainingTimeForWorking)
        {
            NeedToTransition_ToRunning = false;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {

                string PositionLog = "";
                var CurSoakingStatus = SoakingState.GetState();
                //현재 Chuck의 위치가 Not Good Position(Chilling Time증가 위치)에 있는지 체크, PREPARE나 RECOVERY에서는 good position을 볼 필요없이 soaking이 동작되어야 한다.                  
                if (false == Module.ChillingTimeMngObj.IsChuckPositionIncreaseChillingTime(true, ref PositionLog) &&
                    CurSoakingStatus != SoakingStateEnum.PREPARE &&
                    CurSoakingStatus != SoakingStateEnum.RECOVERY)
                {
                    bool force_ReadyToSoak = false;
                    NeedToTransition_ToRunning = false;
                    
                    if (SoakingStateEnum.MAINTAIN == CurSoakingStatus && Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE) //Maintain에서 LOT Idle이면서 Polish wafer를 사용하는데 PW가 없다면 요청할 수 있도록 해야 한다.(Ready to soak이 구동되도록)
                    {
                        bool UsePolishWafer = false;
                        Module.StatusSoakingParamIF.Get_UsePolishWaferFlag(CurSoakingStatus, ref UsePolishWafer);
                        if (UsePolishWafer)
                        {
                            //chuck위에 polish wafer가 없다면
                            var waferExist = Module.GetParam_Wafer().GetStatus();
                            var waferType = Module.StageSupervisor().WaferObject.GetWaferType();
                            if (waferExist != EnumSubsStatus.EXIST || EnumWaferType.POLISH != waferType)
                            {
                                if(default == NotGoodPositonStartTm)
                                    NotGoodPositonStartTm = DateTime.Now;
                                
                                force_ReadyToSoak = true;
                                //LoggerManager.SoakingLog("Need to Polish wafer. So it will set 'NotGoodPositonStartTm'");
                            }
                        }
                    }

                    if(false == force_ReadyToSoak)
                    {
                        NotGoodPositonStartTm = default; //good postion이면 시간 초기화                                                                         
                        return EventCodeEnum.NONE;
                    }                        
                }

                if (NotGoodPositonStartTm == default)
                {
                    NotGoodPositonStartTm = DateTime.Now;
                    NeedToTransition_ToRunning = false;
                    return EventCodeEnum.NONE;
                }

                IStatusSoakingParam statusSoakingIF = SoakingState.GetModule().StatusSoakingParamIF;
                if (null == statusSoakingIF)
                {
                    LoggerManager.SoakingErrLog($"Can not get 'StatusSokingIF', It is null");
                    return EventCodeEnum.SOAKING_ERROR_NULL_DATA;
                }

                int ChuckAwayElapseTimeToTransition = 0;
                if (false == statusSoakingIF.Get_OperationForElapsedTimeSec(ref ChuckAwayElapseTimeToTransition))
                {
                    LoggerManager.SoakingErrLog($"Can not get 'Get_OperationForElapsedTimeSec' ");
                    return EventCodeEnum.SOAKING_ERROR_NULL_DATA;
                }

                TimeSpan ChuckAwayElapsedTime = DateTime.Now - NotGoodPositonStartTm;
                remainingTimeForWorking = ChuckAwayElapseTimeToTransition - (int)ChuckAwayElapsedTime.TotalSeconds;
                //bret// stage mode change 시점에는 ready to soak을 안하도록(maintenance 변경 시점에 soaking이 동작하는 문제 방지를 위함
                if (Module.StageSupervisor().IsModeChanging == false && ChuckAwayElapsedTime.TotalSeconds >= ChuckAwayElapseTimeToTransition)
                {
                    NeedToTransition_ToRunning = true;
                    SoakingStateEnum state = SoakingState.GetState();
                    if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        var waferExist = Module.GetParam_Wafer().GetStatus();
                        var waferType = Module.StageSupervisor().WaferObject.GetWaferType();
                        if (SoakingStateEnum.MAINTAIN == state && waferExist == EnumSubsStatus.EXIST)
                        {
                            //Lot run 중에 Maintain status면서 wafer가 존재하면 soaking은 하지 않는다.(maintain soaking이 완료된 상태로 probing이 가능한 상태이므로)
                            NeedToTransition_ToRunning = false;
                            
                            //Maintain state에서 Lot run중에 Soaking을 위해 들어온 Polish wafer가 있다면 LotRun을 할 수 없다. 따라서 해당 Polish wafer는 반환처리한다.
                            if( EnumWaferType.POLISH == waferType && Module.PolishWaferModule().ModuleState.GetState() == ModuleStateEnum.IDLE )
                            {
                                LoggerManager.SoakingLog("Return polish wafer because of Lot is running(polish wafer for soaking)");
                                SendLoadMapCmdForPolishWafer(LoadMapCmdForPolishWafer.enumSetSoakingDone);
                            }
                        }
                    }
                    
                    if (NeedToTransition_ToRunning)
                        LoggerManager.SoakingLog($"Soaking Start({state.ToString()} working elapsed time({ChuckAwayElapseTimeToTransition} sec), current elapsed time({ChuckAwayElapsedTime.TotalSeconds}), Lot State:{Module.LotOPModule().ModuleState.GetState().ToString()}");
                }

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }

        /// <summary>
        /// 현재 State에서 Soaking해야할 정보를 셋팅한다.(Soaking 동작을 위한 Soaking 정보 셋팅)
        /// </summary>
        private EventCodeEnum Set_StatusSoakingDataForRunning()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            SoakingStateEnum state = SoakingState.GetState();

            try
            {
                IStatusSoakingParam statusSoakingIF = SoakingState.GetModule().StatusSoakingParamIF;
                if (null == statusSoakingIF)
                {
                    LoggerManager.SoakingErrLog($"Parameter is null.");
                    return EventCodeEnum.SOAKING_ERROR_NULL_DATA;
                }

                StatusSoakingCommonParameterInfo StatusCommonParam = new StatusSoakingCommonParameterInfo();
                bool getParam = statusSoakingIF.Get_StatusCommonOption(state, ref StatusCommonParam);
                if (false == getParam)
                {
                    LoggerManager.SoakingErrLog($"Failed to get 'Get_StatusCommonOption'");
                    return EventCodeEnum.SOAKING_FAILED_GET_SOAKING_DATA;
                }
                
                //Polish wafer는 Lot Idle일때는 Prepare/Recovery/Maintain 사용가능하고 Lot run에서는 Maintain일때 이외에 사용가능
                if(Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    if (SoakingState.GetState() == SoakingStateEnum.MAINTAIN)
                        StatusCommonParam.use_polishwafer = false;
                }

                int SoakingTimeMil = 0;
                long Accumulated_chillingTime = 0;
                //long SoakStartChillingTime = 0;
                if (SoakingStateEnum.PREPARE == state)
                {
                    //prepare 인 경우 Lot Idle soaking을 하다가 Lot Run으로 진행 시 기존 수행하던 soaking data를 그대로 사용도록 한다.
                    if(Module.UsePreviousStatusSoakingDataForRunning)
                    {
                        Module.UsePreviousStatusSoakingDataForRunning = false;
                        if (StatusCommonParam.use_polishwafer)
                        {                            
                            LoggerManager.SoakingLog($"Use previous soaking data for running(Prepare Idle soaking(polish wafer) has run.)");

                            LoggerManager.SoakingLog($"Set Data(previous) - {state.ToString()}, Accumulated_chillingTime:{Module.StatusSoakingTempInfo.RecvAccumlatedChillingTime}, SoakingTm:{Module.StatusSoakingTempInfo.StatusSoakingTime}, enalbePW:{Module.StatusSoakingTempInfo.use_polishwafer}, " +
                                $"enableWatingPinAlign:{Module.StatusSoakingTempInfo.enableWatingPinAlign}, waitingPinAlignPeriodSec:{Module.StatusSoakingTempInfo.waitingPinAlignPeriodSec}, notExistWaferObj_ODVal:{Module.StatusSoakingTempInfo.notExistWaferObj_ODVal}");
                            string Temp_previous_data = "";
                            foreach (var SoakingSteItem in Module.StatusSoakingTempInfo.soakingStepList)
                                Temp_previous_data += $"{SoakingSteItem.stepIndex}(Tm:{SoakingSteItem.SoakingTimeSec}, Od:{SoakingSteItem.OD_Value}), ";

                            LoggerManager.SoakingLog($"Set Data(previous) - Soaking Step : {Temp_previous_data}");
                            return EventCodeEnum.NONE;
                        }                        
                    }

                    int PrepareSoakingTime = 0;
                    if (false == statusSoakingIF.Get_PrepareStatusSoakingTimeSec(ref PrepareSoakingTime, Module.UseTempDiff_PrepareSoakingTime))
                    {
                        LoggerManager.SoakingErrLog($"Failed to get prepare soaking time info");
                        return EventCodeEnum.SOAKING_FAILED_GET_SOAKING_DATA;
                    }

                    SoakingTimeMil = PrepareSoakingTime * 1000;
                }
                else if (SoakingStateEnum.RECOVERY == state)
                {
                    bool InChillingTimeTable = false;
                    ret = Module.ChillingTimeMngObj.GetCurrentChilling_N_TimeToSoaking(ref Accumulated_chillingTime, ref SoakingTimeMil, ref InChillingTimeTable);
                    StatusCommonParam.Recovery_NotExistWafer_Ratio = statusSoakingIF.Get_ChillingTimeRatio(SoakingRatioPolicy_Index.Recovery_NotExistWafer);
                }
                else if (SoakingStateEnum.MANUAL == state)
                {
                    int ManualSoakingTime = 0;
                    if (false == statusSoakingIF.Get_ManualSoakingTime(ref ManualSoakingTime))
                    {
                        LoggerManager.SoakingErrLog($"Failed to get manual soaking time info");
                        return EventCodeEnum.SOAKING_FAILED_GET_SOAKING_DATA;
                    }

                    SoakingTimeMil = ManualSoakingTime * 1000;
                    // SoakStartChillingTime = Module.ChillingTimeMngObj.GetChillingTime(); //soaking 시작 시점에 누적되어 있는 chilling time 보관
                    // Manual로 soaking 진행 시 이전 Status가 Recovery라면 현재 사용자가 입력한 Soaking 시간에 맞는 Chilling Time을 가져온다.(Soaking완료 시 chilling time 차감을 위함)
                    var BeforeState = Module.PreInnerState as ISoakingState;
                    if(null != BeforeState && SoakingStateEnum.PREPARE != BeforeState.GetState())
                    {
                        Module.ChillingTimeMngObj.GetChillingTimeAccordingToSoakingTime(SoakingTimeMil, out Accumulated_chillingTime);
                    }
                }
                else if(SoakingStateEnum.STATUS_EVENT_SOAK == state)
                {
                    if(Module.TriggeredStatusEventSoakList.Count() > 0 )
                    {
                        var HighestPriority = Module.TriggeredStatusEventSoakList.Min(x => x.Value);
                        var TargetEventType = Module.TriggeredStatusEventSoakList.FirstOrDefault(x => x.Value == HighestPriority); //우선 순위 가장 높은 event 가져옴.

                        EventSoakingParameterInfo EvtParam = new EventSoakingParameterInfo();
                        if(EventCodeEnum.NONE != Get_StatusEventSoakingDataForRuning(TargetEventType.Key, ref EvtParam))
                        {
                            LoggerManager.SoakingErrLog($"Failed to get evt soaking info");
                            return EventCodeEnum.SOAKING_FAILED_GET_SOAKING_DATA;
                        }
                        //SoakStartChillingTime = Module.ChillingTimeMngObj.GetChillingTime();
                        SoakingTimeMil = EvtParam.Soaking_TimeSec * 1000;
                        Module.ChillingTimeMngObj.GetChillingTimeAccordingToSoakingTime(SoakingTimeMil, out Accumulated_chillingTime);
                        StatusCommonParam.enableWatingPinAlign = false;
                        StatusCommonParam.notExistWaferObj_ODVal = -2000;
                        StatusCommonParam.use_polishwafer = false;
                        StatusCommonParam.soakingStepList.Clear();
                        StatusCommonParam.soakingStepList.Add(new SoakingStepListInfo(1, EvtParam.Soaking_TimeSec, EvtParam.OD_Value));
                        StatusCommonParam.PinAlignMode_AfterSoaking = EvtParam.Soaking_PinAlignMode;
                        StatusCommonParam.SoakingEvtType = TargetEventType.Key;
                    }
                }
                else
                {
                    StatusCommonParam.PinAlignMode_AfterSoaking = PinAlignType.DoNot_PinAlign;
                }

                Module.StatusSoakingTempInfo.Set_StatusSoakingData(state, Accumulated_chillingTime, SoakingTimeMil, ref StatusCommonParam);                
                LoggerManager.SoakingLog($"Set Data - {state.ToString()}, Accumulated_chillingTime:{Accumulated_chillingTime}, SoakingTm:{SoakingTimeMil}, enalbePW:{StatusCommonParam.use_polishwafer}, enableWatingPinAlign:{StatusCommonParam.enableWatingPinAlign}, waitingPinAlignPeriodSec:{StatusCommonParam.waitingPinAlignPeriodSec}, notExistWaferObj_ODVal:{StatusCommonParam.notExistWaferObj_ODVal}");
                string TempLog = "";
                foreach (var SoakingSteItem in StatusCommonParam.soakingStepList)
                    TempLog += $"{SoakingSteItem.stepIndex}(Tm:{SoakingSteItem.SoakingTimeSec}, Od:{SoakingSteItem.OD_Value}), ";

                LoggerManager.SoakingLog($"Set Data - Soaking Step : {TempLog}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            LoggerManager.SoakingLog($"Soaking data setting is successful.(state:{state.ToString()})");
            return ret;
        }

        /// <summary>
        /// Event soaking이 trigger되어 있는지 확인하고 Soaking하기 위한 정보를 셋팅한다.
        /// </summary>
        /// <param name="StatusEvtType"> 진행해야할 Event soaking type </param>
        /// <param name="evtSoakingParamInfo"> 인자로 들어온 soaking type에 맞는 param정보 반환 </param>
        /// <returns></returns>
        private EventCodeEnum Get_StatusEventSoakingDataForRuning(EventSoakType StatusEvtType, ref EventSoakingParameterInfo evtSoakingParamInfo)
        { 
            //every wafer soaking trigger on

            IStatusSoakingParam statusSoakingIF = SoakingState.GetModule().StatusSoakingParamIF;
            if (null == statusSoakingIF)
            {
                LoggerManager.SoakingErrLog($"Parameter is null.");
                return EventCodeEnum.SOAKING_ERROR_NULL_DATA;
            }
            
            if(false == statusSoakingIF.Get_EventSoakingParam(StatusEvtType, ref evtSoakingParamInfo))                      
            {
                LoggerManager.SoakingErrLog($"Failed to get 'Get_EventSoakingParam'({StatusEvtType.ToString()})");
                return EventCodeEnum.SOAKING_FAILED_GET_SOAKING_DATA;
            }

            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;            
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Soaking Running State Class로 Chuck을 Soaking position으로 위치하거나, Pin / wafer Align 을 요청한다.
    /// </summary>
    public class SoakingSubRunning : SoakingSubStateBase
    {
        private bool ExistWafer = false;
        public SoakingSubRunning(ISoakingState state) : base(state)
        {
                       
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }
        
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    LoggerManager.SoakingLog($"Soaking ForceDone....");
                    Raising_StatusPreHeatStartEvent(0);
                    Raising_StatusPreHeatEndEvent();
                    Module.ChillingTimeMngObj.ChillingTimeInit();
                    ret = SoakingState.SubStateTransition(new SoakingSubDone(SoakingState));
                    return ret;
                }

                if (Check_N_DO_ForceTransition())
                    return EventCodeEnum.NONE;

                if (SoakingState.GetState() == SoakingStateEnum.MAINTAIN && Module.ProbingModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {
                    //probing module idle이 아니라면(동작중) maintain state에서는 running으로 가지 않고 soakingSubIdle로 이동처리한다.(probing중 soaking동작하면 안되)
                    LoggerManager.SoakingLog($"ProbingState is not Idle({Module.ProbingModule().ModuleState.GetState().ToString()}). so soaking state change to SoakingSubIdle(cur:SoakingSubRunning");
                    ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                    return ret;
                }

                // soaking 완료 후 마지막 pin align하는 것이 처리되었다면 soaking done처리
                if (PinAlignForStausSoaking.PINALIGN_DONE == Module.StatusSoakingTempInfo.FinalPinAlignAfterSoaking)
                {
                    LoggerManager.SoakingLog($"Final pin align is Done.");
                    Raising_StatusPreHeatEndEvent();
                    if (SoakingState.GetState() == SoakingStateEnum.STATUS_EVENT_SOAK)
                    {
                        LoggerManager.SoakingLog($"Event soaking clear - {Module.StatusSoakingTempInfo.SoakingEvtType.ToString()}");
                        Module.TriggeredStatusEventSoakList.Remove(Module.StatusSoakingTempInfo.SoakingEvtType); //처리 완료된 event soaking 제거
                    }

                    //hhh_todo: 마지막 full ping 처리 완료 후 wafer align 상태를 확인해서 wafer align까지 처리해줘야 하는지는 추후 검토하여 필요시 작업 하자.
                    ret = SoakingState.SubStateTransition(new SoakingSubDone(SoakingState));
                    return ret;
                }

                if (Module.SoakingCancelFlag)
                {
                    LoggerManager.SoakingLog($"-------------- Soaking Cancel(SoakingSubRunning) --------------");
                    ret = SoakingState.SubStateTransition(new SoakingSubAbort(SoakingState, DateTime.Now));
                    return ret;
                }

                if(Module.PolishWaferModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {
                    LoggerManager.SoakingLog($"Polish wafer module is not Idle({Module.PolishWaferModule().ModuleState.GetState().ToString()}). so it will be 'WaitingRelatedModule' state");
                    ret = SoakingState.SubStateTransition(new WaitingRelatedModule(SoakingState));
                    return ret;
                }

                //Running을 하기 위한 조건 확인하고 Running
                //// chuck의 wafer존재 유무등을 파악하고, pin, wafer align을 먼저 처리한다.
                var waferExist = Module.GetParam_Wafer().GetStatus();
                if (waferExist == EnumSubsStatus.EXIST)
                {
                    ExistWafer = true;
                }

                bool NeedToPinAlign = false;
                bool NeedToWaferAlign = false;
                ret = PreCheck_PinAndWaferAlign(out NeedToPinAlign, out NeedToWaferAlign, ExistWafer);
                if(EventCodeEnum.NONE != ret)
                {
                    LoggerManager.SoakingErrLog($"Faild to pre check(ret:{ret.ToString()})");
                    return ret;
                }
                else
                {
                    bool SendCmdWaferAlign = false;
                    if (NeedToPinAlign)
                    {
                        // full pin align 진행
                        if (false == Module.CommandManager().SetCommand<IDOPINALIGN>(Module))
                        {
                            LoggerManager.SoakingErrLog($"Failed to SetCommand(IDOPINALIGN)");
                            return EventCodeEnum.SOAKING_ERROR_SET_CMD;
                        }
                        else
                            LoggerManager.SoakingLog($"Set CMD - IDOPINALIGN");
                        
                        Module.StatusSoakingTempInfo.Call_PinAlignCMD = true;
                    }
                    else if (NeedToWaferAlign) //Pin align과 wafer align을 동시에 cmd하지 말자. Pin Align과정중에 this.WaferAligner().UpdatePadCen()함수를 호출하는 곳들이 있는데 안에서 IDOWAFERALIGN 날려버림..
                    {
                        if (false == Module.CommandManager().SetCommand<IDOWAFERALIGN>(Module))
                        {
                            LoggerManager.SoakingErrLog($"Failed to SetCommand(IDOWAFERALIGN)");
                            return EventCodeEnum.SOAKING_ERROR_SET_CMD;
                        }
                        else
                        {
                            SendCmdWaferAlign = true;
                            LoggerManager.SoakingLog($"Set CMD - IDOWAFERALIGN");                            
                        }
                    }

                    //pin align 또는 wafer align이 필요한 상태, SetCmd를 요청하였다면 suspend로 state 후 대기
                    if(NeedToPinAlign || NeedToWaferAlign)
                    {
                        ret = SoakingState.SubStateTransition(new SoakingSubSuspendForAlign(SoakingState, SendCmdWaferAlign));
                        return ret;
                    }
                }
                
                if (false == Module.CardChangeModule().IsExistCard()) //card 없으면 
                {
                    LoggerManager.SoakingLog($"Card is not docking....");
                    ret = StatusSoakingRunningWithoutCard_or_WaferObj();
                    if (ret != EventCodeEnum.NONE)
                        LoggerManager.SoakingErrLog($"Failed to 'StatusSoakingRunningWithoutCard_or_WaferObj(Not exist card)', retVal : {ret}");
                    else
                        ret = SoakingState.SubStateTransition(new WaitingCardDocking(SoakingState));
                    
                    return ret;
                }

                bool NeedtoTargetWaferObject = false;
                var waferType = Module.StageSupervisor().WaferObject.GetWaferType();
                if (false == ExistWafer)
                {
                    NeedtoTargetWaferObject = true;
                }
                else
                {
                    if(SoakingState.GetState() != SoakingStateEnum.MANUAL)
                    {
                        if (Module.StatusSoakingTempInfo.use_polishwafer) //pw 사용 soaking인데 chuck에 PW가 아닐때
                        {
                            if (EnumWaferType.POLISH != waferType)
                                NeedtoTargetWaferObject = true;
                        }
                        else
                        {
                            if (EnumWaferType.STANDARD != waferType)  //polish wafer soaking이 아닌데 chuck에 standard wafer가 아닌 경우
                                NeedtoTargetWaferObject = true;
                        }
                    }                                        
                }

                // Pin, Wafer align이 준비가 된 상태로 Soaking 처리를 진해한다. Wafer가 없거나 PW 사용일 경우
                if (NeedtoTargetWaferObject)
                {
                    if(PinAlignForStausSoaking.NEED_TO_PINALIGN == Module.StatusSoakingTempInfo.NeedToSamplePinAlign_WithoutWaferObj) //wafer 없을때 pin align처리
                    {
                        LoggerManager.SoakingLog($"Not exist wafer - Need to pin align.");
                        Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                        if (Module.CommandManager().SetCommand<IDOSamplePinAlignForSoaking>(SoakingState.GetModule()) == true) //sample pin align처리를 진행한다.
                        {
                            LoggerManager.SoakingLog($"Set CMD - IDOSamplePinAlignForSoaking");
                            Module.StatusSoakingTempInfo.NeedToSamplePinAlign_WithoutWaferObj = PinAlignForStausSoaking.REQUESTED_PINALIGN;
                            LoggerManager.SoakingLog($"Not exist wafer - Request Pin Align(It will be 'SoakingSubSuspendForAlign')");
                            ret = SoakingState.SubStateTransition(new SoakingSubSuspendForAlign(SoakingState, false));
                            return ret;
                        }
                    }

                    ret = StatusSoakingRunningWithoutCard_or_WaferObj();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.SoakingErrLog($"Failed to 'StatusSoakingRunningWithoutCard_or_WaferObj(Not exist wafer)', retVal : {ret}");
                        return ret;
                    }

                    if (Module.IsCurTempWithinSetSoakingTempRange())
                        ret = SoakingState.SubStateTransition(new SoakingSubSuspendForWaferObject(SoakingState));
                    else
                        ret = SoakingState.SubStateTransition(new WaitingSoakingForTemperature(SoakingState));

                    /// polish wafer 사용인 경우 현재 chuck위에 standard wafer의 존재 여부에 따라 CMD 전달 처리
                    if (Module.StatusSoakingTempInfo.use_polishwafer)
                    {
                        bool request_polishwafer = false;
                        Check_N_Request_PolishWafer(ref request_polishwafer);
                    }
                    else  //Polish wafer 미사용인 경우 chuck에 Polish wafer가 있는지 체크하고 반납(Lot Start triggered되었을때만 동작된다.)
                    {
                        Check_N_Return_PolishWafer();
                    }

                    LoggerManager.SoakingLog($"Wait for the wafer object start");
                    return ret;
                }
                else
                {
                    
                    //Running 동작하는곳(chuck의 wafer 상태를 확인하고 사전 체크가 완료된 상태라면 사용자가 설정한 option에 따라 soaking run)
                    bool SoakinDoneFlag = false;
                    bool NeedToSamplePinAlign = false;

                    int NeedToSamplePinAling_StepIdx = 0;
                    ret = StatusSoakingRunning(out SoakinDoneFlag, out NeedToSamplePinAlign, out NeedToSamplePinAling_StepIdx);
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.SoakingErrLog($"Failed to 'StatusSoakingRunning', retVal : {ret}");
                        return ret;
                    }
                    else
                    {
                        //full pin align이 유효성이 확인되어 soaking step이 실제 진행됨 따라서 해당 flag를 설정하여 pin align valid time으로 인한 중복 full pin align 차단
                        Module.StatusSoakingTempInfo.IsCheckedFullPinAlignForSoaking = true;
                    }

                    if (NeedToSamplePinAlign)
                    {
                        Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                        if (Module.CommandManager().SetCommand<IDOSamplePinAlignForSoaking>(SoakingState.GetModule()))
                        {
                            LoggerManager.SoakingLog($"Set CMD - IDOSamplePinAlignForSoaking");
                            if (NeedToSamplePinAling_StepIdx > 0)
                            {
                                var StepInfo = Module.StatusSoakingTempInfo.StatusSoakingStepProcList.FirstOrDefault(x => x.StepIndex == NeedToSamplePinAling_StepIdx);
                                if(null != StepInfo)
                                {
                                    StepInfo.PinAlignForStatusSoakingEnum = PinAlignForStausSoaking.REQUESTED_PINALIGN;
                                }
                            }
                            
                            ret = SoakingState.SubStateTransition(new SoakingSubSuspendForAlign(SoakingState, false));
                            return ret;
                        }
                    }

                    if (SoakinDoneFlag)
                    {
                        if (SoakingState.GetState() != SoakingStateEnum.MAINTAIN)
                        {
                            //soaking time이 0으로 들어온 case에 대한 예외처리
                            if (Module.StatusSoakingTempInfo.StatusSoakingTime <= 0)
                            {
                                LoggerManager.SoakingErrLog($"Soaking Time is 0. so it doesn't need to pin alignment.");
                                ret = SoakingState.SubStateTransition(new SoakingSubDone(SoakingState));
                                return ret;
                            }
                        }
                           
                        //Soaking 진행중 발생한 Chilling Time에 대해 추가적인 Soaking을 진행한다.
                        bool NeedMoreSoaking = false;
                        if(SoakingState.GetState() != SoakingStateEnum.MAINTAIN)
                            ret = Check_NeedMoreSoakingRunning(out NeedMoreSoaking);

                        if(false == NeedMoreSoaking)
                        {
                            if(Module.StatusSoakingTempInfo.PinAlignMode_AfterSoaking == PinAlignType.DoNot_PinAlign)
                            {
                                LoggerManager.SoakingLog($"Soaking Done(last pin align mode:{PinAlignType.DoNot_PinAlign.ToString()})");
                                Raising_StatusPreHeatEndEvent();                                
                                if (SoakingState.GetState() == SoakingStateEnum.STATUS_EVENT_SOAK)
                                {
                                    Module.TriggeredStatusEventSoakList.Remove(Module.StatusSoakingTempInfo.SoakingEvtType); //처리 완료된 event soaking 제거
                                }
                                
                                ret = SoakingState.SubStateTransition(new SoakingSubDone(SoakingState));
                                return ret;
                            }
                            else
                            {
                                //soaking 완료 시 full pin align을 진행한다.
                                if (PinAlignForStausSoaking.NEED_TO_PINALIGN == Module.StatusSoakingTempInfo.FinalPinAlignAfterSoaking)
                                {
                                    bool sendRet = false;
                                    Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                                    if (Module.StatusSoakingTempInfo.PinAlignMode_AfterSoaking == PinAlignType.Full_PinAlign)
                                    {
                                        sendRet = Module.CommandManager().SetCommand<IDOPinAlignAfterSoaking>(SoakingState.GetModule());
                                        if(sendRet)                                        
                                            LoggerManager.SoakingLog($"Set CMD - IDOPinAlignAfterSoaking");
                                                                                    
                                    }
                                    else
                                    {
                                        sendRet = Module.CommandManager().SetCommand<IDOSamplePinAlignForSoaking>(SoakingState.GetModule());
                                        if (sendRet)
                                            LoggerManager.SoakingLog($"Set CMD - IDOSamplePinAlignForSoaking");
                                    }

                                    if(sendRet)
                                    {
                                        Module.StatusSoakingTempInfo.FinalPinAlignAfterSoaking = PinAlignForStausSoaking.REQUESTED_PINALIGN;
                                        LoggerManager.SoakingLog($"Request Pin Align(after soaking end)  - Last pin align mode:{Module.StatusSoakingTempInfo.PinAlignMode_AfterSoaking.ToString()}");
                                        ret = SoakingState.SubStateTransition(new SoakingSubSuspendForAlign(SoakingState, false));
                                        return ret;
                                    }
                                    else
                                    {
                                        LoggerManager.SoakingErrLog($"Failed to request pin Align(after soaking end)  - Last pin align mode:{Module.StatusSoakingTempInfo.PinAlignMode_AfterSoaking.ToString()}");
                                        return EventCodeEnum.SOAKING_ERROR_SET_CMD;
                                    }
                                }
                                else // soaking 완료 후 Final pin align 상태가 'NEED_TO_PINALIGN' 아닌것으로 일반적으로 발생할 수 없는 문제
                                {
                                    LoggerManager.SoakingErrLog($"Fina pin align state:{Module.StatusSoakingTempInfo.FinalPinAlignAfterSoaking.ToString()}, Change:NEED_TO_PINALIGN");
                                    Module.StatusSoakingTempInfo.FinalPinAlignAfterSoaking = PinAlignForStausSoaking.NEED_TO_PINALIGN;
                                    return ret;
                                }
                            }
                        }
                    }

                    if (Module.IsCurTempWithinSetSoakingTempRange())
                    {
                        if (Module.StatusSoakingTempInfo.StatusSoaking_StartTime == default)
                        {
                            Module.StatusSoakingTempInfo.StatusSoaking_StartTime = DateTime.Now;
                                                        
                            //최초 soaking 시작 시 누적된 ChillingTime을 저장(Manual or event soaking완료 후 soaking중 발생한 chillingTime처리를 위해)
                            SoakingStateEnum state = SoakingState.GetState();
                            if (SoakingStateEnum.MANUAL == state || SoakingStateEnum.STATUS_EVENT_SOAK == state)
                            {
                                Module.StatusSoakingTempInfo.SoakStart_AccumlatedChillingTime = Module.ChillingTimeMngObj.GetChillingTime();
                            }
                        }

                        //Chuck이(wafer obj가 있는 상태) 지정된 위치에 배치되었으므로 WaitingSoakingSubRunning State에서 soaking 할 시간만큼 대기(Soaking step의 시간만큼 대기)
                        ret = SoakingState.SubStateTransition(new WaitingSoakingSubRunning(SoakingState));
                    }
                    else
                    {
                        // chuck이 temp deviation 범위에 들어올때 까지 대기.
                        ret = SoakingState.SubStateTransition(new WaitingSoakingForTemperature(SoakingState));
                    }

                    StatusSoakingInfoUpdateToLoader(Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime); //loader쪽 정보 갱신
                    return ret;
                }               
            }
            catch (Exception err)
            {                
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }

        /// <summary>
        /// Pin과 Wafer Align을 해야 하는지 체크한다.
        /// </summary>
        /// <param name="NeedToPinAlign"> Pin Align 해야함</param>
        /// <param name="NeedToWaferAlign"> Wafer Align 해야함</param>
        /// <param name="ExistWafer"> Wafer Align 해야함</param>
        /// <returns> EventCodeEnum </returns>
        private EventCodeEnum PreCheck_PinAndWaferAlign(out bool NeedToPinAlign, out bool NeedToWaferAlign, bool ExistWafer)
        {
            NeedToPinAlign = false;
            NeedToWaferAlign = false;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.CardChangeModule().IsExistCard())
                {
                    if (Module.StageSupervisor().ProbeCardInfo.AlignState.Value != AlignStateEnum.DONE)
                        NeedToPinAlign = true;

                    TimeSpan PinAlignElapseTime = DateTime.Now - Module.StageSupervisor().PinAligner().LastAlignDoneTime;

                    IStatusSoakingParam statusSoakingIF = SoakingState.GetModule().StatusSoakingParamIF;
                    if (null == statusSoakingIF)
                    {
                        LoggerManager.SoakingErrLog($"Can not get 'StatusSokingIF', It is null");
                        return EventCodeEnum.SOAKING_ERROR_NULL_DATA;
                    }

                    int PinAlignValidTimeSec = 0;
                    if (false == statusSoakingIF.Get_PinAlignValidTimeSec(ref PinAlignValidTimeSec))
                    {
                        LoggerManager.SoakingErrLog($"Can not get PinAlignValidTime Info.");
                        return EventCodeEnum.SOAKING_FAILED_GET_SOAKING_DATA;
                    }

                    if (PinAlignElapseTime.TotalSeconds >= PinAlignValidTimeSec && false == Module.StatusSoakingTempInfo.Call_PinAlignCMD)
                    {
                        // IsCheckedFullPinAlignForSoaking: soking이 이미 들어갔다는것은 full pin에 대한 valid time까지 확인하고 시작된것이므로 중복으로 full ping을 하지 않기 위함(중간 full pin 하면 chuck position soaking position 바뀜)
                        if (false == Module.StatusSoakingTempInfo.IsCheckedFullPinAlignForSoaking)
                        {
                            //pin align valid time이 지나면 pin align 을 한다.(이미 해당 soaking에서 호출되었다면 무시)
                            LoggerManager.SoakingLog($"Need to pin align(Valid time is over). PinAlignValidTimeSec:{PinAlignValidTimeSec}, PinAlignElapseTime(Sec):{PinAlignElapseTime.TotalSeconds}");
                            NeedToPinAlign = true;
                        }
                    }
                    else
                    {
                        //soaking시 contact을 통한 soaking 방식이라면 pin valid time과 상관없이 최초 soaking 들어갈 때 full pin aling을 진행한다.
                        if (false == NeedToPinAlign && false == Module.StatusSoakingTempInfo.Call_PinAlignCMD)
                        {
                            int ContactStepCnt = Module.StatusSoakingTempInfo.StatusSoakingStepProcList.Count(x => x.OD_Value >= -5);
                            if (ContactStepCnt > 0 && false == Module.StatusSoakingTempInfo.IsCheckedFullPinAlignForSoaking)
                            {
                                LoggerManager.SoakingLog($"There is an OD value that can be touched. so full pin alignment is performed.");
                                NeedToPinAlign = true;
                            }
                        }
                    }
                }

                if (ExistWafer)
                {
                    //wafer ojbect는 존재
                    var waferType = Module.StageSupervisor().WaferObject.GetWaferType();
                   if (EnumWaferType.STANDARD == waferType)
                   {
                        if (Module.StageSupervisor().WaferObject.AlignState.Value != AlignStateEnum.DONE)
                        {
                            if(false == Module.StatusSoakingTempInfo.use_polishwafer) //polish wafer 사용이라면 wafer align이 필요없다.
                            {
                                NeedToWaferAlign = true;
                                LoggerManager.SoakingLog($"Need to wafer align({waferType.ToString()})");
                            }
                        }
                   }
                   else
                   {
                        LoggerManager.SoakingLog($"it doesn't need to align({waferType.ToString()})");
                   }
                }

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }

        /// <summary>
        /// Chuck을 Soaking하기 위한 위치로 이동처리하는 함수.
        /// PinAlign, Wafer Align이 되어 있는 상태에서 호출하도록 한다.(OD가 0이상으로 contact 시도로 보이면다면 상위 호출단에서 확인등 작업해줄것)
        /// </summary>
        /// <param name="OD_Value"></param>
        /// <returns>EventCodeEnum return</returns>
        private EventCodeEnum MoveToSoakingPosition(double OD_Value)
        {
            IStatusSoakingParam statusSoakingIF = SoakingState.GetModule().StatusSoakingParamIF;
            if (null == statusSoakingIF)
            {
                LoggerManager.SoakingErrLog($"Parameter is null.");
                return EventCodeEnum.SOAKING_ERROR_NULL_DATA;
            }

            if (null == Module)
            {
                LoggerManager.SoakingErrLog($"Failed to get soaking module object");
                return EventCodeEnum.SOAKING_ERROR_NULL_DATA;
            }

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                bool ManualSoakingWorking = false;
                if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && Module.ManualSoakingStart)
                    ManualSoakingWorking = true;

                bool useEnableStatusSoaking = false;
                if (false == statusSoakingIF.IsEnableStausSoaking(ref useEnableStatusSoaking))
                {
                    LoggerManager.SoakingErrLog($"Failed to call 'IsEnableStausSoaking'");
                    return EventCodeEnum.PARAM_ERROR;
                }

                if (false == useEnableStatusSoaking && false == ManualSoakingWorking)
                {
                    LoggerManager.SoakingErrLog($"Status soaking can not use.It needs to set about 'Enable StatusSoaking'");
                    return EventCodeEnum.SOAKING_ERROR;
                }

                LoggerManager.SoakingLog($"MoveToSoakingPosition Start ------ ");

                if (Module.CardChangeModule().IsExistCard() == false &&
                    (Module.SoakingModule().SoakingSysParam_IParam as SoakingSysParameter).SoakWithoutCard.Value == false)
                {
                    ProbeAxisObject zaxis = Module.MotionManager().GetAxis(EnumAxisConstants.Z);

                    var target_x = (Module.SoakingModule().SoakingSysParam_IParam as SoakingSysParameter).WithoutCardAndNotSoaking_ChuckPosX.Value;
                    var target_y = (Module.SoakingModule().SoakingSysParam_IParam as SoakingSysParameter).WithoutCardAndNotSoaking_ChuckPosY.Value;
                    var target_z = zaxis.Param.ClearedPosition.Value;
                    ret = Module.StageSupervisor().StageModuleState.ZCLEARED();
                    if (EventCodeEnum.NONE != ret)
                    {
                        LoggerManager.SoakingErrLog($"Fail ZCLEARED.('GetStatusSoakingPosition', ret: {ret.ToString()})");
                        return EventCodeEnum.SOAKING_ERROR;
                    }
                    
                    ret = Module.MotionManager().StageMove(target_x, target_y, target_z);
                    if (EventCodeEnum.NONE != ret)
                    {
                        LoggerManager.SoakingErrLog($"return value is wrong. can not move to center.('GetStatusSoakingPosition', ret: {ret.ToString()})target_x: {target_x}, target_y: {target_y}, target_z: {target_z}");
                        return EventCodeEnum.SOAKING_ERROR;
                    }
                    Module.StatusSoakingTempInfo.CurrentODValueForSoaking = zaxis.Param.ClearedPosition.Value;

                    double RefCurChuck_posX = 0;
                    double RefCurChuck_posY = 0;
                    double RefCurChuck_posZ = 0;
                    Module.MotionManager().GetRefPos(EnumAxisConstants.X, ref RefCurChuck_posX);
                    Module.MotionManager().GetRefPos(EnumAxisConstants.Y, ref RefCurChuck_posY);
                    Module.MotionManager().GetRefPos(EnumAxisConstants.Z, ref RefCurChuck_posZ);
                    LoggerManager.SoakingLog($"RefPosFunc(x:{RefCurChuck_posX}, y:{RefCurChuck_posY}, z:{RefCurChuck_posZ}");

                    //기준이 되는 좌표를 확인한다.(debug용)
                    double ChuckAwayTolForChilling_X = Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ChuckAwayTolForChillingTime_X.Value;
                    double ChuckAwayTolForChilling_Y = Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ChuckAwayTolForChillingTime_Y.Value;
                    double ChuckAwayTolForChilling_Z = Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ChuckAwayTolForChillingTime_Z.Value;
                    LoggerManager.SoakingLog($"Tolerance value(x:{ChuckAwayTolForChilling_X}, y:{ChuckAwayTolForChilling_Y}, z:{ChuckAwayTolForChilling_Z}");

                    var DiffX = RefCurChuck_posX - target_x;//현재 위치-가려고 했던 위치
                    var DiffY = RefCurChuck_posY - target_y;
                    var DiffZ = RefCurChuck_posZ - target_z;

                    string logforRefPos = $"Compare Ret: CurChuckPos(x:{RefCurChuck_posX}, y:{RefCurChuck_posY}, z:{RefCurChuck_posZ}), TargetPos(x:{target_x}, y:{target_y}, z:{target_z}), diff:(x:{DiffX}, y:{DiffY}, z:{DiffZ})";
                    LoggerManager.SoakingLog(logforRefPos);
                    LoggerManager.SoakingLog($"Soaking Ref position End ");

                }
                else  //get wafer coord, pin coord 
                {
                    WaferCoordinate wafercoord = new WaferCoordinate();
                    PinCoordinate pincoord = new PinCoordinate();

                    var soakingodthd = Math.Abs(Module.SoakingSysParam_Clone.ChuckFocusingClearanceThd.Value) * -1;
                    //get wafer coord, pin coord 
                    if (Module.SoakingSysParam_Clone.ChuckFocusingSkip.Value &&
                        soakingodthd >= OD_Value &&
                        Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.NOT_EXIST)
                    {
                        ret = Module.Get_StatusSoakingPosition(ref wafercoord, ref pincoord, false, true);
                    }
                    else
                    {
                        ret = Module.Get_StatusSoakingPosition(ref wafercoord, ref pincoord);
                    }

                    if (EventCodeEnum.NONE != ret)
                    {
                        LoggerManager.SoakingErrLog($"return value is wrong('GetStatusSoakingPosition', ret: {ret.ToString()})");
                        return ret;
                    }
                    LoggerManager.SoakingLog($"MovetoSoaking - wafercoord(X:{wafercoord.X.Value:0.00}, Y:{wafercoord.Y.Value:0.00}, Z:{wafercoord.Z.Value:0.00}), pincoord(X:{ pincoord.X.Value:0.00}, Y:{pincoord.Y.Value:0.00}, Z:{pincoord.Z.Value:0.00}), OD: {OD_Value}");
                    ret = Module.StageSupervisor().StageModuleState.MoveToSoaking(wafercoord, pincoord, OD_Value);
                    if (EventCodeEnum.NONE != ret)
                    {
                        LoggerManager.SoakingErrLog($"return value is wrong('MoveToSoaking', ret: {ret.ToString()})");
                        return EventCodeEnum.SOAKING_ERROR;
                    }

                    Module.StatusSoakingTempInfo.CurrentODValueForSoaking = OD_Value;

                    double CurChuck_posX = 0;
                    double CurChuck_posY = 0;
                    double CurChuck_posZ = 0;
                    Module.MotionManager().GetActualPos(EnumAxisConstants.X, ref CurChuck_posX);
                    Module.MotionManager().GetActualPos(EnumAxisConstants.Y, ref CurChuck_posY);
                    Module.MotionManager().GetActualPos(EnumAxisConstants.Z, ref CurChuck_posZ);

                    double RefCurChuck_posX = 0;
                    double RefCurChuck_posY = 0;
                    double RefCurChuck_posZ = 0;
                    Module.MotionManager().GetRefPos(EnumAxisConstants.X, ref RefCurChuck_posX);
                    Module.MotionManager().GetRefPos(EnumAxisConstants.Y, ref RefCurChuck_posY);
                    Module.MotionManager().GetRefPos(EnumAxisConstants.Z, ref RefCurChuck_posZ);
                    LoggerManager.SoakingLog($"Current Chuck - ActualPosFunc(x:{CurChuck_posX:0.00}, y:{CurChuck_posY:0.00}, z:{CurChuck_posZ:0.00}, RefPosFunc(x:{RefCurChuck_posX:0.00}, y:{RefCurChuck_posY:0.00}, z:{RefCurChuck_posZ:0.00}");

                    //기준이 되는 좌표를 확인한다.(debug용)
                    LoggerManager.SoakingLog($"Soaking Ref position Start");
                    WaferCoordinate wafercoordForDebug = new WaferCoordinate();
                    PinCoordinate pincoordForDebug = new PinCoordinate();
                    MachineCoordinate targetpos = new MachineCoordinate();

                    Module.Get_StatusSoakingPosition(ref wafercoordForDebug, ref pincoordForDebug, false, true);
                    LoggerManager.SoakingLog($"Get Ref Coord - wafercoordForDebug(X:{wafercoordForDebug.X.Value:0.00}, Y:{wafercoordForDebug.Y.Value:0.00}, Z:{wafercoordForDebug.Z.Value:0.00}), pincoordForDebug(X:{ pincoordForDebug.X.Value:0.00}, Y:{pincoordForDebug.Y.Value:0.00}, Z:{pincoordForDebug.Z.Value:0.00}), OD: {OD_Value}");

                    targetpos = Module.GetTargetMachinePosition(wafercoordForDebug, pincoordForDebug, OD_Value);
                    LoggerManager.SoakingLog($"Ref Machine Pos(x:{targetpos.X.Value:0.00}, y:{targetpos.Y.Value:0.00}, z:{targetpos.Z.Value:0.00}");

                    double ChuckAwayTolForChilling_X = Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ChuckAwayTolForChillingTime_X.Value;
                    double ChuckAwayTolForChilling_Y = Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ChuckAwayTolForChillingTime_Y.Value;
                    double ChuckAwayTolForChilling_Z = Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ChuckAwayTolForChillingTime_Z.Value;
                    LoggerManager.SoakingLog($"Tolerance value(x:{ChuckAwayTolForChilling_X}, y:{ChuckAwayTolForChilling_Y}, z:{ChuckAwayTolForChilling_Z}");
                    var DiffX = RefCurChuck_posX - targetpos.X.Value;//현재 척 위치-가려고 한 위치
                    var DiffY = RefCurChuck_posY - targetpos.Y.Value;
                    var DiffZ = RefCurChuck_posZ - targetpos.Z.Value;
                    string logforRefPos = $"Compare Ret: CurChuckPos(x:{RefCurChuck_posX:0.00}, y:{RefCurChuck_posY:0.00}, z:{RefCurChuck_posZ:0.00}), RefPos(x:{targetpos.X.Value:0.00}, y:{targetpos.Y.Value:0.00}, z:{targetpos.Z.Value:0.00}), od:{OD_Value}, diff:(x:{DiffX:0.00}, y:{DiffY:0.00}, z:{DiffZ:0.00})";
                    LoggerManager.SoakingLog(logforRefPos);
                    LoggerManager.SoakingLog($"Soaking Ref position End ");

                }

                LoggerManager.SoakingLog($"MoveToSoakingPosition End ------ ");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return ret;
        }

        /// <summary>
        /// Set_StatusSoakingDataForRunning에 설정된 Data대로 Soaking을 진행한다.
        /// 해당 함수 호출단에서 Soakging 수행이 가능한지 여부등을 판단후 호출될 수 있도록 하자.(pin / wafer align)
        /// </summary>
        /// <param name="SoakingDone"> Soaking이 완료되어 SoakingDone 처리 되어야 함</param>
        /// <param name="NeedToSamplePinAlign"> Sample pin align을 해야함. sample pin align을 요청하고 suspend로 대기필요</param>        
        /// <returns>EventCodeEnum</returns>
        private EventCodeEnum StatusSoakingRunning(out bool SoakingDone, out bool NeedToSamplePinAlign, out int StepIndex_NeedToSamplePin)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            SoakingDone = false;
            NeedToSamplePinAlign = false;
            StepIndex_NeedToSamplePin = -1;
            try
            {
                if (SoakingState.GetState() != SoakingStateEnum.MAINTAIN) //maintain status는 유지이기 때문에 특별히 Soaking을 진행해야 하는 시간이 정해져 있지 않다.
                {
                    // 1.Soaking이 진행된 시간과 Soaking해야할 시간을 비교하여 Done처리 여부 결정
                    if (Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime >= Module.StatusSoakingTempInfo.StatusSoakingTime)
                    {
                        SoakingDone = true;
                        foreach (var Item in Module.StatusSoakingTempInfo.StatusSoakingStepProcList)
                        {
                            if (Item.StepDone == false)
                                Item.StepDone = true;
                        }

                        //예외처리(Soaking time을 0으로 설정된 경우)
                        if(Module.StatusSoakingTempInfo.StatusSoakingTime <= 0)
                        {
                            var BeforeState = Module.PreInnerState as ISoakingState;
                            if (SoakingState.GetState() == SoakingStateEnum.PREPARE || (SoakingState.GetState() == SoakingStateEnum.MANUAL && SoakingStateEnum.PREPARE == BeforeState?.GetState()))
                            {
                                Module.ChillingTimeMngObj.ChillingTimeInit();
                                LoggerManager.SoakingErrLog($"Soaking Time is 0. so it can not soaking");
                            }
                        }
                        
                        LoggerManager.SoakingLog($"Soaking Done( State:{Module.StatusSoakingTempInfo.StatusSoaking_State.ToString()}, StatusSoaking_ElapasedTime:{Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime}, StatusSoakingTime:{Module.StatusSoakingTempInfo.StatusSoakingTime}");
                        return EventCodeEnum.NONE;
                    }
                }

                if (Module.StatusSoakingTempInfo.StatusSoakingStepProcList.Count <= 0)
                {
                    LoggerManager.SoakingErrLog($"SoakingStepTable is empty (State:{Module.StatusSoakingTempInfo.StatusSoaking_State.ToString()})");
                    return EventCodeEnum.SOAKING_ERROR_WRONG_DATA;
                }

                // 2. 현재 진행할 SoakingStepTable Index를 확인
                var StepInfo = Module.StatusSoakingTempInfo.StatusSoakingStepProcList.FirstOrDefault(x => x.StepDone == false);
                if (null == StepInfo)
                {
                    //Soaking Step 처리가 모두 완료된 상태로 Pin Align 필요없이 남은 시간은 계속 Soaking하면 되는 상태
                    LoggerManager.SoakingLog($"Soaking Steps have been processed.(State:{Module.StatusSoakingTempInfo.StatusSoaking_State.ToString()})");
                    if (SoakingState.GetState() == SoakingStateEnum.MAINTAIN)
                    {
                        //minatain status에서 soaking step이 완료되면 idle로 이동(Maintain soaking 완료시에는 full pin align을 하지 않는다.
                        Module.StatusSoakingTempInfo.PinAlignMode_AfterSoaking = PinAlignType.DoNot_PinAlign;
                        SoakingDone = true;
                    }

                    return EventCodeEnum.NONE;
                }
                else
                {
                    if (StepInfo.FIRST_SOAKING_STEP_IDX == StepInfo.StepIndex) //제일 처음 Step인 경우는 Full pin align을 하고 들어온 상태이므로 pin align없이 지정된 위치로 chuck을 위치 시킨다.
                    {
                        StepInfo.PinAlignForStatusSoakingEnum = PinAlignForStausSoaking.PINALIGN_DONE;
                        LoggerManager.SoakingLog($"Move to soaking positon(Soaking Step Index:{StepInfo.StepIndex}, OD: {StepInfo.OD_Value}, State:{Module.StatusSoakingTempInfo.StatusSoaking_State.ToString()})");
                        ret = MoveToSoakingPosition(StepInfo.OD_Value);
                        if (EventCodeEnum.NONE == ret)
                        {
                            //prepare 또는 Manual(Prepare로 manual로) 시작 시 현재까지 누적된 ChillingTime은 모두 Reset하고 진행한다.
                            var BeforeState = Module.PreInnerState as ISoakingState;
                            if (SoakingState.GetState() == SoakingStateEnum.PREPARE || (SoakingState.GetState() == SoakingStateEnum.MANUAL && SoakingStateEnum.PREPARE == BeforeState?.GetState()))
                            {
                                LoggerManager.SoakingLog($"Accumulated chilling time is reset for 'prepare or manual(before:prepare)'");
                                Module.ChillingTimeMngObj.ChillingTimeInit();
                            }
                        }
                    }
                    else
                    {
                        //마지막 sample pin align 요청이면 남은 soaking 시간이 AllowableTimeForLastPinAlign 보다 작은 경우에는 pin align하지 않는다(soaking마지막에 무조건 full pin align을 진행하기 때문)
                        var LastStepInfo = Module.StatusSoakingTempInfo.StatusSoakingStepProcList.LastOrDefault();
                        if (null != LastStepInfo)
                        {
                            //마지막 step
                            if (StepInfo.StepIndex == LastStepInfo.StepIndex)
                            {
                                if(SoakingState.GetState() != SoakingStateEnum.MAINTAIN) //maintain state에서는 마지막 full ping align을 하지 않기 때문에 sample pin은 진행하도록
                                {
                                    // 남은 soaking 시간
                                    int allowableTimeForLastPin = 0;
                                    long remainSoakingTime = Module.StatusSoakingTempInfo.StatusSoakingTime - Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime;
                                    if (Module.StatusSoakingParamIF.Get_AllowableTimeForLastPinAign(ref allowableTimeForLastPin))
                                    {
                                        allowableTimeForLastPin *= 1000; //sec -> millisecond
                                        if (remainSoakingTime <= allowableTimeForLastPin)
                                        {
                                            LoggerManager.SoakingLog($"Last step pin align Ignore.(allowableTimeForLastPin:{allowableTimeForLastPin}, Remain soakingTime:{remainSoakingTime})");
                                            StepInfo.PinAlignForStatusSoakingEnum = PinAlignForStausSoaking.PINALIGN_DONE;
                                        }
                                    }
                                }                               
                            }
                        }

                        if (PinAlignForStausSoaking.NEED_TO_PINALIGN == StepInfo.PinAlignForStatusSoakingEnum) //sample pin align 처리 필요
                        {
                            LoggerManager.SoakingLog($"Need to sample pin align(State:{Module.StatusSoakingTempInfo.StatusSoaking_State.ToString()}, Soaking Step Index:{StepInfo.StepIndex})");
                            ret = EventCodeEnum.NONE;

                            NeedToSamplePinAlign = true;
                            StepIndex_NeedToSamplePin = StepInfo.StepIndex;
                        }
                        else if (PinAlignForStausSoaking.REQUESTED_PINALIGN == StepInfo.PinAlignForStatusSoakingEnum) //flag가 잘못되어 요청
                        {
                            LoggerManager.SoakingErrLog($"StepIndex({StepInfo.StepIndex}) already has been requested.)");
                            ret = EventCodeEnum.NONE;
                            NeedToSamplePinAlign = true; //다시 요청 처리해 본다.
                            StepIndex_NeedToSamplePin = StepInfo.StepIndex;
                        }
                        else if (PinAlignForStausSoaking.PINALIGN_DONE == StepInfo.PinAlignForStatusSoakingEnum)
                        {
                            LoggerManager.SoakingLog($"Move to soaking positon(Soaking Step Index:{StepInfo.StepIndex}, OD: {StepInfo.OD_Value}, State:{Module.StatusSoakingTempInfo.StatusSoaking_State.ToString()})");
                            ret = MoveToSoakingPosition(StepInfo.OD_Value);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }

        /// <summary>
        /// card 또는 wafer object가 없는 경우 soaking 처리(사용자가 지정한 OD 값으로 처리)
        /// card나 wafer가 없을때 대기하는 od값은 wafer가 없을 때의 OD로 처리
        /// </summary>
        /// <returns> EventCodeEnum </returns>
        private EventCodeEnum StatusSoakingRunningWithoutCard_or_WaferObj()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                double NotExistWaferObj_OD = Module.StatusSoakingTempInfo.notExistWaferObj_ODVal;// 웨이어퍼 없을때 사용하는 OID
                LoggerManager.SoakingLog($"Not exist card or wafer - Move to soaking positon: State({SoakingState.GetState().ToString()}), OD({NotExistWaferObj_OD})");
                ret = MoveToSoakingPosition(NotExistWaferObj_OD);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }

        /// <summary>
        /// Soaking완료 후 추가적으로 더 Soaking해야 하는 부분이 있는지 확인하고 있다면 해당 정보를 셋팅하여 다음 tick에 남은 soaking시간 처리
        /// </summary>
        /// <param name="NeedMoreSoaking"></param>
        /// <returns></returns>
        private EventCodeEnum Check_NeedMoreSoakingRunning(out bool NeedMoreSoaking)
        {
            NeedMoreSoaking = false;            
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            long Soak_RemainingChillingTime = 0; //manual 또는 event soaking에서 중에 추가적으로 발생한 Chilling Time(prepare,recover,maintain은 아님)
            var BeforeState = Module.PreInnerState as ISoakingState;
            if ( SoakingStateEnum.MANUAL == SoakingState.GetState() || SoakingStateEnum.STATUS_EVENT_SOAK == SoakingState.GetState())
            {
                Soak_RemainingChillingTime = Module.ChillingTimeMngObj.GetChillingTime() - Module.StatusSoakingTempInfo.SoakStart_AccumlatedChillingTime; //recovery상태에서 manualsoak 또는 event soak 진행 시 발생된 추가 chilling time
            }

            if ((SoakingState.GetState() != SoakingStateEnum.PREPARE) ||
            (SoakingState.GetState() == SoakingStateEnum.PREPARE && Module.StatusSoakingTempInfo.RecvAccumlatedChillingTime != 0 ) || //prepare로 동작하면서 발생한 chillingTime이 있으면(일반 Prepare soaking시 누적된 chillingTime을 Reset하고 시작하기 때문에 ChillingTime이 있으면 감소)
            (SoakingStateEnum.MANUAL == SoakingState.GetState() && BeforeState.GetState() != SoakingStateEnum.PREPARE)                      //Manual Prepare 는 chilling time 감소가 의미가 없음.(Manaual Soaking시간이 Prepare soakingTime보다 큰지만 보면됨)
            )
            {
                //Soaking 진행이 완료되었으므로 누적된 Chilling Time을 감소시킨다.
                if(Module.StatusSoakingTempInfo.RecvAccumlatedChillingTime > 0)
                {
                    ret = Module.ChillingTimeMngObj.ActualProcessedSoakingTime((int)Module.StatusSoakingTempInfo.StatusSoakingTime, Module.StatusSoakingTempInfo.RecvAccumlatedChillingTime, true);
                    if (EventCodeEnum.NONE != ret)
                        return ret;
                }                                    
            }

            //manual soaking중 발생한 chilling time에 대한 처리가 완료된 경우
            if(Module.StatusSoakingTempInfo.LastExtraSoakingTime && SoakingStateEnum.MANUAL == SoakingState.GetState())
            {
                LoggerManager.SoakingLog($"Manual soaking done(extra time).");
                return EventCodeEnum.NONE;
            }

            long AccumulatedChillingTime = 0;
            int RemainSoakingTime = 0;
            bool InChillingTimeTable = false;
            ret = Module.ChillingTimeMngObj.GetCurrentChilling_N_TimeToSoaking(ref AccumulatedChillingTime, ref RemainSoakingTime, ref InChillingTimeTable); //Soaking하고 나서 남아 있는 ChillingTime에 대한 Soaknig시간 확인
            if (EventCodeEnum.NONE != ret)
                return ret;
            
            if(AccumulatedChillingTime > 0)
            {
                //RecoveryState 상태에서 Manual soaking을 진행 또는 Event soaking 일때 추가적으로 발생한 Chilling Time에 대해 처리
                if (SoakingStateEnum.MANUAL == SoakingState.GetState() ||
                    SoakingStateEnum.STATUS_EVENT_SOAK == SoakingState.GetState() 
                    )                
                {
                    // soaking하고 감소한 후의 chilling time과 manualsoking중 발생한 시간이 작은 시간에 맞춰 추가 soaking 처리,                
                    long RemainingChillingTimeOnManual = Math.Min(AccumulatedChillingTime, Soak_RemainingChillingTime); //Soak_RemainingChillingTime 값이 0이면 soaking중 발생한 

                    if (RemainingChillingTimeOnManual > 0) //manual(or Status event) soaking을 진행하면서 발생한 추가적인 ChillingTime이 있다면 해당 ChillingTime에 맞는 SoakingTime에 대해 Soaking을 진행
                    {                        
                        ret = Module.ChillingTimeMngObj.GetCurrentChilling_N_TimeToSoaking(ref RemainingChillingTimeOnManual, ref RemainSoakingTime, ref InChillingTimeTable, false); //Soaking하고 나서 남아 있는 ChillingTime에 대한 Soaking시간 확인
                        if (EventCodeEnum.NONE != ret)
                            return ret;
                        else
                        {
                            LoggerManager.SoakingLog($"{SoakingState.GetState().ToString()} Staus soaking(before state:{BeforeState.GetState().ToString()}) - remaining soakingTime after soaking(chilling:{RemainingChillingTimeOnManual}), RemainingSoakingTime:({RemainSoakingTime})");
                            Module.StatusSoakingTempInfo.LastExtraSoakingTime = true;
                        }
                    }
                    else
                        RemainSoakingTime = 0;
                }
            }
                               
            if (RemainSoakingTime > 0) //추가적으로 Soaking해야 할 시간이 있는 경우
            {
                LoggerManager.SoakingLog($"Check Remain soaking time: State({SoakingState.GetState().ToString()}), Remain soaking time({RemainSoakingTime}), AccumulatedChillingTime({AccumulatedChillingTime})");
                string PositionLog ="";
                if(Module.ChillingTimeMngObj.IsChuckPositionIncreaseChillingTime(false, ref PositionLog))
                {
                    //Chuck position이 Chilling Time이 증가하는 위치에 있는 경우 로그를 남기자.
                    LoggerManager.SoakingLog($"Chuck Position -> {PositionLog}");
                }

                double ZTolerancePos = Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ChuckAwayTolForChillingTime_Z.Value;
                double Ref_Z_Pos = Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.MaintainStateConfig.NotExistWaferObj_OD.Value;
                double Limited_Z_Pos = Ref_Z_Pos - ZTolerancePos;

                //soaking od 값이 허용 z tolerance보다 크다면 chilling time이 계속 증가하게 되므로 error log를 기록한다.
                if (Module.StatusSoakingTempInfo.CurrentODValueForSoaking < Limited_Z_Pos) 
                {                    
                    LoggerManager.SoakingErrLog($"Check Remaining Soaking Time : Soaking OD Value is wrong(CurOD:{Module.StatusSoakingTempInfo.CurrentODValueForSoaking }, Limited_Z:{Limited_Z_Pos})");
                }

                Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime_Org = Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime;
                Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime = 0;
                Module.StatusSoakingTempInfo.StatusSoakingTime = RemainSoakingTime;
                Module.StatusSoakingTempInfo.RecvAccumlatedChillingTime = AccumulatedChillingTime;
                NeedMoreSoaking = true;
            }
            else
            {
                LoggerManager.SoakingLog($"Check Remain soaking time: State({SoakingState.GetState().ToString()}), There is no remain soaking time.");
            }

            return EventCodeEnum.NONE;
        }


        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }
        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.SOAKING_RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //pause가 들어왔을때 현재 진행중이던 soaking을 마무리하고 다음 Resume하면 Idle로 가서 다음 tick에서 처리될 수 있도록 한다.
                LoggerManager.SoakingLog($"Soaking Pause. stop soaking, soaking will run next tick(Cur State:SoakingSubRunning)");
                ChillingTimeProcForSoakingPause(DateTime.Now, false); //SoakingSubRunning State에서는 chuck을 움직이는 행위를 하고 있을 수 있으므로 Z positon 이동하면 안됨
                Raising_StatusPreHeatEndEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retVal;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Chuck을 soaking position에 배치되어 있는 상태에서 대기하는 Class
    /// </summary>
    public class WaitingSoakingSubRunning : SoakingSubStateBase
    {
        public DateTime startTimeInfo { get; set; }
        public DateTime startTime_SendModuleMessage { get; set; } = default;
        public DateTime ChuckAwayTimeForSoaking = default;
        public bool WriteSoakingDoneLog = false;
        bool ExtraSoaking = false;
        int SoakingStepIdx = 1;
        double OD_Value = 0;
        int StepSoakingTime = 0;
        public WaitingSoakingSubRunning(ISoakingState state) : base(state)
        {
            startTimeInfo = DateTime.Now;
            startTime_SendModuleMessage = DateTime.Now;
            SoakingStart_ForLotLog();
            int index = Module.LoaderController().GetChuckIndex();
            Module.LoaderController().SetTitleMessage(index, Module.GetModuleMessage());
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        private void SoakingStart_ForLotLog()
        {
            string CurSoakingType = GetCurSoakingTypeForLotLog();
            var StepInfo = Module.StatusSoakingTempInfo.StatusSoakingStepProcList.FirstOrDefault(x => x.StepDone == false);
            if (null != StepInfo)
            {
                OD_Value = StepInfo.OD_Value;
                SoakingStepIdx = StepInfo.StepIndex;
                StepSoakingTime = StepInfo.SoakingTime;
            }
            else
            {
                ExtraSoaking = true;
                //OD값은 가장 마지막 Step의 od로 진해됨.
                var LastStepInfo = Module.StatusSoakingTempInfo.StatusSoakingStepProcList.LastOrDefault();
                if(null != LastStepInfo)
                {
                    OD_Value = LastStepInfo.OD_Value;
                }
            }

            if(ExtraSoaking)
            {
                LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.START,
                               $"Type: {CurSoakingType}, SoakingStep: ExtraSoaking, Soaking OD: {OD_Value}, SubType: Exist Wafer({Module.StageSupervisor().WaferObject.GetWaferType().ToString()}), SoakingTime(sec):{Module.StatusSoakingTempInfo.StatusSoakingTime / 1000}",
                               this.Module.LoaderController().GetChuckIndex());
            }
            else
            {
                LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.START,
                                 $"Type: {CurSoakingType}, SoakingStep:{SoakingStepIdx}, Soaking OD: {OD_Value}, SubType: Exist Wafer({Module.StageSupervisor().WaferObject.GetWaferType().ToString()}), SoakingTime(sec):{Module.StatusSoakingTempInfo.StatusSoakingTime / 1000}, Cur_SoakingElasedTime(Sec):{Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime / 1000}",
                                 this.Module.LoaderController().GetChuckIndex());
            }
        }

        public void SoakingDone_ForLotLog()
        {
            if (WriteSoakingDoneLog)
                return;

            TimeSpan elapsedTime = DateTime.Now - startTimeInfo;
            string CurSoakingType = GetCurSoakingTypeForLotLog();
            string CurSoakingStepInfo;
            if (ExtraSoaking)
                CurSoakingStepInfo = "ExtraSoaking";
            else
            {
                CurSoakingStepInfo = $"{SoakingStepIdx}";
            }

            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.DONE,
                                   $"Type: {CurSoakingType}, SoakingStep:{CurSoakingStepInfo}, device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()},  Accumulated_ElapsedTime(Sec):{elapsedTime.TotalSeconds}",
                                   this.Module.LoaderController().GetChuckIndex());

            WriteSoakingDoneLog = true;
        }

        /// <summary>
        /// Chuck이 Soaking Position에 배치하고 나서 해당 함수에서 Soaking 동작 시간을 체크한다.
        /// Soaking Step table에 따라 pin align을 해야 하므로 Soaking Step table의 정의된 시간만큼 동작. 단 Step tatble의 동작을 모두 수행하고 Soaking할 시간이 남아 있다면 시간을 측정하여 전체 soaking시간을 확인한다.
        /// </summary>
        /// <param name="startTimeInfo"> Wait를 시작한 시간</param>
        /// <param name="NeedToChangeSubRunningState">Running state로 변경 필요한지 여부</param>
        /// <returns>EventCodeEnum</returns>
       private EventCodeEnum Wait_StatusSoaking(out bool NeedToChangeSubRunningState)
        {
            NeedToChangeSubRunningState = false;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                
                DateTime CurTime = DateTime.Now;
                TimeSpan SoakingElapsedTime = CurTime - startTimeInfo; //soaking 경과 시간                
                long CurrentSoakingElapsedTime = Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime + (int)SoakingElapsedTime.TotalMilliseconds; // 누적된 Soaking 시간과 현재 soaking처리가 되고 있는 시간을 합산
                StatusSoakingInfoUpdateToLoader(CurrentSoakingElapsedTime);
                
                //Module Message 전달(5초마다)
                TimeSpan LastModuleMessageSentTime = CurTime - startTime_SendModuleMessage;
                if(LastModuleMessageSentTime.TotalSeconds > 5)
                {
                    startTime_SendModuleMessage = CurTime;
                    int index = Module.LoaderController().GetChuckIndex();
                    Module.LoaderController().SetTitleMessage(index, Module.GetModuleMessage());
                }

                 //현재 soaking step tatble
                var StepInfo = Module.StatusSoakingTempInfo.StatusSoakingStepProcList.FirstOrDefault(x => x.StepDone == false && PinAlignForStausSoaking.PINALIGN_DONE == x.PinAlignForStatusSoakingEnum);
                if (SoakingStateEnum.MAINTAIN != SoakingState.GetState() && CurrentSoakingElapsedTime >= Module.StatusSoakingTempInfo.StatusSoakingTime) //soaking을 한 전체 경과 시간을 비교하여 Soaking 완료 여부 판단
                {
                    LoggerManager.SoakingLog($"Soaking is complete(current soaking ElapsedTime:{CurrentSoakingElapsedTime}, SettedSoakingTime:{Module.StatusSoakingTempInfo.StatusSoakingTime}");
                    NeedToChangeSubRunningState = true;
                    if (null != StepInfo)
                    {
                        StepInfo.StepDone = true;
                    }
                    return EventCodeEnum.NONE;
                }

                if (null == StepInfo)
                {
                    //Soaking Step 처리가 모두 완료된 상태로 Pin Align 필요없이 남은 시간은 계속 Soaking하면 되는 상태
                    if(SoakingStateEnum.MAINTAIN == SoakingState.GetState())
                        NeedToChangeSubRunningState = true;

                    return EventCodeEnum.NONE;
                }
                else
                {
                    var LastStepInfo = Module.StatusSoakingTempInfo.StatusSoakingStepProcList.LastOrDefault();
                    if (null != LastStepInfo && StepInfo.StepIndex == LastStepInfo.StepIndex) //마지막 step으로 지정된 OD로 남은 시간만큼 soaking
                    {
                        LoggerManager.SoakingLog($"Soaking Last Step(StepIdx:{StepInfo.StepIndex}, RemainingSoakingTime(Millisecond):{Module.StatusSoakingTempInfo.StatusSoakingTime - CurrentSoakingElapsedTime}");
                        StepInfo.StepDone = true;
                        return EventCodeEnum.NONE;
                    }
                    else
                    {
                        int CurStepSoakingTimeMil = StepInfo.SoakingTime * 1000;
                        if ((int)SoakingElapsedTime.TotalMilliseconds >= CurStepSoakingTimeMil) // 현재 soaking step table에 있는 시간만큼 처리 완료, Running으로 State변경(Running에서 pin align할것인지 아님 완료할 것인지 판단함)
                        {
                            LoggerManager.SoakingLog($"Soaking Step Done(StepIdx:{StepInfo.StepIndex}, StepSoakingTime(Millisecond):{CurStepSoakingTimeMil}, CurSoakingElapsedTime(Milliesecond):{SoakingElapsedTime.TotalMilliseconds }");
                            NeedToChangeSubRunningState = true;
                            StepInfo.StepDone = true;
                            return EventCodeEnum.NONE;
                        }
                        else
                            return EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {   
                //soaking cancel에 대한 처리
                if(Module.SoakingCancelFlag )
                {
                    LoggerManager.SoakingLog($"-------------- Soaking Cancel(WaitingSoakingSubRunning) --------------");
                    ret = SoakingState.SubStateTransition(new SoakingSubAbort(SoakingState, startTimeInfo));                    
                    return ret;
                }

                if (Check_N_DO_ForceTransition())
                    return EventCodeEnum.NONE;

                if (SoakingState.GetState() == SoakingStateEnum.MAINTAIN && Module.ProbingModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {

                    //probing module idle이 아니라면(동작중) maintain state에서는 running으로 가지 않고 soakingSubIdle로 이동처리한다.(probing중 soaking동작하면 안되)
                    LoggerManager.SoakingLog($"ProbingState is not Idle({Module.ProbingModule().ModuleState.GetState().ToString()}). so soaking state change to SoakingSubIdle");
                    SoakingDone_ForLotLog();
                    ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                    return ret;
                }

                ///Soaking postion이 변경되었는지 체크하여 변경되었다면 SoakingSubIdle로 보내서 soaking을 다시 진행할 수 있도록 하자.
                if(IsChangedSoakingPosition(ref ChuckAwayTimeForSoaking, true))
                {
                    if (SoakingStateEnum.MAINTAIN != SoakingState.GetState())
                    {                        
                        SoakingAbortLotLog("Chuck position is changed");
                        SoakingDone_ForLotLog();
                    }
                        
                    ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                    return ret;
                }

                //지정된 온도 범위에 있지 않다면 waiting temp state로 이동한다.
                if (false == Module.IsCurTempWithinSetSoakingTempRange())
                {
                    ret = SoakingState.SubStateTransition(new WaitingSoakingForTemperature(SoakingState));
                    SoakingAbortLotLog("Temperature is wrong");
                    SoakingDone_ForLotLog();
                    return ret;
                }

                bool NeedToChangeSubRunning = false;
                ret = Wait_StatusSoaking(out NeedToChangeSubRunning);
                if(NeedToChangeSubRunning)
                {
                    // soaking을 위해 머문 시간을 경과 시간에 추가
                    DateTime CurTime = DateTime.Now;
                    TimeSpan SoakingElapsedTime = CurTime - startTimeInfo; //soaking 경과 시간                    
                    Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime += (int)SoakingElapsedTime.TotalMilliseconds;  //soaking 전체 경과시간 관리
                    SoakingDone_ForLotLog();
                    ret = SoakingState.SubStateTransition(new SoakingSubRunning(SoakingState));                    
                }

                return ret;
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }
        public override ModuleStateEnum GetModuleState()
        {
            if (SoakingState.GetState() == SoakingStateEnum.MAINTAIN) // maintain status에서는 soaking이 완료된 상태이기 때문에 Module state는 IDLE로 처리
                return ModuleStateEnum.IDLE;
            else
                return ModuleStateEnum.RUNNING;
        }
        public override SoakingStateEnum GetState()
        {
            if (SoakingState.GetState() == SoakingStateEnum.MAINTAIN)
                return SoakingStateEnum.DONE;
            else
                return SoakingStateEnum.SOAKING_RUNNING;            
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //pause가 들어왔을때 현재 진행중이던 soaking을 마무리하고 다음 Resume하면 Idle로 가서 다음 tick에서 처리될 수 있도록 한다.
                LoggerManager.SoakingLog($"Soaking Pause. stop soaking, soaking will run next tick(Cur State:WatingSoakingSubRunning)");
                ChillingTimeProcForSoakingPause(startTimeInfo, false, true, true);
                Raising_StatusPreHeatEndEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retVal;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }

    }
    public class SoakingSubDone : SoakingSubStateBase
    {        
        public SoakingSubDone(ISoakingState state) : base(state)
        {
            if (Module.SoakingCancelFlag)
            {
                Module.SoakingCancelFlag = false;
            }
           
            Module.UseTempDiff_PrepareSoakingTime = false;

        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;            
            try
            {
                long accumulated_chillingTime = 0;//임의의 값
                int SoakingTimeMil = 0;//임의의 값
                bool InChillingTimeTable = false;//임의의 값
                string msg_soaking_done = "";
               
                EventCodeEnum chillingTime_result = Module.ChillingTimeMngObj.GetCurrentChilling_N_TimeToSoaking(ref accumulated_chillingTime, ref SoakingTimeMil, ref InChillingTimeTable);
                SoakingStateEnum state = SoakingState.GetState();
                if (chillingTime_result == EventCodeEnum.NONE)
                {

                    //Stop 버튼 눌러서 종료
                    if (Module.GetSoakingAbort()) 
                    {
                        //쌓인 칠링 타임 초기화
                        Module.ChillingTimeMngObj.ChillingTimeInit();
                        msg_soaking_done = "Stop Soak";
                    }
                    //개별 Lot End
                    else if (Module.LoaderController().IsCancel)
                    {
                        //Soaking Done에서 개별 Lot Cancel(이때는 Chilling Time을 유지한다.)
                        msg_soaking_done = "Lot canceled.(each)";
                    }
                    else
                    {
                        if (SoakingStateEnum.STATUS_EVENT_SOAK != state && InChillingTimeTable)
                        {
                            //Soaking Time을 모두 Soaking 후 Align까지 모두 마무리 한 후 해당 State로 온다.
                            //이때 chilling Time을 보고 chilling Time이 Chilling Time table의 첫번째 Low의 ChillingTimeSec보다 많이 쌓였다면
                            //Soaking후 동작한 Align 시간에 의해 Chilling Time이 쌓였다고 간주하고 SoakingModule Error로 내보낸다.
                            ret = EventCodeEnum.SOAKING_ERROR_SUSPEND_OVERTIME;
                            string ret_message = $"{ret}\n" +
                                "\nPlease check the minimum value of ChillingTimeSec and the Align time." +
                                "\nAfter removing the error factor, change to Maintenance mode to clear the error.\n" +
                                "\n<Details>" +
                                "\nAfter heating, the Chuck cools again and can't proceed with probing. " +
                                "\nIt is necessary to identify the factors that have occurred." +
                                "\nOne of the factors is that the corresponding error occurs when the Align time is longer than the minimum value of the ChillingTimeTable.\n";
                            Module.ReasonOfError.AddEventCodeInfo(ret, $"[{ret_message}]", GetType().Name);
                            return ret;
                        }
                        else
                        {
                            //현재 동작하는 Status가 Event Soak이면서 InChillingTimeTable가 False일 때
                            //일반 Soak인데 InChillingTimeTable이 False인 경우 
                            //현재 동작하는 Status가 Event Soak이면서 InChillingTimeTable가 True 때
                        }
                    }

                    if (Module.ForcedDone == EnumModuleForcedState.Normal)
                    {
                        LoggerManager.SoakingLog($"{msg_soaking_done}");
                    }

                    LoggerManager.SoakingLog($"******* Status Soaking Done({SoakingState.GetState().ToString()}) *******");

                    Module.IsSoakingDoneAfterWaferLoad = true;

                    if (SoakingStateEnum.MAINTAIN != state)
                    {
                        if (Module.GetParam_Wafer().GetWaferType() == EnumWaferType.POLISH)
                        {
                            if (Module.StatusSoakingTempInfo.use_polishwafer)
                            {
                                LoggerManager.SoakingLog($"Polish wafer was used. so return to polish wafer(Soaking Done)");
                                SendLoadMapCmdForPolishWafer(LoadMapCmdForPolishWafer.enumSetSoakingDone);
                            }
                        }

                        if (SoakingStateEnum.PREPARE == state && Module.GetSoakingAbort() == false)
                        {
                            LoggerManager.SoakingLog($"Before Trigger Value => TempDiff = {Module.TempDiffTriggered}, ProbeCardChangeTriggered = {Module.ProbeCardChangeTriggered}, DeviceChangeTrigger = {Module.DeviceChangeTriggered}");
                            Module.TempDiffTriggered = false;
                            Module.ProbeCardChangeTriggered = false;
                            Module.DeviceChangeTriggered = false;
                            LoggerManager.SoakingLog($"Prepare Soaking Done.");
                            LoggerManager.SoakingLog($"Current Trigger Value => Prepare Soaking Done. TempDiff = {Module.TempDiffTriggered}, ProbeCardChangeTriggered = {Module.ProbeCardChangeTriggered}, DeviceChangeTrigger = {Module.DeviceChangeTriggered}");
                        }
                    }
                    ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return ret;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }
        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.DONE;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class SoakingSubError : SoakingSubStateBase
    {
        public SoakingSubError(ISoakingState state) : base(state)
        {
            try
            {                          
                StatusIdleSoakingStart_InfoUpdate(0, "No Soak");
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    int index = Module.LoaderController().GetChuckIndex();
                    Module.LoaderController().SetTitleMessage(index, "SOAKING(ERROR)");
                }
                string SoakingType = GetCurSoakingTypeForLotLog();
                LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.ERROR, $"Type: {SoakingType}, device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}", Module.LoaderController().GetChuckIndex());
                LoggerManager.SoakingErrLog($"[{this.GetType().Name}] Current State = {GetModuleState()}, Can not add ReasonOfError.");
                Raising_StatusPreHeatFailEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public SoakingSubError(ISoakingState state, EventCodeInfo eventcode) : base(state)
        {
            try
            {
                if (GetModuleState() == ModuleStateEnum.ERROR)
                {
                    StatusIdleSoakingStart_InfoUpdate(0, "No Soak");
                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        int index = Module.LoaderController().GetChuckIndex();
                        Module.LoaderController().SetTitleMessage(index, "SOAKING(ERROR)");
                    }
                    string SoakingType = GetCurSoakingTypeForLotLog();
                    LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.ERROR, $"Type: {SoakingType}, device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}", Module.LoaderController().GetChuckIndex());
                    LoggerManager.SoakingErrLog($"[{this.GetType().Name}] Current State = {GetModuleState()}, Can not add ReasonOfError.");
                    Raising_StatusPreHeatFailEvent();

                    Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
                    if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        Module.MetroDialogManager().ShowMessageDialog("Error Message",$"{eventcode.Message}", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Current State = {GetModuleState()}, Can not add ReasonOfError.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.SOAKING_ERROR;
            try
            {
                Check_N_DO_ForceTransition();
                if (Module.UsePreviousStatusSoakingDataForRunning)  //error일때는 이전 데이터 사용 flag를 초기화한다.(이전 데이터를 사용하지 않음)
                {
                    Module.UsePreviousStatusSoakingDataForRunning = false;
                    LoggerManager.SoakingLog($"Error - UsePreviousStatusSoakingDataForRunning flag off");
                }
                    
                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Soaking중 Pin/Wafer Align을 Done될 때까지 대기하는 Class
    /// </summary>
    public class SoakingSubSuspendForAlign : SoakingSubStateBase
    {
        private bool WaitWaferAlignFlag = true;
        public SoakingSubSuspendForAlign(ISoakingState state, bool WaitWaferAlign) : base(state)
        {
            WaitWaferAlignFlag = WaitWaferAlign;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }
      
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {
              
                if (Check_N_DO_ForceTransition())
                    return EventCodeEnum.NONE;

                bool WaferAlignOk = false;
                EnumWaferType waferType = EnumWaferType.UNDEFINED;
                var waferExist = Module.GetParam_Wafer().GetStatus();
                if (waferExist == EnumSubsStatus.EXIST)
                {
                    waferType = Module.StageSupervisor().WaferObject.GetWaferType(); //undefined type에 대해서는 ??
                    if (Module.StageSupervisor().WaferObject.AlignState.Value == AlignStateEnum.DONE)
                    {
                        if(EnumWaferType.UNDEFINED == waferType)
                        {
                            LoggerManager.SoakingErrLog($"Undefined wafer type - It can't process(Soaking suspend for align)");
                            return EventCodeEnum.UNDEFINED;
                        }

                        if (Module.StatusSoakingTempInfo.use_polishwafer)
                        {
                            //Polish wafer 사용으로 soaking이라면 wafer align이 필요없다.
                            WaferAlignOk = true;
                        }
                        else
                        {
                            if (EnumWaferType.STANDARD == waferType)
                                WaferAlignOk = true;
                        }
                    }             
                }
                else
                    WaferAlignOk = true;

                if (false == WaitWaferAlignFlag)
                    WaferAlignOk = true;

                bool PinAlignOk = false;
                if (Module.CardChangeModule().IsExistCard())
                {
                    if (Module.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE)
                    {
                        // soaking 완료 후 full pin align을 요청한 상태인경우
                        if (Module.StatusSoakingTempInfo.FinalPinAlignAfterSoaking == PinAlignForStausSoaking.REQUESTED_PINALIGN)
                        {
                            Module.StatusSoakingTempInfo.FinalPinAlignAfterSoaking = PinAlignForStausSoaking.PINALIGN_DONE;
                            LoggerManager.SoakingLog($"The final pin align is done(waiting pin align");
                        }

                        if (waferExist == EnumSubsStatus.EXIST && waferType != EnumWaferType.UNDEFINED)
                        {                                                        
                            var StepInfo = Module.StatusSoakingTempInfo.StatusSoakingStepProcList.FirstOrDefault(x => x.StepDone == false && PinAlignForStausSoaking.REQUESTED_PINALIGN == x.PinAlignForStatusSoakingEnum);
                            if (null != StepInfo)
                            {
                                StepInfo.PinAlignForStatusSoakingEnum = PinAlignForStausSoaking.PINALIGN_DONE;
                            }
                        }
                        else
                        {
                            if(PinAlignForStausSoaking.REQUESTED_PINALIGN == Module.StatusSoakingTempInfo.NeedToSamplePinAlign_WithoutWaferObj)
                                Module.StatusSoakingTempInfo.NeedToSamplePinAlign_WithoutWaferObj = PinAlignForStausSoaking.PINALIGN_DONE;
                        }

                        PinAlignOk = true;
                    }                    
                }
                else
                    PinAlignOk = true;
              
                if(PinAlignOk && WaferAlignOk && Module.SequenceEngineManager().GetRunState())
                {
                    LoggerManager.SoakingLog($"Pin Align / WaferAlign OK.(It will be 'SoakingSubRunning')");
                    ret = SoakingState.SubStateTransition(new SoakingSubRunning(SoakingState));
                    return ret;
                }

                SoakingStateEnum state = SoakingState.GetState();
                if(SoakingStateEnum.MAINTAIN == state)
                    StatusSoakingInfoUpdateToLoader(0); //maintain에서는 soaking 남은 시간이 없음
                else
                    StatusSoakingInfoUpdateToLoader(Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime); //loader쪽 정보 갱신
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }
        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.SUSPENDED_FOR_ALIGN;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //pause가 들어왔을때 현재 진행중이던 soaking을 마무리하고 다음 Resume하면 Idle로 가서 다음 tick에서 처리될 수 있도록 한다.
                LoggerManager.SoakingLog($"Soaking Pause. stop soaking, soaking will run next tick(Cur State:SoakingSubSubspendForAlign)");
                ChillingTimeProcForSoakingPause(DateTime.Now, false); //pin align을 위해 suspend로 대기할때는 move to z clearance를 호출하지 않는다(pin align중에 z clearance가 호출될 수 있으므로, pin align하면 z clearance 처리 후 동작됨)
                Raising_StatusPreHeatEndEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return retVal;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// 해당 Class는 Soaking을 하기 위해 Wafer Object를 기다리는 State class 
    /// </summary>
    public class SoakingSubSuspendForWaferObject : SoakingSubStateBase
    {
        bool ShowWaitingWaferTitle = false;
        bool Log_MaintainSoakingDone = false;
        bool Display_RecoverySoakingInfo_On_NoWafer = false;
        private DateTime StartTime; // 해당 State 시작 시        
        private DateTime Request_or_Return_PoliwWafer_StartTm;
        public DateTime ChuckAwayTimeForSoaking = default; // 해당 State에서 일정시간(defulat :3초)가 지나면 soakingSubIdle로 이동하여 다시 soaking을 진행하여 good postion으로 가기 위함.
        
        private void SoakingStart_ForLotLog()
        {
            string CurSoakingType = GetCurSoakingTypeForLotLog();
            double NotExistWaferOD = Module.StatusSoakingTempInfo.notExistWaferObj_ODVal;
            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.START,
                                   $"Type: {CurSoakingType}, Soaking OD: {NotExistWaferOD}, SubType: Not exist wafer",
                                   this.Module.LoaderController().GetChuckIndex());
        }

        public void SoakingDone_ForLotLog()
        {
            if (Log_MaintainSoakingDone)
                return;

            TimeSpan elapsedTime = DateTime.Now - StartTime;
            string CurSoakingType = GetCurSoakingTypeForLotLog();
            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.DONE,
                                   $"Type: {CurSoakingType}, SubType: Not exist wafer, device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}, ElapsedTime(Sec):{elapsedTime.TotalSeconds}",
                                   this.Module.LoaderController().GetChuckIndex());

            Log_MaintainSoakingDone = true;
        }
       
        /// <summary>
        /// Recovery soaking에서 Wafer 없이 대기 시 Chilling time 정책이 Chilling Time을 감소하는 것으로 설정되어 Wafer를 기다리면서 Soaking 동작중인 모습을 표현해야 하는지 체크
        /// </summary>       
        private void CheckNeedToDisplay_DecreasingSoakingTime()
        {
            if (SoakingState.GetState() == SoakingStateEnum.RECOVERY
                && Module.StatusSoakingTempInfo.RecoveryNotExistWafer_Ratio < 0)
            {
                Display_RecoverySoakingInfo_On_NoWafer = true;
                double temp = (double)Module.ChillingTimeMngObj.GetChillingTime() / Module.StatusSoakingTempInfo.RecoveryNotExistWafer_Ratio;
                Module.StatusSoakingTempInfo.RecoverySoakingTime_WithoutWafer = Math.Abs((long)temp);
                LoggerManager.SoakingLog($"Display recovery soaking(Not Exist Wafer):{Module.StatusSoakingTempInfo.RecoverySoakingTime_WithoutWafer}, chillingTime:{Module.ChillingTimeMngObj.GetChillingTime()}, Ratio:{Module.StatusSoakingTempInfo.RecoveryNotExistWafer_Ratio}");                
            }
            else
                Module.StatusSoakingTempInfo.RecoverySoakingTime_WithoutWafer = 0;   
        }

        public SoakingSubSuspendForWaferObject(ISoakingState state) : base(state)
        {
            StartTime = DateTime.Now;
            Request_or_Return_PoliwWafer_StartTm = DateTime.Now;
            
            CheckNeedToDisplay_DecreasingSoakingTime();           
            SoakingStart_ForLotLog();
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {                
                if (Module.SoakingCancelFlag)
                {                    
                    if (SoakingState.GetState() != SoakingStateEnum.MAINTAIN)
                    {
                        LoggerManager.SoakingLog($"-------------- Soaking Cancel(SoakingSubSuspendForWaferObject) --------------");
                        ret = SoakingState.SubStateTransition(new SoakingSubAbort(SoakingState, StartTime));
                    }
                    else
                    {
                        LoggerManager.SoakingLog($"-------------- Soaking Cancel(SoakingSubSuspendForWaferObject - Maintain soaking. don't any action ) --------------");
                        Module.SoakingCancelFlag = false;
                    }

                    return ret;
                }

                if (Check_N_DO_ForceTransition())
                    return EventCodeEnum.NONE;

                bool ExistTargetWaferObject = false;
                
                //wafer가 chuck에 존재하는지 체크
                var waferExist = Module.GetParam_Wafer().GetStatus();
                EnumWaferType waferType = EnumWaferType.UNDEFINED;
                if (waferExist == EnumSubsStatus.EXIST)
                {
                    if (SoakingState.GetState() == SoakingStateEnum.MANUAL)
                    {
                        ExistTargetWaferObject = true;
                    }
                    else
                    {
                        waferType = Module.StageSupervisor().WaferObject.GetWaferType();
                        if (Module.StatusSoakingTempInfo.use_polishwafer)
                        {
                            if (EnumWaferType.POLISH == waferType)
                                ExistTargetWaferObject = true;
                        }
                        else
                        {
                            if (EnumWaferType.STANDARD == waferType)
                                ExistTargetWaferObject = true;
                        }
                    }                   
                }
                                
                TimeSpan elapsedTime = DateTime.Now - StartTime;                
                if (ExistTargetWaferObject)  //soaking을 위한 target wafer가 chuck에 배치되었다면 running으로 이동하여 wafer가 존재하는 soaking을 할 수 있도록 하자.
                {
                    Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTimeWithoutWaferObj += (long)elapsedTime.TotalMilliseconds;
                                            
                    if ( SoakingState.GetState() == SoakingStateEnum.MAINTAIN && 
                        Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING &&
                        EnumWaferType.STANDARD == waferType) //lot running이고 maintain이면 추가 soaking이 필요없이 probing이 가능하다.(standard wafer) SoakingSubidle로 간다.
                    {
                        LoggerManager.SoakingLog("Soaking is maintain and Lot is running. so soaking state will be 'SoakingSubIdle'(it doesn't need to soaking because maintain state)");
                        ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                    }
                    else
                    {
                        if(Display_RecoverySoakingInfo_On_NoWafer)
                        {
                            //진행된 시간에 맞는 chilling time을 계산하여 누적된 chilling time 감소 및 wafer가 있을때의 soaking할 시간 값을 셋팅해준다                                                    
                            long CurrentChillingTime = Module.ChillingTimeMngObj.GetChillingTime();
                            ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));                            
                            LoggerManager.SoakingLog($"Soaking(Not exist wafer) stop. because wafer is in. soaking will transit to idle(Current Accumlated chilling time(mil):{CurrentChillingTime})");                                                        
                        }
                        else
                        {
                            if (Module.SequenceEngineManager().GetRunState())
                            {
                                LoggerManager.SoakingLog($"------- Soaking Start by using the Wafer obj(Exist wafer). so It will be 'SoakingSubRunning' - total waiting time(mil) :{ Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTimeWithoutWaferObj}");
                                ret = SoakingState.SubStateTransition(new SoakingSubRunning(SoakingState));
                            }
                        }
                    }
                                       
                    Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTimeWithoutWaferObj = 0; //초기화
                    SoakingDone_ForLotLog();
                    return ret;
                }
                
                /// Wafer object가 없어 대기 상태에서  주기적으로 pin align을 사용하는 경우
                if (Module.StatusSoakingTempInfo.enableWatingPinAlign)
                {
                    if((int)elapsedTime.TotalSeconds >= Module.StatusSoakingTempInfo.waitingPinAlignPeriodSec)
                    {
                        if (Module.SequenceEngineManager().GetRunState())
                        {
                            //pin align처리 필요                        
                            LoggerManager.SoakingLog($"need to operate PinAlign(Without wafer object),WatingPinAlignPeroidSec:{Module.StatusSoakingTempInfo.waitingPinAlignPeriodSec}, timeSpenSec:{(int)elapsedTime.TotalSeconds }");
                            Module.StatusSoakingTempInfo.NeedToSamplePinAlign_WithoutWaferObj = PinAlignForStausSoaking.NEED_TO_PINALIGN;

                            ret = SoakingState.SubStateTransition(new SoakingSubRunning(SoakingState));
                            
                            TimeSpan ElapsedTime = DateTime.Now - StartTime;
                            Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTimeWithoutWaferObj += (long)(ElapsedTime.TotalMilliseconds);
                            SoakingDone_ForLotLog();
                        }
                        return ret;
                    }
                }

                //Polish wafer를 사용하는 경우, Waiting wafer State에서 지정된 Elapsed time이 지난 경우 Polish wafer를 요청한다(Loader쪽 요청사항)
                if (Module.StatusSoakingTempInfo.use_polishwafer && 
                    false == Module.StatusSoakingTempInfo.request_PolishWafer)
                {
                    int elapsedTimeForPWLoadingSec = 60;
                    if( false == Module.StatusSoakingParamIF.Get_SoakingElapsedTimeSecForPWLoading(ref elapsedTimeForPWLoadingSec) )
                    {
                        LoggerManager.SoakingLog($"Failed to 'Get_SoakingElapsedTimeSecForPWLoading'. so it will use default time(180sec)");
                    }
                    
                    ///단 wafer를 기다리는 경우에서 지정된 시간이 지났거나 Prepare에서 Lot Run이 시작된 경우라면 기다리지 말고 바로 Polish wafer를 요청하도록 한다.
                    TimeSpan elapsedTimeForChk = DateTime.Now - Request_or_Return_PoliwWafer_StartTm;
                    if ( (elapsedTimeForChk.TotalSeconds > elapsedTimeForPWLoadingSec )||
                         (SoakingState.GetState() == SoakingStateEnum.PREPARE && Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                        )
                    {
                        LoggerManager.SoakingLog($"Request Polish wafer.(elapsed time:{elapsedTimeForChk.TotalSeconds}sec, elapsedTimeForPWLoadingSec:{elapsedTimeForPWLoadingSec}sec), SoakingState:{SoakingState.GetState().ToString()}, Lot State:{Module.LotOPModule().ModuleState.GetState().ToString()}");
                        bool requestPolishWafer = false;
                        Check_N_Request_PolishWafer(ref requestPolishWafer);
                        Request_or_Return_PoliwWafer_StartTm = DateTime.Now;
                        if(requestPolishWafer)
                            Module.StatusSoakingTempInfo.request_PolishWafer = true;

                        ShowWaitingWaferTitle = true;
                    }
                }
                else if( false == Module.StatusSoakingTempInfo.use_polishwafer &&
                         Module.GetParam_Wafer().GetWaferType() == EnumWaferType.POLISH )
                {
                    TimeSpan elapsedTimeForChk = DateTime.Now - Request_or_Return_PoliwWafer_StartTm;
                    if (elapsedTimeForChk.TotalSeconds > 30)
                    {
                        LoggerManager.SoakingLog($"Return Polish wafer.");
                        SendLoadMapCmdForPolishWafer(LoadMapCmdForPolishWafer.enumSetSoakingDone);
                        Request_or_Return_PoliwWafer_StartTm = DateTime.Now;
                    }
                }

                //maintain soaking이라면 chuck이 배치된 후 이므로 바로 maintain soaking done log가 남도록 한다.
                if(false == Log_MaintainSoakingDone && SoakingState.GetState() == SoakingStateEnum.MAINTAIN)
                {
                    SoakingDone_ForLotLog();
                }

                ///Soaking postion이 변경되었는지 체크하여 변경되었다면 SoakingSubIdle로 보내서 soaking을 다시 진행할 수 있도록 하자.
                if (IsChangedSoakingPosition(ref ChuckAwayTimeForSoaking))
                {
                    SoakingAbortLotLog("Chuck position is changed");
                    ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                    return ret;
                }

                long SoakingElapsedTime = Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime;

                //Recovery State에서 Wafer가 없을 때 Chilling Time 이 감소하는 경우
                if (Display_RecoverySoakingInfo_On_NoWafer)
                {
                    TimeSpan NotExistWafer_Soaking_ElapsedTime = DateTime.Now - StartTime;                    
                    SoakingElapsedTime = (long)NotExistWafer_Soaking_ElapsedTime.TotalMilliseconds; //현재 

                    //Wafer없이 진행되는 Soaking이 마무리된 Case(시간이 지정된 시간 이상인 경우로 chilling time 이 거의 0 되는 시점), Chilling Time은 ChillingTimeMng에서 감소시키고 있으므로 별도 시간 처리 필요 없음.
                    if(SoakingElapsedTime >= Module.StatusSoakingTempInfo.RecoverySoakingTime_WithoutWafer)
                    {
                        long CurrentChillingTime = Module.ChillingTimeMngObj.GetChillingTime();
                        Module.StatusSoakingTempInfo.NeedToCheck_SoakingStatus_ByChillingTime = true; //지정된 시간만큼 soaking이 진행되었을 때만 chilling time에 따른 Soaking Status 전환처리가 되도록 한다.

                        //lot start되면 pin align을 진행하기 때문에 Iot idle 경우에는 maintain으로 전환 시 pin align을 하지 않도록 한다.(Running중이라면 pin align 되어야 함)
                        if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                            Module.StatusSoakingTempInfo.NeedToPinAlign_For_WithoutWaferSoaking = false;

                        LoggerManager.SoakingLog($"Soaking(Not exist wafer) done.(soaking time:{Module.StatusSoakingTempInfo.RecoverySoakingTime_WithoutWafer}, Elapsed soakingTime:{SoakingElapsedTime}), accumlated chilling time:{CurrentChillingTime}");
                        ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                        SoakingDone_ForLotLog();
                        return ret;
                    }
                }

                //지정된 온도 범위에 있지 않다면 waiting temp state로 이동한다.
                if (false == Module.IsCurTempWithinSetSoakingTempRange())
                {
                    SoakingAbortLotLog("Temperature is wrong");
                    ret = SoakingState.SubStateTransition(new WaitingSoakingForTemperature(SoakingState));
                    SoakingDone_ForLotLog();
                    return ret;
                }

                StatusSoakingInfoUpdateToLoader(SoakingElapsedTime, false, false, false, ShowWaitingWaferTitle, Display_RecoverySoakingInfo_On_NoWafer); //polish wafer를 기다리는것으로 문구 출력                                                
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }
        public override ModuleStateEnum GetModuleState()
        {
            if(SoakingState.GetState() == SoakingStateEnum.MAINTAIN) // maintain status에서는 soaking이 완료된 상태이기 때문에 Module state는 IDLE로 처리
                return ModuleStateEnum.IDLE;
            else
                return ModuleStateEnum.SUSPENDED;
        }
        public override SoakingStateEnum GetState()
        {
            if (SoakingState.GetState() == SoakingStateEnum.MAINTAIN)
                return SoakingStateEnum.DONE;
            else
                return SoakingStateEnum.SUSPENDED_FOR_WAITING_WAFER_OBJ;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //pause가 들어왔을때 현재 진행중이던 soaking을 마무리하고 다음 Resume하면 Idle로 가서 다음 tick에서 처리될 수 있도록 한다.
                LoggerManager.SoakingLog($"Soaking Pause. stop soaking, soaking will run next tick(Cur State:SoakingSubSuspendForWaferObject)");
                ChillingTimeProcForSoakingPause(DateTime.Now, false);
                Raising_StatusPreHeatEndEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retVal;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// chuck 배치 후 soaking 시간으로 판단해도 되는 온도가 아닌 경우, 적정온도가 될때까지 대기하는 State class
    /// </summary>
    public class WaitingSoakingForTemperature : SoakingSubStateBase
    {
        public DateTime startTime { get; set; }
        public DateTime ChuckAwayTimeForSoaking = default;
        public WaitingSoakingForTemperature(ISoakingState state) : base(state)
        {
            startTime = DateTime.Now;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {
                //soaking cancel에 대한 처리
                if (Module.SoakingCancelFlag)
                {
                    LoggerManager.SoakingLog($"-------------- Soaking Cancel(WaitingSoakingForTemperature) --------------");
                    startTime = DateTime.Now; //WaitingSoakingForTemperature 에서 SoakingCancelProc에 시작 시간을 현재 시간으로 전달하여 WaitingSoakingForTemperature에서 대기한 시간은 Soaking시간으로 간주하지 않기 위해
                    ret = SoakingState.SubStateTransition(new SoakingSubAbort(SoakingState, startTime));
                    return ret;
                }

                if (Check_N_DO_ForceTransition())
                    return EventCodeEnum.NONE;
                
                if (Module.IsCurTempWithinSetSoakingTempRange())
                {
                    TimeSpan ElapsedTimeInfo = DateTime.Now - startTime;
                    LoggerManager.SoakingLog($"End 'WaitingSoakingForTemperature' - elapsed time:{ElapsedTimeInfo.TotalMilliseconds}");

                    //적정 온도 도달 시 IDLE 변경하여 누적된 Chilling Time에 따라 동작될 수 있도록 한다.                    
                    ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                    return ret;
                }

                ///Soaking postion이 변경되었는지 체크하여 변경되었다면 SoakingSubIdle로 보내서 soaking을 다시 진행할 수 있도록 하자.
                if (IsChangedSoakingPosition(ref ChuckAwayTimeForSoaking))
                {
                    LoggerManager.SoakingLog($"Soaking position is wrong. so soaking state will be IDLE.");
                    ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                    return ret;
                }
                   
                StatusSoakingInfoUpdateToLoader(Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime); //loader쪽 정보 갱신
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.SUSPENDED_FOR_TEMPERATURE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //pause가 들어왔을때 현재 진행중이던 soaking을 마무리하고 다음 Resume하면 Idle로 가서 다음 tick에서 처리될 수 있도록 한다.
                LoggerManager.SoakingLog($"Soaking Pause. stop soaking, soaking will run next tick(Cur State:WatingSoakingForTemperature)");
                ChillingTimeProcForSoakingPause(DateTime.Now, false);
                Raising_StatusPreHeatEndEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retVal;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            LoggerManager.SoakingLog($"Soaking Resume. Soaking Status Change to Idle");
            retVal = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
            return retVal;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// card가 장착 될때까지 기다린다.
    /// </summary>
    public class WaitingCardDocking : SoakingSubStateBase
    {
        public DateTime startTime { get; set; }
        public DateTime ChuckAwayTimeForSoaking = default; // 해당 State에서 일정시간(defulat :3초)가 지나면 soakingSubIdle로 이동하여 다시 soaking을 진행하여 good postion으로 가기 위함.
        public WaitingCardDocking(ISoakingState state) : base(state)
        {
            startTime = DateTime.Now;
            string CurSoakingType = GetCurSoakingTypeForLotLog();
            double NotExistWaferOD = Module.StatusSoakingTempInfo.notExistWaferObj_ODVal;
            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.START,
                                   $"Type:{CurSoakingType}, Soaking OD: {NotExistWaferOD}, Waiting Card docking - pin height: PinMinRegRange({Module.StageSupervisor().PinMinRegRange}, Wafer Thickness:WaferMaxThickness({Module.StageSupervisor().WaferMaxThickness})",
                                   this.Module.LoaderController().GetChuckIndex());
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {                
                if (Check_N_DO_ForceTransition())
                    return EventCodeEnum.NONE;

                if (Module.SoakingCancelFlag)// 확인 필요
                {
                    LoggerManager.SoakingLog($"-------------- Soaking Cancel(SoakingSubRunning) --------------");
                    ret = SoakingState.SubStateTransition(new SoakingSubAbort(SoakingState, DateTime.Now));
                    return ret;
                }

                //card가 존재하면 Idle 이동하여 Soaking 절차를 다시 진행하도록 한다.(Running으로 가지 않고 Idle로 가자, card가 docking되었으니 처음부터 조건들을 확인하고 진행되도록)
                if (Module.CardChangeModule().IsExistCard())
                {
                    TimeSpan ElapsedTimeInfo = DateTime.Now - startTime;
                    LoggerManager.SoakingLog($"End 'WaitingCardDocking' - elapsed time:{ElapsedTimeInfo.TotalMilliseconds}");

                    string CurSoakingType = GetCurSoakingTypeForLotLog();
                    LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.DONE,
                                           $"Type: {CurSoakingType}, Waiting CardDocking",
                                           this.Module.LoaderController().GetChuckIndex());
                    ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                    return ret;
                }
                else 
                {
                    ///Soaking postion이 변경되었는지 체크하여 변경되었다면 SoakingSubIdle로 보내서 soaking을 다시 진행할 수 있도록 하자.
                    if (IsChangedSoakingPosition(ref ChuckAwayTimeForSoaking))
                    {
                        SoakingAbortLotLog("Chuck position is changed");
                        ret = SoakingState.SubStateTransition(new SoakingSubIdle(SoakingState));
                        return ret;
                    }
                }
                
                StatusSoakingInfoUpdateToLoader(Module.StatusSoakingTempInfo.StatusSoakingTime); //loader쪽 정보 갱신
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;            
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.SUSPENDED_FOR_CARDDOCKING;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //pause가 들어왔을때 현재 진행중이던 soaking을 마무리하고 다음 Resume하면 Idle로 가서 다음 tick에서 처리될 수 있도록 한다.
                LoggerManager.SoakingLog($"Soaking Pause. stop soaking, soaking will run next tick(Cur State:WatingSoakingForTemperature)");
                ChillingTimeProcForSoakingPause(DateTime.Now, false);
                Raising_StatusPreHeatEndEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retVal;
        }
        
        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// soaking 취소 처리 State
    /// </summary>
    public class SoakingSubAbort : SoakingSubStateBase
    {
        private ForceTransitionEnum forceTransitionEnum { get; set; }
        private DateTime beforeSubStateStartTime = default;
        public SoakingSubAbort(ISoakingState state, DateTime beforeStateStartTime, ForceTransitionEnum forceTransitionEnum = ForceTransitionEnum.NOT_NECESSARY) : base(state)
        {
            LoggerManager.Debug($"SoakingSubAbort forceTransitionEnum = {forceTransitionEnum}");

            beforeSubStateStartTime = beforeStateStartTime;
            this.forceTransitionEnum = forceTransitionEnum;


        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }
        
        /// <summary>
        /// Soaking cancel에 대한 처리, Soaking완료 처리로 진행된다.
        /// </summary>
        /// <param name="startTimeInfo"></param>
        /// <returns></returns>
        private bool SoakingCancelProc(DateTime startTimeInfo, ref bool SendedLastPinAlign)
        {
            SendedLastPinAlign = false;
            //현재까지 누적된 chilling time 초기화
            TimeSpan ElapseSoakingTime = DateTime.Now - startTimeInfo;
            long CurrentElapseSoakingTime = Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime + (long)ElapseSoakingTime.TotalMilliseconds;
            Module.StatusSoakingTempInfo.SoakingAbort = true;
            SoakingStateEnum state = SoakingState.GetState();
            if (SoakingStateEnum.MANUAL == state)
            {
                //현재까지 진행된 SoakingTime에 대해 처리 
                var BeforeState = Module.PreInnerState as ISoakingState;
                if (SoakingStateEnum.PREPARE != BeforeState.GetState())
                {
                    //soaking한 시간에 대한 chillingtime을 가져와 감소처리
                    Module.ChillingTimeMngObj.ActualProcessedSoakingTime((int)CurrentElapseSoakingTime, 0, false);
                }
            }
            else
            {
                //event soaking 진행중이었다면 해당 event soaking clear
                if (SoakingStateEnum.STATUS_EVENT_SOAK == Module.StatusSoakingTempInfo.StatusSoaking_State)
                {
                    Module.TriggeredStatusEventSoakList.Clear();
                } 

                LoggerManager.SoakingLog($"Soaking Cancel - SoakingTime:{Module.StatusSoakingTempInfo.StatusSoakingTime}, Elapsed SoakingTime(Sec):{CurrentElapseSoakingTime / 1000}, Remaining soakingTime(Sec):{(Module.StatusSoakingTempInfo.StatusSoakingTime - CurrentElapseSoakingTime) / 1000}");
                Module.ChillingTimeMngObj.ChillingTimeInit();
                Module.InitPrepareSoakingTrigger();
            }

            string SoakingType = GetCurSoakingTypeForLotLog();
            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.DONE,
                                  $"Type: {SoakingType}(Soaking User Stop), device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}",
                                  this.Module.LoaderController().GetChuckIndex());

            if (Module.StatusSoakingTempInfo.PinAlignMode_AfterSoaking != PinAlignType.DoNot_PinAlign)
            {
                if(SoakingStateEnum.STATUS_EVENT_SOAK == Module.StatusSoakingTempInfo.StatusSoaking_State 
                    && (Module.LoaderController().IsCancel == true || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE))
                {
                    //brett// evnet soak중 개별 lot end 하거나 Lot Idle 상태에서 abort 된 경우(lot에 포함된 상태에서 machine init 등으로) pin align을 skip 하기 위함
                    SendedLastPinAlign = false;
                    return true;
                }
                else
                {
                    Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);

                    if(ForceTransitionEnum.NEED_TO_STATUS_SUBCARDCHANGEABORT != this.forceTransitionEnum)
                    {
                        if (Module.Idle_SoakingFailed_PinAlign == false)
                        {
                            if (Module.CommandManager().SetCommand<IDOPinAlignAfterSoaking>(SoakingState.GetModule()) == true)
                            {
                                Module.StatusSoakingTempInfo.FinalPinAlignAfterSoaking = PinAlignForStausSoaking.REQUESTED_PINALIGN;
                                SendedLastPinAlign = true;
                                LoggerManager.SoakingLog($"Soaking Cancel -> Request Pin Align(after soaking end) - Last pin align mode:{Module.StatusSoakingTempInfo.PinAlignMode_AfterSoaking.ToString()}");
                                return true;
                            }
                            else
                            {
                                LoggerManager.SoakingErrLog($"Failed to set CDM(IDOPinAlignAfterSoaking)");
                                return false;
                            }
                        }
                        else
                        {
                            SendedLastPinAlign = false;
                            return true;
                        }
                    }
                    else
                    {
                        SendedLastPinAlign = false;
                        return true;
                    }
                }
            }
            else
                return true;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.SoakingLog($"Soaking Abort. stop soaking, soaking will run next tick(Cur State:SoakingSubAbort)");

                bool SendedLastPinAlign = false;
                bool ProcSoakingCancelFlag = SoakingCancelProc(beforeSubStateStartTime, ref SendedLastPinAlign);
                
                if (ProcSoakingCancelFlag)
                {
                    Module.SetSoakingAbort();
                    if (SendedLastPinAlign)
                    {
                        ret = SoakingState.SubStateTransition(new SoakingSubSuspendForAlign(SoakingState, false));
                        return ret;
                    }
                    else
                        ret = SoakingState.SubStateTransition(new SoakingSubDone(SoakingState));

                    Raising_StatusPreHeatEndEvent();
                }
                else
                    ret = EventCodeEnum.SOAKING_ERROR_SET_CMD;

                return ret;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return ret;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }
        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.ABORT;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class SoakingSubPause : SoakingSubStateBase
    {
        public SoakingSubPause(ISoakingState state) : base(state)
        {
           
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }
        
        public override EventCodeEnum Execute()
        {
            if (Check_N_DO_ForceTransition())
                return EventCodeEnum.NONE;
            
            return EventCodeEnum.NONE;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }
        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.PAUSE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retVal;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// soaking동작을 위해 다른 연관있는 다른 Module이 Idle이 될때까지 대기한다.(ex::Polish wafer))
    /// </summary>
    public class WaitingRelatedModule : SoakingSubStateBase
    {
        private DateTime StartTime;
        public WaitingRelatedModule(ISoakingState state) : base(state)
        {
            StartTime = DateTime.Now;
            string SoakingType = SoakingState.GetState().ToString();
            if (SoakingState.GetState() == SoakingStateEnum.STATUS_EVENT_SOAK)
            {
                if (Module.StatusSoakingTempInfo.SoakingEvtType == EventSoakType.EveryWaferSoak)
                    SoakingType = "EVERYWAFER";
            }

            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.SUSPEND,
                                   $"Type: {SoakingType}, Waiting RelateModule",
                                   this.Module.LoaderController().GetChuckIndex());
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;

            if (Module.SoakingCancelFlag)
            {
                if (SoakingState.GetState() != SoakingStateEnum.MAINTAIN)
                {
                    LoggerManager.SoakingLog($"-------------- Soaking Cancel(WaitingRelatedModule) --------------");
                    ret = SoakingState.SubStateTransition(new SoakingSubAbort(SoakingState, StartTime));
                }
                else
                {
                    LoggerManager.SoakingLog($"-------------- Soaking Cancel(WaitingRelatedModule - Maintain soaking. don't any action ) --------------");
                    Module.SoakingCancelFlag = false;
                }

                return ret;
            }

            //PW module이 IDLE이 될때까지 대기한다.
            if (Module.PolishWaferModule().ModuleState.GetState() == ModuleStateEnum.IDLE && Module.SequenceEngineManager().GetRunState())
            {
                LoggerManager.SoakingLog($"Polish wafer module state is IDLE. So State is changed to SubRunning.");
                ret = SoakingState.SubStateTransition(new SoakingSubRunning(SoakingState));
                return ret;
            }

            if (Check_N_DO_ForceTransition())
                return EventCodeEnum.NONE;

            return EventCodeEnum.NONE;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.SUSPENDED_FOR_OTHER_MODULE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //pause가 들어왔을때 현재 진행중이던 soaking을 마무리하고 다음 Resume하면 Idle로 가서 다음 tick에서 처리될 수 있도록 한다.
                LoggerManager.SoakingLog($"Soaking Pause. stop soaking, soaking will run next tick(Cur State:WaitingRelatedModule)");
                ChillingTimeProcForSoakingPause(DateTime.Now, false);
                Raising_StatusPreHeatEndEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retVal;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class SoakingSubMaintainAbortState : SoakingSubStateBase
    {
        //private DateTime beforeSubStateStartTime = default;
        private SoakingStateEnum PrevSubState;
        public SoakingSubMaintainAbortState(ISoakingState state) : base(state)
        {
            string SoakingType = SoakingState.GetState().ToString();

            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.SUSPEND,
                                   $"Type: {SoakingType}, SoakingSubMaintainAbortState",
                                   this.Module.LoaderController().GetChuckIndex());
        }
        public SoakingSubMaintainAbortState(ISoakingState state, SoakingStateEnum prevstate) : base(state)
        {
            string SoakingType = SoakingState.GetState().ToString();
            PrevSubState = prevstate;

            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.SUSPEND,
                                   $"Type: {SoakingType}, SoakingSubMaintainAbortState",
                                   this.Module.LoaderController().GetChuckIndex());
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            try
            {
                //모든 동작 다 끝 남. prevstate = SUSPENDED_FOR_MAINTAIN_ABORT;
                //if (PrevSubState != SoakingStateEnum.PAUSE&& PrevSubState != SoakingStateEnum.ERROR)
                //{
                //    //칠링타임 정리 
                //    //pause가 들어왔을때 현재 진행중이던 soaking을 마무리하고 다음 Resume하면 Idle로 가서 다음 tick에서 처리될 수 있도록 한다.
                //    LoggerManager.SoakingLog($"Soaking Pause. stop soaking, soaking will run next tick(Cur State:WatingSoakingSubRunning)");
                //    ChillingTimeProcForSoakingPause(DateTime.Now, false);
                //}
                if(PrevSubState != SoakingStateEnum.PAUSE && PrevSubState != SoakingStateEnum.ERROR && PrevSubState != SoakingStateEnum.IDLE)
                {
                    //칠링타임 정리 
                    //pause가 들어왔을때 현재 진행중이던 soaking을 마무리하고 다음 Resume하면 Idle로 가서 다음 tick에서 처리될 수 있도록 한다.
                    LoggerManager.SoakingLog($"Soaking Pause. stop soaking, soaking will run next tick(Cur State:WatingSoakingSubRunning)");
                    ChillingTimeProcForSoakingPause(DateTime.Now, false);
                }

                Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NEED_TO_STATUS_SUBIDLE;

                if (PrevSubState != SoakingStateEnum.IDLE)
                {
                    Raising_StatusPreHeatEndEvent();
                }

                if (Module.TriggeredStatusEventSoakList.Count() > 0)
                {
                    Module.TriggeredStatusEventSoakList.Clear();

                    if (SoakingState.GetState() == SoakingStateEnum.STATUS_EVENT_SOAK)
                    {
                        Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NEED_TO_STATUS_SUBABORT;
                    }
                }

                if (Check_N_DO_ForceTransition())
                    return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                ret = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
           
            return ret;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.SUSPENDED_FOR_MAINTAIN_ABORT;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
        }
    }
}

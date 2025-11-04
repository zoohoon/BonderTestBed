
namespace Soaking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using InternalCommands;
    using LogModule;
    using MetroDialogInterfaces;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Event;
    using ProberInterfaces.Param;
    using ProberInterfaces.Soaking;
    using ProberInterfaces.State;
    using SoakingParameters;

    public abstract class SoakingState : ISoakingState//IInnerState
    {
        public abstract SoakingStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();
        public abstract bool CanExecute(IProbeCommandToken token);

        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();

        public virtual EventCodeEnum End()
        {
            LoggerManager.Debug($"Soaking - 'End()' function is not implemented.(Current soaking state:{GetType().ToString()})");
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

        public virtual EventCodeEnum SubStateTransition(ISoakingSubState innerState)
        {
            return EventCodeEnum.NONE;
        }

        public abstract ISoakingModule GetModule();

    }

    public abstract class StatusSoakingStateBase : SoakingStateBase
    {
        protected bool WriteLogAboutDeterminetheSoaking = false;
        public StatusSoakingStateBase(SoakingModule module) : base(module)
        {
        }
        public ISoakingSubState SubState { get; protected set; }

        public sealed override EventCodeEnum SubStateTransition(ISoakingSubState innerState)
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                if (null != SubState)
                {
                    LoggerManager.SoakingLog($"Transition : before: {SubState.GetType().Name}({SubState.GetState().ToString()}), Current: {innerState.GetType().Name}({innerState.GetState().ToString()})");

                    if (innerState is SoakingSubIdle)
                    {
                        if (SubState is SoakingSubSuspendForWaferObject) //Idle로 Transition할때 Soaking Done 로그 기록(WaitSoakingRunningState 와 SoakingSubSuspendForWaferObject는 시작로그를 남김)
                        {
                            var WaitforWaferState = SubState as SoakingSubSuspendForWaferObject;
                            if (null != WaitforWaferState)
                                WaitforWaferState.SoakingDone_ForLotLog();
                        }
                        else if (SubState is WaitingSoakingSubRunning)
                        {
                            var WaitforSoakingTime = SubState as WaitingSoakingSubRunning;
                            if (null != WaitforSoakingTime)
                                WaitforSoakingTime.SoakingDone_ForLotLog();
                        }
                    }

                    //Soaking Cancel로 인한 Abort가 들어온 경우(사용자 soaking stop) Soaking Done 로그 기록
                    if (innerState is SoakingAbortState)
                    {
                        if (SubState is SoakingSubSuspendForWaferObject || SubState is WaitingSoakingSubRunning)
                        {
                            if (SubState is SoakingSubSuspendForWaferObject)
                            {
                                var WaitforWaferState = SubState as SoakingSubSuspendForWaferObject;
                                if (null != WaitforWaferState)
                                    WaitforWaferState.SoakingDone_ForLotLog();
                            }
                            else
                            {
                                var WaitSoakingRunning = SubState as WaitingSoakingSubRunning;
                                if (null != WaitSoakingRunning)
                                    WaitSoakingRunning.SoakingDone_ForLotLog();
                            }
                        }
                    }
                }

                this.SubState = innerState;
                Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SOAKING, this.SubState.GetModuleState().ToString());
                eventCodeEnum = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"SubStateTransition(): Error occurred. Err = {err.Message}");
                eventCodeEnum = EventCodeEnum.EXCEPTION;
            }
            return eventCodeEnum;
        }
        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;

            try
            {
                Module.Idle_SoakingFailed_PinAlign = false;
                Module.Idle_SoakingFailed_WaferAlign = false;

                retval = SubState.Pause(); //soaking SubState 에서의 Pause 함수는 현재까지 진행중이던 Soaking을 정리해준다. 오해하지 말것                                
                if (Module.TriggeredStatusEventSoakList.Count() > 0)
                {
                    LoggerManager.SoakingLog($"Clear State - EventSoak Count:{Module.TriggeredStatusEventSoakList.Count()}");
                    Module.TriggeredStatusEventSoakList.Clear();
                }

                string CurSoakingType = GetState().ToString();
                if (GetState() == SoakingStateEnum.STATUS_EVENT_SOAK)
                {
                    if (Module.StatusSoakingTempInfo.SoakingEvtType == EventSoakType.EveryWaferSoak)
                        CurSoakingType = "EVERYWAFER";
                }

                if (GetState() != SoakingStateEnum.MAINTAIN ||
                    (GetState() == SoakingStateEnum.MAINTAIN && SubState is WaitingSoakingSubRunning)
                    )
                {
                    LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.ABORT,
                                 $"Type: {CurSoakingType}(Clear State), device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}", this.Module.LoaderController().GetChuckIndex());

                }

                LoggerManager.SoakingLog($"Clear State Immediately(because of current state is error.");
                SubStateTransition(new SoakingSubIdle(this));

                Module.Clear_SoakingInfoTxt();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            return retval;
        }
        public SoakingStateEnum DeterminetheSoakingstate(bool KeepMyselfState = true)
        {
            long accumulated_chillingTime = 0;//임의의 값
            int SoakingTimeMil = 0;//임의의 값
            bool InChillingTimeTable = false;//임의의 값
            EventCodeEnum ret = Module.ChillingTimeMngObj.GetCurrentChilling_N_TimeToSoaking(ref accumulated_chillingTime, ref SoakingTimeMil, ref InChillingTimeTable);
            if (ret != EventCodeEnum.NONE)
            {
                if (false == WriteLogAboutDeterminetheSoaking)
                {
                    LoggerManager.SoakingLog($"[Error] Failed to get 'GetCurrentChilling_N_TimeToSoaking'");
                    WriteLogAboutDeterminetheSoaking = true;
                }
            }

            SoakingStateEnum decided_SoakingState;
            if (Module.ToCheckEventsForPrepareState() == true)
            {
                decided_SoakingState = SoakingStateEnum.PREPARE;
                Module.TempDiffTriggered = false;
                Module.ProbeCardChangeTriggered = false;
                Module.DeviceChangeTriggered = false;
            }
            else if (InChillingTimeTable)
            {
                decided_SoakingState = SoakingStateEnum.RECOVERY;
            }
            else
            {
                if (KeepMyselfState)
                    decided_SoakingState = this.GetState();
                else
                    decided_SoakingState = SoakingStateEnum.MAINTAIN;
            }

            return decided_SoakingState;
        }

        public abstract EventCodeEnum GetParameter();

        public override EventCodeEnum Abort()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (Module.ManualSoakingStart) //manual soaking중 중단이라면 error여부 체크
                {
                    Module.ManualSoakingStart = false;
                    if (Module.Idle_SoakingFailed_PinAlign)
                        Module.ManualSoakingRetVal = EventCodeEnum.SOAKING_ERROR_IDLE_PINALIGN;
                    else if (Module.Idle_SoakingFailed_WaferAlign)
                        Module.ManualSoakingRetVal = EventCodeEnum.SOAKING_ERROR_IDLE_WAFERALIGN;
                }

                retVal = SubState.Pause(); //soaking SubState 에서의 Pause 함수는 현재까지 진행중이던 Soaking을 정리해준다. 오해하지 말것 
                Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NEED_TO_STATUS_SUBIDLE;

                LoggerManager.SoakingLog($"Abort - Force change state(NEED_TO_STATUS_SUBIDLE)");

                if (Module.Idle_SoakingFailed_PinAlign)
                    Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "PIN ALIGNMENT FAILED(SOAKING)");

                if (Module.Idle_SoakingFailed_WaferAlign)
                    Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "WAFER ALIGNMENT FAILED(SOAKING)");

                if (Module.UsePreviousStatusSoakingDataForRunning)
                {
                    Module.UsePreviousStatusSoakingDataForRunning = false;
                    LoggerManager.SoakingLog($"Abort - UsePreviousStatusSoakingDataForRunning flag off");
                }

                if (SubState is SoakingSubIdle)
                    return EventCodeEnum.NONE;

                string SoakingType = "";
                var SubStateBase = SubState as SoakingSubStateBase;
                if (null != SubStateBase)
                {
                    SoakingType = SubStateBase.GetCurSoakingTypeForLotLog();
                }

                //Maintain 상태는 Soaking이 완료된 상태로 Abort에 대한 Log는 남기지 않는다.(단 Lot Idle에서 Chuck에 target wafer가 있어 실제 soaking중인 경우는 abort 처리 후 soaking done로그 필요)
                if (GetState() != SoakingStateEnum.MAINTAIN ||
                   (GetState() == SoakingStateEnum.MAINTAIN && SubState is WaitingSoakingSubRunning)
                   )
                {
                    if (false == Module.Idle_SoakingFailed_PinAlign && false == Module.Idle_SoakingFailed_WaferAlign)
                    {
                        LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.ABORT,
                                      $"Type: {SoakingType}(Maintenace mode), device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}", this.Module.LoaderController().GetChuckIndex());

                        if (SubState is SoakingSubError)
                        {
                            string CurSoakingType = GetState().ToString();
                            if (GetState() == SoakingStateEnum.STATUS_EVENT_SOAK)
                            {
                                if (Module.StatusSoakingTempInfo.SoakingEvtType == EventSoakType.EveryWaferSoak)
                                    CurSoakingType = "EVERYWAFER";
                            }

                            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.DONE,
                                                   $"Type: {CurSoakingType}, device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}",
                                                   this.Module.LoaderController().GetChuckIndex());
                        }
                    }
                }

                if (Module.MonitoringManager().IsMachineInitDone != true)//Machine init 이 깨진 상태라면 Enum만 Set하는 것이 아니라 상태를 직접 바꿔준다. Enum을 Set해도 다음 Tick에서 Execute가 돌지 못할 것이기 때문.
                {
                    ForceChagneSoakingSubIdleState();
                    Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SOAKING, this.SubState.GetModuleState().ToString());
                    LoggerManager.SoakingLog($"Force change state(SoakingSubIdle). IsMachineInitDone:{Module.MonitoringManager().IsMachineInitDone}");
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
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = SubState.Pause(); //sub statue에 pause를 호출하여 soaking중이던게 있으면 현 호출시점까지의 시간으로 soaking 처리를 완료(다음 tick에서 다시 soaking처리할 수 있음)

                // SoakingSubSuspendForWaferObject와 WaitingSoakingSubRunning 생성자에서 Soakgin Start로그가 기록됨.
                if ((SubState is SoakingSubSuspendForWaferObject) || (SubState is WaitingSoakingSubRunning))
                {
                    string SoakingType = "";
                    var SubStateBase = SubState as SoakingSubStateBase;
                    if (null != SubStateBase)
                    {
                        SoakingType = SubStateBase.GetCurSoakingTypeForLotLog();
                    }

                    LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.ABORT,
                                      $"Type: {SoakingType}(Lot Pause), device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}", this.Module.LoaderController().GetChuckIndex());

                    if (SubState is SoakingSubSuspendForWaferObject) //Idle로 Transition할때 Soaking Done 로그 기록(WaitSoakingRunningState 와 SoakingSubSuspendForWaferObject는 시작로그를 남김)
                    {
                        var WaitforWaferState = SubState as SoakingSubSuspendForWaferObject;
                        if (null != WaitforWaferState)
                            WaitforWaferState.SoakingDone_ForLotLog();
                    }
                    else if (SubState is WaitingSoakingSubRunning)
                    {
                        var WaitforSoakingTime = SubState as WaitingSoakingSubRunning;
                        if (null != WaitforSoakingTime)
                            WaitforSoakingTime.SoakingDone_ForLotLog();
                    }
                }

                Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NEED_TO_STATUS_SUBPAUSE;
                if (SoakingStateEnum.PAUSE != SubState.GetState())
                {
                    LoggerManager.SoakingLog($"Pause - Force change state(NEED_TO_STATUS_SUBPAUSE)");
                    if (Module.MonitoringManager().IsMachineInitDone != true)
                    {
                        SubStateTransition(new SoakingSubPause(this));
                        Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SOAKING, this.SubState.GetModuleState().ToString());
                        LoggerManager.SoakingLog($"Force change state(SoakingSubPause). IsMachineInitDone:{Module.MonitoringManager().IsMachineInitDone}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (SubState is SoakingSubPause)
                {
                    LoggerManager.SoakingLog($"Resume Proc(Pause -> Idle)");
                    SubStateTransition(new SoakingSubIdle(this));
                }
                else
                {
                    Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NEED_TO_STATUS_SUBIDLE;
                    if (SoakingStateEnum.IDLE != SubState.GetState())
                        LoggerManager.SoakingLog($"Resume - Force change state(NEED_TO_STATUS_SUBIDLE)");
                }
                //SetStatusSoakingForceTransitionState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            SetStatusSoakingForceTransitionState();
            //Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NEED_TO_STATUS_SUBIDLE;
            //if (SoakingStateEnum.IDLE != SubState.GetState())
            //    LoggerManager.SoakingLog($"End - Force change state(NEED_TO_STATUS_SUBIDLE)");

            return EventCodeEnum.NONE;
        }

        /// <summary>
        /// Soaking State를 SoakingSubIdle로 변경한다. 호출시점에 바로 SoakingSubIdle로 변경 처리됨.
        /// </summary>
        public void ForceChagneSoakingSubIdleState()
        {
            SubStateTransition(new SoakingSubIdle(this));
        }

        public void SetStatusSoakingForceTransitionState()
        {
            Module.StatusSoakingForceTransitionState = ForceTransitionEnum.NEED_TO_STATUS_SUBMAINTAINABORT;
        }
    }

    public abstract class SoakingStateBase : SoakingState
    {
        protected static Random rnd = new Random();
        protected static DateTime StartTime;
        protected static DateTime EndTime;

        private SoakingModule _Module;
        public SoakingModule Module
        {
            get { return _Module; }
            protected set { _Module = value; }
        }

        public SoakingStateBase(SoakingModule module)
        {
            Module = module;
        }
        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.ClearSoakPriorityList();
                retval = Module.InnerStateTransition(new SoakingIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override ISoakingModule GetModule()
        {
            ISoakingModule soakingModule = null;

            try
            {
                soakingModule = Module;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetModule(): Error occurred. Err = {err.Message}");
            }
            return soakingModule;
        }
        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (GetState() == SoakingStateEnum.ERROR) //soaking error일때 처리
                    retVal = ClearState();
                else
                {
                    SoakingInfo soakinfo = new SoakingInfo();
                    soakinfo.SoakingType = "No Soak";
                    soakinfo.RemainTime = 0;
                    soakinfo.ZClearance = 0;
                    soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                    Module.LoaderController()?.UpdateSoakingInfo(soakinfo);
                    retVal = Module.InnerStateTransition(new SoakingAbortState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ResultValidate(object funcname, EventCodeEnum retcode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (retcode != EventCodeEnum.NONE)
                {
                    throw new Exception(string.Format("FunctionName: {0} Returncode: {1} Error occurred", funcname.ToString(), retcode.ToString()));
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        public WaferCoordinate ConvertToWaferCoordinateFromUserIndex(int xuserindex, int yuserindex)
        {
            MachineIndex machine = Module.CoordinateManager().WUIndexConvertWMIndex(xuserindex, yuserindex);

            WaferCoordinate wafercoord = new WaferCoordinate();
            try
            {
                int xindex = Convert.ToInt32(machine.XIndex);
                int yindex = Convert.ToInt32(machine.YIndex);
                wafercoord = Module.WaferAligner().MachineIndexConvertToDieLeftCorner(xindex, yindex);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return wafercoord;
        }
        protected string ConvertStringSoakType(EnumSoakingType soaktype)
        {
            string soakingtype = null;
            switch (soaktype)
            {
                case EnumSoakingType.UNDEFINED:
                    soakingtype = "UNDEFINED";
                    break;
                case EnumSoakingType.LOTRESUME_SOAK:
                    soakingtype = "LotResume";
                    break;
                case EnumSoakingType.CHUCKAWAY_SOAK:
                    soakingtype = "ChuckAway";
                    break;
                case EnumSoakingType.TEMPDIFF_SOAK:
                    soakingtype = "TempDiff";
                    break;
                case EnumSoakingType.PROBECARDCHANGE_SOAK:
                    soakingtype = "ProbeCardChange";
                    break;
                case EnumSoakingType.LOTSTART_SOAK:
                    soakingtype = "LotStart";
                    break;
                case EnumSoakingType.DEVICECHANGE_SOAK:
                    soakingtype = "DeviceChange";
                    break;
                case EnumSoakingType.EVERYWAFER_SOAK:
                    soakingtype = "EveryWafer";
                    break;
                case EnumSoakingType.AUTO_SOAK:
                    soakingtype = "AutoSoak";
                    break;
                default:
                    break;
            }
            return soakingtype;
        }
        protected EventCodeEnum WaitForSoaking(int soakTime)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                Stopwatch stw = new Stopwatch();
                stw.Start();
                bool runflag = true;
                Module.SoakingCancelFlag = false;
                while (runflag)
                {
                    if (Module.SoakingCancelFlag == true)
                    {
                        runflag = false;
                        Module.SoakingCancelFlag = false;
                        ret = EventCodeEnum.SOAKING_CANCLE;
                    }
                    if (stw.Elapsed.TotalSeconds >= soakTime)
                    {
                        ret = EventCodeEnum.NONE;
                        runflag = false;
                        stw.Stop();
                    }
                }
            }
            catch (Exception err)
            {
                throw new Exception(string.Format("Class: {0} Function: {1} ReturnValue: {2} HashCode: {3} ExceptionMessage: {4} ", this, MethodBase.GetCurrentMethod(), ret.ToString(), err.GetHashCode(), err.Message));
            }

            return ret;
        }

        protected bool _isPinAlignNeededCalled = false;

        private bool CheckPinAlignValidTime()
        {
            bool retval = false;

            try
            {
                DateTime EndDate = DateTime.Now;

                TimeSpan dateDiff = EndDate - Module.StageSupervisor().PinAligner().LastAlignDoneTime;

                //Pin Align이 Pin_Align_Valid_Time 지났나 안지났나. 지났으면 IDLE로 
                if (Module.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE)
                {
                    if (dateDiff.TotalMinutes >= Module.SoakingDeviceFile_Clone.AutoSoakingParam.Pin_Align_Valid_Time.Value)
                    {
                        Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        protected bool IsPinAlignNeeded()
        {
            if (_isPinAlignNeededCalled)
            {
                return false;
            }

            bool retval = false;

            try
            {
                if (Module.SoakingDeviceFile_Clone.AutoSoakingParam.IdleSoak_AlignTrigger.Value == true)
                {
                    retval = CheckPinAlignValidTime();

                    if (Module.CardChangeModule().IsExistCard() &&
                        Module.StageSupervisor().ProbeCardInfo.AlignState.Value != AlignStateEnum.DONE)
                    {
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                _isPinAlignNeededCalled = true;
            }

            return retval;
        }
        protected EventCodeEnum SoakingFunc(double zClearance, int timeInSeconds, EnumSoakingType soakingType)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                int time = timeInSeconds;

                if (zClearance > 0)
                {
                    return EventCodeEnum.STAGEMOVE_MOVE_TO_SOAKING_ERROR;
                }

                WaferCoordinate wafercoord = new WaferCoordinate();
                PinCoordinate pincoord = new PinCoordinate();

                LoggerManager.Debug($"Soaking START");

                try
                {
                    // IdleSoak_AlignTrigger 파라미터의 위치가 AutoSoakingParam 내부에 있어, AutoSoaking에서만 동작하는 파라미터라고 생각할 수 있겠지만, 아님. 
                    if (Module.SoakingDeviceFile_Clone.AutoSoakingParam.IdleSoak_AlignTrigger.Value == true)
                    {
                        ret = Module.GetSoakingPosition(ref wafercoord, ref pincoord, soakingType);

                        LoggerManager.Debug($"{GetType().Name}, IdleSoak_AlignTrigger = {Module.IdleSoak_AlignTrigger}");
                        LoggerManager.Debug($"before soaking function. get wafer height: {wafercoord.Z.Value}, get pin height: {pincoord.Z.Value}");
                    }
                    else
                    {
                        ret = Module.GetTargetPosAccordingToCondition(ref wafercoord, ref pincoord, true);

                        LoggerManager.Debug($"{GetType().Name}, IdleSoak_AlignTrigger = {Module.IdleSoak_AlignTrigger}");
                        LoggerManager.Debug($"before soaking function. get wafer height: {wafercoord.Z.Value}, get pin height: {pincoord.Z.Value}");
                    }

                    if (ret == EventCodeEnum.NONE)
                    {
                        ret = Module.StageSupervisor().StageModuleState.MoveToSoaking(wafercoord, pincoord, zClearance);
                        ResultValidate(MethodBase.GetCurrentMethod(), ret);
                        Module.SoakingCancelFlag = false;
                        if (ret == EventCodeEnum.NONE)
                        {
                            if (Module.StageSupervisor().ProbeCardInfo.AlignState.Value != AlignStateEnum.DONE && 
                                Module.CardChangeModule().IsExistCard() && 
                                Module.SoakingDeviceFile_Clone.AutoSoakingParam.IdleSoak_AlignTrigger.Value)
                            {
                                ret = EventCodeEnum.PIN_ALIGN_FAILED;
                                LoggerManager.Error($"SoakingFunc() : SoakingRunningState.Error occurred while Soaking : Pin align fail.");
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"SoakingRunningState.Error occurred while Soaking ");
                            ret = EventCodeEnum.SOAKING_ERROR;
                        }
                    }
                    else
                    {
                        LoggerManager.Error("Unknown Error");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Class: {0} Function: {1} ReturnValue: {2} HashCode: {3} ExceptionMessage: {4} ", this, MethodBase.GetCurrentMethod(), ret.ToString(), ex.GetHashCode(), ex.Message));
                }

                LoggerManager.Debug($"Soaking END");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        bool overlapthreeLegLog = false;
        protected EventCodeEnum CheckCardModuleAndThreeLeg()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                bool isthreelegup = false;
                bool isthreelegdown = false;

                // TODO : ThreeLeg Check 문제 찾고 고쳐야 함.
                ret = Module.MotionManager().IsThreeLegUp(EnumAxisConstants.TRI, ref isthreelegup);
                ret = Module.MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isthreelegdown);

                if (isthreelegup == false && isthreelegdown == true)
                {
                    ret = EventCodeEnum.NONE;
                    overlapthreeLegLog = false;
                }
                else
                {
                    if (overlapthreeLegLog == false)
                    {
                        LoggerManager.Error($"[GP CC]=> ThreelegDown:{isthreelegdown} ThreelegUp:{isthreelegup}");
                        overlapthreeLegLog = true;
                    }
                    ret = EventCodeEnum.STAGEMOVE_THREE_LEG_DOWN_ERROR;
                    return ret;
                }

                if (Extensions_IParam.ProberRunMode != RunMode.EMUL && SystemManager.SysteMode != SystemModeEnum.Single)
                {
                    bool diupmodule_left_sensor;
                    var ioret = Module.IOManager().IOServ.ReadBit(Module.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diupmodule_left_sensor);

                    bool diupmodule_right_sensor;
                    ioret = Module.IOManager().IOServ.ReadBit(Module.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diupmodule_right_sensor);

                    bool diupmodule_cardexist_sensor;
                    ioret = Module.IOManager().IOServ.ReadBit(Module.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_cardexist_sensor);

                    if (diupmodule_left_sensor || diupmodule_right_sensor)
                    {
                        ret = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                        //LoggerManager.Error($"[GP CC]=> Upmodule_Left:{diupmodule_left_sensor} Upmodule_Right:{diupmodule_right_sensor} CardExist:{diupmodule_cardexist_sensor}");
                        return ret;
                    }

                    if (diupmodule_cardexist_sensor)
                    {
                        ret = EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD;
                        //LoggerManager.Error($"[GP CC]=> Upmodule_Left:{diupmodule_left_sensor} Upmodule_Right:{diupmodule_right_sensor} CardExist:{diupmodule_cardexist_sensor}");
                        return ret;
                    }

                    bool dipogocard_vacu_sensor;
                    ioret = Module.IOManager().IOServ.ReadBit(Module.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);
                    if (dipogocard_vacu_sensor != true)
                    {
                        //ret = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                        ret = EventCodeEnum.NONE;//Card가 Doaking되어있지 않아도 Auto Soaking은 돌아야 해서.
                        //LoggerManager.Error($"[GP CC]=> PogoCardVac:{dipogocard_vacu_sensor}");
                        return ret;
                    }
                    bool ditplate_pclatch_sensor_lock;
                    ioret = Module.IOManager().IOServ.ReadBit(Module.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                    bool ditplate_pclatch_sensor_unlock;
                    ioret = Module.IOManager().IOServ.ReadBit(Module.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                    if (ditplate_pclatch_sensor_lock == false || ditplate_pclatch_sensor_unlock == true)
                    {
                        //ret = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                        ret = EventCodeEnum.NONE;//Card가 Doaking되어있지 않아도 Auto Soaking은 돌아야 해서.
                        LoggerManager.Error($"[GP CC]=> Latch_Lock:{ditplate_pclatch_sensor_lock} Latch_Unlock:{ditplate_pclatch_sensor_unlock}");
                        return ret;
                    }
                }

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        protected EventCodeEnum RaisingPreHeatStartEvent(double timeSec)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.GEMModule().GetPIVContainer().SoakingTimeSec.Value = timeSec;
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
        protected EventCodeEnum RaisingPreHeatEndEvent()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.GEMModule().GetPIVContainer().SoakingTimeSec.Value = 0;
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Module.EventManager().RaisingEvent(typeof(PreHeatEndEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        protected EventCodeEnum RaisingPreHeatFailEvent()
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
    }

    public class SoakingIdleState : SoakingStateBase
    {
        public SoakingIdleState(SoakingModule module) : base(module)
        {
            Module.LogWrite_CheckEventSoakingTrigger_ChuckAway = false;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public bool IsSoakingCondition()
        {
            bool retval = false;

            try
            {
                switch (Module.StageSupervisor().StageMoveState)
                {
                    // WHITE LIST

                    case StageStateEnum.IDLE:
                    case StageStateEnum.Z_CLEARED:

                        retval = true;
                        break;

                    // BLACK LIST

                    case StageStateEnum.UNKNOWN:
                        break;
                    case StageStateEnum.ERROR:
                        break;
                    case StageStateEnum.MOVING:
                        break;
                    case StageStateEnum.MOVETONEXTDIE:
                        break;
                    case StageStateEnum.Z_UP:
                        break;
                    case StageStateEnum.Z_IDLE:
                        break;
                    case StageStateEnum.Z_DOWN:
                        break;
                    case StageStateEnum.MOVETOLOADPOS:
                        break;
                    case StageStateEnum.MOVETODOWNPOS:
                        break;
                    case StageStateEnum.WAFERVIEW:
                        break;
                    case StageStateEnum.WAFERHIGHVIEW:
                        break;
                    case StageStateEnum.WAFERLOWVIEW:
                        break;
                    case StageStateEnum.PINVIEW:
                        break;
                    case StageStateEnum.PINHIGHVIEW:
                        break;
                    case StageStateEnum.PINLOWVIEW:
                        break;
                    case StageStateEnum.PROBING:
                        break;
                    case StageStateEnum.SOAKING:
                        break;
                    case StageStateEnum.CARDCHANGE:
                        break;
                    case StageStateEnum.TILT:
                        break;
                    case StageStateEnum.AIRBLOW:
                        break;
                    case StageStateEnum.CHUCKTILT:
                        break;
                    case StageStateEnum.MARK:
                        break;
                    case StageStateEnum.VISIONMAPPING:
                        break;
                    case StageStateEnum.NC_CLEANING:
                        break;
                    case StageStateEnum.NC_PADVIEW:
                        break;
                    case StageStateEnum.NC_SENSING:
                        break;
                    case StageStateEnum.NC_SENSORVIEW:
                        break;
                    case StageStateEnum.MANUAL:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }



        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.MonitoringManager().IsMachineInitDone == true)
                {
                    //오토소킹 페이지 뭐로 해야할지 물어봐야함 그래서 주석침

                    ModuleStateEnum lotOpState = Module.LotOPModule().ModuleState.GetState();
                    //IMainScreenView mainscreen = Module.Prober.ViewModelManager.MainScreenView;
                    StageStateEnum stagestate = Module.StageSupervisor().StageModuleState.GetState();
                    AlignStateEnum pinalignstate = Module.StageSupervisor().ProbeCardInfo.AlignState.Value;
                    AlignStateEnum waferalignstate = Module.StageSupervisor().WaferObject.AlignState.Value;
                    AlignStateEnum markalignstate = Module.StageSupervisor().MarkObject.AlignState.Value;
                    //var probingstate = Module.ProbingModule().ProbingStateEnum;

                    //var tempguid = Module.SoakingDevFile.WhiteList.Where(item => item == mainscreen.ViewGUID).FirstOrDefault();

                    //if (tempguid == new Guid())
                    //{
                    //    isPossiblePage = true;
                    //}
                    //else
                    //{
                    //    isPossiblePage = false;
                    //}
                    if (lotOpState == ModuleStateEnum.PAUSING)
                    {
                        Module.InnerStateTransition(new SoakingPauseState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                    }
                    //런에서 돌아야 할때 
                    if (lotOpState == ModuleStateEnum.RUNNING)
                    {
                        if (pinalignstate == AlignStateEnum.DONE &&
                            markalignstate == AlignStateEnum.DONE &&
                            Module.SequenceEngineManager().GetRunState() & !Module.SoackingDone &&
                            Module.ProbingModule().ModuleState.GetState() != ModuleStateEnum.SUSPENDED &&
                            Module.LoaderController().ModuleState.GetState() != ModuleStateEnum.SUSPENDED &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROBING &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.TESTED &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED &&
                            Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.NOT_EXIST)
                        {
                            if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                            {
                                return EventCodeEnum.NONE;
                            }

                            ret = Module.CheckEventSoakingTrigger();
                            var chuckawayobj = Module.GetEventSoakingObject(EnumSoakingType.CHUCKAWAY_SOAK);

                            //if (chuckawayobj?.Triggered == true)
                            if (Module.ChuckAwayTriggered == true)
                            {
                                LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.START,
                                    $"Type: ChuckAwaySoak, Soaking OD: {Module.GetEventSoakingObject(EnumSoakingType.CHUCKAWAY_SOAK)?.ZClearance.Value}," +
                                    $"Time(sec): {Module.GetEventSoakingObject(EnumSoakingType.CHUCKAWAY_SOAK)?.SoakingTimeInSeconds.Value} ",
                                    this.Module.LoaderController().GetChuckIndex());

                                Module.InnerStateTransition(new ChuckAwaySoakingRunningState(Module));
                                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                            }
                        }
                        else if (waferalignstate == AlignStateEnum.DONE &&
                            pinalignstate == AlignStateEnum.DONE &&
                            Module.SequenceEngineManager().GetRunState() & !Module.SoackingDone &&
                            Module.ProbingModule().ModuleState.GetState() != ModuleStateEnum.SUSPENDED &&
                            Module.LoaderController().ModuleState.GetState() != ModuleStateEnum.SUSPENDED &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROBING &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.TESTED &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED)
                        {
                            if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                            {
                                if (Module.SoakPriorityList.Count != 0 && Module.SoakPriorityList != null)
                                {
                                    Module.SoakPriorityList.Clear();
                                    if (Module.GetLotResumeTriggeredFlag())
                                    {
                                        Module.LotResumeTriggered = false;
                                    }
                                }

                                return EventCodeEnum.NONE;
                            }

                            ret = Module.UpdateEventSoakingPriority();

                            if (Module.SoakPriorityList.Count != 0 && Module.SoakPriorityList != null)
                            {
                                Module.InnerStateTransition(new EventSoakingRunningState(Module));
                                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                            }
                        }

                        else if (waferalignstate == AlignStateEnum.DONE &&
                                 pinalignstate == AlignStateEnum.DONE &&
                                 Module.SequenceEngineManager().GetRunState() & !Module.SoackingDone &&
                                 Module.LoaderController().ModuleState.GetState() != ModuleStateEnum.SUSPENDED &&
                                 Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED &&
                                 Module.ProbingModule().ProbingStateEnum == EnumProbingState.SUSPENDED)
                        {
                            if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                            {
                                if (Module.SoakPriorityList.Count != 0 && Module.SoakPriorityList != null)
                                {
                                    Module.SoakPriorityList.Clear();
                                }

                                return EventCodeEnum.NONE;
                            }

                            //z up 전에 resume soak trigger check.
                            if (Module.LotResumeTriggered == true)
                            {
                                ret = Module.UpdateEventSoakingPriority();
                                if (Module.SoakPriorityList.Count != 0 && Module.SoakPriorityList != null)
                                {
                                    Module.InnerStateTransition(new EventSoakingRunningState(Module));
                                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                                }
                            }
                        }

                        if (Module.SequenceEngineManager().GetRunState() == false)
                        {
                            Module.ChuckInRangeTimeFunc();
                        }
                    }
                    //프리런에서 돌아야 할때 
                    else if (lotOpState == ModuleStateEnum.IDLE && Module.SysState().GetSysState() == EnumSysState.IDLE)
                    {
                        Module.CheckAutoSoakingTrigger();

                        if (Module.SoakingDeviceFile_IParam == null)
                        {
                            ret = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Error("Soaking Param Error");
                            return ret;
                        }

                        var devparam = Module.SoakingDeviceFile_IParam as SoakingDeviceFile;

                        if (Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.SOAKING &&
                            Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.Z_UP &&
                            Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.CARDCHANGE &&
                            devparam.AutoSoakingParam.Enable.Value == true &&
                            Module.StageSupervisor().StageMode == GPCellModeEnum.ONLINE &&
                            Module.StageSupervisor().GetStageLockMode() == StageLockMode.UNLOCK &&
                            Module.AutoSoakTriggered == true &&
                            Module.SequenceEngineManager().GetRunState() &&
                            Module.SysState().GetSysState() == EnumSysState.IDLE)
                        {
                            ret = Module.CheckCardModuleAndThreeLeg();

                            if (ret == EventCodeEnum.NONE)
                            {
                                LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.START,
                                         $"Type: AutoSoak, Soaking OD: {devparam.AutoSoakingParam.ZClearance.Value} ",
                                         this.Module.LoaderController().GetChuckIndex());

                                Module.InnerStateTransition(new AutoSoakingRunningState(Module));
                            }
                        }
                        else
                        {
                            ret = EventCodeEnum.NONE;
                        }
                        #region old
                        //if ((Module.StageSupervisor().StageModuleState.GetState() == StageStateEnum.Z_CLEARED ||
                        //    Module.StageSupervisor().StageModuleState.GetState() == StageStateEnum.MOVETOLOADPOS ||
                        //    Module.StageSupervisor().StageModuleState.GetState() != StageStateEnum.SOAKING) && Module.SoakingDeviceFile_Clone.AutoSoakingType.Value != EnumAutoSoakingType.DISABLE)
                        //{
                        //}
                        //if(IsSoakingCondition())
                        //{
                        //    if (IsEnableEventSokaing(EnumSoakingType.TEMPDIFF_SOAK))
                        //    {
                        //        var DiffTemp = Module.TempController().TempInfo.SetTemp.Value - Module.TempController().TempInfo.CurTemp.Value;

                        //        if (DiffTemp > Module.SoakingDeviceFile_Clone.TemperatureDiffDegree.Value)
                        //        {
                        //            Module.InnerStateTransition(new TempDiffSoakingRunningState(Module));

                        //            ret = EventCodeEnum.NONE;
                        //            return ret;
                        //        }
                        //    }

                        //    if (Module.SoakingDeviceFile_Clone.AutoSoakingType.Value == EnumAutoSoakingType.USE_SOAK_CLEARANCE ||
                        //        Module.SoakingDeviceFile_Clone.AutoSoakingType.Value == EnumAutoSoakingType.TO_SAFE_HEIGHT)
                        //    {
                        //        // Calc. Chuck Away TimeSpan

                        //        // AutoSoaking의 경우 3가지 경우의 Z Clearance동작을 수행해야 한다.
                        //        // 1. 척 위에 Wafer가 존재하지 않을 때
                        //        // 2. Wafer Align 전
                        //        // 3. Wafer Align 후

                        //        bool IsCanAutoSoaking = true;

                        //        double TargetPositionX;
                        //        double ComaparePositionY;

                        //        double ZClearanceForAutoSokaing = 0;

                        //        if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.NOT_EXIST)
                        //        {
                        //            ZClearanceForAutoSokaing = Module.SoakingDeviceFile_Clone.AutoSokaingZClearanceNotExistWaferOnChuck.Value;
                        //        }
                        //        else
                        //        {
                        //            if (Module.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST)
                        //            {
                        //                if (Module.GetParam_Wafer().GetAlignState() == AlignStateEnum.DONE)
                        //                {
                        //                    ZClearanceForAutoSokaing = Module.SoakingDeviceFile_Clone.AutoSokaingZClearanceAfterWaferAlign.Value;
                        //                }
                        //                else
                        //                {
                        //                    ZClearanceForAutoSokaing = Module.SoakingDeviceFile_Clone.AutoSokaingZClearanceBeforeWaferAlign.Value;
                        //                }
                        //            }
                        //            else
                        //            {
                        //                LoggerManager.Error("Unknown Status.");
                        //                IsCanAutoSoaking = false;
                        //            }
                        //        }

                        //        if (IsCanAutoSoaking)
                        //        {
                        //            WaferCoordinate wafercoord = new WaferCoordinate();
                        //            PinCoordinate pincoord = new PinCoordinate();
                        //            MachineCoordinate targetpos;

                        //            ret = GetTargetPosAccordingToCondition(ref wafercoord, ref pincoord);
                        //            targetpos = GetTargetMachinePosition(wafercoord, pincoord, ZClearanceForAutoSokaing);

                        //            // Compare tolerance

                        //            double posX = 0;
                        //            double posY = 0;
                        //            double posZ = 0;
                        //            //double posT = 0;
                        //            Module.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
                        //            Module.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
                        //            Module.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                        //            //Module.MotionManager().GetActualPos(EnumAxisConstants.C, ref posT);

                        //            var ChuckAwayTolX = Module.SoakingDeviceFile_Clone.ChuckAwayToleranceX.Value;
                        //            var ChuckAwayTolY = Module.SoakingDeviceFile_Clone.ChuckAwayToleranceY.Value;
                        //            var ChuckAwayTolZ = Module.SoakingDeviceFile_Clone.ChuckAwayToleranceZ.Value;

                        //            var DiffX = posX - targetpos.X.Value;
                        //            var DiffY = posY - targetpos.Y.Value;
                        //            var DiffZ = posZ - targetpos.Z.Value;

                        //            if ((Math.Abs(DiffX) > ChuckAwayTolX) ||
                        //                (Math.Abs(DiffY) > ChuckAwayTolY) ||
                        //                (Math.Abs(DiffZ) > ChuckAwayTolZ))
                        //            {
                        //                if (Module.SoakingDeviceFile_Clone.LastChuckAwayTime == null)
                        //                {
                        //                    Module.SoakingDeviceFile_Clone.LastChuckAwayTime = DateTime.Now;
                        //                }
                        //            }

                        //            bool isDurationExceeded = false;

                        //            if (Module.SoakingDeviceFile_Clone.LastChuckAwayTime != null)
                        //            {
                        //                DateTime LastChuckAwayTime = (DateTime)Module.SoakingDeviceFile_Clone.LastChuckAwayTime;

                        //                long elapsedTicks = DateTime.Now.Ticks - LastChuckAwayTime.Ticks;
                        //                TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

                        //                // Unit : Sec.
                        //                if (elapsedSpan.TotalSeconds >= Module.SoakingDeviceFile_Clone.ChuckAwayElapsedTime.Value)
                        //                {
                        //                    isDurationExceeded = true;
                        //                    Module.SoakingDeviceFile_Clone.LastChuckAwayTime = null;
                        //                }
                        //            }

                        //            // 모든 모듈이 RUNNING, PENDING, ERROR 가 아니고
                        //            // StageState는 Sokaing이 아니고,
                        //            // 
                        //            // 일정 시간(Parameter) 동안 척으로부터 떨어져 있었을 때
                        //            // Chuck으로부터 떨어져 있던 시간이 일정 시간(파라미터)이 지났다면

                        //            //if ( (Module.SequenceEngineManager().GetRunState()) && (stagestate != StageStateEnum.SOAKING) && (Module.IsLotEndEventOn == true) &&
                        //            //    (isDurationExceeded) )
                        //            if ((Module.SequenceEngineManager().GetRunState()) && (stagestate != StageStateEnum.SOAKING) && (isDurationExceeded))
                        //            {
                        //                if (Module.SoakingDeviceFile_Clone.ColdTempBasicPoint.Value < Module.TempController().TempInfo.CurTemp.Value &&
                        //                    Module.SoakingDeviceFile_Clone.HotTempBasicPoint.Value > Module.TempController().TempInfo.CurTemp.Value)
                        //                {
                        //                    //Module.IsLotEndEventOn = false;
                        //                    Module.InnerStateTransition(new AutoSoakingRunningState(Module));

                        //                    ret = EventCodeEnum.NONE;
                        //                }
                        //                else
                        //                {
                        //                    ret = EventCodeEnum.NONE;
                        //                }
                        //            }
                        //            else
                        //            {
                        //                ret = EventCodeEnum.NONE;
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        ret = EventCodeEnum.NONE;
                        //    }
                        //}
                        #endregion
                    }
                    else
                    {
                        Func<bool> conditionFunc = () =>
                        {
                            bool isCanExecute;

                            isCanExecute = Module.SequenceEngineManager().GetRunState();

                            return isCanExecute;
                        };

                        Action doAction = () =>
                        {
                            if (waferalignstate == AlignStateEnum.DONE &&
                               pinalignstate == AlignStateEnum.DONE)
                            {
                                //     Module.InnerStateTransition(new (Module));
                            }
                            else
                            {
                                //      Module.InnerStateTransition(new AutoSoakingRunningState(Module));
                            }
                        };
                        Action abortAction = () => { };


                        bool consumed;
                        consumed = Module.CommandManager().ProcessIfRequested<IEventSoakingCommand>(
                            Module,
                            conditionFunc,
                            doAction,
                            abortAction);
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred SoakingIdleState.");
                //LoggerManager.Error($err.InnerException);

                LoggerManager.Exception(err);

                ret = EventCodeEnum.SOAKING_ERROR;
                Module.StateTransitionToErrorState(ret);
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
            }

            return ret;
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
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new SoakingPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class PreOpState : StatusSoakingStateBase
    {
        bool Force_Disable_LastUseSoaking = false;
        public PreOpState(SoakingModule module, bool ForceDisable_UseLastSoakingFlag = false) : base(module)
        {
            Force_Disable_LastUseSoaking = ForceDisable_UseLastSoakingFlag;

            if (SubState == null)
            {
                SubStateTransition(new SoakingSubIdle(this));
            }
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Machine Init 할 때 State임
                //New Soaking을 할 것인지 Old Soaking을 할 것인지
                bool show_status_Soaking_Setting_ToggleValue = false;
                if (Module.StatusSoakingDeviceFileObj != null)
                {
                    show_status_Soaking_Setting_ToggleValue = Module.StatusSoakingDeviceFileObj.Get_ShowStatusSoakingSettingPageToggleValue();

                }

                if (show_status_Soaking_Setting_ToggleValue) //New Soaking
                {
                    // Validation Status Soaking Parameter
                    CheckAvailableStatusSoakingEnable();

                    // Status Soaking Start
                    LoggerManager.SoakingLog($"<Status Soaking Start>...................");
                    IInnerState innerState = null;

                    if (Module.StatusSoakingParamIF != null)
                    {
                        bool useLastSoakingState = false;
                        Module.StatusSoakingParamIF.IsUseLastStatusSoakingState(ref useLastSoakingState);

                        bool EnableStatusSoakingFlag = false;
                        Module.StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoakingFlag);
                        Module.BeforeStatusSoakingOption_About_UseFlag = EnableStatusSoakingFlag;
                        if (Force_Disable_LastUseSoaking)
                        {
                            useLastSoakingState = false;
                            LoggerManager.SoakingLog($"Force disable 'use last soaking state'");
                        }

                        // SW가 재시작 되기 이전 상태를 보는 경우
                        if (useLastSoakingState)
                        {
                            // SW가 재시작 되기 이전의 정보를 가져올 수 있으면, ...
                            var lastSoakingStateInfoObj = Module.LastSoakingStateInfoObj.LoadLastSoakingStateInfo();
                            if (null != lastSoakingStateInfoObj)
                            {
                                var lastSoakingStateInfo = lastSoakingStateInfoObj as StatusSoakingInfo;
                                if (null != lastSoakingStateInfo)
                                {
                                    innerState = GetStatusSoakingInitState(lastSoakingStateInfo);
                                }
                            }
                        }
                    }

                    if (null == innerState)
                    {
                        innerState = new PrepareSoakingState(Module);
                    }

                    retVal = Module.InnerStateTransition(innerState);

                    Module.LastSoakingStateInfoObj.TraceLastSoakingStateInfo(true);
                }
                else//Old Soaking
                {
                    LoggerManager.SoakingLog($"<Old Event Soaking Start>...................");
                    Module.LastSoakingStateInfoObj.TraceLastSoakingStateInfo(false);
                    retVal = Module.InnerStateTransition(new SoakingIdleState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum subState = ModuleStateEnum.UNDEFINED;
            try
            {
                subState = SubState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetModuleState(): Error occurred. Err = {err.Message}, SubState : {subState}");
            }
            return subState;
        }

        public override EventCodeEnum GetParameter()
        {
            return EventCodeEnum.NONE;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.PREPARE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new SoakingPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private IInnerState GetStatusSoakingInitState(StatusSoakingInfo statusSoakingInfo)
        {
            IInnerState innerState = null;
            long accChillingTime = 0;

            try
            {
                List<ChillingTimeTableInfo> ChillingTimeTableList = new List<ChillingTimeTableInfo>();
                Module.StatusSoakingParamIF.Get_ChillingTimeTableInfo(ref ChillingTimeTableList);

                accChillingTime = statusSoakingInfo.ChillingTime + (long)(DateTime.Now - statusSoakingInfo.InfoUpdateTime).TotalSeconds;
                Module.ChillingTimeMngObj.CalculateChillingTime(accChillingTime * 1000);

                switch (statusSoakingInfo.SoakingState)
                {
                    case SoakingStateEnum.PREPARE:
                        innerState = new PrepareSoakingState(Module);
                        break;
                    case SoakingStateEnum.RECOVERY:
                        // 누적된 ChillingTime이 ChillingTimeTable의 마지막 Row의 시간보다 크고,
                        // Prepare SoakingTime이 Recovery Soaking Time보다 큰 경우 PrepareState가 된다.
                        if (accChillingTime > ChillingTimeTableList[ChillingTimeTableList.Count - 1].ChillingTimeSec)
                        {
                            int prepareSoakingTime = 0;
                            Module.StatusSoakingParamIF.Get_PrepareStatusSoakingTimeSec(ref prepareSoakingTime, false);

                            if (prepareSoakingTime > ChillingTimeTableList[ChillingTimeTableList.Count - 1].SoakingTimeSec)
                            {
                                innerState = new PrepareSoakingState(Module);
                            }
                            else
                            {
                                innerState = new RecoverySoakingState(Module);
                            }
                        }
                        else
                        {
                            innerState = new RecoverySoakingState(Module);
                        }
                        break;
                    case SoakingStateEnum.MAINTAIN:
                        // 누적된 ChillingTime이 ChillingTimeTable의 마지막 Row의 시간보다 큰 경우 PrepareState가 된다.
                        if (accChillingTime > ChillingTimeTableList[ChillingTimeTableList.Count - 1].ChillingTimeSec)
                        {
                            innerState = new PrepareSoakingState(Module);
                        }
                        else
                        {
                            // 누적된 ChillingTime이 ChillingTimeTable의 첫 번째 Row의 시간보다 큰 경우 RecoveryState가 된다.
                            if (accChillingTime > ChillingTimeTableList[0].ChillingTimeSec)
                            {
                                innerState = new RecoverySoakingState(Module);
                            }
                            else
                            {
                                innerState = new MaintainSoakingState(Module);
                            }
                        }

                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return innerState;
        }

        /// <summary>
        /// Status Soaking이 Enable 가능한지 Parameter를 체크하고, 불가능할 경우 Enalbe을 False로 변경한다.
        /// </summary>
        private void CheckAvailableStatusSoakingEnable()
        {
            bool isEnableStausSoaking = false;
            Module.StatusSoakingParamIF.IsEnableStausSoaking(ref isEnableStausSoaking);

            if (!isEnableStausSoaking)
            {
                return;
            }

            var prepareCommonParam = new StatusSoakingCommonParameterInfo();
            Module.StatusSoakingParamIF.Get_StatusCommonOption(SoakingStateEnum.PREPARE, ref prepareCommonParam);

            var recoveryCommonParam = new StatusSoakingCommonParameterInfo();
            Module.StatusSoakingParamIF.Get_StatusCommonOption(SoakingStateEnum.RECOVERY, ref recoveryCommonParam);

            var ChillingTimeTableList = new List<ChillingTimeTableInfo>();
            Module.StatusSoakingParamIF.Get_ChillingTimeTableInfo(ref ChillingTimeTableList);

            var maintainCommonParam = new StatusSoakingCommonParameterInfo();
            Module.StatusSoakingParamIF.Get_StatusCommonOption(SoakingStateEnum.MAINTAIN, ref maintainCommonParam);

            if (prepareCommonParam.soakingStepList.Count <= 0 ||
                recoveryCommonParam.soakingStepList.Count <= 0 ||
                maintainCommonParam.soakingStepList.Count <= 0 ||
                ChillingTimeTableList.Count <= 0)
            {
                Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.UseStatusSoaking.Value = false;
                Module.SaveParameter();
            }
        }
    }
    public class PrepareSoakingState : StatusSoakingStateBase
    {
        public PrepareSoakingState(SoakingModule module) : base(module)
        {
            if (SubState == null)
            {
                SubStateTransition(new SoakingSubIdle(this));
                Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SOAKING, this.SubState.GetModuleState().ToString());
            }

            module?.SaveLastSoakingStateInfo(SoakingStateEnum.PREPARE);
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public bool IsSoakingCondition()
        {
            bool retval = false;

            try
            {
                switch (Module.StageSupervisor().StageMoveState)
                {
                    // WHITE LIST

                    case StageStateEnum.IDLE:
                    case StageStateEnum.Z_CLEARED:

                        retval = true;
                        break;

                    // BLACK LIST

                    case StageStateEnum.UNKNOWN:
                        break;
                    case StageStateEnum.ERROR:
                        break;
                    case StageStateEnum.MOVING:
                        break;
                    case StageStateEnum.MOVETONEXTDIE:
                        break;
                    case StageStateEnum.Z_UP:
                        break;
                    case StageStateEnum.Z_IDLE:
                        break;
                    case StageStateEnum.Z_DOWN:
                        break;
                    case StageStateEnum.MOVETOLOADPOS:
                        break;
                    case StageStateEnum.MOVETODOWNPOS:
                        break;
                    case StageStateEnum.WAFERVIEW:
                        break;
                    case StageStateEnum.WAFERHIGHVIEW:
                        break;
                    case StageStateEnum.WAFERLOWVIEW:
                        break;
                    case StageStateEnum.PINVIEW:
                        break;
                    case StageStateEnum.PINHIGHVIEW:
                        break;
                    case StageStateEnum.PINLOWVIEW:
                        break;
                    case StageStateEnum.PROBING:
                        break;
                    case StageStateEnum.SOAKING:
                        break;
                    case StageStateEnum.CARDCHANGE:
                        break;
                    case StageStateEnum.TILT:
                        break;
                    case StageStateEnum.AIRBLOW:
                        break;
                    case StageStateEnum.CHUCKTILT:
                        break;
                    case StageStateEnum.MARK:
                        break;
                    case StageStateEnum.VISIONMAPPING:
                        break;
                    case StageStateEnum.NC_CLEANING:
                        break;
                    case StageStateEnum.NC_PADVIEW:
                        break;
                    case StageStateEnum.NC_SENSING:
                        break;
                    case StageStateEnum.NC_SENSORVIEW:
                        break;
                    case StageStateEnum.MANUAL:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.MonitoringManager().IsMachineInitDone == true)
                {

                    retVal = SubState.Execute();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.SoakingErrLog($"Execute() in {this.GetType().Name}, SubState = {SubState}, retVal : {retVal}");
                        EventCodeEnum NoticeRetVal = retVal;
                        Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                        retVal = EventCodeEnum.SOAKING_ERROR;
                        this.SubStateTransition(new SoakingSubError(this));

                        LoggerManager.SoakingErrLog($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        LoggerManager.SoakingErrLog($"Error occurred during PrepareSoaking .");
                        Module.MetroDialogManager().ShowMessageDialog("Error Message", $"Soaking error occurred, Switch to the maintenance mode to clear error.\r\n({NoticeRetVal.ToString()})", EnumMessageStyle.Affirmative);
                        return retVal;
                    }

                    if (SubState.GetState() == SoakingStateEnum.DONE)
                    {
                        retVal = SubState.Execute(); // Done State 처리를 위해 Call
                        if (EventCodeEnum.NONE != retVal)
                        {
                            LoggerManager.SoakingErrLog($"Execute() in {this.GetType().Name}, SubState = {SubState}, retVal : {retVal}");
                            LoggerManager.SoakingErrLog($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                            LoggerManager.SoakingErrLog($"Error occurred during PrepareSoaking.");
                            LoggerManager.SoakingErrLog($"Failed to execute 'SubDone'");

                            Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);

                            this.SubStateTransition(new SoakingSubError(this, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, Module.ReasonOfError.GetLastEventMessage().ToString(), Module.GetType().Name)));

                            return retVal;
                        }

                        //maintain stauts로 transition                        
                        retVal = Module.InnerStateTransition(new MaintainSoakingState(Module));
                        Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "SOAKING(DONE)");
                        SubState.StatusSoakingInfoUpdateToLoader(0, true, true);
                        LoggerManager.SoakingLog($"Transition : Prepare Status -> Maintain Status");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
            }
            return retVal;
        }

#if false
        public EventCodeEnum Execute2()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.MonitoringManager().IsMachineInitDone == true)
                {
                    // SubState IDLE -> Execute();
                    retVal = SubState.Execute();

                    // Curr State = Maintain
                    // Next State = Recovery
                    if (SubState.GetNextSoakingState() != SubState.GetSoakingState())
                    {
                        SoakingState.StateTransition(SubState.GetNextSoakingState());
                        Module.InnerStateTransition(new MaintainSoakingState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
            }
            return retVal;
        }
#endif

        public override ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum subState = ModuleStateEnum.UNDEFINED;
            try
            {
                subState = SubState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetModuleState(): Error occurred. Err = {err.Message}, SubState : {subState}");
            }
            return subState;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.PREPARE;
        }
        public override EventCodeEnum GetParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //TODO : Prepare에 사용되는 파라미터 가져오기
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetModuleState(): Error occurred. Err = {err.Message}, Return Value : {retVal}");
            }
            return retVal;
        }
    }
    public class RecoverySoakingState : StatusSoakingStateBase
    {
        public RecoverySoakingState(SoakingModule module) : base(module)
        {
            if (SubState == null)
            {
                SubStateTransition(new SoakingSubIdle(this));
                Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SOAKING, this.SubState.GetModuleState().ToString());
            }

            module?.SaveLastSoakingStateInfo(SoakingStateEnum.RECOVERY);
            WriteLogAboutDeterminetheSoaking = false;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.MonitoringManager().IsMachineInitDone == true)
                {

                    if (SubState.GetState() == SoakingStateEnum.IDLE || SubState.GetState() == SoakingStateEnum.DONE)
                    {
                        //Wafer없는 상태에서의 soaking으로 chilling time이 감소되는 경우, maintain으로 갈 수 있는 범주라면 maintain으로 flag 인자를 만든다.
                        bool Cannot_ChangeMaintenance_ByChillingTime = true;
                        if (Module.StatusSoakingTempInfo.NeedToCheck_SoakingStatus_ByChillingTime)
                            Cannot_ChangeMaintenance_ByChillingTime = false;

                        var state = DeterminetheSoakingstate(Cannot_ChangeMaintenance_ByChillingTime);
                        if (state == SoakingStateEnum.PREPARE)
                        {
                            retVal = Module.InnerStateTransition(new PrepareSoakingState(Module));
                            LoggerManager.SoakingLog($"Transition : Recovery Status -> Prepare Status");
                            return retVal;
                        }
                        else if (state == SoakingStateEnum.RECOVERY)
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                        else if (state == SoakingStateEnum.MAINTAIN)
                        {
                            LoggerManager.SoakingLog($"Transition : Recovery Status -> Maintain Status");
                            //recovery에서 maintain으로 변경 시 pin align이 필요하다. 따라서 pin align이 되어 있는걸 취소 시켜, full pin을 하고 진행할 수 있도록 유도한다.
                            if (false == Cannot_ChangeMaintenance_ByChillingTime)
                            {
                                LoggerManager.SoakingLog($"Current Chilling Time: {Module.ChillingTimeMngObj.GetChillingTime()}. so status will change 'maintain status'");
                                if (Module.StatusSoakingTempInfo.NeedToPinAlign_For_WithoutWaferSoaking)
                                {
                                    Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                                    LoggerManager.SoakingLog("Pin align state change to IDLE(soaking status change from 'recovery' to 'maintain')");
                                }
                            }

                            retVal = Module.InnerStateTransition(new MaintainSoakingState(Module));
                            Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "SOAKING(DONE)");
                            SubState.StatusSoakingInfoUpdateToLoader(0, true, true);
                            return retVal;
                        }
                        else
                        {
                            LoggerManager.SoakingErrLog($"Unknown state: {state.ToString()}");
                        }
                    }

                    retVal = SubState.Execute();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.SoakingErrLog($"Execute() in {this.GetType().Name}, SubState = {SubState}, retVal : {retVal}");
                        Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                        EventCodeEnum NoticeRetVal = retVal;
                        retVal = EventCodeEnum.SOAKING_ERROR;
                        this.SubStateTransition(new SoakingSubError(this));
                        LoggerManager.SoakingErrLog($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        LoggerManager.SoakingErrLog($"Error occurred during RecoverySoaking .");
                        Module.MetroDialogManager().ShowMessageDialog("Error Message", $"Soaking error occurred, Switch to the maintenance mode to clear error.\r\n({NoticeRetVal.ToString()})", EnumMessageStyle.Affirmative);
                        return retVal;
                    }

                    if (SubState.GetState() == SoakingStateEnum.DONE)
                    {
                        retVal = SubState.Execute(); // Done State 처리를 위해 Call
                        if (EventCodeEnum.NONE != retVal)
                        {
                            LoggerManager.SoakingErrLog($"Execute() in {this.GetType().Name}, SubState = {SubState}, retVal : {retVal}");
                            LoggerManager.SoakingErrLog($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                            LoggerManager.SoakingErrLog($"Error occurred during RecoverySoaking.");
                            LoggerManager.SoakingErrLog($"Failed to execute 'SubDone'");
                            
                            Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);

                            this.SubStateTransition(new SoakingSubError(this, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, Module.ReasonOfError.GetLastEventMessage().ToString(), Module.GetType().Name)));
                           
                            return retVal;
                        }

                        //maintain stauts로 transition
                        retVal = Module.InnerStateTransition(new MaintainSoakingState(Module));
                        Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "SOAKING(DONE)");
                        SubState.StatusSoakingInfoUpdateToLoader(0, true, true);
                        LoggerManager.SoakingLog($"Transition : Recovery Status -> Maintain Status");

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum subState = ModuleStateEnum.UNDEFINED;
            try
            {
                subState = SubState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetModuleState(): Error occurred. Err = {err.Message}, SubState : {subState}");
            }
            return subState;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.RECOVERY;
        }
        public override EventCodeEnum GetParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //TODO : Recovery에 사용되는 파라미터 가져오기
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetModuleState(): Error occurred. Err = {err.Message}, Return Value : {retVal}");
            }
            return retVal;
        }
    }
    public class MaintainSoakingState : StatusSoakingStateBase
    {
        private bool WriteAbout_ProbingLog = false;
        public MaintainSoakingState(SoakingModule module) : base(module)
        {
            if (SubState == null)
            {
                SubStateTransition(new SoakingSubIdle(this));
                Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SOAKING, this.SubState.GetModuleState().ToString());
            }

            WriteLogAboutDeterminetheSoaking = false;
            module?.SaveLastSoakingStateInfo(SoakingStateEnum.MAINTAIN);
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public bool IsSoakingCondition()
        {
            bool retval = false;

            try
            {
                switch (Module.StageSupervisor().StageMoveState)
                {
                    // WHITE LIST

                    case StageStateEnum.IDLE:
                    case StageStateEnum.Z_CLEARED:

                        retval = true;
                        break;

                    // BLACK LIST

                    case StageStateEnum.UNKNOWN:
                        break;
                    case StageStateEnum.ERROR:
                        break;
                    case StageStateEnum.MOVING:
                        break;
                    case StageStateEnum.MOVETONEXTDIE:
                        break;
                    case StageStateEnum.Z_UP:
                        break;
                    case StageStateEnum.Z_IDLE:
                        break;
                    case StageStateEnum.Z_DOWN:
                        break;
                    case StageStateEnum.MOVETOLOADPOS:
                        break;
                    case StageStateEnum.MOVETODOWNPOS:
                        break;
                    case StageStateEnum.WAFERVIEW:
                        break;
                    case StageStateEnum.WAFERHIGHVIEW:
                        break;
                    case StageStateEnum.WAFERLOWVIEW:
                        break;
                    case StageStateEnum.PINVIEW:
                        break;
                    case StageStateEnum.PINHIGHVIEW:
                        break;
                    case StageStateEnum.PINLOWVIEW:
                        break;
                    case StageStateEnum.PROBING:
                        break;
                    case StageStateEnum.SOAKING:
                        break;
                    case StageStateEnum.CARDCHANGE:
                        break;
                    case StageStateEnum.TILT:
                        break;
                    case StageStateEnum.AIRBLOW:
                        break;
                    case StageStateEnum.CHUCKTILT:
                        break;
                    case StageStateEnum.MARK:
                        break;
                    case StageStateEnum.VISIONMAPPING:
                        break;
                    case StageStateEnum.NC_CLEANING:
                        break;
                    case StageStateEnum.NC_PADVIEW:
                        break;
                    case StageStateEnum.NC_SENSING:
                        break;
                    case StageStateEnum.NC_SENSORVIEW:
                        break;
                    case StageStateEnum.MANUAL:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.MonitoringManager().IsMachineInitDone == true)
                {
                    if (SubState.GetState() == SoakingStateEnum.IDLE || SubState.GetState() == SoakingStateEnum.DONE)
                    {
                        //event soaking이 Trigger되어 있는지 확인한다.(Lot Running 이고 wafer가 존재할 때만 event soaking이 동작한다.)
                        if (ModuleStateEnum.RUNNING == Module.LotOPModule().ModuleState.GetState())
                        {
                            if (IsExistStatusEventSoaking())
                            {
                                if(!IsStatusEventSoakSkip())
                                {
                                    var waferExist = Module.GetParam_Wafer().GetStatus();
                                    if (waferExist == EnumSubsStatus.EXIST)
                                    {
                                        retVal = Module.InnerStateTransition(new StatusEventSoakingState(Module));
                                        LoggerManager.SoakingLog($"Exist event soaking - Do change StatusEventSoakingState State");
                                        return retVal;
                                    }
                                }
                            }
                        }

                        var state = DeterminetheSoakingstate();
                        if (state == SoakingStateEnum.PREPARE)
                        {
                            retVal = Module.InnerStateTransition(new PrepareSoakingState(Module));
                            LoggerManager.SoakingLog($"Transition : Maintain Status -> Prepare Status");
                        }
                        else if (state == SoakingStateEnum.RECOVERY)
                        {
                            //Maintain에서 Recovery 변경 시 Probing이 runngind 상태라면 State를 전환하지 않도록 한다. Probing으로 인한 Chilling Time이 감소 될 수 있고, Probing state에 running까지 왔다면 사전조건을 모두 만족하고 들어온 Cas
                            if (Module.ProbingModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                            {
                                retVal = Module.InnerStateTransition(new RecoverySoakingState(Module));
                                LoggerManager.SoakingLog($"Transition : Maintain Status -> Recovery Status");
                            }
                            else
                            {
                                if (false == WriteAbout_ProbingLog)
                                {
                                    WriteAbout_ProbingLog = true;
                                    LoggerManager.SoakingErrLog($"Can't change 'Recovery Soaking State'. because probing module is not idle({Module.ProbingModule().ModuleState.GetState().ToString()})");
                                }
                            }
                        }
                        else if (state == SoakingStateEnum.MAINTAIN)
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            LoggerManager.SoakingErrLog($"DeterminetheSoakingstate return value is worng({state.ToString()})");
                            //unknown error
                        }
                    }
                    retVal = SubState.Execute();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.SoakingErrLog($"Execute() in {this.GetType().Name}, SubState = {SubState}, retVal : {retVal}");
                        Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                        EventCodeEnum NoticeRetVal = retVal;
                        retVal = EventCodeEnum.SOAKING_ERROR;
                        this.SubStateTransition(new SoakingSubError(this));
                        LoggerManager.SoakingErrLog($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        LoggerManager.SoakingErrLog($"Error occurred during Maintain soaking .");
                        Module.MetroDialogManager().ShowMessageDialog("Error Message", $"Soaking error occurred, Switch to the maintenance mode to clear error.\r\n({NoticeRetVal.ToString()})", EnumMessageStyle.Affirmative);
                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum subState = ModuleStateEnum.UNDEFINED;
            try
            {
                subState = SubState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetModuleState(): Error occurred. Err = {err.Message}, SubState : {subState}");
            }
            return subState;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.MAINTAIN;
        }

        public override EventCodeEnum GetParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //TODO : Maintain에 사용되는 파라미터 가져오기
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetModuleState(): Error occurred. Err = {err.Message}, Return Value : {retVal}");
            }
            return retVal;
        }

        /// <summary>
        /// 현재 제공되는 Event soaking중에서 trigger가 되어 있는지 확인한다.
        /// </summary>
        /// <returns></returns>
        public bool IsExistStatusEventSoaking()
        {
            if (Module.TriggeredStatusEventSoakList.Count() > 0)
                return true;

            return false;
        }

        public bool IsStatusEventSoakSkip()
        {
            bool retval = false;

            try
            {
                var HighestPriority = Module.TriggeredStatusEventSoakList.Min(x => x.Value);
                var TargetEventType = Module.TriggeredStatusEventSoakList.FirstOrDefault(x => x.Value == HighestPriority); //우선 순위 가장 높은 event 가져옴.

                var EveryWaferEventParam = Module.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.StatusSoakingEvents.FirstOrDefault(x => x.SoakingTypeEnum.Value == TargetEventType.Key);

                // Skip 파라미터가 켜져 있고
                if (EveryWaferEventParam != null && EveryWaferEventParam.UseEventSoakingSkip.Value)
                {
                    // Wafer가 Load된 후, Soaking이 동작되었다면 스킵하자.
                    if (Module.IsSoakingDoneAfterWaferLoad)
                    {
                        //처리 완료된 event soaking 제거
                        Module.TriggeredStatusEventSoakList.Remove(TargetEventType.Key); 
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    /// <summary>
    /// Manual soaking 동작 처리 State
    /// </summary>
    public class ManualSoakingState : StatusSoakingStateBase
    {
        public ManualSoakingState(SoakingModule module) : base(module)
        {
            Module.ManualSoakingStart = true;
            Module.ManualSoakingRetVal = EventCodeEnum.NONE;
            if (SubState == null)
            {
                SubStateTransition(new SoakingSubIdle(this));
                Module.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.SOAKING, this.SubState.GetModuleState().ToString());
            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.MonitoringManager().IsMachineInitDone == true)
                {
                    var BeforeState = Module.PreInnerState as ISoakingState;
                    if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && Module.ManualSoakingStart)
                    {
                        retVal = SubState.Execute();
                        if (retVal != EventCodeEnum.NONE)
                        {
                            var SubStateBase = SubState as SoakingSubStateBase;
                            if (null != SubStateBase)
                            {
                                SubStateBase.StatusIdleSoakingStart_InfoUpdate(0, "No Soak");
                                int index = Module.LoaderController().GetChuckIndex();
                                Module.LoaderController().SetTitleMessage(index, "SOAKING(ERROR)");
                            }

                            LoggerManager.SoakingErrLog($"Execute() in {this.GetType().Name}, SubState = {SubState}, retVal : {retVal}");
                            Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                            Module.ManualSoakingRetVal = retVal;
                            LoggerManager.SoakingErrLog($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                            LoggerManager.SoakingErrLog($"Error occurred during Manual soaking .");
                            Module.ManualSoakingStart = false;
                            return retVal;
                        }

                        if (SubState.GetState() == SoakingStateEnum.DONE)
                        {
                            SubState.StatusSoakingInfoUpdateToLoader(0, true, true);
                            //maintain state transition
                            Module.ManualSoakingStart = false;
                            if (BeforeState?.GetState() == SoakingStateEnum.PREPARE)  // prepare에서 Manual soaking을 진행 시, Soaking 시간이 Prepare soaking Time이상으로 soaking한 경우 maintain으로 변경
                            {
                                int PrepareSoakingTime = 0;
                                if (Module.StatusSoakingParamIF.Get_PrepareStatusSoakingTimeSec(ref PrepareSoakingTime, Module.UseTempDiff_PrepareSoakingTime))
                                {
                                    if (Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime_Org < (PrepareSoakingTime * 1000) || Module.StatusSoakingTempInfo.SoakingAbort) //manual로 soaking을 수행한 시간이 사용자가 설정한 Prepare soaking time보다 작을때는 기존 prepare로 유지
                                    {
                                        LoggerManager.SoakingLog($"Manual soaking finish(Transit Prepare) - ElapsedSoakingTime:({Module.StatusSoakingTempInfo.StatusSoaking_ElapasedTime_Org }), Setted PrepareSoakingTime({PrepareSoakingTime * 1000}), soakingCancel({Module.StatusSoakingTempInfo.SoakingAbort.ToString()})");
                                        retVal = Module.InnerStateTransition(new PrepareSoakingState(Module));
                                        LoggerManager.SoakingLog($"Transition : Manual Status -> Prepare Status");
                                    }
                                }
                            }
                            else if (BeforeState?.GetState() == SoakingStateEnum.RECOVERY)
                            {
                                if (Module.ChillingTimeMngObj.GetChillingTime() > 0 || Module.StatusSoakingTempInfo.SoakingAbort) //manual soaking완료 후 chillingTime이 남아 있으면 Recovery 또는 중간에 취소도 동일
                                {
                                    LoggerManager.SoakingLog($"Manual soaking finish(Transit Recovery) - RemainingChilling time{Module.ChillingTimeMngObj.GetChillingTime()}), soakingCancel({Module.StatusSoakingTempInfo.SoakingAbort.ToString()})");
                                    retVal = Module.InnerStateTransition(new RecoverySoakingState(Module));
                                    LoggerManager.SoakingLog($"Transition : Manual Status -> Recovery Status");
                                }
                            }
                            else
                            {
                                LoggerManager.SoakingLog($"Manual soaking finish(Transit Maintain)");
                                retVal = Module.InnerStateTransition(new MaintainSoakingState(Module));
                                LoggerManager.SoakingLog($"Transition : Manaul Status -> Maintain Status");
                            }

                            Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "MANUAL SOAKING(END)");
                        }
                    }
                    else if (false == Module.ManualSoakingStart) //manual이 아닌 이전 Status로 이동
                    {
                        if (BeforeState?.GetState() == SoakingStateEnum.PREPARE)
                        {
                            LoggerManager.SoakingLog($"Manual soaking End(Transit PrepareSoakingState)");
                            retVal = Module.InnerStateTransition(new PrepareSoakingState(Module));
                            LoggerManager.SoakingLog($"Transition : Manual Status -> Prepare Status");
                        }
                        else if (BeforeState?.GetState() == SoakingStateEnum.RECOVERY)
                        {
                            LoggerManager.SoakingLog($"Manual soaking End(Transit RecoverySoakingState)");
                            retVal = Module.InnerStateTransition(new RecoverySoakingState(Module));
                            LoggerManager.SoakingLog($"Transition : Manual Status -> Recovery Status");
                        }
                        else
                        {
                            LoggerManager.SoakingLog($"Manual soaking End(Transit MaintainSoakingState)");
                            retVal = Module.InnerStateTransition(new MaintainSoakingState(Module));
                            LoggerManager.SoakingLog($"Transition : Manual Status -> Maintain Status");
                        }

                        Module.Idle_SoakingFailed_PinAlign = false;
                        Module.Idle_SoakingFailed_WaferAlign = false;
                    }
                    else //maintenance 아닌데 manual soaking일때
                    {
                        LoggerManager.SoakingErrLog($"Mode is wrong - StageMode:{Module.StageSupervisor().StageMode.ToString()}, ManualSoakingStart:{Module.ManualSoakingStart.ToString()} ");
                        Module.ManualSoakingStart = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum subState = ModuleStateEnum.UNDEFINED;
            try
            {
                subState = SubState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetModuleState(): Error occurred. Err = {err.Message}, SubState : {subState}");
            }
            return subState;
        }

        public override EventCodeEnum GetParameter()
        {
            return EventCodeEnum.NONE;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.MANUAL;
        }
    }

    /// <summary>
    /// Status soaking에서의 event soaking
    /// </summary>
    public class StatusEventSoakingState : StatusSoakingStateBase
    {
        public StatusEventSoakingState(SoakingModule module) : base(module)
        {
            if (SubState == null)
            {
                SubStateTransition(new SoakingSubIdle(this));
            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = token is IEventSoakingCommand;
            return isValidCommand;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.MonitoringManager().IsMachineInitDone == true)
                {
                    retVal = SubState.Execute();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.SoakingErrLog($"Execute() in {this.GetType().Name}, SubState = {SubState}, retVal : {retVal}");
                        Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                        EventCodeEnum NoticeRetVal = retVal;
                        retVal = EventCodeEnum.SOAKING_ERROR;
                        this.SubStateTransition(new SoakingSubError(this));
                        LoggerManager.SoakingErrLog($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        LoggerManager.SoakingErrLog($"Error occurred during statusEvent soaking .");
                        Module.MetroDialogManager().ShowMessageDialog("Error Message", $"Soaking error occurred, Switch to the maintenance mode to clear error.\r\n({NoticeRetVal.ToString()})", EnumMessageStyle.Affirmative);
                        return retVal;
                    }

                    if (SubState.GetState() == SoakingStateEnum.DONE)
                    {
                        retVal = SubState.Execute(); // Done State 처리를 위해 Call
                        if (EventCodeEnum.NONE != retVal)
                        {
                            LoggerManager.SoakingErrLog($"Failed to execute 'SubDone'");
                            return retVal;
                        }

                        //event soaking이 완료되면 DeterminetheSoakingstate에서 결정된 Status로 처리
                        Module.LoaderController().SetTitleMessage(Module.LoaderController().GetChuckIndex(), "SOAKING(DONE)");
                        var state = DeterminetheSoakingstate(false);
                        if (state == SoakingStateEnum.PREPARE)
                        {
                            retVal = Module.InnerStateTransition(new PrepareSoakingState(Module));
                            LoggerManager.SoakingLog($"Transition : StatusEvent Status -> Prepare Status");
                        }
                        else if (state == SoakingStateEnum.RECOVERY)
                        {
                            retVal = Module.InnerStateTransition(new RecoverySoakingState(Module));
                            LoggerManager.SoakingLog($"Transition : StatusEvent Status -> Recovery Status");
                        }
                        else if (state == SoakingStateEnum.MAINTAIN)
                        {
                            retVal = Module.InnerStateTransition(new MaintainSoakingState(Module));
                            LoggerManager.SoakingLog($"Transition : StatusEvent Status -> Maintain Status");
                        }
                        else
                        {
                            LoggerManager.SoakingErrLog($"Unknown state: {state.ToString()}");
                        }

                        LoggerManager.SoakingLog($"Status Event soaking Done(Transit Maintain)");
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Execute in {this.GetType().Name}: Error occurred. Err = {err.Message}");
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum subState = ModuleStateEnum.UNDEFINED;
            try
            {
                subState = SubState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetModuleState(): Error occurred. Err = {err.Message}, SubState : {subState}");
            }
            return subState;
        }

        public override EventCodeEnum GetParameter()
        {
            return EventCodeEnum.NONE;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.STATUS_EVENT_SOAK;
        }
    }

    public class EventSoakingRunningState : SoakingStateBase
    {
        bool IsPinAlignSkip = false;

        public EventSoakingRunningState(SoakingModule module, bool isPinAlignSkip = false) : base(module)
        {
            _isPinAlignNeededCalled = false;
            this.IsPinAlignSkip = isPinAlignSkip;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    RaisingPreHeatStartEvent(0);
                    RaisingPreHeatEndEvent();

                    Module.InnerStateTransition(new EventSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }

                var curSoakType = Module.SoakPriorityList.FirstOrDefault();

                if (curSoakType == null)
                {
                    Module.InnerStateTransition(new EventSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");

                    return EventCodeEnum.NONE;
                }
                else
                {
                    if(IsPinAlignSkip == false)
                    {
                        if (IsPinAlignNeeded())
                        {
                            bool injected = Module.CommandManager().SetCommand<IDOPINALIGN>(Module);

                            if (injected)
                            {
                                Module.InnerStateTransition(new AwaitingPinAlignCompletionState(Module, true));
                                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");

                                return EventCodeEnum.NONE;
                            }
                        }
                    }

                    RaisingPreHeatStartEvent(curSoakType.SoakingTimeInSeconds.Value);

                    LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.START,
                        $"Type: {curSoakType.EventSoakingType.Value}, Soaking OD: {curSoakType.ZClearance.Value}, " +
                        $"Time(sec): {curSoakType.SoakingTimeInSeconds.Value}",
                        this.Module.LoaderController().GetChuckIndex());

                    Module.SoakingTitle = $"Pre Heat #Cell{Module.LoaderController().GetChuckIndex().ToString()}";
                    Module.SoakingMessage = $"{curSoakType.EventSoakingType.Value.ToString()}";

                    ret = SoakingFunc(curSoakType.ZClearance.Value, 0, curSoakType.EventSoakingType.Value);

                    if (ret == EventCodeEnum.NONE)
                    {
                        Module.InnerStateTransition(new WaitSoakingRunningState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    }
                    else if (ret == EventCodeEnum.PIN_ALIGN_FAILED)
                    {
                        //TODO 에러 띄워야함 방식은 정해야함
                        Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                        Module.StateTransitionToErrorState(ret, new EventCodeInfo(Module.ReasonOfError.ModuleType, ret, Module.ReasonOfError.GetLastEventMessage().ToString(), Module.GetType().Name));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        LoggerManager.Error($"Error occurred during EventSoakRunning .");
                    }
                    else
                    {
                        //TODO 에러 띄워야함 방식은 정해야함
                        Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                        ret = EventCodeEnum.SOAKING_ERROR;
                        Module.StateTransitionToErrorState(ret);
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        LoggerManager.Error($"Error occurred during EventSoakRunning .");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);

                ret = EventCodeEnum.SOAKING_ERROR;
                Module.StateTransitionToErrorState(ret);

                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
            }

            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.EVENTSOAKING_RUNNING;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class ChuckAwaySoakingRunningState : SoakingStateBase
    {
        public ChuckAwaySoakingRunningState(SoakingModule module) : base(module)
        {
            _isPinAlignNeededCalled = false;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new EventSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");

                    return EventCodeEnum.NONE;
                }

                var curSoakType = Module.GetEventSoakingObject(EnumSoakingType.CHUCKAWAY_SOAK);

                if (curSoakType == null)
                {
                    Module.InnerStateTransition(new EventSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }
                else
                {
                    if (IsPinAlignNeeded())
                    {
                        bool injected = Module.CommandManager().SetCommand<IDOPINALIGN>(Module);

                        if (injected)
                        {
                            Module.InnerStateTransition(new AwaitingPinAlignCompletionState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");

                            return EventCodeEnum.NONE;
                        }
                    }

                    RaisingPreHeatStartEvent(curSoakType.SoakingTimeInSeconds.Value);
                    Module.SoakingTitle = $"Pre Heat #Cell{Module.LoaderController().GetChuckIndex().ToString()}";
                    Module.SoakingMessage = $"{curSoakType.EventSoakingType.Value.ToString()}";

                    ret = SoakingFunc(curSoakType.ZClearance.Value, 0, EnumSoakingType.CHUCKAWAY_SOAK);

                    if (ret == EventCodeEnum.NONE)
                    {
                        Module.InnerStateTransition(new WaitChuckAwaySoakingRunningState(Module));

                        if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                        {
                            int index = Module.LoaderController().GetChuckIndex();
                            Module.LoaderController().SetTitleMessage(index, Module.GetModuleMessage());
                        }

                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    }
                    else if (ret == EventCodeEnum.PIN_ALIGN_FAILED)
                    {
                        //TODO 에러 띄워야함 방식은 정해야함
                        Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                        Module.StateTransitionToErrorState(ret, new EventCodeInfo(Module.ReasonOfError.ModuleType, ret, Module.ReasonOfError.GetLastEventMessage().ToString(), Module.GetType().Name));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        LoggerManager.Error($"Error occurred during EventSoakRunning .");
                    }
                    else
                    {
                        //TODO 에러 띄워야함 방식은 정해야함
                        Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                        Module.StateTransitionToErrorState(ret, new EventCodeInfo(Module.ReasonOfError.ModuleType, ret, Module.ReasonOfError.GetLastEventMessage().ToString(), Module.GetType().Name));

                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        LoggerManager.Error($"Error occurred during EventSoakRunning .");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                ret = EventCodeEnum.SOAKING_ERROR;
                Module.StateTransitionToErrorState(ret);
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
            }

            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.EVENTSOAKING_RUNNING;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class WaitSoakingRunningState : SoakingStateBase
    {
        DateTime startDate;
        int SoakingTime = 0;
        int prevtime = -1;
        public WaitSoakingRunningState(SoakingModule module) : base(module)
        {
            startDate = DateTime.Now;
            var curSoakType = Module.SoakPriorityList.FirstOrDefault();
            SoakingTime = curSoakType.SoakingTimeInSeconds.Value - Module.AccumulatedTime;

            if (curSoakType.EventSoakingType.Value != EnumSoakingType.LOTRESUME_SOAK)
            {
                if (SoakingTime < 0 & Module.AccumulatedTime != 0)
                {
                    SoakingTime = 0;
                }
                else
                {
                    SoakingTime = Math.Abs(SoakingTime);
                    Module.AccumulatedTime += SoakingTime;
                    Module.AccAtResumeTime += SoakingTime;
                }
            }
            else
            {
                // LOTRESUME_SOAK이 꼭 돌아야 하는 경우
                //AccAtResumeTimeresume : 멈추고 난 뒤 soak 이 돌았던 시간 soak
                var timeSpendForAnotherSoak = Module.AccAtResumeTime - curSoakType.SoakingTimeInSeconds.Value;
                if (timeSpendForAnotherSoak >= 0)
                {
                    // 이 경우는 있으면 안될것 같긴함... 
                    SoakingTime = 0;
                    LoggerManager.Debug($"[SoakingModule] [LOTRESUME_SOAK] : Soaking Skip( {curSoakType.ResumeSoakingSkipTime.Value}sec )," +
                        $"Total Re - Soaking Time:{Module.AccAtResumeTime}");
                }
                else
                {
                    //resume soak 이 돌아야 하는 시점인 부분
                    SoakingTime = Math.Abs(timeSpendForAnotherSoak);
                    Module.AccumulatedTime += SoakingTime;
                    Module.AccAtResumeTime += SoakingTime;
                    LoggerManager.Debug($"[SoakingModule] soaking SoakType: LOTRESUME_SOAK  Soaking Time : {SoakingTime}sec Time Spend For Another Soak : {Module.AccAtResumeTime}");
                }
            }
            LoggerManager.Debug($"[SoakingModule] Start wait soaking SoakType:{curSoakType.EventSoakingType.Value}  Soaking Time : {SoakingTime}sec " +
                $"Total Soaking Time:{Module.AccumulatedTime}");
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            DateTime EndDate = DateTime.Now;
            TimeSpan datediff = EndDate - startDate;

            try
            {
                ModuleStateEnum lotOpState = Module.LotOPModule().ModuleState.GetState();
                if (lotOpState == ModuleStateEnum.PAUSING)
                {
                    //Module.SoakingDialogService().CloseDialog(Module.LoaderController().GetChuckIndex());
                    var currentsoak = Module.SoakPriorityList.FirstOrDefault();
                    Module.AccumulatedTime = Convert.ToInt32(datediff.TotalSeconds);
                    Module.InnerStateTransition(new SoakingPauseState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                }

                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    RaisingPreHeatEndEvent();
                    //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(PreHeatEndEvent).FullName));
                    Module.InnerStateTransition(new EventSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }

                var curSoakType = Module.SoakPriorityList.FirstOrDefault();

                if (curSoakType == null)
                {
                    RaisingPreHeatEndEvent();
                    //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(PreHeatEndEvent).FullName));
                    Module.InnerStateTransition(new EventSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }
                else
                {
                    if (Module.StageSupervisor() != null)
                    {
                        if (Module.LoaderController() != null)
                        {
                            if (prevtime != SoakingTime - Convert.ToInt32(datediff.TotalSeconds))
                            {
                                SoakingInfo soakinfo = new SoakingInfo();
                                soakinfo.SoakingType = ConvertStringSoakType(curSoakType.EventSoakingType.Value);
                                soakinfo.RemainTime = SoakingTime - Convert.ToInt32(datediff.TotalSeconds);
                                soakinfo.ZClearance = curSoakType.ZClearance.Value;
                                soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                                //Module.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateSoakingInfo(soakinfo);
                                Module.LoaderController()?.UpdateSoakingInfo(soakinfo);
                            }
                        }
                    }
                    if (SoakingTime <= datediff.TotalSeconds)
                    {
                        ret = EventCodeEnum.NONE;
                    }

                    if (Module.SoakingCancelFlag == true)
                    {
                        Module.AccumulatedTime -= SoakingTime;
                        Module.AccumulatedTime += Convert.ToInt32(datediff.TotalSeconds);
                        Module.AccAtResumeTime -= SoakingTime;
                        Module.AccAtResumeTime += Convert.ToInt32(datediff.TotalSeconds);
                        Module.SoakingCancelFlag = false;
                        ret = EventCodeEnum.SOAKING_CANCLE;
                    }


                    if (ret == EventCodeEnum.NONE || ret == EventCodeEnum.SOAKING_CANCLE)
                    {
                        //Module.SoakingDialogService().CloseDialog(Module.LoaderController().GetChuckIndex());
                        RaisingPreHeatEndEvent();
                        //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(PreHeatEndEvent).FullName));
                        if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                        {
                            Module.InnerStateTransition(new EventSoakingDoneState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                            return EventCodeEnum.NONE;
                        }

                        SoakingCommandParam pintolerance = new SoakingCommandParam();

                        if (SoakingTime != 0)
                        {
                            LoggerManager.Debug($"[SoakingModule] End of wait soaking SoakType:{curSoakType.EventSoakingType.Value}  " +
                               $"Soaking Time : { Module.AccumulatedTime - SoakingTime}sec Total Soaking Time:{Module.AccumulatedTime}");

                        }

                        Module.SoakPriorityList.Dequeue();
                        //curSoakType.Triggered = false;
                        Module.OffSoakingTrigger(curSoakType.EventSoakingType.Value);
                        if (curSoakType.EventSoakingType.Value == EnumSoakingType.DEVICECHANGE_SOAK)
                        {
                            Module.DeviceChangeFlag = false;
                        }
                        //ret = Module.ProcessDialog.CloseDialg().Result;
                        if (SoakingTime != 0)
                        {
                            bool pinAlignOn = true;
                            if (Module.SoakPriorityList.Count > 0)
                            {
                                pinAlignOn = false;
                                while (true)
                                {
                                    var nextSoakType = Module.SoakPriorityList.FirstOrDefault();
                                    int nextSoakingTime = 0;
                                    var lotopstate = Module.LotOPModule().ModuleState.GetState();

                                    if (nextSoakType == null)
                                    {
                                        pinAlignOn = true;
                                        break;
                                    }

                                    nextSoakingTime = nextSoakType.SoakingTimeInSeconds.Value - Module.AccumulatedTime;
                                    if (nextSoakingTime <= 0 & Module.AccumulatedTime != 0)
                                    {
                                        LoggerManager.Debug($"[SoakingModule] Skip {nextSoakType.EventSoakingType.Value}, skip soak time: {nextSoakType.SoakingTimeInSeconds.Value}sec. " +
                                            $"Total soaking time:{Module.AccumulatedTime}sec");
                                        Module.SoakPriorityList.Dequeue();
                                        //nextSoakType.Triggered = false;
                                        Module.OffSoakingTrigger(nextSoakType.EventSoakingType.Value);
                                        if (nextSoakType.EventSoakingType.Value == EnumSoakingType.DEVICECHANGE_SOAK)
                                        {
                                            Module.DeviceChangeFlag = false;
                                        }
                                    }
                                    else
                                    {
                                        pinAlignOn = false;
                                        break;
                                    }

                                    if (Module.SoakPriorityList.Count == 0)
                                    {
                                        pinAlignOn = true;
                                        break;
                                    }
                                }
                            }
                            
                            Module.CheckPostPinAlign(ref pinAlignOn, curSoakType);

                            if (pinAlignOn == true)
                            {
                                LoggerManager.Debug($"[SoakingModule] : DO Pin Align After Soaking");
                                bool injected = Module.CommandManager().SetCommand<IDOPinAlignAfterSoaking>(Module, pintolerance);

                                if (injected)
                                {
                                    Module.InnerStateTransition(new EventSoakingSuspendedState(Module));
                                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                                }
                                else
                                {
                                    // 에러 ,로그 금지 
                                }
                            }
                            else
                            {
                                Module.InnerStateTransition(new EventSoakingRunningState(Module, true));
                                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                            }

                        }
                        else
                        {
                            Module.InnerStateTransition(new EventSoakingSuspendedState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                ret = EventCodeEnum.SOAKING_ERROR;
                Module.StateTransitionToErrorState(ret);
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                //ret = Module.ProcessDialog.CloseDialg().Result;
            }

            return ret;
        }


        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.EVENTSOAKING_RUNNING;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class WaitChuckAwaySoakingRunningState : SoakingStateBase
    {
        DateTime startDate;
        int SoakingTime = 0;
        int prevtime = -1;
        public WaitChuckAwaySoakingRunningState(SoakingModule module) : base(module)
        {
            startDate = DateTime.Now;
            var curSoakType = Module.GetEventSoakingObject(EnumSoakingType.CHUCKAWAY_SOAK);
            SoakingTime = curSoakType.SoakingTimeInSeconds.Value - Module.AccumulatedTime;
            if (SoakingTime < 0 & Module.AccumulatedTime != 0)
            {
                SoakingTime = 0;
            }
            else
            {
                SoakingTime = Math.Abs(SoakingTime);
                Module.AccumulatedTime += SoakingTime;
            }

        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            DateTime EndDate = DateTime.Now;
            TimeSpan datediff = EndDate - startDate;

            try
            {
                ModuleStateEnum lotOpState = Module.LotOPModule().ModuleState.GetState();
                if (lotOpState == ModuleStateEnum.PAUSING)
                {
                    //Module.SoakingDialogService().CloseDialog(Module.LoaderController().GetChuckIndex());

                    Module.InnerStateTransition(new SoakingPauseState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                }


                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    RaisingPreHeatEndEvent();
                    //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(PreHeatEndEvent).FullName));
                    Module.InnerStateTransition(new EventSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }

                var curSoakType = Module.GetEventSoakingObject(EnumSoakingType.CHUCKAWAY_SOAK);
                if (Module.StageSupervisor() != null)
                {
                    if (Module.LoaderController() != null)
                    {
                        if (prevtime != SoakingTime - Convert.ToInt32(datediff.TotalSeconds))
                        {
                            SoakingInfo soakinfo = new SoakingInfo();
                            soakinfo.SoakingType = ConvertStringSoakType(curSoakType.EventSoakingType.Value);
                            soakinfo.RemainTime = SoakingTime - Convert.ToInt32(datediff.TotalSeconds);
                            soakinfo.ZClearance = curSoakType.ZClearance.Value;
                            soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                            prevtime = SoakingTime - Convert.ToInt32(datediff.TotalSeconds);
                            //Module.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateSoakingInfo(soakinfo);
                            Module.LoaderController()?.UpdateSoakingInfo(soakinfo);
                        }
                    }
                }

                if (curSoakType == null)
                {
                    RaisingPreHeatEndEvent();
                    //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(PreHeatEndEvent).FullName));
                    Module.InnerStateTransition(new EventSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }
                else
                {

                    if (SoakingTime <= datediff.TotalSeconds)
                    {
                        ret = EventCodeEnum.NONE;
                    }

                    if (Module.SoakingCancelFlag == true)
                    {
                        Module.AccumulatedTime -= SoakingTime;
                        Module.AccumulatedTime += Convert.ToInt32(datediff.TotalSeconds);
                        Module.SoakingCancelFlag = false;
                        ret = EventCodeEnum.SOAKING_CANCLE;
                    }

                    if (ret == EventCodeEnum.NONE || ret == EventCodeEnum.SOAKING_CANCLE)
                    {
                        RaisingPreHeatEndEvent();
                        //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(PreHeatEndEvent).FullName));
                        if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                        {
                            Module.InnerStateTransition(new EventSoakingDoneState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                            return EventCodeEnum.NONE;
                        }

                        //curSoakType.Triggered = false;
                        Module.OffSoakingTrigger(curSoakType.EventSoakingType.Value);

                        if (curSoakType.EventSoakingType.Value == EnumSoakingType.DEVICECHANGE_SOAK)
                        {
                            Module.DeviceChangeFlag = false;
                        }

                        if (Module.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE &&
                            Module.StageSupervisor().WaferObject.AlignState.Value == AlignStateEnum.DONE &&
                            Module.StageSupervisor().MarkObject.AlignState.Value == AlignStateEnum.DONE &&
                            Module.SequenceEngineManager().GetRunState() & !Module.SoackingDone &&
                            Module.ProbingModule().ModuleState.GetState() != ModuleStateEnum.SUSPENDED &&
                            Module.LoaderController().ModuleState.GetState() != ModuleStateEnum.SUSPENDED &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROBING &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.TESTED &&
                            Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED &&
                            Module.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST)
                        {
                            if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                            {
                                return EventCodeEnum.NONE;
                            }

                            ret = Module.UpdateEventSoakingPriority();

                            if (Module.SoakPriorityList.Count != 0 && Module.SoakPriorityList != null)
                            {
                                Module.InnerStateTransition(new EventSoakingRunningState(Module));
                                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                            }
                        }
                        else
                        {
                            Module.InnerStateTransition(new EventSoakingDoneState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        }

                    }

                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($"Error occurred during EventSoakRunning .");
                //LoggerManager.Error($err.InnerException);

                LoggerManager.Exception(err);
                Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                ret = EventCodeEnum.SOAKING_ERROR;
                Module.StateTransitionToErrorState(ret);
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                //ret = Module.ProcessDialog.CloseDialg().Result;

            }


            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.EVENTSOAKING_RUNNING;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class WaitAutoSoakingRunningState : SoakingStateBase
    {
        DateTime startDate;
        int SoakingTime = 0;
        //int prevtime = -1;
        public WaitAutoSoakingRunningState(SoakingModule module) : base(module)
        {
            SoakingTime = (Module.SoakingDeviceFile_IParam as SoakingDeviceFile).AutoSoakingParam.SoakingTimeInSeconds.Value;
            startDate = DateTime.Now;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            DateTime EndDate = DateTime.Now;
            TimeSpan datediff = EndDate - startDate;


            try
            {
                ModuleStateEnum lotOpState = Module.LotOPModule().ModuleState.GetState();
                if (lotOpState == ModuleStateEnum.PAUSING)
                {
                    //Module.SoakingDialogService().CloseDialog(Module.LoaderController().GetChuckIndex());

                    Module.InnerStateTransition(new SoakingPauseState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                }


                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.InnerStateTransition(new AutoSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }


                if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                {
                    Module.InnerStateTransition(new AutoSoakingDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                    return EventCodeEnum.NONE;
                }

                //if (Module.StageSupervisor() != null)
                //{
                //    if (Module.LoaderController() != null)
                //    {
                //        if (prevtime != SoakingTime - Convert.ToInt32(datediff.TotalSeconds))
                //        {
                //            //(Module.SoakingDeviceFile_IParam as SoakingDeviceFile).AutoSoakingParam.SoakingTimeInSeconds.Value
                //            SoakingInfo soakinfo = new SoakingInfo();
                //            soakinfo.SoakingType = ConvertStringSoakType((Module.SoakingDeviceFile_IParam as SoakingDeviceFile).AutoSoakingParam.EventSoakingType.Value);
                //            soakinfo.RemainTime = SoakingTime - Convert.ToInt32(datediff.TotalSeconds);
                //            soakinfo.ZClearance = (Module.SoakingDeviceFile_IParam as SoakingDeviceFile).AutoSoakingParam.ZClearance.Value;
                //            soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                //            //Module.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateSoakingInfo(soakinfo);
                //            Module.LoaderController()?.UpdateSoakingInfo(soakinfo);
                //        }
                //    }
                //}

                if (SoakingTime <= datediff.TotalSeconds)
                {
                    ret = EventCodeEnum.NONE;
                    //(Module.SoakingDeviceFile_IParam as SoakingDeviceFile).AutoSoakingParam.Triggered = false;
                    Module.OffSoakingTrigger(EnumSoakingType.AUTO_SOAK);
                    Module.InnerStateTransition(new AutoSoakingDoneState(Module));
                }


            }
            catch (Exception err)
            {
                //LoggerManager.Error($"Error occurred during EventSoakRunning .");
                //LoggerManager.Error($err.InnerException);

                LoggerManager.Exception(err);
                ret = EventCodeEnum.SOAKING_ERROR;
                Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                Module.StateTransitionToErrorState(ret);
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                //ret = Module.ProcessDialog.CloseDialg().Result;

            }


            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.AUTOSOAKING_RUNNING;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class EventSoakingDoneState : SoakingStateBase
    {
        public EventSoakingDoneState(SoakingModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.DONE, $"device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}", this.Module.LoaderController().GetChuckIndex());
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var probingstate = Module.ProbingModule().ModuleState.GetState();
                var lotopstate = Module.LotOPModule().ModuleState.GetState();
                var waferstatus = Module.StageSupervisor().WaferObject.GetStatus();

                if (lotopstate == ModuleStateEnum.PAUSED || waferstatus == EnumSubsStatus.NOT_EXIST)
                // if (lotopstate == ModuleStateEnum.PAUSED)
                {
                    if (Module.LoaderController() != null)
                    {
                        SoakingInfo soakinfo = new SoakingInfo();
                        soakinfo.SoakingType = "No Soak";
                        soakinfo.RemainTime = 0;
                        soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                        soakinfo.ZClearance = 0;
                        Module.LoaderController().UpdateSoakingInfo(soakinfo);
                        //Module.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateSoakingInfo(soakinfo);
                    }
                    Module.IsPreHeatEvent = false;
                    Module.InnerStateTransition(new SoakingIdleState(Module));
                    Module.AccumulatedTime = 0;
                    Module.AccAtResumeTime = 0;

                    //LoggerManager.Debug($"{0}.StateTransition() : STATE={1} ", GetType().Name, Module.SoakingState.GetState());
                }
                else
                {
                    if (Module.LoaderController() != null)
                    {
                        SoakingInfo soakinfo = new SoakingInfo();
                        soakinfo.SoakingType = "No Soak";
                        soakinfo.RemainTime = 0;
                        soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                        soakinfo.ZClearance = 0;
                        Module.LoaderController().UpdateSoakingInfo(soakinfo);
                        //Module.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateSoakingInfo(soakinfo);
                    }
                    Module.AccumulatedTime = 0;
                    Module.AccAtResumeTime = 0;
                    Module.OffAllSoakingTrigger();
                    Module.InnerStateTransition(new SoakingIdleState(Module));
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred SoakingDoneState.");
                //LoggerManager.Error($err.InnerException);

                LoggerManager.Exception(err);

                Module.StateTransitionToErrorState(ret);
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
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

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new SoakingPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class AutoSoakingRunningState : SoakingStateBase
    {
        public AutoSoakingRunningState(SoakingModule module) : base(module)
        {
            _isPinAlignNeededCalled = false;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsPinAlignNeeded())
                {
                    bool injected = Module.CommandManager().SetCommand<IDOPINALIGN>(Module);

                    if (injected)
                    {
                        Module.InnerStateTransition(new AwaitingPinAlignCompletionState(Module));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");

                        return EventCodeEnum.NONE;
                    }
                }

                Module.DialogManager.ShowWaitCancelDialog(this.GetHashCode().ToString(), "Soaking, Wait...", null, "", true);
                double ZClearanceForAutoSokaing = (Module.SoakingDeviceFile_IParam as SoakingDeviceFile).AutoSoakingParam.ZClearance.Value;

                ret = SoakingFunc(ZClearanceForAutoSokaing, 0, EnumSoakingType.AUTO_SOAK);

                if (ret == EventCodeEnum.PIN_ALIGN_FAILED)
                {
                    //TODO 에러 띄워야함 방식은 정해야함
                    Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                    Module.StateTransitionToErrorState(ret, new EventCodeInfo(Module.ReasonOfError.ModuleType, ret, Module.ReasonOfError.GetLastEventMessage().ToString(), Module.GetType().Name));

                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                    LoggerManager.Error($"Error occurred during EventSoakRunning .");
                }
                else if (ret != EventCodeEnum.NONE)
                {
                    Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                    Module.StateTransitionToErrorState(ret, new EventCodeInfo(Module.ReasonOfError.ModuleType, ret, Module.ReasonOfError.GetLastEventMessage().ToString(), Module.GetType().Name));

                    LoggerManager.Error($"AutoSoaking Error, Clearnce:{ZClearanceForAutoSokaing}");
                    throw new Exception();
                }
                else
                {
                    Module.InnerStateTransition(new WaitAutoSoakingRunningState(Module));

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        int index = Module.LoaderController().GetChuckIndex();
                        Module.LoaderController().SetTitleMessage(index, Module.GetModuleMessage());
                    }

                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.AUTOSOAKING_ERROR;

                Module.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR);
                Module.StateTransitionToErrorState(ret);
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");

                LoggerManager.Exception(err);
            }
            finally
            {
                Module.DialogManager.CloseWaitCancelDialaog(this.GetHashCode().ToString(), true);
            }

            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.AUTOSOAKING_RUNNING;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class AutoSoakingDoneState : SoakingStateBase
    {
        public AutoSoakingDoneState(SoakingModule module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.DONE, $"device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()},Type: AutoSoak", this.Module.LoaderController().GetChuckIndex());
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new SoakingIdleState(Module));
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                ret = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message, "Error occurred SoakingDoneState.");
                //LoggerManager.Error($err.InnerException);

                LoggerManager.Exception(err);

                Module.StateTransitionToErrorState(ret);
                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
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

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    //public class TempDiffSoakingRunningState : SoakingStateBase
    //{
    //    public TempDiffSoakingRunningState(SoakingModule module) : base(module)
    //    { }

    //    public override EventCodeEnum Execute()
    //    {
    //        EventCodeEnum ret = EventCodeEnum.UNDEFINED;

    //        try
    //        {
    //            ret = SoakingFunc(Module.SoakingDeviceFile_Clone.TemperatureDiffZClearance.Value, 0);
    //            Module.InnerStateTransition(new AutoSoakingDoneState(Module));
    //            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
    //        }
    //        catch (Exception err)
    //        {

    //            LoggerManager.Exception(err);

    //            ret = EventCodeEnum.AUTOSOAKING_ERROR;
    //            Module.InnerStateTransition(new SoakingErrorState(Module));
    //            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
    //        }

    //        return ret;
    //    }

    //    public override ModuleStateEnum GetModuleState()
    //    {
    //        return ModuleStateEnum.RUNNING;
    //    }

    //    public override SoakingStateEnum GetState()
    //    {
    //        return SoakingStateEnum.TEMPDIFFSOAKING_RUNNING;
    //    }

    //    public override bool CanExecute(IProbeCommandToken token)
    //    {
    //        bool isValidCommand = false;
    //        return isValidCommand;
    //    }

    //    public override EventCodeEnum Pause()
    //    {
    //        return EventCodeEnum.NONE;
    //    }
    //}

    public class EventSoakingSuspendedState : SoakingStateBase
    {
        public EventSoakingSuspendedState(SoakingModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                AlignStateEnum pinalignstate = Module.StageSupervisor().ProbeCardInfo.AlignState.Value;
                AlignStateEnum waferalignstate = Module.StageSupervisor().WaferObject.AlignState.Value;

                if (pinalignstate == AlignStateEnum.DONE)
                {
                    if (Module.SequenceEngineManager().GetRunState())
                    {
                        if (Module.SoakPriorityList.Count > 0)
                        {
                            while (true)
                            {
                                var curSoakType = Module.SoakPriorityList.FirstOrDefault();
                                int SoakingTime = 0;

                                if (curSoakType == null)
                                {
                                    break;
                                }
                                else if (curSoakType.EventSoakingType.Value == EnumSoakingType.LOTRESUME_SOAK)
                                {
                                    if (curSoakType.SoakingTimeInSeconds.Value >= Module.AccAtResumeTime)
                                    {
                                        Module.UpdateEventSoakingPriority();
                                        LoggerManager.Debug($"[SoakingModule] Soak time: {curSoakType.SoakingTimeInSeconds.Value}sec. " +
                                            $"Total soaking time:{Module.AccumulatedTime}sec, Total Resume soaking time:{Module.AccAtResumeTime}sec");
                                        break;
                                    }
                                    LoggerManager.Debug($"[SoakingModule] Skip {curSoakType.EventSoakingType.Value} skip soak time: {curSoakType.SoakingTimeInSeconds.Value}sec. " +
                                           $"Total soaking time:{Module.AccumulatedTime}sec, Total Resume soaking time:{Module.AccAtResumeTime}sec");
                                }

                                SoakingTime = curSoakType.SoakingTimeInSeconds.Value - Module.AccumulatedTime;

                                if (SoakingTime < 0 & Module.AccumulatedTime != 0)
                                {
                                    LoggerManager.Debug($"[SoakingModule] Skip [{curSoakType.EventSoakingType.Value}], skip soak time: {curSoakType.SoakingTimeInSeconds.Value}sec. " +
                                        $"Total soaking time:{Module.AccumulatedTime}sec");

                                    Module.SoakPriorityList.Dequeue();
                                    Module.OffSoakingTrigger(curSoakType.EventSoakingType.Value);

                                    if (curSoakType.EventSoakingType.Value == EnumSoakingType.DEVICECHANGE_SOAK)
                                    {
                                        Module.DeviceChangeFlag = false;
                                    }
                                }
                                else
                                {
                                    break;
                                }

                                if (Module.SoakPriorityList.Count == 0)
                                {
                                    break;
                                }
                            }
                        }

                        //Align 중 fail 이 났을 경우 EventSoakingRunningState -> done 으로 변경 할수 있는 위험이 있어서 수정함
                        if (pinalignstate == AlignStateEnum.IDLE || waferalignstate == AlignStateEnum.IDLE)
                        {
                            //Align 중 fail 이 났다는 의미 
                            Module.InnerStateTransition(new SoakingIdleState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        }
                        else
                        {
                            Module.InnerStateTransition(new EventSoakingRunningState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.GetState()}");
                        }

                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.EVENTSOAKING_SUSPENDED;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new SoakingPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class SoakingPauseState : SoakingStateBase
    {
        public SoakingPauseState(SoakingModule module) : base(module)
        {
            PauseDate = DateTime.Now;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;

            return ret;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.PAUSE;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }
        DateTime PauseDate;
        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                // retVal = Module.InnerStateTransition(new SoakingPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.AccAtResumeTime = 0;
                LoggerManager.Debug($"EventSoakingRunningState(): Reset AccAtResumeTime as {Module.AccAtResumeTime}");
                LotResumeEventSoaking lotresumeSoakobj = Module.GetEventSoakingObject(EnumSoakingType.LOTRESUME_SOAK) as LotResumeEventSoaking;
                TimeSpan timetoResume = PauseDate - DateTime.Now;
                if (lotresumeSoakobj != null)
                {
                    int ResumeSoakType = lotresumeSoakobj.ResumeSoakingSkipTime.Value;
                    if (ResumeSoakType < Math.Abs(Convert.ToInt32(timetoResume.TotalSeconds)))
                    {
                        if (lotresumeSoakobj.Enable.Value == true)
                        {
                            Module.LotResumeTriggered = true;
                        }
                    }
                    else
                    {
                        // ResumeSoaking이 돌아가는 중이라면 Trigger를 끄면 안되기 때문에 해당 조건 들어감
                        if (Module.PreInnerState.GetType() != typeof(WaitSoakingRunningState))
                        {
                            Module.LotResumeTriggered = false;
                        }
                    }
                    LoggerManager.Debug($"[SoakingModule] Lot resume soak trigger {Module.LotResumeTriggered}");
                    LoggerManager.Debug($"[SoakingModule] Soaking SoakType: LOTRESUME_SOAK LotResumeTriggered : {Module.LotResumeTriggered}");
                }
                Module.UpdateEventSoakingPriority();

                if (Module.PreInnerState.GetType() == typeof(WaitSoakingRunningState))
                {
                    Module.InnerStateTransition(new SoakingSuspendState(Module, typeof(WaitSoakingRunningState)));
                    //Module.InnerStateTransition(new WaitSoakingRunningState(Module));
                }
                else if (Module.PreInnerState.GetType() == typeof(WaitChuckAwaySoakingRunningState))
                {
                    Module.InnerStateTransition(new SoakingSuspendState(Module, typeof(WaitChuckAwaySoakingRunningState)));
                    //Module.InnerStateTransition(new WaitChuckAwaySoakingRunningState(Module));
                }
                else if (Module.PreInnerState.GetType() == typeof(EventSoakingSuspendedState))
                {
                    Module.InnerStateTransition(new SoakingSuspendState(Module, typeof(EventSoakingSuspendedState)));
                    //Module.InnerStateTransition(new EventSoakingSuspendedState(Module));
                }
                else
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
    }

    public class AwaitingPinAlignCompletionState : SoakingStateBase
    {
        private bool pinAlignComplete = false;
        private bool waferAlignComplete = false;

        private bool waferAlignCommandInjected = false;
        public AwaitingPinAlignCompletionState(SoakingModule module, bool waferAlignTrigger = false) : base(module)
        {
            waferAlignComplete = !waferAlignTrigger;
        }

        private bool CheckCommandOperationFinished(IStateModule TargetModule)
        {
            bool retval = false;

            try
            {
                // 내가 갖고 있는 (SendSlot의 HashCode)와 Target의 Token이 갖고 있는 HashCode가 같은지 비교.
                if (Module.CommandSendSlot.Token.SubjectInfo == TargetModule.CommandRecvDoneSlot.Token.SubjectInfo)
                {
                    if (TargetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.DONE &&
                        TargetModule.ModuleState.GetState() != ModuleStateEnum.RUNNING)
                    {
                        // TODO : 
                        TargetModule.CommandRecvDoneSlot.ClearToken();

                        retval = true;
                    }
                    else if (TargetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ABORTED ||
                             TargetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ERROR)
                    {
                        retval = true;
                    }
                    else if (TargetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.NOCOMMAND)
                    {
                        retval = true;
                    }
                    else if (TargetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.REJECTED)
                    {
                        retval = true;
                    }
                    else
                    {
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private bool IsRunningState()
        {
            bool retval = false;

            try
            {
                var pretype = Module.PreInnerState.GetType();

                retval = pretype == typeof(EventSoakingRunningState) ||
                         pretype == typeof(ChuckAwaySoakingRunningState) ||
                         pretype == typeof(AutoSoakingRunningState) &&
                         Module.SequenceEngineManager().GetRunState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void CheckPinAlignment()
        {
            try
            {
                if (pinAlignComplete)
                {
                    return;
                }

                pinAlignComplete = CheckCommandOperationFinished(Module.PinAligner());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CheckWaferAlignment()
        {
            if (waferAlignComplete)
            {
                return;
            }

            if (!waferAlignCommandInjected)
            {
                InjectWaferAlignCommand();

                if(!waferAlignCommandInjected)
                {
                    waferAlignComplete = true;
                }
            }
            else
            {
                waferAlignComplete = CheckCommandOperationFinished(Module.WaferAligner());
            }
        }

        private void InjectWaferAlignCommand()
        {
            try
            {
                // TODO : Pin Align을 수행했지만, DONE을 유지하고 있는 상황.
                if (Module.StageSupervisor().WaferObject.AlignState.Value == AlignStateEnum.DONE)
                {
                    waferAlignComplete = true;
                    return;
                }

                waferAlignCommandInjected = Module.CommandManager().SetCommand<IDOWAFERALIGN>(Module);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (!IsRunningState())
                {
                    return EventCodeEnum.NONE;
                }

                CheckPinAlignment();

                if (pinAlignComplete)
                {
                    CheckWaferAlignment();
                }

                if (pinAlignComplete && waferAlignComplete)
                {
                    retVal = Module.InnerStateTransition(Module.PreInnerState);
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
            return ModuleStateEnum.SUSPENDED;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.SUSPENDED;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }
        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = Module.InnerStateTransition(new SoakingPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = EventCodeEnum.NONE;
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SoakingInfo soakinfo = new SoakingInfo();
                soakinfo.SoakingType = "No Soak";
                soakinfo.RemainTime = 0;
                soakinfo.ZClearance = 0;
                soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                Module.LoaderController()?.UpdateSoakingInfo(soakinfo);
                Module.InnerStateTransition(new SoakingAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class SoakingSuspendState : SoakingStateBase
    {
        Type PreSoakingStateBeforePause;
        public SoakingSuspendState(SoakingModule module, Type preSoakingStateBase) : base(module)
        {
            PreSoakingStateBeforePause = preSoakingStateBase;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    if (PreSoakingStateBeforePause == typeof(WaitSoakingRunningState))
                    {
                        if ((Module.InnerState as SoakingState).GetType() != typeof(WaitSoakingRunningState))
                        {
                            Module.InnerStateTransition(new WaitSoakingRunningState(Module));
                        }
                    }
                    else if (PreSoakingStateBeforePause == typeof(WaitChuckAwaySoakingRunningState))
                    {
                        if ((Module.InnerState as SoakingState).GetType() != typeof(WaitChuckAwaySoakingRunningState))
                        {
                            Module.InnerStateTransition(new WaitChuckAwaySoakingRunningState(Module));
                        }
                    }
                    else if (PreSoakingStateBeforePause == typeof(EventSoakingSuspendedState))
                    {
                        if ((Module.InnerState as SoakingState).GetType() != typeof(EventSoakingSuspendedState))
                        {
                            Module.InnerStateTransition(new EventSoakingSuspendedState(Module));
                        }
                    }
                    else
                    {
                        retVal = Module.InnerStateTransition(Module.PreInnerState);
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
            return ModuleStateEnum.SUSPENDED;
        }

        public override SoakingStateEnum GetState()
        {
            return SoakingStateEnum.SUSPENDED;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }
        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = Module.InnerStateTransition(new SoakingPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = EventCodeEnum.NONE;
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SoakingInfo soakinfo = new SoakingInfo();
                soakinfo.SoakingType = "No Soak";
                soakinfo.RemainTime = 0;
                soakinfo.ZClearance = 0;
                soakinfo.ChuckIndex = Module.LoaderController().GetChuckIndex();
                Module.LoaderController()?.UpdateSoakingInfo(soakinfo);
                Module.InnerStateTransition(new SoakingAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }


    public class SoakingAbortState : SoakingStateBase
    {
        public SoakingAbortState(SoakingModule module) : base(module)
        {
            if (module.PreInnerState.GetModuleState() == ModuleStateEnum.RUNNING)
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                module.EventManager().RaisingEvent(typeof(PreHeatEndEvent).FullName, new ProbeEventArgs(module, semaphore));
                semaphore.Wait();
                //Module.GEMModule().SetEvent(Module.GEMModule().GetEventNumberFormEventName(typeof(PreHeatEndEvent).FullName));
            }
            LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.ABORT, $"device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}", this.Module.LoaderController().GetChuckIndex());
            //LoggerManager.Debug($"Soaking Abort!");
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = Module.InnerStateTransition(new SoakingIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
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

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }
    public class SoakingErrorState : SoakingStateBase
    {
        public SoakingErrorState(SoakingModule module, EventCodeInfo eventcode = null) : base(module)
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.SOAKING, StateLogType.ERROR, $"device:{Module.FileManager().GetDeviceName()} , card ID:{Module.CardChangeModule().GetProbeCardID()}", Module.LoaderController().GetChuckIndex());

                if (this.GetModuleState() == ModuleStateEnum.ERROR && eventcode != null)
                {
                    Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Current State = {this.GetModuleState()}, Can not add ReasonOfError.");
                }

                RaisingPreHeatFailEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //  LoggerManager.Debug($"Soaking Error! ");
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum ret = EventCodeEnum.AUTOSOAKING_ERROR;
            try
            {
                //LoggerManager.Debug($"Soaking Error! ");            
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
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

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            return isValidCommand;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new SoakingIdleState(Module));
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

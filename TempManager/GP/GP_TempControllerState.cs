using System;
using System.Threading.Tasks;

namespace TempControl
{
    using LogModule;
    using MetroDialogInterfaces;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.DialogControl;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Event;
    using ProberInterfaces.Temperature.Chiller;
    using ProberInterfaces.WaferTransfer;
    using System.Threading;
    using Temperature;
    using ProberInterfaces.Temperature;

    public class TC_ColdIdleState : TCColdStateBase
    {
        public TC_ColdIdleState(TempController module) : base(module)
        {

        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.IDLE;
        public override EnumTemperatureState GetState() => EnumTemperatureState.IDLE;

        /// <summary>
        /// Execute 구현 내용.
        ///            
        /// 구현 1.       IChangeTempTemperatureToSetTemp, IChangeTemperatureToSetTempWhenWaferTransfer가 들어오면
        ///              State 변경.-------------ok.
        /// 구현 1-1-1.   IsEnableOverHeating이 true일때, IChangeTemperatureToSetTempWhenWaferTransfer Command가 들어오면,
        ///              TCOverheatingTriggerState로 상태 변경.------------ok.
        /// 구현 1-1-2.   IsEnableOverHeating이 false일때, IChangeTemperatureToSetTempWhenWaferTransfer Command가 들어오면,
        ///              TCNomalTriggerState로 상태 변경.------------ok.
        /// 구현 1-2.     IChangeTemperatureToSetTempWhenWaferTransfer Command가 들어오면
        ///              TCNomalTriggerState로 상태 변경.-----------------ok.
        /// 구현 2.       Probing중이면 TCMonitoringState로 변경.(suspend, running 일때가 프로빙 상태임.)-----ok.
        /// 
        /// </summary>
        /// <returns>EventCodeEnum</returns>
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //구현 1 로직.
                retVal = ProbeCmdExecutor();

                //구현 2 로직.
                retVal = BehaviorFollowingProbingModuleState();

                //구현 3 로직.
                retVal = PreSetTempCompareExcutor();

                if (Module.TempManager.Get_OutPut_State() == 0)
                {
                    Module.TempManager.Set_OutPut_ON(null); // Heater on
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private EventCodeEnum ProbeCmdExecutor()
        {
            try
            {
                EventCodeEnum executeResult = EventCodeEnum.NONE;
                bool consumed;

                if (Module.CommandRecvSlot.IsRequested<IChangeTemperatureToSetTempAfterConnectTempController>())
                {
                    ChangeTempToSetTempDoAction();
                }

                Func<bool> conditionFunc = () => true;
                #region => IChangeTemperatureToSetTempAfterConnectTempController Command
                Action changeTempToSetTempDoAction = ChangeTempToSetTempDoAction;
                Action changeTempToSetTempAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - IChangeTempToSetTempCommandExecutor()] => {executeResult.ToString()}");
                    };
                #endregion

                consumed = Module.CommandManager()
                    .ProcessIfRequested<IChangeTemperatureToSetTemp>(
                        Module, conditionFunc, changeTempToSetTempDoAction, changeTempToSetTempAbortAction);

                #region => IChangeTempToSetTempFullReach Command
                Action IChangeTempToSetTempFullReachDoAction = ChangeTempToSetTempFullReachDoAction;
                Action IChangeTempToSetTempFullReachAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - IChangeTempToSetTempFullReach()] => {executeResult.ToString()}");
                    };
                #endregion

                consumed = Module.CommandManager()
                    .ProcessIfRequested<IChangeTempToSetTempFullReach>(
                        Module, conditionFunc, IChangeTempToSetTempFullReachDoAction, IChangeTempToSetTempFullReachAbortAction);

                #region => IChangeTemperatureToSetTempWhenWaferTransfer Command
                Action startWaferTransferDoAction = StartWaferTransferDoAction;
                Action startWaferTransferAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - IChangeTemperatureToSetTempWhenWaferTransfer()] => {executeResult.ToString()}");
                    };
                #endregion

                consumed = Module.CommandManager()
                    .ProcessIfRequested<IChangeTemperatureToSetTempWhenWaferTransfer>(
                        Module, conditionFunc, startWaferTransferDoAction, startWaferTransferAbortAction);

                #region => ISetTempForFrontDoorOpen Command
                Action setTempForFrontDoorOpenDoAction = SetTempForFrontDoorOpen;
                Action setTempForFrontDoorOpenAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ISetTempForFrontDoorOpen()] => {executeResult.ToString()}");
                    };
                #endregion

                consumed = Module.CommandManager()
                    .ProcessIfRequested<ISetTempForFrontDoorOpen>(
                        Module, conditionFunc, setTempForFrontDoorOpenDoAction, setTempForFrontDoorOpenAbortAction);

                return executeResult;

                void ChangeTempToSetTempDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] " +
                        $"StateTransition to {nameof(TC_ColdNomalTriggerState)} => {executeResult.ToString()}");
                }

                void ChangeTempToSetTempFullReachDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdWaitingFullReachSetTempTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempFullReachDoAction()] " +
                        $"StateTransition to {nameof(TC_ColdWaitingFullReachSetTempTriggerState)} => {executeResult.ToString()}");
                }

                void StartWaferTransferDoAction()
                {
                    if (Module.TempControllerDevParameter.IsEnableOverHeating.Value == true)
                    {
                        executeResult = Module.InnerStateTransition(new TC_ColdOverHeatingTriggerState(Module));
                    }
                    else
                    {
                        executeResult = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                    }
                    LoggerManager.Debug($"[{this.GetType().Name} - StartWaferTransferDoAction()] " +
                        $"StateTransition to {Module.InnerState.GetType().Name} => {executeResult.ToString()}");
                }

                void SetTempForFrontDoorOpen()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdSetTempForFrontDoorOpenReachedTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - SetTempForFrontDoorOpen()] " +
                        $"StateTransition to {nameof(TC_ColdSetTempForFrontDoorOpenReachedTriggerState)} => {executeResult.ToString()}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token,
                                typeof(IChangeTemperatureToSetTemp),
                                typeof(IChangeTempToSetTempFullReach),
                                typeof(IChangeTemperatureToSetTempWhenWaferTransfer),
                                typeof(ISetTempForFrontDoorOpen),
                                typeof(IChangeTemperatureToSetTempAfterConnectTempController));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdMonitoringState : TCColdStateBase
    {
        //private Type ReserveCommandType = null;

        public TC_ColdMonitoringState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Monitoring;
        /// <summary>
        /// Execute 구현 내용.
        /// 
        /// 구현 0.     pause 안함.------------------------------------------------ok. 세부구현 : ok.
        /// 구현 1.     한 번이라도 값이 튀면 pause.--------------------------------ok. 세부구현 : ok.
        /// 구현 1-1.   한 번이라도 값이 튀면 강제 Z-Down, GPIB 연결 끊기.-----------ok. 세부구현 : ok.
        /// 구현 1-2.   한 번이라도 값이 뒤면 Z-Down 기다렸다가 Lot Pause.-----------ok. 세부구현 : ok.
        /// 
        /// </summary>
        /// <returns>EventCodeEnum</returns>
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IProbingModule ProbingModule = Module.ProbingModule();


                if (ProbingModule.ModuleState.State == ModuleStateEnum.IDLE
                    || ProbingModule.ModuleState.State == ModuleStateEnum.DONE
                    || ProbingModule.ModuleState.State == ModuleStateEnum.PAUSED
                    || Module.GetParam_Wafer().GetState() == EnumWaferState.PROCESSED)
                {
                    if (Module.IsUsingChillerState())
                    {
                        retVal = Module.InnerStateTransition(new TempInRange(Module));
                        return retVal;
                    }
                    else
                    {
                        retVal = Module.InnerStateTransition(new TC_ColdDonePerformState(Module));
                        return retVal;
                    }
                }
                if (!CheckDewPointMonitoring())
                {
                    retVal = this.Module.InnerStateTransition(new EmergencyPurge(this.Module));
                    this.Module.ProbingModule().IsReservePause = true;
                    this.Module.CommandManager().SetCommand<ILotOpPause>(this);
                    return retVal;
                }

                bool IsEmergencyTempWithinRange = this.IsEmergencyTempWithinRange(Module.TempInfo.SetTemp.Value);
                if (IsEmergencyTempWithinRange == false)
                {
                    //구현 0 로직. (조건 0에 해당되면 그냥 지나간다.)
                    if (Module.TempControllerDevParameter.TempPauseType.Value == TempPauseTypeEnum.NONE)
                    {
                        var commstate = Module.EnvControlManager().GetChillerModule()?.GetCommState() ?? EnumCommunicationState.DISCONNECT;                        
                        if (commstate == EnumCommunicationState.CONNECTED | Module.IsUsingChillerState())                             
                        {
                            // 웨이퍼 로딩 후, 순간적으로 온도가 바뀔 수 있으며, 이 때 이곳으로 들어올 수 있다.
                            retVal = Module.InnerStateTransition(new TC_ColdWaitingFullReachSetTempTriggerState(Module));
                            return retVal;
                        }
                        else
                        {
                            LoggerManager.Debug($"[TC_ColdMonitoringState] Chiller not connected.");
                            retVal = Module.InnerStateTransition(new ChillerConnectError(this.Module));
                            return retVal;
                        }
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdNomalTriggerState : TCColdStateBase
    {
        public TC_ColdNomalTriggerState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.SetToTemp;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Func<bool> conditionFunc = () => true;
                #region => IChangeTemperatureToSetTemp Command
                Action changeTempToSetTempDoAction = ChangeTempToSetTempDoAction;
                Action changeTempToSetTempAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - IChangeTempToSetTempCommandExecutor()] => {retVal.ToString()}");
                    };

                void ChangeTempToSetTempDoAction()
                {
                    retVal = Module.InnerStateTransition(new TC_ColdNomalTriggerPerformState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] " +
                        $"StateTransition to {nameof(TC_ColdNomalTriggerPerformState)} => {retVal.ToString()}");
                }
                #endregion

                bool consumed = Module.CommandManager()
                    .ProcessIfRequested<IChangeTemperatureToSetTempAfterConnectTempController>(
                        Module, conditionFunc, changeTempToSetTempDoAction, changeTempToSetTempAbortAction);

                if(consumed == false)
                {
                    if (Module.IsUsingChillerState())
                    {
                        //Chiller 를 사용하는 온도 라면 ChillerGroup 확인 하는 상태로 변경한다.
                        retVal = Module.InnerStateTransition(new WaitForTempChangeConfirmation(Module));
                    }
                    else
                    {
                        retVal = Module.InnerStateTransition(new TC_ColdNomalTriggerPerformState(Module));
                    }
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
            bool isValidCommand = false;
            try
            {
                isValidCommand = CheckCanExecuteUsingInterfaceType(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdNomalTriggerPerformState : TCColdStateBase
    {
        private bool isTriggered;
        private int retryCount;
        private DateTime lastupdateTime = DateTime.Now;
        private TimeSpan lowfreqInv = new TimeSpan(0, 0, 10);   // 10 seconds
        public TC_ColdNomalTriggerPerformState(TempController module) : base(module)
        {
            isTriggered = false;
            LoggerManager.Debug($"TC_ColdNomalTriggerPerformState - isTriggered set to false");
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.SetToTemp;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (isTriggered == false)
                {
                    if (retryCount < 2 | (DateTime.Now - lastupdateTime) > lowfreqInv)
                    {
                        Module.TempManager.SetRemote_ON(null);

                        //히터 꺼져있으면 키기
                        if (Module.TempManager.Get_OutPut_State() == 0)
                        {
                            Module.TempManager.Set_OutPut_ON(null);
                        }

                        double targetTemp = Module.TempInfo.TargetTemp.Value;
                        retVal = SetTemperature(targetTemp);

                        LoggerManager.Debug($"[CHANGE TEMP START (Temperature Change Trigger)] Change Temp : {targetTemp}");
                        //Controller 에 적용되었는지 확인 하고 isTriggered 변경해주기.
                        //설정 실패 한다면 잠깐의 시간을 준 뒤 다시 시도 하기. (OPUS 왓츠컴 참고 - 주기와 Retry 횟수 고민)
                        isTriggered = true;
                        lastupdateTime = DateTime.Now;
                        LoggerManager.Debug($"TC_ColdNomalTriggerPerformState - isTriggered set to true");
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    // 설정 한 온도가 온도 Contoller 에 적용되고, 그 데이터로 TempInfo.SetTemp 까지 변경되었는지 확인하고 넘어가기
                    if (Module.TempInfo.SetTemp.Value == Module.TempInfo.TargetTemp.Value)
                    {
                        retryCount = 0;
                        if (Module.IsUsingChillerState())
                        {
                            retVal = Module.InnerStateTransition(new Activated(Module));
                            return retVal;
                        }
                        else
                        {
                            if(Module.EnvControlManager().GetValveState(EnumValveType.IN))
                            {
                                Module.EnvControlManager().SetValveState(false, EnumValveType.IN);
                            }

                            var ChillerModule = Module.EnvControlManager().GetChillerModule();
                            if (ChillerModule != null)
                            {
                                ChillerModule.Inactivate();
                            }

                            LoggerManager.Debug($"TargetTemp : {Module.TempInfo.TargetTemp.Value}, Chiller CoolantInTemp " +
                                $": {Module.EnvControlManager().GetChillerModule().ChillerParam.CoolantInTemp.Value}");
                            retVal = Module.InnerStateTransition(new TC_ColdWaitUntilNomalSetTempReached(Module));
                            return retVal;
                        }
                    }
                    else
                    {
                        if ((Module.GetApplySVChangesBasedOnDeviceValue() && (Module.TempControllerDevParameter.SetTemp.Value == Module.TempInfo.TargetTemp.Value))
                            || !(Module.GetApplySVChangesBasedOnDeviceValue()))
                        {
                            if (Module.TempInfo.SetTemp.Value != Module.TempInfo.TargetTemp.Value)
                            {
                                isTriggered = false;
                                retryCount++;
                            }
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

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }


    public class TC_ColdWaitUntilNomalSetTempReached : TCColdStateBase
    {
        private DateTime runningTime = DateTime.Now;

        public TC_ColdWaitUntilNomalSetTempReached(TempController module) : base(module)
        {
            runningTime = DateTime.Now;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;
        public override EventCodeEnum Execute()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                bool reachTargetTemp = false;
                DateTime curDateTime = DateTime.Now;
                TimeSpan ts = curDateTime - runningTime;
                Module.RunTimeSpan = ts;

                //if (Module.TempControllerParam.LimitRunTimeSeconds.Value < ts.TotalSeconds)
                //{
                //    ICommandManager CommandManager = Module.CommandManager();
                //    ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                //    if (TempDisplayDialogService.IsShowing)
                //    {
                //        TempDisplayDialogService.CloseDialog();
                //    }
                //    CommandManager.SetCommand<IGpibAbort>(Module);
                //    Module.InnerStateTransition(new TC_ColdErrorPerformState(Module));
                //}
                //else
                //{
                try
                {
                    reachTargetTemp = IsTempWithinRange(Module.TempInfo.SetTemp.Value);
                    if (reachTargetTemp == true)
                    {
                        ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                        if (TempDisplayDialogService.IsShowing)
                        {
                            TempDisplayDialogService.CloseDialog();
                        }
                        retVal = Module.InnerStateTransition(new TC_ColdDonePerformState(Module));
                        return retVal;
                    }
                    else
                    {
                        if (Module.IsUsingChillerState())
                        {
                           //Chiller 를 사용할 수 있는지 확인.
                            IChillerModule chillermodule = Module.EnvControlManager().GetChillerModule();
                            if (chillermodule != null)
                            {
                                retVal = chillermodule.CheckCanUseChiller(Module.TempInfo.SetTemp.Value);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    double chillerSV = Module.TempInfo.SetTemp.Value + chillermodule.GetChillerTempoffset(Module.TempInfo.SetTemp.Value);
                                    Module.MetroDialogManager().ShowMessageDialog("Information Message",
                                        $"Set the chiller SV to {chillerSV} degrees.", EnumMessageStyle.Affirmative);
                                    retVal = Module.InnerStateTransition(new Activated(Module));
                                    return retVal;
                                }
                            }
                        }
                    }
                    retVal = EventCodeEnum.NONE;
                        
                }
                catch
                {
                    retVal = Module.InnerStateTransition(new TC_ColdErrorState(Module));
                    return retVal;
                }
                //}

                #region => ISetTempForFrontDoorOpen Command
                Func<bool> conditionFunc = () => true;
                Action setTempForFrontDoorOpenDoAction = SetTempForFrontDoorOpen;
                Action setTempForFrontDoorOpenAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCWaitUntilNomalSetTempReached]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ISetTempForFrontDoorOpen()] => {retVal.ToString()}");
                    };
                bool consumed = Module.CommandManager()
                .ProcessIfRequested<ISetTempForFrontDoorOpen>(
                 Module, conditionFunc, setTempForFrontDoorOpenDoAction, setTempForFrontDoorOpenAbortAction);
                #endregion

                if ((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE)
                {
                    return retVal;
                }
                    

                EventCodeEnum executeResult = EventCodeEnum.NONE;
                #region => ITemperatureSettingTriggerOccurrence Command
                Action temperatureSettingTriggerOccurrenceDoAction = TemperatureSettingTriggerOccurrenceDoAction;
                Action temperatureSettingTriggerOccurrenceAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ITemperatureSettingTriggerOccurrence()] => {executeResult.ToString()}");
                    };

                void TemperatureSettingTriggerOccurrenceDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] " +
                        $"StateTransition to {nameof(TC_ColdNomalTriggerState)} => {executeResult.ToString()}");
                }
                #endregion

                consumed = Module.CommandManager()
                   .ProcessIfRequested<ITemperatureSettingTriggerOccurrence>(
                       Module, conditionFunc, temperatureSettingTriggerOccurrenceDoAction, temperatureSettingTriggerOccurrenceAbortAction);

                return retVal;

                void SetTempForFrontDoorOpen()
                {
                    retVal = Module.InnerStateTransition(new TC_ColdSetTempForFrontDoorOpenReachedTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - SetTempForFrontDoorOpen()] " +
                        $"StateTransition to {nameof(TC_ColdSetTempForFrontDoorOpenReachedTriggerState)} => {retVal.ToString()}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {
                isValidCommand = CheckCanExecuteUsingInterfaceType(token, 
                    typeof(ISetTempForFrontDoorOpen),
                    typeof(ITemperatureSettingTriggerOccurrence));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }


        private new EventCodeEnum PreSetTempCompareExcutor()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if ( Module.TempInfo.TargetTemp.Value != Module.TempInfo.SetTemp.Value)
                {
                    retVal = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));

                }
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
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdWaitUntilSetTempFullReached : TCColdStateBase
    {
        private DateTime RunningTime = DateTime.Now;

        public TC_ColdWaitUntilSetTempFullReached(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool reachTargetTemp = false;
                DateTime curDateTime = DateTime.Now;
                TimeSpan ts = curDateTime - RunningTime;
                Module.RunTimeSpan = ts;

                //if (Module.TempControllerParam.LimitRunTimeSeconds.Value < ts.TotalSeconds)
                //{
                //    ICommandManager CommandManager = Module.CommandManager();
                //    ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                //    if (TempDisplayDialogService.IsShowing)
                //    {
                //        TempDisplayDialogService.CloseDialog();
                //    }
                //    CommandManager.SetCommand<IGpibAbort>(Module);
                //    Module.InnerStateTransition(new TCErrorPerformState(Module));
                //}
                //else
                //{
                try
                {
                    if (Module.TempControllerParam.LimitRunTimeSeconds.Value < ts.TotalSeconds)
                    {
                        ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                        if (TempDisplayDialogService.IsShowing)
                        {
                            TempDisplayDialogService.CloseDialog();
                        }

                        if (Module.TempControllerDevParameter.TempPauseType.Value != TempPauseTypeEnum.NONE)
                        {
                            StopProbingThatDependOnParameter();
                        }

                        ReservedCommandSet();
                        //Module.InnerStateTransition(new TC_ColdErrorState(Module));
                        retVal = EventCodeEnum.UNDEFINED;
                    }

                    double setTemp = Module.TempInfo.SetTemp.Value;

                    reachTargetTemp = IsTempWithinRange(setTemp);
                    if (reachTargetTemp == true)
                    {
                        ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                        if (TempDisplayDialogService.IsShowing)
                        {
                            TempDisplayDialogService.CloseDialog();
                        }
                        if (Module.IsUsingChillerState())
                        {
                            retVal = Module.InnerStateTransition(new TempInRange(Module));
                            return retVal;
                        }
                        else
                        {
                            retVal = Module.InnerStateTransition(new TC_ColdDonePerformState(Module));
                            return retVal;
                        }
                    }
                    retVal = EventCodeEnum.NONE;
                }
                catch
                {
                    ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                    if (TempDisplayDialogService.IsShowing)
                    {
                        TempDisplayDialogService.CloseDialog();
                    }
                    Module.InnerStateTransition(new TC_ColdErrorState(Module));
                    retVal = EventCodeEnum.UNDEFINED;
                }
                //}

                bool consumed;
                EventCodeEnum executeResult = EventCodeEnum.NONE;
                Func<bool> conditionFunc = () => true;
                #region => ITemperatureSettingTriggerOccurrence Command
                Action temperatureSettingTriggerOccurrenceDoAction = TemperatureSettingTriggerOccurrenceDoAction;
                Action temperatureSettingTriggerOccurrenceAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ITemperatureSettingTriggerOccurrence()] => {executeResult.ToString()}");
                    };

                void TemperatureSettingTriggerOccurrenceDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] " +
                        $"StateTransition to {nameof(TC_ColdNomalTriggerState)} => {executeResult.ToString()}");
                }
                #endregion

                consumed = Module.CommandManager()
                   .ProcessIfRequested<ITemperatureSettingTriggerOccurrence>(
                       Module, conditionFunc, temperatureSettingTriggerOccurrenceDoAction, temperatureSettingTriggerOccurrenceAbortAction);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token,
                    typeof(ITemperatureSettingTriggerOccurrence));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdWaitingFullReachSetTempTriggerState : TCColdStateBase
    {
        public TC_ColdWaitingFullReachSetTempTriggerState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double targetTemp = Module.TempInfo.TargetTemp.Value;

                retVal = SetTemperature(targetTemp);

                ITempDisplayDialogService TempDisplayDialogService = Module.TempDisplayDialogService();
                if (TempDisplayDialogService.IsShowing == false & SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    TempDisplayDialogService.TurnOnPossibleFlag();
                    Task dialogServiceTask = Task.Run(async () =>
                    {
                        bool result = false;
                        result = await TempDisplayDialogService.ShowDialog();

                        if (result == false)
                        {
                            Module.InnerStateTransition(new TC_ColdErrorState(Module));
                        }
                    });
                }

                retVal = Module.InnerStateTransition(new TC_ColdWaitUntilSetTempFullReached(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdOverHeatingTriggerState : TCColdStateBase
    {
        public TC_ColdOverHeatingTriggerState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.SetToTemp;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double targetTemp = Module.TempInfo.TargetTemp.Value;
                double overheatingTemp = Module.TempControllerDevParameter.OverHeatingOffset.Value;
                retVal = SetTemperature(targetTemp, overheatingTemp);
                retVal = Module.InnerStateTransition(new TC_ColdWaitUntilOverHeatingSetTempReached(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdWaitUntilOverHeatingSetTempReached : TCColdStateBase
    {
        private DateTime runningTime = DateTime.Now;

        public TC_ColdWaitUntilOverHeatingSetTempReached(TempController module) : base(module)
        {
            runningTime = DateTime.Now;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool reachTargetTemp = false;
                DateTime curDateTime = DateTime.Now;
                TimeSpan ts = curDateTime - runningTime;
                Module.RunTimeSpan = ts;

                //if (Module.TempControllerParam.LimitRunTimeSeconds.Value < ts.TotalSeconds)
                //{
                //    Module.InnerStateTransition(new TC_ColdErrorState(Module));
                //}
                //else
                //{
                try
                {
                    IWaferAligner WaferAligner = Module.WaferAligner();
                    IWaferTransferModule WaferTransferModule = Module.WaferTransferModule();

                    reachTargetTemp = IsTempWithinRange(Module.TempInfo.SetTemp.Value, TempCheckType.OVERHEATING);
                    if ((reachTargetTemp == true && WaferTransferModule.ModuleState.State == ModuleStateEnum.DONE)
                        || WaferAligner.ModuleState.State == ModuleStateEnum.DONE)
                    {
                        ReturnToOriginSetTemp();
                        Module.InnerStateTransition(new TC_ColdDonePerformState(Module));
                    }
                    retVal = EventCodeEnum.NONE;
                }
                catch
                {
                    Module.InnerStateTransition(new TC_ColdErrorState(Module));
                    retVal = EventCodeEnum.UNDEFINED;
                }
                //}

                EventCodeEnum ReturnToOriginSetTemp()
                {
                    retVal = EventCodeEnum.UNDEFINED;
                    try
                    {
                        retVal = SetTemperature(Module.TempInfo.SetTemp.Value);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
                    return retVal;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }



        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdSetTempForFrontDoorOpenReachedTriggerState : TCColdStateBase
    {
        public TC_ColdSetTempForFrontDoorOpenReachedTriggerState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.SetToTemp;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double SetTemp = Module.TempControllerParam.FrontDoorOpenTemp.Value;

                retVal = SetTemperature(SetTemp, willYouSaveSetValue: false);
                retVal = Module.InnerStateTransition(new TC_ColdWaitUntilSetTempForFrontDoorOpenReached(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdWaitUntilSetTempForFrontDoorOpenReached : TCColdStateBase
    {
        private DateTime runningTime = DateTime.Now;

        public TC_ColdWaitUntilSetTempForFrontDoorOpenReached(TempController module) : base(module)
        {
            runningTime = DateTime.Now;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForRechead;
        public override EventCodeEnum Execute()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                bool reachTargetTemp = false;
                bool consumed = false;
                DateTime curDateTime = DateTime.Now;
                TimeSpan ts = curDateTime - runningTime;
                Module.RunTimeSpan = ts;

                try
                {
                    #region => IStopFrontDoorOpenSetTemp Command
                    Func<bool> conditionFunc = () => true;
                    Action stopFrontDoorOpenSetTempDoAction = StopFrontDoorOpenSetTemp;
                    Action stopFrontDoorOpenSetTempAbortAction =
                        () =>
                        {
                            Module.MetroDialogManager().ShowMessageDialog(
                                "[TCWaitUntilSetTempForFrontDoorOpenReached]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                            LoggerManager.Debug($"[{this.GetType().Name} - IStopFrontDoorOpenSetTemp()] => Fail");
                        };
                    #endregion

                    consumed = Module.CommandManager()
                        .ProcessIfRequested<IReturnToDefaltSetTemp>(
                            Module, conditionFunc, stopFrontDoorOpenSetTempDoAction, stopFrontDoorOpenSetTempAbortAction);

                    reachTargetTemp = IsTempWithinRange(Module.TempControllerParam.FrontDoorOpenTemp.Value);
                    if (reachTargetTemp == true)
                    {
                        //retVal = SetTemperature(TempController.PreSetTemp);
                        Module.InnerStateTransition(new TC_ColdDonePerformState(Module));
                    }

                    if (Module.SequenceRunner().ModuleState.GetState() == ModuleStateEnum.DONE
                        || Module.SequenceRunner().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        ICommandManager CommandManager = Module.CommandManager();
                        Module.InnerStateTransition(new TC_ColdIdleState(Module));
                        bool t = CommandManager.SetCommand<IChangeTemperatureToSetTemp>(Module);
                    }
                    retVal = EventCodeEnum.NONE;
                }
                catch
                {
                    retVal = SetTemperature(Module.TempInfo.PreSetTemp.Value);
                    Module.InnerStateTransition(new TC_ColdErrorState(Module));
                    retVal = EventCodeEnum.UNDEFINED;
                }

                return retVal;


                void StopFrontDoorOpenSetTemp()
                {
                    retVal = Module.InnerStateTransition(new TC_ColdIdleState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - SetTempForFrontDoorOpen()] " +
                        $"StateTransition to {nameof(TC_ColdDoneState)} => {retVal.ToString()}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token, typeof(IReturnToDefaltSetTemp));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdErrorPerformState : TCColdStateBase
    {
        public TC_ColdErrorPerformState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Error;
        /// <summary>
        /// Execute() 구현.
        /// 
        ///         구현 1.     Error에 빠지기전에 처리.------------------------------------ok. 세부구현 : ok.
        /// to do:  구현 1-1.   알람.------------------------------------------------------ok. 세부구현 : -no.
        ///         구현 1-2.   State Breaker.---------------------------------------------ok. 세부구현 : ok.
        /// 
        /// </summary>
        /// <returns></returns>
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetErrorState();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void SetErrorState()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                bool functionResult = false;

                //구현 1 로직.
                functionResult = ProceduresPriorToErrorStateChange();
                retVal = Module.InnerStateTransition(new TC_ColdErrorState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private bool ProceduresPriorToErrorStateChange()
        {
            bool functionResult = false;
            try
            {
                //구현 1-1 로직.
                functionResult = Alam();

                //구현 1-2 로직.
                if (Module.TempControllerDevParameter.IsBreakAlignState.Value == true
                    && Module.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROBING) //zup, probing일 때는 위험하니 Align 데이터 깨지않도록 한다.

                {
                    functionResult = AlignModuleStateBreaker();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return functionResult;
        }

        private bool Alam()
        {
            bool result = false;
            try
            {
                //todo : Alam().. how???
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }

        private bool AlignModuleStateBreaker()
        {
            bool result = false;
            try
            {

                try
                {
                    Module.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                    Module.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    Module.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.IDLE);
                    result = true;
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    result = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdErrorState : TCColdStateBase
    {
        private bool CommunicationError = false;
        public TC_ColdErrorState(TempController module, bool checkConnectState = false) : base(module)
        {
            if (Module.IsOccurTimeOut)
            {
                Module.IsOccurTimeOut = false;
            }
            // CommunicationError: ENV 연결 상태 error 로 TC_ColdErrorState로 왔다는 의미
            if (CommunicationError != checkConnectState)
            {
                CommunicationError = checkConnectState;
                LoggerManager.Debug($"[TempState] TC_ColdErrorState() CommunicationError set to {CommunicationError}.");
            }
        } 

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ERROR;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Error;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if ((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE)
                    return retVal;
                retVal = EventCodeEnum.NONE;
                
                if (CommunicationError)
                {
                    // ENV control 통신 문제로 TC_ColdError 가 되었는데, 다시 ENV control 통신이 붙은 경우, Activated 상태로 보낸다.
                    if (Module.EnvControlManager().GetIsExcute() == true)
                    {                        
                        LoggerManager.Debug($"CommunicationError set to {CommunicationError}. But EnvControl Service is available. So switch to activated state.");
                        Module.InnerStateTransition(new Activated(Module));
                        return retVal;
                    }                    
                }
                else
                {
                    if (Module.IsUsingChillerState())
                    {
                        var commstate = Module.EnvControlManager().GetChillerModule()?.GetCommState() ?? EnumCommunicationState.DISCONNECT;
                        if (commstate != EnumCommunicationState.CONNECTED)
                        {
                            retVal = Module.InnerStateTransition(new ChillerConnectError(this.Module));
                            return retVal;
                        }
                    }
                }                                
                
                EventCodeEnum PreSetTempCompareExcutor()
                {
                    retVal = EventCodeEnum.UNDEFINED;
                    try
                    {
                        if ( Module.TempInfo.TargetTemp.Value != Module.TempInfo.SetTemp.Value)
                        {
                            retVal = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
                    return retVal;
                }



                // Error 상태에서 Valve 열려있으면 무조건 닫도록 한다.
                if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == true)
                {
                    retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                        //Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                        return retVal;
                    }
                }

                if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == true)
                {
                    retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                        //Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                        return retVal;
                    }
                }

                #region => IEndTempEmergencyError Command

                bool consumed;
                EventCodeEnum executeResult = EventCodeEnum.NONE;
                Func<bool> conditionFunc = () => true;
                Action IEndTempEmergencyErrorDoAction = ChangeEndTempEmergencyErrorDoAction;
                consumed = Module.CommandManager()
                    .ProcessIfRequested<IEndTempEmergencyError>(
                        Module, conditionFunc, IEndTempEmergencyErrorDoAction);

                void ChangeEndTempEmergencyErrorDoAction()
                {
                    executeResult = Module.InnerStateTransition(new Activated(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeEndTempEmergencyErrorDoAction()] " +
                        $"StateTransition to {nameof(Activated)} => {executeResult.ToString()}");
                }
                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {
                isValidCommand = CheckCanExecuteUsingInterfaceType(token, typeof(IEndTempEmergencyError));
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
            try
            {
                Module.InnerStateTransition(new TC_ColdIdleState(Module));

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
                Module.InnerStateTransition(new TC_ColdIdleState(Module));

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdDonePerformState : TCColdStateBase
    {
        public TC_ColdDonePerformState(TempController module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Done;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.InnerStateTransition(new TC_ColdDoneState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {

                isValidCommand = CheckCanExecuteUsingInterfaceType(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class TC_ColdDoneState : TCColdStateBase
    {
        public TC_ColdDoneState(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Done;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.TempInfo.TargetTemp.Value != Module.TempInfo.SetTemp.Value)
                {
                    retVal = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                    return retVal;
                }

                if (!Module.IsCurTempWithinSetTempRange(false))
                {
                    retVal = Module.InnerStateTransition(new TC_ColdWaitUntilNomalSetTempReached(Module));
                    return retVal;
                }

                if ((retVal = BehaviorFollowingProbingModuleState()) == EventCodeEnum.NONE)
                    return retVal;

                if (Module.IsUsingChillerState())
                {
                    var chillerTargetTemp = Module.TempInfo.SetTemp.Value + ChillerModule.GetChillerTempoffset(Module.TempInfo.SetTemp.Value);
                    if (chillerTargetTemp != Module.EnvControlManager().GetChillerModule().GetSetTempValue())
                    {
                        retVal = Module.InnerStateTransition(new PauseDifferenceTemp(Module));
                        return retVal;
                    }
                    else
                    {
                        retVal = Module.InnerStateTransition(new Activated(Module));
                        return retVal;
                    }
                }
                else
                {
                    // chiller 를 사용하지 않고 온도제어를 하는데 chiller 연결 끊겼다고 상태 변경이 되지 않아야 한다.
                    //if (ChillerModule.GetChillerMode() != EnumChillerModuleMode.NONE)
                    //{
                        //if (ChillerModule.GetCommState() != EnumCommunicationState.CONNECTED)
                        //{
                        //    retVal = Module.InnerStateTransition(new ChillerConnectError(this.Module));
                        //    return retVal;
                        //}
                    //}
                }

                bool consumed;
                EventCodeEnum executeResult = EventCodeEnum.NONE;
                Func<bool> conditionFunc = () => true;
                #region => ITemperatureSettingTriggerOccurrence Command
                Action temperatureSettingTriggerOccurrenceDoAction = TemperatureSettingTriggerOccurrenceDoAction;
                Action temperatureSettingTriggerOccurrenceAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - ITemperatureSettingTriggerOccurrence()] => {executeResult.ToString()}");
                    };

                void TemperatureSettingTriggerOccurrenceDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] " +
                        $"StateTransition to {nameof(TC_ColdNomalTriggerState)} => {executeResult.ToString()}");
                }
                #endregion

                consumed = Module.CommandManager()
                   .ProcessIfRequested<ITemperatureSettingTriggerOccurrence>(
                       Module, conditionFunc, temperatureSettingTriggerOccurrenceDoAction, temperatureSettingTriggerOccurrenceAbortAction);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;
            try
            {
                isValidCommand = CheckCanExecuteUsingInterfaceType(token,
                    typeof(ITemperatureSettingTriggerOccurrence));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isValidCommand;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }
}

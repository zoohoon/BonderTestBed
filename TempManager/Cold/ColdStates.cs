using System;
using System.Threading.Tasks;

namespace TempControl
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Temperature.Chiller;
    using ProberInterfaces.Command.Internal;
    using Temperature;
    using MetroDialogInterfaces;
    using System.Threading;
    using NotifyEventModule;
    using ProberInterfaces.Event;

    public abstract class ActivatePerform : TCColdStateBase
    {
        public ActivatePerform(TempController module) : base(module)
        {
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
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

    public abstract class InactivatePerform : TCColdStateBase
    {
        public InactivatePerform(TempController module) : base(module)
        {
        }
        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
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

    public class Inactivated : TCColdStateBase
    {
        private bool SetChillerSV = false;
        public Inactivated(TempController module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Inactivated;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.IDLE;


        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!(ChillerModule.GetCommState() == EnumCommunicationState.CONNECTED))
                {
                    retVal = Module.InnerStateTransition(new ChillerConnectError(this.Module));
                    return retVal;
                }

                if (((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE) | ((retVal = CheckChillerAbortOp()) == EventCodeEnum.NONE) | ((retVal = CheckChillerDifferentTemp()) == EventCodeEnum.NONE))
                {
                    return retVal;
                }

                if (!SetChillerSV)
                {
                    Module.EnvControlManager().SetValveState(false, EnumValveType.IN);
                    Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);
                    Module.EnvControlManager().SetValveState(false, EnumValveType.DRAIN);
                    Module.EnvControlManager().SetValveState(false, EnumValveType.PURGE);

                    //TODO: 영원히 Inactivate 안돌고 있었는데 NONE 구문 빼는게 맞을까? 
                    //if (setSvRet == EventCodeEnum.NONE & ChillerModule.IsTempControlActive() == false)
                    if (ChillerModule.IsTempControlActive() == false)
                    {
                        ChillerModule.Inactivate();
                    }

                    SetChillerSV = true;
                }

                bool consumed;
                EventCodeEnum executeResult = EventCodeEnum.NONE;

                #region => IEndTempEmergencyError Command
                Func<bool> conditionFunc = () => true;
                Action IEndTempEmergencyErrorDoAction = ChangeEndTempEmergencyErrorDoAction;

                consumed = Module.CommandManager().ProcessIfRequested<IEndTempEmergencyError>(Module, conditionFunc, IEndTempEmergencyErrorDoAction);

                void ChangeEndTempEmergencyErrorDoAction()
                {
                    executeResult = Module.InnerStateTransition(new Activated(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeEndTempEmergencyErrorDoAction()] StateTransition to {nameof(EmergencyPurge)} => {executeResult.ToString()}");
                }
                #endregion

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
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] StateTransition to {nameof(TC_ColdNomalTriggerState)} => {executeResult.ToString()}");
                }
                #endregion

                consumed = Module.CommandManager().ProcessIfRequested<ITemperatureSettingTriggerOccurrence>(Module, conditionFunc, temperatureSettingTriggerOccurrenceDoAction, temperatureSettingTriggerOccurrenceAbortAction);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;

            try
            {
                retVal = CheckCanExecuteUsingInterfaceType(token, typeof(IEndTempEmergencyError), typeof(ITemperatureSettingTriggerOccurrence));
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

    public class Activated : TCColdStateBase
    {
        private DateTime InitDataTime;
        private bool forcedActivated = false;
        public Activated(TempController module, bool forced = false) : base(module)
        {
            InitDataTime = DateTime.Now;
            forcedActivated = forced;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Activated;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;
        bool svMissmatched = false;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ChillerModule.GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    if (Module.IsUsingChillerState() && ChillerModule.CheckCanUseChiller(Module.TempInfo.TargetTemp.Value) != EventCodeEnum.NONE && forcedActivated == false)
                    {
                        retVal = Module.InnerStateTransition(new WaitForTempChangeConfirmation(this.Module));
                        return retVal;
                    }

                    // Target 과 SV 의 데이터가 동기화 되었는지 확인 ( Normal Trigger 에서 데이터 변경후, 확인하고 넘어와야 되지 않나 싶음 )
                    // TempManager.SV 는 실제 TempController(Hardware) 값.
                    // TempInfo.SetTemp 는 TempContoller 의 SequenceRun 에서 TempManager.SV 값을 넣어준다.
                    // TempInfo.TargetTemp 는 온도가 바뀌었을때 TempController 의 SetSV 에서 값을 넣어준다. 
                    if ((Module.TempInfo.TargetTemp.Value != Module.TempManager.SV) ||
                        (Module.TempInfo.TargetTemp.Value != Module.TempInfo.SetTemp.Value))
                    {
                        if (svMissmatched == false)
                        {
                            LoggerManager.Debug($"TempControl.Activated(): SV({Module.TempManager.SV}) is differ from target temp.({Module.TempInfo.TargetTemp.Value})");
                        }

                        svMissmatched = true;
                        PreSetTempCompareExcutor();
                        return EventCodeEnum.NONE;
                    }
                    else
                    {
                        svMissmatched = false;
                    }

                    //셀의 SV 를 기준으로 Chiller Temp Offset 을 계산하여 Target 온도를 계산함.=> Cell's Chiller Target 온도로 표현.;
                    double chillerTargetSV = ChillerModule.ConvertTargetTempApplyOffset(Module.TempInfo.SetTemp.Value);

                    //칠러를 사용하는 온도가 아니라면, Hot 로직으로 가고 (else 문)
                    //칠러를 사용하는 온도지만,  Cell's Chiller Target 와 Chiller 의 SV 가 같고 온도도 deviation 에 들어온다면 TempInRange 상태로 변경된다. 
                    if ((Module.TempInfo.TargetTemp.Value <= ChillerModule.ChillerParam.CoolantInTemp.Value))
                    {
                        // TODO: Activated 에서 바로 TempInRange로 가는 것은 유효하지 않을 수도 있으므로 WaitForDewPoint -> WaitForChiller -> Chilling -> TempInRange 순서로 전환한다.
                        // 또한 해당 구문 때문에 아래쪽에 Activate()가 호출되지 않음.
                        //if (Module.IsCurTempWithinSetTempRange() && (chillerTargetSV == ChillerModule.ChillerInfo.SetTemp))
                        //{
                        //    retVal = Module.InnerStateTransition(new TempInRange(Module));
                        //    return retVal;
                        //}
                    }
                    else
                    {
                        LoggerManager.Debug($"TargetTemp : {Module.TempInfo.TargetTemp.Value}, Chiller CoolantInTemp : ${ChillerModule.ChillerParam.CoolantInTemp.Value}");
                        retVal = Module.InnerStateTransition(new TC_ColdWaitUntilNomalSetTempReached(Module));

                        return retVal;
                    }

                    // 칠러의 SV 와 칠러 Target 온도가 다르다면 칠러에 온도 설정을 해야함.
                    if (ChillerModule.ChillerInfo.SetTemp != chillerTargetSV)
                    {
                        //칠러에 온도를 설정한다.
                        var ret = ChillerModule.SetNormalChilling(Module.TempInfo.SetTemp.Value, TempValueType.HUBER, forcedActivated);

                        if (ret == EventCodeEnum.NONE)
                        {
                            while (true)
                            {
                                if (((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE) | ((retVal = CheckChillerAbortOp()) == EventCodeEnum.NONE))
                                {
                                    return retVal;
                                }

                                // 칠러의 SV 와 칠러 Target 온도가 같다면 끝냄
                                if (ChillerModule.GetSetTempValue() == chillerTargetSV)
                                {
                                    break;
                                }
                                else
                                {
                                    //Activated State 로 들어오고나서 칠러 온도를 변경했을때 실제 칠러온도와 칠러 Target 온도가 동기화 되는시간 Timeout (100 ms)
                                    var curTime = DateTime.Now;
                                    var offsetTime = curTime - InitDataTime;

                                    if (offsetTime.TotalSeconds > 100) //
                                    {
                                        LoggerManager.Debug("[Temp Activated State] Time out wait chiller target temp == chiller internal temp.");
                                        Module.InnerStateTransition(new TC_ColdIdleState(Module));

                                        break;
                                    }
                                }
                            }
                        }

                        if (ret == EventCodeEnum.NONE)
                        {
                            ChillerModule.ChillerInfo.UseLimitChiller = false;

                            //2020.11.09 Add by Leina (칠러 부하를 줄이기 위해 Activate 하기전에 벨브를 열어야한다)
                            if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == false)
                            {
                                retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.IN);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                    Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                    return retVal;
                                }
                            }

                            if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == false)
                            {
                                retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.OUT);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                    Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                    return retVal;
                                }
                            }
                        }
                        else
                        {   // 칠러 온도 설정에 실패 한경우 로직

                            //2020.11.05 Add by Leina
                            Module.EnvControlManager().SetValveState(false, EnumValveType.IN);
                            Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                            LoggerManager.Debug($"[Chiller #{ChillerModule.ChillerInfo.Index}]can not change temperature. Already in-use. Chiller SV = {ChillerModule.ChillerInfo.SetTemp}℃, Chiller Target SV : {chillerTargetSV}, retVal = {retVal}");

                            // MAINTENANCE 모드인 경우에 SV 가 0도 이상이면 Hot 로직으로 변경한다.
                            // SV 가 0도 이하인 경우에는 에러 메세지 띄우고 Error 상태로 변경한다.
                            if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE)
                            {
                                if (Module.TempInfo.TargetTemp.Value >= 0)
                                {
                                    this.Module.InnerStateTransition(new TC_ColdWaitUntilNomalSetTempReached(this.Module));
                                }
                                else
                                {
                                    // Maintanance Mode - Set Temp fail.
                                    Task dialogserviceTask = Task.Run(async () =>
                                    {
                                        await Module.MetroDialogManager().ShowMessageDialog("Set Temperature Error.",
                                            "When another stage in the same layer is in the chiller operation, the temperature can be changed only to room temperature.",
                                            MetroDialogInterfaces.EnumMessageStyle.Affirmative);

                                        Module.InnerStateTransition(new PauseDifferenceTemp(Module));

                                    });
                                }
                            }
                            // MAINTENANCE 모드가 아닌경우에는 Target 온도와 SV 를 비교한다.
                            else
                            {
                                LoggerManager.Debug($"[Temp Cold Activated State] - SetNormalChilling Fail (TargetTemp : {chillerTargetSV} ) (ChillerOPTemp : {ChillerModule.GetInternalTempValue()})");
                                ChillerModule.ChillerInfo.UseLimitChiller = true;

                                //설정하려는 칠러온도와 현재 칠러온도 비교를 왜 했었을까... 
                                //if (chillerTargetSV == ChillerModule.GetSetTempValue())
                                if (ChillerModule.IsMatchedTargetTemp(chillerTargetSV))
                                {
                                    Module.InnerStateTransition(new WaitForDewPoint(Module));
                                }
                                else
                                {
                                    // 온도설정도 실패하고 chiller targer 온도와 현재 설정된 chiller set temp 가 다를경우 처리.
                                    Module.InnerStateTransition(new PauseDifferenceTemp(Module));
                                }
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[Temp Cold Activated] ChillerTargetTemp : {chillerTargetSV} == Chiller SV : {chillerTargetSV} ");

                        if (((retVal = CheckChillerDifferentTemp()) == EventCodeEnum.NONE))
                        {
                            return retVal;
                        }

                        //- Temperature control current status.
                        //  0: Temperature control not active.
                        //  1: Temperature control active.
                        if (ChillerModule.IsTempControlActive() == true)
                        {
                            LoggerManager.Debug($"[Temp Cold Activated] TempControl Active is true, TargetTemp : {chillerTargetSV}, Chiller SV : {chillerTargetSV}");
                            this.Module.InnerStateTransition(new WaitForDewPoint(this.Module));
                            ChillerModule.ChillerInfo.SetActiveState(true);

                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            LoggerManager.Debug($"[Temp Cold Activated] TempControl Active is false, TargetTemp : {chillerTargetSV}, Chiller SV : {chillerTargetSV}");

                            //2020.11.09 Add by Leina (칠러 부하를 줄이기 위해 Activate 하기전에 벨브를 열어야한다)
                            if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == false)
                            {
                                retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.IN);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                    Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                    return retVal;
                                }
                            }

                            if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == false)
                            {
                                retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.OUT);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                    Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                    return retVal;
                                }
                            }

                            ChillerModule.Activate();
                            Module.InnerStateTransition(new WaitForDewPoint(this.Module));
                            ChillerModule.ChillerInfo.SetActiveState(true);

                            retVal = EventCodeEnum.NONE;
                        }
                    }
                }
                else
                {
                    //Chiller 연결이 안되어있다면 연결에러 상태로 변경한다.
                    LoggerManager.Debug($"[Activated] Chiller not connected.");
                    this.Module.InnerStateTransition(new ChillerConnectError(this.Module));
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
            bool retVal = false;
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

    public class WaitForTempChangeConfirmation : TCColdStateBase
    {
        bool writtenLog;
        public WaitForTempChangeConfirmation(TempController module) : base(module)
        {
            writtenLog = false;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.SUSPENDED;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForCondition;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            EventCodeEnum checkConditionRetVal = EventCodeEnum.NONE;
            try
            {
                if ((checkConditionRetVal = ChillerModule.CheckCanUseChiller(Module.TempInfo.TargetTemp.Value)) == EventCodeEnum.NONE)
                {
                    double targetTemp = Module.TempInfo.TargetTemp.Value;
                    double setTemp = Module.TempInfo.SetTemp.Value;

                    if (targetTemp != setTemp)
                    {
                        retVal = Module.InnerStateTransition(new TC_ColdNomalTriggerPerformState(Module));
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
                    // 다른 셀이 Chiller 온도를 설정하는 중이면 기다린다.
                    if (checkConditionRetVal != EventCodeEnum.CHILLER_CHECK_ALREADY_OCCUPY)
                    {
                        if (Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE)
                        {
                            double targetTemp = Module.TempInfo.TargetTemp.Value;
                            double setTemp = Module.TempInfo.SetTemp.Value;
                            if (targetTemp != setTemp)
                            {
                                //칠러 사용 못하는데 Maintenance Mode 에서 온도 변경 시 Heater 로 온도 제어 하도록 상태 변경 한다.
                                retVal = Module.InnerStateTransition(new TC_ColdNomalTriggerPerformState(Module));
                                return retVal;
                            }
                        }


                        if (Module.TempInfo.SetTemp.Value == Module.TempInfo.TargetTemp.Value)
                        {
                            if (ChillerModule.IsMatchedTargetTemp(Module.TempInfo.SetTemp.Value) == false)
                            {
                                // Chiller 사용 조건이 안되는데 Coolant Valve 가 열려있다면 Valve 를 닫는다.
                                if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == true)
                                {
                                    retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                        Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                        return retVal;
                                    }
                                }
                                if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == true)
                                {
                                    retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                        Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                        return retVal;
                                    }
                                }
                            }

                            if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE || Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                            {
                                retVal = CheckChillerDifferentTemp();
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    // PauseDifferenceTemp State로 Transition된다.
                                    return retVal;
                                }
                            }
                        }

                        if (!CheckDewPointMonitoring())
                        {
                            retVal = this.Module.InnerStateTransition(new EmergencyPurge(this.Module));
                            if (Module.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                            {
                                this.Module.ProbingModule().IsReservePause = true;
                                this.Module.CommandManager().SetCommand<ILotOpPause>(this);
                            }
                            return retVal;
                        }
                    }
                    else
                    {
                        if (Module.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE && Module.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE)
                        {
                            double targetTemp = Module.TempInfo.TargetTemp.Value;
                            double setTemp = Module.TempInfo.SetTemp.Value;
                            if (targetTemp != setTemp)
                            {
                                //칠러 사용 못하는데 Maintenance Mode 에서 온도 변경 시 Heater 로 온도 제어 하도록 상태 변경 한다.
                                retVal = Module.InnerStateTransition(new TC_ColdNomalTriggerPerformState(Module));
                                return retVal;
                            }
                        }
                    }
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

                consumed = Module.CommandManager().ProcessIfRequested<ITemperatureSettingTriggerOccurrence>(Module, conditionFunc, temperatureSettingTriggerOccurrenceDoAction, temperatureSettingTriggerOccurrenceAbortAction);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if(writtenLog == false)
                {
                    LoggerManager.Debug($"{GetStateStr()}");
                    writtenLog = true;
                }
                else if ((Module.InnerState as TempControllerState).GetState() != this.GetState())
                {
                    LoggerManager.Debug($"{GetStateStr()}");
                }

                string GetStateStr()
                {
                    string str = $"[TempState][WaitForTempChangeConfirmation] RetVal = {checkConditionRetVal}, TargetTemp : {Module.TempInfo.TargetTemp.Value}, SetTemp : {Module.TempInfo.SetTemp.Value}, Chiller SV : {ChillerModule?.ChillerInfo?.SetTemp}.";
                    return str;
                }
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isValidCommand = false;

            try
            {
                isValidCommand = CheckCanExecuteUsingInterfaceType(token, typeof(ITemperatureSettingTriggerOccurrence));
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

    public class Branch : TCColdStateBase
    {
        public Branch(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForChiller;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            double tcTargetTemp = 0.0;
            double tcCurTemp = 0.0;
            double curDewPoint = 0.0;
            //double ambientTemp = 0.0;
            double hotLimitTemp = 0.0;
            double activatableHighTemp = 0.0;

            try
            {
                curDewPoint = Module.EnvControlManager().GetDewPointModule().CurDewPoint;
                tcTargetTemp = Module.TempInfo.TargetTemp.Value;
                tcCurTemp = Module.TempInfo.CurTemp.Value;
                hotLimitTemp = ChillerModule.ChillerParam.ChillerHotLimitTemp.Value;
                activatableHighTemp = ChillerModule.ChillerParam.ActivatableHighTemp.Value;

                //Limit 온도보다 Target 온도가 높을경우 
                if (hotLimitTemp <= tcTargetTemp)   //if (hotLimitTemp <= tcCurTemp)
                {
                    this.Module.InnerStateTransition(new Inactivated(this.Module));
                }
                else if (tcTargetTemp <= activatableHighTemp)
                {
                    this.Module.InnerStateTransition(new Chilling(this.Module));
                }
                else if (tcTargetTemp < hotLimitTemp && tcTargetTemp > activatableHighTemp)
                {
                    this.Module.InnerStateTransition(new Heatting(this.Module));
                }
                else
                {
                    this.Module.InnerStateTransition(new Inactivated(this.Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                this.Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }

            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
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

    public class Chilling : ActivatePerform
    {
        public Chilling(TempController module) : base(module)
        {
            if (Module.IsOccurTimeOut)
            {
                Module.IsOccurTimeOut = false;
            }
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Chilling;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE) | ((retVal = CheckChillerAbortOp()) == EventCodeEnum.NONE) | ((retVal = CheckChillerDifferentTemp()) == EventCodeEnum.NONE))
                {
                    return retVal;
                }

                if (ChillerModule.GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    if (!CheckDewPointMonitoring())
                    {
                        this.Module.InnerStateTransition(new WaitForDewPoint(this.Module));
                        return retVal;
                    }

                    // Heater
                    if (Module.TempInfo.CurTemp.Value <= Module.TempInfo.SetTemp.Value + Module.TempController().GetDeviaitionValue())
                    {
                        if (Module.TempManager.Get_OutPut_State() == 0)
                        {
                            Module.TempManager.Set_OutPut_ON(null); // Heater on
                        }
                    }

                    //Valve
                    if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == false | Module.EnvControlManager().GetValveState(EnumValveType.OUT) == false)
                    {
                        //칠러중단 당한게 아니거나 & 온도가 Limit 을 넘지 않았다면 벨브를 다시 연다.

                        if ((!ChillerModule.ChillerInfo.AbortChiller) & ((Module.TempInfo.CurTemp.Value <= ChillerModule.ChillerParam.ChillerHotLimitTemp.Value)))
                        {
                            retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.IN);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                return retVal;
                            }

                            retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.OUT);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                return retVal;
                            }
                        }
                    }

                    if (Module.IsCurTempWithinSetTempRange())
                    {
                        Module.TempManager.Set_OutPut_ON(null); // Heater on
                        this.Module.InnerStateTransition(new TempInRange(this.Module));
                    }
                }
                else
                {
                    LoggerManager.Debug($"[Chilling] Chiller not connected.");

                    this.Module.InnerStateTransition(new ChillerConnectError(this.Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                this.Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }

            return retVal;
        }
    }

    public class WaitForChiller : ActivatePerform
    {
        public WaitForChiller(TempController module) : base(module)
        {
            if (Module.IsOccurTimeOut)
            {
                Module.IsOccurTimeOut = false;
            }
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForChiller;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            double setChillingTemp = ChillerModule.ChillerInfo.ZeroTemp;
            double tcTargetTemp = 0.0; // TempController의 SetTemp
            double curDewPoint = 0.0;
            double ambientTemp = 0.0;
            double hotLimitTemp = 0.0;
            double activatableHighTemp = 0.0;

            try
            {
                retVal = CheckChillerAbortOp();

                if (retVal == EventCodeEnum.NONE)
                {
                    // ChillerAbortStop State로 Transition된다.
                    return retVal;
                }

                retVal = CheckChillerDifferentTemp();

                if (retVal == EventCodeEnum.NONE)
                {
                    // PauseDifferenceTemp State로 Transition된다.
                    return retVal;
                }

                retVal = PreSetTempCompareExcutor();

                if (retVal == EventCodeEnum.NONE)
                {
                    // TC_ColdNomalTriggerState State로 Transition된다.
                    return retVal;
                }

                if (ChillerModule.GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    if (!CheckActiveValve())
                    {
                        if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == true)
                        {
                            retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                return retVal;
                            }
                        }

                        if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == true)
                        {
                            retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                               Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                               return retVal;
                            }
                        }
                    }

                    curDewPoint = Module.EnvControlManager().GetDewPointModule().CurDewPoint;
                    tcTargetTemp = Module.TempInfo.TargetTemp.Value;
                    ambientTemp = ChillerModule.ChillerParam.AmbientTemp.Value;
                    hotLimitTemp = ChillerModule.ChillerParam.ChillerHotLimitTemp.Value;
                    activatableHighTemp = ChillerModule.ChillerParam.ActivatableHighTemp.Value;

                    var settemp = ChillerModule.GetSetTempValue();
                    var internaltemp = ChillerModule.GetInternalTempValue();
                    var offset = Math.Abs(ChillerModule.GetSetTempValue() - ChillerModule.GetInternalTempValue());

                    // 적정 DewPoint
                    if (curDewPoint - DewPointModule.Tolerence <= ChillerModule.GetInternalTempValue())
                    {
                        if ((!ChillerModule.ChillerInfo.UseLimitChiller) & (Module.TempInfo.CurTemp.Value <= hotLimitTemp))
                        {
                            if (ChillerModule.IsTempControlActive() == false)// activate 가 되어있어야 valve를 여는 의미가 있으니까 
                            {
                                retVal = ChillerModule.Activate();
                                Thread.Sleep(100);
                            }
                            else
                            {
                                if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == false)
                                {
                                    retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.IN);

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                        Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                        return retVal;
                                    }
                                }

                                if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == false)
                                {
                                    retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.OUT);

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                        Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                        return retVal;
                                    }
                                }

                            }

                            if (retVal == EventCodeEnum.NONE)
                            {
                                ChillerModule.ChillerInfo.ActivdCoolantValve = true;
                            }
                        }
                        else
                        {
                            //현재 온도가 chiller hot limit 온도보다 높으면 벨브잠군다.
                            if (Module.TempInfo.CurTemp.Value >= hotLimitTemp)
                            {
                                if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == true)
                                {
                                    retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                        Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                        return retVal;
                                    }
                                }

                                if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == true)
                                {
                                    retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                        Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                        return retVal;
                                    }
                                }
                            }
                        }

                        //칠러 Set - Internal Temp 가 Range 안에 들어온다면 & 현재 온도가 chiller hot limit 온도보다 높으면
                        if ((offset <= ChillerModule.ChillerParam.InRangeWindowTemp.Value) | (Module.TempInfo.CurTemp.Value >= hotLimitTemp))
                        {
                            this.Module.InnerStateTransition(new Chilling(this.Module));

                            retVal = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == true)
                        {
                            retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                return retVal;
                            }
                        }

                        if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == true)
                        {
                            retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                return retVal;
                            }
                        }

                        LoggerManager.Debug($"[Temp Change WaitForChiller -> WaferForDewPoint]");

                        this.Module.InnerStateTransition(new WaitForDewPoint(this.Module));

                        retVal = EventCodeEnum.NONE;
                        return retVal;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[WaitForChiller] Chiller not connected.");
                    this.Module.InnerStateTransition(new ChillerConnectError(this.Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                this.Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }

            return retVal;
        }
    }

    public class WaitForDewPoint : ActivatePerform
    {
        double initDP;
        DateTime initTime = DateTime.Now;
        public bool rasingTimeOut = false;
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.DewPoint;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.IDLE;
        public WaitForDewPoint(TempController module) : base(module)
        {
            initDP = Module.EnvControlManager().GetDewPointModule().CurDewPoint;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            double curDewPoint = Module.EnvControlManager().GetDewPointModule().CurDewPoint;

            try
            {
                if (((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE) | ((retVal = CheckChillerAbortOp()) == EventCodeEnum.NONE) | ((retVal = CheckChillerDifferentTemp()) == EventCodeEnum.NONE))
                {
                    return retVal;
                }

                if (ChillerModule.GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    if(ChillerModule.ChillerInfo.SetTemp == -999 || ChillerModule.ChillerInfo.ChillerInternalTemp == -999)
                    {
                        return retVal;
                    }
                    if (!CheckActiveValve())
                    {
                        if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == true)
                        {
                            retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                return retVal;
                            }
                        }

                        if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == true)
                        {
                            retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                return retVal;
                            }
                        }
                    }

                    // Check Point: Coolant 의 CurTemp와 비교하지 않고 않고 Coolant의 TargetTemp를 보는 이유는 Coolant가 너무 빨리 들어갈 수도 있기 때문이다. 
                    //              Coolant의 가장 낮은 온도가 Chiller.TargetTemp일 것이라고 가정한 뒤 Coolant가 기화되는 것을 막기 위해서 DewPoint를 보수적으로 판단. 
                    if (ChillerModule.ChillerInfo.SetTemp - curDewPoint >= this.DewPointModule.Hysteresis)
                    {
                        if (ChillerModule.ChillerInfo.SetTemp < ChillerModule.ChillerParam.AmbientTemp.Value)
                        {
                            if (Module.TempManager.Get_OutPut_State() == 0)
                            {
                                Module.TempManager.TempModule.Set_OutPut_ON(null);
                            }
                        }//핫픽스 수정이라서 일단 OFF만 제거..

                        LoggerManager.Debug($"[Temp Change WaferForDewPoint -> WaitForChiller] (TargetTemp({ChillerModule.ChillerInfo.SetTemp} )- CurDewPoint({curDewPoint}) >= Hysterisys {DewPointModule.Hysteresis})");

                        this.Module.InnerStateTransition(new WaitForChiller(this.Module));

                        if (rasingTimeOut | Module.IsOccurTimeOut)
                        {
                            Module.IsOccurTimeOut = false;
                        }

                        retVal = EventCodeEnum.NONE;

                        return retVal;
                    }
                    else
                    {
                        double threshHold = curDewPoint;
                        //Modify by Leina 20.12.10
                        //chiller 의 process value가 표시되지않고 사용하지 않음.
                        //if ((initDP > threshHold) & (threshHold < ChillerModule.GetProcessTempVal()))
                        //TODO: initDP를 왜보는 거지? 이 State를 처음 들어왔을 때의 값보다 낮아지기를 기대했다고 함. 범위안에만 들어오면 되는데 굳이 필요 없는 코드 일수도..? 
                        if ((initDP >= threshHold) & (ChillerModule.GetInternalTempValue() - threshHold >= DewPointModule.Hysteresis))
                        {
                            //Module.TempManager.TempModule.Set_OutPut_OFF(null);
                            LoggerManager.Debug($"[Temp Change WaferForDewPoint -> WaitForChiller] " +
                                $"(InitDewPoint {initDP} >= DewPointThreshHold {threshHold}) & (ChillerProcessTemp ({ChillerModule.GetInternalTempValue()} ) - DewPointThreshHold ({threshHold})>=  Hysterisys {DewPointModule.Hysteresis})");

                            this.Module.InnerStateTransition(new WaitForChiller(this.Module));

                            if (rasingTimeOut | Module.IsOccurTimeOut)
                            {
                                Module.IsOccurTimeOut = false;
                            }

                            retVal = EventCodeEnum.NONE;

                            return retVal;
                        }
                        else
                        {
                            if (DateTime.Now.Subtract(initTime).TotalSeconds >= Module.EnvControlManager().GetDewPointModule().WaitTimeout)
                            {
                                if (!rasingTimeOut)
                                {
                                    LoggerManager.Debug($"[Temp Change WaferForDewPoint -> Error] Wait DewPoint Time Out {Module.EnvControlManager().GetDewPointModule().WaitTimeout}");

                                    //Rasing Alarm
                                    string cellDataStr = "";
                                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                                    {
                                        cellDataStr = $"[CELL #{Module.LoaderController()?.GetChuckIndex() ?? 0}].";
                                    }

                                    Module.NotifyManager().Notify(EventCodeEnum.DEW_POINT_TIMEOUT, $"{cellDataStr} A Wait Dewpoint TimeOut error has occurred. TimeOut : {Module.EnvControlManager().GetDewPointModule().WaitTimeout} (sec)");
                                    rasingTimeOut = true;
                                    Module.IsOccurTimeOut = true;
                                }

                                retVal = EventCodeEnum.DEW_POINT_TIMEOUT;
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[Wait For DewPoint] Chiller not connected.");
                    this.Module.InnerStateTransition(new ChillerConnectError(this.Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                this.Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }

            return retVal;
        }
    }

    public class UpperHotLimit : ActivatePerform
    {
        public UpperHotLimit(TempController module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Heatting;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Module.InnerStateTransition(new Heatting(this.Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                this.Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }

            return retVal;
        }
    }

    public class HotLimitToAmbChilling : ActivatePerform
    {
        public HotLimitToAmbChilling(TempController module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForChiller;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            double setChillingTemp = ChillerModule.ChillerInfo.ZeroTemp;
            double tcSetTemp = 0.0; // TempController의 SetTemp
            double curDewPoint = 0.0;
            double chillerOffset = 0.0;
            double hotLimitTemp = 0.0;
            double activatableHighTemp = 0.0;

            try
            {
                curDewPoint = Module.EnvControlManager().GetDewPointModule().CurDewPoint;
                tcSetTemp = Module.TempInfo.TargetTemp.Value;
                hotLimitTemp = ChillerModule.ChillerParam.ChillerHotLimitTemp.Value;
                activatableHighTemp = ChillerModule.ChillerParam.ActivatableHighTemp.Value;

                if (activatableHighTemp < tcSetTemp)
                {
                    ChillerModule.ChillerInfo.TargetTemp = tcSetTemp;
                    this.Module.InnerStateTransition(new Heatting(this.Module));
                }
                else
                {
                    if (hotLimitTemp < tcSetTemp)
                    {
                        this.Module.InnerStateTransition(new Branch(this.Module));
                    }
                    else
                    {
                        chillerOffset = ChillerModule.GetChillerTempoffset(Module.TempInfo.SetTemp.Value);
                        setChillingTemp = tcSetTemp + chillerOffset;

                        ChillerModule.SetSlowChilling(setChillingTemp, TempValueType.HUBER);
                        ChillerModule.Activate();
                        this.Module.InnerStateTransition(new Activated(this.Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                this.Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }

            return retVal;
        }
    }

    public class AmbToZeroChilling : ActivatePerform
    {
        public AmbToZeroChilling(TempController module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForChiller;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            double setChillingTemp = ChillerModule.ChillerInfo.ZeroTemp;
            double tcSetTemp = 0.0; // TempController의 SetTemp
            double curDewPoint = 0.0;
            double ambientTemp = 0.0;
            double activatableHighTemp = 0.0;

            try
            {
                curDewPoint = Module.EnvControlManager().GetDewPointModule().CurDewPoint;
                tcSetTemp = Module.TempInfo.TargetTemp.Value;
                setChillingTemp = tcSetTemp;
                ambientTemp = ChillerModule.ChillerParam.AmbientTemp.Value;
                activatableHighTemp = ChillerModule.ChillerParam.ActivatableHighTemp.Value;

                if (activatableHighTemp < tcSetTemp)
                {
                    ChillerModule.ChillerInfo.TargetTemp = activatableHighTemp;
                    this.Module.InnerStateTransition(new Heatting(this.Module));
                }
                else
                {
                    if (ambientTemp < tcSetTemp)
                    {
                        this.Module.InnerStateTransition(new Branch(this.Module));
                    }
                    else
                    {
                        setChillingTemp = setChillingTemp / 10;

                        ChillerModule.Activate();
                        ChillerModule.SetNormalChilling(setChillingTemp, TempValueType.TEMPCONTROLLER);
                        this.Module.InnerStateTransition(new Activated(this.Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                this.Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }
            return retVal;
        }
    }

    public class Heatting : InactivatePerform
    {
        public Heatting(TempController module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Heatting;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                // if( Temp Out Put 이 꺼져있다면
                if (Module.TempManager.Get_OutPut_State() == 0)
                {
                    this.Module.TempManager.Set_OutPut_ON(null);
                }

                this.Module.InnerStateTransition(new Inactivated(this.Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                this.Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }

            return retVal;
        }
    }

    public class UnderZeroChilling : ActivatePerform
    {
        public UnderZeroChilling(TempController module) : base(module)
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.WaitForChiller;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            double setChillingTemp = 0.0; //Chiller
            double tcSetTemp = 0.0; // TempController의 SetTemp
            double curDewPoint = 0.0;
            double activatableHighTemp = 0.0;

            try
            {
                curDewPoint = Module.EnvControlManager().GetDewPointModule().CurDewPoint;
                tcSetTemp = Module.TempInfo.TargetTemp.Value;
                setChillingTemp = tcSetTemp;
                activatableHighTemp = ChillerModule.ChillerParam.ActivatableHighTemp.Value;

                if (activatableHighTemp < tcSetTemp)
                {
                    ChillerModule.ChillerInfo.TargetTemp = activatableHighTemp;
                    this.Module.InnerStateTransition(new Heatting(this.Module));
                }
                else
                {
                    if (ChillerModule.ChillerInfo.ZeroTemp < tcSetTemp)
                    {
                        this.Module.InnerStateTransition(new Branch(this.Module));
                    }
                    else
                    {
                        setChillingTemp = setChillingTemp / 10;
                    }

                    // Add DewPoint check (floot stages)
                    // Valve Check
                    // Chiller 가 이미 돌고 있었다면, Purge 하고, Chiller 가동.
                    var envModule = Module.EnvControlManager();
                    envModule.SetValveState(false, EnumValveType.IN);
                    envModule.SetValveState(false, EnumValveType.OUT);

                    ChillerModule.Activate();
                    ChillerModule.SetFastChilling(setChillingTemp, TempValueType.TEMPCONTROLLER);

                    this.Module.InnerStateTransition(new WaitForChiller(this.Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                this.Module.InnerStateTransition(new TC_ColdErrorState(this.Module));
            }

            return retVal;
        }
    }

    public class TempInRange : ActivatePerform
    {
        private DateTime InitTime;
        private bool IsSuccessMonitoringMV = false;
        public TempInRange(TempController module) : base(module)
        {
            InitTime = DateTime.Now;
            LoggerManager.Debug($"[CHANGE TEMP END (Temperature In Range)] Cur Temp : {module.TempInfo.CurTemp.Value}, Deviation : {Module.TempController().GetDeviaitionValue()}");
            if (Module.IsOccurTimeOut)
            {
                Module.IsOccurTimeOut = false;
            }
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;
        public override EnumTemperatureState GetState() => EnumTemperatureState.TempInRange;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                // if( Temp Out Put 이 꺼져있다면
                if (Module.TempManager.Get_OutPut_State() == 0)
                {
                    Module.TempManager.Set_OutPut_ON(null); // Heater on
                }

                retVal = ProbeCmdExecutor(); // Same Temp Idle State OP

                if (ChillerModule.GetCommState() != EnumCommunicationState.CONNECTED)
                {
                    retVal = this.Module.InnerStateTransition(new ChillerConnectError(this.Module));

                    return retVal;
                }

                retVal = CheckChillerAbortOp();

                if (retVal == EventCodeEnum.NONE)
                {
                    // ChillerAbortStop State로 Transition된다.
                    return retVal;
                }

                retVal = CheckChillerDifferentTemp();

                if (retVal == EventCodeEnum.NONE)
                {
                    // PauseDifferenceTemp State로 Transition된다.
                    return retVal;
                }

                retVal = BehaviorFollowingProbingModuleState();

                if (retVal == EventCodeEnum.NONE)
                {
                    // TC_ColdMonitoringState State로 Transition된다.
                    return retVal;
                }

                retVal = PreSetTempCompareExcutor();

                if (retVal == EventCodeEnum.NONE)
                {
                    // TC_ColdNomalTriggerState State로 Transition된다.
                    return retVal;
                }

                if (!CheckDewPointMonitoring())
                {
                    retVal = this.Module.InnerStateTransition(new EmergencyPurge(this.Module));

                    return retVal;
                }

                if (!Module.IsCurTempWithinSetTempRange())
                {
                    if (Module.TempInfo.TargetTemp.Value <= ChillerModule.ChillerParam.CoolantInTemp.Value)
                    {
                        this.Module.InnerStateTransition(new WaitForChiller(this.Module));
                    }
                    else
                    {
                        LoggerManager.Debug($"TargetTemp : {Module.TempInfo.TargetTemp.Value}, Chiller CoolantInTemp : ${ChillerModule.ChillerParam.CoolantInTemp.Value}");
                        retVal = Module.InnerStateTransition(new TC_ColdWaitUntilNomalSetTempReached(Module));

                        return retVal;
                    }
                }
                else
                {
                    if (Module.TempInfo.TargetTemp.Value <= ChillerModule.ChillerParam.CoolantInTemp.Value)
                    {
                        var coolantInTemp = ChillerModule.ChillerParam.CoolantInTemp.Value;

                        if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == false | Module.EnvControlManager().GetValveState(EnumValveType.OUT) == false)
                        {
                            if (ChillerModule.GetCommState() == EnumCommunicationState.CONNECTED)
                            {
                                if (ChillerModule.IsMatchedTargetTemp(Module.TempInfo.SetTemp.Value))
                                {
                                    retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.IN);

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                        Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                        return retVal;
                                    }

                                    retVal = Module.EnvControlManager().SetValveState(true, EnumValveType.OUT);

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                        Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                        return retVal;
                                    }
                                }
                            }
                            else
                            {
                                //Loader 와의 연결이 끊긴경우
                                //retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);
                                //retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);
                            }
                        }

                        // Monitoring MV
                        if (IsSuccessMonitoringMV == false)
                        {
                            double monitoringTime = Module.GetMonitoringMVTimeInSec();

                            if (Module.TempInfo.MV.Value == 100.0 && monitoringTime != 0)
                            {
                                DateTime curTime = DateTime.Now;
                                TimeSpan timediff = InitTime - curTime;

                                if (timediff.Seconds > monitoringTime)
                                {
                                    LoggerManager.Debug($"[{this.GetType().Name}] Monitoring MV Error . " +
                                        $"TempInRangeState Init Time : {InitTime}, CurTime : {curTime},  MonitoringMVTimeInSec : {monitoringTime}");
                                    
                                    retVal = this.Module.InnerStateTransition(new EmergencyPurge(this.Module));
                                    return retVal;
                                }
                            }
                            else
                            {
                                IsSuccessMonitoringMV = true;
                            }
                        }
                    }
                    else
                    {
                        if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == true | Module.EnvControlManager().GetValveState(EnumValveType.OUT) == true)
                        {
                            retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                return retVal;
                            }

                            retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true :false ;
                                Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                return retVal;
                            }
                        }
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }

        private EventCodeEnum ProbeCmdExecutor()
        {
            try
            {
                EventCodeEnum executeResult = EventCodeEnum.NONE;
                bool consumed;

                Func<bool> conditionFunc = () => true;
                #region => IChangeTemperatureToSetTemp Command
                Action changeTempToSetTempDoAction = ChangeTempToSetTempDoAction;
                Action changeTempToSetTempAbortAction =
                    () =>
                    {
                        Module.MetroDialogManager().ShowMessageDialog
                        ("[TCIdleState]", "FAIL", EnumMessageStyle.AffirmativeAndNegative);
                        LoggerManager.Debug($"[{this.GetType().Name} - IChangeTempToSetTempCommandExecutor()] => {executeResult.ToString()}");
                    };
                #endregion

                consumed = Module.CommandManager().ProcessIfRequested<IChangeTemperatureToSetTemp>(Module, conditionFunc, changeTempToSetTempDoAction, changeTempToSetTempAbortAction);

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

                consumed = Module.CommandManager().ProcessIfRequested<IChangeTempToSetTempFullReach>(Module, conditionFunc, IChangeTempToSetTempFullReachDoAction, IChangeTempToSetTempFullReachAbortAction);

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

                consumed = Module.CommandManager().ProcessIfRequested<IChangeTemperatureToSetTempWhenWaferTransfer>(Module, conditionFunc, startWaferTransferDoAction, startWaferTransferAbortAction);

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

                consumed = Module.CommandManager().ProcessIfRequested<ISetTempForFrontDoorOpen>(Module, conditionFunc, setTempForFrontDoorOpenDoAction, setTempForFrontDoorOpenAbortAction);

                return executeResult;

                void ChangeTempToSetTempDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdNomalTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempDoAction()] StateTransition to {nameof(TC_ColdNomalTriggerState)} => {executeResult.ToString()}");
                }

                void ChangeTempToSetTempFullReachDoAction()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdWaitingFullReachSetTempTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - ChangeTempToSetTempFullReachDoAction()] StateTransition to {nameof(TC_ColdWaitingFullReachSetTempTriggerState)} => {executeResult.ToString()}");
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

                    LoggerManager.Debug($"[{this.GetType().Name} - StartWaferTransferDoAction()] StateTransition to {Module.InnerState.GetType().Name} => {executeResult.ToString()}");
                }

                void SetTempForFrontDoorOpen()
                {
                    executeResult = Module.InnerStateTransition(new TC_ColdSetTempForFrontDoorOpenReachedTriggerState(Module));
                    LoggerManager.Debug($"[{this.GetType().Name} - SetTempForFrontDoorOpen()] StateTransition to {nameof(TC_ColdSetTempForFrontDoorOpenReachedTriggerState)} => {executeResult.ToString()}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class EmergencyPurge : InactivatePerform
    {
        DateTime purgeStartTime;
        public EmergencyPurge(TempController module) : base(module)
        {
            purgeStartTime = DateTime.Now;
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.Error;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.ERROR;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            long timeToPurge = 15000;
            bool purgeActivated = false;
            try
            {
                //안전온도 설정.
                //Module.SetSVOnlyTC(ChillerModule.ChillerParam.AmbientTemp.Value);
                // if( Temp Out Put 이 꺼져있다면
                if (Module.TempManager.Get_OutPut_State() == 0)
                {
                    Module.TempManager.Set_OutPut_ON(null); // Heater on
                }

                Module.EnvControlManager().SetValveState(false, EnumValveType.IN);
                Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                if (CheckDewPointMonitoring() == false)
                {
                    //Raise Dew Point Error
                    Module.NotifyManager().Notify(EventCodeEnum.DEW_POINT_HIGH_ERR);
                    Module.SetAmbientTemp();
                }

                if (ChillerModule.ChillerParam.IsEnableUsePurge.Value)
                {
                    if (purgeActivated == false)
                    {
                        purgeActivated = true;
                        LoggerManager.Debug($"Emergency Coolant Purge Activated.");
                    }

                    Module.EnvControlManager().SetValveState(true, EnumValveType.DRAIN);
                    Module.EnvControlManager().SetValveState(true, EnumValveType.PURGE);

                    if (DateTime.Now.Subtract(purgeStartTime).TotalMilliseconds >= timeToPurge & purgeActivated == true)
                    {
                        Module.EnvControlManager().SetValveState(false, EnumValveType.DRAIN);
                        Module.EnvControlManager().SetValveState(false, EnumValveType.PURGE);

                        LoggerManager.Debug($"Emergency Coolant Purged for {timeToPurge}ms");
                        this.Module.InnerStateTransition(new Inactivated(this.Module));
                    }
                }
                else
                {
                    Module.MetroDialogManager().ShowMessageDialog("Error Message", "Temp Emgency Error Occured. Please check temperature state.", EnumMessageStyle.Affirmative);
                    this.Module.InnerStateTransition(new Inactivated(this.Module));
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }
    }

    public class ChillerConnectError : ActivatePerform
    {
        bool isOccureDewPointError = false;
        public ChillerConnectError(TempController module) : base(module)
        {
            isOccureDewPointError = false;

        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.ConnectError;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ChillerModule.GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    retVal = Module.InnerStateTransition(new Activated(Module));
                }

                if ((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE)
                {
                    return retVal;
                }

                // 한번만 전송하도록 처리
                if (isOccureDewPointError == false && CheckDewPointMonitoring() == false)
                {
                    //Raise Dew Point Error
                    Module.NotifyManager().Notify(EventCodeEnum.DEW_POINT_HIGH_ERR);
                    isOccureDewPointError = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class ChillerAbortStop : ActivatePerform
    {
        public ChillerAbortStop(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.AbortChiller;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!(ChillerModule.GetCommState() == EnumCommunicationState.CONNECTED))
                {
                    this.Module.InnerStateTransition(new ChillerConnectError(this.Module));
                }

                if ((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE)
                {
                    return retVal;
                }

                if (!ChillerModule.ChillerInfo.AbortChiller)
                {
                    retVal = Module.InnerStateTransition(new Activated(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class PauseDifferenceTemp : ActivatePerform
    {
        public PauseDifferenceTemp(TempController module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EnumTemperatureState GetState() => EnumTemperatureState.PauseDiffTemp;
        public override ChillerProcessType GetChillerState() => ChillerProcessType.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ChillerModule.GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    if (((retVal = PreSetTempCompareExcutor()) == EventCodeEnum.NONE) | ((retVal = CheckChillerAbortOp()) == EventCodeEnum.NONE))
                    {
                        return retVal;
                    }

                    if(ChillerModule.ChillerInfo.SetTemp == -999)
                    {
                        return retVal;
                    }

                    retVal = ChillerModule.CheckCanUseChiller(Module.TempInfo.SetTemp.Value);
                    
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = Module.InnerStateTransition(new Activated(Module));
                        return retVal;
                    }
                    
                    if (ChillerModule.IsMatchedTargetTemp(Module.TempInfo.SetTemp.Value))
                    {
                        retVal = Module.InnerStateTransition(new Activated(Module));
                        return retVal;
                    }
                    else
                    {
                        if (retVal != EventCodeEnum.CHILLER_CHECK_ALREADY_OCCUPY)
                        {
                            if (Module.EnvControlManager().GetValveState(EnumValveType.IN) == true)
                            {
                                retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.IN);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                    Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                    return retVal;
                                }
                            }

                            if (Module.EnvControlManager().GetValveState(EnumValveType.OUT) == true)
                            {
                                retVal = Module.EnvControlManager().SetValveState(false, EnumValveType.OUT);

                                if (retVal != EventCodeEnum.NONE)
                                {
                                    bool CheckConnectState = (retVal == EventCodeEnum.ENVCONTROL_COMM_ERROR) ? true : false;
                                    Module.InnerStateTransition(new TC_ColdErrorState(this.Module, CheckConnectState));
                                    return retVal;
                                }
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[PauseDifferenceTemp] Chiller not connected.");

                    this.Module.InnerStateTransition(new ChillerConnectError(this.Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}

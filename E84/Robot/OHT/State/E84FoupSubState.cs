

namespace E84
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Foup;
    using System;
    using System.Threading;

    public interface IE84BehaviorState
    {
        E84SubStateEnum GetSubStateEnum();
        EventCodeEnum SingleLoad();
        EventCodeEnum SingleUnLoad();
        EventCodeEnum SingleIdleBehavior();
        EventCodeEnum SimultaneousLoad();
        EventCodeEnum SimultaneousUnLoad();
        EventCodeEnum ContinuousLoad();
        EventCodeEnum ContinuousUnLoad();
    }

    public abstract class E84BehavirStateBase : IE84BehaviorState
    {
        protected E84Controller Module;

        public abstract EventCodeEnum SingleLoad();
        public abstract EventCodeEnum SingleUnLoad();
        public virtual EventCodeEnum SingleIdleBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public abstract E84SubStateEnum GetSubStateEnum();

        public E84BehavirStateBase(E84Controller module)
        {
            Module = module;
        }

        public virtual EventCodeEnum SimultaneousLoad()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum SimultaneousUnLoad()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum ContinuousLoad()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum ContinuousUnLoad()
        {
            return EventCodeEnum.NONE;
        }

        protected EventCodeEnum LightCurtainAutoRecovery(bool isValidSignal)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool foupPlacementFlag = Module.GetClampSignal();

                if (isValidSignal)
                {
                    Module.ClearEvent();

                    if (Module.GetSignal(E84SignalTypeEnum.HO_AVBL) == true && Module.GetSignal(E84SignalTypeEnum.ES) == true)
                    {
                        LoggerManager.Debug($"[E84] {Module.E84ModuleParaemter.E84OPModuleType}.PORT{Module.E84ModuleParaemter.FoupIndex} LightCurtainAutoRecovery(): ClearState.");

                        Module.ClearState();

                        if (Module.FoupController.FoupModuleInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                        {
                            Module.FoupController.ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_UNLOAD);
                        }
                        else if (Module.FoupController.FoupModuleInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_DETACH)
                        {
                            Module.FoupController.ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);
                        }
                    }

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"[E84] {Module.E84ModuleParaemter.E84OPModuleType}.PORT{Module.E84ModuleParaemter.FoupIndex} LightCurtainAutoRecovery(): ClearState.");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class E84SingleReadyState : E84BehavirStateBase
    {
        public E84SingleReadyState(E84Controller module) : base(module)
        {
        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.READY;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                /// <condition>
                /// [IN]CS_0 : true, [IN]VALID : true
                /// </condition> 

                bool isOnCS0 = Module.GetSignal(E84SignalTypeEnum.CS_0);
                bool isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);

                if (isOnCS0 && isOnValid)
                {
                    Module.ClearCurErrorParam();

                    if (Module.GetSignal(E84SignalTypeEnum.BUSY) == false && Module.IsFoupPresenceCarrier())
                    {
                        ///Handshake Error (이재작업이 시작된 상태가 아닌 타이밍에 Presence 가 들어온 상황)
                        if (Module.GetInputSignalComm(E84SignalTypeEnum.BUSY) == false)
                        {
                            Module.E84ErrorOccured(code: E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE);
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        if (Module.GetSignal(E84SignalTypeEnum.L_REQ) == false)
                        {
                            retVal = Module.SetSignal(E84SignalTypeEnum.L_REQ, true);
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }

                        Module.E84BehaviorStateTransition(new E84SingleCSSpecifedState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SingleUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                /// <condition>
                /// [IN]CS_0 : true, [IN]VALID : true
                /// </condition> 

                bool isOnCS0 = Module.GetSignal(E84SignalTypeEnum.CS_0);
                bool isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);

                if (isOnCS0 && isOnValid)
                {
                    Module.ClearCurErrorParam();

                    if (Module.GetSignal(E84SignalTypeEnum.U_REQ) == false)
                    {
                        retVal = Module.SetSignal(E84SignalTypeEnum.U_REQ, true);
                    }

                    Module.E84BehaviorStateTransition(new E84SingleCSSpecifedState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class E84SingleCSSpecifedState : E84BehavirStateBase
    {
        public E84SingleCSSpecifedState(E84Controller module) : base(module)
        {
        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.CS_SPECIFIED;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                /// <condition>
                /// [OUT]L_REQ : true, [IN]TR_REQ : true, [OUT]READY : true, [IN]BUSY : true
                /// </condition>
                bool isOnLReq = false;
                bool isOnTrReq = false;
                bool isOnReady = false;
                bool isOnBusy = false;
                bool isOnPresence = false;

                bool isOnCS0 = Module.GetSignal(E84SignalTypeEnum.CS_0);
                bool isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);

                isOnPresence = (Module.FoupController?.GetFoupModuleInfo()?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_ATTACH ? true : false;

                //L_REQ 신호 확인
                //카세트 없는지 확인.
                isOnLReq = Module.GetSignal(E84SignalTypeEnum.L_REQ);

                if (isOnLReq == false)
                {
                    if ((Module.FoupController?.GetFoupModuleInfo()?.State ?? FoupStateEnum.UNDEFIND) == FoupStateEnum.EMPTY_CASSETTE)
                    {
                        retVal = Module.SetSignal(E84SignalTypeEnum.L_REQ, true);
                    }

                    if (isOnCS0 == false || isOnValid == false)
                    {
                        //#Hynix_Merge: 검토 필요, V20은 아래와 같이 되어있었음. 그렇다고 해서 HO_AVAL_OFF_SEQUENCE_ERROR가 맞는건 아닌듯..
                        LoggerManager.Error($"[E84] {Module.E84ModuleParaemter.E84OPModuleType}.PORT{Module.E84ModuleParaemter.FoupIndex} SingleLoad(): CS0 and Valid is off");
                        Module.E84ErrorOccured(code: E84EventCode.HO_AVAL_OFF_SEQUENCE_ERROR);

                        //V20은 아래와 같이 되어있었음. 
                        //LoggerManager.Error($"SingleLoad(): CS0 and Valid is off");
                        //Module.SetSignal(E84SignalTypeEnum.L_REQ, false);
                        //Module.SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                        //=======================
                    }
                }
                else
                {
                    if (Module.GetSignal(E84SignalTypeEnum.BUSY) == false && isOnPresence == true)
                    {
                        retVal = EventCodeEnum.E84_SEQUENCE_ERROR;
                    }

                    retVal = EventCodeEnum.NONE;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = EventCodeEnum.UNDEFINED;

                    //TR_REQ 신호 true 인지 확인.
                    isOnTrReq = Module.GetSignal(E84SignalTypeEnum.TR_REQ);

                    //READY 신호 확인
                    //foup load 준비 되었는지 확인.
                    isOnReady = Module.GetSignal(E84SignalTypeEnum.READY);

                    if (isOnTrReq)
                    {
                        isOnPresence = (Module.FoupController?.GetFoupModuleInfo()?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_ATTACH ? true : false;

                        if (isOnReady == false)
                        {
                            if (Module.GetSignal(E84SignalTypeEnum.BUSY) == false && isOnPresence == true)
                            {
                                retVal = EventCodeEnum.E84_SEQUENCE_ERROR;
                            }
                            else
                            {
                                if (Module.GetSignal(E84SignalTypeEnum.READY) == false)
                                {
                                    retVal = Module.SetSignal(E84SignalTypeEnum.READY, true);
                                }
                            }
                        }
                        else if (isOnReady == true)
                        {
                            if (Module.GetSignal(E84SignalTypeEnum.BUSY) == false && isOnPresence == true)
                            {
                                retVal = EventCodeEnum.E84_SEQUENCE_ERROR;
                            }
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    isOnBusy = Module.GetSignal(E84SignalTypeEnum.BUSY);

                    if (isOnBusy)
                    {
                        if (Module.E84Module().GetIsCDBypass())
                        {
                            var delayTime = Module.E84Module().GetCDBypassDelayTimeInSec();

                            if (delayTime > 0)
                            {
                                int delayTimeInMS = (int)(delayTime * 1000.0);
                                Thread.Sleep(delayTimeInMS);
                                retVal = Module.SetSignal(E84SignalTypeEnum.L_REQ, false);
                                Module.E84BehaviorStateTransition(new E84SingleCarrierDetetedState(Module));
                            }
                        }
                        else
                        {
                            Module.E84BehaviorStateTransition(new E84SingleTransferStartState(Module));
                        }
                    }
                }

                if (retVal == EventCodeEnum.E84_SEQUENCE_ERROR)
                {
                    if (Module.GetSignal(E84SignalTypeEnum.BUSY) == false && isOnPresence == true)
                    {
                        if (Module.GetInputSignalComm(E84SignalTypeEnum.BUSY) == false)
                        {
                            if (isOnReady == true)
                            {
                                retVal = Module.SetSignal(E84SignalTypeEnum.READY, false);
                            }

                            Module.SetEventCode(E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE);
                            Module.E84ErrorOccured(code: E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE);
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

        public override EventCodeEnum SingleUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                /// <condition>
                /// [OUT]U_REQ : true, [IN]TR_REQ : true, [OUT]READY : true, [IN]BUSY : true
                /// </condition> 
                bool isOnUReq = false;
                bool isOnTrReq = false;
                bool isOnReady = false;
                bool isOnBusy = false;

                bool isOnCS0 = Module.GetSignal(E84SignalTypeEnum.CS_0);
                bool isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);

                //U_REQ 신호 확인
                //카세트 없는지 확인.
                isOnUReq = Module.GetSignal(E84SignalTypeEnum.U_REQ);

                if (isOnUReq == false)
                {
                    if ((Module.FoupController?.GetFoupModuleInfo()?.State ?? FoupStateEnum.UNDEFIND) == FoupStateEnum.EMPTY_CASSETTE)
                    {
                        retVal = Module.SetSignal(E84SignalTypeEnum.U_REQ, true);

                        if (isOnCS0 == false || isOnValid == false)
                        {
                            LoggerManager.Error($"SingleUnLoad(): CS0 and Valid is off");
                            Module.E84ErrorOccured(code: E84EventCode.HO_AVAL_OFF_SEQUENCE_ERROR);
                        }

                    }

                    if (isOnCS0 == false || isOnValid == false)
                    {
                        LoggerManager.Error($"SingleUnLoad(): CS0 and Valid is off");
                        Module.E84ErrorOccured(code: E84EventCode.HO_AVAL_OFF_SEQUENCE_ERROR);
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = EventCodeEnum.UNDEFINED;

                    //TR_REQ 신호 true 인지 확인.
                    isOnTrReq = Module.GetSignal(E84SignalTypeEnum.TR_REQ);

                    //READY 신호 확인
                    //foup load 준비 되었는지 확인.
                    isOnReady = Module.GetSignal(E84SignalTypeEnum.READY);

                    if (isOnTrReq)
                    {
                        if (isOnReady == false)
                        {
                            retVal = Module.SetSignal(E84SignalTypeEnum.READY, true);
                        }
                        else if (isOnReady == true)
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    isOnBusy = Module.GetSignal(E84SignalTypeEnum.BUSY);

                    if (isOnBusy)
                    {
                        if (Module.E84Module().GetIsCDBypass())
                        {
                            var delayTime = Module.E84Module().GetCDBypassDelayTimeInSec();

                            if (delayTime > 0)
                            {
                                int delayTimeInMS = (int)(delayTime * 1000.0);
                                Thread.Sleep(delayTimeInMS);
                                retVal = Module.SetSignal(E84SignalTypeEnum.U_REQ, false);
                                Module.E84BehaviorStateTransition(new E84SingleCarrierDetetedState(Module));
                            }
                        }
                        else
                        {
                            Module.E84BehaviorStateTransition(new E84SingleTransferStartState(Module));
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
    }

    public class E84SingleTransferStartState : E84BehavirStateBase
    {
        public E84SingleTransferStartState(E84Controller module) : base(module)
        {
        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.TRANSFET_START;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                /// <condition>
                ///  [OUT]L_REQ : false
                /// </condition> 
                // 캐리어가 감지되면 L_REQ 신호를 끈다.
                bool isOnLReq = Module.GetSignal(E84SignalTypeEnum.L_REQ);

                if (isOnLReq)
                {
                    if ((Module.FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_ATTACH)
                    {
                        Module.SetCarrierState();
                        retVal = Module.SetSignal(E84SignalTypeEnum.L_REQ, false);

                        Module.E84BehaviorStateTransition(new E84SingleCarrierDetetedState(Module));
                    }
                }
                else
                {
                    if (Module.GetCarrierState() == true && (Module.FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_ATTACH)
                    {
                        Module.E84BehaviorStateTransition(new E84SingleCarrierDetetedState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SingleUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                /// <condition>
                ///  [OUT]U_REQ : false
                /// </condition> 

                // 캐리어가 없어지면 U_REQ 신호를 끈다.
                bool isOnLReq = Module.GetSignal(E84SignalTypeEnum.U_REQ);

                if (isOnLReq)
                {
                    if ((Module.FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_DETACH)
                    {
                        Module.SetCarrierState();
                        retVal = Module.SetSignal(E84SignalTypeEnum.U_REQ, false);

                        Module.E84BehaviorStateTransition(new E84SingleCarrierDetetedState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class E84SingleCarrierDetetedState : E84BehavirStateBase
    {
        public E84SingleCarrierDetetedState(E84Controller module) : base(module)
        {

        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.CARRIER_DETECTED;

        private EventCodeEnum HandlePresenceType(bool isOnBusy, bool isOnTrReq, bool isOnCompt, bool isOnCs, bool isOnValid)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if ((Module.FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_ATTACH)
                {
                    if (isOnBusy == false)
                    {
                        bool cassetteLockFlag = Module.GetClampSignal();

                        var lockFlag = Module.FoupController.FoupModuleInfo.IsCassetteAutoLock;

                        if (lockFlag)
                        {
                            // PRESENCE 켜진 상태 일때 Autolock 이고, ClampLock 은 안된경우
                            if (cassetteLockFlag == false)
                            {
                                Module.E84ErrorOccured(code: E84EventCode.SENSOR_ERROR_LOAD_ONLY_PRESENCS);

                                return retval;
                            }
                        }
                    }
                    else
                    {
                        bool cassetteLockFlag = Module.GetClampSignal();

                        if (cassetteLockFlag)
                        {
                            Module.E84ErrorOccured(code: E84EventCode.CLAMP_ON_BEFORE_OFF_BUSY);

                            return retval;
                        }
                    }

                    retval = HandleTransferCompletion(isOnBusy, isOnTrReq, isOnCompt, isOnCs, isOnValid, E84SubSteps.DONE_LOADING);
                }
                else if ((Module.FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_DETACH)
                {
                    //PLANCEMENT는 꺼졌는데  PRESESNCE 만 켜져있는 경우
                    bool cassetteLockFlag = Module.GetClampSignal();

                    if (cassetteLockFlag)
                    {
                        Module.E84ErrorOccured(code: E84EventCode.SENSOR_ERROR_LOAD_ONLY_PLANCEMENT);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum HandleExistType(bool isOnBusy, bool isOnTrReq, bool isOnCompt, bool isOnCs, bool isOnValid)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (isOnBusy == false)
                {
                    bool useExist = false;

                    //PLANCEMENT는 꺼졌는데  PRESESNCE 만 켜져있는 경우 (Exist 센서만 들어온 경우)
                    if (Module.FoupController.GetFoupIO().IOMap.Inputs.DI_CST_Exists != null)
                    {
                        if (Module.FoupController.GetFoupIO().IOMap.Inputs.DI_CST_Exists.Count >= Module.E84ModuleParaemter.FoupIndex)
                        {
                            if (Module.FoupController.GetFoupIO().IOMap.Inputs.DI_CST_Exists[Module.E84ModuleParaemter.FoupIndex - 1].IOOveride.Value == EnumIOOverride.NONE)
                            {
                                useExist = true;
                            }
                        }
                    }

                    if (useExist)
                    {
                        bool value;
                        int ret = Module.FoupController.GetFoupIO().ReadBit(Module.FoupController.GetFoupIO().IOMap.Inputs.DI_CST_Exists[Module.E84ModuleParaemter.FoupIndex - 1], out value);

                        if (ret == 0)
                        {
                            if (value)
                            {
                                //PLACEMENT is OFF
                                if ((Module.FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) != FoupPRESENCEStateEnum.CST_ATTACH)
                                {
                                    Thread.Sleep((int)Module.E84Module().GetTimeoutOnPresenceAfterOnExistSensor());

                                    if ((Module.FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) != FoupPRESENCEStateEnum.CST_ATTACH)
                                    {
                                        Module.E84ErrorOccured(code: E84EventCode.SENSOR_ERROR_LOAD_ONLY_PRESENCS);

                                        return retval;
                                    }
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}], HandleExistType(), DI_CST_Exists, ret = {ret}");
                        }
                    }
                    else
                    {
                        bool value;
                        int ret = Module.FoupController.GetFoupIO().ReadBit(Module.FoupController.GetFoupIO().IOMap.Inputs.DI_C12IN_PLACEMENT, out value);

                        if (ret == 0)
                        {
                            //PRESESNCE 꺼졌는데 PLANCEMENT는만 켜져있는 경우(Presence 만 눌린 경우)
                            if (value)
                            {
                                //Exist is OFF
                                if (useExist)
                                {
                                    if (Module.FoupController.GetFoupIO().MonitorForIO(Module.FoupController.GetFoupIO().IOMap.Inputs.DI_CST_Exists[Module.E84ModuleParaemter.FoupIndex - 1], true, 100, Module.E84Module().GetTimeoutOnPresenceAfterOnExistSensor()) == -1)
                                    {
                                        Module.E84ErrorOccured(code: E84EventCode.SENSOR_ERROR_LOAD_ONLY_PLANCEMENT);
                                    }
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}], HandleExistType(), DI_C12IN_PLACEMENT, ret = {ret}");
                        }
                    }
                }
                else
                {
                    bool cassetteLockFlag = Module.GetClampSignal();

                    if (cassetteLockFlag)
                    {
                        Module.E84ErrorOccured(code: E84EventCode.CLAMP_ON_BEFORE_OFF_BUSY);

                        return retval;
                    }
                }

                retval = HandleTransferCompletion(isOnBusy, isOnTrReq, isOnCompt, isOnCs, isOnValid, E84SubSteps.DONE_LOADING);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                /// <condition>
                ///  [IN]BUSY : false, [IN]TR_REQ : false, [IN]COMPT : true
                /// </condition> 

                bool isOnBusy = Module.GetSignal(E84SignalTypeEnum.BUSY);
                bool isOnTrReq = Module.GetSignal(E84SignalTypeEnum.TR_REQ);
                bool isOnCompt = Module.GetSignal(E84SignalTypeEnum.COMPT);
                bool isOnCs = Module.GetSignal(E84SignalTypeEnum.CS_0);
                bool isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);

                E84PresenceTypeEnum e84PresenceTypeEnum = Module.E84Module().GetE84PreseceType();

                if (e84PresenceTypeEnum == E84PresenceTypeEnum.PRESENCE)
                {
                    retVal = HandlePresenceType(isOnBusy, isOnTrReq, isOnCompt, isOnCs, isOnValid);
                }
                else if (e84PresenceTypeEnum == E84PresenceTypeEnum.EXIST)
                {
                    retVal = HandleExistType(isOnBusy, isOnTrReq, isOnCompt, isOnCs, isOnValid);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SingleUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                /// <condition>
                ///  [IN]BUSY : false, [IN]TR_REQ : false, [IN]COMPT : true
                /// </condition> 
                if ((Module.FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_DETACH)
                {
                    bool isOnBusy = Module.GetSignal(E84SignalTypeEnum.BUSY);
                    bool isOnTrReq = Module.GetSignal(E84SignalTypeEnum.TR_REQ);
                    bool isOnCompt = Module.GetSignal(E84SignalTypeEnum.COMPT);
                    bool isOnCs = Module.GetSignal(E84SignalTypeEnum.CS_0);
                    bool isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);

                    if (isOnBusy == false)
                    {
                        bool cassetteLockFlag = Module.GetClampSignal();

                        if (cassetteLockFlag)
                        {
                            //PRESESNCE 는 꺼졌는데 PLANCEMENT 만 켜져있는 경우
                            Module.E84ErrorOccured(code: E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PLANCEMENT);

                            return retVal;
                        }
                    }

                    if (Module.IsFoupPresenceCarrier())
                    {
                        bool cassetteLockFlag = Module.GetClampSignal();

                        if (cassetteLockFlag == false)
                        {
                            //PLANCEMENT는 꺼졌는데  PRESESNCE 만 켜져있는 경우
                            Module.E84ErrorOccured(code: E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PRESENCS);

                            return retVal;
                        }
                    }

                    retVal = HandleTransferCompletion(isOnBusy, isOnTrReq, isOnCompt, isOnCs, isOnValid, E84SubSteps.DONE_UNLOADING);
                }
                else if (Module.IsFoupPresenceCarrier())
                {
                    bool cassetteLockFlag = Module.GetClampSignal();

                    if (cassetteLockFlag == false)
                    {
                        //PLANCEMENT는 꺼졌는데  PRESESNCE 만 켜져있는 경우
                        Module.E84ErrorOccured(code: E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PRESENCS);

                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private bool CheckConditions(bool isOnBusy, bool isOnTrReq, bool isOnCompt, bool isOnCs, bool isOnValid, E84SubSteps subStep)
        {
            bool retval = false;

            try
            {
                retval = (!isOnBusy && !isOnTrReq && isOnCompt) || (!isOnBusy && !isOnTrReq && !isOnCompt && !isOnCs && !isOnValid) || Module.GetCommModuleSubStep() == subStep;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum HandleTransferCompletion(bool isOnBusy, bool isOnTrReq, bool isOnCompt, bool isOnCs, bool isOnValid, E84SubSteps subStep)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (CheckConditions(isOnBusy, isOnTrReq, isOnCompt, isOnCs, isOnValid, subStep))
                {
                    if (Module.GetCommModuleSubStep() != subStep)
                    {
                        if (Module.CurErrorParam.ErrorAct == E84ErrorActEnum.ERROR_Warning)
                        {
                            LoggerManager.Debug($"E84SingleCarrierDetetedState.ClearEvent");
                            Module.ClearEvent();
                        }
                    }

                    Module.E84BehaviorStateTransition(new E84SingleTransferDoneState(Module));
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class E84SingleTransferDoneState : E84BehavirStateBase
    {
        public E84SingleTransferDoneState(E84Controller module) : base(module)
        {
        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.TRANSFER_DONE;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                /// <condition>
                ///  [OUT]READY : false, [IN] VALID : false, [IN] COMPT : false, [IN] CS_0 : false, 
                /// </condition> 

                bool isOnReady = Module.GetSignal(E84SignalTypeEnum.READY);
                bool isOnValid = true;
                bool isOnCompt = true;
                bool isonCS0 = true;

                if (isOnReady)
                {
                    retVal = Module.SetSignal(E84SignalTypeEnum.READY, false);
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);
                    isOnCompt = Module.GetSignal(E84SignalTypeEnum.COMPT);
                    isonCS0 = Module.GetSignal(E84SignalTypeEnum.CS_0);

                    if ((!isOnValid && !isOnCompt && !isonCS0) || Module.GetCommModuleSubStep() == E84SubSteps.DONE_LOADING)
                    {
                        Module.E84BehaviorStateTransition(new E84SingleCSReleaseState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SingleUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                /// <condition>
                ///  [OUT]READY : false, [IN] VALID : false, [IN] COMPT : false, [IN] CS_0 : false, 
                /// </condition> 

                bool isOnReady = Module.GetSignal(E84SignalTypeEnum.READY);
                bool isOnValid = true;
                bool isOnCompt = true;
                bool isonCS0 = true;

                if (isOnReady)
                {
                    retVal = Module.SetSignal(E84SignalTypeEnum.READY, false);
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);
                    isOnCompt = Module.GetSignal(E84SignalTypeEnum.COMPT);
                    isonCS0 = Module.GetSignal(E84SignalTypeEnum.CS_0);

                    if ((!isOnValid && !isOnCompt && !isonCS0) || Module.GetCommModuleSubStep() == E84SubSteps.DONE_UNLOADING)
                    {
                        Module.E84BehaviorStateTransition(new E84SingleCSReleaseState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class E84SingleCSReleaseState : E84BehavirStateBase
    {
        public E84SingleCSReleaseState(E84Controller module) : base(module)
        {
        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.CS_RELEASE;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.GetCommModuleSubStep() != E84SubSteps.DONE_LOADING)
                {
                    if (Module.CurErrorParam.ErrorAct == E84ErrorActEnum.ERROR_Warning)
                    {
                        //  정상적으로 종료되지 않았을 경우 donr_loading 상태가 안되어있음. 
                        if (Module.GetSignal(E84SignalTypeEnum.READY) == false &&
                            Module.GetSignal(E84SignalTypeEnum.CS_0) == false &&
                            Module.GetSignal(E84SignalTypeEnum.VALID) == false &&
                            Module.GetSignal(E84SignalTypeEnum.COMPT) == false)
                        {
                            // oht에서 모든 신호가 다 꺼졌으면 정상처리 하기위해서 clearevent를 불러줌. 
                            // E84Module에서 input 신호가 하나라도 true인게 있으면 clearevent 실패하고 70 Error 내보냄.
                            LoggerManager.Debug($"E84SingleCSReleaseState.ClearEvent");
                            Module.ClearEvent();
                        }
                    }
                }

                if (Module.FoupController.FoupModuleInfo.IsCassetteAutoLockLeftOHT)
                {
                    Module.FoupController.Execute(new FoupDockingPlateLockCommand());
                }

                Module.E84BehaviorStateTransition(new E84SingleDoneState(Module));

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SingleUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.GetCommModuleSubStep() != E84SubSteps.DONE_UNLOADING)
                {
                    //  정상적으로 종료되지 않았을 경우 donr_unloading 상태가 안되어있음. 
                    if (Module.GetSignal(E84SignalTypeEnum.READY) == false &&
                        Module.GetSignal(E84SignalTypeEnum.CS_0) == false &&
                        Module.GetSignal(E84SignalTypeEnum.VALID) == false &&
                        Module.GetSignal(E84SignalTypeEnum.COMPT) == false)
                    {
                        // oht에서 모든 신호가 다 꺼졌으면 정상처리 하기위해서 clearevent를 불러줌. 
                        // E84Module에서 input 신호가 하나라도 true인게 있으면 clearevent 실패하고 70 Error 내보냄.
                        LoggerManager.Debug($"E84SingleCSReleaseState.ClearEvent");
                        Module.ClearEvent();
                    }
                }

                Module.E84BehaviorStateTransition(new E84SingleDoneState(Module));

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


    public class E84SingleDoneState : E84BehavirStateBase
    {
        public E84SingleDoneState(E84Controller module) : base(module)
        {
        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.DONE;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //if(Module.GetClampSignal() == true)
                //{
                //    // clamp lock option 이 ATTACH 일 경우, clamp lock 은 되었지만, E84 시퀀스 동안은 HO은 off 하지못했기에 끝나고 HO 를 off 해준다. 
                //    if(Module.GetSignal(E84SignalTypeEnum.HO_AVBL) == true)
                //    {
                //        Module.SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                //    }
                //}
                Module.SetCarrierState();

                Module.E84BehaviorStateTransition(new E84SingleReadyState(Module));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SingleUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.E84BehaviorStateTransition(new E84SingleReadyState(Module));

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class E84AutoRecoveryState : E84BehavirStateBase
    {
        public E84AutoRecoveryState(E84Controller module) : base(module)
        {
        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.AUTO_RECOVERY;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var foupstate = Module.GEMModule().GetPIVContainer().GetFoupState(Module.E84ModuleParaemter.FoupIndex);

                if ((foupstate == GEMFoupStateEnum.READY_TO_LOAD || foupstate == GEMFoupStateEnum.READY_TO_UNLOAD) == false)
                {
                    //skip
                    Thread.Sleep(50);
                    return EventCodeEnum.NONE;
                }

                if (Module.GetEventCode() == E84EventCode.TP1_TIMEOUT ||
                    Module.GetEventCode() == E84EventCode.TP2_TIMEOUT ||
                    Module.GetEventCode() == E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE)
                {
                    bool foupPlacementFlag = Module.GetClampSignal();
                    bool foupPresenceFlag = (Module.FoupController?.GetFoupModuleInfo()?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_DETACH ? true : false;

                    if (CanRecoverySignal() && foupPlacementFlag == false && foupPresenceFlag == false)
                    {
                        if (Module.GetSignal(E84SignalTypeEnum.HO_AVBL) == false)
                        {
                            retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, true);
                        }

                        Module.ClearState();
                        Module.ClearEvent();

                        Module.FoupController.ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);

                        retVal = EventCodeEnum.NONE;
                    }

                }
                else if (Module.GetEventCode() == E84EventCode.TP5_TIMEOUT)
                {
                    bool foupPresenceFlag = (Module.FoupController?.GetFoupModuleInfo()?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_ATTACH ? true : false;

                    if (CanRecoverySignal() && foupPresenceFlag == true)
                    {
                        if (Module.GetSignal(E84SignalTypeEnum.HO_AVBL) == false)
                        {
                            retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, true);
                        }

                        Module.ClearState();
                        Module.ClearEvent();

                        Module.FoupController.ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);

                        Module.E84BehaviorStateTransition(new E84SingleTransferDoneState(Module));

                        retVal = EventCodeEnum.NONE;
                    }
                }
                else if (Module.GetEventCode() == E84EventCode.LIGHT_CURTAIN_ERROR)
                {
                    LightCurtainAutoRecovery(CanRecoverySignal());
                }
                else
                {
                    LoggerManager.Debug($"[E84] {Module.E84ModuleParaemter.E84OPModuleType}.PORT{Module.E84ModuleParaemter.FoupIndex} Cannot Recovery. change to error state.");

                    Module.InnerStateTransition(new E84FoupErrorState(Module));
                    Module.E84BehaviorStateTransition(new E84SingleReadyState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SingleUnLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var foupstate = Module.GEMModule().GetPIVContainer().GetFoupState(Module.E84ModuleParaemter.FoupIndex);

                if ((foupstate == GEMFoupStateEnum.READY_TO_LOAD || foupstate == GEMFoupStateEnum.READY_TO_UNLOAD) == false)
                {
                    //skip
                    Thread.Sleep(50);
                    return EventCodeEnum.NONE;
                }

                if (Module.GetEventCode() == E84EventCode.TP1_TIMEOUT ||
                    Module.GetEventCode() == E84EventCode.TP2_TIMEOUT ||
                    Module.GetEventCode() == E84EventCode.TP5_TIMEOUT ||
                    Module.GetEventCode() == E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE)
                {
                    bool foupPlacementFlag = Module.GetClampSignal();
                    bool foupPresenceFlag = (Module.FoupController?.GetFoupModuleInfo()?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_ATTACH ? true : false;

                    if (CanRecoverySignal() && foupPlacementFlag == false && foupPresenceFlag == true)
                    {
                        retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, true);

                        Module.ClearState();
                        Module.ClearEvent();

                        Module.FoupController.ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_UNLOAD);

                        retVal = EventCodeEnum.NONE;
                    }
                }
                else if (Module.GetEventCode() == E84EventCode.TP5_TIMEOUT)
                {
                    bool foupPlacementFlag = Module.GetClampSignal();
                    bool foupPresenceFlag = (Module.FoupController?.GetFoupModuleInfo()?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_DETACH ? true : false;

                    if (CanRecoverySignal() && foupPlacementFlag == false && foupPresenceFlag == false)
                    {
                        if (Module.GetSignal(E84SignalTypeEnum.HO_AVBL) == false)
                        {
                            retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, true);
                        }

                        Module.ClearState();
                        Module.ClearEvent();

                        Module.FoupController.ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);
                        Module.E84BehaviorStateTransition(new E84SingleTransferDoneState(Module));

                        retVal = EventCodeEnum.NONE;
                    }

                }
                else if (Module.GetEventCode() == E84EventCode.LIGHT_CURTAIN_ERROR)
                {
                    LightCurtainAutoRecovery(CanRecoverySignal());
                }
                else
                {
                    LoggerManager.Debug($"[E84] {Module.E84ModuleParaemter.E84OPModuleType}.PORT{Module.E84ModuleParaemter.FoupIndex} Cannot Recovery. change to error state.");
                    Module.InnerStateTransition(new E84FoupErrorState(Module));
                    Module.E84BehaviorStateTransition(new E84SingleReadyState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SingleIdleBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var foupstate = Module.GEMModule().GetPIVContainer().GetFoupState(Module.E84ModuleParaemter.FoupIndex);

                if ((foupstate == GEMFoupStateEnum.READY_TO_LOAD || foupstate == GEMFoupStateEnum.READY_TO_UNLOAD) == false)
                {
                    //skip
                    Thread.Sleep(50);
                    return EventCodeEnum.NONE;
                }

                if (Module.GetEventCode() == E84EventCode.LIGHT_CURTAIN_ERROR)
                {
                    LightCurtainAutoRecovery(CanRecoverySignal());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private bool CanRecoverySignal()
        {
            bool retVal = false;

            try
            {
                //OHT가 떠난 후 (Go 신호 OFF 또는 CS_0 신호 OFF), 모든 FOUP 센서가 정상 상태 (배치 및 존재 센서 모두 켜짐 또는 배치 및 존재 센서 모두 꺼짐),
                //클램프 신호 꺼짐, 라이트 커튼 신호 꺼짐. 그런 다음 컴퓨터 소프트웨어 (PC) 및 E84 컨트롤러는 3 ~ 5 초 이내에 HO_AVBL 신호를 켜야합니다.
                //로드 포트를 즉시 '로드 준비 완료'또는 '언로드 준비 완료'상태로 되돌립니다. ("자동 복구")
                //(BUSY 신호가 ON 일 때 Light Curtain이 ON되면 "Auto Recovery"를 허용하지 않음, 사용자가 GUI에서 Recover 명령을 보내야 함)
                //자동 모드 'Ready to Load'또는 'Ready to Unload'대기 상태(OHT 아직 도착하지 않음)가되면 라이트 커튼이 켜지고 HO_AVBL은 즉시 꺼져 야합니다.
                //라이트 커튼이 꺼지면 클램프 신호가 꺼지고 모든 FOUP 센서가 정상 상태(배치 및 존재 센서 모두 켜짐 또는 배치 및 감지 센서 모두 꺼짐)가됩니다.
                //그런 다음 컴퓨터 소프트웨어(PC) 및 E84 컨트롤러는 3~5 초 이내에 HO_AVBL 신호를 켜야합니다. 로드 포트를 즉시 '로드 준비 완료'또는 '언로드 준비 완료'상태로 되돌립니다. ("자동 복구")

                bool isOnGoSignal = Module.GetSignal(E84SignalTypeEnum.GO);
                bool isOnCS0Signal = Module.GetSignal(E84SignalTypeEnum.CS_0);

                bool isOnclampSignal = false;

                if (Module.FoupController.GetFoupModuleInfo().DockingPlateState == DockingPlateStateEnum.LOCK)
                {
                    isOnclampSignal = true;
                }
                else if (Module.FoupController.GetFoupModuleInfo().DockingPlateState == DockingPlateStateEnum.UNLOCK)
                {
                    isOnclampSignal = false;
                }

                bool isOnLightCurtain = Module.GetLightCurtainSignal();

                if (isOnGoSignal == false && isOnCS0Signal == false && isOnclampSignal == false && isOnLightCurtain == false)
                {
                    retVal = true;
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

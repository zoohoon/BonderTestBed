namespace E84.CardAGV.State
{
    using LogModule;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Event;
    using System;
    using System.Threading;


    public abstract class E84CardAGVBehavirStateBase : IE84BehaviorState
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

        public E84CardAGVBehavirStateBase(E84Controller module)
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

    }

    public class E84CardAGVSingleReadyState : E84CardAGVBehavirStateBase
    {
        public E84CardAGVSingleReadyState(E84Controller module) : base(module)
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
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    Module.EventManager().RaisingEvent(typeof(CardBusyEvent).FullName, new ProbeEventArgs(this, semaphore, new PIVInfo(stagenumber: Module.GetCCTargetStageIndex())));
                    semaphore.Wait();

                    if (Module.GetSignal(E84SignalTypeEnum.BUSY) == false && Module.CanPutCardBuffer() == false)
                    {
                        ///Handshake Error (이재작업이 시작된 상태가 아닌 타이밍에 Exist 가 들어온 상황)
                        retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                        Module.E84ErrorOccured(code: E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE);
                    }
                    else
                    {
                        retVal = Module.SetSignal(E84SignalTypeEnum.L_REQ, true);
                        Module.E84BehaviorStateTransition(new E84CardAGVSingleCSSpecifedState(Module));
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
                    if (Module.IsCardPresenceInBuffer() == true)
                    {
                        retVal = Module.SetSignal(E84SignalTypeEnum.U_REQ, true);
                        Module.E84BehaviorStateTransition(new E84CardAGVSingleCSSpecifedState(Module));
                    }
                    else
                    {
                        LoggerManager.Debug("E84AGVSingleSingleReadyState Error. SingleUnload() CardPresenceInBuffer is false");
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

    public class E84CardAGVSingleCSSpecifedState : E84CardAGVBehavirStateBase
    {
        public E84CardAGVSingleCSSpecifedState(E84Controller module) : base(module)
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
                bool canLoad = false;

                bool isOnCS0 = Module.GetSignal(E84SignalTypeEnum.CS_0);
                bool isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);

                canLoad = Module.CanPutCardBuffer();
                //L_REQ 신호 확인
                //카드 없는지 확인.
                isOnLReq = Module.GetSignal(E84SignalTypeEnum.L_REQ);
                if (isOnLReq == false)
                {
                    if (canLoad == true)
                    {
                        retVal = Module.SetSignal(E84SignalTypeEnum.L_REQ, true);

                        if (isOnCS0 == false || isOnValid == false)
                        {                            
                            LoggerManager.Error($"SingleLoad(): CS0 and Valid is off");
                            retVal = Module.SetSignal(E84SignalTypeEnum.L_REQ, false);
                            retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                        }
                    }
                }
                else
                {
                    if (canLoad == false)
                    {
                        retVal = Module.SetSignal(E84SignalTypeEnum.L_REQ, false);
                        retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = EventCodeEnum.UNDEFINED;
                    //TR_REQ 신호 true 인지 확인.
                    isOnTrReq = Module.GetSignal(E84SignalTypeEnum.TR_REQ);

                    //READY 신호 확인
                    //Card load 준비 되었는지 확인.
                    isOnReady = Module.GetSignal(E84SignalTypeEnum.READY);
                    if (isOnTrReq)
                    {
                        canLoad = Module.CanPutCardBuffer();
                        if (isOnReady == false)
                        {
                            if (canLoad == false)
                            {
                                retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                            }
                            else
                            {
                                retVal = Module.SetSignal(E84SignalTypeEnum.READY, true);
                            }
                        }
                        else if (isOnReady == true)
                        {
                            if (canLoad == false)
                            {
                                retVal = Module.SetSignal(E84SignalTypeEnum.READY, false);
                                retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, false);

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
                                Module.E84BehaviorStateTransition(new E84CardAGVSingleCarrierDetetedState(Module));
                            }
                        }
                        else
                        {
                            Module.E84BehaviorStateTransition(new E84CardAGVSingleTransferStartState(Module));
                        }
                    }
                }

                if (Module.GetSignal(E84SignalTypeEnum.BUSY) == false && canLoad == false)
                {
                    Module.SetEventCode(E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE);
                    Module.E84ErrorOccured(code: E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE);
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
                //Card 없는지 확인.
                isOnUReq = Module.GetSignal(E84SignalTypeEnum.U_REQ);
                if (isOnUReq == false)
                {
                    if (Module.IsCardPresenceInBuffer() == true)
                    {
                        retVal = Module.SetSignal(E84SignalTypeEnum.U_REQ, true);

                        if(isOnCS0 == false || isOnValid == false)
                        {
                            LoggerManager.Error($"SingleUnLoad(): CS0 and Valid is off");
                            retVal = Module.SetSignal(E84SignalTypeEnum.U_REQ, false);
                            retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                        }
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
                    //card unload 준비 되었는지 확인.
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
                                Module.E84BehaviorStateTransition(new E84CardAGVSingleCarrierDetetedState(Module));
                            }
                        }
                        else
                        {
                            Module.E84BehaviorStateTransition(new E84CardAGVSingleTransferStartState(Module));
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

    public class E84CardAGVSingleTransferStartState : E84CardAGVBehavirStateBase
    {
        public E84CardAGVSingleTransferStartState(E84Controller module) : base(module)
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
                    if (Module.IsCardPresenceInBuffer())
                    {
                        Module.SetCardStateInBuffer();
                        Module.E84BehaviorStateTransition(new E84CardAGVSingleCarrierDetetedState(Module));
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
                    if (Module.IsCardPresenceInBuffer() == false)
                    {
                        Module.SetCardStateInBuffer();
                        Module.E84BehaviorStateTransition(new E84CardAGVSingleCarrierDetetedState(Module));
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

    public class E84CardAGVSingleCarrierDetetedState : E84CardAGVBehavirStateBase
    {
        public E84CardAGVSingleCarrierDetetedState(E84Controller module) : base(module)
        {

        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.CARRIER_DETECTED;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                /// <condition>
                ///  [IN]BUSY : false, [IN]TR_REQ : false, [IN]COMPT : true
                /// </condition> 
                /// 

                if (Module.IsCardPresenceInBuffer() == true)
                {
                    bool isOnBusy = Module.GetSignal(E84SignalTypeEnum.BUSY);
                    bool isOnTrReq = Module.GetSignal(E84SignalTypeEnum.TR_REQ);
                    bool isOnCompt = Module.GetSignal(E84SignalTypeEnum.COMPT);
                    bool isOnCs = Module.GetSignal(E84SignalTypeEnum.CS_0);
                    bool isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);

                    if ((!isOnBusy && !isOnTrReq && isOnCompt) ||
                             (!isOnBusy && !isOnTrReq && !isOnCompt && !isOnCs && !isOnValid) ||
                            Module.GetCommModuleSubStep() == E84SubSteps.DONE_LOADING)
                    {
                        if(Module.GetCommModuleSubStep() != E84SubSteps.DONE_LOADING)
                        {
                            LoggerManager.Debug($"E84CardAGVSingleCarrierDetetedState.ClearEvent");
                            Module.ClearEvent();
                        }

                        Module.E84BehaviorStateTransition(new E84CardAGVSingleTransferDoneState(Module));
                        retVal = EventCodeEnum.NONE;
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
                ///  [IN]BUSY : false, [IN]TR_REQ : false, [IN]COMPT : true
                /// </condition> 
                if (Module.IsCardPresenceInBuffer() == false)
                {
                    bool isOnBusy = Module.GetSignal(E84SignalTypeEnum.BUSY);
                    bool isOnTrReq = Module.GetSignal(E84SignalTypeEnum.TR_REQ);
                    bool isOnCompt = Module.GetSignal(E84SignalTypeEnum.COMPT);
                    bool isOnCs = Module.GetSignal(E84SignalTypeEnum.CS_0);
                    bool isOnValid = Module.GetSignal(E84SignalTypeEnum.VALID);

                    if ((!isOnBusy && !isOnTrReq && isOnCompt) ||
                       (!isOnBusy && !isOnTrReq && !isOnCompt && !isOnCs && !isOnValid) ||
                       Module.GetCommModuleSubStep() == E84SubSteps.DONE_UNLOADING)
                    {
                        if (Module.GetCommModuleSubStep() != E84SubSteps.DONE_LOADING)
                        {
                            LoggerManager.Debug($"E84CardAGVSingleCarrierDetetedState.ClearEvent");
                            Module.ClearEvent();
                        }
                        //Module.SetCarrierState();
                        Module.E84BehaviorStateTransition(new E84CardAGVSingleTransferDoneState(Module));
                        retVal = EventCodeEnum.NONE;
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

    public class E84CardAGVSingleTransferDoneState : E84CardAGVBehavirStateBase
    {
        public E84CardAGVSingleTransferDoneState(E84Controller module) : base(module)
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
                        Module.E84BehaviorStateTransition(new E84CardAGVSingleCSReleaseState(Module));
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
                        Module.E84BehaviorStateTransition(new E84CardAGVSingleCSReleaseState(Module));
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

    public class E84CardAGVSingleCSReleaseState : E84CardAGVBehavirStateBase
    {
        public E84CardAGVSingleCSReleaseState(E84Controller module) : base(module)
        {
        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.CS_RELEASE;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                                
                Module.E84BehaviorStateTransition(new E84CardAGVSingleDoneState(Module));
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
                Module.E84BehaviorStateTransition(new E84CardAGVSingleDoneState(Module));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


    public class E84CardAGVSingleDoneState : E84CardAGVBehavirStateBase
    {
        public E84CardAGVSingleDoneState(E84Controller module) : base(module)
        {
        }
        public override E84SubStateEnum GetSubStateEnum() => E84SubStateEnum.DONE;

        public override EventCodeEnum SingleLoad()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //e84로 cardpresence 상태가 변경되었을 경우 sequence가 완료된 뒤에 presence 변경에 대한 이벤트를 호출한다.
                Module.UpdateCardBufferState(forced_event: true);

                Module.E84BehaviorStateTransition(new E84CardAGVSingleReadyState(Module));
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
                //e84로 cardpresence 상태가 변경되었을 경우 sequence가 완료된 뒤에 presence 변경에 대한 이벤트를 호출한다.
                Module.UpdateCardBufferState(forced_event: true);

                Module.E84BehaviorStateTransition(new E84CardAGVSingleReadyState(Module));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}

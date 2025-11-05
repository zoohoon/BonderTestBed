using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using System.Linq;
using System.Threading;
using LoaderServiceBase;
using NotifyEventModule;
using ProberInterfaces.Event;
using LoaderBase.Communication;

namespace CardChangeSupervisor.CardChangeSupervisorState.InnerState
{

    public class IDLE : CardChangeInnerStateBase
    {
        public IDLE(CardChangeSupervisor module) : base(module) { }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.IDLE;        
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.IDLE;

        public override ActiveCCInfo GetRunningCCInfo()
        {
            return null;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Func<bool> abortConditionFuc = () => true;

                Action abortDoAction = () =>
                {
                    Module.Abort();                    
                };

                bool isprocessed = Module.CommandManager().ProcessIfRequested<IAbortCardChangeSequence>(
                    Module,
                    abortConditionFuc,
                    abortDoAction);

                if (isprocessed)
                {
                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }

               
                Func<bool> conditionFuc = () =>
                {
                    bool isvalid = false;

                    try
                    {
                        var cmdParam = Module.CommandRecvSlot.Token.Parameter as RequestCCJobInfo;

                        var cardloadport = Module.FindActiveCCInfo(cmdParam.allocSeqId).activeCCInfo.CardLoadPort as ICardOwnable;
                        if (cardloadport.Holder.Status == EnumSubsStatus.EXIST || cardloadport.Holder.Status == EnumSubsStatus.CARRIER)// TODO: 항상 Origin에서만 PGV 동작하도록 하는 코드, 나중에 중간 진입점에 대해서 고려되어야함.
                        {
                            isvalid = true;
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        Module.InnerStateTransition(new ERROR(Module));
                    }

                    return isvalid;
                };
                

                Action startAction = () =>
                {
                    var cmdParam = Module.CommandRecvProcSlot.Token.Parameter as RequestCCJobInfo;

                    Module.InnerStateTransition(new TRANSFER_READY(Module, new WAIT_CMD(Module, cmdParam.allocSeqId), Module.FindActiveCCInfo(cmdParam.allocSeqId).activeCCInfo ));
                };


                Action abortAction = () => { LoggerManager.Debug($"[CardChangeSupervisor].IDLE(): Command Is Not Excuted."); };

                    Module.CommandManager().ProcessIfRequested<IStartCardChangeSequence>(
                       Module,
                       conditionFuc,
                       startAction,
                       abortAction);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));                
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
                isInjected = token is IStartCardChangeSequence ||
                             token is IAbortCardChangeSequence;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);
            }
            return isInjected;
        }
       
    }

    /// <summary>
    /// 받은 cmd정보대로 transfer를 하는 state.
    /// runningCCInfo가 유효 하지 않고 WAIT_CMD state에 runningCCInfo를 넘겨줌.
    /// </summary>
    public class TRANSFER_READY : CardChangeInnerStateBase
    {
        bool StopSoakingRequested = false;
        ActiveCCInfo runningCCInfo = null;
        public TRANSFER_READY(CardChangeSupervisor module) : base(module) 
        {
            Module.PrevLoaderState = Module.Master.ModuleState.GetState();
            LoggerManager.Debug($"[CardChangeSupervisor] PrevLoaderState = {Module.PrevLoaderState}");
        }

        public CardChangeInnerStateBase NextState { get; set; }

        private bool IsShowingWait = false;
        
        private bool IsShowingForWaitTemp = false;

        bool setCCActiveTempOnce = false;
        bool needToSetSV = true;
        bool isOkToChangeCCTemp = false;

        public TRANSFER_READY(CardChangeSupervisor module, CardChangeInnerStateBase nextState, ActiveCCInfo cmdParam) : base(module)
        {            
            NextState = nextState;
            runningCCInfo = cmdParam;
            Module.PrevLoaderState = Module.Master.ModuleState.GetState();
            LoggerManager.Debug($"[CardChangeSupervisor] PrevLoaderState = {Module.PrevLoaderState}");

        }


        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.TRANSFER_READY;

        public override ActiveCCInfo GetRunningCCInfo()
        {
            return runningCCInfo;
        }


        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsShowingWait == false)
                {
                    Module.ShowCardChangeWaitDialog();

                    IsShowingWait = true;
                }
                ILoaderServiceCallback client = null;

                Func<bool> abortConditionFuc = () => true;

                Action abortDoAction = () =>
                {
                    Module.Abort();
                };
                
                bool isprocessed = Module.CommandManager().ProcessIfRequested<IAbortCardChangeSequence>(
                    Module,
                    abortConditionFuc,
                    abortDoAction);

                if (isprocessed)
                {
                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }

                int stgIndex = Module.ActiveCCList.First().CardReqModule.ID.Index;
                bool isWaferExist = false;
                bool isPolish = false;

                if (Module.IsNeedRemoveWafer(stgIndex, out isWaferExist, out isPolish) == EventCodeEnum.NONE)
                {
                    if (isWaferExist == false)
                    {
                        var remainJob = Module.Master.Loader.LoaderJobViewList.Count(x => x.JobDone == false);
                        if (remainJob == 0 && Module.Master.Loader.ModuleState == ModuleStateEnum.IDLE)
                        {
                            // 셀이 PW Load할 예정이 없다고 하면 CC Temp 로 바꿔도 됨. 
                            SetIsOkToChangeCCTemp(true);
                        }
                        else 
                        {
                            SetIsOkToChangeCCTemp(false);
                            LoggerManager.Debug($"[CardChangeSupervisor] Waiting Loader remain job cur:{remainJob} or loaderstate idle, cur:{Module.Master.Loader.ModuleState}");
                        }

                    }
                    else
                    {
                        SetIsOkToChangeCCTemp(false);
                        if (isPolish == true)
                        {
                            if (Module.StopSoakingAvailable(stgIndex) == true && StopSoakingRequested == false)
                            {
                                LoggerManager.Debug($"[CardChangeSupervisor] Stop Soaking Requested.");
                                Module.StopSoaking(stgIndex);
                                StopSoakingRequested = true;
                            }
                        }
                        else
                        {
                            //웨이퍼가 있지만 제거할 수 없는 웨이퍼가 있음. 
                            Module.NotifyManager().Notify(EventCodeEnum.CANNOT_TRANSFER);
                            Module.InnerStateTransition(new ERROR(Module));
                            return retVal;
                        }
                    }
                }
                else
                {
                    //셀한테 정상적으로 물어보지 못한다는 뜻이니까 종료.
                    Module.NotifyManager().Notify(EventCodeEnum.STAGE_DISCONNECTED);
                    Module.InnerStateTransition(new ERROR(Module));
                    return retVal;
                }

                if (isOkToChangeCCTemp)
                {
                    if (IsShowingWait && IsShowingForWaitTemp == false)
                    {
                        Module.MetroDialogManager().SetDataWaitCancelDialog(message: "Wait for card changing", hashcoe: Module.GetHashCode().ToString(), Module.CancelTransferTokenSource, "Abort");
                        IsShowingForWaitTemp = true;
                    }

                    client = Module.Master.GetClient(runningCCInfo.CardReqModule.ID.Index);
                    EventCodeEnum checkCardChangeConditionOK = EventCodeEnum.UNDEFINED;

                    if (client != null && Module.Master.IsAliveClient(client))
                    {
                        if (client.NeedToSetCCActivatableTemp() && setCCActiveTempOnce == false)
                        {
                            client.SetCCActiveTemp();

                            setCCActiveTempOnce = true;
                        }
                        else
                        {
                            needToSetSV = false;
                        }


                        checkCardChangeConditionOK = client.CardChangeIsConditionSatisfied(needToSetTempToken: needToSetSV);
                    }

                    if (checkCardChangeConditionOK == EventCodeEnum.NONE)
                    {
                        if (Module.Master.ModuleState.GetState() == ModuleStateEnum.IDLE ||
                            Module.Master.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                        {
                            if (client == null)
                            {
                                Module.InnerStateTransition(new ERROR(Module));
                            }

                            if (client.GetRunState() == true)// true is not busy.
                            {
                                var cardloadport = runningCCInfo.CardLoadPort as ICardOwnable;
                                var stage = Module.Master.GetClient(runningCCInfo.CardReqModule.ID.Index);

                                if (Extensions_IParam.ProberRunMode != RunMode.EMUL)
                                {
                                    cardloadport.RecoveryCardStatus();
                                }


                                EnumWaferState cardStateEnum = EnumWaferState.UNDEFINED;
                                EnumSubsStatus ccstatus = Module.Master.GetCardStatusClient(stage, out cardStateEnum);

                                //if (cardbuffer.Holder.Status == EnumSubsStatus.EXIST && cardarm.Holder.Status == EnumSubsStatus.NOT_EXIST)// TODO: 항상 Origin에서만 PGV 동작하도록 하는 코드, 나중에 중간 진입점에 대해서 고려되어야함.
                                //{
                                //    Module.CommandManager().SetCommand<ITransferObject>(Module, new TransferObjectHolderInfo() { source = car});
                                //    Module.InnerStateTransition(new WAIT_CMD(Module));//, new TRANSFER(Module, new WAIT_CMD(Module))));
                                //}
                                if (cardloadport.Holder.Status == EnumSubsStatus.CARRIER && ccstatus == EnumSubsStatus.EXIST)// TODO: 항상 Origin에서만 PGV 동작하도록 하는 코드, 나중에 중간 진입점에 대해서 고려되어야함.
                                {
                                    Module.CommandManager().SetCommand<ITransferObject>(Module, new TransferObjectHolderInfo() { source = runningCCInfo.CardReqModule, target = runningCCInfo.CardLoadPort });
                                    Module.InnerStateTransition(new WAIT_CMD(Module, runningCCInfo.AllcatedSeqId));//, new TRANSFER(Module, new WAIT_CMD(Module))));
                                }
                                else
                                {
                                    Module.NotifyManager().Notify(EventCodeEnum.CANNOT_TRANSFER);
                                    Module.InnerStateTransition(new ERROR(Module));
                                }

                                retVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                ////Soaking이 Running이면 Abort시켜야 함. -> Like maintenance Mode Click
                                //if(client.GetSoakingModuleState() == ModuleStateEnum.RUNNING)
                                //{
                                //    client.Check_N_ClearStatusSoaking();
                                //}
                            }

                            return retVal;

                        }

                        if (Module.Master.ModuleState.GetState() == ModuleStateEnum.RUNNING)
                        {
                            if ((Module.Master.CommandRecvSlot.Token is IGPLotOpPause == false)
                                && (Module.Master.ModuleState.GetState() == Module.PrevLoaderState))
                            {
                                Module.Master.SetMapSlicerLotPause(true);
                                Module.CommandManager().SetCommand<IGPLotOpPause>(Module); // 계속 setcommand하면 안됨.
                            }
                        }
                    }
                }

              

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected = (NextState == null ? false : NextState.CanExecute(token)) ||
                            token is IAbortCardChangeSequence;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);
            }
            return isInjected;
        }      

        private void SetIsOkToChangeCCTemp(bool value)
        {
            if(isOkToChangeCCTemp != value)
            {
                LoggerManager.Debug($"[CardChangeSupervisor] isOkToChangeCCTemp value changed {isOkToChangeCCTemp} to {value}");
                isOkToChangeCCTemp = value;
            }
        }
    }

    /// <summary>
    /// 받은 cmd정보대로 transfer를 하는 state.
    /// runningCCInfo가 유효한 state.(WAIT_CMD, TRANSFER, TRANSFERED)
    /// </summary>
    public class TRANSFER: CardChangeInnerStateBase
    {
        string runningSeqId = null;
        public CardChangeInnerStateBase NextState { get; set; }

        public TRANSFER(CardChangeSupervisor module, string runningSeqId) : base(module)
        {
            this.runningSeqId = runningSeqId;
        }

        public TRANSFER(CardChangeSupervisor module, CardChangeInnerStateBase nextState) : base(module) 
        {
            NextState = nextState;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.TRANSFER;
        public override ActiveCCInfo GetRunningCCInfo()
        {
            return Module.FindActiveCCInfo(this.runningSeqId).activeCCInfo;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                

                Func<bool> abortConditionFuc = () => true;

                Action abortDoAction = () =>
                {
                    Module.Abort();                    
                };
                
                bool isprocessed = Module.CommandManager().ProcessIfRequested<IAbortCardChangeSequence>(
                    Module,
                    abortConditionFuc,
                    abortDoAction);

                if (isprocessed)
                {
                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }


                bool canExcuteMap = false;
                var remainJob = Module.Master.Loader.LoaderJobViewList.Count(x => x.JobDone == false);
                if (remainJob == 0)
                {
                    canExcuteMap = true;
                }

                if (canExcuteMap && Module.Master.Loader.ModuleState == ModuleStateEnum.IDLE)
                {
                    bool isSuccess = false;

                    if (Module.CommandRecvDoneSlot.IsNoCommand() == false)
                    {
                        if (Module.CommandRecvDoneSlot.Token is ITransferObject)
                        {
                            if (Module.CommandRecvDoneSlot.Token.Parameter as TransferObjectHolderInfo != null)
                            {
                                var commandInfo = Module.CommandRecvDoneSlot.Token.Parameter as TransferObjectHolderInfo;

                                if (commandInfo.source is ICardOwnable && 
                                    commandInfo.target is ICardOwnable &&
                                    commandInfo.source != commandInfo.target )
                                {
                                    //카드암은 두개가 될 가능성이 적은것 같아서 그 사항은 고려하지 않았음...
                                    isSuccess = Module.Master.TransferCardObjectFunc(commandInfo.source as ICardOwnable, commandInfo.target as ICardOwnable);

                                    if (!isSuccess)
                                    {
                                        LoggerManager.Debug($"[CardChangeSupervisor].TRANSFER(): Cannot Action.");
                                    }
                            
                                }
                            }
                        }
                        
                        if (isSuccess)
                        {
                            Module.InnerStateTransition(new TRANSFERED(Module, new WAIT_CMD(Module, this.runningSeqId), this.runningSeqId));// targetstagenumber을 clear함.
                            return retVal;
                        }
                        else
                        {
                            Module.NotifyManager().Notify(EventCodeEnum.CANNOT_TRANSFER);
                            Module.InnerStateTransition(new ERROR(Module));
                        }

                    }

                    Module.NotifyManager().Notify(EventCodeEnum.CANNOT_TRANSFER);
                    Module.InnerStateTransition(new ERROR(Module));

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected = token is ITransferObject ||
                             token is IAbortCardChangeSequence;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);                
            }
            return isInjected;
        }
    }

    /// <summary>
    /// cmd를 기다리는 state. 
    /// runningCCInfo가 유효한 state.(WAIT_CMD, TRANSFER, TRANSFERED)
    /// </summary>
    public class WAIT_CMD : CardChangeInnerStateBase
    {       
        string runningSeqId = null;
        public CardChangeInnerStateBase NextState { get; set; }      

        public WAIT_CMD(CardChangeSupervisor module, string runningSeqId) : base(module)
        {
            this.runningSeqId = runningSeqId;
        }

        public WAIT_CMD(CardChangeSupervisor module, CardChangeInnerStateBase nextState) : base(module)
        {
            NextState = nextState;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.WAIT_CMD;

        /// <summary>
        /// 현재 시퀀스를 진행중인 CCInfo 를 반환하는 함수.
        /// 지금은 (WAIT_CMD - TRANSFER - TRANSFERED)에서만 동일한 RunningCCInfo을 사용한다라고 정의함.
        /// TODO: 만약 여러개의 시퀀스가 동시 할당이 되었고 병렬로 섞여서 돌아야한다고 하면 전환되는 시점이 필요함.지금은 READY에서 받은 값을 사용하고 있고 중간에 전환되는 부분 없음.
        /// </summary>
        /// <returns></returns>
        public override ActiveCCInfo GetRunningCCInfo()
        {
            return Module.FindActiveCCInfo(this.runningSeqId).activeCCInfo;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Func<bool> abortConditionFuc = () => true;

                Action abortDoAction = () =>
                {
                    Module.Abort();
                };

                bool isprocessed = Module.CommandManager().ProcessIfRequested<IAbortCardChangeSequence>(
                    Module,
                    abortConditionFuc,
                    abortDoAction);


                if (isprocessed)
                {
                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }


                Func<bool> conditionFuc = () =>
                {
                    bool isvalid = false;
                    try
                    {
                        if (Module.CommandRecvSlot.Token.Parameter is TransferObjectHolderInfo)
                        {
                            var transferinfo = Module.CommandRecvSlot.Token.Parameter as TransferObjectHolderInfo;
                            if (Module.CommandRecvSlot.Token is ITransferObject)
                            {
                                isvalid = Module.Master.ValidateTransferCardObject(source: transferinfo.source as ICardOwnable, target: transferinfo.target as ICardOwnable);
                            }
                            else
                            {
                                LoggerManager.Debug($"[CardChangeSupervisor].WAIT_CMD(): Cannot Excute.");
                            }

                        }                        
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        Module.InnerStateTransition(new ERROR(Module));
                    }

                    return isvalid;
                };

                Action doAction = () =>
                {                   
                    Module.InnerStateTransition(new TRANSFER(Module, runningSeqId: GetRunningCCInfo().AllcatedSeqId));
                };

                Action abortAction = () =>
                {
                    Module.NotifyManager().Notify(EventCodeEnum.CANNOT_TRANSFER);
                    LoggerManager.Debug($"[CardChangeSupervisor].WAIT_CMD(): Command Is Not Excuted.");
                    Module.InnerStateTransition(new ERROR(Module));
                };


                Module.CommandManager().ProcessIfRequested<ITransferObject>(
                    Module,
                    conditionFuc,
                    doAction,
                    abortAction);


                if (Module.CommandRecvSlot.IsNoCommand() && Module.ActiveCCList.Count <= 0) // 모든 Sequence가 끝났음.
                {
                    Module.InnerStateTransition(new CLEAR(Module));                    
                }
                        

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));                            
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
       
                isInjected = token is ITransferObject ||
                             token is IAbortCardChangeSequence;
          
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);
            }
            return isInjected;
        }
    }

    /// <summary>
    /// transfer가 완료된 state.
    /// runningCCInfo가 유효한 state.(WAIT_CMD, TRANSFER, TRANSFERED)
    /// </summary>
    public class TRANSFERED : CardChangeInnerStateBase
    {
        string runningSeqId = null;
        public CardChangeInnerStateBase NextState { get; set; }        

        public TRANSFERED(CardChangeSupervisor module, CardChangeInnerStateBase nextState, string runningSeqId) : base(module)
        {
            NextState = nextState;
            this.runningSeqId = runningSeqId;
        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.TRANSFERED;

        public override ActiveCCInfo GetRunningCCInfo()
        {
            return Module.FindActiveCCInfo(this.runningSeqId ?? "").activeCCInfo;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Func<bool> abortConditionFuc = () => true;

                Action abortDoAction = () =>
                {
                    Module.Abort();
                };

                bool isprocessed = Module.CommandManager().ProcessIfRequested<IAbortCardChangeSequence>(
                    Module,
                    abortConditionFuc,
                    abortDoAction);

                if (isprocessed)
                {
                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }

                

                if (Module.IsEndSequence(GetRunningCCInfo()) && Module.CommandRecvSlot.IsNoCommand())
                {                    
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    Module.Master.EventManager().RaisingEvent(typeof(CardIdleEvent).FullName, new ProbeEventArgs(this, semaphore, new PIVInfo(stagenumber: GetRunningCCInfo()?.CardReqModule.ID.Index ?? 0,
                                                                                                                                              cardbufferindex: GetRunningCCInfo()?.CardLoadPort.ID.Index ?? 0)));
                    semaphore.Wait();

                    this.NextState = new WAIT_CMD(Module, runningSeqId);// WAIT_CMD에서 끝내야함.
                    Module.DeallocateActiveCCInfo(runningSeqId);// 한시퀀스가 끝났음.                                       
                }

                Module.InnerStateTransition(NextState);



                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));                      
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
                isInjected = (NextState == null ? false : NextState.CanExecute(token))//&& (token is ITransferObject))
                             || token is IAbortCardChangeSequence;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);
            }
            return isInjected;
        }
    }

    /// <summary>
    /// 할당된 모든 cc sequnce가 수행되었고 pgv 세션을 열기 전 상태로 만드는 state.
    /// runningCCInfo가 유효하지 않은 state.
    /// </summary>
    public class CLEAR : CardChangeInnerStateBase
    {
        public CLEAR(CardChangeSupervisor module) : base(module) 
        {
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.CLEAR;

        public override ActiveCCInfo GetRunningCCInfo()
        {
            return null;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                
                /*
                 (LotResume) 

                  CloseWaitMessage

                   Lot Resume이 완료되면(Lot상태가 Prev 상태로 돌아가고 Prev 상태로 돌아가지 않으면 에러? 메뉴얼 Abort? )closeWaitMsg 후에 DoneState가 됨.
                 
                 
                 */
                Func<bool> abortConditionFuc = () => true;

                Action abortDoAction = () =>
                {
                    Module.Abort();
                };

                bool isprocessed = Module.CommandManager().ProcessIfRequested<IAbortCardChangeSequence>(
                                    Module,
                                    abortConditionFuc,
                                    abortDoAction);

                if (isprocessed)
                {
                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }

                //prevloaderstate == paused , idle 
                var curloaderstate = Module.Master.ModuleState.GetState();
                if (Module.PrevLoaderState == curloaderstate || curloaderstate == ModuleStateEnum.IDLE)
                {
                    LoggerManager.Debug($"[CardChangeSupervisor] PrevLoaderState = {Module.PrevLoaderState}, CurLoaderState = {curloaderstate}");
                    Module.CloseCardChangeWaitDialog();
                    Module.ClearAllActiveCCInfo();
                    Module.InnerStateTransition(new DONE(Module));                    

                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }
                else
                {
                    if (Module.Master.CommandRecvSlot.Token is IGPLotOpResume == false)
                    {
                        Module.Master.SetMapSlicerLotPause(false);
                        Module.CommandManager().SetCommand<IGPLotOpResume>(Module); // 계속 setcommand하면 안됨.
                    }
                }
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));                          
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected =
                    token is IAbortCardChangeSequence;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);
            }
            return isInjected;
        }
    }

    public class DONE : CardChangeInnerStateBase
    {
        public DONE(CardChangeSupervisor module) : base(module) 
        {
           
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.DONE;

        public override ActiveCCInfo GetRunningCCInfo()
        {
            return null;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Func<bool> abortConditionFuc = () => true;

                Action abortDoAction = () =>
                {
                    Module.Abort();                    
                };

               bool isprocessed = Module.CommandManager().ProcessIfRequested<IAbortCardChangeSequence>(
                    Module,
                    abortConditionFuc,
                    abortDoAction);

                if (isprocessed)
                {
                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }

                Module.ClearState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
                isInjected = 
                token is IAbortCardChangeSequence;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);
            }
            return isInjected;
        }
    }

    public class ABORT : CardChangeInnerStateBase
    {
        public ABORT(CardChangeSupervisor module) : base(module) 
        {

        }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ABORT;
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.ABORT;

        public override ActiveCCInfo GetRunningCCInfo()
        {
            return null;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // 이미 Abort 이므로 쓸모없는 코드.
                //Func<bool> abortConditionFuc = () => true;

                //Action abortDoAction = () =>
                //{
                //    Module.Abort();
                //    return;
                //};

                //Module.CommandManager().ProcessIfRequested<IAbortCardChangeSequence>(
                //    Module,
                //    abortConditionFuc,
                //    abortDoAction);

                Module.ClearState();//ISSD-3529
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                //isInjected = token is IAbortCardChangeSequence;// abort 상태에서 abort 받으면 그냥 abort 상태.

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);
            }
            return isInjected;
        }

        public override EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class ERROR : CardChangeInnerStateBase
    {
        public ERROR(CardChangeSupervisor module) : base(module) { }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ERROR;
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.ERROR;

        public override ActiveCCInfo GetRunningCCInfo()
        {
            return null;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Func<bool> abortConditionFuc = () => true;

                Action abortDoAction = () =>
                {
                    Module.Abort();
                    Module.Master.Loader.ClearRequestData();

                };

                bool isprocessed = Module.CommandManager().ProcessIfRequested<IAbortCardChangeSequence>(
                    Module,
                    abortConditionFuc,
                    abortDoAction);

                if (isprocessed)
                {
                    retVal = EventCodeEnum.NONE;
                    return retVal;
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
            bool isInjected = false;
            try
            {
                isInjected = token is IAbortCardChangeSequence;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);
            }
            return isInjected;
        }

        //base 의 abort를 불러야함.
        //public override EventCodeEnum Abort()
        //{
        //    return EventCodeEnum.NONE;
        //}
    }

    public class RECOVERY : CardChangeInnerStateBase// 아직 지원 안함.
    {
        public RECOVERY(CardChangeSupervisor module) : base(module) { }

        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RECOVERY;
        public override CardChangeStateEnum GetState() => CardChangeStateEnum.RECOVERY;
        public override ActiveCCInfo GetRunningCCInfo()
        {
            return null;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {              
                retVal = EventCodeEnum.NONE;             
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
            }
            return retVal;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected = false;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Module.InnerStateTransition(new ERROR(Module));
                Module.NotifyManager().Notify(EventCodeEnum.COMMAND_ERROR);
            }
            return isInjected;
        }

        public override EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
    }
}

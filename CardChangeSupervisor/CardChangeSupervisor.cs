using Autofac;
using LoaderBase;
using LoaderBase.Communication;
//using LoaderServiceBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
//using System.ServiceModel;
//using System.ServiceModel.Channels;
using System.Threading;
//using TimeServices;
//using MetroDialogInterfaces;
//using NotifyEventModule;
//using LoaderParameters.Data;
using ProberInterfaces.Event;
using SequenceService;
using CardChangeSupervisor.CardChangeSupervisorState;
using ProberInterfaces.State;
using CardChangeSupervisor.CardChangeSupervisorState.InnerState;
using MetroDialogInterfaces;
using NotifyEventModule;
using System.Runtime.CompilerServices;
using LoaderServiceBase;
using ProberInterfaces.Proxies;

namespace CardChangeSupervisor
{
    public class CardChangeSupervisor : SequenceServiceBase, ICardChangeSupervisor, INotifyPropertyChanged
    {
        public Autofac.IContainer Container { get; set; }        

        public CardChangeSupervisor()
        {

        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.Loader);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
                }
            }
        }
        private CommandSlot _CommandRecvSlot = new CommandSlot();
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set { _CommandRecvSlot = value; }
        }

        private CommandSlot _CommandProcSlot = new CommandSlot();
        public CommandSlot CommandRecvProcSlot
        {
            get { return _CommandProcSlot; }
            set { _CommandProcSlot = value; }
        }

        private CommandSlot _CommandRecvDoneSlot = new CommandSlot();
        public CommandSlot CommandRecvDoneSlot
        {
            get { return _CommandRecvDoneSlot; }
            set { _CommandRecvDoneSlot = value; }
        }
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }
        private CommandTokenSet _RunTokenSet = new CommandTokenSet();
        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }
        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get
            {
                return _ModuleState;
            }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;                  
                }
            }
        }


        private ObservableCollection<TransitionInfo> _TransitionInfo
            = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }


        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }


        public bool Initialized { get; set; } = false;


        public event PropertyChangedEventHandler PropertyChanged;
        private new void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private ILoaderSupervisor _Master;
        public ILoaderSupervisor Master
        {
            get { return _Master; }
            set
            {
                _Master = value;
            }

        }


        private List<ActiveCCInfo> _ActiveCCList = new List<ActiveCCInfo>();

        public List<ActiveCCInfo> ActiveCCList
        {
            get { return _ActiveCCList; }
            //set { _UsingModuleList = value; }
        }

        /// <summary>
        /// pgv에서 사용할 CCInfo를 할당함.
        /// </summary>
        /// <param name="activeinfo"></param>
        /// <returns></returns>
        public EventCodeEnum AllocateActiveCCInfo(ActiveCCInfo activeinfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (FindActiveCCInfo(activeinfo.CardLoadPort, activeinfo.CardReqModule).isExist == false)// 동일한 ActiveCCInfo가 존재해서 reject 하는건데 존재하면안되는건가..?
                {
                    ActiveCCList.Add(activeinfo);
                    LoggerManager.Debug($"[CardChangeSupervisor] AllocateActiveCCInfo(): success. " +
                                                                $"AllcatedSeqId:{activeinfo.AllcatedSeqId}, " +
                                                                $"CardLoadPort:{activeinfo.CardLoadPort.ID}, " +
                                                                $"CardReqModule:{activeinfo.CardReqModule.ID}");
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"[CardChangeSupervisor] AllocateActiveCCInfo(): failed. CardLoadPort:{activeinfo.CardLoadPort.ID} CardReqModule:{activeinfo.CardReqModule.ID}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum DeallocateActiveCCInfo(string allocSeqId)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var activeinfo = ActiveCCList.Where(i => i.AllcatedSeqId == allocSeqId).FirstOrDefault();
                if (activeinfo != null)
                {
                    ActiveCCList.Remove(activeinfo);
                    LoggerManager.Debug($"[CardChangeSupervisor] DeallocateActiveCCInfo(): success." +
                                                                $"AllcatedSeqId:{activeinfo.AllcatedSeqId}, " +
                                                                $"CardLoadPort:{activeinfo.CardLoadPort.ID}, " +
                                                                $"CardReqModule:{activeinfo.CardReqModule.ID}");
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"[CardChangeSupervisor] DeallocateActiveCCInfo(): cannot find ActiveCCInfo({allocSeqId}). ");
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ClearAllActiveCCInfo()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                
                if(ActiveCCList.Count() > 0)
                {
                    LoggerManager.Debug($"[CardChangeSupervisor] ClearAllActiveCCInfo()");
                    for (int i = 0; i < ActiveCCList.Count(); i++)
                    {
                        DeallocateActiveCCInfo(ActiveCCList[i].AllcatedSeqId);
                    }                    
                }
            
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsAllocatedStage(int number)
        {
            bool isExist = false;          
            try
            {
                var findccinfo = ActiveCCList.Where(i => i.CardReqModule.ModuleType == ModuleTypeEnum.CC &&
                                            i.CardReqModule.ID.Index == number
                                            ).FirstOrDefault();
                if (findccinfo != null)
                {
                    isExist = true;                  
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return isExist;
        }

        public (bool isExist, ActiveCCInfo activeCCInfo) FindActiveCCInfo(IAttachedModule cardloadport, IAttachedModule cardreqmodule)
        {
            bool isExist = false;
            ActiveCCInfo activeCCInfo = null;
            try
            {
                var findccinfo = ActiveCCList.Where(i => i.CardLoadPort.ModuleType == cardloadport.ModuleType &&
                                            i.CardReqModule.ModuleType == cardreqmodule.ModuleType &&
                                            i.CardLoadPort.ID.Index == cardloadport.ID.Index &&
                                            i.CardReqModule.ID.Index == cardreqmodule.ID.Index).FirstOrDefault();
                if (findccinfo != null)
                {
                    isExist = true;
                    activeCCInfo = findccinfo;
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return (isExist, activeCCInfo);
        }

        public (bool isExist, ActiveCCInfo activeCCInfo) FindActiveCCInfo(string allocSeqId)
        {
            bool isExist = false;
            ActiveCCInfo activeCCInfo = null;
            try
            {
                var findccinfo = ActiveCCList.Where(i => i.AllcatedSeqId == allocSeqId).FirstOrDefault();
                if (findccinfo != null)//TODO: == 으로 봐도 될까? 오브젝트가 변경되면 false로 나올까? moduletype이랑 index
                {
                    isExist = true;
                    activeCCInfo = findccinfo;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return (isExist, activeCCInfo);
        }
        

        public (string allocSeqId, ModuleTypeEnum cardlpType, ModuleTypeEnum cardreqmoduleType, int cardlpIndex, int cardreqmoduleIndex)
            GetRunningCCInfo()
        {
            string allocSeqId = "";
            ModuleTypeEnum cardlpType = ModuleTypeEnum.UNDEFINED;
            ModuleTypeEnum cardreqmoduleType = ModuleTypeEnum.UNDEFINED;
            int cardlpIndex = 0;
            int cardreqmoduleIndex = 0;

            var ccinfo = CCModuleState.SubModule.GetRunningCCInfo();
            if (ccinfo != null)
            {
                allocSeqId = ccinfo.AllcatedSeqId;
                cardlpType = ccinfo.CardLoadPort.ModuleType;
                cardreqmoduleType = ccinfo.CardReqModule.ModuleType;
                cardlpIndex = ccinfo.CardLoadPort.ID.Index;
                cardreqmoduleIndex = ccinfo.CardReqModule.ID.Index;
            }
            return (allocSeqId, cardlpType, cardreqmoduleType, cardlpIndex, cardreqmoduleIndex);
        }

        public List<ActiveCCInfo> GetActiveCCInfos()
        {
            return ActiveCCList;
        }



        private ModuleStateEnum _PrevLoaderState;

        public ModuleStateEnum PrevLoaderState
        {
            get { return _PrevLoaderState; }
            set { _PrevLoaderState = value; }
        }

        static CancellationTokenSource _CancelTransferTokenSource = new CancellationTokenSource();
        public CancellationTokenSource CancelTransferTokenSource
        {
            get { return _CancelTransferTokenSource; }
            set
            {
                if (value != _CancelTransferTokenSource)
                {
                    _CancelTransferTokenSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {
                RetVal = CCModuleState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum retval = ModuleStateEnum.ERROR;
            try
            {
                retval = Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public void StateTransition(ModuleStateBase state)
        {
            try
            {
                ModuleState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        public EventCodeEnum ClearState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //NeedToRecovery = false;
                CommandRecvSlot.ClearToken();
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;

        }

        public void DeInitModule()
        {
            
        }

        public ModuleStateEnum End()
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Execute()
        {
            ModuleStateEnum retState = ModuleStateEnum.ERROR;
            try
            {
                EventCodeEnum retVal = InnerState.Execute(); 
                ModuleState.StateTransition(InnerState.GetModuleState()); //<-- 이건 뭐징..
                RunTokenSet.Update();
                retState = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retState;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                retval = InnerState.GetModuleState().ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public EventCodeEnum InitModule()
        {          
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ModuleState = new ModuleUndefinedState(this);
                _CCModuleState = new CardChange_Idle(this); // 꺼지기 이전 상태 에러였으면? 
                InnerState = new IDLE(this);                
                
                Master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //this.Container = container;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private CardChangeSupervisorStateBase _CCModuleState;
        public CardChangeSupervisorStateBase CCModuleState
        {
            get { return _CCModuleState; }
        }

        public IInnerState InnerState
        {
            get { return _CCModuleState.SubModule; }
            set
            {
                if (value != _CCModuleState.SubModule)
                {
                    _CCModuleState.SubModule = value as CardChangeInnerStateBase;
                }
            }
        }
        public IInnerState PreInnerState { get; set; }

        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL4;

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (state != null)
                {
                    PreInnerState = InnerState;
                    InnerState = state;     
                    LoggerManager.Debug($"[{GetType().Name}].InnerStateTransition() : Pre state = {PreInnerState}), Now State = {InnerState})");

                    if(state.GetModuleState() == ModuleStateEnum.ERROR)
                    {
                        this.MetroDialogManager().ShowMessageDialog($"PGV ERROR", $"Need To Manual Recovery, PGV Abort and Click 'MENU - Cell Operation - CC Setting OP - Manual Control - PGV Clear' Button ", EnumMessageStyle.Affirmative);
                        // if result == yes 이면 Module.Abort 한다 : Wait 안떴을때 State Clear 안되는 문제 있음. 
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public bool CanStartCardChange(IAttachedModule cardloadport, IAttachedModule cardreqmodule)
        {
            bool isvalid = false;
            
            try
            {
                //var cardbuffer = Master.Loader.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, 1);
                var loaderCommManager = Master.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                var stage = Master.GetClient(cardreqmodule.ID.Index);

                Action<string> ShowErrorMessage = (errormsg) => 
                {
                    this.MetroDialogManager().ShowMessageDialog($"PGV ERROR", $"Can't Start Pgv \n {errormsg}", EnumMessageStyle.Affirmative);
                };

                string msg = "";
                
                if (cardloadport is null)
                {
                    msg = $"[ERROR] CardLoadPort info is null\n";                    
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                if (cardreqmodule is null)
                {
                    msg = $"[ERROR] CardRequestModule info is null\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                if (cardloadport.ID.Index <= 0)
                {
                    msg = $"[ERROR] CardLoadPort info is invalid. cur: ID.Index({cardloadport.ID.Index})\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                if (cardreqmodule.ID.Index <= 0)
                {
                    msg = $"[ERROR] CardRequestModule info is invalid. cur: ID.Index({cardreqmodule.ID.Index})\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                if (cardloadport.ModuleType == ModuleTypeEnum.CARDBUFFER)
                {
                    if (cardloadport.ID.Index > SystemModuleCount.ModuleCnt.CardBufferCount)
                    {
                        msg = $"[ERROR] CardLoadPort.ID.Index is upper than system parameter. cmdParam.ID.Index:{cardloadport.ID.Index}, moduleCount:{SystemModuleCount.ModuleCnt.CardBufferCount}\n";
                        ShowErrorMessage(msg);
                        return isvalid;
                    }
                }

                if (cardreqmodule.ModuleType == ModuleTypeEnum.CC)
                {
                    if (cardreqmodule.ID.Index > SystemModuleCount.ModuleCnt.StageCount)
                    {
                        msg = $"[ERROR] CardRequestModule.ID.Index is upper than system parameter. cmdParam.ID.Index:{cardreqmodule.ID.Index}, moduleCount:{SystemModuleCount.ModuleCnt.StageCount}\n";
                        ShowErrorMessage(msg);
                        return isvalid;
                    }
                }
              


                if (FindActiveCCInfo(cardloadport, cardreqmodule).isExist == true)
                {
                    msg = $"[ERROR] Active cardchange information is already exist\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                if (stage is null)
                {
                    msg = $"[ERROR] stage({cardreqmodule.ID.Index}) is not connected\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }


                // 로더 상태 Error 아님. 
                if (Master.ModuleState.GetState() == ModuleStateEnum.ERROR)
                {
                    msg = $"[ERROR] LoaderSupervisor moduleState is error. cur:{Master.ModuleState.GetState()}\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                // 카드 체인지 모듈 상태 Idle 아님. Idle 일때만 시작할 수 있음.
                if (this.InnerState.GetModuleState() != ModuleStateEnum.IDLE)
                {
                    msg = $"[ERROR] CardSupervisor moduleState is not idle. cur:{this.InnerState.GetModuleState()}\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }
               

                // 셀 연결 되어있음. Online이어야함. 
                if (loaderCommManager == null)
                {
                    msg = $"[ERROR] LoaderCommManager is null\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }
                else
                {
                    var connedcell = loaderCommManager.Cells.Where(c => c.Index == cardreqmodule.ID.Index).FirstOrDefault();
                    if (connedcell == null)
                    {
                        msg = $"[ERROR] Loader-Cell commnunication info is null\n";
                        ShowErrorMessage(msg);
                        return isvalid;
                    }
                    else
                    {
                        if (connedcell.StageMode != GPCellModeEnum.ONLINE)
                        {
                            msg = $"[ERROR] CellMode is not online. cur:{loaderCommManager?.Cells[cardreqmodule.ID.Index - 1]?.StageMode}\n";
                            ShowErrorMessage(msg);
                            return isvalid;
                        }
                    }
                }

              

                // 젬 연결 되어있음. 
                if (Master.GEMModule().GemCommManager.SecsCommInformData.ControlState != SecsEnum_ControlState.ONLINE_REMOTE)
                {
                    msg = $"[ERROR] Gem Host Connection is not online. cur:{Master.GEMModule().GemCommManager.SecsCommInformData.ControlState}\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                if (cardloadport.ModuleType == ModuleTypeEnum.CARDBUFFER)
                {
                    var cardbuffer = cardloadport as ICardBufferModule;
                    // 카드버퍼에 캐리어만 있어야함. 
                    if (cardbuffer.Holder.Status != EnumSubsStatus.CARRIER)
                    {
                        msg = $"[ERROR] Cardbuffer is not carrier state. cur:{(cardloadport as ICardOwnable).Holder.Status}\n";
                        ShowErrorMessage(msg);
                        return isvalid;
                    }

                    // cardBuffer의 presence상태 empty            
                    if (cardbuffer.CardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                    {
                        msg = $"[ERROR] Cardbuffer presence state is not empty. cur:{cardbuffer.CardPRESENCEState}\n";
                        ShowErrorMessage(msg);
                        return isvalid;
                    }
                }

                // e84controller가 null이 아님.
                if (Master.E84Module().GetE84Controller(cardloadport.ID.Index, E84OPModuleTypeEnum.CARD) == null)
                {
                    msg = $"E84 Controller({E84OPModuleTypeEnum.CARD}, {cardloadport.ID.Index}) is null. check attached.\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                // 카드암에 아무것도 없어야함.
                var cardarm = Master.Loader.ModuleManager.FindModules<ICardARMModule>().FirstOrDefault();
                if (cardarm?.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    msg = $"[ERROR] CardArmModule status is not NOT_EXIST cur:{cardarm?.Holder.Status}\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                // 셀상태가 Idle이어야함. 셀이 Pause 상태일때도 동작하면 안되는 이유는 셀이 Pause되었다는것은 Lot중에 멈췄다고 판단하기 때문. 로더만 Lot Pause로 만들어서 사용할 수 있어야함.
                // Lot에 할당된 셀이면 Lot Running 중인 셀과 동일하게 취급함.
                var celllotstate = stage.GetLotState();
                if (celllotstate != ModuleStateEnum.IDLE)//|| Master.ActiveLotInfos.Where( w => w.UsingStageIdxList.Contains(cardreqmodule.ID.Index)).Count() != 0)
                {
                    msg = $"[ERROR] stage({cardreqmodule.ID.Index}) lot state is not idle. cur:{celllotstate}\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                // 셀이나 카드에 홀더가 도킹되어있어야함. 

                EnumWaferState cardStateEnum = EnumWaferState.UNDEFINED;
                EnumSubsStatus status = Master.GetCardStatusClient(stage, out cardStateEnum);
                
                if (((cardStateEnum == EnumWaferState.UNPROCESSED ||// 카드팟에 있을 수도 
                    cardStateEnum == EnumWaferState.READY ||// ziflock 일 수도 
                    cardStateEnum == EnumWaferState.PROCESSED)// 카드인데 zifunlock 이거나 홀더 docked 일수도 
                    && status == EnumSubsStatus.EXIST) == false)
                {
                    msg = $"[ERROR] stage({cardreqmodule.ID.Index}) cardstate is not (ready & exist). cur:({cardStateEnum}, {status})\n";
                    ShowErrorMessage(msg);
                    return isvalid;
                }

                //if ((cardStateEnum == EnumWaferState.READY && status == EnumSubsStatus.EXIST) == false)// 홀더(READY && EXIST)
                //{
                //    msg = $"[ERROR] stage({cardreqmodule.ID.Index}) cardstate is not (ready & exist). cur:({cardStateEnum}, {status})\n";
                //    ShowErrorMessage(msg);
                //    return isvalid;
                //}

                bool isWaferExist = false;
                bool isPolish = false;
                if (IsNeedRemoveWafer(cardreqmodule.ID.Index, out isWaferExist, out isPolish) == EventCodeEnum.NONE)
                {
                    if (isWaferExist == true && isPolish == false)
                    {
                        msg = $"[ERROR] stage({cardreqmodule.ID.Index}) unknown wafer is on chuck. it needs to be removed. cur:(isWaferExist:{isWaferExist}, isPolish:{isPolish})\n";
                        ShowErrorMessage(msg);
                        return isvalid;
                    }
                }

                 isvalid = true;

            }
            catch (Exception err)
            {
                isvalid = false;    
                LoggerManager.Exception(err);
            }

            return isvalid;
        }

        public EventCodeEnum CardinfoValidation(string Cardid, out string Msg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            Msg = "";
            try
            {
                //TODO: PGV state 안에 두고 CardinfoValidation이 불렸을때 상황별로 Card 지금은 arm to chuck 한정으로만 불려야함.
                CardChangeInnerStateBase cardChangeInneState = InnerState as CardChangeInnerStateBase; 

                if (cardChangeInneState.GetState() != CardChangeStateEnum.IDLE) // AMR인지 Manual상황인지 알 방법으로 cardChangeInneState가 idle 인지 아닌지를 확인
                {
                    var ARMinfo = GetRunningCCInfo();
                    if (ARMinfo.cardreqmoduleType == ModuleTypeEnum.CC)
                    {
                        if (ARMinfo.cardreqmoduleIndex != 0)
                        {
                            LoggerManager.Debug($"GetRunningCCInfo().cardreqmoduleIndex: {GetRunningCCInfo().cardreqmoduleIndex}");
                            var client = Master.GetClient(GetRunningCCInfo().cardreqmoduleIndex);// 셀 번호 확인
                            retVal = client.GetCardIDValidateResult(Cardid, out Msg);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"AMR: Returns to the origin without user input. Error :{retVal}{Msg}");
                            }
                            else
                            {
                                retVal = EventCodeEnum.NONE;
                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Debug($" This is a manual transfer situation, not an AMR situation.");
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        /// <summary>
        /// CardReqModule에 카드를 로드 완료했는지 판단하는 함수
        /// </summary>
        /// <param name="ReqSeq"></param>
        /// <returns></returns>
        public bool IsEndSequence(ActiveCCInfo ReqSeq)
        {
            bool ret = false;
            try
            {
                var cardobject = this.Master.Loader.ModuleManager.FindModule(ReqSeq.CardReqModule.ModuleType, ReqSeq.CardReqModule.ID.Index);
                if(cardobject is ICardOwnable)
                {
                    var real_cardReqModule = cardobject as ICardOwnable;
                    if (real_cardReqModule?.Holder?.Status == EnumSubsStatus.EXIST)
                    {
                        ret = true;
                    }
                }
               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public bool IsBusy()
        {
            bool isbusy = false;
            if (InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
            {
                isbusy = true;
            }
            return isbusy;
            //throw new NotImplementedException();
        }

        public bool IsLotReady(out string msg)
        {
            msg = string.Empty;
            return false;
            //throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public ModuleStateEnum Pause()
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Resume()
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum Abort()
        {
            try
            {
                this.CloseCardChangeWaitDialog();

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.EventManager().RaisingEvent(typeof(CardChangeSeqAbortEvent).FullName, new ProbeEventArgs(this, semaphore, new PIVInfo(stagenumber: GetRunningCCInfo().cardreqmoduleIndex)));
                semaphore.Wait();

                CommandSendSlot.ClearToken();
                CommandRecvProcSlot.ClearToken();
                CommandRecvSlot.ClearToken();

                ILoaderServiceCallback client = null;
                for (int i = 0; i < ActiveCCList.Count(); i++)
                {
                    client = Master.GetClient(ActiveCCList[i].CardReqModule.ID.Index);
                    client.AbortCardChange();
                }

                ClearAllActiveCCInfo();

                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }
            return InnerState.GetModuleState();
        }



        public void ShowCardChangeWaitDialog()
        {
            try
            {
                Action<object> action = AbortAction;
                
                Master.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait for card changing", CancelTransferTokenSource, "", false, "Abort", action, this, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CloseCardChangeWaitDialog()
        {
            try
            {
                if (true == Master.MetroDialogManager().IsShowingWaitCancelDialog())
                {
                    Master.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void AbortAction(object cardChangeObj)
        {
            try
            {
                if (cardChangeObj is CardChangeSupervisor)
                {
                    CardChangeSupervisor cardChange = cardChangeObj as CardChangeSupervisor;
                    cardChange.CommandManager().SetCommand<IAbortCardChangeSequence>(cardChange);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        public EventCodeEnum IsNeedRemoveWafer(int stageIndex, out bool isWaferExist, out bool isPolish)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            isWaferExist = false;
            isPolish = false;

            try
            {
                var client = this.Master.GetClient(stageIndex);
                if (this.Master.IsAliveClient(client))
                {
                    isWaferExist = client.GetChuckWaferStatus() == EnumSubsStatus.EXIST;
                    isPolish = client.GetWaferType() == EnumWaferType.POLISH;
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        public bool StopSoakingAvailable(int stageIndex)
        {
            bool retVal = false;
            try
            {
                var client = this.Master.GetClient(stageIndex);
                if (this.Master.IsAliveClient(client))
                {
                    retVal = client.GetStageInfo()?.StopSoakBtnEnable ?? false;
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetStageLotDataClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum StopSoaking(int stageIndex)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ILoaderCommunicationManager LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                ISoakingModuleProxy proxy = LoaderCommunicationManager?.GetProxy<ISoakingModuleProxy>(stageIndex);
                if (proxy != null)
                {
                    proxy.SetCancleFlag(true, stageIndex);
                    retVal = EventCodeEnum.NONE;
                }

                var stage = LoaderCommunicationManager?.GetStage(stageIndex);

                if (stage != null)
                {
                    if (stage.StageInfo != null)
                    {
                        if (stage.StageInfo.LotData != null)
                        {
                            stage.StageInfo.LotData.SoakingZClearance = "N/A";
                            stage.StageInfo.LotData.SoakingRemainTime = 0.ToString();
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
}

using Autofac;
using LoaderBase;
using LogModule;
using MetroDialogInterfaces;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Event;
using ProberInterfaces.Foup;
using ProberInterfaces.State;
using SecsGemServiceInterface;
using SequenceService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WaferChangeSupervisor.WaferChangeState;

namespace WaferChangeSupervisor
{
    public class WaferChangeSupervisor : SequenceServiceBase, IWaferChangeSupervisor
    {
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

        private WaferChangeSupervisorStateBase _WCModuleState;
        public WaferChangeSupervisorStateBase WCModuleState
        {
            get { return _WCModuleState; }
        }

        public IInnerState InnerState
        {
            get { return _WCModuleState; }
            set
            {
                if (value != _WCModuleState)
                {
                    _WCModuleState = value as WaferChangeSupervisorStateBase;
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

                    if (PreInnerState.GetModuleState() != InnerState.GetModuleState())
                    {
                        LoggerManager.Debug($"[{GetType().Name}].InnerStateTransition() : Pre state = {PreInnerState}), Now State = {InnerState})");
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

        private WaferChangeAutofeed _WaferChangeAutofeed = new WaferChangeAutofeed(ProberInterfaces.Enum.OCRModeEnum.NONE, new List<AutoFeedAction>());
        public WaferChangeAutofeed WaferChangeAutofeed

        {
            get { return _WaferChangeAutofeed; }
            set
            {
                if (value != _WaferChangeAutofeed)
                {
                    _WaferChangeAutofeed = value;
                }
            }
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

        public ModuleStateEnum Execute()
        {
            ModuleStateEnum retState = ModuleStateEnum.ERROR;

            try
            {
                EventCodeEnum retVal = InnerState.Execute();
                ModuleState.StateTransition(InnerState.GetModuleState());
                
                // TODO : 필요?
                //RunTokenSet.Update();

                retState = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retState;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ModuleState = new ModuleUndefinedState(this);
                _WCModuleState = new WAFERCHANGE_IDLE(this);

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
                retVal = EventCodeEnum.NONE;
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

        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;

            try
            {
                RetVal = WCModuleState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
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

        public ModuleStateEnum Abort()
        {
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return InnerState.GetModuleState();
        }

        public void ShowWaferChangeWaitDialog()
        {
            try
            {
                Master.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait for Wafer changing");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CloseWaferChangeWaitDialog()
        {
            try
            {
                Master.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

        public bool IsEndSequence(AutoFeedAction ReqSeq)
        {
            bool ret = false;
            try
            {


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
            try
            {
                if (InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    isbusy = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return isbusy;
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

        public bool IsLotReady(out string msg)
        {
            msg = string.Empty;
            return false;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }

        public static void AbortAction(object waferChangeObj)
        {
            try
            {
                if (waferChangeObj is WaferChangeSupervisor)
                {
                    WaferChangeSupervisor waferChange = waferChangeObj as WaferChangeSupervisor;
                    waferChange.CommandManager().SetCommand<IAbortWaferChangeSequence>(waferChange);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CanStartWaferChange()
        {
            bool retval = false;

            try
            {
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //exchange rcmd에 정의되어 있는 loc1, loc2 를 데이터를 얻기 위한 함수
        public EventCodeEnum AllocateAutoFeedActions(WaferChangeData data)
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                List<AutoFeedAction> autofeedactions = new List<AutoFeedAction>();

                //Set OCR Mode
                if (data.OCRRead == 0)
                {
                    WaferChangeAutofeed = new WaferChangeAutofeed(ProberInterfaces.Enum.OCRModeEnum.NONE, autofeedactions);
                }
                else
                {
                    WaferChangeAutofeed = new WaferChangeAutofeed(ProberInterfaces.Enum.OCRModeEnum.READ, autofeedactions);
                }

                LoggerManager.Debug($"[WaferSupervisor] AllocateActiveCCInfo(): success. OCR Read Mode : {WaferChangeAutofeed.PolishWaferOCRMode.ToString()}");

                for (int i = 0; i < data.WaferID.Count(); i++)
                {
                    string loc1 = data.LOC1_LP[i];
                    string loc2 = data.LOC2_LP[i];

                    string loc1_atom_idx = data.LOC1_Atom_Idx[i];
                    string loc2_atom_idx = data.LOC2_Atom_Idx[i];

                    int locNumber1 = 0;
                    int locNumber2 = 0;

                    int LP_Number1 = 0;
                    int LP_Number2 = 0;

                    IAttachedModule transfer_loc1 = null;
                    IAttachedModule transfer_loc2 = null;

                    if (!string.IsNullOrEmpty(loc1_atom_idx))
                    {
                        if (loc1_atom_idx.ToUpper().StartsWith("S"))
                        {
                            // Extract the number following 's'
                            if (int.TryParse(loc1_atom_idx.Substring(1), out locNumber1))
                            {
                                if (int.TryParse(loc1.Substring(2), out LP_Number1))
                                {
                                    // Assuming ModuleManager and FindModule are accessible in this context
                                    transfer_loc1 = this.Master.Loader.ModuleManager.FindModule(ModuleTypeEnum.SLOT, locNumber1 + ((LP_Number1 - 1) * 25));
                                }
                            }
                        }
                        else if (loc1_atom_idx.ToUpper().StartsWith("F"))
                        {
                            // Extract the number following 'f'
                            if (int.TryParse(loc1_atom_idx.Substring(1), out locNumber1))
                            {
                                transfer_loc1 = this.Master.Loader.ModuleManager.FindModule(ModuleTypeEnum.FIXEDTRAY, locNumber1);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(loc2_atom_idx))
                    {
                        if (loc2_atom_idx.ToUpper().StartsWith("S"))
                        {
                            // Extract the number following 's'
                            if (int.TryParse(loc2_atom_idx.Substring(1), out locNumber2))
                            {
                                if (int.TryParse(loc2.Substring(2), out LP_Number2))
                                {
                                    // Assuming ModuleManager and FindModule are accessible in this context
                                    transfer_loc2 = this.Master.Loader.ModuleManager.FindModule(ModuleTypeEnum.SLOT, locNumber2 + ((LP_Number2 - 1) * 25));
                                }
                            }
                        }
                        else if (loc2_atom_idx.ToUpper().StartsWith("F"))
                        {
                            // Extract the number following 'f'
                            if (int.TryParse(loc2_atom_idx.Substring(1), out locNumber2))
                            {
                                transfer_loc2 = this.Master.Loader.ModuleManager.FindModule(ModuleTypeEnum.FIXEDTRAY, locNumber2);
                            }
                        }
                    }

                    if (transfer_loc1 != null && transfer_loc2 != null)
                    {
                        AutoFeedAction activeinfo = new AutoFeedAction(transfer_loc1, transfer_loc2, LP_Number1, LP_Number2, data.WaferID[i]);

                        WaferChangeAutofeed.AutoFeedActions.Add(activeinfo);

                        LoggerManager.Debug($"[WaferSupervisor] AllocateActiveCCInfo(): success. Transfer Location1:{activeinfo.Allocate_Loc1.ID}, Location2:{activeinfo.Allocate_Loc2.ID}");
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public List<int> GetFilteredLPNums(List<AutoFeedAction> activeModuleInfos)
        {
            var filteredLPNums = new List<int>();

            foreach (var module in activeModuleInfos)
            {
                // Add LPNum1 if it is greater than 1 and not already in the list
                if (module.LPNum1 >= 1 && !filteredLPNums.Contains(module.LPNum1))
                {
                    filteredLPNums.Add(module.LPNum1);
                }

                // Add LPNum2 if it is greater than 1 and not already in the list
                if (module.LPNum2 >= 1 && !filteredLPNums.Contains(module.LPNum2))
                {
                    filteredLPNums.Add(module.LPNum2);
                }
            }

            return filteredLPNums;
        }

        public bool CSTAutoUnloadAfterWaferChange()
        {
            bool isunloaded = false;
            try
            {
                if (this.Master.GetIsCassetteAutoUnloadAfterLot() == true)
                {
                    var LPlist = GetFilteredLPNums(WaferChangeAutofeed.AutoFeedActions);

                    foreach (var lp in LPlist)
                    {
                        var Cassette = this.Master.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, lp);

                        if (Cassette.FoupState == FoupStateEnum.LOAD)
                        {
                            var retVal = this.FoupOpModule().FoupControllers[lp - 1].Execute(new FoupUnloadCommand());

                            // TODO : retVal의 값이 NONE이 아닌경우

                            // TODO : 필요한가?
                            this.Master.Loader.BroadcastLoaderInfo();
                        }
                    }

                    isunloaded = true;
                }
                else
                {
                    isunloaded =  true;
                }
            }
            catch (Exception err)
            {
                isunloaded = false;
                LoggerManager.Exception(err);
            }
            return isunloaded;
        }
       
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autofac;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.WaferTransfer;
using ProberInterfaces.CardChange;
using ProberErrorCode;
using LoaderControllerBase;

namespace WaferTransfer
{
    using LogModule;
    using ProberInterfaces.State;
    using WaferTransferStates;

    public class WaferTransferModule : IWaferTransferModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.WaferTransfer);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool Initialized { get; set; } = false;

        private List<IWaferTransferProcModule> _Processors = new List<IWaferTransferProcModule>();

        private WaferTransferSystemParam _SystemParam;
        public WaferTransferSystemParam SystemParam
        {
            get { return _SystemParam; }
            set { _SystemParam = value; RaisePropertyChanged(); }
        }
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
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


        public CommandTokenSet RunTokenSet { get; set; }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set { _ModuleState = value; RaisePropertyChanged(); }
        }

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private WaferTransferStateBase _WaferTransferState;
        public WaferTransferStateBase WaferTransferState
        {
            get { return _WaferTransferState; }
        }
        public IInnerState InnerState
        {
            get { return _WaferTransferState; }
            set
            {
                if (value != _WaferTransferState)
                {
                    _WaferTransferState = value as WaferTransferStateBase;
                }
            }
        }
        public IInnerState PreInnerState { get; set; }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set { _TransitionInfo = value; RaisePropertyChanged(); }
        }

        public ILoaderControllerExtension LoaderControllerExt => this.LoaderController() as ILoaderControllerExtension;

        public IWaferTransferProcModule ProcModule { get; set; }
        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        private bool _NeedToRecovery;
        public bool NeedToRecovery
        {
            get { return _NeedToRecovery; }
            set { _NeedToRecovery = value; }
        }

        private bool _StopAfterTransferDone = false;
        public bool StopAfterTransferDone
        {
            get { return _StopAfterTransferDone; }
            set { _StopAfterTransferDone = value; }
        }

        private bool _TransferBrake = false;
        public bool TransferBrake
        {
            get { return _TransferBrake; }
            set { _TransferBrake = value; }
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            return WaferTransferState.CanExecute(token);
        }



        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    //=>temp code
                    SystemParam = new WaferTransferSystemParam();
                    SystemParam.WaferTransferMode = WaferTransferModeEnum.TransferByThreeLeg;
                    //===
                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        _Processors.Add(new WaferLoadProcModule(this));
                        _Processors.Add(new WaferUnloadProcModule(this));
                    }
                    else
                    {
                        _Processors.Add(new GP_WaferLoadProcModule(this));
                        _Processors.Add(new GP_WaferUnloadProcModule(this));

                        if (this.CardChangeModule().GetCCType() == EnumCardChangeType.CARRIER)
                        {
                            _Processors.Add(new GOP_CardLoadProcModule(this));
                            _Processors.Add(new GOP_CardUnLoadProcModule(this));
                        }
                        else if (this.CardChangeModule().GetCCType() == EnumCardChangeType.DIRECT_CARD)
                        {
                            _Processors.Add(new GP_CardLoadProcModule(this));
                            _Processors.Add(new GP_CardUnLoadProcModule(this));
                        }
                        else
                        {
                            LoggerManager.Debug($"InitModule() in {this.GetType().Name} CardChangeType: {this.CardChangeModule().GetCCType()}");
                        }
                    }
                  

                    _WaferTransferState = new IDLE(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    NeedToRecovery = false;
                    this.LoaderController().SetRecoveryMode(NeedToRecovery);

                    this.LoaderController().SetTransferError(false);
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;
                //LoggerManager.Error($ex.Message);
                LoggerManager.Exception(err);

            }

            return retval;
        }
       
        public void SelfRecovery()
        {
            try
            {
                WaferTransferState.SelfRecovery();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferTransferProcStateEnum GetProcModuleState()
        {
            WaferTransferProcStateEnum state = WaferTransferProcStateEnum.IDLE;
            try
            {
                if (this.ProcModule != null)
                {
                    state = this.ProcModule.State;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return state;
        }
        public WaferTransferTypeEnum GetProcModuleTransferType()
        {
            WaferTransferTypeEnum state = WaferTransferTypeEnum.IDLE;
            try
            {
                state = this.ProcModule.TransferType;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return state;
        }


        public void ClearErrorState()
        {
            try
            {
                WaferTransferState.ClearErrorState();
                // ModuleState.Execute(WaferTransferState.ModuleState);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.LoaderController().SetTransferError(false);
                NeedToRecovery = false;
                this.LoaderController().SetRecoveryMode(NeedToRecovery);
                this.StageSupervisor().IsRecoveryMode = false;
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch(Exception err)
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
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;
            try
            {
                
                EventCodeEnum retVal = InnerState.Execute();
                ModuleState.StateTransition(InnerState.GetModuleState());
                RunTokenSet.Update();
                stat = InnerState.GetModuleState();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return stat;
        }


        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (state != null)
                {
                    PreInnerState = InnerState;
                    InnerState = state;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void StateTransition(ModuleStateBase state)
        {
            try
            {
                ModuleState = state;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #region Internal Methods
        public void ActivateProcModule(WaferTransferTypeEnum type)
        {
            try
            {
                WaferTransferModeEnum mode = SystemParam.WaferTransferMode;

                IWaferTransferProcModule procModule = _Processors
                    .Where(item => item.TransferType == type && item.TransferMode == mode)
                    .FirstOrDefault();

                this.ProcModule = procModule;
                this.ProcModule.InitState();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                ModuleStateEnum state = (InnerState as WaferTransferState).GetState;

                retval = state.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public bool IsBusy()
        {
            bool retVal = true;
            try
            {
                //foreach (var subModule in SubModules.SubModules)
                //{
                //    if (subModule.GetState() == SubModuleStateEnum.PROCESSING)
                //    {
                //        retVal = false;
                //    }
                //}
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            try
            {
                msg = "";
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return true;
        }
     
        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }
    }
}

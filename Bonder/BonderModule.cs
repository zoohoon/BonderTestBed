using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Bonder;
using ProberInterfaces.Command;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonder
{
    public class BonderModule : IBonderModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Properties
        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.Bonder);
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

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set { _ModuleState = value; RaisePropertyChanged(); }
        }

        private BonderState _BonderState;
        public BonderState BonderState
        {
            get { return _BonderState; }
        }
        public IInnerState InnerState
        {
            get { return _BonderState; }
            set
            {
                if (value != _BonderState)
                {
                    _BonderState = value as BonderStateBase;
                }
            }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set { _TransitionInfo = value; RaisePropertyChanged(); }
        }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        public IInnerState PreInnerState { get; set; }

        private List<IBonderProcModule> _Processors = new List<IBonderProcModule>();

        public IBonderProcModule ProcModule { get; set; }     // Pick, Place, Rotation처럼 단일모듈 실행일 때
        public List<IBonderProcModule> ProcModules { get; set; }    // Bonder 모듈 전체 실행일 때

        private BonderSystemParam _SystemParam;
        public BonderSystemParam SystemParam
        {
            get { return _SystemParam; }
            set { _SystemParam = value; RaisePropertyChanged(); }
        }

        // Token 체크 대신 사용하는 bool 변수
        private bool _IsFDChuckMove;
        public bool IsFDChuckMove
        {
            get { return _IsFDChuckMove; }
            set
            {
                _IsFDChuckMove = value;
                // RaisePropertyChanged();
            }
        }
        public bool IsWaferChuckMove { get; set; }

        public bool IsPickerDoing { get; set; }

        public bool IsPickerOnlyDoing { get; set; }

        public bool IsFDResume { get; set; }

        public bool IsRotationMove { get; set; }

        public bool IsRotationOnlyMove { get; set; }

        public bool IsPlaceDoing { get; set; }

        public bool IsPlaceOnlyDoing { get; set; }

        public bool IsBonderEnd { get; set; }

        private int _RotationCount = 1;
        public int RotationCount 
        { 
            get { return _RotationCount; }
            set
            {
                _RotationCount = value;
                // RaisePropertyChanged();
            }
        }
        #endregion

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    SystemParam = new BonderSystemParam();
                    SystemParam.BonderTransferMode = BonderModeEnum.BONDER;

                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        _Processors.Add(new BonderStageProcModule(this));
                        _Processors.Add(new PickProcModule(this));
                        _Processors.Add(new RotationProcModule(this));
                        _Processors.Add(new PlaceProcModule(this));
                    }
                    else
                    {

                    }

                    _BonderState = new IDLE(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    this.LoaderController().SetTransferError(false);
                    Initialized = true;

                    this.BonderSupervisor().LoadSysParameter();

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
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void ActivateProcModule(BonderTransferTypeEnum type)
        {
            try
            {
                BonderModeEnum mode = SystemParam.BonderTransferMode;

                IBonderProcModule procModule = _Processors
                    .Where(item => item.TransferType == type && item.TransferMode == mode)
                    .FirstOrDefault();

                this.ProcModule = procModule;
                this.ProcModule.InitState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void ActivateModules()
        {
            this.ProcModules = _Processors;
        }

        public void PickActivateProcModule()
        {

            BonderModeEnum mode = BonderModeEnum.BONDER;
            var type = new[] { BonderTransferTypeEnum.STAGE, BonderTransferTypeEnum.PICKING };

            this.ProcModules = _Processors
                    .Where(item => type.Contains(item.TransferType) && item.TransferMode == mode)
                    .ToList();
        }

        public void PlaceActivateProcModule()
        {

            BonderModeEnum mode = BonderModeEnum.BONDER;
            var type = new[] { BonderTransferTypeEnum.STAGE, BonderTransferTypeEnum.PLACING };

            this.ProcModules = _Processors
                    .Where(item => type.Contains(item.TransferType) && item.TransferMode == mode)
                    .ToList();
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
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            return BonderState.CanExecute(token);
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
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return stat;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
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
        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
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
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
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
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }
        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //this.LoaderController().SetTransferError(false);
                //NeedToRecovery = false;
                //this.LoaderController().SetRecoveryMode(NeedToRecovery);
                //this.StageSupervisor().IsRecoveryMode = false;
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                ModuleStateEnum state = (InnerState as BonderState).GetState;

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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
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
        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            try
            {
                msg = "";
            }
            catch (Exception err)
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

        int tempcount = 1;
        public EventCodeEnum IsDieExist()
        {
            // FD Wafer에 남은 다이가 있는지 체크하는 함수
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            LoggerManager.Debug("[Bonder] (Place) check = Die exist on FD Stage?");

            if (tempcount == 1)     // 임시 회전 반복용, 일단 1사이클로
            {
                ret = EventCodeEnum.NONE;
            }
            else
            {
                tempcount++;
                ret = EventCodeEnum.NODATA;
            }
            return ret;
        }
    }
    public class ShareInstance
    {
        public static BonderModule bonderModule = new BonderModule();
    }
}

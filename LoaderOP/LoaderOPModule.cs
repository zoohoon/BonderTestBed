using ProberInterfaces;
using SequenceService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ProberInterfaces.Foup;
using ProberInterfaces.Command;
using ProberErrorCode;
using ProberInterfaces.State;
using ProberInterfaces.Wizard;
using LogModule;
using System.Runtime.CompilerServices;
using SubstrateObjects;
using ProberInterfaces.Enum;

namespace LoaderOP
{
    public class LoaderOPModule : SequenceServiceBase, ILoaderOPModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private class WaferInfo : IWaferInfo
        {
            public EnumWaferSize WaferSize { get; set; }
            public double Notchangle { get; set; }
            public double WaferThickness { get; set; }
            public OCRTypeEnum OCRType { get; set; }
            public OCRDirectionEnum OCRDirection { get; set; }
            public OCRModeEnum OCRMode { get; set; }
        }

        public bool Initialized { get; set; } = false;

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
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
                    RaisePropertyChanged();
                }
            }
        }

        public List<IStateModule> RunList { get; set; }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
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

        private ModuleStateEnum _CurrentModuleState;
        public ModuleStateEnum CurrentModuleState
        {
            get { return _CurrentModuleState; }
            set
            {
                if (value != _CurrentModuleState)
                {
                    _CurrentModuleState = value;
                    RaisePropertyChanged();
                }
            }
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

                    CurrentModuleState = _ModuleState.GetState();

                    RaisePropertyChanged();
                }
            }
        }
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }

        private LoaderOPState _LoaderOPModuleState;

        public LoaderOPState LoaderOPModuleState
        {
            get { return _LoaderOPModuleState; }
        }

        public IInnerState InnerState
        {
            get { return _LoaderOPModuleState; }
            set
            {
                if (value != _LoaderOPModuleState)
                {
                    _LoaderOPModuleState = value as LoaderOPState;
                }
            }
        }

        public IInnerState PreInnerState { get; set; }


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

        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }
        private SubStepModules _SubModules;
        public ISubStepModules SubModules
        {
            get { return _SubModules; }
            set
            {
                if (value != _SubModules)
                {
                    _SubModules = (SubStepModules)value;
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

        private bool _TransferReservationAboutPolishWafer = false;
        public bool TransferReservationAboutPolishWafer
        {
            get { return _TransferReservationAboutPolishWafer; }
            set { _TransferReservationAboutPolishWafer = value; }
        }


        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            bool retVal = false;
            try
            {
                foreach (var subModule in SubModules.SubModules)
                {
                    if (subModule.GetMovingState() == MovingStateEnum.MOVING)
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum RetVal = ModuleStateEnum.UNDEFINED;

            try
            {
                RetVal = Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }


        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;

            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retVal = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());

                retVal = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());

                retVal = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public ModuleStateEnum Abort()
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());

                retVal = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
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

                this.MonitoringManager().LoaderEmergencyStop();
            }

            return stat;
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
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            IStageSupervisor stageSupervisor = this.StageSupervisor();
            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    _LoaderOPModuleState = new LoaderOPIdleState(this);

                    RunList = new List<IStateModule>();
                    RunList.Add(this.LoaderController());

                    ModuleState = new ModuleUndefinedState(this);

                    Initialized = true;
                    stageSupervisor.ChangedWaferObjectEvent += OnChangedWaferObjectFunc;
                    stageSupervisor.WaferObject.ChangedWaferObjectEvent += OnChangedWaferObjectFunc;
                    stageSupervisor.WaferObject.CallWaferobjectChangedEvent();

                    //stageSupervisor.ChangedWaferObjectEvent += OnChangedWaferObjectFunc;
                    //stageSupervisor.CallWaferobjectChangedEvent();

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
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void OnChangedWaferObjectFunc(object sender, EventArgs e)
        {
            try
            {
                EventCodeEnum SaveResult = EventCodeEnum.UNDEFINED;
                if (e is WaferObjectEventArgs)
                {
                    IWaferInfo waferInfo = GetWaferInfo(e as WaferObjectEventArgs);

                    SaveResult = this.LoaderController().SetWaferInfo(waferInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private WaferInfo GetWaferInfo(WaferObjectEventArgs waferObjArgs)
        {
            WaferInfo waferInfo = null;
            try
            {
                WaferObject waferObj = waferObjArgs?.WaferObject as WaferObject;
                EnumWaferSize waferSize = waferObj?.GetPhysInfo()?.WaferSizeEnum ?? EnumWaferSize.UNDEFINED;
                double? angle = waferObj?.GetPhysInfo()?.NotchAngle.Value;
                double? thickness = waferObj?.GetPhysInfo()?.Thickness.Value;
                OCRTypeEnum OCRType = waferObj.GetPhysInfo().OCRType.Value;
                OCRModeEnum OCRMode = waferObj.GetPhysInfo().OCRMode.Value;
                OCRDirectionEnum OCRDirection = waferObj.GetPhysInfo().OCRDirection.Value;

                waferInfo = new WaferInfo()
                {
                    WaferSize = waferSize,
                    Notchangle = angle.Value,
                    WaferThickness = thickness.Value,
                    OCRType = OCRType,
                    OCRMode = OCRMode,
                    OCRDirection = OCRDirection
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return waferInfo;
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
            }

            return retVal;
        }


        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;

            try
            {
                RetVal = LoaderOPModuleState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
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

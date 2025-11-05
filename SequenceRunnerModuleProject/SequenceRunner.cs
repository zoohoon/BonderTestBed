using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Command;
using ProberInterfaces.SequenceRunner;
using ProberInterfaces.State;
using SequenceService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LogModule;
using ProberInterfaces.Event;
using NotifyEventModule;
using SequenceRunner;

namespace SequenceRunnerModule
{
    public class SequenceRunner : SequenceServiceBase, ISequenceRunner, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public CommandSlot CommandSendSlot { get; set; } = new CommandSlot();


        private CommandSlot _CommandRecvSlot = new CommandSlot();
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set { _CommandRecvSlot = value; }
        }
        
        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.SequenceRunner);
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


        public CommandTokenSet RunTokenSet { get; set; } = new CommandTokenSet();

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (_ModuleState != value)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<TransitionInfo> TransitionInfo { get; set; }
            = new ObservableCollection<TransitionInfo>();

        private SequenceRunnerState SequenceRunnerState = null;

        public IInnerState SuspendedBeforeInnerState { get; set; } = null;
        public IInnerState ErrorBeforeInnerState { get; set; } = null;
        public IInnerState PreInnerState { get; set; } = null;
        public IInnerState InnerState
        {
            get
            {
                return SequenceRunnerState;
            }
            set
            {
                if (SequenceRunnerState != value)
                {
                    SequenceRunnerState = value as SequenceRunnerState;
                    RaisePropertyChanged();
                }
            }
        }
        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }
        public IInnerState CauseOfErrorInnerState { get; set; } = null;

        private SynchronizedObservableCollection<ISequenceBehaviorGroupItem> _StepGroupCollection;
        public SynchronizedObservableCollection<ISequenceBehaviorGroupItem> StepGroupCollection
        {
            get { return _StepGroupCollection; }
            set
            {
                if (value != _StepGroupCollection)
                {
                    _StepGroupCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private SynchronizedObservableCollection<IOPortDescripter<bool>> _InputPorts
        //    = new SynchronizedObservableCollection<IOPortDescripter<bool>>();
        //public SynchronizedObservableCollection<IOPortDescripter<bool>> InputPorts
        //{
        //    get { return _InputPorts; }
        //    set
        //    {
        //        if (value != _InputPorts)
        //        {
        //            _InputPorts = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool _RunEnable;
        public bool RunEnable
        {
            get { return _RunEnable; }
            set
            {
                _RunEnable = value;
                RaisePropertyChanged();
            }
        }

        private bool _ErrorVisible;
        public bool ErrorVisible
        {
            get { return _ErrorVisible; }
            set
            {
                _ErrorVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool _DirectRunEnable;
        public bool DirectRunEnable
        {
            get { return _DirectRunEnable; }
            set
            {
                _DirectRunEnable = value;
                RaisePropertyChanged();
            }
        }

        private int _CCStartPoint;
        public int CCStartPoint
        {
            get { return _CCStartPoint; }
            set
            {
                if (value != _CCStartPoint)
                {
                    _CCStartPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedBehaviorIdx;
        public int SelectedBehaviorIdx
        {
            get { return _SelectedBehaviorIdx; }
            set
            {
                _SelectedBehaviorIdx = value;
                RaisePropertyChanged();
            }
        }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        internal string NextID = "";

        public bool CanExecute(IProbeCommandToken token)
        {
            return false;
        }

        public EventCodeEnum ClearState()
        {
            return EventCodeEnum.NONE;
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

                PreInnerState = SequenceRunnerState;
                InnerState = state;

                if (InnerState.GetModuleState() != ModuleStateEnum.RUNNING
                    && InnerState.GetModuleState() != ModuleStateEnum.ERROR
                    && InnerState.GetModuleState() != ModuleStateEnum.RECOVERY
                    )
                {
                    RunEnable = true;
                }
                else
                {
                    RunEnable = false;

                    if (InnerState.GetModuleState() == ModuleStateEnum.ERROR)
                    {
                        ErrorVisible = true;
                        if (ErrorBeforeInnerState == null)
                            ErrorBeforeInnerState = PreInnerState;
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

        public ModuleStateEnum End()
        {
            ModuleStateEnum retVal;
            try
            {
                SequenceRunnerState.End();
                retVal = SequenceRunnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        internal bool IsExecutting { get; set; } = false;
        internal SequenceRunnerExecuteController SequenceRunnerExecuteCtrl { get; set; }
            = new SequenceRunnerExecuteController_Idle();
        internal SequenceRunnerExecuteController_Idle executeIdle { get; set; }
         = new SequenceRunnerExecuteController_Idle();
        internal SequenceRunnerExecuteController_Running executeRunning { get; set; }
         = new SequenceRunnerExecuteController_Running();
        public ModuleStateEnum Execute()
        {
            ModuleStateEnum moduleStateEnum = ModuleStateEnum.ERROR;
            try
            {
                SequenceRunnerExecuteCtrl.Run(this, SequenceRunnerState);
                ModuleState.StateTransition(SequenceRunnerState.GetModuleState());
                moduleStateEnum = ModuleState.GetState();
            }
            catch (Exception err)
            {
                IsExecutting = false;
                LoggerManager.Exception(err);
                throw;
            }
            return moduleStateEnum;
        }


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ModuleState = new ModuleInitState(this);
                    InnerStateTransition(new SequenceRunnerIdleState(this));

                    RunEnable = true;
                    ErrorVisible = false;
                    DirectRunEnable = false;

                    Initialized = true;

                    retval = this.EventManager().RegisterEvent(typeof(SetFrontDoorTempEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(StopFrontDoorTempEvent).FullName, "ProbeEventSubscibers", EventFired);
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

        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is SetFrontDoorTempEvent)
                {
                    SequenceRunnerState.frontDoorTempEventFlag = true;
                }
                else if (sender is StopFrontDoorTempEvent)
                {
                    SequenceRunnerState.frontDoorTempStopEventFlag = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
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
            return false;
        }

        public ModuleStateEnum Pause()
        {
            ModuleStateEnum moduleStateEnum;
            try
            {

                moduleStateEnum = ModuleStateEnum.IDLE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moduleStateEnum;
        }

        public ModuleStateEnum Abort()
        {
            ModuleStateEnum moduleStateEnum;
            try
            {

                moduleStateEnum = ModuleStateEnum.IDLE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moduleStateEnum;
        }

        public ModuleStateEnum Resume()
        {
            ModuleStateEnum moduleStateEnum;
            try
            {

                moduleStateEnum = ModuleStateEnum.IDLE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moduleStateEnum;
        }

        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum moduleStateEnum = ModuleStateEnum.IDLE;
            try
            {
                moduleStateEnum = this.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moduleStateEnum;
        }

        public void StateTransition(ModuleStateBase state)
        {
            ModuleState = state;
        }

        public EventCodeEnum RunCardChange(bool IsTempSetBeforeOperation)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunnerState.RunCardChange(IsTempSetBeforeOperation);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum RunTestHeadDockUndock(THDockType type, bool IsTempSetBeforeOperation)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = SequenceRunnerState.RunTestHeadDockUndock(type, IsTempSetBeforeOperation);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum RunNcPadChage(bool IsTempSetBeforeOperation)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunnerState.RunNCPadChange(IsTempSetBeforeOperation);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum RunRetry()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = SequenceRunnerState.RunRetry();
            return retVal;
        }

        public EventCodeEnum RunReverse()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunnerState.RunReverse();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum AlternateManualMode()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunnerState.AlternateManualMode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum RunManualRetry()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = SequenceRunnerState.RunManualRetry();
            return retVal;
        }

        public EventCodeEnum RunManualReverse()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SequenceRunnerState.RunManualReverse();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public SequenceRunnerStateEnum GetSequenceRunnerStateEnum()
        {
            SequenceRunnerStateEnum retState = SequenceRunnerStateEnum.IDLE;
            try
            {
                retState = SequenceRunnerState.GetSequenceRunnerStateEnum();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retState;
        }

        public object GetSelectedSequence()
        {
            return this.StepGroupCollection;
        }
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
            return true;
        }
  
        public IList<IOPortDescripter<bool>> GetInputPorts(IList<ISequenceBehaviorGroupItem> behaviorGroupItems)
        {
            SynchronizedObservableCollection<IOPortDescripter<bool>> retIOPorts = null;

            if (behaviorGroupItems != null)
            {
                HashSet<IOPortDescripter<bool>> hashInputPorts = new HashSet<IOPortDescripter<bool>>();
                foreach (ISequenceBehaviorGroupItem item in behaviorGroupItems)
                {
                    foreach (ISequenceBehaviorSafety safety in item.IPreSafetyList)
                    {
                        foreach (IOPortDescripter<bool> io in safety.InputPorts)
                        {
                            hashInputPorts.Add(io);
                        }
                    }

                    foreach (IOPortDescripter<bool> io in item.IBehavior.InputPorts)
                    {
                        hashInputPorts.Add(io);
                    }

                    foreach (ISequenceBehaviorSafety safety in item.IPostSafetyList)
                    {
                        foreach (IOPortDescripter<bool> io in safety.InputPorts)
                        {
                            hashInputPorts.Add(io);
                        }
                    }
                }

                retIOPorts = new SynchronizedObservableCollection<IOPortDescripter<bool>>(hashInputPorts?.ToList());
            }

            return retIOPorts;
        }

        public bool InitStepGroupCollection(IList<ISequenceBehaviorGroupItem> behaviorGroupItems)
        {
            bool initResult = false;

            try
            {
                if (behaviorGroupItems != null)
                {
                    foreach (var v in behaviorGroupItems)
                    {
                        v.InitState();
                    }
                    initResult = true;
                }
                else
                {
                    initResult = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return initResult;
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

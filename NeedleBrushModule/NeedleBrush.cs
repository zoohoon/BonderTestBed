using LogModule;
using NeedleBrushParamObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.NeedleBrush;
using ProberInterfaces.State;
using ProberInterfaces.Template;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NeedleBrushModule
{
    public class NeedleBrush : INeedleBrushModule, INotifyPropertyChanged, IHasDevParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public bool Initialized { get; set; } = false;

        #region //..Template
        private TemplateStateCollection _Template;
        public TemplateStateCollection Template
        {
            get { return _Template; }
            set
            {
                if (value != _Template)
                {
                    _Template = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ITemplateFileParam _TemplateParameter;
        public ITemplateFileParam TemplateParameter
        {
            get { return _TemplateParameter; }
            set
            {
                if (value != _TemplateParameter)
                {
                    _TemplateParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ITemplateParam LoadTemplateParam { get; set; }
        public ISubRoutine SubRoutine { get; set; }

        #endregion

        private NeedleBrushState _NeedleBrushState;
        public IInnerState InnerState
        {
            get { return _NeedleBrushState; }
            set
            {
                if (value != _NeedleBrushState)
                {
                    _NeedleBrushState = value as NeedleBrushState;
                    RaisePropertyChanged();
                }
            }
        }

        public IInnerState PreInnerState
        {
            get;
            set;
        }

        private ReasonOfError _ReasonOfError;
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

        private CommandSlot _CommandSendSlot;
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set
            {
                if (value != _CommandSendSlot)
                {
                    _CommandSendSlot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CommandSlot _CommandRecvSlot;
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set
            {
                if (value != _CommandRecvSlot)
                {
                    _CommandRecvSlot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CommandSlot _CommandRecvProcSlot;
        public CommandSlot CommandRecvProcSlot
        {
            get { return _CommandRecvProcSlot; }
            set
            {
                if (value != _CommandRecvProcSlot)
                {
                    _CommandRecvProcSlot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CommandSlot _CommandRecvDoneSlot;
        public CommandSlot CommandRecvDoneSlot
        {
            get { return _CommandRecvDoneSlot; }
            set
            {
                if (value != _CommandRecvDoneSlot)
                {
                    _CommandRecvDoneSlot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CommandTokenSet _RunTokenSet;
        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set
            {
                if (value != _RunTokenSet)
                {
                    _RunTokenSet = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set
            {
                if (value != _CommandInfo)
                {
                    _CommandInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo;
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
            set
            {
                if (value != _ForcedDone)
                {
                    _ForcedDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NeedleBrush()
        {

        }
        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;

            try
            {
                RetVal = _NeedleBrushState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum LoadDevParameter() // Parameter Type만 변경
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;

                //tmpParam = new PMIModuleDevParam();
                //tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                //retval = this.LoadParameter(ref tmpParam, typeof(PMIModuleDevParam));

                //if (retval == EventCodeEnum.NONE)
                //{
                //    PMIModuleDevParam_IParam = tmpParam;
                //    //PMIModuleDevParam_Clone = PMIModuleDevParam_IParam as PMIModuleDevParam;
                //}

                tmpParam = new NeedleBrushTemplateFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(NeedleBrushTemplateFile));

                if (retval == EventCodeEnum.NONE)
                {
                    TemplateParameter = tmpParam as NeedleBrushTemplateFile;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveDevParameter() // Don`t Touch
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(TemplateParameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                PreInnerState = _NeedleBrushState;
                InnerState = state;
            }
            catch (Exception err)
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
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum Execute()
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

        public EventCodeEnum ClearState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());

                List<ISubModule> modules = Template.GetProcessingModule();
                for (int index = 0; index < modules.Count; index++)
                {
                    retVal = modules[index].ClearData();
                }
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
                NeedleBrushStateEnum state = (InnerState as NeedleBrushState).GetState();

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
            bool retVal = false;

            try
            {
                List<ISubModule> modules = Template.GetProcessingModule();
                foreach (var subModule in modules)
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    _ReasonOfError = new ReasonOfError(ModuleEnum.NeedleBrush);
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    InnerState = new NeedleBrushIdleState(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    retval = this.TemplateManager().InitTemplate(this);

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
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsLotReady(out string msg)
        {
            // TODO: 로직 확인
            bool retVal = false;
            msg = null;

            try
            {
                if (this.Template.GetProcessingModule() == null)
                {
                    msg = Properties.Resources.CheckTemplateErrorMessage;
                    retVal = false;
                    return retVal;
                }

                foreach (var module in this.Template.GetProcessingModule())
                {
                    if (module is ILotReadyAble)
                    {
                        retVal = (module as ILotReadyAble).IsLotReady(out msg);
                        if (!retVal)
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            // TODO: Test Code
            retVal = true;

            return retVal;

        }

        public EventCodeEnum ParamValidation()
        {
            // TODO: 로직 확인

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }

    }
}

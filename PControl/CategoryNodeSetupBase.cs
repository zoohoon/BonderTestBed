using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnPontrol
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.State;
    using ProberInterfaces.Wizard;
    using States;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Controls;
    using System.Xml.Serialization;

    [Serializable, DataContract]
    public abstract class CategoryNodeSetupBase : ICategoryNodeItem, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //.. Property
        public abstract bool Initialized { get; set; }
        private Guid _PageGUID;

        public Guid PageGUID
        {
            get { return _PageGUID; }
            set { _PageGUID = value; }
        }

        private string _Header;
        [XmlAttribute("Name")]
        public string Header
        {
            get { return _Header; }
            set
            {
                if (value != _Header)
                {
                    _Header = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _RecoveryHeader;
        public string RecoveryHeader
        {
            get { return _RecoveryHeader; }
            set
            {
                if (value != _RecoveryHeader)
                {
                    _RecoveryHeader = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private EnableStateBase _EnableState
        //     = new EnableIdleState();
        //[XmlIgnore, JsonIgnore]
        //public EnableStateBase EnableState
        //{
        //    get { return _EnableState; }
        //    set { _EnableState = value; }
        //}
        private EnableStateBase _EnableState
            = new EnableState();
        [XmlIgnore, JsonIgnore]
        public EnableStateBase EnableState
        {
            get { return _EnableState; }
            set { _EnableState = value; }
        }




        private EnumEnableState _StateEnable;
        public EnumEnableState StateEnable
        {
            get
            {
                return EnableState.GetEnableState();
            }
            set
            {
                if (value != _StateEnable)
                {
                    _StateEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SetupStateBase _SetupState
               = new NotCompletedState();
        [XmlIgnore, JsonIgnore]
        public SetupStateBase SetupState
        {
            get { return _SetupState; }
            set
            {
                if (value != _SetupState)
                {
                    _SetupState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private EnumMoudleSetupState _StateSetup;
        public EnumMoudleSetupState StateSetup
        {
            get
            {
                return SetupState.GetState();
            }
            set
            {
                if (value != _StateSetup)
                {
                    _StateSetup = value;
                    RaisePropertyChanged();
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetupStateHeaderUpdated(_StateSetup, IsParent,Header, RecoveryHeader);
                }
            }
        }

        private EnumMoudleSetupState _StateRecoverySetup;
        public EnumMoudleSetupState StateRecoverySetup
        {
            get { return _StateRecoverySetup; }
            set
            {
                if (value != _StateRecoverySetup)
                {
                    _StateRecoverySetup = value;
                    RaisePropertyChanged();
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.SetupRecoveryStateHeaderUpdated(_StateSetup, IsParent, Header, RecoveryHeader);
                }
            }
        }

        private ICategoryNodeItem _Parent;
        [ParamIgnore]
        public ICategoryNodeItem Parent
        {
            get { return _Parent; }
            set
            {
                if (value != _Parent)
                {
                    _Parent = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsParent
        {
            get { return Categories.Count == 0 ? false : true; }
        }

        //private ObservableCollection<ICategoryNodeItem> _Categories
        //     = new ObservableCollection<ICategoryNodeItem>();
        //public ObservableCollection<ICategoryNodeItem> Categories
        //{
        //    get { return _Categories; }
        //    set
        //    {
        //        if (value != _Categories)
        //        {
        //            _Categories = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<ITemplateModule> _Categories
             = new ObservableCollection<ITemplateModule>();
        public ObservableCollection<ITemplateModule> Categories
        {
            get { return _Categories; }
            set
            {
                if (value != _Categories)
                {
                    _Categories = value;
                    RaisePropertyChanged();
                }
            }
        }

        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        private bool _NoneCleanUp = false;
        public bool NoneCleanUp
        {
            get { return _NoneCleanUp; }
            set
            {
                if (value != _NoneCleanUp)
                {
                    _NoneCleanUp = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public abstract EventCodeEnum ParamValidation();

        public virtual void SetEnableState(EnumEnableState state)
        {
            try
            {
                switch (state)
                {
                    case EnumEnableState.ENABLE:
                        EnableState = (EnableStateBase)EnableState.SetEnable(EnableState);
                        break;
                    case EnumEnableState.MUST:
                        EnableState = (EnableStateBase)EnableState.SetMust(EnableState);
                        break;
                    case EnumEnableState.MUSTNOT:
                        EnableState = (EnableStateBase)EnableState.SetMustNot(EnableState);
                        break;
                    case EnumEnableState.DISABLE:
                        EnableState = (EnableStateBase)EnableState.SetDisable(EnableState);
                        break;
                }
                StateEnable = EnableState.GetEnableState();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

        }

        public EnumMoudleSetupState GetModuleSetupState()
        {
            return SetupState.GetState();
        }
        public void SetNodeSetupState(EnumMoudleSetupState state, bool isparent = false)
        {
            try
            {
                if (isparent)
                {
                    if(Parent != null)
                        Parent.SetNodeSetupState(state);
                    return;
                }

                //if (SetupState == null)
                //    SetupState = new NotCompletedState(this);
                if (SetupState._Module == null)
                    SetupState._Module = this;

                switch (state)
                {
                    case EnumMoudleSetupState.COMPLETE:
                        SetupState.SetComplete();
                        break;
                    case EnumMoudleSetupState.NOTCOMPLETED:
                        SetupState.SetNotCompleted();
                        break;
                    case EnumMoudleSetupState.VERIFY:
                        SetupState.SetVerify();
                        break;
                    case EnumMoudleSetupState.NONE:
                        SetupState.SetNone();
                        break;
                }
                StateSetup = SetupState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw err;
            }
        }

        public void SetNodeSetupRecoveryState(EnumMoudleSetupState state, bool isparent = false)
        {
            try
            {
                if (isparent)
                {
                    if (Parent != null)
                        Parent.SetNodeSetupState(state);
                    return;
                }

                //if (SetupState == null)
                //    SetupState = new NotCompletedState(this);
                if (SetupState._Module == null)
                    SetupState._Module = this;

                switch (state)
                {
                    case EnumMoudleSetupState.COMPLETE:
                        SetupState.SetComplete();
                        break;
                    case EnumMoudleSetupState.NOTCOMPLETED:
                        SetupState.SetNotCompleted();
                        break;
                    case EnumMoudleSetupState.VERIFY:
                        SetupState.SetVerify();
                        break;
                    case EnumMoudleSetupState.NONE:
                        SetupState.SetNone();
                        break;
                }
                StateSetup = SetupState.GetState();
                StateRecoverySetup = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw err;
            }
        }
        public void ChangeSetupState(IMoudleSetupState state)
        {
            SetupState = state as SetupStateBase;
            StateSetup = SetupState.GetState();
        }

        public abstract Task<EventCodeEnum> Cleanup(object parameter = null);
        public virtual EventCodeEnum ClearSettingData()
        {
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
        }
        public virtual EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retval;
        }
        public virtual Guid ScreenGUID { get; }
        public virtual Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public virtual Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public abstract bool IsParameterChanged(bool issave = false);

        public abstract void SetStepSetupState(string header = null);
    }

    [DataContract]
    public class CategoryNodeItem : ITemplateModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //.. Property

        public bool Initialized { get; set; }

        private Guid _PageGUID;

        public Guid PageGUID
        {
            get { return _PageGUID; }
            set { _PageGUID = value; }
        }

        private string _Header;
        [XmlAttribute("Name")]
        public string Header
        {
            get { return _Header; }
            set
            {
                if (value != _Header)
                {
                    _Header = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnableStateBase _EnableState
            = new EnableState();
        [XmlIgnore, JsonIgnore]
        public EnableStateBase EnableState
        {
            get { return _EnableState; }
            set { _EnableState = value; }
        }




        private EnumEnableState _StateEnable;
        public EnumEnableState StateEnable
        {
            get
            {
                return EnableState.GetEnableState();
            }
            set
            {
                if (value != _StateEnable)
                {
                    _StateEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SetupStateBase _SetupState
               = new NotCompletedState();
        [XmlIgnore]
        public SetupStateBase SetupState
        {
            get { return _SetupState; }
            set
            {
                if (value != _SetupState)
                {
                    _SetupState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private EnumMoudleSetupState _StateSetup;
        public EnumMoudleSetupState StateSetup
        {
            get
            {
                return SetupState.GetState();
            }
            set
            {
                if (value != _StateSetup)
                {
                    _StateSetup = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumMoudleSetupState _StateDevSetup;
        public EnumMoudleSetupState StateDevSetup
        {
            get { return _StateDevSetup; }
            set
            {
                if (value != _StateDevSetup)
                {
                    _StateDevSetup = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICategoryNodeItem _Parent;
        [ParamIgnore]
        public ICategoryNodeItem Parent
        {
            get { return _Parent; }
            set
            {
                if (value != _Parent)
                {
                    _Parent = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ICategoryNodeItem> _Categories
             = new ObservableCollection<ICategoryNodeItem>();
        public ObservableCollection<ICategoryNodeItem> Categories
        {
            get { return _Categories; }
            set
            {
                if (value != _Categories)
                {
                    _Categories = value;
                    RaisePropertyChanged();
                }
            }
        }

        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        private bool _NoneCleanUp = false;
        public bool NoneCleanUp
        {
            get { return _NoneCleanUp; }
            set
            {
                if (value != _NoneCleanUp)
                {
                    _NoneCleanUp = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region //..Creator

        public CategoryNodeItem()
        {

        }
        public CategoryNodeItem(string header)
        {
            Header = header;
        }
        public CategoryNodeItem(ICategoryNodeItem parent, string header)
        {
            Parent = parent;
            Header = header;
        }
        #endregion

        #region //..Method
        public EventCodeEnum ClearSettingData()
        {
            return EventCodeEnum.NONE;
        }

        public void SetEnableState(EnumEnableState state)
        {
            throw new NotImplementedException();
        }

        public void ChangeSetupState(IMoudleSetupState state)
        {
            throw new NotImplementedException();
        }

        public void SetStepSetupState(string header = null)
        {
            return;
        }

        public void DeInitModule()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return true;
        }
        #endregion

        public  Guid ViewModelGUID { get; }
        public virtual Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public virtual Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }

    public abstract class WizardStepBase : CategoryNodeSetupBase, IWizardStep, INotifyPropertyChanged
    {
        //public abstract bool Initialized { get; set; }

        #region //..

        private UserControl _UCDetailSummary;
        [XmlIgnore, JsonIgnore]
        public UserControl UCDetailSummary
        {
            get { return _UCDetailSummary; }
            set
            {
                if (value != _UCDetailSummary)
                {
                    _UCDetailSummary = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        public virtual ObservableCollection<IWizardStep> GetWizardStep()
        {
            return null;
        }

        public void SetHeader(string name)
        {
            Header = name;
        }

        public override Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public new void DeInitModule()
        {
        }

        public override void SetStepSetupState(string header = null)
        {

        }

    }

}

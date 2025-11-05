using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeedleCleanRecipeSettingVM
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using LogModule;
    using NeedleCleanerModuleParameter;
    using ProberErrorCode;
    using ProberInterfaces;
    using RecipeEditorControl.RecipeEditorParamEdit;
    using RelayCommandBase;
    using SubstrateObjects;
    using VirtualKeyboardControl;

    public class NeedleCleanRecipeSettingViewModel : IMainScreenViewModel, IParamScrollingViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region ==> NCEnableClickCommand
        private RelayCommand<Object> _NCEnableClickCommand;

        public ICommand NCEnableClickCommand
        {
            get
            {
                if (null == _NCEnableClickCommand) _NCEnableClickCommand = new RelayCommand<Object>(NCEnableClickCommandFunc);
                return _NCEnableClickCommand;
            }
        }
        private void NCEnableClickCommandFunc(Object param)
        {
            try
            {
                SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> TextBoxClickCommand
        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }
        private void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> NCSequenceChangedCommand
        private RelayCommand _NCSequenceChangedCommand;
        public ICommand NCSequenceChangedCommand
        {
            get
            {
                if (null == _NCSequenceChangedCommand) _NCSequenceChangedCommand = new RelayCommand(NCSequenceChangedCommandFunc);
                return _NCSequenceChangedCommand;
            }
        }
        private void NCSequenceChangedCommandFunc()
        {
            try
            {
                SaveDevParameter();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> SequenceSetupCommand_NC1
        private RelayCommand<CUI.Button> _SequenceSetupCommand_NC1;
        public ICommand SequenceSetupCommand_NC1
        {
            get
            {
                if (null == _SequenceSetupCommand_NC1) _SequenceSetupCommand_NC1 = new RelayCommand<CUI.Button>(SequenceSetupCommandFunc_NC1);
                return _SequenceSetupCommand_NC1;
            }
        }
        private void SequenceSetupCommandFunc_NC1(CUI.Button cuiparam)
        {
            try
            {
                Guid viewguid = new Guid();
                List<Guid> pnpsteps = new List<Guid>();

                this.PnPManager().GetCuiBtnParam(this.NeedleCleaner(), cuiparam.GUID, out viewguid, out pnpsteps);

                if (pnpsteps.Count != 0)
                {
                    ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index = 0;
                    this.PnPManager().SetNavListToGUIDs(this.NeedleCleaner(), pnpsteps);
                    this.ViewModelManager().ViewTransitionAsync(viewguid);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> SequenceSetupCommand_NC2
        private RelayCommand<CUI.Button> _SequenceSetupCommand_NC2;
        public ICommand SequenceSetupCommand_NC2
        {
            get
            {
                if (null == _SequenceSetupCommand_NC2) _SequenceSetupCommand_NC2 = new RelayCommand<CUI.Button>(SequenceSetupCommandFunc_NC2);
                return _SequenceSetupCommand_NC2;
            }
        }
        private void SequenceSetupCommandFunc_NC2(CUI.Button cuiparam)
        {
            try
            {
                Guid viewguid = new Guid();
                List<Guid> pnpsteps = new List<Guid>();
                this.PnPManager().GetCuiBtnParam(this.NeedleCleaner(), cuiparam.GUID, out viewguid, out pnpsteps);
                if (pnpsteps.Count != 0)
                {
                    ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index = 1;
                    this.PnPManager().SetNavListToGUIDs(this.NeedleCleaner(), pnpsteps);
                    this.ViewModelManager().ViewTransitionAsync(viewguid);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> SequenceSetupCommand_NC3
        private RelayCommand<CUI.Button> _SequenceSetupCommand_NC3;
        public ICommand SequenceSetupCommand_NC3
        {
            get
            {
                if (null == _SequenceSetupCommand_NC3) _SequenceSetupCommand_NC3 = new RelayCommand<CUI.Button>(SequenceSetupCommandFunc_NC3);
                return _SequenceSetupCommand_NC3;
            }
        }
        private void SequenceSetupCommandFunc_NC3(CUI.Button cuiparam)
        {
            try
            {
                Guid viewguid = new Guid();
                List<Guid> pnpsteps = new List<Guid>();
                this.PnPManager().GetCuiBtnParam(this.NeedleCleaner(), cuiparam.GUID, out viewguid, out pnpsteps);
                if (pnpsteps.Count != 0)
                {
                    ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index = 2;
                    this.PnPManager().SetNavListToGUIDs(this.NeedleCleaner(), pnpsteps);
                    this.ViewModelManager().ViewTransitionAsync(viewguid);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> NC1SelectedChangedCommand
        private RelayCommand _NC1SelectedChangedCommand;
        public ICommand NC1SelectedChangedCommand
        {
            get
            {
                if (null == _NC1SelectedChangedCommand) _NC1SelectedChangedCommand = new RelayCommand(NC1SelectedChangedCommandFunc);
                return _NC1SelectedChangedCommand;
            }
        }
        private void NC1SelectedChangedCommandFunc()
        {
            try
            {
                ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> NC2SelectedChangedCommand
        private RelayCommand _NC2SelectedChangedCommand;
        public ICommand NC2SelectedChangedCommand
        {
            get
            {
                if (null == _NC2SelectedChangedCommand) _NC2SelectedChangedCommand = new RelayCommand(NC2SelectedChangedCommandFunc);
                return _NC2SelectedChangedCommand;
            }
        }
        private void NC2SelectedChangedCommandFunc()
        {
            try
            {
                ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> NC3SelectedChangedCommand
        private RelayCommand _NC3SelectedChangedCommand;
        public ICommand NC3SelectedChangedCommand
        {
            get
            {
                if (null == _NC3SelectedChangedCommand) _NC3SelectedChangedCommand = new RelayCommand(NC3SelectedChangedCommandFunc);
                return _NC3SelectedChangedCommand;
            }
        }
        private void NC3SelectedChangedCommandFunc()
        {
            try
            {
                ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index = 2;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private readonly Guid _ViewModelGUID = new Guid("377bfd55-081b-4f46-9c48-5e20ec3e20e9");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public IHasDevParameterizable NeedleCleanModule
        {
            get
            {
                return this.NeedleCleaner();
            }
        }
        public NeedleCleanDeviceParameter NeedleCleanDevParam { get; set; }


        #region ==> RecipeEditorParamEdit
        private RecipeEditorParamEditViewModel _RecipeEditorParamEdit;
        public RecipeEditorParamEditViewModel RecipeEditorParamEdit
        {
            get { return _RecipeEditorParamEdit; }
            set
            {
                if (value != _RecipeEditorParamEdit)
                {
                    _RecipeEditorParamEdit = value;
                    RaisePropertyChanged();
                }
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
                    if(this.NeedleCleaner() != null)
                    {
                        NeedleCleanDevParam = (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam;
                    }

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

        public void DeInitModule()
        {

        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
            NeedleCleanDevParam = (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam;

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        //public EventCodeEnum RollBackParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    return retVal;
        //}


        //public bool HasParameterToSave()
        //{
        //    return true;
        //}


        public EventCodeEnum UpProc()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DownProc()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug("____________Save NC Devparameter____________");

                retVal = NeedleCleanModule.SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
    }
}

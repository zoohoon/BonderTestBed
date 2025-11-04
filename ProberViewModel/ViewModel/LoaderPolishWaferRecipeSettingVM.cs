using Autofac;
using LoaderBase.Communication;
using LogModule;
using PolishWaferParameters;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ControlClass.ViewModel.Polish_Wafer;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using VirtualKeyboardControl;
using ItemSelectionControl;
using ProberInterfaces.PolishWafer;
using SerializerUtil;
using MetroDialogInterfaces;
using System.Collections.Generic;

namespace LoaderPolishWaferRecipeSettingViewModelModule
{
    public class LoaderPolishWaferRecipeSettingVM : IPolishWaferRecipeSettingVM
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
        public LoaderBase.ILoaderSupervisor LoaderMaster => this.GetLoaderContainer().Resolve<LoaderBase.ILoaderSupervisor>();
        private SubstrateSizeEnum SelectedStageWaferSize = SubstrateSizeEnum.UNDEFINED;
        private readonly Guid _ViewModelGUID = new Guid("7e938aed-e420-4496-8f89-8e145d26c06b");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        private PolishWaferParameter _PolishWaferParam;
        public PolishWaferParameter PolishWaferParam
        {
            get { return _PolishWaferParam; }
            set
            {
                if (value != _PolishWaferParam)
                {
                    _PolishWaferParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PolishWaferIntervalParameter _SelectedIntervalParam;
        public PolishWaferIntervalParameter SelectedIntervalParam
        {
            get { return _SelectedIntervalParam; }
            set
            {
                if (value != _SelectedIntervalParam)
                {
                    _SelectedIntervalParam = value;

                    RaisePropertyChanged();
                }
            }
        }

        private IPolishWaferCleaningParameter _SelectedCleaningParam;
        public IPolishWaferCleaningParameter SelectedCleaningParam
        {
            get { return _SelectedCleaningParam; }
            set
            {
                if (value != _SelectedCleaningParam)
                {
                    _SelectedCleaningParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private object _SelectionSelectedItem;
        public object SelectionSelectedItem
        {
            get { return _SelectionSelectedItem; }
            set
            {
                if (value != _SelectionSelectedItem)
                {
                    _SelectionSelectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SelectionUIType _selectionuitype;
        public SelectionUIType selectionuitype
        {
            get { return _selectionuitype; }
            set
            {
                if (value != _selectionuitype)
                {
                    _selectionuitype = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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

        private UCItemSelection _SelectionUI;
        public UCItemSelection SelectionUI
        {
            get { return _SelectionUI; }
            set
            {
                if (value != _SelectionUI)
                {
                    _SelectionUI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SelectionTitle;
        public string SelectionTitle
        {
            get { return _SelectionTitle; }
            set
            {
                if (value != _SelectionTitle)
                {
                    _SelectionTitle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private object _SelectionSource;
        public object SelectionSource
        {
            get { return _SelectionSource; }
            set
            {
                if (value != _SelectionSource)
                {
                    _SelectionSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsOK = false;
        public bool IsOK
        {
            get { return _IsOK; }
            set
            {
                if (value != _IsOK)
                {
                    _IsOK = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncObservableCollection<IPolishWaferSourceInformation> _PolishWaferSourceInformations;
        public AsyncObservableCollection<IPolishWaferSourceInformation> PolishWaferSourceInformations
        {
            get { return _PolishWaferSourceInformations; }
            set
            {
                if (value != _PolishWaferSourceInformations)
                {
                    _PolishWaferSourceInformations = value;
                    RaisePropertyChanged();
                }
            }
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
             {
                //AsyncHelpers.RunSync(() => _RemoteMediumProxy.PolishWaferRecipeSettingVM_PageSwitched());
                await _RemoteMediumProxy.PolishWaferRecipeSettingVM_PageSwitched();

                PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    SelectionUI = new UCItemSelection();
                    SelectionUI.DataContext = this;
                    SelectionTitle = "Select the polish wafer type";
                });

                PolishWaferSourceInformations = this.DeviceManager().GetPolishWaferSources();
                
                var cellinfo = LoaderMaster.GetDeviceInfoClient(LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index));
                SelectedStageWaferSize = cellinfo.Size.Value;

                List<IPolishWaferSourceInformation> PolishWaferSources = new List<IPolishWaferSourceInformation>();
                if (PolishWaferSourceInformations.Count > 0)
                {
                    foreach (var source in PolishWaferSourceInformations)
                    {
                        if (source.Size.Value == SelectedStageWaferSize)
                        {
                            PolishWaferSources.Add(source);
                        }
                    }

                    if (PolishWaferSources.Count > 0) 
                    {
                        SelectionSource = PolishWaferSources;
                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                ParamSynchronization();

                AsyncHelpers.RunSync(() => _RemoteMediumProxy.PolishWaferRecipeSettingVM_Cleanup());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private AsyncCommand<Object> _IntervalAddCommand;
        public IAsyncCommand IntervalAddCommand
        {
            get
            {
                if (null == _IntervalAddCommand) _IntervalAddCommand = new AsyncCommand<Object>(IntervalAddCommandFunc);
                return _IntervalAddCommand;
            }
        }

        private async Task IntervalAddCommandFunc(object obj)
        {
            try
            {
                await _RemoteMediumProxy.PolishWaferRecipeSettingVM_IntervalAddCommand();

                PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;

                SelectedIntervalParam = PolishWaferParam.PolishWaferIntervalParameters.Last();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<PolishWaferIndexModel> _CleaningDeleteCommand;
        public ICommand CleaningDeleteCommand
        {
            get
            {
                if (null == _CleaningDeleteCommand) _CleaningDeleteCommand = new RelayCommand<PolishWaferIndexModel>(CleaningDeleteCommandFunc);
                return _CleaningDeleteCommand;
            }
        }

        private void CleaningDeleteCommandFunc(PolishWaferIndexModel param)
        {
            try
            {
                _RemoteMediumProxy.PolishWaferRecipeSettingVM_CleaningDelete(param);
                PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _IntervalDeleteCommand;
        public ICommand IntervalDeleteCommand
        {
            get
            {
                if (null == _IntervalDeleteCommand) _IntervalDeleteCommand = new RelayCommand<Object>(IntervalDeleteCommandFunc);
                return _IntervalDeleteCommand;
            }
        }

        private void IntervalDeleteCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj;
                _RemoteMediumProxy.PolishWaferRecipeSettingVM_IntervalDelete(index);

                PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<Object> _IntervalDeleteSyncCommand;
        public ICommand IntervalDeleteSyncCommand
        {
            get
            {
                if (null == _IntervalDeleteSyncCommand) _IntervalDeleteSyncCommand = new RelayCommand<Object>(IntervalDeleteSyncCommandFunc);
                return _IntervalDeleteSyncCommand;
            }
        }

        private void IntervalDeleteSyncCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj;
                _RemoteMediumProxy.PolishWaferRecipeSettingVM_IntervalDelete(index);

                PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private RelayCommand<object> _CleaningAddSyncCommand;
        public ICommand CleaningAddSyncCommand
        {
            get
            {
                if (null == _CleaningAddSyncCommand) _CleaningAddSyncCommand = new RelayCommand<object>(CleaningAddCommandSyncFunc);
                return _CleaningAddSyncCommand;
            }
        }
        private AsyncCommand<object> _CleaningAddCommand;
        public IAsyncCommand CleaningAddCommand
        {
            get
            {
                if (null == _CleaningAddCommand) _CleaningAddCommand = new AsyncCommand<object>(CleaningAddCommandFunc);
                return _CleaningAddCommand;
            }
        }
        private async Task CleaningAddCommandFunc(object obj)
        {
            try
            {
                //_RemoteMediumProxy.PolishWaferRecipeSettingVM_CleaningAddCommand(obj);
                int index = (int)obj;
                // 임시(?) Stage에서 MetroDialogManager 사용이 껄끄러워, 로더측에서 데이터 전달하는 형태로 구성.
                List<IPolishWaferSourceInformation> PolishWaferSources = new List<IPolishWaferSourceInformation>();
                if (PolishWaferSourceInformations != null)
                {
                    foreach (var source in PolishWaferSourceInformations)
                    {
                        if (source.Size.Value == SelectedStageWaferSize)
                        {
                            PolishWaferSources.Add(source);
                        }
                    }
                    if (PolishWaferSources.Count > 0)
                    {
                        SelectionSource = PolishWaferSources;
                        SelectedIntervalParam = PolishWaferParam.PolishWaferIntervalParameters[index];
                        SelectionSelectedItem = null;
                        selectionuitype = SelectionUIType.ADD;
                        await this.MetroDialogManager().ShowWindow(SelectionUI);
                    }
                    else
                    {
                        IsOK = false;
                        await this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    IsOK = false;
                    await this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CleaningAddCommandSyncFunc(object obj)
        {
            try
            {
                int index = (int)obj;
                List<IPolishWaferSourceInformation> PolishWaferSources = new List<IPolishWaferSourceInformation>();
                if (PolishWaferSourceInformations == null)
                {
                    PolishWaferSourceInformations = this.DeviceManager().GetPolishWaferSources();
                }
                if (PolishWaferSourceInformations.Count > 0)
                {
                    foreach (var source in PolishWaferSourceInformations) 
                    {
                        if (source.Size.Value == SelectedStageWaferSize) 
                        {
                            PolishWaferSources.Add(source);
                        }
                    }

                    if (PolishWaferSources.Count > 0)
                    {
                        SelectionSource = PolishWaferSources;
                        SelectedIntervalParam = PolishWaferParam.PolishWaferIntervalParameters[index];
                        SelectionSelectedItem = null;
                        selectionuitype = SelectionUIType.ADD;
                        this.MetroDialogManager().ShowWindow(SelectionUI);
                    }
                    else 
                    {
                        IsOK = false;
                        this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.\nThere is currently no polish wafer type that matches the device size of the cell.", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    IsOK = false;
                    this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task SetSelectedInfos()
        {
            try
            {
                byte[] cleaningparam = null;
                byte[] pwinfoparam = null;
                byte[] intervalparam = null;

                cleaningparam = SerializeManager.SerializeToByte(SelectedCleaningParam, typeof(PolishWaferCleaningParameter));
                pwinfoparam = SerializeManager.SerializeToByte(SelectionSelectedItem, typeof(PolishWaferInformation));
                intervalparam = SerializeManager.SerializeToByte(SelectedIntervalParam, typeof(PolishWaferIntervalParameter));

                int intervalindex = -1;
                int cleaingindex = -1;

                intervalindex = PolishWaferParam.PolishWaferIntervalParameters.IndexOf(SelectedIntervalParam);

                if (SelectedCleaningParam != null)
                {
                    cleaingindex = PolishWaferParam.PolishWaferIntervalParameters[intervalindex].CleaningParameters.IndexOf(SelectedCleaningParam);
                }

                await _RemoteMediumProxy.PolishWaferRecipeSettingVM_SetSelectedInfos(selectionuitype, cleaningparam, pwinfoparam, intervalparam, intervalindex, cleaingindex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task CleaningDeleteCommandWrapper(byte[] param)
        {
            throw new NotImplementedException();
        }

        private AsyncCommand _SelectionOKButtonCommand;
        public ICommand SelectionOKButtonCommand
        {
            get
            {
                if (_SelectionOKButtonCommand == null) _SelectionOKButtonCommand = new AsyncCommand(SelectionOKButtonCommandfunc, false);
                return _SelectionOKButtonCommand;
            }
        }

        private async Task SelectionOKButtonCommandfunc()
        {
            try
            {
                IsOK = true;
                //await this.MetroDialogManager().CloseWindow(SelectionUI);
                await this.MetroDialogManager().CloseWindow(SelectionUI);
                if (IsOK == true)
                {
                    // Send Stage & Update
                    await SetSelectedInfos();
                    PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;
                }
                else
                {
                    // Cancel
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ParamSynchronization()
        {
            try
            {
                #region Thickness값을 Cell로 전달
                foreach (var intervalparam in PolishWaferParam?.PolishWaferIntervalParameters)
                {
                    foreach (var cleaningparam in intervalparam?.CleaningParameters)
                    {
                        var pwInfo = this.DeviceManager().GetPolishWaferSources().Where(x => x.DefineName.Value == cleaningparam.WaferDefineType.Value).FirstOrDefault();
                        if (pwInfo != null)
                        {
                            if (cleaningparam.Thickness.Value != pwInfo.Thickness.Value)
                            {
                                cleaningparam.Thickness.Value = pwInfo.Thickness.Value;
                            }
                        }
                    }
                }
                #endregion

                byte[] param = null;

                param = SerializeManager.SerializeToByte(PolishWaferParam, typeof(PolishWaferParameter));

                if (param != null)
                {
                    SetPolishWaferIParam(param);
                }
                else
                {
                    LoggerManager.Error($"ParamSynchronization() Failed");
                }

                PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SelectionCancelButtonCommand;
        public ICommand SelectionCancelButtonCommand
        {
            get
            {
                if (_SelectionCancelButtonCommand == null) _SelectionCancelButtonCommand
                        = new AsyncCommand(SelectionCancelButtonCommandFunc, false);
                return _SelectionCancelButtonCommand;
            }
        }

        private async Task SelectionCancelButtonCommandFunc()
        {
            try
            {
                IsOK = false;
                await this.MetroDialogManager().CloseWindow(SelectionUI);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DecimalTextBoxClickCommand;
        public ICommand DecimalTextBoxClickCommand
        {
            get
            {
                if (null == _DecimalTextBoxClickCommand) _DecimalTextBoxClickCommand = new RelayCommand<Object>(FuncDecimalTextBoxClickCommand);
                return _DecimalTextBoxClickCommand;
            }
        }

        private void FuncDecimalTextBoxClickCommand(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _WaferDefineTypeTextBoxClickCommand;
        public ICommand WaferDefineTypeTextBoxClickCommand
        {
            get
            {
                if (null == _WaferDefineTypeTextBoxClickCommand) _WaferDefineTypeTextBoxClickCommand = new AsyncCommand<object>(WaferDefineTypeTextBoxClickCommandFunc);
                return _WaferDefineTypeTextBoxClickCommand;
            }
        }

        public async Task WaferDefineTypeTextBoxClickCommandFunc(object obj)
        {
            try
            {
                PolishWaferDefineModel param = obj as PolishWaferDefineModel;

                if (PolishWaferParam.PolishWaferIntervalParameters.Count - 1 >= param.IntervalIndex)
                {
                    if (PolishWaferParam.PolishWaferIntervalParameters[param.IntervalIndex].CleaningParameters.Count - 1 >= param.CleaningIndex)
                    {
                        SelectedIntervalParam = PolishWaferParam.PolishWaferIntervalParameters[param.IntervalIndex];
                        SelectedCleaningParam = PolishWaferParam.PolishWaferIntervalParameters[param.IntervalIndex].CleaningParameters[param.CleaningIndex];

                        var source = PolishWaferSourceInformations.Where(x => x.DefineName.Value == param.DefineName).FirstOrDefault();

                        if ((source != null))
                        {
                            SelectionSelectedItem = source as PolishWaferInformation;

                            if (SelectionSelectedItem != null)
                            {
                                if ((source as PolishWaferInformation).Size.Value == SelectedStageWaferSize)
                                {
                                    selectionuitype = SelectionUIType.MODIFY;
                                    await this.MetroDialogManager().ShowWindow(SelectionUI);
                                }
                                else
                                {
                                    IsOK = false;
                                    this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.\nThere is currently no polish wafer type that matches the device size of the cell.", EnumMessageStyle.Affirmative);
                                }
                            }
                            else
                            {
                                IsOK = false;
                                LoggerManager.Debug("Unknown");
                            }
                        }
                        else
                        {
                            IsOK = false;
                            LoggerManager.Debug("Invalid name");
                        }
                    }
                }

                //if (IsOK == true)
                //{
                //    // Send Stage & Update
                //    SetSelectedInfos();

                //    PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;
                //}
                //else
                //{
                //    // Cancel
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _FloatTextBoxClickCommand;
        public ICommand FloatTextBoxClickCommand
        {
            get
            {
                if (null == _FloatTextBoxClickCommand) _FloatTextBoxClickCommand = new RelayCommand<Object>(FloatTextBoxClickCommandFunc);
                return _FloatTextBoxClickCommand;
            }
        }

        private void FloatTextBoxClickCommandFunc(object param)
        {
            try
            {
                //System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                //tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL);
                //tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                string backuptext = "";

                (System.Windows.Controls.TextBox, IElement) paramVal = ((System.Windows.Controls.TextBox, IElement))param;
                System.Windows.Controls.TextBox textbox = paramVal.Item1;
                IElement element = paramVal.Item2;
                backuptext = textbox.Text;

                if (textbox != null && element != null)
                {
                    textbox.Text = VirtualKeyboard.Show(textbox.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 7);

                    if (Convert.ToInt32(textbox.Text) >= (int)element.LowerLimit && Convert.ToInt32(textbox.Text) <= (int)element.UpperLimit)
                    {

                    }
                    else
                    {
                        textbox.Text = backuptext;
                        var ret = this.MetroDialogManager().ShowMessageDialog($"Value Limit Error", $"Out of set value, Lower Limit : {element.LowerLimit} , Upper Limit : {element.UpperLimit}, \n Set to previous value", EnumMessageStyle.Affirmative);
                    }
                    textbox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                }

                ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _PWEnableClickCommand;
        public ICommand PWEnableClickCommand
        {
            get
            {
                if (null == _PWEnableClickCommand) _PWEnableClickCommand = new AsyncCommand(FuncPWEnableClickCommand);
                return _PWEnableClickCommand;
            }
        }
        private async Task FuncPWEnableClickCommand()
        {
            try
            {
                ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<Object> _ChangedCleaningIntervalModeCommand;
        public ICommand ChangedCleaningIntervalModeCommand
        {
            get
            {
                if (null == _ChangedCleaningIntervalModeCommand) _ChangedCleaningIntervalModeCommand = new RelayCommand<Object>(ChangedCleaningIntervalModeCommandFunc);
                return _ChangedCleaningIntervalModeCommand;
            }
        }

        private void ChangedCleaningIntervalModeCommandFunc(object obj)
        {
            try
            {
                ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetPolishWaferIParam(byte[] param)
        {
            try
            {
                _RemoteMediumProxy.PolishWaferRecipeSettingVM_SetPolishWaferIParam(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task SetSelectedInfos(SelectionUIType selectiontype, byte[] cleaningparam, byte[] pwinfo, byte[] intervalparam, int intervalindex, int cleaningindex)
        {
            throw new NotImplementedException();
        }

        public Task IntervalAddRemoteCommand()
        {
            throw new NotImplementedException();
        }

        //public Task IntervalDeleteRemoteCommand(object obj)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task CleaningAddRemoteCommand(object obj)
        //{
        //    throw new NotImplementedException();
        //}

        public void IntervalDelete(int index)
        {
            throw new NotImplementedException();
        }

        public void CleaningAdd(int index)
        {
            throw new NotImplementedException();
        }

        public void CleaningDelete(PolishWaferIndexModel obj)
        {
            throw new NotImplementedException();
        }
    }
}

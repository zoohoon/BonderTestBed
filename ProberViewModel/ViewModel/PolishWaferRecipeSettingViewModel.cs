using System;
using System.Linq;
using System.Threading.Tasks;

namespace PolishWaferRecipeSettingVM
{
    using CUIServices;
    using Focusing;
    using ItemSelectionControl;
    using LogModule;
    using MetroDialogInterfaces;
    using PolishWaferParameters;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.ControlClass.ViewModel.Polish_Wafer;
    using ProberInterfaces.PolishWafer;
    using RecipeEditorControl.RecipeEditorParamEdit;
    using RelayCommandBase;
    using SerializerUtil;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using VirtualKeyboardControl;

    public class PolishWaferRecipeSettingViewModel : IMainScreenViewModel, IFactoryModule, IPolishWaferRecipeSettingVM
    {
        #region //..IMainScreenViewModel Property
        public bool Initialized { get; set; } = false;
        private readonly Guid _ViewModelGUID = new Guid("666B9F0E-B3E1-8D45-89F0-84A496E4A1FF");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }
        #endregion

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        private const int _PolsiWaferDevCatID = 10041000;

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

        //private static object _selectedItem = null;
        //private static object _SelectedItem;
        //public static object SelectedItem
        //{
        //    get { return _SelectedItem; }
        //    set
        //    {
        //        if (value != _SelectedItem)
        //        {
        //            _SelectedItem = value;
        //            OnSelectedItemChanged();
        //        }
        //    }
        //}
        //public static void OnSelectedItemChanged()
        //{
        //    // Raise event / do other things
        //}

        //private bool _isSelected;
        //public bool IsSelected
        //{
        //    get { return _isSelected; }
        //    set
        //    {
        //        if (_isSelected != value)
        //        {
        //            _isSelected = value;
        //            RaisePropertyChanged();

        //            if (_isSelected)
        //            {
        //                SelectedItem = this;
        //            }
        //        }
        //    }
        //}

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

        public PolishWaferRecipeSettingViewModel()
        {
        }

        private void InitParam() 
        {
            try
            {
                var pwparam = this.PolishWaferModule().GetPolishWaferIParam();

                PolishWaferSourceInformations = this.DeviceManager()?.GetPolishWaferSources();

                if (pwparam != null)
                {
                    PolishWaferParam = pwparam as PolishWaferParameter;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    SelectionUI = new UCItemSelection();
                    SelectionUI.DataContext = this;

                    SelectionTitle = "Select the polish wafer type";
                    SelectionSource = PolishWaferSourceInformations;

                }));

                //this.MainTitle = "Source select setup";
                //this.SelectionSource = PolishWaferParam.SourceParameters;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");

                    retVal = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void DeInitModule()
        {
            return;
        }


        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task<EventCodeEnum> DeInitViewModel(object parameter = null)
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

            return retval;
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {

                Task task = new Task(() =>
                {
                    InitParam();
                });
                task.Start();
                await task;

                //RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                //RecipeEditorParamEdit.HardCategoryFiltering(new List<int>() { _PolsiWaferDevCatID });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                Task task = new Task(() =>
                {
                    retval = this.SaveParameter(PolishWaferParam);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Type FindAncestor<Type>(DependencyObject dependencyObject)
                   where Type : class
        {
            DependencyObject target = dependencyObject;
            try
            {
                do
                {
                    target = VisualTreeHelper.GetParent(target);
                }
                while (target != null && !(target is Type));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return target as Type;
        }

        #region //..Command & Command Method

        private RelayCommand<object> _CleaningAddSyncCommand;
        public ICommand CleaningAddSyncCommand
        {
            get
            {
                if (null == _CleaningAddSyncCommand) _CleaningAddSyncCommand = new RelayCommand<object>(CleaningAddCommandSyncFunc);
                return _CleaningAddSyncCommand;
            }
        }

        private void CleaningAddCommandSyncFunc(object obj)
        {
            try
            {
                //this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                int index = (int)obj;

                // Source가 존재하지 않는 경우, 추가 할 수 없도록 처리를 해놓자.
                List<IPolishWaferSourceInformation> PolishWaferSources = new List<IPolishWaferSourceInformation>();
                if (PolishWaferSourceInformations != null)
                {
                    if (PolishWaferSourceInformations.Count > 0)
                    {
                        foreach (var source in PolishWaferSourceInformations)
                        {
                            if (source.Size.Value == (SubstrateSizeEnum)this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum)
                            {
                                PolishWaferSources.Add(source);
                            }
                        }

                        if (PolishWaferSources.Count > 0)
                        {
                            if (SelectionSource == null)
                            {
                                SelectionSource = PolishWaferSources;
                            }
                            SelectedIntervalParam = PolishWaferParam.PolishWaferIntervalParameters[index];
                            SelectionSelectedItem = PolishWaferSources[0];
                            selectionuitype = SelectionUIType.ADD;
                            this.MetroDialogManager().ShowWindow(SelectionUI);
                        }
                        else
                        {
                            this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.\nThere is currently no polish wafer type that matches the device size of the cell.", EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.", EnumMessageStyle.Affirmative);
                }

                if (IsOK == true)
                {
                    cleaingadd();
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
            finally
            {
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
                IntervalDelete(index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<CUI.Button> _PolishWaferDevSetupCommand;
        public ICommand PolishWaferDevSetupCommand
        {
            get
            {
                if (null == _PolishWaferDevSetupCommand) _PolishWaferDevSetupCommand = new AsyncCommand<CUI.Button>(FuncPolishWaferDevSetupCommand);
                return _PolishWaferDevSetupCommand;
            }
        }

        private async Task FuncPolishWaferDevSetupCommand(CUI.Button cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                await this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _PWEnableClickCommand;
        public IAsyncCommand PWEnableClickCommand
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
                //await this.WaitCancelDialogService().ShowDialog("Wait");
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                this.PolishWaferModule().SaveDevParameter();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private AsyncCommand<object> _WaferDefineTypeTextBoxClickCommand;
        public IAsyncCommand WaferDefineTypeTextBoxClickCommand
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
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

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
                                selectionuitype = SelectionUIType.MODIFY;
                                await this.MetroDialogManager().ShowWindow(SelectionUI);
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


                if (IsOK == true)
                {
                    cleaingadd();
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
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private AsyncCommand _SelectionOKButtonCommand;
        public IAsyncCommand SelectionOKButtonCommand
        {
            get
            {
                if (_SelectionOKButtonCommand == null) _SelectionOKButtonCommand = new AsyncCommand(SelectionOKButtonCommandfunc);
                return _SelectionOKButtonCommand;
            }
        }

        private void cleaingadd()
        {
            // 현재 선택되어 있는 Cleaning Parameter의 Name을 현재 선택되어 있는 Information의 Name으로 변경하기
            if (selectionuitype == SelectionUIType.MODIFY)
            {
                SelectedCleaningParam.WaferDefineType.Value = (SelectionSelectedItem as PolishWaferInformation).DefineName.Value;
            }
            // 선택되어 있는 Interval List에 Cleaning Data Add하기, 현재 선택되어 있는 Information의 Name으로
            else if (selectionuitype == SelectionUIType.ADD)
            {
                string name = (SelectionSelectedItem as PolishWaferInformation).DefineName.Value;

                PolishWaferCleaningParameter tempparam = new PolishWaferCleaningParameter();

                tempparam.WaferDefineType.Value = name;
                tempparam.CleaningScrubMode.Value = PWScrubMode.UP_DOWN;
                tempparam.CleaningDirection.Value = CleaningDirection.Up;
                tempparam.ContactLength.Value = 1000;
                tempparam.ContactCount.Value = 10;
                tempparam.ScrubingLength.Value = 10;
                tempparam.OverdriveValue.Value = -1000;
                tempparam.Clearance.Value = -10000;

                tempparam.FocusingPointMode.Value = PWFocusingPointMode.POINT1;

                tempparam.FocusParam = new NormalFocusParameter();

                //tempparam.FocusParam.FocusRange.Value = 300;
                //tempparam.FocusParam.LightParams = new ObservableCollection<LightValueParam>();
                //tempparam.FocusParam.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;

                this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, tempparam.FocusParam, 300);

                ICamera CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                foreach (var lightCh in CurCam.LightsChannels)
                {
                    LightValueParam tmp = new LightValueParam();
                    tmp.Type.Value = lightCh.Type.Value;
                    tmp.Value.Value = 100;

                    tempparam.FocusParam.LightParams.Add(tmp);
                }

                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);

                tempparam.CenteringLightParams = new ObservableCollection<LightValueParam>();

                foreach (var lightCh in CurCam.LightsChannels)
                {
                    LightValueParam tmp = new LightValueParam();
                    tmp.Type.Value = lightCh.Type.Value;
                    tmp.Value.Value = 255;

                    tempparam.CenteringLightParams.Add(tmp);
                }

                tempparam.FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                tempparam.EdgeDetectionBeforeCleaning.Value = true;
                tempparam.PinAlignBeforeCleaning.Value = false;
                tempparam.PinAlignAfterCleaning.Value = true;

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    SelectedIntervalParam.CleaningParameters.Add(tempparam);
                }));

                //this.SaveParameter(PolishWaferParam);
            }
        }

        private async Task SelectionOKButtonCommandfunc()
        {
            try
            {
                IsOK = true;
                await this.MetroDialogManager().CloseWindow(SelectionUI);

                //// 현재 선택되어 있는 Cleaning Parameter의 Name을 현재 선택되어 있는 Information의 Name으로 변경하기
                //if (selectionuitype == SelectionUIType.MODIFY)
                //{
                //    SelectedCleaningParam.WaferDefineType.Value = (SelectionSelectedItem as PolishWaferInformation).DefineName.Value;
                //}
                //// 선택되어 있는 Interval List에 Cleaning Data Add하기, 현재 선택되어 있는 Information의 Name으로
                //else if (selectionuitype == SelectionUIType.ADD)
                //{
                //    string name = (SelectionSelectedItem as PolishWaferInformation).DefineName.Value;

                //    PolishWaferCleaningParameter tempparam = new PolishWaferCleaningParameter();
                //    tempparam.WaferDefineType.Value = name;
                //    tempparam.CleaningScrubMode.Value = PWScrubMode.UP_DOWN;
                //    tempparam.CleaningDirection.Value = CleaningDirection.Up;
                //    tempparam.ContactLength.Value = 1000;
                //    tempparam.ContactCount.Value = 10;
                //    tempparam.ScrubingLength.Value = 10;
                //    tempparam.OverdriveValue.Value = -1000;
                //    tempparam.Clearance.Value = -10000;
                //    tempparam.FocusParam = new NormalFocusParameter();
                //    tempparam.FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                //    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                //    {
                //        SelectedIntervalParam.CleaningParameters.Add(tempparam);
                //    }));

                //    this.SaveParameter(PolishWaferParam);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand _SelectionCancelButtonCommand;
        public IAsyncCommand SelectionCancelButtonCommand
        {
            get
            {
                if (_SelectionCancelButtonCommand == null) _SelectionCancelButtonCommand = new AsyncCommand(SelectionCancelButtonCommandFunc);
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<PolishWaferIndexModel> _CleaningDeleteCommand;
        public IAsyncCommand CleaningDeleteCommand
        {
            get
            {
                //if (null == _CleaningDeleteCommand) _CleaningDeleteCommand = new RelayCommand<PolishWaferIndexModel>(CleaningDeleteCommandFunc);
                if (null == _CleaningDeleteCommand) _CleaningDeleteCommand = new AsyncCommand<PolishWaferIndexModel>(CleaningDeleteCmdFunc);

                return _CleaningDeleteCommand;
            }
        }

        public async Task CleaningDeleteCommandWrapper(byte[] param)
        {
            try
            {
                PolishWaferIndexModel obj = await this.ByteArrayToObject(param) as PolishWaferIndexModel;
                await CleaningDeleteCommandFunc(obj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task CleaningDeleteCmdFunc(PolishWaferIndexModel obj)
        {
            try
            {
                await CleaningDeleteCommandFunc(obj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task CleaningDeleteCommandFunc(PolishWaferIndexModel obj)
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                PolishWaferIndexModel param = obj as PolishWaferIndexModel;

                Task task = new Task(() =>
                {
                    if (PolishWaferParam.PolishWaferIntervalParameters.Count > 0)
                    {
                        if (PolishWaferParam.PolishWaferIntervalParameters[param.IntervalIndex].CleaningParameters.Count > 0)
                        {
                            PolishWaferParam.PolishWaferIntervalParameters[param.IntervalIndex].CleaningParameters.RemoveAt(param.CleaningIndex);
                        }
                        else
                        {
                            LoggerManager.Error($"CleaningDeleteCommandFunc() failed. unknown index");
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"CleaningDeleteCommandFunc() failed. unknown index");
                    }

                    this.SaveParameter(PolishWaferParam);
                });
                task.Start();
                await task;

                //TreeViewItem item = (TreeViewItem)obj;
                //TreeView CleaningtreeView = ItemsControl.ItemsControlFromItemContainer(item) as TreeView;

                //int cleaningindex = CleaningtreeView.ItemContainerGenerator.IndexFromContainer(item);

                //TreeView IntervaltreeView = FindAncestor<TreeView>(CleaningtreeView);

                //if (IntervaltreeView != null)
                //{
                //    // Interval Index를 얻기 위해, 현재 얻어온 Cleaning Tree의 ItemSource를 이용

                //    ObservableCollection<PolishWaferCleaningParameter> cleaningsource = (ObservableCollection<PolishWaferCleaningParameter>)CleaningtreeView.ItemsSource;

                //    int intervalindex = -1;

                //    for (int i = 0; i < PolishWaferParam.PolishWaferIntervalParameters.Count; i++)
                //    {
                //        bool isTrue = PolishWaferParam.PolishWaferIntervalParameters[i].CleaningParameters.Equals(cleaningsource);

                //        if (isTrue == true)
                //        {
                //            intervalindex = i;
                //            break;
                //        }
                //    }

                //    if (intervalindex >= 0)
                //    {
                //        PolishWaferParam.PolishWaferIntervalParameters[intervalindex].CleaningParameters.RemoveAt(cleaningindex);
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        public void CleaningDelete(PolishWaferIndexModel param)
        {
            try
            {

                if (PolishWaferParam.PolishWaferIntervalParameters.Count > 0)
                {
                    if (PolishWaferParam.PolishWaferIntervalParameters[param.IntervalIndex].CleaningParameters.Count > 0)
                    {
                        PolishWaferParam.PolishWaferIntervalParameters[param.IntervalIndex].CleaningParameters.RemoveAt(param.CleaningIndex);
                    }
                    else
                    {
                        LoggerManager.Error($"CleaningDeleteCommandFunc() failed. unknown index");
                    }
                }
                else
                {
                    LoggerManager.Error($"CleaningDeleteCommandFunc() failed. unknown index");
                }

                this.SaveParameter(PolishWaferParam);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        //private RelayCommand<Object> _IntervalDeleteCommand;
        //public ICommand IntervalDeleteCommand
        //{
        //    get
        //    {
        //        if (null == _IntervalDeleteCommand) _IntervalDeleteCommand = new RelayCommand<Object>(IntervalDeleteCommandFunc);
        //        return _IntervalDeleteCommand;
        //    }
        //}

        private void IntervalDeleteCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj;

                if (index <= PolishWaferParam.PolishWaferIntervalParameters.Count - 1)
                {
                    PolishWaferParam.PolishWaferIntervalParameters.RemoveAt(index);

                    this.SaveParameter(PolishWaferParam);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
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
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    PolishWaferParam.PolishWaferIntervalParameters.Add(new PolishWaferIntervalParameter());
                }));

                this.SaveParameter(PolishWaferParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private AsyncCommand<Object> _ChangedCleaningIntervalModeCommand;
        public IAsyncCommand ChangedCleaningIntervalModeCommand
        {
            get
            {
                if (null == _ChangedCleaningIntervalModeCommand) _ChangedCleaningIntervalModeCommand = new AsyncCommand<Object>(ChangedCleaningIntervalModeCommandFunc);
                return _ChangedCleaningIntervalModeCommand;
            }
        }

        private async Task ChangedCleaningIntervalModeCommandFunc(object obj)
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                this.PolishWaferModule().PolishWaferParameter = this.PolishWaferParam;
                this.PolishWaferModule().SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        public void SetPolishWaferIParam(byte[] param)
        {
            try
            {
                object target = null;

                var result = SerializeManager.DeserializeFromByte(param, out target, typeof(PolishWaferParameter));

                if (target != null)
                {
                    PolishWaferParam = target as PolishWaferParameter;

                    this.PolishWaferModule().PolishWaferParameter = PolishWaferParam;
                    this.PolishWaferModule().SaveDevParameter();
                }
                else
                {
                    LoggerManager.Error($"SetPolishWaferIParam function is faild.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task SetSelectedInfos(SelectionUIType selectiontype, byte[] cleaningparam, byte[] pwinfo, byte[] intervalparam, int intervalindex, int cleaningindex)
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                this.selectionuitype = selectiontype;

                object cleaningparamtarget = null;
                object pwinfotarget = null;
                object intervalparamtarget = null;

                bool result = false;

                if (cleaningparam != null)
                {
                    result = SerializeManager.DeserializeFromByte(cleaningparam, out cleaningparamtarget, typeof(PolishWaferCleaningParameter));
                }

                if (cleaningparamtarget != null)
                {
                    this.SelectedCleaningParam = cleaningparamtarget as PolishWaferCleaningParameter;
                }

                result = SerializeManager.DeserializeFromByte(pwinfo, out pwinfotarget, typeof(PolishWaferInformation));

                if (pwinfotarget != null)
                {
                    this.SelectionSelectedItem = pwinfotarget as PolishWaferInformation;
                }
                else
                {
                    LoggerManager.Error($"SetSelectedInfos function is faild.");
                }

                result = SerializeManager.DeserializeFromByte(intervalparam, out intervalparamtarget, typeof(PolishWaferIntervalParameter));

                if (intervalparamtarget != null)
                {
                    this.SelectedIntervalParam = intervalparamtarget as PolishWaferIntervalParameter;

                    if (cleaningindex >= 0)
                    {
                        this.SelectedIntervalParam.CleaningParameters[cleaningindex] = this.SelectedCleaningParam;
                    }

                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        PolishWaferParam.PolishWaferIntervalParameters[intervalindex] = SelectedIntervalParam;
                    }));
                }
                else
                {
                    LoggerManager.Error($"SetSelectedInfos function is faild.");
                }

                cleaingadd();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        #endregion


        #region Remote commands

        public async Task IntervalAddRemoteCommand()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                PolishWaferIntervalParameter param = new PolishWaferIntervalParameter();

                var already_lotstart_trigger = PolishWaferParam.PolishWaferIntervalParameters.FirstOrDefault(x => x.CleaningTriggerMode.Value == EnumCleaningTriggerMode.LOT_START);

                if (already_lotstart_trigger != null)
                {
                    param.CleaningTriggerMode.Value = EnumCleaningTriggerMode.WAFER_INTERVAL;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    PolishWaferParam.PolishWaferIntervalParameters.Add(param);
                }));

                this.SaveParameter(PolishWaferParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        //public async Task IntervalDeleteRemoteCommand(object obj)
        //{
        //    try
        //    {
        //        await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

        //        int index = (int)obj;

        //        if (index <= PolishWaferParam.PolishWaferIntervalParameters.Count - 1)
        //        {
        //            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
        //            {
        //                PolishWaferParam.PolishWaferIntervalParameters.RemoveAt(index);
        //            }));

        //            this.SaveParameter(PolishWaferParam);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
        //    }
        //}

        public void IntervalDelete(int index)
        {
            try
            {
                //this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                if (index <= PolishWaferParam.PolishWaferIntervalParameters.Count - 1)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        PolishWaferParam.PolishWaferIntervalParameters.RemoveAt(index);
                    }));

                    this.SaveParameter(PolishWaferParam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        public void CleaningAdd(int index)
        {
            try
            {
                // Source가 존재하지 않는 경우, 추가 할 수 없도록 처리를 해놓자.
                List<IPolishWaferSourceInformation> PolishWaferSources = new List<IPolishWaferSourceInformation>();
                if (PolishWaferSourceInformations.Count > 0)
                {
                    foreach (var source in PolishWaferSourceInformations)
                    {
                        if (source.Size.Value == (SubstrateSizeEnum)this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum)
                        {
                            PolishWaferSources.Add(source);
                        }
                    }

                    if (PolishWaferSources.Count > 0)
                    {
                        if (SelectionSource == null)
                        {
                            SelectionSource = PolishWaferSources;
                        }
                        SelectedIntervalParam = PolishWaferParam.PolishWaferIntervalParameters[index];
                        SelectionSelectedItem = PolishWaferSources[0];
                        selectionuitype = SelectionUIType.ADD;
                        var ret = this.MetroDialogManager().ShowWindow(SelectionUI);
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.\nThere is currently no polish wafer type that matches the device size of the cell.", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    var ret = this.MetroDialogManager().ShowMessageDialog("[Information]", "Please check polish wafer type data first.", EnumMessageStyle.Affirmative);
                }

                if (IsOK == true)
                {
                    cleaingadd();
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
            finally
            {
            }
        }
        #endregion
    }
}

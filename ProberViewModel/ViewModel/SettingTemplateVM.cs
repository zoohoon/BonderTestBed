using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UcDataForSettingPage;

namespace ProberViewModel
{
    public class SettingTemplateVM : IMainScreenViewModel, ISettingTemplateViewModel
    {
        private Guid _ViewModelGUID = new Guid("AA74CE25-F777-9DCC-C6FB-A0CFC1259DB5");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private readonly string uncheckedCircle = "M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
        private readonly string checkedCircle = "M20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4C12.76,4 13.5,4.11 14.2,4.31L15.77,2.74C14.61,2.26 13.34,2 12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12M7.91,10.08L6.5,11.5L11,16L21,6L19.59,4.58L11,13.17L7.91,10.08Z";

        public bool Initialized { get; set; } = false;
        private string _SettingName;
        public string SettingName
        {
            get { return _SettingName; }
            set
            {
                if (value != _SettingName)
                {
                    _SettingName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSearchDataClearButtonVisible;
        public bool IsSearchDataClearButtonVisible
        {
            get { return _IsSearchDataClearButtonVisible; }
            set
            {
                if (value != _IsSearchDataClearButtonVisible)
                {
                    _IsSearchDataClearButtonVisible = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Visibility _NavPageButtonVisible;
        public Visibility NavPageButtonVisible
        {
            get { return _NavPageButtonVisible; }
            set
            {
                if (value != _NavPageButtonVisible)
                {
                    _NavPageButtonVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SettingViewInfos OrigineCollection;
        private SettingViewInfos _SettingInfoCollection = new SettingViewInfos();
        public SettingViewInfos SettingInfoCollection
        {
            get { return _SettingInfoCollection; }
            set
            {
                if (value != _SettingInfoCollection)
                {
                    _SettingInfoCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenViewModel _PreSelectedSettingViewModel;
        private SettingInfo _PreSelectedSettingViewInfo;

        private SettingInfo _selectedSettingViewInfo;
        public SettingInfo SelectedSettingViewInfo
        {
            get { return _selectedSettingViewInfo; }
            set
            {
                if (value != _selectedSettingViewInfo)
                {
                    if (_selectedSettingViewInfo != null)
                    {
                        _selectedSettingViewInfo.Icon = uncheckedCircle;
                        SaveElementUsingCategoryID();
                    }

                    _PreSelectedSettingViewInfo = _selectedSettingViewInfo;
                    _selectedSettingViewInfo = value;

                    if (_selectedSettingViewInfo != null)
                    {
                        _selectedSettingViewInfo.Icon = checkedCircle;
                        _selectedSettingViewInfo.IsSavedCategory = false;
                    }

                    RaisePropertyChanged();
                    UpDownBtnVisibility = true;
                }
            }
        }
        private IMainScreenView _SelectedView;
        public IMainScreenView SelectedView
        {
            get { return _SelectedView; }
            set
            {
                if (value != _SelectedView)
                {
                    _SelectedView = value;
                    RaisePropertyChanged(nameof(SelectedView));
                }
            }
        }

        private string _SearchText;
        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                if (value != _SearchText)
                {
                    _SearchText = value;
                    RaisePropertyChanged();
                    FindCategoryUsingText(value);
                }
            }
        }

        private string _Icon;
        public string Icon
        {
            get { return _Icon; }
            set
            {
                if (value != _Icon)
                {
                    _Icon = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _UpDownBtnVisibilitye;
        public bool UpDownBtnVisibility
        {
            get { return _UpDownBtnVisibilitye; }
            set
            {
                if (value != _UpDownBtnVisibilitye)
                {
                    _UpDownBtnVisibilitye = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _TemplateUserLevel;
        public int TemplateUserLevel
        {
            get { return _TemplateUserLevel; }
            set
            {
                if (value != _TemplateUserLevel)
                {
                    _TemplateUserLevel = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> Function for find category
        private SettingInfo TempSelectedInfo;
        private void FindCategoryUsingText(string searchData)
        {
            try
            {
                if (string.IsNullOrEmpty(searchData) == true)
                {
                    this.SettingInfoCollection = OrigineCollection;

                    if (TempSelectedInfo != null && SelectedSettingViewInfo == null)
                    {
                        SelectedSettingViewInfo = TempSelectedInfo;
                        TempSelectedInfo = null;
                    }

                    IsSearchDataClearButtonVisible = false;
                }
                else
                {
                    FindSearchCategory(searchData);
                    IsSearchDataClearButtonVisible = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async void FindSearchCategory(string searchData)
        {
            try
            {
                SettingViewInfos searchCategories = new SettingViewInfos();

                if (SelectedSettingViewInfo != null)
                {
                    TempSelectedInfo = SelectedSettingViewInfo;
                }

                await Task.Run(() =>
                {
                    foreach (var SettingInfo in OrigineCollection)
                    {

                        if (SettingInfo.Name.ToLower().Contains(searchData.ToLower()))
                        {
                            searchCategories.Add(SettingInfo);
                        }
                    }
                });

                if (searchCategories == null)
                {
                    this.SettingInfoCollection = OrigineCollection;
                }
                else
                {
                    this.SettingInfoCollection = searchCategories;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> Up Button Command
        private RelayCommand<object> _UpButtonProcCommand;
        public ICommand UpButtonProcCommand
        {
            get
            {
                if (null == _UpButtonProcCommand)
                    _UpButtonProcCommand = new RelayCommand<object>(UpButtonProc);
                return _UpButtonProcCommand;
            }
        }

        private void UpButtonProc(object obj)
        {
            try
            {
                if (SelectedView is FrameworkElement)
                {
                    FrameworkElement frameworkItem = SelectedView as FrameworkElement;
                    if (frameworkItem.DataContext is IParamScrollingViewModel)
                    {
                        IParamScrollingViewModel ParamScrollingViewModel = frameworkItem.DataContext as IParamScrollingViewModel;
                        ParamScrollingViewModel?.UpProc();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        #region ==> Down Button Command
        private RelayCommand<object> _DwButtonProcCommand;
        public ICommand DwButtonProcCommand
        {
            get
            {
                if (null == _DwButtonProcCommand)
                    _DwButtonProcCommand = new RelayCommand<object>(DwButtonProc);
                return _DwButtonProcCommand;
            }
        }

        private void DwButtonProc(object obj)
        {
            try
            {
                if (SelectedView is FrameworkElement)
                {
                    FrameworkElement frameworkItem = SelectedView as FrameworkElement;

                    if (frameworkItem.DataContext is IParamScrollingViewModel)
                    {
                        IParamScrollingViewModel ParamScrollingViewModel = frameworkItem.DataContext as IParamScrollingViewModel;
                        ParamScrollingViewModel?.DownProc();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> Clear SearchData Command
        private RelayCommand<object> _ClearSearchDataCommand;
        public ICommand ClearSearchDataCommand
        {
            get
            {
                if (null == _ClearSearchDataCommand)
                {
                    _ClearSearchDataCommand = new RelayCommand<object>(ClearSearchData);
                }

                return _ClearSearchDataCommand;
            }
        }

        private void ClearSearchData(object obj)
        {
            this.SearchText = string.Empty;
        }
        #endregion

        #region ==> Category Changed Command
        private AsyncCommand _CategoryChangedCommand;
        public ICommand CategoryChangedCommand
        {
            get
            {
                if (null == _CategoryChangedCommand)
                {
                    _CategoryChangedCommand = new AsyncCommand(CategoryChangedFunc);
                }

                return _CategoryChangedCommand;
            }
        }

        // SelectedSettingViewInfo 값 변경 시, 호출 되도록 UI와 연결되어 있음.
        private async Task CategoryChangedFunc()
        {
            try
            {
                await ChangeToSettingInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool SettingNameIsDifferent()
        {
            bool retval = false;

            try
            {
                if (SelectedSettingViewInfo?.Name != _PreSelectedSettingViewInfo?.Name)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private async Task ChangeToSettingInfo()
        {
            try
            {
                if (SelectedSettingViewInfo != null && (SelectedView == null || SettingNameIsDifferent()))
                {
                    bool retVal = false;

                    retVal = await AuthenticationViewEnter(SelectedSettingViewInfo);

                    if (retVal == true)
                    {
                        await ChangeSelectedView(SelectedSettingViewInfo);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //SecurityVM 확인
        private async Task<bool> AuthenticationViewEnter(SettingInfo selectedSettingViewInfo)
        {
            bool retval = false;

            try
            {
                if (selectedSettingViewInfo != null)
                {
                    IMainScreenViewModel tmpSelectedView = this.ViewModelManager().FindViewModelObject(selectedSettingViewInfo.ViewGUID);

                    // TODO: Dialog 정리 및 확인 필요
                    if (tmpSelectedView is ISecurityViewModel)
                    {
                        ISecurityViewModel securityVM = tmpSelectedView as ISecurityViewModel;

                        string password = string.Empty;
                        bool isMatchedPassword = false;

                        var dialogResult = await this.MetroDialogManager().ShowPasswordInputDialog("Password", "Submit");

                        if (dialogResult == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            password = this.MetroDialogManager().GetPasswordInputData();

                            isMatchedPassword = securityVM.CheckSecurityPassword(password);

                            if (isMatchedPassword)
                            {
                                retval = true;
                            }
                            else
                            {
                                SelectedSettingViewInfo = _PreSelectedSettingViewInfo;

                                await this.MetroDialogManager().ShowMessageDialog("Fail", "Fail", EnumMessageStyle.Affirmative);
                            }
                        }
                        else
                        {
                            SelectedSettingViewInfo = _PreSelectedSettingViewInfo;
                        }
                    }
                    else
                    {
                        retval = true;
                    }
                }
                else
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }



        private void SendCategoryIdTodVM()
        {
            try
            {
                if (SelectedView != null)
                {
                    VmRecipeEditorMainPage recipeEditorViewModel = null;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (SelectedView is FrameworkElement)
                        {
                            FrameworkElement frameworkItem = SelectedView as FrameworkElement;
                            if (frameworkItem.DataContext is VmRecipeEditorMainPage)
                            {
                                recipeEditorViewModel = frameworkItem.DataContext as VmRecipeEditorMainPage;
                                recipeEditorViewModel.NavPageButtonVisible = Visibility.Hidden;

                            }
                        }
                    });

                    if (recipeEditorViewModel != null && SelectedSettingViewInfo != null)
                    {
                        recipeEditorViewModel?.RecipeEditorParamEdit.InitParameter();
                        recipeEditorViewModel?.RecipeEditorParamEdit.CategoryFiltering(SelectedSettingViewInfo.CategoryID);
                        recipeEditorViewModel.InitParameter(SelectedSettingViewInfo, this.SettingName);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpDownButtonVisibleFunc()
        {
            try
            {
                if (SelectedView != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (SelectedView is FrameworkElement)
                        {
                            FrameworkElement frameworkItem = SelectedView as FrameworkElement;
                            if (frameworkItem.DataContext is IUpDownBtnNoneVisible)
                            {
                                this.UpDownBtnVisibility = false;
                            }
                        }
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> ControlUnloadedCommand
        private RelayCommand<object> _ControlUnloadedCommand;
        public ICommand ControlUnloadedCommand
        {
            get
            {
                if (null == _ControlUnloadedCommand)
                    _ControlUnloadedCommand = new RelayCommand<object>(ControlUnloaded);
                return _ControlUnloadedCommand;
            }
        }
        private void ControlUnloaded(object obj)
        {
            try
            {
                SaveElementUsingCategoryID();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SaveElementUsingCategoryID()
        {
            try
            {
                if (_selectedSettingViewInfo != null)
                {
                    if (_selectedSettingViewInfo.IsSavedCategory == false)
                    {
                        if (_selectedSettingViewInfo.CategoryID != null)
                        {
                            this.ParamManager().SaveElement(_selectedSettingViewInfo?.CategoryID);
                        }

                        if (_selectedSettingViewInfo != null)
                        {
                            _selectedSettingViewInfo.IsSavedCategory = true;
                        }
                        else
                        {
                            LoggerManager.Error($"[SettingTemplateVM], SaveElementUsingCategoryID() = _SelectedSettingViewInfo is null value.");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
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
                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Task task = new Task(() =>
                {
                    if (parameter is Category selectedCategory)
                    {
                        InitView(selectedCategory);
                    }

                    NavPageButtonVisible = Visibility.Hidden;

                    retval = EventCodeEnum.NONE;
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

        private void InitView(Category selectedCategory)
        {
            try
            {
                SearchText = string.Empty;
                UpDownBtnVisibility = true;
                OrigineCollection = selectedCategory.SettingViewInfos;

                foreach (var settingInfo in OrigineCollection)
                {
                    settingInfo.Icon = this.uncheckedCircle;
                }

                SelectedView = null;

                this.SettingInfoCollection = OrigineCollection;
                this.SettingName = selectedCategory.Name;
                this.Icon = selectedCategory.Icon;

                if ((this.SettingInfoCollection != null) && (0 < this.SettingInfoCollection.Count) && (0 < this.SettingInfoCollection.Count(info => info.IsEnabled == true)))
                {
                    SetFirstItemToSelectedSettingViewInfo();

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetFirstItemToSelectedSettingViewInfo()
        {
            try
            {
                foreach (var v in SettingInfoCollection)
                {
                    if (v.IsEnabled == true)
                    {
                        ChangeSelectedSettingViewInfo(v);
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeSelectedSettingViewInfo(SettingInfo info)
        {
            try
            {
                SelectedSettingViewInfo = info;
                RaisePropertyChanged(nameof(SelectedSettingViewInfo));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task ChangeSelectedView(SettingInfo selectedSettingViewInfo)
        {
            try
            {
                IMainScreenView tmpSelectedView = null;

                if (selectedSettingViewInfo != null)
                {
                    tmpSelectedView = await this.ViewModelManager().GetViewObj(selectedSettingViewInfo.ViewGUID);
                }

                if (_PreSelectedSettingViewModel != null)
                {
                    await _PreSelectedSettingViewModel.Cleanup();
                }

                SelectedView = tmpSelectedView;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedView is FrameworkElement)
                    {
                        FrameworkElement frameworkItem = SelectedView as FrameworkElement;

                        if (frameworkItem.DataContext is IMainScreenViewModel)
                        {
                            _PreSelectedSettingViewModel = frameworkItem.DataContext as IMainScreenViewModel;
                        }
                    }
                });

                Task task = new Task(() =>
                {
                    SendCategoryIdTodVM();
                    UpDownButtonVisibleFunc();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (_PreSelectedSettingViewModel != null)
                {
                    await _PreSelectedSettingViewModel.Cleanup();
                    _PreSelectedSettingViewModel = null;
                }
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

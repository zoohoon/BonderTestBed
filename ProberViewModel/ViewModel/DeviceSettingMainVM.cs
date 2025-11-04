using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UcDataForSettingPage;

namespace DeviceSettingMainViewModel
{
    public class DeviceSettingMainVM : IMainScreenViewModel
    {
        private Guid _ViewModelGUID = new Guid("AF818282-0A06-791F-910F-5B8BCFC21466");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public bool Initialized { get; set; } = false;

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

        private Categories _ShowCategories
        = new Categories();
        public Categories ShowCategories
        {
            get { return _ShowCategories; }
            set
            {
                if (value != _ShowCategories)
                {
                    _ShowCategories = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Categories OrigineCategories = new Categories();

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

                    if (string.IsNullOrEmpty(value) == true)
                    {
                        this.ShowCategories = OrigineCategories;
                        IsSearchDataClearButtonVisible = false;
                    }
                    else
                    {
                        FindSearchCategory(value);
                        IsSearchDataClearButtonVisible = true;
                    }
                }
            }
        }

        private bool _IsSearchDataClearButtonVisible = false;
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

        private RelayCommand<object> _ControlLoadedCommand;
        public ICommand ControlLoadedCommand
        {
            get
            {
                if (null == _ControlLoadedCommand)
                    _ControlLoadedCommand = new RelayCommand<object>(ControlLoaded);
                return _ControlLoadedCommand;
            }
        }

        private void ControlLoaded(object obj)
        {
        }

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
          
        }

        //private RelayCommand<object> _ItemSelectCommand;
        //public ICommand ItemSelectCommand
        //{
        //    get
        //    {
        //        if (null == _ItemSelectCommand)
        //            _ItemSelectCommand = new RelayCommand<object>(ItemSelect);
        //        return _ItemSelectCommand;
        //    }
        //}

        //private void ItemSelect(object obj)
        //{
        //    Category selectedItem = obj as Category;

        //    if (selectedItem != null)
        //    {
        //        try
        //        {
        //            this.ViewModelManager().ViewTransition(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), selectedItem);
        //        }
        //        catch (Exception err)
        //        {
        //        }

        //        //화면 전환.
        //    }
        //}

        private AsyncCommand<object> _ItemSelectCommand;
        public ICommand ItemSelectCommand
        {
            get
            {
                if (null == _ItemSelectCommand)
                    _ItemSelectCommand = new AsyncCommand<object>(ItemSelect);
                return _ItemSelectCommand;
            }
        }

        private async Task ItemSelect(object obj)
        {
            try
            {
                Category selectedItem = obj as Category;

                if (selectedItem != null)
                {
                    try
                    {
                        //this.ViewModelManager().ViewTransition(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), selectedItem);
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("B0737A90-696C-9155-D4C9-A628D1EED129"), selectedItem);
                    }
                    catch (Exception err)
                    {
                    }

                    //화면 전환.
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private async void FindSearchCategory(string searchData)
        {
            try
            {
                Categories searchCategories = new Categories();
                bool hasSearchData = false;

                await Task.Run(() =>
                {
                    foreach (var category in OrigineCategories)
                    {
                        hasSearchData = false;

                        if (category.Name.ToLower().Contains(searchData.ToLower()))
                        {
                            hasSearchData = true;
                        }

                        foreach (var categoryInfo in category.SettingViewInfos)
                        {
                            if (categoryInfo.Name.ToLower().Contains(searchData.ToLower()))
                            {
                                hasSearchData = true;
                                break;
                            }
                        }

                        if (hasSearchData == true)
                        {
                            searchCategories.Add(category);
                        }
                    }

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (searchCategories == null)
                        {
                            this.ShowCategories = OrigineCategories;
                        }
                        else
                        {
                            this.ShowCategories = searchCategories;
                        }
                    });
                });
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ClearSearchDataCommand;
        public ICommand ClearSearchDataCommand
        {
            get
            {
                if (null == _ClearSearchDataCommand)
                    _ClearSearchDataCommand = new RelayCommand<object>(ClearSearchData);
                return _ClearSearchDataCommand;
            }
        }
        
        private void ClearSearchData(object obj)
        {
            try
            {
                this.SearchText = "";
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IViewModelManager ViewModelManager = this.ViewModelManager();
                    OrigineCategories = new Categories();
                    SearchText = null;

                    foreach (var CategoryInfo in ViewModelManager.DeviceSettingCategoryInfos)
                    {
                        Category category = new Category();
                        List<string> descriptionList = new List<string>();
                        int i = 0;

                        category.Name = CategoryInfo.Name;
                        category.Icon = CategoryInfo.Icon;
                        category.Visibility = CategoryInfo.Visibility;
                        category.IsEnabled = CategoryInfo.IsEnabled;
                        category.MaskingLevel = CategoryInfo.MaskingLevel;

                        foreach (var SettingInfo in CategoryInfo.SettingInfos)
                        {
                            category.SettingViewInfos.Add(SettingInfo);

                            if (i < 3)
                            {
                                descriptionList.Add(SettingInfo.Name);
                                i++;
                            }
                        }

                        category.Description = string.Join(", ", descriptionList);
                        OrigineCategories.Add(category);
                    }

                    ShowCategories = OrigineCategories;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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

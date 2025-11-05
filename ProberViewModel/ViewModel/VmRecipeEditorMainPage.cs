using System;
using System.Threading.Tasks;

namespace ProberViewModel
{
    using System.ComponentModel;
    using ProberInterfaces;
    using ProberErrorCode;
    using RecipeEditorControl.RecipeEditorParamEdit;
    using System.Runtime.CompilerServices;
    using LogModule;
    using System.Windows;

    public class VmRecipeEditorMainPage : IMainScreenViewModel, IParamScrollingViewModel, IVmRecipeEditorMainPage
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

        #region ==> SettingInfoView
        private SettingInfo _SelectedSettingViewInfo;
        public SettingInfo SelectedSettingViewInfo
        {
            get { return _SelectedSettingViewInfo; }
            set
            {
                if (value != _SelectedSettingViewInfo)
                {
                    _SelectedSettingViewInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> SettingName
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
        #endregion
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
        readonly Guid _ViewModelGUID = new Guid("F05A242E-3834-33B7-9B4A-B3D913F2F255");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public VmRecipeEditorMainPage()
        {
        }
        public void SwitchParamEditPage(String searchKeyWord)
        {
            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.KeywordFiltering(searchKeyWord);//==> Fileter + Update
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }        

        #region ==> InitModule
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    SelectedSettingViewInfo = new SettingInfo();
                    IViewModelManager ViewModelManager = this.ViewModelManager();

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
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
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
                SwitchParamEditPage(String.Empty);
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
        #endregion

        public void InitParameter(SettingInfo settinginfo,string settingName)
        {
            SelectedSettingViewInfo = settinginfo;
            SettingName = settingName;
        }

        //public EventCodeEnum RollBackParameter()
        //{
        //    return EventCodeEnum.NONE;
        //}

        //public bool HasParameterToSave()
        //{
        //    return false;
        //}

        public EventCodeEnum UpProc()
        {
            RecipeEditorParamEdit.PrevPageCommandFunc();
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DownProc()
        {
            RecipeEditorParamEdit.NextPageCommandFunc();
            return EventCodeEnum.NONE;
        }

        //public EventCodeEnum CheckParameterToSave()
        //{
        //    return EventCodeEnum.NONE;
        //}

        //public EventCodeEnum SaveParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {

        //    retVal = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //         throw;
        //    }
        //    return retVal;
        //}
    }
}

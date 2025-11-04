using System;
using System.Threading.Tasks;

namespace CognexOCRSubSettingViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using CUIServices;
    using LogModule;

    public class CognexOCRSubSettingViewModel : IMainScreenViewModel, IParamScrollingViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        //#region ==> RecipeEditorParamEdit
        //private RecipeEditorParamEditViewModel _RecipeEditorParamEdit;
        //public RecipeEditorParamEditViewModel RecipeEditorParamEdit
        //{
        //    get { return _RecipeEditorParamEdit; }
        //    set
        //    {
        //        if (value != _RecipeEditorParamEdit)
        //        {
        //            _RecipeEditorParamEdit = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //#endregion

        #region ==> OCRSetupCommand
        private RelayCommand<CUI.Button> _OCRSetupCommand;
        public ICommand OCRSetupCommand
        {
            get
            {
                if (null == _OCRSetupCommand) _OCRSetupCommand = new RelayCommand<CUI.Button>(FuncOCRSetupCommand);
                return _OCRSetupCommand;
            }
        }

        private void FuncOCRSetupCommand(CUI.Button cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        //private const int _CognexDevCatID = 10013001;
        //private const int _CognexSysCatID = 10014;
        public bool Initialized { get; set; } = false;

        private readonly Guid _ViewModelGUID = new Guid("8127b8e5-2bef-41bd-a426-fce7a9527b7d");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
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
        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                //RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                //RecipeEditorParamEdit.HardCategoryFiltering(new List<int>() { _CognexDevCatID, _CognexSysCatID });
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
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public EventCodeEnum UpProc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //RecipeEditorParamEdit.PrevPageCommandFunc();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DownProc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //RecipeEditorParamEdit.NextPageCommandFunc();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}

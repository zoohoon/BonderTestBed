using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemicsOCRSubSettingViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using CUIServices;
    using LogModule;
    using RecipeEditorControl.RecipeEditorParamEdit;

    public class SemicsOCRSubSettingViewModel : IMainScreenViewModel, IParamScrollingViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

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

        private const int _SemicsDevCatID = 10013101;
        public bool Initialized { get; set; } = false;

        private readonly Guid _ViewModelGUID = new Guid("3f55d23f-b99a-49a8-8dec-08e256f316be");
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
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(new List<int>() { _SemicsDevCatID});

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
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
        public EventCodeEnum UpProc()
        {
            try
            {
                RecipeEditorParamEdit.PrevPageCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DownProc()
        {
            try
            {
                RecipeEditorParamEdit.NextPageCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
    }
}

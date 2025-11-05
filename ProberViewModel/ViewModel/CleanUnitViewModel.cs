using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanUnitVM
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RecipeEditorControl.RecipeEditorParamEdit;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class CleanUnitViewModel : IMainScreenViewModel, IParamScrollingViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

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

        private readonly Guid _ViewModelGUID = new Guid("c93e4f41-0b94-4e37-9e50-a71fb3d565b1");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(00020002);
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


        public EventCodeEnum RollBackParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
                //retVal = GPIB.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool HasParameterToSave()
        {
            return true;
        }

        public EventCodeEnum UpProc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                RecipeEditorParamEdit.PrevPageCommandFunc();

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
                RecipeEditorParamEdit.NextPageCommandFunc();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private AsyncCommand<CUI.Button> _CleanSheetSetupCommand;
        public ICommand CleanSheetSetupCommand
        {
            get
            {
                if (null == _CleanSheetSetupCommand) _CleanSheetSetupCommand = new AsyncCommand<CUI.Button>(CleanSheetSetupCommandFunc);
                return _CleanSheetSetupCommand;
            }
        }

        private async Task CleanSheetSetupCommandFunc(CUI.Button cuiparam)
        {
            try
            {
                Guid viewguid = new Guid();
                List<Guid> pnpsteps = new List<Guid>();

                this.PnPManager().GetCuiBtnParam(this.NeedleCleaner(), cuiparam.GUID, out viewguid, out pnpsteps);

                if (pnpsteps.Count != 0)
                {
                    this.PnPManager().SetNavListToGUIDs(this.NeedleCleaner(), pnpsteps);
                    await this.ViewModelManager().ViewTransitionAsync(viewguid);
                }

                //Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                //this.ViewModelManager().ViewTransitionUsingVM(ViewGUID, this.NeedleCleaner());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}

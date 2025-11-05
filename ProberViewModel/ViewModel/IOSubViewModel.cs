using System;
using System.Threading.Tasks;

namespace IOSubViewModel
{
    using CUIServices;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class IOSubViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("1f7d4581-2baf-4703-9c38-7989972ba2f4");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;

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
        public EventCodeEnum RollBackParameter()
        {
            return EventCodeEnum.NONE;
        }

        public bool HasParameterToSave()
        {
            return true;
        }

        #region //..Command 
        private RelayCommand<CUI.Button> _IOViewCommand;
        public ICommand IOViewCommand
        {
            get
            {
                if (null == _IOViewCommand) _IOViewCommand = new RelayCommand<CUI.Button>(FuncIOViewCommand);
                return _IOViewCommand;
            }
        }

        private void FuncIOViewCommand(CUI.Button cuiparam)
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

        public ILightAdmin LightAdmin { get; set; }
        private AsyncCommand _LoadLUTCommand;
        public IAsyncCommand LoadLUTCommand
        {
            get
            {
                if (null == _LoadLUTCommand) _LoadLUTCommand = new AsyncCommand(LoadLUTCommandFunc);
                return _LoadLUTCommand;
            }
        }

        public async Task LoadLUTCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                LightAdmin = this.LightAdmin();
                LightAdmin.LoadLUT();
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
    }
}

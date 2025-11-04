using System;
using System.Threading.Tasks;

namespace AlarmsSubViewModel
{
    using CUIServices;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;

    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class AlarmsSubViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("bf7c986a-df8c-47b4-ad75-8960e32557b8");
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
        #region //..Command 
        private RelayCommand<CUI.Button> _AlarmsViewCommand;
        public ICommand AlarmsViewCommand
        {
            get
            {
                if (null == _AlarmsViewCommand) _AlarmsViewCommand = new RelayCommand<CUI.Button>(FuncAlarmsViewCommand);
                return _AlarmsViewCommand;
            }
        }

        private void FuncAlarmsViewCommand(CUI.Button cuiparam)
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

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        #endregion
    }
}

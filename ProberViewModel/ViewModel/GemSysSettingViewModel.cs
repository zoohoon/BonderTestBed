using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GemSysSettingVM
{
    public class GemSysSettingViewModel : IMainScreenViewModel
    {
        public Guid ScreenGUID { get; set; } = new Guid("0708E5B2-484A-497B-9AEE-844F294C1FB3");

        public bool Initialized { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void DeInitModule()
        {
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private RelayCommand _InitializeCommand;
        public ICommand InitializeCommand
        {
            get
            {
                if (null == _InitializeCommand) _InitializeCommand = new RelayCommand(Initialize);
                return _InitializeCommand;
            }
        }
        private void Initialize()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                IGEMModule gemModule = this.GEMModule();
                gemModule?.DeInitModule();
                gemModule?.InitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _OpenSettingDialogCommand;
        public ICommand OpenSettingDialogCommand
        {
            get
            {
                if (null == _OpenSettingDialogCommand) _OpenSettingDialogCommand = new RelayCommand(OpenSettingDialog);
                return _OpenSettingDialogCommand;
            }
        }
        private void OpenSettingDialog()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                IGEMModule gemModule = this.GEMModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
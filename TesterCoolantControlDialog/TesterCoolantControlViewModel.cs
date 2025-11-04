using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TesterCoolantControlDialog
{
    public class TesterCoolantControlViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region // ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
        private IGPLoader _GPLoader;
        public IGPLoader GPLoader
        {

            get { return _GPLoader; }
            private set
            {
                if (value != _GPLoader)
                {
                    _GPLoader = value;
                    RaisePropertyChanged();
                }
            }
        }
        public TesterCoolantControlViewModel()
        {
            InitViewModel();
        }
        public void UpdateValveStates()
        {
            bool state = false;
            if(GPLoader != null)
            {
                (this.GPLoader as IGPLoaderCommands).GetTesterCoolantValveState(0, out state);
            }
        }
        private EventCodeEnum InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.GetGPLoader() != null)
                {
                    GPLoader = this.GetGPLoader();
                    UpdateValveStates();
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private RelayCommand<object> _CoolantONCommand;
        public ICommand CoolantONCommand
        {
            get
            {
                if (null == _CoolantONCommand) _CoolantONCommand
                        = new RelayCommand<object>(CoolantONCommandFunc);
                return _CoolantONCommand;
            }
        }
        private RelayCommand<object> _CoolantOFFCommand;
        public ICommand CoolantOFFCommand
        {
            get
            {
                if (null == _CoolantOFFCommand) _CoolantOFFCommand
                        = new RelayCommand<object>(CoolantOFFCommandFunc);
                return _CoolantOFFCommand;
            }
        }
        private void CoolantONCommandFunc(object obj)
        {
            int index = -1;
            try
            {
                if (obj != null && int.TryParse((string)obj, out index))
                {
                    EventCodeEnum ret = ((IGPLoaderCommands)this.GetGPLoader()).SetTesterCoolantValve(index, true);
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"CoolantONCommandFunc(): Error occurred. Ret = {ret}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CoolantOFFCommandFunc(object obj)
        {
            int index = -1;
            try
            {
                if (obj != null && int.TryParse((string)obj, out index))
                {
                    EventCodeEnum ret = ((IGPLoaderCommands)this.GetGPLoader()).SetTesterCoolantValve(index, false);
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"CoolantOFFCommandFunc(): Error occurred. Ret = {ret}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

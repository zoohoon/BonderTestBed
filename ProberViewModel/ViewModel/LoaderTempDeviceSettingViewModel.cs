using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Temperature;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LoaderTempDeviceSettingVM
{
    public class LoaderTempDeviceSettingViewModel : IMainScreenViewModel
    {
        public Guid ScreenGUID { get; set; } = new Guid("42777a61-b70f-4134-90d2-d38227d00fef");

        public bool Initialized { get; set; }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private double _ValueForDeviceTemp;
        public double ValueForDeviceTemp
        {
            get { return _ValueForDeviceTemp; }
            set
            {
                if (value != _ValueForDeviceTemp)
                {
                    _ValueForDeviceTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ValueForSettingTemp;
        public double ValueForSettingTemp
        {
            get { return _ValueForSettingTemp; }
            set
            {
                if (value != _ValueForSettingTemp)
                {
                    _ValueForSettingTemp = value;
                    RaisePropertyChanged();
                }
            }
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

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                // TODO : 스테이지 정보를 얻어오는지 확인.
                ITempController TempController = this.TempController();
                //ValueForSV = TempController.GetSV();
                ValueForDeviceTemp = TempController.GetDevSetTemp();
                ValueForSettingTemp = TempController.TempInfo?.SetTemp?.Value ?? 9999;
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
            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private RelayCommand<object> _SettingSVCommand;
        public ICommand SettingSVCommand
        {
            get
            {
                if (null == _SettingSVCommand) _SettingSVCommand = new RelayCommand<object>(SettingSVFunc);
                return _SettingSVCommand;
            }
        }

        private void SettingSVFunc(object obj)
        {
            try
            {
                ITempController TempController = this.TempController();
                //TempController.SettingSV(ValueForSV, true);
                TempController.SetSV(TemperatureChangeSource.TEMP_DEVICE, ValueForSettingTemp, forcedSetValue:true, willYouSaveSetValue: true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }
}

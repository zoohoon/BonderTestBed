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

namespace LoaderTempDeviationVM
{
    public class LoaderTempDeviationViewModel : IMainScreenViewModel, IUpDownBtnNoneVisible
    {
        public Guid ScreenGUID { get; } = new Guid("784e230a-53e0-4234-973d-5c7323d6e7d8");

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; }

        public ITempController TempController { get; internal set; }

        private double _ValueForDeviation;
        public double ValueForDeviation
        {
            get { return _ValueForDeviation; }
            set
            {
                if (value != _ValueForDeviation)
                {
                    _ValueForDeviation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _EmergencyAbortTempDeviation;
        public double EmergencyAbortTempDeviation
        {
            get { return _EmergencyAbortTempDeviation; }
            set
            {
                if (value != _EmergencyAbortTempDeviation)
                {
                    _EmergencyAbortTempDeviation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TempPauseTypeEnum _TemperaturePauseType;
        public TempPauseTypeEnum TemperaturePauseType
        {
            get { return _TemperaturePauseType; }
            set
            {
                if (value != _TemperaturePauseType)
                {
                    _TemperaturePauseType = value;
                    SetTempPauseTypeFunc();
                    RaisePropertyChanged();
                }
            }
        }
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

        public EventCodeEnum InitModule()
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

        public Task<EventCodeEnum> InitViewModel()
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

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                TempController = this.TempController();

                ValueForDeviation = TempController.GetDeviaitionValue();
                EmergencyAbortTempDeviation = TempController.GetEmergencyAbortTempTolereance();
                TemperaturePauseType = TempController.GetTempPauseType();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        private AsyncCommand<object> _SettingDeviationCommand;
        public ICommand SettingDeviationCommand
        {
            get
            {
                if (null == _SettingDeviationCommand) _SettingDeviationCommand = new AsyncCommand<object>(SettingDeviationCommandFunc);
                return _SettingDeviationCommand;
            }
        }

        private async Task SettingDeviationCommandFunc(object obj)
        {
            try
            {
                ITempController TempController = this.TempController();

                string value = obj as string;
                bool retval = false;
                if (value == "ValueForDeviation")
                {
                    retval = TempController.CheckSetDeviationParamLimit(ValueForDeviation);
                    if(retval == true)
                    {
                        TempController.SetDeviaitionValue(ValueForDeviation);
                    }
                    else
                    {
                        ValueForDeviation = TempController.GetDeviaitionValue();
                    }
                }
                else if (value == "EmergencyAbortTempDeviation")
                {
                    retval = TempController.CheckSetDeviationParamLimit(EmergencyAbortTempDeviation, true);
                    if (retval == true)
                    {
                        TempController.SetDeviaitionValue(EmergencyAbortTempDeviation, true);
                    }
                    else
                    {
                        EmergencyAbortTempDeviation = TempController.GetEmergencyAbortTempTolereance();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void SetTempPauseTypeFunc()
        {
            try
            {
                ITempController TempController = this.TempController();
                TempController.SetTempPauseType(TemperaturePauseType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }
}

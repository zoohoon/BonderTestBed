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
using Temperature;
using VirtualKeyboardControl;

namespace TempDeviationVM
{
    public class TempDeviationViewModel : IMainScreenViewModel, INotifyPropertyChanged, IUpDownBtnNoneVisible
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public Guid ScreenGUID { get; } = new Guid("7EE3929B-D54A-4E39-BF50-CFB2193515E4");
        public bool Initialized { get; set; }
        public ITempController TempController { get; internal set; }

        #region .. Property

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
        
        private double _TempGEMToleranceDeviation;
        public double TempGEMToleranceDeviation
        {
            get { return _TempGEMToleranceDeviation; }
            set
            {
                if (value != _TempGEMToleranceDeviation)
                {
                    _TempGEMToleranceDeviation = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _TempMonitoringEnable;
        public bool TempMonitoringEnable
        {
            get { return _TempMonitoringEnable; }
            set
            {
                if (value != _TempMonitoringEnable)
                {
                    _TempMonitoringEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TempMonitorRange;
        public double TempMonitorRange
        {
            get { return _TempMonitorRange; }
            set
            {
                if (value != _TempMonitorRange)
                {
                    _TempMonitorRange = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _WaitMonitorTimeSec;
        public double WaitMonitorTimeSec
        {
            get { return _WaitMonitorTimeSec; }
            set
            {
                if (value != _WaitMonitorTimeSec)
                {
                    _WaitMonitorTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _AssistStopOnZDown;
        public bool AssistStopOnZDown
        {
            get { return _AssistStopOnZDown; }
            set
            {
                if (value != _AssistStopOnZDown)
                {
                    _AssistStopOnZDown = value;
                    RaisePropertyChanged();
                }
            }
        }



        #endregion

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
                TempController = this.TempController();

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
                TempController.LoadDevParameter();
                TempControllerDevParam tempDevParam = TempController.TempControllerDevParam as TempControllerDevParam;
                ValueForDeviation = tempDevParam.TempToleranceDeviation.Value;
                TempGEMToleranceDeviation = tempDevParam.TempGEMToleranceDeviation.Value;
                TempMonitoringEnable = tempDevParam.TemperatureMonitorEnable.Value;
                TempMonitorRange = tempDevParam.TempMonitorRange.Value;
                WaitMonitorTimeSec = tempDevParam.WaitMonitorTimeSec.Value;
                AssistStopOnZDown = tempDevParam.AssistStopOnZDown.Value;
                EmergencyAbortTempDeviation = tempDevParam.EmergencyAbortTempTolerance.Value;
                TemperaturePauseType = tempDevParam.TempPauseType.Value;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(FuncTextBoxClickCommand);
                return _TextBoxClickCommand;
            }
        }

        private void FuncTextBoxClickCommand(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 200);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
                    if (retval == true)
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

        private RelayCommand _SetAndSaveTempDevParamCommand;
        public ICommand SetAndSaveTempDevParamCommand
        {
            get
            {
                if (null == _SetAndSaveTempDevParamCommand) _SetAndSaveTempDevParamCommand = new RelayCommand(SetAndSaveTempDevParamCommandFunc);
                return _SetAndSaveTempDevParamCommand;
            }
        }

        private void SetAndSaveTempDevParamCommandFunc()
        {
            try
            {
                ITempController TempController = this.TempController();
                TempControllerDevParam tempDevParam = TempController.TempControllerDevParam as TempControllerDevParam;
                if (tempDevParam.TempGEMToleranceDeviation == null)
                    tempDevParam.TempGEMToleranceDeviation = new Element<double>();
                tempDevParam.TempGEMToleranceDeviation.Value = TempGEMToleranceDeviation;
                if (tempDevParam.TemperatureMonitorEnable == null)
                    tempDevParam.TemperatureMonitorEnable = new Element<bool>();
                tempDevParam.TemperatureMonitorEnable.Value = TempMonitoringEnable;
                if (tempDevParam.TempMonitorRange == null)
                    tempDevParam.TempMonitorRange = new Element<double>();
                tempDevParam.TempMonitorRange.Value = TempMonitorRange;
                if (tempDevParam.WaitMonitorTimeSec == null)
                    tempDevParam.WaitMonitorTimeSec = new Element<double>();
                tempDevParam.WaitMonitorTimeSec.Value = WaitMonitorTimeSec;
                if (tempDevParam.AssistStopOnZDown == null)
                    tempDevParam.AssistStopOnZDown = new Element<bool>();
                tempDevParam.AssistStopOnZDown.Value = AssistStopOnZDown;

                TempController.SaveDevParameter();
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

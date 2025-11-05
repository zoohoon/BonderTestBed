using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Autofac;
using LoaderBase.Communication;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using SerializerUtil;
using SoakingParameters;
using VirtualKeyboardControl;

namespace ProberViewModel
{
    public abstract class UcStatusSoakingViewModelBase : IFactoryModule, INotifyPropertyChanged, IModule
    {
        public ILoaderCommunicationManager _LoaderCommunicationManager = null;

        public SoakingStateEnum soakingState { get; set; } = SoakingStateEnum.PREPARE;

        private StatusSoakingConfig _StatusSoakingParam = null;
        public StatusSoakingConfig StatusSoakingParam
        {
            get 
            { 
                if(_StatusSoakingParam == null)
                {
                    UpdateStatusSoakingConfig();
                }
                return _StatusSoakingParam;
            } 
        }
        
        public UcStatusSoakingViewModelBase()
        {
        }

        public void UpdateStatusSoakingConfig(StatusSoakingConfig statusConfig = null)
        {
            if (null == statusConfig)
            {
                _StatusSoakingParam = GetStatusSoakingConfig();
            }
            else
            {
                _StatusSoakingParam = statusConfig;
            }
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
        
        #region IMainScreenViewModel
        public bool Initialized { get; set; } = false;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

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

            return retval;
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
            LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {GetType().Name}");
        }
        #endregion

        #region ICommand
        private RelayCommand<Object> textBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == textBoxClickCommand) textBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return textBoxClickCommand;
            }
        }

        public virtual void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                if (tb.IsReadOnly)
                {
                    return;
                }

                int oldValue = int.Parse(tb.Text);
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 5);

                switch (tb.Name)
                {
                    // Time Limit
                    case "WaitingPinAlignPeriodTime_tb":
                    case "EventSoakingTime_tb":
                        if (!CheckMinValue(tb.Text, 1))
                        {
                            tb.Text = oldValue.ToString();
                        }
                        break;
                    case "ChillingTableChillingTime_tb":
                        if (!CheckMinValue(tb.Text, StatusSoakingParam.AdvancedSetting.ChillingTimeMin.Value))
                        {
                            tb.Text = oldValue.ToString();
                        }
                        break;
                    case "ChillingTableSoakingTimeSec_tb":
                        if (!CheckMinValue(tb.Text, StatusSoakingParam.AdvancedSetting.ChillingSoakingTimeMin.Value))
                        {
                            tb.Text = oldValue.ToString();
                        }
                        break;
                    case "SoakingStepSoakingTime_tb":
                        if (!CheckMinValue(tb.Text, StatusSoakingParam.AdvancedSetting.SoakingStepTimeMin.Value))
                        {
                            tb.Text = oldValue.ToString();
                        }
                        break;
                    // OD Limit
                    case "SoakingStepSoakingOD_tb":
                        if (!CheckMaxValue(tb.Text, StatusSoakingParam.AdvancedSetting.ODLimit.Value))
                        {
                            tb.Text = oldValue.ToString();
                        }

                        var baseOD = (int)StatusSoakingParam.MaintainStateConfig.NotExistWaferObj_OD.Value;
                        var chuckAwayZ = (int)StatusSoakingParam.AdvancedSetting.ChuckAwayTolForChillingTime_Z.Value;
                        if (!CheckMinValue(tb.Text, baseOD - chuckAwayZ))
                        {
                            tb.Text = oldValue.ToString();
                        }
                        break;
                    case "WaitingOD_tb":
                    case "EventSoakingOD_tb":
                        if (!CheckMaxValue(tb.Text, StatusSoakingParam.AdvancedSetting.ODLimit.Value))
                        {
                            tb.Text = oldValue.ToString();
                        }
                        break;
                }

                if (String.IsNullOrEmpty(tb.Text))
                {
                    tb.Text = "0";
                }

                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }
        #endregion

        #region Method
        private StatusSoakingConfig GetStatusSoakingConfig()
        {
            try
            {
                if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    if (_LoaderCommunicationManager == null)
                    {
                        _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                    }

                    if ((_LoaderCommunicationManager == null) || (_LoaderCommunicationManager.SelectedStage == null))
                    {
                        return null;
                    }
                }

                var statusSoakingParamByte = this.SoakingModule().GetStatusSoakingConfigParam();
                var statusSoakingParamObj = SerializeManager.ByteToObject(statusSoakingParamByte);
                if (null == statusSoakingParamObj)
                {
                    return null;
                }

                return statusSoakingParamObj as StatusSoakingConfig;
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return null;
        }

        public virtual void SetStatusSoakingState(SoakingStateEnum state)
        {
            soakingState = state;
        }

        public void ReadStatusSoakingParameter()
        {
            try
            {
                UpdateStatusSoakingConfig();
                if (StatusSoakingParam != null)
                {
                    ReadStatusSoakinConfigParameter(StatusSoakingParam);
                }
            }
            catch (Exception ex)
            {
                LoggerManager.SoakingErrLog($"{ex.Message}");
            }
        }

        public EventCodeEnum SaveStatusSoakingParameter()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                UpdateStatusSoakingConfig();
                var statusSoakingParam = StatusSoakingParam;
                if (statusSoakingParam != null)
                {
                    ret = SaveStatusSoakingConfigParameter(ref statusSoakingParam);

                    var statusSoakingSaveParamByte = SerializeManager.ObjectToByte(statusSoakingParam);
                    this.SoakingModule().SetStatusSoakingConfigParam(statusSoakingSaveParamByte);
                }
            }
            catch (Exception ex)
            {
                LoggerManager.SoakingErrLog($"{ex.Message}");
            }

            return ret;
        }

        public abstract EventCodeEnum ReadStatusSoakinConfigParameter(StatusSoakingConfig statusSoakingParam);
        public abstract EventCodeEnum SaveStatusSoakingConfigParameter(ref StatusSoakingConfig statusSoakingParam);

        private bool CheckMinValue(string src, int minValue)
        {
            bool bRet = true;
            try
            {
                int.TryParse(src, out int value);
                if (value < minValue)
                {
                    bRet = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err.Message}");
            }

            return bRet;
        }

        private bool CheckMaxValue(string src, int maxValue)
        {
            bool bRet = true;
            try
            {
                int.TryParse(src, out int value);
                if (value > maxValue)
                {
                    bRet = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err.Message}");
            }

            return bRet;
        }
        #endregion
    }
}
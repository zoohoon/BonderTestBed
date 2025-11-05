using Focusing;
using LogModule;
using ProbeCardObject;
using ProberInterfaces;
using ProberInterfaces.Param;
using RelayCommandBase;
using SerializerUtil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VirtualKeyboardControl;

namespace BaseFocusingAdvanceSetup.ViewModel
{
    public class BaseFocusingAdvanceSetupViewModel : IFactoryModule, INotifyPropertyChanged, IPnpAdvanceSetupViewModel
    {
        #region //=>RaisePropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region //..Property
        public bool Initialized { get; set; } = false;

        private PinAlignDevParameters pinParam;

        private FocusParameter _FocusParam = new NormalFocusParameter();
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set
            {
                if (value != _FocusParam)
                {
                    _FocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BaseFocusingEnable _BaseFocusEnable;
        public BaseFocusingEnable BaseFocusEnable
        {
            get { return _BaseFocusEnable; }
            set
            {
                if (value != _BaseFocusEnable)
                {
                    _BaseFocusEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<BaseFocusingEnable> _BaseFocusingEnableEnums
             = new ObservableCollection<BaseFocusingEnable>();
        public ObservableCollection<BaseFocusingEnable> BaseFocusingEnableEnums
        {
            get { return _BaseFocusingEnableEnums; }
            set
            {
                if (value != _BaseFocusingEnableEnums)
                {
                    _BaseFocusingEnableEnums = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<BaseFocusingFirstPin> _BaseFocusingFirstPinEnums
             = new ObservableCollection<BaseFocusingFirstPin>();
        public ObservableCollection<BaseFocusingFirstPin> BaseFocusingFirstPinEnums
        {
            get { return _BaseFocusingFirstPinEnums; }
            set
            {
                if (value != _BaseFocusingFirstPinEnums)
                {
                    _BaseFocusingFirstPinEnums = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BaseFocusingFirstPin _DoFirstPin;
        public BaseFocusingFirstPin DoFirstPin
        {
            get { return _DoFirstPin; }
            set
            {
                if (value != _DoFirstPin)
                {
                    _DoFirstPin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusingTolerance;
        public double FocusingTolerance
        {
            get { return _FocusingTolerance; }
            set
            {
                if (value != _FocusingTolerance)
                {
                    _FocusingTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region //.. IPnpAdvanceSetupViewModel Method

        public void SetParameters(List<byte[]> datas)
        {
            try
            {
                if (datas != null)
                {
                    foreach (var param in datas)
                    {
                        object target;
                        SerializeManager.DeserializeFromByte(param, out target, typeof(PinAlignDevParameters));
                        if (target != null)
                        {
                            SettingDataPinAlign(target as PinAlignDevParameters);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<byte[]> GetParameters()
        {
            List<byte[]> parameters = new List<byte[]>();
            try
            {
                if (pinParam != null)
                {
                    parameters.Add(SerializeManager.SerializeToByte(pinParam));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        #endregion

        private AsyncCommand _CmdOKClick;
        public ICommand CmdOKClick
        {
            get
            {
                if (null == _CmdOKClick) _CmdOKClick = new AsyncCommand(CmdOKClickFunc, false);
                return _CmdOKClick;
            }
        }
        private async Task CmdOKClickFunc()
        {
            try
            {
                pinParam.PinHighAlignBaseFocusingParam.BaseFocsuingEnable.Value = BaseFocusEnable;
                pinParam.PinHighAlignBaseFocusingParam.DoFirstPin.Value = DoFirstPin;
                pinParam.PinHighAlignBaseFocusingParam.FocusingTolerance.Value = FocusingTolerance;
                pinParam.PinHighAlignBaseFocusingParam.FocusingParam = FocusParam;

                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void SettingDataPinAlign(PinAlignDevParameters param)
        {
            try
            {
                pinParam = param;
                BaseFocusEnable = pinParam.PinHighAlignBaseFocusingParam.BaseFocsuingEnable.Value;
                DoFirstPin = pinParam.PinHighAlignBaseFocusingParam.DoFirstPin.Value;
                FocusingTolerance = pinParam.PinHighAlignBaseFocusingParam.FocusingTolerance.Value;
                FocusParam = pinParam.PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter; 

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand _CmdCancelClick;
        public ICommand CmdCancelClick
        {
            get
            {
                if (null == _CmdCancelClick) _CmdCancelClick = new AsyncCommand(CmdCancelClickFunc, false);
                return _CmdCancelClick;
            }
        }
        private async Task CmdCancelClickFunc()
        {
            try
            {
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }


        private void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void Init()
        {
            try
            {
                if (Initialized == false)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var en in Enum.GetValues(typeof(BaseFocusingEnable)))
                        {
                            BaseFocusingEnableEnums.Add((BaseFocusingEnable)en);
                        }

                        foreach (var en in Enum.GetValues(typeof(BaseFocusingFirstPin)))
                        {
                            BaseFocusingFirstPinEnums.Add((BaseFocusingFirstPin)en);
                        }
                    });
                    Initialized = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
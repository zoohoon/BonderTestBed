using System.Windows;
using System.Windows.Input;
using LogModule;
using ProberInterfaces;
using RelayCommandBase;
using SoakingParameters;
using VirtualKeyboardControl;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProberViewModel
{
    /// <summary>
    /// Metro 창 닫기 커맨드를 구현한 VM 클래스
    /// </summary>
    public class MetroCustomDialogViewModel : ViewModelBase
    {
        #region 창 닫기
        private MessageBoxResult _result = MessageBoxResult.None;
        public MessageBoxResult Result { get => _result; }

        internal event EventHandler BeforeClose;

        private AsyncCommand _okCommand;
        public ICommand IDOK
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new AsyncCommand(OKCommand);
                }
                return _okCommand;
            }
        }


        public async Task OKCommand()
        {
            try
            {
                _result = MessageBoxResult.OK;
                BeforeClose?.Invoke(this, null);
                this.MetroDialogManager().CloseWindow(window, windowName);
            }
            catch (Exception e)
            {

            }
        }

        private AsyncCommand _cancelCommand;
        public ICommand IDCancel
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new AsyncCommand(CancelCommand);
                }
                return _cancelCommand;
            }
        }


        public async Task CancelCommand()
        {
            try
            {
                _result = MessageBoxResult.Cancel;
                BeforeClose?.Invoke(this, null);
                this.MetroDialogManager().CloseWindow(window, windowName);
            }
            catch (Exception e)
            {

            }
        }
        #endregion

        private ContentControl window;
        private string windowName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"> 다이얼로그 닫기를 위한 창 핸들 </param>
        /// <param name="name">다이얼로그 닫기를 위한 창 이름</param>
        public MetroCustomDialogViewModel(ContentControl window, string name)
        {
            this.window = window;
            this.windowName = name;
        }
    }

    public class TouchMetroCustomDialogViewModel : MetroCustomDialogViewModel
    {
        private RelayCommand<object> _onScreenKeyboard;
        public ICommand OnScreenKeyBoard
        {
            get
            {
                if (_onScreenKeyboard == null)
                {
                    _onScreenKeyboard = new RelayCommand<object>(
                        (param) =>
                        {
                            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                            tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL, 0, 100);
                            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                        });
                }

                return _onScreenKeyboard;
            }
        }

        private RelayCommand<object> _onScreenIntegerKeyboard;
        public ICommand OnScreenIntegerKeyboard
        {
            get
            {
                if (_onScreenIntegerKeyboard == null)
                {
                    _onScreenIntegerKeyboard = new RelayCommand<object>(
                        (param) =>
                        {
                            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                            tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 5);
                            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                        });
                }

                return _onScreenIntegerKeyboard;
            }
        }

        private RelayCommand<object> _onScreenFloatKeyboard;
        public ICommand OnScreenFloatKeyboard
        {
            get
            {
                if (_onScreenFloatKeyboard == null)
                {
                    _onScreenFloatKeyboard = new RelayCommand<object>(
                        (param) =>
                        {
                            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                            tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                        });
                }

                return _onScreenFloatKeyboard;
            }
        }

        public TouchMetroCustomDialogViewModel(ContentControl window, string name) : base(window, name)
        {
        }
    }



    public class UcSoakingAdvancedSettingViewModel : TouchMetroCustomDialogViewModel
    {

        private bool? _isValid = null;
        public bool? IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        private string _validateResult;
        public string ValidateResult
        {
            get => _validateResult;
            set => SetProperty(ref _validateResult, value);
        }

        private bool _IsUsePolishWafer;
        public bool IsUsePolishWafer
        {
            get => _IsUsePolishWafer;
            set => SetProperty(ref _IsUsePolishWafer, value);
        }

        /// <summary>
        /// 템플릿 적용 요청
        /// </summary>
        public event ObjectEventHandler<Advancedsetting> ApplyRequest;

        public ICommand CmdApply { get; set; }

        private Advancedsetting _setting;
        public Advancedsetting Setting
        {
            get => _setting;
            set
            {
                AddPropertyChanged(value);
                SetProperty(ref _setting, value);
                SetEnableParameter();
            }
        }

        #region 척 이동범위 제한 옵션의 최대치 설정
        private int _chuckAwayToleranceLimitX;
        public int ChuckAwayToleranceLimitX { get => _chuckAwayToleranceLimitX; set { SetProperty(ref _chuckAwayToleranceLimitX, value); } }
        private int _chuckAwayToleranceLimitY;
        public int ChuckAwayToleranceLimitY { get => _chuckAwayToleranceLimitY; set { SetProperty(ref _chuckAwayToleranceLimitY, value); } }
        private int _chuckAwayToleranceLimitZ;
        public int ChuckAwayToleranceLimitZ { get => _chuckAwayToleranceLimitZ; set { SetProperty(ref _chuckAwayToleranceLimitZ, value); } }
        #endregion

        public void AddPropertyChanged(Advancedsetting obj)
        {
            foreach (System.Reflection.PropertyInfo prop in obj.GetType().GetProperties())
            {
                object value = prop.GetValue(obj);
                if (value is Element<double> d)
                {
                    d.ValueChangedEvent += Element_ValueChangedEvent;
                }
                else if (value is Element<int> i)
                {
                    i.ValueChangedEvent += Element_ValueChangedEvent;
                }
            }
        }

        public void RemovePropertyChanged(Advancedsetting obj)
        {
            foreach (System.Reflection.PropertyInfo prop in obj.GetType().GetProperties())
            {
                object value = prop.GetValue(obj);
                if (value is Element<double> d)
                {
                    d.ValueChangedEvent -= Element_ValueChangedEvent;
                }
                else if (value is Element<int> i)
                {
                    i.ValueChangedEvent -= Element_ValueChangedEvent;
                }
            }
        }

        private void Element_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            ValidateCheck(Setting);
        }

        private void SetEnableParameter()
        {
            try
            {
                IsUsePolishWafer = this.SoakingModule().IsUsePolishWafer();
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"> 다이얼로그 닫기를 위한 창 핸들 </param>
        /// <param name="name">다이얼로그 닫기를 위한 창 이름</param>
        public UcSoakingAdvancedSettingViewModel(ContentControl window, string name) : base(window, name)
        {
            SetCommand();
        }

        public void SetCommand()
        {
            //선택된 템플릿 적용을 위해 이벤트 발생
            CmdApply = new AsyncCommand(
                async () =>
                {
                    RemovePropertyChanged(Setting); // 직렬화할때 에러나는경우 있어서 
                    ApplyRequest?.Invoke(this, new ObjectEventArgs<Advancedsetting>(Setting));
                    AddPropertyChanged(Setting);
                });


            this.BeforeClose += UcSoakingAdvancedSettingViewModel_BeforeClose;
        }

        private void UcSoakingAdvancedSettingViewModel_BeforeClose(object sender, EventArgs e)
        {
            RemovePropertyChanged(Setting);
        }

        public ObjectValidator<object> Validator;
        public bool ValidateCheckFunc(object obj, out string resultMessage)
        {
            resultMessage = "";
            if (Validator == null)
                return true;

            resultMessage = "OK";
            return Validator(obj, out resultMessage);
        }

        private void ValidateCheck(Advancedsetting list)
        {
            bool? result = null;
            string message = "";

            if (Validator == null) // 유효성 검증 하지 않음. 
            {
                result = true;

            }
            else
            {
                result = Validator.Invoke(list, out message);
            }

            IsValid = result;
            ValidateResult = message;
        }
    }
}

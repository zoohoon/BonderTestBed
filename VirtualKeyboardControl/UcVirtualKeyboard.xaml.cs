using LogModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace VirtualKeyboardControl
{
    /*
     * Flag 방식
     */
    [Flags]
    public enum KB_TYPE : uint
    {
        NONE = 0,
        DECIMAL = 1 << 0,
        FLOAT = 1 << 1,
        ALPHABET = 1 << 2,
        SPECIAL = 1 << 3,
        PASSWORD = 1 << 4,
        ALL = int.MaxValue
    };
    /// <summary>
    /// UcVirtualKeyboard.xaml에 대한 상호 작용 논리
    /// </summary>

    /*
     * Virtual Keyboard, User Control
     */
    public partial class UcVirtualKeyboard : UserControl, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> DEP EnterKeyCommand : Virtual Keyboard에서 Enter Key 눌렀을 때 호출되는 Call Back
        public static readonly DependencyProperty EnterKeyCommandProperty =
            DependencyProperty.Register(
                nameof(EnterKeyCommand),
                typeof(ICommand),
                typeof(UcVirtualKeyboard),
                new FrameworkPropertyMetadata(null));
        public ICommand EnterKeyCommand
        {
            get { return (ICommand)this.GetValue(EnterKeyCommandProperty); }
            set { this.SetValue(EnterKeyCommandProperty, value); }
        }
        #endregion

        #region ==> DEP TextBrush : Textbox 색깔
        public static readonly DependencyProperty TextBrushProperty =
            DependencyProperty.Register(
                nameof(TextBrush),
                typeof(Brush),
                typeof(UcVirtualKeyboard),
                new FrameworkPropertyMetadata(null));
        public Brush TextBrush
        {
            get { return (Brush)GetValue(TextBrushProperty); }
            set { SetValue(TextBrushProperty, value); }
        }
        #endregion

        #region ==> DEP TextData : Textbox의 Text
        public static readonly DependencyProperty TextDataProperty =
            DependencyProperty.Register(
                nameof(TextData)
                , typeof(String),
                typeof(UcVirtualKeyboard),
                new FrameworkPropertyMetadata(String.Empty));

        public String TextData
        {
            get { return (String)this.GetValue(TextDataProperty); }
            set { this.SetValue(TextDataProperty, value); }
        }
        #endregion

        #region ==> DEP TitleBarVisible : TitleBar 가시성
        public static readonly DependencyProperty TitleBarVisibleProperty =
            DependencyProperty.Register(
                nameof(TitleBarVisible),
                typeof(Visibility),
                typeof(UcVirtualKeyboard),
                new FrameworkPropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnTextChangePropertyChanged)));
        public Visibility TitleBarVisible
        {
            get { return (Visibility)GetValue(TitleBarVisibleProperty); }
            set { SetValue(TitleBarVisibleProperty, value); }
        }
        private static void OnTextChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcVirtualKeyboard window = d as UcVirtualKeyboard;

            Visibility newText = (Visibility)e.NewValue;
            if (newText == Visibility.Visible)
            {
                window.exitBtnWidthCol.Width = new GridLength(1, GridUnitType.Star);
                window.titleBar.Visibility = Visibility.Visible;
                window.exitBtn.Visibility = Visibility.Visible;
            }
            else if (newText == Visibility.Collapsed ||
                    newText == Visibility.Hidden)
            {
                window.exitBtnWidthCol.Width = new GridLength(0);
                window.titleBar.Visibility = Visibility.Visible;
                window.exitBtn.Visibility = Visibility.Collapsed;
            }
        }
        #endregion


        private List<UcKey> _AlphabetKeyList;//==> 알파뱃 키 모음
        private List<UcKey> _DecimalKeyList;//==> 숫자 키 모음
        private List<UcKey> _SpecialKeyList;//==> 특수 문자 키 모음

        private bool _EnterKeyDown;
        private bool _PasswordInputMode = false;//==> true이면 Textbox의 문자가 * 로 보임
        private String _Text = String.Empty;//==> Textbox 창의 text
        private String _InitText = String.Empty;//==> Virtual Keyboard 처음 실행 될때 초기 Textbox 창의 text 값

        public int MinCharLen { get; set; }//==> text 입력 해야할 최소한의 문자열 길이

        #region ==> MaxCharLen, XAML에서 Binding 되어 있음
        public int _MaxCharLen;
        public int MaxCharLen
        {
            get { return _MaxCharLen; }
            set
            {
                if (_MaxCharLen != value)
                {
                    _MaxCharLen = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private Window _window;
        private KB_TYPE _KbType;
        public UcVirtualKeyboard()
        {
            InitializeComponent();

            #region ==> DEP Textbox Background Binding
            Binding bindTextBrush = new Binding();
            bindTextBrush.Path = new PropertyPath(nameof(TextBrush));
            bindTextBrush.Source = this;
            bindTextBrush.Mode = BindingMode.TwoWay;
            bindTextBrush.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(inputTextBox, TextBox.BackgroundProperty, bindTextBrush);
            #endregion

            #region ==> DEP Textbox text Binding
            Binding bindBtnText = new Binding();
            bindBtnText.Path = new PropertyPath(nameof(TextData));
            bindBtnText.Source = this;
            bindBtnText.Mode = BindingMode.TwoWay;
            bindBtnText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(inputTextBox, TextBox.TextProperty, bindBtnText);
            #endregion

            #region ==> DEP Title Bar
            //Binding bindTitleBar1 = new Binding();
            //bindTitleBar1.Path = new PropertyPath(nameof(TitleBarVisible));
            //bindTitleBar1.Source = this;
            //bindTitleBar1.Mode = BindingMode.TwoWay;
            //bindTitleBar1.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //BindingOperations.SetBinding(titleBar, TextBlock.VisibilityProperty, bindTitleBar1);

            //Binding bindTitleBar2 = new Binding();
            //bindTitleBar2.Path = new PropertyPath(nameof(TitleBarVisible));
            //bindTitleBar2.Source = this;
            //bindTitleBar2.Mode = BindingMode.TwoWay;
            //bindTitleBar2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //BindingOperations.SetBinding(exitBtn, TextBlock.VisibilityProperty, bindTitleBar2);
            #endregion

            _KbType = KB_TYPE.DECIMAL | KB_TYPE.FLOAT | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL;

            inputTextBox.ContextMenu = null;

            MinCharLen = 0;
            MaxCharLen = 15;

            InitKeyList();
        }
        public UcVirtualKeyboard(String text, KB_TYPE kbType, int minCharLen = 0, int maxCharLen = 15)
            : this()
        {
            _Text = text ?? String.Empty;
            _InitText = _Text;//==> Virtual Keyboard Popup 시 초기화된 Text

            _KbType = kbType;

            TextBrush = Brushes.LightGray;

            TextData = _Text;
            inputTextBox.Text = _Text;
            inputTextBox.SelectionStart = 0;
            inputTextBox.SelectionLength = inputTextBox.Text.Length;

            MinCharLen = minCharLen;
            MaxCharLen = maxCharLen;

            //==> keyboard 타입 분류

            //==> Password Type이면 Text를 '*'로 바꿔서 표시 한다.
            if ((_KbType & KB_TYPE.PASSWORD) == KB_TYPE.PASSWORD)
            {
                _PasswordInputMode = true;
                ChangePasswordText(_Text);
                return;
            }

            key_CapsLock.IsEnabled = false;
            key_CapsLock.Opacity = 0.5;
            key_SPACE.IsEnabled = false;
            key_SPACE.Opacity = 0.5;

            //==> 알파벳
            if ((_KbType & KB_TYPE.ALPHABET) == 0)
            {
                foreach (UcKey key in _AlphabetKeyList)
                {
                    key.IsEnabled = false;
                    key.Opacity = 0.5;
                }
            }
            else
            {
                key_CapsLock.IsEnabled = true;
                key_CapsLock.Opacity = 1;
                key_SPACE.IsEnabled = true;
                key_SPACE.Opacity = 1;
            }

            //==> 숫자
            if ((_KbType & KB_TYPE.DECIMAL) == 0)
            {
                foreach (UcKey key in _DecimalKeyList)
                {
                    key.IsEnabled = false;
                    key.Opacity = 0.5;
                }
            }

            //==> 특수 문자
            if ((_KbType & KB_TYPE.SPECIAL) == 0)
            {
                foreach (UcKey key in _SpecialKeyList)
                {
                    key.IsEnabled = false;
                    key.Opacity = 0.5;
                }
            }

            //==> '.' 문자
            if ((_KbType & KB_TYPE.FLOAT) == 0)
            {
                key_Dot.IsEnabled = false;
                key_Dot.Opacity = 0.5;
            }

            exitBtn.Visibility = Visibility.Visible;
            titleBar.Visibility = Visibility.Visible;
            _EnterKeyDown = false;
        }
        /*
         * Virtual Keyboard 처음 Popup 시 한번 호출 됨
         */
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //==> Virtual Keyboard PopUp시 처음 Textbox에 포커스 맞게끔
            inputTextBox.Focus();
            inputTextBox.ForceCursor = true;
        }
        /*
         * UcKey 들을 각 List에 추가함으로서 분류 작업
         */
        private void InitKeyList()
        {
            #region ==> Init Key List
            _AlphabetKeyList = new List<UcKey>();
            _DecimalKeyList = new List<UcKey>();
            _SpecialKeyList = new List<UcKey>();

            //==> Alphabet
            _AlphabetKeyList.Add(key_a);
            _AlphabetKeyList.Add(key_b);
            _AlphabetKeyList.Add(key_c);
            _AlphabetKeyList.Add(key_d);
            _AlphabetKeyList.Add(key_e);
            _AlphabetKeyList.Add(key_f);
            _AlphabetKeyList.Add(key_g);
            _AlphabetKeyList.Add(key_h);
            _AlphabetKeyList.Add(key_i);
            _AlphabetKeyList.Add(key_j);
            _AlphabetKeyList.Add(key_k);
            _AlphabetKeyList.Add(key_l);
            _AlphabetKeyList.Add(key_m);
            _AlphabetKeyList.Add(key_n);
            _AlphabetKeyList.Add(key_o);
            _AlphabetKeyList.Add(key_p);
            _AlphabetKeyList.Add(key_q);
            _AlphabetKeyList.Add(key_r);
            _AlphabetKeyList.Add(key_s);
            _AlphabetKeyList.Add(key_t);
            _AlphabetKeyList.Add(key_u);
            _AlphabetKeyList.Add(key_v);
            _AlphabetKeyList.Add(key_w);
            _AlphabetKeyList.Add(key_x);
            _AlphabetKeyList.Add(key_y);
            _AlphabetKeyList.Add(key_z);
            //==> Decimal
            _DecimalKeyList.Add(key_1);
            _DecimalKeyList.Add(key_2);
            _DecimalKeyList.Add(key_3);
            _DecimalKeyList.Add(key_4);
            _DecimalKeyList.Add(key_5);
            _DecimalKeyList.Add(key_6);
            _DecimalKeyList.Add(key_7);
            _DecimalKeyList.Add(key_8);
            _DecimalKeyList.Add(key_9);
            _DecimalKeyList.Add(key_0);
            _DecimalKeyList.Add(key_Minus);
            //==> Special
            _SpecialKeyList.Add(key_Plus);
            _SpecialKeyList.Add(key_Sharp);
            _SpecialKeyList.Add(key_Colon);
            _SpecialKeyList.Add(key_Alpha);
            _SpecialKeyList.Add(key_Under);
            _SpecialKeyList.Add(key_Slash);
            _SpecialKeyList.Add(key_Asterisk);
            _SpecialKeyList.Add(key_BSlash);
            #endregion
        }

        #region ==> KEYBOARD
        /*
         * Input TextBox Event
         */
        private void inputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int caretIndex;
            int selectionStart;
            int selectionLength;
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.Enter:
                    EnterKeyExecute();
                    e.Handled = true;//==> 키 입력을 막는다.
                    break;
                case Key.Left:
                case Key.Right:
                case Key.End:
                case Key.Home:
                    break;
                case Key.Delete:
                    GetKeyDELSelectedArea(out selectionStart, out selectionLength);
                    _Text = RemoveChar(_Text, selectionStart, selectionLength);
                    break;
                case Key.Back:
                    GetKeyBACKSelectedArea(out selectionStart, out selectionLength);
                    _Text = RemoveChar(_Text, selectionStart, selectionLength);
                    break;
                case Key.CapsLock:
                    InputKeyHandler(key_CapsLock);
                    break;
                default:

                    if (inputTextBox.Text.Length + 1 > MaxCharLen && inputTextBox.SelectionLength < 1)
                    {
                        //==> 텍스트 문자열이 최대 문자 갯수를 넘는것을 방지 한다.
                        //==> 텍스트 박스 문자열은 이미 최대치를 초과했지만 텍스트 박스 
                        //==> 문자열을 하나도 선택 안 해서 지울 수 있는 상태로 안 만들었다.
                        e.Handled = true;
                    }
                    else
                    {
                        char ch = KeyboardKeyToChar(e.Key);
                        if (ch == '\x00')
                        {
                            e.Handled = true;
                        }
                        else
                        {
                            GetKeyCharSelectedArea(out caretIndex, out selectionStart, out selectionLength);
                            _Text = InsertChar(_Text, ch, caretIndex, selectionStart, selectionLength);
                        }
                    }

                    break;
            }
        }
        /*
         * Input TextBox Event
         */
        private void inputTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (_PasswordInputMode)
            {
                char ch = KeyboardKeyToChar(e.Key);
                if (ch == '\x00')
                    return;

                //==> Only Printable Character
                ChangePasswordText(inputTextBox.Text);
            }
            else
            {
                if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    (sender as TextBox).Copy();
                }
                if (e.Key == Key.X && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    (sender as TextBox).Cut();
                    _Text = inputTextBox.Text;
                }
                if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    (sender as TextBox).Paste();
                    _Text = inputTextBox.Text;
                }
            }
        }
        /*
         * Keyboard로부터 입력된 Key를 char로 변환
         */
        private char KeyboardKeyToChar(Key key)
        {

            if (Keyboard.IsKeyDown(Key.LeftAlt) ||
                Keyboard.IsKeyDown(Key.RightAlt) ||
                Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightAlt))
            {
                return '\x00';
            }

            bool caplock = Console.CapsLock;
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) ||
                                    Keyboard.IsKeyDown(Key.RightShift);
            bool iscap = (caplock && !shift) || (!caplock && shift);


            if ((_KbType & KB_TYPE.ALPHABET) == KB_TYPE.ALPHABET ||
                (_KbType & KB_TYPE.PASSWORD) == KB_TYPE.PASSWORD)
            {
                //==> Main Pad
                if (key == Key.A) return (iscap ? 'A' : 'a');
                else if (key == Key.B) return (iscap ? 'B' : 'b');
                else if (key == Key.C) return (iscap ? 'C' : 'c');
                else if (key == Key.D) return (iscap ? 'D' : 'd');
                else if (key == Key.E) return (iscap ? 'E' : 'e');
                else if (key == Key.F) return (iscap ? 'F' : 'f');
                else if (key == Key.G) return (iscap ? 'G' : 'g');
                else if (key == Key.H) return (iscap ? 'H' : 'h');
                else if (key == Key.I) return (iscap ? 'I' : 'i');
                else if (key == Key.J) return (iscap ? 'J' : 'j');
                else if (key == Key.K) return (iscap ? 'K' : 'k');
                else if (key == Key.L) return (iscap ? 'L' : 'l');
                else if (key == Key.M) return (iscap ? 'M' : 'm');
                else if (key == Key.N) return (iscap ? 'N' : 'n');
                else if (key == Key.O) return (iscap ? 'O' : 'o');
                else if (key == Key.P) return (iscap ? 'P' : 'p');
                else if (key == Key.Q) return (iscap ? 'Q' : 'q');
                else if (key == Key.R) return (iscap ? 'R' : 'r');
                else if (key == Key.S) return (iscap ? 'S' : 's');
                else if (key == Key.T) return (iscap ? 'T' : 't');
                else if (key == Key.U) return (iscap ? 'U' : 'u');
                else if (key == Key.V) return (iscap ? 'V' : 'v');
                else if (key == Key.W) return (iscap ? 'W' : 'w');
                else if (key == Key.X) return (iscap ? 'X' : 'x');
                else if (key == Key.Y) return (iscap ? 'Y' : 'y');
                else if (key == Key.Z) return (iscap ? 'Z' : 'z');
            }

            // Number Pad
            if ((_KbType & KB_TYPE.DECIMAL) == KB_TYPE.DECIMAL ||
                (_KbType & KB_TYPE.PASSWORD) == KB_TYPE.PASSWORD)
            {
                if (key == Key.NumPad0) return '0';
                else if (key == Key.NumPad1) return '1';
                else if (key == Key.NumPad2) return '2';
                else if (key == Key.NumPad3) return '3';
                else if (key == Key.NumPad4) return '4';
                else if (key == Key.NumPad5) return '5';
                else if (key == Key.NumPad6) return '6';
                else if (key == Key.NumPad7) return '7';
                else if (key == Key.NumPad8) return '8';
                else if (key == Key.NumPad9) return '9';
                else if (key == Key.Subtract) return ('-');
                else if (shift == false)
                {
                    if (key == Key.D0) return ('0');
                    else if (key == Key.D1) return ('1');
                    else if (key == Key.D2) return ('2');
                    else if (key == Key.D3) return ('3');
                    else if (key == Key.D4) return ('4');
                    else if (key == Key.D5) return ('5');
                    else if (key == Key.D6) return ('6');
                    else if (key == Key.D7) return ('7');
                    else if (key == Key.D8) return ('8');
                    else if (key == Key.D9) return ('9');
                    else if (key == Key.OemMinus) return ('-');
                }
            }

            if ((_KbType & KB_TYPE.SPECIAL) == KB_TYPE.SPECIAL ||
                (_KbType & KB_TYPE.PASSWORD) == KB_TYPE.PASSWORD ||
                (_KbType & KB_TYPE.ALPHABET) == KB_TYPE.ALPHABET)
            {
                //==> Main Pad
                if (key == Key.OemPlus) return (shift ? '+' : '=');
                else if (key == Key.OemMinus) return (shift ? '_' : '-');
                else if (key == Key.OemQuestion) return (shift ? '?' : '/');
                else if (key == Key.OemComma) return (shift ? '<' : ',');
                else if (key == Key.OemPeriod) return (shift ? '>' : '.');
                else if (key == Key.OemOpenBrackets) return (shift ? '{' : '[');
                else if (key == Key.OemQuotes) return (shift ? '"' : '\'');
                else if (key == Key.Oem1) return (shift ? ':' : ';');
                else if (key == Key.Oem3) return (shift ? '~' : '`');
                else if (key == Key.Oem5) return (shift ? '|' : '\\');
                else if (key == Key.Oem6) return (shift ? '}' : ']');
                else if (key == Key.Space) return ' ';

                //==> Number Pad
                else if (key == Key.Subtract) return '-';
                else if (key == Key.Add) return '+';
                else if (key == Key.Divide) return '/';
                else if (key == Key.Multiply) return '*';
                else if (shift)
                {
                    if (key == Key.D0) return (')');
                    else if (key == Key.D1) return ('!');
                    else if (key == Key.D2) return ('@');
                    else if (key == Key.D3) return ('#');
                    else if (key == Key.D4) return ('$');
                    else if (key == Key.D5) return ('%');
                    else if (key == Key.D6) return ('^');
                    else if (key == Key.D7) return ('&');
                    else if (key == Key.D8) return ('*');
                    else if (key == Key.D9) return ('(');
                }
            }

            if ((_KbType & KB_TYPE.FLOAT) == KB_TYPE.FLOAT ||
                (_KbType & KB_TYPE.PASSWORD) == KB_TYPE.PASSWORD)
            {
                if (key == Key.Decimal) return '.'; // number pad dot
                else if (key == Key.OemPeriod) return '.'; // tenkeless dot
            }

            return '\x00';
        }
        #endregion

        #region ==> MOUSE
        /*
         * UcKey Event
         */
        private void UcKey_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            InputKeyHandler((UcKey)sender);
        }
        /*
         * UcKey Event
         */
        private void UcKey_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //==> UcKey_PreviewMouseDown 과의 시간 차이로 인해 일반 Text로 잠깐 보였다가 '*'로 바뀌는 효과를 줄 수 있다.
            if (_PasswordInputMode)
            {
                //==> Paswword 모드일때 Input Text Box의 text를 *fh qusghks
                ChangePasswordText(inputTextBox.Text);
            }
        }

        private void UcKey_EnterMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((UcKey)sender == key_ENTER && _EnterKeyDown)//==> ENTER KEY
                {
                    EnterKeyExecute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void EnterKeyExecute()
        {
            TextData = _Text;

            if (MinCharLen <= TextData.Length && TextData.Length <= MaxCharLen)
            {
                EnterKeyCommand?.Execute(this);
                Close();
            }
        }

        #endregion
        /*
         * UcKey로부터 Key 입력 받았을 때의 처리
         */
        private void InputKeyHandler(UcKey key)
        {
            if (key == key_Clr)//==> CLEAR KEY
            {
                ClearTextBoxChar();
            }
            else if (key == key_ENTER)//==> ENTER KEY
            {
                _EnterKeyDown = true;
            }
            else if (key == key_Back)//==> BACK KEY
            {
                int selectionStart;
                int selectionLength;

                GetKeyBACKSelectedArea(out selectionStart, out selectionLength);

                inputTextBox.Text = RemoveChar(inputTextBox.Text, selectionStart, selectionLength);
                inputTextBox.CaretIndex = selectionStart;

                //==> inputTextBox.Text 와 _Text 문자열은 따로 처리
                //==> 이유는 Password 모드 때문에(inputTextBox.Text 에서는 * 로 표현 해야 할 때가 있다.)
                _Text = RemoveChar(_Text, selectionStart, selectionLength);
            }
            else if (key == key_DEL)//==> DEL KEY
            {
                int selectionStart;
                int selectionLength;

                GetKeyDELSelectedArea(out selectionStart, out selectionLength);

                inputTextBox.Text = RemoveChar(inputTextBox.Text, selectionStart, selectionLength);
                inputTextBox.CaretIndex = selectionStart;

                //==> inputTextBox.Text 와 _Text 문자열은 따로 처리
                //==> 이유는 Password 모드 때문에(inputTextBox.Text 에서는 * 로 표현 해야 할 때가 있다.)
                _Text = RemoveChar(_Text, selectionStart, selectionLength);
            }
            else if (key == key_Lt)//==> LEFT KEY
            {
                if (inputTextBox.CaretIndex > 0)
                {
                    inputTextBox.CaretIndex--;
                }
            }
            else if (key == key_Gt)//==> RIGHT KEY
            {
                if (inputTextBox.CaretIndex < inputTextBox.Text.Length)
                {
                    inputTextBox.CaretIndex++;
                }
            }
            else if (key == key_CapsLock)//==> CAPS LOCK KEY
            {
                bool isLowerCase = false;

                char ch;
                char.TryParse(_AlphabetKeyList[0].KeyText, out ch);
                if (char.IsLower(ch))
                {
                    isLowerCase = true;
                }

                foreach (UcKey uckey in _AlphabetKeyList)
                {
                    String keyChar = String.Empty;
                    if (isLowerCase)
                    {
                        keyChar = uckey.KeyText.ToUpper();
                    }
                    else
                    {
                        keyChar = uckey.KeyText.ToLower();
                    }

                    uckey.KeyText = keyChar;
                }
            }
            else//==> Decimal, Alphabet, Special Key
            {
                char inputChar;
                UcKey inputKey = key;
                if (char.TryParse(inputKey.KeyText, out inputChar))
                {
                    int caretIndex;
                    int selectionStart;
                    int selectionLength;
                    GetKeyCharSelectedArea(out caretIndex, out selectionStart, out selectionLength);

                    inputTextBox.Text = InsertChar(inputTextBox.Text, inputChar, caretIndex, selectionStart, selectionLength);
                    inputTextBox.CaretIndex = caretIndex + 1;

                    //==> inputTextBox.Text 와 _Text 문자열은 따로 처리
                    //==> 이유는 Password 모드 때문에(inputTextBox.Text 에서는 * 로 표현 해야 할 때가 있다.)
                    _Text = InsertChar(_Text, inputChar, caretIndex, selectionStart, selectionLength);
                }
            }
        }
        /*
         * caretIndex, selectionStart, selectionLength 반영해서
         * key 를 text에 추가
         */
        private String InsertChar(String text, char key, int caretIndex, int selectionStart, int selectionLength)
        {
            //==> Drag 되어서 문자를 지우게 되면 Key를 입력 받을 수 있는 공간이 나온다.
            if (text.Length + 1 > MaxCharLen && selectionLength < 1)
            {
                return text;
            }

            StringBuilder stb = new StringBuilder(text);
            //==> 문자열 Drag 된 부분은 삭제
            stb.Remove(selectionStart, selectionLength);//==> SelectionStart는 CaretIndex와 같다.

            //==> 문자 삽입
            stb.Insert(caretIndex, key);

            return stb.ToString();
        }
        /*
         * Selection Start, Selection Length 반영해서
         * text에서 부터 일부 문자열 삭제한 문자열 반환
         */
        private String RemoveChar(String text, int selectionStart, int selectionLength)
        {
            if (selectionStart < 0 || selectionStart >= text.Length)
                return text;

            StringBuilder stb = new StringBuilder(text);
            stb.Remove(selectionStart, selectionLength);

            return stb.ToString();
        }
        /*
         * BACK Key를 눌렀을 때 Input Text Box 지워야할 text의 시작점과 길이 반환
         */
        void GetKeyBACKSelectedArea(out int selectionStart, out int selectionLength)
        {
            if (inputTextBox.SelectionLength > 0)
            {
                //==> Input Text Box의 Block이 잡혀 있음
                selectionStart = inputTextBox.SelectionStart;
                selectionLength = inputTextBox.SelectionLength;
            }
            else
            {
                //==> Input Text Box의 block이 잡혀 있지 않음
                if (inputTextBox.CaretIndex == 0)
                {
                    //==> caret이 Input Text box에서 맨 앞에 있음
                    selectionStart = 0;
                    selectionLength = 0;
                }
                else
                {
                    selectionStart = inputTextBox.CaretIndex - 1;
                    selectionLength = 1;
                }
            }
        }
        /*
         * DEL Key를 눌렀을 때 Input Text Box 지워야할 text의 시작점과 길이 반환
         */
        void GetKeyDELSelectedArea(out int selectionStart, out int selectionLength)
        {
            if (inputTextBox.SelectionLength > 0)
            {
                //==> Input Text Box의 Block이 잡혀 있음
                selectionStart = inputTextBox.SelectionStart;
                selectionLength = inputTextBox.SelectionLength;
            }
            else
            {
                //==> Input Text Box의 block이 잡혀 있지 않음
                if (inputTextBox.CaretIndex == inputTextBox.Text.Length)
                {
                    //==> caret이 Input Text box에서 맨 끝에 있음
                    selectionStart = inputTextBox.CaretIndex;
                    selectionLength = 0;
                }
                else
                {
                    selectionStart = inputTextBox.CaretIndex;
                    selectionLength = 1;
                }
            }
        }
        /*
         * Input Text Box의 Text를 Password 형태로 바꿈
         */
        private void ChangePasswordText(String text)
        {
            int backupCaretIndex;
            int backupSelectionStart;
            int bacupSelectionLength;
            GetKeyCharSelectedArea(out backupCaretIndex, out backupSelectionStart, out bacupSelectionLength);

            String passwordStr = String.Empty;

            for (int i = 0; i < text.Length; i++)
                passwordStr += '*';

            inputTextBox.Text = passwordStr;//==> Text를 *로 변경
            inputTextBox.CaretIndex = backupCaretIndex;//==> CaretIndex 복구
            inputTextBox.SelectionStart = backupSelectionStart;//==> SelectionStart 복구
            inputTextBox.SelectionLength = bacupSelectionLength;//==> SelectionLength 복구
        }
        /*
         * Text input box에서의 text 선택 범위 획득
         */
        private void GetKeyCharSelectedArea(out int caretIndex, out int selectionStart, out int selectionLength)
        {
            caretIndex = inputTextBox.CaretIndex;//==> CaretIndex : TextBox에서 커서가 위치한 인덱스
            selectionStart = inputTextBox.SelectionStart;//==> SelectionStart : TextBox에서 Text가 Block 되었을때 시작 인덱스
            selectionLength = inputTextBox.SelectionLength;//==> SelectionLength : TextBox에서 Text가 Block 되었을때 블록 시작 인덱스부터의 길이
        }
        /*
         * input Text Box Clear
         */
        private void ClearTextBoxChar()
        {
            inputTextBox.Text = String.Empty;
            //if (_PasswordInputMode)
            _Text = String.Empty;
        }
        /*
         * Virtual Keyboard 종료
         */
        private void Close()
        {
            _window?.Close();
        }
        /*
         * Window 종료 메시지를 호출 하기 위해 window 개체를 받는다.
         */
        public void SetWindow(Window window)
        {
            _window = window;
        }


        #region ==> Exit Button Event
        /*
         * Mouse Event
         */
        private void exitBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            exitBtn.Opacity = 0.5;//==>눌렀을때 Exit Button이 약간 흐릿한 효과를 보여주기 위해
            TextData = _InitText;//==> exit Button을 눌렀으니 최종적인 입력 값은 초기 입력 값이다.
            Close();
        }
        /*
         * Mouse Event
         */
        private void exitBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //==> Exit Button을 눌렀다 때었을때 다시 선명하게 표시
            exitBtn.Opacity = 1;
        }
        /*
         * Mouse Event
         */
        private void exitBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            //==> Exit Button 누르고 다르데로 드래그 하면 다시 선명하게 표시
            exitBtn.Opacity = 1;
        }
        #endregion
    }
}

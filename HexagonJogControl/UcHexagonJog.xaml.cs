using ProberInterfaces.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace HexagonJogControl
{
    using CUIServices;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using CustomMoveView;
    using LogModule;


    /// <summary>
    /// UcHexagonJog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcHexagonJog : UserControl, IFactoryModule, ICUIControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public Guid GUID { get; set; }
        private int _MaskingLevel;
        public int MaskingLevel
        {
            get
            {
                _MaskingLevel = CUIService.GetMaskingLevel(this.GUID);
                return _MaskingLevel;
            }
            set
            {
                if (value != _MaskingLevel)
                {
                    _MaskingLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsReleaseMode;
        public bool IsReleaseMode
        {
            get { return _IsReleaseMode; }
            set
            {
                if (value != _IsReleaseMode)
                {
                    _IsReleaseMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BindingBase _IsEnableBindingBase;
        public BindingBase IsEnableBindingBase
        {
            get { return _IsEnableBindingBase; }
            set
            {
                if (value != _IsEnableBindingBase)
                {
                    _IsEnableBindingBase = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool Lockable { get; set; } = true;
        public bool InnerLockable { get; set; } = true;
        public List<int> AvoidLockHashCodes { get; set; }

        #region ==> DEP AssignedCamera
        public static readonly DependencyProperty AssignedCameraProperty =
            DependencyProperty.Register(nameof(AssignedCamera)
                , typeof(ICamera),
                typeof(UcHexagonJog),
                new FrameworkPropertyMetadata(null));
        public ICamera AssignedCamera
        {
            get { return (ICamera)this.GetValue(AssignedCameraProperty); }
            set { this.SetValue(AssignedCameraProperty, value); }
        }
        private EnumProberCam CurCameraType
        {
            get
            {
                if (AssignedCamera == null)
                    return EnumProberCam.UNDEFINED;
                return AssignedCamera.CameraChannel.Type;
            }
        }

        public static readonly DependencyProperty JogTypeProperty = DependencyProperty.Register(nameof(JogType), typeof(JogMode), typeof(UcHexagonJog), new FrameworkPropertyMetadata(JogMode.Normal, new PropertyChangedCallback(JogTypePropertyChanged)));
        public JogMode JogType
        {
            get { return (JogMode)this.GetValue(JogTypeProperty); }
            set { this.SetValue(JogTypeProperty, value); }
        }

        private static void JogTypePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                UcHexagonJog hj = null;

                if (sender is UcHexagonJog)
                {
                    hj = (UcHexagonJog)sender;

                    if (hj.JogType == JogMode.Normal)
                    {
                        hj.ChangeStickUI(zMoveBtnEnable: true, distanceSwitchEnable: true, indexMoveBtnOpacity: 1);
                    }
                    else
                    {
                        hj.ChangeStickUI(zMoveBtnEnable: true, distanceSwitchEnable: true, indexMoveBtnOpacity: 0.25);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> ViewModel Interface
        public static readonly DependencyProperty SetMoveZOffsetEnableProperty =
            DependencyProperty.Register("SetMoveZOffsetEnable", typeof(bool),
                typeof(UcHexagonJog), new FrameworkPropertyMetadata(null));
        public bool SetMoveZOffsetEnable
        {
            get { return (bool)this.GetValue(SetMoveZOffsetEnableProperty); }
            set { this.SetValue(SetMoveZOffsetEnableProperty, value); }
        }
        #endregion

        private IHexagonJogViewModel _HexagonJogViewModel;

        //==> Brush

        //==> UI Button List
        private List<UserControl> _IndexMoveBtnList;
        private List<UserControl> _XYStepMoveBtnList;
        private List<UserControl> _XYStepDiagonalMoveBtnList;
        private List<UserControl> _ZStepMoveBtnList;

        private Task _ContinuousStepMoveEventTask;
        private Task _ContinuousIndexMoveEventTask;

        public UcHexagonJog()
        {
            InitializeComponent();

            //==> 8가지 Index Move 버튼들을 리스트로 관리
            _IndexMoveBtnList = new List<UserControl>();
            _IndexMoveBtnList.Add(IndexMoveUp);
            _IndexMoveBtnList.Add(IndexMoveRightUpBtn);
            _IndexMoveBtnList.Add(IndexMoveRight);
            _IndexMoveBtnList.Add(IndexMoveRightDown);
            _IndexMoveBtnList.Add(IndexMoveDown);
            _IndexMoveBtnList.Add(IndexMoveLeftDown);
            _IndexMoveBtnList.Add(IndexMoveLeft);
            _IndexMoveBtnList.Add(IndexMoveLeftUpBtn);

            //==> 8가지 Step Move 버튼들을 리스트로 관리
            _XYStepMoveBtnList = new List<UserControl>();
            _XYStepMoveBtnList.Add(StepBtnUp);
            _XYStepMoveBtnList.Add(StepBtnRight);
            _XYStepMoveBtnList.Add(StepBtnDown);
            _XYStepMoveBtnList.Add(StepBtnLeft);

            _XYStepDiagonalMoveBtnList = new List<UserControl>();
            _XYStepDiagonalMoveBtnList.Add(StepBtnRightUp);
            _XYStepDiagonalMoveBtnList.Add(StepBtnRightDown);
            _XYStepDiagonalMoveBtnList.Add(StepBtnLeftDown);
            _XYStepDiagonalMoveBtnList.Add(StepBtnLeftUp);

            _ZStepMoveBtnList = new List<UserControl>();
            _ZStepMoveBtnList.Add(StepBtnZBigUp);
            _ZStepMoveBtnList.Add(StepBtnZBigDown);
            _ZStepMoveBtnList.Add(StepBtnZSmallDown);
            _ZStepMoveBtnList.Add(StepBtnZSmallUp);

            XYZSwitch_Unchecked(null, null);
        }
        #region ==> Z Axis Switch
        private void XYZSwitch_Checked(object sender, RoutedEventArgs e)
        {
            //==> Z Axis 활성화
            ChangeStickUI(zMoveBtnEnable: false, distanceSwitchEnable: true,indexMoveBtnOpacity: 0.25);
        }
        private void XYZSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            //==> Z Axis 비 활성화
            ChangeStickUI(zMoveBtnEnable: true, distanceSwitchEnable: true, indexMoveBtnOpacity: 1);
        }

        private void ChangeStickUI(bool zMoveBtnEnable, bool distanceSwitchEnable, double indexMoveBtnOpacity)
        {
            Visibility xyAxisVisi = zMoveBtnEnable ? Visibility.Visible : Visibility.Collapsed;
            Visibility zAxisVisi = zMoveBtnEnable ? Visibility.Collapsed : Visibility.Visible;

            if(JogType == JogMode.Normal)
            {
                foreach (UserControl uc in _IndexMoveBtnList)
                {
                    uc.Opacity = indexMoveBtnOpacity;
                    uc.IsEnabled = zMoveBtnEnable;
                }

                foreach (UserControl btn in _XYStepMoveBtnList)
                {
                    btn.Visibility = xyAxisVisi;
                }

                foreach (UserControl btn in _XYStepDiagonalMoveBtnList)
                {
                    btn.Visibility = Visibility.Collapsed;
                }
            }
            else if (JogType == JogMode.DiagonalAll)
            {
                foreach (UserControl uc in _IndexMoveBtnList)
                {
                    uc.Opacity = indexMoveBtnOpacity;
                    uc.IsEnabled = false;
                }

                foreach (UserControl btn in _XYStepMoveBtnList)
                {
                    btn.Visibility = Visibility.Collapsed;
                }

                foreach (UserControl btn in _XYStepDiagonalMoveBtnList)
                {
                    btn.Visibility = xyAxisVisi;
                }
            }
            else if (JogType == JogMode.DiagonalRightUpLeftDown || JogType == JogMode.DiagonalLeftUpRightDown)
            {
                foreach (UserControl uc in _IndexMoveBtnList)
                {
                    uc.Opacity = indexMoveBtnOpacity;
                    uc.IsEnabled = false;
                }

                foreach (UserControl btn in _XYStepMoveBtnList)
                {
                    btn.Visibility = Visibility.Collapsed;
                }

                foreach (UserControl btn in _XYStepDiagonalMoveBtnList)
                {
                    if(JogType == JogMode.DiagonalRightUpLeftDown)
                    {
                        if (btn == StepBtnRightUp || btn == StepBtnLeftDown)
                        {
                            btn.Visibility = xyAxisVisi;
                        }
                        else
                        {
                            btn.Visibility = Visibility.Collapsed;
                        }
                    }
                    else if(JogType == JogMode.DiagonalLeftUpRightDown)
                    {
                        if (btn == StepBtnLeftUp || btn == StepBtnRightDown)
                        {
                            btn.Visibility = xyAxisVisi;
                        }
                        else
                        {
                            btn.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }

            foreach (UserControl btn in _ZStepMoveBtnList)
            {
                btn.Visibility = zAxisVisi;
            }

            StepMoveDistanceSwitch.IsEnabled = distanceSwitchEnable;
        }
        #endregion

        #region ==> Index Move

        private void BtnTrapezoidArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (_HexagonJogViewModel == null)
                _HexagonJogViewModel = (IHexagonJogViewModel)this.DataContext;
            //_HexagonJogViewModel = (HexagonJogViewModel)this.DataContext;

            if (_ContinuousIndexMoveEventTask?.IsCompleted == false)
                return;

            bool isEnableCam = true;
            switch (CurCameraType)
            {
                case EnumProberCam.WAFER_LOW_CAM:
                case EnumProberCam.PIN_LOW_CAM:
                case EnumProberCam.WAFER_HIGH_CAM:
                case EnumProberCam.PIN_HIGH_CAM:
                case EnumProberCam.MAP_REF_CAM:
                    isEnableCam = true;
                    break;
                default:
                    isEnableCam = false;
                    break;
            }

            if (isEnableCam == false)
                return;

            EnumJogDirection indexJogDirection = EnumJogDirection.Center;

            if (sender == IndexMoveUp)
            {
                //==> U
                indexJogDirection = EnumJogDirection.Up;
            }
            else if (sender == IndexMoveRightUpBtn)
            {
                //==> RU
                indexJogDirection = EnumJogDirection.RightUp;
            }
            else if (sender == IndexMoveRight)
            {
                //==> R
                indexJogDirection = EnumJogDirection.Right;
            }
            else if (sender == IndexMoveRightDown)
            {
                //==> RD
                indexJogDirection = EnumJogDirection.RightDown;
            }
            else if (sender == IndexMoveDown)
            {
                //==> D
                indexJogDirection = EnumJogDirection.Down;
            }
            else if (sender == IndexMoveLeftDown)
            {
                //==> LD
                indexJogDirection = EnumJogDirection.LeftDown;
            }
            else if (sender == IndexMoveLeft)
            {
                //==> L
                indexJogDirection = EnumJogDirection.Left;
            }
            else if (sender == IndexMoveLeftUpBtn)
            {
                //==> LU
                indexJogDirection = EnumJogDirection.LeftUp;
            }

            StartContinuousIndexMoveEvent(new JogParam(CurCameraType, indexJogDirection), SetMoveZOffsetEnable);

        }
        private void StartContinuousIndexMoveEvent(JogParam jogParam, bool setzoffsetenable)
        {
            if (_HexagonJogViewModel == null)
                _HexagonJogViewModel = (IHexagonJogViewModel)this.DataContext;
            //_HexagonJogViewModel = (HexagonJogViewModel)this.DataContext;

            if (_ContinuousIndexMoveEventTask?.IsCompleted == false)
                return;

            _ContinuousIndexMoveEventTask = Task.Run(() =>
            {
                _HexagonJogViewModel.StickIndexMove(jogParam, setzoffsetenable);

                //==> Step Move
                System.Threading.Thread.Sleep(100);//==> Continue move 인식하는 민감도
                                                   //==> Continue Move
                bool pressed = false;
                do
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        pressed =
                            Mouse.LeftButton == MouseButtonState.Pressed;
                    }));

                    if (pressed == false)
                        break;

                    _HexagonJogViewModel.StickIndexMove(jogParam, setzoffsetenable);

                    System.Threading.Thread.Sleep(70);

                } while (true);

            });
        }
        #endregion

        #region ==> Step Move
        //==> X,Y Axis
        private void StepMoveBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_HexagonJogViewModel == null)
            {
                _HexagonJogViewModel = (IHexagonJogViewModel)this.DataContext;
            }

            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (_ContinuousStepMoveEventTask?.IsCompleted == false)
            {
                return;
            }

            double distance = StepMoveDistanceSwitch.IsChecked == true ? 10 : 1;

            EnumJogDirection touchDirection = EnumJogDirection.Center;

            if (sender == StepBtnUp)
            {
                //==> U
                touchDirection = EnumJogDirection.Up;
            }
            else if (sender == StepBtnRight)
            {
                //==> R
                touchDirection = EnumJogDirection.Right;
            }
            else if (sender == StepBtnDown)
            {
                //==> D
                touchDirection = EnumJogDirection.Down;
            }
            else if (sender == StepBtnLeft)
            {
                //==> L
                touchDirection = EnumJogDirection.Left;
            }
            else if(sender == StepBtnRightUp)
            {
                touchDirection = EnumJogDirection.RightUp;
            }
            else if (sender == StepBtnRightDown)
            {
                touchDirection = EnumJogDirection.RightDown;
            }
            else if (sender == StepBtnLeftDown)
            {
                touchDirection = EnumJogDirection.LeftDown;
            }
            else if (sender == StepBtnLeftUp)
            {
                touchDirection = EnumJogDirection.LeftUp;
            }
            else if (sender == StepBtnZBigUp)
            {
                //==> RU
                touchDirection = EnumJogDirection.ZBigUp;
                distance = StepMoveDistanceSwitch.IsChecked == true ? 100 : 10;
            }
            else if (sender == StepBtnZBigDown)
            {
                //==> RD
                touchDirection = EnumJogDirection.ZBigDown;
                distance = StepMoveDistanceSwitch.IsChecked == true ? 100 : 10;
            }
            else if (sender == StepBtnZSmallDown)
            {
                //==> LD
                touchDirection = EnumJogDirection.ZSmallDown;
                //distance = 1;
            }
            else if (sender == StepBtnZSmallUp)
            {
                //==> LU
                touchDirection = EnumJogDirection.ZSmallUp;
                //distance = 1;
            }

            StartContinuousStepMoveEvent(new JogParam(CurCameraType, touchDirection, distance));//==> 살짝 눌렀을 때는 Step Move Event가 한번만 발생 한다.
        }
        private void StartContinuousStepMoveEvent(JogParam jogParam)
        {
            if (_ContinuousStepMoveEventTask?.IsCompleted == false)
                return;

            _ContinuousStepMoveEventTask = Task.Run(() =>
            {
                if (_HexagonJogViewModel == null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        _HexagonJogViewModel = (IHexagonJogViewModel)this.DataContext;
                    }));
                }

                if (_HexagonJogViewModel != null)
                {
                    _HexagonJogViewModel.StickStepMove(jogParam);

                    System.Threading.Thread.Sleep(100);//==> Continue move 인식하는 민감도
                    bool pressed = false;
                    do
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            pressed =
                                Mouse.LeftButton == MouseButtonState.Pressed;
                        }));

                        if (pressed == false)
                            break;

                        _HexagonJogViewModel.StickStepMove(jogParam);

                        System.Threading.Thread.Sleep(70);

                    } while (true);
                }
            });
        }
        #endregion

        private void MainCanvas_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            EnumJogDirection indexJogDirection = EnumJogDirection.Center;
            JogParam jogParam = new JogParam(CurCameraType, indexJogDirection);
            switch (e.Key)
            {
                case Key.Up:
                    jogParam.Direction = EnumJogDirection.Up;
                    _HexagonJogViewModel.StickIndexMove(jogParam, SetMoveZOffsetEnable);
                    break;
                case Key.Down:
                    jogParam.Direction = EnumJogDirection.Down;
                    _HexagonJogViewModel.StickIndexMove(jogParam, SetMoveZOffsetEnable);
                    break;
                case Key.Right:
                    jogParam.Direction = EnumJogDirection.Right;
                    _HexagonJogViewModel.StickIndexMove(jogParam, SetMoveZOffsetEnable);
                    break;
                case Key.Left:
                    jogParam.Direction = EnumJogDirection.Left;
                    _HexagonJogViewModel.StickIndexMove(jogParam, SetMoveZOffsetEnable);
                    break;
                default:
                    break;
            }
        }
        MahApps.Metro.Controls.MetroWindow CustomMoveView = null;

        private void XYZCustom_Click(object sender, RoutedEventArgs e)
        {
            if (CustomMoveView != null && CustomMoveView.Visibility == Visibility.Visible)
            {
                CustomMoveView.Activate();
                return;
            }

            var custom_move = CustomMove.GetInstance();
            CustomMoveView = custom_move;
            CustomMoveView.WindowStyle = WindowStyle.ToolWindow;
            CustomMoveView.IgnoreTaskbarOnMaximize = false;
            CustomMoveView.Width = 350;
            CustomMoveView.Height = 300;
            CustomMoveView.Visibility = Visibility.Visible;
            CustomMoveView.Show();
        }

    }
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }
    public class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
}
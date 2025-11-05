using LogModule;
using ProberInterfaces;
using ProberInterfaces.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChuckTiltHexagonJog
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcChuckTiltHexagonJog : UserControl
    {
        #region ==> DEP AssignedCamera
        public static readonly DependencyProperty AssignedCameraProperty =
            DependencyProperty.Register("AssignedCamera"
                , typeof(ICamera),
                typeof(UcChuckTiltHexagonJog),
                new FrameworkPropertyMetadata(null));
        public ICamera AssignedCamera
        {
            get { return (ICamera)this.GetValue(AssignedCameraProperty); }
            set { this.SetValue(AssignedCameraProperty, value); }
        }
        #endregion

        private EnumProberCam CurCameraType
        {
            get
            {
                if (AssignedCamera == null)
                    return EnumProberCam.UNDEFINED;
                return AssignedCamera.CameraChannel.Type;
            }
        }

        private ChuckTiltHexagonJogViewModel _HexagonJogViewModel;
        private const int _ZAxisMargin = 10;//==> Stick Jog를 Z Axis Enable Mode 변화 판단 크기
        private Point _StickJogCenterPos;//==> Stick Jog 활동 영역(StickJogArea)에서 중앙 위치
        private Point _StickJogPos;//==> Stick Jog 현재 위치
        private EnumJogDirection _StickJogDirection;//==> Stick Jog 현재 방향
        private readonly int _StickJogAreaLayerCnt;//==> StickJogArea에 존재하는 Layer 수
        private readonly int _StickJogAreaLayerSepa;//==> StickJogArea의 Layer간의 간격

        //==> 사용자의 Stick Jog의 당김 정도 관련
        private double _StickJogDistance;//==> Stick Jog에서 부터 Stick Jog Center 까지의 거리, 범위[0 ~ StickJogArea 원의 반지름(100)]
        private int StickJogTuningDistance//==> Stick Jog Distance를 정수형으로 변환한 거리
        {
            get
            {
                //==> Stick Jog Area 밖 영역 이상으로는 Distance 값이 더이상 증가하지 않도록 함, 
                double retDistance = _StickJogDistance < (StickJogArea.Width / 2) ?
                    _StickJogDistance : _StickJogAreaLayerSepa * _StickJogAreaLayerCnt - 1;//==>-1은 StickJogArea 외각의 선을 제외하기 위해
                retDistance /= _StickJogAreaLayerSepa;//==> [0 ~ StickJogArea 원의 반지름 -1(100)] --> [0 ~ 2] 숫자 범위 변경
                return (int)retDistance;
            }
        }

        private bool _IsStickJogHold;//==> Stick Jog를 누르고 있는 상태 여부(Scan Move에서 사용)
        private bool _IsContinuousStepMoveEventEnable;//==> 지속적인 Step Move Event 동작 여부 (Continuous Step Move에서 사용)
        private bool _IsContinuousIndexMoveEventEnable;//==> 지속적인 Index Move Event 동작 여부 (Continuous Step Move에서 사용)
        private bool _IsScanMoveInOperation;//==> Scan Move 가 동작 상태 여부(true : Scan Move 가 계속 진행되고 있음, false : Scan Move가 동작하고 있지 않음)
        private bool _ZAxisEnable;//==> Z 축 동작 여부

        //==> Brush
        private LinearGradientBrush _StickJogBlackBrush;
        private LinearGradientBrush _StickJogRedBrush;

        private List<UserControl> _IndexMoveBtnList = null;//==> 8 Index Move Btn List
        private Task _ContinuousStepMoveEventTask;
        private Task _ContinuousIndexMoveEventTask;


        /*
         * StickJogArea Layer
         * 1) 제일 내부의 Layer에서의 Tuning 값은 0이다.
         * 2) Layer가 StickJog의 지름보다 작을 때는 Main Canvas에 표시되지 않는다.
         */
        private List<Ellipse> _StickJogAreaLayers;//==> StickJogArea를 일정 간격(_StickJogAreaLayerSepa)으로 나누기 위한 Layer들

        public UcChuckTiltHexagonJog()
        {
            try
            {
                InitializeComponent();

                //==> StickJogArea Layer 생성 및 초기화
                _StickJogAreaLayers = new List<Ellipse>();
                _StickJogAreaLayerCnt = 3;
                _StickJogAreaLayerSepa = (int)(StickJogArea.Width / 2) / _StickJogAreaLayerCnt;//==> StickJogArea 반지름 / Layer Count
                for (int i = 0; i < _StickJogAreaLayerCnt; i++)
                {
                    double elipDiameter = _StickJogAreaLayerSepa * 2;
                    if ((i + 1) * elipDiameter <= StickJog.Width)
                        continue;

                    Ellipse layer = new Ellipse();
                    layer.Visibility = Visibility.Hidden;
                    layer.Stroke = new SolidColorBrush(Colors.Gray);
                    layer.StrokeThickness = 1;
                    //layer.StrokeDashArray = new DoubleCollection() { 2, 2 };//==> elip의 선 Style은 Dash
                    layer.Width = layer.Height = (i + 1) * elipDiameter;

                    //==> elip이 Canvas의 중앙에 위치하도록 조정
                    Canvas.SetLeft(layer, (MainCanvas.Width - layer.Width) / 2);
                    Canvas.SetTop(layer, (MainCanvas.Height - layer.Height) / 2);

                    MainCanvas.Children.Insert(0, layer);
                    _StickJogAreaLayers.Add(layer);
                }

                //==> Brush 초기화
                _StickJogBlackBrush = new LinearGradientBrush();
                _StickJogRedBrush = new LinearGradientBrush();

                GradientStop blackGS = new GradientStop();
                blackGS.Color = Colors.Black;
                blackGS.Offset = 1;

                GradientStop redGS = new GradientStop();
                redGS.Color = Colors.Red;
                redGS.Offset = 1;

                GradientStop whiteGS = new GradientStop();
                whiteGS.Color = Colors.White;
                whiteGS.Offset = 0;

                _StickJogBlackBrush.GradientStops.Add(blackGS);
                _StickJogBlackBrush.GradientStops.Add(whiteGS);

                _StickJogRedBrush.GradientStops.Add(redGS);
                _StickJogRedBrush.GradientStops.Add(whiteGS);

                _PrevStickJogDirection = EnumJogDirection.Center;
                _PrevStickJogTuningDistance = 0;

                StickJogEllip.Fill = _StickJogBlackBrush;

                //==> 8가지 Index Move 버튼들을 리스트로 관리하기 위해...
                //_IndexMoveBtnList = new List<UserControl>();
                //_IndexMoveBtnList.Add(IndexMoveUp);
                //_IndexMoveBtnList.Add(IndexMoveRightUpBtn);
                //_IndexMoveBtnList.Add(IndexMoveRight);
                //_IndexMoveBtnList.Add(IndexMoveRightDown);
                //_IndexMoveBtnList.Add(IndexMoveDown);
                //_IndexMoveBtnList.Add(IndexMoveLeftDown);
                //_IndexMoveBtnList.Add(IndexMoveLeft);
                //_IndexMoveBtnList.Add(IndexMoveLeftUpBtn);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //==> StickJog가 가운데에 있을 수 있도록 위치 시킴
                Canvas.SetLeft(StickJog, (MainCanvas.ActualWidth - StickJog.ActualWidth) / 2);
                Canvas.SetTop(StickJog, (MainCanvas.ActualHeight - StickJog.ActualHeight) / 2);

                //==> StickJogArea가 가운데에 있을 수 있도록 위치 시킴
                Canvas.SetLeft(StickJogArea, (MainCanvas.ActualWidth - StickJogArea.ActualWidth) / 2);
                Canvas.SetTop(StickJogArea, (MainCanvas.ActualHeight - StickJogArea.ActualHeight) / 2);

                _StickJogCenterPos = new Point(Canvas.GetLeft(StickJog), Canvas.GetTop(StickJog));
                _StickJogPos = _StickJogCenterPos;

                _StickJogDirection = EnumJogDirection.Center;
                _StickJogDistance = 0;

                _HexagonJogViewModel = (ChuckTiltHexagonJogViewModel)this.DataContext;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #region ==> Index Move
        private void BtnTrapezoidArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ResetStickJog();

                //==> _Continuous Step Move Event가 종료중일때 다른 Event가 개입되지 못하도록 막는 역할
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

                if (_IsScanMoveInOperation)
                    return;

                if (sender is IInputElement)
                    Mouse.Capture((IInputElement)sender, CaptureMode.SubTree);

                _IsContinuousIndexMoveEventEnable = true;

                //if (sender == IndexMoveUp)
                //    StartContinuousIndexMoveEvent(new JogParam(CurCameraType, EnumJogDirection.Up));
                //else if (sender == IndexMoveRightUpBtn)
                //    StartContinuousIndexMoveEvent(new JogParam(CurCameraType, EnumJogDirection.RightUp));
                //else if (sender == IndexMoveRight)
                //    StartContinuousIndexMoveEvent(new JogParam(CurCameraType, EnumJogDirection.Right));
                //else if (sender == IndexMoveRightDown)
                //    StartContinuousIndexMoveEvent(new JogParam(CurCameraType, EnumJogDirection.RightDown));
                //else if (sender == IndexMoveDown)
                //    StartContinuousIndexMoveEvent(new JogParam(CurCameraType, EnumJogDirection.Down));
                //else if (sender == IndexMoveLeftDown)
                //    StartContinuousIndexMoveEvent(new JogParam(CurCameraType, EnumJogDirection.LeftDown));
                //else if (sender == IndexMoveLeft)
                //    StartContinuousIndexMoveEvent(new JogParam(CurCameraType, EnumJogDirection.Left));
                //else if (sender == IndexMoveLeftUpBtn)
                //    StartContinuousIndexMoveEvent(new JogParam(CurCameraType, EnumJogDirection.LeftUp));
                //else
                //    _IsContinuousIndexMoveEventEnable = false;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void BtnTrapezoidArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Mouse.Captured == null)
                    return;

                Mouse.Capture(null);

                ResetStickJog();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void StartContinuousIndexMoveEvent(JogParam jogParam)
        {
            try
            {
                if (_ContinuousIndexMoveEventTask?.IsCompleted == false)
                    return;

                _ContinuousIndexMoveEventTask = Task.Run(() =>
                {
                    do
                    {
                        _HexagonJogViewModel.StickJogIndexMoveFunc(jogParam);
                        System.Threading.Thread.Sleep(100);
                    } while (_IsContinuousIndexMoveEventEnable);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void ExitContinuousIndexMoveEvent()
        {
            try
            {
                if (_ContinuousIndexMoveEventTask == null)
                    return;

                ResetStickJog();

                //==> _Continuous Step Move Event가 종료중일때 다른 Event가 개입되지 못하도록 막는 역할
                _ContinuousIndexMoveEventTask.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> Step Move & Scan Move
        private void StickJogArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ResetStickJog();

                //==> _Continuous Step Move Event가 종료중일때 다른 Mouse Down Event가 개입되지 못하도록 막는 역할
                if (_ContinuousStepMoveEventTask?.IsCompleted == false)
                    return;

                Mouse.Capture(StickJogArea);//==> Drag 한 상태에서 StickJogArea 영역 밖에 나가도 추적하여 MouseUp Event를 받을 수 있도록

                Point pos = e.GetPosition(MainCanvas);
                UpdateStickJogPos(pos);

                if (_IsScanMoveInOperation)
                    return;

                Point curStickJogPos = new Point(Canvas.GetLeft(StickJog), Canvas.GetTop(StickJog));
                double stickJogDistance = GetDistance(_StickJogCenterPos, curStickJogPos);


                if (stickJogDistance < (StickJogEllip.Width / 2))//==> Stick Jog를 눌름
                {
                    _IsStickJogHold = true;
                    foreach (Ellipse layer in _StickJogAreaLayers)
                        layer.Visibility = Visibility.Visible;
                }
                else//==> Stick Jog가 아닌 Stick Jog Area를 눌름
                {
                    _IsContinuousStepMoveEventEnable = true;
                    StartContinuousStepMoveEvent();//==> 살짝 눌렀을 때는 Step Move Event가 한번만 발생 한다.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private EnumJogDirection _PrevStickJogDirection;
        private int _PrevStickJogTuningDistance;
        private void StickJogArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (_IsStickJogHold == false)
                return;

            try
            {
                Point pos = e.GetPosition(MainCanvas);
                UpdateStickJogPos(pos);

                //==> Stick Jog 위치가 Stick jog 반지름 이상으로 움직였을 때만 Move Event로 인식 한다.
                if (_StickJogDistance < (StickJog.Width / 2))
                    return;

                //==> Scan Move
                if (_PrevStickJogDirection == _StickJogDirection && //==> Stick Jog의 방향 변화 감지
                    _PrevStickJogTuningDistance == StickJogTuningDistance)//==> Stick Jog의 거리 변화 감지
                {
                    //==> 이전 jog의 방향이 같다.
                }
                else
                {
                    _HexagonJogViewModel.StickJogScanMoveChangeFunc(new JogParam(
                        CurCameraType,
                        _StickJogDirection,
                        StickJogTuningDistance));
                    _IsScanMoveInOperation = true;
                }

                _PrevStickJogDirection = _StickJogDirection;
                _PrevStickJogTuningDistance = StickJogTuningDistance;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


        }
        private void StickJogArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured == null)
                return;

            try
            {
                Mouse.Capture(null);

                Point curStickJogPos = new Point(Canvas.GetLeft(StickJog), Canvas.GetTop(StickJog));

                if (Math.Abs(_StickJogCenterPos.X - curStickJogPos.X) < _ZAxisMargin &&
                    Math.Abs(_StickJogCenterPos.Y - curStickJogPos.Y) < _ZAxisMargin)
                //==> Z Axis 변경 조건 판단
                {
                    if (ZLabel.Visibility == Visibility.Hidden)
                    {
                        //==> Z Axis 활성화
                        ChangeStickJogUI(
                            zAxisVisi: Visibility.Visible,
                            stickJogColor: _StickJogRedBrush,
                            stickJogAreaBrush: new SolidColorBrush(Colors.Red),
                            indexMoveBtnOpacity: 0.25,
                            indexMoveBtnEnable: false);
                        _ZAxisEnable = true;
                    }
                    else
                    {
                        //==> Z Axis 비 활성화
                        ChangeStickJogUI(
                            zAxisVisi: Visibility.Hidden,
                            stickJogColor: _StickJogBlackBrush,
                            stickJogAreaBrush: new SolidColorBrush(Colors.White),
                            indexMoveBtnOpacity: 1,
                            indexMoveBtnEnable: true);
                        _ZAxisEnable = false;
                    }
                }
                ResetStickJog();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


        }
        //==> Stick Jog UI를 변경
        private void ChangeStickJogUI(Visibility zAxisVisi, Brush stickJogColor, Brush stickJogAreaBrush, double indexMoveBtnOpacity, bool indexMoveBtnEnable)
        {
            try
            {
                ZLabel.Visibility = zAxisVisi;
                ZAxisSepLineHor.Visibility = zAxisVisi;
                ZAxisSepLineVer.Visibility = zAxisVisi;

                ZAxisSmallPlusLabel.Visibility = zAxisVisi;
                ZAxisSmallMinusLabel.Visibility = zAxisVisi;
                ZAxisBigPlusLabel.Visibility = zAxisVisi;
                ZAxisBigMinusLabel.Visibility = zAxisVisi;

                StickJogEllip.Fill = stickJogColor;
                StickJogArea.Fill = stickJogAreaBrush;

                foreach (var uc in _IndexMoveBtnList)
                {
                    uc.Opacity = indexMoveBtnOpacity;
                    uc.IsEnabled = indexMoveBtnEnable;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void StartContinuousStepMoveEvent()
        {
            try
            {
                //if (_StickJogDirection == EnumJogDirection.Zup || _StickJogDirection == EnumJogDirection.Zdown)
                //{
                //    StickJogStepMoveControl.Command.Execute(new JogParam(CurCameraType, _StickJogDirection, StickJogTuningDistance));
                //    return;
                //}
                if (_ContinuousStepMoveEventTask?.IsCompleted == false)
                    return;

                EnumProberCam curCameraType = CurCameraType;
                EnumJogDirection stickJogDirection = _StickJogDirection;
                int stickJogTuningDistance = StickJogTuningDistance;
                _ContinuousStepMoveEventTask = Task.Run(() =>
                {
                    bool pressed = false;
                    do
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            pressed = Mouse.LeftButton == MouseButtonState.Pressed;
                        }));

                        System.Threading.Thread.Sleep(10);

                        if (pressed == false)
                            break;

                        _HexagonJogViewModel.StickJogStepMoveFunc(new JogParam(curCameraType, stickJogDirection, stickJogTuningDistance));
                    } while (_IsContinuousStepMoveEventEnable);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        //==> Stick Jog의 상태를 위치를 초기 상태로 변경
        private void ResetStickJog()
        {
            try
            {
                _IsStickJogHold = false;
                _IsContinuousStepMoveEventEnable = false;
                _IsContinuousIndexMoveEventEnable = false;

                if (_IsScanMoveInOperation)
                {
                    _HexagonJogViewModel.StickJogScanMoveEndFunc(new JogParam(CurCameraType, _StickJogDirection, StickJogTuningDistance));
                    _IsScanMoveInOperation = false;
                    _PrevStickJogDirection = EnumJogDirection.Center;
                    _PrevStickJogTuningDistance = 0;
                }

                foreach (Ellipse layer in _StickJogAreaLayers)
                    layer.Visibility = Visibility.Hidden;

                Canvas.SetLeft(StickJog, _StickJogCenterPos.X);
                Canvas.SetTop(StickJog, _StickJogCenterPos.Y);
                _StickJogPos = _StickJogCenterPos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> MATH ALGORITHM
        //==> Stick Jog 좌표, 방향 Update
        private void UpdateStickJogPos(Point pos)
        {
            try
            {
                Point stickJogPos = new Point(
                    pos.X - (StickJog.Width / 2),
                    pos.Y - (StickJog.Height / 2));

                double stickJogDistance = GetDistance(_StickJogCenterPos, stickJogPos);
                if (stickJogDistance < StickJogArea.Width / 2)
                {
                    Canvas.SetLeft(StickJog, stickJogPos.X);
                    Canvas.SetTop(StickJog, stickJogPos.Y);
                    _StickJogPos = stickJogPos;
                }

                _StickJogDistance = stickJogDistance;

                double degree = GetDegree();

                if (_ZAxisEnable)
                {
                    //==> RU
                    if (-90 < degree && degree <= 0)
                        _StickJogDirection = EnumJogDirection.ZBigUp;
                    //==> RD
                    else if (0 < degree && degree <= 90)
                        _StickJogDirection = EnumJogDirection.ZBigDown;
                    //==> LD
                    else if (90 < degree && degree <= 180)
                        _StickJogDirection = EnumJogDirection.ZSmallDown;
                    //==> LU
                    else if (-180 < degree && degree <= -90)
                        _StickJogDirection = EnumJogDirection.ZSmallUp;
                }
                else
                {
                    //==> U
                    if (-112.5 < degree && degree <= -67.5)
                        _StickJogDirection = EnumJogDirection.Up;
                    //==> RU
                    else if (-67.5 < degree && degree <= -22.5)
                        _StickJogDirection = EnumJogDirection.RightUp;
                    //==> R
                    else if (-22.5 < degree && degree <= 22.5)
                        _StickJogDirection = EnumJogDirection.Right;
                    //==> RD
                    else if (22.5 < degree && degree <= 67.5)
                        _StickJogDirection = EnumJogDirection.RightDown;
                    //==> D
                    else if (67.5 < degree && degree <= 112.5)
                        _StickJogDirection = EnumJogDirection.Down;
                    //==> LD
                    else if (112.5 < degree && degree <= 157.5)
                        _StickJogDirection = EnumJogDirection.LeftDown;
                    //==> L
                    else if (157.5 < degree && degree <= 180 || -180 <= degree && degree <= -157.5)
                        _StickJogDirection = EnumJogDirection.Left;
                    //==> LU
                    else if (-157.5 < degree && degree <= -112.5)
                        _StickJogDirection = EnumJogDirection.LeftUp;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        //==> 피타고라스 방정식 이용(a^2 + B^2 = c^2)하여 거리 계산
        private double GetDistance(Point pt1, Point pt2)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2));
        }
        //==> Stick Jog와 Stick jog Center를 이용 각도 계산
        /* ==> Degree         
         *           +90   
         *      +135     +45
         * +180                0  
         * -180                0 
         *      -135     -45
         *           -90
         *           
         * ==> Direction          
         *           U   
         *      LU       RU
         * +L                  R  
         *      LD       RD
         *           D   
         */
        public double GetDegree()
        {
            double updateDegree = 0;

            try
            {
                updateDegree = Math.Atan2(
                    _StickJogPos.Y - _StickJogCenterPos.Y,
                    _StickJogPos.X - _StickJogCenterPos.X) * 180 / Math.PI;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return updateDegree;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ucDutViewer
{
    using DXControlBase;
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.Mathematics.Interop;
    using System.ComponentModel;
    using System.Windows;
    using ProberInterfaces;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using System.Windows.Input;
    using ProberInterfaces.Param;
    using RelayCommandBase;
    using System.Diagnostics;
    using LogModule;

    public class DutViewControl : D2dControl, IInputElement, IDutViewControlVM
    {
        public static readonly DependencyProperty StageSupervisorProperty =
            DependencyProperty.Register(nameof(StageSupervisor), typeof(IStageSupervisor), typeof(DutViewControl), new FrameworkPropertyMetadata(null));

        public IStageSupervisor StageSupervisor
        {
            get { return (IStageSupervisor)this.GetValue(StageSupervisorProperty); }
            set { this.SetValue(StageSupervisorProperty, value); }
        }

        public static readonly DependencyProperty MotionManagerProperty =
            DependencyProperty.Register(nameof(MotionManager), typeof(IMotionManager), typeof(DutViewControl), new FrameworkPropertyMetadata(null));

        public IMotionManager MotionManager
        {
            get { return (IMotionManager)this.GetValue(MotionManagerProperty); }
            set { this.SetValue(MotionManagerProperty, value); }
        }

        public static readonly DependencyProperty ProbeCardProperty =
            DependencyProperty.Register(nameof(ProbeCard), typeof(IProbeCard), typeof(DutViewControl), new FrameworkPropertyMetadata(null));
        public IProbeCard ProbeCard
        {
            get { return (IProbeCard)this.GetValue(ProbeCardProperty); }
            set { this.SetValue(ProbeCardProperty, value); }
        }

        public static readonly DependencyProperty WaferObjectProperty =
            DependencyProperty.Register(nameof(WaferObject), typeof(IWaferObject), typeof(DutViewControl), new FrameworkPropertyMetadata(null));
        public IWaferObject WaferObject
        {
            get { return (IWaferObject)this.GetValue(WaferObjectProperty); }
            set { this.SetValue(WaferObjectProperty, value); }
        }

        public static readonly DependencyProperty VisionManagerProperty =
            DependencyProperty.Register(nameof(VisionManager), typeof(IVisionManager), typeof(DutViewControl), new FrameworkPropertyMetadata(null));
        public IVisionManager VisionManager
        {
            get { return (IVisionManager)this.GetValue(VisionManagerProperty); }
            set { this.SetValue(VisionManagerProperty, value); }
        }

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(DutViewControl), new FrameworkPropertyMetadata((double)0));
        public double ZoomLevel
        {
            get { return (double)this.GetValue(ZoomLevelProperty); }
            set { this.SetValue(ZoomLevelProperty, value); }
        }

        public static readonly DependencyProperty AddCheckBoxIsCheckedProperty =
            DependencyProperty.Register(nameof(AddCheckBoxIsChecked), typeof(bool?), typeof(DutViewControl), new FrameworkPropertyMetadata(false));
        public bool? AddCheckBoxIsChecked
        {
            get { return (bool?)this.GetValue(AddCheckBoxIsCheckedProperty); }
            set { this.SetValue(AddCheckBoxIsCheckedProperty, value); }
        }

        // 더트 맵 외부에 바둑판을 표시할 것인가 말 것인가 
        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register(nameof(ShowGrid), typeof(bool?), typeof(DutViewControl), new FrameworkPropertyMetadata(true));
        public bool? ShowGrid
        {
            get { return (bool?)this.GetValue(ShowGridProperty); }
            set { this.SetValue(ShowGridProperty, value); }
        }

        // 핀 위치 표시 On/Off
        public static readonly DependencyProperty ShowPinProperty =
            DependencyProperty.Register(nameof(ShowPin), typeof(bool?), typeof(DutViewControl), new FrameworkPropertyMetadata(false));
        public bool? ShowPin
        {
            get { return (bool?)this.GetValue(ShowPinProperty); }
            set { this.SetValue(ShowPinProperty, value); }
        }

        // 패드 위치 표시 On/Off
        public static readonly DependencyProperty ShowPadProperty =
            DependencyProperty.Register(nameof(ShowPad), typeof(bool?), typeof(DutViewControl), new FrameworkPropertyMetadata(false));
        public bool? ShowPad
        {
            get { return (bool?)this.GetValue(ShowPadProperty); }
            set { this.SetValue(ShowPadProperty, value); }
        }

        // 마우스로 더트맵을 클릭했을 때, 클릭한 곳의 선택된 더트를 붉은색 테두리로 표시
        public static readonly DependencyProperty ShowSelectedDutProperty =
            DependencyProperty.Register(nameof(ShowSelectedDut), typeof(bool?), typeof(DutViewControl), new FrameworkPropertyMetadata(true));
        public bool? ShowSelectedDut
        {
            get { return (bool?)this.GetValue(ShowSelectedDutProperty); }
            set { this.SetValue(ShowSelectedDutProperty, value); }
        }

        // 현재의 위치를 십자선으로 표시. 맵을 드래그할 수 있는 옵션이 켜져 있지 않다면 이 옵션이 켜져 있을 때 맵을 터치하면 해당 위치로 움직인다.
        // 더트 외부로는 터치해서 움직일 수 없다.
        public static readonly DependencyProperty ShowCurrentPosProperty =
            DependencyProperty.Register(nameof(ShowCurrentPos), typeof(bool?), typeof(DutViewControl), new FrameworkPropertyMetadata(false));
        public bool? ShowCurrentPos
        {
            get { return (bool?)this.GetValue(ShowCurrentPosProperty); }
            set { this.SetValue(ShowCurrentPosProperty, value); }
        }

        // 현재의 위치를 기준으로, Camera의 ROI 영역을 표시. 
        public static readonly DependencyProperty ShowCurrentPosROIProperty =
            DependencyProperty.Register(nameof(ShowCurrentPosROI), typeof(bool?), typeof(DutViewControl), new FrameworkPropertyMetadata(false));
        public bool? ShowCurrentPosROI
        {
            get { return (bool?)this.GetValue(ShowCurrentPosROIProperty); }
            set { this.SetValue(ShowCurrentPosROIProperty, value); }
        }

        // 맵을 드래그하여 보이는 위치를 시프트 가능. 이 옵션이 켜져 있으면 마우스로 맵을 터치해도 움직이지 않는다.
        public static readonly DependencyProperty EnableDragMapProperty =
            DependencyProperty.Register(nameof(EnableDragMap), typeof(bool?), typeof(DutViewControl), new FrameworkPropertyMetadata(false));
        public bool? EnableDragMap
        {
            get { return (bool?)this.GetValue(EnableDragMapProperty); }
            set { this.SetValue(EnableDragMapProperty, value); }
        }

        // 현재 위치를 표시하기 위한 카메라 설정. 맵을 드래그할 수 있는 옵션이 켜져 있지 않다면 이 옵션이 켜져 있을 때 맵을 터치하면 해당 위치로 움직인다.
        // 더트 외부로는 터치해서 움직일 수 없다.
        public static readonly DependencyProperty CamTypeProperty =
            DependencyProperty.Register(nameof(CamType), typeof(EnumProberCam), typeof(DutViewControl), new FrameworkPropertyMetadata(EnumProberCam.UNDEFINED));
        public EnumProberCam CamType
        {
            get { return (EnumProberCam)this.GetValue(CamTypeProperty); }
            set { this.SetValue(CamTypeProperty, value); }
        }

        public static readonly DependencyProperty CurCamProperty =
            DependencyProperty.Register(nameof(CurCam), typeof(ICamera), typeof(DutViewControl), new FrameworkPropertyMetadata(null));
        public ICamera CurCam
        {
            get { return (ICamera)this.GetValue(CurCamProperty); }
            set { this.SetValue(CurCamProperty, value); }
        }

        public static readonly DependencyProperty IsEnableMovingProperty =
            DependencyProperty.Register(nameof(IsEnableMoving), typeof(bool), typeof(DutViewControl), new FrameworkPropertyMetadata(true));
        public bool IsEnableMoving
        {
            get { return (bool)this.GetValue(IsEnableMovingProperty); }
            set { this.SetValue(IsEnableMovingProperty, value); }
        }

        public static readonly DependencyProperty CurXPosProperty =
            DependencyProperty.Register(nameof(CurXPos), typeof(double), typeof(DutViewControl), new FrameworkPropertyMetadata((double)0,
                new PropertyChangedCallback(CurPosXPropertyChanged)));
        public double CurXPos
        {
            get { return (double)this.GetValue(CurXPosProperty); }
            set { this.SetValue(CurXPosProperty, value); }
        }

        public static readonly DependencyProperty CurYPosProperty =
            DependencyProperty.Register(nameof(CurYPos), typeof(double), typeof(DutViewControl), new FrameworkPropertyMetadata((double)0,
                new PropertyChangedCallback(CurPosYPropertyChanged)));
        public double CurYPos
        {
            get { return (double)this.GetValue(CurYPosProperty); }
            set { this.SetValue(CurYPosProperty, value); }
        }

        public System.Windows.Point MouseDownPos { get; set; }
        public bool IsMouseDown { get; set; }
        public float DutSizeX { get; set; }
        public float DutSizeY { get; set; }

        Stopwatch stw = new Stopwatch();
        //DateTime lastRenderTime = new DateTime();
        //int frames;
        //int framesPerSec;

        //private MachineIndex SelectDutIndex { get; set; }
        //private List<Dut> TempDutList { get; set; }
        public DutViewControl()
        {
            try
            {
                InitEvents();
                ZoomLevel = 4;

                //SelectDutIndex = new MachineIndex();
                //TempDutList = new List<Dut>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        // 그림에서 좌우 마진이 바뀐 경우 현재 선택된 더트의 위치를 함께 이동시켜 주기 위해서 이전 값을 저장해 둔다.
        private long PrevFirstCoordX = 0;
        private long PrevFirstCoordY = 0;
        public bool AutoAddChecked = false;

        private void SetCurPosX(double xpos)
        {
            var curpos = this.StageSupervisor.CoordinateManager().StageCoordConvertToUserCoord(CamType);
            CurXPos = curpos.X.Value;
            CurYPos = curpos.Y.Value;
        }
        private static void CurPosXPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                DutViewControl dv = null;
                if (sender is DutViewControl)
                {
                    dv = (DutViewControl)sender;
                }
                if (sender != null & e.NewValue != null && dv.ShowCurrentPos == true)
                {
                    dv.SetCurPosX((double)e.OldValue);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetCurPosY(double ypos)
        {
            var curpos = this.StageSupervisor.CoordinateManager().StageCoordConvertToUserCoord(CamType);
            CurXPos = curpos.X.Value;
            CurYPos = curpos.Y.Value;
        }
        private static void CurPosYPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                DutViewControl dv = null;
                if (sender is DutViewControl)
                {
                    dv = (DutViewControl)sender;
                }
                if (sender != null & e.NewValue != null && dv.ShowCurrentPos == true)
                {
                    dv.SetCurPosY((double)e.OldValue);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InitEvents()
        {
            try
            {
                this.MouseMove += MapViewControl_MouseMove;
                //this.MouseDown += MapViewControl_MouseDown;
                this.MouseLeave += MapViewControl_MouseLeave;
                this.MouseUp += MapViewControl_MouseUp;
                this.PreviewMouseDown += MapViewControl_MouseDown;
                this.Loaded += OnLoad;
                this.SizeChanged += OnSizeChanged;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void OnLoad(object sender, RoutedEventArgs e)
        {
            CmdMoveToCenter();
            EnableDragMap = true;
            InitColorPallette();
        }
        public void OnSizeChanged(object sender, RoutedEventArgs e)
        {
            CmdMoveToCenter();
        }

        private RelayCommand<object> _CmdZoomInButton;
        public ICommand CmdZoomInButton
        {
            get
            {
                if (null == _CmdZoomInButton) _CmdZoomInButton = new RelayCommand<object>(CmdZoomIn);
                return _CmdZoomInButton;
            }
        }

        private void CmdZoomIn(object noparam)
        {
            try
            {
                //double Size = 10000;
                //double ratioX = (ActualWidth / Size) * ZoomLevel;
                //double ratioY = (ActualHeight / Size) * ZoomLevel;
                //int interval = 100;
                //DutSizeX = (float)(interval * ratioX);
                //DutSizeY = (float)(interval * ratioY);

                //if (ActualWidth <= DutSizeX && ActualHeight <= DutSizeY) return;
                if (ZoomLevel < 300) ZoomLevel += 1;

                // 현재 보고 있는 곳을 기준으로 줌 인을 하기 위하여 줌 레이쇼가 바뀐 만큼 그리는 위치를 시프트해 준다.
                //float shiftX = 0;
                //shiftX = (float)((ActualWidth / 2) + viewCenPos.X);
                //viewCurrentCenPos.X += shiftX;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _CmdZoomOutButton;
        public ICommand CmdZoomOutButton
        {
            get
            {
                if (null == _CmdZoomOutButton) _CmdZoomOutButton = new RelayCommand<object>(CmdZoomOut);
                return _CmdZoomOutButton;
            }
        }
        private void CmdZoomOut(object noparam)
        {
            try
            {
                double Size = 10000;
                double ratioX = (ActualWidth / Size) * ZoomLevel;
                double ratioY = (ActualHeight / Size) * ZoomLevel;
                int interval = 100;

                DutSizeX = (float)(interval * ratioX);
                DutSizeY = (float)(interval * ratioY);

                //DutSizeX = (float)(StageSupervisor.WaferObject.GetSubsInfo().ActualDieSize.Width.Value * ratioX);
                //DutSizeY = (float)(StageSupervisor.WaferObject.GetSubsInfo().ActualDieSize.Height.Value * ratioY);

                //if (DutSizeX <= 20 || DutSizeY <= 20) return;
                if (DutSizeX <= 7 || DutSizeY <= 7) return;


                ZoomLevel -= 1;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _CmdMoveToCenterButton;
        public ICommand CmdMoveToCenterButton
        {
            get
            {
                if (null == _CmdMoveToCenterButton) _CmdMoveToCenterButton = new RelayCommand(CmdMoveToCenter);
                return _CmdMoveToCenterButton;
            }
        }

        public Guid GUID { get; set; }
        public int MaskingLevel { get; set; }
        public bool Lockable { get; set; }
        public bool InnerLockable { get; set; }
        public List<int> AvoidLockHashCodes { get; set; }

        public IAsyncCommand DutAddMouseDownCommand { get; set; }


        private void CmdMoveToCenter()
        {
            try
            {
                if (ActualWidth <= 0 || ActualHeight <= 0) return;
                if (!IsEnableMoving || ProbeCard == null) return;

                double Size = 10000;
                double StageWidth = Size; // 300000;         // 프로브 카드가 존재하는 최대 영역  (300mm)
                double StageHeight = Size; // 300000;
                double ratioX = (ActualWidth / StageWidth);     // 줌 레벨 1배의 경우
                double ratioY = (ActualHeight / StageHeight);
                int interval = 100;

                DutSizeX = (float)(interval * ratioX);
                DutSizeY = (float)(interval * ratioY);

                if (ShowCurrentPos == true)
                {
                    // 자동 줌인 한다.
                    if (DutSizeX < DutSizeY)
                        ZoomLevel = (double)Math.Truncate((ActualWidth - 120) / (ProbeCard.ProbeCardDevObjectRef.DutIndexSizeX * DutSizeX));
                    else
                        ZoomLevel = (double)Math.Truncate((ActualHeight - 120) / (ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY * DutSizeY));
                    if (ZoomLevel < 11)
                        ZoomLevel = 11;
                }
                else
                {
                    // 현재 위치 표시를 안하는 화면 = 더트 에디트. 줌을 너무 크게 하면 오히려 불편
                    ZoomLevel = 8;
                }

                viewCenPos.X = 0;
                viewCenPos.Y = 0;

                viewCurrentCenPos.X = 0;
                viewCurrentCenPos.Y = 0;
                // ZoomLevel = 3;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MapViewControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                IsMouseDown = false;
                viewCurrentCenPos.X = viewCenPos.X;
                viewCurrentCenPos.Y = viewCenPos.Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private double roundEx(double x)
        // 양수이면 반올림, 음수이면 반내림 한다
        {
            double retVal = 0;

            try
            {
                retVal = ((x > 0) ? Math.Truncate(x) : Math.Floor(x));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void MapViewControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!IsEnableMoving || ProbeCard == null) return;

                System.Windows.Point downPos = new System.Windows.Point();
                downPos = e.GetPosition((System.Windows.IInputElement)sender);
                MouseDownPos = downPos;

                double MarginX = 0;
                double MarginY = 0;
                double macIdxX = 0;
                double macIdxY = 0;

                MarginX = ProbeCard.ProbeCardDevObjectRef.DutIndexSizeX / 2.0 * -1.0;
                MarginY = ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY / 2.0 - 1.0;   // 더트 좌측 하단을 기준으로 카운트하므로 Y는 하나 빼준다


                double dutCenterMousePosX = (this.ActualWidth / 2.0);
                double dutCenterMousePosY = (this.ActualHeight / 2.0);

                double diffMousePtX = MouseDownPos.X - dutCenterMousePosX + (float)((MarginX - (int)MarginX)) * DutSizeX;
                double diffMousePtY = dutCenterMousePosY - MouseDownPos.Y - (float)((MarginY - (int)MarginY)) * DutSizeY;   // Y는 그리는 좌표계 방향이 반대

                double dutIntervalX = diffMousePtX / DutSizeX;
                double dutIntervalY = diffMousePtY / DutSizeY;

                float center_shiftX = (float)((MarginX - (int)MarginX)) * DutSizeX;     // 더트맵 사이즈가 홀수냐 짝수냐에 따라 중심점이 더트사이즈의 절반만큼 시프트되므로 이 값만큼 고려해 주어야 한다.
                float center_shiftY = (float)((MarginY - (int)MarginY)) * DutSizeY;

                //==> 나눗셈 영향에 의한 Dut계산 오차 조절
                if (dutIntervalX < 0)
                    dutIntervalX--;

                if (dutIntervalY > 0)
                    dutIntervalY++;

                // 그림의 정 가운데에 위치한 기준선이 (0, 0) 좌표 더트의 좌측 상단이 된다.
                // 그리는 좌표계는 우측이 +, 하단이 + 이다. 주의할 것.

                //if (AddCheckBoxIsChecked == true)
                //{
                //    if (ProbeCardData.DutList.FirstOrDefault(dut =>
                //    dut.MacIndex.XIndex == macIdxX && dut.MacIndex.YIndex == macIdxY) == null)
                //    {
                //        Dut tmpDut = new Dut();

                //        tmpDut.MacIndex.XIndex = (long)macIdxX;
                //        tmpDut.MacIndex.YIndex = (long)macIdxY;

                //        tmpDut.DutNumber = ProbeCardData.DutList.Count() + 1;

                //        ProbeCardData.DutList.Add(tmpDut);
                //    }
                //}

                int reverseXDir = 1;
                int reverseYDir = 1;

                if (StageSupervisor.CoordinateManager().GetReverseManualMoveX() == true)
                {
                    reverseXDir = -1;
                }

                if (StageSupervisor.CoordinateManager().GetReverseManualMoveY() == true)
                {
                    reverseYDir = -1;
                }

                if (ShowCurrentPos == true && EnableDragMap == false)
                {
                    // 찍은 위치가 더트맵 + 1 사이즈 이상인 경우 무시한다.
                    if (MouseDownPos.X - dutCenterMousePosX - viewCenPos.X < (ProbeCard.ProbeCardDevObjectRef.DutIndexSizeX + 1) * DutSizeX / 2.0 * -1)
                    {
                        return;
                    }
                    if (MouseDownPos.X - dutCenterMousePosX - viewCenPos.X > (ProbeCard.ProbeCardDevObjectRef.DutIndexSizeX + 1) * DutSizeX / 2.0)
                    {
                        return;
                    }

                    if (dutCenterMousePosY - MouseDownPos.Y + viewCenPos.Y < (ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY + 1) * DutSizeY / 2.0 * -1)
                    {
                        return;
                    }
                    if (dutCenterMousePosY - MouseDownPos.Y + viewCenPos.Y > (ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY + 1) * DutSizeY / 2.0)
                    {
                        return;
                    }

                    IViewModelManager tmpVMmanager = StageSupervisor.ViewModelManager();


                    // 더트맵이 카메라와 연동하여 현재 위치를 표시하는 경우 맵 드래그 모드가 아니면 클릭한 위치로 이동한다.
                    double posX = 0;
                    double posY = 0;
                    double uMperpixel_X = WaferObject.GetSubsInfo().ActualDieSize.Width.Value / DutSizeX;       // 픽셀 당 몇 마이크론 인가. 픽셀값에 곱하면 실제 거리가 나온다. 
                    double uMperpixel_Y = WaferObject.GetSubsInfo().ActualDieSize.Height.Value / DutSizeY;
                    

                    if (CamType == EnumProberCam.PIN_HIGH_CAM)
                    {
                        posX = ProbeCard.ProbeCardDevObjectRef.DutCenX - reverseXDir * (viewCenPos.X * uMperpixel_X) + (reverseXDir * (MouseDownPos.X - dutCenterMousePosX) * uMperpixel_X);
                        posY = ProbeCard.ProbeCardDevObjectRef.DutCenY + reverseYDir * (viewCenPos.Y * uMperpixel_Y) + (reverseYDir * (dutCenterMousePosY - MouseDownPos.Y) * uMperpixel_Y);
                        StageSupervisor.StageModuleState.PinHighViewMove(posX, posY);
                    }
                    else if (CamType == EnumProberCam.PIN_LOW_CAM)
                    {
                        posX = ProbeCard.ProbeCardDevObjectRef.DutCenX - reverseXDir * (viewCenPos.X * uMperpixel_X) + (reverseXDir * (MouseDownPos.X - dutCenterMousePosX) * uMperpixel_X);
                        posY = ProbeCard.ProbeCardDevObjectRef.DutCenY + reverseYDir * (viewCenPos.Y * uMperpixel_Y) + (reverseYDir * (dutCenterMousePosY - MouseDownPos.Y) * uMperpixel_Y);
                        StageSupervisor.StageModuleState.PinLowViewMove(posX, posY);
                    }
                    else if (CamType == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        posX = WaferObject.GetSubsInfo().DutCenX - reverseXDir * (viewCenPos.X * uMperpixel_X) + (reverseXDir * (MouseDownPos.X - dutCenterMousePosX) * uMperpixel_X);
                        posY = WaferObject.GetSubsInfo().DutCenY + reverseYDir * (viewCenPos.Y * uMperpixel_Y) + (reverseYDir * (dutCenterMousePosY - MouseDownPos.Y) * uMperpixel_Y);

                        //posX = GetTeachDieOffsetX(false) - (viewCenPos.X * uMperpixel_X) + ((MouseDownPos.X - dutCenterMousePosX) * uMperpixel_X);
                        //posY = GetTeachDieOffsetY(false) + (viewCenPos.Y * uMperpixel_Y) + ((dutCenterMousePosY - MouseDownPos.Y) * uMperpixel_Y);

                        StageSupervisor.StageModuleState.WaferHighViewMove(posX, posY);
                    }
                    else if (CamType == EnumProberCam.WAFER_LOW_CAM)
                    {
                        posX = WaferObject.GetSubsInfo().DutCenX - reverseXDir * (viewCenPos.X * uMperpixel_X) + (reverseXDir * (MouseDownPos.X - dutCenterMousePosX) * uMperpixel_X);
                        posY = WaferObject.GetSubsInfo().DutCenY + reverseYDir * (viewCenPos.Y * uMperpixel_Y) + (reverseYDir * (dutCenterMousePosY - MouseDownPos.Y) * uMperpixel_Y);

                        //posX = GetTeachDieOffsetX(false) - (viewCenPos.X * uMperpixel_X) + ((MouseDownPos.X - dutCenterMousePosX) * uMperpixel_X);
                        //posY = GetTeachDieOffsetY(false) + (viewCenPos.Y * uMperpixel_Y) + ((dutCenterMousePosY - MouseDownPos.Y) * uMperpixel_Y);

                        StageSupervisor.StageModuleState.WaferLowViewMove(posX, posY);
                    }
                    //var curpos = VisionManager.GetCam(CamType).GetCurCoordPos();
                    //var curpos = this.StageSupervisor.CoordinateManager().StageCoordConvertToUserCoord(CamType);
                    //CurXPos = curpos.X.Value;
                    //CurYPos = curpos.Y.Value;
                }
                else
                {
                    macIdxX = roundEx((MouseDownPos.X - dutCenterMousePosX - viewCenPos.X + center_shiftX) / DutSizeX);
                    macIdxY = roundEx((dutCenterMousePosY - MouseDownPos.Y + viewCenPos.Y - center_shiftY) / DutSizeY) + 1;

                    ProbeCard.SelectedCoordM.XIndex = (int)macIdxX * reverseXDir;
                    ProbeCard.SelectedCoordM.YIndex = (int)macIdxY * reverseYDir;

                    //SelectDutIndex.XIndex = (int)macIdxX;
                    //SelectDutIndex.YIndex = (int)macIdxY;

                    IsMouseDown = true;
                    e.Handled = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {

            }
        }

        private void MapViewControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            IsMouseDown = false;
        }

        private void MapViewControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (IsMouseDown == false)
                    return;

                if (EnableDragMap == false)
                    return;

                System.Windows.Point currPos = new System.Windows.Point();
                currPos = e.GetPosition((System.Windows.IInputElement)sender);
                //currPos.Y -= 70;
                double diffX = MouseDownPos.X - currPos.X;
                double diffY = MouseDownPos.Y - currPos.Y;
                viewCenPos.X = viewCurrentCenPos.X - (float)diffX;
                viewCenPos.Y = viewCurrentCenPos.Y - (float)diffY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void InitColorPallette()
        {
            try
            {
                resCache.Clear();
                resCache.Add("rectBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 0 / 255f, 0 / 255f, 255 / 255f)));
                resCache.Add("normalDutBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 0 / 255f, 0 / 255f, 255 / 255f)));
                resCache.Add("dutBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 255 / 255f, 255 / 255f, 255 / 255f)));
                resCache.Add("emptyDutBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(112 / 255f, 128 / 255f, 144 / 255f, 255 / 255f)));
                resCache.Add("textBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0 / 255f, 0 / 255f, 0 / 255f, 255 / 255f)));
                resCache.Add("lineBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(245 / 255f, 245 / 255f, 245 / 255f, 255 / 255f)));
                resCache.Add("currentPosBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 0, 255f, 255f)));
                resCache.Add("pinBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 0 / 255f, 0 / 255f, 255 / 255f)));
                resCache.Add("refpinBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0 / 255f, 255 / 255f, 0 / 255f, 255 / 255f)));
                resCache.Add("padBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(135 / 255f, 206 / 255f, 235 / 255f, 255 / 255f)));
                resCache.Add("dutBorderBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0 / 255f, 0 / 255f, 0 / 255f, 255 / 255f)));

                resCache.Add("dutIndexBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(237 / 255f, 125 / 255f, 49 / 255f, 255 / 255f)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        static SharpDX.DirectWrite.Factory fontFactory = new SharpDX.DirectWrite.Factory();

        TextFormat textFormat = new TextFormat(fontFactory, "Segoe UI", 12.0f);

        TextFormat textFormatSmall = new TextFormat(fontFactory, "Segoe UI", 10.0f);
        TextFormat textFormatMid = new TextFormat(fontFactory, "Segoe UI", 16.0f);
        TextFormat textFormatLarge = new TextFormat(fontFactory, "Segoe UI", 20.0f);

        RawVector2 viewCenPos = new RawVector2();
        RawVector2 viewCurrentCenPos = new RawVector2();

        RawVector2 lineTopPlus = new RawVector2();
        RawVector2 lineBottomPlus = new RawVector2();
        //RawVector2 lineTopMinus = new RawVector2();
        RawVector2 lineBottomMinus = new RawVector2();
        RawVector2 lineLeftPlus = new RawVector2();
        RawVector2 lineRightPlus = new RawVector2();
        //RawVector2 lineLeftMinus = new RawVector2();
        RawVector2 lineRightMinus = new RawVector2();

        RawVector2 crossC_cur1 = new RawVector2();
        RawVector2 crossC_cur2 = new RawVector2();

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public int interval = 100;

        // 현재 화면으로 볼 수 있는 최대 거리 (마이크론)
        public double fovWidth = 0;
        public double fovHeight = 0;

        // 더트를 가운데 그리기 위한 마진  (더트맵의 좌측 하단의 위치가 현재 캔버스 중심으로부터 어느 위치에 있는가)
        public double MarginX = 0;
        public double MarginY = 0;

        public float FloatActualWidth = 0.0f;
        public float FloatActualHeight = 0.0f;

        public int GridCount = 50;

        public double UsefulDieSizeWidth = 0;
        public double UsefulDieSizeHeight = 0;

        private List<IDut> ShowingDutList;
        bool bEnablePrefMonitor = false;
        long drawTime = 0;

        //long minDutIndexX = 0;
        //long minDutIndexY = 0;
        long maxDutIndexX = 1;
        long maxDutIndexY = 1;


        public override void Render(RenderTarget target)
        {
            try
            {
                if (bEnablePrefMonitor == true)
                {
                    stw.Stop();
                    stw.Reset();
                    stw.Start();
                }

                target.Clear(new RawColor4(105 / 255f, 105 / 255f, 105 / 255f, 1));

                if (ProbeCard == null || WaferObject == null || ProbeCard.ProbeCardDevObjectRef == null)
                {
                    return;
                }

                IProbeCardDevObject probeCardDevObject = ProbeCard.ProbeCardDevObjectRef;

                ISubstrateInfo substrateInfo = WaferObject.GetSubsInfo();
                IPhysicalInfo physicalInfo = WaferObject.GetPhysInfo();

                if (probeCardDevObject.DutList == null || substrateInfo == null || physicalInfo == null)
                {
                    return;
                }

                long inFOVDutCount = 0;

                double Size = 10000;
                double StageWidth = Size; // 300000;         // 프로브 카드가 존재하는 최대 영역  (300mm)
                double StageHeight = Size; // 300000;

                FloatActualWidth = (float)ActualWidth;
                FloatActualHeight = (float)ActualHeight;

                double ratioX = (ActualWidth / StageWidth) * ZoomLevel;
                double ratioY = (ActualHeight / StageHeight) * ZoomLevel;           // 마이크론 당 몇 픽셀 인가

                fovWidth = StageWidth / ZoomLevel;                           // 현재 화면으로 볼 수 있는 최대 거리 (마이크론)
                fovHeight = StageHeight / ZoomLevel;

                DutSizeX = (float)(interval * ratioX);
                DutSizeY = (float)(interval * ratioY);

                double MMperpixel_X = 0;
                double MMperpixel_Y = 0;

                int verflip = 1;
                int horflip = 1;

                if ((substrateInfo.ActualDieSize.Width.Value <= 0) ||
                    (substrateInfo.ActualDieSize.Height.Value <= 0))
                {
                    UsefulDieSizeWidth = physicalInfo.DieSizeX.Value;
                    UsefulDieSizeHeight = physicalInfo.DieSizeY.Value;

                    DutSizeX = (float)(substrateInfo.ActualDieSize.Width.Value / interval * ratioX);
                    DutSizeY = (float)(substrateInfo.ActualDieSize.Height.Value / interval * ratioY);

                    MMperpixel_X = physicalInfo.DieSizeX.Value / DutSizeX;       // 픽셀 당 몇 마이크론 인가. 픽셀값에 곱하면 실제 거리가 나온다. 
                    MMperpixel_Y = physicalInfo.DieSizeY.Value / DutSizeY;
                }
                else
                {
                    UsefulDieSizeWidth = substrateInfo.ActualDieSize.Width.Value;
                    UsefulDieSizeHeight = substrateInfo.ActualDieSize.Height.Value;

                    DutSizeX = (float)(substrateInfo.ActualDieSize.Width.Value / interval * ratioX);
                    DutSizeY = (float)(substrateInfo.ActualDieSize.Height.Value / interval * ratioY);

                    MMperpixel_X = substrateInfo.ActualDieSize.Width.Value / DutSizeX;       // 픽셀 당 몇 마이크론 인가. 픽셀값에 곱하면 실제 거리가 나온다. 
                    MMperpixel_Y = substrateInfo.ActualDieSize.Height.Value / DutSizeY;
                }

                double centerX = (ActualWidth / 2) + viewCenPos.X;
                double centerY = (ActualHeight / 2) + viewCenPos.Y;

                // 현재 보고있는 화면의 중심

                double roi_left = 0 - (ActualWidth / 2);
                double roi_right = 0 + (ActualWidth / 2);
                double roi_top = 0 - (ActualHeight / 2);
                double roi_bottom = 0 + (ActualHeight / 2);

                double currX = 0;
                double currY = 0;
                double tmpValX = 0;
                double tmpValY = 0;

                MarginX = ProbeCard.ProbeCardDevObjectRef.DutIndexSizeX / 2.0 * -1.0;
                MarginY = probeCardDevObject.DutIndexSizeY / 2.0 - 1.0;   // 더트 좌측 하단을 기준으로 카운트하므로 Y는 하나 빼준다

                float center_shiftX = (float)((MarginX - (int)MarginX)) * DutSizeX;     // 더트맵 사이즈가 홀수냐 짝수냐에 따라 중심점이 더트사이즈의 절반만큼 시프트되므로 이 값만큼 고려해 주어야 한다.
                float center_shiftY = (float)((MarginY - (int)MarginY)) * DutSizeY;

                long indexShiftX = 0;
                long indexShiftY = 0;

                CatCoordinates curPos = new CatCoordinates();

                try
                {
                    if (probeCardDevObject.DutList != null)
                    {
                        if (WaferObject.DispVerFlip == DispFlipEnum.FLIP)
                        {
                            verflip = -1;
                        }

                        if (WaferObject.DispHorFlip == DispFlipEnum.FLIP)
                        {
                            horflip = -1;
                        }

                        ShowingDutList = probeCardDevObject.DutList.ToList();

                        int width = probeCardDevObject.DutIndexSizeX;
                        int height = probeCardDevObject.DutIndexSizeY;

                        maxDutIndexX = 1;
                        maxDutIndexY = 1;

                        // compute this before the loop
                        float dutXPos_preCalc = (float)centerX + horflip * (float)MarginX * DutSizeX;
                        float dutYPos_preCalc = (float)centerY + verflip * (float)MarginY * DutSizeY;

                        foreach (IDut dut in ShowingDutList)
                        {
                            float dutXPos = dutXPos_preCalc + horflip * (float)dut.MacIndex.XIndex * DutSizeX;
                            float dutYPos = dutYPos_preCalc + verflip * ((float)dut.MacIndex.YIndex * -1.0f) * DutSizeY;

                            if (dut.DutNumber == 1)
                            {
                                // 현재 그림에서 첫번째 더트가 위치한 상대 위치를 그림 좌표계 (0,0) 위치를 기준으로 계산한다. 
                                float posX = (float)((dutXPos - centerX + center_shiftX) / DutSizeX);
                                float posY = (float)((centerY - dutYPos - center_shiftY) / DutSizeY);

                                indexShiftX = (long)Math.Round(posX) - (horflip * ProbeCard.FirstDutM.XIndex);
                                indexShiftY = (long)Math.Round(posY) - (verflip * ProbeCard.FirstDutM.YIndex);

                                ProbeCard.FirstDutM.XIndex = horflip * (long)Math.Round(posX);
                                ProbeCard.FirstDutM.YIndex = verflip * (long)Math.Round(posY);

                                PrevFirstCoordX = ProbeCard.FirstDutM.XIndex;
                                PrevFirstCoordY = ProbeCard.FirstDutM.YIndex;
                            }

                            bool InArea = false;

                            if ((0 < (dutXPos + DutSizeX)) && 
                                (0 < (dutYPos + DutSizeY)) &&
                                (dutXPos < ActualWidth) && 
                                (dutYPos < ActualHeight))
                            {
                                inFOVDutCount++;
                                InArea = true;

                                if (dut.DutEnable == false)
                                {
                                    target.FillRectangle(new RawRectangleF(dutXPos + 1, dutYPos + 1, dutXPos + DutSizeX - 1, dutYPos + DutSizeY - 1), resCache["emptyDutBrush"] as Brush);
                                }
                                else
                                {
                                    target.FillRectangle(new RawRectangleF(dutXPos + 1, dutYPos + 1, dutXPos + DutSizeX - 1, dutYPos + DutSizeY - 1), resCache["dutBrush"] as Brush);
                                }
                            }

                            if (InArea == true)
                            {
                                if (DutSizeX <= 30)
                                {
                                }
                                else if (DutSizeX > 30 && DutSizeX <= 50)
                                {
                                    target.DrawText(string.Format("{0}", dut.DutNumber), textFormatMid, new RawRectangleF(dutXPos + 2f, dutYPos - 3f, dutXPos + 200.0f, dutYPos + 36.0f), resCache["textBrush"] as Brush);
                                }
                                else
                                {
                                    target.DrawText(string.Format("{0}", dut.DutNumber), textFormatLarge, new RawRectangleF(dutXPos + 5f, dutYPos, dutXPos + 200.0f, dutYPos + 36.0f), resCache["textBrush"] as Brush);
                                }
                            }

                            // 패드 위치 표시
                            if (ShowPad == true)
                            {
                                float paddutXPos = (float)((float)centerX + horflip * ((float)dut.MacIndex.XIndex + ((horflip < 0) ? horflip : 0) + (float)MarginX) * DutSizeX);
                                float paddutYPos = (float)((float)centerY + verflip * ((float)dut.MacIndex.YIndex * -1.0 - ((verflip < 0) ? verflip : 0) + (float)MarginY) * DutSizeY);

                                DrawPads(target, dut, paddutXPos, paddutYPos, (float)centerX, (float)centerY);
                            }
                            // 핀 위치 표시
                            if (ShowPin == true)
                            {
                                foreach (IPinData ipin in dut.PinList)
                                {
                                    //var dutMacIndex = ipin.DutMacIndex.Value;

                                    var pinPosX = (float)ipin.AbsPosOrg.X.Value;
                                    var pinPosY = (float)ipin.AbsPosOrg.Y.Value;

                                    tmpValX = (pinPosX + -probeCardDevObject.DutCenX) / MMperpixel_X;
                                    tmpValY = (pinPosY + -probeCardDevObject.DutCenY) / MMperpixel_Y;

                                    // TODO: FLIP 검토 할것 
                                    var currPinX = centerX + tmpValX * horflip;
                                    var currPinY = centerY - tmpValY * verflip;

                                    RawVector2 CenPos = new RawVector2((float)currPinX, (float)currPinY);

                                    if (ipin.PinNum.Value == 1)
                                    {
                                        target.FillEllipse(new Ellipse(CenPos, 3, 3), resCache["refpinBrush"] as Brush);
                                    }
                                    else
                                    {
                                        target.FillEllipse(new Ellipse(CenPos, 3, 3), resCache["pinBrush"] as Brush);
                                    }
                                }
                            }

                            if (dut.MacIndex.XIndex >= maxDutIndexX)
                            {
                                maxDutIndexX = dut.MacIndex.XIndex;
                            }

                            if (dut.MacIndex.YIndex >= maxDutIndexY)
                            {
                                maxDutIndexY = dut.MacIndex.YIndex;
                            }
                        }

                        #region //==> dut map Line drawing

                        if (ShowGrid == true)
                        {
                            if (maxDutIndexX < GridCount)
                            {
                                maxDutIndexX = GridCount;
                            }

                            if (maxDutIndexY < GridCount)
                            {
                                maxDutIndexY = GridCount;
                            }

                            DrawGrids(target);
                        }
                        #endregion
                    }

                    ProbeCard.SelectedCoordM.XIndex += (horflip * indexShiftX);
                    ProbeCard.SelectedCoordM.YIndex += (verflip * indexShiftY);


                    // 그림의 정 가운데에 위치한 기준선이 그림좌표계 (0, 0) 더트의 좌측 상단이 된다.
                    // 그리는 좌표계는 우측이 +, 하단이 + 이다. 주의할 것.
                    long SelectedIndexX = horflip * ProbeCard.SelectedCoordM.XIndex;
                    long SelectedIndexY = verflip * ProbeCard.SelectedCoordM.YIndex;

                    // 현재 선택된 더트 표시
                    if (ShowSelectedDut == true)
                    {
                        float xPos = (float)(centerX + (SelectedIndexX * DutSizeX) - center_shiftX);
                        float yPos = (float)(centerY + (-(SelectedIndexY * DutSizeY)) - center_shiftY);        // Y축은 그리는 좌표방향이 반대

                        //==>선택된 Dut는 다른 색으로 표현
                        target.DrawRectangle(new RawRectangleF(xPos, yPos, xPos + DutSizeX, yPos + DutSizeY), resCache["rectBrush"] as Brush);
                    }

                    // 현재 위치 표시
                    if (ShowCurrentPos == true)
                    {
                        if (CurCam == null)
                        {
                            curPos = VisionManager.GetCam(CamType).GetCurCoordPos();
                        }

                        if (CamType == EnumProberCam.PIN_HIGH_CAM || CamType == EnumProberCam.PIN_LOW_CAM)
                        {
                            tmpValX = (CurXPos - probeCardDevObject.DutCenX) / MMperpixel_X;
                            tmpValY = (CurYPos - probeCardDevObject.DutCenY) / MMperpixel_Y;
                        }
                        else if (CamType == EnumProberCam.WAFER_HIGH_CAM || CamType == EnumProberCam.WAFER_LOW_CAM)
                        {
                            tmpValX = (CurXPos - substrateInfo.DutCenX) / MMperpixel_X;
                            tmpValY = (CurYPos - substrateInfo.DutCenY) / MMperpixel_Y;                            
                        }
                        currX = (ActualWidth / 2) + tmpValX * horflip;// TODO: FLIP 맞는거
                        currY = (ActualHeight / 2) - tmpValY * verflip;// TODO: FLIP 맞는거

                        currX -= ((horflip < 0) ? horflip : 0) * DutSizeX;
                        currY += -1.0 * ((verflip < 0) ? verflip : 0) * DutSizeY;

                        ////currX, currY: 현재 십자가 위치
                        currX += viewCenPos.X;
                        currY += viewCenPos.Y;

                        ///// TODO: FLIP 검토 할것 

                        //--- 가로선
                        crossC_cur1.X = (float)(currX - 10.0);
                        crossC_cur2.X = (float)(currX + 10.0);

                        crossC_cur1.Y = (float)(currY);
                        crossC_cur2.Y = (float)(currY);

                        target.DrawLine(crossC_cur1, crossC_cur2, resCache["currentPosBrush"] as Brush);

                        //--- 세로선
                        crossC_cur1.X = (float)(currX);
                        crossC_cur2.X = (float)(currX);

                        crossC_cur1.Y = (float)(currY - 10.0);
                        crossC_cur2.Y = (float)(currY + 10.0);

                        target.DrawLine(crossC_cur1, crossC_cur2, resCache["currentPosBrush"] as Brush);

                        if (ShowCurrentPosROI == true)
                        {
                            double roi_x_um = CurCam.GetRatioX() * CurCam.GetGrabSizeWidth();
                            double roi_y_um = CurCam.GetRatioY() * CurCam.GetGrabSizeHeight();

                            float rect_size_x = (float)(roi_x_um / 2.0 / MMperpixel_X);
                            float rect_size_y = (float)(roi_y_um / 2.0 / MMperpixel_Y);

                            float roi_sx = (float)currX - rect_size_x;
                            float roi_sy = (float)currY - rect_size_y;

                            float roi_ex = (float)currX + rect_size_x;
                            float roi_ey = (float)currY + rect_size_y;

                            target.DrawRectangle(new RawRectangleF(roi_sx, roi_sy, roi_ex, roi_ey), resCache["currentPosBrush"] as Brush);
                        }
                    }

                    target.AntialiasMode = AntialiasMode.Aliased;

                    if (bEnablePrefMonitor == true)
                    {
                        drawTime = stw.ElapsedMilliseconds;
                    }
                    else
                    {
                        stw.Stop();
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DrawPads(RenderTarget target, IDut dut, float dutXPos, float dutYPos, float cenPosX, float cenPosY)
        {
            float padPosX = 0;
            float padPosY = 0;

            float cenx = cenPosX;
            float ceny = cenPosY;

            int verflip = 1;
            int horflip = 1;

            try
            {
                if (WaferObject.GetSubsInfo().Pads.DutPadInfos != null)
                {
                    if (WaferObject.DispVerFlip == DispFlipEnum.FLIP)
                    {
                        verflip = -1;
                    }

                    if (WaferObject.DispHorFlip == DispFlipEnum.FLIP)
                    {
                        horflip = -1;
                    }

                    foreach (DUTPadObject dutPadData in WaferObject.GetSubsInfo().Pads.DutPadInfos)
                    {
                        // TODO: FLIP 검토 할것 
                        DUTPadObject tempPad = dutPadData;


                        if (tempPad.DutNumber == dut.DutNumber)
                        {
                            padPosX = (float)(tempPad.PadCenter.X.Value * horflip * (DutSizeX / UsefulDieSizeWidth));
                            padPosY = (float)(tempPad.PadCenter.Y.Value * verflip * (DutSizeY / UsefulDieSizeHeight));

                            // 그리는 좌표계에서는 Y축 방향이 반대이므로 부호를 반대로 한다. + dutYpos는 더트의 왼쪽 상단 위를 가리키므로 더트 좌측 하단으로부터의 상대거리를 계산하기 위해 Y방향으로 하나 더해준다.
                            if (tempPad.PadNumber.Value == this.ProbeCard.ProbeCardDevObjectRef.RefPinNum.Value)
                            {
                                target.FillRectangle(new RawRectangleF(
                                        dutXPos + (padPosX - 2),
                                        dutYPos + DutSizeY - (padPosY - 2),
                                        dutXPos + padPosX + 2,
                                        dutYPos + DutSizeY - (padPosY + 2)),
                                    resCache["refpinBrush"] as Brush);
                            }
                            else
                            {
                                target.FillRectangle(new RawRectangleF(
                                    dutXPos + (padPosX - 2),
                                    dutYPos + DutSizeY - (padPosY - 2),
                                    dutXPos + padPosX + 2,
                                    dutYPos + DutSizeY - (padPosY + 2)),
                                    resCache["pinBrush"] as Brush);
                            }
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


        }

        //private void DrawPMIObject(RenderTarget target, uint Enable, float rectHalfWidth, float rectHalfHeight, float posx, float posy)
        private void DrawGrids(RenderTarget target)
        {
            lineTopPlus.Y = 0;
            lineBottomPlus.Y = (float)ActualHeight;
            //lineTopMinus.Y = 0;
            lineBottomMinus.Y = (float)ActualHeight;

            lineLeftPlus.X = 0;
            lineRightPlus.X = (float)ActualWidth;
            //lineLeftMinus.X = 0;
            lineRightMinus.X = (float)ActualWidth;

            float lineWidthPlus = 0;
            //float lineWidthMinus = 0;
            //float lineHeightPlus = 0;
            float lineHeightMinus = 0;

            var half_fovWidth = (fovWidth / 2.0);
            var half_fovHeight = (fovHeight / 2.0);

            var ratiox = (ActualWidth / fovWidth);
            var ratioy = (ActualHeight / fovHeight);

            Brush linebrush = resCache["lineBrush"] as Brush;

            float center_shiftX = (float)((MarginX - (int)MarginX)) * DutSizeX;     // 더트맵 사이즈가 홀수냐 짝수냐에 따라 중심점이 더트사이즈의 절반만큼 시프트되므로 이 값만큼 고려해 주어야 한다.
            float center_shiftY = (float)((MarginY - (int)MarginY)) * DutSizeY;

            float xref = (float)(half_fovWidth * ratiox) + viewCenPos.X - center_shiftX;// + (float)(MarginX * DutSizeX);
            float yref = (float)(half_fovHeight * ratioy) + viewCenPos.Y + center_shiftY;// + (float)(MarginY * DutSizeY);

            int verticalCount = (int)maxDutIndexX;
            int horizontalCount = (int)maxDutIndexY;

            //if (maxDutIndexX > maxDutIndexY)
            //{
            //    GridCount = (int)((double)((int)maxDutIndexX + 1) * 1.5);
            //}
            //else
            //{
            //    GridCount = (int)((double)((int)maxDutIndexY + 1) * 1.5);
            //}

            // Vertical line
            for (int i = -verticalCount; i < verticalCount; i++)
            {
                lineWidthPlus = xref + (float)(DutSizeX * i);
                lineTopPlus.X = lineWidthPlus;
                lineBottomPlus.X = lineWidthPlus;
                target.DrawLine(lineTopPlus, lineBottomPlus, linebrush);
            }

            // Horizontal line
            for (int i = -horizontalCount; i < horizontalCount; i++)
            {
                lineHeightMinus = yref + (float)(DutSizeY * i);
                lineLeftPlus.Y = lineHeightMinus;
                lineRightPlus.Y = lineHeightMinus;
                target.DrawLine(lineLeftPlus, lineRightPlus, linebrush);
            }

            //// TODO : Line 4개가 빠른지, Renctangle 1개가 빠른지 테스트 해볼 것.
            //for (int i = 0; i < GridCount; i++)
            //{

            //    if (i != 0)
            //    {
            //        lineWidthMinus = (float)(((-interval * i) + (fovWidth / 2.0)) * (FloatActualWidth / fovWidth));
            //        lineTopMinus.X = lineWidthMinus + viewCenPos.X + (float)(MarginX * DutSizeX);
            //        lineBottomMinus.X = lineWidthMinus + viewCenPos.X + (float)(MarginX * DutSizeX);
            //        target.DrawLine(lineTopMinus, lineBottomMinus, resCache["lineBrush"] as Brush);
            //    }

            //    // (2)
            //    lineWidthPlus = (float)((fovWidth / 2.0) * ratiox) + (float)(DutSizeX * i);
            //    xpos = lineWidthPlus + viewCenPos.X + (float)(MarginX * DutSizeX);
            //    lineTopPlus.X = xpos;
            //    lineBottomPlus.X = xpos;

            //    target.DrawLine(lineTopPlus, lineBottomPlus, resCache["lineBrush"] as Brush);

            //    //if (i != 0)
            //    //{
            //    //    lineHeightMinus = (float)(((-interval * i) + (fovHeight / 2.0)) * (FloatActualHeight / fovHeight));
            //    //    lineLeftMinus.Y = lineHeightMinus + viewCenPos.Y + (float)(MarginY * DutSizeY);
            //    //    lineRightMinus.Y = lineHeightMinus + viewCenPos.Y + (float)(MarginY * DutSizeY);
            //    //    target.DrawLine(lineLeftMinus, lineRightMinus, resCache["lineBrush"] as Brush);
            //    //}

            //    //lineHeightPlus = (float)(((interval * i) + (fovHeight / 2.0)) * (FloatActualHeight / fovHeight));
            //    //lineLeftPlus.Y = lineHeightPlus + viewCenPos.Y + (float)(MarginY * DutSizeY);
            //    //lineRightPlus.Y = lineHeightPlus + viewCenPos.Y + (float)(MarginY * DutSizeY);
            //    //target.DrawLine(lineLeftPlus, lineRightPlus, resCache["lineBrush"] as Brush);
            //}
        }

        private void DrawPins(RenderTarget target, IDut dut, float dutXPos, float dutYPos, double pcCenX, double pcCenY)
        {
            float pinPosX = 0;
            float pinPosY = 0;
            double posToPixelX = UsefulDieSizeWidth / DutSizeX;
            double posToPixelY = UsefulDieSizeHeight / DutSizeY;
            int verflip = 1;
            int horflip = 1;
            foreach (IPinData ipin in dut.PinList)
            {
                var dutMacIndex = ipin.DutMacIndex.Value;

                //pinPosX = (float)((ipin.RelPos.X.Value + ipin.AlignedOffset.X.Value) * (DutSizeX / UsefulDieSizeWidth));
                //pinPosY = (float)((ipin.RelPos.Y.Value + ipin.AlignedOffset.Y.Value) * (DutSizeY / UsefulDieSizeHeight));
                pinPosX = (float)((ipin.RelPos.X.Value + ipin.AlignedOffset.X.Value) / posToPixelX);
                pinPosY = (float)((ipin.RelPos.Y.Value + ipin.AlignedOffset.Y.Value) / posToPixelY);

                //그리는 좌표계에서는 Y축 방향이 반대이므로 부호를 반대로 한다. +dutYpos는 더트의 왼쪽 상단 위를 가리키므로 더트 좌측 하단으로부터의 상대거리를 계산하기 위해 Y방향으로 하나 더해준다.
                //RawVector2 CenPos = new RawVector2(dutXPos + pinPosX, dutYPos + DutSizeY - pinPosY);

                RawVector2 CenPos = new RawVector2((float)pcCenX + pinPosX * horflip, (float)(pcCenY - pinPosY * verflip));

                if (ipin.PinNum.Value == 1)
                {
                    target.FillEllipse(new Ellipse(CenPos, 3, 3), resCache["refpinBrush"] as Brush);
                }
                else
                {
                    target.FillEllipse(new Ellipse(CenPos, 3, 3), resCache["pinBrush"] as Brush);
                }

            }
        }

        public Task DutAddbyMouseDown()
        {
            throw new NotImplementedException();
        }
    }
}

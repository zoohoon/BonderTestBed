using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UcDisplayPort
{
    using Autofac;
    using CUIServices;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using SharpDXRender;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    [DataContract]
    public partial class DisplayPort : UserControl, IDisplayPort, IFactoryModule, ICUIControl
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        public List<Label> OSDLabels = new List<Label>();

        public static readonly DependencyProperty RenderLayerProperty = DependencyProperty.Register("RenderLayer", typeof(RenderLayer), typeof(DisplayPort), null);
        public RenderLayer RenderLayer
        {
            get { return (RenderLayer)this.GetValue(DisplayPort.RenderLayerProperty); }
            set
            {
                this.SetValue(DisplayPort.RenderLayerProperty, value);
            }
        }
        private bool disposed = false;
        public void Dispose()
        {
            try
            {
                this.Dispose(true);
                //  GC.SuppressFinalize(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            try
            {

                if (this.disposed) return;
                if (disposing)
                {

                    // IDisposable 인터페이스를 구현하는 멤버들을 여기서 정리합니다.
                    //IDisposable[] targetList = new IDisposable[this.items.Count];
                    //this.items.CopyTo(targetList);
                    //foreach (IDisposable eachItem in targetList)
                    //{
                    //    eachItem.Dispose();
                    //}
                    //this.items.Clear();
                }
                // .NET Framework에 의하여 관리되지 않는 외부 리소스들을 여기서 정리합니다.
                this.disposed = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        #region // Public members
        private bool mOverlayVisible;
        public bool OverlayVisible
        {
            get { return mOverlayVisible; }
            set { mOverlayVisible = value; }
        }

        private EnumProberCam mBindedChannel;

        public EnumProberCam BindedChannel
        {
            get { return mBindedChannel; }
            set { mBindedChannel = value; }
        }

        private int mGrabSizeX;

        public int GrabSizeX
        {
            get { return mGrabSizeX; }
            set { mGrabSizeX = value; }
        }

        private int mGrabSizeY;

        public int GrabSizeY
        {
            get { return mGrabSizeY; }
            set { mGrabSizeY = value; }
        }

        private double _ActualWidth;
        public new double ActualWidth
        {
            get { return _ActualWidth; }
            set
            {
                if (value != _ActualWidth)
                {
                    _ActualWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ActualHeight;
        public new double ActualHeight
        {
            get { return _ActualHeight; }
            set
            {
                if (value != _ActualHeight)
                {
                    _ActualHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _VmDispVerFlip;
        public int VmDispVerFlip
        {
            get { return _VmDispVerFlip; }
            set
            {
                if (value != _VmDispVerFlip)
                {
                    _VmDispVerFlip = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _VmDispHorFlip;
        public int VmDispHorFlip
        {
            get { return _VmDispHorFlip; }
            set
            {
                if (value != _VmDispHorFlip)
                {
                    _VmDispHorFlip = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public DisplayPort()
        {
            try
            {
                InitEvents();
                OSDLabelStyleFontSize = _DefaultOSDLabelStyleFontSize;
                OverlayFontSize = _DefaultOverlayFontSize;
                InitializeComponent();

                mGrabSizeX = 480;
                mGrabSizeY = 480;

                horizontalLine = new Line();
                verticalLine = new Line();

                //if(this.GetContainer().IsRegistered<IVisionManager>())
                //if(SystemManager.SysteMode == SystemModeEnum.Single)
                //{
                if (this.GetContainer().IsRegistered<IStageSupervisor>())
                    StageSupervisor = this.StageSupervisor();
                if (this.GetContainer().IsRegistered<IVisionManager>())
                    VisionManager = this.VisionManager();
                if (this.GetContainer().IsRegistered<ICoordinateManager>())
                    CoordManager = this.CoordinateManager();
                if (this.GetContainer().IsRegistered<IMotionManager>())
                {
                    MotionManager = this.MotionManager();
                    AxisXPos = MotionManager.GetAxis(EnumAxisConstants.X);
                    AxisYPos = MotionManager.GetAxis(EnumAxisConstants.Y);
                    AxisZPos = MotionManager.GetAxis(EnumAxisConstants.Z);
                    AxisTPos = MotionManager.GetAxis(EnumAxisConstants.C);
                    AxisPZPos = MotionManager.GetAxis(EnumAxisConstants.PZ);
                }
                //}


                UseUserControlFunc = UserControlFucEnum.DEFAULT;
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
                this.Loaded += DisplayControl_Loaded;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void DisplayControl_Loaded(object sender, RoutedEventArgs e)
        {
            VmDispVerFlip = (int)StageSupervisor.VisionManager().GetDispVerFlip();
            VmDispHorFlip = (int)StageSupervisor.VisionManager().GetDispHorFlip();
            LoggerManager.Debug($"DisplayControl_Loaded(): VmDispHorFlip:{VmDispHorFlip}, VmDispVerFlip,{VmDispVerFlip}");

        }


        //public DisplayPort(Guid guid)
        //{
        //    try
        //    {
        //        OSDLabelStyleFontSize = _DefaultOSDLabelStyleFontSize;
        //        OverlayFontSize = _DefaultOverlayFontSize;
        //        this.GUID = guid;
        //        InitializeComponent();

        //        mGrabSizeX = 480;
        //        mGrabSizeY = 480;

        //        horizontalLine = new Line();
        //        verticalLine = new Line();

        //        StageSupervisor = this.StageSupervisor();
        //        VisionManager = this.VisionManager();
        //        MotionManager = this.MotionManager();
        //        CoordManager = this.CoordinateManager();
        //        AxisXPos = MotionManager.GetAxis(EnumAxisConstants.X);
        //        AxisYPos = MotionManager.GetAxis(EnumAxisConstants.Y);
        //        AxisZPos = MotionManager.GetAxis(EnumAxisConstants.Z);
        //        AxisTPos = MotionManager.GetAxis(EnumAxisConstants.C);
        //        AxisPZPos = MotionManager.GetAxis(EnumAxisConstants.PZ);

        //        UseUserControlFunc = UserControlFucEnum.DEFAULT;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        ~DisplayPort()
        {

        }

        public void RegistMouseDownEvent(MouseButtonEventHandler MouseDownHandler)
        {
            MouseDown += MouseDownHandler;
        }
        public Object GetViewObject()
        {
            return this.OverlayCanvas;
        }

        public Object GetRenderControl()
        {
            return this.mainRender;
        }


        private double _OSDCanvasHalfWidth;
        public double OSDCanvasHalfWidth
        {
            get { return _OSDCanvasHalfWidth; }
            set
            {
                if (value != _OSDCanvasHalfWidth)
                {
                    _OSDCanvasHalfWidth = value;
                    ChangeTargetRectWidth(TargetRectangleWidth);
                    ChangeOSDCanvasSize();
                    RaisePropertyChanged();
                }
            }
        }

        private double _OSDCanvasHalfHeight;
        public double OSDCanvasHalfHeight
        {
            get { return _OSDCanvasHalfHeight; }
            set
            {
                if (value != _OSDCanvasHalfHeight)
                {
                    _OSDCanvasHalfHeight = value;
                    ChangeTargetRectHeight(TargetRectangleHeight);
                    ChangeOSDCanvasSize();
                    RaisePropertyChanged();
                }
            }
        }

        private double _RectWidth;
        public double RectWidth
        {
            get { return _RectWidth; }
            set
            {
                if (value != _RectWidth)
                {
                    _RectWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _RectHeight;
        public double RectHeight
        {
            get { return _RectHeight; }
            set
            {
                if (value != _RectHeight)
                {
                    _RectHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _RectLeft;
        public double RectLeft
        {
            get { return _RectLeft; }
            set
            {
                if (value != _RectLeft)
                {
                    _RectLeft = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _RectTop;
        public double RectTop
        {
            get { return _RectTop; }
            set
            {
                if (value != _RectTop)
                {
                    _RectTop = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DefaultOSDLabelStyleFontSize = 14;
        private double _DefaultOverlayFontSize = 24;

        private double _OSDLabelStyleFontSize;
        public double OSDLabelStyleFontSize
        {
            get { return _OSDLabelStyleFontSize; }
            set
            {
                if (value != _OSDLabelStyleFontSize)
                {
                    _OSDLabelStyleFontSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OverlayFontSize;
        public double OverlayFontSize
        {
            get { return _OverlayFontSize; }
            set
            {
                if (value != _OverlayFontSize)
                {
                    _OverlayFontSize = value;
                    RaisePropertyChanged();
                }
            }
        }





        private double _StandardOverlayCanvaseWidth = 890;

        public double StandardOverlayCanvaseWidth
        {
            get { return _StandardOverlayCanvaseWidth; }
            set { _StandardOverlayCanvaseWidth = value; }
        }

        private double _StandardOverlayCanvaseHeight = 890;

        public double StandardOverlayCanvaseHeight
        {
            get { return _StandardOverlayCanvaseHeight; }
            set { _StandardOverlayCanvaseHeight = value; }
        }

        private double _MarkButtonSize;
        public double MarkButtonSize
        {
            get { return _MarkButtonSize; }
            set
            {
                if (value != _MarkButtonSize)
                {
                    _MarkButtonSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MarkButtonSizeRatio = 20;
        public double MarkButtonSizeRatio
        {
            get { return _MarkButtonSizeRatio; }
            set
            {
                if (value != _MarkButtonSizeRatio)
                {
                    _MarkButtonSizeRatio = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Thickness _MarkerButtonMargin;
        public Thickness MarkerButtonMargin
        {
            get { return _MarkerButtonMargin; }
            set
            {
                if (value != _MarkerButtonMargin)
                {
                    _MarkerButtonMargin = value;
                    RaisePropertyChanged();
                }
            }
        }



        //private static CatCoordinates _PosCoord;

        private static void MotionPosPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                CatCoordinates _PosCoord;
                DisplayPort dp = null;

                if (sender is DisplayPort)
                {
                    dp = (DisplayPort)sender;
                }

                if (dp.CoordManager != null & dp != null)
                {
                    _PosCoord = dp.CoordManager.StageCoordConvertToUserCoord(_AssignedCameraType);

                    //dp.UserCoordXPos = _PosCoord.X.Value;
                    //dp.UserCoordYPos = _PosCoord.Y.Value;
                    //dp.UserCoordZPos = _PosCoord.Z.Value;

                    if (dp.AssignedCamera != null)
                    {
                        //dp.AssignedCamera.ViewDisplay();
                        if (_AssignedCameraType == EnumProberCam.WAFER_HIGH_CAM ||
                        _AssignedCameraType == EnumProberCam.WAFER_LOW_CAM)
                        {
                            UserIndex wuindex = dp.CoordManager.GetCurUserIndex(_PosCoord);
                            //dp.UserWaferIndexX = wuindex.XIndex.Value;
                            //dp.UserWaferIndexY = wuindex.YIndex.Value;

                            MachineIndex mcindex = dp.CoordManager.WUIndexConvertWMIndex(wuindex.XIndex, wuindex.YIndex);
                            dp.WaferMachineIndexX = mcindex.XIndex;
                            dp.WaferMachineIndexY = mcindex.YIndex;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private static void SetZeroPosPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                CatCoordinates _PosCoord;
                DisplayPort dp = null;

                if (sender is DisplayPort)
                {
                    dp = (DisplayPort)sender;
                }

                if (dp.CoordManager != null & dp != null)
                {
                    double tempX = 0d;
                    double tempY = 0d;
                    double tempZ = 0d;
                    double tempPZ = 0d;

                    _PosCoord = dp.CoordManager.StageCoordConvertToUserCoord(_AssignedCameraType);

                    tempX = _PosCoord.X.Value;
                    tempX = _PosCoord.Y.Value;
                    tempX = _PosCoord.Z.Value;
                    tempPZ = _PosCoord.PZ.Value;

                    dp.SetZeroCoordXPos = dp.MoveXValue - tempX;
                    dp.SetZeroCoordYPos = dp.MoveYValue - tempY;
                    dp.SetZeroCoordZPos = tempZ;
                    dp.SetZeroCoordPZPos = tempPZ;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #region //..Binding DependencyProperty 

        public static readonly DependencyProperty PosX =
        DependencyProperty.Register("MotionPosX",
                                    typeof(double),
                                    typeof(DisplayPort),
                                    new FrameworkPropertyMetadata(0D, new PropertyChangedCallback(MotionPosPropertyChanged)));
        public double MotionPosX
        {
            get { return (double)this.GetValue(PosX); }
            set { this.SetValue(PosX, value); RaisePropertyChanged(); }
        }


        public static readonly DependencyProperty PosY =
        DependencyProperty.Register("MotionPosY",
                               typeof(double),
                               typeof(DisplayPort),
                               new FrameworkPropertyMetadata(0D, new PropertyChangedCallback(MotionPosPropertyChanged)));
        public double MotionPosY
        {
            get { return (double)this.GetValue(DisplayPort.PosY); }
            set { this.SetValue(DisplayPort.PosY, value); RaisePropertyChanged(); }
        }

        public static readonly DependencyProperty PosZ =
        DependencyProperty.Register("MotionPosZ",
                               typeof(double),
                               typeof(DisplayPort),
                               new FrameworkPropertyMetadata(0D, new PropertyChangedCallback(MotionPosPropertyChanged)));
        public double MotionPosZ
        {
            get { return (double)this.GetValue(DisplayPort.PosZ); }
            set
            {
                this.SetValue(DisplayPort.PosZ, value);
                RaisePropertyChanged();
            }
        }

        public static readonly DependencyProperty PosT =
        DependencyProperty.Register("MotionPosT",
                               typeof(double),
                               typeof(DisplayPort),
                               new FrameworkPropertyMetadata(0D, new PropertyChangedCallback(MotionPosPropertyChanged)));
        public double MotionPosT
        {
            get { return (double)GetValue(DisplayPort.PosT); }
            set
            {
                this.SetValue(DisplayPort.PosT, value);
                RaisePropertyChanged();
            }
        }

        #region ==>SetZero
        public static readonly DependencyProperty SetZeroCoordX =
        DependencyProperty.Register("SetZeroCoordXPos",
                               typeof(double),
                               typeof(DisplayPort),
                               new FrameworkPropertyMetadata(0D));
        public double SetZeroCoordXPos
        {
            get { return (double)this.GetValue(DisplayPort.SetZeroCoordX); }
            set
            {
                this.SetValue(DisplayPort.SetZeroCoordX, value);
                RaisePropertyChanged();
            }
        }

        public static readonly DependencyProperty SetZeroCoordY =
        DependencyProperty.Register("SetZeroCoordYPos",
                       typeof(double),
                       typeof(DisplayPort),
                       new FrameworkPropertyMetadata(0D));
        public double SetZeroCoordYPos
        {
            get { return (double)this.GetValue(DisplayPort.SetZeroCoordY); }
            set
            {
                this.SetValue(DisplayPort.SetZeroCoordY, value);
                RaisePropertyChanged();
            }
        }

        public static readonly DependencyProperty SetZeroCoordZ =
        DependencyProperty.Register("SetZeroCoordZPos",
                       typeof(double),
                       typeof(DisplayPort),
                       new FrameworkPropertyMetadata(0D));
        public double SetZeroCoordZPos
        {
            get { return (double)this.GetValue(DisplayPort.SetZeroCoordZ); }
            set
            {
                this.SetValue(DisplayPort.SetZeroCoordZ, value);
                RaisePropertyChanged();
            }
        }

        public static readonly DependencyProperty SetZeroCoordPZ =
        DependencyProperty.Register("SetZeroCoordPZPos",
               typeof(double),
               typeof(DisplayPort),
               new FrameworkPropertyMetadata(0D));
        public double SetZeroCoordPZPos
        {
            get { return (double)this.GetValue(DisplayPort.SetZeroCoordPZ); }
            set
            {
                this.SetValue(DisplayPort.SetZeroCoordPZ, value);
                RaisePropertyChanged();
            }
        }
        #endregion

        public static readonly DependencyProperty ChuckCoordX =
        DependencyProperty.Register("ChuckCoordXPos",
                               typeof(double),
                               typeof(DisplayPort),
                               new FrameworkPropertyMetadata(0D));
        public double ChuckCoordXPos
        {
            get { return (double)this.GetValue(DisplayPort.ChuckCoordX); }
            set { this.SetValue(DisplayPort.ChuckCoordX, value); }
        }

        public static readonly DependencyProperty ChuckCoordY =
        DependencyProperty.Register("ChuckCoordYPos",
                       typeof(double),
                       typeof(DisplayPort),
                       new FrameworkPropertyMetadata(0D));
        public double ChuckCoordYPos
        {
            get { return (double)this.GetValue(DisplayPort.ChuckCoordY); }
            set { this.SetValue(DisplayPort.ChuckCoordY, value); }
        }

        public static readonly DependencyProperty ChuckCoordZ =
        DependencyProperty.Register("ChuckCoordZPos",
                       typeof(double),
                       typeof(DisplayPort),
                       new FrameworkPropertyMetadata(0D));
        public double ChuckCoordZPos
        {
            get { return (double)this.GetValue(DisplayPort.ChuckCoordZ); }
            set { this.SetValue(DisplayPort.ChuckCoordZ, value); }
        }

        public static readonly DependencyProperty WaferMachineIndexXProperty =
        DependencyProperty.Register("WaferMachineIndexX",
                              typeof(double),
                              typeof(DisplayPort),
                              new FrameworkPropertyMetadata(0D));

        public double WaferMachineIndexX
        {
            get { return (double)this.GetValue(DisplayPort.WaferMachineIndexXProperty); }
            set { this.SetValue(DisplayPort.WaferMachineIndexXProperty, value); }
        }

        public static readonly DependencyProperty WaferMachineIndexYProperty =
        DependencyProperty.Register("WaferMachineIndexY",
                      typeof(double),
                      typeof(DisplayPort),
                      new FrameworkPropertyMetadata(0D));

        public double WaferMachineIndexY
        {
            get { return (double)this.GetValue(DisplayPort.WaferMachineIndexYProperty); }
            set { this.SetValue(DisplayPort.WaferMachineIndexYProperty, value); }
        }


        private static void UseUserControlFuncPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                DisplayPort dp = null;
                if (sender is DisplayPort)
                {
                    dp = (DisplayPort)sender;
                }

                if (dp != null)
                {
                    if (e.NewValue is UserControlFucEnum)
                    {
                        dp.UseUserControlFunc = (UserControlFucEnum)e.NewValue;

                        switch (dp.UseUserControlFunc)
                        {
                            case UserControlFucEnum.DEFAULT:
                                dp.BoundaryLineHidden();
                                dp.TargetRectangle.Visibility = Visibility.Hidden;
                                dp.ViewCrossLine();
                                break;
                            case UserControlFucEnum.PTRECT:
                                dp.BoundaryLineHidden();
                                dp.ChangeTargetRectWidth(dp.TargetRectangleWidth);
                                dp.ChangeTargetRectHeight(dp.TargetRectangleHeight);
                                dp.TargetRectangle.Visibility = Visibility.Visible;
                                break;
                            case UserControlFucEnum.DIELEFTCORNER:
                                dp.TargetRectangle.Visibility = Visibility.Hidden;
                                dp.BoundaryLineHidden();
                                dp.LeftBottemCornerLineVisiable();
                                break;
                            case UserControlFucEnum.DIERIGHTCORNER:
                                dp.TargetRectangle.Visibility = Visibility.Hidden;
                                dp.BoundaryLineHidden();
                                dp.RightBottomXLine.Visibility = Visibility.Visible;
                                dp.RightBottomYLine.Visibility = Visibility.Visible;

                                dp.RightTopCornerLineVisiable();
                                break;
                            case UserControlFucEnum.DIEBOUNDARY:
                                dp.TargetRectangle.Visibility = Visibility.Hidden;
                                dp.BoundaryLineVisiable();
                                break;
                            default:
                                //dp.TargetRectangle.Visibility = Visibility.Hidden;
                                //dp.BoundaryLineHidden();
                                break;
                        }
                    }
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public static readonly DependencyProperty UseUserControlFuncProp =
        DependencyProperty.Register("UseUserControlFunc",
              typeof(UserControlFucEnum),
              typeof(DisplayPort),
              new FrameworkPropertyMetadata(UserControlFucEnum.UNDEFINED,
                         new PropertyChangedCallback(UseUserControlFuncPropertyChanged)));

        public UserControlFucEnum UseUserControlFunc
        {
            get { return (UserControlFucEnum)this.GetValue(DisplayPort.UseUserControlFuncProp); }
            set { this.SetValue(DisplayPort.UseUserControlFuncProp, value); }
        }

        private static void TargetRectangelWidthPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                DisplayPort dp = null;
                if (sender is DisplayPort)
                {
                    dp = (DisplayPort)sender;
                }

                if (dp != null)
                {

                    dp.ChangeTargetRectWidth((double)e.OldValue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static void TargetRectangeHeightlPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DisplayPort dp = null;
            if (sender is DisplayPort)
            {
                dp = (DisplayPort)sender;
            }

            if (dp != null)
            {
                dp.ChangeTargetRectHeight((double)e.OldValue);
            }
        }



        public static readonly DependencyProperty TargetRectangle_Left =
        DependencyProperty.Register("TargetRectangleLeft",
              typeof(double),
              typeof(DisplayPort),
              new FrameworkPropertyMetadata(0D));
        public double TargetRectangleLeft
        {
            get { return (double)this.GetValue(DisplayPort.TargetRectangle_Left); }
            set
            {
                this.SetValue(DisplayPort.TargetRectangle_Left, value);
                RaisePropertyChanged();

            }
        }

        public static readonly DependencyProperty TargetRectangle_Top =
        DependencyProperty.Register("TargetRectangleTop",
              typeof(double),
              typeof(DisplayPort),
              new FrameworkPropertyMetadata(0D));
        public double TargetRectangleTop
        {
            get { return (double)this.GetValue(DisplayPort.TargetRectangle_Top); }
            set
            {
                this.SetValue(DisplayPort.TargetRectangle_Top, value);
                RaisePropertyChanged();
            }
        }

        public static readonly DependencyProperty TargetRectangle_Width =
        DependencyProperty.Register("TargetRectangleWidth",
             typeof(double),
             typeof(DisplayPort),
             new FrameworkPropertyMetadata(0D, new PropertyChangedCallback(TargetRectangelWidthPropertyChanged)));
        public double TargetRectangleWidth
        {
            get { return (double)this.GetValue(DisplayPort.TargetRectangle_Width); }
            set { this.SetValue(DisplayPort.TargetRectangle_Width, value); }
        }

        public static readonly DependencyProperty TargetRectangle_Height =
        DependencyProperty.Register("TargetRectangleHeight",
              typeof(double),
              typeof(DisplayPort),
              new FrameworkPropertyMetadata(0D, new PropertyChangedCallback(TargetRectangeHeightlPropertyChanged)));
        public double TargetRectangleHeight
        {
            get { return (double)this.GetValue(DisplayPort.TargetRectangle_Height); }
            set { this.SetValue(DisplayPort.TargetRectangle_Height, value); }
        }


        public static readonly DependencyProperty MoveToX =
        DependencyProperty.Register("MoveXValue",
              typeof(double),
              typeof(DisplayPort),
              new PropertyMetadata(0D));
        public double MoveXValue
        {
            get { return (double)this.GetValue(DisplayPort.MoveToX); }
            set
            {
                this.SetValue(DisplayPort.MoveToX, value);
                RaisePropertyChanged();
            }
        }

        public static readonly DependencyProperty MoveToY =
        DependencyProperty.Register("MoveYValue",
              typeof(double),
              typeof(DisplayPort),
              new PropertyMetadata(0D));
        public double MoveYValue
        {
            get { return (double)this.GetValue(DisplayPort.MoveToY); }
            set
            {
                this.SetValue(DisplayPort.MoveToY, value);
                RaisePropertyChanged();
            }
        }

        private static void StageSupervisorPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                DisplayPort dp = null;

                if (sender is DisplayPort)
                {
                    dp = (DisplayPort)sender;
                }
                if (dp != null)
                {
                    if (e.NewValue is IStageSupervisor)
                    {
                        //IStageSupervisor stagesupervisor = (IStageSupervisor)e.NewValue;
                        //dp.VisionManager = stagesupervisor.VisionManager();
                        //dp.MotionManager = stagesupervisor.MotionManager();
                        //dp.CoordManager = stagesupervisor.CoordinateManager();
                        //dp.AxisXPos = stagesupervisor.MotionManager().GetAxis(EnumAxisConstants.X);
                        //dp.AxisYPos = stagesupervisor.MotionManager().GetAxis(EnumAxisConstants.Y);
                        //dp.AxisZPos = stagesupervisor.MotionManager().GetAxis(EnumAxisConstants.Z);
                        //dp.AxisTPos = stagesupervisor.MotionManager().GetAxis(EnumAxisConstants.C);
                        //dp.AxisPZPos = stagesupervisor.MotionManager().GetAxis(EnumAxisConstants.PZ);
                        dp.StageSupervisor = e.NewValue as IStageSupervisor;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static readonly DependencyProperty StageSupervisorProperty =
       DependencyProperty.Register("StageSupervisor",
             typeof(IStageSupervisor),
             typeof(DisplayPort),
             new FrameworkPropertyMetadata(null, new PropertyChangedCallback(StageSupervisorPropertyChanged)));
        public IStageSupervisor StageSupervisor
        {
            get { return (IStageSupervisor)this.GetValue(DisplayPort.StageSupervisorProperty); }
            set
            {
                this.SetValue(DisplayPort.StageSupervisorProperty, value);
            }
        }

        //private IVisionManager _VisionManager;

        //public IVisionManager VisionManager
        //{
        //    get { return _VisionManager; }
        //    set { _VisionManager = value; }
        //}

        //private IMotionManager _MotionManager;

        //public IMotionManager MotionManager
        //{
        //    get { return _MotionManager; }
        //    set { _MotionManager = value; }
        //}

        //private ICoordinateManager _CoordManager;

        //public ICoordinateManager CoordManager
        //{
        //    get { return _CoordManager; }
        //    set { _CoordManager = value; }
        //}


        //public static readonly DependencyProperty VisionManagerProperty =
        //DependencyProperty.Register("VisionManager",
        //         typeof(IVisionManager),
        //         typeof(DisplayPort), null);
        //public IVisionManager VisionManager
        //{
        //    get { return (IVisionManager)this.GetValue(DisplayPort.VisionManagerProperty); }
        //    set
        //    {
        //        this.SetValue(DisplayPort.VisionManagerProperty, value);
        //    }
        //}


        public static readonly DependencyProperty VisionManagerProperty =
        DependencyProperty.Register("VisionManager",
                 typeof(IVisionManager),
                 typeof(DisplayPort), null);
        public IVisionManager VisionManager
        {
            get { return (IVisionManager)this.GetValue(DisplayPort.VisionManagerProperty); }
            set
            {
                this.SetValue(DisplayPort.VisionManagerProperty, value);
            }
        }


        public static readonly DependencyProperty MotionManagerProperty =
        DependencyProperty.Register("MotionManager",
                 typeof(IMotionManager),
                 typeof(DisplayPort), null);
        public IMotionManager MotionManager
        {
            get { return (IMotionManager)this.GetValue(DisplayPort.MotionManagerProperty); }
            set
            {
                this.SetValue(DisplayPort.MotionManagerProperty, value);
            }
        }


        public static readonly DependencyProperty CoordManagerProperty =
    DependencyProperty.Register("CoordManager"
        , typeof(ICoordinateManager),
        typeof(DisplayPort), null);
        public ICoordinateManager CoordManager
        {
            get { return (ICoordinateManager)this.GetValue(CoordManagerProperty); }
            set { this.SetValue(CoordManagerProperty, value); }
        }


        public static readonly DependencyProperty AxisXPosProperty =
            DependencyProperty.Register("AxisXPos"
            , typeof(ProbeAxisObject),
            typeof(DisplayPort), null);
        public ProbeAxisObject AxisXPos
        {
            get { return (ProbeAxisObject)this.GetValue(AxisXPosProperty); }
            set { this.SetValue(AxisXPosProperty, value); }
        }

        public static readonly DependencyProperty AxisYPosProperty =
        DependencyProperty.Register("AxisYPos"
        , typeof(ProbeAxisObject),
    typeof(DisplayPort), null);
        public ProbeAxisObject AxisYPos
        {
            get { return (ProbeAxisObject)this.GetValue(AxisYPosProperty); }
            set { this.SetValue(AxisYPosProperty, value); }
        }

        public static readonly DependencyProperty AxisZPosProperty =
        DependencyProperty.Register("AxisZPos"
        , typeof(ProbeAxisObject),
    typeof(DisplayPort), null);
        public ProbeAxisObject AxisZPos
        {
            get { return (ProbeAxisObject)this.GetValue(AxisZPosProperty); }
            set { this.SetValue(AxisZPosProperty, value); }
        }

        public static readonly DependencyProperty AxisPZPosProperty =
DependencyProperty.Register("AxisPZPos"
, typeof(ProbeAxisObject),
typeof(DisplayPort), null);
        public ProbeAxisObject AxisPZPos
        {
            get { return (ProbeAxisObject)this.GetValue(AxisPZPosProperty); }
            set { this.SetValue(AxisPZPosProperty, value); }
        }

        public static readonly DependencyProperty AxisTPosPosProperty =
       DependencyProperty.Register("AxisTPos"
       , typeof(ProbeAxisObject),
   typeof(DisplayPort), null);
        public ProbeAxisObject AxisTPos
        {
            get { return (ProbeAxisObject)this.GetValue(AxisTPosPosProperty); }
            set { this.SetValue(AxisTPosPosProperty, value); }
        }
        //private ProbeAxisObject _AxisXPos;
        //public ProbeAxisObject AxisXPos
        //{
        //    get { return _AxisXPos; }
        //    set
        //    {
        //        if (value != _AxisXPos)
        //        {
        //            _AxisXPos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private ProbeAxisObject _AxisYPos;
        //public ProbeAxisObject AxisYPos
        //{
        //    get { return _AxisYPos; }
        //    set
        //    {
        //        if (value != _AxisYPos)
        //        {
        //            _AxisYPos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //private ProbeAxisObject _AxisZPos;
        //public ProbeAxisObject AxisZPos
        //{
        //    get { return _AxisZPos; }
        //    set
        //    {
        //        if (value != _AxisZPos)
        //        {
        //            _AxisZPos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private ProbeAxisObject _AxisPZPos;
        //public ProbeAxisObject AxisPZPos
        //{
        //    get { return _AxisPZPos; }
        //    set
        //    {
        //        if (value != _AxisPZPos)
        //        {
        //            _AxisPZPos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private ProbeAxisObject _AxisTPos;
        //public ProbeAxisObject AxisTPos
        //{
        //    get { return _AxisTPos; }
        //    set
        //    {
        //        if (value != _AxisTPos)
        //        {
        //            _AxisTPos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        #region ==>SetZero

        private bool _MarkerEnable;
        public bool MarkerEnable
        {
            get { return _MarkerEnable; }
            set
            {
                if (value != _MarkerEnable)
                {
                    _MarkerEnable = value;

                    MarkerSetPos.SetCoordinates(AxisXPos.Status.Position.Ref,
                                                AxisYPos.Status.Position.Ref,
                                                AxisZPos.Status.Position.Ref,
                                                AxisTPos.Status.Position.Ref,
                                                AxisPZPos.Status.Position.Ref);

                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _MarkerSetPos = new CatCoordinates();
        public CatCoordinates MarkerSetPos
        {
            get { return _MarkerSetPos; }
            set
            {
                if (value != _MarkerSetPos)
                {
                    _MarkerSetPos = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private double _ZeroPosX = -1;
        //public double ZeroPosX
        //{
        //    get { return _ZeroPosX; }
        //    set
        //    {
        //        if (value != _ZeroPosX)
        //        {
        //            _ZeroPosX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double _ZeroPosY = -1;
        //public double ZeroPosY
        //{
        //    get { return _ZeroPosY; }
        //    set
        //    {
        //        if (value != _ZeroPosY)
        //        {
        //            _ZeroPosY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double _ZeroPosZ = -1;
        //public double ZeroPosZ
        //{
        //    get { return _ZeroPosZ; }
        //    set
        //    {
        //        if (value != _ZeroPosZ)
        //        {
        //            _ZeroPosZ = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        #endregion





        private static void AssignedCameraPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DisplayPort dp = null;
            try
            {
                if (sender is DisplayPort)
                {
                    dp = (DisplayPort)sender;
                }

                if (dp != null)
                {
                    if (dp.AssignedCamera != null)
                    {
                        _AssignedCameraType = dp.AssignedCamera.GetChannelType();
                    }
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public static EnumProberCam _AssignedCameraType;

        public static readonly DependencyProperty AssignedCamearaProperty =
            DependencyProperty.Register("AssignedCamera"
                , typeof(ICamera),
                typeof(DisplayPort),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(AssignedCameraPropertyChanged)));
        public ICamera AssignedCamera
        {
            get { return (ICamera)this.GetValue(AssignedCamearaProperty); }
            set { this.SetValue(AssignedCamearaProperty, value); }
        }

        // Grid Visibility
        public static DependencyProperty GridVisibilityProperty =
        DependencyProperty.Register("GridVisibility",
                 typeof(bool),
                 typeof(DisplayPort), new FrameworkPropertyMetadata(true));
        public bool GridVisibility
        {
            get { return (bool)this.GetValue(DisplayPort.GridVisibilityProperty); }
            set
            {
                this.SetValue(DisplayPort.GridVisibilityProperty, value);
            }
        }



        public new static readonly DependencyProperty IsHitTestVisibleProperty =
            DependencyProperty.Register("IsHitTestVisible", typeof(bool), typeof(DisplayPort), new FrameworkPropertyMetadata(true));
        public new bool IsHitTestVisible
        {
            get { return (bool)this.GetValue(IsHitTestVisibleProperty); }
            set { this.SetValue(IsHitTestVisibleProperty, value); }
        }

        public static readonly DependencyProperty EnalbeClickToMoveProperty =
        DependencyProperty.Register("EnalbeClickToMove", typeof(bool), typeof(DisplayPort), new FrameworkPropertyMetadata(true));
        public bool EnalbeClickToMove
        {
            get { return (bool)this.GetValue(EnalbeClickToMoveProperty); }
            set { this.SetValue(EnalbeClickToMoveProperty, value); }
        }

        public static readonly DependencyProperty OverlayTextProperty =
                DependencyProperty.Register("OverlayText", typeof(string), typeof(DisplayPort), new FrameworkPropertyMetadata(null));
        public string OverlayText
        {
            get { return (string)this.GetValue(OverlayTextProperty); }
            set { this.SetValue(OverlayTextProperty, value); }
        }

        public static readonly DependencyProperty OverlayLabelProperty =
                DependencyProperty.Register("OverlayLabel", typeof(string), typeof(DisplayPort), new FrameworkPropertyMetadata(null));
        public string OverlayLabel
        {
            get { return (string)this.GetValue(OverlayLabelProperty); }
            set { this.SetValue(OverlayLabelProperty, value); }
        }

        public static readonly DependencyProperty OverlayTextActiveProperty =
            DependencyProperty.Register("OverlayTextActive", typeof(bool), typeof(DisplayPort), new FrameworkPropertyMetadata(null));
        public string OverlayTextActive
        {
            get { return (string)this.GetValue(OverlayTextActiveProperty); }
            set { this.SetValue(OverlayTextActiveProperty, value); }
        }

        public static readonly DependencyProperty OverlayLabelActiveProperty =
                DependencyProperty.Register("OverlayLabelActive", typeof(bool), typeof(DisplayPort), new FrameworkPropertyMetadata(null));
        public string OverlayLabelActive
        {
            get { return (string)this.GetValue(OverlayLabelActiveProperty); }
            set { this.SetValue(OverlayLabelActiveProperty, value); }
        }
        private double _OverlayTextVerPos;
        public double OverlayTextVerPos
        {
            get { return _OverlayTextVerPos; }
            set
            {
                if (value != _OverlayTextVerPos)
                {
                    _OverlayTextVerPos = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _OverlayTextHorPos;
        public double OverlayTextHorPos
        {
            get { return _OverlayTextHorPos; }
            set
            {
                if (value != _OverlayTextHorPos)
                {
                    _OverlayTextHorPos = value;
                    RaisePropertyChanged();
                }
            }
        }

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
        public bool InnerLockable { get; set; }
        public List<int> AvoidLockHashCodes { get; set; }


        #endregion

        private WriteableBitmap _WriteableBitmap;
        public WriteableBitmap WriteableBitmap
        {
            get { return _WriteableBitmap; }
            set
            {
                if (value != _WriteableBitmap)
                {
                    _WriteableBitmap = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ImageBuffer _DisplayImage;
        [DataMember]
        public ImageBuffer DisplayImage
        {
            get { return _DisplayImage; }
            set
            {
                //if (value != _DisplayImage)
                //{
                _DisplayImage = value;
                RaisePropertyChanged();
                //}
            }
        }


        public void SetImage(ICamera cam, ImageBuffer img)
        {
            try
            {

                SetCamera(cam);
                if (img != null)
                {
                    DisplayImage = img;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //public void SetImage(ICamera cam, WriteableBitmap wrbmp = null)
        //{
        //    try
        //    {
        //        if (cam != AssignedCamera)
        //        {
        //            SetCamera(cam);
        //        }
        //        if (wrbmp != null)
        //        {
        //            DisplayImage.Source = wrbmp;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw err;
        //    }
        //}



        private async void SetCamera(ICamera cam)
        {
            try
            {
                //this.Dispatcher.Invoke(delegate
                await this.Dispatcher.BeginInvoke((Action)delegate
               {
                   if (cam != AssignedCamera)
                   {
                       AssignedCamera = cam;
                       if ((AssignedCamera.Param.ColorDept.Value * AssignedCamera.Param.Band.Value) == (int)ColorDept.BlackAndWhite)
                       {
                           WriteableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                              (int)AssignedCamera.GetGrabSizeWidth(), (int)AssignedCamera.GetGrabSizeHeight(), 96, 96,
                              System.Windows.Media.PixelFormats.Gray8, null);
                       }
                       else if ((AssignedCamera.Param.ColorDept.Value * AssignedCamera.Param.Band.Value) == (int)ColorDept.Color24)
                       {
                           WriteableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                              (int)AssignedCamera.GetGrabSizeWidth(), (int)AssignedCamera.GetGrabSizeHeight(), 96, 96,
                              System.Windows.Media.PixelFormats.Rgb24, null);
                       }
                   }
                   else if (WriteableBitmap == null)
                   {
                       if ((AssignedCamera.Param.ColorDept.Value * AssignedCamera.Param.Band.Value) == (int)ColorDept.BlackAndWhite)
                       {
                           WriteableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                              (int)AssignedCamera.GetGrabSizeWidth(), (int)AssignedCamera.GetGrabSizeHeight(), 96, 96,
                              System.Windows.Media.PixelFormats.Gray8, null);
                       }
                       else if ((AssignedCamera.Param.ColorDept.Value * AssignedCamera.Param.Band.Value) == 72)
                       {
                           WriteableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                              (int)AssignedCamera.GetGrabSizeWidth(), (int)AssignedCamera.GetGrabSizeHeight(), 96, 96,
                              System.Windows.Media.PixelFormats.Rgb24, null);
                       }
                   }
               });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void HideOSDs()
        {
            try
            {
                //OverlayCanvas.Visibility = System.Windows.Visibility.Hidden;
                OSDCanvas.Visibility = System.Windows.Visibility.Hidden;
                mOverlayVisible = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ShowOSDs()
        {
            try
            {
                OverlayCanvas.Visibility = System.Windows.Visibility.Visible;
                OSDCanvas.Visibility = System.Windows.Visibility.Visible;
                mOverlayVisible = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void HideOverlays()
        {
            try
            {
                OverlayCanvas.Visibility = System.Windows.Visibility.Hidden;
                OSDCanvas.Visibility = System.Windows.Visibility.Hidden;
                mOverlayVisible = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ShowOverlays()
        {
            try
            {
                OverlayCanvas.Visibility = System.Windows.Visibility.Visible;
                OSDCanvas.Visibility = System.Windows.Visibility.Visible;
                mOverlayVisible = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ShowRectangle()
        {

        }
        public void HideRectangle()
        {

        }


        public void AddText(double posx, double posy, string text, SolidColorBrush color)
        {
            try
            {
                BrushConverter bc = new BrushConverter();

                OSDLabels.Add(new Label());

                Canvas.SetTop(OSDLabels[OSDLabels.Count - 1], posy);
                Canvas.SetLeft(OSDLabels[OSDLabels.Count - 1], posx);
                OSDLabels[OSDLabels.Count - 1].Style = (Style)FindResource("OSDLabelStyle");
                OSDLabels[OSDLabels.Count - 1].Foreground = color;
                OSDLabels[OSDLabels.Count - 1].Content = text;

                OverlayCanvas.Children.Add(OSDLabels[OSDLabels.Count - 1]);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DrawROIBox(double posx, double posy, double width, double height)
        {
            try
            {

                if (posx > OSDCanvas.ActualWidth - width / 2)
                {
                    posx = OSDCanvas.ActualWidth - width / 2;
                }
                else if (posx < width / 2)
                {
                    posx = width / 2;
                }


                if (posy > OSDCanvas.ActualHeight - height / 2)
                {
                    posy = OSDCanvas.ActualHeight - height / 2;
                }
                else if (posy < height / 2)
                {
                    posy = height / 2;
                }


                Canvas.SetTop(TargetRectangle, posy - height / 2);
                Canvas.SetLeft(TargetRectangle, posx - width / 2);

                TargetRectangle.Width = width;
                TargetRectangle.Height = height;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void DrawROIBox(double width, double height)
        {
            try
            {
                double posx = 0;
                double posy = 0;

                double viewWidth = 0;
                double viewHeight = 0;

                viewWidth = width * (OSDCanvas.ActualWidth / mGrabSizeX);
                viewHeight = height * (OSDCanvas.ActualHeight / mGrabSizeY);

                if (posx > OSDCanvas.ActualWidth - viewWidth / 2)
                {
                    posx = OSDCanvas.ActualWidth - viewWidth / 2;
                }
                else if (posx < width / 2)
                {
                    posx = width / 2;
                }


                if (posy > OSDCanvas.ActualHeight - viewHeight / 2)
                {
                    posy = OSDCanvas.ActualHeight - viewHeight / 2;
                }
                else if (posy < viewHeight / 2)
                {
                    posy = viewHeight / 2;
                }

                Canvas.SetTop(TargetRectangle, OSDCanvas.ActualHeight / 2 - viewHeight / 2);
                Canvas.SetLeft(TargetRectangle, OSDCanvas.ActualWidth / 2 - viewWidth / 2);

                TargetRectangle.Width = viewWidth;
                TargetRectangle.Height = viewHeight;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void DrawBox(double posx, double posy, double width, double height, double angle)
        {
            try
            {
                Rectangle rect = new Rectangle();
                Line horLine = new Line();
                Line verLine = new Line();

                posx = posx * (OSDCanvas.ActualWidth / mGrabSizeX);
                posy = posy * (OSDCanvas.ActualHeight / mGrabSizeY);
                width = width * (OSDCanvas.ActualWidth / mGrabSizeX);
                height = height * (OSDCanvas.ActualHeight / mGrabSizeY);

                rect.Stroke = Brushes.Red;
                rect.StrokeThickness = 1;
                rect.Width = Math.Abs(width);
                rect.Height = Math.Abs(height);

                Canvas.SetTop(rect, posy - rect.Height / 2);
                Canvas.SetLeft(rect, posx - rect.Width / 2);

                rect.RenderTransform = new RotateTransform(angle * -1.0
                                                        , rect.Width / 2
                                                        , rect.Height / 2);

                horLine.Stroke = Brushes.Red;
                verLine.Stroke = Brushes.Red;
                horLine.StrokeThickness = 1;
                verLine.StrokeThickness = 1;

                horLine.X1 = posx - 6;
                horLine.X2 = posx + 6;
                horLine.Y1 = posy;
                horLine.Y2 = posy;
                horLine.RenderTransform = new RotateTransform(angle * -1.0, posx, posy);

                verLine.X1 = posx;
                verLine.X2 = posx;
                verLine.Y1 = posy - 6;
                verLine.Y2 = posy + 6;
                verLine.RenderTransform = new RotateTransform(angle * -1.0, posx, posy);


                rect.Visibility = System.Windows.Visibility.Visible;

                OverlayCanvas.Children.Add(rect);
                OverlayCanvas.Children.Add(horLine);
                OverlayCanvas.Children.Add(verLine);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ClearOverlays()
        {
            OverlayCanvas.Children.Clear();
        }

        private void OSDCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (e.HeightChanged == true)
                {
                    //StandardOverlayCanvaseWidth = OSDCanvas.ActualWidth;
                    //StandardOverlayCanvaseHeight = OSDCanvas.ActualHeight;

                    //Canvas.SetTop(POSXLabel, OSDCanvas.ActualHeight - 135);
                    //Canvas.SetTop(POSYLabel, OSDCanvas.ActualHeight - 120);

                    Canvas.SetTop(TargetRectangle, OSDCanvas.ActualHeight / 2 - 32);
                    Canvas.SetLeft(TargetRectangle, OSDCanvas.ActualWidth / 2 - 32);

                    TargetRectangle.Width = 128;
                    TargetRectangle.Height = 128;

                    HorCrossLine.X1 = 0;
                    HorCrossLine.Y1 = OSDCanvas.ActualHeight / 2;
                    HorCrossLine.X2 = OSDCanvas.ActualWidth;
                    HorCrossLine.Y2 = OSDCanvas.ActualHeight / 2;

                    VertCrossLine.X1 = OSDCanvas.ActualWidth / 2;
                    VertCrossLine.Y1 = 0;
                    VertCrossLine.X2 = OSDCanvas.ActualWidth / 2;
                    VertCrossLine.Y2 = OSDCanvas.ActualHeight;

                    OSDCanvasHalfWidth = OSDCanvas.ActualWidth / 2;
                    OSDCanvasHalfHeight = OSDCanvas.ActualHeight / 2;

                    //OverlayLabelX.PointFromScreen(new Point(OSDCanvas.ActualWidth, OSDCanvas.ActualHeight / 2));
                    //OverlayLabelY.PointFromScreen(new Point(OSDCanvas.ActualWidth / 2, OSDCanvas.ActualHeight));
                    OverlayTextVerPos = OSDCanvas.ActualHeight / 4;

                    MarkButtonSize = OSDCanvas.ActualWidth / MarkButtonSizeRatio;

                    // Left, Top, Right, Bottom

                    MarkerButtonMargin = new Thickness(0, (OSDCanvas.ActualWidth / 1.483), -(OSDCanvas.ActualWidth / 1.01714), 0);

                    //if(mainRender.RenderLayerData != null)
                    //    mainRender.RenderLayerData.ChangedLayerSize(new Size(OSDCanvas.ActualWidth, OSDCanvas.ActualHeight));

                    //if (mainRender.RenderLayerData != null)
                    //{
                    //    mainRender.RenderLayerData.UpdateRenderLayer(new Size(OSDCanvas.ActualWidth, OSDCanvas.ActualHeight));
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void POSYLabel_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateLayout();

                ShowOSDs();

                Canvas.SetTop(POSXLabel, OSDCanvas.ActualHeight - 35);
                Canvas.SetTop(POSYLabel, OSDCanvas.ActualHeight - 20);

                Canvas.SetTop(TargetRectangle, OSDCanvas.ActualHeight / 2 - 32);
                Canvas.SetLeft(TargetRectangle, OSDCanvas.ActualWidth / 2 - 32);

                TargetRectangle.Width = 128;
                TargetRectangle.Height = 128;

                HorCrossLine.X1 = 0;
                HorCrossLine.Y1 = OSDCanvas.ActualHeight / 2;
                HorCrossLine.X2 = OSDCanvas.ActualWidth;
                HorCrossLine.Y2 = OSDCanvas.ActualHeight / 2;

                VertCrossLine.X1 = OSDCanvas.ActualWidth / 2;
                VertCrossLine.Y1 = 0;
                VertCrossLine.X2 = OSDCanvas.ActualWidth / 2;
                VertCrossLine.Y2 = OSDCanvas.ActualHeight;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public Point GetCornerInfo()
        {
            return new Point(VertCorner.X1, HorCorner.Y1);
        }

        private void ViewCrossLine()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    VertCrossLine.Visibility = Visibility.Visible;
                    HorCrossLine.Visibility = Visibility.Visible;
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void HiddenCrossLine()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    VertCrossLine.Visibility = Visibility.Hidden;
                    HorCrossLine.Visibility = Visibility.Hidden;
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public Line horizontalLine;
        public Line verticalLine;


        private void DPGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!EnalbeClickToMove)
                    return;

                // ProberSystem이 Widget모드인 경우 이동하지 않는다. 
                if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe" &&
                    this.ViewModelManager().MainWindowWidget.DataContext != null)
                {
                    return;
                }

                double posX = 0;
                double posY = 0;

                horizontalLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                horizontalLine.X1 = e.GetPosition(this.OverlayCanvas).X - 20;
                horizontalLine.X2 = e.GetPosition(this.OverlayCanvas).X + 20;
                horizontalLine.Y1 = e.GetPosition(this.OverlayCanvas).Y;
                horizontalLine.Y2 = e.GetPosition(this.OverlayCanvas).Y;
                horizontalLine.HorizontalAlignment = HorizontalAlignment.Left;
                horizontalLine.VerticalAlignment = VerticalAlignment.Center;
                horizontalLine.StrokeThickness = 2;


                verticalLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                verticalLine.X1 = e.GetPosition(this.OverlayCanvas).X;
                verticalLine.X2 = e.GetPosition(this.OverlayCanvas).X;
                verticalLine.Y1 = e.GetPosition(this.OverlayCanvas).Y - 20;
                verticalLine.Y2 = e.GetPosition(this.OverlayCanvas).Y + 20;
                verticalLine.HorizontalAlignment = HorizontalAlignment.Left;
                verticalLine.VerticalAlignment = VerticalAlignment.Center;
                verticalLine.StrokeThickness = 2;


                OverlayCanvas.Children.Remove(horizontalLine);
                OverlayCanvas.Children.Remove(verticalLine);

                OverlayCanvas.Children.Add(horizontalLine);
                OverlayCanvas.Children.Add(verticalLine);

                posX = e.GetPosition(this.OverlayCanvas).X - (OverlayCanvas.ActualWidth / 2);
                posY = (e.GetPosition(this.OverlayCanvas).Y - (OverlayCanvas.ActualHeight / 2));

                if (StageSupervisor == null)
                    StageSupervisor = this.StageSupervisor();
                if (AssignedCamera == null | StageSupervisor == null)
                    return;

                if (AssignedCamera != null && EnalbeClickToMove && !StageSupervisor.StageMoveFlag_Display)
                {
                    if (AssignedCamera.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM || AssignedCamera.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    {
                        posX = -1 * posX * AssignedCamera.GetRatioX() * (AssignedCamera.GetGrabSizeWidth() / this.OverlayCanvas.ActualWidth);
                        posY = posY * AssignedCamera.GetRatioY() * (AssignedCamera.GetGrabSizeHeight() / this.OverlayCanvas.ActualHeight);
                    }
                    else if (AssignedCamera.GetChannelType() == EnumProberCam.PIN_HIGH_CAM || AssignedCamera.GetChannelType() == EnumProberCam.PIN_LOW_CAM)
                    {
                        posX = posX * AssignedCamera.GetRatioX() * (AssignedCamera.GetGrabSizeWidth() / this.OverlayCanvas.ActualWidth);
                        posY = -1 * posY * AssignedCamera.GetRatioY() * (AssignedCamera.GetGrabSizeHeight() / this.OverlayCanvas.ActualHeight);

                    }
                    else
                    {
                        posX = -1 * posX * AssignedCamera.GetRatioX() * (AssignedCamera.GetGrabSizeWidth() / this.OverlayCanvas.ActualWidth);
                        posY = posY * AssignedCamera.GetRatioY() * (AssignedCamera.GetGrabSizeHeight() / this.OverlayCanvas.ActualHeight);
                    }
                }

                MoveXValue = posX;
                MoveYValue = posY;

                StageSupervisor.MoveTargetPosX = posX;
                StageSupervisor.MoveTargetPosY = posY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DPGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!EnalbeClickToMove)
                    return;

                OverlayCanvas.Children.Remove(horizontalLine);
                OverlayCanvas.Children.Remove(verticalLine);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void BoundaryLineHidden()
        {
            try
            {
                LeftUpperXLine.Visibility = Visibility.Hidden;
                LeftUpperYLine.Visibility = Visibility.Hidden;
                RightUpperXLine.Visibility = Visibility.Hidden;
                RightUpperYLine.Visibility = Visibility.Hidden;
                LeftBottomXLine.Visibility = Visibility.Hidden;
                LeftBottomYLine.Visibility = Visibility.Hidden;
                RightBottomXLine.Visibility = Visibility.Hidden;
                RightBottomYLine.Visibility = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void BoundaryLineVisiable()
        {
            try
            {
                LeftUpperXLine.Visibility = Visibility.Visible;
                LeftUpperYLine.Visibility = Visibility.Visible;
                RightUpperXLine.Visibility = Visibility.Visible;
                RightUpperYLine.Visibility = Visibility.Visible;
                LeftBottomXLine.Visibility = Visibility.Visible;
                LeftBottomYLine.Visibility = Visibility.Visible;
                RightBottomXLine.Visibility = Visibility.Visible;
                RightBottomYLine.Visibility = Visibility.Visible;


                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    HiddenCrossLine();
                    double height = OSDCanvas.ActualHeight / 2;
                    double width = OSDCanvas.ActualWidth / 2;

                    double targetrectwidth = RectWidth;
                    double targetrectheight = RectHeight;

                    double lenght = 70;

                    LeftUpperXLine.X1 = width + (targetrectwidth / 2);
                    LeftUpperXLine.X2 = LeftUpperXLine.X1 + lenght;
                    LeftUpperXLine.Y1 = height + (targetrectheight / 2);
                    LeftUpperXLine.Y2 = LeftUpperXLine.Y1;

                    LeftUpperYLine.X1 = width + (targetrectwidth / 2);
                    LeftUpperYLine.X2 = LeftUpperYLine.X1;
                    LeftUpperYLine.Y1 = height + (targetrectheight / 2);
                    LeftUpperYLine.Y2 = LeftUpperYLine.Y1 + lenght;

                    RightUpperXLine.X1 = width - (targetrectwidth / 2);
                    RightUpperXLine.X2 = RightUpperXLine.X1 - lenght;
                    RightUpperXLine.Y1 = height + (targetrectheight / 2);
                    RightUpperXLine.Y2 = RightUpperXLine.Y1;

                    RightUpperYLine.X1 = width - (targetrectwidth / 2);
                    RightUpperYLine.X2 = RightUpperYLine.X1;
                    RightUpperYLine.Y1 = height + (targetrectheight / 2);
                    RightUpperYLine.Y2 = RightUpperYLine.Y1 + lenght;

                    LeftBottomXLine.X1 = width + (targetrectwidth / 2);
                    LeftBottomXLine.X2 = LeftBottomXLine.X1 + lenght;
                    LeftBottomXLine.Y1 = height - (targetrectheight / 2);
                    LeftBottomXLine.Y2 = LeftBottomXLine.Y1;

                    LeftBottomYLine.X1 = width + (targetrectwidth / 2);
                    LeftBottomYLine.X2 = LeftBottomYLine.X1;
                    LeftBottomYLine.Y1 = height - (targetrectheight / 2);
                    LeftBottomYLine.Y2 = LeftBottomYLine.Y1 - lenght;

                    RightBottomXLine.X1 = width - (targetrectwidth / 2);
                    RightBottomXLine.X2 = RightBottomXLine.X1 - lenght;
                    RightBottomXLine.Y1 = height - (targetrectheight / 2);
                    RightBottomXLine.Y2 = RightBottomXLine.Y1;

                    RightBottomYLine.X1 = width - (targetrectwidth / 2);
                    RightBottomYLine.X2 = RightBottomYLine.X1;

                    RightBottomYLine.Y1 = height - (targetrectheight / 2);
                    RightBottomYLine.Y2 = RightBottomYLine.Y1 - lenght;

                    LeftUpperXLine.Visibility = Visibility.Visible;
                    LeftUpperYLine.Visibility = Visibility.Visible;
                    RightUpperXLine.Visibility = Visibility.Visible;
                    RightUpperYLine.Visibility = Visibility.Visible;
                    LeftBottomXLine.Visibility = Visibility.Visible;
                    LeftBottomYLine.Visibility = Visibility.Visible;
                    RightBottomXLine.Visibility = Visibility.Visible;
                    RightBottomYLine.Visibility = Visibility.Visible;

                    TargetRectangleLeft = LeftUpperXLine.X1 * (AssignedCamera.GetGrabSizeWidth() / this.OverlayCanvas.ActualWidth);
                    RectLeft = LeftUpperXLine.X1;
                    TargetRectangleTop = LeftUpperXLine.Y1 * (AssignedCamera.GetGrabSizeHeight() / this.OverlayCanvas.ActualHeight);
                    RectTop = LeftUpperXLine.Y1;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ChangeOSDCanvasSize()
        {
            try
            {
                switch (UseUserControlFunc)
                {
                    case UserControlFucEnum.DIELEFTCORNER:
                        LeftBottemCornerLineVisiable();
                        break;
                    case UserControlFucEnum.DIERIGHTCORNER:
                        RightTopCornerLineVisiable();
                        break;
                    case UserControlFucEnum.DIEBOUNDARY:
                        BoundaryLineVisiable();
                        break;
                }

                OverlayTextVerPos = OSDCanvas.ActualHeight / 4;

                OSDLabelStyleFontSize = Convert.ToInt32(OSDCanvas.ActualHeight / 63);
                OSDLabelStyleFontSize = Convert.ToInt32(OSDCanvas.ActualHeight / 63);
                OverlayFontSize = Convert.ToInt32(OSDCanvas.ActualHeight / 34);
                if (OverlayFontSize < 10)
                    OverlayFontSize = 12;

                ActualWidth = OSDCanvas.ActualWidth;
                ActualHeight = OSDCanvas.ActualHeight;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LeftBottemCornerLineVisiable()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    HiddenCrossLine();

                    double lenght = OSDCanvas.ActualWidth / 5;

                    LeftUpperXLine.X1 = OSDCanvasHalfWidth;

                    if (this.StageSupervisor.WaferObject.DispHorFlip == DispFlipEnum.FLIP)
                    {
                        LeftUpperXLine.X2 = OSDCanvasHalfWidth - lenght;
                    }
                    else
                    {
                        LeftUpperXLine.X2 = OSDCanvasHalfWidth + lenght;
                    }

                    LeftUpperXLine.Y1 = OSDCanvasHalfHeight;
                    LeftUpperXLine.Y2 = OSDCanvasHalfHeight;

                    LeftUpperYLine.X1 = OSDCanvasHalfWidth;
                    LeftUpperYLine.X2 = OSDCanvasHalfWidth;
                    LeftUpperYLine.Y1 = OSDCanvasHalfHeight;

                    if (this.StageSupervisor.WaferObject.DispVerFlip == DispFlipEnum.FLIP)
                    {
                        LeftUpperYLine.Y2 = OSDCanvasHalfHeight + lenght;
                    }
                    else
                    {
                        LeftUpperYLine.Y2 = OSDCanvasHalfHeight - lenght;
                    }

                    LeftUpperXLine.Visibility = Visibility.Visible;
                    LeftUpperYLine.Visibility = Visibility.Visible;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void RightTopCornerLineVisiable()
        {
            try
            {

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    HiddenCrossLine();

                    double lenght = OSDCanvas.ActualWidth / 5;

                    RightBottomXLine.X1 = OSDCanvasHalfWidth;

                    if (this.StageSupervisor.WaferObject.DispHorFlip == DispFlipEnum.FLIP)
                    {
                        RightBottomXLine.X2 = OSDCanvasHalfWidth + lenght;
                    }
                    else
                    {
                        RightBottomXLine.X2 = OSDCanvasHalfWidth - lenght;
                    }

                    RightBottomXLine.Y1 = OSDCanvasHalfHeight;
                    RightBottomXLine.Y2 = OSDCanvasHalfHeight;

                    RightBottomYLine.X1 = OSDCanvasHalfWidth;
                    RightBottomYLine.X2 = OSDCanvasHalfWidth;
                    RightBottomYLine.Y1 = OSDCanvasHalfHeight;

                    if (this.StageSupervisor.WaferObject.DispVerFlip == DispFlipEnum.FLIP)
                    {
                        RightBottomYLine.Y2 = OSDCanvasHalfHeight - lenght;
                    }
                    else
                    {
                        RightBottomYLine.Y2 = OSDCanvasHalfHeight + lenght;
                    }


                    RightBottomXLine.Visibility = Visibility.Visible;
                    RightBottomYLine.Visibility = Visibility.Visible;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsTargetRectModify = false;

        private void SetOverlayRect()
        {
            try
            {
                if (AssignedCamera != null)
                {
                    RectLeft = RectLeft * (OSDCanvas.ActualWidth / AssignedCamera.GetGrabSizeWidth());
                    RectTop = RectTop * (OSDCanvas.ActualHeight / AssignedCamera.GetGrabSizeHeight());

                    RectWidth = RectWidth * (OSDCanvas.ActualWidth / AssignedCamera.GetGrabSizeWidth());
                    RectHeight = RectHeight * (OSDCanvas.ActualHeight / AssignedCamera.GetGrabSizeHeight());
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ChangeTargetRectWidth(double preval)
        {
            try
            {

                if (AssignedCamera != null)
                {
                    //if (TargetRectangleWidth > 0 && this.OverlayCanvas.ActualWidth != 0 && preval > 0)
                    if (TargetRectangleWidth > 0 && this.OverlayCanvas.ActualWidth != 0)
                    {
                        if (((this.OverlayCanvas.ActualWidth * TargetRectangleWidth) / StandardOverlayCanvaseWidth) > OverlayCanvas.ActualWidth)
                        {
                            TargetRectangleWidth = preval;
                            return;
                        }

                        //TargetRectangleLeft = AssignedCamera.GetGrabSizeWidth() / 2 - (TargetRectangleWidth / 2);
                        TargetRectangleLeft = (this.OverlayCanvas.ActualWidth / 2) -
                            (((this.OverlayCanvas.ActualWidth * TargetRectangleWidth) / StandardOverlayCanvaseWidth) / 2);

                        if (TargetRectangleLeft < 0 || TargetRectangleLeft >= OSDCanvasHalfWidth)
                        {
                            TargetRectangleLeft = OSDCanvasHalfWidth - (preval / 2);
                            TargetRectangleWidth = preval;
                        }
                        else if (TargetRectangleLeft == 0)
                        {
                            TargetRectangleLeft = 0;
                            TargetRectangleWidth = TargetRectangleWidth;
                            return;
                        }

                        RectWidth = ((this.OverlayCanvas.ActualWidth * TargetRectangleWidth) / StandardOverlayCanvaseWidth);
                        RectLeft = (this.OverlayCanvas.ActualWidth / 2) - (RectWidth / 2);
                    }
                    else
                    {
                        if (TargetRectangleWidth <= 0)
                            TargetRectangleWidth = 0;
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ChangeTargetRectHeight(double preval)
        {
            try
            {
                if (AssignedCamera != null)
                {
                    if (TargetRectangleHeight != 0 && OverlayCanvas.ActualHeight != 0)
                    {
                        if (((this.OverlayCanvas.ActualHeight * TargetRectangleHeight) / StandardOverlayCanvaseHeight) > OverlayCanvas.ActualHeight)
                        {
                            TargetRectangleHeight = preval;
                            return;
                        }

                        //TargetRectangleTop = AssignedCamera.GetGrabSizeHeight() / 2 - (TargetRectangleHeight / 2);
                        TargetRectangleTop = (this.OverlayCanvas.ActualHeight / 2) -
                            (((this.OverlayCanvas.ActualHeight * TargetRectangleHeight) / StandardOverlayCanvaseHeight) / 2);
                        //if (TargetRectangleTop < 0 || TargetRectangleTop >= OSDCanvasHalfHeight)
                        //{
                        //    TargetRectangleTop = 0;
                        //    TargetRectangleHeight = preval;
                        //}

                        if (TargetRectangleTop < 0 || TargetRectangleTop >= OSDCanvasHalfHeight)
                        {
                            TargetRectangleTop = OSDCanvasHalfHeight - (preval / 2);
                            TargetRectangleHeight = preval;
                        }
                        else if (TargetRectangleTop == 0)
                        {
                            TargetRectangleTop = 0;
                            TargetRectangleHeight = TargetRectangleHeight;
                            return;
                        }

                        RectHeight = (((this.OverlayCanvas.ActualHeight * TargetRectangleHeight) / StandardOverlayCanvaseHeight));
                        RectTop = (this.OverlayCanvas.ActualHeight / 2) - (RectHeight / 2);
                    }
                    else
                    {
                        if (TargetRectangleHeight <= 0)
                            TargetRectangleHeight = 0;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public RegisteImageBufferParam GetPatternRectInfo()
        {
            RegisteImageBufferParam ret = new RegisteImageBufferParam();
            try
            {
                if (AssignedCamera == null)
                    return ret;

                double width = (((AssignedCamera.GetGrabSizeWidth() * TargetRectangleWidth) / StandardOverlayCanvaseWidth));
                double height = (((AssignedCamera.GetGrabSizeHeight() * TargetRectangleHeight) / StandardOverlayCanvaseHeight));
                double offsetx = (AssignedCamera.GetGrabSizeWidth() / 2) - width / 2;
                double offsety = (AssignedCamera.GetGrabSizeHeight() / 2) - height / 2;

                ret.Width = (int)width;
                ret.Height = (int)height;
                ret.LocationX = (int)offsetx;
                ret.LocationY = (int)offsety;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public double ConvertDisplayWidth(double sizex, double imgwidth)
        {
            try
            {
                double canvaswidth = 890;
                if (this.OverlayCanvas.ActualWidth != 0)
                    canvaswidth = this.OverlayCanvas.ActualWidth;
                double width = (canvaswidth * sizex) / imgwidth;
                return width;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public double ConvertDisplayHeight(double sizey, double imgheight)
        {
            try
            {
                double canvasheight = 890;
                if (this.OverlayCanvas.ActualWidth != 0)
                    canvasheight = this.OverlayCanvas.ActualWidth;
                double height = (canvasheight * sizey) / imgheight;
                return height;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private void ucwindow_Loaded(object sender, RoutedEventArgs e)
        {
            MaskingLevel = 999;
            // 셀 재접속 또는 선택 셀 변경시 Vision 화면에서 머신 좌표 값 업데이트 안됨.
            //if(AxisXPos == null)
            AxisXPos = MotionManager.GetAxis(EnumAxisConstants.X);
            //if (AxisYPos == null)
            AxisYPos = MotionManager.GetAxis(EnumAxisConstants.Y);
            //if (AxisZPos == null)
            AxisZPos = MotionManager.GetAxis(EnumAxisConstants.Z);
            //if (AxisTPos == null)
            AxisTPos = MotionManager.GetAxis(EnumAxisConstants.C);
            //if (AxisPZPos == null)
            AxisPZPos = MotionManager.GetAxis(EnumAxisConstants.PZ);
        }
    }

    public class WirteableBitmapConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] != null & values[1] != null)
                {
                    ImageBuffer image = values[0] as ImageBuffer;
                    WriteableBitmap writeableBitmap = values[1] as WriteableBitmap;
                    if (image != null)
                    {
                        //WriteableBitmap writeableBitmap = null;
                        if (image.Buffer?.Count() != 0 && image != null && image.Buffer != new byte[0]
                                     && image.SizeX != 0 && image.SizeY != 0)
                        {
                            if (image.Band == 1)
                            {
                                return WriteableBitmapFromArray(image.Buffer, image.SizeX, image.SizeY, writeableBitmap);
                            }
                            else if (image.Band == 3)
                            {
                                return WriteableBitmapFromColoredArray(image.Buffer, image.SizeX, image.SizeY, writeableBitmap);
                            }
                            else
                                return null;
                        }
                        else
                            return null;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ImageBuffer image = value as ImageBuffer;
                if (image != null)
                {
                    WriteableBitmap writeableBitmap = null;
                    if (image.Buffer.Count() != 0 && image != null && image.Buffer != new byte[0]
                                 && image.SizeX != 0 && image.SizeY != 0)
                    {
                        if ((image.Band * image.ColorDept) == (int)ColorDept.BlackAndWhite)
                        {
                            return WriteableBitmapFromArray(image.Buffer, image.SizeX, image.SizeY, writeableBitmap);
                        }
                        else if ((image.Band * image.ColorDept) == (int)ColorDept.Color24)
                        {
                            return WriteableBitmapFromColoredArray(image.Buffer, image.SizeX, image.SizeY, writeableBitmap);
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


        /// <summary>
        /// BlackAndWhite Image 
        /// </summary>
        /// <param name="imgarray"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="wrbitmap"></param>
        /// <returns></returns>
        public static System.Windows.Media.Imaging.WriteableBitmap WriteableBitmapFromArray(byte[] imgarray,
            int width, int height, System.Windows.Media.Imaging.WriteableBitmap wrbitmap)
        {
            try
            {
                System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Gray8;

                int rawStride = (width * pf.BitsPerPixel + 7) / 8;

                System.Windows.Int32Rect anImageRectangle = new System.Windows.Int32Rect(0, 0, width, height);

                if (wrbitmap == null || wrbitmap.Format != pf
                   || width != wrbitmap.Width || height != wrbitmap.Height)
                {
                    wrbitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                                width, height, 96, 96,
                                pf, null);
                }

                if (imgarray.Length == (wrbitmap.Width * wrbitmap.Height))
                    wrbitmap.WritePixels(anImageRectangle, imgarray, rawStride, 0);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("BitMapSourceFromRawImage(): Exception caught.\n - Error message: {0}", err.Message));
                LoggerManager.Exception(err);
                throw err;
            }
            return wrbitmap;
        }


        /// <summary>
        /// Color Image
        /// </summary>
        /// <param name="imgarray"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="wrbitmap"></param>
        /// <returns></returns>
        public static System.Windows.Media.Imaging.WriteableBitmap WriteableBitmapFromColoredArray(byte[] imgarray, int width, int height,
                                        System.Windows.Media.Imaging.WriteableBitmap wrbitmap)
        {
            try
            {

                System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Rgb24;

                int rawStride = width * ((pf.BitsPerPixel + 7) / 8);
                rawStride = width * ((pf.BitsPerPixel) / 8);

                System.Windows.Int32Rect anImageRectangle = new System.Windows.Int32Rect(0, 0, width, height);


                if (wrbitmap == null || wrbitmap.Format != pf)
                {
                    wrbitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                                width, height, 96, 96,
                                pf, null);
                }


                wrbitmap.WritePixels(anImageRectangle, imgarray, rawStride, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return wrbitmap;
        }


    }


}

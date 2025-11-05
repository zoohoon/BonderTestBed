using CylType;
using Focusing;
using LogModule;
using NotifyEventModule;
using MetroDialogInterfaces;
using Pranas;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ControlClass.ViewModel;
using ProberInterfaces.Enum;
using ProberInterfaces.Event;
using ProberInterfaces.Loader.RemoteDataDescription;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using SerializerUtil;
using SoakingParameters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using UcDisplayPort;
using VirtualKeyboardControl;

namespace ChuckPlanarityViewModel
{
    //public static class ExtensionMethods
    //{
    //    private static Action EmptyDelegate = delegate () { };

    //    public static void Refresh(this UIElement uiElement)

    //    {
    //        uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
    //    }
    //}

    public class ChuckPlanarityVM : IChuckPlanarityVM, ISetUpState
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("060f91cf-30ff-4d81-9025-d434822d384a");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        private IFocusing _FocusingModule;
        public IFocusing FocusingModule
        {
            get
            {
                if (_FocusingModule == null)
                    _FocusingModule = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());

                return _FocusingModule;
            }
        }

        private FocusParameter FocusingParam { get; set; }

        private double _FocusingRange;
        public double FocusingRange
        {
            get { return _FocusingRange; }
            set
            {
                if (value != _FocusingRange)
                {
                    _FocusingRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                if (value != _CurCam)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 사용자가 변경한 Z값을 저장 및 사용하기 위한 변수
        /// </summary>
        private double _UserZHeight;
        public double UserZHeight
        {
            get { return _UserZHeight; }
            set
            {
                if (value != _UserZHeight)
                {
                    _UserZHeight = value;
                    RaisePropertyChanged();
                }
            }
        }


        public IPnpManager PnpManager
        {
            get { return this.PnPManager(); }
            set { }
        }

        private ObservableCollection<ChuckPos> _ChuckPosList;
        public ObservableCollection<ChuckPos> ChuckPosList
        {
            get { return _ChuckPosList; }
            set
            {
                if (value != _ChuckPosList)
                {
                    _ChuckPosList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ChuckPos _CurrentChuckPos = null;
        public ChuckPos CurrentChuckPos
        {
            get { return _CurrentChuckPos; }
            set
            {
                if (value != _CurrentChuckPos)
                {
                    _CurrentChuckPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private EnumChuckPosition _CurrentChuckPosEnum;
        //public EnumChuckPosition CurrentChuckPosEnum
        //{
        //    get { return _CurrentChuckPosEnum; }
        //    set
        //    {
        //        if (value != _CurrentChuckPosEnum)
        //        {
        //            _CurrentChuckPosEnum = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private double _MinHeight;
        public double MinHeight
        {
            get { return _MinHeight; }
            set
            {
                if (value != _MinHeight)
                {
                    _MinHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MaxHeight;
        public double MaxHeight
        {
            get { return _MaxHeight; }
            set
            {
                if (value != _MaxHeight)
                {
                    _MaxHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DiffHeight;
        public double DiffHeight
        {
            get { return _DiffHeight; }
            set
            {
                if (value != _DiffHeight)
                {
                    _DiffHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AvgHeight;
        public double AvgHeight
        {
            get { return _AvgHeight; }
            set
            {
                if (value != _AvgHeight)
                {
                    _AvgHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ChuckEndPointMargin;
        public double ChuckEndPointMargin
        {
            get { return _ChuckEndPointMargin; }
            set
            {
                if (value != _ChuckEndPointMargin)
                {
                    _ChuckEndPointMargin = value;

                    RaisePropertyChanged();
                }
            }
        }

        //private double _SpecHeight;
        //public double SpecHeight
        //{
        //    get { return _SpecHeight; }
        //    set
        //    {
        //        if (value != _SpecHeight)
        //        {
        //            _SpecHeight = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private object _MainViewTarget;
        public object MainViewTarget
        {
            get { return _MainViewTarget; }
            set
            {
                if (value != _MainViewTarget)
                {
                    _MainViewTarget = value;
                    if (_MainViewTarget == ZoomObject)
                        MainViewZoomVisible = Visibility.Visible;
                    else
                        MainViewZoomVisible = Visibility.Hidden;

                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MainViewZoomVisible;
        public Visibility MainViewZoomVisible
        {
            get { return _MainViewZoomVisible; }
            set
            {
                if (value != _MainViewZoomVisible)
                {
                    _MainViewZoomVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IZoomObject _ZoomObject;

        public IZoomObject ZoomObject
        {
            get { return _ZoomObject; }
            set
            {
                if (value != _ZoomObject)
                {
                    _ZoomObject = value;
                    RaisePropertyChanged();
                }
            }
        }


        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public IViewModelManager ViewModelManager { get; set; }


        private IStageSupervisor _StageSupervisor;
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set
            {
                if (value != _StageSupervisor)
                {
                    _StageSupervisor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinAlignDevParameters PinAlignParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);
        private PinAlignResultes AlignResult => (this.PinAligner().PinAlignInfo as PinAlignInfo)?.AlignResult;
        private SoakingSysParameter _SoakingSysParam_Clone;
        public SoakingSysParameter SoakingSysParam_Clone
        {
            get { return _SoakingSysParam_Clone; }
            set { _SoakingSysParam_Clone = value; }
        }
        private IParam _SoakingSysParam_IParam;
        public IParam SoakingSysParam_IParam
        {
            get { return _SoakingSysParam_IParam; }
            set
            {
                if (value != _SoakingSysParam_IParam)
                {
                    _SoakingSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStageMove _StageModuleState;
        public IStageMove StageModuleState
        {
            get { return _StageModuleState; }
            set { _StageModuleState = value; }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public EventCodeEnum InitModule()
        {
            try
            {
                ViewModelManager = this.ViewModelManager();
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                StageSupervisor = this.StageSupervisor();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        //private void MakeChuckPos()
        //{
        //    try
        //    {
        //        double margin = 5000;

        //        double edgepos = 0.0;

        //        //double tempzpos = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;
        //        double tempzpos = 0;

        //        edgepos = ((ChuckSize / 2) / Math.Sqrt(2));

        //        ChuckPos leftup = new ChuckPos();
        //        ChuckPos up = new ChuckPos();
        //        ChuckPos rightup = new ChuckPos();
        //        ChuckPos left = new ChuckPos();
        //        ChuckPos center = new ChuckPos();
        //        ChuckPos right = new ChuckPos();
        //        ChuckPos leftdown = new ChuckPos();
        //        ChuckPos down = new ChuckPos();
        //        ChuckPos rightdown = new ChuckPos();

        //        leftup.ChuckPosEnum = EnumChuckPosition.LEFTUP;
        //        leftup.XPos = -edgepos + margin;
        //        leftup.YPos = edgepos - margin;
        //        leftup.ZPos = tempzpos;

        //        up.ChuckPosEnum = EnumChuckPosition.UP;
        //        up.XPos = 0;
        //        up.YPos = edgepos - margin;
        //        up.ZPos = tempzpos;

        //        rightup.ChuckPosEnum = EnumChuckPosition.RIGHTUP;
        //        rightup.XPos = edgepos - margin;
        //        rightup.YPos = edgepos - margin;
        //        rightup.ZPos = tempzpos;

        //        left.ChuckPosEnum = EnumChuckPosition.LEFT;
        //        left.XPos = -edgepos + margin;
        //        left.YPos = 0;
        //        left.ZPos = tempzpos;

        //        center.ChuckPosEnum = EnumChuckPosition.CENTER;
        //        center.XPos = 0;
        //        center.YPos = 0;
        //        center.ZPos = tempzpos;

        //        right.ChuckPosEnum = EnumChuckPosition.RIGHT;
        //        right.XPos = edgepos - margin;
        //        right.YPos = 0;
        //        right.ZPos = tempzpos;

        //        leftdown.ChuckPosEnum = EnumChuckPosition.LEFTDOWN;
        //        leftdown.XPos = -edgepos + margin;
        //        leftdown.YPos = -edgepos + margin;
        //        leftdown.ZPos = tempzpos;

        //        down.ChuckPosEnum = EnumChuckPosition.DOWN;
        //        down.XPos = 0;
        //        down.YPos = -edgepos + margin;
        //        down.ZPos = tempzpos;

        //        rightdown.ChuckPosEnum = EnumChuckPosition.RIGHTDOWN;
        //        rightdown.XPos = edgepos - margin;
        //        rightdown.YPos = -edgepos + margin;
        //        rightdown.ZPos = tempzpos;

        //        ChuckPosList.Clear();

        //        ChuckPosList.Add(leftup);
        //        ChuckPosList.Add(up);
        //        ChuckPosList.Add(rightup);
        //        ChuckPosList.Add(left);
        //        ChuckPosList.Add(center);
        //        ChuckPosList.Add(right);
        //        ChuckPosList.Add(leftdown);
        //        ChuckPosList.Add(down);
        //        ChuckPosList.Add(rightdown);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private int _MyProperty;
        public int MyProperty
        {
            get { return _MyProperty; }
            set
            {
                if (value != _MyProperty)
                {
                    _MyProperty = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModuleBase()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                DisplayPort = new DisplayPort();

                Array stagecamvalues = Enum.GetValues(typeof(StageCam));

                foreach (var cam in this.VisionManager().GetCameras())
                {
                    for (int index = 0; index < stagecamvalues.Length; index++)
                    {
                        if (((StageCam)stagecamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                        {
                            this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                            break;
                        }
                    }
                }

                ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;

                Binding bindX = new Binding
                {
                    Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosX"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToX, bindX);

                Binding bindY = new Binding
                {
                    Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosY"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToY, bindY);

                Binding bindCamera = new Binding
                {
                    Path = new System.Windows.PropertyPath("CurCam"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        private void CalcHeight()
        {
            try
            {
                MinHeight = _ChuckPosList.Where(x => x.CanvasTextBox.TbText != "F").Min(x => x.ZPos);

                double maxtmp = MinHeight;
                int validcount = 0;
                double ZSum = 0;

                for (int i = 0; i < ChuckPosList.Count; i++)
                {
                    if (ChuckPosList[i].CanvasTextBox.TbText != "F")
                    {
                        if ((ChuckPosList[i].ZPos != 0) && (ChuckPosList[i].ZPos >= maxtmp))
                        {
                            maxtmp = ChuckPosList[i].ZPos;
                        }
                        ZSum += ChuckPosList[i].ZPos;
                        validcount++;
                    }
                }

                MaxHeight = maxtmp;

                DiffHeight = Math.Abs(MaxHeight - MinHeight);

                if (validcount > 0)
                {
                    AvgHeight = ZSum / validcount;

                    AvgHeight = Math.Round(AvgHeight, 1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = InitModuleBase();

                MainViewTarget = DisplayPort;

                this.PnPManager().PnpLightJog.InitCameraJog(this, CurCam.GetChannelType());

                if (ChuckPosList == null)
                {
                    ChuckPosList = new ObservableCollection<ChuckPos>();
                }

                FocusingParam = new NormalFocusParameter();
                FocusingParam.SetDefaultParam();
                FocusingParam.FocusRange.Value = 150;
                FocusingRange = FocusingParam.FocusRange.Value;

                MinHeight = 0;
                MaxHeight = 0;
                DiffHeight = 0;

                //SpecHeight = 10;

                ChuckEndPointMargin = 30000;

                //CurrentChuckPosEnum = EnumChuckPosition.CENTER;

                //MakeChuckPos();


            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private PathGeometry _pathGeometry;
        public PathGeometry pathGeometry
        {
            get { return _pathGeometry; }
            set
            {
                if (value != _pathGeometry)
                {
                    _pathGeometry = value;
                    RaisePropertyChanged();
                }
            }
        }

        static double sin(double degrees)
        {
            return Math.Sin(degrees * 2d * Math.PI / MaxAngle);
        } //sin
        static double cos(double degrees)
        {
            return Math.Cos(degrees * 2d * Math.PI / MaxAngle);
        } //cos

        internal const double MaxAngle = 360;
        public const double ChuckSize = 300000;

        public double CanvasDiameter = 368;

        private ObservableCollection<BaseThing> _Items;
        public ObservableCollection<BaseThing> Items
        {
            get { return _Items; }
            set
            {
                if (value != _Items)
                {
                    _Items = value;
                    RaisePropertyChanged();
                }
            }
        }


        public void MakePositionData()
        {
            try
            {
                // 0 : Rectangle
                // 1 : Circle
                int PositionShapeMode = 0;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Point Origin = new Point(0, 0);
                    Point RealOrigin = new Point(-(ChuckSize / 2.0), +(ChuckSize / 2.0));

                    double RealXRatio = ChuckSize / CanvasDiameter;
                    double RealYRatio = ChuckSize / CanvasDiameter;

                    //ChuckEndPointMargin = 50000;

                    double InnerMargin = ChuckEndPointMargin * CanvasDiameter / ChuckSize;

                    double InnerCircleDiameter = CanvasDiameter - InnerMargin;

                    Point InnerOrigin = new Point(InnerMargin / 2.0, InnerMargin / 2.0);

                    double RadiusX = CanvasDiameter / 2.0;
                    double RadiusY = CanvasDiameter / 2.0;

                    double InnerCircleRadiusX = InnerCircleDiameter / 2.0;
                    double InnerCircleRadiusY = InnerCircleDiameter / 2.0;

                    Point Center = new Point(RadiusX, RadiusY);

                    double LineNum = 8;
                    int PointNum = 2;

                    double Angle = 360 / LineNum;

                    //double AngularSize = 45;

                    //double angleTo = AngularSize;

                    if (Items == null)
                    {
                        Items = new ObservableCollection<BaseThing>();
                    }
                    else
                    {
                        Items.Clear();
                    }

                    CircleVM ChuckBaseCircle = new CircleVM();

                    ChuckBaseCircle.Top = Origin.X;
                    ChuckBaseCircle.Left = Origin.Y;

                    ChuckBaseCircle.EllipseWidth = CanvasDiameter;
                    ChuckBaseCircle.EllipseHeight = CanvasDiameter;

                    ChuckBaseCircle.color = Brushes.White;
                    ChuckBaseCircle.Fillcolor = Brushes.Transparent;

                    Items.Add(ChuckBaseCircle);

                    CircleVM ChuckBaseInnerCircle = new CircleVM();

                    ChuckBaseInnerCircle.Top = InnerOrigin.X;
                    ChuckBaseInnerCircle.Left = InnerOrigin.Y;

                    ChuckBaseInnerCircle.EllipseWidth = InnerCircleDiameter;
                    ChuckBaseInnerCircle.EllipseHeight = InnerCircleDiameter;
                    ChuckBaseInnerCircle.color = Brushes.Green;
                    ChuckBaseInnerCircle.Fillcolor = Brushes.Transparent;

                    Items.Add(ChuckBaseInnerCircle);

                    double ratio = (1.0 * 1 / (PointNum));
                    Double ShapeWidth = 30;
                    Double ShapeHeight = 30;

                    ChuckPosList.Clear();

                    //double textboxOffsetX = 0;
                    //double textboxOffsetY = 0;

                    for (int i = 0; i < LineNum; i++)
                    {
                        LineVM tmpline = new LineVM();
                        tmpline.color = Brushes.White;

                        double tmpAngle = Angle * i;

                        tmpline.X1 = Center.X;
                        tmpline.Y1 = Center.Y;

                        //tmpline.X2 = Center.X + RadiusX * cos(tmpAngle);
                        //tmpline.Y2 = Center.Y + RadiusY * sin(tmpAngle);

                        tmpline.X2 = Center.X + InnerCircleRadiusX * cos(tmpAngle);
                        tmpline.Y2 = Center.Y + InnerCircleRadiusY * sin(tmpAngle);

                        Items.Add(tmpline);

                        // Make Point

                        for (int j = 1; j <= PointNum; j++)
                        {
                            if (PositionShapeMode == 0)
                            {
                                RectangleVM tmpShape = new RectangleVM();

                                tmpShape.Width = ShapeWidth;
                                tmpShape.Height = ShapeHeight;

                                tmpShape.command = ChuckMoveCommand;

                                tmpShape.color = Brushes.Green;
                                tmpShape.Fillcolor = Brushes.White;

                                // 반드시 포함되는 점 : tmpline.X2, tmpline.Y2에 해당하는 위치
                                // 그 다음 위치는 위의 점으로부터 센터로 들어오면서...

                                if (j == 1)
                                {
                                    tmpShape.Top = tmpline.X2 - (tmpShape.Height / 2.0);
                                    tmpShape.Left = tmpline.Y2 - (tmpShape.Width / 2.0);
                                }
                                else
                                {
                                    tmpShape.Top = tmpline.X2 - (InnerCircleRadiusX * ratio * cos(tmpAngle)) * (j - 1);
                                    tmpShape.Left = tmpline.Y2 - (InnerCircleRadiusY * ratio * sin(tmpAngle)) * (j - 1);

                                    tmpShape.Top = tmpShape.Top - (tmpShape.Height / 2.0);
                                    tmpShape.Left = tmpShape.Left - (tmpShape.Width / 2.0);
                                }

                                TextBoxVM tmpTextBox = new TextBoxVM();

                                tmpTextBox.Top = tmpShape.Top;
                                tmpTextBox.Left = tmpShape.Left + (tmpShape.Width / 2.0);
                                tmpTextBox.TbText = $"X";
                                //tmpTextBox.ForegroundColor = Brushes.Red;
                                //tmpTextBox.BackgroundColor = Brushes.Transparent;

                                Items.Add(tmpShape);
                                Items.Add(tmpTextBox);

                                // Calculate Real Position

                                ChuckPos tmpChuckPos = new ChuckPos();
                                //tmpChuckPos.CanvasCircle = tmpShape;
                                tmpChuckPos.CanvasTextBox = tmpTextBox;

                                double RealXOffset = RealXRatio * (tmpShape.Left + (tmpShape.Width / 2.0));
                                double RealYOffset = RealYRatio * (tmpShape.Top + (tmpShape.Height / 2.0));

                                tmpChuckPos.XPos = RealOrigin.X + RealXOffset;
                                tmpChuckPos.YPos = RealOrigin.Y - RealYOffset;

                                tmpChuckPos.XPos = Math.Round(tmpChuckPos.XPos, 0);
                                tmpChuckPos.YPos = Math.Round(tmpChuckPos.YPos, 0);

                                tmpChuckPos.ZPos = 0;

                                ChuckPosList.Add(tmpChuckPos);

                                tmpShape.commandparam = tmpChuckPos;
                            }
                            else
                            {
                                CircleVM tmpShape = new CircleVM();

                                tmpShape.EllipseWidth = ShapeWidth;
                                tmpShape.EllipseHeight = ShapeHeight;

                                tmpShape.color = Brushes.Green;
                                tmpShape.Fillcolor = Brushes.White;

                                // 반드시 포함되는 점 : tmpline.X2, tmpline.Y2에 해당하는 위치
                                // 그 다음 위치는 위의 점으로부터 센터로 들어오면서...

                                if (j == 1)
                                {
                                    tmpShape.Top = tmpline.X2 - (tmpShape.EllipseHeight / 2.0);
                                    tmpShape.Left = tmpline.Y2 - (tmpShape.EllipseWidth / 2.0);
                                }
                                else
                                {
                                    tmpShape.Top = tmpline.X2 - (InnerCircleRadiusX * ratio * cos(tmpAngle)) * (j - 1);
                                    tmpShape.Left = tmpline.Y2 - (InnerCircleRadiusY * ratio * sin(tmpAngle)) * (j - 1);

                                    tmpShape.Top = tmpShape.Top - (tmpShape.EllipseHeight / 2.0);
                                    tmpShape.Left = tmpShape.Left - (tmpShape.EllipseWidth / 2.0);
                                }

                                TextBoxVM tmpTextBox = new TextBoxVM();

                                tmpTextBox.Top = tmpShape.Top;
                                tmpTextBox.Left = tmpShape.Left + (tmpShape.EllipseWidth / 2.0);
                                tmpTextBox.TbText = $"X";
                                //tmpTextBox.ForegroundColor = Brushes.Red;
                                //tmpTextBox.BackgroundColor = Brushes.Transparent;

                                Items.Add(tmpShape);
                                Items.Add(tmpTextBox);

                                // Calculate Real Position

                                ChuckPos tmpChuckPos = new ChuckPos();
                                //tmpChuckPos.CanvasCircle = tmpShape;
                                tmpChuckPos.CanvasTextBox = tmpTextBox;

                                double RealXOffset = RealXRatio * (tmpShape.Left + (tmpShape.EllipseWidth / 2.0));
                                double RealYOffset = RealYRatio * (tmpShape.Top + (tmpShape.EllipseHeight / 2.0));

                                tmpChuckPos.XPos = RealOrigin.X + RealXOffset;
                                tmpChuckPos.YPos = RealOrigin.Y - RealYOffset;

                                tmpChuckPos.ZPos = 0;

                                ChuckPosList.Add(tmpChuckPos);


                            }
                        }
                    }

                    if (PositionShapeMode == 0)
                    {
                        RectangleVM tmpShape = new RectangleVM();

                        tmpShape.Width = ShapeWidth;
                        tmpShape.Height = ShapeHeight;
                        tmpShape.color = Brushes.Green;
                        tmpShape.Fillcolor = Brushes.White;

                        tmpShape.Top = Center.X - (tmpShape.Width / 2.0);
                        tmpShape.Left = Center.Y - (tmpShape.Height / 2.0);

                        TextBoxVM tmpTextBox = new TextBoxVM();

                        tmpTextBox.Top = tmpShape.Top;
                        tmpTextBox.Left = tmpShape.Left + (tmpShape.Width / 2.0);
                        tmpTextBox.TbText = $"X";
                        //tmpTextBox.ForegroundColor = Brushes.Red;
                        //tmpTextBox.BackgroundColor = Brushes.Transparent;

                        Items.Add(tmpShape);
                        Items.Add(tmpTextBox);

                        ChuckPos tmpChuckPos = new ChuckPos();
                        tmpChuckPos.CanvasTextBox = tmpTextBox;

                        tmpChuckPos.XPos = 0;
                        tmpChuckPos.YPos = 0;
                        tmpChuckPos.ZPos = 0;

                        ChuckPosList.Add(tmpChuckPos);

                        tmpShape.command = ChuckMoveCommand;
                        tmpShape.commandparam = tmpChuckPos;
                    }
                    else
                    {
                        CircleVM tmpShape = new CircleVM();

                        tmpShape.EllipseWidth = ShapeWidth;
                        tmpShape.EllipseHeight = ShapeHeight;
                        tmpShape.color = Brushes.Green;
                        tmpShape.Fillcolor = Brushes.White;

                        tmpShape.Top = Center.X - (tmpShape.EllipseWidth / 2.0);
                        tmpShape.Left = Center.Y - (tmpShape.EllipseHeight / 2.0);

                        TextBoxVM tmpTextBox = new TextBoxVM();

                        tmpTextBox.Top = tmpShape.Top;
                        tmpTextBox.Left = tmpShape.Left + (tmpShape.EllipseWidth / 2.0);
                        tmpTextBox.TbText = $"X";
                        //tmpTextBox.ForegroundColor = Brushes.Red;
                        //tmpTextBox.BackgroundColor = Brushes.Transparent;

                        Items.Add(tmpShape);
                        Items.Add(tmpTextBox);

                        ChuckPos tmpChuckPos = new ChuckPos();
                        tmpChuckPos.CanvasTextBox = tmpTextBox;

                        tmpChuckPos.XPos = 0;
                        tmpChuckPos.YPos = 0;
                        tmpChuckPos.ZPos = 0;

                        ChuckPosList.Add(tmpChuckPos);
                    }
                });

                //{
                //    new CircleVM {Top=100.0, Left=50.0, EllipseHeight=20, EllipseWidth=20 },
                //    new TextBoxVM{Top=50.0, Left=100.0,  TbText="Original Text" }
                //};

                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    if (pathGeometry == null)
                //    {
                //        pathGeometry = new PathGeometry();
                //    }

                //    pathGeometry.Clear();

                //    pathGeometry.FillRule = FillRule.Nonzero;

                //    PathFigure pathFigure = new PathFigure();

                //    pathFigure.StartPoint = new Point(50, 50);
                //    pathFigure.IsClosed = true;
                //    pathGeometry.Figures.Add(pathFigure);

                //    LineSegment lineSegment1 = new LineSegment();

                //    lineSegment1.Point = new Point(0, 100);
                //    //pathFigure.Segments.Add(lineSegment1);

                //    LineSegment lineSegment2 = new LineSegment();

                //    lineSegment2.Point = new Point(50, 100);
                //    //pathFigure.Segments.Add(lineSegment2);

                //    ArcSegment arcSegment1 = new ArcSegment();
                //    ArcSegment arcSegment2 = new ArcSegment();

                //    Point Center = new Point(50, 50);

                //    double RadiusX = 100;
                //    double RadiusY = 100;

                //    double AngularSize = 45;

                //    double angleTo = AngularSize;

                //    Point first = new Point(Center.X + RadiusX, Center.Y);
                //    Point second = new Point(Center.X + RadiusX * cos(angleTo), Center.Y - RadiusX * sin(angleTo));

                //    arcSegment1.Point = first;
                //    arcSegment2.Point = second;

                //    pathFigure.Segments.Add(arcSegment1);
                //    //pathFigure.Segments.Add(arcSegment2);

                //    //pathFigure.StartPoint = new Point(47.7778, 48.6667);
                //    //pathFigure.IsClosed = true;
                //    //pathGeometry.Figures.Add(pathFigure);

                //    //LineSegment lineSegment1 = new LineSegment();

                //    //lineSegment1.Point = new Point(198, 48.6667);
                //    //pathFigure.Segments.Add(lineSegment1);

                //    //LineSegment lineSegment2 = new LineSegment();

                //    //lineSegment2.Point = new Point(198, 102);
                //    //pathFigure.Segments.Add(lineSegment2);

                //    //BezierSegment bezierSegment1 = new BezierSegment();

                //    //bezierSegment1.Point1 = new Point(174.889, 91.3334);
                //    //bezierSegment1.Point2 = new Point(157.111, 79.7778);
                //    //bezierSegment1.Point3 = new Point(110.889, 114.444);
                //    //pathFigure.Segments.Add(bezierSegment1);

                //    //BezierSegment bezierSegment2 = new BezierSegment();

                //    //bezierSegment2.Point1 = new Point(64.667, 149.111);
                //    //bezierSegment2.Point2 = new Point(58.4444, 130.444);
                //    //bezierSegment2.Point3 = new Point(47.7778, 118.889);
                //    //pathFigure.Segments.Add(bezierSegment2);
                //});
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //Path path = new Path();

            //path.Stretch = Stretch.Fill;
            //path.StrokeLineJoin = PenLineJoin.Round;
            //path.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            //path.Fill = new SolidColorBrush(Color.FromRgb(170, 87, 170));
            //path.StrokeThickness = 2;
            //path.Data = pathGeometry;

            //Content = path;
        }

        //public void MakeLine()
        //{
        //    new LineSegment(new Point(x2, y2), true),
        //    new ArcSegment(new Point(x3, y3), new Size(100 * outerRadius, 100 * outerRadius), 0, largeAngle, SweepDirection.Clockwise, true),
        //    new LineSegment(new Point(x4, y4), true),
        //    new ArcSegment(new Point(x1, y1), new Size(100 * innerRadius, 100 * innerRadius), 0, largeAngle, SweepDirection.Counterclockwise, true),
        //}

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                //Tilted 끄기
                TiltedStateChange();

                this.StageSupervisor().StageModuleState.ZCLEARED();

                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
            
                UserZHeight = 0;

                ChangedMarginValue();

                LoggerManager.Debug($"[ChuckPlanarityVM], PageSwitched() : WaferObject Status = {this.StageSupervisor().WaferObject.GetStatus()}, ActualThickness = {this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness}");

                if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST)
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness, true);
                }
                else
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, 0, true);
                }

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                CurCam.SetLight(EnumLightType.COAXIAL, 100);

                GetSoakingParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                //this.SysState().SetSetUpDoneState();
                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.VisionManager().StopGrab(CurCam.GetChannelType());
                StageCylinderType.MoveWaferCam.Retract();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public void TiltedStateChange()
        {
            try
            {
                //Tilted 초기화.
                var zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                for (int i = 0; i < zaxis.GroupMembers.Count; i++)
                {
                    zaxis.GroupMembers[i].Status.CompValue = 0.0;
                    AlignResult.PlaneOffset[i] = zaxis.GroupMembers[i].Status.CompValue;
                }
                //Pin Align State를 DONE -> IDLE 상태로 변경한다.
                AlignStateEnum pinalignstate = this.StageSupervisor().ProbeCardInfo.AlignState.Value;
                if(pinalignstate == AlignStateEnum.DONE)
                {
                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _ChuckMoveCommand;
        public IAsyncCommand ChuckMoveCommand
        {
            get
            {
                if (null == _ChuckMoveCommand) _ChuckMoveCommand = new AsyncCommand<object>(ChuckMoveCommandFunc);
                return _ChuckMoveCommand;
            }
        }

        public async Task WrapperChuckMoveCommand(ChuckPos param)
        {
            try
            {
                await ChuckMoveCommandFunc(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ChuckMoveCommandFunc(object param)
        {
            try
            {
                if (param != null)
                {
                    foreach (var item in ChuckPosList)
                    {
                        item.CanvasTextBox.IsSelected = false;
                    }

                    ChuckPos pos = (param as ChuckPos);

                    if (pos != null)
                    {
                        double tmpzpos = 0.0;

                        if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST)
                        {
                            if (pos.ZPos != 0)
                            {
                                tmpzpos = pos.ZPos;
                            }
                            else
                            {
                                tmpzpos = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;
                            }
                        }
                        else
                        {
                            if(UserZHeight == 0)
                            {
                                WaferCoordinate wcoord = null;

                                if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                                {
                                    wcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                                }
                                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                                {
                                    wcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                                }

                                UserZHeight = wcoord.GetZ();
                            }

                            if (pos.ZPos != 0)
                            {
                                // Measure 이후 특정 포인트로 움직일 때는 해당 포지션에 저장된 위치로 간다.
                                tmpzpos = pos.ZPos;
                            }
                            else
                            {
                                // Measuer 시 포커싱을 시작할 위치 (이전 포인트에서 포커싱이 된 포지션)
                                tmpzpos = UserZHeight;
                            }
                        }

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(pos.XPos, pos.YPos, tmpzpos, true);

                        pos.CanvasTextBox.IsSelected = true;
                        CurrentChuckPos = pos;
                        ValueCopy();
                    }
                    else
                    {
                        LoggerManager.Error($"Command parameter's type is wrong.");
                    }
                }
                else
                {
                    LoggerManager.Error($"Command parameter is null value.");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //
            }

            this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
        }

        //private async void ChuckMoveCommandFunc(EnumChuckPosition param)
        //{
        //    try
        //    {
        //        //EnumChuckPosition chuckposenum = (EnumChuckPosition)param;

        //        //

        //        //ChuckPos tmp = null;

        //        //tmp = ChuckPosList.FirstOrDefault(x => x.ChuckPosEnum == param);

        //        //if (tmp != null)
        //        //{
        //        //    double tmpzpos = 0.0;

        //        //    if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST)
        //        //    {
        //        //        tmpzpos = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;
        //        //    }

        //        //    this.StageSupervisor().StageModuleState.WaferHighViewMove(tmp.XPos, tmp.YPos, tmpzpos);

        //        //    CurrentChuckPosEnum = param;
        //        //}
        //        //else
        //        //{
        //        //    LoggerManager.Error($"Logic Check");
        //        //}
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //
        //    }
        //}


        private AsyncCommand _MeasureOnePositionCommand;
        public IAsyncCommand MeasureOnePositionCommand
        {
            get
            {
                if (null == _MeasureOnePositionCommand) _MeasureOnePositionCommand = new AsyncCommand(MeasureOnePositionCommandFunc);
                return _MeasureOnePositionCommand;
            }
        }

        public async Task<EventCodeEnum> MeasureOnePositionCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                MachineCoordinate mccoord = new MachineCoordinate();
                WaferCoordinate wcoord = null;

                this.VisionManager().StopGrab(CurCam.GetChannelType());

                Thread.Sleep(10);

                double focuswidth = 480;
                double focusheight = 480;
                double startposX = (CurCam.GetGrabSizeWidth() - focuswidth) / 2.0;
                double startposY = (CurCam.GetGrabSizeHeight() - focusheight) / 2.0;

                FocusingParam.FocusingROI.Value = new Rect(startposX, startposY, focuswidth, focusheight);
                
                retval = FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);

                if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    wcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                }
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    wcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }

                if (retval == EventCodeEnum.NONE)
                {
                    CurrentChuckPos.ZPos = wcoord.Z.Value;
                    CurrentChuckPos.CanvasTextBox.TbText = $"{CurrentChuckPos.ZPos:F0}";
                }
                else
                {
                    CurrentChuckPos.CanvasTextBox.TbText = $"F";
                    LoggerManager.Debug($"MeasureOnePositionCommandFunc() : Focusing Failed. Point (X : {CurrentChuckPos.XPos}, Y : {CurrentChuckPos.YPos}) reason = {retval} ");
                }

                ValueCopy();
                CalcHeight();

                //척 가운데값이 비정상으로 낮게 나오기때문에 data 평균값으로 바꿈... (Lloyd)
                ChuckPosList.Where(x => x.XPos == 0 && x.YPos == 0).FirstOrDefault().ZPos = AvgHeight;
                ChuckPosList.Where(x => x.XPos == 0 && x.YPos == 0).FirstOrDefault().CanvasTextBox.TbText = $"{AvgHeight:F0}";

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void ValueCopy()
        {
            ChuckPos updateChuckPos = ChuckPosList.Where(x => x.XPos == CurrentChuckPos.XPos && x.YPos == CurrentChuckPos.YPos).FirstOrDefault();
            updateChuckPos.CanvasTextBox.IsSelected = CurrentChuckPos.CanvasTextBox.IsSelected;
            updateChuckPos.CanvasTextBox.TbText = CurrentChuckPos.CanvasTextBox.TbText;
            updateChuckPos.ZPos = CurrentChuckPos.ZPos;
        }

        private AsyncCommand _MeasureAllPositionCommand;
        public IAsyncCommand MeasureAllPositionCommand
        {
            get
            { 
                if (null == _MeasureAllPositionCommand) _MeasureAllPositionCommand = new AsyncCommand(MeasureAllPositionCommandFunc);
                return _MeasureAllPositionCommand;
            }
        }

        public async Task<EventCodeEnum> MeasureAllPositionCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            string measureResultforLog = "";
            try
            {
                foreach (var pos in ChuckPosList)
                {
                    pos.ZPos = 0;
                    pos.CanvasTextBox.TbText = $"X";
                }

                double tmpzpos = 0.0;

                foreach (var pos in ChuckPosList)
                {
                    // MOVE
                    await ChuckMoveCommandFunc(pos);

                    // FOCUSING
                    await MeasureOnePositionCommandFunc();

                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateDeviceChangeInfo(GetChuckPlanarityInfo());
                }

                LoggerManager.Debug($"Chuck Planarity Result");

                foreach (var pos in ChuckPosList)
                {
                    if (pos.CanvasTextBox.TbText == "F")
                    {
                        measureResultforLog = "[FAIL]";
                    }
                    else
                    {
                        measureResultforLog = "[SUCCESS]";
                    }

                    LoggerManager.Debug($"{measureResultforLog} Position : X : {pos.XPos}, Y : {pos.YPos}, Z : {pos.ZPos}");
                }

                LoggerManager.Debug($"MinHeight = {MinHeight}, MaxHeight = {MaxHeight}, DiffHeight = {DiffHeight}, AvgHeight = {AvgHeight}");

                this.StageSupervisor().StageModuleState.ZCLEARED();

                if(ChuckPosList.Count(x => x.CanvasTextBox.TbText == "F") > 0)
                {
                    this.MetroDialogManager().ShowMessageDialog("Measure Failed", "There is a point where focusing failed. Please measure again.", EnumMessageStyle.Affirmative);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private RelayCommand<Object> _CaptureCommand;
        public ICommand CaptureCommand
        {
            get
            {
                if (null == _CaptureCommand) _CaptureCommand = new RelayCommand<Object>(CaptureCommandFunc);
                return _CaptureCommand;
            }
        }

        private void CaptureCommandFunc(object obj)
        {
            try
            {
                string filepath;
                string filename;
                string fullpath;
                string fileextension;

                double curtemp = this.TempController().TempInfo.CurTemp.Value;

                System.Drawing.Image screen = ScreenshotCapture.TakeScreenshot(true);

                filepath = @"C:\ProberSystem\Snapshot\ChuckPlanarity";
                //filename = DateTime.Now.ToString($"yyyy_MM_dd_HH_mm_ss_CurTemp_{curtemp}_Diff_{DiffHeight}");
                filename = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                filename = filename + $"_CurTemp_{curtemp}_Diff_{DiffHeight}";
                fileextension = ".jpg";

                filename = filename + fileextension;

                fullpath = System.IO.Path.Combine(filepath, filename);

                if (Directory.Exists(System.IO.Path.GetDirectoryName(fullpath)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullpath));
                }

                if (File.Exists(fullpath) == false)
                {
                    screen.Save(fullpath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SetAdjustPlanartyCommand;
        public IAsyncCommand SetAdjustPlanartyCommand
        {
            get
            {
                if (null == _SetAdjustPlanartyCommand) _SetAdjustPlanartyCommand = new AsyncCommand(SetAdjustPlanartyFunc);
                return _SetAdjustPlanartyCommand;
            }
        }

        public async Task<EventCodeEnum> SetAdjustPlanartyFunc()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                string msg = "";

                float z0angle = this.CoordinateManager().StageCoord.Z0Angle.Value;
                float z1angle = this.CoordinateManager().StageCoord.Z1Angle.Value;
                float z2angle = this.CoordinateManager().StageCoord.Z2Angle.Value;
                float pcd = this.CoordinateManager().StageCoord.PCD.Value;
                double homeoffsettolerance = this.CoordinateManager().StageCoord.ChuckPlanarityTol.Value;
                //1. Pillar 위치 찾기.
                double pillar0X = pcd * Math.Cos(Math.PI * (z0angle) / 180)*-1;
                double pillar0Y = pcd * Math.Sin(Math.PI * (z0angle) / 180)*-1;
                double pillar1X = pcd * Math.Cos(Math.PI * (z1angle) / 180)*-1;
                double pillar1Y = pcd * Math.Sin(Math.PI * (z1angle) / 180)*-1;
                double pillar2X = pcd * Math.Cos(Math.PI * (z2angle) / 180)*-1;
                double pillar2Y = pcd * Math.Sin(Math.PI * (z2angle) / 180)*-1;

                List<WaferCoordinate> PillarPos = new List<WaferCoordinate>();
                PillarPos.Add(new WaferCoordinate(pillar0X, pillar0Y));
                PillarPos.Add(new WaferCoordinate(pillar1X, pillar1Y));
                PillarPos.Add(new WaferCoordinate(pillar2X, pillar2Y));

                List<double> pillaroffset = new List<double>();

                //홈 옵셋 Back : 실패가 났을 때 값을 돌려놓기 위함
                List<double> HomeOffsetBackup = new List<double>();
                HomeOffsetBackup.Add(this.MotionManager().GetAxis(EnumAxisConstants.Z0).Param.HomeOffset.Value);
                HomeOffsetBackup.Add(this.MotionManager().GetAxis(EnumAxisConstants.Z1).Param.HomeOffset.Value);
                HomeOffsetBackup.Add(this.MotionManager().GetAxis(EnumAxisConstants.Z2).Param.HomeOffset.Value);

                //2. Wafer의 Exist, Type에 따라서 Focusing height + FocusingFlatnessThd 달라질 수 있음. -> Focusing Z 위치 계산해주기 위함.
                double focusingZPos = GetFocusingZ();
                if(focusingZPos < 0)
                {
                    //error
                    return retVal;
                }

                int index = 0;
                foreach (var pillar in PillarPos)
                {
                    double actZpos = 0.0;
                    retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(pillar.GetX(), pillar.GetY(), focusingZPos, true); 
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"SetAdjustPlanartyFunc() : Failed to 'WaferHighViewMove', " +
                            $"return value : {retVal.ToString()}, pillar index : {index} x:{pillar.GetX():0.0}, y:{pillar.GetY():0.0}, z:{focusingZPos:0.0}");
                        return retVal;
                    }

                    retVal = PillarHeightFocusing(focusingZPos, ref actZpos);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"SetAdjustPlanartyFunc() : Failed to 'PillarHeightFocusing()', " +
                                $"return value : {retVal.ToString()}, pillar index : {index} x:{pillar.GetX():0.0}, y:{pillar.GetY():0.0}, z:{focusingZPos:0.0}");
                        return retVal;
                    }

                    pillaroffset.Add(actZpos);

                    if (index > 0)
                    {
                          pillaroffset[index] = pillaroffset[index] - pillaroffset[0];
                    }

                    index++;
                }
                pillaroffset[0] = 0.0; //pillaroffset[0] = pillaroffset[0] - pillaroffset[0];

                LoggerManager.Debug($"Group Z HomeOffset. values = Z0 : [{HomeOffsetBackup[0]:0.0}], Z1 : [{HomeOffsetBackup[1]:0.0}], Z2 : [{HomeOffsetBackup[2]:0.0}]");
                LoggerManager.Debug($"Calculated Offset. values = Z0 : [{pillaroffset[0]:0.0}], Z1 : [{pillaroffset[1]:0.0}], Z2 : [{pillaroffset[2]:0.0}]");


                double compLimit = PinAlignParam.PinPlaneAdjustParam.MaxCompHeight.Value;
                if (Math.Abs(pillaroffset[0]) > compLimit || Math.Abs(pillaroffset[1]) > compLimit || Math.Abs(pillaroffset[2]) > compLimit)
                {
                    msg = $"Group-Z offset tolereance : {compLimit:0.0}\r\n" +
                        $"Updated offset Z0 : {pillaroffset[0]:0.0}, Z1 : {pillaroffset[1]:0.0}, Z2 : {pillaroffset[2]:0.0}\r\n" +
                        $"Homeoffset does not updated.";
                    var retryresult = await this.MetroDialogManager().ShowMessageDialog("[Error] Updated offset tolereance error", msg, MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    LoggerManager.Error($"SetAdjustPlanartyFunc() : Failed to {msg}");
                    retVal = EventCodeEnum.PARAM_ERROR;
                    return retVal;
                }
                else
                {
                    this.MotionManager().GetAxis(EnumAxisConstants.Z0).Param.HomeOffset.Value += Math.Round(pillaroffset[0], 1);
                    this.MotionManager().GetAxis(EnumAxisConstants.Z1).Param.HomeOffset.Value += Math.Round(pillaroffset[1], 1);
                    this.MotionManager().GetAxis(EnumAxisConstants.Z2).Param.HomeOffset.Value += Math.Round(pillaroffset[2], 1);
                }

                //이전값과 변경된 값 표시 -> Machine Init ? OK , Cancle 하면 이전값으로 SET
                var result = await this.MetroDialogManager().ShowMessageDialog("Update Group Z(Z0, Z1, Z2) HomeOffset",
                    $"Current homeoffset Z0 : {HomeOffsetBackup[0]:0.0}, Z1 : {HomeOffsetBackup[1]:0.0}, Z2 : {HomeOffsetBackup[2]:0.0}\r\n" 
                    + $"Updated offset Z0 : {pillaroffset[0]:0.0}, Z1 : {pillaroffset[1]:0.0}, Z2 : {pillaroffset[2]:0.0}\n\r" 
                    + $"To reflect the updated offset in the homeoffset, Machine Init is required.\r\n"
                    + $"OK button: Start Machine Init. \r\n" 
                    + $"Cancel button: Home Offset does not updated. \r\n"
                    , MetroDialogInterfaces.EnumMessageStyle.AffirmativeAndNegative);

                if (result == MetroDialogInterfaces.EnumMessageDialogResult.AFFIRMATIVE) //OK Button
                {
                    this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Machine Init");

                    ErrorCodeResult initretval = new ErrorCodeResult();
                    initretval = await Task.Run(() => MachineInit());

                    if(initretval.ErrorCode == EventCodeEnum.NONE)
                    {
                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                        CurCam.SetLight(EnumLightType.COAXIAL, 100);

                        //Retry Focusing
                        List<double> retrypillaroffset = new List<double>();
                        index = 0;
                        foreach (var pillar in PillarPos)
                        {
                            double actZpos = 0.0;
                            retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(pillar.GetX(), pillar.GetY(), focusingZPos, true);
                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"SetHomeOffsetFunc() : Failed to 'WaferHighViewMove', " +
                                    $"return value : {retVal.ToString()}, pillar index : {index} x:{pillar.GetX():0.0}, y:{pillar.GetY():0.0}, z:{focusingZPos:0.0}");
                                return retVal;
                            }

                            retVal = PillarHeightFocusing(focusingZPos, ref actZpos);
                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"SetAdjustPlanartyFunc() : Failed to 'PillarHeightFocusing()', " +
                                    $"return value : {retVal.ToString()}, pillar index : {index} x:{pillar.GetX():0.0}, y:{pillar.GetY():0.0}, z:{focusingZPos:0.0}");
                                return retVal;
                            }

                            retrypillaroffset.Add(actZpos);

                            if (index > 0)
                            {
                                retrypillaroffset[index] = retrypillaroffset[index] - retrypillaroffset[0];
                            }

                            index++;
                        }
                        retrypillaroffset[0] = 0.0; //pillaroffset[0] = pillaroffset[0] - pillaroffset[0];

                        if(Math.Abs(retrypillaroffset[0]) > homeoffsettolerance || Math.Abs(retrypillaroffset[1]) > homeoffsettolerance || Math.Abs(retrypillaroffset[2]) > homeoffsettolerance)
                        {
                            LoggerManager.Debug($"SetAdjustPlanartyFunc() : 'PillarHeightFocusing()', " +
                                    $"Updated homeoffset Z0 : {this.MotionManager().GetAxis(EnumAxisConstants.Z0).Param.HomeOffset.Value:0.0}, Z1 : {this.MotionManager().GetAxis(EnumAxisConstants.Z1).Param.HomeOffset.Value:0.0}, Z2 : {this.MotionManager().GetAxis(EnumAxisConstants.Z2).Param.HomeOffset.Value:0.0}"
                                    + $"DiffZ0 : {retrypillaroffset[0]:0.0}, DiffZ1 : {retrypillaroffset[1]:0.0}, DiffZ2 : {retrypillaroffset[2]:0.0}"
                                    + "It is recommended that the diff value be within 15um.");

                            //TO DO : 자동으로 다시 한번 더 수행하도록 한다 or 이전 값으로 돌린 다음 Machine Init을 한다
                        }

                        //message 표시
                        msg = $"Current homeoffset Z0 : {this.MotionManager().GetAxis(EnumAxisConstants.Z0).Param.HomeOffset.Value:0.0}, Z1 : {this.MotionManager().GetAxis(EnumAxisConstants.Z1).Param.HomeOffset.Value:0.0}, Z2 : {this.MotionManager().GetAxis(EnumAxisConstants.Z2).Param.HomeOffset.Value:0.0}\n\r"
                            + $"The difference value verified by refocusing the updated offset\r\n"
                            + $"DiffZ0 : {Math.Abs(retrypillaroffset[0]):0.0}, DiffZ1 : {Math.Abs(retrypillaroffset[1]):0.0}, DiffZ2 : {Math.Abs(retrypillaroffset[2]):0.0}\r\n"
                            + "If the diff value is greater than to 15um, It is recommended to operate the Adjust Planarity again.";

                        var retryresult = await this.MetroDialogManager().ShowMessageDialog("Updated Group Z(Z0,Z1,Z2) HomeOffset", msg, MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        
                    }
                    else
                    {
                        //머신인잇 실패
                        this.MotionManager().GetAxis(EnumAxisConstants.Z0).Param.HomeOffset.Value = HomeOffsetBackup[0];
                        this.MotionManager().GetAxis(EnumAxisConstants.Z1).Param.HomeOffset.Value = HomeOffsetBackup[1];
                        this.MotionManager().GetAxis(EnumAxisConstants.Z2).Param.HomeOffset.Value = HomeOffsetBackup[2];
                    }
                }
                else //Cancel Button
                {
                    this.MotionManager().GetAxis(EnumAxisConstants.Z0).Param.HomeOffset.Value = HomeOffsetBackup[0];
                    this.MotionManager().GetAxis(EnumAxisConstants.Z1).Param.HomeOffset.Value = HomeOffsetBackup[1];
                    this.MotionManager().GetAxis(EnumAxisConstants.Z2).Param.HomeOffset.Value = HomeOffsetBackup[2];
                }

                this.MotionManager().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

            return retVal;
        }

        public double GetFocusingZ()
        {
            double focusingZPos = -1;
            string msg = "";
            try
            {
                var waferExist = this.StageSupervisor().WaferObject.GetStatus();
                var waferType = this.StageSupervisor().WaferObject.GetWaferType();
                if (waferExist == EnumSubsStatus.EXIST)
                {
                    FocusingParam.FlatnessThreshold.Value = SoakingSysParam_Clone.ProcessingFocusingFlatnessThd.Value;

                    if (waferType == EnumWaferType.POLISH)
                    {
                        FocusingParam.FlatnessThreshold.Value = SoakingSysParam_Clone.PolishFocusingFlatnessThd.Value;

                        var polish_thickness = this.StageSupervisor().WaferObject.GetPolishInfo().Thickness.Value;
                        if (polish_thickness <= 0)
                        {
                            msg = $"Current Wafer Information EXIST : {waferExist}, TYPE : {waferType}, Start Focusing Z Value : {polish_thickness}";
                            return focusingZPos;
                        }
                        else
                        {
                            focusingZPos = polish_thickness;
                        }
                    }
                    else if (waferType == EnumWaferType.STANDARD)
                    {
                        var wafer_thickness = this.StageSupervisor().WaferObject.GetPhysInfo().Thickness.Value;
                        if (wafer_thickness <= 0)
                        {
                            msg = $"Current Wafer Information EXIST : {waferExist}, TYPE : {waferType}, Start Focusing Z Value : {wafer_thickness}";
                            return focusingZPos;
                        }
                        else
                        {
                            focusingZPos = wafer_thickness;
                        }

                    }
                    else
                    {
                        focusingZPos = this.StageSupervisor().WaferMaxThickness;
                    }
                }
                else if (waferExist == EnumSubsStatus.NOT_EXIST)
                {
                    //chuck focusing
                    focusingZPos = (double)SoakingSysParam_Clone.ChuckRefHight.Value;
                    FocusingParam.FlatnessThreshold.Value = SoakingSysParam_Clone.ChuckFocusingFlatnessThd.Value;
                }
                else
                {
                    msg = $"Current Wafer Information EXIST : {waferExist}, TYPE : {waferType}, Failed";
                    return focusingZPos;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if(msg != "")
                {
                    this.MetroDialogManager().ShowMessageDialog("[Error] Get Focusing Z Value", msg, MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
            }
            return focusingZPos;
        }

        public async Task<ErrorCodeResult> MachineInit()
        {
            ErrorCodeResult initretval = new ErrorCodeResult();

            try
            {
                initretval = await this.StageSupervisor().SystemInit();

                if (initretval.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Prolog(PrologType.INFORMATION, initretval.ErrorCode);
                    if (initretval.ErrorCode == EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.MOTION_STAGE_INIT_ERROR);
                        LoggerManager.Error($"ChuckPlanarity - MachineInit() : SystemInit Failed, Reason = {initretval.ErrorCode} \n have to check that " + $"card up module and then manual operation ");
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }
                    else if (initretval.ErrorCode == EventCodeEnum.GP_CardChange_CHECK_TO_LATCH)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.MOTION_STAGE_INIT_ERROR);
                        LoggerManager.Error($"ChuckPlanarity - MachineInit() : SystemInit Failed, Reason = {initretval.ErrorCode} \n have to check that " +
                            $"card latch and then manual operation ");
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }
                    else if (initretval.ErrorCode == EventCodeEnum.STAGEMOVE_LOCK)
                    {
                        initretval.ErrorMsg = "The stage is currently locked. Please try again after Unlock.";
                        LoggerManager.Error($"ChuckPlanarity - MachineInit() : SystemInit Failed, Reason = {initretval.ErrorCode}");
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }
                    else if (initretval.ErrorCode == EventCodeEnum.TESTERHEAD_PURGE_AIR_ERROR)
                    {
                        initretval.ErrorMsg = "Tester head purge air off. Please check.";
                        LoggerManager.Error($"ChuckPlanarity - MachineInit() : SystemInit Failed, Reason = {initretval.ErrorCode}");
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }
                    else
                    {
                        this.NotifyManager().Notify(EventCodeEnum.MOTION_STAGE_INIT_ERROR);
                        LoggerManager.Error($"ChuckPlanarity - MachineInit() : SystemInit Failed, Reason = {initretval.ErrorCode}");
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return initretval;
        }
        public EventCodeEnum PillarHeightFocusing(double focusingZPos, ref double actZpos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (FocusingParam.FocusRange.Value == 0)
                {
                    LoggerManager.Debug($"FocusingParam.FocusRange.Value is zero. so it will be default value");
                    FocusingParam.SetDefaultParam();
                }

                retVal = FocusingModule.Focusing_Retry(FocusingParam, true, true, false, this);
                if(retVal == EventCodeEnum.NONE)
                {
                    WaferCoordinate wfcoord = new WaferCoordinate();
                    this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref actZpos);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private RelayCommand<Object> _DecimalBoxClickCommand;
        public ICommand DecimalBoxClickCommand
        {
            get
            {
                if (null == _DecimalBoxClickCommand) _DecimalBoxClickCommand = new RelayCommand<Object>(DecimalBoxClickCommandFunc);
                return _DecimalBoxClickCommand;
            }
        }


        private void DecimalBoxClickCommandFunc(Object param)
        {
            try
            {



                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                double oldvalue = Convert.ToDouble(tb.Text);

                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 1, 7);

                double newvalue = Convert.ToDouble(tb.Text);

                if (newvalue < 0)
                {
                    tb.Text = oldvalue.ToString();
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }
                else
                {
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                    if (oldvalue != newvalue)
                    {
                        ChangedMarginValue();
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _FloatTextBoxClickCommand;
        public ICommand FloatTextBoxClickCommand
        {
            get
            {
                if (null == _FloatTextBoxClickCommand) _FloatTextBoxClickCommand = new RelayCommand<Object>(FloatTextBoxClickCommandFunc);
                return _FloatTextBoxClickCommand;
            }
        }

        private void FloatTextBoxClickCommandFunc(object param)
        {
            try
            {
                if (param is System.Windows.Controls.TextBox)
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                    //tb.Text.Replace("+/")

                    string replaceStr = tb.Text.Replace(" +/- ", "");

                    tb.Text = replaceStr;

                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL);
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GetSoakingParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new SoakingSysParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(SoakingSysParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    SoakingSysParam_IParam = tmpParam;
                    SoakingSysParam_Clone = SoakingSysParam_IParam as SoakingSysParameter;
                }
                else
                {
                    LoggerManager.Error($"GetSoakingParam() : Parameter load failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangedMarginValue()
        {
            try
            {
                MakePositionData();

                MinHeight = 0;
                MaxHeight = 0;
                DiffHeight = 0;
                AvgHeight = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeMarginValue(double value)
        {
            try
            {
                ChuckEndPointMargin = value;

                ChangedMarginValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeFocusingRange(double value)
        {
            try
            {
                FocusingRange = value;
                FocusingParam.FocusRange.Value = FocusingRange;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _FocusingRangeClickCommand;
        public ICommand FocusingRangeClickCommand
        {
            get
            {
                if (null == _FocusingRangeClickCommand) _FocusingRangeClickCommand = new RelayCommand<Object>(FocusingRangeClickCommandFunc);
                return _FocusingRangeClickCommand;
            }
        }
        private void FocusingRangeClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                double oldvalue = Convert.ToDouble(tb.Text);

                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 1, 7);

                double newvalue = Convert.ToDouble(tb.Text);

                if (newvalue < 0)
                {
                    tb.Text = oldvalue.ToString();
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                    LoggerManager.Debug($"FocusingRangeClickCommandFunc() value({newvalue})is invalid");
                }
                else if(newvalue > 1000)
                {
                    tb.Text = oldvalue.ToString();
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                    LoggerManager.Debug($"FocusingRangeClickCommandFunc() value({newvalue})is invalid");
                }
                else
                {
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                    if (oldvalue != newvalue)
                    {
                        FocusingRange = newvalue;
                        FocusingParam.FocusRange.Value = FocusingRange;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public void ChangeSpecHeightValue(double value)
        //{
        //    try
        //    {
        //        SpecHeight = value;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}

        public ChuckPlanarityDataDescription GetChuckPlanarityInfo()
        {
            ChuckPlanarityDataDescription info = new ChuckPlanarityDataDescription();

            try
            {
                info.MinHeight = this.MinHeight;
                info.MaxHeight = this.MaxHeight;
                info.DiffHeight = this.DiffHeight;
                //info.SpecHeight = this.SpecHeight;
                info.AvgHeight = this.AvgHeight;
                info.ChuckEndPointMargin = this.ChuckEndPointMargin;
                info.ChuckFocusingRange = this.FocusingRange;
                //info.CurrentChuckPosEnum = this.CurrentChuckPosEnum;

                List<ChuckPos> chukcposlist = this.ChuckPosList.ToList();

                //List<BaseThing> BaseThinglist = this.Items.ToList();

                //info.ChuckPosList = SerializeManager.SerializeToByte(chukcposlist, typeof(List<ChuckPos>));
                info.ChuckPosList = chukcposlist;
                //info.Items = BaseThinglist;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return info;
        }

        public void UpdateDeviceChangeInfo(ChuckPlanarityDataDescription info)
        {
        }
    }
}

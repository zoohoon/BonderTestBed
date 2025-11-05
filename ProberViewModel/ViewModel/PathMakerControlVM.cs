using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using SharpDXRender.RenderObjectPack;
using System.Windows.Input;
using RelayCommandBase;
using Microsoft.Win32;
using WinPoint = System.Windows.Point;
using GenericUndoRedo;
using LogModule;

namespace PathMakerControlViewModel
{
    using System.Windows.Shapes;
    using System.Windows.Media;
    using System.Threading;
    using System.Timers;
    using System.Xml.Serialization;
    using GeometryHelp;
    using Newtonsoft.Json;

    public class ArcPathInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ArcPathInfo(PathMakerControlVM vm)
        {
            try
            {
                this.XRad = 5;
                this.YRad = 5;
                this.rotationAngle = 0;
                this.isLargeArcFlag = 0;
                this.sweepDirectionFlag = 1;

                this.ViewModel = vm;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private PathMakerControlVM _ViewModel;
        public PathMakerControlVM ViewModel
        {
            get { return _ViewModel; }
            set
            {
                if (value != _ViewModel)
                {
                    _ViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void UpdateArcProperty()
        {
            try
            {
                if (ViewModel != null && ViewModel.ArcUpdated == false)
                {
                    ViewModel.ArcUpdated = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private double _XRad;
        public double XRad
        {
            get { return _XRad; }
            set
            {
                if (value != _XRad)
                {
                    _XRad = value;
                    UpdateArcProperty();

                    RaisePropertyChanged();
                }
            }
        }

        private double _YRad;
        public double YRad
        {
            get { return _YRad; }
            set
            {
                if (value != _YRad)
                {
                    _YRad = value;
                    UpdateArcProperty();

                    RaisePropertyChanged();
                }
            }
        }

        private double _rotationAngle;
        public double rotationAngle
        {
            get { return _rotationAngle; }
            set
            {
                if (value != _rotationAngle)
                {
                    _rotationAngle = value;
                    UpdateArcProperty();

                    RaisePropertyChanged();
                }
            }
        }

        private int _isLargeArcFlag;
        public int isLargeArcFlag
        {
            get { return _isLargeArcFlag; }
            set
            {
                if (value != _isLargeArcFlag)
                {
                    _isLargeArcFlag = value;
                    UpdateArcProperty();

                    RaisePropertyChanged();
                }
            }
        }

        private int _sweepDirectionFlag;
        public int sweepDirectionFlag
        {
            get { return _sweepDirectionFlag; }
            set
            {
                if (value != _sweepDirectionFlag)
                {
                    _sweepDirectionFlag = value;
                    UpdateArcProperty();

                    RaisePropertyChanged();
                }
            }
        }
    }
    public class PathDataClass : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public PathDataClass()
        {
            this.Path = new Path();
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Path _Path;
        public Path Path
        {
            get { return _Path; }
            set
            {
                if (value != _Path)
                {
                    _Path = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

    [Serializable]
    public class PathParamList : List<string>, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; set; } = "PathMakerParam";

        public string FileName { get; set; } = "PathMakerParam.json";

        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public List<object> Nodes { get; set; }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public void SetElementMetaData()
        {

        }
    }

    public class PathMakerControlVM : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;


        readonly Guid _ViewModelGUID = new Guid("340bc489-5506-46cc-ac7e-919ff0103c07");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }


        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region Fields

        //private List<Point> ImagePathPoint = new List<WinPoint>();

        private char[] pathCmdChar = { 'M', 'L', 'A' };

        private double Ratio_LayerToGrabSize_X;
        private double Ratio_LayerToGrabSize_Y;

        private PathDataPool InitPath = new PathDataPool();
        private UndoRedoHistory<PathDataPool> history;

        #endregion

        #region Properties

        private IVisionManager _VisionManager;
        public IVisionManager VisionManager
        {
            get { return _VisionManager; }
            set
            {
                if (value != _VisionManager)
                {
                    _VisionManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMotionManager _MotionManager;
        public IMotionManager MotionManager
        {
            get { return _MotionManager; }
            set
            {
                if (value != _MotionManager)
                {
                    _MotionManager = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private ICamera _AssignedCamera;
        public ICamera AssignedCamera
        {
            get { return _AssignedCamera; }
            set
            {
                if (value != _AssignedCamera)
                {
                    _AssignedCamera = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ArcUpdated;
        public bool ArcUpdated
        {
            get { return _ArcUpdated; }
            set
            {
                if (value != _ArcUpdated)
                {
                    _ArcUpdated = value;
                    UpdateRealTimeArcPath();

                    RaisePropertyChanged();
                }
            }
        }


        private ArcPathInfo _ArcInfo;
        public ArcPathInfo ArcInfo
        {
            get { return _ArcInfo; }
            set
            {
                if (value != _ArcInfo)
                {
                    _ArcInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PathMakerRenderLayer _MainRenderLayer;
        public PathMakerRenderLayer MainRenderLayer
        {
            get { return _MainRenderLayer; }
            set
            {
                if (value != _MainRenderLayer)
                {
                    _MainRenderLayer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ThresholdValue;
        public double ThresholdValue
        {
            get { return _ThresholdValue; }
            set
            {
                if (value != _ThresholdValue)
                {
                    // Event

                    _ThresholdValue = value;

                    if (CroppedBuffer != null)
                    {
                        Binarization();
                    }

                    RaisePropertyChanged();
                }
            }
        }

        private ImageBuffer _CroppedBuffer;
        public ImageBuffer CroppedBuffer
        {
            get { return _CroppedBuffer; }
            set
            {
                if (value != _CroppedBuffer)
                {
                    _CroppedBuffer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ImageBuffer _MaskingBuffer;
        public ImageBuffer MaskingBuffer
        {
            get { return _MaskingBuffer; }
            set
            {
                if (value != _MaskingBuffer)
                {
                    _MaskingBuffer = value;
                    RaisePropertyChanged();
                }
            }
        }



        private ImageBuffer _LoadedImage;
        public ImageBuffer LoadedImage
        {
            get { return _LoadedImage; }
            set
            {
                if (value != _LoadedImage)
                {
                    _LoadedImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ColorDepthChecked;
        public bool? ColorDepthChecked
        {
            get { return _ColorDepthChecked; }
            set
            {
                if (value != _ColorDepthChecked)
                {
                    _ColorDepthChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _FillChecked;
        public bool? FillChecked
        {
            get { return _FillChecked; }
            set
            {
                if (value != _FillChecked)
                {
                    _FillChecked = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool? _OverlapChecked;
        public bool? OverlapChecked
        {
            get { return _OverlapChecked; }
            set
            {
                if (value != _OverlapChecked)
                {
                    _OverlapChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PathCommandIsEnabled;
        public bool PathCommandIsEnabled
        {
            get { return _PathCommandIsEnabled; }
            set
            {
                if (value != _PathCommandIsEnabled)
                {
                    _PathCommandIsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }



        private bool _StartCmdIsEnabled;
        public bool StartCmdIsEnabled
        {
            get { return _StartCmdIsEnabled; }
            set
            {
                if (value != _StartCmdIsEnabled)
                {
                    _StartCmdIsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EndCmdIsEnabled;
        public bool EndCmdIsEnabled
        {
            get { return _EndCmdIsEnabled; }
            set
            {
                if (value != _EndCmdIsEnabled)
                {
                    _EndCmdIsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PlusCmdIsEnabled;
        public bool PlusCmdIsEnabled
        {
            get { return _PlusCmdIsEnabled; }
            set
            {
                if (value != _PlusCmdIsEnabled)
                {
                    _PlusCmdIsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Path _CurrentPath;
        public Path CurrentPath
        {
            get { return _CurrentPath; }
            set
            {
                if (value != _CurrentPath)
                {
                    _CurrentPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<PathDataClass> _PathTemplate;
        public List<PathDataClass> PathTemplate
        {
            get { return _PathTemplate; }
            set
            {
                if (value != _PathTemplate)
                {
                    _PathTemplate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PathParamList _PathParam = new PathParamList();
        public PathParamList PathParam
        {
            get { return _PathParam; }
            set
            {
                if (value != _PathParam)
                {
                    _PathParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Command & Function

        private RelayCommand<PathMarkupCommandEnum> _PathDrawingCommand;
        public ICommand PathDrawingCommand
        {
            get
            {
                if (null == _PathDrawingCommand) _PathDrawingCommand = new RelayCommand<PathMarkupCommandEnum>(FuncPathDrawingCommand);
                return _PathDrawingCommand;
            }
        }

        private void FuncPathDrawingCommand(PathMarkupCommandEnum cmd)
        {
            string CommandString = null;
            string x, y;
            int count = MainRenderLayer.PathPointRenderContainer.RenderObjectList.Count;
            bool updateFlag = false;

            try
            {
                switch (cmd)
                {
                    case PathMarkupCommandEnum.Start:

                        CommandString = "M";

                        StartCmdIsEnabled = false;
                        EndCmdIsEnabled = false;
                        PlusCmdIsEnabled = true;

                        break;
                    case PathMarkupCommandEnum.Point:

                        if ((count == 1) || (count == 2))
                        {
                            if (count == 2)
                            {
                                PlusCmdIsEnabled = false;
                                PathCommandIsEnabled = true;
                            }

                            x = MainRenderLayer.PathPointRenderContainer.RenderObjectList[count - 1].CenterX.ToString();
                            y = MainRenderLayer.PathPointRenderContainer.RenderObjectList[count - 1].CenterY.ToString();

                            CommandString = x + " " + y;
                        }
                        break;

                    case PathMarkupCommandEnum.Line:

                        updateFlag = true;
                        EndCmdIsEnabled = true;

                        CommandString = "L";

                        break;

                    case PathMarkupCommandEnum.Arc:

                        updateFlag = true;
                        EndCmdIsEnabled = true;

                        CommandString = "A";

                        string arcinfostr = ArcInfo.XRad.ToString() + " "
                                        + ArcInfo.YRad.ToString() + " "
                                        + ArcInfo.rotationAngle.ToString() + " "
                                        + ArcInfo.isLargeArcFlag.ToString() + " "
                                        + ArcInfo.sweepDirectionFlag.ToString() + " ";

                        CommandString = CommandString + arcinfostr;

                        break;

                    case PathMarkupCommandEnum.End:
                        updateFlag = true;
                        CommandString = "Z";

                        EndCmdIsEnabled = false;
                        StartCmdIsEnabled = false;
                        PlusCmdIsEnabled = false;

                        break;
                    case PathMarkupCommandEnum.Clear:

                        MainRenderLayer.PathRenderContainer.RenderObjectList.Clear();
                        MainRenderLayer.PathPointRenderContainer.RenderObjectList.Clear();

                        history.Clear();
                        InitPath.datas.Clear();

                        StartCmdIsEnabled = true;
                        EndCmdIsEnabled = false;
                        PlusCmdIsEnabled = false;
                        PathCommandIsEnabled = false;

                        break;

                    default:
                        break;
                }

                if (CommandString != null)
                {
                    AddPathData(CommandString);

                    if (updateFlag == true)
                    {
                        UpdatePathData();

                        if (cmd != PathMarkupCommandEnum.End)
                        {
                            PathGeometry pg = CurrentPath.Data.GetFlattenedPathGeometry();
                            List<Point> pointsOnFlattenedPath = GeometryHelper.GetPointsOnFlattenedPath(pg);

                            //if (pointsOnFlattenedPath.Count >= 3)
                            //{
                            //    EndCmdIsEnabled = true;
                            //}
                            //else
                            //{
                            //    EndCmdIsEnabled = false;
                            //}
                        }

                        string PartialPath = GetLastPartialPath();

                        try
                        {
                            Geometry geo = Geometry.Parse(PartialPath);

                            PathGeometry partialpg = geo.GetFlattenedPathGeometry();
                            List<Point> pts = GeometryHelper.GetPointsOnFlattenedPath(partialpg);
                            //List<Point> pts = ChangeLayerToImageCoord(pointsOnFlattenedPath);

                            RenderGeometry renderObj = new RenderGeometry();
                            renderObj.SetGeometryPoint(pts);
                            renderObj.IsVisible = true;
                            renderObj.StrokeWidth = 1;
                            renderObj.Color = "Yellow";

                            MainRenderLayer.PathRenderContainer.RenderObjectList.Add(renderObj);
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }

                        //RenderEllipse renderObj = new RenderEllipse(MainRenderLayer.Cursor.CenterX, MainRenderLayer.Cursor.CenterY,
                        //                                      MainRenderLayer.EllipseWidth, MainRenderLayer.EllipseHeight,
                        //                                      "Yellow", 1);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string GetLastPartialPath()
        {
            string PartialPath = null;
            string str1;
            string str2;

            int ret1, ret2;
            int loopinterval = 2;

            try
            {
                for (int i = 0; i < InitPath.datas.Count; i += loopinterval)
                {
                    if (i < InitPath.datas.Count - 1)
                    {
                        str1 = InitPath.datas[i];
                        str2 = InitPath.datas[i + 1];

                        ret1 = str1.LastIndexOfAny(pathCmdChar);
                        ret2 = str2.LastIndexOfAny(pathCmdChar);

                        if (ret1 >= 0 && ret2 < 0)
                        {
                            PartialPath = PartialPath + str1 + str2;
                        }
                        else if (ret1 < 0 && ret2 >= 0)
                        {
                            PartialPath = PartialPath + str2 + str1;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        // Z

                        str1 = InitPath.datas[i];

                        // Last Line 

                        if (str1 == "Z")
                        {
                            str2 = InitPath[1];

                            PartialPath = PartialPath + "L" + str2 + str1;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return PartialPath;
        }

        private RelayCommand<object> _PathPlusCommand;
        public ICommand PathPlusCommand
        {
            get
            {
                if (null == _PathPlusCommand) _PathPlusCommand = new RelayCommand<object>(FuncPathPlusCommand);
                return _PathPlusCommand;
            }
        }

        private void FuncPathPlusCommand(object obj)
        {
            try
            {
                string x = MainRenderLayer.Cursor.CenterX.ToString("R");
                string y = MainRenderLayer.Cursor.CenterY.ToString("R");

                RenderEllipse renderObj = new RenderEllipse(MainRenderLayer.Cursor.CenterX, MainRenderLayer.Cursor.CenterY,
                                                              MainRenderLayer.EllipseWidth, MainRenderLayer.EllipseHeight, "Yellow",
                                                              1);

                MainRenderLayer.PathRenderContainer.RenderObjectList.Add(renderObj);

                if (MainRenderLayer.PathPointRenderContainer.RenderObjectList.Count < 2)
                {
                    RenderEllipse renderObj2 = new RenderEllipse(MainRenderLayer.Cursor.CenterX, MainRenderLayer.Cursor.CenterY,
                                                              MainRenderLayer.EllipseWidth, MainRenderLayer.EllipseHeight, "Red",
                                                              1);

                    MainRenderLayer.PathPointRenderContainer.RenderObjectList.Add(renderObj2);

                    FuncPathDrawingCommand(PathMarkupCommandEnum.Point);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<CursorEnum> _CursorMoveCommand;
        public ICommand CursorMoveCommand
        {
            get
            {
                if (null == _CursorMoveCommand) _CursorMoveCommand = new RelayCommand<CursorEnum>(FuncCursorMoveCommand);
                return _CursorMoveCommand;
            }
        }

        private void FuncCursorMoveCommand(CursorEnum cmd)
        {
            try
            {
                MainRenderLayer.CursorMove(cmd);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _LoadOriImageCommand;
        public ICommand LoadOriImageCommand
        {
            get
            {
                if (null == _LoadOriImageCommand) _LoadOriImageCommand = new RelayCommand<object>(FuncLoadOriImageCommand);
                return _LoadOriImageCommand;
            }
        }

        private void FuncLoadOriImageCommand(object obj)
        {
            try
            {
                if (LoadedImage != null)
                {
                    this.VisionManager().ImageGrabbed(AssignedCamera.GetChannelType(), LoadedImage);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DrawingCommand;
        public ICommand DrawingCommand
        {
            get
            {
                if (null == _DrawingCommand) _DrawingCommand = new RelayCommand<object>(FuncDrawingCommand);
                return _DrawingCommand;
            }
        }

        private void FuncDrawingCommand(object obj)
        {
            try
            {
                //PathGeometry pg1 = CurrentPath.Data.GetOutlinedPathGeometry();
                PathGeometry pg = CurrentPath.Data.GetFlattenedPathGeometry();

                List<Point> pointsOnFlattenedPath = GeometryHelper.GetPointsOnFlattenedPath(pg);

                List<Point> pt = ChangeLayerToImageCoord(pointsOnFlattenedPath);

                ColorDept color;

                if (ColorDepthChecked == true)
                {
                    color = ColorDept.Color24;
                }
                else
                {
                    color = ColorDept.BlackAndWhite;
                }

                ImageBuffer image = this.VisionManager().SingleGrab(EnumProberCam.WAFER_LOW_CAM, this);

                ImageBuffer img = this.VisionManager().VisionProcessing.Algorithmes.DrawingShape
                    (
                        image,
                        pt,
                        color,
                        FillChecked,
                        OverlapChecked
                    );

                this.VisionManager().ImageGrabbed(AssignedCamera.GetChannelType(), LoadedImage);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _CropCommand;
        public ICommand CropCommand
        {
            get
            {
                if (null == _CropCommand) _CropCommand = new RelayCommand<object>(FuncCropCommand);
                return _CropCommand;
            }
        }

        private void FuncCropCommand(object obj)
        {
            double left;
            double top;
            double width;
            double height;

            try
            {

                left = CurrentPath.Data.Bounds.Left / Ratio_LayerToGrabSize_X;
                top = CurrentPath.Data.Bounds.Top / Ratio_LayerToGrabSize_Y;
                width = CurrentPath.Data.Bounds.Width / Ratio_LayerToGrabSize_X;
                height = CurrentPath.Data.Bounds.Height / Ratio_LayerToGrabSize_Y;

                ImageBuffer image = this.VisionManager().SingleGrab(EnumProberCam.WAFER_LOW_CAM, this);

                ImageBuffer img = this.VisionManager().VisionProcessing.Algorithmes.CropImage
                    (
                        image, (int)left, (int)top, (int)width, (int)height
                    );

                if (img != null)
                {
                    CroppedBuffer = img;
                }

                this.VisionManager().ImageGrabbed(AssignedCamera.GetChannelType(), LoadedImage);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _GetThresoldCommand;
        public ICommand GetThresoldCommand
        {
            get
            {
                if (null == _GetThresoldCommand) _GetThresoldCommand = new RelayCommand<object>(FuncThresholdCommand);
                return _GetThresoldCommand;
            }
        }

        private void FuncThresholdCommand(object obj)
        {
            double threshold;

            try
            {

                if ((CroppedBuffer != null) && (MaskingBuffer != null))
                {
                    threshold = this.VisionManager().VisionProcessing.Algorithmes.GetThreshold_ProbeMark(CroppedBuffer, MaskingBuffer, 0);

                    ThresholdValue = threshold;
                }
                else
                {
                    // Can't Calculate
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _MaskingCommand;
        public ICommand MaskingCommand
        {
            get
            {
                if (null == _MaskingCommand) _MaskingCommand = new RelayCommand<object>(FuncMaskingCommand);
                return _MaskingCommand;
            }
        }

        private void FuncMaskingCommand(object obj)
        {
            try
            {
                if (CroppedBuffer != null)
                {
                    //string FullPath = GetImageFullPath();

                    //System.Windows.Media.Geometry g = System.Windows.Media.Geometry.Parse(FullPath);
                    //PathGeometry pg = g.GetFlattenedPathGeometry();
                    //List<Point> pointsOnFlattenedPath = GeometryHelper.GetPointsOnFlattenedPath(pg);

                    PathGeometry pg = CurrentPath.Data.GetFlattenedPathGeometry();
                    List<Point> pointsOnFlattenedPath = GeometryHelper.GetPointsOnFlattenedPath(pg, -CurrentPath.Data.Bounds.Left, -CurrentPath.Data.Bounds.Top);
                    List<Point> pt = ChangeLayerToImageCoord(pointsOnFlattenedPath);

                    ImageBuffer image = this.VisionManager().VisionProcessing.Algorithmes.MaskingBuffer(CroppedBuffer, pt);

                    if (image != null)
                    {
                        MaskingBuffer = image;
                    }
                }

                //double len = 0;
                //Point pt, ptTan;
                //double lengthInterval = 24;

                //for (int c = 0; c < lengthInterval; ++c)
                //{
                //    len += (double)(1 / lengthInterval);
                //    pg1.GetPointAtFractionLength(len, out pt, out ptTan);
                //    RenderRectangle rect = new RenderRectangle((float)pt.X, (float)pt.Y, 2, 2, "OpaqueCyan");
                //    //rect.Height = rect.Width = 2;
                //    //this.mainCanvas.Children.Add(rect);

                //    MainRenderLayer.TestRenderContainer.RenderObjectList.Add(rect);

                //    //Canvas.SetTop(rect, pt.Y);
                //    //Canvas.SetLeft(rect, pt.X);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _FileLoadCommand;
        public ICommand FileLoadCommand
        {
            get
            {
                if (null == _FileLoadCommand) _FileLoadCommand = new RelayCommand<object>(FuncFileLoadCommand);
                return _FileLoadCommand;
            }
        }
        private void FuncFileLoadCommand(object noparam)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.DefaultExt = ".bmp";
                dlg.Filter = "bmp files(*.bmp)|*.bmp|All files(*.*)|*.*";

                var rel = dlg.ShowDialog();

                if (rel == true)
                {
                    List<ImageBuffer> imgs = new List<ImageBuffer>();

                    imgs.Add(this.VisionManager().LoadImageFile(dlg.FileName));

                    this.VisionManager().StartGrab(AssignedCamera.GetChannelType(), this);
                    this.VisionManager().DigitizerService[AssignedCamera.GetDigitizerIndex()].GrabberService.LoadUserImageFiles(imgs);

                    if (imgs.Count > 0)
                    {
                        LoadedImage = imgs[0];
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region Methods

        private List<Point> ChangeLayerToImageCoord(List<Point> list)
        {
            List<Point> ret = new List<WinPoint>();

            try
            {
                foreach (var item in list)
                {
                    double x, y;

                    x = (item.X / Ratio_LayerToGrabSize_X);
                    y = (item.Y / Ratio_LayerToGrabSize_Y);

                    Point tmp = new WinPoint(x, y);
                    ret.Add(tmp);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private void Binarization()
        {
            try
            {
                ImageBuffer Image = this.VisionManager().VisionProcessing.Algorithmes.Binarization(CroppedBuffer, ThresholdValue);

                this.VisionManager().ImageGrabbed(AssignedCamera.GetChannelType(), LoadedImage);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void AddPathData(string data)
        {
            try
            {
                string ret = null;

                int containChar = data.LastIndexOfAny(pathCmdChar);

                if (containChar < 0)
                {
                    ret = InitPath.datas.Find(t => t.ToString() == data);
                }

                if (ret == null)
                {
                    history.Do(new AddStringMemento());
                    InitPath.Add(data);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpdatePathData(bool arcUpdate = false)
        {
            try
            {

                string FullPath = null;
                string str1, str2;
                int ret1, ret2;
                int loopinterval = 2;

                string lastPath = CurrentPath.Data.ToString();
                string lastCommand = InitPath.datas[InitPath.datas.Count - 1];

                string tmpStartData;
                string tmpEndData;

                for (int i = 0; i < InitPath.datas.Count; i += loopinterval)
                {
                    if (i < InitPath.datas.Count - 1)
                    {
                        str1 = InitPath.datas[i];
                        str2 = InitPath.datas[i + 1];

                        ret1 = str1.LastIndexOfAny(pathCmdChar);
                        ret2 = str2.LastIndexOfAny(pathCmdChar);

                        if (ret1 >= 0 && ret2 < 0)
                        {
                            FullPath = FullPath + str1 + str2;
                        }
                        else if (ret1 < 0 && ret2 >= 0)
                        {
                            if (arcUpdate == true)
                            {
                                if (i + 1 == InitPath.datas.Count - 1)
                                {
                                    string arcinfostr = ArcInfo.XRad.ToString() + " "
                                                    + ArcInfo.YRad.ToString() + " "
                                                    + ArcInfo.rotationAngle.ToString() + " "
                                                    + ArcInfo.isLargeArcFlag.ToString() + " "
                                                    + ArcInfo.sweepDirectionFlag.ToString() + " ";

                                    InitPath.datas[i + 1] = "A" + arcinfostr;
                                    str2 = InitPath.datas[i + 1];
                                }
                                // Check Last Arc 
                                // If OK, Change String InitPath 
                            }

                            FullPath = FullPath + str2 + str1;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        // Z ?

                        str1 = InitPath.datas[i];

                        // Last Line 

                        if (str1 == "Z")
                        {
                            str2 = InitPath[1];

                            FullPath = FullPath + "L" + str2 + str1;
                        }


                    }
                }

                //if (lastCommand == "Z")
                //{
                //    FullPath = lastPath + lastCommand;
                //}
                //else
                //{
                //    for (int i = 0; i < InitPath.datas.Count; i += loopinterval)
                //    {
                //        str1 = InitPath.datas[i];
                //        str2 = InitPath.datas[i + 1];

                //        ret1 = str1.LastIndexOfAny(pathCmdChar);
                //        ret2 = str2.LastIndexOfAny(pathCmdChar);

                //        if (ret1 >= 0 && ret2 < 0)
                //        {
                //            FullPath = FullPath + str1 + str2;
                //        }
                //        else if (ret1 < 0 && ret2 >= 0)
                //        {
                //            if (arcUpdate == true)
                //            {
                //                if (i + 1 == InitPath.datas.Count - 1)
                //                {
                //                    string arcinfostr = ArcInfo.XRad.ToString() + " "
                //                                    + ArcInfo.YRad.ToString() + " "
                //                                    + ArcInfo.rotationAngle.ToString() + " "
                //                                    + ArcInfo.isLargeArcFlag.ToString() + " "
                //                                    + ArcInfo.sweepDirectionFlag.ToString() + " ";

                //                    InitPath.datas[i + 1] = "A" + arcinfostr;
                //                    str2 = InitPath.datas[i + 1];
                //                }
                //                // Check Last Arc 
                //                // If OK, Change String InitPath 
                //            }

                //            FullPath = FullPath + str2 + str1;
                //        }
                //        else
                //        {

                //        }
                //    }
                //}

                if (FullPath != null)
                {
                    if (FullPath != "M")
                    {
                        Geometry g = Geometry.Parse(FullPath);
                        CurrentPath.Data = g;

                        if (MainRenderLayer.PathPointRenderContainer.RenderObjectList.Count > 0)
                        {
                            MainRenderLayer.PathPointRenderContainer.RenderObjectList.RemoveAt(0);

                            PlusCmdIsEnabled = true;
                            PathCommandIsEnabled = false;
                        }
                    }
                }
                else
                {
                    CurrentPath.Data = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    VisionManager = this.VisionManager();
                    MotionManager = this.MotionManager();
                    StageSupervisor = this.StageSupervisor();

                    AssignedCamera = VisionManager.GetCam(EnumProberCam.WAFER_LOW_CAM);

                    Size size = new Size();

                    size.Width = AssignedCamera.GetGrabSizeWidth();
                    size.Height = AssignedCamera.GetGrabSizeHeight();

                    MainRenderLayer = new PathMakerRenderLayer(new Size(940, 940));
                    MainRenderLayer.Init();

                    Ratio_LayerToGrabSize_X = MainRenderLayer.LayerSize.Width / AssignedCamera.GetGrabSizeWidth();
                    Ratio_LayerToGrabSize_Y = MainRenderLayer.LayerSize.Height / AssignedCamera.GetGrabSizeHeight();

                    //ArcInfo.Update = true;

                    //Geometry g = Geometry.Parse(< your string >);
                    //RangeSignalPath1.Data = g;

                    //Path_data = "M 0 0 L 5 0 L 5 -5 L 10 -5 L 10 0 L 15 0 L 15 5 L 10 5 L 10 10 L 5 10 L 5 5 L 0 5 Z";

                    //System.Windows.Media.Geometry g = System.Windows.Media.Geometry.Parse(Path_data);

                    //CurrentPath = new System.Windows.Shapes.Path();

                    //CurrentPath.Stretch = System.Windows.Media.Stretch.Uniform;
                    //CurrentPath.Fill = Brushes.Transparent;
                    //CurrentPath.Data = g;

                    history = new UndoRedoHistory<PathDataPool>(InitPath);

                    InitBindingProperties();

                    // Test Load Param
                    retval = LoadPathMakerControlVMParam();

                    if (PathTemplate != null && PathTemplate.Count > 0)
                    {
                        if (CurrentPath == null)
                        {
                            CurrentPath = new Path();
                        }

                        CurrentPath = PathTemplate[0].Path;
                    }

                    ArcInfo = new ArcPathInfo(this);

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void UpdateRealTimeArcPath()
        {
            try
            {
                if (ArcUpdated == true)
                {
                    if (InitPath != null && InitPath.datas.Count > 0)
                    {
                        bool ret = InitPath.datas[InitPath.datas.Count - 1].Contains("A");

                        if (ret == true)
                        {
                            UpdatePathData(true);
                        }

                        string PartialPath = GetLastPartialPath();

                        Geometry geo = Geometry.Parse(PartialPath);

                        PathGeometry partialpg = geo.GetFlattenedPathGeometry();
                        List<Point> pts = GeometryHelper.GetPointsOnFlattenedPath(partialpg);
                        //List<Point> pts = ChangeLayerToImageCoord(pointsOnFlattenedPath);

                        RenderGeometry lastobj = (RenderGeometry)MainRenderLayer.PathRenderContainer.RenderObjectList.Last();
                        lastobj.SetGeometryPoint(pts);
                    }

                    ArcUpdated = false;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }


        private void InitBindingProperties()
        {
            OverlapChecked = false;
            FillChecked = false;
            ColorDepthChecked = false;

            // IsEnabled
            PathCommandIsEnabled = false;
            StartCmdIsEnabled = true;
            EndCmdIsEnabled = false;
            PlusCmdIsEnabled = false;

            ArcUpdated = false;
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                VisionManager.StartGrab(AssignedCamera.GetChannelType(), this);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        private Path MakePathData(string str)
        {
            Path path = new Path();

            try
            {
                System.Windows.Media.Geometry g = System.Windows.Media.Geometry.Parse(str);

                path.Data = g;
                path.Stretch = System.Windows.Media.Stretch.Uniform;
                path.Fill = Brushes.Transparent;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
         
            return path;
        }

        private void SetDefaultParam()
        {
            string data1 = "M 0 0 L 5 0 L 5 -5 L 10 -5 L 10 0 L 15 0 L 15 5 L 10 5 L 10 10 L 5 10 L 5 5 L 0 5 Z";
            string data2 = "M 0 0 A 100 50 0 1 1 100 50";

            try
            {
                PathParam.Add(data1);
                PathParam.Add(data2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadPathMakerControlVMParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            object deserializedObj;

            string FullPath = this.FileManager().GetSystemParamFullPath(PathParam.FilePath, PathParam.FileName);

            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                IParam tmpParam = null;
                tmpParam = new PathParamList();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(PathParamList), null, FullPath);

                if (retVal == EventCodeEnum.NONE)
                {
                    PathParam = tmpParam as PathParamList;
                }

                if (PathTemplate == null)
                {
                    PathTemplate = new List<PathDataClass>();
                }

                foreach (var item in PathParam)
                {
                    PathDataClass tmp = new PathDataClass();

                    tmp.Name = "Unknown";
                    tmp.Path = MakePathData(item);

                    PathTemplate.Add(tmp);
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                RetVal = EventCodeEnum.PARAM_ERROR;
            }

            return RetVal;
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

        #endregion
    }
}

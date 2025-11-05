using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapView
{
    using DXControlBase;
    using SharpDX;
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.Mathematics.Interop;
    using System.ComponentModel;
    using System.Windows;
    using System.Collections.ObjectModel;
    using ProberInterfaces;
    using SubstrateObjects;
    using System.Runtime.CompilerServices;
    using LogModule;
    using OutlinedTextBlockControl;
    using System.Windows.Data;
    using RelayCommandBase;
    using System.Windows.Input;
    using LoaderBase.Communication;
    using ProbingDataInterface;
    using ProberInterfaces.Enum;

    public class MapViewControl : D2dControl, INotifyPropertyChanged, IMapViewControl
    {
        #region PropertyChanged


        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty RecSizeProperty = DependencyProperty.Register("RecSize", typeof(float), typeof(MapViewControl), new FrameworkPropertyMetadata((float)0));
        public float RecSize
        {
            get { return (float)this.GetValue(RecSizeProperty); }
            set { this.SetValue(RecSizeProperty, value); }
        }

        public static readonly DependencyProperty StageSyncEnalbeProperty = DependencyProperty.Register("StageSyncEnalbe", typeof(bool), typeof(MapViewControl), new FrameworkPropertyMetadata((bool)true));
        public bool StageSyncEnalbe
        {
            get { return (bool)this.GetValue(StageSyncEnalbeProperty); }
            set { this.SetValue(StageSyncEnalbeProperty, value); }
        }

        public static readonly DependencyProperty MapViewEncoderVisiabilityProperty = DependencyProperty.Register("MapViewEncoderVisiability", typeof(bool), typeof(MapViewControl), new FrameworkPropertyMetadata((bool)false));
        /// <summary>
        /// MapView 오른쪽 하단에 Motion Encoder표시할지 안할지. (True: 표시함, False :표시안함)
        /// </summary>
        public bool MapViewEncoderVisiability
        {
            get { return (bool)this.GetValue(MapViewEncoderVisiabilityProperty); }
            set { this.SetValue(MapViewEncoderVisiabilityProperty, value); }
        }

        public static readonly DependencyProperty BINTypeProperty = DependencyProperty.Register("BINType", typeof(BinType), typeof(MapViewControl), new FrameworkPropertyMetadata(BinType.BIN_PASSFAIL));

        public BinType BINType
        {
            get { return (BinType)this.GetValue(BINTypeProperty); }
            set { this.SetValue(BINTypeProperty, value); }
        }

        public static readonly DependencyProperty LoaderCommunicationManagerProperty = DependencyProperty.Register("LoaderCommunicationManager", typeof(ILoaderCommunicationManager), typeof(MapViewControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(LoaderCommunicationManagerPropertyChanged)));
        public ILoaderCommunicationManager LoaderCommunicationManager
        {
            get { return (ILoaderCommunicationManager)this.GetValue(LoaderCommunicationManagerProperty); }
            set { this.SetValue(LoaderCommunicationManagerProperty, value); }
        }

        public static readonly DependencyProperty CurCameraProperty = DependencyProperty.Register("CurCamera", typeof(ICamera), typeof(MapViewControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(CurCamPropertyChanged)));
        public ICamera CurCamera
        {
            get { return (ICamera)this.GetValue(CurCameraProperty); }
            set { this.SetValue(CurCameraProperty, value); }
        }

        public static readonly DependencyProperty DutObjectProperty = DependencyProperty.Register("DutObject", typeof(IProbeCard), typeof(MapViewControl), new FrameworkPropertyMetadata(null));
        public IProbeCard DutObject
        {
            get { return (IProbeCard)this.GetValue(DutObjectProperty); }
            set
            {
                this.SetValue(DutObjectProperty, value);
            }
        }

        public static readonly DependencyProperty WaferObjectProperty = DependencyProperty.Register("WaferObject", typeof(IWaferObject), typeof(MapViewControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(WaferObjectPropertyChanged)));
        public IWaferObject WaferObject
        {
            get { return (IWaferObject)this.GetValue(WaferObjectProperty); }
            set
            {
                this.SetValue(WaferObjectProperty, value);
                Wafer = (WaferObject)WaferObject;
            }
        }

        public static readonly DependencyProperty CoordinateManagerProperty = DependencyProperty.Register("CoordinateManager", typeof(ICoordinateManager), typeof(MapViewControl), null);
        public ICoordinateManager CoordinateManager
        {
            get { return (ICoordinateManager)this.GetValue(CoordinateManagerProperty); }
            set { this.SetValue(CoordinateManagerProperty, value); }
        }

        public static readonly DependencyProperty MaxXCntProperty = DependencyProperty.Register("MaxXCnt", typeof(int), typeof(MapViewControl), new FrameworkPropertyMetadata((int)0));
        public int MaxXCnt
        {
            get { return (int)this.GetValue(MaxXCntProperty); }
            set { this.SetValue(MaxXCntProperty, value); }
        }

        public static readonly DependencyProperty MaxYCntProperty = DependencyProperty.Register("MaxYCnt", typeof(int), typeof(MapViewControl), new FrameworkPropertyMetadata((int)0));
        public int MaxYCnt
        {
            get { return (int)this.GetValue(MaxYCntProperty); }
            set { this.SetValue(MaxYCntProperty, value); }
        }

        public static readonly DependencyProperty StepLabelProperty = DependencyProperty.Register("StepLabel", typeof(string), typeof(MapViewControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(StepLabelPropertyChanged)));
        public string StepLabel
        {
            get { return (string)this.GetValue(StepLabelProperty); }
            set { this.SetValue(StepLabelProperty, value); }
        }

        public static readonly DependencyProperty CursorXIndexProperty = DependencyProperty.Register("CursorXIndex", typeof(int), typeof(MapViewControl), new FrameworkPropertyMetadata((int)0, new PropertyChangedCallback(CursorXIndexPropertyChanged)));
        public int CursorXIndex
        {
            get { return (int)this.GetValue(CursorXIndexProperty); }
            set { this.SetValue(CursorXIndexProperty, value); }
        }

        public static readonly DependencyProperty CursorYIndexProperty = DependencyProperty.Register("CursorYIndex", typeof(int), typeof(MapViewControl), new FrameworkPropertyMetadata((int)0, new PropertyChangedCallback(CursorYIndexPropertyChanged)));
        public int CursorYIndex
        {
            get { return (int)this.GetValue(CursorYIndexProperty); }
            set { this.SetValue(CursorYIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrXYIndexProperty = DependencyProperty.Register(nameof(CurrXYIndex), typeof(System.Windows.Point), typeof(MapViewControl), new FrameworkPropertyMetadata(new System.Windows.Point(0, 0), new PropertyChangedCallback(CurrXYIndexPropertyChanged)));

        public System.Windows.Point CurrXYIndex
        {
            get { return (System.Windows.Point)this.GetValue(CurrXYIndexProperty); }
            set { this.SetValue(CurrXYIndexProperty, value); }
        }


        public static readonly DependencyProperty RenderModeProperty = DependencyProperty.Register("RenderMode", typeof(MapViewMode), typeof(MapViewControl), new FrameworkPropertyMetadata(MapViewMode.MapMode));
        public MapViewMode RenderMode
        {
            get { return (MapViewMode)this.GetValue(RenderModeProperty); }
            set { this.SetValue(RenderModeProperty, value); }
        }

        public static readonly DependencyProperty CurrXSubIndexProperty = DependencyProperty.Register("CurrXSubIndex", typeof(int), typeof(MapViewControl), new FrameworkPropertyMetadata((int)0));
        public int CurrXSubIndex
        {
            get { return (int)this.GetValue(CurrXSubIndexProperty); }
            set { this.SetValue(CurrXSubIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrYSubIndexProperty = DependencyProperty.Register("CurrYSubIndex", typeof(int), typeof(MapViewControl), new FrameworkPropertyMetadata((int)0));
        public int CurrYSubIndex
        {
            get { return (int)this.GetValue(CurrYSubIndexProperty); }
            set { this.SetValue(CurrYSubIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedXIndexProperty = DependencyProperty.Register("SelectedXIndex", typeof(int), typeof(MapViewControl),
             new FrameworkPropertyMetadata((int)0));
        public int SelectedXIndex
        {
            get { return (int)this.GetValue(SelectedXIndexProperty); }
            set { this.SetValue(SelectedXIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedYIndexProperty = DependencyProperty.Register("SelectedYIndex", typeof(int), typeof(MapViewControl), new FrameworkPropertyMetadata((int)0));
        public int SelectedYIndex
        {
            get { return (int)this.GetValue(SelectedYIndexProperty); }
            set { this.SetValue(SelectedYIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrBinCodeProperty = DependencyProperty.Register("CurrBinCode", typeof(int), typeof(MapViewControl), new FrameworkPropertyMetadata((int)0));
        public int CurrBinCode
        {
            get { return (int)this.GetValue(CurrBinCodeProperty); }
            set { this.SetValue(CurrBinCodeProperty, value); }
        }

        public static readonly DependencyProperty PointedDieProperty = DependencyProperty.Register("PointedDie", typeof(IDeviceObject), typeof(MapViewControl), new FrameworkPropertyMetadata(null));
        public IDeviceObject PointedDie
        {
            get { return (IDeviceObject)this.GetValue(PointedDieProperty); }
            set { this.SetValue(PointedDieProperty, value); }
        }

        public static readonly DependencyProperty IsMiniMapVisibleProperty = DependencyProperty.Register("IsMiniMapVisible", typeof(bool), typeof(MapViewControl), new FrameworkPropertyMetadata(false));
        public bool IsMiniMapVisible
        {
            get { return (bool)this.GetValue(IsMiniMapVisibleProperty); }
            set { this.SetValue(IsMiniMapVisibleProperty, value); }
        }


        public static readonly DependencyProperty IsDutVisibleProperty = DependencyProperty.Register("IsDutVisible", typeof(bool), typeof(MapViewControl), new FrameworkPropertyMetadata(true));
        public bool IsDutVisible
        {
            get { return (bool)this.GetValue(IsDutVisibleProperty); }
            set { this.SetValue(IsDutVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsCrossLineVisibleProperty = DependencyProperty.Register("IsCrossLineVisible", typeof(bool), typeof(MapViewControl), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(IsCrossLineVisiblePropertyChanged)));
        public bool IsCrossLineVisible
        {
            get { return (bool)this.GetValue(IsCrossLineVisibleProperty); }
            set { this.SetValue(IsCrossLineVisibleProperty, value); }
        }


        public static readonly DependencyProperty SelectedDieProperty = DependencyProperty.Register("SelectedDie", typeof(ObservableCollection<DeviceObject>), typeof(MapViewControl), new FrameworkPropertyMetadata(new ObservableCollection<DeviceObject>()));
        public ObservableCollection<DeviceObject> SelectedDie
        {
            get { return (ObservableCollection<DeviceObject>)this.GetValue(SelectedDieProperty); }
            set { this.SetValue(SelectedDieProperty, value); }
        }

        public static readonly DependencyProperty UnderDutDicesProperty = DependencyProperty.Register("UnderDutDices", typeof(ObservableCollection<IDeviceObject>), typeof(MapViewControl), new FrameworkPropertyMetadata(new ObservableCollection<IDeviceObject>()));
        public ObservableCollection<IDeviceObject> UnderDutDices
        {
            get { return (ObservableCollection<IDeviceObject>)this.GetValue(UnderDutDicesProperty); }
            set { this.SetValue(UnderDutDicesProperty, value); }
        }

        public static readonly DependencyProperty MultiDevVisibleProperty = DependencyProperty.Register("MultiDevVisible", typeof(bool), typeof(MapViewControl), new FrameworkPropertyMetadata(false));
        public bool MultiDevVisible
        {
            get { return (bool)this.GetValue(MultiDevVisibleProperty); }
            set { this.SetValue(MultiDevVisibleProperty, value); }
        }

        public static readonly DependencyProperty EnalbeClickToMoveProperty = DependencyProperty.Register("EnalbeClickToMove", typeof(bool), typeof(MapViewControl), new FrameworkPropertyMetadata(true));
        public bool EnalbeClickToMove
        {
            get { return (bool)this.GetValue(EnalbeClickToMoveProperty); }
            set { this.SetValue(EnalbeClickToMoveProperty, value); }
        }

        public static readonly DependencyProperty HighlightDiesProperty = DependencyProperty.Register("HighlightDies", typeof(AsyncObservableCollection<HighlightDieComponent>), typeof(MapViewControl), new FrameworkPropertyMetadata(new AsyncObservableCollection<HighlightDieComponent>()));
        public AsyncObservableCollection<HighlightDieComponent> HighlightDies
        {
            get { return (AsyncObservableCollection<HighlightDieComponent>)this.GetValue(HighlightDiesProperty); }
            set { this.SetValue(HighlightDiesProperty, value); }
        }

        #endregion

        #region Properties

        private WaferObject Wafer;
        private float _MaxZoomLevel;

        public float MaxZoomLevel
        {
            get { return _MaxZoomLevel; }
            set { _MaxZoomLevel = value; }
        }

        private float _MinZoomLevel;

        public float MinZoomLevel
        {
            get { return _MinZoomLevel; }
            set { _MinZoomLevel = value; }
        }

        private float _ZoomLevel;

        public float ZoomLevel
        {
            get { return _ZoomLevel; }
            set { _ZoomLevel = value; }
        }

        private float _rectWidth;

        public float RectWidth
        {
            get { return _rectWidth; }
            set { _rectWidth = value; }
        }

        private float _rectHeight;

        public float RectHeight
        {
            get { return _rectHeight; }
            set { _rectHeight = value; }
        }

        private int _downXIdx;
        public int DownXIdx
        {
            get { return _downXIdx; }
            set { _downXIdx = value; }
        }

        private int _downYIdx;

        public int DownYIdx
        {
            get { return _downYIdx; }
            set { _downYIdx = value; }
        }
        private int _downXSubIdx;
        public int DownXSubIdx
        {
            get { return _downXSubIdx; }
            set { _downXSubIdx = value; }
        }

        private int _downYSubIdx;

        public int DownYSubIdx
        {
            get { return _downYSubIdx; }
            set { _downYSubIdx = value; }
        }

        private double _WaferRadiusInPixel;

        public double WaferRadiusInPixel
        {
            get { return _WaferRadiusInPixel; }
            set
            {
                _WaferRadiusInPixel = value;

                if (ActualWidth < WaferRadiusInPixel * 1.2)
                {
                    this.IsMiniMapVisible = true;
                }
                else
                {
                    this.IsMiniMapVisible = false;
                }
            }
        }

        private float _MinimapWidth = 120;
        private float _MinimapHeight = 120;

        static object lockCamType = new object();

        public System.Windows.Point MouseDownPos { get; set; }
        public float GutterSize { get; set; }
        public bool IsMouseDown { get; set; }
        public bool IsMouseDownMove { get; set; }

        //List<MachineIndex> DrawIndexs = new List<MachineIndex>();

        private bool _changeindex;
        public bool changeindex
        {
            get { return _changeindex; }
            set
            {
                if (value != _changeindex)
                {
                    _changeindex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PreXIndex = 0;
        private int _PreYIndex = 0;

        private int _CurrXIndex;
        public int CurrXIndex
        {
            get { return _CurrXIndex; }
            set
            {
                if (value != _CurrXIndex)
                {
                    _CurrXIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurrYIndex;
        public int CurrYIndex
        {
            get { return _CurrYIndex; }
            set
            {
                if (value != _CurrYIndex)
                {
                    _CurrYIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool initflag = false;

        private bool InitIsCrossLineVisible = true;

        private List<IDeviceObject> _UnderDutDices;

        public Dictionary<int, System.Windows.Media.Color> BinColors { get; set; }

        private OutlinedTextBlock TextBlock;

        Dictionary<int, SolidColorBrush> dictBinCodeSolidBrush;
        float umPerPixel;

        private ProbeAxisObject _AxisXPos;
        public ProbeAxisObject AxisXPos
        {
            get { return _AxisXPos; }
            set
            {
                if (value != _AxisXPos)
                {
                    _AxisXPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _AxisYPos;
        public ProbeAxisObject AxisYPos
        {
            get { return _AxisYPos; }
            set
            {
                if (value != _AxisYPos)
                {
                    _AxisYPos = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ProbeAxisObject _AxisZPos;
        public ProbeAxisObject AxisZPos
        {
            get { return _AxisZPos; }
            set
            {
                if (value != _AxisZPos)
                {
                    _AxisZPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _AxisPZPos;
        public ProbeAxisObject AxisPZPos
        {
            get { return _AxisPZPos; }
            set
            {
                if (value != _AxisPZPos)
                {
                    _AxisPZPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _AxisTPos;
        public ProbeAxisObject AxisTPos
        {
            get { return _AxisTPos; }
            set
            {
                if (value != _AxisTPos)
                {
                    _AxisTPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private float _PreZoomLevel = 0;
        private bool isMinimapClicked = false;

        TransformedGeometry tfGeometry;

        RawVector2 geometryStartPosition = new RawVector2();
        RawVector2 miniMapStartPosition;
        RawVector2 waferCenPos;

        static SharpDX.DirectWrite.Factory fontFactory = new SharpDX.DirectWrite.Factory();

        TextFormat textFormat = new TextFormat(fontFactory, "Segoe UI", 12.0f);
        TextFormat uiTextFormat = new TextFormat(fontFactory, "Segoe UI", SharpDX.DirectWrite.FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, 20.0f);
        List<TextFormat> BINTextFormats = new List<TextFormat>();

        TextFormatForBinData TextFormatsForBin { get; set; }

        RawVector2 TrianglePos1 = new RawVector2();
        RawVector2 TrianglePos2 = new RawVector2();

        ArcSegment arc1 = new ArcSegment();
        ArcSegment arc2 = new ArcSegment();
        ArcSegment arc3 = new ArcSegment();
        Ellipse notchellipse = new Ellipse();
        Rectangle notchrectangle = new Rectangle();
        RawVector2 pointVector = new RawVector2();

        RawVector2 subDevOffset = new RawVector2(0, 0);

        int cenIndexX = 0, cenIndexY = 0;
        double waferRadius;
        float waferAngle = 0f;
        float stepX = 0, stepY = 0;

        float rectWidth;
        float rectHeight;

        float rectHalfWidth;
        float rectHalfHeight;

        bool drawMultDev = false;

        int mapCountX;
        int mapCountY;

        #endregion

        #region DependencyPropertyChangedEvents
        private static void LoaderCommunicationManagerPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                MapViewControl mv = null;

                if (sender is MapViewControl)
                {
                    mv = (MapViewControl)sender;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private static void CurCamPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Task.Run(() =>
                {
                    MapViewControl mv = null;

                    if (sender is MapViewControl)
                    {
                        mv = (MapViewControl)sender;
                    }

                    if (sender != null && e.NewValue != null)
                    {
                        if (mv.Wafer != null)
                        {
                            if (mv.Wafer.MapViewAssignCamType != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    lock (lockCamType)
                                    {
                                        mv.Wafer.MapViewAssignCamType.Value = (e.NewValue as ICamera).GetChannelType();

                                        if (mv.LoaderCommunicationManager != null)
                                        {
                                            mv.LoaderCommunicationManager.SetWaferMapCamera(mv.Wafer.MapViewAssignCamType.Value);
                                        }
                                    }
                                });
                            }
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private static void WaferObjectPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                MapViewControl mv = null;

                if (sender is MapViewControl)
                {
                    mv = (MapViewControl)sender;
                }

                if (sender != null && e.NewValue != null)
                {
                    mv.Wafer = (WaferObject)e.NewValue;
                    mv.ZoomLevel = mv.Wafer.ZoomLevel;
                    mv.WaferObject.ZoomLevel = mv.ZoomLevel;

                    if (mv.ActualWidth < mv.WaferRadiusInPixel * 1.2)
                    {
                        mv.IsMiniMapVisible = true;
                    }
                    else
                    {
                        mv.IsMiniMapVisible = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private static void StepLabelPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                MapViewControl mv = null;

                if (sender is MapViewControl)
                {
                    mv = (MapViewControl)sender;
                }

                if (mv.StepLabel != null)
                {
                    mv.StepLabel = (string)e.NewValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private static void CurrXYIndexPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!(sender is MapViewControl mapViewControl))
                {
                    return;
                }

                System.Windows.Point newValue = (System.Windows.Point)e.NewValue;
                System.Windows.Point oldValue = (System.Windows.Point)e.OldValue;

                if (newValue == null)
                {
                    return;
                }

                bool isUnderDutDiceExist = mapViewControl.UnderDutDices != null &&
                                           mapViewControl.UnderDutDices.Count != 0 &&
                                           mapViewControl.GetUnderDutDices(new MachineIndex((int)newValue.X, (int)newValue.Y));

                mapViewControl.CursorXIndex = isUnderDutDiceExist || mapViewControl.UnderDutDices == null ? (int)newValue.X : (int)oldValue.X;
                mapViewControl.CursorYIndex = isUnderDutDiceExist || mapViewControl.UnderDutDices == null ? (int)newValue.Y : (int)oldValue.Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private static void CursorXIndexPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                MapViewControl mv = null;

                if (sender is MapViewControl)
                {
                    mv = (MapViewControl)sender;
                }

                if (sender != null && e.NewValue != null & mv.WaferObject != null)
                {
                    if (e.NewValue != e.OldValue)
                    {
                        mv._PreXIndex = (int)e.OldValue;
                    }

                    var width = mv.ActualWidth;

                    float rectWidth = mv.RectWidth;
                    float guttWidth = mv.GutterSize;

                    RawVector2 subDevOffset = new RawVector2(0, 0);

                    int xidx = Convert.ToInt32(e.NewValue);

                    double posx = (xidx - mv.CurrXIndex) * (rectWidth + guttWidth) + (float)width / 2f - (rectWidth + guttWidth) / 2f - subDevOffset.X;

                    if (mv.WaferObject != null)
                    {
                        int val;

                        if (xidx < -1 || xidx > (int)mv.WaferObject.GetPhysInfo().MapCountX.Value)
                        {
                            val = (int)e.OldValue;
                        }
                        else
                        {
                            val = (int)e.NewValue;
                        }

                        mv.SetCurrXIndexWrapper(val);
                    }
                }

                if (mv.IsMouseDown)
                {
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private static void CursorYIndexPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                MapViewControl mv = null;

                if (sender is MapViewControl)
                {
                    mv = (MapViewControl)sender;
                }

                if (sender != null && e.NewValue != null & mv.WaferObject != null)
                {
                    if (e.NewValue != e.OldValue)
                    {
                        mv._PreYIndex = (int)e.OldValue;
                    }

                    int topBorder, botBorder;
                    var height = mv.ActualHeight;

                    float rectHeight = mv.RectHeight;
                    float guttHeight = mv.GutterSize;

                    float overlapped = 1.3f;

                    topBorder = mv.CurrYIndex - (int)Math.Ceiling(height / (rectHeight + guttHeight) * overlapped / 2);
                    botBorder = mv.CurrYIndex + (int)Math.Ceiling(height / (rectHeight + guttHeight) * overlapped / 2);

                    RawVector2 subDevOffset = new RawVector2(0, 0);

                    int yidx = Convert.ToInt32(e.NewValue);
                    double posy = (yidx - mv.CurrYIndex) * (rectHeight + guttHeight) + (float)height / 2f - (rectHeight + guttHeight) / 2f - subDevOffset.X;

                    if (mv.WaferObject != null)
                    {
                        int val;

                        if (yidx < -1 || yidx > (int)mv.WaferObject.GetPhysInfo().MapCountY.Value)
                        {
                            val = (int)e.OldValue;
                        }
                        else
                        {
                            val = (int)e.NewValue;
                        }

                        mv.SetCurrYIndexWrapper(val);
                    }
                }

                if (mv.IsMouseDown)
                {
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private static void IsCrossLineVisiblePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                MapViewControl mv = null;

                if (sender is MapViewControl)
                {
                    mv = (MapViewControl)sender;
                }

                if (sender != null && e.NewValue != null)
                {
                    if (!mv.initflag)
                    {
                        mv.InitIsCrossLineVisible = (bool)e.NewValue;
                        mv.initflag = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region Commands
        private RelayCommand _CmdZoomInButton;
        public ICommand CmdZoomInButton
        {
            get
            {
                if (null == _CmdZoomInButton)
                {
                    _CmdZoomInButton = new RelayCommand(CmdZoomIn);
                }

                return _CmdZoomInButton;
            }
        }

        private RelayCommand _CmdZoomOutButton;
        public ICommand CmdZoomOutButton
        {
            get
            {
                if (null == _CmdZoomOutButton)
                {
                    _CmdZoomOutButton = new RelayCommand(CmdZoomOut);
                }

                return _CmdZoomOutButton;
            }
        }

        #endregion

        #region ctor
        public MapViewControl()
        {
            try
            {
                InitEvents();

                MaxZoomLevel = 40;
                MinZoomLevel = 2;

                ZoomLevel = MaxZoomLevel;

                RecSize = 5 * ZoomLevel / 5;

                _rectWidth = RecSize;
                _rectHeight = RecSize;

                GutterSize = 2;

                MaxXCnt = 100;
                MaxYCnt = 100;

                BinColors = new Dictionary<int, System.Windows.Media.Color>();
                System.Windows.Media.Color binColor;

                binColor = new System.Windows.Media.Color();
                binColor = System.Windows.Media.Colors.Red;
                BinColors.Add(0, binColor);

                binColor = System.Windows.Media.Colors.Green;
                BinColors.Add(1, binColor);

                binColor = System.Windows.Media.Colors.Lime;
                BinColors.Add(2, binColor);

                binColor = System.Windows.Media.Colors.Pink;
                BinColors.Add(3, binColor);

                binColor = System.Windows.Media.Colors.Blue;
                BinColors.Add(4, binColor);

                binColor = System.Windows.Media.Colors.Violet;
                BinColors.Add(5, binColor);

                binColor = System.Windows.Media.Colors.WhiteSmoke;
                BinColors.Add(6, binColor);

                binColor = System.Windows.Media.Colors.Yellow;
                BinColors.Add(7, binColor);

                dictBinCodeSolidBrush = new Dictionary<int, SolidColorBrush>();

                MakeBINTextFormat();

                TextFormatsForBin = new TextFormatForBinData();

                MultiDevVisible = true;

                TextBlock = new OutlinedTextBlock();
                TextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                TextBlock.HorizontalAlignment = HorizontalAlignment.Left;

                Binding text = new Binding
                {
                    Path = new PropertyPath("UseUserControl"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                BindingOperations.SetBinding(TextBlock, OutlinedTextBlock.TextProperty, text);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region Functions
        #region EventHandler
        private async void MapViewControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            await MapViewMouseUpAcition();
        }
        private void MapViewControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                float OldZoomLevel = WaferObject.ZoomLevel;

                float zl = 0;

                if (e.Delta > 0)
                {
                    if (ZoomLevel < MaxZoomLevel & ZoomLevel > 0)
                    {
                        zl = (int)Math.Ceiling(ZoomLevel * 1.3f);

                        if (zl > MaxZoomLevel)
                        {
                            ZoomLevel = MaxZoomLevel;

                        }
                        else
                        {
                            ZoomLevel = zl;
                        }
                    }
                    else
                    {
                        ZoomLevel = MaxZoomLevel;
                    }

                    WaferObject.ZoomLevel = ZoomLevel;
                }

                if (e.Delta < 0)
                {
                    if (ZoomLevel > MaxZoomLevel)
                    {
                        ZoomLevel = MaxZoomLevel;
                    }

                    if (ZoomLevel > 2)
                    {
                        zl = (int)Math.Ceiling(ZoomLevel * 0.7f);

                        if (zl < 2)
                        {
                            ZoomLevel = 2;
                        }
                        else
                        {
                            ZoomLevel = zl;
                        }
                    }
                    else
                    {
                        ZoomLevel = 2;
                    }

                    WaferObject.ZoomLevel = ZoomLevel;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void MapViewControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Wafer.StageSupervisor().StageModuleState.GetState() == ProberInterfaces.StageStateEnum.Z_UP)
                {
                    LoggerManager.Debug($"Current ZUP state. Cannot change the position of the die in MapViewControl_MouseDown.");
                    return;
                }

                IsMouseDown = true;
                System.Windows.Point downPos = new System.Windows.Point();
                downPos = e.GetPosition((IInputElement)sender);
                MouseDownPos = downPos;

                double curposX, curposY;

                float centerX = (float)this.ActualWidth / 2f - (RectWidth + GutterSize) / 2f;
                float centerY = (float)this.ActualHeight / 2f + (RectHeight + GutterSize) / 2f;

                //minimap 영역에 들어왔는지 검사
                if (IsMinimapClicked(downPos))
                {
                    int indexX, indexY;
                    CalcMinimapClick(downPos, out indexX, out indexY);

                    SetCurrXIndex(indexX);
                    SetCurrYIndex(indexY);

                    //SetCurrXYIndex(indexX, indexY);

                    isMinimapClicked = true;
                }
                else
                {
                    curposX = centerX - downPos.X;
                    curposY = centerY - downPos.Y;


                    double offsetX = (curposX / (RectWidth + GutterSize));
                    SelectedXIndex = (int)Math.Ceiling(offsetX);
                    SelectedXIndex = CurrXIndex - (SelectedXIndex);

                    double offsetY = (curposY / (RectHeight + GutterSize));
                    SelectedYIndex = (int)Math.Floor(offsetY);
                    SelectedYIndex = CurrYIndex + (SelectedYIndex);
                }

                var bfx = Convert.ToInt32(SelectedXIndex.ToString());
                var bfy = Convert.ToInt32(SelectedYIndex.ToString());

                if (CoordinateManager != null)
                {
                    bool isXReversed = CoordinateManager.GetReverseManualMoveX();
                    bool isYReversed = CoordinateManager.GetReverseManualMoveY();

                    if (isXReversed)
                    {
                        SelectedXIndex = (int)Wafer.GetPhysInfo().MapCountX.Value - 1 - (int)SelectedXIndex;
                    }

                    if (isYReversed)
                    {
                        SelectedYIndex = (int)Wafer.GetPhysInfo().MapCountY.Value - 1 - (int)SelectedYIndex;
                    }

                    if (isXReversed || isYReversed)
                    {
                        LoggerManager.Debug($"Selected Index Reversed [{bfx},{bfy}] => [{SelectedXIndex},{SelectedYIndex}]");
                    }
                }

                DownXIdx = SelectedXIndex;
                DownYIdx = SelectedYIndex;

                DownXSubIdx = CurrXSubIndex;
                DownYSubIdx = CurrYSubIndex;

                LoggerManager.Debug($"Selected die = [{SelectedXIndex}, {SelectedYIndex}]");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void MapViewControl_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                IsMouseDown = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        void MapViewControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (this.ActualWidth != 0)
                {
                    if (Wafer != null)
                    {
                        if (!Wafer.ZoomLevelInit)
                        {
                            float rectsize = ((float)this.ActualWidth / Wafer.GetPhysInfo().MapCountX.Value) - GutterSize * 2;

                            ZoomLevel = (((float)this.ActualWidth - GutterSize * 2) / rectsize) + 3;
                            WaferObject.ZoomLevel = ZoomLevel;
                            Wafer.ZoomLevelInit = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }
        private void MapViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Wafer != null)
                {
                    CurrXIndex = (int)Math.Truncate((double)(Wafer.GetPhysInfo().MapCountX.Value / 2));
                    CurrYIndex = (int)Math.Ceiling((double)(Wafer.GetPhysInfo().MapCountY.Value / 2));
                }
                else
                {
                    LoggerManager.Debug($"MapViewControl_Loaded(): Wafer is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void MapViewControl_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (IsMouseDown == true)
                {
                    double diffX;
                    double diffY;

                    System.Windows.Point currPos = new System.Windows.Point();
                    currPos = e.GetPosition((IInputElement)sender);

                    if (IsMinimapClicked(currPos) | isMinimapClicked)
                    {
                        int indexX, indexY;
                        CalcMinimapClick(currPos, out indexX, out indexY);

                        SetCurrXIndex(indexX);
                        SetCurrYIndex(indexY);

                        //SetCurrXYIndex(indexX, indexY);
                    }
                    else
                    {
                        diffX = MouseDownPos.X - currPos.X;
                        diffY = MouseDownPos.Y - currPos.Y;

                        if (Math.Abs(diffX) > RectWidth + GutterSize | Math.Abs(diffY) > RectHeight + GutterSize)
                        {
                            if (MultiDevVisible == true)
                            {
                                DeviceObject baseDev;
                                baseDev = (DeviceObject)Wafer.Map(CursorXIndex, CursorYIndex);

                                if (Math.Abs(diffX) < (RectWidth + GutterSize) & Math.Abs(diffY) < (RectHeight + GutterSize))
                                {
                                    if (baseDev != null)
                                    {
                                        if (baseDev.SubDevice.Children.Count > 0)
                                        {
                                            SubDieObject subDev;
                                            DeviceObject baseDie;

                                            baseDie = (DeviceObject)(Wafer.Map(CurrXIndex, CurrYIndex));
                                            subDev = (SubDieObject)baseDie.SubDev(CurrXSubIndex, CurrYSubIndex);

                                            var orgSubDev = baseDev.SubDevice.Children.FirstOrDefault(c => c.DieIndexM.XIndex == DownXSubIdx & c.DieIndexM.YIndex == DownYSubIdx);

                                            if (subDev != null)
                                            {
                                                if (orgSubDev == null)
                                                {
                                                    orgSubDev = baseDev.SubDevice.Children[0];
                                                }

                                                double orgX, orgY;
                                                double subDiffX, subDiffY;
                                                double locX, locY;

                                                orgX = orgSubDev.Position.GetX();
                                                orgY = orgSubDev.Position.GetY();

                                                subDiffX = diffX % (RectWidth + GutterSize);
                                                subDiffY = diffY % (RectHeight + GutterSize) * -1;

                                                var childWidth = (float)orgSubDev.Size.Width.Value / umPerPixel - GutterSize;
                                                var childHeight = (float)orgSubDev.Size.Height.Value / umPerPixel - GutterSize;
                                                var childCenX = (float)orgSubDev.Position.GetX() / umPerPixel + childWidth / 2f;
                                                var childCenY = (float)orgSubDev.Position.GetY() / umPerPixel + childHeight / 2f;
                                                var childOffsetX = (float)orgSubDev.Position.GetX() / umPerPixel;
                                                var childOffsetY = (float)orgSubDev.Position.GetY() / umPerPixel;

                                                locX = subDiffX + childCenX;
                                                locY = subDiffY + childCenY;

                                                SubDieObject inRangeSubDev = baseDev.SubDevice.Children.FirstOrDefault(s => FindInRangeSubDev(s, locX, locY) == true);

                                                if (inRangeSubDev != null)
                                                {
                                                    CurrXSubIndex = (int)inRangeSubDev.DieIndexM.XIndex;
                                                    CurrYSubIndex = (int)inRangeSubDev.DieIndexM.YIndex;

                                                    CurrXIndex = DownXIdx + (int)(diffX / (RectWidth + GutterSize));
                                                    CurrYIndex = DownYIdx - (int)(diffY / (RectHeight + GutterSize));
                                                }
                                                else
                                                {
                                                    if (locX > RectWidth + GutterSize + childOffsetX)
                                                    {
                                                        CurrXIndex = DownXIdx + (int)(diffX / (RectWidth + GutterSize)) + 1;
                                                        CurrXSubIndex = 0;
                                                    }

                                                    if (locX < 0)
                                                    {
                                                        CurrXIndex = DownXIdx + (int)(diffX / (RectWidth + GutterSize)) - 1;

                                                        var maxIndexX = baseDev.SubDevice.Children.Where(c => c.DieIndexM.YIndex == CurrYSubIndex).Max(s => s.DieIndexM.XIndex);
                                                        CurrXSubIndex = (int)maxIndexX;
                                                    }

                                                    if (locY > RectHeight + GutterSize + childOffsetY)
                                                    {
                                                        CurrYIndex = DownYIdx + (int)(diffY / (RectHeight + GutterSize)) + 1;
                                                        CurrYSubIndex = 0;
                                                    }

                                                    if (locY < 0)
                                                    {
                                                        CurrYIndex = DownYIdx + (int)(diffY / (RectHeight + GutterSize)) - 1;

                                                        var maxIndexY = baseDev.SubDevice.Children.Where(c => c.DieIndexM.XIndex == CurrXSubIndex).Max(s => s.DieIndexM.YIndex);
                                                        CurrYSubIndex = (int)maxIndexY;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                CurrXIndex = DownXIdx + (int)(diffX / (RectWidth + GutterSize));
                                                CurrYIndex = DownYIdx - (int)(diffY / (RectHeight + GutterSize));

                                                CurrXSubIndex = 0;
                                                CurrYSubIndex = 0;
                                            }
                                        }
                                        else
                                        {
                                            SetCurrXYIndex(DownXIdx + (int)(diffX / (RectWidth + GutterSize)), DownYIdx - (int)(diffY / (RectHeight + GutterSize)));
                                        }
                                    }
                                    else
                                    {
                                        SetCurrXYIndex(DownXIdx + (int)(diffX / (RectWidth + GutterSize)), DownYIdx - (int)(diffY / (RectHeight + GutterSize)));
                                    }
                                }
                                else
                                {
                                    SetCurrXYIndex(DownXIdx + (int)(diffX / (RectWidth + GutterSize)), DownYIdx - (int)(diffY / (RectHeight + GutterSize)));
                                }
                            }
                            else
                            {
                                SetCurrXYIndex(DownXIdx + (int)(diffX / (RectWidth + GutterSize)), DownYIdx - (int)(diffY / (RectHeight + GutterSize)));
                            }

                            IsMouseDownMove = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        private async Task MapViewMouseUpAcition()
        {
            try
            {
                if (Wafer.StageSupervisor().StageModuleState.GetState() == ProberInterfaces.StageStateEnum.Z_UP)
                {
                    LoggerManager.Debug($"Current ZUP state. Cannot change the position of the die in MapViewControl_MouseUp.");
                    return;
                }

                if (!InitIsCrossLineVisible)
                {
                    IsCrossLineVisible = false;
                }

                if (IsMouseDownMove) // Probing 시 라는 상태 추가 되어야함.
                {

                }
                else
                {
                    try
                    {
                        if (EnalbeClickToMove & StageSyncEnalbe & !isMinimapClicked)
                        {
                            if (CurCamera != null)
                            {
                                CurCamera.IsMovingPos = true;

                                await CurCamera.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                                MachineIndex mindex = await CurCamera.SeletedMoveToPos(SelectedXIndex, SelectedYIndex);

                                if (mindex.XIndex != SelectedXIndex && mindex.YIndex != SelectedYIndex)
                                {
                                    SelectedXIndex = CursorXIndex;
                                    SelectedYIndex = CursorYIndex;
                                }

                                await CurCamera.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }

                    CursorXIndex = SelectedXIndex;
                    CursorYIndex = SelectedYIndex;

                    CurrXYIndex = new System.Windows.Point((int)CursorXIndex, (int)CursorYIndex);

                    var mapchangeDelegate = WaferObject?.ChangeMapIndexDelegate;

                    if (mapchangeDelegate != null)
                    {
                        await WaferObject.ChangeMapIndexDelegate(new System.Windows.Point(CursorXIndex, CursorYIndex));
                    }
                }

                IsMouseDown = false;
                IsMouseDownMove = false;
                isMinimapClicked = false;
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
                this.MouseDown += MapViewControl_MouseDown;
                this.MouseLeave += MapViewControl_MouseLeave;
                this.MouseWheel += MapViewControl_MouseWheel;
                this.MouseUp += MapViewControl_MouseUp;
                this.SizeChanged += MapViewControl_SizeChanged;
                this.Loaded += MapViewControl_Loaded;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void InitColorPallette(RenderTarget target)
        {
            try
            {
                resCache.Add("backgroundBrush", t => new SolidColorBrush(t, new RawColor4(105 / 255f, 105 / 255f, 105 / 255f, 255 / 255f)));
                resCache.Add("rectBrush", t => new SolidColorBrush(t, new RawColor4(0 / 255f, 0 / 255f, 255 / 255f, 255 / 255f)));
                resCache.Add("passBrush", t => new SolidColorBrush(t, new RawColor4(50 / 255f, 205 / 255f, 50 / 255f, 255 / 255f)));
                //resCache.Add("testdieBrush", t => new SolidColorBrush(t, new RawColor4(1f, 1f, 0f, 0.9f)));
                resCache.Add("testdieBrush", t => new SolidColorBrush(t, new RawColor4(244 / 255f, 196 / 255f, 48 / 255f, 240 / 255f)));
                resCache.Add("testingBrush", t => new SolidColorBrush(t, new RawColor4(154 / 255f, 205 / 255f, 50 / 255f, 255 / 255f)));
                resCache.Add("failBrush", t => new SolidColorBrush(t, new RawColor4(220 / 255f, 20 / 255f, 60 / 255f, 255 / 255f)));
                resCache.Add("edgeBrush", t => new SolidColorBrush(t, new RawColor4(105 / 255f, 105 / 255f, 105 / 255f, 0.8f)));
                // resCache.Add("markBrush", t => new SolidColorBrush(t, new RawColor4(0 / 255f, 0 / 255f, 139 / 255f, 0.8f)));
                resCache.Add("markBrush", t => new SolidColorBrush(t, new RawColor4(0f, 200 / 255f, 200 / 255f, 0.9f)));
                resCache.Add("DarkRedBrush", t => new SolidColorBrush(t, new RawColor4(139 / 255f, 0 / 255f, 0 / 255f, 1)));
                resCache.Add("changedtestkBrush", t => new SolidColorBrush(t, new RawColor4(100 / 100, 0 / 100, 0 / 255f, 1)));
                resCache.Add("modifymarkBrush", t => new SolidColorBrush(t, new RawColor4(138 / 255f, 43 / 255f, 226 / 255f, 1)));
                resCache.Add("skipBrush", t => new SolidColorBrush(t, new RawColor4(112 / 255f, 128 / 255f, 144 / 255f, 1)));
                resCache.Add("textBrush", t => new SolidColorBrush(t, new RawColor4(0 / 255f, 0 / 255f, 0 / 255f, 1)));
                resCache.Add("miniMapBackGroundBrush", t => new SolidColorBrush(t, new RawColor4(176 / 255f, 196 / 255f, 222 / 255f, 1)));
                resCache.Add("miniMapForeGroundBrush", t => new SolidColorBrush(t, new RawColor4(245 / 255f, 245 / 255f, 245 / 255f, 1)));
                resCache.Add("selectedBrush", t => new SolidColorBrush(t, new RawColor4(245 / 255f, 245 / 255f, 245 / 255f, 0.5f)));
                resCache.Add("infoBackgroundBrush", t => new SolidColorBrush(t, new RawColor4(245 / 255f, 245 / 255f, 245 / 255f, 0.9f)));
                resCache.Add("CurBrush", t => new SolidColorBrush(t, new RawColor4(0f, 0f, 0f, 0.9f)));
                resCache.Add("FirstDutBrush", t => new SolidColorBrush(t, new RawColor4(60 / 255f, 60 / 255f, 60 / 255f, 0.8f)));
                resCache.Add("lockBrush", t => new SolidColorBrush(t, new RawColor4(254, 190, 56, 1)));
                resCache.Add("brownBrush", t => new SolidColorBrush(t, new RawColor4(100 / 255f, 0f, 0f, 0.8f)));
                resCache.Add("DarkGrayBrush", t => new SolidColorBrush(t, new RawColor4(40 / 255f, 40 / 255f, 40 / 255f, 0.8f)));
                resCache.Add("OrangeBrush", t => new SolidColorBrush(t, new RawColor4(255 / 255f, 165 / 255f, 0 / 255f, 0.8f)));
                resCache.Add("DarkGreenBrush", t => new SolidColorBrush(t, new RawColor4(20 / 255f, 132 / 255f, 58 / 255f, 1f)));
                resCache.Add("VioletBrush", t => new SolidColorBrush(t, new RawColor4(179 / 255f, 35 / 255f, 167 / 255f, 1f)));
                resCache.Add("LightGreenBrush", t => new SolidColorBrush(t, new RawColor4(183 / 255f, 234 / 255f, 25 / 255f, 1f)));
                resCache.Add("GreenBrush", t => new SolidColorBrush(t, new RawColor4(0f, 255f, 0f, 1f)));
                resCache.Add("RedBrush", t => new SolidColorBrush(t, new RawColor4(255f, 0f, 0f, 1f)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetCurrXIndexWrapper(int x)
        {
            try
            {
                int cx = x;

                if (Wafer.DispHorFlip == DispFlipEnum.FLIP)
                {
                    cx = (int)Wafer.GetPhysInfo().MapCountX.Value - 1 - x;
                }

                SetCurrXIndex(cx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetCurrYIndexWrapper(int y)
        {
            try
            {
                int cy = y;

                if (Wafer.DispVerFlip == DispFlipEnum.FLIP)
                {
                    cy = (int)Wafer.GetPhysInfo().MapCountY.Value - 1 - y;
                }

                SetCurrYIndex(cy);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetCurrXYIndex(int x, int y)
        {
            try
            {
                SetCurrXIndexWrapper(x);
                SetCurrYIndexWrapper(y);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int leftcount;
        public int rightcount;
        public int lefthidecount;
        public int righthidecount;

        public int uppercount;
        public int bottomcount;
        public int upperhidecount;
        public int bottomhidecount;

        private void SetCurrXIndex(int newValue)
        {
            try
            {
                var mapCountX = Wafer.GetPhysInfo().MapCountX.Value;

                var width = this.ActualWidth;

                float rectWidth = RectWidth;
                float guttWidth = GutterSize;

                RawVector2 subDevOffset = new RawVector2(0, 0);

                double posx = (rectWidth + guttWidth) + (float)width / 2f - (rectWidth + guttWidth) / 2f - subDevOffset.X;
                double leftoffset = posx;
                double rightoffset = ActualWidth - posx;

                leftcount = (int)(leftoffset / (rectWidth + guttWidth));
                rightcount = (int)(rightoffset / (rectWidth + guttWidth));

                lefthidecount = -(newValue - leftcount) - 1;
                righthidecount = mapCountX - newValue - rightcount - 1;

                if (leftcount + rightcount < mapCountX)
                {
                    if (lefthidecount >= 0 || righthidecount <= 0)
                    {
                        if (newValue < (int)CursorXIndex)
                        {
                            //Move Left
                            //왼쪽으로 숨겨진 Map 이있다면
                            if (lefthidecount < 0)
                            {
                                if (lefthidecount > ((int)CursorXIndex - newValue))
                                {
                                    if (righthidecount < 0)
                                    {
                                        CurrXIndex = (int)newValue - Math.Abs(righthidecount);
                                    }
                                    else
                                    {
                                        CurrXIndex = (int)newValue;
                                    }
                                }
                            }
                            else
                            {
                                CurrXIndex = newValue + Math.Abs(lefthidecount);
                            }
                        }
                        else
                        {
                            //Move Right
                            if (righthidecount > 0)
                            {
                                if (righthidecount > (newValue - (int)CursorXIndex))
                                {
                                    if (lefthidecount > 0)
                                    {
                                        CurrXIndex = (int)newValue + Math.Abs(lefthidecount);
                                    }
                                    else
                                    {
                                        CurrXIndex = (int)newValue;
                                    }
                                }
                            }
                            else
                            {
                                CurrXIndex = (int)newValue - Math.Abs(righthidecount);
                            }
                        }
                    }
                    else
                    {
                        CurrXIndex = newValue;
                    }
                }
                else
                {
                    if (CurrXIndex != (mapCountX / 2))
                    {
                        CurrXIndex = mapCountX / 2;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SetCurrYIndex(int newValue)
        {
            try
            {
                var mapCountY = Wafer.GetPhysInfo().MapCountY.Value;

                var height = ActualHeight;
                float rectHeight = RectHeight;
                float guttHeight = GutterSize;

                RawVector2 subDevOffset = new RawVector2(0, 0);

                double posy = rectHeight + guttHeight + (float)height / 2f - (rectHeight + guttHeight) / 2f - subDevOffset.X;

                double upperoffset = posy;
                double bottomoffset = ActualHeight - posy;

                uppercount = (int)(upperoffset / (rectHeight + guttHeight));
                bottomcount = (int)(bottomoffset / (rectHeight + guttHeight));

                upperhidecount = mapCountY - newValue - uppercount;
                bottomhidecount = (newValue - bottomcount);

                if (uppercount + bottomcount < mapCountY)
                {
                    if (upperhidecount <= 0 || bottomhidecount <= 0)
                    {
                        if (newValue < CursorYIndex)
                        {
                            //Move bottom
                            //bottom으로 숨겨진 Map 이있다면
                            if (bottomhidecount > 0)
                            {
                                if (bottomhidecount > (CursorYIndex - newValue))
                                {
                                    if (upperhidecount < 0)
                                    {
                                        CurrYIndex = newValue - Math.Abs(upperhidecount);
                                    }
                                    else
                                    {
                                        CurrYIndex = newValue;
                                    }
                                }
                            }
                            else
                            {
                                CurrYIndex = newValue + Math.Abs(bottomhidecount);
                            }
                        }
                        else
                        {
                            //Move Upper
                            if (upperhidecount > 0)
                            {
                                if (upperhidecount > (newValue - CursorYIndex))
                                {
                                    if (bottomhidecount < 0)
                                    {
                                        CurrYIndex = newValue + Math.Abs(bottomhidecount);
                                    }
                                    else
                                    {
                                        CurrYIndex = newValue;
                                    }
                                }
                            }
                            else
                            {
                                CurrYIndex = newValue - Math.Abs(upperhidecount);
                            }
                        }
                    }
                    else
                    {
                        CurrYIndex = newValue;
                    }
                }
                else
                {
                    if (CurrYIndex != (mapCountY / 2))
                    {
                        CurrYIndex = mapCountY / 2;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CmdZoomIn()
        {
            try
            {
                WaferObject.ZoomLevel--;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CmdZoomOut()
        {
            try
            {
                WaferObject.ZoomLevel++;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void MakeBINTextFormat()
        {
            try
            {
                int fontNum = 255;

                if (BINTextFormats == null)
                {
                    BINTextFormats = new List<TextFormat>();
                }
                else
                {
                    BINTextFormats.Clear();
                }

                for (float i = 1f; i <= fontNum; i++)
                {
                    TextFormat tmp = new TextFormat(fontFactory, "Segoe UI", i);
                    BINTextFormats.Add(tmp);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SetTextFormatWidthMetrics(float width, float height, BinType binType)
        {
            try
            {
                float MetWidth = 0;
                float MetHeight = 0;

                TextFormat tf = null;

                List<string> tmpdata = new List<string>();

                // TODO : 문자마다 크기가 다른 부분으로 인해, 오차 발생 됨.
                // 한 맵에서 여러 폰트를 사용할 것인가?
                if (binType == BinType.BIN_PASSFAIL)
                {
                    tmpdata.Add("0");
                }
                else if (binType == BinType.BIN_6BIT)
                {
                    tmpdata.Add("0");
                    tmpdata.Add("00");
                }
                else if (binType == BinType.BIN_8BIT)
                {
                    tmpdata.Add("0");
                    tmpdata.Add("00");
                    tmpdata.Add("000");
                }

                TextFormatsForBin.Formats.Clear();

                foreach (var item in tmpdata)
                {
                    foreach (var format in BINTextFormats)
                    {
                        using (var textLayout = new TextLayout(fontFactory, item, format, float.PositiveInfinity, float.PositiveInfinity))
                        {
                            if (textLayout.Metrics.Width < width && textLayout.Metrics.Height < height)
                            {
                                tf = format;

                                MetWidth = textLayout.Metrics.Width;
                                MetHeight = textLayout.Metrics.Height;
                            }
                            else
                            {
                                break;
                            }

                            textLayout.Dispose();
                        }
                    }

                    if (tf != null)
                    {
                        TextFormatWidthMetrics tmp = new TextFormatWidthMetrics();
                        tmp.Foramt = tf;
                        tmp.Width = MetWidth;
                        tmp.Height = MetHeight;
                        tmp.Length = item.Length;

                        TextFormatsForBin.Formats.Add(tmp);
                    }
                }

                TextFormatsForBin.LastBinType = binType;
                TextFormatsForBin.LastZoomLevel = WaferObject.ZoomLevel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GetCenterIndex(IWaferObject map, out int xidx, out int yidx)
        {
            xidx = 0;
            yidx = 0;

            try
            {
                if (WaferObject != null)
                {
                    xidx = (int)Wafer.GetPhysInfo().CenM.XIndex.Value;
                    yidx = (int)Wafer.GetPhysInfo().CenM.YIndex.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SetFOV()
        {
            try
            {
                if (WaferObject != null)
                {
                    if (WaferObject.ZoomLevel > MaxZoomLevel)
                    {
                        WaferObject.ZoomLevel = MaxZoomLevel;
                    }

                    if (WaferObject.ZoomLevel < MinZoomLevel)
                    {
                        WaferObject.ZoomLevel = MinZoomLevel;
                    }

                    RecSize = ((float)this.ActualWidth / (float)WaferObject.ZoomLevel) - GutterSize * 2;

                    if (WaferObject.ZoomLevel != _PreZoomLevel)
                    {
                        SetCurrXYIndex(CursorXIndex, CursorYIndex);
                    }

                    _PreZoomLevel = WaferObject.ZoomLevel;
                }
                else
                {
                    ZoomLevel = 25;
                    RecSize = 5 * ZoomLevel / 5;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CalcMinimapClick(System.Windows.Point pos, out int indexX, out int indexY)
        {
            try
            {
                float mapcenterX = (float)this.ActualWidth / 2f;
                float mapcenterY = (float)this.ActualHeight / 2f;

                float minimapRadius = _MinimapWidth * 0.5f;
                float minimapcenterX = (float)miniMapStartPosition.X + minimapRadius;
                float minimapcenterY = (float)miniMapStartPosition.Y + minimapRadius;

                float scalex = (float)(mapcenterX / minimapRadius);
                float scaley = (float)(mapcenterY / minimapRadius);

                float transX = (float)(pos.X - minimapcenterX) * scalex;
                float transY = (float)(pos.Y - minimapcenterY) * scaley * -1;//부호 반대

                float currX = mapcenterX + transX;
                float currY = mapcenterY + transY;

                float dieW = (float)this.ActualWidth / this.Wafer.GetPhysInfo().MapCountX.Value;
                float dieH = (float)this.ActualHeight / this.Wafer.GetPhysInfo().MapCountY.Value;

                indexX = (int)Math.Round(currX / dieW, 0);
                indexY = (int)Math.Round(currY / dieH, 0);
            }
            catch (Exception err)
            {
                indexX = 0;
                indexY = 0;
                LoggerManager.Exception(err);
            }
        }
        public bool GetUnderDutDices(MachineIndex mCoord)
        {
            bool retVal = false;

            if (WaferObject == null)
            {
                return retVal;
            }

            var cardinfo = WaferObject.GetParam_ProbeCard();
            List<IDeviceObject> dev = new List<IDeviceObject>();

            try
            {
                if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                {
                    for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                    {
                        IndexCoord retindex = mCoord.Add(cardinfo.GetRefOffset(dutIndex));
                        IDeviceObject devobj = WaferObject.GetDevices().Find(x => x.DieIndexM.Equals(retindex));

                        if (devobj != null)
                        {
                            dev.Add(devobj);
                            dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                        }
                        else
                        {
                            devobj = new DeviceObject();
                            devobj.DieIndexM.XIndex = retindex.XIndex;
                            devobj.DieIndexM.YIndex = retindex.YIndex;
                            dev.Add(devobj);
                            dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                        }
                    }

                    _UnderDutDices = dev;
                }

                if (dev.Count != 0)
                {
                    foreach (var ddie in dev)
                    {
                        if (WaferObject.GetDevices().Find(devobj => devobj.DieIndexM.Equals(ddie.DieIndexM)) != null)
                        {
                            if (WaferObject.GetDevices().Find(devobj => devobj.DieIndexM.Equals(ddie.DieIndexM)).State.Value != DieStateEnum.NOT_EXIST)
                            {
                                retVal = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private bool IsMinimapClicked(System.Windows.Point pos)
        {
            bool retVal = false;

            try
            {
                retVal = IsMiniMapVisible && this.ActualWidth > 256 && this.ActualHeight > 256 &&
                         pos.X >= miniMapStartPosition.X && miniMapStartPosition.X + _MinimapWidth >= pos.X &&
                         pos.Y >= miniMapStartPosition.Y && miniMapStartPosition.Y + _MinimapHeight >= pos.Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private bool FindInRangeSubDev(SubDieObject subDev, double subDiffX, double subDiffY)
        {
            bool inRange = false;

            try
            {
                var childWidth = (float)subDev.Size.Width.Value / umPerPixel - GutterSize;
                var childHeight = (float)subDev.Size.Height.Value / umPerPixel - GutterSize;
                var childOffsetX = (float)subDev.Position.GetX() / umPerPixel;
                var childOffsetY = ((float)subDev.Position.GetY() / umPerPixel);
                double baseDevHeight = (RectHeight + GutterSize);

                if ((subDiffX > childOffsetX & subDiffX < childOffsetX + childWidth) &&
                    (subDiffY > childOffsetY & subDiffY < childOffsetY + childHeight))
                {
                    inRange = true;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return inRange;
        }
        private void DrawPMIObject(RenderTarget target, uint Enable, float rectHalfWidth, float rectHalfHeight, float posx, float posy)
        {
            try
            {
                if (Enable == 0x01)
                {
                    geometryStartPosition.X = posx;
                    geometryStartPosition.Y = posy;

                    using (var geometry = new PathGeometry(target.Factory))
                    {
                        using (var geoSink = geometry.Open())
                        {
                            geoSink.BeginFigure(geometryStartPosition, FigureBegin.Filled);

                            TrianglePos1.X = geometryStartPosition.X + rectHalfWidth;
                            TrianglePos1.Y = geometryStartPosition.Y;

                            TrianglePos2.X = geometryStartPosition.X;
                            TrianglePos2.Y = geometryStartPosition.Y + rectHalfHeight;

                            geoSink.AddLine(TrianglePos1);
                            geoSink.AddLine(TrianglePos2);

                            geoSink.EndFigure(new FigureEnd());
                            geoSink.Close();
                            target.FillGeometry(geometry, resCache["edgeBrush"] as Brush);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private long DrawDevice(RenderTarget target, IDeviceObject dev, float rectWidth, float rectHeight, float posx, float posy, long validDieCount)
        {
            try
            {
                if (RenderMode == MapViewMode.MapMode)
                {
                    switch (dev.DieType.Value)
                    {
                        case DieTypeEnum.TEST_DIE:

                            if (dev is SubDieObject)
                            {
                                target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["testdieBrush"] as Brush);
                                validDieCount++;
                            }
                            else
                            {
                                List<IDeviceObject> children = dev.GetChildren();

                                if (children != null)
                                {
                                    if (dev.GetChildren().Count > 0)
                                    {
                                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["skipBrush"] as Brush);
                                        validDieCount++;
                                    }
                                    else
                                    {
                                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["testdieBrush"] as Brush);
                                        validDieCount++;
                                    }
                                }
                            }

                            break;

                        case DieTypeEnum.MARK_DIE:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["markBrush"] as Brush);
                            break;
                        case DieTypeEnum.NOT_EXIST:
                            break;
                        case DieTypeEnum.UNKNOWN:
                            break;
                        case DieTypeEnum.SKIP_DIE:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["skipBrush"] as Brush);
                            validDieCount++;
                            break;
                        case DieTypeEnum.CHANGEMARK_DIE:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["DarkRedBrush"] as Brush);
                            break;
                        case DieTypeEnum.CHANGETEST_DIE:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["changedtestkBrush"] as Brush);
                            break;
                        case DieTypeEnum.MODIFY_DIE:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["modifymarkBrush"] as Brush);
                            break;
                        default:
                            break;
                    }
                }
                else if (RenderMode == MapViewMode.BinMode) // BinMode
                {
                    switch (dev.State.Value)
                    {
                        case DieStateEnum.UNKNOWN:
                            break;
                        case DieStateEnum.NOT_EXIST:
                            break;
                        case DieStateEnum.NORMAL:

                            if (dev is SubDieObject)
                            {
                                target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["testdieBrush"] as Brush);
                                validDieCount++;
                            }
                            else
                            {
                                List<IDeviceObject> children = dev.GetChildren();
                                if (children != null)
                                {
                                    if (dev.GetChildren().Count > 0)
                                    {
                                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["skipBrush"] as Brush);
                                        validDieCount++;
                                    }
                                    else
                                    {
                                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["testdieBrush"] as Brush);
                                        validDieCount++;
                                    }
                                }
                            }
                            break;
                        case DieStateEnum.TESTING:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["testingBrush"] as Brush);
                            validDieCount++;
                            break;
                        case DieStateEnum.TESTED:

                            // TODO : BIN별 색상 도입 필요.

                            if (dev.CurTestHistory != null)
                            {
                                if (dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_PASS)
                                {
                                    target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["passBrush"] as Brush);
                                }
                                else
                                {
                                    target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["failBrush"] as Brush);
                                }
                            }

                            string bincode = string.Empty;

                            if (BINType == BinType.BIN_PASSFAIL)
                            {
                                if (dev.CurTestHistory != null)
                                {
                                    if (dev.CurTestHistory.BinCode.Value == 2)
                                    {
                                        bincode = "P";
                                    }
                                    else
                                    {
                                        bincode = "F";
                                    }
                                }
                            }
                            else
                            {
                                if (dev.CurTestHistory != null)
                                {
                                    bincode = dev.CurTestHistory.BinCode.Value.ToString();
                                }
                            }

                            // 데이터 미생성 또는 데이터 변경 시
                            if (TextFormatsForBin.Formats == null || TextFormatsForBin.Formats.Count == 0 ||
                                TextFormatsForBin.LastZoomLevel != WaferObject.ZoomLevel ||
                                TextFormatsForBin.LastBinType != BINType)
                            {
                                SetTextFormatWidthMetrics(rectWidth, rectHeight, BINType);
                            }

                            TextFormatWidthMetrics tf = TextFormatsForBin.GetMatchedFormat(bincode.Length);

                            float l, r, t, b;

                            float met_hw, met_hh, rec_hw, rec_hh;

                            if (tf != null)
                            {
                                met_hw = (tf.Width / 2);
                                met_hh = (tf.Height / 2);

                                rec_hw = (rectWidth / 2.0f);
                                rec_hh = (rectHeight / 2.0f);

                                l = posx - met_hw + rec_hw;
                                t = posy - met_hh + rec_hh;
                                r = posx + met_hw + rec_hw;
                                b = posy + met_hh + rec_hh;

                                target.DrawText(bincode, tf.Foramt, new RawRectangleF(l, t, r, b), resCache["textBrush"] as Brush);
                            }

                            validDieCount++;

                            break;
                        case DieStateEnum.MARK:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["markBrush"] as Brush);
                            break;
                        case DieStateEnum.SKIPPED:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["skipBrush"] as Brush);
                            validDieCount++;
                            break;
                        default:
                            break;
                    }
                }
                else if (RenderMode == MapViewMode.SeqenceMode)
                {
                    switch (dev.DieType.Value)
                    {
                        case DieTypeEnum.TEST_DIE:

                            if ((dev.ExistSeq != null) && (dev.ExistSeq.Count > 0))

                            {
                                target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["brownBrush"] as Brush);
                                validDieCount++;
                            }
                            else
                            {
                                target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["testdieBrush"] as Brush);
                                validDieCount++;
                            }

                            break;
                        case DieTypeEnum.MARK_DIE:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["markBrush"] as Brush);
                            break;
                        case DieTypeEnum.NOT_EXIST:
                            break;
                        case DieTypeEnum.UNKNOWN:
                            break;
                        case DieTypeEnum.SKIP_DIE:
                            target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["skipBrush"] as Brush);
                            validDieCount++;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return validDieCount;
        }
        private void FindFirstDut()
        {
            try
            {
                List<IDeviceObject> underdutdices = UnderDutDices.ToList();

                if (underdutdices.Count > 0)
                {
                    for (int dindex = 0; dindex < underdutdices.Count; dindex++)
                    {
                        if ((underdutdices[dindex] != null) && (underdutdices[dindex].DieIndexM != null))
                        {
                            if (underdutdices[dindex].DieIndexM.XIndex == CursorXIndex & underdutdices[dindex].DieIndexM.YIndex == CursorYIndex)
                            {
                                underdutdices[dindex].DutNumber = 1;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CalcWaferAngle()
        {
            try
            {
                waferAngle = (float)Wafer.GetPhysInfo().NotchAngle.Value;

                if (Wafer.DispHorFlip == DispFlipEnum.FLIP && Wafer.DispVerFlip == DispFlipEnum.FLIP)
                {
                    waferAngle += 180;
                }

                if (waferAngle > 360)
                {
                    waferAngle = waferAngle - 360;
                }

                var rotateangle = 360 - waferAngle - 90;

                waferAngle = rotateangle;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DrawWaferOutline(RenderTarget target)
        {
            try
            {
                int indexOffsetX = 0, indexOffsetY = 0;
                float waferRadiusInPixel = 0f;
                float refDieOffsetXInPixel = 0f;
                float refDieOffsetYInPixel = 0f;

                #region // Wafer outline drawing

                indexOffsetX = cenIndexX - CurrXIndex;
                indexOffsetY = CurrYIndex - cenIndexY;

                waferRadiusInPixel = (float)(waferRadius / ((this.ActualHeight / (RectHeight + GutterSize) * stepY)) * this.ActualHeight);
                WaferRadiusInPixel = waferRadiusInPixel;

                umPerPixel = (float)Wafer.GetPhysInfo().DieSizeY.Value / RectHeight;

                refDieOffsetXInPixel = (float)(Wafer.GetPhysInfo().LowLeftCorner.GetX() / umPerPixel);
                refDieOffsetYInPixel = (float)(Wafer.GetPhysInfo().LowLeftCorner.GetY() / umPerPixel);

                double dieWidthPixel = (float)(Wafer.GetSubsInfo().ActualDieSize.Width.Value / umPerPixel);
                double dieHeightPixel = (float)(Wafer.GetSubsInfo().ActualDieSize.Height.Value / umPerPixel);

                double cenOffsetXPixel = (-dieWidthPixel / 2) - refDieOffsetXInPixel;
                double cenOffsetYPixel = (-dieHeightPixel / 2) - refDieOffsetYInPixel;

                if (drawMultDev == true)
                {
                    DeviceObject baseDev = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>().Find(d => d.DieIndexM.XIndex == CurrXIndex & d.DieIndexM.YIndex == CurrYIndex);

                    if (baseDev != null)
                    {
                        var subDev = baseDev.SubDevice.Children.FirstOrDefault(c => c.DieIndexM.XIndex == CurrXSubIndex & c.DieIndexM.YIndex == CurrYSubIndex);

                        if (subDev != null)
                        {
                            subDevOffset.X = +(float)subDev.Position.GetX() / umPerPixel + (float)subDev.Size.Width.Value / 2f / umPerPixel - (float)(RectWidth) / 2.0f;
                            subDevOffset.Y = -(float)subDev.Position.GetY() / umPerPixel - (float)subDev.Size.Height.Value / 2f / umPerPixel + (float)(RectHeight) / 2.0f;

                            waferCenPos.X = (float)(indexOffsetX * (RectWidth + GutterSize) + this.ActualWidth / 2) + (float)(refDieOffsetXInPixel) - subDevOffset.X;
                            waferCenPos.Y = (float)(indexOffsetY * (RectHeight + GutterSize) + this.ActualHeight / 2) - (float)(refDieOffsetYInPixel) - subDevOffset.Y;
                        }
                        else
                        {
                            waferCenPos.X = (float)(indexOffsetX * (RectWidth + GutterSize) - (RectWidth) / 2.0f + this.ActualWidth / 2) + (float)(refDieOffsetXInPixel);
                            waferCenPos.Y = (float)(indexOffsetY * (RectHeight + GutterSize) - (RectHeight) / 2.0f + this.ActualHeight / 2) - (float)(refDieOffsetYInPixel);
                        }
                    }
                    else
                    {
                        waferCenPos.X = (float)(indexOffsetX * (RectWidth + GutterSize) - (RectWidth) / 2.0f + this.ActualWidth / 2) + (float)(refDieOffsetXInPixel);
                        waferCenPos.Y = (float)(indexOffsetY * (RectHeight + GutterSize) - (RectHeight) / 2.0f + this.ActualHeight / 2) - (float)(refDieOffsetYInPixel);
                    }
                }
                else
                {
                    waferCenPos.X = (float)(indexOffsetX * (RectWidth + GutterSize) + this.ActualWidth / 2.0) + (float)(cenOffsetXPixel);
                    waferCenPos.Y = (float)(indexOffsetY * (RectHeight + GutterSize) + this.ActualHeight / 2.0) - (float)(cenOffsetYPixel);
                }

                geometryStartPosition.X = waferCenPos.X;
                geometryStartPosition.Y = waferCenPos.Y - waferRadiusInPixel / 2;

                float size = waferRadiusInPixel / 2;

                float notchLength = 5000;
                float notchRadiusInPixel = (float)(notchLength / ((this.ActualHeight / (RectHeight + GutterSize)) * stepY) * this.ActualHeight);
                float flatZoneAngle;

                if (Wafer.GetPhysInfo().NotchType.Value == WaferNotchTypeEnum.FLAT.ToString())
                {
                    notchLength = 57500;
                    flatZoneAngle = (float)Math.Acos(notchLength / waferRadius) + (float)Math.PI;

                    arc1.Size.Width = size;
                    arc1.Size.Height = size;
                    arc1.Point.X = (float)waferCenPos.X + waferRadiusInPixel / 2 * (float)Math.Cos(flatZoneAngle);
                    arc1.Point.Y = (float)waferCenPos.Y + (waferRadiusInPixel / 2 * (float)Math.Sin(flatZoneAngle) * -1);
                    arc1.SweepDirection = SweepDirection.CounterClockwise;

                    arc3.Point.X = geometryStartPosition.X;
                    arc3.Point.Y = geometryStartPosition.Y;
                    arc3.Size.Width = size;
                    arc3.Size.Height = size;
                    arc3.SweepDirection = SweepDirection.CounterClockwise;

                    notchrectangle.Width = (int)(notchRadiusInPixel / 3) * 2;
                    notchrectangle.Height = notchrectangle.Width;
                    notchrectangle.X = (int)geometryStartPosition.X - (notchrectangle.Width / 2);
                    notchrectangle.Y = (int)(arc1.Point.Y + (notchRadiusInPixel * 2)) - (notchrectangle.Height / 2);

                    using (var geometry = new PathGeometry(target.Factory))
                    {
                        using (var geoSink = geometry.Open())
                        {
                            geoSink.SetFillMode(FillMode.Winding);
                            geoSink.BeginFigure(geometryStartPosition, new FigureBegin());
                            geoSink.AddArc(arc1);
                            pointVector.X = (float)waferCenPos.X - waferRadiusInPixel / 2 * (float)Math.Cos(flatZoneAngle);
                            pointVector.Y = arc1.Point.Y;
                            geoSink.AddLine(pointVector);
                            geoSink.AddArc(arc3);
                            geoSink.EndFigure(new FigureEnd());
                            geoSink.Close();

                            pointVector.X = waferCenPos.X;
                            pointVector.Y = waferCenPos.Y;

                            var matrix = Transformation.Rotation(waferAngle, pointVector);
                            tfGeometry = new TransformedGeometry(target.Factory, geometry, matrix);

                            RawVector2 notpoint = new RawVector2(notchrectangle.X, notchrectangle.Y);
                            var notchpoint = Transformation.Rotation(notpoint, waferCenPos, waferAngle);

                            notchrectangle.X = (int)notchpoint.X;
                            notchrectangle.Y = (int)notchpoint.Y;

                            target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                            target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);
                            target.FillRectangle(new RawRectangleF(notchrectangle.X, notchrectangle.Y, notchrectangle.X + notchrectangle.Width, notchrectangle.Y + notchrectangle.Height), resCache["DarkRedBrush"] as Brush);

                            tfGeometry.Dispose();
                        }
                    }
                }
                else
                {
                    notchLength = 5000;
                    notchRadiusInPixel = (float)(notchLength / ((this.ActualHeight / (RectHeight + GutterSize)) * stepY) * this.ActualHeight);
                    flatZoneAngle = (float)Math.Acos(notchLength / waferRadius) + (float)Math.PI;

                    arc1.Size.Width = size;
                    arc1.Size.Height = size;
                    arc1.Point.X = (float)waferCenPos.X + waferRadiusInPixel / 2 * (float)Math.Cos(flatZoneAngle);
                    arc1.Point.Y = (float)waferCenPos.Y + (waferRadiusInPixel / 2 * (float)Math.Sin(flatZoneAngle) * -1);
                    arc1.SweepDirection = SweepDirection.CounterClockwise;

                    arc2.Size.Width = notchRadiusInPixel / 2;
                    arc2.Size.Height = notchRadiusInPixel / 2;
                    arc2.Point.X = (float)waferCenPos.X - waferRadiusInPixel / 2 * (float)Math.Cos(flatZoneAngle);
                    arc2.Point.Y = arc1.Point.Y;
                    arc2.SweepDirection = SweepDirection.Clockwise;

                    arc3.Point.X = geometryStartPosition.X;
                    arc3.Point.Y = geometryStartPosition.Y;
                    arc3.Size.Width = size;
                    arc3.Size.Height = size;
                    arc3.SweepDirection = SweepDirection.CounterClockwise;

                    pointVector.X = waferCenPos.X;
                    pointVector.Y = waferCenPos.Y;

                    notchellipse.Point.X = arc3.Point.X;
                    notchellipse.Point.Y = arc2.Point.Y + (notchRadiusInPixel * 2);
                    notchellipse.RadiusX = notchRadiusInPixel / (float)3;
                    notchellipse.RadiusY = notchRadiusInPixel / (float)3;

                    using (var geometry = new PathGeometry(target.Factory))
                    {
                        using (var geoSink = geometry.Open())
                        {
                            geoSink.SetFillMode(FillMode.Winding);
                            geoSink.BeginFigure(geometryStartPosition, new FigureBegin());
                            geoSink.AddArc(arc1);
                            geoSink.AddArc(arc2);
                            geoSink.AddArc(arc3);
                            geoSink.EndFigure(new FigureEnd());
                            geoSink.Close();

                            var matrix = Transformation.Rotation(waferAngle, pointVector);
                            tfGeometry = new TransformedGeometry(target.Factory, geometry, matrix);
                            var notchpoint = Transformation.Rotation(notchellipse.Point, waferCenPos, waferAngle);

                            target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                            target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);

                            tfGeometry.Dispose();
                        }
                    }

                    ///Draw Notch
                    using (var geometry = new PathGeometry(target.Factory))
                    {
                        using (var geoSink = geometry.Open())
                        {
                            geoSink.SetFillMode(FillMode.Winding);

                            notchellipse.Point.X = arc3.Point.X;
                            notchellipse.Point.Y = arc2.Point.Y + (notchRadiusInPixel);

                            var notchstartpoint = Transformation.Rotation(notchellipse.Point, waferCenPos, waferAngle);
                            geoSink.BeginFigure(notchstartpoint, new FigureBegin());

                            if (waferAngle == 0)
                            {
                                geoSink.AddLine(new RawVector2(notchstartpoint.X - 14, notchstartpoint.Y + 14));
                                geoSink.AddLine(new RawVector2(notchstartpoint.X + 14, notchstartpoint.Y + 14));
                            }
                            else if (waferAngle == 90)
                            {
                                geoSink.AddLine(new RawVector2(notchstartpoint.X - 14, notchstartpoint.Y - 14));
                                geoSink.AddLine(new RawVector2(notchstartpoint.X - 14, notchstartpoint.Y + 14));
                            }
                            else if (waferAngle == 180)
                            {
                                geoSink.AddLine(new RawVector2(notchstartpoint.X - 14, notchstartpoint.Y - 14));
                                geoSink.AddLine(new RawVector2(notchstartpoint.X + 14, notchstartpoint.Y - 14));
                            }
                            else if (waferAngle == 270)
                            {
                                geoSink.AddLine(new RawVector2(notchstartpoint.X + 14, notchstartpoint.Y - 14));
                                geoSink.AddLine(new RawVector2(notchstartpoint.X + 14, notchstartpoint.Y + 14));
                            }

                            geoSink.EndFigure(new FigureEnd());
                            geoSink.Close();

                            target.DrawGeometry(geometry, resCache["LightGreenBrush"] as Brush);
                            target.FillGeometry(geometry, resCache["LightGreenBrush"] as Brush);
                            tfGeometry.Dispose();
                        }
                    }
                }

                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DrawMiniMap(RenderTarget target)
        {
            try
            {
                Size2 miniMapSize = new Size2(120, 120);

                miniMapStartPosition = new RawVector2();
                miniMapStartPosition.X = (float)this.ActualWidth - 135;
                miniMapStartPosition.Y = 0 + 15;

                double maxIndexXInView, maxIndexYInView;

                if (IsMiniMapVisible && this.ActualWidth > 256 && this.ActualHeight > 256)
                {
                    target.FillRectangle(new RawRectangleF(miniMapStartPosition.X, miniMapStartPosition.Y, miniMapStartPosition.X + miniMapSize.Width, miniMapStartPosition.Y + miniMapSize.Height), resCache["miniMapBackGroundBrush"] as Brush);

                    geometryStartPosition.X = miniMapStartPosition.X + miniMapSize.Width / 2f;
                    geometryStartPosition.Y = miniMapStartPosition.Y + 10f;

                    float size = 48.5f;
                    float notchLength = (float)waferRadius / 10.0f;
                    float notchRadiusInPixel = 6;

                    if (Wafer.GetPhysInfo().NotchType.Value == WaferNotchTypeEnum.FLAT.ToString())
                    {
                        notchLength = 20;
                        size = 50;
                        float flatZoneAngle = (float)Math.Acos(notchLength / (size * 2)) + (float)Math.PI;

                        arc1 = new ArcSegment();
                        arc1.Size.Width = size;
                        arc1.Size.Height = size;
                        arc1.Point.X = geometryStartPosition.X + (size * (float)Math.Cos(flatZoneAngle));
                        arc1.Point.Y = miniMapStartPosition.Y + miniMapSize.Height / 2 + (size * (float)Math.Sin(flatZoneAngle) * -1);
                        arc1.SweepDirection = SweepDirection.CounterClockwise;

                        arc3 = new ArcSegment();
                        arc3.Point.X = geometryStartPosition.X;
                        arc3.Point.Y = geometryStartPosition.Y;
                        arc3.Size.Width = size;
                        arc3.Size.Height = size;
                        arc3.SweepDirection = SweepDirection.CounterClockwise;

                        notchrectangle.Width = (int)(notchRadiusInPixel / 3) * 2;
                        notchrectangle.Height = notchrectangle.Width;
                        notchrectangle.X = (int)geometryStartPosition.X - (notchrectangle.Width / 2);
                        notchrectangle.Y = (int)(arc1.Point.Y + (notchRadiusInPixel)) - (notchrectangle.Height / 2);

                        using (var geometry = new PathGeometry(target.Factory))
                        {
                            using (var geoSink = geometry.Open())
                            {
                                geoSink.SetFillMode(FillMode.Winding);
                                geoSink.BeginFigure(new RawVector2(geometryStartPosition.X, geometryStartPosition.Y), new FigureBegin());
                                geoSink.AddArc(arc1);
                                geoSink.AddLine(new RawVector2(geometryStartPosition.X + (size * (float)Math.Cos(flatZoneAngle) * -1), arc1.Point.Y));
                                geoSink.AddArc(arc3);
                                geoSink.EndFigure(new FigureEnd());
                                geoSink.Close();

                                var matrix = Transformation.Rotation(waferAngle, new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + miniMapSize.Height / 2f));

                                tfGeometry = new TransformedGeometry(target.Factory, geometry, matrix);

                                RawVector2 notpoint = new RawVector2(notchrectangle.X, notchrectangle.Y);
                                var notchpoint = Transformation.Rotation(notpoint, new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + miniMapSize.Height / 2f), waferAngle);

                                notchrectangle.X = (int)notchpoint.X;
                                notchrectangle.Y = (int)notchpoint.Y;

                                target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                                target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);
                                target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                                target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);
                                target.FillRectangle(new RawRectangleF(notchrectangle.X, notchrectangle.Y, notchrectangle.X + notchrectangle.Width, notchrectangle.Y + notchrectangle.Height), resCache["DarkRedBrush"] as Brush);

                                tfGeometry.Dispose();
                            }
                        }
                    }
                    else
                    {
                        notchLength = (float)waferRadius / 10.0f;
                        notchRadiusInPixel = 14;
                        float flatZoneAngle = (float)Math.Acos(notchLength / waferRadius) + (float)Math.PI;

                        arc1 = new ArcSegment();
                        arc1.Size.Width = size;
                        arc1.Size.Height = size;
                        arc1.Point.X = geometryStartPosition.X + (size * (float)Math.Cos(flatZoneAngle));
                        arc1.Point.Y = miniMapStartPosition.Y + miniMapSize.Height / 2 + (size * (float)Math.Sin(flatZoneAngle) * -1);
                        arc1.SweepDirection = SweepDirection.CounterClockwise;

                        arc2 = new ArcSegment();
                        arc2.Point.X = miniMapStartPosition.X + miniMapSize.Width / 2 - (size * (float)Math.Cos(flatZoneAngle));
                        arc2.Point.Y = arc1.Point.Y;
                        arc2.Size.Width = notchRadiusInPixel;
                        arc2.Size.Height = notchRadiusInPixel;
                        arc2.SweepDirection = SweepDirection.Clockwise;

                        arc3 = new ArcSegment();
                        arc3.Point.X = geometryStartPosition.X;
                        arc3.Point.Y = geometryStartPosition.Y;
                        arc3.Size.Width = size;
                        arc3.Size.Height = size;
                        arc3.SweepDirection = SweepDirection.CounterClockwise;

                        notchellipse.Point.X = arc3.Point.X;
                        notchellipse.Point.Y = arc2.Point.Y + 4;
                        notchellipse.RadiusX = 3;
                        notchellipse.RadiusY = 3;

                        using (var geometry = new PathGeometry(target.Factory))
                        {
                            using (var geoSink = geometry.Open())
                            {
                                geoSink.SetFillMode(FillMode.Winding);
                                geoSink.BeginFigure(new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + 10f), new FigureBegin());
                                geoSink.AddArc(arc1);
                                geoSink.AddArc(arc2);
                                geoSink.AddArc(arc3);
                                geoSink.EndFigure(new FigureEnd());
                                geoSink.Close();

                                var matrix = Transformation.Rotation(waferAngle, new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + miniMapSize.Height / 2f));
                                tfGeometry = new TransformedGeometry(target.Factory, geometry, matrix);

                                var notchpoint = Transformation.Rotation(notchellipse.Point, new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + miniMapSize.Height / 2f), waferAngle);
                                notchellipse.Point = notchpoint;

                                target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                                target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);
                                target.FillEllipse(notchellipse, resCache["DarkRedBrush"] as Brush);

                                tfGeometry.Dispose();
                            }
                        }
                    }

                    RawVector2 fovBoxSize = new RawVector2();
                    RawVector2 fovBoxCen = new RawVector2();

                    //int indexOffsetX = ((Wafer.DispHorFlip == DispFlipEnum.FLIP) ? -1 : 1) * (cenIndexX - CurrXIndex);
                    //int indexOffsetY = ((Wafer.DispVerFlip == DispFlipEnum.FLIP) ? -1 : 1) * (cenIndexY - CurrYIndex);

                    int indexOffsetX = cenIndexX - CurrXIndex;
                    int indexOffsetY = cenIndexY - CurrYIndex;

                    if (stepX <= 0)
                    {
                        stepX = 1000;
                    }

                    if (stepY <= 0)
                    {
                        stepY = 1000;
                    }

                    maxIndexXInView = (waferRadius) / stepX;
                    maxIndexYInView = (waferRadius) / stepY;

                    fovBoxSize.X = (float)((Math.Round((this.ActualWidth / (RectWidth + GutterSize))) * stepX) / waferRadius * (miniMapSize.Width - 20));

                    if (fovBoxSize.X > miniMapSize.Width)
                    {
                        fovBoxSize.X = miniMapSize.Width;
                    }

                    fovBoxSize.Y = (float)((Math.Round((this.ActualHeight / (RectHeight + GutterSize))) * stepY) / waferRadius * (miniMapSize.Height - 20));

                    if (fovBoxSize.Y > miniMapSize.Height)
                    {
                        fovBoxSize.Y = miniMapSize.Height;
                    }

                    bool xOutOfFov = false, yOutOfFov = false;

                    fovBoxCen.X = (float)(indexOffsetX / Math.Round(maxIndexXInView) * 100.0f) * -1.0f + miniMapStartPosition.X + miniMapSize.Width / 2f;

                    if (fovBoxCen.X < miniMapStartPosition.X + fovBoxSize.X / 2)
                    {
                        fovBoxCen.X = miniMapStartPosition.X + fovBoxSize.X / 2f;
                        xOutOfFov = true;
                    }

                    if (fovBoxCen.X > miniMapStartPosition.X + miniMapSize.Width - fovBoxSize.X / 2)
                    {
                        fovBoxCen.X = miniMapStartPosition.X + miniMapSize.Width - fovBoxSize.X / 2f;
                        xOutOfFov = true;
                    }

                    fovBoxCen.Y = (float)(indexOffsetY / Math.Round(maxIndexYInView) * 100.0f) * 1.0f + miniMapStartPosition.Y + miniMapSize.Height / 2f;

                    if (fovBoxCen.Y < miniMapStartPosition.Y + fovBoxSize.Y / 2)
                    {
                        fovBoxCen.Y = miniMapStartPosition.Y + fovBoxSize.Y / 2;
                        yOutOfFov = true;
                    }

                    if (fovBoxCen.Y > miniMapStartPosition.Y + miniMapSize.Height - fovBoxSize.Y / 2)
                    {
                        fovBoxCen.Y = miniMapStartPosition.Y + miniMapSize.Height - fovBoxSize.Y / 2;
                        yOutOfFov = true;
                    }

                    if (xOutOfFov || yOutOfFov)
                    {
                        target.DrawRectangle(new RawRectangleF(fovBoxCen.X - fovBoxSize.X / 2f, fovBoxCen.Y - fovBoxSize.Y / 2f, fovBoxSize.X + fovBoxCen.X - fovBoxSize.X / 2f, fovBoxSize.Y + fovBoxCen.Y - fovBoxSize.Y / 2f), resCache["failBrush"] as Brush);
                    }
                    else
                    {
                        target.DrawRectangle(new RawRectangleF(fovBoxCen.X - fovBoxSize.X / 2f, fovBoxCen.Y - fovBoxSize.Y / 2f, fovBoxCen.X - fovBoxSize.X / 2f + fovBoxSize.X, fovBoxCen.Y - fovBoxSize.Y / 2f + fovBoxSize.Y), resCache["edgeBrush"] as Brush);
                    }

                    target.DrawLine(new RawVector2((float)(fovBoxCen.X - 3), (float)(fovBoxCen.Y)), new RawVector2((float)(fovBoxCen.X + 3), (float)(fovBoxCen.Y)), resCache["textBrush"] as Brush, 1f);
                    target.DrawLine(new RawVector2((float)(fovBoxCen.X), (float)(fovBoxCen.Y - 3)), new RawVector2((float)(fovBoxCen.X), (float)(fovBoxCen.Y + 3)), resCache["textBrush"] as Brush, 1f);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private void DrawCrossLine(RenderTarget target)
        {
            try
            {
                if (IsCrossLineVisible)
                {
                    int flipCurrXIndex = CursorXIndex;
                    int flipCurrYIndex = CursorYIndex;

                    if (Wafer.DispHorFlip == DispFlipEnum.FLIP)
                    {
                        flipCurrXIndex = (int)mapCountX - CursorXIndex;
                    }
                    if (Wafer.DispVerFlip == DispFlipEnum.FLIP)
                    {
                        flipCurrYIndex = (int)mapCountY - CursorYIndex;
                    }

                    var flipposx = (flipCurrXIndex - CurrXIndex) * (RectWidth + GutterSize) + (float)this.ActualWidth / 2f - (RectWidth + GutterSize) / 2f - subDevOffset.X;
                    var flipposy = (CurrYIndex - flipCurrYIndex) * (RectHeight + GutterSize) + (float)this.ActualHeight / 2f - (RectHeight + GutterSize) / 2f - subDevOffset.Y;

                    target.DrawLine(new RawVector2((float)(0), (float)((flipposy + RectHeight / 2)) - 1), new RawVector2((float)(this.ActualWidth), (float)((flipposy + RectHeight / 2)) - 1), resCache["textBrush"] as Brush, 1f);
                    target.DrawLine(new RawVector2((float)(flipposx + RectWidth / 2 - 1), (float)(0)), new RawVector2((float)(flipposx + RectWidth / 2 - 1), (float)(this.ActualHeight)), resCache["textBrush"] as Brush, 1f);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
       
        private void DrawDIE(RenderTarget target, IDeviceObject[,] DIEs)
        {
            try
            {
                float posx, posy = 0;

                long inFOVDieCount = 0;
                float overlapped = 1.3f;

                int leftBorder, rightBorder, topBorder, botBorder;

                int borderflipX = (Wafer.DispHorFlip == DispFlipEnum.FLIP) ? mapCountX - CurrXIndex : CurrXIndex;
                int borderflipY = (Wafer.DispVerFlip == DispFlipEnum.FLIP) ? mapCountY - CurrYIndex : CurrYIndex;

                leftBorder = borderflipX - (int)Math.Ceiling(this.ActualWidth / (RectWidth + GutterSize) * overlapped / 2);
                rightBorder = borderflipX + (int)Math.Ceiling(this.ActualWidth / (RectWidth + GutterSize) * overlapped / 2);
                topBorder = borderflipY - (int)Math.Ceiling(this.ActualHeight / (RectHeight + GutterSize) * overlapped / 2);
                botBorder = borderflipY + (int)Math.Ceiling(this.ActualHeight / (RectHeight + GutterSize) * overlapped / 2);

                long devCnt;
                long validDieCount = 0;

                devCnt = (DIEs.GetUpperBound(0) + 1) * (DIEs.GetUpperBound(1) + 1);

                if (devCnt > 0)
                {
                    int xidx, yidx;

                    if (topBorder < 0)
                    {
                        topBorder = 0;
                    }

                    if (leftBorder < 0)
                    {
                        leftBorder = 0;
                    }

                    for (int y = topBorder; y < botBorder; y++)
                    {
                        for (int x = leftBorder; x < rightBorder; x++)
                        {
                            if (y < DIEs.GetUpperBound(1) + 1 && x < DIEs.GetUpperBound(0) + 1)
                            {
                                if (DIEs[x, y] != null)
                                {
                                    if (DIEs[x, y].DieType.Value != DieTypeEnum.NOT_EXIST)
                                    {
                                        inFOVDieCount++;

                                        xidx = (int)DIEs[x, y].DieIndexM.XIndex;
                                        yidx = (int)DIEs[x, y].DieIndexM.YIndex;

                                        posx = (xidx - CurrXIndex) * (RectWidth + GutterSize) + (float)this.ActualWidth / 2f - (RectWidth + GutterSize) / 2f - subDevOffset.X;
                                        posy = (CurrYIndex - yidx) * (RectHeight + GutterSize) + (float)this.ActualHeight / 2f - (RectHeight + GutterSize) / 2f - subDevOffset.Y;

                                        if (xidx == CurrXIndex & yidx == CurrYIndex)
                                        {
                                            if (DIEs[x, y].CurTestHistory != null)
                                            {
                                                CurrBinCode = DIEs[x, y].CurTestHistory.BinCode.Value;
                                            }

                                            PointedDie = DIEs[x, y];
                                        }


                                        if (Wafer.DispHorFlip == DispFlipEnum.FLIP)
                                        {
                                            int flipCurrXIndex = (int)mapCountX - xidx;
                                            float flipposx = (flipCurrXIndex - CurrXIndex) * (RectWidth + GutterSize) + (float)this.ActualWidth / 2f - (RectWidth + GutterSize) / 2f - subDevOffset.X;
                                            posx = flipposx;// - 200;
                                        }

                                        if (Wafer.DispVerFlip == DispFlipEnum.FLIP)
                                        {
                                            int flipCurrYIndex = (int)mapCountY - yidx;
                                            float flipposy = (CurrYIndex - flipCurrYIndex) * (RectHeight + GutterSize) + (float)this.ActualHeight / 2f - (RectHeight + GutterSize) / 2f - subDevOffset.Y;
                                            posy = flipposy;// -200;
                                        }

                                        if (posx < -(RectWidth + GutterSize) || posx > (this.ActualWidth + RectWidth + GutterSize) ||
                                            posy < -(RectHeight + GutterSize) || posy > (this.ActualHeight + RectHeight + GutterSize))
                                        {
                                            switch (DIEs[x, y].DieType.Value)
                                            {
                                                case DieTypeEnum.UNKNOWN:
                                                    break;
                                                case DieTypeEnum.NOT_EXIST:
                                                case DieTypeEnum.MARK_DIE:
                                                    break;
                                                case DieTypeEnum.TEST_DIE:
                                                    break;
                                                case DieTypeEnum.SKIP_DIE:
                                                    validDieCount++;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            validDieCount = DrawDevice(target, DIEs[x, y], RectWidth, RectHeight, posx, posy, validDieCount);

                                            // Current Die
                                            if (xidx == CursorXIndex & yidx == CursorYIndex)
                                            {
                                                if (Wafer.MapViewCurIndexVisiablity)
                                                {

                                                    if (UnderDutDices != null)
                                                    {
                                                        if (!(UnderDutDices.Count != 0 && IsDutVisible))
                                                        {
                                                            target.DrawRectangle(new RawRectangleF(posx, posy, posx + RectWidth, posy + RectHeight), resCache["CurBrush"] as Brush, GutterSize * 1.5f);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        target.DrawRectangle(new RawRectangleF(posx, posy, posx + RectWidth, posy + RectHeight), resCache["CurBrush"] as Brush, GutterSize * 1.5f);
                                                    }
                                                }
                                            }

                                            if (IsMouseDown && xidx == SelectedXIndex && yidx == SelectedYIndex && !IsMouseDownMove)
                                            {
                                                target.FillRectangle(new RawRectangleF(posx, posy, posx + RectWidth, posy + RectHeight), resCache["FirstDutBrush"] as Brush);
                                            }

                                            if ((Wafer.IsMapViewShowPMITable == true) || (Wafer.IsMapViewShowPMIEnable == true))
                                            {
                                                int index = Wafer.PMIInfo.SelectedNormalPMIMapTemplateIndex;
                                                var PMINormalMap = Wafer.PMIInfo.NormalPMIMapTemplateInfo[index];

                                                if (PMINormalMap != null)
                                                {
                                                    if (DIEs[x, y].DieType.Value == DieTypeEnum.TEST_DIE)
                                                    {
                                                        var Enable = PMINormalMap.GetEnable(x, y);

                                                        if ((RectWidth > 5 & RectHeight > 5) && (Enable == 0x01))
                                                        {
                                                            if (Wafer.IsMapViewShowPMITable)
                                                            {
                                                                string tableNum = (PMINormalMap.GetTable(x, y)).ToString();

                                                                target.DrawText(tableNum, textFormat, new RawRectangleF(posx + GutterSize / 2.0f, posy + GutterSize / 2.0f, posx + GutterSize / 2.0f + 42f, posy + GutterSize / 2.0f + 10f), resCache["textBrush"] as Brush);
                                                            }

                                                            if (Wafer.IsMapViewShowPMIEnable)
                                                            {
                                                                DrawPMIObject(target, Enable, rectHalfWidth, rectHalfWidth, posx, posy);

                                                                //DrawIndexs.Add(new MachineIndex(x, y));
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            #region // Multi device draw

                                            if (drawMultDev == true)
                                            {
                                                if (DIEs[x, y].GetChildren().Count > 0)
                                                {
                                                    List<IDeviceObject> childs = DIEs[x, y].GetChildren();

                                                    float childOffsetX, childOffsetY;
                                                    float childWidth, childHeight;

                                                    for (int subIndex = 0; subIndex < childs.Count; subIndex++)
                                                    {
                                                        var currTransform = target.Transform;

                                                        childWidth = (float)childs[subIndex].Size.Width.Value / umPerPixel - GutterSize;
                                                        childHeight = (float)childs[subIndex].Size.Height.Value / umPerPixel - GutterSize;

                                                        if (childWidth < 10 | childHeight < 10)
                                                        {
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            childOffsetX = (float)childs[subIndex].Position.GetX() / umPerPixel + GutterSize / 2f;
                                                            childOffsetY = RectHeight - ((float)childs[subIndex].Position.GetY() / umPerPixel + GutterSize / 2f) - childHeight;

                                                            if (childs[subIndex].Position.GetT() != 0)
                                                            {
                                                                var matrix = Transformation.Rotation((float)childs[subIndex].Position.GetT(), new RawVector2(posx + childOffsetX + childWidth / 2f, posy + childOffsetY + childHeight / 2f));
                                                                target.Transform = matrix;

                                                                validDieCount = DrawDevice(target, childs[subIndex], childWidth, childHeight, posx + childOffsetX, posy + childOffsetY, validDieCount);
                                                                target.Transform = currTransform;
                                                            }
                                                            else
                                                            {
                                                                validDieCount = DrawDevice(target, childs[subIndex], childWidth, childHeight, posx + childOffsetX, posy + childOffsetY, validDieCount);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            if (!IsDutVisible)
                                            {
                                                if (SelectedDie != null & !IsMouseDownMove)
                                                {
                                                    if (SelectedDie.Count > 0)
                                                    {
                                                        if (SelectedDie.ToList().Find(d => (d.DieIndexM.XIndex == xidx) & (d.DieIndexM.YIndex == yidx)) != null)
                                                        {
                                                            target.DrawRectangle(new RawRectangleF(posx + GutterSize / 2.0f, posy + GutterSize / 2.0f, posx + GutterSize / 2.0f + RectWidth - GutterSize * 1.0f, posy + GutterSize / 2.0f + RectHeight - GutterSize * 1.0f), resCache["edgeBrush"] as Brush, GutterSize * 1.0f);
                                                            target.FillRectangle(new RawRectangleF(posx, posy, posx + RectWidth, posy + RectHeight), resCache["OrangeBrush"] as Brush);
                                                        }
                                                    }
                                                }
                                            }

                                            if (HighlightDies != null && HighlightDies.Count > 0)
                                            {
                                                HighlightDieComponent tmphighlightdie = null;

                                                tmphighlightdie = HighlightDies.ToList().Find(d => (d.MI.XIndex == xidx) & (d.MI.YIndex == yidx));

                                                if (tmphighlightdie != null)
                                                {
                                                    target.DrawRectangle(new RawRectangleF(posx + GutterSize / 2.0f, posy + GutterSize / 2.0f, posx + GutterSize / 2.0f + RectWidth - GutterSize * 1.0f, posy + GutterSize / 2.0f + RectHeight - GutterSize * 1.0f), resCache["passBrush"] as Brush, GutterSize * 1.0f);
                                                }
                                            }
                                        }
                                    }
                                }
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
        private void DrawDUT(RenderTarget target, IDeviceObject[,] DIEs)
        {
            try
            {
                if (IsDutVisible == true)
                {
                    if (UnderDutDices != null)
                    {
                        if (UnderDutDices.Count > 0)
                        {
                            int xidx, yidx;

                            FindFirstDut();

                            List<IDeviceObject> underdutdices = UnderDutDices.ToList();

                            foreach (var dutdie in underdutdices)
                            {
                                long? xindex = null;
                                long? yindex = null;
                                int? dutnumber = null;

                                xindex = dutdie?.DieIndexM?.XIndex;
                                yindex = dutdie?.DieIndexM?.YIndex;

                                dutnumber = dutdie?.DutNumber;

                                if ((xindex != null) && (yindex != null) && (dutnumber != null))
                                {
                                    xidx = Convert.ToInt32(xindex);
                                    yidx = Convert.ToInt32(yindex);

                                    int flipCurrXIndex = (int)xindex;
                                    int flipCurrYIndex = (int)yindex;

                                    // TODO: FLIP 검토 할것 
                                    if (Wafer.DispHorFlip == DispFlipEnum.FLIP)
                                    {
                                        flipCurrXIndex = mapCountX - (int)xindex;
                                    }
                                    if (Wafer.DispVerFlip == DispFlipEnum.FLIP)
                                    {
                                        flipCurrYIndex = mapCountY - (int)yindex;
                                    }

                                    float flipposx = (flipCurrXIndex - CurrXIndex) * (RectWidth + GutterSize) + (float)this.ActualWidth / 2f - (RectWidth + GutterSize) / 2f - subDevOffset.X;
                                    float flipposy = (CurrYIndex - flipCurrYIndex) * (RectHeight + GutterSize) + (float)this.ActualHeight / 2f - (RectHeight + GutterSize) / 2f - subDevOffset.Y;

                                    if (dutnumber != 1)
                                    {
                                        target.DrawRectangle(new RawRectangleF(flipposx - GutterSize / 2.0f, flipposy - GutterSize / 2.0f, flipposx + RectWidth + GutterSize / 2.0f, flipposy + RectHeight + GutterSize / 2.0f), resCache["CurBrush"] as Brush, GutterSize * 1.5f);
                                    }
                                    else
                                    {
                                        target.DrawRectangle(new RawRectangleF(flipposx - GutterSize / 2.0f, flipposy - GutterSize / 2.0f, flipposx + RectWidth + GutterSize / 2.0f, flipposy + RectHeight + GutterSize / 2.0f), resCache["CurBrush"] as Brush, GutterSize * 1.5f);

                                    }

                                    if (RectWidth > 42 & RectHeight > 10)
                                    {
                                        if (xidx <= DIEs.GetUpperBound(0) &&
                                            yidx <= DIEs.GetUpperBound(1) &&
                                            xidx >= 0 && yidx >= 0)
                                        {
                                            if (DIEs[xidx, yidx] != null && DIEs[xidx, yidx].CurTestHistory != null)
                                            {
                                                target.DrawText($"{DIEs[xidx, yidx].CurTestHistory.DutIndex.Value}: {xidx},{yidx}", textFormat, new RawRectangleF(flipposx + GutterSize / 2.0f, flipposy + GutterSize / 2.0f, flipposx + GutterSize / 2.0f + 42f, flipposy + GutterSize / 2.0f + 10f), resCache["textBrush"] as Brush);
                                            }
                                        }
                                        else
                                        {
                                            target.DrawText($"{dutnumber}: {xidx},{yidx}", textFormat, new RawRectangleF(flipposx + GutterSize / 2.0f, flipposy + GutterSize / 2.0f, flipposx + GutterSize / 2.0f + 42f, flipposy + GutterSize / 2.0f + 10f), resCache["textBrush"] as Brush);
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }
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


        private void DrawCurIndex(RenderTarget target)
        {
            try
            {
                if (Wafer.MapViewCurIndexVisiablity)
                {
                    Brush brush = resCache["infoBackgroundBrush"] as Brush;
                    brush.Opacity = (float)0.5;

                    if (CoordinateManager != null)
                    {
                        UserIndex index = CoordinateManager.WMIndexConvertWUIndex(CursorXIndex, CursorYIndex);

                        target.DrawText(string.Format("UI:{0,4:+#;-#;0},{1,4:+#;-#;0}", index.XIndex, index.YIndex), uiTextFormat, new RawRectangleF(12, 8, 150.0f, 36.0f), resCache["textBrush"] as Brush);
                    }
                    else
                    {
                        target.DrawText(string.Format("MI:{0,4:+#;-#;0}, {1,4:+#;-#;0}", CursorXIndex, CursorYIndex), uiTextFormat, new RawRectangleF(12, 8, 150.0f, 36.0f), resCache["textBrush"] as Brush);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //private void DrawTestCode(RenderTarget target)
        //{
        //    try
        //    {
        //        target.DrawText($"L({leftcount}), R({rightcount}), LH({lefthidecount}), RH({righthidecount})", uiTextFormat, new RawRectangleF(12, 40, 300.0f, 58.0f), resCache["textBrush"] as Brush);
        //        target.DrawText($"U({uppercount}), B({bottomcount}), UH({upperhidecount}), BH({bottomhidecount})", uiTextFormat, new RawRectangleF(12, 60, 300.0f, 88.0f), resCache["textBrush"] as Brush);

        //        target.DrawText($"CurrX : {CurrXIndex}, CurrY : {CurrYIndex}", uiTextFormat, new RawRectangleF(12, 90, 300.0f, 118.0f), resCache["textBrush"] as Brush);
        //        target.DrawText($"{Wafer.DispHorFlip}, {Wafer.DispVerFlip}", uiTextFormat, new RawRectangleF(12, 120, 200.0f, 148), resCache["textBrush"] as Brush);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        /// <summary>
        /// Does the actual rendering. 
        /// BeginDraw and EndDraw are already called by the caller. 
        /// </summary>
        public override void Render(RenderTarget target)
        {
            try
            {
                bool CanRender = false;

                if (WaferObject != null && Wafer?.WaferDevObjectRef != null &&
                    Wafer.GetSubsInfo() != null &&
                    Wafer?.GetPhysInfo().DieSizeY != null &&
                    Wafer.GetSubsInfo().ActualDieSize.Width.Value > 0 &&
                    Wafer.GetSubsInfo().ActualDieSize.Height.Value > 0 &&
                    Wafer.DeviceModule()?.ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    CanRender = true;
                }
                else
                {
                    CanRender = false;
                }

                if (!CanRender)
                {
                    return;
                }

                try
                {
                    SetFOV();

                    target.Clear(new RawColor4(105 / 255f, 105 / 255f, 105 / 255f, 1));

                    rectWidth = RecSize;
                    rectHeight = RecSize;

                    rectHalfWidth = rectWidth / 2;
                    rectHalfHeight = rectHeight / 2;

                    InitColorPallette(target);

                    GetCenterIndex(WaferObject, out cenIndexX, out cenIndexY);

                    waferRadius = Wafer.GetPhysInfo().WaferSize_um.Value;

                    stepX = (float)Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                    stepY = (float)Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                    double aspectRatio = stepX / stepY;
                    rectWidth = (float)((rectHeight + GutterSize) * aspectRatio) - GutterSize;

                    RectWidth = rectWidth;
                    RectHeight = rectHeight;

                    mapCountX = Wafer.GetPhysInfo().MapCountX.Value - 1;
                    mapCountY = Wafer.GetPhysInfo().MapCountY.Value - 1;

                    CalcWaferAngle();

                    DrawWaferOutline(target);

                    IDeviceObject[,] DIEs = null;

                    if (Wafer.GetSubsInfo().DIEs != null)
                    {
                        DIEs = Wafer.GetSubsInfo().DIEs.Clone() as IDeviceObject[,];

                        DrawDIE(target, DIEs);

                        DrawDUT(target, DIEs);
                    }

                    DrawMiniMap(target);

                    DrawCrossLine(target);

                    DrawCurIndex(target);

                    target.AntialiasMode = AntialiasMode.Aliased;
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
        #endregion
    }
}

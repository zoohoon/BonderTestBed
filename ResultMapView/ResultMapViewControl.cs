using DXControlBase;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProbingDataInterface;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace ResultMapView
{
    public class ResultMapViewControl : D2dControl, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty MultiDevVisibleProperty =
                DependencyProperty.Register("MultiDevVisible", typeof(bool), typeof(ResultMapViewControl), new FrameworkPropertyMetadata(false));
        public bool MultiDevVisible
        {
            get { return (bool)this.GetValue(MultiDevVisibleProperty); }
            set { this.SetValue(MultiDevVisibleProperty, value); }
        }

        public static readonly DependencyProperty SelectedXIndexProperty =
         DependencyProperty.Register("SelectedXIndex", typeof(int), typeof(ResultMapViewControl),
             new FrameworkPropertyMetadata((int)0));
        public int SelectedXIndex
        {
            get { return (int)this.GetValue(SelectedXIndexProperty); }
            set { this.SetValue(SelectedXIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedYIndexProperty =
        DependencyProperty.Register("SelectedYIndex", typeof(int), typeof(ResultMapViewControl), new FrameworkPropertyMetadata((int)0));
        public int SelectedYIndex
        {
            get { return (int)this.GetValue(SelectedYIndexProperty); }
            set { this.SetValue(SelectedYIndexProperty, value); }
        }

        public static readonly DependencyProperty MaxXCntProperty =
            DependencyProperty.Register("MaxXCnt", typeof(int), typeof(ResultMapViewControl), new FrameworkPropertyMetadata((int)0));
        public int MaxXCnt
        {
            get { return (int)this.GetValue(MaxXCntProperty); }
            set { this.SetValue(MaxXCntProperty, value); }
        }

        public static readonly DependencyProperty MaxYCntProperty =
            DependencyProperty.Register("MaxYCnt", typeof(int), typeof(ResultMapViewControl), new FrameworkPropertyMetadata((int)0));
        public int MaxYCnt
        {
            get { return (int)this.GetValue(MaxYCntProperty); }
            set { this.SetValue(MaxYCntProperty, value); }
        }

        public static readonly DependencyProperty IsCrossLineVisibleProperty =
        DependencyProperty.Register("IsCrossLineVisible", typeof(bool), typeof(ResultMapViewControl),
            new FrameworkPropertyMetadata(true, new PropertyChangedCallback(IsCrossLineVisiblePropertyChanged)));
        public bool IsCrossLineVisible
        {
            get { return (bool)this.GetValue(IsCrossLineVisibleProperty); }
            set { this.SetValue(IsCrossLineVisibleProperty, value); }
        }

        public static readonly DependencyProperty HighlightDiesProperty =
          DependencyProperty.Register("HighlightDies", typeof(AsyncObservableCollection<HighlightDieComponent>), typeof(ResultMapViewControl),
              new FrameworkPropertyMetadata(new AsyncObservableCollection<HighlightDieComponent>()));
        public AsyncObservableCollection<HighlightDieComponent> HighlightDies
        {
            get { return (AsyncObservableCollection<HighlightDieComponent>)this.GetValue(HighlightDiesProperty); }
            set { this.SetValue(HighlightDiesProperty, value); }
        }

        public static readonly DependencyProperty PointedDieProperty = DependencyProperty.Register("PointedDie", typeof(IDeviceObject), typeof(ResultMapViewControl), new FrameworkPropertyMetadata(null));
        public IDeviceObject PointedDie
        {
            get { return (IDeviceObject)this.GetValue(PointedDieProperty); }
            set { this.SetValue(PointedDieProperty, value); }
        }

        public static readonly DependencyProperty BINTypeProperty = DependencyProperty.Register("BINType",
                        typeof(BinType), typeof(ResultMapViewControl),
                        new FrameworkPropertyMetadata(BinType.BIN_PASSFAIL));

        public BinType BINType
        {
            get { return (BinType)this.GetValue(BINTypeProperty); }
            set { this.SetValue(BINTypeProperty, value); }
        }

        public static readonly DependencyProperty CursorXIndexProperty =
         DependencyProperty.Register("CursorXIndex", typeof(int), typeof(ResultMapViewControl),
             new FrameworkPropertyMetadata((int)0, new PropertyChangedCallback(CursorXIndexPropertyChanged)));
        public int CursorXIndex
        {
            get { return (int)this.GetValue(CursorXIndexProperty); }
            set { this.SetValue(CursorXIndexProperty, value); }
        }

        public static readonly DependencyProperty CursorYIndexProperty =
         DependencyProperty.Register("CursorYIndex", typeof(int), typeof(ResultMapViewControl),
             new FrameworkPropertyMetadata((int)0, new PropertyChangedCallback(CursorYIndexPropertyChanged)));
        public int CursorYIndex
        {
            get { return (int)this.GetValue(CursorYIndexProperty); }
            set { this.SetValue(CursorYIndexProperty, value); }
        }

        public static readonly DependencyProperty RenderModeProperty =
        DependencyProperty.Register("RenderMode", typeof(MapViewMode), typeof(ResultMapViewControl),
            new FrameworkPropertyMetadata(MapViewMode.MapMode));
        public MapViewMode RenderMode
        {
            get { return (MapViewMode)this.GetValue(RenderModeProperty); }
            set { this.SetValue(RenderModeProperty, value); }
        }

        public static readonly DependencyProperty CurrBinCodeProperty =
         DependencyProperty.Register("CurrBinCode", typeof(int), typeof(ResultMapViewControl), new FrameworkPropertyMetadata((int)0));
        public int CurrBinCode
        {
            get { return (int)this.GetValue(CurrBinCodeProperty); }
            set { this.SetValue(CurrBinCodeProperty, value); }
        }

        public static readonly DependencyProperty CoordinateManagerProperty = DependencyProperty.Register("CoordinateManager", typeof(ICoordinateManager), typeof(ResultMapViewControl), null);
        public ICoordinateManager CoordinateManager
        {
            get { return (ICoordinateManager)this.GetValue(CoordinateManagerProperty); }
            set { this.SetValue(CoordinateManagerProperty, value); }
        }


        public static readonly DependencyProperty WaferObjectProperty = DependencyProperty.Register("WaferObject", typeof(IWaferObject), typeof(ResultMapViewControl),
        new FrameworkPropertyMetadata(null, new PropertyChangedCallback(WaferObjectPropertyChanged)));
        public IWaferObject WaferObject
        {
            get { return (IWaferObject)this.GetValue(WaferObjectProperty); }
            set
            {
                this.SetValue(WaferObjectProperty, value);
                Wafer = (WaferObject)WaferObject;
            }
        }

        public static readonly DependencyProperty ASCIIMapProperty = DependencyProperty.Register("ASCIIMap", typeof(char[,]), typeof(ResultMapViewControl),
       new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ASCIIMapPropertyChanged)));
        public char[,] ASCIIMap
        {
            get { return (char[,])this.GetValue(ASCIIMapProperty); }
            set
            {
                this.SetValue(ASCIIMapProperty, value);
                ASCIIMapInfo = (char[,])ASCIIMap;
            }
        }

        public static readonly DependencyProperty DisplayBinCodeProperty =
            DependencyProperty.Register(nameof(DisplayBinCode), typeof(bool), typeof(ResultMapViewControl), new FrameworkPropertyMetadata(null));
        public bool DisplayBinCode
        {
            get { return (bool)this.GetValue(DisplayBinCodeProperty); }
            set { this.SetValue(DisplayBinCodeProperty, value); }
        }


        public static readonly DependencyProperty IsMiniMapVisibleProperty =
        DependencyProperty.Register("IsMiniMapVisible", typeof(bool), typeof(ResultMapViewControl), new FrameworkPropertyMetadata(false));
        public bool IsMiniMapVisible
        {
            get { return (bool)this.GetValue(IsMiniMapVisibleProperty); }
            set { this.SetValue(IsMiniMapVisibleProperty, value); }
        }


        public static readonly DependencyProperty RecSizeProperty =
         DependencyProperty.Register("RecSize", typeof(float), typeof(ResultMapViewControl), new FrameworkPropertyMetadata((float)0));
        public float RecSize
        {
            get { return (float)this.GetValue(RecSizeProperty); }
            set { this.SetValue(RecSizeProperty, value); }
        }

        public static readonly DependencyProperty CurrXSubIndexProperty =
            DependencyProperty.Register("CurrXSubIndex", typeof(int), typeof(ResultMapViewControl), new FrameworkPropertyMetadata((int)0));
        public int CurrXSubIndex
        {
            get { return (int)this.GetValue(CurrXSubIndexProperty); }
            set { this.SetValue(CurrXSubIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrYSubIndexProperty =
         DependencyProperty.Register("CurrYSubIndex", typeof(int), typeof(ResultMapViewControl), new FrameworkPropertyMetadata((int)0));
        public int CurrYSubIndex
        {
            get { return (int)this.GetValue(CurrYSubIndexProperty); }
            set { this.SetValue(CurrYSubIndexProperty, value); }
        }

        #endregion

        #region Properties

        private bool initflag = false;
        private bool InitIsCrossLineVisible = true;

        public Dictionary<int, System.Windows.Media.Color> BinColors { get; set; }
        public Dictionary<int, SolidColorBrush> dictBinCodeSolidBrush;

        private WaferObject Wafer;
        private char[,] ASCIIMapInfo;

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

        private float _PreZoomLevel = 0;

        private float _ZoomLevel;

        public float ZoomLevel
        {
            get { return _ZoomLevel; }
            set { _ZoomLevel = value; }
        }

        private double _WaferRadiusInPixel;

        public double WaferRadiusInPixel
        {
            get { return _WaferRadiusInPixel; }
            set
            {
                _WaferRadiusInPixel = value;
                //WaferObject.ZoomLevel = ZoomLevel;
                //ZoomLevel = ZoomLevel;

                //ZoomLevel = WaferObject.ZoomLevel;

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

        public System.Windows.Point MouseDownPos { get; set; }

        public float GutterSize { get; set; }

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

        private int _CurrXIndex;
        public int CurrXIndex
        {
            get { return _CurrXIndex; }
            set
            {
                if (value != _CurrXIndex)
                {
                    int Pre = _CurrXIndex;
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
                    int Pre = _CurrYIndex;
                    _CurrYIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private float _MinimapWidth = 120;
        private float _MinimapHeight = 120;

        private int _PreXIndex = 0;
        private int _PreYIndex = 0;

        float umPerPixel_X;
        float umPerPixel_Y;

        public bool IsMouseDown { get; set; }
        public bool IsMouseDownMove { get; set; }
        private bool isMinimapClicked = false;

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

        RawVector2 waferCenPos = new RawVector2();
        RawVector2 subDevOffset = new RawVector2(0, 0);
        RawVector2 geometryStartPosition = new RawVector2();
        RawVector2 pointVector = new RawVector2();
        RawVector2 miniMapStartPosition = new RawVector2();

        static SharpDX.DirectWrite.Factory fontFactory = new SharpDX.DirectWrite.Factory();

        TextFormat uiTextFormat = new TextFormat(fontFactory, "Segoe UI", SharpDX.DirectWrite.FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, 20.0f);

        public List<TextFormat> TextFormats = new List<TextFormat>();
        TextFormatSet TextFormatSet { get; set; }

        SharpDX.Direct2D1.TransformedGeometry tfGeometry;

        #endregion

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
                throw;
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
                LoggerManager.Debug($"[MapViewControl] [CalcMinimapClick()] : {err}");
            }
        }

        private bool IsMinimapClicked(System.Windows.Point pos)
        {
            bool retVal = false;
            try
            {
                retVal = IsMiniMapVisible && this.ActualWidth > 256 && this.ActualHeight > 256 &&
                        pos.X >= miniMapStartPosition.X &&
                        miniMapStartPosition.X + _MinimapWidth >= pos.X &&
                        pos.Y >= miniMapStartPosition.Y &&
                        miniMapStartPosition.Y + _MinimapHeight >= pos.Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private bool FindInRangeSubDev(SubDieObject subDev, double subDiffX, double subDiffY)
        {
            bool inRange = false;
            try
            {
                var childWidth = (float)subDev.Size.Width.Value / umPerPixel_X - GutterSize;
                var childHeight = (float)subDev.Size.Height.Value / umPerPixel_Y - GutterSize;

                var childOffsetX = (float)subDev.Position.GetX() / umPerPixel_X;
                var childOffsetY = ((float)subDev.Position.GetY() / umPerPixel_Y);

                double baseDevHeight = (RectHeight + GutterSize);

                if ((subDiffX > childOffsetX & subDiffX < childOffsetX + childWidth) &
                    (subDiffY > childOffsetY & subDiffY < childOffsetY + childHeight))
                {
                    inRange = true;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            return inRange;
        }

        #region Event

        private void MapViewControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (IsMouseDown == true)
                {
                    double diffX;
                    double diffY;
                    System.Windows.Point currPos = new System.Windows.Point();
                    currPos = e.GetPosition((System.Windows.IInputElement)sender);

                    if (IsMinimapClicked(currPos) | isMinimapClicked)
                    {

                        int indexX, indexY;
                        CalcMinimapClick(currPos, out indexX, out indexY);

                        //this.CurrXIndex = indexX;
                        //this.CurrYIndex = indexY;

                        SetCurrXIndex(indexX);
                        SetCurrYIndex(indexY);
                        //this.CursorXIndex = indexX;
                        //this.CursorYIndex = indexY;
                    }

                    else
                    {
                        diffX = (MouseDownPos.X - currPos.X);
                        diffY = (MouseDownPos.Y - currPos.Y);

                        //int prevIndexX = CurrXIndex;
                        //int prevIndexY = CurrYIndex;
                        //int prevIndexX = CursorXIndex;
                        //int prevIndexY = CursorYIndex;

                        if (Math.Abs(diffX) > RectWidth + GutterSize | Math.Abs(diffY) > RectHeight + GutterSize)
                        {
                            if (MultiDevVisible == true)
                            {
                                DeviceObject baseDev;
                                //baseDev = Wafer.Devices.Find(d => d.DieIndexM.XIndex == CurrXIndex & d.DieIndexM.YIndex == CurrYIndex);
                                //baseDev = (DeviceObject)Wafer.Map(CurrXIndex, CurrYIndex);
                                baseDev = (DeviceObject)Wafer.Map(CursorXIndex, CursorYIndex);

                                if (Math.Abs(diffX) < (RectWidth + GutterSize) & Math.Abs(diffY) < (RectHeight + GutterSize))
                                {
                                    if (baseDev != null)
                                    {
                                        if (baseDev.SubDevice.Children.Count > 0)
                                        {
                                            SubDieObject subDev;
                                            DeviceObject baseDie;
                                            //subDev = baseDev.SubDevice.Children.FirstOrDefault(c => c.DieIndexM.XIndex == Wafer.CurSubIndexX & c.DieIndexM.YIndex == Wafer.CurSubIndexY);

                                            baseDie = (DeviceObject)(Wafer.Map(CurrXIndex, CurrYIndex));
                                            subDev = (SubDieObject)baseDie.SubDev(CurrXSubIndex, CurrYSubIndex);

                                            var orgSubDev = baseDev.SubDevice.Children.FirstOrDefault(c => c.DieIndexM.XIndex == DownXSubIdx & c.DieIndexM.YIndex == DownYSubIdx);
                                            if (subDev != null)
                                            {
                                                if (orgSubDev == null) orgSubDev = baseDev.SubDevice.Children[0];
                                                double orgX, orgY;
                                                double subDiffX, subDiffY;
                                                double locX, locY;
                                                orgX = orgSubDev.Position.GetX();
                                                orgY = orgSubDev.Position.GetY();
                                                subDiffX = diffX % (RectWidth + GutterSize);
                                                subDiffY = diffY % (RectHeight + GutterSize) * -1;
                                                //if (subDiffX < 0) subDiffX = (RectWidth + GutterSize) - subDiffX;
                                                //if (subDiffY < 0) subDiffY = (RectHeight + GutterSize) - subDiffY;

                                                var childWidth = (float)orgSubDev.Size.Width.Value / umPerPixel_X - GutterSize;
                                                var childHeight = (float)orgSubDev.Size.Height.Value / umPerPixel_Y - GutterSize;

                                                var childCenX = (float)orgSubDev.Position.GetX() / umPerPixel_X + childWidth / 2f;
                                                var childCenY = (float)orgSubDev.Position.GetY() / umPerPixel_Y + childHeight / 2f;

                                                var childOffsetX = (float)orgSubDev.Position.GetX() / umPerPixel_X;
                                                var childOffsetY = (float)orgSubDev.Position.GetY() / umPerPixel_Y;

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
                                            SetCurrXIndex(DownXIdx + (int)(diffX / (RectWidth + GutterSize)));
                                            SetCurrYIndex(DownYIdx - (int)(diffY / (RectHeight + GutterSize)));

                                            //CursorXIndex = DownXIdx + (int)(diffX / (RectWidth + GutterSize));
                                            //CursorXIndex = DownYIdx - (int)(diffY / (RectHeight + GutterSize));
                                        }
                                    }
                                    else
                                    {
                                        SetCurrXIndex(DownXIdx + (int)(diffX / (RectWidth + GutterSize)));
                                        SetCurrYIndex(DownYIdx - (int)(diffY / (RectHeight + GutterSize)));
                                        //CursorXIndex = DownXIdx + (int)(diffX / (RectWidth + GutterSize));
                                        //CursorXIndex = DownYIdx - (int)(diffY / (RectHeight + GutterSize));
                                    }
                                }
                                else
                                {
                                    SetCurrXIndex(DownXIdx + (int)(diffX / (RectWidth + GutterSize)));
                                    SetCurrYIndex(DownYIdx - (int)(diffY / (RectHeight + GutterSize)));
                                    //CursorXIndex = DownXIdx + (int)(diffX / (RectWidth + GutterSize));
                                    //CursorYIndex = DownYIdx - (int)(diffY / (RectHeight + GutterSize));
                                }

                            }
                            else
                            {
                                SetCurrXIndex(DownXIdx + (int)(diffX / (RectWidth + GutterSize)));
                                SetCurrYIndex(DownYIdx - (int)(diffY / (RectHeight + GutterSize)));
                                //CursorXIndex = DownXIdx + (int)(diffX / (RectWidth + GutterSize));
                                //CursorYIndex = DownYIdx - (int)(diffY / (RectHeight + GutterSize));
                            }

                            //SelectedXIndex = CursorXIndex;
                            //SelectedYIndex = CursorYIndex;
                            IsMouseDownMove = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[MapViewControl] [MapViewControl_MouseMove()] : {err}");
            }
        }

        private void MapViewControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                downPos = e.GetPosition((System.Windows.IInputElement)sender);
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

                DownXIdx = SelectedXIndex;
                DownYIdx = SelectedYIndex;
                DownXSubIdx = CurrXSubIndex;
                DownYSubIdx = CurrYSubIndex;


                LoggerManager.Debug($"Selected die = [{SelectedXIndex}, {SelectedYIndex}]");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[MapViewControl] [MapViewControl_MouseDown()] : {err}");
            }
        }

        private void MapViewControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                IsMouseDown = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void MapViewControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
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

                if (OldZoomLevel != WaferObject.ZoomLevel)
                {
                    //IsChangedZoomLevel = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[MapViewControl] [MapViewControl_MouseWheel()] : {err}");
            }
        }

        private async void MapViewControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await MapViewMouseUpAcition();
        }

        private async Task MapViewMouseUpAcition()
        {
            //if (StageSyncEnalbe == false)
            //    return;
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

                if (IsMouseDownMove) // Probign 시 라는 상태 추가 되어야함.
                {

                }
                else
                {
                    CursorXIndex = SelectedXIndex;
                    CursorYIndex = SelectedYIndex;

                    //CurrXYIndex = new System.Windows.Point((int)CursorXIndex, (int)CursorYIndex);

                    //if (EnalbeClickToMove & StageSyncEnalbe & !isMinimapClicked)
                    //{
                    //    if (CurCamera != null)
                    //    {
                    //        CurCamera.IsMovingPos = true;
                    //        //await CurCamera.WaitCancelDialogService().ShowDialog("Wait", null, false);
                    //        //await CurCamera.WaitCancelDialogService().ShowDialog("Wait");
                    //        await CurCamera.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                    //        MachineIndex mindex = await CurCamera.SeletedMoveToPos(CursorXIndex, CursorYIndex, SelectedXIndex, SelectedYIndex);

                    //        if (mindex.XIndex != SelectedXIndex & mindex.YIndex != SelectedYIndex)
                    //        {
                    //            SelectedXIndex = CursorXIndex;
                    //            SelectedYIndex = CursorYIndex;
                    //        }
                    //        //await CurCamera.WaitCancelDialogService().CloseDialog();
                    //        await CurCamera.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                    //    }
                    //    else
                    //    {

                    //    }
                    //}

                    var mapchangeDelegate = WaferObject?.ChangeMapIndexDelegate;
                    if (mapchangeDelegate != null)
                    {
                        //await WaferObject.ChangeMapIndexDelegate(new Point( CurrXYIndex);
                        await WaferObject.ChangeMapIndexDelegate(new System.Windows.Point(CursorXIndex, CursorYIndex));
                    }
                }

                IsMouseDown = false;
                IsMouseDownMove = false;
                isMinimapClicked = false;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[MapViewControl] [MapViewControl_MouseUp()] : {err}");
            }
        }

        private void MapViewControl_SizeChanged(object sender, SizeChangedEventArgs e)
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
                throw;
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        public ResultMapViewControl()
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

                MakeTextFormats();

                TextFormatSet = new TextFormatSet();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MakeTextFormats()
        {
            try
            {
                int fontNum = 255;

                if (TextFormats == null)
                {
                    TextFormats = new List<TextFormat>();
                }
                else
                {
                    TextFormats.Clear();
                }

                for (float i = 1f; i <= fontNum; i++)
                {
                    TextFormat tmp = new TextFormat(fontFactory, "Segoe UI", i);
                    TextFormats.Add(tmp);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetCurrXIndex(int newValue)
        {
            try
            {
                var width = this.ActualWidth;

                float rectWidth = RectWidth;
                float guttWidth = GutterSize;

                //float overlapped = 1.3f;


                RawVector2 subDevOffset = new RawVector2(0, 0);

                double posx = (rectWidth + guttWidth) + (float)width / 2f - (rectWidth + guttWidth) / 2f - subDevOffset.X;
                int newVal = newValue;
                double leftoffset = posx;
                double rightoffset = ActualWidth - posx;
                int leftcount = (int)(leftoffset / (rectWidth + guttWidth));
                int rightcount = (int)(rightoffset / (rectWidth + guttWidth));
                int lefthidecount = -(newVal - leftcount) - 1;
                int righthidecount = (WaferObject.GetPhysInfo().MapCountX.Value - newVal) - rightcount - 1;


                if (leftcount + rightcount < WaferObject.GetPhysInfo().MapCountX.Value)
                {
                    if (lefthidecount >= 0 | righthidecount <= 0)
                    {
                        if (newVal < (int)CursorXIndex)
                        {
                            //Move Left
                            //왼쪽으로 숨겨진 Map 이있다면
                            if (lefthidecount < 0)
                            {
                                if (lefthidecount > ((int)CursorXIndex - newVal))
                                {
                                    if (righthidecount < 0)
                                        CurrXIndex = (int)newVal - Math.Abs(righthidecount);
                                    else
                                        CurrXIndex = (int)newVal;
                                }
                            }
                            else
                            {
                                CurrXIndex = newVal + Math.Abs(lefthidecount);
                            }
                        }
                        else
                        {
                            //Move Right
                            if (righthidecount > 0)
                            {
                                if (righthidecount > (newVal - (int)CursorXIndex))
                                {
                                    if (lefthidecount > 0)
                                    {
                                        CurrXIndex = (int)newVal + Math.Abs(lefthidecount);
                                    }
                                    else
                                        CurrXIndex = (int)newVal;
                                }
                            }
                            else
                            {
                                //if(righthidecount == -1)
                                CurrXIndex = (int)newVal - Math.Abs(righthidecount);
                            }
                        }
                    }
                    else
                    {
                        CurrXIndex = (int)newVal;
                    }
                }
                else
                {
                    if (CurrXIndex != (WaferObject.GetPhysInfo().MapCountX.Value / 2))
                        CurrXIndex = (WaferObject.GetPhysInfo().MapCountX.Value / 2);
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
                var height = ActualHeight;
                float rectHeight = RectHeight;
                float guttHeight = GutterSize;

                RawVector2 subDevOffset = new RawVector2(0, 0);
                double posy = (rectHeight + guttHeight) + (float)height / 2f - (rectHeight + guttHeight) / 2f - subDevOffset.X;
                int newVal = newValue;
                double upperoffset = posy;
                double bottomoffset = ActualHeight - posy;
                int uppercount = (int)(upperoffset / (rectHeight + guttHeight));
                int bottomcount = (int)(bottomoffset / (rectHeight + guttHeight));
                int upperhidecount = (WaferObject.GetPhysInfo().MapCountY.Value - newVal) - uppercount;
                int bottomhidecount = (newVal - bottomcount);

                if (uppercount + bottomcount < WaferObject.GetPhysInfo().MapCountY.Value)
                {
                    if (upperhidecount <= 0 | bottomhidecount <= 0)
                    {
                        if (newVal < CursorYIndex)
                        {
                            //Move bottom
                            //bottom으로 숨겨진 Map 이있다면
                            if (bottomhidecount > 0)
                            {
                                if (bottomhidecount > (CursorYIndex - newVal))
                                {
                                    if (upperhidecount < 0)
                                        CurrYIndex = newVal - Math.Abs(upperhidecount);
                                    else
                                        CurrYIndex = newVal;
                                }
                            }
                            else
                            {
                                CurrYIndex = newVal + Math.Abs(bottomhidecount);
                            }
                        }
                        else
                        {
                            //Move Upper
                            if (upperhidecount > 0)
                            {
                                if (upperhidecount > (newVal - CursorYIndex))
                                {
                                    if (bottomhidecount < 0)
                                    {
                                        CurrYIndex = newVal + Math.Abs(bottomhidecount);
                                    }
                                    else
                                        CurrYIndex = newVal;
                                }
                            }
                            else
                            {
                                //if(righthidecount == -1)
                                CurrYIndex = newVal - Math.Abs(upperhidecount);
                            }
                        }
                    }
                    else
                    {
                        CurrYIndex = newVal;
                    }

                }
                else
                {
                    if (CurrYIndex != (WaferObject.GetPhysInfo().MapCountY.Value / 2))
                        CurrYIndex = (WaferObject.GetPhysInfo().MapCountY.Value / 2);
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
                ResultMapViewControl mv = null;

                if (sender is ResultMapViewControl)
                {
                    mv = (ResultMapViewControl)sender;
                }
                else
                    return;

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

        private static void CursorXIndexPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                ResultMapViewControl mv = null;

                if (sender is ResultMapViewControl)
                {
                    mv = (ResultMapViewControl)sender;
                }
                else
                    return;

                if (sender != null && e.NewValue != null & mv.WaferObject != null)
                {
                    if (e.NewValue != e.OldValue)
                        mv._PreXIndex = (int)e.OldValue;

                    int leftBorder, rightBorder;
                    var width = mv.ActualWidth;

                    float rectWidth = mv.RectWidth;
                    float guttWidth = mv.GutterSize;

                    float overlapped = 1.3f;

                    leftBorder = mv.CurrXIndex - (int)Math.Ceiling(width / (rectWidth + guttWidth) * overlapped / 2);
                    rightBorder = mv.CurrXIndex + (int)Math.Ceiling(width / (rectWidth + guttWidth) * overlapped / 2);

                    RawVector2 subDevOffset = new RawVector2(0, 0);

                    int xidx = Convert.ToInt32(e.NewValue);

                    double posx = (xidx - mv.CurrXIndex) * (rectWidth + guttWidth) + (float)width / 2f - (rectWidth + guttWidth) / 2f - subDevOffset.X;

                    if (mv.WaferObject != null)
                    {
                        if (xidx < -1 | xidx > (int)mv.WaferObject.GetPhysInfo().MapCountX.Value)
                        {
                            //mv.CursorXIndex = (int)e.OldValue;
                            mv.SetCurrXIndex((int)e.OldValue);
                            //return;
                        }
                        else
                        {
                            mv.SetCurrXIndex((int)e.NewValue);
                        }

                    }
                }

                if (mv.IsMouseDown)
                    return;
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
                ResultMapViewControl mv = null;

                if (sender is ResultMapViewControl)
                {
                    mv = (ResultMapViewControl)sender;
                }
                else
                    return;

                if (sender != null && e.NewValue != null & mv.WaferObject != null)
                {
                    if (e.NewValue != e.OldValue)
                        mv._PreYIndex = (int)e.OldValue;

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
                        if (yidx < -1 | yidx > (int)mv.WaferObject.GetPhysInfo().MapCountY.Value)
                        {
                            //mv.CursorYIndex = (int)e.OldValue;
                            mv.SetCurrYIndex((int)e.OldValue);
                            //return;
                        }
                        else
                        {
                            mv.SetCurrYIndex((int)e.NewValue);
                        }
                    }

                }
                if (mv.IsMouseDown)
                    return;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private static void ASCIIMapPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                // TODO : NEED?
                ResultMapViewControl mv = null;

                if (sender is ResultMapViewControl)
                {
                    mv = (ResultMapViewControl)sender;
                }

                //if (sender != null && e.NewValue != null)
                //{
                //    mv.Wafer = (WaferObject)e.NewValue;
                //    mv.ZoomLevel = mv.Wafer.ZoomLevel;
                //    mv.WaferObject.ZoomLevel = mv.ZoomLevel;

                //    if (mv.ActualWidth < mv.WaferRadiusInPixel * 1.2)
                //    {
                //        mv.IsMiniMapVisible = true;
                //    }
                //    else
                //    {
                //        mv.IsMiniMapVisible = false;
                //    }
                //}
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
                ResultMapViewControl mv = null;

                if (sender is ResultMapViewControl)
                {
                    mv = (ResultMapViewControl)sender;
                }
                else
                    return;

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

        private void InitColorPallette()
        {
            try
            {
                resCache.Add("backgroundBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(105 / 255f, 105 / 255f, 105 / 255f, 255 / 255f)));
                resCache.Add("rectBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0 / 255f, 0 / 255f, 255 / 255f, 255 / 255f)));
                resCache.Add("passBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(50 / 255f, 205 / 255f, 50 / 255f, 255 / 255f)));
                resCache.Add("testdieBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(1f, 1f, 0f, 0.9f)));
                resCache.Add("testingBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(154 / 255f, 205 / 255f, 50 / 255f, 255 / 255f)));
                resCache.Add("notexistBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(150 / 255f, 150 / 255f, 150 / 255f, 255 / 255f)));
                resCache.Add("failBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(220 / 255f, 20 / 255f, 60 / 255f, 255 / 255f)));
                resCache.Add("edgeBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(105 / 255f, 105 / 255f, 105 / 255f, 0.8f)));
                // resCache.Add("markBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0 / 255f, 0 / 255f, 139 / 255f, 0.8f)));
                resCache.Add("markBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0f, 200 / 255f, 200 / 255f, 0.9f)));
                resCache.Add("DarkRedBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(139 / 255f, 0 / 255f, 0 / 255f, 1)));
                resCache.Add("changedtestkBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(100 / 100, 0 / 100, 0 / 255f, 1)));
                resCache.Add("modifymarkBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(138 / 255f, 43 / 255f, 226 / 255f, 1)));
                resCache.Add("skipBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(112 / 255f, 128 / 255f, 144 / 255f, 1)));
                resCache.Add("textBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0 / 255f, 0 / 255f, 0 / 255f, 1)));
                resCache.Add("miniMapBackGroundBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(176 / 255f, 196 / 255f, 222 / 255f, 1)));
                resCache.Add("miniMapForeGroundBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(245 / 255f, 245 / 255f, 245 / 255f, 1)));
                resCache.Add("selectedBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(245 / 255f, 245 / 255f, 245 / 255f, 0.5f)));
                resCache.Add("infoBackgroundBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(245 / 255f, 245 / 255f, 245 / 255f, 0.9f)));
                resCache.Add("CurBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0f, 0f, 0f, 0.9f)));
                resCache.Add("FirstDutBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(60 / 255f, 60 / 255f, 60 / 255f, 0.8f)));
                resCache.Add("lockBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(254, 190, 56, 1)));
                resCache.Add("brownBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(100 / 255f, 0f, 0f, 0.8f)));
                resCache.Add("DarkGrayBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(40 / 255f, 40 / 255f, 40 / 255f, 0.8f)));
                resCache.Add("OrangeBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 165 / 255f, 0 / 255f, 0.8f)));
                resCache.Add("DarkGreenBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(20 / 255f, 132 / 255f, 58 / 255f, 1f)));
                resCache.Add("VioletBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(179 / 255f, 35 / 255f, 167 / 255f, 1f)));
                resCache.Add("LightGreenBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(183 / 255f, 234 / 255f, 25 / 255f, 1f)));
                resCache.Add("GreenBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0f, 255f, 0f, 1f)));
                resCache.Add("RedBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255f, 0f, 0f, 1f)));

                resCache.Add("UnknwonBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(128 / 255f, 128 / 255f, 128 / 255f, 1f)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetTextFormatWidthMetrics(float width, float height)
        {
            try
            {
                float MetWidth = 0;
                float MetHeight = 0;

                TextFormat tf = null;

                List<string> tmpdata = new List<string>();

                tmpdata.Add("0");
                tmpdata.Add("00");
                tmpdata.Add("000");

                TextFormatSet.Formats.Clear();

                foreach (var item in tmpdata)
                {
                    foreach (var format in TextFormats)
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

                        TextFormatSet.Formats.Add(tmp);
                    }
                }

                TextFormatSet.LastZoomLevel = WaferObject.ZoomLevel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetVerticalFOV()
        {
            try
            {
                if (WaferObject != null)
                {

                    //if (ZoomLevel > MaxZoomLevel) ZoomLevel = MaxZoomLevel;
                    //if (ZoomLevel < MinZoomLevel) ZoomLevel = MinZoomLevel;
                    //RecSize = ((float)this.ActualWidth / (float)ZoomLevel) - GutterSize * 2;

                    if (WaferObject.ZoomLevel > MaxZoomLevel) WaferObject.ZoomLevel = MaxZoomLevel;
                    if (WaferObject.ZoomLevel < MinZoomLevel) WaferObject.ZoomLevel = MinZoomLevel;
                    RecSize = ((float)this.ActualWidth / (float)WaferObject.ZoomLevel) - GutterSize * 2;

                    if (WaferObject.ZoomLevel != _PreZoomLevel)
                    {
                        SetCurrXIndex(CursorXIndex);
                        SetCurrYIndex(CursorYIndex);
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
                LoggerManager.Debug($"[MapViewControl] [SetVerticalFOV()] : {err}");
            }
        }

        public override void Render(RenderTarget target)
        {
            try
            {
                bool CanRender = false;

                if (WaferObject != null & Wafer?.WaferDevObjectRef != null & Wafer?.GetPhysInfo().DieSizeY != null &&
                    Wafer.GetSubsInfo().ActualDieSize.Width.Value > 0 &&
                    Wafer.GetSubsInfo().ActualDieSize.Height.Value > 0)
                {
                    CanRender = true;
                }
                else
                {
                    CanRender = false;
                }

                if (CanRender == false)
                {
                    return;
                }

                SetVerticalFOV();

                target.Clear(new RawColor4(105 / 255f, 105 / 255f, 105 / 255f, 1));

                InitColorPallette();

                try
                {
                    double maxIndexXInView, maxIndexYInView;

                    double waferRadius;
                    double aspectRatio;

                    bool drawMultDev = false;

                    float posx, posy = 0;
                    float waferAngle = 0f;
                    float stepX = 0, stepY = 0;
                    float rectWidth = RecSize;
                    float rectHeight = RecSize;
                    float guttWidth = GutterSize;
                    float guttHeight = GutterSize;

                    int cenIndexX = 0, cenIndexY = 0;
                    int indexOffsetX = 0, indexOffsetY = 0;
                    float waferRadiusInPixel = 0f;
                    float refDieOffsetXInPixel = 0f;
                    float refDieOffsetYInPixel = 0f;

                    var width = this.ActualWidth;
                    var height = this.ActualHeight;

                    float size;
                    float notchLength;
                    float notchRadiusInPixel;
                    float flatZoneAngle = 0f;

                    long inFOVDieCount = 0;

                    ArcSegment arc1 = new ArcSegment();
                    ArcSegment arc2 = new ArcSegment();
                    ArcSegment arc3 = new ArcSegment();
                    Size2 miniMapSize = new Size2(120, 120);

                    Rectangle notchrectangle = new Rectangle();
                    Ellipse notchellipse = new Ellipse();

                    cenIndexX = (int)Wafer.GetPhysInfo().CenM.XIndex.Value;
                    cenIndexY = (int)Wafer.GetPhysInfo().CenM.YIndex.Value;

                    waferRadius = Wafer.GetPhysInfo().WaferSize_um.Value;
                    waferAngle = (float)Wafer.GetPhysInfo().NotchAngle.Value;

                    if (waferAngle > 360)
                    {
                        waferAngle = waferAngle - 360;
                    }

                    var rotateangle = 360 - waferAngle - 90;
                    waferAngle = rotateangle;

                    stepX = (float)Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                    stepY = (float)Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                    aspectRatio = stepX / stepY;

                    rectWidth = (float)((rectHeight + guttWidth) * aspectRatio) - guttHeight;

                    RectWidth = rectWidth;
                    RectHeight = rectHeight;

                    #region // Wafer outline drawing
                    indexOffsetX = (cenIndexX - CurrXIndex);
                    indexOffsetY = (CurrYIndex - cenIndexY);
                    waferRadiusInPixel = (float)(waferRadius / ((height / (rectHeight + guttHeight) * stepY)) * height);
                    WaferRadiusInPixel = waferRadiusInPixel;

                    umPerPixel_X = (float)Wafer.GetPhysInfo().DieSizeX.Value / rectWidth;
                    umPerPixel_Y = (float)Wafer.GetPhysInfo().DieSizeY.Value / rectHeight;

                    refDieOffsetXInPixel = (float)(Wafer.GetPhysInfo().LowLeftCorner.GetX() / umPerPixel_X);
                    refDieOffsetYInPixel = (float)(Wafer.GetPhysInfo().LowLeftCorner.GetY() / umPerPixel_Y);

                    double dieWidthPixel = (float)(Wafer.GetSubsInfo().ActualDieSize.Width.Value / umPerPixel_X);
                    double dieHeightPixel = (float)(Wafer.GetSubsInfo().ActualDieSize.Height.Value / umPerPixel_Y);

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
                                subDevOffset.X = +(float)subDev.Position.GetX() / umPerPixel_X + (float)subDev.Size.Width.Value / 2f / umPerPixel_X - (float)(rectWidth) / 2.0f;
                                subDevOffset.Y = -(float)subDev.Position.GetY() / umPerPixel_Y - (float)subDev.Size.Height.Value / 2f / umPerPixel_Y + (float)(rectHeight) / 2.0f;

                                waferCenPos.X = (float)(indexOffsetX * (rectWidth + guttHeight) + width / 2) + (float)(refDieOffsetXInPixel) - subDevOffset.X;
                                waferCenPos.Y = (float)(indexOffsetY * (rectHeight + guttHeight) + height / 2) - (float)(refDieOffsetYInPixel) - subDevOffset.Y;
                            }
                            else
                            {
                                waferCenPos.X = (float)(indexOffsetX * (rectWidth + guttHeight) - (rectWidth) / 2.0f + width / 2)
                                    + (float)(refDieOffsetXInPixel);
                                waferCenPos.Y = (float)(indexOffsetY * (rectHeight + guttHeight) - (rectHeight) / 2.0f + height / 2)
                                    - (float)(refDieOffsetYInPixel);
                            }
                        }
                        else
                        {
                            waferCenPos.X = (float)(indexOffsetX * (rectWidth + guttHeight) - (rectWidth) / 2.0f + width / 2)
                                + (float)(refDieOffsetXInPixel);
                            waferCenPos.Y = (float)(indexOffsetY * (rectHeight + guttHeight) - (rectHeight) / 2.0f + height / 2)
                                - (float)(refDieOffsetYInPixel);
                        }
                    }
                    else
                    {
                        waferCenPos.X = (float)(indexOffsetX * (rectWidth + guttWidth) + width / 2.0) + (float)(cenOffsetXPixel);
                        waferCenPos.Y = (float)(indexOffsetY * (rectHeight + guttHeight) + height / 2.0) - (float)(cenOffsetYPixel);
                    }

                    geometryStartPosition.X = waferCenPos.X;
                    geometryStartPosition.Y = waferCenPos.Y - waferRadiusInPixel / 2;

                    size = waferRadiusInPixel / 2;

                    notchLength = 5000;
                    notchRadiusInPixel = (float)(notchLength / ((height / (rectHeight + guttHeight)) * stepY) * height);

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

                        //notchellipse.Point.X = geometryStartPosition.X;
                        //notchellipse.Point.Y = arc1.Point.Y + (notchRadiusInPixel * 2);
                        //notchellipse.RadiusX = notchRadiusInPixel / (float)3;
                        //notchellipse.RadiusY = notchRadiusInPixel / (float)3;

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

                                //float cos = (float)Math.Cos(waferAngle * Math.PI / 180.0);
                                //float sin = (float)Math.Sin(waferAngle * Math.PI / 180.0);

                                //var matrix = new RawMatrix3x2()
                                //{
                                //    M11 = cos,
                                //    M12 = sin,
                                //    M21 = -sin,
                                //    M22 = cos,
                                //    //M31 = pointVector.X - size / 2f,
                                //    //M32 = pointVector.Y - size / 2f
                                //    M31 = 0,
                                //    M32 = 0
                                //};
                                var matrix = Transformation.Rotation(waferAngle, pointVector);
                                tfGeometry = new TransformedGeometry(target.Factory, geometry, matrix);

                                RawVector2 notpoint = new RawVector2(notchrectangle.X, notchrectangle.Y);
                                var notchpoint = Transformation.Rotation(notpoint, waferCenPos, waferAngle);

                                notchrectangle.X = (int)notchpoint.X;
                                notchrectangle.Y = (int)notchpoint.Y;
                                //tfGeometry =
                                //    new TransformedGeometry(
                                //        target.Factory, geometry,
                                //        Matrix3x2.Rotation((float)(waferAngle * Math.PI / 180.0),
                                //        pointVector));
                                target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                                target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);
                                target.FillRectangle(new RawRectangleF(
                                    notchrectangle.X, notchrectangle.Y, notchrectangle.X + notchrectangle.Width, notchrectangle.Y + notchrectangle.Height),
                                    resCache["DarkRedBrush"] as Brush);

                                tfGeometry.Dispose();
                            }
                        }

                    }
                    else
                    {
                        notchLength = 5000;
                        notchRadiusInPixel = (float)(notchLength / ((height / (rectHeight + guttHeight)) * stepY) * height);
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
                                tfGeometry =
                                    new TransformedGeometry(target.Factory, geometry, matrix);
                                var notchpoint = Transformation.Rotation(notchellipse.Point, waferCenPos, waferAngle);

                                //notchellipse.Point = notchpoint;

                                target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                                target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);
                                //target.FillEllipse(notchellipse, resCache["DarkGreenBrush"] as Brush);
                                //target.DrawGeometry(geometry, resCache["DarkGreenBrush"] as Brush, 5);
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
                                //geoSink.BeginFigure(new RawVector2(notchellipse.Point.X, notchellipse.Point.Y), new FigureBegin());
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
                                //geoSink.AddLine(Transformation.Rotation(new RawVector2(notchstartpoint.Y - 14, notchstartpoint.X + 14), waferCenPos, waferAngle));
                                //geoSink.AddLine(Transformation.Rotation(new RawVector2(notchstartpoint.Y - 14, notchstartpoint.X - 14), waferCenPos, waferAngle));


                                geoSink.EndFigure(new FigureEnd());
                                geoSink.Close();

                                target.DrawGeometry(geometry, resCache["LightGreenBrush"] as Brush);
                                target.FillGeometry(geometry, resCache["LightGreenBrush"] as Brush);
                                tfGeometry.Dispose();
                            }
                        }


                    }

                    #endregion

                    long devCnt;
                    //rawDevices = WaferObject.GetDevices();

                    int leftBorder, rightBorder, topBorder, botBorder;
                    float overlapped = 1.3f;
                    //double dutoffsetx = 0, dutoffsety = 0;

                    leftBorder = CurrXIndex - (int)Math.Ceiling(width / (rectWidth + guttWidth) * overlapped / 2);
                    rightBorder = CurrXIndex + (int)Math.Ceiling(width / (rectWidth + guttWidth) * overlapped / 2);
                    topBorder = CurrYIndex - (int)Math.Ceiling(height / (rectHeight + guttHeight) * overlapped / 2);
                    botBorder = CurrYIndex + (int)Math.Ceiling(height / (rectHeight + guttHeight) * overlapped / 2);

                    //Draw Wafer Device
                    long validDieCount = 0;

                    IDeviceObject[,] DIEs = null;

                    if (Wafer.GetSubsInfo().DIEs != null)
                    {
                        DIEs = Wafer.GetSubsInfo().DIEs.Clone() as IDeviceObject[,];

                        int DieXCount = (DIEs.GetUpperBound(0) + 1);
                        int DieYCount = (DIEs.GetUpperBound(1) + 1);

                        devCnt = DieXCount * DieYCount;

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
                                //int count = 0;

                                for (int x = leftBorder; x < rightBorder; x++)
                                {
                                    if (y < DieYCount & x < DieXCount)
                                    {
                                        if (DIEs[x, y] != null)
                                        {
                                            //if (DIEs[x, y].DieType.Value != DieTypeEnum.NOT_EXIST)
                                            //{
                                            inFOVDieCount++;

                                            xidx = (int)DIEs[x, y].DieIndexM.XIndex;
                                            yidx = (int)DIEs[x, y].DieIndexM.YIndex;

                                            posx = (xidx - CurrXIndex) * (rectWidth + guttWidth) + (float)width / 2f - (rectWidth + guttWidth) / 2f - subDevOffset.X;
                                            posy = (CurrYIndex - yidx) * (rectHeight + guttHeight) + (float)height / 2f - (rectHeight + guttHeight) / 2f - subDevOffset.Y;

                                            if (xidx == CurrXIndex & yidx == CurrYIndex)
                                            {
                                                CurrBinCode = DIEs[x, y].CurTestHistory.BinCode.Value;
                                                PointedDie = DIEs[x, y];
                                            }

                                            if (posx < -(rectWidth + guttWidth) | posx > (width + rectWidth + guttWidth) |
                                                posy < -(rectHeight + guttHeight) | posy > (height + rectHeight + guttHeight))
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
                                                validDieCount = DrawDevice(target, x, y, DIEs[x, y], rectWidth, rectHeight, posx, posy, validDieCount);

                                                //target.DrawRectangle(
                                                //    new RawRectangleF(
                                                //posx + guttWidth / 2.0f,
                                                //posy + guttHeight / 2.0f,
                                                //posx + guttWidth / 2.0f + rectWidth - guttWidth * 1.0f,
                                                //posy + guttHeight / 2.0f + rectHeight - guttHeight * 1.0f),
                                                //resCache["CurBrush"] as Brush,
                                                //guttWidth * 0.5f);

                                                //// Current Die
                                                //if (xidx == CursorXIndex & yidx == CursorYIndex)
                                                //{
                                                //    if (Wafer.MapViewCurIndexVisiablity)
                                                //    {
                                                //        if (UnderDutDices != null)
                                                //        {
                                                //            if (!(UnderDutDices.Count != 0 & IsDutVisible))
                                                //            {
                                                //                target.DrawRectangle(
                                                //                   new RawRectangleF(
                                                //                                posx,
                                                //                                posy,
                                                //                                posx + rectWidth,
                                                //                                posy + rectHeight),
                                                //                                resCache["CurBrush"] as Brush,
                                                //                                guttWidth * 1.5f);
                                                //            }
                                                //        }
                                                //        else
                                                //        {
                                                //            target.DrawRectangle(
                                                //                   new RawRectangleF(
                                                //                                posx,
                                                //                                posy,
                                                //                                posx + rectWidth,
                                                //                                posy + rectHeight),
                                                //                                resCache["CurBrush"] as Brush,
                                                //                                guttWidth * 1.5f);
                                                //        }
                                                //    }
                                                //}

                                                //if (IsMouseDown & xidx == SelectedXIndex & yidx == SelectedYIndex & !IsMouseDownMove)
                                                //{
                                                //    target.FillRectangle(new RawRectangleF(
                                                //        posx,
                                                //        posy,
                                                //        posx + rectWidth,
                                                //        posy + rectHeight),
                                                //        resCache["FirstDutBrush"] as Brush);
                                                //}

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

                                                            childWidth = (float)childs[subIndex].Size.Width.Value / umPerPixel_X - guttWidth;
                                                            childHeight = (float)childs[subIndex].Size.Height.Value / umPerPixel_Y - guttHeight;
                                                            if (childWidth < 10 | childHeight < 10)
                                                            {
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                childOffsetX = (float)childs[subIndex].Position.GetX() / umPerPixel_X + guttWidth / 2f;
                                                                childOffsetY = rectHeight - ((float)childs[subIndex].Position.GetY() / umPerPixel_Y + guttHeight / 2f) - childHeight;

                                                                if (childs[subIndex].Position.GetT() != 0)
                                                                {

                                                                    //float cos = (float)Math.Cos(childs[subIndex].Position.GetT() * Math.PI / 180.0);
                                                                    //float sin = (float)Math.Sin(childs[subIndex].Position.GetT() * Math.PI / 180.0);
                                                                    //var matrix = new RawMatrix3x2()
                                                                    //{
                                                                    //    M11 = cos,
                                                                    //    M12 = sin,
                                                                    //    M21 = -sin,
                                                                    //    M22 = cos,
                                                                    //    //M31 = posx + childOffsetX,
                                                                    //    //M32 = posy + childOffsetY
                                                                    //    M31 = childWidth / 2f,
                                                                    //    M32 = childHeight / 2f
                                                                    //};
                                                                    var matrix = Transformation.Rotation((float)childs[subIndex].Position.GetT(),
                                                                        new RawVector2(posx + childOffsetX + childWidth / 2f, posy + childOffsetY + childHeight / 2f));
                                                                    target.Transform = matrix;
                                                                    //target.Transform = Matrix3x2.Rotation((float)childs[subIndex].Position.GetT(),
                                                                    //                new SharpDX.Mathematics.Interop.RawVector2(posx + childOffsetX, posy + childOffsetY));
                                                                    validDieCount = DrawDevice(
                                                                    target, x, y, childs[subIndex],
                                                                    childWidth, childHeight,
                                                                    posx + childOffsetX, posy + childOffsetY,
                                                                    validDieCount);
                                                                    target.Transform = currTransform;
                                                                }
                                                                else
                                                                {
                                                                    validDieCount = DrawDevice(
                                                                                        target, x, y, childs[subIndex],
                                                                                        childWidth, childHeight,
                                                                                        posx + childOffsetX, posy + childOffsetY,
                                                                                        validDieCount);
                                                                }

                                                            }
                                                        }
                                                    }
                                                }
                                                #endregion

                                                //IsDutVisible = true;

                                                //if (IsDutVisible == true)
                                                //{

                                                //}
                                                //else
                                                //{
                                                //    if (SelectedDie != null & !IsMouseDownMove)
                                                //    {
                                                //        if (SelectedDie.Count > 0)
                                                //        {
                                                //            if (SelectedDie.ToList().Find(d => (d.DieIndexM.XIndex == xidx) & (d.DieIndexM.YIndex == yidx)) != null)
                                                //            {
                                                //                target.DrawRectangle(
                                                //                    new RawRectangleF(
                                                //                        posx + guttWidth / 2.0f,
                                                //                        posy + guttHeight / 2.0f,
                                                //                        posx + guttWidth / 2.0f + rectWidth - guttWidth * 1.0f,
                                                //                        posy + guttHeight / 2.0f + rectHeight - guttHeight * 1.0f),
                                                //                    resCache["edgeBrush"] as Brush,
                                                //                    guttWidth * 1.0f);
                                                //                target.FillRectangle(
                                                //                    new RawRectangleF(
                                                //                        posx, posy,
                                                //                        posx + rectWidth, posy + rectHeight),
                                                //                    resCache["selectedBrush"] as Brush);
                                                //            }
                                                //        }
                                                //    }
                                                //}

                                                if (HighlightDies != null && HighlightDies.Count > 0)
                                                {
                                                    HighlightDieComponent tmphighlightdie = null;

                                                    tmphighlightdie = HighlightDies.ToList().Find(d => (d.MI.XIndex == xidx) & (d.MI.YIndex == yidx));

                                                    if (tmphighlightdie != null)
                                                    {
                                                        target.DrawRectangle(
                                                            new RawRectangleF(
                                                                posx + guttWidth / 2.0f,
                                                                posy + guttHeight / 2.0f,
                                                                posx + guttWidth / 2.0f + rectWidth - guttWidth * 1.0f,
                                                                posy + guttHeight / 2.0f + rectHeight - guttHeight * 1.0f),
                                                                resCache[tmphighlightdie.BrushAlias] as Brush, guttWidth * 1.0f);

                                                        //target.FillRectangle(
                                                        //    new RawRectangleF(
                                                        //        posx, posy,
                                                        //        posx + rectWidth, posy + rectHeight),
                                                        //    resCache["selectedBrush"] as Brush);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //    }
                                    //    else
                                    //    {

                                    //    }
                                    //}
                                }
                            }
                            //target.DrawText(string.Format("Dev. Cnt.: {0}, Tested: {1}", validDieCount, Wafer.TestedCount), textFormat, new RectangleF(10, 24, 200.0f, 36.0f), textBrush);
                        }
                    }

                    //miniMapStartPosition = new SharpDX.Mathematics.Interop.RawVector2();
                    miniMapStartPosition.X = (float)width - 135;
                    miniMapStartPosition.Y = 0 + 15;

                    if (IsMiniMapVisible & width > 256 & height > 256)
                    {
                        target.FillRectangle(
                            new RawRectangleF(
                                miniMapStartPosition.X, miniMapStartPosition.Y,
                                miniMapStartPosition.X + miniMapSize.Width, miniMapStartPosition.Y + miniMapSize.Height),
                            resCache["miniMapBackGroundBrush"] as Brush);

                        geometryStartPosition.X = miniMapStartPosition.X + miniMapSize.Width / 2f;
                        geometryStartPosition.Y = miniMapStartPosition.Y + 10f;
                        size = 48.5f;

                        notchLength = (float)waferRadius / 10.0f;
                        notchRadiusInPixel = 6;
                        if (Wafer.GetPhysInfo().NotchType.Value == WaferNotchTypeEnum.FLAT.ToString())
                        {
                            notchLength = 20;
                            size = 50;
                            flatZoneAngle = (float)Math.Acos(notchLength / (size * 2)) + (float)Math.PI;
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
                                    geoSink.BeginFigure(
                                        new SharpDX.Mathematics.Interop.RawVector2(geometryStartPosition.X, geometryStartPosition.Y),
                                        new FigureBegin());
                                    geoSink.AddArc(arc1);
                                    geoSink.AddLine(new SharpDX.Mathematics.Interop.RawVector2(geometryStartPosition.X + (size * (float)Math.Cos(flatZoneAngle) * -1), arc1.Point.Y));
                                    geoSink.AddArc(arc3);
                                    geoSink.EndFigure(new FigureEnd());
                                    geoSink.Close();

                                    //float cos = (float)Math.Cos(waferAngle * Math.PI / 180.0);
                                    //float sin = (float)Math.Sin(waferAngle * Math.PI / 180.0);
                                    //var matrix = new RawMatrix3x2()
                                    //{
                                    //    M11 = cos,
                                    //    M12 = sin,
                                    //    M21 = -sin,
                                    //    M22 = cos,
                                    //    //M31 = miniMapStartPosition.X + 60 - size / 2f,
                                    //    //M32 = miniMapStartPosition.Y + 60 - size / 2f
                                    //    M31 = 0,
                                    //    M32 = 0
                                    //};
                                    var matrix = Transformation.Rotation(waferAngle,
                                        new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + miniMapSize.Height / 2f));
                                    tfGeometry =
                                        new TransformedGeometry(target.Factory, geometry, matrix);

                                    RawVector2 notpoint = new RawVector2(notchrectangle.X, notchrectangle.Y);
                                    var notchpoint = Transformation.Rotation(notpoint, new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + miniMapSize.Height / 2f), waferAngle);

                                    notchrectangle.X = (int)notchpoint.X;
                                    notchrectangle.Y = (int)notchpoint.Y;
                                    //tfGeometry = new TransformedGeometry(
                                    //        target.Factory, geometry,
                                    //        Matrix3x2.Rotation((float)(waferAngle * Math.PI / 180.0),
                                    //        new SharpDX.Mathematics.Interop.RawVector2(miniMapStartPosition.X + 60, miniMapStartPosition.Y + 60)));
                                    target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                                    target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);
                                    target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                                    target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);
                                    target.FillRectangle(new RawRectangleF(
                                           notchrectangle.X, notchrectangle.Y, notchrectangle.X + notchrectangle.Width, notchrectangle.Y + notchrectangle.Height),
                                           resCache["DarkRedBrush"] as Brush);
                                    tfGeometry.Dispose();
                                }
                            }
                        }
                        else
                        {
                            notchLength = (float)waferRadius / 10.0f;
                            notchRadiusInPixel = 14;
                            flatZoneAngle = (float)Math.Acos(notchLength / waferRadius) + (float)Math.PI;

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
                                    geoSink.BeginFigure(
                                        new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + 10f),
                                        new FigureBegin());
                                    geoSink.AddArc(arc1);
                                    geoSink.AddArc(arc2);
                                    geoSink.AddArc(arc3);
                                    geoSink.EndFigure(new FigureEnd());
                                    geoSink.Close();

                                    //float cos = (float)Math.Cos(waferAngle * Math.PI / 180.0);
                                    //float sin = (float)Math.Sin(waferAngle * Math.PI / 180.0);
                                    //var matrix = new RawMatrix3x2()
                                    //{
                                    //    M11 = cos,
                                    //    M12 = sin,
                                    //    M21 = -sin,
                                    //    M22 = cos,
                                    //    //M31 = miniMapStartPosition.X + 60 - size / 2f,
                                    //    //M32 = miniMapStartPosition.Y + 60 - size / 2f
                                    //    M31 = 0f,
                                    //    M32 = 0f
                                    //};
                                    var matrix = Transformation.Rotation(waferAngle,
                                        new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + miniMapSize.Height / 2f));
                                    tfGeometry =
                                        new TransformedGeometry(target.Factory, geometry, matrix);
                                    //tfGeometry =
                                    //    new TransformedGeometry(
                                    //        target.Factory, geometry,
                                    //        Matrix3x2.Rotation((float)(waferAngle * Math.PI / 180.0),
                                    //        new SharpDX.Mathematics.Interop.RawVector2(miniMapStartPosition.X + 60, miniMapStartPosition.Y + 60)));

                                    var notchpoint = Transformation.Rotation(notchellipse.Point, new RawVector2(miniMapStartPosition.X + miniMapSize.Width / 2f, miniMapStartPosition.Y + miniMapSize.Height / 2f), waferAngle);
                                    notchellipse.Point = notchpoint;

                                    target.DrawGeometry(tfGeometry, resCache["edgeBrush"] as Brush);
                                    target.FillGeometry(tfGeometry, resCache["miniMapForeGroundBrush"] as Brush);
                                    target.FillEllipse(notchellipse, resCache["DarkRedBrush"] as Brush);
                                    tfGeometry.Dispose();
                                }
                            }
                        }

                        if (IsMiniMapVisible == true & WaferObject != null)
                        {

                            SharpDX.Mathematics.Interop.RawVector2 fovBoxSize = new SharpDX.Mathematics.Interop.RawVector2();
                            SharpDX.Mathematics.Interop.RawVector2 fovBoxCen = new SharpDX.Mathematics.Interop.RawVector2();
                            indexOffsetX = (cenIndexX - CurrXIndex);
                            indexOffsetY = (cenIndexY - CurrYIndex);
                            if (stepX <= 0) stepX = 1000;
                            if (stepY <= 0) stepY = 1000;

                            maxIndexXInView = (waferRadius) / stepX;
                            maxIndexYInView = (waferRadius) / stepY;

                            fovBoxSize.X = (float)((Math.Round((width / (rectWidth + guttWidth))) * stepX) / waferRadius * (miniMapSize.Width - 20));
                            if (fovBoxSize.X > miniMapSize.Width)
                            {
                                fovBoxSize.X = miniMapSize.Width;
                            }
                            fovBoxSize.Y = (float)((Math.Round((height / (rectHeight + guttHeight))) * stepY) / waferRadius * (miniMapSize.Height - 20));
                            if (fovBoxSize.Y > miniMapSize.Height)
                            {
                                fovBoxSize.Y = miniMapSize.Height;
                            }

                            bool xOutOfFov = false, yOutOfFov = false;
                            fovBoxCen.X = (float)(indexOffsetX / Math.Round(maxIndexXInView) * 100.0f)
                                * -1.0f + miniMapStartPosition.X + miniMapSize.Width / 2f;

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
                            fovBoxCen.Y = (float)(indexOffsetY / Math.Round(maxIndexYInView) * 100.0f)
                                * 1.0f + miniMapStartPosition.Y + miniMapSize.Height / 2f;
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

                            if (xOutOfFov | yOutOfFov)
                            {
                                target.DrawRectangle(
                                    new RawRectangleF(
                                        fovBoxCen.X - fovBoxSize.X / 2f,
                                        fovBoxCen.Y - fovBoxSize.Y / 2f,
                                        fovBoxSize.X + fovBoxCen.X - fovBoxSize.X / 2f,
                                        fovBoxSize.Y + fovBoxCen.Y - fovBoxSize.Y / 2f),
                                    resCache["failBrush"] as Brush);
                            }
                            else
                            {
                                target.DrawRectangle(
                                    new RawRectangleF(
                                        fovBoxCen.X - fovBoxSize.X / 2f,
                                        fovBoxCen.Y - fovBoxSize.Y / 2f,
                                        fovBoxCen.X - fovBoxSize.X / 2f + fovBoxSize.X,
                                        fovBoxCen.Y - fovBoxSize.Y / 2f + fovBoxSize.Y),
                                    resCache["edgeBrush"] as Brush);
                            }
                            target.DrawLine(
                                new SharpDX.Mathematics.Interop.RawVector2((float)(fovBoxCen.X - 3),
                                (float)(fovBoxCen.Y)),
                                new SharpDX.Mathematics.Interop.RawVector2((float)(fovBoxCen.X + 3),
                                (float)(fovBoxCen.Y)), resCache["textBrush"] as Brush, 1f);
                            target.DrawLine(
                                new SharpDX.Mathematics.Interop.RawVector2((float)(fovBoxCen.X),
                                (float)(fovBoxCen.Y - 3)),
                                new SharpDX.Mathematics.Interop.RawVector2((float)(fovBoxCen.X),
                                (float)(fovBoxCen.Y + 3)), resCache["textBrush"] as Brush, 1f);
                        }
                    }

                    if (IsCrossLineVisible)
                    {
                        posx = (CursorXIndex - CurrXIndex) * (rectWidth + guttWidth) + (float)width / 2f - (rectWidth + guttWidth) / 2f - subDevOffset.X;
                        posy = (CurrYIndex - CursorYIndex) * (rectHeight + guttHeight) + (float)height / 2f - (rectHeight + guttHeight) / 2f - subDevOffset.Y;

                        target.DrawLine(
                       new SharpDX.Mathematics.Interop.RawVector2((float)(0), (float)((posy + rectHeight / 2)) - 1),
                       new SharpDX.Mathematics.Interop.RawVector2((float)(width), (float)((posy + rectHeight / 2)) - 1), resCache["textBrush"] as Brush, 1f);
                        target.DrawLine(
                            new SharpDX.Mathematics.Interop.RawVector2((float)(posx + rectWidth / 2 - 1), (float)(0)),
                            new SharpDX.Mathematics.Interop.RawVector2((float)(posx + rectWidth / 2 - 1), (float)(height)), resCache["textBrush"] as Brush, 1f);

                        // target.DrawLine(
                        //new SharpDX.Mathematics.Interop.RawVector2((float)(0), (float)(height / 2) - 1),
                        //new SharpDX.Mathematics.Interop.RawVector2((float)(width), (float)(height / 2) - 1), resCache["textBrush"] as Brush, 1f);
                        // target.DrawLine(
                        //     new SharpDX.Mathematics.Interop.RawVector2((float)(width / 2 - 1), (float)(0)),
                        //     new SharpDX.Mathematics.Interop.RawVector2((float)(width / 2 - 1), (float)(height)), resCache["textBrush"] as Brush, 1f);
                    }

                    if (Wafer.MapViewCurIndexVisiablity)
                    {
                        Brush brush = resCache["infoBackgroundBrush"] as Brush;
                        brush.Opacity = (float)0.5;
                        target.FillRectangle(new RawRectangleF(8, 8, 150.0f, 36), brush);

                        if (CoordinateManager != null)
                        {
                            UserIndex index = CoordinateManager.WMIndexConvertWUIndex(CursorXIndex, CursorYIndex);
                            target.DrawText(string.Format("UI:{0,4:+#;-#;0},{1,4:+#;-#;0}", index.XIndex, index.YIndex),
                                uiTextFormat, new RawRectangleF(12, 8, 150.0f, 36.0f), resCache["textBrush"] as Brush);
                            //target.DrawText(string.Format("MI:{0,4:+#;-#;0},{1,4:+#;-#;0}", CursorXIndex, CursorYIndex),
                            //    uiTextFormat, new RawRectangleF(10, 44, 150.0f, 36.0f), resCache["textBrush"] as Brush);
                        }
                        else
                        {
                            target.DrawText(string.Format("MI:{0,4:+#;-#;0}, {1,4:+#;-#;0}", CursorXIndex, CursorYIndex),
                                uiTextFormat, new RawRectangleF(12, 8, 150.0f, 36.0f), resCache["textBrush"] as Brush);
                        }
                    }

                    //if(MapViewEncoderVisiability)
                    //{
                    //    if (AxisXPos == null | AxisYPos == null | AxisZPos == null | AxisTPos == null)
                    //    {
                    //        AxisXPos = Wafer.MotionManager().GetAxis(EnumAxisConstants.X);
                    //        AxisYPos = Wafer.MotionManager().GetAxis(EnumAxisConstants.Y);
                    //        AxisZPos = Wafer.MotionManager().GetAxis(EnumAxisConstants.Z);
                    //        AxisTPos = Wafer.MotionManager().GetAxis(EnumAxisConstants.TRI);
                    //    }

                    //    target.DrawText($"[Encoder] X:{AxisXPos.Status.Position.Ref}, Y{AxisYPos.Status.Position.Ref}, Z:{ AxisZPos.Status.Position.Ref}, T:{AxisTPos.Status.Position.Ref}",
                    //          uiTextFormat, new RawRectangleF(12, (float)(this.ActualHeight - uiTextFormat.FontSize) - 4, (float)this.ActualWidth, (float)this.ActualHeight), resCache["textBrush"] as Brush);
                    //}

                    target.AntialiasMode = AntialiasMode.Aliased;
                }
                catch (Exception err)
                {
                    // TODO : Check
                    LoggerManager.Error($"{err}");
                    //LoggerManager.Error($err, string.Format("Render(): Error occurred. Err = {0}, Stack trace = {1}", err.Message, err.StackTrace));
                    //LoggerManager.Exception(err);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private long DrawDevice(RenderTarget target, int xindex, int yindex, IDeviceObject dev, float rectWidth, float rectHeight, float posx, float posy, long validDieCount)
        {
            try
            {
                switch (dev.State.Value)
                {
                    case DieStateEnum.UNKNOWN:
                        break;
                    case DieStateEnum.NOT_EXIST:
                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["notexistBrush"] as Brush);
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
                                if (children.Count > 0)
                                {
                                    target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["skipBrush"] as Brush);
                                    validDieCount++;
                                }
                                else
                                {
                                    if(dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_PASS)
                                    {
                                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["passBrush"] as Brush);
                                    }
                                    else if(dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_FAIL)
                                    {
                                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["failBrush"] as Brush);
                                    }
                                    else if (dev.CurTestHistory.TestResult.Value == TestState.MAP_STS_SKIP)
                                    {
                                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["skipBrush"] as Brush);
                                    }
                                    else
                                    {
                                        // TODO : 
                                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["UnknwonBrush"] as Brush);
                                    }
                                    
                                    validDieCount++;
                                }
                            }
                        }

                        validDieCount++;

                        break;
                    case DieStateEnum.MARK:
                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["markBrush"] as Brush);
                        //validDieCount++;
                        break;
                    case DieStateEnum.SKIPPED:
                        target.FillRectangle(new RawRectangleF(posx, posy, posx + rectWidth, posy + rectHeight), resCache["skipBrush"] as Brush);
                        validDieCount++;
                        break;
                    default:
                        break;
                }

                // TEST CODE
                if (ASCIIMap != null)
                {
                    if (DisplayBinCode == true)
                    {
                        string bincode = dev.CurTestHistory.BinCode.Value.ToString();

                        // 데이터 미생성 또는 ZoomLevel 변경 시
                        if (TextFormatSet.Formats == null || TextFormatSet.Formats.Count == 0 ||
                            TextFormatSet.LastZoomLevel != WaferObject.ZoomLevel)
                        {
                            SetTextFormatWidthMetrics(rectWidth, rectHeight);
                        }

                        TextFormatWidthMetrics tf = TextFormatSet.GetMatchedFormat(bincode.Length);

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
                    }
                    else
                    {
                        int dieXlength = ASCIIMap.GetLength(0);
                        int dieYlength = ASCIIMap.GetLength(1);

                        if ((xindex <= dieXlength) && (yindex <= dieYlength) &&
                    (xindex >= 0) && (yindex >= 0))
                        {
                            string ascii = ASCIIMap[xindex, yindex].ToString();

                            // 데이터 미생성 또는 ZoomLevel 변경 시
                            if (TextFormatSet.Formats == null || TextFormatSet.Formats.Count == 0 ||
                                TextFormatSet.LastZoomLevel != WaferObject.ZoomLevel)
                            {
                                SetTextFormatWidthMetrics(rectWidth, rectHeight);
                            }

                            TextFormatWidthMetrics tf = TextFormatSet.GetMatchedFormat(ascii.Length);

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

                                target.DrawText(ascii, tf.Foramt, new RawRectangleF(l, t, r, b), resCache["textBrush"] as Brush);
                            }
                        }
                            
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return validDieCount;
        }
    }

    public class TextFormatWidthMetrics
    {
        public int Length { get; set; }
        public TextFormat Foramt { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public class TextFormatSet
    {
        public List<TextFormatWidthMetrics> Formats { get; set; }
        public float LastZoomLevel { get; set; }
        public TextFormatWidthMetrics GetMatchedFormat(int length)
        {
            TextFormatWidthMetrics retval = null;

            try
            {
                retval = Formats.FirstOrDefault(x => x.Length == length);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public TextFormatSet()
        {
            Formats = new List<TextFormatWidthMetrics>();
        }
    }
}

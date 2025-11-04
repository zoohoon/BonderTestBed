using ProberInterfaces;
using ProberInterfaces.Enum;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ResultMapView
{
    /// <summary>
    /// ucResultMapViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ucResultMapViewer : UserControl
    {
        public ucResultMapViewer()
        {
            InitializeComponent();
        }

        #region ..//DependencyProperty

        public static readonly DependencyProperty LockableProperty =
            DependencyProperty.Register(nameof(Lockable), typeof(bool), typeof(ucResultMapViewer), new FrameworkPropertyMetadata(true));
        public bool Lockable
        {
            get { return (bool)this.GetValue(LockableProperty); }
            set { this.SetValue(LockableProperty, value); }
        }

        public static readonly DependencyProperty ZoomLevelVisibilityProperty =
            DependencyProperty.Register(nameof(ZoomLevelVisibility), typeof(Visibility), typeof(ucResultMapViewer), new FrameworkPropertyMetadata(Visibility.Visible));
        public Visibility ZoomLevelVisibility
        {
            get { return (Visibility)this.GetValue(ZoomLevelVisibilityProperty); }
            set { this.SetValue(ZoomLevelVisibilityProperty, value); }

        }

        public static readonly DependencyProperty WaferObjectProperty =
            DependencyProperty.Register(nameof(WaferObject), typeof(IWaferObject), typeof(ucResultMapViewer), new FrameworkPropertyMetadata(null));
        public IWaferObject WaferObject
        {
            get { return (IWaferObject)this.GetValue(WaferObjectProperty); }
            set { this.SetValue(WaferObjectProperty, value); }
        }

        public static readonly DependencyProperty ASCIIMapProperty =
            DependencyProperty.Register(nameof(ASCIIMap), typeof(char[,]), typeof(ucResultMapViewer), new FrameworkPropertyMetadata(null));
        public char[,] ASCIIMap
        {
            get { return (char[,])this.GetValue(ASCIIMapProperty); }
            set { this.SetValue(ASCIIMapProperty, value); }
        }

        public static readonly DependencyProperty DisplayBinCodeProperty =
            DependencyProperty.Register(nameof(DisplayBinCode), typeof(bool), typeof(ucResultMapViewer), new FrameworkPropertyMetadata(null));
        public bool DisplayBinCode
        {
            get { return (bool)this.GetValue(DisplayBinCodeProperty); }
            set { this.SetValue(DisplayBinCodeProperty, value); }
        }

        public static readonly DependencyProperty CoordinateManagerProperty =
            DependencyProperty.Register(nameof(CoordinateManager), typeof(ICoordinateManager), typeof(ucResultMapViewer), null);
        public ICoordinateManager CoordinateManager
        {
            get { return (ICoordinateManager)this.GetValue(CoordinateManagerProperty); }
            set { this.SetValue(CoordinateManagerProperty, value); }
        }

        public static readonly DependencyProperty UnderDutDicesProperty =
            DependencyProperty.Register(nameof(UnderDutDices), typeof(ObservableCollection<IDeviceObject>), typeof(ucResultMapViewer),
                new FrameworkPropertyMetadata(new ObservableCollection<IDeviceObject>()));
        public ObservableCollection<IDeviceObject> UnderDutDices
        {
            get { return (ObservableCollection<IDeviceObject>)this.GetValue(UnderDutDicesProperty); }
            set { this.SetValue(UnderDutDicesProperty, value); }
        }

        public static readonly DependencyProperty CursorXIndexProperty =
            DependencyProperty.Register(nameof(CursorXIndex), typeof(int), typeof(ucResultMapViewer),
            new FrameworkPropertyMetadata((int)0));
        public int CursorXIndex
        {
            get { return (int)this.GetValue(CursorXIndexProperty); }
            set { this.SetValue(CursorXIndexProperty, value); }
        }

        public static readonly DependencyProperty CursorYIndexProperty =
            DependencyProperty.Register(nameof(CursorYIndex), typeof(int), typeof(ucResultMapViewer),
             new FrameworkPropertyMetadata((int)0));
        public int CursorYIndex
        {
            get { return (int)this.GetValue(CursorYIndexProperty); }
            set { this.SetValue(CursorYIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedXIndexProperty =
            DependencyProperty.Register(nameof(SelectedXIndex), typeof(int), typeof(ucResultMapViewer),
            new FrameworkPropertyMetadata((int)0));
        public int SelectedXIndex
        {
            get { return (int)this.GetValue(SelectedXIndexProperty); }
            set { this.SetValue(SelectedXIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedYIndexProperty =
            DependencyProperty.Register(nameof(SelectedYIndex), typeof(int), typeof(ucResultMapViewer),
             new FrameworkPropertyMetadata((int)0));
        public int SelectedYIndex
        {
            get { return (int)this.GetValue(SelectedYIndexProperty); }
            set { this.SetValue(SelectedYIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrXYIndexProperty =
            DependencyProperty.Register(nameof(CurrXYIndex), typeof(System.Windows.Point), typeof(ucResultMapViewer),
            new FrameworkPropertyMetadata(new System.Windows.Point(0, 0)));

        public System.Windows.Point CurrXYIndex
        {
            get { return (System.Windows.Point)this.GetValue(CurrXYIndexProperty); }
            set { this.SetValue(CurrXYIndexProperty, value); }
        }

        public static readonly DependencyProperty CurCameraProperty =
            DependencyProperty.Register(nameof(CurCamera)
                , typeof(ICamera),
                typeof(ucResultMapViewer),
                new FrameworkPropertyMetadata(null));
        public ICamera CurCamera
        {
            get { return (ICamera)this.GetValue(CurCameraProperty); }
            set { this.SetValue(CurCameraProperty, value); }
        }

        public static readonly DependencyProperty MultiDevVisibleProperty =
                DependencyProperty.Register(nameof(MultiDevVisible), typeof(bool), typeof(ucResultMapViewer), new FrameworkPropertyMetadata(false));
        public bool MultiDevVisible
        {
            get { return (bool)this.GetValue(MultiDevVisibleProperty); }
            set { this.SetValue(MultiDevVisibleProperty, value); }
        }

        public static readonly DependencyProperty EnalbeClickToMoveProperty =
        DependencyProperty.Register(nameof(EnalbeClickToMove), typeof(bool), typeof(ucResultMapViewer), new FrameworkPropertyMetadata(true));
        public bool EnalbeClickToMove
        {
            get { return (bool)this.GetValue(EnalbeClickToMoveProperty); }
            set { this.SetValue(EnalbeClickToMoveProperty, value); }
        }

        public static readonly DependencyProperty RenderModeProperty =
        DependencyProperty.Register(nameof(RenderMode), typeof(MapViewMode), typeof(ucResultMapViewer),
            new FrameworkPropertyMetadata(MapViewMode.MapMode));
        public MapViewMode RenderMode
        {
            get { return (MapViewMode)this.GetValue(RenderModeProperty); }
            set { this.SetValue(RenderModeProperty, value); }
        }

        public static readonly DependencyProperty StageSyncEnalbeProperty =
        DependencyProperty.Register(nameof(StageSyncEnalbe),
            typeof(bool), typeof(ucResultMapViewer),
            new FrameworkPropertyMetadata((bool)true));
        public bool StageSyncEnalbe
        {
            get { return (bool)this.GetValue(StageSyncEnalbeProperty); }
            set { this.SetValue(StageSyncEnalbeProperty, value); }
        }

        public static readonly DependencyProperty StepLabelProperty =
            DependencyProperty.Register(nameof(StepLabel), typeof(string),
                typeof(ucResultMapViewer), new FrameworkPropertyMetadata(null));
        public string StepLabel
        {
            get { return (string)this.GetValue(StepLabelProperty); }
            set { this.SetValue(StepLabelProperty, value); }
        }

        public static readonly DependencyProperty IsCrossLineVisibleProperty =
            DependencyProperty.Register(nameof(IsCrossLineVisible), typeof(bool), typeof(ucResultMapViewer),
            new FrameworkPropertyMetadata(true));
        public bool IsCrossLineVisible
        {
            get { return (bool)this.GetValue(IsCrossLineVisibleProperty); }
            set { this.SetValue(IsCrossLineVisibleProperty, value); }
        }

        public static readonly DependencyProperty HighlightDiesProperty =
            DependencyProperty.Register("HighlightDies", typeof(AsyncObservableCollection<HighlightDieComponent>), typeof(ucResultMapViewer),
                new FrameworkPropertyMetadata(new AsyncObservableCollection<HighlightDieComponent>()));
        public AsyncObservableCollection<HighlightDieComponent> HighlightDies
        {
            get { return (AsyncObservableCollection<HighlightDieComponent>)this.GetValue(HighlightDiesProperty); }
            set { this.SetValue(HighlightDiesProperty, value); }
        }

        public static readonly DependencyProperty BINTypeProperty = DependencyProperty.Register("BINType", typeof(BinType), typeof(ucResultMapViewer), new FrameworkPropertyMetadata(BinType.BIN_PASSFAIL));
        public BinType BINType
        {
            get { return (BinType)this.GetValue(BINTypeProperty); }
            set { this.SetValue(BINTypeProperty, value); }
        }

        #endregion
    }
}

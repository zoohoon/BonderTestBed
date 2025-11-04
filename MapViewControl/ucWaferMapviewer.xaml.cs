using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MapView
{
    using LoaderBase.Communication;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ucWaferMapviewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ucWaferMapviewer : UserControl
    {
        public ucWaferMapviewer()
        {
            InitializeComponent();
        }

        public Guid GUID { get; set; } = new Guid("D1F3E640-2AD0-8085-F5D1-512D41DBAE71");

        public bool IsReleaseMode { get; set; }
        public BindingBase IsEnableBindingBase { get; set; }
        public int MaskingLevel { get; set; }
        //public bool Lockable { get; set; } = true;
        public bool InnerLockable { get; set; }
        public List<int> AvoidLockHashCodes { get; set; }

        #region ..//DependencyProperty

        public static readonly DependencyProperty LockableProperty =
            DependencyProperty.Register(nameof(Lockable), typeof(bool), typeof(ucWaferMapviewer), new FrameworkPropertyMetadata(true));
        public bool Lockable
        {
            get { return (bool)this.GetValue(LockableProperty); }
            set { this.SetValue(LockableProperty, value); }
        }

        public static readonly DependencyProperty ZoomLevelVisibilityProperty =
            DependencyProperty.Register(nameof(ZoomLevelVisibility), typeof(Visibility), typeof(ucWaferMapviewer), new FrameworkPropertyMetadata(Visibility.Visible));
        public Visibility ZoomLevelVisibility
        {
            get { return (Visibility)this.GetValue(ZoomLevelVisibilityProperty); }
            set { this.SetValue(ZoomLevelVisibilityProperty, value); }
        }

        public static readonly DependencyProperty WaferObjectProperty =
            DependencyProperty.Register(nameof(WaferObject), typeof(IWaferObject), typeof(ucWaferMapviewer), new FrameworkPropertyMetadata(null));
        public IWaferObject WaferObject
        {
            get { return (IWaferObject)this.GetValue(WaferObjectProperty); }
            set { this.SetValue(WaferObjectProperty, value); }
        }

        public static readonly DependencyProperty CoordinateManagerProperty =
            DependencyProperty.Register(nameof(CoordinateManager), typeof(ICoordinateManager), typeof(ucWaferMapviewer), null);
        public ICoordinateManager CoordinateManager
        {
            get { return (ICoordinateManager)this.GetValue(CoordinateManagerProperty); }
            set { this.SetValue(CoordinateManagerProperty, value); }
        }

        public static readonly DependencyProperty UnderDutDicesProperty =
            DependencyProperty.Register(nameof(UnderDutDices), typeof(ObservableCollection<IDeviceObject>), typeof(ucWaferMapviewer),
                new FrameworkPropertyMetadata(new ObservableCollection<IDeviceObject>()));
        public ObservableCollection<IDeviceObject> UnderDutDices
        {
            get { return (ObservableCollection<IDeviceObject>)this.GetValue(UnderDutDicesProperty); }
            set { this.SetValue(UnderDutDicesProperty, value); }
        }

        public static readonly DependencyProperty CursorXIndexProperty =
            DependencyProperty.Register(nameof(CursorXIndex), typeof(int), typeof(ucWaferMapviewer),
            new FrameworkPropertyMetadata((int)0));
        public int CursorXIndex
        {
            get { return (int)this.GetValue(CursorXIndexProperty); }
            set { this.SetValue(CursorXIndexProperty, value); }
        }

        public static readonly DependencyProperty CursorYIndexProperty =
            DependencyProperty.Register(nameof(CursorYIndex), typeof(int), typeof(ucWaferMapviewer),
             new FrameworkPropertyMetadata((int)0));
        public int CursorYIndex
        {
            get { return (int)this.GetValue(CursorYIndexProperty); }
            set { this.SetValue(CursorYIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedXIndexProperty =
            DependencyProperty.Register(nameof(SelectedXIndex), typeof(int), typeof(ucWaferMapviewer),
            new FrameworkPropertyMetadata((int)0));
        public int SelectedXIndex
        {
            get { return (int)this.GetValue(SelectedXIndexProperty); }
            set { this.SetValue(SelectedXIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedYIndexProperty =
            DependencyProperty.Register(nameof(SelectedYIndex), typeof(int), typeof(ucWaferMapviewer),
             new FrameworkPropertyMetadata((int)0));
        public int SelectedYIndex
        {
            get { return (int)this.GetValue(SelectedYIndexProperty); }
            set { this.SetValue(SelectedYIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrXYIndexProperty =
            DependencyProperty.Register(nameof(CurrXYIndex), typeof(System.Windows.Point), typeof(ucWaferMapviewer),
            new FrameworkPropertyMetadata(new System.Windows.Point(0, 0)));

        public System.Windows.Point CurrXYIndex
        {
            get { return (System.Windows.Point)this.GetValue(CurrXYIndexProperty); }
            set { this.SetValue(CurrXYIndexProperty, value); }
        }

        public static readonly DependencyProperty LoaderCommunicationManagerProperty =
    DependencyProperty.Register(nameof(LoaderCommunicationManager), typeof(ILoaderCommunicationManager), typeof(ucWaferMapviewer), null);
        public ILoaderCommunicationManager LoaderCommunicationManager
        {
            get { return (ILoaderCommunicationManager)this.GetValue(LoaderCommunicationManagerProperty); }
            set { this.SetValue(LoaderCommunicationManagerProperty, value); }
        }

        public static readonly DependencyProperty CurCameraProperty =
            DependencyProperty.Register(nameof(CurCamera)
                , typeof(ICamera),
                typeof(ucWaferMapviewer),
                new FrameworkPropertyMetadata(null));
        public ICamera CurCamera
        {
            get { return (ICamera)this.GetValue(CurCameraProperty); }
            set { this.SetValue(CurCameraProperty, value); }
        }

        public static readonly DependencyProperty MultiDevVisibleProperty =
                DependencyProperty.Register(nameof(MultiDevVisible), typeof(bool), typeof(ucWaferMapviewer), new FrameworkPropertyMetadata(false));
        public bool MultiDevVisible
        {
            get { return (bool)this.GetValue(MultiDevVisibleProperty); }
            set { this.SetValue(MultiDevVisibleProperty, value); }
        }

        public static readonly DependencyProperty EnalbeClickToMoveProperty =
        DependencyProperty.Register(nameof(EnalbeClickToMove), typeof(bool), typeof(ucWaferMapviewer), new FrameworkPropertyMetadata(true));
        public bool EnalbeClickToMove
        {
            get { return (bool)this.GetValue(EnalbeClickToMoveProperty); }
            set { this.SetValue(EnalbeClickToMoveProperty, value); }
        }

        public static readonly DependencyProperty RenderModeProperty =
        DependencyProperty.Register(nameof(RenderMode), typeof(MapViewMode), typeof(ucWaferMapviewer),
            new FrameworkPropertyMetadata(MapViewMode.MapMode));
        public MapViewMode RenderMode
        {
            get { return (MapViewMode)this.GetValue(RenderModeProperty); }
            set { this.SetValue(RenderModeProperty, value); }
        }

        public static readonly DependencyProperty StageSyncEnalbeProperty =
        DependencyProperty.Register(nameof(StageSyncEnalbe),
            typeof(bool), typeof(ucWaferMapviewer),
            new FrameworkPropertyMetadata((bool)true));
        public bool StageSyncEnalbe
        {
            get { return (bool)this.GetValue(StageSyncEnalbeProperty); }
            set { this.SetValue(StageSyncEnalbeProperty, value); }
        }

        public static readonly DependencyProperty StepLabelProperty =
            DependencyProperty.Register(nameof(StepLabel), typeof(string),
                typeof(ucWaferMapviewer), new FrameworkPropertyMetadata(null));
        public string StepLabel
        {
            get { return (string)this.GetValue(StepLabelProperty); }
            set { this.SetValue(StepLabelProperty, value); }
        }

        public static readonly DependencyProperty IsCrossLineVisibleProperty =
            DependencyProperty.Register(nameof(IsCrossLineVisible), typeof(bool), typeof(ucWaferMapviewer),
            new FrameworkPropertyMetadata(true));
        public bool IsCrossLineVisible
        {
            get { return (bool)this.GetValue(IsCrossLineVisibleProperty); }
            set { this.SetValue(IsCrossLineVisibleProperty, value); }
        }

        public static readonly DependencyProperty MapViewEncoderVisiabilityProperty =
            DependencyProperty.Register("MapViewEncoderVisiability",
                        typeof(bool), typeof(ucWaferMapviewer),
                        new FrameworkPropertyMetadata((bool)false));
        /// <summary>
        /// MapView 오른쪽 하단에 Motion Encoder표시할지 안할지. (True: 표시함, False :표시안함)
        /// </summary>
        public bool MapViewEncoderVisiability
        {
            get { return (bool)this.GetValue(MapViewEncoderVisiabilityProperty); }
            set { this.SetValue(MapViewEncoderVisiabilityProperty, value); }
        }

        public static readonly DependencyProperty HighlightDiesProperty =
            DependencyProperty.Register("HighlightDies", typeof(AsyncObservableCollection<HighlightDieComponent>), typeof(ucWaferMapviewer),
                new FrameworkPropertyMetadata(new AsyncObservableCollection<HighlightDieComponent>()));
        public AsyncObservableCollection<HighlightDieComponent> HighlightDies
        {
            get { return (AsyncObservableCollection<HighlightDieComponent>)this.GetValue(HighlightDiesProperty); }
            set { this.SetValue(HighlightDiesProperty, value); }
        }

        public static readonly DependencyProperty BINTypeProperty = DependencyProperty.Register("BINType", typeof(BinType), typeof(ucWaferMapviewer), new FrameworkPropertyMetadata(BinType.BIN_PASSFAIL));
        public BinType BINType
        {
            get { return (BinType)this.GetValue(BINTypeProperty); }
            set { this.SetValue(BINTypeProperty, value); }
        }

        //public static readonly DependencyProperty MapViewOpacityProperty = DependencyProperty.Register("MapViewOpacity",
        //        typeof(double), typeof(ucWaferMapviewer),
        //        new FrameworkPropertyMetadata((double)1.0));
        ///// <summary>
        ///// MapView 오른쪽 하단에 Motion Encoder표시할지 안할지. (True: 표시함, False :표시안함)
        ///// </summary>
        //public double MapViewOpacity
        //{
        //    get { return (double)this.GetValue(MapViewOpacityProperty); }
        //    set { this.SetValue(MapViewOpacityProperty, value); }
        //}

        #endregion

    }
}

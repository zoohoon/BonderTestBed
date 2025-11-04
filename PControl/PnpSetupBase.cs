using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnPControl
{
    using LogModule;
    using MahApps.Metro.Controls.Dialogs;
    using PnPontrol;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.Param;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using RelayCommandBase;
    using SharpDXRender;
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using LoaderBase.Communication;
    using ProberInterfaces.NeedleClean;
    using System.IO;
    using MetroDialogInterfaces;
    using ucDutViewer;
    using CylType;
    using System.Linq;
    using ProberInterfaces.WaferAlignEX;

    [Serializable, DataContract]
    public abstract class PNPSetupBase : CategoryNodeSetupBase, IPnpSetup, INotifyPropertyChanged, IHasRenderLayer, IDutViewControlVM
    {
        public override bool Initialized { get; set; } = false;

        [NonSerialized, DataMember]
        private IProberStation _Prober;
        public IProberStation Prober
        {
            get { return _Prober; }
            set { _Prober = value; }
        }

        [NonSerialized]
        private IStageSupervisor _StageSupervisor;
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set { _StageSupervisor = value; }
        }

        [NonSerialized]
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

        [NonSerialized]
        private ICoordinateManager _CoordManager;
        public ICoordinateManager CoordManager
        {
            get { return _CoordManager; }
            set
            {
                if (value != _CoordManager)
                {
                    _CoordManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private IPnpManager _PnpManager;
        public IPnpManager PnpManager
        {
            get { return _PnpManager; }
            set
            {
                if (value != _PnpManager)
                {
                    _PnpManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IProbingModule Probing
        {
            get { return this.ProbingModule(); }
        }


        private ILoaderCommunicationManager _LoaderCommManager;
        public ILoaderCommunicationManager LoaderCommManager
        {
            get { return _LoaderCommManager; }
            set
            {
                if (value != _LoaderCommManager)
                {
                    _LoaderCommManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<byte[]> _PackagableParams
             = new List<byte[]>();

        public List<byte[]> PackagableParams
        {
            get { return _PackagableParams; }
            set { _PackagableParams = value; }
        }

        public double ZoomLevel { get; set; }
        public bool? AddCheckBoxIsChecked { get; set; }
        public bool? EnableDragMap { get; set; } = true;
        public bool? ShowCurrentPos { get; set; }
        public bool? ShowGrid { get; set; }
        public bool? ShowPad { get; set; }
        public bool? ShowPin { get; set; }
        public bool? ShowSelectedDut { get; set; }

        public bool IsEnableMoving { get; set; } = false;
        public double CurXPos { get; set; }
        public double CurYPos { get; set; }
        #region //..Property
        private bool _IsSeleted;
        public bool IsSeleted
        {
            get { return _IsSeleted; }
            set
            {
                if (value != _IsSeleted)
                {
                    _IsSeleted = value;
                    RaisePropertyChanged();
                }
            }
        }



        private EnumSetupProgressState _ProcessingType = EnumSetupProgressState.IDLE;
        public EnumSetupProgressState ProcessingType
        {
            get { return _ProcessingType; }
            set
            {
                if (value != _ProcessingType)
                {
                    _ProcessingType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _TreeviewItemVisibility = Visibility.Hidden;
        public Visibility TreeviewItemVisibility
        {
            get { return _TreeviewItemVisibility; }
            set
            {
                if (value != _TreeviewItemVisibility)
                {
                    _TreeviewItemVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private float _MapSize;
        public float MapSize
        {
            get { return _MapSize; }
            set
            {
                if (value != _MapSize)
                {
                    _MapSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MapTransparency;
        public double MapTransparency
        {
            get { return _MapTransparency; }
            set
            {
                if (value != _MapTransparency)
                {
                    _MapTransparency = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineIndex _MapIndex;
        [ParamIgnore]
        public MachineIndex MapIndex
        {
            get { return _MapIndex; }
            set
            {
                if (value != _MapIndex)
                {
                    _MapIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumProberCam _CameraType;
        public EnumProberCam CameraType
        {
            get { return _CameraType; }
            set
            {
                if (value != _CameraType)
                {
                    _CameraType = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EnumProberCam CamType => CameraType;

        private object _MapViewTarget;
        public object MapViewTarget
        {
            get { return _MapViewTarget; }
            set
            {
                if (value != _MapViewTarget)
                {
                    _MapViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private RenderLayer _SharpDXLayer;
        public RenderLayer SharpDXLayer
        {
            get { return _SharpDXLayer; }
            set
            {
                if (value != _SharpDXLayer)
                {
                    _SharpDXLayer = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return this.PnpManager.DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }



        public IWaferObject WaferObject { get { return this.GetParam_Wafer(); } }

        public IProbeCard ProbeCard => this.GetParam_ProbeCard();

        public INeedleCleanObject NC => this.GetParam_NcObject();

        [NonSerialized]
        private ImageBuffer _ImgBuffer;
        public ImageBuffer ImgBuffer
        {
            get { return _ImgBuffer; }
            set
            {
                if (value != _ImgBuffer)
                {
                    _ImgBuffer = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private INeedleCleanObject _NCObject;
        //public INeedleCleanObject NCObject
        //{
        //    get { return _NCObject; }
        //    set
        //    {
        //        if (value != _NCObject)
        //        {
        //            _NCObject = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}



        [NonSerialized]
        private IMotionManager _MotionManager;
        public IMotionManager MotionManager
        {
            get { return _MotionManager; }
            set { _MotionManager = value; }
        }

        private int _CurrMaskingLevel;
        public int CurrMaskingLevel
        {
            get { return _CurrMaskingLevel; }
            set
            {
                if (value != _CurrMaskingLevel)
                {
                    _CurrMaskingLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private String _StepLabel;
        public String StepLabel
        {
            get { return _StepLabel; }
            set
            {
                if (value != _StepLabel)
                {
                    _StepLabel = value;
                    RaisePropertyChanged();

                    if (this.LoaderRemoteMediator() != null)
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.StepLabelUpdated(_StepLabel);
                    }
                }
            }
        }

        private String _StepSecondLabel;
        public String StepSecondLabel
        {
            get { return _StepSecondLabel; }
            set
            {
                if (value != _StepSecondLabel)
                {
                    _StepSecondLabel = value;
                    RaisePropertyChanged();

                    if (this.LoaderRemoteMediator() != null)
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.StepSecondLabelUpdated(_StepSecondLabel);
                    }
                }
            }
        }
        private bool _StepLabelActive;
        public bool StepLabelActive
        {
            get { return _StepLabelActive; }
            set
            {
                if (value != _StepLabelActive)
                {
                    _StepLabelActive = value;
                    RaisePropertyChanged();

                    if (this.LoaderRemoteMediator() != null)
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.StepLabelActiveUpdated(_StepLabelActive);
                    }
                }
            }
        }

        private bool _StepSecondLabelActive;
        public bool StepSecondLabelActive
        {
            get { return _StepSecondLabelActive; }
            set
            {
                if (value != _StepSecondLabelActive)
                {
                    _StepSecondLabelActive = value;
                    RaisePropertyChanged();

                    if (this.LoaderRemoteMediator() != null)
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.StepSecondLabelActiveUpdated(_StepSecondLabelActive);
                    }
                }
            }
        }
        //private ObservableCollection<IDeviceObject> _UnderDutList;
        //public ObservableCollection<IDeviceObject> UnderDutList
        //{
        //    get { return _UnderDutList; }
        //    set
        //    {
        //        if (value != _UnderDutList)
        //        {
        //            _UnderDutList = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}               

        public double MainViewTargetWidth = 888;
        public double MainViewTargetHeight = 890;
        public double MiniViewTargetWidth = 250;
        public double MiniViewTargetHeight = 250;

        private bool _IsParamChanged = false;
        public bool IsParamChanged
        {
            get { return _IsParamChanged; }
            set
            {
                if (value != _IsParamChanged)
                {
                    _IsParamChanged = value;
                    RaisePropertyChanged();
                }
            }
        }




        #region ==> CurCam
        private ICamera _CurCam;
        public virtual ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                ICamera precam = null;
                if (value != _CurCam)
                {
                    precam = _CurCam;
                    _CurCam = value;

                    if (WaferObject != null)
                    {
                        if (WaferObject.MapViewAssignCamType != null)
                        {
                            if ((MiniViewTarget is IWaferObject | MainViewTarget is IWaferObject)
                                & WaferObject.MapViewStageSyncEnable & _CurCam.GetChannelType() != WaferObject.MapViewAssignCamType.Value)
                            {
                                WaferObject.MapViewAssignCamType.Value = _CurCam.GetChannelType();
                            }
                        }
                    }

                    RaisePropertyChanged();
                }
                ConfirmDisplay(precam, _CurCam);
            }
        }

        public bool KeppCamOverlay = true;


        public void MakeSideTextblock(string content, double fontsize, Brush fontcolor, Brush Backcolor)
        {
            try
            {
                SideViewTextBlockDescriptor tb = new SideViewTextBlockDescriptor();
                tb.SideTextContents = content;
                tb.SideTextFontSize = fontsize;
                tb.SideTextFontColor = fontcolor;
                tb.SideTextBackground = Backcolor;

                SideViewTextBlocks.Add(tb);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected void ConfirmDisplay(ICamera precam, ICamera curcam)
        {
            try
            {
                if (precam != null)
                {
                    if (precam.DrawDisplayDelegate != null && KeppCamOverlay)
                    {
                        curcam.DrawDisplayDelegate += precam.DrawDisplayDelegate;
                        precam.InDrawOverlayDisplay();
                        curcam.UpdateOverlayFlag = true;
                    }
                }
                else if (curcam != null)
                {
                    if (curcam.DrawDisplayDelegate != null)
                        curcam.UpdateOverlayFlag = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> UcDispaly Port Target Rectangle
        private double _TargetRectangleLeft;
        public virtual double TargetRectangleLeft
        {
            get { return _TargetRectangleLeft; }
            set
            {
                if (value != _TargetRectangleLeft)
                {
                    _TargetRectangleLeft = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TargetRectangleTop;
        public virtual double TargetRectangleTop
        {
            get { return _TargetRectangleTop; }
            set
            {
                if (value != _TargetRectangleTop)
                {
                    _TargetRectangleTop = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TargetRectangleWidth;
        public double TargetRectangleWidth
        {
            get { return _TargetRectangleWidth; }
            set
            {
                if (value != _TargetRectangleWidth)
                {
                    //if (!(value > CurCam.GetGrabSizeWidth()) && !(value < 0))
                    //{
                    //    _TargetRectangleWidth = value;
                    //    NotifyPropertyChanged("TargetRectangleWidth");
                    //}
                    if (!(value < 4))
                    {
                        _TargetRectangleWidth = value;
                        RaisePropertyChanged();
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.DisplayPortRetangleUpdated(_TargetRectangleWidth, _TargetRectangleHeight);
                    }
                }
            }
        }

        private double _TargetRectangleHeight;
        public double TargetRectangleHeight
        {
            get { return _TargetRectangleHeight; }
            set
            {
                if (value != _TargetRectangleHeight)
                {
                    //if (!(value > CurCam.GetGrabSizeHeight()) && !(value < 0))
                    //{
                    //    _TargetRectangleHeight = value;
                    //    NotifyPropertyChanged("TargetRectangleHeight");
                    //}

                    if (!(value < 4))
                    {
                        _TargetRectangleHeight = value;
                        RaisePropertyChanged();
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.DisplayPortRetangleUpdated(_TargetRectangleWidth, _TargetRectangleHeight);
                    }
                }
            }
        }

        private UserControlFucEnum _UseUserControl
             = UserControlFucEnum.DEFAULT;
        public UserControlFucEnum UseUserControl
        {
            get { return _UseUserControl; }
            set
            {
                if (value != _UseUserControl)
                {
                    _UseUserControl = value;
                    RaisePropertyChanged();
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.DisplayPortUserControlUpdated(_UseUserControl);

                }
            }
        }

        private bool _DisplayClickToMoveEnalbe = true;
        public bool DisplayClickToMoveEnalbe
        {
            get { return _DisplayClickToMoveEnalbe; }
            set
            {
                if (value != _DisplayClickToMoveEnalbe)
                {
                    _DisplayClickToMoveEnalbe = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int ChangeWidthValue = 4;
        public int ChangeHeightValue = 4;

        public double PatternSizeWidth;
        public double PatternSizeHeight;
        public double PatternSizeLeft;
        public double PatternSizeTop;

        public void UCDisplayRectWidthPlus()
        {
            try
            {
                ChangeWidthValue = Math.Abs(ChangeWidthValue);
                ModifyUCDisplayRect(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public void UCDisplayRectWidthMinus()
        {
            try
            {
                ChangeWidthValue = -Math.Abs(ChangeWidthValue);
                ModifyUCDisplayRect(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void UCDisplayRectHeightMinus()
        {
            try
            {
                ChangeHeightValue = -Math.Abs(ChangeHeightValue);
                ModifyUCDisplayRect(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void UCDisplayRectHeightPlus()
        {
            try
            {
                ChangeHeightValue = Math.Abs(ChangeHeightValue);
                ModifyUCDisplayRect(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private EventCodeEnum ModifyUCDisplayRect(bool iswidth = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PatternSizeWidth = TargetRectangleWidth;
                PatternSizeHeight = TargetRectangleHeight;
                PatternSizeLeft = TargetRectangleLeft;
                PatternSizeTop = TargetRectangleTop;
                if (iswidth)
                {
                    PatternSizeWidth += ChangeWidthValue;
                    PatternSizeLeft -= (ChangeWidthValue / 2);
                    TargetRectangleWidth = PatternSizeWidth;
                    PatternSizeWidth = TargetRectangleWidth;
                }
                else
                {
                    PatternSizeHeight += ChangeHeightValue;
                    PatternSizeTop -= (ChangeHeightValue / 2);
                    TargetRectangleHeight = PatternSizeHeight;
                    PatternSizeHeight = TargetRectangleHeight;
                }


                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        #endregion

        #region ==> ViewSwapCommand
        //==> Main View와 Mini View를 Swap 하기 위한 버튼과의 Binding Command
        private RelayCommand<object> _ViewSwapCommand;
        public ICommand ViewSwapCommand
        {
            get
            {
                if (null == _ViewSwapCommand) _ViewSwapCommand = new RelayCommand<object>(ViewSwapFunc);
                return _ViewSwapCommand;
            }
        }
        public virtual void ViewSwapFunc(object parameter)
        {
            object swap = MainViewTarget;
            //MainViewTarget = WaferObject;
            MainViewTarget = MiniViewTarget;
            MiniViewTarget = swap;
        }
        #endregion
        #region ==> PatternCloseCommand
        //==> Main View와 Mini View를 Swap 하기 위한 버튼과의 Binding Command
        private RelayCommand<object> _PatternCloseCommand;
        public ICommand PatternCloseCommand
        {
            get
            {
                if (null == _PatternCloseCommand) _PatternCloseCommand = new RelayCommand<object>(PatternCloseFunc);
                return _PatternCloseCommand;
            }
        }
        public virtual void PatternCloseFunc(object parameter)
        {
            MiniViewTarget = PrevMiniViewTarget;
        }
        #endregion
        #region ==> MainViewTarget
        private object _MainViewTarget;
        public object MainViewTarget
        {
            get { return _MainViewTarget; }
            set
            {
                if (value != _MainViewTarget)
                {
                    _MainViewTarget = value;
                    if (_MainViewTarget is IWaferObject)
                    {
                        MainViewZoomVisibility = Visibility.Visible;
                        LightJogVisibility = Visibility.Hidden;
                    }
                    else if (_MainViewTarget is IDisplayPort)
                    {
                        MainViewZoomVisibility = Visibility.Hidden;
                        LightJogVisibility = Visibility.Visible;
                    }
                    else if (_MainViewTarget is IProbeCard)
                    {
                        MainViewZoomVisibility = Visibility.Hidden;
                        LightJogVisibility = Visibility.Hidden;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private BitmapImage _MainViewImageSource;
        public BitmapImage MainViewImageSource
        {
            get { return _MainViewImageSource; }
            set
            {
                if (value != _MainViewImageSource)
                {
                    _MainViewImageSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte[] _MainViewImageSourceArray;
        [DataMember]
        public byte[] MainViewImageSourceArray
        {
            get { return _MainViewImageSourceArray; }
            set
            {
                if (value != _MainViewImageSourceArray)
                {
                    _MainViewImageSourceArray = value;
                    RaisePropertyChanged();

                }
            }
        }

        public void SetMainViewImageSource(string path)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    var image = new System.Windows.Media.Imaging.BitmapImage(
                                           new Uri(path, UriKind.Absolute));

                    byte[] buffer;
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        buffer = ms.ToArray();
                    }

                    MainViewImageSource = image;
                    MainViewImageSourceArray = buffer;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public void SetMiniViewImageSource(string path)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    var image = new System.Windows.Media.Imaging.BitmapImage(new Uri(path, UriKind.Absolute));

                    byte[] buffer;
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        buffer = ms.ToArray();
                    }

                    // image to ImageBuffer

                    MiniViewTarget = BitmapToImageBuffer(image);

                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ImageBuffer BitmapToImageBuffer(BitmapSource bitmapSource)
        {
            ImageBuffer retval = null;

            try
            {
                int stride = (int)bitmapSource.PixelWidth * ((bitmapSource.Format.BitsPerPixel + 7) / 8);

                var ArrImageBuff = new byte[(int)bitmapSource.PixelHeight * stride];

                bitmapSource.CopyPixels(ArrImageBuff, stride, 0);

                retval = new ImageBuffer(ArrImageBuff, bitmapSource.PixelWidth, bitmapSource.PixelHeight, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                bitmapSource = null;
            }

            return retval;
        }
        #region ==> PrevMiniViewTarget
        private object _PrevMiniViewTarget;
        public object PrevMiniViewTarget
        {
            get { return _PrevMiniViewTarget; }
            set
            {
                if (value != _PrevMiniViewTarget)
                {
                    _PrevMiniViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MiniViewTarget
        private object _MiniViewTarget;
        public object MiniViewTarget
        {
            get { return _MiniViewTarget; }
            set
            {
                if (value != _MiniViewTarget)
                {
                    if (!(_MiniViewTarget is ImageBuffer))
                    {
                        PrevMiniViewTarget = _MiniViewTarget;
                    }
                    _MiniViewTarget = value;
                    if (_MiniViewTarget is IDisplayPort)
                    {
                        MiniViewZoomVisibility = Visibility.Hidden;
                    }
                    else if (_MiniViewTarget is IWaferObject)
                    {
                        MiniViewZoomVisibility = Visibility.Visible;
                    }
                    else if (_MiniViewTarget is IProbeCard)
                    {
                        MiniViewZoomVisibility = Visibility.Hidden;
                    }
                    else if (_MiniViewTarget is ImageBuffer)
                    {
                        if (this.LoaderRemoteMediator() != null)
                        {
                            this.LoaderRemoteMediator()?.GetServiceCallBack()?.ImageBufferUpdated((ImageBuffer)_MiniViewTarget);
                        }
                    }

                    RaisePropertyChanged(nameof(MiniViewTarget));
                }
                if (!(_MiniViewTarget is ImageBuffer))
                {
                    PatternVisibility = Visibility.Hidden;
                    if (_MiniViewTarget == null)
                        MiniViewSwapVisibility = Visibility.Hidden;
                    else
                        MiniViewSwapVisibility = Visibility.Visible;
                }
                else
                {
                    MiniViewSwapVisibility = Visibility.Hidden;
                    PatternVisibility = Visibility.Visible;
                }
            }
        }
        #endregion

        #region ==> PatternViewTarget
        private object _PatternViewTarget;
        public object PatternViewTarget
        {
            get { return _PatternViewTarget; }
            set
            {
                if (value != _PatternViewTarget)
                {
                    _PatternViewTarget = value;

                    RaisePropertyChanged();
                }
                if (_PatternViewTarget == null)
                    MiniViewSwapVisibility = Visibility.Hidden;
                else
                    MiniViewSwapVisibility = Visibility.Visible;
            }
        }
        #endregion

        #region ==> SideViewTarget
        private Visibility _SideViewTargetVisibility;
        public Visibility SideViewTargetVisibility
        {
            get { return _SideViewTargetVisibility; }
            set
            {
                if (value != _SideViewTargetVisibility)
                {
                    _SideViewTargetVisibility = value;

                    if (_SideViewTargetVisibility == Visibility.Hidden)
                    {
                        SideViewSwitchVisibility = Visibility.Hidden;
                    }
                    else
                    {
                        //if ((SideViewDisplayMode != SideViewMode.EXPANDER_ONLY) ||
                        //    (SideViewDisplayMode != SideViewMode.TEXTBLOCK_ONLY))
                        //{
                        //    SideViewSwitchVisibility = Visibility.Visible;
                        //}
                    }

                    RaisePropertyChanged();

                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        private Visibility _SideViewSwitchVisibility = Visibility.Hidden;
        public Visibility SideViewSwitchVisibility
        {
            get { return _SideViewSwitchVisibility; }
            set
            {
                if (value != _SideViewSwitchVisibility)
                {
                    _SideViewSwitchVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SideViewExpanderVisibility;
        public bool SideViewExpanderVisibility
        {
            get { return _SideViewExpanderVisibility; }
            set
            {
                if (value != _SideViewExpanderVisibility)
                {
                    _SideViewExpanderVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SideViewTextVisibility;
        public bool SideViewTextVisibility
        {
            get { return _SideViewTextVisibility; }
            set
            {
                if (value != _SideViewTextVisibility)
                {
                    _SideViewTextVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SideViewWidth;
        public double SideViewWidth
        {
            get { return _SideViewWidth; }
            set
            {
                if (value != _SideViewWidth)
                {
                    _SideViewWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SideViewHeight;
        public double SideViewHeight
        {
            get { return _SideViewHeight; }
            set
            {
                if (value != _SideViewHeight)
                {
                    _SideViewHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Thickness _SideViewMargin = new Thickness(0, 0, 0, 0);
        public Thickness SideViewMargin
        {
            get { return _SideViewMargin; }
            set
            {
                if (value != _SideViewMargin)
                {
                    _SideViewMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SideViewMode _SideViewDisplayMode;
        public SideViewMode SideViewDisplayMode
        {
            get { return _SideViewDisplayMode; }
            set
            {
                if (value != _SideViewDisplayMode)
                {
                    _SideViewDisplayMode = value;

                    if (_SideViewDisplayMode == SideViewMode.EXPANDER_MODE)
                    {
                        SideViewExpanderVisibility = true;
                        SideViewTextVisibility = false;
                        SideViewSwitchVisibility = Visibility.Visible;
                    }
                    else if (_SideViewDisplayMode == SideViewMode.TEXTBLOCK_MODE)
                    {
                        SideViewExpanderVisibility = false;
                        SideViewTextVisibility = true;
                        SideViewSwitchVisibility = Visibility.Visible;
                    }
                    else if (_SideViewDisplayMode == SideViewMode.EXPANDER_ONLY)
                    {
                        SideViewExpanderVisibility = true;
                        SideViewTextVisibility = false;
                        SideViewSwitchVisibility = Visibility.Hidden;
                    }
                    else if (_SideViewDisplayMode == SideViewMode.TEXTBLOCK_ONLY)
                    {
                        SideViewExpanderVisibility = false;
                        SideViewTextVisibility = true;
                        SideViewSwitchVisibility = Visibility.Hidden;
                    }

                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand _ChangeSideViewVisibleCommand;
        public ICommand ChangeSideViewVisibleCommand
        {
            get
            {
                if (null == _ChangeSideViewVisibleCommand) _ChangeSideViewVisibleCommand = new RelayCommand(ChangeSideViewVisible);
                return _ChangeSideViewVisibleCommand;
            }
        }

        public virtual void ChangeSideViewVisible()
        {
            try
            {
                if (SideViewDisplayMode == SideViewMode.EXPANDER_MODE)
                {
                    SideViewDisplayMode = SideViewMode.TEXTBLOCK_MODE;
                    SideViewExpanderVisibility = false;
                    SideViewTextVisibility = true;
                }
                else if (SideViewDisplayMode == SideViewMode.TEXTBLOCK_MODE)
                {
                    SideViewDisplayMode = SideViewMode.EXPANDER_MODE;
                    SideViewExpanderVisibility = true;
                    SideViewTextVisibility = false;
                }
                //else if ((SideViewDisplayMode == SideViewMode.EXPANDER_ONLY) || 
                //         (SideViewDisplayMode == SideViewMode.TEXTBLOCK_ONLY))
                //{                    
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string _SideViewTitle;
        public string SideViewTitle
        {
            get { return _SideViewTitle; }
            set
            {
                if (value != _SideViewTitle)
                {
                    _SideViewTitle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SideViewTitleFontSize = 24;
        public double SideViewTitleFontSize
        {
            get { return _SideViewTitleFontSize; }
            set
            {
                if (value != _SideViewTitleFontSize)
                {
                    _SideViewTitleFontSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _SideViewTitleFontColor = Brushes.Transparent;
        public Brush SideViewTitleFontColor
        {
            get { return _SideViewTitleFontColor; }
            set
            {
                if (value != _SideViewTitleFontColor)
                {
                    _SideViewTitleFontColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _SideViewTitleBackground = Brushes.Transparent;
        public Brush SideViewTitleBackground
        {
            get { return _SideViewTitleBackground; }
            set
            {
                if (value != _SideViewTitleBackground)
                {
                    _SideViewTitleBackground = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> Expanders
        private SideViewExpanderDescriptor _ExpanderItem_01 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_01
        {
            get { return _ExpanderItem_01; }
            set
            {
                if (value != _ExpanderItem_01)
                {
                    _ExpanderItem_01 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SideViewExpanderDescriptor _ExpanderItem_02 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_02
        {
            get { return _ExpanderItem_02; }
            set
            {
                if (value != _ExpanderItem_02)
                {
                    _ExpanderItem_02 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SideViewExpanderDescriptor _ExpanderItem_03 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_03
        {
            get { return _ExpanderItem_03; }
            set
            {
                if (value != _ExpanderItem_03)
                {
                    _ExpanderItem_03 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SideViewExpanderDescriptor _ExpanderItem_04 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_04
        {
            get { return _ExpanderItem_04; }
            set
            {
                if (value != _ExpanderItem_04)
                {
                    _ExpanderItem_04 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SideViewExpanderDescriptor _ExpanderItem_05 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_05
        {
            get { return _ExpanderItem_05; }
            set
            {
                if (value != _ExpanderItem_05)
                {
                    _ExpanderItem_05 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SideViewExpanderDescriptor _ExpanderItem_06 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_06
        {
            get { return _ExpanderItem_06; }
            set
            {
                if (value != _ExpanderItem_06)
                {
                    _ExpanderItem_06 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SideViewExpanderDescriptor _ExpanderItem_07 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_07
        {
            get { return _ExpanderItem_07; }
            set
            {
                if (value != _ExpanderItem_07)
                {
                    _ExpanderItem_07 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SideViewExpanderDescriptor _ExpanderItem_08 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_08
        {
            get { return _ExpanderItem_08; }
            set
            {
                if (value != _ExpanderItem_08)
                {
                    _ExpanderItem_08 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SideViewExpanderDescriptor _ExpanderItem_09 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_09
        {
            get { return _ExpanderItem_09; }
            set
            {
                if (value != _ExpanderItem_09)
                {
                    _ExpanderItem_09 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SideViewExpanderDescriptor _ExpanderItem_10 = new SideViewExpanderDescriptor();
        public SideViewExpanderDescriptor ExpanderItem_10
        {
            get { return _ExpanderItem_10; }
            set
            {
                if (value != _ExpanderItem_10)
                {
                    _ExpanderItem_10 = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SideViewTextBlockDescriptor _SideViewTextBlock = new SideViewTextBlockDescriptor();
        public SideViewTextBlockDescriptor SideViewTextBlock
        {
            get { return _SideViewTextBlock; }
            set
            {
                if (value != _SideViewTextBlock)
                {
                    _SideViewTextBlock = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<SideViewTextBlockDescriptor> _SideViewTextBlocks = new ObservableCollection<SideViewTextBlockDescriptor>();
        public ObservableCollection<SideViewTextBlockDescriptor> SideViewTextBlocks
        {
            get { return _SideViewTextBlocks; }
            set
            {
                if (value != _SideViewTextBlocks)
                {
                    _SideViewTextBlocks = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> PadJogLeft
        private PNPCommandButtonDescriptor _PadJogLeft = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogLeft
        {
            get { return _PadJogLeft; }
            set
            {
                if (value != _PadJogLeft)
                {
                    _PadJogLeft = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PadJogRight
        private PNPCommandButtonDescriptor _PadJogRight = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogRight
        {
            get { return _PadJogRight; }
            set
            {
                if (value != _PadJogRight)
                {
                    _PadJogRight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PadJogUp
        private PNPCommandButtonDescriptor _PadJogUp = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogUp
        {
            get { return _PadJogUp; }
            set
            {
                if (value != _PadJogUp)
                {
                    _PadJogUp = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PadJogDown
        private PNPCommandButtonDescriptor _PadJogDown = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogDown
        {
            get { return _PadJogDown; }
            set
            {
                if (value != _PadJogDown)
                {
                    _PadJogDown = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PadJogSelect

        private CancellationTokenSourcePack _PadJogSelectTokenPack = new CancellationTokenSourcePack();
        public CancellationTokenSourcePack PadJogSelectTokenPack
        {
            get { return _PadJogSelectTokenPack; }
            set
            {
                if (value != _PadJogSelectTokenPack)
                {
                    _PadJogSelectTokenPack = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PNPCommandButtonDescriptor _PadJogSelect = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogSelect
        {
            get { return _PadJogSelect; }
            set
            {
                if (value != _PadJogSelect)
                {
                    _PadJogSelect = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> PadJogLeftUp
        private PNPCommandButtonDescriptor _PadJogLeftUp = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogLeftUp
        {
            get { return _PadJogLeftUp; }
            set
            {
                if (value != _PadJogLeftUp)
                {
                    _PadJogLeftUp = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PadJogRightUp
        private PNPCommandButtonDescriptor _PadJogRightUp = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogRightUp
        {
            get { return _PadJogRightUp; }
            set
            {
                if (value != _PadJogRightUp)
                {
                    _PadJogRightUp = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PadJogLeftDown
        private PNPCommandButtonDescriptor _PadJogLeftDown = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogLeftDown
        {
            get { return _PadJogLeftDown; }
            set
            {
                if (value != _PadJogLeftDown)
                {
                    _PadJogLeftDown = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PadJogRightDown
        private PNPCommandButtonDescriptor _PadJogRightDown = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogRightDown
        {
            get { return _PadJogRightDown; }
            set
            {
                if (value != _PadJogRightDown)
                {
                    _PadJogRightDown = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        //#region ==> ZoomObject
        //private IZoomObject _ZoomObject;
        //public IZoomObject ZoomObject
        //{
        //    get { return this.StageSupervisor().WaferObject;/*return _ZoomObject*/}
        //    set
        //    {
        //        if (value != _ZoomObject)
        //        {
        //            _ZoomObject = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //#endregion

        //#region ==> PlusCommand
        //private RelayCommand _PlusCommand;
        //public ICommand PlusCommand
        //{
        //    get
        //    {
        //        if (null == _PlusCommand) _PlusCommand = new RelayCommand(Plus);
        //        return _PlusCommand;
        //    }
        //}
        //public virtual void Plus()
        //{
        //    string Plus = string.Empty;
        //    if (ZoomObject != null)
        //        ZoomObject.ZoomLevel--;
        //}
        //#endregion

        //#region ==> MinusCommand
        //private RelayCommand _MinusCommand;
        //public ICommand MinusCommand
        //{
        //    get
        //    {
        //        if (null == _MinusCommand) _MinusCommand = new RelayCommand(Minus);
        //        return _MinusCommand;
        //    }
        //}
        //public virtual void Minus()
        //{
        //    string Minus = string.Empty;
        //    if (ZoomObject != null)
        //        ZoomObject.ZoomLevel++;
        //}
        //#endregion

        #region ==> OneButton
        private PNPCommandButtonDescriptor _OneButton = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor OneButton
        {
            get { return _OneButton; }
            set
            {
                if (value != _OneButton)
                {
                    _OneButton = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> TwoButton
        private PNPCommandButtonDescriptor _TwoButton = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor TwoButton
        {
            get { return _TwoButton; }
            set
            {
                if (value != _TwoButton)
                {
                    _TwoButton = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ThreeButton
        private PNPCommandButtonDescriptor _ThreeButton = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor ThreeButton
        {
            get { return _ThreeButton; }
            set
            {
                if (value != _ThreeButton)
                {
                    _ThreeButton = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> FourButton
        private PNPCommandButtonDescriptor _FourButton = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor FourButton
        {
            get { return _FourButton; }
            set
            {
                if (value != _FourButton)
                {
                    _FourButton = value;
                    RaisePropertyChanged();
                }
            }
        }



        #endregion

        #region ==> FiveButton

        private PNPCommandButtonDescriptor _FiveButton = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor FiveButton
        {
            get { return _FiveButton; }
            set
            {
                if (value != _FiveButton)
                {
                    _FiveButton = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> Visibilities
        private Visibility _MiniViewTargetVisibility;
        public Visibility MiniViewTargetVisibility
        {
            get { return _MiniViewTargetVisibility; }
            set
            {
                if (value != _MiniViewTargetVisibility)
                {
                    _MiniViewTargetVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MiniViewSwapVisibility;
        public Visibility MiniViewSwapVisibility
        {
            get { return _MiniViewSwapVisibility; }
            set
            {
                if (value != _MiniViewSwapVisibility)
                {
                    _MiniViewSwapVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _LightJogVisibility;
        public Visibility LightJogVisibility
        {
            get { return _LightJogVisibility; }
            set
            {
                if (value != _LightJogVisibility)
                {
                    _LightJogVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MotionJogVisibility;
        public Visibility MotionJogVisibility
        {
            get { return _MotionJogVisibility; }
            set
            {
                if (value != _MotionJogVisibility)
                {
                    _MotionJogVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _PatternVisibility;
        public Visibility PatternVisibility
        {
            get { return _PatternVisibility; }
            set
            {
                if (value != _PatternVisibility)
                {
                    _PatternVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private JogMode _JogType;
        public JogMode JogType
        {
            get { return _JogType; }
            set
            {
                if (value != _JogType)
                {
                    _JogType = value;
                    RaisePropertyChanged();

                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.JogModeUpdated(_JogType);
                    }
                }
            }
        }

        private bool _MotionJogEnabled = true;
        public bool MotionJogEnabled
        {
            get { return _MotionJogEnabled; }
            set
            {
                if (value != _MotionJogEnabled)
                {
                    _MotionJogEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }



        private Visibility _PNPBtnVisibility;
        public Visibility PNPBtnVisibility
        {
            get { return _PNPBtnVisibility; }
            set
            {
                if (value != _PNPBtnVisibility)
                {
                    _PNPBtnVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _NaviVisibility;
        public Visibility NaviVisibility
        {
            get { return _NaviVisibility; }
            set
            {
                if (value != _NaviVisibility)
                {
                    _NaviVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MiniViewZoomVisibility;
        public Visibility MiniViewZoomVisibility
        {
            get { return _MiniViewZoomVisibility; }
            set
            {
                if (value != _MiniViewZoomVisibility)
                {
                    _MiniViewZoomVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MainViewZoomVisibility;
        public Visibility MainViewZoomVisibility
        {
            get { return _MainViewZoomVisibility; }
            set
            {
                if (value != _MainViewZoomVisibility)
                {
                    _MainViewZoomVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LightJogEnable = true;
        public bool LightJogEnable
        {
            get { return _LightJogEnable; }
            set
            {
                if (value != _LightJogEnable)
                {
                    _LightJogEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private bool _MotionJogEnable = true;
        //public bool MotionJogEnable
        //{
        //    get { return _MotionJogEnable; }
        //    set
        //    {
        //        if (value != _MotionJogEnable)
        //        {
        //            _MotionJogEnable = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool _DisplayClickToMoveEnable = true;
        public bool DisplayClickToMoveEnable
        {
            get { return _DisplayClickToMoveEnable; }
            set
            {
                if (value != _DisplayClickToMoveEnable)
                {
                    _DisplayClickToMoveEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        private bool _DisplayIsHitTestVisible = true;
        public bool DisplayIsHitTestVisible
        {
            get { return _DisplayIsHitTestVisible; }
            set
            {
                if (value != _DisplayIsHitTestVisible)
                {
                    _DisplayIsHitTestVisible = value;
                    RaisePropertyChanged();
                }
            }
        }


        private HorizontalAlignment _MiniViewHorizontalAlignment
            = HorizontalAlignment.Right;
        public HorizontalAlignment MiniViewHorizontalAlignment
        {
            get { return _MiniViewHorizontalAlignment; }
            set
            {
                if (value != _MiniViewHorizontalAlignment)
                {
                    _MiniViewHorizontalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }


        private VerticalAlignment _MiniViewVerticalAlignment
             = VerticalAlignment.Top;
        public VerticalAlignment MiniViewVerticalAlignment
        {
            get { return _MiniViewVerticalAlignment; }
            set
            {
                if (value != _MiniViewVerticalAlignment)
                {
                    _MiniViewVerticalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }

        private HorizontalAlignment _SideViewHorizontalAlignment = HorizontalAlignment.Right;
        public HorizontalAlignment SideViewHorizontalAlignment
        {
            get { return _SideViewHorizontalAlignment; }
            set
            {
                if (value != _SideViewHorizontalAlignment)
                {
                    _SideViewHorizontalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }


        private VerticalAlignment _SideViewVerticalAlignment = VerticalAlignment.Top;
        public VerticalAlignment SideViewVerticalAlignment
        {
            get { return _SideViewVerticalAlignment; }
            set
            {
                if (value != _SideViewVerticalAlignment)
                {
                    _SideViewVerticalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public async void ShowMessages(string message)
        {

            await this.MetroDialogManager().ShowMessageDialog("Access denieded.", $"Authority level is not sufficient to access this button({message}).", EnumMessageStyle.AffirmativeAndNegative);
        }

        protected EventCodeEnum InitPnpModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //this.SysState().SetSetUpState();
                if (SetupState == null)
                    SetupState = new NotCompletedState(this);
                if (SetupState._Module == null)
                    SetupState._Module = this;
                if (Prober == null)
                    Prober = this.ProberStation();
                if (StageSupervisor == null)
                    StageSupervisor = this.StageSupervisor();
                if (MotionManager == null)
                    MotionManager = this.MotionManager();
                if (VisionManager == null)
                    VisionManager = this.VisionManager();
                if (CoordManager == null)
                    CoordManager = this.CoordinateManager();
                if (PnpManager == null)
                    PnpManager = this.PnPManager();
                //if (DisplayPort == null)
                //{
                //    Application.Current.Dispatcher.Invoke((() =>
                //    {
                //        DisplayPort = new DisplayPort() { GUID = new Guid("34EA361B-3487-4DBC-BF5C-72040A09F73D") };
                //        ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
                //        foreach (var cam in this.VisionManager().GetCameras())
                //        {
                //            this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                //        }
                //    }));
                //}

                ZoomLevel = 20;
                AddCheckBoxIsChecked = true;
                EnableDragMap = true;
                ShowCurrentPos = true;
                ShowGrid = true;
                ShowPad = true;
                ShowPin = true;
                ShowSelectedDut = true;
                CurXPos = 0;
                CurYPos = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw err;
            }

            return retVal;
        }

        protected EventCodeEnum InitPnpModule_AdvenceSetting()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                if (SetupState == null)
                    SetupState = new NotCompletedState(this);
                if (SetupState._Module == null)
                    SetupState._Module = this;
                if (Prober == null)
                    Prober = this.ProberStation();
                if (StageSupervisor == null)
                    StageSupervisor = this.StageSupervisor();
                if (MotionManager == null)
                    MotionManager = this.MotionManager();
                if (VisionManager == null)
                    VisionManager = this.VisionManager();
                if (CoordManager == null)
                    CoordManager = this.CoordinateManager();
                if (PnpManager == null)
                    PnpManager = this.PnPManager();

                //if (DisplayPort == null)
                //{
                //    Application.Current.Dispatcher.Invoke((() =>
                //    {
                //        DisplayPort = new DisplayPort() { GUID = new Guid("34EA361B-3487-4DBC-BF5C-72040A09F73D") };
                //        ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
                //        foreach (var cam in this.VisionManager().GetCameras())
                //        {
                //            this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                //        }
                //        AdvanceSetupUISetting();
                //        retVal = BindingPNPSetup();
                //    }));
                //}
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        protected EventCodeEnum InitPnpModuleStage()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SetupState == null)
                    SetupState = new NotCompletedState(this);
                if (SetupState._Module == null)
                    SetupState._Module = this;
                if (Prober == null)
                    Prober = this.ProberStation();
                if (StageSupervisor == null)
                    StageSupervisor = this.StageSupervisor();
                if (MotionManager == null)
                    MotionManager = this.MotionManager();
                if (VisionManager == null)
                    VisionManager = this.VisionManager();
                if (CoordManager == null)
                    CoordManager = this.CoordinateManager();
                if (PnpManager == null)
                    PnpManager = this.PnPManager();


                Application.Current.Dispatcher.Invoke((() =>
                {
                    //DisplayPort = new DisplayPort() { GUID = new Guid("34EA361B-3487-4DBC-BF5C-72040A09F73D") };
                    //((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
                    //Array stagecamvalues = Enum.GetValues(typeof(StageCam));

                    //foreach (var cam in this.VisionManager().GetCameras())
                    //{
                    //    for (int index = 0; index < stagecamvalues.Length; index++)
                    //    {
                    //        if (((StageCam)stagecamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                    //        {
                    //            this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                    //            break;
                    //        }
                    //    }
                    //}
                    TargetRectangleWidth = 0;
                    TargetRectangleHeight = 0;
                    retVal = BindingPNPSetup();
                }));

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return retVal;
        }

        protected EventCodeEnum InitPnpModuleStage_AdvenceSetting()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SetupState == null)
                    SetupState = new NotCompletedState(this);
                if (SetupState._Module == null)
                    SetupState._Module = this;
                if (Prober == null)
                    Prober = this.ProberStation();
                if (StageSupervisor == null)
                    StageSupervisor = this.StageSupervisor();
                if (MotionManager == null)
                    MotionManager = this.MotionManager();
                if (VisionManager == null)
                    VisionManager = this.VisionManager();
                if (CoordManager == null)
                    CoordManager = this.CoordinateManager();
                if (PnpManager == null)
                    PnpManager = this.PnPManager();


                Application.Current.Dispatcher.Invoke((() =>
                {
                    //DisplayPort = new DisplayPort() { GUID = new Guid("34EA361B-3487-4DBC-BF5C-72040A09F73D") };
                    //((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
                    //Array stagecamvalues = Enum.GetValues(typeof(StageCam));

                    //foreach (var cam in this.VisionManager().GetCameras())
                    //{
                    //    for (int index = 0; index < stagecamvalues.Length; index++)
                    //    {
                    //        if (((StageCam)stagecamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                    //        {
                    //            this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                    //            break;
                    //        }
                    //    }
                    //}
                    TargetRectangleWidth = 0;
                    TargetRectangleHeight = 0;
                    AdvanceSetupUISetting();
                    retVal = BindingPNPSetup();
                }));

            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public virtual EventCodeEnum InitLightJog(IUseLightJog module, EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //StageSupervisor = this.StageSupervisor();
                //VisionManager = this.VisionManager();
                //CurCam 이 할당된 뒤에 호출해야함.
                if (this.PnPManager() != null)
                    this.PnPManager().PnpLightJog.InitCameraJog(module, camtype);//==> Nick : Light Jog를 Update하여 UI 구성을 함, InitSetup마다 호출 해야함. 
                //EnableState = new EnableIdleState(EnableState);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        public EventCodeEnum SetMotionJogMoveZOffsetEnable(bool movezoffsetenable)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.PnPManager() != null)
                    this.PnPManager().PnpMotionJog.SetMoveZOffsetEnable = movezoffsetenable;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum UpdateCameraLightValue()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.PnPManager() != null)
                    this.PnPManager().PnpLightJog.UpdateCameraLightValue();
                //EnableState = new EnableIdleState(EnableState);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        public EventCodeEnum BindingPNPSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Binding bindRenderLayer = new Binding
                {
                    Path = new System.Windows.PropertyPath("SharpDXLayer"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.RenderLayerProperty, bindRenderLayer);

                Binding bindTargetRectWidth = new Binding
                {
                    Path = new System.Windows.PropertyPath("TargetRectangleWidth"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.TargetRectangle_Width, bindTargetRectWidth);

                Binding bindTargetRectHeight = new Binding
                {
                    Path = new System.Windows.PropertyPath("TargetRectangleHeight"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.TargetRectangle_Height, bindTargetRectHeight);

                Binding bindTargetRectLeft = new Binding
                {
                    Path = new System.Windows.PropertyPath("TargetRectangleLeft"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.TargetRectangle_Left, bindTargetRectLeft);

                Binding bindTargetRectTop = new Binding
                {
                    Path = new System.Windows.PropertyPath("TargetRectangleTop"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.TargetRectangle_Top, bindTargetRectTop);

                Binding bindUseUserControl = new Binding
                {
                    Path = new System.Windows.PropertyPath("UseUserControl"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.UseUserControlFuncProp, bindUseUserControl);

                Binding bindDisplayClickMoveEnable = new Binding
                {
                    Path = new System.Windows.PropertyPath("DisplayClickToMoveEnalbe"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.EnalbeClickToMoveProperty, bindDisplayClickMoveEnable);

                Binding bindCamera = new Binding
                {
                    Path = new System.Windows.PropertyPath("CurCam"),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);

                Binding bindtext = new Binding
                {
                    Path = new System.Windows.PropertyPath("StepLabel"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.OverlayTextProperty, bindtext);

                Binding bindlabel = new Binding
                {
                    Path = new System.Windows.PropertyPath("StepSecondLabel"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.OverlayLabelProperty, bindlabel);

                Binding bindtextactive = new Binding
                {
                    Path = new System.Windows.PropertyPath("StepLabelActive"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.OverlayTextActiveProperty, bindtextactive);

                Binding bindlabelactive = new Binding
                {
                    Path = new System.Windows.PropertyPath("StepSecondLabelActive"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.OverlayLabelActiveProperty, bindlabelactive);


                Binding bindhittestvisiable = new Binding
                {
                    Path = new System.Windows.PropertyPath("DisplayIsHitTestVisible"),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.IsHitTestVisibleProperty, bindhittestvisiable);

                Binding bindcurx = new Binding
                {
                    Path = new System.Windows.PropertyPath(nameof(CurXPos)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((DutViewControl)this.PnPManager().DutViewControl,
                            DutViewControl.CurXPosProperty, bindcurx);

                Binding bindcury = new Binding
                {
                    Path = new System.Windows.PropertyPath(nameof(CurYPos)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((DutViewControl)this.PnPManager().DutViewControl,
                            DutViewControl.CurYPosProperty, bindcury);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return retVal;
        }

        public async Task Abort()
        {
            try
            {

                EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                                    "Are you sure you want to stop all the current work and exit the PNP window?",
                                    "Press the OK button and the previous contents will disappear.",
                                    EnumMessageStyle.AffirmativeAndNegative);

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    await this.ViewModelManager().BackPreScreenTransition();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SwitchCamera(ICamera cam)
        {
            CurCam = cam;
        }

        public void EnableDisplayClickMove()
        {
            DisplayClickToMoveEnable = true;
        }

        public void DisableDisplayClickMove()
        {
            DisplayClickToMoveEnable = false;
        }

        //public abstract Guid ViewModelGUID { get; }

        public virtual void SetPackagableParams()
        {

        }

        public virtual Task CloseAdvanceSetupView()
        {
            try
            {
                PackagableParams = AdvanceSetupViewModel.GetParameters();
                this.PnPManager().ApplyParams(PackagableParams);

                this.PnPManager().CloseAdvanceSetupView();

                //SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        public virtual void UpdateLabel()
        {

        }

        //public virtual Task<EventCodeEnum> InitViewModel()
        //{
        //    return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        //}

        //public virtual Task<EventCodeEnum> PageSwitched(object parameter = null)
        //{
        //    return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        //}

        //public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        //{
        //    return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        //}

        public void RegisteViewModel()
        {

        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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

        public new virtual void DeInitModule()
        {
        }

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                if (parameter != null)
                {
                    if (IsParameterChanged())
                    {
                        if (this is IHasDevParameterizable)
                            (this as IHasDevParameterizable).SaveDevParameter();
                        if (this is IHasSysParameterizable)
                            (this as IHasSysParameterizable).SaveSysParameter();
                    }

                    ClearOverlayCameras();

                    if (parameter is EventCodeEnum)
                    {
                        if ((EventCodeEnum)parameter == EventCodeEnum.NODATA)
                        {
                            parameter = EventCodeEnum.NONE;
                            return (EventCodeEnum)parameter;
                        }
                        if ((EventCodeEnum)parameter == EventCodeEnum.NONE)
                        {
                            this.StageSupervisor().StageModuleState.ZCLEARED();
                            if (CurCam != null)
                            {
                                this.VisionManager().StopGrab(CurCam.GetChannelType());
                            }
                            StageCylinderType.MoveWaferCam.Retract();
                            return (EventCodeEnum)parameter;
                        }
                    }

                    if (this.PnPManager().PnpScreen is ITemplate)
                    {
                        if (parameter == this)
                            return EventCodeEnum.NONE;
                        ITemplate template = this.PnPManager().PnpScreen as ITemplate;
                        if (template != null)
                        {
                            //ObservableCollection<ICategoryNodeItem> stpes = this.PnpManager().GetNotFormPnpStps();
                            foreach (var step in this.PnPManager().PnpSteps)
                            //for (int step =0;step < stpes.Count; step++)
                            {
                                for (int index = 0; index < step.Count; index++)
                                {

                                    if (step[index] == parameter)
                                        return EventCodeEnum.NONE;
                                    else if (step[index] is IPnpCategoryForm)
                                    {
                                        EventCodeEnum ret = (step[index] as IPnpCategoryForm).ValidationCategoryStep(parameter);
                                        if (ret == EventCodeEnum.NONE)
                                            return EventCodeEnum.NONE;
                                        else if (ret == EventCodeEnum.UNDEFINED)
                                            continue;
                                        else
                                        {

                                            await this.MetroDialogManager().ShowMessageDialog(
                                                "Can not move Setup step.",
                                                "Move Setup is possible only after completing all previous setups.",
                                                EnumMessageStyle.Affirmative);

                                            return EventCodeEnum.UNDEFINED;
                                        }
                                    }
                                    else if (step[index] is ICategoryNodeItem)
                                    {
                                        if ((step[index] as ICategoryNodeItem).StateSetup == EnumMoudleSetupState.NONE
                                             || (step[index] as ICategoryNodeItem).StateSetup == EnumMoudleSetupState.COMPLETE)
                                            continue;
                                        else if (((ICategoryNodeItem)step[index]).StateSetup == EnumMoudleSetupState.NOTCOMPLETED)
                                        {

                                            await this.MetroDialogManager().ShowMessageDialog(
                                                "Can not move Setup step.",
                                                "The setting is not completed.unsaved data will be lost.",
                                                EnumMessageStyle.Affirmative);

                                            return EventCodeEnum.UNDEFINED;
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
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return EventCodeEnum.UNDEFINED;
            }
            return EventCodeEnum.UNDEFINED;
        }

        public override EventCodeEnum ClearSettingData()
        {
            return EventCodeEnum.NONE;
        }

        #region //..DisplayPort
        public RegisteImageBufferParam GetDisplayPortRectInfo()
        {
            RegisteImageBufferParam ret = new RegisteImageBufferParam();
            try
            {

                if (CurCam == null)
                    return ret;

                double a = CurCam.GetGrabSizeWidth();
                double b = TargetRectangleWidth;
                double c = DisplayPort.StandardOverlayCanvaseWidth;

                double width = (((CurCam.GetGrabSizeWidth() * TargetRectangleWidth) / DisplayPort.StandardOverlayCanvaseWidth));
                double height = (((CurCam.GetGrabSizeHeight() * TargetRectangleHeight) / DisplayPort.StandardOverlayCanvaseHeight));
                double offsetx = (((CurCam.GetGrabSizeWidth() * TargetRectangleLeft) / DisplayPort.StandardOverlayCanvaseWidth));
                double offsety = (((CurCam.GetGrabSizeHeight() * TargetRectangleTop) / DisplayPort.StandardOverlayCanvaseHeight));

                //double offsetx = (CurCam.GetGrabSizeWidth() / 2) - width / 2;
                //double offsety = (CurCam.GetGrabSizeHeight() / 2) - height / 2;

                ret.Width = (int)width;
                ret.Height = (int)height;
                ret.LocationX = (int)offsetx;
                ret.LocationY = (int)offsety;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString() }. PnpSetupBase - GetPatternRectInfo()  : Error occured. ");
            }
            return ret;

            //return DisplayPort.GetPatternRectInfo();
        }

        public double ConvertDisplayWidthPNP(double sizex, double imgwidth)
        {
            double width = (((UcDisplayPort.DisplayPort)DisplayPort).StandardOverlayCanvaseWidth * sizex) / imgwidth;
            return width;
        }

        public double ConvertDisplayHeightPNP(double sizey, double imgheight)
        {
            double height = (((UcDisplayPort.DisplayPort)DisplayPort).StandardOverlayCanvaseHeight * sizey) / imgheight;
            return height;
        }

        public void GetDisplayPortActualSize(ref double width, ref double height)
        {
            try
            {
                width = ((UcDisplayPort.DisplayPort)DisplayPort).ActualWidth;
                height = ((UcDisplayPort.DisplayPort)DisplayPort).ActualHeight;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ClearOverlayCameras()
        {
            try
            {
                if (this.VisionManager() != null)
                {
                    List<ICamera> cameras = this.VisionManager().GetCameras();
                    if (cameras != null)
                    {
                        foreach (var cam in cameras)
                        {
                            if (cam.DrawDisplayDelegate != null)
                                cam.DrawDisplayDelegate = null;
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region //..AdvenceSetting
        private CustomDialog _AdvanceSetupView;

        public CustomDialog AdvanceSetupView
        {
            get { return _AdvanceSetupView; }
            set
            {
                _AdvanceSetupView = value;
            }
        }

        private IPnpAdvanceSetupViewModel _AdvanceSetupViewModel;

        public IPnpAdvanceSetupViewModel AdvanceSetupViewModel
        {
            get { return _AdvanceSetupViewModel; }
            set { _AdvanceSetupViewModel = value; }
        }

        public void SetAdvanceSetupView(IPnpAdvanceSetupView view)
        {
            try
            {
                if (view is CustomDialog)
                {
                    AdvanceSetupView = (CustomDialog)view;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetAdvanceSetupViewModel(IPnpAdvanceSetupViewModel viewmodel)
        {
            try
            {
                AdvanceSetupViewModel = viewmodel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        protected void AdvanceSetupUISetting(PNPCommandButtonDescriptor button = null, string IconCaption = "", string IconSource = "")
        {
            if (button == null)
            {
                if (FiveButton.Command == null)
                {
                    FiveButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PNP_Setting.png");
                    FiveButton.Command = new AsyncCommand(ShowAdvanceSetupView, false);
                    //FiveButton.IconCaption = Properies
                    FiveButton.IconCaption = "Setting";
                }
            }
            else
            {
                if (button.Command == null)
                {
                    button.SetIconSoruce(IconSource);
                    button.Command = new AsyncCommand(ShowAdvanceSetupView, false);
                    button.IconCaption = IconCaption;
                }
            }
        }

        public async Task ShowAdvanceSetupView()
        {
            try
            {
                if (AdvanceSetupView == null)
                    return;

                SetPackagableParams();

                AdvanceSetupViewModel.SetParameters(PackagableParams);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AdvanceSetupView.DataContext = AdvanceSetupViewModel;
                });

                IPnpAdvanceSetupViewModel vm = AdvanceSetupViewModel as IPnpAdvanceSetupViewModel;

                vm?.Init();

                //await this.MetroDialogManager().ShowWindow(AdvanceSetupView, (IMetroDialogViewModel)AdvanceSetupViewModel, PackagableParams, true);
                //await this.MetroDialogManager().ShowWindow(AdvanceSetupView);
                await this.MetroDialogManager().ShowAdvancedDialog(AdvanceSetupView, PackagableParams, false);

                //var parameters = this.PnPManager().ParamObjectListConvertToByteList(PackagableParams);
                //if(parameters != null)

                //PackagableParams = AdvanceSetupViewModel.GetParameters();

                //this.PnPManager().ApplyParams(PackagableParams);

                //SetStepSetupState();

            }
            catch (Exception err)
            {
                LoggerManager.Error($"{ err.ToString()} , PnpSetupBase - SetManualInputControl() ");
            }

        }

        ///// <summary>
        ///// Remote Loader 파라미터를 Stage 로 보내주기 위한 함수.
        ///// </summary>
        //public void SetParametersToStage()
        //{
        //    try
        //    {
        //        PackagableParams = AdvanceSetupViewModel.GetParameters();
        //        this.PnPManager().ApplyParams(PackagableParams);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        #endregion

        #region //..SAVE , EXIT

        private AsyncCommand _ExitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (null == _ExitCommand) _ExitCommand = new AsyncCommand(
                    Exit);
                return _ExitCommand;
            }
        }

        public virtual async Task Exit()
        {
            try
            {
                await (this.PnPManager().SeletedStep as IMainScreenViewModel).Cleanup(EventCodeEnum.NONE);

                IsParameterChanged(true);

                await this.ViewModelManager().BackPreScreenTransition();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (null == _SaveCommand) _SaveCommand = new AsyncCommand(
                    Save);
                return _SaveCommand;
            }
        }

        public virtual async Task Save()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                if (this.PnpManager.PnpScreen is ITemplate)
                {
                    foreach (var module in this.PnPManager().PnpSteps)
                    {
                        for (int index = 0; index < module.Count; index++)
                        {
                            if (module[index] is IHasDevParameterizable)
                                retVal = (module[index] as IHasDevParameterizable).SaveDevParameter();
                            if (module[index] is IHasSysParameterizable)
                                retVal = (module[index] as IHasSysParameterizable).SaveSysParameter();
                            if (module[index] is IPnpCategoryForm)
                            {
                                retVal = (module[index] as IPnpCategoryForm).SaveParameter();
                            }
                        }

                    }
                }

                if (this.PnpManager.PnpScreen is IHasDevParameterizable)
                    retVal = (this.PnpManager.PnpScreen as IHasDevParameterizable).SaveDevParameter();
                if (this.PnpManager.PnpScreen is IHasSysParameterizable)
                    retVal = (this.PnpManager.PnpScreen as IHasSysParameterizable).SaveSysParameter();

                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                if (retVal == EventCodeEnum.NONE)
                    await this.MetroDialogManager().ShowMessageDialog("", "Saved", EnumMessageStyle.Affirmative);
                else
                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "An error occurred while saving.I have a module that is not saved properly. Please confirm.", EnumMessageStyle.Affirmative);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SAVE_ExitCommand;
        public ICommand SAVE_ExitCommand
        {
            get
            {
                if (null == _SAVE_ExitCommand) _SAVE_ExitCommand = new AsyncCommand(
                    SAVE_Exit);
                return _SAVE_ExitCommand;
            }
        }

        public virtual async Task SAVE_Exit()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                await (this.PnPManager().SeletedStep as IMainScreenViewModel).Cleanup(EventCodeEnum.NONE);

                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                if (this.PnpManager.PnpScreen is ITemplate)
                {
                    foreach (var module in this.PnPManager().PnpSteps)
                    {
                        if (module is IHasDevParameterizable)
                            retVal = (module as IHasDevParameterizable).SaveDevParameter();
                        if (module is IHasSysParameterizable)
                            retVal = (module as IHasSysParameterizable).SaveSysParameter();
                    }
                }

                if (this.PnpManager.PnpScreen is IHasDevParameterizable)
                    retVal = (this.PnpManager.PnpScreen as IHasDevParameterizable).SaveDevParameter();
                if (this.PnpManager.PnpScreen is IHasSysParameterizable)
                    retVal = (this.PnpManager.PnpScreen as IHasSysParameterizable).SaveSysParameter();


                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                EnumMessageDialogResult mresult = EnumMessageDialogResult.UNDEFIND;
                if (retVal == EventCodeEnum.NONE)
                    await this.MetroDialogManager().ShowMessageDialog("", "Saved", EnumMessageStyle.Affirmative);
                else
                    mresult = await this.MetroDialogManager().ShowMessageDialog("Error Message", "An error occurred while saving.You may have a problem, but do you want to Exit?", EnumMessageStyle.AffirmativeAndNegative);

                if (mresult == EnumMessageDialogResult.AFFIRMATIVE)
                    await this.ViewModelManager().BackPreScreenTransition();
                else if (mresult == EnumMessageDialogResult.NEGATIVE)
                    return;
                else
                    await this.ViewModelManager().BackPreScreenTransition();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #endregion

        #region //..FlyOut
        private bool _OpenFlyOut = false;
        public bool OpenFlyOut
        {
            get { return _OpenFlyOut; }
            set
            {
                if (value != _OpenFlyOut)
                {
                    _OpenFlyOut = value;
                    RaisePropertyChanged();
                }
            }
        }


        private AsyncCommand _PnpFlyoutCommand;
        public ICommand PnpFlyoutCommand
        {
            get
            {
                if (null == _PnpFlyoutCommand) _PnpFlyoutCommand = new AsyncCommand(
                    PnpFlyout);
                return _PnpFlyoutCommand;
            }
        }

        public IAsyncCommand DutAddMouseDownCommand => null;

        public Task PnpFlyout()
        {
            OpenFlyOut = false;
            OpenFlyOut = true;
            return Task.CompletedTask;
        }
        #endregion

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.PnPManager().PnpScreen is ITemplate)
                {
                    foreach (var module in this.PnPManager().PnpSteps)
                    {
                        for (int index = 0; index < module.Count; index++)
                        {
                            if (module[index] is ICategoryNodeItem)
                            {
                                retVal = (module[index] as ICategoryNodeItem).ParamValidation();
                                if (retVal != EventCodeEnum.NONE)
                                    return retVal;
                            }
                        }
                    }

                    //foreach (var module in this.PnpManager().PnpSteps)
                    //{
                    //    for (int index = 0; index < module.Count; index++)
                    //    {
                    //        if (module[index] is ICategoryNodeItem)
                    //        {
                    //            if ((module[index] as ICategoryNodeItem).StateSetup == EnumMoudleSetupState.COMPLETE
                    //                || (module[index]  as ICategoryNodeItem).StateSetup == EnumMoudleSetupState.NONE)
                    //            {
                    //                retVal = EventCodeEnum.NONE;
                    //            }
                    //            else
                    //            {
                    //                retVal = EventCodeEnum.UNDEFINED;
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// PNP Step의 IsParameterChanged를 확인하고 저장해주는 역할까지 한다.
        /// </summary>
        /// <returns></returns>
        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                if (this.PnPManager().PnpSteps == null)
                    return false;

                foreach (var module in this.PnPManager().PnpSteps)
                {
                    for (int index = 0; index < module.Count; index++)
                    {
                        if (module[index] is ICategoryNodeItem)
                        {
                            if (module[index] is IPnpCategoryForm)
                            {
                                IPnpCategoryForm form = module[index] as IPnpCategoryForm;
                                if (form.Categories != null)
                                {
                                    foreach (var cmoudle in form.Categories)
                                    {
                                        retVal = (cmoudle as ICategoryNodeItem).IsParameterChanged();
                                        if (retVal & issave)
                                        {
                                            if (cmoudle is IHasDevParameterizable)
                                                (cmoudle as IHasDevParameterizable).SaveDevParameter();
                                            if (cmoudle is IHasSysParameterizable)
                                                (cmoudle as IHasSysParameterizable).SaveSysParameter();
                                        }


                                    }
                                }
                            }

                            retVal = (module[index] as ICategoryNodeItem).IsParameterChanged();
                            if (retVal & issave)
                            {
                                if (module[index] is IHasDevParameterizable)
                                    (module[index] as IHasDevParameterizable).SaveDevParameter();
                                if (module[index] is IHasSysParameterizable)
                                    (module[index] as IHasSysParameterizable).SaveSysParameter();
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum EnableUseBtn()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (PadJogSelect.Command == null)
                    PadJogSelect.IsEnabled = false;
                else
                    PadJogSelect.IsEnabled = true;

                if (PadJogLeftUp.Command == null)
                    PadJogLeftUp.IsEnabled = false;
                else
                    PadJogLeftUp.IsEnabled = true;
                if (PadJogRightUp.Command == null)
                    PadJogRightUp.IsEnabled = false;
                else
                    PadJogRightUp.IsEnabled = true;
                if (PadJogLeftDown.Command == null)
                    PadJogLeftDown.IsEnabled = false;
                else
                    PadJogLeftDown.IsEnabled = true;
                if (PadJogRightDown.Command == null)
                    PadJogRightDown.IsEnabled = false;
                else
                    PadJogRightDown.IsEnabled = true;
                if (PadJogLeft.Command == null)
                    PadJogLeft.IsEnabled = false;
                else
                    PadJogLeft.IsEnabled = true;
                if (PadJogRight.Command == null)
                    PadJogRight.IsEnabled = false;
                else
                    PadJogRight.IsEnabled = true;
                if (PadJogUp.Command == null)
                    PadJogUp.IsEnabled = false;
                else
                    PadJogUp.IsEnabled = true;
                if (PadJogDown.Command == null)
                    PadJogDown.IsEnabled = false;
                else
                    PadJogDown.IsEnabled = true;

                if (OneButton.Command == null)
                    OneButton.IsEnabled = false;
                else
                    OneButton.IsEnabled = true;
                if (TwoButton.Command == null)
                    TwoButton.IsEnabled = false;
                else
                    TwoButton.IsEnabled = true;
                if (ThreeButton.Command == null)
                    ThreeButton.IsEnabled = false;
                else
                    ThreeButton.IsEnabled = true;
                if (FourButton.Command == null)
                    FourButton.IsEnabled = false;
                else
                    FourButton.IsEnabled = true;

                LightJogEnable = true;

                if (AdvanceSetupView == null && FiveButton.Command == null)
                    FiveButton.IsEnabled = false;
                else
                    FiveButton.IsEnabled = true;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //Log.Error(err, "WAPnpSetupBase - InitCommonUI() :  Error occurred.");
                LoggerManager.Exception(err);

            }

            return retVal;
        }

        public void InitStateUI()
        {
            try
            {
                StepLabel = null;

                StepSecondLabel = null;

                #region ==> Jog Button
                PadJogLeftUp.IconSource = null;
                PadJogLeftUp.IconCaption = null;
                PadJogLeftUp.IconSoruceArray = null;
                PadJogLeftUp.MiniIconSoruceArray = null;
                PadJogLeftUp.MiniIconSource = null;
                PadJogLeftUp.Caption = null;
                PadJogLeftUp.Command = null;
                PadJogLeftUp.IsEnabled = false;

                PadJogRightUp.IconSource = null;
                PadJogRightUp.IconCaption = null;
                PadJogRightUp.IconSoruceArray = null;
                PadJogRightUp.MiniIconSoruceArray = null;
                PadJogRightUp.MiniIconSource = null;
                PadJogRightUp.Caption = null;
                PadJogRightUp.Command = null;
                PadJogRightUp.IsEnabled = false;

                PadJogLeftDown.IconSource = null;
                PadJogLeftDown.IconCaption = null;
                PadJogLeftDown.IconSoruceArray = null;
                PadJogLeftDown.MiniIconSoruceArray = null;
                PadJogLeftDown.MiniIconSource = null;
                PadJogLeftDown.Caption = null;
                PadJogLeftDown.Command = null;
                PadJogLeftDown.IsEnabled = false;

                PadJogRightDown.IconSource = null;
                PadJogRightDown.IconCaption = null;
                PadJogLeftUp.IconSoruceArray = null;
                PadJogRightDown.MiniIconSoruceArray = null;
                PadJogRightDown.MiniIconSource = null;
                PadJogRightDown.Caption = null;
                PadJogRightDown.Command = null;
                PadJogRightDown.IsEnabled = false;

                PadJogLeft.IconSource = null;
                PadJogLeft.IconCaption = null;
                PadJogLeft.IconSoruceArray = null;
                PadJogLeft.MiniIconSoruceArray = null;
                PadJogLeft.MiniIconSource = null;
                PadJogLeft.Caption = null;
                PadJogLeft.Command = null;
                PadJogLeft.IsEnabled = false;

                PadJogRight.IconSource = null;
                PadJogRight.IconCaption = null;
                PadJogRight.IconSoruceArray = null;
                PadJogRight.MiniIconSoruceArray = null;
                PadJogRight.MiniIconSource = null;
                PadJogRight.Caption = null;
                PadJogRight.Command = null;
                PadJogRight.IsEnabled = false;

                PadJogDown.IconSource = null;
                PadJogDown.IconCaption = null;
                PadJogDown.IconSoruceArray = null;
                PadJogDown.MiniIconSoruceArray = null;
                PadJogDown.MiniIconSource = null;
                PadJogDown.Caption = null;
                PadJogDown.Command = null;
                PadJogDown.IsEnabled = false;

                PadJogUp.IconSource = null;
                PadJogUp.IconCaption = null;
                PadJogUp.IconSoruceArray = null;
                PadJogUp.MiniIconSoruceArray = null;
                PadJogUp.MiniIconSource = null;
                PadJogUp.Caption = null;
                PadJogUp.Command = null;
                PadJogUp.IsEnabled = false;

                PadJogSelect.IconSource = null;
                PadJogSelect.IconSoruceArray = null;
                PadJogSelect.MiniIconSoruceArray = null;
                PadJogSelect.MiniIconSource = null;
                PadJogSelect.IconCaption = null;
                PadJogSelect.Caption = null;
                PadJogSelect.Command = null;
                PadJogSelect.IsEnabled = false;

                OneButton.IconSource = null;
                OneButton.IconCaption = null;
                OneButton.IconSoruceArray = null;
                OneButton.MiniIconSoruceArray = null;
                OneButton.MiniIconSource = null;
                OneButton.Caption = null;
                //OneButton.MaskingLevel = -1;
                OneButton.Command = null;
                OneButton.IsEnabled = false;
                OneButton.Visibility = Visibility.Visible;

                TwoButton.IconSource = null;
                TwoButton.IconCaption = null;
                TwoButton.IconSoruceArray = null;
                TwoButton.MiniIconSoruceArray = null;
                TwoButton.MiniIconSource = null;
                TwoButton.Caption = null;
                //TwoButton.MaskingLevel = -1;
                TwoButton.Command = null;
                TwoButton.IsEnabled = false;
                TwoButton.Visibility = Visibility.Visible;

                ThreeButton.IconSource = null;
                ThreeButton.IconCaption = null;
                ThreeButton.IconSoruceArray = null;
                ThreeButton.MiniIconSoruceArray = null;
                ThreeButton.MiniIconSource = null;
                ThreeButton.Caption = null;
                //ThreeButton.MaskingLevel = -1;
                ThreeButton.Command = null;
                ThreeButton.IsEnabled = false;
                ThreeButton.Visibility = Visibility.Visible;

                FourButton.IconSource = null;
                FourButton.IconCaption = null;
                FourButton.IconSoruceArray = null;
                FourButton.MiniIconSoruceArray = null;
                FourButton.MiniIconSource = null;
                FourButton.Caption = null;
                //FourButton.MaskingLevel = -1;
                FourButton.Command = null;
                FourButton.IsEnabled = false;
                FourButton.Visibility = Visibility.Visible;


                if (AdvanceSetupView == null)
                {

                    FiveButton.IconSource = null;
                    FiveButton.IconCaption = null;
                    FiveButton.Caption = null;
                    FiveButton.IconSoruceArray = null;
                    FiveButton.MiniIconSoruceArray = null;
                    FiveButton.MiniIconSource = null;
                    FiveButton.Command = null;
                    FiveButton.IsEnabled = false;
                    FiveButton.Visibility = Visibility.Visible;
                }

                #endregion

                #region ==> SideView Visibility
                SideViewTargetVisibility = Visibility.Hidden;
                SideViewSwitchVisibility = Visibility.Hidden;
                SideViewTextVisibility = false;
                SideViewExpanderVisibility = true;
                SideViewDisplayMode = SideViewMode.TEXTBLOCK_MODE;
                #endregion

                #region ==> SideView TextBlock
                SideViewTextBlock.SideTextContents = "";
                SideViewTextBlock.SideTextFontSize = 18;
                SideViewTextBlock.SideTextFontColor = Brushes.White;
                SideViewTextBlock.SideTextBackground = Brushes.Transparent;
                #endregion

                #region ==> SideView Expanders...
                ExpanderItem_01.Header = "";
                ExpanderItem_01.Description = "";
                ExpanderItem_01.HeaderFontSize = 14;
                ExpanderItem_01.DescriptionFontSize = 14;
                ExpanderItem_01.HeaderColor = Brushes.White;
                ExpanderItem_01.ExpanderVisibility = Visibility.Visible;

                ExpanderItem_02.Header = "";
                ExpanderItem_02.Description = "";
                ExpanderItem_02.HeaderFontSize = 14;
                ExpanderItem_02.DescriptionFontSize = 14;
                ExpanderItem_02.HeaderColor = Brushes.White;
                ExpanderItem_02.ExpanderVisibility = Visibility.Visible;

                ExpanderItem_03.Header = "";
                ExpanderItem_03.Description = "";
                ExpanderItem_03.HeaderFontSize = 14;
                ExpanderItem_03.DescriptionFontSize = 14;
                ExpanderItem_03.HeaderColor = Brushes.White;
                ExpanderItem_03.ExpanderVisibility = Visibility.Visible;

                ExpanderItem_04.Header = "";
                ExpanderItem_04.Description = "";
                ExpanderItem_04.HeaderFontSize = 14;
                ExpanderItem_04.DescriptionFontSize = 14;
                ExpanderItem_04.HeaderColor = Brushes.White;
                ExpanderItem_04.ExpanderVisibility = Visibility.Visible;

                ExpanderItem_05.Header = "";
                ExpanderItem_05.Description = "";
                ExpanderItem_05.HeaderFontSize = 14;
                ExpanderItem_05.DescriptionFontSize = 14;
                ExpanderItem_05.HeaderColor = Brushes.White;
                ExpanderItem_05.ExpanderVisibility = Visibility.Visible;

                ExpanderItem_06.Header = "";
                ExpanderItem_06.Description = "";
                ExpanderItem_06.HeaderFontSize = 14;
                ExpanderItem_06.DescriptionFontSize = 14;
                ExpanderItem_06.HeaderColor = Brushes.White;
                ExpanderItem_06.ExpanderVisibility = Visibility.Visible;

                ExpanderItem_07.Header = "";
                ExpanderItem_07.Description = "";
                ExpanderItem_07.HeaderFontSize = 14;
                ExpanderItem_07.DescriptionFontSize = 14;
                ExpanderItem_07.HeaderColor = Brushes.White;
                ExpanderItem_07.ExpanderVisibility = Visibility.Visible;

                ExpanderItem_08.Header = "";
                ExpanderItem_08.Description = "";
                ExpanderItem_08.HeaderFontSize = 14;
                ExpanderItem_08.DescriptionFontSize = 14;
                ExpanderItem_08.HeaderColor = Brushes.White;
                ExpanderItem_08.ExpanderVisibility = Visibility.Visible;

                ExpanderItem_09.Header = "";
                ExpanderItem_09.Description = "";
                ExpanderItem_09.HeaderFontSize = 14;
                ExpanderItem_09.DescriptionFontSize = 14;
                ExpanderItem_09.HeaderColor = Brushes.White;
                ExpanderItem_09.ExpanderVisibility = Visibility.Visible;

                ExpanderItem_10.Header = "";
                ExpanderItem_10.Description = "";
                ExpanderItem_10.HeaderFontSize = 14;
                ExpanderItem_10.DescriptionFontSize = 14;
                ExpanderItem_10.HeaderColor = Brushes.White;
                ExpanderItem_10.ExpanderVisibility = Visibility.Visible;
                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PNPSetupBase] [InitStateUI()] : {err}");
            }
        }


        /// <summary>
        /// Side View TextBlock을 설정하는 함수
        /// </summary>
        /// <param name="fontcolor"></param>
        /// <param name="background"></param>
        /// <param name="fontsize"></param>
        public void SetSideViewTextProperties(string fontcolor = "White", string background = "Transparent", double fontsize = 18)
        {
            try
            {
                var converter = new System.Windows.Media.BrushConverter();
                var textbrush = (Brush)converter.ConvertFromString(fontcolor);
                var backgroundbrush = (Brush)converter.ConvertFromString(background);

                SideViewTextBlock.SideTextFontSize = fontsize;
                SideViewTextBlock.SideTextFontColor = textbrush;
                SideViewTextBlock.SideTextBackground = backgroundbrush;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PNPSetupBase] [SetSideViewTextProperties()] : {err}");
            }
        }

        /// <summary>
        /// SideView의 Mode를 설정하는 함수
        /// </summary>
        /// <param name="mode"></param>
        public void SetSideViewMode(SideViewMode mode = SideViewMode.TEXTBLOCK_ONLY,
                                    string title = "Information")
        {
            try
            {
                SideViewDisplayMode = mode;

                if (mode == SideViewMode.EXPANDER_MODE)
                {
                    SideViewSwitchVisibility = Visibility.Visible;
                    SideViewTextVisibility = false;
                    SideViewExpanderVisibility = true;
                }
                else if (mode == SideViewMode.TEXTBLOCK_MODE)
                {
                    SideViewSwitchVisibility = Visibility.Visible;
                    SideViewTextVisibility = true;
                    SideViewExpanderVisibility = false;
                }
                else if (mode == SideViewMode.EXPANDER_ONLY)
                {
                    SideViewSwitchVisibility = Visibility.Hidden;
                    SideViewTextVisibility = false;
                    SideViewExpanderVisibility = true;
                }
                else if (mode == SideViewMode.TEXTBLOCK_ONLY)
                {
                    SideViewSwitchVisibility = Visibility.Hidden;
                    SideViewTextVisibility = true;
                    SideViewExpanderVisibility = false;
                }
                else
                {
                    SideViewTargetVisibility = Visibility.Hidden;
                    SideViewSwitchVisibility = Visibility.Hidden;
                    SideViewTextVisibility = false;
                    SideViewExpanderVisibility = false;
                }

                SideViewTitle = title;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PNPSetupBase] [SetSideViewMode()] : {err}");
            }
        }

        /// <summary>
        /// PNP UI들의 Visibility를 설정하는 함수
        /// </summary>
        /// <param name="MiniView"></param>
        /// <param name="MiniViewSwap"></param>
        /// <param name="MiniViewZoom"></param>
        /// <param name="MainViewZoom"></param>
        /// <param name="LightJog"></param>
        /// <param name="SideView"></param>
        /// <param name="MotionJog"></param>
        /// <param name="Navigation"></param>
        /// <param name="PnpButton"></param>
        public void SetPNPVisibility(bool MiniView = true, bool MiniViewSwap = true, bool MiniViewZoom = true,
                                     bool MainViewZoom = true, bool LightJog = true, bool SideView = false,
                                     bool MotionJog = true, bool Navigation = true, bool PnpButton = true)
        {
            try
            {
                #region ==> MiniView
                if (MiniView)
                {
                    MiniViewTargetVisibility = Visibility.Visible;

                    #region ==> MiniViewSwap
                    if (MiniViewSwap)
                    {
                        MiniViewSwapVisibility = Visibility.Visible;
                    }
                    else
                    {
                        MiniViewSwapVisibility = Visibility.Hidden;
                    }
                    #endregion

                    #region ==> MiniViewZoom
                    if (MiniViewZoom)
                    {
                        MiniViewZoomVisibility = Visibility.Visible;
                    }
                    else
                    {
                        MiniViewZoomVisibility = Visibility.Hidden;
                    }
                    #endregion
                }
                else
                {
                    MiniViewTargetVisibility = Visibility.Hidden;
                }
                #endregion


                if (MainViewTarget is IWaferObject)
                {
                    LightJogVisibility = Visibility.Hidden;

                    #region ==> MainViewZoom
                    if (MainViewZoom)
                    {
                        MainViewZoomVisibility = Visibility.Visible;
                    }
                    else
                    {
                        MainViewZoomVisibility = Visibility.Hidden;
                    }
                    #endregion
                }
                else if (MainViewTarget is IDisplayPort)
                {
                    MainViewZoomVisibility = Visibility.Hidden;

                    #region ==> LightJog
                    if (LightJog)
                    {
                        LightJogVisibility = Visibility.Visible;
                    }
                    else
                    {
                        LightJogVisibility = Visibility.Hidden;
                    }
                    #endregion
                }
                else
                {
                    #region ==> MainViewZoom
                    if (MainViewZoom)
                    {
                        MainViewZoomVisibility = Visibility.Visible;
                    }
                    else
                    {
                        MainViewZoomVisibility = Visibility.Hidden;
                    }
                    #endregion

                    #region ==> LightJog
                    if (LightJog)
                    {
                        LightJogVisibility = Visibility.Visible;
                    }
                    else
                    {
                        LightJogVisibility = Visibility.Hidden;
                    }
                    #endregion
                }

                #region ==> SideView
                if (SideView)
                {
                    SideViewTargetVisibility = Visibility.Visible;
                }
                else
                {
                    SideViewTargetVisibility = Visibility.Hidden;
                }
                #endregion

                #region ==> MotionJog
                if (MotionJog)
                {
                    LightJogVisibility = Visibility.Visible;
                }
                else
                {
                    LightJogVisibility = Visibility.Hidden;
                }

                #endregion

                #region ==> Navigation
                if (Navigation)
                {
                    NaviVisibility = Visibility.Visible;
                }
                else
                {
                    NaviVisibility = Visibility.Hidden;
                }
                #endregion

                #region ==> PnpButton
                if (PnpButton)
                {
                    PNPBtnVisibility = Visibility.Visible;
                }
                else
                {
                    PNPBtnVisibility = Visibility.Hidden;
                }
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PNPSetupBase] [SetPNPVisibility()] : {err}");
            }
        }

        public void SetAllJogBtnDisable()
        {
            try
            {
                PadJogLeftUp.IsEnabled = false;
                PadJogRightUp.IsEnabled = false;
                PadJogLeftDown.IsEnabled = false;
                PadJogRightDown.IsEnabled = false;
                PadJogLeft.IsEnabled = false;
                PadJogRight.IsEnabled = false;
                PadJogDown.IsEnabled = false;
                PadJogUp.IsEnabled = false;
                PadJogSelect.IsEnabled = false;

                OneButton.IsEnabled = false;
                TwoButton.IsEnabled = false;
                ThreeButton.IsEnabled = false;
                FourButton.IsEnabled = false;
                FiveButton.IsEnabled = false;

                LightJogEnable = false;
                DisableDisplayClickMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetAllJogBtnEnable()
        {
            try
            {
                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;
                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = true;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;
                PadJogDown.IsEnabled = true;
                PadJogUp.IsEnabled = true;
                PadJogSelect.IsEnabled = true;
                OneButton.IsEnabled = true;
                TwoButton.IsEnabled = true;
                ThreeButton.IsEnabled = true;
                FourButton.IsEnabled = true;
                FiveButton.IsEnabled = true;
                LightJogEnable = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void SetNextStepsNotCompleteState(string selectedstep)
        {
            try
            {
                var curstep = this.PnPManager().PnPNodeItem.Where(item => item.Header == selectedstep).FirstOrDefault();
                int curindex = this.PnPManager().PnPNodeItem.IndexOf(curstep);
                if (curindex >= 0)
                {
                    for (int index = curindex + 1; index < this.PnPManager().PnPNodeItem.Count; index++)
                    {
                        if (this.PnPManager().PnPNodeItem[index].GetModuleSetupState() == EnumMoudleSetupState.COMPLETE)
                        {
                            this.PnPManager().PnPNodeItem[index].SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #region //..Loader
        public virtual void SetPNPDataDescriptor(PNPDataDescription pNPDataDescriptor)
        {
            return;
        }

        public void ConverterImageSource(PNPCommandButtonDescriptor button)
        {
            try
            {
                if (button.IconSoruceArray != null)
                {
                    BitmapImage biImg = new BitmapImage();

                    using (MemoryStream ms = new MemoryStream(button.IconSoruceArray))
                    {
                        biImg.BeginInit();
                        biImg.StreamSource = ms;
                        biImg.CacheOption = BitmapCacheOption.OnLoad;
                        biImg.EndInit();
                        biImg.StreamSource = null;
                        button.IconSource = biImg as ImageSource;
                        button.IconSoruceArray = null;
                    }
                }

                if (button.MiniIconSoruceArray != null)
                {
                    BitmapImage biImg = new BitmapImage();

                    using (MemoryStream ms = new MemoryStream(button.MiniIconSoruceArray))
                    {
                        biImg.BeginInit();
                        biImg.StreamSource = ms;
                        biImg.CacheOption = BitmapCacheOption.OnLoad;
                        biImg.EndInit();
                        biImg.StreamSource = null;
                        button.MiniIconSource = biImg as ImageSource;
                        button.MiniIconSoruceArray = null;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task DutAddbyMouseDown()
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public abstract class PMIPNPSetupBase : PNPSetupBase
    {

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                if (parameter == null)
                {
                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                                                                    "Information",
                                                                    "The PMI setup has been changed."
                                                                    + Environment.NewLine + "Do you want to save chages?"
                                                                    + Environment.NewLine + "OK         : Save and exit"
                                                                    + Environment.NewLine + "Cancel     : Exit without save",
                                                                    EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        return EventCodeEnum.SAVE_PARAM;
                    }
                    else if (ret == EnumMessageDialogResult.NEGATIVE)
                    {
                        return EventCodeEnum.NONE;
                    }
                }
                else
                {
                    return EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return EventCodeEnum.UNDEFINED;
            }
            return EventCodeEnum.UNDEFINED;
        }
    }

    public class LoaderPNPSetupBase : CategoryNodeSetupBase
    {
        public LoaderPNPSetupBase()
        {

        }
        public LoaderPNPSetupBase(string header)
        {
            Header = header;
        }
        public LoaderPNPSetupBase(ICategoryNodeItem parent, string header)
        {
            Parent = parent;
            Header = header;
        }
        public override bool Initialized { get; set; }

        public override Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            throw new NotImplementedException();
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public override void SetStepSetupState(string header = null)
        {
            throw new NotImplementedException();
        }
    }
}

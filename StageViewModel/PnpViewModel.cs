using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnpViewModelModule
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.Communication;
    using LogModule;
    using PnPControl;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using RelayCommandBase;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using LoaderMapView;

    using LoaderBase.FactoryModules.ViewModelModule;
    using CameraModule;
    using VisionParams.Camera;
    using ServiceProxy;
    using DllImporter;
    using System.Reflection;
    using MahApps.Metro.Controls.Dialogs;
    using PMISetup.UC;
    using ProberInterfaces.PMI;
    using UCNeedleClean;
    using NeedleCleanSequencePageView;

    public class PnpViewModel : PNPSetupBase, IHasPMITemplateMiniViewModel
    {
        protected Autofac.IContainer _Container { get; set; }
        public ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return _Container.Resolve<ILoaderCommunicationManager>();
            }
        }
        public new IVisionManager VisionManager
        {
            get { return _Container.Resolve<IVisionManagerProxy>(); }
        }
        private ILoaderViewModelManager LoaderViewModelManager => _Container.Resolve<ILoaderViewModelManager>();

        private StageObject Stage
        {
            get { return (StageObject)LoaderCommunicationManager.SelectedStage; }
        }

        private IRemoteMediumProxy ViewModelProxy //stagedataservice
        {
            get { return LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(); }
        }

        private StageSupervisorProxy StageProxy
        {
            get { return (StageSupervisorProxy)LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(); }
        }

        public override Guid ScreenGUID { get; }

        public PnpViewModel()
        {

        }
        public PnpViewModel(IContainer container)
        {
            _Container = container;
            Init();
            InitData();
            //PnpManager.GetCategoryNameList("WaferAlign", "IWaferAligner", new Guid("a05a34bf-e63f-41ee-9819-285274faef1a"));
        }

        public PnpViewModel(IContainer container, ICategoryNodeItem parent, string header)
        {
            _Container = container;
            Init();
            InitData();
            Parent = parent;
            Header = header;
        }

        public PnpViewModel(IContainer container, string header)
        {
            _Container = container;
            Init();
            InitData();
            Header = header;
        }

        private UcNeedleCleanMainPage _NCMainPageView;
        private UcNeedleCleanSequencePage _NCSequencePageView;
        private TemplateMiniView _PMITemplateMiniView;

        private PMITemplateMiniViewModel _PMITemplateMiniViewModel;
        public PMITemplateMiniViewModel PMITemplateMiniViewModel
        {
            get { return _PMITemplateMiniViewModel; }
            set
            {
                if (value != _PMITemplateMiniViewModel)
                {
                    RaisePropertyChanged();
                }
            }
        }


        public IPMIInfo PMIInfo
        {
            get { return this.GetParam_Wafer().GetSubsInfo().GetPMIInfo(); }
        }

        private double _TargetRectangleLeft;
        public override double TargetRectangleLeft
        {
            get { return _TargetRectangleLeft; }
            set
            {
                if (value != _TargetRectangleLeft)
                {
                    _TargetRectangleLeft = value;
                    RaisePropertyChanged();
                    if (ViewModelProxy != null & UseUserControl == UserControlFucEnum.PTRECT)
                        ViewModelProxy.SetDislayPortTargetRectInfo(_TargetRectangleLeft, _TargetRectangleTop);
                }
            }
        }

        private double _TargetRectangleTop;
        public override double TargetRectangleTop
        {
            get { return _TargetRectangleTop; }
            set
            {
                if (value != _TargetRectangleTop)
                {
                    _TargetRectangleTop = value;
                    RaisePropertyChanged();
                    if (ViewModelProxy != null & UseUserControl == UserControlFucEnum.PTRECT)
                        ViewModelProxy.SetDislayPortTargetRectInfo(_TargetRectangleLeft, _TargetRectangleTop);
                }
            }
        }

        private Visibility _VisibilityZoomIn;
        public Visibility VisibilityZoomIn
        {
            get { return _VisibilityZoomIn; }
            set
            {
                if (value != _VisibilityZoomIn)
                {
                    _VisibilityZoomIn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityZoomOut;
        public Visibility VisibilityZoomOut
        {
            get { return _VisibilityZoomOut; }
            set
            {
                if (value != _VisibilityZoomOut)
                {
                    _VisibilityZoomOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityMoveToCenter;
        public Visibility VisibilityMoveToCenter
        {
            get { return _VisibilityMoveToCenter; }
            set
            {
                if (value != _VisibilityMoveToCenter)
                {
                    _VisibilityMoveToCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region //..Init
        public void Init()
        {
            try
            {
                StageSupervisor = _Container.Resolve<IStageSupervisor>();
                PnpManager = _Container.Resolve<IPnpManager>();
                CoordManager = _Container.Resolve<ICoordinateManager>();
                LoaderCommManager = LoaderCommunicationManager;
                SetupState = new NotCompletedState(this);
                MotionManager = _Container.Resolve<IMotionManager>();

                ShowPad = true;
                ShowPin = false;
                EnableDragMap = true;
                ShowSelectedDut = false;
                ShowGrid = false;
                ShowCurrentPos = true;
                CurXPos = 0;
                CurYPos = 0;
                ZoomLevel = 11;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void InitData()
        {


        }

        public void InitLoaderPNPViewModel()
        {
            if (SetupState == null)
                SetupState = new NotCompletedState(this);
            if (SetupState._Module == null)
                SetupState._Module = this;


            if (DisplayPort == null)
            {

                ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;

                TargetRectangleWidth = 0;
                TargetRectangleHeight = 0;
                //LoaderViewModelManager.RegisteDisplayPort(DisplayPort);
            }
            BindingPNPSetup();
        }

        public override EventCodeEnum InitLightJog(IUseLightJog module, EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (PnpManager != null)
                {
                    if (PnpManager.PnpLightJog != null)
                    {
                        PnpManager.PnpLightJog.InitCameraJog(module, camtype);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum UpdateCameraLightValue()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (PnpManager != null)
                {
                    if (PnpManager.PnpLightJog != null)
                    {
                        PnpManager.PnpLightJog.UpdateCameraLightValue();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region //..Method



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



        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                await PnpManager.StepPageSwitching(this.Header, parameter);
                await InitStageData();
                EnableUseBtn();
                this.VisionManager().SetDisplayChannel(null, DisplayPort);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void UpdatePMITemplateMiniViewModel()
        {
            try
            {
                _PMITemplateMiniViewModel = ViewModelProxy.GetPMITemplateMiniViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PNPCommandButtonDescriptor GetPNPCommandButtonDescriptor(PNPCommandButtonType type)
        {
            PNPCommandButtonDescriptor retval = null;

            try
            {
                switch (type)
                {
                    case PNPCommandButtonType.PADJOGLEFT:
                        retval = PadJogLeft;
                        break;
                    case PNPCommandButtonType.PADJOGRIGHT:
                        retval = PadJogRight;
                        break;
                    case PNPCommandButtonType.PADJOGUP:
                        retval = PadJogUp;
                        break;
                    case PNPCommandButtonType.PADJOGDOWN:
                        retval = PadJogDown;
                        break;
                    case PNPCommandButtonType.PADJOGSELECT:
                        retval = PadJogSelect;
                        break;
                    case PNPCommandButtonType.PADJOGLEFTUP:
                        retval = PadJogLeftUp;
                        break;
                    case PNPCommandButtonType.PADJOGRIGHTUP:
                        retval = PadJogRightUp;
                        break;
                    case PNPCommandButtonType.PADJOGLEFTDOWN:
                        retval = PadJogLeftDown;
                        break;
                    case PNPCommandButtonType.PADJOGRIGHTDOWN:
                        retval = PadJogRightDown;
                        break;
                    case PNPCommandButtonType.ONEBUTTON:
                        retval = OneButton;
                        break;
                    case PNPCommandButtonType.TWOBUTTON:
                        retval = TwoButton;
                        break;
                    case PNPCommandButtonType.THREEBUTTON:
                        retval = ThreeButton;
                        break;
                    case PNPCommandButtonType.FOURBUTTON:
                        retval = FourButton;
                        break;
                    case PNPCommandButtonType.FIVEBUTTON:
                        retval = FiveButton;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public PNPCommandButtonDescriptor GetPNPCommandButtonDescriptorInDescription(PNPDataDescription pNPDataDescriptor, PNPCommandButtonType type)
        {
            PNPCommandButtonDescriptor retval = null;

            try
            {
                if (pNPDataDescriptor != null)
                {
                    switch (type)
                    {
                        case PNPCommandButtonType.PADJOGLEFT:
                            retval = pNPDataDescriptor.PadJogLeft;
                            break;
                        case PNPCommandButtonType.PADJOGRIGHT:
                            retval = pNPDataDescriptor.PadJogRight;
                            break;
                        case PNPCommandButtonType.PADJOGUP:
                            retval = pNPDataDescriptor.PadJogUp;
                            break;
                        case PNPCommandButtonType.PADJOGDOWN:
                            retval = pNPDataDescriptor.PadJogDown;
                            break;
                        case PNPCommandButtonType.PADJOGSELECT:
                            retval = pNPDataDescriptor.PadJogSelect;
                            break;
                        case PNPCommandButtonType.PADJOGLEFTUP:
                            retval = pNPDataDescriptor.PadJogLeftUp;
                            break;
                        case PNPCommandButtonType.PADJOGRIGHTUP:
                            retval = pNPDataDescriptor.PadJogRightUp;
                            break;
                        case PNPCommandButtonType.PADJOGLEFTDOWN:
                            retval = pNPDataDescriptor.PadJogLeftDown;
                            break;
                        case PNPCommandButtonType.PADJOGRIGHTDOWN:
                            retval = pNPDataDescriptor.PadJogRightDown;
                            break;
                        case PNPCommandButtonType.ONEBUTTON:
                            retval = pNPDataDescriptor.OneButton;
                            break;
                        case PNPCommandButtonType.TWOBUTTON:
                            retval = pNPDataDescriptor.TwoButton;
                            break;
                        case PNPCommandButtonType.THREEBUTTON:
                            retval = pNPDataDescriptor.ThreeButton;
                            break;
                        case PNPCommandButtonType.FOURBUTTON:
                            retval = pNPDataDescriptor.FourButton;
                            break;
                        case PNPCommandButtonType.FIVEBUTTON:
                            retval = pNPDataDescriptor.FiveButton;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void SetButtonCommand(PNPDataDescription pNPDataDescriptor, PNPCommandButtonType commandType, bool checkRepeat = false, bool sourcecopy = false, bool clear = false)
        {
            try
            {
                var TargetButton = GetPNPCommandButtonDescriptor(commandType);

                if (sourcecopy)
                {
                    var sourceButton = GetPNPCommandButtonDescriptorInDescription(pNPDataDescriptor, commandType);
                    sourceButton.CopyTo(TargetButton);
                }

                ConverterImageSource(TargetButton);

                if (TargetButton.IsEnabled && TargetButton.Command == null)
                {
                    if (checkRepeat)
                    {
                        if (!TargetButton.RepeatEnable)
                        {
                            TargetButton.Command = new AsyncCommand<object>(param => ButtonClickAsyncCommand(param, TargetButton.PNPButtonType));
                        }
                        else
                        {
                            TargetButton.Command = new RelayCommand<object>(param => ButtonClickSyncCommand(param, TargetButton.PNPButtonType));
                        }
                    }
                    else
                    {
                        TargetButton.Command = new AsyncCommand<object>(param => ButtonClickAsyncCommand(param, TargetButton.PNPButtonType));
                    }
                }
                else
                {
                    if (clear)
                    {
                        TargetButton.IconSource = null;
                        TargetButton.IconCaption = null;
                        TargetButton.IsEnabled = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void WaferObjectInfoNonSerializeUpdated(WaferObjectInfoNonSerialized waferobjinfo)
        {

            try
            {
                this.StageSupervisor().WaferObject.MapViewControlMode = waferobjinfo?.MapViewRenderMode ?? MapViewMode.UNDIFIND;
                this.StageSupervisor().WaferObject.MapViewStageSyncEnable = waferobjinfo.MapViewStageSyncEnable;
                this.StageSupervisor().WaferObject.MapViewCurIndexVisiablity = waferobjinfo.MapViewCurIndexVisiablity;

                this.StageSupervisor().WaferObject.TopLeftToBottomRightLineVisible = waferobjinfo.TopLeftToBottomRightLineVisible;
                this.StageSupervisor().WaferObject.BottomLeftToTopRightLineVisible = waferobjinfo.BottomLeftToTopRightLineVisible;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> InitStageData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            PNPDataDescription pNPDataDescriptor = null;
            WaferObjectInfoNonSerialized waferobjinfo = null;

            Task task = new Task(() =>
            {
                pNPDataDescriptor = ViewModelProxy.GetPNPDataDescriptor();

                waferobjinfo = this.StageSupervisor().GetWaferObjectInfoNonSerialize();

                CurCam.Param = new CameraParameter(ViewModelProxy.GetCamType());

                if (pNPDataDescriptor == null)
                    return;
                InitLightJog(this, CurCam.GetChannelType());
                //MainViewTarget = DisplayPort;                   
            });
            task.Start();
            await task;

            Application.Current.Dispatcher.Invoke(() =>
            {
                #region //..Button

                if (pNPDataDescriptor != null)
                {
                    OneButton = pNPDataDescriptor.OneButton;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.ONEBUTTON, true);

                    TwoButton = pNPDataDescriptor.TwoButton;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.TWOBUTTON, true);

                    ThreeButton = pNPDataDescriptor.ThreeButton;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.THREEBUTTON, true);

                    FourButton = pNPDataDescriptor.FourButton;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.FOURBUTTON, true);

                    FiveButton = pNPDataDescriptor.FiveButton;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.FIVEBUTTON, true, false, true);

                    PadJogUp = pNPDataDescriptor.PadJogUp;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGUP, true);

                    PadJogDown = pNPDataDescriptor.PadJogDown;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGDOWN, true);

                    PadJogLeft = pNPDataDescriptor.PadJogLeft;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGLEFT, true);

                    PadJogLeftDown = pNPDataDescriptor.PadJogLeftDown;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGLEFTDOWN, true);

                    PadJogLeftUp = pNPDataDescriptor.PadJogLeftUp;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGLEFTUP, true);

                    PadJogRight = pNPDataDescriptor.PadJogRight;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGRIGHT, true);

                    PadJogRightDown = pNPDataDescriptor.PadJogRightDown;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGRIGHTDOWN, true);

                    PadJogRightUp = pNPDataDescriptor.PadJogRightUp;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGRIGHTUP, true);

                    PadJogSelect = pNPDataDescriptor.PadJogSelect;
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGSELECT, true);
                }

                #endregion

                #region //..UI

                if (!Initialized)
                {
                    CurCam.InitModule();
                    Initialized = true;
                }

                LoaderViewModelManager.Camera = CurCam;

                if (pNPDataDescriptor != null)
                {
                    UseUserControl = pNPDataDescriptor.UseUserControl;
                    TargetRectangleHeight = pNPDataDescriptor.TargetRectangleHeight;
                    TargetRectangleWidth = pNPDataDescriptor.TargetRectangleWidth;
                    StepLabel = pNPDataDescriptor.StepLabel;
                    StepSecondLabel = pNPDataDescriptor.StepSecondLabel;

                    JogType = pNPDataDescriptor.JogType;
                    DisplayClickToMoveEnalbe = pNPDataDescriptor.DisplayClickToMoveEnalbe;

                    if (pNPDataDescriptor.MainViewImageSource != null)
                    {
                        BitmapImage biImg = new BitmapImage();

                        using (MemoryStream ms = new MemoryStream(pNPDataDescriptor.MainViewImageSource))
                        {
                            biImg.BeginInit();
                            biImg.StreamSource = ms;
                            biImg.CacheOption = BitmapCacheOption.OnLoad;
                            biImg.EndInit();
                            biImg.StreamSource = null;
                            this.MainViewImageSource = biImg as BitmapImage;
                        }
                    }

                    SetNodeSetupState(pNPDataDescriptor.SetupState);

                    if (pNPDataDescriptor.IsExistAdvenceSetting)
                        AdvanceSetupView = new CutomDialogViewModel();

                    //WaferObject = ((StageObject)(LoaderCommunicationManager.SelectedStage)).StageInfo.WaferObject;
                    //NC = StageSupervisor.NCObject;

                    //StageSupervisor.WaferObject = WaferObject;
                    switch (pNPDataDescriptor.MainViewTarget)
                    {
                        case "UcDisplayPort.DisplayPort":
                            MainViewTarget = DisplayPort;
                            break;
                        case "SubstrateObjects.WaferObject":
                            MainViewTarget = WaferObject;
                            break;
                        case "ProbeCardObject.ProbeCard":
                            MainViewTarget = ProbeCard;
                            break;
                        case "UCNeedleClean.UcNeedleCleanMainPage":
                            if (_NCMainPageView == null)
                                _NCMainPageView = new UcNeedleCleanMainPage();
                            this.StageSupervisor().NCObject = StageProxy.GetNCObject();
                            NC.InitCleanPadRender();
                            MainViewTarget = _NCMainPageView;
                            break;
                        case "NeedleCleanSequencePageView.UcNeedleCleanSequencePage":
                            if (_NCSequencePageView == null)
                                _NCSequencePageView = new UcNeedleCleanSequencePage();
                            this.StageSupervisor().NCObject = StageProxy.GetNCObject();
                            NC.InitNCSequenceRender();
                            MainViewTarget = _NCSequencePageView;
                            break;
                        default:
                            break;
                    }
                    switch (pNPDataDescriptor.MiniViewTarget)
                    {
                        case "UcDisplayPort.DisplayPort":
                            MiniViewTarget = DisplayPort;
                            break;
                        case "SubstrateObjects.WaferObject":
                            MiniViewTarget = WaferObject;
                            break;
                        case "ProbeCardObject.ProbeCard":
                            MiniViewTarget = ProbeCard;
                            break;
                        case "ProberInterfaces.ImageBuffer":
                           MiniViewTarget = ImgBuffer;
                            break;
                        case "UCNeedleClean.UcNeedleCleanMainPage":
                            if (_NCMainPageView == null)
                                _NCMainPageView = new UcNeedleCleanMainPage();
                            this.StageSupervisor().NCObject = StageProxy.GetNCObject();
                            NC.InitCleanPadRender();
                            MiniViewTarget = _NCMainPageView;
                            break;
                        case "NeedleCleanSequencePageView.UcNeedleCleanSequencePage":
                            if (_NCSequencePageView == null)
                                _NCSequencePageView = new UcNeedleCleanSequencePage();
                            this.StageSupervisor().NCObject = StageProxy.GetNCObject();
                            NC.InitNCSequenceRender();
                            MiniViewTarget = _NCSequencePageView;
                            break;
                        case "PMISetup.UC.TemplateMiniView":
                            if (_PMITemplateMiniView == null)
                                _PMITemplateMiniView = new TemplateMiniView();

                            if (_PMITemplateMiniViewModel == null)
                            {
                                _PMITemplateMiniViewModel = new PMITemplateMiniViewModel();
                            }

                            UpdatePMITemplateMiniViewModel();

                            _PMITemplateMiniView.DataContext = _PMITemplateMiniViewModel;

                            MiniViewTarget = _PMITemplateMiniView;
                            break;
                        default:
                            break;
                    }


                    MiniViewHorizontalAlignment = pNPDataDescriptor.MiniViewHorizontalAlignment;
                    MiniViewVerticalAlignment = pNPDataDescriptor.MiniViewVerticalAlignment;

                    WaferObject.SetCurrentMIndex(pNPDataDescriptor.MapXIndex, pNPDataDescriptor.MapYIndex);

                    if (pNPDataDescriptor.AdvanceSetupViewModuleInfo != null & pNPDataDescriptor.AdvanceSetupViewModelModuleInfo != null)
                    {
                        if (this.MetroDialogManager().IsShowExistAdvance(pNPDataDescriptor.AdvanceSetupViewModuleInfo.ClassName.First(),
                                 pNPDataDescriptor.AdvanceSetupViewModelModuleInfo.ClassName.First()) == false)
                        {
                            DllImporter DLLImporter = new DllImporter();
                            Tuple<bool, Assembly> viewmodule = DLLImporter.LoadDLL(pNPDataDescriptor.AdvanceSetupViewModuleInfo);
                            if (viewmodule != null)
                            {
                                var view = DLLImporter.Assignable<CustomDialog>(viewmodule.Item2);
                                foreach (var module in view)
                                {
                                    string name = module.GetType().Name;
                                    if (pNPDataDescriptor.AdvanceSetupViewModuleInfo.ClassName.SingleOrDefault(modulename => modulename.Equals(name)) != null)
                                    {
                                        AdvanceSetupView = module;
                                        break;
                                    }
                                }

                                if (AdvanceSetupViewModel == null)
                                {
                                    var viewmodel = DLLImporter.Assignable<IPnpAdvanceSetupViewModel>(viewmodule.Item2);
                                    foreach (var module in viewmodel)
                                    {
                                        string name = module.GetType().Name;

                                        if (pNPDataDescriptor.AdvanceSetupViewModelModuleInfo.ClassName.SingleOrDefault(modulename => modulename.Equals(name)) != null)
                                        {
                                            AdvanceSetupViewModel = module;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (pNPDataDescriptor.AdvanceSetupViewModelModuleInfo.ClassName.SingleOrDefault(modulename => modulename.Equals(AdvanceSetupViewModel.GetType().Name)) == null)
                                    {
                                        var viewmodel = DLLImporter.Assignable<IPnpAdvanceSetupViewModel>(viewmodule.Item2);
                                        foreach (var module in viewmodel)
                                        {
                                            string name = module.GetType().Name;

                                            if (pNPDataDescriptor.AdvanceSetupViewModelModuleInfo.ClassName.SingleOrDefault(modulename => modulename.Equals(name)) != null)
                                            {
                                                AdvanceSetupViewModel = module;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (pNPDataDescriptor.Params != null)
                                {
                                    foreach (var param in pNPDataDescriptor.Params)
                                    {
                                        //if (PackagableParams == null)
                                        //    PackagableParams = new List<byte[]>();
                                        //object target;
                                        //SerializeManager.DeserializeFromByte(param, out target, typeof(object));
                                        //PackagableParams.Add(target);
                                    }
                                }

                                PackagableParams = pNPDataDescriptor.Params;
                                if (AdvanceSetupView != null & AdvanceSetupViewModel != null)
                                    AdvanceSetupView.DataContext = AdvanceSetupViewModel;
                            }
                        }
                        else
                        {
                            this.MetroDialogManager().SetAdvanceDialogToPnpStep();
                        }
                    }

                    #endregion

                    #region SideViewer

                    SideViewDisplayMode = pNPDataDescriptor.SideViewDisplayMode;
                    SideViewTargetVisibility = pNPDataDescriptor.SideViewTargetVisibility;
                    SideViewSwitchVisibility = pNPDataDescriptor.SideViewSwitchVisibility;
                    SideViewExpanderVisibility = pNPDataDescriptor.SideViewExpanderVisibility;
                    SideViewTextVisibility = pNPDataDescriptor.SideViewTextVisibility;
                    SideViewVerticalAlignment = pNPDataDescriptor.SideViewVerticalAlignment;
                    SideViewHorizontalAlignment = pNPDataDescriptor.SideViewHorizontalAlignment;
                    SideViewWidth = pNPDataDescriptor.SideViewWidth;
                    SideViewHeight = pNPDataDescriptor.SideViewHeight;
                    SideViewMargin = pNPDataDescriptor.SideViewMargin;
                    SideViewTitle = pNPDataDescriptor.SideViewTitle;
                    SideViewTitleFontSize = pNPDataDescriptor.SideViewTitleFontSize;

                    BrushConverter converter = new BrushConverter();

                    if (pNPDataDescriptor.SideViewTitleFontColorString != null)
                    {
                        SideViewTitleFontColor = converter.ConvertFromString(pNPDataDescriptor.SideViewTitleFontColorString) as Brush;
                    }

                    if (pNPDataDescriptor.SideViewTitleBackgroundString != null)
                    {
                        SideViewTitleBackground = converter.ConvertFromString(pNPDataDescriptor.SideViewTitleBackgroundString) as Brush;
                    }

                    SideViewTextBlocks = pNPDataDescriptor.SideViewTextBlocks;

                    foreach (var textblock in SideViewTextBlocks)
                    {
                        if (textblock.SideTextFontColorString != null)
                        {
                            textblock.SideTextFontColor = converter.ConvertFromString(textblock.SideTextFontColorString) as Brush;
                        }

                        if (textblock.SideTextBackgroundString != null)
                        {
                            textblock.SideTextBackground = converter.ConvertFromString(textblock.SideTextBackgroundString) as Brush;
                        }
                    }
                    #endregion

                    #region //..Expander

                    #endregion

                    if (pNPDataDescriptor.UseRender)
                    {
                        SharpDXLayer = new RenderLayerExternal(new Size(pNPDataDescriptor.RenderWidth, pNPDataDescriptor.RenderHeight));
                        (SharpDXLayer as RenderLayerExternal).SetContainer(_Container);
                    }
                }

                WaferObjectInfoNonSerializeUpdated(waferobjinfo);
                

                AxisXPos = this.MotionManager().GetAxis(EnumAxisConstants.X);
                AxisYPos = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                AxisZPos = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                AxisTPos = this.MotionManager().GetAxis(EnumAxisConstants.C);
                AxisPZPos = this.MotionManager().GetAxis(EnumAxisConstants.PZ);

                ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
            });
            return retVal;
        }

        public override void SetPNPDataDescriptor(PNPDataDescription pNPDataDescriptor)
        {
            try
            {
                if (pNPDataDescriptor == null)
                {
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    #region //..Button

                    // TODO : InitStageData에서는 RepeatEnable을 모두 확인하고 있는데...?

                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.ONEBUTTON, sourcecopy:true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.TWOBUTTON, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.THREEBUTTON, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.FOURBUTTON, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.FIVEBUTTON, sourcecopy: true);

                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGUP, true, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGDOWN, true, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGLEFT, true, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGLEFTDOWN, true, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGLEFTUP, true, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGRIGHT, true, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGRIGHTDOWN, true, sourcecopy: true);
                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGRIGHTUP, true, sourcecopy: true);

                    SetButtonCommand(pNPDataDescriptor, PNPCommandButtonType.PADJOGSELECT, sourcecopy: true);

                    #endregion

                    #region SideViewer

                    SideViewDisplayMode = pNPDataDescriptor.SideViewDisplayMode;
                    SideViewTargetVisibility = pNPDataDescriptor.SideViewTargetVisibility;
                    SideViewSwitchVisibility = pNPDataDescriptor.SideViewSwitchVisibility;
                    SideViewExpanderVisibility = pNPDataDescriptor.SideViewExpanderVisibility;
                    SideViewTextVisibility = pNPDataDescriptor.SideViewTextVisibility;
                    SideViewVerticalAlignment = pNPDataDescriptor.SideViewVerticalAlignment;
                    SideViewHorizontalAlignment = pNPDataDescriptor.SideViewHorizontalAlignment;
                    SideViewWidth = pNPDataDescriptor.SideViewWidth;
                    SideViewHeight = pNPDataDescriptor.SideViewHeight;
                    SideViewMargin = pNPDataDescriptor.SideViewMargin;
                    SideViewTitle = pNPDataDescriptor.SideViewTitle;
                    SideViewTitleFontSize = pNPDataDescriptor.SideViewTitleFontSize;

                    BrushConverter converter = new BrushConverter();

                    if (pNPDataDescriptor.SideViewTitleFontColorString != null)
                    {
                        SideViewTitleFontColor = converter.ConvertFromString(pNPDataDescriptor.SideViewTitleFontColorString) as Brush;
                    }

                    if (pNPDataDescriptor.SideViewTitleBackgroundString != null)
                    {
                        SideViewTitleBackground = converter.ConvertFromString(pNPDataDescriptor.SideViewTitleBackgroundString) as Brush;
                    }

                    SideViewTextBlocks = pNPDataDescriptor.SideViewTextBlocks;

                    foreach (var textblock in SideViewTextBlocks)
                    {
                        if (textblock.SideTextFontColorString != null)
                        {
                            textblock.SideTextFontColor = converter.ConvertFromString(textblock.SideTextFontColorString) as Brush;
                        }

                        if (textblock.SideTextBackgroundString != null)
                        {
                            textblock.SideTextBackground = converter.ConvertFromString(textblock.SideTextBackgroundString) as Brush;
                        }
                    }
                    #endregion

                    #region //..UI

                    UseUserControl = pNPDataDescriptor.UseUserControl;
                    TargetRectangleHeight = pNPDataDescriptor.TargetRectangleHeight;
                    TargetRectangleWidth = pNPDataDescriptor.TargetRectangleWidth;
                    StepLabel = pNPDataDescriptor.StepLabel;
                    StepSecondLabel = pNPDataDescriptor.StepSecondLabel;

                    JogType = pNPDataDescriptor.JogType;
                    DisplayClickToMoveEnalbe = pNPDataDescriptor.DisplayClickToMoveEnalbe;

                    if (pNPDataDescriptor.MainViewImageSource != null)
                    {
                        BitmapImage biImg = new BitmapImage();

                        using (MemoryStream ms = new MemoryStream(pNPDataDescriptor.MainViewImageSource))
                        {
                            biImg.BeginInit();
                            biImg.StreamSource = ms;
                            biImg.CacheOption = BitmapCacheOption.OnLoad;
                            biImg.EndInit();
                            biImg.StreamSource = null;
                            this.MainViewImageSource = biImg as BitmapImage;
                        }
                    }

                    MiniViewHorizontalAlignment = pNPDataDescriptor.MiniViewHorizontalAlignment;
                    MiniViewVerticalAlignment = pNPDataDescriptor.MiniViewVerticalAlignment;

                    #endregion

                    #region //..Expander

                    #endregion

                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (parameter != null)
                {
                    if (parameter is ICategoryNodeItem)
                        parameter = (parameter as ICategoryNodeItem).Header;
                }

                retVal = await PnpManager.StepCleanup(this.Header, parameter);
                this.StageSupervisor().WaferObject.MapViewControlMode = MapViewMode.MapMode;

                if (SharpDXLayer != null)
                {
                    (SharpDXLayer as RenderLayerExternal).RenderContainers.Clear();
                    (SharpDXLayer as RenderLayerExternal).DeInit();
                }
                //this.StageSupervisor().WaferObject.MapViewStageSyncEnable = true;
                //this.StageSupervisor().WaferObject.MapViewCurIndexVisiablity = true;

                ///화면 나갈땐 Stage 에게 MapIndex 업데이트 무조건 해제.
                //if (this.StageSupervisor().WaferObject.ChangeMapIndexDelegate != null)
                //    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate -= UpdateMapIndex;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = PnpManager.StepParamValidation(this.Header);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            return PnpManager.StepIsParameterChanged(this.Header, issave);
        }


        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CurCam = new Camera();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InitLoaderPNPViewModel();
                });
                //Initialized = true;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        #endregion

        #region //..Command

        //private async Task ButtonClickAsyncCommand(object param)
        private async Task ButtonClickAsyncCommand(object param, PNPCommandButtonType type)
        {
            try
            {
                if (ViewModelProxy == null)
                {
                    return;
                }

                if (type == PNPCommandButtonType.FIVEBUTTON && AdvanceSetupView != null)
                {
                    await ShowAdvanceSetupView();
                }
                else
                {
                    await ViewModelProxy.ButtonExecuteAsync(param, type);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void ButtonClickSyncCommand(object param, PNPCommandButtonType type)
        {
            try
            {
                if (type == PNPCommandButtonType.FIVEBUTTON)
                {
                    if (AdvanceSetupView != null)
                    {
                        await ShowAdvanceSetupView();
                    }
                    else
                    {
                        if (ViewModelProxy != null)
                        {
                            ViewModelProxy.ButtonExecuteSync(param, type);
                        }
                    }
                }
                else
                {
                    if (ViewModelProxy != null)
                    {
                        ViewModelProxy.ButtonExecuteSync(param, type);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public override void PatternCloseFunc(object parameter)
        {
            MiniViewTarget = PrevMiniViewTarget;
            ViewModelProxy.SetMiniViewTarget(null);
        }
        public override void SetStepSetupState(string header = null)
        {
            try
            {
                if (ViewModelProxy != null)
                {
                    ViewModelProxy.SetSetupState(header);
                    SetNodeSetupState(ViewModelProxy.GetSetupState(header));

                    if (this is LoaderCategoryForm)
                    {
                        foreach (var module in Categories)
                        {
                            if (module is ICategoryNodeItem)
                            {
                                ICategoryNodeItem item = module as ICategoryNodeItem;
                                item.SetNodeSetupState(ViewModelProxy.GetSetupState(item.Header));
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

        public override void UpdateLabel()
        {
            return;
        }

        public override void SetPackagableParams()
        {
            try
            {
                if (ViewModelProxy != null)
                {
                    ViewModelProxy.PNPSetPackagableParams();
                    PackagableParams = ViewModelProxy.PNPGetPackagableParams();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class LoaderCategoryForm : PnpViewModel, IPnpCategoryForm
    {
        public LoaderCategoryForm()
        {
            Init();
        }

        public LoaderCategoryForm(IContainer container, string header)
        {
            _Container = container;
            Init();
            InitData();
            Header = header;
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Categories.Count != 0)
                {
                    foreach (var module in Categories)
                    {
                        if (module is IHasDevParameterizable)
                            retVal = (module as IHasDevParameterizable).SaveDevParameter();
                        if (module is IHasSysParameterizable)
                            retVal = (module as IHasSysParameterizable).SaveSysParameter();
                        if (module is IPnpCategoryForm)
                        {
                            retVal = (module as IPnpCategoryForm).SaveParameter();
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

        public EventCodeEnum ValidationCategoryStep(object parameter)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //UNDEFINED = PASS
            //NONE = 현재 Form 내에 일치하는 파라미터 있음. Step이동 가능함
            //PNP_EXCEPTION = 이동 못함.
            try
            {
                foreach (var module in Categories)
                {
                    if (module == parameter)
                    {
                        retVal = EventCodeEnum.NONE;
                        break;
                    }
                    else
                    {
                        if (module is ICategoryNodeItem)
                        {
                            if ((module as ICategoryNodeItem).StateSetup == EnumMoudleSetupState.NONE
                              || (module as ICategoryNodeItem).StateSetup == EnumMoudleSetupState.COMPLETE)
                                continue;
                            else
                            {
                                retVal = EventCodeEnum.PNP_EXCEPTION;
                                break;
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
    }
}

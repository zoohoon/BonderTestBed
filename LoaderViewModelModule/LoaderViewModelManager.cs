using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoaderViewModelModule
{
    using Autofac;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using ProberErrorCode;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Media.Media3D;
    using System.Windows.Controls;
    using LoaderViewModelModuleParameter;
    using System.Reflection;
    using Camera = CameraModule.Camera;
    using ProberInterfaces.Vision;
    using System.Windows.Media;
    using UcDisplayPort;
    using CUI;
    using ProberInterfaces.Param;
    using System.Windows.Input;
    using RelayCommandBase;
    using MaterialDesignThemes.Wpf;
    using ucDutViewer;
    using LoaderBase.FactoryModules.ServiceClient;
    using ViewModelModule;
    using System.Windows.Threading;
    using FilterPanelVM;

    public class LoaderViewModelManager : ILoaderViewModelManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Event

        #endregion

        #region //..Properties
        private Autofac.IContainer _Container { get; set; }
        public ILoaderCommunicationManager LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

        public const string ProberViewModelDll = "ProberViewModel.dll";

        //public IPnpManager PnpManager => _Container.Resolve<IPnpManager>();

        //private IMainScreenViewModel _MainViewModel;
        //public IMainScreenViewModel MainViewModel
        //{
        //    get { return _MainViewModel; }
        //    set
        //    {
        //        //if (value != _MainViewModel)
        //        //{
        //        _MainViewModel = value;
        //        RaisePropertyChanged();
        //        //}
        //    }
        //}

        //private ILoaderMainVM _LoaderMainMenuVM;
        //public ILoaderMainVM LoaderMainMenuVM
        //{
        //    get { return _LoaderMainMenuVM; }
        //    set
        //    {
        //        if (value != _LoaderMainMenuVM)
        //        {
        //            _LoaderMainMenuVM = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //            _MainMenuView = value;
        //            RaisePropertyChanged();

        public LoaderViewModelManager()
        {
            try
            {
                VMFilterPanel = new FilterPanelViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private IMainScreenView _MainMenuView;
        public IMainScreenView MainMenuView
        {
            get
            {
                return _MainMenuView;
            }
            set
            {
                if (value != _MainMenuView)
                {

                    _MainMenuView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenViewModel _CurrentVM;
        public IMainScreenViewModel CurrentVM
        {
            get
            {
                return _CurrentVM;
            }
            set
            {
                if (value != _CurrentVM)
                {

                    _CurrentVM = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private IMainMenuViewModel _MainMenuVM;
        //public IMainMenuViewModel MainMenuVM
        //{
        //    get
        //    {
        //        return _MainMenuVM;
        //    }
        //    set
        //    {
        //        if (value != _MainMenuVM)
        //        {

        //            _MainMenuVM = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}



        private Int32 _MainViewRowSpan = 1;
        public Int32 MainViewRowSpan
        {
            get { return _MainViewRowSpan; }
            set
            {
                if (value != _MainViewRowSpan)
                {
                    _MainViewRowSpan = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Int32 _MainViewRow;
        public Int32 MainViewRow
        {
            get { return _MainViewRow; }
            set
            {
                if (value != _MainViewRow)
                {
                    _MainViewRow = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _MainTopBarView;
        public IMainScreenView MainTopBarView
        {
            get
            {
                return _MainTopBarView;
            }
            set
            {
                if (value != _MainTopBarView)
                {

                    _MainTopBarView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _TabControlSelectedIndex;
        public int TabControlSelectedIndex
        {
            get { return _TabControlSelectedIndex; }
            set
            {
                if (value != _TabControlSelectedIndex)
                {
                    _TabControlSelectedIndex = value;
                    RaisePropertyChanged();

                    if (TabControlSelectedIndex == 0)
                    {
                        LogLevel = ProberLogLevel.PROLOG;
                    }
                    else if (TabControlSelectedIndex == 1)
                    {
                        LogLevel = ProberLogLevel.EVENT;

                        //LogViewModel?.EventMarkAllSet();
                        LoaderLogViewModel?.EventMarkAllSet();
                    }
                    else
                    {
                        LogLevel = ProberLogLevel.UNDEFINED;
                    }
                }
            }
        }


        private bool _IsPageSelected;
        public bool IsPageSelected
        {
            get { return _IsPageSelected; }
            set
            {
                if (value != _IsPageSelected)
                {
                    _IsPageSelected = value;
                    if (_IsPageSelected)
                    {
                        //Task.Run(() =>
                        //{
                        //    GetViewModel(0);
                        //});
                    }
                    RaisePropertyChanged();
                }
            }
        }



        //private LoaderTopBarViewModelModule.LoaderTopBarViewModel _MainTopBarVM;

        //public LoaderTopBarViewModelModule.LoaderTopBarViewModel MainTopBarVM
        //{
        //    get { return _MainTopBarVM; }
        //    set { _MainTopBarVM = value; }
        //}

        private StageListViewModelModule.StageListViewModel _StageListVM;

        public StageListViewModelModule.StageListViewModel StageListVM
        {
            get { return _StageListVM; }
            set { _StageListVM = value; }
        }

        private LuncherListViewModelModule.LuncherListViewModel _LuncherListVM;

        public LuncherListViewModelModule.LuncherListViewModel LuncherListVM
        {
            get { return _LuncherListVM; }
            set { _LuncherListVM = value; }
        }





        #endregion

        #region //.. Method
        public void SetContainer(Autofac.IContainer container)
        {
            _Container = container;
        }

        public IMainScreenViewModel GetViewModel(int cellindex)
        {
            IMainScreenViewModel viewModel = null;

            //GUID 
            try
            {
                //PnpManager.GetCategoryNameList("WaferAlign","IWaferAligner",new Guid("a05a34bf-e63f-41ee-9819-285274faef1a"));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return viewModel;
        }

        public EventCodeEnum SetDataContext(object obj)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        IMainScreenViewModel tmpViewModel = null;

                        ViewAndViewModelDictionary.TryGetValue(MainScreenView, out tmpViewModel);

                        if (tmpViewModel != null)
                        {
                            if (tmpViewModel is IPnpSetup || tmpViewModel is ICategoryNodeItem)
                            {
                                (MainScreenView as UserControl).DataContext = obj;
                                //MainViewModel = (IMainScreenViewModel)obj;
                            }
                        }
                        else
                        {

                        }
                    });

                    RetVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($err + "ViewModelManager - SetDataContext() : Error occured.");
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        #endregion

        #region //..Display

        #region //..DisplayPort

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


        private List<IDisplayPort> _DisplayPorts = new List<IDisplayPort>();
        public List<IDisplayPort> DisplayPorts
        {
            get { return _DisplayPorts; }
            set
            {
                if (value != _DisplayPorts)
                {
                    _DisplayPorts = value;
                    RaisePropertyChanged();
                }
            }
        }



        public void RegisteDisplayPort(IDisplayPort displayport)
        {
            if (DisplayPorts.Find(port => port == displayport) == null)
                DisplayPorts.Add(displayport);
        }

        public void RegisteDisplayPort(Type type, IDisplayPort displayport)
        {

        }

        private ICamera _Camera;
        public ICamera Camera
        {
            get { return _Camera; }
            set
            {
                if (value != _Camera)
                {
                    _Camera = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineCoordinate _GrabCoord = new MachineCoordinate();
        public MachineCoordinate GrabCoord
        {
            get { return _GrabCoord; }
            set
            {
                if (value != _GrabCoord)
                {
                    _PreGrabCoord = _GrabCoord;
                    _GrabCoord = value;
                }
            }
        }
        private MachineCoordinate _PreGrabCoord = new MachineCoordinate();
        public MachineCoordinate PreGrabCoord
        {
            get { return _PreGrabCoord; }
            set
            {
                if (value != _PreGrabCoord)
                {
                    _PreGrabCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumProberCam _PreCamType { get; set; }

        private EnumProberCam _CamType;

        public EnumProberCam CamType
        {
            get { return _CamType; }
            set
            {
                _PreCamType = _CamType;
                _CamType = value;
            }
        }

        private ILogVM _LoaderLogViewModel;
        public ILogVM LoaderLogViewModel
        {
            get { return _LoaderLogViewModel; }
            set
            {
                if (value != _LoaderLogViewModel)
                {
                    _LoaderLogViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        ProberInterfaces.Param.CatCoordinates precoord = new ProberInterfaces.Param.CatCoordinates();
        private ProberInterfaces.Param.CatCoordinates precatcoord = new ProberInterfaces.Param.CatCoordinates();

        public async void DispHostService_ImageUpdate(ImageBuffer image)
        {
            if (Camera.Param.ChannelType.Value == EnumProberCam.UNDEFINED)
            {
                Camera.Param.Band.Value = image.Band;
                Camera.Param.ColorDept.Value = image.ColorDept;
                Camera.Param.GrabSizeX.Value = image.SizeX;
                Camera.Param.GrabSizeY.Value = image.SizeY;
                Camera.Param.ChannelType.Value = image.CamType;
            }
            else
            {
                Camera.Param.Band.Value = image.Band;
                Camera.Param.ColorDept.Value = image.ColorDept;
                Camera.Param.GrabSizeX.Value = image.SizeX;
                Camera.Param.GrabSizeY.Value = image.SizeY;
                Camera.Param.ChannelType.Value = image.CamType;
                Camera.Param.RatioX.Value = image.RatioX.Value;
                Camera.Param.RatioY.Value = image.RatioY.Value;
                Camera.SetCamSystemPos(image.CatCoordinates);
                Camera.SetCamSystemUI(image.UserIdx);
                Camera.SetCamSystemMI(image.MachineIdx);
                Camera.CameraChannel.Type = Camera.Param.ChannelType.Value;
            }

            await (this.VisionManager() as IVisionManagerServiceClient).DispHostService_ImageUpdate(Camera, image);
        }

        #endregion

        #endregion

        #region //..IViewModelManager

        #region //..Property      
        public bool Initialized { get; set; } = false;

        public Guid LOGIN_PAGE_GUID = new Guid("28A11F12-8918-47FE-8161-3652F2EFEF29");

        private ObservableCollection<IMainScreenView> _PreMainScreenViewlist = new ObservableCollection<IMainScreenView>();
        public ObservableCollection<IMainScreenView> PreMainScreenViewList
        {
            get { return _PreMainScreenViewlist; }
            set
            {
                if (value != _PreMainScreenViewlist)
                {
                    _PreMainScreenViewlist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _MainScreenView;
        public IMainScreenView MainScreenView
        {
            get
            {
                return _MainScreenView;
            }
            set
            {
                if (value != _MainScreenView)
                {
                    _MainScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }



        private IMainScreenViewModel _MainScreenViewModel;
        public IMainScreenViewModel MainScreenViewModel
        {
            get { return _MainScreenViewModel; }
            set
            {
                if (value != _MainScreenViewModel)
                {
                    _MainScreenViewModel = value;
                    Type type = _MainScreenViewModel.GetType();
                    RaisePropertyChanged();
                }
            }
        }

        private Dictionary<Guid, IMainScreenView> _ScreenViewDictionary = new Dictionary<Guid, IMainScreenView>();
        public Dictionary<Guid, IMainScreenView> ScreenViewDictionary
        {
            get { return _ScreenViewDictionary; }
            set
            {
                if (value != _ScreenViewDictionary)
                {
                    _ScreenViewDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Dictionary<Guid, IMainScreenViewModel> _ScreenViewModelDictionary = new Dictionary<Guid, IMainScreenViewModel>();
        public Dictionary<Guid, IMainScreenViewModel> ScreenViewModelDictionary
        {
            get { return _ScreenViewModelDictionary; }
            set
            {
                if (value != _ScreenViewModelDictionary)
                {
                    _ScreenViewModelDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Dictionary<IMainScreenView, IMainScreenViewModel> _ViewAndViewModelDictionary
            = new Dictionary<IMainScreenView, IMainScreenViewModel>();
        public Dictionary<IMainScreenView, IMainScreenViewModel> ViewAndViewModelDictionary
        {
            get { return _ViewAndViewModelDictionary; }
            set
            {
                if (value != _ViewAndViewModelDictionary)
                {
                    _ViewAndViewModelDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Dictionary<string, Guid> _ViewGUIDDictionary = new Dictionary<string, Guid>();
        public Dictionary<string, Guid> ViewGUIDDictionary
        {
            get { return _ViewGUIDDictionary; }
            set
            {
                if (value != _ViewGUIDDictionary)
                {
                    _ViewGUIDDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }


        private List<LockControlInfo> _LockControls
             = new List<LockControlInfo>();

        public List<LockControlInfo> LockControls
        {
            get { return _LockControls; }
            set { _LockControls = value; }
        }


        private LoaderViewDLLParam _ViewParam;
        public LoaderViewDLLParam ViewParam
        {
            get { return _ViewParam; }
            set
            {
                if (value != _ViewParam)
                {
                    _ViewParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LoaderViewModelDLLParam _ViewModelParam;
        public LoaderViewModelDLLParam ViewModelParam
        {
            get { return _ViewModelParam; }
            set
            {
                if (value != _ViewModelParam)
                {
                    _ViewModelParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private LoaderViewAndViewModelConnectParam _ViewConnectParam;
        public LoaderViewAndViewModelConnectParam ViewConnectParam
        {
            get { return _ViewConnectParam; }
            set
            {
                if (value != _ViewConnectParam)
                {
                    _ViewConnectParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private LoaderViewGUIDAndViewModelConnectParam _ViewGUIDAndVMTypeParam;
        //public LoaderViewGUIDAndViewModelConnectParam ViewGUIDAndVMTypeParam
        //{
        //    get { return _ViewGUIDAndVMTypeParam; }
        //    set
        //    {
        //        if (value != _ViewGUIDAndVMTypeParam)
        //        {
        //            _ViewGUIDAndVMTypeParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private SettingViewParam _SettingViewParameter;
        public SettingViewParam SettingViewParameter
        {
            get { return _SettingViewParameter; }
            set
            {
                if (value != _SettingViewParameter)
                {
                    _SettingViewParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMapViewControl _MapViewControl;
        public IMapViewControl MapViewControl
        {
            get { return _MapViewControl; }
            set
            {
                if (value != _MapViewControl)
                {
                    _MapViewControl = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMapViewControl _MapViewControlFD;
        public IMapViewControl MapViewControlFD
        {
            get { return _MapViewControlFD; }
            set
            {
                if (value != _MapViewControlFD)
                {
                    _MapViewControlFD = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDutViewControlVM _DutViewControl;
        public IDutViewControlVM DutViewControl
        {
            get { return _DutViewControl; }
            set
            {
                if (value != _DutViewControl)
                {
                    _DutViewControl = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<SettingCategoryInfo> DeviceSettingCategoryInfos => SettingViewParameter.DeviceSettingCategoryInfos;
        public List<SettingCategoryInfo> SystemSettingCategoryInfos => SettingViewParameter.SystemSettingCategoryInfos;

        public IMainScreenView PostMainScreenView { get; set; }


        public IMainScreenViewModel DiagnosisViewModel { get; set; }

        public IProberStation Prober { get; set; }

        public IFilterPanelViewModel VMFilterPanel { get; set; }
        public IStage3DModel Stage3DModel { get; set; }
        public IMenuLockable MenuLoakables { get; set; }
        public INeedleCleanView NeedleCleanView { get; set; }
        public ILogVM LogViewModel { get; set; }
        public ProberLogLevel LogLevel { get; set; }
        public Window MainWindowWidget { get; set; }
        public Point3D CamPosition { get; set; }
        public Vector3D CamLookDirection { get; set; }
        public Vector3D CamUpDirection { get; set; }

        //public string HomeViewGuid { get; } = "75878d72-d4b9-4b6d-9ee6-f83290712b2e";
        public Guid HomeViewGuid { get; set; }
        public Guid MainMenuViewGuid { get; set; }
        public Guid TopBarViewGuid { get; set; }

        public bool HelpQuestionEnable { get; set; }
        public bool HasLockHandle { get; set; }
        public bool LoginSkipEnable { get; set; }

        private double RemoteViewWidth => 1280;
        private double RemoteviewHeight => 928;

        private double _ORGMainViewWidth;

        public double ORGMainViewWidth
        {
            get { return _ORGMainViewWidth; }
            set
            {
                _ORGMainViewWidth = value;
                MainViewWidth = _ORGMainViewWidth;
            }
        }

        private double _ORGMainViewHeight;

        public double ORGMainViewHeight
        {
            get { return _ORGMainViewHeight; }
            set
            {
                _ORGMainViewHeight = value;
                MainViewHeight = _ORGMainViewHeight;
            }
        }

        private double _MainViewWidth;
        public double MainViewWidth
        {
            get { return _MainViewWidth; }
            set
            {
                if (value != _MainViewWidth)
                {
                    _MainViewWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MainViewHeight;
        public double MainViewHeight
        {
            get { return _MainViewHeight; }
            set
            {
                if (value != _MainViewHeight)
                {
                    _MainViewHeight = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        private bool GetRemoteType(Guid guid)
        {
            bool retVal = false;
            try
            {
                IMainScreenView view = null;

                ScreenViewDictionary.TryGetValue(guid, out view);
                //viewtype.Assembly.GetName
                if (view != null)
                {
                    //string str = view.ToString().Split('.')[0] + ".dll";
                    var viewStr = view.ToString().Split('.');
                    string str = viewStr[viewStr.Length - 1];

                    var viewinfo = ViewParam.info.Find(param => param.ClassName[0].Equals(str.ToString()));

                    if (viewinfo != null)
                    {
                        retVal = viewinfo.RemoteFlag;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task<IMainScreenView> GetViewObjFromViewModelGuid(Guid viewmodelguid)
        {
            IMainScreenView retval = null;

            try
            {

                Task task = new Task(() =>
                {
                    Guid viewGuid = GetViewGuidFromViewModelGuid(viewmodelguid);

                    retval = GetScreenView(viewGuid);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public async Task<IMainScreenView> GetViewObj(Guid viewGuid, object vmParam = null)
        {
            IMainScreenView retView = null;
            try
            {
                Type T = GetViewType(viewGuid);

                try
                {
                    if (T != null)
                    {
                        if (T.Name == nameof(IMainScreenView))
                        {
                            IMainScreenViewModel viewModel;

                            retView = GetScreenView(viewGuid);
                            
                            ViewAndViewModelDictionary.TryGetValue(retView, out viewModel);

                            if (this.VisionManager() != null)
                            {
                                this.VisionManager().AllStageCameraStopGrab();
                            }

                            if (viewModel != null)
                            {
                                IMainScreenViewModel ScreenViewModel = null;
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    (retView as UserControl).DataContext = viewModel;
                                    ScreenViewModel = (IMainScreenViewModel)(retView as UserControl).DataContext;
                                });

                                await Task.Run(async () =>
                                {
                                    await ScreenViewModel?.PageSwitched(vmParam);
                                });
                            }
                            else
                            {
                                (retView as IMainScreenViewModel)?.InitModule();
                                await (retView as IMainScreenViewModel)?.PageSwitched(vmParam);

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    (retView as UserControl).DataContext = MainScreenView;
                                });
                            }
                        }
                    }
                    else
                    {
                        retView = null;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retView;
        }

        public Type GetViewType(Guid guid)
        {
            Type T = null;
            try
            {

                bool ret1;
                //bool ret2;

                ret1 = ScreenViewDictionary.ContainsKey(guid);

                if ((ret1 == true))
                {
                    T = typeof(IMainScreenView);
                }
                else
                {
                    // ERROR
                    LoggerManager.Error($"ViewModelManager : Can't find View.");
                }

                //ret1 = ScreenViewDictionary.ContainsKey(guid);
                //ret2 = TopBarViewDictionary.ContainsKey(guid);

                //if ((ret1 == true) && (ret2 == true))
                //{
                //    // ERROR
                //    LoggerManager.Error($"ViewModelManager : GetViewType() Error");
                //}
                //else if (ret1 == true)
                //{
                //    T = typeof(IMainScreenView);
                //}
                //else if (ret2 == true)
                //{
                //    T = typeof(IMainTopBarView);
                //}
                //else
                //{
                //    // ERROR
                //    LoggerManager.Error($"ViewModelManager : Can't find View.");
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return T;
        }

        private IMainScreenView GetScreenView(Guid guid)
        {
            IMainScreenView ret = null;

            try
            {
                ScreenViewDictionary.TryGetValue(guid, out ret);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }


        public IMainScreenViewModel FindViewModelObject(Guid viewguid)
        {
            IMainScreenViewModel vm = null;

            try
            {
                var view = GetScreenView(viewguid);
                if (view != null)
                {
                    ViewAndViewModelDictionary.TryGetValue(view, out vm);
                }
                else
                {
                    LoggerManager.Debug($"View (GUID:{viewguid}) does not exist.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return vm;
        }

        public EventCodeEnum MakeLogHistory()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InsertView(IMainScreenView view)
        {
            throw new NotImplementedException();
        }

        private bool _FlyoutIsOpen = false;
        public bool FlyoutIsOpen
        {
            get { return _FlyoutIsOpen; }
            set
            {
                if (value != _FlyoutIsOpen)
                {
                    _FlyoutIsOpen = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Guid _OperatorViewGuid;
        //public Guid OperatorViewGuid
        //{
        //    get
        //    {
        //        return _OperatorViewGuid;
        //    }
        //    set
        //    {
        //        if (value != _OperatorViewGuid)
        //        {

        //            _OperatorViewGuid = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private List<Type> _ProberViewTypes;
        public List<Type> ProberViewTypes
        {
            get { return _ProberViewTypes; }
            set
            {
                if (value != _ProberViewTypes)
                {
                    _ProberViewTypes = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<Type> _ProberViewModelTypes;
        public List<Type> ProberViewModelTypes
        {
            get { return _ProberViewModelTypes; }
            set
            {
                if (value != _ProberViewModelTypes)
                {
                    _ProberViewModelTypes = value;
                    RaisePropertyChanged();
                }
            }
        }


        public void ChangeFlyOutControlStatus(bool flag)
        {
            FlyoutIsOpen = flag;
        }

        public void FlyOutLotInformation(Guid guid)
        {
            throw new NotImplementedException();
        }

        public async void UpdateCurMainViewModel()
        {
            try
            {
                await MainScreenViewModel?.PageSwitched();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public Task<bool> CheckUnlockWidget()
        {
            return Task.FromResult<bool>(true);
        }
        public void UpdateWidget()
        {
            throw new NotImplementedException();
        }

        public void SetLoadProgramFlag(bool loadflag)
        {
            throw new NotImplementedException();
        }

        public bool GetLoadProgramFlag()
        {
            return true;
        }

        public void ChangeTabControlSelectedIndex(ProberLogLevel level)
        {
            try
            {
                if (level == ProberLogLevel.PROLOG)
                {
                    TabControlSelectedIndex = 0;
                }
                else if (level == ProberLogLevel.EVENT)
                {
                    TabControlSelectedIndex = 1;
                }
                else
                {
                    LoggerManager.Error($"Wrong Input.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Set3DCamPosition(CameraViewPoint ViewNUM, bool IsItDisplayed2RateMagnification)
        {
            throw new NotImplementedException();
        }

        public void TopBarDrawerOpen(bool ReloadAlarmList = false)
        {
            if (LogLevel == ProberLogLevel.EVENT)
            {
                //LogViewModel?.EventMarkAllSet();
                LoaderLogViewModel?.EventMarkAllSet();
                if(null != LoaderLogViewModel && ReloadAlarmList)
                {
                    LoaderLogViewModel.RefreshLoaderLogAlarmList();
                }
            }
        }

        public async Task<EventCodeEnum> BackPreScreenTransition(bool paramvalidation = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (PreMainScreenViewList.Count > 0)
                {
                    await ViewTransitionAsync(PreMainScreenViewList[PreMainScreenViewList.Count - 1].ScreenGUID, null, paramvalidation, true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum RegisteViewInstance(UCControlInfo info)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RegisteViewModelInstance(UCControlInfo info)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ConnectControlInstances(Guid viewguid, Guid viewmodelguid)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AllUnLock()
        {
            throw new NotImplementedException();
        }

        private void MenuItemLock(ItemCollection menuItems, bool flag)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void MenuItemLock(CUI.MenuItem menuItems, bool flag)
        {
        }


        public void ShowNotifyMessage(int hash, string title, string message)
        {
            throw new NotImplementedException();
        }

        public void HideNotifyMessage(int hash)
        {
            throw new NotImplementedException();
        }

        public void ShowNotifyToastMessage(int hash, string title, string message, int scond)
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (!Initialized)
                {
                    MapViewControl = new MapView.MapViewControl();
                    DutViewControl = new DutViewControl();
                    DisplayPort = new DisplayPort() { GUID = new Guid("21712D23-5F28-51F7-64CF-9C5E86DEC7A4") };
                    Camera = new CameraModule.Camera();
                    Camera.InitModule();
                    //RegisteDisplayPort(DisplayPort);

                    //MainTopBarVM = new LoaderTopBarViewModelModule.LoaderTopBarViewModel();
                    StageListVM = new StageListViewModelModule.StageListViewModel();
                    LuncherListVM = new LuncherListViewModelModule.LuncherListViewModel();

                    //OperatorViewGuid = new Guid(MainMenuViewGuid);

                    LoaderLogViewModel = new LoaderLogViewModel();
                    LoaderLogViewModel.InitModule();

                    //LoggerManager.EventLogMg.OriginEventLogList.CollectionChanged += LoaderCommunicationManager.UpdateStateAlram;

                    retVal = LoadViewDLLParam();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadViewDLLParam() Failed");
                    }

                    retVal = LoadViewModelDLLParam();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadViewModelDLLParam() Failed");
                    }

                    retVal = LoadViewAndViewModelConnectParam();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadViewAndViewModelConnectParam() Failed");
                    }

                    retVal = CreateViewInstance();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"CreateViewInstance() Failed");
                    }

                    retVal = CreateViewModelInstance();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"CreateViewModelInstance() Failed");
                    }

                    retVal = ConnectDataContext();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"ConnectDataContext() Failed");
                    }

                    // TODO : 순서 주의, 필요한 GUID를 먼저 SET 해줘야 됨.
                    SetSystemGUID();

                    //IMainScreenView tempScreenView = null;

                    //// Assign - TopBar
                    //tempScreenView = GetScreenView(TopBarViewGuid);
                    //MainTopBarView = tempScreenView;

                    //// Assign - MainMenu
                    //tempScreenView = GetScreenView(MainMenuViewGuid);
                    //MainMenuView = tempScreenView;

                    //ViewTransitionAsync(HomeViewGuid);

                    ///////

                    var retval = LoadSettingViewParam();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadSettingViewParam() Failed");
                    }

                    Initialized = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;

        }

        /// <summary>
        /// Opera system과 Operetta system에서 사용하는 UI 설정 
        /// </summary>
        public void SetSystemGUID()
        {
            try
            {
                switch (SystemManager.SystemType)
                {
                    case SystemTypeEnum.None:
                        break;
                    case SystemTypeEnum.Opera:
                        HomeViewGuid = new Guid("21f7b3d0-f1e9-4cd4-95a7-7c33742d5787");
                        MainMenuViewGuid = new Guid("fe9417f5-9c54-40bc-b73c-6d415ea1c398");
                        TopBarViewGuid = new Guid("6bc035c4-1aed-4154-9857-49bbdbcb75d8");
                        break;
                    case SystemTypeEnum.GOP:
                        HomeViewGuid = new Guid("75878d72-d4b9-4b6d-9ee6-f83290712b2e");
                        MainMenuViewGuid = new Guid("fe9417f5-9c54-40bc-b73c-6d415ea1c398");
                        TopBarViewGuid = new Guid("6bc035c4-1aed-4154-9857-49bbdbcb75d8");
                        break;
                    case SystemTypeEnum.DRAX:
                        HomeViewGuid = new Guid("3f033346-7b8e-4862-81bc-8cbac4fb2090");
                        MainMenuViewGuid = new Guid("fe9417f5-9c54-40bc-b73c-6d415ea1c398");
                        TopBarViewGuid = new Guid("6bc035c4-1aed-4154-9857-49bbdbcb75d8");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IMainScreenViewModel GetViewModelFromInterface(Type inttype)
        {
            return null;
        }


        #region //.. Load Parameter

        public EventCodeEnum LoadViewDLLParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new LoaderViewDLLParam();
                string paramPath = Path.Combine(this.FileManager().GetRootParamPath(), $@"Parameters\SystemParam\{tmpParam.FileName}");
                RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderViewDLLParam), null, paramPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    ViewParam = tmpParam as LoaderViewDLLParam;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum LoadViewModelDLLParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new LoaderViewModelDLLParam();
                string paramPath = Path.Combine(this.FileManager().GetRootParamPath(), $@"Parameters\SystemParam\{tmpParam.FileName}");
                RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderViewModelDLLParam), null, paramPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    ViewModelParam = tmpParam as LoaderViewModelDLLParam;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($"[ViewModelManager] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }

            return RetVal;
        }


        public EventCodeEnum LoadViewAndViewModelConnectParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;

                tmpParam = new LoaderViewAndViewModelConnectParam();
                string paramPath = Path.Combine(this.FileManager().GetRootParamPath(), $@"Parameters\SystemParam\{tmpParam.FileName}");

                try
                {

                    RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderViewAndViewModelConnectParam), null, paramPath);

                    if (RetVal == EventCodeEnum.NONE)
                    {
                        ViewConnectParam = tmpParam as LoaderViewAndViewModelConnectParam;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        #endregion

        #region //..Load Dll

        private EventCodeEnum CreateViewInstance()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                object tmpView = null;

                RetVal = GetProberViewTypes();

                foreach (var list in ViewParam.info)
                {
                    try
                    {
                        string assemblyName = list.AssemblyName;

                        if (list.ClassName != null && list.ClassName.Count > 0)
                        {
                            string className = list.ClassName[0];

                            Type tmpvm = ProberViewTypes.Find(x => x.Name == className);

                            if (tmpvm != null)
                            {
                                tmpView = Activator.CreateInstance(tmpvm);

                                if (tmpView != null)
                                {
                                    if (tmpView is IMainScreenView)
                                    {
                                        IMainScreenView tmp = (tmpView as IMainScreenView);

                                        ScreenViewDictionary.Add(tmp.ScreenGUID, tmp);
                                    }
                                    else
                                    {

                                    }

                                    //if (tmpView is IMainTopBarView)
                                    //{
                                    //    IMainTopBarView tmp = (tmpView as IMainTopBarView);

                                    //    TopBarViewDictionary.Add(tmp.ScreenGUID, tmp);
                                    //}

                                    //if (tmpView is IMainMenuView)
                                    //{
                                    //    IMainMenuView tmp = (tmpView as IMainMenuView);

                                    //    MainMenuViewDictionary.Add(tmp.ScreenGUID, tmp);
                                    //}
                                }
                                else
                                {
                                    LoggerManager.Debug($"[{assemblyName}] instance does not created.");
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                        LoggerManager.Error($"ViewModelManager - CreateViewInstance(). => {list.AssemblyName}");
                        throw new Exception($"ViewModelManager - CreateViewInstance(). => {list.AssemblyName}");
                    }
                }
                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        //private IMainTopBarView GetTopBarView(Guid guid)
        //{
        //    IMainTopBarView ret = null;

        //    try
        //    {
        //        TopBarViewDictionary.TryGetValue(guid, out ret);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return ret;
        //}

        //private IMainMenuView GetMainMenuView(Guid guid)
        //{
        //    IMainMenuView ret = null;

        //    try
        //    {
        //        MainMenuViewDictionary.TryGetValue(guid, out ret);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return ret;
        //}

        public EventCodeEnum GetProberViewTypes()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (ProberViewTypes == null)
                {
                    ProberViewTypes = new List<Type>();
                }

                //string pluginPath = System.Environment.CurrentDirectory + "\\" + ProberViewModelDll;
                string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProberViewModelDll);

                Assembly plugin = Assembly.LoadFrom(pluginPath);

                if (plugin != null)
                {
                    Type[] types = plugin.GetTypes();

                    foreach (Type t in types)
                    {
                        if (t.IsPublic &&
                           !t.IsAbstract &&
                           t.IsClass)
                        {
                            if (typeof(IMainScreenView).IsAssignableFrom(t))
                            {
                                ProberViewTypes.Add(t);
                            }

                            //if (typeof(IMainScreenView).IsAssignableFrom(t) || typeof(IMainTopBarView).IsAssignableFrom(t))
                            //{
                            //    ProberViewTypes.Add(t);
                            //}
                        }
                    }

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum GetProberViewModelTypes()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (ProberViewModelTypes == null)
                {
                    ProberViewModelTypes = new List<Type>();
                }

                //string pluginPath = System.Environment.CurrentDirectory + "\\" + ProberViewModelDll;
                string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProberViewModelDll);

                Assembly plugin = Assembly.LoadFrom(pluginPath);

                if (plugin != null)
                {
                    Type[] types = plugin.GetTypes();

                    foreach (Type t in types)
                    {
                        if (t.IsPublic &&
                           !t.IsAbstract &&
                           t.IsClass)
                        {
                            if (typeof(IMainScreenViewModel).IsAssignableFrom(t))
                            {
                                ProberViewModelTypes.Add(t);
                            }

                            //if (typeof(IMainScreenViewModel).IsAssignableFrom(t) || typeof(IMainTopBarViewModel).IsAssignableFrom(t))
                            //{
                            //    ProberViewModelTypes.Add(t);
                            //}
                        }
                    }

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum CreateViewModelInstance()
        {

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                object tmpViewModel = null;

                retval = GetProberViewModelTypes();

                //try
                //{
                foreach (var list in ViewModelParam.info)
                {
                    String ass;
                    try
                    {
                        string assemblyName = list.AssemblyName;
                        ass = assemblyName;

                        if (list.ClassName != null && list.ClassName.Count > 0)
                        {
                            string className = list.ClassName[0];

                            Type tmpvm = ProberViewModelTypes.Find(x => x.Name == className);

                            if (tmpvm != null)
                            {
                                tmpViewModel = Activator.CreateInstance(tmpvm);

                                if (tmpViewModel != null)
                                {
                                    if (tmpViewModel is IMainScreenViewModel)
                                    {
                                        IMainScreenViewModel tmp = (tmpViewModel as IMainScreenViewModel);

                                        ScreenViewModelDictionary.Add(tmp.ScreenGUID, tmp);

                                        retval = tmp.InitModule();

                                        if (retval != EventCodeEnum.NONE)
                                        {
                                            LoggerManager.Error($"tmp.InitModule() Failed");
                                        }

                                        Application.Current.Dispatcher.Invoke(delegate
                                        {
                                            tmp.InitViewModel();
                                        });
                                    }

                                    //if (tmpViewModel is IMainTopBarViewModel)
                                    //{
                                    //    IMainTopBarViewModel topbarvm = (tmpViewModel as IMainTopBarViewModel);
                                    //    TopBarViewModelDictionary.Add(topbarvm.ScreenGUID, topbarvm);

                                    //    retval = topbarvm.InitModule();

                                    //    if (retval != EventCodeEnum.NONE)
                                    //    {
                                    //        LoggerManager.Error($"tmp.InitModule() Failed");
                                    //    }
                                    //}

                                    //if (tmpViewModel is IMainMenuViewModel)
                                    //{
                                    //    IMainMenuViewModel mainmenuvm = (tmpViewModel as IMainMenuViewModel);
                                    //    MainMenuViewModelDictionary.Add(mainmenuvm.ScreenGUID, mainmenuvm);

                                    //    retval = mainmenuvm.InitModule();

                                    //    if (retval != EventCodeEnum.NONE)
                                    //    {
                                    //        LoggerManager.Error($"tmp.InitModule() Failed");
                                    //    }
                                    //}
                                }
                                else
                                {
                                    LoggerManager.Debug($"[{assemblyName}] instance does not created.");
                                }
                            }
                        }

                        //string assemblyName = list.AssemblyName;
                        //ass = assemblyName;

                        //tmpViewModel = GetDynamicLoadedViewModel(assemblyName);

                        //if (tmpViewModel != null)
                        //{
                        //    if (tmpViewModel is IMainScreenViewModel)
                        //    {
                        //        IMainScreenViewModel tmp = (tmpViewModel as IMainScreenViewModel);

                        //        ScreenViewModelDictionary.Add(tmp.ViewModelGUID, tmp);

                        //        //tmp.SetContainer(Container);
                        //        retval = tmp.InitModule();

                        //        if (retval != EventCodeEnum.NONE)
                        //        {
                        //            LoggerManager.Error($"tmp.InitModule() Failed");
                        //        }

                        //        Application.Current.Dispatcher.Invoke(delegate
                        //        {
                        //            tmp.InitViewModel();
                        //        });



                        //        //LoggerManager.Debug($"######CreateViewModelInstance#### = {assemblyName}");
                        //    }

                        //    if (tmpViewModel is IMainTopBarViewModel)
                        //    {
                        //        IMainTopBarViewModel tmp = (tmpViewModel as IMainTopBarViewModel);
                        //        TopBarViewModelDictionary.Add(tmp.ViewModelGUID, tmp);

                        //        //tmp.SetContainer(Container);
                        //        retval = tmp.InitModule();

                        //        if (retval != EventCodeEnum.NONE)
                        //        {
                        //            LoggerManager.Error($"tmp.InitModule() Failed");
                        //        }

                        //        //LoggerManager.Debug($"######CreateViewModelInstance#### = {assemblyName}");
                        //    }
                        //}
                        //else
                        //{
                        //    LoggerManager.Debug($"[{assemblyName}] instance does not created.");
                        //}
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Debug($"[{list.AssemblyName}] instance does not created.");
                        LoggerManager.Exception(err);
                        throw new Exception($"CreateViewModelInstance : {list.AssemblyName}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }

        public EventCodeEnum ConnectDataContext()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            IMainScreenView ScreenView = null;
            IMainScreenViewModel ScreenViewModel = null;

            try
            {
                foreach (var item in ViewConnectParam.ConnectionInfos)
                {
                    ScreenView = null;
                    ScreenViewModel = null;

                    //IMainScreenView TopBarView = null;
                    //IMainScreenViewModel TopBarViewModel = null;

                    bool ScreenViewRetFlag = false;
                    bool ScreenViewModelRetFlag = false;

                    //bool TopBarViewRetFlag = false;
                    //bool TopBarViewModelRetFlag = false;

                    ScreenViewRetFlag = ScreenViewDictionary.TryGetValue(item.ViewGUID, out ScreenView);
                    ScreenViewModelRetFlag = ScreenViewModelDictionary.TryGetValue(item.ViewModelGUID, out ScreenViewModel);

                    //TopBarViewRetFlag = TopBarViewDictionary.TryGetValue(item.ViewGUID, out TopBarView);
                    //TopBarViewModelRetFlag = TopBarViewModelDictionary.TryGetValue(item.ViewModelGUID, out TopBarViewModel);

                    if ((ScreenViewRetFlag == true && ScreenViewModelRetFlag == true))
                    {
                        if (ScreenView is UserControl)
                        {
                            (ScreenView as UserControl).DataContext = ScreenViewModel;

                            ViewAndViewModelDictionary.Add(ScreenView, ScreenViewModel);

                            RetVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                        }
                    }

                    //if ((TopBarViewRetFlag == true && TopBarViewModelRetFlag == true))
                    //{
                    //    if (TopBarView is UserControl)
                    //    {
                    //        (TopBarView as UserControl).DataContext = TopBarViewModel;
                    //    }
                    //    else
                    //    {
                    //        RetVal = EventCodeEnum.PARAM_ERROR;
                    //    }
                    //}
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($$"[ViewModelManager] ConnectDataContext(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);


                throw err;
            }

            return RetVal;
        }

        public object GetDynamicLoadedView(string assemblyName)
        {
            object control = null;
            try
            {

                try
                {
                    //string pluginPath = System.Environment.CurrentDirectory + "\\" + assemblyName;
                    string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);

                    Assembly plugin = Assembly.LoadFrom(pluginPath);

                    if (plugin != null)
                    {
                        Version Version = plugin.GetName().Version;

                        Type[] types = plugin.GetTypes();

                        foreach (Type t in types)
                        {
                            if (t.IsPublic &&
                               !t.IsAbstract &&
                               t.IsClass)
                            {
                                if (typeof(IMainScreenView).IsAssignableFrom(t))
                                {
                                    control = (IMainScreenView)Activator.CreateInstance(t);
                                    break;
                                }

                                //if (typeof(IMainTopBarView).IsAssignableFrom(t))
                                //{
                                //    control = (IMainTopBarView)Activator.CreateInstance(t);
                                //    break;
                                //}
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
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return control;
        }


        public object GetDynamicLoadedViewModel(string assemblyName)
        {
            object control = null;
            try
            {

                try
                {
                    //string pluginPath = System.Environment.CurrentDirectory + "\\" + assemblyName;
                    string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);

                    Assembly plugin = Assembly.LoadFrom(pluginPath);


                    //LoggerManager.Debug($"######GetDynamicLoadedViewModel#### = {assemblyName}");


                    if (plugin != null)
                    {
                        Version Version = plugin.GetName().Version;

                        Type[] types = plugin.GetTypes();

                        foreach (Type t in types)
                        {
                            if (t.IsPublic &&
                               !t.IsAbstract &&
                               t.IsClass)
                            {
                                if (typeof(IMainScreenViewModel).IsAssignableFrom(t))
                                {
                                    control = (IMainScreenViewModel)Activator.CreateInstance(t);
                                    break;
                                }

                                //if (typeof(IMainTopBarViewModel).IsAssignableFrom(t))
                                //{
                                //    control = (IMainTopBarViewModel)Activator.CreateInstance(t);
                                //    break;
                                //}
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Exception exSub in ex.LoaderExceptions)
                    {
                        sb.AppendLine(exSub.Message);
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (exFileNotFound != null)
                        {
                            if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                            {
                                sb.AppendLine("Fusion Log:");
                                sb.AppendLine(exFileNotFound.FusionLog);
                            }
                        }
                        sb.AppendLine();
                    }
                    string errorMessage = sb.ToString();
                    //Display or log the error based on your application.
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"Assembly Name: {assemblyName}, Exception: {err.Message}");
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return control;
        }

        #endregion

        #region //.. ViewTransition


        private void SetPostMainScreenToEmpty()
        {
            PostMainScreenView = null;
        }
        private void SetPostMainScreenInfo()
        {
            PostMainScreenView = MainScreenView;
        }
        public async Task<EventCodeEnum> ViewTransitionAsync(Guid guid, object parameter = null, bool paramvalidatiaon = true, bool ChangePrev = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            // 단축키로 바로 들어올 경우에 await으로 인해 Main UI에 hang과 비슷한 현상이 발생함. 따라서 주 쓰레드가 아닌 작업자 쓰레드에서 처리할 수 있도록 ConfigureAwait(false)을 사용함.
            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait").ConfigureAwait(false);

            try
            {
                Type T = GetViewType(guid);
                if (T == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Internal Error", $"View not found. GUID = {guid}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    LoggerManager.Debug($"View not found. GUID = {guid}");
                    retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                    return retVal;
                }
                if (T.Name == "IMainScreenView")
                {
                    IMainScreenView tmp;

                    tmp = GetScreenView(guid);

                    if ((this.MainScreenView != tmp) || (parameter != null))
                    {
                        if (MainScreenView != null)
                        {
                            if (paramvalidatiaon)
                            {
                                IMainScreenViewModel curviewModel = null;
                                ViewAndViewModelDictionary.TryGetValue(MainScreenView, out curviewModel);

                                if (curviewModel != null)
                                {
                                    if (curviewModel is IParamValidation)
                                    {
                                        (curviewModel as IParamValidation).IsParameterChanged(true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            MainScreenView = tmp;
                        }

                        //Login
                        if ((tmp.ScreenGUID).ToString().ToLower() == LOGIN_PAGE_GUID.ToString().ToLower())
                        {
                            MainViewRow = 0;
                            MainViewRowSpan = 4;
                            MainTopBarView = null;
                        }
                        else
                        {
                            MainViewRow = 1;
                            MainViewRowSpan = 3;
                        }

                        if (MainScreenView != null)
                        {
                            // Home
                            if ((tmp.ScreenGUID).ToString() == HomeViewGuid.ToString())
                            {
                                SetPostMainScreenToEmpty();
                                PreMainScreenViewList.Clear();
                            }
                            else
                            {
                                if (ChangePrev == false)
                                {
                                    SetPostMainScreenToEmpty();
                                    if ((MainScreenView.ScreenGUID).ToString().ToLower() != "4732F634-2292-6228-C7E5-24A18C888187".ToLower())
                                    {
                                        PreMainScreenViewList.Add(MainScreenView);
                                    }
                                }
                                else
                                {
                                    SetPostMainScreenInfo();
                                    PreMainScreenViewList.RemoveAt(PreMainScreenViewList.Count() - 1);
                                }
                            }
                        }
                        else
                        {
                            SetPostMainScreenToEmpty();
                            PreMainScreenViewList.Clear();
                        }

                        IMainScreenViewModel PrevScreenViewModel = null;
                        IMainScreenViewModel CurrentViewModel = null;

                        ViewAndViewModelDictionary.TryGetValue(tmp, out CurrentViewModel);

                        CurrentVM = CurrentViewModel;

                        if (CurrentViewModel != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if ((MainScreenView as UserControl).DataContext is IMainScreenViewModel)
                                {
                                    PrevScreenViewModel = (MainScreenView as UserControl).DataContext as IMainScreenViewModel;
                                }
                            });

                            if (PrevScreenViewModel != null && PrevScreenViewModel != CurrentViewModel)
                            {
                               await PrevScreenViewModel.Cleanup(EventCodeEnum.NONE);

                                if (PrevScreenViewModel is IPnpSetup)
                                {
                                    await this.PnPManager().PnpCleanup();
                                }
                            }
                            else if(PrevScreenViewModel == CurrentViewModel)
                            {
                                if(PrevScreenViewModel is ISettingTemplateViewModel)
                                {
                                    bool isSettingNameDifferent = (CurrentViewModel as ISettingTemplateViewModel).SettingNameIsDifferent();

                                    if(isSettingNameDifferent)
                                    {
                                        await PrevScreenViewModel.Cleanup(EventCodeEnum.NONE);
                                    }
                                }
                            }

                            await ApplyViewModel(guid, tmp, CurrentViewModel);
                            
                            if (parameter != null)
                            {
                                retVal = await CurrentViewModel.PageSwitched(parameter);
                            }
                            else if (CurrentViewModel is IPnpSetup == false)
                            {
                                retVal = await CurrentViewModel.PageSwitched();
                            }
                            else
                            {
                                // TODO : ???
                            }
                        }
                    }
                    else
                    {
                        await Application.Current.Dispatcher.Invoke<Task<EventCodeEnum>>(() =>
                        {
                            return ((MainScreenView as UserControl).DataContext as IMainScreenViewModel).Cleanup();
                        });
                    }
                }

                Task task = new Task(() =>
                {
                    ChangeFlyOutControlStatus(false);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoggerManager.Debug($"MainScreenView : {MainScreenView.ToString()}, MainScreenViewModel : {(MainScreenView as UserControl).DataContext.ToString()}");
                    });
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //throw err; //임시주석
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

            return retVal;
        }

        private async Task<EventCodeEnum> ApplyViewModel(Guid guid, IMainScreenView tmp, IMainScreenViewModel CurrentViewModel)
        {
            EventCodeEnum eventCode = EventCodeEnum.UNDEFINED;
            try
            {

                Task task = new Task(() =>
                {
                    if (GetRemoteType(guid))
                    {
                        MainViewWidth = RemoteViewWidth;
                        MainViewHeight = RemoteviewHeight;
                    }
                    else
                    {
                        if (MainViewWidth != ORGMainViewWidth)
                            MainViewWidth = ORGMainViewWidth;
                        if (MainViewHeight != ORGMainViewHeight)
                            MainViewHeight = ORGMainViewHeight;
                    }

                    MainScreenView = tmp;

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if ((MainScreenView as UserControl).DataContext.GetHashCode() != CurrentViewModel.GetHashCode())
                        {
                            //CurrentViewModel = (IMainScreenViewModel)((MainScreenView as UserControl).DataContext);

                            if (CurrentViewModel is IPnpSetup == false)
                            {

                                (MainScreenView as UserControl).DataContext = CurrentViewModel;

                            }
                        }
                    });

                    WaitUIUpdate("ApplyViewModel");

                    eventCode = EventCodeEnum.NONE;
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                eventCode = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Debug($"ApplyViewModel(): Error occurred. Err = {err.Message}");
            }
            return eventCode;
        }

        public async Task<EventCodeEnum> ViewTransitionType(object obj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                try
                {

                    IMainScreenView tmp = null;
                    Guid guid = new Guid();
                    Type[] types = obj.GetType().GetInterfaces();
                    foreach (var item in types)
                    {
                        bool falg = ViewGUIDDictionary.TryGetValue(item.Name, out guid);
                        if (falg)
                        {
                            tmp = GetScreenView(guid);
                            break;
                        }
                    }

                    retVal = await ViewTransitionAsync(guid);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                //SetOffMaskingRect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum ViewTransitionToStatisticsErrorView()
        {
            throw new NotImplementedException();
        }


        private void DrawOverlayCanvas(ImageBuffer image)
        {
            try
            {
                //Camera.DisplayService.OverlayCanvas.Children.Clear();

                if (image.DrawOverlayContexts.Count != 0)
                {
                    //Camera.DisplayService.DrawOverlayContexts.Clear();

                    foreach (var drawable in image.DrawOverlayContexts)
                    {
                        if (drawable is IControlDrawable)
                        {
                            if ((drawable as IControlDrawable).StringTypeColor != null)
                                (drawable as IControlDrawable).Color = (Color)ColorConverter.ConvertFromString((drawable as IControlDrawable).StringTypeColor);
                        }
                        else if (drawable is ITextDrawable)
                        {
                            if ((drawable as ITextDrawable).StringTypeFontColor != null)
                                (drawable as ITextDrawable).Fontcolor = (Color)ColorConverter.ConvertFromString((drawable as ITextDrawable).StringTypeFontColor);
                            if ((drawable as ITextDrawable).StringTypeBackColor != null)
                                (drawable as ITextDrawable).BackColor = (Color)ColorConverter.ConvertFromString((drawable as ITextDrawable).StringTypeBackColor);
                        }
                    }

                    Camera.DisplayService.DrawOverlayContexts = image.DrawOverlayContexts;
                    Camera.DisplayService.Draw(image);
                    Camera.DisplayService.OverlayCanvas.UpdateLayout();
                }
                else
                {
                    Camera.DisplayService.OverlayCanvas.Children.Clear();
                    Camera.DisplayService.OverlayCanvas.UpdateLayout();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private IMainScreenView _OperatorView;
        //public IMainScreenView OperatorView
        //{
        //    get
        //    {
        //        return _OperatorView;
        //    }
        //    set
        //    {
        //        if (value != _OperatorView)
        //        {

        //            _OperatorView = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public IMainScreenViewModel GetViewModelFromGuid(Guid viewmodelguid)
        {
            IMainScreenViewModel retViewModel = null;
            try
            {
                ScreenViewModelDictionary.TryGetValue(viewmodelguid, out retViewModel);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retViewModel;
        }
        public IMainScreenViewModel GetViewModelFromViewGuid(Guid viewguid)
        {
            IMainScreenViewModel viewModel = null;
            try
            {
                IMainScreenView view = null;
                ScreenViewDictionary.TryGetValue(viewguid, out view);
                if (view != null)
                {
                    ViewAndViewModelDictionary.TryGetValue(view, out viewModel);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return viewModel;
        }

        public Guid GetViewGuidFromViewModelGuid(Guid guid)
        {
            return (Guid)LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>()?.GetViewGuidFromViewModelGuid(guid);
        }


        #endregion


        public EventCodeEnum LoadSettingViewParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                SettingViewParameter = new SettingViewParam();

                string fullPath = this.FileManager().GetSystemParamFullPath(SettingViewParameter.FilePath, SettingViewParameter.FileName);

                try
                {
                    IParam tmpParam = null;
                    tmpParam = new SettingViewParam();
                    tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                    RetVal = this.LoadParameter(ref tmpParam, typeof(SettingViewParam));

                    if (RetVal == EventCodeEnum.NONE)
                    {
                        SettingViewParameter = tmpParam as SettingViewParam;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($"[ViewModelManager] LoadSettingViewParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public async Task<EventCodeEnum> InitScreen()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //IMainScreenView ScreenView = null;
                //ScreenViewDictionary.TryGetValue(MainMenuViewGuid, out ScreenView);

                //if (ScreenView != null)
                //{
                //    System.Windows.Application.Current.Dispatcher.Invoke((() =>
                //    {
                //        OperatorView = ScreenView;
                //        (OperatorView as UserControl).DataContext = MainMenuVM;
                //    }));
                //}
                IMainScreenView ScreenView = null;

                ScreenViewDictionary.TryGetValue(LOGIN_PAGE_GUID, out ScreenView);

                if (ScreenView != null)
                {
                    await ViewTransitionAsync(ScreenView.ScreenGUID);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        //public void CloseMainMenu()
        //{
        //    try
        //    {

        //        this.MainMenuVM.CloseMenu();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}

        public EventCodeEnum InsertConnectView(IMainScreenView view, IMainScreenViewModel viewModel)
        {
            return EventCodeEnum.NONE;
        }

        public void WriteCurrentViewAndViewModelName()
        {
            LoggerManager.Debug($"Current View's name : {MainScreenView.GetType().Name}");
            LoggerManager.Debug($"Current ViewModel's name : {(MainScreenView as UserControl).DataContext.GetType().Name}");
        }
        public async Task HomeViewTransition()
        {
            try
            {
                IMainScreenView tempScreenView = null;

                // Assign - TopBar
                tempScreenView = GetScreenView(TopBarViewGuid);
                MainTopBarView = tempScreenView;

                // Assign - MainMenu
                tempScreenView = GetScreenView(MainMenuViewGuid);
                MainMenuView = tempScreenView;
                
                this.ViewModelManager().MainMenuView = MainMenuView;

                Guid tmp = HomeViewGuid;

                await ViewTransitionAsync(tmp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region .. Log Drawer

        private RelayCommand<object> _DrawerCloseCommand;
        public ICommand DrawerCloseCommand
        {
            get
            {
                if (null == _DrawerCloseCommand)
                    _DrawerCloseCommand = new RelayCommand<object>(DrawerCloseCmd);
                return _DrawerCloseCommand;
            }
        }

        private void DrawerCloseCmd(object obj)
        {
            try
            {
                var LoaderLogViewModelObj = LoaderLogViewModel as LoaderLogViewModel;
                if(null != LoaderLogViewModelObj)
                    LoaderLogViewModelObj.RefreshLoaderLogAlarmList();
                
                LoaderCommunicationManager.UpdateStageAlarmCount();
                DrawerHost drawer = Application.Current.Resources["LoaderDrawer"] as DrawerHost;

                if (drawer != null)
                {
                    drawer.IsTopDrawerOpen = !drawer.IsTopDrawerOpen;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void UpdateLoaderAlarmCount()
        {
            (LoaderLogViewModel as LoaderLogViewModel).UpdateLoaderAlarmCount();
        }

        public void SetMainScreenView(IMainScreenView mainScreenView)
        {
            this.MainScreenView = mainScreenView;
        }

        #endregion

        public void WaitUIUpdate(string methodName, int timeoutSec = 3)
        {
            try
            {
                //bool IsDebug = true;

                //Application.Current.Dispatcher.Invoke(new Action(() =>  // Background에서 작업할 내용(UI Update, RePainting)가 남아 있으면 기다림.
                //{
                //    LoggerManager.Debug($"WaitUIUpdate(): END UI({methodName}) Update...");   // Rendering 완료 후 수행하는 동작.
                //}), DispatcherPriority.ContextIdle, null);

                //if (IsDebug == false)
                //{
                if(timeoutSec < 0)
                {
                    LoggerManager.Debug($"WaitUIUpdate timeout: {timeoutSec}, methodName: {methodName}");
                    return;
                }
                LoggerManager.Debug($"WaitUIUpdate(): START UI({methodName}) Update...");

                Application.Current.Dispatcher.Invoke(DispatcherPriority.SystemIdle,
                                                        TimeSpan.FromSeconds(timeoutSec),
                                                        new Action(
                                                            delegate ()
                                                            {
                                                                LoggerManager.Debug($"WaitUIUpdate(): END UI({methodName}) Update...");
                                                            }
                ));
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

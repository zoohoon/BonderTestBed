using ProberInterfaces;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberErrorCode;
using System.Windows;
using FilterPanelVM;

namespace ViewModelModule
{

    using Autofac;
    using System.IO;
    using System.Reflection;
    using System.Linq.Expressions;
    using System.Windows.Controls;
    using MainWindowWidgetControl;
    using ProberInterfaces.PnpSetup;
    using System.Collections.ObjectModel;
    using System.Collections.Concurrent;
    using LogModule;
    using VisualChildrenHelper;
    using WPFNotification.Services;
    using WPFNotification.Core.Configuration;
    using WPFNotification.Model;
    using UcDisplayPort;
    using System.Windows.Media;
    using RelayCommandBase;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using LoaderControllerBase;
    using ProberInterfaces.Foup;
    using MaterialDesignThemes.Wpf;
    using System.Collections.Generic;
    using System.Windows.Controls.Primitives;
    using LoginParamObject;
    using ViewModelModuleParameter;
    using Viewer3DModel;
    using ProberInterfaces.State;
    using ProberInterfaces.WaferAlignEX;

    public class ViewModelManager : IViewModelManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        Dictionary<string, PropertyChangedEventArgs> handle;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (handle == null)
            {
                handle = new Dictionary<string, PropertyChangedEventArgs>();
            }
            if (PropertyChanged != null)
            {
                if (!handle.ContainsKey(propertyName))
                {
                    try
                    {
                        var pc = new PropertyChangedEventArgs(propertyName);
                        handle.Add(propertyName, pc);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
                }


                // WeakReference wr = new WeakReference(new PropertyChangedEventArgs(propertyName));
                PropertyChanged(this, handle[propertyName]);
            }
        }
        #endregion

        public bool Initialized { get; set; } = false;

        //public static readonly string LOGIN_PAGE_GUID = "28A11F12-8918-47FE-8161-3652F2EFEF29";
        //public static readonly string LOT_PAGE_GUID = "CBED19A9-1A90-43DB-B31F-DAF29BC852B4";
        //public static readonly string MAIN_TOP_BAR_GUID = "EC8FB988-222F-1E88-2C18-6DF6A742B3E9";
        //public static readonly string OPERATOR_PAGE_GUID = "d1bc0f1e-36b7-4508-b9c0-c09f08a5587c";

        public Guid OPERATOR_PAGE_GUID = new Guid("d1bc0f1e-36b7-4508-b9c0-c09f08a5587c");
        public Guid LOGIN_PAGE_GUID = new Guid("28A11F12-8918-47FE-8161-3652F2EFEF29");
        public Guid HomeViewGuid { get; set; }
        public Guid TopBarViewGuid { get; set; }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

                foreach (var vm in ScreenViewModelDictionary)
                {
                    vm.Value.DeInitModule();
                }

                //foreach (var vm in TopBarViewModelDictionary)
                //{
                //    vm.Value.DeInitModule();
                //}

                System.Windows.Application.Current.Dispatcher.Invoke((() =>
                {
                    if (widget != null)
                    {
                        widget.Close();
                        widget = null;
                    }
                }));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool _LoginSkipEnable;
        public bool LoginSkipEnable
        {
            get { return _LoginSkipEnable; }
            set
            {
                if (value != _LoginSkipEnable)
                {
                    _LoginSkipEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IProberStation Prober
        {
            get
            {
                return this.ProberStation();
            }
        }

        private readonly INotificationDialogService _dailogService;

        private bool _HasLockHandle = false;
        public bool HasLockHandle
        {
            get { return _HasLockHandle; }
            set
            {
                if (value != _HasLockHandle)
                {
                    _HasLockHandle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILogVM _LogViewModel;
        public ILogVM LogViewModel
        {
            get { return _LogViewModel; }
            set
            {
                if (value != _LogViewModel)
                {
                    _LogViewModel = value;
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

                        LogViewModel.EventMarkAllSet();
                    }
                    //else if (TabControlSelectedIndex == 2)
                    //{
                    //    LogLevel = ProberLogLevel.DEBUG;
                    //}
                    else
                    {
                        LogLevel = ProberLogLevel.UNDEFINED;
                    }

                    //TabControlSelectionChangedCmd();
                }
            }
        }


        private ProberLogLevel _LogLevel;
        public ProberLogLevel LogLevel
        {
            get { return _LogLevel; }
            set
            {
                if (value != _LogLevel)
                {
                    _LogLevel = value;
                    RaisePropertyChanged();

                    //if (LogLevel == ProberLogLevel.PROLOG)
                    //{
                    //    TabControlSelectedIndex = 0;
                    //}
                    //else if (LogLevel == ProberLogLevel.EVENT)
                    //{
                    //    TabControlSelectedIndex = 1;
                    //}
                    //else if (LogLevel == ProberLogLevel.DEBUG)
                    //{
                    //    TabControlSelectedIndex = 2;
                    //}
                    //else
                    //{
                    //    LoggerManager.Error("Error");
                    //}
                }
            }
        }

        private ObservableCollection<object> _SelectedItems;
        public ObservableCollection<object> SelectedItems
        {
            get { return _SelectedItems; }
            set
            {
                if (value != _SelectedItems)
                {
                    _SelectedItems = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LogDataStructure _SelectedItem;
        public LogDataStructure SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (value != _SelectedItem)
                {
                    //bool firstTrigger = false;
                    //if (_SelectedItem == null)
                    //{
                    //    firstTrigger = true;
                    //}

                    _SelectedItem = value;


                    //if (_SelectedItem != null)
                    //{
                    //    _SelectedItem.IsSelected = true;
                    //}

                    RaisePropertyChanged();

                    //if (firstTrigger == true)
                    //{
                    //    _SelectedItem.IsSelected = true;
                    //    DataGridSelectionChangedCmd();
                    //}
                }
            }
        }


        private string _SearchKeyword = string.Empty;
        public string SearchKeyword
        {
            get { return _SearchKeyword; }
            set
            {
                if (value != _SearchKeyword)
                {
                    _SearchKeyword = value;
                    RaisePropertyChanged();
                    SearchMatched();
                }
            }
        }

        private bool _HelpQuestionEnable;
        public bool HelpQuestionEnable
        {
            get { return _HelpQuestionEnable; }
            set
            {
                if (value != _HelpQuestionEnable)
                {
                    _HelpQuestionEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Int32 _MainViewRowSpan;
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

        //private Guid _InitViewModelGUIDForWidget;
        //public Guid InitViewModelGUIDForWidget
        //{
        //    get
        //    {
        //        return _InitViewModelGUIDForWidget;
        //    }
        //    set
        //    {
        //        if (value != _InitViewModelGUIDForWidget)
        //        {

        //            _InitViewModelGUIDForWidget = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Guid _StatisticsErrorViewGuid;
        //public Guid StatisticsErrorViewGuid
        //{
        //    get { return _StatisticsErrorViewGuid; }
        //    set
        //    {
        //        if (value != _StatisticsErrorViewGuid)
        //        {
        //            _StatisticsErrorViewGuid = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Guid _InitScreenViewGuid;
        //public Guid InitScreenViewGuid
        //{
        //    get
        //    {
        //        return _InitScreenViewGuid;
        //    }
        //    set
        //    {
        //        if (value != _InitScreenViewGuid)
        //        {

        //            _InitScreenViewGuid = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Guid _InitTopBarViewGuid;
        //public Guid InitTopBarViewGuid
        //{
        //    get
        //    {
        //        return _InitTopBarViewGuid;
        //    }
        //    set
        //    {
        //        if (value != _InitTopBarViewGuid)
        //        {

        //            _InitTopBarViewGuid = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Window _MainWindowWidget;
        public Window MainWindowWidget
        {
            get { return _MainWindowWidget; }
            set
            {
                if (value != _MainWindowWidget)
                {
                    _MainWindowWidget = value;
                    RaisePropertyChanged(nameof(MainWindowWidget));
                }
            }
        }
        private MainWindowWidget _widget;
        public MainWindowWidget widget
        {
            get
            {
                return _widget;
            }
            set
            {
                if (value != _widget)
                {
                    _widget = value;
                    MainWindowWidget = _widget;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _LotInfoView;
        public IMainScreenView LotInfoView
        {
            get
            {
                return _LotInfoView;
            }
            set
            {
                if (value != _LotInfoView)
                {

                    _LotInfoView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMainScreenView _OperatorView;
        public IMainScreenView OperatorView
        {
            get
            {
                return _OperatorView;
            }
            set
            {
                if (value != _OperatorView)
                {

                    _OperatorView = value;
                    RaisePropertyChanged();
                }
            }
        }



        private IMainScreenView _LoginScreenView;
        public IMainScreenView LoginScreenView
        {
            get
            {
                return _LoginScreenView;
            }
            set
            {
                if (value != _LoginScreenView)
                {

                    _LoginScreenView = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        public IMainScreenView PostMainScreenView { get; set; }

        public IMainScreenViewModel DiagnosisViewModel { get; set; }

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
        //private IMainScreenViewModel _MainTopBarViewModel;
        //public IMainScreenViewModel MainTopBarViewModel
        //{
        //    get { return _MainTopBarViewModel; }
        //    set
        //    {
        //        if (value != _MainTopBarViewModel)
        //        {
        //            _MainTopBarViewModel = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

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
                    RaisePropertyChanged();
                }
            }
        }

        //private Dictionary<Guid, IMainTopBarView> _TopBarViewDictionary = new Dictionary<Guid, IMainTopBarView>();
        //public Dictionary<Guid, IMainTopBarView> TopBarViewDictionary
        //{
        //    get { return _TopBarViewDictionary; }
        //    set
        //    {
        //        if (value != _TopBarViewDictionary)
        //        {
        //            _TopBarViewDictionary = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Dictionary<Guid, IMainTopBarViewModel> _TopBarViewModelDictionary = new Dictionary<Guid, IMainTopBarViewModel>();
        //public Dictionary<Guid, IMainTopBarViewModel> TopBarViewModelDictionary
        //{
        //    get { return _TopBarViewModelDictionary; }
        //    set
        //    {
        //        if (value != _TopBarViewModelDictionary)
        //        {
        //            _TopBarViewModelDictionary = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

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

        //private Dictionary<UserControl, UserControl> _LockViewAndTopBars
        //    = new Dictionary<UserControl, UserControl>();

        //public Dictionary<UserControl, UserControl> LockViewAndTopBars
        //{
        //    get { return _LockViewAndTopBars; }
        //    set { _LockViewAndTopBars = value; }
        //}


        //private Dictionary<int, Tuple<UserControl, UserControl>> _LockViewAndTopBars
        //    = new Dictionary<int, Tuple<UserControl, UserControl>>();

        //public Dictionary<int, Tuple<UserControl, UserControl>> LockViewAndTopBars
        //{
        //    get { return _LockViewAndTopBars; }
        //    set { _LockViewAndTopBars = value; }
        //}

        private List<LockControlInfo> _LockControls
             = new List<LockControlInfo>();

        public List<LockControlInfo> LockControls
        {
            get { return _LockControls; }
            set { _LockControls = value; }
        }




        private ViewAndViewModelConnectParam _ViewConnectParam;
        public ViewAndViewModelConnectParam ViewConnectParam
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



        private ViewDLLParam _ViewParam;
        public ViewDLLParam ViewParam
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

        private ViewModelDLLParam _ViewModelParam;
        public ViewModelDLLParam ViewModelParam
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

        private ViewGUIDAndViewModelConnectParam _ViewGUIDAndVMTypeParam;
        public ViewGUIDAndViewModelConnectParam ViewGUIDAndVMTypeParam
        {
            get { return _ViewGUIDAndVMTypeParam; }
            set
            {
                if (value != _ViewGUIDAndVMTypeParam)
                {
                    _ViewGUIDAndVMTypeParam = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        //private ObservableCollection<IPnpSetupScreen> _PnpSetupScreen
        //    = new ObservableCollection<IPnpSetupScreen>();
        //public ObservableCollection<IPnpSetupScreen> PnpSetupScreen
        //{
        //    get { return _PnpSetupScreen; }
        //    set
        //    {
        //        if (value != _PnpSetupScreen)
        //        {
        //            _PnpSetupScreen = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool _FlyoutLotInfoIsOpen;
        public bool FlyoutLotInfoIsOpen
        {
            get { return _FlyoutLotInfoIsOpen; }
            set
            {
                if (value != _FlyoutLotInfoIsOpen)
                {
                    _FlyoutLotInfoIsOpen = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _FlyoutIsOpen;
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

        private IStage3DModel _Stage3DModel;
        public IStage3DModel Stage3DModel
        {
            get
            {
                return _Stage3DModel;
            }
            set
            {
                if (value != _Stage3DModel)
                {

                    _Stage3DModel = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IStageSupervisor _StageSupervisor;
        public IStageSupervisor StageSupervisor
        {
            get
            {
                return _StageSupervisor;
            }
            set
            {
                if (value != _StageSupervisor)
                {

                    _StageSupervisor = value;
                    RaisePropertyChanged();
                }
            }
        }

        //  public IStageSupervisor StageSupervisor { get; private set; }

        private Point3D _CamPosition = new Point3D(0, 785.84, 1255.72);
        public Point3D CamPosition
        {
            get { return _CamPosition; }
            set
            {
                if (value != _CamPosition)
                {
                    _CamPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Vector3D _CamLookDirection = new Vector3D(-0.76, -0.33, -0.57);
        public Vector3D CamLookDirection
        {
            get { return _CamLookDirection; }
            set
            {
                if (value != _CamLookDirection)
                {
                    _CamLookDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Vector3D _CamUpDirection = new Vector3D(-0.26, 0.945, -0.2);
        public Vector3D CamUpDirection
        {
            get { return _CamUpDirection; }
            set
            {
                if (value != _CamUpDirection)
                {
                    _CamUpDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMenuLockable _MenuLockables;

        public IMenuLockable MenuLoakables
        {
            get { return _MenuLockables; }
            set { _MenuLockables = value; }
        }

        private RelayCommand _LotInfoViewFlyOutCloseCommand;
        public ICommand LotInfoViewFlyOutCloseCommand
        {
            get
            {
                if (null == _LotInfoViewFlyOutCloseCommand) _LotInfoViewFlyOutCloseCommand = new RelayCommand(FuncLotInfoViewFlyOutCloseCommand);
                return _LotInfoViewFlyOutCloseCommand;
            }
        }

        private void FuncLotInfoViewFlyOutCloseCommand()
        {
            FlyoutLotInfoIsOpen = false;
        }


        public IFilterPanelViewModel VMFilterPanel
        {
            get;
            set;
        }
        private double _FoupCoverPos;
        public double FoupCoverPos
        {
            get { return _FoupCoverPos; }
            set
            {
                if (value != _FoupCoverPos)
                {
                    _FoupCoverPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _FoupCoverHeight;
        public double FoupCoverHeight
        {
            get { return _FoupCoverHeight; }
            set
            {
                if (value != _FoupCoverHeight)
                {
                    _FoupCoverHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> Motion
        private IMotionManager _Motion;
        public IMotionManager Motion
        {
            get { return _Motion; }
            set
            {
                if (value != _Motion)
                {
                    _Motion = value;
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
        #endregion
        #region ==> Z1
        private ProbeAxisObject _Z1;
        public ProbeAxisObject Z1
        {
            get { return _Z1; }
            set
            {
                if (value != _Z1)
                {
                    _Z1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> Z2
        private ProbeAxisObject _Z2;
        public ProbeAxisObject Z2
        {
            get { return _Z2; }
            set
            {
                if (value != _Z2)
                {
                    _Z2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> Z3
        private ProbeAxisObject _Z3;
        public ProbeAxisObject Z3
        {
            get { return _Z3; }
            set
            {
                if (value != _Z3)
                {
                    _Z3 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> U1
        private ProbeAxisObject _U1;
        public ProbeAxisObject U1
        {
            get { return _U1; }
            set
            {
                if (value != _U1)
                {
                    _U1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> U2
        private ProbeAxisObject _U2;
        public ProbeAxisObject U2
        {
            get { return _U2; }
            set
            {
                if (value != _U2)
                {
                    _U2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LZ
        private AxisObject _LZ;
        public AxisObject LZ
        {
            get { return _LZ; }
            set
            {
                if (value != _LZ)
                {
                    _LZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LW
        private ProbeAxisObject _LW;
        public ProbeAxisObject LW
        {
            get { return _LW; }
            set
            {
                if (value != _LW)
                {
                    _LW = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> XAxis
        private ProbeAxisObject _XAxis;
        public ProbeAxisObject XAxis
        {
            get { return _XAxis; }
            set
            {
                if (value != _XAxis)
                {
                    _XAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> YAxis
        private ProbeAxisObject _YAxis;
        public ProbeAxisObject YAxis
        {
            get { return _YAxis; }
            set
            {
                if (value != _YAxis)
                {
                    _YAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ZAxis
        private ProbeAxisObject _ZAxis;
        public ProbeAxisObject ZAxis
        {
            get { return _ZAxis; }
            set
            {
                if (value != _ZAxis)
                {
                    _ZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> TAxis
        private ProbeAxisObject _TAxis;
        public ProbeAxisObject TAxis
        {
            get { return _TAxis; }
            set
            {
                if (value != _TAxis)
                {
                    _TAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DOSCAN_SENSOR_OUT
        private IOPortDescripter<bool> _DOSCAN_SENSOR_OUT;
        public IOPortDescripter<bool> DOSCAN_SENSOR_OUT
        {
            get { return _DOSCAN_SENSOR_OUT; }
            set
            {
                if (value != _DOSCAN_SENSOR_OUT)
                {
                    _DOSCAN_SENSOR_OUT = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> DOLOADER_DOOR
        private IOPortDescripter<bool> _DOLOADER_DOOR;
        public IOPortDescripter<bool> DOLOADER_DOOR
        {
            get { return _DOLOADER_DOOR; }
            set
            {
                if (value != _DOLOADER_DOOR)
                {
                    _DOLOADER_DOOR = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MoniteringManager
        private IMonitoringManager _MonitoringManager;
        public IMonitoringManager MonitoringManager
        {
            get { return _MonitoringManager; }
            set
            {
                if (value != _MonitoringManager)
                {
                    _MonitoringManager = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private ProbeAxisObject _PZAxis;
        public ProbeAxisObject PZAxis
        {
            get { return _PZAxis; }
            set
            {
                if (value != _PZAxis)
                {
                    _PZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _MNCAxis;
        public ProbeAxisObject MNCAxis
        {
            get { return _MNCAxis; }
            set
            {
                if (value != _MNCAxis)
                {
                    _MNCAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _SC;
        public ProbeAxisObject SC
        {
            get { return _SC; }
            set
            {
                if (value != _SC)
                {
                    _SC = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumProbingState _ZUpState;
        public EnumProbingState ZUpState
        {
            get { return _ZUpState; }
            set
            {
                if (value != _ZUpState)
                {
                    _ZUpState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _IsWaferBridgeExtended;
        public IOPortDescripter<bool> IsWaferBridgeExtended
        {
            get { return _IsWaferBridgeExtended; }
            set
            {
                if (value != _IsWaferBridgeExtended)
                {
                    _IsWaferBridgeExtended = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _IsInspectionTrayExtended;
        public IOPortDescripter<bool> IsInspectionTrayExtended
        {
            get { return _IsInspectionTrayExtended; }
            set
            {
                if (value != _IsInspectionTrayExtended)
                {
                    _IsInspectionTrayExtended = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _IsWaferOnInspectionTray;
        public IOPortDescripter<bool> IsWaferOnInspectionTray
        {
            get { return _IsWaferOnInspectionTray; }
            set
            {
                if (value != _IsWaferOnInspectionTray)
                {
                    _IsWaferOnInspectionTray = value;
                    RaisePropertyChanged();
                }
            }
        }


        public IWaferObject Wafer { get { return this.GetParam_Wafer(); } }

        #region ==> ThreeLegHeight
        private double _ThreeLegHeight;
        public double ThreeLegHeight
        {
            get { return _ThreeLegHeight; }
            set
            {
                if (value != _ThreeLegHeight)
                {
                    _ThreeLegHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private ILoaderControllerExtension _LoaderController;
        public ILoaderControllerExtension LoaderController
        {
            get { return _LoaderController; }
            set
            {
                if (value != _LoaderController)
                {
                    _LoaderController = value;
                    RaisePropertyChanged();
                }
            }
        }


        public List<SettingCategoryInfo> DeviceSettingCategoryInfos => SettingViewParameter.DeviceSettingCategoryInfos;
        public List<SettingCategoryInfo> SystemSettingCategoryInfos => SettingViewParameter.SystemSettingCategoryInfos;
        private Dictionary<int, Brush> PrevBrushTable = new Dictionary<int, Brush>();
        private Brush LockBrush = Brushes.Orange;
        private EventCodeEnum ControlLockOrUnLock(UserControl uc, bool flag, int avoidhashcode = -1)
        {
            // flag => false : Control Lock
            // flag => true : Control UnLock

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                //foreach (var  control in VisualChildrenHelper.FindChildren(uc))
                //{
                //    LockControl(control as UIElement, flag);
                //}

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // Button
                    foreach (var button in VisualChildrenHelper.FindVisualChildren<Button>(uc))
                    {
                        if (button is CUI.Button)
                        {
                            CUI.Button cuiButton = button as CUI.Button;

                            if (!(cuiButton?.GUID.ToString() == "3776199a-0c2c-49c1-8f6c-5c2a5a9fa873" ||
                            cuiButton?.GUID.ToString() == "5e80ff23-fbc8-4a56-9c31-a5d060397807"))
                            {
                                if (cuiButton.Lockable == true)
                                {
                                    button.SetCurrentValue(Button.IsEnabledProperty, flag);
                                }
                                if (cuiButton.AvoidLockHashCodes != null)
                                {
                                    if (cuiButton.AvoidLockHashCodes.Find(hashcode => hashcode == avoidhashcode) != 0 && avoidhashcode != -1)
                                    {
                                        button.SetCurrentValue(Button.IsEnabledProperty, true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            button.IsEnabled = flag;
                        }
                        button.UpdateLayout();
                    }

                    foreach (var button in VisualChildrenHelper.FindVisualChildren<RepeatButton>(uc))
                    {
                        if (button is CUI.RepeatButton)
                        {
                            CUI.RepeatButton cuiButton = button as CUI.RepeatButton;

                            if (cuiButton.Lockable == true)
                            {
                                button.SetCurrentValue(Button.IsEnabledProperty, flag);
                            }

                            if (cuiButton.AvoidLockHashCodes != null)
                            {
                                if (cuiButton.AvoidLockHashCodes.Find(hashcode => hashcode == avoidhashcode) != 0 && avoidhashcode != -1)
                                {
                                    button.SetCurrentValue(Button.IsEnabledProperty, true);
                                }
                            }
                        }
                        else
                        {
                            button.IsEnabled = flag;
                        }

                        button.UpdateLayout();
                    }

                    // UcDisplayPort
                    foreach (var displayport in VisualChildrenHelper.FindVisualChildren<DisplayPort>(uc))
                    {
                        if (displayport.IsEnabled != flag)
                        {
                            //displayport.SetCurrentValue(Button.IsEnabledProperty, flag);
                            displayport.SetCurrentValue(UserControl.IsEnabledProperty, flag);
                        }
                        //if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                        //    displayport.SetCurrentValue(Button.IsEnabledProperty, true);
                    }

                    //MenuItem
                    foreach (var menuItem in VisualChildrenHelper.FindVisualChildren<MenuItem>(uc))
                    {
                        MenuItemLock(menuItem.Items, flag);
                    }
                    ////UserControl
                    foreach (var usercontrol in VisualChildrenHelper.FindVisualChildren<UserControl>(uc))
                    {
                        if (usercontrol is DisplayPort)
                            continue;
                        if (usercontrol is ICUIControl)
                        {

                            if ((usercontrol as ICUIControl).Lockable)
                            {
                                if (usercontrol.IsEnabled != flag)
                                {
                                    usercontrol.SetCurrentValue(Button.IsEnabledProperty, flag);
                                }
                            }
                            //else
                            //    usercontrol.IsEnabled = true;

                            if (VisualChildrenHelper.FindVisualChildren<UserControl>(usercontrol) != null & (usercontrol as ICUIControl).InnerLockable)
                            {
                                foreach (var item in VisualChildrenHelper.FindVisualChildren<UserControl>(usercontrol))
                                {
                                    if (item is ICUIControl)
                                    {
                                        if ((item as ICUIControl).Lockable)
                                        {
                                            if (item.IsEnabled != flag)
                                            {
                                                item.SetCurrentValue(Button.IsEnabledProperty, flag);
                                            }
                                        }
                                        else
                                            item.SetCurrentValue(Button.IsEnabledProperty, true);
                                    }
                                }
                            }
                        }

                    }

                    //ListBox
                    //foreach (var listbox in VisualChildrenHelper.FindVisualChildren<ListBox>(uc))
                    //{
                    //    if(listbox.IsEnabled != flag)
                    //        listbox.IsEnabled = flag;
                    //}

                    retval = EventCodeEnum.NONE;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retval;
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

        private void MenuItemLock(ItemCollection menuItems, bool flag)
        {
            try
            {


                //if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING || !flag)
                //if (!flag)
                //{
                foreach (var menuitem in menuItems)
                {
                    var item = menuitem as MenuItem;
                    if (item != null)
                    {
                        foreach (var menu in item.Items)
                        {
                            if (menu is CUI.MenuItem)
                            {
                                //if (item.AlternationCount > 0)
                                //{
                                //    MenuItemLock(item.Items, flag);
                                //}

                                if (!PrevBrushTable.ContainsKey(item.GetHashCode()))
                                {
                                    //if (menu.Background != Brushes.Orange)
                                    //{
                                    //    PrevBrushTable.Add(menu.GetHashCode(), menu.Background);
                                    //}
                                }

                                var cuiItem = menu as CUI.MenuItem;
                                if (cuiItem.Lockable == true)
                                {
                                    cuiItem.IsEnabled = flag;
                                    if (flag)
                                    {
                                        //cuiItem.Background = PrevBrushTable[item.GetHashCode()];
                                    }
                                    else
                                    {
                                        //cuiItem.Background = LockBrush;
                                    }
                                }
                                else
                                {
                                    cuiItem.IsEnabled = true;
                                }
                                if (cuiItem.Items.Count != 0)
                                {
                                    MenuItemLock((MenuItem)cuiItem, flag);
                                }
                            }
                            else
                            {
                                MenuItemLock((MenuItem)menu, flag);
                            }
                        }
                    }


                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void MenuItemLock(MenuItem menuItems, bool flag)
        {
            try
            {
                if (menuItems != null)
                {
                    foreach (var menu in menuItems.Items)
                    {
                        if (menu is CUI.MenuItem)
                        {
                            //if (item.AlternationCount > 0)
                            //{
                            //    MenuItemLock(item.Items, flag);
                            //}

                            if (!PrevBrushTable.ContainsKey(menuItems.GetHashCode()))
                            {
                                //if (menu.Background != Brushes.Orange)
                                //{
                                //    PrevBrushTable.Add(menu.GetHashCode(), menu.Background);
                                //}
                            }

                            var cuiItem = menu as CUI.MenuItem;
                            if (cuiItem.Lockable == true)
                            {
                                cuiItem.IsEnabled = flag;
                                if (flag)
                                {
                                    //cuiItem.Background = PrevBrushTable[item.GetHashCode()];
                                }
                                else
                                {
                                    //cuiItem.Background = LockBrush;
                                }
                            }
                            else
                            {
                                cuiItem.IsEnabled = true;
                            }
                        }
                        else
                        {
                            MenuItemLock((ItemCollection)menu, flag);
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

        private void Notification(string title, string message, NotificationFlowDirection direction = NotificationFlowDirection.Custom)
        {
            try
            {

                var notificationConfiguration = NotificationConfiguration.DefaultConfiguration;
                notificationConfiguration.NotificationFlowDirection = direction;

                var newNotification = new Notification()
                {
                    Title = title,
                    Message = message
                };

                _dailogService.ShowNotificationWindow(newNotification, notificationConfiguration);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ToastNotification(string title, string message, int Second, NotificationFlowDirection direction = NotificationFlowDirection.Custom)
        {
            try
            {
                NotificationConfiguration Default = NotificationConfiguration.DefaultConfiguration;

                TimeSpan DurationTime = new TimeSpan(0, 0, Second);

                var notificationConfiguration = new NotificationConfiguration(DurationTime, Default.Width, Default.Height, Default.TemplateName, Default.NotificationFlowDirection);

                notificationConfiguration.NotificationFlowDirection = direction;

                var newNotification = new Notification()
                {
                    Title = title,
                    Message = message
                };

                _dailogService.ShowToastNotificationWindow(newNotification, notificationConfiguration);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum AllUnLock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var control in LockControls)
                {
                    //MainView
                    //ControlLockOrUnLock(control.Value.Item1, false);
                    ControlLockOrUnLock(control.ViewControl, true);
                    //TopBar
                    //ControlLockOrUnLock(control.Value.Item2, false);
                    ControlLockOrUnLock(control.TopBarControl, true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void ShowNotifyMessage(int hash, string title, string message)
        {
            try
            {
                if (_dailogService != null)
                {
                    //var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

                    //Task.Factory.StartNew(Notification())

                    System.Windows.Application.Current.Dispatcher.Invoke((() =>
                    {
                        Notification(title, message);
                    }));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
        }
        public void ShowNotifyToastMessage(int hash, string title, string message, int secont)
        {
            try
            {
                if (_dailogService != null)
                {
                    //var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

                    //Task.Factory.StartNew(Notification())

                    System.Windows.Application.Current.Dispatcher.Invoke((() =>
                    {
                        ToastNotification(title, message, secont);
                    }));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
        }

        public void HideNotifyMessage(int hash)
        {
            try
            {
                if (_dailogService != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((() =>
                    {
                        _dailogService.ClearNotifications();
                    }));
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
        }

        //public EventCodeEnum ViewTransitionToStatisticsErrorView()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;
        //    try
        //    {

        //        retval = ViewTransition(StatisticsErrorViewGuid);

        //        retval = EventCodeEnum.NONE;

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return retval;
        //}

        public void FlyOutLotInformation(Guid guid)
        {
            try
            {
                IMainScreenView tmp = null;

                tmp = GetScreenView(guid);

                if (tmp != null)
                {
                    LotInfoView = tmp;
                    FlyoutLotInfoIsOpen = !FlyoutLotInfoIsOpen;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeFlyOutControlStatus(bool flag)
        {
            FlyoutIsOpen = flag;

            if (FlyoutIsOpen)
            {
                UserControl panel = OperatorView as UserControl;
                if (panel != null)
                {
                    this.ApplyMasking(panel);
                }
            }
        }

        private void SearchMatched()
        {
            //try
            //{
            //    string upper = SearchKeyword.ToUpper();
            //    //string lower = SearchKeyword.ToLower();

            //    if ((LogLevel == ProberLogLevel.DEBUG) || (LogLevel == ProberLogLevel.FILTEREDDEBUG))
            //    {
            //        await Task.Run(() =>
            //        {
            //            try
            //            {
            //                if (SearchKeyword.Length > 0)
            //                {
            //                    SynchronizedObservableCollection<LogDataStructure> histories = null;

            //                    LogViewModel.LogHistories.FilteredLogHistories.Clear();
            //                    histories = LogViewModel.LogHistories.DebugLogHistories;

            //                    if (histories != null)
            //                    {
            //                        //IEnumerable<LogDataStructure> filtered = from log in histories
            //                        //                                         where log.Message.ToUpper().Contains(upper) ||
            //                        //                                               log.Description.ToString().ToUpper().Contains(upper) ||
            //                        //                                               log.Tag.Contains(upper)
            //                        //                                         select log;

            //                        //var filtered = from log in histories
            //                        //               where log.Message.ToUpper().Contains(upper) ||
            //                        //                     log.Description.ToString().ToUpper().Contains(upper) ||
            //                        //                     log.Tag.Contains(upper)
            //                        //               select log;


            //                        //var filtered = LogViewModel.LogHistories.DebugLogHistories.Where(t => t.Message.ToUpper().Contains(upper) |
            //                        //                               t.Description.ToString().ToUpper().Contains(upper)
            //                        //                              );

            //                        //var filtered = LogViewModel.LogHistories.DebugLogHistories.Where(t => t.Message.ToUpper().Contains(upper));

            //                        var filtered = from log in histories
            //                                       where log.Message.ToUpper().Contains(upper) ||
            //                                             log.Description.ToString().ToUpper().Contains(upper) ||
            //                                             log.Tag.Contains(upper, StringComparer.OrdinalIgnoreCase)
            //                                       select log;

            //                        //var filtered = from log in histories
            //                        //               where String.Compare(log.Message, upper, StringComparison.InvariantCultureIgnoreCase) == 0 ||
            //                        //                     log.Description.ToString().ToUpper().Contains(upper) ||
            //                        //                     log.Tag.Contains(upper, StringComparer.OrdinalIgnoreCase)
            //                        //               select log;


            //                        //var filtered = histories.Where(t => t.Message.ToUpper().Contains(upper) |
            //                        //                                   t.Description.ToString().ToUpper().Contains(upper)
            //                        //                                   );

            //                        if ((filtered != null))
            //                        {
            //                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            //                            {
            //                                var enumerator = filtered.GetEnumerator();

            //                                try
            //                                {
            //                                    while (enumerator.MoveNext())
            //                                    {
            //                                        LogDataStructure n = enumerator.Current;
            //                                        LogViewModel.LogHistories.FilteredLogHistories.Add(n);
            //                                    }
            //                                }
            //                                catch (Exception err)
            //                                {
            //                                    LoggerManager.Exception(err);
            //                                }
            //                                finally
            //                                {
            //                                    var disposable = enumerator as IDisposable;
            //                                    if (disposable != null)
            //                                    {
            //                                        disposable.Dispose();
            //                                    }
            //                                }

            //                                //if(LogViewModel.LogHistories.FilteredLogHistories.Count > 0)
            //                                //{
            //                                //    LogType
            //                                //}
            //                                //foreach (var item in filtered)
            //                                //{
            //                                //    FilteredLogHistories.Add(item);
            //                                //}
            //                            });
            //                        }
            //                    }

            //                    if (LogViewModel.LogHistories.FilteredLogHistories.Count > 0)
            //                    {
            //                        LogLevel = ProberLogLevel.FILTEREDDEBUG;
            //                    }
            //                }
            //                else
            //                {
            //                    LogLevel = ProberLogLevel.DEBUG;
            //                }
            //            }
            //            catch (Exception err)
            //            {
            //                LoggerManager.Exception(err);
            //            }
            //        });
            //    }
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //    throw;
            //}
        }

        private RelayCommand<object> _DrawerCloseCommand;
        public ICommand DrawerCloseCommand
        {
            get
            {
                if (null == _DrawerCloseCommand) _DrawerCloseCommand = new RelayCommand<object>(DrawerCloseCmd);
                return _DrawerCloseCommand;
            }
        }

        private void DrawerCloseCmd(object obj)
        {
            try
            {
                DrawerHost drawer = Application.Current.Resources["Drawer"] as DrawerHost;

                //LogViewModel.MadeHistoryForProlog = false;
                //LogViewModel.MadeHistoryForEventlog = false;
                //LogViewModel.MadeHistoryForDebuglog = false;
                LogViewModel?.RefreshLoaderLogAlarmList();                
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

        public void TopBarDrawerOpen(bool ReloadAlarmList = false)
        {
            if (LogLevel == ProberLogLevel.EVENT)
            {
                LogViewModel.EventMarkAllSet();
                if (null != LogViewModel && ReloadAlarmList)
                {
                    LogViewModel.RefreshLoaderLogAlarmList();
                }
            }
        }

        //private RelayCommand _TabControlSelectionChangedCommand;
        //public ICommand TabControlSelectionChangedCommand
        //{
        //    get
        //    {
        //        if (null == _TabControlSelectionChangedCommand) _TabControlSelectionChangedCommand = new RelayCommand(TabControlSelectionChangedCmd);
        //        return _TabControlSelectionChangedCommand;
        //    }
        //}

        //private void TabControlSelectionChangedCmd()
        //{
        //    try
        //    {
        //        //bool updateflag = false;

        //        //if (LogLevel == ProberLogLevel.PROLOG)
        //        //{
        //        //    if (LogViewModel.MadeHistoryForProlog == false)
        //        //    {
        //        //        LogViewModel.MadeHistoryForProlog = true;
        //        //        updateflag = true;
        //        //    }
        //        //}
        //        //else if (LogLevel == ProberLogLevel.EVENT)
        //        //{
        //        //    if (LogViewModel.MadeHistoryForEventlog == false)
        //        //    {
        //        //        LogViewModel.MadeHistoryForEventlog = true;
        //        //        updateflag = true;
        //        //    }
        //        //}
        //        ////else if (LogLevel == ProberLogLevel.DEBUG)
        //        ////{
        //        ////    if (LogViewModel.MadeHistoryForDebuglog == false)
        //        ////    {
        //        ////        LogViewModel.MadeHistoryForDebuglog = true;
        //        ////        updateflag = true;
        //        ////    }
        //        ////}

        //        //if (updateflag == true)
        //        //{
        //        //    MakeLogHistory();
        //        //}

        //        //LoggerManager.Event("AAA", "BBB", EventlogType.EVENT, new List<string> { "Tag1", "Tag2" });
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}



        private RelayCommand _UpdateLogCommand;
        public ICommand UpdateLogCommand
        {
            get
            {
                if (null == _UpdateLogCommand) _UpdateLogCommand = new RelayCommand(UpdateLogCmd);
                return _UpdateLogCommand;
            }
        }
        private void UpdateLogCmd()
        {
            MakeLogHistory();
        }

        private RelayCommand _DataGridSelectionChangedCommand;
        public ICommand DataGridSelectionChangedCommand
        {
            get
            {
                if (null == _DataGridSelectionChangedCommand) _DataGridSelectionChangedCommand = new RelayCommand(DataGridSelectionChangedCmd);
                return _DataGridSelectionChangedCommand;
            }
        }

        private void DataGridSelectionChangedCmd()
        {
            try
            {
                //var selectedItems = LogHistories.Where(x => x.IsSelected).Select(y => y.Item);

                //SelectedItems = new ObservableCollection<LogDataStructure>(selectedItems.Cast<LogDataStructure>());
                ////SelectedItems = new SynchronizedObservableCollection<LogDataStructure>(selectedItems.Cast<LogDataStructure>());

                ////var aaa = new ObservableCollection<LogDataStructure>(selectedItems.Cast<LogDataStructure>());

                ////SynchronizedObservableCollection<LogDataStructure> tmp = Enumerable.Reverse(aaa);

                //SelectedItems = Enumerable.Reverse(SelectedItems);

                //if (LogType == LogType.EVENT)
                //{
                //    //if(SelectedItem.Item.UserNotified == true)
                //    //{
                //    //    SelectedItem.Item.UserNotified = !SelectedItem.Item.UserNotified;
                //    //}

                //    var eventlog = LogViewModel.EventLogHistories.Where(x => x.GetHashCode() == SelectedItem.Item.HashCode);

                //    eventlog.ElementAt(0).Properties[LoggerManager.EventMarkPropertyName] = false;
                //    LogViewModel.UpdateNotifiedCount();

                //    //var selectedItems = LogHistories.Where(x => x.IsSelected).Select(y => y.Item);

                //    //SelectedItems = new ObservableCollection<LogDataStructure>(selectedItems.Cast<LogDataStructure>());

                //    //SelectedItems = Enumerable.Reverse(SelectedItems);
                //}
                //else
                //{

                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum MakeLogHistory()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            //try
            //{
            //    SynchronizedObservableCollection<LogEventInfo> items = null;
            //    SynchronizedObservableCollection<LogDataStructure> curhistory = null;

            //    if (LogLevel == ProberLogLevel.PROLOG)
            //    {
            //        items = LogViewModel.ProLogHistories;
            //        LogViewModel.LogHistories.ProLogHistories.Clear();
            //        curhistory = LogViewModel.LogHistories.ProLogHistories;
            //    }
            //    else if (LogLevel == ProberLogLevel.EVENT)
            //    {
            //        items = LogViewModel.EventLogHistories;
            //        LogViewModel.LogHistories.EventLogHistories.Clear();
            //        curhistory = LogViewModel.LogHistories.EventLogHistories;

            //        foreach (var item in items)
            //        {
            //            item.Properties[LoggerManager.NotifiedPropertyName] = true;
            //        }

            //        LogViewModel.UpdateNotifiedCount();
            //    }
            //    else if (LogLevel == ProberLogLevel.DEBUG)
            //    {
            //        items = LogViewModel.DebugLogHistories;
            //        LogViewModel.LogHistories.DebugLogHistories.Clear();
            //        curhistory = LogViewModel.LogHistories.DebugLogHistories;
            //    }
            //    else
            //    {
            //        LoggerManager.Error("Unknown ERROR");
            //    }

            //    //if (LogHistories != null && LogHistories.Count > 0)
            //    //{
            //    //    LogHistories.Clear();
            //    //}

            //    //List<LogEventInfo> copyobj = new List<LogEventInfo>();

            //    //copyobj = items.ToList();

            //    if ((items != null) && (curhistory != null))
            //    {
            //        //foreach (var item in Enumerable.Reverse(items))
            //        //{
            //        //    LogDataStructure tmp = new LogDataStructure(item);

            //        //    if (tmp != null)
            //        //    {
            //        //        curhistory.Add(tmp);
            //        //    }
            //        //}

            //        foreach (var item in items)
            //        {
            //            LogDataStructure tmp = new LogDataStructure(item);

            //            if (tmp != null)
            //            {
            //                curhistory.Add(tmp);
            //            }
            //        }
            //    }

            //    //LoggerManager.Debug($"MakeLogHistory() End");

            //    retval = EventCodeEnum.NONE;
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}

            return retval;
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
                    LoggerManager.Error($"ViewModelManager : Can't find View. GUID = {guid}");
                }

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
                //    LoggerManager.Error($"ViewModelManager : Can't find View. GUID = {guid}");
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return T;
        }

        //public IMainScreenView GetView(Guid guid)
        //{
        //    IMainScreenView view = null;
        //    try
        //    {
        //        Type type = GetViewType(guid);
        //        if (type == typeof(IMainScreenView))
        //        {
        //            view = GetScreenView(guid);
        //        }
        //        else if (type == typeof(IMainScreenViewModel))
        //        {
        //            IMainScreenViewModel viewmodel = null;
        //            ScreenViewModelDictionary.TryGetValue(guid, out viewmodel);
        //            if (viewmodel != null)
        //            {
        //                view = ViewAndViewModelDictionary.FirstOrDefault(x => x.Value == viewmodel).Key;
        //            }
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Error($err, "GetView() : Error occurred.");
        //    }

        //    return view;
        //}

        /// <summary>
        /// View / ViewModel 의 GUID 가 들어오면 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Guid GetView(Guid guid)
        {
            Guid viewGuid = new Guid();
            try
            {
                Type type = GetViewType(guid);
                if (type == typeof(IMainScreenView))
                {
                    viewGuid = guid;
                }
                else if (type == typeof(IMainScreenViewModel))
                {
                    IMainScreenViewModel viewmodel = null;
                    ScreenViewModelDictionary.TryGetValue(guid, out viewmodel);
                    if (viewmodel != null)
                    {
                        IMainScreenView view = ViewAndViewModelDictionary.FirstOrDefault(x => x.Value == viewmodel).Key;
                        if (view == null)
                        {
                            Type[] types = viewmodel.GetType().GetInterfaces();
                            foreach (var item in types)
                            {
                                bool falg = ViewGUIDDictionary.TryGetValue(item.Name, out viewGuid);
                                if (falg)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            viewGuid = ScreenViewDictionary.FirstOrDefault(x => x.Value == view).Key;
                        }

                    }
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "GetView() : Error occurred.");
                LoggerManager.Exception(err);
            }

            return viewGuid;
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


        public async Task<EventCodeEnum> BackPreScreenTransition(bool paramvalidation = true)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (PreMainScreenViewList.Count > 0)
                {
                    await ViewTransitionAsync(PreMainScreenViewList[PreMainScreenViewList.Count - 1].ScreenGUID, null, paramvalidation, true);
                    //ViewTransition(PreMainScreenViewlist[PreMainScreenViewlist.Count - 1].ViewGUID, true);
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "BackPreScreenTransition() : Error occured.");
                LoggerManager.Exception(err);

                throw err;
            }

            return ret;
        }

        public async Task<EventCodeEnum> ViewTransitionAsync(Guid guid, object parameter = null, bool paramvalidatiaon = true, bool ChangePrev = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var dialogHash = this.GetHashCode().ToString();
            try
            {
                Type T = GetViewType(guid);

                //await this.WaitCancelDialogService().ShowDialog("Please Wait");
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                if (T != null && T.Name == "IMainScreenView")
                {
                    //await this.MetroDialogManager().ShowWaitCancelDialog(dialogHash, "Wait"); //Wait Message 안없어져서 임시적으로 지움. 다시 살려야 함!!!

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

                        // Login
                        if ((tmp.ScreenGUID).ToString().ToLower() == LOGIN_PAGE_GUID.ToString().ToLower())
                        {
                            MainViewRow = 0;
                            MainViewRowSpan = 4;
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
                                    PreMainScreenViewList.Add(MainScreenView);
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


                        if (CurrentViewModel != null)
                        {

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if ((MainScreenView as UserControl).DataContext is IMainScreenViewModel)
                                    PrevScreenViewModel = (MainScreenView as UserControl).DataContext as IMainScreenViewModel;
                            });

                            if (PrevScreenViewModel != null)
                            {
                                //await Task.Run(async () =>
                                //{
                                await PrevScreenViewModel.Cleanup(EventCodeEnum.NONE);
                                //});
                            }

                            if ((this).LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                            {
                                if (this.MonitoringManager() != null)
                                {
                                    if (this.MonitoringManager().IsSystemError == true)
                                    {

                                    }
                                    else
                                    {
                                        // TODO : 
                                        //(this).StageSupervisor().StageModuleState.ZCLEARED();
                                        //(this).StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                                        MenuLoakables.LockableTrue();
                                    }
                                }
                            }

                            //MenuLoakables.LockableTrue();

                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                MainScreenView = tmp;

                                if ((MainScreenView as UserControl).DataContext != null)
                                {
                                    if ((MainScreenView as UserControl).DataContext.GetHashCode() != CurrentViewModel.GetHashCode())
                                    {
                                        //CurrentViewModel = (IMainScreenViewModel)((MainScreenView as UserControl).DataContext);

                                        if (CurrentViewModel is IPnpSetup == false)
                                        {
                                            (MainScreenView as UserControl).DataContext = CurrentViewModel;
                                        }
                                    }
                                }
                            }));

                            //await ScreenViewModel.PageSwitched();

                            this.VisionManager().AllStageCameraStopGrab();

                            if (parameter != null)
                            {
                                //await Task.Run((Func<Task>)(async () =>
                                //{
                                await CurrentViewModel.PageSwitched(parameter);
                                //}));
                            }
                            //var ret = await Task.Factory.StartNew(async () =>
                            //{
                            else if (CurrentViewModel is IPnpSetup == false)
                            {
                                if (CurrentViewModel is ISetUpState == true)
                                {
                                    this.SysState().SetSetUpState();
                                }
                                else
                                {
                                    this.SysState().SetIdleState();
                                }
                                retVal = await CurrentViewModel.PageSwitched();
                            }
                            else
                            {
                                this.SysState().SetSetUpState();
                                await this.PnPManager().SetDefaultInitViewModel();
                            }
                            //});

                        }
                    }
                }

                // TODO : 왜 필요하지?
                //if (T != null && T.Name == "IMainTopBarView")
                //{
                //    IMainTopBarView tmp;

                //    tmp = GetTopBarView(guid);

                //    MainTopBarView = tmp;
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //throw err; //임시주석
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                //await this.MetroDialogManager().CloseWaitCancelDialaog(dialogHash); //Wait Message 안없어져서 임시적으로 지움. 다시 살려야 함!!!

                //if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE & this.StageSupervisor().MarkObject.GetDoMarkAlign())
                //    //await this.MetroDialogManager().ShowMessageDialog("Error Message", "MarkAlign Fail", EnumMessageStyle.Affirmative);
                //    this.ShowNotification(this.GetHashCode(), "Error Message", "MarkAlign Fail");
            }

            SetOffMaskingRect();

            return retVal;
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

                    //ViewTransition(guid);
                    retVal = await ViewTransitionAsync(guid);
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($err + "ViewTransitionUsingViewModel() : Error occured.");
                    LoggerManager.Exception(err);
                }

                SetOffMaskingRect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void SetPostMainScreenToEmpty()
        {
            PostMainScreenView = null;
        }

        private void SetPostMainScreenInfo()
        {
            try
            {
                PostMainScreenView = MainScreenView;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IMainScreenViewModel FindViewModelObject(Guid viewguid)
        {
            IMainScreenViewModel retViewModel = null;
            try
            {
                IMainScreenView retView = null;
                Type T = GetViewType(viewguid);

                try
                {
                    if (T != null)
                    {
                        if (T.Name == nameof(IMainScreenView))
                        {
                            retView = GetScreenView(viewguid);
                            ViewAndViewModelDictionary.TryGetValue(retView, out retViewModel);
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retViewModel;
        }

        public IMainScreenViewModel GetViewModelFromInterface(Type inttype)
        {
            IMainScreenViewModel retViewModel = null;
            try
            {
                retViewModel = ScreenViewModelDictionary.Values.Single(vm => vm.GetType().ToString().Equals(inttype.ToString()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retViewModel;
        }

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
            Guid viewguid = new Guid();
            try
            {
                var view = ViewConnectParam.ConnectionInfos.Find(connguid => connguid.ViewModelGUID.Equals(guid));
                if (view != null)
                    viewguid = view.ViewGUID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return viewguid;
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
                            this.VisionManager().AllStageCameraStopGrab();

                            if (viewModel != null)
                            {
                                IMainScreenViewModel ScreenViewModel = null;
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    (retView as UserControl).DataContext = viewModel;
                                    ScreenViewModel = (IMainScreenViewModel)((retView as UserControl).DataContext);
                                });
                                //Lock(ScreenViewModel.GetHashCode(), "Wait", "Page Switching");
                                await ScreenViewModel?.PageSwitched(vmParam);
                                //UnLock(ScreenViewModel.GetHashCode());
                            }
                            else
                            {
                                (retView as IMainScreenViewModel)?.InitModule();
                                //Lock(retView.GetHashCode(), "Wait", "Page Switching");

                                Task task = (retView as IMainScreenViewModel)?.PageSwitched(vmParam);

                                if(task != null)
                                {
                                    task.Start();
                                    await task;
                                }

                                //UnLock(retView.GetHashCode());

                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
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
                    //LoggerManager.Error($err + "GetViewObj() : Error occured.");
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

        private void SetOffMaskingRect()
        {
            VMFilterPanel.RequestDisableMode();
        }

        public EventCodeEnum SetDataContext(object obj)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((() =>
                    {
                        if (MainScreenView != null)
                        {
                            (MainScreenView as UserControl).DataContext = obj;
                        }
                    }));

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

        public EventCodeEnum LoadViewDLLParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new ViewDLLParam();
                RetVal = this.LoadParameter(ref tmpParam, typeof(ViewDLLParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    ViewParam = tmpParam as ViewDLLParam;
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

        public EventCodeEnum LoadViewModelDLLParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new ViewModelDLLParam();
                RetVal = this.LoadParameter(ref tmpParam, typeof(ViewModelDLLParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    ViewModelParam = tmpParam as ViewModelDLLParam;
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

                ViewConnectParam = new ViewAndViewModelConnectParam();

                string fullPath = this.FileManager().GetSystemParamFullPath(ViewConnectParam.FilePath, ViewConnectParam.FileName);

                try
                {
                    tmpParam = new ViewAndViewModelConnectParam();
                    RetVal = this.LoadParameter(ref tmpParam, typeof(ViewAndViewModelConnectParam));

                    if (RetVal == EventCodeEnum.NONE)
                    {
                        ViewConnectParam = tmpParam as ViewAndViewModelConnectParam;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($"[ViewModelManager] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
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

        public EventCodeEnum LoadViewGUIDAndVMTypeParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    IParam tmpParam = null;

                    RetVal = this.LoadParameter(ref tmpParam, typeof(ViewGUIDAndViewModelConnectParam));

                    if (RetVal == EventCodeEnum.NONE)
                    {
                        ViewGUIDAndVMTypeParam = tmpParam as ViewGUIDAndViewModelConnectParam;
                        foreach (var item in ViewGUIDAndVMTypeParam.ConnectionInfos)
                        {
                            ViewGUIDDictionary.Add(item.ViewModelInterface, item.ViewGUID);
                        }
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($"[ViewModelManager] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
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

        public EventCodeEnum SaveSettingViewParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                string fullPath = this.FileManager().GetSystemParamFullPath(SettingViewParameter.FilePath, SettingViewParameter.FileName);

                try
                {
                    RetVal = this.SaveParameter(SettingViewParameter);
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($"[ViewModelManager] SaveSettingViewParam(): Error occurred while saveing parameters. Err = {0}", err.Message));
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

        public EventCodeEnum GetProberViewTypes()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (ProberViewTypes == null)
                {
                    ProberViewTypes = new List<Type>();
                }

                string ProberVMDllName = "ProberViewModel.dll";

                //string pluginPath = System.Environment.CurrentDirectory + "\\" + ProberVMDllName;
                string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProberVMDllName);

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

                string ProberVMDllName = "ProberViewModel.dll";

                //string pluginPath = System.Environment.CurrentDirectory + "\\" + ProberVMDllName;
                string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProberVMDllName);

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

        public static object Cast(object obj, Type t)
        {
            try
            {
                var param = Expression.Parameter(obj.GetType());
                return Expression.Lambda(Expression.Convert(param, t), param)
                         .Compile().DynamicInvoke(obj);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public static IEnumerable<FieldInfo> GetAllFields(Type t, BindingFlags flags)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            return t.GetFields(flags).Concat(GetAllFields(t.BaseType, flags));
        }


        //public static IList<FieldInfo> GetAllFields(this Type type, BindingFlags flags)
        //{
        //    if (type == typeof(Object)) return new List<FieldInfo>();

        //    var list = type.BaseType.GetAllFields(flags);
        //    // in order to avoid duplicates, force BindingFlags.DeclaredOnly
        //    list.AddRange(type.GetFields(flags | BindingFlags.DeclaredOnly));
        //    return list;
        //}


        private EventCodeEnum CreateViewInstance()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                object tmpView = null;

                RetVal = GetProberViewTypes();

                Type tmpvm = null;

                foreach (var list in ViewParam.info)
                {
                    try
                    {
                        string assemblyName = list.AssemblyName;

                        if (list.ClassName != null && list.ClassName.Count > 0)
                        {
                            string className = list.ClassName[0];

                            tmpvm = ProberViewTypes.Find(x => x.Name == className);

                            if (tmpvm != null)
                            {
                                tmpView = Activator.CreateInstance(tmpvm);

                                if (tmpView != null)
                                {
                                    if (tmpView is IMainScreenView)
                                    {
                                        IMainScreenView tmp = (tmpView as IMainScreenView);

                                        UserControl usercontrol = tmp as UserControl;
                                        if (usercontrol != null)
                                        {
                                            usercontrol.Loaded += ViewPanel_Loaded;
                                        }

                                        if (!ScreenViewDictionary.ContainsKey(tmp.ScreenGUID))
                                        {
                                            ScreenViewDictionary.Add(tmp.ScreenGUID, tmp);
                                        }
                                    }

                                    //if (tmpView is IMainTopBarView)
                                    //{
                                    //    IMainTopBarView tmp = (tmpView as IMainTopBarView);

                                    //    UserControl usercontrol = tmp as UserControl;
                                    //    if (usercontrol != null)
                                    //    {
                                    //        usercontrol.Loaded += ViewPanel_Loaded;
                                    //    }

                                    //    TopBarViewDictionary.Add(tmp.ViewGUID, tmp);
                                    //}
                                }
                                else
                                {
                                    LoggerManager.Debug($"[{assemblyName}] instance does not created.");
                                }
                            }
                        }

                        //string path = list.DLLPath + list.AssemblyName;

                        //tmpView = GetDynamicLoadedView(path);

                        //if (tmpView != null)
                        //{
                        //    if (tmpView is IMainScreenView)
                        //    {
                        //        IMainScreenView tmp = (tmpView as IMainScreenView);

                        //        UserControl usercontrol = tmp as UserControl;
                        //        if (usercontrol != null)
                        //        {
                        //            usercontrol.Loaded += ViewPanel_Loaded;
                        //        }



                        //        ScreenViewDictionary.Add(tmp.ViewGUID, tmp);
                        //    }

                        //    if (tmpView is IMainTopBarView)
                        //    {
                        //        IMainTopBarView tmp = (tmpView as IMainTopBarView);

                        //        UserControl usercontrol = tmp as UserControl;
                        //        if (usercontrol != null)
                        //        {
                        //            usercontrol.Loaded += ViewPanel_Loaded;
                        //        }

                        //        TopBarViewDictionary.Add(tmp.ViewGUID, tmp);
                        //    }
                        //}
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



        private void ViewPanel_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UserControl panel = sender as UserControl;
                ApplyMasking(panel);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ApplyMasking(UserControl control)
        {
            try
            {
                IEnumerable<FrameworkElement> elementList = VisualChildrenHelper.FindVisualChildren<FrameworkElement>(control);
                foreach (var element in elementList)
                {
                    ICUIControl cuiControl = element as ICUIControl;

                    if (cuiControl != null)
                    {
                        CUIServices.CUIService.ApplyMasking(cuiControl);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
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

                    Type tmpvm = null;

                    try
                    {
                        string assemblyName = list.AssemblyName;
                        ass = assemblyName;

                        if (list.ClassName != null && list.ClassName.Count > 0)
                        {
                            string className = list.ClassName[0];

                            tmpvm = ProberViewModelTypes.Find(x => x.Name == className);

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
                                    //    TopBarViewModelDictionary.Add(topbarvm.ViewModelGUID, topbarvm);

                                    //    retval = topbarvm.InitModule();

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

                        //if ("OCRViewModel.dll" == assemblyName)
                        //{ }

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
                        //        IMainTopBarViewModel topbarvm = (tmpViewModel as IMainTopBarViewModel);
                        //        TopBarViewModelDictionary.Add(topbarvm.ViewModelGUID, topbarvm);

                        //        //tmp.SetContainer(Container);
                        //        retval = topbarvm.InitModule();

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
                        LoggerManager.Debug($"[{list.ClassName[0]}] instance does not created.");
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
                    //IMainTopBarView TopBarView = null;
                    //IMainTopBarViewModel TopBarViewModel = null;

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

                            bool exist = ViewAndViewModelDictionary.TryGetValue(ScreenView, out IMainScreenViewModel tmpScreenViewModel);

                            if (exist)
                            {
                                LoggerManager.Error($"[ViewModelManager] ConnectDataContext() : Already been added({ScreenView}), GUID = {item.ViewGUID}");
                            }
                            else
                            {
                                ViewAndViewModelDictionary.Add(ScreenView, ScreenViewModel);
                            }
                        }
                        else
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                        }
                    }
                    else
                    {
                        RetVal = EventCodeEnum.PARAM_ERROR;
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
                    //else
                    //{
                    //    RetVal = EventCodeEnum.PARAM_ERROR;
                    //}
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($$"[ViewModelManager] ConnectDataContext(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);


                throw;
            }

            return RetVal;
        }

        public Type GetTypeViewOrViewModel(Guid guid)
        {
            Type T = null;

            try
            {
                bool ContainViewFlag = false;
                bool ContainViewModelFlag = false;

                ContainViewFlag = ScreenViewDictionary.ContainsKey(guid);
                ContainViewModelFlag = ScreenViewModelDictionary.ContainsKey(guid);

                if ((ContainViewFlag == true) && (ContainViewModelFlag == true))
                {
                    // ERROR
                }
                else if (ContainViewFlag == true)
                {
                    T = typeof(IMainScreenView);
                }
                else if (ContainViewModelFlag == true)
                {
                    T = typeof(IMainScreenViewModel);
                }
                else
                {
                    // ERROR
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return T;
        }

        public EventCodeEnum ConnectDataContext(Guid viewmodelguid)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IMainScreenViewModel ViewModel = null;
                IMainScreenView View = null;
                bool ret = ScreenViewModelDictionary.TryGetValue(viewmodelguid, out ViewModel);
                if (ret == true)
                {
                    var nestedTypes = ViewModel.GetType().GetInterfaces().Where(x => typeof(IMainScreenViewModel).IsAssignableFrom(x)).ToList();
                    string tmpKey = null;

                    foreach (var item in nestedTypes)
                    {
                        if (item.Name != typeof(IMainScreenViewModel).Name)
                        {
                            tmpKey = item.Name;

                            break;
                        }
                    }

                    if (tmpKey != null)
                    {
                        Guid viewGuid;
                        bool ret1 = ViewGUIDDictionary.TryGetValue(tmpKey, out viewGuid);
                        if (ret1 == true)
                        {
                            bool ret2 = ScreenViewDictionary.TryGetValue(viewGuid, out View);
                            if (ret2 == true)
                            {
                                if (View is UserControl)
                                {
                                    (View as UserControl).DataContext = ViewModel;
                                }
                                else
                                {
                                    RetVal = EventCodeEnum.PARAM_ERROR;
                                }
                            }
                            else
                            {
                                RetVal = EventCodeEnum.PARAM_ERROR;
                            }
                        }
                        else
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                        }

                    }
                    else
                    {
                        RetVal = EventCodeEnum.PARAM_ERROR;
                    }
                }
                else
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($$"[ViewModelManager] ConnectDataContext(Guid guid): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);


                throw;
            }

            return RetVal;
        }

        public ViewModelManager()
        {
            try
            {
                VMFilterPanel = new FilterPanelViewModel();
                _dailogService = new NotificationDialogService();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    SelectedItems = new ObservableCollection<object>();

                    StageSupervisor = this.StageSupervisor();

                    LogViewModel = new LogVM();
                    LogViewModel.InitModule();

                    //if (LogHistories == null)
                    //{
                    //    LogHistories = new SynchronizedObservableCollection<LogDataStructure>();
                    //}

                    //if (FilteredLogHistories == null)
                    //{
                    //    FilteredLogHistories = new SynchronizedObservableCollection<LogDataStructure>();
                    //}

                    LogLevel = ProberLogLevel.PROLOG;

                    this.Stage3DModel = new Viewer3DModelV();
                    (Stage3DModel as UserControl).DataContext = this;
                    MenuLoakables = new MenuLockable();
                    MonitoringManager = this.MonitoringManager();
                    MotionManager = this.MotionManager();
                    retval = LoadViewDLLParam();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadViewDLLParam() Failed");
                    }

                    retval = LoadViewModelDLLParam();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadViewModelDLLParam() Failed");
                    }

                    retval = LoadViewAndViewModelConnectParam();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadViewAndViewModelConnectParam() Failed");
                    }

                    retval = LoadViewGUIDAndVMTypeParam();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadViewGUIDAndVMTypeParam() Failed");
                    }

                    retval = LoadSettingViewParam();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"LoadSettingViewParam() Failed");
                    }

                    retval = CreateViewInstance();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"CreateViewInstance() Failed");
                    }

                    retval = CreateViewModelInstance();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"CreateViewModelInstance() Failed");
                    }

                    retval = ConnectDataContext();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"ConnectDataContext() Failed");
                    }

                    IParam tmpParam = null;
                    tmpParam = new LoginParameter();
                    retval = this.LoadParameter(ref tmpParam, typeof(LoginParameter));

                    LoginSkipEnable = (tmpParam as LoginParameter).LoginSkipEnable;

                    // TODO : 순서 주의, 필요한 GUID를 먼저 SET 해줘야 됨.
                    SetSystemGUID();

                    IMainScreenView tempScreenView = null;

                    // Assign - TopBar
                    tempScreenView = GetScreenView(TopBarViewGuid);

                    MainTopBarView = tempScreenView;

                    // LOGIN Page
                    //InitScreenViewGuid = new Guid(LOGIN_PAGE_GUID);

                    //InitTopBarViewGuid = new Guid(MAIN_TOP_BAR_GUID);
                    //InitViewModelGUIDForWidget = new Guid(LOT_PAGE_GUID);
                    //OperatorViewGuid = new Guid(OPERATOR_PAGE_GUID);

                    //StatisticsErrorViewGuid = new Guid(STATISTICS_ERROR_PAGE_GUID);

                    //IMainScreenView ScreenView = null;
                    //ScreenViewDictionary.TryGetValue(OperatorViewGuid, out ScreenView);
                    //if (ScreenView != null)
                    //{
                    //    OperatorView = ScreenView;
                    //}

                    //ScreenViewDictionary.TryGetValue(InitScreenViewGuid, out ScreenView);
                    //if (ScreenView != null)
                    //{
                    //    ViewTransition(ScreenView.ViewGUID);
                    //}

                    //IMainTopBarView TopBarView = null;
                    //TopBarViewDictionary.TryGetValue(InitTopBarViewGuid, out TopBarView);

                    //if (TopBarView != null)
                    //{
                    //    ViewTransition(TopBarView.ViewGUID);
                    //}

                    // 250911 LJH LJH Widget 안보이게
                    //widget = new MainWindowWidget();
                    //widget.MainHandle = Application.Current.MainWindow;

                    //var vm = GetViewModelFromViewGuid(HomeViewGuid);

                    //if (vm != null)
                    //{
                    //    widget.DataContext = vm;
                    //}
                    //////////





                    //IMainScreenViewModel ScreenViewModel = null;
                    //ScreenViewModelDictionary.TryGetValue(InitViewModelGUIDForWidget, out ScreenViewModel);
                    //widget.DataContext = ScreenViewModel;

                    IMainScreenViewModel diagnosisViewModel = null;
                    ScreenViewModelDictionary.TryGetValue(new Guid("0E719D6C-C283-4643-9AFB-B2C1465892C8"), out diagnosisViewModel);
                    DiagnosisViewModel = diagnosisViewModel;

                    Motion = this.MotionManager();
                    LoaderController = this.LoaderController() as ILoaderControllerExtension;

                    if (Motion != null)
                    {
                        Z1 = Motion.GetAxis(EnumAxisConstants.Z0);
                        Z2 = Motion.GetAxis(EnumAxisConstants.Z1);
                        Z3 = Motion.GetAxis(EnumAxisConstants.Z2);

                        U1 = Motion.GetAxis(EnumAxisConstants.U1);
                        U2 = Motion.GetAxis(EnumAxisConstants.U2);
                        LZ = Motion.GetAxis(EnumAxisConstants.A);
                        LW = Motion.GetAxis(EnumAxisConstants.W);
                        XAxis = Motion.GetAxis(EnumAxisConstants.X);
                        YAxis = Motion.GetAxis(EnumAxisConstants.Y);
                        ZAxis = Motion.GetAxis(EnumAxisConstants.Z);
                        TAxis = Motion.GetAxis(EnumAxisConstants.C);
                        MNCAxis = Motion.GetAxis(EnumAxisConstants.NC);
                        PZAxis = Motion.GetAxis(EnumAxisConstants.PZ);
                        SC = Motion.GetAxis(EnumAxisConstants.SC);
                    }

                    ThreeLegHeight = 0.0d;
                    FoupCoverHeight = -380d;
                    FoupCoverPos = -40d;

                    ZUpState = this.ProbingModule().ProbingStateEnum;

                    if (this.FoupOpModule() != null)
                    {
                        FoupController = this.FoupOpModule().GetFoupController(1);
                    }

                    if (this.IOManager() != null)
                    {
                        IsWaferBridgeExtended = this.IOManager().IO.Outputs.DOWAFERMIDDLE;
                        IsInspectionTrayExtended = this.IOManager().IO.Inputs.DIDRAWEROPEN;
                        IsWaferOnInspectionTray = this.IOManager().IO.Inputs.DIWAFERONDRAWER;
                        DOSCAN_SENSOR_OUT = this.IOManager().IO.Outputs.DOSCAN_SENSOR_OUT;
                        DOLOADER_DOOR = this.IOManager().IO.Outputs.DOLOADERDOOR_OPEN;
                    }

                    MapViewControl = new MapView.MapViewControl();
                    MapViewControlFD = new MapView.MapViewControl();
                    NeedleCleanView = new NeedleCleanViewer.NeedleCleanView();

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

        public void SetSystemGUID()
        {
            try
            {
                HomeViewGuid = new Guid("6223dfd5-efaa-4b49-ab70-d8a5f03fa65d");
                TopBarViewGuid = new Guid("EC8FB988-222F-1E88-2C18-6DF6A742B3E9");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> InitScreen()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IMainScreenView ScreenView = null;

                ScreenViewDictionary.TryGetValue(OPERATOR_PAGE_GUID, out ScreenView);

                if (ScreenView != null)
                {
                    OperatorView = ScreenView;
                }

                ScreenViewDictionary.TryGetValue(LOGIN_PAGE_GUID, out ScreenView);

                if (ScreenView != null)
                {
                    await ViewTransitionAsync(ScreenView.ScreenGUID);
                }

                //IMainTopBarView TopBarView = null;
                //TopBarViewDictionary.TryGetValue(InitTopBarViewGuid, out TopBarView);

                //ScreenViewDictionary.TryGetValue(TopBarViewGuid, out ScreenView);

                //if (ScreenView != null)
                //{
                //    await ViewTransitionAsync(ScreenView.ScreenGUID);

                //    //ViewTransitionAsync(TopBarView.ViewGUID);
                //}

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private bool _IsWidgetScreen;
        public bool IsWidgetScreen()
        {
            return _IsWidgetScreen;
        }
        public void SetIsWidgetScreen(bool flag)
        {
            _IsWidgetScreen = flag;
        }

        public async void UpdateCurMainViewModel()
        {
            try
            {

                if (PreMainScreenViewList.Count != 0 && PreMainScreenViewList[0] != null)
                {
                    this.LotOPModule().ViewTarget = null;
                    var previewmodel = GetViewModelFromViewGuid(PreMainScreenViewList[0].ScreenGUID);
                    //await previewmodel?.PageSwitched();
                    await ViewTransitionAsync(PreMainScreenViewList[0].ScreenGUID);
                }

                // TODO : 
                if (this.MetroDialogManager().IsShowingWaitCancelDialog() == true)
                {
                    // CLOSE
                    await this.MetroDialogManager().CloseWindow("WaitCancelDialog");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Cell Program 조작으로인한 이슈들에 대한 방안으로 셀프로그램을 위젯상태에서 푸는것에 대한 제한을 추가하려고 합니다.
        ///현재 구상중인 컨셉은<cf.Cell Program 위젯 Lock(위젯상태), Unlock(위젯이 풀린 상태) 으로 표현>
        ///1. 로더와 연결이 안된상태
        ///  1)치트키를 두어서 해당 차트키 사용시에만 위젯 Unlock할 수 있음
        ///  2)Auto Soaking 이 동작중(pin align, chuck focusing, move to soaking position) 이면 피트키를 써도 message 띄우고 위젯 lock 상태유지
        ///2. 로더와 연결된상태
        ///  1)치트키를 두어서 해당 차트키 사용시에만 위젯 Unlock할 수 있음(Maintenance , Offline 의경우)
        ///  2)Online mode 로 변경될경우, Unlock 이었어도 Lock 상태로 강제전환. (message 띄워줌)
        /// </summary>
        /// <returns></returns>
        public Task<bool> CheckUnlockWidget()
        {
            bool retVal = false;
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.C))
                {
                    if (this.SoakingModule().GetModuleMessage().Equals(SoakingStateEnum.AUTOSOAKING_RUNNING.ToString())
                        || this.SoakingModule().ModuleState.GetState() == ModuleStateEnum.RUNNING
                        || this.SoakingModule().ModuleState.GetState() == ModuleStateEnum.SUSPENDED
                        )
                    {
                        string title = "Error Message";
                        string message = $"[Cell#{this.LoaderController().GetChuckIndex()}]\r\n" +
                            $"Auto Soaking is processing.\r\n" +
                            "Can't turn off the widget.";
                        MessageBox.Show(message, title);
                        //await this.MetroDialogManager().ShowMessageDialog(title, message, MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        retVal = false;
                    }
                    else if(this.LoaderController().GetconnectFlag() && this.StageSupervisor.StageMode == GPCellModeEnum.ONLINE)
                    {
                        string title = "Error Message";
                        string message = $"[Cell#{this.LoaderController().GetChuckIndex()}]\r\n" +
                            $"The loader and cell are connected in online mode.\r\n" +
                            "Can't turn off the widget.";
                        MessageBox.Show(message, title);
                        //await this.MetroDialogManager().ShowMessageDialog(title, message, MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        retVal = false;
                    }
                    else
                    {
                        retVal = true;
                    }
                }
                else
                {
                    string title = "Error Message";
                    string message = $"[Cell#{this.LoaderController().GetChuckIndex()}]\r\n" +
                        $"Can't turn off the widget.\r\n" +
                        "Can control in main program.";
                    MessageBox.Show(message, title);
                    retVal = false;
                    //await this.MetroDialogManager().ShowMessageDialog(title, message, MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<bool>(retVal);
        }

        public void UpdateWidget()
        {
            //if(widget!=null)
            //{
            //    widget = null;
            //}
            //widget = new MainWindowWidget();
            //widget.MainHandle = Application.Current.MainWindow;

            try
            {
                LoggerManager.Debug($"UpdateWidget() called.");

                // 250911 LJH Widget 안보이게. (return 추가)
                return;
                //

                //IMainScreenViewModel ScreenViewModel = null;
                ((widget.MainHandle.DataContext as IViewModelManager)?.MainScreenView as UserControl).DataContext = null;
                //  widget.MainHandle.DataContext = null;

                var vm = GetViewModelFromViewGuid(HomeViewGuid);

                if (vm != null)
                {
                    widget.DataContext = vm;
                }

                //ScreenViewModelDictionary.TryGetValue(HomeViewGuid, out ScreenViewModel);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InsertView(IMainScreenView view)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            ScreenViewDictionary.Add(view.ScreenGUID, view);
            return retVal;
        }
        public EventCodeEnum InsertConnectView(IMainScreenView view, IMainScreenViewModel viewModel)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            ViewAndViewModelDictionary.Add(view, viewModel);
            return retVal;
        }

        private IFoupController _FoupController;
        public IFoupController FoupController
        {
            get { return _FoupController; }
            set
            {
                if (value != _FoupController)
                {
                    _FoupController = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region //..Template

        public EventCodeEnum RegisteViewInstance(UCControlInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (info != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        object tmpView = null;
                        tmpView = GetDynamicLoadedView(info.DllInfo.AssemblyName);

                        if (tmpView != null)
                        {
                            if (tmpView is IMainScreenView)
                            {
                                IMainScreenView tmp = (tmpView as IMainScreenView);

                                UserControl usercontrol = tmp as UserControl;
                                if (usercontrol != null)
                                {
                                    usercontrol.Loaded += ViewPanel_Loaded;
                                }

                                IMainScreenView existview = null;
                                ScreenViewDictionary.TryGetValue(info.DllGuid, out existview);
                                if (existview != null)
                                    ScreenViewDictionary.Remove(info.DllGuid);

                                ScreenViewDictionary.Add(tmp.ScreenGUID, tmp);
                                //LoggerManager.Debug($"######RegisteViewInstance#### = {info.DllInfo.AssemblyName}");
                            }
                        }
                    });

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum RegisteViewModelInstance(UCControlInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (info != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        object tmpViewModel = null;

                        tmpViewModel = GetDynamicLoadedViewModel(info.DllInfo.AssemblyName);

                        if (tmpViewModel != null)
                        {
                            if (tmpViewModel is IMainScreenViewModel)
                            {
                                IMainScreenViewModel tmp = (tmpViewModel as IMainScreenViewModel);

                                IMainScreenViewModel existvm = null;

                                ScreenViewModelDictionary.TryGetValue(info.DllGuid, out existvm);

                                if (existvm != null)
                                    ScreenViewModelDictionary.Remove(info.DllGuid);

                                ScreenViewModelDictionary.Add(tmp.ScreenGUID, tmp);

                                tmp.InitModule();

                                Application.Current.Dispatcher.Invoke(delegate
                                {
                                    tmp.InitViewModel();
                                });
                                //LoggerManager.Debug($"######RegisteViewModelInstance#### = {info.DllInfo.AssemblyName}");

                            }

                        }
                    });

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ConnectControlInstances(Guid viewguid, Guid viewmodelguid)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (viewguid != null && viewmodelguid != null)
                {
                    IMainScreenView existview = null;
                    ScreenViewDictionary.TryGetValue(viewguid, out existview);

                    IMainScreenViewModel existvm = null;
                    ScreenViewModelDictionary.TryGetValue(viewmodelguid, out existvm);

                    IMainScreenViewModel vm = null;
                    ViewAndViewModelDictionary.TryGetValue(existview, out vm);
                    if (vm != null)
                        ViewAndViewModelDictionary.Remove(existview);

                    ViewAndViewModelDictionary.Add(existview, existvm);
                }


            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return retVal;
        }

        #endregion


        #region //..ViewControl
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

        private INeedleCleanView _NeedleCleanView;
        public INeedleCleanView NeedleCleanView
        {
            get { return _NeedleCleanView; }
            set
            {
                if (value != _NeedleCleanView)
                {
                    _NeedleCleanView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IWaferObject _WaferObject;
        public IWaferObject WaferObject
        {
            get { return _WaferObject; }
            set
            {
                if (value != _WaferObject)
                {
                    _WaferObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IProbingModule _ProbingModule;
        public IProbingModule ProbingModule
        {
            get { return _ProbingModule; }
            set
            {
                if (value != _ProbingModule)
                {
                    _ProbingModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion


        private bool _LoadProgramFalg = false;
        public void SetLoadProgramFlag(bool loadflag)
        {
            _LoadProgramFalg = loadflag;
        }
        public bool GetLoadProgramFlag()
        {
            return _LoadProgramFalg;
        }

        //public Assembly GetViewAssemblyForViewModel(object viewmodel)
        //{
        //    Assembly assmbly = null;

        //    IMainScreenViewModel tmpVm;

        //    if (viewmodel is IMainScreenViewModel)
        //    {
        //        tmpVm = viewmodel as IMainScreenViewModel;

        //        Guid viewguid = ViewConnectParam.ConnectionInfos.Find(x => x.ViewModelGUID == tmpVm.ViewModelGUID).ViewGUID;

        //        IMainScreenView tmpview;

        //        bool t = ScreenViewDictionary.TryGetValue(viewguid, out tmpview);

        //        AppDomain currentDomain = AppDomain.CurrentDomain;
        //        Evidence asEvidence = currentDomain.Evidence;
        //        Assembly[] assems = currentDomain.GetAssemblies();

        //        if (t == true)
        //        {
        //            var ret = assems.FirstOrDefault(x => x.GetName().Name == tmpview.GetType().Assembly.GetName().Name);

        //            if (ret != null)
        //            {
        //                assmbly = ret;
        //            }
        //        }
        //    }
        //    else
        //    {

        //    }
        //    //if (ScreenViewDictionary.TryGetValue(tag, out string myValue))
        //    //{
        //    //    // use myValue;
        //    //}
        //    return assmbly;
        //}


        public void Set3DCamPosition(CameraViewPoint camViewPoint, bool IsItDisplayed2RateMagnification)
        {
            if (IsItDisplayed2RateMagnification)
            {
                switch (camViewPoint)
                {
                    case CameraViewPoint.FRONT:
                        CamPosition = new Point3D(123, 1148, 1412);
                        CamLookDirection = new Vector3D(0.0, -0.6, -0.8);
                        CamUpDirection = new Vector3D(0.0, 0.8, -0.6);
                        break;
                    case CameraViewPoint.FOUP:
                        CamPosition = new Point3D(1208, 1148, 911);
                        CamLookDirection = new Vector3D(-0.6, -0.6, -0.5);
                        CamUpDirection = new Vector3D(-0.5, 0.8, -0.4);
                        break;
                    case CameraViewPoint.LOADER:
                        CamPosition = new Point3D(1542, 1148, 3);
                        CamLookDirection = new Vector3D(-0.8, -0.6, 0.0);
                        CamUpDirection = new Vector3D(-0.6, 0.8, 0.0);
                        break;
                    case CameraViewPoint.LOADER_BACK:
                        CamPosition = new Point3D(1041, 1148, -1083);
                        CamLookDirection = new Vector3D(-0.5, -0.6, 0.6);
                        CamUpDirection = new Vector3D(-0.4, 0.8, 0.5);
                        break;
                    case CameraViewPoint.BACK:
                        CamPosition = new Point3D(133, 1148, -1417);
                        CamLookDirection = new Vector3D(0.0, -0.6, 0.8);
                        CamUpDirection = new Vector3D(0.0, 0.8, 0.6);
                        break;
                    case CameraViewPoint.STAGE_BACK:
                        CamPosition = new Point3D(-1095, 1148, -714);
                        CamLookDirection = new Vector3D(0.7, -0.6, 0.4);
                        CamUpDirection = new Vector3D(0.5, 0.8, 0.3);
                        break;
                    case CameraViewPoint.STAGE_1:
                        CamPosition = new Point3D(-1287, 1148, -7);
                        CamLookDirection = new Vector3D(0.8, -0.6, 0.0);
                        CamUpDirection = new Vector3D(0.6, 0.8, 0.0);
                        break;
                    case CameraViewPoint.STAGE_2:
                        CamPosition = new Point3D(-876, 1148, 994);
                        CamLookDirection = new Vector3D(0.6, -0.6, -0.5);
                        CamUpDirection = new Vector3D(0.4, 0.8, -0.4);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (camViewPoint)
                {
                    case CameraViewPoint.FRONT:
                        CamPosition = new Point3D(121.4, 1487.7, 1830.4);
                        CamLookDirection = new Vector3D(0, -0.7, -0.8);
                        CamUpDirection = new Vector3D(0, 0.8, -0.6);
                        break;
                    case CameraViewPoint.FOUP:
                        CamPosition = new Point3D(1419.1, 1487.7, 1298.2);
                        CamLookDirection = new Vector3D(-0.5, -0.6, -0.6);
                        CamUpDirection = new Vector3D(-0.4, 0.8, -0.4);
                        break;
                    case CameraViewPoint.LOADER:
                        CamPosition = new Point3D(1960.4, 1487.7, 4.2);
                        CamLookDirection = new Vector3D(-0.8, -0.6, 0);
                        CamUpDirection = new Vector3D(-0.6, 0.8, 0);
                        break;
                    case CameraViewPoint.LOADER_BACK:
                        CamPosition = new Point3D(1535.8, 1487.7, -1175.2);
                        CamLookDirection = new Vector3D(-0.6, -0.6, 0.5);
                        CamUpDirection = new Vector3D(-0.5, 0.8, 0.4);
                        break;
                    case CameraViewPoint.BACK:
                        CamPosition = new Point3D(134.3, 1487.7, -1834.7);
                        CamLookDirection = new Vector3D(0, -0.6, 0.8);
                        CamUpDirection = new Vector3D(0, 0.8, 0.6);
                        break;
                    case CameraViewPoint.STAGE_BACK:
                        CamPosition = new Point3D(-1271.8, 1487.7, -1185);
                        CamLookDirection = new Vector3D(0.6, -0.6, 0.5);
                        CamUpDirection = new Vector3D(0.5, 0.8, 0.4);
                        break;
                    case CameraViewPoint.STAGE_1:
                        CamPosition = new Point3D(-1704.7, 1487.7, -8.6);
                        CamLookDirection = new Vector3D(0.8, -0.6, 0);
                        CamUpDirection = new Vector3D(0.6, 0.8, 0);
                        break;
                    case CameraViewPoint.STAGE_2:
                        CamPosition = new Point3D(-1055, 1487.7, 1397.5);
                        CamLookDirection = new Vector3D(0.5, -0.6, -0.6);
                        CamUpDirection = new Vector3D(0.4, 0.8, -0.5);
                        break;
                    default:
                        break;
                }
            }
        }

        public void WriteCurrentViewAndViewModelName()
        {
            LoggerManager.Debug($"Current View's name : {MainScreenView.GetType().Name}");
            LoggerManager.Debug($"Current ViewModel's name : {(MainScreenView as UserControl).DataContext.GetType().Name}");
        }

        public void SetMainScreenView(IMainScreenView mainScreenView)
        {
            this.MainScreenView = mainScreenView;
        }
    }
}
using LogModule;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LoaderSystem
{
    using Autofac;
    using BarcordReaderView;
    using Cognex.Controls;
    using CUIServices;
    using DBManagerModule;
    using EmulGemView;
    using EnvControlWindow.GP;
    using LoaderBase;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LoaderMapView;
    using LoaderMaster;
    using LoaderOperateViewModelModule;
    using LoaderParameters;
    using LoaderParameterSettingView;
    using LoaderServiceBase;
    using MaterialDesignThemes.Wpf;
    using MetroDialogInterfaces;
    using PnpControlViewModel;
    using Pranas;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.DialogControl;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.ProberSystem;
    using ProberInterfaces.Utility;
    using ProberViewModel;
    using RelayCommandBase;
    using SecsGemSettingDlg;
    using SplasherService;
    using SplashWindowControl;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Threading;
    using AccountModule;
    using UtilityDialogs.Account;
    using VirtualKeyboardControl;
    using RepeatedTransferDialog;
    using TestSimulationDialog;
    using E84SimulatorDialog;
    using SignalTowerDialogServiceProvider;
    using NotifyEventModule;
    using ProberInterfaces.Event;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged, IFactoryModule, IReleaseResource
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Autofac.IContainer container;
        private ILoaderModule loaderModule => container.Resolve<ILoaderModule>();

        private ILoaderSupervisor loaderMaster => container.Resolve<ILoaderSupervisor>();
        private ILoaderCommunicationManager CommunicationManager => container.Resolve<ILoaderCommunicationManager>();
        private ILoaderDoorDisplayDialogService LoaderdoorDlg => container.Resolve<ILoaderDoorDisplayDialogService>();

        private ICardChangeSupervisor cardChangeSupervisor => container.Resolve<ICardChangeSupervisor>();

        #region //Properties
        public ILoaderServiceCallback LoaderCallback { get; set; }
        public ILoaderService LoaderService { get; set; }
        public IGPLoaderService GPLoaderService { get; set; }
        public ILoaderService DAL { get; set; }
        private IGPLoader GPLoader
        {
            get { return container.Resolve<IGPLoader>(); }
        }
        public IViewModelManager ViewModelManager
        {
            get { return container.Resolve<IViewModelManager>(); }
        }

        public ILoaderViewModelManager LoaderViewModelManager
        {
            get { return container.Resolve<ILoaderViewModelManager>(); }
        }

        public IPnpManager PnpManager => container.Resolve<IPnpManager>();

        public IParamManager ParamManager
        {
            get { return container.Resolve<IParamManager>(); }
        }

        private string _PipeName = "Service Host: Not initialized.";
        public string PipeName
        {
            get { return _PipeName; }
            set
            {
                if (value != _PipeName)
                {
                    _PipeName = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region //..View Model
        public LoaderOperateViewModel loaderOperationViewModel { get; set; }

        public LoaderMapViewModel loaderMapViewModel { get; set; }
        public LoaderParameterViewModel loaderParameterViewModel { get; set; }

        public PnpControlVM pnpViewModel { get; set; }

        public TemplateTransferObjectVM TransferObjectViewModel { get; set; }

        public GPFoupSettingVM GPFoupSettingViewModel { get; set; }

        public GPLoaderJobVM GPLoaderJobViewModel { get; set; }


        #endregion
        #region // Dialogs
        TesterCoolantControlDialog.MainWindow tccWindow;

        #endregion
        private string _Version;
        public string Version
        {
            get { return _Version; }
            set
            {
                if (value != _Version)
                {
                    _Version = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool loadingVisible;
        public bool LoadingVisible
        {
            get
            {
                return loadingVisible;
            }
            set
            {
                loadingVisible = value;
                RaisePropertyChanged();
            }
        }

        public MainWindow()
        {
            this.ShowTitleBar = false;
            SystemManager.LoadParam();
            SystemModuleCount.LoadParam();
            LoaderParkingParam.LoadParam();
            //==> WIndow option
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowState = WindowState.Maximized;
            this.IgnoreTaskbarOnMaximize = true;
            this.IsWindowDraggable = false;

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            Version = fvi.ProductVersion;

            DBManager.Open();

            StartProgress();
            InitSystem();
            var loader = loaderModule;

            PreviewKeyDown += MainWindow_PreviewKeyDown;
            MouseDown += MainWindow_MouseDown;
            TouchDown += MainWindow_MouseDown;

            InitializeComponent();
            StopProgress();
            InitDrawer();
        }

        private void MainWindow_MouseDown(object sender, EventArgs e)
        {
            try
            {
                if (GPLoader.IsBuzzerOn)
                {
                    GPLoader.LoaderBuzzer(false);       // 이 코드 삭제해도 되는지? test 할 것
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(BuzzerOffStateEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        /// <summary>
        /// pwssword를 받을 창을 출력하고 입력된 값이 super account와 일치하는지 여부 반환
        /// </summary>
        /// <returns>true:PWD 정상, flase:PWD 비정상</returns>
        private bool ShowPasswordDlg_N_Check()
        {
            try
            {
                string text = null;
                text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                String superPassword = AccountManager.MakeSuperAccountPassword();

                if (text.ToLower().CompareTo(superPassword) == 0)
                    return true;
                else
                    return false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return false;
        }

        private SecsGemSettingDialog _SecsGemDialog { get; set; }
        private SignalTowerDisplayDialog _SignalTowerDisplayDialog { get; set; }

        private ProberDevelopPackWindow.MainWindow _ProberDevelopPackWindow { get; set; }
        private async void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftAlt) &&
                   Keyboard.IsKeyDown(Key.F4))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftAlt] + [F4] : Disable Feature");
                    e.Handled = true;
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                   Keyboard.IsKeyDown(Key.F1))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F1]");
                    if (loaderMaster.ModuleState.State != ModuleStateEnum.RUNNING)
                    {
                        await ViewModelManager.ViewTransitionAsync(new Guid("DD8BED78-B2A7-974E-6941-C970993050FD"));
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.F2))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F2]");
                    if (loaderMaster.ModuleState.State != ModuleStateEnum.RUNNING)
                    {
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            bool IsGoodPwd = ShowPasswordDlg_N_Check();
                            if (IsGoodPwd)
                            {
                                switch (SystemManager.SystemType)
                                {
                                    case SystemTypeEnum.None:
                                        break;
                                    case SystemTypeEnum.Opera:
                                        await ViewModelManager.ViewTransitionAsync(new Guid("156F45C2-472E-A15D-1B1E-793F7E22DCA4"));

                                        break;
                                    case SystemTypeEnum.GOP:
                                        await ViewModelManager.ViewTransitionAsync(new Guid("da72dfc3-4a34-4206-b321-4bdbf074de7d"));

                                        break;
                                    case SystemTypeEnum.DRAX:
                                        await ViewModelManager.ViewTransitionAsync(new Guid("758ceac2-5962-4810-a8ab-7719d2b12f0c"));

                                        break;
                                    default:
                                        break;
                                }
                            }
                        }));
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                   Keyboard.IsKeyDown(Key.F3))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F3]");

                    if (loaderMaster.ModuleState.State != ModuleStateEnum.RUNNING)
                    {
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            bool IsGoodPwd = ShowPasswordDlg_N_Check();
                            if (IsGoodPwd)
                            {
                                await ViewModelManager.ViewTransitionAsync(new Guid("4732F634-2292-6228-C7E5-24A18C888187"));
                            }
                        }));
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                 Keyboard.IsKeyDown(Key.F4))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F4]");
                    if (loaderMaster.ModuleState.State != ModuleStateEnum.RUNNING)
                    {
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            bool IsGoodPwd = ShowPasswordDlg_N_Check();
                            if (IsGoodPwd)
                            {
                                await ViewModelManager.ViewTransitionAsync(new Guid("AE2B4076-1C65-87E1-7DA5-64BA7B1D4CCA"));
                            }
                        }));
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                Keyboard.IsKeyDown(Key.F5))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F5]");

                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bool IsGoodPwd = ShowPasswordDlg_N_Check();
                        if (IsGoodPwd)
                        {
                            EmulGemVM.Show(this.loaderMaster as LoaderSupervisor);
                        }
                    }));
                }
                else if (Keyboard.IsKeyDown(Key.LeftAlt) &&
                Keyboard.IsKeyDown(Key.F5))
                {

                    LoggerManager.Debug("[Window Key Down] - [LeftAlt] + [F5]");

                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bool IsGoodPwd = ShowPasswordDlg_N_Check();
                        if (IsGoodPwd)
                        {
                            DryRunVM.Show(this.loaderMaster as LoaderSupervisor);
                        }
                    }));
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
               Keyboard.IsKeyDown(Key.D) && Keyboard.IsKeyDown(Key.Y))
                {
                    DemoRunDYWindow.MainWindow window = new DemoRunDYWindow.MainWindow(this.loaderMaster as LoaderSupervisor);
                    window.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.E) & Keyboard.IsKeyDown(Key.A))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [E] + [A]");
                    EventCodeEditor.ErrorCodeAlarmVM.Show(this.loaderMaster as LoaderSupervisor);
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.C) & Keyboard.IsKeyDown(Key.V))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [C] + [V]");
                    if (tccWindow == null)
                    {
                        tccWindow = new TesterCoolantControlDialog.MainWindow();
                    }
                    tccWindow.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
              Keyboard.IsKeyDown(Key.F9))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F9]");
                    BacordReaderVM.Show(GPLoader);
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.G) &&
                        Keyboard.IsKeyDown(Key.M))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [G] + [M]");
                    if (_SecsGemDialog != null)
                    {
                        _SecsGemDialog.Close();
                        _SecsGemDialog = null;
                    }

                    _SecsGemDialog = new SecsGemSettingDialog();
                    _SecsGemDialog.Closed += SubWindowsClosed;

                    _SecsGemDialog.Width = 800;
                    _SecsGemDialog.Height = 680;
                    _SecsGemDialog.Title = "Gem Setting";
                    _SecsGemDialog.Show();

                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.G) &&
                        Keyboard.IsKeyDown(Key.E))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [G] + [E]");
                    var testDialog = new SecsGemEventSettingDialog();

                    testDialog.Width = 1170;
                    testDialog.Height = 680;
                    testDialog.Title = "Gem Event Setting";
                    testDialog.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
              Keyboard.IsKeyDown(Key.D) && Keyboard.IsKeyDown(Key.Y))
                {
                    DemoRunDYWindow.MainWindow window = new DemoRunDYWindow.MainWindow(this.loaderMaster as LoaderSupervisor);
                    window.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.LeftAlt) &&
                        Keyboard.IsKeyDown(Key.K) &&
                        Keyboard.IsKeyDown(Key.F))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [K] + [F]");
                    await this.MetroDialogManager().CloseWaitCancelDialaog(string.Empty);

                    // TODO : TEST CODE
                    LoggerManager.Error("Developer command occurred. WaitCancelDialogSerivce().CloseDialog");
                    //this.WaitCancelDialogService().CloseDialog();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.K) &&
                        Keyboard.IsKeyDown(Key.D))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [K] + [D]");
                    await this.MetroDialogManager().CloseWindow();

                    // TODO : TEST CODE
                    LoggerManager.Error("Developer command occurred. CloseAdvancedDialog()");
                    //this.WaitCancelDialogService().CloseDialog();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.S))
                {
                    // SnapShot
                    try
                    {
                        LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [S]");
                        string filepath;
                        string filename;
                        string fullpath;
                        string fileextension;

                        System.Drawing.Image screen = ScreenshotCapture.TakeScreenshot(true);

                        filepath = @"C:\ProberSystem\Snapshot";
                        filename = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
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
                        throw;
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.E) &&
                        Keyboard.IsKeyDown(Key.C))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [E] + [C]");
                    GPEnvControlVM.Show(this.loaderMaster as LoaderSupervisor);
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.T) & Keyboard.IsKeyDown(Key.O))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [T] + [O]");
                    string text = null;
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                        String superPassword = AccountManager.MakeSuperAccountPassword();

                        if (text.ToLower().CompareTo(superPassword) == 0)
                        {
                            TransferObjectViewModel.Show();
                        }
                    }));
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                   Keyboard.IsKeyDown(Key.F) & Keyboard.IsKeyDown(Key.P))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F] + [P]");
                    GPFoupSettingViewModel.Show(loaderModule);
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.V) &&
                        Keyboard.IsKeyDown(Key.M))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [V] + [M]");
                    ViewModelManager.WriteCurrentViewAndViewModelName();
                }

                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                      Keyboard.IsKeyDown(Key.L) &&
                      Keyboard.IsKeyDown(Key.V))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [L] + [V]");
                    GPLoaderJobViewModel.Show(loaderModule);
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                       Keyboard.IsKeyDown(Key.P) &&
                       Keyboard.IsKeyDown(Key.D))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [P] + [D]");

                    bool IsGoodPwd = ShowPasswordDlg_N_Check();

                    if (IsGoodPwd)
                    {
                        if (_ProberDevelopPackWindow != null)
                        {
                            _ProberDevelopPackWindow.Close();
                            _ProberDevelopPackWindow = null;
                        }

                        _ProberDevelopPackWindow = new ProberDevelopPackWindow.MainWindow();
                        _ProberDevelopPackWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        _ProberDevelopPackWindow.Show();
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                       Keyboard.IsKeyDown(Key.D) && Keyboard.IsKeyDown(Key.B))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [D] + [B]");
                    var window = new DBTableEditor.MainWindow();
                    window.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.F))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F]");
                    string path = @"C://";
                    Process.Start(path);
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.A) && Keyboard.IsKeyDown(Key.C))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [A] + [C]");
                    AccountSettingViewModel.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                       Keyboard.IsKeyDown(Key.F7))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F7]");
                    if (loaderMaster != null && loaderMaster.ManualContactModule() != null)
                    {
                        if (loaderMaster.ManualContactModule().CPC_Visibility == Visibility.Visible)
                        {
                            loaderMaster.ManualContactModule().CPC_Visibility = Visibility.Hidden;
                            LoggerManager.Debug("CPC_Visibility=Hidden");
                        }
                        else
                        {
                            loaderMaster.ManualContactModule().CPC_Visibility = Visibility.Visible;
                            LoggerManager.Debug("CPC_Visibility=Visible");
                        }
                    }
                }
                else if (e.Key == Key.F7)
                {
                    LoggerManager.Debug("[Window Key Down] - [F7]");
                    if (AccountManager.MaskingLevelCollection != null)
                    {
                        if (AccountManager.IsUserLevelAboveThisNum(0))
                        {
                            if (ViewModelManager.VMFilterPanel.IsEnable == false)
                            {
                                ViewModelManager.VMFilterPanel.RequestEnableMode();
                            }
                            else
                            {
                                ViewModelManager.VMFilterPanel.RequestDisableMode();
                            }
                        }
                    }
                }                
                else if (Keyboard.IsKeyDown(Key.LeftAlt) &&
                       Keyboard.IsKeyDown(Key.Enter))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftAlt] + [Enter]");
                    this.WindowState = WindowState.Maximized;
                    this.IsWindowDraggable = false;
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.R) &&
                        Keyboard.IsKeyDown(Key.T))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [R] + [T]");
                    repeatedTransferVM.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.T) &&
                        Keyboard.IsKeyDown(Key.D))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [T] + [D]");
                    var testDialog = new TestSimulationDialogView();

                    testDialog.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.E) &&
                        Keyboard.IsKeyDown(Key.D8))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [E] + [D8]");

                    // Get all open windows in the application
                    var openWindows = Application.Current.Windows;

                    E84SimulatorDialogView dialog = null;

                    // Check if there's an E84SimulatorDialogView already open
                    foreach (Window window in openWindows)
                    {
                        if (window is E84SimulatorDialogView)
                        {
                            dialog = window as E84SimulatorDialogView;
                            break;
                        }
                    }

                    // If the dialog is not open, create a new one
                    if (dialog == null)
                    {
                        dialog = new E84SimulatorDialogView();
                        dialog.Show();
                    }
                    else
                    {
                        // If the dialog is already open, bring it to the front
                        dialog.Activate();
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.Q) &&
                        Keyboard.IsKeyDown(Key.L))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [Q] + [L]");

                    if (_SignalTowerDisplayDialog != null)
                    {
                        _SignalTowerDisplayDialog.Close();
                        _SignalTowerDisplayDialog = null;
                    }
                    _SignalTowerDisplayDialog = new SignalTowerDisplayDialog();
                    _SignalTowerDisplayDialog.Width = 300;
                    _SignalTowerDisplayDialog.Height = 600;
                    _SignalTowerDisplayDialog.Title = "SignalTower Setting";
                    _SignalTowerDisplayDialog.Show();
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        Thread thread;
        RepeatedTransferVM repeatedTransferVM = new RepeatedTransferVM();

        private void StartProgress()
        {
            thread = new Thread(new ThreadStart(ThreadStartingPoint));

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void ThreadStartingPoint()
        {
            Splasher.Splash = new SplashWindowView(this.Version);
            Splasher.ShowSplash();

            System.Windows.Threading.Dispatcher.Run();
        }

        private void StopProgress()
        {
            Splasher.CloseSplash();
        }

        private void StratDBUpdateProgress()
        {
            SplashWindowView spWindowVM = Splasher.Splash as SplashWindowView;
            spWindowVM?.StartDBUpdate();
        }

        private void StopDBUpdateProgress()
        {
            SplashWindowView spWindowVM = Splasher.Splash as SplashWindowView;
            spWindowVM?.StopDbUpdate();
        }

        private void ServiceHost_Opened(object sender, EventArgs e)
        {
            InitHostService();
        }

        private int InitHostService()
        {
            LoaderCallback = OperationContext.Current.GetCallbackChannel<ILoaderServiceCallback>();

            //foreach (var axis in StageAxes.ProbeAxisProviders)
            //{
            //    var probAxis = axis;
            //    //probAxis.Status.Position.PosUpdated += OnAxisPropertyChanged;
            //    probAxis.Status.Position.PosUpdated += probAxis.OnStatusUpdated;
            //    probAxis.OnAxisStatusUpdated += OnAxisPropertyChanged;
            //}
            LoaderService.SetCallBack(LoaderCallback);
            LoggerManager.Debug($"InitHostService(): Hash = {LoaderCallback.GetHashCode()}");
            return LoaderCallback.GetHashCode();
        }


        private void InitSystem()
        {
            try
            {
                LoggerManager.Init_GPLoader();

                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

                LoggerManager.Debug($"Maestro Software version Information : {fvi.FileVersion}", isInfo: true);
                LoggerManager.Debug($"Maestro Product version Information : {fvi.ProductVersion}", isInfo: true);

                LoggerManager.Debug($"Loader system Initializing...");

                string loaderFactoryDllName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LoaderFactory.dll");

                var loaderFactoryAssembly = Assembly.LoadFrom(loaderFactoryDllName);

                var loaderResolver = ReflectionEx.GetAssignableInstances<ILoaderResolver>(loaderFactoryAssembly).FirstOrDefault();
                container = loaderResolver.RemoteModeConfigDependencies();


                Extensions_IModule.SetLoaderContainer(null, container);
                Extensions_IModule.SetContainer(null, container);

                // TODO :Check
                Extensions_ICommand.SetStageContainer(null, container);
                Extensions_ICommand.SetLoaderContainer(null, container);

                InSightDisplayApp.SetLoaderContainer(container);
                InSightDisplayApp.GP_Get();//==> Cognex Background Processs 실행

                LoggerManager.Debug($"[S] CUIService Init");
                CUIService.InitModule();
                LoggerManager.Debug($"[E] CUIService Init");

                LoggerManager.Debug($"[S] AccountManager Init");
                AccountManager.InitModule(container);
                LoggerManager.Debug($"[E] AccountManager Init");

                // ToDo: Replace with file manager GetRootParamPath() method.
                //string rootParamPath = this.FileManager().GetRootParamPath();
                //(container.Resolve<IFileManager>()).InitModule();

                var gemModule = this.GEMModule();
                //brett// GEM(CELL) Disconnect Callback 등록
                gemModule.SetGEMDisconnectCallBack(loaderMaster.GEMDisconnectCallback);
                //ParamManager.LoadElementInfoFromDB();

                //string loaderCoreDllName = "LoaderCore.dll";
                string loaderCoreDllName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LoaderCore.dll");

                var loaderCoreAssembly = Assembly.LoadFrom(loaderCoreDllName);
                var services = ReflectionEx.GetAssignableInstances<ILoaderService>(loaderCoreAssembly);

                LoaderService = services.FirstOrDefault(s => s.GetServiceType() == LoaderServiceTypeEnum.REMOTE);
                LoaderService.SetContainer(container);

                LoaderService.Connect();
                LoaderService.IsServiceAvailable();

                this.SequenceEngineManager();
                this.SequenceEngineManager().RunSequences();

                loaderMaster.Initialize(container);
                loaderModule.SystemInit();

                //var paramVM = LoaderParameterViewModel.Instance;
                //paramVM.Initialize(container);

                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++) 
                {
                    LoaderService.UpdateLoaderSystem(i + 1);
                    LoaderService.UpdateCassetteSystem(loaderModule.GetLoaderCommands().GetDeviceSize(i), i + 1);
                }

                //var modules = LoaderResolver.RegisteFactoryModules;
                //foreach (var module in modules)
                //{
                //    if (module is IHasDevParameterizable)
                //        (module as IHasDevParameterizable).LoadDevParameter();
                //    else if (module is IHasSysParameterizable)
                //        (module as IHasSysParameterizable).LoadSysParameter();
                //    if (module is IModule)
                //        (module as IModule).InitModule();

                //    //if (module is ILoaderFactoryModule)
                //    //{
                //    //    (module as ILoaderFactoryModule).InitModule(container);
                //    //}
                //}
                //loaderResolver.InitModules();




                //ILoaderCommunicationManager loaderCommManager = container.Resolve<ILoaderCommunicationManager>();
                //ILoaderCommunicationManager loaderCommManager = ReflectionEx.GetAssignableInstances<ILoaderCommunicationManager>();
                //loaderCommManager.SetContainer(container);
                //loaderCommManager.InitModule();
                //LoaderViewModelManager.SetContainer(container);
                //((LoaderPnpManager)PnpManager).SetContainer(container);


                //IWaitCancelDialogService metroDialogManager = container.Resolve<IWaitCancelDialogService>();
                //((WaitCancelDialogService)metroDialogManager).SetContainer(container);
                //(container.Resolve<IFoupOpModule>()).InitModule();

                #region //..ViewModelInit

                TransferObjectViewModel = new TemplateTransferObjectVM();
                Application.Current.Resources.Add("TransferObjectVM", TransferObjectViewModel);
                GPFoupSettingViewModel = new GPFoupSettingVM();
                Application.Current.Resources.Add("GPFoupSetting", GPFoupSettingViewModel);

                GPLoaderJobViewModel = new GPLoaderJobVM();
                Application.Current.Resources.Add("GPLoaderJob", GPLoaderJobViewModel);
                #endregion

                //IParamManager paramManager = new ParamManager();
                //paramManager.SetLoaderContainer(container);
                //paramManager.InitModule();

                /*
                    이름을 가진 Mutex를 사용하여 다른 프로세스에서는 해당 Mutex가 반환될 때까지 사용할 수 없도록 처리함.                   
                    DB_UPDATE라는 이름을 가진 Mutex를 열고 작업이 성공적으로 수행되었는지 여부를 체크함. Mutex를 열었으면 True, 그렇지 않으면 false로 반환함.
                    DB Update 및 추가는 최초에 한 번만 이뤄지면 되기 때문에 해당 방법을 사용함.
                */
                if (!Mutex.TryOpenExisting("DB_UPDATE", out var DBUpdateMutex))
                {
                    // Mutex를 열지 못했기 때문에 DB_UPDATE라는 이름을 가진 Mutex를 열고 작업을 수행할 수 있도록 함.
                    using (var Mtx = new Mutex(true, "DB_UPDATE"))
                    {
                        StratDBUpdateProgress();

                        ////==> CSV 파일(DevParameterData, SysParameterData, CommonParameterData)을 읽어서 DB에 Update
                        // DB update 완료 후에는 EnableUpdateDBFile = false로 변경됨. 따라서 최초에 DB_UPDATE Mutex를 선점한 프로세스만 DB Update 작업을 수행함.                           
                        ParamManager.SyncDBTableByCSV();

                        ////==> ParamManager가 수집한 Element 들을(DevDBElementDictionary, SysDBElementDictionary, CommonDBElementDictionary)을 DB에 추가               
                        // DB update 완료 후에는 EnableDBSourceServer = false로 변경됨. 따라서 최초에 DB_UPDATE Mutex를 선점한 프로세스만 DB에 추가하는 작업을 수행함.
                        ParamManager.RegistElementToDB();

                        StopDBUpdateProgress();

                        ////==> DB 의 데이터들을 Csv로 Export(C:\ProberSystem\DB\)
                        ParamManager.ExportDBtoCSV();

                        // DB_UPDATE Mutex를 사용한 후에 반환해줘야 함. 그래야 다른 프로세스가 해당 Mutex를 사용할 수 있음.
                        Mtx.ReleaseMutex();
                    }
                }
                else
                {
                    // DB_UPDATE라는 이름을 가진 Mutex를 이미 열었기 때문에 해당 Mutex가 반환되서 사용할 수 있을때까지 기다림.               
                    DBUpdateMutex.WaitOne();
                    // 반환된 Mutex를 얻었기 때문에 반드시 Mutex를 반환해줘야 함.
                    DBUpdateMutex.ReleaseMutex();
                }

                ////==> DB의 Meta data를 읽어 들여서 Element의 Meta Data 초기화
                ParamManager.LoadElementInfoFromDB();

                this.GEMModule().CommEnable();
                // TODO : 필요한가?
                //ViewModelManager.InitScreen();

                this.DataContext = container.Resolve<IViewModelManager>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SubWindowsClosed(object sender, EventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(SecsGemSettingDialog))
                {
                    _SecsGemDialog = null;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            try
            {
                Window w = null;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    w = System.Windows.Application.Current.MainWindow;
                });

                Extensions_IParam.ProgramShutDown = true;
                Deinitialize();


                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    w.Close();
                });


                //await Task.Run(() =>
                //{
                //    retval = Deinitialize();
                //});

                //container.Dispose();

                //App.Current.Shutdown();
                LoggerManager.Deinit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum Deinitialize()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //SequenceEngineManager.DeInitModule();
                ((IModule)this.loaderModule.MotionManager).DeInitModule();
                ((IModule)this.loaderModule.IOManager).DeInitModule();
                ((IModule)this.loaderModule.VisionManager).DeInitModule();
                container.Resolve<ILoaderCommunicationManager>().DeInitModule();
                loaderMaster.DeInitModule();
                loaderModule.Deinitialize();
                //var modules = container.Resolve<IEnumerable<IFactoryModule>>();
                //var modules = LoaderResolver.GetFactoryModules();
                var modules = container.Resolve<IEnumerable<IFactoryModule>>();
                foreach (var item in modules)
                {
                    if (item is IModule)
                    {
                        (item as IModule).DeInitModule();
                    }
                    else
                    {
                        //Debugger.Break();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void MetroWindow_Deactivated(object sender, EventArgs e)
        {
            //this.Topmost = false;
        }

        private void MetroWindow_Activated(object sender, EventArgs e)
        {
            //this.Topmost = true;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            int TopBarHeight = 70;

            (LoaderViewModelManager as LoaderViewModelModule.LoaderViewModelManager).ORGMainViewWidth = this.ActualWidth;
            (LoaderViewModelManager as LoaderViewModelModule.LoaderViewModelManager).ORGMainViewHeight = this.ActualHeight - TopBarHeight;

            container.Resolve<IMetroDialogManager>().SetMetroWindowLoaded(true);
            ViewModelManager.InitScreen();
            Extensions_IParam.LoadProgramFlag = true;
        }

        public void Release()
        {
            Window w = null;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                w = System.Windows.Application.Current.MainWindow;
            });

            Extensions_IParam.ProgramShutDown = true;
            Deinitialize();


            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.Application.Current.Shutdown();
                Environment.Exit(0);
            });
        }


        #region .. Drawer
        private object _DrawerControl;
        public object DrawerControl
        {
            get { return _DrawerControl; }
            set
            {
                if (value != _DrawerControl)
                {
                    _DrawerControl = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void InitDrawer()
        {
            DrawerControl = this.LoaderDrawer;
            Application.Current.Resources.Add("LoaderDrawer", DrawerControl);
        }
        #endregion
    }

    public class DrawerHostEx : DrawerHost
    {
        public DrawerHostEx()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var grid = GetTemplateChild(TemplateContentCoverPartName) as System.Windows.Controls.Grid;
            grid.Visibility = System.Windows.Visibility.Collapsed;
        }
    }

    //public class WindowBehavior
    //{
    //    private static readonly Type OwnerType = typeof(WindowBehavior);

    //    #region HideCloseButton (attached property)

    //    public static readonly DependencyProperty HideCloseButtonProperty =
    //        DependencyProperty.RegisterAttached(
    //            "HideCloseButton",
    //            typeof(bool),
    //            OwnerType,
    //            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(HideCloseButtonChangedCallback)));

    //    [AttachedPropertyBrowsableForType(typeof(Window))]
    //    public static bool GetHideCloseButton(Window obj)
    //    {
    //        return (bool)obj.GetValue(HideCloseButtonProperty);
    //    }

    //    [AttachedPropertyBrowsableForType(typeof(Window))]
    //    public static void SetHideCloseButton(Window obj, bool value)
    //    {
    //        obj.SetValue(HideCloseButtonProperty, value);
    //    }

    //    private static void HideCloseButtonChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var window = d as Window;
    //        if (window == null) return;

    //        var hideCloseButton = (bool)e.NewValue;
    //        if (hideCloseButton && !GetIsHiddenCloseButton(window))
    //        {
    //            if (!window.IsLoaded)
    //            {
    //                window.Loaded += HideWhenLoadedDelegate;
    //            }
    //            else
    //            {
    //                HideCloseButton(window);
    //            }
    //            SetIsHiddenCloseButton(window, true);
    //        }
    //        else if (!hideCloseButton && GetIsHiddenCloseButton(window))
    //        {
    //            if (!window.IsLoaded)
    //            {
    //                window.Loaded -= ShowWhenLoadedDelegate;
    //            }
    //            else
    //            {
    //                ShowCloseButton(window);
    //            }
    //            SetIsHiddenCloseButton(window, false);
    //        }
    //    }

    //    #region Win32 imports

    //    private const int GWL_STYLE = -16;
    //    private const int WS_SYSMENU = 0x80000;
    //    [DllImport("user32.dll", SetLastError = true)]
    //    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    //    [DllImport("user32.dll")]
    //    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    //    #endregion

    //    private static readonly RoutedEventHandler HideWhenLoadedDelegate = (sender, args) => {
    //        if (sender is Window == false) return;
    //        var w = (Window)sender;
    //        HideCloseButton(w);
    //        w.Loaded -= HideWhenLoadedDelegate;
    //    };

    //    private static readonly RoutedEventHandler ShowWhenLoadedDelegate = (sender, args) => {
    //        if (sender is Window == false) return;
    //        var w = (Window)sender;
    //        ShowCloseButton(w);
    //        w.Loaded -= ShowWhenLoadedDelegate;
    //    };

    //    public static void HideCloseButton(Window w)
    //    {
    //        var hwnd = new WindowInteropHelper(w).Handle;
    //        SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
    //    }

    //    private static void ShowCloseButton(Window w)
    //    {
    //        var hwnd = new WindowInteropHelper(w).Handle;
    //        SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) | WS_SYSMENU);
    //    }

    //    #endregion

    //    #region IsHiddenCloseButton (readonly attached property)

    //    private static readonly DependencyPropertyKey IsHiddenCloseButtonKey =
    //        DependencyProperty.RegisterAttachedReadOnly(
    //            "IsHiddenCloseButton",
    //            typeof(bool),
    //            OwnerType,
    //            new FrameworkPropertyMetadata(false));

    //    public static readonly DependencyProperty IsHiddenCloseButtonProperty =
    //        IsHiddenCloseButtonKey.DependencyProperty;

    //    [AttachedPropertyBrowsableForType(typeof(Window))]
    //    public static bool GetIsHiddenCloseButton(Window obj)
    //    {
    //        return (bool)obj.GetValue(IsHiddenCloseButtonProperty);
    //    }

    //    private static void SetIsHiddenCloseButton(Window obj, bool value)
    //    {
    //        obj.SetValue(IsHiddenCloseButtonKey, value);
    //    }

    //    #endregion

    //}
}

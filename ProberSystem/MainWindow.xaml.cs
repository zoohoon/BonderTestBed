using AccountModule;
using Autofac;
using CUI;
using LogModule;
using MahApps.Metro.Controls;
using ModuleFactory;
using ProberErrorCode;
using ProberInterfaces;
using SplasherService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace ProberSystem
{
    using ComponentVerificationDialog;
    using CUIServices;
    using ExtraCameraMainDialog;
    using FocusingSettingDialog;
    using ForcedIODialog;
    using MaterialDesignThemes.Wpf;
    using MetroDialogInterfaces;
    using OPUSV3DView;
    using Pranas;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.ProberSystem;
    using ProberSystem.UserControls.VisionMapping;
    using RelayCommandBase;
    using SecsGemSettingDlg;
    using SplashWindowControl;
    using System.Windows.Documents;
    using System.Windows.Interop;
    using System.Windows.Media;
    using TestResultDlg;
    using TestSetupDialog;
    using UCTaskManagement;
    using VirtualKeyboardControl;
    //using ProberInterfaces.ThreadSync;
    using WaferAlignControlDialog;

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

    public class TransparentAdorner : Adorner
    {
        public TransparentAdorner(UIElement adornedElement) : base(adornedElement)
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            SolidColorBrush sr = new SolidColorBrush();

            sr.Color = Colors.Black;

            sr.Opacity = 0.0;

            drawingContext.DrawRectangle(sr, null, new Rect(new Point(0, 0), DesiredSize));

            base.OnRender(drawingContext);
        }
    }

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged, IReleaseResource, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        TransparentAdorner sim;

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    SolidColorBrush sr = new SolidColorBrush();

        //    sr.Color = Colors.Black;
        //    sr.Opacity = 0.1; //change the opacity here

        //    drawingContext.DrawRectangle(sr, null, new Rect(new Point(0, 0), DesiredSize));

        //    base.OnRender(drawingContext);
        //}

        EnumLangs currLang;

        //static string AssemblyPath = System.Environment.CurrentDirectory;
        static string AssemblyPath = AppDomain.CurrentDomain.BaseDirectory;

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

        private Autofac.IContainer Container;

        //private DispatcherTimer _timer;
        //DispatcherTimer timer = new DispatcherTimer();

        //ProberSystemModeParam ModeParam;

        public ISequenceEngineManager SequenceEngineManager
        {
            get { return Container.Resolve<ISequenceEngineManager>(); }
        }

        public IViewModelManager ViewModelManager
        {
            get { return Container.Resolve<IViewModelManager>(); }
        }
        public IParamManager ParamManager
        {
            get { return Container.Resolve<IParamManager>(); }
        }
        //public ProberStation Prober { get; set; }

        public IProberStation ProberStation
        {
            get { return Container.Resolve<IProberStation>(); }
        }

        //====EditResource Test Code
        public object OriginContent;
        //====
        public bool ForceSoftwareRendering
        {
            get
            {
                int renderingTier = (System.Windows.Media.RenderCapability.Tier >> 16);
                return renderingTier == 0;
            }
        }

        //private void InitHeader()
        //{
        //    var border = Find<Border>("borderHeader");
        //    var restoreIfMove = false;

        //    border.MouseLeftButtonDown += (s, e) =>
        //    {
        //        if (e.ClickCount == 2)
        //        {
        //            if ((ResizeMode == ResizeMode.CanResize) ||
        //                (ResizeMode == ResizeMode.CanResizeWithGrip))
        //            {
        //                SwitchState();
        //            }
        //        }
        //        else
        //        {
        //            if (WindowState == WindowState.Maximized)
        //            {
        //                restoreIfMove = true;
        //            }

        //            DragMove();
        //        }
        //    };
        //    border.MouseLeftButtonUp += (s, e) =>
        //    {
        //        restoreIfMove = false;
        //    };
        //    border.MouseMove += (s, e) =>
        //    {
        //        if (restoreIfMove)
        //        {
        //            restoreIfMove = false;
        //            var mouseX = e.GetPosition(this).X;
        //            var width = RestoreBounds.Width;
        //            var x = mouseX - width / 2;

        //            if (x < 0)
        //            {
        //                x = 0;
        //            }
        //            else
        //            if (x + width > screenSize.X)
        //            {
        //                x = screenSize.X - width;
        //            }

        //            WindowState = WindowState.Normal;
        //            Left = x;
        //            Top = 0;
        //            DragMove();
        //        }
        //    };
        //}
        private void SwitchState()
        {
            try
            {
                switch (WindowState)
                {
                    case WindowState.Normal:
                        {
                            WindowState = WindowState.Maximized;
                            break;
                        }
                    case WindowState.Maximized:
                        {
                            WindowState = WindowState.Normal;
                            break;
                        }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Process PowerShellProcess;

        public void SendMessageToLoader(string Message)
        {
            try
            {
                int index = this.LoaderController().GetChuckIndex();
                this.LoaderController().SetTitleMessage(index, Message);
                //this.StageSupervisor()?.GetServiceCallBack()?.SetTitleMessage(index, Message);
                //this.NotifyManager().SetLastStageMSG(Message);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SendLotLogToLoader(string Message, int idx, ModuleLogType ModuleType, StateLogType State)
        {
            try
            {
                this.LoaderController().SetLotLogMessage(Message, idx, ModuleType, State);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SendParamLogToLoader(string Message, int idx)
        {
            try
            {
                this.LoaderController().SetParamLogMessage(Message, idx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public MainWindow()
        {
            //System.Windows.Forms.Screen s = System.Windows.Forms.Screen.AllScreens[0];

            //this.Top = s.Bounds.Top;
            //this.Left = s.Bounds.Left;

            try
            {
                //PowerShellProcess = Process.Start(startInfo);

                //PowerShell powershell = PowerShell.Create();
                ////powershell.AddCommand(@"Get-Content C:\Logs\c01\Debug\Debug_2019-10-08.log -Wait -Tail 10");
                //powershell.Commands.Clear();
                //powershell.Connect();
                //powershell.AddScript(@"Get-Content C:\Logs\c01\Debug\Debug_2019-10-08.log -Wait -Tail 10");

                SystemManager.LoadParam();
                SystemModuleCount.LoadParam();
                LoggerManager.Init();

                LoggerManager.Debug($"[MainWindow], MainWindow() : SystemMode = {SystemManager.SysteMode}, SystemType = {SystemManager.SystemType}, SystemRunMode = {Extensions_IParam.ProberRunMode}");

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    LoggerManager.SendMessageToLoaderDelegate += SendMessageToLoader;
                    LoggerManager.SendActionLogToLoaderDelegate += SendLotLogToLoader;
                    LoggerManager.SendParamLogToLoaderDelegate += SendParamLogToLoader;
                }

                Process[] allProc = Process.GetProcesses();
                var retProc = allProc.SingleOrDefault(proc => proc.ProcessName == "powershell");
                if (retProc == null)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = false;
                    startInfo.FileName = "powershell.exe";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.Arguments = @"Get-Content " + LoggerManager.CurrentDebuglogPath + " -Wait -Tail 10";

                    if (startInfo.Arguments != string.Empty)
                    {
                        PowerShellProcess = Process.Start(startInfo);
                    }
                }

                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                Version = fvi.ProductVersion;
                LoggerManager.Debug($"Maestro Software version Information : {fvi.FileVersion}", isInfo: true);
                LoggerManager.Debug($"Maestro Product version Information : {fvi.ProductVersion}", isInfo: true);

                //  RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
                //RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
                //this.AllowsTransparency = false;
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                this.AllowsTransparency = true;

                Container = ModuleResolver.ConfigureDependencies();

                //Container.CurrentScopeEnding += OnScopeEnding;

                //LoggerManager.Debug($"ConfigureDependencies Done.");

                Extensions_IModule.SetContainer(null, Container);

                Extensions_ICommand.SetStageContainer(null, Container);

                //LoggerManager.Debug($"Set Container to Extensions_IModule(static class).");

                //Properties.Settings.Default["Test"] = "AAA";

                //if (int.TryParse(Properties.Settings.Default["Language"].ToString(), out lcid))
                //{
                //}
                //else
                //{
                //    lcid = 1033;    // en-US
                //    Properties.Settings.Default.Language = lcid;
                //    Properties.Settings.Default.Save();
                //}

                //Properties.Settings.Default.Save();

                //currLang = (EnumLangs)lcid;

                //Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)Properties.Settings.Default["Language"]);
                //Thread.CurrentThread.CurrentCulture = new CultureInfo((int)Properties.Settings.Default["Language"]);

                //this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

                //var theme = ThemeManager.DetectAppStyle(Application.Current);s
                //ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, ThemeManager.GetAppTheme("DarkTheme"));
                LoggerManager.Debug($"[S] CUIService Init");
                retval = CUIService.InitModule();
                LoggerManager.Debug($"[E] CUIService Init");

                LoggerManager.Debug($"[S] AccountManager Init");
                retval = AccountManager.InitModule(Container);
                LoggerManager.Debug($"[E] AccountManager Init");

                LoggerManager.Debug($"[S] CUIManager Init");
                retval = CUIManager.InitModule();
                LoggerManager.Debug($"[E] CUIManager Init");

                currLang = (EnumLangs)CUIService.param.lcid;

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(CUIService.param.lcid);
                //LoggerManager.Debug($"Set CultureInfo to Thread.CurrentThread.CurrentUICulture.");
                Thread.CurrentThread.CurrentCulture = new CultureInfo(CUIService.param.lcid);
                //LoggerManager.Debug($"Set CultureInfo to Thread.CurrentThread.CurrentCulture.");

                this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
                LoggerManager.Debug($"UI Culture set as {currLang.ToString()}({CUIService.param.lcid}).");

                InitializeComponent();
                //sim = new SimpleCircleAdorner(mainPane);

                //LoggerManager.Debug($"Components Initialize Done.");

                DrawerControl = this.Drawer;
                Application.Current.Resources.Add("Drawer", DrawerControl);

                StartProgress();
                Init();
                StopProgress();

                //ViewModelManager.InitScreen();

                this.SourceInitialized += Window_SourceInitialized;
                isWindowMovingStop = true;

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    this.Title = ("POS" + $" [{this.LoaderController().GetChuckIndex()}]");
                else
                    this.Title = "POS";
                //LoggerManager.Debug($"MainWindow Initialize Done.");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void AttachAdorner()
        {
            try
            {
                var grid = GetTemplateChild("PART_OverlayBox") as System.Windows.Controls.Grid;
                AdornerLayer parentAdorner = AdornerLayer.GetAdornerLayer(grid);

                if (parentAdorner != null)
                {
                    sim = new TransparentAdorner(grid);
                    parentAdorner.Add(sim);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DettachAdorner()
        {
            try
            {
                AdornerLayer parentAdorner = AdornerLayer.GetAdornerLayer(mainPane);
                parentAdorner.Remove(sim);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        bool isWindowMovingStop = false;

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = isWindowMovingStop;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }



        private void CreateSettings()
        {
            try
            {
                Properties.Settings.Default.Save();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        Thread thread;
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

        #region ==> OLD Version
        //Thread thread;
        //private void StartProgress()
        //{
        //    thread = new Thread(() =>
        //    {
        //        Splasher.Splash = new SplashWindowView();
        //        Splasher.ShowSplash();

        //        System.Windows.Threading.Dispatcher.Run();
        //    });

        //    thread.SetApartmentState(ApartmentState.STA);
        //    thread.IsBackground = true;
        //    thread.Start();
        //}
        //private void StopProgress()
        //{
        //    Splasher.CloseSplash();
        //    thread.Abort();
        //}
        #endregion


        private void Init()
        {
            try
            {
                //LoggerManager.Debug($"Config dependencies.");
                ProberStation.InitModule();

                ViewModelManager.InitModule();

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    Container.Resolve<IStageCommunicationManager>().InitServiceHosts();
                }

                UcVisionMappingOPUSVSetup visionMappingOPUSV = new UcVisionMappingOPUSVSetup();
                visionMappingOPUSV.InitViewModel();
                ViewModelManager.InsertView(visionMappingOPUSV);
                ViewModelManager.InsertConnectView(visionMappingOPUSV, visionMappingOPUSV);

                SequenceEngineManager.RunSequences();

                this.Visibility = Visibility.Visible;

                this.DataContext = ViewModelManager;

                //LoggerManager.Debug($"View model created.");

                PreviewKeyDown += MainWindow_PreviewKeyDown;
                PreviewKeyUp += MainWindow_PreviewKeyUp;
                PreviewMouseUp += MainWindow_PreviewMouseUp;
                PreviewMouseDown += MainWindow_PreviewMouseDown;

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

                        //==> CSV 파일(DevParameterData, SysParameterData, CommonParameterData)을 읽어서 DB에 Update                        
                        // EnableUpdateDBFile = True일 때 동작.
                        // DB update 완료 후에는 EnableUpdateDBFile = false로 변경됨. 따라서 처음 DB_UPDATE Mutex를 선점한 프로세스만 DB Update 작업을 수행함.                           
                        ParamManager.SyncDBTableByCSV();


                        //==> ParamManager가 수집한 Element 들을(DevDBElementDictionary, SysDBElementDictionary, CommonDBElementDictionary)을 DB에 추가                        
                        // EnableDBSourceServer = True일 때 동작.
                        // DB update 완료 후에는 EnableDBSourceServer = false로 변경됨. 따라서 처음 DB_UPDATE Mutex를 선점한 프로세스만 DB에 추가하는 작업을 수행함.
                        ParamManager.RegistElementToDB();

                        StopDBUpdateProgress();

                        //==> DB 의 데이터들을 Csv로 Export(C:\ProberSystem\DB\)
                        // EnableDBSourceServer = true일 때 동작.
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

                //==> DB의 Meta data를 읽어 들여서 Element의 Meta Data 초기화
                ParamManager.LoadElementInfoFromDB();

                //INIFile.InitModule();

                Container.Resolve<ICoordinateManager>().InitCoordinateManager();

                Extensions_IParam.LoadProgramFlag = true;
                //this.GEMModule().RegisteEvent_OnLoadElementInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"MainWindow.xaml : Init Err: " + err.Message);
                LoggerManager.Exception(err);
            }
        }
        private void Cu_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        MahApps.Metro.Controls.MetroWindow OPUS3DView = null;
        Window forcedIOView = null;
        Window TaskView = null;
        Window ComponentVerificationView = null;
        ExtraCameraDialog extraCameraDialog = null;
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
                else if (e.Key == Key.F2 | e.Key == Key.Oem3)
                {
                    LoggerManager.Debug("[Window Key Down] - [F2] or [Oem3]");
                    await Task.Run(() =>
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Drawer.IsBottomDrawerOpen = !Drawer.IsBottomDrawerOpen;
                        }));
                    });
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.F5))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F5]");
                    if (OPUS3DView != null && OPUS3DView.Visibility == Visibility.Visible)
                    {
                        OPUS3DView.Activate();
                        return;
                    }

                    var opus3d = OPUSV3D.GetInstance();

                    OPUS3DView = opus3d;
                    OPUS3DView.WindowStyle = WindowStyle.ToolWindow;
                    OPUS3DView.IgnoreTaskbarOnMaximize = false;
                    OPUS3DView.Width = 650;
                    OPUS3DView.Height = 650;
                    OPUS3DView.Visibility = Visibility.Visible;
                    // graphSingle.Owner = Model.ProberMain;
                    // OPUS3DView.Closed += (o, args) => OPUS3DView = null;
                    OPUS3DView.Show();
                    //await ProberStation.ModuleUpdater.ShowPanel();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.F11))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F11]");
                    if (AccountManager.IsUserLevelAboveThisNum(-1))
                    {
                        if (forcedIOView != null && forcedIOView.Visibility == Visibility.Visible)
                        {
                            forcedIOView.Activate();
                            return;
                        }

                        var opus3d = ForcedIOView.GetInstance();
                        forcedIOView = opus3d;
                        forcedIOView.WindowStyle = WindowStyle.ToolWindow;
                        forcedIOView.Width = 650;
                        forcedIOView.Height = 650;
                        forcedIOView.Visibility = Visibility.Visible;
                        // graphSingle.Owner = Model.ProberMain;
                        // OPUS3DView.Closed += (o, args) => OPUS3DView = null;
                        forcedIOView.Show();
                        //await ProberStation.ModuleUpdater.ShowPanel();
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.F12))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F12]");

                    // Idle 상태가 아니거나 Loader와 Connect 상태인 경우 창을 열지 않는다.
                    if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Warning Message", "Can not open the Component Verification Dialog\r\n(Reason : Current state is not idle)", EnumMessageStyle.Affirmative);
                        return;
                    }

                    if (this.LoaderController().GetconnectFlag())
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Warning Message", "Can not open the Component Verification Dialog\r\n(Reason : Loader is connected)", EnumMessageStyle.Affirmative);
                        return;
                    }

                    if (AccountManager.IsUserLevelAboveThisNum(-1))
                    {
                        if (ComponentVerificationView != null && ComponentVerificationView.Visibility == Visibility.Visible)
                        {
                            ComponentVerificationView.Activate();
                            return;
                        }

                        // Component Verification 창을 Open할 때 패스워드 추가
                        string text = null;
                        text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                        string superPassword = "eogksalsrnr"; // Password는 대한민국

                        if (text.ToLower().CompareTo(superPassword) == 0)
                        {
                            var CompVerifyView = ComponentVerificationDialogView.GetInstance();
                            ComponentVerificationView = CompVerifyView;
                            ComponentVerificationView.WindowStyle = WindowStyle.ToolWindow;
                            ComponentVerificationView.Visibility = Visibility.Visible;
                            ComponentVerificationView.ShowDialog();
                        }
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                  Keyboard.IsKeyDown(Key.F3))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F3]");

                    if (TaskView != null && TaskView.Visibility == Visibility.Visible)
                    {
                        TaskView.Activate();
                        return;
                    }

                    var taskManager = TaskManagerDialog.GetInstance();
                    TaskView = taskManager;
                    TaskView.WindowStyle = WindowStyle.ToolWindow;
                    TaskView.Visibility = Visibility.Visible;
                    // graphSingle.Owner = Model.ProberMain;
                    // OPUS3DView.Closed += (o, args) => OPUS3DView = null;
                    TaskView.Show();
                }
                else if (e.Key == Key.F6)
                {

                }
                else if (e.Key == Key.F7)
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F7]");
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
                else if (e.Key == Key.F8)
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F8]");
                    if (AccountManager.MaskingLevelCollection != null)
                    {
                        if (AccountManager.IsUserLevelAboveThisNum(-1))
                        {
                            TestSetup.AsyncShow();
                        }
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.E) &&
                    Keyboard.IsKeyDown(Key.C))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [E] + [C]");
                    if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        System.Windows.MessageBox.Show("[Error] Cannot option window . LotOPModule is Running state.");
                        LoggerManager.Debug("[Error] Cannot option window . LotOPModule is Running state.");
                        return;
                    }
                    if (AccountManager.IsUserLevelAboveThisNum(-1))
                    {
                        if (extraCameraDialog != null)
                        {
                            if (extraCameraDialog.HasCamera())
                            {
                                extraCameraDialog.Activate();
                            }
                            else
                            {
                                extraCameraDialog.Close();
                            }

                            return;
                        }

                        extraCameraDialog = new ExtraCameraDialog();
                        extraCameraDialog.Width = 1480;
                        extraCameraDialog.Height = 1040;
                        extraCameraDialog.Title = "산타할아버지는 알고 계신데, 누가 착한 프로그래머인지 나쁜 프로그래머인지.";

                        if (extraCameraDialog.HasCamera())
                        {
                            extraCameraDialog.Closed += (o, args) => extraCameraDialog = null;
                            extraCameraDialog.Show();
                        }
                        else
                        {
                            extraCameraDialog = null;
                        }
                    }
                }

                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.W) &&
                    Keyboard.IsKeyDown(Key.A))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [W] + [A]");
                    WAControlDialog wacontroldialog = new WAControlDialog();
                    wacontroldialog.Width = 600;
                    wacontroldialog.Height = 400;
                    wacontroldialog.Closed += (o, args) => wacontroldialog = null;
                    wacontroldialog.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.W) &&
                    Keyboard.IsKeyDown(Key.E))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [W] + [E]");

                    ProberStation.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.EXIST, EnumWaferType.STANDARD, "", 0);
                    ProberStation.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.UNPROCESSED);
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.K) &&
                    Keyboard.IsKeyDown(Key.F))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [K] + [F]");
                    await Container.Resolve<IMetroDialogManager>().CloseWaitCancelDialaog(string.Empty);
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.G) &&
                    Keyboard.IsKeyDown(Key.M))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [G] + [M]");
                    var testDialog = new SecsGemSettingDialog();

                    testDialog.Width = 800;
                    testDialog.Height = 680;
                    testDialog.Title = "Gem Setting";
                    testDialog.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                    Keyboard.IsKeyDown(Key.T) &&
                    Keyboard.IsKeyDown(Key.E) &&
                    Keyboard.IsKeyDown(Key.S)) //테스트하려고 만든 위한 단축키
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [T] + [E] + [S]");
                    var testDialog = new TestResultDialog();

                    testDialog.Width = 800;
                    testDialog.Height = 680;
                    testDialog.Title = "ㅅㄷㄴㅅ";
                    testDialog.Show();
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
                else if (Keyboard.IsKeyDown(Key.LeftShift) &&
                   Keyboard.IsKeyDown(Key.S) &&
                   Keyboard.IsKeyDown(Key.C))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [S] + [C]");
                    if (ProberStation.SoakingModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        ProberStation.SoakingModule().SoakingCancelFlag = true;
                    }
                }
                else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] or [RightCtrl]");
                    isWindowMovingStop = false;
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                   Keyboard.IsKeyDown(Key.F) &&
                   Keyboard.IsKeyDown(Key.D))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F] + [D]");

                    ProberStation.AirBlowChuckCleaningModule().ForcedDone = EnumModuleForcedState.ForcedDone;
                    ProberStation.PinAligner().ForcedDone = EnumModuleForcedState.ForcedDone;
                    ProberStation.AirBlowWaferCleaningModule().ForcedDone = EnumModuleForcedState.ForcedDone;
                    ProberStation.WaferAligner().ForcedDone = EnumModuleForcedState.ForcedDone;
                    ProberStation.PMIModule().ForcedDone = EnumModuleForcedState.ForcedDone;
                    ProberStation.PolishWaferModule().ForcedDone = EnumModuleForcedState.ForcedDone;
                    ProberStation.SoakingModule().ForcedDone = EnumModuleForcedState.ForcedDone;
                    ProberStation.NeedleCleaner().ForcedDone = EnumModuleForcedState.ForcedDone;
                    ProberStation.NeedleBrush().ForcedDone = EnumModuleForcedState.ForcedDone;
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.D) && Keyboard.IsKeyDown(Key.O))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [D] + [O]");
                    if (PowerShellProcess != null)
                    {
                        PowerShellProcess.Start();
                        //PowerShellProcess.Kill();
                        //PowerShellProcess = Process.Start(startInfo);
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                         Keyboard.IsKeyDown(Key.V) &&
                         Keyboard.IsKeyDown(Key.M))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [V] + [M]");
                    ViewModelManager.WriteCurrentViewAndViewModelName();
                }
                else if (Keyboard.IsKeyDown(Key.LeftShift) & Keyboard.IsKeyDown(Key.LeftAlt) & Keyboard.IsKeyDown(Key.P))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftShift] + [LeftAlt] + [P]");
                    var megRet = await this.MetroDialogManager().ShowMessageDialog("Warning Message", "Pause 하시겠습니까?", EnumMessageStyle.AffirmativeAndNegative);
                    if (megRet == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        this.CommandManager().SetCommand<ILotOpPause>(this);
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                            Keyboard.IsKeyDown(Key.F) &&
                            Keyboard.IsKeyDown(Key.V))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [F] + [V]");
                    var focusdialog = new FocusSettingDialog();

                    focusdialog.Title = "Focusing";
                    focusdialog.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                       Keyboard.IsKeyDown(Key.P) && Keyboard.IsKeyDown(Key.D))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [P] + [D]");
                    var window = new ProberDevelopPackWindow.MainWindow();
                    window.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.D) && Keyboard.IsKeyDown(Key.B))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [D] + [B]");
                    var window = new DBTableEditor.MainWindow();
                    window.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                      Keyboard.IsKeyDown(Key.A) &&
                      Keyboard.IsKeyDown(Key.L))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [A] + [L]");

                    var ALDialog = new AdjustLightDialog.MainWindow();

                    ALDialog.Show();
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                     Keyboard.IsKeyDown(Key.T) &&
                     Keyboard.IsKeyDown(Key.L))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [T] + [L]");
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                   Keyboard.IsKeyDown(Key.M) &&
                   Keyboard.IsKeyDown(Key.O))
                {
                    LoggerManager.Debug("[Window Key Down] - [LeftCtrl] + [M] + [O]");

                    ModuleResolver.PrintModuleTree();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Keyboard.IsKeyUp(Key.LeftCtrl) || Keyboard.IsKeyUp(Key.RightCtrl))
                {
                    isWindowMovingStop = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void MainWindow_PreviewMouseUp(object sender, MouseEventArgs e)
        {
            isWindowMovingStop = true;
        }
        private void MainWindow_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (ProberStation.MonitoringManager().IsSystemError == true)
            {
                ProberStation.LampManager().RequestRedLamp();
            }

            // TODO : Buzzer Turn off

            this.LampManager().WrappingSetBuzzerStatus(LampStatusEnum.Off);

        }
        private EventCodeEnum Deinitialize()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                SequenceEngineManager.DeInitModule();

                var modules = Container.Resolve<IEnumerable<IFactoryModule>>();

                foreach (var item in modules)
                {
                    try
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
                    catch (Exception err)
                    {
                        LoggerManager.Debug($"[Deinitialize] Fail : {item} + {err}");
                    }
                }

                //Container.Dispose();

                //retval = LoggerManager.Deinit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void Release()
        {
            try
            {
                Deinitialize();
                Container.Dispose();
                LoggerManager.Deinit();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.Application.Current.Shutdown();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                });
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
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                if (!Extensions_IParam.ProgramShutDown)
                    Release();
                //await Task.Run(() =>
                //{
                //    retval = Deinitialize();
                //});

                //Container.Dispose();
                LoggerManager.Deinit();
                App.Current.Shutdown();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MetroWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.A)
                {
                    ProberStation.VisionManager().ChangeGrabMode(ProberInterfaces.Vision.EnumGrabberMode.AUTO);
                }
                else if (e.Key == Key.M)
                {
                    ProberStation.VisionManager().ChangeGrabMode(ProberInterfaces.Vision.EnumGrabberMode.MANUAL);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void SimpleLogView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Drawer.IsTopDrawerOpen = !Drawer.IsTopDrawerOpen;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MetroWindow_Deactivated(object sender, EventArgs e)
        {
            //Container.Resolve<IMetroDialogManager>().SetMetroWindowLoaded(false);
        }

        private void MetroWindow_Activated(object sender, EventArgs e)
        {
            //Container.Resolve<IMetroDialogManager>().SetMetroWindowLoaded(true);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Container.Resolve<IMetroDialogManager>().SetMetroWindowLoaded(true);
            ViewModelManager.InitScreen();
        }
    }

    [DataContract]
    public enum EnumLangs
    {
        [Description("English")]
        English = 1033,
        [Description("한글")]
        Korean = 1042,
        [Description("中文-TW")]
        ChineseTW = 1028,
        [Description("中文-CN")]
        ChineseCN = 2052,
        [Description("日本語")]
        Japanese = 1041,
        [Description("French")]
        French = 1036
    }




}

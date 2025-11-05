using System;
using System.Windows;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using Autofac;
using MaterialDesignThemes.Wpf;

using MahApps.Metro;
using MahApps.Metro.Controls;
using ProberInterfaces;
using LogModule;
using ProberErrorCode;
using ModuleFactory;
using CUI;
using CUIServices;
using AccountModule;
using RelayCommandBase;
using SplasherService;
using SplashWindowControl;
using ProberInterfaces.ProberSystem;
using ProberSystem.UserControls.VisionMapping;
using MetroDialogInterfaces;

namespace BonderSystem
{
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
    public partial class MainWindow : MetroWindow, IReleaseResource, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
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
        public IProberStation ProberStation
        {
            get { return Container.Resolve<IProberStation>(); }
        }

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

        private Autofac.IContainer Container;

        public Process PowerShellProcess;

        EnumLangs currLang;

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

        public void SendMessageToLoader(string Message)
        {
            try
            {
                int index = this.LoaderController().GetChuckIndex();
                this.LoaderController().SetTitleMessage(index, Message);
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
            try
            {
                // SystemMode.json에서 데이터 Load
                SystemManager.LoadParam();
                // ModuleCount.json에서 데이터 Load
                SystemModuleCount.LoadParam();
                // 로그 컨트롤 관련 초기화
                LoggerManager.Init();

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    LoggerManager.SendMessageToLoaderDelegate += SendMessageToLoader;
                    LoggerManager.SendActionLogToLoaderDelegate += SendLotLogToLoader;
                    LoggerManager.SendParamLogToLoaderDelegate += SendParamLogToLoader;
                }

                // 현재 PC에서 실행 중인 모든 프로세스 목록을 가져옴.
                Process[] allProc = Process.GetProcesses();
                var retProc = allProc.SingleOrDefault(proc => proc.ProcessName == "powershell");
                // 실행 중인 프로세스 중에서 이름이 "powershell" 찾고, 없다면 실행
                // C:\\Logs\\DEBUG\\Debug_날짜.log 로그 실시간 모니터링
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

                // 프로그램 버전
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                Version = fvi.ProductVersion;
                LoggerManager.Debug($"Maestro Software version Information : {fvi.FileVersion}", isInfo: true);
                LoggerManager.Debug($"Maestro Product version Information : {fvi.ProductVersion}", isInfo: true);

                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                this.AllowsTransparency = true;

                // 각 클래스들 컨테이너에 등록
                Container = ModuleResolver.ConfigureDependencies();

                // 컨테이너 전달
                Extensions_IModule.SetContainer(null, Container);
                Extensions_ICommand.SetStageContainer(null, Container);

                // UI관련 초기화 (어떤 화면을 먼저 띄울지?, 파라미터)
                LoggerManager.Debug($"[S] CUIService Init");
                retval = CUIService.InitModule();
                LoggerManager.Debug($"[E] CUIService Init");

                // 계정 정보 관련 (User Name, Password 등등)
                LoggerManager.Debug($"[S] AccountManager Init");
                retval = AccountManager.InitModule(Container);
                LoggerManager.Debug($"[E] AccountManager Init");

                // 버튼 리스트, View Item List 초기화
                LoggerManager.Debug($"[S] CUIManager Init");
                retval = CUIManager.InitModule();
                LoggerManager.Debug($"[E] CUIManager Init");

                // 언어 설정 관련
                currLang = (EnumLangs)CUIService.param.lcid;
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(CUIService.param.lcid);
                Thread.CurrentThread.CurrentCulture = new CultureInfo(CUIService.param.lcid);
                this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
                LoggerManager.Debug($"UI Culture set as {currLang.ToString()}({CUIService.param.lcid}).");

                // BonderSystem MainWindow Initalize
                InitializeComponent();

                this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
                DrawerControl = this.Drawer;
                Application.Current.Resources.Add("Drawer", DrawerControl);

                // 시작 Waitting 화면
                StartProgress();
                Init();
                StopProgress();

                // 창 고정
                this.SourceInitialized += Window_SourceInitialized;
                isWindowMovingStop = true;

                // 타이틀바 이름
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    this.Title = ("POS" + $" [{this.LoaderController().GetChuckIndex()}]");
                else
                    this.Title = "POS";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        double originalWidth, originalHeight;

        ScaleTransform scale = new ScaleTransform();
        void MainWindow_Loaded(object sender, RoutedEventArgs e)

        {

            originalWidth = this.Width;

            originalHeight = this.Height;



            if (this.WindowState == WindowState.Maximized)

            {

                ChangeSize(this.ActualWidth, this.ActualHeight);

            }

            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)

        {

            ChangeSize(e.NewSize.Width, e.NewSize.Height);

        }

        private void ChangeSize(double width, double heigt)

        {

            scale.ScaleX = width / originalWidth;

            scale.ScaleY = heigt / originalHeight;



            FrameworkElement rootElement = this.Content as FrameworkElement;

            rootElement.LayoutTransform = scale;

        }

        private void Init()
        {
            try
            {
                // ProberCore InitModule (각 모듈 LoadParameter)
                ProberStation.InitModule();

                // Ui 셋팅 및 모션 IO 셋팅 부분
                ViewModelManager.InitModule();

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    Container.Resolve<IStageCommunicationManager>().InitServiceHosts();
                }

                UcVisionMappingOPUSVSetup visionMappingOPUSV = new UcVisionMappingOPUSVSetup();
                visionMappingOPUSV.InitViewModel();
                ViewModelManager.InsertView(visionMappingOPUSV);
                ViewModelManager.InsertConnectView(visionMappingOPUSV, visionMappingOPUSV);

                // SequenceJob 스레드 생성
                SequenceEngineManager.RunSequences();

                this.Visibility = Visibility.Visible;

                this.DataContext = ViewModelManager;

                // UI 키 관련
                //PreviewKeyDown += MainWindow_PreviewKeyDown;
                //PreviewKeyUp += MainWindow_PreviewKeyUp;
                //PreviewMouseUp += MainWindow_PreviewMouseUp;
                //PreviewMouseDown += MainWindow_PreviewMouseDown;

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

                Container.Resolve<ICoordinateManager>().InitCoordinateManager();

                Extensions_IParam.LoadProgramFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"MainWindow.xaml : Init Err: " + err.Message);
                LoggerManager.Exception(err);
            }
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

using System;
using System.Threading.Tasks;

namespace MainTopBarControlViewModel
{
    using System.ComponentModel;
    using ProberErrorCode;
    using ProberInterfaces;
    using MainMenuControlViewModel;
    using PopupControlViewModel;
    using ProberInterfaces.Temperature;
    using RelayCommandBase;
    using System.Windows.Input;
    using System.Windows;
    using System.Timers;
    using System.Runtime.CompilerServices;
    using LogModule;
    using System.Windows.Media;
    using MaterialDesignThemes.Wpf;
    using ProberInterfaces.LoaderController;
    using System.Diagnostics;
    using ProberInterfaces.State;
    using WindowsInput;
    using WindowsInput.Native;
    using VirtualKeyboardControl;
    using AccountModule;
    using MetroDialogInterfaces;

    public class MainTopBarControlVM : IMainScreenViewModel, IHasClock
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;


        readonly Guid _ViewModelGUID = new Guid("CBED19F9-1A90-43DB-B31F-DAF29BC852B4");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        private System.Timers.Timer Timer;

        private MainMenuVM _MainMenu = new MainMenuVM();
        public MainMenuVM MainMenu
        {
            get { return _MainMenu; }
            set
            {
                if (value != _MainMenu)
                {
                    _MainMenu = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PopupPanelBase _PopupPanelBase = new PopupPanelBase();
        public PopupPanelBase PopupPanelBase
        {
            get { return _PopupPanelBase; }
            set
            {
                if (value != _PopupPanelBase)
                {
                    _PopupPanelBase = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SwVersion;
        public string SwVersion
        {
            get { return _SwVersion; }
            set
            {
                if (value != _SwVersion)
                {
                    _SwVersion = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _DateTimeStr;
        public DateTime DateTimeStr
        {
            get { return _DateTimeStr; }
            set
            {
                if (value != _DateTimeStr)
                {
                    _DateTimeStr = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnabledSetTempEmulBtn;
        public bool IsEnabledSetTempEmulBtn
        {
            get { return _IsEnabledSetTempEmulBtn; }
            set
            {
                if (value != _IsEnabledSetTempEmulBtn)
                {
                    _IsEnabledSetTempEmulBtn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Thickness _BadgeMarginOffset;
        public Thickness BadgeMarginOffset
        {
            get { return _BadgeMarginOffset; }
            set
            {
                if (value != _BadgeMarginOffset)
                {
                    _BadgeMarginOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _BadgeBackground;
        public Brush BadgeBackground
        {
            get { return _BadgeBackground; }
            set
            {
                if (value != _BadgeBackground)
                {
                    _BadgeBackground = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _BadgeForeground;
        public Brush BadgeForeground
        {
            get { return _BadgeForeground; }
            set
            {
                if (value != _BadgeForeground)
                {
                    _BadgeForeground = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _BadgeCount;
        public int BadgeCount
        {
            get { return _BadgeCount; }
            set
            {
                if (value != _BadgeCount)
                {
                    _BadgeCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStageSupervisor _StageSupervisor;
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set
            {
                if (value != _StageSupervisor)
                {
                    _StageSupervisor = value;
                    RaisePropertyChanged(nameof(StageSupervisor));
                }
            }
        }

        public ILampManager LampManager { get => this.LampManager(); }
        public ISystemstatus Systemstatus { get => this.SysState(); }
        public ITempController TempController { get; set; }
        public ILotOPModule LotOPModule { get; set; }
        public ILoaderController LoaderController { get; set; }
        public IViewModelManager ViewModelManager { get; set; }
        public IMonitoringManager MonitoringManager { get; set; }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

                Timer.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    MainMenu.SetContainer(this.GetContainer());
                    MainMenu.ViewModelManager = this.ViewModelManager();

                    MainMenu.LotOPModule = this.LotOPModule();
                    MainMenu.LoaderController = this.LoaderController();
                    MainMenu.MonitoringManager = this.MonitoringManager();

                    PopupPanelBase.InitModule();

                    LotOPModule = this.LotOPModule();
                    TempController = this.TempController();
                    ViewModelManager = this.ViewModelManager();
                    LoaderController = this.LoaderController();
                    MonitoringManager = this.MonitoringManager();
                    StageSupervisor = this.StageSupervisor();

                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                    SwVersion = fvi.ProductVersion; //minskim// AssemblyInformationalVersion 정보가 있을 경우 해당 정보를 가져오기 위해 수정 함

                    Timer = new System.Timers.Timer();
                    Timer.Interval = 100;
                    Timer.Elapsed += GetDateTime;
                    Timer.Start();


                    IsEnabledSetTempEmulBtn = Extensions_IParam.ProberRunMode == RunMode.EMUL;

                    // Test Code for Badge
                    BadgeCount = ViewModelManager.LogViewModel.EventLogUserNotNotifiedCount;

                    BadgeMarginOffset = new Thickness(0, -2, -2, 0);
                    //BadgeBackground = new SolidColorBrush(Colors.Black);
                    //BadgeBackground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFAA38"));
                    //BadgeForeground = new SolidColorBrush(Colors.Red);

                    BadgeBackground = new SolidColorBrush(Colors.Red);
                    BadgeForeground = new SolidColorBrush(Colors.NavajoWhite);


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



        private void GetDateTime(object sender, ElapsedEventArgs e)
        {
            this.DateTimeStr = DateTime.Now;
        }

        private RelayCommand<object> _UsedBellDrawerOpenCommand;
        public ICommand UsedBellDrawerOpenCommand
        {
            get
            {
                if (null == _UsedBellDrawerOpenCommand) _UsedBellDrawerOpenCommand = new RelayCommand<object>(UsedBellDrawerOpenCmd);
                return _UsedBellDrawerOpenCommand;
            }
        }

        private void UsedBellDrawerOpenCmd(object obj)
        {
            try
            {
                //var tmp = (MainWindow)Application.Current.MainWindow;

                DrawerHost drawer = Application.Current.Resources["Drawer"] as DrawerHost;

                //var DrawerHost = tmp.DrawerControl as DrawerHost;

                if (drawer != null)
                {
                    ViewModelManager.ChangeTabControlSelectedIndex(ProberLogLevel.EVENT);
                    ViewModelManager.TopBarDrawerOpen(true);

                    drawer.IsTopDrawerOpen = !drawer.IsTopDrawerOpen;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _DrawerOpenCommand;
        public ICommand DrawerOpenCommand
        {
            get
            {
                if (null == _DrawerOpenCommand) _DrawerOpenCommand = new RelayCommand<object>(DrawerOpenCmd);
                return _DrawerOpenCommand;
            }
        }

        private void DrawerOpenCmd(object obj)
        {
            try
            {
                //LoggerManager.EventLog("TEST Event Log 1", "Description asdkasldjmqwciomejqwiocemqwoeqwioecjmqwioeqwioejmq qwjecioqwejnioqwcjioqwjemqwocjeio qcfmjeqwioejmqwioejcqwioejmqwiojeio qcmeqwjiqcwejmioqwejo");

                // [First number in event code] :
                // 0 : Information
                // 1 : Operation alarm
                // 2 : System fault

                //LoggerManager.Prolog("0", "ExtLog");
                //LoggerManager.Prolog("1", "ExtLog");
                //LoggerManager.Prolog("2", "ExtLog");

                //LoggerManager.Debug("Test Debug");

                //var tmp = (MainWindow)Application.Current.MainWindow;
                //var DrawerHost = tmp.DrawerControl as DrawerHost;

                DrawerHost drawer = Application.Current.Resources["Drawer"] as DrawerHost;

                if (drawer != null)
                {
                    ViewModelManager.TopBarDrawerOpen(true);

                    drawer.IsTopDrawerOpen = !drawer.IsTopDrawerOpen;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _MainWindowHideCommand;
        public ICommand MainWindowHideCommand
        {
            get
            {
                if (null == _MainWindowHideCommand) _MainWindowHideCommand = new AsyncCommand(MainWindowHideCommandFunc, showWaitCancel : false);
                return _MainWindowHideCommand;
            }
        }

        public async Task MainWindowHideCommandFunc()
        {
            try
            {
                // 250912 LJH Widget 안보이게 주석
                //Visibility v = Visibility.Hidden;

                Application.Current.Dispatcher.Invoke
                (
                    () =>
                    
                    {
                        //v = (System.Windows.Application.Current.MainWindow).Visibility;

                        //if (v == Visibility.Visible)
                        //{
                        //    MainMenu.Go_HomeScreen();
                        //    (System.Windows.Application.Current.MainWindow).Hide();                           
                        //    this.ViewModelManager().UpdateWidget();
                        //    this.ViewModelManager().MainWindowWidget.Show();
                        //}
                    }
                );
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }


        private RelayCommand<object> _SendWindowsKeyEventCommand;
        public ICommand SendWindowsKeyEventCommand
        {
            get
            {
                if (null == _SendWindowsKeyEventCommand)
                    _SendWindowsKeyEventCommand = new RelayCommand<object>(SendWindowsKeyFunc);
                return _SendWindowsKeyEventCommand;
            }   
        }

        private void SendWindowsKeyFunc(object obj)
        {
            InputSimulator sim = new InputSimulator();
            sim.Keyboard.KeyPress(VirtualKeyCode.LWIN);
            LoggerManager.Debug($"SendWindowsKeyFunc(): Windows Key Event Fired.");
        }

        private AsyncCommand _StageLockUnLockCommand;
        public ICommand StageLockUnLockCommand
        {
            get
            {
                if (null == _StageLockUnLockCommand) _StageLockUnLockCommand = new AsyncCommand(StageLockUnLockCommandFunc);
                return _StageLockUnLockCommand;
            }
        }

        public async Task StageLockUnLockCommandFunc()
        {
            try
            {
                string text = "";
                if (this.StageSupervisor.GetStageLockMode() == StageLockMode.LOCK)
                {
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                        String superPassword = AccountManager.MakeSuperAccountPassword();

                        if (text.ToLower().CompareTo(superPassword) == 0)
                        {
                            this.StageSupervisor.SetStageUnlock(ReasonOfStageMoveLock.MANUAL);
                        }
                        else
                        {
                            var ret = this.MetroDialogManager().ShowMessageDialog("Stage UnLock Failed", $"Please enter a valid password", EnumMessageStyle.Affirmative);
                        }
                    }));
                }
                else if (this.StageSupervisor.GetStageLockMode() == StageLockMode.UNLOCK)
                {
                    this.StageSupervisor.SetStageLock(ReasonOfStageMoveLock.MANUAL);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

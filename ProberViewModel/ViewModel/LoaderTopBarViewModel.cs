using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderBase.FactoryModules.ViewModelModule;
using LoaderParameters;
using LogModule;
using MaterialDesignThemes.Wpf;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Event;
using ProberInterfaces.Temperature.Chiller;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using Brush = System.Windows.Media.Brush;

namespace LoaderTopBarViewModelModule
{
    public class DateTimeObject : IHasClock, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
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

        public DateTime Now { get; internal set; }

        private System.Timers.Timer Timer;

        public DateTimeObject()
        {
            Timer = new System.Timers.Timer();
            Timer.Interval = 500;
            Timer.Elapsed += GetDateTime;
            Timer.Start();
        }
        ~DateTimeObject()
        {
            Timer.Stop();
        }
        private void GetDateTime(object sender, ElapsedEventArgs e)
        {
            this.DateTimeStr = DateTime.Now;
        }
    }
    //public class LoaderTopBarViewModel : INotifyPropertyChanged, IMainTopBarViewModel, IFactoryModule, IModule, ILoaderMapConvert
    public class LoaderTopBarViewModel : INotifyPropertyChanged, IMainScreenViewModel, IFactoryModule, IModule, ILoaderMapConvert
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;
        private Autofac.IContainer _Container => this.GetLoaderContainer();


        readonly Guid _ViewModelGUID = new Guid("52143deb-97d6-4b8a-8e60-882fbab3abfb");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }


        private ObservableCollection<IStageObject> _Stages;
        public ObservableCollection<IStageObject> Stages
        {
            get { return _Stages; }
            set
            {
                if (value != _Stages)
                {
                    _Stages = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IGPLoader _GPLoader;
        public IGPLoader GPLoader
        {
            get { return _GPLoader; }
            set
            {
                if (value != _GPLoader)
                {
                    _GPLoader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IChillerModule> _Chillers = new ObservableCollection<IChillerModule>();
        public ObservableCollection<IChillerModule> Chillers
        {
            get { return _Chillers; }
            set
            {
                if (value != _Chillers)
                {
                    _Chillers = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ChillerActive;
        public bool ChillerActive
        {
            get { return _ChillerActive; }
            set
            {
                if (value != _ChillerActive)
                {
                    _ChillerActive = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _CellRow;
        public int CellRow
        {
            get { return _CellRow; }
            set
            {
                if (value != _CellRow)
                {
                    _CellRow = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CellColumn;
        public int CellColumn
        {
            get { return _CellColumn; }
            set
            {
                if (value != _CellColumn)
                {
                    _CellColumn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CardTrayRows;
        public int CardTrayRows
        {
            get { return _CardTrayRows; }
            set
            {
                if (value != _CardTrayRows)
                {
                    _CardTrayRows = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _CardTrayColumns;
        public int CardTrayColumns
        {
            get { return _CardTrayColumns; }
            set
            {
                if (value != _CardTrayColumns)
                {
                    _CardTrayColumns = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<EnumSubsStatus> _CardTrays;
        public ObservableCollection<EnumSubsStatus> CardTrays
        {
            get { return _CardTrays; }
            set
            {
                if (value != _CardTrays)
                {
                    _CardTrays = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<int> _CStartIndex = new ObservableCollection<int>();
        public ObservableCollection<int> CStartIndex
        {
            get { return _CStartIndex; }
            set
            {
                if (value != _CStartIndex)
                {
                    _CStartIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<int> _CEndIndex = new ObservableCollection<int>();
        public ObservableCollection<int> CEndIndex
        {
            get { return _CEndIndex; }
            set
            {
                if (value != _CEndIndex)
                {
                    _CEndIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<string> _CTStartIndex = new ObservableCollection<string>();
        public ObservableCollection<string> CTStartIndex
        {
            get { return _CTStartIndex; }
            set
            {
                if (value != _CTStartIndex)
                {
                    _CTStartIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<string> _CTEndIndex = new ObservableCollection<string>();
        public ObservableCollection<string> CTEndIndex
        {
            get { return _CTEndIndex; }
            set
            {
                if (value != _CTEndIndex)
                {
                    _CTEndIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<string> _ChillerIconIndex = new ObservableCollection<string>();
        public ObservableCollection<string> ChillerIconIndex
        {
            get { return _ChillerIconIndex; }
            set
            {
                if (value != _ChillerIconIndex)
                {
                    _ChillerIconIndex = value;
                    RaisePropertyChanged();
                }
            }
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

                DrawerHost drawer = Application.Current.Resources["LoaderDrawer"] as DrawerHost;
                //var DrawerHost = tmp.DrawerControl as DrawerHost;

                if (drawer != null)
                {
                    this.ViewModelManager().ChangeTabControlSelectedIndex(ProberLogLevel.EVENT);
                    this.ViewModelManager().TopBarDrawerOpen(true);

                    drawer.IsTopDrawerOpen = !drawer.IsTopDrawerOpen;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private void UsedBellDrawerOpenCmd(object obj)
        //{
        //    try
        //    {
        //        ObservableCollection<ICellInfo> StagesAlarm = new ObservableCollection<ICellInfo>();
        //        foreach (var stage in Stages)
        //        {
        //            if (Stages != null && stage.StageInfo != null && stage.StageInfo.IsConnected == true)
        //            {
        //                if (stage.StageInfo.ErrorCodeAlarams == null)
        //                {
        //                    stage.StageInfo.ErrorCodeAlarams = new ObservableCollection<ProberInterfaces.ErrorCodeModule.DataParam.ErrorCodeDiscriptionParam>();
        //                    StagesAlarm.Add(stage.StageInfo);
        //                }

        //            }
        //        }
        //        AlarmViewDialogViewModel.Show(StagesAlarm);

        //        //object objStateInfo = obj as IStageObject;
        //        //IStageObject stageObjectInfo = obj as IStageObject;


        //        //IStageObject SelecgtedStage = obj as IStageObject;
        //        //if (SelecgtedStage != null && SelecgtedStage.StageInfo != null && SelecgtedStage.StageInfo.IsConnected == true)
        //        //    {
        //        //    if (SelecgtedStage.StageInfo.ErrorCodeAlarams == null)
        //        //    {
        //        //        SelecgtedStage.StageInfo.ErrorCodeAlarams = new ObservableCollection<ProberInterfaces.ErrorCodeModule.DataParam.ErrorCodeDiscriptionParam>();
        //        //    }

        //        //    AlarmViewDialogViewModel.Show(SelecgtedStage.StageInfo);
        //        //}
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private List<int> _CStartIndex = new List<int>();
        //public List<int> CStartIndex
        //{
        //    get { return _CStartIndex; }
        //    set
        //    {
        //        if (value != _CStartIndex)
        //        {
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private List<int> _CEndIndex = new List<int>();
        //public List<int> CEndIndex
        //{
        //    get { return _CEndIndex; }
        //    set
        //    {
        //        if (value != _CEndIndex)
        //        {
        //            //_CellStartNum.Add(value);
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

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
        private DateTimeObject _DateTime = new DateTimeObject();
        public DateTimeObject DateTime
        {
            get { return _DateTime; }
            set
            {

                if (value != _DateTime)
                {
                    _DateTime = value;
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


        private IGEMModule _GemModule;
        public IGEMModule GemModule
        {
            get { return _GemModule; }
            set
            {
                if (value != _GemModule)
                {
                    _GemModule = value;
                    RaisePropertyChanged();
                }
            }
        }



        private EnumCommunicationState _ChillerCommunicationState;
        public EnumCommunicationState ChillerCommunicationState
        {
            get { return _ChillerCommunicationState; }
            set
            {
                _ChillerCommunicationState = value;
                RaisePropertyChanged();
            }
        }


        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();


        public ILoaderModule LoaderModule => _Container.Resolve<ILoaderModule>();
        public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();

        public void DeInitModule()
        {
            try
            {

                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IViewModelManager ViewModelManager { get; set; }
        public ILoaderViewModelManager LoaderViewModelManager { get; set; }

        public EventCodeEnum InitModule()
        {

            //ViewModelManager = this.ViewModelManager();
            LoaderViewModelManager = this.ViewModelManager() as ILoaderViewModelManager;
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                    SwVersion = fvi.ProductVersion; //minskim// AssemblyInformationalVersion 정보가 있을 경우 해당 정보를 가져오기 위해 수정 함

                    IsEnabledSetTempEmulBtn = Extensions_IParam.ProberRunMode == RunMode.EMUL;

                    // Test Code for Badge

                    BadgeMarginOffset = new Thickness(0, -2, -2, 0);
                    //BadgeBackground = new SolidColorBrush(Colors.Black);
                    //BadgeBackground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFAA38"));
                    //BadgeForeground = new SolidColorBrush(Colors.Red);


                    GemModule = this.GEMModule();
                    BadgeBackground = new SolidColorBrush(Colors.Red);
                    BadgeForeground = new SolidColorBrush(Colors.NavajoWhite);

                    Stages = LoaderCommunicationManager.Cells;
                    GPLoader = _Container.Resolve<IGPLoader>();
                    this.LoaderModule.SetTopBar(this);
                    var chillers = this.EnvControlManager().ChillerManager.GetChillerModules();
                    foreach (var chiller in chillers)
                    {
                        this.Chillers.Add(chiller);
                        ChillerIconIndex.Add("CH" + chiller.ChillerInfo.Index);
                    }


                  


                    var cts = this.LoaderModule.ModuleManager.FindModules<ICardBufferTrayModule>();
                    CardTrays = new ObservableCollection<EnumSubsStatus>();
                    CTStartIndex = new ObservableCollection<string>();
                    foreach (var ct in cts)
                    {
                        CardTrays.Add(EnumSubsStatus.UNDEFINED);
                    }
                    if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                    {
                        CellColumn = 6;
                        CellRow = 2;
                        CardTrayColumns = 1;
                        CardTrayRows = 5;
                        for (int i = 0; i < CardTrayRows; i++)
                        {
                            CTStartIndex.Add($"CT{i + 1}");
                        }
                    }
                    else
                    {
                        CellColumn = 4;
                        CellRow = 3;
                        var cardTrayCount = CardTrays.Count;
                        if (cardTrayCount == 0)
                        {
                            cardTrayCount = cardTrayCount / CellRow;
                        }
                        var rowCount = (double)cardTrayCount / (double)CellRow;
                        CardTrayRows = (int)Math.Ceiling(rowCount);
                        for (int i = 0; i < cardTrayCount; i++)
                        {
                            if (i % CellRow == 0)
                            {
                                CTStartIndex.Add($"CT{i + 1}");
                            }
                        }


                        CardTrayColumns = 3;
                    }

                    CellIndexSort();

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
        //private void AddStartNum(int retVal)
        //{
        //    try
        //    {
        //        //CStartIndex.Add(retVal);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //private void AddEndNum(int retVal)
        //{
        //    try
        //    {
        //        //CEndIndex.Add(retVal);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public IEnumerable<TResult> SortBy<TResult, TKey>(IEnumerable<TResult> itemsToSort, IEnumerable<TKey> sortKeys, Func<TResult, TKey> matchFunc)
        {
            return sortKeys.Join(itemsToSort, key => key, matchFunc, (key, iitem) => iitem);
        }

        private void CellIndexSort()
        {
            try
            {
                List<int> sortindex = new List<int>();

                //left top
                for (int i = 1; i <= CellRow; i++)
                {

                    for (int j = 0; j < CellColumn; j++)
                    {
                        sortindex.Add((i - 1) * CellColumn + j + 1);
                    }

                    // 0, 3 / 4, 7/ 8, 11
                    //AddStartNum(sortindex[(i-1)*CellColumn]);
                    //AddEndNum(sortindex[i*CellColumn - 1]);

                    CStartIndex.Add(sortindex[(i - 1) * CellColumn]);
                    CEndIndex.Add(sortindex[i * CellColumn - 1]);
                }


                Stages = new ObservableCollection<IStageObject>(SortBy(Stages, sortindex, c => c.Index));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _OpenRecoveryCommand;
        public ICommand OpenRecoveryCommand
        {
            get
            {
                if (null == _OpenRecoveryCommand) _OpenRecoveryCommand = new AsyncCommand(OpenRecoveryFunc);
                return _OpenRecoveryCommand;
            }
        }
        private async Task OpenRecoveryFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    this.GetGPLoader().LoaderBuzzer(false);
                    LoaderRecoveryControl.LoaderRecoveryControlVM.Show(_Container, LoaderModule.ResonOfError, LoaderModule.ErrorDetails);
                    LoaderModule.ResonOfError = "";
                    LoaderModule.ErrorDetails = "";
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //return Task.Run(() =>
            //{
            //    try
            //    {
            //        this.GetGPLoader().LoaderBuzzer(false);
            //        LoaderRecoveryControl.LoaderRecoveryControlVM.Show(_Container, LoaderModule.ResonOfError);
            //    }
            //    catch (Exception err)
            //    {
            //    }
            //});
        }

        private AsyncCommand _GoHomeScreen;
        public ICommand GoHomeScreen
        {
            get
            {
                if (null == _GoHomeScreen) _GoHomeScreen = new AsyncCommand(HomeScreen);
                return _GoHomeScreen;
            }
        }

        private async Task HomeScreen()
        {
            try
            {
                //await this.ViewModelManager().ViewTransitionAsync(new Guid("DD8BED78-B2A7-974E-6941-C970993050FD"));
                //await this.ViewModelManager().ViewTransitionAsync(new Guid(this.ViewModelManager().HomeViewGuid));
                await this.ViewModelManager().ViewTransitionAsync(this.ViewModelManager().HomeViewGuid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand _BuzzerCommand;
        public ICommand BuzzerCommand
        {
            get
            {
                if (null == _BuzzerCommand) _BuzzerCommand = new AsyncCommand(BuzzerCommandFunc);
                return _BuzzerCommand;
            }
        }

        private async Task BuzzerCommandFunc()
        {
            try
            {
                if (GPLoader.IsBuzzerOn)
                {
                    GPLoader.LoaderBuzzer(false);           // 이 코드 삭제해도 되는지? 
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(BuzzerOffStateEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();
                }
                else
                {
                    GPLoader.LoaderBuzzer(true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<object> _GoPrevScreen;
        public ICommand GoPrevScreen
        {
            get
            {
                if (null == _GoPrevScreen) _GoPrevScreen = new AsyncCommand<object>(PrevScreen);
                return _GoPrevScreen;
            }
        }

        private async Task PrevScreen(object noparam)
        {
            try
            {
                await this.ViewModelManager().BackPreScreenTransition();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _StageSummaryPageSwitchingCommand;
        public ICommand StageSummaryPageSwitchingCommand
        {
            get
            {
                if (null == _StageSummaryPageSwitchingCommand) _StageSummaryPageSwitchingCommand = new AsyncCommand(FuncStageSummaryPageSwitchingCommand);
                return _StageSummaryPageSwitchingCommand;
            }
        }
        private async Task FuncStageSummaryPageSwitchingCommand()
        {
            try
            {
                //await this.ViewModelManager().ViewTransitionAsync(new Guid(this.ViewModelManager().HomeViewGuid));
                await this.ViewModelManager().ViewTransitionAsync(this.ViewModelManager().HomeViewGuid);

                //(this.ViewModelManager() as ILoaderViewModelManager)?.CloseMainMenu();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _OpenFlyout;
        public ICommand OpenFlyout
        {
            get
            {
                if (null == _OpenFlyout) _OpenFlyout = new AsyncCommand(FuncOpenFlyout);
                return _OpenFlyout;
            }
        }

        public async Task FuncOpenFlyout()
        {
            try
            {
                this.ViewModelManager().ChangeFlyOutControlStatus(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //FlyoutIsOpen = true;
        }

        private AsyncCommand _ChillerPageSwitchingCommand;
        public ICommand ChillerPageSwitchingCommand
        {
            get
            {
                if (null == _ChillerPageSwitchingCommand) _ChillerPageSwitchingCommand = new AsyncCommand(ChillerPageSwitchingCommandFunc);
                return _ChillerPageSwitchingCommand;
            }
        }
        private async Task ChillerPageSwitchingCommandFunc()
        {
            try
            {
                if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    await this.ViewModelManager().ViewTransitionAsync(new Guid("3817ed23-c61a-47a1-8e97-6fc2c3a210c4"));
                }
                else
                {
                    await this.ViewModelManager().ViewTransitionAsync(new Guid("7797A3B8-5CF5-FCCD-22BB-F612618C4B34"));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _BtnClick;
        public ICommand BtnClick
        {
            get
            {
                if (null == _BtnClick) _BtnClick = new RelayCommand<object>(BtnClickCmd);
                return _BtnClick;
            }
        }
        private void BtnClickCmd(object obj)
        {
            try
            {
                EventCodeParam temp = new EventCodeParam();
                temp.EventCode = EventCodeEnum.AIRBLOW_TEMPERATURE_CONTROL_ERROR;
                temp.Message = "AIRBLOW TEMPERATURE CONTROL ERROR";
                temp.OccurTime = System.DateTime.Now;
                temp.IsChecked = false;
                temp.ProberErrorKind = EnumProberErrorKind.INVALID;
                temp.Index = 1;
                LoaderMaster.NotifyStageAlarm(temp);
                LoggerManager.Event(0, System.DateTime.Now, ProberErrorCode.EventCodeEnum.AIRBLOW_CLEANING_ERROR, "AIRBLOW CLEANING ERROR", false, true);
                LoggerManager.Event(2, System.DateTime.Now, ProberErrorCode.EventCodeEnum.BIN_PF_DATA_IS_NOT_MATCHED, "BIN PF DATA IS NOT MATCHED", false, true);
                //LoggerManager.Event(3, System.DateTime.Now, ProberErrorCode.EventCodeEnum.CARD_CHANGE_FAIL, "CARD CHANGE FAIL");
                //LoggerManager.Event(4, System.DateTime.Now, ProberErrorCode.EventCodeEnum.CASSETTE_LOCK_ERROR, "CASSETTE LOCK ERROR");
                //LoggerManager.Event(5, System.DateTime.Now, ProberErrorCode.EventCodeEnum.CHILLER_PUMP_ERROR, "CHILLER PUMP ERROR");
                //LoggerManager.Event(6, System.DateTime.Now, ProberErrorCode.EventCodeEnum.DEW_POINT_HIGH_ERR, "DEW POINT HIGH ERR");
                //LoggerManager.Event(7, System.DateTime.Now, ProberErrorCode.EventCodeEnum.DRYAIR_SETVALUE_ERROR, "DRYAIR SETVALUE ERROR");
                //LoggerManager.Event(8, System.DateTime.Now, ProberErrorCode.EventCodeEnum.EDGE_SKIPED, "EDGE SKIPED");
                //LoggerManager.Event(9, System.DateTime.Now, ProberErrorCode.EventCodeEnum.FOCUS_FAILED, "FOCUS FAILED");
                //LoggerManager.Event(10, System.DateTime.Now, ProberErrorCode.EventCodeEnum.FOCUS_VALUE_FLAT, "FOCUS VALUE FLAT");
                //LoggerManager.Event(11, System.DateTime.Now, ProberErrorCode.EventCodeEnum.GP_CardChange_CARD_ALIGN_FAIL, "GP CardChange CARD ALIGN FAIL");
                //LoggerManager.Event(12, System.DateTime.Now, ProberErrorCode.EventCodeEnum.LOADER_BUFF_FAILED, "LOADER BUFF FAILED");

                LoggerManager.Prolog(PrologType.INFORMATION, "1d5d6f5f5", EventCodeEnum.ARM_VAC_ERROR, "Loadr Arm Vac Error Occured.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public async Task LoaderMapConvert(LoaderMap map)
        {
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < map.CardTrayModules.Count(); i++)
                {
                    CardTrays[i] = map.CardTrayModules[i].WaferStatus;
                }

            }));

        }

        public void UpdateChanged()
        {
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
    }
}

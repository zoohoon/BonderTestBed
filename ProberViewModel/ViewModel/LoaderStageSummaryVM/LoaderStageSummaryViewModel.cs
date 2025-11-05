using AccountModule;
using AlarmViewDialog;
using AlignRecoveryViewDialog;
using Autofac;
using EmulGemView;
using LoaderBase;
using LoaderBase.Communication;
using LoaderBase.FactoryModules.ViewModelModule;
using LoaderMapView;
using LoaderMaster;
using LoaderParameters;
using LogModule;
using MahApps.Metro.Controls;
using MetroDialogInterfaces;
using OPUSV3DView;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.DialogControl;
using ProberInterfaces.Foup;
using ProberInterfaces.Loader;
using ProberInterfaces.LoaderController;
using ProberInterfaces.Monitoring;
using ProberInterfaces.Param;
using ProberViewModel.View.Summary;
using ProberViewModel.ViewModel;
using RelayCommandBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UcDisplayPort;
using VirtualKeyboardControl;
using StageStateEnum = LoaderBase.Communication.StageStateEnum;

namespace LoaderStageSummaryViewModelModule
{
    public class LoaderStageSummaryViewModel : IMainScreenViewModel, INotifyPropertyChanged, IFactoryModule, ILoaderStageSummaryViewModel, ILoaderMapConvert
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("6e199680-a422-4882-841d-cd4628a8c009");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();

        public IMotionManager MotionManager => _Container.Resolve<IMotionManager>();

        public ILoaderViewModelManager LoaderViewModelManager => (ILoaderViewModelManager)this.ViewModelManager();

        public IDeviceManager DeviceManager => _Container.Resolve<IDeviceManager>();

        private Dictionary<TabControlEnum, int> TabControlMappingDictionary = new Dictionary<TabControlEnum, int>();

        public ILotOPModule LotOPModule => this.LotOPModule();
        public IStageSupervisor StageSupervisor => this.StageSupervisor();

        private ILoaderModule _LoaderModule;
        public ILoaderModule LoaderModule
        {
            get { return _LoaderModule; }
            set
            {
                if (value != _LoaderModule)
                {

                    _LoaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ILoaderDoorDisplayDialogService LoaderDoorDialog => _Container.Resolve<ILoaderDoorDisplayDialogService>();

        public ILoaderParkingDisplayDialogService LoaderParkingDialog => _Container.Resolve<ILoaderParkingDisplayDialogService>();
        private ILoaderSupervisor _LoaderMaster;
        public ILoaderSupervisor LoaderMaster
        {
            get { return _LoaderMaster; }
            set
            {
                if (value != _LoaderMaster)
                {

                    _LoaderMaster = value;
                    RaisePropertyChanged();
                }
            }
        }
        private DisplayPort _DisplayPort;
        public DisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
                if (value == null)
                {
                    WirteLogCallerMethodFormSetDisplayPort();
                }
            }
        }
        private OPERA_STAGE_3DView _OPERA3DView;
        public OPERA_STAGE_3DView OPERA3DView
        {
            get { return _OPERA3DView; }
            set
            {
                if (value != _OPERA3DView)
                {
                    _OPERA3DView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void WirteLogCallerMethodFormSetDisplayPort()
        {
            try
            {
                StackTrace stackTrace = new StackTrace();
                // get calling method name
                LoggerManager.Debug($"{stackTrace.GetFrame(1).GetMethod().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ManualOperationCommand _ManualOPCommandSet;
        public ManualOperationCommand ManualOPCommandSet
        {
            get { return _ManualOPCommandSet; }
            set
            {
                if (value != _ManualOPCommandSet)
                {
                    _ManualOPCommandSet = value;
                    RaisePropertyChanged();
                }
            }
        }



        //private string _SoakingType;
        //public string SoakingType
        //{
        //    get { return _SoakingType; }
        //    set
        //    {
        //        if (value != _SoakingType)
        //        {
        //            _SoakingType = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private string _SoakRemainTIme;
        //public string SoakRemainTIme
        //{
        //    get { return _SoakRemainTIme; }
        //    set
        //    {
        //        if (value != _SoakRemainTIme)
        //        {
        //            _SoakRemainTIme = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private string _SoakZClearance;
        //public string SoakZClearance
        //{
        //    get { return _SoakZClearance; }
        //    set
        //    {
        //        if (value != _SoakZClearance)
        //        {
        //            _SoakZClearance = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private string _StageMoveState;
        public string StageMoveState
        {
            get { return _StageMoveState; }
            set
            {
                if (value != _StageMoveState)
                {
                    _StageMoveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private OCR_OperateMode _OCRMode;
        public OCR_OperateMode OCRMode
        {
            get { return _OCRMode; }
            set
            {
                if (value != _OCRMode)
                {
                    _OCRMode = value;
                    RaisePropertyChanged();
                }
            }
        }
        private DynamicModeEnum _DynamicMode;
        public DynamicModeEnum DynamicMode
        {
            get { return _DynamicMode; }
            set
            {
                if (value != _DynamicMode)
                {
                    _DynamicMode = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private SelectionMode _CellListSelectionMode;
        public SelectionMode CellListSelectionMode
        {
            get { return _CellListSelectionMode; }
            set
            {
                if (value != _CellListSelectionMode)
                {
                    _CellListSelectionMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SubstrateSizeEnum _DevSize = SubstrateSizeEnum.INCH6;
        public SubstrateSizeEnum DevSize
        {
            get { return _DevSize; }
            set
            {
                if (value != _DevSize)
                {
                    _DevSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ObservableCollection<FoupObject> _Foups = new ObservableCollection<FoupObject>();
        //public ObservableCollection<FoupObject> Foups
        //{
        //    get { return _Foups; }
        //    set
        //    {
        //        if (value != _Foups)
        //        {
        //            _Foups = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        public void UpdateChanged()
        {
            if (SelectedStage != null && SelectedStage.StageInfo != null && SelectedStage.StageInfo.IsConnected == true)
            {
                //GetCellInfo(SelectedStage);
                if (IsTimerEnabled == true && UpdateTimer != null)
                {
                    TimeLeft = UpdateTimerInterval;
                }
            }
            if (SelectedStage != null)
            {
                SelectedStage.StageState = LoaderMaster.StageStates[SelectedStage.Index - 1];
            }
            if (SelectedStage == null)
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = false;
                CellLotStartFlag = false;
            }
            else if (SelectedStage.IsRecoveryMode == true)
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = false;
                CellLotStartFlag = false;
            }
            else if (this.LoaderCommunicationManager.Cells != null && SelectedStage.Index - 1 >= 0 && LoaderCommunicationManager.Cells[SelectedStage.Index - 1].StageMode != GPCellModeEnum.ONLINE)
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = false;
                CellLotStartFlag = false;
            }
            else if (SelectedStage.StageState == ModuleStateEnum.RUNNING || SelectedStage.StageState == ModuleStateEnum.ABORT || SelectedStage.StageState == ModuleStateEnum.RESUMMING)
            {
                CellLotPauseFlag = true;
                CellLotResumeFlag = false;
                CellLotEndFlag = false;
                CellLotStartFlag = false;
            }
            else if (SelectedStage.StageState == ModuleStateEnum.PAUSED && (SelectedStage.StageInfo.LotData != null && SelectedStage.StageInfo.LotData.LotAbortedByUser == true))
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = true;
                CellLotStartFlag = false;
            }
            else if (SelectedStage.StageState == ModuleStateEnum.PAUSED)
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = true;
                CellLotEndFlag = true;
                CellLotStartFlag = false;
            }
            else if (SelectedStage.StageState == ModuleStateEnum.IDLE)
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = false;
                CellLotStartFlag = true;
            }
            else
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = false;
                CellLotStartFlag = false;
            }
        }

        private int _SelectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set
            {
                if (value != _SelectedTabIndex)
                {
                    _SelectedTabIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedOperationTabIndex;
        public int SelectedOperationTabIndex
        {
            get { return _SelectedOperationTabIndex; }
            set
            {
                if (value != _SelectedOperationTabIndex)
                {
                    _SelectedOperationTabIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IStageObject _SelectedStage;
        public IStageObject SelectedStage
        {
            get { return _SelectedStage; }
            set
            {
                if (value != _SelectedStage)
                {
                    _SelectedStage = value;
                    RaisePropertyChanged();


                    if (SelectedTabIndex != Convert.ToInt32(TabControlEnum.CELL))
                    {
                        SelectedTabIndex = Convert.ToInt32(TabControlEnum.CELL);
                    }
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

        private CellIndexDirection _CellDirection;
        public CellIndexDirection CellDirection
        {
            get { return _CellDirection; }
            set
            {
                if (value != _CellDirection)
                {
                    _CellDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _UpdateTimerInterval;
        public int UpdateTimerInterval
        {
            get { return _UpdateTimerInterval; }
            set
            {
                if (value != _UpdateTimerInterval)
                {
                    _UpdateTimerInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _TimeLeft;
        public int TimeLeft
        {
            get { return _TimeLeft; }
            set
            {
                if (value != _TimeLeft)
                {
                    _TimeLeft = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsTimerEnabled = false;
        public bool IsTimerEnabled
        {
            get { return _IsTimerEnabled; }
            set
            {
                if (value != _IsTimerEnabled)
                {
                    _IsTimerEnabled = value;

                    if (_IsTimerEnabled == true)
                    {
                        StartUpdateTimer();
                    }
                    else
                    {
                        EndUpdateTimer();
                    }

                    RaisePropertyChanged();
                }
            }
        }

        private int _ChangedSelectedItemsCount = 0;
        public int ChangedSelectedItemsCount
        {
            get { return _ChangedSelectedItemsCount; }
            set
            {
                _ChangedSelectedItemsCount = value;
                RaisePropertyChanged();
                //if (value != _ChangedSelectedItemsCount)
                //{
                //    _ChangedSelectedItemsCount = value;
                //    RaisePropertyChanged();
                //}
            }
        }

        public EventCodeEnum InitModule()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
        }

        private AsyncCommand<object> _OCRDebugingEnableCommand;
        public ICommand OCRDebugingEnableCommand
        {
            get
            {
                if (null == _OCRDebugingEnableCommand)
                    _OCRDebugingEnableCommand = new AsyncCommand<object>(OCRDebugingEnableCommandFunc);
                return _OCRDebugingEnableCommand;
            }
        }

        private async Task OCRDebugingEnableCommandFunc(object param)
        {
            try
            {
                string text = null;
                LoaderMaster.IsSuperUser = false;
                if (!LoaderMaster.OCRDebugginFlag)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                        String superPassword = AccountManager.MakeSuperAccountPassword();

                        if (text.ToLower().CompareTo(superPassword) == 0)
                        {
                            LoaderMaster.IsSuperUser = true;
                            LoaderMaster.OCRDebugginFlag = true;
                            LoaderMaster.IsSuperUser = false;
                        }
                        else
                        {
                            LoaderMaster.OCRDebugginFlag = false;
                        }
                    }));
                }
                else
                {
                    LoaderMaster.IsSuperUser = true;
                    LoaderMaster.OCRDebugginFlag = false;
                    LoaderMaster.IsSuperUser = false;
                }
                LoaderMaster.IsSuperUser = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _ContinueLotEnableCommand;
        public ICommand ContinueLotEnableCommand
        {
            get
            {
                if (null == _ContinueLotEnableCommand)
                    _ContinueLotEnableCommand = new AsyncCommand<object>(ContinueLotEnableCommandFunc);
                return _ContinueLotEnableCommand;
            }
        }

        private async Task ContinueLotEnableCommandFunc(object param)
        {
            try
            {
                string text = null;
                LoaderMaster.IsSuperUser = false;
                if (!LoaderMaster.ContinueLotFlag)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                        String superPassword = AccountManager.MakeSuperAccountPassword();

                        if (text.ToLower().CompareTo(superPassword) == 0)
                        {
                            LoaderMaster.IsSuperUser = true;
                            LoaderMaster.ContinueLotFlag = true;

                            LoaderMaster.IsSuperUser = false;
                        }
                        else
                        {
                            LoaderMaster.ContinueLotFlag = false;
                        }
                    }));
                }
                else
                {
                    LoaderMaster.IsSuperUser = true;
                    LoaderMaster.ContinueLotFlag = false;
                    LoaderMaster.IsSuperUser = false;
                }
                LoaderMaster.IsSuperUser = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool _CellLotStartFlag;
        public bool CellLotStartFlag
        {
            get { return _CellLotStartFlag; }
            set
            {
                if (value != _CellLotStartFlag)
                {

                    _CellLotStartFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellLotPauseFlag;
        public bool CellLotPauseFlag
        {
            get { return _CellLotPauseFlag; }
            set
            {
                if (value != _CellLotPauseFlag)
                {

                    _CellLotPauseFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellLotResumeFlag;
        public bool CellLotResumeFlag
        {
            get { return _CellLotResumeFlag; }
            set
            {
                if (value != _CellLotResumeFlag)
                {

                    _CellLotResumeFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CellLotEndFlag;
        public bool CellLotEndFlag
        {
            get { return _CellLotEndFlag; }
            set
            {
                if (value != _CellLotEndFlag)
                {

                    _CellLotEndFlag = value;




                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CellListSelectionMode = SelectionMode.Single;

                if (ManualOPCommandSet == null)
                {
                    ManualOPCommandSet = new ManualOperationCommand(this);
                }

                DisplayPort = new DisplayPort();
                DisplayPort.EnalbeClickToMove = false;

                ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;

                // TODO : MotionProxy 사용과 같이 살펴봐야 됨.
                //OPERA3DView = new OPERA_STAGE_3DView();
                // OPERA3DView.DataContext = this;

                LoaderMaster = _Container.Resolve<ILoaderSupervisor>();
                LoaderModule = _Container.Resolve<ILoaderModule>();

                Stages = LoaderCommunicationManager.Cells;


                foreach (var stage in Stages)
                {
                    stage.StageInfo.SetTitles = new ObservableCollection<string>();
                }
                //if(LoaderModule.Foups.Count > 0)
                //{
                //    Foups = LoaderModule.Foups;
                //}

                LoaderModule.SetLoaderMapConvert(this);
                LoaderModule.BroadcastLoaderInfo();

                SelectedTabIndex = 0;
                SelectedOperationTabIndex = 0;

                UpdateTimerInterval = 30;

                if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    CellColumn = 6;
                    CellRow = 2;

                }
                else
                {
                    CellColumn = 4;
                    CellRow = 3;
                }

                CellDirection = CellIndexDirection.TOP_AND_LEFT;

                CellIndexSort();

                GetCellsInfo();

                MakeTabControlMappingDictionary();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private void MakeTabControlMappingDictionary()
        {
            try
            {
                if (TabControlMappingDictionary == null)
                {
                    TabControlMappingDictionary = new Dictionary<TabControlEnum, int>();
                }

                TabControlMappingDictionary.Clear();

                TabControlMappingDictionary.Add(TabControlEnum.FOUP, 0);
                TabControlMappingDictionary.Add(TabControlEnum.CELL, 1);
                TabControlMappingDictionary.Add(TabControlEnum.LOTOPTION, 2);
                TabControlMappingDictionary.Add(TabControlEnum.VISION, 3);
                TabControlMappingDictionary.Add(TabControlEnum.MONITORING, 4);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                if (this.DisplayPort == null)
                {
                    this.DisplayPort = new DisplayPort();
                    DisplayPort.EnalbeClickToMove = false;

                    LotOPModule.ViewTarget = this.DisplayPort;
                }

                LoaderModule.SetLoaderMapConvert(this);
                LoaderModule.BroadcastLoaderInfo();

                if (IsTimerEnabled == true)
                {
                    StartUpdateTimer();
                    //LoggerManager.Debug("StageSummaryVM_PageSwitched : StartUpdateTimer() done");
                }
                if (SelectedStage != null && SelectedStage.StageInfo != null && SelectedStage.StageInfo.IsConnected == true)
                {
                    GetCellInfo(SelectedStage);
                    //LoggerManager.Debug("StageSummaryVM_PageSwitched : GetCellInfo() done");
                }
                //}
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
                //});
                this.VisionManager().SetDisplayChannel(null, DisplayPort);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private static System.Timers.Timer UpdateTimer;

        private void StartUpdateTimer()
        {
            try
            {
                //int IsConnectedStageCount = 0;

                // 연결되어 있는 스테이지가 하나라도 있다면
                //foreach (var stage in Stages)
                //{
                //    if (stage.StageInfo.IsConnected == true)
                //    {
                //        IsConnectedStageCount++;
                //    }
                //}

                if (SelectedStage != null && SelectedStage.StageInfo.IsConnected == true)
                {
                    UpdateTimer = new System.Timers.Timer();
                    // Hook up the Elapsed event for the timer.
                    UpdateTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

                    // Set the Interval to 2 seconds (2000 milliseconds).
                    UpdateTimer.Interval = 1000;
                    UpdateTimer.Enabled = true;

                    IsTimerEnabled = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void EndUpdateTimer()
        {
            try
            {
                if (UpdateTimer != null)
                {
                    UpdateTimer.Stop();
                    UpdateTimer.Enabled = false;
                    UpdateTimer.Dispose();

                    TimeLeft = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                if (TimeLeft > 0)
                {
                    // Display the new time left
                    // by updating the Time Left label.
                    TimeLeft = TimeLeft - 1;
                }
                else
                {
                    UpdateTimer.Stop();

                    //IsTimerEnabled = false;

                    // Call Update Function & Start Timer

                    if (SelectedStage != null && SelectedStage.StageInfo.IsConnected == true)
                    {
                        GetCellInfo(SelectedStage);
                    }

                    //GetCellsInfo();

                    TimeLeft = UpdateTimerInterval;
                    UpdateTimer.Enabled = true;
                    //IsTimerEnabled = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                EndUpdateTimer();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private void GetCellsInfo()
        {
            try
            {
                foreach (var stage in Stages)
                {
                    if (stage.StageInfo.IsConnected == true)
                    {
                        GetCellInfo(stage);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GetCellInfo(IStageObject stage)
        {
            try
            {
                stage.StageInfo.LotData = LoaderMaster.GetStageLotData(stage.Index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private long _MapIndexX;
        public long MapIndexX
        {
            get { return _MapIndexX; }
            set
            {
                if (value != _MapIndexX)
                {
                    _MapIndexX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _MapIndexY;
        public long MapIndexY
        {
            get { return _MapIndexY; }
            set
            {
                if (value != _MapIndexY)
                {
                    _MapIndexY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private void CellIndexSort()
        {
            try
            {
                int CellCount = Stages.Count;

                if (CellCount == CellRow * CellColumn)
                {
                    if (CellDirection == CellIndexDirection.BOTTOM_AND_LEFT)
                    {
                        // 09 10 11 12
                        // 05 06 07 08
                        // 01 02 03 04

                        List<int> sortindex = new List<int>();

                        for (int i = 1; i <= CellRow; i++)
                        {
                            int StartNo = CellCount - (CellColumn * (i)) + 1;
                            //int FloorNo = (CellRow - i);

                            for (int j = 0; j < CellColumn; j++)
                            {
                                sortindex.Add(StartNo + j);
                            }
                        }

                        Stages = new ObservableCollection<IStageObject>(SortBy(Stages, sortindex, c => c.Index));
                    }
                    else if (CellDirection == CellIndexDirection.BOTTOM_AND_Right)
                    {
                    }
                    else if (CellDirection == CellIndexDirection.TOP_AND_LEFT)
                    {

                        // 01 02 03 04
                        // 05 06 07 08
                        // 09 10 11 12

                        List<int> sortindex = new List<int>();

                        for (int i = 1; i <= CellRow; i++)
                        {
                            int StartNo = CellCount - (CellColumn * (CellRow - (i - 1))) + 1;
                            //int FloorNo = (CellRow - i);

                            for (int j = 0; j < CellColumn; j++)
                            {
                                sortindex.Add(StartNo + j);
                            }
                        }

                        Stages = new ObservableCollection<IStageObject>(SortBy(Stages, sortindex, c => c.Index));
                    }
                    else if (CellDirection == CellIndexDirection.TOP_AND_Right)
                    {
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IEnumerable<TResult> SortBy<TResult, TKey>(IEnumerable<TResult> itemsToSort, IEnumerable<TKey> sortKeys, Func<TResult, TKey> matchFunc)
        {
            return sortKeys.Join(itemsToSort, key => key, matchFunc, (key, iitem) => iitem);
        }

        private AsyncCommand<object> _CellInfoRenewCommand;
        public ICommand CellInfoRenewCommand
        {
            get
            {
                if (null == _CellInfoRenewCommand)
                    _CellInfoRenewCommand = new AsyncCommand<object>(CellInfoRenewCommandFunc);
                return _CellInfoRenewCommand;
            }
        }

        private async Task CellInfoRenewCommandFunc(object param)
        {
            try
            {
                // 선택된 셀이 존재하고, 연결되어 있을 때 데이터를 얻어올 수 있다.
                if (SelectedStage != null && SelectedStage.StageInfo != null && SelectedStage.StageInfo.IsConnected == true)
                {
                    GetCellInfo(SelectedStage);
                    StageMoveState = SelectedStage.StageInfo.LotData.StageMoveState;
                    if (IsTimerEnabled == true && UpdateTimer != null)
                    {
                        TimeLeft = UpdateTimerInterval;
                    }
                }
                else
                {
                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Update data", $"The connected cell does not exist.", EnumMessageStyle.Affirmative).Result;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private AsyncCommand<object> _SingleSelectCommand;
        public ICommand SingleSelectCommand
        {
            get
            {
                if (null == _SingleSelectCommand)
                    _SingleSelectCommand = new AsyncCommand<object>(SingleSelectCommandFunc);
                return _SingleSelectCommand;
            }
        }

        private async Task SingleSelectCommandFunc(object param)
        {
            try
            {
                CellListSelectionMode = SelectionMode.Single;

                UnselectedAllCellsCommandFunc(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _MultipleSelectCommand;
        public ICommand MultipleSelectCommand
        {
            get
            {
                if (null == _MultipleSelectCommand)
                    _MultipleSelectCommand = new AsyncCommand<object>(MultipleSelectCommandFunc);
                return _MultipleSelectCommand;
            }
        }

        private async Task MultipleSelectCommandFunc(object param)
        {
            try
            {
                CellListSelectionMode = SelectionMode.Multiple;

                UnselectedAllCellsCommandFunc(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _CollectWaferCommand;
        public ICommand CollectWaferCommand
        {
            get
            {
                if (null == _CollectWaferCommand)
                    _CollectWaferCommand = new AsyncCommand<object>(CollectWaferCommandFunc);
                return _CollectWaferCommand;
            }
        }

        private async Task CollectWaferCommandFunc(object param)
        {
            try
            {
                EventCodeEnum errorCode = EventCodeEnum.NONE;
                var retVal = await (this).MetroDialogManager().ShowMessageDialog("Collect All Wafer", "Do you Want Collect All Wafer?", EnumMessageStyle.AffirmativeAndNegative);
                string msg = "";
                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    (errorCode, msg) = await this.LoaderMaster.CollectAllWafer();
                    if (!(errorCode == EventCodeEnum.NONE))
                    {
                        retVal = await (this).MetroDialogManager().ShowMessageDialog("Collect All Wafer Error", msg, EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _SoakStopCommand;
        public ICommand SoakStopCommand
        {
            get
            {
                if (null == _SoakStopCommand)
                    _SoakStopCommand = new AsyncCommand<object>(SoakStopCommandFunc);
                return _SoakStopCommand;
            }
        }

        private async Task SoakStopCommandFunc(object param)
        {
            try
            {
                int stageindex = LoaderCommunicationManager.SelectedStageIndex;

                Task task = new Task(() =>
                {
                    this.SoakingModule().SetCancleFlag(true, stageindex);
                    var stage = LoaderCommunicationManager.GetStage(stageindex);

                    if (stage != null)
                    {
                        if (stage.StageInfo != null)
                        {
                            if (stage.StageInfo.LotData != null)
                            {
                                stage.StageInfo.LotData.SoakingZClearance = "N/A";
                                stage.StageInfo.LotData.SoakingRemainTime = 0.ToString();
                            }
                        }
                    }

                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<object> _StageMoveLockCommand;
        public ICommand StageMoveLockCommand
        {
            get
            {
                if (null == _StageMoveLockCommand)
                    _StageMoveLockCommand = new AsyncCommand<object>(StageMoveLockCommandFunc);
                return _StageMoveLockCommand;
            }
        }
        private async Task StageMoveLockCommandFunc(object param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var stages = LoaderCommunicationManager.GetStages();

                if (stages.Count(stage => stage.StageInfo.IsChecked) > 0)
                {
                    foreach (var stage in stages)
                    {
                        if (stage.StageInfo.IsChecked)
                        {
                            if (stage != null && stage.StageInfo != null && stage.StageInfo.IsConnected == true)
                            {
                                LoggerManager.Debug($"StageMoveLockCommand() Stage: {stage.Index}");
                                retVal = LoaderMaster.SetStageMoveLockState(stage.Index, ReasonOfStageMoveLock.MANUAL);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _StageMoveUnLockCommand;
        public ICommand StageMoveUnLockCommand
        {
            get
            {
                if (null == _StageMoveUnLockCommand)
                    _StageMoveUnLockCommand = new AsyncCommand<object>(StageMoveUnLockCommandFunc);
                return _StageMoveUnLockCommand;
            }
        }
        private async Task StageMoveUnLockCommandFunc(object param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                string text = "";
                bool checkpassword = false;
                var stages = LoaderCommunicationManager.GetStages();

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                    String superPassword = AccountManager.MakeSuperAccountPassword();

                    if (text.ToLower().CompareTo(superPassword) == 0)
                    {
                        checkpassword = true;
                    }
                    else
                    {
                        var ret = this.MetroDialogManager().ShowMessageDialog("Stage UnLock Failed", $"Please enter a valid password", EnumMessageStyle.Affirmative);
                    }
                }));

                if (stages.Count(stage => stage.StageInfo.IsChecked) > 0 && checkpassword == true)
                {
                    foreach (var stage in stages)
                    {
                        if (stage.StageInfo.IsChecked)
                        {
                            if (stage != null && stage.StageInfo != null && stage.StageInfo.IsConnected == true)
                            {
                                LoggerManager.Debug($"StageMoveUnLockCommand() Stage: {stage.Index}");
                                retVal = LoaderMaster.SetStageMoveUnLockState(stage.Index, ReasonOfStageMoveLock.MANUAL);
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
        //private bool _StageMoveToggleBnt_IsEnabled = false;
        //public bool StageMoveToggleBnt_IsEnabled
        //{
        //    get
        //    {
        //        StageLockStateIsEnabled();
        //        return _StageMoveToggleBnt_IsEnabled;
        //    }
        //    set
        //    {
        //        if (value != _StageMoveToggleBnt_IsEnabled)
        //        {

        //            _StageMoveToggleBnt_IsEnabled = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //public void StageLockStateIsEnabled()
        //{
        //    if (SelectedStage.StageInfo.LotData.CellMode == GPCellModeEnum.MAINTENANCE)
        //    {
        //        StageMoveToggleBnt_IsEnabled = true;
        //    }
        //    else
        //    {
        //        StageMoveToggleBnt_IsEnabled = false;
        //    }
        //}
        private AsyncCommand<GPCellModeEnum> _StageConnectStatusChangeCommand;
        public ICommand StageConnectStatusChangeCommand
        {
            get
            {
                if (null == _StageConnectStatusChangeCommand)
                    _StageConnectStatusChangeCommand = new AsyncCommand<GPCellModeEnum>(StageConnectStatusChangeCommandFunc);
                return _StageConnectStatusChangeCommand;
            }
        }

        private async Task StageConnectStatusChangeCommandFunc(GPCellModeEnum param)
        {
            ObservableCollection<IStageObject> filteredCollection = new ObservableCollection<IStageObject>();
            IStageObject selectStage = null;
            try
            {
                if (SelectedStage != null && SelectedStage.StageInfo != null)
                {
                    selectStage = SelectedStage;
                    //Online을 변경할 때, Inspection화면에 있는 Probe Mark Shift값이 Clear를 안하고 나와서 0이 아닐때 알람 띄운다
                    var client = LoaderMaster.GetClient(selectStage.StageInfo.Index);
                    EnumMessageDialogResult retval = EnumMessageDialogResult.UNDEFIND;
                    CatCoordinates pmShitftValue;

                    if (client != null && param == GPCellModeEnum.ONLINE)//Online눌렀을 때
                    {
                        if (selectStage.StageInfo.IsConnected && this.GEMModule().GemCommManager.GetRemoteConnectState(selectStage.StageInfo.Index))
                        {
                            //Cell의 Chiller Target Temp와 Coolant Temp가 일치하는지 확인. (불일치 시 모드 변경 안됨)
                            string tempState = selectStage.StageInfo.LotData.TempState;
                            if (tempState.Equals(EnumTemperatureState.PauseDiffTemp.ToString()))
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Error Message", "Cannot be switched when chiller target temperature and set temperature do not match.", EnumMessageStyle.Affirmative);
                                return;
                            }
                            LoggerManager.Debug($"[LoaderStageSummaryViewModel] StageConnectStatusChange(). Cell #{selectStage.StageInfo.Index}, TempState : {tempState}.");

                            pmShitftValue = client.GetPMShifhtValue();
                            var info = client.GetStageInfo();
                            if (pmShitftValue.X.Value != 0 || pmShitftValue.Y.Value != 0)
                            {
                                retval = await this.MetroDialogManager().ShowMessageDialog("The probe mark shift value was not cleared.", $"PMShiftValue X: {pmShitftValue.X.Value}, Y: {pmShitftValue.Y.Value}. OverDrive: {info.ProbingOD} \n Do you want to change the mode to Online?", EnumMessageStyle.AffirmativeAndNegative, "OK", "Cancel");

                                if (retval == EnumMessageDialogResult.AFFIRMATIVE)//OK
                                {
                                    //그냥 Online으로 바뀜~
                                }
                                else if (retval == EnumMessageDialogResult.NEGATIVE)//Cancel
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                if (CellListSelectionMode == SelectionMode.Single)
                {
                    if (selectStage != null && selectStage.StageInfo != null && selectStage.StageInfo.IsConnected == true
                        && this.GEMModule().GemCommManager.GetRemoteConnectState(selectStage.StageInfo.Index))
                    {
                        LoaderCommunicationManager.SetCellModeChanging(selectStage.StageInfo.Index);
                        // 온라인 && 오프라인 모드의 경우, RUNNING | UNDEFINED가 아니어야 한다.
                        // 메인터넌스 모드의 경우, IDLE | PAUSED | ERROR여야 한다.

                        bool CanChanged = false;
                        bool statussoak_toglgle = false;
                        if (param == GPCellModeEnum.ONLINE || param == GPCellModeEnum.OFFLINE)
                        {

                            if (selectStage.StageInfo.LotData.SoakingState == ModuleStateEnum.RUNNING.ToString()
                                || selectStage.StageInfo.LotData.SoakingState == ModuleStateEnum.SUSPENDED.ToString())
                            {
                                CanChanged = false;
                                await this.MetroDialogManager().ShowMessageDialog(
                                               "Error Message",
                                               "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                                LoggerManager.SoakingLog($"StageConnectStatusChangeCommandFunc: Cannot be switched during Manual Soaking status.");
                            }
                            else if ((selectStage as StageObject).ManualZUPState == ManualZUPStateEnum.Z_UP)
                            {
                                CanChanged = false;
                                await this.MetroDialogManager().ShowMessageDialog("Can not Change CellMode", "Cell is Manual ZUP Mode.", EnumMessageStyle.Affirmative);
                            }
                            else if (selectStage.StageState != ModuleStateEnum.RUNNING && selectStage.StageState != ModuleStateEnum.UNDEFINED)
                            {
                                CanChanged = true;
                                if (selectStage.isTransferError)
                                {
                                    CanChanged = false;
                                    await this.MetroDialogManager().ShowMessageDialog("Can not Change CellMode", "Transfer Error must be cleared first.", EnumMessageStyle.Affirmative);
                                }
                                //this.LotOPModule().InitLotScreen();
                            }

                        }
                        else if (param == GPCellModeEnum.MAINTENANCE)
                        {

                            //기존 soaking 사용일 때에는 Maintenance로 갈때 Soaking State가 Running이면 바뀌지 않는다
                            {
                                if (selectStage.StageState == ModuleStateEnum.IDLE ||
                                    selectStage.StageState == ModuleStateEnum.PAUSED ||
                                    selectStage.StageState == ModuleStateEnum.PAUSING ||
                                    selectStage.StageState == ModuleStateEnum.ERROR)
                                {
                                    if (LoaderMaster.ModuleState.State == ModuleStateEnum.RUNNING || LoaderMaster.ModuleState.State == ModuleStateEnum.ABORT)
                                    {
                                        if (LoaderMaster.Loader.LoaderJobViewList != null && LoaderMaster.Loader.LoaderJobViewList.Count > 0 && LoaderMaster.Loader.LoaderJobViewList.Count(i => i.JobDone == false &&
                                               ((i.CurrentHolder.ModuleType == ModuleTypeEnum.CHUCK && i.CurrentHolder.Index == selectStage.Index) || (i.DstHolder.ModuleType == ModuleTypeEnum.CHUCK && i.CurrentHolder.Index == selectStage.Index))) > 0)
                                        {
                                            await this.MetroDialogManager().ShowMessageDialog("Not Change  Maintenance mode", $"Loader has work remaining in Cell{selectStage.Index}.\r\nPlease try again later.", EnumMessageStyle.Affirmative);
                                            return;
                                        }

                                    }
                                    statussoak_toglgle = LoaderMaster.SoakingModule().GetShowStatusSoakingSettingPageToggleValue();

                                    if (statussoak_toglgle) //status soaking사용일 때에는 Maintenance로 가면 동작 중이던 soaking을 정리해주므로 maintenance로 진입가능
                                    {
                                        if (selectStage.StageMode == GPCellModeEnum.MAINTENANCE)//현재 Cell Mode가 Maintenance인데 Maintenannce를 눌렀을 때 Soaking Data가 Reset되는 것을 막기 위한 것.
                                        {
                                            CanChanged = false;
                                        }
                                        else
                                        {
                                            bool pinAlign = false;
                                            bool waferAlign = false;
                                            LoaderMaster.IsAlignDoing(ref pinAlign, ref waferAlign);
                                            string DoingModuleName = "Pin";
                                            if (waferAlign)
                                            {
                                                DoingModuleName = "Wafer";
                                            }

                                            if (pinAlign || waferAlign)
                                            {
                                                await this.MetroDialogManager().ShowMessageDialog("Not Change  Maintenance mode", $"{DoingModuleName} Alignment is running.\r\nPlease try again later.", EnumMessageStyle.Affirmative);
                                            }
                                            else
                                            {
                                                CanChanged = true;
                                            }
                                        }
                                    }
                                    else // 기존 동작 방식 처리
                                    {
                                        LoaderMaster.StageSoakingMode();
                                        if (LoaderMaster.StageSetSoakingState != ModuleStateEnum.RUNNING)
                                        {
                                            CanChanged = true;
                                        }
                                        else
                                        {
                                            CanChanged = false;
                                            param = GPCellModeEnum.ONLINE;
                                            await this.MetroDialogManager().ShowMessageDialog("Can not to Change  Maintenance mode", "Soaking state is running.", EnumMessageStyle.Affirmative);
                                        }
                                    }
                                }
                                else
                                {
                                    LoggerManager.Error($"Select Stage's stage is Undefined.");
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Error("Unknown Type");
                        }

                        if (CanChanged == true)
                        {
                            LoggerManager.Debug($"[LoaderStageSummaryViewModel], StageConnectStatusChangeCommandFunc() : Cell's Index = {selectStage.StageInfo.Index}, Before Mode : {selectStage.StageMode}, After Mode : {param}");
                            LoggerManager.ActionLog(ModuleLogType.MODE_CHANGE, StateLogType.SET, $"Cell's Index = {selectStage.StageInfo.Index}, Before Mode : {selectStage.StageMode}, After Mode : {param}", index: selectStage.StageInfo.Index);



                            bool CanIChangeMaintenance = true;
                            if (GPCellModeEnum.MAINTENANCE == param)
                                CanIChangeMaintenance = LoaderCommunicationManager.Can_I_ChangeMaintenanceModeInStatusSoaking(selectStage.StageInfo.Index);

                            if (CanIChangeMaintenance)
                            {
                                selectStage.StageInfo.LotData = LoaderMaster.GetStageLotData(selectStage.StageInfo.Index);
                                StageMoveState = selectStage.StageInfo.LotData.StageMoveState;
                                bool ret = await LoaderCommunicationManager.SetStageMode(param, selectStage.StreamingMode);
                                if (ret)
                                {
                                    selectStage.StageMode = param;
                                    selectStage.IsStageModeChanged = true;
                                }
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Not Change  Maintenance mode", $"Soaking Module is busy. Please try again later.", EnumMessageStyle.Affirmative);
                            }

                        }
                        LoaderCommunicationManager.ResetCellModeChanging(selectStage.StageInfo.Index);
                        selectStage = null; //finally에서 불필요한 동작을 방지하기 위함
                    }
                    else
                    {
                        if (selectStage == null)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error Message", "Not exist selected cell information. Please select the cell you want.", EnumMessageStyle.Affirmative);
                        }
                        else if (selectStage.StageInfo == null)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error Message", "There is not enough information for the selected cell. Please check the information of the cell.", EnumMessageStyle.Affirmative);
                        }
                        else if (selectStage.StageInfo.IsConnected == false)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error Message", "The selected cell is not connected. Please connect first.", EnumMessageStyle.Affirmative);
                        }
                        else if (!this.GEMModule().GemCommManager.GetRemoteConnectState(selectStage.StageInfo.Index))
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error Message", $"The selected cell is disconnected(GEM). Please Reconnect Cell({selectStage.StageInfo.Index})", EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            LoggerManager.Error($"StageConnectStatusChangeCommandFunc() : Unknown case.");
                        }
                    }
                    UpdateChanged();
                }
                else if (CellListSelectionMode == SelectionMode.Multiple)
                {
                    var stages = LoaderCommunicationManager.GetStages();
                    foreach (var item in stages.Where(x => x.StageInfo.IsChecked))
                    {
                        filteredCollection.Add(item);
                    }
                    if (filteredCollection.Count > 0)
                    {
                        foreach (var stage in filteredCollection)
                        {
                            LoaderCommunicationManager.SetCellModeChanging(stage.StageInfo.Index);
                            if (stage != null && stage.StageInfo != null && stage.StageInfo.IsConnected == true)
                            {
                                // 온라인 && 오프라인 모드의 경우, RUNNING | UNDEFINED가 아니어야 한다.
                                // 메인터넌스 모드의 경우, IDLE | PAUSED | ERROR여야 한다.

                                bool CanChanged = false;
                                bool statussoak_toglgle = false;

                                if (param == GPCellModeEnum.ONLINE || param == GPCellModeEnum.OFFLINE)
                                {
                                    if (stage.StageInfo.LotData.SoakingState == ModuleStateEnum.RUNNING.ToString()
                                        || stage.StageInfo.LotData.SoakingState == ModuleStateEnum.SUSPENDED.ToString())
                                    {
                                        CanChanged = false;
                                        await this.MetroDialogManager().ShowMessageDialog(
                                                        "Error Message",
                                                        "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                                        LoggerManager.SoakingLog($"StageConnectStatusChangeCommandFunc: Cannot be switched during Manual Soaking status.");
                                    }
                                    else if (stage.StageState != ModuleStateEnum.RUNNING && stage.StageState != ModuleStateEnum.UNDEFINED)
                                    {
                                        CanChanged = true;
                                        if (stage.isTransferError)
                                        {
                                            CanChanged = false;
                                            await this.MetroDialogManager().ShowMessageDialog("Not Change CellMode", "Transfer Error must be cleared first.", EnumMessageStyle.Affirmative);
                                        }
                                        //this.LotOPModule().InitLotScreen();
                                    }

                                }
                                else if (param == GPCellModeEnum.MAINTENANCE)
                                {

                                    if (stage.StageState == ModuleStateEnum.IDLE ||
                                        stage.StageState == ModuleStateEnum.PAUSED ||
                                        stage.StageState == ModuleStateEnum.PAUSING ||
                                        stage.StageState == ModuleStateEnum.ERROR)
                                    {
                                        statussoak_toglgle = LoaderMaster.SoakingModule().GetShowStatusSoakingSettingPageToggleValue(); //To_do: 선택된 cell에 대한 옵션정보를 가져와야 한다..
                                        if (statussoak_toglgle)  //status soaking사용일 때에는 Maintenance로 가면 동작 중이던 soaking을 정리해주므로 maintenance로 진입가능
                                        {
                                            if (stage.StageMode == GPCellModeEnum.MAINTENANCE)//현재 Cell Mode가 Maintenance인데 Maintenannce를 눌렀을 때 Soaking Data가 Reset되는 것을 막기 위한 것.
                                            {
                                                CanChanged = false;
                                            }
                                            else
                                            {
                                                bool pinAlign = false;
                                                bool waferAlign = false;
                                                LoaderMaster.IsAlignDoing(ref pinAlign, ref waferAlign);
                                                string DoingModuleName = "Pin";
                                                if (waferAlign)
                                                {
                                                    DoingModuleName = "Wafer";
                                                }

                                                if (pinAlign || waferAlign)
                                                {
                                                    await this.MetroDialogManager().ShowMessageDialog("Not Change  Maintenance mode", $"{DoingModuleName} Alignment is running.\r\nPlease try again later.", EnumMessageStyle.Affirmative);
                                                }
                                                else
                                                {
                                                    CanChanged = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LoaderMaster.StageSoakingMode();
                                            if (LoaderMaster.StageSetSoakingState != ModuleStateEnum.RUNNING)
                                            {
                                                CanChanged = true;
                                            }
                                            else
                                            {
                                                CanChanged = false;
                                                param = GPCellModeEnum.ONLINE;
                                                await this.MetroDialogManager().ShowMessageDialog("Not Change  Maintenance mode", "Soaking state is running.", EnumMessageStyle.Affirmative);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Error($"Select Stage's stage is Undefined.");
                                    }

                                }
                                else
                                {
                                    LoggerManager.Error("Unknown Type");
                                }

                                if (CanChanged == true)
                                {
                                    LoggerManager.Debug($"[LoaderStageSummaryViewModel], StageConnectStatusChangeCommandFunc() : Cell's Index = {stage.StageInfo.Index}, Before Mode : {stage.StageMode}, After Mode : {param}");

                                    bool CanIChangeMaintenance = true;
                                    if (GPCellModeEnum.MAINTENANCE == param)
                                        CanIChangeMaintenance = LoaderCommunicationManager.Can_I_ChangeMaintenanceModeInStatusSoaking(stage.StageInfo.Index);

                                    if (CanIChangeMaintenance)
                                    {
                                        stage.StageInfo.LotData = LoaderMaster.GetStageLotData(stage.StageInfo.Index);
                                        StageMoveState = stage.StageInfo.LotData.StageMoveState;
                                        bool ret = await LoaderCommunicationManager.SetStageMode(param, stage.StreamingMode, true, stage.Index);
                                        if (ret)
                                        {
                                            stage.StageMode = param;
                                            stage.IsStageModeChanged = true;
                                        }
                                    }
                                    else
                                    {
                                        await this.MetroDialogManager().ShowMessageDialog("Not Change  Maintenance mode", $"Soaking Module is busy. Please try again later.", EnumMessageStyle.Affirmative);
                                    }
                                }
                            }
                            else
                            {
                                if (stage == null)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "Not exist selected cell information. Please select the cell you want.", EnumMessageStyle.Affirmative);
                                }
                                else if (stage.StageInfo == null)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "There is not enough information for the selected cell. Please check the information of the cell.", EnumMessageStyle.Affirmative);
                                }
                                else if (stage.StageInfo.IsConnected == false)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "The selected cell is not connected. Please connect first.", EnumMessageStyle.Affirmative);
                                }
                                else if (!this.GEMModule().GemCommManager.GetRemoteConnectState(stage.StageInfo.Index))
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Error Message", $"The selected cell is disconnected(GEM). Please Reconnect Cell({stage.StageInfo.Index})", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    LoggerManager.Error($"StageConnectStatusChangeCommandFunc() : Unknown case.");
                                }
                            }
                            UpdateChanged();
                            LoaderCommunicationManager.ResetCellModeChanging(stage.StageInfo.Index);
                        }
                        filteredCollection.Clear(); //finally에서 불필요한 동작을 방지하기 위함
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (CellListSelectionMode == SelectionMode.Single)
                {
                    if (selectStage != null && selectStage.StageInfo != null)
                    {
                        LoaderCommunicationManager.ResetCellModeChanging(selectStage.StageInfo.Index);
                    }
                }
                else if (CellListSelectionMode == SelectionMode.Multiple)
                {
                    if (filteredCollection?.Count > 0)
                    {
                        foreach (var stage in filteredCollection)
                        {
                            LoaderCommunicationManager.ResetCellModeChanging(stage.StageInfo.Index);
                        }
                    }
                }
            }
        }

        private AsyncCommand<StreamingModeEnum> _StageStreamingOnOffCommand;
        public ICommand StageStreamingOnOffCommand
        {
            get
            {
                if (null == _StageStreamingOnOffCommand)
                    _StageStreamingOnOffCommand = new AsyncCommand<StreamingModeEnum>(StageStreamingOnOffCommandFunc);
                return _StageStreamingOnOffCommand;
            }
        }

        private async Task StageStreamingOnOffCommandFunc(StreamingModeEnum streamingmode)
        {
            try
            {
                if (SelectedStage != null && SelectedStage.StageInfo != null && SelectedStage.StageInfo.IsConnected == true &&
                    this.GEMModule().GemCommManager.GetRemoteConnectState(SelectedStage.StageInfo.Index))
                {
                    if (SelectedStage.StageMode == GPCellModeEnum.ONLINE)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], StageStreamingOnOffCommandFunc() : {streamingmode}");

                        if (streamingmode == StreamingModeEnum.STREAMING_ON)
                        {
                            if (this.DisplayPort == null)
                            {
                                this.DisplayPort = new DisplayPort();
                                DisplayPort.EnalbeClickToMove = false;
                            }

                            LotOPModule.ViewTarget = this.DisplayPort;
                        }
                        await LoaderCommunicationManager.SetStageMode(SelectedStage.StageMode, streamingmode);
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Error Message", "This button is only operation when in online mode", EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public void CellLotBtnAllDisable()
        //{
        //    CellLotEndFlag = false;
        //    CellLotStartFlag = false;
        //    CellLotResumeFlag = false;
        //    CellLotPauseFlag = false;
        //}
        private AsyncCommand<object> _CellLotStartCommand;
        public ICommand CellLotStartCommand
        {
            get
            {
                if (null == _CellLotStartCommand)
                    _CellLotStartCommand = new AsyncCommand<object>(CellLotStartCommandFunc);
                return _CellLotStartCommand;
            }
        }

        private async Task CellLotStartCommandFunc(object param)
        {
            try
            {
                if (SelectedStage != null && SelectedStage.StageInfo != null)
                {
                    if (SelectedStage.LockMode == StageLockMode.UNLOCK)
                    {
                        var client = LoaderMaster.GetClient(SelectedStage.StageInfo.Index);
                        if (client != null)
                        {
                            var retVal = this.MetroDialogManager().ShowMessageDialog("Cell LOT Start", $"Cell Number: {SelectedStage.StageInfo.Index}\n Do you Want LOT Start?", EnumMessageStyle.AffirmativeAndNegative).Result;

                            if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                (string lotid, int foupnumber, string csHashCode) = CheckLotID(SelectedStage.StageInfo.Index);
                                LoaderMaster.LotOPStartClient(client, true, lotid, foupnumber, csHashCode);
                                // CellLotBtnAllDisable();
                            }
                        }
                        else
                        {
                            LoaderMaster.GetGPLoader().LoaderBuzzer(true);
                            this.MetroDialogManager().ShowMessageDialog("Cell LOT Start Failed", $"Cell{SelectedStage.StageInfo.Index}, Please check the communication again.", EnumMessageStyle.Affirmative);
                        }

                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private (string, int, string) CheckLotID(int stageidx)
        {
            string lotid = "";
            int foupno = -1;
            string cstHashCode = "";
            try
            {
                if (LoaderMaster != null)
                {
                    var runningFoups = LoaderMaster.ActiveLotInfos.FindAll(infos => infos.State == LotStateEnum.Running);
                    if (runningFoups != null)
                    {
                        if (runningFoups.Count != 0)
                        {
                            var foups = runningFoups.OrderBy(foup => foup.LotPriority);
                            foupno = foups.FirstOrDefault().FoupNumber;
                            lotid = foups.FirstOrDefault().LotID;
                            cstHashCode = foups.FirstOrDefault().CST_HashCode;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return (lotid ,foupno, cstHashCode);
        }

        private AsyncCommand<object> _CellLotPauseCommand;
        public ICommand CellLotPauseCommand
        {
            get
            {
                if (null == _CellLotPauseCommand)
                    _CellLotPauseCommand = new AsyncCommand<object>(CellLotPauseCommandFunc);
                return _CellLotPauseCommand;
            }
        }

        private async Task CellLotPauseCommandFunc(object param)
        {
            try
            {
                if (SelectedStage != null && SelectedStage.StageInfo != null)
                {
                    var client = LoaderMaster.GetClient(SelectedStage.StageInfo.Index);
                    if (client != null)
                    {
                        var retVal = await this.MetroDialogManager().ShowMessageDialog("Cell LOT Pause", $"Cell Number: {SelectedStage.StageInfo.Index}\n Do you Want LOT Pause?",
                            EnumMessageStyle.AffirmativeAndNegativeAndSingleAuxiliary, "OK", "CANCLE", "ABORT");

                        if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            LoaderMaster.LotOPPauseClient(client, false, SelectedStage);
                            //  CellLotBtnAllDisable();
                        }
                        else if (retVal == EnumMessageDialogResult.FirstAuxiliary)
                        {
                            retVal = await this.MetroDialogManager().ShowMessageDialog("Warning Message", $"Cell Number: {SelectedStage.StageInfo.Index}\n Applies even if  waiting for a remote control. Do you want to continue anyway?",
                            EnumMessageStyle.AffirmativeAndNegative);
                            if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                LoaderMaster.LotOPPauseClient(client, true);
                            }

                        }
                        else if (retVal == EnumMessageDialogResult.NEGATIVE)
                        {
                            if (SelectedStage.StageInfo.IsReservePause == true)
                            {
                                LoaderMaster.GetClient(SelectedStage.StageInfo.Index)?.CancelCellReservePause();
                            }
                        }
                    }
                    else
                    {

                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _CellLotResumeCommand;
        public ICommand CellLotResumeCommand
        {
            get
            {
                if (null == _CellLotResumeCommand)
                    _CellLotResumeCommand = new AsyncCommand<object>(CellLotResumeCommandFunc);
                return _CellLotResumeCommand;
            }
        }

        private async Task CellLotResumeCommandFunc(object param)
        {
            try
            {
                EnumMessageDialogResult dialogRet = EnumMessageDialogResult.UNDEFIND;

                if (SelectedStage != null && SelectedStage.StageInfo != null)
                {
                    if (SelectedStage.LockMode != StageLockMode.UNLOCK)
                    {
                        LoaderMaster.GetGPLoader().LoaderBuzzer(true);
                        this.MetroDialogManager().ShowMessageDialog("Cell LOT Resume Failed", $"Cell{SelectedStage.StageInfo.Index} BackSide Door is Opened.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        if (SelectedStage.IsRecoveryMode == false)
                        {
                            var client = LoaderMaster.GetClient(SelectedStage.StageInfo.Index);
                            if (client != null)
                            {
                                var clientErrorState = client.GetErrorEndState();
                                if (clientErrorState == ErrorEndStateEnum.Reserve)
                                {
                                    if (client.GetChuckWaferStatus() == EnumSubsStatus.EXIST)
                                    {
                                        dialogRet = await (this).MetroDialogManager().ShowMessageDialog(
                                            "Cell LOT Resume", $"Cell Number: {SelectedStage.StageInfo.Index}\n" +
                                            $" The cell is scheduled to cancel the test. If you resume, the wafer will be unloaded and pause again. \n Do you Want LOT Resume?",
                                            EnumMessageStyle.AffirmativeAndNegative);
                                    }
                                    else if (client.GetChuckWaferStatus() == EnumSubsStatus.NOT_EXIST)
                                    {
                                        dialogRet = await (this).MetroDialogManager().ShowMessageDialog(
                                            "Cell LOT Resume", $"Cell Number: {SelectedStage.StageInfo.Index}\n" +
                                            $" The cell is scheduled to cancel the test. However, the lot resumes normally because there is no wafer.. \n Do you Want LOT Resume?",
                                            EnumMessageStyle.AffirmativeAndNegative);
                                    }
                                }
                                else
                                {
                                    dialogRet = await (this).MetroDialogManager().ShowMessageDialog(
                                        "Cell LOT Resume", $"Cell Number: {SelectedStage.StageInfo.Index}\n Do you Want LOT Resume?",
                                        EnumMessageStyle.AffirmativeAndNegative);
                                }

                                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    if(CheckCellLotInfo(SelectedStage.StageInfo.Index))
                                    {
                                        LoaderMaster.LotOPResumeClient(client);
                                    }
                                    else
                                    {
                                        await (this).MetroDialogManager().ShowMessageDialog(
                                          "Cell LOT Resume Error", $"Selected Stage Index #{SelectedStage.StageInfo.Index}'s device infomation is not correct.",
                                          EnumMessageStyle.Affirmative);
                                    }
                                    
                                    //  CellLotBtnAllDisable();
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"CellLotResumeCommandFunc() : Selected Stage Index #{SelectedStage.StageInfo.Index}'s client is null");
                            }
                        }
                        else
                        {
                            dialogRet = await (this).MetroDialogManager().ShowMessageDialog(
                               "Cell LOT Resume Error", $"Cell Number: {SelectedStage.StageInfo.Index} is Recovery Mode. Please finish Recovery.",
                               EnumMessageStyle.Affirmative);
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool CheckCellLotInfo(int stageIdx)
        {
            bool retVal = true;
            try
            {
                string stageDev = SelectedStage.StageInfo.LotData.DeviceName;
                string stageLotID = SelectedStage.StageInfo.LotData.LotID;

                var lotInfo = LoaderMaster.ActiveLotInfos.Find(info => string.IsNullOrEmpty(info.LotID) == false && info.LotID.Equals(stageLotID));
                if(lotInfo != null)
                {
                    if(lotInfo.CellDeviceInfoDic != null)
                    {
                        string assignDev = "";
                        bool existCellDev = lotInfo.CellDeviceInfoDic.TryGetValue(stageIdx, out assignDev);
                        if(existCellDev)
                        {
                            if(stageDev.Equals(assignDev) == false)
                            {
                                LoggerManager.Debug($"[Cell Resume] CheckCellLotInfo() verify is fail. " +
                                    $"Stage LOTID : {stageLotID}, DEV NAME : {stageDev}, ASSIGN DEV NAME : {assignDev}");
                                retVal = false;
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

        private AsyncCommand<object> _CellLotEndCommand;
        public ICommand CellLotEndCommand
        {
            get
            {
                if (null == _CellLotEndCommand)
                    _CellLotEndCommand = new AsyncCommand<object>(CellLotEndCommandFunc);
                return _CellLotEndCommand;
            }
        }

        private async Task CellLotEndCommandFunc(object param)
        {
            try
            {
                EnumMessageDialogResult dialogRet = EnumMessageDialogResult.UNDEFIND;
                if (SelectedStage != null && SelectedStage.StageInfo != null)
                {
                    if (SelectedStage.LockMode != StageLockMode.UNLOCK)
                    {
                        LoaderMaster.GetGPLoader().LoaderBuzzer(true);
                        this.MetroDialogManager().ShowMessageDialog("Cell LOT End Failed", $"Cell{SelectedStage.StageInfo.Index} BackSide Door is Opened.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        if (SelectedStage.IsRecoveryMode == false)
                        {
                            var client = LoaderMaster.GetClient(SelectedStage.StageInfo.Index);
                            if (client != null)
                            {
                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Cell LOT End", $"Cell Number: {SelectedStage.StageInfo.Index}\n Do you Want LOT End?", EnumMessageStyle.AffirmativeAndNegative, "OK", "Cancel").Result;

                                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    client.CancelLot(-1, true);
                                    client.SetAbort(true, false);
                                    LoaderMaster.LotOPEndClient(client, isabortlot: true);
                                    // CellLotBtnAllDisable();
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"CellLotEndCommandFunc() : Selected Stage Index #{SelectedStage.StageInfo.Index}'s client is null");
                            }
                        }
                        else
                        {

                            dialogRet = await (this).MetroDialogManager().ShowMessageDialog(
                                       "Cell LOT End Error", $"Cell Number: {SelectedStage.StageInfo.Index} is Recovery Mode. Please finish Recovery.",
                                       EnumMessageStyle.Affirmative);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string LotCellDesc(bool Resumecommand = false)
        {
            string desc = "";
            try
            {
                if (this.LoaderMaster.IsSelectedLoader)
                {
                    desc = "Loader Selected,\n";
                }
                else
                {
                    desc = "Loader Unselected,\n";
                }
                bool isCellExist = false;
                bool isAbortedCellExist = false;
                string cellDesc = "Selected Cell Number: ";
                string abortedcellDesc = "Selected Cell( ";

                foreach (var cell in Stages)
                {
                    if (cell.StageInfo != null && cell.StageInfo.IsChecked)
                    {
                        cell.StageInfo.IsExcuteLot = true;
                        isCellExist = true;
                        cellDesc += cell.Index + ",";
                        if (cell.StageInfo.LotData.LotAbortedByUser == true && Resumecommand)
                        {
                            isAbortedCellExist = true;
                            abortedcellDesc += cell.Index + ",";
                        }
                    }
                }
                cellDesc = cellDesc.Remove(cellDesc.Count() - 1, 1);
               
                if (isCellExist)
                {
                    desc += cellDesc + "\n";
                    if (isAbortedCellExist)
                    {
                        abortedcellDesc = abortedcellDesc.Remove(cellDesc.Count() - 1, 1);
                        desc += abortedcellDesc + ") cannot be resumed.\n";
                    }
                }
                else
                {
                    desc += "Cell Unselected,\n";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return desc;
        }

        private AsyncCommand<object> _StartLotOPCommand;
        public ICommand StartLotOPCommand
        {
            get
            {
                if (null == _StartLotOPCommand)
                    _StartLotOPCommand = new AsyncCommand<object>(StartLotOPCommandFunc);
                return _StartLotOPCommand;
            }
        }

        private async Task StartLotOPCommandFunc(object param)
        {
            try
            {
                if (!this.LoaderMaster.IsSelectedLoader)
                {
                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Lot Start Fail", "Loader must be Selected", EnumMessageStyle.Affirmative).Result;
                }
                else
                {
                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Lot Start", LotCellDesc() + "Do you Want LOT Start?", EnumMessageStyle.AffirmativeAndNegative).Result;

                    if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        LoaderMaster.ChangeInternalMode();
                        (this).CommandManager().SetCommand<IGPLotOpStart>(this);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _PauseLotOPCommand;
        public ICommand PauseLotOPCommand
        {
            get
            {
                if (null == _PauseLotOPCommand)
                {
                    _PauseLotOPCommand = new AsyncCommand<object>(PauseLotOPCommandFunc);
                    _PauseLotOPCommand.SetJobTask(WaitLoaderPause);
                }
                return _PauseLotOPCommand;
            }
        }

        private bool PauseLotOPCancelFlag = false;
        private bool EndLotOPCancelFlag = false;

        // Lot Pasue 버튼 수행 시 해당 function에서 module state가 pause되기를 기다림(Lot Running일때만 해당 버튼이 활성화되어 사용자 클릭 가능)
        public async Task WaitLoaderPause()
        {
            try
            {
                //PauseLotOPCancelFlag = false;

                while (LoaderMaster.ModuleState.GetState() != ModuleStateEnum.PAUSED)
                {
                    //_delays.DelayFor(1);

                    if (PauseLotOPCancelFlag == true)
                    {
                        break;
                    }

                    //module state가 done나 idle로 바뀌어 있는 상태에서는 Pause로 갈 수 없음.(Lot running 중 pause를 눌렀으나 타이밍상 이미 Module state가 done으로 변경된 경우 이곳 루프 탈출을 위함)
                    if (LoaderMaster.ModuleState.GetState() == ModuleStateEnum.DONE
                       || LoaderMaster.ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        LoggerManager.Debug($"Escape - waiting lot pause state(module state done or idle), LoaderMaster state:{LoaderMaster.ModuleState.GetState().ToString()}");
                        break;
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task PauseLotOPCommandFunc(object param)
        {
            try
            {
                PauseLotOPCancelFlag = false;

                var retVal = await (this).MetroDialogManager().ShowMessageDialog("Lot Pause", LotCellDesc() + "Do you Want LOT Pause?", EnumMessageStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                    firstAuxiliaryButtonText: "With Cell");

                if (retVal == EnumMessageDialogResult.AFFIRMATIVE || retVal == EnumMessageDialogResult.FirstAuxiliary)
                {
                    if (LoaderMaster.ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        bool lotOpPauseWithCell = retVal == EnumMessageDialogResult.AFFIRMATIVE ? false : true;
                        LoaderMaster.SetMapSlicerLotPause(true);
                        if (LoaderMaster.IsSelectedLoader)
                        {
                            //this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Lot Pause");
                        }

                        this.CommandManager().SetCommand<IGPLotOpPause>(this, new GPLotOpPauseParam() { LotOpPauseWithCell = lotOpPauseWithCell });
                    }
                    else
                    {
                        PauseLotOPCancelFlag = true;
                    }
                }
                else
                {
                    // 명령을 취소했기 때문에, AsyncCommand에서 대기하는 Task를 종료하기 위해 플래그를 활용
                    PauseLotOPCancelFlag = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _ResumeLotOPCommand;
        public ICommand ResumeLotOPCommand
        {
            get
            {
                if (null == _ResumeLotOPCommand)
                    _ResumeLotOPCommand = new AsyncCommand<object>(ResumeLotOPCommandFunc);
                return _ResumeLotOPCommand;
            }
        }

        private async Task ResumeLotOPCommandFunc(object param)
        {
            try
            {
                var retVal = (this).MetroDialogManager().ShowMessageDialog("Lot Resume", LotCellDesc(true) + "Do you Want LOT Resume?", EnumMessageStyle.AffirmativeAndNegative).Result;

                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    (this).CommandManager().SetCommand<IGPLotOpResume>(this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _UnselectedAllCellsCommand;
        public ICommand UnselectedAllCellsCommand
        {
            get
            {
                if (null == _UnselectedAllCellsCommand)
                {
                    _UnselectedAllCellsCommand = new AsyncCommand<object>(UnselectedAllCellsCommandFunc);
                }

                return _UnselectedAllCellsCommand;
            }
        }

        private async Task UnselectedAllCellsCommandFunc(object arg)
        {
            try
            {
                foreach (var cell in Stages)
                {
                    cell.StageInfo.IsChecked = false;
                }

                LoaderCommunicationManager.SelectedStage = null;

                this.SelectedStage = null;

                UpdateChanged();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _EndLotOPCommand;
        public ICommand EndLotOPCommand
        {
            get
            {
                if (null == _EndLotOPCommand)
                {
                    _EndLotOPCommand = new AsyncCommand<object>(EndLotOPCommandFunc);
                    _EndLotOPCommand.SetJobTask(WaitLoaderEnd);
                }

                return _EndLotOPCommand;
            }
        }

        public async Task WaitLoaderEnd()
        {
            try
            {
                //EndLotOPCancelFlag = false;

                bool Condition = true;

                // Done 또는 IDLE 상태가 되면 종료
                while (Condition)
                {
                    if (LoaderMaster.ModuleState.GetState() == ModuleStateEnum.DONE ||
                       LoaderMaster.ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        Condition = false;
                    }

                    Thread.Sleep(1);
                    //_delays.DelayFor(1);

                    if (EndLotOPCancelFlag == true)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task EndLotOPCommandFunc(object param)
        {
            try
            {
                EndLotOPCancelFlag = false;
                bool isAllEnd = true;
                List<int> lotCancelList = new List<int>();
                foreach (var foup in LoaderModule.Foups)
                {
                    if (foup.LotState == LotStateEnum.Running)
                    {
                        if (foup.IsLotEnd)
                        {
                            lotCancelList.Add(foup.Index);

                        }
                        else
                        {
                            isAllEnd = false;
                        }
                    }
                }
                var cellToRecover = this.LoaderCommunicationManager.Cells.Where(c => c.IsRecoveryMode == true);
                if (cellToRecover != null)
                {
                    if (cellToRecover.Count() > 0)
                    {
                        StringBuilder cellsName = new StringBuilder();
                        IStageObject ci;
                        cellsName.Clear();
                        foreach (var cell in cellToRecover)
                        {
                            ci = cell;
                            cellsName.Append($"[{ci.Name}], ");
                        }
                        var msg = cellsName.ToString();
                        msg = msg.Remove(msg.Length - 2, 2);
                        await this.MetroDialogManager().ShowMessageDialog("Can not end the LOT", $"Cell ({msg}) is(are) under recovery mode. Manual operation required due to hadling error.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await RequestEndLOT(isAllEnd, lotCancelList);
                    }
                }
                else
                {
                    await RequestEndLOT(isAllEnd, lotCancelList);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                EndLotOPCancelFlag = true;
            }

            async Task RequestEndLOT(bool isAllEnd, List<int> lotCancelList)
            {
                if ((lotCancelList.Count == 0 || isAllEnd))
                {
                    if (LoaderMaster.ModuleState.GetState() == ModuleStateEnum.PAUSED)
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Lot Abort", "Do you Want All LOT Abort?", EnumMessageStyle.AffirmativeAndNegative).Result;

                        if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            bool isCanClientLotEnd = true;
                            string clientNotEndLIst = string.Empty;
                            // Cell이 IDLE, PAUSED, ABORT 상태가 아니면 LotEnd를 못하게 한다.
                            foreach (var client in LoaderMaster.ClientList)
                            {
                                var stg = client.Value;
                                if (!LoaderMaster.IsAliveClient(stg))
                                {
                                    continue;
                                }

                                var clntState = stg.GetLotState();
                                if (clntState != ModuleStateEnum.IDLE && clntState != ModuleStateEnum.PAUSED && clntState != ModuleStateEnum.ABORT)
                                {
                                    isCanClientLotEnd = false;
                                    clientNotEndLIst += "CELL" + stg.GetChuckID().Index + ',';
                                }
                            }

                            if (clientNotEndLIst.Length != 0)
                            {
                                clientNotEndLIst = clientNotEndLIst.TrimEnd(',');
                            }

                            if (isCanClientLotEnd)
                            {
                                foreach (var lotInfo in LoaderMaster.ActiveLotInfos)
                                {
                                    if (lotInfo.State == LotStateEnum.Running)
                                    {
                                        lotInfo.SetAssignLotState(LotAssignStateEnum.CANCEL);
                                    }
                                }
                                //this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Lot End");
                                (this).CommandManager().SetCommand<IGPLotOpEnd>(this);
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("LOT end Error", $"Lot End cannot be done because there is an operational cell.\r\n" +
                                                                            $"Please change the cell to pause state so that Lot End is possible.\r\n" +
                                                                            $"({clientNotEndLIst})", EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("LOT end Error", $"Lot End cannot be done because Loader State.\r\n" +
                                                                            $"Please change the loader to pause state so that Lot End is possible.\r\n", EnumMessageStyle.Affirmative);
                    }
                }
                else //개별 LOT End
                {
                    if (lotCancelList.Count > 0)
                    {
                        string indexstr = "(Foup: ";
                        foreach (var idx in lotCancelList)
                        {
                            indexstr += idx.ToString() + ",";
                        }
                        indexstr = indexstr.Remove(indexstr.Length - 1, 1);
                        indexstr += ")";
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Lot ABORT", $"Do you Want LOT ABORT {indexstr}?", EnumMessageStyle.AffirmativeAndNegative).Result;
                        if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            foreach (var idx in lotCancelList)
                            {
                                LoaderMaster.ActiveLotInfos[idx - 1].State = LotStateEnum.Cancel;
                                LoaderMaster.ActiveLotInfos[idx - 1].SetAssignLotState(LotAssignStateEnum.CANCEL);
                                LoaderModule.Foups[idx - 1].LotState = LoaderMaster.ActiveLotInfos[idx - 1].State;
                                LoaderMaster.ActiveLotInfos[idx - 1].IsManaulEnd = false;

                                (this).CommandManager().SetCommand<IGPLotOpResume>(this);
                            }
                        }
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("LOT End Error", "Please check the information of the LOT.", EnumMessageStyle.Affirmative);
                    }
                }
            }
        }

        private AsyncCommand<object> _CellListViewPreviewMouseDown;
        public ICommand CellListViewPreviewMouseDown
        {
            get
            {
                if (null == _CellListViewPreviewMouseDown)
                    _CellListViewPreviewMouseDown = new AsyncCommand<object>(CellListViewPreviewMouseDownFunc);
                return _CellListViewPreviewMouseDown;
            }
        }

        private async Task CellListViewPreviewMouseDownFunc(object obj)
        {
            try
            {
                //items = obj as IList;

                //if (items != null)
                //{
                //    // 셀이 한개 선택되어 있는 경우, 

                //    if(SelectedStage != null)
                //    {
                //        if (items.Count == 1)
                //        {
                //            SelectedStage = null;
                //            LoaderCommunicationManager.SelectedStage = null;

                //            ChangedSelectedItemsCount = 0;
                //        }
                //    }
                //    else
                //    {
                //        SelectedItemChangedCommandFunc(obj);
                //    }
                //}

                //ListView list = obj as ListView;

                //if(list != null)
                //{
                //    IStageObject stage = list.SelectedItem as IStageObject;

                //    if(stage != null)
                //    {

                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<object> _SelectedItemChangedCommand;
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (null == _SelectedItemChangedCommand)
                    _SelectedItemChangedCommand = new AsyncCommand<object>(SelectedItemChangedCommandFunc);
                return _SelectedItemChangedCommand;
            }
        }
        private IList items = null;
        private async Task SelectedItemChangedCommandFunc(object obj)
        {
            try
            {
                items = obj as IList;

                if (items != null)
                {
                    int selectedcount = items.Count;

                    ChangedSelectedItemsCount = selectedcount;

                    if (selectedcount > 0)
                    {
                        int LastSelectedIndex = -1;

                        LastSelectedIndex = (items[selectedcount - 1] as IStageObject).Index;

                        LoggerManager.Debug($"[LoaderStageSummaryViewModel], SelectedItemChangedCommandFunc() : Selcted cell's Index = {LastSelectedIndex}");

                        var disableStage = Stages.Where(i => i.StageState != (items[selectedcount - 1] as IStageObject).StageState).ToList();
                        foreach (var stage in disableStage)
                        {
                            stage.IsEnableTransfer = true;
                            stage.StageInfo.AlarmEnabled = true;
                        }

                        if (LoaderCommunicationManager.SelectedStage != null && SelectedStage != null)
                        {
                            //brett// 이전 연결된 cell의 불필요한 proxy정보를 제거 한다.
                            if (this.GEMModule().GemCommManager.GetRemoteConnectState(LoaderCommunicationManager.SelectedStage.Index))
                            {
                                LoaderCommunicationManager.DisconnectProxyOnline(LoaderCommunicationManager.SelectedStage.Index);
                            }
                        }
                        // 마지막 선택된 Index의 Data를 보여준다.
                        if (LastSelectedIndex != -1)
                        {
                            SelectedStage = Stages.FirstOrDefault(x => x.Index == LastSelectedIndex);

                            if (SelectedStage != null)
                            {
                                //brett// GEM 연결이 끊어졌다는 것은 해당 Cell이 Down되었다고 판단한다.
                                if (SelectedStage.StageInfo.IsConnected && !this.GEMModule().GemCommManager.GetRemoteConnectState(SelectedStage.Index))
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Error Message", $"The selected cell is disconnected(GEM). Please Reconnect Cell({SelectedStage.Index}).", EnumMessageStyle.Affirmative);
                                }

                                if (SelectedStage.StageMode == GPCellModeEnum.MAINTENANCE)
                                {
                                    if (this.GEMModule().GemCommManager.GetRemoteConnectState(SelectedStage.Index))
                                    {
                                        LoaderCommunicationManager.GetProxy<IMotionAxisProxy>(SelectedStage.Index);
                                    }
                                }
                            }

                            LoaderCommunicationManager.SelectedStage = SelectedStage;
                        }

                        if (LoaderCommunicationManager.SelectedStage != null)
                        {
                            //현재 연결되어있는 스테이지가 ONLINE일 경우 online mode에서 불필요하게 연결된 proxy가 있으면 제거한다.
                            if (LoaderCommunicationManager.SelectedStage.StageMode == GPCellModeEnum.ONLINE)
                            {
                                //brett// GEM 연결이 끊어졌다는 것은 해당 Cell이 Down되었다고 판단한다.
                                if (this.GEMModule().GemCommManager.GetRemoteConnectState(LoaderCommunicationManager.SelectedStage.Index))
                                {
                                    LoaderCommunicationManager.DisconnectProxyOnline(LoaderCommunicationManager.SelectedStage.Index);
                                }
                            }
                        }

                        if (this.LoaderCommunicationManager.Cells != null && SelectedStage != null && SelectedStage.Index - 1 >= 0 && LoaderCommunicationManager.Cells[SelectedStage.Index - 1].StageMode != GPCellModeEnum.ONLINE)
                        {
                            CellLotPauseFlag = false;
                            CellLotResumeFlag = false;
                            CellLotEndFlag = false;
                            CellLotStartFlag = false;
                        }
                        else if ((items[selectedcount - 1] as IStageObject).IsRecoveryMode == true)
                        {
                            CellLotPauseFlag = false;
                            CellLotResumeFlag = false;
                            CellLotEndFlag = false;
                            CellLotStartFlag = false;
                        }
                        else if ((items[selectedcount - 1] as IStageObject).StageState == ModuleStateEnum.RUNNING || (items[selectedcount - 1] as IStageObject).StageState == ModuleStateEnum.ABORT)
                        {
                            CellLotPauseFlag = true;
                            CellLotResumeFlag = false;
                            CellLotEndFlag = false;
                            CellLotStartFlag = false;
                        }
                        else if ((items[selectedcount - 1] as IStageObject).StageState == ModuleStateEnum.PAUSED && SelectedStage.StageInfo.LotData.LotAbortedByUser == true)
                        {
                            CellLotPauseFlag = false;
                            CellLotResumeFlag = false;
                            CellLotEndFlag = true;
                            CellLotStartFlag = false;
                        }
                        else if ((items[selectedcount - 1] as IStageObject).StageState == ModuleStateEnum.PAUSED)
                        {
                            CellLotPauseFlag = false;
                            CellLotResumeFlag = true;
                            CellLotEndFlag = true;
                            CellLotStartFlag = false;
                        }
                        else if ((items[selectedcount - 1] as IStageObject).StageState == ModuleStateEnum.IDLE)
                        {
                            CellLotPauseFlag = false;
                            CellLotResumeFlag = false;
                            CellLotEndFlag = false;
                            CellLotStartFlag = true;
                        }
                        else
                        {
                            CellLotPauseFlag = false;
                            CellLotResumeFlag = false;
                            CellLotEndFlag = false;
                            CellLotStartFlag = false;
                        }

                    }
                    else
                    {
                        SelectedStage = null;
                        LoaderCommunicationManager.SelectedStage = null;

                        foreach (var stage in Stages)
                        {
                            stage.IsEnableTransfer = true;
                        }
                        CellLotPauseFlag = false;
                        CellLotResumeFlag = false;
                        CellLotEndFlag = false;
                        CellLotStartFlag = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _ConnectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if (null == _ConnectCommand)
                    _ConnectCommand = new AsyncCommand<int>(ConnectCommandFunc);
                return _ConnectCommand;
            }
        }

        public async Task LoaderMapConvert(LoaderMap map)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    int index = -1;

                    //if(SelectedStage != null && SelectedStage.LockMode == StageLockMode.LOCK)
                    //{
                    //    SelectedStage = null;
                    //}

                    for (int i = 0; i < map.ChuckModules.Count(); i++)
                    {
                        IStageObject CurStage = Stages.Where(x => x.Index - 1 == i).FirstOrDefault();

                        if (CurStage != null && CurStage.StageInfo.IsConnected)
                        {
                            index = CurStage.Index - 1;

                            if (this.LoaderMaster.StageStates.Count > i)
                            {
                                CurStage.StageState = this.LoaderMaster.StageStates[i];
                                GetCellInfo(CurStage);
                                if (CurStage.StageState == ModuleStateEnum.PAUSED)
                                {
                                    //CurStage.PauseReason = this.LoaderMaster.GetClient(i + 1)?.GetPauseReason() ?? "";
                                }
                            }
                            if (map.ChuckModules[i].Substrate != null)
                            {
                                CurStage.State = StageStateEnum.Requested;
                                CurStage.TargetName = map.ChuckModules[i].Substrate.PrevPos.ToString();
                                CurStage.Progress = map.ChuckModules[i].Substrate.OriginHolder.Index;
                                CurStage.WaferObj = map.ChuckModules[i].Substrate;
                            }
                            else
                            {
                                CurStage.State = StageStateEnum.Not_Request;
                            }

                            CurStage.WaferStatus = map.ChuckModules[i].WaferStatus;

                            if (map.CCModules[i].Substrate != null)
                            {
                                CurStage.CardObj = map.CCModules[i].Substrate;
                                CurStage.CardStatus = map.CCModules[i].WaferStatus;
                                CurStage.Progress = map.CCModules[i].Substrate.OriginHolder.Index;
                            }

                            CurStage.CardStatus = map.CCModules[i].WaferStatus;
                        }
                    }

                    int selectedFoupNum = -1;
                    bool isExternalLotStart = false;

                    if (LoaderMaster.Mode == LoaderMasterMode.External)
                    {
                        isExternalLotStart = true;
                    }

                    for (int i = 0; i < map.CassetteModules.Count(); i++)
                    {                         
                        var temp = this.FoupOpModule().FoupManagerSysParam_IParam as FoupManagerSystemParameter;
                        LoaderModule.Foups[i].CassetteType = temp.FoupModules[i].CassetteType.Value;

                        for (int j = 0; j < map.CassetteModules[i].SlotModules.Count(); j++)
                        {
                            //var slot = new SlotObject(map.CassetteModules[i].SlotModules.Count() - j);
                            if (map.CassetteModules[i].SlotModules[j].Substrate != null)
                            {
                                if (isExternalLotStart && LoaderMaster.ActiveLotInfos[i].State == LotStateEnum.Running) //foupNumber가 같을때
                                {
                                    if (!(LoaderMaster.ActiveLotInfos[i].UsingSlotList.Where(idx => idx == LoaderModule.Foups[i].Slots[j].Index).FirstOrDefault() > 0))
                                    {
                                        if(LoaderModule.Foups[i].Slots[j].WaferState != EnumWaferState.SKIPPED)
                                        {
                                            LoaderModule.Foups[i].Slots[j].WaferState = EnumWaferState.SKIPPED;
                                            var waferObj = LoaderModule.ModuleManager.FindTransferObject(map.CassetteModules[i].SlotModules[j].Substrate.ID.Value);

                                            if(waferObj != null)
                                            {
                                                waferObj.WaferState = EnumWaferState.SKIPPED;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LoaderModule.Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                    }

                                    if (LoaderMaster.ActiveLotInfos[i].UsingPMIList.Where(idx => idx == LoaderModule.Foups[i].Slots[j].Index).FirstOrDefault() > 0)
                                    {
                                        if (LoaderModule.Foups[i].Slots[j].WaferObj != null)
                                        {
                                            //LoaderModule.Foups[i].Slots[j].WaferObj.PMITirgger = ProberInterfaces.Enum.PMITriggerEnum.WAFER_INTERVAL;
                                            LoaderModule.Foups[i].Slots[j].WaferObj.PMITirgger = ProberInterfaces.Enum.PMIRemoteTriggerEnum.UNDIFINED;
                                            LoaderModule.Foups[i].Slots[j].WaferObj.DoPMIFlag = true;
                                        }
                                    }
                                }
                                else
                                {
                                    LoaderModule.Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                }
                            }
                            //foup.Slots.Add(slot);
                        }
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeTabIndex(TabControlEnum TabEnum)
        {
            try
            {
                int val;
                bool isExist = TabControlMappingDictionary.TryGetValue(TabEnum, out val);

                if (isExist == true)
                {
                    if (SelectedTabIndex != val)
                    {
                        SelectedTabIndex = val;
                    }

                    //if (SelectedTabIndex != Convert.ToInt32(TabEnum))
                    //{
                    //    SelectedTabIndex = Convert.ToInt32(TabEnum);
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        private async Task ConnectCommandFunc(int index)
        {
            try
            {
                bool iskeydown = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    iskeydown = Keyboard.IsKeyDown(Key.LeftCtrl);
                });

                if (iskeydown == false)
                {
                    return;
                }

                IStageObject stage = Stages.Where(x => x.Index == index).FirstOrDefault();

                // 연결이 되어 있다면
                if (stage != null && stage.StageInfo.IsConnected == false)
                {
                    var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Connect", $"Do you want to Connect a Cell{index}?", EnumMessageStyle.AffirmativeAndNegative).Result;

                    if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        bool success = await LoaderCommunicationManager.ConnectStage(index);

                        if (success == true)
                        {
                            GetCellInfo(stage);

                            ChangeTabIndex(TabControlEnum.CELL);                            

                            stage.StageInfo.IsChecked = true;
                            LoaderCommunicationManager.SelectedStage = stage;

                            //this.VisionManager().DispHorFlip = this.VisionManager().GetDispHorFlip();
                            //this.VisionManager().DispVerFlip = this.VisionManager().GetDispVerFlip();
                            //this.CoordinateManager().ReverseManualMoveX = this.CoordinateManager().GetReverseManualMoveX();
                            //this.CoordinateManager().ReverseManualMoveY = this.CoordinateManager().GetReverseManualMoveY();

                            // TODO : MotionProxy 사용과 같이 살펴봐야 됨.
                            //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            //{
                            //    OPERA3DView = new OPERA_STAGE_3DView(MotionManager, LoaderCommunicationManager.GetRemoteMediumClient(stage.Index));
                            //}));


                            if (stage.StageInfo.LotData != null)
                            {
                                StageMoveState = stage.StageInfo.LotData.StageMoveState;
                            }
                            else
                            {
                                LoggerManager.Error($"[LoaderStageSummaryViewModel], ConnectCommandFunc() : LotData is null value.");
                            }

                            var statussoak_toglgle = LoaderMaster.SoakingModule().GetShowStatusSoakingSettingPageToggleValue();
                            if (statussoak_toglgle)
                            {
                                //Status soaking 미사용이면 알림을 띄워준다.
                                if (false == LoaderMaster.SoakingModule().GetCurrentStatusSoakingUsingFlag())
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Status Soaking", $"Status Soaking is off.\r\nPlease turn on the Status Soaking.", EnumMessageStyle.Affirmative);
                                }
                            }

                        }
                        //SelectedObj = loaderCommunicationManager.SelectedStage as StageObject;
                        //SelectedStageObj = loaderCommunicationManager.SelectedStage as StageObject;
                        //LotData = LoaderMaster.GetStageLotData(SelectedStageObj.Index);
                    }
                }
                else
                {
                    var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Disconnect", $"Do you want to Disconnect a Cell{index}?", EnumMessageStyle.AffirmativeAndNegative).Result;

                    if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        LoaderCommunicationManager.DisConnectStage(index);

                        // TODO : MotionProxy 사용과 같이 살펴봐야 됨.
                        //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        //    OPERA3DView = new OPERA_STAGE_3DView(null, null);
                        //}));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ConnectCommandFunc(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private AsyncCommand<object> _StreamingCommand;
        public ICommand StreamingCommandUsedBellDrawerOpenCommand
        {
            get
            {
                if (null == _StreamingCommand)
                    _StreamingCommand = new AsyncCommand<object>(StreamingCommandFunc);
                return _StreamingCommand;
            }
        }

        private async Task StreamingCommandFunc(object param)
        {
            try
            {
                if (SelectedStage != null)
                {
                    if (SelectedStage?.StageInfo?.IsConnected == true)
                    {
                        if (SelectedStage.StreamingMode == StreamingModeEnum.STREAMING_OFF)
                        {
                            //Stage Connect  & Streaming On
                            await LoaderCommunicationManager.SetStageMode(SelectedStage.StageMode, StreamingModeEnum.STREAMING_ON);

                        }
                        else if (SelectedStage.StreamingMode == StreamingModeEnum.STREAMING_ON)
                        {
                            //Stage DisConnect  & Streaming Off
                            await LoaderCommunicationManager.SetStageMode(SelectedStage.StageMode, StreamingModeEnum.STREAMING_OFF);
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _UsedBellDrawerOpenCommand;
        public ICommand UsedBellDrawerOpenCommand
        {
            get
            {
                if (null == _UsedBellDrawerOpenCommand)
                    _UsedBellDrawerOpenCommand = new RelayCommand<object>(UsedBellDrawerOpenCmd);
                return _UsedBellDrawerOpenCommand;
            }
        }

        private void UsedBellDrawerOpenCmd(object obj)
        {
            try
            {
                IStageObject SelecgtedStage = obj as IStageObject;
                //if (SelecgtedStage != null && SelecgtedStage.StageInfo != null && SelecgtedStage.StageInfo.IsConnected == true)
                if (SelecgtedStage != null && SelecgtedStage.StageInfo != null)
                {
                    if (SelecgtedStage.StageInfo.ErrorCodeAlarams == null)
                    {
                        SelecgtedStage.StageInfo.ErrorCodeAlarams = new ObservableCollection<AlarmLogData>();
                    }

                    AlarmViewDialogViewModel.Show(SelecgtedStage.StageInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<int> _StageGetErrorInfoCommand;
        public ICommand StageGetErrorInfoCommand
        {
            get
            {
                if (null == _StageGetErrorInfoCommand)
                {
                    _StageGetErrorInfoCommand = new AsyncCommand<int>(StageGetErrorInfoCommandFunc);
                }
                return _StageGetErrorInfoCommand;
            }
        }
        private async Task StageGetErrorInfoCommandFunc(int index)
        {
            try
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(index);
                if (stage != null)
                {
                    var message = stage.GetLotErrorMessage();
                    //await this.MetroDialogManager().ShowMessageDialog("Error Message", message, EnumMessageStyle.Affirmative);

                    await this.MetroDialogManager().ShowMessageDialog($"[LOT PAUSE] in [Cell#{index}]", message, EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _WaferRecoveryInformCammand;
        public ICommand WaferRecoveryInformCammand
        {
            get
            {
                if (null == _WaferRecoveryInformCammand)
                {
                    _WaferRecoveryInformCammand = new AsyncCommand<int>(WaferRecoveryInformCammandFunc);
                }
                return _WaferRecoveryInformCammand;
            }
        }
        private async Task WaferRecoveryInformCammandFunc(int index)
        {
            try
            {
               var LotData= LoaderMaster.GetStageLotData(index);
                if (LotData.CellMode == GPCellModeEnum.MAINTENANCE)
                {
                    if (SelectedStage.Index != index)
                    {
                        List<IStageObject> stgList = new List<IStageObject>();
                        stgList.Add(LoaderCommunicationManager.GetStage(index));
                        SelectedItemChangedCommandFunc(stgList);
                    }

                    if (await this.WaferAligner().CheckPossibleSetup(true))
                    {
                        this.WaferAligner().IsNewSetup = false;
                        this.WaferAligner().SetSetupState();
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                        await this.PnPManager().SettingRemoteRecoveryPNP("WaferAlign", "IWaferAligner", new Guid("D0A33FFE-DD22-4572-5B69-73F66C38CEB4"), true);
                    }
                }
                else
                {
                    var retVal = await this.MetroDialogManager().ShowMessageDialog($"Check Mode in [Cell{index}]",
                    $"Do you want to proceed with recovery after changing to maintenance mode? ", EnumMessageStyle.AffirmativeAndNegative);
                    if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        if (SelectedStage.Index != index)
                        {
                            List<IStageObject> stgList = new List<IStageObject>();
                            stgList.Add(LoaderCommunicationManager.GetStage(index));
                            SelectedItemChangedCommandFunc(stgList);
                        }

                        StageConnectStatusChangeCommandFunc(GPCellModeEnum.MAINTENANCE);
                        if (SelectedStage.StageMode == GPCellModeEnum.MAINTENANCE)
                        {
                            if (await this.WaferAligner().CheckPossibleSetup(true))
                            {
                                this.WaferAligner().IsNewSetup = false;
                                this.WaferAligner().SetSetupState();
                                await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                                await this.PnPManager().SettingRemoteRecoveryPNP("WaferAlign", "IWaferAligner", new Guid("D0A33FFE-DD22-4572-5B69-73F66C38CEB4"), true);
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

        private AsyncCommand<int> _PinRecoveryInformCammand;
        public ICommand PinRecoveryInformCammand
        {
            get
            {
                if (null == _PinRecoveryInformCammand)
                {
                    _PinRecoveryInformCammand = new AsyncCommand<int>(PinRecoveryInformCammandFunc);
                }
                return _PinRecoveryInformCammand;
            }
        }
        private async Task PinRecoveryInformCammandFunc(int index)
        {
            try
            {
                var LotData = LoaderMaster.GetStageLotData(index);
                if (LotData.CellMode == GPCellModeEnum.MAINTENANCE)
                {
                    if (SelectedStage.Index != index)
                    {
                        List<IStageObject> stgList = new List<IStageObject>();
                        stgList.Add(LoaderCommunicationManager.GetStage(index));
                        SelectedItemChangedCommandFunc(stgList);
                    }
                    AlignRecoveryControlVM.Show(_Container, LotData, index);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog($"[PinAlign Recovery]",
                    $"Please try after changing to maintenance mode. Cell{index}", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _ReservePauseIconClickCommand;
        public ICommand ReservePauseIconClickCommand
        {
            get
            {
                if (null == _ReservePauseIconClickCommand)
                {
                    _ReservePauseIconClickCommand = new AsyncCommand(ReservePauseIconClickCommandFunc);
                }
                return _ReservePauseIconClickCommand;
            }
        }
        private async Task ReservePauseIconClickCommandFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowMessageDialog($"Information Message",
                      "After the current process, it will be to pause. \r\n  Press cancel button from the cell pause button to cancel the reserved pause when cell z_up state, .", EnumMessageStyle.Affirmative);

                bool checkKeyDown = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    checkKeyDown = Keyboard.IsKeyDown(Key.LeftCtrl);
                });
                if (checkKeyDown == true)
                {
                    if (SelectedStage.StageInfo.IsReservePause == true)
                    {
                        SelectedStage.StageInfo.IsReservePause = false;
                        LoaderMaster.GetClient(SelectedStage.StageInfo.Index)?.CancelCellReservePause();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private AsyncCommand<int> _StopBeforeProbingCommand;
        public ICommand StopBeforeProbingCommand
        {
            get
            {
                if (null == _StopBeforeProbingCommand)
                    _StopBeforeProbingCommand = new AsyncCommand<int>(StopBeforeProbingCommandFunc);
                return _StopBeforeProbingCommand;
            }
        }
        public async Task StopBeforeProbingCommandFunc(int index)
        {
            try
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(index);
                if (stage != null)
                {
                    stage.StopBeforeProbingCmd(Stages[index - 1].StopBeforeProbing);
                    LoggerManager.Error($"StopBeforeProbing Command Click. StageIndex : {index}, Turn on : {Stages[index - 1].StopBeforeProbing}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _StopAfterProbingCommand;
        public ICommand StopAfterProbingCommand
        {
            get
            {
                if (null == _StopAfterProbingCommand)
                    _StopAfterProbingCommand = new AsyncCommand<int>(StopAfterProbingCommandFunc);
                return _StopAfterProbingCommand;
            }
        }
        public async Task StopAfterProbingCommandFunc(int index)
        {
            try
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(index);
                if (stage != null)
                {
                    stage.StopAfterProbingCmd(Stages[index - 1].StopAfterProbing);
                    LoggerManager.Error($"StopAfterProbing Command Click. StageIndex : {index}, Turn on : {Stages[index - 1].StopAfterProbing}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _OnceStopBeforeProbingCommand;
        public ICommand OnceStopBeforeProbingCommand
        {
            get
            {
                if (null == _OnceStopBeforeProbingCommand)
                    _OnceStopBeforeProbingCommand = new AsyncCommand<int>(OnceStopBeforeProbingCommandFunc);
                return _OnceStopBeforeProbingCommand;
            }
        }
        public async Task OnceStopBeforeProbingCommandFunc(int index)
        {
            try
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(index);
                if (stage != null)
                {
                    stage.OnceStopBeforeProbingCmd(Stages[index - 1].OnceStopBeforeProbing);
                    LoggerManager.Error($"OnceStopBeforeProbing Command Click. StageIndex : {index}, Turn on : {Stages[index - 1].OnceStopBeforeProbing}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _OnceStopAfterProbingCommand;
        public ICommand OnceStopAfterProbingCommand
        {
            get
            {
                if (null == _OnceStopAfterProbingCommand)
                    _OnceStopAfterProbingCommand = new AsyncCommand<int>(OnceStopAfterProbingCommandFunc);
                return _OnceStopAfterProbingCommand;
            }
        }
        public async Task OnceStopAfterProbingCommandFunc(int index)
        {
            try
            {
                var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(index);
                if (stage != null)
                {
                    stage.OnceStopAfterProbingCmd(Stages[index - 1].OnceStopAfterProbing);
                    LoggerManager.Error($"OnceStopAfterProbing Command Click. StageIndex : {index}, Turn on : {Stages[index - 1].OnceStopAfterProbing}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _OCRModeSelectCommand;
        public ICommand OCRModeSelectCommand
        {
            get
            {
                if (null == _OCRModeSelectCommand) _OCRModeSelectCommand = new AsyncCommand<object>(OCRModeSelectCommandFunc);
                return _OCRModeSelectCommand;
            }
        }

        private async Task OCRModeSelectCommandFunc(object param)
        {
            try
            {
                LoaderModule.OCRConfig.Mode = OCRMode;
                LoaderModule.SaveLoaderOCRConfig();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand<object> _NormalDyanmicModeCommand;
        public ICommand NormalDyanmicModeCommand
        {
            get
            {
                if (null == _NormalDyanmicModeCommand) _NormalDyanmicModeCommand = new AsyncCommand<object>(NormalDyanmicModeCommandFunc);
                return _NormalDyanmicModeCommand;
            }
        }

        private async Task NormalDyanmicModeCommandFunc(object param)
        {
            try
            {
                DynamicMode = DynamicModeEnum.NORMAL;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand<object> _DyanmicModeCommand;
        public ICommand DyanmicModeCommand
        {
            get
            {
                if (null == _DyanmicModeCommand) _DyanmicModeCommand = new AsyncCommand<object>(DyanmicModeCommandFunc);
                return _DyanmicModeCommand;
            }
        }

        private async Task DyanmicModeCommandFunc(object param)
        {
            try
            {
                DynamicMode = DynamicModeEnum.DYNAMIC;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand<object> _DynamicModeSelectCommand;
        public ICommand DynamicModeSelectCommand
        {
            get
            {
                if (null == _DynamicModeSelectCommand) _DynamicModeSelectCommand = new AsyncCommand<object>(DynamicModeSelectCommandFunc);
                return _DynamicModeSelectCommand;
            }
        }

        private async Task DynamicModeSelectCommandFunc(object param)
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.NONE;
                string combinedLog = "";
                if (DynamicMode == DynamicModeEnum.DYNAMIC) 
                {
                    retval = LoaderMaster.Loader.ValidateCassetteTypesConsistency(out combinedLog);
                }
                
                if (retval == EventCodeEnum.NONE)
                {
                    LoaderMaster.SetDynamicMode(DynamicMode);
                }
                else 
                {
                    this.MetroDialogManager().ShowMessageDialog("Failed to set Dynamic Mode", $"To enable dynamic mode, both cassette types must be standard.\n{combinedLog}\n ", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _LotReIntroduceCommand;
        public ICommand LotReIntroduceCommand
        {
            get
            {
                if (null == _LotReIntroduceCommand)
                {
                    _LotReIntroduceCommand = new AsyncCommand<object>(LotReIntroduceCommandFunc);
                }
                return _LotReIntroduceCommand;
            }
        }
        private async Task LotReIntroduceCommandFunc(object obj)
        {
            try
            {
                if (SelectedStage != null)
                {
                    var client = this.LoaderMaster.GetClient(SelectedStage.Index);
                    if (client != null)
                    {
                        client.SetErrorEndFalg(false);
                        SelectedStage.StageInfo.IsReceiveErrorEnd = client.GetErrorEndFlag();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _NormalOCRModeCommand;
        public ICommand NormalOCRModeCommand
        {
            get
            {
                if (null == _NormalOCRModeCommand) _NormalOCRModeCommand = new AsyncCommand<object>(NormalOCRModeCommandFunc);
                return _NormalOCRModeCommand;
            }
        }

        private async Task NormalOCRModeCommandFunc(object param)
        {
            try
            {
                OCRMode = OCR_OperateMode.NORMAL;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _DebugOCRModeCommand;
        public ICommand DebugOCRModeCommand
        {
            get
            {
                if (null == _DebugOCRModeCommand) _DebugOCRModeCommand = new AsyncCommand<object>(DebugOCRModeCommandFunc);
                return _DebugOCRModeCommand;
            }
        }

        private async Task DebugOCRModeCommandFunc(object param)
        {
            try
            {
                OCRMode = OCR_OperateMode.DEBUG;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<object> _NG_GO_OCRModeCommand;
        public ICommand NG_GO_OCRModeCommand
        {
            get
            {
                if (null == _NG_GO_OCRModeCommand) _NG_GO_OCRModeCommand = new AsyncCommand<object>(NG_GO_OCRModeCommandFunc);
                return _NG_GO_OCRModeCommand;
            }
        }

        private async Task NG_GO_OCRModeCommandFunc(object param)
        {
            try
            {
                OCRMode = OCR_OperateMode.NG_GO;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _ManualInput_OCRModeCommand;
        public ICommand ManualInput_OCRModeCommand
        {
            get
            {
                if (null == _ManualInput_OCRModeCommand) _ManualInput_OCRModeCommand = new AsyncCommand<object>(ManualInput_OCRModeCommandFunc);
                return _ManualInput_OCRModeCommand;
            }
        }

        private async Task ManualInput_OCRModeCommandFunc(object param)
        {
            try
            {
                OCRMode = OCR_OperateMode.MANUAL_INPUT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _ShowReasonOfLockList;
        public ICommand ShowReasonOfLockList
        {
            get
            {
                if (null == _ShowReasonOfLockList) _ShowReasonOfLockList = new AsyncCommand<int>(ShowReasonOfLockListFunc);
                return _ShowReasonOfLockList;
            }
        }

        private async Task ShowReasonOfLockListFunc(int index)
        {
            try
            {
                List<ReasonOfStageMoveLock> Reasons = GetReasonofLockFromClient(index);
                string RemainReasons = "";
                for (int i = 0; i < Reasons.Count; i++)
                {
                    RemainReasons += $"{i + 1}: " + Reasons[i];
                    RemainReasons += "\n";
                }
                RemainReasons = RemainReasons.Substring(0, RemainReasons.Length - 1);
                this.MetroDialogManager().ShowMessageDialog("Stage Lock", $"There are reasons of lock that remain." +
                    $"\nCount: {Reasons.Count}" +
                    $"\n{RemainReasons}",
                    EnumMessageStyle.Affirmative);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private List<ReasonOfStageMoveLock> GetReasonofLockFromClient(int stageIdx)
        {
            List<ReasonOfStageMoveLock> retval = new List<ReasonOfStageMoveLock>();
            try
            {
                retval = LoaderMaster.GetReasonofLockFromClient(stageIdx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }


        private AsyncCommand<object> _LeftParkingCommand;
        public ICommand LeftParkingCommand
        {
            get
            {
                if (null == _LeftParkingCommand) _LeftParkingCommand = new AsyncCommand<object>(LeftParkingCommandFunc);
                return _LeftParkingCommand;
            }
        }

        private async Task LeftParkingCommandFunc(object param)
        {
            try
            {
                //LOT가 Running이나 로더가 움직이지 않을때
                if (LoaderMaster.ModuleState.State == ModuleStateEnum.RUNNING)
                {
                    LoggerManager.Debug($"[Loader Left Parking Position Error] Cannot move to Left Parking Position. LOT State is Running.");

                    await this.MetroDialogManager().ShowMessageDialog("Loader Parking Error", "Cannot move to Left Parking Position. LOT State is Running.", EnumMessageStyle.Affirmative);
                }
                else if (LoaderModule.ModuleState != ModuleStateEnum.IDLE)
                {
                    LoggerManager.Debug($"[Loader Left Parking Position Error] Cannot move to Left Parking Position. Loader State is {LoaderModule.ModuleState}.");

                    await this.MetroDialogManager().ShowMessageDialog("Loader Parking Error", $"Cannot move to Left Parking Position. Loader State is {LoaderModule.ModuleState}.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    var ret = await this.MetroDialogManager().ShowMessageDialog("Loader Parking", "Do you want to Park Loader to the Left?", EnumMessageStyle.AffirmativeAndNegative);
                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        await Task.Run(() =>
                        {

                            var modules = this.LoaderModule.ModuleManager;
                            int chuckIndex = 1;
                            if (LoaderParkingParam.parkingParam.LeftDownStageIndex >= 1)
                            {
                                chuckIndex = LoaderParkingParam.parkingParam.LeftDownStageIndex;
                            }
                            var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, 1);
                            var chuck = this.LoaderModule.ModuleManager.FindModule<IChuckModule>(ModuleTypeEnum.CHUCK, chuckIndex);
                            try
                            {
                                var retVal = this.LoaderModule.GetLoaderCommands().ChuckMoveLoadingPosition(chuck, arm);

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    //Parking 포지션으로 보내고 해야하는 동작
                                    this.LoaderModule.GetLoaderCommands().LockRobot();
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Error($"[LeftParkingCommandFunc] WaferLoadMove: Exception occurred. Err = {err.Message}");
                            }
                        });
                        LoggerManager.Debug($"[Loader Left Parking Position] Loader placed on the Left Parking Position.");
                        LoaderParkingDialog.ShowDialog("Loader placed on the Left Parking Position.");

                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<object> _RightParkingCommand;
        public ICommand RightParkingCommand
        {
            get
            {
                if (null == _RightParkingCommand) _RightParkingCommand = new AsyncCommand<object>(RightParkingCommandFunc);
                return _RightParkingCommand;
            }
        }

        private async Task RightParkingCommandFunc(object param)
        {
            try
            {
                if (LoaderMaster.ModuleState.State == ModuleStateEnum.RUNNING)
                {
                    LoggerManager.Debug($"[Loader Right Parking Position] Cannot move to Left Parking Position. LOT State is Running.");

                    await this.MetroDialogManager().ShowMessageDialog("Loader Parking Error", "Cannot move to Right Parking Position. LOT State is Running.", EnumMessageStyle.Affirmative);
                }
                else if (LoaderModule.ModuleState != ModuleStateEnum.IDLE)
                {
                    LoggerManager.Debug($"[Loader Right Parking Position] Cannot move to Left Parking Position. Loader State is {LoaderModule.ModuleState}.");

                    await this.MetroDialogManager().ShowMessageDialog("Loader Parking Error", $"Cannot move to Right Parking Position. Loader State is {LoaderModule.ModuleState}.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    var ret = await this.MetroDialogManager().ShowMessageDialog("Loader Parking", "Do you want to Park Loader to the Right?", EnumMessageStyle.AffirmativeAndNegative);
                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        await Task.Run(() =>
                        {

                            var modules = this.LoaderModule.ModuleManager;
                            int chuckIndex = 1;
                            if (LoaderParkingParam.parkingParam.RightDownStageIndex >= 1)
                            {
                                chuckIndex = LoaderParkingParam.parkingParam.RightDownStageIndex;
                            }
                            var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, 1);
                            var chuck = this.LoaderModule.ModuleManager.FindModule<IChuckModule>(ModuleTypeEnum.CHUCK, chuckIndex);
                            try
                            {
                                var retVal = this.LoaderModule.GetLoaderCommands().ChuckMoveLoadingPosition(chuck, arm);

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    //Parking 포지션으로 보내고 해야하는 동작
                                    this.LoaderModule.GetLoaderCommands().LockRobot();
                                }

                            }
                            catch (Exception err)
                            {
                                LoggerManager.Error($"[RightParkingCommandFunc] WaferLoadMove: Exception occurred. Err = {err.Message}");
                            }
                        });
                        LoggerManager.Debug($"[Loader Right Parking Position] Loader placed on the Left Parking Position.");
                        LoaderParkingDialog.ShowDialog("Loader placed on the Right Parking Position.");
                    }
                }


                //await this.MetroDialogManager().ShowMessageDialog("Loader Parking", "Do you want to Park Loader to the Right?", EnumMessageStyle.AffirmativeAndNegative);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _ShowActiveStageViewCommand;
        public ICommand ShowActiveStageViewCommand
        {
            get
            {
                if (null == _ShowActiveStageViewCommand) _ShowActiveStageViewCommand = new AsyncCommand<object>(ShowActiveStageViewCommandFunc);
                return _ShowActiveStageViewCommand;
            }
        }

        private async Task ShowActiveStageViewCommandFunc(object param)
        {
            try
            {
                int foupIndex = (int)param;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window container = new Window();
                    ActiveStageInformationUC activeStageInformationUC = new ActiveStageInformationUC();
                    activeStageInformationUC.DataContext = new ActiveStageInformationUCViewModel(LoaderMaster, container, foupIndex);
                    container.Content = activeStageInformationUC;
                    container.Width = 680;
                    container.Height = 260;
                    container.WindowStyle = WindowStyle.ToolWindow;
                    container.Title = $"Foup #{foupIndex} Active Stage View";
                    container.Topmost = true;
                    container.VerticalAlignment = VerticalAlignment.Center;
                    container.HorizontalAlignment = HorizontalAlignment.Center;
                    container.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    container.Show();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _DryRunWindowOpenCommand;
        public ICommand DryRunWindowOpenCommand
        {
            get
            {
                if (null == _DryRunWindowOpenCommand) _DryRunWindowOpenCommand = new AsyncCommand(DryRunWindowOpenCommandFunc, false);
                return _DryRunWindowOpenCommand;
            }
        }

        private async Task DryRunWindowOpenCommandFunc()
        {
            try
            {   
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    string text = null;
                    text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                    String superPassword = AccountManager.MakeSuperAccountPassword();

                    if (text.ToLower().CompareTo(superPassword) == 0)
                    {
                        DryRunVM.Show(LoaderMaster as LoaderSupervisor);
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _MonitoringCheckCommand;
        public ICommand MonitoringCheckCommand
        {
            get
            {
                if (null == _MonitoringCheckCommand)
                    _MonitoringCheckCommand = new AsyncCommand<object>(MonitoringCheckCommandFunc);
                return _MonitoringCheckCommand;
            }
        }

        public async Task MonitoringCheckCommandFunc(object indexOjbect)
        {
            int behaviorIdx;
            int stageIdx;
            try
            {
                List<int> indexObjectList = indexOjbect as List<int>;
                behaviorIdx = indexObjectList[0];
                stageIdx = indexObjectList[1];

                EventCodeEnum errorCode = Stages[stageIdx].MonitoringBehaviorList[behaviorIdx].ErrorCode;
                string FuncName = Stages[stageIdx].MonitoringBehaviorList[behaviorIdx].Name;
                List<IMonitoringBehavior> behaviorList = Stages[stageIdx].MonitoringBehaviorList;
                IMonitoringBehavior behavior = behaviorList[behaviorIdx];

                string recoveryList = "";

                if (behavior.PreCheckRecoveryBehaviors.Count > 0)
                {
                    foreach (string preBahavior in behavior.PreCheckRecoveryBehaviors)
                    {
                        IMonitoringBehavior beh = behaviorList.Where(x => x.Name == preBahavior).FirstOrDefault() as IMonitoringBehavior;
                        if(beh != null)
                        {
                            if (beh.IsError == true)
                            {
                                recoveryList += beh.Name + ", ";
                            }
                        }
                    }
                }

                if (behavior.CanManualRecovery)
                {
                    string err_msg = "";
                    if(behavior.ErrorDescription == "")
                    {
                        err_msg = $"Error Code : { errorCode} \n\n\n\nPressing the recovery button performs this action\n- {behavior.RecoveryDescription}";
                    }
                    else
                    {
                        err_msg = $"Error Code : { errorCode}\nError Description: {behavior.ErrorDescription}\n\n\nPressing the recovery button performs this action\n- {behavior.RecoveryDescription}";
                    }

                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog($"Monitoring Error({FuncName})", err_msg
                        , EnumMessageStyle.AffirmativeAndNegative, "Recovery", "Cancel");

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        if (string.IsNullOrEmpty(recoveryList) == false)
                        {
                            recoveryList = recoveryList.TrimEnd(new char[] { ',', ' ' });
                            LoggerManager.Debug($"MonitoringCheckCommandFunc() Pre Recoverylist is Exist");
                            this.MetroDialogManager().ShowMessageDialog($"Recovery Fail", $"Please recover these items first ({recoveryList})",EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            LoggerManager.Debug($"MonitoringCheckCommandFunc() Recovery Start");
                            LoaderMaster.ManualRecoveryToStage(stageIdx, behaviorIdx);
                        }
                    }
                    else if (ret == EnumMessageDialogResult.NEGATIVE)
                    {
                        LoggerManager.Debug($"MonitoringCheckCommandFunc() Recovery Cancel");
                    }
                }
                else
                {
                    if(behavior.ErrorDescription == "")
                    {
                        var ret = await this.MetroDialogManager().ShowMessageDialog($"Monitoring Error({FuncName})", $"Error Code : {errorCode} \n" +
                                        $"Manual Recovery : {behavior.RecoveryDescription}", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        var ret = await this.MetroDialogManager().ShowMessageDialog($"Monitoring Error({FuncName})", $"Error Code : {errorCode} \n" +
                                        $"Description: {behavior.ErrorDescription}\n" +
                                        $"Manual Recovery : {behavior.RecoveryDescription}", EnumMessageStyle.Affirmative);
                    }

                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    //public class ManualOperationCommand : INotifyPropertyChanged, IFactoryModule
    //{
    //    #region ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion

    //    private Autofac.IContainer _Container => this.GetLoaderContainer();

    //    public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();

    //    public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();



}

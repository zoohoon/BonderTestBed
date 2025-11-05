using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderBase.FactoryModules.ViewModelModule;
using LoaderBase.LoaderResultMapUpDown;
using LoaderParameters;
using LoaderParameters.Data;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Loader;
using ProberInterfaces.LoaderController;
using RelayCommandBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using StageStateEnum = LoaderBase.Communication.StageStateEnum;
using System.Windows.Input;
using UcDisplayPort;
using AlarmViewDialog;
using System.Windows.Controls;
using System.Threading;
using System.Windows;
using System.IO;
using VirtualKeyboardControl;
using ProberInterfaces.Foup;
using SecsGemServiceInterface;
using System.Diagnostics;
using ProberInterfaces.Enum;
using ProberInterfaces.PMI;
using System.Windows.Media.Animation;
using UcAnimationScrollViewer;
using ProberInterfaces.Utility;
using AccountModule;
using ProberInterfaces.Monitoring;
using System.Windows.Threading;
using System.Text;

namespace LoaderStageSummaryViewModelModule
{
    public class LoaderStageSummaryViewModel_GOP : IMainScreenViewModel, INotifyPropertyChanged, IFactoryModule, ILoaderStageSummaryViewModel_GOP, ILoaderMapConvert
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("3f89a8d0-c550-48a4-ad00-5aea701f7a82");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();

        public ILoaderViewModelManager LoaderViewModelManager => (ILoaderViewModelManager)this.ViewModelManager();
        public ILoaderLogSplitManager LoaderLogSplitmanager => _Container.Resolve<ILoaderLogSplitManager>();
        private ILoaderResultMapUpDownMng LoaderResultMapUpDownMng => _Container.Resolve<ILoaderResultMapUpDownMng>();
        public IDeviceManager DeviceManager => _Container.Resolve<IDeviceManager>();

        private Dictionary<TabControlEnum, int> TabControlMappingDictionary = new Dictionary<TabControlEnum, int>();

        public ILotOPModule LotOPModule => this.LotOPModule();
        public IStageSupervisor StageSupervisor => this.StageSupervisor();

        private bool IsManualTeachpinMode = false;

        private ILoaderModule _loaderModule;
        public ILoaderModule LoaderModule
        {
            get { return _loaderModule; }
            set
            {
                if (value != _loaderModule)
                {

                    _loaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderSupervisor _loaderMaster;
        public ILoaderSupervisor LoaderMaster
        {
            get { return _loaderMaster; }
            set
            {
                if (value != _loaderMaster)
                {

                    _loaderMaster = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DisplayPort _displayPort;
        public DisplayPort DisplayPort
        {
            get { return _displayPort; }
            set
            {
                if (value != _displayPort)
                {
                    _displayPort = value;
                    RaisePropertyChanged();
                }
                if (value == null)
                {
                    WirteLogCallerMethodFormSetDisplayPort();
                }
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

        // TODO : 브랜치 통합 시, LoaderStageSummaryView쪽 같이 수정되어야 함.

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

        private int _selectedLotOptionTabIndex;
        public int SelectedLotOptionTabIndex
        {
            get { return _selectedLotOptionTabIndex; }
            set
            {
                if (value != _selectedLotOptionTabIndex)
                {
                    _selectedLotOptionTabIndex = value;
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

                    // TODO : ?
                    if (_SelectedStage != null)
                    {
                        SelectedCellIndex = _SelectedStage.Index - 1;
                    }

                    RaisePropertyChanged();

                    ChangeTabIndex(TabControlEnum.CELL);

                    //if (SelectedTabIndex != Convert.ToInt32(TabControlEnum.CELL))
                    //{
                    //    SelectedTabIndex = Convert.ToInt32(TabControlEnum.CELL);
                    //}
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

        private ObservableCollection<string> _DeviceNames;
        public ObservableCollection<string> DeviceNames
        {
            get { return _DeviceNames; }
            set
            {
                if (value != _DeviceNames)
                {
                    _DeviceNames = value;
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
        private int _SelectedCellIndex;
        public int SelectedCellIndex
        {
            get { return _SelectedCellIndex; }
            set
            {
                if (value != _SelectedCellIndex)
                {
                    _SelectedCellIndex = value;
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

        private int _ChangedSelectedSettingItemsCount = 0;
        public int ChangedSelectedSettingItemsCount
        {
            get { return _ChangedSelectedSettingItemsCount; }
            set
            {
                _ChangedSelectedSettingItemsCount = value;
                RaisePropertyChanged();
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
        private const double DeafultSettingLength = 775;

        private GPSummaryLayoutModeEnum _SelectedLayoutMode;
        public GPSummaryLayoutModeEnum SelectedLayoutMode
        {
            get { return _SelectedLayoutMode; }
            set
            {
                if (value != _SelectedLayoutMode)
                {
                    _SelectedLayoutMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _settingLayoutLength;
        public double SettingLayoutLength
        {
            get { return _settingLayoutLength; }
            set
            {
                if (value != _settingLayoutLength)
                {
                    _settingLayoutLength = value;
                    RaisePropertyChanged();

                    SummaryLayoutChangeCommandFunc(SelectedLayoutMode);
                }
            }
        }

        private GridLength _SettingGridLength;
        public GridLength SettingGridLength
        {
            get { return _SettingGridLength; }
            set
            {
                if (value != _SettingGridLength)
                {
                    _SettingGridLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GridLength _CellsGridLength;
        public GridLength CellsGridLength
        {
            get { return _CellsGridLength; }
            set
            {
                if (value != _CellsGridLength)
                {
                    _CellsGridLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool PauseLotOPCancelFlag = false;
        private bool EndLotOPCancelFlag = false;

        private Visibility _ExistMultiFoup;
        public Visibility ExistMultiFoup
        {
            get { return _ExistMultiFoup; }
            set
            {
                if (value != _ExistMultiFoup)
                {
                    _ExistMultiFoup = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _strCurrentInputLotName;
        public string strCurrentInputLotName
        {
            get { return _strCurrentInputLotName; }
            set
            {
                if (value != _strCurrentInputLotName)
                {
                    _strCurrentInputLotName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _strCurrentInputOperatorName;
        public string strCurrentInputOperatorName
        {
            get { return _strCurrentInputOperatorName; }
            set
            {
                if (value != _strCurrentInputOperatorName)
                {
                    _strCurrentInputOperatorName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _strCurrentInputDevName;
        public string strCurrentInputDevName
        {
            get { return _strCurrentInputDevName; }
            set
            {
                if (value != _strCurrentInputDevName)
                {
                    _strCurrentInputDevName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LotModeEnum _LotMode;
        public LotModeEnum LotMode
        {
            get { return _LotMode; }
            set
            {
                if (value != _LotMode)
                {
                    _LotMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupObject _SelectedFoupObj;
        public FoupObject SelectedFoupObj
        {
            get { return _SelectedFoupObj; }
            set
            {
                if (value != _SelectedFoupObj)
                {
                    _SelectedFoupObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMIRemoteTriggerEnum _SelectedPMITriggerType = PMIRemoteTriggerEnum.UNDIFINED;
        public PMIRemoteTriggerEnum SelectedPMITriggerType
        {
            get { return _SelectedPMITriggerType; }
            set
            {
                if (value != _SelectedPMITriggerType)
                {
                    _SelectedPMITriggerType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _inputPMICount;
        public int InputPMICount
        {
            get { return _inputPMICount; }
            set
            {
                if (value != _inputPMICount)
                {
                    _inputPMICount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SlotObject _SelectedSlotForLot;
        public SlotObject SelectedSlotForLot
        {
            get { return _SelectedSlotForLot; }
            set
            {
                if (value != _SelectedSlotForLot)
                {
                    _SelectedSlotForLot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool isWaitDownloadRecipe = true;
        private IFoupOpModule Foup => _Container.Resolve<IFoupOpModule>();
        private DownloadStageRecipeActReqData DownloadReqData = new DownloadStageRecipeActReqData();
        private IGEMModule GemModule => _Container.Resolve<IGEMModule>();
        public GEMModule.GEM GemModuleRef => GemModule as GEMModule.GEM;

        private bool _TriggerForStartConfirm;
        public bool TriggerForStartConfirm
        {
            get { return _TriggerForStartConfirm; }
            set
            {
                if (value != _TriggerForStartConfirm)
                {
                    _TriggerForStartConfirm = value;
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

        public void UpdateChanged()
        {
            if (SelectedStage != null && SelectedStage.StageInfo != null && SelectedStage.StageInfo.IsConnected == true)
            {
                GetCellInfo(SelectedStage);
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
            else if (SelectedStage.StageState == ModuleStateEnum.PAUSED && SelectedStage.StageInfo.LotData.LotAbortedByUser == true)
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = true;
                CellLotStartFlag = false;
            }
            else if (SelectedStage.StageState == ModuleStateEnum.PAUSED)
            {
                CellLotPauseFlag = false;
                if (this.LoaderMaster.IsReAssignUnavailableStage(SelectedStage.Index))
                {
                    CellLotResumeFlag = false;//
                }
                else
                {
                    CellLotResumeFlag = true;//
                }
                
                CellLotEndFlag = true;
                CellLotStartFlag = false;
            }
            else if (SelectedStage.StageState == ModuleStateEnum.IDLE && this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKS"))
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = false;
                CellLotStartFlag = false;
            }
            else if (SelectedStage.StageState == ModuleStateEnum.IDLE)
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = false;
                if (this.LoaderMaster.IsReAssignUnavailableStage(SelectedStage.Index))
                {
                    CellLotStartFlag = false;//
                }
                else
                {
                    CellLotStartFlag = true;//
                }
                    
            }
            else
            {
                CellLotPauseFlag = false;
                CellLotResumeFlag = false;
                CellLotEndFlag = false;
                CellLotStartFlag = false;
            }
        }

        public EventCodeEnum InitModule()
        {
            try
            {
                //ResultMapManager = this.ResultMapManager() as ResultMapManager;
                //ResultMapManager = new ResultMapManager();
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
                LoggerManager.Exception(err);
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

                LoaderMaster = _Container.Resolve<ILoaderSupervisor>();
                LoaderModule = _Container.Resolve<ILoaderModule>();

                Stages = LoaderCommunicationManager.Cells;

                foreach (var stage in Stages)
                {
                    stage.StageInfo.SetTitles = new ObservableCollection<string>();
                }

                LoaderModule.SetLoaderMapConvert(this);
                LoaderModule.BroadcastLoaderInfo();

                SelectedTabIndex = 0;
                SelectedOperationTabIndex = 0;
                SelectedLotOptionTabIndex = 0;

                UpdateTimerInterval = 30;

                if (Stages.Count == 12)
                {
                    CellColumn = 4;
                    CellRow = 3;
                }
                else if (Stages.Count == 5)
                {
                    CellColumn = 5;
                    CellRow = 1;

                    //CellColumn_LotSettings = 5;
                    //CellRow_forLotSettings = 1;
                }
                else
                {
                    CellColumn = 3;
                    CellRow = 1;

                    //CellColumn_LotSettings = 4;
                    //CellRow_forLotSettings = 3;
                }

                UpdateDeviceNames();

                CellDirection = CellIndexDirection.TOP_AND_LEFT;
                CellIndexSort();

                GetCellsInfo();

                SelectedFoupObj = LoaderModule.Foups[0];

                if (LoaderModule.Foups.Count > 1)
                {
                    ExistMultiFoup = Visibility.Visible;
                }
                else
                {
                    ExistMultiFoup = Visibility.Hidden;
                }

                SettingGridLength = new GridLength(DeafultSettingLength, GridUnitType.Pixel);
                SettingLayoutLength = DeafultSettingLength;

                CellsGridLength = new GridLength(1, GridUnitType.Star);
                SelectedLayoutMode = GPSummaryLayoutModeEnum.BOTH;

                SummaryLayoutChangeCommandFunc(SelectedLayoutMode);

                MakeTabControlMappingDictionary();

                LotMode = LotModeEnum.CP1;
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
                TabControlMappingDictionary.Add(TabControlEnum.LOTOPTION, 1);
                TabControlMappingDictionary.Add(TabControlEnum.LOTSETTING, 2);
                TabControlMappingDictionary.Add(TabControlEnum.VISION, 3);
                TabControlMappingDictionary.Add(TabControlEnum.MONITORING, 3);
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

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LoaderCommunicationManager.SelectedStage != null)
                {
                    if (IsManualTeachpinMode == true) 
                    {
                        IsManualTeachpinMode = false;
                        LoaderCommunicationManager.SetStageMode(GPCellModeEnum.ONLINE, StreamingModeEnum.STREAMING_OFF);
                    }
                }

                if (this.DisplayPort == null)
                {
                    this.DisplayPort = new DisplayPort();
                    DisplayPort.EnalbeClickToMove = false;

                    LotOPModule.ViewTarget = this.DisplayPort;
                }

                if (SelectedFoupObj.LotState == LotStateEnum.Idle && SelectedFoupObj.ScanState == CassetteScanStateEnum.READ)
                {
                    for (int i = 0; i < LoaderModule.ScanCount; i++)
                    {
                        if (SelectedFoupObj.Slots[i].WaferObj?.CurrHolder.ModuleType != ModuleTypeEnum.SLOT)
                        {
                            SelectedFoupObj.Slots[i].IsPreSelected = false;
                        }
                    }
                }

                Task task = new Task(() =>
                {
                    LoaderModule.SetLoaderMapConvert(this);
                    LoaderModule.BroadcastLoaderInfo();

                    if (IsTimerEnabled == true)
                    {
                        StartUpdateTimer();
                        //LoggerManager.Debug("StageSummaryVM_PageSwitched : StartUpdateTimer() done");
                    }

                    // TODO : 선택된 셀 업데이트 -> GetCellsInfo() (전체 셀 업데이트로 변경)
                    // 문제 있을까?
                    //if (SelectedStage != null && SelectedStage.StageInfo != null && SelectedStage.StageInfo.IsConnected == true)
                    //{
                    //    GetCellInfo(SelectedStage);
                    //    //LoggerManager.Debug("StageSummaryVM_PageSwitched : GetCellInfo() done");
                    //}

                    UpdateDeviceNames();
                    GetCellsInfo();

                    this.VisionManager().SetDisplayChannel(null, DisplayPort);
                });
                task.Start();
                await task;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return eventCodeEnum;
        }

        private void UpdateDeviceNames()
        {
            try
            {
                //for Listview of Devices on Lot settings
                // TODO : Device 추가 된 것 반영 안될 수 있음.
                // 적절한 시점에 업데이트 해줘야 함.
                // 또는 최신 데이터와 연결할 것.
                var devpath = DeviceManager.GetLoaderDevicePath();

                if (Directory.Exists(devpath))
                {
                    string[] dirs = Directory.GetDirectories(devpath);

                    var tmpDeviceNames = new ObservableCollection<string>();                    
                    foreach (var dir in dirs)
                    {
                        tmpDeviceNames.Add(new DirectoryInfo(dir).Name);
                    }
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        DeviceNames = tmpDeviceNames;
                    }));                    
                }
                else
                {
                    Directory.CreateDirectory(devpath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

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
                if (stage.StageInfo != null)
                {
                    stage.StageInfo.LotData = LoaderMaster.GetStageLotData(stage.Index);
                    stage.StageMode = stage.StageInfo.LotData.CellMode;

                    // Tester와 연결 가능한 상태인지?
                    stage.StageInfo.IsAvailableTesterConnect = LoaderMaster.GetTesterAvailableData(stage.Index);
                    // = LoaderMaster.GetTesterAvailableData(stage.Index);

                }
                else
                {
                    // TODO : ?
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                else
                {
                    LoggerManager.Error("ERROR");
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





        private AsyncCommand<ScrollViewer> _CellDataCollectionScrollUpCommand;
        public ICommand CellDataCollectionScrollUpCommand
        {
            get
            {
                if (null == _CellDataCollectionScrollUpCommand)
                    _CellDataCollectionScrollUpCommand = new AsyncCommand<ScrollViewer>(CellDataCollectionScrollUpCommandFunc, false);
                return _CellDataCollectionScrollUpCommand;
            }
        }

        private async Task CellDataCollectionScrollUpCommandFunc(ScrollViewer arg)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {

                    DoubleAnimation verticalAnimation = new DoubleAnimation();

                    //arg.BringIntoView();

                    verticalAnimation.From = arg.VerticalOffset;
                    verticalAnimation.To = arg.VerticalOffset - ((arg.ActualHeight / 3) * 2);
                    verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                    Storyboard storyboard = new Storyboard();
                    storyboard.Children.Add(verticalAnimation);

                    Storyboard.SetTarget(verticalAnimation, arg);
                    Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));

                    storyboard.Begin();

                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<ScrollViewer> _CellDataCollectionScrollDownCommand;
        public ICommand CellDataCollectionScrollDownCommand
        {
            get
            {
                if (null == _CellDataCollectionScrollDownCommand)
                    _CellDataCollectionScrollDownCommand = new AsyncCommand<ScrollViewer>(CellDataCollectionScrollDownCommandFunc, false);
                return _CellDataCollectionScrollDownCommand;
            }
        }

        private async Task CellDataCollectionScrollDownCommandFunc(ScrollViewer arg)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //arg.BringIntoView();

                    DoubleAnimation verticalAnimation = new DoubleAnimation();

                    verticalAnimation.From = arg.VerticalOffset;
                    verticalAnimation.To = arg.VerticalOffset + ((arg.ActualHeight / 3) * 2);
                    verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                    Storyboard storyboard = new Storyboard();
                    storyboard.Children.Add(verticalAnimation);

                    Storyboard.SetTarget(verticalAnimation, arg);
                    Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));
                    storyboard.Begin();
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<GPSummaryLayoutModeEnum> _SummaryLayoutChangeCommand;
        public ICommand SummaryLayoutChangeCommand
        {
            get
            {
                if (null == _SummaryLayoutChangeCommand)
                    _SummaryLayoutChangeCommand = new AsyncCommand<GPSummaryLayoutModeEnum>(SummaryLayoutChangeCommandFunc);
                return _SummaryLayoutChangeCommand;
            }
        }

        private async Task SummaryLayoutChangeCommandFunc(GPSummaryLayoutModeEnum param)
        {
            try
            {
                switch (param)
                {
                    case GPSummaryLayoutModeEnum.SETTING:
                        SettingGridLength = new GridLength(1, GridUnitType.Star);
                        CellsGridLength = new GridLength(0, GridUnitType.Star);

                        SelectedLayoutMode = GPSummaryLayoutModeEnum.SETTING;
                        break;
                    case GPSummaryLayoutModeEnum.CELLS:
                        SettingGridLength = new GridLength(0, GridUnitType.Pixel);
                        CellsGridLength = new GridLength(1, GridUnitType.Star);

                        SelectedLayoutMode = GPSummaryLayoutModeEnum.CELLS;
                        break;
                    case GPSummaryLayoutModeEnum.BOTH:

                        //SettingGridLength = new GridLength(DeafultSettingLength, GridUnitType.Pixel);
                        SettingGridLength = new GridLength(SettingLayoutLength, GridUnitType.Pixel);
                        CellsGridLength = new GridLength(1, GridUnitType.Star);

                        SelectedLayoutMode = GPSummaryLayoutModeEnum.BOTH;
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

                    if (IsTimerEnabled == true && UpdateTimer != null)
                    {
                        TimeLeft = UpdateTimerInterval;
                    }
                }
                else
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Update data", $"The connected cell does not exist.", EnumMessageStyle.Affirmative);
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

                await UnselectedAllCellsCommandFunc(this);
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

                await UnselectedAllCellsCommandFunc(this);
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

        private AsyncCommand<int> _CellCommunicationConnectionCommand;
        public ICommand CellCommunicationConnectionCommand
        {
            get
            {
                if (null == _CellCommunicationConnectionCommand)
                    _CellCommunicationConnectionCommand = new AsyncCommand<int>(CellCommunicationConnectionCommandFunc);
                return _CellCommunicationConnectionCommand;
            }
        }

        private async Task CellCommunicationConnectionCommandFunc(int index)
        {
            try
            {
                var stage = Stages[index];

                if (stage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.RUNNING.ToString()
                     || LoaderCommunicationManager.SelectedStage.StageInfo.LotData.SoakingState == ModuleStateEnum.SUSPENDED.ToString())
                    {
                        await this.MetroDialogManager().ShowMessageDialog(
                                       "Error Message",
                                       "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                        LoggerManager.SoakingLog($"WaferAlignmentExcuteCommandFunc(): Cannot be switched during Manual Soaking status.");
                    }
                    else
                    {
                        Task task = new Task(() =>
                        {
                            IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(index);

                            if (proxy != null)
                            {
                                var retVal = proxy.ReInitializeAndConnect();
                            }
                            else
                            {
                                LoggerManager.Debug($"CellCommunicationConnectionCommandFunc() Proxy is null, Cell Index : {index}");
                            }
                        });
                        task.Start();
                        await task;


                    }
                }     
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<int> _CellTeachPinCommand;
        public ICommand CellTeachPinCommand
        {
            get
            {
                if (null == _CellTeachPinCommand)
                    _CellTeachPinCommand = new AsyncCommand<int>(CellTeachPinCommandFunc);
                return _CellTeachPinCommand;
            }
        }

        private async Task CellTeachPinCommandFunc(int index)
        {
            try
            {
                if (index < 1)
                    return;
                if (LoaderCommunicationManager.Cells[index - 1].LockMode != StageLockMode.UNLOCK)
                    return;
                if (LoaderCommunicationManager.Cells[index - 1].StageMode == GPCellModeEnum.OFFLINE)
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                   "Error Message",
                                   "Cannot be switched during OFFLINE Mode.", EnumMessageStyle.Affirmative);
                    return;
                }

                if (LoaderCommunicationManager.Cells[index - 1].StageInfo.LotData.SoakingState == ModuleStateEnum.RUNNING.ToString()
                        || LoaderCommunicationManager.Cells[index - 1].StageInfo.LotData.SoakingState == ModuleStateEnum.SUSPENDED.ToString())
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                                   "Error Message",
                                   "Cannot be switched during Manual Soaking status.", EnumMessageStyle.Affirmative);
                    LoggerManager.SoakingLog($"WaferAlignmentExcuteCommandFunc(): Cannot be switched during Manual Soaking status.");
                }
                else
                {
                    await SelectedItemChangedFunc(index);

                    bool bForceModeChange = (LoaderCommunicationManager.Cells[index - 1].StageMode == GPCellModeEnum.ONLINE);
                    if (bForceModeChange)
                    {
                        if (LoaderMaster.ModuleState.State == ModuleStateEnum.RUNNING || LoaderMaster.ModuleState.State == ModuleStateEnum.ABORT)
                        {
                            if (LoaderMaster.Loader.LoaderJobViewList != null && LoaderMaster.Loader.LoaderJobViewList.Count > 0 && LoaderMaster.Loader.LoaderJobViewList.Count(i => i.JobDone == false &&
                                   ((i.CurrentHolder.ModuleType == ModuleTypeEnum.CHUCK && i.CurrentHolder.Index == SelectedStage.Index) || (i.DstHolder.ModuleType == ModuleTypeEnum.CHUCK && i.CurrentHolder.Index == SelectedStage.Index))) > 0)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Error Message", $"Loader has work remaining in Cell{SelectedStage.Index}.\r\nPlease try again later.", EnumMessageStyle.Affirmative);
                                return;
                            }
                        }
                        await LoaderCommunicationManager.SetStageMode(GPCellModeEnum.MAINTENANCE, StreamingModeEnum.STREAMING_ON, false, index);
                    }
                    
                    IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(index);
                    var ssc = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(index);
                    if (ssc.DutPadInfosCount() > 1)
                    {
                        EventCodeEnum retVal = ssc.CheckPinPadParameterValidity();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            if (retVal == EventCodeEnum.PTPA_WRONG_DUT_NUMBER)
                            {
                                // 더트 번호가 0이하인 것이 존재함. 즉 더트 등록이 이상함.
                                await this.MetroDialogManager().ShowMessageDialog("Operation Fail",
                                 "DUT data has wrong DUT number. \nPlease register DUT again...", EnumMessageStyle.Affirmative, "OK");
                                return;
                            }
                            else if (retVal == EventCodeEnum.PTPA_WRONG_PAD_NUMBER)
                            {
                                // 패드 번호가 0이하인 것이 존재함. 즉 패드 등록이 이상함.
                                await this.MetroDialogManager().ShowMessageDialog("Operation Fail",
                                 "Pad data has wrong pad number. \nPlease register pad again...", EnumMessageStyle.Affirmative, "OK");
                                return;
                            }
                            else
                            {
                                var mresult = await this.MetroDialogManager().ShowMessageDialog("Warning",
                                    "Pad data is different with pin data, pin data will be initialized from pad data. \n \nPress [ OK ] to continue...", EnumMessageStyle.AffirmativeAndNegative, "OK", "Cancel");

                                if (mresult == EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    //this.StageSupervisor().ProbeCardInfo.GetPinDataFromPads();
                                    proxy.StageSupervisor().GetPinDataFromPads();

                                    //PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterCardChange = false;   // Reg 다시 해야 함
                                }
                                else
                                    return;
                            }
                        }
                        if (SystemManager.SysteMode == SystemModeEnum.Single)
                        {

                        }
                        else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                        {

                            if (proxy != null)
                            {
                                ///Guid viewguid = new Guid();
                                // List<Guid> pnpsteps = new List<Guid>();
                                //Guid cuiparam = new Guid("29d16b55-50af-43bc-847c-c1deb1f22008");
                                if (proxy.TemplateManager() != null & proxy.PinAligner() != null)
                                {
                                    proxy.TemplateManager().CheckTemplate(proxy.PinAligner(), true, 2, index);
                                    //      proxy.PnPManager().GetCuiBtnParam(this.WaferAligner(), cuiparam, out viewguid, out pnpsteps);

                                    await proxy.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                                    await proxy.PnPManager().SettingRemotePNP("PinAlign", "IPinAligner", new Guid("29D16B55-50AF-43BC-847C-C1DEB1F22008"));
                                    
                                    IsManualTeachpinMode = bForceModeChange;
                                }
                            }
                        }
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Operation Fail", "Pad data is not ready yet. \nPlease finish pad registration firstly.", EnumMessageStyle.Affirmative, "OK");
                        if (bForceModeChange)
                        {
                            await StageConnectStatusChangeCommandFunc(GPCellModeEnum.ONLINE);
                        }
                        return;
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
                // TODO : 버튼 입력 시, SelectedStageIndex가 변경된 상태에서 동작하는지에 따라, 로직 수정 필요.
                // 버튼만 눌렀을 때, SelectedStageIndex값은 변하지 않는다.
                // 파라미터로 커맨드를 입력받은 셀의 인덱스를 받아서 사용해야 할 듯.

                int stageindex = (int)param;

                if (LoaderCommunicationManager.Cells[stageindex - 1].LockMode != StageLockMode.UNLOCK)
                {
                    return;
                }

                //Soak Stop 버튼을 눌렀을 때 Soaking Time이 Reset되는 사실을 명시한다.
                //Soaking Cancel을 중단할지? 물어본다. 사용자가 메세지를 읽게 하도록..
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog(
                                           "Warning Message",
                                           "The soaking abortion will abort all soaking process.\n" +
                                           "Please use the Cell End or Maintanence mode to postpond remain soaking steps.\n" +
                                           "Cancel soaking abortion?", EnumMessageStyle.AffirmativeAndNegative, "YES", "NO");
                if (result != EnumMessageDialogResult.AFFIRMATIVE)
                {
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
                else
                {
                    LoggerManager.Debug($"Stop soak command cancel. Does not Stop Soak.");
                }
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
            IStageObject selectStage = null;
            try
            {
                if (SelectedStage != null && SelectedStage.StageInfo != null)
                {
                    selectStage = SelectedStage;
                }

                if (selectStage != null && selectStage.StageInfo != null && selectStage.StageInfo.IsConnected == true
                    && this.GEMModule().GemCommManager.GetRemoteConnectState(selectStage.StageInfo.Index))
                {
                    // 온라인 && 오프라인 모드의 경우, RUNNING | UNDEFINED가 아니어야 한다.
                    // 메인터넌스 모드의 경우, IDLE | PAUSED | ERROR여야 한다.

                    LoaderCommunicationManager.SetCellModeChanging(selectStage.StageInfo.Index);

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
                        else if (selectStage.StageState != ModuleStateEnum.RUNNING && selectStage.StageState != ModuleStateEnum.UNDEFINED)
                        {
                            CanChanged = true;
                            if (selectStage.isTransferError)
                            {
                                CanChanged = false;
                                await this.MetroDialogManager().ShowMessageDialog("Can Not Change CellMode", "Transfer Error must be cleared first.", EnumMessageStyle.Affirmative);
                            }
                            //this.LotOPModule().InitLotScreen();
                        }

                    }
                    else if (param == GPCellModeEnum.MAINTENANCE)
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
                                    await this.MetroDialogManager().ShowMessageDialog("Not Change Maintenance mode", $"Loader has work remaining in Cell{selectStage.Index}.\r\nPlease try again later.", EnumMessageStyle.Affirmative);
                                    return;
                                }

                            }

                            statussoak_toglgle = LoaderMaster.SoakingModule().GetShowStatusSoakingSettingPageToggleValue();
                            if (statussoak_toglgle) //status soaking사용일 때에는 Maintenance로 가면 동작 중이던 soaking을 정리해주므로 maintenance로 진입가능
                            {

                                if (selectStage.StageMode == GPCellModeEnum.MAINTENANCE)//현재 Cell Mode가 Maintenance인데 Maintenannce를 눌렀을 때 Soaking Data가 Reset되는 것을 막기 위한 것
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
                            LoggerManager.Error($"Selecte Stage's stage is undeifned.");
                        }

                    }
                    else
                    {
                        LoggerManager.Error("Unknown Type");
                    }

                    if (CanChanged == true)
                    {
                        LoggerManager.Debug($"[LoaderStageSummaryViewModel], StageConnectStatusChangeCommandFunc() : Cell's Index = {selectStage.StageInfo.Index}, Before Mode : {selectStage.StageMode}, After Mode : {param}");

                        selectStage.StageInfo.LotData = LoaderMaster.GetStageLotData(selectStage.StageInfo.Index);
                        StageMoveState = selectStage.StageInfo.LotData.StageMoveState;

                        bool CanIChangeMaintenance = true;
                        if (GPCellModeEnum.MAINTENANCE == param)
                        {
                            CanIChangeMaintenance = LoaderCommunicationManager.Can_I_ChangeMaintenanceModeInStatusSoaking(selectStage.StageInfo.Index);
                        }

                        if (CanIChangeMaintenance)
                        {
                            bool ret = await LoaderCommunicationManager.SetStageMode(param, selectStage.StreamingMode);

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
                        await this.MetroDialogManager().ShowMessageDialog("Error Message", $"The selected cell is disconnected(GEM). Please Reconnect Cell({selectStage.StageInfo.Index}).", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        LoggerManager.Error($"StageConnectStatusChangeCommandFunc() : Unknown case.");
                    }
                }
                UpdateChanged();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (selectStage != null && selectStage.StageInfo != null)
                {
                    LoaderCommunicationManager.ResetCellModeChanging(selectStage.StageInfo.Index);
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
                    if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKS"))
                    {
                        LoaderMaster.GetGPLoader().LoaderBuzzer(true);
                        this.MetroDialogManager().ShowMessageDialog("Cell LOT Start Failed", "The scenario you're attempting is not currently supported.", EnumMessageStyle.Affirmative);
                    }
                    else
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
            return (lotid, foupno, cstHashCode);
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
                                string text = null;
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                                {
                                    text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                                    String superPassword = AccountManager.MakeSuperAccountPassword();

                                    if (text.ToLower().CompareTo(superPassword) == 0)
                                    {
                                        Task task = new Task(() =>
                                        {
                                            LoaderMaster.LotOPPauseClient(client, true);
                                        });
                                        task.Start();
                                        await task;
                                    }
                                    else
                                    {
                                        await this.MetroDialogManager().ShowMessageDialog("Warning Message", $"Password is incorrect",
                                            EnumMessageStyle.Affirmative, "OK");
                                    }
                                }));

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
                if (SelectedStage != null && SelectedStage.StageInfo != null)
                {
                    if (SelectedStage.LockMode != StageLockMode.UNLOCK)
                    {
                        LoaderMaster.GetGPLoader().LoaderBuzzer(true);
                        this.MetroDialogManager().ShowMessageDialog("Cell LOT Resume Failed", $"Cell{SelectedStage.StageInfo.Index} BackSide Door is Opened.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        var client = LoaderMaster.GetClient(SelectedStage.StageInfo.Index);
                        if (client != null)
                        {
                            EnumMessageDialogResult dialogRet = EnumMessageDialogResult.UNDEFIND;
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
                                Task task = new Task(() =>
                                {
                                    LoaderMaster.LotOPResumeClient(client);
                                });
                                task.Start();
                                await task;
                                //  CellLotBtnAllDisable();
                            }
                        }
                        else
                        {

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
                if (SelectedStage != null && SelectedStage.StageInfo != null)
                {
                    var client = LoaderMaster.GetClient(SelectedStage.StageInfo.Index);

                    if (client != null)
                    {
                        EnumMessageDialogResult retVal = EnumMessageDialogResult.UNDEFIND;

                        if (SelectedStage.StageInfo.LotData != null)
                        {
                            if (SelectedStage.StageInfo.LotData.StageMoveState == "Z_UP")
                            {
                                retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell LOT End", $"Cell Number: {SelectedStage.StageInfo.Index}\n Do you Want LOT End?\n Stage is {SelectedStage.StageInfo.LotData.StageMoveState} state\n Please check whether the Z Down operation is acceptable before proceeding.", EnumMessageStyle.AffirmativeAndNegative, "OK", "Cancel");
                            }
                            else
                            {
                                retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell LOT End", $"Cell Number: {SelectedStage.StageInfo.Index}\n Do you Want LOT End?", EnumMessageStyle.AffirmativeAndNegative, "OK", "Cancel");
                            }
                        }
                        else
                        {
                            retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell LOT End", $"Cell Number: {SelectedStage.StageInfo.Index}\n Do you Want LOT End?", EnumMessageStyle.AffirmativeAndNegative, "OK", "Cancel");
                        }

                        if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            Task task = new Task(() =>
                            {
                                int foupnumber = -1;
                                bool IsParse = int.TryParse(SelectedStage.StageInfo.LotData.FoupNumber, out foupnumber);

                                if (IsParse == true && foupnumber > 0)
                                {
                                    client.CancelLot(foupnumber - 1, true);
                                }
                                else
                                {
                                    client.CancelLot(-1, true);
                                }
                                client.SetAbort(true, false);
                                LoaderMaster.LotOPEndClient(client, foupnumber - 1, true);

                            });
                            task.Start();
                            await task;
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
                    // var retVal = await (this).MetroDialogManager().ShowMessageDialog("Lot Start Fail", "Loader must be Selected", EnumMessageStyle.Affirmative);
                    SelectedTabIndex = 4;
                    //SelectedTabIndex1 = 0;
                }
                else
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Lot Start", LotCellDesc() + "Do you Want LOT Start?", EnumMessageStyle.AffirmativeAndNegative);

                    if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {

                        Task task = new Task(() =>
                        {
                            LoaderMaster.ChangeInternalMode();
                            (this).CommandManager().SetCommand<IGPLotOpStart>(this);
                        });
                        task.Start();
                        await task;
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

        // Lot Pasue 버튼 수행 시 해당 function에서 module state가 pause되기를 기다림(Lot Running일때만 해당 버튼이 활성화되어 사용자 클릭 가능)
        public async Task WaitLoaderPause()
        {
            try
            {
                //PauseLotOPCancelFlag = false;

                while (LoaderMaster.ModuleState.GetState() != ModuleStateEnum.PAUSED)
                {
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
                    firstAuxiliaryButtonText:"With Cell");

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

                        Task task = new Task(() =>
                        {
                            this.CommandManager().SetCommand<IGPLotOpPause>(this, new GPLotOpPauseParam() { LotOpPauseWithCell = lotOpPauseWithCell });
                        });
                        task.Start();
                        await task;
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
                var retVal = await (this).MetroDialogManager().ShowMessageDialog("Lot Resume", LotCellDesc(true) + "Do you Want LOT Resume?", EnumMessageStyle.AffirmativeAndNegative);

                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                {

                    Task task = new Task(() =>
                    {
                        (this).CommandManager().SetCommand<IGPLotOpResume>(this);
                    });
                    task.Start();
                    await task;
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
                this.LoaderMaster.IsLoaderJobDoneWait = true;
                // Done 또는 IDLE 상태가 되면 종료
                while (Condition)
                {
                    if (LoaderMaster.ModuleState.GetState() == ModuleStateEnum.DONE ||
                       LoaderMaster.ModuleState.GetState() == ModuleStateEnum.IDLE||
                       LoaderMaster.IsLoaderJobDoneWait==false)
                    {
                        Condition = false;
                    }


                    if (EndLotOPCancelFlag == true)
                    {
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
                        //messasge 변경 의논 필요!
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
                 if ((lotCancelList.Count == 0 || isAllEnd)) //Lot Abort
                {
                    if(LoaderMaster.ModuleState.GetState() == ModuleStateEnum.PAUSED)
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

        AsyncCommand<SELECT_DIRECTION> _FoupIndexForLotChangeCommand;
        public ICommand FoupIndexForLotChangeCommand
        {
            get
            {
                if (null == _FoupIndexForLotChangeCommand)
                    _FoupIndexForLotChangeCommand = new AsyncCommand<SELECT_DIRECTION>(FoupIndexForLotChangeCommandFunc, false);
                return _FoupIndexForLotChangeCommand;
            }
        }

        private async Task FoupIndexForLotChangeCommandFunc(SELECT_DIRECTION arg)
        {
            try
            {
                if (LoaderModule.Foups != null)
                {
                    int foupTotalcount = LoaderModule.Foups.Count;
                    int selectedFoupIndex = -1;
                    if (SelectedFoupObj != null)
                    {
                        selectedFoupIndex = SelectedFoupObj.Index - 1;
                        if (arg == SELECT_DIRECTION.PREV)
                        {
                            // 마지막 FOUP으로 
                            if (selectedFoupIndex == 0)
                            {
                                selectedFoupIndex = foupTotalcount - 1;
                            }
                            else
                            {
                                selectedFoupIndex--;
                            }
                        }
                        else if (arg == SELECT_DIRECTION.NEXT)
                        {
                            // 첫 FOUP으로 
                            if (selectedFoupIndex == foupTotalcount - 1)
                            {
                                selectedFoupIndex = 0;
                            }
                            else
                            {
                                selectedFoupIndex++;
                            }
                        }

                        foreach (var item in SelectedFoupObj.LotSettings)
                        {
                            if(item.IsSelected == true && item.IsAssigned == false)
                            {
                                item.IsSelected = false;
                            }
                        }

                        SelectedFoupObj = LoaderModule.Foups[selectedFoupIndex];

                        LoggerManager.Debug($"[{this.GetType().Name}], FoupIndexForLotChangeCommandFunc() : SelectedFoup Index = {SelectedFoupObj.Index}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        AsyncCommand<object> _SelectedSlotItemChecked;
        public ICommand SelectedSlotItemChecked
        {
            get
            {
                if (null == _SelectedSlotItemChecked)
                    _SelectedSlotItemChecked = new AsyncCommand<object>(SelectedSlotItemCheckedFunc, false);
                return _SelectedSlotItemChecked;
            }
        }

        private async Task SelectedSlotItemCheckedFunc(object obj)
        {
            int index;
            int reverseindex;
            //int pageindex;
            bool ret;
            //bool bSelAll, bDeSelAll;

            ret = int.TryParse(obj.ToString(), out index);

            try
            {
                // TODO : 
                //index = index - 1; //Array의 index는 0~24
                reverseindex = (SelectedFoupObj.Slots.Count) - index;  //Foup UI Slot index: 1~25
                bool reversevalue = !SelectedFoupObj.Slots[reverseindex].IsPreSelected;

                if (SelectedFoupObj.ScanState == CassetteScanStateEnum.READ)
                {
                    if (SelectedFoupObj.Slots[reverseindex].WaferObj?.CurrHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        SelectedFoupObj.Slots[reverseindex].IsPreSelected = reversevalue;
                        LoggerManager.Debug($"[LoaderStageSummaryViewModel], SelectedSlotItemCheckedFunc() : Selected slot index = {index}, reverseindex = {reverseindex}, IsChecked = {SelectedFoupObj.Slots[reverseindex].IsPreSelected}");
                    }
                    else
                    {
                        SelectedFoupObj.Slots[reverseindex].IsPreSelected = false;
                        LoggerManager.Debug($"[LoaderStageSummaryViewModel], SelectedSlotItemCheckedFunc() : Selected slot index = {index}, reverseindex = {reverseindex}, CurrHolder {SelectedFoupObj.Slots[reverseindex].WaferObj?.CurrHolder.ModuleType}");
                    }
                }
                else if (SelectedFoupObj.ScanState == CassetteScanStateEnum.ILLEGAL)
                {
                    SelectedFoupObj.Slots[reverseindex].IsPreSelected = false;
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Slot Selected Error", $"The scan information is not valid.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    SelectedFoupObj.Slots[reverseindex].IsPreSelected = reversevalue;
                    LoggerManager.Debug($"[LoaderStageSummaryViewModel], SelectedSlotItemCheckedFunc() : Selected slot index = {index}, reverseindex = {reverseindex}, IsChecked = {SelectedFoupObj.Slots[reverseindex].IsPreSelected}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand<object> _SelectedTabChangedonLotsettingCommand;
        public ICommand SelectedTabChangedonLotsettingCommand
        {
            get
            {
                if (null == _SelectedTabChangedonLotsettingCommand)
                    _SelectedTabChangedonLotsettingCommand = new AsyncCommand<object>(SelectedTabChangedonLotsettingCommandFunc, false);
                return _SelectedTabChangedonLotsettingCommand;
            }
        }
        private async Task SelectedTabChangedonLotsettingCommandFunc(object obj)
        {
            int pageindex, reverseindex;

            try
            {
                SelectedFoupObj = obj as FoupObject;
                pageindex = SelectedFoupObj.Index;

                if (obj != null)
                {
                    int selectedcount = SelectedFoupObj.Index;

                    LoggerManager.Debug($"[LoaderStageSummaryViewModel], SelectedItemChangedonLotsettingCommandFunc() : Selcted Tab's Index = {selectedcount}");

                    if ((int)LoaderModule.Foups[pageindex].LotState == 0) //idle
                    {
                        LoaderModule.Foups[pageindex].EnableLotSetting = true;
                    }
                    else
                    {
                        LoaderModule.Foups[pageindex].EnableLotSetting = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<EnumSelectType> _SlotSelectForLotCommand;
        public ICommand SlotSelectForLotCommand
        {
            get
            {
                if (null == _SlotSelectForLotCommand)
                    _SlotSelectForLotCommand = new AsyncCommand<EnumSelectType>(SlotSelectForLotCommandFunc, false);
                return _SlotSelectForLotCommand;
            }
        }

        private async Task SlotSelectForLotCommandFunc(EnumSelectType arg)
        {
            try
            {
                bool selectval = false;

                if (arg == EnumSelectType.SELECTALL)
                {
                    selectval = true;
                }
                else if (arg == EnumSelectType.DESELCTEALL)
                {
                    selectval = false;
                }
                else
                {
                    // TODO : ERROR
                }

                if (SelectedFoupObj != null && SelectedFoupObj.Slots != null)
                {
                    if (SelectedFoupObj.ScanState == CassetteScanStateEnum.READ && arg == EnumSelectType.SELECTALL)
                    {
                        foreach (var slot in SelectedFoupObj.Slots)
                        {
                            if (SelectedFoupObj.Slots[SelectedFoupObj.Slots.Count - slot.Index].WaferObj?.CurrHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                slot.IsPreSelected = selectval;
                            }
                            else
                            {
                                slot.IsPreSelected = !selectval;
                            }
                            LoggerManager.Debug($"[LoaderStageSummaryViewModel], SlotSelectForLotCommandFunc() : slot {slot.Index} Selected {slot.IsPreSelected}");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderStageSummaryViewModel], SlotSelectForLotCommandFunc() : Argument = {arg}");
                        foreach (var slot in SelectedFoupObj.Slots)
                        {
                            slot.IsPreSelected = selectval;
                        }

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<EnumSelectType> _CellSelectForLotCommand;
        public ICommand CellSelectForLotCommand
        {
            get
            {
                if (null == _CellSelectForLotCommand)
                    _CellSelectForLotCommand = new AsyncCommand<EnumSelectType>(CellSelectForLotCommandFunc, false);
                return _CellSelectForLotCommand;
            }
        }

        private async Task CellSelectForLotCommandFunc(EnumSelectType arg)
        {
            try
            {
                bool selectval = false;

                if (arg == EnumSelectType.SELECTALL)
                {
                    selectval = true;
                }
                else if (arg == EnumSelectType.DESELCTEALL)
                {
                    selectval = false;
                }

                foreach (var stage in Stages)
                {
                    if ((selectval && (stage.StageMode == GPCellModeEnum.MAINTENANCE || stage.StageMode == GPCellModeEnum.ONLINE)) ||
                        selectval == false)
                    {
                        // TODO : 인덱스 확인
                        var lotSetting = SelectedFoupObj.LotSettings.FirstOrDefault(x => x.Index == stage.Index);

                        if (lotSetting != null)
                        {
                            lotSetting.IsSelected = selectval;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _StartSelectedLot;

        public ICommand StartSelectedLot
        {
            get
            {
                if (null == _StartSelectedLot)
                    _StartSelectedLot = new AsyncCommand(StartSelectedLotFunc, true);
                return _StartSelectedLot;
            }
        }
        private async Task StartSelectedLotFunc()
        {
            try
            {
                if (SelectedFoupObj == null)
                {
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Slot Selected Error", $"Please set the Slot again", EnumMessageStyle.Affirmative);
                }
                else if (LoaderMaster.ActiveLotInfos[SelectedFoupObj.Index - 1].State != LotStateEnum.Running)
                {
                    LoaderMaster.GEMModule().IsExternalLotMode = true;

                    // FoupObject의 Index는 1부터 할당되어 있음.
                    int selectedFoupIndex = SelectedFoupObj.Index;

                    bool AvailablePA = true;
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.PACount; i++)
                    {
                        var pa = LoaderModule.ModuleManager.FindModule<IPreAlignable>(ModuleTypeEnum.PA, i + 1);

                        if(pa.Enable == true)
                        {
                            var holder = pa.Holder;
                            if (holder.Status == EnumSubsStatus.EXIST)
                            {
                                AvailablePA = false;
                            }
                            else
                            {
                                AvailablePA = true;
                                break;
                            }
                        }
                        else
                        {
                            AvailablePA = false;
                        }
                    }

                    bool AvailableArm = false;
                    var ARMs = LoaderModule.ModuleManager.FindModules<IARMModule>();
                    if (ARMs != null)
                    {
                        AvailableArm = ARMs.Where(x => x.Holder.Status == EnumSubsStatus.NOT_EXIST).Count() > 1;
                    }
                
                    bool Availablebuffer = true;
                    var buffers = LoaderModule.ModuleManager.FindModules<IBufferModule>();
                    if (buffers != null) 
                    {
                        Availablebuffer = buffers.Where(x => x.Enable == true && x.Holder.Status == EnumSubsStatus.NOT_EXIST).Count() > 1;
                        if (buffers.Where(x => x.Enable == true).Count() == 0) 
                        {
                            Availablebuffer = true;
                        }
                    }

                    bool ExistAssignedCell = SelectedFoupObj.LotSettings.Any(x => x.IsAssigned == true);

                    // 선택된 셀 중, Wafer가 존재하는 셀 확인

                    bool WaferexistCell = false;
                    string devicename = "";
                    foreach (var setting in SelectedFoupObj.LotSettings)
                    {
                        if (setting.IsSelected == true && setting.IsAssigned == true && setting.IsVerified == true)
                        {
                            devicename = setting.RecipeName;
                            var stage = Stages.FirstOrDefault(x => x.Index == setting.Index);

                            if (stage.StageState == ModuleStateEnum.IDLE && stage.WaferStatus == EnumSubsStatus.EXIST)
                            {
                                WaferexistCell = true;
                                break;
                            }
                        }
                    }

                    bool isLotrunning = LoaderMaster.ActiveLotInfos.Any(x => x.State == LotStateEnum.Running);
                    bool IsSelectedCell = SelectedFoupObj.LotSettings.Any(x => x.IsSelected == true);
                    
                    bool existSelectedSlot = SelectedFoupObj.Slots.Any(x => x.IsPreSelected == true);
                    if (existSelectedSlot == false)
                    {
                        // 선택 된 Slot이 존재하지 않는 경우
                        var retVal = await this.MetroDialogManager().ShowMessageDialog("Slot Selected Error", $"Please set the Slot again", EnumMessageStyle.Affirmative);
                    }
                    else if (ExistAssignedCell == false)
                    {
                        var retVal = await this.MetroDialogManager().ShowMessageDialog("Stage Selected Error", $"Please set the Stage again", EnumMessageStyle.Affirmative);
                    }
                    else if (WaferexistCell == true)
                    {
                        var retVal = await this.MetroDialogManager().ShowMessageDialog("Stage Selected Error", $"There is already a wafer on the chuck. Please check the selected stage again", EnumMessageStyle.Affirmative);
                    }
                    else if (AvailablePA == false && isLotrunning == false)
                    {
                        var retVal = await this.MetroDialogManager().ShowMessageDialog("PreAlign Error", $"There is no PreAligner available. Please Check the PreAligner", EnumMessageStyle.Affirmative);
                    }
                    else if (Availablebuffer == false && isLotrunning == false)
                    {
                        var retVal = await this.MetroDialogManager().ShowMessageDialog("Wafer buffer Error", $"Please Check the Wafer buffer", EnumMessageStyle.Affirmative);
                    }
                    else if (AvailableArm == false && isLotrunning == false)
                    {
                        var retVal = await this.MetroDialogManager().ShowMessageDialog("Wafer arm Error", $"Please Check the Wafer arm", EnumMessageStyle.Affirmative);
                    }
                    else if (IsSelectedCell == false)
                    {
                        var retVal = await this.MetroDialogManager().ShowMessageDialog("Stage Selected Error", $"The selected cells do not exist. Please select the cells", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        //maintenance mode가있는지 체크
                        (bool CellMode, string maintenanceOfstages) = isCellModeMAINTENANCE();
                        if (CellMode)//maintenance mode인 Cell이 있을 때
                        {
                            var dlgRel = await this.MetroDialogManager().ShowMessageDialog("Do not Lot Start.", $"Cell {maintenanceOfstages} Mode is MAINTENANCE. Do you want Lot Start?", EnumMessageStyle.AffirmativeAndNegative, "YES", "NO");
                            if (dlgRel == EnumMessageDialogResult.NEGATIVE) // NO
                            {
                                return;
                            }
                        }

                        int recipeDicindex;

                        var modules = LoaderMaster.Loader.ModuleManager;
                        var Cassette = LoaderMaster.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, selectedFoupIndex);

                        Foup.FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());

                        LoaderMaster.Loader.ScanCount = 25;

                        Cassette.SetNoReadScanState();
                        var retval = LoaderMaster.DeviceManager().RecipeValidation(devicename, Cassette.Device.AllocateDeviceInfo.Size.Value);
                        if (retval != EventCodeEnum.NONE)
                        {
                            this.MetroDialogManager().ShowMessageDialog("Do not Lot Start.", $"The selected device's wafer size and the wafer size detected in the FOUP do not match. Please verify the wafer size.", EnumMessageStyle.Affirmative);
                            Foup.FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                            ClearLotSettingCommandFunc(EnumClearType.ALL);
                            return;
                        }
                        #region ActiveProcessActReqData

                        ActiveProcessActReqData activeData = new ActiveProcessActReqData();

                        activeData.LotID = SelectedFoupObj.PreLotID;
                        //activeData.LotID = LotSettings[SelectedFoupObj.Index].LotID;
                        activeData.FoupNumber = selectedFoupIndex;

                        for (int i = 0; i < SelectedFoupObj.LotSettings.Count; i++)
                        {
                            activeData.UseStageNumbers_str = activeData.UseStageNumbers_str + "0";

                            var lotsetting = SelectedFoupObj.LotSettings[i];

                            if (lotsetting.IsAssigned == true &&
                                lotsetting.IsVerified == true &&
                                lotsetting.IsSelected == true)
                            {
                                activeData.UseStageNumbers.Add(i + 1);
                            }
                        }

                        LoaderMaster.ActiveProcess(activeData);

                        #endregion


                        #region DownloadStageRecipeActReqData

                        for (int i = 0; i < SelectedFoupObj.LotSettings.Count; i++)
                        {
                            recipeDicindex = i + 1;

                            var lotsetting = SelectedFoupObj.LotSettings[i];

                            if (lotsetting.IsAssigned == true &&
                                lotsetting.IsVerified == true &&
                                lotsetting.IsSelected == true)
                            {
                                // 사전에 이미, Stage Index에 해당하는 데이터가 들어가 있는 경우, Remove
                                if (DownloadReqData.RecipeDic.ContainsKey(recipeDicindex))
                                {
                                    DownloadReqData.RecipeDic.Remove(recipeDicindex);
                                }

                                //  IsAssigned 가 되었지만 랏드 스타트 시에 
                                DownloadReqData.RecipeDic.Add(recipeDicindex, lotsetting.RecipeName);
                            }
                            else
                            {
                                if (DownloadReqData.RecipeDic.ContainsKey(recipeDicindex))
                                {
                                    LoggerManager.Debug($"[LoaderStageSummaryViewModel], StartSelectedLotFunc() : Remove RecipeDic stage {recipeDicindex} ");
                                    DownloadReqData.RecipeDic.Remove(recipeDicindex);
                                }
                            }
                        }

                        DownloadReqData.LotID = activeData.LotID;
                        DownloadReqData.FoupNumber = activeData.FoupNumber;
                        DownloadReqData.UseFTP = false;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // ActiveLotInfo의 DeviceName이 할당 됨.
                            var ret = LoaderMaster.DeviceManager().SetRecipeToStage(DownloadReqData);
                            LoaderMaster.SetRecipeToDevice(DownloadReqData);
                        });

                        isWaitDownloadRecipe = true;

                        // 모든 셀 로드 완료 확인
                        Task task = new Task(() =>
                        {
                            while (isWaitDownloadRecipe == true)
                            {
                                if (GemModule.GetComplateDownloadRecipe(activeData.UseStageNumbers))
                                {
                                    isWaitDownloadRecipe = false;
                                }
                            }
                        });
                        task.Start();
                        await task;

                        #endregion


                        #region DockFoupActReqData

                        DockFoupActReqData dockFoupActReqData = new DockFoupActReqData();
                        dockFoupActReqData.FoupNumber = selectedFoupIndex;

                        #endregion

                        #region SelectSlotsActReqData

                        List<int> slotList = new List<int>();

                        // Allocate SlotList
                        //for (int i = 0; i < LoaderModule.Foups[SelectedFoupObj.Index].Slots.Count; i++)
                        for (int i = 0; i < SelectedFoupObj.Slots.Count; i++)
                        {
                            if (SelectedFoupObj.Slots[i].IsPreSelected == true)
                            {
                                slotList.Add(SelectedFoupObj.Slots[i].Index);
                            }
                        }

                        #region PMI DATA SET

                        // NONE
                        if (SelectedPMITriggerType == PMIRemoteTriggerEnum.UNDIFINED)
                        {
                            LoaderMaster.ActiveLotInfos[selectedFoupIndex - 1].PMIEveryInterval = 0;
                            LoaderMaster.ActiveLotInfos[selectedFoupIndex - 1].DoPMICount = 0;
                        }
                        // EVERY
                        else if (SelectedPMITriggerType == PMIRemoteTriggerEnum.EVERY_WAFER_TRIGGER)
                        {
                            LoaderMaster.ActiveLotInfos[selectedFoupIndex - 1].PMIEveryInterval = InputPMICount;
                        }
                        // TOTAL
                        else if (SelectedPMITriggerType == PMIRemoteTriggerEnum.TOTALNUMBER_WAFER_TRIGGER)
                        {
                            LoaderMaster.ActiveLotInfos[selectedFoupIndex - 1].DoPMICount = InputPMICount;
                        }

                        #endregion

                        SelectSlotsActReqData selectSlotsActReqData = new SelectSlotsActReqData();
                        selectSlotsActReqData.LotID = activeData.LotID;
                        selectSlotsActReqData.FoupNumber = selectedFoupIndex;
                        selectSlotsActReqData.UseSlotNumbers = slotList;

                        #endregion

                        #region StartLotActReqData

                        StartLotActReqData startLotActReqData = new StartLotActReqData();
                        startLotActReqData.LotID = activeData.LotID;
                        startLotActReqData.FoupNumber = selectedFoupIndex;

                        #endregion

                        task = new Task(() =>
                        {
                            GemModule.GemCommManager.OnRemoteCommandAction(dockFoupActReqData);
                            GemModule.GemCommManager.OnRemoteCommandAction(selectSlotsActReqData);
                            GemModule.GemCommManager.OnRemoteCommandAction(startLotActReqData);
                        });
                        task.Start();
                        await task;

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private (bool, string) isCellModeMAINTENANCE()
        {
            bool CellModeCheck = false;

            string maintenanceOfstages = string.Empty;

            foreach (var setting in SelectedFoupObj.LotSettings)
            {
                if (setting.IsSelected == true && setting.IsAssigned == true && setting.IsVerified == true)
                {
                    var stage = Stages.FirstOrDefault(x => x.Index == setting.Index);

                    if (stage.StageMode == GPCellModeEnum.MAINTENANCE)
                    {
                        maintenanceOfstages += $"{stage.StageInfo.Index}, ";
                        CellModeCheck = true;
                    }
                }
            }

            if (string.IsNullOrEmpty(maintenanceOfstages) == false)
            {
                maintenanceOfstages.TrimEnd(',');
            }

            return (CellModeCheck, maintenanceOfstages);
        }

        private AsyncCommand<Object> _DeviceAssignonLotsettingCommand;
        public ICommand DeviceAssignonLotsettingCommand
        {
            get
            {
                if (null == _DeviceAssignonLotsettingCommand)
                    _DeviceAssignonLotsettingCommand = new AsyncCommand<Object>(DeviceAssignonLotsettingCommandFunc);
                return _DeviceAssignonLotsettingCommand;
            }
        }

        private async Task DeviceAssignonLotsettingCommandFunc(Object obj)
        {
            try
            {
                IList IList_Dev = obj as IList;

                if (IList_Dev != null && IList_Dev.Count > 0)
                {
                    string val = IList_Dev[0].ToString();

                    if (string.IsNullOrEmpty(val) == false)
                    {
                        strCurrentInputDevName = val;
                    }
                    else
                    {
                        strCurrentInputDevName = string.Empty;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AssignLotSettingCommand;
        public ICommand AssignLotSettingCommand
        {
            get
            {
                if (null == _AssignLotSettingCommand)
                    _AssignLotSettingCommand = new AsyncCommand(AssignLotSettingCommandFunc, false);
                return _AssignLotSettingCommand;
            }
        }

        private async Task AssignLotSettingCommandFunc()
        {
            try
            {
                bool bFTPAvailable;
                bool bFTPneed = false;

                bool ExistSelectedCell = SelectedFoupObj.LotSettings.Any(x => x.IsSelected == true);
                bool ExistNotConnectedCell = false;

                // 선택 된 셀이 존재하지 않는 경우
                if (!ExistSelectedCell)
                {
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Information Message", $"The selected cells do not exist. Please select the cells.", EnumMessageStyle.Affirmative);
                    return;
                }

                // 선택 된 셀 중, 연결 상태가 이상한 경우
                foreach (var lotsetting in SelectedFoupObj.LotSettings)
                {
                    if (lotsetting.IsSelected)
                    {
                        var stage = Stages.FirstOrDefault(x => x.Index == lotsetting.Index);

                        if (stage != null)
                        {
                            if (stage.StageMode != GPCellModeEnum.MAINTENANCE && stage.StageMode != GPCellModeEnum.ONLINE)
                            {
                                ExistNotConnectedCell = true;
                                break;
                            }
                        }
                    }
                }

                if (ExistNotConnectedCell)
                {
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Information Message", $"Please check the cell's connection status", EnumMessageStyle.Affirmative);
                    return;
                }

                // LotName이 공백인 경우
                if (string.IsNullOrEmpty(strCurrentInputLotName))
                {
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Information Message", $"Lot name is empty. Please key in the name of lot.", EnumMessageStyle.Affirmative);
                    return;
                }

                // LotName의 길이가 4자리보다 작은 경우
                if (strCurrentInputLotName.Length < 4)
                {
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Information Message", $"Lot name must be at least 4 characters long. Please set the Lot name again.", EnumMessageStyle.Affirmative);
                    return;
                }

                // 디바이스 이름이 공백인 경우(= 선택되지 않은 경우)
                if (string.IsNullOrEmpty(strCurrentInputDevName))
                {
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Information Message", $"The selected device does not exist. Please select the device.", EnumMessageStyle.Affirmative);
                    return;
                }

                SelectedFoupObj.PreLotID = strCurrentInputLotName;

                var selectedSettings = SelectedFoupObj.LotSettings.Where(x => x.IsSelected == true);

                bool IsServerPathCheck = false;

                EventCodeEnum IsServerPathValid = EventCodeEnum.UNDEFINED;

                if(LotMode == LotModeEnum.MPP)
                {
                    try
                    {
                        // 한번만 수행해도 됨.
                        IsServerPathValid = LoaderResultMapUpDownMng.ServerResultMapPathCheck(SelectedFoupObj.PreLotID);
                        IsServerPathCheck = true;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
                else
                {
                    IsServerPathValid = EventCodeEnum.NONE;
                }

                if (IsServerPathCheck)
                {
                    if(IsServerPathValid != EventCodeEnum.NONE)
                    {
                        var retVal = await this.MetroDialogManager().ShowMessageDialog("Information Message", $"Please check the Result map server path.", EnumMessageStyle.Affirmative);
                    }
                }

                // CassetteModule에 LotMode 할당
                var cassette = LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, SelectedFoupObj.Index);

                if (cassette != null)
                {
                    cassette.LotMode = LotMode;
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], AssignLotSettingCommandFunc() : cassette is null. index = {SelectedFoupObj.Index}");
                }

                // Assign
                foreach (var lotsetting in selectedSettings)
                {
                    lotsetting.RecipeName = strCurrentInputDevName;
                    lotsetting.LotName = SelectedFoupObj.PreLotID;
                    lotsetting.OperatorName = strCurrentInputOperatorName;
                    lotsetting.LotMode = LotMode;
                    lotsetting.IsAssigned = true;
                    if (LoaderModule.GetUseLotProcessingVerify()) 
                    {
                        lotsetting.IsVerified = false;
                    }

                    LoggerManager.Debug($"[{this.GetType().Name}], AssignLotSettingCommandFunc() : Foup#{SelectedFoupObj.Index}, cell[{lotsetting.Index}] is Assign , LotName [{SelectedFoupObj.PreLotID}]");

                    Task task = new Task(() =>
                    {
                        IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(lotsetting.Index);

                        if (proxy != null)
                        {
                            proxy.SetOperatorName(lotsetting.OperatorName);

                            var retVal = proxy.CheckAndConnect();

                            FoupAllocatedInfo allocatedInfo = new FoupAllocatedInfo(SelectedFoupObj.Index, lotsetting.RecipeName, lotsetting.LotName);
                            EventCodeEnum ret = proxy.FoupAllocated(allocatedInfo);
                        }
                    });
                    task.Start();
                    await task;
                }

                // For converter's trigger
                TriggerForStartConfirm = !TriggerForStartConfirm;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private AsyncCommand<EnumClearType> _ClearLotSettingCommand;
        public ICommand ClearLotSettingCommand
        {
            get
            {
                if (null == _ClearLotSettingCommand)
                    _ClearLotSettingCommand = new AsyncCommand<EnumClearType>(ClearLotSettingCommandFunc, false);
                return _ClearLotSettingCommand;
            }
        }

        private async Task ClearLotSettingCommandFunc(EnumClearType arg)
        {
            try
            {
                // 선택 된 셀들의 LotSetting 정보들을 초기화.
                bool useLotProcessingVerify = LoaderModule.GetUseLotProcessingVerify();
                foreach (var lotsetting in SelectedFoupObj.LotSettings)
                {
                    if (arg == EnumClearType.SELECTALL)
                    {
                        if (lotsetting.IsSelected == true)
                        {
                            lotsetting.Clear(useLotProcessingVerify);
                        }
                    }
                    else
                    {
                        lotsetting.Clear(useLotProcessingVerify);
                    }
                }

                SelectedFoupObj.PreLotID = string.Empty;

                foreach (var slot in SelectedFoupObj.Slots)
                {
                    slot.IsPreSelected = false;
                }

                TriggerForStartConfirm = !TriggerForStartConfirm;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        //private AsyncCommand _ClearSelectedLotsettingCommand;
        //public ICommand ClearSelectedLotsettingCommand
        //{
        //    get
        //    {
        //        if (null == _ClearSelectedLotsettingCommand) _ClearSelectedLotsettingCommand = new AsyncCommand(ClearSelectedLotsettingCommandFunc, false);
        //        return _ClearSelectedLotsettingCommand;
        //    }
        //}
        //private async Task ClearSelectedLotsettingCommandFunc()
        //{
        //    int selectedindex;

        //    try
        //    {
        //        // 선택 된 셀들의 LotSetting 정보들을 초기화.

        //        foreach (var stage in Stages)
        //        {
        //            if(stage.LotSetting.IsSelected == true)
        //            {
        //                stage.LotSetting.Clear();
        //            }
        //        }

        //        SelectedFoupObj.PreLotID = string.Empty;

        //        foreach (var slot in SelectedFoupObj.Slots)
        //        {
        //            slot.IsPreSelected = false;
        //        }

        //        //while (LotSettings[SelectedFoupObj.Index].IselectedItemList.Count > 0)
        //        //{
        //        //    if (LotSettings[SelectedFoupObj.Index].IselectedItemList.Count > CurrentStages_forLot.Count)
        //        //    {
        //        //        break;
        //        //    }

        //        //    if (LotSettings[SelectedFoupObj.Index].IselectedItemList.Count == 1)
        //        //    {
        //        //        selectedindex = LotSettings[SelectedFoupObj.Index].IselectedItemList[0];
        //        //        CurrentStages_forLot[selectedindex].LotSetting.RecipeName = "Empty";
        //        //        CurrentStages_forLot[selectedindex].LotSetting.LotName = "Empty";
        //        //        CurrentStages_forLot[selectedindex].LotSetting.bIsassigned = false;
        //        //        CurrentStages_forLot[selectedindex].LotSetting.bIsSelected = false;

        //        //        //PMI
        //        //        CurrentStages_forLot[selectedindex].LotSetting.iPmiCount = 0;
        //        //        CurrentStages_forLot[selectedindex].LotSetting.ipmiType = 0;

        //        //        break;
        //        //    }
        //        //    else
        //        //    {
        //        //        selectedindex = LotSettings[SelectedFoupObj.Index].IselectedItemList[0];
        //        //        CurrentStages_forLot[selectedindex].LotSetting.RecipeName = "Empty";
        //        //        CurrentStages_forLot[selectedindex].LotSetting.LotName = "Empty";
        //        //        CurrentStages_forLot[selectedindex].LotSetting.bIsassigned = false;
        //        //        CurrentStages_forLot[selectedindex].LotSetting.bIsSelected = false;

        //        //        //PMI
        //        //        CurrentStages_forLot[selectedindex].LotSetting.iPmiCount = 0;
        //        //        CurrentStages_forLot[selectedindex].LotSetting.ipmiType = 0;
        //        //    }
        //        //}

        //        //LotSettings[SelectedFoupObj.Index].IselectedItemList.Clear();
        //        //LotSettings[SelectedFoupObj.Index].LotID = "Empty";

        //        //for (int j = 0; j < LoaderModule.Foups[SelectedFoupObj.Index].Slots.Count(); j++)
        //        //{
        //        //    //LotSettings[SelectedFoupObj.Index].bSlotSelectedList[j] = false;
        //        //    LoaderModule.Foups[SelectedFoupObj.Index].Slots[j].IsPreSelected = false;
        //        //}

        //        //Lotsettings_AllStagesSelected = false;
        //        //Lotsettings_AllStagesDeSelected = true;

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private AsyncCommand _ClearLotsettingCommand;
        //public ICommand ClearLotsettingCommand
        //{
        //    get
        //    {
        //        if (null == _ClearLotsettingCommand) _ClearLotsettingCommand = new AsyncCommand(ClearLotsettingCommandFunc, false);
        //        return _ClearLotsettingCommand;
        //    }
        //}

        //private async Task ClearLotsettingCommandFunc()
        //{
        //    try
        //    {
        //        // 1. Stage가 갖고 있는 LotSetting의 값을 초기화
        //        // 2. FoupObject가 갖고 있는 PreLotID를 초기화
        //        // 3. 현재 선택되어 있는 Cell 정보들과 Slot 정보들을 초기화

        //        foreach (var stage in Stages)
        //        {
        //            stage.LotSetting.Clear();
        //        }

        //        SelectedFoupObj.PreLotID = string.Empty;

        //        //LotSettings[SelectedFoupObj.Index].LotID = "Empty";
        //        //LotSettings[SelectedFoupObj.Index].IselectedItemList.Clear();

        //        for (int j = 0; j < LoaderModule.Foups[SelectedFoupObj.Index].Slots.Count(); j++)
        //        {
        //            //LotSettings[SelectedFoupObj.Index].bSlotSelectedList[j] = false;
        //            LoaderModule.Foups[SelectedFoupObj.Index].Slots[j].IsPreSelected = false;
        //        }

        //        //for (int i = 0; i < CurrentStages_forLot.Count; i++)
        //        //{
        //        //    CurrentStages_forLot[i].LotSetting.RecipeName = "Empty";
        //        //    CurrentStages_forLot[i].LotSetting.LotName = "Empty";
        //        //    CurrentStages_forLot[i].LotSetting.bIsassigned = false;
        //        //    CurrentStages_forLot[i].LotSetting.bIsSelected = false;

        //        //    //PMI
        //        //    CurrentStages_forLot[i].LotSetting.iPmiCount = 0;
        //        //    CurrentStages_forLot[i].LotSetting.ipmiType = 0;
        //        //}

        //        //LotSettings[SelectedFoupObj.Index].LotID = "Empty";
        //        //LotSettings[SelectedFoupObj.Index].IselectedItemList.Clear();

        //        //for (int j = 0; j < LoaderModule.Foups[SelectedFoupObj.Index].Slots.Count(); j++)
        //        //{
        //        //    //LotSettings[SelectedFoupObj.Index].bSlotSelectedList[j] = false;
        //        //    LoaderModule.Foups[SelectedFoupObj.Index].Slots[j].IsPreSelected = false;
        //        //}

        //        //Lotsettings_AllStagesSelected = false;
        //        //Lotsettings_AllStagesDeSelected = true;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}



        private AsyncCommand<object> _LotNameAssignonLotsettingCommand;
        public ICommand LotNameAssignonLotsettingCommand
        {
            get
            {
                if (null == _LotNameAssignonLotsettingCommand)
                    _LotNameAssignonLotsettingCommand = new AsyncCommand<object>(LotNameAssignonLotsettingCommandFunc, false);
                return _LotNameAssignonLotsettingCommand;
            }
        }

        private async Task LotNameAssignonLotsettingCommandFunc(object obj)
        {
            try
            {
                if (obj is System.Windows.Controls.TextBox)
                {
                    System.Windows.Controls.TextBox tb_LOTID = (System.Windows.Controls.TextBox)obj;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        tb_LOTID.Text = VirtualKeyboard.Show(tb_LOTID.Text, KB_TYPE.ALPHABET | KB_TYPE.DECIMAL);

                        if (string.IsNullOrEmpty(tb_LOTID.Text) == false)
                        {
                            strCurrentInputLotName = tb_LOTID.Text;
                        }
                        else
                        {
                            strCurrentInputLotName = string.Empty;
                        }
                    });

                    //tb_LOTID.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                strCurrentInputLotName = string.Empty;
            }
        }

        private AsyncCommand<object> _OperatorNameAssignonLotsettingCommand;
        public ICommand OperatorNameAssignonLotsettingCommand
        {
            get
            {
                if (null == _OperatorNameAssignonLotsettingCommand)
                    _OperatorNameAssignonLotsettingCommand = new AsyncCommand<object>(OperatorNameAssignonLotsettingCommandFunc, false);
                return _OperatorNameAssignonLotsettingCommand;
            }
        }

        private async Task OperatorNameAssignonLotsettingCommandFunc(object obj)
        {
            try
            {
                if (obj is System.Windows.Controls.TextBox)
                {
                    System.Windows.Controls.TextBox tb_OperatorID = (System.Windows.Controls.TextBox)obj;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        tb_OperatorID.Text = VirtualKeyboard.Show(tb_OperatorID.Text, KB_TYPE.ALPHABET | KB_TYPE.DECIMAL);

                        if (string.IsNullOrEmpty(tb_OperatorID.Text) == false)
                        {
                            strCurrentInputOperatorName = tb_OperatorID.Text;
                        }
                        else
                        {
                            strCurrentInputOperatorName = string.Empty;
                        }
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                strCurrentInputLotName = string.Empty;
            }
        }

        private AsyncCommand<object> _PMICountAssignonLotsettingCommand;
        public ICommand PMICountAssignonLotsettingCommand
        {
            get
            {
                if (null == _PMICountAssignonLotsettingCommand)
                    _PMICountAssignonLotsettingCommand = new AsyncCommand<object>(PMICountAssignonLotsettingCommandFunc);
                return _PMICountAssignonLotsettingCommand;
            }
        }

        private async Task PMICountAssignonLotsettingCommandFunc(object obj)
        {
            try
            {
                if (SelectedPMITriggerType == PMIRemoteTriggerEnum.UNDIFINED)
                {
                    string tmpstr = EnumExtensions.GetDescription(SelectedPMITriggerType);

                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Information Message", $"Selected trigger type is {tmpstr}. Please select other trigger type.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    if (obj is System.Windows.Controls.TextBox)
                    {
                        System.Windows.Controls.TextBox tb_LOTID = (System.Windows.Controls.TextBox)obj;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            tb_LOTID.Text = VirtualKeyboard.Show(tb_LOTID.Text, KB_TYPE.DECIMAL);
                            InputPMICount = Int32.Parse(tb_LOTID.Text);
                        });

                        //tb_LOTID.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                InputPMICount = 0;
            }
        }


        private AsyncCommand<object> _SelectedItemChangedonLotsettingCommand;
        public ICommand SelectedItemChangedonLotsettingCommand
        {
            get
            {
                if (null == _SelectedItemChangedonLotsettingCommand)
                    _SelectedItemChangedonLotsettingCommand = new AsyncCommand<object>(SelectedItemChangedonLotsettingCommandFunc, false);
                return _SelectedItemChangedonLotsettingCommand;
            }
        }

        private async Task SelectedItemChangedonLotsettingCommandFunc(object obj)
        {
            try
            {
                int stageidx = -1;

                List<IStagelotSetting> Assignedlist = new List<IStagelotSetting>();

                foreach (var item in SelectedFoupObj.LotSettings)
                {
                    if (item.IsSelected)
                    {
                        stageidx = item.Index - 1;

                        for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                        {
                            // 현재 선택되어 있는 Foup
                            if (SelectedFoupObj.Index == LoaderModule.Foups[i].Index)
                            {
                                // 할당되어 있는 경우
                                if (LoaderModule.Foups[i].LotSettings[stageidx].IsAssigned)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (LoaderModule.Foups[i].LotSettings[stageidx].IsAssigned == true)
                                {
                                    Assignedlist.Add(LoaderModule.Foups[i].LotSettings[stageidx]);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (Assignedlist.Count > 0)
                {
                    IStagelotSetting assgineditem = Assignedlist.First();

                    var currentSeletecItem = SelectedFoupObj.LotSettings.FirstOrDefault(x => x.Index == assgineditem.Index);

                    if (currentSeletecItem != null)
                    {
                        currentSeletecItem.IsSelected = false;
                    }

                    await this.MetroDialogManager().ShowMessageDialog($"Information Message", $"Selected Cell#{assgineditem.Index} is already assigned in Foup#{assgineditem.FoupNumber}. Please select unassigned cell.", EnumMessageStyle.Affirmative);
                }

                int count = SelectedFoupObj.LotSettings.Count(x => x.IsSelected == true);

                ChangedSelectedSettingItemsCount = count;
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

                            //brett// GEM 연결이 끊어졌다는 것은 해당 Cell이 Down되었다고 판단한다.
                            if (SelectedStage.StageInfo.IsConnected && !this.GEMModule().GemCommManager.GetRemoteConnectState(SelectedStage.Index))
                            {
                                this.MetroDialogManager().ShowMessageDialog("Error Message", $"The selected cell is disconnected(GEM). Please Reconnect Cell({SelectedStage.Index}).", EnumMessageStyle.Affirmative);
                            }

                            if (SelectedStage != null && SelectedStage.StageMode == GPCellModeEnum.MAINTENANCE)
                            {
                                //brett// GEM 연결이 끊어졌다는 것은 해당 Cell이 Down되었다고 판단한다.
                                if (this.GEMModule().GemCommManager.GetRemoteConnectState(SelectedStage.Index))
                                {
                                    LoaderCommunicationManager.GetProxy<IMotionAxisProxy>(SelectedStage.Index);
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
                    if (SelectedFoupObj != null)
                    {
                        if ((int)LoaderModule.Foups[SelectedFoupObj.Index - 1].LotState == 0) //idle
                        {
                            LoaderModule.Foups[SelectedFoupObj.Index - 1].EnableLotSetting = true;
                        }
                        else
                        {
                            LoaderModule.Foups[SelectedFoupObj.Index - 1].EnableLotSetting = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task SelectedItemChangedFunc(int LastSelectedIndex)
        {
            try
            {
                if (LastSelectedIndex != null)
                {
                    LoggerManager.Debug($"[LoaderStageSummaryViewModel], SelectedItemChangedCommandFunc() : Selcted cell's Index = {LastSelectedIndex}");

                    var disableStage = Stages[LastSelectedIndex - 1];

                    disableStage.IsEnableTransfer = true;
                    disableStage.StageInfo.AlarmEnabled = true;

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
                        SelectedStage = Stages.Where(x => x.Index == LastSelectedIndex).FirstOrDefault();
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
                    else if (SelectedStage.StageState == ModuleStateEnum.RUNNING || SelectedStage.StageState == ModuleStateEnum.ABORT)
                    {
                        CellLotPauseFlag = true;
                        CellLotResumeFlag = false;
                        CellLotEndFlag = false;
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
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                int index = -1;

                for (int i = 0; i < map.ChuckModules.Count(); i++)
                {
                    IStageObject CurStage = Stages.Where(x => x.Index - 1 == i).FirstOrDefault();

                    if (CurStage != null && CurStage.StageInfo.IsConnected)
                    {
                        index = CurStage.Index - 1;

                        if (this.LoaderMaster.StageStates.Count > i)
                        {
                            CurStage.StageState = this.LoaderMaster.StageStates[i];
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
                        IChuckModule Chuck = (IChuckModule)this.LoaderModule.ModuleManager.FindModule(ModuleTypeEnum.CHUCK, i + 1);
                        if (Chuck != null)
                        {
                            CurStage.IsWaferOnHandler = Chuck.Holder.IsWaferOnHandler;
                        }
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
                    if (i < SystemModuleCount.ModuleCnt.FoupCount)
                    {
                        //brett, ann// Systemmodecount의 foup count 설정값 보다 cassettemodule count 값이 클경우 exception 발생에 대한 예외처리 추가함
                        //추후(TBD) Cassette module count를 쓸지 systemmodecount값을 쓸지 결정 후 refactoring 해야함 
                        LoaderModule.Foups[i].ScanState = map.CassetteModules[i].ScanState;

                        var temp = this.FoupOpModule().FoupManagerSysParam_IParam as FoupManagerSystemParameter;
                        LoaderModule.Foups[i].CassetteType = temp.FoupModules[i].CassetteType.Value;                        
                    }

                    for (int j = 0; j < map.CassetteModules[i].SlotModules.Count(); j++)
                    {
                        //var slot = new SlotObject(map.CassetteModules[i].SlotModules.Count() - j);
                        if (map.CassetteModules[i].SlotModules[j].Substrate != null)
                        {
                            if (isExternalLotStart && LoaderMaster.ActiveLotInfos[i].State == LotStateEnum.Running) //foupNumber가 같을때
                            {
                                if (!(LoaderMaster.ActiveLotInfos[i].UsingSlotList.Where(idx => idx == LoaderModule.Foups[i].Slots[j].Index).FirstOrDefault() > 0))
                                {
                                    LoaderModule.Foups[i].Slots[j].WaferState = EnumWaferState.SKIPPED;
                                    var waferObj = LoaderModule.ModuleManager.FindTransferObject(map.CassetteModules[i].SlotModules[j].Substrate.ID.Value);

                                    if (waferObj != null)
                                    {
                                        waferObj.WaferState = EnumWaferState.SKIPPED;
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
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Cell Connect", $"Do you want to Connect a Cell{index}?", EnumMessageStyle.AffirmativeAndNegative);

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
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Cell Disconnect", $"Do you want to Disconnect a Cell{index}?", EnumMessageStyle.AffirmativeAndNegative);

                    if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        LoaderCommunicationManager.DisConnectStage(index);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ConnectCommandFunc(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            //finally
            //{
            //    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            //}
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
                var LotData = LoaderMaster.GetStageLotData(index);
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

        public void LotModeChangedCommandFunc(LotModeWithIndex obj)
        {
            try
            {
                if (obj != null)
                {
                    if (LoaderModule.Foups[SelectedFoupObj.Index - 1].LotState == LotStateEnum.Running)
                    {
                        LoggerManager.Debug($"[LoaderStageSummaryViewModel_GOP], LotModeChangedCommandFunc() called. LotState is Running, Can not change Mode");
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderStageSummaryViewModel_GOP], LotModeChangedCommandFunc() called. Index = {obj.Index}, Mode = {obj.Lotmode}");
                        foreach (var stage in Stages)
                        {
                            if (stage.StageInfo.IsConnected == true)
                            {
                                var proxy = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stage.Index);

                                if (proxy != null)
                                {
                                    proxy.ChangeLotMode(obj.Lotmode);
                                }
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
                Task task = new Task(() =>
                {
                    LoaderMaster.SetStopBeforeProbingFlag(index, Stages[index - 1].StopBeforeProbing);

                    var stage = LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(index);
                    if (stage != null)
                    {
                        stage.StopBeforeProbingCmd(Stages[index - 1].StopBeforeProbing);
                        LoggerManager.Error($"StopBeforeProbing Command Click. StageIndex : {index}, Turn on : {Stages[index - 1].StopBeforeProbing}");
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
                LoaderMaster.SetStopAfterProbingFlag(index, Stages[index - 1].StopAfterProbing);

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
                        if (beh != null)
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
                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog($"Monitoring Error({FuncName})", $"Error Code : {errorCode} \n\n\n\n" +
                        $"Pressing the recovery button performs this action\n" +
                        $"- {behavior.RecoveryDescription}"
                        , EnumMessageStyle.AffirmativeAndNegative, "Recovery", "Cancel");

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        if (string.IsNullOrEmpty(recoveryList) == false)
                        {
                            recoveryList = recoveryList.TrimEnd(new char[] { ',', ' ' });
                            LoggerManager.Debug($"MonitoringCheckCommandFunc() Pre Recoverylist is Exist");
                            this.MetroDialogManager().ShowMessageDialog($"Recovery Fail", $"Please recover these items first ({recoveryList})", EnumMessageStyle.Affirmative);
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
                    var ret = await this.MetroDialogManager().ShowMessageDialog($"Monitoring Error({FuncName})", $"Error Code : {errorCode} \n" +
                        $"Manual Recovery : {behavior.RecoveryDescription}", EnumMessageStyle.Affirmative);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}


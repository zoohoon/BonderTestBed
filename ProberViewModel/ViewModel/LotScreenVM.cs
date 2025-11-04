using AppSelector;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.Temperature;
using RelayCommandBase;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SubstrateObjects;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VirtualKeyboardControl;
using ProberInterfaces.LoaderController;
using System.Runtime.CompilerServices;
using ProberInterfaces.NeedleClean;
using BVisionTestViewModel;

namespace LotScreenViewModel
{
    using CUIServices;
    using LogModule;
    using LoaderControllerBase;
    //using Viewer3DModel;
    using System.Windows.Data;
    using LotOP;
    using LoaderTestDialog;
    using WaferSelectSetup;
    using MetroDialogInterfaces;
    using CognexOCRManualDialog;
    using LoaderParameters;
    using MarkObjects;
    using ProbeCardObject;
    using ProberInterfaces.Param;

    enum StageCam
    {
        WAFER_HIGH_CAM,
        WAFER_LOW_CAM,
        PIN_HIGH_CAM,
        PIN_LOW_CAM,
    }

    enum LoaderCam
    {
        PACL6_CAM,
        PACL8_CAM,
        PACL12_CAM,
        ARM_6_CAM,
        ARM_8_12_CAM,
        OCR1_CAM,
        OCR2_CAM,
    }

    public class CassetteTableContents : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string _infoTableName { get; set; }
        public string InfoTableName
        {
            get { return _infoTableName; }
            set
            {
                if (value != _infoTableName)
                {
                    _infoTableName = value;
                    NotifyPropertyChanged("_infoTableName");
                }
            }
        }

        public CassetteTableContents()
        {
            try
            {
                InfoTableName = "SKIP";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public CassetteTableContents(string Name)
        {
            try
            {
                this.InfoTableName = Name;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class LotScreenVM : WaferSelectSetupBase, IMainScreenViewModel, INotifyPropertyChanged, IHasCameraControl, IDisposable
    {
        readonly Guid _ViewModelGUID = new Guid("CBED19A9-1A90-43DB-B31F-DAF29BC852B4");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        //DispatcherTimer timer = new DispatcherTimer();

        private Action<PropertyChangedEventArgs> ActionRaisePropertyChanged()
        {
            try
            {
                return args => PropertyChanged?.Invoke(this, args);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public LotScreenVM()
        {

        }
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
            //bStopUpdateThread = true;
            //if(updateThread != null)
            //{
            //     updateThread.Join();
            //}
        }

        #region // AppToolkit usercontrol 용 properties
        private ObservableCollection<AppModuleItem> _AppItems;
        public ObservableCollection<AppModuleItem> AppItems
        {
            get { return _AppItems; }
            set { this.MutateVerbose(ref _AppItems, value, ActionRaisePropertyChanged()); }
        }

        int probingRemainCnt = 0;
        bool isSeqCheck = false;
        MachineIndex CurrentMI = new MachineIndex();
        public void ProbingSeqBackUp()
        {
            try
            {
                var seqModule = this.ProbingSequenceModule();
                CurrentMI = seqModule.ProbingSeqParameter.ProbingSeq.Value[seqModule.ProbingSequenceCount - seqModule.ProbingSequenceRemainCount];
                probingRemainCnt = seqModule.ProbingSequenceRemainCount;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _FastRewindCommand;
        public ICommand FastRewindCommand
        {
            get
            {
                if (null == _FastRewindCommand) _FastRewindCommand = new RelayCommand<object>(FastRewindCmdFunc);
                return _FastRewindCommand;
            }
        }

        private void FastRewindCmdFunc(object obj)
        {
            try
            {
                if (!isSeqCheck)
                {
                    ProbingSeqBackUp();
                    ProbingModule.ProbingSequenceTransfer(11);
                    isSeqCheck = true;
                }
                else
                {
                    ProbingModule.ProbingSequenceTransfer(+10);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // 20251028 LJH
        private RelayCommand<object> _VisionSetUpViewCommand;
        public ICommand VisionSetUpViewCommand
        {
            get
            {
                if (null == _VisionSetUpViewCommand) _VisionSetUpViewCommand = new RelayCommand<object>(FunVisionSetUpViewCommand);
                return _VisionSetUpViewCommand;
            }
        }

        private BVisionTestViewModelBase _visionVM;

        private void FunVisionSetUpViewCommand(object obj)
        {
            try
            {
                // 1) VisionTestViewModel 생성(또는 재사용)
                if (_visionVM == null)
                {
                    _visionVM = new BVisionTestViewModelBase();

                    // InitModule은 '해당 VM 인스턴스'에서 호출해야 바인딩 대상 컬렉션이 그 VM에 채워집니다.
                    _visionVM.InitModule();

                    _visionVM.InitViewModel();
                }

                // 2) View 생성 + 정확한 VM을 DataContext로
                var view = new BVisionTestView.BUcVisionTest
                {
                    DataContext = _visionVM
                };

                // 동일한 테마/리소스 적용용 Window 생성
                var win = new Window
                {
                    Title = "Vision Test",
                    Content = view,
                    Owner = Application.Current.MainWindow,
                    Width = 1280,
                    Height = 940,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,

                    // 기존과 동일한 배경 지정 (예: 어두운 테마)
                    Background = System.Windows.Media.Brushes.Black
                };

                // 기존 App.xaml에 있는 테마/리소스 복사
                foreach (var dict in Application.Current.Resources.MergedDictionaries)
                {
                    win.Resources.MergedDictionaries.Add(dict);
                }

                // 창을 도구창처럼 유지하고 여러 번 안 뜨게 관리
                win.ShowInTaskbar = false;
                win.Topmost = false;
                win.Show(); // 모덜리스
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        // end

        private RelayCommand<object> _NormalRewindCommand;
        public ICommand NormalRewindCommand
        {
            get
            {
                if (null == _NormalRewindCommand) _NormalRewindCommand = new RelayCommand<object>(NormalRewindCmdFunc);
                return _NormalRewindCommand;
            }
        }


        private void NormalRewindCmdFunc(object obj)
        {
            try
            {
                if (!isSeqCheck)
                {
                    ProbingSeqBackUp();
                    ProbingModule.ProbingSequenceTransfer(2);
                    isSeqCheck = true;
                }
                else
                {
                    ProbingModule.ProbingSequenceTransfer(1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _NormalForwardCommand;
        public ICommand NormalForwardCommand
        {
            get
            {
                if (null == _NormalForwardCommand) _NormalForwardCommand = new RelayCommand<object>(NormalForwardCmdFunc);
                return _NormalForwardCommand;
            }
        }

        private void NormalForwardCmdFunc(object obj)
        {
            try
            {
                if (!isSeqCheck)
                {
                    ProbingSeqBackUp();
                    ProbingModule.ProbingSequenceTransfer(0);
                    isSeqCheck = true;
                }
                else
                {
                    ProbingModule.ProbingSequenceTransfer(-1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _FastForwardCommand;
        public ICommand FastForwardCommand
        {
            get
            {
                if (null == _FastForwardCommand) _FastForwardCommand = new RelayCommand<object>(FastForwardCmdFunc);
                return _FastForwardCommand;
            }
        }

        private void FastForwardCmdFunc(object obj)
        {
            try
            {
                if (!isSeqCheck)
                {
                    ProbingSeqBackUp();
                    ProbingModule.ProbingSequenceTransfer(-9);
                    isSeqCheck = true;
                }
                else
                {
                    ProbingModule.ProbingSequenceTransfer(-10);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SaveComand;
        public ICommand SaveComand
        {
            get
            {
                if (null == _SaveComand) _SaveComand = new RelayCommand<object>(FuncSaveComand);
                return _SaveComand;
            }
        }
        private void FuncSaveComand(object obj)
        {            
            try
            {
                if (IsSaveComplete == true)
                {
                    IsSaveComplete = false;
                    return;
                }

                if (SaveProgress != 0) return;

                var started = DateTime.Now;
                IsSaving = true;

                ///Save
                LotOPModule.SaveAppItems();
                ///
                
                new DispatcherTimer(
                    TimeSpan.FromMilliseconds(50),
                    DispatcherPriority.Normal,
                    new EventHandler((o, e) =>
                    {
                        var totalDuration = started.AddSeconds(3).Ticks - started.Ticks;
                        var currentProgress = DateTime.Now.Ticks - started.Ticks;
                        var currentProgressPercent = 100.0 / totalDuration * currentProgress;

                        SaveProgress = currentProgressPercent;

                        if (SaveProgress >= 100)
                        {
                            IsSaving = false;
                            SaveProgress = 0;

                            ((DispatcherTimer)o).Stop();
                            IsSaveComplete = false;

                            //AppItems.Add(
                            //    new AppModuleItem(
                            //        "Lot", "O", "Lot Opertaion",
                            //        this.ProberStation().GetPageFromViewModel(new Guid("C0ABDC7F-FBC6-541D-C8F4-A7693BDDB1A2")),
                            //        PageSwitching));
                            //AppItems[4].Icon = "FlipToFront";
                            //AppItems.Add(
                            //    new AppModuleItem(
                            //        "Statistics", "S", "Lot Statistics",
                            //        this.ProberStation().GetPageFromViewModel(new Guid("C0ABDC7F-FBC6-541D-C8F4-A7693BDDB1A2")),
                            //        PageSwitching));
                            //AppItems[5].Icon = "ChartBar";
                            //AppItems.Add(
                            //    new AppModuleItem(
                            //        "WaferAlign", "W", "Wafer Alignment",
                            //        this.ProberStation().GetPageFromViewModel(new Guid("C0ABDC7F-FBC6-541D-C8F4-A7693BDDB1A2")),
                            //        PageSwitching));
                            //AppItems[6].Icon = "BlurRadial";
                            //AppItems.Add(
                            //    new AppModuleItem(
                            //        "OCR", "O", "OCR",
                            //        this.ProberStation().GetPageFromViewModel(new Guid("5142BD1C-E64B-51F5-29CE-50620BED445A")),
                            //        PageSwitching));
                            //AppItems[7].Icon = "BarcodeScan";
                            //AppItems.Add(
                            //    new AppModuleItem(
                            //        "Notice", "N", "Notice",
                            //        this.ProberStation().GetPageFromViewModel(new Guid("C0ABDC7F-FBC6-541D-C8F4-A7693BDDB1A2")),
                            //        PageSwitching));
                            //AppItems[8].Icon = "Bell";
                            //AppItems.Add(
                            //    new AppModuleItem(
                            //        "Language", "L", "Language",
                            //        this.ProberStation().GetPageFromViewModel(new Guid("C0ABDC7F-FBC6-541D-C8F4-A7693BDDB1A2")),
                            //        PageSwitching));
                            //AppItems[9].Icon = "Earth";
                            //AppItems.Add(
                            //    new AppModuleItem(
                            //        "Bin", "B", "Bin",
                            //        this.ProberStation().GetPageFromViewModel(new Guid("C0ABDC7F-FBC6-541D-C8F4-A7693BDDB1A2")),
                            //        PageSwitching));
                            //AppItems[10].Icon = "ChartPie";

                        }

                    }), Dispatcher.CurrentDispatcher);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _FDAlignCommand; // 251013 sebas
        public ICommand FDAlignCommand    // 251013 sebas
        {
            get
            {
                if (null == _FDAlignCommand) _FDAlignCommand = new RelayCommand<object>(FuncFDAlignCommand);
                return _FDAlignCommand;
            }
        }
        private async void FuncFDAlignCommand(object obj)    // 251013 sebas
        {
            try
            {
                LoggerManager.Debug("[Bonder] Framed Wafer Align Button Click");

                var task = Task<IProbeCommandToken>.Run((Func<Task>)(async () =>
                {
                    if (this.MonitoringManager().IsSystemError)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "SYSTEM ERROR";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else if ((this).LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "LotOP RUNNING";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else
                    {
                        (this).CommandManager().SetCommand<IDOFDWAFERALIGN>(this);
                    }
                }));
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _PickCommand; // 251013 sebas
        public ICommand PickCommand    // 251013 sebas
        {
            get
            {
                if (null == _PickCommand) _PickCommand = new RelayCommand<object>(FuncPickComand);
                return _PickCommand;
            }
        }
        private bool _IsPickEnable = true;
        public bool IsPickEnable
        {
            get => _IsPickEnable;
            set { _IsPickEnable = value; RaisePropertyChanged(); }
        }
        private async void FuncPickComand(object obj)    // 251013 sebas
        {
            IsPickEnable = false;
            IsRotationEnable = true;

            try
            {
                LoggerManager.Debug("[Bonder] Pick Button Click");

                var task = Task<IProbeCommandToken>.Run((Func<Task>)(async () =>
                {
                    if (this.MonitoringManager().IsSystemError)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "SYSTEM ERROR";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else if ((this).LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "LotOP RUNNING";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else
                    {
                        (this).CommandManager().SetCommand<IPickCommand>(this);
                    }
                }));

                await task;
            }
            catch
            {

            }
        }
        private RelayCommand<object> _RotationCommand; // 251013 sebas
        public ICommand RotationCommand    // 251013 sebas
        {
            get
            {
                if (null == _RotationCommand) _RotationCommand = new RelayCommand<object>(FuncRotationComand);
                return _RotationCommand;
            }
        }
        private bool _IsRotationEnable;
        public bool IsRotationEnable
        {
            get => _IsRotationEnable;
            set { _IsRotationEnable = value; RaisePropertyChanged(); }
        }

        private async void FuncRotationComand(object obj)    // 251013 sebas
        {
            IsRotationEnable = false;
            IsPlaceEnable = true;

            try
            {
                LoggerManager.Debug("[Bonder] Rotation Button Click");

                var task = Task<IProbeCommandToken>.Run((Func<Task>)(async () =>
                {
                    if (this.MonitoringManager().IsSystemError)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "SYSTEM ERROR";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else if ((this).LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "LotOP RUNNING";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else
                    {
                        (this).CommandManager().SetCommand<IRotationCommand>(this);
                    }
                }));

                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _PlaceCommand; // 251013 sebas
        public ICommand PlaceCommand    // 251013 sebas
        {
            get
            {
                if (null == _PlaceCommand) _PlaceCommand = new RelayCommand<object>(FuncPlaceComand);
                return _PlaceCommand;
            }
        }
        private bool _IsPlaceEnable;
        public bool IsPlaceEnable
        {
            get => _IsPlaceEnable;
            set { _IsPlaceEnable = value; RaisePropertyChanged(); }
        }
        private async void FuncPlaceComand(object obj)    // 251013 sebas
        {
            IsPlaceEnable = false;
            IsPickEnable = true;

            try
            {
                LoggerManager.Debug("[Bonder] Place Button Click");

                var task = Task<IProbeCommandToken>.Run((Func<Task>)(async () =>
                {
                    if (this.MonitoringManager().IsSystemError)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "SYSTEM ERROR";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else if ((this).LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "LotOP RUNNING";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else
                    {
                        (this).CommandManager().SetCommand<IPlaceCommand>(this);
                    }
                }));

                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err); 
            }
        }
        private RelayCommand<object> _BonderStartCommand; // 251013 sebas
        public ICommand BonderStartCommand    // 251013 sebas
        {
            get
            {
                if (null == _BonderStartCommand) _BonderStartCommand = new RelayCommand<object>(FuncBonderStartCommand);
                return _BonderStartCommand;
            }
        }
        private async void FuncBonderStartCommand(object obj)
        {
            try
            {
                LoggerManager.Debug("[Bonder] Bonder Run Start Button Click");

                var task = Task<IProbeCommandToken>.Run((Func<Task>)(async () =>
                {
                    if (this.MonitoringManager().IsSystemError)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "SYSTEM ERROR";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else if ((this).LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "LotOP RUNNING";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else
                    {
                        (this).CommandManager().SetCommand<IBonderStartCommand>(this);
                    }
                }));

                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _BonderEndCommand; // 251013 sebas
        public ICommand BonderEndCommand    // 251013 sebas
        {
            get
            {
                if (null == _BonderEndCommand) _BonderEndCommand = new RelayCommand<object>(FuncBonderEndCommand);
                return _BonderEndCommand;
            }
        }
        private async void FuncBonderEndCommand(object obj)
        {
            try
            {
                LoggerManager.Debug("[Bonder] Bonder Run End Button Click");

                var task = Task<IProbeCommandToken>.Run((Func<Task>)(async () =>
                {
                    if (this.MonitoringManager().IsSystemError)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "SYSTEM ERROR";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else if ((this).LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        string message = "Can not execute the button, because of a System error.";
                        string caption = "LotOP RUNNING";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;

                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else
                    {
                        (this).CommandManager().SetCommand<IBonderEndCommand>(this);
                    }
                }));

                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set { this.MutateVerbose(ref _isSaving, value, ActionRaisePropertyChanged()); }
        }

        private bool _isSaveComplete;
        public bool IsSaveComplete
        {
            get { return _isSaveComplete; }
            private set { this.MutateVerbose(ref _isSaveComplete, value, ActionRaisePropertyChanged()); }
        }

        private double _saveProgress;
        public double SaveProgress
        {
            get { return _saveProgress; }
            private set { this.MutateVerbose(ref _saveProgress, value, ActionRaisePropertyChanged()); }
        }

        //private IndicatorLevel _isWaferOnChuck;
        //public IndicatorLevel isWaferOnChuck
        //{
        //    get { return _isWaferOnChuck; }
        //    private set { this.MutateVerbose(ref _isWaferOnChuck, value, RaisePropertyChanged()); }
        //}

        public ILotOPModule LotModule
        {
            get { return this.LotOPModule(); }

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
        #endregion

        #region Property
        private bool? _ContinueLotFlag;
        public bool? ContinueLotFlag
        {
            get { return _ContinueLotFlag; }
            set
            {
                if (value != _ContinueLotFlag)
                {
                    LotOPModule.LotInfo.ContinueLot = value;
                    _ContinueLotFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private LotModeEnum _LotMode;
        //public LotModeEnum LotMode
        //{
        //    get { return _LotMode; }
        //    set
        //    {
        //        if (value != _LotMode)
        //        {
        //            if(this.LotOPModule == null)
        //            {

        //            }
        //            else
        //            {
        //                this.LotOPModule.LotInfo.LotMode = value;
        //            }
        //            _LotMode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private bool? _AutoLotNameFlag;
        //public bool? AutoLotNameFlag
        //{
        //    get { return _AutoLotNameFlag; }
        //    set
        //    {
        //        if (value != _AutoLotNameFlag)
        //        {
        //            _AutoLotNameFlag = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private bool _IsCanDoProceedProbing;
        public bool IsCanDoProceedProbing
        {
            get { return _IsCanDoProceedProbing; }
            set
            {
                if (value != _IsCanDoProceedProbing)
                {
                    _IsCanDoProceedProbing = value;
                    RaisePropertyChanged();
                }
            }
        }
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

        public PadGroup Pads
        {
            get
            {
                return this.StageSupervisor().WaferObject.GetSubsInfo().Pads;
            }
        }

        public WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
            }
        }

        public MarkObject Mark
        {
            get
            {
                return (MarkObject)this.StageSupervisor().MarkObject;
            }
        }
        public ProbeCard ProbeCard
        {
            get
            {
                return (ProbeCard)this.StageSupervisor().ProbeCardInfo;
            }
        }


        public EnumSubsStatus WaferOnPA
        {

            get
            {
                //ILoaderControllerExtension LoaderControllerExt = this.LoaderController() as ILoaderControllerExtension;
                if (LoaderControllerExt != null)
                {
                    return (EnumSubsStatus)LoaderControllerExt.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus;
                }
                else
                {
                    return EnumSubsStatus.NOT_EXIST;
                }
            }
        }

        public EnumSubsStatus WaferOnArm1
        {

            get
            {
                //ILoaderControllerExtension LoaderControllerExt = this.LoaderController() as ILoaderControllerExtension;
                if(LoaderControllerExt != null)
                {
                    return (EnumSubsStatus)LoaderControllerExt.LoaderInfo.StateMap.ARMModules[0].WaferStatus;
                }
                else
                {
                    return EnumSubsStatus.NOT_EXIST;
                }
            }
        }
        public EnumSubsStatus WaferOnArm2
        {

            get
            {
                //ILoaderControllerExtension LoaderControllerExt = this.LoaderController() as ILoaderControllerExtension;
                if(LoaderControllerExt != null)
                {
                    return (EnumSubsStatus)LoaderControllerExt.LoaderInfo.StateMap.ARMModules[1].WaferStatus;
                }
                else
                {
                    return EnumSubsStatus.NOT_EXIST;
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
                    RaisePropertyChanged(nameof(MotionManager));
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

        private ICoordinateManager _CoordinateManager;
        public ICoordinateManager CoordinateManager
        {
            get { return _CoordinateManager; }
            set
            {
                if (value != _CoordinateManager)
                {
                    _CoordinateManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDisplayPort _LoaderDisplayPort;
        public IDisplayPort LoaderDisplayPort
        {
            get { return _LoaderDisplayPort; }
            set
            {
                if (value != _LoaderDisplayPort)
                {
                    _LoaderDisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ViewNUM;
        public int ViewNUM
        {
            get { return _ViewNUM; }
            set
            {
                if (value != _ViewNUM)
                {
                    _ViewNUM = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsItDisplayed2RateMagnification;
        public bool IsItDisplayed2RateMagnification
        {
            get { return _IsItDisplayed2RateMagnification; }
            set
            {
                if (value != _IsItDisplayed2RateMagnification)
                {
                    _IsItDisplayed2RateMagnification = value;
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

        private INeedleCleanModule _NeedleCleanModule;

        public INeedleCleanModule NeedleCleanModule
        {
            get { return _NeedleCleanModule; }
            set { _NeedleCleanModule = value; }
        }


        private ILotOPModule _LotOPModule;
        public ILotOPModule LotOPModule
        {
            get { return _LotOPModule; }
            set
            {
                if (value != _LotOPModule)
                {
                    _LotOPModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IProberStation _ProberStation;
        public IProberStation ProberStation
        {
            get { return _ProberStation; }
            set
            {
                if (value != _ProberStation)
                {
                    _ProberStation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderController _LoaderController;
        public ILoaderController LoaderController
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

        private ITempController _Temp;
        public ITempController Temp
        {
            get { return _Temp; }
            set
            {
                if (value != _Temp)
                {
                    _Temp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CylinderStateEnum _WaferBridgeCylState;
        public CylinderStateEnum WaferBridgeCylState
        {
            get { return _WaferBridgeCylState; }
            set
            {
                if (value != _WaferBridgeCylState)
                {
                    _WaferBridgeCylState = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private ProbeAxisObject _SCAxis;
        public ProbeAxisObject SCAxis
        {
            get { return _SCAxis; }
            set
            {
                if (value != _SCAxis)
                {
                    _SCAxis = value;
                    RaisePropertyChanged();
                }
            }
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

        private EnumSubsStatus _WaferOnChuck;
        public EnumSubsStatus WaferOnChuck
        {
            get { return _WaferOnChuck; }
            set
            {
                if (value != _WaferOnChuck)
                {
                    _WaferOnChuck = value;
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

        private XyDataSeries<DateTime, double> _dataSeries_CurTemp;
        public XyDataSeries<DateTime, double> dataSeries_CurTemp
        {
            get { return _dataSeries_CurTemp; }
            set
            {
                if (value != _dataSeries_CurTemp)
                {
                    _dataSeries_CurTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private XyDataSeries<DateTime, double> _dataSeries_SetTemp;
        public XyDataSeries<DateTime, double> dataSeries_SetTemp
        {
            get { return _dataSeries_SetTemp; }
            set
            {
                if (value != _dataSeries_SetTemp)
                {
                    _dataSeries_SetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<IRenderableSeriesViewModel> RenderableSeriesViewModels { get; set; }

        private XyDataSeries<double, double> _dataSeries;
        public XyDataSeries<double, double> dataSeries
        {
            get { return _dataSeries; }
            set
            {
                if (value != _dataSeries)
                {
                    _dataSeries = value;
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

        private IZoomObject _ZoomObject;

        public IZoomObject ZoomObject
        {
            get { return _ZoomObject; }
            set { _ZoomObject = value; }
        }
        private string _DeviceName { get; set; }
        public string DeviceName
        {
            get
            {
                return _DeviceName;
            }
            set
            {
                if (value != _DeviceName)
                {
                    _DeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _LotName { get; set; }
        public string LotName
        {
            get
            {
                //_LotName = this.LotOPModule().LotInfo.LotName.Value;
                return _LotName;
            }
            set
            {
                if (value != _LotName)
                {
                    _LotName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _WaferID;
        public string WaferID
        {
            get
            {
                _WaferID = this.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value;
                return _WaferID;
            }
            set
            {
                if (value != _WaferID)
                {
                    _WaferID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte _CSTNum;
        public byte CSTNum
        {
            get
            {
                //_CSTNum = this.ProberStation().LotOP.LotInfo.CassetteNo;
                _CSTNum = this.StageSupervisor().WaferObject.GetSubsInfo().CassetteNo;
                return _CSTNum;
            }
            set
            {
                if (value != _CSTNum)
                {
                    _CSTNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SlotNum;
        public int SlotNum
        {
            get
            {
                _SlotNum = this.StageSupervisor().WaferObject.GetSubsInfo().SlotIndex.Value;
                return _SlotNum;
            }
            set
            {
                if (value != _SlotNum)
                {
                    _SlotNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Overdrive;
        public double Overdrive
        {
            get
            {
                _Overdrive = this.ProbingModule().OverDrive;
                return _Overdrive;
            }
            set
            {
                if (value != _Overdrive)
                {
                    _Overdrive = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _XCoord;
        public long XCoord
        {
            get
            {
                return _XCoord;
            }
            set
            {
                if (value != _XCoord && value != 0)
                {
                    _XCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _YCoord;
        public long YCoord
        {
            get
            {
                return _YCoord;
            }
            set
            {
                if (value != _YCoord && value != 0)
                {
                    _YCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineIndex _ProbingMIndex;
        public MachineIndex ProbingMIndex
        {
            get { return _ProbingMIndex; }
            set
            {
                if (value != _ProbingMIndex)
                {
                    _ProbingMIndex = value;
                    if (_ProbingMIndex.XIndex != 0)
                        XCoord = _ProbingMIndex.XIndex;
                    if (_ProbingMIndex.YIndex != 0)
                        YCoord = _ProbingMIndex.YIndex;
                    RaisePropertyChanged();
                }
            }
        }
        private double _CurYield { get; set; }
        public double CurYield
        {
            get
            {
                //_CurYield = this.ProberStation().LotOP.LotInfo.CurrentLotYeild;
                _CurYield = this.StageSupervisor().WaferObject.GetSubsInfo().Yield;

                return _CurYield;
            }
            set
            {
                if (value != _CurYield)
                {
                    _CurYield = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _totalTestDieCnt;
        public long totalTestDieCnt
        {
            get
            {
                //_totalTestDieCnt = this.ProberStation().LotOP.LotInfo.TotalTestedDieCount;
                _totalTestDieCnt = this.StageSupervisor().WaferObject.GetSubsInfo().TestedDieCount.Value;

                return _totalTestDieCnt;
            }
            set
            {
                if (value != _totalTestDieCnt)
                {
                    _totalTestDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _PassDieCnt;
        public long PassDieCnt
        {
            get
            {
                //_PassDieCnt = this.ProberStation().LotOP.LotInfo.TotalPassedDieCount;
                _PassDieCnt = this.StageSupervisor().WaferObject.GetSubsInfo().PassedDieCount.Value;

                return _PassDieCnt;
            }
            set
            {
                if (value != _PassDieCnt)
                {
                    _PassDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _FailDieCnt;
        public long FailDieCnt
        {
            get
            {
                _FailDieCnt = this.StageSupervisor().WaferObject.GetSubsInfo().FailedDieCount.Value;
                return _FailDieCnt;
            }
            set
            {
                if (value != _FailDieCnt)
                {
                    _FailDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _OperatorName { get; set; }
        public string OperatorName
        {
            get
            {
                return _OperatorName;
            }
            set
            {
                if (value != _OperatorName)
                {
                    _OperatorName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<CassetteTableContents> _CST_StatusInfo;
        public ObservableCollection<CassetteTableContents> CST_StatusInfo
        {
            get { return _CST_StatusInfo; }
            set
            {
                if (value != _CST_StatusInfo)
                {
                    _CST_StatusInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _LotStartDateTime;
        public DateTime LotStartDateTime
        {
            get { return _LotStartDateTime; }
            set
            {
                if (value != _LotStartDateTime)
                {
                    _LotStartDateTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LotStartTime;
        public string LotStartTime
        {
            get { return _LotStartTime; }
            set
            {
                if (value != _LotStartTime)
                {
                    _LotStartTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _RunningTime;
        public string RunningTime
        {
            get { return _RunningTime; }
            set
            {
                if (value != _RunningTime)
                {
                    _RunningTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _HoldingTime;
        public string HoldingTime
        {
            get { return _HoldingTime; }
            set
            {
                if (value != _HoldingTime)
                {
                    _HoldingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private byte _ProbingProcessType;
        //public byte ProbingProcessType
        //{
        //    get
        //    {
        //        _ProbingProcessType = this.ProberStation().LotOP.ProbingProcessType;
        //        return _ProbingProcessType;
        //    }
        //    set
        //    {
        //        if (value != _ProbingProcessType)
        //        {
        //            _ProbingProcessType = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private int _TotalTestDieCount;
        public int TotalTestDieCount
        {
            get
            {
                _TotalTestDieCount = (int)Wafer.ValidDieCount.Value;
                return _TotalTestDieCount;
            }
            set
            {
                if (value != _TotalTestDieCount)
                {
                    _TotalTestDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _StopBeforeAfterFlag;
        public int StopBeforeAfterFlag
        {
            get { return _StopBeforeAfterFlag; }
            set
            {
                if (value != _StopBeforeAfterFlag)
                {
                    _StopBeforeAfterFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StopWaferSetHeader;
        public string StopWaferSetHeader
        {
            get { return _StopWaferSetHeader; }
            set
            {
                if (value != _StopWaferSetHeader)
                {
                    _StopWaferSetHeader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _OpenStopWaferSet;
        public bool OpenStopWaferSet
        {
            get { return _OpenStopWaferSet; }
            set
            {
                if (value != _OpenStopWaferSet)
                {
                    if (StopBeforeAfterFlag == 1 || StopBeforeAfterFlag == 2)
                    {
                        if(value==false)
                        {
                            ExpanderVisibility = Visibility.Collapsed;
                            GroupboxVisibility = Visibility.Visible;
                        }
                        _OpenStopWaferSet = value;
                    }
                    else
                    {
                        ExpanderVisibility = Visibility.Collapsed;
                        GroupboxVisibility = Visibility.Visible;
                        _OpenStopWaferSet = false;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _LotModeVisibility;
        public Visibility LotModeVisibility
        {
            get { return _LotModeVisibility; }
            set
            {
                if (value != _LotModeVisibility)
                {
                    _LotModeVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _StopOptionGroupBoxVisibility;
        public Visibility StopOptionGroupBoxVisibility
        {
            get { return _StopOptionGroupBoxVisibility; }
            set
            {
                if (value != _StopOptionGroupBoxVisibility)
                {
                    _StopOptionGroupBoxVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _ExpanderVisibility;
        public Visibility ExpanderVisibility
        {
            get { return _ExpanderVisibility; }
            set
            {
                if (value != _ExpanderVisibility)
                {
                    _ExpanderVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Visibility _GroupboxVisibility;
        public Visibility GroupboxVisibility
        {
            get { return _GroupboxVisibility; }
            set
            {
                if (value != _GroupboxVisibility)
                {
                    _GroupboxVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public ILoaderControllerExtension LoaderControllerExt { get; set; }

        private HolderModuleInfo[] _CassetteHolderModuleInfos;
        public HolderModuleInfo[] CassetteHolderModuleInfos
        {
            get { return _CassetteHolderModuleInfos; }
            set
            {
                if (value != _CassetteHolderModuleInfos)
                {
                    _CassetteHolderModuleInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ILampManager LampManager { get => this.LampManager(); }

        #region Command

        private RelayCommand _SelectAllDoProceedProbingCommand;
        public ICommand SelectAllDoProceedProbingCommand
        {
            get
            {
                if (null == _SelectAllDoProceedProbingCommand) _SelectAllDoProceedProbingCommand = new RelayCommand(FuncSelectAllDoProceedProbingCommand);
                return _SelectAllDoProceedProbingCommand;
            }
        }

        private void FuncSelectAllDoProceedProbingCommand()
        {
            var Info = LotOPModule.LotInfo as LotInfo;
            foreach (var item in Info.WaferSummarys)
            {
                item.DoProceedProbing = true;
                item.Yield = 99.99;
                item.RetestYield = 99.99;
            }
        }

        private RelayCommand _UnSelectAllDoProceedProbingCommand;
        public ICommand UnSelectAllDoProceedProbingCommand
        {
            get
            {
                if (null == _UnSelectAllDoProceedProbingCommand) _UnSelectAllDoProceedProbingCommand = new RelayCommand(FuncUnSelectAllDoProceedProbingCommand);
                return _UnSelectAllDoProceedProbingCommand;
            }
        }

        private void FuncUnSelectAllDoProceedProbingCommand()
        {
            var Info = LotOPModule.LotInfo as LotInfo;
            foreach (var item in Info.WaferSummarys)
            {
                item.DoProceedProbing = false;
            }
        }

        private RelayCommand _ChangeDoProceedProbingCommand;
        public ICommand ChangeDoProceedProbingCommand
        {
            get
            {
                if (null == _ChangeDoProceedProbingCommand) _ChangeDoProceedProbingCommand = new RelayCommand(FuncChangeDoProceedProbingCommand);
                return _ChangeDoProceedProbingCommand;
            }
        }

        private void FuncChangeDoProceedProbingCommand()
        {
            IsCanDoProceedProbing = !IsCanDoProceedProbing;
        }

        private RelayCommand _OutputOffCommand;
        public ICommand OutputOffCommand
        {
            get
            {
                if (null == _OutputOffCommand) _OutputOffCommand = new RelayCommand(ResetValue);
                return _OutputOffCommand;
            }
        }
        private RelayCommand _OutputOnCommand;
        public ICommand OutputOnCommand
        {
            get
            {
                if (null == _OutputOnCommand) _OutputOnCommand = new RelayCommand(SetValue);
                return _OutputOnCommand;
            }
        }

        public void SetValue()
        {

        }

        public void ResetValue()
        {

        }

        private RelayCommand<object> _LotRestartCommand;
        public ICommand LotRestartCommand
        {
            get
            {
                if (null == _LotRestartCommand) _LotRestartCommand = new RelayCommand<object>(FuncLotRestartCommand);
                return _LotRestartCommand;
            }
        }

        private async void FuncLotRestartCommand(object obj)
        {
            try
            {

                EnumMessageDialogResult ret;

                ret = await this.MetroDialogManager().ShowMessageDialog("LOT RESTART", "If you restart, Probing result will be lost.", EnumMessageStyle.AffirmativeAndNegative);

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    ProbingModule.ProbingRestart();
                    this.CommandManager().SetCommand<ILotOpResume>(this);
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

        private RelayCommand<object> _SwitchPage;
        public ICommand SwitchPage
        {
            get
            {
                if (null == _SwitchPage) _SwitchPage = new RelayCommand<object>(PageSwitching);
                return _SwitchPage;
            }
        }

        private void PageSwitching(object obj)
        {
            try
            {

                //var dd=obj.ToString();

                this.ViewModelManager().ViewTransitionAsync(new Guid(obj.ToString()));


                //PropertyInfo prop = obj.GetType().GetProperty("PageGUID");

                //if (prop != null)
                //{
                //    Object val = prop.GetValue(obj, null);
                //    this.ProberStation().MovePageTo((Guid)val);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _AddTempData;
        public ICommand AddTempData
        {
            get
            {
                if (null == _AddTempData) _AddTempData = new RelayCommand<object>(AddTempDataFunc);
                return _AddTempData;
            }
        }

        private void AddTempDataFunc(object obj)
        {
            try
            {
                double CurTemp = 30;

                Random rand = new Random();

                double diff = rand.NextDouble() % 1;

                dataSeries.Append(dataSeries.Count, CurTemp + diff);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ViewSwipCommand;
        public ICommand ViewSwipCommand
        {
            get
            {
                if (null == _ViewSwipCommand) _ViewSwipCommand = new RelayCommand<object>(ViewSwipFunc);
                return _ViewSwipCommand;
            }
        }

        private void ViewSwipFunc(object parameter)
        {
            try
            {
                this.LotOPModule().ViewSwip();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _PlusCommand;
        public ICommand PlusCommand
        {
            get
            {
                if (null == _PlusCommand) _PlusCommand = new RelayCommand(Plus);
                return _PlusCommand;
            }
        }
        private void Plus()
        {
            Wafer.ZoomLevel--;
            //ZoomObject.ZoomLevel--;
        }

        private RelayCommand _MinusCommand;
        public ICommand MinusCommand
        {
            get
            {
                if (null == _MinusCommand) _MinusCommand = new RelayCommand(Minus);
                return _MinusCommand;
            }
        }
        private void Minus()
        {
            Wafer.ZoomLevel++;
            //ZoomObject.ZoomLevel++;
        }

        private RelayCommand _PinAlignmentCommand;
        public ICommand PinAlignmentCommand
        {
            get
            {
                if (null == _PinAlignmentCommand) _PinAlignmentCommand = new RelayCommand(PinAlignmentCommandFunc);
                return _PinAlignmentCommand;
            }
        }
        private void PinAlignmentCommandFunc()
        {
            FlyoutIsOpen = false;
        }

        private RelayCommand<CUI.Button> _OperatorPageSwitchCommand;
        public ICommand OperatorPageSwitchCommand
        {
            get
            {
                if (null == _OperatorPageSwitchCommand) _OperatorPageSwitchCommand = new RelayCommand<CUI.Button>(FuncOperatorPageSwitchCommand);
                return _OperatorPageSwitchCommand;
            }
        }

        private void FuncOperatorPageSwitchCommand(CUI.Button cuiparam)
        {
            try
            {
                this.ViewModelManager().ChangeFlyOutControlStatus(true);

                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<CUI.Button> _ManualContactPageSwitchCommand;
        public ICommand ManualContactPageSwitchCommand
        {
            get
            {
                if (null == _ManualContactPageSwitchCommand) _ManualContactPageSwitchCommand = new AsyncCommand<CUI.Button>(ManualContactPageSwitchCommandFunc);
                return _ManualContactPageSwitchCommand;
            }
        }

        private async Task ManualContactPageSwitchCommandFunc(CUI.Button cuiparam)
        {
            try
            {
                bool WaferAligned = false;
                bool PinAligned = false;
                string waferdatavalid = string.Empty;
                string pindatavalid = string.Empty;

                Element<AlignStateEnum> WaferAlignState = this.StageSupervisor().GetAlignState(AlignTypeEnum.Wafer);

                if (WaferAlignState.Value == AlignStateEnum.DONE)
                {
                    WaferAligned = true;
                    waferdatavalid = "is enough.";
                }
                else
                {
                    waferdatavalid = "is not enough.";
                }

                Element<AlignStateEnum> PinAlignState = this.StageSupervisor().GetAlignState(AlignTypeEnum.Pin);

                if (PinAlignState.Value == AlignStateEnum.DONE)
                {
                    PinAligned = true;
                    pindatavalid = "is enough.";
                }
                else
                {
                    pindatavalid = "is not enough.";
                }

                if ((WaferAligned == true) && (PinAligned == true))
                {
                    this.ViewModelManager().ChangeFlyOutControlStatus(true);

                    Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                    this.ViewModelManager().ViewTransitionAsync(ViewGUID);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog(
                        "[Information]", $"There is not enough data required.\nWafer Align data {waferdatavalid}\nPin Align data {pindatavalid}", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private RelayCommand<object> _SetStopSetting;
        //public ICommand SetStopSetting
        //{
        //    get
        //    {
        //        if (null == _SetStopSetting) _SetStopSetting = new RelayCommand<object>(FuncSetStopSetting);
        //        return _SetStopSetting;
        //    }
        //}

        //public void FuncSetStopSetting(object noparam)
        //{
        //    if (StopBeforeAfterFlag == 1)
        //    {
        //        for (int i = 0; i < 25; i++)
        //        {
        //            StopBeforeWaferSet[i] = tmpStopWaferSet[i];
        //        }
        //    }
        //    else if (StopBeforeAfterFlag == 2)
        //    {
        //        for (int i = 0; i < 25; i++)
        //        {
        //            StopAfterWaferSet[i] = tmpStopWaferSet[i];
        //        }
        //    }
        //}

        private RelayCommand<object> _cmdCloseStopWaferSet;
        public ICommand cmdCloseStopWaferSet
        {
            get
            {
                if (null == _cmdCloseStopWaferSet) _cmdCloseStopWaferSet = new RelayCommand<object>(FuncCloseStopWaferSet);
                return _cmdCloseStopWaferSet;
            }
        }

        public void FuncCloseStopWaferSet(object noparam)
        {
            OpenStopWaferSet = false;
        }

        private RelayCommand<CUI.Button> _LotInfoCommand;
        public ICommand LotInfoCommand
        {
            get
            {
                if (null == _LotInfoCommand) _LotInfoCommand = new RelayCommand<CUI.Button>(FuncLotInfoCommand);
                return _LotInfoCommand;
            }
        }

        public void FuncLotInfoCommand(CUI.Button cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);

                this.ViewModelManager().FlyOutLotInformation(ViewGUID);
                //this.ViewModelManager().ChangeFlyOutControlStatus();
                //FlyoutIsOpen = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _OCRManualInputCommand;
        public ICommand OCRManualInputCommand
        {
            get
            {
                if (null == _OCRManualInputCommand) _OCRManualInputCommand = new RelayCommand(OCRManualInputCommandFunc);
                return _OCRManualInputCommand;
            }
        }
        public void OCRManualInputCommandFunc()
        {
            try
            {
                CognexManualInput.AsyncShow(this.GetContainer());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _LoaderTestCommand;
        public ICommand LoaderTestCommand
        {
            get
            {
                if (null == _LoaderTestCommand) _LoaderTestCommand = new RelayCommand(LoaderTestCommandFunc);
                return _LoaderTestCommand;
            }
        }
        public void LoaderTestCommandFunc()
        {
            try
            {
                LoaderTestWindow.AsyncShow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _OpenFlyout;
        public ICommand OpenFlyout
        {
            get
            {
                if (null == _OpenFlyout) _OpenFlyout = new RelayCommand<object>(FuncOpenFlyout);
                return _OpenFlyout;
            }
        }

        public void FuncOpenFlyout(object noparam)
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

        private RelayCommand<object> _CloseFlyout;
        public ICommand CloseFlyout
        {
            get
            {
                if (null == _CloseFlyout) _CloseFlyout = new RelayCommand<object>(FuncCloseFlyout);
                return _CloseFlyout;
            }
        }
        public void FuncCloseFlyout(object noparam)
        {
            try
            {
                this.ViewModelManager().ChangeFlyOutControlStatus(false);
                //FlyoutIsOpen = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _cmdOpenStopWaferSet;
        public ICommand cmdOpenStopWaferSet
        {
            get
            {
                if (null == _cmdOpenStopWaferSet) _cmdOpenStopWaferSet = new RelayCommand<object>(FuncOpenStopWaferSet);
                return _cmdOpenStopWaferSet;
            }
        }

        public void FuncOpenStopWaferSet(object param)
        {
            try
            {
                ExpanderVisibility = Visibility.Visible;
                GroupboxVisibility = Visibility.Collapsed;
                if (param.ToString() == "1")        // Stop before probe
                {
                    StopBeforeAfterFlag = Convert.ToInt32(param);
                    StopWaferSetHeader = "Stop Before Probe Interval";

                    if(LotOPModule.LotDeviceParam.OperatorStopOption.StopBeforeProbingFlag.Value != null)
                    {
                        for (int i = 0; i < 25; i++)
                        {
                            WaferSelectBtn[i].isChecked = LotOPModule.LotDeviceParam.OperatorStopOption.StopBeforeProbingFlag.Value[i];
                        }
                    }
                }
                else if (param.ToString() == "2")   // Stop after probe
                {
                    StopBeforeAfterFlag = Convert.ToInt32(param);
                    StopWaferSetHeader = "Stop After Probe Interval";

                    if(LotOPModule.LotDeviceParam.OperatorStopOption.StopAfterProbingFlag.Value != null)
                    {
                        for (int i = 0; i < 25; i++)
                        {
                            WaferSelectBtn[i].isChecked = LotOPModule.LotDeviceParam.OperatorStopOption.StopAfterProbingFlag.Value[i];
                        }
                    }
                }
                OpenStopWaferSet = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditLotNameCommand;
        public ICommand EditLotNameCommand
        {
            get
            {
                if (null == _EditLotNameCommand) _EditLotNameCommand = new RelayCommand(EditLotName);
                return _EditLotNameCommand;
            }
        }

        private void EditLotName()
        {
            try
            {
                string retVal;
                retVal = VirtualKeyboard.Show(this.LotOPModule().LotInfo.LotName.Value, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET);
                this.LotOPModule().LotInfo.SetLotName(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EditOPNameCommand;
        public ICommand EditOPNameCommand
        {
            get
            {
                if (null == _EditOPNameCommand) _EditOPNameCommand = new RelayCommand(EditOPName);
                return _EditOPNameCommand;
            }
        }

        private void EditOPName()
        {
            try
            {
                string retVal;

                //retVal = VirtualKeyboard.Show(this.LotOPModule().LotInfo.OperatorName.Value);
                //this.LotOPModule().LotInfo.OperatorName.Value = retVal;
                retVal = VirtualKeyboard.Show(LotModule.LotInfo.OperatorID.Value, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET);
                LotModule.LotInfo.OperatorID.Value = retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<CUI.Button> _StartLotOP;
        public ICommand StartLotOP
        {
            get
            {

                if (null == _StartLotOP) _StartLotOP = new RelayCommand<CUI.Button>(FuncStartLotOP);
                return _StartLotOP;
            }
        }

        private async void FuncStartLotOP(CUI.Button cuiparam)
        {
            //this.ViewModelManager().AllUnLock();
            try
            {

                var task = Task<IProbeCommandToken>.Run((Func<Task>)(async () =>
                {
                    if (this.MonitoringManager().IsSystemError)
                    {
                        string message = "Can not execute the Lot,because of a System error.";
                        string caption = "SYSTEM ERROR";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Question;
                        // MessageBox.Show(message, caption, buttons, icon)==MessageBBO
                        if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                        {
                        }
                    }
                    else if ((this).LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        isSeqCheck = false;

                        //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Lot pausing");
                        await this.MetroDialogManager().ShowWaitCancelDialog(this.LotOPModule().GetHashCode().ToString(), "Lot pausing");

                        //(this).ViewModelManager().UnLockViewControl((this).LotOPModule().GetHashCode());

                        (this).CommandManager().SetCommand<ILotOpPause>(this);
                    }
                    else if ((this).LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                    {
                        if (isSeqCheck)
                        {

                            EnumMessageDialogResult ret;

                            ret = await this.MetroDialogManager().ShowMessageDialog("Probing sequence has changed", "Probing sequence has changed. Do you want to proceed like this?", EnumMessageStyle.AffirmativeAndNegative);

                            if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                this.StageSupervisor().WaferObject.ResetWaferData(ProbingModule.ProbingLastMIndex);
                            }
                            else
                            {
                                ProbingModule.ProbingSequenceTransfer(CurrentMI, probingRemainCnt);
                            }

                        }
                        isSeqCheck = false;


                        (this).CommandManager().SetCommand<ILotOpResume>(this);
                    }
                    else
                    {
                        isSeqCheck = false;

                        (this).CommandManager().SetCommand<ILotOpStart>(this);
                    }
                }));
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }

        }

        private AsyncCommand _EndLotOPCommand;
        public ICommand EndLotOPCommand
        {
            get
            {
                if (null == _EndLotOPCommand) _EndLotOPCommand = new AsyncCommand(EndLotOP);
                return _EndLotOPCommand;
            }
        }

        private async Task EndLotOP()
        {
            try
            {
                //this.ViewModelManager().UnLockViewControl(this.LotOPModule().GetHashCode());

                var task = Task<IProbeCommandToken>.Run(async () =>
                {
                    // TODO : Loader 에러 상태일 때 어떻게 해야되는지?

                    if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                    {
                        if (this.MonitoringManager().IsSystemError)
                        {
                            string message = "Can not execute the Lot,because of a System error.";
                            string caption = "SYSTEM ERROR";
                            MessageBoxButton buttons = MessageBoxButton.OK;
                            MessageBoxImage icon = MessageBoxImage.Question;
                            // MessageBox.Show(message, caption, buttons, icon)==MessageBBO
                            if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                            {
                            }
                        }
                        //else if (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.ERROR || this.LoaderController().ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                        //{
                        //    string message = "When You Finish the Lot, You Must Initialize The Loader First. Do you Want To Initialize?";
                        //    string caption = "Loader Initialize";
                        //    MessageBoxButton buttons = MessageBoxButton.YesNo;
                        //    MessageBoxImage icon = MessageBoxImage.Question;
                        //    if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                        //    {
                        //        EventCodeEnum retVal = this.LoaderController().LoaderSystemInit();
                        //        if (retVal == EventCodeEnum.NONE)
                        //        {
                        //        }
                        //    }
                        //    else
                        //    {
                        //    }
                        //}
                        else
                        {

                            EnumMessageDialogResult ret;

                            ret = await this.MetroDialogManager().ShowMessageDialog("LOT END", "Do you want to end LOT?", EnumMessageStyle.AffirmativeAndNegative);

                            if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Lot end");
                                await this.MetroDialogManager().ShowWaitCancelDialog(this.LotOPModule().GetHashCode().ToString(), "Lot end");
                                
                                this.LotOPModule().LotInfo.ContinueLot = false;
                                this.CommandManager().SetCommand<ILotOpEnd>(this);
                            }
                            else
                            {
                            }

                        }

                    }
                });
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// ///////////////////////////////////////배율
        /// </summary>
        /// 

        public IViewModelManager ViewModelManager { get; set; }

        public void Viewx2() // 2x view
        {
            try
            {
                IsItDisplayed2RateMagnification = !IsItDisplayed2RateMagnification;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CWVIEW() //CW
        {
            try
            {
                ViewNUM = ((Enum.GetNames(typeof(CameraViewPoint)).Length) + (--ViewNUM)) % Enum.GetNames(typeof(CameraViewPoint)).Length;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CenterView() //FRONT
        {
            try
            {
                ViewNUM = 0;
                IsItDisplayed2RateMagnification = false;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CCWView() // CCW
        {
            try
            {
                ViewNUM = Math.Abs(++ViewNUM % Enum.GetNames(typeof(CameraViewPoint)).Length);
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _X2ViewChangeCommand;
        public RelayCommand X2ViewChangeCommand
        {
            get
            {
                if (null == _X2ViewChangeCommand) _X2ViewChangeCommand = new RelayCommand(Viewx2);
                return _X2ViewChangeCommand;
            }
        }


        private RelayCommand _CWViewChangeCommand;
        public ICommand CWViewChangeCommand
        {
            get
            {
                if (null == _CWViewChangeCommand) _CWViewChangeCommand = new RelayCommand(CWVIEW);
                return _CWViewChangeCommand;
            }
        }


        private RelayCommand _CenterViewChangeCommand;
        public ICommand CenterViewChangeCommand
        {
            get
            {
                if (null == _CenterViewChangeCommand) _CenterViewChangeCommand = new RelayCommand(CenterView);
                return _CenterViewChangeCommand;
            }
        }


        private RelayCommand _CCWViewChangeCommand;
        public ICommand CCWViewChangeCommand
        {
            get
            {
                if (null == _CCWViewChangeCommand) _CCWViewChangeCommand = new RelayCommand(CCWView);
                return _CCWViewChangeCommand;
            }
        }


        #endregion
        private IStage3DModel _Stage3DModel;
        public IStage3DModel Stage3DModel
        {
            get { return _Stage3DModel; }
            set
            {
                if (value != _Stage3DModel)
                {
                    _Stage3DModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Title
        {
            get { return this.FileManager().DevFolder; }
        }

        //private IStage3DModel ThreePodStage { get; set; }

        //private IStage3DModel _Stage3DModel;
        //public IStage3DModel Stage3DModel
        //{
        //    get { return _Stage3DModel; }
        //    set
        //    {
        //        _Stage3DModel = ThreePodStage;
        //        RaisePropertyChanged();

        //    }
        //}


        //test

        private string _CameraPosition = "1878.06313394745,785.84173995825,1255.71754058166";
        public string CameraPosition
        {
            get { return _CameraPosition; }
            set
            {
                if (value != _CameraPosition)
                {
                    _CameraPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CameraLookDirection = "-0.756034605754391,-0.326100645337205,-0.567512153184811";
        public string CameraLookDirection
        {
            get { return _CameraLookDirection; }
            set
            {
                if (value != _CameraLookDirection)
                {
                    _CameraLookDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CameraUpDirection = "-0.260799989516842,0.945335056533216,-0.195767710201035";
        public string CameraUpDirection
        {
            get { return _CameraUpDirection; }
            set
            {
                if (value != _CameraUpDirection)
                {
                    _CameraUpDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void UpdateApplications()
        {
            try
            {
                foreach (var item in AppItems)
                {
                    item.SwitchTo += PageSwitching;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AlignStateEnum _WaferAlignState;
        public AlignStateEnum WaferAlignState
        {
            get { return _WaferAlignState; }
            set
            {
                if (value != _WaferAlignState)
                {
                    _WaferAlignState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AlignStateEnum _PinAlignState;
        public AlignStateEnum PinAlignState
        {
            get { return _PinAlignState; }
            set
            {
                if (value != _PinAlignState)
                {
                    _PinAlignState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AlignStateEnum _MarkAlignState;
        public AlignStateEnum MarkAlignState
        {
            get { return _MarkAlignState; }
            set
            {
                if (value != _MarkAlignState)
                {
                    _MarkAlignState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _PadCount;
        public int PadCount
        {
            get { return _PadCount; }
            set
            {
                if (value != _PadCount)
                {
                    _PadCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private EnumSubsStatus _WaferStatusOnChuck;
        //public EnumSubsStatus WaferStatusOnChuck
        //{
        //    get { return _WaferStatusOnChuck; }
        //    set
        //    {
        //        if (value != _WaferStatusOnChuck)
        //        {
        //            _WaferStatusOnChuck = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //Thread updateThread;
        //bool bStopUpdateThread;
        private ObservableCollection<ButtonDescriptor> Wafers;
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    LoaderControllerExt = this.LoaderController() as ILoaderControllerExtension;
                    
                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        CassetteHolderModuleInfos = LoaderControllerExt?.LoaderInfo.StateMap.CassetteModules[0].SlotModules;
                    }
                    else
                    {
                        CassetteHolderModuleInfos = null;
                    }

                    //CassetteHolderModuleInfos

                    //var waferaligner = this.WaferAligner();
                    //var lotModule = this.LotOPModule();

                    //_OPName = this.LotOPModule().LotInfo.OperatorName.Value;
                    //_DeviceName = this.LotOPModule().LotInfo.DeviceName.Value;
                    //_OperatorName = this.LotOPModule().LotInfo.OperatorName.Value;
                    ViewModelManager = this.ViewModelManager();
                    _DeviceName = this.StageSupervisor().WaferObject.GetSubsInfo().DeviceName.Value;

                    CenterView();
                    ViewNUM = 0;
                    IsItDisplayed2RateMagnification = false;
                    //_StopAfterScanCST = this.LotOPModule().LotInfo.StopAfterScanCST;
                    //_StopAfterWaferLoad = this.LotOPModule().LotInfo.StopAfterWaferLoad;
                    //_StopBeforeProbing = this.LotOPModule().LotInfo.StopBeforeProbing;
                    //_StopAfterProbing = this.LotOPModule().LotInfo.StopAfterProbing;
                    //_StopBeforeRetest = this.LotOPModule().LotInfo.StopBeforeRetest;
                    //_StopBeforeWaferSet = this.LotOPModule().LotInfo.StopBeforeWaferSet;
                    //_StopAfterWaferSet = this.LotOPModule().LotInfo.StopAfterWaferSet;
                    _ContinueLotFlag = this.LotOPModule().LotInfo.ContinueLot;
                    //LotMode = this.LotOPModule().LotInfo.LotMode;

                    MonitoringManager = this.MonitoringManager();
                    //WaferObject = this.StageSupervisor().WaferObject;
                    ProbingModule = this.ProbingModule();
                    LotOPModule = this.LotOPModule();
                    LoaderController = this.LoaderController();
                    CoordinateManager = this.CoordinateManager();
                    StageSupervisor = this.StageSupervisor();
                    MotionManager = this.MotionManager();

                    Motion = this.MotionManager();
                    Temp = this.TempController();

                    if(Motion != null)
                    {
                        U1 = Motion.GetAxis(EnumAxisConstants.U1);
                        U2 = Motion.GetAxis(EnumAxisConstants.U2);
                        LZ = Motion.GetAxis(EnumAxisConstants.A);
                        LW = Motion.GetAxis(EnumAxisConstants.W);
                        XAxis = Motion.GetAxis(EnumAxisConstants.X);
                        YAxis = Motion.GetAxis(EnumAxisConstants.Y);
                        ZAxis = Motion.GetAxis(EnumAxisConstants.Z);
                        MNCAxis = Motion.GetAxis(EnumAxisConstants.NC);
                        PZAxis = Motion.GetAxis(EnumAxisConstants.PZ);
                        SCAxis = Motion.GetAxis(EnumAxisConstants.SC);
                    }

                    ThreeLegHeight = 0.0d;
                    FoupCoverHeight = -380d;
                    FoupCoverPos = -40d;
                    ZUpState = this.ProbingModule().ProbingStateEnum;

                    if (this.StageSupervisor().WaferObject != null)
                    {
                        WaferOnChuck = this.StageSupervisor().WaferObject.WaferStatus;
                    }

                    //ILoaderControllerExtension LoaderControllerExt = this.LoaderController() as ILoaderControllerExtension;

                    //if (LoaderControllerExt.LoaderInfo != null)
                    //{
                    //    //WaferOnArm1 = LoaderControllerExt.LoaderInfo.StateMap.ARMModules[0].WaferStatus;
                    //    //WaferOnArm2 = LoaderControllerExt.LoaderInfo.StateMap.ARMModules[1].WaferStatus;
                    //    //WaferOnPA = LoaderControllerExt.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus;
                    //}

                    IsWaferBridgeExtended = this.IOManager()?.IO.Outputs.DOWAFERMIDDLE;


                    //WaferBridgeCylState = StageCylinderType.MoveWaferCam.State;
                    //StageCylinderType.MoveWaferCam.State
                    //this.ProberStation()._StageSuperVisor.IOState.IO.Outputs.DOTHREELEGUP.Value
                    //updateThread = new Thread(new ThreadStart(UpdateProc));
                    //updateThread.Start();
                    //this.ProberStation()._StageSuperVisor.IOState.IO.Outputs.DOSCAN_SENSOR_OUT.Value
                    //IFoupOPModule foup = container.Resolve<IFoupOPModule>();
                    //foup.
                    //this.ProberStation().this.LoaderController().LoaderInfo.StateMap.ChuckModules[0].Substrate.OriginHolder.Index
                    if (dataSeries_CurTemp == null)
                    {
                        dataSeries_CurTemp = Temp?.dataSeries_CurTemp;
                    }

                    if (dataSeries_SetTemp == null)
                    {
                        dataSeries_SetTemp = Temp?.dataSeries_SetTemp;
                    }

                    if (RenderableSeriesViewModels == null)
                    {
                        RenderableSeriesViewModels = new ObservableCollection<IRenderableSeriesViewModel>()
                            {
                                new LineRenderableSeriesViewModel {DataSeries = dataSeries_CurTemp, StyleKey = "LineSeriesStyle", Stroke = Colors.Red},
                                new LineRenderableSeriesViewModel {DataSeries = dataSeries_SetTemp, StyleKey = "LineSeriesStyle", Stroke = Colors.Green},
                            };
                    }

                    Stage3DModel = this.ViewModelManager().Stage3DModel;

                    IsCanDoProceedProbing = false;

                    Initialized = true;

                    retval = EventCodeEnum.NONE;

                    Wafers = new ObservableCollection<ButtonDescriptor>();

                    for (int i = 0; i < 25; i++)
                    {
                        Wafers.Add(new ButtonDescriptor());
                    }

                    WaferSelectBtn = new ObservableCollection<ButtonDescriptor>(Wafers);

                    for (int i = 0; i < WaferSelectBtn.Count; i++)
                    {
                        WaferSelectBtn[i].Command = new RelayCommand<Object>(SelectWaferIndexCommand);
                        WaferSelectBtn[i].CommandParameter = i;
                        // WaferSelectBtn[i].isChecked = PMIInfo.GetWaferTemplate(i).PMIEnable.Value;
                    }

                    SelectAllBtn.Command = new RelayCommand<Object>(SetAllWaferUsingCommand);
                    SelectAllBtn.CommandParameter = true;

                    ClearAllBtn.Command = new RelayCommand<Object>(SetAllWaferUsingCommand);
                    ClearAllBtn.CommandParameter = false;

                    FuncSelectAllDoProceedProbingCommand();

                    ExpanderVisibility = Visibility.Collapsed;
                    GroupboxVisibility = Visibility.Visible;
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
        public void SelectWaferIndexCommand(Object index)
        {
            try
            {
                if (index is int)
                {
                    if (StopBeforeAfterFlag == 1)
                    {
                        LotOPModule.LotDeviceParam.OperatorStopOption.StopBeforeProbingFlag.Value[(int)index] = WaferSelectBtn[(int)index].isChecked;
                    }
                    else if (StopBeforeAfterFlag == 2)
                    {
                        LotOPModule.LotDeviceParam.OperatorStopOption.StopAfterProbingFlag.Value[(int)index] = WaferSelectBtn[(int)index].isChecked;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [SelectWaferIndexCommand()] : {err}");
                LoggerManager.Exception(err);
            }
        }
        public void SetAllWaferUsingCommand(Object Using)
        {
            try
            {
                bool flag = (bool)Using;

                for (int i = 0; i < 25; i++)
                {
                    if (StopBeforeAfterFlag == 1)
                    {
                        LotOPModule.LotDeviceParam.OperatorStopOption.StopBeforeProbingFlag.Value[i] = flag;
                    }
                    else
                    {
                        LotOPModule.LotDeviceParam.OperatorStopOption.StopAfterProbingFlag.Value[i] = flag;
                    }
                    WaferSelectBtn[i].isChecked = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [SetAllWaferUsingCommand()] : {err}");
                LoggerManager.Exception(err);
            }
        }
        //private void UpdateProc()
        //{
        //    while (bStopUpdateThread == false)
        //    {
        //        if (this.ProberStation()._StageSuperVisor.WaferObject.GetStatus() == EnumWaferStatus.EXIST )
        //        {
        //            WaferOnChuck = true;
        //        }
        //        else
        //        {
        //            WaferOnChuck = false;  
        //        }

        //        if(this.ProberStation().Probing.ProbingModuleState.GetState() == EnumProbingState.ZUP)
        //        {
        //            ZUpState = true;
        //        }
        //        else
        //        {
        //            ZUpState = false;
        //        }

        //        if(this.ProberStation().this.LoaderController().LoaderInfo.StateMap.ARMModules[0].WaferStatus == EnumWaferStatus.EXIST)
        //        {
        //            WaferOnArm1 = true;
        //        }
        //        else
        //        {
        //            WaferOnArm1 = false;
        //        }

        //        if (this.ProberStation().this.LoaderController().LoaderInfo.StateMap.ARMModules[1].WaferStatus == EnumWaferStatus.EXIST)
        //        {
        //            WaferOnArm2 = true;
        //        }
        //        else
        //        {
        //            WaferOnArm2 = false;
        //        }

        //        if (this.ProberStation().this.LoaderController().LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumWaferStatus.EXIST)
        //        {
        //            WaferOnPA = true;
        //        }
        //        else
        //        {
        //            WaferOnPA = false;
        //        }

        //        if(this.ProberStation()._StageSuperVisor.IOState.IO.Inputs.DITHREELEGUP_0.Value == true
        //        || this.ProberStation()._StageSuperVisor.IOState.IO.Inputs.DITHREELEGUP_1.Value == true)
        //        {
        //            ThreeLegHeight = 8;
        //        }
        //        else
        //        {
        //            ThreeLegHeight = -0.05;
        //        }
        //        Thread.Sleep(100);
        //    }

        //}

        public EventCodeEnum InitPage(object parameter = null)
        {
            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
                var lotsysparam = this.LotOPModule().AppItems_IParam;

                AppItems = (lotsysparam as AppModuleItems).Items;
                ZoomObject = Wafer;
                UpdateApplications();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                StopOptionGroupBoxVisibility = Visibility.Collapsed;
                LotModeVisibility = Visibility.Collapsed;


                //WaferAlignState = this.GetParam_Wafer().AlignState.Value;
                //PinAlignState = this.GetParam_ProbeCard().AlignState.Value;
                //MarkAlignState = this.StageSupervisor().MarkObject.AlignState.Value;

                PadCount = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Count;

                //WaferStatusOnChuck = this.GetParam_Wafer().WaferStatus;

                Wafer.MapViewControlMode = MapViewMode.BinMode;
                Wafer.DPMarkerVisible = Visibility.Hidden;

                if (this.PMIModule().GetPMIEnableParam())
                {
                    Wafer.IsMapViewShowPMIEnable = true;
                    Wafer.IsMapViewShowPMITable = false;
                }
                else
                {
                    Wafer.IsMapViewShowPMIEnable = false;
                    Wafer.IsMapViewShowPMITable = false;
                }

                LotOPModule.SetLotViewDisplayChannel();
                CenterView();
                ViewNUM = 0;
                IsItDisplayed2RateMagnification = false;
                LotOPModule.MapScreenToLotScreen();
                //

                LotOPModule.ViewTargetUpdate();

                //SettingLockable
                //this.ViewModelManager().MenuLockables.Utility_TaskManagement = false;
                //this.ViewModelManager().MenuLockables.System_SettingChangeLockable = false;
                //this.ViewModelManager().MenuLockables.Sytem_InitializationLockable = false;
                //this.ViewModelManager().MenuLockables.Loader_InitializationLockable = false;

                //Stage3DModel = null;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Stage3DModel = null;
                    Stage3DModel = this.ViewModelManager().Stage3DModel;
                    this.ViewModelManager().MapViewControl.WaferObject = StageSupervisor.WaferObject;
                    this.ViewModelManager().MapViewControl.CoordinateManager = this.CoordinateManager();
                    this.ViewModelManager().MapViewControl.EnalbeClickToMove = false;
                    this.ViewModelManager().MapViewControl.IsCrossLineVisible = false;

                    this.ViewModelManager().MapViewControlFD.WaferObject = StageSupervisor.WaferObject;
                    this.ViewModelManager().MapViewControlFD.CoordinateManager = this.CoordinateManager();
                    this.ViewModelManager().MapViewControlFD.EnalbeClickToMove = false;
                    this.ViewModelManager().MapViewControlFD.IsCrossLineVisible = false;


                    Binding bindunderdut = new Binding
                    {
                        Path = new System.Windows.PropertyPath("ProbingModule.ProbingProcessStatus.UnderDutDevs"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((MapView.MapViewControl)this.ViewModelManager().MapViewControl,
                        MapView.MapViewControl.UnderDutDicesProperty, bindunderdut);

                    Binding bindcurxindex = new Binding
                    {
                        Path = new System.Windows.PropertyPath("ProbingModule.ProbingMXIndex"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((MapView.MapViewControl)this.ViewModelManager().MapViewControl,
                        MapView.MapViewControl.CursorXIndexProperty, bindcurxindex);

                    Binding bindcuryindex = new Binding
                    {
                        Path = new System.Windows.PropertyPath("ProbingModule.ProbingMYIndex"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((MapView.MapViewControl)this.ViewModelManager().MapViewControl,
                        MapView.MapViewControl.CursorYIndexProperty, bindcuryindex);

                    Binding bindrendermode = new Binding
                    {
                        Path = new System.Windows.PropertyPath("StageSupervisor.WaferObject.MapViewControlMode"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((MapView.MapViewControl)this.ViewModelManager().MapViewControl,
                        MapView.MapViewControl.RenderModeProperty, bindrendermode);


                    // ===========================
                    // MapViewControlFD 동일하게 바인딩 추가
                    // ===========================
                    Binding bindunderdut1 = new Binding
                    {
                        Path = new System.Windows.PropertyPath("ProbingModule.ProbingProcessStatus.UnderDutDevs"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((MapView.MapViewControl)this.ViewModelManager().MapViewControlFD,
                        MapView.MapViewControl.UnderDutDicesProperty, bindunderdut1);

                    Binding bindcurxindex1 = new Binding
                    {
                        Path = new System.Windows.PropertyPath("ProbingModule.ProbingMXIndex"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((MapView.MapViewControl)this.ViewModelManager().MapViewControlFD,
                        MapView.MapViewControl.CursorXIndexProperty, bindcurxindex1);

                    Binding bindcuryindex1 = new Binding
                    {
                        Path = new System.Windows.PropertyPath("ProbingModule.ProbingMYIndex"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((MapView.MapViewControl)this.ViewModelManager().MapViewControlFD,
                        MapView.MapViewControl.CursorYIndexProperty, bindcuryindex1);

                    Binding bindrendermode1 = new Binding
                    {
                        Path = new System.Windows.PropertyPath("StageSupervisor.WaferObject.MapViewControlMode"),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding((MapView.MapViewControl)this.ViewModelManager().MapViewControlFD,
                        MapView.MapViewControl.RenderModeProperty, bindrendermode1);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Wafer.MapViewControlMode = MapViewMode.MapMode;
                Wafer.DPMarkerVisible = Visibility.Visible;
                Wafer.MapViewCurIndexVisiablity = true;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
           
            return Task.FromResult<EventCodeEnum>(retval);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public void Dispose()
        {
        }
    }
}

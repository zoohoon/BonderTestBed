using Autofac;

using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using RelayCommandBase;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FoupE84Control;
using FoupRecoveryControl;
using FoupManualControl;
using System.Windows.Input;
using System.Threading;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using FoupMainControl;
using LogModule;
using LoaderControllerBase;
//using ProberInterfaces.ThreadSync;

namespace FoupControlViewModel
{
    public class FoupControlVM : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private ObservableCollection<IOPortDescripter<bool>> _OutputPorts
    = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> OutputPorts
        {
            get { return _OutputPorts; }
            set
            {
                if (value != _OutputPorts)
                {
                    _OutputPorts = value;
                    RaisePropertyChanged("OutputPorts");
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _InputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> InputPorts
        {
            get { return _InputPorts; }
            set
            {
                if (value != _InputPorts)
                {
                    _InputPorts = value;
                    RaisePropertyChanged("InputPorts");
                }
            }
        }
        //private LockKey outPortLock = new LockKey("Foup control VM - out port");
        private object outPortLock = new object();

        private ObservableCollection<IOPortDescripter<bool>> _FilteredOutputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> FilteredOutputPorts
        {
            get { return _FilteredOutputPorts; }
            set
            {
                if (value != _FilteredOutputPorts)
                {
                    _FilteredOutputPorts = value;
                    RaisePropertyChanged("FilteredOutputPorts");
                }
            }
        }
        //private LockKey inPortLock = new LockKey("Foup control VM - in port");
        private object inPortLock = new object();

        private ObservableCollection<IOPortDescripter<bool>> _FilteredInputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> FilteredInputPorts
        {
            get { return _FilteredInputPorts; }
            set
            {
                if (value != _FilteredInputPorts)
                {
                    _FilteredInputPorts = value;
                    RaisePropertyChanged("FilteredInputPorts");
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
                    RaisePropertyChanged("SearchKeyword");
                    SearchMatched();
                }
            }
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("972B231E-CA73-4AA1-9F43-C5115CF980BB");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;



        private IFoupProcedureManager _FoupProcedureManager;
        public IFoupProcedureManager FoupProcedureManager
        {
            get { return _FoupProcedureManager; }
            set
            {
                if (value != _FoupProcedureManager)
                {
                    _FoupProcedureManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoup3DModel _Foup3DModel;
        public IFoup3DModel Foup3DModel
        {
            get { return _Foup3DModel; }
            set
            {
                if (value == null)
                {
                    if (this.Fouptype == FoupTypeEnum.TOP)
                    {
                        _Foup3DModel = Foup12InchModel;
                        RaisePropertyChanged();
                    }
                    else if (this.Fouptype == FoupTypeEnum.CST8PORT_FLAT)
                    {
                        _Foup3DModel = Foup8Inch;
                        RaisePropertyChanged();
                    }
                }
                if (value != _Foup3DModel)
                {
                    _Foup3DModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupSubView _FoupSubView;
        public IFoupSubView FoupSubView
        {
            get { return _FoupSubView; }
            set
            {
                if (value == null)
                {
                    _FoupSubView = RecoveryView;
                }
                if (value != _FoupSubView)
                {
                    _FoupSubView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupStateEnum _FoupstateEnum;
        public FoupStateEnum FoupstateEnum
        {
            get { return _FoupstateEnum; }
            set
            {
                if (value != _FoupstateEnum)
                {
                    _FoupstateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IFoup3DModel Foup8Inch { get; set; }
        private IFoup3DModel Foup12InchModel { get; set; }
        //private IFoup3DModel TestModel { get; set; }

        private IFoupSubView RecoveryView { get; set; }
        private IFoupSubView E84View { get; set; }
        private IFoupSubView ManualView { get; set; }

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

        public IViewModelManager ViewModelManager { get; set; }

        //public IFoupController FoupController => this.FoupOpModule().GetFoupController(1);

        private IFoupSubUserControl _FoupSubUC;
        public IFoupSubUserControl FoupSubUC
        {
            get { return _FoupSubUC; }
            set
            {
                if (value != _FoupSubUC)
                {
                    _FoupSubUC = value;
                    RaisePropertyChanged();
                }
            }
        }
        private FoupManagerSystemParameter _SystemParam;
        public FoupManagerSystemParameter SystemParam
        {
            get { return _SystemParam; }
            set
            {
                if (value != _SystemParam)
                {
                    _SystemParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private FoupManagerDeviceParameter _DeviceParam;
        public FoupManagerDeviceParameter DeviceParam
        {
            get { return _DeviceParam; }
            set
            {
                if (value != _DeviceParam)
                {
                    _DeviceParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _FoupIOEngMode;
        public bool FoupIOEngMode
        {
            get { return _FoupIOEngMode; }
            set
            {
                if (value != _FoupIOEngMode)
                {
                    _FoupIOEngMode = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Visibility _IOButtonVIsible;
        public Visibility IOButtonVIsible
        {
            get { return _IOButtonVIsible; }
            set
            {
                if (value != _IOButtonVIsible)
                {
                    _IOButtonVIsible = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private IFoupSubUserControlVM _FoupSubUCVM;
        //public IFoupSubUserControlVM FoupSubUCVM
        //{
        //    get { return _FoupSubUCVM; }
        //    set
        //    {
        //        if (value != _FoupSubUCVM)
        //        {
        //            _FoupSubUCVM = value;
        //            NotifyPropertyChanged("FoupSubUCVM");
        //        }
        //    }
        //}
        private bool _BlinkLED;
        public bool BlinkLED
        {
            get { return _BlinkLED; }
            set
            {
                if (value != _BlinkLED)
                {
                    _BlinkLED = value;
                    RaisePropertyChanged();
                }
            }
        }
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
        bool bStopUpdateStateThread = false;
        Thread UpdateStateThread;

        public void BlinkJob()
        {
            try
            {
                bool LEDBLINK = true;

                while (bStopUpdateStateThread == false)
                {
                    BlinkLED = !BlinkLED;

                    //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함
                    //sleep시간은 기존 timer interval 주기(1000)보다 작은 300ms를 대기 하고 있었으므로 이와 동일하게 300ms로 설정함
                    System.Threading.Thread.Sleep(300);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private IFoupSubUserControl FoupSubUC_For_E84 { get; set; }
        private IFoupSubUserControl FoupSubUC_For_Manual { get; set; }
        private IFoupSubUserControl FoupSubUC_For_Recovery { get; set; }


        public IFoupOpModule FoupOp => this.FoupOpModule();// => Container.Resolve<IFoupOpModule>();

        private IFoupModule FoupModule { get; }
        public IProberStation Prober;// => Container.Resolve<IProberStation>();
        public ILotOPModule LotOP;// => Container.Resolve<ILotOPModule>();

        public IStageSupervisor StageSupervisor;//=> Container.Resolve<IStageSupervisor>();


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                if (Initialized == false)
                {
                    ViewModelManager = this.ViewModelManager();

                    SystemParam = this.FoupOpModule()?.FoupManagerSysParam_IParam as FoupManagerSystemParameter;
                    DeviceParam = this.FoupOpModule()?.FoupManagerDevParam_IParam as FoupManagerDeviceParameter;
                    PropertyInfo[] propertyInfos;
                    IOPortDescripter<bool> port;
                    object propObject;
                    BlinkLED = false;
                    FoupIOEngMode = false;
                    IOButtonVIsible = Visibility.Hidden;

                    if (SystemParam != null)
                    {
                        Fouptype = SystemParam.FoupModules[0].FoupType.Value;
                    }

                    if (DeviceParam != null)
                    {
                        Size = DeviceParam.FoupModules[0].SubstrateSize.Value;
                    }

                    LoaderController = this.LoaderController() as ILoaderControllerExtension;
                    bStopUpdateStateThread = false;
                    UpdateStateThread = new Thread(new ThreadStart(BlinkJob));
                    UpdateStateThread.Name = this.GetType().Name;
                    UpdateStateThread.Start();

                    if (this.IOManager() != null)
                    {

                        OutputPorts.Clear();
                        InputPorts.Clear();

                        //Foup OutPut
                        propertyInfos = this.FoupOpModule().GetFoupIOMap(1).Outputs.GetType().GetProperties();

                        foreach (var item in propertyInfos)
                        {
                            if (item.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                port = new IOPortDescripter<bool>();
                                propObject = item.GetValue(this.FoupOpModule().GetFoupIOMap(1).Outputs);
                                port = (IOPortDescripter<bool>)propObject;
                                OutputPorts.Add(port);
                                FilteredOutputPorts.Add(port);
                            }
                        }

                        //Foup InPut
                        propertyInfos = this.FoupOpModule().GetFoupIOMap(1).Inputs.GetType().GetProperties();
                        foreach (var item in propertyInfos)
                        {
                            if (item.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                port = new IOPortDescripter<bool>();
                                propObject = item.GetValue(this.FoupOpModule().GetFoupIOMap(1).Inputs);
                                port = (IOPortDescripter<bool>)propObject;
                                InputPorts.Add(port);
                                FilteredInputPorts.Add(port);
                            }
                        }
                        //port.Key
                    }

                    FoupController = this.FoupOpModule()?.GetFoupController(1);

                    #region Sub view
                    RecoveryView = new FoupRecoveryControlView();
                    E84View = new FoupE84ControlView();
                    ManualView = new FoupManualControlView();
                    #endregion

                    #region  3D Model
                    Foup12InchModel = new Foup3DModel();
                    Foup8Inch = new Foup8Inch();
                    //TestModel = new ThreePodStage();

                    if (this.Fouptype == FoupTypeEnum.TOP)
                    {
                        Foup3DModel = Foup12InchModel;
                    }
                    else if (this.Fouptype == FoupTypeEnum.CST8PORT_FLAT)
                    {
                        Foup3DModel = Foup8Inch;
                    }

                    if (FoupController != null)
                    {
                        FoupstateEnum = FoupController.FoupModuleInfo.State;
                        FoupProcedureManager = FoupController.GetFoupProcedureManager();
                    }

                    #endregion


                    FoupSubView = RecoveryView;

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
                //LoggerManager.Error($err, string.Format("InitModule(): Error occurred."));
                LoggerManager.Exception(err);

                retval = EventCodeEnum.SYSTEM_ERROR;
            }

            return retval;
        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

                bStopUpdateStateThread = true;

                UpdateStateThread?.Join();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #region Value

        #endregion

        #region Command

        private RelayCommand<object> _PlateTiltUpCommand;
        public ICommand PlateTiltUpCommand
        {
            get
            {
                if (null == _PlateTiltUpCommand) _PlateTiltUpCommand = new RelayCommand<object>(PlateTiltUpCommandFunc);
                return _PlateTiltUpCommand;
            }
        }
        private void PlateTiltUpCommandFunc(object noparam)
        {
            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOFOUPSWING, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _PlateTiltDownCommand;
        public ICommand PlateTiltDownCommand
        {
            get
            {
                if (null == _PlateTiltDownCommand) _PlateTiltDownCommand = new RelayCommand<object>(PlateTiltDownCommandFunc);
                return _PlateTiltDownCommand;
            }
        }
        private void PlateTiltDownCommandFunc(object noparam)
        {
            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOFOUPSWING, false);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _CassetteLockCommand;
        public ICommand CassetteLockCommand
        {
            get
            {
                if (null == _CassetteLockCommand) _CassetteLockCommand = new RelayCommand<object>(CassetteLockCommandFunc);
                return _CassetteLockCommand;
            }
        }
        private void CassetteLockCommandFunc(object noparam)
        {
            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCASSETTELOCK, true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _CassetteUnLockCommand;
        public ICommand CassetteUnLockCommand
        {
            get
            {
                if (null == _CassetteUnLockCommand) _CassetteUnLockCommand = new RelayCommand<object>(CassetteUnLockCommandFunc);
                return _CassetteUnLockCommand;
            }
        }
        private void CassetteUnLockCommandFunc(object noparam)
        {
            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCASSETTELOCK, false);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _EngModeOnCommand;
        public ICommand EngModeOnCommand
        {
            get
            {
                if (null == _EngModeOnCommand) _EngModeOnCommand = new RelayCommand<object>(EngModeOnCommandFunc);
                return _EngModeOnCommand;
            }
        }
        private void EngModeOnCommandFunc(object noparam)
        {
            try
            {
                FoupIOEngMode = true;
                IOButtonVIsible = Visibility.Visible;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _EngModeOffCommand;
        public ICommand EngModeOffCommand
        {
            get
            {
                if (null == _EngModeOffCommand) _EngModeOffCommand = new RelayCommand<object>(EngModeOffCommandFunc);
                return _EngModeOffCommand;
            }
        }
        private void EngModeOffCommandFunc(object noparam)
        {
            try
            {
                FoupIOEngMode = false;
                IOButtonVIsible = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ChangeStateToLoad;
        public RelayCommand<object> ChangeStateToLoad
        {
            get
            {
                if (null == _ChangeStateToLoad) _ChangeStateToLoad = new RelayCommand<object>(FuncChangeStateToLoad);
                return _ChangeStateToLoad;
            }
        }

        private void FuncChangeStateToLoad(object obj)
        {
            try
            {
                FoupController.ChangeState(FoupStateEnum.LOAD);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private RelayCommand<object> _ChangeStateToILLEGAL;
        //public RelayCommand<object> ChangeStateToILLEGAL
        //{
        //    get
        //    {
        //        if (null == _ChangeStateToILLEGAL) _ChangeStateToILLEGAL = new RelayCommand<object>(FuncChangeStateToILLEGAL);
        //        return _ChangeStateToILLEGAL;
        //    }
        //}

        //private void FuncChangeStateToILLEGAL(object obj)
        //{
        //    try
        //    {
        //        FoupController.ChangeState(FoupStateEnum.ILLEGAL);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private RelayCommand<object> _RecoveryFastBackwardCommand;
        public RelayCommand<object> RecoveryFastBackwardCommand
        {
            get
            {
                if (null == _RecoveryFastBackwardCommand) _RecoveryFastBackwardCommand = new RelayCommand<object>(FuncRecoveryFastBackwardCommand);
                return _RecoveryFastBackwardCommand;
            }
        }

        private void FuncRecoveryFastBackwardCommand(object obj)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupRecoveryFastBackwardCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _RecoveryPreviousCommand;
        public RelayCommand<object> RecoveryPreviousCommand
        {
            get
            {
                if (null == _RecoveryPreviousCommand) _RecoveryPreviousCommand = new RelayCommand<object>(FuncRecoveryPreviousCommand);
                return _RecoveryPreviousCommand;
            }
        }

        private void FuncRecoveryReverseCommand(object obj)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupRecoveryReverseCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

        }

        private RelayCommand<object> _RecoveryReverseCommand;
        public RelayCommand<object> RecoveryReverseCommand
        {
            get
            {
                if (null == _RecoveryReverseCommand) _RecoveryReverseCommand = new RelayCommand<object>(FuncRecoveryReverseCommand);
                return _RecoveryReverseCommand;
            }
        }

        private void FuncRecoveryPreviousCommand(object obj)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupRecoveryPreviousCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

        }

        private RelayCommand<object> _RecoveryNextCommand;
        public RelayCommand<object> RecoveryNextCommand
        {
            get
            {
                if (null == _RecoveryNextCommand) _RecoveryNextCommand = new RelayCommand<object>(FuncRecoveryNextCommand);
                return _RecoveryNextCommand;
            }
        }

        private void FuncRecoveryNextCommand(object obj)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupRecoveryNextCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

        }

        //private RelayCommand<object> _RecoveryFastForwardCommand;
        //public RelayCommand<object> RecoveryFastForwardCommand
        //{
        //    get
        //    {
        //        if (null == _RecoveryFastForwardCommand) _RecoveryFastForwardCommand = new RelayCommand<object>(FuncRecoveryFastForwardCommand);
        //        return _RecoveryFastForwardCommand;
        //    }
        //}

        //private void FuncRecoveryFastForwardCommand(object obj)
        //{
        //    this.FoupOpModule().GetFoupController(1).Execute(new FoupRecoveryFastForwardCommand());

        //}
        private AsyncCommand _RecoveryFastForwardCommand;
        public ICommand RecoveryFastForwardCommand
        {
            get
            {
                if (null == _RecoveryFastForwardCommand) _RecoveryFastForwardCommand = new AsyncCommand(FuncRecoveryFastForwardCommand);
                return _RecoveryFastForwardCommand;
            }
        }

        private Task FuncRecoveryFastForwardCommand()
        {
            Task stateTask = null;

            try
            {
                stateTask = Task.Run(() => this.FoupOpModule().GetFoupController(1).Execute(new FoupRecoveryFastForwardCommand()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

            return stateTask;
        }


        private RelayCommand<object> _Recovery_SubViewChangeCommand;
        public RelayCommand<object> Recovery_SubViewChangeCommand
        {
            get
            {
                if (null == _Recovery_SubViewChangeCommand) _Recovery_SubViewChangeCommand = new RelayCommand<object>(FuncRecovery_SubViewChangeCoomand);
                return _Recovery_SubViewChangeCommand;
            }
        }

        private void FuncRecovery_SubViewChangeCoomand(object obj)
        {
            try
            {
                FoupSubView = RecoveryView;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _E84_SubViewChangeCommand;
        public RelayCommand<object> E84_SubViewChangeCommand
        {
            get
            {
                if (null == _E84_SubViewChangeCommand) _E84_SubViewChangeCommand = new RelayCommand<object>(FuncE84_SubViewChangeCoomand);
                return _E84_SubViewChangeCommand;
            }
        }

        private void FuncE84_SubViewChangeCoomand(object obj)
        {
            try
            {
                FoupSubView = E84View;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _Manual_SubViewChangeCommand;
        public RelayCommand<object> Manual_SubViewChangeCommand
        {
            get
            {
                if (null == _Manual_SubViewChangeCommand) _Manual_SubViewChangeCommand = new RelayCommand<object>(FuncManual_SubViewChangeCoomand);
                return _Manual_SubViewChangeCommand;
            }
        }

        private void FuncManual_SubViewChangeCoomand(object obj)
        {
            try
            {
                FoupSubView = ManualView;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _FoupTiltUpCommand;
        public RelayCommand<object> FoupTiltUpCommand
        {
            get
            {
                if (null == _FoupTiltUpCommand) _FoupTiltUpCommand = new RelayCommand<object>(Func_FoupTiltUpCommand);
                return _FoupTiltUpCommand;
            }
        }

        private void Func_FoupTiltUpCommand(object obj)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupTiltUpCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

        }

        private RelayCommand<object> _FoupTiltDownCommand;
        public RelayCommand<object> FoupTiltDownCommand
        {
            get
            {
                if (null == _FoupTiltDownCommand) _FoupTiltDownCommand = new RelayCommand<object>(Func_FoupTiltDownCommand);
                return _FoupTiltDownCommand;
            }
        }

        private void Func_FoupTiltDownCommand(object obj)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupTiltDownCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

        }

        private RelayCommand<object> _cmdCSTLock;
        public RelayCommand<object> cmdCSTLock
        {
            get
            {
                if (null == _cmdCSTLock) _cmdCSTLock = new RelayCommand<object>(CSTLockFunc);
                return _cmdCSTLock;
            }
        }
        public void CSTLockFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPlateLockCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdCSTUnlock;
        public RelayCommand<object> cmdCSTUnlock
        {
            get
            {
                if (null == _cmdCSTUnlock) _cmdCSTUnlock = new RelayCommand<object>(CSTUnlockFunc);
                return _cmdCSTUnlock;
            }
        }
        public void CSTUnlockFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPlateUnlockCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdDPLoad;
        public RelayCommand<object> cmdDPLoad
        {
            get
            {
                if (null == _cmdDPLoad) _cmdDPLoad = new RelayCommand<object>(DPLoadFunc);
                return _cmdDPLoad;
            }
        }
        public void DPLoadFunc(object noparam)
        {
            try
            {

                int retValue = -1;
                bool value = false;
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPortInCommand());

                // To-Dowafer out sensor 관련 동작 넣기
                //Module.FoupIOManager.ReadBit(Module.FoupIOManager.FoupIOMap.Inputs.DI_WAFER_OUT, out value);
                //if(value == true)
                //{
                //}
                //_Module.DockingPort.In();
                //Module.FoupIOManager.WaitForIO(Module.FoupIOManager.FoupIOMap.Inputs.DI_CP_IN, true);
                //Module.DockingPort.StateInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        private RelayCommand<object> _cmdDPUnLoad;
        public RelayCommand<object> cmdDPUnLoad
        {
            get
            {
                if (null == _cmdDPUnLoad) _cmdDPUnLoad = new RelayCommand<object>(DPUnLoadFunc);
                return _cmdDPUnLoad;
            }
        }
        public void DPUnLoadFunc(object noparam)
        {
            try
            {
                bool value = false;
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPortOutCommand());

                //_Module.DockingPort.Out();
                //Module.FoupIOManager.WaitForIO(Module.FoupIOManager.FoupIOMap.Inputs.DI_CP_OUT, true);
                //Module.DockingPort.StateInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        private RelayCommand<object> _cmdDP40Load;
        public RelayCommand<object> cmdDP40Load
        {
            get
            {
                if (null == _cmdDP40Load) _cmdDP40Load = new RelayCommand<object>(DP40LoadFunc);
                return _cmdDP40Load;
            }
        }
        public void DP40LoadFunc(object noparam)
        {
            try
            {
                bool value = false;
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPort40InCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdDP40UnLoad;
        public RelayCommand<object> cmdDP40UnLoad
        {
            get
            {
                if (null == _cmdDP40UnLoad) _cmdDP40UnLoad = new RelayCommand<object>(DP40UnLoadFunc);
                return _cmdDP40UnLoad;
            }
        }
        public void DP40UnLoadFunc(object noparam)
        {
            bool value = false;

            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPort40OutCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdFOCUp;
        public RelayCommand<object> cmdFOCUp
        {
            get
            {
                if (null == _cmdFOCUp) _cmdFOCUp = new RelayCommand<object>(FOCUpFunc);
                return _cmdFOCUp;
            }
        }
        public void FOCUpFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupCoverUpCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdFOCDown;
        public RelayCommand<object> cmdFOCDown
        {
            get
            {
                if (null == _cmdFOCDown) _cmdFOCDown = new RelayCommand<object>(FOCDownFunc);
                return _cmdFOCDown;
            }
        }
        public void FOCDownFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupCoverDownCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdROTOpen;
        public RelayCommand<object> cmdROTOpen
        {
            get
            {
                if (null == _cmdROTOpen) _cmdROTOpen = new RelayCommand<object>(ROTOpenFunc);
                return _cmdROTOpen;
            }
        }
        public void ROTOpenFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupCassetteOpenerUnlockCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        private RelayCommand<object> _cmdROTClose;
        public RelayCommand<object> cmdROTClose
        {
            get
            {
                if (null == _cmdROTClose) _cmdROTClose = new RelayCommand<object>(ROTCloseFunc);
                return _cmdROTClose;
            }
        }
        public void ROTCloseFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupCassetteOpenerLockCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

        }

        private RelayCommand<object> _cmdVACOn;
        public RelayCommand<object> cmdVACOn
        {
            get
            {
                if (null == _cmdVACOn) _cmdVACOn = new RelayCommand<object>(VACOnFunc);
                return _cmdVACOn;
            }
        }
        public void VACOnFunc(object noparam)
        {

        }
        private RelayCommand<object> _cmdVACOff;
        public RelayCommand<object> cmdVACOff
        {
            get
            {
                if (null == _cmdVACOff) _cmdVACOff = new RelayCommand<object>(VACOffFunc);
                return _cmdVACOff;
            }
        }
        public void VACOffFunc(object noparam)
        {

        }

        private RelayCommand<object> _cmdCYLOpen;
        public RelayCommand<object> cmdCYLOpen
        {
            get
            {
                if (null == _cmdCYLOpen) _cmdCYLOpen = new RelayCommand<object>(CYLOpenFunc);
                return _cmdCYLOpen;
            }
        }
        public void CYLOpenFunc(object noparam)
        {
            // To-Do
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupCassetteOpenerUnlockCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

        }
        private RelayCommand<object> _cmdPlate;

        public RelayCommand<object> cmdPlate
        {
            get { return _cmdPlate; }
            set { _cmdPlate = value; }
        }




        private RelayCommand<object> _cmdFOCoverOpen;
        public RelayCommand<object> cmdFOCoverOpen
        {
            get
            {
                if (null == _cmdFOCoverOpen) _cmdFOCoverOpen = new RelayCommand<object>(FOCoverOpenFunc);
                return _cmdFOCoverOpen;
            }
        }
        public void FOCoverOpenFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupCassetteOpenerUnlockCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        private RelayCommand<object> _cmdFOCoverClose;
        public RelayCommand<object> cmdFOCoverClose
        {
            get
            {
                if (null == _cmdFOCoverClose) _cmdFOCoverClose = new RelayCommand<object>(FOCoverCloseFunc);
                return _cmdFOCoverClose;
            }
        }
        public void FOCoverCloseFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupCassetteOpenerLockCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private AsyncCommand _cmdLOAD;
        public ICommand cmdLOAD
        {
            get
            {
                if (null == _cmdLOAD) _cmdLOAD = new AsyncCommand(LOADFunc);
                return _cmdLOAD;
            }
        }

        private Task LOADFunc()
        {
            Task stateTask = null;

            try
            {
                stateTask = Task.Run(() => this.FoupOpModule().GetFoupController(1).Execute(new FoupLoadCommand()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }



            return stateTask;
        }

        //public async Task<EventCodeEnum> LOADFunc(object noparam)
        //{
        //    this.FoupOpModule().GetFoupController(1).Execute(new FoupLoadCommand());
        //}
        //private AsyncParamCommand _cmdUNLOAD;
        //public AsyncParamCommand cmdUNLOAD
        //{
        //    get
        //    {
        //        if (null == _cmdUNLOAD) _cmdUNLOAD = new AsyncParamCommand(UNLOADFunc);
        //        return _cmdUNLOAD;
        //    }
        //}
        //private void UNLOADFunc(object cuiparam)
        //{
        //    Task stateTask;

        //    try
        //    {
        //        stateTask = Task.Run(() => this.FoupOpModule().GetFoupController(1).Execute(new FoupUnloadCommand()));
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    //return stateTask;
        //}
        private AsyncCommand _cmdUNLOAD;
        public AsyncCommand cmdUNLOAD
        {
            get
            {
                if (null == _cmdUNLOAD) _cmdUNLOAD = new AsyncCommand(UNLOADFunc);
                return _cmdUNLOAD;
            }
        }
        private Task UNLOADFunc()
        {
            Task stateTask = null;

            try
            {
                stateTask = Task.Run(() => this.FoupOpModule().GetFoupController(1).Execute(new FoupUnloadCommand()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return stateTask;
        }

        private AsyncCommand _cmdBACK;
        public IAsyncCommand cmdBACK
        {
            get
            {
                if (null == _cmdBACK) _cmdBACK = new AsyncCommand(BACKFunc);
                return _cmdBACK;
            }
        }
        private async Task BACKFunc()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //          Task stateTask;

            //stateTask = Task.Run(() => this.FoupOpModule().GetFoupController(1).Execute(new FoupUnloadCommand()));
            //return stateTask;
        }
        private RelayCommand<object> _cmd6Inch;
        public RelayCommand<object> cmd6Inch
        {
            get
            {
                if (null == _cmd6Inch) _cmd6Inch = new RelayCommand<object>(Inch6Func);

                return _cmd6Inch;
            }
            set
            {

            }

        }
        public void Inch6Func(object noparam)
        {
            try
            {
                // To-Do
                //_Module.DeviceParam.SubstrateSize = ProberInterfaces.Loader.SubstrateSizeEnum.INCH6;
                _imaUNLOADSQ1 = "pack://application:,,,/ImageResourcePack;component/Images/6_8LOCK.png";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _cmd8Inch;
        public RelayCommand<object> cmd8Inch
        {
            get
            {
                if (null == _cmd8Inch) _cmd8Inch = new RelayCommand<object>(Inch8Func);
                return _cmd8Inch;
            }
        }
        public void Inch8Func(object noparam)
        {
            try
            {
                // To-Do
                //_Module.DeviceParam.SubstrateSize = ProberInterfaces.Loader.SubstrateSizeEnum.INCH8;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        private RelayCommand<object> _cmd12Inch;
        public RelayCommand<object> cmd12Inch
        {
            get
            {
                if (null == _cmd12Inch) _cmd12Inch = new RelayCommand<object>(Inch12Func);
                return _cmd12Inch;
            }
        }
        public void Inch12Func(object noparam)
        {
            try
            {
                // To-Do
                //_Module.DeviceParam.SubstrateSize = ProberInterfaces.Loader.SubstrateSizeEnum.INCH12;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdErrorReset;
        public RelayCommand<object> cmdErrorReset
        {
            get
            {
                if (null == _cmdErrorReset) _cmdErrorReset = new RelayCommand<object>(ErrorResetFunc);
                return _cmdErrorReset;

            }
        }
        public void ErrorResetFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupInitStateCommand());
                //FoupController.FoupErrorReset();
                //this.FoupOpModule().GetFoupController(1).Execute(new FoupCassetteOpenerLockCommand());
                //this.FoupOpModule().InitModule();
                ////To - Do
                //_Module.ErrorClear();
                //FoupManagerProvider.Singleton.FoupSetting(1);
                //Module.DockingPort40.StateInit();
                //Module.DockingPort.StateInit();
                //Module.DockingPlate.StateInit();
                //Module.FoupCover.StateInit();
                //Module.FoupOpener.StateInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

        }

        private RelayCommand<object> _cmdFO40In;
        public RelayCommand<object> cmdFO40In
        {
            get
            {
                if (null == _cmdFO40In) _cmdFO40In = new RelayCommand<object>(FO40InFunc);
                return _cmdFO40In;
            }
        }
        public void FO40InFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPort40InCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdFO40Out;
        public RelayCommand<object> cmdFO40Out
        {
            get
            {
                if (null == _cmdFO40Out) _cmdFO40Out = new RelayCommand<object>(FO40OutFunc);
                return _cmdFO40Out;
            }
        }
        public void FO40OutFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPort40OutCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdFOIn;
        public RelayCommand<object> cmdFOIn
        {
            get
            {
                if (null == _cmdFOIn) _cmdFOIn = new RelayCommand<object>(FOInFunc);
                return _cmdFOIn;
            }
        }
        public void FOInFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPortInCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand<object> _cmdFOOut;
        public RelayCommand<object> cmdFOOut
        {
            get
            {
                if (null == _cmdFOOut) _cmdFOOut = new RelayCommand<object>(FOOutFunc);
                return _cmdFOOut;
            }
        }
        public void FOOutFunc(object noparam)
        {
            try
            {
                this.FoupOpModule().GetFoupController(1).Execute(new FoupDockingPortOutCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }


        public EventCodeEnum InitPage()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitPage(object parameter = null)
        {
            return EventCodeEnum.NONE;
        }
        private FoupTypeEnum _Fouptype = new FoupTypeEnum();
        public FoupTypeEnum Fouptype
        {
            get { return _Fouptype; }
            set
            {
                if (value != _Fouptype)
                {
                    _Fouptype = value;
                    RaisePropertyChanged();
                }
            }
        }


        public SubstrateSizeEnum Size { get; set; }


        private RelayCommand<object> _cmdUNLOADSQ1;
        public RelayCommand<object> cmdUNLOADSQ1
        {
            get
            {

                if (null == _cmdUNLOADSQ1) _cmdUNLOADSQ1 = new RelayCommand<object>(UNLOADSQ1);
                return _cmdUNLOADSQ1;
            }
        }
        public void UNLOADSQ1(object noparam)
        {

            try
            {
                // To-Do
                // 조건문 INCH or LOADER TYPE 보고 Sequence 넣기.

                EnumWaferSize wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum;
                FoupTypeEnum fouptype = Fouptype;

                if (fouptype == FoupTypeEnum.TOP)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
                    {

                    }
                    else
                    {

                    }
                }
                else if (fouptype == FoupTypeEnum.FLAT)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
                    {

                    }
                    else
                    {

                    }
                }
                else if (fouptype == FoupTypeEnum.CST8PORT_FLAT)
                {
                    if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
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
        private RelayCommand<object> _cmdUNLOADSQ2;
        public RelayCommand<object> cmdUNLOADSQ2
        {
            get
            {
                if (null == _cmdUNLOADSQ2) _cmdUNLOADSQ2 = new RelayCommand<object>(UNLOADSQ2);
                return _cmdUNLOADSQ2;
            }
        }
        public void UNLOADSQ2(object noparam)
        {
            try
            {
                // To-Do
                // 조건문 INCH or LOADER TYPE 보고 Sequence 넣기.

                EnumWaferSize wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum;
                FoupTypeEnum fouptype = Fouptype;

                if (fouptype == FoupTypeEnum.TOP)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
                    {

                    }
                    else
                    {

                    }
                }
                else if (fouptype == FoupTypeEnum.FLAT)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
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
        private RelayCommand<object> _cmdUNLOADSQ3;
        public RelayCommand<object> cmdUNLOADSQ3
        {
            get
            {
                if (null == _cmdUNLOADSQ3) _cmdUNLOADSQ3 = new RelayCommand<object>(UNLOADSQ3);
                return _cmdUNLOADSQ3;
            }
        }
        public void UNLOADSQ3(object noparam)
        {
            try
            {
                // To-Do
                // 조건문 INCH or LOADER TYPE 보고 Sequence 넣기.

                EnumWaferSize wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum;
                FoupTypeEnum fouptype = Fouptype;

                if (fouptype == FoupTypeEnum.TOP)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
                    {

                    }
                    else
                    {

                    }
                }
                else if (fouptype == FoupTypeEnum.FLAT)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
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
        private RelayCommand<object> _cmdUNLOADSQ4;
        public RelayCommand<object> cmdUNLOADSQ4
        {
            get
            {
                if (null == _cmdUNLOADSQ4) _cmdUNLOADSQ4 = new RelayCommand<object>(UNLOADSQ4);
                return _cmdUNLOADSQ4;
            }
        }
        public void UNLOADSQ4(object noparam)
        {
            try
            {
                // To-Do
                // 조건문 INCH or LOADER TYPE 보고 Sequence 넣기.

                EnumWaferSize wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum;
                FoupTypeEnum fouptype = Fouptype;

                if (fouptype == FoupTypeEnum.TOP)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
                    {

                    }
                    else
                    {

                    }
                }
                else if (fouptype == FoupTypeEnum.FLAT)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
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
        private RelayCommand<object> _cmdUNLOADSQ5;
        public RelayCommand<object> cmdUNLOADSQ5
        {
            get
            {
                if (null == _cmdUNLOADSQ5) _cmdUNLOADSQ5 = new RelayCommand<object>(UNLOADSQ5);
                return _cmdUNLOADSQ5;
            }
        }
        public void UNLOADSQ5(object noparam)
        {
            try
            {
                // To-Do
                // 조건문 INCH or LOADER TYPE 보고 Sequence 넣기.

                EnumWaferSize wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum;
                FoupTypeEnum fouptype = Fouptype;

                if (fouptype == FoupTypeEnum.TOP)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
                    {

                    }
                    else
                    {

                    }
                }
                else if (fouptype == FoupTypeEnum.FLAT)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
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
        private RelayCommand<object> _cmdUNLOADSQ6;
        public RelayCommand<object> cmdUNLOADSQ6
        {
            get
            {
                if (null == _cmdUNLOADSQ6) _cmdUNLOADSQ6 = new RelayCommand<object>(UNLOADSQ6);
                return _cmdUNLOADSQ6;
            }
        }
        public void UNLOADSQ6(object noparam)
        {
            try
            {
                // To-Do
                // 조건문 INCH or LOADER TYPE 보고 Sequence 넣기.

                EnumWaferSize wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum;
                FoupTypeEnum fouptype = Fouptype;

                if (fouptype == FoupTypeEnum.TOP)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
                    {

                    }
                    else
                    {

                    }
                }
                else if (fouptype == FoupTypeEnum.FLAT)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
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
        private RelayCommand<object> _cmdUNLOADSQ7;
        public RelayCommand<object> cmdUNLOADSQ7
        {
            get
            {
                if (null == _cmdUNLOADSQ7) _cmdUNLOADSQ7 = new RelayCommand<object>(UNLOADSQ7);
                return _cmdUNLOADSQ7;
            }
        }
        public void UNLOADSQ7(object noparam)
        {
            try
            {
                // To-Do
                // 조건문 INCH or LOADER TYPE 보고 Sequence 넣기.

                EnumWaferSize wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum;
                FoupTypeEnum fouptype = Fouptype;

                if (fouptype == FoupTypeEnum.TOP)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
                    {

                    }
                    else
                    {

                    }
                }
                else if (fouptype == FoupTypeEnum.FLAT)
                {
                    if (wafersize == EnumWaferSize.INCH12)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH8)
                    {

                    }
                    else if (wafersize == EnumWaferSize.INCH6)
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
        private async void SearchMatched()
        {
            try
            {
                string upper = SearchKeyword.ToUpper();
                string lower = SearchKeyword.ToLower();

                await Task.Run(() =>
                {
                    if (SearchKeyword.Length > 0)
                    {
                        var outs = OutputPorts.Where(
                            t => t.Key.Value.StartsWith(upper) ||
                            t.Key.Value.StartsWith(lower) ||
                            t.Key.Value.ToUpper().Contains(upper));
                        var filtered = new ObservableCollection<IOPortDescripter<bool>>(outs);

                        //using (Locker locker = new Locker(outPortLock))
                        //{
                        lock (outPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredOutputPorts.Clear();
                                foreach (var item in filtered)
                                {
                                    FilteredOutputPorts.Add(item);
                                }
                            });
                        }


                        var inputs = InputPorts.Where(
                            t => t.Key.Value.StartsWith(upper) ||
                            t.Key.Value.StartsWith(lower) ||
                            t.Key.Value.ToUpper().Contains(upper));
                        filtered = new ObservableCollection<IOPortDescripter<bool>>(inputs);

                        //using (Locker locker = new Locker(inPortLock))
                        //{
                        lock (inPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredInputPorts.Clear();
                                foreach (var item in filtered)
                                {
                                    FilteredInputPorts.Add(item);
                                }
                            });

                        }
                    }
                    else
                    {
                        //using (Locker locker = new Locker(inPortLock))
                        //{
                        lock (inPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredInputPorts.Clear();
                                foreach (var item in InputPorts)
                                {
                                    FilteredInputPorts.Add(item);
                                }
                            });
                        }

                        //using (Locker locker = new Locker(outPortLock))
                        //{
                        lock (outPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredOutputPorts.Clear();
                                foreach (var item in OutputPorts)
                                {
                                    FilteredOutputPorts.Add(item);
                                }
                            });
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

        }
        #endregion

        #region button image

        public class Values
        {
            private int _ImageValue;
            public int ImageValue
            {
                get { return _ImageValue; }
                set { _ImageValue = value; }
            }

            private string imagePath_;
            public string ImagePath
            {
                get { return imagePath_; }
                set { imagePath_ = value; }
            }

            public Values(int ImageValue, string ImagePath)
            {

                this.ImagePath = ImagePath;
            }
        }


        //public string imaUNLOADSQ1 =IMAUNLOADSQ1(); //= @"pack://application:,,,/ImageResourcePack;component/Images/6_8LOCK.png";



        //public string IMAUNLOADSQ1()
        //{
        //    // To-Do
        //    string val;
        //    val = "pack://application:,,,/ImageResourcePack;component/Images/6_8LOCK.png";
        //    return val;
        //}
        //private RelayCommand<object> _imaUNLOADSQ1;
        //public RelayCommand<object> imaUNLOADSQ1
        //{
        //    get
        //    {
        //        if (null == _imaUNLOADSQ1) _imaUNLOADSQ1 = new RelayCommand<object>(IMAUNLOADSQ1);
        //        return _imaUNLOADSQ1;
        //    }
        //}
        //public string IMAUNLOADSQ1(object noparam)
        //{
        //    // To-Do
        //    return "";
        //}


        private string _imaUNLOADSQ1; //= "pack://application:,,,/ImageResourcePack;component/Images/6_8LOCK.png";
        public string imaUNLOADSQ1
        {
            get { return _imaUNLOADSQ1; }
            set
            {
                if (value != _imaUNLOADSQ1)
                {
                    _imaUNLOADSQ1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private string _imaUNLOADSQ1;

        //public string IMAUNLOADSQ1
        //{
        //    get { return _imaUNLOADSQ1; }
        //}

        private RelayCommand<object> _imaUNLOADSQ2;
        public RelayCommand<object> imaUNLOADSQ2
        {
            get
            {
                if (null == _imaUNLOADSQ2) _imaUNLOADSQ2 = new RelayCommand<object>(IMAUNLOADSQ2);
                return _imaUNLOADSQ2;
            }
        }
        public void IMAUNLOADSQ2(object noparam)
        {
            //imaUNLOADSQ2
            // To-Do

        }
        private RelayCommand<object> _imaUNLOADSQ3;
        public RelayCommand<object> imaUNLOADSQ3
        {
            get
            {
                if (null == _imaUNLOADSQ3) _imaUNLOADSQ3 = new RelayCommand<object>(IMAUNLOADSQ3);
                return _imaUNLOADSQ3;
            }
        }
        public void IMAUNLOADSQ3(object noparam)
        {
            // To-Do


        }
        private RelayCommand<object> _imaUNLOADSQ4;
        public RelayCommand<object> imaUNLOADSQ4
        {
            get
            {
                if (null == _imaUNLOADSQ4) _imaUNLOADSQ4 = new RelayCommand<object>(IMAUNLOADSQ4);
                return _imaUNLOADSQ4;
            }
        }
        public void IMAUNLOADSQ4(object noparam)
        {
            // To-Do


        }
        private RelayCommand<object> _imaUNLOADSQ5;
        public RelayCommand<object> imaUNLOADSQ5
        {
            get
            {
                if (null == _imaUNLOADSQ5) _imaUNLOADSQ5 = new RelayCommand<object>(IMAUNLOADSQ5);
                return _imaUNLOADSQ5;
            }
        }
        public void IMAUNLOADSQ5(object noparam)
        {
            // To-Do


        }
        private RelayCommand<object> _imaUNLOADSQ6;
        public RelayCommand<object> imaUNLOADSQ6
        {
            get
            {
                if (null == _imaUNLOADSQ6) _imaUNLOADSQ6 = new RelayCommand<object>(IMAUNLOADSQ6);
                return _imaUNLOADSQ6;
            }
        }
        public void IMAUNLOADSQ6(object noparam)
        {
            // To-Do


        }
        private RelayCommand<object> _imaUNLOADSQ7;
        public RelayCommand<object> imaUNLOADSQ7
        {
            get
            {
                if (null == _imaUNLOADSQ7) _imaUNLOADSQ7 = new RelayCommand<object>(IMAUNLOADSQ7);
                return _imaUNLOADSQ7;
            }
        }



        public void IMAUNLOADSQ7(object noparam)
        {
            // To-Do


        }

        #endregion

        #region 3D Move


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
        private double _FoupPortPos;

        public double FoupPortPos
        {
            get { return _FoupPortPos; }
            set
            {
                if (value != _FoupPortPos)
                {
                    _FoupPortPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _FoupOpenerStatus;

        public string FoupOpenerStatus
        {
            get { return _FoupOpenerStatus; }
            set
            {
                if (value != _FoupOpenerStatus)
                {
                    _FoupOpenerStatus = value;
                    RaisePropertyChanged();
                }
            }

        }

        private string _FoupDockingPlateStatus;
        public string FoupDockingPlateStatus
        {
            get { return _FoupDockingPlateStatus; }
            set
            {
                if (value != _FoupOpenerStatus)
                {
                    _FoupDockingPlateStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

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
            FoupIOEngMode = false;
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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

        #region Recovery Tap

        #region Recovery Button Command
        private RelayCommand<object> _RecoveryButtonTest;
        public RelayCommand<object> RecoveryButtonTest
        {
            get
            {
                if (null == _RecoveryButtonTest) _RecoveryButtonTest = new RelayCommand<object>(FuncRecoveryButtonTest);
                return _RecoveryButtonTest;
            }
        }

        public void FuncRecoveryButtonTest(object noparam)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _SwitchRecovery;
        public RelayCommand<object> SwitchRecovery
        {
            get
            {
                if (null == _SwitchRecovery) _SwitchRecovery = new RelayCommand<object>(FuncSwitchRecovery);
                return _SwitchRecovery;
            }
        }
        public void FuncSwitchRecovery(object noparam)
        {
            try
            {
                FoupSubUC = FoupSubUC_For_Recovery;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        #endregion

        #region Recovery Function


        #endregion

        #endregion
    }

}

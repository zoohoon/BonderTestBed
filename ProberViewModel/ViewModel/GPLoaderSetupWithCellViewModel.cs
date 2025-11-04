using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderCore;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.Foup;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using UcDisplayPort;

namespace ProberViewModel.ViewModel
{
    public class GPLoaderSetupWithCellViewModel : INotifyPropertyChanged, IMainScreenViewModel, IFactoryModule
    {
        readonly Guid _ViewModelGUID = new Guid("F673D2C8-D7E1-426C-BF97-99B7102EDC6C");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region property
        private object Lockobj;

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
        private string _EdgeAlignResult;
        public string EdgeAlignResult
        {
            get { return _EdgeAlignResult; }
            set
            {
                if (value != _EdgeAlignResult)
                {
                    _EdgeAlignResult = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _DoorResult;
        public string DoorResult
        {
            get { return _DoorResult; }
            set
            {
                if (value != _DoorResult)
                {
                    _DoorResult = value;
                    RaisePropertyChanged();
                }
            }
        }


        private WaferCoordinate _WaferCenterOffset;
        public WaferCoordinate WaferCenterOffset
        {
            get { return _WaferCenterOffset; }
            set
            {
                if (value != _WaferCenterOffset)
                {
                    _WaferCenterOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CardExist;
        public string CardExist
        {
            get { return _CardExist; }
            set
            {
                if (value != _CardExist)
                {
                    _CardExist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ThreeLeg;
        public string ThreeLeg
        {
            get { return _ThreeLeg; }
            set
            {
                if (value != _ThreeLeg)
                {
                    _ThreeLeg = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CardPodPosition;
        public string CardPodPosition
        {
            get { return _CardPodPosition; }
            set
            {
                if (value != _CardPodPosition)
                {
                    _CardPodPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _WaferOnChuck;
        public string WaferOnChuck
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

        private double _InputXValue;
        public double InputXValue
        {
            get { return _InputXValue; }
            set
            {
                if (value != _InputXValue)
                {
                    _InputXValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _InputYValue;
        public double InputYValue
        {
            get { return _InputYValue; }
            set
            {
                if (value != _InputYValue)
                {
                    _InputYValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _InputZValue;
        public double InputZValue
        {
            get { return _InputZValue; }
            set
            {
                if (value != _InputZValue)
                {
                    _InputZValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _InputThetaValue;
        public double InputThetaValue
        {
            get { return _InputThetaValue; }
            set
            {
                if (value != _InputThetaValue)
                {
                    _InputThetaValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public bool Initialized { get; set; } = false;
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
        private Autofac.IContainer _Container => this.GetLoaderContainer();
        public ILoaderModule loaderModule => _Container.Resolve<ILoaderModule>();
        public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();
        private IFoupOpModule Foup => _Container.Resolve<IFoupOpModule>();

        System.Timers.Timer _VacuumUpdateTimer;
        private static int VacUpdateInterValInms = 500;

        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
                DisplayPort = new DisplayPort() { GUID = new Guid("e5f0c0f9-b7c3-4b85-b611-4dd6163758ac") };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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
                }
            }
        }
        

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    this.VisionManager().SetDisplayChannel(null, DisplayPort);

                    XAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                    YAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ZAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    TAxis = this.MotionManager().GetAxis(EnumAxisConstants.C);
                    _VacuumUpdateTimer.Start();
                    InspectionLoaderDoor();

                    SelectedStage = _LoaderCommunicationManager.Cells[_LoaderCommunicationManager.SelectedStageIndex - 1];
                    
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(ret);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                _VacuumUpdateTimer.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            try
            {
                _VacuumUpdateTimer.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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
        public EventCodeEnum InitModule()
        {
            try
            {
                Lockobj = new object();
                WaferCenterOffset = new WaferCoordinate();
                EdgeAlignResult = "Unknown";
                _DoorResult = "Error";
                _VacuumUpdateTimer = new Timer(VacUpdateInterValInms);
                _VacuumUpdateTimer.Elapsed += _VacUpdateTimer_Elapsed;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        private void _VacUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    lock (Lockobj)
                    {
                        System.Threading.Thread.Sleep(500);
                        var vacdata = _RemoteMediumProxy.GPCC_OP_GetCCVacuumStatus();
                        if (vacdata != null)
                        {
                            if (vacdata.IsCardExistOnCardPod == true)
                            {
                                CardExist = "Exist";
                            }
                            else
                            {
                                CardExist = "Not Exist";
                            }

                            if (vacdata.IsThreeLegUp == true && vacdata.IsThreeLegDown == false)
                            {
                                ThreeLeg = "UP";
                            }
                            else if (vacdata.IsThreeLegUp == false && vacdata.IsThreeLegDown == true)
                            {
                                ThreeLeg = "DOWN";
                            }
                            else
                            {
                                ThreeLeg = "Check to Three Leg";
                            }

                            if (vacdata.IsLeftUpModuleUp == true && vacdata.IsRightUpModuleUp == true)
                            {
                                CardPodPosition = "UP";
                            }
                            else if (vacdata.IsLeftUpModuleUp == false && vacdata.IsRightUpModuleUp == false)
                            {
                                CardPodPosition = "DOWN";
                            }
                            else
                            {
                                CardPodPosition = "Check to card pod up module";
                            }

                            bool vac = false;
                            this.StageSupervisor().StageModuleState.ReadVacuum(out vac);
                            if (vac == true)
                                WaferOnChuck = "Wafer On Chuck";
                            else
                                WaferOnChuck = "No Wafer On Chuck";
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }


        #region ==> Move To Loading Pos Command
        private AsyncCommand _MoveToLoaderCommand;
        public IAsyncCommand MoveToLoaderCommand
        {
            get
            {
                if (null == _MoveToLoaderCommand) _MoveToLoaderCommand = new AsyncCommand(MoveToLoaderCommandFunc);
                return _MoveToLoaderCommand;
            }
        }
        private Task MoveToLoaderCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    double zoffset = 0;
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.MoveLoadingPosition(zoffset);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion


        #region ==> Move To Center Pos Command
        private AsyncCommand _MoveToCenterCommand;
        public IAsyncCommand MoveToCenterCommand
        {
            get
            {
                if (null == _MoveToCenterCommand) _MoveToCenterCommand = new AsyncCommand(MoveToCenterCommandFunc);
                return _MoveToCenterCommand;
            }
        }
        private Task MoveToCenterCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        EventCodeEnum ret = this.StageSupervisor().StageModuleState.ZCLEARED();
                        if (ret == EventCodeEnum.NONE)
                        {
                            this.StageSupervisor().StageModuleState.MoveToCenterPosition();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }
        #endregion


        #region ==> Move To Back Pos Command
        private AsyncCommand _MoveToBackCommand;
        public IAsyncCommand MoveToBackCommand
        {
            get
            {
                if (null == _MoveToBackCommand) _MoveToBackCommand = new AsyncCommand(MoveToBackCommandFunc);
                return _MoveToBackCommand;
            }
        }
        private Task MoveToBackCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        EventCodeEnum ret = this.StageSupervisor().StageModuleState.ZCLEARED();
                        if (ret == EventCodeEnum.NONE)
                        {
                            this.StageSupervisor().StageModuleState.MoveToBackPosition();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion


        #region ==> Move To Front Pos Command
        private AsyncCommand _MoveToFrontCommand;
        public IAsyncCommand MoveToFrontCommand
        {
            get
            {
                if (null == _MoveToFrontCommand) _MoveToFrontCommand = new AsyncCommand(MoveToFrontCommandFunc);
                return _MoveToFrontCommand;
            }
        }
        private Task MoveToFrontCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        EventCodeEnum ret = this.StageSupervisor().StageModuleState.ZCLEARED();
                        if (ret == EventCodeEnum.NONE)
                        {
                            this.StageSupervisor().StageModuleState.MoveToFrontPosition();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region ==> Move to zcleared pos
        private AsyncCommand _MoveToZClearedCommand;
        public IAsyncCommand MoveToZClearedCommand
        {
            get
            {
                if (null == _MoveToZClearedCommand) _MoveToZClearedCommand = new AsyncCommand(MoveToZClearedCommandFunc);
                return _MoveToZClearedCommand;
            }
        }
        private Task MoveToZClearedCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.ZCLEARED();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region ==> Three Leg Up
        private AsyncCommand _ThreeLegUpCommand;
        public IAsyncCommand ThreeLegUpCommand
        {
            get
            {
                if (null == _ThreeLegUpCommand) _ThreeLegUpCommand = new AsyncCommand(ThreeLegUpCommandFunc);
                return _ThreeLegUpCommand;
            }
        }
        private Task ThreeLegUpCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        bool isThreelegUp = false;
                        bool isThreelegDown = false;
                        this.MotionManager().IsThreeLegUp(EnumAxisConstants.TRI, ref isThreelegUp);
                        this.MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isThreelegDown);

                        if (isThreelegUp == false || isThreelegDown == true)
                        {
                            var retval = this.StageSupervisor().StageModuleState.ThreeLegUp();
                            if (retval == EventCodeEnum.NONE)
                            {
                                this.MetroDialogManager().ShowMessageDialog("Done", "Sucess", EnumMessageStyle.Affirmative);
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Faile", "Error occured wait for motion done", EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region ==> Three Leg Down
        private AsyncCommand _ThreeLegDownCommand;
        public IAsyncCommand ThreeLegDownCommand
        {
            get
            {
                if (null == _ThreeLegDownCommand) _ThreeLegDownCommand = new AsyncCommand(ThreeLegDownCommandFunc);
                return _ThreeLegDownCommand;
            }
        }
        private Task ThreeLegDownCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {

                        bool isThreelegUp = false;
                        bool isThreelegDown = false;
                        this.MotionManager().IsThreeLegUp(EnumAxisConstants.TRI, ref isThreelegUp);
                        this.MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isThreelegDown);

                        if (isThreelegUp == true || isThreelegDown == false)
                        {

                            var retval = this.StageSupervisor().StageModuleState.ThreeLegDown();
                            if (retval == EventCodeEnum.NONE)
                            {
                                this.MetroDialogManager().ShowMessageDialog("Done", "Sucess", EnumMessageStyle.Affirmative);
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Faile", "Error occured wait for motion done", EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region ==> RaisePodCommand
        private AsyncCommand _RaisePodCommand;
        public IAsyncCommand RaisePodCommand
        {
            get
            {
                if (null == _RaisePodCommand) _RaisePodCommand = new AsyncCommand(RaisePodCommandFunc);
                return _RaisePodCommand;
            }
        }
        private async Task RaisePodCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_OP_RaisePodCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> DropPodCommand
        private AsyncCommand _DropPodCommand;
        public IAsyncCommand DropPodCommand
        {
            get
            {
                if (null == _DropPodCommand) _DropPodCommand = new AsyncCommand(DropPodCommandFunc);
                return _DropPodCommand;
            }
        }
        private async Task DropPodCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_OP_DropPodCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PCardPodVacuumOffCommand
        private AsyncCommand _PCardPodVacuumOffCommand;
        public IAsyncCommand PCardPodVacuumOffCommand
        {
            get
            {
                if (null == _PCardPodVacuumOffCommand)
                {
                    _PCardPodVacuumOffCommand = new AsyncCommand(PCardPodVacuumOffCommandFunc);
                }

                return _PCardPodVacuumOffCommand;
            }
        }
        private async Task PCardPodVacuumOffCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_OP_PCardPodVacuumOffCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PCardPodVacuumOnCommand
        private AsyncCommand _PCardPodVacuumOnCommand;
        public IAsyncCommand PCardPodVacuumOnCommand
        {
            get
            {
                if (null == _PCardPodVacuumOnCommand) _PCardPodVacuumOnCommand = new AsyncCommand(PCardPodVacuumOnCommandFunc);
                return _PCardPodVacuumOnCommand;
            }
        }
        private async Task PCardPodVacuumOnCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_OP_PCardPodVacuumOnCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> ChuckVacOn
        private AsyncCommand _ChuckVacOnCommand;
        public IAsyncCommand ChuckVacOnCommand
        {
            get
            {
                if (null == _ChuckVacOnCommand) _ChuckVacOnCommand = new AsyncCommand(ChuckVacOnCommandFunc);
                return _ChuckVacOnCommand;
            }
        }
        private Task ChuckVacOnCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        var ret = this.StageSupervisor().StageModuleState.VacuumOnOff(true, extraVacReady: true);
                        if (ret == EventCodeEnum.NONE)
                        {
                            this.MetroDialogManager().ShowMessageDialog("Vacuum On", "Success", EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            this.MetroDialogManager().ShowMessageDialog("Vacuum On", "Faile", EnumMessageStyle.Affirmative);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion
        #region ==> ChuckVacOff
        private AsyncCommand _ChuckVacOffCommand;
        public IAsyncCommand ChuckVacOffCommand
        {
            get
            {
                if (null == _ChuckVacOffCommand) _ChuckVacOffCommand = new AsyncCommand(ChuckVacOffCommandFunc);
                return _ChuckVacOffCommand;
            }
        }
        private Task ChuckVacOffCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        var ret = this.StageSupervisor().StageModuleState.VacuumOnOff(false, extraVacReady: false);
                        if (ret == EventCodeEnum.NONE)
                        {
                            this.MetroDialogManager().ShowMessageDialog("Vacuum Off", "Success", EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            this.MetroDialogManager().ShowMessageDialog("Vacuum Off", "Faile", EnumMessageStyle.Affirmative);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region Edge Align

        private AsyncCommand _EdgeAlignCommand;
        public IAsyncCommand EdgeAlignCommand
        {
            get
            {
                if (null == _EdgeAlignCommand) _EdgeAlignCommand = new AsyncCommand(EdgeAlignCommandFunc);
                return _EdgeAlignCommand;
            }
        }
        private Task EdgeAlignCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        WaferCoordinate centeroffset = new WaferCoordinate();

                        var ret = this.StageSupervisor().StageModuleState.ZCLEARED();
                        bool exceed_limit = false;
                        double maximum_Value_X = 0.0;
                        double maximum_Value_Y = 0.0;

                        ret = this.WaferAligner().EdgeCheck(ref centeroffset, ref  maximum_Value_X, ref maximum_Value_Y);
                 
                        WaferCenterOffset.X.Value = centeroffset.GetX();
                        WaferCenterOffset.Y.Value = centeroffset.GetY();

                        if (Math.Abs(centeroffset.X.Value) > maximum_Value_X)
                        {
                            if (centeroffset.X.Value > 0)
                            {
                                centeroffset.X.Value = maximum_Value_X;
                            }
                            else
                            {
                                centeroffset.X.Value = -maximum_Value_X;
                            }
                            exceed_limit = true;
                            LoggerManager.Debug($"{this.GetType().Name}, Limit check. centeroffset X: { centeroffset.X.Value }, X Limit : {maximum_Value_X}");
                        }
                        if (Math.Abs(centeroffset.Y.Value) > maximum_Value_Y)
                        {
                            if (centeroffset.Y.Value > 0)
                            {
                                centeroffset.Y.Value = maximum_Value_Y;
                            }
                            else
                            {
                                centeroffset.Y.Value = -maximum_Value_Y;
                            }
                            exceed_limit = true;
                            LoggerManager.Debug($"{this.GetType().Name}, centeroffset Y: { centeroffset.Y.Value }, Y Limit : {maximum_Value_Y}");
                        }

                        var cellIdx = this._LoaderCommunicationManager.SelectedStage.Index;
                        var cellTO = this._LoaderCommunicationManager.SelectedStage.WaferObj;
                        int FoupIdx = -1;
                        if (cellTO.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            int slotNum = cellTO.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            FoupIdx = ((cellTO.OriginHolder.Index + offset) / 25) + 1;
                        }
                        bool shouldCheckCassetteType = false;
                        CassetteTypeEnum cassetteTypeEnum = CassetteTypeEnum.FOUP_25;
                        if (FoupIdx > 0) 
                        {
                            cassetteTypeEnum = Foup.FoupControllers[FoupIdx - 1].GetCassetteType();
                            shouldCheckCassetteType = true;
                        }

                        var slot_size = loaderModule.DeviceSize;
                        var chuckAccPos = loaderModule.SystemParameter.ChuckModules[cellIdx - 1].AccessParams.FirstOrDefault(i => i.SubstrateSize.Value == slot_size
                        && (!shouldCheckCassetteType || i.CassetteType.Value == cassetteTypeEnum));
                        var lx_pos = chuckAccPos.Position.LX.Value;
                        var lu_pos = chuckAccPos.Position.U.Value;
                                               
                        if (ret == EventCodeEnum.NONE)
                        {
                            EdgeAlignResult = "Success";
                            string limit_alarm_MSG = "";
                            if (exceed_limit)
                            {
                                limit_alarm_MSG = "※ Warning\nThis offset is applied as a maximum because it exceeds the maximum. \nIf correct calibration is desired, the wafer must be reloaded and Edge re-performed.\n" +
                                    $"Maximum Value X : {maximum_Value_X}, Y : {maximum_Value_Y}";
                            }
                            var retVal = this.MetroDialogManager().ShowMessageDialog("Update Message", $"Edge Align Success. Do you want to update the Loading Position?\n" +
                                $"{limit_alarm_MSG}\n" +
                                $"Wafer Center Offset X : {Math.Truncate(WaferCenterOffset.X.Value)}\n" +
                                $"Wafer Center Offset Y : {Math.Truncate(WaferCenterOffset.Y.Value)}\n" +
                                $"LX : {lx_pos} → {lx_pos - Math.Truncate(WaferCenterOffset.X.Value)}\n" +
                                $"LU : {lu_pos} → {lu_pos - Math.Truncate(WaferCenterOffset.Y.Value)}\n\n" +
                                $"※ If edge alignment continues after wafer loading, the cumulative edge offset value is applied to the loading position. The wafer must be reloaded before the next edge alignment to avoid incorrect values being applied.", EnumMessageStyle.AffirmativeAndNegative).Result;

                            if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                chuckAccPos.Position.LX.Value = lx_pos - Math.Truncate(WaferCenterOffset.X.Value);
                                chuckAccPos.Position.U.Value = lu_pos - Math.Truncate(WaferCenterOffset.Y.Value);
                                var save_ret = (loaderModule as LoaderModule).SaveSysParameter();
                                if (save_ret == EventCodeEnum.NONE)
                                {
                                    var update_ret = (loaderModule as LoaderModule).LoaderService.UpdateLoaderSystem(FoupIdx);
                                }
                            }
                        }
                        else
                        {
                            EdgeAlignResult = "Fail";
                        }
                        ret = this.StageSupervisor().StageModuleState.ZCLEARED();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion
        #region Move

        #region ==> X
        private AsyncCommand _MoveNegXCommand;
        public IAsyncCommand MoveNegXCommand
        {
            get
            {
                if (null == _MoveNegXCommand) _MoveNegXCommand = new AsyncCommand(MoveNegXCommandFunc);
                return _MoveNegXCommand;
            }
        }
        private Task MoveNegXCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        InputXValue = Math.Abs(InputXValue);
                        //this.MotionManager().RelMove(EnumAxisConstants.C, (InputThetaValue * -1));
                        var axisx = this.MotionManager().GetAxis(EnumAxisConstants.X);
                        if ((axisx.Status.Position.Actual + InputXValue) > axisx.Param.NegSWLimit.Value)
                        {
                            this.MotionManager().RelMove(EnumAxisConstants.X, (InputXValue * -1));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand _MovePosXCommand;
        public IAsyncCommand MovePosXCommand
        {
            get
            {
                if (null == _MovePosXCommand) _MovePosXCommand = new AsyncCommand(MovePosXCommandFunc);
                return _MovePosXCommand;
            }
        }
        private Task MovePosXCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {

                        InputXValue = Math.Abs(InputXValue);
                        var axisx = this.MotionManager().GetAxis(EnumAxisConstants.X);
                        if ((axisx.Status.Position.Actual + InputXValue) < axisx.Param.PosSWLimit.Value)
                        {
                            this.MotionManager().RelMove(EnumAxisConstants.X, InputXValue);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion
        #region ==> Y
        private AsyncCommand _MoveNegYCommand;
        public IAsyncCommand MoveNegYCommand
        {
            get
            {
                if (null == _MoveNegYCommand) _MoveNegYCommand = new AsyncCommand(MoveNegYCommandFunc);
                return _MoveNegYCommand;
            }
        }
        private Task MoveNegYCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        InputYValue = Math.Abs(InputYValue);
                        var axisy = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                        if ((axisy.Status.Position.Actual + InputYValue) > axisy.Param.NegSWLimit.Value)
                        {
                            this.MotionManager().RelMove(EnumAxisConstants.Y, (InputYValue * -1));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand _MovePosyCommand;
        public IAsyncCommand MovePosyCommand
        {
            get
            {
                if (null == _MovePosyCommand) _MovePosyCommand = new AsyncCommand(MovePosyCommandFunc);
                return _MovePosyCommand;
            }
        }
        private Task MovePosyCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        InputYValue = Math.Abs(InputYValue);
                        var axisy = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                        if ((axisy.Status.Position.Actual + InputYValue) < axisy.Param.PosSWLimit.Value)
                        {
                            this.MotionManager().RelMove(EnumAxisConstants.Y, InputYValue);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion
        #region ==> Z
        private AsyncCommand _MoveNegZCommand;
        public IAsyncCommand MoveNegZCommand
        {
            get
            {
                if (null == _MoveNegZCommand) _MoveNegZCommand = new AsyncCommand(MoveNegZCommandFunc);
                return _MoveNegZCommand;
            }
        }
        private Task MoveNegZCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        InputZValue = Math.Abs(InputZValue);
                        var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                        if ((axisz.Status.Position.Actual + InputZValue) > axisz.Param.NegSWLimit.Value)
                        {
                            this.MotionManager().RelMove(EnumAxisConstants.Z, (InputZValue * -1));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand _MovePosZCommand;
        public IAsyncCommand MovePosZCommand
        {
            get
            {
                if (null == _MovePosZCommand) _MovePosZCommand = new AsyncCommand(MovePosZCommandFunc);
                return _MovePosZCommand;
            }
        }
        private Task MovePosZCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        InputZValue = Math.Abs(InputZValue);
                        var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                        if ((axisz.Status.Position.Actual + InputZValue) < axisz.Param.PosSWLimit.Value)
                        {
                            this.MotionManager().RelMove(EnumAxisConstants.Z, InputZValue);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion
        #region ==>T
        private AsyncCommand _MoveNegThetaCommand;
        public IAsyncCommand MoveNegThetaCommand
        {
            get
            {
                if (null == _MoveNegThetaCommand) _MoveNegThetaCommand = new AsyncCommand(MoveNegThetaCommandFunc);
                return _MoveNegThetaCommand;
            }
        }
        private Task MoveNegThetaCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        InputThetaValue = Math.Abs(InputThetaValue);
                        var axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                        if ((axist.Status.Position.Actual + InputThetaValue) > axist.Param.NegSWLimit.Value)
                        {
                            this.MotionManager().RelMove(EnumAxisConstants.C, (InputThetaValue * -1));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        private AsyncCommand _MovePosThetaCommand;
        public IAsyncCommand MovePosThetaCommand
        {
            get
            {
                if (null == _MovePosThetaCommand) _MovePosThetaCommand = new AsyncCommand(MovePosThetaCommandFunc);
                return _MovePosThetaCommand;
            }
        }
        private Task MovePosThetaCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        InputThetaValue = Math.Abs(InputThetaValue);
                        var axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                        if ((axist.Status.Position.Actual + InputThetaValue) < axist.Param.PosSWLimit.Value)
                        {
                            this.MotionManager().RelMove(EnumAxisConstants.C, InputThetaValue);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #endregion

        #region ==> DoorOpenCommand
        private AsyncCommand _DoorOpenCommand;
        public IAsyncCommand DoorOpenCommand
        {
            get
            {
                if (null == _DoorOpenCommand) _DoorOpenCommand = new AsyncCommand(DoorOpenCommandFunc);
                return _DoorOpenCommand;
            }
        }
        private Task DoorOpenCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.LoaderDoorOpen();
                    }

                    InspectionLoaderDoor();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region ==> DoorCloseCommand
        private AsyncCommand _DoorCloseCommand;
        public IAsyncCommand DoorCloseCommand
        {
            get
            {
                if (null == _DoorCloseCommand) _DoorCloseCommand = new AsyncCommand(DoorCloseCommandFunc);
                return _DoorCloseCommand;
            }
        }
        private Task DoorCloseCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.LoaderDoorClose();
                    }
                    InspectionLoaderDoor();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        private void InspectionLoaderDoor()
        {
            bool isOpen = false;
            bool isClose = false;
            this.StageSupervisor().StageModuleState.IsLoaderDoorOpen(ref isOpen);
            this.StageSupervisor().StageModuleState.IsLoaderDoorClose(ref isClose);
            if (isOpen == true && isClose == false)
            {
                DoorResult = "Open";
            }
            else if (isOpen == false && isClose == true)
            {
                DoorResult = "Close";
            }
            else
            {
                DoorResult = "Error";
            }
        }
        #endregion

        #region ==> CardDoorOpenCommand
        private AsyncCommand _CardDoorOpenCommand;
        public IAsyncCommand CardDoorOpenCommand
        {
            get
            {
                if (null == _CardDoorOpenCommand) _CardDoorOpenCommand = new AsyncCommand(CardDoorOpenCommandFunc);
                return _CardDoorOpenCommand;
            }
        }
        private Task CardDoorOpenCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.CardDoorOpen();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region ==> CardDoorCloseCommand
        private AsyncCommand _CardDoorCloseCommand;
        public IAsyncCommand CardDoorCloseCommand
        {
            get
            {
                if (null == _CardDoorCloseCommand) _CardDoorCloseCommand = new AsyncCommand(CardDoorCloseCommandFunc);
                return _CardDoorCloseCommand;
            }
        }
        private Task CardDoorCloseCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative).Result;
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.CardDoorClose();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        private void InspectionCardDoor()
        {
            bool isOpen = false;
            bool isClose = false;
            isOpen = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorOpenCommand();
            isClose = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorCloseCommand();
            if (isOpen == true && isClose == false)
            {
                DoorResult = "Open";
            }
            else if (isOpen == false && isClose == true)
            {
                DoorResult = "Close";
            }
            else
            {
                DoorResult = "Error";
            }
        }
        #endregion
    }
}

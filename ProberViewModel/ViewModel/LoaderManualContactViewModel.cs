using System;
using System.Threading.Tasks;

namespace LoaderManualContactViewModelModule
{
    using Autofac;
    using LoaderBase.Communication;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;
    using ProbingModule;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using VirtualKeyboardControl;

    public class LoaderManualContactViewModel : IMainScreenViewModel, IManualContactControlVM, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

        private ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        private IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        public IManualContact MCM { get; set; }
        public IProbingModule ProbingModule { get; set; }

        private bool _IsVisiblePanel = true;
        public bool IsVisiblePanel
        {
            get { return _IsVisiblePanel; }
            set
            {
                if (value != _IsVisiblePanel)
                {
                    _IsVisiblePanel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _WantToMoveZInterval = 10;
        public double WantToMoveZInterval
        {
            get { return _WantToMoveZInterval; }
            set
            {
                if (value != _WantToMoveZInterval)
                {
                    _WantToMoveZInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CanUsingManualContactControl;
        public bool CanUsingManualContactControl
        {
            get { return _CanUsingManualContactControl; }
            set
            {
                if (value != _CanUsingManualContactControl)
                {
                    _CanUsingManualContactControl = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region //..Jog
        #region ==> PadJogLeft
        private PNPCommandButtonDescriptor _PadJogLeft = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogLeft
        {
            get { return _PadJogLeft; }
            set { _PadJogLeft = value; }
        }
        #endregion

        #region ==> PadJogRight
        private PNPCommandButtonDescriptor _PadJogRight = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogRight
        {
            get { return _PadJogRight; }
            set { _PadJogRight = value; }
        }
        #endregion

        #region ==> PadJogUp
        private PNPCommandButtonDescriptor _PadJogUp = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogUp
        {
            get { return _PadJogUp; }
            set { _PadJogUp = value; }
        }
        #endregion

        #region ==> PadJogDown
        private PNPCommandButtonDescriptor _PadJogDown = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogDown
        {
            get { return _PadJogDown; }
            set { _PadJogDown = value; }
        }
        #endregion

        #region ==> PadJogLeftUp
        private PNPCommandButtonDescriptor _PadJogLeftUp = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogLeftUp
        {
            get { return _PadJogLeftUp; }
            set { _PadJogLeftUp = value; }
        }
        #endregion

        #region ==> PadJogRightUp
        private PNPCommandButtonDescriptor _PadJogRightUp = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogRightUp
        {
            get { return _PadJogRightUp; }
            set { _PadJogRightUp = value; }
        }
        #endregion

        #region ==> PadJogLeftDown
        private PNPCommandButtonDescriptor _PadJogLeftDown = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogLeftDown
        {
            get { return _PadJogLeftDown; }
            set { _PadJogLeftDown = value; }
        }
        #endregion

        #region ==> PadJogRightDown
        private PNPCommandButtonDescriptor _PadJogRightDown = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogRightDown
        {
            get { return _PadJogRightDown; }
            set { _PadJogRightDown = value; }
        }
        #endregion

        #region ==> PadJogSelect
        private PNPCommandButtonDescriptor _PadJogSelect = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor PadJogSelect
        {
            get { return _PadJogSelect; }
            set { _PadJogSelect = value; }
        }
        #endregion

        #endregion

        #endregion

        #region //..IMainScreenViewModel 

        readonly Guid _ViewModelGUID = new Guid("0D97D8A3-AB55-CE78-A458-74101CD8950C");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;

        public LoaderManualContactViewModel()
        {

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

        public EventCodeEnum InitModule()
        {
            try
            {
                MCM = this.ManualContactModule();
                ProbingModule = this.ProbingModule();
                this.ProbingModule().InitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        private int _Tab_Idx;
        public int Tab_Idx
        {
            get { return _Tab_Idx; }
            set
            {
                if (value != _Tab_Idx)
                {
                    _Tab_Idx = value;
                    RaisePropertyChanged();
                }
            }
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftG.png");
                PadJogLeft.Command = new RelayCommand(DecreaseX);

                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightG.png");
                PadJogRight.Command = new RelayCommand(IncreaseX);

                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogUpG.png");
                PadJogUp.Command = new RelayCommand(IncreaseY);

                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogDownG.png");
                PadJogDown.Command = new RelayCommand(DecreaseY);

                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftUpG.png");
                PadJogLeftUp.Command = new RelayCommand(DecreaseXIncreaseY);

                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightUpG.png");
                PadJogRightUp.Command = new RelayCommand(IncreaseXIncreaseY);

                PadJogLeftDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftDownG.png");
                PadJogLeftDown.Command = new RelayCommand(DecreaseXDecreaseY);

                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightDownG.png");
                PadJogRightDown.Command = new RelayCommand(IncreaseXDecreaseY);
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

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            if (_LoaderCommunicationManager.SelectedStage != null)
            {
                //this.ManualContactModule().ViewTarget = this.StageSupervisor().WaferObject;
                this.StageSupervisor().WaferObject.MapViewCurIndexVisiablity = true;
                //_LoaderCommunicationManager.SelectedStage.StageInfo.WaferObject.MapViewCurIndexVisiablity = true;
                //this.ManualContactModule().ViewTarget = _LoaderCommunicationManager.SelectedStage.StageInfo.WaferObject;
                this.ManualContactModule().ViewTarget = this.StageSupervisor().WaferObject;
            }

            this.ManualContactModule().CoordinateManager = this.CoordinateManager();

            if (_RemoteMediumProxy == null)
            {
                await this.MetroDialogManager().ShowMessageDialog("Error Message", "Please use page after connection to Stage.", EnumMessageStyle.Affirmative);

                return EventCodeEnum.NONE;
            }

            AsyncHelpers.RunSync(() => _RemoteMediumProxy.ManualContactVM_PageSwitched());

            //_RemoteMediumProxy.PageSwitched(new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83"), parameter);

            GetManualContactInfo();
            if (MCM != null)
            {
                MCM.CPC_Visibility = Visibility.Hidden;
                Tab_Idx = 0;
            }

            SelectedStage = _LoaderCommunicationManager.Cells[_LoaderCommunicationManager.SelectedStageIndex - 1];

            return EventCodeEnum.NONE;
        }
        private void GetManualContactInfo()
        {
            try
            {
                var info = _RemoteMediumProxy.GetManualContactInfo();
                if (info != null)
                {
                    CanUsingManualContactControl = info.CanUsingManualContactControl;
                    IsVisiblePanel = info.IsVisiblePanel;
                    WantToMoveZInterval = info.WantToMoveZInterval;

                    this.ManualContactModule().ChangeOverDrive(info.ManualContactModuleOverDrive.ToString());

                    this.ManualContactModule().ChangeCPC_Z1(info.CPC_Z1.ToString());
                    this.ManualContactModule().ChangeCPC_Z2(info.CPC_Z2.ToString());
                    //if (info.ManualCotactModuleIsZUpState)
                    //{
                    //    this.ManualContactModule().ChangeToZUpState();
                    //}
                    //else
                    //{
                    //    this.ManualContactModule().ChangeToZDownState();
                    //}

                    this.ProbingModule().FirstContactHeight = info.ProbingModuleFirstContactHeight;
                    this.ProbingModule().AllContactHeight = info.ProbingModuleAllContactHeight;
                    (this.ProbingModule().ProbingModuleSysParam_IParam as ProbingModuleSysParam).OverDriveStartPosition.Value = info.ProbingModuleOverDriveStartPosition;
                    //this.ManualContactModule().MXYIndex = info.ManualContactModuleXYIndex;
                    //this.ManualContactModule().MachinePosition = info.ManualContactModuleMachinePosition;

                    this.ManualContactModule().IsZUpState = info.IsZUpState;

                    //if (this.ManualContactModule().IsZUpState)
                    //{
                    //    this.ManualContactModule().ChangeToZUpState();
                    //}
                    //else
                    //{
                    //    this.ManualContactModule().ChangeToZDownState();
                    //}

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
                if (_LoaderCommunicationManager.SelectedStage != null)
                {
                    AsyncHelpers.RunSync(() => _RemoteMediumProxy.ManualContactVM_Cleanup());
                    this.StageSupervisor().WaferObject.MapViewCurIndexVisiablity = false;
                    //_LoaderCommunicationManager.SelectedStage.StageInfo.WaferObject.MapViewCurIndexVisiablity = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        #region //..Command & Command Method

        #region //.. Jog

        private void IncreaseY()
        {
            try
            {
                MCM.IncreaseY();
                GetManualContactInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DecreaseY()
        {
            try
            {
                MCM.DecreaseY();
                GetManualContactInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void IncreaseX()
        {
            try
            {
                MCM.IncreaseX();
                GetManualContactInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DecreaseX()
        {
            try
            {
                MCM.DecreaseX();
                GetManualContactInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void IncreaseXIncreaseY()
        {
            try
            {
                //MCM.IncreaseX();
                //MCM.IncreaseY();
                MCM.SetIndex(EnumMovingDirection.RIGHT, EnumMovingDirection.UP);
                GetManualContactInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DecreaseXIncreaseY()
        {
            try
            {
                //MCM.DecreaseX();
                //MCM.IncreaseY();
                MCM.SetIndex(EnumMovingDirection.LEFT, EnumMovingDirection.UP);
                GetManualContactInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void IncreaseXDecreaseY()
        {
            try
            {
                //MCM.IncreaseX();
                //MCM.DecreaseY();
                MCM.SetIndex(EnumMovingDirection.RIGHT, EnumMovingDirection.DOWN);
                GetManualContactInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DecreaseXDecreaseY()
        {
            try
            {
                //MCM.DecreaseX();
                //MCM.DecreaseY();
                MCM.SetIndex(EnumMovingDirection.LEFT, EnumMovingDirection.DOWN);
                GetManualContactInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        private AsyncCommand _ControlLoadedCommand;
        public IAsyncCommand ControlLoadedCommand
        {
            get
            {
                if (null == _ControlLoadedCommand)
                    _ControlLoadedCommand = new AsyncCommand(ControlLoaded);
                return _ControlLoadedCommand;
            }
        }
        private async Task ControlLoaded()
        {
        }

        private AsyncCommand _FirstContactSetCommand;
        public IAsyncCommand FirstContactSetCommand
        {
            get
            {
                if (null == _FirstContactSetCommand) _FirstContactSetCommand = new AsyncCommand(FirstContactSet);
                return _FirstContactSetCommand;
            }
        }
        private async Task FirstContactSet()
        {
            try
            {
                if(_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ManualContact_FirstContactSetCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private RelayCommand _FirstContactSetCommand;
        //public ICommand FirstContactSetCommand
        //{
        //    get
        //    {
        //        if (null == _FirstContactSetCommand) _FirstContactSetCommand = new RelayCommand(FirstContactSet);
        //        return _FirstContactSetCommand;
        //    }
        //}
        //private async void FirstContactSet()
        //{
        //    try
        //    {
        //        _RemoteMediumProxy.ManualContact_FirstContactSetCommand();
        //        GetManualContactInfo();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private AsyncCommand _AllContactSetCommand;
        public IAsyncCommand AllContactSetCommand
        {
            get
            {
                if (null == _AllContactSetCommand) _AllContactSetCommand = new AsyncCommand(AllContactSet);
                return _AllContactSetCommand;
            }
        }
        private async Task AllContactSet()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ManualContact_AllContactSetCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ResetContactStartPositionCommand;
        public IAsyncCommand ResetContactStartPositionCommand
        {
            get
            {
                if (null == _ResetContactStartPositionCommand)
                    _ResetContactStartPositionCommand = new AsyncCommand(ResetContactStartPosition);
                return _ResetContactStartPositionCommand;
            }
        }

        private async Task ResetContactStartPosition()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ManualContact_ResetContactStartPositionCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _OverDriveTBClickCommand;
        public IAsyncCommand OverDriveTBClickCommand
        {
            get
            {
                if (null == _OverDriveTBClickCommand) _OverDriveTBClickCommand = new AsyncCommand(OverDriveTBClick);
                return _OverDriveTBClickCommand;
            }
        }

        private async Task OverDriveTBClick()
        {
            try
            {
                // TODO : Virtual keyboard를 사용하기 위해, 직접 호출하고, Set하자.

                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(MCM.OverDrive.ToString());

                MCMChangeOverDrive(retVal);

                GetManualContactInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _CPC_Z1_ClickCommand;
        public IAsyncCommand CPC_Z1_ClickCommand
        {
            get
            {
                if (null == _CPC_Z1_ClickCommand) _CPC_Z1_ClickCommand = new AsyncCommand(CPC_Z1_Click);
                return _CPC_Z1_ClickCommand;
            }
        }

        private async Task CPC_Z1_Click()
        {
            try
            {
                // TODO : Virtual keyboard를 사용하기 위해, 직접 호출하고, Set하자.

                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(MCM.CPC_Z1.ToString());

                double value = 0;
                var ret = ParsingToDouble(retVal, out value);
                if (ret)
                {
                    if (Math.Abs(value) <= 45)
                    {
                        MCMChangeCPC_Z1(retVal);
                        GetManualContactInfo();
                    }
                    else
                    {
                        LoggerManager.Debug($"CPC_Z1 Not Change. Value is {value}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _CPC_Z2_ClickCommand;
        public IAsyncCommand CPC_Z2_ClickCommand
        {
            get
            {
                if (null == _CPC_Z2_ClickCommand) _CPC_Z2_ClickCommand = new AsyncCommand(CPC_Z2_Click);
                return _CPC_Z2_ClickCommand;
            }
        }

        private async Task CPC_Z2_Click()
        {
            try
            {
                // TODO : Virtual keyboard를 사용하기 위해, 직접 호출하고, Set하자.

                string retVal = null;

                retVal = GetInputValueFromVirtualKeyboard(MCM.CPC_Z2.ToString());
                double value = 0;
                var ret = ParsingToDouble(retVal,out value);
                if (ret)
                {
                    if (Math.Abs(value) <= 45)
                    {
                        MCMChangeCPC_Z2(retVal);
                        GetManualContactInfo();
                    }
                    else
                    {
                        LoggerManager.Debug($"CPC_Z2 Not Change. Value is {value}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void MCMChangeOverDrive(string overdrive)
        {
            try
            {
                _RemoteMediumProxy.MCMChangeOverDrive(overdrive);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool ParsingToDouble(string parseData, out double parseValue)
        {
            bool retVal = false;
            parseValue = 0;
            if (!string.IsNullOrEmpty(parseData))
            {
                retVal = double.TryParse(parseData, out parseValue);
            }

            return retVal;
        }
        public void MCMChangeCPC_Z1(string Z1)
        {
            try
            {
                _RemoteMediumProxy.MCMChangeCPC_Z1(Z1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void MCMChangeCPC_Z2(string Z2)
        {
            try
            {
                _RemoteMediumProxy.MCMChangeCPC_Z2(Z2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string GetInputValueFromVirtualKeyboard(string curValue)
        {
            string retString = null;

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    retString = VirtualKeyboard.Show(WindowLocationType.BOTTOM, curValue);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retString;
        }

        private AsyncCommand _ChangeZUpStateCommand;
        public IAsyncCommand ChangeZUpStateCommand
        {
            get
            {
                if (null == _ChangeZUpStateCommand)
                    _ChangeZUpStateCommand = new AsyncCommand(ChangeZUpStateFunc);
                return _ChangeZUpStateCommand;
            }
        }
        private async Task ChangeZUpStateFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ManualContact_ChangeZUpStateCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SetOverDriveCommand;
        public IAsyncCommand SetOverDriveCommand
        {
            get
            {
                if (null == _SetOverDriveCommand) _SetOverDriveCommand = new AsyncCommand(SetOverDrive);
                return _SetOverDriveCommand;
            }
        }
        private async Task SetOverDrive()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ManualContact_SetOverDriveCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _MoveToWannaZIntervalMinusCommand;
        public IAsyncCommand MoveToWannaZIntervalMinusCommand
        {
            get
            {
                if (null == _MoveToWannaZIntervalMinusCommand)
                    _MoveToWannaZIntervalMinusCommand = new AsyncCommand(MoveToWannaZIntervalMinus);
                return _MoveToWannaZIntervalMinusCommand;
            }
        }
        private async Task MoveToWannaZIntervalMinus()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ManualContact_MoveToWannaZIntervalMinusCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _MoveToWannaZIntervalPlusCommand;
        public IAsyncCommand MoveToWannaZIntervalPlusCommand
        {
            get
            {
                if (null == _MoveToWannaZIntervalPlusCommand)
                    _MoveToWannaZIntervalPlusCommand = new AsyncCommand(MoveToWannaZIntervalPlus);
                return _MoveToWannaZIntervalPlusCommand;
            }
        }

        private async Task MoveToWannaZIntervalPlus()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ManualContact_MoveToWannaZIntervalPlusCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _WantToMoveZIntervalTBClickCommand;
        public IAsyncCommand WantToMoveZIntervalTBClickCommand
        {
            get
            {
                if (null == _WantToMoveZIntervalTBClickCommand) _WantToMoveZIntervalTBClickCommand = new AsyncCommand(WantToMoveZIntervalTBClick);
                return _WantToMoveZIntervalTBClickCommand;
            }
        }
        private async Task WantToMoveZIntervalTBClick()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.ManualContact_WantToMoveZIntervalTBClickCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _GoToInspectionViewCommand;
        public IAsyncCommand GoToInspectionViewCommand
        {
            get
            {
                if (null == _GoToInspectionViewCommand) _GoToInspectionViewCommand = new AsyncCommand(GoToInspectionView);
                return _GoToInspectionViewCommand;
            }
        }

        public InitPriorityEnum InitPriority => throw new NotImplementedException();

        private async Task GoToInspectionView()
        {
            try
            {
                this.StageSupervisor().StageModuleState.ZCLEARED();

                await this.ViewModelManager().ViewTransitionAsync(new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1"));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            throw new NotImplementedException();
        }

        public ManaulContactDataDescription GetManaulContactSetupDataDescription()
        {
            return null;
        }

        public bool GetManaulContactMovingStage()
        {
            throw new NotImplementedException();
        }

        public Task FirstContactSetRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task AllContactSetRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task ResetContactStartPositionRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task OverDriveTBClickRemoteCommand()
        {
            throw new NotImplementedException();
        }
        public Task CPC_Z1_ClickRemoteCommand()
        {
            throw new NotImplementedException();
        }
        public Task CPC_Z2_ClickRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task ChangeZUpStateRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task MoveToWannaZIntervalPlusRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task WantToMoveZIntervalTBClickRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task SetOverDriveRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task MoveToWannaZIntervalMinusRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public void GetOverDriveFromProbingModule()
        {
            throw new NotImplementedException();
        }



        #endregion
    }
}

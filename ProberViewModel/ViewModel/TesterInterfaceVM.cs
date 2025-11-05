namespace ProberViewModel.ViewModel
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.Communication;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class TesterInterfaceVM : INotifyPropertyChanged, IFactoryModule, IMainScreenViewModel
    {
        #region <remarks> PropertyChanged </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
        #region <remarks> Property </remarks>
        readonly Guid _ViewModelGUID = new Guid("1C933AF2-F0F0-4235-80A8-D202A9CEF2F7");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;
        #endregion
        #region <remarks> IMainScreenViewModel Method </remarks>

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var vacstatus = _RemoteMediumProxy.GPCC_OP_GetCCVacuumStatus();
                    this.TesterPogoTouched = vacstatus.TesterPogoTouched;
                    this.CardPogoTouched = vacstatus.CardPogoTouched;

                    if (VacUpdateThread.ThreadState == ThreadState.Unstarted)
                    {
                        VacUpdateThread.Start();
                    }
                    else if (VacUpdateThread.ThreadState == ThreadState.Suspended)
                    {
                        VacUpdateThread.Resume();
                    }
                }
                SelectedStage = _LoaderCommunicationManager.Cells[_LoaderCommunicationManager.SelectedStageIndex - 1];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    VacUpdateThread.Suspend();
                    _RemoteMediumProxy.GPCC_OP_UpPlateTesterPurgeAirOffCommand();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        Thread VacUpdateThread;
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                VacUpdateThread = new Thread(new ThreadStart(VacUpdateData));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion

        private bool _TesterPogoTouched;
        public bool TesterPogoTouched
        {
            get { return _TesterPogoTouched; }
            set
            {
                if (value != _TesterPogoTouched)
                {
                    _TesterPogoTouched = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CardPogoTouched;
        public bool CardPogoTouched
        {
            get { return _CardPogoTouched; }
            set
            {
                if (value != _CardPogoTouched)
                {
                    _CardPogoTouched = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private bool _TesterClamped;
        //public bool TesterClamped
        //{
        //    get { return _TesterClamped; }
        //    set
        //    {
        //        if (value != _TesterClamped)
        //        {
        //            _TesterClamped = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private void VacUpdateData()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(2000);
                    var vacdata = _RemoteMediumProxy.GPCC_OP_GetCCVacuumStatus();
                    if (vacdata != null)
                    {
                        CardPogoTouched = vacdata.CardPogoTouched;
                        TesterPogoTouched = vacdata.TesterPogoTouched;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();

        private AsyncCommand _DockTesterCommand;
        public IAsyncCommand DockTesterCommand
        {
            get
            {
                if (null == _DockTesterCommand) _DockTesterCommand = new AsyncCommand(DockTesterCommandFunc);
                return _DockTesterCommand;
            }
        }
        private async Task DockTesterCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                    {
                        if (stageBusy == false)
                        {
                            await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            await _RemoteMediumProxy.GPCC_OP_UpPlateTesterPurgeAirOffCommand();
                            await _RemoteMediumProxy.GPCC_OP_UpPlateTesterContactVacuumOnCommand();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _UnDockTesterCommand;
        public IAsyncCommand UnDockTesterCommand
        {
            get
            {
                if (null == _UnDockTesterCommand) _UnDockTesterCommand = new AsyncCommand(UnDockTesterCommandFunc);
                return _UnDockTesterCommand;
            }
        }
        private async Task UnDockTesterCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                    {
                        if (stageBusy == false)
                        {
                            await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            await _RemoteMediumProxy.GPCC_OP_UpPlateTesterCOfftactVacuumOffCommand();
                            await _RemoteMediumProxy.GPCC_OP_UpPlateTesterPurgeAirOnCommand();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

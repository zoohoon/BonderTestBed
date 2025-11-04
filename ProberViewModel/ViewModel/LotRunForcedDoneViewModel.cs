using AccountModule;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ForcedDoneViewModel
{
    public class LotRunModule : INotifyPropertyChanged, IFactoryModule, IModule
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

        private IStateModule _RunModule;
        public IStateModule RunModule
        {
            get { return _RunModule; }
            set
            {
                if (value != _RunModule)
                {
                    _RunModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _RunModuleName;
        public string RunModuleName
        {
            get { return _RunModuleName; }
            set
            {
                if (value != _RunModuleName)
                {
                    _RunModuleName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ForcedDoneFlag;
        public bool ForcedDoneFlag
        {
            get { return _ForcedDoneFlag; }
            set
            {
                if (value != _ForcedDoneFlag)
                {
                    _ForcedDoneFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ProbingDryRunFlag;
        public bool ProbingDryRunFlag
        {
            get { return _ProbingDryRunFlag; }
            set
            {
                if (value != _ProbingDryRunFlag)
                {
                    _ProbingDryRunFlag = value;
                    this.ProbingModule().ProbingDryRunFlag = _ProbingDryRunFlag;
                    RaisePropertyChanged();
                }
            }
        }




        private EnumModuleForcedState _ModuleForcedState = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ModuleForcedState
        {
            get { return _ModuleForcedState; }
            set
            {
                if (value != _ModuleForcedState)
                {
                    _ModuleForcedState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LabelColor = "Green";
        public string LabelColor
        {
            get { return _LabelColor; }
            set
            {
                if (value != _LabelColor)
                {
                    _LabelColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _ModuleForcedDoneOnCommand;
        public ICommand ModuleForcedDoneOnCommand
        {
            get
            {
                if (null == _ModuleForcedDoneOnCommand) _ModuleForcedDoneOnCommand = new AsyncCommand(ModuleForcedDoneOn);
                return _ModuleForcedDoneOnCommand;
            }
        }
        private async Task ModuleForcedDoneOn()
        {
            try
            {
                LabelColor = "DeepPink";
                ModuleForcedState = EnumModuleForcedState.ForcedDone;
                RunModule.ForcedDone = EnumModuleForcedState.ForcedDone;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ModuleForcedDoneOffCommand;
        public ICommand ModuleForcedDoneOffCommand
        {
            get
            {
                if (null == _ModuleForcedDoneOffCommand) _ModuleForcedDoneOffCommand = new AsyncCommand(ModuleForcedDoneOff);
                return _ModuleForcedDoneOffCommand;
            }
        }
        private async Task ModuleForcedDoneOff()
        {
            try
            {
                LabelColor = "Green";
                ModuleForcedState = EnumModuleForcedState.Normal;
                RunModule.ForcedDone = EnumModuleForcedState.Normal;
                ForcedDoneFlag = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand _ModuleForcedDoneHalfCommand;
        public ICommand ModuleForcedDoneHalfCommand
        {
            get
            {
                if (null == _ModuleForcedDoneHalfCommand) _ModuleForcedDoneHalfCommand = new AsyncCommand(ModuleForcedDoneHalf);
                return _ModuleForcedDoneHalfCommand;
            }
        }
        private async Task ModuleForcedDoneHalf()
        {
            try
            {
                LabelColor = "Red";
                ModuleForcedState = EnumModuleForcedState.ForcedRunningAndDone;
                RunModule.ForcedDone = EnumModuleForcedState.ForcedRunningAndDone;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DeInitModule()
        {
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {

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
    }
    public class LotRunForcedDoneViewModel : IMainScreenViewModel, INotifyPropertyChanged
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

        readonly Guid _ViewModelGUID = new Guid("0E719D6C-C283-4643-9AFB-B2C1465892C8");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        private ObservableCollection<LotRunModule> _LotRunList;

        public ObservableCollection<LotRunModule> LotRunList
        {
            get
            {
                return _LotRunList;
            }
            set
            {
                if (value != _LotRunList)
                {
                    _LotRunList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurrentUserLevel;
        public int CurrentUserLevel
        {
            get { return _CurrentUserLevel; }
            set
            {
                if (value != _CurrentUserLevel)
                {
                    _CurrentUserLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ExecuteEngineerModeFalg;
        public bool ExecuteEngineerModeFalg
        {
            get { return _ExecuteEngineerModeFalg; }
            set
            {
                if (value != _ExecuteEngineerModeFalg)
                {
                    _ExecuteEngineerModeFalg = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _WaferOnChuckExistFalg;
        public bool WaferOnChuckExistFalg
        {
            get { return _WaferOnChuckExistFalg; }
            set
            {
                if (value != _WaferOnChuckExistFalg)
                {
                    _WaferOnChuckExistFalg = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                foreach (var item in LotRunList)
                {
                    if(item.RunModule.ForcedDone == EnumModuleForcedState.Normal)
                    {
                        item.ForcedDoneFlag = false;
                        item.ModuleForcedState = EnumModuleForcedState.Normal;
                        item.LabelColor = "Green";
                    }
                }

                //LotRunList[i].RunModule.ForcedDone = EnumModuleForcedState.ForcedDone;
                //LotRunList[i].ForcedDoneFlag = true;
                //LotRunList[i].ModuleForcedState = EnumModuleForcedState.ForcedDone;
                //LotRunList[i].LabelColor = "DeepPink";

                if (Extensions_IParam.ProberExecuteMode == ExecuteMode.DEFAULT)
                    ExecuteEngineerModeFalg = false;
                else
                    ExecuteEngineerModeFalg = true;

                if (this.GetParam_Wafer().WaferStatus == EnumSubsStatus.EXIST)
                    WaferOnChuckExistFalg = true;
                else
                    WaferOnChuckExistFalg = false;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                foreach (LotRunModule item in LotRunList)
                {
                    //item.ModuleForcedDoneOffCommand.Execute(null);
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

        public void ChangedAccount()
        {
            CurrentUserLevel = AccountManager.CurrentUserInfo.UserLevel;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    LotRunList = new ObservableCollection<LotRunModule>();

                    foreach (IStateModule item in this.LotOPModule().RunList)
                    {
                        var runmodule = new LotRunModule();
                        runmodule.RunModule = item;

                        if(runmodule.RunModule != null)
                        {
                            runmodule.RunModuleName = runmodule.RunModule.GetType().Name;
                        }
                        
                        LotRunList.Add(runmodule);
                    }

                    ////// GP TEST

                    ////for (int i = 0; i < LotRunList.Count() - 2; i++)
                    ////{
                    ////    LotRunList[i].RunModule.ForcedDone = EnumModuleForcedState.ForcedDone;
                    ////    LotRunList[i].ForcedDoneFlag = true;
                    ////    LotRunList[i].ModuleForcedState = EnumModuleForcedState.ForcedDone;
                    ////    LotRunList[i].LabelColor = "DeepPink";
                    ////}
                    AccountManager.accountChangedDelegate += ChangedAccount;

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


        #region //.. 부가 설정 기능들
        private AsyncCommand _ExuteEngineerModeOnCommand;
        public ICommand ExuteEngineerModeOnCommand
        {
            get
            {
                if (null == _ExuteEngineerModeOnCommand) _ExuteEngineerModeOnCommand = new AsyncCommand(ExuteEngineerModeOnFunc);
                return _ExuteEngineerModeOnCommand;
            }
        }
        private async Task ExuteEngineerModeOnFunc()
        {
            try
            {
                Extensions_IParam.ProberExecuteMode = ExecuteMode.ENGINEER;
                ExecuteEngineerModeFalg = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private AsyncCommand _ExuteEngineerModeOffCommand;
        public ICommand ExuteEngineerModeOffCommand
        {
            get
            {
                if (null == _ExuteEngineerModeOffCommand) _ExuteEngineerModeOffCommand = new AsyncCommand(ExuteEngineerModeOffFunc);
                return _ExuteEngineerModeOffCommand;
            }
        }
        private async Task ExuteEngineerModeOffFunc()
        {
            try
            {
                Extensions_IParam.ProberExecuteMode = ExecuteMode.DEFAULT;
                ExecuteEngineerModeFalg = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private AsyncCommand _WaferOnChuckExistCommand;
        public ICommand WaferOnChuckExistCommand
        {
            get
            {
                if (null == _WaferOnChuckExistCommand) _WaferOnChuckExistCommand = new AsyncCommand(WaferOnChuckExistFunc);
                return _WaferOnChuckExistCommand;
            }
        }
        private async Task WaferOnChuckExistFunc()
        {
            try
            {
                this.MonitoringManager().SkipCheckChuckVacuumFlag = true;

                this.GetParam_Wafer().SetWaferStatus(EnumSubsStatus.EXIST, EnumWaferType.STANDARD, "", 0);
                this.GetParam_Wafer().SetWaferState(EnumWaferState.UNPROCESSED);

                WaferOnChuckExistFalg = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _WaferOnChuckNotExistCommand;
        public ICommand WaferOnChuckNotExistCommand
        {
            get
            {
                if (null == _WaferOnChuckNotExistCommand) _WaferOnChuckNotExistCommand = new AsyncCommand(WaferOnChuckNotExistFunc);
                return _WaferOnChuckNotExistCommand;
            }
        }
        private async Task WaferOnChuckNotExistFunc()
        {
            try
            {
                this.GetParam_Wafer().SetWaferStatus(EnumSubsStatus.NOT_EXIST);
                WaferOnChuckExistFalg = false;

                this.MonitoringManager().SkipCheckChuckVacuumFlag = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _ProbingDryRunTrueCommand;
        public ICommand ProbingDryRunTrueCommand
        {
            get
            {
                if (null == _ProbingDryRunTrueCommand) _ProbingDryRunTrueCommand = new AsyncCommand(ProbingDryRunTrueFunc);
                return _ProbingDryRunTrueCommand;
            }
        }
        private async Task ProbingDryRunTrueFunc()
        {
            try
            {
                this.ProbingModule().ProbingDryRunFlag = true;
                LoggerManager.Debug($"[Probing Module] ProbingDryRunFlag set to {this.ProbingModule().ProbingDryRunFlag}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ProbingDryRunFalseCommand;
        public ICommand ProbingDryRunFalseCommand
        {
            get
            {
                if (null == _ProbingDryRunFalseCommand) _ProbingDryRunFalseCommand = new AsyncCommand(ProbingDryRunFalseFunc);
                return _ProbingDryRunFalseCommand;
            }
        }
        private async Task ProbingDryRunFalseFunc()
        {
            try
            {
                this.ProbingModule().ProbingDryRunFlag = false;
                LoggerManager.Debug($"[Probing Module] ProbingDryRunFlag set to {this.ProbingModule().ProbingDryRunFlag}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

    }
}

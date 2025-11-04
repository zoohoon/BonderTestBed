namespace ProberViewModel.ViewModel
{
    using FoupReoveryViewModel;
    using LoaderBase;
    using LoaderCore;
    using LoaderParameters.Data;
    using LogModule;
    using MetroDialogInterfaces;
    using NotifyEventModule;
    using ProberInterfaces;
    using ProberInterfaces.Event;
    using ProberInterfaces.Foup;
    using RelayCommandBase;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using VirtualKeyboardControl;

    public class FoupSubSettingViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        private ICassetteModule _CassetteModule;
        public ICassetteModule CassetteModule
        {
            get { return _CassetteModule; }
            set
            {
                if (value != _CassetteModule)
                {
                    _CassetteModule = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ActiveLotInfo _FoupActiveLotInfo;
        public ActiveLotInfo FoupActiveLotInfo
        {
            get { return _FoupActiveLotInfo; }
            set
            {
                if (value != _FoupActiveLotInfo)
                {
                    _FoupActiveLotInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupStateEnum _FoupState;
        public FoupStateEnum FoupState
        {
            get { return _FoupState; }
            set
            {
                if (value != _FoupState)
                {
                    _FoupState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Window PWindow { get; set; }

        public FoupSubSettingViewModel(ILoaderSupervisor loaderSupervisor, IFoupController foupController, Window window)
        {
            try
            {
                LoaderMaster = loaderSupervisor;
                FoupController = foupController;
                FoupIndex = foupController.FoupModuleInfo.FoupNumber;
                FoupState = foupController.FoupModuleInfo.State;
                PWindow = window;
                if(LoaderMaster != null)
                {
                    if(LoaderMaster.ActiveLotInfos.Count >0 && LoaderMaster.ActiveLotInfos.Count >= FoupController.FoupModuleInfo.FoupNumber)
                    {
                        FoupActiveLotInfo = LoaderMaster.ActiveLotInfos[FoupController.FoupModuleInfo.FoupNumber - 1];
                    }
                    CassetteModule = LoaderMaster.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, FoupController.FoupModuleInfo.FoupNumber);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _FoupChangEnableCommand;
        public ICommand FoupChangEnableCommand
        {
            get
            {
                if (null == _FoupChangEnableCommand) _FoupChangEnableCommand = new RelayCommand<object>(FoupChangEnableCommandFunc);
                return _FoupChangEnableCommand;
            }
        }

        private void FoupChangEnableCommandFunc(object param)
        {
            try
            {
                LoaderModule loaderModule = (LoaderMaster.Loader as LoaderModule);
                if(loaderModule != null)
                {
                    loaderModule.SystemParameter.CassetteModules[FoupController.FoupModuleInfo.FoupNumber - 1].Enable.Value
                         = (bool)param;
                    loaderModule.SaveSysParameter();
                    loaderModule.SetModuleEnable();

                    LoggerManager.Debug("Loader Module SaveSystem");

                    loaderModule.LoaderMapUpdate();

                    FoupModeStatusEnum foupstate = (bool)param ? FoupModeStatusEnum.ONLINE : FoupModeStatusEnum.OFFLINE;

                    this.FoupOpModule().SetFoupModeStatus(FoupController.FoupModuleInfo.FoupNumber, foupstate);
                    this.FoupOpModule().SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _FoupChangModeCommand;
        public ICommand FoupChangModeCommand
        {
            get
            {
                if (null == _FoupChangModeCommand) _FoupChangModeCommand = new RelayCommand<object>(FoupChangModeCommandFunc);
                return _FoupChangModeCommand;
            }
        }

        private void FoupChangModeCommandFunc(object param)
        {
            try
            {
                this.FoupOpModule().SetFoupModeStatus(FoupController.FoupModuleInfo.FoupNumber, (FoupModeStatusEnum)param);
                this.FoupOpModule().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DYFoupStateChangeCommand;
        public ICommand DYFoupStateChangeCommand
        {
            get
            {
                if (null == _DYFoupStateChangeCommand) _DYFoupStateChangeCommand = new RelayCommand<object>(DYFoupStateChangeCommandFunc);
                return _DYFoupStateChangeCommand;
            }
        }

        private void DYFoupStateChangeCommandFunc(object param)
        {
            try
            {
                DynamicFoupStateEnum stateEnum = (DynamicFoupStateEnum) param;
                FoupActiveLotInfo.DynamicFoupState = stateEnum;

                if (LoaderMaster.Loader.Foups.Count > 0 && LoaderMaster.Loader.Foups.Count >= FoupController.FoupModuleInfo.FoupNumber)
                {
                    LoaderMaster.Loader.Foups[FoupController.FoupModuleInfo.FoupNumber - 1].DynamicFoupState = stateEnum;
                    if(stateEnum == DynamicFoupStateEnum.LOAD_AND_UNLOAD)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupController.FoupModuleInfo.FoupNumber);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupStateChangedToLoadAndUnloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                    else if(stateEnum == DynamicFoupStateEnum.UNLOAD)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupController.FoupModuleInfo.FoupNumber);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupStateChangedToUnloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                }

                LoggerManager.Debug($"#{FoupController.FoupModuleInfo.FoupNumber}Foup State Mode Changed to {stateEnum}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _EntryFoupRecoveryPageCommand;
        public ICommand EntryFoupRecoveryPageCommand
        {
            get
            {
                if (null == _EntryFoupRecoveryPageCommand) _EntryFoupRecoveryPageCommand = new RelayCommand(EntryFoupRecoveryPageFunc);
                return _EntryFoupRecoveryPageCommand;
            }
        }

        private void EntryFoupRecoveryPageFunc()
        {
            try
            {
                PWindow.Close();
                PWindow = null;
                //fouprecovery view 호출되도록 한다.
                string text = null;

                ModuleStateEnum loaderstate = this.LoaderMaster.ModuleState.State;

                if (loaderstate == ModuleStateEnum.IDLE || loaderstate == ModuleStateEnum.PAUSED)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var viewmodel = this.ViewModelManager().GetViewModelFromGuid(new Guid("0DA29549-83DA-40A2-9A6A-1D058BDA3D88")); // viewmodel
                        if (viewmodel is GP_LoaderFoupRecoveryControlViewModel foupRecoveryViewModel)
                        {
                            // SelectedFoup 값을 변경합니다.
                            //foupRecoveryViewModel.InitViewModel();
                            foupRecoveryViewModel.SetSelectedFoup(FoupIndex);
                        }

                        this.ViewModelManager().ViewTransitionAsync(new Guid("F7BE0142-1ED7-4483-A257-AC512454F6F2")); // FoupRecoveryViewGuid
                    }));
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog($"Loader State {loaderstate}", "You can enter the page only when the loader status is IDLE or PASUED.", EnumMessageStyle.Affirmative, "OK");
                }
                //view 호출 시 , foup number을 넘긴다.
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ExitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (null == _ExitCommand) _ExitCommand = new RelayCommand(ExitCommandFunc);
                return _ExitCommand;
            }
        }

        private void ExitCommandFunc()
        {
            try
            {
                PWindow.Close();
                PWindow = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

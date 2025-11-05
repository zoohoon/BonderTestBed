namespace ProberViewModel.ViewModel.E84
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using RelayCommandBase;
    using System.Windows.Input;
    using ProberInterfaces;
    using ProberInterfaces.E84.ProberInterfaces;
    using ProberErrorCode;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using LoaderParameters;
    using LoaderBase;
    using Autofac;
    using ProberInterfaces.Foup;
    using System.Threading.Tasks;
    using MetroDialogInterfaces;
    using System.Threading;
    using System.Windows;
    using LogModule;
    using ProberViewModel.View.E84.Setting;
    using WinAPIWrapper;
    using ProberInterfaces.E84;

    public class E84ControlViewModel : IMainScreenViewModel, INotifyPropertyChanged, IFactoryModule
    {
        #region // ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region <remarks> Property </remarks>

        readonly Guid _ViewModelGUID = new Guid("79b81c60-6082-49de-9ae5-2e3719298307");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;
        private IE84Module e84Module { get; set; }

        private List<string> _FoupIndexs;
        public List<string> FoupIndexs
        {
            get { return _FoupIndexs; }
            set
            {
                if (value != _FoupIndexs)
                {
                    _FoupIndexs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<E84Info> _E84Infos = new ObservableCollection<E84Info>();
        public ObservableCollection<E84Info> E84Infos
        {
            get { return _E84Infos; }
            set
            {
                if (value != _E84Infos)
                {
                    _E84Infos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _E84InfosCount;
        public int E84InfosCount
        {
            get { return _E84InfosCount; }
            set
            {
                if (value != _E84InfosCount)
                {
                    _E84InfosCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region <remarks> Command </remarks>

        private AsyncCommand<object> _TabControlSelectionChanged;
        public ICommand TabControlSelectionChanged
        {
            get
            {
                if (null == _TabControlSelectionChanged) _TabControlSelectionChanged
                        = new AsyncCommand<object>(TabControlSelectionChangedFunc);
                return _TabControlSelectionChanged;
            }
        }

        private async Task TabControlSelectionChangedFunc(object obj)
        {
            try
            {
                //if (obj != null)
                //{
                //    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                //    {
                //        var tabControl = obj as TabControl;
                //        if (tabControl != null)
                //        {
                //            if (tabControl.SelectedItem != null)
                //            {
                //                var selectedHeader = (tabControl.SelectedItem as TabItem).Header;
                //                if (selectedHeader.Equals("SETTING"))
                //                {

                //                    string text = null;
                //                    text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                //                    String superPassword = AccountManager.MakeSuperAccountPassword();

                //                    if (text.ToLower().CompareTo(superPassword) == 0)
                //                    {

                //                    }
                //                    else
                //                    {
                //                        TabControlSelectedIndex = preTabControlSelectedIndex;
                //                    }
                //                }
                //            }
                //        }

                //    }));
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SettingClickCommand;
        public ICommand SettingClickCommand
        {
            get
            {
                if (null == _SettingClickCommand) _SettingClickCommand
                        = new RelayCommand<object>(SettingClickCommandFunc);
                return _SettingClickCommand;
            }
        }

        private void SettingClickCommandFunc(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                int foupindex = 0;
                
                E84OPModuleTypeEnum moduleytpe = E84OPModuleTypeEnum.UNDEFINED;

                if (paramArr[0] is int && paramArr[1] is E84OPModuleTypeEnum)
                {
                    foupindex = (int)paramArr[0];
                    moduleytpe = (E84OPModuleTypeEnum)paramArr[1];
                }

                E84ControlSettingView settingView = new E84ControlSettingView();
                
                settingView.DataContext = new E84ControlSettingViewModel(e84Module.GetE84Controller(foupindex, moduleytpe));
                string windowName = $"[LOAD PORT #{foupindex}] E84 Setting";
                bool bFindWindow = Win32APIWrapper.CheckWindowExists(windowName);
                
                if (!bFindWindow)
                {
                    Window container = new Window();
                    container.Content = settingView;
                    container.Width = 460;
                    container.Height = 800;
                    container.WindowStyle = WindowStyle.ToolWindow;
                    container.Title = windowName;
                    container.Topmost = true;
                    container.VerticalAlignment = VerticalAlignment.Center;
                    container.HorizontalAlignment = HorizontalAlignment.Center;
                    container.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    container.Show();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _OutPutCommand;
        public ICommand OutPutCommand
        {
            get
            {
                if (null == _OutPutCommand) _OutPutCommand = new RelayCommand<object>(OutPutCommandFunc);
                return _OutPutCommand;
            }
        }

        private void OutPutCommandFunc(object param)
        {
            try
            {
                object[] _param = param as object[];
                string boolean = _param[0].ToString();
                bool flag = false;
                int foupindex = (int)_param[1];
                E84OPModuleTypeEnum oPModuleTypeEnum = (E84OPModuleTypeEnum)_param[2];
                E84SignalTypeEnum signalTypeEnum = (E84SignalTypeEnum)_param[3];


                if (boolean.Equals(Boolean.TrueString))
                    flag = true;
                else if (boolean.Equals(Boolean.FalseString))
                    flag = false;

                e84Module.SetSignal(foupindex, oPModuleTypeEnum, signalTypeEnum, flag);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _AuxOutPutCommand;
        public ICommand AuxOutPutCommand
        {
            get
            {
                if (null == _AuxOutPutCommand) _AuxOutPutCommand = new RelayCommand<object>(AuxOutPutCommandFunc);
                return _AuxOutPutCommand;
            }
        }

        private void AuxOutPutCommandFunc(object param)
        {
            try
            {
                object[] _param = param as object[];
                string boolean = _param[0].ToString();
                bool flag = false;
                int foupindex = (int)_param[1];
                E84OPModuleTypeEnum moduleytpe = E84OPModuleTypeEnum.UNDEFINED;

                if (boolean.Equals(Boolean.TrueString))
                    flag = true;
                else if (boolean.Equals(Boolean.FalseString))
                    flag = false;

                moduleytpe = (E84OPModuleTypeEnum)_param[2];

                int outpu0 = flag ? 1 : 0;
                e84Module.GetE84Controller(foupindex, moduleytpe)?.CommModule.SetOutput0(outpu0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _FoupLockCommand;
        public ICommand FoupLockCommand
        {
            get
            {
                if (null == _FoupLockCommand) _FoupLockCommand = new AsyncCommand<object>(FoupLockCommandFunc);
                return _FoupLockCommand;
            }
        }

        private async Task FoupLockCommandFunc(object param)
        {
            try
            {
                int foupindex = (int)param;
                Task task = new Task(() =>
                {
                    try
                    {
                        ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();
                        var Cassette = loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupDockingPlateLockCommand());
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
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


        private AsyncCommand<object> _FoupUnLockCommand;
        public ICommand FoupUnLockCommand
        {
            get
            {
                if (null == _FoupUnLockCommand) _FoupUnLockCommand = new AsyncCommand<object>(FoupUnLockCommandFunc);
                return _FoupUnLockCommand;
            }
        }

        private async Task FoupUnLockCommandFunc(object param)
        {
            try
            {
                int foupindex = (int)param;
                Task task = new Task(() =>
                {
                    try
                    {
                        ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();
                        var Cassette = loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);

                        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupDockingPlateUnlockCommand());
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
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


        private AsyncCommand<object> _FoupLoadCommand;
        public ICommand FoupLoadCommand
        {
            get
            {
                if (null == _FoupLoadCommand) _FoupLoadCommand = new AsyncCommand<object>(FoupLoadCommandFunc);
                return _FoupLoadCommand;
            }
        }

        private async Task FoupLoadCommandFunc(object param)
        {
            try
            {
                int foupindex = (int)param;
                Task task = new Task(() =>
                {
                    try
                    {
                        ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();
                        var Cassette = loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
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

        private AsyncCommand<object> _FoupUnloadCommand;
        public ICommand FoupUnloadCommand
        {
            get
            {
                if (null == _FoupUnloadCommand) _FoupUnloadCommand = new AsyncCommand<object>(FoupUnloadCommandFunc);
                return _FoupUnloadCommand;
            }
        }

        private async Task FoupUnloadCommandFunc(object param)
        {
            try
            {
                int foupindex = (int)param;
                Task task = new Task(() =>
                {
                    try
                    {
                        ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();
                        var Cassette = loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
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

        private AsyncCommand<object> _FoupScanCommand;
        public ICommand FoupScanCommand
        {
            get
            {
                if (null == _FoupScanCommand) _FoupScanCommand = new AsyncCommand<object>(FoupScanCommandFunc);
                return _FoupScanCommand;
            }
        }

        private async Task FoupScanCommandFunc(object param)
        {
            try
            {
                int foupindex = (int)param;
                Task task = new Task(() =>
                {
                    try
                    {

                        ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();
                        var Cassette = loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        loaderModule.ScanCount = 25;

                        Cassette.SetNoReadScanState();
                        bool scanWaitFlag = false;

                        if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.NONE)
                        {
                            var retVal = loaderModule.DoScanJob(foupindex);
                            if (retVal.Result == EventCodeEnum.NONE)
                            {
                                scanWaitFlag = true;
                            }
                        }
                        while (scanWaitFlag)
                        {
                            if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL)
                            {
                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Error", "Illegal ScanState", EnumMessageStyle.Affirmative).Result;
                                break;
                            }
                            else if (Cassette.ScanState == CassetteScanStateEnum.READ)
                            {
                                break;
                            }
                            Thread.Sleep(10);
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
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

        private RelayCommand<object> _InitializeCommand;
        public ICommand InitializeCommand
        {
            get
            {
                if (null == _InitializeCommand) _InitializeCommand = new RelayCommand<object>(InitializeCommandFunc);
                return _InitializeCommand;
            }
        }

        private void InitializeCommandFunc(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                int foupindex = 0;
                E84OPModuleTypeEnum moduleytpe = E84OPModuleTypeEnum.UNDEFINED;
                if (paramArr[0] is int && paramArr[1] is E84OPModuleTypeEnum)
                {
                    foupindex = (int)paramArr[0];
                    moduleytpe = (E84OPModuleTypeEnum)paramArr[1];
                }

                IE84Controller e84Controller = e84Module.GetE84Controller(foupindex, moduleytpe);
                e84Controller.ClearState();
                e84Controller.ResetE84Interface();
                e84Controller.ClearState();
                e84Controller.ClearEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ClearSequenceCommand;
        public ICommand ClearSequenceCommand
        {
            get
            {
                if (null == _ClearSequenceCommand) _ClearSequenceCommand = new RelayCommand<object>(ClearSequenceCommandFunc);
                return _ClearSequenceCommand;
            }
        }

        private void ClearSequenceCommandFunc(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                int foupindex = 0;
                E84OPModuleTypeEnum moduleytpe = E84OPModuleTypeEnum.UNDEFINED;
                if (paramArr[0] is int && paramArr[1] is E84OPModuleTypeEnum)
                {
                    foupindex = (int)paramArr[0];
                    moduleytpe = (E84OPModuleTypeEnum)paramArr[1];
                }

                IE84Controller e84Controller = e84Module.GetE84Controller(foupindex, moduleytpe);
                if (e84Controller.GetModuleStateEnum() == ModuleStateEnum.ERROR)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error Message",
                        "E84 is in the error state so cannot be changed the mode.\r Please reset the status through the [Clear sequence] or [Initialize] menu and try again.", EnumMessageStyle.Affirmative);
                    return;
                }
                e84Controller.ClearState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ClarEventCommand;
        public ICommand ClarEventCommand
        {
            get
            {
                if (null == _ClarEventCommand) _ClarEventCommand = new RelayCommand<object>(ClarEventCommandFunc);
                return _ClarEventCommand;
            }
        }

        private void ClarEventCommandFunc(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                int foupindex = 0;
                E84OPModuleTypeEnum moduleytpe = E84OPModuleTypeEnum.UNDEFINED;
                if (paramArr[0] is int && paramArr[1] is E84OPModuleTypeEnum)
                {
                    foupindex = (int)paramArr[0];
                    moduleytpe = (E84OPModuleTypeEnum)paramArr[1];
                }

                IE84Controller e84Controller = e84Module.GetE84Controller(foupindex, moduleytpe);

                if (e84Controller.GetModuleStateEnum() == ModuleStateEnum.ERROR)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error Message",
                        "E84 is in the error state so cannot be changed the mode.\r Please reset the status through the [Clear sequence] or [Initialize] menu and try again.", EnumMessageStyle.Affirmative);
                    return;
                }

                e84Controller.ClearEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SetCarrierCardAttachStateCommand;
        public ICommand SetCarrierCardAttachStateCommand
        {
            get
            {
                if (null == _SetCarrierCardAttachStateCommand) _SetCarrierCardAttachStateCommand = new RelayCommand<object>(SetCarrierCardAttachStateCommandFunc);
                return _SetCarrierCardAttachStateCommand;
            }
        }
        private void SetCarrierCardAttachStateCommandFunc(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                int foupindex = 0;
                E84OPModuleTypeEnum moduleytpe = E84OPModuleTypeEnum.UNDEFINED;
                if (paramArr[0] is int && paramArr[1] is E84OPModuleTypeEnum)
                {
                    foupindex = (int)paramArr[0];
                    moduleytpe = (E84OPModuleTypeEnum)paramArr[1];
                }
                if (moduleytpe == E84OPModuleTypeEnum.FOUP)
                {
                    if (foupindex != 0)
                    {
                        var foupController = this.FoupOpModule().GetFoupController(foupindex);
                        this.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[foupindex - 1].Value = true;
                        this.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[foupindex - 1].Value = true;
                        this.GetFoupIO().IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2.Value = true;
                        foupController.FoupModuleInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_ATTACH;
                    }
                }
                else if (moduleytpe == E84OPModuleTypeEnum.CARD)
                {
                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    var cardbufmodule = LoaderMaster.Loader.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, foupindex);
                    if (cardbufmodule != null)
                    {
                        cardbufmodule.CardPRESENCEState = ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_ATTACH;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SetCarrierCardDetachStateCommand;
        public ICommand SetCarrierCardDetachStateCommand
        {
            get
            {
                if (null == _SetCarrierCardDetachStateCommand) _SetCarrierCardDetachStateCommand = new RelayCommand<object>(SetCarrierCardDetachStateCommandFunc);
                return _SetCarrierCardDetachStateCommand;
            }
        }
        private void SetCarrierCardDetachStateCommandFunc(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                int foupindex = 0;
                E84OPModuleTypeEnum moduleytpe = E84OPModuleTypeEnum.UNDEFINED;
                if (paramArr[0] is int && paramArr[1] is E84OPModuleTypeEnum)
                {
                    foupindex = (int)paramArr[0];
                    moduleytpe = (E84OPModuleTypeEnum)paramArr[1];
                }
                if (moduleytpe == E84OPModuleTypeEnum.FOUP)
                {
                    if (foupindex != 0)
                    {
                        var foupController = this.FoupOpModule().GetFoupController(foupindex);
                        this.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[foupindex - 1].Value = false;
                        this.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[foupindex - 1].Value = false;
                        this.GetFoupIO().IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2.Value = false;
                        foupController.FoupModuleInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_DETACH;
                    }
                }
                else if (moduleytpe == E84OPModuleTypeEnum.CARD)
                {
                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    var cardbufmodule = LoaderMaster.Loader.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, foupindex);
                    if (cardbufmodule != null)
                    {
                        cardbufmodule.CardPRESENCEState = ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_DETACH;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SetLoadStateCommand;
        public ICommand SetLoadStateCommand
        {
            get
            {
                if (null == _SetLoadStateCommand) _SetLoadStateCommand = new RelayCommand<object>(SetLoadStateCommandFunc);
                return _SetLoadStateCommand;
            }
        }
        private void SetLoadStateCommandFunc(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                int foupindex = 0;
                E84OPModuleTypeEnum moduleytpe = E84OPModuleTypeEnum.UNDEFINED;
                if (paramArr[0] is int && paramArr[1] is E84OPModuleTypeEnum)
                {
                    foupindex = (int)paramArr[0];
                    moduleytpe = (E84OPModuleTypeEnum)paramArr[1];
                }

                IE84Controller e84Controller = e84Module.GetE84Controller(foupindex, moduleytpe);

                if (moduleytpe == E84OPModuleTypeEnum.FOUP)
                {
                    e84Controller.SetCarrierState();
                    e84Controller.SetFoupBehaviorStateEnum();
                }
                else if (moduleytpe == E84OPModuleTypeEnum.CARD)
                {
                    e84Controller.SetCardStateInBuffer();
                    e84Controller.SetCardBehaviorStateEnum();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SetUnloadStateCommand;
        public ICommand SetUnloadStateCommand
        {
            get
            {
                if (null == _SetUnloadStateCommand) _SetUnloadStateCommand = new RelayCommand<object>(SetUnloadStateCommandFunc);
                return _SetUnloadStateCommand;
            }
        }
        private void SetUnloadStateCommandFunc(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                int foupindex = 0;
                E84OPModuleTypeEnum moduleytpe = E84OPModuleTypeEnum.UNDEFINED;
                if (paramArr[0] is int && paramArr[1] is E84OPModuleTypeEnum)
                {
                    foupindex = (int)paramArr[0];
                    moduleytpe = (E84OPModuleTypeEnum)paramArr[1];
                }

                IE84Controller e84Controller = e84Module.GetE84Controller(foupindex, moduleytpe);

                if (moduleytpe == E84OPModuleTypeEnum.FOUP)
                {
                    e84Controller.SetCarrierState();
                    e84Controller.SetFoupBehaviorStateEnum();
                }
                else if (moduleytpe == E84OPModuleTypeEnum.CARD)
                {
                    e84Controller.SetCardStateInBuffer();
                    e84Controller.SetCardBehaviorStateEnum();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
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

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                e84Module = this.E84Module();
                if (e84Module != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        E84Infos.Clear();
                        foreach (var foupinfo in e84Module.E84SysParam.E84Moduls)
                        {
                            //if(foupinfo.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                            //{
                            var controller = e84Module.GetE84Controller(foupinfo.FoupIndex, foupinfo.E84OPModuleType);
                            E84Infos.Add(new E84Info(foupinfo.FoupIndex, controller, foupinfo.E84OPModuleType));
                            //}
                        }
                        E84InfosCount = e84Module.E84SysParam.E84Moduls.Count;
                    });
                }
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion
    }
}
